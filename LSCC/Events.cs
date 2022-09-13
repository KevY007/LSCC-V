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
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace RNC
{
    class Events : Script
    {
        [ServerEvent(Event.PlayerConnected)]
        public void OnPlayerConnected_Old(Player player)
        {
            EPlayer.ResetPlayerData(player);
            player.SetSharedData("Wounded", 0);
        }

        [RemoteEvent("ServerConsoleOutput")]
        public void ServerConsoleOutput(Player player, string output)
        {
            NAPI.Util.ConsoleOutput(output);
        }

        [RemoteEvent("KickPlayer")]
        public void KickPlayer(Player player, string reason, bool silent)
        {
            if (player.HasData("Kicked")) return;

            player.SetData("Kicked", true);
            NAPI.Task.Run(() => 
            {
                try
                {
                    if(reason == "AFK")
                        EPlayer.SendMessageToAll($"!{{#FF6347}}AdmCmd: Player {player.Name} has been kicked by SERVER for staying AFK too long.");
                    if (silent)
                    {
                        player.KickSilent(reason);
                    }
                    else
                    {
                        player.Kick(reason);
                    }
                }
                catch { } 
            }, 1000);
            return;
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

            player.TriggerEvent("InitChaseTimer");

            player.SetSharedData("PlayerWeather", -1);

            string pSerial = player.Serial;
            string pAddress = player.Address;
            ulong pSCID = player.SocialClubId;
            NAPI.Notification.SendNotificationToPlayer(player, "Welcome to ~y~Race ~w~'n ~b~Chase~w~. Please wait...");
            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    BanData ban = await context.Bans.AsQueryable().FirstOrDefaultAsync(ban => ban.HWID.Equals(pSerial) || ban.IP.Equals(pAddress) ||
                        ban.RockstarID == pSCID);

                    if (ban != null)
                    {
                        Account evadeAcc = await context.Accounts.AsQueryable().FirstOrDefaultAsync(a => a.SQL_ID == ban.AccountSQLID);
                        string evadeAccountName = evadeAcc.Username;
                        NAPI.Task.Run(() =>
                        {
                            EPlayer.SetDiscordStatus(player, "Gazing at the 'BANNED' screen.");

                            player.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}[BANNED] You are not allowed to play on this server, Ban ID: {ban.SQL_ID}");

                            player.TriggerEvent("pushMsgToChat", "!{#A9C4E4}Server closed the connection.");

                            EPlayer.Notify(player, $"You are banned from the server.<p>Please appeal on our forums (www.racenchase.com) or consult on discord server at discord.gg/y3g2WuR</p><p>Your ban ID is {ban.SQL_ID}.</p>", "red", "fas fa-ban", true, 30000);


                            EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}ANTI SUS SYSTEM (ASS) has kicked {player.Name} for ban evading as {evadeAccountName}.");
                            NAPI.Util.ConsoleOutput($"[BAN SYSTEM] Kicking {player.Name} as they're banned. (BANID: {ban.SQL_ID} ACCOUNTID: {ban.AccountSQLID})");

                            player.SetData("Kicked", true);
                            NAPI.Task.Run(() => { try { player.Kick(); } catch { } }, 1000);
                        });
                        return;
                    }
                }
            });
            
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

        [RemoteEvent("RequestPlayerPing")]
        public void OnRequestPlayerPing(Player player)
        {
            if (player == null) return;
            player.TriggerEvent("setPing", player.Ping);
        }

        [RemoteEvent("WoundedTimerTick")]
        public void OnWoundedTimerTick(Player player, int time)
        {
            if(time == 0)
            {
                player.Health = 0;
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}You succumbed to your injuries and have bled out.");
            }
        }

        [RemoteEvent("SetPlayerWounded")]
        public static void OnPlayerWounded(Player player, int state, int killerID)
        {
            try
            {
                if(player == null)
                {
                    NAPI.Util.ConsoleOutput("OnPlayerWounded - player object was null!? (" + state + ", kID: " + killerID + ")");
                    return;
                }
                if (player.HasSharedData("Wounded") && player.GetSharedData<int>("Wounded") == state && state != 0) return; // REPEL: Laggers... So it doesn't call twice...

                // if the player is in copchase, in the ragdoll stage, and it's trying to reset him, return, since it shouldn't be possible
                // in any logical way
                if (player.GetSharedData<int>("Team") >= 0 && player.GetSharedData<int>("Wounded") == 1 && state == 0) return;
                // if it tries to set them wounded but they arent in a team
                if ((player.GetSharedData<int>("Team") != 0 && player.GetSharedData<int>("Team") != 1) && state != 0) return;
                if(player.Dead) return;

                // if it tries to set them back to ragdoll state when they're already in crawl/wounded state
                if (player.GetSharedData<int>("Wounded") == 2 && state == 1) return;

                if (!player.HasSharedData("Wounded"))
                {
                    player.SetSharedData("Wounded", state);
                }

                player.SetSharedData("Wounded", state);
                NAPI.Util.ConsoleOutput("[ONPLAYERWOUNDED " + player.Name + " STATE (" + state + ") KILLER ID  (" + killerID + ")");

                Player killer = null;
                if (killerID != -1)
                {
                    killer = Settings.LoggedInPlayers.Find(p => p.Id == killerID);
                    if (killer == null)
                    {
                        player.TriggerEvent("pushMsgToChat", "!{#FF6347}A fatal error has occurred - please contact the devs and tell them this code: " + player.Name + "_D" + player.Id);
                        return;
                    }
                }

                switch (state)
                {
                    case 0:
                        {
                            player.TriggerEvent("ResetWounded");
                            player.SetSharedData("ReviveTarget", "n/a");
                            player.SetSharedData("BeingRevived", false);
                            break;
                        }
                    case 1:
                        {
                            if (player.HasSharedData("ReviveTarget") && player.GetSharedData<string>("ReviveTarget") != "n/a")
                            {
                                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] ~w~Your revive attempt failed due to your death.");
                                OnStopReviving(player);
                            }
                            player.SetSharedData("ReviveTarget", "n/a");
                            player.SetSharedData("BeingRevived", false);
                            player.TriggerEvent("pushMsgToChat", "!{#C3A3DB}* You suddenly start losing strength and collapse - you lost too much blood.");
                            player.Health = 100;

                            if (player.IsInVehicle && player.Vehicle != null)
                            {
                                if (player.Vehicle.Class == 15 || player.Vehicle.Class == 16)
                                {
                                    player.Vehicle.EngineStatus = false;
                                }

                                player.Health = 25;
                                player.TriggerEvent("pushMsgToChat", "!{#FF6347}You have been severely injured, but you are not dead yet!");
                                player.TriggerEvent("pushMsgToChat", "!{#FF6347}You are currently in a vehicle & do not have enough strength to exit it.");
                                player.TriggerEvent("pushMsgToChat", "!{#FF6347}A teammate will have to enter the vehicle in order to treat your wounds. [unlock your vehicle]");
                                player.TriggerEvent("pushMsgToChat", "!{#FF6347}You will die in 60 seconds if you are not revived by a teammate. Use (/lobby) to respawn instantly.");
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

                            if (!player.GetSharedData<string>("ReviveTarget").Equals("n/a"))
                            {
                                player.TriggerEvent("StopReviving", 3);
                            }

                            #region -> Giving XP to killer, handling stats, kill feed, clan kill
                            if (killer != null && killer != player)
                            {
                                LobbyData lobby = Settings.Lobbies.FirstOrDefault(lobby => lobby.lobbyId == player.GetSharedData<int>("InLobby"));
                                if (lobby != null)
                                {

                                    string reasonStr = "";
                                    jWeaponData wepDat = new jWeaponData();

                                    if (Settings.WeaponData.TryGetValue((long)killer.CurrentWeapon, out wepDat))
                                    {
                                        reasonStr += wepDat.Name;
                                    }
                                    else if (Main.WeaponReasons.TryGetValue((long)killer.CurrentWeapon, out reasonStr))
                                    {
                                        // oh its already set ^^
                                    }
                                    else
                                    {
                                        reasonStr = "";
                                    }
                                    if (wepDat != null && wepDat != null)
                                    {
                                        EPlayer.TryAdvanceChallenge(killer, Challenge.ChallengeType.CopchaseKill, wepDat.Group);
                                        EPlayer.TryAdvanceChallenge(killer, Challenge.ChallengeType.AnyKill, wepDat.Group);
                                    }

                                    string rez = reasonStr;
                                    if (rez != "")
                                    {
                                        reasonStr = "(" + rez + ")";
                                    }

                                    EPlayer.UnlockAchievement(killer, "first_kill");

                                    if (killer.Health <= 30)
                                        EPlayer.UnlockAchievement(killer, "low_hp_kill");

                                    if (lobby.FirstKill.Key == null && lobby.FirstKill.Value == null)
                                    {
                                        lobby.FirstKill = new KeyValuePair<Player, Player>(killer, player);

                                        if (lobby.gameMode == GameModes.BANKROBBERY && lobby.BankRobbery.HeistLeader == player)
                                            EPlayer.UnlockAchievement(killer, "first_kill_leader");
                                    }

                                    EPlayer.GetAccount(killer).Kills++;
                                    player.SetSharedData("LastKiller", killer.Name);

                                    EPlayer.GetAccount(player).Deaths++;

                                    if (EPlayer.GetAccount(player).ClanID != -1) 
                                    {
                                        Task.Run(async () =>
                                        {
                                            using (MainDatabase context = ContextFactory.CreateContext())
                                            {
                                                Clan clan = await context.Clans.AsQueryable().FirstOrDefaultAsync(c => c.SQL_ID == EPlayer.GetAccount(player).ClanID);
                                                if (clan != null)
                                                {
                                                    clan.TotalKills++;
                                                    clan.WeekKills++;
                                                }
                                            }
                                        });
                                 
                                    }

                                    killer.SetSharedData("FugitiveChance", killer.GetSharedData<float>("FugitiveChance") + 0.3 + (double)((double)EPlayer.GetAccount(killer).DonatorLevel / 10.0));
                                    EPlayer.GrantXP(killer, 2);

                                    if (lobby.Fugitives.Contains(player) && killer.GetSharedData<int>("Team") == 0)
                                    {
                                        PlayerRequestShotTag(killer);
                                    }
                                    foreach (Player ay in Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == lobby.lobbyId))
                                    {
                                        ay.TriggerEvent("pushToKillFeed", killer.Name, EPlayer.HexConverter(EPlayer.GetColor(killer)), $"Downed {reasonStr}", player.Name, EPlayer.HexConverter(EPlayer.GetColor(player)));
                                    }

                                    int killStreak = killer.GetData<int>("Killstreak") + 1;
                                    killer.SetData("Killstreak", killStreak);
                                    if (killStreak >= 5) EPlayer.UnlockAchievement(killer, "serial_killer");

                                    if (killStreak % 4 == 0 && killStreak >= 4 && (lobby.Cops.Contains(killer) || lobby.Fugitives.Contains(killer)))
                                    {
                                        EPlayer.GrantXP(killer, 5);
                                        EPlayer.SendMessageInLobby(lobby.lobbyId, $"!{{#FF0000}}[KILLSTREAK] !{{#FF6347}}{killer.Name} !{{#FFFFFF}}is on a {killStreak} kills rampage! Someone stop them!");

                                        killer.TriggerEvent("pushMsgToChat", "!{#00FF33}[!] !{#FFFFFF}You have earnt 5 XP and 5 points for maintaining a killstreak!");
                                        EPlayer.GivePoints(killer, 5);
                                        killer.SetSharedData("FugitiveChance", killer.GetSharedData<float>("FugitiveChance") + (0.1 * (double)((double)EPlayer.GetAccount(killer).DonatorLevel + 1.0)));
                                    }

                                }
                            }
                            #endregion
                            break;
                        }
                    case 2:
                        {

                            LobbyData lobby = Settings.Lobbies.FirstOrDefault(lobby => lobby.lobbyId == player.GetSharedData<int>("InLobby"));

                            player.SetSharedData("ReviveTarget", "n/a");
                            player.SetSharedData("BeingRevived", false);
                            player.Health = 25;
                            if (player.Vehicle == null && !player.IsInVehicle)
                            {
                                player.TriggerEvent("pushMsgToChat", "!{#FF6347}You have been severely injured, but you are not dead yet!");
                                player.TriggerEvent("pushMsgToChat", "!{#FF6347}You have enough strength to crawl - use it to crawl to safety.");
                                player.TriggerEvent("pushMsgToChat", "!{#FF6347}You will die in 60 seconds if you are not revived by a teammate. Use (/lobby) to respawn instantly.");
                            }
                            
                            if (player.HasSharedData("Team"))
                            {
                                if (lobby != null)
                                {
                                    if (player.GetSharedData<int>("Team") == 1)
                                    {
                                        /*
                                        if (lobby.Cops.Count <= 1)
                                        {
                                            NAPI.Task.Run(() =>
                                            {
                                                Main.StopGame(lobby.lobbyId, 2);
                                                Events.RequestClearAnimation(player);
                                                player.StopAnimation();
                                            }, 1000);
                                        }
                                        
                                        else
                                        {
                                            Player alive = lobby.Cops.FirstOrDefault(a => a.HasSharedData("Wounded") && a.GetSharedData<int>("Wounded") == 0);
                                            if (alive == null)
                                            {
                                                NAPI.Task.Run(() =>
                                                {
                                                    Main.StopGame(lobby.lobbyId, 2);
                                                    Events.RequestClearAnimation(player);
                                                    player.StopAnimation();
                                                }, 1000);
                                            }
                                        }
                                        */

                                        string chatStr = $"!{{#FFEC8B}}** [RADIO | FREQ: 911] DISPATCH: Officer {player.Name} is down and needs medical assistance!";
                                        foreach (Player cop in lobby.Cops.Where(c => c.Position.DistanceTo(player.Position) < 75))
                                        {
                                            cop.TriggerEvent("pushMsgToChat", chatStr);
                                            if (cop != player)
                                            {
                                                cop.TriggerEvent("RemoveTooltip", "H");
                                                cop.TriggerEvent("AddTooltip", "H", "Revive", 1);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        /*
                                        if (lobby.Fugitives.Count <= 1)
                                        {
                                            NAPI.Task.Run(() =>
                                            {
                                                Main.StopGame(lobby.lobbyId, 3);
                                                Events.RequestClearAnimation(player);
                                                player.StopAnimation();
                                            }, 1000);
                                        }
                                        else
                                        {
                                            Player alive = lobby.Fugitives.FirstOrDefault(a => a.HasSharedData("Wounded") && a.GetSharedData<int>("Wounded") == 0);
                                            if (alive == null)
                                            {
                                                NAPI.Task.Run(() =>
                                                {
                                                    Main.StopGame(lobby.lobbyId, 3);
                                                    Events.RequestClearAnimation(player);
                                                    player.StopAnimation();
                                                }, 1000);
                                            }
                                        }
                                        */

                                        int msgrand = new Random().Next(0, 5);
                                        string msg = "";
                                        switch (msgrand)
                                        {
                                            case 0:
                                                msg = "Fuck, I've been shot badly, need help, quick!";
                                                break;
                                            case 1:
                                                msg = "I'm bleeding out over here, will somebody fucking help me?";
                                                break;
                                            case 2:
                                                msg = "Need help, quick, I'm bleeding! Please come help me!";
                                                break;
                                            case 3:
                                                msg = "I can't get up, I've got more holes in me than Swiss cheese.";
                                                break;
                                            default:
                                                msg = "I'm going to die if someone doesn't help me! Please, give me a hand over here!";
                                                break;
                                        }

                                        string chatStr = $"!{{#FFEC8B}}** [WT | FREQ: {lobby.lobbyId + 8346}] {player.Name.Replace("_", " ")}: {msg}";
                                        foreach (Player fug in lobby.Fugitives.Where(c => c.Position.DistanceTo(player.Position) < 75))
                                        {
                                            fug.TriggerEvent("pushMsgToChat", chatStr);
                                            if (fug != player)
                                            {
                                                fug.TriggerEvent("RemoveTooltip", "H");
                                                fug.TriggerEvent("AddTooltip", "H", "Revive", 0);
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        }
                }
            }
            catch(Exception ex)
            {
                NAPI.Util.ConsoleOutput("Exception in OnPlayerWounded: " + ex.ToString());
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
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Your action was stopped: you are no longer in copchase.");
                    break;
                case 1: // Death
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Your action has failed: you are severely wounded or dead.");
                    break;
                case 2: // Max HP reached (painkillers)
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Your painkillers effect stopped earlier: your HP is full.");
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

                    player.TriggerEvent("pushMsgToChat", $"~g~You used a medkit and gained +60HP. Your HP is now {(pHP + 60 > 100 ? 100 : pHP+60)}.");
                    break;
                case "painkiller":
                    EPlayer.GetAccount(player).Painkillers--;
                    player.TriggerEvent("pushMsgToChat", $"~g~Painkillers effect expired. You gained +40HP from them and your HP is now {player.Health}.");
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

        [RemoteEvent("SetVoiceMode")]
        public static void OnSetVoiceMode(Player player, int voiceMode, bool send_msg)
        {
            switch (voiceMode)
            {
                case 0:
                    if(send_msg)
                        player.TriggerEvent("pushMsgToChat", "!{#FFFF00}Voice mode set to: !{#FFFFFF}Nearby (30m) (All players)");
                    break;
                case 1:
                    if (send_msg)
                        player.TriggerEvent("pushMsgToChat", "!{#FFFF00}Voice mode set to: !{#FFFFFF}Team Only (+15m proximity)");
                    break;
                case 2:
                    if (send_msg)
                        player.TriggerEvent("pushMsgToChat", "!{#FFFF00}Voice mode set to: !{#FFFFFF}Squad Only (Everyone else gets muted)");
                    break;
                case 3:
                    if (send_msg)
                        player.TriggerEvent("pushMsgToChat", "!{#FFFF00}Voice mode set to: !{#FFFFFF}Disabled");
                    foreach (Player p in Settings.LoggedInPlayers)
                    {
                        p.DisableVoiceTo(player);
                        player.DisableVoiceTo(p);
                    }
                    break;
            }

            player.SetSharedData("VoiceMode", voiceMode);
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

                OnSetVoiceMode(player, voiceMode, true);
                player.TriggerEvent("setVoiceModeMenu", voiceMode);
            }

            if (keyId == 0x4E)
            {
                if (keyState)
                {
                    if (EPlayer.GetAccount(player).MuteTime > 0)
                    {
                        player.TriggerEvent("pushMsgToChat", "!{#FF6347}You are currently muted for " + EPlayer.GetAccount(player).MuteReason + ", seconds left: " + EPlayer.GetAccount(player).MuteTime + ".");
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
                        else player.TriggerEvent("pushMsgToChat", "!{#FF6347}NOTE: You are trying to speak in squad chat while not being in a squad!");
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
                        player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot get inside a cop plane/chopper as a fugitive due to balance!");
                    }
                }
            }
        }

        [RemoteEvent("RequestDeployStinger")]
        public static void OnRequestDeployStinger(Player player, int spikeID)
        {
            Stinger sg = Stinger.Stingers.FirstOrDefault(s => s.ID == spikeID);
            if (sg == null) 
            {
                player.TriggerEvent("MDC:StingerFail", "No backup unit is available to deploy right now!");
                return; 
            }

            LobbyData lobby = Settings.Lobbies.Find(a => a.lobbyId == player.GetSharedData<int>("InLobby"));
            if (lobby.Stingers.FirstOrDefault(s => s.ID == spikeID) != null)
            {
                player.TriggerEvent("MDC:StingerFail", "This stinger has already been deployed!");
                return;
            }

            Lobby_StingerData lobbyStinger = new Lobby_StingerData() { ID = spikeID };
            lobby.Stingers.Add(lobbyStinger);
            player.TriggerEvent("MDC:StingerSuccess", spikeID, "Success - a backup unit was dispatched to deploy the stingers.");
            player.TriggerEvent("MDC:UpdateBlip", spikeID, "spike", sg.StingerObjects[0].Position.X, sg.StingerObjects[0].Position.Y);
            
            // Stinger is added to the lobby stingers array.
            // GameUpdate() will then handle the deployment timer and create new objects if necessary.
        }

        [RemoteEvent("OnRequestCreateClan")]
        public void OnRequestCreateClan(Player player, string clanname, string tag, string desc)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            Account acc = EPlayer.GetAccount(player);
            if (player.GetSharedData<int>("ClanID") != -1 || player.GetSharedData<int>("ClanTier") != -1)
            {
                EPlayer.Error(player, "You are already in a clan! Please leave your current one first <strong>(Menu [M] > Clan > Leave)</strong>.", "sex");
                return;
            }
            if (acc.Points < 300)
            {
                EPlayer.Error(player, "You need <strong>300 Points</strong> to create a clan!", "sex");
                return;
            }
            if (clanname.Length > 32 || clanname.Length < 3)
            {
                EPlayer.Error(player, "Clan name must be between 3 and 32 characters!", "sex");
                return;
            }
            string nameCheck = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890 ";
            bool isNameGood = true;
            for (int i = 0; i < clanname.Length; i++)
            {
                if (!nameCheck.Contains(clanname[i])) { isNameGood = false; break; }
            }
            if (!isNameGood)
            {
                EPlayer.Error(player, "Clan name must be alphanumeric (A-Z, 0-9)!", "sex");
                return;
            }

            /*
             * For testing purposes I am creating a clan this way.
             * In real scenario there will be a UI with a confirmation window or something along those lines.
             * Also a warning that a weekly/monthly clan fee will be paid.
             */
            EPlayer.SendMessage(player, "~y~Creating clan...");
            Clan clan = new Clan()
            {
                Name = clanname,
                WeekKills = 0,
                TotalKills = 0,
                WeekWins = 0,
                Description = desc,
                Tag = tag,
                CreatedOn = DateTime.Now.ToString("F")
            };
            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    await context.Clans.AddAsync(clan);
                    await context.SaveChangesAsync();
                    Clan thaClan = await context.Clans.AsQueryable().FirstOrDefaultAsync(c => c.Name == clanname);

                    acc.ClanTier = 0;
                    acc.ClanID = thaClan.SQL_ID;
                    EPlayer.GivePoints(player, -300);
                    EPlayer.Save(player, false);
                    int cID = thaClan.SQL_ID;
                    NAPI.Task.Run(() =>
                    {
                        player.SetSharedData("ClanID", cID);
                        player.SetSharedData("ClanTier", 0);
                        player.SetSharedData("ClanName", clanname);
                        player.SetSharedData("ClanCreated", DateTime.Now.ToString("F"));
                        EPlayer.Notify(player, "Clan successfully created! Read chat for more information.", "green", "", false, 3000);
                        EPlayer.SendMessage(player, $"~g~[CLAN] ~w~Congratulations! You've successfully created a clan named ~y~{clanname}~w~ for 300 Points!");
                        EPlayer.SendMessage(player, $"~g~[CLAN] ~w~You can manage your clan through the clan panel (Menu [M] > Clan > Manage)");
                        EPlayer.SendMessage(player, $"~g~[CLAN] ~w~If you have any questions, ask the staff team or management.");
                        EPlayer.SendMessage(player, $"~g~[CLAN] ~r~NOTICE: ~w~If you leave your clan, it will automatically be disbanded and deleted.");
                        EPlayer.SendMessage(player, $"~g~[CLAN] ~w~Best of luck!");

                        player.TriggerEvent("UpdateMenuClanTab");
                    });
                }
            });
        }

        [ServerEvent(Event.PlayerEnterColshape)]
        public void OnPlayerEnterColshape(ColShape shape, Player player)
        {
            if (player.IsInVehicle)
            {
                if (shape.HasData("LinkedStingerObject") && shape.HasData("StingerID"))
                {
                    LobbyData lobby = Settings.Lobbies.Find(a => a.lobbyId == player.GetSharedData<int>("InLobby"));
                    string stingerKey = shape.GetData<string>("LinkedStingerObject");
                    int stingerID = shape.GetData<int>("StingerID");
                    Lobby_StingerData stinger = lobby.Stingers.FirstOrDefault(s => s.ID == stingerID);
                    if (stinger.StingerColshapes.Contains(shape))
                    {
                        player.TriggerEvent("PopVehicleTires");
                        EPlayer.SendMessage(player, $"~r~You drove over a stinger and your tires have been popped!");

                        if (stinger == null) return;
                        GTANetworkAPI.Object stingerObj = stinger.StingerObjects.FirstOrDefault(s => s.GetData<string>("StingerObjectKey") == stingerKey);
                        if (stingerObj == null) return;

                        stingerObj.Delete();
                        shape.Delete();
                        stinger.Stage = 4;
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

                if (player.GetSharedData<int>("Team") == 0)
                {
                    LobbyData l = Settings.Lobbies.FirstOrDefault(f => f.lobbyId == player.GetSharedData<int>("InLobby"));
                    if (l == null) return;
                    if (l.gameMode == GameModes.BANKROBBERY && l.BankRobbery.HeistLeader == player)
                    {
                        BankRobberyData brob = l.BankRobbery;
                        if (brob.FugitiveVehicles.Contains(vehicle))
                        {
                            Events.BankRobbery_SetState(player, 2);
                        }
                        else if (brob.PoliceVehicles.Contains(vehicle))
                        {
                            foreach (Player f in l.Fugitives)
                            {
                                f.TriggerEvent("pushMsgToChat", $"!{{#FFEC8B}}** [RADIO | FREQ: 69420] {player.Name}: Fuck it! We'll take a police vehicle, who gives a shit if they track us?");
                            }
                            Events.BankRobbery_SetState(player, 2);
                        }
                    }
                }
            }

            if (Settings.Dynamics.OnfootCollision == false)
            {
                if (player.GetSharedData<int>("Team") == 0 || player.GetSharedData<int>("Team") == 1)
                {
                    player.SetSharedData("Collisions", true);
                }
            }

            player.SetData("LastVehEnterSeat", seatID);
        }

        [ServerEvent(Event.PlayerExitVehicle)]
        public void OnPlayerExitVehicle(Player player, Vehicle vehicle)
        {
            player.SetSharedData("Ping", player.Ping);
            try // Because triggers on disconnection and stuff invalid vehicles etc
            {
                if (vehicle == null) return;

                if (player.GetData<sbyte>("LastVehEnterSeat") == 0 && player.GetSharedData<int>("InLobby") >= 0 && player.GetSharedData<int>("Team") != -1)
                {
                    if(vehicle.Locked || (vehicle.HasSharedData("Locked") && vehicle.GetSharedData<bool>("Locked") == true))
                    {
                        EPlayer.Notify(player, "Your vehicle has been left unlocked as you exited it.", "orange", "fas fa-exclamation", false, 1500, "topCenter", "replace");
                        vehicle.Locked = false;
                        vehicle.SetSharedData("Locked", false);
                    }
                }

                if (player.HasData("ExitedDragoutVehicle") && player.HasData("DragoutVehicle"))
                {
                    player.SetData<bool>("ExitedDragoutVehicle", true);
                }

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

            if (vehicle.HasSharedData("SirenState")) {
                vehicle.SetSharedData("SirenState", !vehicle.GetSharedData<bool>("SirenState"));
                vehicle.SetSharedData("SirenState", !vehicle.GetSharedData<bool>("SirenState")); // Revert it to false then true or vice-versa so clients pick a "change" and apply it
            }
        }

        [RemoteEvent("UpdateLastVehEnterExit")]
        public void OnUpdateLastEnterExit(Player player, long supposedTime)
        {
            player.SetSharedData("LastVehEnterExit", supposedTime);
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

            if (player.HasSharedData("HeistLeader"))
            {
                if (player.GetSharedData<bool>("HeistLeader") == true)
                {
                    LobbyData lobby = Settings.Lobbies.FirstOrDefault(l => l.lobbyId == player.GetSharedData<int>("InLobby"));
                    if (lobby != null)
                    {
                        if (lobby.gameMode == GameModes.BANKROBBERY)
                        {
                            if (lobby.BankRobbery.State > 0)
                            {
                                lobby.BankRobbery.LeaderLives--;
                                if (lobby.BankRobbery.LeaderLives <= 0 || (lobby.Fugitives.Count - 1) <= 0)
                                {
                                    Main.StopGame(lobby.lobbyId, 5);
                                }
                                else
                                {

                                    BankRobberyData _brob = lobby.BankRobbery;
                                    _brob.HeistLeader = null;
                                    int unfuck = 0;
                                    while (_brob.HeistLeader == null)
                                    {
                                        _brob.HeistLeader = lobby.Fugitives[new Random().Next(lobby.Fugitives.Count)];

                                        unfuck++;
                                        if (unfuck > 50)
                                        {
                                            Main.StopGame(lobby.lobbyId, 5);
                                            break;
                                        }
                                    }
                                    _brob.HeistLeader.SetSharedData("HeistLeader", true);
                                    lobby.BankRobbery.HeistLeader = _brob.HeistLeader;

                                    foreach (Player f in lobby.Fugitives)
                                    {
                                        f.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] ~w~Your heist leader has disconnected! You have one more chance - " + _brob.HeistLeader.Name + " is your new leader.");

                                        f.TriggerEvent("pushMsgToChat", $"!{{#FFEC8B}}** [RADIO | FREQ: 69420] {_brob.HeistLeader.Name}: Sadly, we lost {player.Name}, but we must keep on going! Let's go!");
                                    }
                                    _brob.HeistLeader.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] ~w~You are now the heist leader. If you die, the round is over for your team.");
                                }
                            }
                            else
                            {   // if leader exits within first minute, fuck his tight little pussy up and stop the game
                                Main.StopGame(lobby.lobbyId, 5);                               
                            }
                        }

                        
                    }
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
                        player.SetSharedData("PlayerSquadID", -1);
                    }

                    lock (Settings.LoggedInPlayers)
                    {
                        foreach (Player mem in Settings.LoggedInPlayers.Where(p => p.HasData("PlayerSquad") && p.GetData<Player>("PlayerSquad") == player))
                        {
                            mem.TriggerEvent("pushMsgToChat", "!{#FFAC38}[Partners] This squad has been disbanded as the leader left.");
                            mem.ResetData("PlayerSquad");
                            mem.SetSharedData("PlayerSquadID", -1);
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
                            EPlayer.SendMessageToAll("!{#4343FA}[PART] !{#FFFFFF}" + player.Name + " has left the server. !{#4343FA}(Leaving)");
                            break;

                        case DisconnectionType.Timeout:
                            EPlayer.SendMessageToAll("!{#4343FA}[PART] !{#FFFFFF}" + player.Name + " has left the server. !{#4343FA}(Kicked/TimedOut)");
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

            try
            {
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

                if (Settings.WeaponData.TryGetValue((long)reason, out wepDat))
                {
                    reasonStr = wepDat.Name;
                }
                else if (Main.WeaponReasons.TryGetValue(reason, out reasonStr))
                {
                    // oh its already set ^^
                }
                else
                {
                    reasonStr = null;
                }

                if (player.GetSharedData<string>("ActionType") != "none")
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
                    if (player.GetSharedData<int>("DerbyTime") >= 5)
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

                        if(wepDat != null && wepDat.Group != null)
                            EPlayer.TryAdvanceChallenge(killer, Challenge.ChallengeType.AnyKill, wepDat.Group);

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

                        if (killer.GetSharedData<int>("Team") != player.GetSharedData<int>("Team") && player.GetSharedData<int>("Team") != -1 && player.GetSharedData<int>("InLobby") >= 0)
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
                                            ey.TriggerEvent("pushMsgToChat", $"!{{FF6347}}[RULEBREAK] {killer.Name}'s XP has been reduced by {xpReduce} for killing a fugitive that hasn't shot.");
                                        });

                                        killer.SetSharedData("FugitiveChance", killer.GetSharedData<float>("FugitiveChance") - 1.0);

                                        //killer.Health = 0;
                                        killer.TriggerEvent("pushMsgToChat", $"!{{FF6347}}[RULEBREAK] Your fugitive chance has been decreased for killing an innocent fugitive.");
                                        warnings = 0;
                                    }

                                    killer.SetData<int>("InnocentShotWarning", warnings);
                                    killer.TriggerEvent("pushMsgToChat", $"!{{FF6347}}[RULEBREAK] !{{#FFFFFF}}Repeated attacking of fugitive's that haven't shot will result in a jail.");
                                }


                                player.SetData("UnfairKillSpawn", true);



                                NAPI.Player.SpawnPlayer(player, player.Position);

                                player.Armor = 0;
                                player.Health = 100;
                                player.TriggerEvent("pushMsgToChat", $"!{{FF6347}}[!] !{{#FFFFFF}}You have been put back in the game as you were killed unfairly.");

                                if (pVeh != null)
                                {
                                    player.SetIntoVehicle(pVeh, seat);
                                }

                                return;
                            }
                        }


                        if (reason != 696988 && reason != 696989 && player.GetSharedData<int>("Team") != -1 && player.GetSharedData<int>("Team") == killer.GetSharedData<int>("Team") &&
                            player.GetSharedData<int>("InLobby") >= 0)
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
                                    killer.TriggerEvent("pushMsgToChat", $"!{{FF6347}}[RULEBREAK] !{{#FFFFFF}}Please do not heliblade.");
                                }
                                else if (warnings >= 7)
                                {
                                    //killer.Health = 0;
                                    killer.TriggerEvent("pushMsgToChat", $"!{{FF6347}}[RULEBREAK] Your fugitive chance has been decreased for team killing.");
                                    killer.SetSharedData("FugitiveChance", killer.GetSharedData<float>("FugitiveChance") - 1.0);

                                    int xpReduce = EPlayer.GetAccount(killer).XP >= 8 ? -(EPlayer.GetAccount(killer).XP / 4) : -3;

                                    EPlayer.GrantXP(killer, xpReduce);
                                    RAGE.Entities.Players.All.ForEach((Player ey) =>
                                    {
                                        ey.TriggerEvent("pushMsgToChat", $"!{{FF6347}}[RULEBREAK] {killer.Name}'s XP has been reduced by {xpReduce} for team killing.");
                                    });

                                    warnings = 0;
                                }

                                killer.SetData<int>("TeamShotWarning", warnings);

                                killer.TriggerEvent("pushMsgToChat", $"!{{FF6347}}[RULEBREAK] !{{#FFFFFF}}Repeated team killing will result in a jail.");
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
                                player.TriggerEvent("pushMsgToChat", $"!{{FF6347}}[!] !{{#FFFFFF}}You have been put back in the game as you were killed unfairly.");

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
                        if (killer == null && player.GetSharedData<int>("Wounded") != 0)
                        {
                            killer = Settings.LoggedInPlayers.FirstOrDefault(a => a.Name.Equals(player.GetSharedData<string>("LastKiller")));
                            reasonStr = "Executed";
                        }

                        if (lobby.gameStatus == 1)
                        {
                            if (lobby.Cops.Contains(player) || lobby.Fugitives.Contains(player)) EPlayer.CreateAmmoBox(player);

                            if (killer != null)
                            {
                                Player lastkilla = Settings.LoggedInPlayers.FirstOrDefault(a => a.Name.Equals(player.GetSharedData<string>("LastKiller")));
                                if (lastkilla != killer)
                                {
                                    int killStreak = killer.GetData<int>("Killstreak") + 1;
                                    killer.SetData("Killstreak", killStreak);
                                    if (killStreak >= 5) EPlayer.UnlockAchievement(killer, "serial_killer");

                                    if (killStreak % 4 == 0 && killStreak >= 4 && (lobby.Cops.Contains(killer) || lobby.Fugitives.Contains(killer)))
                                    {
                                        EPlayer.GrantXP(killer, 3);
                                        EPlayer.SendMessageInLobby(lobby.lobbyId, $"!{{#FF0000}}[KILLSTREAK] !{{#FF6347}}{killer.Name} !{{#FFFFFF}}is on a {killStreak} kills rampage! Someone stop them!");

                                        killer.TriggerEvent("pushMsgToChat", "!{#00FF33}[!] !{#FFFFFF}You have earnt 3 XP and 3 points for maintaining a killstreak!");
                                        EPlayer.GivePoints(killer, 3);
                                        killer.SetSharedData("FugitiveChance", killer.GetSharedData<float>("FugitiveChance") + (0.1 * (double)((double)EPlayer.GetAccount(killer).DonatorLevel + 1.0)));
                                    }
                                }


                                /////////////////////////////////////
                                if (player.GetSharedData<int>("Wounded") == 0)
                                {
                                    EPlayer.GetAccount(killer).Kills++;
                                    EPlayer.GetAccount(player).Deaths++;
                                }
                                else
                                {
                                    killer.SetSharedData("FugitiveChance", killer.GetSharedData<float>("FugitiveChance") + 0.05 + (double)((double)EPlayer.GetAccount(killer).DonatorLevel / 20.0));
                                }
                            }

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
                                                ay.TriggerEvent("pushToKillFeed", killer.Name, EPlayer.HexConverter(EPlayer.GetColor(killer)), "Killed", player.Name, EPlayer.HexConverter(EPlayer.GetColor(player)));
                                            }
                                            else
                                            {
                                                if (reason == 696988)
                                                {
                                                    ay.TriggerEvent("pushToKillFeed", killer.Name, EPlayer.HexConverter(EPlayer.GetColor(killer)), "Arrested", player.Name, EPlayer.HexConverter(EPlayer.GetColor(player)));
                                                }
                                                else if (reason == 696989)
                                                {
                                                    ay.TriggerEvent("pushToKillFeed", killer.Name, EPlayer.HexConverter(EPlayer.GetColor(killer)), "Busted", player.Name, EPlayer.HexConverter(EPlayer.GetColor(player)));
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

                                bool select_new_leader = false;
                                if (lobby.gameMode == GameModes.BANKROBBERY)
                                {
                                    if ((lobby.Fugitives.Count - 1) <= 0) // in case all fugis die 
                                    {
                                        Main.StopGame(lobby.lobbyId, 3);
                                    }

                                    if (lobby.BankRobbery.HeistLeader == player)
                                    {
                                        lobby.BankRobbery.LeaderLives--;
                                        if (lobby.BankRobbery.LeaderLives <= 0)
                                        {
                                            Main.StopGame(lobby.lobbyId, 5);
                                        }
                                        else
                                        {
                                            if (killer != null)
                                            {
                                                killer.TriggerEvent("pushMsgToChat", "~g~[>]~w~ You killed the first heist leader! You receive +2XP as bonus.");
                                                foreach (Player c in lobby.Cops)
                                                {
                                                    c.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}[>] {killer.Name} has killed the first heist leader!");
                                                }
                                                EPlayer.GrantXP(killer, 2);
                                            }
                                            select_new_leader = true;
                                        }
                                    }
                                }

                                lobby.Fugitives.Remove(player);
                                player.SetSharedData("HeistLeader", false);

                                if (lobby.Fugitives.Count <= 0)
                                {
                                    Main.StopGame(lobby.lobbyId, 3);
                                }

                                if (select_new_leader)
                                {
                                    BankRobberyData _brob = lobby.BankRobbery;
                                    _brob.HeistLeader = null;
                                    int unfuck = 0;
                                    while (_brob.HeistLeader == null)
                                    {
                                        _brob.HeistLeader = lobby.Fugitives[new Random().Next(lobby.Fugitives.Count)];

                                        unfuck++;
                                        if (unfuck > 50)
                                        {
                                            Main.StopGame(lobby.lobbyId, 5);
                                            break;
                                        }
                                    }
                                    _brob.HeistLeader.SetSharedData("HeistLeader", true);
                                    lobby.BankRobbery.HeistLeader = _brob.HeistLeader;

                                    foreach (Player f in lobby.Fugitives)
                                    {
                                        f.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] ~w~Your heist leader has been killed! You have one more chance - " + _brob.HeistLeader.Name + " is your new leader.");

                                        f.TriggerEvent("pushMsgToChat", $"!{{#FFEC8B}}** [RADIO | FREQ: 69420] {_brob.HeistLeader.Name}: Sadly, we lost {player.Name}, but we must keep on going! Let's go!");
                                    }
                                    _brob.HeistLeader.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] ~w~You are now the heist leader. If you die, the round is over for your team.");
                                }
                            }

                            if (lobby.Cops.Contains(player))
                            {
                                wasInGame = true;

                                if (killer != null && killer.GetSharedData<int>("Team") == 0)
                                {
                                    Events.PlayerRequestShotTag(killer);
                                }

                                lock (Settings.LoggedInPlayers)
                                {
                                    foreach (Player ay in Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == lobby.lobbyId))
                                    {
                                        if (killer != null && !killer.IsNull && player != killer)
                                        {
                                            if (reasonStr == null)
                                            {
                                                ay.TriggerEvent("pushToKillFeed", killer.Name, EPlayer.HexConverter(EPlayer.GetColor(killer)), "Killed", player.Name, EPlayer.HexConverter(EPlayer.GetColor(player)));
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

                                if (lobby.Cops.Count <= 0)
                                {
                                    Main.StopGame(lobby.lobbyId, 2);
                                }
                            }

                            player.TriggerEvent("ResetWounded");
                            player.SetSharedData("Wounded", 0);
                            //OnStopReviving(player);
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

                EPlayer.Save(player, false);
                player.SetData("Killstreak", 0);
            }
            catch(Exception ex)
            {
                NAPI.Util.ConsoleOutput("[EXCEPTION IN OnPlayerDeath] " + ex.ToString());
            }
        }

        [RemoteEvent("SetAutoOOC")]
        public static void SetAutoOOC(Player player, bool toggle)
        {
            player.SetSharedData("AutoOOC", toggle);
        }

        [ServerEvent(Event.ChatMessage)]
        public void OnPlayerText(Player player, string message)
        {
            player.SetData("TUA_LT", 0);

            if (!EPlayer.LoggedIn(player))
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You must be logged in to chat!");
                return;
            }

            if (EPlayer.GetAccount(player).MuteTime > 0)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}You are currently muted for " + EPlayer.GetAccount(player).MuteReason + ", seconds left: " + EPlayer.GetAccount(player).MuteTime + ".");
                return;
            }

            if (player.GetSharedData<bool>("AutoOOC") == false)
            {
                string chatStr = $"{player.Name.Replace("_", " ")} says: {message}";
                foreach (Player i in RAGE.Entities.Players.All)
                {
                    if (i.Dimension == player.Dimension)
                    {
                        if (i.Position.DistanceTo(player.Position) <= 10.0) i.TriggerEvent("pushMsgToChat", $"!{{#D4D4D4}}{chatStr}");
                        if (i.Position.DistanceTo(player.Position) > 10.0 && i.Position.DistanceTo(player.Position) <= 15.0) i.TriggerEvent("pushMsgToChat", $"!{{#B5B5B5}}{chatStr}");
                        if (i.Position.DistanceTo(player.Position) > 15.0 && i.Position.DistanceTo(player.Position) <= 20.0) i.TriggerEvent("pushMsgToChat", $"!{{#999999}}{chatStr}");
                    }
                }
            }
            else
            {
                Commands.cmd_ooc(player, message);
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
                    Settings.LoggedInPlayers.Where(a => a.Name.Equals(lastJSTriggerer)).First().TriggerEvent("pushMsgToChat", $"Result ({player.Name}): {result}");
            }
        }

        [RemoteEvent("ClimbProtection")]
        public static void OnClimbTrigger(Player player, int status)
        {
            if (!EPlayer.LoggedIn(player)) return;
            player.SendChatMessage($"[CP - SERVER] OnClimbTrigger {player.Name}, status: {status}.");

            LobbyData lobby = Settings.Lobbies.Find(lobby => lobby.lobbyId == player.GetSharedData<int>("InLobby"));
            if (lobby == null)
            {
                return;
            }
            //if (!lobby.Fugitives.Contains(player) && !lobby.Cops.Contains(player)) return;

            /*if(Environment.TickCount64 - player.GetData<long>("LastClimbProtection") >= 45000)
            {
                player.SetData("ClimbProtectionProcs", 0);
            }*/

            int time = 0;
            switch(status)
            {
                case 0:
                    if (player.GetData<int>("ClimbProtectionProcs") < 2)
                    {
                        player.SetSharedData("ClimbProtection", true);

                        if (player.GetSharedData<int>("Team") == 0) time = 9000;
                        else time = 18500;

                        //player.SetData("ClimbProtectionEnd", Environment.TickCount64 + time);
                        //player.SetData("LastClimbProtection", Environment.TickCount64);
                        //player.SetData("ClimbProtectionProcs", player.GetData<int>("ClimbProtectionProcs")+1);
                        EPlayer.Notify(player, "Climb protection...", "green", "fas fa-hourglass", false, time, "topCenter", "replace");
                    }
                    break;

                case 1:
                    if (player.GetData<int>("ClimbProtectionProcs") < 2)
                    {
                        player.SetSharedData("ClimbProtection", true);

                        if (player.GetSharedData<int>("Team") == 0) time = 2500;
                        else time = 4000;

                        //player.SetData("ClimbProtectionEnd", Environment.TickCount64 + time);
                        //player.SetData("LastClimbProtection", Environment.TickCount64);
                        EPlayer.Notify(player, "Climb protection...", "green", "fas fa-hourglass", false, time, "topCenter", "replace");
                    }
                    break;
                default:
                    {
                        player.SetSharedData("ClimbProtection", false);
                        player.TriggerEvent("ClearClimbTaseProtection", false);
                        break;
                    }
            }
            return;
        }

        [RemoteEvent("SetPlayerPrivateWeather")]
        public static void SetPrivateWeather(Player player, int weatherid)
        {
            LobbyData lobby = Settings.Lobbies.FirstOrDefault(l => l.lobbyId == player.GetSharedData<int>("InLobby"));
            if (lobby != null)
            {
                if (weatherid != -1)
                    player.TriggerEvent("SetTimeWeather", lobby.Time[0], lobby.Time[1], lobby.Time[2], weatherid);
                else player.TriggerEvent("SetTimeWeather", lobby.Time[0], lobby.Time[1], lobby.Time[2], lobby.Weather);
            }
            player.SetSharedData("PlayerWeather", weatherid);
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
            player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Evading by swimming is not allowed, you are taking damage.");
        }

        public static void OnPlayerLogin(Player player, Account acc)
        {
            if (EPlayer.LoggedIn(player)) return;
            if (player.HasData("Kicked")) return;

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

            Task.Run(async () =>
            {
                string pClanName = "";
                string pClanCreated = "NULL";

                List <Account> linkedAccs = await EPlayer.GetLinkedAccounts(acc);
                var linkedSQLs = linkedAccs.Select(accs => accs.SQL_ID).ToList(); // BONKERS

                Clan pClan = null;
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    pClan = await context.Clans.AsQueryable().FirstOrDefaultAsync(c => c.SQL_ID == acc.ClanID);
                    if(pClan != null)
                    {
                        pClanName = pClan.Name;
                        pClanCreated = pClan.CreatedOn;                  
                    }

                    if (linkedSQLs.Intersect(context.Bans.AsQueryable().Select(bans => bans.AccountSQLID)).Any())
                    {
                        NAPI.Task.Run(() =>
                        {
                            EPlayer.SetDiscordStatus(player, "Gazing at the 'BANNED' screen.");

                            player.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}[BANNED] You are not allowed to play on this server.");

                            EPlayer.Notify(player, $"You are banned from the server.<p>Please appeal on our forums (www.racenchase.com) or consult on discord server at discord.gg/y3g2WuR</p><p>Use your account name as a reference.</p>", "red", "fas fa-ban", true, 30000);

                            player.TriggerEvent("pushMsgToChat", "!{#A9C4E4}Server closed the connection.");

                            EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}ANTI SUS SYSTEM (ASS) has kicked {acc.Username} for ban evading/outstanding bans.");
                            NAPI.Util.ConsoleOutput($"[BAN SYSTEM] Kicking on login: {player.Name} (account name) as they're banned.");

                            player.SetData("Kicked", true);
                            NAPI.Task.Run(() => { try { player.Kick(); } catch { } }, 1000);
                        });
                        return;
                    }
                }
                NAPI.Task.Run(() =>
                {
                    player.Name = acc.Username;
                    player.SetData("LoggedIn", true);
                    player.SetData("AccountHandle", acc);
                    player.SetSharedData("pLogged", true);
                    player.SetSharedData("RegisterDate", acc.RegisterDate.ToString("F"));
                    
                    player.SetSharedData("Donator", acc.DonatorLevel);
                    player.SetSharedData("ScreenshotServerAPI", $"{acc.Password}@{acc.Salt}");

                    player.SetSharedData("ClanID", acc.ClanID);
                    player.SetSharedData("ClanTier", acc.ClanTier);
                    player.SetSharedData("ClanName", pClanName);
                    player.SetSharedData("ClanCreated", pClanCreated);

                    player.SetSharedData("Admin", acc.Admin);
                    player.SetSharedData("PlayerSquadID", -1);

                    player.TriggerEvent("BankDoorLocked", false);
                    player.TriggerEvent("BankRob_SetMusic", -1);

                    player.SetSharedData("Level", acc.Level);
                    player.SetSharedData("CurrentXP", acc.XP);
                    player.SetSharedData("NextLevelXP", EPlayer.GetMaxLevelXP(acc.Level));
                    player.TriggerEvent("Menu_UpdateLevelXP", true);
                });
                await NAPI.Task.WaitForMainThread();


                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    Language lang = await context.Languages.AsQueryable().FirstOrDefaultAsync(l => l.SQL_ID == acc.Language);
                    if (lang != null && acc.Language != -1)
                    {
                        NAPI.Task.Run(() =>
                        {
                            player.SetData("Language", lang);
                        });
                    }
                    else
                        NAPI.Task.Run(() => { player.ResetData("Language"); });
                }

                NAPI.Task.Run(() =>
                {
                    player.SetData<long>("JoinTimeUnix", DateTimeOffset.Now.ToUnixTimeSeconds());

                    CharacterData ConvChar = new CharacterData();

                    try
                    {
                        if (acc.Character == null || acc.Character.Length < 10) throw (new System.Exception("Invalid characterdata-8 sending to creation."));

                        ConvChar = NAPI.Util.FromJson<CharacterData>(acc.Character);

                        EPlayer.ApplyCharacter(player, ConvChar, acc.LobbyClothing);

                        EPlayer.Spawn(player);
                    }
                    catch
                    {
                        player.TriggerEvent("DestroyPlayerCamera");
                        player.TriggerEvent("TogglePlayerChat", true);

                        CharacterData charp = new CharacterData();
                        EPlayer.GetAccount(player).Character = "{}";
                        EPlayer.ApplyCharacter(player, charp); // Give them the default CHAR.
                        EPlayer.Spawn(player);

                        EPlayer.Notify(player, "Sorry, your character was reset (possibly due to an update), please create a new one. (<strong>/char</strong>)", "yellow", "fas fa-tshirt", false, 7000);
                    }

                    if (acc.WeaponComponents == null || acc.WeaponComponents.Length < 4)
                    {
                        Dictionary<long, wComponentData> wepItems = new Dictionary<long, wComponentData>();

                        wComponentData test = new wComponentData();
                        test.Components.Add(123456789);
                        wepItems.Add(123456789, test);

                        acc.WeaponComponents = NAPI.Util.ToJson(wepItems);
                    }

                    if (EPlayer.GetAccount(player).MOTD != null && !EPlayer.GetAccount(player).MOTD.Equals("none"))
                    {
                        MOTDData motdToShow = NAPI.Util.FromJson<MOTDData>(EPlayer.GetAccount(player).MOTD);
                        Main.DisplayMOTD(player, motdToShow);
                    }

                    player.TriggerEvent("pushMsgToChat", "!{#a6a6a6}Your last login was on !{#c2c1b6}" + EPlayer.GetAccount(player).LastLogin.ToString());
                    EPlayer.GetAccount(player).LastLogin = DateTime.Now;

                    if (EPlayer.GetAccount(player).LastSessionMins >= 60.0)
                        player.TriggerEvent("pushMsgToChat", $"!{{#a6a6a6}}Last session lasted !{{#c2c1b6}}{(EPlayer.GetAccount(player).LastSessionMins / 60.0).ToString("N1")} hours.");
                    else
                        player.TriggerEvent("pushMsgToChat", $"!{{#a6a6a6}}Last session lasted !{{#c2c1b6}}{(EPlayer.GetAccount(player).LastSessionMins).ToString("N1")} minutes.");
                });


                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    if (await context.PlayerOutfits.AsQueryable().FirstOrDefaultAsync(o => o.AccountID == acc.SQL_ID) == null)
                    {
                        CharacterData[] defaultOutfits = new CharacterData[5];
                        for (int i = 0; i < 5; i++)
                        {
                            defaultOutfits[i] = new CharacterData();
                        }
                        await context.PlayerOutfits.AddAsync(new PlayerOutfit()
                        {
                            AccountID = acc.SQL_ID,
                            OutfitData = NAPI.Util.ToJson(defaultOutfits)
                        });
                        await context.SaveChangesAsync();
                    }
                }

                List<string> cLoadouts = await EPlayer.GetOwnedLoadouts(acc, 1);
                List<string> fLoadouts = await EPlayer.GetOwnedLoadouts(acc, 0);
                int totalOwned = cLoadouts.Count + fLoadouts.Count + 2;
                if (totalOwned >= Settings.Loadouts.Count)
                    EPlayer.UnlockAchievement(player, "all_loadouts");

                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    Clan playerClan = await context.Clans.AsQueryable().FirstOrDefaultAsync(c => c.SQL_ID == acc.ClanID);
                    if (playerClan != null)
                    {
                        List<Account> clanAccounts = await context.Accounts.AsQueryable().Where(a => a.ClanID == pClan.SQL_ID).ToListAsync();
                        NAPI.Task.Run(() =>
                        {
                            player.TriggerEvent("Menu_ClearClanMembers");
                            foreach (Account accnt in clanAccounts)
                            {
                                NAPI.Util.ConsoleOutput($"Adding account {accnt.Username} [tier {accnt.ClanTier}] to clan list.");
                                player.TriggerEvent("Menu_AddClanMember", accnt.SQL_ID, accnt.Username, accnt.Level, accnt.ClanTier);
                            }
                            player.TriggerEvent("Menu_UpdateClanStats", playerClan.WeekKills, playerClan.TotalKills, playerClan.WeekWins);
                        });
                    }
                }
                NAPI.Task.Run(() =>
                {
                    player.SetSharedData("Points", EPlayer.GetAccount(player).Points);

                    EPlayer.GetAccount(player).LastSessionMins = 0.0;

                    EPlayer.Notify(player, "Login successful!", "green", "fas fa-check", false, 2000);
                    DiscordData d = NAPI.Util.FromJson<DiscordData>(EPlayer.GetAccount(player).DiscordInfo);
                    if (d.DiscordID == "n/a")
                    {
                        EPlayer.Notify(player, "We highly recommend you to <strong>link your Discord account</strong> by using <strong>/verify</strong>. You will earn some rewards in return!", "blue", "fas fa-exclamation-circle", false, 15000, "topCenter");
                        player.TriggerEvent("pushMsgToChat", "!{#5eb9ff}[DISCORD] !{#9ebaff}We recommend you to link your Discord account and win some cool perks! ~y~(/verify)");
                    }

                    foreach (Player adm in Settings.LoggedInPlayers.Where(p => EPlayer.GetAccount(p).Admin > 0))
                    {
                        EPlayer.ShowAdminNotes(adm, acc, AdminNote.ANoteType.OnConnect);
                        EPlayer.ShowAdminNotes(adm, acc, AdminNote.ANoteType.Both, false);
                    }
                    // test

                    if (EPlayer.GetAccount(player).Level >= 100)
                    {
                        EPlayer.UnlockAchievement(player, "reach_level_100");
                    }
                    player.TriggerEvent("showLoginBrowser", false);
                    player.TriggerEvent("OnPlayerLoginRegister");
                    player.TriggerEvent("LoadCrosshairProfiles", acc.CrosshairProfiles, acc.ActiveCrosshairProfile);
                    Achievement.LoadAchievementsForPlayer(player);
                    Challenge.AddChallengesForPlayer(player);
                });                
            });
            
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

            if (EPlayer.GetAccount(player).Level == 1 && EPlayer.GetAccount(player).XP < 2)
            {
                if (gender == 1)
                {
                    pChar.Clothing[0] = pChar.Clothing[2];
                    pChar.Clothing[1] = pChar.Clothing[3];
                }
            }

            pChar.Clothing[0].Clothes[3].Drawable = pChar.Clothing[0].TorsoDrawableTexture.Key;
            pChar.Clothing[0].Clothes[3].Texture = pChar.Clothing[0].TorsoDrawableTexture.Value;

            pChar.Clothing[1].Clothes[3].Drawable = pChar.Clothing[1].TorsoDrawableTexture.Key;
            pChar.Clothing[1].Clothes[3].Texture = pChar.Clothing[1].TorsoDrawableTexture.Value;


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

            player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You can edit your character's clothing with /char clothing.");

            if (EPlayer.GetAccount(player).Level == 1 && EPlayer.GetAccount(player).XP < 2)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF0000}[!] !{#FFFFFF}Please use /help and /keys to get started. Press M to open menu.");
                EPlayer.Notify(player, "We highly recommend you to <strong>link your Discord account</strong> by using <strong>/verify</strong>. You will earn some rewards in return!", "blue", "fas fa-exclamation-circle", false, 15000, "topCenter");
            }
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

            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    PlayerOutfit fit = await context.PlayerOutfits.AsQueryable().FirstOrDefaultAsync(o => o.AccountID == acc.SQL_ID);
                    string jsonReceived = fit.OutfitData;

                    try
                    {
                        List<CharacterData> outfits = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CharacterData>>(jsonReceived);

                        outfits[slot] = NAPI.Util.FromJson<CharacterData>(acc.Character);
                        fit.OutfitData = NAPI.Util.ToJson(outfits);

                        await context.SaveChangesAsync();

                        NAPI.Task.Run(() =>
                        {
                            player.TriggerEvent("pushMsgToChat", "!{#00FF00}You successfully saved your current outfit to slot " + (slot + 1) + ".");
                        });
                    }
                    catch
                    {
                        NAPI.Task.Run(() =>
                        {
                            player.SendNotification("~r~Error fetching outfits! Please try to relog or contact the devs.");

                        });
                    }
                }
            });
            
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
            
                    Task.Run(async () =>
                    {
                        using (MainDatabase context = ContextFactory.CreateContext())
                        {
                            try
                            {
                                PlayerOutfit fit = await context.PlayerOutfits.AsQueryable().FirstOrDefaultAsync(o => o.AccountID == EPlayer.GetAccount(player).SQL_ID);
                                string jsonReceived = fit.OutfitData;

                                List<CharacterData> outfits = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CharacterData>>(jsonReceived);

                                NAPI.Task.Run(() =>
                                {
                                    EPlayer.ApplyCharacter(player, outfits[slot], team);

                                    if (save == true)
                                    {
                                        acc.Character = NAPI.Util.ToJson(outfits[slot]);
                                        EPlayer.Save(player);
                                        player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You selected your outfit on slot " + (slot + 1) + ".");
                                    }
                                });
                            }
                            catch
                            {
                                NAPI.Task.Run(() =>
                                {
                                    player.SendNotification("~r~Error fetching outfits! Please try to relog or contact the devs.");
                                });
                            }

                        }
                    });
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

        [RemoteEvent("BankRob_ContinueState")]
        public static void BankRobbery_SetState(Player player, int state)
        {
            try
            {
                LobbyData lobby = Settings.Lobbies.FirstOrDefault(l => l.lobbyId == player.GetSharedData<int>("InLobby"));
                if (lobby == null) return;
                NAPI.Util.ConsoleOutput("Continuing state bank rob for " + player.Name + " state ID " + state);
                lobby.BankRobbery.State = state;
                lobby.BankRobbery.Hideout = new Random().Next(BankRobberyData.Hideouts.Length);
                switch (state)
                {
                    case 0: { return; }
                    case 1: // One minute passed, leader robbed the bank
                        {
                            foreach (Player f in lobby.Fugitives)
                            {
                                f.TriggerEvent("pushMsgToChat", $"!{{#FFEC8B}}** [RADIO | FREQ: 69420] {lobby.BankRobbery.HeistLeader.Name}: I took all the cash I can, it's time to move!");
                                f.TriggerEvent("pushMsgToChat", "!{#FF6347}[#] ~w~Escort your heist leader to one of the getaway vehicles. Once they enter one, the heist will proceed. Bank doors have been unlocked.");
                                f.TriggerEvent("pushMsgToChat", "!{#FF6347}[#] ~w~Your hideout is located in ~y~" + BankRobberyData.HideoutNames[lobby.BankRobbery.Hideout] + "~w~ !{#c9c9c9}(a marker will be set when the heist proceeds)!");

                                f.TriggerEvent("BankRob_SetMusic", 1);
                                f.TriggerEvent("BankDoorLocked", false);

                                if (f == lobby.BankRobbery.HeistLeader)
                                {
                                    Events.RequestClearAnimation(f);
                                    player.TriggerEvent("ClearTaskNotFalling");
                                    EPlayer.Freeze(f, false);
                                }
                                EPlayer.GiveFugitiveWeapons(f);
                            }
                            Player leader = lobby.BankRobbery.HeistLeader;
                            Account la = EPlayer.GetAccount(leader);
                            CharacterData cd = NAPI.Util.FromJson<CharacterData>(la.Character);


                            if(cd.Clothing.Length >= 1)
                            {
                                cd.Clothing[0].Clothes[5].Drawable = 40;
                                cd.Clothing[0].Clothes[5].Texture = 0;
                                cd.Clothing[0].Hat = 62;
                                cd.Clothing[0].HatTxt = 9;
                            }
                            if(cd.Clothing.Length >= 3)
                            {
                                cd.Clothing[2].Clothes[5].Drawable = 40;
                                cd.Clothing[2].Clothes[5].Texture = 0;
                                cd.Clothing[2].Hat = 62;
                                cd.Clothing[2].HatTxt = 9;
                            }

                            EPlayer.ApplyCharacter(leader, cd, 0);
                            EPlayer.Action(leader, "grabs the duffel bag filled with cash and slings it over their shoulder.");

                            //
                            foreach (Player c in lobby.Cops)
                            {
                                c.TriggerEvent("pushMsgToChat", $"!{{#FFEC8B}}** [RADIO | FREQ: 911] DISPATCH: All units, their leader stopped taking the money, they might try to get away soon!");
                                c.TriggerEvent("pushMsgToChat", "!{#FF6347}[#] ~w~You must find the heist leader and neutralize them! The bank doors have been unlocked..");
                                c.TriggerEvent("pushMsgToChat", "!{#FF6347}[#] ~w~Alternatively, you can keep the fugitives trapped inside the bank until the round ends to win.");
                                c.TriggerEvent("BankRob_SetMusic", 1);
                                c.TriggerEvent("BankDoorLocked", false);
                                EPlayer.GiveCopWeapons(c);
                            }

                            foreach (Vehicle v in lobby.BankRobbery.FugitiveVehicles)
                            {
                                v.Locked = false;
                                v.EngineStatus = true;
                            }
                        }
                        break;
                    case 2:
                        {
                            Vector3 hp = BankRobberyData.Hideouts[lobby.BankRobbery.Hideout];
                            foreach (Player f in lobby.Fugitives)
                            {
                                f.TriggerEvent("playerCreateWaypoint", hp);
                                f.TriggerEvent("pushMsgToChat", $"!{{#FFEC8B}}** [RADIO | FREQ: 69420] {lobby.BankRobbery.HeistLeader.Name}: Alright, I'm in the car, let's move, quick!");
                                if (f.IsInVehicle)
                                {
                                    if (f.Vehicle.Occupants.Contains(lobby.BankRobbery.HeistLeader))
                                    {
                                        f.TriggerEvent("pushMsgToChat", "!{#FF6347}[#] ~w~Drive the heist leader to the ~y~hideout~w~ marked on your minimap.");
                                    }
                                    else
                                    {
                                        f.TriggerEvent("pushMsgToChat", "!{#FF6347}[#] ~w~Escort your heist leader to the hideout.");
                                    }
                                }
                                else f.TriggerEvent("pushMsgToChat", "!{#FF6347}[#] ~w~Get in a vehicle and escort the heist leader to the hideout.");

                                f.TriggerEvent("BankRob_CreateFinishMarker", hp.X, hp.Y, hp.Z);

                            }
                            foreach (Player c in lobby.Cops)
                            {
                                c.TriggerEvent("pushMsgToChat", $"!{{#FFEC8B}}** [RADIO | FREQ: 911] DISPATCH: The robbers have made their way with the money into the vehicle.");
                                c.TriggerEvent("pushMsgToChat", $"!{{#FFEC8B}}** [RADIO | FREQ: 911] DISPATCH: Stop them before they get away!");
                                c.TriggerEvent("pushMsgToChat", "!{#FF6347}[#] ~w~This is your last chance - take down the heist leader, or all the fugitives, before they get to their hideout!");
                            }
                        }
                        break;
                    case 3:
                        {
                            Main.StopGame(lobby.lobbyId, 6);
                        }
                        break;
                }
            }
            catch(Exception ex)
            {
                NAPI.Util.ConsoleOutput("ERROR IN BANKROB SETSTATE SHOWMEDAWAE.EXE:" + ex.ToString());
            }
        }

        [RemoteEvent("BankRob_SendMessage")]
        public void BankRobbery_SendMessage(Player player, int teamID, int msgID)
        {
            LobbyData lobby = Settings.Lobbies.FirstOrDefault(l => l.lobbyId == player.GetSharedData<int>("InLobby"));
            if (lobby == null) return;
            if(teamID == 0)
            {
                switch (msgID)
                {
                    case 0:
                        foreach(Player f in lobby.Fugitives)
                        {
                            f.TriggerEvent("pushMsgToChat", "");
                            f.TriggerEvent("pushMsgToChat", "!{#FFEC8B}** [RADIO | FREQ: 69420] SCOUT: Now that I think about it, those Burritos you left as getaway might be too slow.");
                            f.TriggerEvent("pushMsgToChat", "!{#FFEC8B}** [RADIO | FREQ: 69420] SCOUT: I will take a look around and see if there is anything better you can \"borrow\".");
                            f.TriggerEvent("BankRob_SetFugiCarBlip", msgID, BankRobberyData.FugitiveVehicleData[0].Position.X, BankRobberyData.FugitiveVehicleData[0].Position.Y, BankRobberyData.FugitiveVehicleData[0].Position.Z);
                        }
                        break;
                    case 1:
                        foreach (Player f in lobby.Fugitives)
                        {
                            f.TriggerEvent("pushMsgToChat", "");
                            f.TriggerEvent("pushMsgToChat", "!{#FFEC8B}** [RADIO | FREQ: 69420] SCOUT: Some famous person pulled up in front of a building down the road with their bodyguards.");
                            f.TriggerEvent("pushMsgToChat", "!{#FFEC8B}** [RADIO | FREQ: 69420] SCOUT: There's a limousine and a bunch of fancy cars... not super fast, but at least you could go out in style!");
                            f.TriggerEvent("BankRob_SetFugiCarBlip", msgID, BankRobberyData.FugitiveVehicleData[4].Position.X, BankRobberyData.FugitiveVehicleData[4].Position.Y, BankRobberyData.FugitiveVehicleData[4].Position.Z);
                        }
                        break;
                    case 2:
                        foreach (Player f in lobby.Fugitives)
                        {
                            f.TriggerEvent("pushMsgToChat", "");
                            f.TriggerEvent("pushMsgToChat", "!{#FFEC8B}** [RADIO | FREQ: 69420] SCOUT: There's a small parking lot here with a bunch of different rides, they look good enough to me.");
                            f.TriggerEvent("pushMsgToChat", "!{#FFEC8B}** [RADIO | FREQ: 69420] SCOUT: It's located in an alley across the street, anyways, I hear sirens so I'm outta here, see you at the hideout!"); 
                            f.TriggerEvent("BankRob_SetFugiCarBlip", msgID, BankRobberyData.FugitiveVehicleData[11].Position.X, BankRobberyData.FugitiveVehicleData[11].Position.Y, BankRobberyData.FugitiveVehicleData[11].Position.Z);
                        }
                        break;
                }
            }
            else
            {
                switch (msgID)
                {
                    case 0:
                        {
                            foreach(Player c in lobby.Cops)
                            {
                                c.TriggerEvent("pushMsgToChat", $"!{{#FFEC8B}}** [RADIO | FREQ: 911] DISPATCH: All units, CCTV footage suggests that there are {lobby.Fugitives.Count} robbers.");
                                c.TriggerEvent("pushMsgToChat", $"!{{#FFEC8B}}** [RADIO | FREQ: 911] DISPATCH: They are all armed and dangerous, proceed with caution.");
                            }
                        }
                        break;
                }
            }
        }

        [RemoteEvent("UpdateTabbedState")]
        public void OnRequestUpdateTabState(Player player, bool toggle)
        {
            if (!EPlayer.LoggedIn(player)) return;

            //player.SetSharedData("IsTabbed", toggle);
        }

        [RemoteEvent("onFocusChat")]
        public void OnChatFocus(Player player, bool toggle)
        {
            if (!EPlayer.LoggedIn(player)) return;

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

            player.SetSharedData("Donator", acc.DonatorLevel);

            player.Name = acc.Username;
            player.SetSharedData("Admin", 0);
            player.SetSharedData("PlayerSquadID", -1);

            player.SetSharedData("ClanID", acc.ClanID);
            player.SetSharedData("ClanTier", acc.ClanTier);

            player.SetSharedData("ScreenshotServerAPI", $"{acc.Password}@{acc.Salt}");

            player.SetSharedData("RegisterDate", acc.RegisterDate.ToString("F"));

            player.TriggerEvent("DestroyPlayerCamera");
            player.TriggerEvent("TogglePlayerChat", true);

            player.TriggerEvent("BankDoorLocked", false);
            player.TriggerEvent("BankRob_SetMusic", -1);

            player.TriggerEvent("OnPlayerLoginRegister");
            player.TriggerEvent("LoadCrosshairProfiles", acc.CrosshairProfiles, acc.ActiveCrosshairProfile);

            player.ResetData("Language");

            player.SetData<long>("JoinTimeUnix", DateTimeOffset.Now.ToUnixTimeSeconds());


            EPlayer.ShowCharCreator(player);

            // Adding the 5 default characters to the outfits section so he has his own entry in the outfits table bla bla boring
            CharacterData[] defaultOutfits = new CharacterData[5];
            for (int i = 0; i < 5; i++)
            {
                defaultOutfits[i] = new CharacterData();
            }
            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    await context.PlayerOutfits.AddAsync(new PlayerOutfit()
                    {
                        AccountID = acc.SQL_ID,
                        OutfitData = NAPI.Util.ToJson(defaultOutfits)
                    });
                    //ContextFactory.Instance.SaveChanges();
                }
            });

            player.SetSharedData("Level", acc.Level);
            player.SetSharedData("CurrentXP", acc.XP);
            player.SetSharedData("NextLevelXP", EPlayer.GetMaxLevelXP(acc.Level));
            player.TriggerEvent("Menu_UpdateLevelXP");

            //EPlayer.Spawn(player);
            EPlayer.Notify(player, "Registration successful!", "green", "fas fa-check", false, 2000);

            player.TriggerEvent("showLoginBrowser", false);
            Achievement.LoadAchievementsForPlayer(player);
            Challenge.AddChallengesForPlayer(player);
        }

        [RemoteEvent("OnAutoLoginAttempt")]
        public static void OnAutoLoginAttempt(Player player, string username)
        {
            try
            {
                if (EPlayer.LoggedIn(player)) return;
                if (player.HasData("Kicked")) return;

                Task.Run(async () =>
                {
                    using (MainDatabase context = ContextFactory.CreateContext())
                    {
                        Account acc = await context.Accounts.AsQueryable().FirstOrDefaultAsync(a => a.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
                        if (acc != null)
                        {
                            NAPI.Task.Run(() =>
                            {
                                OnPlayerLogin(player, acc);
                            });
                            return;
                        }
                        else NAPI.Task.Run(() => { EPlayer.Error(player, "A fatal error has occurred, please log in manually.", "fas fa-lock"); });
                    }
                });

            }
            catch(Exception ex)
            {
                NAPI.Util.ConsoleOutput("[LOGIN ERROR] Error during auto-login: " + ex.ToString());
            }
        }

        [RemoteEvent("requestQuickLoginForgotUsernameData")]
        public static void OnRequestQuickLoginForgotUsername(Player player)
        {
            EPlayer.SendAutoLoginData(player);
            EPlayer.SendForgotUsernameData(player);
        }

        [RemoteEvent("OnPlayerLoginAttempt")]
        public static void OnPlayerLoginAttempt(Player player, string username, string password)
        {
            try
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
                    Task.Run(async () =>
                    {
                        using (MainDatabase context = ContextFactory.CreateContext())
                        {
                            Account acc = await context.Accounts.AsQueryable().FirstOrDefaultAsync(a => a.Email != null && a.Email.Length > 5 && a.Email.Equals(username, StringComparison.OrdinalIgnoreCase));
                            if (acc != null)
                            {
                                NAPI.Task.Run(() =>
                                {
                                    if (AccountManager.Login(acc, password))
                                    {
                                        OnPlayerLogin(player, acc);
                                        return;
                                    }
                                    else
                                    {
                                        player.TriggerEvent("login_ShowInvalidInfo", true);
                                    }
                                });
                            }
                        }
                    });
                
                    /*
                    else
                    {
                        Account archiveCheck = BackupContextFactory.Instance.Accounts.ToList().Find(a => a.Email != null && a.Email.Length > 5 && a.Email.Equals(username, StringComparison.OrdinalIgnoreCase));
                        if (archiveCheck != null)
                        {
                            AccountManager.RecoverAccount(archiveCheck);

                            acc = ContextFactory.Instance.Accounts.ToList().Find(a => a.Email != null && a.Email.Length > 5 && a.Email.Equals(username, StringComparison.OrdinalIgnoreCase));
                            if (acc != null)
                            {
                                if (AccountManager.Login(acc, password))
                                {
                                    OnPlayerLogin(player, acc);
                                    return;
                                }
                            }
                        }
                    }
                    */
                }
                else
                {
                    Task.Run(async () =>
                    {
                        using (MainDatabase context = ContextFactory.CreateContext())
                        {
                            Account acc = await context.Accounts.AsQueryable().FirstOrDefaultAsync(a => a.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
                            if (acc != null)
                            {
                                NAPI.Task.Run(() =>
                                {
                                    if (AccountManager.Login(acc, password))
                                    {
                                        OnPlayerLogin(player, acc);
                                        return;
                                    }
                                    else
                                    {
                                        player.TriggerEvent("login_ShowInvalidInfo", true);
                                    }
                                });

                            }
                        }
                    });

                    /*
                    else
                    {
                        Account archiveCheck = BackupContextFactory.Instance.Accounts.ToList().Find(a => a.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
                        if (archiveCheck != null)
                        {
                            AccountManager.RecoverAccount(archiveCheck);

                            acc = ContextFactory.Instance.Accounts.ToList().Find(a => a.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
                            if (acc != null)
                            {
                                if (AccountManager.Login(acc, password))
                                {
                                    OnPlayerLogin(player, acc);
                                    return;
                                }
                            }
                        }
                    }
                    */
                }
                //EPlayer.Notify(player, "Invalid credentials or unable to log-in!", "red", "fas fa-lock", false, 4000, "bottomCenter");
                
            }
            catch(Exception ex)
            {
                NAPI.Util.ConsoleOutput("CRINGE LOGIN FAIL: " + ex.ToString());
            }
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

        [RemoteEvent("SetAutoJoin")]
        public void OnSetAutoJoin(Player player, bool toggle, bool notify)
        {
            player.SetSharedData("AutoJoin", toggle);

            if (notify)
            {
                if (toggle)
                {
                    EPlayer.Notify(player, $"AutoJoin has been turned <strong>ON</strong>.", "green", "fas fa-check", false, 1500, "bottomCenter", "replace");
                }
                else
                {
                    EPlayer.Notify(player, $"AutoJoin has been turned <strong>OFF</strong>.", "red", "fas fa-check", false, 1500, "bottomCenter", "replace");
                }
            }
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
                        player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot switch minigames that fast.");
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
                    player.SetSharedData("AutoJoin", false);
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

        [RemoteEvent("Menu_RequestClansList")]
        public void OnMenuRequestClansList(Player player)
        {
            if (player == null) return;
            Task.Run(async () =>
            {
                using(MainDatabase context = ContextFactory.CreateContext())
                {
                    await context.Clans.AsQueryable().ForEachAsync(clan =>
                    {
                        string clanName = clan.Name;
                        string tag = clan.Tag; // to add
                        string desc = clan.Description; // to add
                        string banner = "default"; // to add
                        bool isOpen = false; // to add
                        int maxPlayers = 20; // to add
                        int players = context.Accounts.AsQueryable().Count(a => a.ClanID == clan.SQL_ID);
                        int totalKills = clan.TotalKills;
                        int totalDeaths = clan.WeekKills; // temp
                        int COTW_Wins = clan.WeekWins;

                        NAPI.Task.Run(() =>
                        {
                            player.TriggerEvent("Menu_AddClan", clanName, tag, desc, banner, isOpen, players,
                                                                maxPlayers, totalKills, totalDeaths, COTW_Wins);

                        });
                        NAPI.Task.WaitForMainThread();
                    });
                }
            });
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

            if (car.Model.Equals("superd")) // discord-exclusive car
            {
                DiscordData d = NAPI.Util.FromJson<DiscordData>(acc.DiscordInfo);
                if (d.DiscordID == "n/a")
                {
                    EPlayer.Notify(player, $"You must <strong>link your Discord account</strong> to unlock this. <strong>(/verify)</strong>", "red", "fas fa-lock", false, 4000, "bottomCenter", "replace");
                    return;
                }
            }
            Task.Run(async () =>
            {
                if (await EPlayer.HasOwnedVehicle(acc, model, team))
                {
                    if (team == 0)
                    {
                        NAPI.Task.Run(() =>
                        {
                            EPlayer.Notify(player, $"Fugitive vehicle set to <strong>{car.Name}</strong>.", "blue", "fas fa-car", false, 3000, "bottomCenter", "replace");
                        });
                        acc.CarFugitive = car.Model;
                    }
                    else
                    {
                        NAPI.Task.Run(() =>
                        {
                            EPlayer.Notify(player, $"Police vehicle set to <strong>{car.Name}</strong>.", "blue", "fas fa-car", false, 3000, "bottomCenter", "replace");
                        });
                        acc.CarCop = car.Model;
                    }
                    List<object[]> vehData = await Menu_VehicleData(acc);
                    NAPI.Task.Run(() =>
                    {
                        player.TriggerEvent("UpdateMenuVehicles", NAPI.Util.ToJson(vehData), acc.CarCop, acc.CarFugitive);
                    });
                }
                else
                {
                    NAPI.Task.Run(() =>
                    {
                        if (car.LevelRequired != 0 && car.LevelRequired > acc.Level)
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
                        EPlayer.UnlockVehicle(acc, player, car.Model, car.Team);
                        EPlayer.Notify(player, $"You have unlocked <strong>{car.Name}</strong>. You may select it now.", "green", "fas fa-unlock", false, 5000, "bottomCenter", "replace");

                        Task.Run(async () =>
                        {
                            List<object[]> vehData = await Menu_VehicleData(acc);
                            NAPI.Task.Run(() =>
                            {
                                player.TriggerEvent("UpdateMenuVehicles", NAPI.Util.ToJson(vehData), acc.CarCop, acc.CarFugitive);
                            });
                        });
                    });

                }
            });
            
            return;
        }

        public async Task<List<object[]>> Menu_VehicleData(Account _a)
        {
            List<object[]> vehItems = new List<object[]>();
            foreach (var car in Settings.Cars)
            {
                string price = "[";
                if (car.LevelRequired > 0) price += "\"Level " + car.LevelRequired.ToString() + "\",";
                if (car.PointsRequired > 0) price += "\"" + car.PointsRequired.ToString() + " Points\",";
                if (car.DonatorPointsRequired > 0) price += "\"" + car.DonatorPointsRequired.ToString() + " Donator Points\",";
                if (car.DonatorLevelRequired > 0) price += "\"" + EPlayer.GetDonatorLevelName(car.DonatorLevelRequired) + "\"";
                if (price.Length < 2) price = "[\"Free\"";

                price += "]";


                bool isOwnedVeh = await EPlayer.HasOwnedVehicle(_a, car.Model, car.Team);

                vehItems.Add(new object[]
                {
                            car.Team,
                            car.Model,
                            car.Name,
                            car.Image,
                            !isOwnedVeh,
                            car.Info,
                            price
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

            if(loadout.Name == "Negotiator")
            {
                DiscordData d = NAPI.Util.FromJson<DiscordData>(acc.DiscordInfo);
                if (d.DiscordID == "n/a")
                {
                    EPlayer.Notify(player, $"You must <strong>link your Discord account</strong> to unlock this. <strong>(/verify)</strong>", "red", "fas fa-lock", false, 4000, "bottomCenter", "replace");
                    return;
                }
            }

            Task.Run(async () =>
            {
                if (await EPlayer.HasOwnedLoadout(acc, name, team))
                {
                    if (team == 0)
                    {
                        NAPI.Task.Run(() =>
                        {
                            EPlayer.Notify(player, $"Fugitive loadout set to <strong>{loadout.Name}</strong>.", "blue", "fas fa-check", false, 3000, "bottomCenter", "replace");
                        });
                        acc.LoadoutFugitive = (loadout.Name.Equals("default", StringComparison.OrdinalIgnoreCase) ? "?" : loadout.Name);
                    }
                    else
                    {
                        NAPI.Task.Run(() =>
                        {
                            EPlayer.Notify(player, $"Police loadout set to <strong>{loadout.Name}</strong>.", "blue", "fas fa-check", false, 3000, "bottomCenter", "replace");
                        });
                        acc.LoadoutCop = (loadout.Name.Equals("default", StringComparison.OrdinalIgnoreCase) ? "?" : loadout.Name);
                    }
                    List<object[]> loadoutData = await Menu_LoadoutData(acc);
                    NAPI.Task.Run(() =>
                    {
                        player.TriggerEvent("UpdateMenuLoadouts", NAPI.Util.ToJson(loadoutData), acc.LoadoutCop, acc.LoadoutFugitive);
                    });
                }
                else
                {
                    NAPI.Task.Run(() =>
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
                        EPlayer.UnlockLoadout(acc, player, loadout.Name, loadout.Team);
                        EPlayer.Notify(player, $"You have unlocked <strong>{loadout.Name}</strong>. You may select it now.", "green", "fas fa-unlock", false, 5000, "bottomCenter", "replace");

                        Task.Run(async () =>
                        {
                            List<object[]> loadoutData = await Menu_LoadoutData(acc);
                            NAPI.Task.Run(() =>
                            {
                                player.TriggerEvent("UpdateMenuLoadouts", NAPI.Util.ToJson(loadoutData), acc.LoadoutCop, acc.LoadoutFugitive);

                            });
                        });
                    });
                }
            });
            
            return;
        }

        public async Task<List<object[]>> Menu_LoadoutData(Account acc)
        {
            List<object[]> loadoutItems = new List<object[]>();
            foreach (var loadout in Settings.Loadouts)
            {
                string weaponString = "\"weapons\": [";

                foreach (var weapon in loadout.Weapons)
                {
                    weaponString += "{name: \"" + (string)weapon[2] + "\", img: \"" + (string)weapon[1] + "\", ammo: " + (int)weapon[3] + ",},";
                }

                weaponString += "],";

                string requireString = "\"requirements\": [";

                if (loadout.LevelRequired > 0) requireString += $"\"Level {loadout.LevelRequired}\",";
                if (loadout.PointsRequired > 0) requireString += $"\"{loadout.PointsRequired} Points\",";
                if (loadout.DonatorPointsRequired > 0) requireString += $"\"{loadout.DonatorPointsRequired} Donator Points\",";
                if (loadout.DonatorLevelRequired > 0) requireString += $"\"{EPlayer.GetDonatorLevelName(loadout.DonatorLevelRequired)}\",";

                requireString += "],";

                bool hasOwnedLoadout = await EPlayer.HasOwnedLoadout(acc, loadout.Name, loadout.Team);
                loadoutItems.Add(new object[]
                {
                            loadout.Team,
                            loadout.Name,
                            loadout.Background,
                            !hasOwnedLoadout,
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
            Account acc = EPlayer.GetAccount(player);
            if(status == true) // OPEN: Update lobby data and everything else.
            {
                Task.Run(async () =>
                {
                    List<object[]> loadoutData = await Menu_LoadoutData(acc);
                    List<object[]> vehData = await Menu_VehicleData(acc);

                    NAPI.Task.Run(() =>
                    {
                        player.TriggerEvent("UpdateMenuLobbies", NAPI.Util.ToJson(Menu_LobbiesData(player)), player.GetSharedData<int>("InLobby"));
                        player.TriggerEvent("UpdateMenuMinigames", NAPI.Util.ToJson(Menu_MinigamesData(player)), (player.GetSharedData<int>("InLobby") < 0) ? player.GetSharedData<int>("InLobby") : player.GetSharedData<int>("InDM"));
                        player.TriggerEvent("UpdateMenuVehicles", NAPI.Util.ToJson(vehData), EPlayer.GetAccount(player).CarCop, EPlayer.GetAccount(player).CarFugitive);
                        player.TriggerEvent("UpdateMenuLoadouts", NAPI.Util.ToJson(loadoutData), EPlayer.GetAccount(player).LoadoutCop, EPlayer.GetAccount(player).LoadoutFugitive);
                        player.TriggerEvent("addAchievementsToMenu");
                    });
                });
             
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

        [RemoteEvent("SetTaseProtection")]
        public void OnSetTaseProtection(Player player, bool toggle)
        {
            if (player == null) return;
            if (toggle)
            {
                if (player.GetSharedData<int>("Team") != 0) return;

                player.SetSharedData("TaseProtection", true); 
                EPlayer.Notify(player, "Tase protection...", "yellow", "fas fa-hourglass", false, 5000, "topCenter", "replace");
            }
            else
            {
                EPlayer.Notify(player, "Tase protection...", "yellow", "fas fa-hourglass", false, 1, "topCenter", "replace");
                player.SetSharedData("TaseProtection", false);
            }
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
                        player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Please do not get out of your vehicle in derby!");
                    }
                }
            }

            return;
        }

        [RemoteEvent("SetShotsFired")]
        public static void SetShotsFired(Player player, bool toggle)
        {
            player.SetData<bool>("ShotsFired", toggle);
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
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Do not shoot at teammates, repeatedly doing this will result in punishment!");

                    if (warnings >= 7)
                    {
                        player.Health = 0;
                        EPlayer.GrantXP(player, -1);
                        player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You have been killed and lost 1XP due to team attacking.");
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
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Do not shoot at fugitives that haven't shot, repeatedly doing this will result in punishment!");

                    if (warnings >= 7)
                    {
                        player.Health = 0;
                        EPlayer.GrantXP(player, -1);
                        player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You have lost 1XP due to shooting at a fugitive without shot tag.");
                        warnings = 0;
                    }

                    player.SetData("InnocentShotWarning", warnings);
                }

                //target.Health = 100;
            }
        }

        [RemoteEvent("UpdateVehicleFuel")]
        public static void UpdateVehicleFuel(Player player, float fuel, ushort vehicle_id = 65535)
        {
            Vehicle vehicle = null;
            if(vehicle_id == 65535) {
                if (!player.IsInVehicle) return;
                vehicle = player.Vehicle;
            }
            else
            {
                Vehicle target = NAPI.Pools.GetAllVehicles().FirstOrDefault(v => v.Id == vehicle_id);
                if (target == null) return;
                vehicle = target;
            }

            vehicle.SetSharedData("Fuel", fuel);

            if(fuel <= 0.0)
            {
                vehicle.EngineStatus = false;
                vehicle.Health = 0;
            }
        }

        public static void RunWeeklyMonthlyChecks()
        {
            try
            {
                if (new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds() >= Settings.Dynamics.NextMonthUnix)
                {
                    Task.Run(async () =>
                    {
                        using (MainDatabase context = ContextFactory.CreateContext())
                        {
                            List<Logs_Session> seslog = await context.Logs_Session.AsQueryable().ToListAsync();
                            context.Logs_Session.RemoveRange(seslog);
                            List<Logs_Admin> adlog = await context.Logs_Admin.AsQueryable().ToListAsync();
                            context.Logs_Admin.RemoveRange(adlog);


                            await context.SaveChangesAsync();
                            NAPI.Task.Run(() =>
                            {
                                Settings.Dynamics.NextMonthUnix = new DateTimeOffset(DateTime.Now).AddMonths(1).ToUnixTimeSeconds();
                                NAPI.Util.ConsoleOutput("[1 MONTH] Server logs [Admin, Session] have been cleared!");
                                Main.SaveDynamicSettings();
                            });
                        }
                    });
                    
                }
                if (new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds() >= Settings.Dynamics.NextWeekUnix)
                {
                    // CHECKING FOR CLAN OF THE WEEK, RESETTING ALL CLANS' WEEKLY KILLS
                    Task.Run(async () =>
                    {
                        using (MainDatabase context = ContextFactory.CreateContext())
                        {
                            int mostKill = await context.Clans.AsQueryable().MaxAsync(c => c.WeekKills);
                            Clan mostKills = null;
                            if(mostKill > 0)
                            {
                                mostKills = await context.Clans.AsQueryable().FirstOrDefaultAsync(c => c.WeekKills == mostKill);
                            }
                            if (mostKills != null)
                            {
                                foreach (Account account in await context.Accounts.AsQueryable().Where(a => a.ClanID != -1).ToListAsync())
                                {
                                    if (account.ClanID == mostKills.SQL_ID)
                                    {
                                        MOTDData motd = new MOTDData()
                                        {
                                            AdminName = "SERVER",
                                            PostedOn = DateTime.Now.ToShortDateString(),
                                            Message = "~y~[CLAN OF THE WEEK]~w~ Congratulations! Your clan had the most kills this week and you won a reward! All clans' weekly kills have been reset."
                                        };
                                        account.MOTD = NAPI.Util.ToJson(motd);

                                        // reward skripts here
                                    }
                                    else
                                    {
                                        MOTDData motd = new MOTDData()
                                        {
                                            AdminName = "SERVER",
                                            PostedOn = DateTime.Now.ToShortDateString(),
                                            Message = $"~r~[CLAN OF THE WEEK]~w~ Clan {mostKills.Name} won clan of the week. All clans' weekly kills have been reset!"
                                        };
                                        account.MOTD = NAPI.Util.ToJson(motd);
                                    }
                                }
                                NAPI.Task.Run(() =>
                                {
                                    EPlayer.SendMessageAll($"~y~[CLAN OF THE WEEK] ~w~Clan ~g~{mostKills.Name} ~w~had the most kills this week! Congratulations!");
                                    EPlayer.SendMessageAll($"~y~[>] ~w~ A new week begins and all clans' weekly kills have been reset.");
                                });
                            }
                        }
                    });
                    
                    // ARCHIVING INACTIVE ACCOUNTS.....
                    /*
                    Account cunt = ContextFactory.Instance.Accounts.ToList().FirstOrDefault(a => (new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds() - new DateTimeOffset(a.LastLogin).ToUnixTimeSeconds()) > 2629746);
                    if (cunt != null)
                    {
                        EPlayer.SendMessageToAll("~p~[WEEK PASS] ~w~One week has passed. Inactive accounts will now be archived.");
                        EPlayer.SendMessageToAll("~b~[ACCOUNT ARCHIVE] ~w~This might cause some server lag for a few seconds, please wait...");
                        EPlayer.SendMessageToAll("~b~[ACCOUNT ARCHIVE] ~w~You will be frozen until the archiving process is over.");
                        NAPI.Util.ConsoleOutput("[1 WEEK] All accounts inactive for 1+ months will be archived.");
                        int archivedAccs = 0;

                        NAPI.Task.Run(() =>
                        {
                            foreach (Player pl in NAPI.Pools.GetAllPlayers().Where(p => p.GetSharedData<int>("Team") > 0))
                            {
                                EPlayer.Notify(pl, "<strong>Account archiving in process...</strong> This will take a few seconds.", "orange", "fa-regular fa-server", false, 999999999, "center", "replace");
                                EPlayer.Freeze(pl, true);
                            }
                            foreach (Account acc in ContextFactory.Instance.Accounts)
                            {
                                acc.ChallengeStreak = 0;
                                acc.CompletedDailyChallenge = false;

                                if ((new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds() - new DateTimeOffset(acc.LastLogin).ToUnixTimeSeconds()) > 2629746) // 2,629,746 = 1 month in seconds
                                {
                                    acc.MOTD = NAPI.Util.ToJson(new MOTDData()
                                    {
                                        AdminName = "SERVER",
                                        Message = "~b~[ACCOUNT ARCHIVE]~w~ You've been inactive for over a month, so your account was archived. It has now been recovered automatically.",
                                        PostedOn = DateTime.Now.ToShortDateString()
                                    });

                                    if (BackupContextFactory.Instance.Accounts.FirstOrDefault(a => a.Username == acc.Username) == null)
                                    {
                                        BackupContextFactory.Instance.Accounts.Add(acc);

                                        List<OwnedVehData> ownedVehs = ContextFactory.Instance.OwnedVehicles.ToList().Where(v => v.AccountSQLID == acc.SQL_ID).ToList();
                                        List<OwnedLoadoutData> ownedLoadouts = ContextFactory.Instance.OwnedLoadouts.ToList().Where(v => v.AccountSQLID == acc.SQL_ID).ToList();
                                        List<PlayerOutfit> savedOutfits = ContextFactory.Instance.PlayerOutfits.ToList().Where(v => v.AccountID == acc.SQL_ID).ToList();
                                        List<PlayerTunings> savedTunings = ContextFactory.Instance.PlayerTunings.ToList().Where(v => v.AccountSQLID == acc.SQL_ID).ToList();
                                        List<UnlockedAchievement> unlockedAchievements = ContextFactory.Instance.UnlockedAchievements.ToList().Where(v => v.AccountID == acc.SQL_ID).ToList();

                                        BackupContextFactory.Instance.OwnedLoadouts.AddRange(ownedLoadouts);
                                        BackupContextFactory.Instance.OwnedVehicles.AddRange(ownedVehs);
                                        BackupContextFactory.Instance.PlayerOutfits.AddRange(savedOutfits);
                                        BackupContextFactory.Instance.PlayerTunings.AddRange(savedTunings);
                                        BackupContextFactory.Instance.UnlockedAchievements.AddRange(unlockedAchievements);

                                        ContextFactory.Instance.PlayerOutfits.RemoveRange(savedOutfits);
                                        ContextFactory.Instance.OwnedLoadouts.RemoveRange(ownedLoadouts);
                                        ContextFactory.Instance.OwnedVehicles.RemoveRange(ownedVehs);
                                        ContextFactory.Instance.PlayerTunings.RemoveRange(savedTunings);
                                        ContextFactory.Instance.UnlockedAchievements.RemoveRange(unlockedAchievements);

                                        ContextFactory.Instance.Accounts.Remove(acc);
                                        archivedAccs++;

                                        List<Logs_Session> records = ContextFactory.Instance.Logs_Session.ToList().Where(a => a.AccountID == acc.SQL_ID).ToList();
                                        ContextFactory.Instance.Logs_Session.RemoveRange(records);
                                    }
                                }
                                else
                                {
                                    acc.MOTD = NAPI.Util.ToJson(new MOTDData()
                                    {
                                        AdminName = "SERVER",
                                        Message = "~y~[WEEKLY CHALLENGE] ~w~Seven days have passed and the weekly challenge streak was reset.",
                                        PostedOn = DateTime.Now.ToShortDateString()
                                    });
                                }
                            }

                            BackupContextFactory.Instance.SaveChanges();
                            ContextFactory.Instance.SaveChanges();
                            NAPI.Util.ConsoleOutput($"[ACCOUNT ARCHIVE] {archivedAccs} have been archived. Total accounts in archive: {BackupContextFactory.Instance.Accounts.Count()}");

                            foreach (Player pl in NAPI.Pools.GetAllPlayers())
                            {
                                if (EPlayer.LoggedIn(pl) == true)
                                {
                                    EPlayer.Notify(pl, "<strong>Account archiving in process...</strong> This will take a few seconds.", "orange", "fa-regular fa-server", false, 1, "center", "replace");
                                    EPlayer.Freeze(pl, false);
                                }
                            }
                            EPlayer.SendMessageToAll("~b~[ACCOUNT ARCHIVE] ~w~Account archive process ~g~finished~w~! You've been unfrozen.");
                        }, 100);
                    } 
                    */
                    Settings.Dynamics.NextWeekUnix = new DateTimeOffset(DateTime.Now).AddDays(7).ToUnixTimeSeconds();

                    foreach (Player pl in NAPI.Pools.GetAllPlayers())
                    {
                        if (EPlayer.LoggedIn(pl) == true)
                        {
                            pl.TriggerEvent("addChallengesToMenu", 0, (Settings.Dynamics.NextWeekUnix - new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()));
                            pl.TriggerEvent("pushMsgToChat", "~y~[WEEKLY CHALLENGE] ~w~Seven days have passed and the weekly challenge streak was reset. Good luck!");
                        }
                    }
                    Main.SaveDynamicSettings();
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput("Time Checking week/month issue: " + ex.ToString());
            }
        }

        [RemoteEvent("DebugLog_SendCameraStuckLog")]
        public static void DebugLog_SendCameraStuck(Player player, string camswitch_native_return)
        {
            if (player == null || player.GetSharedData<int>("Team") == -1) return;
            LobbyData lobby = Settings.Lobbies.FirstOrDefault(l => l.lobbyId == player.GetSharedData<int>("InLobby"));
            if (lobby == null) return;

            var writer = new System.IO.StreamWriter($"racenchase/debug_logs/camera_switch.txt", true);

            writer.WriteLine($"[ {DateTime.Now.ToString()} ]");
            writer.WriteLine($"PLAYER: {player.Name} (ServerID: {player.Id})");
            writer.WriteLine($"LOBBY: Lobby #{lobby.lobbyId}, GameMode: {Main.ConvertGameMode(lobby.gameMode)}");
            writer.WriteLine($"CAM_SWITCH NATIVE RETURNED: {camswitch_native_return}");
            writer.WriteLine($"[==========> END <==========]");
            writer.Flush();
            writer.Close();
            return;
        }

        [RemoteEvent("Server_ToggleAutoLogin")]
        public static void ToggleAutoLogin(Player player, bool toggle)
        {
            Account acc = EPlayer.GetAccount(player);
            acc.AutoLogin = toggle;

            if (toggle)
                EPlayer.Notify(player, "You turned Auto Login on! You will no longer need to enter your login info on this account.", "green", "fa-solid fa-user-check", true, 4000);
            else
                EPlayer.Notify(player, "You turned Auto Login off! You will now have to enter your login info when you connect.", "red", "fa-solid fa-user-xmark", true, 4000);

                /*
                Task.Run(async () =>
                {
                    using (MainDatabase context = ContextFactory.CreateContext())
                    {
                        Account acc = await context.Accounts.AsQueryable().FirstOrDefaultAsync(a => a.Username.Equals(player.Name, StringComparison.OrdinalIgnoreCase));
                        if (acc == null) return;
                        acc.AutoLogin = toggle;

                        NAPI.Task.Run(() =>
                        {
                            if (toggle)
                                EPlayer.Notify(player, "You turned Auto Login on! You will no longer need to enter your login info on this account.", "green", "fa-solid fa-user-check", true, 4000);
                            else
                                EPlayer.Notify(player, "You turned Auto Login off! You will now have to enter your login info when you connect.", "red", "fa-solid fa-user-xmark", true, 4000);
                        });
                    }
                });
                */
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

                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Server detected you as an AFK driver and swapped your seat.");
                newDriver.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}[!] !{{#FFFFFF}}{player.Name} is AFK, you are now the new driver.");
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

            ulong pSCID = player.SocialClubId;
            string pSerial = player.Serial;

            bool reachedAccountLimit = false;
            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    Account acc = await context.Accounts.AsQueryable().FirstOrDefaultAsync(a => a.Username.Equals(username, StringComparison.OrdinalIgnoreCase) == true);
                    if (acc != null)
                    {
                        //EPlayer.Error(player, "An account with this name already exists!", "fas fa-user");
                        NAPI.Task.Run(() =>
                        {
                            player.TriggerEvent("login_ShowAccountExists", true);
                        });
                        return;
                    }
                    try
                    {
                        if (await context.Accounts.AsQueryable().CountAsync(a => a.RegisterHWID.Equals(pSerial) || a.RegisterRID == pSCID) >= 2)
                        {
                            NAPI.Util.ConsoleOutput($"(RnC | REGISTER) {username} attempted to register, they have 2 or more accounts already!");
                        
                            reachedAccountLimit = true;
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        NAPI.Util.ConsoleOutput("(RnC | REGISTER) Failed the 2+ account check for " + username + "!");
                        NAPI.Util.ConsoleOutput(ex.ToString());
                    }
                    finally
                    {
                        if (!reachedAccountLimit)
                        {
                            NAPI.Task.Run(() =>
                            {
                                player.SetData("LastCryptAttempt", Environment.TickCount64);
                                string salt = AccountManager.GenSalt();
                                string newPass = AccountManager.Crypt(password, salt);

                                Dictionary<long, wComponentData> wepItems = new Dictionary<long, wComponentData>();

                                wComponentData test = new wComponentData();
                                test.Components.Add(123456789);
                                wepItems.Add(123456789, test);

                                DiscordData _discordData = new DiscordData();
                                string jsonDiscordData = NAPI.Util.ToJson(_discordData);

                                List<ChallengeData> challengeData = new List<ChallengeData>();
                                string jsonChallengeData = NAPI.Util.ToJson(challengeData);

                                var _new = new Account
                                {
                                    Email = email,
                                    Password = newPass,
                                    Salt = salt,
                                    Username = username,
                                    DiscordInfo = jsonDiscordData,
                                    ChallengeInfo = jsonChallengeData,
                                    AutoLogin = true,
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
                                    ChallengeStreak = 0,
                                    CompletedDailyChallenge = false,
                                    Namechanges = 0,
                                    ClanTier = -1,
                                    ClanID = -1,
                                    CrosshairProfiles = "[]",
                                    ActiveCrosshairProfile = "{}",
                                };
                                AccountManager.Add(_new);
                                OnPlayerRegister(player, _new);
                            });
                        }
                        else
                        {
                            NAPI.Task.Run(() =>
                            {
                                player.TriggerEvent("login_MaxAccountLimit", true);
                            });
                        }
                    }
                }
            });
        }

        [RemoteEvent("Crosshair_SetProfile")]
        public void OnSetCrosshairProfile(Player player, string profileID)
        {
            EPlayer.GetAccount(player).ActiveCrosshairProfile = profileID;
            EPlayer.Save(player, false);
        }
        [RemoteEvent("Crosshair_SaveProfiles")]
        public void OnSaveCrosshairProfiles(Player player, string profileData)
        {
            EPlayer.GetAccount(player).CrosshairProfiles = profileData;
            EPlayer.Save(player, false);
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
                        player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Your loadout for team fugitive has been set to 'default'.");
                        return;
                    }
                    LoadoutData loadout = Settings.Loadouts.Find(b => b.Team == 0 && b.Name.Equals(ldname, StringComparison.OrdinalIgnoreCase));

                    if ((loadout != null) == false)
                    {
                        player.TriggerEvent("pushMsgToChat", "!{#999999}Failed to find a loadout with the specified name. Are you sure that's a fugitive/valid loadout?");
                        return;
                    }

                    if (loadout.LevelRequired > EPlayer.GetAccount(player).Level)
                    {
                        player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You don't have the required level (" + loadout.LevelRequired.ToString() + ") " + "for this loadout.");
                        return;
                    }

                    EPlayer.GetAccount(player).LoadoutFugitive = loadout.Name;
                    player.TriggerEvent("pushMsgToChat", "!{#33C446}[#] !{#FFFFFF}You have set your fugitive loadout to " + loadout.Name + ".");
                }
                else if(team == 1)
                {
                    if (ldname.StartsWith("default", StringComparison.OrdinalIgnoreCase))
                    {
                        EPlayer.GetAccount(player).LoadoutCop = "?";
                        player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Your loadout for team cop has been set to 'default'.");
                        return;
                    }
                    LoadoutData loadout = Settings.Loadouts.Find(b => b.Team == 1 && b.Name.Equals(ldname, StringComparison.OrdinalIgnoreCase));

                    if ((loadout != null) == false)
                    {
                        player.TriggerEvent("pushMsgToChat", "!{#999999}Failed to find a loadout with the specified name. Are you sure that's a cop/valid loadout?");
                        return;
                    }

                    if (loadout.LevelRequired > EPlayer.GetAccount(player).Level)
                    {
                        player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You don't have the required level (" + loadout.LevelRequired.ToString() + ") " + "for this loadout.");
                        return;
                    }

                    EPlayer.GetAccount(player).LoadoutCop = loadout.Name;
                    player.TriggerEvent("pushMsgToChat", "!{#33C446}[#] !{#FFFFFF}You have set your cop loadout to " + loadout.Name + ".");
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
                    ContextFactory.SaveDB();
                    player.TriggerEvent("pushMsgToChat", "!{#00FF33}You have been unjailed as you have served your time.");
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
                    ContextFactory.SaveDB();
                    player.TriggerEvent("pushMsgToChat", "!{#00FF33}You have been unmuted as you have served your time.");
                }
            }
            return;
        }

        [RemoteEvent("opsp")] // wont trigger incar btw 
        public void OnPlayerShootPlayer(Player player, Player target, uint bone, Int64 weapon)
        {
            if (player == null || target == null) return;

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
                                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Do not tase your own teammates! You have been given a teamattack warning.");
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
                                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You have been killed for team tasing!");
                                    warnings = 0;
                                }

                                player.SetData("TeamShotWarning", warnings);
                            }
                            return;
                        }
                    }

                    if (target.HasSharedData("TasedTill") && Environment.TickCount64 - (target.GetSharedData<long>("TasedTill") - 18000) >= 2000)
                    {
                        if (lobby.GameProgress < 60)
                        {
                            player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot tase at the start of the round!");
                            return;
                        }

                        int time = 18000;
                        target.TriggerEvent("setToRagdoll", time, time * 2, 1, false, false, false);
                        target.SetSharedData("TasedTill", Environment.TickCount64 + 18000);
                        target.TriggerEvent("clearTaseAnims", 18000);

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
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot tase a player that has shot.");
                }
            }
            if(target.GetSharedData<int>("Team") >= 0 && target.GetSharedData<int>("Wounded") == 1)
            {
                target.Health = 100;
            }
            if (target.GetSharedData<int>("Team") >= 0 && target.GetSharedData<int>("Team") != player.GetSharedData<int>("Team") && target.GetSharedData<int>("Wounded") == 2)
            {
                target.Health = 0;
            }
        }

        [RemoteEvent("SetHealth")]
        public void SetPlayerHealth(Player player, int HP)
        {
            if (player == null) return;
            player.Health = HP;
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
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Please do not get out of your vehicle in derby!");
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
            target.TriggerEvent("pushMsgToChat", "!{#FFFF00}Your wounds have been treated and you gained your strength back.");
            
            //target.StopAnimation();
            RequestPlayAnimation(target, "amb@world_human_push_ups@male@exit", "exit", 0, 9);
            NAPI.Util.ConsoleOutput("OnRevivePlayer " + player.Name + " " + target.Name);
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (target != null)
                    {
                        RequestClearAnimation(target);
                        target.StopAnimation();
                    }
                }
                catch { }
            }, 3000);

            target.SetSharedData("Wounded", 0);
            target.TriggerEvent("ResetWounded");

            player.TriggerEvent("pushMsgToChat", $"!{{#2fc900}}[REVIVE] You successfully revived {target.Name} and gained +2XP & 1 point for teamwork!");
            target.TriggerEvent("pushMsgToChat", $"!{{#2fc900}}[REVIVE] You have been revived by {player.Name}.");

            EPlayer.GrantXP(player, 2);
            EPlayer.GivePoints(player, 1);
            EPlayer.UnlockAchievement(player, "first_revive");
            EPlayer.UnlockAchievement(target, "get_revived");

            RequestClearAnimation(player);
            target.SetSharedData("LastKiller", "n/a");
            target.SetSharedData("BeingRevived", false);
            player.SetSharedData("ReviveTarget", "n/a");
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

            player.SetSharedData("ReviveTarget", target.Name);
            target.SetSharedData("BeingRevived", false);

            player.TriggerEvent("pushMsgToChat", $"!{{#2fc900}}[REVIVE] You have begun reviving {target.Name} - keep holding H for 7 seconds.");
            target.TriggerEvent("pushMsgToChat", $"!{{#2fc900}}[REVIVE] You are now being revived by {player.Name} - it will take 7 seconds.");

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
            player.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}[!] !{{#FFFFFF}}You began cuffing {target.Name}, keep holding !{{#FFFF00}}H !{{#FFFFFF}}to handcuff them.");
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

            NAPI.Util.ConsoleOutput("StopCuffing " + player.Name);

            Events.RequestClearAnimation(player);
        }
        [RemoteEvent("StopReviving")]
        public static void OnStopReviving(Player player)
        {
            if (player == null) return;
            if (player.GetSharedData<string>("ReviveTarget").Equals("n/a")) return;

            Player target = RAGE.Entities.Players.All.FirstOrDefault(p => p.Name.Equals(player.GetSharedData<string>("ReviveTarget")));

            player.SetSharedData("ReviveTarget", "n/a");

            NAPI.Util.ConsoleOutput("OnStopReviving " + player.Name);

            if (target != null)
                target.SetSharedData("BeingRevived", false);

            if (player.GetSharedData<int>("Wounded") == 0)
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
                            admin.TriggerEvent("pushMsgToChat", "!{#33C446}[#] !{#FFFFFF}The screenshot has been uploaded. To view it use /viewss last or /viewss (id)");
                            admin.TriggerEvent("pushMsgToChat", $"!{{#33C446}}[#] !{{#FFFFFF}}Controls: F4 (Stop viewing), F2 (Toggle cursor) | Metadata for preserving/viewing:");
                            admin.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}ID: {data} | Requestee: {identifier} | Source: {player.Name}");
                            admin.SetData("LastSSIdentifier", data);
                        }
                        else
                        {
                            admin.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}The screenshot of !{#FF6347}" + player.Name + "'s !{#FFFFFF}game has failed to upload.");
                            admin.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Error Details: " + data);
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
                            admin.TriggerEvent("pushMsgToChat", "!{#33C446}[#] !{#FFFFFF}The GIF has been uploaded. To view it use /viewss last or /viewss (id)");
                            admin.TriggerEvent("pushMsgToChat", $"!{{#33C446}}[#] !{{#FFFFFF}}Controls: F4 (Stop viewing), F2 (Toggle cursor) | Metadata for preserving/viewing:");
                            admin.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}ID: {data} | Requestee: {identifier} | Source: {player.Name}");
                            admin.SetData("LastSSIdentifier", data);
                        }
                        else
                        {
                            admin.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}The GIF of !{#FF6347}" + player.Name + "'s !{#FFFFFF}game has failed to upload.");
                            admin.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Error Details: " + data);
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

            EPlayer.UnlockAchievement(player, "first_arrest");
            EPlayer.TryAdvanceChallenge(player, Challenge.ChallengeType.Arrest, "N/A");


            target.SetAccessories(7, 41, 0); // Handcuffs

            EPlayer.Handcuff(target, true);

            EPlayer.GetAccount(player).Arrests++;
            EPlayer.GivePoints(player, 5);

            if(EPlayer.GetAccount(player).Arrests >= 100)
            {
                EPlayer.UnlockAchievement(player, "100_arrests");
            }

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
            else if(slot == 56)
            {
                veh.WheelType = mod;
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

            uint vmodel = player.Vehicle.Model;
            Account acc = EPlayer.GetAccount(player);
            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    var tuning = await context.PlayerTunings.AsQueryable().FirstOrDefaultAsync(a => a.AccountSQLID == acc.SQL_ID && a.Model == player.Vehicle.Model);

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
                            NAPI.Task.Run(() =>
                            {
                                if (!player.HasData("TunerNoRefundWarn"))
                                {
                                    player.SetData("TunerNoRefundWarn", true);
                                    EPlayer.Notify(player, "Removing all components! <p>THIS ACTION IS IRREVERSIBLE AND NON REFUNDABLE, USE AGAIN TO PROCEED.</p>", "red", "fas fa-exclamation", false, 2500, "topCenter", "replace");
                                    return;
                                }
                            });                
                            context.PlayerTunings.Remove(tuning);
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
                                    NAPI.Task.Run(() =>
                                    {
                                        EPlayer.Notify(player, $"You need {(vMods.Keys.Count * 10) - acc.DonatorPoints} more donator points for this upgrade! <p>You can only purchase max 2 components with standard points.</p>", "red", "fas fa-lock", false, 2500, "topCenter", "replace");
                                    });
                                    return;
                                }
                                acc.DonatorPoints -= (vMods.Keys.Count * 10);
                            }
                            else
                            {
                                if (acc.Points < (vMods.Keys.Count * 50))
                                {
                                    NAPI.Task.Run(() =>
                                    {
                                        EPlayer.Notify(player, $"You need {(vMods.Keys.Count * 50) - acc.Points} more points for this upgrade!", "red", "fas fa-lock", false, 2500, "topCenter", "replace");
                                    });
                                    return;
                                }
                                NAPI.Task.Run(() =>
                                {
                                    EPlayer.GivePoints(player, -(vMods.Keys.Count * 50));
                                });
                            }
                            NAPI.Task.Run(() =>
                            {
                                EPlayer.Notify(player, $"Modifications saved!", "green", "fas fa-check", false, 2500, "topCenter");
                            });

                            tuning = new PlayerTunings
                            {
                                AccountSQLID = acc.SQL_ID,
                                Model = vmodel,
                                PrimaryColor = priCol,
                                SecondaryColor = secCol,
                                PearlescentColor = pearlCol,
                                WheelColor = wheelCol,
                                Mods = NAPI.Util.ToJson(vMods),
                            };
                            await context.PlayerTunings.AddAsync(tuning);
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
                                    NAPI.Task.Run(() =>
                                    {
                                        EPlayer.Notify(player, $"You need {(changes * 10) - acc.DonatorPoints} more donator points for this upgrade! <p>You can only purchase max 2 components with standard points.</p>", "red", "fas fa-lock", false, 2500, "topCenter", "replace");
                                    });
                                    return;
                                }
                                acc.DonatorPoints -= (changes * 10);
                            }
                            else
                            {
                                NAPI.Task.Run(() =>
                                {
                                    if (acc.Points < (changes * 50))
                                    {

                                        EPlayer.Notify(player, $"You need {(changes * 50) - acc.Points} more points for this upgrade!", "red", "fas fa-lock", false, 2500, "topCenter", "replace");
                                        return;
                                    }
                                    EPlayer.GivePoints(player, -(changes * 50));
                                });
                            }

                            if (changes > 0)
                            {
                                NAPI.Task.Run(() =>
                                {
                                    EPlayer.Notify(player, $"Modifications saved!", "green", "fas fa-check", false, 2500, "topCenter");
                                });                           
                                tuning.PrimaryColor = priCol;
                                tuning.SecondaryColor = secCol;
                                tuning.PearlescentColor = pearlCol;
                                tuning.WheelColor = wheelCol;
                                tuning.Mods = NAPI.Util.ToJson(vMods);
                            }
                        }
                    }
                    NAPI.Task.Run(() =>
                    {
                        player.TriggerEvent("CloseTuner");
                    });
                    await NAPI.Task.WaitForMainThread();
                    await context.SaveChangesAsync();
                }
            });
            
        }

        [ServerEvent(Event.PlayerSpawn)]
        public static void OnPlayerSpawn(Player player)
        {
            if (player == null || !player.Exists) return;

            if (player.HasData("Kicked")) return;

            LobbyData lobby = Settings.Lobbies.Find(lobby => lobby.lobbyId == player.GetSharedData<int>("InLobby"));
            if (lobby != null && lobby.gameStatus != 0 && (lobby.Cops.Contains(player) || lobby.Fugitives.Contains(player)))
            {
                return; // WHITE COLOR BUG FYX..
            }


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

                if (!EPlayer.LoggedIn(player))
                {
                    EPlayer.LoginCameraSetup(player);
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

                    player.SetSharedData("AutoJoin", false);
                    player.SetSharedData("InLobby", -2);
                    player.SetSharedData("JailTime", acc.JailTime);
                    player.TriggerEvent("SetJailCounter", acc.JailTime);
                    EPlayer.SetColor(player, new Color(168, 128, 8, 255));
                    player.Position = (Vector3)Settings.VectorJail[0];
                    player.Heading = (float)Settings.VectorJail[1];
                    player.Dimension = 200 + Convert.ToUInt32(player.Id);
                    EPlayer.Freeze(player, true);
                    player.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}[JAILED] !{{#FFFFFF}}You are in jail for {acc.JailTime} seconds, reason: {acc.JailReason}.");

                    return;
                }


                if (player.GetData<bool>("UnfairKillSpawn") == true)
                {
                    player.SetData("UnfairKillSpawn", false);
                    return;
                }

                player.TriggerEvent("HideChaseTimer");

                player.TriggerEvent("SetTimeWeather", 12, 0, 0, 0);

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
                        if (player.GetSharedData<int>("PlayerWeather") == -1)
                            player.TriggerEvent("SetTimeWeather", lobby.Time[0], lobby.Time[1], lobby.Time[2], lobby.Weather);
                        else player.TriggerEvent("SetTimeWeather", lobby.Time[0], lobby.Time[1], lobby.Time[2], player.GetSharedData<int>("PlayerWeather"));

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
                        player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You have been removed from DM as it's locked while copchase starts.");
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
                else if (player.GetSharedData<int>("InLobby") == -6)
                {
                    player.RemoveAllWeapons();

                    player.TriggerEvent("SetActorInvulnerable", false);

                    int dmarena = player.GetSharedData<int>("InDM");
                    Task.Run(async () =>
                    {
                        var arena = await EPlayer.IsValidDMArena(dmarena);

                        if (arena.Value == false)
                        {
                            NAPI.Task.Run(() =>
                            {
                                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}The arena you are playing in does not exist.");
                                player.SetSharedData("InDM", -1);
                                player.SetSharedData("Team", -1);

                                EPlayer.Spawn(player);
                            });
                            return;
                        }

                        var team = await EPlayer.IsValidDMTeam(arena.Key.SQL_ID, player.GetSharedData<int>("Team"));

                        if (team.Value == false)
                        {
                            NAPI.Task.Run(() =>
                            {
                                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}The arena you are playing in does not have the team you are set as, resetting...");
                                player.SetSharedData("InDM", -1);
                                player.SetSharedData("Team", -1);

                                EPlayer.Spawn(player);
                            });

                            return;
                        }

                        NAPI.Task.Run(() => { player.TriggerEvent("showHPBars", team.Key.ShowHPBars); });

                        using (MainDatabase context = ContextFactory.CreateContext())
                        {
                            var spawn = context.CustomArenas_Spawns.AsQueryable().OrderBy(a => Guid.NewGuid()).FirstOrDefault(a => a.ArenaID == arena.Key.SQL_ID &&
                            (a.Team == team.Key.SharedSpawns || a.Team == team.Key.Team));

                            if (spawn != null && spawn != default(CustomDMArenaSpawns))
                            {
                                NAPI.Task.Run(() =>
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
                                                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}This DM arena does not have weapons set for your team " + team.Key.Team);
                                                    return;
                                                }

                                                DM_WeaponSets[] weaponSets = NAPI.Util.FromJson<DM_WeaponSets[]>(team.Key.WeaponSets);
                                                var set = weaponSets.OrderBy(a => Guid.NewGuid()).FirstOrDefault();

                                                if (set == null)
                                                {
                                                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}This DM arena does not have a proper weapons set for your team " + team.Key.Team);
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
                                });
                            }
                        }
                    });
                }
                else
                {
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}The arena you are playing in does not have a spawn for your team id!");
                }
            }
        }
    }
}
