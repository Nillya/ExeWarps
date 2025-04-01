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
        public List<Warp> Warps;

        public void LoadDefaults()
        {
            DelayTeleportToWarp = 10;
            CancelOnDamage = true;
            CancelOnMovement = true;
            MovementCancelRadius = 2f;
            CancelOnShooting = true;
            NoBuildRadius = 5f;
            WarpProtect = 3.5;
            Warps = new List<Warp>();
        }
    }
}