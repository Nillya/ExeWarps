// CommandWarp.cs
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
        public string Syntax => "<add|addpd|replace|rem|rempd> [warp_name] [second_warp_name/subwarp_id]";
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
                int index1 = -1, index2 = -1;
                bool isId1 = int.TryParse(command[1], out int id1);
                bool isId2 = int.TryParse(command[2], out int id2);

                // Проверка на попытку замены одного и того же варпа
                if (isId1 && isId2 && id1 == id2)
                {
                    new Transelation("warp_same_swap", Array.Empty<object>()).execute(player);
                    return;
                }
                if (!isId1 && !isId2 && command[1].ToLower() == command[2].ToLower())
                {
                    new Transelation("warp_same_swap", Array.Empty<object>()).execute(player);
                    return;
                }

                // Определяем индексы для замены
                if (isId1 && isId2)
                {
                    // Замена по WarpId (перемещение варпа на указанную позицию)
                    Warp warp1 = Plugin.Instance.Configuration.Instance.Warps.Find(w => w.WarpId == id1);
                    index1 = warp1 != null ? Plugin.Instance.Configuration.Instance.Warps.IndexOf(warp1) : -1;
                    index2 = id2 - 1; // Целевая позиция в списке (0-based)
                }
                else if (!isId1 && !isId2)
                {
                    // Замена по именам (стандартный свап двух существующих варпов)
                    index1 = Plugin.Instance.Configuration.Instance.Warps.FindIndex(w => w.Name.ToLower() == command[1].ToLower());
                    index2 = Plugin.Instance.Configuration.Instance.Warps.FindIndex(w => w.Name.ToLower() == command[2].ToLower());
                }
                else
                {
                    // Смешанный вариант: имя и ID или ID и имя
                    if (isId1)
                    {
                        Warp warp1 = Plugin.Instance.Configuration.Instance.Warps.Find(w => w.WarpId == id1);
                        index1 = warp1 != null ? Plugin.Instance.Configuration.Instance.Warps.IndexOf(warp1) : -1;
                        index2 = Plugin.Instance.Configuration.Instance.Warps.FindIndex(w => w.Name.ToLower() == command[2].ToLower());
                        if (index2 == -1 && isId2) index2 = id2 - 1; // Если имя не найдено, но есть ID
                    }
                    else
                    {
                        index1 = Plugin.Instance.Configuration.Instance.Warps.FindIndex(w => w.Name.ToLower() == command[1].ToLower());
                        Warp warp2 = Plugin.Instance.Configuration.Instance.Warps.Find(w => w.WarpId == id2);
                        index2 = warp2 != null ? Plugin.Instance.Configuration.Instance.Warps.IndexOf(warp2) : (id2 - 1);
                    }
                }

                // Проверка на существование первого варпа (источника)
                if (index1 == -1)
                {
                    new Transelation("warp_null", Array.Empty<object>()).execute(player);
                    return;
                }

                // Проверка на корректность позиции назначения
                if (index2 < 0)
                {
                    new Transelation("invalid_position", Array.Empty<object>()).execute(player);
                    return;
                }

                // Если index2 больше текущего размера списка, но меньше максимального индекса UI (10), перемещаем варп
                if (index2 >= Plugin.Instance.Configuration.Instance.Warps.Count && index2 < 10)
                {
                    Warp movingWarp = Plugin.Instance.Configuration.Instance.Warps[index1];
                    Plugin.Instance.Configuration.Instance.Warps.RemoveAt(index1);
                    Plugin.Instance.Configuration.Instance.Warps.Insert(index2, movingWarp);
                }
                else if (index2 < Plugin.Instance.Configuration.Instance.Warps.Count)
                {
                    // Свап двух существующих варпов или перемещение на занятую позицию
                    Warp temp = Plugin.Instance.Configuration.Instance.Warps[index1];
                    Plugin.Instance.Configuration.Instance.Warps.RemoveAt(index1);
                    if (index2 > index1) index2--; // Корректировка индекса после удаления
                    Plugin.Instance.Configuration.Instance.Warps.Insert(index2, temp);
                }
                else
                {
                    new Transelation("invalid_position", Array.Empty<object>()).execute(player);
                    return;
                }

                Plugin.Instance.Configuration.Save();

                string identifier1 = isId1 ? id1.ToString() : command[1];
                string identifier2 = isId2 ? id2.ToString() : command[2];
                new Transelation("warp_replace_ok", new object[] { identifier1, identifier2 }).execute(player);
            }
            else if (command[0].ToLower() == "rem" && command.Length >= 2 && player.IsAdmin)
            {
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
            else if (command[0].ToLower() == "rempd" && command.Length >= 3 && player.IsAdmin)
            {
                string warpName = command[1];
                Warp warp = Plugin.Instance.Configuration.Instance.Warps.Find(w => w.Name.ToLower() == warpName.ToLower());
                if (warp == null)
                {
                    new Transelation("warp_null", Array.Empty<object>()).execute(player);
                    return;
                }

                if (!int.TryParse(command[2], out int subWarpId) || subWarpId <= 0)
                {
                    new Transelation("invalid_subwarp_id", Array.Empty<object>()).execute(player);
                    return;
                }

                SubWarp subWarp = warp.SubWarps.Find(sw => sw.Id == subWarpId);
                if (subWarp == null)
                {
                    new Transelation("subwarp_not_found", new object[] { subWarpId }).execute(player);
                    return;
                }

                warp.SubWarps.Remove(subWarp);
                for (int i = 0; i < warp.SubWarps.Count; i++)
                {
                    warp.SubWarps[i].Id = i + 1;
                }

                Plugin.Instance.Configuration.Save();
                new Transelation("subwarp_delete_ok", new object[] { warpName, subWarpId }).execute(player);
            }
            else
            {
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