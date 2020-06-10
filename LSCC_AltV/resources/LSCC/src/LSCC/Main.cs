using System;
using System.Collections.Generic;
using System.Text;
using AltV.Net;
using AltV.Net.Elements.Entities;
using AltV.Net.Data;
using AltV.Net.Enums;
using System.Linq;
using AltV.Net.Async;
using LSCC;
using System.Threading.Tasks;

public class LobbyData
{
    public List<EPlayer> Cops { get; set; }
    public List<EPlayer> Fugitives { get; set; }
    public List<IVehicle> Vehicles { get; set; }

    public int lobbyId { get; set; }
    public int gameStatus { get; set; }
    public int FugitiveShowCounter { get; set; }
    public int LastSeenCounter { get; set; }
    public int CollisionCounter { get; set; }
    public int GameProgress { get; set; }
    public int FugitivePercent { get; set; }
    public bool GameAutoStart { get; set; }
    public int LastAutoStartMsg { get; set; }
    public bool Enabled { get; set; }
    public bool PlayerCreated { get; set; }
    public string Password { get; set; }
    public int[] Time { get; set; }
    public int Weather { get; set; }

    public LobbyData()
    {
        Cops = new List<EPlayer>();
        Fugitives = new List<EPlayer>();
        Vehicles = new List<IVehicle>();

        Time = new int[3];
        Time[0] = new Random().Next(0, 23);
        Time[1] = new Random().Next(0, 59);
        Time[2] = new Random().Next(0, 59);

        Weather = new Random().Next(0, 9);

        lobbyId = Settings.NextLobbyID;
        Settings.NextLobbyID++;
        gameStatus = 0;
        FugitiveShowCounter = 0;
        LastSeenCounter = 0;
        CollisionCounter = 0;
        GameProgress = 0;
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
    public EPlayer Reporter { get; set; }
    public EPlayer Reported { get; set; }

    public RequestData(EPlayer reporter, string description, int type)
    {
        RequestID = Settings.NextRequestID;
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
    public List<Tuple<WeaponModel, int>> Weapons { get; set; }

    public LoadoutData(int team, string name, int level, List<Tuple<WeaponModel, int>> weps)
    {
        this.Team = team;
        this.Name = name;
        this.LevelRequired = level;

        this.Weapons = new List<Tuple<WeaponModel, int>>();
        this.Weapons.AddRange(weps);

        Settings.Loadouts.Add(this);
    }
};

public static class Settings
{
    // Static Settings / Const
    public const string Version = "0.1a";
    public const int GameRunTime = 390; // Game Running Time in Seconds before STOP
    public const int MaxLobbies = 5; // Default/starting lobby num.
    public const int DefaultMaxInLobby = 35;

    // Connection Settings
    public const string MySQL_Host = "localhost";
    public const string MySQL_User = "lsccv";
    public const string MySQL_Password = "perkovicMatko3035";
    public const string MySQL_Database = "lsccvalt";
    public const string DCBotToken = "NjAyNTQ1Mzc0MDY1OTgzNTgw.XtaCEQ.clpqVL5c0X_38xkT-mYTr4_N9a4";
    public const ulong DCServerID = 713731146072064040;
    public const ulong DCCountID = 717405966123139104;
    public const ulong DCGamesID = 717408461083639870;

    // Global Variables (changing accordingly)
    public static List<LobbyData> Lobbies { get; set; }
    public static List<EPlayer> LoggedInPlayers { get; set; }
    public static List<IVehicle> AdminVehicles { get; set; }
    public static int NextLobbyID { get; set; }
    public static int MaxPlayersInLobby { get; set; }
    public static int[] Time { get; set; }
    public static int DerbyTimeLeft { get; set; }
    public static int LastDerbyTick { get; set; }
    public static List<RequestData> Requests { get; set; }
    public static int NextRequestID { get; set; }
    public static int LastCountUpdate { get; set; }
    public static List<LoadoutData> Loadouts { get; set; }


    // Locations
    public const float LoginCameraX = -550;
    public const float LoginCameraY = -210;
    public const float LoginCameraZ = 1200;
    public const float LoginCameraRx = -50;
    public const float LoginCameraRy = 0;
    public const float LoginCameraRz = 0;
    public const float LoginCameraViewAngle = 50;

    public static Position[] LobbySpawns = new Position[]
    {
        new Position(464.8465f, -995.0156f, 24.914879f),
        new Position(450.13f, -987.06f, 26.67f),
        new Position(439.86f, -995.12f, 30.68f),
        new Position(443.308f, -986.31f, 30.689f),
        new Position(441.24f, -978.24f, 30.68f)
    };

    public static object[] VectorJail = { new Position(459.55798f, -998.202f, 24.914848f), -75.06f };

    public static string[] DMNames = new string[]
    {
        "Life Invader Office",
        "Warehouse"
    };

    public static object[][,] DMSpawns = new object[][,]
    {
        new object[,] { // Life Invader Office
            { new Position(-1042.9781f, -237.05504f, 37.96496f), 112.122505f },
            { new Position(-1065.8978f, -246.8463f, 39.733177f), 2.7907195f },
            { new Position(-1087.0682f, -250.7736f, 37.76329f), -84.64264f },
            { new Position(-1071.687f, -240.58011f, 42.271416f), 158.59511f },
            { new Position(-1081.3223f, -254.02095f, 44.023148f), -37.803333f },
            { new Position(-1063.2289f, -248.76646f, 44.02106f), -19.056158f },
            { new Position(-1055.6722f, -239.3023f, 44.021183f), 124.46403f },
            { new Position(-1049.8112f, -242.46056f, 44.021015f), 80.71298f },
            { new Position(-1056.6835f, -246.28284f, 44.02107f), 60.64799f },
            { new Position(-1078.404f, -244.99226f, 44.021255f), -105.56132f },
            { new Position(-1059.8085f, -238.40387f, 39.733192f), -145.55379f },
        },
        new object[,] { // Warehouse 
            { new Position(1026.7279f, -3100.3984f, -38.999912f), 74.94901f },
            { new Position(1018.37555f, -3093.7153f, -38.999912f), 92.48154f },
            { new Position(1000.2626f, -3091.4524f, -38.999866f), -122.95632f },
            { new Position(995.9403f, -3105.2683f, -38.999886f), -129.19583f },
            { new Position(993.27515f, -3097.609f, -38.99586f), -87.74806f },
            { new Position(1009.0142f, -3105.6724f, -38.999836f), -94.19384f },
            { new Position(1014.16077f, -3111.6494f, -38.999855f), -16.558058f },
            { new Position(1023.80054f, -3101.6365f, -38.999866f), 91.27259f },
        },
    };

    public static object[,] DerbySpawns = new object[,]
    {
        { new Position(-1154.9131f, -4625.371f, 128.5272f), 1.1739266f },
        { new Position(-1138.5717f, -4579.601f, 128.53639f), -2.9987478f },
        { new Position(-1143.5912f, -4557.85f, 128.5494f), 46.68429f },
        { new Position(-1124.3333f, -4560.672f, 128.51184f), 92.38568f },
        { new Position(-1147.1642f, -4580.426f, 130.75934f), 87.83704f },
        { new Position(-1170.742f, -4579.066f, 132.667f), 90.3033f },
        { new Position(-1191.0192f, -4579.288f, 132.67397f), 90.938835f },
        { new Position(-1201.3973f, -4579.4526f, 130.6752f), 90.72156f },
        { new Position(-1173.9358f, -4588.8486f, 139.22603f), 0.6066646f },
        { new Position(-1174.1758f, -4570.0615f, 139.22366f), -179.47902f }
    };
}


namespace LSCC
{
    class Main : AsyncResource
    {
        public static int GetMaxSeats(string vehiclename)
        {
            int seats = 0;
            return seats;
        }
        public static IDictionary<uint, string> WeaponReasons = new Dictionary<uint, string>()
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
                    weatherString = "The weather is reported to be foggy.";
                    break;
                case 5:
                    weatherString = "A huge overcast has been reported over the sky.";
                    break;
                case 6:
                    weatherString = "The sky gets darker, cloudy and starts raining, it is advised to drive safe and slow.";
                    break;
                case 7:
                    weatherString = "Thunder has been detected, it is advised to stay indoors.";
                    break;
                case 8:
                    weatherString = "The clouds are clearing and the sky gets clearer.";
                    break;
                case 9:
                    weatherString = "The sky is bright and clear, small chances of rain.";
                    break;
                default:
                    weatherString = "The atmosphere is clear.";
                    break;
            }
            return weatherString;
        }
        public static string ConvertGameStatus(int status)
        {
            string ret = "Waiting";
            switch (status)
            {
                case 0:
                    break;
                case 1:
                    ret = "Running";
                    break;
                case 2:
                    ret = "Starting";
                    break;
            }
            return ret;
        }

        public override void OnStart()
        {
            System.IO.Directory.CreateDirectory("logs");

            ContextFactory.Instance.Database.EnsureCreated();

            Alt.Log("[>===========|Los Santos Copchase|===========<]");
            Alt.Log("[Version]: " + Settings.Version);
            Alt.Log("[Developers]: KevY");
            Alt.Log($"[Accounts]: {ContextFactory.Instance.Accounts.Count()}");
            Alt.Log("[>===========|Los Santos Copchase|===========<]");


            Settings.Lobbies = new List<LobbyData>();
            Settings.Requests = new List<RequestData>();
            Settings.LoggedInPlayers = new List<EPlayer>();
            Settings.AdminVehicles = new List<IVehicle>();
            Settings.Loadouts = new List<LoadoutData>();

            Settings.MaxPlayersInLobby = Settings.DefaultMaxInLobby;
            Settings.NextLobbyID = 1;
            Settings.NextRequestID = 1;
            Settings.LastCountUpdate = Environment.TickCount;

            for (int i = 0; i < Settings.MaxLobbies; i++)
            {
                new LobbyData();
            }

            /* DEFAULT LOADOUTS CREATION */

            List<Tuple<WeaponModel, int>> wepList = new List<Tuple<WeaponModel, int>>();
            wepList.Add(new Tuple<WeaponModel, int>(WeaponModel.Pistol, 40));
            wepList.Add(new Tuple<WeaponModel, int>(WeaponModel.PumpShotgun, 20));
            wepList.Add(new Tuple<WeaponModel, int>(WeaponModel.TearGas, 1));
            wepList.Add(new Tuple<WeaponModel, int>(WeaponModel.CarbineRifle, 80));
            new LoadoutData(1, "Heavy", 5, wepList);

            wepList = new List<Tuple<WeaponModel, int>>();
            wepList.Add(new Tuple<WeaponModel, int>(WeaponModel.APPistol, 60));
            wepList.Add(new Tuple<WeaponModel, int>(WeaponModel.HeavyShotgun, 40));
            wepList.Add(new Tuple<WeaponModel, int>(WeaponModel.TearGas, 3));
            wepList.Add(new Tuple<WeaponModel, int>(WeaponModel.MG, 200));
            new LoadoutData(1, "SWAT", 10, wepList);

            wepList = new List<Tuple<WeaponModel, int>>();
            wepList.Add(new Tuple<WeaponModel, int>(WeaponModel.Pistol, 40));
            wepList.Add(new Tuple<WeaponModel, int>(WeaponModel.PumpShotgun, 15));
            wepList.Add(new Tuple<WeaponModel, int>(WeaponModel.AssaultRifle, 80));
            new LoadoutData(0, "Convicted", 5, wepList);

            wepList = new List<Tuple<WeaponModel, int>>();
            wepList.Add(new Tuple<WeaponModel, int>(WeaponModel.BattleAxe, 1));
            wepList.Add(new Tuple<WeaponModel, int>(WeaponModel.APPistol, 40));
            wepList.Add(new Tuple<WeaponModel, int>(WeaponModel.BullpupShotgun, 30));
            wepList.Add(new Tuple<WeaponModel, int>(WeaponModel.CombatPDW, 125));
            new LoadoutData(0, "Rebel", 10, wepList);

            wepList = new List<Tuple<WeaponModel, int>>();
            wepList.Add(new Tuple<WeaponModel, int>(WeaponModel.Knife, 1));
            wepList.Add(new Tuple<WeaponModel, int>(WeaponModel.HeavyRevolverMkII, 30));
            wepList.Add(new Tuple<WeaponModel, int>(WeaponModel.SniperRifle, 20));
            wepList.Add(new Tuple<WeaponModel, int>(WeaponModel.AussaultRifleMkII, 120));
            new LoadoutData(0, "Assassin", 20, wepList);


            ///////////////////////////////////////////////////////////////////////////////

            AltAsync.Do(() =>
            {
                TimeUpdate();
                GameUpdate();
            });
        }

      

        public override void OnStop()
        {
            ContextFactory.Instance.SaveChanges();
        }

        public override IEntityFactory<IPlayer> GetPlayerFactory()
        {
            return new EPlayerFactory();
        }

        public async Task GameStartCountdown(int time, int lobbyid)
        {
            await Task.Delay(2000);
            time -= 2;

            LobbyData lobby = Settings.Lobbies.Find(l => l.lobbyId == lobbyid);
            if (lobby != null)
            {
                foreach (EPlayer player in Settings.LoggedInPlayers)
                {
                    if (player.LobbyID == lobbyid && player.AutoJoin == true)
                    {
                        if (time > 0)
                        {
                            player.SendNotification($"Game is starting in ~g~{time} seconds~w~.", true);
                        }
                        else
                        {
                            player.SendNotification($"~y~Game is now starting.", true);
                        }
                    }
                }

                if (time > 0)
                {
                    await AltAsync.Do(() => GameStartCountdown(time, lobbyid));
                }
                else
                {
                    StartGame(lobbyid);
                }
            }
        }

        public async Task TimeUpdate()
        {
            await Task.Delay(15000);
            foreach (LobbyData lobby in Settings.Lobbies)
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

                            foreach (EPlayer player in Settings.LoggedInPlayers)
                            {
                                if (player.LobbyID == lobby.lobbyId)
                                {
                                    player.Emit("SetTimeWeather", lobby.Time[0], lobby.Time[1], lobby.Time[2], lobby.Weather);

                                    player.SendChatMessage("!{#A8FFAB}[WEATHER FORECAST] " + weatherString);
                                }
                            }
                        }

                        foreach (EPlayer player in Settings.LoggedInPlayers)
                        {
                            if (player.LobbyID == lobby.lobbyId)
                            {
                                player.Emit("SetTimeWeather", lobby.Time[0], lobby.Time[1], lobby.Time[2], lobby.Weather);
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
                                foreach (EPlayer player in Settings.LoggedInPlayers)
                                {
                                    if (player.LobbyID == lobby.lobbyId && player.AutoJoin == true)
                                    {
                                        pCount++;
                                    }
                                }

                                if (pCount >= 2)
                                {
                                    lobby.gameStatus = 2;

                                    foreach (EPlayer player in Settings.LoggedInPlayers)
                                    {
                                        player.SendChatMessage($"!{{#FFFF00}}[Lobby {lobby.lobbyId}] !{{#FFFFFF}}Copchase will start in 10 seconds... ({pCount} players currently)");
                                        if (player.LobbyID == lobby.lobbyId && player.AutoJoin == true)
                                        {
                                            player.DMID = -1;

                                            player.Spawn();
                                            player.SendNotification($"~w~Game is starting in ~g~10 seconds~w~.", true);

                                            Alt.Log($"[TimeUpdate] Game starting in 10 seconds in lobby {lobby.lobbyId}...");
                                        }
                                    }

                                    await AltAsync.Do(() => GameStartCountdown(10, lobby.lobbyId));
                                }
                                else
                                {
                                    if (Environment.TickCount - lobby.LastAutoStartMsg >= 90000)
                                    {
                                        foreach (EPlayer player in Settings.LoggedInPlayers)
                                        {
                                            if(player.LobbyID == lobby.lobbyId) player.SendChatMessage("!{#FFFF00}[GAME] !{#FFFFFF}Game will not start unless there are 2+ players to start a match.");
                                        }
                                        lobby.LastAutoStartMsg = Environment.TickCount;
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (EPlayer player in Settings.LoggedInPlayers)
                            {
                                if (player.LobbyID == lobby.lobbyId)
                                {
                                    player.LobbyID = -1;
                                    player.SendChatMessage("!{#FF0000}This lobby is disabled, resetting you to any other lobby.");
                                    player.Spawn();
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

            await AltAsync.Do(TimeUpdate);
            return;
        }

        public static void CheckDerbyWinner()
        {
            int derbyPlayers = Settings.LoggedInPlayers.Where(a => a.LobbyID == -4).Count();
            if (Settings.DerbyTimeLeft <= 0 && derbyPlayers >= 2)
            {
                EPlayer winner = null;
                int lastBigTime = 0;
                int winXP = derbyPlayers / 2;

                foreach (EPlayer player in Settings.LoggedInPlayers.Where(a => a.LobbyID == -4))
                {
                    if (player.DerbyTime > lastBigTime)
                    {
                        lastBigTime = player.DerbyTime;
                        winner = player;
                    }
                }

                foreach (EPlayer player in Settings.LoggedInPlayers.Where(a => a.LobbyID == -4))
                {
                    player.Health = 0;
                    if (winner == null)
                    {
                        player.SendChatMessage("!{#E6DD3C}[DERBY] !{#FFFFFF}There was no winner in the derby match! Starting a new round.");
                    }
                    else
                    {
                        player.SendChatMessage("!{#E6DD3C}[DERBY] " + winner.Name + " won the derby and has gained " + winXP + "XP.");
                    }
                }

                if (winner != null) winner.GrantXP(winXP);

                Settings.DerbyTimeLeft = 180;
                Settings.LastDerbyTick = Environment.TickCount + 3000;
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
                        foreach(EPlayer p in Settings.LoggedInPlayers) p.SendChatMessage($"[Lobby {lobbyid}] !{{#FF0000}}The game has been stopped by an administrator.");
                        break;
                    case 1:
                        foreach (EPlayer p in Settings.LoggedInPlayers) p.SendChatMessage($"[Lobby {lobbyid}] !{{#DAA520}}The fugitives managed to escape from the cops.");

                        foreach (EPlayer i in lobby.Fugitives)
                        {
                            // Reward
                            i.GrantXP(5);
                        }
                        break;
                    case 2:
                        foreach (EPlayer p in Settings.LoggedInPlayers) p.SendChatMessage($"[Lobby {lobbyid}] !{{#DAA520}}All law enforcement units have been taken out by the fugitives.");

                        foreach (EPlayer i in lobby.Fugitives)
                        {
                            // Reward
                            i.GrantXP(3);
                        }
                        break;
                    case 3:
                        foreach (EPlayer p in Settings.LoggedInPlayers) p.SendChatMessage($"[Lobby {lobbyid}] !{{#DAA520}}The situation has been successfully handled by the police.");

                        foreach (EPlayer i in lobby.Cops)
                        {
                            // Reward
                            i.GrantXP(4);
                        }
                        break;
                }

                lobby.gameStatus = 0;

                foreach (EPlayer i in Settings.LoggedInPlayers)
                {
                    if (i.LobbyID != lobby.lobbyId) continue;

                    i.Emit("HideChaseTimer");
                    if (lobby.Cops.Contains(i))
                    {
                        lobby.Cops.Remove(i);
                        i.Spawn();
                    }
                    if (lobby.Fugitives.Contains(i))
                    {
                        lobby.Fugitives.Remove(i);
                        i.Spawn();
                    }
                }

                for (int i = lobby.Vehicles.Count - 1; i >= 0; i--)
                {
                    if (!lobby.Vehicles[i].Exists && lobby.Vehicles[i] != null)
                    {
                        lobby.Vehicles[i].Remove();
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
                if (lobby.gameStatus == 1) { Alt.Log("[Lobby " + lobbyid + "] Copchase is already running, failed to start"); return false; }

                lobby.gameStatus = 0;
                int pCount = 0;

                lobby.Cops.Clear();
                lobby.Fugitives.Clear();
                lobby.GameProgress = 0;

                foreach (EPlayer player in Settings.LoggedInPlayers)
                {
                    if (player.LobbyID == lobbyid && player.AutoJoin == true)
                    {
                        player.DMID = -1;

                        pCount++;
                    }
                }

                if (pCount < 2) { Alt.Log("[Lobby " + lobbyid + "] Total players less than two, failed to start"); return false; }

                decimal perc = (decimal)pCount * ((decimal)lobby.FugitivePercent / (decimal)100);

                int fugitives = (int)Decimal.Round(perc);

                if (fugitives < 1) { Alt.Log("[Lobby " + lobbyid + "] Fugitives count is zero, failed to start"); return false; }

                if (fugitives == pCount) fugitives--; // There will ALWAYS be AT LEAST one COP.

                int crashcheck = 0;
                try
                {
                    crashcheck++;

                    if (ContextFactory.Instance.Maps.Count() < 1) { Alt.Log("[Lobby " + lobbyid + "] Server has no maps added, failed to start"); return false; }
                    crashcheck++;

                    List<Map> mapList = ContextFactory.Instance.Maps.AsQueryable().Where(a => a.Enabled == true).ToList();
                    crashcheck++;

                    if (mapList.Count < 1) { Alt.Log("[Lobby " + lobbyid + "] Server has maps but none of them were enabled! Failed to start."); return false; }
                    crashcheck++;

                    Map map = mapList.OrderBy(c => Guid.NewGuid()).FirstOrDefault();
                    crashcheck++;

                    Alt.Log($"[Lobby " + lobbyid + $"] Selected Map [{map.Name}] | Total Spawns: [{ContextFactory.Instance.MapSpawns.AsQueryable().Where(a => a.mapID == map.SQL_ID).Count()}]");
                    crashcheck++;

                    if (ContextFactory.Instance.MapSpawns.AsQueryable().Where(a => a.mapID == map.SQL_ID && a.spawnTeam == 0).Count() < 1) { Alt.Log($"[Lobby {lobbyid}] Map {map.Name} has no fugitive spawns! Abort."); return false; }
                    if (ContextFactory.Instance.MapSpawns.AsQueryable().Where(a => a.mapID == map.SQL_ID && a.spawnTeam == 1).Count() < 1) { Alt.Log($"[Lobby {lobbyid}] Map {map.Name} has no cop spawns! Abort."); return false; }

                    crashcheck++;

                    List<MapSpawn> Spawns = ContextFactory.Instance.MapSpawns.AsQueryable().Where(x => x.mapID == map.SQL_ID).ToList();
                    crashcheck++;

                    List<int> UsedSpawns = new List<int>(Spawns.Count + 1);
                    crashcheck++;

                    List<EPlayer> lobbyPlayers = Settings.LoggedInPlayers.Where(a => a.LobbyID == lobbyid).ToList();
                    crashcheck++;

                    List<EPlayer> PutInsideCar = new List<EPlayer>(lobbyPlayers.Count());
                    crashcheck++;

                    UsedSpawns.Add(-1);
                    crashcheck++;

                    Random random = new Random();

                    pCount = 0;
                    EPlayer _tempPlayer;
                    crashcheck++;

                    while (fugitives > 0)
                    {
                        _tempPlayer = lobbyPlayers[random.Next(lobbyPlayers.Count)];

                        if (_tempPlayer.AutoJoin == false) continue;

                        if (!lobby.Fugitives.Contains(_tempPlayer))
                        {
                            fugitives--;
                            lobby.Fugitives.Add(_tempPlayer);
                        }
                        else pCount++;

                        if (pCount >= 100)
                        {
                            Alt.Log("[Lobby " + lobbyid + "] Stuck inside random iteration loop for selecting fugitive, aborting method.");
                            return false;
                        }
                    }
                    fugitives = lobby.Fugitives.Count;
                    crashcheck++;

                    foreach (EPlayer player in lobbyPlayers)
                    {
                        if (player.AutoJoin == false) continue;

                        player.Cuffed = false;
                        if (!lobby.Fugitives.Contains(player))
                        {
                            player.CuffingTarget = null;
                            lobby.Cops.Add(player);
                        }
                    }
                    crashcheck++;

                    List<Tuple<EPlayer, IVehicle, int>> playerVehicle = new List<Tuple<EPlayer, IVehicle, int>>();
                    crashcheck++;

                    MapSpawn spawnLoc = null;
                    pCount = 0;

                    foreach (EPlayer player in lobby.Cops.OrderBy(x => new Random().Next()))
                    {
                        player.Model = Alt.Hash("FreemodeMale01");

                        foreach (MapSpawn tSpawn in Spawns.Where(a => a.spawnTeam == 1))
                        {
                            if (!UsedSpawns.Contains(tSpawn.SQL_ID))
                            {
                                spawnLoc = tSpawn;
                                break;
                            }
                        }

                        if ((spawnLoc == null) == false)
                        {
                            if (!PutInsideCar.Contains(player)) UsedSpawns.Add(spawnLoc.SQL_ID);
                        }
                        else
                        {
                            pCount++;
                            spawnLoc = new MapSpawn();

                            spawnLoc = Spawns.First(a => a.spawnTeam == 1);

                            spawnLoc.spawnZ += (pCount * 5); // If no spawns left, stack cars ontop so at least they don't stuck in each other.
                            spawnLoc.spawnX += (pCount * 2);
                        }

                        Position vecSpawn = new Position(spawnLoc.spawnX, spawnLoc.spawnY, spawnLoc.spawnZ);

                        player.Health = 100;
                        player.Armor = 60;
                        player.SetColor(141, 141, 255);

                        if (!PutInsideCar.Contains(player))
                        {
                            string carName = "police";

                            byte col1 = 0;

                            if (player.Account.CarCop.Length < 3)
                            {
                                switch (random.Next(0, 3))
                                {
                                    case 0:
                                        carName = "police";
                                        col1 = 111;
                                        break;
                                    case 1:
                                        carName = "police3";
                                        col1 = 8;
                                        break;
                                    case 2:
                                        carName = "police4";
                                        col1 = 111;
                                        break;
                                }
                            }
                            else
                            {
                                carName = player.Account.CarCop;

                                if (carName.StartsWith("police"))
                                {
                                    col1 = 111;
                                }
                            }

                            int overridemaxseats = GetMaxSeats(carName);

                            if (carName.Equals("polvmav"))
                            {
                                if (Spawns.Count(sp => sp.spawnTeam == 2) >= 1)
                                {
                                    spawnLoc = Spawns.OrderBy(x => new Random().Next()).First(a => a.spawnTeam == 2);
                                }
                            }

                            IVehicle pVehicle = Alt.CreateVehicle(carName, vecSpawn, new Rotation(spawnLoc.spawnRoll, spawnLoc.spawnPitch, spawnLoc.spawnYaw));
                            pVehicle.PrimaryColor = col1;
                            pVehicle.SecondaryColor = 111;
                            pVehicle.NumberplateText = player.Name;
                            pVehicle.EngineOn = true;
                            pVehicle.Dimension = lobbyid;

                            pVehicle.SetSyncedMetaData("Team", 1);

                            player.SetData("putVehicle", pVehicle);
                            player.SetData("putVehicleSeat", 0);

                            lobby.Vehicles.Add(pVehicle);

                            playerVehicle.Add(new Tuple<EPlayer, IVehicle, int>(player, pVehicle, 0));
                            PutInsideCar.Add(player);

                            int seat = 1;
                            int maxseats = GetMaxSeats(carName);

                            if (player.PlayerSquad != null)
                            {
                                foreach (EPlayer mem in Settings.LoggedInPlayers.Where(p => p.PlayerSquad != null && p.PlayerSquad == player.PlayerSquad && lobby.Cops.Contains(p) && p.Id != player.Id))
                                {
                                    if (seat < maxseats)
                                    {
                                        mem.SetData("putVehicle", pVehicle);
                                        mem.SetData("putVehicleSeat", 0 + seat);

                                        playerVehicle.Add(new Tuple<EPlayer, IVehicle, int>(mem, pVehicle, 0 + seat));
                                        PutInsideCar.Add(mem);
                                        mem.SendChatMessage("!{#FFAC38}[Partners] You have been put inside " + player.Name + "'s vehicle.");

                                        seat++;
                                    }
                                }
                            }
                        }

                        player.Freeze(true);

                        player.GPS = false;
                        player.Visible = false;

                        player.Emit("SetActorInvulnerable", false);
                        player.Emit("CanFallOutOfCar", true);
                        player.Emit("showHPBars", 1); // Always show HP Bars.
                        player.CuffingTarget = null;
                        player.Team = 1;
                    }

                    spawnLoc = null;
                    pCount = 0;
                    crashcheck++;

                    foreach (EPlayer player in lobby.Fugitives.OrderBy(x => new Random().Next()))
                    {
                        player.Model = Alt.Hash("FreemodeMale01");

                        spawnLoc = new MapSpawn();

                        spawnLoc = Spawns.First(a => a.spawnTeam == 0);

                        Position vecSpawn = new Position(spawnLoc.spawnX, spawnLoc.spawnY, spawnLoc.spawnZ);

                        player.Freeze(true);

                        player.Health = 100;
                        player.Armor = 0;
                        player.SetColor(232, 64, 64);

                        player.Emit("SetActorInvulnerable", false);
                        player.Emit("CanFallOutOfCar", true);
                        player.Emit("showHPBars", 1); // Always show HP Bars.

                        player.Visible = true;
                        player.Cuffed = false;
                        player.Team = 0;
                        player.CuffingTarget = null;
                        player.HasShot = false;
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

                        Position vecSpawn = new Position(spawnLoc.spawnX, spawnLoc.spawnY, spawnLoc.spawnZ);

                        string carName = "washington";

                        switch (random.Next(0, 5))
                        {
                            case 0:
                                carName = "washington";
                                break;
                            case 1:
                                carName = "sultan";
                                break;
                            case 2:
                                carName = "asea";
                                break;
                            case 3:
                                carName = "cognoscenti";
                                break;
                            case 4:
                                carName = "oracle2";
                                break;
                        }

                        IVehicle pVehicle = Alt.CreateVehicle(carName, vecSpawn, new Rotation(spawnLoc.spawnRoll, spawnLoc.spawnPitch, spawnLoc.spawnYaw));

                        pVehicle.PrimaryColor = Convert.ToByte(random.Next(122));
                        pVehicle.SecondaryColor = Convert.ToByte(random.Next(122));
                        pVehicle.NumberplateText = $"SA-{random.Next(1000, 9999)}";
                        pVehicle.EngineOn = true;
                        pVehicle.Dimension = lobby.lobbyId;
                        pVehicle.SetSyncedMetaData("Team", 0);

                        lobby.Vehicles.Add(pVehicle);

                        
                        _tempPlayer.Freeze(true);

                        playerVehicle.Add(new Tuple<EPlayer, IVehicle, int>(_tempPlayer, pVehicle, 0));
                        _tempPlayer.SetData("putVehicle", pVehicle);
                        _tempPlayer.SetData("putVehicleSeat", 0);

                        int seat = 1;
                        int maxseats = GetMaxSeats(carName);

                        while (seat < maxseats)
                        {
                            if (fugitives == 0) break;

                            EPlayer _tempPlayer2 = lobby.Fugitives[random.Next(lobby.Fugitives.Count)];

                            if (PutInsideCar.Contains(_tempPlayer2)) continue;

                            PutInsideCar.Add(_tempPlayer2);
                            iteratedAlready.Add(_tempPlayer2);
                            fugitives--;

                            _tempPlayer2.SetData("putVehicle", pVehicle);
                            _tempPlayer2.SetData("putVehicleSeat", seat);

                            _tempPlayer2.Freeze(true);
                            playerVehicle.Add(new Tuple<EPlayer, IVehicle, int>(_tempPlayer2, pVehicle, 0 + seat));

                            seat++;
                        }
                    }

                    AltAsync.Do(() =>
                    {
                        Task.Delay(1000);
                        foreach (EPlayer player in lobby.Cops)
                        {
                            player.Dimension = lobbyid;
                        }
                        foreach (EPlayer player in lobby.Fugitives)
                        {
                            player.Dimension = lobbyid;
                        }

                    });

                    AltAsync.Do(() =>
                    {
                        Task.Delay(1700);
                        spawnLoc = new MapSpawn();

                        spawnLoc = Spawns.First(a => a.spawnTeam == 1);
                        foreach (EPlayer player in lobby.Cops)
                        {
                            player.Position = new Position(spawnLoc.spawnX, spawnLoc.spawnY, spawnLoc.spawnZ - 5f);

                            try
                            {
                                if (player.HasData("putVehicle"))
                                {
                                    IVehicle pVeh = null;
                                    int pSeat = 0;
                                    player.GetData<IVehicle>("putVehicle", out pVeh);
                                    player.GetData<int>("putVehicleSeat", out pSeat);

                                    player.Emit("putInsideVehicle", pVeh, pSeat);
                                }
                            }
                            catch { }
                        }

                        spawnLoc = Spawns.First(a => a.spawnTeam == 0);
                        foreach (Player player in lobby.Fugitives)
                        {
                            player.Position = new Position(spawnLoc.spawnX, spawnLoc.spawnY, spawnLoc.spawnZ - 5f);

                            try
                            {
                                if (player.HasData("putVehicle"))
                                {
                                    IVehicle pVeh = null;
                                    int pSeat = 0;
                                    player.GetData<IVehicle>("putVehicle", out pVeh);
                                    player.GetData<int>("putVehicleSeat", out pSeat);

                                    player.Emit("putInsideVehicle", pVeh, pSeat);
                                }
                            }
                            catch { }
                        }
                    });

                    AltAsync.Do(() =>
                    {
                        Task.Delay(2000);
                        foreach (Tuple<EPlayer, IVehicle, int> data in playerVehicle)
                        {
                            if (!data.Item1.Exists || data.Item1 == null || !data.Item2.Exists || data.Item2 == null || data.Item1 == default(Player)) continue;

                            try
                            {
                                if (data.Item1.HasData("putVehicle"))
                                {
                                    IVehicle pVeh = null;
                                    int pSeat = 0;
                                    data.Item1.GetData<IVehicle>("putVehicle", out pVeh);
                                    data.Item1.GetData<int>("putVehicleSeat", out pSeat);

                                    data.Item1.Emit("putInsideVehicle", pVeh, pSeat);
                                }
                            }
                            catch { }

                            data.Item1.Freeze(false);

                            data.Item1.Emit("ToggleChaseCollisions", false);
                            data.Item1.HasShot = false;

                            data.Item1.SendNotification($"~g~Weapons will be given after collisions enable.");
                        }

                        Alt.Log($"[Lobby {lobbyid}] Copchase started with !{{#8D8DFF}}{lobby.Cops.Count} cops !{{#FFFFFF}}& !{{#E84040}}{lobby.Fugitives.Count} fugitives !{{#FFFFFF}}on map !{{#DAA520}}{map.Name}!{{#FFFFFF}}.");

                        lobby.FugitiveShowCounter = 60;
                        lobby.LastSeenCounter = 0;
                        lobby.GameProgress = 0;
                        lobby.CollisionCounter = 30;

                        foreach (EPlayer p in Settings.LoggedInPlayers)
                        {
                            p.SetSyncedMetaData("Ping", p.Ping);

                            if (p.Ping == lobbyid)
                            {
                                p.Emit("StartChaseTimer", 0, Settings.GameRunTime, lobby.CollisionCounter);
                            }
                        }
                    });

                    AltAsync.Do(() =>
                    {
                        Task.Delay(3000);
                        foreach (EPlayer p in lobby.Cops)
                        {
                            p.Emit("ToggleChaseCollisions", false);
                        }
                        foreach (EPlayer p in lobby.Fugitives)
                        {
                            p.Emit("ToggleChaseCollisions", false);
                        }

                    });

                    AltAsync.Do(() =>
                    {
                        Task.Delay(3000);
                        foreach (EPlayer player in lobby.Fugitives)
                        {
                            try
                            {
                                player.Emit("ToggleChaseCollisions", false);
                                if (player.HasData("putVehicle"))
                                {
                                    IVehicle pVeh = null;
                                    int pSeat = 0;
                                    player.GetData<IVehicle>("putVehicle", out pVeh);
                                    player.GetData<int>("putVehicleSeat", out pSeat);

                                    player.Emit("putInsideVehicle", pVeh, pSeat);
                                }
                            }
                            catch { }
                        }

                        foreach (EPlayer player in lobby.Cops)
                        {
                            try
                            {
                                player.Emit("ToggleChaseCollisions", false);
                                if (player.HasData("putVehicle"))
                                {
                                    IVehicle pVeh = null;
                                    int pSeat = 0;
                                    player.GetData<IVehicle>("putVehicle", out pVeh);
                                    player.GetData<int>("putVehicleSeat", out pSeat);

                                    player.Emit("putInsideVehicle", pVeh, pSeat);
                                }
                            }
                            catch { }
                        }

                    });

                    AltAsync.Do(() =>
                    {
                        Task.Delay(5000);
                        foreach (EPlayer player in lobby.Fugitives)
                        {
                            try
                            {
                                player.Emit("ToggleChaseCollisions", false);
                                if (player.HasData("putVehicle"))
                                {
                                    IVehicle pVeh = null;
                                    int pSeat = 0;
                                    player.GetData<IVehicle>("putVehicle", out pVeh);
                                    player.GetData<int>("putVehicleSeat", out pSeat);

                                    player.Emit("putInsideVehicle", pVeh, pSeat);
                                }
                            }
                            catch { }
                        }

                        foreach (EPlayer player in lobby.Cops)
                        {
                            try
                            {
                                player.Emit("ToggleChaseCollisions", false); 
                                if (player.HasData("putVehicle"))
                                {
                                    IVehicle pVeh = null;
                                    int pSeat = 0;
                                    player.GetData<IVehicle>("putVehicle", out pVeh);
                                    player.GetData<int>("putVehicleSeat", out pSeat);

                                    player.Emit("putInsideVehicle", pVeh, pSeat);
                                }
                            }
                            catch { }
                        }

                    });


                    lobby.gameStatus = 1;
                    Alt.Log($"[Lobby {lobbyid}] Game has been started with {lobby.Cops.Count} cops & {lobby.Fugitives.Count} fugitives on map {map.Name}.");
                }
                catch
                {
                    Alt.Log("Exception in startgame, ctx: " + crashcheck);
                    return false;
                }
            }
            else return false;

            return true;
        }

        public async Task GameUpdate()
        {
            await Task.Delay(1000);
            foreach (EPlayer pl in Settings.LoggedInPlayers)
            {
                pl.UpdateBlip();
            }

            if (Environment.TickCount - Settings.LastDerbyTick >= 1000 && Settings.LoggedInPlayers.Where(a => a.LobbyID == -4).Count() >= 2)
            {
                Settings.DerbyTimeLeft--;
                Settings.LastDerbyTick = Environment.TickCount;

                if (Settings.DerbyTimeLeft <= 0)
                {
                    Main.CheckDerbyWinner();
                }
            }

            foreach (LobbyData lobby in Settings.Lobbies.Where(l => l.gameStatus == 1))
            {
                lobby.GameProgress++;

                if (lobby.GameProgress >= Convert.ToInt32(0.6 * Settings.GameRunTime))
                {
                    foreach (EPlayer fugitive in lobby.Fugitives)
                    {
                        if (fugitive.HasShot == false)
                        {
                            fugitive.HasShot = true;
                            fugitive.Visible = true;
                            lobby.FugitiveShowCounter = 29; // Set them to VISIBLE

                            fugitive.SendChatMessage("!{#FF6347}You have the has shot tag now because the game has progress more than 60%!");
                        }
                    }
                    if (lobby.GameProgress == Convert.ToInt32(0.6 * Settings.GameRunTime))
                    {
                        foreach (EPlayer i in Settings.LoggedInPlayers.Where(a => a.LobbyID == lobby.lobbyId))
                        {
                            i.SendChatMessage("!{#FF6347}[GAME] !{#FFFFFF}60% of the game has elapsed, all fugitives have shot tag now.");
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
                        lobby.Fugitives[new Random().Next(lobby.Fugitives.Count)].Emit("SendLastSeen");
                    }
                    lobby.LastSeenCounter = 0;
                }

                if (lobby.CollisionCounter > 0)
                {
                    lobby.CollisionCounter--;

                    if (lobby.CollisionCounter < 1)
                    {
                        foreach (EPlayer player in Settings.LoggedInPlayers.Where(p => p.LobbyID == lobby.lobbyId))
                        {
                            if (!lobby.Cops.Contains(player) && !lobby.Fugitives.Contains(player)) continue;

                            player.Emit("ToggleChaseCollisions", true);
                            player.SendNotification($"~r~Collisions enabled and weapons deployed!", true);

                            if (lobby.Cops.Contains(player))
                            {
                                player.GiveCopWeapons();
                            }
                            else if (lobby.Fugitives.Contains(player))
                            {
                                player.GiveFugitiveWeapons();
                            }
                        }
                    }
                }

                foreach (EPlayer cop in lobby.Cops)
                {
                    if (cop.GPS == false)
                    {
                        cop.Visible = false;
                    }
                    else
                    {
                        cop.Visible = true;
                    }
                }

                if (lobby.FugitiveShowCounter >= 30 && lobby.FugitiveShowCounter < 120)
                {
                    foreach (EPlayer fugi in lobby.Fugitives)
                    {
                        fugi.Visible = true;

                        if (lobby.FugitiveShowCounter == 30) fugi.SendNotification("~r~You are now visible on the radar.", true);
                    }
                }

                if (lobby.FugitiveShowCounter >= 120)
                {
                    lobby.FugitiveShowCounter = 0;
                    foreach (EPlayer fugi in lobby.Fugitives)
                    {
                        fugi.Visible = false;

                        fugi.SendNotification("~g~You are no longer visible on the radar.", true);
                    }
                }
            }

            await AltAsync.Do(GameUpdate);
            return;
        }
    }
}
