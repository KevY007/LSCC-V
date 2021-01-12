using GTANetworkAPI;
using System;
using LSCC;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net.Mail;

namespace LSCC
{
    class Events : Script
    {
        [ServerEvent(Event.PlayerConnected)]
        public void OnPlayerConnected_Old(Player player)
        {

        }

        [RemoteEvent("playerReadyCon")]
        public void OnPlayerConnected(Player player)
        {
            BanData ban = ContextFactory.Instance.Bans.ToList().Find(ban => ban.HWID.CompareTo(player.Serial) == 0 || ban.IP.CompareTo(player.Address) == 0 ||
                ban.RockstarID == player.SocialClubId);

            Discord.SendTextChannel("staff-bot", $"**{player.Name} has joined the server.** *(Total online: {RAGE.Entities.Players.Count})*");

            if (ban != null)
            {
                EPlayer.SetDiscordStatus(player, "Gazing at the 'BANNED' screen.");

                player.SendChatMessage($"!{{#FF6347}}[BANNED] You are not allowed to play on this server, Ban ID: {ban.SQL_ID}");

                player.SendChatMessage("!{#A9C4E4}Server closed the connection.");

                NAPI.Task.Run(() =>
                {
                    lock (Settings.LoggedInPlayers)
                    {
                        try
                        {
                            if (player != null && player.Exists)
                            {
                                player.Kick("Banned.");
                            }
                        }
                        catch { }
                    }
                }, 350);

                return;
            }

            foreach (Player pl in RAGE.Entities.Players.All)
            {
                if (player.HasData("BlockList"))
                {
                    List<Player> blockList = pl.GetData<List<Player>>("BlockList");
                    blockList.Remove(player);
                }
            }



            EPlayer.SetDiscordStatus(player, "Connecting to the server...");
            NAPI.Util.ConsoleOutput($"[Connection] {player.Name} has joined the server. (IP: {player.Address} - Serial: {player.Serial} - SC: {player.SocialClubName})");
            NAPI.Notification.SendNotificationToPlayer(player, "~g~Welcome to ~w~Los Santos Copchase!");
            player.SetData("LastCryptAttempt", Environment.TickCount64);

            EPlayer.ResetPlayerData(player);
            EPlayer.Freeze(player, true);

            EPlayer.LoginCameraSetup(player);


            EPlayer.LoginBrowserSetup(player);

            player.TriggerEvent("InitChaseTimer");

            player.TriggerEvent("PlayThemeSong", "http://51.91.109.79/_lsccv_stuff/v_open.mp3", 0.035);

            NAPI.Task.Run(() =>
            {
                try
                {
                    if (player != null && player.Exists)
                    {
                        EPlayer.LoginCameraSetup(player);
                    }
                }
                catch { }
            }, 2000);
        }

        [ServerEvent(Event.PlayerWeaponSwitch)]
        public void OnPlayerWeaponSwitch(Player player, WeaponHash oldWeapon, WeaponHash newWeapon)
        {
            if (!EPlayer.LoggedIn(player)) return;

            return;
        }

        [RemoteEvent("clear_anim")]
        public static void RequestClearAnimation(Player player)
        {
            RAGE.Entities.Players.All.Where(a => a.Dimension == player.Dimension && a.Position.DistanceTo(player.Position) < 200.0).ToList().ForEach((Player act) =>
            {
                if (player.HasData("LastAnimDict"))
                {
                    act.TriggerEvent("entclearanim", player, player.GetData<string>("LastAnimDict"), player.GetData<string>("LastAnim"));
                }
                else act.TriggerEvent("entclearanim", player, "null", "null");
            });
        }

        [RemoteEvent("UpdateLastAnim")]
        public static void RequestUpdateLastAnim(Player player, string animDict, string animName)
        {
            player.SetData("LastAnimDict", animDict);
            player.SetData("LastAnim", animName);
        }

        [RemoteEvent("play_anim")]
        public static void RequestPlayAnimation(Player player, string animDict, string animName, int animFlags)
        {
            RAGE.Entities.Players.All.Where(a => a.Dimension == player.Dimension && a.Position.DistanceTo(player.Position) < 500.0).ToList().ForEach((Player act) =>
            {
                act.TriggerEvent("entplayanim", player, animDict, animName, animFlags);
            });
        }

        [RemoteEvent("OnScriptedKeyPress")]
        public static void OnScriptedKeyPress(Player player, int keyId, bool keyState) // keyState == true? Down
        {
            if (keyId == 0x5A)
            {
                LobbyData lobby = Settings.Lobbies.Find(lobby => lobby.lobbyId == player.GetSharedData<int>("InLobby"));
                if (lobby != null)
                {
                    if (lobby.Cops.Contains(player)) LSCC.Commands.cmd_gps(player);
                }
            }

            if (keyId == 0x09)
            {
                int voiceMode = player.GetSharedData<int>("VoiceMode");
                voiceMode++;

                if (voiceMode >= 3) voiceMode = 0;

                switch (voiceMode)
                {
                    case 0:
                        player.SendChatMessage("!{#FFFF00}Voice mode set to: !{#FFFFFF}Nearby (30m) (70m with megaphone/cop car)");
                        break;
                    case 1:
                        player.SendChatMessage("!{#FFFF00}Voice mode set to: !{#FFFFFF}Team Chat");
                        break;
                    case 2:
                        player.SendChatMessage("!{#FFFF00}Voice mode set to: !{#FFFFFF}Disabled");
                        break;
                }

                player.SetSharedData("VoiceMode", voiceMode);
            }

            if (keyId == 0x4E)
            {
                if (keyState)
                {
                    if (EPlayer.GetAccount(player).MuteTime > 0)
                    {
                        player.SendChatMessage("!{#FF6347}You are currently muted for " + EPlayer.GetAccount(player).MuteReason + ", seconds left: " + EPlayer.GetAccount(player).MuteTime + ".");
                        return;
                    }

                    // add listeners

                    List<Player> listeners = new List<Player>();
                    if (player.GetSharedData<int>("VoiceMode") == 0)
                    {
                        double dist = 30.0;
                        if (player.IsInVehicle && player.Vehicle.HasSharedData("Team") && player.Vehicle.GetSharedData<int>("Team") == 1)
                        {
                            dist = 70.0;
                        }

                        listeners.AddRange(RAGE.Entities.Players.All.Where(t => t.Dimension == player.Dimension &&
                            Vector3.Distance(t.Position, player.Position) <= dist));
                    }
                    else if (player.GetSharedData<int>("VoiceMode") == 1)
                    {
                        listeners.AddRange(RAGE.Entities.Players.All.Where(t => t.Dimension == player.Dimension &&
                            Vector3.Distance(t.Position, player.Position) <= 15.0));

                        LobbyData lobby = Settings.Lobbies.Find(lobby => lobby.lobbyId == player.GetSharedData<int>("InLobby"));

                        if (lobby != null)
                        {
                            if (lobby.Cops.Contains(player))
                            {
                                listeners.AddRange(lobby.Cops);
                            }
                            else if (lobby.Fugitives.Contains(player))
                            {
                                listeners.AddRange(lobby.Fugitives);
                            }
                        }
                    }
                    else if (player.GetSharedData<int>("VoiceMode") == 2)
                    {
                        // Disabled voice. Nah, no listeners for you.
                    }

                    listeners.RemoveAll(p => p.Id == player.Id); // Remove ALL instances of local player.

                    foreach (Player t in listeners)
                    {
                        if (t.HasData("BlockList"))
                        {
                            List<Player> blockList = t.GetData<List<Player>>("BlockList");
                            if (blockList.Contains(player))
                            {
                                continue;
                            }
                        }

                        if (t.GetSharedData<int>("VoiceMode") == 2) continue;

                        player.EnableVoiceTo(t);
                    }
                }
                else
                {
                    // remove all listeners
                    foreach (Player t in RAGE.Entities.Players.All)
                    {
                        player.DisableVoiceTo(t);
                    }
                }
            }
            return;
        }

        [RemoteEvent("RequestParachute")]
        public void OnPlayerReqParachute(Player player, Vehicle veh)
        {
            player.GiveWeapon(WeaponHash.Parachute, 1);

            if(player.GetSharedData<int>("Team") == 0 && player.GetSharedData<int>("InLobby") > -1)
            {
                if(veh != null)
                {
                    if(veh.HasSharedData("Team") && veh.GetSharedData<int>("Team") == 1)
                    {
                        player.WarpOutOfVehicle();
                        player.Position = player.Position.Around(1.0f);
                        player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot get inside a cop plane/chopper as a fugitive due to balance!");
                    }
                }
            }
        }

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void OnPlayerEnterVehicle(Player player, Vehicle vehicle, sbyte seatID)
        {
            
        }

