using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Resources;
using GTANetworkAPI;
using System.Linq;
using LSCC;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using LSCC.Events;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

public static class Settings
{
    // Static Settings / Const
    public const string Version = "0.1a";
    public const int GameRunTime = 600; // Game Running Time in Seconds before STOP

    // MySQL Settings
    public const string MySQL_Host = "localhost";
    public const string MySQL_User = "root";
    public const string MySQL_Password = "";
    public const string MySQL_Database = "lsccv";

    // Global Variables (changing accordingly)
    public static int GameRunning { get; set; }
    public static int FugitiveShowCounter { get; set; }
    public static int CollisionCounter { get; set; }
    public static int GameProgress { get; set; }
    public static bool GameAutoStart { get; set; }
    public static int LastAutoStartMsg { get; set; }
    public static List<Client> LoggedInPlayers { get; set; }
    public static List<Client> Cops { get; set; }
    public static List<Client> Fugitives { get; set; }
    public static List<Vehicle> GameVehicles { get; set; }
    public static List<Vehicle> AdminVehicles { get; set; }

    // Locations
    public const float LoginCameraX = -550;
    public const float LoginCameraY = -210;
    public const float LoginCameraZ = 90;
    public const float LoginCameraRx = -30;
    public const float LoginCameraRy = 0;
    public const float LoginCameraRz = -30;
    public const float LoginCameraViewAngle = 60;

    public static Vector3[] LobbySpawns = new Vector3[]
    {
        new Vector3(-592f, -98f, 43f)
    };
}

namespace LSCC
{
    public class Main : Script
    {
        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
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

            Settings.GameRunning = 0;
            Settings.LoggedInPlayers = new List<Client>();
            Settings.Cops = new List<Client>();
            Settings.Fugitives = new List<Client>();
            Settings.GameVehicles = new List<Vehicle>();
            Settings.AdminVehicles = new List<Vehicle>();
            Settings.FugitiveShowCounter = 0;
            Settings.CollisionCounter = 0;
            Settings.GameProgress = 0;
            Settings.GameAutoStart = true;
            Settings.LastAutoStartMsg = 0;

            NAPI.Task.Run(() =>
            {
                TimeUpdate();
            }, delayTime: 15000);
        }

