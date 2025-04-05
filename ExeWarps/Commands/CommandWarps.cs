using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System.Collections.Generic;
using System.Linq;
using AdvancedWarps.Core;
using AdvancedWarps.Models;

namespace AdvancedWarps.Commands
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

            // Enable Modal flag for blur and mouse freedom
            player.Player.enablePluginWidgetFlag(EPluginWidgetFlags.Modal);

            // Open UI with effect ID from configuration
            EffectManager.sendUIEffect((ushort)Plugin.Instance.Configuration.Instance.UIEffectID, short.MaxValue, player.Player.channel.owner.transportConnection, true);

            // Clear all UI slots first up to MaxWarpsInUI
            int maxSlots = Plugin.Instance.Configuration.Instance.MaxWarpsInUI;
            for (int i = 1; i <= maxSlots; i++)
            {
                EffectManager.sendUIEffectText(short.MaxValue, player.Player.channel.owner.transportConnection, true, $"Warp_loc_text_{i}", "");
            }

            // Fill UI slots with active warps
            var activeWarps = Plugin.Instance.Configuration.Instance.Warps.Where(w => w.IsActive).ToList();
            foreach (var warp in activeWarps)
            {
                int uiSlot = warp.WarpId; // Use WarpId as the UI slot
                if (uiSlot >= 1 && uiSlot <= maxSlots) // Ограничиваем количеством слотов из конфига
                {
                    EffectManager.sendUIEffectText(short.MaxValue, player.Player.channel.owner.transportConnection, true, $"Warp_loc_text_{uiSlot}", warp.Name);
                }
            }
        }
    }
}