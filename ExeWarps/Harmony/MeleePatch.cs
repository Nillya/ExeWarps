using HarmonyLib;
using Rocket.Unturned.Player;
using SDG.Unturned;
using AdvancedWarps.Core;
using AdvancedWarps.Models;
using AdvancedWarps.Commands;
using AdvancedWarps.Utilities;

namespace AdvancedWarps.Harmony
{
    [HarmonyPatch(typeof(UseableMelee), "fire")]
    public class MeleePatch
    {
        static void Prefix(UseableMelee __instance)
        {
            if (__instance.player == null) return;

            var player = __instance.player;
            var unturnedPlayer = UnturnedPlayer.FromPlayer(player);

            // Снятие WarpProtect при ударе холодным оружием
            Plugin.Instance.RemoveWarpProtect(unturnedPlayer.CSteamID);
        }
    }
}