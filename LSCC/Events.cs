using GTANetworkAPI;
using System;
using RNC;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net.Mail;

namespace RNC
{
    class Events : Script
    {
        [ServerEvent(Event.PlayerConnected)]
        public void OnPlayerConnected_Old(Player player)
        {
            EPlayer.ResetPlayerData(player);
        }

        [RemoteEvent("playerReadyCon")]
        public void OnPlayerConnected(Player player)
        {
            Discord.SendTextChannel("staff-bot", $"**{player.Name} has joined the server.** *(Total online: {RAGE.Entities.Players.Count})*");

            NAPI.Util.ConsoleOutput($"[Connection] {player.Name} has joined the server. (IP: {player.Address} - Serial: {player.Serial} - SC: {player.SocialClubName})");

            if (player.HasData("Kicked")) return;

            EPlayer.SetDiscordStatus(player, "Connecting to the server...");

            player.SetData("LastCryptAttempt", Environment.TickCount64);

            EPlayer.ResetPlayerData(player);
            EPlayer.SyncServerSettings(player);
            EPlayer.Freeze(player, true);

            EPlayer.LoginCameraSetup(player);

            EPlayer.LoginBrowserSetup(player);

            player.TriggerEvent("InitChaseTimer");

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

            BanData ban = ContextFactory.Instance.Bans.ToList().Find(ban => ban.HWID.Equals(player.Serial) || ban.IP.Equals(player.Address) ||
                ban.RockstarID == player.SocialClubId);

            if (ban != null)
            {
                EPlayer.SetDiscordStatus(player, "Gazing at the 'BANNED' screen.");

                player.SendChatMessage($"!{{#FF6347}}[BANNED] You are not allowed to play on this server, Ban ID: {ban.SQL_ID}");

                player.SendChatMessage("!{#A9C4E4}Server closed the connection.");

                EPlayer.Notify(player, $"You are banned from the server.<p>Please appeal on our forums (www.racenchase.com) or consult on discord server at discord.gg/y3g2WuR</p><p>Your ban ID is {ban.SQL_ID}.</p>", "red", "fas fa-ban", true, 30000);

                string evadeAccountName = ContextFactory.Instance.Accounts.First(a => a.SQL_ID == ban.AccountSQLID).Username;

                EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}ANTI SUS SYSTEM (ASS) has kicked {player.Name} for ban evading as {evadeAccountName}.");
                NAPI.Util.ConsoleOutput($"[BAN SYSTEM] Kicking {player.Name} as they're banned. (BANID: {ban.SQL_ID} ACCOUNTID: {ban.AccountSQLID})");

                player.SetData("Kicked", true);
                NAPI.Task.Run(() => { try { player.Kick(); } catch { } }, 1000);
                return;
            }


            NAPI.Notification.SendNotificationToPlayer(player, "Welcome to ~y~Race ~w~'n ~b~Chase");
        }

        [RemoteEvent("UpdateAimingState")]
        public void OnUpdateAimingState(Player player, bool state)
        {
            if (!EPlayer.LoggedIn(player)) return;

            if (state)
            {
                player.SetSharedData("IsAiming", state);
                player.SetSharedData("UpdateCameraData", true);
            }
            else
            {
                player.ResetSharedData("IsAiming");
                player.ResetSharedData("UpdateCameraData");
            }
        }

        [RemoteEvent("UpdateCameraDir")]
        public void OnUpdateCameraDirection(Player player, float x, float y, float z)
        {
            if (!EPlayer.LoggedIn(player)) return;

            if (x == -1.0 && y == -1.0 && z == -1.0)
            {
                player.ResetSharedData("CameraDirectionX");
                player.ResetSharedData("CameraDirectionY");
                player.ResetSharedData("CameraDirectionZ");
            }
            else
            {
                player.SetSharedData("CameraDirectionX", x);
                player.SetSharedData("CameraDirectionY", y);
                player.SetSharedData("CameraDirectionZ", z);
            }
        }

        [ServerEvent(Event.PlayerWeaponSwitch)]
        public void OnPlayerWeaponSwitch(Player player, WeaponHash oldWeapon, WeaponHash newWeapon)
        {
            if (!EPlayer.LoggedIn(player)) return;

            player.ResetSharedData("FlashlightEquipped");
            player.ResetSharedData("UpdateCameraData");

            if (newWeapon != WeaponHash.Unarmed)
            {
                try
                {
                    if (newWeapon == WeaponHash.Flashlight)
                    {
                        player.SetSharedData("FlashlightEquipped", true);
                    }

                    if (player.HasData("AttachedComponents"))
                    {
                        List<KeyValuePair<long, long>> comps = player.GetData<List<KeyValuePair<long, long>>>("AttachedComponents");

                        jWeaponData wepdat;
                        if (Settings.WeaponData.TryGetValue(Convert.ToInt64(newWeapon), out wepdat))
                        {
                            var allFlashlights = wepdat.Components.Where(a => a.Value.Name.Contains("Flashlight")).ToList();

                            foreach (var flashlights in allFlashlights)
                            {
                                if(comps.Contains(new KeyValuePair<long, long>(Convert.ToInt64(newWeapon), flashlights.Key)))
                                {
                                    player.SetSharedData("FlashlightEquipped", true);
                                    break;
                                }
                            }
                        }
                    }

                    foreach (Player p in Settings.LoggedInPlayers)
                    {
                        if (p.Dimension == player.Dimension && p.Position.DistanceTo(player.Position) < 500.0)
                        {
                            EPlayer.SyncComponents(p, player, Convert.ToInt64(newWeapon));
                        }
                    }
                }
                catch { }
            }

            return;
        }
        /*
        [RemoteEvent("ExecuteWounded")]
        public void OnExecuteWoundedPlayer(Player player, Player target)
        {
            NAPI.Chat.SendChatMessageToAll("PLAYER: " + player.Name + " TARGET: " + target.Name + " IN ExecuteWounded");

            if(!target.HasSharedData("Wounded") || !player.HasSharedData("Wounded"))
            {
                target.SetSharedData("Wounded", 0);
                player.SetSharedData("Wounded", 0);
                return;
            }

            if (target.GetSharedData<int>("Wounded") == 2 || target.GetSharedData<int>("Wounded") == 6)
            {
                LobbyData lobby = Settings.Lobbies.Find(lobby => lobby.lobbyId == player.GetSharedData<int>("InLobby"));
                if (lobby != null)
                {
                    EPlayer.GrantXP(player, 1); // cuz why not :))
                    foreach (Player ay in Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == lobby.lobbyId))
                    {
                        ay.TriggerEvent("pushToKillFeed", player.Name, EPlayer.HexConverter(EPlayer.GetColor(player)), "Executed", target.Name, EPlayer.HexConverter(EPlayer.GetColor(target)));
                    }
                }
                    
                target.TriggerEvent("setWounded", 3, -1); // 4 = special state for executing
            }
        
        }*/

        [RemoteEvent("WoundedTimerTick")]
        public void OnWoundedTimerTick(Player player, int time)
        {
            if(time == 0)
            {
                player.Health = 0;
                player.SendChatMessage("!{#FF6347}You succumbed to your injuries and have bled out.");
            }
        }

