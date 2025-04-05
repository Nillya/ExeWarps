using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using UnityEngine;
using AdvancedWarps.Core;
using AdvancedWarps.Models;
using AdvancedWarps.Commands;
using AdvancedWarps.Harmony;

namespace AdvancedWarps.Utilities
{
    public class Transelation
    {
        private Color _color;
        private string _message;
        private string _transelation;
        private readonly RocketPlugin _plugin;

        public Transelation(string transelation, params object[] args)
        {
            this._transelation = transelation;
            this._plugin = Plugin.Instance;
            this.refreshMessage(transelation, args);
        }

        public void refreshMessage(string trans, params object[] args)
        {
            string text = this.plugin.Translate(trans, args);
            try
            {
                int num = text.ToLower().LastIndexOf("color=");
                string text2 = text.Substring(num + 6);
                string text3 = text2;
                bool flag = text2.StartsWith("#");
                if (flag)
                {
                    text2 = text2.Remove(0, 1);
                }
                bool flag2 = !text3.StartsWith("#");
                if (flag2)
                {
                    text3 = text3.Insert(0, "#");
                }
                text = text.Remove(num);
                bool flag3 = ColorUtility.TryParseHtmlString(text3, out this._color);
                if (!flag3)
                {
                    this._color = UnturnedChat.GetColorFromName(text2, Color.green);
                }
            }
            catch
            {
                this._color = Color.green;
            }
            this._message = text;
        }

        public void execute(Player player)
        {
            UnturnedChat.Say(UnturnedPlayer.FromPlayer(player), this._message, this._color);
        }

        public void execute(UnturnedPlayer player)
        {
            UnturnedChat.Say(player, this._message, this._color);
        }

        public void execute()
        {
            UnturnedChat.Say(this._message, this._color);
        }

        public RocketPlugin plugin => this._plugin;
        public Color color => this._color;
        public string message => this._message;
        public string transelation => this._transelation;
    }
}