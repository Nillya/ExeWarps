using System;
using System.Collections.Generic;
using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using UnityEngine;

namespace AdvancedWarps
{
    public class Plugin : RocketPlugin<Configuration>
    {
        public static Plugin Instance;
        public List<CSteamID> Warping;
        private Dictionary<CSteamID, DateTime> _warpProtect;

        public override TranslationList DefaultTranslations
        {
            get
            {
                TranslationList translationList = new TranslationList();
                translationList.Add("warp_null", "Warp with this name not found. Color=red");
                translationList.Add("warp_teleport_ok", "You will be teleported to warp: [{0}] in: [{1}sec]. Color=yellow");
                translationList.Add("warp_successfully_teleported", "You have been successfully teleported to the warp. Color=yellow");
                translationList.Add("warp_create_ok", "You have successfully created a warp named: [{0}]. Color=yellow");
                translationList.Add("warp_delete_ok", "You have successfully deleted the warp named: [{0}]. Color=yellow");
                translationList.Add("warp_replace_ok", "Warps [{0}] and [{1}] have been successfully swapped. Color=yellow");
                translationList.Add("warp_add_subwarp_ok", "Sub-warp added to warp [{0}] with ID: [{1}]. Color=yellow");
                translationList.Add("warp_cancel_damage", "Teleportation canceled due to damage. Color=red");
                translationList.Add("warp_cancel_movement", "Teleportation canceled due to movement. Color=red");
                translationList.Add("warp_cancel_shooting", "Teleportation canceled due to shooting. Color=red");
                translationList.Add("warp_cancel_disconnect", "Teleportation canceled due to disconnection. Color=red");
                translationList.Add("warp_cancel_death", "Teleportation canceled due to death. Color=red");
                translationList.Add("already_delay", "You are already waiting to be warped. Color=red");
                return translationList;
            }
        }

        protected override void Load()
        {
            Instance = this;
            Warping = new List<CSteamID>();
            _warpProtect = new Dictionary<CSteamID, DateTime>();

            // Subscribe to events
            U.Events.OnPlayerDisconnected += EventsOnPlayerDisconnected;
            BarricadeManager.onDeployBarricadeRequested = (DeployBarricadeRequestHandler)Delegate.Combine(BarricadeManager.onDeployBarricadeRequested, new DeployBarricadeRequestHandler(OnDeployBarricadeRequested));
            StructureManager.onDeployStructureRequested = (DeployStructureRequestHandler)Delegate.Combine(StructureManager.onDeployStructureRequested, new DeployStructureRequestHandler(OnDeployStructureRequested));
            DamageTool.damagePlayerRequested += DamageToolOnDamagePlayerRequested;
            SteamChannel.onTriggerSend = (TriggerSend)Delegate.Combine(SteamChannel.onTriggerSend, new TriggerSend(OnTriggerSend));
            EffectManager.onEffectButtonClicked += OnEffectButtonClicked;
        }

        protected override void Unload()
        {
            // Unsubscribe from events
            U.Events.OnPlayerDisconnected -= EventsOnPlayerDisconnected;
            BarricadeManager.onDeployBarricadeRequested = (DeployBarricadeRequestHandler)Delegate.Remove(BarricadeManager.onDeployBarricadeRequested, new DeployBarricadeRequestHandler(OnDeployBarricadeRequested));
            StructureManager.onDeployStructureRequested = (DeployStructureRequestHandler)Delegate.Remove(StructureManager.onDeployStructureRequested, new DeployStructureRequestHandler(OnDeployStructureRequested));
            DamageTool.damagePlayerRequested -= DamageToolOnDamagePlayerRequested;
            SteamChannel.onTriggerSend = (TriggerSend)Delegate.Remove(SteamChannel.onTriggerSend, new TriggerSend(OnTriggerSend));
            EffectManager.onEffectButtonClicked -= OnEffectButtonClicked;
        }

        // В Plugin.cs, метод OnEffectButtonClicked
        private void OnEffectButtonClicked(Player player, string buttonName)
        {
            UnturnedPlayer unturnedPlayer = UnturnedPlayer.FromPlayer(player);
            PlayerComponent component = unturnedPlayer.GetComponent<PlayerComponent>();

            // Handle warp button clicks
            if (buttonName.StartsWith("Warp_loc_"))
            {
                int warpIndex;
                if (int.TryParse(buttonName.Replace("Warp_loc_", ""), out warpIndex))
                {
                    warpIndex--; // Adjust for 0-based indexing
                    if (warpIndex >= 0 && warpIndex < Plugin.Instance.Configuration.Instance.Warps.Count)
                    {
                        Warp warp = Plugin.Instance.Configuration.Instance.Warps[warpIndex];
                        if (warp.SubWarps.Count == 0)
                        {
                            new Transelation("warp_null", Array.Empty<object>()).execute(unturnedPlayer);
                            return;
                        }

                        if (Plugin.Instance.Warping.Contains(unturnedPlayer.CSteamID))
                        {
                            new Transelation("already_delay", Array.Empty<object>()).execute(unturnedPlayer);
                            return;
                        }

                        component.CurrentWarp = warp;
                        component.TimeTeleportWarp = DateTime.Now;
                        component.InitialPosition = new SerializableVector3(unturnedPlayer.Position.x, unturnedPlayer.Position.y, unturnedPlayer.Position.z);
                        component.IsTeleporting = true;

                        Plugin.Instance.Warping.Add(unturnedPlayer.CSteamID);
                        new Transelation("warp_teleport_ok", new object[] { warp.Name, Plugin.Instance.Configuration.Instance.DelayTeleportToWarp }).execute(unturnedPlayer);

                        // Закрываем UI и убираем blur при выборе варпа
                        EffectManager.askEffectClearByID(45882, unturnedPlayer.Player.channel.owner.transportConnection);
                        unturnedPlayer.Player.setPluginWidgetFlag(EPluginWidgetFlags.Modal, false);
                    }
                }
            }
            else if (buttonName == "Close_warp")
            {
                // Закрываем UI и убираем blur при нажатии кнопки Close
                EffectManager.askEffectClearByID(45882, unturnedPlayer.Player.channel.owner.transportConnection);
                unturnedPlayer.Player.setPluginWidgetFlag(EPluginWidgetFlags.Modal, false);
            }
        }