        [RemoteEvent("SetPlayerWounded")]
        public void OnPlayerWounded(Player player, int state, int killerID)
        {
            if (player.GetSharedData<int>("Wounded") == state && state != 0) return; // REPEL: Laggers... So it doesn't call twice...

            player.SetSharedData("Wounded", state);
            Player killer = null;
            if(killerID != -1)
            {
                killer = RAGE.Entities.Players.All.Find(p => p.Id == killerID);
                if(killer == null)
                {
                    player.SendChatMessage("!{#FF6347}A fatal error has occurred - please contact the devs and tell them this code: " + player.Name + "_D" + player.Id);
                    return;
                }
            }

            switch (state)
            {
                case 0:
                    {
                        //player.SendChatMessage("DEBUG: Wounded state set to 0");
                        Events.RequestClearAnimation(player);
                        player.StopAnimation();
                        player.TriggerEvent("ResetWounded");
                        break;
                    }
                case 1:
                    {
                        player.SendChatMessage("!{#C3A3DB}* You suddenly start losing strength and collapse - you lost too much blood.");
                        player.Health = 100;

                        if (player.IsInVehicle && player.Vehicle != null)
                        {
                            if (player.Vehicle.Class == 15 || player.Vehicle.Class == 16)
                            {
                                player.Vehicle.EngineStatus = false;
                            }

                            player.Health = 25;
                            player.SendChatMessage("!{#FF6347}You have been severely injured, but you are not dead yet!");
                            player.SendChatMessage("!{#FF6347}You are currently in a vehicle & do not have enough strength to exit it.");
                            player.SendChatMessage("!{#FF6347}A teammate will have to enter the vehicle in order to treat your wounds. [unlock your vehicle]");
                            player.SendChatMessage("!{#FF6347}You will die in 60 seconds if you are not revived by a teammate. Use (/lobby) to respawn instantly.");
                        }

                        if (player.GetSharedData<string>("ActionType") != "none")
                        {
                            EPlayer.StopPlayerAction(player, 1);
                        }

                        foreach (Player pl in RAGE.Entities.Players.All.Where(p => p.Position.DistanceTo(player.Position) < 10.0f))
                        {
                            if (pl.GetSharedData<string>("CuffedBy") == player.Name)
                            {
                                player.TriggerEvent("StopCuffing", 12);
                                break;
                            }
                        }

                        if (player.HasSharedData("ReviveTarget") && player.GetSharedData<Player>("ReviveTarget") != null)
                        {
                            player.TriggerEvent("StopReviving", 3);
                        }

                        #region -> Giving XP to killer, handling stats, kill feed
                        if (killer != null && killer != player)
                        {
                            LobbyData lobby = Settings.Lobbies.Find(lobby => lobby.lobbyId == player.GetSharedData<int>("InLobby"));
                            if (lobby != null)
                            {
                                string reasonStr = null;
                                jWeaponData wepDat = new jWeaponData();

                                if (Main.WeaponReasons.TryGetValue((long)killer.CurrentWeapon, out reasonStr))
                                {
                                    // oh its already set ^^
                                }
                                else if (Settings.WeaponData.TryGetValue((long)killer.CurrentWeapon, out wepDat))
                                {
                                    reasonStr = wepDat.Name;
                                }
                                else
                                {
                                    reasonStr = null;
                                }

                                if (!player.HasSharedData("LastKiller") || player.GetSharedData<Player>("LastKiller") != killer)
                                {
                                    EPlayer.GetAccount(killer).Kills++;
                                    player.SetSharedData("LastKiller", killer);
                                }
                                killer.SetSharedData("FugitiveChance", killer.GetSharedData<float>("FugitiveChance") + 0.3 + (double)((double)EPlayer.GetAccount(killer).DonatorLevel / 10.0));
                                EPlayer.GrantXP(killer, 2);
                                if (lobby.Fugitives.Contains(player) && killer.GetSharedData<int>("Team") == 0)
                                {
                                    PlayerRequestShotTag(killer);
                                }

                                if (reasonStr != null)
                                {
                                    foreach (Player ay in Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == lobby.lobbyId))
                                    {
                                        ay.TriggerEvent("pushToKillFeed", killer.Name, EPlayer.HexConverter(EPlayer.GetColor(killer)), $"Downed ({reasonStr})", player.Name, EPlayer.HexConverter(EPlayer.GetColor(player)));
                                    }
                                }
                                else
                                {
                                    foreach (Player ay in Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == lobby.lobbyId))
                                    {
                                        ay.TriggerEvent("pushToKillFeed", killer.Name, EPlayer.HexConverter(EPlayer.GetColor(killer)), "Downed", player.Name, EPlayer.HexConverter(EPlayer.GetColor(player)));
                                    }
                                }
                            }
                        }
                        #endregion
                        break;
                    }
                case 2:
                    {
                        player.Health = 25;
                        player.SendChatMessage("!{#FF6347}You have been severely injured, but you are not dead yet!");
                        player.SendChatMessage("!{#FF6347}You have enough strength to crawl - use it to crawl to safety.");
                        player.SendChatMessage("!{#FF6347}You will die in 60 seconds if you are not revived by a teammate. Use (/lobby) to respawn instantly.");
                        break;
                    }
            }
        }

        [RemoteEvent("SilentSirenSync")]
        public void OnSilentSirenSync(Player player, Vehicle vehicle)
        {
            if(vehicle != null && vehicle.Exists)
            {
                if (!vehicle.HasSharedData("SilentSiren")) vehicle.SetSharedData("SilentSiren", false);
                vehicle.SetSharedData("SilentSiren", !vehicle.GetSharedData<bool>("SilentSiren"));
            }
        }

        [RemoteEvent("OnPlayerActionTick")]
        public static void OnPlayerActionTick(Player player)
        {
            if(player.GetSharedData<int>("Team") == -1)
            {
                EPlayer.StopPlayerAction(player, 0);
                return;
            }
            player.SetSharedData("ActionTime", player.GetSharedData<int>("ActionTime") - 1);

            string action = player.GetSharedData<string>("ActionType");
            switch (action)
            {
                case "painkiller":
                    player.Health++;
                    if (player.Health >= 99)
                    {
                        EPlayer.StopPlayerAction(player, 2);
                    }
                    break;
                default: return;
            }
        }

        [RemoteEvent("StopPlayerAction")]
        public static void OnStopPlayerAction(Player player, int reason)
        {
            switch (reason)
            {
                case 0: // Not in CC
                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Your action was stopped: you are no longer in copchase.");
                    break;
                case 1: // Death
                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Your action has failed: you are severely wounded or dead.");
                    break;
                case 2: // Max HP reached (painkillers)
                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Your painkillers effect stopped earlier: your HP is full.");
                    break;
                default: break;
            }
            player.SetSharedData("ActionType", "none");
            player.SetSharedData("ActionTime", 0);
        }

        [RemoteEvent("FinishPlayerAction")]
        public static void OnFinishPlayerAction(Player player, string action)
        {
            switch (action.ToLower())
            {
                case "medkit":
                    EPlayer.GetAccount(player).Medkits--;

                    int pHP = player.Health;
                    if (pHP + 60 > 100) { player.Health = 100; }
                    else player.Health = pHP + 60;

                    player.SendChatMessage($"~g~You used a medkit and gained +60HP. Your HP is now {(pHP + 60 > 100 ? 100 : pHP+60)}.");
                    break;
                case "painkiller":
                    EPlayer.GetAccount(player).Painkillers--;
                    player.SendChatMessage($"~g~Painkillers effect expired. You gained +40HP from them and your HP is now {player.Health}.");
                    break;
                default: return;
            }

            player.SetSharedData("ActionType", "none");
            player.SetSharedData("ActionTime", 0);
        }

        [RemoteEvent("SyncShotStats")]
        public static void OnSynchronizeShotStatistics(Player player, int TotalShotsFired, int PlayerHits, int VehicleHits, int Missed)
        {
            player.SetData("ShotStats.TotalFired", TotalShotsFired);
            player.SetData("ShotStats.HitPlayer", PlayerHits);
            player.SetData("ShotStats.HitVehicle", VehicleHits);
            player.SetData("ShotStats.Missed", Missed);
            player.SetData("ShotStats.LastSync", Environment.TickCount64);
        }

        [RemoteEvent("UpdateFPS")]
        public static void OnSynchronizeFPS(Player player, int FPS)
        {
            player.SetSharedData("FPS", FPS);
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
        public static void RequestPlayAnimation(Player player, string animDict, string animName, int animFlags, float speed = 8.0f, float speedMul = 0.0f, int duration = -1, float playbackRate = 0.0f, bool lockX = false, bool lockY = false, bool lockZ = false)
        {
            RAGE.Entities.Players.All.Where(a => a.Dimension == player.Dimension && a.Position.DistanceTo(player.Position) < 500.0).ToList().ForEach((Player act) =>
            {
                act.TriggerEvent("entplayanim", player, animDict, animName, animFlags, speed, speedMul, duration, playbackRate, lockX, lockY, lockZ);
            });
        }

        [RemoteEvent("clear_tasks")]
        public static void ClearTasksSync(Player player, int immediately)
        {
            RAGE.Entities.Players.All.Where(a => a.Dimension == player.Dimension && a.Position.DistanceTo(player.Position) < 200.0).ToList().ForEach((Player act) =>
            {
                act.TriggerEvent("entcleartasks", player, immediately);
            });
        }

        [RemoteEvent("Sync:DoorShut")]
        public static void OnShutDoorSync(Player player, Vehicle vehicle, int index, bool instantly)
        {
            if (vehicle == null) return;

            RAGE.Entities.Players.All.Where(a => a.Dimension == player.Dimension && a.Position.DistanceTo(player.Position) < 200.0).ToList().ForEach((Player act) =>
            {
                act.TriggerEvent("Sync:SetDoorShut", vehicle, index, instantly);
            });

            if(index == 4)  // Bonnet
            {
                vehicle.ResetSharedData("BonnetOpened");
                EPlayer.Action(player, "slams the hood shut.");
            }
            else if (index == 5)  // Boot/Trunk
            {
                vehicle.ResetSharedData("TrunkOpened");
                EPlayer.Action(player, "closes the trunk of the vehicle.");
            }
        }
        [RemoteEvent("Sync:DoorOpen")]
        public static void OnOpenDoorSync(Player player, Vehicle vehicle, int index, bool loose, bool instantly)
        {
            if (vehicle == null) return;

            RAGE.Entities.Players.All.Where(a => a.Dimension == player.Dimension && a.Position.DistanceTo(player.Position) < 200.0).ToList().ForEach((Player act) =>
            {
                act.TriggerEvent("Sync:SetDoorOpen", vehicle, index, loose, instantly);
            });

            if (index == 4)  // Bonnet
            {
                vehicle.SetSharedData("BonnetOpened", true);

                if (player.CurrentWeapon == WeaponHash.Crowbar) EPlayer.Action(player, "forces the bonnet open with a crowbar.");
                else EPlayer.Action(player, "lifts up the hood of the vehicle.");
            }
            else if (index == 5)  // Boot
            {
                vehicle.SetSharedData("TrunkOpened", true);

                if (player.CurrentWeapon == WeaponHash.Crowbar) EPlayer.Action(player, "forces the trunk open with a crowbar.");
                else EPlayer.Action(player, "pops the trunk open.");
            }
        }

        [RemoteEvent("OnScriptedKeyPress")]
        public static void OnScriptedKeyPress(Player player, int keyId, bool keyState) // keyState == true? Down
        {
            if (keyId == 0x5A)
            {
                LobbyData lobby = Settings.Lobbies.Find(lobby => lobby.lobbyId == player.GetSharedData<int>("InLobby"));
                if (lobby != null)
                {
                    if (lobby.Cops.Contains(player)) RNC.Commands.cmd_gps(player);
                }
            }

            if(keyId == 0x74)
            {
                Commands.cmd_dl(player);
            }

            if(keyId == 0x4C)
            {
                player.SetData("SilentLock", Environment.TickCount64);
                Commands.cmd_lock(player);
            }

            if (keyId == 0x59)
            {
                Commands.cmd_engine(player);
            }

            if(keyId == 0x4A && player.IsInVehicle)
            {
                Commands.cmd_seatbelt(player);
            }

            if (keyId == 0x09)
            {
                int voiceMode = player.GetSharedData<int>("VoiceMode");
                voiceMode++;

                if (voiceMode >= 4) voiceMode = 0;

                switch (voiceMode)
                {
                    case 0:
                        player.SendChatMessage("!{#FFFF00}Voice mode set to: !{#FFFFFF}Nearby (30m) (All players)");
                        break;
                    case 1:
                        player.SendChatMessage("!{#FFFF00}Voice mode set to: !{#FFFFFF}Team Only (+15m proximity)");
                        break;
                    case 2:
                        player.SendChatMessage("!{#FFFF00}Voice mode set to: !{#FFFFFF}Squad Only (Everyone else gets muted)");
                        break;
                    case 3:
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
                        if (player.HasData("PlayerSquad"))
                        {
                            listeners.AddRange(Settings.LoggedInPlayers.Where(p => p.HasData("PlayerSquad") && p.GetData<Player>("PlayerSquad") == player.GetData<Player>("PlayerSquad")));
                        }
                        else player.SendChatMessage("!{#FF6347}NOTE: You are trying to speak in squad chat while not being in a squad!");
                    }
                    else if (player.GetSharedData<int>("VoiceMode") == 3)
                    {
                        // Disabled voice. Nah, no listeners for you.
                    }
                    
                    listeners.RemoveAll(p => p.Id == player.Id); // Remove ALL instances of local player.

                    foreach (Player t in listeners)
                    {
                        if (t.HasData($"VoiceMute{player.Name}"))
                        {
                            continue;
                        }

                        if (t.GetSharedData<int>("VoiceMode") == 3) continue;
                        if (t.GetSharedData<int>("VoiceMode") == 2)
                        {
                            if (!t.HasData("PlayerSquad")) continue;
                            if (!player.HasData("PlayerSquad")) continue;

                            if (t.GetData<Player>("PlayerSquad") != player.GetData<Player>("PlayerSquad")) continue;
                        }

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
            player.SetSharedData("Ping", player.Ping);
            vehicle.SetSharedData("Engine", vehicle.EngineStatus);

            if(player.HasSharedData("Team") && player.GetSharedData<int>("Team") >= 0)
            {
                int count = RAGE.Entities.Players.All.Where(a => a.IsInVehicle && a.Vehicle == vehicle && a.HasSharedData("Team") && a.VehicleSeat == 0 && 
                    a.GetSharedData<int>("Team") != -1 && a.GetSharedData<int>("Team") != player.GetSharedData<int>("Team")).Count();

                if (count >= 1)
                {
                    // player.WarpOutOfVehicle(); WARP MAGICALLY CRASHES UR GAME LIKE OK BRO
                    player.Position = player.Position.Around(5f);

                    player.SendNotification("~r~You cannot enter a vehicle that has an opposition team driver!");
                }
            }

            if (Settings.Dynamics.OnfootCollision == false)
            {
                if (player.GetSharedData<int>("Team") == 0 || player.GetSharedData<int>("Team") == 1)
                {
                    player.SetSharedData("Collisions", true);
                }
            }

            player.SetSharedData("LastVehEnterExit", DateTimeOffset.Now.ToUnixTimeSeconds());
        }

        [ServerEvent(Event.PlayerExitVehicle)]
        public void OnPlayerExitVehicle(Player player, Vehicle vehicle)
        {
            player.SetSharedData("Ping", player.Ping);
            try // Because triggers on disconnection and stuff invalid vehicles etc
            {
                if (vehicle == null) return;

                if (Settings.Dynamics.OnfootCollision == false)
                {
                    if (player.GetSharedData<int>("Team") == 0 || player.GetSharedData<int>("Team") == 1)
                    {
                        player.SetSharedData("Collisions", false);
                        player.TriggerEvent("ToggleCollisionsForEnt", player, false);
                    }
                }

                if (vehicle.HasSharedData("Engine"))
                {
                    vehicle.EngineStatus = vehicle.GetSharedData<bool>("Engine");
                    vehicle.SetSharedData("Engine", vehicle.EngineStatus);
                }
                else
                {
                    vehicle.SetSharedData("Engine", vehicle.EngineStatus);
                }
            }
            catch { }

            player.SetSharedData("LastVehEnterExit", DateTimeOffset.Now.ToUnixTimeSeconds());
            if (vehicle.HasSharedData("SirenState")) {
                vehicle.SetSharedData("SirenState", !vehicle.GetSharedData<bool>("SirenState"));
                vehicle.SetSharedData("SirenState", !vehicle.GetSharedData<bool>("SirenState")); // Revert it to false then true or vice-versa so clients pick a "change" and apply it
            }
        }

        [ServerEvent(Event.PlayerDisconnected)]
        public void OnPlayerDisconnected(Player player, DisconnectionType type, string reason)
        {
            if (player.HasData("Tuner_Vehicle")) player.GetData<Vehicle>("Tuner_Vehicle").Delete();

            foreach(Player p in Settings.LoggedInPlayers.Where(a => a.HasData("Spectating") && a.GetData<Player>("SpectatingPlayer") == player))
            {
                Commands.cmd_specoff(p);
            }
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

            foreach(var lobby in Settings.Lobbies)
            {
                lock (lobby)
                {
                    lock (lobby.Cops) { lobby.Cops.Remove(player); }
                    lock (lobby.Fugitives) { lobby.Fugitives.Remove(player); }
                }
            }

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

                    Logs.LogSession(player);

                    EPlayer.Save(player);
                }

                Settings.LoggedInPlayers.Remove(player);
                EPlayer.ResetPlayerData(player);

                Discord.SendTextChannel("staff-bot", $"**{player.Name} has left the server.** *(Total online: {RAGE.Entities.Players.Count - 1})*");
            }
        }

        [ServerEvent(Event.PlayerDeath)]
        public static void OnPlayerDeath(Player player, Player killer, uint reason)
        {
            if (!EPlayer.LoggedIn(player)) return;

            player.SetSharedData("Collisions", true); // Reset collision from chase.
            player.SetSharedData("CuffedBy", "n/a");
            player.ResetData("FreezeWeapons");
            player.SetData("Dead", Environment.TickCount64);
            player.ResetData("Seatbelt");
            player.TriggerEvent("ToggleSeatbelt", false);
            player.ResetData("DraggingOut");
            player.TriggerEvent("togglePlayerCrawl", false);

            string reasonStr;
            bool wasInGame = false;
            jWeaponData wepDat = new jWeaponData();

            if (Main.WeaponReasons.TryGetValue(reason, out reasonStr))
            {
                // oh its already set ^^
            }
            else if(Settings.WeaponData.TryGetValue((long)reason, out wepDat))
            {
                reasonStr = wepDat.Name;
            }
            else
            {
                reasonStr = null;
            }

            if(player.GetSharedData<string>("ActionType") != "none")
            {
                EPlayer.StopPlayerAction(player, 1);
            }

            if (reason == 696988) reasonStr = "handcuffs";
            if (reason == 696989) reasonStr = "arrest/busted";

            if (reasonStr != null)
            {
                reasonStr.Replace(".", "");
            }

            if (player.GetSharedData<int>("InLobby") == -4 && Settings.DerbyTimeLeft > 0 && Settings.DerbyTimeLeft != 180 && 
                player.HasSharedData("DerbyTime"))
            {
                if(player.GetSharedData<int>("DerbyTime") >= 5)
                {
                    foreach (Player inderby in Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == -4))
                    {
                        inderby.TriggerEvent("pushToKillFeed2", player.Name, EPlayer.HexConverter(EPlayer.GetColor(player)), "Fell down");
                    }
                }
                player.SetSharedData("DerbyTime", 0);
            }
            try
            {
                if (killer != null && !killer.IsNull && killer != player)
                {
                    NAPI.Util.ConsoleOutput($"[Death] {player.Name} has been killed by {killer.Name} using {(reasonStr == null ? "N/A" : reasonStr)}.");

                    if ((killer.GetSharedData<int>("InLobby") == -6 || killer.GetSharedData<int>("InLobby") > -1) && killer.GetSharedData<int>("InDM") > -1)
                    {
                        int arenaID = killer.GetSharedData<int>("InDM");

                        lock (Settings.LoggedInPlayers)
                        {
                            foreach (Player indm in Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == killer.GetSharedData<int>("InLobby") && a.GetSharedData<int>("InDM") == arenaID))
                            {
                                if (reasonStr == null)
                                {
                                    indm.TriggerEvent("pushToKillFeed2", player.Name, EPlayer.HexConverter(EPlayer.GetColor(player)), "Fell");
                                }
                                else
                                {
                                    indm.TriggerEvent("pushToKillFeed", killer.Name, EPlayer.HexConverter(EPlayer.GetColor(killer)), reasonStr, player.Name, EPlayer.HexConverter(EPlayer.GetColor(player)));
                                }
                            }
                            EPlayer.GetAccount(killer).Kills++;
                            EPlayer.GetAccount(player).Deaths++;
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
                               

                                int warnings = 0;
                                if (killer.HasData("InnocentShotWarning")) warnings = killer.GetData<int>("InnocentShotWarning");

                                warnings += 3;
                                if (warnings >= 7)
                                {
                                    int xpReduce = EPlayer.GetAccount(killer).XP >= 8 ? -(EPlayer.GetAccount(killer).XP / 4) : -3;

                                    EPlayer.GrantXP(killer, xpReduce);
                                    RAGE.Entities.Players.All.ForEach((Player ey) =>
                                    {
                                        ey.SendChatMessage($"!{{FF6347}}[RULEBREAK] {killer.Name}'s XP has been reduced by {xpReduce} for killing a fugitive that hasn't shot.");
                                    });

                                    killer.SetSharedData("FugitiveChance", killer.GetSharedData<float>("FugitiveChance") - 1.0);

                                    //killer.Health = 0;
                                    killer.SendChatMessage($"!{{FF6347}}[RULEBREAK] Your fugitive chance has been decreased for killing an innocent fugitive.");
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
                            int warnings = 0;
                            if (killer.HasData("TeamShotWarning")) warnings = killer.GetData<int>("TeamShotWarning");

                            warnings += 3;

                            if (killer.IsInVehicle == true && killer.Vehicle.Model == NAPI.Util.GetHashKey("polmav"))
                            {
                                killer.Vehicle.Position = new Vector3(killer.Vehicle.Position.X, killer.Vehicle.Position.Y, killer.Vehicle.Position.Z + 20.0f);
                                killer.SendChatMessage($"!{{FF6347}}[RULEBREAK] !{{#FFFFFF}}Please do not heliblade.");
                            }
                            else if (warnings >= 7)
                            {
                                //killer.Health = 0;
                                killer.SendChatMessage($"!{{FF6347}}[RULEBREAK] Your fugitive chance has been decreased for team killing.");
                                killer.SetSharedData("FugitiveChance", killer.GetSharedData<float>("FugitiveChance") - 1.0);

                                int xpReduce = EPlayer.GetAccount(killer).XP >= 8 ? -(EPlayer.GetAccount(killer).XP / 4) : -3;

                                EPlayer.GrantXP(killer, xpReduce);
                                RAGE.Entities.Players.All.ForEach((Player ey) =>
                                {
                                    ey.SendChatMessage($"!{{FF6347}}[RULEBREAK] {killer.Name}'s XP has been reduced by {xpReduce} for team killing.");
                                });

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
                        if (lobby.Cops.Contains(player) || lobby.Fugitives.Contains(player)) EPlayer.CreateAmmoBox(player);

                        if (killer != null)
                        {
                            int killStreak = killer.GetData<int>("Killstreak");
                            killer.SetData("Killstreak", killStreak + 1);

                            if (killStreak % 4 == 0 && killStreak >= 4 && (lobby.Cops.Contains(killer) || lobby.Fugitives.Contains(killer)))
                            {
                                EPlayer.GrantXP(killer, 5);
                                EPlayer.SendMessageInLobby(lobby.lobbyId, $"!{{#FF0000}}[KILLSTREAK] !{{#FF6347}}{killer.Name} !{{#FFFFFF}}is on a {killStreak} kills rampage! Someone stop them!");

                                killer.SendChatMessage("!{#00FF33}[!] !{#FFFFFF}You have earnt 5 XP and 5 points for maintaining a killstreak!");
                                EPlayer.GivePoints(killer, 5);
                                killer.SetSharedData("FugitiveChance", killer.GetSharedData<float>("FugitiveChance") + (0.1 * (double)((double)EPlayer.GetAccount(killer).DonatorLevel + 1.0)));
                            }
                        }
                        if (lobby.Fugitives.Contains(player))
                        {
                            if (killer != null)
                            {
                                //EPlayer.GetAccount(player).Deaths++;
                                //EPlayer.GetAccount(killer).Kills++;
                                //killer.SetSharedData("FugitiveChance", killer.GetSharedData<float>("FugitiveChance") + 0.3 + (double)((double)EPlayer.GetAccount(killer).DonatorLevel / 10.0));
                            }

                            wasInGame = true;

                            //if (killer != null && killer.GetSharedData<int>("Team") == 1) EPlayer.GrantXP(killer, 2);

                            lock (Settings.LoggedInPlayers)
                            {
                                foreach (Player ay in Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == lobby.lobbyId))
                                {
                                    if (killer != null && !killer.IsNull && killer != player)
                                    {
                                        if (reasonStr == null)
                                        {
                                            ay.TriggerEvent("pushToKillFeed", killer.Name, EPlayer.HexConverter(EPlayer.GetColor(killer)), "killed", player.Name, EPlayer.HexConverter(EPlayer.GetColor(player)));
                                        }
                                        else
                                        {
                                            if (reason == 696988)
                                            {
                                                ay.TriggerEvent("pushToKillFeed", killer.Name, EPlayer.HexConverter(EPlayer.GetColor(killer)), "arrested", player.Name, EPlayer.HexConverter(EPlayer.GetColor(player)));
                                            }
                                            else if (reason == 696989)
                                            {
                                                ay.TriggerEvent("pushToKillFeed", killer.Name, EPlayer.HexConverter(EPlayer.GetColor(killer)), "busted", player.Name, EPlayer.HexConverter(EPlayer.GetColor(player)));
                                            }
                                            else ay.TriggerEvent("pushToKillFeed", killer.Name, EPlayer.HexConverter(EPlayer.GetColor(killer)), reasonStr, player.Name, EPlayer.HexConverter(EPlayer.GetColor(player)));
                                        }
                                    }
                                    else
                                    {
                                        if (reasonStr == null)
                                        {
                                            ay.TriggerEvent("pushToKillFeed2", player.Name, EPlayer.HexConverter(EPlayer.GetColor(player)), "Fell");
                                        }
                                        else
                                        {
                                            ay.TriggerEvent("pushToKillFeed2", player.Name, EPlayer.HexConverter(EPlayer.GetColor(player)), reasonStr);
                                        }
                                    }
                                }
                            }
                            lobby.Fugitives.Remove(player);
                        }

                        if (lobby.Cops.Contains(player))
                        {
                            if (killer != null)
                            {
                                //EPlayer.GetAccount(player).Deaths++;
                                //EPlayer.GetAccount(killer).Kills++;
                                //killer.SetSharedData("FugitiveChance", killer.GetSharedData<float>("FugitiveChance") + 0.3 + (double)((double)EPlayer.GetAccount(killer).DonatorLevel / 10.0));
                            }

                            wasInGame = true;

                            if (killer != null && killer.GetSharedData<int>("Team") == 0)
                            {
                                //EPlayer.GrantXP(killer, 2);
                                //Events.PlayerRequestShotTag(killer);
                            }

                            lock (Settings.LoggedInPlayers)
                            {
                                foreach (Player ay in Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == lobby.lobbyId))
                                {
                                    if (killer != null && !killer.IsNull && player != killer)
                                    {
                                        if (reasonStr == null)
                                        {
                                            ay.TriggerEvent("pushToKillFeed", killer.Name, EPlayer.HexConverter(EPlayer.GetColor(killer)), "killed", player.Name, EPlayer.HexConverter(EPlayer.GetColor(player)));
                                        }
                                        else
                                        {
                                            ay.TriggerEvent("pushToKillFeed", killer.Name, EPlayer.HexConverter(EPlayer.GetColor(killer)), reasonStr, player.Name, EPlayer.HexConverter(EPlayer.GetColor(player)));
                                        }
                                    }
                                    else
                                    {
                                        if (reasonStr == null)
                                        {
                                            ay.TriggerEvent("pushToKillFeed2", player.Name, EPlayer.HexConverter(EPlayer.GetColor(player)), "Fell");
                                        }
                                        else
                                        {
                                            ay.TriggerEvent("pushToKillFee2d", player.Name, EPlayer.HexConverter(EPlayer.GetColor(player)), reasonStr);
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
                            Random rand = new Random(DateTimeOffset.UtcNow.Millisecond);
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
                            if (player.GetSharedData<int>("InLobby") == -6)
                            {
                                SpawnArea = player.Position; // So that player spawns inside the arena, and is transported whenever he spawns to a new point.
                            }

                            NAPI.Player.SpawnPlayer(player, SpawnArea);
                        }
                    }
                    catch { }
                }, (wasInGame == true && reason != 696989) ? 10000 : 1500);
            }
            catch { }

            player.SetData("Killstreak", 0);
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

        [RemoteEvent("JSResolved")]
        public static void OnDevExecJS(Player player, string code, string result)
        {
            string lastJSTriggerer = "n/a";
            if (player.HasData("TriggerJSReturn")) lastJSTriggerer = player.GetData<string>("TriggerJSReturn");
            try { Main.Log("jsadmin.txt", $"{DateTime.Now.ToString().Replace(":", "|")} {player.Name} executed (by: {lastJSTriggerer}) and resolved JS code: " + Environment.NewLine + code + Environment.NewLine + " with result: " + Environment.NewLine + result + Environment.NewLine); } catch { }
            if (!lastJSTriggerer.Equals("n/a"))
            {
                if(Settings.LoggedInPlayers.Where(a => a.Name.Equals(lastJSTriggerer)).Count() >= 1) 
                    Settings.LoggedInPlayers.Where(a => a.Name.Equals(lastJSTriggerer)).First().SendChatMessage($"Result ({player.Name}): {result}");
            }
        }

        [RemoteEvent("ClimbProtection")]
        public static void OnClimbTrigger(Player player, int status)
        {
            if (!EPlayer.LoggedIn(player)) return;

            LobbyData lobby = Settings.Lobbies.Find(lobby => lobby.lobbyId == player.GetSharedData<int>("InLobby"));
            if (lobby == null)
            {
                return;
            }
            if (!lobby.Fugitives.Contains(player) && !lobby.Cops.Contains(player)) return;

            if(Environment.TickCount64 - player.GetData<long>("LastClimbProtection") >= 45000)
            {
                player.SetData("ClimbProtectionProcs", 0);
            }

            int time = 0;
            switch(status)
            {
                case 0:
                    if (player.GetData<int>("ClimbProtectionProcs") < 2)
                    {
                        player.SetSharedData("ClimbProtection", true);

                        if (player.GetSharedData<int>("Team") == 0) time = 7500;
                        else time = 15000;

                        player.SetData("ClimbProtectionEnd", Environment.TickCount64 + time);
                        player.SetData("LastClimbProtection", Environment.TickCount64);
                        player.SetData("ClimbProtectionProcs", player.GetData<int>("ClimbProtectionProcs")+1);
                        EPlayer.Notify(player, "Climb protection...", "green", "fas fa-hourglass", false, time, "topCenter", "replace");
                    }
                    break;

                case 1:
                    if (player.GetData<int>("ClimbProtectionProcs") < 2)
                    {
                        player.SetSharedData("ClimbProtection", true);

                        if (player.GetSharedData<int>("Team") == 0) time = 1000;
                        else time = 2000;

                        player.SetData("ClimbProtectionEnd", Environment.TickCount64 + time);
                        player.SetData("LastClimbProtection", Environment.TickCount64);
                        EPlayer.Notify(player, "Climb protection...", "green", "fas fa-hourglass", false, time, "topCenter", "replace");
                    }
                    break;
            }
            return;
        }

        [RemoteEvent("SpectateChangeHPAR")]
        public static void SpectateHPARChange(Player player, int health, int armour)
        {
            if (player.HasData("Spectating"))
            {
                if (health < 1) health = 1;
                player.Health = health;
                player.Armor = armour;
            }
        }

        [RemoteEvent("RequestSwimPenalty")]
        public static void OnRequestSwimPenalty(Player player)
        {
            player.Health -= 5; // Slowly reduce health by 5 if in wotah.
            player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Evading by swimming is not allowed, you are taking damage.");
        }

        public static void OnPlayerLogin(Player player, Account acc)
        {
            player.SetSharedData("EntAlpha", 255);
            player.SetData("PlaytimeTick", Environment.TickCount64);
            Discord.SendTextChannel("staff-bot", $"*{player.Name} has logged in as {acc.Username}.*");

            NAPI.Util.ConsoleOutput($"[Login] {player.Name} has logged in as {acc.Username}. (IP: {player.Address} - Serial: {player.Serial} - SC: {player.SocialClubName})");

            acc.LastHWID = player.Serial;
            acc.LastIP = player.Address;
            acc.LastRID = player.SocialClubId;
            acc.LastSC = player.SocialClubName;

            if (acc.RegisterHWID.Equals("n/a")) acc.RegisterHWID = player.Serial;
            if (acc.RegisterIP.Equals("n/a")) acc.RegisterIP = player.Address;
            if (acc.RegisterSC.Equals("n/a")) acc.RegisterSC = player.SocialClubName;
            if (acc.RegisterRID == 0) acc.RegisterRID = player.SocialClubId;

            if (acc.RegisterDate.Year == 2000) acc.RegisterDate = DateTime.Now; // Year 2000 = it's not set.

            if (acc.DiscordInfo == null || acc.DiscordInfo.Length < 4)
            {
                DiscordData _discordData = new DiscordData();
                acc.DiscordInfo = NAPI.Util.ToJson(_discordData);
                NAPI.Util.ConsoleOutput($"[DISCORD INTEGRATION] {player.Name}'s discord data was invalid, generating them new. This is a one time thing, unless discord data is deleted.");
            }

            var linkedSQLs = EPlayer.GetLinkedAccounts(acc).Select(accs => accs.SQL_ID).ToList(); // BONKERS

            if(linkedSQLs.Intersect(ContextFactory.Instance.Bans.ToList().Select(bans => bans.AccountSQLID)).Any())
            {
                EPlayer.SetDiscordStatus(player, "Gazing at the 'BANNED' screen.");

                player.SendChatMessage($"!{{#FF6347}}[BANNED] You are not allowed to play on this server.");

                EPlayer.Notify(player, $"You are banned from the server.<p>Please appeal on our forums (www.racenchase.com) or consult on discord server at discord.gg/y3g2WuR</p><p>Use your account name as a reference.</p>", "red", "fas fa-ban", true, 30000);

                player.SendChatMessage("!{#A9C4E4}Server closed the connection.");

                EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}ANTI SUS SYSTEM (ASS) has kicked {acc.Username} for ban evading/outstanding bans.");
                NAPI.Util.ConsoleOutput($"[BAN SYSTEM] Kicking on login: {player.Name} (account name) as they're banned.");

                player.SetData("Kicked", true);
                NAPI.Task.Run(() => { try { player.Kick(); } catch { } }, 1000);

                return;
            }

            player.Name = acc.Username;
            player.SetData("LoggedIn", true);
            player.SetData("AccountHandle", acc);
            player.SetSharedData("pLogged", true);

            player.ResetSharedData("LastKiller"); 
            player.ResetSharedData("BeingRevived");
            player.ResetSharedData("ReviveTarget");

            if (ContextFactory.Instance.Languages.ToList().Find(l => l.SQL_ID == acc.Language) != null && EPlayer.GetAccount(player).Language != -1)
                player.SetData("Language", ContextFactory.Instance.Languages.ToList().Find(l => l.SQL_ID == acc.Language));
            else
                player.ResetData("Language");

            player.SetData<long>("JoinTimeUnix", DateTimeOffset.Now.ToUnixTimeSeconds());

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
                Dictionary<long, wComponentData> wepItems = new Dictionary<long, wComponentData>();

                wComponentData test = new wComponentData();
                test.Components.Add(123456789);
                wepItems.Add(123456789, test);

                acc.WeaponComponents = NAPI.Util.ToJson(wepItems);
            }

            EPlayer.ApplyCharacter(player, ConvChar, acc.LobbyClothing);

            EPlayer.Spawn(player);

            if (EPlayer.GetAccount(player).MOTD != null && !EPlayer.GetAccount(player).MOTD.Equals("none"))
            {
                MOTDData motdToShow = NAPI.Util.FromJson<MOTDData>(EPlayer.GetAccount(player).MOTD);
                Main.DisplayMOTD(player, motdToShow);
            }

            player.SendChatMessage("!{#a6a6a6}Your last login was on !{#c2c1b6}" + EPlayer.GetAccount(player).LastLogin.ToString());
            EPlayer.GetAccount(player).LastLogin = DateTime.Now;

            if (EPlayer.GetAccount(player).LastSessionMins >= 60.0) 
                player.SendChatMessage($"!{{#a6a6a6}}Last session lasted !{{#c2c1b6}}{(EPlayer.GetAccount(player).LastSessionMins / 60.0).ToString("N1")} hours.");
            else
                player.SendChatMessage($"!{{#a6a6a6}}Last session lasted !{{#c2c1b6}}{(EPlayer.GetAccount(player).LastSessionMins).ToString("N1")} minutes.");

            if(ContextFactory.Instance.PlayerOutfits.ToList().Find(o => o.AccountID == acc.SQL_ID) == null)
            {
                CharacterData[] defaultOutfits = new CharacterData[5];
                for(int i = 0; i < 5; i++)
                {
                    defaultOutfits[i] = new CharacterData();
                }
                ContextFactory.Instance.PlayerOutfits.Add(new PlayerOutfit()
                {
                    AccountID = acc.SQL_ID,
                    OutfitData = NAPI.Util.ToJson(defaultOutfits)
                });
                ContextFactory.Instance.SaveChanges();
            }

            player.SetSharedData("Points", EPlayer.GetAccount(player).Points);

            EPlayer.GetAccount(player).LastSessionMins = 0.0;

            EPlayer.Notify(player, "Login successful!", "green", "fas fa-check", false, 2000);

            foreach(Player adm in Settings.LoggedInPlayers.Where(p => EPlayer.GetAccount(p).Admin > 0)){
                EPlayer.ShowAdminNotes(adm, acc, AdminNote.ANoteType.OnConnect);
                EPlayer.ShowAdminNotes(adm, acc, AdminNote.ANoteType.Both, false);
            }

            player.TriggerEvent("showLoginBrowser", false);
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

            float[] features = new float[20];
            
            features = NAPI.Util.FromJson<float[]>(featureData);

            for (int i = 0; i < 20; i++)
            {
                try
                {
                    pChar.Features[i] = features[i];
                }
                catch { }
            }

            pChar.Appearance = new AppearanceData[10];
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


            if (player.HasData("cEditClothingData"))
            {
                pChar.Clothing = player.GetData<ClothingData[]>("cEditClothingData");
            }
            player.ResetData("cEditClothingData");

            EPlayer.GetAccount(player).Character = NAPI.Util.ToJson(pChar);
            EPlayer.ApplyCharacter(player, pChar);

            player.SetData("Creator_Using", false);
            player.TriggerEvent("toggleCreator", false);

            EPlayer.Save(player);

            EPlayer.Spawn(player);

            player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You can edit your character's clothing with /char clothing.");
            
            if(EPlayer.GetAccount(player).Level == 1 && EPlayer.GetAccount(player).XP < 2) 
                player.SendChatMessage("!{#FF0000}[!] !{#FFFFFF}Please use /help and /keys to get started. Press M to open menu.");
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

        [RemoteEvent("RequestSaveOutfit")]
        public void OnRequestSaveOutfit(Player player, int slot)
        {
            if (!EPlayer.LoggedIn(player)) return;

            Account acc = EPlayer.GetAccount(player);
            if (slot >= 3 && acc.DonatorLevel < 1)
            {
                player.SendNotification("~r~I don't know how you got here, but this slot is for donator only!");
                return;
            }
            if (slot == -1) return;

            string jsonReceived = ContextFactory.Instance.PlayerOutfits.ToList().Find(o => o.AccountID == EPlayer.GetAccount(player).SQL_ID).OutfitData;

            try
            {
                List<CharacterData> outfits = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CharacterData>>(jsonReceived);

                outfits[slot] = NAPI.Util.FromJson<CharacterData>(EPlayer.GetAccount(player).Character);
                ContextFactory.Instance.PlayerOutfits.ToList().Find(o => o.AccountID == EPlayer.GetAccount(player).SQL_ID).OutfitData = NAPI.Util.ToJson(outfits);

                ContextFactory.Instance.SaveChanges();

                player.SendChatMessage("!{#00FF00}You successfully saved your current outfit to slot " + (slot+1) + ".");
            }
            catch
            {
                player.SendNotification("~r~Error fetching outfits! Please try to relog or contact the devs.");
            }
        }

        [RemoteEvent("RequestSetOutfit")]
        public void OnRequestSetOutfit(Player player, int slot, int team, bool save)
        {
            if (!EPlayer.LoggedIn(player)) return;

            Account acc = EPlayer.GetAccount(player);
            if(slot >= 3 && acc.DonatorLevel < 1)
            {
                player.SendNotification("~r~I don't know how you got here, but this slot is for donator only!");
                return;
            }

            if (slot != -1)
            {
                try
                {
                    string jsonReceived = jsonReceived = ContextFactory.Instance.PlayerOutfits.ToList().Find(o => o.AccountID == EPlayer.GetAccount(player).SQL_ID).OutfitData;

                    List<CharacterData> outfits = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CharacterData>>(jsonReceived);

                    EPlayer.ApplyCharacter(player, outfits[slot], team);

                    if (save == true)
                    {
                        acc.Character = NAPI.Util.ToJson(outfits[slot]);
                        EPlayer.Save(player);
                        player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You selected your outfit on slot " + (slot + 1) + ".");
                    }
                }
                catch
                {
                    player.SendNotification("~r~Error fetching outfits! Please try to relog or contact the devs.");
                }
            }
            else
            {
                CharacterData normalChar = NAPI.Util.FromJson<CharacterData>(EPlayer.GetAccount(player).Character);
                EPlayer.ApplyCharacter(player, normalChar, team);

                if(save == true)
                {
                    acc.Character = NAPI.Util.ToJson(normalChar);
                    EPlayer.Save(player);
                }
            }
        }

        [RemoteEvent("RequestSetClothes")]
        public void OnRequestSetClothes(Player player, int slot, int drawable, int texture)
        {
            if (!EPlayer.LoggedIn(player)) return;

            if (Main.BlacklistedClothing(slot, drawable))
            {
                player.SendNotification("~r~This clothing is blacklisted!");
                return;
            }

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

            if (Main.BlacklistedClothing(1, drawable))
            {
                player.SendNotification("~r~This clothing is blacklisted!");
                return;
            }

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

            if (Main.BlacklistedClothing(0, drawable))
            {
                player.SendNotification("~r~This clothing is blacklisted!");
                return;
            }

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

        [RemoteEvent("SetWaypoint")]
        public static void OnPlayerSetWaypoint(Player player, float x, float y, float z)
        {
            if (!EPlayer.LoggedIn(player)) return;

            if (player.IsInVehicle)
            {
                foreach (Player t in RAGE.Entities.Players.All.Where(a => a.IsInVehicle && a.Vehicle == player.Vehicle))
                {
                    t.TriggerEvent("gameSetWaypoint", x, y);
                }
            }
            return;
        }

        [RemoteEvent("DisableWaypoint")]
        public static void OnPlayerDisableWaypoint(Player player)
        {
            if (!EPlayer.LoggedIn(player)) return;

            if (player.IsInVehicle)
            {
                foreach (Player t in RAGE.Entities.Players.All.Where(a => a.IsInVehicle && a.Vehicle == player.Vehicle))
                {
                    t.TriggerEvent("gameDisableWaypoint");
                }
            }
            return;
        }

        public static void OnPlayerRegister(Player player, Account acc)
        {
            player.SetSharedData("EntAlpha", 255);
            player.SetData("PlaytimeTick", Environment.TickCount64);
            Discord.SendTextChannel("staff-bot", $"*{player.Name} has registered.*");

            EPlayer.SetDiscordStatus(player, $"Registering as {acc.Username}");
            NAPI.Util.ConsoleOutput($"[Connection] {player.Name} has registered as {acc.Username}. (IP: {player.Address} - Serial: {player.Serial} - SC: {player.SocialClubName})");

            player.SetData("LoggedIn", true);
            player.SetData("AccountHandle", acc);
            player.SetSharedData("pLogged", true);

            player.ResetSharedData("LastKiller");
            player.ResetSharedData("BeingRevived");
            player.ResetSharedData("ReviveTarget");

            player.Name = acc.Username;

            player.TriggerEvent("DestroyPlayerCamera");
            player.TriggerEvent("TogglePlayerChat", true);

            player.ResetData("Language");

            player.SetData<long>("JoinTimeUnix", DateTimeOffset.Now.ToUnixTimeSeconds());

            EPlayer.ShowCharCreator(player);

            // Adding the 5 default characters to the outfits section so he has his own entry in the outfits table bla bla boring
            CharacterData[] defaultOutfits = new CharacterData[5];
            for (int i = 0; i < 5; i++)
            {
                defaultOutfits[i] = new CharacterData();
            }
            ContextFactory.Instance.PlayerOutfits.Add(new PlayerOutfit()
            {
                AccountID = acc.SQL_ID,
                OutfitData = NAPI.Util.ToJson(defaultOutfits)
            });
            ContextFactory.Instance.SaveChanges();

            //EPlayer.Spawn(player);
            EPlayer.Notify(player, "Registration successful!", "green", "fas fa-check", false, 2000);

            player.TriggerEvent("showLoginBrowser", false);
        }

        [RemoteEvent("OnPlayerLoginAttempt")]
        public static void OnPlayerLoginAttempt(Player player, string username, string password)
        {
            if (EPlayer.LoggedIn(player)) return;
            if (player.HasData("Kicked")) return;

            if ((username == null || username.Length < 2))
            {
                EPlayer.Error(player, "Username field is required!", "fas fa-user");
                return;
            }
            if ((password == null || password.Length < 2))
            {
                EPlayer.Error(player, "Password field is required!", "fas fa-lock");
                return;
            }

            NAPI.Util.ConsoleOutput($"[Login Attempt] Username {username} from {player.Name}");
            if (Environment.TickCount64 - player.GetData<long>("LastCryptAttempt") < 1000 || EPlayer.LoggedIn(player))
            {
                EPlayer.Error(player, "Please wait between each attempt.", "fas fa-lock");
                return;
            }
            player.SetData("LastCryptAttempt", Environment.TickCount64);
            if (username.Contains("@"))
            {
                Account acc = ContextFactory.Instance.Accounts.ToList().Find(a => a.Email != null && a.Email.Length > 5 && a.Email.Equals(username, StringComparison.OrdinalIgnoreCase));
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
                Account acc = ContextFactory.Instance.Accounts.ToList().Find(a => a.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
                if (acc != null)
                {
                    if (AccountManager.Login(acc, password))
                    {
                        OnPlayerLogin(player, acc);
                        return;
                    }
                }
            }
            EPlayer.Error(player, "Invalid credentials or unable to log-in!", "fas fa-lock");

        }

        [RemoteEvent("RequestLobbySwitch")]
        public void OnRequestLobbySwitch(Player player, int lobbyID, string password)
        {
            if (!EPlayer.LoggedIn(player)) return;

            if (player.GetSharedData<int>("InLobby") == lobbyID)
            {
                Commands.cmd_lobby(player);
            }
            else
            {
                if (password.Length > 1)
                    Commands.cmd_switchlobby(player, $"{lobbyID} {password}");
                else
                    Commands.cmd_switchlobby(player, $"{lobbyID}");
            }

            player.TriggerEvent("UpdateMenuLobbies", NAPI.Util.ToJson(Menu_LobbiesData(player)), player.GetSharedData<int>("InLobby"));
            player.TriggerEvent("UpdateMenuMinigames", NAPI.Util.ToJson(Menu_MinigamesData(player)), (player.GetSharedData<int>("InLobby") < 0) ? player.GetSharedData<int>("InLobby") : player.GetSharedData<int>("InDM"));
        }

        [RemoteEvent("RequestMinigameJoin")]
        public void OnRequestMinigameJoin(Player player, int dmID)
        {
            if (!EPlayer.LoggedIn(player)) return;

            if (!player.HasData("DMLobbySkipID"))
            {
                if (!player.HasData("LastLobbySwitch"))
                {
                    player.SetData<long>("LastLobbySwitch", Environment.TickCount64);
                }
                else
                {
                    if (Environment.TickCount64 - player.GetData<long>("LastLobbySwitch") < 1000)
                    {
                        player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot switch minigames that fast.");
                        EPlayer.Notify(player, "Please don't repeatedly join minigames.", "red", "fas fa-lock", false, 1000, "bottomCenter", "replace");
                        return;
                    }
                }
            }


            if (dmID == -3)
            {
                Commands.cmd_freeroam(player);
            }
            else if (dmID == -4)
            {
                Commands.cmd_derby(player);
            }
            else if (dmID == -999)
            {
                Commands.cmd_lobby(player);
            }
            else
            {
                if (player.HasData("DMLobbySkipID"))
                {
                    int skipToLobby = player.GetData<int>("DMLobbySkipID");
                    player.ResetData("DMLobbySkipID");
                    Commands.cmd_switchlobby(player, $"{skipToLobby}");
                    player.SetData("AutoJoin", false);
                }

                Commands.cmd_dm(player, dmID);
            }

            player.TriggerEvent("UpdateMenuLobbies", NAPI.Util.ToJson(Menu_LobbiesData(player)), player.GetSharedData<int>("InLobby"));
            player.TriggerEvent("UpdateMenuMinigames", NAPI.Util.ToJson(Menu_MinigamesData(player)), (player.GetSharedData<int>("InLobby") < 0) ? player.GetSharedData<int>("InLobby") : player.GetSharedData<int>("InDM"));
            player.SetData<long>("LastLobbySwitch", Environment.TickCount64);
        }

        public List<object[]> Menu_LobbiesData(Player player)
        {
            if (!EPlayer.LoggedIn(player)) return new List<object[]>();

            // Lobby Data

            List<object[]> lobbyItems = new List<object[]>();

            // ID, Name, Background, Gamemode, Password, Players, MaxPlayers, Description
            Settings.Lobbies.ForEach((LobbyData lobby) =>
            {
                int players = Settings.LoggedInPlayers.Count(a => a.GetSharedData<int>("InLobby") == lobby.lobbyId);
                lobbyItems.Add(new object[]
                {
                        lobby.lobbyId,
                        "Lobby " + lobby.lobbyId.ToString(),
                        lobby.Background,
                        Main.ConvertGameMode(lobby.gameMode),
                        EPlayer.GetAccount(player).Admin > 0 ? "" : lobby.Password,
                        players,
                        Settings.Dynamics.MaxInLobby,
                        players > 0 ? Main.ConvertGameStatus(lobby.gameStatus) : ""
                });
            });
            return lobbyItems;
        }

        public List<object[]> Menu_MinigamesData(Player player, int lobbyOverride = 0)
        {
            if (!EPlayer.LoggedIn(player)) return new List<object[]>();

            // DM Arena Data
            string[] dmBgs = {
                   "package://Artwork/DM1.jpg",
                   "package://Artwork/DM2.jpg",
                };

            List<object[]> dmItems = new List<object[]>();

            // ID, Name, Background, Players, MaxPlayers, Gamemode/Desc

            dmItems.Add(new object[]
            {
                -3, "Freeroam", "package://Artwork/Freeroam.png",
                Settings.LoggedInPlayers.Count(a => a.GetSharedData<int>("InLobby") == -3), 1000, "", "Enter a world with no limits"
            });


            dmItems.Add(new object[]
            {
                -4, "Derby", "package://Artwork/Derby.jpg",
                Settings.LoggedInPlayers.Count(a => a.GetSharedData<int>("InLobby") == -4), 1000, "", "Longest alive till end wins"
            });


            int lobby = player.GetSharedData<int>("InLobby");
            int dmArena = player.GetSharedData<int>("InDM");

            if (lobbyOverride != 0) lobby = lobbyOverride;

            if (lobby <= -1)
            {
                var lobbies = Settings.Lobbies.Where(a => a.Enabled && a.Password.Length < 2 && Settings.LoggedInPlayers.Count(b => b.GetSharedData<int>("InLobby") == a.lobbyId) < Settings.Dynamics.MaxInLobby).OrderByDescending(l => Settings.LoggedInPlayers.Count(b => b.GetSharedData<int>("InLobby") == l.lobbyId));
                if (lobbies.Count() > 0)
                {
                    player.SetData("DMLobbySkipID", lobbies.First().lobbyId);
                    return Menu_MinigamesData(player, lobbies.First().lobbyId);
                }
                else
                {
                    dmItems.Add(new object[]
                    {
                        666, "Not Available", dmBgs[0], 0, 0, "", "You must be in a copchase lobby to see and join DM arenas!"
                    });
                }
            }
            else
            {
                for (int i = 0; i < Settings.DMNames.Length; i++)
                {
                    dmItems.Add(new object[]
                    {
                        i,
                        Regex.Replace(Regex.Replace(Settings.DMNames[i], @"\(.*\)", ""), @"\s+", " "),
                        (dmBgs.Length > i) ? dmBgs[i] : dmBgs.OrderBy(c => Guid.NewGuid()).First(),
                        Settings.LoggedInPlayers.Count(a => a.GetSharedData<int>("InLobby") == player.GetSharedData<int>("InLobby") && a.GetSharedData<int>("InDM") == i),
                        7 + (Settings.LoggedInPlayers.Count(a => a.GetSharedData<int>("InLobby") == player.GetSharedData<int>("InLobby")) / 3),
                        "DM",
                        Settings.DMNames[i].Contains('(') ? Settings.DMNames[i].Split('(', ')')[1] : "",
                    });
                }          
            }

            if (dmArena >= 0 || player.GetSharedData<int>("InLobby") < 0)
            {
                dmItems.Add(new object[]
                {
                    -999, "Lobby", "package://Artwork/ExitMinigame.jpg",
                    0, 1, "Return to lobby", ""
                });
            }

            return dmItems;
        }

        [RemoteEvent("MenuSelectedVehicle")]
        public void OnMenuVehicleSelect(Player player, string model, int team)
        {
            if(Settings.Cars.Where(a => a.Model.Equals(model, StringComparison.OrdinalIgnoreCase) && a.Team == team).Count() == 0 || team > 1 || team < 0)
            {
                EPlayer.Notify(player, "An unknown error has occured. Bad juju?!", "red", "fas fa-ban", false, 2000, "bottomCenter", "replace");
                return;
            }

            Account acc = EPlayer.GetAccount(player);
            CarData car = Settings.Cars.ToList().Find(a => a.Model.Equals(model, StringComparison.OrdinalIgnoreCase) && a.Team == team);

            if (EPlayer.HasOwnedVehicle(player, model, team))
            {
                if (team == 0)
                {
                    EPlayer.Notify(player, $"Fugitive vehicle set to <strong>{car.Name}</strong>.", "blue", "fas fa-car", false, 3000, "bottomCenter", "replace");
                    acc.CarFugitive = car.Model;
                }
                else
                {
                    EPlayer.Notify(player, $"Police vehicle set to <strong>{car.Name}</strong>.", "blue", "fas fa-car", false, 3000, "bottomCenter", "replace");
                    acc.CarCop = car.Model;
                }

                player.TriggerEvent("UpdateMenuVehicles", NAPI.Util.ToJson(Menu_VehicleData(player)), acc.CarCop, acc.CarFugitive);
            }
            else
            {
                if(car.LevelRequired != 0 && car.LevelRequired > acc.Level)
                {
                    EPlayer.Notify(player, $"You need to be at least <strong>Level {car.LevelRequired}</strong> for this vehicle.", "red", "fas fa-lock", false, 3000, "bottomCenter", "replace");
                    return;
                }
                if (car.DonatorPointsRequired != 0 && car.DonatorPointsRequired > acc.DonatorPoints)
                {
                    EPlayer.Notify(player, $"You need <strong>{car.DonatorPointsRequired} Donator Points</strong> for this vehicle.", "red", "fas fa-lock", false, 3000, "bottomCenter", "replace");
                    return;
                }
                if (car.PointsRequired != 0 && car.PointsRequired > acc.Points)
                {
                    EPlayer.Notify(player, $"You need <strong>{car.PointsRequired} Points</strong> for this vehicle.", "red", "fas fa-lock", false, 3000, "bottomCenter", "replace");
                    return;
                }
                if (car.DonatorLevelRequired != 0 && car.DonatorLevelRequired > acc.DonatorLevel)
                {
                    EPlayer.Notify(player, $"This vehicle is restricted to <strong>{EPlayer.GetDonatorLevelName(car.DonatorLevelRequired)}</strong>.", "red", "fas fa-lock", false, 3000, "bottomCenter", "replace");
                    return;
                }

                EPlayer.GivePoints(player, -car.PointsRequired);
                acc.DonatorPoints -= car.DonatorPointsRequired;
                EPlayer.UnlockVehicle(player, car.Model, car.Team);
                EPlayer.Notify(player, $"You have unlocked <strong>{car.Name}</strong>. You may select it now.", "green", "fas fa-unlock", false, 5000, "bottomCenter", "replace");

                player.TriggerEvent("UpdateMenuVehicles", NAPI.Util.ToJson(Menu_VehicleData(player)), acc.CarCop, acc.CarFugitive);
            }
            return;
        }

        public List<object[]> Menu_VehicleData(Player player)
        {
            if (!EPlayer.LoggedIn(player)) return new List<object[]>();
            // Vehicle Data


            List<object[]> vehItems = new List<object[]>();
            foreach (var car in Settings.Cars)
            {
                string price = "";
                if (car.LevelRequired > 0) price = "Level " + car.LevelRequired.ToString();
                if (car.PointsRequired > 0) price = price + (price.Length > 3 ? " & " : "") + car.PointsRequired.ToString() + " Points";
                if (car.DonatorPointsRequired > 0) price = price + (price.Length > 3 ? " & " : "") + car.DonatorPointsRequired.ToString() + " Donator Points";
                if (car.DonatorLevelRequired > 0) price = price + (price.Length > 3 ? " & " : "") + EPlayer.GetDonatorLevelName(car.DonatorLevelRequired);
                if (price.Length < 2) price = "Free";

                vehItems.Add(new object[]
                {
                    car.Team,
                    car.Model,
                    car.Name,
                    car.Image,
                    price,
                    !EPlayer.HasOwnedVehicle(player, car.Model, car.Team),
                    car.Info
                });
            }
            
            return vehItems;
        }

        [RemoteEvent("MenuSelectedLoadout")]
        public void OnMenuLoadoutSelect(Player player, string name, int team)
        {
            if (Settings.Loadouts.Where(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && a.Team == team).Count() == 0 || team > 1 || team < 0)
            {
                EPlayer.Notify(player, "An unknown error has occured. Bad juju?!", "red", "fas fa-ban", false, 2000, "bottomCenter", "replace");
                return;
            }

            Account acc = EPlayer.GetAccount(player);
            LoadoutData loadout = Settings.Loadouts.ToList().Find(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && a.Team == team);

            if (EPlayer.HasOwnedLoadout(player, name, team))
            {
                if (team == 0)
                {
                    EPlayer.Notify(player, $"Fugitive loadout set to <strong>{loadout.Name}</strong>.", "blue", "fas fa-check", false, 3000, "bottomCenter", "replace");
                    acc.LoadoutFugitive = (loadout.Name.Equals("default", StringComparison.OrdinalIgnoreCase) ? "?" : loadout.Name);
                }
                else
                {
                    EPlayer.Notify(player, $"Police loadout set to <strong>{loadout.Name}</strong>.", "blue", "fas fa-check", false, 3000, "bottomCenter", "replace");
                    acc.LoadoutCop = (loadout.Name.Equals("default", StringComparison.OrdinalIgnoreCase) ? "?" : loadout.Name);
                }

                player.TriggerEvent("UpdateMenuLoadouts", NAPI.Util.ToJson(Menu_LoadoutData(player)), acc.LoadoutCop, acc.LoadoutFugitive);
            }
            else
            {
                if (loadout.LevelRequired != 0 && loadout.LevelRequired > acc.Level)
                {
                    EPlayer.Notify(player, $"You need to be at least <strong>Level {loadout.LevelRequired}</strong> for this loadout.", "red", "fas fa-lock", false, 3000, "bottomCenter", "replace");
                    return;
                }
                if (loadout.DonatorPointsRequired != 0 && loadout.DonatorPointsRequired > acc.DonatorPoints)
                {
                    EPlayer.Notify(player, $"You need <strong>{loadout.DonatorPointsRequired} Donator Points</strong> for this loadout.", "red", "fas fa-lock", false, 3000, "bottomCenter", "replace");
                    return;
                }
                if (loadout.PointsRequired != 0 && loadout.PointsRequired > acc.Points)
                {
                    EPlayer.Notify(player, $"You need <strong>{loadout.PointsRequired} Points</strong> for this loadout.", "red", "fas fa-lock", false, 3000, "bottomCenter", "replace");
                    return;
                }
                if (loadout.DonatorLevelRequired != 0 && loadout.DonatorLevelRequired > acc.DonatorLevel)
                {
                    EPlayer.Notify(player, $"This loadout is restricted to <strong>{EPlayer.GetDonatorLevelName(loadout.DonatorLevelRequired)}</strong>.", "red", "fas fa-lock", false, 3000, "bottomCenter", "replace");
                    return;
                }

                EPlayer.GivePoints(player, -loadout.PointsRequired);
                acc.DonatorPoints -= loadout.DonatorPointsRequired;
                EPlayer.UnlockLoadout(player, loadout.Name, loadout.Team);
                EPlayer.Notify(player, $"You have unlocked <strong>{loadout.Name}</strong>. You may select it now.", "green", "fas fa-unlock", false, 5000, "bottomCenter", "replace");

                player.TriggerEvent("UpdateMenuLoadouts", NAPI.Util.ToJson(Menu_LoadoutData(player)), acc.LoadoutCop, acc.LoadoutFugitive);
            }
            return;
        }

        public List<object[]> Menu_LoadoutData(Player player)
        {
            if (!EPlayer.LoggedIn(player)) return new List<object[]>();
            // Loadout Data


            List<object[]> loadoutItems = new List<object[]>();
            foreach (var loadout in Settings.Loadouts)
            {
                string weaponString = "weapons: [";

                foreach(var weapon in loadout.Weapons)
                {
                    weaponString += "{name: \"" + (string)weapon[2] + "\", img: \"" + (string)weapon[1] + "\", ammo: " + (int)weapon[3] + ",},";
                }

                weaponString += "],";

                string requireString = "requirements: [";

                if (loadout.LevelRequired > 0) requireString += $"{{icon: \"fas fa-level-up-alt\", text: \"Level {loadout.LevelRequired}\"}},";
                if (loadout.PointsRequired > 0) requireString += $"{{icon: \"bx bxs-dollar-circle\", text: \"{loadout.PointsRequired} Points\"}},";
                if (loadout.DonatorPointsRequired > 0) requireString += $"{{icon: \"fas fa-credit-card\", text: \"{loadout.DonatorPointsRequired} Donator Points\"}},";
                if (loadout.DonatorLevelRequired > 0) requireString += $"{{icon: \"bx bxs-crown\", text: \"{EPlayer.GetDonatorLevelName(loadout.DonatorLevelRequired)}\"}},";

                requireString += "],";

                loadoutItems.Add(new object[]
                {
                    loadout.Team,
                    loadout.Name,
                    loadout.Background,
                    !EPlayer.HasOwnedLoadout(player, loadout.Name, loadout.Team),
                    requireString,
                    weaponString
                });
            }

            return loadoutItems;
        }

        [RemoteEvent("OnToggleMenu")]
        public void OnPlayerToggleMenu(Player player, bool status)
        {
            if (!EPlayer.LoggedIn(player)) return;

            if(status == true) // OPEN: Update lobby data and everything else.
            {
                player.TriggerEvent("UpdateMenuLobbies", NAPI.Util.ToJson(Menu_LobbiesData(player)), player.GetSharedData<int>("InLobby"));
                player.TriggerEvent("UpdateMenuMinigames", NAPI.Util.ToJson(Menu_MinigamesData(player)), (player.GetSharedData<int>("InLobby") < 0) ? player.GetSharedData<int>("InLobby") : player.GetSharedData<int>("InDM"));
                player.TriggerEvent("UpdateMenuVehicles", NAPI.Util.ToJson(Menu_VehicleData(player)), EPlayer.GetAccount(player).CarCop, EPlayer.GetAccount(player).CarFugitive);
                player.TriggerEvent("UpdateMenuLoadouts", NAPI.Util.ToJson(Menu_LoadoutData(player)), EPlayer.GetAccount(player).LoadoutCop, EPlayer.GetAccount(player).LoadoutFugitive);
            }
            else // CLOSE: Anything you want when menu gets closed.
            {

            }
        }

        [RemoteEvent("ReturnAmmoPosZ")]
        public void OnReturnAmmoCoordinates(Player player, float x, float y, float z, uint dimension)
        {
            if (!EPlayer.LoggedIn(player) || !player.HasData("CreatingAmmoBox")) return;

            LobbyData lobby = Settings.Lobbies.Find(a => a.lobbyId == player.GetSharedData<int>("InLobby"));

            if (lobby == null) return;

            Pickup newPickup = new Pickup(NAPI.Util.GetHashKey("prop_box_ammo07b"), new Vector3(x, y, z), 2.0f, dimension, false, Pickups.ToPropLighting(255f, 255f, 255f, 1.0f, 1.5f, 0.5f));

            newPickup.SetData("AmmoBox", player.GetData<Dictionary<string, int>>("CreatingAmmoBox"));
            player.ResetData("CreatingAmmoBox");
            lobby.Drops.Add(newPickup);

            newPickup.OnPickup = (guy) =>
            {
                if (!guy.Dead && guy.Health > 0 && !guy.HasData("Dead") && guy.Position.DistanceTo(newPickup.position) < 20.0 && !guy.IsInVehicle) // so if player dies he isnt picking it up
                {
                    if (Environment.TickCount64 - 10000 > guy.GetData<long>("SpawnTick")) // spawned like more than 10 seconds ago
                    {
                        var ammoBox = newPickup.GetData<Dictionary<string, int>>("AmmoBox");
                        string notificationString = "";
                        foreach (KeyValuePair<long, jWeaponData> i in Settings.WeaponData)
                        {
                            if (guy.GetWeaponAmmo((WeaponHash)i.Key) > 0 || guy.CurrentWeapon == (WeaponHash)i.Key || (WeaponHash)i.Key == WeaponHash.Bzgas)
                            {
                                if (i.Value.AmmoType != null)
                                {
                                    int weaponAmmo = 0;
                                    if (ammoBox.TryGetValue(i.Value.AmmoType, out weaponAmmo))
                                    {
                                        guy.SetWeaponAmmo((WeaponHash)i.Key, guy.GetWeaponAmmo((WeaponHash)i.Key) + weaponAmmo);
                                        notificationString = notificationString + $"<p>+{weaponAmmo} ammo to <strong>{i.Value.Name}</strong></p>";
                                        ammoBox.Remove(i.Value.AmmoType);
                                    }
                                }
                            }
                        }

                        newPickup.SetData("AmmoBox", ammoBox);
                        if (notificationString.Length >= 3)
                        {
                            EPlayer.Action(guy, "restocks from an ammunition box.");
                            EPlayer.Notify(guy, notificationString, "green", "fas fa-boxes", false, 2000, "bottomRight");
                        }

                        if (ammoBox.Count == 0)
                        {
                            lobby.Drops.Remove(newPickup);
                            newPickup.Destroy();
                        }
                    }
                }
            };
        }

        [RemoteEvent("UpdateStreamData")]
        public void OnPlayerUpdateStreamData(Player player)
        {
            if (player.IsInVehicle) player.SetSharedData("UnstreamedState", 1);
            else player.SetSharedData("UnstreamedState", 0);

            if (player.GetSharedData<int>("Team") == 0 && player.GetSharedData<int>("InLobby") > 0)
            {
				if(player.IsInVehicle && player.Vehicle.HasSharedData("Team") && player.Vehicle.GetSharedData<int>("Team") == 1)
				{
                    player.SetSharedData("VisibleBlip", true);
                }
				if(player.GetSharedData<bool>("HasShot") == true)
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

        [RemoteEvent("GivePlayerShotTag")]
        public static void PlayerRequestShotTag(Player player)
        {
            if (player.GetSharedData<int>("Team") == 0)
            {
                if (player.GetSharedData<bool>("HasShot") == false)
                {
                    EPlayer.Notify(player, $"You have now shot, <strong>cops can now kill you on sight.</strong>", "red", "fas fa-walking", false, 1500, "bottomCenter", "replace");
                }

                player.SetSharedData("HasShot", true);
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

        [RemoteEvent("ReplaceAFKDriver")]
        public static void ReplaceAFKDriver(Player player)
        {
            if (player == null || !player.IsInVehicle || player.GetSharedData<int>("Team") == -1) return;

            try
            {
                Vehicle car = player.Vehicle;
                List<Entity> occupants = car.Occupants;
                occupants.Remove(player);

                if (occupants.Count <= 0) return;

                Player newDriver = (Player)occupants.OrderBy(c => Guid.NewGuid()).First();
                Commands.cmd_switchseats(player, newDriver);
                NAPI.Task.Run(() => Commands.cmd_switchseats(newDriver, player), 500 + newDriver.Ping);

                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Server detected you as an AFK driver and swapped your seat.");
                newDriver.SendChatMessage($"!{{#FF6347}}[!] !{{#FFFFFF}}{player.Name} is AFK, you are now the new driver.");
            }
            catch { }
        }

        [RemoteEvent("OnPlayerRegisterAttempt")]
        public static void OnPlayerRegisterAttempt(Player player, string email, string username, string password)
        {
            if (EPlayer.LoggedIn(player)) return;
            if (player.HasData("Kicked")) return;

            if ((username == null || username.Length < 2))
            {
                EPlayer.Error(player, "Username field is required!", "fas fa-user");
                return;
            }
            if ((password == null || password.Length < 2))
            {
                EPlayer.Error(player, "Password field is required!", "fas fa-lock");
                return;
            }

            NAPI.Util.ConsoleOutput($"[Register Attempt] Username {username} from {player.Name}");
            if (Environment.TickCount64 - player.GetData<long>("LastCryptAttempt") < 1000 || EPlayer.LoggedIn(player))
            {
                EPlayer.Error(player, "Please wait between each attempt.", "fas fa-lock");
                return;
            }
  
            if (password.Length < 5)
            {
                EPlayer.Error(player, "Password does not match requirements!", "fas fa-lock");
                return;
            }
            if (username.Contains("@") || username.Length < 4 || username.Length >= 24)
            {
                EPlayer.Error(player, "Username does not match requirements!", "fas fa-user");
                return;
            }
            Regex exp = new Regex(@"^[a-zA-Z0-9]*$");
            Match match = exp.Match(username);
            if (!match.Success)
            {
                EPlayer.Error(player, "Username contains illegal characters!", "fas fa-user");
                return;
            }
            if (email == null || !email.Contains("@") || email.Length < 5)
            {
                email = "N/A";
            }

            Account acc = ContextFactory.Instance.Accounts.ToList().Find(a => a.Username.Equals(username, StringComparison.OrdinalIgnoreCase) == true);
            if (acc != null)
            {
                EPlayer.Error(player, "An account with this name already exists!", "fas fa-user");
                return;
            }

            if (ContextFactory.Instance.Accounts.ToList().Count(a => a.RegisterHWID.Equals(player.Serial) || a.RegisterRID == player.SocialClubId) >= 2)
            {
                EPlayer.Error(player, "You cannot create any more accounts on the same machine!", "fas fa-user");
                return;
            }

            player.SetData("LastCryptAttempt", Environment.TickCount64);
            string salt = AccountManager.GenSalt();
            string newPass = AccountManager.Crypt(password, salt);

            Dictionary<long, wComponentData> wepItems = new Dictionary<long, wComponentData>();

            wComponentData test = new wComponentData();
            test.Components.Add(123456789);
            wepItems.Add(123456789, test);

            DiscordData _discordData = new DiscordData();
            string jsonDiscordData = NAPI.Util.ToJson(_discordData);

            var _new = new Account
            {
                Email = email,
                Password = newPass,
                Salt = salt,
                Username = username,
                DiscordInfo = jsonDiscordData,
                Admin = 0,
                RegisterHWID = player.Serial,
                RegisterIP = player.Address,
                RegisterRID = player.SocialClubId,
                RegisterSC = player.SocialClubName,
                LastHWID = player.Serial,
                LastIP = player.Address,
                LastRID = player.SocialClubId,
                LastSC = player.SocialClubName,
                MuteTime = 0,
                MuteReason = "N/A",
                JailTime = 0,
                JailReason = "N/A",
                DerbyWins = 0,
                Level = 1,
                Medkits = 0,
                Painkillers = 0,
                Language = -1,
                XP = 1,
                Escapes = 0,
                Arrests = 0,
                Kills = 1,
                Deaths = 1,
                MinutesPlayed = 1.0,
                LastSessionMins = 0.0,
                DonatorPoints = 0,
                Points = 10,
                Warns = 0,
                Suspended = 0,
                DonatorLevel = 0,
                LobbyClothing = 1,
                LoadoutCop = "?",
                LoadoutFugitive = "?",
                CarCop = "?",
                CarFugitive = "?",
                WeaponComponents = NAPI.Util.ToJson(wepItems),
                MOTD = "none",
                RegisterDate = DateTime.Now,
                LastLogin = DateTime.Now,
                Character = "{}",
                PurchasedAmmo = "{}",
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
        public void OnPlayerShootPlayer(Player player, Player target, uint bone, Int64 weapon)
        {
            if (weapon == (uint)WeaponHash.Stungun)
            {
                if (player.GetSharedData<int>("InLobby") > -1 && player.GetSharedData<int>("InDM") == 6 && target.GetSharedData<int>("InDM") == 6)
                {
                    //target.Health = 0;
                    return;
                }
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

                    if (target.HasData("TasedTill") && Environment.TickCount64 - (target.GetData<long>("TasedTill") - 18000) >= 2000)
                    {
                        if (lobby.GameProgress < 60)
                        {
                            player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot tase at the start of the round!");
                            return;
                        }

                        int time = 18000;
                        target.TriggerEvent("setToRagdoll", time, time * 2, 1, false, false, false);
                        target.SetData("TasedTill", Environment.TickCount64 + 18000);

                        if (lobby.Fugitives.Contains(target))
                        {
                            foreach (Player pl in Settings.LoggedInPlayers.Where(p => p.Position.DistanceTo(target.Position) < 15f && lobby.Cops.Contains(p)))
                            {
                                pl.TriggerEvent("ShowHandcuffTooltip");
                            }
                        }

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
                    EPlayer.GetAccount(assist).Arrests++;
                    assist.SendNotification("~g~+5 XP for helping bust " + fugitive.Name);
                }

                EPlayer.GrantXP(cop, 7);
                EPlayer.GetAccount(cop).Arrests++;
                cop.SendNotification("~g~+7 XP for busting " + fugitive.Name);

                EPlayer.SendMessageInLobby(lobby.lobbyId, $"!{{#ADD8E6}}Fugitive {fugitive.Name} has been busted!");

                Events.OnPlayerDeath(fugitive, cop, 696989); // Reason 696989 = handcuffs/busted (fast respawn).
            }
            return;
        }

        [RemoteEvent("FinishRevive")]
        public void OnRevivePlayer(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player)) return;
            if (player.GetSharedData<int>("Team") != target.GetSharedData<int>("Team")) return;

            target.Health = 50;
            target.SendChatMessage("!{#FFFF00}Your wounds have been treated and you gained your strength back.");
            
            //target.StopAnimation();
            RequestPlayAnimation(target, "amb@world_human_push_ups@male@exit", "exit", 34);

            NAPI.Task.Run(() =>
            {
                try
                {
                    if (target != null)
                    {
                        Events.RequestClearAnimation(target);
                        //target.StopAnimation();
                    }
                }
                catch { }
            }, 3800);

            target.SetSharedData("Wounded", 0);
            target.TriggerEvent("ResetWounded");

            player.SendChatMessage($"!{{#2fc900}}[REVIVE] You successfully revived {target.Name} and gained +2XP & 1 point for teamwork!");
            target.SendChatMessage($"!{{#2fc900}}[REVIVE] You have been revived by {player.Name}.");

            RequestClearAnimation(player);
            player.ResetSharedData("ReviveTarget");
            target.ResetSharedData("BeingRevived");
        }

        [RemoteEvent("AssignReviveTarget")]
        public void OnAssignReviveTarget(Player player, int remoteId)
        {
            Player target = RAGE.Entities.Players.All.Find(p => p.Id == remoteId);
            if(target == null)
            {
                return;
            }

            if (!EPlayer.LoggedIn(player) || !EPlayer.LoggedIn(target))
            {
                return;
            }
            if (player.GetSharedData<int>("Team") != target.GetSharedData<int>("Team"))
            {
                return;
            }

            player.SetSharedData("ReviveTarget", target);
            target.SetSharedData("BeingRevived", player);

            player.SendChatMessage($"!{{#2fc900}}[REVIVE] You have begun reviving ${target.Name} - keep holding H for 7 seconds.");
            target.SendChatMessage($"!{{#2fc900}}[REVIVE] You are now being revived by ${player.Name} - it will take 7 seconds.");

            RequestPlayAnimation(player, "missheistfbi3b_ig8_2", "cpr_loop_paramedic", 1);
            player.TriggerEvent("BeginReviving");
        }

        [RemoteEvent("RequestCuffTarget")]
        public void OnRequestCuffTarget(Player player)
        {
            if (!EPlayer.LoggedIn(player)) return;

            if (player.GetSharedData<int>("Team") != 1 || player.IsInVehicle) return;

            LobbyData playerLobby = Settings.Lobbies.Find(l => l.lobbyId == player.GetSharedData<int>("InLobby"));

            if (playerLobby == null || playerLobby.GameProgress < 60)
            {
                return;
            }

            Player target = Settings.LoggedInPlayers.Find(
                p => p.Position.DistanceTo(player.Position) < 3.0f
                && p.GetSharedData<int>("Team") == 0
                && p.GetData<bool>("Cuffed") == false
                && !p.IsInVehicle
                && p.GetSharedData<string>("CuffedBy").Equals("n/a") // making sure the fugitive isnt being cuffed by someone else
            );
            if (target == null) return;

            player.TriggerEvent("BeginCuffing", target);
            player.SendChatMessage($"!{{#FF6347}}[!] !{{#FFFFFF}}You began cuffing {target.Name}, keep holding !{{#FFFF00}}H !{{#FFFFFF}}to handcuff them.");
            EPlayer.Action(player, $"takes out a pair of handcuffs and starts locking them onto {target.Name}'s wrists.");

            Events.RequestPlayAnimation(player, "mp_arresting", "a_uncuff", 50); // 50 = allow movement while anim
            target.SetSharedData("CuffedBy", player.Name);
        }

        [RemoteEvent("StopCuffing")]
        public void OnStopCuffing(Player player)
        {
            Player target = Settings.LoggedInPlayers.Find(p => p.GetSharedData<string>("CuffedBy") == player.Name);
            if (target == null) return;
            target.SetSharedData("CuffedBy", "n/a");

            Events.RequestClearAnimation(player);
        }
        [RemoteEvent("StopReviving")]
        public void OnStopReviving(Player player)
        {
            if (!player.HasSharedData("ReviveTarget")) return;

            Player target = player.GetSharedData<Player>("ReviveTarget");
            if (target == null) return;
            target.ResetSharedData("BeingRevived");
            player.ResetSharedData("ReviveTarget");

            if(player.GetSharedData<int>("Wounded") == 0)
            {
                Events.RequestClearAnimation(player);
            }
        }

        [RemoteEvent("ScreenshotReceived")]
        public void OnReceiveSS(Player player, int result, string data, string identifier)
        {
            if (!identifier.Equals("null"))
            {
                if (identifier.StartsWith("adm_"))
                {
                    string adminName = identifier.Replace("adm_", "");
                    Player admin = Settings.LoggedInPlayers.Find(a => a.Name.Equals(adminName, StringComparison.OrdinalIgnoreCase));
                    if(admin != null)
                    {
                        if (result == 1)
                        {
                            admin.SendChatMessage("!{#33C446}[#] !{#FFFFFF}The screenshot has been uploaded. To view it use /viewss last or /viewss (id)");
                            admin.SendChatMessage($"!{{#33C446}}[#] !{{#FFFFFF}}Controls: F4 (Stop viewing), F2 (Toggle cursor) | Metadata for preserving/viewing:");
                            admin.SendChatMessage($"!{{#FF6347}}ID: {data} | Requestee: {identifier} | Source: {player.Name}");
                            admin.SetData("LastSSIdentifier", data);
                        }
                        else
                        {
                            admin.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}The screenshot of !{#FF6347}" + player.Name + "'s !{#FFFFFF}game has failed to upload.");
                            admin.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Error Details: " + data);
                        }
                    }
                }
            }
            NAPI.Util.ConsoleOutput($"{player.Name}'s screenshot result: {result} at: {data} with identifier: {identifier}");
            return;
        }

        [RemoteEvent("GIFReceived")]
        public void OnReceiveGIF(Player player, int result, string data, string identifier)
        {
            player.ResetData("GIF_Creating");

            if (!identifier.Equals("null"))
            {
                if (identifier.StartsWith("adm_"))
                {
                    string adminName = identifier.Replace("adm_", "");
                    Player admin = Settings.LoggedInPlayers.Find(a => a.Name.Equals(adminName, StringComparison.OrdinalIgnoreCase));
                    if (admin != null)
                    {
                        if (result == 1)
                        {
                            admin.SendChatMessage("!{#33C446}[#] !{#FFFFFF}The GIF has been uploaded. To view it use /viewss last or /viewss (id)");
                            admin.SendChatMessage($"!{{#33C446}}[#] !{{#FFFFFF}}Controls: F4 (Stop viewing), F2 (Toggle cursor) | Metadata for preserving/viewing:");
                            admin.SendChatMessage($"!{{#FF6347}}ID: {data} | Requestee: {identifier} | Source: {player.Name}");
                            admin.SetData("LastSSIdentifier", data);
                        }
                        else
                        {
                            admin.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}The GIF of !{#FF6347}" + player.Name + "'s !{#FFFFFF}game has failed to upload.");
                            admin.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Error Details: " + data);
                        }
                    }
                }
            }
            NAPI.Util.ConsoleOutput($"{player.Name}'s GIF result: {result} at: {data} with identifier: {identifier}");
            return;
        }

        [RemoteEvent("FinishCuffing")]
        public void OnFinishCuffing(Player player, Player target)
        {
            if (target == null) return;

            LobbyData lobbye = Settings.Lobbies.Find(lob => lob.lobbyId == player.GetSharedData<int>("InLobby"));

            if (lobbye == null || !lobbye.Cops.Contains(player) || !lobbye.Fugitives.Contains(target)) return;

            Events.RequestClearAnimation(player);
            EPlayer.GrantXP(player, 5);
            player.SetSharedData("FugitiveChance", player.GetSharedData<float>("FugitiveChance") + (0.2 * (double)((double)EPlayer.GetAccount(player).DonatorLevel + 1.0)));
            EPlayer.Action(player, $"has grabbed ahold of {target.Name} and slapped the handcuffs on their wrists.");

            EPlayer.SendMessageInLobby(lobbye.lobbyId, $"!{{#ADD8E6}}Cop {player.Name} has apprehended Fugitive {target.Name}.");

            target.SetAccessories(7, 41, 0); // Handcuffs

            EPlayer.Handcuff(target, true);

            EPlayer.GetAccount(player).Arrests++;
            EPlayer.GivePoints(player, 5);

            Events.OnPlayerDeath(target, player, 696988); // Reason 696988 = handcuffs.

            target.SetSharedData("CuffedBy", "n/a");
        }

        [RemoteEvent("RequestCompSync")]
        public void OnPlayerRequestCompSync(Player player, Player target)
        {
            try
            {
                EPlayer.SyncComponents(player, target);
            }
            catch { }
        }

        [RemoteEvent("SendSirenSync")]
        public void OnPlayerSyncSiren(Player player, Vehicle vehicle, bool state)
        {
            if (vehicle != null) vehicle.SetSharedData("SirenState", state);
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

        [RemoteEvent("RequestSetInVehicle")]
        public void ChaseRequestSetInCar(Player player, Vehicle veh, int seat)
        {
            if (player.GetSharedData<int>("Team") >= 0)
            {
                player.SetIntoVehicle(veh, seat);
                player.TriggerEvent("ChangeVehicleHeading", veh.GetData<float>("SpawnA"));
            }
        }

        [RemoteEvent("TunerRequestDrive")]
        public void OnRequestTunerSet(Player player, Vehicle veh)
        {
            if (player.GetData<bool>("Tuner_Using") == true && !player.IsInVehicle)
            {
                player.SetIntoVehicle(veh, 0);
            }
        }

        [RemoteEvent("TunerSwitch")]
        public void OnSwitchTunerCamera(Player player, int leftClickOrRight)
        {
            if (!EPlayer.LoggedIn(player)) return;

            int pos = player.GetData<int>("Tuner_Pos");
            if (leftClickOrRight == 1)
            {
                pos++;
                if (pos >= 4) pos = 0;
            }
            else
            {
                pos--;
                if (pos <= -1) pos = 3;
            }

            int x = -1, y = -1, z = -1, rx = -1, ry = -1, rz = -1;

            switch (pos)
            {
                case 0: // Main / Front Side
                    x = -373; y = -121; z = 39; rx = -10; ry = 0; rz = -10;
                    break;
                case 1: // Full Front
                    x = -377; y = -120; z = 39; rx = 0; ry = 0; rz = 300;
                    break;
                case 2: // Other Side
                    x = -376; y = -115; z = 39; rx = -10; ry = 0; rz = 240;
                    break;
                case 3: // Back
                    x = -368; y = -115; z = 39; rx = 0; ry = 0; rz = 120;
                    break;
            }

            player.SetData("Tuner_Pos", pos);
            player.TriggerEvent("DestroyPlayerCamera");
            player.TriggerEvent("CreatePlayerCamera", x, y, z, rx, ry, rz, 60, true);
        }

        [RemoteEvent("RequestVehicleMod")]
        public void OnRequestVehicleTune(Player player, int slot, int mod)
        {
            if (!EPlayer.LoggedIn(player) || !player.IsInVehicle) return; // Not using tuner nor in freeroam.

            Vehicle veh = null;
            if (player.GetData<bool>("Tuner_Using") == true) veh = player.GetData<Vehicle>("Tuner_Vehicle");
            else if (player.GetSharedData<int>("InLobby") == -3) veh = player.Vehicle;
            else return;

            if (slot == 66)
            {
                veh.PrimaryColor = mod;
            }
            else if (slot == 67)
            {
                veh.SecondaryColor = mod;
            }
            else if (slot == 68)
            {
                veh.PearlescentColor = mod;
            }
            else if (slot == 69)
            {
                veh.WheelColor = mod;
            }
            else {
                if (mod == -1) veh.SetMod(slot, -1);
                else veh.SetMod(slot, mod);
            }

            player.TriggerEvent("ModRequestComplete");
        }

        [RemoteEvent("OnTunerClose")]
        public void OnCloseTuner(Player player)
        {
            if (player.GetData<bool>("Tuner_Using") == false) return;

            player.SetData("Tuner_Using", false);
            player.Position = new Vector3(0, 0, 0);
            player.GetData<Vehicle>("Tuner_Vehicle").Delete();
            player.ResetData("Tuner_Vehicle");
            player.TriggerEvent("DestroyPlayerCamera");

            EPlayer.Spawn(player);
        }

        [RemoteEvent("RequestVehicleModPrice")]
        public void OnRequestVehicleModPrice(Player player, string modstring)
        {
            if (!EPlayer.LoggedIn(player) || player.GetData<bool>("Tuner_Using") == false) return;

            Account acc = EPlayer.GetAccount(player);
            var tuning = player.GetData<PlayerTunings>("Tuner_CurrentItems");

            Dictionary<int, int> vMods = NAPI.Util.FromJson<Dictionary<int, int>>(modstring);
            vMods.Remove(11); // Engine
            vMods.Remove(12); // Brakes
            vMods.Remove(13); // Transmission
            vMods.Remove(16); // Armor
            vMods.Remove(18); // Turbo

            int items = 0;

            if (vMods.Count > 0)
            {
                if (tuning == null)
                {
                    items = vMods.Keys.Count;
                }
                else
                {
                    var oldMods = NAPI.Util.FromJson<Dictionary<int, int>>(tuning.Mods);
                    items = 0;

                    foreach (var key1 in vMods)
                    {
                        int oldVal = -1;
                        if (oldMods.TryGetValue(key1.Key, out oldVal))
                        {
                            if (key1.Value != oldVal && oldVal != -1 && key1.Value != -1) items++;
                        }
                        else if (key1.Value != -1) items++;
                    }
                }
            }

            if (items >= 3)
            {
                player.TriggerEvent("UpdateTunerPrice", $"Buy: {((items != 0) ? ($"{items * 10} Donator Points") : ("N/A"))}", $"{(items == 0 ? "No" : items.ToString())} components changed.", 235, 205, 12);
            }
            else player.TriggerEvent("UpdateTunerPrice", $"Buy: {((items != 0) ? ($"{items * 50} Points") : ("N/A") )}", $"{(items == 0 ? "No" : items.ToString())} components changed.", 220, 10, 10);
        }

        [RemoteEvent("RequestSaveVehicleMod")]
        public void OnRequestSaveVehicleMod(Player player, string modstring, int priCol, int secCol, int pearlCol, int wheelCol)
        {
            if (!EPlayer.LoggedIn(player) || player.GetData<bool>("Tuner_Using") == false) return;

            Account acc = EPlayer.GetAccount(player);
            var tuning = ContextFactory.Instance.PlayerTunings.ToList().Find(a => a.AccountSQLID == acc.SQL_ID && a.Model == player.Vehicle.Model);

            Dictionary<int, int> vMods = NAPI.Util.FromJson<Dictionary<int, int>>(modstring);
            vMods.Remove(11); // Engine
            vMods.Remove(12); // Brakes
            vMods.Remove(13); // Transmission
            vMods.Remove(16); // Armor
            vMods.Remove(18); // Turbo

            if (vMods.Count < 1)
            {
                if (tuning != null)
                {
                    if (!player.HasData("TunerNoRefundWarn"))
                    {
                        player.SetData("TunerNoRefundWarn", true);
                        EPlayer.Notify(player, "Removing all components! <p>THIS ACTION IS IRREVERSIBLE AND NON REFUNDABLE, USE AGAIN TO PROCEED.</p>", "red", "fas fa-exclamation", false, 2500, "topCenter", "replace");
                        return;
                    }

                    ContextFactory.Instance.PlayerTunings.Remove(tuning);
                }
            }
            else
            {
                if (tuning == null)
                {
                    if (vMods.Keys.Count >= 3)
                    {
                        if (acc.DonatorPoints < (vMods.Keys.Count * 10))
                        {
                            EPlayer.Notify(player, $"You need {(vMods.Keys.Count * 10) - acc.DonatorPoints} more donator points for this upgrade! <p>You can only purchase max 2 components with standard points.</p>", "red", "fas fa-lock", false, 2500, "topCenter", "replace");
                            return;
                        }
                        acc.DonatorPoints -= (vMods.Keys.Count * 10);
                    }
                    else
                    {
                        if (acc.Points < (vMods.Keys.Count * 50))
                        {
                            EPlayer.Notify(player, $"You need {(vMods.Keys.Count * 50) - acc.Points} more points for this upgrade!", "red", "fas fa-lock", false, 2500, "topCenter", "replace");
                            return;
                        }
                        EPlayer.GivePoints(player, -(vMods.Keys.Count * 50));
                    }
                    EPlayer.Notify(player, $"Modifications saved!", "green", "fas fa-check", false, 2500, "topCenter");

                    tuning = new PlayerTunings
                    {
                        AccountSQLID = acc.SQL_ID,
                        Model = player.Vehicle.Model,
                        PrimaryColor = priCol,
                        SecondaryColor = secCol,
                        PearlescentColor = pearlCol,
                        WheelColor = wheelCol,
                        Mods = NAPI.Util.ToJson(vMods),
                    };
                    ContextFactory.Instance.PlayerTunings.Add(tuning);
                }
                else
                {
                    var oldMods = NAPI.Util.FromJson<Dictionary<int, int>>(tuning.Mods);
                    int changes = 0;

                    foreach (var key1 in vMods)
                    {
                        int oldVal = -1;
                        if (oldMods.TryGetValue(key1.Key, out oldVal))
                        {
                            if (key1.Value != oldVal && oldVal != -1 && key1.Value != -1) changes++;
                        }
                        else if (key1.Value != -1) changes++;
                    }

                    if (changes >= 3)
                    {
                        if (acc.DonatorPoints < (changes * 10))
                        {
                            EPlayer.Notify(player, $"You need {(changes * 10) - acc.DonatorPoints} more donator points for this upgrade! <p>You can only purchase max 2 components with standard points.</p>", "red", "fas fa-lock", false, 2500, "topCenter", "replace");
                            return;
                        }
                        acc.DonatorPoints -= (changes * 10);
                    }
                    else
                    {
                        if (acc.Points < (changes * 50))
                        {
                            EPlayer.Notify(player, $"You need {(changes * 50) - acc.Points} more points for this upgrade!", "red", "fas fa-lock", false, 2500, "topCenter", "replace");
                            return;
                        }
                        EPlayer.GivePoints(player, -(changes * 50));
                    }

                    if (changes > 0)
                    {
                        EPlayer.Notify(player, $"Modifications saved!", "green", "fas fa-check", false, 2500, "topCenter");

                        tuning.PrimaryColor = priCol;
                        tuning.SecondaryColor = secCol;
                        tuning.PearlescentColor = pearlCol;
                        tuning.WheelColor = wheelCol;
                        tuning.Mods = NAPI.Util.ToJson(vMods);
                    }
                }
            }
            player.TriggerEvent("CloseTuner");
            ContextFactory.Instance.SaveChanges();
        }

        [ServerEvent(Event.PlayerSpawn)]
        public static void OnPlayerSpawn(Player player)
        {
            if (player.HasData("Kicked")) return;

            if (player == null || !player.Exists) return;

            player.SetSharedData("Ping", player.Ping);
            player.SetData("SpawnTick", Environment.TickCount64);
            player.ResetData("Dead");
            player.ResetData("Seatbelt");
            player.TriggerEvent("ToggleSeatbelt", false);
            player.ResetSharedData("FlashlightEquipped");
            player.ResetSharedData("UpdateCameraData");
            player.SetSharedData("Collisions", true); // Reset collision from chase.
            player.SetSharedData("ClimbProtection", false);
            player.SetData("LastClimbProtection", Environment.TickCount64-30000);
            player.SetData("ClimbProtectionProcs", 0);
            player.ResetData("DraggingOut");

            player.TriggerEvent("ResetWounded");
            player.SetSharedData("Wounded", 0);

            player.ResetData("_currentlySwappingSeats");
            foreach (Player p in Settings.LoggedInPlayers.Where(a => a.HasData("Spectating") && a.GetData<Player>("SpectatingPlayer") == player))
            {
                if (p == null || !player.Exists) continue;

                Commands.cmd_specoff(p);
            }
            player.SetData("Killstreak", 0);
            player.ResetData("AttachedComponents");

            lock (player)
            {
                Account acc = EPlayer.GetAccount(player);

                if (acc != null && acc.MuteTime > 0)
                {
                    player.SetSharedData("MuteTime", acc.MuteTime);
                    player.TriggerEvent("SetMuteCounter", acc.MuteTime);
                }

                player.SetSharedData("BustedProgress", 0.0);
                player.SetSharedData("CuffedBy", "n/a");

                if (player.HasData("Cuffed"))
                {
                    player.TriggerEvent("SetHandcuffs", false);

                    CharacterData ConvChar = new CharacterData();

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

                player.SetSharedData("Level", acc.Level);
                player.SetSharedData("HasShot", false);

                if (acc.JailTime > 0)
                {
                    EPlayer.SetDiscordStatus(player, $"In jail as {player.Name}");

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

                if (player.GetSharedData<int>("InDM") == -1) player.TriggerEvent("SetActorInvulnerable", true);
                player.TriggerEvent("ToggleAFOB", false, 0);
                player.TriggerEvent("showHPBars", 0); // Never show HP Bars.

                EPlayer.SetColor(player, new Color(255, 255, 255, 255));


                player.TriggerEvent("ToggleHud", true);



                if (player.GetSharedData<int>("InLobby") == -3) // Freeroam
                {
                    SpawnSet spawnCoords = new SpawnSet(0, 1061.547f, -720.113f, 56.786f, 130.497f);

                    Settings.FreeroamSpawns.ForEach(spawn =>
                    {
                        if(spawn.ID == player.GetData<uint>("FreeroamWorld"))
                        {
                            spawnCoords = spawn;
                        }
                    });
                    player.Position = spawnCoords.Coords;
                    player.Heading = spawnCoords.Heading;

                    EPlayer.SetDiscordStatus(player, $"Freeroam as {player.Name}");

                    EPlayer.SetColor(player, new Color(102, 232, 217), 0);

                    player.TriggerEvent("SetActorInvulnerable", player.GetData<bool>("FreeroamGodmode"));

                    player.SetSharedData("VisibleBlip", player.GetData<bool>("FreeroamBlip"));

                    player.TriggerEvent("SetTimeWeather", player.GetData<int>("FreeroamTime"), 0, 0, player.GetData<int>("FreeroamWeather"));

                    player.RemoveAllWeapons();

                    player.Dimension = (uint)256 + player.GetData<uint>("FreeroamWorld");
                }

                if (player.GetSharedData<int>("InLobby") == -4) // Derby
                {
                    player.TriggerEvent("SetTimeWeather", 12, 0, 0, 0);
                    player.TriggerEvent("showHPBars", 0); // Never show HP Bars.

                    player.SetData("DerbySpawnTick", Environment.TickCount64);

                    Random rand = new Random(DateTimeOffset.UtcNow.Millisecond);
                    int spawnID = rand.Next(Settings.DerbySpawns.GetLength(0));

                    if (player.HasData("DerbyVehicle"))
                    {
                        try { player.GetData<Vehicle>("DerbyVehicle").Delete(); }
                        catch { }

                        player.ResetData("DerbyVehicle");
                    }

                    Vector3 PlayerSpawn = (Vector3)Settings.DerbySpawns[spawnID, 0];
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
                            if (p == null || !p.Exists) continue;

                            p.TriggerEvent("SetDerbyCounting", true, Settings.DerbyTimeLeft);
                        }
                    }
                    else
                    {
                        player.SendNotification("~r~Derby time will not be counted unless there are enough players.");
                        Settings.DerbyTimeLeft = 180;
                        Settings.LastDerbyTick = Environment.TickCount64;
                    }

                    Vehicle derbyVeh = NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(Settings.Dynamics.DerbyVehicle), PlayerSpawn, (float)Settings.DerbySpawns[spawnID, 1], rand.Next(255), rand.Next(255), "DERBY", 255, true, true, 500);
                    player.SetData("DerbyVehicle", derbyVeh);
                    derbyVeh.Locked = true;
                    derbyVeh.SetSharedData("Locked", true);
                    derbyVeh.EngineStatus = true;
                    derbyVeh.SetSharedData("Engine", true);

                    player.SetIntoVehicle(derbyVeh, 0);

                    NAPI.Task.Run(() =>
                    {
                        try
                        {
                            if (player != null && player.Exists)
                            {
                                player.SetIntoVehicle(derbyVeh, 0);
                                EPlayer.SetColor(player, new Color(41, 214, 101), 0);
                            }
                        }
                        catch { }
                    }, 750);

                    int delayTimeCalc = 50 + player.Ping;
                    NAPI.Task.Run(() =>
                    {
                        try
                        {
                            if (player != null && player.Exists)
                            {
                                player.SetIntoVehicle(derbyVeh, 0);
                                EPlayer.SetColor(player, new Color(41, 214, 101), 0);
                            }
                        }
                        catch { }
                    }, delayTimeCalc);
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
                        EPlayer.Spawn(player, false);
                        player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You have been removed from DM as it's locked while copchase starts.");
                    }

                    if (arenaID > -1 && lobby != null && lobby.gameStatus != 2) // DM
                    {
                        Random rand = new Random(DateTimeOffset.UtcNow.Millisecond);

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

                        int delayTimeCalc = 250 + player.Ping;
                        NAPI.Task.Run(() =>
                        {
                            try
                            {
                                if (player != null && player.Exists)
                                {
                                    if (arenaID != 1 && arenaID != 5 && arenaID != 6 && arenaID != 7)
                                    {
                                        switch (new Random(DateTimeOffset.UtcNow.Millisecond).Next(0, 5))
                                        {
                                            case 0:
                                                player.GiveWeapon(WeaponHash.Appistol, 1000);
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
                                                player.GiveWeapon(WeaponHash.Pistol50, 1000);
                                                player.GiveWeapon(WeaponHash.Carbinerifle, 1000);
                                                player.GiveWeapon(WeaponHash.Sawnoffshotgun, 1000);
                                                player.GiveWeapon(WeaponHash.Minismg, 1000);
                                                break;
                                            case 3:
                                                player.GiveWeapon(WeaponHash.Assaultsmg, 1000);
                                                player.GiveWeapon(WeaponHash.Pistol, 1000);
                                                player.GiveWeapon(WeaponHash.Bottle, 1000);
                                                player.GiveWeapon(WeaponHash.Autoshotgun, 1000);
                                                break;
                                            case 4:
                                                player.GiveWeapon(WeaponHash.Compactrifle, 1000);
                                                player.GiveWeapon(WeaponHash.Combatpdw, 1000);
                                                player.GiveWeapon(WeaponHash.Pistol50, 1000);
                                                player.GiveWeapon(WeaponHash.Assaultshotgun, 1000);
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        switch (arenaID)
                                        {
                                            case 1:
                                                player.GiveWeapon(WeaponHash.Pistol_mk2, 1000);
                                                break;
                                            case 5:
                                                player.GiveWeapon(WeaponHash.Pistol, 1000);
                                                player.GiveWeapon(WeaponHash.Pumpshotgun, 1000);
                                                player.Armor = 0;
                                                player.Health = 100;
                                                break;
                                            case 6:
                                                player.GiveWeapon(WeaponHash.Stungun, 1000);
                                                player.Health = 1;
                                                player.Armor = 0;
                                                break;
                                            case 7:
                                                player.GiveWeapon(WeaponHash.Pistol, 1000);
                                                player.GiveWeapon(WeaponHash.Smg, 1000);
                                                player.GiveWeapon(WeaponHash.Specialcarbine, 1000);
                                                player.GiveWeapon(WeaponHash.Marksmanrifle, 1000);
                                                player.GiveWeapon(WeaponHash.Musket, 1000);
                                                break;
                                        }
                                    }

                                    if (EPlayer.GetAccount(player).WeaponComponents != null && EPlayer.GetAccount(player).WeaponComponents.Length > 3)
                                    {
                                        Dictionary<long, wComponentData> weaponCompsData = new Dictionary<long, wComponentData>();
                                        weaponCompsData = NAPI.Util.FromJson<Dictionary<long, wComponentData>>(EPlayer.GetAccount(player).WeaponComponents);

                                        foreach (KeyValuePair<long, wComponentData> i in weaponCompsData)
                                        {
                                            if (player.GetWeaponAmmo((WeaponHash)i.Key) > 0)
                                            {
                                                i.Value.Components.ForEach((long comphash) =>
                                                {
                                                    EPlayer.AttachComponent(player, i.Key, comphash);
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                            catch { }
                        }, delayTimeCalc);
                    }
                }
                else if(player.GetSharedData<int>("InLobby") == -6)
                {
                    player.RemoveAllWeapons();

                    player.TriggerEvent("SetActorInvulnerable", false);

                    var arena = EPlayer.IsValidDMArena(player.GetSharedData<int>("InDM"));

                    if(arena.Value == false)
                    {
                        player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}The arena you are playing in does not exist.");
                        player.SetSharedData("InDM", -1);
                        player.SetSharedData("Team", -1);

                        EPlayer.Spawn(player);
                        return;
                    }

                    var team = EPlayer.IsValidDMTeam(arena.Key.SQL_ID, player.GetSharedData<int>("Team"));

                    if (team.Value == false)
                    {
                        player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}The arena you are playing in does not have the team you are set as, resetting...");
                        player.SetSharedData("InDM", -1);
                        player.SetSharedData("Team", -1);

                        EPlayer.Spawn(player);
                        return;
                    }


                    player.TriggerEvent("showHPBars", team.Key.ShowHPBars);

                    var spawn = ContextFactory.Instance.CustomArenas_Spawns.ToList().OrderBy(a => Guid.NewGuid()).FirstOrDefault(a => a.ArenaID == arena.Key.SQL_ID &&
                        (a.Team == team.Key.SharedSpawns || a.Team == team.Key.Team));

                    if (spawn != null && spawn != default(CustomDMArenaSpawns))
                    {
                        player.Health = team.Key.Health;
                        player.Armor = team.Key.Armour;
                        player.SetSharedData("VisibleBlip", team.Key.Blip);
                        player.SetSharedData("DMBlipColor", team.Key.BlipColor);
                        player.SetSharedData("EntAlpha", team.Key.Alpha);
                        EPlayer.SetColor(player, new Color(team.Key.ColorR, team.Key.ColorG, team.Key.ColorB), team.Key.Armour);

                        player.Position = new Vector3(spawn.X, spawn.Y, spawn.Z);
                        player.Heading = spawn.A;

                        EPlayer.SetDiscordStatus(player, $"[Deathmatch] Team {team.Key.Name} in {arena.Key.Name}");

                        int delayTimeCalc = 250 + player.Ping;
                        NAPI.Task.Run(() =>
                        {
                            try
                            {
                                if (player != null && player.Exists)
                                {
                                    if (team.Key.WeaponSets == null || team.Key.WeaponSets.Length < 5)
                                    {
                                        player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}This DM arena does not have weapons set for your team " + team.Key.Team);
                                        return;
                                    }

                                    DM_WeaponSets[] weaponSets = NAPI.Util.FromJson<DM_WeaponSets[]>(team.Key.WeaponSets);
                                    var set = weaponSets.OrderBy(a => Guid.NewGuid()).FirstOrDefault();

                                    if (set == null)
                                    {
                                        player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}This DM arena does not have a proper weapons set for your team " + team.Key.Team);
                                        return;
                                    }

                                    foreach (var wep in set.Weapons)
                                    {
                                        player.GiveWeapon((WeaponHash)wep.Weapon, wep.Ammo);

                                        foreach (var comp in wep.Components)
                                        {
                                            EPlayer.AttachComponent(player, wep.Weapon, comp);
                                        }
                                    }
                                }
                            }
                            catch { }
                        }, delayTimeCalc);
                    }
                    else
                    {
                        player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}The arena you are playing in does not have a spawn for your team id: " + team.Key.Team);
                    }
                }
            }
        }
    }
}
