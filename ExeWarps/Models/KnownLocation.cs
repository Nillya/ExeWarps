using System.Collections.Generic;
using Rocket.API;
using AdvancedWarps.Core;
using AdvancedWarps.Commands;
using AdvancedWarps.Harmony;
using AdvancedWarps.Utilities;

namespace AdvancedWarps.Models
{
    public class KnownLocation
    {
        public string Name { get; set; }
        public SerializableVector3 Position { get; set; }
        public int WarpId { get; set; }

        public KnownLocation(string name, SerializableVector3 position, int warpId)
        {
            Name = name;
            Position = position;
            WarpId = warpId;
        }
    }
}