        [ServerEvent(Event.PlayerDisconnected)]
        public void OnPlayerDisconnected(Player player, DisconnectionType type, string reason)
        {
            NAPI.Util.ConsoleOutput($"[Connection] {player.Name} has left the server. (IP: {player.Address} - Serial: {player.Serial} - SC: {player.SocialClubName})");

            lock (Settings.Requests)
            {
                Settings.Requests.RemoveAll(a => a.Reporter == player || a.Reported == player);
            }

            try
            {
                lock (Settings.LoggedInPlayers)
                {
                    Settings.LoggedInPlayers.ForEach((Player pl) =>
                    {
                        if (pl.HasData("AskedBySquad") && pl.GetData<Player>("AskedBySquad") == player)
                        {
                            pl.ResetData("AskedBySquad");
                        }
                    });
                }
            }
            catch { }

            lock (player)
            {
                if (EPlayer.LoggedIn(player))
                {
                    if (player.HasData("DerbyVehicle"))
                    {
                        try { player.GetData<Vehicle>("DerbyVehicle").Delete(); }
                        catch { }

                        player.ResetData("DerbyVehicle");
                    }

                    if (player.HasData("PlayerSquad"))
                    {
                        player.ResetData("PlayerSquad");
                    }

                    lock (Settings.LoggedInPlayers)
                    {
                        foreach (Player mem in Settings.LoggedInPlayers.Where(p => p.HasData("PlayerSquad") && p.GetData<Player>("PlayerSquad") == player))
                        {
                            mem.SendChatMessage("!{#FFAC38}[Partners] This squad has been disbanded as the leader left.");
                            mem.ResetData("PlayerSquad");
                        }
                    }

                    player.SetSharedData("VisibleBlip", false);

                    List<Vehicle> freeroamVehs = new List<Vehicle>();
                    freeroamVehs = player.GetData<List<Vehicle>>("FreeroamVehicles");

                    for (int i = freeroamVehs.Count - 1; i >= 0; i--)
                    {
                        if (!freeroamVehs[i].IsNull && freeroamVehs[i] != null)
                        {
                            freeroamVehs[i].Delete();
                            freeroamVehs.RemoveAt(i);
                        }
                    }

                    if(freeroamVehs.Count > 0)
                    {
                        for(int i = 0; i < freeroamVehs.Count; i++)
                        {
                            freeroamVehs.RemoveAt(i);
                        }
                    }

                    freeroamVehs.Clear();

                    switch (type)
                    {
                        case DisconnectionType.Left:
                            NAPI.Chat.SendChatMessageToAll("!{#4343FA}[PART] !{#FFFFFF}" + player.Name + " has left the server. !{#4343FA}(Leaving)");
                            break;

                        case DisconnectionType.Timeout:
                            NAPI.Chat.SendChatMessageToAll("!{#4343FA}[PART] !{#FFFFFF}" + player.Name + " has left the server. !{#4343FA}(Kicked/TimedOut)");
                            break;
                    }

                    EPlayer.Save(player);
                }

                EPlayer.ResetPlayerData(player);

                Discord.SendTextChannel("staff-bot", $"**{player.Name} has left the server.** *(Total online: {RAGE.Entities.Players.Count - 1})*");
            }
        }

        [ServerEvent(Event.PlayerDeath)]
        public static void OnPlayerDeath(Player player, Player killer, uint reason)
        {
            if (!EPlayer.LoggedIn(player)) return;

            player.ResetData("CuffingTarget");
            player.ResetData("FreezeWeapons");

            string reasonStr;
            bool wasInGame = false;
            jWeaponData wepDat = new jWeaponData();

            if (Main.WeaponReasons.TryGetValue(reason, out reasonStr))
            {
                // oh its already set ^^
            }
            else if(Settings.WeaponData.TryGetValue((ulong)reason, out wepDat))
            {
                reasonStr = wepDat.Name;
            }
            else
            {
                reasonStr = null;
            }

            if (reason == 696988) reasonStr = "handcuffs";
            if (reason == 696989) reasonStr = "arrest/busted";

            if (reasonStr != null)
            {
                reasonStr.Replace(".", "");
            }

            if (player.GetSharedData<int>("InLobby") == -4 && Settings.DerbyTimeLeft > 0 && Settings.DerbyTimeLeft != 180)
            {
                lock (Settings.LoggedInPlayers)
                {
                    foreach (Player indm in Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == -4))
                    {
                        indm.TriggerEvent("pushToKillFeed", $"DERBY> {player.Name} fell down");
                    }

                    player.SetSharedData("DerbyTime", 0);
                }
            }

            if (killer != null && !killer.IsNull && killer != player)
            {
                NAPI.Util.ConsoleOutput($"[Death] {player.Name} has been killed by {killer.Name} using {(reasonStr == null ? "N/A" : reasonStr)}.");
                Discord.SendTextChannel("staff-bot", $"`(Death) | Killer: {killer.Name} ({killer.GetSharedData<int>("Team")}), Victim: {player.Name} ({player.GetSharedData<int>("Team")}), Item: {(reasonStr == null ? "N/A" : reasonStr)}`");

                if (killer.GetSharedData<int>("InLobby") > -1 && killer.GetSharedData<int>("InDM") > -1)
                {
                    int arenaID = killer.GetSharedData<int>("InDM");

                    lock (Settings.LoggedInPlayers)
                    {
                        foreach (Player indm in Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == killer.GetSharedData<int>("InLobby") && a.GetSharedData<int>("InDM") == arenaID))
                        {
                            if (reasonStr == null)
                            {
                                indm.TriggerEvent("pushToKillFeed", $"DM> {killer.Name} killed {player.Name}");
                            }
                            else
                            {
                                indm.TriggerEvent("pushToKillFeed", $"DM> {killer.Name} killed {player.Name} with {reasonStr}");
                            }
                        }
                    }

                    EPlayer.GrantXP(killer, 1);
                }

                if (killer.GetSharedData<int>("Team") != player.GetSharedData<int>("Team") && player.GetSharedData<int>("Team") != -1)
                {
                    EPlayer.GrantXP(killer, 1);

                    if (reason != 696988 && reason != 696989 && player.GetSharedData<int>("Team") == 0 && killer.GetSharedData<int>("Team") == 1 && player.GetSharedData<bool>("HasShot") == false)
                    {

                        Vehicle pVeh = null;
                        int seat = 0;
                        if (player.IsInVehicle)
                        {
                            pVeh = player.Vehicle;
                            seat = player.VehicleSeat;
                        }

                        if (pVeh == null || RAGE.Entities.Players.All.Count(ey => ey.IsInVehicle && ey.Vehicle == player.Vehicle &&
                                 ey.GetSharedData<int>("Team") == 0 && ey.GetSharedData<bool>("HasShot") == true) == 0)
                        {
                            EPlayer.GrantXP(killer, -3);
                            RAGE.Entities.Players.All.ForEach((Player ey) =>
                            {
                                ey.SendChatMessage($"!{{FF6347}}[RULEBREAK] {killer.Name}'s XP has been reduced by 3 for killing a fugitive that hasn't shot.");
                            });

                            int warnings = 0;
                            if (killer.HasData("InnocentShotWarning")) warnings = killer.GetData<int>("InnocentShotWarning");

                            warnings += 5;
                            if (warnings >= 7)
                            {
                                killer.Health = 0;
                                killer.SendChatMessage($"!{{FF6347}}[RULEBREAK] You have been killed for killing an innocent fugitive.");
                                warnings = 0;
                            }

                            killer.SetData<int>("InnocentShotWarning", warnings);
                            killer.SendChatMessage($"!{{FF6347}}[RULEBREAK] !{{#FFFFFF}}Repeated attacking of fugitive's that haven't shot will result in a jail.");
                        }


                        player.SetData("UnfairKillSpawn", true);

                        

                        NAPI.Player.SpawnPlayer(player, player.Position);

                        player.Armor = 0;
                        player.Health = 100;
                        player.SendChatMessage($"!{{FF6347}}[!] !{{#FFFFFF}}You have been put back in the game as you were killed unfairly.");

                        if (pVeh != null)
                        {
                            player.SetIntoVehicle(pVeh, seat);
                        }

                        return;
                    }
                }


