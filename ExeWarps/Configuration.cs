using System.Collections.Generic;
using Rocket.API;

namespace AdvancedWarps
{
    public class Configuration : IDefaultable, IRocketPluginConfiguration
    {
        public int DelayTeleportToWarp;
        public bool CancelOnDamage;
        public bool CancelOnMovement;
        public float MovementCancelRadius;
        public bool CancelOnShooting;
        public float NoBuildRadius;
        public double WarpProtect;
        public bool AutoLocation;
        public bool AddSubWarpOnCreate;
        public List<Warp> Warps;
        public List<AdminWarp> AdminWarps;

        public void LoadDefaults()
        {
            DelayTeleportToWarp = 3;
            CancelOnDamage = true;
            CancelOnMovement = true;
            MovementCancelRadius = 2f;
            CancelOnShooting = true;
            NoBuildRadius = 5f;
            WarpProtect = 4;
            AutoLocation = true;
            AddSubWarpOnCreate = true;
            Warps = new List<Warp>();
            AdminWarps = new List<AdminWarp>();
        }
    }
}