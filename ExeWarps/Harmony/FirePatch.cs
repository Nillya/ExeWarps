using HarmonyLib;
using Rocket.Unturned.Player;
using SDG.Unturned;
using AdvancedWarps.Core;
using AdvancedWarps.Utilities;
using System;

namespace AdvancedWarps.Harmony
{
    [HarmonyPatch(typeof(PlayerEquipment), "use")]
    public class EquipmentUsePatch
    {
        static void Prefix(PlayerEquipment __instance)
        {
            // Проверяем наличие игрока и используемого предмета
            if (__instance?.player == null || __instance.useable == null)
                return;

            // Получаем Rocket-объект игрока
            UnturnedPlayer player = UnturnedPlayer.FromPlayer(__instance.player);
            if (player == null)
                return;

            // Получаем компонент игрока
            PlayerComponent component = player.GetComponent<PlayerComponent>();
            if (component == null)
                return;

            // Определяем тип используемого предмета и соответствующий ключ перевода
            string translationKey = null;
            if (__instance.useable is UseableGun)
            {
                translationKey = "warp_cancel_shooting";
            }
            else if (__instance.useable is UseableThrowable)
            {
                translationKey = "warp_cancel_throwable";
            }
            else if (__instance.useable is UseableMelee)
            {
                translationKey = "warp_cancel_melee";
            }

            // Если предмет подпадает под одну из категорий
            if (translationKey != null)
            {
                // Снимаем защиту от варпа
                Plugin.Instance.RemoveWarpProtect(player.CSteamID);

                // Отменяем телепортацию, если она активна и включена опция CancelOnShooting
                if (component.IsTeleporting && Plugin.Instance.Configuration.Instance.CancelOnShooting)
                {
                    new Transelation(translationKey, Array.Empty<object>()).execute(player);
                    component.CancelTeleport(null); // Сообщение уже отправлено через Transelation
                }
            }
        }
    }
}