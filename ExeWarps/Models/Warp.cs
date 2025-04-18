﻿using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using AdvancedWarps.Core;
using AdvancedWarps.Commands;
using AdvancedWarps.Harmony;
using AdvancedWarps.Utilities;

namespace AdvancedWarps.Models
{
    public class Warp
    {
        [XmlAttribute]
        public string Name;

        [XmlAttribute]
        public int WarpId;

        [XmlAttribute]
        public bool IsActive = true;

        public List<SubWarp> SubWarps;

        public Warp(string name, int warpId)
        {
            this.Name = name;
            this.WarpId = warpId;
            this.SubWarps = new List<SubWarp>();
            this.IsActive = true;
        }

        public Warp()
        {
            this.SubWarps = new List<SubWarp>();
            this.IsActive = false;
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
    public class AdminWarp
    {
        [XmlAttribute]
        public string Name;

        public SerializableVector3 Position;

        public AdminWarp(string name, SerializableVector3 position)
        {
            this.Name = name;
            this.Position = position;
        }

        public AdminWarp()
        {
            this.Position = new SerializableVector3();
        }
    }
}