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

public class LobbyData
{
    public List<Player> Cops { get; set; }
    public List<Player> Fugitives { get; set; }
    public List<Vehicle> Vehicles { get; set; }

    public int lobbyId { get; set; }
    public int gameStatus { get; set; }
    public int FugitiveShowCounter { get; set; }
    public int CollisionCounter { get; set; }
    public int GameProgress { get; set; }
    public int FugitivePercent { get; set; }
    public bool GameAutoStart { get; set; }
    public int LastAutoStartMsg { get; set; }

    public LobbyData(int id)
    {
        Cops = new List<Player>();
        Fugitives = new List<Player>();
        Vehicles = new List<Vehicle>();

        lobbyId = id;
        gameStatus = 0;
        FugitiveShowCounter = 0;
        CollisionCounter = 0;
        GameProgress = 0;
        GameAutoStart = true;
        LastAutoStartMsg = 0;
        FugitivePercent = 30;
    }
};

public static class Settings
{
    // Static Settings / Const
    public const string Version = "0.1a";
    public const int GameRunTime = 600; // Game Running Time in Seconds before STOP
    public const int MaxLobbies = 5;
    public const int DefaultMaxInLobby = 35;

    // MySQL Settings
    public const string MySQL_Host = "localhost";
    public const string MySQL_User = "root";
    public const string MySQL_Password = "";
    public const string MySQL_Database = "lsccv";

    // Global Variables (changing accordingly)
    public static List<LobbyData> Lobbies { get; set; }
    public static List<Player> LoggedInPlayers { get; set; }
    public static List<Vehicle> AdminVehicles { get; set; }
    public static int MaxPlayersInLobby { get; set; }

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

           
            Settings.Lobbies = new List<LobbyData>();
            Settings.LoggedInPlayers = new List<Player>();
            Settings.AdminVehicles = new List<Vehicle>();

            Settings.MaxPlayersInLobby = Settings.DefaultMaxInLobby;

            for(int i = 0; i < Settings.MaxLobbies; i++)
            {
                LobbyData eLobbyData = new LobbyData(i + 1);
                Settings.Lobbies.Add(eLobbyData);
            }
          
            NAPI.Task.Run(() =>
            {
                TimeUpdate();
            }, delayTime: 15000);

