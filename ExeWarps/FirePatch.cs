using HarmonyLib;
using Rocket.Unturned.Player;
using SDG.Unturned;

namespace AdvancedWarps
{
    [HarmonyPatch(typeof(UseableGun), "fire")]
    public class FirePatch
    {
        static void Prefix(UseableGun __instance)
        {
            if (__instance.player == null) return;

            var player = __instance.player;
            var unturnedPlayer = UnturnedPlayer.FromSteamPlayer(player);
            var component = unturnedPlayer.GetComponent<PlayerComponent>();

            if (component != null && component.IsTeleporting && Plugin.Instance.Configuration.Instance.CancelOnShooting)
            {
                component.CancelTeleport("warp_cancel_shooting");
            }
        }
    }
}