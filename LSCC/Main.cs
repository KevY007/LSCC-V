using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Resources;
using GTANetworkAPI;
using System.Linq;
using LSCC;
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

namespace LSCC
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
        public Dictionary<ulong, Component> Components { get; set; }

        [JsonProperty("Tints")]
        public List<LiveryColor> Tints { get; set; }

        [JsonProperty("LiveryColors")]
        public List<LiveryColor> LiveryColors { get; set; }

        [JsonProperty("DLC")]
        public string Dlc { get; set; }

        public static Dictionary<ulong, jWeaponData> FromJson(string json) => JsonConvert.DeserializeObject<Dictionary<ulong, jWeaponData>>(json, new JsonSerializerSettings
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
    }

    public class LobbyData
    {
        public List<Player> Cops { get; set; }
        public List<Player> Fugitives { get; set; }
        public List<Vehicle> Vehicles { get; set; }

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

        public LobbyData()
        {
            Cops = new List<Player>();
            Fugitives = new List<Player>();
            Vehicles = new List<Vehicle>();

            Time = new int[3];
            Time[0] = new Random().Next(0, 23);
            Time[1] = new Random().Next(0, 59);
            Time[2] = new Random().Next(0, 59);

            Weather = new Random().Next(0, 9);

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
        public int LevelRequired { get; set; }
        public List<Tuple<WeaponHash, int>> Weapons { get; set; }

        public LoadoutData(int team, string name, int level, List<Tuple<WeaponHash, int>> weps)
        {
            this.Team = team;
            this.Name = name;
            this.LevelRequired = level;

            this.Weapons = new List<Tuple<WeaponHash, int>>();
            this.Weapons.AddRange(weps);

            Settings.Loadouts.Add(this);
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

        [JsonProperty("WeaponDataURL")]
        public string WeaponDataURL { get; set; }

        [JsonProperty("MaxInLobby")]
        public int MaxInLobby { get; set; }

        [JsonProperty("Discord")]
        public bool Discord { get; set; }

        [JsonProperty("Discord_ServerID")]
        public ulong DCServerID { get; set; }

        [JsonProperty("Discord_NumPlayersID")]
        public ulong DCCountID { get; set; }

        [JsonProperty("Discord_NumGamesID")]
        public ulong DCGamesID { get; set; }

        [JsonProperty("Discord_MentionID")]
        public ulong DCMentionRoleID { get; set; }

        [JsonProperty("Discord_MentionChanID")]
        public ulong DCMentionRoleChan { get; set; }

        public DynSet()
        {
            this.WeaponDataURL = "https://gist.githubusercontent.com/root-cause/3f29d38179b12245a003fb4fff615335/raw/7a8804b275fb1b9314cb39418a7415ea8bfd148d/v2189%2520Weapon%2520Data.json";
            this.DerbyVehicle = "brioso";
            this.OnfootCollision = true;
            this.Discord = true;
            this.MaxInLobby = Settings.DefaultMaxInLobby;
            this.DCServerID = 713731146072064040;
            this.DCCountID = 717405966123139104;
            this.DCGamesID = 849065885562044476;
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

    public static class Settings
    {
        // Static Settings / Const
        public const string Version = "0.2h";
        public const int GameRunTime = 390; // Game Running Time in Seconds before STOP
        public const int MaxLobbies = 5; // Default/starting lobby num.
        public const int DefaultMaxInLobby = 30;

        // Connection Settings
        public const string MySQL_Host = "localhost";
        public const string MySQL_User = "lsccv";
        public const string MySQL_Password = "perkovicMatko3035";
        public const string MySQL_Database = "lscc_ragemp";
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
        public static List<LoadoutData> Loadouts { get; set; }
        public static Dictionary<ulong, jWeaponData> WeaponData { get; set; }
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
        public const float LoginCameraX = -550;
        public const float LoginCameraY = -210;
        public const float LoginCameraZ = 1200;
        public const float LoginCameraRx = -50;
        public const float LoginCameraRy = 0;
        public const float LoginCameraRz = 0;
        public const float LoginCameraViewAngle = 50;

        public static Vector3[] LobbySpawns = new Vector3[]
        {
            new Vector3(464.8465f, -995.0156f, 24.914879f),
            new Vector3(450.13f, -987.06f, 26.67f),
            new Vector3(439.86f, -995.12f, 30.68f),
            new Vector3(443.308f, -986.31f, 30.689f),
            new Vector3(441.24f, -978.24f, 30.68f)
        };

        public static object[] VectorJail = { new Vector3(459.55798f, -998.202f, 24.914848f), -75.06f };

        public static string[] DMNames = new string[]
        {
            "Life Invader Office",
            "Warehouse (Pistol Only)",
            "Tennis Court",
            "Construction Site",
            "Mile High Club"
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
            new object[,] { // Mile High Club
                { new Vector3(-189.9890f, -1006.218f, 257.2304f), 249.8210f },
                { new Vector3(-152.8178f, -1022.546f, 254.3519f), 19.5289f },
                { new Vector3(-145.1180f, -988.6318f, 254.1316f), 105.5798f},
                { new Vector3(-163.8666f, -963.8352f, 254.1313f), 153.9387f},
                { new Vector3(-165.9847f, -977.5870f, 269.2273f), 323.5029f},
                { new Vector3(-139.0145f, -953.3565f, 269.1350f), 164.8710f},
                { new Vector3(-159.3349f, -944.4025f, 269.2180f), 251.5890f},
                { new Vector3(-153.8990f, -947.4684f, 264.2260f), 241.3984f},
                { new Vector3(-160.5658f, -969.7756f, 264.2260f), 247.6226f},
                { new Vector3(-153.6451f, -947.7322f, 254.1314f), 229.3157f},
                { new Vector3(-139.3184f, -955.3798f, 254.1315f), 327.5726f},
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
                if ((drawable >= 16 && drawable <= 18) || drawable == 39 || (drawable >= 48 && drawable <= 53) || 
                    drawable == 67 || (drawable >= 69 && drawable <= 75) || (drawable >= 78 && drawable <= 82) || 
                    (drawable >= 84 && drawable <= 94) || drawable == 111 || (drawable >= 115 && drawable <= 119) || 
                    (drawable >= 125 && drawable <= 128) || drawable == 144 || drawable == 150) return true; // (Blocks: Unfair headshot blocking helmets)
            }
            return false;
        }
        public static int GetMaxSeats(string vehiclename)
        {
            int seats = NAPI.Vehicle.GetVehicleMaxOccupants((VehicleHash)NAPI.Util.GetHashKey(vehiclename));

            switch(vehiclename)
            {
                case "cheburek":
                    seats = 4;
                    break;
                case "blista2":
                    seats = 2;
                    break;
                case "polbufac2":
                    seats = 4;
                    break;
                case "polbufac":
                    seats = 4;
                    break;
                case "fbibufac":
                    seats = 4;
                    break;
                case "600sel":
                    seats = 4;
                    break;
                case "udchall1":
                    seats = 4;
                    break;
                case "lspdcara":
                    seats = 4;
                    break;
                case "prevolter":
                    seats = 4;
                    break;
                case "m3e46":
                    seats = 2;
                    break;
                case "66fastback":
                    seats = 2;
                    break;
            }
            return seats;
        }
        public static IDictionary<ulong, string> WeaponReasons = new Dictionary<ulong, string>()
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
            }
            return ret;
        }

        public static void Log(string fileName, string msg)
        {
            var writer = new System.IO.StreamWriter($"lscc/logs/{fileName}", true);

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
                if (File.Exists("lscc/lscc.json")) File.Delete("lscc/lscc.json");

                File.WriteAllText("lscc/lscc.json", NAPI.Util.ToJson(Settings.Dynamics));
            }
            catch { NAPI.Util.ConsoleOutput("(LSCC | SETTINGS) Failed to save dynamic settings!"); }

            return;
        }

        public static void LoadDynamicSettings()
        {
            try
            {
                if (File.Exists("lscc/lscc.json"))
                {
                    Settings.Dynamics = new DynSet();
                    Settings.Dynamics = NAPI.Util.FromJson<DynSet>(File.ReadAllText("lscc/lscc.json"));
                }
            }
            catch { NAPI.Util.ConsoleOutput("(LSCC | Settings) Failed to read lscc.json, permission problem or format?"); }

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
                p.SendNotification("~r~Server stopping now!", true);



                p.KickSilent(); // Sends a "timeout" type disconnection so they can reconnect.

            });

            NAPI.Util.ConsoleOutput("Server saving & exiting...");
        }

        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            NAPI.Util.ConsoleOutput("(LSCC | STARTUP) Ensuring LSCC folder..."); 
            System.IO.Directory.CreateDirectory("lscc");

            NAPI.Util.ConsoleOutput("(LSCC | STARTUP) Ensuring logs folder..."); 
            System.IO.Directory.CreateDirectory("lscc/logs");

            NAPI.Util.ConsoleOutput("(LSCC | STARTUP) ===== Heading towards second stage of startup =====");

            NAPI.Server.SetAutoRespawnAfterDeath(false);
            NAPI.Server.SetAutoSpawnOnConnect(false);
            NAPI.Server.SetGlobalServerChat(false);
            NAPI.Server.SetCommandErrorMessage("!{#FF6347}[!] !{#FFFFFF}This command is invalid, refer to /help for a list.");

            ContextFactory.Instance.Database.EnsureCreated();
            NAPI.Util.ConsoleOutput("(LSCC | STARTUP) Connected to database.");

            Settings.Lobbies = new List<LobbyData>();
            Settings.Requests = new List<RequestData>();
            Settings.LoggedInPlayers = new List<Player>();
            Settings.AdminVehicles = new List<Vehicle>();
            Settings.Loadouts = new List<LoadoutData>();
            Settings.FreeroamSpawns = new List<SpawnSet>();

            Settings.WeaponData = new Dictionary<ulong, jWeaponData>();
            
            Settings.Dynamics = new DynSet();

            Settings.NextLobbyID = 1;
            Settings.NextRequestID = 1;
            Settings.LastCountUpdate = Environment.TickCount64;
            Settings.AutoSaveTick = Environment.TickCount64;
            Settings.LastMessageTick = Environment.TickCount64;

            Settings.DCMentionTime = Environment.TickCount64 - 900000;
            Settings.DCMentionPlayers = 0;

            NAPI.World.SetTime(12, 0, 0);
            NAPI.World.SetWeather(Weather.CLOUDS);

            for (int i = 0; i < Settings.MaxLobbies; i++)
            {
                if (i >= Settings.MaxLobbies-1) new LobbyData().gameMode = GameModes.BUSTED; // Some busted lobby too.
                else new LobbyData();
            }

            
            NAPI.Util.ConsoleOutput("(LSCC | STARTUP) Ensuring & loading lscc.json...");

            LoadDynamicSettings();

            if (!File.Exists("lscc/weapondata.json"))
            {
                NAPI.Util.ConsoleOutput("(LSCC | STARTUP) Updating weapondata.json...");
                try
                {
                    using (var client = new WebClient())
                    {
                        client.DownloadFile(Settings.Dynamics.WeaponDataURL, "lscc/weapondata.json.tmp");
                    }

                    try
                    {
                        if (File.Exists("lscc/weapondata.json"))
                        {
                            File.Delete("lscc/weapondata.json");
                        }
                    }
                    catch { NAPI.Util.ConsoleOutput("(LSCC | STARTUP) Failed to clear old weapondata.json, permission problem?"); }

                    try
                    {
                        File.Move("lscc/weapondata.json.tmp", "lscc/weapondata.json");
                    }
                    catch { NAPI.Util.ConsoleOutput("(LSCC | STARTUP) Failed to update weapondata.json! (Complete final version, 'move' fail)"); }

                }
                catch { NAPI.Util.ConsoleOutput("(LSCC | STARTUP) [Warning] Failed to update weapondata.json (not critical, using old instead, note that server will crash without one.)"); }
            }

            NAPI.Util.ConsoleOutput("(LSCC | STARTUP) Created and initialised settings & enums.");

            NAPI.Util.ConsoleOutput("(LSCC | STARTUP) ===== Heading towards final stage of startup =====");

            try
            {
                foreach (KeyValuePair<ulong, jWeaponData> item in jWeaponData.FromJson(File.ReadAllText("lscc/weapondata.json")))
                {
                    Settings.WeaponData.Add(item.Key, item.Value);
                }
            }
            catch { NAPI.Util.ConsoleOutput("(LSCC | STARTUP) Unable to load in weapondata.json! Server will work but it can cause crashes due to component data."); }
            NAPI.Util.ConsoleOutput($"(LSCC | STARTUP) Loading in weapondata.json... ({Settings.WeaponData.Count} weapons/entries)");

            /* DEFAULT LOADOUTS CREATION */
            NAPI.Util.ConsoleOutput("(LSCC | STARTUP) Creating the loadouts...");

            List<Tuple<WeaponHash, int>> wepList = new List<Tuple<WeaponHash, int>>();
            wepList.Add(new Tuple<WeaponHash, int>(WeaponHash.Combatpistol, 40));
            wepList.Add(new Tuple<WeaponHash, int>(WeaponHash.Pumpshotgun, 20));
            wepList.Add(new Tuple<WeaponHash, int>(WeaponHash.Bzgas, 1));
            wepList.Add(new Tuple<WeaponHash, int>(WeaponHash.Carbinerifle, 80));
            new LoadoutData(1, "Heavy", 5, wepList);

            wepList = new List<Tuple<WeaponHash, int>>();
            wepList.Add(new Tuple<WeaponHash, int>(WeaponHash.Pistol50, 60));
            wepList.Add(new Tuple<WeaponHash, int>(WeaponHash.Heavyshotgun, 40));
            wepList.Add(new Tuple<WeaponHash, int>(WeaponHash.Bzgas, 3));
            wepList.Add(new Tuple<WeaponHash, int>((WeaponHash)0x9D1F17E6, 120)); // Military Rifle
            new LoadoutData(1, "SWAT", 10, wepList);

            wepList = new List<Tuple<WeaponHash, int>>();
            wepList.Add(new Tuple<WeaponHash, int>(WeaponHash.Bzgas, 1));
            wepList.Add(new Tuple<WeaponHash, int>(WeaponHash.NavyRevolver, 30));
            wepList.Add(new Tuple<WeaponHash, int>(WeaponHash.Musket, 30));
            wepList.Add(new Tuple<WeaponHash, int>((WeaponHash)0x9D1F17E6, 90)); // Military Rifle
            new LoadoutData(1, "Whiplash", 20, wepList);

            wepList = new List<Tuple<WeaponHash, int>>();
            wepList.Add(new Tuple<WeaponHash, int>(WeaponHash.Combatpistol, 40));
            wepList.Add(new Tuple<WeaponHash, int>(WeaponHash.Microsmg, 50));
            wepList.Add(new Tuple<WeaponHash, int>(WeaponHash.Assaultrifle, 80));
            new LoadoutData(0, "Convicted", 5, wepList);

            wepList = new List<Tuple<WeaponHash, int>>();
            wepList.Add(new Tuple<WeaponHash, int>(WeaponHash.Battleaxe, 1));
            wepList.Add(new Tuple<WeaponHash, int>(WeaponHash.Pistol50, 40));
            wepList.Add(new Tuple<WeaponHash, int>(WeaponHash.Bullpupshotgun, 30));
            wepList.Add(new Tuple<WeaponHash, int>(WeaponHash.Combatpdw, 125));
            new LoadoutData(0, "Rebel", 10, wepList);

            wepList = new List<Tuple<WeaponHash, int>>();
            wepList.Add(new Tuple<WeaponHash, int>(WeaponHash.Pistol_mk2, 40));
            wepList.Add(new Tuple<WeaponHash, int>(WeaponHash.Minismg, 100));
            wepList.Add(new Tuple<WeaponHash, int>((WeaponHash)0x5A96BA4, 30)); // Combat Shotgun
            wepList.Add(new Tuple<WeaponHash, int>(WeaponHash.Molotov, 1));
            new LoadoutData(0, "O.G", 13, wepList);

            wepList = new List<Tuple<WeaponHash, int>>();
            wepList.Add(new Tuple<WeaponHash, int>(WeaponHash.Bzgas, 1));
            wepList.Add(new Tuple<WeaponHash, int>(WeaponHash.Doubleaction, 25));
            wepList.Add(new Tuple<WeaponHash, int>(WeaponHash.Sawnoffshotgun, 30));
            wepList.Add(new Tuple<WeaponHash, int>(WeaponHash.Gusenberg, 120));
            new LoadoutData(0, "Executioner", 15, wepList);

            wepList = new List<Tuple<WeaponHash, int>>();
            wepList.Add(new Tuple<WeaponHash, int>(WeaponHash.Knife, 1));
            wepList.Add(new Tuple<WeaponHash, int>(WeaponHash.Revolver_mk2, 15));
            wepList.Add(new Tuple<WeaponHash, int>(WeaponHash.Assaultrifle_mk2, 120));
            new LoadoutData(0, "Assassin", 20, wepList);



            if (Settings.Dynamics.Discord)
            {
                NAPI.Util.ConsoleOutput("(LSCC | STARTUP) Loading discord plugin.");
                Discord.Main();

                long tick = Environment.TickCount64 + 5000;
                while (tick > Environment.TickCount64)
                {

                }
            }
            else NAPI.Util.ConsoleOutput("(LSCC | OPT) Discord Bot is disabled, live commands and statuses not updated!");

            NAPI.TextLabel.CreateTextLabel("Bored of waiting? /dm & /dmarenas", new Vector3(440.95462f, -976.35516f, 32.059594f), 10.0f, 2.2f, 0, new Color(255, 0, 0, 156), false);
            NAPI.TextLabel.CreateTextLabel("Check out /freeroam & /cmds", new Vector3(435.73737f, -981.86523f, 30.698616f), 10.0f, 2.2f, 0, new Color(118, 232, 221, 156), false);
            NAPI.TextLabel.CreateTextLabel("Do you know about /derby?", new Vector3(448.35562f, -987.96454f, 30.689596f), 10.0f, 2.2f, 0, new Color(230, 177, 21, 156), false);

            NAPI.Util.ConsoleOutput("(LSCC | STARTUP) Server startup proceeded:\n");

            NAPI.Util.ConsoleOutput("[>===========|Los Santos Copchase|===========<]");
            NAPI.Util.ConsoleOutput("[Version]: " + Settings.Version);
            NAPI.Util.ConsoleOutput("[Developers]: KevY");
            NAPI.Util.ConsoleOutput($"[Accounts]: {ContextFactory.Instance.Accounts.Count()}");
            NAPI.Util.ConsoleOutput("[>===========|Los Santos Copchase|===========<]");


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
                                    if (player.GetSharedData<int>("InLobby") == lobbyid && player.GetData<bool>("AutoJoin") == true)
                                    {
                                        if (time > 0)
                                        {
                                            NAPI.Notification.SendNotificationToPlayer(player, $"Game is starting in ~g~{time} seconds~w~.", true);
                                        }
                                        else
                                        {
                                            NAPI.Notification.SendNotificationToPlayer(player, $"~y~Game is now starting.", true);
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
            if(Environment.TickCount64 - Settings.AutoSaveTick >= 300000) // 5 mins
            {
                ContextFactory.Instance.SaveChangesAsync();
                NAPI.Util.ConsoleOutput("Server auto-saved => " + DateTime.Now.ToString());

                Settings.AutoSaveTick = Environment.TickCount64;
            }

            if (Environment.TickCount64 - Settings.LastMessageTick > 90000)
            {
                string[] msgList = {
                    "If you would like to experience some exclusive and limited features then donate to keep the server running!",
                    "The server's discord, information and credits can be found at /discord or /info!",
                    "Use /rules to see the server rules.",
                    "Use /cmds to see the server commands.",
                    "Have you seen someone cheating? Please, contact an administrator ASAP.",
                    "Bored of waiting in the lobby? Check out our side activities like Deathmatch, Freeroam, Derby!",
                    "Want to earn some quick score? Head over to /dm!",
                    "You can save your current cop & fugitive appearance profile and select a different one by using /outfit."
                };

                RAGE.Entities.Players.All.ForEach((Player e) => e.SendChatMessage("[?] !{#F0FC72}" + msgList.ElementAt(new Random().Next(0, msgList.Length))));

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

                                    lobby.Weather = new Random().Next(0, 9); // Change weather every hour or so.

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
                                                player.TriggerEvent("SetTimeWeather", lobby.Time[0], lobby.Time[1], lobby.Time[2], lobby.Weather);

                                                player.SendChatMessage("!{#A8FFAB}[WEATHER FORECAST] " + weatherString);
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
                                            player.TriggerEvent("SetTimeWeather", lobby.Time[0], lobby.Time[1], lobby.Time[2], lobby.Weather);
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
                                                if (player.GetSharedData<int>("InLobby") == lobby.lobbyId && player.GetData<bool>("AutoJoin") == true && !EPlayer.IsAFK(player))
                                                {
                                                    pCount++;
                                                }
                                            }
                                        }

                                        if (pCount >= 2)
                                        {
                                            lobby.gameStatus = 2;

                                            if(lobby.gameMode == GameModes.STANDARD)
                                                NAPI.Chat.SendChatMessageToAll($"!{{#FFFF00}}[Lobby {lobby.lobbyId}] !{{#FFFFFF}}Copchase will start in 10 seconds... ({pCount} players)");
                                            else if(lobby.gameMode == GameModes.BUSTED)
                                                NAPI.Chat.SendChatMessageToAll($"!{{#FFFF00}}[Lobby {lobby.lobbyId}] !{{#FFFFFF}}BUSTED! mode will start in 10 seconds... ({pCount} players)");
											
											NAPI.Util.ConsoleOutput($"[TimeUpdate] Game starting in 10 seconds in lobby {lobby.lobbyId}...");
                                            foreach (Player player in Settings.LoggedInPlayers)
                                            {
                                                lock (player)
                                                {
                                                    if (player.GetSharedData<int>("InLobby") == lobby.lobbyId && player.GetData<bool>("AutoJoin") == true)
                                                    {
                                                        if(player.HasData("Spectating"))
                                                        {
                                                            Commands.cmd_specoff(player);
                                                        }
                                                        player.SetSharedData("InDM", -1);

                                                        EPlayer.Spawn(player);
                                                        NAPI.Notification.SendNotificationToPlayer(player, $"~w~Game is starting in ~g~10 seconds~w~.", true);

                                                        if (player.GetSharedData<bool>("IsTabbed") == true || EPlayer.IsAFK(player)) {

                                                            player.TriggerEvent("SendNotification", "AFK Check", "Copchase is starting in 10 seconds",
                                                                "Los Santos Copchase", 10);
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
                                    var botchan = guild.TextChannels.First(a => a.Id == Settings.Dynamics.DCMentionRoleChan);

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
                            else {
                                Settings.DCMentionPlayers = (Settings.LoggedInPlayers.Count / 2); // Half the players, but next time mention in 30 mins.
                                Settings.DCMentionTime = Environment.TickCount64 + (1800000 - 900000); // Mention next in half an hour unless PB drops.
                            }
                        }
                        else Settings.DCMentionPlayers = Settings.LoggedInPlayers.Count;
                    }
                    if (Environment.TickCount64 - Settings.LastCountUpdate >= 120000)
                    {
                        Settings.LastCountUpdate = Environment.TickCount64;

                        foreach (var guild in Discord._client.Guilds.Where(a => a.Id == Settings.Dynamics.DCServerID))
                        {
                            var botchan = guild.VoiceChannels.First(b => b.Id == Settings.Dynamics.DCCountID);

                            if (botchan != null)
                            {
                                botchan.ModifyAsync(vc => vc.Name = ($"Players Online: {RAGE.Entities.Players.Count}"));
                            }
                        }

                        foreach (var guild in Discord._client.Guilds.Where(a => a.Id == Settings.Dynamics.DCServerID))
                        {
                            var botchan = guild.VoiceChannels.First(b => b.Id == Settings.Dynamics.DCGamesID);

                            if (botchan != null)
                            {
                                botchan.ModifyAsync(vc => vc.Name = ($"Chases Running: {Settings.Lobbies.Where(l => l.gameStatus == 1).Count()}"));
                            }
                        }

                    //NAPI.Util.ConsoleOutput("Updated the DISCORD counts.");
                }
                });
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
					Discord.SendTextChannel("game-notify", $"**(Derby) | Winner: {winner.Name} | XP Gained: {winXP} | Participants: {Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == -4).Count()}**");

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
                        NAPI.Chat.SendChatMessageToAll($"[Lobby {lobbyid}] !{{#FF0000}}The game has been stopped by an administrator.");
                        Discord.SendTextChannel("game-notify", $"**(Lobby {lobbyid}) Copchase has ended! | Stopped by an admin.**");
                        break;
                    case 1:
                        NAPI.Chat.SendChatMessageToAll($"[Lobby {lobbyid}] !{{#DAA520}}The fugitives managed to escape from the cops.");
                        Discord.SendTextChannel("game-notify", $"**(Lobby {lobbyid}) Copchase has ended! | Fugitives have escaped.**");

                        foreach (Player i in lobby.Fugitives)
                        {
                            // Reward
                            EPlayer.GetAccount(i).Escapes++;
                            EPlayer.GrantXP(i, 5);
                            i.SetSharedData("FugitiveChance", i.GetSharedData<float>("FugitiveChance") + 0.3);
                        }
                        break;
                    case 2:
                        NAPI.Chat.SendChatMessageToAll($"[Lobby {lobbyid}] !{{#DAA520}}All law enforcement units have been taken out by the fugitives.");
                        Discord.SendTextChannel("game-notify", $"**(Lobby {lobbyid}) Copchase has ended! | All cops have been eliminated.**");

                        foreach (Player i in lobby.Fugitives)
                        {
                            // Reward
                            EPlayer.GrantXP(i, 3);
                            i.SetSharedData("FugitiveChance", i.GetSharedData<float>("FugitiveChance") + 0.2);
                        }
                        break;
                    case 3:
                        NAPI.Chat.SendChatMessageToAll($"[Lobby {lobbyid}] !{{#DAA520}}The situation has been successfully handled by the police.");
                        Discord.SendTextChannel("game-notify", $"**(Lobby {lobbyid}) Copchase has ended! | Police has neutralized all fugitives.**");

                        foreach (Player i in lobby.Cops)
                        {
                            // Reward
                            EPlayer.GrantXP(i, 4);
                            i.SetSharedData("FugitiveChance", i.GetSharedData<float>("FugitiveChance") + 0.3);
                        }
                        break;
                }

                lobby.gameStatus = 0;

                foreach (Player i in Settings.LoggedInPlayers)
                {
                    if (i.GetSharedData<int>("InLobby") != lobby.lobbyId) continue;

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

                lobby.Cops.Clear();
                lobby.Fugitives.Clear();
                lobby.Vehicles.Clear();
            }
            else return false;

            return true;
        }

        public static bool StartGame(int lobbyid)
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
                    if (player.GetSharedData<int>("InLobby") == lobbyid && player.GetData<bool>("AutoJoin") == true && !EPlayer.IsAFK(player) && 
                        !player.HasData("Spectating"))
                    {
                        player.SetSharedData("InDM", -1);

                        pCount++;
                    }
                }

                if (pCount < 2) { NAPI.Util.ConsoleOutput("[Lobby " + lobbyid + "] Total players less than two, failed to start"); return false; }

                decimal perc = (decimal)pCount * ((decimal)lobby.FugitivePercent / (decimal)100);

                int fugitives = (int)Decimal.Round(perc);

                if (fugitives < 1) { NAPI.Util.ConsoleOutput("[Lobby " + lobbyid + "] Fugitives count is zero, failed to start"); return false; }

                if (fugitives == pCount) fugitives--; // There will ALWAYS be AT LEAST one COP.

                int crashcheck = 0;
                try
                {
                    crashcheck++;

                    if (ContextFactory.Instance.Maps.Count() < 1) { NAPI.Util.ConsoleOutput("[Lobby " + lobbyid + "] Server has no maps added, failed to start"); return false; }
                    crashcheck++;

                    List<Map> mapList = ContextFactory.Instance.Maps.AsQueryable().Where(a => a.Enabled == true).ToList();
                    crashcheck++;

                    if (mapList.Count < 1) { NAPI.Util.ConsoleOutput("[Lobby " + lobbyid + "] Server has maps but none of them were enabled! Failed to start."); return false; }
                    crashcheck++;

                    Map map = mapList.OrderBy(c => Guid.NewGuid()).FirstOrDefault();
                    if (lobby.NextMapID != -1) map = mapList.FirstOrDefault(a => a.SQL_ID == lobby.NextMapID);
                    lobby.NextMapID = -1;

                    if (map == null) { NAPI.Util.ConsoleOutput("[Lobby " + lobbyid + "] Next (force-set) MapID == null; Failed to start!"); return false; }
                    crashcheck++;

                    NAPI.Util.ConsoleOutput($"[Lobby " + lobbyid + $"] Selected Map [{map.Name}] | Total Spawns: [{ContextFactory.Instance.MapSpawns.AsQueryable().Where(a => a.mapID == map.SQL_ID).Count()}]");
                    crashcheck++;

                    lobby.lastMapId = map.SQL_ID;

                    if (ContextFactory.Instance.MapSpawns.AsQueryable().Where(a => a.mapID == map.SQL_ID && a.spawnTeam == 0).Count() < 1) { NAPI.Util.ConsoleOutput($"[Lobby {lobbyid}] Map {map.Name} has no fugitive spawns! Abort."); return false; }
                    if (ContextFactory.Instance.MapSpawns.AsQueryable().Where(a => a.mapID == map.SQL_ID && a.spawnTeam == 1).Count() < 1) { NAPI.Util.ConsoleOutput($"[Lobby {lobbyid}] Map {map.Name} has no cop spawns! Abort."); return false; }

                    crashcheck++;

                    List<MapSpawn> Spawns = ContextFactory.Instance.MapSpawns.AsQueryable().Where(x => x.mapID == map.SQL_ID).ToList();
                    crashcheck++;

                    List<int> UsedSpawns = new List<int>(Spawns.Count + 1);
                    crashcheck++;

                    List<Player> lobbyPlayers = Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == lobbyid && !EPlayer.IsAFK(a) && !a.HasData("Spectating") && a.GetData<bool>("AutoJoin") == true).OrderBy(c => Guid.NewGuid()).ToList();
                    crashcheck++;

                    List<Player> PutInsideCar = new List<Player>(lobbyPlayers.Count());
                    crashcheck++;

                    UsedSpawns.Add(-1);
                    crashcheck++;

                    Random random = new Random();

                    pCount = 0;
                    Player _tempPlayer;
                    crashcheck++;

                    while (fugitives > 0)
                    {
                        // FULL RANDOM CHANCE MODE 
                        /*
                        if (lobbyPlayers.Where(a => a.HasData("GrantFugitive") && !lobby.Fugitives.Contains(a) && a.GetData<bool>("AutoJoin") == true).Count() > 0)
                        {
                            _tempPlayer = lobbyPlayers.Where(a => a.HasData("GrantFugitive") && !lobby.Fugitives.Contains(a) && a.GetData<bool>("AutoJoin") == true).OrderBy(c => Guid.NewGuid()).First();
                        }
                        else _tempPlayer = lobbyPlayers[random.Next(lobbyPlayers.Count)];

                        */

                        // FULL RANDOM CHANCE MODE END

                        /* HIGHEST FUGITIVE CHANCE MODE */
                        var highestFugitives = lobbyPlayers.Where(p => !lobby.Fugitives.Contains(p) && p.GetData<bool>("AutoJoin") == true).OrderByDescending(p => p.HasData("GrantFugitive")).ThenByDescending(p => p.GetSharedData<float>("FugitiveChance"));

                        if(highestFugitives.Count() == 0) _tempPlayer = lobbyPlayers[random.Next(lobbyPlayers.Count)];
                        else _tempPlayer = highestFugitives.First();


                        // HIGHEST CHANCE MODE END

                        if (_tempPlayer.GetData<bool>("AutoJoin") == false) continue;


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
                        if (player.GetData<bool>("AutoJoin") == false) continue;

                        player.SetSharedData("BustedProgress", 0.0);
                        player.ResetData("BustSupporters");
                        player.ResetData("CuffingTarget");
                        player.RemoveAllWeapons();

                        if (lobby.gameMode == GameModes.STANDARD)
                            player.TriggerEvent("showHPBars", 1);
                        else if(lobby.gameMode == GameModes.BUSTED) 
                            player.TriggerEvent("showHPBars", 3);

                        if (!lobby.Fugitives.Contains(player))
                        {
                            lobby.Cops.Add(player);
                            player.SetSharedData("FugitiveChance", player.GetSharedData<float>("FugitiveChance") + 1.0);
                        }
                    }
                    crashcheck++;

                    List<Tuple<Player, Vehicle, int>> playerVehicle = new List<Tuple<Player, Vehicle, int>>();
                    crashcheck++;

                    MapSpawn spawnLoc = null;
                    pCount = 0;
                    
                    foreach (Player player in lobby.Cops.OrderBy(x => new Random().Next()))
                    {
                        foreach(MapSpawn tSpawn in Spawns.Where(a => a.spawnTeam == 1))
                        {
                            if(!UsedSpawns.Contains(tSpawn.SQL_ID))
                            {
                                spawnLoc = tSpawn;
                                break;
                            }
                        }

                        if ((spawnLoc == null) == false)
                        {
                            if(!PutInsideCar.Contains(player)) UsedSpawns.Add(spawnLoc.SQL_ID);
                        }
                        else
                        {
                            pCount++;
                            spawnLoc = new MapSpawn();

                            spawnLoc = Spawns.First(a => a.spawnTeam == 1);

                            spawnLoc.spawnZ += (pCount * 5); // If no spawns left, stack cars ontop so at least they don't stuck in each other.
                            spawnLoc.spawnX += (pCount * 2);
                        }

                        Vector3 vecSpawn = new Vector3(spawnLoc.spawnX, spawnLoc.spawnY, spawnLoc.spawnZ);

                        CharacterData ConvChar = new CharacterData();

                        try
                        {
                            ConvChar = NAPI.Util.FromJson<CharacterData>(EPlayer.GetAccount(player).Character);

                            EPlayer.ApplyCharacter(player, ConvChar, 1);
                        }
                        catch { }

                        player.Health = 100;
                        player.Armor = 60;
                        EPlayer.SetColor(player, new Color(141, 141, 255, 255), player.Armor);

                        if (!PutInsideCar.Contains(player))
                        {
                            string carName = "police";

                            int col1 = 0; // Black?
                            int col2 = 111; // Frost white

                            if (EPlayer.GetAccount(player).CarCop.Length < 3)
                            {
                                switch (random.Next(0, 3))
                                {
                                    case 0:
                                        carName = "police";
                                        col1 = 111;
                                        break;
                                    case 1:
                                        carName = "police3";
                                        col1 = 111;
                                        break;
                                    case 2:
                                        carName = "police4";
                                        col1 = 111;
                                        break;
                                }
                            }
                            else
                            {
                                carName = EPlayer.GetAccount(player).CarCop;

                                if(carName.StartsWith("police"))
                                {
                                    col1 = 111;
                                }

                            }

                            if (carName.Equals("prevolter")) col2 = 0; // Full black undercover brr.
                            if (carName.Equals("police4")) col1 = 8; // Stone Silver // Undercover Default Cruiser
                            int overridemaxseats = GetMaxSeats(carName);

                            if (carName.Equals("polmav"))
                            {
                                if (Spawns.Count(sp => sp.spawnTeam == 2) >= 1)
                                {
                                    spawnLoc = Spawns.OrderBy(x => new Random().Next()).First(a => a.spawnTeam == 2);
                                }
                            }

                            Vehicle pVehicle = NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(carName), vecSpawn, spawnLoc.spawnA, col1, col2, player.Name, 255, false, true, Convert.ToUInt32(lobbyid));
                            pVehicle.EngineStatus = true;
                            pVehicle.SetSharedData("Engine", true);
                            pVehicle.SetSharedData("Locked", true);
                            pVehicle.Locked = true;
                            pVehicle.SetSharedData("Team", 1);

                            player.SetData("putVehicle", pVehicle);
                            player.SetData("putVehicleSeat", 0);

                            lobby.Vehicles.Add(pVehicle);

                            NAPI.Task.Run(() =>
                            {
                                lock (pVehicle)
                                {
                                    if(pVehicle.Exists && pVehicle != null) pVehicle.Position = vecSpawn;
                                }
                            }, 1000);

                            playerVehicle.Add(new Tuple<Player, Vehicle, int>(player, pVehicle, 0));
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
                                        mem.SetData("putVehicle", pVehicle);

                                        if (carName.Equals("polmav"))
                                        {
                                            // 4 - seat
                                            // 4-1, 3
                                            // 4-2, 2
                                            // 4-3, 1
                                            mem.SetData("putVehicleSeat", maxseats - seat);
                                        }
                                        else
                                        {
                                            // 0 + 1, 2, 3

                                            mem.SetData("putVehicleSeat", 0 + seat);
                                        }

                                        playerVehicle.Add(new Tuple<Player, Vehicle, int>(mem, pVehicle, 0 + seat));
                                        PutInsideCar.Add(mem);
                                        mem.SendChatMessage("!{#FFAC38}[Partners] You have been put inside " + player.Name + "'s vehicle.");

                                        seat++;
                                    }
                                }
                            }
                        }

                        player.Position = new Vector3(spawnLoc.spawnX, spawnLoc.spawnY, spawnLoc.spawnZ - 50f).Around(5f);
                        EPlayer.Freeze(player, true);

                        player.SetData("GPS", false);

                        EPlayer.SetDiscordStatus(player, $"[Lobby {lobbyid}] Cop as {player.Name}");

                        player.TriggerEvent("SetActorInvulnerable", false);
                        player.TriggerEvent("CanFallOutOfCar", false);
                        player.ResetData("CuffingTarget");
                        player.SetSharedData("Team", 1);
                    }

                    spawnLoc = null;
                    pCount = 0;
                    crashcheck++;

                    foreach (Player player in lobby.Fugitives.OrderBy(x => new Random().Next()))
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

                        player.Position = new Vector3(spawnLoc.spawnX, spawnLoc.spawnY, spawnLoc.spawnZ - 50f).Around(5f);
                        EPlayer.Freeze(player, true);

                        player.Health = 100;
                        player.Armor = 0;
                        EPlayer.SetColor(player, new Color(232, 64, 64, 255), player.Armor);

                        EPlayer.SetDiscordStatus(player, $"[Lobby {lobbyid}] Fugitive as {player.Name}");

                        player.TriggerEvent("SetActorInvulnerable", false);
                        player.TriggerEvent("CanFallOutOfCar", false);

                        player.SetSharedData("VisibleBlip", true);
                        player.ResetData("Handsup");
                        player.ResetData("Cuffed");
                        player.SetSharedData("Team", 0);
                        player.ResetData("CuffingTarget");
                        player.SetSharedData("HasShot", false);
                    }

                    spawnLoc = null;
                    List<Player> iteratedAlready = new List<Player>();
                    fugitives = lobby.Fugitives.Count;

                    while (fugitives > 0)
                    {
                        _tempPlayer = lobby.Fugitives[random.Next(lobby.Fugitives.Count)];

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

                            spawnLoc.spawnZ += (pCount * 5); // If no spawns left, stack cars ontop so at least they don't stuck in each other.
                            spawnLoc.spawnX += (pCount * 2);
                        }

                        Vector3 vecSpawn = new Vector3(spawnLoc.spawnX, spawnLoc.spawnY, spawnLoc.spawnZ);

                        string carName = "washington";

                        if (EPlayer.GetAccount(_tempPlayer).CarFugitive.Length < 3)
                        {
                            switch (random.Next(0, 6))
                            {
                                case 0:
                                    carName = "washington";
                                    break;
                                case 1:
                                    carName = "600sel";
                                    break;
                                case 2:
                                    carName = "oracle";
                                    break;
                                case 3:
                                    carName = "moonbeam";
                                    break;
                                case 4:
                                    carName = "rancherxl";
                                    break;
                                case 5:
                                    carName = "cavalcade2";
                                    break;
                            }
                        }
                        else
                        {
                            carName = EPlayer.GetAccount(_tempPlayer).CarFugitive;
                        }

                        Vehicle pVehicle = NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(carName), vecSpawn, spawnLoc.spawnA, random.Next(122), random.Next(122), $"SA-{random.Next(1000, 9999)}", 255, false, true, Convert.ToUInt32(lobbyid));
                        pVehicle.EngineStatus = true;
                        pVehicle.SetSharedData("Engine", true);
                        pVehicle.SetSharedData("Locked", false);
                        pVehicle.Locked = false;
                        pVehicle.SetSharedData("Team", 0);
                        lobby.Vehicles.Add(pVehicle);

                        NAPI.Task.Run(() =>
                        {
                            lock (pVehicle)
                            {
                                if(pVehicle != null && pVehicle.Exists) pVehicle.Position = vecSpawn;
                            }
                        }, 1000);

                        _tempPlayer.Position = new Vector3(spawnLoc.spawnX, spawnLoc.spawnY, spawnLoc.spawnZ - 50f).Around(5f);
                        EPlayer.Freeze(_tempPlayer, true);

                        playerVehicle.Add(new Tuple<Player, Vehicle, int>(_tempPlayer, pVehicle, 0));
                        _tempPlayer.SetData("putVehicle", pVehicle);
                        _tempPlayer.SetData("putVehicleSeat", 0);

                        int seat = 1;

                        int overridemaxseats = GetMaxSeats(carName); // For DLC cars that aren't registered in vehicledata.

                        int maxseats = pVehicle.MaxOccupants;
                        if (overridemaxseats != 0) maxseats = overridemaxseats;

                        if (lobby.gameMode == GameModes.BUSTED) maxseats = 1; // Separate cars for everyone by not giving available seats.
                        while (seat < maxseats)
                        {
                            if (fugitives == 0) break;

                            Player _tempPlayer2 = lobby.Fugitives[random.Next(lobby.Fugitives.Count)];

                            if (PutInsideCar.Contains(_tempPlayer2)) continue;

                            PutInsideCar.Add(_tempPlayer2);
                            iteratedAlready.Add(_tempPlayer2);
                            fugitives--;

                            _tempPlayer2.SetData("putVehicle", pVehicle);
                            _tempPlayer2.SetData("putVehicleSeat", seat);

                            _tempPlayer2.Position = new Vector3(spawnLoc.spawnX, spawnLoc.spawnY, spawnLoc.spawnZ - 50f).Around(5f);
                            EPlayer.Freeze(_tempPlayer2, true);
                            playerVehicle.Add(new Tuple<Player, Vehicle, int>(_tempPlayer2, pVehicle, 0 + seat));

                            seat++;
                        }
                    }

                    NAPI.Task.Run(() =>
                    {
                        foreach (Player player in lobby.Cops)
                        {
                            lock (player)
                            {
                                if(player != null && player.Exists) player.Dimension = Convert.ToUInt32(lobbyid);
                            }
                        }
                        foreach (Player player in lobby.Fugitives)
                        {
                            lock (player)
                            {
                                if(player != null && player.Exists) player.Dimension = Convert.ToUInt32(lobbyid);
                            }
                        }

                    }, 250);

                    NAPI.Task.Run(() =>
                    {
                        spawnLoc = new MapSpawn();

                        spawnLoc = Spawns.First(a => a.spawnTeam == 1);
                        foreach (Player player in lobby.Cops)
                        {
                            lock (player)
                            {
                                if (player != null && player.Exists)
                                {
                                    player.Position = new Vector3(spawnLoc.spawnX, spawnLoc.spawnY, spawnLoc.spawnZ).Around(5f);

                                    try
                                    {
                                        if (player.HasData("putVehicle")) player.SetIntoVehicle(player.GetData<Vehicle>("putVehicle"), player.GetData<int>("putVehicleSeat"));
                                    }
                                    catch { }
                                }
                            }
                        }

                        spawnLoc = Spawns.First(a => a.spawnTeam == 0);
                        foreach (Player player in lobby.Fugitives)
                        {
                            player.Position = new Vector3(spawnLoc.spawnX, spawnLoc.spawnY, spawnLoc.spawnZ).Around(5f);

                            try
                            {
                                if (player.HasData("putVehicle")) player.SetIntoVehicle(player.GetData<Vehicle>("putVehicle"), player.GetData<int>("putVehicleSeat"));
                            }
                            catch { }
                        }

                    }, 1700);

                    NAPI.Task.Run(() =>
                    {
                        foreach (Tuple<Player, Vehicle, int> data in playerVehicle)
                        {
                            if (data.Item1.IsNull || data.Item1 == null || data.Item2.IsNull || data.Item2 == null || data.Item1 == default(Player)) continue;

                            if (data.Item1.Exists)
                            {
                                try
                                {
                                    if (data.Item1.HasData("putVehicle")) data.Item1.SetIntoVehicle(data.Item1.GetData<Vehicle>("putVehicle"), data.Item1.GetData<int>("putVehicleSeat"));
                                }
                                catch { }

                                data.Item1.SetIntoVehicle(data.Item2, data.Item3);

                                if (!lobby.Cops.Contains(data.Item1))
                                {
                                    EPlayer.Freeze(data.Item1, false);
                                }
                                data.Item1.SetSharedData("HasShot", false);
                            }
                        }

                        if (lobby.gameMode == GameModes.STANDARD)
                        {
                            NAPI.Chat.SendChatMessageToAll($"[Lobby {lobbyid}] Copchase started with !{{#8D8DFF}}{lobby.Cops.Count} cops !{{#FFFFFF}}& !{{#E84040}}{lobby.Fugitives.Count} fugitives !{{#FFFFFF}}on map !{{#DAA520}}{map.Name}!{{#FFFFFF}}.");
                            Discord.SendTextChannel("game-notify", $"**(Lobby {lobbyid}) Standard Copchase has been started! | Cops: {lobby.Cops.Count} | Fugitives: {lobby.Fugitives.Count}**");
                        }
                        else if (lobby.gameMode == GameModes.BUSTED)
                        {
                            NAPI.Chat.SendChatMessageToAll($"[Lobby {lobbyid}] Mode BUSTED! started with !{{#8D8DFF}}{lobby.Cops.Count} cops !{{#FFFFFF}}& !{{#E84040}}{lobby.Fugitives.Count} fugitives !{{#FFFFFF}}on map !{{#DAA520}}{map.Name}!{{#FFFFFF}}.");
                            Discord.SendTextChannel("game-notify", $"**(Lobby {lobbyid}) Copchase in BUSTED! mode has been started! | Cops: {lobby.Cops.Count} | Fugitives: {lobby.Fugitives.Count}**");
                        }

                        foreach (Player p in RAGE.Entities.Players.All.Where(p => lobby.Cops.Contains(p) || lobby.Fugitives.Contains(p)))
                        {
                            if (lobby.gameMode == GameModes.STANDARD)
                            {
                                p.SendNotification($"~g~Weapons will be given after collisions enable.");
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

                        lobby.FugitiveShowCounter = 80;
                        lobby.LastSeenCounter = 0;
                        lobby.GameProgress = 0;
                        lobby.CollisionCounter = 35;

                        foreach (Player p in Settings.LoggedInPlayers)
                        {
                            p.SetSharedData("Ping", p.Ping);

                            if (p.GetSharedData<int>("InLobby") == lobbyid)
                            {
                                p.TriggerEvent("StartChaseTimer", 0, Settings.GameRunTime, lobby.CollisionCounter, (short)lobby.gameMode);
                            }
                        }
                    }, delayTime: 2000);

                    NAPI.Task.Run(() =>
                    {
                        try
                        {
                            foreach (Player p in RAGE.Entities.Players.All.Where(p => lobby.Cops.Contains(p) || lobby.Fugitives.Contains(p)))
                            {
                                if (p != null && p.Exists)
                                {
                                    if (p.HasData("putVehicle")) p.SetIntoVehicle(p.GetData<Vehicle>("putVehicle"), p.GetData<int>("putVehicleSeat"));
                                }
                            }
                        }
                        catch { }

                    }, delayTime: 3000);

                    NAPI.Task.Run(() =>
                    {
                        try
                        {
                            foreach (Player p in RAGE.Entities.Players.All.Where(p => lobby.Cops.Contains(p) || lobby.Fugitives.Contains(p)))
                            {
                                if (p != null && p.Exists)
                                {
                                    if (p.HasData("putVehicle")) p.SetIntoVehicle(p.GetData<Vehicle>("putVehicle"), p.GetData<int>("putVehicleSeat"));
                                }
                            }
                        }
                        catch { }
                    }, delayTime: 5000);


                    lobby.gameStatus = 1;
                    NAPI.Util.ConsoleOutput($"[Lobby {lobbyid}] Mode {ConvertGameMode(lobby.gameMode)} has been started with {lobby.Cops.Count} cops & {lobby.Fugitives.Count} fugitives on map {map.Name}.");
                }
                catch {
                    NAPI.Util.ConsoleOutput("Exception in startgame, ctx: " + crashcheck);
                    return false;
                }
            }
            else return false;

            return true;
        }

        public static void GameUpdate()
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

                        if ((10 - lobby.GameProgress) % 2 == 0 && lobby.GameProgress <= 8 && lobby.GameProgress > 0)
                        {
                            lobby.Cops.ForEach(p => {
                                EPlayer.Freeze(p, true);
                                p.SendNotification($"~b~Unfrozen in {10 - lobby.GameProgress}s...");
                            });
                        }

                        if (lobby.GameProgress == 10)
                        {
                            lobby.Cops.ForEach(p => {
                                EPlayer.Freeze(p, false);
                                p.SendNotification("~b~You may begin the chase now.");
                            });
                        }

                        if (lobby.GameProgress >= Convert.ToInt32(0.8 * Settings.GameRunTime) && lobby.gameMode != GameModes.BUSTED)
                        {
                            foreach (Player fugitive in lobby.Fugitives)
                            {
                                if (fugitive.GetSharedData<bool>("HasShot") == false)
                                {
                                    fugitive.SetSharedData("HasShot", true);
                                    fugitive.SetSharedData("VisibleBlip", true);
                                    lobby.FugitiveShowCounter = 29; // Set them to VISIBLE

                                    fugitive.SendChatMessage("!{#FF6347}You have the has shot tag now because the game has progress more than 80%!");
                                }
                            }
                            if (lobby.GameProgress == Convert.ToInt32(0.8 * Settings.GameRunTime) && lobby.gameMode != GameModes.BUSTED)
                            {
                                foreach (Player pl in Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == lobby.lobbyId))
                                {
                                    pl.SendChatMessage("!{#FF6347}[GAME] !{#FFFFFF}80% of the game has elapsed, all fugitives have shot tag now.");
                                    pl.TriggerEvent("StartChaseTimer", lobby.GameProgress, Settings.GameRunTime, -1, (short)lobby.gameMode);
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
                            try { lobby.Fugitives[new Random().Next(lobby.Fugitives.Count)].TriggerEvent("SendLastSeen"); } catch { }
                        }
                        lobby.LastSeenCounter = 0;
                    }

                    if (lobby.CollisionCounter > 0)
                    {
                        if (lobby.CollisionCounter % 5 == 0) // Every 5 seconds/multiple during collision phase
                        {
                            foreach (Player player in Settings.LoggedInPlayers.Where(p => p.GetSharedData<int>("InLobby") == lobby.lobbyId))
                            {
                                player.TriggerEvent("StartChaseTimer", lobby.GameProgress, Settings.GameRunTime, lobby.CollisionCounter, (short)lobby.gameMode);
                            }
                        }

                        lobby.CollisionCounter--;
                        
                        if (lobby.CollisionCounter <= 0)
                        {
                            lock (Settings.LoggedInPlayers)
                            {
                                foreach (Player player in Settings.LoggedInPlayers.Where(p => p.GetSharedData<int>("InLobby") == lobby.lobbyId))
                                {
                                    player.TriggerEvent("StartChaseTimer", lobby.GameProgress, Settings.GameRunTime, 0, (short)lobby.gameMode);

                                    player.SetSharedData("BustedProgress", 0.0);
                                    if (!lobby.Cops.Contains(player) && !lobby.Fugitives.Contains(player)) continue;

                                    player.SendNotification("~r~Collisions enabled and weapons deployed!", true);

                                    if (lobby.Cops.Contains(player))
                                    {
                                        EPlayer.GiveCopWeapons(player);
                                        player.TriggerEvent("SetActorInvulnerable", false);
                                    }
                                    else if (lobby.Fugitives.Contains(player))
                                    {
                                        EPlayer.GiveFugitiveWeapons(player);
                                        player.TriggerEvent("SetActorInvulnerable", false);
                                    }
                                }

                                foreach(Vehicle veh in lobby.Vehicles)
                                {
                                    if(veh.HasSharedData("Team") && veh.GetSharedData<int>("Team") == 0)
                                    {
                                        veh.SetSharedData("vMaxSpeed", (float)(165 / 3.6)); // 165 KM/H
                                    }
                                    if(veh.Model == 2149939648) // if it's a Prevolter reduce OP :(
                                    {
                                        veh.SetSharedData("vMaxSpeed", (float)(180 / 3.6)); // 180 KM/H
                                    }
                                }
                            }
                        }
                    }

                    lock (lobby.Cops)
                    {
                        foreach (Player cop in lobby.Cops)
                        {
                            if (cop.GetData<bool>("GPS") == false)
                            {
                                cop.SetSharedData("VisibleBlip", false);
                            }
                            else
                            {
                                cop.SetSharedData("VisibleBlip", true);
                            }
                        }
                    }
                    if (lobby.FugitiveShowCounter >= 60 && lobby.FugitiveShowCounter < 120)
                    {
                        lock (lobby.Fugitives)
                        {
                            foreach (Player fugi in lobby.Fugitives)
                            {
                                fugi.SetSharedData("VisibleBlip", true);

                                if (lobby.FugitiveShowCounter == 60) fugi.SendNotification("~r~You are now visible on the radar.", true);
                            }

                            if(lobby.FugitiveShowCounter == 60 && lobby.GameProgress >= 60 && lobby.CollisionCounter <= -1)
                            {
                                foreach(Player p in Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == lobby.lobbyId))
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

                                fugi.SendNotification("~g~You are no longer visible on the radar.", true);
                            }
                        }
                    }
                }
            }
            return;
        }
    }
}