using HarmonyLib;
using Rocket.Unturned.Player;
using SDG.Unturned;
using AdvancedWarps.Core;
using AdvancedWarps.Models;
using AdvancedWarps.Commands;
using AdvancedWarps.Utilities;

namespace AdvancedWarps.Harmony
{
    [HarmonyPatch(typeof(UseableThrowable), "fire")]
    public class ThrowablePatch
    {
        static void Prefix(UseableThrowable __instance)
        {
            if (__instance.player == null) return;

            var player = __instance.player;
            var unturnedPlayer = UnturnedPlayer.FromPlayer(player);

            // Снятие WarpProtect при броске (гранаты, ракеты и т.д.)
            Plugin.Instance.RemoveWarpProtect(unturnedPlayer.CSteamID);
        }
    }
}