        public void GameStartCountdown(int time)
        {
            time -= 2;

            foreach (Client player in Settings.LoggedInPlayers)
            {
                if (player.GetData("AutoJoin") == true)
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
                    GameStartCountdown(time);
                }, delayTime: 2000);
            }
            else
            {
                StartGame();
            }
        }

        public void TimeUpdate()
        {
            if (Settings.GameRunning == 0)
            {
                if (Settings.GameAutoStart)
                {
                    int pCount = 0;
                    foreach (Client player in Settings.LoggedInPlayers)
                    {
                        if (player.GetData("AutoJoin") == true)
                        {
                            pCount++;
                        }
                    }

                    if (pCount >= 2)
                    {
                        Settings.GameRunning = 2;

                        foreach (Client player in Settings.LoggedInPlayers)
                        {
                            if (player.GetData("AutoJoin") == true)
                            {
                                Player.Spawn(player);
                                NAPI.Notification.SendNotificationToPlayer(player, $"~w~Game is starting in ~g~10 seconds~w~.", true);

                                NAPI.Util.ConsoleOutput("[TimeUpdate] Game starting in 10 seconds...");
                            }
                        }

                        NAPI.Task.Run(() =>
                        {
                            GameStartCountdown(10);
                        }, delayTime: 2000);
                    }
                    else
                    {
                        if (Environment.TickCount - Settings.LastAutoStartMsg >= 90000)
                        {
                            NAPI.Chat.SendChatMessageToAll("!{#FFFF00}[GAME] !{#FFFFFF}Game will not start unless there are 2+ players to start a match.");
                            Settings.LastAutoStartMsg = Environment.TickCount;
                        }
                    }
                }
            }
            else if(Settings.GameRunning == 1)
            {
                /*if(Settings.GameProgress >= Settings.GameRunTime)
                {
                    StopGame(1);
                }  -- Already used in GameProgress() update.*/

                if(Settings.Cops.Count < 1)
                {
                    StopGame(2);
                }
                else if(Settings.Fugitives.Count < 1)
                {
                    StopGame(3);
                }
            }

            NAPI.Task.Run(() =>
            {
                TimeUpdate();
            }, delayTime: 15000);
        }

        public static bool StopGame(int reason)
        {
            if (Settings.GameRunning != 1) return false;

            switch(reason)
            {
                case 0:
                    NAPI.Chat.SendChatMessageToAll("[GAME] !{#FF0000}The game has been stopped by an administrator.");
                    break;
                case 1:
                    NAPI.Chat.SendChatMessageToAll("[GAME] !{#DAA520}The fugitives managed to escape from the cops.");
                    
                    foreach (Client i in Settings.Fugitives)
                    {
                        // Reward
                    }
                    break;
                case 2:
                    NAPI.Chat.SendChatMessageToAll("[GAME] !{#DAA520}All law enforcement units have been taken out by the fugitives.");

                    foreach (Client i in Settings.Fugitives)
                    {
                        // Reward
                    }
                    break;
                case 3:
                    NAPI.Chat.SendChatMessageToAll("[GAME] !{#DAA520}The situation has been successfully handled by the police.");

                    foreach (Client i in Settings.Cops) 
                    {
                        // Reward
                    }
                    break;
            }

            Settings.GameRunning = 0;

            foreach(Client i in Settings.LoggedInPlayers)
            {
                i.TriggerEvent("HideChaseTimer");
                if(Settings.Cops.Contains(i))
                {
                    Settings.Cops.Remove(i);
                    Player.Spawn(i);
                }
                if (Settings.Fugitives.Contains(i))
                {
                    Settings.Fugitives.Remove(i);
                    Player.Spawn(i);
                }
            }

            for (int i = Settings.GameVehicles.Count - 1; i >= 0; i--)
            {
                if(!Settings.GameVehicles[i].IsNull && Settings.GameVehicles[i] != null)
                {
                    Settings.GameVehicles[i].Delete();
                    Settings.GameVehicles.RemoveAt(i);
                }
            }

            Settings.Cops.Clear();
            Settings.Fugitives.Clear();
            Settings.GameVehicles.Clear();

            return true;
        }

        public static bool StartGame()
        {
            if (Settings.GameRunning == 1) { NAPI.Util.ConsoleOutput("[StartGame] Copchase is already running, failed to start"); return false; }

            Settings.GameRunning = 0;  
            int pCount = 0;

            Settings.Cops.Clear();
            Settings.Fugitives.Clear();

            foreach (Client player in Settings.LoggedInPlayers)
            {
                if (player.GetData("AutoJoin") == true)
                {
                    pCount++;
                }
            }

            if (pCount < 2) { NAPI.Util.ConsoleOutput("[StartGame] Total players less than two, failed to start"); return false; }

            double perc = (double)pCount * ((double)30 / (double)100);

            int fugitives = (int)Math.Round(perc);

            if (fugitives < 1) { NAPI.Util.ConsoleOutput("[StartGame] Fugitives count is zero, failed to start"); return false; }

            if(ContextFactory.Instance.Maps.Count() < 1) { NAPI.Util.ConsoleOutput("[StartGame] Server has no maps added, failed to start"); return false; }

            List<Map> mapList = ContextFactory.Instance.Maps.Where(a => a.Enabled == true).ToList();

            if(mapList.Count < 1) { NAPI.Util.ConsoleOutput("[StartGame] Server has maps but none of them were enabled! Failed to start."); return false; }

            Map map = mapList.OrderBy(c => Guid.NewGuid()).FirstOrDefault();

            NAPI.Util.ConsoleOutput($"[StartGame] Selected Map [{map.Name}] | Total Spawns: [{ContextFactory.Instance.MapSpawns.Where(a => a.mapID == map.SQL_ID).Count()}]");


            if (ContextFactory.Instance.MapSpawns.Where(a => a.mapID == map.SQL_ID && a.spawnTeam == 0).Count() < 1) { NAPI.Util.ConsoleOutput($"[StartGame] Map {map.Name} has no fugitive spawns! Abort."); return false; }
            if (ContextFactory.Instance.MapSpawns.Where(a => a.mapID == map.SQL_ID && a.spawnTeam == 1).Count() < 1) { NAPI.Util.ConsoleOutput($"[StartGame] Map {map.Name} has no cop spawns! Abort."); return false; }


            List<MapSpawn> Spawns = ContextFactory.Instance.MapSpawns.Where(x => x.mapID == map.SQL_ID).ToList();
            List<int> UsedSpawns = new List<int>();
            
            Random random = new Random();

            pCount = 0;
            Client _tempPlayer;
            
            while (fugitives > 0)
            {
                _tempPlayer = Settings.LoggedInPlayers[random.Next(Settings.LoggedInPlayers.Count)];

                if (!Settings.Fugitives.Contains(_tempPlayer))
                {
                    fugitives--;
                    Settings.Fugitives.Add(_tempPlayer);
                }
                else pCount++;

                if(pCount >= 100)
                {
                    NAPI.Util.ConsoleOutput("[StartGame] Stuck inside random iteration loop for selecting fugitive, aborting method.");
                    return false;
                }
            }

            foreach (Client player in Settings.LoggedInPlayers)
            {
                if (!Settings.Fugitives.Contains(player))
                {
                    Settings.Cops.Add(player);
                }
            }

            MapSpawn spawnLoc = null;
            pCount = 0;
            foreach(Client player in Settings.Cops)
            {
                foreach(MapSpawn spawn in Spawns.Where(a=> a.spawnTeam == 1))
                {
                    if (!UsedSpawns.Contains(spawn.SQL_ID))
                    {
                        spawnLoc = spawn;
                        UsedSpawns.Add(spawn.SQL_ID);
                    }
                }

                if (spawnLoc == null)
                {
                    pCount++;
                    spawnLoc = Spawns.FirstOrDefault(a=> a.spawnTeam == 1);
                    spawnLoc.spawnZ += (pCount * 5); // If no spawns left, stack cars ontop so at least they don't stuck in each other.
                }

                Vector3 vecSpawn = new Vector3(spawnLoc.spawnX, spawnLoc.spawnY, spawnLoc.spawnZ);

                string carName = "police";

                switch (random.Next(0, 5))
                {
                    case 0:
                        carName = "police";
                        break;
                    case 1:
                        carName = "police4";
                        break;
                    case 2:
                        carName = "police3";
                        break;
                    case 3:
                        carName = "police2";
                        break;
                    case 4:
                        carName = "fbi";
                        break;
                    case 5:
                        carName = "sheriff";
                        break;
                }


                Vehicle pVehicle = NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(carName), vecSpawn, spawnLoc.spawnA, 0, 1, player.Name, 255, false, true, 0);


                Settings.GameVehicles.Add(pVehicle);

                player.Dimension = 0;
                player.Position = vecSpawn.Around(15);
                Player.Freeze(player, true);
                
                player.Health = 100;
                player.Armor = 60;
                Player.SetColor(player, new Color(141, 141, 255, 255), player.Armor);

                switch (random.Next(0, 3))
                {
                    case 0:
                        player.SetSkin(0x5E3DA4A4);
                        break;
                    case 1:
                        player.SetSkin(0xFBF98469);
                        break;
                    case 2:
                        player.SetSkin(0x9AB35F63);
                        break;
                    case 3:
                        player.SetSkin(0x1AE8BB58);
                        break;
                }


                NAPI.Task.Run(() =>
                {
                    player.SetIntoVehicle(pVehicle, -1);
                    Player.Freeze(player, false);

                    player.TriggerEvent("ToggleChaseCollisions", false);

                }, delayTime: 2000);


                player.GiveWeapon(WeaponHash.CarbineRifle, 100);

                Player.SetDiscordStatus(player, $"Playing cop as {player.Name}");

                player.TriggerEvent("SetActorInvulnerable", false);
                player.TriggerEvent("CanFallOutOfCar", true);
                player.TriggerEvent("showHPBars", 1); // Always show HP Bars.
            }

            spawnLoc = null;
            foreach (Client player in Settings.Fugitives)
            {
                foreach (MapSpawn spawn in Spawns.Where(a => a.spawnTeam == 0))
                {
                    if (!UsedSpawns.Contains(spawn.SQL_ID))
                    {
                        spawnLoc = spawn;
                        UsedSpawns.Add(spawn.SQL_ID);
                    }
                }

                if (spawnLoc == null)
                {
                    pCount++;
                    spawnLoc = Spawns.FirstOrDefault(a => a.spawnTeam == 0);
                    spawnLoc.spawnZ += (pCount * 5); // If no spawns left, stack cars ontop so at least they don't stuck in each other.
                }
                Vector3 vecSpawn = new Vector3(spawnLoc.spawnX, spawnLoc.spawnY, spawnLoc.spawnZ);

                string carName = "sultan";

                switch (random.Next(0, 5))
                {
                    case 0:
                        carName = "sultan";
                        break;
                    case 1:
                        carName = "fugitive";
                        break;
                    case 2:
                        carName = "ingot";
                        break;
                    case 3:
                        carName = "­dilettante";
                        break;
                    case 4:
                        carName = "oracle";
                        break;
                    case 5:
                        carName = "oracle2";
                        break;
                }

                Vehicle pVehicle = NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(carName), vecSpawn, spawnLoc.spawnA, random.Next(64), random.Next(64), player.Name, 255, false, true, 0);


                Settings.GameVehicles.Add(pVehicle);

                player.Dimension = 0;
                player.Position = vecSpawn.Around(15);
                Player.Freeze(player, true);

                player.Health = 100;
                player.Armor = 0;
                Player.SetColor(player, new Color(232, 64, 64, 255), player.Armor);

                switch (random.Next(0, 3))
                {
                    case 0:
                        player.SetSkin(0xA70B4A92);
                        break;
                    case 1:
                        player.SetSkin(0x76284640);
                        break;
                    case 2:
                        player.SetSkin(0x3FB5C3D3);
                        break;
                    case 3:
                        player.SetSkin(0x04430687);
                        break;
                }

                NAPI.Task.Run(() =>
                {
                    player.SetIntoVehicle(pVehicle, -1);
                    Player.Freeze(player, false);

                    player.TriggerEvent("ToggleChaseCollisions", false);

                }, delayTime: 2000);


                player.GiveWeapon(WeaponHash.AssaultRifle, 100);


                if (player.HasData("Blip"))
                {
                    Blip tBlip = player.GetData("Blip");
                    tBlip.Delete();
                    player.ResetData("Blip");
                }

                Blip pBlip = NAPI.Blip.CreateBlip(new Vector3(spawnLoc.spawnX, spawnLoc.spawnY, spawnLoc.spawnZ), 0);
                pBlip.Transparency = 255;
                pBlip.Color = 75;

                player.SetData("Blip", pBlip);

                Player.SetDiscordStatus(player, $"Playing fugitive as {player.Name}");

                player.TriggerEvent("SetActorInvulnerable", false);
                player.TriggerEvent("CanFallOutOfCar", true);
                player.TriggerEvent("showHPBars", 1); // Always show HP Bars.
            }

            NAPI.Util.ConsoleOutput($"[StartGame] Game has been started with {Settings.Cops.Count} cops & {Settings.Fugitives.Count} fugitives on map {map.Name}.");
            NAPI.Chat.SendChatMessageToAll($"[GAME] Copchase started with !{{#8D8DFF}}{Settings.Cops.Count} cops !{{#FFFFFF}}& !{{#E84040}}{Settings.Fugitives.Count} fugitives !{{#FFFFFF}}on map !{{#DAA520}}{map.Name}!{{#FFFFFF}}.");

            Settings.GameRunning = 1;
            Settings.FugitiveShowCounter = 60;
            Settings.GameProgress = 0;
            Settings.CollisionCounter = 30;

            foreach(Client p in Settings.LoggedInPlayers)
            {
                p.TriggerEvent("StartChaseTimer", 0, Settings.GameRunTime, Settings.CollisionCounter);
            }
            
            NAPI.Task.Run(() =>
            {
                GameUpdate();
            }, delayTime: 1000);

            return true;
        }

        public static void GameUpdate()
        {
            if(Settings.GameRunning == 1)
            {
                Settings.GameProgress++;

                if (Settings.GameProgress >= Settings.GameRunTime)
                {
                    StopGame(1);
                    return;
                }

                Settings.FugitiveShowCounter++;

                if (Settings.CollisionCounter > 0)
                {
                    Settings.CollisionCounter--;

                    foreach (Client player in Settings.LoggedInPlayers)
                    {
                        if (!Settings.Cops.Contains(player) && !Settings.Fugitives.Contains(player)) continue;

                        if (Settings.CollisionCounter < 1)
                        {
                            player.TriggerEvent("ToggleChaseCollisions", true);
                            NAPI.Notification.SendNotificationToPlayer(player, $"~r~Collisions enabled!", true);
                        }
                    }
                }

                if (Settings.FugitiveShowCounter >= 60 && Settings.FugitiveShowCounter < 90)
                {
                    foreach (Client fugi in Settings.Fugitives)
                    {
                        if (fugi.HasData("Blip"))
                        {
                            Blip tBlip = fugi.GetData("Blip");
                            tBlip.Delete();
                            fugi.ResetData("Blip");
                        }

                        Blip pBlip = NAPI.Blip.CreateBlip(fugi.Position, 0);

                        pBlip.Transparency = 255;
                        pBlip.Color = 75;

                        fugi.SetData("Blip", pBlip);

                        if(Settings.FugitiveShowCounter == 60) fugi.SendNotification("~r~You are now visible on the radar.", true);
                    }
                }
                if (Settings.FugitiveShowCounter >= 90)
                {
                    Settings.FugitiveShowCounter = 0;
                    foreach (Client fugi in Settings.Fugitives)
                    {
                        if (fugi.HasData("Blip"))
                        {
                            Blip tBlip = fugi.GetData("Blip");
                            tBlip.Delete();
                            fugi.ResetData("Blip");
                        }

                        fugi.SendNotification("~g~You are no longer visible on the radar.", true);
                    }
                }

                NAPI.Task.Run(() =>
                {
                    GameUpdate();
                }, delayTime: 1000);
            }
            return;
        }

        public void OnPlayerLogin(Client player, Account acc)
        {
            Player.SetDiscordStatus(player, $"Logging in as {acc.Username}");
            NAPI.Util.ConsoleOutput($"[Login] {player.Name} has logged in as {acc.Username}. (IP: {player.Address} - Serial: {player.Serial} - SC: {player.SocialClubName})");

            player.SetData("LoggedIn", true);
            player.SetData("AccountHandle", acc);

            player.Name = acc.Username;
            player.TriggerEvent("destroyBrowser");
            
            Player.Spawn(player);
        }
        public void OnPlayerRegister(Client player, Account acc)
        {
            Player.SetDiscordStatus(player, $"Registering as {acc.Username}");
            NAPI.Util.ConsoleOutput($"[Connection] {player.Name} has registered as {acc.Username}. (IP: {player.Address} - Serial: {player.Serial} - SC: {player.SocialClubName})");

            player.SetData("LoggedIn", true);
            player.SetData("AccountHandle", acc);

            player.Name = acc.Username;
            player.TriggerEvent("destroyBrowser");

            Player.Spawn(player);
        }

        [RemoteEvent("OnPlayerLoginAttempt")]
        public void OnPlayerLoginAttempt(Client player, string username, string password)
        {
            NAPI.Util.ConsoleOutput($"[Login Attempt] Username {username}");
            if (Environment.TickCount - player.GetData("LastCryptAttempt") < 1000)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~Please wait some time between each register attempt!", true);
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Please wait some time between each register attempt!");
                return;
            }
            player.SetData("LastCryptAttempt", Environment.TickCount);
            if (username.Contains("@"))
            {
                Account acc = ContextFactory.Instance.Accounts.FirstOrDefault(a => a.Email == username);
                if (acc != null)
                {
                    if(AccountManager.Login(acc, password))
                    {
                        OnPlayerLogin(player, acc);
                        return;
                    }
                }
            }
            else
            {
                Account acc = ContextFactory.Instance.Accounts.FirstOrDefault(a => a.Username == username);
                if (acc != null)
                {
                    if (AccountManager.Login(acc, password))
                    {
                        OnPlayerLogin(player, acc);
                        return;
                    }
                }
            }
            NAPI.Notification.SendNotificationToPlayer(player, "~r~Invalid password/username!", true);

        }

        [RemoteEvent("OnPlayerRegisterAttempt")]
        public void OnPlayerRegisterAttempt(Client player, string email, string username, string password, bool allcorrect, bool rules)
        {
            NAPI.Util.ConsoleOutput($"[Register Attempt] Username {username}");
            if(Environment.TickCount - player.GetData("LastCryptAttempt") < 1000)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Please wait some time between each register attempt!");
                return;
            }
            if(!allcorrect || !rules)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You must agree to the rules and ensure all information is correct!");
                return;
            }
            if(password.Length < 5)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Your password is too small!");
                return;
            }
            if (username.Contains("@") || username.Length < 4)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Your username is too small or it cannot contain the character '@'");
                return;
            }
            Regex exp = new Regex(@"^[a-zA-Z0-9]*$");
            Match match = exp.Match(username);
            if (!match.Success)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Your username can only contain alphabets or numbers!");
                return;
            }
            if (!email.Contains("@") || email.Length < 5)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Your username cannot contain the character '@'");
                return;
            }
            Account acc = ContextFactory.Instance.Accounts.FirstOrDefault(a => a.Username == username);
            if (acc != null)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}An account with this name already exists!");
                return;
            }
            acc = ContextFactory.Instance.Accounts.FirstOrDefault(a => a.Email == email);
            if (acc != null)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}An account with this email already exists!");
                return;
            }
            player.SetData("LastCryptAttempt", Environment.TickCount);
            string salt = AccountManager.GenSalt();
            string newPass = AccountManager.Crypt(password, salt);
            var _new = new Account
            {
                Email = email,
                Password = newPass,
                Salt = salt,
                Username = username,
                Admin = 0,
                HWID = player.Serial
            };
            AccountManager.Add(_new);
            OnPlayerRegister(player, _new);
        }
    }
}