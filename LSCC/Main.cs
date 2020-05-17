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
    public const string Version = "0.1a";

    // MySQL Settings
    public const string MySQL_Host = "localhost";
    public const string MySQL_User = "lsccv";
    public const string MySQL_Password = "perkovicMatko3035";
    public const string MySQL_Database = "lsccv";

    // Global Variables
    public static int GameRunning { get; set; }
    public static int FugitiveShowCounter { get; set; }
    public static List<Client> LoggedInPlayers { get; set; }
    public static List<Client> Cops { get; set; }
    public static List<Client> Fugitives { get; set; }
    public static List<Vehicle> GameVehicles { get; set; }

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
        Random rand = new Random();

        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            NAPI.Server.SetAutoRespawnAfterDeath(false);
            NAPI.Server.SetAutoSpawnOnConnect(false);
            NAPI.Server.SetGlobalServerChat(false);
            NAPI.Server.SetCommandErrorMessage("~r~[Error] ~w~This command is invalid, refer to /help for a list.");

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
            Settings.FugitiveShowCounter = 0;

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
                        NAPI.Notification.SendNotificationToPlayer(player, $"~w~Game is starting in ~g~{time} seconds~w~.", true);
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
        fix blip(re-attach every 2-3 seconds), game stop system etc
        public void TimeUpdate()
        {
            if(Settings.GameRunning == 0)
            {
                int pCount = 0;
                foreach(Client player in Settings.LoggedInPlayers)
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
                    NAPI.Chat.SendChatMessageToAll("~y~[GAME] ~w~Game is not starting as there's not enough players to start a match. (2+)");
                    NAPI.Util.ConsoleOutput("[TimeUpdate] Failed to start game, not enough players.");
                }
            }

            if(Settings.GameRunning == 1)
            {
                Settings.FugitiveShowCounter++;

                if(Settings.FugitiveShowCounter >= 4)
                {
                    foreach(Client fugi in Settings.Fugitives)
                    {
                        Blip pBlip = fugi.GetData("Blip");
                        if (pBlip.IsNull) pBlip = NAPI.Blip.CreateBlip(new Vector3(0.0, 0.0, -200.0), 0);

                        pBlip.MovePosition(fugi.Position, 0);
                        pBlip.AttachTo(fugi, "spine", new Vector3(0.0, 0.0, 0.0), new Vector3(0.0, 0.0, 0.0));
                        NAPI.Blip.SetBlipColor(pBlip, new Color(255, 0, 0, 255).ToInt32());
                        NAPI.Blip.SetBlipTransparency(pBlip, 128);

                        fugi.SetData("Blip", pBlip);

                        fugi.SendNotification("~r~You are visible on the radar.", false);
                    }
                }
                if(Settings.FugitiveShowCounter >= 6)
                {
                    Settings.FugitiveShowCounter = 0;
                    foreach (Client fugi in Settings.Fugitives)
                    {
                        Blip pBlip = fugi.GetData("Blip");
                        if (pBlip.IsNull) pBlip = NAPI.Blip.CreateBlip(new Vector3(0.0, 0.0, -200.0), 0);

                        pBlip.MovePosition(fugi.Position, 0);
                        pBlip.AttachTo(fugi, "spine", new Vector3(0.0, 0.0, 0.0), new Vector3(0.0, 0.0, 0.0));
                        NAPI.Blip.SetBlipColor(pBlip, new Color(255, 0, 0, 255).ToInt32());
                        NAPI.Blip.SetBlipTransparency(pBlip, 0);

                        fugi.SetData("Blip", pBlip);

                        fugi.SendNotification("~g~You are now invisible on the radar.", true);
                    }
                }
            }


            NAPI.Task.Run(() =>
            {
                TimeUpdate();
            }, delayTime: 15000);
        }

        public void StartGame()
        {
            if (Settings.GameRunning == 1) { NAPI.Util.ConsoleOutput("[StartGame] Copchase is already running, failed to start"); return; }

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

            if (pCount < 2) { NAPI.Util.ConsoleOutput("[StartGame] Total players less than two, failed to start"); return; }

            double perc = (double)pCount * ((double)30 / (double)100);

            int fugitives = (int)Math.Round(perc);

            if (fugitives < 1) { NAPI.Util.ConsoleOutput("[StartGame] Fugitives count is zero, failed to start"); return; }

            if(ContextFactory.Instance.Maps.Count() < 1) { NAPI.Util.ConsoleOutput("[StartGame] Server has no maps added, failed to start"); return; }

            List<Map> mapList = ContextFactory.Instance.Maps.Where(a => a.Enabled == true).ToList();

            if(mapList.Count < 1) { NAPI.Util.ConsoleOutput("[StartGame] Server has maps but none of them were enabled! Failed to start."); return; }

            Map map = mapList.OrderBy(c => Guid.NewGuid()).FirstOrDefault();

            NAPI.Util.ConsoleOutput($"[StartGame] Selected Map [{map.Name}] | Total Spawns: [{ContextFactory.Instance.MapSpawns.Where(a => a.mapID == map.SQL_ID).Count()}]");


            if (ContextFactory.Instance.MapSpawns.Where(a => a.mapID == map.SQL_ID && a.spawnTeam == 0).Count() < 1) { NAPI.Util.ConsoleOutput($"[StartGame] Map {map.Name} has no fugitive spawns! Abort."); return; }
            if (ContextFactory.Instance.MapSpawns.Where(a => a.mapID == map.SQL_ID && a.spawnTeam == 1).Count() < 1) { NAPI.Util.ConsoleOutput($"[StartGame] Map {map.Name} has no cop spawns! Abort."); return; }


            List<MapSpawn> Spawns = ContextFactory.Instance.MapSpawns.Where(x => x.mapID == map.SQL_ID).ToList();
            List<int> UsedSpawns = new List<int>();


            pCount = 0;
            Client _tempPlayer;
            
            while (fugitives > 0)
            {
                _tempPlayer = Settings.LoggedInPlayers[rand.Next(Settings.LoggedInPlayers.Count)];

                if (!Settings.Fugitives.Contains(_tempPlayer))
                {
                    fugitives--;
                    Settings.Fugitives.Add(_tempPlayer);
                }
                else pCount++;

                if(pCount >= 100)
                {
                    NAPI.Util.ConsoleOutput("[StartGame] Stuck inside random iteration loop for selecting fugitive, aborting method.");
                    return;
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
                    spawnLoc = Spawns.Last(a=> a.spawnTeam == 1);
                    spawnLoc.spawnZ += (pCount * 5); // If no spawns left, stack cars ontop so at least they don't stuck in each other.
                }

                player.Dimension = 0;
                player.Health = 100;
                player.Armor = 100;
                NAPI.Player.SetPlayerNametagColor(player, 0, 0, 255);
                player.Collisionless = true;

                switch (rand.Next(0, 3))
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

                string carName = "police";

                switch(rand.Next(0, 5))
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

                player.MovePosition(new Vector3(spawnLoc.spawnX, spawnLoc.spawnY, spawnLoc.spawnZ).Around(15), 0);
                Vehicle pVehicle = NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(carName), new Vector3(spawnLoc.spawnX, spawnLoc.spawnY, spawnLoc.spawnZ), spawnLoc.spawnA, 0, 1, player.Name, 255, false, true, 0);
                pVehicle.Collisionless = true; 
                player.SetIntoVehicle(pVehicle, -1);

                NAPI.Notification.SendNotificationToPlayer(player, $"~w~Collisions enable in 10 seconds.", true);
            }

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
                    spawnLoc = Spawns.Last(a => a.spawnTeam == 0);
                    spawnLoc.spawnZ += (pCount * 5); // If no spawns left, stack cars ontop so at least they don't stuck in each other.
                }

                player.Dimension = 0;
                player.Health = 100;
                player.Armor = 100;
                player.Collisionless = true;
                NAPI.Player.SetPlayerNametagColor(player, 255, 0, 0);

                switch (rand.Next(0, 3))
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

                string carName = "sultan";

                switch (rand.Next(0, 5))
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

                player.MovePosition(new Vector3(spawnLoc.spawnX, spawnLoc.spawnY, spawnLoc.spawnZ).Around(15), 0);
                Vehicle pVehicle = NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(carName), new Vector3(spawnLoc.spawnX, spawnLoc.spawnY, spawnLoc.spawnZ), spawnLoc.spawnA, 0, 1, player.Name, 255, false, true, 0);
                pVehicle.Collisionless = true;
                player.SetIntoVehicle(pVehicle, -1);

                NAPI.Notification.SendNotificationToPlayer(player, $"~w~Collisions enable in 10 seconds.", true);


                Blip pBlip = player.GetData("Blip");
                if (pBlip.IsNull) pBlip = NAPI.Blip.CreateBlip(new Vector3(0.0, 0.0, -200.0), 0);

                pBlip.MovePosition(new Vector3(spawnLoc.spawnX, spawnLoc.spawnY, spawnLoc.spawnZ), 0);
                pBlip.AttachTo(player, "spine", new Vector3(0.0, 0.0, 0.0), new Vector3(0.0, 0.0, 0.0));
                NAPI.Blip.SetBlipColor(pBlip, new Color(255, 0, 0, 255).ToInt32());
                NAPI.Blip.SetBlipTransparency(pBlip, 128);

                player.SetData("Blip", pBlip);
            }

            NAPI.Util.ConsoleOutput($"[StartGame] Game has been started with {Settings.Cops.Count} cops & {Settings.Fugitives.Count} fugitives on map {map.Name}.");
            NAPI.Chat.SendChatMessageToAll($"~w~Game has been started with ~b~{Settings.Cops.Count} cops ~w~& ~r~{Settings.Fugitives.Count} fugitives ~w~on map ~y~{map.Name}~w~.");

            Settings.GameRunning = 1;
            Settings.FugitiveShowCounter = 4;

            NAPI.Task.Run(() =>
            {
                CollisionToggleHandler(10);
            }, delayTime: 2000);

            return;
        }

        public void CollisionToggleHandler(int time)
        {
            time -= 2;

            foreach (Client player in Settings.Cops)
            {
                if (time > 0)
                {
                    NAPI.Notification.SendNotificationToPlayer(player, $"~w~Collisions enable in {time} seconds.", true);
                }
                else
                {
                    NAPI.Notification.SendNotificationToPlayer(player, $"~r~Collisions enabled!", true);
                    player.Collisionless = false;
                }
            }

            foreach (Client player in Settings.Fugitives)
            {
                if (time > 0)
                {
                    NAPI.Notification.SendNotificationToPlayer(player, $"~w~Collisions enable in {time} seconds.", true);
                }
                else
                {
                    NAPI.Notification.SendNotificationToPlayer(player, $"~r~Collisions enabled!", true);
                    player.Collisionless = false;
                }
            }

            if (time > 0)
            {
                NAPI.Task.Run(() =>
                {
                    CollisionToggleHandler(time);
                }, delayTime: 2000);
            }
            else
            {
                foreach(Vehicle vehicle in Settings.GameVehicles)
                {
                    vehicle.Collisionless = false;
                }
            }
        }

        public void OnPlayerLogin(Client player, Account acc)
        {
            Player.SetDiscordStatus(player, $"Logging in as {acc.Username}");
            NAPI.Util.ConsoleOutput($"[Login] {player.Name} has logged in as {acc.Username}. (IP: {player.Address} - Serial: {player.Serial} - SC: {player.SocialClubName})");

            Blip pBlip = NAPI.Blip.CreateBlip(new Vector3(0.0, 0.0, -200.0), 0);
            pBlip.Transparency = 100;
            player.SetData("Blip", pBlip);

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
                player.SendChatMessage("~r~Please wait some time between each register attempt!");
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
                NAPI.Notification.SendNotificationToPlayer(player, "~r~Please wait some time between each register attempt!", true);
                player.SendChatMessage("~r~Please wait some time between each register attempt!");
                return;
            }
            if(!allcorrect || !rules)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~You must agree to the rules and ensure all information is correct!", true);
                player.SendChatMessage("~r~You must agree to the rules and ensure all information is correct!");
                return;
            }
            if(password.Length < 5)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~Your password is too small!", true);
                player.SendChatMessage("~r~Your password is too small!");
                return;
            }
            if (username.Contains("@") || username.Length < 4)
            { 
                NAPI.Notification.SendNotificationToPlayer(player, "~r~Your username is too small or it cannot contain the character '@'", true);
                player.SendChatMessage("~r~Your username is too small or it cannot contain the character '@'");
                return;
            }
            Regex exp = new Regex(@"^[a-zA-Z0-9]*$");
            Match match = exp.Match(username);
            if (!match.Success)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~Your username can only contain alphabets or numbers!", true);
                player.SendChatMessage("~r~Your username can only contain alphabets or numbers!");
                return;
            }
            if (!email.Contains("@") || email.Length < 5)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~Your username cannot contain the character '@'", true);
                player.SendChatMessage("~r~Your username cannot contain the character '@'");
                return;
            }
            Account acc = ContextFactory.Instance.Accounts.FirstOrDefault(a => a.Username == username);
            if (acc != null)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~An account with this name already exists!", true);
                player.SendChatMessage("~r~An account with this name already exists!");
                return;
            }
            acc = ContextFactory.Instance.Accounts.FirstOrDefault(a => a.Email == email);
            if (acc != null)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~An account with this email already exists!", true);
                player.SendChatMessage("~r~An account with this email already exists!");
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