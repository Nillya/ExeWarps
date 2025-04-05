using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System.Collections.Generic;
using System.Linq;
using AdvancedWarps.Core;
using AdvancedWarps.Models;
using AdvancedWarps.Harmony;
using AdvancedWarps.Utilities;

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

            // Open UI with effect ID 45882
            EffectManager.sendUIEffect(45882, short.MaxValue, player.Player.channel.owner.transportConnection, true);

            // Clear all UI slots first
            for (int i = 1; i <= 10; i++)
            {
                EffectManager.sendUIEffectText(short.MaxValue, player.Player.channel.owner.transportConnection, true, $"Warp_loc_text_{i}", "");
            }
            var activeWarps = Plugin.Instance.Configuration.Instance.Warps.Where(w => w.IsActive).ToList();
            foreach (var warp in activeWarps)
            {
                int uiSlot = warp.WarpId; // Use WarpId as the UI slot
                if (uiSlot >= 1 && uiSlot <= 10) // Only show warps with IDs 1-10
                {
                    EffectManager.sendUIEffectText(short.MaxValue, player.Player.channel.owner.transportConnection, true, $"Warp_loc_text_{uiSlot}", warp.Name);
                }
            }
        }
    }
}