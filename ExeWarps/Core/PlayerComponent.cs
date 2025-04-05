using Rocket.Unturned.Player;
using SDG.Unturned;
using UnityEngine;
using System;
using AdvancedWarps.Models;
using AdvancedWarps.Commands;
using AdvancedWarps.Harmony;
using AdvancedWarps.Utilities;

namespace AdvancedWarps.Core
{
    public class PlayerComponent : UnturnedPlayerComponent
    {
        public Warp CurrentWarp = null;
        public DateTime TimeTeleportWarp = DateTime.Now;
        public SerializableVector3 InitialPosition;
        public bool IsTeleporting = false;

        public void CancelTeleport(string messageKey)
        {
            if (!string.IsNullOrEmpty(messageKey))
            {
                new Transelation(messageKey, Array.Empty<object>()).execute(base.Player);
            }
            Plugin.Instance.Warping.Remove(base.Player.CSteamID);
            CurrentWarp = null;
            IsTeleporting = false;
        }

        public void FixedUpdate()
        {
            if (CurrentWarp == null || !IsTeleporting) return;

            // Check movement cancellation
            if (Plugin.Instance.Configuration.Instance.CancelOnMovement)
            {
                float distance = Vector3.Distance(base.Player.Position, (Vector3)InitialPosition);
                if (distance > Plugin.Instance.Configuration.Instance.MovementCancelRadius)
                {
                    CancelTeleport("warp_cancel_movement");
                    return;
                }
            }

            // Check if teleport delay is over
            if ((DateTime.Now - TimeTeleportWarp).TotalSeconds >= Plugin.Instance.Configuration.Instance.DelayTeleportToWarp)
            {
                // Select a random sub-warp
                if (CurrentWarp.SubWarps.Count > 0)
                {
                    int randomIndex = UnityEngine.Random.Range(0, CurrentWarp.SubWarps.Count);
                    SubWarp subWarp = CurrentWarp.SubWarps[randomIndex];
                    base.Player.Teleport((Vector3)subWarp.Position, base.Player.Rotation);
                    new Transelation("warp_successfully_teleported", Array.Empty<object>()).execute(base.Player);
                    Plugin.Instance.AfterWarp(base.Player);
                }
                CurrentWarp = null;
                IsTeleporting = false;
            }
        }
    }
}