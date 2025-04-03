using System;
using System.Collections.Generic;
using System.Linq;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using UnityEngine;

namespace AdvancedWarps
{
    public class CommandWarp : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "warp";
        public string Help => "Warp command for teleportation and management.";
        public string Syntax => "<list|add|adda|addpd|replace|rem|rempd> [warp_name] [second_warp_name/subwarp_id]";
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
            if (command[0].ToLower() == "list")
            {
                var activeWarps = Plugin.Instance.Configuration.Instance.Warps.Where(w => w.IsActive).OrderBy(w => w.WarpId).ToList();
                if (activeWarps.Count > 0)
                {
                    string warpList = player.IsAdmin
                        ? string.Join(", ", activeWarps.Select(w => $"[ID: {w.WarpId}] {w.Name}"))
                        : string.Join(", ", activeWarps.Select(w => w.Name));
                    UnturnedChat.Say(player, $"Available warps: {warpList}", Color.yellow);
                }
                else
                {
                    new Transelation("warp_list_empty", Array.Empty<object>()).execute(player);
                }

                if (player.IsAdmin && Plugin.Instance.Configuration.Instance.AdminWarps.Count > 0)
                {
                    string adminWarpList = string.Join(", ", Plugin.Instance.Configuration.Instance.AdminWarps.Select(w => w.Name));
                    UnturnedChat.Say(player, $"Admin warps: {adminWarpList}", Color.cyan);
                }
                return;
            }
            else if (command[0].ToLower() == "add" && command.Length >= 2 && player.IsAdmin)
            {
                string warpName = command[1];

                if (Plugin.Instance.Configuration.Instance.Warps.Any(w => w.Name != null && w.Name.ToLower() == warpName.ToLower()) ||
                    Plugin.Instance.Configuration.Instance.AdminWarps.Any(w => w.Name != null && w.Name.ToLower() == warpName.ToLower()))
                {
                    new Transelation("warp_exists", new object[] { warpName }).execute(player);
                    return;
                }

                Vector3 playerPosition = player.Position;
                Warp newWarp;

                if (Plugin.Instance.Configuration.Instance.AutoLocation)
                {
                    // Находим ближайшую известную локацию
                    KnownLocation nearestLocation = null;
                    float minDistance = float.MaxValue;
                    var knownLocations = KnownLocationsProvider.GetKnownLocations();

                    foreach (var location in knownLocations)
                    {
                        float distance = Vector3.Distance(playerPosition, (Vector3)location.Position);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            nearestLocation = location;
                        }
                    }

                    if (nearestLocation == null)
                    {
                        new Transelation("warp_null", Array.Empty<object>()).execute(player);
                        return;
                    }

                    // Проверяем, есть ли уже активный варп с таким WarpId
                    Warp existingWarp = Plugin.Instance.Configuration.Instance.Warps.Find(w => w.WarpId == nearestLocation.WarpId && w.IsActive);
                    if (existingWarp != null)
                    {
                        new Transelation("warp_already_at_location", new object[] { nearestLocation.Name }).execute(player);
                        return;
                    }

                    // Создаем новый варп с привязкой к известной локации
                    newWarp = new Warp(warpName, nearestLocation.WarpId);
                }
                else
                {
                    // Находим первый свободный WarpId
                    var existingWarpIds = Plugin.Instance.Configuration.Instance.Warps
                        .Where(w => w.IsActive)
                        .Select(w => w.WarpId)
                        .OrderBy(id => id)
                        .ToList();

                    int newWarpId = 1;
                    while (existingWarpIds.Contains(newWarpId))
                    {
                        newWarpId++;
                    }

                    // Создаем новый варп с первым свободным WarpId
                    newWarp = new Warp(warpName, newWarpId);
                }

                newWarp.SubWarps.Add(new SubWarp(1, new SerializableVector3(playerPosition.x, playerPosition.y, playerPosition.z)));
                Plugin.Instance.Configuration.Instance.Warps.Add(newWarp);

