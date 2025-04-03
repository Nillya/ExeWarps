using System.Collections.Generic;

namespace AdvancedWarps
{
    public static class KnownLocationsProvider
    {
        public static List<KnownLocation> GetKnownLocations()
        {
            return new List<KnownLocation>
            {
                new KnownLocation("Summerside Military Base", new SerializableVector3(0, 0, 0), 1),
                new KnownLocation("Stratford", new SerializableVector3(0, 0, 0), 2),
                new KnownLocation("Alberton", new SerializableVector3(0, 0, 0), 3),
                new KnownLocation("Belfast Airport", new SerializableVector3(0, 0, 0), 4),
                new KnownLocation("Cape Rock", new SerializableVector3(0, 0, 0), 5),
                new KnownLocation("Charlottetown", new SerializableVector3(0, 0, 0), 6),
                new KnownLocation("Confederation Bridge", new SerializableVector3(0, 0, 0), 7),
                new KnownLocation("Courtin Isle", new SerializableVector3(0, 0, 0), 8),
                new KnownLocation("Fernwood Farm", new SerializableVector3(0, 0, 0), 9),
                new KnownLocation("Holman Isle", new SerializableVector3(0, 0, 0), 10)
            };
        }
    }
}