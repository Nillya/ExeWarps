using System.Xml.Serialization;
using AdvancedWarps.Core;
using AdvancedWarps.Commands;
using AdvancedWarps.Harmony;
using AdvancedWarps.Utilities;

namespace AdvancedWarps.Models
{
    [XmlRoot("SerializableVector3")]
    public class SerializableVector3
    {
        [XmlAttribute("x")]
        public float X { get; set; }

        [XmlAttribute("y")]
        public float Y { get; set; }

        [XmlAttribute("z")]
        public float Z { get; set; }

        public SerializableVector3()
        {
            X = 0f;
            Y = 0f;
            Z = 0f;
        }

        public SerializableVector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static implicit operator UnityEngine.Vector3(SerializableVector3 vec)
        {
            return new UnityEngine.Vector3(vec.X, vec.Y, vec.Z);
        }

        public static implicit operator SerializableVector3(UnityEngine.Vector3 vec)
        {
            return new SerializableVector3(vec.x, vec.y, vec.z);
        }
    }
}