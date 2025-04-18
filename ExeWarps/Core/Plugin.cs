﻿using System;
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
using AdvancedWarps.Models;
using AdvancedWarps.Commands;
using AdvancedWarps.Harmony;
using AdvancedWarps.Utilities;
using static Rocket.Unturned.Events.UnturnedPlayerEvents;
using System.ComponentModel;

namespace AdvancedWarps.Core
{
    public class Plugin : RocketPlugin<Configuration>
    {
        public static Plugin Instance;
        public List<CSteamID> Warping;
        internal Dictionary<CSteamID, DateTime> _warpProtect;
        private Dictionary<CSteamID, DateTime> _lastProtectMessage;

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
                translationList.Add("warp_cancel_melee", "Teleportation canceled due to melee attack. Color=red");
                translationList.Add("warp_cancel_throwable", "Teleportation canceled due to throwing. Color=red");
                translationList.Add("warp_cancel_punch", "Teleportation canceled due to punch. Color=red");
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
                translationList.Add("warp_near_loc_null", "Warp location not found. Color=red");
                translationList.Add("warp_protect_active", "Player is under warp protection for [{0}] more seconds and cannot be damaged. Color=red");
                translationList.Add("warp_protect_expired", "Your warp protection has expired. Color=yellow");
                translationList.Add("warp_protect_removed_by_shooting", "Warp protection removed due to attack. Color=red");
                return translationList;
            }
        }

        protected override void Load()
        {
            Instance = this;
            Warping = new List<CSteamID>();
            _warpProtect = new Dictionary<CSteamID, DateTime>();
            _lastProtectMessage = new Dictionary<CSteamID, DateTime>();
            if (Configuration.Instance.DownloadWorkshop)
            {
                var workshopConfig = WorkshopDownloadConfig.getOrLoad();
                if (!workshopConfig.File_IDs.Contains(3456118035))
                {
                    workshopConfig.File_IDs.Add(3456118035);
                }
            }
            var harmony = new HarmonyLib.Harmony("com.warps.exe");
            harmony.PatchAll();
            U.Events.OnPlayerDisconnected += EventsOnPlayerDisconnected;
            UnturnedPlayerEvents.OnPlayerDeath += OnPlayerDeath;
            UnturnedPlayerEvents.OnPlayerUpdateGesture += OnPlayerUpdateGesture;
            BarricadeManager.onDeployBarricadeRequested = (DeployBarricadeRequestHandler)Delegate.Combine(BarricadeManager.onDeployBarricadeRequested, new DeployBarricadeRequestHandler(OnDeployBarricadeRequested));
            StructureManager.onDeployStructureRequested = (DeployStructureRequestHandler)Delegate.Combine(StructureManager.onDeployStructureRequested, new DeployStructureRequestHandler(OnDeployStructureRequested));
            DamageTool.damagePlayerRequested += DamageToolOnDamagePlayerRequested;
            EffectManager.onEffectButtonClicked += OnEffectButtonClicked;
            UseableThrowable.onThrowableSpawned += OnThrowableSpawned;
        }

        protected override void Unload()
        {
            U.Events.OnPlayerDisconnected -= EventsOnPlayerDisconnected;
            UnturnedPlayerEvents.OnPlayerDeath -= OnPlayerDeath;
            UnturnedPlayerEvents.OnPlayerUpdateGesture -= OnPlayerUpdateGesture;
            UseableThrowable.onThrowableSpawned -= OnThrowableSpawned;
            BarricadeManager.onDeployBarricadeRequested = (DeployBarricadeRequestHandler)Delegate.Remove(BarricadeManager.onDeployBarricadeRequested, new DeployBarricadeRequestHandler(OnDeployBarricadeRequested));
            StructureManager.onDeployStructureRequested = (DeployStructureRequestHandler)Delegate.Remove(StructureManager.onDeployStructureRequested, new DeployStructureRequestHandler(OnDeployStructureRequested));
            DamageTool.damagePlayerRequested -= DamageToolOnDamagePlayerRequested;
            EffectManager.onEffectButtonClicked -= OnEffectButtonClicked;
            var harmony = new HarmonyLib.Harmony("com.warps.exe");
            harmony.UnpatchAll("com.warps.exe");
        }

        private void OnPlayerDeath(UnturnedPlayer player, EDeathCause cause, ELimb limb, CSteamID murderer)
        {
            var component = player.GetComponent<PlayerComponent>();
            if (component != null && component.IsTeleporting)
            {
                component.CancelTeleport("warp_cancel_death");
            }
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

                        // Используем UIEffectID из конфига с приведением к ushort
                        EffectManager.askEffectClearByID((ushort)Plugin.Instance.Configuration.Instance.UIEffectID, unturnedPlayer.Player.channel.owner.transportConnection);
                        unturnedPlayer.Player.setPluginWidgetFlag(EPluginWidgetFlags.Modal, false);
                    }
                }
            }
            else if (buttonName == "Close_warp")
            {
                // Используем UIEffectID из конфига с приведением к ushort
                EffectManager.askEffectClearByID((ushort)Plugin.Instance.Configuration.Instance.UIEffectID, unturnedPlayer.Player.channel.owner.transportConnection);
                unturnedPlayer.Player.setPluginWidgetFlag(EPluginWidgetFlags.Modal, false);
            }
        }

        private void FixedUpdate()
        {
            // Проверяем истечение времени WarpProtect
            List<CSteamID> toRemove = new List<CSteamID>();
            foreach (var pair in _warpProtect)
            {
                if (pair.Value <= DateTime.Now)
                {
                    toRemove.Add(pair.Key);
                }
            }

            foreach (var playerId in toRemove)
            {
                RemoveWarpProtect(playerId, false); // false, чтобы отправить warp_protect_expired
            }
        }

        private void OnThrowableSpawned(UseableThrowable throwable, GameObject projectile)
        {
            if (throwable == null || throwable.player == null)
                return;

            var player = UnturnedPlayer.FromPlayer(throwable.player);
            if (player == null)
                return;

            var component = player.GetComponent<PlayerComponent>();
            if (component == null)
                return;

            // Снимаем защиту, если она активна
            if (_warpProtect.ContainsKey(player.CSteamID))
            {
                RemoveWarpProtect(player.CSteamID, true);
            }

            // Отменяем телепорт при включённой опции
            if (component.IsTeleporting && Configuration.Instance.CancelOnShooting)
            {
                component.CancelTeleport("warp_cancel_throwable");
            }
        }

        private void OnPlayerUpdateGesture(UnturnedPlayer player, PlayerGesture gesture)
        {
            if (gesture != PlayerGesture.PunchLeft && gesture != PlayerGesture.PunchRight) return;
            if (Plugin.Instance == null) return;

            if (_warpProtect.ContainsKey(player.CSteamID))
            {
                RemoveWarpProtect(player.CSteamID, true); // true, чтобы отправить warp_protect_removed_by_shooting
            }

            PlayerComponent component = player.GetComponent<PlayerComponent>();
            if (component != null && component.IsTeleporting && Configuration.Instance.CancelOnShooting)
            {
                component.CancelTeleport("warp_cancel_punch");
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
                if (!_lastProtectMessage.ContainsKey(steamID) || (DateTime.Now - _lastProtectMessage[steamID]).TotalSeconds >= 1)
                {
                    UnturnedPlayer attacker = UnturnedPlayer.FromCSteamID(parameters.killer);
                    if (attacker != null)
                    {
                        double remainingSeconds = (_warpProtect[steamID] - DateTime.Now).TotalSeconds;
                        int roundedSeconds = (int)Math.Round(remainingSeconds);
                        new Transelation("warp_protect_active", new object[] { roundedSeconds }).execute(attacker);
                    }
                    _lastProtectMessage[steamID] = DateTime.Now;
                }
            }
        }

        //private void OnTriggerSend(SteamPlayer player, string s, ESteamCall mode, ESteamPacket type, object[] arguments)
        //{
        //    if (s != "tellEquip" || !_warpProtect.ContainsKey(player.playerID.steamID) || _warpProtect[player.playerID.steamID] < DateTime.Now)
        //    {
        //        return;
        //    }

        //    byte b = (byte)arguments[3];
        //    if (Assets.find(EAssetType.ITEM, (ushort)b) is ItemGunAsset)
        //    {
        //        _warpProtect.Remove(player.playerID.steamID);
        //    }
        //}
        //private void OnUseableUsed(PlayerEquipment equipment, Useable useable)
        //{
        //    var player = UnturnedPlayer.FromPlayer(equipment.player);
        //    if (player == null) return;

        //    // Проверяем, является ли используемый предмет оружием
        //    if (equipment.asset is ItemWeaponAsset)
        //    {
        //        if (_warpProtect.ContainsKey(player.CSteamID))
        //        {
        //            _warpProtect.Remove(player.CSteamID);
        //        }
        //    }
        //}

        public void RemoveWarpProtect(CSteamID playerId, bool byPlayerAction = false)
        {
            if (_warpProtect.ContainsKey(playerId))
            {
                var player = UnturnedPlayer.FromCSteamID(playerId);
                if (player != null)
                {
                    string key = byPlayerAction ? "warp_protect_removed_by_shooting" : "warp_protect_expired";
                    new Transelation(key).execute(player);
                }
                _warpProtect.Remove(playerId);
                _lastProtectMessage.Remove(playerId);
            }
        }


        public void AfterWarp(UnturnedPlayer player)
        {
            Warping.Remove(player.CSteamID);
            _warpProtect[player.CSteamID] = DateTime.Now.AddSeconds(Configuration.Instance.WarpProtect);
        }
    }
}