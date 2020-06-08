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

public class LobbyData
{
    public List<Player> Cops { get; set; }
    public List<Player> Fugitives { get; set; }
    public List<Vehicle> Vehicles { get; set; }

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
        Cops = new List<Player>();
        Fugitives = new List<Player>();
        Vehicles = new List<Vehicle>();

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
    public Player Reporter { get; set; }
    public Player Reported { get; set; }

    public RequestData(Player reporter, string description, int type)
    {
        RequestID = Settings.NextRequestID;
        Settings.NextRequestID++;
        Reporter = reporter;
        Description = description;
        RequestType = type;
        Reported = reporter;
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
    public const string MySQL_Database = "lsccv";
    public const string DCBotToken = "NjAyNTQ1Mzc0MDY1OTgzNTgw.XtaCEQ.clpqVL5c0X_38xkT-mYTr4_N9a4";
    public const ulong DCServerID = 713731146072064040;
    public const ulong DCCountID = 717405966123139104;
    public const ulong DCGamesID = 717408461083639870;

    // Global Variables (changing accordingly)
    public static List<LobbyData> Lobbies { get; set; }
    public static List<Player> LoggedInPlayers { get; set; }
    public static List<Vehicle> AdminVehicles { get; set; }
    public static int NextLobbyID { get; set; }
    public static int MaxPlayersInLobby { get; set; }
    public static int[] Time { get; set; }
    public static int DerbyTimeLeft { get; set; }
    public static int LastDerbyTick { get; set; }
    public static List<RequestData> Requests { get; set; }
    public static int NextRequestID { get; set; }
    public static int LastCountUpdate { get; set; }

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
        "Warehouse"
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
    };

    public static object[,] DerbySpawns = new object[,]
    {
        { new Vector3(-1154.9131f, -4625.371f, 128.5272f), 1.1739266f },
        { new Vector3(-1138.5717f, -4579.601f, 128.53639f), -2.9987478f },
        { new Vector3(-1143.5912f, -4557.85f, 128.5494f), 46.68429f },
        { new Vector3(-1124.3333f, -4560.672f, 128.51184f), 92.38568f },
        { new Vector3(-1147.1642f, -4580.426f, 130.75934f), 87.83704f },
        { new Vector3(-1170.742f, -4579.066f, 132.667f), 90.3033f },
        { new Vector3(-1191.0192f, -4579.288f, 132.67397f), 90.938835f },
        { new Vector3(-1201.3973f, -4579.4526f, 130.6752f), 90.72156f },
        { new Vector3(-1173.9358f, -4588.8486f, 139.22603f), 0.6066646f },
        { new Vector3(-1174.1758f, -4570.0615f, 139.22366f), -179.47902f }
    };
}

namespace LSCC
{
    public class Main : Script
    {
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

        public static void Log(string fileName, string msg)
        {
            var writer = new System.IO.StreamWriter($"logs/{fileName}", true);

            writer.WriteLine(msg);
            writer.Flush();
            writer.Close();
            return;
        }

        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            System.IO.Directory.CreateDirectory("logs");

            NAPI.Server.SetAutoRespawnAfterDeath(false);
            NAPI.Server.SetAutoSpawnOnConnect(false);
            NAPI.Server.SetGlobalServerChat(false);
            NAPI.Server.SetCommandErrorMessage("!{#FF6347}[!] !{#FFFFFF}This command is invalid, refer to /help for a list.");

            ContextFactory.Instance.Database.EnsureCreated();

            NAPI.Util.ConsoleOutput("[>===========|Los Santos Copchase|===========<]");
            NAPI.Util.ConsoleOutput("[Version]: " + Settings.Version);
            NAPI.Util.ConsoleOutput("[Developers]: KevY");
            NAPI.Util.ConsoleOutput($"[Accounts]: {ContextFactory.Instance.Accounts.Count()}");
            NAPI.Util.ConsoleOutput("[>===========|Los Santos Copchase|===========<]");

           
            Settings.Lobbies = new List<LobbyData>();
            Settings.Requests = new List<RequestData>();
            Settings.LoggedInPlayers = new List<Player>();
            Settings.AdminVehicles = new List<Vehicle>();

