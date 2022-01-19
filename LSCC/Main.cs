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

namespace RNC
{
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

    public class LobbyData
    {
        public List<Player> Cops { get; set; }
        public List<Player> Fugitives { get; set; }
        public List<Vehicle> Vehicles { get; set; }
        public List<Pickup> Drops { get; set; }

        public BankRobberyData BankRobbery { get; set; }

        public int lobbyId { get; set; }
        public int lastMapId { get; set; }
        public int gameStatus { get; set; }
        public GameModes gameMode { get; set; }

        public int FugitiveShowCounter { get; set; }
        public int LastSeenCounter { get; set; }
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

        public LobbyData()
        {
            Cops = new List<Player>();
            Fugitives = new List<Player>();
            Vehicles = new List<Vehicle>();
            Drops = new List<Pickup>();

            BankRobbery = new BankRobberyData();

            Time = new int[3];
            Time[0] = new Random(DateTimeOffset.UtcNow.Millisecond).Next(0, 23);
            Time[1] = new Random(DateTimeOffset.UtcNow.Millisecond).Next(0, 59);
            Time[2] = new Random(DateTimeOffset.UtcNow.Millisecond).Next(0, 59);

            Weather = new Random(DateTimeOffset.UtcNow.Millisecond).Next(0, 9);

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
            CollisionCounter = 0;
            GameProgress = 0;
            NextMapID = -1;
            GameAutoStart = true;
            LastAutoStartMsg = 0;
            FugitivePercent = 28;
            Enabled = true;
            PlayerCreated = false;
            Password = "";

            string[] lobbyBgs = {
                    "package://Artwork/CC1.jpg",
                };

            Background = lobbyBgs.OrderBy(x => Guid.NewGuid()).First();

            Settings.Lobbies.Add(this);
        }
    };

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

        public DynSet()
        {
            this.WeaponDataURL = "https://gist.githubusercontent.com/root-cause/3f29d38179b12245a003fb4fff615335/raw/7a8804b275fb1b9314cb39418a7415ea8bfd148d/v2189%2520Weapon%2520Data.json";
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
        public static readonly Vector3[] Hideouts =
        {
            new Vector3(1066.9955f, -2384.7617f, 30.421745f), // El Burro Heights
            new Vector3(-181.55147f, -1327.5475f, 31.198029f) // Strawberry
        };
        public static readonly string[] HideoutNames =
        {
            "El Burro Heights", "Strawberry"
        };

        public static readonly Vector3 BankCheckpointPos = new Vector3(207.33696f, 194.81523f, 105.56393f);

        public static readonly Vector3 SniperPos = new Vector3(267.78983f, 245.49059f, 138.19551f);
        public static readonly float SniperHeading = 161.013f;
        public static readonly Vector3 LeaderPos = new Vector3(263.71643f, 214.61465f, 101.68339f);
        public static readonly float LeaderHeading = 72.69729f;

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

        public class BR_FugitiveVehicleData
        {
            public Vector3 Position;
            public float Heading;
            public string Name;
            public BR_FugitiveVehicleData(Vector3 pos, float head, string name)
            {
                this.Position = pos;
                this.Heading = head;
                this.Name = name;
            }
        }

        public static BR_FugitiveVehicleData[] FugitiveVehicleData = 
        {
            new BR_FugitiveVehicleData(new Vector3(300.0705f, 262.5568f, 105.376785f), -105.05242f, "burrito3"),
            new BR_FugitiveVehicleData(new Vector3(294.0118f, 264.40457f, 105.40566f), -110.0321f, "burrito3"),
            new BR_FugitiveVehicleData(new Vector3(287.0754f, 266.7743f, 105.404945f), -108.04021f, "burrito3"),
            new BR_FugitiveVehicleData(new Vector3(308.86548f, 261.9242f, 104.85302f), -85.801796f, "burrito3"),
            new BR_FugitiveVehicleData(new Vector3(104.54904f, 246.66824f, 107.74905f), 69.642006f, "stretch"),
            new BR_FugitiveVehicleData(new Vector3(111.98483f, 244.39029f, 107.418335f), 68.90393f, "cognoscenti"),
            new BR_FugitiveVehicleData(new Vector3(123.95057f, 239.9692f, 107.08887f), 69.3799f, "cognoscenti"),
            new BR_FugitiveVehicleData(new Vector3(95.74224f, 250.45674f, 107.93961f), 68.840904f, "cognoscenti"),
            new BR_FugitiveVehicleData(new Vector3(83.43303f, 254.90424f, 108.37654f), 67.456f, "cognoscenti"),
            new BR_FugitiveVehicleData(new Vector3(236.19055f, 115.73223f, 102.24549f), -36.028374f, "slamvan3"),
            new BR_FugitiveVehicleData(new Vector3(240.58038f, 129.90594f, 102.196754f), 160.48413f, "glendale2"),
            new BR_FugitiveVehicleData(new Vector3(226.4308f, 118.67955f, 101.97867f), 151.46762f, "impaler"),
            new BR_FugitiveVehicleData(new Vector3(237.09787f, 130.90378f, 102.12132f), 159.22339f, "kalahari"),
            new BR_FugitiveVehicleData(new Vector3(229.15312f, 127.188934f, 102.29818f), -111.604225f, "fugitive"),
        };

        public static readonly Vector3[] PatrolPositions =
        {
            new Vector3(407.99747f, -989.2855f, 28.872591f),
            new Vector3(407.65045f, -997.8775f, 28.872187f),
            new Vector3(407.9964f, -980.1574f, 28.875753f),
            new Vector3(417.89395f, -952.3943f, 28.975674f),
            new Vector3(429.78812f, -952.223f, 28.800528f),
            new Vector3(399.7428f, -937.24005f, 28.958307f),
        };
        public static readonly float[] PatrolHeadings =
        {
             49.752483f,
             48.555405f,
            51.68575f ,
             90.1135f ,
            88.55487f ,
            179.61603f,
        };
        public static readonly Vector3[] FbiSuvPositions =
        {
            new Vector3(452.21127f, -1015.2985f, 28.092287f),
            new Vector3(454.08533f, -1023.79486f, 28.091333f),
            new Vector3(423.2366f, -1014.0081f, 28.66856f)
        };
        public static readonly float[] FbiSuvHeadings =
        {
            98.04166f,
            91.55372f,
            116.714745f
        };
        public static readonly Vector3[] FbiBuffaloPositions =
        {
            new Vector3(408.27548f, -1007.90894f, 28.913239f),
            new Vector3(406.23038f, -1003.2062f, 28.91233f)
        };
        public static readonly float[] FbiBuffaloHeadings =
        {
            123.24947f,
            125.129906f
        };

        public List<Vehicle> FugitiveVehicles = new List<Vehicle>();
        public List<Vehicle> PoliceVehicles = new List<Vehicle>();
        public Player HeistLeader = null;
        public Player Sniper = null;
        public int State = 0;
        public int Hideout = 0;
        public int LeaderLives = 2;

        public BankRobberyData()
        {
            HeistLeader = null;
            Sniper = null;
            FugitiveVehicles = new List<Vehicle>();
            PoliceVehicles = new List<Vehicle>();
            State = 0;
            Hideout = 0;
            LeaderLives = 2;
        }
    }
    public static class Settings
    {
        // Static Settings / Const
        public const string Version = "1.1";
        public const int GameRunTime = 390; // Game Running Time in Seconds before STOP
        public const int MaxLobbies = 5; // Default/starting lobby num.
        public const int DefaultMaxInLobby = 30;

        // Connection Settings
        public const string MySQL_Host = "194.163.144.228";
        public const string MySQL_User = "dbmandelivery";
        public const string MySQL_Password = "LLW5HUcpJQZ7g45PmR5rHovh7YUb89";
        public const string MySQL_Database = "racenchase";
        public const string MySQL_Port = "3369";
        public const string DCBotToken = "NjAyNTQ1Mzc0MDY1OTgzNTgw.XtaCEQ.clpqVL5c0X_38xkT-mYTr4_N9a4";

        // Global Variables (changing accordingly)
        public static List<LobbyData> Lobbies { get; set; }
        public static List<Player> LoggedInPlayers { get; set; }
        public static List<Vehicle> AdminVehicles { get; set; }
        public static int NextLobbyID { get; set; }
        public static int DerbyTimeLeft { get; set; }
        public static long LastDerbyTick { get; set; }
        public static long LastMessageTick { get; set; }
        public static long AutoSaveTick { get; set; }
        public static List<RequestData> Requests { get; set; }
        public static int NextRequestID { get; set; }
        public static long LastCountUpdate { get; set; }
        public static bool OOC { get; set; }
        public static List<LoadoutData> Loadouts { get; set; }
        public static List<CarData> Cars { get; set; }
        public static Dictionary<long, jWeaponData> WeaponData { get; set; }
        public static List<SpawnSet> FreeroamSpawns { get; set; }

        // Dyn Settings Global
        public static DynSet Dynamics { get; set; }

        // Discord Stuff
        public static long DCMentionTime { get; set; }
        public static int DCMentionPlayers { get; set; }



        public static class Timers
        { 
            public static long GameUpdate_Tick { get; set; }
            public static long TimeUpdate_Tick { get; set; }
        }


        // Locations
        public const float LoginCameraX = -1646.6002f;
        public const float LoginCameraY = -1123.7172f;
        public const float LoginCameraZ = 26.576164f;
        public const float LoginCameraRx = 90;
        public const float LoginCameraRy = 0;
        public const float LoginCameraRz = 0;
        public const float LoginCameraViewAngle = 50;

        public static Vector3[] LobbySpawns = new Vector3[]
        {
            new Vector3(117.00068f, -743.66974f, 242.15219f),
            new Vector3(123.96491f, -768.45325f, 242.15216f),
            new Vector3(124.12404f, -747.3597f, 242.15218f),
            new Vector3(121.5661f, -728.85785f, 242.15215f),
            new Vector3(109.62965f, -753.2608f, 242.15224f),
            new Vector3(109.04599f, -744.92554f, 242.15225f),
            new Vector3(135.08902f, -766.33203f, 242.152f),
        };

        public static object[] VectorJail = { new Vector3(459.55798f, -998.202f, 24.914848f), -75.06f };

        public static string[] DMNames = new string[]
        {
            "Life Invader Office",
            "Warehouse (Pistol Only)",
            "Tennis Court",
            "Construction Site",
            "Diamond Casino Rooftop",
            "Humane Labs (Pistol & Shotgun Only)",
            "Morgue (Stungun Only)",
            "Server Room"
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

    public class Main : Script
    {
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
            if(slot == 0) // Hats
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

                case "tailgater2":
                    seats = 4;
                    break;

                case "zr350":
                    seats = 2;
                    break;
            }
            return seats;
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
	        {137902532, "Vintage Pistol"},
	        {1198879012, "Flare Gun"},
	        {3696079510, "Marksman Pistol"},
	        {3249783761, "Heavy Revolver"},
	        {3415619887, "Heavy Revolver MK2"},
	        {2548703416, "Double Action"},
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
			if(colored) ret = "!{#BD3831}" + ret;
            switch (status)
            {
                case 0:
                    break;
                case 1:
                    ret = "Running";
					if(colored) ret = "!{#28993E}" + ret;
                    break;
                case 2:
                    ret = "Starting";
					if(colored) ret = "!{#00FF33}" + ret;
                    break;
            }
            return ret;
        }

