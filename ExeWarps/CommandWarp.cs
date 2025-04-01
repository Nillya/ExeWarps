using System;
using System.Collections.Generic;
using System.Linq;
using Rocket.API;
using Rocket.Unturned.Player;
using UnityEngine;

namespace AdvancedWarps
{
    public class CommandWarp : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "warp";
        public string Help => "Warp command for teleportation and management.";
        public string Syntax => "<add|addpd|replace|rem> [warp_name] [second_warp_name]";
        public List<string> Aliases => new List<string>();
        public List<string> Permissions => new List<string> { "warp" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer player = (UnturnedPlayer)caller;
            PlayerComponent component = player.GetComponent<PlayerComponent>();

            if (command.Length == 0)
            {
                new Transelation("warp_null", Array.Empty<object>()).execute(player);
                return;
            }

            if (command[0].ToLower() == "add" && command.Length >= 2 && player.IsAdmin)
            {
                string warpName = command[1];
                if (Plugin.Instance.Configuration.Instance.Warps.Any(w => w.Name.ToLower() == warpName.ToLower()))
                {
                    new Transelation("warp_create_ok", new object[] { warpName }).execute(player);
                    return;
                }

                // Находим максимальный WarpId и добавляем +1
                int newWarpId = Plugin.Instance.Configuration.Instance.Warps.Count > 0
                    ? Plugin.Instance.Configuration.Instance.Warps.Max(w => w.WarpId) + 1
                    : 1;

                Warp newWarp = new Warp(warpName, newWarpId);
                Plugin.Instance.Configuration.Instance.Warps.Add(newWarp);
                Plugin.Instance.Configuration.Save();
                new Transelation("warp_create_ok", new object[] { warpName }).execute(player);
            }
            else if (command[0].ToLower() == "addpd" && command.Length >= 2 && player.IsAdmin)
            {
                // Add a sub-warp to an existing warp
                string warpName = command[1];
                Warp warp = Plugin.Instance.Configuration.Instance.Warps.Find(w => w.Name.ToLower() == warpName.ToLower());
                if (warp == null)
                {
                    new Transelation("warp_null", Array.Empty<object>()).execute(player);
                    return;
                }

                int newSubWarpId = warp.SubWarps.Count + 1;
                warp.SubWarps.Add(new SubWarp(newSubWarpId, new SerializableVector3(player.Position.x, player.Position.y, player.Position.z)));
                Plugin.Instance.Configuration.Save();
                new Transelation("warp_add_subwarp_ok", new object[] { warpName, newSubWarpId }).execute(player);
            }
            else if (command[0].ToLower() == "replace" && command.Length >= 3 && player.IsAdmin)
            {
                Warp warp1, warp2;
                int index1 = -1, index2 = -1;

                // Проверяем, являются ли аргументы числами (для WarpId)
                bool isId1 = int.TryParse(command[1], out int id1);
                bool isId2 = int.TryParse(command[2], out int id2);

                if (isId1 && isId2)
                {
                    // Замена по WarpId
                    warp1 = Plugin.Instance.Configuration.Instance.Warps.Find(w => w.WarpId == id1);
                    warp2 = Plugin.Instance.Configuration.Instance.Warps.Find(w => w.WarpId == id2);
                    index1 = warp1 != null ? Plugin.Instance.Configuration.Instance.Warps.IndexOf(warp1) : -1;
                    index2 = warp2 != null ? Plugin.Instance.Configuration.Instance.Warps.IndexOf(warp2) : -1;
                }
                else if (!isId1 && !isId2)
                {
                    // Существующий вариант замены по именам
                    index1 = Plugin.Instance.Configuration.Instance.Warps.FindIndex(w => w.Name.ToLower() == command[1].ToLower());
                    index2 = Plugin.Instance.Configuration.Instance.Warps.FindIndex(w => w.Name.ToLower() == command[2].ToLower());
                }
                else
                {
                    // Смешанный вариант: имя и ID или ID и имя
                    if (isId1)
                    {
                        warp1 = Plugin.Instance.Configuration.Instance.Warps.Find(w => w.WarpId == id1);
                        index1 = warp1 != null ? Plugin.Instance.Configuration.Instance.Warps.IndexOf(warp1) : -1;
                        index2 = Plugin.Instance.Configuration.Instance.Warps.FindIndex(w => w.Name.ToLower() == command[2].ToLower());
                    }
                    else
                    {
                        index1 = Plugin.Instance.Configuration.Instance.Warps.FindIndex(w => w.Name.ToLower() == command[1].ToLower());
                        warp2 = Plugin.Instance.Configuration.Instance.Warps.Find(w => w.WarpId == id2);
                        index2 = warp2 != null ? Plugin.Instance.Configuration.Instance.Warps.IndexOf(warp2) : -1;
                    }
                }

                if (index1 == -1 || index2 == -1)
                {
                    new Transelation("warp_null", Array.Empty<object>()).execute(player);
                    return;
                }

                // Выполняем замену
                Warp temp = Plugin.Instance.Configuration.Instance.Warps[index1];
                Plugin.Instance.Configuration.Instance.Warps[index1] = Plugin.Instance.Configuration.Instance.Warps[index2];
                Plugin.Instance.Configuration.Instance.Warps[index2] = temp;
                Plugin.Instance.Configuration.Save();

                // Формируем сообщение о результате
                string identifier1 = isId1 ? id1.ToString() : command[1];
                string identifier2 = isId2 ? id2.ToString() : command[2];
                new Transelation("warp_replace_ok", new object[] { identifier1, identifier2 }).execute(player);
            }
            else if (command[0].ToLower() == "rem" && command.Length >= 2 && player.IsAdmin)
            {
                // Remove a warp and its sub-warps
                string warpName = command[1];
                Warp warp = Plugin.Instance.Configuration.Instance.Warps.Find(w => w.Name.ToLower() == warpName.ToLower());
                if (warp == null)
                {
                    new Transelation("warp_null", Array.Empty<object>()).execute(player);
                    return;
                }

                Plugin.Instance.Configuration.Instance.Warps.Remove(warp);
                Plugin.Instance.Configuration.Save();
                new Transelation("warp_delete_ok", new object[] { warpName }).execute(player);
            }
            else
            {
                // Teleport to a warp (random sub-warp)
                string warpName = command[0];
                Warp warp = Plugin.Instance.Configuration.Instance.Warps.Find(w => w.Name.ToLower() == warpName.ToLower());
                if (warp == null || warp.SubWarps.Count == 0)
                {
                    new Transelation("warp_null", Array.Empty<object>()).execute(player);
                    return;
                }

                if (Plugin.Instance.Warping.Contains(player.CSteamID))
                {
                    new Transelation("already_delay", Array.Empty<object>()).execute(player);
                    return;
                }

                component.CurrentWarp = warp;
                component.TimeTeleportWarp = DateTime.Now;
                component.InitialPosition = new SerializableVector3(player.Position.x, player.Position.y, player.Position.z);
                component.IsTeleporting = true;

                Plugin.Instance.Warping.Add(player.CSteamID);
                new Transelation("warp_teleport_ok", new object[] { warp.Name, Plugin.Instance.Configuration.Instance.DelayTeleportToWarp }).execute(player);
            }
        }
    }
}