using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System.Collections.Generic;

namespace AdvancedWarps
{
    public class CommandWarps : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "warps";
        public string Help => "Opens the warp selection UI.";
        public string Syntax => "";
        public List<string> Aliases => new List<string>();
        public List<string> Permissions => new List<string> { "warps" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer player = (UnturnedPlayer)caller;

            // Включаем Modal флаг для размытия и свободы движения мыши
            player.Player.enablePluginWidgetFlag(EPluginWidgetFlags.Modal);

            // Открываем UI с эффектом ID 45882
            EffectManager.sendUIEffect(45882, short.MaxValue, player.Player.channel.owner.transportConnection, true);

            // Обновляем UI с названиями варпов (до 10 варпов)
            for (int i = 0; i < 10; i++)
            {
                string warpName = i < Plugin.Instance.Configuration.Instance.Warps.Count && Plugin.Instance.Configuration.Instance.Warps[i].IsActive
                    ? Plugin.Instance.Configuration.Instance.Warps[i].Name
                    : "";
                EffectManager.sendUIEffectText(short.MaxValue, player.Player.channel.owner.transportConnection, true, $"Warp_loc_text_{i + 1}", warpName);
            }
        }
    }
}