                if (reason != 696988 && reason != 696989 && player.GetSharedData<int>("Team") != -1 && player.GetSharedData<int>("Team") == killer.GetSharedData<int>("Team"))
                {
                    if (RAGE.Entities.Players.All.Count(ey => ey.GetSharedData<int>("Team") != -1 && ey.GetSharedData<int>("Team") != player.GetSharedData<int>("Team") &&
                             ey.Position.DistanceTo(player.Position) < 10.0) == 0)
                    {
                        EPlayer.GrantXP(killer, -2);
                        RAGE.Entities.Players.All.ForEach((Player ey) =>
                        {
                            ey.SendChatMessage($"!{{FF6347}}[RULEBREAK] {killer.Name}'s XP has been reduced by 2 for team killing.");
                        });

                        int warnings = 0;
                        if (killer.HasData("TeamShotWarning")) warnings = killer.GetData<int>("TeamShotWarning");

                        warnings += 5;

                        if (killer.IsInVehicle == true && killer.Vehicle.Model == NAPI.Util.GetHashKey("polmav"))
                        {
                            killer.Vehicle.Position = new Vector3(killer.Vehicle.Position.X, killer.Vehicle.Position.Y, killer.Vehicle.Position.Z + 20.0f);
                            killer.SendChatMessage($"!{{FF6347}}[RULEBREAK] !{{#FFFFFF}}Please do not heliblade.");
                        }
                        else if(warnings >= 7)
                        {
                            killer.Health = 0;
                            killer.SendChatMessage($"!{{FF6347}}[RULEBREAK] You have been killed for team killing.");
                            warnings = 0;
                        }

                        killer.SetData<int>("TeamShotWarning", warnings);

                        killer.SendChatMessage($"!{{FF6347}}[RULEBREAK] !{{#FFFFFF}}Repeated team killing will result in a jail.");
                    }

                    player.SetData("UnfairKillSpawn", true);

                    lock (player)
                    {
                        Vehicle pVeh = null;
                        int seat = 0;
                        if (player.IsInVehicle)
                        {
                            pVeh = player.Vehicle;
                            seat = player.VehicleSeat;
                        }

                        NAPI.Player.SpawnPlayer(player, player.Position);

                        player.Armor = 0;
                        player.Health = 100;
                        player.SendChatMessage($"!{{FF6347}}[!] !{{#FFFFFF}}You have been put back in the game as you were killed unfairly.");

                        if (pVeh != null)
                        {
                            player.SetIntoVehicle(pVeh, seat);
                        }
                    }

                    return;
                }
            }
            else
            {
                NAPI.Util.ConsoleOutput($"[Death] {player.Name} has died of {(reasonStr == null ? "N/A" : reasonStr)}.");
            }