        private void EventsOnPlayerDisconnected(UnturnedPlayer player)
        {
            if (Warping.Contains(player.CSteamID))
            {
                Warping.Remove(player.CSteamID);
                var component = player.GetComponent<PlayerComponent>();
                if (component != null && component.IsTeleporting)
                {
                    component.CancelTeleport("warp_cancel_disconnect");
                }
            }
        }

        private void OnDeployBarricadeRequested(Barricade barricade, ItemBarricadeAsset asset, Transform hit, ref Vector3 point, ref float angle_x, ref float angle_y, ref float angle_z, ref ulong owner, ref ulong group, ref bool shouldAllow)
        {
            if (shouldAllow)
            {
                foreach (var warp in Configuration.Instance.Warps)
                {
                    foreach (var subWarp in warp.SubWarps)
                    {
                        if (Vector3.Distance((Vector3)subWarp.Position, point) <= Configuration.Instance.NoBuildRadius)
                        {
                            shouldAllow = false;
                            return;
                        }
                    }
                }
            }
        }

        private void OnDeployStructureRequested(Structure structure, ItemStructureAsset asset, ref Vector3 point, ref float angle_x, ref float angle_y, ref float angle_z, ref ulong owner, ref ulong group, ref bool shouldAllow)
        {
            if (shouldAllow)
            {
                foreach (var warp in Configuration.Instance.Warps)
                {
                    foreach (var subWarp in warp.SubWarps)
                    {
                        if (Vector3.Distance((Vector3)subWarp.Position, point) <= Configuration.Instance.NoBuildRadius)
                        {
                            shouldAllow = false;
                            return;
                        }
                    }
                }
            }
        }

        private void DamageToolOnDamagePlayerRequested(ref DamagePlayerParameters parameters, ref bool shouldAllow)
        {
            CSteamID steamID = parameters.player.channel.owner.playerID.steamID;
            var player = UnturnedPlayer.FromCSteamID(steamID);
            var component = player.GetComponent<PlayerComponent>();

            // Cancel teleport if damaged during teleport
            if (component != null && component.IsTeleporting && Configuration.Instance.CancelOnDamage)
            {
                component.CancelTeleport("warp_cancel_damage");
            }

            // Apply WarpProtect (immunity after teleport)
            if (shouldAllow && _warpProtect.ContainsKey(steamID) && _warpProtect[steamID] > DateTime.Now)
            {
                shouldAllow = false;
            }

            // Cancel teleport if the player dies
            if (shouldAllow && component != null && component.IsTeleporting && parameters.player.life.health - parameters.damage <= 0)
            {
                component.CancelTeleport("warp_cancel_death");
            }
        }

        private void OnTriggerSend(SteamPlayer player, string s, ESteamCall mode, ESteamPacket type, object[] arguments)
        {
            if (s != "tellEquip" || !_warpProtect.ContainsKey(player.playerID.steamID) || _warpProtect[player.playerID.steamID] < DateTime.Now)
            {
                return;
            }

            byte b = (byte)arguments[3];
            if (Assets.find(EAssetType.ITEM, (ushort)b) is ItemGunAsset)
            {
                _warpProtect.Remove(player.playerID.steamID);

                // Cancel teleport if shooting during teleport
                var unturnedPlayer = UnturnedPlayer.FromSteamPlayer(player);
                var component = unturnedPlayer.GetComponent<PlayerComponent>();
                if (component != null && component.IsTeleporting && Configuration.Instance.CancelOnShooting)
                {
                    component.CancelTeleport("warp_cancel_shooting");
                }
            }
        }

        public void AfterWarp(UnturnedPlayer player)
        {
            Warping.Remove(player.CSteamID);
            if (player.Player.equipment.isEquipped && player.Player.equipment.asset is ItemGunAsset)
            {
                return;
            }
            _warpProtect[player.CSteamID] = DateTime.Now.AddSeconds(Configuration.Instance.WarpProtect);
        }
    }
}