using HarmonyLib;
using Rocket.Unturned.Player;
using SDG.Unturned;
using AdvancedWarps.Core;
using AdvancedWarps.Models;
using AdvancedWarps.Commands;
using AdvancedWarps.Utilities;

namespace AdvancedWarps.Harmony
{
    [HarmonyPatch(typeof(UseableGun), "fire")]
    public class FirePatch
    {
        static void Prefix(UseableGun __instance)
        {
            if (__instance.player == null) return;

            var player = __instance.player;
            var unturnedPlayer = UnturnedPlayer.FromPlayer(player);
            var component = unturnedPlayer.GetComponent<PlayerComponent>();

            // Отмена телепортации при выстреле
            if (component != null && component.IsTeleporting && Plugin.Instance.Configuration.Instance.CancelOnShooting)
            {
                component.CancelTeleport("warp_cancel_shooting");
            }

            // Снятие WarpProtect при выстреле
            Plugin.Instance.RemoveWarpProtect(unturnedPlayer.CSteamID);
        }
    }
}