                Plugin.Instance.Configuration.Save();
                new Transelation("warp_create_ok", new object[] { warpName }).execute(player);
            }
            else if (command[0].ToLower() == "adda" && command.Length >= 2 && player.IsAdmin)
            {
                string warpName = command[1];
                if (Plugin.Instance.Configuration.Instance.Warps.Any(w => w.Name != null && w.Name.ToLower() == warpName.ToLower()) ||
                    Plugin.Instance.Configuration.Instance.AdminWarps.Any(w => w.Name != null && w.Name.ToLower() == warpName.ToLower()))
                {
                    new Transelation("warp_exists", new object[] { warpName }).execute(player);
                    return;
                }

                AdminWarp newAdminWarp = new AdminWarp(warpName, new SerializableVector3(player.Position.x, player.Position.y, player.Position.z));
                Plugin.Instance.Configuration.Instance.AdminWarps.Add(newAdminWarp);
                Plugin.Instance.Configuration.Save();
                new Transelation("admin_warp_create_ok", new object[] { warpName }).execute(player);
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
                int id1, id2;
                bool isId1 = int.TryParse(command[1], out id1);
                bool isId2 = int.TryParse(command[2], out id2);

                // Check if trying to move to the same warp
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

                // Find source warp
                Warp sourceWarp = null;
                if (isId1)
                {
                    sourceWarp = Plugin.Instance.Configuration.Instance.Warps.Find(w => w.WarpId == id1 && w.IsActive);
                }
                else
                {
                    sourceWarp = Plugin.Instance.Configuration.Instance.Warps.Find(w => w.Name != null && w.IsActive && w.Name.ToLower() == command[1].ToLower());
                }

                if (sourceWarp == null)
                {
                    new Transelation("warp_null", Array.Empty<object>()).execute(player);
                    return;
                }

                // Handle target based on whether it's an ID or name
                if (isId2)
                {
                    // Target is an ID
                    int targetId = id2;
                    if (targetId < 1 || targetId > 100) // Reasonable upper limit
                    {
                        new Transelation("invalid_position", Array.Empty<object>()).execute(player);
                        return;
                    }

                    Warp existingWarp = Plugin.Instance.Configuration.Instance.Warps.Find(w => w.WarpId == targetId && w.IsActive);
                    if (existingWarp != null && existingWarp != sourceWarp)
                    {
                        // Swap IDs if the target ID is occupied
                        int tempId = sourceWarp.WarpId;
                        sourceWarp.WarpId = targetId;
                        existingWarp.WarpId = tempId;
                    }
                    else
                    {
                        // Just reassign the source warp's ID to the target ID
                        sourceWarp.WarpId = targetId;
                    }
                }
                else
                {
                    // Target is a name
                    Warp targetWarp = Plugin.Instance.Configuration.Instance.Warps.Find(w => w.Name != null && w.IsActive && w.Name.ToLower() == command[2].ToLower());
                    if (targetWarp == null)
                    {
                        new Transelation("warp_null", Array.Empty<object>()).execute(player);
                        return;
                    }

                    // Swap IDs between sourceWarp and targetWarp
                    int tempId = sourceWarp.WarpId;
                    sourceWarp.WarpId = targetWarp.WarpId;
                    targetWarp.WarpId = tempId;
                }

                // Sort the list by WarpId
                Plugin.Instance.Configuration.Instance.Warps.Sort((a, b) => a.WarpId.CompareTo(b.WarpId));

                Plugin.Instance.Configuration.Save();

                string identifier1 = isId1 ? id1.ToString() : command[1];
                string identifier2 = isId2 ? id2.ToString() : command[2];
                new Transelation("warp_replace_ok", new object[] { identifier1, identifier2 }).execute(player);
            }
            else if (command[0].ToLower() == "rem" && command.Length >= 2 && player.IsAdmin)
            {
                string warpName = command[1];
                Warp warp = Plugin.Instance.Configuration.Instance.Warps.Find(w => w.Name != null && w.Name.ToLower() == warpName.ToLower());
                AdminWarp adminWarp = Plugin.Instance.Configuration.Instance.AdminWarps.Find(w => w.Name.ToLower() == warpName.ToLower());

                if (warp != null)
                {
                    warp.Name = null; // Mark as inactive instead of removing
                    warp.SubWarps.Clear();
                    warp.IsActive = false;
                    Plugin.Instance.Configuration.Save();
                    new Transelation("warp_delete_ok", new object[] { warpName }).execute(player);
                }
                else if (adminWarp != null)
                {
                    Plugin.Instance.Configuration.Instance.AdminWarps.Remove(adminWarp);
                    Plugin.Instance.Configuration.Save();
                    new Transelation("admin_warp_delete_ok", new object[] { warpName }).execute(player);
                }
                else
                {
                    new Transelation("warp_null", Array.Empty<object>()).execute(player);
                }
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
                Warp warp = Plugin.Instance.Configuration.Instance.Warps.Find(w => w.Name != null && w.IsActive && w.Name.ToLower() == warpName.ToLower());
                AdminWarp adminWarp = Plugin.Instance.Configuration.Instance.AdminWarps.Find(w => w.Name.ToLower() == warpName.ToLower());

                if (adminWarp != null)
                {
                    if (!player.IsAdmin)
                    {
                        new Transelation("no_admin_access", Array.Empty<object>()).execute(player);
                        return;
                    }

                    player.Teleport((Vector3)adminWarp.Position, player.Rotation);
                    new Transelation("warp_successfully_teleported", Array.Empty<object>()).execute(player);
                }
                else if (warp != null && warp.SubWarps.Count > 0)
                {
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
                else
                {
                    new Transelation("warp_null", Array.Empty<object>()).execute(player);
                }
            }
        }
    }
}