            NAPI.Task.Run(() =>
            {
                GameUpdate();
            }, delayTime: 1000);
        }

        public void GameStartCountdown(int time, int lobbyid)
        {
            time -= 2;

            LobbyData lobby = Settings.Lobbies.Find(l => l.lobbyId == lobbyid);
            if (lobby != null)
            {
                foreach (Player player in Settings.LoggedInPlayers)
                {
                    if (player.GetData<int>("InLobby") == lobbyid && player.GetData<bool>("AutoJoin") == true)
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
            foreach (LobbyData lobby in Settings.Lobbies)
            {
                if (lobby != null)
                {
                    if (lobby.gameStatus == 0)
                    {
                        if (lobby.GameAutoStart)
                        {
                            int pCount = 0;
                            foreach (Player player in Settings.LoggedInPlayers)
                            {
                                if (player.GetData<int>("InLobby") == lobby.lobbyId && player.GetData<bool>("AutoJoin") == true)
                                {
                                    pCount++;
                                }
                            }

                            if (pCount >= 2)
                            {
                                lobby.gameStatus = 2;

                                foreach (Player player in Settings.LoggedInPlayers)
                                {
                                    if (player.GetData<int>("InLobby") == lobby.lobbyId && player.GetData<bool>("AutoJoin") == true)
                                    {
                                        EPlayer.Spawn(player);
                                        NAPI.Notification.SendNotificationToPlayer(player, $"~w~Game is starting in ~g~10 seconds~w~.", true);

                                        NAPI.Util.ConsoleOutput("[TimeUpdate] Game starting in 10 seconds...");
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
                                        EPlayer.SendMessageInLobby(lobby.lobbyId, "!{#FFFF00}[GAME] !{#FFFFFF}Game will not start unless there are 2+ players to start a match.");
                                    }
                                    lobby.LastAutoStartMsg = Environment.TickCount;
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

            NAPI.Task.Run(() =>
            {
                TimeUpdate();
            }, delayTime: 15000);
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
                        }
                        break;
                    case 2:
                        NAPI.Chat.SendChatMessageToAll($"[Lobby {lobbyid}] !{{#DAA520}}All law enforcement units have been taken out by the fugitives.");

                        foreach (Player i in lobby.Fugitives)
                        {
                            // Reward
                        }
                        break;
                    case 3:
                        NAPI.Chat.SendChatMessageToAll($"[Lobby {lobbyid}] !{{#DAA520}}The situation has been successfully handled by the police.");

                        foreach (Player i in lobby.Cops)
                        {
                            // Reward
                        }
                        break;
                }

                lobby.gameStatus = 0;

                foreach (Player i in Settings.LoggedInPlayers)
                {
                    if (i.GetData<int>("InLobby") != lobby.lobbyId) continue;

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
            LobbyData lobby = Settings.Lobbies.Find(l => l.lobbyId == lobbyid);
            if (lobby != null && lobby.gameStatus != 1)
            {
                if (lobby.gameStatus == 1) { NAPI.Util.ConsoleOutput("[Lobby " + lobbyid + "] Copchase is already running, failed to start"); return false; }

                lobby.gameStatus = 0;
                int pCount = 0;

                lobby.Cops.Clear();
                lobby.Fugitives.Clear();

                foreach (Player player in Settings.LoggedInPlayers)
                {
                    if (player.GetData<int>("InLobby") == lobbyid && player.GetData<bool>("AutoJoin") == true)
                    {
                        pCount++;
                    }
                }

                if (pCount < 2) { NAPI.Util.ConsoleOutput("[Lobby " + lobbyid + "] Total players less than two, failed to start"); return false; }

                double perc = (double)pCount * ((double)lobby.FugitivePercent / (double)100);

                int fugitives = (int)Math.Round(perc);

                if (fugitives < 1) { NAPI.Util.ConsoleOutput("[Lobby " + lobbyid + "] Fugitives count is zero, failed to start"); return false; }

                if (fugitives == pCount) fugitives--; // There will ALWAYS be AT LEAST one COP.

                if (ContextFactory.Instance.Maps.Count() < 1) { NAPI.Util.ConsoleOutput("[Lobby " + lobbyid + "] Server has no maps added, failed to start"); return false; }

                List<Map> mapList = ContextFactory.Instance.Maps.Where(a => a.Enabled == true).ToList();

                if (mapList.Count < 1) { NAPI.Util.ConsoleOutput("[Lobby " + lobbyid + "] Server has maps but none of them were enabled! Failed to start."); return false; }

                Map map = mapList.OrderBy(c => Guid.NewGuid()).FirstOrDefault();

                NAPI.Util.ConsoleOutput($"[Lobby " + lobbyid + $"] Selected Map [{map.Name}] | Total Spawns: [{ContextFactory.Instance.MapSpawns.Where(a => a.mapID == map.SQL_ID).Count()}]");


                if (ContextFactory.Instance.MapSpawns.Where(a => a.mapID == map.SQL_ID && a.spawnTeam == 0).Count() < 1) { NAPI.Util.ConsoleOutput($"[Lobby {lobbyid}] Map {map.Name} has no fugitive spawns! Abort."); return false; }
                if (ContextFactory.Instance.MapSpawns.Where(a => a.mapID == map.SQL_ID && a.spawnTeam == 1).Count() < 1) { NAPI.Util.ConsoleOutput($"[Lobby {lobbyid}] Map {map.Name} has no cop spawns! Abort."); return false; }


                List<MapSpawn> Spawns = ContextFactory.Instance.MapSpawns.Where(x => x.mapID == map.SQL_ID).ToList();
                List<int> UsedSpawns = new List<int>(Spawns.Count + 1);

                List<Player> lobbyPlayers = Settings.LoggedInPlayers.Where(a => a.GetData<int>("InLobby") == lobbyid).ToList();

                List<Player> PutInsideCar = new List<Player>(lobbyPlayers.Count());

                UsedSpawns.Add(-1);

                Random random = new Random();

                pCount = 0;
                Player _tempPlayer;

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

                foreach (Player player in lobbyPlayers)
                {
                    if (player.GetData<bool>("AutoJoin") == false) continue;

                    if (!lobby.Fugitives.Contains(player))
                    {
                        lobby.Cops.Add(player);
                    }
                }

                List<Tuple<Player, Vehicle, int>> playerVehicle = new List<Tuple<Player, Vehicle, int>>();

                MapSpawn spawnLoc = null;
                pCount = 0;
                foreach (Player player in lobby.Cops)
                {
                    spawnLoc = null;
                    foreach (MapSpawn spawn in Spawns.Where(a => a.spawnTeam == 1))
                    {
                        if (!UsedSpawns.Contains(spawn.SQL_ID) || UsedSpawns.Count() < 1)
                        {
                            spawnLoc = spawn;
                            UsedSpawns.Add(spawn.SQL_ID);
                            break;
                        }
                    }

                    if (spawnLoc == null)
                    {
                        pCount++;
                        spawnLoc = Spawns.FirstOrDefault(a => a.spawnTeam == 1);
                        spawnLoc.spawnZ += (pCount * 3); // If no spawns left, stack cars ontop so at least they don't stuck in each other.
                        spawnLoc.spawnX += (pCount * 2);
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


                    Vehicle pVehicle = NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(carName), vecSpawn, spawnLoc.spawnA, 0, 111, "SIMBA", 255, false, true, Convert.ToUInt32(lobbyid));

                    pVehicle.SetSharedData("Team", 1);
                    lobby.Vehicles.Add(pVehicle);

                    player.Dimension = Convert.ToUInt32(lobbyid);
                    player.Position = vecSpawn.Around(15);
                    EPlayer.Freeze(player, true);

                    player.Health = 100;
                    player.Armor = 60;
                    EPlayer.SetColor(player, new Color(141, 141, 255, 255), player.Armor);

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

                    PutInsideCar.Add(player);
                    playerVehicle.Add(new Tuple<Player, Vehicle, int>(player, pVehicle, 0));

                    player.SetData("GPS", false);

                    EPlayer.SetDiscordStatus(player, $"[Lobby {lobbyid}] Playing cop as {player.Name}");

                    player.TriggerEvent("SetActorInvulnerable", false);
                    player.TriggerEvent("CanFallOutOfCar", true);
                    player.TriggerEvent("showHPBars", 1); // Always show HP Bars.

                    player.SetSharedData("Team", 1);
                }

                spawnLoc = null;


                foreach (Player player in lobby.Fugitives)
                {
                    spawnLoc = Spawns.FirstOrDefault(a => a.spawnTeam == 0);

                    Vector3 vecSpawn = new Vector3(spawnLoc.spawnX, spawnLoc.spawnY, spawnLoc.spawnZ);

                    player.Dimension = Convert.ToUInt32(lobbyid);
                    player.Position = vecSpawn.Around(15);
                    EPlayer.Freeze(player, true);

                    player.Health = 100;
                    player.Armor = 0;
                    EPlayer.SetColor(player, new Color(232, 64, 64, 255), player.Armor);

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


                    if (player.HasData("Blip"))
                    {
                        Blip tBlip = player.GetData<Blip>("Blip");
                        tBlip.Delete();
                        player.ResetData("Blip");
                    }

                    Blip pBlip = NAPI.Blip.CreateBlip(new Vector3(spawnLoc.spawnX, spawnLoc.spawnY, spawnLoc.spawnZ), player.Dimension);
                    pBlip.Transparency = 255;
                    pBlip.Color = 75;

                    player.SetData("Blip", pBlip);

                    EPlayer.SetDiscordStatus(player, $"[Lobby {lobbyid}] Playing fugitive as {player.Name}");

                    player.TriggerEvent("SetActorInvulnerable", false);
                    player.TriggerEvent("CanFallOutOfCar", true);
                    player.TriggerEvent("showHPBars", 1); // Always show HP Bars.

                    player.SetSharedData("Team", 0);
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

                    spawnLoc = null;
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
                            carName = "oracle";
                            break;
                        case 4:
                            carName = "oracle2";
                            break;
                    }

                    Vehicle pVehicle = NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(carName), vecSpawn, spawnLoc.spawnA, random.Next(64), random.Next(64), "KEVY", 255, false, true, Convert.ToUInt32(lobbyid));
                    pVehicle.SetSharedData("Team", 0);
                    lobby.Vehicles.Add(pVehicle);

                    playerVehicle.Add(new Tuple<Player, Vehicle, int>(_tempPlayer, pVehicle, 0));

                    int seat = 1;
                    while (seat < pVehicle.MaxOccupants)
                    {
                        if (fugitives == 0) break;

                        Player _tempPlayer2 = lobby.Fugitives[random.Next(lobby.Fugitives.Count)];

                        if (PutInsideCar.Contains(_tempPlayer2)) continue;

                        PutInsideCar.Add(_tempPlayer2);
                        iteratedAlready.Add(_tempPlayer2);
                        fugitives--;


                        playerVehicle.Add(new Tuple<Player, Vehicle, int>(_tempPlayer2, pVehicle, 0 + seat));

                        seat++;
                    }
                }

                NAPI.Task.Run(() =>
                {
                    foreach (Tuple<Player, Vehicle, int> data in playerVehicle)
                    {
                        if (data.Item1.IsNull || data.Item1 == null || data.Item2.IsNull || data.Item2 == null) continue;

                        data.Item1.SetIntoVehicle(data.Item2, data.Item3);
                        EPlayer.Freeze(data.Item1, false);

                        data.Item1.TriggerEvent("ToggleChaseCollisions", false);

                        if (lobby.Cops.Contains(data.Item1))
                        {
                            data.Item1.GiveWeapon(WeaponHash.Carbinerifle, 100);
                            data.Item1.GiveWeapon(WeaponHash.Combatpistol, 100);
                        }
                        else if (lobby.Fugitives.Contains(data.Item1))
                        {
                            data.Item1.GiveWeapon(WeaponHash.Assaultrifle, 100);
                            data.Item1.GiveWeapon(WeaponHash.Combatpistol, 100);
                        }
                    }

                    NAPI.Chat.SendChatMessageToAll($"[Lobby {lobbyid}] Copchase started with !{{#8D8DFF}}{lobby.Cops.Count} cops !{{#FFFFFF}}& !{{#E84040}}{lobby.Fugitives.Count} fugitives !{{#FFFFFF}}on map !{{#DAA520}}{map.Name}!{{#FFFFFF}}.");

                    lobby.FugitiveShowCounter = 60;
                    lobby.GameProgress = 0;
                    lobby.CollisionCounter = 30;

                    foreach (Player p in Settings.LoggedInPlayers)
                    {
                        if (p.GetData<int>("InLobby") == lobbyid)
                        {
                            p.TriggerEvent("StartChaseTimer", 0, Settings.GameRunTime, lobby.CollisionCounter);
                        }
                    }

                    NAPI.Task.Run(() =>
                    {
                        foreach (Tuple<Player, Vehicle, int> data in playerVehicle)
                        {
                            if (data.Item1.IsNull || data.Item1 == null) continue;
                            data.Item1.TriggerEvent("ToggleChaseCollisions", false);
                        } // Toggle Collisions OFF again. smh they dont activate sometimes.

                    }, delayTime: 2000);

                }, delayTime: 3000);


                lobby.gameStatus = 1;
                NAPI.Util.ConsoleOutput($"[Lobby {lobbyid}] Game has been started with {lobby.Cops.Count} cops & {lobby.Fugitives.Count} fugitives on map {map.Name}.");
            }
            else return false;

            return true;
        }

        public static void GameUpdate()
        {
            foreach(LobbyData lobby in Settings.Lobbies.Where(l => l.gameStatus == 1))
            {
                lobby.GameProgress++;

                if (lobby.GameProgress >= Settings.GameRunTime)
                {
                    StopGame(lobby.lobbyId, 1);
                    return;
                }

                lobby.FugitiveShowCounter++;

                if (lobby.CollisionCounter > 0)
                {
                    lobby.CollisionCounter--;

                    foreach (Player player in Settings.LoggedInPlayers.Where(p => p.GetData<int>("InLobby") == lobby.lobbyId))
                    {
                        if (!lobby.Cops.Contains(player) && !lobby.Fugitives.Contains(player)) continue;

                        if (lobby.CollisionCounter < 1)
                        {
                            player.TriggerEvent("ToggleChaseCollisions", true);
                            NAPI.Notification.SendNotificationToPlayer(player, $"~r~Collisions enabled!", true);
                        }
                    }
                }

                foreach (Player cop in lobby.Cops)
                {
                    if (cop.HasData("Blip"))
                    {
                        Blip tBlip = cop.GetData<Blip>("Blip");
                        tBlip.Delete();
                        cop.ResetData("Blip");
                    }

                    if (cop.GetData<bool>("GPS") == false) continue;

                    Blip pBlip = NAPI.Blip.CreateBlip(cop.Position, lobby.lobbyId);

                    pBlip.Transparency = 192;
                    pBlip.Color = 3;

                    cop.SetData("Blip", pBlip);
                }

                if (lobby.FugitiveShowCounter >= 60 && lobby.FugitiveShowCounter < 90)
                {
                    foreach (Player fugi in lobby.Fugitives)
                    {
                        if (fugi.HasData("Blip"))
                        {
                            Blip tBlip = fugi.GetData<Blip>("Blip");
                            tBlip.Delete();
                            fugi.ResetData("Blip");
                        }

                        Blip pBlip = NAPI.Blip.CreateBlip(fugi.Position, lobby.lobbyId);

                        pBlip.Transparency = 238;
                        pBlip.Color = 1;

                        fugi.SetData("Blip", pBlip);

                        if(lobby.FugitiveShowCounter == 60) fugi.SendNotification("~r~You are now visible on the radar.", true);
                    }
                }

                if (lobby.FugitiveShowCounter >= 90)
                {
                    lobby.FugitiveShowCounter = 0;
                    foreach (Player fugi in lobby.Fugitives)
                    {
                        if (fugi.HasData("Blip"))
                        {
                            Blip tBlip = fugi.GetData<Blip>("Blip");
                            tBlip.Delete();
                            fugi.ResetData("Blip");
                        }

                        fugi.SendNotification("~g~You are no longer visible on the radar.", true);
                    }
                }
            }

            NAPI.Task.Run(() =>
            {
                GameUpdate();
            }, delayTime: 1000);

            return;
        }
    }
}