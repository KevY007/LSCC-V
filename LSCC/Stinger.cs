using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;

namespace RNC
{
    public class Stinger : Script
    {
        public int ID { get; }
        public string Area;
        public string Street;
        public bool IsDeployed;
        public List<StingerInfo> StingerObjects;
        public Stinger() { }
        public Stinger(int id, string area, string street)
        {
            ID = id;
            Area = area;
            Street = street;
            IsDeployed = false;
            StingerObjects = new List<StingerInfo>();

            Stingers.Add(this);
        }
        public Stinger Add(Vector3 position, Vector3 rotation, bool isNPC)
        {
            this.StingerObjects.Add(new StingerInfo() { Position = position, Rotation = rotation, IsNPC = isNPC });
            return this;
        }

        public static List<Stinger> Stingers = new List<Stinger>();
    }
    public class StingerInfo : Script
    {
        public Vector3 Position;
        public Vector3 Rotation;
        public bool IsNPC; // whether this object is a stinger, or an NPC [cops that stand next to them]
    }
}
