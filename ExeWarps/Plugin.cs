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
using HarmonyLib;

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
                translationList.Add("build_restricted", "Building is prohibited near warps! Color=red");
                translationList.Add("invalid_subwarp_id", "Invalid sub-warp ID! Color=red");
                translationList.Add("subwarp_not_found", "Sub-warp with ID [{0}] not found! Color=red");
                translationList.Add("subwarp_delete_ok", "Sub-warp with ID [{1}] removed from warp [{0}]. Color=yellow");
                translationList.Add("warp_same_swap", "Cannot swap a warp with itself! Color=red");
                translationList.Add("invalid_position", "Invalid position specified! Color=red");
                translationList.Add("admin_warp_create_ok", "Admin warp [{0}] created successfully. Color=yellow");
                translationList.Add("admin_warp_delete_ok", "Admin warp [{0}] deleted successfully. Color=yellow");
                translationList.Add("no_admin_access", "Only admins can teleport to this warp! Color=red");
                translationList.Add("warp_exists", "Warp [{0}] already exists! Color=red");
                translationList.Add("warp_list_header", "Available warps: Color=yellow");
                translationList.Add("warp_list_empty", "No warps available. Color=red");
                translationList.Add("admin_warp_list_header", "Admin warps: Color=cyan");
                translationList.Add("warp_already_at_location", "A warp already exists at location [{0}]! Color=red");
                return translationList;
            }
        }

        protected override void Load()
        {
            Instance = this;
            Warping = new List<CSteamID>();
            _warpProtect = new Dictionary<CSteamID, DateTime>();
            var harmony = new Harmony("com.warps.exe");
            harmony.PatchAll();
            U.Events.OnPlayerDisconnected += EventsOnPlayerDisconnected;
            BarricadeManager.onDeployBarricadeRequested = (DeployBarricadeRequestHandler)Delegate.Combine(BarricadeManager.onDeployBarricadeRequested, new DeployBarricadeRequestHandler(OnDeployBarricadeRequested));
            StructureManager.onDeployStructureRequested = (DeployStructureRequestHandler)Delegate.Combine(StructureManager.onDeployStructureRequested, new DeployStructureRequestHandler(OnDeployStructureRequested));
            DamageTool.damagePlayerRequested += DamageToolOnDamagePlayerRequested;
            SteamChannel.onTriggerSend = (TriggerSend)Delegate.Combine(SteamChannel.onTriggerSend, new TriggerSend(OnTriggerSend));
            EffectManager.onEffectButtonClicked += OnEffectButtonClicked;
        }

        protected override void Unload()
        {
            U.Events.OnPlayerDisconnected -= EventsOnPlayerDisconnected;
            BarricadeManager.onDeployBarricadeRequested = (DeployBarricadeRequestHandler)Delegate.Remove(BarricadeManager.onDeployBarricadeRequested, new DeployBarricadeRequestHandler(OnDeployBarricadeRequested));
            StructureManager.onDeployStructureRequested = (DeployStructureRequestHandler)Delegate.Remove(StructureManager.onDeployStructureRequested, new DeployStructureRequestHandler(OnDeployStructureRequested));
            DamageTool.damagePlayerRequested -= DamageToolOnDamagePlayerRequested;
            SteamChannel.onTriggerSend = (TriggerSend)Delegate.Remove(SteamChannel.onTriggerSend, new TriggerSend(OnTriggerSend));
            EffectManager.onEffectButtonClicked -= OnEffectButtonClicked;
            var harmony = new Harmony("com.warps.exe");
            harmony.UnpatchAll("com.warps.exe");
        }

        public static readonly List<KnownLocation> KnownLocations = new List<KnownLocation>
        {
            new KnownLocation("Summerside Military Base", new SerializableVector3(0, 0, 0), 1),
            new KnownLocation("Stratford", new SerializableVector3(0, 0, 0), 2),
            new KnownLocation("Alberton", new SerializableVector3(0, 0, 0), 3),
            new KnownLocation("Belfast Airport", new SerializableVector3(0, 0, 0), 4),
            new KnownLocation("Cape Rock", new SerializableVector3(0, 0, 0), 5),
            new KnownLocation("Charlottetown", new SerializableVector3(0, 0, 0), 6),
            new KnownLocation("Confederation Bridge", new SerializableVector3(0, 0, 0), 7),
            new KnownLocation("Courtin Isle", new SerializableVector3(0, 0, 0), 8),
            new KnownLocation("Fernwood Farm", new SerializableVector3(0, 0, 0), 9),
            new KnownLocation("Holman Isle", new SerializableVector3(0, 0, 0), 10)
        };

        private void OnEffectButtonClicked(Player player, string buttonName)
        {
            UnturnedPlayer unturnedPlayer = UnturnedPlayer.FromPlayer(player);
            PlayerComponent component = unturnedPlayer.GetComponent<PlayerComponent>();

            if (buttonName.StartsWith("Warp_loc_"))
            {
                int warpId;
                if (int.TryParse(buttonName.Replace("Warp_loc_", ""), out warpId))
                {
                    // Find warp by WarpId instead of index
                    Warp warp = Plugin.Instance.Configuration.Instance.Warps.Find(w => w.WarpId == warpId && w.IsActive);
                    if (warp != null)
                    {
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

                        EffectManager.askEffectClearByID(45882, unturnedPlayer.Player.channel.owner.transportConnection);
                        unturnedPlayer.Player.setPluginWidgetFlag(EPluginWidgetFlags.Modal, false);
                    }
                }
            }
            else if (buttonName == "Close_warp")
            {
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
                            UnturnedPlayer player = UnturnedPlayer.FromCSteamID(new CSteamID(owner));
                            if (player != null)
                            {
                                new Transelation("build_restricted", Array.Empty<object>()).execute(player);
                            }
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
                            UnturnedPlayer player = UnturnedPlayer.FromCSteamID(new CSteamID(owner));
                            if (player != null)
                            {
                                new Transelation("build_restricted", Array.Empty<object>()).execute(player);
                            }
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

            if (component != null && component.IsTeleporting && Configuration.Instance.CancelOnDamage)
            {
                component.CancelTeleport("warp_cancel_damage");
            }

            if (shouldAllow && _warpProtect.ContainsKey(steamID) && _warpProtect[steamID] > DateTime.Now)
            {
                shouldAllow = false;
            }

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