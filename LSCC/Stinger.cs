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

        public static void ClearStingers(int lobbyID)
        {
            LobbyData _l = Settings.Lobbies.Find(l => l.lobbyId == lobbyID);
            if (_l != null) ClearStingers(_l);
        }
        public static void ClearStingers(LobbyData lobby)
        {
            foreach(Lobby_StingerData lobbyStinger in lobby.Stingers)
            {
                foreach(GTANetworkAPI.Object stingerObject in lobbyStinger.StingerObjects)
                {
                    stingerObject.Delete();
                }
                foreach(ColShape stingerShape in lobbyStinger.StingerColshapes)
                {
                    stingerShape.Delete();
                }
                foreach(Ped stingerPed in lobbyStinger.StingerPeds)
                {
                    stingerPed.Delete();
                }
            }
            lobby.Stingers.Clear();
        }
    }
    public class StingerInfo : Script
    {
        public Vector3 Position;
        public Vector3 Rotation;
        public bool IsNPC; // whether this object is a stinger, or an NPC [cops that stand next to them]
    }
}