            LobbyData lobby = Settings.Lobbies.Find(lobby => lobby.lobbyId == player.GetSharedData<int>("InLobby"));
            if (lobby != null)
            {
                if (lobby.gameStatus == 1)
                {
                    if (lobby.Fugitives.Contains(player))
                    {
                        wasInGame = true;

                        if (killer != null && killer.GetSharedData<int>("Team") == 1) EPlayer.GrantXP(killer, 2);

                        lock (Settings.LoggedInPlayers)
                        {
                            foreach (Player ay in Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == lobby.lobbyId))
                            {
                                if (killer != null && !killer.IsNull && killer != player)
                                {
                                    if (reasonStr == null)
                                    {
                                        ay.TriggerEvent("pushToKillFeed", $"{killer.Name} killed {player.Name}");
                                    }
                                    else
                                    {
                                        if (reason == 696988)
                                        {
                                            ay.TriggerEvent("pushToKillFeed", $"{killer.Name} has apprehended {player.Name}");
                                        }
                                        else if (reason == 696989)
                                        {
                                            ay.TriggerEvent("pushToKillFeed", $"{killer.Name} has busted {player.Name}");
                                        }
                                        else ay.TriggerEvent("pushToKillFeed", $"{killer.Name} killed {player.Name} with {reasonStr}");
                                    }
                                }
                                else
                                {
                                    ay.SendNotification($"Fugitive ~r~<C>{player.Name}</C> ~w~has died.");

                                    if (reasonStr == null)
                                    {
                                        ay.TriggerEvent("pushToKillFeed", $"{player.Name} died");
                                    }
                                    else
                                    {
                                        ay.TriggerEvent("pushToKillFeed", $"{player.Name} died of {reasonStr}");
                                    }
                                }
                            }
                        }
                        lobby.Fugitives.Remove(player);
                    }

                    if (lobby.Cops.Contains(player))
                    {
                        wasInGame = true;

                        if (killer != null && killer.GetSharedData<int>("Team") == 0) EPlayer.GrantXP(killer, 2);

                        lock (Settings.LoggedInPlayers)
                        {
                            foreach (Player ay in Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == lobby.lobbyId))
                            {
                                if (killer != null && !killer.IsNull && player != killer)
                                {
                                    ay.SendNotification($"Cop ~b~<C>{player.Name}</C> ~w~has been taken out by ~r~<C>{killer.Name}</C>~w~.");

                                    if (reasonStr == null)
                                    {
                                        ay.TriggerEvent("pushToKillFeed", $"{killer.Name} killed {player.Name}");
                                    }
                                    else
                                    {
                                        ay.TriggerEvent("pushToKillFeed", $"{killer.Name} killed {player.Name} with {reasonStr}");
                                    }
                                }
                                else
                                {
                                    ay.SendNotification($"Cop ~r~<C>{player.Name}</C> ~w~has died.");

                                    if (reasonStr == null)
                                    {
                                        ay.TriggerEvent("pushToKillFeed", $"{player.Name} died");
                                    }
                                    else
                                    {
                                        ay.TriggerEvent("pushToKillFeed", $"{player.Name} died of {reasonStr}");
                                    }
                                }
                            }
                        }

                        lobby.Cops.Remove(player);
                    }
                }
            }

            player.RemoveAllWeapons();

            NAPI.Task.Run(() =>
            {
                try
                {
                    if (player != null && player.Exists)
                    {
                        Random rand = new Random();
                        Vector3 SpawnArea = Settings.LobbySpawns[rand.Next(Settings.LobbySpawns.Length)];
                        if (player.GetSharedData<int>("InLobby") == -4) SpawnArea = ((Vector3)Settings.DerbySpawns[0, 0]).Around(2.0f);
                        if (player.GetSharedData<int>("InLobby") > -1 && lobby != null)
                        {
                            int arenaID = player.GetSharedData<int>("InDM");
                            if (arenaID > -1)
                            {
                                int spawnID = rand.Next(Settings.DMSpawns[arenaID].GetLength(0));
                                Vector3 dmSpawn = (Vector3)Settings.DMSpawns[arenaID][spawnID, 0];
                                SpawnArea = (Vector3)dmSpawn;
                            }
                        }

                        NAPI.Player.SpawnPlayer(player, SpawnArea);
                    }
                }
                catch { }
            }, (wasInGame == true && reason != 696989) ? 15000 : 3000);
        }

        [ServerEvent(Event.ChatMessage)]
        public void OnPlayerText(Player player, string message)
        {
            player.SetData("TUA_LT", 0);

            if (!EPlayer.LoggedIn(player))
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You must be logged in to chat!");
                return;
            }

            if (EPlayer.GetAccount(player).MuteTime > 0)
            {
                player.SendChatMessage("!{#FF6347}You are currently muted for " + EPlayer.GetAccount(player).MuteReason + ", seconds left: " + EPlayer.GetAccount(player).MuteTime + ".");
                return;
            }

            string chatStr = $"{player.Name.Replace("_", " ")} says: {message}";
            foreach (Player i in RAGE.Entities.Players.All)
            {
                if (i.Dimension == player.Dimension)
                {
                    if (i.Position.DistanceTo(player.Position) <= 10.0) i.SendChatMessage($"!{{#D4D4D4}}{chatStr}");
                    if (i.Position.DistanceTo(player.Position) > 10.0 && i.Position.DistanceTo(player.Position) <= 15.0) i.SendChatMessage($"!{{#B5B5B5}}{chatStr}");
                    if (i.Position.DistanceTo(player.Position) > 15.0 && i.Position.DistanceTo(player.Position) <= 20.0) i.SendChatMessage($"!{{#999999}}{chatStr}");
                }
            }
        }

        public static void OnPlayerLogin(Player player, Account acc)
        {
            player.TriggerEvent("StopThemeSong");
            Discord.SendTextChannel("staff-bot", $"*{player.Name} has logged in as {acc.Username}.*");

            EPlayer.SetDiscordStatus(player, $"Logging in as {acc.Username}");
            NAPI.Util.ConsoleOutput($"[Login] {player.Name} has logged in as {acc.Username}. (IP: {player.Address} - Serial: {player.Serial} - SC: {player.SocialClubName})");


            BanData ban = ContextFactory.Instance.Bans.ToList().Find(ban => ban.AccountSQLID == acc.SQL_ID);

            if (ban != null)
            {
                EPlayer.SetDiscordStatus(player, "Gazing at the 'BANNED' screen.");

                player.SendChatMessage($"!{{#FF6347}}[BANNED] You are not allowed to play on this server, Ban ID: {ban.SQL_ID}");
                player.SendChatMessage("!{#A9C4E4}KevY is surprised how you made it this far though.");

                player.SendChatMessage("!{#A9C4E4}Server closed the connection.");

                NAPI.Task.Run(() =>
                {
                    try { { if (player != null && player.Exists) player.Kick("Banned."); } } catch { }
                }, 350);

                return;
            }

            player.Name = acc.Username;
            player.TriggerEvent("destroyBrowser");
            player.SetData("LoggedIn", true);
            player.SetData("AccountHandle", acc);
            player.SetSharedData("pLogged", true);

            CharacterData ConvChar = new CharacterData();

            try
            {
                if (acc.Character == null || acc.Character.Length < 10) throw (new System.Exception("Invalid characterdata-8 sending to creation."));

                ConvChar = NAPI.Util.FromJson<CharacterData>(acc.Character);
            }
            catch
            {
                player.TriggerEvent("DestroyPlayerCamera");
                player.TriggerEvent("TogglePlayerChat", true);

                EPlayer.ShowCharCreator(player);

                return;
            }

            if(acc.WeaponComponents == null || acc.WeaponComponents.Length < 4)
            {
                Dictionary<ulong, wComponentData> wepItems = new Dictionary<ulong, wComponentData>();

                wComponentData test = new wComponentData();
                test.Components.Add(123456789);
                wepItems.Add(123456789, test);

                acc.WeaponComponents = NAPI.Util.ToJson(wepItems);
            }

            EPlayer.ApplyCharacter(player, ConvChar, acc.LobbyClothing);

            EPlayer.Spawn(player);
        }


        [RemoteEvent("creator_GenderChange")]
        public static void GenderChange(Player player, int gender)
        {
            if (!EPlayer.LoggedIn(player)) return;

            if (gender == 0) player.SetSkin(PedHash.FreemodeMale01);
            else player.SetSkin(PedHash.FreemodeFemale01);

            player.Position = new Vector3(402.8664, -996.4108, -99.00027);
            player.Heading = -185.0f;
        }

        [RemoteEvent("creator_Save")]
        public void CreatorSave(Player player, byte gender, string parentData, string featureData, string appearanceData, string hairAndColorData)
        {
            if (!EPlayer.LoggedIn(player)) return;

            CharacterData pChar = new CharacterData();
            pChar.Gender = gender;
            ParentData tParentData = NAPI.Util.FromJson<ParentData>(parentData);

            pChar.Parents.Father = tParentData.Father;
            pChar.Parents.Mother = tParentData.Mother;
            pChar.Parents.Similarity = tParentData.Similarity;
            pChar.Parents.SkinSimilarity = tParentData.SkinSimilarity;

            float[] features = NAPI.Util.FromJson<float[]>(featureData);

            for (int i = 0; i < 20; i++)
            {
                try
                {
                    pChar.Features[i] = features[i];
                }
                catch { }
            }

            pChar.Appearance = NAPI.Util.FromJson<AppearanceData[]>(appearanceData);

            byte[] hairAndCol = NAPI.Util.FromJson<byte[]>(hairAndColorData);
            pChar.Hair.Hair = hairAndCol[0];
            pChar.Hair.Color = hairAndCol[1];
            pChar.Hair.HighlightColor = hairAndCol[2];
            pChar.EyebrowColor = hairAndCol[3];
            pChar.BeardColor = hairAndCol[4];
            pChar.EyeColor = hairAndCol[5];
            pChar.BlushColor = hairAndCol[6];
            pChar.LipstickColor = hairAndCol[7];
            pChar.ChestHairColor = hairAndCol[8];

            EPlayer.GetAccount(player).Character = NAPI.Util.ToJson(pChar);
            EPlayer.ApplyCharacter(player, pChar);

            player.SetData("Creator_Using", false);
            player.TriggerEvent("toggleCreator", false);

            EPlayer.Save(player);

            EPlayer.Spawn(player);

            player.TriggerEvent("StopThemeSong");

            player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You can edit your character's clothing with /char clothing.");
        }

        [RemoteEvent("creator_Leave")]
        public void CreatorLeave(Player player)
        {
            if (!EPlayer.LoggedIn(player)) return;

            CharacterData ConvChar = new CharacterData();
            string PlayerChar = EPlayer.GetAccount(player).Character;

            try
            {
                if (PlayerChar == null || PlayerChar.Length < 10) throw (new System.Exception("Invalid characterdata-7 sending to creation."));
                ConvChar = NAPI.Util.FromJson<CharacterData>(PlayerChar);

                EPlayer.ApplyCharacter(player, ConvChar);
            }
            catch
            {
                CharacterData charp = new CharacterData();
                EPlayer.GetAccount(player).Character = "{}";
                EPlayer.ApplyCharacter(player, charp); // Give them the default CHAR.
            }

            player.SetData("Creator_Using", false);
            player.TriggerEvent("toggleCreator", false);

            EPlayer.Spawn(player);
        }

        [RemoteEvent("RequestSetClothes")]
        public void OnRequestSetClothes(Player player, int slot, int drawable, int texture)
        {
            if (!EPlayer.LoggedIn(player)) return;

            Account acc = EPlayer.GetAccount(player);

            if ((slot == 1) && acc.DonatorLevel < 1)
            {
                player.SendNotification("~r~Only donators can use this itemgroup (masks).");
                return;
            }

            if (player.GetData<int>("EditingClothing") != -1)
            {
                try
                {
                    if (acc.Character == null || acc.Character.Length < 10) throw (new System.Exception("Invalid characterdata-6 edit clothes."));

                    CharacterData charData = NAPI.Util.FromJson<CharacterData>(acc.Character);

                    charData.Clothing[player.GetData<int>("EditingClothing")].Clothes[slot].Drawable = drawable;
                    charData.Clothing[player.GetData<int>("EditingClothing")].Clothes[slot].Texture = texture;

                    acc.Character = NAPI.Util.ToJson(charData);
                }
                catch { }
            }

            player.SetClothes(slot, drawable, texture);

        }

        [RemoteEvent("RequestSetGlasses")]
        public void OnRequestSetGlasses(Player player, int drawable, int texture)
        {
            if (!EPlayer.LoggedIn(player)) return;

            if (player.GetData<int>("EditingClothing") != -1)
            {
                Account acc = EPlayer.GetAccount(player);
                try
                {
                    if (acc.Character == null || acc.Character.Length < 10) throw (new System.Exception("Invalid characterdata-5 edit props."));

                    CharacterData charData = NAPI.Util.FromJson<CharacterData>(acc.Character);

                    charData.Clothing[player.GetData<int>("EditingClothing")].Glasses = drawable;
                    charData.Clothing[player.GetData<int>("EditingClothing")].GlassesTxt = texture;

                    acc.Character = NAPI.Util.ToJson(charData);
                }
                catch { }
            }

            player.SetAccessories(1, drawable, texture);
        }

        [RemoteEvent("RequestSetHat")]
        public void OnRequestSetHat(Player player, int drawable, int texture)
        {
            if (!EPlayer.LoggedIn(player)) return;

            if (player.GetData<int>("EditingClothing") != -1)
            {
                Account acc = EPlayer.GetAccount(player);
                try
                {
                    if (acc.Character == null || acc.Character.Length < 10) throw (new System.Exception("Invalid characterdata-4 edit props."));

                    CharacterData charData = NAPI.Util.FromJson<CharacterData>(acc.Character);

                    charData.Clothing[player.GetData<int>("EditingClothing")].Hat = drawable;
                    charData.Clothing[player.GetData<int>("EditingClothing")].HatTxt = texture;

                    acc.Character = NAPI.Util.ToJson(charData);
                }
                catch { }
            }

            player.SetAccessories(0, drawable, texture);
        }

        [RemoteEvent("UpdateTabbedState")]
        public void OnRequestUpdateTabState(Player player, bool toggle)
        {
            if (!EPlayer.LoggedIn(player)) return;

            //player.SetSharedData("IsTabbed", toggle);
        }

        [RemoteEvent("UpdateChatState")]
        public void OnRequestUpdateChatState(Player player, bool toggle)
        {
            if (!EPlayer.LoggedIn(player)) return;

            player.SetData("TUA_LT", 0);
        }

        public static void OnPlayerRegister(Player player, Account acc)
        {
            Discord.SendTextChannel("staff-bot", $"*{player.Name} has registered.*");

            EPlayer.SetDiscordStatus(player, $"Registering as {acc.Username}");
            NAPI.Util.ConsoleOutput($"[Connection] {player.Name} has registered as {acc.Username}. (IP: {player.Address} - Serial: {player.Serial} - SC: {player.SocialClubName})");

            player.SetData("LoggedIn", true);
            player.SetData("AccountHandle", acc);
            player.SetSharedData("pLogged", true);

            player.Name = acc.Username;
            player.TriggerEvent("destroyBrowser");

            player.TriggerEvent("DestroyPlayerCamera");
            player.TriggerEvent("TogglePlayerChat", true);

            EPlayer.ShowCharCreator(player);

            //EPlayer.Spawn(player);
        }

        [RemoteEvent("OnPlayerLoginAttempt")]
        public static void OnPlayerLoginAttempt(Player player, string username, string password)
        {
            if ((username == null || username.Length < 2) || (password == null || password.Length < 2)) return;

            NAPI.Util.ConsoleOutput($"[Login Attempt] Username {username}");
            if (Environment.TickCount64 - player.GetData<long>("LastCryptAttempt") < 1000 || EPlayer.LoggedIn(player))
            {
                player.SendNotification("~r~Please wait some time between each login attempt!", true);
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Please wait some time between each login attempt!");
                return;
            }
            player.SetData("LastCryptAttempt", Environment.TickCount64);
            if (username.Contains("@"))
            {
                Account acc = ContextFactory.Instance.Accounts.FirstOrDefault(a => a.Email == username);
                if (acc != null)
                {
                    if (AccountManager.Login(acc, password))
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
            player.SendNotification("~r~Invalid credentials or unable to log in!", true);
            player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Invalid credentials or unable to log in!");

        }

        [RemoteEvent("UpdateStreamData")]
        public void OnPlayerUpdateStreamData(Player player)
        {
            if (player.IsInVehicle) player.SetSharedData("UnstreamedState", 1);
            else player.SetSharedData("UnstreamedState", 0);

            if (player.GetSharedData<int>("Team") == 0 && player.GetSharedData<int>("InLobby") > 0 && player.IsInVehicle && player.Vehicle.HasSharedData("Team"))
            {
                if (player.Vehicle.GetSharedData<int>("Team") == 1)
                {
                    player.SetSharedData("VisibleBlip", true);
                }
            }

            player.SetSharedData("UnstreamedX", player.Position.X);
            player.SetSharedData("UnstreamedY", player.Position.Y);
            player.SetSharedData("UnstreamedZ", player.Position.Z);

            if (EPlayer.LoggedIn(player) && player.GetSharedData<int>("InLobby") == -4)
            {
                if (player.Position.Z <= 3.0f && player.IsInstanceAlive && !player.Dead && player.Health > 10)
                {
                    player.Health = 0;
                }

                if (Environment.TickCount64 - player.GetData<long>("DerbySpawnTick") > 2000)
                {
                    if (player.IsInstanceAlive && !player.Dead && player.Health > 10 && !player.IsInVehicle)
                    {
                        player.Health = 0;
                        player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Please do not get out of your vehicle in derby!");
                    }
                }
            }

            return;
        }

        [RemoteEvent("SentLastSeen")]
        public void OnReceiveLastSeen(Player player, string streetName, string zoneName)
        {
            if (!EPlayer.LoggedIn(player) || (player.HasData("VisibleBlip") && player.GetSharedData<bool>("VisibleBlip") == true))
                return;

            bool isGood = false;
            LobbyData lobby = Settings.Lobbies.Find(lobby => lobby.lobbyId == player.GetSharedData<int>("InLobby"));
            if (lobby != null)
            {
                if (lobby.Fugitives.Contains(player) && lobby.gameStatus == 1)
                {
                    isGood = true;
                }
            }

            if (!isGood)
            {
                return;
            }

            string chatStr = $"!{{#FFEC8B}}** [RADIO | FREQ: 911] DISPATCH: Felon {player.Name} last spotted at {streetName}, {zoneName}";

            lobby.Cops.ForEach((Player cop) =>
            {
                cop.SendChatMessage(chatStr);
            });
            return;
        }

        [RemoteEvent("GivePlayerShotTag")]
        public void PlayerRequestShotTag(Player player)
        {
            if (player.GetSharedData<int>("Team") == 0)
            {
                player.SetSharedData("HasShot", true);
                player.SendNotification("~r~You have now shot, opposite team can now safely shoot you and detect you.");
                player.SetSharedData("VisibleBlip", true);
            }
        }

        [RemoteEvent("PlayerShotAtWrongPlayer")]
        public void PlayerShootWrongPlayer(Player player, int caseid, Player target)
        {
            if (caseid == 0 && (false == true)) // DISABLING THIS BECAUSE DAMAGE IS DISABLED NOW
            {
                if (RAGE.Entities.Players.All.Count(ey => ey.GetSharedData<int>("Team") != -1 && ey.GetSharedData<int>("Team") != player.GetSharedData<int>("Team") &&
                        ey.Position.DistanceTo(player.Position) < 10.0) == 0)
                {
                    int warnings = 0;
                    if (player.HasData("TeamShotWarning")) warnings = player.GetData<int>("TeamShotWarning");

                    if (warnings >= 3)
                    {
                        EPlayer.Freeze(player, true);

                        NAPI.Task.Run(() =>
                        {
                            try
                            {
                                if (player != null && player.Exists)
                                {
                                    if (!player.IsNull && player != null && player.IsInstanceAlive)
                                    {
                                        EPlayer.Freeze(player, false);
                                    }
                                }
                            }
                            catch { }
                        }, player.IsInVehicle == true ? 500 : 2500);
                    }


                    warnings++;
                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Do not shoot at teammates, repeatedly doing this will result in punishment!");

                    if (warnings >= 7)
                    {
                        player.Health = 0;
                        EPlayer.GrantXP(player, -1);
                        player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You have been killed and lost 1XP due to team attacking.");
                        warnings = 0;
                    }

                    player.SetData("TeamShotWarning", warnings);
                }

                /* // Heal fellow COP as long as there's no fugitive nearby that hasn't shot.
                if ( (target.GetSharedData<bool>("HasShot") == true && target.GetSharedData<int>("Team") == 0) == false) // Not a fugitive that has shot.
                {
                    // So any fugitive that hasn't shot got TK'd or lost hp somehow can has his hp reset to 100.


                    if (RAGE.Entities.Players.All.Count(a => a.Position.DistanceTo(target.Position) < 100.0f &&
                        a.GetSharedData<bool>("HasShot") == true && a.GetSharedData<int>("Team") == 0 && a.Dimension == target.Dimension && a != target) == 0)
                    { // No nearby "has shot" fugitive's to the target so they can't have healing advantage.
                        target.Health = 100;
                    }
                }*/
            }
            else if (caseid == 1 && player.CurrentWeapon != WeaponHash.Stungun && (false == true)) // DISABLING THIS CUZ DAMAGE IS DISABLED NOW
            {

                if (!target.IsInVehicle || RAGE.Entities.Players.All.Count(ey => ey.IsInVehicle && ey.Vehicle == target.Vehicle &&
                                 ey.GetSharedData<int>("Team") == 0 && ey.GetSharedData<bool>("HasShot") == true) == 0)
                {

                    int warnings = 0;
                    if (player.HasData("InnocentShotWarning")) warnings = player.GetData<int>("InnocentShotWarning");

                    if (warnings >= 3)
                    {
                        EPlayer.Freeze(player, true);
                        NAPI.Task.Run(() =>
                        {
                            try
                            {
                                if (player != null && player.Exists)
                                {
                                    if (!player.IsNull && player != null && player.IsInstanceAlive)
                                    {
                                        EPlayer.Freeze(player, false);
                                    }
                                }
                            }
                            catch { }
                        }, player.IsInVehicle == true ? 500 : 2500);
                    }

                    warnings++;
                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Do not shoot at fugitives that haven't shot, repeatedly doing this will result in punishment!");

                    if (warnings >= 7)
                    {
                        player.Health = 0;
                        EPlayer.GrantXP(player, -1);
                        player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You have lost 1XP due to shooting at a fugitive without shot tag.");
                        warnings = 0;
                    }

                    player.SetData("InnocentShotWarning", warnings);
                }

                //target.Health = 100;
            }
        }

        [RemoteEvent("OnPlayerRegisterAttempt")]
        public static void OnPlayerRegisterAttempt(Player player, string email, string username, string password, string confirmpass)
        {
            if ( (email == null || email.Length < 2) || (username == null || username.Length < 2) || (password == null || password.Length < 2)) return;

            NAPI.Util.ConsoleOutput($"[Register Attempt] Username {username}");
            if (Environment.TickCount64 - player.GetData<long>("LastCryptAttempt") < 1000 || EPlayer.LoggedIn(player))
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Please wait some time between each register attempt!");
                player.SendNotification("~r~Please wait some time between each register attempt!", true);
                return;
            }
            if (!password.Equals(confirmpass))
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Passwords do not match!");
                player.SendNotification("~r~Passwords do not match!", true);
                return;
            }
            if (password.Length < 5)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Your password is too small!");
                player.SendNotification("~r~Password is too small!", true);
                return;
            }
            if (username.Contains("@") || username.Length < 4)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Your username is too small or it cannot contain the character '@'");
                player.SendNotification("~r~Username small or contains illegal characters!", true);
                return;
            }
            Regex exp = new Regex(@"^[a-zA-Z0-9]*$");
            Match match = exp.Match(username);
            if (!match.Success)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Your username can only contain alphabets or numbers!");
                player.SendNotification("~r~Username contains illegal characters!", true);
                return;
            }
            if (!email.Contains("@") || email.Length < 5)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Your email is invalid or too small!");
                player.SendNotification("~r~Invalid email!", true);
                return;
            }

            Account acc = ContextFactory.Instance.Accounts.FirstOrDefault(a => a.Username.Equals(username, StringComparison.OrdinalIgnoreCase) == true);
            if (acc != null)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}An account with this name already exists!");
                player.SendNotification("~r~Account with this name already exists!", true);
                return;
            }

            /*acc = ContextFactory.Instance.Accounts.FirstOrDefault(a => a.Email == email);
            if (acc != null)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}An account with this email already exists!");
                player.SendNotification("~r~Account with this email already exists!", true);
                return;
            }*/

            if (ContextFactory.Instance.Accounts.Count(a => a.HWID.Equals(player.Serial) || a.RockstarID == player.SocialClubId) >= 2)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You can only have TWO accounts per computer or rockstar ID!");
                player.SendNotification("~r~You have more than two accounts on the same computer/game serial!", true);
                return;
            }

            player.SetData("LastCryptAttempt", Environment.TickCount64);
            string salt = AccountManager.GenSalt();
            string newPass = AccountManager.Crypt(password, salt);

            Dictionary<ulong, wComponentData> wepItems = new Dictionary<ulong, wComponentData>();

            wComponentData test = new wComponentData();
            test.Components.Add(123456789);
            wepItems.Add(123456789, test);

            var _new = new Account
            {
                Email = email,
                Password = newPass,
                Salt = salt,
                Username = username,
                Admin = 0,
                HWID = player.Serial,
                MuteTime = 0,
                MuteReason = "N/A",
                JailTime = 0,
                JailReason = "N/A",
                Level = 1,
                XP = 1,
                Warns = 0,
                Suspended = 0,
                DonatorLevel = 0,
                RockstarID = player.SocialClubId,
                LobbyClothing = 1,
                LoadoutCop = "?",
                LoadoutFugitive = "?",
                CarCop = "?",
                WeaponComponents = NAPI.Util.ToJson(wepItems),
                Character = "{}",
            };
            AccountManager.Add(_new);
            OnPlayerRegister(player, _new);
        }

        [RemoteEvent("char_ResetEcho")]
        public void OnCharacterReset(Player player)
        {
            if (player.HasData("LastResetEchoEvent") && player.GetData<int>("LastResetEchoEvent") == 1)
            {
                EPlayer._ApplyCharacter(player, player.GetData<CharacterData>("ApplyChar"), player.GetData<int>("ApplyCharTeam"));
            }

            player.SetData("LastResetEchoEvent", 0);
        }

        [RemoteEvent("OnCarSelectorSelect")]
        public void OnSelectorSelectCar(Player player, int team, string carname)
        {
            if (!EPlayer.LoggedIn(player)) return;

            if (player.HasData("ViewingCarSelector") && player.GetData<bool>("ViewingCarSelector") == true)
            {
                if (team == 1)
                {
                    switch (carname)
                    {
                        case "Default":
                            {
                                EPlayer.GetAccount(player).CarCop = "?";
                                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Cop car set to Random/Default.");
                                break;
                            }
                        case "Police Cruiser":
                            {
                                EPlayer.GetAccount(player).CarCop = "police";
                                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Cop car set to Police Cruiser.");
                                break;
                            }
                        case "Police Cruiser (U)":
                            {
                                EPlayer.GetAccount(player).CarCop = "police4";
                                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Cop car set to Unmarked Cruiser.");
                                break;
                            }
                        case "Police Bike":
                            {
                                EPlayer.GetAccount(player).CarCop = "policeb";
                                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Cop car set to Police Bike.");
                                break;
                            }
                        case "Police Buffalo":
                            {
                                EPlayer.GetAccount(player).CarCop = "polbufac";
                                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Cop car set to Police Buffalo.");
                                break;
                            }
                        case "Police Buffalo (U)":
                            {
                                EPlayer.GetAccount(player).CarCop = "polbufac2";
                                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Cop car set to Unmarked Police Buffalo.");
                                break;
                            }
                        case "FBI Buffalo":
                            {
                                EPlayer.GetAccount(player).CarCop = "fbibufac";
                                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Cop car set to FBI Buffalo.");
                                break;
                            }
                        case "FBI Granger":
                            {
                                EPlayer.GetAccount(player).CarCop = "fbi2";
                                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Cop car set to FBI Granger.");
                                break;
                            }
                        case "Chopper":
                            {
                                EPlayer.GetAccount(player).CarCop = "polmav";
                                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Cop car set to Police Chopper.");
                                break;
                            }
                        case "SWAT Truck":
                            {
                                EPlayer.GetAccount(player).CarCop = "riot";
                                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Cop car set to SWAT Truck.");
                                break;
                            }
                        case "Charger '16 (U)":
                            {
                                EPlayer.GetAccount(player).CarCop = "udchall1";
                                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Cop car set to Unmarked Dodge Charger 2016.");
                                break;
                            }
                        case "Police Caracara":
                            {
                                EPlayer.GetAccount(player).CarCop = "lspdcara";
                                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Cop car set to Police Caracara.");
                                break;
                            }
                    }
                }
            }
            player.ResetData("ViewingCarSelector");
            return;
        }

        [RemoteEvent("OnLoadoutSelectorSelect")]
        public void OnSelectorSelectLoadout(Player player, int team, string ldname)
        {
            if (!EPlayer.LoggedIn(player)) return;

            if (player.HasData("ViewingLoadoutSelector") && player.GetData<bool>("ViewingLoadoutSelector") == true)
            {
                if(team == 0)
                {
                    if (ldname.StartsWith("default", StringComparison.OrdinalIgnoreCase))
                    {
                        EPlayer.GetAccount(player).LoadoutFugitive = "?";
                        player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Your loadout for team fugitive has been set to 'default'.");
                        return;
                    }
                    LoadoutData loadout = Settings.Loadouts.Find(b => b.Team == 0 && b.Name.Equals(ldname, StringComparison.OrdinalIgnoreCase));

                    if ((loadout != null) == false)
                    {
                        player.SendChatMessage("!{#999999}Failed to find a loadout with the specified name. Are you sure that's a fugitive/valid loadout?");
                        return;
                    }

                    if (loadout.LevelRequired > EPlayer.GetAccount(player).Level)
                    {
                        player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You don't have the required level (" + loadout.LevelRequired.ToString() + ") " + "for this loadout.");
                        return;
                    }

                    EPlayer.GetAccount(player).LoadoutFugitive = loadout.Name;
                    player.SendChatMessage("!{#33C446}[#] !{#FFFFFF}You have set your fugitive loadout to " + loadout.Name + ".");
                }
                else if(team == 1)
                {
                    if (ldname.StartsWith("default", StringComparison.OrdinalIgnoreCase))
                    {
                        EPlayer.GetAccount(player).LoadoutCop = "?";
                        player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Your loadout for team cop has been set to 'default'.");
                        return;
                    }
                    LoadoutData loadout = Settings.Loadouts.Find(b => b.Team == 1 && b.Name.Equals(ldname, StringComparison.OrdinalIgnoreCase));

                    if ((loadout != null) == false)
                    {
                        player.SendChatMessage("!{#999999}Failed to find a loadout with the specified name. Are you sure that's a cop/valid loadout?");
                        return;
                    }

                    if (loadout.LevelRequired > EPlayer.GetAccount(player).Level)
                    {
                        player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You don't have the required level (" + loadout.LevelRequired.ToString() + ") " + "for this loadout.");
                        return;
                    }

                    EPlayer.GetAccount(player).LoadoutCop = loadout.Name;
                    player.SendChatMessage("!{#33C446}[#] !{#FFFFFF}You have set your cop loadout to " + loadout.Name + ".");
                }
            }
            player.ResetData("ViewingLoadoutSelector");
            return;
        }

        [RemoteEvent("RequestCounterUpdate")]
        public void OnPlayerRequestCounterUpdate(Player player, int currentTime, int type)
        {
            if (!EPlayer.LoggedIn(player)) return;

            if (type == 0)
            {
                if (player.GetSharedData<int>("JailTime") <= 0) return;

                Account acc = EPlayer.GetAccount(player);
                acc.JailTime = currentTime;

                player.SetSharedData("JailTime", acc.JailTime);

                if (currentTime <= 0)
                {
                    EPlayer.Freeze(player, false);
                    EPlayer.Spawn(player);
                    ContextFactory.Instance.SaveChanges();
                    player.SendChatMessage("!{#00FF33}You have been unjailed as you have served your time.");
                }
            }
            else if (type == 1)
            {
                if (player.GetSharedData<int>("MuteTime") <= 0) return;

                Account acc = EPlayer.GetAccount(player);
                acc.MuteTime = currentTime;

                player.SetSharedData("MuteTime", acc.MuteTime);

                if (currentTime <= 0)
                {
                    ContextFactory.Instance.SaveChanges();
                    player.SendChatMessage("!{#00FF33}You have been unmuted as you have served your time.");
                }
            }
            return;
        }

        [RemoteEvent("opsp")] // wont trigger incar btw 
        public void OnPlayerShootPlayer(Player player, Player target, uint bone, UInt64 weapon)
        {
            if (weapon == (uint)WeaponHash.Stungun)
            {
                if (target.GetSharedData<bool>("HasShot") != true && player.Position.DistanceTo(target.Position) <= 32.0 && !target.IsInVehicle)
                {
                    LobbyData lobby = Settings.Lobbies.Find(lob => lob.lobbyId == player.GetSharedData<int>("InLobby"));
                    if (lobby != null)
                    {
                        if (lobby.Cops.Contains(player) && lobby.Cops.Contains(target))
                        {
                            if (RAGE.Entities.Players.All.Count(ey => lobby.Fugitives.Contains(ey) &&
                                ey.Position.DistanceTo(player.Position) < 10.0) == 0)
                            {
                                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Do not tase your own teammates! You have been given a teamattack warning.");
                                EPlayer.Freeze(player, true);

                                NAPI.Task.Run(() =>
                                {
                                    try
                                    {
                                        if (player != null && player.Exists)
                                        {
                                            if (!player.IsNull && player != null && player.IsInstanceAlive)
                                            {
                                                EPlayer.Freeze(player, false);
                                            }
                                        }
                                    }
                                    catch { }
                                }, 1500);

                                int warnings = 0;
                                if (player.HasData("TeamShotWarning")) warnings = player.GetData<int>("TeamShotWarning");

                                warnings++;

                                if (warnings >= 4)
                                {
                                    player.Health = 0;
                                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You have been killed for team tasing!");
                                    warnings = 0;
                                }

                                player.SetData("TeamShotWarning", warnings);
                            }
                            return;
                        }
                    }

                    if (target.HasData("TasedTill") && Environment.TickCount64 - (target.GetData<long>("TasedTill") - 30000) >= 2000)
                    {
                        int time = 30000;
                        target.TriggerEvent("setToRagdoll", time, time * 2, 0, false, false, false);
                        target.SetData("TasedTill", Environment.TickCount64 + 30000);
                        Commands.cmd_do(player, $"{target.Name} has been tased and has been rendered immobile.");
                    }
                }
                else
                {
                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot tase a player that has shot.");
                }
            }
        }

        [RemoteEvent("DerbyIncreaseCount")]
        public static void OnIncreaseDerbyCount(Player player)
        {
            if (!EPlayer.LoggedIn(player)) return;
            if (player.GetSharedData<int>("InLobby") != -4) return;

            if(Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == -4).Count() < 2)
            {
                player.SetSharedData("DerbyTime", 0);
            }
            else
            {
                int highestDerbyTime = 0;
                foreach (Player p in Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == -4))
                {
                    if(p.GetSharedData<int>("DerbyTime") > highestDerbyTime) highestDerbyTime = p.GetSharedData<int>("DerbyTime");
                }
                Settings.DerbyTimeLeft = 180 - highestDerbyTime;
            }

            foreach (Player p in Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == -4))
            {
                if (Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == -4).Count() >= 2)
                {
                    p.TriggerEvent("SetDerbyCounting", true, Settings.DerbyTimeLeft);
                }
            }

            player.SetSharedData("DerbyTime", player.GetSharedData<int>("DerbyTime") + 1);

            if (Environment.TickCount64 - player.GetData<long>("DerbySpawnTick") > 4000)
            {
                if (!player.Dead && player.Health > 10 && !player.IsInVehicle)
                {
                    player.Health = 0;
                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Please do not get out of your vehicle in derby!");
                }
            }

            if (Environment.TickCount64 - Settings.LastDerbyTick >= 1000 && Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == -4).Count() >= 2)
            {
                Settings.DerbyTimeLeft--;
                Settings.LastDerbyTick = Environment.TickCount64;

                if (Settings.DerbyTimeLeft <= 0)
                {
                    Main.CheckDerbyWinner();
                }
            }
            return;
        }

        [RemoteEvent("BustProgress")]
        public void OnPlayerBustProgress(Player cop, Player fugitive, float distance) // Triggers every 100ms a cop is near fugitive
        {
            if (!EPlayer.LoggedIn(cop) || !EPlayer.LoggedIn(fugitive)) return;

            if (fugitive.GetSharedData<float>("BustedProgress") == -1.0) return;

            LobbyData lobby = Settings.Lobbies.Find(lobby => lobby.lobbyId == cop.GetSharedData<int>("InLobby") && lobby.gameMode == GameModes.BUSTED);
            if (lobby == null) return;

            if (!lobby.Cops.Contains(cop) || !lobby.Fugitives.Contains(fugitive)) return;

            List<ushort> bustSupporters = fugitive.HasData("BustSupporters") ? fugitive.GetData<List<ushort>>("BustSupporters") : new List<ushort>();

            if (!bustSupporters.Contains(cop.Id)) bustSupporters.Add(cop.Id);

            fugitive.SetData("BustSupporters", bustSupporters);

            double rate = 0.5;

            if (distance <= 3.5) rate = 1.0;
            else if (distance <= 5.0) rate = 0.75;
            else if (distance <= 10.0) rate = 0.5;

            fugitive.SetSharedData("BustedProgress", fugitive.GetSharedData<float>("BustedProgress") + rate);

            if (fugitive.GetSharedData<float>("BustedProgress") >= 100.0)
            {
                lobby.Fugitives.Remove(fugitive);
                fugitive.SetSharedData("BustedProgress", -1.0);

                foreach (Player assist in Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == lobby.lobbyId))
                {
                    if (!bustSupporters.Contains(assist.Id) || assist.Id == cop.Id) continue;

                    EPlayer.GrantXP(assist, 5);
                    assist.SendNotification("~g~+5 XP for helping bust " + fugitive.Name);
                }

                EPlayer.GrantXP(cop, 7);
                cop.SendNotification("~g~+7 XP for busting " + fugitive.Name);

                EPlayer.SendMessageInLobby(lobby.lobbyId, $"!{{#ADD8E6}}Fugitive {fugitive.Name} has been busted!");

                Events.OnPlayerDeath(fugitive, cop, 696989); // Reason 696989 = handcuffs/busted (fast respawn).
            }
            return;
        }

        [RemoteEvent("BustRegress")]
        public void OnPlayerBustRegress(Player fugitive, float closestCop) // Triggers every 100ms a fugitive is without cop and has progress
        {
            if (!EPlayer.LoggedIn(fugitive)) return;

            if (fugitive.GetSharedData<float>("BustedProgress") == -1.0) return;
            
            LobbyData lobby = Settings.Lobbies.Find(lobby => lobby.lobbyId == fugitive.GetSharedData<int>("InLobby"));
            if (lobby == null) return;

            if (!lobby.Fugitives.Contains(fugitive) || lobby.gameMode != GameModes.BUSTED) return;


            double rate = 1.0;
            if (closestCop >= 30.0) rate = 2.5;
            else if (closestCop <= 10.0) rate = 0.5;
            else if (closestCop <= 15.0) rate = 1.0;

            fugitive.SetSharedData("BustedProgress", fugitive.GetSharedData<float>("BustedProgress") - rate);

            if (fugitive.GetSharedData<float>("BustedProgress") <= 1.0)
            {
                fugitive.ResetData("BustSupporters");
                fugitive.SetSharedData("BustedProgress", 0.0);
            }
            return;
        }

        [ServerEvent(Event.PlayerSpawn)]
        public static void OnPlayerSpawn(Player player)
        {
            lock (player)
            {
                if (EPlayer.GetAccount(player).MuteTime > 0)
                {
                    Account acc = EPlayer.GetAccount(player);

                    player.SetSharedData("MuteTime", acc.MuteTime);
                    player.TriggerEvent("SetMuteCounter", acc.MuteTime);
                }

                player.SetSharedData("BustedProgress", 0.0);
                player.ResetData("CuffingTarget");

                if (player.HasData("Cuffed"))
                {
                    player.TriggerEvent("SetHandcuffs", false);

                    CharacterData ConvChar = new CharacterData();

                    Account acc = EPlayer.GetAccount(player);
                    try
                    {
                        if (acc.Character == null || acc.Character.Length < 10) throw (new System.Exception("Invalid characterdata-9 sending to creation."));

                        ConvChar = NAPI.Util.FromJson<CharacterData>(acc.Character);
                    }
                    catch
                    {
                        ConvChar = new CharacterData();
                    }

                    EPlayer.ApplyCharacter(player, ConvChar, acc.LobbyClothing);
                    player.ResetData("Cuffed");
                }
                if (player.HasData("Handsup"))
                {
                    player.TriggerEvent("SetHandsup", false);
                    player.ResetData("Handsup");
                }

                player.SetSharedData("Team", -1);

                LobbyData lobby = Settings.Lobbies.Find(lobby => lobby.lobbyId == player.GetSharedData<int>("InLobby"));
                if (lobby != null)
                {
                    if (lobby.Cops.Contains(player)) player.SetSharedData("Team", 1);
                    else if (lobby.Fugitives.Contains(player)) player.SetSharedData("Team", 0);
                }

                if (!EPlayer.LoggedIn(player))
                {
                    EPlayer.LoginCameraSetup(player);
                    EPlayer.LoginBrowserSetup(player);
                    return;
                }

                if (player.HasData("Creator_Using") && player.GetData<bool>("Creator_Using") == true)
                {
                    player.SetSharedData("InLobby", -5);
                    return;
                }


                player.ResetData("FreezeWeapons");

                player.SetSharedData("DerbyTime", 0);

                player.SetSharedData("Level", EPlayer.GetAccount(player).Level);
                player.SetSharedData("Ping", player.Ping);
                player.SetSharedData("HasShot", false);

                if (EPlayer.GetAccount(player).JailTime > 0)
                {
                    EPlayer.SetDiscordStatus(player, $"In jail as {player.Name}");

                    Account acc = EPlayer.GetAccount(player);

                    player.SetData("AutoJoin", false);
                    player.SetSharedData("InLobby", -2);
                    player.SetSharedData("JailTime", acc.JailTime);
                    player.TriggerEvent("SetJailCounter", acc.JailTime);
                    EPlayer.SetColor(player, new Color(168, 128, 8, 255));
                    player.Position = (Vector3)Settings.VectorJail[0];
                    player.Heading = (float)Settings.VectorJail[1];
                    player.Dimension = 200 + Convert.ToUInt32(player.Id);
                    EPlayer.Freeze(player, true);
                    player.SendChatMessage($"!{{#FF6347}}[JAILED] !{{#FFFFFF}}You are in jail for {acc.JailTime} seconds, reason: {acc.JailReason}.");

                    return;
                }


                if (player.GetData<bool>("UnfairKillSpawn") == true)
                {
                    player.SetData("UnfairKillSpawn", false);
                    return;
                }

                player.TriggerEvent("HideChaseTimer");
                player.TriggerEvent("SetTimeWeather", 12, 0, 0, -1);

                player.SetSharedData("VisibleBlip", false);

                player.TriggerEvent("SetActorInvulnerable", true);
                player.TriggerEvent("CanFallOutOfCar", false);
                player.TriggerEvent("showHPBars", 0); // Never show HP Bars.

                EPlayer.SetColor(player, new Color(255, 255, 255, 255));


                player.TriggerEvent("ToggleHud", true);



                if (player.GetSharedData<int>("InLobby") == -3) // Freeroam
                {
                    player.Position = new Vector3(1061.547f, -720.113f, 56.786f);
                    player.Heading = 130.497f;
                    EPlayer.SetDiscordStatus(player, $"Freeroam as {player.Name}");

                    EPlayer.SetColor(player, new Color(102, 232, 217), 0);

                    player.TriggerEvent("SetActorInvulnerable", true);

                    player.SetSharedData("VisibleBlip", true);

                    player.TriggerEvent("SetTimeWeather", player.GetData<int>("FreeroamTime"), 0, 0, player.GetData<int>("FreeroamWeather"));

                    player.RemoveAllWeapons();

                    player.Dimension = 256;
                }

                if (player.GetSharedData<int>("InLobby") == -4) // Derby
                {
                    player.TriggerEvent("SetTimeWeather", 12, 0, 0, 0);
                    player.TriggerEvent("showHPBars", 0); // Never show HP Bars.

                    player.SetData("DerbySpawnTick", Environment.TickCount64);

                    Random rand = new Random();
                    int spawnID = rand.Next(Settings.DerbySpawns.GetLength(0));

                    if (player.HasData("DerbyVehicle"))
                    {
                        try { player.GetData<Vehicle>("DerbyVehicle").Delete(); }
                        catch { }

                        player.ResetData("DerbyVehicle");
                    }

                    Vector3 PlayerSpawn = (Vector3)Settings.DerbySpawns[spawnID, 0];
                    PlayerSpawn.Z += 5.0f;
                    player.Position = PlayerSpawn;
                    player.Heading = (float)Settings.DerbySpawns[spawnID, 1];
                    player.Dimension = 500;


                    EPlayer.SetDiscordStatus(player, $"Derby as {player.Name}");

                    player.TriggerEvent("SetActorInvulnerable", true);

                    player.SetSharedData("VisibleBlip", true);

                    player.RemoveAllWeapons();

                    player.TriggerEvent("SetDerbyCounting", false, Settings.DerbyTimeLeft);

                    if (Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == -4).Count() >= 2)
                    {
                        foreach (Player p in Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == -4))
                        {
                            p.TriggerEvent("SetDerbyCounting", true, Settings.DerbyTimeLeft);
                        }
                    }
                    else
                    {
                        player.SendNotification("~r~Derby time will not be counted unless there are enough players.");
                        Settings.DerbyTimeLeft = 180;
                        Settings.LastDerbyTick = Environment.TickCount64;
                    }

                    Vehicle derbyVeh = NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey("dominator"), (Vector3)Settings.DerbySpawns[spawnID, 0], (float)Settings.DerbySpawns[spawnID, 1], rand.Next(255), rand.Next(255), "DERBY", 255, false, true, 500);
                    player.SetData("DerbyVehicle", derbyVeh);

                    player.SetIntoVehicle(derbyVeh, 0);

                    NAPI.Task.Run(() =>
                    {
                        try
                        {
                            if (player != null && player.Exists)
                            {
                                player.SetIntoVehicle(derbyVeh, 0);
                                EPlayer.SetColor(player, new Color(250, 191, 62), 0);
                            }
                        }
                        catch { }
                    }, 350);

                    NAPI.Task.Run(() =>
                    {
                        try
                        {
                            if (player != null && player.Exists)
                            {
                                player.SetIntoVehicle(derbyVeh, 0);
                                EPlayer.SetColor(player, new Color(250, 191, 62), 0);
                            }
                        }
                        catch { }
                    }, 750);

                    NAPI.Task.Run(() =>
                    {
                        try
                        {
                            if (player != null && player.Exists)
                            {
                                player.SetIntoVehicle(derbyVeh, 0);
                                EPlayer.SetColor(player, new Color(250, 191, 62), 0);
                            }
                        }
                        catch { }
                    }, player.Ping + 50);
                }

                if (player.GetSharedData<int>("InLobby") > -1) // Copchase Lobby
                {
                    EPlayer.SetDiscordStatus(player, $"[Lobby {player.GetSharedData<int>("InLobby")}] Waiting as {player.Name}");

                    player.RemoveAllWeapons();

                    player.Dimension = 100 + Convert.ToUInt32(player.GetSharedData<int>("InLobby"));

                    if (lobby != null)
                    {
                        player.TriggerEvent("SetTimeWeather", lobby.Time[0], lobby.Time[1], lobby.Time[2], lobby.Weather);

                        if (lobby.gameStatus == 1)
                        {
                            player.TriggerEvent("StartChaseTimer", lobby.GameProgress, Settings.GameRunTime, lobby.CollisionCounter, (int)lobby.gameMode);
                        }
                    }

                    int arenaID = player.GetSharedData<int>("InDM");
                    if (arenaID > -1 && lobby != null && lobby.gameStatus == 2)
                    {
                        arenaID = -1;
                        player.SetSharedData("InDM", -1);
                        player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You have been removed from DM as it's locked while copchase starts.");
                    }

                    if (arenaID > -1)
                    {
                        Random rand = new Random();

                        int spawnID = rand.Next(Settings.DMSpawns[arenaID].GetLength(0));
                        Vector3 dmSpawn = (Vector3)Settings.DMSpawns[arenaID][spawnID, 0];
                        player.Position = dmSpawn;
                        player.Heading = (float)Settings.DMSpawns[arenaID][spawnID, 1];

                        player.Health = 100;
                        player.Armor = 100;

                        player.TriggerEvent("SetActorInvulnerable", false);
                        player.SetSharedData("VisibleBlip", true);
                        player.TriggerEvent("showHPBars", 2); // Show if AIMing

                        player.Dimension = (uint)arenaID + 356 + Convert.ToUInt32(player.GetSharedData<int>("InLobby"));

                        EPlayer.SetDiscordStatus(player, $"[Lobby {player.GetSharedData<int>("InLobby")}] DM '{Settings.DMNames[arenaID]}' as {player.Name}");
                        EPlayer.SetColor(player, new Color(179, 179, 179), 100);

                        NAPI.Task.Run(() =>
                        {
                            try
                            {
                                if (player != null && player.Exists)
                                {
                                    switch (new Random().Next(0, 5))
                                    {
                                        case 0:
                                            player.GiveWeapon(WeaponHash.Revolver_mk2, 1000);
                                            player.GiveWeapon(WeaponHash.Pumpshotgun, 1000);
                                            player.GiveWeapon(WeaponHash.Smg_mk2, 1000);
                                            player.GiveWeapon(WeaponHash.Assaultrifle_mk2, 1000);
                                            break;
                                        case 1:
                                            player.GiveWeapon(WeaponHash.Pistol50, 1000);
                                            player.GiveWeapon(WeaponHash.Bullpuprifle_mk2, 1000);
                                            player.GiveWeapon(WeaponHash.Bullpupshotgun, 1000);
                                            player.GiveWeapon(WeaponHash.Microsmg, 1000);
                                            break;
                                        case 2:
                                            player.GiveWeapon(WeaponHash.Appistol, 1000);
                                            player.GiveWeapon(WeaponHash.Carbinerifle, 1000);
                                            player.GiveWeapon(WeaponHash.Sawnoffshotgun, 1000);
                                            player.GiveWeapon(WeaponHash.Minismg, 1000);
                                            break;
                                        case 3:
                                            player.GiveWeapon(WeaponHash.Assaultsmg, 1000);
                                            player.GiveWeapon(WeaponHash.CeramicPistol, 1000);
                                            player.GiveWeapon(WeaponHash.Bottle, 1000);
                                            player.GiveWeapon(WeaponHash.Autoshotgun, 1000);
                                            break;
                                        case 4:
                                            player.GiveWeapon(WeaponHash.Compactrifle, 1000);
                                            player.GiveWeapon(WeaponHash.Combatpdw, 1000);
                                            player.GiveWeapon(WeaponHash.Raypistol, 1000);
                                            player.GiveWeapon(WeaponHash.Assaultshotgun, 1000);
                                            break;
                                    }
                                }
                            }
                            catch { }
                        }, 250 + player.Ping);
                    }
                }
            }
        }
    }
}
