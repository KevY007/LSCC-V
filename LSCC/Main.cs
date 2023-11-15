using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Resources;
using GTANetworkAPI;
using System.Linq;
using RNC;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using CryptSharp.Utility;
using Microsoft.EntityFrameworkCore.Internal;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using RNC.Data;
using Microsoft.Extensions.Logging;

namespace RNC
{
    public partial class jCarGenData
    {
        [JsonProperty("x")]
        public float X { get; set; }
        [JsonProperty("y")]
        public float Y { get; set; }
        [JsonProperty("z")]
        public float Z { get; set; }
        [JsonProperty("heading")]
        public float Heading { get; set; }
        [JsonProperty("models")]
        public string[] Models { get; set; }

        public static List<jCarGenData> FromJson(string json) => JsonConvert.DeserializeObject<List<jCarGenData>>(json, new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        });
    }
    public partial class jWeaponData
    {
        [JsonProperty("HashKey")]
        public string HashKey { get; set; }

        [JsonProperty("NameGXT")]
        public string NameGxt { get; set; }

        [JsonProperty("DescriptionGXT")]
        public string DescriptionGxt { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty("Group")]
        public string Group { get; set; }

        [JsonProperty("ModelHashKey")]
        public string ModelHashKey { get; set; }

        [JsonProperty("DefaultClipSize")]
        public long DefaultClipSize { get; set; }

        [JsonProperty("AmmoType", NullValueHandling = NullValueHandling.Ignore)]
        public string AmmoType { get; set; }

        [JsonProperty("Components")]
        public Dictionary<long, Component> Components { get; set; }

        [JsonProperty("Tints")]
        public List<LiveryColor> Tints { get; set; }

        [JsonProperty("LiveryColors")]
        public List<LiveryColor> LiveryColors { get; set; }

        [JsonProperty("DLC")]
        public string Dlc { get; set; }

        public static Dictionary<long, jWeaponData> FromJson(string json) => JsonConvert.DeserializeObject<Dictionary<long, jWeaponData>>(json, new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        });
    }

    public partial class Component
    {
        [JsonProperty("HashKey")]
        public string HashKey { get; set; }

        [JsonProperty("NameGXT")]
        public string NameGxt { get; set; }

        [JsonProperty("DescriptionGXT")]
        public string DescriptionGxt { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty("ModelHashKey")]
        public string ModelHashKey { get; set; }

        [JsonProperty("IsDefault")]
        public bool IsDefault { get; set; }

        [JsonProperty("AmmoType", NullValueHandling = NullValueHandling.Ignore)]
        public string AmmoType { get; set; }
    }

    public partial class LiveryColor
    {
        [JsonProperty("NameGXT")]
        public string NameGxt { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }
    }

    public enum GameModes : short
    {
        STANDARD = 0,
        BUSTED = 1,
        BANKROBBERY = 2,
    }

    public class Lobby_StingerData
    {
        public int ID;
        public List<GTANetworkAPI.Object> StingerObjects = new List<GTANetworkAPI.Object>();
        public List<ColShape> StingerColshapes = new List<ColShape>();
        public List<Ped> StingerPeds = new List<Ped>();

        /*
        SPIKE STAGES:
        1 => White Icon => Spikes have just been placed down.
        2 => Yellow Icon => 15 seconds have passed, police peds are spawned next to the spikes.
        3 => Green Icon => Spikes are currently deployed at that location.
        4 => Dark-red Icon => Spikes were destroyed, probably because suspects' vehicle drove over them.
        */
        public int Stage = 1;
        public int TimeElapsed = 0;

        public Lobby_StingerData() { }
    }

    public class EventManager
    {
        public class EventData
        {
            public class EventPlayerData
            {
                public Player Player { get; set; }
                public int Team { get; set; }
                public bool IsParticipating { get; set; }
            }
            
            public string Name { get; set; }
            public string Description { get; set; }
            public LobbyData.EventLobbyID EventID { get; set; }
            public bool Enabled { get; set; }
            public bool InProgress { get; set; }
            public List<EventPlayerData> EventPlayers { get; set; }
            public int TimeUntilStart = 90;
            public LobbyData EventLobby { get; set; }

            public EventData()
            {

            }
            public EventData(string name, LobbyData.EventLobbyID eventLobbyID, bool enabled, GameModes lobbyMode, string description = "")
            {
                this.InProgress = false;
                this.Name = name;
                this.EventID = eventLobbyID;
                this.Enabled = enabled;
                EventPlayers = new List<EventPlayerData>();
                this.Description = description;
                this.TimeUntilStart = 90;

                this.EventLobby = new LobbyData()
                {
                    lobbyId = (int)eventLobbyID,
                    gameMode = lobbyMode,
                    Name = $"[EVENT] {name}",
                    MaxPlayers = 1000,
                    Enabled = enabled
                };
            }
        }

        public static List<EventData> Events = new List<EventData>();
        public EventManager()
        {

        }
        public static void Initialize()
        {
            Events.Add(new EventData("Bank Robbery", LobbyData.EventLobbyID.BankRobbery, false, GameModes.BANKROBBERY,
                "An action & objective-focused event in which you have to rob a bank & deliver the money as fugitive, " +
                "or stop the robbery as a police officer!"));
        }
        public static EventData GetEvent(LobbyData.EventLobbyID EventID)
        {
            return Events.FirstOrDefault(e => e.EventID == EventID);
        }
        public static EventData GetEvent(string eventName)
        {
            return Events.FirstOrDefault(e => e.Name.Equals(eventName));
        }
        public static void ToggleEvent(LobbyData.EventLobbyID EventID, bool value)
        {
            EventData _d = GetEvent(EventID);
            if (_d == null) return;

            _d.Enabled = value;
            if (value == false) OnEventDisabled(_d);
            if (value == true) OnEventEnabled(_d);
        }
        public static void StartEvent(EventData gameEvent)
        {
            gameEvent.InProgress = true;
            gameEvent.EventLobby.GameAutoStart = true;
            foreach(EventData.EventPlayerData eventPlayer in gameEvent.EventPlayers)
            {
                eventPlayer.Player.SendChatMessage("!{#ffff00}[EVENT] !{#ffffff}Event room is now closed & the event will begin shortly!");
            }
        }


        private static void OnEventEnabled(EventData eventData)
        {
            LobbyData lobby = Settings.Lobbies.FirstOrDefault(l => l.lobbyId == (int)eventData.EventID);
            if (lobby == null) return;
            lobby.Enabled = true;
            lobby.GameAutoStart = false; // don't let it start for a while, maybe 60-90 seconds?

            foreach(Player player in Settings.LoggedInPlayers)
            {
                EPlayer.SendEventNotification(player, eventData.Name, eventData.Description);
            }
        }
        private static void OnEventDisabled(EventData eventData)
        {
            LobbyData lobby = Settings.Lobbies.FirstOrDefault(l => l.lobbyId == (int)eventData.EventID);
            if (lobby == null) return;
            lobby.Enabled = false;
            lobby.GameAutoStart = false;

            // If they are still in the lobby when it's disabled, fuck them niggas. Spawn them in some other lobby.
            // Since this one is disabled and their lobby ID is set to -1, it'll pick a new one for them automatically.
            foreach (Player player in Settings.LoggedInPlayers.Where(p => p.GetSharedDataSafe<int>("InLobby") == (int)eventData.EventID))
            {
                player.SetSharedData("InLobby", -1);
                NAPI.Player.SpawnPlayer(player, Settings.LobbySpawns.OrderBy(c => Guid.NewGuid()).First());
            }
            foreach(EventData.EventPlayerData ePlayer in eventData.EventPlayers)
            {
                ePlayer.Player.SendChatMessage("Thanks for participating in the event. You get score and shit now bla bla");
            }

            eventData.EventPlayers.Clear();
        }
    }

    public class LobbyData
    {
        public enum EventLobbyID
        {
            BankRobbery = 50000
        };

        public enum GameStatus 
        { 
            None = 0,
            Active = 1,
            Starting = 2
        };

        public Map CurrentMap { get; set; }
        public string Name { get; set; } = "";
        public int LobbyOwnerSQLID { get; set; }

        public List<Player> Cops { get; set; }
        public List<Player> Fugitives { get; set; }
        public List<Vehicle> Vehicles { get; set; }
        public List<Pickup> Drops { get; set; }

        public List<Vehicle> ParkedVehicles { get; set; }

        public BankRobberyData BankRobbery { get; set; }
        public int NextEventCounter { get; set; }

        public int lobbyId { get; set; }
        public int lastMapId { get; set; }
        public int gameStatus { get; set; }
        public GameModes gameMode { get; set; }

        public int FugitiveShowCounter { get; set; }
        public int LastSeenCounter { get; set; }
        public bool AreFugitivesVisible { get; set; }

        public int CollisionCounter { get; set; }
        public int GameProgress { get; set; }
        public int FugitivePercent { get; set; }
        public bool GameAutoStart { get; set; }
        public long LastAutoStartMsg { get; set; }
        public bool Enabled { get; set; }
        public bool PlayerCreated { get; set; }
        public string Password { get; set; }
        public int[] Time { get; set; }
        public int Weather { get; set; }
        public int NextMapID { get; set; }
        public string Background { get; set; }
        public int MaxPlayers { get; set; } = 20;
        public KeyValuePair<Player, Player> FirstKill { get; set; }
        public int MaxPing { get; set; } = 500;

        public bool EarlyFugitiveWarningSent = false;

        public List<Lobby_StingerData> Stingers { get; set; }

        public List<Blip> ParkedVehicleBlips { get; set; }

        public List<Player> BannedPlayers { get; set; }

        public LobbyData()
        {
            EarlyFugitiveWarningSent = false;
            CurrentMap = null;

            Name = "";
            Cops = new List<Player>();
            Fugitives = new List<Player>();
            Vehicles = new List<Vehicle>();
            Drops = new List<Pickup>();

            ParkedVehicleBlips = new List<Blip>();

            BannedPlayers = new List<Player>();

            BankRobbery = new BankRobberyData();
            NextEventCounter = 10; // how many rounds left until an event

            FirstKill = new KeyValuePair<Player, Player>(null, null);

            Stingers = new List<Lobby_StingerData>();

            Time = new int[3];
            Time[0] = new Random(DateTimeOffset.UtcNow.Millisecond).Next(0, 23);
            Time[1] = new Random(DateTimeOffset.UtcNow.Millisecond).Next(0, 59);
            Time[2] = new Random(DateTimeOffset.UtcNow.Millisecond).Next(0, 59);

            Weather = new Random(DateTimeOffset.UtcNow.Millisecond).Next(0, 9);

            MaxPing = 500;
            MaxPlayers = 20;
            lobbyId = -1;
            lastMapId = -1;
            for (int i = 1; i < Settings.NextLobbyID; i++)
            {
                if (Settings.Lobbies.Count(a => a.lobbyId == i) == 0)
                {
                    lobbyId = i;
                }
            }

            if (lobbyId == -1) lobbyId = Settings.NextLobbyID;

            Settings.NextLobbyID++;

            gameStatus = 0;
            gameMode = GameModes.STANDARD;
            FugitiveShowCounter = 0;
            LastSeenCounter = 0;
            AreFugitivesVisible = false;
            CollisionCounter = 0;
            GameProgress = 0;
            NextMapID = -1;
            GameAutoStart = true;
            LastAutoStartMsg = 0;
            FugitivePercent = 28;
            Enabled = true;
            PlayerCreated = false;
            LobbyOwnerSQLID = -1;
            Password = "";

            string[] lobbyBgs = {
                    "package://Artwork/CC1.jpg",
                };

            Background = lobbyBgs.OrderBy(x => Guid.NewGuid()).First();

            Settings.Lobbies.Add(this);
        }
    };

    public class ItemData
    {
        public int ID { get; set; }
        public string Image { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public int Price { get; set; }
        public string Description { get; set; }
        public int TeamID { get; set; }

        public ItemData(int ID, string name, string type, int price, string description, string image, int teamID = -1)
        {
            this.ID = ID;
            Name = name;
            Type = type;
            Price = price;
            Description = description;
            Image = image;
            TeamID = teamID;

            Items.Add(this);
        }

        public static List<ItemData> Items = new List<ItemData>();
    }

    public class RequestData
    {
        public int RequestID { get; set; }
        public int RequestType { get; set; }
        public string Description { get; set; }
        public Player Reporter { get; set; }
        public Player Reported { get; set; }

        public RequestData(Player reporter, string description, int type)
        {
            RequestID = -1;
            for (int i = 1; i < Settings.NextRequestID; i++)
            {
                if (Settings.Requests.Count(a => a.RequestID == i) == 0)
                {
                    RequestID = i;
                }
            }

            if (RequestID == -1) RequestID = Settings.NextRequestID;

            Settings.NextRequestID++;
            Reporter = reporter;
            Description = description;
            RequestType = type;
            Reported = reporter;
        }
    };

    public class LoadoutData
    {
        public int Team { get; set; }
        public string Name { get; set; }
        public string Background { get; set; }
        public int LevelRequired { get; set; }
        public int PointsRequired { get; set; }
        public int DonatorPointsRequired { get; set; }
        public int DonatorLevelRequired { get; set; }
        public List<object[]> Weapons { get; set; }

        public LoadoutData(int team, string name, int level = 0, int points = 0, int donorpoints = 0, int donator = 0)
        {
            Background = "package://Artwork/LoadoutBG.png";
            this.Team = team;
            this.Name = name;
            this.LevelRequired = level;
            this.PointsRequired = points;
            this.DonatorPointsRequired = donorpoints;
            this.DonatorLevelRequired = donator;

            this.Weapons = new List<object[]>();
            //this.Weapons.AddRange(weps);

            Settings.Loadouts.Add(this);
        }

        public LoadoutData Add(WeaponHash weapon, string image, string name, int ammo)
        {
            #region Adding weapons to challenges list
            if (name != "Full Armour" && weapon != WeaponHash.Bzgas && weapon != WeaponHash.Molotov && ammo > 10)
            {
                jWeaponData wepDat = new jWeaponData();
                string reasonStr = "";

                if (Settings.WeaponData.TryGetValue((long)weapon, out wepDat))
                {
                    reasonStr = wepDat.Group;
                }
                else
                {
                    NAPI.Util.ConsoleOutput("[WEAPONDATA.JSON HARAM?] Could not find key for weapon " + name + " in 'weapondata.json'. (Hash: " + ((uint)weapon).ToString() + ")");
                    reasonStr = "Unknown";
                }

                if (!Challenge.PossibleWeaponGroups.Contains(reasonStr) && reasonStr != "Unknown")
                {
                    Challenge.PossibleWeaponGroups.Add(reasonStr);
                }
            }
            #endregion

            this.Weapons.Add(new object[] { weapon, image, name, ammo });
            return this;
        }
    };

    public class CarData
    {
        public int Team { get; set; }
        public string Model { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public int LevelRequired { get; set; }
        public int PointsRequired { get; set; }
        public int DonatorPointsRequired { get; set; }
        public int DonatorLevelRequired { get; set; }
        public string Info { get; set; }
        public CarData(int team, string name, string model, string image, string info, int level = 0, int points = 0, int donorpoints = 0, int donator = 0)
        {
            this.Team = team;
            this.Name = name;
            this.LevelRequired = level;
            this.PointsRequired = points;
            this.DonatorPointsRequired = donorpoints;
            this.DonatorLevelRequired = donator;
            this.Model = model;
            this.Image = image;
            this.Info = info;

            if (model != "?")
                Challenge.CopchaseVehicles.Add(name); // adding this model to the list of possible vehicles for a copchase challenge

            Settings.Cars.Add(this);
        }
    };

    public class MOTDData
    {
        public string AdminName { get; set; }
        public string PostedOn { get; set; }
        public string Message { get; set; }
    };


    public partial class DynSet
    {
        [JsonProperty("DerbyVehicle")]
        public string DerbyVehicle { get; set; }

        [JsonProperty("OnfootCollision")]
        public bool OnfootCollision { get; set; }

        [JsonProperty("ChaseAntiFallOffBike")]
        public bool AntiFallOffBike { get; set; }

        [JsonProperty("ChaseAntiFallOffBikeSpeedLimit")]
        public int AntiFallOffBikeSpeedLimit { get; set; }

        [JsonProperty("CameraSettingsUpdate")]
        public int CameraSettingsUpdate { get; set; }

        [JsonProperty("WeaponDataURL")]
        public string WeaponDataURL { get; set; }

        [JsonProperty("MaxInLobby")]
        public int MaxInLobby { get; set; }

        [JsonProperty("Discord")]
        public bool Discord { get; set; }

        [JsonProperty("Discord_ServerID")]
        public ulong DCServerID { get; set; }

        [JsonProperty("Discord_MentionID")]
        public ulong DCMentionRoleID { get; set; }

        [JsonProperty("Discord_MentionChanID")]
        public ulong DCMentionRoleChan { get; set; }

        [JsonProperty("NextWeekUnix")]
        public long NextWeekUnix { get; set; }
        [JsonProperty("NextMonthUnix")]
        public long NextMonthUnix { get; set; }

        public DynSet()
        {
            this.WeaponDataURL = "https://gist.githubusercontent.com/root-cause/3f29d38179b12245a003fb4fff615335/raw/5713349a648f7fde716fea1b089635ab39a56019/v2944%2520Weapon%2520Data.json";
            this.DerbyVehicle = "brioso";
            this.OnfootCollision = true;
            this.AntiFallOffBike = true;
            this.AntiFallOffBikeSpeedLimit = 60;
            this.CameraSettingsUpdate = 100;
            this.Discord = true;
            this.MaxInLobby = Settings.DefaultMaxInLobby;
            this.DCServerID = 713731146072064040;
            this.DCMentionRoleID = 796386517525725224;
            this.DCMentionRoleChan = 796389588880588830;
            this.NextWeekUnix = new DateTimeOffset(DateTime.Now).AddDays(7).ToUnixTimeSeconds();
            this.NextMonthUnix = new DateTimeOffset(DateTime.Now).AddMonths(1).ToUnixTimeSeconds();
        }
    }

    public class SpawnSet
    {
        public uint ID { get; set; }
        public Vector3 Coords { get; set; }
        public float Heading { get; set; }

        public SpawnSet(uint id, float x, float y, float z, float h)
        {
            ID = id;
            Coords = new Vector3(x, y, z);
            Heading = h;
        }
    }

    public class BankRobberyData
    {
        public enum State
        {
            None = -1,
            Starting = 0,
            StealingMoney = 1,
            GettingToVehicle = 2,
            DeliveringMoney = 3,
        }
        public enum Hideout
        {
            MurrietaHeights = 1,
            BaytreeCanyonRoad = 2,
        }

        public static readonly Vector3[] Hideouts =
        {
            new Vector3(1488.5116f, -1924.0371f, 71.16769f), // Murrieta Heights
            new Vector3(824.71844f, 2193.029f, 52.32432f) // Baytree Canyon Road
        };
        public static readonly string[] HideoutNames =
        {
            "Murrieta Heights", "Baytree Canyon Road"
        };
        public static readonly object[,] FugitiveSpawns = new object[,]
        {
            { new Vector3(250.89134f, 219.42146f, 101.68343f), -48.746826f },
            { new Vector3(260.25327f, 225.47815f, 101.683266f), -108.328705f },
            { new Vector3(260.79956f, 225.62846f, 107.08325f), 113.07362f },
            { new Vector3(254.43677f, 207.87517f, 110.28294f), 11.597087f },
            { new Vector3(237.5227f, 225.21448f, 110.28275f), -147.80551f },
            { new Vector3(242.49188f, 211.0919f, 106.286865f), -3.1565855f },
            { new Vector3(234.98256f, 225.41786f, 106.28686f), -109.99029f },
            { new Vector3(246.8318f, 223.12218f, 106.286736f), 160.70934f }
        };



        public static readonly Vector3 BankCheckpointPos = new Vector3(207.33696f, 194.81523f, 105.56393f);

        public static readonly Vector3 LeaderPos = new Vector3(263.71643f, 214.61465f, 101.68339f);
        public static readonly float LeaderHeading = 252.69729f;
        public static readonly Vector3 CoLeaderPos = new Vector3(263.8176f, 212.4389f, 101.68339f);
        public static readonly float CoLeaderHeading = 161.5344f;


        public class BR_VehicleData
        {
            public Vector3 Position;
            public float Heading;
            public string Name;
            public BR_VehicleData(Vector3 pos, float head, string name)
            {
                this.Position = pos;
                this.Heading = head;
                this.Name = name;
            }
        }

        public static BR_VehicleData[] FugitiveVehicleData =
        {
            new BR_VehicleData(new Vector3(265.71365f, 69.81996f, 93.9544f), -21.32815f, "gauntlet"),
            new BR_VehicleData(new Vector3(272.52008f, 67.94361f, 93.7648f), -20.796246f, "sultan"),
            new BR_VehicleData(new Vector3(93.724525f, 163.9236f, 104.2377f), 47.72873f, "tailgater"),
            new BR_VehicleData(new Vector3(91.666145f, 159.95593f, 104.19193f), 41.80119f, "asterope"),
            new BR_VehicleData(new Vector3(359.73273f, 271.6067f, 102.900795f), -18.211304f, "burrito2"),
            new BR_VehicleData(new Vector3(364.7055f, 270.201f, 102.88683f), -23.34409f, "burrito2")
        };
        public static BR_VehicleData[] PoliceVehicleData =
        {
            new BR_VehicleData(new Vector3(-626.6653f, -395.2306f, 34.564323f), 1.5066698f, "police3"),
            new BR_VehicleData(new Vector3(-806.84973f, -85.16195f, 37.53779f), -63.391445f, "polscout"),
            new BR_VehicleData(new Vector3(-1109.0085f, 257.55493f, 64.34868f), -88.857796f, "polscout"),
            new BR_VehicleData(new Vector3(-311.73505f, -407.8586f, 29.781143f), -95.15733f, "intcept"),
            new BR_VehicleData(new Vector3(-230.17188f, -658.8686f, 33.02503f), 70.92625f, "buffalosxpol"),
            new BR_VehicleData(new Vector3(109.85292f, -595.0444f, 43.791832f), -21.20643f, "lspdunm"),
            new BR_VehicleData(new Vector3(407.90063f, -979.61176f, 29.025034f), 50.401356f, "police2"),
            new BR_VehicleData(new Vector3(416.9311f, -1024.644f, 29.113342f), 91.243935f, "bcat") // Special car - make sure it's last in array!
        };

        public List<Vehicle> FugitiveVehicles = new List<Vehicle>();
        public List<Vehicle> PoliceVehicles = new List<Vehicle>();
        public Player HeistLeader = null;
        public Player CoLeader = null;
        public State GameState = State.None;
        public Hideout HideoutLocation = Hideout.MurrietaHeights;
        public ColShape BankRobberyFinishMarker = null;
        public bool IsMoneybagDelivered = false;

        public BankRobberyData()
        {
            IsMoneybagDelivered = false;
            BankRobberyFinishMarker = null;
            HeistLeader = null;
            CoLeader = null;
            FugitiveVehicles = new List<Vehicle>();
            PoliceVehicles = new List<Vehicle>();
            GameState = State.Starting;
            HideoutLocation = Hideout.MurrietaHeights;
        }
    }

    public class Achievement
    {
        public int ID;
        public string Code;
        public string Name;
        public string Description;
        public int RewardXP;
        public int RewardPoints;
        public string Background;

        public Achievement(string Code, string Name, string Description, int RewardXP, int RewardPoints, bool custom_bg = false)
        {
            this.ID = Settings.Achievements.Count;
            this.Code = Code;
            this.Name = Name;
            this.Description = Description;
            this.RewardXP = RewardXP;
            this.RewardPoints = RewardPoints;

            if (custom_bg)
            {
                this.Background = "package://Artwork/Achievements/" + Code + ".png";
            }
            else
            {
                this.Background = "package://Artwork/Achievements/default.png";
            }

            Settings.Achievements.Add(this);
        }
    }


    public static class Settings
    {
        // Static Settings / Const
        public const string Version = "1.2";
        public const int GameRunTime = 390; // Game Running Time in Seconds before STOP
        public const int MaxLobbies = 5; // Default/starting lobby num.
        public const int DefaultMaxInLobby = 30;
        public static int MaxTeleports = 400; // Used for dimension calculations

        // Connection Settings
        public const string MySQL_Host = "161.97.73.126";
        public const string MySQL_User = "QrE2bvDp3C";
        public const string MySQL_Password = "FXF47ZKJLVHGrLSRAwFTmQEThPkQTU8W";
        public const string MySQL_Database = "racenchase";
        public const string MySQL_Port = "65517";
        public const string DCBotToken = "NjAyNTQ1Mzc0MDY1OTgzNTgw.XtaCEQ.clpqVL5c0X_38xkT-mYTr4_N9a4";

        // Global Variables (changing accordingly)
        public static bool SpawnParkedVehicles { get; set; } = false; // for debug purposes whether to spawn random cars
        public static int ParkedVehiclesToSpawn { get; set; } = 200;
        public static List<LobbyData> Lobbies { get; set; } = new List<LobbyData>();
        public static List<Player> LoggedInPlayers { get; set; } = new List<Player>();
        public static List<Vehicle> AdminVehicles { get; set; } = new List<Vehicle>();
        public static List<Achievement> Achievements = new List<Achievement>();
        public static int NextLobbyID { get; set; }
        public static int DerbyTimeLeft { get; set; }
        public static long LastDerbyTick { get; set; }
        public static long LastMessageTick { get; set; }
        public static long AutoSaveTick { get; set; }
        public static List<RequestData> Requests { get; set; } = new List<RequestData>();
        public static int NextRequestID { get; set; }
        public static long LastCountUpdate { get; set; }
        public static bool OOC { get; set; }
        public static List<LoadoutData> Loadouts { get; set; } = new List<LoadoutData>();
        public static List<CarData> Cars { get; set; }
        public static Dictionary<long, jWeaponData> WeaponData { get; set; } = new Dictionary<long, jWeaponData>();
        public static List<jCarGenData> CarGenData { get; set; } = new List<jCarGenData>();
        public static List<SpawnSet> FreeroamSpawns { get; set; } = new List<SpawnSet>();

        public static bool OOCSlowMode { get; set; } = false;

        // Dyn Settings Global
        public static DynSet Dynamics { get; set; }

        // Discord Stuff
        public static long DCMentionTime { get; set; }
        public static int DCMentionPlayers { get; set; }



        public static class Timers
        {
            public static long GameUpdate_Tick { get; set; }
            public static long TimeUpdate_Tick { get; set; }
            public static long EventUpdate_Tick { get; set; }
        }


        // Locations
        public const float LoginCameraXEnd = 1290.44897f;
        public const float LoginCameraYEnd = 478.5669f;
        public const float LoginCameraZEnd = 171.23333f;
        public const float LoginCameraRxEnd = -38.715f;
        public const float LoginCameraRyEnd = 0f;
        public const float LoginCameraRzEnd = -139.536f;
        public const float LoginCameraViewAngle = 40;

        public const float LoginCameraX = 977.969f;
        public const float LoginCameraY = 701.1895f;
        public const float LoginCameraZ = 200f;
        public const float LoginCameraRx = -8.83f;
        public const float LoginCameraRy = 0f;
        public const float LoginCameraRz = 144.452f;

        public static string[] CivilianVehicles =
        {
            "panto",
            "rhapsody",
            "club",
            "oracle",
            "washington",
            "premier",
            "cavalcade2",
            "rancherxl",
            "faggio",
            "voodoo2",
            "bjxl",
            "dubsta",
            "asea",
            "asterope",
            "stratum",
            "primo2",
            "primo",
            "glendale",
            "tourbus",
            "coach",
            "tornado4",
            "sadler",
            "towtruck",
            "scrap",
            "surfer",
            "surfer2",
            "minivan",
            "minivan2",
            "burrito",
            "burrito2",
            "burrito3",
            "burrito4",
            "camper",
            "gburrito",
            "gburrito2",
            "rumpo",
            "emperor",
            "emperor2",
            "regina",
            "romero",
            "virgo2",
            "virgo3",
            "virgo",
            "yosemite"
        };

        public static Vector3[] LobbySpawns = new Vector3[]
        {
            new Vector3(435.16357f, -983.11615f, 30.689608f),
            new Vector3(441.00525f, -976.70306f, 30.689604f),
            new Vector3(447.96674f, -987.3671f, 30.689596f),
            new Vector3(451.5091f, -982.6244f, 30.689596f),
            new Vector3(459.29898f, -985.8472f, 30.689606f),
            new Vector3(439.45166f, -994.0734f, 30.689585f),
            new Vector3(452.88248f, -985.1788f, 26.67423f),
            new Vector3(461.93222f, -990.7201f, 24.91487f),
        };

        public static object[] VectorJail = { new Vector3(2061.5889f, 2967.7412f, -67.30185f), 53f };

        public static string[] DMNames = new string[]
        {
            "Life Invader Office",
            "Warehouse (Pistol Only)",
            "Tennis Court",
            "Construction Site",
            "Diamond Casino Rooftop",
            "Humane Labs (Pistol & Shotgun Only)",
            "Morgue (Stungun Only)",
            "Server Room",
            "Yacht"
        };

        public static object[][,] DMSpawns = new object[][,]
        {
            new object[,] { // Life Invader Office
                { new Vector3(-1042.9781f, -237.05504f, 37.96496f), 112.122505f },
                { new Vector3(-1065.8978f, -246.8463f, 39.733177f), 2.7907195f },
                { new Vector3(-1087.0682f, -250.7736f, 37.76329f), -84.64264f },
                { new Vector3(-1071.687f, -240.58011f, 42.271416f), 158.59511f },
                { new Vector3(-1081.3223f, -254.02095f, 44.023148f), -37.803333f },
                { new Vector3(-1063.2289f, -248.76646f, 44.02106f), -19.056158f },
                { new Vector3(-1055.6722f, -239.3023f, 44.021183f), 124.46403f },
                { new Vector3(-1049.8112f, -242.46056f, 44.021015f), 80.71298f },
                { new Vector3(-1056.6835f, -246.28284f, 44.02107f), 60.64799f },
                { new Vector3(-1078.404f, -244.99226f, 44.021255f), -105.56132f },
                { new Vector3(-1059.8085f, -238.40387f, 39.733192f), -145.55379f },
            },
            new object[,] { // Warehouse 
                { new Vector3(1026.7279f, -3100.3984f, -38.999912f), 74.94901f },
                { new Vector3(1018.37555f, -3093.7153f, -38.999912f), 92.48154f },
                { new Vector3(1000.2626f, -3091.4524f, -38.999866f), -122.95632f },
                { new Vector3(995.9403f, -3105.2683f, -38.999886f), -129.19583f },
                { new Vector3(993.27515f, -3097.609f, -38.99586f), -87.74806f },
                { new Vector3(1009.0142f, -3105.6724f, -38.999836f), -94.19384f },
                { new Vector3(1014.16077f, -3111.6494f, -38.999855f), -16.558058f },
                { new Vector3(1023.80054f, -3101.6365f, -38.999866f), 91.27259f },
            },
            new object[,] { // Tennis Court
                { new Vector3(-1339.0612f, -86.17103f, 50.70433f), 163.59131f },
                { new Vector3(-1350.675f, -114.54801f, 50.704338f), -55.28427f },
                { new Vector3(-1339.0763f, -112.86697f, 50.704353f), 78.14612f },
                { new Vector3(-1356.3923f, -120.092705f, 50.674458f), -21.771978f },
                { new Vector3(-1376.948f, -117.589f, 50.704338f), 2.0458739f },
                { new Vector3(-1380.0442f, -89.38932f, 50.70434f), -109.858505f },
                { new Vector3(-1367.795f, -85.32934f, 50.704365f), -141.75507f },
                { new Vector3(-1363.9862f, -103.44455f, 50.704327f), -161.3273f },
                { new Vector3(-1315.6982f, -110.76596f, 47.866528f), -15.326206f },
                { new Vector3(-1302.3474f, -111.222145f, 47.86635f), 27.674273f },
                { new Vector3(-1303.0864f, -86.85428f, 47.86452f), 67.42608f },
                { new Vector3(-1313.2341f, -86.358795f, 47.86452f), -129.29897f },
            },
            new object[,] { // Construction Site
                { new Vector3(99.4386f, -374.97763f, 55.496834f), -12.512988f },
                { new Vector3(106.195435f, -375.18335f, 55.496834f), -2.4919534f },
                { new Vector3(106.47802f, -357.54053f, 55.496834f), 107.3899f },
                { new Vector3(116.39296f, -342.71448f, 55.496826f), -177.35867f },
                { new Vector3(105.71075f, -336.08627f, 55.49683f), 146.98698f },
                { new Vector3(82.705376f, -328.99182f, 55.508385f), -163.03473f },
                { new Vector3(54.689102f, -342.37216f, 55.50839f), -132.10645f },
                { new Vector3(74.776054f, -351.3653f, 55.506992f), -45.4294f },
                { new Vector3(94.19476f, -360.79895f, 55.496826f), -19.65776f },
            },
            new object[,] { // Diamond Casino Rooftop
                { new Vector3(969.83386f, 66.50592f, 112.553055f), 59.639584f },
                { new Vector3(959.83203f, 75.23554f, 111.24796f), -123.92462f },
                { new Vector3(954.08466f, 75.75135f, 113.55563f), 154.49847f },
                { new Vector3(912.97485f, 62.960815f, 111.66129f), -159.49384f },
                { new Vector3(915.492f, 0.2503112f, 111.27825f), -37.55123f },
                { new Vector3(933.18427f, 2.361477f, 111.29503f), 153.47862f },
                { new Vector3(959.95807f, 1.698037f, 111.33385f), -40.16393f },
                { new Vector3(993.58545f, 54.10783f, 111.33607f), 25.90957f },
                { new Vector3(989.6868f, 74.41087f, 111.36049f), -31.775827f },
                { new Vector3(951.07733f, 38.44182f, 112.52241f), 59.61095f },
                { new Vector3(939.3183f, 19.616312f, 112.55286f), 60.126602f },
            },
            new object[,] { // Humane Labs
                { new Vector3(3604.8674f, 3722.3547f, 29.689428f), 67.83862f },
                { new Vector3(3595.1663f, 3716.1853f, 29.689415f), 143.92741f },
                { new Vector3(3593.6687f, 3706.2952f, 29.689428f), 57.193066f },
                { new Vector3(3585.312f, 3695.9563f, 28.821373f), -43.584488f },
                { new Vector3(3604.136f, 3688.5598f, 28.82141f), 44.8802f },
                { new Vector3(3587.646f, 3681.104f, 27.621513f), -19.743713f },
                { new Vector3(3568.3704f, 3701.1924f, 28.12155f), 173.33734f },
                { new Vector3(3561.166f, 3682.017f, 28.121893f), 12.639866f },
                { new Vector3(3560.7651f, 3678.754f, 28.121876f), 148.31625f },
                { new Vector3(3559.638f, 3665.191f, 28.121876f), 79.95095f },
            },
            new object[,] { // Morgue
                { new Vector3(286.22977f, -1350.202f, 24.534641f), -45.446587f },
                { new Vector3(287.40567f, -1340.18f, 24.537807f), 136.58209f },
                { new Vector3(276.72275f, -1336.4142f, 24.537813f), -156.291f },
                { new Vector3(271.91223f, -1340.7411f, 24.537815f), -120.68168f },
                { new Vector3(250.20967f, -1352.8145f, 24.537815f), -61.26596f },
                { new Vector3(252.88127f, -1371.9757f, 24.537817f), 42.08989f },
                { new Vector3(237.52446f, -1374.1863f, 27.090235f), -31.815935f },
                { new Vector3(238.27739f, -1358.8711f, 24.537817f), -129.04114f },
                { new Vector3(268.8348f, -1363.9379f, 24.537786f), 24.815224f },
            },
            new object[,] { // Server Room 
                { new Vector3(2112.1062f, 2893.9622f, -84.793236f), 4.996611f },
                { new Vector3(2085.0032f, 2900.161f, -84.79318f), -82.84927f },
                { new Vector3(2079.1013f, 2916.622f, -84.80008f), -102.106384f },
                { new Vector3(2060.023f, 2933.967f, -84.766464f), 87.58847f },
                { new Vector3(2018.2507f, 2942.3606f, -84.79817f), 164.24449f },
                { new Vector3(1986.7706f, 2924.6992f, -84.80003f), -92.48643f },
                { new Vector3(1998.1271f, 2908.578f, -84.79333f), -101.26414f },
                { new Vector3(2017.023f, 2895.549f, -84.799995f), 13.951931f },
                { new Vector3(2110.8079f, 2907.5823f, -84.800026f), 8.491646f },
                { new Vector3(2122.5027f, 2924.5251f, -84.800026f), 68.417305f },
                { new Vector3(2128.0457f, 2945.1536f, -84.793175f), 169.32663f },
                { new Vector3(2168.6033f, 2934.917f, -84.80003f), 174.97028f },
                { new Vector3(2193.2227f, 2929.7917f, -84.80002f), -173.26201f },
                { new Vector3(2209.1692f, 2920.7107f, -84.8f), -175.40488f },
                { new Vector3(2229.5518f, 2916.9126f, -84.80003f), 83.331406f },
                { new Vector3(2251.4849f, 2933.1616f, -84.80004f), 32.401173f },
                { new Vector3(2278.5293f, 2948.9988f, -84.80003f), 174.97028f },
                { new Vector3(2297.6655f, 2916.411f, -84.793304f), 87.06708f },
                { new Vector3(2263.997f, 2891.1506f, -84.66055f), 90.80276f },
                { new Vector3(2218.9797f, 2894.7573f, -84.60675f), 79.27485f },
                { new Vector3(2172.9082f, 2915.4321f, -84.80004f), 82.91886f },
                { new Vector3(2172.6338f, 2926.562f, -84.799805f), -96.70913f },
                { new Vector3(2174.3457f, 2923.4778f, -81.07484f), 30.905758f },
                { new Vector3(2155.554f, 2921.0679f, -81.07545f), -99.31608f },
            },
            new object[,] // Yacht
            {
                { new Vector3(-2020.4397f, -1044.3085f, 2.446505f), 38.107838f },
                { new Vector3(-2035.8844f, -1034.2062f, 5.882396f), -108.74696f },
                { new Vector3(-2057.9128f, -1019.32263f, 5.882021f), -22.493902f },
                { new Vector3(-2076.1602f, -1017.83997f, 5.8841314f), -134.3238f },
                { new Vector3(-2085.8054f, -1016.5439f, 5.884128f), 82.145386f },
                { new Vector3(-2092.0054f, -1009.32764f, 5.884347f), 118.70009f },
                { new Vector3(-2105.5747f, -1011.3669f, 5.8870034f), 110.23749f },
                { new Vector3(-2125.5837f, -1004.77936f, 8.56874f), -108.86156f },
                { new Vector3(-2088.8308f, -1016.88025f, 8.971198f), 71.05865f },
                { new Vector3(-2052.3716f, -1028.8345f, 8.97149f), -108.099525f },
                { new Vector3(-2062.7554f, -1025.5779f, 11.907997f), -109.13658f },
                { new Vector3(-2082.9897f, -1018.7605f, 15.988901f), 72.777504f },
                { new Vector3(-2047.9214f, -1023.5474f, 8.971496f), -114.67136f },
            }
        };

        public static object[,] DerbySpawns = new object[,]
        {
            { new Vector3(-751.9754f, -4704.608f, 11.760467f), 1.7134262f },
            { new Vector3(-758.6151f, -4663.954f, 11.753874f), 46.039272f },
            { new Vector3(-786.2232f, -4636.954f, 11.7379265f), 45.476135f },
            { new Vector3(-826.0424f, -4630.5513f, 11.760642f), -88.72788f },
            { new Vector3(-793.56274f, -4589.758f, 11.743181f), -0.5389074f },
            { new Vector3(-821.889f, -4568.2236f, 11.756021f), -90.118774f },
            { new Vector3(-756.98834f, -4531.969f, 11.753576f), -47.8092f },
            { new Vector3(-752.3551f, -4496.236f, 11.761034f), -177.50394f },
            { new Vector3(-721.15784f, -4530.337f, 11.756191f), 177.99602f },
            { new Vector3(-690.26154f, -4498.898f, 11.758506f), 179.3546f },
            { new Vector3(-671.03217f, -4546.647f, 11.736773f), -136.62564f },
            { new Vector3(-618.71564f, -4568.229f, 11.76013f), 93.30676f },
            { new Vector3(-619.3676f, -4630.3857f, 11.750687f), 89.1567f },
            { new Vector3(-698.6009f, -4599.8916f, 35.49637f), 97.8283f },
            { new Vector3(-709.5937f, -4616.554f, 35.496002f), 58.393223f },
            { new Vector3(-737.30347f, -4612.234f, 35.49531f), -39.6135f },
            { new Vector3(-732.5698f, -4583.419f, 35.496174f), -125.14127f },
            { new Vector3(-755.71173f, -4565.5093f, 5.8608093f), -133.36496f },
            { new Vector3(-704.46185f, -4615.7827f, 5.877163f), 46.06693f },
        };
    }

    public static class Challenge
    {
        public enum ChallengeType
        {
            CopchaseKill = 0, // Items: any firearm from a loadout | Kill in copchase
            CopchaseVehicle = 1, // Items: any vehicle from copchase | Vehicle used in copchase
            AnyKill = 2, // Items: any firearm from a loadout or deathmatch | Any kill, meaning DM or CC
            CopchaseRounds = 3, // Items: N/A | How many copchase rounds played
            Win = 4, // Items: 'fugitive', 'cop', 'derby' | Amount of wins
            Arrest = 5
        };
        public static List<string> CopchaseVehicles = new List<string>();
        public static List<string> PossibleWeaponGroups = new List<string>();
        private static string[] possibleWins = { "fugitive", "cop", "derby" };

        public static void AddChallengesForPlayer(Player player)
        {
            NAPI.Util.ConsoleOutput($"{player.Name} fuck challenges");
            return;

            string desc = "";
            Account acc = EPlayer.GetAccount(player);
            List<ChallengeData> cdInfo = NAPI.Util.FromJson<List<ChallengeData>>(acc.ChallengeInfo);

            Task.Run(async () =>
            {
                bool areChallengesUpToDate = true;
                if (cdInfo.Count > 0)
                {
                    if (cdInfo[0].Expire < DateTimeOffset.Now.ToUnixTimeSeconds())
                    {
                        areChallengesUpToDate = false;
                    }
                }
                else areChallengesUpToDate = false;


                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    List<DailyChallenge> serverChallenges = await context.DailyChallenges.AsQueryable().ToListAsync();

                    if (!areChallengesUpToDate)
                    {
                        NAPI.Util.ConsoleOutput($"Challenges are not up to date for account {acc.Username}!");
                        List<ChallengeData> cdList = new List<ChallengeData>();
                        ChallengeData temp = new ChallengeData();
                        await context.DailyChallenges.AsQueryable().ForEachAsync(dc =>
                        {
                            temp = new ChallengeData();
                            temp.ChallengeItem = dc.Item;
                            temp.ChallengeType = dc.Type;
                            temp.ChallengeProgress = 0;
                            temp.Completed = false;
                            temp.Expire = dc.ExpireTimestamp;

                            cdList.Add(temp);
                        });
                        cdInfo = cdList;
                        string jData = NAPI.Util.ToJson(cdList);
                        acc.ChallengeInfo = jData;
                        NAPI.Util.ConsoleOutput($"Set up new challenges for {acc.Username} and saving their account now!");
                        EPlayer.Save(player, false);
                    }

                    NAPI.Task.Run(() =>
                    {
                        if (cdInfo.Count <= 0)
                        {
                            Task.Run(async () =>
                            {
                                using (MainDatabase context = ContextFactory.CreateContext())
                                {
                                    List<ChallengeData> cdList = new List<ChallengeData>();
                                    ChallengeData temp = new ChallengeData();
                                    await context.DailyChallenges.AsQueryable().ForEachAsync(dc =>
                                    {
                                        temp = new ChallengeData();
                                        temp.ChallengeItem = dc.Item;
                                        temp.ChallengeType = dc.Type;
                                        temp.ChallengeProgress = 0;
                                        temp.Completed = false;
                                        temp.Expire = dc.ExpireTimestamp;

                                        cdList.Add(temp);
                                    });
                                    cdInfo = cdList;
                                    string jData = NAPI.Util.ToJson(cdList);
                                    acc.ChallengeInfo = jData;
                                    EPlayer.Save(player, false);
                                }
                            });

                        }

                        player.TriggerEvent("clearDailyChallenges");

                        Task.Run(async () =>
                        {
                            using (MainDatabase context = ContextFactory.CreateContext())
                            {
                                await context.DailyChallenges.AsQueryable().ForEachAsync(async c =>
                                {
                                    NAPI.Task.Run(() =>
                                    {
                                        switch ((ChallengeType)c.Type)
                                        {
                                            case ChallengeType.AnyKill:
                                                {
                                                    string reasonDisp = "";
                                                    if (c.Item.Split("_", 2).Length > 0)
                                                        reasonDisp = c.Item.Split("_", 2)[1];
                                                    else reasonDisp = c.Item;

                                                    if (reasonDisp == "RIFLE")
                                                        reasonDisp = "(ASSAULT) RIFLE";

                                                    desc = $"Kill {c.Target} players with a weapon from the {reasonDisp} category in any mode.";
                                                }
                                                break;
                                            case ChallengeType.CopchaseKill:
                                                {
                                                    string reasonDisp = "";
                                                    if (c.Item.Split("_", 2).Length > 0)
                                                        reasonDisp = c.Item.Split("_", 2)[1];
                                                    else reasonDisp = c.Item;

                                                    if (reasonDisp == "RIFLE")
                                                        reasonDisp = "(ASSAULT) RIFLE";

                                                    desc = $"Kill {c.Target} players with a weapon from the {reasonDisp} category in copchase.";
                                                }
                                                break;
                                            case ChallengeType.CopchaseRounds:
                                                {
                                                    desc = $"Play {c.Target} rounds of copchase.";
                                                }
                                                break;
                                            case ChallengeType.CopchaseVehicle:
                                                {
                                                    CarData cdt = Settings.Cars.FirstOrDefault(t => t.Model == c.Item);
                                                    string veh = "";
                                                    if (cdt == null) { veh = c.Item; }
                                                    else veh = cdt.Name;

                                                    desc = $"Play {c.Target} copchase rounds with >> {c.Item} << set as your vehicle.";
                                                }
                                                break;
                                            case ChallengeType.Win:
                                                {
                                                    if (c.Item == "derby")
                                                        desc = $"Win {c.Target} rounds of the derby side mode.";
                                                    else
                                                    {
                                                        desc = $"Win {c.Target} copchase rounds as a {c.Item}.";
                                                    }
                                                }
                                                break;
                                            case ChallengeType.Arrest:
                                                {
                                                    desc = $"Arrest {c.Target} fugitives.";
                                                }
                                                break;
                                        }

                                        ChallengeData cd = cdInfo.FirstOrDefault(t => (t.ChallengeItem == c.Item && t.ChallengeType == c.Type));
                                        bool completed = false;
                                        if (cd != null)
                                        {
                                            completed = cd.Completed;
                                            TimeSpan sp = new TimeSpan(DateTime.Now.Ticks);
                                            double secs = Math.Ceiling(c.ExpireTimestamp - sp.TotalSeconds);
                                            // (id, desc, rXP, rPts, expireDate, completed, target, progress, type, item)

                                            player.TriggerEvent("addDailyChallenge", c.SQL_ID, desc, c.RewardXP, c.RewardPoints, secs, completed, c.Target, cd.ChallengeProgress, c.Type, c.Item);

                                        }
                                        else
                                        {
                                            TimeSpan sp = new TimeSpan(DateTime.Now.Ticks);
                                            double secs = Math.Ceiling(c.ExpireTimestamp - sp.TotalSeconds);

                                            player.TriggerEvent("addDailyChallenge", c.SQL_ID, desc, c.RewardXP, c.RewardPoints, secs, completed, c.Target, 0, c.Type, c.Item);

                                        }
                                    });
                                });
                                NAPI.Task.Run(() =>
                                {
                                    player.TriggerEvent("addChallengesToMenu", acc.ChallengeStreak, (Settings.Dynamics.NextWeekUnix - new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()));
                                });
                            }
                        });
                    });
                }
            });
        }
        public static void AddChallengesForPlayer(Player player, List<ChallengeData> cdList)
        {
            try
            {

                string desc = "";
                Account acc = EPlayer.GetAccount(player);
                List<ChallengeData> cdInfo = NAPI.Util.FromJson<List<ChallengeData>>(acc.ChallengeInfo);

                player.TriggerEvent("clearDailyChallenges");

                string jData = NAPI.Util.ToJson(cdList);
                acc.ChallengeInfo = jData;
                cdInfo = NAPI.Util.FromJson<List<ChallengeData>>(jData);
                EPlayer.Save(player, false);

                Task.Run(async () =>
                {
                    using (MainDatabase context = ContextFactory.CreateContext())
                    {
                        await context.DailyChallenges.AsQueryable().ForEachAsync(c =>
                        {
                            NAPI.Task.Run(() =>
                            {
                                switch ((ChallengeType)c.Type)
                                {
                                    case ChallengeType.AnyKill:
                                        {
                                            string reasonDisp = "";
                                            if (c.Item.Split("_", 2).Length > 0)
                                                reasonDisp = c.Item.Split("_", 2)[1];
                                            else reasonDisp = c.Item;

                                            if (reasonDisp == "RIFLE")
                                                reasonDisp = "(ASSAULT) RIFLE";

                                            desc = $"Kill {c.Target} players with a weapon from the {reasonDisp} category in any mode.";
                                        }
                                        break;
                                    case ChallengeType.CopchaseKill:
                                        {
                                            string reasonDisp = "";
                                            if (c.Item.Split("_", 2).Length > 0)
                                                reasonDisp = c.Item.Split("_", 2)[1];
                                            else reasonDisp = c.Item;

                                            if (reasonDisp == "RIFLE")
                                                reasonDisp = "(ASSAULT) RIFLE";

                                            desc = $"Kill {c.Target} players with a weapon from the {reasonDisp} category in copchase.";
                                        }
                                        break;
                                    case ChallengeType.CopchaseRounds:
                                        {
                                            desc = $"Play {c.Target} rounds of copchase.";
                                        }
                                        break;
                                    case ChallengeType.CopchaseVehicle:
                                        {
                                            CarData cdt = Settings.Cars.FirstOrDefault(t => t.Model == c.Item);
                                            string veh = "";
                                            if (cdt == null) { veh = c.Item; }
                                            else veh = cdt.Name;

                                            desc = $"Play {c.Target} copchase rounds with >> {c.Item} << set as your vehicle.";
                                        }
                                        break;
                                    case ChallengeType.Win:
                                        {
                                            if (c.Item == "derby")
                                                desc = $"Win {c.Target} rounds of the derby side mode.";
                                            else
                                            {
                                                desc = $"Win {c.Target} copchase rounds as a {c.Item}.";
                                            }
                                        }
                                        break;
                                    case ChallengeType.Arrest:
                                        {
                                            desc = $"Arrest {c.Target} fugitives.";
                                        }
                                        break;
                                }

                                ChallengeData cd = cdInfo.FirstOrDefault(t => (t.ChallengeItem == c.Item && t.ChallengeType == c.Type));
                                bool completed = false;
                                if (cd != null)
                                {
                                    completed = cd.Completed;
                                    TimeSpan sp = new TimeSpan(DateTime.Now.Ticks);
                                    double secs = Math.Ceiling(c.ExpireTimestamp - sp.TotalSeconds);
                                    // (id, desc, rXP, rPts, expireDate, completed, target, progress, type, item)

                                    player.TriggerEvent("addDailyChallenge", c.SQL_ID, desc, c.RewardXP, c.RewardPoints, secs, completed, c.Target, cd.ChallengeProgress, c.Type, c.Item);

                                }
                                else
                                {
                                    TimeSpan sp = new TimeSpan(DateTime.Now.Ticks);
                                    double secs = Math.Ceiling(c.ExpireTimestamp - sp.TotalSeconds);

                                    player.TriggerEvent("addDailyChallenge", c.SQL_ID, desc, c.RewardXP, c.RewardPoints, secs, completed, c.Target, 0, c.Type, c.Item);

                                }
                            });

                        });
                        NAPI.Task.Run(() =>
                        {
                            player.TriggerEvent("addChallengesToMenu", acc.ChallengeStreak, (Settings.Dynamics.NextWeekUnix - new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()));
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RnC Challenges] Error when adding channelges for player: {ex.ToString()}");
            }


        }
        public async static void SetNewDailyChallenges()
        {
            using (MainDatabase context = ContextFactory.CreateContext())
            {
                Random randomizer = new Random();

                int cntr = 0;
                int succ = 0;

                context.DailyChallenges.RemoveRange(context.DailyChallenges);
                await context.SaveChangesAsync();

                DateTime expiry = DateTime.Now.AddHours(24);
                TimeSpan sp = new TimeSpan(expiry.Ticks);
                long expireInSeconds = (long)Math.Ceiling(sp.TotalSeconds);

                List<KeyValuePair<int, string>> generatedChallenges = new List<KeyValuePair<int, string>>();

                int generatedType = 1;
                string generatedItem = "police";
                int target = 20;
                int rXp = 10, rPt = 4;

                while (await context.DailyChallenges.AsQueryable().CountAsync() < 10)
                {
                    if (succ >= 10) break;

                    cntr++;
                    if (cntr >= 500)
                    {
                        NAPI.Task.Run(() =>
                        {
                            NAPI.Util.ConsoleOutput("(RnC | CHALLENGE) Reached maximum loop value in SetNewDailyChallenges! Breaking it... (500)");
                        });
                        break;
                    }

                    generatedType = randomizer.Next(0, 6);
                    switch ((ChallengeType)generatedType)
                    {
                        case ChallengeType.CopchaseKill:
                            {
                                List<string> ccWeps = PossibleWeaponGroups;
                                ccWeps.RemoveAll(lol => lol.Contains("SNIPER", StringComparison.OrdinalIgnoreCase));

                                generatedItem = ccWeps[randomizer.Next(ccWeps.Count)];

                                target = randomizer.Next(20, 40);
                                if (target % 5 != 0)
                                    target = (5 - target % 5) + target; // rounding to upper multiply of 5

                                rXp = (int)Math.Floor((decimal)target / 4);
                                rPt = (int)Math.Floor((decimal)target / 6);
                            }
                            break;
                        case ChallengeType.CopchaseVehicle:
                            {
                                generatedItem = CopchaseVehicles[randomizer.Next(CopchaseVehicles.Count)];

                                target = randomizer.Next(20, 36);
                                if (target % 5 != 0)
                                    target = (5 - target % 5) + target;

                                rXp = (int)Math.Floor((decimal)target / 4);
                                rPt = (int)Math.Floor((decimal)target / 6);
                            }
                            break;
                        case ChallengeType.AnyKill:
                            {
                                List<string> anyWeps = PossibleWeaponGroups;
                                anyWeps.Add("GROUP_SNIPER");
                                generatedItem = anyWeps[randomizer.Next(anyWeps.Count)];

                                target = randomizer.Next(50, 100);
                                if (target % 5 != 0)
                                    target = (5 - target % 5) + target;

                                rXp = (int)Math.Floor((decimal)target / 4);
                                rPt = 0;
                            }
                            break;
                        case ChallengeType.CopchaseRounds:
                            {
                                generatedItem = "N/A";

                                target = randomizer.Next(26, 42);
                                if (target % 5 != 0)
                                    target = (5 - target % 5) + target;

                                rXp = (int)Math.Floor((decimal)target / 4);
                                rPt = (int)Math.Floor((decimal)target / 6);
                            }
                            break;
                        case ChallengeType.Win:
                            {
                                generatedItem = possibleWins[randomizer.Next(possibleWins.Length)];

                                target = randomizer.Next(5, 19);
                                if (target % 5 != 0)
                                    target = (5 - target % 5) + target;

                                rXp = target;
                                rPt = (int)Math.Floor((decimal)target / 2);
                            }
                            break;
                        case ChallengeType.Arrest:
                            {
                                generatedItem = "N/A";

                                target = randomizer.Next(11, 24);
                                if (target % 5 != 0)
                                    target = (5 - target % 5) + target;

                                rXp = (int)Math.Floor((decimal)target / 2);
                                rPt = (int)Math.Floor((decimal)target / 3);
                            }
                            break;
                    }
                    KeyValuePair<int, string> check = new KeyValuePair<int, string>(generatedType, generatedItem);
                    bool exists = generatedChallenges.Exists(c => c.Key == check.Key && c.Value == check.Value);
                    if (exists == false)
                    {
                        generatedChallenges.Add(check);

                        DailyChallenge c = new DailyChallenge()
                        {
                            Type = generatedType,
                            Target = target,
                            Item = generatedItem,
                            ExpireTimestamp = expireInSeconds,
                            RewardPoints = rPt,
                            RewardXP = rXp
                        };
                        await context.DailyChallenges.AddAsync(c);
                        succ++;
                        continue;
                    }
                    else
                    {
                        continue;
                    }

                }

                await context.SaveChangesAsync();

                List<ChallengeData> cdList = new List<ChallengeData>();
                ChallengeData temp = new ChallengeData();
                await context.DailyChallenges.AsQueryable().ForEachAsync(dc =>
                {
                    temp = new ChallengeData();
                    temp.ChallengeItem = dc.Item;
                    temp.ChallengeType = dc.Type;
                    temp.ChallengeProgress = 0;
                    temp.Completed = false;
                    temp.Expire = dc.ExpireTimestamp;

                    cdList.Add(temp);
                });
                /*
                string jData = NAPI.Util.ToJson(cdList);
                foreach(Account acc in context.Accounts)
                {
                    try { acc.ChallengeInfo = jData; } catch { }
                }
                */

                int cCount = await context.DailyChallenges.AsQueryable().CountAsync();
                Console.WriteLine("[RnC CHALLENGES] Generated new challenges, count: " + cCount);
                NAPI.Task.Run(() =>
                {
                    foreach (Player pl in Settings.LoggedInPlayers)
                    {
                        AddChallengesForPlayer(pl, cdList);
                    }
                });
                //await context.SaveChangesAsync();
            }
        }
    }

    public class Main : Script
    {
        static string Translate(Player player, string message_code, params object[] args)
        {
            return Translator.Translate(player, message_code, args);
        }

        public static void DisplayMOTD(Player target, MOTDData motd)
        {
            target.SendChatMessage("!{#2B82FF}========================================");
            target.SendChatMessage("!{#2B82FF}MOTD: !{#FFFFFF}" + motd.Message);
            target.SendChatMessage("!{#2B82FF}Posted by !{#FF9100}" + motd.AdminName + " !{#2B82FF}on !{#FF9100}" + motd.PostedOn);
            target.SendChatMessage("!{#2B82FF}========================================");

            EPlayer.GetAccount(target).MOTD = "none"; // auto-clear the motd if they read it
        }

        public static bool BlacklistedClothing(int slot, int drawable)
        {
            if (slot == 0) // Hats
            {
                if ((drawable >= 16 && drawable <= 18) || drawable == 39 || (drawable >= 47 && drawable <= 53) ||
                       drawable == 38 || (drawable >= 66 && drawable <= 75) || (drawable >= 77 && drawable <= 82) ||
                       (drawable >= 83 && drawable <= 94) || drawable == 111 || (drawable >= 114 && drawable <= 119) ||
                       (drawable >= 125 && drawable <= 128) || drawable == 144 || drawable == 150 || drawable == 112 || drawable == 143 || drawable == 149 || drawable == 124)
                {
                    return true; // (Blocks: Unfair headshot blocking helmets)
                }
            }
            return false;
        }
        public static int GetMaxSeats(string vehiclename)
        {
            int seats = NAPI.Vehicle.GetVehicleMaxOccupants((VehicleHash)NAPI.Util.GetHashKey(vehiclename));

            switch (vehiclename)
            {
                case "cheburek":
                    seats = 4;
                    break;
                case "blista2":
                    seats = 2;
                    break;
                case "police2":
                    seats = 4;
                    break;
                case "police3":
                    seats = 4;
                    break;
                case "police4":
                    seats = 4;
                    break;
                case "police":
                    seats = 4;
                    break;
                case "lspdvic":
                    seats = 4;
                    break;
                case "polbufac":
                    seats = 4;
                    break;
                case "fbibufac":
                    seats = 4;
                    break;
                case "sultan2":
                    seats = 4;
                    break;
                case "lspdunm":
                    seats = 4;
                    break;
                case "prevolter":
                    seats = 4;
                    break;
                case "m3e46":
                    seats = 2;
                    break;
                case "dominator8":
                    seats = 2;
                    break;
                case "impala":
                    seats = 4;
                    break;
                case "pscout":
                    seats = 4;
                    break;
                case "umkscout":
                    seats = 4;
                    break;
                case "tahoma":
                    seats = 4;
                    break;
                case "futo2":
                    seats = 2;
                    break;
                case "seminole2":
                    seats = 4;
                    break;
                case "huntley2":
                    seats = 4;
                    break;

                case "buffalosx":
                    seats = 4;
                    break;
                case "buffalosxpol":
                    seats = 4;
                    break;
                case "cayenneturbo":
                    seats = 4;
                    break;
                case "pursport":
                    seats = 2;
                    break;

                case "polalamor":
                    seats = 4;
                    break;
                case "polbisonr":
                    seats = 4;
                    break;
                case "polbuffalor":
                    seats = 4;
                    break;
                case "polbuffalor2":
                    seats = 4;
                    break;
                case "polfugitiver":
                    seats = 4;
                    break;
                case "poltorencer":
                    seats = 4;
                    break;
                case "polgauntler":
                    seats = 2;
                    break;
                case "polvigeror":
                    seats = 2;
                    break;

                case "tailgater2":
                    seats = 4;
                    break;

                case "zr350":
                    seats = 2;
                    break;
                case "shegrangerold":
                    seats = 4;
                    break;
                case "shergauntlet":
                    seats = 2;
                    break;
                case "polbisonf":
                    seats = 4;
                    break;
                case "gresleyh":
                    seats = 4;
                    break;
                case "tulip2":
                    seats = 2;
                    break;
                case "golfmk6":
                    seats = 4;
                    break;
                case "centurionlspd":
                    seats = 8;
                    break;
                case "bcso2slick":
                    seats = 4;
                    break;
                case "police3slick":
                    seats = 4;
                    break;
                case "police3umk":
                    seats = 4;
                    break;
                case "fibexecutioner":
                    seats = 7;
                    break;
                case "bcat":
                    seats = 8;
                    break;

                case "omnis":
                    seats = 2;
                    break;
                case "primo3":
                    seats = 4;
                    break;
                case "landroamer":
                    seats = 4;
                    break;
                case "altior":
                    seats = 4;
                    break;
                case "esperanto2d":
                    seats = 2;
                    break;
            }
            return seats;
        }

        public class StaticBadgeData
        {
            public int SQL_ID { get; set; }
            public string Type { get; set; }
            public int Value { get; set; }
            public string Image { get; set; }
            public StaticBadgeData(int id, string type, int value, string image)
            {
                SQL_ID = id;
                Type = type;
                Value = value;
                Image = image;
            }
        }
        // For determining which badge to fetch based on player's Admin and Donator level.
        public static StaticBadgeData[] StaticBadges = new StaticBadgeData[]
        {
            new StaticBadgeData(1, "Admin", 6, "management.png"),
            new StaticBadgeData(2, "Admin", 5, "lead-admin.png"),
            new StaticBadgeData(3, "Admin", 4, "senior-admin.png"),
            new StaticBadgeData(4, "Admin", 3, "general-admin.png"),
            new StaticBadgeData(5, "Admin", 2, "trial-admin.png"),
            new StaticBadgeData(6, "Admin", 1, "moderator.png"),
            new StaticBadgeData(7, "Donator", 1, "bronze-donator.png"),
            new StaticBadgeData(8, "Donator", 2, "silver-donator.png"),
            new StaticBadgeData(9, "Donator", 3, "gold-donator.png"),
        };
        public static StaticBadgeData GetStaticBadgeData(string type, int level)
        {
            StaticBadgeData _d = StaticBadges.FirstOrDefault(b => b.Type == type && b.Value == level);
            return _d;
        }
        public class SelectedBadgeData
        {
            public int id;
            public string name;
            public string image;
            public SelectedBadgeData(int id, string name, string image)
            {
                this.id = id;
                this.name = name;
                this.image = image;
            }
        }

        public static IDictionary<long, string> WeaponReasons = new Dictionary<long, string>()
        {
            {2460120199, "Antique Cavalry Dagger"},
            {2508868239, "Baseball Bat"},
            {4192643659, "Bottle"},
            {2227010557, "Crowbar"},
            {2725352035, "Fist"},
            {2343591895, "Flashlight"},
            {1141786504, "Golf Club"},
            {1317494643, "Hammer"},
            {4191993645, "Hatchet"},
            {3638508604, "Knuckle"},
            {2578778090, "Knife"},
            {3713923289, "Machete"},
            {3756226112, "Switchblade"},
            {1737195953, "Nightstick"},
            {419712736, "Pipe Wrench"},
            {3441901897, "Battle Axe"},
            {2484171525, "Pool Cue"},
            {940833800, "Stone Hatchet"},
            {453432689, "Pistol"},
            {3219281620, "Pistol MK2"},
            {1593441988, "Combat Pistol"},
            {584646201, "AP Pistol"},
            {911657153, "Stun Gun"},
            {2578377531, "Pistol .50"},
            {3218215474, "SNS Pistol"},
            {2285322324, "SNS Pistol MK2"},
            {3523564046, "Heavy Pistol"},
            {2636060646, "Military Rifle"},
            {3347935668, "Heavy Rifle"},
            {137902532, "Vintage Pistol"},
            {1198879012, "Flare Gun"},
            {465894841, "Pistol XM3" },
            {3696079510, "Marksman Pistol"},
            {3249783761, "Heavy Revolver"},
            {3415619887, "Heavy Revolver MK2"},
            {2548703416, "Double Action"},
            {2441047180, "Navy Revolver" },
            {2939590305, "Up-n-Atomizer"},
            {324215364, "Micro SMG"},
            {736523883, "SMG"},
            {2024373456, "SMG MK2"},
            {4024951519, "Assault SMG"},
            {171789620, "Combat PDW"},
            {3675956304, "Machine Pistol"},
            {3173288789, "Mini SMG"},
            {1198256469, "Unholy Hellbringer"},
            {487013001, "Pump Shotgun"},
            {1432025498, "Pump Shotgun MK2"},
            {2017895192, "Sawed-Off Shotgun"},
            {3800352039, "Assault Shotgun"},
            {2640438543, "Bullpup Shotgun"},
            {2828843422, "Musket"},
            {984333226, "Heavy Shotgun"},
            {4019527611, "Double Barrel Shotgun"},
            {317205821, "Sweeper Shotgun"},
            {3220176749, "Assault Rifle"},
            {961495388, "Assault Rifle MK2"},
            {2210333304, "Carbine Rifle"},
            {4208062921, "Carbine Rifle MK2"},
            {2937143193, "Advanced Rifle"},
            {3231910285, "Special Carbine"},
            {2526821735, "Special Carbine MK2"},
            {2132975508, "Bullpup Rifle"},
            {2228681469, "Bullpup Rifle MK2"},
            {1649403952, "Compact Rifle"},
            {2634544996, "MG"},
            {2144741730, "Combat MG"},
            {3686625920, "Combat MG MK2"},
            {1627465347, "Gusenberg Sweeper"},
            {100416529, "Sniper Rifle"},
            {205991906, "Heavy Sniper"},
            {177293209, "Heavy Sniper MK2"},
            {1853742572, "Precision Rifle" },
            {3342088282, "Marksman Rifle"},
            {1785463520, "Marksman Rifle MK2"},
            {2982836145, "RPG"},
            {2726580491, "Grenade Launcher"},
            {1305664598, "Smoke Grenade Launcher"},
            {1119849093, "Minigun"},
            {2138347493, "Firework Launcher"},
            {1834241177, "Railgun"},
            {1672152130, "Homing Launcher"},
            {125959754, "Compact Grenade Launcher"},
            {3056410471, "Ray Minigun"},
            {2481070269, "Grenade"},
            {2694266206, "BZ Gas"},
            {4256991824, "Smoke Grenade"},
            {1233104067, "Flare"},
            {615608432, "Molotov"},
            {3520460075, "Service Rifle"},
            {94989220, "Combat Shotgun"},
            {350597077, "Tactical SMG"},
            {741814745, "Sticky Bomb"},
            {2874559379, "Proximity Mine"},
            {126349499, "Snowball"},
            {3125143736, "Pipe Bomb"},
            {600439132, "Baseball"},
            {883325847, "Jerry Can"},
            {101631238, "Fire Extinguisher"},
            {4222310262, "Parachute"},
            {2461879995, "Electric Fence"},
            {3425972830, "Hit by Water Cannon"},
            {133987706, "Rammed by Car"},
            {2741846334, "Run Over by Car"},
            {3452007600, "Fall"},
            {4194021054, "Animal"},
            {324506233, "Airstrike Rocket"},
            {2339582971, "Bleeding"},
            {2294779575, "Briefcase"},
            {28811031, "Briefcase 02"},
            {148160082, "Cougar"},
            {1223143800, "Barbed Wire"},
            {4284007675, "Drowning"},
            {1936677264, "Drowning In Vehicle"},
            {539292904, "Explosion"},
            {910830060, "Exhaustion"},
            {3750660587, "Fire"},
            {341774354, "Heli Crash"},
            {3204302209, "Vehicle Rocket"},
            {2282558706, "Vehicle Akula Barrage"},
            {431576697, "Vehicle Akula Minigun"},
            {2092838988, "Vehicle Akula Missile"},
            {476907586, "Vehicle Akula Turret Dual"},
            {3048454573, "Vehicle Akula Turret Single"},
            {328167896, "Vehicle APC Cannon"},
            {190244068, "Vehicle APC MG"},
            {1151689097, "Vehicle APC Missile"},
            {3293463361, "Vehicle Ardent MG"},
            {2556895291, "Vehicle Avenger Cannon"},
            {2756453005, "Vehicle Barrage Rear GL"},
            {1200179045, "Vehicle Barrage Rear MG"},
            {525623141, "Vehicle Barrage Rear Minigun"},
            {4148791700, "Vehicle Barrage Top MG"},
            {1000258817, "Vehicle Barrage Top Minigun"},
            {3628350041, "Vehicle Bombushka Cannon"},
            {741027160, "Vehicle Bombushka Dual MG"},
            {3959029566, "Vehicle Cannon Blazer"},
            {1817275304, "Vehicle Caracara MG"},
            {1338760315, "Vehicle Caracara Minigun"},
            {2722615358, "Vehicle Cherno Missile"},
            {3936892403, "Vehicle Comet MG"},
            {2600428406, "Vehicle Deluxo MG"},
            {3036244276, "Vehicle Deluxo Missile"},
            {1595421922, "Vehicle Dogfighter MG"},
            {3393648765, "Vehicle Dogfighter Missile"},
            {2700898573, "Vehicle Dune Grenade Launcher"},
            {3507816399, "Vehicle Dune MG"},
            {1416047217, "Vehicle Dune Minigun"},
            {1566990507, "Vehicle Enemy Laser"},
            {1987049393, "Vehicle Hacker Missile"},
            {2011877270, "Vehicle Hacker Missile Homing"},
            {1331922171, "Vehicle Halftrack Dual MG"},
            {1226518132, "Vehicle Halftrack Quad MG"},
            {855547631, "Vehicle Havok Minigun"},
            {785467445, "Vehicle Hunter Barrage"},
            {704686874, "Vehicle Hunter Cannon"},
            {1119518887, "Vehicle Hunter MG"},
            {153396725, "Vehicle Hunter Missile"},
            {2861067768, "Vehicle Insurgent Minigun"},
            {507170720, "Vehicle Khanjali Cannon"},
            {2206953837, "Vehicle Khanjali Cannon Heavy"},
            {394659298, "Vehicle Khanjali GL"},
            {711953949, "Vehicle Khanjali MG"},
            {3754621092, "Vehicle Menacer MG"},
            {3303022956, "Vehicle Microlight MG"},
            {3846072740, "Vehicle Mobileops Cannon"},
            {3857952303, "Vehicle Mogul Dual Nose"},
            {3123149825, "Vehicle Mogul Dual Turret"},
            {4128808778, "Vehicle Mogul Nose"},
            {3808236382, "Vehicle Mogul Turret"},
            {2220197671, "Vehicle Mule4 MG"},
            {1198717003, "Vehicle Mule4 Missile"},
            {3708963429, "Vehicle Mule4 Turret GL"},
            {2786772340, "Vehicle Nightshark MG"},
            {1097917585, "Vehicle Nose Turret Valkyrie"},
            {3643944669, "Vehicle Oppressor MG"},
            {2344076862, "Vehicle Oppressor Missile"},
            {3595383913, "Vehicle Oppressor2 Cannon"},
            {3796180438, "Vehicle Oppressor2 MG"},
            {1966766321, "Vehicle Oppressor2 Missile"},
            {3473446624, "Vehicle Plane Rocket"},
            {1186503822, "Vehicle Player Buzzard"},
            {3800181289, "Vehicle Player Lazer"},
            {1638077257, "Vehicle Player Savage"},
            {2456521956, "Vehicle Pounder2 Barrage"},
            {2467888918, "Vehicle Pounder2 GL"},
            {2263283790, "Vehicle Pounder2 Mini"},
            {162065050, "Vehicle Pounder2 Missile"},
            {3530961278, "Vehicle Radar"},
            {3177079402, "Vehicle Revolter MG"},
            {3878337474, "Vehicle Rogue Cannon"},
            {158495693, "Vehicle Rogue MG"},
            {1820910717, "Vehicle Rogue Missile"},
            {50118905, "Vehicle Ruiner Bullet"},
            {84788907, "Vehicle Ruiner Rocket"},
            {3946965070, "Vehicle Savestra MG"},
            {231629074, "Vehicle Scramjet MG"},
            {3169388763, "Vehicle Scramjet Missile"},
            {1371067624, "Vehicle Seabreeze MG"},
            {3450622333, "Vehicle Searchlight"},
            {4171469727, "Vehicle Space Rocket"},
            {3355244860, "Vehicle Speedo4 MG"},
            {3595964737, "Vehicle Speedo4 Turret MG"},
            {2667462330, "Vehicle Speedo4 Turret Mini"},
            {968648323, "Vehicle Strikeforce Barrage"},
            {955522731, "Vehicle Strikeforce Cannon"},
            {519052682, "Vehicle Strikeforce Missile"},
            {1176362416, "Vehicle Subcar MG"},
            {3565779982, "Vehicle Subcar Missile"},
            {3884172218, "Vehicle Subcar Torpedo"},
            {1744687076, "Vehicle Tampa Dual Minigun"},
            {3670375085, "Vehicle Tampa Fixed Minigun"},
            {2656583842, "Vehicle Tampa Missile"},
            {1015268368, "Vehicle Tampa Mortar"},
            {1945616459, "Vehicle Tank"},
            {3683206664, "Vehicle Technical Minigun"},
            {1697521053, "Vehicle Thruster MG"},
            {1177935125, "Vehicle Thruster Missile"},
            {2156678476, "Vehicle Trailer Dualaa"},
            {341154295, "Vehicle Trailer Missile"},
            {1192341548, "Vehicle Trailer Quad MG"},
            {2966510603, "Vehicle Tula Dual MG"},
            {1217122433, "Vehicle Tula MG"},
            {376489128, "Vehicle Tula Minigun"},
            {1100844565, "Vehicle Tula Nose MG"},
            {3041872152, "Vehicle Turret Boxville"},
            {1155224728, "Vehicle Turret Insurgent"},
            {729375873, "Vehicle Turret Limo"},
            {2144528907, "Vehicle Turret Technical"},
            {2756787765, "Vehicle Turret Valkyrie"},
            {4094131943, "Vehicle Vigilante MG"},
            {1347266149, "Vehicle Vigilante Missile"},
            {2275421702, "Vehicle Viseris MG"},
            {1150790720, "Vehicle Volatol Dual MG"},
            {1741783703, "Vehicle Water Cannon"}
        };

        public static string WeatherToForecast(int weatherid)
        {
            string weatherString = $"To be added. {weatherid}";
            switch (weatherid)
            {
                case 0:
                    weatherString = "The weather is reported to soon be sunny and the atmosphere feels warm.";
                    break;
                case 1:
                    weatherString = "The weather is reported to be clear.";
                    break;
                case 2:
                    weatherString = "The atmosphere is cloudy and cold, high chances of rain.";
                    break;
                case 3:
                    weatherString = "There are chances for a smoggy weather according to reports.";
                    break;
                case 4:
                    weatherString = "Dense fog tends to collect in the atmosphere, resulting in a low visibility.";
                    break;
                case 5:
                    weatherString = "A huge overcast has been reported over the sky.";
                    break;
                case 6:
                    weatherString = "The sky gets darker, cloudy and starts raining, it is advised to drive safe and slow.";
                    break;
                case 7:
                    weatherString = "As the sky roars, a thunderstorm takes place. It is advised to stay indoors.";
                    break;
                case 8:
                    weatherString = "The rainy atmosphere starts to clear and dark clouds fade.";
                    break;
                case 9:
                    weatherString = "The sky appears to be foggy, as snowflakes start to fall down.";
                    break;
                default:
                    weatherString = "The atmosphere is clear.";
                    break;
            }
            return weatherString;
        }
        public static string ConvertGameStatus(int status, bool colored = false)
        {
            string ret = "Waiting";
            if (colored) ret = "!{#BD3831}" + ret;
            switch (status)
            {
                case 0:
                    break;
                case 1:
                    ret = "Running";
                    if (colored) ret = "!{#28993E}" + ret;
                    break;
                case 2:
                    ret = "Starting";
                    if (colored) ret = "!{#00FF33}" + ret;
                    break;
            }
            return ret;
        }

        public static string ConvertGameMode(object mode, bool colored = false)
        {
            string ret = "Unknown";
            if (colored) ret = "!{#FF0000}" + ret;
            switch (mode)
            {
                case GameModes.STANDARD:
                    ret = "Standard";
                    if (colored) ret = "!{#FFFFFF}" + ret;
                    break;
                case GameModes.BUSTED:
                    ret = "Busted";
                    if (colored) ret = "!{#3569DB}" + ret;
                    break;
                case GameModes.BANKROBBERY:
                    ret = "Bank Robbery";
                    if (colored) ret = "!{#DB6135}" + ret;
                    break;
            }
            return ret;
        }

        public static void Log(string fileName, string msg)
        {
            var writer = new System.IO.StreamWriter($"racenchase/logs/{fileName}", true);

            writer.WriteLine(msg);
            writer.Flush();
            writer.Close();
            return;
        }

        public static void SaveDynamicSettings()
        {
            RAGE.Entities.Players.All.ForEach(pl =>
            {
                EPlayer.SyncServerSettings(pl);
            });

            try
            {
                if (File.Exists("racenchase/settings.json")) File.Delete("racenchase/settings.json");

                File.WriteAllText("racenchase/settings.json", NAPI.Util.ToJson(Settings.Dynamics));
            }
            catch { NAPI.Util.ConsoleOutput("(RnC | SETTINGS) Failed to save dynamic settings!"); }

            return;
        }

        public static void LoadDynamicSettings()
        {
            try
            {
                if (File.Exists("racenchase/settings.json"))
                {
                    Settings.Dynamics = new DynSet();
                    Settings.Dynamics = NAPI.Util.FromJson<DynSet>(File.ReadAllText("racenchase/settings.json"));
                }
            }
            catch { NAPI.Util.ConsoleOutput("(RnC | Settings) Failed to read settings.json, permission problem or format?"); }

            return;
        }

        [ServerEvent(Event.ResourceStop)]
        public void OnResourceStop()
        {
            ContextFactory.SaveDB();

            RAGE.Entities.Players.All.ForEach((Player p) =>
            {
                p.SendChatMessage("!{#FFFFFF}(( !{#FF6347}Server is stopping... !{#FFFFFF}))");
                p.SendChatMessage("!{#A9C4E4}Server closed the connection.");

                EPlayer.Save(p);
                EPlayer.Freeze(p, true);
                EPlayer.Handcuff(p, true);
                p.Position = Vector3.RandomXy();
                EPlayer.Notify(p, "Goodbye", "The server is going offline!");

                p.KickSilent(); // Sends a "timeout" type disconnection so they can reconnect.

            });

            NAPI.Util.ConsoleOutput("Server saving & exiting...");
        }

        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            NAPI.Util.ConsoleOutput("(RnC | STARTUP) Ensuring 'racenchase' folder...");
            System.IO.Directory.CreateDirectory("racenchase");

            NAPI.Util.ConsoleOutput("(RnC | STARTUP) Ensuring logs folder...");
            System.IO.Directory.CreateDirectory("racenchase/logs");

            NAPI.Util.ConsoleOutput("(RnC | STARTUP) ===== Heading towards second stage of startup =====");

            NAPI.Server.SetAutoRespawnAfterDeath(false);
            NAPI.Server.SetAutoSpawnOnConnect(false);
            NAPI.Server.SetGlobalServerChat(false);
            NAPI.Server.SetCommandErrorMessage("!{#FF6347}[!] !{#FFFFFF}This command is invalid, refer to /help for a list.");

            NAPI.Util.ConsoleOutput($"(RnC | STARTUP) Connecting to database & backup database => User {Settings.MySQL_User} for db {Settings.MySQL_Database} on host {Settings.MySQL_Host}");
            ContextFactory.Instance.Database.EnsureCreated();
            BackupContextFactory.Instance.Database.EnsureCreated();
            NAPI.Util.ConsoleOutput("(RnC | STARTUP) Connected to database.");

            Settings.Lobbies = new List<LobbyData>();
            Settings.Requests = new List<RequestData>();
            Settings.LoggedInPlayers = new List<Player>();
            Settings.AdminVehicles = new List<Vehicle>();
            Settings.Loadouts = new List<LoadoutData>();
            Settings.Cars = new List<CarData>();
            Settings.FreeroamSpawns = new List<SpawnSet>();

            Settings.WeaponData = new Dictionary<long, jWeaponData>();
            Settings.CarGenData = new List<jCarGenData>();

            Settings.Dynamics = new DynSet();

            Settings.NextLobbyID = 1;
            Settings.NextRequestID = 1;
            Settings.LastCountUpdate = Environment.TickCount64;
            Settings.AutoSaveTick = Environment.TickCount64;
            Settings.LastMessageTick = Environment.TickCount64;
            Settings.OOC = true;

            Settings.DCMentionTime = Environment.TickCount64 - 900000;
            Settings.DCMentionPlayers = 0;

            NAPI.World.SetTime(12, 0, 0);
            NAPI.World.SetWeather(Weather.CLOUDS);

            for (int i = 0; i < Settings.MaxLobbies; i++)
            {
                if (i >= Settings.MaxLobbies - 1) new LobbyData().gameMode = GameModes.BUSTED; // Some busted lobby too.
                else new LobbyData();
            }

            // This will initialize all the event lobbies and such.
            EventManager.Initialize();


            NAPI.Util.ConsoleOutput("(RnC | STARTUP) Ensuring & loading settings.json...");

            LoadDynamicSettings();

            if (!File.Exists("racenchase/weapondata.json"))
            {
                NAPI.Util.ConsoleOutput("(RnC | STARTUP) Updating weapondata.json...");
                try
                {
                    using (var client = new WebClient())
                    {
                        client.DownloadFile(Settings.Dynamics.WeaponDataURL, "racenchase/weapondata.json.tmp");
                    }

                    try
                    {
                        if (File.Exists("racenchase/weapondata.json"))
                        {
                            File.Delete("racenchase/weapondata.json");
                        }
                    }
                    catch { NAPI.Util.ConsoleOutput("(RnC | STARTUP) Failed to clear old weapondata.json, permission problem?"); }

                    try
                    {
                        File.Move("racenchase/weapondata.json.tmp", "racenchase/weapondata.json");
                    }
                    catch { NAPI.Util.ConsoleOutput("(RnC| STARTUP) Failed to update weapondata.json! (Complete final version, 'move' fail)"); }

                }
                catch { NAPI.Util.ConsoleOutput("(RnC | STARTUP) [Warning] Failed to update weapondata.json (not critical, using old instead, note that server will crash without one.)"); }
            }

            NAPI.Util.ConsoleOutput("(RnC | STARTUP) Created and initialised settings & enums.");

            NAPI.Util.ConsoleOutput("(RnC | STARTUP) ===== Heading towards final stage of startup =====");

            GenerateRedeemableCodes();

            try
            {
                foreach (KeyValuePair<long, jWeaponData> item in jWeaponData.FromJson(File.ReadAllText("racenchase/weapondata.json")))
                {
                    Settings.WeaponData.Add(item.Key, item.Value);
                }
            }
            catch { NAPI.Util.ConsoleOutput("(RnC | STARTUP) Unable to load in weapondata.json! Server will work but it can cause crashes due to component data."); }
            NAPI.Util.ConsoleOutput($"(RnC | STARTUP) Loading in weapondata.json... ({Settings.WeaponData.Count} weapons/entries)");

            try
            {
                foreach (jCarGenData item in jCarGenData.FromJson(File.ReadAllText("racenchase/CarGens_NoZoneVehicles.json")))
                {
                    Settings.CarGenData.Add(item);
                }
            }
            catch { NAPI.Util.ConsoleOutput("(RnC | STARTUP) Unable to load in CarGens_NoZoneVehicles.json! Server will work but it can cause crashes due to component data."); }
            NAPI.Util.ConsoleOutput($"(RnC | STARTUP) Loading in CarGens_NoZoneVehicles.json... ({Settings.CarGenData.Count} vehicle spawns/entries)");

            /* ACHIEVEMENTS CREATION */
            NAPI.Util.ConsoleOutput("(RnC | STARTUP) Creating the achievements...");

            new Achievement("discord_verify", "Social Butterfly", "Link your Discord account to your RnC account. (/link)", 0, 0, true);
            new Achievement("first_derby", "Derby Master?", "Win a round in the Derby mode.", 4, 3, true);
            new Achievement("first_kill", "First Kill", "Get your first kill in copchase.", 4, 2, true);
            new Achievement("first_escape", "You'll never catch me!", "Escape as a fugitive in copchase.", 5, 2, true);
            new Achievement("first_arrest", "Caught Alive", "Arrest a fugitive in copchase.", 5, 2, true);
            new Achievement("100_arrests", "Where's my promotion?", "Arrest a total of 100 fugitives in copchase.", 14, 5, true);
            new Achievement("serial_killer", "Serial Killer", "Get a 5-kill killstreak.", 7, 3, true);
            new Achievement("first_loadout", "Gear up!", "Buy a loadout.", 2, 1, true);
            new Achievement("first_vehicle", "Speed is key", "Buy a vehicle.", 2, 1, true);
            new Achievement("all_loadouts", "Arms Dealer", "Unlock all loadouts.", 10, 3, true);
            new Achievement("all_vehicles", "Grand Theft Auto", "Unlock all vehicles.", 10, 3, true);
            new Achievement("first_busted", "Professional Driver", "Win a round of BUSTED! mode as a fugitive.", 5, 3, true);
            new Achievement("first_bankrob_fugitive", "Professional Thief", "Win a round of the Bank Robbery event as a fugitive.", 5, 3, true);
            new Achievement("first_bankrob_cop", "True Hero", "Win a round of the Bank Robbery event as a cop.", 5, 3, true);
            new Achievement("first_revive", "Future Medic?", "Revive a teammate in copchase.", 3, 1, true);
            new Achievement("low_hp_kill", "Call an ambulance, but not for me!", "Get a kill in copchase while your HP is lower than 30.", 7, 4, true);
            new Achievement("first_seatbelt", "Safety first!", "Buckle your seatbelt in copchase.", 1, 0, true);
            new Achievement("first_squad", "Best friends forever", "Squad up with a player.", 3, 0, true);
            new Achievement("get_revived", "Back from the dead", "Be revived by another player.", 3, 0, true);
            new Achievement("heist_leader", "I'm the boss now!", "Be the first heist leader in the Bank Robbery event.", 3, 0, true);
            new Achievement("heist_leader_win", "True Leader", "Win a round of the Bank Robbery event as the heist leader.", 7, 3, true);
            new Achievement("reach_level_100", "I'm kinda active...", "Reach level 100.", 15, 5, true);
            new Achievement("no_shooting_win", "Peaceful Criminal", "Win a round in copchase as a fugitive without opening fire at all.", 12, 5, true);
            new Achievement("first_kill_leader", "Go for the head!", "As a cop, have your first kill be the heist leader in the Bank Robbery event.", 15, 5, true);
            new Achievement("last_man_standing", "Last Man Standing", "Win a copchase round as the last person alive in your team.", 8, 3, true);
            new Achievement("all_achievements", "Where's my medal?", "Unlock all achievements!", 75, 35, true);

            NAPI.Util.ConsoleOutput("(RnC | STARTUP) Created " + Settings.Achievements.Count + " achievements.");


            NAPI.Util.ConsoleOutput("(RnC | STARTUP) Creating the items...");
            new ItemData(0, "Medkit", "medkit", 10, "Replenishes 60 HP, takes 10 seconds to fully use.", "https://cdn-icons-png.flaticon.com/512/2467/2467977.png");
            new ItemData(1, "Painkiller", "painkiller", 6, "Replenishes 1 HP/second for 40 seconds; you are free to move around.", "https://cdn-icons-png.flaticon.com/512/3437/3437974.png");
            new ItemData(2, "Lockpick", "lockpick", 10, "Allows you to break into locked vehicles; only fugitives can use this item.", "https://cdn-icons-png.flaticon.com/512/4690/4690067.png");

            /* PRE-SET STINGERS CREATION */
            NAPI.Util.ConsoleOutput("(RnC | STARTUP) Creating the pre-set stingers...");

            new Stinger(1, "Vinewood Hills", "Los Santos Freeway")
                .Add(new Vector3(864.457458, 104.437248, 69.017662), new Vector3(-3.99996424, 7.74004316, -126.800285), false)
                .Add(new Vector3(868.037231, 101.380112, 68.6703796), new Vector3(-5.00000238, 7.74004602, -130.525909), false)
                .Add(new Vector3(860.004333, 107.840805, 69.2920532), new Vector3(-1.99996078, 7.7400341, -125.932564), false)
                .Add(new Vector3(855.60553, 111.016136, 69.3830948), new Vector3(-1.99995077, 7.74003124, -128.789978), false)
                .Add(new Vector3(849.903442, 115.229126, 69.5843048), new Vector3(-4.99994087, 7.74002695, -123.418114), false)
                .Add(new Vector3(842.170959, 120.748032, 70.139534), new Vector3(-2.9999454, 7.73997259, -124.023514), false)
                .Add(new Vector3(846.066345, 117.960052, 69.9208679), new Vector3(-3.99994087, 7.74001884, -123.244217), false)
                .Add(new Vector3(838.105347, 123.878937, 70.2356415), new Vector3(-0.999967813, 7.7399683, -123.417183), false)
                .Add(new Vector3(868.605225, 100.659203, 69.5594177), new Vector3(0, 0, 37.5658493), true)
                .Add(new Vector3(834.803467, 125.96109, 71.2073212), new Vector3(0, -0, -142.016968), true);
            new Stinger(2, "Grand Senora Desert", "Route 68")
                .Add(new Vector3(2224.20386, 2741.32983, 44.4980011), new Vector3(-0.999999821, -0, -142.383636), false)
                .Add(new Vector3(2220.97827, 2745.30518, 44.5816536), new Vector3(8.34464913e-07, 1.06737957e-07, -142.989487), false)
                .Add(new Vector3(2217.93848, 2749.14063, 44.5730858), new Vector3(8.34456557e-07, 1.06735975e-07, -137.704315), false)
                .Add(new Vector3(2209.45532, 2760.54541, 44.496563), new Vector3(7.74860439e-07, 1.12074838e-06, -154.766357), false)
                .Add(new Vector3(2206.87427, 2764.88818, 44.5330009), new Vector3(7.74859643e-07, 1.12074849e-06, -141.600708), false)
                .Add(new Vector3(2203.13843, 2769.43115, 44.4885025), new Vector3(7.74855039e-07, 1.12074645e-06, -137.961624), false)
                .Add(new Vector3(2212.03735, 2750.4126, 45.6890831), new Vector3(0, -0, -115.353119), true)
                .Add(new Vector3(2208.9292, 2754.23828, 45.723278), new Vector3(0, 0, 11.9699469), true);
            new Stinger(3, "Braddock Pass", "Senora Freeway")
                .Add(new Vector3(2457.21069, 5659.93799, 44.0309181), new Vector3(-0.99999994, 2.66844893e-08, 114.096939), false)
                .Add(new Vector3(2461.46411, 5661.81543, 44.0759163), new Vector3(-0.999997079, 2.66844893e-07, 107.598099), false)
                .Add(new Vector3(2465.94019, 5663.06982, 44.0672264), new Vector3(-0.999995589, 2.40160375e-07, 108.896149), false)
                .Add(new Vector3(2439.80786, 5650.24902, 44.0841599), new Vector3(-0.999997675, 2.13475914e-07, 121.97596), false)
                .Add(new Vector3(2435.18311, 5648.24854, 44.083992), new Vector3(-0.999997616, 6.93796721e-07, 109.762627), false)
                .Add(new Vector3(2431.36719, 5645.89014, 44.0596313), new Vector3(-0.999995112, 1.17411753e-06, 123.273232), false)
                .Add(new Vector3(2445.32251, 5651.67285, 44.9679985), new Vector3(0, 0, 71.1312256), true)
                .Add(new Vector3(2455.64795, 5659.46094, 45.0228004), new Vector3(-0, -0, -68.5768204), true);
            new Stinger(4, "Paleto Bay", "Procopio Drive")
                .Add(new Vector3(149.713669, 6508.15625, 30.5366707), new Vector3(-0.15400061, -3.33506449e-08, -135.700516), false)
                .Add(new Vector3(145.890289, 6511.95117, 30.5634766), new Vector3(-0.15400365, -1.20062325e-07, -137.605225), false)
                .Add(new Vector3(142.026657, 6516.42822, 30.5839539), new Vector3(-0.154006004, 7.47054571e-07, -141.502258), false)
                .Add(new Vector3(138.492798, 6520.68896, 30.6014023), new Vector3(-0.154008195, 1.69421298e-06, -140.462143), false)
                .Add(new Vector3(135.24118, 6524.80762, 30.6457195), new Vector3(-0.154011637, 1.54079999e-06, -139.161865), false)
                .Add(new Vector3(131.531219, 6528.2666, 31.4761162), new Vector3(0, -0, -122.195518), true)
                .Add(new Vector3(151.796249, 6508.33887, 31.5315285), new Vector3(0, -0, 48.0318222), true);
            new Stinger(5, "Chilliad Mountain State Wilderness", "Freeway")
                .Add(new Vector3(-1514.46912, 4977.00537, 61.545269), new Vector3(0, -0, -132.189255), false)
                .Add(new Vector3(-1519.02454, 4980.96533, 61.5092201), new Vector3(0, -0, -130.629471), false)
                .Add(new Vector3(-1523.27197, 4984.71924, 61.5214119), new Vector3(2.54444361e-14, 4.66321922e-07, -134.439362), false)
                .Add(new Vector3(-1527.37207, 4988.83398, 61.5525398), new Vector3(5.08888721e-14, 4.66321922e-07, -132.879608), false)
                .Add(new Vector3(-1530.24573, 4991.30078, 62.488884), new Vector3(0, -0, -127.170448), true)
                .Add(new Vector3(-1513.81091, 4976.24756, 62.540863), new Vector3(0, 0, 44.7893028), true);
            new Stinger(6, "North Chumash", "Freeway")
                .Add(new Vector3(-2479.62964, 3624.20532, 13.0303288), new Vector3(-0.50000006, -2.00110843e-08, 80.9697876), false)
                .Add(new Vector3(-2474.46704, 3623.42651, 13.0659523), new Vector3(-0.499998569, 3.46858769e-07, 82.0080719), false)
                .Add(new Vector3(-2468.65039, 3622.49976, 13.0622196), new Vector3(-0.499999493, -2.66814464e-08, 76.638588), false)
                .Add(new Vector3(-2486.57275, 3625.57153, 13.0318298), new Vector3(-0.499999583, 9.33850401e-08, 78.9773941), false)
                .Add(new Vector3(-2492.24268, 3626.68555, 13.0596399), new Vector3(-0.499999315, -2.73484801e-07, 80.2763519), false)
                .Add(new Vector3(-2496.99585, 3627.37134, 13.0581608), new Vector3(-0.499999493, -5.06947345e-07, 83.5672607), false)
                .Add(new Vector3(-2464.12939, 3621.66553, 14.0353832), new Vector3(0, 0, 80.6171722), true)
                .Add(new Vector3(-2497.479, 3627.66089, 14.0366983), new Vector3(0, -0, -105.027687), true);
            new Stinger(7, "Chumash", "Braham Canyon Drive")
                .Add(new Vector3(-3135.90405, 1047.28992, 18.9112473), new Vector3(0, 0, 71.4114304), false)
                .Add(new Vector3(-3131.33862, 1045.73242, 18.9060917), new Vector3(0, 0, 69.5920105), false)
                .Add(new Vector3(-3126.02515, 1043.71851, 18.8783131), new Vector3(0, 0, 67.5993042), false)
                .Add(new Vector3(-3121.22461, 1041.95837, 18.8624325), new Vector3(-1.00000012, -2.66844893e-08, 76.3466721), false)
                .Add(new Vector3(-3116.56934, 1040.77893, 18.9025059), new Vector3(-0.399999291, -2.80151312e-07, 67.8580933), false)
                .Add(new Vector3(-3111.04956, 1039.20215, 19.8973217), new Vector3(0, 0, 72.7090454), true)
                .Add(new Vector3(-3136.52075, 1047.70605, 19.9158516), new Vector3(0, -0, -119.487709), true);
            new Stinger(8, "Del Perro", "Bay City Incline")
                .Add(new Vector3(-1873.98242, -573.478333, 10.6292706), new Vector3(0, -0, 137.21962), false)
                .Add(new Vector3(-1870.05371, -569.218323, 10.6286602), new Vector3(0, -0, 134.013748), false)
                .Add(new Vector3(-1866.02002, -565.142151, 10.6267757), new Vector3(0, -0, 135.658279), false)
                .Add(new Vector3(-1859.60217, -557.949158, 10.6270981), new Vector3(0, -0, 143.019257), false)
                .Add(new Vector3(-1856.05835, -553.419312, 10.6265841), new Vector3(0, -0, 133.750412), false)
                .Add(new Vector3(-1852.39807, -549.512817, 10.626915), new Vector3(0, -0, 142.411026), false)
                .Add(new Vector3(-1849.45105, -545.578125, 11.6357021), new Vector3(0, -0, 141.360046), true)
                .Add(new Vector3(-1874.77954, -574.08844, 11.6253786), new Vector3(-0, -0, -48.0632362), true);
            new Stinger(9, "Downtown", "Del Perro Freeway")
                .Add(new Vector3(234.38826, -536.193176, 32.8832474), new Vector3(-1.90000117, 2.00213268e-08, 178.898361), false)
                .Add(new Vector3(234.486755, -530.66925, 33.0870056), new Vector3(-1.89999783, -1.93539492e-07, 177.945099), false)
                .Add(new Vector3(234.505707, -525.444336, 33.0470772), new Vector3(-1.36000121, -9.34077988e-08, 172.227478), false)
                .Add(new Vector3(235.098328, -516.142273, 32.9900703), new Vector3(1.66999674, 6.6729406e-08, 177.943008), false)
                .Add(new Vector3(234.808716, -520.276733, 33.0710716), new Vector3(-1.89999652, 1.60170623e-06, 167.374725), false)
                .Add(new Vector3(235.570282, -506.213562, 32.9310493), new Vector3(-1.89999807, 1.33475515e-08, 177.511246), false)
                .Add(new Vector3(235.790405, -500.535522, 33.0644073), new Vector3(-1.89999378, 7.11174266e-08, -179.976547), false)
                .Add(new Vector3(235.710281, -494.570587, 33.0574646), new Vector3(-1.89999402, 3.3368881e-08, 178.20369), false)
                .Add(new Vector3(235.864166, -488.991943, 33.0237579), new Vector3(-1.89999413, 9.34328668e-08, 177.423462), false)
                .Add(new Vector3(235.003235, -537.083374, 33.8183784), new Vector3(0, 0, 42.9004402), true)
                .Add(new Vector3(235.123032, -483.120911, 33.8356438), new Vector3(0, -0, -122.532112), true);
            new Stinger(10, "El Burro Heights", "Del Perro Freeway")
                .Add(new Vector3(1217.78455, -2042.31616, 43.1598511), new Vector3(-1.99999988, -0, 112.153732), false)
                .Add(new Vector3(1223.01575, -2040.51404, 43.3536491), new Vector3(-1.99999928, 9.61080673e-07, 104.011246), false)
                .Add(new Vector3(1229.12683, -2038.55579, 43.398613), new Vector3(-1.99999833, 6.40720543e-07, 106.868874), false)
                .Add(new Vector3(1238.27332, -2035.29749, 43.3516464), new Vector3(-1.99999821, 7.47507272e-07, 107.387886), false)
                .Add(new Vector3(1249.26611, -2031.55603, 43.3414154), new Vector3(1.00000393, 1.46764683e-06, 109.550652), false)
                .Add(new Vector3(1243.93164, -2033.22839, 43.3508568), new Vector3(-1.99999642, 5.87327122e-07, 107.386009), false)
                .Add(new Vector3(1214.74988, -2043.18506, 44.2154427), new Vector3(-0, -0, -71.4681854), true)
                .Add(new Vector3(1255.46912, -2029.19788, 44.2202759), new Vector3(0, -0, 108.430885), true);
            new Stinger(11, "Elysian Island", "Elysian Fields Freeway")
                .Add(new Vector3(-470.615723, -2300.28662, 61.8680038), new Vector3(0, -0, 143.997528), false)
                .Add(new Vector3(-466.76886, -2294.84448, 61.8686485), new Vector3(0, -0, 145.295471), false)
                .Add(new Vector3(-462.407654, -2288.81226, 61.871048), new Vector3(0, -0, 144.773941), false)
                .Add(new Vector3(-459.048828, -2283.94263, 61.8701477), new Vector3(0, -0, 147.890656), false)
                .Add(new Vector3(-456.533173, -2279.86304, 63.0619659), new Vector3(0, -0, 141.527161), true)
                .Add(new Vector3(-471.166107, -2301.00879, 63.06493), new Vector3(-0, -0, -33.0876427), true);
            new Stinger(12, "La Puerta", "Olympic Freeway")
                .Add(new Vector3(-427.332275, -1413.20557, 37.4033699), new Vector3(0, -0, 98.654541), false)
                .Add(new Vector3(-417.428894, -1412.20862, 37.4408646), new Vector3(-2.00000048, 8.34271485e-10, 90.2518845), false)
                .Add(new Vector3(-411.006653, -1412.35266, 37.5516548), new Vector3(-0.999993324, 4.13609627e-07, 88.9522705), false)
                .Add(new Vector3(-400.705048, -1412.79565, 37.4484062), new Vector3(-1.9999913, 5.33933751e-08, 83.754982), false)
                .Add(new Vector3(-394.801544, -1413.354, 37.5613976), new Vector3(1.01923933e-05, -3.33558781e-09, 87.7389755), false)
                .Add(new Vector3(-388.997894, -1413.42212, 38.3984985), new Vector3(0, -0, 91.293602), true)
                .Add(new Vector3(-428.316162, -1413.14014, 38.3985176), new Vector3(0, -0, -91.7226715), true);
            new Stinger(13, "Downtown", "Integrity Way")
                .Add(new Vector3(131.923889, -535.307617, 42.1809769), new Vector3(-1.99999964, -1.0678675e-07, -109.102646), false)
                .Add(new Vector3(124.845146, -532.420288, 42.2503815), new Vector3(-1.99999821, 4.27147e-07, -118.023193), false)
                .Add(new Vector3(118.52803, -529.531433, 42.2256737), new Vector3(-0.999995887, 4.53636318e-07, -117.76284), false)
                .Add(new Vector3(111.909256, -525.881897, 42.2281647), new Vector3(5.0663939e-06, 6.40427686e-07, -111.006363), false)
                .Add(new Vector3(105.690163, -523.344421, 43.1842804), new Vector3(0, -0, -116.978622), true)
                .Add(new Vector3(133.88591, -536.137146, 43.2901878), new Vector3(0, 0, 65.8080444), true);
            new Stinger(14, "Palomino Highlands", "Olympic Freeway")
                .Add(new Vector3(1553.37244, -979.971863, 58.0622787), new Vector3(-3.00000024, 2.13736342e-07, 30.8056431), false)
                .Add(new Vector3(1556.27795, -985.244507, 58.2351265), new Vector3(2.54444361e-14, 7.47165643e-07, 21.3645229), false)
                .Add(new Vector3(1560.3252, -993.956787, 58.0739975), new Vector3(-2.99999928, 1.28241788e-06, 35.4824295), false)
                .Add(new Vector3(1563.55103, -999.126221, 58.2181778), new Vector3(5.24520783e-06, 1.01401054e-06, 19.4585514), false)
                .Add(new Vector3(1566.18738, -1005.19189, 59.0160332), new Vector3(0, 0, 25.7860851), true)
                .Add(new Vector3(1550.68091, -979.448486, 58.9455185), new Vector3(0, -0, -136.38591), true);
            new Stinger(15, "Tataviam Mountains", "Olympic Freeway")
                .Add(new Vector3(2403.79761, 1036.41675, 83.1021576), new Vector3(-3.00000024, 1.06868171e-07, -50.0246048), false)
                .Add(new Vector3(2399.05737, 1032.45215, 83.270668), new Vector3(2.00000596, -2.135735e-07, -57.8197899), false)
                .Add(new Vector3(2387.38794, 1023.61346, 83.1945267), new Vector3(2.0000062, -6.407206e-07, -51.3233681), false)
                .Add(new Vector3(2391.22632, 1026.96741, 83.1070404), new Vector3(-1.99999416, -9.61080673e-07, -49.7645073), false)
                .Add(new Vector3(2405.11353, 1037.63586, 84.0974884), new Vector3(0, -0, 148.877441), true)
                .Add(new Vector3(2383.35547, 1020.03998, 84.0156479), new Vector3(-0, -0, -47.421257), true);

            /* DEFAULT LOADOUTS CREATION */
            NAPI.Util.ConsoleOutput("(RnC | STARTUP) Creating the loadouts...");

            new LoadoutData(1, "Default")
                .Add(WeaponHash.Pistol, "https://assets.gm.miami/gtav/weapons/Pistol-icon.png", "Pistol", 40);

            // Discord-exclusive loadout
            new LoadoutData(1, "Negotiator")
                .Add(WeaponHash.Heavyshotgun, "https://assets.gm.miami/gtav/weapons/Heavy-shotgun-icon.png", "Heavy Shotgun", 20)
                .Add(WeaponHash.Bzgas, "https://assets.gm.miami/gtav/weapons/Tear-gas-icon.png", "BZ Gas", 1)
                .Add(WeaponHash.Heavypistol, "https://assets.gm.miami/gtav/weapons/Heavy-pistol-icon.png", "Heavy Pistol", 50);
            // -------------------------

            new LoadoutData(1, "Patrol", 5)
                .Add(WeaponHash.Pumpshotgun, "https://assets.gm.miami/gtav/weapons/Pump-shotgun-icon.png", "Pump Shotgun", 15)
                .Add(WeaponHash.Combatpistol, "https://assets.gm.miami/gtav/weapons/Combat-pistol-icon.png", "Combat Pistol", 30);

            new LoadoutData(1, "Metropolitan", 8)
                .Add(WeaponHash.Pumpshotgun, "https://assets.gm.miami/gtav/weapons/Pump-shotgun-icon.png", "Pump Shotgun", 25)
                .Add(WeaponHash.Bzgas, "https://assets.gm.miami/gtav/weapons/Tear-gas-icon.png", "BZ Gas", 1)
                .Add(WeaponHash.Heavypistol, "https://assets.gm.miami/gtav/weapons/Heavy-pistol-icon.png", "Heavy Pistol", 40);

            new LoadoutData(1, "FBI", 12, 15)
                .Add(WeaponHash.Bullpupshotgun, "https://assets.gm.miami/gtav/weapons/Bullpup-shotgun-icon.png", "Bullpup Shotgun", 28)
                .Add(WeaponHash.Bzgas, "https://assets.gm.miami/gtav/weapons/Tear-gas-icon.png", "BZ Gas", 2)
                .Add(WeaponHash.Pistol50, "https://assets.gm.miami/gtav/weapons/Pistol.50-icon.png", "Pistol .50", 40);

            new LoadoutData(1, "CIA", 15, 30)
                .Add(WeaponHash.Heavyshotgun, "https://assets.gm.miami/gtav/weapons/Heavy-shotgun-icon.png", "Heavy Shotgun", 20)
                .Add(WeaponHash.Assaultsmg, "https://assets.gm.miami/gtav/weapons/Assault-smg-icon.png", "Assault SMG", 80)
                .Add(WeaponHash.Bzgas, "https://assets.gm.miami/gtav/weapons/Tear-gas-icon.png", "BZ Gas", 1)
                .Add(WeaponHash.Pistol_mk2, "https://assets.gm.miami/gtav/weapons/Pistol-mk2-icon.png", "Pistol Mk2", 50);

            new LoadoutData(1, "Undercover", 17, 40)
                .Add(WeaponHash.Machinepistol, "https://assets.gm.miami/gtav/weapons/Machine-pistol-icon.png", "Machine Pistol", 80)
                .Add(WeaponHash.Sawnoffshotgun, "https://assets.gm.miami/gtav/weapons/Sawed-off-shotgun-icon.png", "Sawn Off Shotgun", 20)
                .Add(WeaponHash.Bzgas, "https://assets.gm.miami/gtav/weapons/Tear-gas-icon.png", "BZ Gas", 1)
                .Add(WeaponHash.Snspistol_mk2, "https://assets.gm.miami/gtav/weapons/Sns-pistol-mk2-icon.png", "SNS Pistol Mk2", 30);

            new LoadoutData(1, "SWAT", 20, 50)
                .Add((WeaponHash)350597077, "https://i.imgur.com/rbqgDgt.png", "Tactical SMG", 60)
                .Add(WeaponHash.Pumpshotgun, "https://assets.gm.miami/gtav/weapons/Pump-shotgun-icon.png", "Pump Shotgun", 15)
                .Add(WeaponHash.Bzgas, "https://assets.gm.miami/gtav/weapons/Tear-gas-icon.png", "BZ Gas", 2)
                .Add(WeaponHash.Combatpistol, "https://assets.gm.miami/gtav/weapons/Combat-pistol-icon.png", "Combat Pistol", 30);

            new LoadoutData(1, "Marine", 25, 70)
                .Add(WeaponHash.Carbinerifle, "https://assets.gm.miami/gtav/weapons/Carbine-rifle-icon.png", "Carbine Rifle", 60)
                .Add(WeaponHash.Bullpupshotgun, "https://assets.gm.miami/gtav/weapons/Bullpup-shotgun-icon.png", "Bullpup Shotgun", 28)
                .Add(WeaponHash.Bzgas, "https://assets.gm.miami/gtav/weapons/Tear-gas-icon.png", "BZ Gas", 1)
                .Add(WeaponHash.Heavypistol, "https://assets.gm.miami/gtav/weapons/Heavy-pistol-icon.png", "Heavy Pistol", 30);

            new LoadoutData(1, "Navy Seal", 35, 100)
                .Add(WeaponHash.Heavyshotgun, "https://assets.gm.miami/gtav/weapons/Heavy-shotgun-icon.png", "Heavy Shotgun", 20)
                .Add(WeaponHash.Specialcarbine, "https://assets.gm.miami/gtav/weapons/Special-carbine-icon.png", "Special Carbine", 70)
                .Add(WeaponHash.Bzgas, "https://assets.gm.miami/gtav/weapons/Tear-gas-icon.png", "BZ Gas", 1)
                .Add(WeaponHash.Heavypistol, "https://assets.gm.miami/gtav/weapons/Heavy-pistol-icon.png", "Heavy Pistol", 30);

            // 94989220 - Combat Shotgun hash
            // 2636060646 - Military Rifle hash
            // https://i.imgur.com/TY7Npph.png - Snub Nose revolver custom icon by codename (looks like shit)
            new LoadoutData(1, "Black Ops", 42, 110)
                .Add((WeaponHash)94989220, "https://assets.gm.miami/gtav/weapons/Combat-shotgun-icon.png", "Combat Shotgun", 30)
                .Add((WeaponHash)2636060646, "https://assets.gm.miami/gtav/weapons/Military-rifle-icon.png", "Military Rifle", 90)
                .Add(WeaponHash.Bzgas, "https://assets.gm.miami/gtav/weapons/Tear-gas-icon.png", "BZ Gas", 1)
                .Add(WeaponHash.Doubleaction, "https://i.imgur.com/TY7Npph.png", "Snub Nose Revolver", 40);

            // 3520460075 = Tactical Rifle hash
            new LoadoutData(1, "Veteran", 45, 125)
               .Add((WeaponHash)3520460075, "https://assets.gm.miami/gtav/weapons/Tactical-rifle-icon.png", "Service Carbine", 90)
               .Add(WeaponHash.Bzgas, "https://assets.gm.miami/gtav/weapons/Tear-gas-icon.png", "BZ Gas", 1)
               .Add(WeaponHash.NavyRevolver, "https://assets.gm.miami/gtav/weapons/Navy-revolver-icon.png", "Navy Revolver", 26);

            new LoadoutData(1, "Juggernaut", 50, 150)
                .Add(WeaponHash.Mg, "https://assets.gm.miami/gtav/weapons/Mg-icon.png", "Combat MG", 160)
                .Add(WeaponHash.Pumpshotgun, "https://assets.gm.miami/gtav/weapons/Pump-shotgun-icon.png", "Pump Shotgun", 15)
                .Add((WeaponHash)1, "package://Artwork/Weapons/Armour.png", "Full Armour", 100)
                .Add(WeaponHash.Heavypistol, "https://assets.gm.miami/gtav/weapons/Heavy-pistol-icon.png", "Heavy Pistol", 40);



            //

            new LoadoutData(0, "Default")
                .Add(WeaponHash.Pistol, "https://assets.gm.miami/gtav/weapons/Pistol-icon.png", "Pistol", 40);

            // Discord-exclusive loadout
            new LoadoutData(0, "Negotiator")
                .Add(WeaponHash.Heavyshotgun, "https://assets.gm.miami/gtav/weapons/Heavy-shotgun-icon.png", "Heavy Shotgun", 50)
                .Add(WeaponHash.Molotov, "https://assets.gm.miami/gtav/weapons/Molotov-icon.png", "Molotov", 1)
                .Add(WeaponHash.Heavypistol, "https://assets.gm.miami/gtav/weapons/Heavy-pistol-icon.png", "Heavy Pistol", 50);
            // -------------------------

            new LoadoutData(0, "Wannabe", 5)
                .Add(WeaponHash.Sawnoffshotgun, "https://assets.gm.miami/gtav/weapons/Sawed-off-shotgun-icon.png", "Sawn Off Shotgun", 40)
                .Add(WeaponHash.Snspistol, "https://assets.gm.miami/gtav/weapons/Sns-pistol-icon.png", "SNS Pistol", 60);

            new LoadoutData(0, "Hoodrat", 8, 10)
                .Add(WeaponHash.Compactrifle, "https://assets.gm.miami/gtav/weapons/Compact-rifle-icon.png", "Compact Rifle", 80)
                .Add(WeaponHash.Bullpupshotgun, "https://assets.gm.miami/gtav/weapons/Bullpup-shotgun-icon.png", "Bullpup Shotgun", 50)
                .Add(WeaponHash.Pistol_mk2, "https://assets.gm.miami/gtav/weapons/Pistol-mk2-icon.png", "Pistol Mk2", 50);

            new LoadoutData(0, "Thug", 13, 15)
                .Add(WeaponHash.Microsmg, "https://assets.gm.miami/gtav/weapons/Micro-smg-icon.png", "Micro SMG", 60)
                .Add(WeaponHash.Bullpupshotgun, "https://assets.gm.miami/gtav/weapons/Bullpup-shotgun-icon.png", "Bullpup Shotgun", 40)
                .Add(WeaponHash.Pistol, "https://assets.gm.miami/gtav/weapons/Pistol-icon.png", "Pistol", 40);

            new LoadoutData(0, "Hustler", 15, 25)
                .Add(WeaponHash.Smg, "https://assets.gm.miami/gtav/weapons/Smg-icon.png", "SMG", 70)
                .Add(WeaponHash.Combatpistol, "https://assets.gm.miami/gtav/weapons/Combat-pistol-icon.png", "Combat Pistol", 60);

            new LoadoutData(0, "Cartel", 18, 30)
                .Add(WeaponHash.Minismg, "https://assets.gm.miami/gtav/weapons/Mini-smg-icon.png", "Mini SMG", 80)
                .Add(WeaponHash.Pumpshotgun, "https://assets.gm.miami/gtav/weapons/Pump-shotgun-icon.png", "Pump Shotgun", 30)
                .Add(WeaponHash.Pistol_mk2, "https://assets.gm.miami/gtav/weapons/Pistol-mk2-icon.png", "Pistol Mk2", 40);

            new LoadoutData(0, "Trigger", 22, 40)
                .Add(WeaponHash.Assaultrifle, "https://assets.gm.miami/gtav/weapons/Assault-rifle-icon.png", "Assault Rifle", 80)
                .Add(WeaponHash.Pistol, "https://assets.gm.miami/gtav/weapons/Pistol-icon.png", "Pistol", 60);

            new LoadoutData(0, "Enforcer", 27, 60)
                .Add(WeaponHash.Assaultrifle, "https://assets.gm.miami/gtav/weapons/Assault-rifle-icon.png", "Assault Rifle", 80)
                .Add(WeaponHash.Pumpshotgun, "https://assets.gm.miami/gtav/weapons/Pump-shotgun-icon.png", "Pump Shotgun", 40)
                .Add(WeaponHash.Pistol50, "https://assets.gm.miami/gtav/weapons/Pistol.50-icon.png", "Pistol .50", 28);

            new LoadoutData(0, "Shot-Caller", 27, 40)
                .Add(WeaponHash.Microsmg, "https://assets.gm.miami/gtav/weapons/Micro-smg-icon.png", "Micro SMG", 70)
                .Add(WeaponHash.Heavyshotgun, "https://assets.gm.miami/gtav/weapons/Heavy-shotgun-icon.png", "Heavy Shotgun", 50)
                .Add(WeaponHash.Molotov, "https://assets.gm.miami/gtav/weapons/Molotov-icon.png", "Molotov", 1)
                .Add(WeaponHash.Snspistol, "https://assets.gm.miami/gtav/weapons/Sns-pistol-icon.png", "SNS Pistol", 60);

            new LoadoutData(0, "Kingpin", 35, 70)
                .Add(WeaponHash.Heavyshotgun, "https://assets.gm.miami/gtav/weapons/Heavy-shotgun-icon.png", "Heavy Shotgun", 50)
                .Add(WeaponHash.Carbinerifle, "https://assets.gm.miami/gtav/weapons/Carbine-rifle-icon.png", "Carbine Rifle", 80)
                .Add(WeaponHash.Molotov, "https://assets.gm.miami/gtav/weapons/Molotov-icon.png", "Molotov", 1)
                .Add(WeaponHash.Heavypistol, "https://assets.gm.miami/gtav/weapons/Heavy-pistol-icon.png", "Heavy Pistol", 60);

            // 3347935668 - Heavy Rifle hash
            new LoadoutData(0, "Rebel", 40, 60)
                .Add((WeaponHash)3347935668, "https://assets.gm.miami/gtav/weapons/Heavy-rifle-icon.png", "Heavy Rifle", 100)
                .Add(WeaponHash.Dbshotgun, "https://assets.gm.miami/gtav/weapons/Double-barrel-shotgun-icon.png", "Double-Barrel Shotgun", 6)
                .Add(WeaponHash.CeramicPistol, "https://assets.gm.miami/gtav/weapons/Ceramic-pistol-icon.png", "Ceramic Pistol", 30);

            new LoadoutData(0, "The Elder", 45, 70)
               .Add(WeaponHash.Dbshotgun, "https://assets.gm.miami/gtav/weapons/Double-barrel-shotgun-icon.png", "Double-Barrel Shotgun", 12)
               .Add(WeaponHash.Gusenberg, "https://assets.gm.miami/gtav/weapons/Gusenberg-sweeper-icon.png", "Gusenberg Sweeper", 120)
               .Add(WeaponHash.NavyRevolver, "https://assets.gm.miami/gtav/weapons/Navy-revolver-icon.png", "Navy Revolver", 20);

            new LoadoutData(0, "Godfather", 50, 70)
                .Add(WeaponHash.Bullpupshotgun, "https://assets.gm.miami/gtav/weapons/Bullpup-shotgun-icon.png", "Bullpup Shotgun", 50)
                .Add(WeaponHash.Specialcarbine, "https://assets.gm.miami/gtav/weapons/Special-carbine-icon.png", "Special Carbine", 80)
                .Add(WeaponHash.Molotov, "https://assets.gm.miami/gtav/weapons/Molotov-icon.png", "Molotov", 3)
                .Add(WeaponHash.Pistol50, "https://assets.gm.miami/gtav/weapons/Pistol.50-icon.png", "Pistol .50", 40);


            /* VEHICLES/SELECTION CREATION */
            NAPI.Util.ConsoleOutput("(RnC | STARTUP) Creating the selection vehicles...");

            // Defaults
            new CarData(0, "Random", "?", "package://Artwork/Vehicles/default.png", "");
            new CarData(1, "Random", "?", "package://Artwork/Vehicles/default.png", "");

            // 0-100's Cop
            // >8 second mark = Slow
            // 7-8 second mark = Average
            // 6-7 second mark = Average+
            // <6 second mark = Fast
            //  IDK MAN JUST CHECK THE DRIVING HANDLINGS AND HOW FAST IT FEELS LUL

            // Cop
            new CarData(1, "Police Cruiser", "police", "package://Artwork/Vehicles/police.png", "Slow");
            new CarData(1, "Unmarked Cruiser", "police4", "package://Artwork/Vehicles/police4.png", "Slow");
            new CarData(1, "Granger Retro", "shegrangerold", "package://Artwork/Vehicles/shegrangerold.png", "Slow", 3);
            new CarData(1, "Police Bike", "policeb", "package://Artwork/Vehicles/policeb.png", "Average", 3);
            new CarData(1, "Police Buffalo", "police2", "package://Artwork/Vehicles/police2.png", "Average", 5, 5);
            new CarData(1, "Sheriff Alamo", "polalamor", "package://Artwork/Vehicles/polalamor.png", "Slow", 6, 5);
            new CarData(1, "Sheriff Bison", "polbisonr", "package://Artwork/Vehicles/polbisonr.png", "Slow", 6, 8);
            new CarData(1, "Police Bison", "polbisonf", "package://Artwork/Vehicles/polbisonf.png", "Average", 6, 10);
            new CarData(1, "Police Scout", "polscout", "package://Artwork/Vehicles/polscout.png", "Average+", 6, 10);
            new CarData(1, "Police Bike 2", "policeb1", "package://Artwork/Vehicles/policeb1.png", "Fast", 6, 15);
            new CarData(1, "Unmarked Scout", "unmarkedscout", "package://Artwork/Vehicles/unmarkedscout.png", "Average+", 8, 10);
            new CarData(1, "LSPD Crown Vic", "lspdvic", "package://Artwork/Vehicles/lspdvic.png", "Average+", 10, 10);
            new CarData(1, "FBI Granger", "fbi2", "package://Artwork/Vehicles/fbi2.png", "Slow", 10, 15);
            new CarData(1, "Police Buffalo A/C", "polbufac", "package://Artwork/Vehicles/polbufac.png", "Fast", 13, 20);
            new CarData(1, "Police Transporter", "policet", "package://Artwork/Vehicles/policet.png", "Snail", 15);
            //new CarData(1, "Police Alamo", "polalamo", "package://Artwork/Vehicles/polalamo.png", "Fast", 15, 15);
            new CarData(1, "Unmarked Alamo", "umkalamo", "package://Artwork/Vehicles/umkalamo.png", "Fast", 15, 20);
            new CarData(1, "FBI Buffalo A/C", "fbibufac", "package://Artwork/Vehicles/fbibufac.png", "Fast", 17, 20);
            new CarData(1, "Police Chopper", "polmav", "package://Artwork/Vehicles/polmav.png", "N/A", 20, 20);
            new CarData(1, "AS-350", "as350", "package://Artwork/Vehicles/as350.png", "N/A", 20, 25);
            new CarData(1, "Sheriff Fugitive", "polfugitiver", "package://Artwork/Vehicles/polfugitiver.png", "Fast", 25, 40);
            new CarData(1, "Unmarked Charger", "lspdunm", "package://Artwork/Vehicles/lspdunm.png", "Fast+", 25, 40);
            new CarData(1, "Sheriff Gauntlet 2", "polgauntletr", "package://Artwork/Vehicles/polgauntletr.png", "Fast", 25, 45);
            new CarData(1, "FIB Executioner", "fibexecutioner", "package://Artwork/Vehicles/fibexecutioner.png", "Average", 25, 50);
            //new CarData(1, "Sheriff Torrence", "poltorencer", "package://Artwork/Vehicles/poltorencer.png", "Fast", 28, 40);
            new CarData(1, "Police Buffalo SX", "buffalosxpol", "package://Artwork/Vehicles/buffalosxpol.png", "Fast+", 30, 50);
            new CarData(1, "Chevrolet Caprice PPV", "bcso2slick", "package://Artwork/Vehicles/bcso2slick.png", "Fast", 30, 65);
            new CarData(1, "Torrence Interceptor", "intcept", "package://Artwork/Vehicles/intcept.png", "Fast+", 30, 65);
            new CarData(1, "Torrence Unmarked", "intcept2", "package://Artwork/Vehicles/intcept2.png", "Fast+", 30, 70);
            new CarData(1, "Sheriff Gauntlet", "shergauntlet", "package://Artwork/Vehicles/shergauntlet.png", "Fast+", 38, 80);
            new CarData(1, "Sheriff Vigero", "polvigeror", "package://Artwork/Vehicles/polvigeror.png", "Fast+", 45, 70);
            new CarData(1, "Sheriff Buffalo SX", "polbuffalor", "package://Artwork/Vehicles/polbuffalor.png", "Fast+", 50, 70);
            new CarData(1, "Sheriff Buffalo", "polbuffalor2", "package://Artwork/Vehicles/polbuffalor2.png", "Fast+", 50, 75);
            new CarData(1, "LENCO Bearcat", "bcat", "package://Artwork/Vehicles/bcat.png", "Slow", 55, 100);

            // 0-100's Fugitive
            // >9 second mark = Slow
            // 7.5-8 second mark = Average
            // 7-8 second mark = Average+
            // 7 second mark = Fast
            // ~6.5 = Fast+
            // <6.5 = Super

            // Fugitives
            new CarData(0, "Super Diamond", "superd", "package://Artwork/Vehicles/superd.png", "Fast");
            new CarData(0, "Primo", "primo", "package://Artwork/Vehicles/primo.png", "Slow");
            new CarData(0, "Esperanto", "esperanto2d", "package://Artwork/Vehicles/esperanto2d.png", "Slow", 0, 3);
            new CarData(0, "Primo LX", "primo3", "package://Artwork/Vehicles/primo3.png", "Slow", 0, 5);
            new CarData(0, "Futo GTX", "futo2", "package://Artwork/Vehicles/futo2.png", "Slow", 0, 5);
            new CarData(0, "Stratum", "stratum", "package://Artwork/Vehicles/stratum.png", "Slow", 0, 5);
            new CarData(0, "Seminole Frontier", "seminole2", "package://Artwork/Vehicles/seminole2.png", "Slow", 1);
            new CarData(0, "Huntley", "huntley", "package://Artwork/Vehicles/huntley.png", "Average", 3);
            new CarData(0, "Altior", "altior", "package://Artwork/Vehicles/altior.png", "Average", 4, 5);
            new CarData(0, "Tailgater", "tailgater", "package://Artwork/Vehicles/tailgater.png", "Slow", 5);
            //new CarData(0, "Sanchez", "sanchez2", "package://Artwork/Vehicles/sanchez2.png", "Fast", 5, 10);
            new CarData(0, "Felon GT", "felon2", "package://Artwork/Vehicles/felon2.png", "Average+", 5);
            new CarData(0, "Huntley IV", "huntley2", "package://Artwork/Vehicles/huntley2.png", "Average", 6);
            new CarData(0, "Gresley Hellhound", "gresleyh", "package://Artwork/Vehicles/gresleyh.png", "Average+", 8, 3);
            new CarData(0, "Blista Classic", "blista2", "package://Artwork/Vehicles/blista2.png", "Average", 10);
            new CarData(0, "Landroamer", "landroamer", "package://Artwork/Vehicles/landroamer.png", "Average+", 10, 5);
            new CarData(0, "Gang Burrito", "gburrito2", "package://Artwork/Vehicles/gburrito2.png", "Slow", 10, 5);
            new CarData(0, "Fugitive", "fugitive", "package://Artwork/Vehicles/fugitive.png", "Average", 10, 10);
            new CarData(0, "Cheburek", "cheburek", "package://Artwork/Vehicles/cheburek.png", "Average", 15, 10);
            new CarData(0, "Exemplar", "exemplar", "package://Artwork/Vehicles/exemplar.png", "Average+", 17, 15);
            new CarData(0, "Chevrolet Impala SS '96", "impala", "package://Artwork/Vehicles/impala.png", "Average", 18, 30);
            new CarData(0, "Issi 8", "issi8", "package://Artwork/Vehicles/issi8.png", "Average+", 22, 25);
            new CarData(0, "Buffalo", "buffalo", "package://Artwork/Vehicles/buffalo.png", "Average", 22, 30);
            new CarData(0, "Tailgater S", "tailgater2", "package://Artwork/Vehicles/tailgater2.png", "Average+", 23, 30);
            new CarData(0, "Sultan", "sultan", "package://Artwork/Vehicles/sultan.png", "Average", 25, 40);
            new CarData(0, "Omnis", "omnis", "package://Artwork/Vehicles/omnis.png", "Fast", 25, 50);
            new CarData(0, "Scout", "scout", "package://Artwork/Vehicles/scout.png", "Fast", 30, 40);
            new CarData(0, "Tulip SS", "tulip2", "package://Artwork/Vehicles/tulip2.png", "Fast", 30, 50);
            //new CarData(0, "Kuruma", "kuruma", "package://Artwork/Vehicles/kuruma.png", "Fast", 30, 60); KURUMA TEMPORARILY REMOVED UNTIL BULLET PROOF GTA ONLIEN BUG FIX
            new CarData(0, "Buffalo SX", "buffalosx", "package://Artwork/Vehicles/buffalosx.png", "Fast+", 30, 70);
            new CarData(0, "Dominator GTT", "dominator8", "package://Artwork/Vehicles/dominator8.png", "Fast+", 32, 50);
            new CarData(0, "BMW M3 E46", "m3e46", "package://Artwork/Vehicles/m3e46.png", "Fast", 35, 0, 10);
            new CarData(0, "Sultan Classic", "sultan2", "package://Artwork/Vehicles/sultan2.png", "Fast", 37, 80);
            new CarData(0, "Windsor 2", "windsor2", "package://Artwork/Vehicles/windsor2.png", "Fast", 40, 60);
            new CarData(0, "ZR350", "zr350", "package://Artwork/Vehicles/zr350.png", "Fast", 40, 70);
            new CarData(0, "Golf Mk6", "golfmk6", "package://Artwork/Vehicles/golfmk6.png", "Fast+", 50, 85);
            new CarData(0, "2022 Porsche Cayenne", "cayenneturbo", "package://Artwork/Vehicles/cayenneturbo.png", "Fast+", 60, 80);

            if (Settings.Dynamics.Discord)
            {
                NAPI.Util.ConsoleOutput("(RnC | STARTUP) Loading discord plugin.");

                Task.Run(async () =>
                {
                    await DiscordBot.Start();
                });

            }
            else NAPI.Util.ConsoleOutput("(RnC | OPT) Discord Bot is disabled, live commands and statuses not updated!");

            //Start websocket server
            //Websocket.Start();

            NAPI.TextLabel.CreateTextLabel("not today, friend", new Vector3(104.954094f, -744.55927f, 45.754745f), 2.0f, 1.0f, 0, new Color(255, 255, 255), false);
            NAPI.TextLabel.CreateTextLabel("Bored of waiting? Play some DM", new Vector3(107.493866f, -744.79114f, 242.15219f), 13.0f, 2.2f, 0, new Color(255, 0, 0, 156), false);
            NAPI.TextLabel.CreateTextLabel("Press M to open menu and more", new Vector3(109.71294f, -753.1537f, 242.15219f), 13.0f, 2.2f, 0, new Color(56, 236, 242, 156), false);
            NAPI.TextLabel.CreateTextLabel("Check out other modes in the menu", new Vector3(121.34787f, -738.852f, 242.1522f), 13.0f, 2.2f, 0, new Color(40, 153, 45, 156), false);
            NAPI.TextLabel.CreateTextLabel("Change appearance with /char & /clothing", new Vector3(120.48237f, -726.71735f, 242.15198f), 13.0f, 2.2f, 0, new Color(212, 67, 217, 156), false);
            NAPI.TextLabel.CreateTextLabel("Confused? /help or /helpme", new Vector3(120.84272f, -744.534f, 242.15207f), 13.0f, 2.2f, 0, new Color(255, 255, 255, 156), false);
            NAPI.TextLabel.CreateTextLabel("Want to watch the round? /spec", new Vector3(118.04166f, -757.9125f, 242.15215f), 13.0f, 2.2f, 0, new Color(255, 228, 0, 156), false);

            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    if (await context.DailyChallenges.AsQueryable().CountAsync() <= 0)
                        Challenge.SetNewDailyChallenges();
                }
            });


            NAPI.Util.ConsoleOutput("(RnC | STARTUP) Server startup proceeded:\n");

            string admins = "";
            int adminCount = 0;
            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    try
                    {
                        List<Account> adminsList = await context.Accounts.AsQueryable().Where(a => a.Admin > 0).ToListAsync();
                        foreach (Account acc in adminsList)
                        {
                            if (adminCount != 0) admins += ", ";
                            admins += $"{acc.Username} [{acc.SQL_ID}]";
                            adminCount++;
                        }
                        if (adminCount == 0) admins = "None";
                        int accountsCnt = await context.Accounts.AsQueryable().CountAsync();
                        NAPI.Task.Run(() =>
                        {
                            string dateFormat = DateTime.Now.ToString("g");
                            NAPI.Util.ConsoleOutput("CURRENT DATE: " + dateFormat);
                            dateFormat = DateTimeOffset.FromUnixTimeSeconds(Settings.Dynamics.NextWeekUnix).ToString("g");
                            NAPI.Util.ConsoleOutput("NEXT WEEK HIT: " + dateFormat);
                            dateFormat = DateTimeOffset.FromUnixTimeSeconds(Settings.Dynamics.NextMonthUnix).ToString("g");
                            NAPI.Util.ConsoleOutput("NEXT MONTH HIT: " + dateFormat);

                            NAPI.Util.ConsoleOutput("[>===========|Race 'N' Chase|===========<]");
                            NAPI.Util.ConsoleOutput("[Version]: " + Settings.Version);
                            NAPI.Util.ConsoleOutput("[Developers]: KevY, CodeName420 (jambredrek)");
                            NAPI.Util.ConsoleOutput($"[Accounts]: {accountsCnt}");
                            NAPI.Util.ConsoleOutput($"[Admins]: {admins}");
                            NAPI.Util.ConsoleOutput("[>===========|Race 'N' Chase|===========<]");

                            // After all the stuff is set.
                            SaveDynamicSettings();

                            ///////////////////////////////////////////////////////////////////////////////

                            Settings.Timers.GameUpdate_Tick = Environment.TickCount64 + 1000;
                            Settings.Timers.TimeUpdate_Tick = Environment.TickCount64 + 1000;
                            Settings.Timers.EventUpdate_Tick = Environment.TickCount64 + 1000;

                        });
                    }
                    catch (Exception ex)
                    {
                        NAPI.Util.ConsoleOutput("GAY " + ex.ToString());
                    }
                }
            });

        }

        [ServerEvent(Event.Update)]
        public void OnUpdate()
        {
            if (Environment.TickCount64 - Settings.Timers.GameUpdate_Tick >= 1000)
            {
                GameUpdate();
                Settings.Timers.GameUpdate_Tick = Environment.TickCount64;
            }
            if (Environment.TickCount64 - Settings.Timers.TimeUpdate_Tick >= 15000)
            {
                TimeUpdate();
                Settings.Timers.TimeUpdate_Tick = Environment.TickCount64;
            }
            if (Environment.TickCount64 - Settings.Timers.EventUpdate_Tick >= 1000)
            {
                EventUpdate();
                Settings.Timers.EventUpdate_Tick = Environment.TickCount64;
            }
        }

        private void EventUpdate()
        {
            foreach(EventManager.EventData gameEvent in EventManager.Events)
            {
                if(gameEvent.TimeUntilStart > 0)
                {
                    gameEvent.TimeUntilStart--;
                    continue;
                }
                if(gameEvent.TimeUntilStart == 0 && !gameEvent.InProgress)
                {
                    EventManager.StartEvent(gameEvent);
                    continue;
                }
            }            
        }

        public void GameStartCountdown(int time, int lobbyid)
        {
            time -= 2;

            lock (Settings.Lobbies)
            {
                LobbyData lobby = Settings.Lobbies.Find(l => l.lobbyId == lobbyid);
                if (lobby != null)
                {
                    lock (lobby)
                    {
                        foreach (Player player in Settings.LoggedInPlayers)
                        {
                            lock (player)
                            {
                                if (player != null && player.Exists)
                                {
                                    if (player.GetSharedDataSafe<int>("InLobby") == lobbyid && player.GetSharedDataSafe<bool>("AutoJoin") == true)
                                    {
                                        if (time > 0)
                                        {
                                            //NAPI.Notification.SendNotificationToPlayer(player, $"Game is starting in ~g~{time} seconds~w~.", true);
                                        }
                                        else
                                        {
                                            //NAPI.Notification.SendNotificationToPlayer(player, $"~y~Game is now starting.", true);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (time > 0)
                    {
                        NAPI.Task.Run(() =>
                        {
                            GameStartCountdown(time, lobbyid);
                        }, delayTime: 2000);
                    }
                    else
                    {
                        StartGameNew(lobbyid);
                    }
                }
            }
        }

        public async void GenerateRedeemableCodes()
        {
            // UNUSED: Codes are generated on purchase.
            return;
            /* ---Donation Types/Values---
                0 - Rank Budle ( To je ono kad mu s rankom da i namechangeve i sve kaj treba ko noramlan rank kad kupe_ 
                -- 0 - Bronze i0
                -- 1 - Silver i1
                -- 2 - Gold i2
                1 - Rank (Samo rank bez cjelog bundla)
                -- 0 - Bronze i3
                -- 1 - Silver i4
                -- 2 - Gold i5
                2 - DiamondPoints
                -- broj pointova kolko za dat i6
                3 - Namechanges
                -- broj namechangeva i7
             */
        }

        public void TimeUpdate()
        {
            try
            {
                if (Environment.TickCount64 - Settings.AutoSaveTick >= 300000) // 5 mins
                {
                    Task.Run(async () =>
                    {
                        using (MainDatabase context = ContextFactory.CreateContext())
                        {
                            DateTime challengeCheck = DateTime.Now;
                            TimeSpan sp = new TimeSpan(challengeCheck.Ticks);
                            long seconds = (long)Math.Ceiling(sp.TotalSeconds);

                            // since they are all set at the time, it's enough that only one of them expired to re-new them
                            DailyChallenge dc = await context.DailyChallenges.AsQueryable().FirstOrDefaultAsync(c => c.ExpireTimestamp <= seconds);
                            if (dc != null)
                            {
                                Challenge.SetNewDailyChallenges();
                                NAPI.Task.Run(() =>
                                {
                                    EPlayer.SendMessageToAll("NEW_CHALLENGES");
                                });

                                await context.Accounts.AsQueryable().ForEachAsync(acc =>
                                {
                                    acc.CompletedDailyChallenge = false;
                                });
                            }

                            // runweeklymonthlychecks - will check if a week or month has passed and do shit
                            NAPI.Task.Run(() =>
                            {
                                Events.RunWeeklyMonthlyChecks();
                                SaveDynamicSettings();
                                ContextFactory.SaveDB();
                                NAPI.Util.ConsoleOutput("Server auto-saved => " + DateTime.Now.ToString());
                                Settings.AutoSaveTick = Environment.TickCount64;
                            });
                        }
                    });

                }

                if (Environment.TickCount64 - Settings.LastMessageTick > 90000)
                {
                    /*
                    string[] msgList = {
                        "If you would like to experience some exclusive and limited features then donate to keep the server running!",
                        "The server's discord, information and credits can be found at /discord or /info!",
                        "Use /rules to see the server rules.",
                        "Use /cmds to see the server commands & /helpme if you need assistance.",
                        "Have you seen someone cheating? Screenshot or record them and you'll never see them again.",
                        "Bored of waiting in the lobby? Check out our side activities like Deathmatch, Freeroam, Derby!",
                        "Want to earn some quick score? Head over to a DM arena!",
                        "You can save your current cop & fugitive appearance profile and select a different one by using /outfit.",
                        "Points can be used to purchase items such as medkits (/mk), painkillers (/pk), and even tune vehicles (/vtuner).",
                        "You can press M to open the menu, check more at /keys!",
                        "Use /clothing and /char to change your clothing or character's face.",
                    };

                    RAGE.Entities.Players.All.ForEach((Player e) => e.SendChatMessage("[?] !{#F0FC72}" + msgList.OrderBy(c => Guid.NewGuid()).First()));
                    */
                    Settings.LastMessageTick = Environment.TickCount64;
                }

                RAGE.Entities.Players.All.ForEach((Player e) =>
                {
                    if (e.Position.DistanceTo(e.GetData<Vector3>("TUA_LP")) < 2.0) // Haven't moved more than 2m
                    {
                        e.SetData("TUA_LT", e.GetData<int>("TUA_LT") + 1);
                    }
                    else { e.SetData("TUA_LT", 0); }

                    e.SetData("TUA_LP", e.Position); // Set new pos so it keeps checking
                });

                lock (Settings.Lobbies)
                {
                    foreach (LobbyData lobby in Settings.Lobbies)
                    {
                        lock (lobby)
                        {
                            if (lobby != null)
                            {
                                lobby.Time[2] += 30; // Twice as fast as in real life.
                                if (lobby.Time[2] >= 60)
                                {
                                    lobby.Time[2] = 0;
                                    lobby.Time[1]++;

                                    if (lobby.Time[1] >= 60)
                                    {
                                        lobby.Time[1] = 0;
                                        lobby.Time[0]++;

                                        lobby.Weather = new Random(DateTimeOffset.UtcNow.Millisecond).Next(0, 9); // Change weather every hour or so.
                                        if (lobby.Weather == 6 || lobby.Weather == 7 || lobby.Weather == 8)
                                        {
                                            lobby.Weather = new Random(DateTimeOffset.UtcNow.Millisecond).Next(0, 9); // Once again, if it gets rain you are just UNLUCKY.
                                            if (lobby.Weather == 6 || lobby.Weather == 7 || lobby.Weather == 8)
                                            {
                                                lobby.Weather = new Random(DateTimeOffset.UtcNow.Millisecond).Next(0, 9); // Actually, we all hate rain so.
                                            }
                                        }
                                        if (lobby.Time[0] >= 24)
                                        {
                                            lobby.Time[0] = 0;
                                        }

                                        string weatherString = WeatherToForecast(lobby.Weather);

                                        foreach (Player player in Settings.LoggedInPlayers)
                                        {
                                            lock (player)
                                            {
                                                if (player.HasSharedData("InLobby") && player.GetSharedDataSafe<int>("InLobby") == lobby.lobbyId)
                                                {
                                                    if (player.HasSharedData("InLobby") && player.GetSharedDataSafe<int>("PlayerWeather") == -1)
                                                    {
                                                        player.TriggerEvent("SetTimeWeather", lobby.Time[0], lobby.Time[1], lobby.Time[2], lobby.Weather);
                                                        //player.SendChatMessage(Translator.Translate(player, "WEATHER_FORECAST_2", weatherString));
                                                    }
                                                    else
                                                    {
                                                        player.TriggerEvent("SetTimeWeather", lobby.Time[0], lobby.Time[1], lobby.Time[2], player.GetSharedDataSafe<int>("PlayerWeather"));
                                                    }


                                                }
                                            }
                                        }
                                    }

                                    foreach (Player player in Settings.LoggedInPlayers)
                                    {
                                        lock (player)
                                        {
                                            if (player.HasSharedData("InLobby") && player.GetSharedDataSafe<int>("InLobby") == lobby.lobbyId)
                                            {
                                                if (player.GetSharedDataSafe<int>("PlayerWeather") == -1)
                                                    player.TriggerEvent("SetTimeWeather", lobby.Time[0], lobby.Time[1], lobby.Time[2], lobby.Weather);
                                                else player.TriggerEvent("SetTimeWeather", lobby.Time[0], lobby.Time[1], lobby.Time[2], player.GetSharedDataSafe<int>("PlayerWeather"));
                                            }
                                        }
                                    }
                                }

                                if (lobby.gameStatus == 0)
                                {
                                    if (lobby.Enabled)
                                    {
                                        if (lobby.GameAutoStart)
                                        {
                                            int pCount = 0;
                                            foreach (Player player in Settings.LoggedInPlayers)
                                            {
                                                lock (player)
                                                {
                                                    if(player != null)
                                                    {
                                                        if (player.HasSharedData("InLobby") && player.GetSharedDataSafe<int>("InLobby") == lobby.lobbyId && player.GetSharedDataSafe<int>("InDM") == -1 && player.GetSharedDataSafe<bool>("AutoJoin") == true && !EPlayer.IsAFK(player))
                                                        {
                                                            pCount++;
                                                        }
                                                    }
                                                 
                                                }
                                            }

                                            if (pCount >= 2)
                                            {
                                                lobby.gameStatus = 2;

                                                if (lobby.gameMode == GameModes.STANDARD)
                                                    EPlayer.SendMessageToAll("COPCHASE_START", lobby.lobbyId, pCount);
                                                else if (lobby.gameMode == GameModes.BUSTED)
                                                    EPlayer.SendMessageToAll("BUSTED_START", lobby.lobbyId, pCount);
                                                else if (lobby.gameMode == GameModes.BANKROBBERY)
                                                    EPlayer.SendMessageToAll("BANKROB_START", lobby.lobbyId, pCount);

                                                NAPI.Util.ConsoleOutput($"[TimeUpdate] Game starting in 10 seconds in lobby {lobby.lobbyId}...");
                                                foreach (Player player in Settings.LoggedInPlayers)
                                                {
                                                    lock (player)
                                                    {
                                                        if (player.GetSharedDataSafe<int>("InLobby") == lobby.lobbyId && player.GetSharedDataSafe<bool>("AutoJoin") == true)
                                                        {
                                                            if (player.GetSharedDataSafe<int>("InLobby") < 0 || player.GetSharedDataSafe<int>("InDM") != -1)
                                                            {
                                                                player.TriggerEvent("PlaySoundFrontend", "10s", "MP_MISSION_COUNTDOWN_SOUNDSET");
                                                                EPlayer.Notify(player, Translate(player, "ROUND_STARTING_GO_LOBBY"), "orange", "fas fa-hourglass-start", false, 10000, "topCenter");
                                                            }
                                                            else
                                                            {
                                                                if (player.HasData("Spectating"))
                                                                {
                                                                    Commands.cmd_specoff(player);
                                                                }
                                                                if (player.GetSharedDataSafe<bool>("global.WeaponEditorActive"))
                                                                {
                                                                    player.TriggerEvent("WeaponEditor:exit");
                                                                }
                                                                player.SetSharedData("InDM", -1);

                                                                EPlayer.Spawn(player);
                                                                player.TriggerEvent("PlaySoundFrontend", "10s", "MP_MISSION_COUNTDOWN_SOUNDSET");
                                                                EPlayer.Notify(player, Translate(player, "ROUND_STARTING"), "orange", "fas fa-hourglass-start", false, 10000, "topCenter");

                                                                if (player.GetSharedDataSafe<bool>("IsTabbed") == true || EPlayer.IsAFK(player))
                                                                {
                                                                    player.TriggerEvent("SendNotification", "AFK Check", "Chase is starting in 10 seconds",
                                                                        "Race 'N' Chase", 10);
                                                                }
                                                            }

                                                        }
                                                    }
                                                }

                                                NAPI.Task.Run(() =>
                                                {
                                                    GameStartCountdown(10, lobby.lobbyId);
                                                }, delayTime: 2000);
                                            }
                                            else
                                            {
                                                if (Environment.TickCount64 - lobby.LastAutoStartMsg >= 90000)
                                                {
                                                    EPlayer.SendMessageInLobby(lobby.lobbyId, "LOBBY_MIN_PLAYERS_MSG");

                                                    lobby.LastAutoStartMsg = Environment.TickCount64;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        foreach (Player player in Settings.LoggedInPlayers)
                                        {
                                            lock (player)
                                            {
                                                if (player.GetSharedDataSafe<int>("InLobby") == lobby.lobbyId)
                                                {
                                                    player.SetSharedData("InLobby", -1);
                                                    player.SendChatMessage(Translate(player, "LOBBY_DISABLED_SENDING_ANOTHER"));
                                                    EPlayer.Spawn(player);
                                                }
                                            }
                                        }
                                    }
                                }
                                else if (lobby.gameStatus == 1)
                                {
                                    if (lobby.gameMode != GameModes.BANKROBBERY)
                                    {
                                        if (lobby.Cops.Count < 1)
                                        {
                                            StopGame(lobby.lobbyId, 2);
                                        }
                                        else if (lobby.Fugitives.Count < 1)
                                        {
                                            StopGame(lobby.lobbyId, 3);
                                        }

                                        int woundedCops = lobby.Cops.Where(c => c.GetSharedDataSafe<int>("Wounded") > 0).ToList().Count;
                                        if (woundedCops >= lobby.Cops.Count)
                                        {
                                            StopGame(lobby.lobbyId, 2);
                                        }
                                        int woundedFugs = lobby.Fugitives.Where(c => c.GetSharedDataSafe<int>("Wounded") > 0).ToList().Count;
                                        if (woundedFugs >= lobby.Fugitives.Count)
                                        {
                                            StopGame(lobby.lobbyId, 3);
                                        }

                                        if (lobby.Fugitives.Count > 0)
                                        {
                                            List<Player> driverFugis = lobby.Fugitives.Where(f => f.IsInVehicle && f.VehicleSeat == 0).ToList();
                                            if (driverFugis.Count <= 0) { driverFugis.Add(lobby.Fugitives[new Random().Next(lobby.Fugitives.Count)]); }

                                            List<KeyValuePair<double, double>> fugiPoses = new List<KeyValuePair<double, double>>();
                                            foreach (Player fugitive in lobby.Fugitives)
                                            {
                                                fugiPoses.Add(new KeyValuePair<double, double>(fugitive.Position.X, fugitive.Position.Y));
                                            }

                                            foreach (Player cop in lobby.Cops)
                                            {
                                                cop.TriggerEvent("PoliceHeliCam_ClearFugitives");
                                                foreach (KeyValuePair<double, double> pos in fugiPoses)
                                                {
                                                    cop.TriggerEvent("PoliceHeliCam_AddFugitivePosition", pos.Key, pos.Value);
                                                }

                                                cop.TriggerEvent("MDC:RemoveBlipsOfType", "fugitivezone");
                                                foreach (Player driver in driverFugis)
                                                {
                                                    Vector3 randomPos = driver.Position;
                                                    randomPos.X += new Random().Next(-250, 250);
                                                    randomPos.Y += new Random().Next(-250, 250);
                                                    cop.TriggerEvent("MDC:AddBlip", driver.Id, "fugitivezone");
                                                    cop.TriggerEvent("MDC:UpdateBlip", driver.Id, "fugitivezone", randomPos.X, randomPos.Y);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                //if (Discord._client != null && Settings.Dynamics.Discord && (int)Discord._client.ConnectionState == 2)
                //{
                //    System.Threading.Tasks.Task.Run(() =>
                //    {
                //        // Update discord bot status
                //        if (Environment.TickCount64 - Settings.LastCountUpdate >= 60000)
                //        {
                //            Settings.LastCountUpdate = Environment.TickCount64;
                //            Discord._client.SetGameAsync($"{RAGE.Entities.Players.Count} Players & {Settings.Lobbies.Where(l => l.gameStatus == 1).Count()} Chases");
                //            //NAPI.Util.ConsoleOutput("Updated the DISCORD counts.");
                //        }
                //    });
                //}
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput("[EXCEPTION IN TimeUpdate]: " + ex.ToString());
            }
            return;
        }

        public static void CheckDerbyWinner()
        {
            int derbyPlayers = Settings.LoggedInPlayers.Where(a => a.GetSharedDataSafe<int>("InLobby") == -4).Count();
            if (Settings.DerbyTimeLeft <= 0 && derbyPlayers >= 2)
            {
                Player winner = null;
                int lastBigTime = 0;
                int winXP = derbyPlayers * 2;

                foreach (Player player in Settings.LoggedInPlayers.Where(a => a.GetSharedDataSafe<int>("InLobby") == -4))
                {
                    if (player.GetSharedDataSafe<int>("DerbyTime") > lastBigTime)
                    {
                        lastBigTime = player.GetSharedDataSafe<int>("DerbyTime");
                        winner = player;
                    }
                }

                foreach (Player player in Settings.LoggedInPlayers.Where(a => a.GetSharedDataSafe<int>("InLobby") == -4))
                {
                    player.Health = 0;
                    if (winner == null)
                    {
                        player.SendChatMessage(Translate(player, "DERBY_WINNER_NONE"));
                    }
                    else
                    {
                        player.SendChatMessage(Translate(player, "DERBY_WINNER", EPlayer.Name(winner), winXP));
                    }
                }

                if (winner != null)
                {
                    EPlayer.GetAccount(winner).DerbyWins++;
                    EPlayer.GrantXP(winner, winXP); DiscordBot.SendChatMessage(DiscordBot.ChatMessageType.General, $"**{winner.Name}** has won the Derby round! | XP Gained: {winXP} | Participants: {Settings.LoggedInPlayers.Where(a => a.GetSharedDataSafe<int>("InLobby") == -4).Count()}", $"Derby");
                    ////Discord.SendTextChannel("staff-bot", $"**(Derby) | Winner: {winner.Name} | XP Gained: {winXP} | Participants: {Settings.LoggedInPlayers.Where(a => a.GetSharedDataSafe<int>("InLobby") == -4).Count()}**");

                    winner.SetSharedData("FugitiveChance", winner.GetSharedDataSafe<float>("FugitiveChance") + 5 + (double)((double)EPlayer.GetAccount(winner).DonatorLevel / 2.0));

                    EPlayer.UnlockAchievement(winner, "first_derby");
                    EPlayer.TryAdvanceChallenge(winner, Challenge.ChallengeType.Win, "derby");
                }

                Settings.DerbyTimeLeft = 180;
                Settings.LastDerbyTick = Environment.TickCount64 + 3000;
            }
            return;
        }
        public static bool StopGame(int lobbyid, int reason)
        {
            LobbyData lobby = Settings.Lobbies.Find(l => l.lobbyId == lobbyid);
            Stinger.ClearStingers(lobby);


            if (lobby.gameMode == GameModes.BANKROBBERY && lobby.BankRobbery.BankRobberyFinishMarker != null)
            {
                NAPI.ColShape.DeleteColShape(lobby.BankRobbery.BankRobberyFinishMarker);
            }

            foreach (Blip blip in lobby.ParkedVehicleBlips)
            {
                blip.Delete();
            }
            lobby.ParkedVehicleBlips.Clear();

            int winnerTeam = -1;
            if (lobby != null && lobby.gameStatus == 1)
            {
                switch (reason)
                {
                    case 0:
                        EPlayer.SendMessageToAll("GAME_STOP_ADMIN", lobbyid);
                        DiscordBot.SendChatMessage(DiscordBot.ChatMessageType.General, "Copchase has ended! | Stopped by an admin.", $"Lobby {lobbyid}");
                        //Discord.SendTextChannel("staff-bot", $"**(Lobby {lobbyid}) Copchase has ended! | Stopped by an admin.**");
                        break;
                    case 1: // time run out | fugi escape shit
                        if (lobby.gameMode != GameModes.BANKROBBERY)
                        {
                            EPlayer.SendMessageToAll("GAME_STOP_FUGITIVE_ESCAPE", lobbyid);
                            DiscordBot.SendChatMessage(DiscordBot.ChatMessageType.General, "Copchase has ended! | Fugitives have escaped.", $"Lobby {lobbyid}");
                            //Discord.SendTextChannel("staff-bot", $"**(Lobby {lobbyid}) Copchase has ended! | Fugitives have escaped.**");
                            winnerTeam = 0;
                            foreach (Player i in lobby.Fugitives)
                            {
                                if (lobby.GameProgress <= 40) continue;
                                // Reward
                                EPlayer.GetAccount(i).Escapes++;
                                EPlayer.GrantXP(i, 5);
                                EPlayer.GivePoints(i, 5);

                                if(i.HasSharedData("FugitiveChance"))
                                    i.SetSharedData("FugitiveChance", i.GetSharedDataSafe<float>("FugitiveChance") + 0.3);
                            }
                            EPlayer.UnlockAchievement(lobby.BankRobbery.HeistLeader, "heist_leader_win");

                        }
                        else
                        {
                            if (lobby.gameMode == GameModes.BANKROBBERY)
                            {
                                winnerTeam = 1;
                                EPlayer.SendMessageToAll("GAME_STOP_FUGITIVE_BANKROB_FAIL", lobbyid);
                                DiscordBot.SendChatMessage(DiscordBot.ChatMessageType.General, "Copchase has ended! | Fugitives failed to rob the bank in time.", $"Lobby {lobbyid}");
                                //Discord.SendTextChannel("staff-bot", $"**(Lobby {lobbyid}) Copchase has ended! | Fugitives didn't manage to rob the bank in time.**");

                                foreach (Player i in lobby.Cops)
                                {
                                    if (lobby.GameProgress <= 40) continue;
                                    // Reward
                                    EPlayer.GrantXP(i, 8);
                                    EPlayer.GivePoints(i, 7);
                                    i.SetSharedData("FugitiveChance", i.GetSharedDataSafe<float>("FugitiveChance") + 0.5);
                                    EPlayer.UnlockAchievement(i, "first_bankrob_cop");
                                }
                            }
                        }
                        break;
                    case 2:
                        EPlayer.SendMessageToAll("GAME_STOP_COPS_DEAD", lobbyid);
                        DiscordBot.SendChatMessage(DiscordBot.ChatMessageType.General, "Copchase has ended! | All cops have been eliminated.", $"Lobby {lobbyid}");
                        //Discord.SendTextChannel("staff-bot", $"**(Lobby {lobbyid}) Copchase has ended! | All cops have been eliminated.**");

                        if (lobby.gameMode != GameModes.BANKROBBERY)
                        {
                            winnerTeam = 0;
                            foreach (Player i in lobby.Fugitives)
                            {
                                if (lobby.GameProgress <= 40) continue;
                                // Reward
                                EPlayer.GrantXP(i, 3);
                                EPlayer.GivePoints(i, 2);
                                i.SetSharedData("FugitiveChance", i.GetSharedDataSafe<float>("FugitiveChance") + 0.2);
                            }
                        }
                        else
                        {
                            if (lobby.gameMode == GameModes.BANKROBBERY)
                            {
                                winnerTeam = 0;
                                foreach (Player i in lobby.Fugitives)
                                {
                                    // Reward
                                    EPlayer.GrantXP(i, 8);
                                    EPlayer.GivePoints(i, 7);
                                    i.SetSharedData("FugitiveChance", i.GetSharedDataSafe<float>("FugitiveChance") + 0.1);

                                    EPlayer.UnlockAchievement(i, "first_bankrob_fugitive");

                                    if (i.GetData<bool>("ShotsFired") == false)
                                        EPlayer.UnlockAchievement(i, "no_shooting_win");
                                }
                                EPlayer.UnlockAchievement(lobby.BankRobbery.HeistLeader, "heist_leader_win");
                            }
                        }
                        break;
                    case 3:
                        EPlayer.SendMessageToAll("GAME_STOP_FUGITIVES_DEAD", lobbyid);
                        DiscordBot.SendChatMessage(DiscordBot.ChatMessageType.General, "Copchase has ended! | Police has neutralized all fugitives.", $"Lobby {lobbyid}");
                        //Discord.SendTextChannel("staff-bot", $"**(Lobby {lobbyid}) Copchase has ended! | Police has neutralized all fugitives.**");

                        if (lobby.gameMode != GameModes.BANKROBBERY)
                        {
                            winnerTeam = 1;
                            foreach (Player i in lobby.Cops)
                            {
                                if (lobby.GameProgress <= 40) continue;
                                // Reward
                                EPlayer.GrantXP(i, 4);
                                EPlayer.GivePoints(i, 2);
                                i.SetSharedData("FugitiveChance", i.GetSharedDataSafe<float>("FugitiveChance") + 0.3);
                            }
                        }
                        else
                        {
                            if (lobby.gameMode == GameModes.BANKROBBERY)
                            {
                                winnerTeam = 1;
                                foreach (Player i in lobby.Cops)
                                {
                                    // Reward
                                    EPlayer.GrantXP(i, 8);
                                    EPlayer.GivePoints(i, 7);
                                    i.SetSharedData("FugitiveChance", i.GetSharedDataSafe<float>("FugitiveChance") + 0.5);
                                    EPlayer.UnlockAchievement(i, "first_bankrob_cop");
                                }
                            }
                        }
                        break;
                    case 4:
                        EPlayer.SendMessageToAll("GAME_STOP_NO_PLAYERS", lobbyid);
                        DiscordBot.SendChatMessage(DiscordBot.ChatMessageType.General, "Copchase has ended! | Lack of players.", $"Lobby {lobbyid}");
                        //Discord.SendTextChannel("staff-bot", $"**(Lobby {lobbyid}) Copchase has ended! | Game was ended due to lack of players.**");
                        break;
                    case 5: // event cop win
                        EPlayer.SendMessageToAll("GAME_STOP_BANKROB_NO_LEADER", lobbyid);
                        DiscordBot.SendChatMessage(DiscordBot.ChatMessageType.General, "Copchase has ended! | No one was left to replace the heist leader.", $"Lobby {lobbyid}");
                        //Discord.SendTextChannel("staff-bot", $"**(Lobby {lobbyid}) Copchase has ended! | No one was left to replace the heist leader.**");
                        winnerTeam = 1;
                        foreach (Player i in lobby.Cops)
                        {
                            // Reward
                            EPlayer.GrantXP(i, 8);
                            EPlayer.GivePoints(i, 6);
                            i.SetSharedData("FugitiveChance", i.GetSharedDataSafe<float>("FugitiveChance") + 0.5);
                            EPlayer.UnlockAchievement(i, "first_bankrob_cop");
                        }
                        break;
                    case 6: // event fugi win
                        if (lobby.gameMode == GameModes.BANKROBBERY)
                        {
                            winnerTeam = 0;
                            EPlayer.SendMessageToAll("GAME_STOP_BANKROB_FUGI_WIN", lobbyid);
                            DiscordBot.SendChatMessage(DiscordBot.ChatMessageType.General, "Copchase has ended! | Fugitives successfully delivered the money to their hideout.", $"Lobby {lobbyid}");
                            //Discord.SendTextChannel("staff-bot", $"**(Lobby {lobbyid}) Copchase has ended! | Fugitives successfully delivered the money to their hideout.**");
                            foreach (Player i in lobby.Fugitives)
                            {
                                // Reward
                                EPlayer.GrantXP(i, 10);
                                EPlayer.GivePoints(i, 13);
                                i.SetSharedData("FugitiveChance", i.GetSharedDataSafe<float>("FugitiveChance") + 0.2);
                                EPlayer.UnlockAchievement(i, "first_bankrob_fugitive");

                                if (i.GetData<bool>("ShotsFired") == false)
                                    EPlayer.UnlockAchievement(i, "no_shooting_win");
                            }
                            EPlayer.UnlockAchievement(lobby.BankRobbery.HeistLeader, "heist_leader_win");
                        }
                        break;
                }

                lobby.gameStatus = 0;

                lobby.FirstKill = new KeyValuePair<Player, Player>(null, null);

                if (winnerTeam == 0 && lobby.Fugitives.Where(c => c.GetSharedDataSafe<int>("Wounded") == 0).ToList().Count == 1)
                {
                    Player last = lobby.Fugitives.FirstOrDefault(c => c.GetSharedDataSafe<int>("Wounded") == 0);
                    if (last != null)
                        EPlayer.UnlockAchievement(last, "last_man_standing");
                }
                if (winnerTeam == 1 && lobby.Cops.Where(c => c.GetSharedDataSafe<int>("Wounded") == 0).ToList().Count == 1)
                {
                    Player last = lobby.Cops.FirstOrDefault(c => c.GetSharedDataSafe<int>("Wounded") == 0);
                    if (last != null)
                        EPlayer.UnlockAchievement(last, "last_man_standing");
                }


                foreach (Player i in Settings.LoggedInPlayers)
                {
                    EPlayer.RemovePlayerFromCopchase(i, lobby, winnerTeam, reason);
                }

                for (int i = lobby.Vehicles.Count - 1; i >= 0; i--)
                {
                    if (!lobby.Vehicles[i].IsNull && lobby.Vehicles[i] != null)
                    {
                        lobby.Vehicles[i].Delete();
                        lobby.Vehicles.RemoveAt(i);
                    }
                }

                if (lobby.Vehicles.Count > 0)
                {
                    for (int i = 0; i < lobby.Vehicles.Count; i++)
                    {
                        lobby.Vehicles.RemoveAt(i);
                    }
                }

                for (int i = lobby.Drops.Count - 1; i >= 0; i--)
                {
                    if (lobby.Drops[i] != null)
                    {
                        lobby.Drops[i].Destroy();
                        lobby.Drops.RemoveAt(i);
                    }
                }

                if (lobby.Drops.Count > 0)
                {
                    for (int i = 0; i < lobby.Drops.Count; i++)
                    {
                        lobby.Drops.RemoveAt(i);
                    }
                }

                lobby.Cops.Clear();
                lobby.Fugitives.Clear();
                lobby.Vehicles.Clear();
                lobby.Drops.Clear();

                if (lobby.gameMode == GameModes.BANKROBBERY)
                {
                    lobby.gameMode = GameModes.STANDARD;
                    lobby.BankRobbery = new BankRobberyData();
                }
            }
            else return false;

            return true;
        }


        public static void StartEvent_BankRobbery(LobbyData lobby, List<Player> participants)
        {
            if (lobby == null || participants.Count == 0) return;
            if(participants.Count < 3)
            {
                NAPI.Util.ConsoleOutput($"[Lobby {lobby.lobbyId}] Bank Robbery could not be started - at least 4 players are required!");
                return;
            }

            int fugitives = GetFugitivesCount(lobby, participants.Count);
            if(fugitives < 2) // At least 2 fugitives are required for the Bank Robbery mode!
            {
                NAPI.Util.ConsoleOutput($"[Lobby {lobby.lobbyId}] Bank Robbery could not be started - min. 2 fugitives are required!");
                return;
            }

            // Get fugitive list -> "GrantFugitive" have priority, then order by fugitive chance
            List<Player> fugitiveListOrdered = participants.OrderByDescending(p => p.HasData("GrantFugitive"))
                                                              .ThenByDescending(p => p.GetSharedDataSafe<float>("FugitiveChance"))
                                                              .ToList();

            int infiniteLoopKillswitch = 0;
            // Adding fugitives to the Fugitive list in lobby and resetting their data
            while (fugitives > 0)
            {
                if (infiniteLoopKillswitch > 1000) { NAPI.Util.ConsoleOutput($"[Lobby {lobby.lobbyId}] Stuck picking fugitives (1000 loops), breaking loop."); break; }
                infiniteLoopKillswitch++;
                if (fugitiveListOrdered.Count == 0) break;

                Player nextFugitive = fugitiveListOrdered.First();
                if (lobby.Fugitives.Contains(nextFugitive)) continue;

                lobby.Fugitives.Add(nextFugitive);
                fugitiveListOrdered.Remove(nextFugitive);
                fugitives--;
            }
            fugitives = lobby.Fugitives.Count;

            // Setting cops to all players who aren't in fugitives, aka the rest
            lobby.Cops = participants.Where(p => !lobby.Fugitives.Contains(p)).ToList();


            // Setting up custom BANK ROBBERY vehicles
            lobby.BankRobbery.FugitiveVehicles.Clear();
            lobby.BankRobbery.PoliceVehicles.Clear();
            lobby.BankRobbery = new BankRobberyData();

            for(int i = 0; i < BankRobberyData.FugitiveVehicleData.Length; i++)
            {
                BankRobberyData.BR_VehicleData vehData = BankRobberyData.FugitiveVehicleData[i];

                Vehicle fugiVehicle = CreateGameVehicle(vehData.Name, vehData.Position, vehData.Heading, lobby, false, true, "Fast", 0, true, 5.0f);
                lobby.BankRobbery.FugitiveVehicles.Add(fugiVehicle);
            }

            int totalSeats = 0;
            bool isBearcatActive = false;
            if(lobby.Cops.Count > 4) // is more than 4 cops are in bank robbery, spawn in a bearcat to be filled and we can have some patrol cars
            {
                totalSeats = 4;
                isBearcatActive = true;
                BankRobberyData.BR_VehicleData vehData = BankRobberyData.PoliceVehicleData[BankRobberyData.PoliceVehicleData.Length-1];

                Vehicle bearcatVehicle = CreateGameVehicle(vehData.Name, vehData.Position, vehData.Heading, lobby, true, false, "Fast+", 1, true, 20.0f, 0, 111);
                lobby.BankRobbery.PoliceVehicles.Add(bearcatVehicle);
            }


            int currentCopVehicle = 0;
            while(totalSeats < lobby.Cops.Count)
            {
                BankRobberyData.BR_VehicleData vehData = BankRobberyData.PoliceVehicleData[currentCopVehicle];
                currentCopVehicle++;
                if (currentCopVehicle >= BankRobberyData.PoliceVehicleData.Length-1) currentCopVehicle = 0; // check all patrols except BCAT

                Vehicle policeVehicle = CreateGameVehicle(vehData.Name, vehData.Position.Around(3.0f), vehData.Heading, lobby, true, false, "Fast+", 1, true, 10.0f, 111, 0);
                lobby.BankRobbery.PoliceVehicles.Add(policeVehicle);

                totalSeats += 2;

            }

            // Selecting a heist leader & co-leader
            Random randomSelect = new Random();
            int leaderIdx = 0;
            int coLeaderIdx = 0;
            leaderIdx = randomSelect.Next(0, lobby.Fugitives.Count);
            int leaderLoopKillswitch = 0;
            do
            {
                if (leaderLoopKillswitch > 1000) break;
                leaderLoopKillswitch++;
                coLeaderIdx = randomSelect.Next(0, lobby.Fugitives.Count);
            }
            while (coLeaderIdx == leaderIdx);

            Player leader = lobby.BankRobbery.HeistLeader = lobby.Fugitives[leaderIdx];
            Player coleader = lobby.BankRobbery.CoLeader = lobby.Fugitives[coLeaderIdx];

            leader.SetSharedData("HeistLeader", true);
            leader.Position = BankRobberyData.LeaderPos;
            leader.Heading = BankRobberyData.LeaderHeading;
            coleader.SetSharedData("HeistLeader", true);
            coleader.Position = BankRobberyData.CoLeaderPos;
            coleader.Heading = BankRobberyData.CoLeaderHeading;

            // Variables to keep track of how many cops are spawned per vehicle
            int leftoverSeatsInCurrentCar = 2;
            int currentPoliceCarIndex = 0;
            int seatToPutIn = 0;

            int fugiSpawnToPick = 0;

            foreach (Player player in participants)
            {
                if (!lobby.Fugitives.Contains(player) && !lobby.Cops.Contains(player)) continue;
                EPlayer.ResetCopchaseVariables(player);
                

                player.RemoveAllWeaponsAC();
                player.Health = 100;
                player.Armor = 100;
                player.Dimension = Convert.ToUInt32(lobby.lobbyId);

                player.TriggerEvent("ToggleNametags", false);
                player.SetSharedData("Collisions", false);
                player.ResetData("Seatbelt");
                player.TriggerEvent("ToggleSeatbelt", false);

                EPlayer.Freeze(player, true);

                player.SetData("GPS", false);
                player.TriggerEvent("SetActorInvulnerable", false);
                player.TriggerEvent("ToggleAFOB", Settings.Dynamics.AntiFallOffBike, Settings.Dynamics.AntiFallOffBikeSpeedLimit);
                player.SetSharedData("CuffedBy", "n/a");
                player.ResetData("Handsup");
                player.ResetData("Cuffed");
                player.SetSharedData("HasShot", false);

                EPlayer.UpdateTeleports(player);

                if (lobby.Fugitives.Contains(player))
                {
                    player.TriggerEvent("EVENT:ToggleBankRobberyBeginningScreen", true, 0, lobby.Fugitives.Count);
                    Character.SetTeamOutfit(player, 0);
                    EPlayer.SetDiscordStatus(player, $"[Lobby {lobby.lobbyId}] Robbing bank as {player.Name}");
                    EPlayer.SetColor(player, new Color(255, 174, 0), 0);
                    player.SetSharedData("Team", 0);
                    player.SetSharedData("VisibleBlip", true);

                    player.SetSharedData("FugitiveChance", 10.0);

                    if(player != leader && player != coleader)
                    {
                        Vector3 spawnPos = (Vector3)BankRobberyData.FugitiveSpawns[fugiSpawnToPick, 0];
                        float spawnHeading = (float)BankRobberyData.FugitiveSpawns[fugiSpawnToPick, 1];
                        player.Position = spawnPos;
                        player.Heading = spawnHeading;

                        fugiSpawnToPick++;
                        if (fugiSpawnToPick >= BankRobberyData.FugitiveSpawns.Length) fugiSpawnToPick = 0;
                    }
                    continue;
                }
                if (lobby.Cops.Contains(player))
                {
                    player.TriggerEvent("EVENT:ToggleBankRobberyBeginningScreen", true, 1, lobby.Fugitives.Count);
                    Character.SetTeamOutfit(player, 1);
                    EPlayer.SetColor(player, new Color(100, 100, 255), 0);
                    EPlayer.SetDiscordStatus(player, $"[Lobby {lobby.lobbyId}] Cop as {player.Name} in Bank Robbery");
                    player.SetSharedData("Team", 1);

                    player.SetSharedData("FugitiveChance", player.GetSharedDataSafe<float>("FugitiveChance") + 1.0f);

                    player.SetData<bool>("BR_IsBearcatUnit", false);
                    if (leftoverSeatsInCurrentCar == 0)
                    {
                        seatToPutIn = 0;
                        leftoverSeatsInCurrentCar = 2;
                        currentPoliceCarIndex++;
                    }
                    if (isBearcatActive && currentPoliceCarIndex == 0) // since bearcat is the first car created, it's index 0
                    {
                        leftoverSeatsInCurrentCar = 4;
                        player.SetData<bool>("BR_IsBearcatUnit", true);
                    }

                    Vehicle toPutIn = lobby.BankRobbery.PoliceVehicles[currentPoliceCarIndex];
                    player.TriggerEvent("SetChaseVehicle", toPutIn.Id, seatToPutIn);
                    player.SetData("ChaseVehicle", toPutIn);
                    player.SetData("ChaseVehicleSeat", seatToPutIn);

                    leftoverSeatsInCurrentCar--;
                    seatToPutIn++;
                    continue;
                }
            }

            // ------------- Finalizing copchase variables ------------- //
            lobby.FugitiveShowCounter = 47;
            lobby.LastSeenCounter = -5;
            lobby.GameProgress = -5;
            lobby.CollisionCounter = 20;
            lobby.gameStatus = (int)LobbyData.GameStatus.Active;
            lobby.BankRobbery.GameState = BankRobberyData.State.Starting;

            int gameRunTime = 900;
            foreach (Player player in participants)
            {
                player.TriggerEvent("StartChaseTimer", -5, gameRunTime, lobby.CollisionCounter, (short)lobby.gameMode);
            }
        }

        public static string GenerateRandomVehiclePlate()
        {
            return $"SA-{new Random(DateTimeOffset.UtcNow.Millisecond).Next(1000, 9999)}";
        }
        public static bool IsCopchaseBanned(Player player)
        {
            bool isCopchaseBanned = false;
            if (EPlayer.GetAccount(player).CopchaseBanTime > 0)
            {
                long secs = EPlayer.GetAccount(player).CopchaseBanTime - DateTimeOffset.Now.ToUnixTimeSeconds();
                if (secs > 0)
                {
                    player.SendChatMessage(Translate(player, "BANNED_FROM_COPCHASE"));
                    isCopchaseBanned = true;
                }
                else
                {
                    EPlayer.GetAccount(player).CopchaseBanTime = 0;
                    EPlayer.Save(player, false);
                }
            }
            return isCopchaseBanned;
        }
        public static List<Player> GetPlayersForCopchase(LobbyData lobby)
        {
            int lobbyID = lobby.lobbyId;
            List<Player> CopchasePlayers = Settings.LoggedInPlayers.Where(p => p.GetSharedDataSafe<int>("InLobby") == lobbyID
                && !EPlayer.IsAFK(p)
                && p.GetSharedDataSafe<bool>("AutoJoin") == true
                && !p.Dead
                && !p.HasData("CHARACTER:EditorOpen")
                && p.GetSharedDataSafe<int>("InDM") == -1
                && p.GetSharedDataSafe<bool>("WeaponEditorActive") == false)
                    .OrderBy(c => Guid.NewGuid()).ToList();

            return CopchasePlayers;
        }
        public static int GetFugitivesCount(LobbyData lobby, int playerCount)
        {
            decimal perc = (decimal)0;
            if (lobby.gameMode != GameModes.BANKROBBERY) perc = (decimal)playerCount * ((decimal)lobby.FugitivePercent / (decimal)100);
            else
            {
                if (lobby.gameMode == GameModes.BANKROBBERY) perc = (decimal)playerCount * ((decimal)51.0 / (decimal)100);
            }

            int fugitives = (int)Decimal.Round(perc);

            
            if (fugitives == playerCount) fugitives--; // There will ALWAYS be AT LEAST one COP.


            return fugitives;
        }

        public static bool IsHelicopter(string carModel)
        {
            return NAPI.Vehicle.GetVehicleClass((VehicleHash)NAPI.Util.GetHashKey(carModel)) == 15 
                || carModel.Equals("polmav") 
                || carModel.Equals("as350");
        }

        public static Vehicle CreateGameVehicle(string modelName, Vector3 spawnPosition, float spawnHeading, LobbyData lobby, bool engineOn = false, bool isLocked = true, string speedClass = "Average", int team = -1, bool collisionsActive = true, float enginePower = 1.0f, int color1 = -1, int color2 = -2)
        {
            Vehicle pVehicle = NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(modelName), spawnPosition, spawnHeading, color1, color2,
                                                         GenerateRandomVehiclePlate(), 255, isLocked, engineOn, Convert.ToUInt32(lobby.lobbyId));

            pVehicle.EngineStatus = engineOn;
            pVehicle.SetSharedData("Engine", engineOn);
            pVehicle.SetSharedData("Fuel", 100.0);
            pVehicle.SetSharedData("Locked", isLocked);
            pVehicle.Locked = isLocked;
            pVehicle.SetSharedData("DirtLevel", 0.0);
            pVehicle.SetSharedData("Team", team);
            pVehicle.SetSharedData("SpeedInfo", speedClass);
            lobby.Vehicles.Add(pVehicle);
            pVehicle.SetSharedData("Collisions", collisionsActive);

            return pVehicle;
        }

        public static Vehicle CreatePoliceVehicle(Player owner, List<MapSpawn> policeSpawns, List<MapSpawn> heliSpawns, LobbyData lobby, int spawnIndex)
        {
            Account acc = EPlayer.GetAccount(owner);

            string carName = "police";
            int col1 = 0;
            int col2 = 111;
            float enginePower = 1.0f;
            string vehSpeedClass = "Average";

            Vector3 spawnPosition = null;
            float spawnHeading = 0.0f;

            // Class 15 = helicopters
            if (IsHelicopter(acc.CarCop))
            {
                if(spawnIndex >= heliSpawns.Count) spawnIndex = 0;
                spawnPosition = new Vector3(heliSpawns[spawnIndex].spawnX, heliSpawns[spawnIndex].spawnY, heliSpawns[spawnIndex].spawnZ);
                spawnHeading = heliSpawns[spawnIndex].spawnA;
            }
            else
            {
                if (spawnIndex >= policeSpawns.Count) spawnIndex = 0;

                spawnPosition = new Vector3(policeSpawns[spawnIndex].spawnX, policeSpawns[spawnIndex].spawnY, policeSpawns[spawnIndex].spawnZ);
                spawnHeading = policeSpawns[spawnIndex].spawnA;
            }

            if(acc.CarCop.Length < 3)
            {
                switch (new Random(DateTimeOffset.UtcNow.Millisecond).Next(0, 3))
                {
                    case 0:
                        carName = "police";
                        col1 = 111;
                        enginePower = 10.0f;
                        break;
                    case 1:
                        carName = "police3";
                        col1 = 111;
                        enginePower = 15.0f;
                        break;
                    case 2:
                        carName = "police4";
                        col1 = 111;
                        enginePower = 30.0f;
                        break;
                }
            }
            else
            {
                carName = acc.CarCop;
                // To add: increment times police car was used
                switch (carName)
                {
                    case "police4":
                        col1 = 8;
                        break;
                    case "lspdvic":
                        enginePower = 10.0f;
                        break;
                    case "fbi2":
                        enginePower = 15.0f;
                        break;
                    case "policet":
                        enginePower = 12.0f;
                        break;
                }
            }

            Vehicle pVehicle = NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(carName), spawnPosition, spawnHeading, col1, col2, owner.Name, 255, false, true, Convert.ToUInt32(lobby.lobbyId));
            pVehicle.EngineStatus = true;
            pVehicle.SetSharedData("Engine", true);
            pVehicle.SetSharedData("Fuel", 100.0);
            pVehicle.SetSharedData("Locked", true);
            pVehicle.Locked = true;
            pVehicle.SetSharedData("DirtLevel", 0.0);
            pVehicle.SetSharedData("Team", 1);
            pVehicle.SetSharedData("SpeedInfo", vehSpeedClass);

            pVehicle.SetSharedData("Collisions", false);
            if (enginePower > 1.0f) pVehicle.SetSharedData("vInitialDriveForce", enginePower);

            Dictionary<string, int> ammoTypes = new Dictionary<string, int>();
            ammoTypes.Add("AMMO_PISTOL", 40);
            ammoTypes.Add("AMMO_SMG", 30);
            ammoTypes.Add("AMMO_RIFLE", 20);
            ammoTypes.Add("AMMO_MG", 20);
            ammoTypes.Add("AMMO_SHOTGUN", 20);
            ammoTypes.Add("AMMO_BZGAS", 3);
            pVehicle.SetData("BackupAmmo", ammoTypes);

            Account _a = EPlayer.GetAccount(owner);
            uint vehModel = pVehicle.Model;
            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    var inst = await context.PlayerTunings.AsQueryable().FirstOrDefaultAsync(a => a.Model == vehModel && a.AccountSQLID == _a.SQL_ID);
                    if (inst != null)
                    {
                        NAPI.Task.Run(() =>
                        {
                            pVehicle.PrimaryColor = inst.PrimaryColor;
                            pVehicle.SecondaryColor = inst.SecondaryColor;

                            foreach (var kvp in NAPI.Util.FromJson<Dictionary<int, int>>(inst.Mods))
                            {
                                pVehicle.SetMod(kvp.Key, kvp.Value);
                                if (kvp.Key == 56) pVehicle.WheelType = kvp.Value;
                            }
                            pVehicle.PearlescentColor = inst.PearlescentColor;
                            pVehicle.WheelColor = inst.WheelColor;
                        });
                        await NAPI.Task.WaitForMainThread();
                    }
                }
            });

            lobby.Vehicles.Add(pVehicle);
            pVehicle.Position = spawnPosition;
            pVehicle.SetData("SpawnA", spawnHeading);
            pVehicle.Rotation.Z = spawnHeading;

            pVehicle.SetData<int>("LeftoverSeats", (GetMaxSeats(carName)-1));

            owner.TriggerEvent("SetChaseVehicle", pVehicle.Id, 0);
            owner.SetData("ChaseVehicle", pVehicle);
            owner.SetData("ChaseVehicleSeat", 0);

            return pVehicle;
        }
        public static Vehicle CreateFugitiveVehicle(Player owner, Vector3 spawnPosition, float spawnHeading, LobbyData lobby)
        {
            float enginePower = 1.0f;
            string carName = "washington";
            string vehSpeedClass = "Average";

            if (EPlayer.GetAccount(owner).CarFugitive.Length < 3)
            {
                switch (new Random(DateTimeOffset.UtcNow.Millisecond).Next(0, 5))
                {
                    case 0:
                        carName = "washington";
                        enginePower = 10.0f;
                        break;
                    case 1:
                        carName = "oracle";
                        enginePower = 10.0f;
                        break;
                    case 2:
                        carName = "premier";
                        enginePower = 10.0f;
                        break;
                    case 3:
                        carName = "rancherxl";
                        enginePower = 10.0f;
                        break;
                    case 4:
                        carName = "cavalcade2";
                        enginePower = 10.0f;
                        break;
                }
            }
            else
            {
                carName = EPlayer.GetAccount(owner).CarFugitive;

                Account pAcc = EPlayer.GetAccount(owner);
                // incrementing times this police vehicle was used
                Task.Run(async () =>
                {
                    using (MainDatabase context = ContextFactory.CreateContext())
                    {
                        if (await EPlayer.HasOwnedVehicle(pAcc, carName, 0))
                        {
                            OwnedVehData ownedVeh = await context.OwnedVehicles.AsQueryable().FirstOrDefaultAsync(v => v.AccountSQLID == EPlayer.GetAccount(owner).SQL_ID
                                                    && v.Model.Equals(carName, StringComparison.OrdinalIgnoreCase));
                            if (ownedVeh != null)
                                ownedVeh.TimesUsed++;

                            CarData cardata = Settings.Cars.FirstOrDefault(crd => crd.Model == carName && crd.Team == 0);
                            if (cardata != null)
                            {
                                EPlayer.TryAdvanceChallenge(owner, Challenge.ChallengeType.CopchaseVehicle, cardata.Name);
                                vehSpeedClass = cardata.Info;
                            }
                            else
                            {
                                if (carName != "pursport")
                                    vehSpeedClass = "Average";
                                else vehSpeedClass = "Fast+";
                            }
                        }
                    }
                });

            }

            Vehicle pVehicle = NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(carName), spawnPosition, spawnHeading,
                new Random(DateTimeOffset.UtcNow.Millisecond).Next(160), new Random(DateTimeOffset.UtcNow.Millisecond).Next(160),
                $"SA-{new Random(DateTimeOffset.UtcNow.Millisecond).Next(1000, 9999)}", 255, false, true, Convert.ToUInt32(lobby.lobbyId));

            pVehicle.EngineStatus = true;
            pVehicle.SetSharedData("Engine", true);
            pVehicle.SetSharedData("Fuel", 100.0);
            pVehicle.SetSharedData("Locked", false);
            pVehicle.Locked = false;
            pVehicle.SetSharedData("DirtLevel", 0.0);
            pVehicle.SetSharedData("Team", 0);
            pVehicle.SetSharedData("SpeedInfo", vehSpeedClass);
            lobby.Vehicles.Add(pVehicle);
            pVehicle.SetSharedData("Collisions", false);

            if (enginePower > 1.0f) pVehicle.SetSharedData("vInitialDriveForce", enginePower);

            Account _a = EPlayer.GetAccount(owner);
            uint vehModel = pVehicle.Model;
            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    var inst = await context.PlayerTunings.AsQueryable().FirstOrDefaultAsync(a => a.Model == vehModel && a.AccountSQLID == _a.SQL_ID);
                    if (inst != null)
                    {
                        NAPI.Task.Run(() =>
                        {
                            pVehicle.PrimaryColor = inst.PrimaryColor;
                            pVehicle.SecondaryColor = inst.SecondaryColor;

                            foreach (var kvp in NAPI.Util.FromJson<Dictionary<int, int>>(inst.Mods))
                            {
                                pVehicle.SetMod(kvp.Key, kvp.Value);
                                if (kvp.Key == 56) pVehicle.WheelType = kvp.Value;
                            }
                            pVehicle.PearlescentColor = inst.PearlescentColor;
                            pVehicle.WheelColor = inst.WheelColor;
                        });
                    }
                }
            });


            pVehicle.Position = spawnPosition;
            pVehicle.SetData("SpawnA", spawnHeading);
            pVehicle.Rotation.Z = spawnHeading;

            owner.TriggerEvent("SetChaseVehicle", pVehicle.Id, 0);
            owner.SetData("ChaseVehicle", pVehicle);
            owner.SetData("ChaseVehicleSeat", 0);
            owner.TriggerEvent("AFKDriverCheck", spawnPosition.X, spawnPosition.Y, spawnPosition.Z);

            return pVehicle;
        }

        public static void StartCopchase(LobbyData lobby, List<Player> CopchasePlayers, Map map, List<MapSpawn> copSpawns, List<MapSpawn> fugitiveSpawns, List<MapSpawn> heliSpawns)
        {
            if (lobby == null || CopchasePlayers.Count == 0) return;
            if (map == null)
            {
                NAPI.Util.ConsoleOutput($"[Lobby {lobby.lobbyId}] Failed to start copchase - no map found!");
                return;
            }
            if((copSpawns == null || copSpawns.Count == 0) || (fugitiveSpawns == null || fugitiveSpawns.Count == 0))
            {
                NAPI.Util.ConsoleOutput($"[Lobby {lobby.lobbyId}] Failed to start copchase - Map {map.Name} is missing spawns for both teams!");
                return;
            }
            int fugitives = GetFugitivesCount(lobby, CopchasePlayers.Count); // GetFugitivesCount calculates number of fugitives from %
            if(fugitives == 0)
            {
                NAPI.Util.ConsoleOutput($"[Lobby {lobby.lobbyId}] Failed to start copchase - not enough fugitives");
                return;
            }

            // Get fugitive list -> "GrantFugitive" have priority, then order by fugitive chance
            List<Player> fugitiveListOrdered = CopchasePlayers.OrderByDescending(p => p.HasData("GrantFugitive"))
                                                              .ThenByDescending(p => p.GetSharedDataSafe<float>("FugitiveChance"))
                                                              .ToList();

            int infiniteLoopKillswitch = 0;
            // Adding fugitives to the Fugitive list in lobby and resetting their data
            while(fugitives > 0)
            {
                if (infiniteLoopKillswitch > 1000) { NAPI.Util.ConsoleOutput($"[Lobby {lobby.lobbyId}] Stuck picking fugitives (1000 loops), breaking loop."); break; }                infiniteLoopKillswitch++;
                if (fugitiveListOrdered.Count == 0) break;

                Player nextFugitive = fugitiveListOrdered.First();
                if (lobby.Fugitives.Contains(nextFugitive)) continue;

                lobby.Fugitives.Add(nextFugitive);
                fugitiveListOrdered.Remove(nextFugitive);
                fugitives--;
            }
            fugitives = lobby.Fugitives.Count;

            // Setting cops to all players who aren't in fugitives, aka the rest
            lobby.Cops = CopchasePlayers.Where(p => !lobby.Fugitives.Contains(p)).ToList();


            int spawnLocCop = 0;
            int spawnLocFugitive = 0;
            int spawnLocHeli = 0;
            int currentFugiSeats = 0;
            Vehicle currentFugitiveCar = null;

            /*
             Brief explanation of how this works:
             When creating cop cars, the player's clan is checked. If they are in a clan, it will check if their clan is in the list.
             If their clan is FOUND in the list, it will assigned them to the "clan vehicle" aka partner them with the clan member.
             If their clan ISN'T FOUND in the list, it will create a new "clan vehicle" in the list for others to join if found.
             All this is done ONLY if they have Clan Copchase Partnering enabled.

             The "Squad Pairing" works in the same way, except it checks for the squad ID instead of clan ID.
             */
            List<KeyValuePair<int, Vehicle>> PairedClanVehicles = new List<KeyValuePair<int, Vehicle>>();

            foreach(Player player in CopchasePlayers)
            {
                if (!lobby.Fugitives.Contains(player) && !lobby.Cops.Contains(player)) continue;
                EPlayer.ResetCopchaseVariables(player);
                player.TriggerEvent("startLoadingScreen", -1, "Loading copchase match...");

                player.RemoveAllWeaponsAC();
                player.Health = 100;
                player.Armor = 0;
                player.Dimension = Convert.ToUInt32(lobby.lobbyId);

                player.TriggerEvent("ToggleNametags", false);
                player.SetSharedData("Collisions", false);
                player.ResetData("Seatbelt");
                player.TriggerEvent("ToggleSeatbelt", false);
                
                EPlayer.Freeze(player, true);

                player.SetData("GPS", false);
                player.TriggerEvent("SetActorInvulnerable", false);
                player.TriggerEvent("ToggleAFOB", Settings.Dynamics.AntiFallOffBike, Settings.Dynamics.AntiFallOffBikeSpeedLimit);
                player.SetSharedData("CuffedBy", "n/a");
                player.ResetData("Handsup");
                player.ResetData("Cuffed");
                player.SetSharedData("HasShot", false);

                EPlayer.UpdateTeleports(player);

                if (lobby.Fugitives.Contains(player))
                {
                    Character.SetTeamOutfit(player, 0);
                    EPlayer.SetDiscordStatus(player, $"[Lobby {lobby.lobbyId}] Fugitive as {player.Name}");
                    EPlayer.SetColor(player, new Color(255, 174, 0), 0);
                    player.SetSharedData("Team", 0);
                    player.SetSharedData("VisibleBlip", true);

                    player.SetSharedData("FugitiveChance", 10.0);

                    if (currentFugiSeats == 0)
                    {
                        MapSpawn spawn = fugitiveSpawns[spawnLocFugitive];
                        spawnLocFugitive++;
                        if (spawnLocFugitive >= fugitiveSpawns.Count) spawnLocFugitive = 0;

                        Vector3 spawnVector = new Vector3(spawn.spawnX, spawn.spawnY, spawn.spawnZ);
                        player.Position = spawnVector.Around(10.0f);

                        currentFugitiveCar = CreateFugitiveVehicle(player, spawnVector, spawn.spawnA, lobby);

                        // Setting leftover seats to Max-1 since one is already taken by the driver, pro maths

                        currentFugiSeats = (GetMaxSeats(EPlayer.GetAccount(player).CarFugitive)-1);

                        //if (GetMaxSeats(EPlayer.GetAccount(player).CarFugitive) != 0) currentFugiSeats = GetMaxSeats(EPlayer.GetAccount(player).CarFugitive)-1;
                        //else currentFugiSeats = currentFugitiveCar.MaxOccupants-1;
                    }
                    else
                    {
                        player.Position = currentFugitiveCar.Position.Around(10.0f);
                        player.TriggerEvent("SetChaseVehicle", currentFugitiveCar.Id, currentFugiSeats);
                        player.SetData("ChaseVehicle", currentFugitiveCar);
                        player.SetData("ChaseVehicleSeat", currentFugiSeats);

                        currentFugiSeats--;
                    }
                    continue;
                }
                if (lobby.Cops.Contains(player))
                {
                    Character.SetTeamOutfit(player, 1);
                    EPlayer.SetColor(player, new Color(0, 151, 255), 0);
                    EPlayer.SetDiscordStatus(player, $"[Lobby {lobby.lobbyId}] Cop as {player.Name}");
                    player.SetSharedData("Team", 1);

                    player.SetSharedData("FugitiveChance", player.GetSharedDataSafe<float>("FugitiveChance") + 1.0f);

                    int clanID = EPlayer.GetAccount(player).ClanID;
                    bool isClanPartneringEnabled = EPlayer.GetAccount(player).ClanPartnering;
                    string carModel = EPlayer.GetAccount(player).CarCop;

                    // Clan Pairing
                    if(clanID != -1 && isClanPartneringEnabled)
                    {
                        KeyValuePair<int, Vehicle> clanVehicle = PairedClanVehicles.FirstOrDefault(p => p.Key == clanID);
                        if(clanVehicle.Equals(default(KeyValuePair<int, Vehicle>))) // if null
                        {
                            carModel = EPlayer.GetAccount(player).CarCop;
                            Vehicle policeClanCar = null;
                            if (IsHelicopter(carModel)) // If helicopter, use the helicopter spawn index
                            {
                                policeClanCar = CreatePoliceVehicle(player, copSpawns, heliSpawns, lobby, spawnLocHeli);
                                spawnLocHeli++;
                                if (spawnLocHeli >= heliSpawns.Count) spawnLocHeli = 0;
                            }
                            else // Otherwise just use the normal car spawns
                            {
                                policeClanCar = CreatePoliceVehicle(player, copSpawns, heliSpawns, lobby, spawnLocCop);
                                spawnLocCop++;
                                if (spawnLocCop >= copSpawns.Count) spawnLocCop = 0;
                            }
                            player.Position = policeClanCar.Position.Around(10.0f);

                            PairedClanVehicles.Add(new KeyValuePair<int, Vehicle>(clanID, policeClanCar));
                            continue;
                        }
                        else
                        {
                            Vehicle clanCar = clanVehicle.Value;
                            int leftoverSeats = 0;
                            if (clanCar.HasData("LeftoverSeats"))
                                leftoverSeats = clanCar.GetData<int>("LeftoverSeats");

                            if(leftoverSeats == 0) // If no seats are found in the last clan car, create a new one
                            {
                                PairedClanVehicles.Remove(clanVehicle);

                                carModel = EPlayer.GetAccount(player).CarCop;
                                Vehicle policeClanCar = null;
                                if (IsHelicopter(carModel))
                                {
                                    policeClanCar = CreatePoliceVehicle(player, copSpawns, heliSpawns, lobby, spawnLocHeli);
                                    spawnLocHeli++;
                                    if (spawnLocHeli >= heliSpawns.Count) spawnLocHeli = 0;
                                }
                                else
                                {
                                    policeClanCar = CreatePoliceVehicle(player, copSpawns, heliSpawns, lobby, spawnLocCop);
                                    spawnLocCop++;
                                    if (spawnLocCop >= copSpawns.Count) spawnLocCop = 0;
                                }
                                player.Position = policeClanCar.Position.Around(10.0f);

                                PairedClanVehicles.Add(new KeyValuePair<int, Vehicle>(clanID, policeClanCar));
                                continue;
                            }
                            else // If there's more leftover seats, set player in the next free seat
                            {
                                player.Position = clanCar.Position.Around(10.0f);
                                player.TriggerEvent("SetChaseVehicle", clanCar.Id, leftoverSeats);
                                player.SetData("ChaseVehicle", clanCar);
                                player.SetData("ChaseVehicleSeat", leftoverSeats);

                                leftoverSeats--;
                                clanCar.SetData<int>("LeftoverSeats", leftoverSeats);
                                continue;
                            }
                        }
                    }

                    // To Add: Squad Pairing


                    // Normal car spawning, no partners
                    Vehicle noPartnerCar = null;
                    if (IsHelicopter(carModel))
                    {
                        noPartnerCar = CreatePoliceVehicle(player, copSpawns, heliSpawns, lobby, spawnLocHeli);
                        spawnLocHeli++;
                        if (spawnLocHeli >= heliSpawns.Count) spawnLocHeli = 0;
                    }
                    else
                    {
                        noPartnerCar = CreatePoliceVehicle(player, copSpawns, heliSpawns, lobby, spawnLocCop);
                        spawnLocCop++;
                        if (spawnLocCop >= copSpawns.Count) spawnLocCop = 0;
                    }
                    player.Position = noPartnerCar.Position.Around(10.0f);

                    continue;
                }
            }

            // ------------- Finalizing copchase variables ------------- //
            lobby.CurrentMap = map;
            lobby.FugitiveShowCounter = 47;
            lobby.LastSeenCounter = -5;
            lobby.GameProgress = -5;
            lobby.CollisionCounter = 48;
            lobby.EarlyFugitiveWarningSent = false;
            lobby.gameStatus = (int)LobbyData.GameStatus.Active;

            int gameRunTime = Settings.GameRunTime;
            foreach (Player player in CopchasePlayers)
            {
                player.TriggerEvent("StartChaseTimer", -5, gameRunTime, lobby.CollisionCounter, (short)lobby.gameMode);
            }

            NAPI.Util.ConsoleOutput($"[Lobby ${lobby.lobbyId}] Mode {ConvertGameMode(lobby.gameMode)} started with {lobby.Cops.Count} Cops & {lobby.Fugitives.Count} fugitives on map {(map == null ? "EVENT" : map.Name)}!");
            DiscordBot.SendChatMessage(DiscordBot.ChatMessageType.General, $"**[Lobby ${lobby.lobbyId}]** {ConvertGameMode(lobby.gameMode)} started with **{lobby.Cops.Count} Cops** & **{lobby.Fugitives.Count} fugitives** on map *{(map == null ? "EVENT" : map.Name)}*!");
        }

        public static void StartGameNew(int lobbyID)
        {
            try
            {
                LobbyData lobby = Settings.Lobbies.FirstOrDefault(l => l.lobbyId == lobbyID && l.Enabled == true);
                if (lobby == null) { NAPI.Util.ConsoleOutput("[Lobby " + lobbyID + "] Lobby is null, could not start the copchase round."); return; }
                if (lobby.gameStatus == 1) { NAPI.Util.ConsoleOutput("[Lobby " + lobbyID + "] Copchase is already running, failed to start"); return; }

                // Setting up the default variables
                lobby.gameStatus = (int)LobbyData.GameStatus.None;
                lobby.Cops.Clear();
                lobby.Fugitives.Clear();
                Stinger.ClearStingers(lobby);

                // Picking players for copchase - conditions under which they can join!
                int copchasePlayerCount = 0;
                List<Player> CopchasePlayers = GetPlayersForCopchase(lobby);

                // Setting up default variables & flagging banned copchase players
                List<Player> BannedCopchasePlayers = new List<Player>();
                foreach(Player player in CopchasePlayers)
                {
                    bool isCopchaseBanned = IsCopchaseBanned(player);
                    if (isCopchaseBanned)
                    {
                        BannedCopchasePlayers.Add(player);
                        continue;
                    }
                }
                CopchasePlayers.RemoveAll(player => BannedCopchasePlayers.Contains(player));

                copchasePlayerCount = CopchasePlayers.Count;
                if(copchasePlayerCount < 2) { NAPI.Util.ConsoleOutput("[Lobby " + lobbyID + "] Less than two copchase players in lobby, failed to start."); return; }

                // Checking if event - events go in their own functions for readability & organisation
                if (lobby.gameMode == GameModes.BANKROBBERY)
                {
                    StartEvent_BankRobbery(lobby, CopchasePlayers);
                    return;
                }
                // Handle stuff for normal gamemodes | Busted counts as a 'normal' gamemode
                if (lobby.gameMode == GameModes.STANDARD || lobby.gameMode == GameModes.BUSTED)
                {
                    // Do all database operations here, such as fetching game maps, spawns, etc. Then run StartCopchase on main thread
                    Task.Run(async () => 
                    {
                        using(MainDatabase context = ContextFactory.CreateContext())
                        {
                            List<Map> maps = await context.Maps.AsQueryable().Where(m => m.Enabled == true).ToListAsync();
                            Map mapToStart = null;
                            if(maps.Count == 0)
                            {
                                NAPI.Task.Run(() =>
                                {
                                    // If a map isn't found, set null as map so it doesn't proceed
                                    StartCopchase(lobby, CopchasePlayers, null, null, null, null);
                                });
                                return;
                            }

                            if (lobby.NextMapID != -1) mapToStart = maps.Find(m => m.SQL_ID == lobby.NextMapID);
                            lobby.NextMapID = -1;

                            if(mapToStart == null)
                                mapToStart = maps.OrderBy(c => Guid.NewGuid()).FirstOrDefault();

                            lobby.lastMapId = mapToStart.SQL_ID;
                            try
                            {
                                List<MapSpawn> mapSpawns = await context.MapSpawns.AsQueryable().Where(m => m.mapID == mapToStart.SQL_ID).ToListAsync();

                                List<MapSpawn> policeSpawns = mapSpawns.Where(s => s.spawnTeam == 1).ToList();
                                List<MapSpawn> fugitiveSpawns = mapSpawns.Where(s => s.spawnTeam == 0).ToList();
                                List<MapSpawn> heliSpawns = mapSpawns.Where(s => s.spawnTeam == 2).ToList();

                                NAPI.Task.Run(() =>
                                {
                                    // Once all database operations are complete, start copchase on main thread with all data you fetched
                                    StartCopchase(lobby, CopchasePlayers, mapToStart, policeSpawns, fugitiveSpawns, heliSpawns);
                                });
                            }
                            catch(Exception ex)
                            {
                                NAPI.Util.ConsoleOutput(ex.ToString());
                            }
                        }
                    });
                    return;
                }
         
            }
            catch(Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[STARTGAME - {lobbyID}] {ex.ToString()}");
            }
        }

        // Handles when and when not to show fugitive blips on the map - run every second
        private static void HandleFugitiveShowCounters(LobbyData lobby)
        {
            NAPI.Util.ConsoleOutput($"Fugitive Show Counter: {lobby.FugitiveShowCounter} | Last seen: {lobby.LastSeenCounter}");

            lobby.FugitiveShowCounter++;
            lobby.LastSeenCounter++;

            // Every 20 seconds, send a message to all cops about one fugitive's last seen location
            if (lobby.LastSeenCounter >= 20)
            {
                if (lobby.Fugitives.Count() > 0)
                {
                    try
                    {
                        Player fugi = lobby.Fugitives.OrderBy(x => Guid.NewGuid()).First();

                        foreach (var a in lobby.Cops)
                        {
                            try
                            {
                                if (a == null) continue;
                                a.TriggerEvent("ReportLastSeen", fugi.Name, fugi.Position.X, fugi.Position.Y, fugi.Position.Z);
                                NAPI.Util.ConsoleOutput($"Reporting last seen to {a.Name}");
                            }
                            catch { }
                        }
                    }
                    catch { }
                }
                lobby.LastSeenCounter = 0;
            }

            // BLIP HANDLING:
            // Show fugitives for 60 seconds when the counter is between 60 and 120
            if (lobby.FugitiveShowCounter >= 60 && lobby.FugitiveShowCounter < 120 && !lobby.AreFugitivesVisible)
            {
                lobby.AreFugitivesVisible = true;
                lock (lobby.Fugitives)
                {
                    if (lobby.Fugitives.Where(fg => fg.HasSharedData("VisibleBlip") && fg.GetSharedDataSafe<bool>("VisibleBlip") == false).ToList().Count > 0)
                    {
                        foreach (Player fugi in lobby.Fugitives)
                        {
                            fugi.SetSharedData("VisibleBlip", true);

                            if (lobby.FugitiveShowCounter == 60) EPlayer.Notify(fugi, Translate(fugi, "RADAR_VISIBLE"), "red", "fas fa-running", false, 3000);
                        }
                    }
                }
            }

            // At 120th second, reset the counter to 0 seconds. During this time, fugitives are invisible for 60 seconds
            if (lobby.FugitiveShowCounter >= 120)
            {
                lobby.FugitiveShowCounter = 0;
                lobby.AreFugitivesVisible = false;

                lock (lobby.Fugitives)
                {
                    foreach (Player fugi in lobby.Fugitives.Where(a => a.GetSharedDataSafe<bool>("HasShot") == false))
                    {
                        fugi.SetSharedData("VisibleBlip", false);

                        EPlayer.Notify(fugi, Translate(fugi, "RADAR_INVISIBLE"), "green", "fas fa-walking", false, 3000);
                        NAPI.Util.ConsoleOutput($"Reporting last seen to {fugi.Name}");
                    }
                }
            }
        }

        private static void SetCopchaseVehicleMaxSpeed(Vehicle vehicle)
        {
            if (vehicle == null) return;
            int team = vehicle.GetSharedDataSafe<int>("Team");
            string speedClass = vehicle.GetSharedDataSafe<string>("SpeedInfo");

            if (team == 0) // Fugitive vehicles
            {
                switch (speedClass)
                {
                    case "Slow":
                        vehicle.SetSharedData("vMaxSpeed", (float)(145 / 3.6));
                        break;
                    case "Slow+":
                        vehicle.SetSharedData("vMaxSpeed", (float)(158 / 3.6));
                        break;
                    case "Average":
                        vehicle.SetSharedData("vMaxSpeed", (float)(163 / 3.6));
                        break;
                    case "Average+":
                        vehicle.SetSharedData("vMaxSpeed", (float)(168 / 3.6));
                        break;
                    case "Fast":
                        vehicle.SetSharedData("vMaxSpeed", (float)(175 / 3.6));
                        break;
                    case "Fast+":
                        vehicle.SetSharedData("vMaxSpeed", (float)(180 / 3.6));
                        break;
                }
                return;
            }
            if(team == 1) // Police vehicles
            {
                switch (speedClass)
                {
                    case "Slow":
                        vehicle.SetSharedData("vMaxSpeed", (float)(180 / 3.6));
                        break;
                    case "Slow+":
                        vehicle.SetSharedData("vMaxSpeed", (float)(183 / 3.6));
                        break;
                    case "Average":
                        vehicle.SetSharedData("vMaxSpeed", (float)(185 / 3.6));
                        break;
                    case "Average+":
                        vehicle.SetSharedData("vMaxSpeed", (float)(188 / 3.6));
                        break;
                    case "Fast":
                        vehicle.SetSharedData("vMaxSpeed", (float)(190 / 3.6));
                        break;
                    case "Fast+":
                        vehicle.SetSharedData("vMaxSpeed", (float)(196 / 3.6));
                        break;
                }
                return;
            }
        }

        // Used in normal copchase, no need to use this in events such as bank robbery since they use custom logic
        private static void HandleCollisionCounters(LobbyData lobby)
        {
            int gameRunTime = Settings.GameRunTime;

            if (lobby.CollisionCounter > 0)
            {
                if (lobby.CollisionCounter % 5 == 0) // Every 5 seconds/multiple during collision phase
                {
                    foreach (Vehicle veh in lobby.Vehicles)
                    {
                        veh.SetSharedData("Collisions", false);
                    }
                }

                lobby.CollisionCounter--;

                if (lobby.CollisionCounter <= 0) // Collision timer ended => give loadouts, set healths, etc
                {
                    lock (Settings.LoggedInPlayers)
                    {

                        foreach (Player player in Settings.LoggedInPlayers.Where(p => p.GetSharedDataSafe<int>("InLobby") == lobby.lobbyId))
                        {
                            try
                            {
                                if (player == null) continue;

                                player.TriggerEvent("StartChaseTimer", lobby.GameProgress, gameRunTime, 0, (short)lobby.gameMode);

                                player.SetSharedData("BustedProgress", 0.0);

                                //////////////////////////////////////////////////////////////////////////////////////////
                                ///
                                if (!lobby.Cops.Contains(player) && !lobby.Fugitives.Contains(player)) continue;

                                EPlayer.Notify(player, Translate(player, "COLLISIONS_ENABLED"), "green", "fas fa-car-crash", false, 3000);

                                player.Health = 100;
                                player.Armor = 0;

                                player.SetSharedData("Collisions", true);
                                if (Settings.Dynamics.OnfootCollision == false) player.SetSharedData("Collisions", false);


                                if (lobby.Cops.Contains(player))
                                {
                                    player.Armor = 60;

                                    EPlayer.GiveCopWeapons(player);
                                    player.TriggerEvent("SetActorInvulnerable", false);
                                }
                                else if (lobby.Fugitives.Contains(player))
                                {
                                    player.Armor = 25;

                                    EPlayer.GiveFugitiveWeapons(player);
                                    player.TriggerEvent("SetActorInvulnerable", false);
                                }


                            }
                            catch { }
                        }
                        foreach (Vehicle veh in lobby.Vehicles)
                        {
                            veh.SetSharedData("Collisions", true);
                            SetCopchaseVehicleMaxSpeed(veh);

                        }
                    }
                }

                lock (lobby.Cops)
                {
                    foreach (Player cop in lobby.Cops)
                    {
                        try
                        {
                            if (cop == null) continue;

                            if (cop.GetData<bool>("GPS") == false)
                            {
                                cop.SetSharedData("VisibleBlip", false);
                            }
                            else
                            {
                                cop.SetSharedData("VisibleBlip", true);
                            }

                            if (cop.GetSharedDataSafe<bool>("ClimbProtection") == true && cop.GetData<long>("ClimbProtectionEnd") < Environment.TickCount64)
                            {
                                cop.SetSharedData("ClimbProtection", false);
                            }
                        }
                        catch { }
                    }
                }
                lock (lobby.Fugitives)
                {
                    foreach (Player fugi in lobby.Fugitives)
                    {
                        try
                        {
                            if (fugi == null) continue;

                            if (fugi.GetSharedDataSafe<bool>("ClimbProtection") == true && fugi.GetData<long>("ClimbProtectionEnd") < Environment.TickCount64)
                            {
                                fugi.SetSharedData("ClimbProtection", false);
                            }
                        }
                        catch { }
                    }
                }
            }
        }

        private static void CopchaseSecondTick(LobbyData lobby, int secondsElapsed)
        {
            NAPI.Util.ConsoleOutput($"Copchase second tick: {lobby.lobbyId} [secs: {secondsElapsed}] | {lobby.Cops.Count} fugs: {lobby.Fugitives.Count}");
            HandleFugitiveShowCounters(lobby);
            HandleCollisionCounters(lobby);

            List<Player> CopchasePlayers = new List<Player>();
            CopchasePlayers.AddRange(lobby.Fugitives);
            CopchasePlayers.AddRange(lobby.Cops);

            // Pre-game setups -> putting players into vehicles, toggling collisions, etc.
            if (secondsElapsed == -3)
            {
                foreach (Player player in CopchasePlayers)
                {
                    try
                    {
                        if (player == null) continue;

                        Vehicle playerVehicle = player.GetData<Vehicle>("ChaseVehicle");
                        int vehicleSeat = player.GetData<int>("ChaseVehicleSeat");

                        player.Dimension = Convert.ToUInt32(lobby.lobbyId);
                        player.SetIntoVehicle(playerVehicle, vehicleSeat);
                        EPlayer.Freeze(player, true);
                        player.TriggerEvent("stopLoadingScreen");

                        playerVehicle.SetSharedData("Collisions", false);

                        player.SetSharedData("Collisions", false);

                        if (lobby.Fugitives.Contains(player))
                        {
                            player.TriggerEvent("ToggleCollisionsForEnt", playerVehicle, false);

                            continue;
                        }
                        if (lobby.Cops.Contains(player))
                        {

                            continue;
                        }
                    }
                    catch { }
                }
                return;
            }

            // Actual game start -> unfreezing fugitives, etc.
            if(secondsElapsed == 0)
            {
                if (lobby.gameMode == GameModes.STANDARD)
                {
                    EPlayer.SendMessageToAll("COPCHASE_STARTED", lobby.lobbyId, lobby.Cops.Count, lobby.Fugitives.Count, lobby.CurrentMap.Name);
                }
                else if (lobby.gameMode == GameModes.BUSTED)
                {
                    EPlayer.SendMessageToAll("BUSTED_STARTED", lobby.lobbyId, lobby.Cops.Count, lobby.Fugitives.Count, lobby.CurrentMap.Name);
                }


                foreach (Player player in CopchasePlayers)
                {
                    try
                    {
                        if (player == null) continue;

                        Vehicle playerVehicle = player.GetData<Vehicle>("ChaseVehicle");
                        int vehicleSeat = player.GetData<int>("ChaseVehicleSeat");

                        player.SetIntoVehicle(playerVehicle, vehicleSeat);

                        player.TriggerEvent("ToggleNametags", true);
                        player.TriggerEvent("ToggleCanExitVehicle", false);

                        player.GetData<Vehicle>("ChaseVehicle").SetSharedData("Collisions", false);
                        player.SetSharedData("Collisions", false);
                        player.TriggerEvent("ToggleCollisionsForEnt", playerVehicle, false);

                        player.RemoveAllWeaponsAC();

                        player.Health = 100;
                        player.Armor = 0;

                        player.TriggerEvent("ResetKillFeed");

                        if (lobby.gameMode == GameModes.STANDARD)
                        {
                            player.SendChatMessage(Translate(player, "COPS_10S_UNFREEZE"));
                        }
                        else if (lobby.gameMode == GameModes.BUSTED)
                        {
                            player.SendChatMessage(Translate(player, "START_BUSTED_MSG_1"));
                            player.SendChatMessage(Translate(player, "START_BUSTED_MSG_2"));
                            player.SendChatMessage(Translate(player, "START_BUSTED_MSG_3"));
                            player.SendChatMessage(Translate(player, "START_BUSTED_MSG_4"));
                            player.SendChatMessage(Translate(player, "START_BUSTED_MSG_5"));
                        }

                        if (lobby.Fugitives.Contains(player))
                        {
                            EPlayer.Freeze(player, false);
                            continue;
                        }
                        if (lobby.Cops.Contains(player))
                        {
                            EPlayer.Notify(player, Translate(player, "UNFREEZE_10S_NOTIFY"), "orange", "fas fa-hourglass-start", false, 10000);
                            continue;
                        }
                    }
                    catch { }
                }
                foreach (Vehicle vehicle in lobby.Vehicles)
                {
                    if (vehicle == null) continue;

                    vehicle.SetSharedData("Collisions", false);
                    CopchasePlayers.ForEach((p) =>
                    {
                        if (p.Position.DistanceTo(vehicle.Position) < 500.0)
                        {
                            p.TriggerEvent("ToggleCollisionsForEnt", p.GetData<Vehicle>("ChaseVehicle"), false);
                        }
                    });
                }
                return;
            }

            // After 10 seconds -> unfreeze the cops, everyone can exit vehicles
            if(secondsElapsed == 10)
            {
                foreach(Player player in CopchasePlayers)
                {
                    player.SendNotification("~y~You can now exit vehicles.");
                    player.TriggerEvent("ToggleCanExitVehicle", true);
                    EPlayer.TryAdvanceChallenge(player, Challenge.ChallengeType.CopchaseRounds, "N/A");
                    EPlayer.Freeze(player, false);

                    if (player.IsInVehicle && player.Vehicle != null)
                    {
                        player.Vehicle.Repair();
                    }

                    if (lobby.Fugitives.Contains(player))
                    {
                        continue;
                    }
                    if (lobby.Cops.Contains(player))
                    {
                        player.SendNotification("~b~You may begin the chase now.");
                        continue;
                    }
                }
            }


            // 70% of the full game's runtime
            int seventyPercentSeconds = Convert.ToInt32(Settings.GameRunTime * 0.7);

            // 60 seconds before the 70% of the game -> used to send early warning to fugitives
            if(secondsElapsed ==  seventyPercentSeconds- 60 && lobby.gameMode != GameModes.BUSTED) 
            {
                if (!lobby.EarlyFugitiveWarningSent)
                {
                    lobby.EarlyFugitiveWarningSent = true;
                    foreach(Player player in lobby.Fugitives)
                    {
                        if (player.GetSharedDataSafe<bool>("HasShot") == false)
                        {
                            player.SendChatMessage(Translate(player, "CAN_BE_SHOT_LONG_GAME_EARLY_WARNING"));
                            player.TriggerEvent("startCopchaseTimerEarlyWarningEffect");
                        }
                    }
                }
            }

            // 70% of the game elapsed
            if(secondsElapsed == seventyPercentSeconds && lobby.gameMode != GameModes.BUSTED)
            {
                foreach(Player player in CopchasePlayers)
                {
                    if (lobby.Fugitives.Contains(player))
                    {
                        if (player.GetSharedDataSafe<bool>("HasShot") == false)
                        {
                            player.SetSharedData("HasShot", true);
                            player.SetSharedData("VisibleBlip", true);
                            lobby.FugitiveShowCounter = 29; // Set them to VISIBLE

                            player.SendChatMessage(Translate(player, "CAN_BE_SHOT_LONG_GAME"));
                        }
                    }
                    if (lobby.Cops.Contains(player))
                    {
                        player.SendChatMessage(Translate(player, "CAN_BE_SHOT_LONG_GAME_GLOBAL"));
                    }
                }
            }

            // Game ended - time expired
            if(secondsElapsed > Settings.GameRunTime)
            {
                StopGame(lobby.lobbyId, 1);
            }

        }

        public static bool BankRobbery_AnyLeadersAlive(LobbyData lobby)
        {
            int aliveLeaders = 0;
            if(lobby.BankRobbery.HeistLeader != null)
            {
                if (lobby.Fugitives.Contains(lobby.BankRobbery.HeistLeader) 
                    && !lobby.BankRobbery.HeistLeader.Dead 
                    && lobby.BankRobbery.HeistLeader.GetSharedDataSafe<int>("Team") == 0)
                {
                    aliveLeaders++;
                }
            }
            if (lobby.BankRobbery.CoLeader != null)
            {
                if (lobby.Fugitives.Contains(lobby.BankRobbery.CoLeader)
                    && !lobby.BankRobbery.CoLeader.Dead
                    && lobby.BankRobbery.CoLeader.GetSharedDataSafe<int>("Team") == 0)
                {
                    aliveLeaders++;
                }
            }

            if(aliveLeaders == 0)
            {
                // Stop game if no leaders are left alive
                //StopGame(lobby.lobbyId, 5);
                return false;
            }
            return true;
        }

        private static void BankRobberySecondTick(LobbyData lobby, int secondsElapsed)
        {
            List<Player> CopchasePlayers = new List<Player>();
            CopchasePlayers.AddRange(lobby.Fugitives);
            CopchasePlayers.AddRange(lobby.Cops);

            // Just placing cops into their vehicles at the beginning, game still not started. Setting dimension too.
            if (secondsElapsed == -3)
            {
                foreach (Player player in CopchasePlayers)
                {
                    try
                    {
                        if (player == null) continue;

                        player.Dimension = Convert.ToUInt32(lobby.lobbyId);
                        player.TriggerEvent("ToggleCanExitVehicle", true);
                        player.TriggerEvent("SetActorInvulnerable", false);
                        player.TriggerEvent("ToggleAFOB", Settings.Dynamics.AntiFallOffBike, Settings.Dynamics.AntiFallOffBikeSpeedLimit);

                        player.SetSharedData("CuffedBy", "n/a");
                        player.ResetData("Cuffed");
                        player.ResetData("Handsup");

                        if (lobby.Fugitives.Contains(player))
                        {
                            EPlayer.SetColor(player, new Color(255, 174, 0), 0);
                            player.SetSharedData("Team", 0);
                            player.SetSharedData("VisibleBlip", true);
                            player.SetSharedData("HasShot", true);
                            continue;
                        }
                        if (lobby.Cops.Contains(player))
                        {
                            EPlayer.SetColor(player, new Color(0, 151, 255), 0);
                            player.SetData("GPS", false);
                            player.SetSharedData("Team", 1);

                            Vehicle playerVehicle = player.GetData<Vehicle>("ChaseVehicle");
                            int vehicleSeat = player.GetData<int>("ChaseVehicleSeat");
                            player.SetIntoVehicle(playerVehicle, vehicleSeat);
                            EPlayer.Freeze(player, true);
                            continue;
                        }
                    }
                    catch { }
                }
                return;
            }

            // Setting up the basics -> team outfits, weapons, health, armor, etc
            if(secondsElapsed == 0)
            {
                lobby.CollisionCounter = 0;
                foreach(Player player in CopchasePlayers)
                {
                    player.Health = 100;
                    player.SetSharedData("Collisions", true);
                    player.TriggerEvent("SetActorInvulnerable", false);
                    if (lobby.Fugitives.Contains(player))
                    {
                        if(player == lobby.BankRobbery.HeistLeader)
                        {
                            player.SetSharedData("HeistLeader", true);
                            player.SetSharedData("BR_IsHeistLeader", true);
                            player.TriggerEvent("SetActorInvulnerable", false);
                            player.Armor = 100;

                            player.GiveWeaponAC(WeaponHash.Pistol_mk2, 75);
                            player.GiveWeaponAC(WeaponHash.Pumpshotgun, 50);
                            player.GiveWeaponAC(WeaponHash.Smg_mk2, 150);
                            player.GiveWeaponAC(WeaponHash.Carbinerifle_mk2, 150);

                            Events.RequestPlayAnimation(player, "mp_missheist_ornatebank", "stand_cash_in_bag_loop", 1);
                        }
                        else if(player == lobby.BankRobbery.CoLeader)
                        {
                            player.SetSharedData("HeistLeader", true);
                            player.SetSharedData("BR_IsHeistLeader", true);
                            player.TriggerEvent("SetActorInvulnerable", false);
                            player.Armor = 100;

                            player.GiveWeaponAC(WeaponHash.Pistol_mk2, 75);
                            player.GiveWeaponAC(WeaponHash.Pumpshotgun, 50);
                            player.GiveWeaponAC(WeaponHash.Smg_mk2, 150);
                            player.GiveWeaponAC(WeaponHash.Carbinerifle, 150);

                            Events.RequestPlayAnimation(player, "mp_missheist_ornatebank", "stand_cash_in_bag_loop", 1);
                        }
                        else
                        {
                            player.SetSharedData("IsHeistLeader", false);
                            player.TriggerEvent("SetActorInvulnerable", false);
                            player.Armor = 75;

                            player.GiveWeaponAC(WeaponHash.Pistol, 75);
                            player.GiveWeaponAC(WeaponHash.Sawnoffshotgun, 50);
                            player.GiveWeaponAC(WeaponHash.Assaultrifle, 150);
                        }
                        continue;
                    }
                    if (lobby.Cops.Contains(player))
                    {
                        bool isBearcatUnit = false;
                        if (player.HasData("BR_IsBearcatUnit")) isBearcatUnit = player.GetData<bool>("BR_IsBearcatUnit");

                        if (isBearcatUnit)
                        {
                            player.TriggerEvent("SetActorInvulnerable", false);
                            player.Armor = 150;

                            player.GiveWeaponAC(WeaponHash.Nightstick, 1);
                            player.GiveWeaponAC(WeaponHash.Heavypistol, 100);
                            player.GiveWeaponAC(WeaponHash.Pumpshotgun, 50);
                            player.GiveWeaponAC((WeaponHash)3347935668, 200); // Heavy Rifle
                            player.GiveWeaponAC(WeaponHash.Smg, 200);
                        }
                        else
                        {
                            player.TriggerEvent("SetActorInvulnerable", false);
                            player.Armor = 100;

                            player.GiveWeaponAC(WeaponHash.Nightstick, 1);
                            player.GiveWeaponAC(WeaponHash.Combatpistol, 100);
                            player.GiveWeaponAC(WeaponHash.Pumpshotgun, 50);
                            player.GiveWeaponAC(WeaponHash.Carbinerifle, 150);
                        }
                        continue;
                    }
                }
            }

            // Bank Robbery started!
            if(secondsElapsed == 10)
            {
                lobby.BankRobbery.GameState = BankRobberyData.State.StealingMoney;
                foreach (Player player in CopchasePlayers)
                {

                    player.TriggerEvent("StartChaseTimer", 10, 900, 0, (short)lobby.gameMode);


                    player.SetSharedData("ClimbProtection", false);
                    player.SetSharedData("Collisions", true);
                    player.TriggerEvent("SetActorInvulnerable", false);
                    player.TriggerEvent("EVENTS:SetBankRobberyActive", true);
                    // Stop the custom loading screen
                    if (lobby.Fugitives.Contains(player))
                    {
                        if(lobby.BankRobbery.HeistLeader != player && lobby.BankRobbery.CoLeader != player)
                        {
                            EPlayer.Freeze(player, false);
                        }

                        EPlayer.Notify(player, Translate(player, "STEALING_MONEY"), "green", "fa-cash", false, 40000, "bottom-center");

                        player.TriggerEvent("EVENT:ToggleBankRobberyBeginningScreen", false);
                        player.TriggerEvent("EVENTS:SetBankRobberyObjective", true, "BANKROB_FUGI_OBJECTIVE_STEALING_MONEY",
                                            lobby.BankRobbery.HeistLeader.Name, lobby.BankRobbery.CoLeader.Name);
                        player.TriggerEvent("SetActorInvulnerable", false);
                        continue;
                    }
                    if (lobby.Cops.Contains(player))
                    {
                        Vehicle toPutIn = player.GetData<Vehicle>("ChaseVehicle");
                        player.Position = toPutIn.Position.Around(10.0f);

                        NAPI.Task.Run(() =>
                        {
                            if(player != null)
                            {
                                int seat = player.GetData<int>("ChaseVehicleSeat");
                                player.SetIntoVehicle(toPutIn, seat);

                                EPlayer.Freeze(player, false);

                                player.SendChatMessage(Translate(player, "START_BROB_RADIO_COP"));

                                player.TriggerEvent("EVENT:ToggleBankRobberyBeginningScreen", false);
                                player.TriggerEvent("EVENTS:SetBankRobberyObjective", true, "BANKROB_COP_OBJECTIVE_GET_TO_BANK");
                                player.TriggerEvent("SetActorInvulnerable", false);

                                player.TriggerEvent("EVENT:AddBankRobberyObjectiveBlip", BankRobberyData.BankCheckpointPos.X, BankRobberyData.BankCheckpointPos.Y);
                                player.TriggerEvent("EVENT:SetBankRobberyWaypoint", BankRobberyData.BankCheckpointPos.X, BankRobberyData.BankCheckpointPos.Y);
                            }

                        }, 500);

                        continue;
                    }
                }
            }

            // 40 seconds after the round started aka 50th second => leader steals money, advance to next stage
            if(secondsElapsed == 50)
            {
                lobby.BankRobbery.GameState = BankRobberyData.State.GettingToVehicle;

                int hideoutLocation = new Random().Next(0, BankRobberyData.HideoutNames.Length);
                lobby.BankRobbery.HideoutLocation = (BankRobberyData.Hideout)hideoutLocation;

                foreach(Player player in CopchasePlayers)
                {
                    player.TriggerEvent("EVENT:ClearObjectiveBlips");
                    if (lobby.Fugitives.Contains(player))
                    {
                        player.SendChatMessage(Translate(player, "BANK_ROB_CASH_TAKEN", EPlayer.Name(lobby.BankRobbery.HeistLeader)));
                        player.SendChatMessage(Translate(player, "BANK_ROB_ESCORT_1"));
                        player.SendChatMessage(Translate(player, "BANK_ROB_ESCORT_2", BankRobberyData.HideoutNames[hideoutLocation]));

                        if (lobby.BankRobbery.HeistLeader == player || lobby.BankRobbery.CoLeader == player)
                        {
                            EPlayer.Action(player, "ACTION_BANKROB_GRAB_BAG");
                            player.TriggerEvent("ClearTaskNotFalling");
                            EPlayer.Freeze(player, false);
                            Events.RequestClearAnimation(player);

                            player.TriggerEvent("EVENTS:SetBankRobberyObjective", true, "BANKROB_FUGI_OBJECTIVE_GET_TO_CAR_LEADER");
                        }
                        else
                        {
                            player.TriggerEvent("EVENTS:SetBankRobberyObjective", true, "BANKROB_FUGI_OBJECTIVE_GET_TO_CAR",
                                                lobby.BankRobbery.HeistLeader.Name, lobby.BankRobbery.CoLeader.Name);
                        }

                        foreach(BankRobberyData.BR_VehicleData fugiVeh in BankRobberyData.FugitiveVehicleData)
                        {
                            player.TriggerEvent("EVENT:AddBankRobberyObjectiveBlip", fugiVeh.Position.X,
                                                                                     fugiVeh.Position.Y,
                                                                                     fugiVeh.Position.Z);
                        }

                        continue;
                    }
                    if (lobby.Cops.Contains(player))
                    {
                        player.SendChatMessage(Translate(player, "BANK_ROB_COP_CASH_TAKEN"));
                        player.SendChatMessage(Translate(player, "BANK_ROB_COP_CASH_1"));
                        player.SendChatMessage(Translate(player, "BANK_ROB_COP_CASH_2"));
                        player.TriggerEvent("EVENTS:SetBankRobberyObjective", true, "BANKROB_COP_OBJECTIVE_STOP_REACHING_CAR");
                        continue;
                    }
                }
            }

            // 15 minutes passed => end bank robbery
            if (lobby.GameProgress >= 900)
            {
                StopGame(lobby.lobbyId, 1);
            }
        }


        public static void GameUpdate()
        {
            try
            {
                if (Environment.TickCount64 - Settings.LastDerbyTick >= 1000 && Settings.LoggedInPlayers != null && Settings.LoggedInPlayers.Where(a => a.HasSharedData("InLobby") && a.GetSharedDataSafe<int>("InLobby") == -4).Count() >= 2)
                {
                    //Settings.DerbyTimeLeft--;
                    Settings.LastDerbyTick = Environment.TickCount64;

                    if (Settings.DerbyTimeLeft <= 0)
                    {
                        Main.CheckDerbyWinner();
                    }
                }


                lock (Settings.Lobbies)
                {
                    foreach (LobbyData lobby in Settings.Lobbies.Where(l => l.gameStatus == 1))
                    {
                        lock (lobby)
                        {
                            lobby.GameProgress++;
                            if(lobby.gameMode == GameModes.STANDARD || lobby.gameMode == GameModes.BUSTED)
                            {
                                CopchaseSecondTick(lobby, lobby.GameProgress);
                                continue;
                            }
                            if(lobby.gameMode == GameModes.BANKROBBERY)
                            {
                                BankRobberySecondTick(lobby, lobby.GameProgress);
                                continue;
                            }

                            /*
                            if (lobby.GameProgress == -10)
                            {
                                if (lobby.gameMode != GameModes.BANKROBBERY)
                                {
                                    Task.Run(async () =>
                                    {
                                        using (MainDatabase context = ContextFactory.CreateContext())
                                        {
                                            MapSpawn fugi = await context.MapSpawns.AsQueryable().FirstOrDefaultAsync(a => a.mapID == lobby.lastMapId && a.spawnTeam == 0);
                                            MapSpawn cop = await context.MapSpawns.AsQueryable().FirstOrDefaultAsync(a => a.mapID == lobby.lastMapId && a.spawnTeam == 1);

                                            NAPI.Task.Run(() =>
                                            {
                                                lock (lobby.Fugitives)
                                                {
                                                    foreach (var p in lobby.Fugitives)
                                                    {
                                                        try
                                                        {
                                                            if (p == null) continue;

                                                            p.Position = new Vector3(fugi.spawnX, fugi.spawnY, fugi.spawnZ).Around(30f);
                                                        }
                                                        catch { }
                                                    }
                                                }

                                                lock (lobby.Cops)
                                                {
                                                    foreach (var p in lobby.Cops)
                                                    {
                                                        try
                                                        {
                                                            if (p == null) continue;

                                                            p.Position = new Vector3(cop.spawnX, cop.spawnY, cop.spawnZ).Around(30f);
                                                        }
                                                        catch { }
                                                    }
                                                }
                                            });

                                        }
                                    });


                                }
                                else
                                {
                                    if (lobby.gameMode == GameModes.BANKROBBERY)
                                    {
                                        foreach (Player p in Settings.LoggedInPlayers.Where(p => lobby.Cops.Contains(p)))
                                        {
                                            lock (p)
                                            {
                                                try
                                                {
                                                    if (p == null) continue;

                                                    p.TriggerEvent("ResetKillFeed");
                                                    p.TriggerEvent("ToggleNametags", true);


                                                    p.Dimension = Convert.ToUInt32(lobby.lobbyId);
                                                    p.Position = new Vector3(427.2379f, -979.13336f, 30.709988f).Around(7f);

                                                    p.SendChatMessage(Translate(p, "START_BANKROB_1"));
                                                    p.SendChatMessage(Translate(p, "START_BANKROB_2"));
                                                    p.SendChatMessage(Translate(p, "START_BANKROB_3"));
                                                    p.SendChatMessage(Translate(p, "START_BANKROB_4"));
                                                    p.SendChatMessage(Translate(p, "START_BANKROB_5"));

                                                }
                                                catch { }
                                            }
                                        }
                                        BankRobberyData _brob = lobby.BankRobbery;

                                        foreach (Player p in Settings.LoggedInPlayers.Where(p => lobby.Fugitives.Contains(p)))
                                        {
                                            lock (p)
                                            {
                                                try
                                                {
                                                    if (p == null) continue;

                                                    p.TriggerEvent("ResetKillFeed");
                                                    p.TriggerEvent("ToggleNametags", true);

                                                    p.SendChatMessage(Translate(p, "START_BANKROB_1"));
                                                    if (p == _brob.HeistLeader)
                                                    {
                                                        EPlayer.Notify(p, Translate(p, "SELECTED_AS_LEADER"), "orange", "fas fa-hourglass-start", false, 10000);
                                                        p.SendChatMessage(Translate(p, "SELECTED_AS_LEADER_2"));
                                                        p.SendChatMessage(Translate(p, "START_BROB_LEADER_1"));
                                                        p.SendChatMessage(Translate(p, "START_BROB_LEADER_2"));
                                                        p.SendChatMessage(Translate(p, "START_BROB_LEADER_3"));
                                                        p.SendChatMessage(Translate(p, "START_BROB_LEADER_4"));

                                                        p.Position = BankRobberyData.LeaderPos;
                                                        p.Heading = BankRobberyData.LeaderHeading;
                                                    }
                                                    else if (p == _brob.Sniper)
                                                    {
                                                        p.SendChatMessage(Translate(p, "SELECTED_AS_SNIPER"));
                                                        p.SendChatMessage(Translate(p, "START_BROB_SNIPER_1", EPlayer.Name(_brob.HeistLeader)));
                                                        p.SendChatMessage(Translate(p, "START_BROB_SNIPER_2"));
                                                        p.SendChatMessage(Translate(p, "START_BROB_SNIPER_3"));
                                                        p.SendChatMessage(Translate(p, "START_BROB_SNIPER_4"));


                                                        p.Position = BankRobberyData.SniperPos;
                                                        p.Heading = BankRobberyData.SniperHeading;
                                                    }
                                                    else
                                                    {
                                                        p.SendChatMessage(Translate(p, "START_BROB_NORMAL_1", EPlayer.Name(_brob.HeistLeader)));
                                                        p.SendChatMessage(Translate(p, "START_BROB_NORMAL_2"));
                                                        p.SendChatMessage(Translate(p, "START_BROB_NORMAL_3"));
                                                        p.SendChatMessage(Translate(p, "START_BROB_NORMAL_4"));

                                                        p.Position = (Vector3)BankRobberyData.FugitiveSpawns[0, 0];
                                                        p.Heading = (float)BankRobberyData.FugitiveSpawns[0, 1];
                                                    }
                                                    p.SendChatMessage(Translate(p, "START_BROB_WORKASTEAM"));

                                                }
                                                catch { }
                                            }
                                        }
                                    }
                                }
                            }
                            if (lobby.GameProgress == -5)
                            {
                                if (lobby.gameMode != GameModes.BANKROBBERY)
                                {
                                    lock (lobby.Fugitives)
                                    {
                                        foreach (var p in lobby.Fugitives)
                                        {
                                            try
                                            {
                                                if (p == null) continue;

                                                p.Dimension = Convert.ToUInt32(lobby.lobbyId);
                                                p.SetIntoVehicle(p.GetData<Vehicle>("ChaseVehicle"), p.GetData<int>("ChaseVehicleSeat"));
                                                p.TriggerEvent("ChangeVehicleHeading", p.GetData<Vehicle>("ChaseVehicle").GetData<float>("SpawnA"));
                                                EPlayer.Freeze(p, true);
                                                p.TriggerEvent("stopLoadingScreen");
                                            }
                                            catch { }
                                        }
                                    }

                                    lock (lobby.Cops)
                                    {
                                        foreach (var p in lobby.Cops)
                                        {
                                            try
                                            {
                                                if (p == null) continue;

                                                p.Dimension = Convert.ToUInt32(lobby.lobbyId);
                                                p.SetIntoVehicle(p.GetData<Vehicle>("ChaseVehicle"), p.GetData<int>("ChaseVehicleSeat"));
                                                p.TriggerEvent("ChangeVehicleHeading", p.GetData<Vehicle>("ChaseVehicle").GetData<float>("SpawnA"));
                                                EPlayer.Freeze(p, true);
                                                p.TriggerEvent("stopLoadingScreen");
                                            }
                                            catch { }
                                        }
                                    }
                                }
                            }
                            if (lobby.GameProgress == -3)
                            {
                                if (lobby.gameMode != GameModes.BANKROBBERY)
                                {
                                    lock (lobby.Fugitives)
                                    {
                                        foreach (var p in lobby.Fugitives)
                                        {
                                            try
                                            {
                                                if (p == null) continue;

                                                p.SetIntoVehicle(p.GetData<Vehicle>("ChaseVehicle"), p.GetData<int>("ChaseVehicleSeat"));

                                                //p.TriggerEvent("ChangeVehicleHeading", p.GetData<Vehicle>("ChaseVehicle").GetData<float>("SpawnA"));
                                                //EPlayer.Freeze(p, true);

                                                p.GetData<Vehicle>("ChaseVehicle").SetSharedData("Collisions", false);
                                                p.TriggerEvent("ToggleCollisionsForEnt", p.GetData<Vehicle>("ChaseVehicle"), false);

                                                p.SetSharedData("Collisions", false);
                                                //p.TriggerEvent("stopLoadingScreen");
                                            }
                                            catch { }
                                        }
                                    }

                                    lock (lobby.Cops)
                                    {
                                        foreach (var p in lobby.Cops)
                                        {
                                            try
                                            {
                                                if (p == null) continue;

                                                p.SetIntoVehicle(p.GetData<Vehicle>("ChaseVehicle"), p.GetData<int>("ChaseVehicleSeat"));

                                                // p.TriggerEvent("ChangeVehicleHeading", p.GetData<Vehicle>("ChaseVehicle").GetData<float>("SpawnA"));
                                                //EPlayer.Freeze(p, true);

                                                p.GetData<Vehicle>("ChaseVehicle").SetSharedData("Collisions", false);
                                                p.SetSharedData("Collisions", false);
                                                //p.TriggerEvent("stopLoadingScreen");
                                            }
                                            catch { }
                                        }
                                    }
                                }
                            }

                            if (lobby.GameProgress == 0)
                            {
                                if (lobby.gameMode != GameModes.BANKROBBERY)
                                {
                                    lock (lobby.Fugitives)
                                    {
                                        foreach (var p in lobby.Fugitives)
                                        {
                                            try
                                            {
                                                if (p == null) continue;

                                                p.SetIntoVehicle(p.GetData<Vehicle>("ChaseVehicle"), p.GetData<int>("ChaseVehicleSeat"));
                                                p.TriggerEvent("ToggleNametags", true);

                                                //p.TriggerEvent("ChangeVehicleHeading", p.GetData<Vehicle>("ChaseVehicle").GetData<float>("SpawnA"));

                                                p.GetData<Vehicle>("ChaseVehicle").SetSharedData("Collisions", true);
                                                p.SetSharedData("Collisions", true);
                                                p.TriggerEvent("ToggleCollisionsForEnt", p.GetData<Vehicle>("ChaseVehicle"), true);

                                                p.GetData<Vehicle>("ChaseVehicle").SetSharedData("Collisions", false);
                                                p.SetSharedData("Collisions", false);
                                                p.TriggerEvent("ToggleCollisionsForEnt", p.GetData<Vehicle>("ChaseVehicle"), false);

                                                p.RemoveAllWeapons();


                                                p.Health = 100;
                                                p.Armor = 0;

                                                EPlayer.Freeze(p, false);

                                                if (p.GetData<int>("ChaseVehicleSeat") == 0)
                                                {
                                                    Account _a = EPlayer.GetAccount(p);
                                                    uint vehModel = p.GetData<Vehicle>("ChaseVehicle").Model;
                                                    Task.Run(async () =>
                                                    {
                                                        using (MainDatabase context = ContextFactory.CreateContext())
                                                        {
                                                            var inst = await context.PlayerTunings.AsQueryable().FirstOrDefaultAsync(a => a.Model == vehModel && a.AccountSQLID == _a.SQL_ID);
                                                            if (inst != null)
                                                            {
                                                                NAPI.Task.Run(() =>
                                                                {
                                                                    p.GetData<Vehicle>("ChaseVehicle").PrimaryColor = inst.PrimaryColor;
                                                                    p.GetData<Vehicle>("ChaseVehicle").SecondaryColor = inst.SecondaryColor;

                                                                    foreach (var kvp in NAPI.Util.FromJson<Dictionary<int, int>>(inst.Mods))
                                                                    {
                                                                        p.GetData<Vehicle>("ChaseVehicle").SetMod(kvp.Key, kvp.Value);
                                                                        if (kvp.Key == 56) p.GetData<Vehicle>("ChaseVehicle").WheelType = kvp.Value;
                                                                    }
                                                                    p.GetData<Vehicle>("ChaseVehicle").PearlescentColor = inst.PearlescentColor;
                                                                    p.GetData<Vehicle>("ChaseVehicle").WheelColor = inst.WheelColor;
                                                                });
                                                            }
                                                        }
                                                    });

                                                }
                                                p.TriggerEvent("stopLoadingScreen");

                                                //EPlayer.Freeze(p, false);
                                                p.TriggerEvent("ToggleCanExitVehicle", false);
                                            }
                                            catch { }
                                        }
                                    }

                                    lock (lobby.Cops)
                                    {
                                        foreach (var p in lobby.Cops)
                                        {
                                            if (p == null || lobby.Fugitives.Contains(p)) continue;
                                            try
                                            {
                                                p.TriggerEvent("stopLoadingScreen");
                                                p.SetIntoVehicle(p.GetData<Vehicle>("ChaseVehicle"), p.GetData<int>("ChaseVehicleSeat"));
                                                p.TriggerEvent("ToggleNametags", true);

                                                //p.TriggerEvent("ChangeVehicleHeading", p.GetData<Vehicle>("ChaseVehicle").GetData<float>("SpawnA"));

                                                p.GetData<Vehicle>("ChaseVehicle").SetSharedData("Collisions", true);
                                                p.SetSharedData("Collisions", true);
                                                p.TriggerEvent("ToggleCollisionsForEnt", p.GetData<Vehicle>("ChaseVehicle"), true);

                                                p.GetData<Vehicle>("ChaseVehicle").SetSharedData("Collisions", false);
                                                p.SetSharedData("Collisions", false);
                                                p.TriggerEvent("ToggleCollisionsForEnt", p.GetData<Vehicle>("ChaseVehicle"), false);

                                                p.RemoveAllWeapons();

                                                p.Health = 100;
                                                p.Armor = 0;

                                                if (p.GetData<int>("ChaseVehicleSeat") == 0)
                                                {
                                                    Account _a = EPlayer.GetAccount(p);
                                                    uint vehModel = p.GetData<Vehicle>("ChaseVehicle").Model;
                                                    Task.Run(async () =>
                                                    {
                                                        using (MainDatabase context = ContextFactory.CreateContext())
                                                        {
                                                            var inst = await context.PlayerTunings.AsQueryable().FirstOrDefaultAsync(a => a.Model == vehModel && a.AccountSQLID == _a.SQL_ID);
                                                            if (inst != null)
                                                            {
                                                                NAPI.Task.Run(() =>
                                                                {
                                                                    try
                                                                    {
                                                                        if (p.HasData("ChaseVehicle"))
                                                                        {
                                                                            p.GetData<Vehicle>("ChaseVehicle").PrimaryColor = inst.PrimaryColor;
                                                                            p.GetData<Vehicle>("ChaseVehicle").SecondaryColor = inst.SecondaryColor;

                                                                            foreach (var kvp in NAPI.Util.FromJson<Dictionary<int, int>>(inst.Mods))
                                                                            {
                                                                                if (kvp.Key != null && kvp.Value != null)
                                                                                {
                                                                                    if (p.HasData("ChaseVehicle"))
                                                                                    {
                                                                                        p.GetData<Vehicle>("ChaseVehicle").SetMod(kvp.Key, kvp.Value);
                                                                                        if (kvp.Key == 56) p.GetData<Vehicle>("ChaseVVehicle").WheelType = kvp.Value;
                                                                                    }
                                                                                }
                                                                            }
                                                                            p.GetData<Vehicle>("ChaseVehicle").PearlescentColor = inst.PearlescentColor;
                                                                            p.GetData<Vehicle>("ChaseVehicle").WheelColor = inst.WheelColor;
                                                                        }
                                                                    }
                                                                    catch (Exception ex)
                                                                    {
                                                                        NAPI.Util.ConsoleOutput($"Failed to set vehicle tunings for {p.Name}: {ex.ToString()}");
                                                                    }
                                                                });

                                                            }
                                                        }
                                                    });

                                                }
                                                //EPlayer.Freeze(p, false);
                                                p.TriggerEvent("ToggleCanExitVehicle", false);
                                            }
                                            catch { }
                                        }
                                    }

                                    foreach (var v in lobby.Vehicles)
                                    {
                                        try
                                        {
                                            if (v == null) continue;

                                            v.SetSharedData("Collisions", false);
                                            Settings.LoggedInPlayers.ForEach((p) =>
                                            {
                                                if (p.Position.DistanceTo(v.Position) < 500.0)
                                                {
                                                    p.TriggerEvent("ToggleCollisionsForEnt", p.GetData<Vehicle>("ChaseVehicle"), false);
                                                }
                                            });
                                        }
                                        catch { }
                                    }
                                }
                                else
                                {
                                    List<Vehicle> ccars = new List<Vehicle>();
                                    ccars.AddRange(lobby.BankRobbery.PoliceVehicles);
                                    int currentCar = new Random().Next(ccars.Count);
                                    List<int> usedCars = new List<int>();
                                    usedCars.Add(currentCar);
                                    int lobbyid = lobby.lobbyId;

                                    // currentCar, occupants, maxOccupants
                                    (int, int, int) currentCarInfo = (currentCar, 0, ccars[currentCar].MaxOccupants);

                                    foreach (Player player in lobby.Cops.OrderBy(x => Guid.NewGuid()))
                                    {
                                        player.Dimension = Convert.ToUInt32(lobbyid);
                                        player.Position = new Vector3(427.2379f, -979.13336f, 30.709988f).Around(7f);
                                        CharacterData ConvChar = new CharacterData();

                                        if (currentCar >= 6) // Special/SWAT/FIB unit 
                                        {
                                            player.SetData<string>("BankRobberyUnit", "fib");
                                            player.SendChatMessage(Translate(player, "BROB_ASSIGN_FIB"));
                                        }
                                        else // patrol unit
                                        {
                                            player.SetData<string>("BankRobberyUnit", "patrol");
                                            player.SendChatMessage(Translate(player, "BROB_ASSIGN_PATROL"));
                                        }

                                        EPlayer.Freeze(player, true);

                                        player.SetData("GPS", false);

                                        EPlayer.SetDiscordStatus(player, $"[Lobby {lobby.lobbyId}] Cop as {player.Name}");

                                        EPlayer.SetColor(player, new Color(0, 151, 255), 0);
                                        player.TriggerEvent("SetActorInvulnerable", false);
                                        player.TriggerEvent("ToggleAFOB", Settings.Dynamics.AntiFallOffBike, Settings.Dynamics.AntiFallOffBikeSpeedLimit);
                                        player.SetSharedData("CuffedBy", "n/a");
                                        player.SetSharedData("Team", 1);

                                        try
                                        {
                                            ConvChar = NAPI.Util.FromJson<CharacterData>(EPlayer.GetAccount(player).Character);

                                            EPlayer.ApplyCharacter(player, ConvChar, 1);

                                            Character.SetTeamOutfit(player, 1);
                                        }
                                        catch { }

                                        if (currentCarInfo.Item2 < currentCarInfo.Item3)
                                        {
                                            int seat = currentCarInfo.Item2;
                                            player.SetData<Vehicle>("BR_UnitVehicle", ccars[currentCar]);
                                            player.SetData<int>("BR_UnitVehicleSeat", seat);
                                            currentCarInfo.Item2++;
                                            continue;
                                        }
                                        currentCar = -1;
                                        for (int tries = 0; tries < 50; tries++)
                                        {
                                            currentCar = new Random().Next(ccars.Count);
                                            if (ccars[currentCar].Occupants.Count < ccars[currentCar].MaxOccupants)
                                            {
                                                currentCarInfo.Item1 = currentCar;
                                                currentCarInfo.Item2 = ccars[currentCar].Occupants.Count;
                                                currentCarInfo.Item3 = ccars[currentCar].MaxOccupants;
                                            }
                                            else
                                            {
                                                currentCar = -1;
                                                continue;
                                            }
                                        }
                                        if (currentCar == -1)
                                        {
                                            currentCar = 1;
                                            Vehicle dick = NAPI.Vehicle.CreateVehicle((uint)VehicleHash.Police, ccars[currentCar - 1].Position, ccars[currentCar - 1].Heading, 0, 0, "SEX", 255, false, true, player.Dimension);
                                            player.SetData<Vehicle>("BR_UnitVehicle", dick);
                                            player.SetData<int>("BR_UnitVehicleSeat", 0);
                                        }
                                    }

                                    BankRobberyData _brob = lobby.BankRobbery;
                                    Random kevycanusendmefeetpicspls = new Random();
                                    foreach (Player player in lobby.Fugitives.OrderBy(x => Guid.NewGuid()))
                                    {
                                        Account _a = EPlayer.GetAccount(player);
                                        CharacterData cd = NAPI.Util.FromJson<CharacterData>(_a.Character);

                                        try
                                        {
                                            if (player != _brob.HeistLeader && player != _brob.Sniper)
                                            {
                                                int spid = kevycanusendmefeetpicspls.Next(BankRobberyData.FugitiveSpawns.GetLength(0));
                                                player.Position = (Vector3)BankRobberyData.FugitiveSpawns[spid, 0];
                                                player.Heading = (float)BankRobberyData.FugitiveSpawns[spid, 1];

                                                cd.Clothing[0].Hat = 62;
                                                cd.Clothing[0].HatTxt = new Random().Next(0, 10);
                                                cd.Clothing[2].Hat = 62;
                                                cd.Clothing[2].HatTxt = new Random().Next(0, 10);
                                            }
                                            else
                                            {
                                                if (player == _brob.HeistLeader)
                                                {
                                                    player.Position = BankRobberyData.LeaderPos;
                                                    player.Heading = BankRobberyData.LeaderHeading;

                                                    cd.Clothing[0].Hat = 62;
                                                    cd.Clothing[0].HatTxt = 9;
                                                    cd.Clothing[2].Hat = 62;
                                                    cd.Clothing[2].HatTxt = 9;
                                                }
                                                else if (player == _brob.Sniper)
                                                {
                                                    player.Position = BankRobberyData.SniperPos;
                                                    player.Heading = BankRobberyData.SniperHeading;

                                                    cd.Clothing[0].Hat = 62;
                                                    cd.Clothing[0].HatTxt = new Random().Next(0, 10);
                                                    cd.Clothing[2].Hat = 62;
                                                    cd.Clothing[2].HatTxt = new Random().Next(0, 10);
                                                }
                                            }

                                            EPlayer.ApplyCharacter(player, cd, 0);
                                        }
                                        catch (Exception ex)
                                        {
                                            try
                                            {
                                                CharacterData ConvChar = NAPI.Util.FromJson<CharacterData>(EPlayer.GetAccount(player).Character);

                                                EPlayer.ApplyCharacter(player, ConvChar, 0);

                                                Character.SetTeamOutfit(player, 0);
                                            }
                                            catch { }
                                            NAPI.Util.ConsoleOutput("ZAMN! BankRob fugi outfit fail: " + ex.ToString());
                                        }

                                        EPlayer.Freeze(player, true);

                                        EPlayer.SetColor(player, new Color(255, 174, 0), 0);

                                        EPlayer.SetDiscordStatus(player, $"[Lobby {lobbyid}] Fugitive as {player.Name}");

                                        player.TriggerEvent("SetActorInvulnerable", false);
                                        player.TriggerEvent("ToggleAFOB", Settings.Dynamics.AntiFallOffBike, Settings.Dynamics.AntiFallOffBikeSpeedLimit);

                                        player.SetSharedData("VisibleBlip", true);
                                        player.ResetData("Handsup");
                                        player.ResetData("Cuffed");
                                        player.SetSharedData("Team", 0);

                                        player.SetSharedData("CuffedBy", "n/a");
                                        player.SetSharedData("HasShot", true);
                                        player.Dimension = Convert.ToUInt32(lobbyid);
                                    }
                                }
                                Task.Run(async () =>
                                {
                                    using (MainDatabase context = ContextFactory.CreateContext())
                                    {
                                        Map curMap = await context.Maps.AsQueryable().FirstOrDefaultAsync(a => a.SQL_ID == lobby.lastMapId);
                                        string mapName = curMap.Name;

                                        NAPI.Task.Run(() =>
                                        {
                                            if (lobby.gameMode == GameModes.STANDARD)
                                            {
                                                EPlayer.SendMessageToAll("COPCHASE_STARTED", lobby.lobbyId, lobby.Cops.Count, lobby.Fugitives.Count, mapName);
                                            }
                                            else if (lobby.gameMode == GameModes.BUSTED)
                                            {
                                                EPlayer.SendMessageToAll("BUSTED_STARTED", lobby.lobbyId, lobby.Cops.Count, lobby.Fugitives.Count, mapName);
                                            }
                                            else if (lobby.gameMode == GameModes.BANKROBBERY)
                                            {
                                                EPlayer.SendMessageToAll("BANKROB_STARTED", lobby.lobbyId, lobby.Cops.Count, lobby.Fugitives.Count, mapName);
                                            }
                                        });
                                    }
                                });



                                foreach (Player p in Settings.LoggedInPlayers.Where(p => lobby.Cops.Contains(p) || lobby.Fugitives.Contains(p)))
                                {
                                    lock (p)
                                    {
                                        try
                                        {
                                            if (p == null) continue;

                                            p.TriggerEvent("ResetKillFeed");

                                            if (lobby.Cops.Contains(p)) EPlayer.Notify(p, Translate(p, "UNFREEZE_10S_NOTIFY"), "orange", "fas fa-hourglass-start", false, 10000);
                                            if (lobby.gameMode == GameModes.STANDARD)
                                            {
                                                p.SendChatMessage(Translate(p, "COPS_10S_UNFREEZE"));
                                            }
                                            else if (lobby.gameMode == GameModes.BUSTED)
                                            {
                                                p.SendChatMessage(Translate(p, "START_BUSTED_MSG_1"));
                                                p.SendChatMessage(Translate(p, "START_BUSTED_MSG_2"));
                                                p.SendChatMessage(Translate(p, "START_BUSTED_MSG_3"));
                                                p.SendChatMessage(Translate(p, "START_BUSTED_MSG_4"));
                                                p.SendChatMessage(Translate(p, "START_BUSTED_MSG_5"));
                                            }
                                        }
                                        catch { }
                                    }
                                }
                            }

                            if (lobby.GameProgress < 10)
                            {
                                if (lobby.gameMode != GameModes.BANKROBBERY)
                                {
                                    foreach (Player p in lobby.Cops)
                                    {
                                        if (p != null)
                                        {
                                            EPlayer.Freeze(p, true);
                                            p.TriggerEvent("ToggleCanExitVehicle", false);
                                        }
                                    }

                                    foreach (Player p in lobby.Fugitives)
                                    {
                                        if (p != null)
                                        {
                                            //EPlayer.Freeze(p, false);
                                            p.TriggerEvent("ToggleCanExitVehicle", false);
                                        }
                                    }
                                }
                                else
                                {
                                    if (lobby.gameMode == GameModes.BANKROBBERY)
                                    {
                                        foreach (Player p in lobby.Cops)
                                        {
                                            if (p != null)
                                            {
                                                EPlayer.Freeze(p, true);
                                                p.TriggerEvent("ToggleCanExitVehicle", false);
                                            }
                                        }

                                        foreach (Player p in lobby.Fugitives)
                                        {
                                            if (p != null)
                                            {
                                                EPlayer.Freeze(p, true);
                                                p.TriggerEvent("ToggleCanExitVehicle", false);
                                            }
                                        }
                                    }
                                }
                            }
                            */
                            /*
                            if (lobby.GameProgress == 10)
                            {
                                if (lobby.gameMode != GameModes.BANKROBBERY)
                                {
                                    foreach (Player p in lobby.Cops)
                                    {
                                        if (p != null)
                                        {
                                            EPlayer.Freeze(p, false);
                                            p.SendNotification("~y~You can now exit vehicles.");
                                            p.SendNotification("~b~You may begin the chase now.");
                                            p.TriggerEvent("ToggleCanExitVehicle", true);
                                            EPlayer.TryAdvanceChallenge(p, Challenge.ChallengeType.CopchaseRounds, "N/A");

                                            if (p.IsInVehicle && p.Vehicle != null)
                                            {
                                                p.Vehicle.Repair();
                                            }
                                        }
                                    }
                                    foreach (Player p in lobby.Fugitives)
                                    {
                                        if (p != null)
                                        {
                                            p.SendNotification("~y~You can now exit vehicles.");
                                            p.TriggerEvent("ToggleCanExitVehicle", true);
                                            EPlayer.TryAdvanceChallenge(p, Challenge.ChallengeType.CopchaseRounds, "N/A");

                                            if (p.IsInVehicle && p.Vehicle != null)
                                            {
                                                p.Vehicle.Repair();
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (lobby.gameMode == GameModes.BANKROBBERY)
                                    {
                                        try
                                        {
                                            foreach (Player p in lobby.Cops)
                                            {
                                                if (p != null)
                                                {
                                                    EPlayer.TryAdvanceChallenge(p, Challenge.ChallengeType.CopchaseRounds, "N/A");
                                                    p.TriggerEvent("BankDoorLocked", true);
                                                    p.TriggerEvent("BankRob_SetMusic", 0);

                                                    if (p.HasData("BankRobberyUnit"))
                                                    {
                                                        if (p.GetData<string>("BankRobberyUnit") == "fib")
                                                        {
                                                            Account a = EPlayer.GetAccount(p);
                                                            try
                                                            {
                                                                try
                                                                {
                                                                    CharacterData ConvChar = NAPI.Util.FromJson<CharacterData>(EPlayer.GetAccount(p).Character);

                                                                    EPlayer.ApplyCharacter(p, ConvChar, 1);


                                                                    Character.SetTeamOutfit(p, 1);
                                                                }
                                                                catch { }

                                                                
                                                                CharacterData cd = NAPI.Util.FromJson<CharacterData>(a.Character);
                                                                
                                                                int len = cd.Clothing.Length;

                                                                bool hasHelmet = (new Random().Next(0, 10) > 5);
                                                                bool set_clothing_successfully = false;
                                                                if (len >= 2)
                                                                {
                                                                    cd.Clothing[1].Clothes[11].Drawable = 320;
                                                                    cd.Clothing[1].Clothes[11].Texture = 0;
                                                                    cd.Clothing[1].Clothes[4].Drawable = 121;
                                                                    cd.Clothing[1].Clothes[4].Texture = 0;
                                                                    cd.Clothing[1].Clothes[6].Drawable = 96;
                                                                    cd.Clothing[1].Clothes[6].Texture = 0;
                                                                
                                                                    if (hasHelmet)
                                                                    {
                                                                        cd.Clothing[1].Hat = 141;
                                                                        cd.Clothing[1].HatTxt = 0;
                                                                    }
                                                                    set_clothing_successfully = true;
                                                                }
                                                                if(len >= 4)
                                                                {
                                                                    cd.Clothing[3].Clothes[11].Drawable = 331;
                                                                    cd.Clothing[3].Clothes[11].Texture = 0;
                                                                    cd.Clothing[3].Clothes[4].Drawable = 127;
                                                                    cd.Clothing[3].Clothes[4].Texture = 0;
                                                                    cd.Clothing[3].Clothes[6].Drawable = 100;
                                                                    cd.Clothing[3].Clothes[6].Texture = 0;

                                                                    if (hasHelmet)
                                                                    {
                                                                        cd.Clothing[3].Hat = 140;
                                                                        cd.Clothing[3].HatTxt = 0;
                                                                    }
                                                                    set_clothing_successfully = true;
                                                                }
                                                                EPlayer.ApplyCharacter(p, cd, 1);
                                                                
                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                try
                                                                {
                                                                    CharacterData ConvChar = NAPI.Util.FromJson<CharacterData>(EPlayer.GetAccount(p).Character);

                                                                    EPlayer.ApplyCharacter(p, ConvChar, 1);


                                                                    Character.SetTeamOutfit(p, 1);
                                                                }
                                                                catch { }
                                                                NAPI.Util.ConsoleOutput("ZAMN! BANKROB FIB FAIL: " + ex.ToString());
                                                            }
                                                        }
                                                    }

                                                    try
                                                    {
                                                        EPlayer.Freeze(p, false);
                                                        if (p.HasData("BR_UnitVehicle") && p.HasData("BR_UnitVehicleSeat"))
                                                        {
                                                            Vehicle v = p.GetData<Vehicle>("BR_UnitVehicle");
                                                            int seat = p.GetData<int>("BR_UnitVehicleSeat");
                                                            if (v != null)
                                                            {
                                                                NAPI.Task.Run(() =>
                                                                {
                                                                    p.SetIntoVehicle(v, seat);
                                                                }, 100);
                                                            }
                                                        }

                                                        p.TriggerEvent("stopLoadingScreen");

                                                        EPlayer.Freeze(p, false);
                                                        p.SendNotification("~y~You can now exit vehicles.");
                                                        p.SendNotification("~b~Head to the bank! It has been marked on your GPS.");
                                                        p.TriggerEvent("ToggleCanExitVehicle", true);
                                                        p.TriggerEvent("playerCreateWaypoint", BankRobberyData.BankCheckpointPos);
                                                        p.ResetData("EarlyWarningGiven");

                                                        p.SendChatMessage(Translate(p, "START_BROB_RADIO_COP"));
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        NAPI.Util.ConsoleOutput("FAIL IN SECOND STAGE OF THAT SWAT SHIT " + ex.ToString());
                                                    }
                                                }
                                            }
                                            foreach (Player p in lobby.Fugitives)
                                            {
                                                if (p != null)
                                                {
                                                    EPlayer.TryAdvanceChallenge(p, Challenge.ChallengeType.CopchaseRounds, "N/A");
                                                    p.TriggerEvent("BankDoorLocked", true);
                                                    p.TriggerEvent("BankRob_SetMusic", 0);
                                                    EPlayer.Freeze(p, false);
                                                    p.SendNotification("~y~The event has started!");
                                                    p.TriggerEvent("ToggleCanExitVehicle", true);

                                                    if (p == lobby.BankRobbery.HeistLeader)
                                                    {
                                                        p.TriggerEvent("StartBankRobberyTimer");
                                                        p.SendChatMessage(Translate(p, "TAKING_CASH_MSG"));

                                                        Events.RequestPlayAnimation(p, "mp_missheist_ornatebank", "stand_cash_in_bag_loop", 1);
                                                        EPlayer.Freeze(p, true);

                                                        EPlayer.Notify(p, Translate(p, "STEALING_MONEY"), "green", "fas fa-hourglass-start", false, 40000, "bottomCenter");
                                                    }
                                                    else
                                                    {
                                                        p.SendChatMessage(Translate(p, "LAW_ON_WAY"));
                                                    }
                                                    p.TriggerEvent("stopLoadingScreen");

                                                    p.SendChatMessage(Translate(p, "LAW_ON_WAY_RADIO"));
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            NAPI.Util.ConsoleOutput("THIS IS ETERNAL SUFFERING " + ex.ToString());
                                        }
                                    }
                                }
                            }
                            */

                            /*
                            if (lobby.GameProgress >= (Convert.ToInt32(0.7 * Settings.GameRunTime) - 60) && lobby.gameMode != GameModes.BUSTED && lobby.gameMode != GameModes.BANKROBBERY)
                            {
                                foreach (Player fugitive in lobby.Fugitives)
                                {
                                    try
                                    {
                                        if (fugitive == null) continue;

                                        if (fugitive.GetSharedDataSafe<bool>("HasShot") == false && !fugitive.HasData("EarlyWarningGiven"))
                                        {
                                            fugitive.SetData<bool>("EarlyWarningGiven", true);
                                            fugitive.SendChatMessage(Translate(fugitive, "CAN_BE_SHOT_LONG_GAME_EARLY_WARNING"));
                                            fugitive.TriggerEvent("startCopchaseTimerEarlyWarningEffect");
                                        }
                                    }
                                    catch { }
                                }
                            }
                            if (lobby.GameProgress >= Convert.ToInt32(0.7 * Settings.GameRunTime) && lobby.gameMode != GameModes.BUSTED && lobby.gameMode != GameModes.BANKROBBERY)
                            {
                                foreach (Player fugitive in lobby.Fugitives)
                                {
                                    try
                                    {
                                        if (fugitive == null) continue;

                                        if (fugitive.GetSharedDataSafe<bool>("HasShot") == false)
                                        {
                                            fugitive.SetSharedData("HasShot", true);
                                            fugitive.SetSharedData("VisibleBlip", true);
                                            lobby.FugitiveShowCounter = 29; // Set them to VISIBLE

                                            fugitive.SendChatMessage(Translate(fugitive, "CAN_BE_SHOT_LONG_GAME"));
                                        }
                                    }
                                    catch { }
                                }

                                if (lobby.GameProgress == Convert.ToInt32(0.7 * Settings.GameRunTime) && lobby.gameMode != GameModes.BUSTED && lobby.gameMode != GameModes.BANKROBBERY)
                                {
                                    foreach (Player pl in Settings.LoggedInPlayers.Where(a => a.GetSharedDataSafe<int>("InLobby") == lobby.lobbyId))
                                    {
                                        try
                                        {
                                            if (pl == null) continue;

                                            pl.SendChatMessage(Translate(pl, "CAN_BE_SHOT_LONG_GAME_GLOBAL"));
                                            pl.TriggerEvent("StartChaseTimer", lobby.GameProgress, Settings.GameRunTime, -1, (short)lobby.gameMode);
                                        }
                                        catch { }
                                    }
                                }
                            }
                            */
                        }

                        foreach (Lobby_StingerData stinger in lobby.Stingers)
                        {
                            stinger.TimeElapsed++;
                            if (stinger.TimeElapsed == 15)
                            {
                                stinger.Stage = 2;
                                foreach (Player pl in lobby.Cops)
                                {
                                    pl.TriggerEvent("MDC:AddSpikeBlip", stinger.ID, stinger.Stage);
                                    pl.TriggerEvent("MDC:UpdateSpike", stinger.ID, stinger.Stage);
                                }

                                Stinger stdata = Stinger.Stingers.FirstOrDefault(s => s.ID == stinger.ID);
                                if (stdata != null)
                                {
                                    foreach (StingerInfo sgobject in stdata.StingerObjects.Where(s => s.IsNPC))
                                    {
                                        bool female = new Random().Next(0, 10) > 5;
                                        Ped stPed = NAPI.Ped.CreatePed(female ? PedHash.Cop01SFY : PedHash.Cop01SMY, sgobject.Position, sgobject.Rotation.Z, Convert.ToUInt32(lobby.lobbyId));
                                        stinger.StingerPeds.Add(stPed);
                                    }
                                }
                            }
                            else if (stinger.TimeElapsed == 30)
                            {
                                stinger.Stage = 3;
                                foreach (Player pl in lobby.Cops)
                                {
                                    pl.TriggerEvent("MDC:AddSpikeBlip", stinger.ID, stinger.Stage);
                                    pl.TriggerEvent("MDC:UpdateSpike", stinger.ID, stinger.Stage);
                                }

                                Stinger stdata = Stinger.Stingers.FirstOrDefault(s => s.ID == stinger.ID);
                                if (stdata != null)
                                {
                                    foreach (StingerInfo sgobject in stdata.StingerObjects.Where(s => !s.IsNPC))
                                    {
                                        GTANetworkAPI.Object obj = NAPI.Object.CreateObject(NAPI.Util.GetHashKey("p_stinger_04"),
                                        sgobject.Position, sgobject.Rotation, 255, Convert.ToUInt32(lobby.lobbyId));

                                        obj.SetData<string>("StingerObjectKey", $"{stdata.ID}_{stdata.StingerObjects.IndexOf(sgobject)}");
                                        stinger.StingerObjects.Add(obj);

                                        float offsetY = 1.75f; // forward = offset by 1.5f so it's in center of spike
                                        float offsetX = 0.0f;

                                        Vector3 newPos = sgobject.Position;
                                        float angle = sgobject.Rotation.Z + 180f;
                                        angle = (float)((angle * Math.PI) / 180f);

                                        newPos.X += (float)(offsetX * Math.Cos(angle) - offsetY * Math.Sin(angle));
                                        newPos.Y += (float)(offsetX * Math.Sin(angle) + offsetY * Math.Cos(angle));

                                        ColShape col = NAPI.ColShape.CreateSphereColShape(newPos, 1.5f, Convert.ToUInt32(lobby.lobbyId));
                                        col.SetData<string>("LinkedStingerObject", $"{stdata.ID}_{stdata.StingerObjects.IndexOf(sgobject)}");
                                        col.SetData<int>("StingerID", stdata.ID);
                                        stinger.StingerColshapes.Add(col);
                                    }
                                }
                            }
                        }

                        /*
                        int gameRunTime = Settings.GameRunTime;
                        if (lobby.gameMode == GameModes.BANKROBBERY) gameRunTime = 900;
                        if (lobby.GameProgress >= gameRunTime)
                        {
                            StopGame(lobby.lobbyId, 1);
                        }
                        */
                        /*
                        // Some code to start chase timer every 5 seconds, unnecessary?
                        
                        if (lobby.GameProgress % 5 == 0) // Every 5 seconds/multiple during collision phase
                        {

                            foreach (Player player in Settings.LoggedInPlayers.Where(p => p.GetSharedDataSafe<int>("InLobby") == lobby.lobbyId))
                            {
                                try
                                {
                                    if (player == null) continue;

                                    player.TriggerEvent("StartChaseTimer", lobby.GameProgress, gameRunTime, lobby.CollisionCounter, (short)lobby.gameMode);
                                }
                                catch { }
                            }
                        }
                        

                        // Unused in bank robbery
                        if (lobby.CollisionCounter > 0)
                        {
                            if (lobby.CollisionCounter % 5 == 0) // Every 5 seconds/multiple during collision phase
                            {
                                foreach (Vehicle veh in lobby.Vehicles)
                                {
                                    veh.SetSharedData("Collisions", false);
                                }
                            }

                            lobby.CollisionCounter--;

                            if (lobby.CollisionCounter <= 0)
                            {
                                lock (Settings.LoggedInPlayers)
                                {
                                    if (lobby.gameMode != GameModes.BANKROBBERY)
                                    {
                                        foreach (Player player in Settings.LoggedInPlayers.Where(p => p.GetSharedDataSafe<int>("InLobby") == lobby.lobbyId))
                                        {
                                            try
                                            {
                                                if (player == null) continue;

                                                player.TriggerEvent("StartChaseTimer", lobby.GameProgress, Settings.GameRunTime, 0, (short)lobby.gameMode);

                                                player.SetSharedData("BustedProgress", 0.0);

                                                //////////////////////////////////////////////////////////////////////////////////////////
                                                ///

                                                if (!lobby.Cops.Contains(player) && !lobby.Fugitives.Contains(player)) continue;

                                                EPlayer.Notify(player, Translate(player, "COLLISIONS_ENABLED"), "green", "fas fa-car-crash", false, 3000);

                                                player.Health = 100;
                                                player.Armor = 0;

                                                player.SetSharedData("Collisions", true);
                                                if (Settings.Dynamics.OnfootCollision == false) player.SetSharedData("Collisions", false);


                                                if (lobby.Cops.Contains(player))
                                                {
                                                    player.Armor = 60;

                                                    EPlayer.GiveCopWeapons(player);
                                                    player.TriggerEvent("SetActorInvulnerable", false);
                                                }
                                                else if (lobby.Fugitives.Contains(player))
                                                {
                                                    player.Armor = 25;

                                                    EPlayer.GiveFugitiveWeapons(player);
                                                    player.TriggerEvent("SetActorInvulnerable", false);
                                                }


                                            }
                                            catch { }
                                        }
                                        foreach (Vehicle veh in lobby.Vehicles)
                                        {
                                            veh.SetSharedData("Collisions", true);
                                            if (veh.HasSharedData("Team") && veh.GetSharedDataSafe<int>("Team") == 0)
                                            {
                                                if (veh.HasSharedData("SpeedInfo") && veh.GetSharedDataSafe<string>("SpeedInfo").Contains("Fast"))
                                                {
                                                    veh.SetSharedData("vMaxSpeed", (float)(175 / 3.6)); // 175 KM/H for fast cars
                                                }
                                                else
                                                {
                                                    veh.SetSharedData("vMaxSpeed", (float)(163 / 3.6)); // 163 KM/H for avg fugi cars
                                                }

                                            }
                                            else if (veh.HasSharedData("Team") && veh.GetSharedDataSafe<int>("Team") == 1)
                                            {
                                                if (veh.HasSharedData("SpeedInfo") && veh.GetSharedDataSafe<string>("SpeedInfo").Contains("Fast"))
                                                {
                                                    veh.SetSharedData("vMaxSpeed", (float)(193 / 3.6)); // 193 KM/H for fast cop cars
                                                }
                                                else
                                                {
                                                    veh.SetSharedData("vMaxSpeed", (float)(183 / 3.6)); // 183 KM/H for avg cc
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (lobby.gameMode == GameModes.BANKROBBERY)
                                        {
                                            foreach (Player player in Settings.LoggedInPlayers.Where(p => p.GetSharedDataSafe<int>("InLobby") == lobby.lobbyId))
                                            {
                                                try
                                                {
                                                    if (player == null) continue;

                                                    player.TriggerEvent("StartChaseTimer", lobby.GameProgress, 600, 0, (short)lobby.gameMode);

                                                    player.SetSharedData("BustedProgress", 0.0);

                                                    //////////////////////////////////////////////////////////////////////////////////////////
                                                    ///

                                                    if (!lobby.Cops.Contains(player) && !lobby.Fugitives.Contains(player)) continue;

                                                    player.SetSharedData("Collisions", true);
                                                    if (Settings.Dynamics.OnfootCollision == false) player.SetSharedData("Collisions", false);

                                                    player.TriggerEvent("SetActorInvulnerable", false);

                                                    if (lobby.Cops.Contains(player))
                                                    {
                                                        EPlayer.GiveCopWeapons(player);
                                                    }
                                                    else if (lobby.Fugitives.Contains(player))
                                                    {
                                                        EPlayer.GiveFugitiveWeapons(player);
                                                    }
                                                }
                                                catch { }
                                            }

                                        }

                                        foreach (Vehicle veh in lobby.Vehicles)
                                        {
                                            veh.SetSharedData("Collisions", true);
                                            if (veh.HasSharedData("Team") && veh.GetSharedDataSafe<int>("Team") == 0)
                                            {
                                                veh.SetSharedData("vMaxSpeed", (float)(155 / 3.6)); // 155 KM/H
                                            }
                                        }
                                    }
                                }
                            }

                            lock (lobby.Cops)
                            {
                                foreach (Player cop in lobby.Cops)
                                {
                                    try
                                    {
                                        if (cop == null) continue;

                                        if (cop.GetData<bool>("GPS") == false)
                                        {
                                            cop.SetSharedData("VisibleBlip", false);
                                        }
                                        else
                                        {
                                            cop.SetSharedData("VisibleBlip", true);
                                        }

                                        if (cop.GetSharedDataSafe<bool>("ClimbProtection") == true && cop.GetData<long>("ClimbProtectionEnd") < Environment.TickCount64)
                                        {
                                            cop.SetSharedData("ClimbProtection", false);
                                        }
                                    }
                                    catch { }
                                }
                            }
                            lock (lobby.Fugitives)
                            {
                                foreach (Player fugi in lobby.Fugitives)
                                {
                                    try
                                    {
                                        if (fugi == null) continue;

                                        if (fugi.GetSharedDataSafe<bool>("ClimbProtection") == true && fugi.GetData<long>("ClimbProtectionEnd") < Environment.TickCount64)
                                        {
                                            fugi.SetSharedData("ClimbProtection", false);
                                        }
                                    }
                                    catch { }
                                }
                            }
                        }
                        */
                    }
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[EXCEPTION IN GameUpdate]: {ex.ToString()}");
            }
            return;
        }
    }
}