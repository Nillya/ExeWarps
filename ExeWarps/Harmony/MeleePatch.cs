﻿using HarmonyLib;
using Rocket.Unturned.Player;
using SDG.Unturned;
using AdvancedWarps.Core;
using AdvancedWarps.Utilities;

namespace AdvancedWarps.Harmony
{
    [HarmonyPatch(typeof(UseableMelee), "fire")]
    public class MeleePatch
    {
        static void Prefix(UseableMelee __instance)
        {
            if (__instance.player == null) return;

            var player = UnturnedPlayer.FromPlayer(__instance.player);
            if (player == null) return;

            var component = player.GetComponent<PlayerComponent>();

            // Снимаем защиту, если она активна
            if (Plugin.Instance._warpProtect.ContainsKey(player.CSteamID))
            {
                Plugin.Instance.RemoveWarpProtect(player.CSteamID, true);
            }

            // Отменяем телепорт при включённой настройке
            if (component != null && component.IsTeleporting && Plugin.Instance.Configuration.Instance.CancelOnShooting)
            {
                component.CancelTeleport("warp_cancel_melee");
            }
        }
    }
}