            Settings.MaxPlayersInLobby = Settings.DefaultMaxInLobby;
            Settings.NextLobbyID = 1;
            Settings.NextRequestID = 1;
            Settings.LastCountUpdate = Environment.TickCount;

            NAPI.World.SetTime(12, 0, 0);
            NAPI.World.SetWeather(Weather.CLOUDS);

            for(int i = 0; i < Settings.MaxLobbies; i++)
            {
                LobbyData eLobbyData = new LobbyData();
            }

            TimeUpdate();
            GameUpdate();

            Discord.Main();
        }

        public void GameStartCountdown(int time, int lobbyid)
        {
            time -= 2;

            LobbyData lobby = Settings.Lobbies.Find(l => l.lobbyId == lobbyid);
            if (lobby != null)
            {
                foreach (Player player in Settings.LoggedInPlayers)
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

        public void TimeUpdate()
        {
            if(Environment.TickCount - Settings.LastCountUpdate >= 120000 && 
                (int)Discord._client.ConnectionState == 2)
            {
                Settings.LastCountUpdate = Environment.TickCount;

                foreach (var guild in Discord._client.Guilds.Where(a => a.Id == Settings.DCServerID))
                {
                    var botchan = guild.VoiceChannels.First(b => b.Id == Settings.DCCountID);

                    if (botchan != null)
                    {
                        botchan.ModifyAsync(vc => vc.Name = ($"Players Online: {RAGE.Entities.Players.Count}"));
                    }
                }
				
                foreach (var guild in Discord._client.Guilds.Where(a => a.Id == Settings.DCServerID))
                {
                    var botchan = guild.VoiceChannels.First(b => b.Id == Settings.DCGamesID);

                    if (botchan != null)
                    {
                        botchan.ModifyAsync(vc => vc.Name = ($"Chases Running: {Settings.Lobbies.Where(l => l.gameStatus == 1).Count()}"));
                    }
                }

                //NAPI.Util.ConsoleOutput("Updated the DISCORD counts.");
            }
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

                            foreach (Player player in Settings.LoggedInPlayers)
                            {
                                if (player.GetSharedData<int>("InLobby") == lobby.lobbyId)
                                {
                                    player.TriggerEvent("SetTimeWeather", lobby.Time[0], lobby.Time[1], lobby.Time[2], lobby.Weather);

                                    player.SendChatMessage("!{#A8FFAB}[WEATHER FORECAST] " + weatherString);
                                }
                            }
                        }

                        foreach (Player player in Settings.LoggedInPlayers)
                        {
                            if (player.GetSharedData<int>("InLobby") == lobby.lobbyId)
                            {
                                player.TriggerEvent("SetTimeWeather", lobby.Time[0], lobby.Time[1], lobby.Time[2], lobby.Weather);
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
                                    if (player.GetSharedData<int>("InLobby") == lobby.lobbyId && player.GetData<bool>("AutoJoin") == true)
                                    {
                                        pCount++;
                                    }
                                }

                                if (pCount >= 2)
                                {
                                    lobby.gameStatus = 2;

                                    NAPI.Chat.SendChatMessageToAll($"!{{#FFFF00}}[Lobby {lobby.lobbyId}] !{{#FFFFFF}}Copchase will start in 10 seconds... ({pCount} players currently)");

                                    foreach (Player player in Settings.LoggedInPlayers)
                                    {
                                        if (player.GetSharedData<int>("InLobby") == lobby.lobbyId && player.GetData<bool>("AutoJoin") == true)
                                        {
                                            player.SetSharedData("InDM", -1);

                                            EPlayer.Spawn(player);
                                            NAPI.Notification.SendNotificationToPlayer(player, $"~w~Game is starting in ~g~10 seconds~w~.", true);
                                            
                                            NAPI.Util.ConsoleOutput($"[TimeUpdate] Game starting in 10 seconds in lobby {lobby.lobbyId}...");
                                        }
                                    }

                                    NAPI.Task.Run(() =>
                                    {
                                        GameStartCountdown(10, lobby.lobbyId);
                                    }, delayTime: 2000);
                                }
                                else
                                {
                                    if (Environment.TickCount - lobby.LastAutoStartMsg >= 90000)
                                    {
                                        foreach (Player player in Settings.LoggedInPlayers)
                                        {
                                            EPlayer.SendMessageInLobby(lobby.lobbyId, "!{#FFFF00}[GAME] !{#FFFFFF}Game will not start unless there are 2+ players to start a match.", true);
                                        }
                                        lobby.LastAutoStartMsg = Environment.TickCount;
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (Player player in Settings.LoggedInPlayers)
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

            NAPI.Task.Run(TimeUpdate, 15000);
            return;
        }

        public static void CheckDerbyWinner()
        {
            int derbyPlayers = Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == -4).Count();
            if (Settings.DerbyTimeLeft <= 0 && derbyPlayers >= 2)
            {
                Player winner = null;
                int lastBigTime = 0;
                int winXP = derbyPlayers / 2;

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
                        player.SendChatMessage("!{#E6DD3C}[DERBY] " + winner.Name + " won the derby and has gained " + winXP + "XP.");
                        Discord.SendTextChannel("staff-bot", $"**(Derby) | Winner: {winner.Name} | XP Gained: {winXP}**");
                    }
                }

                if (winner != null) EPlayer.GrantXP(winner, winXP);

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
                        NAPI.Chat.SendChatMessageToAll($"[Lobby {lobbyid}] !{{#FF0000}}The game has been stopped by an administrator.");
                        break;
                    case 1:
                        NAPI.Chat.SendChatMessageToAll($"[Lobby {lobbyid}] !{{#DAA520}}The fugitives managed to escape from the cops.");

                        foreach (Player i in lobby.Fugitives)
                        {
                            // Reward
                            EPlayer.GrantXP(i, 5);
                        }
                        break;
                    case 2:
                        NAPI.Chat.SendChatMessageToAll($"[Lobby {lobbyid}] !{{#DAA520}}All law enforcement units have been taken out by the fugitives.");

                        foreach (Player i in lobby.Fugitives)
                        {
                            // Reward
                            EPlayer.GrantXP(i, 3);
                        }
                        break;
                    case 3:
                        NAPI.Chat.SendChatMessageToAll($"[Lobby {lobbyid}] !{{#DAA520}}The situation has been successfully handled by the police.");

                        foreach (Player i in lobby.Cops)
                        {
                            // Reward
                            EPlayer.GrantXP(i, 4);
                        }
                        break;
                }

                lobby.gameStatus = 0;

                foreach (Player i in Settings.LoggedInPlayers)
                {
                    if (i.GetSharedData<int>("InLobby") != lobby.lobbyId) continue;

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
                    if (player.GetSharedData<int>("InLobby") == lobbyid && player.GetData<bool>("AutoJoin") == true)
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
                    crashcheck++;

                    NAPI.Util.ConsoleOutput($"[Lobby " + lobbyid + $"] Selected Map [{map.Name}] | Total Spawns: [{ContextFactory.Instance.MapSpawns.AsQueryable().Where(a => a.mapID == map.SQL_ID).Count()}]");
                    crashcheck++;

                    if (ContextFactory.Instance.MapSpawns.AsQueryable().Where(a => a.mapID == map.SQL_ID && a.spawnTeam == 0).Count() < 1) { NAPI.Util.ConsoleOutput($"[Lobby {lobbyid}] Map {map.Name} has no fugitive spawns! Abort."); return false; }
                    if (ContextFactory.Instance.MapSpawns.AsQueryable().Where(a => a.mapID == map.SQL_ID && a.spawnTeam == 1).Count() < 1) { NAPI.Util.ConsoleOutput($"[Lobby {lobbyid}] Map {map.Name} has no cop spawns! Abort."); return false; }

                    crashcheck++;

                    List<MapSpawn> Spawns = ContextFactory.Instance.MapSpawns.AsQueryable().Where(x => x.mapID == map.SQL_ID).ToList();
                    crashcheck++;

                    List<int> UsedSpawns = new List<int>(Spawns.Count + 1);
                    crashcheck++;

                    List<Player> lobbyPlayers = Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == lobbyid).ToList();
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
                        _tempPlayer = lobbyPlayers[random.Next(lobbyPlayers.Count)];

                        if (_tempPlayer.GetData<bool>("AutoJoin") == false) continue;

                        if (!lobby.Fugitives.Contains(_tempPlayer))
                        {
                            fugitives--;
                            lobby.Fugitives.Add(_tempPlayer);
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

                        if (!lobby.Fugitives.Contains(player))
                        {
                            player.ResetData("CuffingTarget");
                            lobby.Cops.Add(player);
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
                            int col1 = 0;
                            int overridemaxseats = 0;

                            switch (random.Next(0, 5))
                            {
                                case 0:
                                    carName = "police";
                                    col1 = 111;
                                    break;
                                case 1:
                                    carName = "polbufac2";
                                    overridemaxseats = 4;
                                    col1 = 8;
                                    break;
                                case 2:
                                    carName = "police3";
                                    col1 = 111;
                                    break;
                                case 3:
                                    carName = "polbufac";
                                    overridemaxseats = 4;
                                    col1 = 111;
                                    break;
                                case 4:
                                    carName = "fbibufac";
                                    overridemaxseats = 4;
                                    break;
                            }


                            Vehicle pVehicle = NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(carName), vecSpawn, spawnLoc.spawnA, col1, 111, player.Name, 255, false, true, Convert.ToUInt32(lobbyid));

                            pVehicle.SetSharedData("Team", 1);

                            player.SetData("putVehicle", pVehicle);
                            player.SetData("putVehicleSeat", 0);

                            lobby.Vehicles.Add(pVehicle);

                            NAPI.Task.Run(() =>
                            {
                                pVehicle.Position = vecSpawn;
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
                                        mem.SetData("putVehicleSeat", 0 + seat);

                                        playerVehicle.Add(new Tuple<Player, Vehicle, int>(mem, pVehicle, 0 + seat));
                                        PutInsideCar.Add(mem);
                                        mem.SendChatMessage("!{#FFAC38}[Partners] You have been put inside " + player.Name + "'s vehicle.");

                                        seat++;
                                    }
                                }
                            }
                        }

                        EPlayer.Freeze(player, true);

                        player.SetData("GPS", false);

                        EPlayer.SetDiscordStatus(player, $"[Lobby {lobbyid}] Playing cop as {player.Name}");

                        player.TriggerEvent("SetActorInvulnerable", false);
                        player.TriggerEvent("CanFallOutOfCar", true);
                        player.TriggerEvent("showHPBars", 1); // Always show HP Bars.
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

                        EPlayer.Freeze(player, true);

                        player.Health = 100;
                        player.Armor = 0;
                        EPlayer.SetColor(player, new Color(232, 64, 64, 255), player.Armor);

                        EPlayer.SetDiscordStatus(player, $"[Lobby {lobbyid}] Playing fugitive as {player.Name}");

                        player.TriggerEvent("SetActorInvulnerable", false);
                        player.TriggerEvent("CanFallOutOfCar", true);
                        player.TriggerEvent("showHPBars", 1); // Always show HP Bars.

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
                        int overridemaxseats = 0; // For DLC cars that aren't registered in vehicledata.

                        switch (random.Next(0, 5))
                        {
                            case 0:
                                carName = "washington";
                                break;
                            case 1:
                                carName = "600sel";
                                overridemaxseats = 4;
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

                        Vehicle pVehicle = NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(carName), vecSpawn, spawnLoc.spawnA, random.Next(122), random.Next(122), $"SA-{random.Next(1000, 9999)}", 255, false, true, Convert.ToUInt32(lobbyid));
                        pVehicle.SetSharedData("Team", 0);
                        lobby.Vehicles.Add(pVehicle);

                        NAPI.Task.Run(() =>
                        {
                            pVehicle.Position = vecSpawn;
                        }, 1000);

                        EPlayer.Freeze(_tempPlayer, true);

                        playerVehicle.Add(new Tuple<Player, Vehicle, int>(_tempPlayer, pVehicle, 0));
                        _tempPlayer.SetData("putVehicle", pVehicle);
                        _tempPlayer.SetData("putVehicleSeat", 0);

                        int seat = 1;
                        int maxseats = pVehicle.MaxOccupants;
                        if (overridemaxseats != 0) maxseats = overridemaxseats;
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

                            EPlayer.Freeze(_tempPlayer2, true);
                            playerVehicle.Add(new Tuple<Player, Vehicle, int>(_tempPlayer2, pVehicle, 0 + seat));

                            seat++;
                        }
                    }

                    NAPI.Task.Run(() =>
                    {
                        foreach (Player player in lobby.Cops)
                        {
                            player.Dimension = Convert.ToUInt32(lobbyid);
                        }
                        foreach (Player player in lobby.Fugitives)
                        {
                            player.Dimension = Convert.ToUInt32(lobbyid);
                        }

                    }, 1000);

                    NAPI.Task.Run(() =>
                    {
                        spawnLoc = new MapSpawn();

                        spawnLoc = Spawns.First(a => a.spawnTeam == 1);
                        foreach (Player player in lobby.Cops)
                        {
                            player.Position = new Vector3(spawnLoc.spawnX, spawnLoc.spawnY, spawnLoc.spawnZ).Around(5f);

                            try
                            {
                                if (player.HasData("putVehicle")) player.SetIntoVehicle(player.GetData<Vehicle>("putVehicle"), player.GetData<int>("putVehicleSeat"));
                            }
                            catch { }
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

                            try
                            {
                                if (data.Item1.HasData("putVehicle")) data.Item1.SetIntoVehicle(data.Item1.GetData<Vehicle>("putVehicle"), data.Item1.GetData<int>("putVehicleSeat"));
                            }
                            catch { }

                            data.Item1.SetIntoVehicle(data.Item2, data.Item3);
                            EPlayer.Freeze(data.Item1, false);

                            data.Item1.TriggerEvent("ToggleChaseCollisions", false);
                            data.Item1.SetSharedData("HasShot", false);

                            data.Item1.SendNotification($"~g~Weapons will be given after collisions enable.");
                        }

                        NAPI.Chat.SendChatMessageToAll($"[Lobby {lobbyid}] Copchase started with !{{#8D8DFF}}{lobby.Cops.Count} cops !{{#FFFFFF}}& !{{#E84040}}{lobby.Fugitives.Count} fugitives !{{#FFFFFF}}on map !{{#DAA520}}{map.Name}!{{#FFFFFF}}.");

                        lobby.FugitiveShowCounter = 60;
                        lobby.LastSeenCounter = 0;
                        lobby.GameProgress = 0;
                        lobby.CollisionCounter = 30;

                        foreach (Player p in Settings.LoggedInPlayers)
                        {
                            p.SetSharedData("Ping", p.Ping);

                            if (p.GetSharedData<int>("InLobby") == lobbyid)
                            {
                                p.TriggerEvent("StartChaseTimer", 0, Settings.GameRunTime, lobby.CollisionCounter);
                            }
                        }
                    }, delayTime: 2000);

                    NAPI.Task.Run(() =>
                    {
                        foreach (Player p in lobby.Cops)
                        {
                            p.TriggerEvent("ToggleChaseCollisions", false);
                        }
                        foreach (Player p in lobby.Fugitives)
                        {
                            p.TriggerEvent("ToggleChaseCollisions", false);
                        }

                    }, delayTime: 3000);

                    NAPI.Task.Run(() =>
                    {
                        foreach (Player player in lobby.Fugitives)
                        {
                            try
                            {
                                player.TriggerEvent("ToggleChaseCollisions", false);
                                if (player.HasData("putVehicle")) player.SetIntoVehicle(player.GetData<Vehicle>("putVehicle"), player.GetData<int>("putVehicleSeat"));
                            }
                            catch { }
                        }

                        foreach (Player player in lobby.Cops)
                        {
                            try
                            {
                                player.TriggerEvent("ToggleChaseCollisions", false);
                                if (player.HasData("putVehicle")) player.SetIntoVehicle(player.GetData<Vehicle>("putVehicle"), player.GetData<int>("putVehicleSeat"));
                            }
                            catch { }
                        }

                    }, delayTime: 3000);

                    NAPI.Task.Run(() =>
                    {
                        foreach (Player player in lobby.Fugitives)
                        {
                            try
                            {
                                player.TriggerEvent("ToggleChaseCollisions", false);
                                if (player.HasData("putVehicle")) player.SetIntoVehicle(player.GetData<Vehicle>("putVehicle"), player.GetData<int>("putVehicleSeat"));
                            }
                            catch { }
                        }

                        foreach (Player player in lobby.Cops)
                        {
                            try
                            {
                                player.TriggerEvent("ToggleChaseCollisions", false);
                                if (player.HasData("putVehicle")) player.SetIntoVehicle(player.GetData<Vehicle>("putVehicle"), player.GetData<int>("putVehicleSeat"));
                            }
                            catch { }
                        }

                    }, delayTime: 5000);


                    lobby.gameStatus = 1;
                    NAPI.Util.ConsoleOutput($"[Lobby {lobbyid}] Game has been started with {lobby.Cops.Count} cops & {lobby.Fugitives.Count} fugitives on map {map.Name}.");
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
            foreach(Player pl in Settings.LoggedInPlayers)
            {
                EPlayer.UpdateBlip(pl);
            }

            if (Environment.TickCount - Settings.LastDerbyTick >= 1000 && Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == -4).Count() >= 2)
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
                    foreach (Player fugitive in lobby.Fugitives)
                    {
                        if (fugitive.GetSharedData<bool>("HasShot") == false)
                        {
                            fugitive.SetSharedData("HasShot", true);
                            fugitive.SetSharedData("VisibleBlip", true);
                            lobby.FugitiveShowCounter = 29; // Set them to VISIBLE

                            fugitive.SendChatMessage("!{#FF6347}You have the has shot tag now because the game has progress more than 60%!");
                        }
                    }
                    if (lobby.GameProgress == Convert.ToInt32(0.6 * Settings.GameRunTime))
                    {
                        EPlayer.SendMessageInLobby(lobby.lobbyId, "!{#FF6347}[GAME] !{#FFFFFF}60% of the game has elapsed, all fugitives have shot tag now.");
                    }
                }

                if (lobby.GameProgress >= Settings.GameRunTime)
                {
                    StopGame(lobby.lobbyId, 1);
                }

                lobby.FugitiveShowCounter++;
                lobby.LastSeenCounter++;

                if(lobby.LastSeenCounter >= 20)
                {
                    if(lobby.Fugitives.Count() > 0)
                    {
                        lobby.Fugitives[new Random().Next(lobby.Fugitives.Count)].TriggerEvent("SendLastSeen");
                    }
                    lobby.LastSeenCounter = 0;
                }

                if (lobby.CollisionCounter > 0)
                {
                    lobby.CollisionCounter--;

                    if (lobby.CollisionCounter < 1)
                    {
                        foreach (Player player in Settings.LoggedInPlayers.Where(p => p.GetSharedData<int>("InLobby") == lobby.lobbyId))
                        {
                            if (!lobby.Cops.Contains(player) && !lobby.Fugitives.Contains(player)) continue;

                            player.TriggerEvent("ToggleChaseCollisions", true);
                            NAPI.Notification.SendNotificationToPlayer(player, $"~r~Collisions enabled and weapons deployed!", true);

                            if (lobby.Cops.Contains(player))
                            {
                                player.GiveWeapon(WeaponHash.Carbinerifle, 120);
                                player.GiveWeapon(WeaponHash.Pumpshotgun, 20);
                                player.GiveWeapon(WeaponHash.Pistol, 100);
                                player.GiveWeapon(WeaponHash.Stungun, 1);
                                player.SendChatMessage("!{#FF6347}[TASER] !{#FFFFFF}You have been equipped with a stungun, shoot at a fugitive that hasn't shot to tase them.");
                            }
                            else if (lobby.Fugitives.Contains(player))
                            {
                                player.GiveWeapon(WeaponHash.Assaultrifle, 120);
                                player.GiveWeapon(WeaponHash.Pistol, 100);
                                player.GiveWeapon(WeaponHash.Pumpshotgun, 20);
                            }
                        }
                    }
                }

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

                if (lobby.FugitiveShowCounter >= 30 && lobby.FugitiveShowCounter < 120)
                {
                    foreach (Player fugi in lobby.Fugitives)
                    {
                        fugi.SetSharedData("VisibleBlip", true);

                        if (lobby.FugitiveShowCounter == 30) fugi.SendNotification("~r~You are now visible on the radar.", true);
                    }
                }

                if (lobby.FugitiveShowCounter >= 120)
                {
                    lobby.FugitiveShowCounter = 0;
                    foreach (Player fugi in lobby.Fugitives)
                    {
                        fugi.SetSharedData("VisibleBlip", false);

                        fugi.SendNotification("~g~You are no longer visible on the radar.", true);
                    }
                }
            }

            NAPI.Task.Run(GameUpdate, 1000);

            return;
        }
    }
}