        public static string ConvertGameMode(object mode, bool colored = false)
        {
            string ret = "Unknown";
			if(colored) ret = "!{#FF0000}" + ret;
            switch (mode)
            {
                case GameModes.STANDARD:
                    ret = "Standard";
					if(colored) ret = "!{#FFFFFF}" + ret;
                    break;
                case GameModes.BUSTED:
                    ret = "Busted";
					if(colored) ret = "!{#3569DB}" + ret;
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
            ContextFactory.Instance.SaveChanges();

            RAGE.Entities.Players.All.ForEach((Player p) => {
                p.SendChatMessage("!{#FFFFFF}(( !{#FF6347}Server is stopping... !{#FFFFFF}))");
                p.SendChatMessage("!{#A9C4E4}Server closed the connection.");

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

            NAPI.Util.ConsoleOutput($"(RnC | STARTUP) Connecting to database => User {Settings.MySQL_User} for db {Settings.MySQL_Database} on host {Settings.MySQL_Host}");
            ContextFactory.Instance.Database.EnsureCreated();
            NAPI.Util.ConsoleOutput("(RnC | STARTUP) Connected to database.");

            Settings.Lobbies = new List<LobbyData>();
            Settings.Requests = new List<RequestData>();
            Settings.LoggedInPlayers = new List<Player>();
            Settings.AdminVehicles = new List<Vehicle>();
            Settings.Loadouts = new List<LoadoutData>();
            Settings.Cars = new List<CarData>();
            Settings.FreeroamSpawns = new List<SpawnSet>();

            Settings.WeaponData = new Dictionary<long, jWeaponData>();

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

            try
            {
                foreach (KeyValuePair<long, jWeaponData> item in jWeaponData.FromJson(File.ReadAllText("racenchase/weapondata.json")))
                {
                    Settings.WeaponData.Add(item.Key, item.Value);
                }
            }
            catch { NAPI.Util.ConsoleOutput("(RnC | STARTUP) Unable to load in weapondata.json! Server will work but it can cause crashes due to component data."); }
            NAPI.Util.ConsoleOutput($"(RnC | STARTUP) Loading in weapondata.json... ({Settings.WeaponData.Count} weapons/entries)");

            /* DEFAULT LOADOUTS CREATION */
            NAPI.Util.ConsoleOutput("(RnC | STARTUP) Creating the loadouts...");

            new LoadoutData(1, "Default")
                .Add(WeaponHash.Pistol, "package://Artwork/Weapons/Pistol.png", "Beretta M9", 40);

            // Discord-exclusive loadout
            new LoadoutData(1, "Negotiator")
                .Add(WeaponHash.Heavypistol, "package://Artwork/Weapons/HeavyPistol.png", "M1911", 50)
                .Add(WeaponHash.Assaultshotgun, "package://Artwork/Weapons/AssaultShotgun.png", "AA-12", 20)
                .Add(WeaponHash.Bzgas, "package://Artwork/Weapons/BZGas.png", "BZ Gas", 1);
            // -------------------------

            new LoadoutData(1, "Patrol", 5)
                .Add(WeaponHash.Combatpistol, "package://Artwork/Weapons/CombatPistol.png", "Glock 17", 30)
                .Add(WeaponHash.Pumpshotgun, "package://Artwork/Weapons/PumpShotgun.png", "Remington 870", 15);

            new LoadoutData(1, "Metropolitan", 8)
                .Add(WeaponHash.Heavypistol, "package://Artwork/Weapons/HeavyPistol.png", "M1911", 40)
                .Add(WeaponHash.Pumpshotgun, "package://Artwork/Weapons/PumpShotgun.png", "Remington 870", 25)
                .Add(WeaponHash.Bzgas, "package://Artwork/Weapons/BZGas.png", "BZ Gas", 1);

            new LoadoutData(1, "FBI", 12, 15)
                .Add(WeaponHash.Pistol50, "package://Artwork/Weapons/Pistol50.png", "Desert Eagle", 40)
                .Add(WeaponHash.Bullpupshotgun, "package://Artwork/Weapons/BullpupShotgun.png", "Kel Tec KSG", 28)
                .Add(WeaponHash.Bzgas, "package://Artwork/Weapons/BZGas.png", "BZ Gas", 2);

            new LoadoutData(1, "CIA", 15, 30)
                .Add(WeaponHash.Appistol, "package://Artwork/Weapons/APPistol.png", "Colt SCAMP", 50)
                .Add(WeaponHash.Assaultshotgun, "package://Artwork/Weapons/AssaultShotgun.png", "AA-12", 20)
                .Add(WeaponHash.Bzgas, "package://Artwork/Weapons/BZGas.png", "BZ Gas", 1);

            new LoadoutData(1, "SWAT", 20, 50)
                .Add(WeaponHash.Combatpistol, "package://Artwork/Weapons/CombatPistol.png", "Glock 17", 30)
                .Add(WeaponHash.Smg, "package://Artwork/Weapons/SMG.png", "UMP-45", 60)
                .Add(WeaponHash.Pumpshotgun, "package://Artwork/Weapons/PumpShotgun.png", "Remington 870", 15)
                .Add(WeaponHash.Bzgas, "package://Artwork/Weapons/BZGas.png", "BZ Gas", 2);

            new LoadoutData(1, "Marine", 25, 70)
                .Add(WeaponHash.Heavypistol, "package://Artwork/Weapons/HeavyPistol.png", "M1911", 30)
                .Add(WeaponHash.Carbinerifle, "package://Artwork/Weapons/CarbineRifle.png", "AR-15", 60)
                .Add(WeaponHash.Bullpupshotgun, "package://Artwork/Weapons/BullpupShotgun.png", "Kel Tec KSG", 28)
                .Add(WeaponHash.Bzgas, "package://Artwork/Weapons/BZGas.png", "BZ Gas", 1);

            new LoadoutData(1, "Navy Seal", 35, 100)
                .Add(WeaponHash.Heavypistol, "package://Artwork/Weapons/HeavyPistol.png", "M1911", 30)
                .Add(WeaponHash.Assaultshotgun, "package://Artwork/Weapons/AssaultShotgun.png", "AA-12", 20)
                .Add(WeaponHash.Specialcarbine, "package://Artwork/Weapons/SpecialCarbine.png", "SCAR-H", 70)
                .Add(WeaponHash.Bzgas, "package://Artwork/Weapons/BZGas.png", "BZ Gas", 1);

            new LoadoutData(1, "Juggernaut", 50, 150)
                .Add(WeaponHash.Heavypistol, "package://Artwork/Weapons/HeavyPistol.png", "M1911", 40)
                .Add(WeaponHash.Combatmg, "package://Artwork/Weapons/CombatMG.png", "M60", 160)
                .Add(WeaponHash.Pumpshotgun, "package://Artwork/Weapons/PumpShotgun.png", "Remington 870", 15)
                .Add((WeaponHash)1, "package://Artwork/Weapons/Armour.png", "Full Armour", 100);

            //

            new LoadoutData(0, "Default")
                .Add(WeaponHash.Pistol, "package://Artwork/Weapons/Pistol.png", "Beretta M9", 40);

            // Discord-exclusive loadout
            new LoadoutData(0, "Negotiator")
                .Add(WeaponHash.Heavypistol, "package://Artwork/Weapons/HeavyPistol.png", "M1911", 50)
                .Add(WeaponHash.Heavyshotgun, "package://Artwork/Weapons/HeavyShotgun.png", "Benelli M1014", 50)
                .Add(WeaponHash.Molotov, "package://Artwork/Weapons/Molotov.png", "Molotov", 1);
            // -------------------------

            new LoadoutData(0, "Wannabe", 5)
                .Add(WeaponHash.Snspistol, "package://Artwork/Weapons/SNSPistol.png", "Makarov", 60)
                .Add(WeaponHash.Sawnoffshotgun, "package://Artwork/Weapons/SawnOffShotgun.png", "Winchester Model 1887", 40);

            new LoadoutData(0, "Hoodrat", 8, 10)
                .Add(WeaponHash.Appistol, "package://Artwork/Weapons/APPistol.png", "Colt SCAMP", 50)
                .Add(WeaponHash.Bullpupshotgun, "package://Artwork/Weapons/BullpupShotgun.png", "Kel Tec KSG", 50);

            new LoadoutData(0, "Thug", 13, 15)
                .Add(WeaponHash.Pistol, "package://Artwork/Weapons/Pistol.png", "Beretta M9", 40)
                .Add(WeaponHash.Microsmg, "package://Artwork/Weapons/MicroSMG.png", "MAC-10", 60)
                .Add(WeaponHash.Bullpupshotgun, "package://Artwork/Weapons/BullpupShotgun.png", "Kel Tec KSG", 40);

            new LoadoutData(0, "Hustler", 15, 25)
                .Add(WeaponHash.Combatpistol, "package://Artwork/Weapons/CombatPistol.png", "Glock 17", 60)
                .Add(WeaponHash.Smg, "package://Artwork/Weapons/SMG.png", "UMP-45", 70);

            new LoadoutData(0, "Trigger", 22, 40)
                .Add(WeaponHash.Pistol, "package://Artwork/Weapons/Pistol.png", "Beretta M9", 60)
                .Add(WeaponHash.Assaultrifle, "package://Artwork/Weapons/AssaultRifle.png", "AK-47", 80);

            new LoadoutData(0, "Enforcer", 27, 60)
                .Add(WeaponHash.Pistol50, "package://Artwork/Weapons/Pistol50.png", "Desert Eagle", 28)
                .Add(WeaponHash.Assaultrifle, "package://Artwork/Weapons/AssaultRifle.png", "AK-47", 80)
                .Add(WeaponHash.Pumpshotgun, "package://Artwork/Weapons/PumpShotgun.png", "Remington 870", 40);

            new LoadoutData(0, "Shot-Caller", 27)
                .Add(WeaponHash.Snspistol, "package://Artwork/Weapons/SNSPistol.png", "Makarov", 60)
                .Add(WeaponHash.Microsmg, "package://Artwork/Weapons/MicroSMG.png", "MAC-10", 70)
                .Add(WeaponHash.Heavyshotgun, "package://Artwork/Weapons/HeavyShotgun.png", "Benelli M1014", 50)
                .Add(WeaponHash.Molotov, "package://Artwork/Weapons/Molotov.png", "Molotov", 1);

            new LoadoutData(0, "Kingpin", 35, 70)
                .Add(WeaponHash.Heavypistol, "package://Artwork/Weapons/HeavyPistol.png", "M1911", 60)
                .Add(WeaponHash.Heavyshotgun, "package://Artwork/Weapons/HeavyShotgun.png", "Benelli M1014", 50)
                .Add(WeaponHash.Carbinerifle, "package://Artwork/Weapons/CarbineRifle.png", "AR-15", 80)
                .Add(WeaponHash.Molotov, "package://Artwork/Weapons/Molotov.png", "Molotov", 1);

            new LoadoutData(0, "Godfather", 50, 70)
                .Add(WeaponHash.Pistol50, "package://Artwork/Weapons/Pistol50.png", "Desert Eagle", 40)
                .Add(WeaponHash.Bullpupshotgun, "package://Artwork/Weapons/BullpupShotgun.png", "Kel Tec KSG", 50)
                .Add(WeaponHash.Specialcarbine, "package://Artwork/Weapons/SpecialCarbine.png", "SCAR-H", 80)
                .Add(WeaponHash.Molotov, "package://Artwork/Weapons/Molotov.png", "Molotov", 3);


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
            new CarData(1, "Police Bike", "policeb", "package://Artwork/Vehicles/policeb.png", "Average", 3);
            new CarData(1, "Police Buffalo", "police2", "package://Artwork/Vehicles/police2.png", "Average", 5, 5);
            new CarData(1, "Police Scout", "pscout", "package://Artwork/Vehicles/pscout.png", "Average+", 6, 10);
            new CarData(1, "Unmarked Scout", "umkscout", "package://Artwork/Vehicles/umkscout.png", "Average+", 8, 10);
            new CarData(1, "LSPD Crown Vic", "lspdvic", "package://Artwork/Vehicles/lspdvic.png", "Average+", 10, 10);
            new CarData(1, "FBI Granger", "fbi2", "package://Artwork/Vehicles/fbi2.png", "Slow", 10, 15);
            new CarData(1, "Police Buffalo A/C", "polbufac", "package://Artwork/Vehicles/polbufac.png", "Fast", 13, 20);
            new CarData(1, "Police Transporter", "policet", "package://Artwork/Vehicles/policet.png", "Snail", 15);
            new CarData(1, "FBI Buffalo A/C", "fbibufac", "package://Artwork/Vehicles/fbibufac.png", "Fast", 17, 20);
            new CarData(1, "Police Chopper", "polmav", "package://Artwork/Vehicles/polmav.png", "N/A", 20, 20);
            new CarData(1, "Unmarked Charger", "lspdunm", "package://Artwork/Vehicles/lspdunm.png", "Fast+", 25, 40);
            new CarData(1, "Unmarked Revolter", "prevolter", "package://Artwork/Vehicles/prevolter.png", "Fast+", 30, 70);

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
            new CarData(0, "Futo GTX", "futo2", "package://Artwork/Vehicles/futo2.png", "Slow", 0, 5);
            new CarData(0, "Seminole Frontier", "seminole2", "package://Artwork/Vehicles/seminole2.png", "Slow", 1);
            new CarData(0, "Huntley", "huntley", "package://Artwork/Vehicles/huntley.png", "Average", 3);
            new CarData(0, "Tailgater", "tailgater", "package://Artwork/Vehicles/tailgater.png", "Slow", 5);
            //new CarData(0, "Sanchez", "sanchez2", "package://Artwork/Vehicles/sanchez2.png", "Fast", 5, 10);
            new CarData(0, "Felon GT", "felon2", "package://Artwork/Vehicles/felon2.png", "Average+", 5);
            new CarData(0, "Huntley IV", "huntley2", "package://Artwork/Vehicles/huntley2.png", "Average", 6);
            new CarData(0, "Blista Classic", "blista2", "package://Artwork/Vehicles/blista2.png", "Average", 10);
            new CarData(0, "Cheburek", "cheburek", "package://Artwork/Vehicles/cheburek.png", "Average", 15, 10);
            new CarData(0, "Exemplar", "exemplar", "package://Artwork/Vehicles/exemplar.png", "Average+", 17, 15);
            new CarData(0, "Chevrolet Impala SS '96", "impala", "package://Artwork/Vehicles/impala.png", "Average", 18, 30);
            new CarData(0, "Buffalo", "buffalo", "package://Artwork/Vehicles/buffalo.png", "Average", 22, 30);
            new CarData(0, "Tailgater S", "tailgater2", "package://Artwork/Vehicles/tailgater2.png", "Average+", 23, 30);
            new CarData(0, "Sultan", "sultan", "package://Artwork/Vehicles/sultan.png", "Average", 25, 40);
            //new CarData(0, "Kuruma", "kuruma", "package://Artwork/Vehicles/kuruma.png", "Fast", 30, 60); KURUMA TEMPORARILY REMOVED UNTIL BULLET PROOF GTA ONLIEN BUG FIX
            new CarData(0, "Dominator GTT", "dominator8", "package://Artwork/Vehicles/dominator8.png", "Fast+", 32, 50);
            new CarData(0, "BMW M3 E46", "m3e46", "package://Artwork/Vehicles/m3e46.png", "Fast", 35, 0, 10);
            new CarData(0, "Sultan Classic", "sultan2", "package://Artwork/Vehicles/sultan2.png", "Fast", 37, 80);
            new CarData(0, "ZR350", "zr350", "package://Artwork/Vehicles/zr350.png", "Fast", 40, 70);

            if (Settings.Dynamics.Discord)
            {
                NAPI.Util.ConsoleOutput("(RnC | STARTUP) Loading discord plugin.");
                Discord.Main();

                long tick = Environment.TickCount64 + 5000;
                while (tick > Environment.TickCount64)
                {

                }
            }
            else NAPI.Util.ConsoleOutput("(RnC | OPT) Discord Bot is disabled, live commands and statuses not updated!");

            NAPI.TextLabel.CreateTextLabel("Bored of waiting? Play some DM", new Vector3(107.493866f, -744.79114f, 242.15219f), 13.0f, 2.2f, 0, new Color(255, 0, 0, 156), false);
            NAPI.TextLabel.CreateTextLabel("Press M to open menu and more", new Vector3(109.71294f, -753.1537f, 242.15219f), 13.0f, 2.2f, 0, new Color(56, 236, 242, 156), false);
            NAPI.TextLabel.CreateTextLabel("Check out other modes in the menu", new Vector3(121.34787f, -738.852f, 242.1522f), 13.0f, 2.2f, 0, new Color(40, 153, 45, 156), false);
            NAPI.TextLabel.CreateTextLabel("Change appearance with /char & /clothing", new Vector3(120.48237f, -726.71735f, 242.15198f), 13.0f, 2.2f, 0, new Color(212, 67, 217, 156), false);
            NAPI.TextLabel.CreateTextLabel("Confused? /help or /helpme", new Vector3(120.84272f, -744.534f, 242.15207f), 13.0f, 2.2f, 0, new Color(255, 255, 255, 156), false);
            NAPI.TextLabel.CreateTextLabel("Want to watch the round? /spec", new Vector3(118.04166f, -757.9125f, 242.15215f), 13.0f, 2.2f, 0, new Color(255, 228, 0, 156), false);
            
            NAPI.Util.ConsoleOutput("(RnC | STARTUP) Server startup proceeded:\n");

            string admins = "";
            int adminCount = 0;
            foreach (Account acc in ContextFactory.Instance.Accounts.ToList().Where(a => a.Admin > 0))
            {
                if (adminCount != 0) admins += ", ";
                admins += $"{acc.Username} [{acc.SQL_ID}]";
                adminCount++;
            }
            if (adminCount == 0) admins = "None";

            NAPI.Util.ConsoleOutput("[>===========|Race 'N' Chase|===========<]");
            NAPI.Util.ConsoleOutput("[Version]: " + Settings.Version);
            NAPI.Util.ConsoleOutput("[Developers]: KevY, CodeName420 (jambredrek)");
            NAPI.Util.ConsoleOutput($"[Accounts]: {ContextFactory.Instance.Accounts.Count()}");
            NAPI.Util.ConsoleOutput($"[Admins]: {admins}");
            NAPI.Util.ConsoleOutput("[>===========|Race 'N' Chase|===========<]");

            // After all the stuff is set.
            SaveDynamicSettings();

            ContextFactory.Instance.Bans.RemoveRange(ContextFactory.Instance.Bans.ToList().Where(a => a.RockstarID == 126828171 || a.HWID == "C0BC79984106C4584DAE1004223ED22035807A7CDE1E17A033C28940DFECF7C0426A40387F24FB287C0E376470BAB6B0AE649F00F6826AD83958E9AC3F16FC80"));


            ///////////////////////////////////////////////////////////////////////////////

            Settings.Timers.GameUpdate_Tick = Environment.TickCount64 + 1000;
            Settings.Timers.TimeUpdate_Tick = Environment.TickCount64 + 1000;
        }

        [ServerEvent(Event.Update)]
        public void OnUpdate()
        {
            if(Environment.TickCount64 - Settings.Timers.GameUpdate_Tick >= 1000)
            {
                GameUpdate();
                Settings.Timers.GameUpdate_Tick = Environment.TickCount64;
            }
            if (Environment.TickCount64 - Settings.Timers.TimeUpdate_Tick >= 15000)
            {
                TimeUpdate();
                Settings.Timers.TimeUpdate_Tick = Environment.TickCount64;
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
                                    if (player.GetSharedData<int>("InLobby") == lobbyid && player.GetSharedData<bool>("AutoJoin") == true)
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
                        StartGame(lobbyid);
                    }
                }
            }
        }

        public void TimeUpdate()
        {
            try
            {
                if (Environment.TickCount64 - Settings.AutoSaveTick >= 300000) // 5 mins
                {
                    ContextFactory.Instance.SaveChanges();
                    NAPI.Util.ConsoleOutput("Server auto-saved => " + DateTime.Now.ToString());

                    Settings.AutoSaveTick = Environment.TickCount64;
                }

                if (Environment.TickCount64 - Settings.LastMessageTick > 90000)
                {
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
                                                if (player.GetSharedData<int>("InLobby") == lobby.lobbyId)
                                                {
                                                    if (player.GetSharedData<int>("PlayerWeather") == -1)
                                                    {
                                                        player.TriggerEvent("SetTimeWeather", lobby.Time[0], lobby.Time[1], lobby.Time[2], lobby.Weather);
                                                        player.SendChatMessage("!{#A8FFAB}[WEATHER FORECAST] " + weatherString);
                                                    }
                                                    else
                                                    {
                                                        player.TriggerEvent("SetTimeWeather", lobby.Time[0], lobby.Time[1], lobby.Time[2], player.GetSharedData<int>("PlayerWeather"));
                                                    }


                                                }
                                            }
                                        }
                                    }

                                    foreach (Player player in Settings.LoggedInPlayers)
                                    {
                                        lock (player)
                                        {
                                            if (player.GetSharedData<int>("InLobby") == lobby.lobbyId)
                                            {
                                                if (player.GetSharedData<int>("PlayerWeather") == -1)
                                                    player.TriggerEvent("SetTimeWeather", lobby.Time[0], lobby.Time[1], lobby.Time[2], lobby.Weather);
                                                else player.TriggerEvent("SetTimeWeather", lobby.Time[0], lobby.Time[1], lobby.Time[2], player.GetSharedData<int>("PlayerWeather"));
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
                                                    if (player.GetSharedData<int>("InLobby") == lobby.lobbyId && player.GetSharedData<bool>("AutoJoin") == true && !EPlayer.IsAFK(player))
                                                    {
                                                        pCount++;
                                                    }
                                                }
                                            }

                                            if (pCount >= 2)
                                            {
                                                lobby.gameStatus = 2;

                                                if (lobby.gameMode == GameModes.STANDARD)
                                                    NAPI.Chat.SendChatMessageToAll($"!{{#33C446}}[Lobby {lobby.lobbyId}] !{{#FFFFFF}}Copchase will start in 10 seconds... ({pCount} players)");
                                                else if (lobby.gameMode == GameModes.BUSTED)
                                                    NAPI.Chat.SendChatMessageToAll($"!{{#33C446}}[Lobby {lobby.lobbyId}] !{{#FFFFFF}}BUSTED! mode will start in 10 seconds... ({pCount} players)");
                                                else if (lobby.gameMode == GameModes.BANKROBBERY)
                                                    NAPI.Chat.SendChatMessageToAll($"!{{#33C446}}[Lobby {lobby.lobbyId}] !{{#FFFFFF}}Bank Robbery event will start in 10 seconds... ({pCount} players)");

                                                NAPI.Util.ConsoleOutput($"[TimeUpdate] Game starting in 10 seconds in lobby {lobby.lobbyId}...");
                                                foreach (Player player in Settings.LoggedInPlayers)
                                                {
                                                    lock (player)
                                                    {
                                                        if (player.GetSharedData<int>("InLobby") == lobby.lobbyId && player.GetSharedData<bool>("AutoJoin") == true)
                                                        {
                                                            if (player.HasData("Spectating"))
                                                            {
                                                                Commands.cmd_specoff(player);
                                                            }
                                                            player.SetSharedData("InDM", -1);

                                                            EPlayer.Spawn(player);
                                                            EPlayer.Notify(player, "The round is about to start!", "orange", "fas fa-hourglass-start", false, 10000, "topCenter");

                                                            if (player.GetSharedData<bool>("IsTabbed") == true || EPlayer.IsAFK(player))
                                                            {

                                                                player.TriggerEvent("SendNotification", "AFK Check", "Chase is starting in 10 seconds",
                                                                    "Race 'N' Chase", 10);
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
                                                    EPlayer.SendMessageInLobby(lobby.lobbyId, "!{#FFFF00}[GAME] !{#FFFFFF}Game will not start unless there are 2+ players to start a match.", true);

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
                                                if (player.GetSharedData<int>("InLobby") == lobby.lobbyId)
                                                {
                                                    player.SetSharedData("InLobby", -1);
                                                    player.SendChatMessage("!{#FF0000}This lobby is disabled, resetting you to any other lobby.");
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
                                    }
                                }
                            }
                        }
                    }
                }

                if (Settings.Dynamics.Discord && (int)Discord._client.ConnectionState == 2)
                {
                    System.Threading.Tasks.Task.Run(() =>
                    {
                        if (Settings.LoggedInPlayers.Count <= 1 && Settings.DCMentionTime > Environment.TickCount64) // Mention time is high but don't have pb
                            Settings.DCMentionTime = Environment.TickCount64 - 600000; // Allow it to mention again next time players after 5 mins.

                        if (Environment.TickCount64 - Settings.DCMentionTime >= 900000) // 15 mins passed?
                        {
                            if (Settings.LoggedInPlayers.Count >= 2)
                            {
                                if (Settings.LoggedInPlayers.Count >= (Settings.DCMentionPlayers * 2) || Settings.DCMentionPlayers == 0) // Players doubled
                                {
                                    // trigger
                                    foreach (var guild in Discord._client.Guilds.Where(a => a.Id == Settings.Dynamics.DCServerID))
                                    {
                                        var botchan = guild.TextChannels.ToList().Find(a => a.Id == Settings.Dynamics.DCMentionRoleChan);

                                        if (botchan != null)
                                        {
                                            string playerDiff = "0";
                                            if (Settings.LoggedInPlayers.Count - Settings.DCMentionPlayers >= 0)
                                            {
                                                playerDiff = $"+{Settings.LoggedInPlayers.Count - Settings.DCMentionPlayers}";
                                            }
                                            else playerDiff = $"{Settings.LoggedInPlayers.Count - Settings.DCMentionPlayers}";

                                            botchan.SendMessageAsync($"<@&{Settings.Dynamics.DCMentionRoleID}> **There are {Settings.LoggedInPlayers.Count} players ingame.** \n" +
                                             $"Last proper update: **{(Environment.TickCount64 - Settings.DCMentionTime) / 1000}s** ago. | " +
                                             $"Last update players: **{Settings.DCMentionPlayers}** ({playerDiff} since before)");
                                        }
                                    }

                                    Settings.DCMentionTime = Environment.TickCount64;

                                    Settings.DCMentionPlayers = Settings.LoggedInPlayers.Count;
                                }
                                else
                                {
                                    Settings.DCMentionPlayers = (Settings.LoggedInPlayers.Count / 2); // Half the players, but next time mention in 30 mins.
                                    Settings.DCMentionTime = Environment.TickCount64 + (1800000 - 900000); // Mention next in half an hour unless PB drops.
                                }
                            }
                            else Settings.DCMentionPlayers = Settings.LoggedInPlayers.Count;
                        }
                        if (Environment.TickCount64 - Settings.LastCountUpdate >= 60000)
                        {
                            Settings.LastCountUpdate = Environment.TickCount64;
                            Discord._client.SetGameAsync($"{RAGE.Entities.Players.Count} Players & {Settings.Lobbies.Where(l => l.gameStatus == 1).Count()} Chases");
                            //NAPI.Util.ConsoleOutput("Updated the DISCORD counts.");
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput("[EXCEPTION IN TimeUpdate]: " + ex.ToString());
            }
            return;
        }

        public static void CheckDerbyWinner()
        {
            int derbyPlayers = Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == -4).Count();
            if (Settings.DerbyTimeLeft <= 0 && derbyPlayers >= 2)
            {
                Player winner = null;
                int lastBigTime = 0;
                int winXP = derbyPlayers * 2;

                foreach(Player player in Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == -4))
                {
                    if(player.GetSharedData<int>("DerbyTime") > lastBigTime)
                    {
                        lastBigTime = player.GetSharedData<int>("DerbyTime");
                        winner = player;
                    }
                }

                foreach (Player player in Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == -4))
                {
                    player.Health = 0;
                    if (winner == null)
                    {
                        player.SendChatMessage("!{#E6DD3C}[DERBY] !{#FFFFFF}There was no winner in the derby match! Starting a new round.");
                    }
                    else
                    {
                        player.SendChatMessage("!{#E6DD3C}[DERBY] " + winner.Name + " won the derby and has gained " + winXP + "XP and +5% fugitive chance.");
                    }
                }

                if (winner != null)
                {
                    EPlayer.GetAccount(winner).DerbyWins++;
                    EPlayer.GrantXP(winner, winXP);
					Discord.SendTextChannel("staff-bot", $"**(Derby) | Winner: {winner.Name} | XP Gained: {winXP} | Participants: {Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == -4).Count()}**");

                    winner.SetSharedData("FugitiveChance", winner.GetSharedData<float>("FugitiveChance") + 5 + (double)((double)EPlayer.GetAccount(winner).DonatorLevel / 2.0));
                }

                Settings.DerbyTimeLeft = 180;
                Settings.LastDerbyTick = Environment.TickCount64 + 3000;
            }
            return;
        }
        public static bool StopGame(int lobbyid, int reason)
        {
            LobbyData lobby = Settings.Lobbies.Find(l => l.lobbyId == lobbyid);
            if (lobby != null && lobby.gameStatus == 1)
            {
                switch (reason)
                {
                    case 0:
                        NAPI.Chat.SendChatMessageToAll($"!{{#33C446}}[Lobby {lobbyid}] !{{#FF0000}}The game has been stopped by an administrator.");
                        Discord.SendTextChannel("staff-bot", $"**(Lobby {lobbyid}) Copchase has ended! | Stopped by an admin.**");
                        break;
                    case 1: // time run out
                        if (lobby.gameMode != GameModes.BANKROBBERY)
                        {
                            NAPI.Chat.SendChatMessageToAll($"!{{#33C446}}[Lobby {lobbyid}] !{{#FFFFFF}}The fugitives managed to escape from the cops.");
                            Discord.SendTextChannel("staff-bot", $"**(Lobby {lobbyid}) Copchase has ended! | Fugitives have escaped.**");

                            foreach (Player i in lobby.Fugitives)
                            {
                                if (lobby.GameProgress <= 40) continue;
                                // Reward
                                EPlayer.GetAccount(i).Escapes++;
                                EPlayer.GrantXP(i, 5);
                                EPlayer.GivePoints(i, 5);
                                i.SetSharedData("FugitiveChance", i.GetSharedData<float>("FugitiveChance") + 0.3);
                            }
                        }
                        else
                        {
                            if (lobby.gameMode == GameModes.BANKROBBERY)
                            {
                                NAPI.Chat.SendChatMessageToAll($"!{{#33C446}}[Lobby {lobbyid}] !{{#FFFFFF}}The fugitives didn't manage to rob the bank in time. Cops win!");
                                Discord.SendTextChannel("staff-bot", $"**(Lobby {lobbyid}) Copchase has ended! | Fugitives didn't manage to rob the bank in time.**");

                                foreach (Player i in lobby.Cops)
                                {
                                    if (lobby.GameProgress <= 40) continue;
                                    // Reward
                                    EPlayer.GrantXP(i, 8);
                                    EPlayer.GivePoints(i, 7);
                                    i.SetSharedData("FugitiveChance", i.GetSharedData<float>("FugitiveChance") + 0.5);
                                }
                            }
                        }
                        break;
                    case 2:
                        NAPI.Chat.SendChatMessageToAll($"!{{#33C446}}[Lobby {lobbyid}] !{{FFFFFF}}All law enforcement units have been taken out by the fugitives.");
                        Discord.SendTextChannel("staff-bot", $"**(Lobby {lobbyid}) Copchase has ended! | All cops have been eliminated.**");

                        if (lobby.gameMode != GameModes.BANKROBBERY)
                        {
                            foreach (Player i in lobby.Fugitives)
                            {
                                if (lobby.GameProgress <= 40) continue;
                                // Reward
                                EPlayer.GrantXP(i, 3);
                                EPlayer.GivePoints(i, 2);
                                i.SetSharedData("FugitiveChance", i.GetSharedData<float>("FugitiveChance") + 0.2);
                            }
                        }
                        else
                        {
                            if (lobby.gameMode == GameModes.BANKROBBERY)
                            {
                                foreach (Player i in lobby.Fugitives)
                                {
                                    // Reward
                                    EPlayer.GrantXP(i, 8);
                                    EPlayer.GivePoints(i, 7);
                                    i.SetSharedData("FugitiveChance", i.GetSharedData<float>("FugitiveChance") + 0.1);
                                }
                            }
                        }
                        break;
                    case 3:
                        NAPI.Chat.SendChatMessageToAll($"!{{#33C446}}[Lobby {lobbyid}] !{{#FFFFFF}}The situation has been successfully handled by the police.");
                        Discord.SendTextChannel("staff-bot", $"**(Lobby {lobbyid}) Copchase has ended! | Police has neutralized all fugitives.**");

                        if (lobby.gameMode != GameModes.BANKROBBERY)
                        {
                            foreach (Player i in lobby.Cops)
                            {
                                if (lobby.GameProgress <= 40) continue;
                                // Reward
                                EPlayer.GrantXP(i, 4);
                                EPlayer.GivePoints(i, 2);
                                i.SetSharedData("FugitiveChance", i.GetSharedData<float>("FugitiveChance") + 0.3);
                            }
                        }
                        else
                        {
                            if(lobby.gameMode == GameModes.BANKROBBERY)
                            {
                                foreach (Player i in lobby.Cops)
                                {
                                    // Reward
                                    EPlayer.GrantXP(i, 8);
                                    EPlayer.GivePoints(i, 7);
                                    i.SetSharedData("FugitiveChance", i.GetSharedData<float>("FugitiveChance") + 0.5);
                                }
                            }
                        }
                        break;
                    case 4:
                        NAPI.Chat.SendChatMessageToAll($"!{{#33C446}}[Lobby {lobbyid}] !{{#FFFFFF}}Game has ended due to lack of players.");
                        Discord.SendTextChannel("staff-bot", $"**(Lobby {lobbyid}) Copchase has ended! | Game was ended due to lack of players.**");
                        break;
                    case 5: // event cop win
                        NAPI.Chat.SendChatMessageToAll($"!{{#33C446}}[Lobby {lobbyid}] !{{#FFFFFF}}Bank Robbery has ended - there was no one left to replace the heist leader. Cops win!");
                        Discord.SendTextChannel("staff-bot", $"**(Lobby {lobbyid}) Copchase has ended! | No one was left to replace the heist leader.**");
                        foreach (Player i in lobby.Cops)
                        {
                            // Reward
                            EPlayer.GrantXP(i, 8);
                            EPlayer.GivePoints(i, 6);
                            i.SetSharedData("FugitiveChance", i.GetSharedData<float>("FugitiveChance") + 0.5);
                        }
                        break;
                    case 6: // event fugi win
                        NAPI.Chat.SendChatMessageToAll($"!{{#33C446}}[Lobby {lobbyid}] !{{#FFFFFF}}Bank Robbery has ended - fugitives successfully delivered the money to their hideout!");
                        Discord.SendTextChannel("staff-bot", $"**(Lobby {lobbyid}) Copchase has ended! | Fugitives successfully delivered the money to their hideout.**");
                        foreach (Player i in lobby.Fugitives)
                        {
                            // Reward
                            EPlayer.GrantXP(i, 10);
                            EPlayer.GivePoints(i, 13);
                            i.SetSharedData("FugitiveChance", i.GetSharedData<float>("FugitiveChance") + 0.2);
                        }
                        break;
                }

                lobby.gameStatus = 0;

                foreach (Player i in Settings.LoggedInPlayers)
                {
                    if (i.GetSharedData<int>("InLobby") != lobby.lobbyId) continue;

                    i.TriggerEvent("ResetKillFeed");
                    i.SetSharedData("HeistLeader", false);
                    i.TriggerEvent("BankRob_ClearFugiCarBlips");
                    i.TriggerEvent("BankRob_ClearFinishMarker");

                    Events.OnPlayerWounded(i, 0, -1);

                    if (lobby.Fugitives.Contains(i))
                    {
                        Account a = EPlayer.GetAccount(i);
                        CharacterData cd = NAPI.Util.FromJson<CharacterData>(a.Character);
                        EPlayer._ApplyCharacter(i, cd, a.LobbyClothing);
                    }


                    if (i.HasData("Spectating") && EPlayer.GetAccount(i).Admin < 1)
                    {
                        Commands.cmd_specoff(i);
                    }

                    i.TriggerEvent("HideChaseTimer");
                    if (lobby.Cops.Contains(i))
                    {
                        lobby.Cops.Remove(i);
                        EPlayer.Spawn(i);
                    }
                    if (lobby.Fugitives.Contains(i))
                    {
                        lobby.Fugitives.Remove(i);
                        EPlayer.Spawn(i);
                    }
                }

                for (int i = lobby.Vehicles.Count - 1; i >= 0; i--)
                {
                    if (!lobby.Vehicles[i].IsNull && lobby.Vehicles[i] != null)
                    {
                        lobby.Vehicles[i].Delete();
                        lobby.Vehicles.RemoveAt(i);
                    }
                }

                if(lobby.Vehicles.Count > 0)
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

                if(lobby.gameMode == GameModes.BANKROBBERY)
                {
                    lobby.gameMode = GameModes.STANDARD;
                }
            }
            else return false;

            return true;
        }

        public static bool StartGame(int lobbyid)
        {
            try
            {
                LobbyData lobby = Settings.Lobbies.Find(l => l.lobbyId == lobbyid && l.Enabled == true);
                if (lobby != null && lobby.gameStatus != 1)
                {
                    if (lobby.gameStatus == 1) { NAPI.Util.ConsoleOutput("[Lobby " + lobbyid + "] Copchase is already running, failed to start"); return false; }

                    lobby.gameStatus = 0;
                    int pCount = 0;

                    lobby.Cops.Clear();
                    lobby.Fugitives.Clear();
                    lobby.GameProgress = 0;

                    foreach (Player player in Settings.LoggedInPlayers)
                    {
                        if (player.GetSharedData<int>("InLobby") == lobbyid && player.GetSharedData<bool>("AutoJoin") == true && !EPlayer.IsAFK(player) &&
                            !player.HasData("Spectating"))
                        {
                            player.SetSharedData("InDM", -1);

                            pCount++;
                        }
                    }

                    if (pCount < 2) { NAPI.Util.ConsoleOutput("[Lobby " + lobbyid + "] Total players less than two, failed to start"); return false; }

                    decimal perc = (decimal)0;
                    if(lobby.gameMode != GameModes.BANKROBBERY) perc = (decimal)pCount * ((decimal)lobby.FugitivePercent / (decimal)100);
                    else
                    {
                        if(lobby.gameMode == GameModes.BANKROBBERY) perc = (decimal)pCount * ((decimal)55.0 / (decimal)100);
                    }

                    int fugitives = (int)Decimal.Round(perc);

                    if (fugitives < 1) { NAPI.Util.ConsoleOutput("[Lobby " + lobbyid + "] Fugitives count is zero, failed to start"); return false; }

                    if (fugitives == pCount) fugitives--; // There will ALWAYS be AT LEAST one COP.

                    int crashcheck = 0;
                    try
                    {
                        List<MapSpawn> Spawns = new List<MapSpawn>();
                        List<int> UsedSpawns = new List<int>();
                        List<Player> lobbyPlayers = new List<Player>();
                        List<Player> PutInsideCar = new List<Player>();
                        Player _tempPlayer;
                        Map map = null;
                        crashcheck++;
                        if (lobby.gameMode != GameModes.BANKROBBERY)
                        {
                            if (ContextFactory.Instance.Maps.Count() < 1) { NAPI.Util.ConsoleOutput("[Lobby " + lobbyid + "] Server has no maps added, failed to start"); return false; }
                            crashcheck++;

                            List<Map> mapList = ContextFactory.Instance.Maps.ToList().Where(a => a.Enabled == true).ToList();
                            crashcheck++;

                            if (mapList.Count < 1) { NAPI.Util.ConsoleOutput("[Lobby " + lobbyid + "] Server has maps but none of them were enabled! Failed to start."); return false; }
                            crashcheck++;

                            map = mapList.OrderBy(c => Guid.NewGuid()).FirstOrDefault();
                            if (lobby.NextMapID != -1) map = mapList.Find(a => a.SQL_ID == lobby.NextMapID);
                            lobby.NextMapID = -1;

                            if (map == null) { NAPI.Util.ConsoleOutput("[Lobby " + lobbyid + "] Next (force-set) MapID == null; Failed to start!"); return false; }
                            crashcheck++;

                            NAPI.Util.ConsoleOutput($"[Lobby " + lobbyid + $"] Selected Map [{map.Name}] | Total Spawns: [{ContextFactory.Instance.MapSpawns.ToList().Where(a => a.mapID == map.SQL_ID).Count()}]");
                            crashcheck++;

                            lobby.lastMapId = map.SQL_ID;

                            if (ContextFactory.Instance.MapSpawns.ToList().Where(a => a.mapID == map.SQL_ID && a.spawnTeam == 0).Count() < 1) { NAPI.Util.ConsoleOutput($"[Lobby {lobbyid}] Map {map.Name} has no fugitive spawns! Abort."); return false; }
                            if (ContextFactory.Instance.MapSpawns.ToList().Where(a => a.mapID == map.SQL_ID && a.spawnTeam == 1).Count() < 1) { NAPI.Util.ConsoleOutput($"[Lobby {lobbyid}] Map {map.Name} has no cop spawns! Abort."); return false; }

                            crashcheck++;

                            Spawns = ContextFactory.Instance.MapSpawns.ToList().Where(x => x.mapID == map.SQL_ID).ToList();
                            crashcheck++;

                            UsedSpawns = new List<int>(Spawns.Count + 1);
                            crashcheck++;

                            lobbyPlayers = Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == lobbyid && !EPlayer.IsAFK(a) && !a.HasData("Spectating") && a.GetSharedData<bool>("AutoJoin") == true && !a.Dead).OrderBy(c => Guid.NewGuid()).ToList();
                            // !a.Dead so the white color bug (OnPlayerSpawn trigger as game start) does not happen -- if it still happens or if you want to enable dead players being taken in game then do it that EPlayer.Spawn is called when selecting players again.
                            crashcheck++;

                            PutInsideCar = new List<Player>(lobbyPlayers.Count());
                            crashcheck++;

                            UsedSpawns.Add(-1);
                            crashcheck++;


                            pCount = 0;
                            crashcheck++;
                        }
                        else
                        {
                            lobbyPlayers = Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == lobbyid && !EPlayer.IsAFK(a) && !a.HasData("Spectating") && a.GetSharedData<bool>("AutoJoin") == true && !a.Dead).OrderBy(c => Guid.NewGuid()).ToList();
                        }

                        while (fugitives > 0)
                        {
                            // FULL RANDOM CHANCE MODE 
                            /*
                            if (lobbyPlayers.Where(a => a.HasData("GrantFugitive") && !lobby.Fugitives.Contains(a) && a.GetSharedData<bool>("AutoJoin") == true).Count() > 0)
                            {
                                _tempPlayer = lobbyPlayers.Where(a => a.HasData("GrantFugitive") && !lobby.Fugitives.Contains(a) && a.GetSharedData<bool>("AutoJoin") == true).OrderBy(c => Guid.NewGuid()).First();
                            }
                            else _tempPlayer = lobbyPlayers.OrderBy(c => Guid.NewGuid()).First();

                            */

                            // FULL RANDOM CHANCE MODE END

                            /* HIGHEST FUGITIVE CHANCE MODE */
                            var highestFugitives = lobbyPlayers.Where(p => !lobby.Fugitives.Contains(p) && p.GetSharedData<bool>("AutoJoin") == true).OrderByDescending(p => p.HasData("GrantFugitive")).ThenByDescending(p => p.GetSharedData<float>("FugitiveChance"));

                            if (highestFugitives.Count() == 0) _tempPlayer = lobbyPlayers.OrderBy(c => Guid.NewGuid()).First();
                            else _tempPlayer = highestFugitives.First();


                            // HIGHEST CHANCE MODE END

                            if (_tempPlayer.GetSharedData<bool>("AutoJoin") == false) continue;


                            if (!lobby.Fugitives.Contains(_tempPlayer))
                            {
                                fugitives--;
                                lobby.Fugitives.Add(_tempPlayer);
                                _tempPlayer.SetSharedData("FugitiveChance", 10.0);
                            }
                            else pCount++;

                            if (pCount >= 100)
                            {
                                NAPI.Util.ConsoleOutput("[Lobby " + lobbyid + "] Stuck inside random iteration loop for selecting fugitive, aborting method.");
                                return false;
                            }
                        }
                        fugitives = lobby.Fugitives.Count;
                        crashcheck++;

                        foreach (Player player in lobbyPlayers)
                        {
                            if (player.GetSharedData<bool>("AutoJoin") != true) continue;

                            player.SetSharedData("BustedProgress", 0.0);
                            player.ResetData("BustSupporters");
                            player.ResetSharedData("LastKiller");
                            player.ResetSharedData("BeingRevived");
                            player.ResetSharedData("ReviveTarget");
                            player.SetSharedData("ClimbProtection", false);
                            player.SetData("LastClimbProtection", Environment.TickCount64 - 30000);
                            player.SetData("ClimbProtectionProcs", 0);
                            player.SetData("ClimbProtectionEnd", Environment.TickCount64);
                            player.SetSharedData("CuffedBy", "n/a");
                            player.TriggerEvent("moveSkyCamera", "up", 1, false);
                            player.TriggerEvent("moveSkyCamera", "down", 1, false, 5000);
                            player.TriggerEvent("ToggleNametags", false);
                            player.SetSharedData("Collisions", false);
                            player.ResetData("Seatbelt");
                            player.TriggerEvent("ToggleSeatbelt", false);

                            EPlayer.Freeze(player, false);

                            player.RemoveAllWeapons();
                            player.Health = 100;
                            player.Armor = 0;  // ARMOR AND GEAR IS GIVEN ON COLLISIONS END
                            player.Dimension = Convert.ToUInt32(699 + player.Id);

                            if (lobby.gameMode == GameModes.STANDARD)
                                player.TriggerEvent("showHPBars", 1);
                            else if (lobby.gameMode == GameModes.BUSTED)
                                player.TriggerEvent("showHPBars", 3);
                            else if (lobby.gameMode == GameModes.BANKROBBERY)
                                player.TriggerEvent("showHPBars", 1);

                            if (!lobby.Fugitives.Contains(player))
                            {
                                lobby.Cops.Add(player);
                                player.SetSharedData("FugitiveChance", player.GetSharedData<float>("FugitiveChance") + 1.0);
                            }
                        }
                        crashcheck++;

                        MapSpawn spawnLoc = null;
                        pCount = 0;

                        if (lobby.gameMode != GameModes.BANKROBBERY)
                        {
                            foreach (Player player in lobby.Cops.OrderBy(x => Guid.NewGuid()))
                            {
                                spawnLoc = null;

                                foreach (MapSpawn tSpawn in Spawns.Where(a => a.spawnTeam == 1))
                                {
                                    if (!UsedSpawns.Contains(tSpawn.SQL_ID))
                                    {
                                        spawnLoc = tSpawn;
                                        break;
                                    }
                                }

                                if (EPlayer.GetAccount(player).CarCop.Equals("polmav", StringComparison.OrdinalIgnoreCase))
                                {
                                    spawnLoc = null;
                                    foreach (MapSpawn tSpawn in Spawns.Where(a => a.spawnTeam == 2))
                                    {
                                        if (!UsedSpawns.Contains(tSpawn.SQL_ID))
                                        {
                                            spawnLoc = tSpawn;
                                            break;
                                        }

                                        if (spawnLoc != null)
                                        {
                                            if (!PutInsideCar.Contains(player)) UsedSpawns.Add(spawnLoc.SQL_ID);
                                        }
                                        else
                                        {
                                            pCount++;
                                            spawnLoc = new MapSpawn();

                                            spawnLoc = Spawns.First(a => a.spawnTeam == 1);

                                            //spawnLoc.spawnZ += (pCount * 5); // If no spawns left, stack cars ontop so at least they don't stuck in each other.
                                            //spawnLoc.spawnX += (pCount * 2);
                                        }
                                    }
                                }


                                if (spawnLoc != null)
                                {
                                    if (!PutInsideCar.Contains(player)) UsedSpawns.Add(spawnLoc.SQL_ID);
                                }
                                else
                                {
                                    pCount++;
                                    spawnLoc = new MapSpawn();

                                    spawnLoc = Spawns.First(a => a.spawnTeam == 1);

                                    //spawnLoc.spawnZ += (pCount * 5); // If no spawns left, stack cars ontop so at least they don't stuck in each other.
                                    //spawnLoc.spawnX += (pCount * 2);
                                }

                                Vector3 vecSpawn = new Vector3(spawnLoc.spawnX, spawnLoc.spawnY, spawnLoc.spawnZ);

                                CharacterData ConvChar = new CharacterData();

                                try
                                {
                                    ConvChar = NAPI.Util.FromJson<CharacterData>(EPlayer.GetAccount(player).Character);

                                    EPlayer.ApplyCharacter(player, ConvChar, 1);
                                }
                                catch { }

                                EPlayer.SetColor(player, new Color(0, 151, 255), 0);

                                if (!PutInsideCar.Contains(player))
                                {
                                    string carName = "police";

                                    int col1 = 0; // Black?
                                    int col2 = 111; // Frost white

                                    float enginePower = 1.0f;

                                    if (EPlayer.GetAccount(player).CarCop.Length < 3)
                                    {
                                        switch (new Random(DateTimeOffset.UtcNow.Millisecond).Next(0, 3))
                                        {
                                            case 0:
                                                carName = "police";
                                                col1 = 111;
                                                enginePower = 30.0f;
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
                                        carName = EPlayer.GetAccount(player).CarCop;

                                        if (carName.StartsWith("police"))
                                        {
                                            enginePower = 10.0f;
                                            col1 = 111;
                                        }

                                        // incrementing times this police vehicle was used
                                        if (EPlayer.HasOwnedVehicle(player, carName, 1))
                                        {
                                            OwnedVehData ownedVeh = ContextFactory.Instance.OwnedVehicles.ToList().Find(v => v.AccountSQLID == EPlayer.GetAccount(player).SQL_ID
                                                                    && v.Model.Equals(carName, StringComparison.OrdinalIgnoreCase));
                                            if (ownedVeh != null)
                                                ownedVeh.TimesUsed++;
                                        }

                                    }

                                    if (carName.Equals("prevolter")) col2 = 0; // Full black undercover brr.
                                    if (carName.Equals("police4")) col1 = 8; // Stone Silver // Undercover Default Cruiser
                                    int overridemaxseats = GetMaxSeats(carName);

                                    if (carName.Equals("polmav"))
                                    {

                                    }

                                    if (carName.Equals("lspdvic")) enginePower = 10.0f;
                                    if (carName.Equals("fbi2")) enginePower = 15.0f;
                                    if (carName.Equals("policet")) enginePower = 12.0f;

                                    Vehicle pVehicle = NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(carName), vecSpawn, spawnLoc.spawnA, col1, col2, player.Name, 255, false, true, Convert.ToUInt32(lobbyid));
                                    pVehicle.EngineStatus = true;
                                    pVehicle.SetSharedData("Engine", true);
                                    pVehicle.SetSharedData("Locked", true);
                                    pVehicle.Locked = true;
                                    pVehicle.SetSharedData("Team", 1);

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


                                    var inst = ContextFactory.Instance.PlayerTunings.ToList().Find(a => a.Model == pVehicle.Model && a.AccountSQLID == EPlayer.GetAccount(player).SQL_ID);
                                    if (inst != null)
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
                                    }

                                    lobby.Vehicles.Add(pVehicle);

                                    pVehicle.Position = vecSpawn;
                                    pVehicle.SetData("SpawnA", spawnLoc.spawnA);

                                    player.TriggerEvent("SetChaseVehicle", pVehicle.Id, 0);
                                    player.SetData("ChaseVehicle", pVehicle);
                                    player.SetData("ChaseVehicleSeat", 0);
                                    PutInsideCar.Add(player);

                                    int seat = 1;
                                    int maxseats = pVehicle.MaxOccupants;
                                    if (overridemaxseats != 0) maxseats = overridemaxseats;

                                    if (player.HasData("PlayerSquad"))
                                    {
                                        foreach (Player mem in Settings.LoggedInPlayers.Where(p => p.HasData("PlayerSquad") && p.GetData<Player>("PlayerSquad") == player.GetData<Player>("PlayerSquad") && lobby.Cops.Contains(p) && p.Id != player.Id))
                                        {
                                            if (seat < maxseats)
                                            {
                                                if (carName.Equals("polmav"))
                                                {
                                                    // 4 - seat
                                                    // 4-1, 3
                                                    // 4-2, 2
                                                    // 4-3, 1
                                                    mem.TriggerEvent("SetChaseVehicle", pVehicle.Id, maxseats - seat);
                                                    mem.SetData("ChaseVehicle", pVehicle);
                                                    mem.SetData("ChaseVehicleSeat", maxseats - seat);
                                                }
                                                else
                                                {
                                                    // 0 + 1, 2, 3
                                                    mem.TriggerEvent("SetChaseVehicle", pVehicle.Id, 0 + seat);
                                                    mem.SetData("ChaseVehicle", pVehicle);
                                                    mem.SetData("ChaseVehicleSeat", 0 + seat);
                                                }

                                                PutInsideCar.Add(mem);
                                                mem.SendChatMessage("!{#FFAC38}[Partners] You are being paired with " + player.Name + " this chase.");

                                                seat++;
                                            }
                                        }
                                    }
                                }

                                EPlayer.Freeze(player, true);

                                player.SetData("GPS", false);

                                EPlayer.SetDiscordStatus(player, $"[Lobby {lobbyid}] Cop as {player.Name}");

                                player.TriggerEvent("SetActorInvulnerable", false);
                                player.TriggerEvent("ToggleAFOB", Settings.Dynamics.AntiFallOffBike, Settings.Dynamics.AntiFallOffBikeSpeedLimit);
                                player.SetSharedData("CuffedBy", "n/a");
                                player.SetSharedData("Team", 1);
                            }
                        }
                        else
                        {
                            if (lobby.gameMode == GameModes.BANKROBBERY)
                            {
                                // clearing the data
                                lobby.BankRobbery.PoliceVehicles.Clear();
                                lobby.BankRobbery.FugitiveVehicles.Clear();
                                lobby.BankRobbery = new BankRobberyData();
                                crashcheck++;

                                uint dimension = 100 + (uint)lobby.lobbyId;
                                for (int i = 0; i < BankRobberyData.PatrolHeadings.Length; i++)
                                {
                                    Vehicle _v = NAPI.Vehicle.CreateVehicle((uint)VehicleHash.Police, BankRobberyData.PatrolPositions[i], BankRobberyData.PatrolHeadings[i],
                                        111, 0, "PATROL", 255, false, true, Convert.ToUInt32(lobbyid));
                                    lobby.BankRobbery.PoliceVehicles.Add(_v);
                                    lobby.Vehicles.Add(_v);
                                }
                                crashcheck++;
                                for (int i = 0; i < BankRobberyData.FbiBuffaloHeadings.Length; i++)
                                {
                                    Vehicle _v = NAPI.Vehicle.CreateVehicle((uint)VehicleHash.Fbi, BankRobberyData.FbiBuffaloPositions[i], BankRobberyData.FbiBuffaloHeadings[i],
                                        0, 0, "FIB", 255, false, true, Convert.ToUInt32(lobbyid));
                                    lobby.BankRobbery.PoliceVehicles.Add(_v);
                                    lobby.Vehicles.Add(_v);
                                }
                                crashcheck++;
                                for (int i = 0; i < BankRobberyData.FbiSuvHeadings.Length; i++)
                                {
                                    Vehicle _v = NAPI.Vehicle.CreateVehicle((uint)VehicleHash.Fbi2, BankRobberyData.FbiSuvPositions[i], BankRobberyData.FbiSuvHeadings[i],
                                        0, 0, "NOOSE", 255, false, true, Convert.ToUInt32(lobbyid));
                                    lobby.BankRobbery.PoliceVehicles.Add(_v);
                                    lobby.Vehicles.Add(_v);
                                }
                                crashcheck++;

                                foreach (Player player in lobby.Cops.OrderBy(x => Guid.NewGuid()))
                                {
                                    EPlayer.Freeze(player, true);

                                    player.SetData("GPS", false);

                                    EPlayer.SetDiscordStatus(player, $"[Lobby {lobbyid}] Cop as {player.Name}");

                                    EPlayer.SetColor(player, new Color(0, 151, 255), 0);
                                    player.TriggerEvent("SetActorInvulnerable", false);
                                    player.TriggerEvent("ToggleAFOB", Settings.Dynamics.AntiFallOffBike, Settings.Dynamics.AntiFallOffBikeSpeedLimit);
                                    player.SetSharedData("CuffedBy", "n/a");
                                    player.SetSharedData("Team", 1);

                                    EPlayer.Notify(player, "<strong>Please wait</strong>, Bank Robbery is loading...", "yellow", "fas fa-spinner", false, 15000); 
                                }
                            }
                        }

                        spawnLoc = null;
                        pCount = 0;
                        crashcheck++;

                        if (lobby.gameMode != GameModes.BANKROBBERY)
                        {
                            foreach (Player player in lobby.Fugitives.OrderBy(x => Guid.NewGuid()))
                            {
                                spawnLoc = new MapSpawn();

                                spawnLoc = Spawns.First(a => a.spawnTeam == 0);

                                Vector3 vecSpawn = new Vector3(spawnLoc.spawnX, spawnLoc.spawnY, spawnLoc.spawnZ);

                                CharacterData ConvChar = new CharacterData();

                                try
                                {
                                    ConvChar = NAPI.Util.FromJson<CharacterData>(EPlayer.GetAccount(player).Character);

                                    EPlayer.ApplyCharacter(player, ConvChar, 0);
                                }
                                catch { }

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
                                player.SetSharedData("HasShot", false);
                            }

                            spawnLoc = null;
                            List<Player> iteratedAlready = new List<Player>();
                            fugitives = lobby.Fugitives.Count;

                            while (fugitives > 0)
                            {
                                _tempPlayer = lobby.Fugitives.OrderBy(c => Guid.NewGuid()).First();

                                if (lobby.Fugitives.Count(a => !PutInsideCar.Contains(a) && !iteratedAlready.Contains(a) && a.Ping < 350) > 0)
                                {
                                    _tempPlayer = lobby.Fugitives.Where(a => !PutInsideCar.Contains(a) && !iteratedAlready.Contains(a) && a.Ping < 350).OrderBy(c => Guid.NewGuid()).FirstOrDefault();
                                }

                                if (PutInsideCar.Contains(_tempPlayer))
                                {
                                    if (!iteratedAlready.Contains(_tempPlayer))
                                    {
                                        iteratedAlready.Add(_tempPlayer);
                                        fugitives--;
                                    }

                                    continue;
                                }

                                fugitives--;
                                PutInsideCar.Add(_tempPlayer);
                                iteratedAlready.Add(_tempPlayer);

                                spawnLoc = null;
                                foreach (MapSpawn tSpawn in Spawns.Where(a => a.spawnTeam == 0))
                                {
                                    if (!UsedSpawns.Contains(tSpawn.SQL_ID))
                                    {
                                        spawnLoc = tSpawn;
                                        break;
                                    }
                                }

                                if ((spawnLoc == null) == false)
                                {
                                    UsedSpawns.Add(spawnLoc.SQL_ID);
                                }
                                else
                                {
                                    pCount++;
                                    spawnLoc = new MapSpawn();

                                    spawnLoc = Spawns.First(a => a.spawnTeam == 0);

                                    //spawnLoc.spawnZ += (pCount * 5); // If no spawns left, stack cars ontop so at least they don't stuck in each other.
                                    //spawnLoc.spawnX += (pCount * 2);
                                }

                                Vector3 vecSpawn = new Vector3(spawnLoc.spawnX, spawnLoc.spawnY, spawnLoc.spawnZ);

                                float enginePower = 1.0f;
                                string carName = "washington";

                                if (EPlayer.GetAccount(_tempPlayer).CarFugitive.Length < 3)
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
                                    carName = EPlayer.GetAccount(_tempPlayer).CarFugitive;
                                    if (carName == "sanchez")
                                    {
                                        _tempPlayer.SendChatMessage("!{#FF0000}> The Sanchez is no longer a fugitive vehicle.");
                                        _tempPlayer.SendChatMessage("!{#FF0000}> It has been replaced with the !{#FFFFFF}Felon GT!{#FF0000}, which has been added to your inventory.");
                                        EPlayer.UnlockVehicle(_tempPlayer, "felon2", 0);
                                        if (ContextFactory.Instance.OwnedVehicles.FirstOrDefault<OwnedVehData>(v => v.Model == "sanchez" && v.AccountSQLID == EPlayer.GetAccount(_tempPlayer).SQL_ID) != null)
                                        {
                                            OwnedVehData _d = ContextFactory.Instance.OwnedVehicles.FirstOrDefault<OwnedVehData>(v => v.Model == "sanchez" && v.AccountSQLID == EPlayer.GetAccount(_tempPlayer).SQL_ID);
                                            ContextFactory.Instance.OwnedVehicles.Remove(_d);
                                            ContextFactory.Instance.SaveChanges();
                                        }
                                        carName = "felon2";
                                    }

                                    // incrementing times this police vehicle was used
                                    if (EPlayer.HasOwnedVehicle(_tempPlayer, carName, 0))
                                    {
                                        OwnedVehData ownedVeh = ContextFactory.Instance.OwnedVehicles.ToList().Find(v => v.AccountSQLID == EPlayer.GetAccount(_tempPlayer).SQL_ID
                                                                && v.Model.Equals(carName, StringComparison.OrdinalIgnoreCase));
                                        if (ownedVeh != null)
                                            ownedVeh.TimesUsed++;
                                    }
                                }

                                Vehicle pVehicle = NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(carName), vecSpawn, spawnLoc.spawnA,
                                    new Random(DateTimeOffset.UtcNow.Millisecond).Next(160), new Random(DateTimeOffset.UtcNow.Millisecond).Next(160),
                                    $"SA-{new Random(DateTimeOffset.UtcNow.Millisecond).Next(1000, 9999)}", 255, false, true, Convert.ToUInt32(lobbyid));

                                pVehicle.EngineStatus = true;
                                pVehicle.SetSharedData("Engine", true);
                                pVehicle.SetSharedData("Locked", false);
                                pVehicle.Locked = false;
                                pVehicle.SetSharedData("Team", 0);
                                lobby.Vehicles.Add(pVehicle);
                                pVehicle.SetSharedData("Collisions", false);

                                if (enginePower > 1.0f) pVehicle.SetSharedData("vInitialDriveForce", enginePower);

                                var inst = ContextFactory.Instance.PlayerTunings.ToList().Find(a => a.Model == pVehicle.Model && a.AccountSQLID == EPlayer.GetAccount(_tempPlayer).SQL_ID);
                                if (inst != null)
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
                                }

                                pVehicle.Position = vecSpawn;
                                pVehicle.SetData("SpawnA", spawnLoc.spawnA);

                                _tempPlayer.TriggerEvent("SetChaseVehicle", pVehicle.Id, 0);
                                _tempPlayer.SetData("ChaseVehicle", pVehicle);
                                _tempPlayer.SetData("ChaseVehicleSeat", 0);
                                _tempPlayer.TriggerEvent("AFKDriverCheck", vecSpawn.X, vecSpawn.Y, vecSpawn.Z);

                                int seat = 1;

                                int overridemaxseats = GetMaxSeats(carName); // For DLC cars that aren't registered in vehicledata.

                                int maxseats = pVehicle.MaxOccupants;
                                if (overridemaxseats != 0) maxseats = overridemaxseats;

                                if (lobby.gameMode == GameModes.BUSTED) maxseats = 1; // Separate cars for everyone by not giving available seats.
                                while (seat < maxseats)
                                {
                                    if (fugitives == 0) break;

                                    Player _tempPlayer2 = lobby.Fugitives.OrderBy(c => Guid.NewGuid()).First();

                                    if (PutInsideCar.Contains(_tempPlayer2)) continue;

                                    PutInsideCar.Add(_tempPlayer2);
                                    iteratedAlready.Add(_tempPlayer2);
                                    fugitives--;

                                    _tempPlayer2.TriggerEvent("SetChaseVehicle", pVehicle.Id, 0 + seat);
                                    _tempPlayer2.SetData("ChaseVehicle", pVehicle);
                                    _tempPlayer2.SetData("ChaseVehicleSeat", 0 + seat);

                                    seat++;
                                }
                            }
                        }
                        else
                        {
                            if (lobby.gameMode == GameModes.BANKROBBERY)
                            {
                                BankRobberyData _brob = lobby.BankRobbery;
                                uint dimension = 100 + (uint)lobby.lobbyId;
                                for (int i = 0; i < BankRobberyData.FugitiveVehicleData.Length; i++)
                                {
                                    Vehicle _v = NAPI.Vehicle.CreateVehicle(
                                        NAPI.Util.GetHashKey(BankRobberyData.FugitiveVehicleData[i].Name),
                                        BankRobberyData.FugitiveVehicleData[i].Position,
                                        BankRobberyData.FugitiveVehicleData[i].Heading,
                                        new Random().Next(128), new Random().Next(128), "fartlol", 255, true, true,
                                        Convert.ToUInt32(lobbyid));
                                    lobby.BankRobbery.FugitiveVehicles.Add(_v);
                                    lobby.Vehicles.Add(_v);
                                }
                                Random kevycanusendmefeetpicspls = new Random();
                                crashcheck++;

                                _brob.HeistLeader = null;
                                int unfuck = 0;
                                while (_brob.HeistLeader == null)
                                {
                                    _brob.HeistLeader = lobby.Fugitives[new Random().Next(lobby.Fugitives.Count)];

                                    unfuck++;
                                    if (unfuck > 50)
                                    {
                                        StopGame(lobby.lobbyId, 4);
                                        break;
                                    }
                                }
                                _brob.HeistLeader.SetSharedData("HeistLeader", true);

                                _brob.Sniper = null;
                                unfuck = 0;
                                while (_brob.Sniper == null)
                                {
                                    Player pl = lobby.Fugitives[new Random().Next(lobby.Fugitives.Count)];
                                    //if (pl != _brob.HeistLeader)
                                        _brob.Sniper = lobby.Fugitives[new Random().Next(lobby.Fugitives.Count)];
                                   // else _brob.Sniper = null; // just in case idk

                                    unfuck++;
                                    if (unfuck > 50)
                                    {
                                        StopGame(lobby.lobbyId, 4);
                                        break;
                                    }
                                }
                                crashcheck++;

                                int[] rndmasks = { 35, 37, 51, 52, 54 };
                                int maskid = 0;
                                foreach (Player player in lobby.Fugitives.OrderBy(x => Guid.NewGuid()))
                                {
                                    Account _a = EPlayer.GetAccount(player);
                                    CharacterData cd = NAPI.Util.FromJson<CharacterData>(_a.Character);


                                    if (player != _brob.HeistLeader && player != _brob.Sniper)
                                    {
                                        maskid = new Random().Next(rndmasks.Length);
                                        int spid = kevycanusendmefeetpicspls.Next(BankRobberyData.FugitiveSpawns.GetLength(0));
                                        player.Position = (Vector3)BankRobberyData.FugitiveSpawns[spid, 0];
                                        player.Heading = (float)BankRobberyData.FugitiveSpawns[spid, 1];
                                        player.Health = 100;
                                        player.Armor = 100;

                                        player.GiveWeapon(WeaponHash.Pistol, 100);
                                        player.GiveWeapon(WeaponHash.Pumpshotgun, 50);
                                        player.GiveWeapon(WeaponHash.Microsmg, 200);
                                        player.GiveWeapon(WeaponHash.Assaultrifle, 300);
                                    }
                                    else
                                    {
                                        if (player == _brob.HeistLeader)
                                        {
                                            maskid = 2;
                                            player.Position = BankRobberyData.LeaderPos;
                                            player.Heading = BankRobberyData.LeaderHeading;
                                            player.Health = 100;
                                            player.Armor = 150;

                                            player.GiveWeapon(WeaponHash.Pistol50, 100);
                                            player.GiveWeapon(WeaponHash.Heavyshotgun, 50);
                                            player.GiveWeapon(WeaponHash.Smg, 250);
                                            player.GiveWeapon(WeaponHash.Specialcarbine, 250);
                                            player.GiveWeapon(WeaponHash.Molotov, 1);
                                        }
                                        else if (player == _brob.Sniper)
                                        {
                                            maskid = new Random().Next(rndmasks.Length);
                                            player.Position = BankRobberyData.SniperPos;
                                            player.Heading = BankRobberyData.SniperHeading;
                                            player.Health = 100;
                                            player.Armor = 100;

                                            player.GiveWeapon(WeaponHash.Snspistol, 100);
                                            player.GiveWeapon(WeaponHash.Sawnoffshotgun, 50);
                                            player.GiveWeapon(WeaponHash.Assaultrifle, 90);
                                            player.GiveWeapon(WeaponHash.Marksmanrifle, 10);
                                            player.GiveWeapon(WeaponHash.Molotov, 1);
                                        }
                                    }
                                    cd.Clothing[0].Clothes[1].Drawable = rndmasks[maskid];
                                    cd.Clothing[0].Clothes[1].Texture = 0;
                                    cd.Clothing[2].Clothes[1].Drawable = rndmasks[maskid];
                                    cd.Clothing[2].Clothes[1].Texture = 0;

                                    EPlayer._ApplyCharacter(player, cd, 0);

                                    EPlayer.Freeze(player, true);

                                    EPlayer.SetColor(player, new Color(255, 174, 0), 0);

                                    EPlayer.SetDiscordStatus(player, $"[Lobby {lobbyid}] Fugitive as {player.Name}");
                                    EPlayer.Notify(player, "<strong>Please wait</strong>, Bank Robbery is loading...", "yellow", "fas fa-spinner", false, 15000);

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
                                crashcheck++;
                            }
                        }

                        if (lobby.gameMode == GameModes.STANDARD)
                        {
                            Discord.SendTextChannel("staff-bot", $"**(Lobby {lobbyid}) Standard Copchase has been started! Cops: {lobby.Cops.Count} | Fugitives: {lobby.Fugitives.Count}**");
                        }
                        else if (lobby.gameMode == GameModes.BUSTED)
                        {
                            Discord.SendTextChannel("staff-bot", $"**(Lobby {lobbyid}) Copchase in BUSTED! mode has been started! Cops: {lobby.Cops.Count} | Fugitives: {lobby.Fugitives.Count}**");
                        }
                        else if (lobby.gameMode == GameModes.BANKROBBERY)
                        {
                            Discord.SendTextChannel("staff-bot", $"**(Lobby {lobbyid}) Copchase in Bank Robbery event has been started! Cops: {lobby.Cops.Count} | Fugitives: {lobby.Fugitives.Count}**");
                            lobby.BankRobbery.State = 0;
                        }

                        lobby.FugitiveShowCounter = 47;
                        lobby.LastSeenCounter = -13;
                        lobby.GameProgress = -13;
                        lobby.CollisionCounter = 48;

                        foreach (Player p in Settings.LoggedInPlayers)
                        {
                            if (p.GetSharedData<int>("InLobby") == lobbyid)
                            {
                                p.TriggerEvent("StartChaseTimer", -13, Settings.GameRunTime, lobby.CollisionCounter, (short)lobby.gameMode);
                            }
                        }


                        lobby.gameStatus = 1;
                        NAPI.Util.ConsoleOutput($"[Lobby {lobbyid}] Mode {ConvertGameMode(lobby.gameMode)} has been started with {lobby.Cops.Count} cops & {lobby.Fugitives.Count} fugitives on map {(map == null ? "EVENT" : map.Name)}.");
                    }
                    catch (Exception ex)
                    {
                        NAPI.Util.ConsoleOutput("Exception in startgame, ctx: " + crashcheck);
                        NAPI.Util.ConsoleOutput(ex.Message);
                        return false;
                    }
                }
                else return false;

                return true;
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput("[EXCEPTION IN StartGame]: " + ex.ToString());
                return true;
            }
        }

        public static void GameUpdate()
        {
            try
            {

                if (Environment.TickCount64 - Settings.LastDerbyTick >= 1000 && Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == -4).Count() >= 2)
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

                            if (lobby.GameProgress == -10)
                            {
                                MapSpawn fugi = ContextFactory.Instance.MapSpawns.ToList().Find(a => a.mapID == lobby.lastMapId && a.spawnTeam == 0);
                                MapSpawn cop = ContextFactory.Instance.MapSpawns.ToList().Find(a => a.mapID == lobby.lastMapId && a.spawnTeam == 1);

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

                                                p.TriggerEvent("ChangeVehicleHeading", p.GetData<Vehicle>("ChaseVehicle").GetData<float>("SpawnA"));
                                                EPlayer.Freeze(p, true);

                                                p.GetData<Vehicle>("ChaseVehicle").SetSharedData("Collisions", false);
                                                p.TriggerEvent("ToggleCollisionsForEnt", p.GetData<Vehicle>("ChaseVehicle"), false);

                                                p.SetSharedData("Collisions", false);
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

                                                p.TriggerEvent("ChangeVehicleHeading", p.GetData<Vehicle>("ChaseVehicle").GetData<float>("SpawnA"));
                                                EPlayer.Freeze(p, true);

                                                p.GetData<Vehicle>("ChaseVehicle").SetSharedData("Collisions", false);
                                                p.SetSharedData("Collisions", false);
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

                                                p.TriggerEvent("ChangeVehicleHeading", p.GetData<Vehicle>("ChaseVehicle").GetData<float>("SpawnA"));

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
                                                    var inst = ContextFactory.Instance.PlayerTunings.ToList().Find(a => a.Model == p.GetData<Vehicle>("ChaseVehicle").Model && a.AccountSQLID == EPlayer.GetAccount(p).SQL_ID);
                                                    if (inst != null)
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
                                                    }
                                                }

                                                EPlayer.Freeze(p, false);
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
                                                p.SetIntoVehicle(p.GetData<Vehicle>("ChaseVehicle"), p.GetData<int>("ChaseVehicleSeat"));
                                                p.TriggerEvent("ToggleNametags", true);

                                                p.TriggerEvent("ChangeVehicleHeading", p.GetData<Vehicle>("ChaseVehicle").GetData<float>("SpawnA"));

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
                                                    var inst = ContextFactory.Instance.PlayerTunings.ToList().Find(a => a.Model == p.GetData<Vehicle>("ChaseVehicle").Model && a.AccountSQLID == EPlayer.GetAccount(p).SQL_ID);
                                                    if (inst != null)
                                                    {
                                                        p.GetData<Vehicle>("ChaseVehicle").PrimaryColor = inst.PrimaryColor;
                                                        p.GetData<Vehicle>("ChaseVehicle").SecondaryColor = inst.SecondaryColor;

                                                        foreach (var kvp in NAPI.Util.FromJson<Dictionary<int, int>>(inst.Mods))
                                                        {
                                                            p.GetData<Vehicle>("ChaseVehicle").SetMod(kvp.Key, kvp.Value);
                                                            if (kvp.Key == 56) p.GetData<Vehicle>("ChaseVVehicle").WheelType = kvp.Value;
                                                        }
                                                        p.GetData<Vehicle>("ChaseVehicle").PearlescentColor = inst.PearlescentColor;
                                                        p.GetData<Vehicle>("ChaseVehicle").WheelColor = inst.WheelColor;
                                                    }
                                                }
                                                EPlayer.Freeze(p, true);
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
                                    int currentCar = 0;
                                    int lobbyid = lobby.lobbyId;

                                    foreach (Player player in lobby.Cops.OrderBy(x => Guid.NewGuid()))
                                    {
                                        player.Dimension = Convert.ToUInt32(lobbyid);
                                        player.Position = BankRobberyData.FbiSuvPositions[0];

                                        CharacterData ConvChar = new CharacterData();

                                        try
                                        {
                                            ConvChar = NAPI.Util.FromJson<CharacterData>(EPlayer.GetAccount(player).Character);

                                            EPlayer.ApplyCharacter(player, ConvChar, 1);
                                        }
                                        catch { }
                                        if (ccars[currentCar].Occupants.Count < ccars[currentCar].MaxOccupants)
                                        {
                                            int seat = ccars[currentCar].Occupants.Count;
                                            NAPI.Task.Run(() =>
                                            {
                                                player.SetIntoVehicle(ccars[currentCar], seat);
                                            }, 100);
                                        }
                                        else
                                        {
                                            currentCar++;
                                            if (currentCar >= ccars.Count)
                                            {
                                                Vehicle dick = NAPI.Vehicle.CreateVehicle((uint)VehicleHash.Police, ccars[currentCar - 1].Position, ccars[currentCar - 1].Heading, 0, 0);
                                                player.SetIntoVehicle(dick, 0);
                                            }
                                            else
                                            {
                                                int seat = ccars[currentCar].Occupants.Count; 
                                                NAPI.Task.Run(() =>
                                                {
                                                    player.SetIntoVehicle(ccars[currentCar], seat);
                                                }, 100);
                                            }
                                        }
                                        if (currentCar >= 6) // Special/SWAT/FIB unit 
                                        {
                                            player.SetData<string>("BankRobberyUnit", "fib");
                                            player.Health = 100;
                                            player.Armor = 150;

                                            player.GiveWeapon(WeaponHash.Heavypistol, 100);
                                            player.GiveWeapon(WeaponHash.Assaultsmg, 200);
                                            player.GiveWeapon(WeaponHash.Specialcarbine, 500);
                                            player.GiveWeapon(WeaponHash.Bzgas, 3);
                                            player.GiveWeapon(WeaponHash.Assaultshotgun, 500);
                                            player.SendChatMessage("~b~[>] ~w~You've been assigned to the ~y~FIB~w~ unit.");
                                        }
                                        else // patrol unit
                                        {
                                            player.SetData<string>("BankRobberyUnit", "patrol");
                                            player.Health = 100;
                                            player.Armor = 100;

                                            player.GiveWeapon(WeaponHash.Combatpistol, 100);
                                            player.GiveWeapon(WeaponHash.Smg, 300);
                                            player.GiveWeapon(WeaponHash.Carbinerifle, 500);
                                            player.GiveWeapon(WeaponHash.Bzgas, 1);
                                            player.GiveWeapon(WeaponHash.Pumpshotgun, 72);
                                            player.SendChatMessage("~b~[>] ~w~You've been assigned to the ~y~PATROL~w~ unit.");
                                        }

                                        EPlayer.Freeze(player, true);

                                        player.SetData("GPS", false);

                                        EPlayer.SetDiscordStatus(player, $"[Lobby {lobby.lobbyId}] Cop as {player.Name}");

                                        EPlayer.SetColor(player, new Color(0, 151, 255), 0);
                                        player.TriggerEvent("SetActorInvulnerable", false);
                                        player.TriggerEvent("ToggleAFOB", Settings.Dynamics.AntiFallOffBike, Settings.Dynamics.AntiFallOffBikeSpeedLimit);
                                        player.SetSharedData("CuffedBy", "n/a");
                                        player.SetSharedData("Team", 1);
                                    }

                                    int[] rndmasks = { 35, 37, 51, 52, 54 };
                                    int maskid = 0;
                                    BankRobberyData _brob = lobby.BankRobbery;
                                    Random kevycanusendmefeetpicspls = new Random();
                                    foreach (Player player in lobby.Fugitives.OrderBy(x => Guid.NewGuid()))
                                    {
                                        Account _a = EPlayer.GetAccount(player);
                                        CharacterData cd = NAPI.Util.FromJson<CharacterData>(_a.Character);


                                        if (player != _brob.HeistLeader && player != _brob.Sniper)
                                        {
                                            maskid = new Random().Next(rndmasks.Length);
                                            int spid = kevycanusendmefeetpicspls.Next(BankRobberyData.FugitiveSpawns.GetLength(0));
                                            player.Position = (Vector3)BankRobberyData.FugitiveSpawns[spid, 0];
                                            player.Heading = (float)BankRobberyData.FugitiveSpawns[spid, 1];
                                            player.Health = 100;
                                            player.Armor = 100;

                                            player.GiveWeapon(WeaponHash.Pistol, 100);
                                            player.GiveWeapon(WeaponHash.Pumpshotgun, 50);
                                            player.GiveWeapon(WeaponHash.Microsmg, 200);
                                            player.GiveWeapon(WeaponHash.Assaultrifle, 300);
                                        }
                                        else
                                        {
                                            if (player == _brob.HeistLeader)
                                            {
                                                maskid = 2;
                                                player.Position = BankRobberyData.LeaderPos;
                                                player.Heading = BankRobberyData.LeaderHeading;
                                                player.Health = 100;
                                                player.Armor = 150;

                                                player.GiveWeapon(WeaponHash.Pistol50, 100);
                                                player.GiveWeapon(WeaponHash.Heavyshotgun, 50);
                                                player.GiveWeapon(WeaponHash.Smg, 250);
                                                player.GiveWeapon(WeaponHash.Specialcarbine, 250);
                                                player.GiveWeapon(WeaponHash.Molotov, 1);
                                            }
                                            else if (player == _brob.Sniper)
                                            {
                                                maskid = new Random().Next(rndmasks.Length);
                                                player.Position = BankRobberyData.SniperPos;
                                                player.Heading = BankRobberyData.SniperHeading;
                                                player.Health = 100;
                                                player.Armor = 100;

                                                player.GiveWeapon(WeaponHash.Snspistol, 100);
                                                player.GiveWeapon(WeaponHash.Sawnoffshotgun, 50);
                                                player.GiveWeapon(WeaponHash.Assaultrifle, 90);
                                                player.GiveWeapon(WeaponHash.Marksmanrifle, 10);
                                                player.GiveWeapon(WeaponHash.Molotov, 1);
                                            }
                                        }
                                        cd.Clothing[0].Clothes[1].Drawable = rndmasks[maskid];
                                        cd.Clothing[0].Clothes[1].Texture = 0;
                                        cd.Clothing[2].Clothes[1].Drawable = rndmasks[maskid];
                                        cd.Clothing[2].Clothes[1].Texture = 0;

                                        EPlayer._ApplyCharacter(player, cd, 0);

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

                                if (lobby.gameMode == GameModes.STANDARD)
                                {
                                    NAPI.Chat.SendChatMessageToAll($"[Lobby {lobby.lobbyId}] Copchase started with !{{#8D8DFF}}{lobby.Cops.Count} cops !{{#FFFFFF}}& !{{#E84040}}{lobby.Fugitives.Count} fugitives !{{#FFFFFF}}on map !{{#DAA520}}{ContextFactory.Instance.Maps.ToList().Find(a => a.SQL_ID == lobby.lastMapId).Name}!{{#FFFFFF}}.");
                                }
                                else if (lobby.gameMode == GameModes.BUSTED)
                                {
                                    NAPI.Chat.SendChatMessageToAll($"[Lobby {lobby.lobbyId}] Mode BUSTED! started with !{{#8D8DFF}}{lobby.Cops.Count} cops !{{#FFFFFF}}& !{{#E84040}}{lobby.Fugitives.Count} fugitives !{{#FFFFFF}}on map !{{#DAA520}}{ContextFactory.Instance.Maps.ToList().Find(a => a.SQL_ID == lobby.lastMapId).Name}!{{#FFFFFF}}.");
                                }
                                else if (lobby.gameMode == GameModes.BANKROBBERY)
                                {
                                    NAPI.Chat.SendChatMessageToAll($"[Lobby {lobby.lobbyId}] Mode Bank Robbery started with !{{#8D8DFF}}{lobby.Cops.Count} cops !{{#FFFFFF}}& !{{#E84040}}{lobby.Fugitives.Count} fugitives !{{#FFFFFF}}on map !{{#DAA520}}Pacific Standard Bank!{{#FFFFFF}}.");
                                }


                                foreach (Player p in Settings.LoggedInPlayers.Where(p => lobby.Cops.Contains(p) || lobby.Fugitives.Contains(p)))
                                {
                                    lock (p)
                                    {
                                        try
                                        {
                                            if (p == null) continue;

                                            p.TriggerEvent("ResetKillFeed");

                                            if (lobby.Cops.Contains(p)) EPlayer.Notify(p, "You will be unfreezed in 10 seconds.", "orange", "fas fa-hourglass-start", false, 10000);
                                            if (lobby.gameMode == GameModes.STANDARD)
                                            {
                                                p.SendChatMessage("!{#FF6347}[!] !{#8D8DFF}Cops will be unfrozen in 10 seconds. !{#E84040}Fugitive's may take this time to prepare/start the chase.");
                                            }
                                            else if (lobby.gameMode == GameModes.BUSTED)
                                            {
                                                p.SendChatMessage($"!{{#1E9913}}[!] ATTENTION: The lobby you're playing is in BUSTED! mode. This is not a standard chase.");
                                                p.SendChatMessage($"!{{#1E9913}}[#] !{{#FFFFFF}}In this mode, you do not receive weapons nor fight, rather you EVADE and win by skills!");
                                                p.SendChatMessage($"!{{#1E9913}}[#] !{{#FFFFFF}}Cops have to box/stick close to fugitive's to start the busting process.");
                                                p.SendChatMessage($"!{{#1E9913}}[#] !{{#FFFFFF}}More cops would makes the process a lot faster and the closer you get.");
                                                p.SendChatMessage($"!{{#1E9913}}[#] !{{#FFFFFF}}Fugitive's have to get away to regress the busting progress.");
                                            }
                                        }
                                        catch { }
                                    }
                                }
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

                                                if (lobby.Cops.Contains(p)) EPlayer.Notify(p, "You will be unfreezed in 10 seconds.", "orange", "fas fa-hourglass-start", false, 10000);

                                                p.SendChatMessage($"!{{#1E9913}}[!] ATTENTION: You are currently in the Bank Robbery event! This is not a standard chase.");
                                                p.SendChatMessage($"!{{#1E9913}}[#] !{{#FFFFFF}}In this mode, you must prevent the fugitives from delivering the money to their hideout.");
                                                p.SendChatMessage($"!{{#1E9913}}[#] !{{#FFFFFF}}You are allowed to shoot fugitives on sight without them firing first.");
                                                p.SendChatMessage($"!{{#1E9913}}[#] !{{#FFFFFF}}The fugitives are currently robbing the ~y~Pacific Standard~w~ bank.");
                                                p.SendChatMessage($"!{{#1E9913}}[#] !{{#FFFFFF}}Neutralize all the fugitives, or keep slowing them down until the round ends to win!");

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

                                                p.SendChatMessage($"!{{#1E9913}}[!] ATTENTION: You are currently in the Bank Robbery event! This is not a standard chase.");
                                                if (p == _brob.HeistLeader)
                                                {
                                                    if (lobby.Cops.Contains(p)) EPlayer.Notify(p, "You've been selected as the <strong>heist leader</strong>!", "orange", "fas fa-hourglass-start", false, 10000);
                                                    p.SendChatMessage($"~y~!! YOU'VE BEEN SELECTED AS THE HEIST LEADER !!");
                                                    p.SendChatMessage($"!{{#1E9913}}[#] !{{#FFFFFF}}In this mode, you must rob the bank and deliver the money to the hideout.");
                                                    p.SendChatMessage($"!{{#1E9913}}[#] !{{#FFFFFF}}You are currently in the vault. Stay here for ~y~one minute ~w~to steal all the money.");
                                                    p.SendChatMessage($"!{{#1E9913}}[#] !{{#FFFFFF}}After that, do anything you can in order to get to the ~y~hideout~w~ with your team.");
                                                    p.SendChatMessage($"!{{#1E9913}}[#] !{{#FFFFFF}}Watch out! Your death will significantly decrease your chances of winning the round.");
                                                }
                                                else if (p == _brob.Sniper)
                                                {
                                                    p.SendChatMessage($"~y~!! YOU'VE BEEN SELECTED AS THE SNIPER !!");
                                                    p.SendChatMessage($"!{{#1E9913}}[#] !{{#FFFFFF}}In this mode, you must rob help your heist leader, ~y~{_brob.HeistLeader.Name}~w~, deliver the money to the hideout..");
                                                    p.SendChatMessage($"!{{#1E9913}}[#] !{{#FFFFFF}}Due to your excellent marksmanship skills, you were picked to be the \"sniper\".");
                                                    p.SendChatMessage($"!{{#1E9913}}[#] !{{#FFFFFF}}You received a special set of weapons, including a sniper. Use it wisely to fend off against the police.");
                                                    p.SendChatMessage($"!{{#1E9913}}[#] !{{#FFFFFF}}You can use the ladders to climb up to a higher spot, or back down onto the street - it's entirely your choice.");
                                                }
                                                else
                                                {
                                                    p.SendChatMessage($"!{{#1E9913}}[#] !{{#FFFFFF}}In this mode, you must help your heist leader, ~y~{_brob.HeistLeader.Name}~w~, deliver the money to the hideout.");
                                                    p.SendChatMessage($"!{{#1E9913}}[#] !{{#FFFFFF}}You are currently in the bank. The heist leader is in the vault, taking the money.");
                                                    p.SendChatMessage($"!{{#1E9913}}[#] !{{#FFFFFF}}After ~y~one minute~w~, you may head with the heist leader to the ~y~hideout~w~.");
                                                    p.SendChatMessage($"!{{#1E9913}}[#] !{{#FFFFFF}}Make sure you defend the heist leader! Leader's death significantly decreases your chances of winning.");
                                                }
                                                p.SendChatMessage($"!{{#1E9913}}[#] ~y~Work as a TEAM! ~w~You have one minute to plan out your getaway.");

                                            }
                                            catch { }
                                        }
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
                                            EPlayer.Freeze(p, false);
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
                                        }
                                    }
                                    foreach (Player p in lobby.Fugitives)
                                    {
                                        if (p != null)
                                        {
                                            p.SendNotification("~y~You can now exit vehicles.");
                                            p.TriggerEvent("ToggleCanExitVehicle", true);
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
                                                EPlayer.Freeze(p, false);
                                                p.SendNotification("~y~You can now exit vehicles.");
                                                p.SendNotification("~b~Head to the bank! It has been marked on your GPS.");
                                                p.TriggerEvent("ToggleCanExitVehicle", true);
                                                p.TriggerEvent("playerCreateWaypoint", BankRobberyData.BankCheckpointPos);

                                                p.SendChatMessage($"!{{#FFEC8B}}** [RADIO | FREQ: 911] DISPATCH: All available units, code 3, there's a 211 in progress in the Pacific Standard Bank!");
                                            }
                                        }
                                        foreach (Player p in lobby.Fugitives)
                                        {
                                            if (p != null)
                                            {
                                                EPlayer.Freeze(p, false);
                                                p.SendNotification("~y~The event has started!");
                                                p.TriggerEvent("ToggleCanExitVehicle", true);

                                                if (p == lobby.BankRobbery.HeistLeader)
                                                {
                                                    p.TriggerEvent("StartBankRobberyTimer");
                                                    p.SendChatMessage("!{#FF6347}[#] ~w~You must stay in the vault for 60 seconds to proceed - timer has started!");
                                                }
                                                else
                                                {
                                                    p.SendChatMessage("!{#FF6347}[#] ~w~Law enforcement is on their way, protect the heist leader!");
                                                }

                                                p.SendChatMessage($"!{{#FFEC8B}}** [RADIO | FREQ: 69420] SCOUT: Alright ladies and gentlemen, I hear that the cops are on their way. Good luck!");
                                            }
                                        }
                                    }
                                }
                            }

                            if (lobby.GameProgress >= Convert.ToInt32(0.7 * Settings.GameRunTime) && lobby.gameMode != GameModes.BUSTED && lobby.gameMode != GameModes.BANKROBBERY)
                            {
                                foreach (Player fugitive in lobby.Fugitives)
                                {
                                    try
                                    {
                                        if (fugitive == null) continue;

                                        if (fugitive.GetSharedData<bool>("HasShot") == false)
                                        {
                                            fugitive.SetSharedData("HasShot", true);
                                            fugitive.SetSharedData("VisibleBlip", true);
                                            lobby.FugitiveShowCounter = 29; // Set them to VISIBLE

                                            fugitive.SendChatMessage("!{#FF6347}You can be shot down by cops as the game has progressed to 70%!");
                                        }
                                    }
                                    catch { }
                                }

                                if (lobby.GameProgress == Convert.ToInt32(0.7 * Settings.GameRunTime) && lobby.gameMode != GameModes.BUSTED && lobby.gameMode != GameModes.BANKROBBERY)
                                {
                                    foreach (Player pl in Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == lobby.lobbyId))
                                    {
                                        try
                                        {
                                            if (pl == null) continue;

                                            pl.SendChatMessage("!{#FF6347}[GAME] !{#FFFFFF}70% of the game has elapsed, all fugitives can be shot down now.");
                                            pl.TriggerEvent("StartChaseTimer", lobby.GameProgress, Settings.GameRunTime, -1, (short)lobby.gameMode);
                                        }
                                        catch { }
                                    }
                                }
                            }
                        }

                        if (lobby.GameProgress >= Settings.GameRunTime)
                        {
                            StopGame(lobby.lobbyId, 1);
                        }

                        lobby.FugitiveShowCounter++;
                        lobby.LastSeenCounter++;

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
                                        }
                                        catch { }
                                    }
                                }
                                catch { }
                            }
                            lobby.LastSeenCounter = 0;
                        }

                        if (lobby.GameProgress % 5 == 0) // Every 5 seconds/multiple during collision phase
                        {
                            foreach (Player player in Settings.LoggedInPlayers.Where(p => p.GetSharedData<int>("InLobby") == lobby.lobbyId))
                            {
                                try
                                {
                                    if (player == null) continue;

                                    player.TriggerEvent("StartChaseTimer", lobby.GameProgress, Settings.GameRunTime, lobby.CollisionCounter, (short)lobby.gameMode);
                                }
                                catch { }
                            }
                        }

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
                                    foreach (Player player in Settings.LoggedInPlayers.Where(p => p.GetSharedData<int>("InLobby") == lobby.lobbyId))
                                    {
                                        try
                                        {
                                            if (player == null) continue;

                                            player.TriggerEvent("StartChaseTimer", lobby.GameProgress, Settings.GameRunTime, 0, (short)lobby.gameMode);

                                            player.SetSharedData("BustedProgress", 0.0);

                                            //////////////////////////////////////////////////////////////////////////////////////////
                                            ///

                                            if (!lobby.Cops.Contains(player) && !lobby.Fugitives.Contains(player)) continue;

                                            EPlayer.Notify(player, "Collisions have been enabled and weapons issued", "green", "fas fa-car-crash", false, 3000);

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
                                                EPlayer.GiveFugitiveWeapons(player);
                                                player.TriggerEvent("SetActorInvulnerable", false);
                                            }
                                        }
                                        catch { }
                                    }

                                    foreach (Vehicle veh in lobby.Vehicles)
                                    {
                                        veh.SetSharedData("Collisions", true);
                                        if (veh.HasSharedData("Team") && veh.GetSharedData<int>("Team") == 0)
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

                                    if (cop.GetSharedData<bool>("ClimbProtection") == true && cop.GetData<long>("ClimbProtectionEnd") < Environment.TickCount64)
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

                                    if (fugi.GetSharedData<bool>("ClimbProtection") == true && fugi.GetData<long>("ClimbProtectionEnd") < Environment.TickCount64)
                                    {
                                        fugi.SetSharedData("ClimbProtection", false);
                                    }
                                }
                                catch { }
                            }
                        }

                        if (lobby.FugitiveShowCounter >= 60 && lobby.FugitiveShowCounter < 120)
                        {
                            lock (lobby.Fugitives)
                            {
                                foreach (Player fugi in lobby.Fugitives)
                                {
                                    fugi.SetSharedData("VisibleBlip", true);

                                    if (lobby.FugitiveShowCounter == 60) EPlayer.Notify(fugi, "You are now visible on the radar.", "red", "fas fa-running", false, 3000);
                                }

                                if (lobby.FugitiveShowCounter == 60 && lobby.GameProgress >= 60 && lobby.CollisionCounter <= -1)
                                {
                                    foreach (Player p in Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == lobby.lobbyId))
                                    {
                                        p.TriggerEvent("StartChaseTimer", lobby.GameProgress, Settings.GameRunTime, -1, (short)lobby.gameMode);
                                    }
                                }
                            }
                        }

                        if (lobby.FugitiveShowCounter >= 120)
                        {
                            lobby.FugitiveShowCounter = 0;

                            lock (lobby.Fugitives)
                            {
                                foreach (Player fugi in lobby.Fugitives.Where(a => a.GetSharedData<bool>("HasShot") == false))
                                {
                                    fugi.SetSharedData("VisibleBlip", false);

                                    EPlayer.Notify(fugi, "You are no longer visible on the radar", "green", "fas fa-walking", false, 3000);
                                }
                            }
                        }
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