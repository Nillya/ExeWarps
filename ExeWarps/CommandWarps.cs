using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System.Collections.Generic;
using System.Linq;

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

            player.Player.enablePluginWidgetFlag(EPluginWidgetFlags.Modal);
            EffectManager.sendUIEffect(45882, short.MaxValue, player.Player.channel.owner.transportConnection, true);

            // Sort warps by WarpId and display up to 10
            var activeWarps = Plugin.Instance.Configuration.Instance.Warps.Where(w => w.IsActive).OrderBy(w => w.WarpId).ToList();
            for (int i = 0; i < 10; i++)
            {
                string warpName = i < activeWarps.Count ? activeWarps[i].Name : "";
                EffectManager.sendUIEffectText(short.MaxValue, player.Player.channel.owner.transportConnection, true, $"Warp_loc_text_{i + 1}", warpName);
            }
        }
    }
}