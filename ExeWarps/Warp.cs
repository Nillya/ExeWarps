using System.Collections.Generic;
using System.Xml.Serialization;

namespace AdvancedWarps
{
    public class Warp
    {
        [XmlAttribute]
        public string Name;

        [XmlAttribute]
        public int WarpId;

        public List<SubWarp> SubWarps;

        public Warp(string name, int warpId)
        {
            this.Name = name;
            this.WarpId = warpId;
            this.SubWarps = new List<SubWarp>();
        }

        public Warp()
        {
            this.SubWarps = new List<SubWarp>();
        }
    }

    public class SubWarp
    {
        public int Id;
        public SerializableVector3 Position;

        public SubWarp(int id, SerializableVector3 position)
        {
            this.Id = id;
            this.Position = position;
        }

        public SubWarp()
        {
            this.Position = new SerializableVector3();
        }
    }
}