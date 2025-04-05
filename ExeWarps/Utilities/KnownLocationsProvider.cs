using System.Collections.Generic;
using AdvancedWarps.Core;
using AdvancedWarps.Models;
using AdvancedWarps.Commands;
using AdvancedWarps.Harmony;

namespace AdvancedWarps.Utilities
{
    public static class KnownLocationsProvider
    {
        public static List<KnownLocation> GetKnownLocations()
        {
            return new List<KnownLocation>
            {
                // Локации с предоставленными координатами
                new KnownLocation("Summerside Military Base", new SerializableVector3(-462.136f, 36.2897f, 686.4271f), 1),
                new KnownLocation("Stratford", new SerializableVector3(-76.6875f, 38.60232f, 643.3672f), 2),
                new KnownLocation("Alberton", new SerializableVector3(-554.960938f, 33.57555f, 160.367188f), 3),
                new KnownLocation("O'Leary Prison", new SerializableVector3(-244.96875f, 44.00486f, 14.828125f), 4),
                new KnownLocation("Charlottetown", new SerializableVector3(16.3046875f, 33.8957367f, -447.476563f), 5),
                new KnownLocation("Holman Isle", new SerializableVector3(-743.1719f, 55.1531219f, -766.3828f), 6),
                new KnownLocation("Tignish Farm", new SerializableVector3(547.726563f, 38.39512f, -747.1172f), 7),
                new KnownLocation("Montague", new SerializableVector3(295.3047f, 34.1097946f, -75.07031f), 8),
                new KnownLocation("Courtin Isle", new SerializableVector3(826.726563f, 47.8821335f, 134.679688f), 9),
                new KnownLocation("Belfast Airport", new SerializableVector3(727.085938f, 34.3646622f, 674.03125f), 10),

                // Оставшиеся локации из полного списка с нулевыми координатами
                //new KnownLocation("Cape Rock", new SerializableVector3(0, 0, 0), 11),
                //new KnownLocation("Confederation Bridge", new SerializableVector3(0, 0, 0), 12),
                //new KnownLocation("Fernwood Farm", new SerializableVector3(0, 0, 0), 13),
                //new KnownLocation("Kensington Campground", new SerializableVector3(0, 0, 0), 14),
                //new KnownLocation("Liberation Bridge", new SerializableVector3(0, 0, 0), 15),
                //new KnownLocation("Oulton's Isle", new SerializableVector3(0, 0, 0), 16),
                //new KnownLocation("Pirate Cove", new SerializableVector3(0, 0, 0), 17),
                //new KnownLocation("Souris Campground", new SerializableVector3(0, 0, 0), 18),
                //new KnownLocation("Taylor Beach", new SerializableVector3(0, 0, 0), 19),
                //new KnownLocation("Wellington Farm", new SerializableVector3(0, 0, 0), 20),
                //new KnownLocation("Wiltshire Farm", new SerializableVector3(0, 0, 0), 21)
            };
        }
    }
}