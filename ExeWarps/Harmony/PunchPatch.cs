using HarmonyLib;
using Rocket.Unturned.Player;
using SDG.Unturned;
using AdvancedWarps.Core;
using AdvancedWarps.Models;
using AdvancedWarps.Commands;
using AdvancedWarps.Utilities;

namespace AdvancedWarps.Harmony
{
    [HarmonyPatch(typeof(PlayerStance), "punch")]
    public class PunchPatch
    {
        static void Prefix(PlayerStance __instance, EPlayerPunch punch)
        {
            if (__instance.player == null) return;

            var player = __instance.player;
            var unturnedPlayer = UnturnedPlayer.FromPlayer(player);

            // Снятие WarpProtect при ударе кулаком
            Plugin.Instance.RemoveWarpProtect(unturnedPlayer.CSteamID);
        }
    }
}