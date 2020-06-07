using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace LSCC
{
    class Commands : Script
    {
        [Command("serveroptions", GreedyArg = true)]
        public void cmd_serveroptions(Player player, string action)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 4)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not an admin.");
                return;
            }

            if (action.StartsWith("maxinlobby", StringComparison.OrdinalIgnoreCase))
            {
                string[] args = action.Split(' ');
                if (args.Count() > 1)
                {
                    int value = 0;
                    if (!Int32.TryParse(args[1], out value) || value < 1 || value > 999)
                    {
                        player.SendChatMessage($"!{{#999999}}Max players in a lobby should be between 1-999!");
                        return;
                    }

                    Settings.MaxPlayersInLobby = value;

                    NAPI.Chat.SendChatMessageToAll($"!{{#FF6347}}[AdmCmd] !{{#FFFFFF}}{player.Name} has changed max players in a lobby to {value}.");
                }
                else player.SendChatMessage("!{#999999}Proper usage: /serveroptions maxinlobby <amount>");
            }
      
            else player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Invalid option, possible options: 'maxinlobby'");

            return;
        }

        [Command("trunk", Alias = "boot")]
        public void cmd_trunk(Player player)
        {
            if(!player.IsInVehicle || player.Vehicle == null) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not in any vehicle!"); return; }

            if (player.Vehicle.HasSharedData("Status_Trunk"))
            {
                if (player.Vehicle.GetSharedData<bool>("Status_Trunk") == false)
                {
                    player.Vehicle.SetSharedData("Status_Trunk", true);
                }
                else
                {
                    player.Vehicle.SetSharedData("Status_Trunk", false);
                }
            }
            else
            {
                player.Vehicle.SetSharedData("Status_Trunk", true);
            }

            return;
        }

        [Command("time", Alias = "weather")]
        public void cmd_timeweather(Player player)
        {
            if (!EPlayer.LoggedIn(player)) return;

            if(player.GetSharedData<int>("InLobby") < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You must be in a lobby to use this command!");
                return;
            }

            LobbyData lobby = Settings.Lobbies.Find(a => a.lobbyId == player.GetSharedData<int>("InLobby"));
            if ((lobby != null) == false)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You must be in a lobby to use this command.");
                return;
            }

            player.SendChatMessage($"!{{#A8FFAB}}Time In Lobby: (H:M:S) !{{#FFFFFF}}{lobby.Time[0]}:{lobby.Time[1]}:{lobby.Time[2]}");
            player.SendChatMessage("!{#A8FFAB}Weather Forecast: !{#FFFFFF}" + Main.WeatherToForecast(lobby.Weather));

            return;

        }


        [Command("game", GreedyArg = true)]
        public void cmd_game(Player player, string action)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 2)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not an admin.");
                return;
            }

            if (action.StartsWith("stop", StringComparison.OrdinalIgnoreCase))
            {
                string[] args = action.Split(' ');
                if (args.Count() > 1)
                {
                    int value = 0;
                    if (!Int32.TryParse(args[1], out value) || value < 1 || value > Settings.Lobbies.Count)
                    {
                        player.SendChatMessage($"!{{#999999}}Lobby ID must be between 1-{Settings.Lobbies.Count}!");
                        return;
                    }

                    if (!Main.StopGame(value, 0))
                    {
                        player.SendChatMessage("!{#FF0000}Failed to stop game! Is there even a copchase running?");
                    }
                    else NAPI.Chat.SendChatMessageToAll($"!{{#FF6347}}[AdmCmd] !{{#FFFFFF}}{player.Name} has forcefully stopped the copchase in lobby {value}.");
                }
                else player.SendChatMessage("!{#999999}Proper usage: /game stop <lobby id>");
            }
            else if (action.StartsWith("start", StringComparison.OrdinalIgnoreCase))
            {
                string[] args = action.Split(' ');
                if (args.Count() > 1)
                {
                    int value = 0;
                    if (!Int32.TryParse(args[1], out value) || value < 1 || value > 99)
                    {
                        player.SendChatMessage($"!{{#999999}}Lobby ID must be between 1-{Settings.Lobbies.Count}!");
                        return;
                    }

                    if (!Main.StartGame(value))
                    {
                        player.SendChatMessage("!{#FF0000}Failed to start game! Is there enough players or a game running already?");
                    }
                    else NAPI.Chat.SendChatMessageToAll($"!{{#FF6347}}[AdmCmd] !{{#FFFFFF}}{player.Name} has forcefully started the copchase in lobby {value}.");
                }
                else player.SendChatMessage("!{#999999}Proper usage: /game start <lobby id>");
            }
            else if (action.StartsWith("autostart", StringComparison.OrdinalIgnoreCase))
            {
                string[] args = action.Split(' ');
                if (args.Count() > 1)
                {
                    int value = 0;
                    if (!Int32.TryParse(args[1], out value) || value < 1 || value > 99)
                    {
                        player.SendChatMessage($"!{{#999999}}Lobby ID must be between 1-{Settings.Lobbies.Count}!");
                        return;
                    }
                    LobbyData lobby = Settings.Lobbies.Find(a => a.lobbyId == value);
                    if (lobby != null)
                    {
                        lobby.GameAutoStart = !lobby.GameAutoStart;
                        if (lobby.GameAutoStart) NAPI.Notification.SendNotificationToAll($"~r~<C>{player.Name}</C> ~g~has enabled copchases for lobby {lobby.lobbyId}.");
                        else NAPI.Notification.SendNotificationToAll($"~r~<C>{player.Name}</C> ~w~has disabled copchases for lobby {lobby.lobbyId}.");
                    }
                    else NAPI.Chat.SendChatMessageToAll($"!{{#FF6347}}[AdmCmd] !{{#FFFFFF}}The lobby id is invalid or doesn't exist.");
                }
                else player.SendChatMessage("!{#999999}Proper usage: /game autostart <lobby id>");
            }
            else if (action.StartsWith("teambalance", StringComparison.OrdinalIgnoreCase))
            {
                string[] args = action.Split(' ');
                if (args.Count() > 2)
                {
                    int value = 0;
                    int lobbyid = 0;
                    if (!Int32.TryParse(args[1], out lobbyid) || lobbyid < 1 || lobbyid > Settings.Lobbies.Count)
                    {
                        player.SendChatMessage("!{#999999}Lobby ID must be in numbers between 1-" + Settings.Lobbies.Count + "!");
                        return;
                    }
                    if (!Int32.TryParse(args[2], out value) || value < 1 || value > 99) 
                    { 
                        player.SendChatMessage("!{#999999}Team balance must be in numbers between 1-99!"); 
                        return; 
                    }

                    LobbyData lobby = Settings.Lobbies.Find(a => a.lobbyId == lobbyid);
                    if ((lobby != null) == false)
                    {
                        player.SendChatMessage("!{#999999}Lobby ID is invalid!");
                        return;
                    }

                    lobby.FugitivePercent = value;

                    NAPI.Chat.SendChatMessageToAll($"!{{#FF6347}}[AdmCmd] !{{#FFFFFF}}{player.Name} has set lobby {lobby.lobbyId}'s fugitives percentage to {lobby.FugitivePercent}%.");
                }
                else player.SendChatMessage("!{#999999}Proper usage: /game teambalance <lobby id> <percentage of fugitives>");
            }
            else player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Invalid option, possible options: 'start', 'stop', 'autostart', 'teambalance'");

            return;
        }

        [Command("tog", GreedyArg = true)]
        public void cmd_tog(Player player, string action)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (action.StartsWith("hud", StringComparison.OrdinalIgnoreCase))
            {
                player.SetData("HUDState", !player.GetData<bool>("HUDState"));
                if (player.GetData<bool>("HUDState") == true)
                {
                    player.SendChatMessage("!{#FF6347}[#] !{#FFFFFF}You have enabled your HUD.");
                }
                else player.SendChatMessage("!{#FF6347}[#] !{#FFFFFF}You have disabled your HUD.");

                player.TriggerEvent("ToggleHud", player.GetData<bool>("HUDState"));
            }
            else if (action.StartsWith("pm", StringComparison.OrdinalIgnoreCase))
            {
                player.SetData("PMState", !player.GetData<bool>("PMState"));
                if (player.GetData<bool>("PMState") == true)
                {
                    player.SendChatMessage("!{#FF6347}[#] !{#FFFFFF}You have enabled your PMs.");
                }
                else player.SendChatMessage("!{#FF6347}[#] !{#FFFFFF}You have disabled your PMs.");
            }
            else player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Invalid option, possible options: 'hud', 'pm'");

            return;
        }

        [Command("aveh", Alias = "av")]
        public void cmd_aveh(Player player, string carname)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not an admin.");
                return;
            }

            Random rand = new Random();

            Vehicle aVehicle = NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(carname), player.Position.Around(5.0f), player.Heading, rand.Next(122), rand.Next(122), player.SocialClubName, 255, false, true, player.Dimension);

            aVehicle.SetSharedData("Team", -1);
       
            Settings.AdminVehicles.Add(aVehicle);

            NAPI.Task.Run(() =>
            {
                if(!aVehicle.IsNull) player.SetIntoVehicle(aVehicle, 0);
            }, 1000);

            return;
        }

        [Command("veh", Alias = "v")]
        public void cmd_veh(Player player, string carname)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (player.GetSharedData<int>("InLobby") != -3 || player.Dimension != 256)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not in freeroam!");
                return;
            }

            List<Vehicle> freeroamVehs = new List<Vehicle>();
            freeroamVehs = player.GetData<List<Vehicle>>("FreeroamVehicles");

            if(freeroamVehs.Count >= 10)
            {
                Vehicle veh = freeroamVehs.First();
                freeroamVehs.Remove(veh);
                veh.Delete();
                player.SendNotification("~r~You had too many freeroam vehicles, removed the least recent.");
            }

            Random rand = new Random();

            Vehicle aVehicle = NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(carname), player.Position.Around(5.0f), player.Heading, rand.Next(122), rand.Next(122), player.SocialClubName, 255, false, true, player.Dimension);

            aVehicle.SetSharedData("Team", -1);

            freeroamVehs.Add(aVehicle);

            player.SetData("FreeroamVehicles", freeroamVehs);

            player.SendChatMessage("!{#FF6347}>> !{#FFFFFF}You have spawned a " + carname + " at your current location.");

            NAPI.Task.Run(() =>
            {
                if (!aVehicle.IsNull) player.SetIntoVehicle(aVehicle, 0);
            }, 1000);

            return;
        }

        [Command("forcelogin", Alias = "flogin")]
        public static void cmd_forcelogin(Player player, Player target, string acc, string pass)
        {
            Events.OnPlayerLoginAttempt(target, acc, pass);
        }

        [Command("gps", Alias = "bk")]
        public static void cmd_gps(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            bool isACop = false;
            LobbyData lobby = Settings.Lobbies.Find(lobby => lobby.lobbyId == player.GetSharedData<int>("InLobby"));
            if (lobby != null)
            {
                if (lobby.Cops.Contains(player) && lobby.gameStatus == 1)
                {
                    isACop = true;
                }
            }

            if (!isACop)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not a cop.");
                return;
            }

            player.SetData("GPS", !player.GetData<bool>("GPS"));

            player.SendChatMessage("!{#FF6347}[#] !{#FFFFFF}You have " + (player.GetData<bool>("GPS") == true ? "enabled" : "disabled") + 
                " your emergency GPS beacon.");
            return;
        }

        [Command("goto")]
        public void cmd_goto(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not an admin.");
                return;
            }

            if (player.IsInVehicle)
            {
                player.Vehicle.Position = target.Position.Around(5.0f);
            }
            else
            {
                player.Position = target.Position.Around(3.0f);
            }
            player.Dimension = target.Dimension;

            EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has teleported to {target.Name}.");
            return;
        }

        [Command("bring", Alias = "gethere")]
        public void cmd_bring(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not an admin.");
                return;
            }

            if (target.IsInVehicle)
            {
                target.Vehicle.Position = player.Position.Around(5.0f);
                target.Vehicle.Dimension = player.Dimension;
            }
            else
            {
                target.Position = player.Position.Around(3.0f);
            }
            target.Dimension = player.Dimension;

            EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has summoned {target.Name}.");
            return;
        }

        [Command("sethp")]
        public void cmd_sethp(Player player, Player target, int health)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not an admin.");
                return;
            }

            target.Health = health;
            return;
        }

        [Command("setinv", Alias = "setinvulnerable")]
        public void cmd_setinv(Player player, Player target, int status)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not an admin.");
                return;
            }

            player.TriggerEvent("SetActorInvulnerable", status > 0 ? true : false);
            player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You have toggled " + target.Name + "'s invulnerability.");
            return;
        }

        [Command("setar")]
        public void cmd_setar(Player player, Player target, int armour)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not an admin.");
                return;
            }

            target.Armor = armour;
            return;
        }

        [Command("stopanim", Alias = "sa")]
        public void cmd_sa(Player player)
        {
            if(player.HasData("TasedTill") && Environment.TickCount < player.GetData<int>("TasedTill"))
                { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot do this right now as you're tased."); return; }

            if (player.HasData("Cuffed"))
                { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot do this while cuffed."); return; }

            if (player.IsInVehicle) 
                { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot do this inside of a vehicle!"); return; }

            if (player.HasData("Handsup"))
            {
                player.TriggerEvent("SetHandsup", false);
                player.ResetData("Handsup");
            }
            else
            {
                player.StopAnimation();
                Events.RequestClearAnimation(player);
            }
        }

        [Command("handsup")]
        public void cmd_handsup(Player player)
        {
            if (player.HasData("Cuffed") || (player.HasData("TasedTill") && Environment.TickCount < player.GetData<int>("TasedTill"))) 
                { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot do this right now."); return; }

            if (player.IsInVehicle)
                { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot do this inside of a vehicle!"); return; }

            if (player.HasData("Handsup"))
            {
                player.TriggerEvent("SetHandsup", false);
                player.ResetData("Handsup");
            }
            else
            {
                player.TriggerEvent("SetHandsup", true);
                player.SetData("Handsup", true);
            }
           
            return;
        }

        [Command("cuff")]
        public void cmd_cuff(Player player, Player target)
        {
            bool isACop = false;
            LobbyData lobby = Settings.Lobbies.Find(lobby => lobby.lobbyId == player.GetSharedData<int>("InLobby"));
            if (lobby != null)
            {
                if (lobby.Cops.Contains(player) && lobby.gameStatus == 1 && lobby.Fugitives.Contains(target) && !target.HasData("Cuffed"))
                {
                    isACop = true;
                }
            }

            if (!isACop)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not a cop or the target cannot be cuffed!");
                return;
            }

            if (player.Position.DistanceTo(target.Position) > 3.0) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not near that player!"); return; }

            try
            {
                foreach (Player cop in lobby.Cops)
                {
                    if (cop.HasData("CuffingTarget") && cop.GetData<Player>("CuffingTarget") == target)
                    {
                        player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}The target is already being cuffed by someone!");
                        return;
                    }
                }
            }
            catch { }

            player.SetData("CuffingTarget", target);
            EPlayer.Action(player, $"attempts to grapple {target.Name} as they reach for their cuffs.");
            player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are now cuffing the target, stay close to them for 3 seconds to complete the operation.");

            NAPI.Task.Run(() =>
            {
                if(target == null || !target.IsInstanceAlive || player == null || !player.IsInstanceAlive)
                {
                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Operation failed, target or you are no longer alive.");
                    player.ResetData("CuffingTarget");
                    return;
                }

                LobbyData lobbye = Settings.Lobbies.Find(lob => lob.lobbyId == player.GetSharedData<int>("InLobby"));
                if (lobbye != null)
                {
                    if (lobbye.Cops.Contains(player) && lobbye.gameStatus == 1 && lobbye.Fugitives.Contains(target) && !target.HasData("Cuffed"))
                    {
                        isACop = true;
                    }
                }

                if (!isACop)
                {
                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Operation failed, you cannot cuff that target anymore.");
                    player.ResetData("CuffingTarget");
                    return;
                }

                if (player.Position.DistanceTo(target.Position) > 2.0) 
                { 
                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not near the target!");
                    player.ResetData("CuffingTarget");

                    return; 
                }

                player.ResetData("CuffingTarget");

                EPlayer.GrantXP(player, 5);
                EPlayer.Action(player, $"has grabbed ahold of {target.Name} and slapped the handcuffs on their wrists.");
                EPlayer.Handcuff(target, true);

                Events.RequestPlayAnimation(player, "mp_arresting", "a_uncuff", 2);

                NAPI.Task.Run(() =>
                {
                    Events.RequestClearAnimation(player);
                }, 2000);

                target.Heading = player.Heading;

                Events.OnPlayerDeath(target, player, 696988); // Reason 696988 = handcuffs.

            }, 3000);
            return;
        }

        [Command("givegun", GreedyArg = true)]
        public void cmd_givegun(Player player, Player target, int ammo, string wep)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 3)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not a level 3 admin.");
                return;
            }


            UInt64 wephash = 0;
            bool found = false;
            string wepname = "";

            if (!UInt64.TryParse(wep, out wephash))
            {
                wephash = 0;
            }

            foreach (KeyValuePair<uint, string> i in Main.WeaponReasons)
            {
                if (wephash != 0)
                {
                    if (i.Key == wephash)
                    {
                        found = true;
                        wepname = i.Value;
                        break;
                    }
                }
                else
                {
                    if (i.Value.Contains(wep, StringComparison.CurrentCultureIgnoreCase))
                    {
                        found = true;
                        wephash = i.Key;
                        wepname = i.Value;
                        break;
                    }
                }
            }

            
            if(found == false)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Invalid weapon! Try using hashes or exact names.");
                return;
            }

            target.GiveWeapon((WeaponHash)wephash, ammo);

            player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You have given a " + wepname + " with " + ammo + " ammo to the target.");
            return;
        }

        [Command("fix")]
        public void cmd_fix(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not a level 1 admin.");
                return;
            }

            if(!target.IsInVehicle) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Target is not in any vehicle!"); return; }

            target.Vehicle.Repair();
            RAGE.Entities.Players.All.ForEach((Player p) =>
            {
                p.SendChatMessage($"!{{#FF6347}}[AdmCmd] {player.Name} has repaired {target.Name}'s vehicle.");
            });

            return;
        }

        [Command("flip")]
        public void cmd_flip(Player player)
        {
            player.SendChatMessage("In GTA V you can literally unflip by yourself, dumbo.");
            return;
        }

        [Command("removeavehs")]
        public void cmd_removeavehs(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not an admin.");
                return;
            }

            for (int i = Settings.AdminVehicles.Count - 1; i >= 0; i--)
            {
                if (!Settings.AdminVehicles[i].IsNull && Settings.AdminVehicles[i] != null)
                {
                    Settings.AdminVehicles[i].Delete();
                    Settings.AdminVehicles.RemoveAt(i);
                }
            }

            Settings.AdminVehicles.Clear();
            EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} removed all spawned admin vehicles.");
            return;
        }

        [Command("autojoin")]
        public void cmd_autojoin(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (player.GetSharedData<int>("InLobby") == -5) return;

            if (EPlayer.GetAccount(player).JailTime > 0)
            {
                player.SetData("AutoJoin", false);
                player.SendChatMessage("You are in jail!");
                return;
            }

            if (player.GetData<bool>("AutoJoin") == true)
            {
                player.SendNotification("~r~AutoJoin has been turned off.", false);
                player.SetData("AutoJoin", false);
            }
            else
            {
                player.SendNotification("~w~AutoJoin has been turned on.", false);
                player.SetData("AutoJoin", true);
            }
        }

        [Command("stats")]
        public void cmd_stats(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            Account acc = EPlayer.GetAccount(player);

            player.SendChatMessage($"_____| Statistics for {acc.Username} at {DateTime.Now.ToString()} |_____");
            player.SendChatMessage($"| Account ID: {acc.SQL_ID} | Level: {acc.Level} | XP: {acc.XP}/{EPlayer.GetMaxLevelXP(acc.Level)} ");
            player.SendChatMessage($"| Email: {acc.Email} | Donator Level: {acc.DonatorLevel} | Admin Level: {acc.Admin}");
            player.SendChatMessage($"_____| Statistics for {acc.Username} at {DateTime.Now.ToString()} |_____");

            EPlayer.GrantXP(player, 0);
            return;
        }

        [Command("pos")]
        public void cmd_poslog(Player player)
        {
            if (!player.IsInVehicle)
            {
                player.SendChatMessage($"X: {player.Position.X} - Y: {player.Position.Y} - Z: {player.Position.Z}");
                player.SendChatMessage($"Heading: {player.Heading}");
                NAPI.Util.ConsoleOutput($"{player.Name} Position log: {{ new Vector3({player.Position.X}f, {player.Position.Y}f, {player.Position.Z}f), {player.Heading}f }},");
            }
            else
            {
                player.SendChatMessage($"vX: {player.Vehicle.Position.X} - vY: {player.Vehicle.Position.Y} - vZ: {player.Vehicle.Position.Z}");
                player.SendChatMessage($"vHeading: {player.Vehicle.Heading}");
                NAPI.Util.ConsoleOutput($"{player.Name} Position log: {{ new Vector3({player.Vehicle.Position.X}f, {player.Vehicle.Position.Y}f, {player.Vehicle.Position.Z}f), {player.Vehicle.Heading}f }},");
            }
        }

        [Command("voicemode")]
        public void cmd_voicemode(Player player)
        {
            Events.OnScriptedKeyPress(player, 0x09, true);
            return;
        }

        [Command("voiceblock", GreedyArg = true)]
        public void cmd_vblock(Player player, Player target)
        {
            List<Player> blockList = new List<Player>();
            if (player.HasData("BlockList"))
            {
                blockList.AddRange(player.GetData<List<Player>>("BlockList"));
            }

            if(!blockList.Contains(target))
            {
                blockList.Add(target);
                player.SendChatMessage($"!{{#FF6347}}[!] !{{#FFFFFF}}You have muted {target.Name} on voice chat, use this command again to unmute them.");
            }
            else
            {
                blockList.Remove(target);
                player.SendChatMessage($"!{{#FF6347}}[#] !{{#FFFFFF}}You have unmuted {target.Name} on voice chat.");
            }

            player.SetData("BlockList", blockList);

            return;
        }

        [Command("lobby")]
        public void cmd_lobby(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (player.GetSharedData<int>("InLobby") == -5) return;

            if (player.HasData("DerbyVehicle"))
            {
                player.GetData<Vehicle>("DerbyVehicle").Delete();

                player.ResetData("DerbyVehicle");
            }

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

            if(!player.HasData("ForcedData"))
            {
                // Do lobby checks here so /sendtolobby ignores all of them.
            }

            freeroamVehs.Clear();
            player.SetData("FreeroamVehicles", freeroamVehs);
            player.SetSharedData("InDM", -1);
            player.ResetData("ForcedData");

            player.SetSharedData("InLobby", -1);
            player.SendChatMessage($"!{{#FFFF00}}[!] Sending you to !{{#FF6347}}LOBBY.");
            EPlayer.Spawn(player);

            return;
        }

        [Command("mylobby")]
        public void cmd_mylobby(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;


            player.SendChatMessage($"!{{#FFFF00}}You are in lobby {player.GetSharedData<int>("InLobby")}, /switchlobby to change to another lobby.");

            player.SendChatMessage("Current Lobby Players:");
            RAGE.Entities.Players.All.ForEach((Player ePlayer) =>
            {
                player.SendChatMessage($"!{{#999999}}{ePlayer.Name}");
            });

            return;
        }

        [Command("updaterate", GreedyArg = true)] // It's a client-side command we're adding here just so it doesn't show "invalid command" when triggered.
        public void cmd_updaterate(Player player)
        {
            return;
        }

        [Command("fontsize", GreedyArg = true)] // It's a client-side command we're adding here just so it doesn't show "invalid command" when triggered.
        public void cmd_fontsize(Player player)
        {
            return;
        }

        [Command("rappel", GreedyArg = true)] // It's a client-side command we're adding here just so it doesn't show "invalid command" when triggered.
        public void cmd_rappel(Player player)
        {
            return;
        }


        [Command("lobbies")]
        public void cmd_lobbies(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            List<LobbyData> lobbyList = Settings.Lobbies.ToList();
            string enabledStr = "";
            foreach (LobbyData lobby in lobbyList)
            {
                if (lobby.Enabled)
                {
                    if (lobby.Password.Length > 1 && lobby.PlayerCreated)
                    {
                        enabledStr = "!{#FF6347}PASSWORD/LOCKED";
                    }
                    else enabledStr = "!{#00FF33}ENABLED";
                }
                else
                {
                    enabledStr = "!{#FF0000}DISABLED";
                }

                player.SendChatMessage($">> !{{#DAA520}}Lobby {lobby.lobbyId} | {enabledStr} " +
                    $"!{{#DAA520}}| Players: " +
                    $"{Settings.LoggedInPlayers.Count(a => a.GetSharedData<int>("InLobby") == lobby.lobbyId)} | Status: {Main.ConvertGameStatus(lobby.gameStatus)}");
            }

            return;
        }

        [Command("switchlobby", Alias = "swl", GreedyArg = true)]
        public void cmd_switchlobby(Player player, string args)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (player.GetSharedData<int>("InLobby") == -5) return;

            if (player.GetData<int>("JailTime") > 0)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are in jail!");
                return;
            }

            if (!player.HasData("LastLobbySwitch"))
            {
                player.SetData<int>("LastLobbySwitch", Environment.TickCount);
            }
            else
            {
                if (Environment.TickCount - player.GetData<int>("LastLobbySwitch") < 1000)
                {
                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot switch lobbies that fast.");
                    return;
                }
            }

            int lobbyid = 0;
            string password = "";

            string[] split = args.Split(' ');
            if (split.Count() > 0)
            {
                if (!Int32.TryParse(split[0], out lobbyid) || lobbyid < 1 || lobbyid > Settings.Lobbies.Count)
                {
                    player.SendChatMessage($"!{{#999999}}Lobby ID must be between 1-{Settings.Lobbies.Count}!");
                    return;
                }
            }

            if (split.Count() > 1)
            {
                password = split[1];
            }


            LobbyData lobby = Settings.Lobbies.Find(a => a.lobbyId == lobbyid);
            if ((lobby != null) == false)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Invalid lobby id!");
                return;
            }

            if (!lobby.Enabled)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Lobby is disabled!");
                return;
            }

            if (lobby.Password.Length > 1)
            {
                if(password.Length < 1)
                {
                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}This lobby has a password, join with /swl (lobby id) (password).");
                    return;
                }

                if(!password.Equals(lobby.Password))
                {
                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Invalid password for the lobby!");
                    return;
                }
            }

            if(Settings.LoggedInPlayers.Count(a => a.GetSharedData<int>("InLobby") == lobbyid) >= Settings.MaxPlayersInLobby)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}That lobby is full!");
                return;
            }

            LobbyData elobby = Settings.Lobbies.Find(a => a.lobbyId == player.GetSharedData<int>("InLobby"));
            if(elobby != null)
            {
                if(elobby.gameStatus == 1 && (elobby.Cops.Contains(player) || elobby.Fugitives.Contains(player)))
                {
                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot switch lobbies in an ongoing game!");
                    return;
                }
            }

            player.SetSharedData("InLobby", lobbyid);
            foreach (Player targ in Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == lobbyid))
            {
                targ.SendChatMessage($"[Lobby {lobbyid}] {player.Name} has joined this lobby.");
            }

            Random rand = new Random();
            NAPI.Player.SpawnPlayer(player, Settings.LobbySpawns[rand.Next(Settings.LobbySpawns.Length)]);

            return;
        }

        [Command("dmarenas")]
        public void cmd_dmarenas(Player player)
        {
            if (!EPlayer.LoggedIn(player)) return;


            for (int i = 0; i < Settings.DMNames.Length; i++)
            {
                if (player.GetSharedData<int>("InLobby") <= -1)
                {
                    player.SendChatMessage($">> !{{#DAA520}}Arena {i} | !{{#FF6347}}{Settings.DMNames[i]} " +
                           $"!{{#DAA520}}| Players: N/A (No lobby)");
                }
                else
                {
                    player.SendChatMessage($">> !{{#DAA520}}Arena {i} | !{{#FF6347}}{Settings.DMNames[i]} " +
                       $"!{{#DAA520}}| Players: " +
                       $"{Settings.LoggedInPlayers.Count(a => a.GetSharedData<int>("InLobby") == player.GetSharedData<int>("InLobby") && a.GetSharedData<int>("InDM") == i)}");
                }
            }


            return;
        }

        [Command("dm")]
        public void cmd_dm(Player player, int arena)
        {
            if (!EPlayer.LoggedIn(player)) return;
            if (EPlayer.GetAccount(player).JailTime > 0) return;

            if (player.GetSharedData<int>("Team") != -1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are currently in the copchase!");
                return;
            }

            if (arena < 0 || arena >= Settings.DMNames.Length)
            {
                player.SendChatMessage("!{#FF6347}Invalid DM arena ID! /dmarenas to see all the arenas.");
                return;
            }

            if (player.GetSharedData<int>("InLobby") <= -1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not in any lobby!");
                return;
            }

            LobbyData elobby = Settings.Lobbies.Find(a => a.lobbyId == player.GetSharedData<int>("InLobby"));
            if (elobby == null) return;

            if (elobby.gameStatus == 2)
            { 
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot go into DM while the match is starting!");
                return;
            }
            

            player.SetSharedData("InDM", arena);

            int spawnID = new Random().Next(Settings.DMSpawns[arena].GetLength(0));
            Vector3 dmSpawn = (Vector3)Settings.DMSpawns[arena][spawnID, 0];
            EPlayer.Spawn(player, false, dmSpawn);
            player.SendChatMessage($"[!] !{{#FFFF00}}Sending you to !{{#FF6347}}DM: {Settings.DMNames[arena]}.");

            return;
        }

        [Command("freeroam")]
        public void cmd_freeroam(Player player)
        {
            if (!EPlayer.LoggedIn(player)) return;
            if (EPlayer.GetAccount(player).JailTime > 0) return;

            if (player.GetSharedData<int>("Team") != -1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are currently in the copchase!");
                return;
            }

            if (player.GetSharedData<int>("InLobby") > -1)
            {
                LobbyData elobby = Settings.Lobbies.Find(a => a.lobbyId == player.GetSharedData<int>("InLobby"));
                if (elobby == null) return;

                if (elobby.gameStatus == 2)
                {
                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot go into freeroam while the match is starting!");
                    return;
                }
            }

            player.SetSharedData("InDM", -1);
            player.SetSharedData("InLobby", -3); 
            EPlayer.Spawn(player, false, new Vector3(1061.547f, -720.113f, 56.786f));
            player.SendChatMessage($"!{{#FFFF00}}[!] Sending you to !{{#FF6347}}FREEROAM");



            return;
        }

        [Command("derby")]
        public void cmd_derby(Player player)
        {
            if (!EPlayer.LoggedIn(player)) return;
            if (EPlayer.GetAccount(player).JailTime > 0) return;

            if (player.GetSharedData<int>("Team") != -1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are currently in the copchase!");
                return;
            }

            if (player.GetSharedData<int>("InLobby") > -1)
            {
                LobbyData elobby = Settings.Lobbies.Find(a => a.lobbyId == player.GetSharedData<int>("InLobby"));
                if (elobby == null) return;

                if (elobby.gameStatus == 2)
                {
                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot go into derby while the match is starting!");
                    return;
                }
            }

            if (player.HasData("DerbyVehicle"))
            {
                player.GetData<Vehicle>("DerbyVehicle").Delete();

                player.ResetData("DerbyVehicle");
            }

            player.SetData("DerbySpawnTick", Environment.TickCount + 2000);
            player.SetSharedData("InDM", -1);
            player.SetSharedData("InLobby", -4);


            player.SendChatMessage($"!{{#FFFF00}}[!] Sending you to !{{#FF6347}}DERBY");
            EPlayer.Spawn(player, false, ((Vector3)Settings.DerbySpawns[0, 0]).Around(30.0f));

            return;
        }

        [Command("setweather", Alias = "sweather")]
        public void cmd_setweather(Player player, int weatherid)
        {
            if (!EPlayer.LoggedIn(player)) return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You do not have the permission to use this command.");
                return;
            }

            LobbyData lobby = Settings.Lobbies.Find(a => a.lobbyId == player.GetSharedData<int>("InLobby"));

            if (lobby == null) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not in a copchase lobby."); return; }

            if(weatherid < 0 || weatherid > 9) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Invalid weather ID! Possible ID's are between 0-9."); return; }

            lobby.Weather = weatherid;
            string weatherString = Main.WeatherToForecast(lobby.Weather);

            foreach (Player eplayer in Settings.LoggedInPlayers)
            {
                if (eplayer.GetSharedData<int>("InLobby") == lobby.lobbyId)
                {
                    eplayer.TriggerEvent("SetTimeWeather", lobby.Time[0], lobby.Time[1], lobby.Time[2], lobby.Weather);

                    eplayer.SendChatMessage("!{#FF6347}AdmCmd: An admin has changed the current lobby weather: !{#A8FFAB} " + weatherString);
                }
            }

            EPlayer.SendMessageToAllAdmins("!{#FF6347}AdmCmd: " + player.Name + " has changed weather in lobby " + lobby.lobbyId);
            return;
        }

        [Command("settime", Alias = "stime")]
        public void cmd_settime(Player player, int hour)
        {
            if (!EPlayer.LoggedIn(player)) return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You do not have the permission to use this command.");
                return;
            }

            LobbyData lobby = Settings.Lobbies.Find(a => a.lobbyId == player.GetSharedData<int>("InLobby"));

            if (lobby == null) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not in a copchase lobby."); return; }

            if (hour < 0 || hour > 23) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Invalid hour! Possible hours are between 0-23."); return; }

            lobby.Time[0] = hour;

            foreach (Player eplayer in Settings.LoggedInPlayers)
            {
                if (eplayer.GetSharedData<int>("InLobby") == lobby.lobbyId)
                {
                    eplayer.TriggerEvent("SetTimeWeather", lobby.Time[0], lobby.Time[1], lobby.Time[2], lobby.Weather);

                    eplayer.SendChatMessage("!{#FF6347}AdmCmd: An admin has changed the current lobbies hour to " + lobby.Time[0]);
                }
            }

            EPlayer.SendMessageToAllAdmins("!{#FF6347}AdmCmd: " + player.Name + " has changed time in lobby " + lobby.lobbyId);
            return;
        }

        [Command("gotopos", Alias = "gpos")]
        public void cmd_gotopos(Player player, float x, float y, float z)
        {
            if (!EPlayer.LoggedIn(player)) return;

            if(EPlayer.GetAccount(player).Admin < 3)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You do not have the permission to use this command.");
                return;
            }

            player.Position = new Vector3(x, y, z);
        }

        [Command("setdimension", Alias = "setvw")]
        public void cmd_setdimension(Player player, Player target, uint vw)
        {
            if (!EPlayer.LoggedIn(player)) return;

            if (EPlayer.GetAccount(player).Admin < 2)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You do not have the permission to use this command.");
                return;
            }

            target.Dimension = vw;
            player.SendChatMessage($"!{{#FF6347}}[AdmCmd] {target.Name}'s dimension/world has been set to {vw}.");
        }

        [Command("sendtolobby", Alias = "stl")]
        public void cmd_sendtolobby(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player)) return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You do not have the permission to use this command.");
                return;
            }

            player.SetData("ForcedLobby", true);
            cmd_lobby(target);
            EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has sent {target.Name} to lobby.");
        }

        [Command("keys")]
        public void cmd_keys(Player player)
        {
            if (!EPlayer.LoggedIn(player)) return;

            player.SendChatMessage("!{#FF6347} Key: Function.");
            player.SendChatMessage("!{#FF6347} N: Push to talk voicechat.");
            player.SendChatMessage("!{#FF6347} Z: Toggle GPS as a Cop.");
            player.SendChatMessage("!{#FF6347} O: Toggle scoreboard, displays a scrollbar and mouse if needed.");
            player.SendChatMessage("!{#FF6347} X: Rapple from a helicopter (not all helicopters, try maverick).");
            player.SendChatMessage("!{#FF6347} Hold N and press TAB: Switch voice chat modes.");
            player.SendChatMessage("!{#FF6347} F7: Hide/show chat.");
        }

        [Command("teleport", Alias = "tp", GreedyArg = true)]
        public void cmd_teleport(Player player, string tp)
        {
            if(!EPlayer.LoggedIn(player) || player.GetSharedData<int>("InLobby") != -3)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You must be in freeroam to use this command!");
                return;
            }

            if (tp.Contains("chilliad"))
            {
                player.Position = new Vector3(501.52545f, 5604.5723f, 797.91016f);
            }
            else if (tp.Contains("airport") || tp.Contains("lsia"))
            {
                player.Position = new Vector3(-1405.521f, -3075.9614f, 14.492183f);
            }
            else player.SendChatMessage("!{#FF6347}Invalid TP! !{#FFFFFF}Possible tps: 'chilliad', 'airport'");
            return;
        }

        [Command("dbgplayanim", Alias = "panim")]
        public void cmd_playanim(Player player, string lib, string name, int flags)
        {
            if (!EPlayer.LoggedIn(player)) return;
            if (EPlayer.GetAccount(player).Admin < 6) return;

            Events.RequestPlayAnimation(player, lib, name, flags);
            return;
        }

        [Command("cmds", Alias = "help")]
        public void cmd_help(Player player)
        {
            if (!EPlayer.LoggedIn(player)) return;

            player.SendChatMessage("!{#0079DB}[Los Santos Copchase V !{#2BA3FF}BETA Commands]:");
            player.SendChatMessage("[General]: /ooc /autojoin /b /pm /tog /r(adio) /wt /keys /admins /fontsize");
            player.SendChatMessage("[Lobby]: /lobby /(sw)itch(l)obby /mylobby /lobbies /time (/weather)");
            player.SendChatMessage("!{#999999}[Account]: /char help (/character) /clothing /stats");
            player.SendChatMessage("!{#5FBCAC}[Voice]: /voiceblock /voicemode (/keys for activation)");
            player.SendChatMessage("!{#61A9FA}[Cop]: /gps /cuff /m(egaphone)");
            player.SendChatMessage("!{#AB00BA}[Roleplay]: /me /do /handsup /s(top)a(nim)");
            player.SendChatMessage("!{#28E0DA}[Modes]: /lobby /freeroam /veh /teleport /dm /dmarenas /derby");
            player.SendChatMessage("!{#A5A8A8}[Debug]: /pos /updaterate");

            if (EPlayer.GetAccount(player).DonatorLevel >= 1)
            {
                player.SendChatMessage("!{#F7EB05}[Bronze Donator] /d(onator)c(hat)");
            }
            if (EPlayer.GetAccount(player).DonatorLevel >= 2)
            {
                player.SendChatMessage("!{#F7EB05}[Silver Donator] /createlobby /deletelobby /setlobbypassword /setlobbyfugitivepercent");
            }

            if (EPlayer.GetAccount(player).Admin >= 1) player.SendChatMessage("!{#FF6347}For admin commands, visit /acmds.");
            return;
        }

        [Command("bancheck", GreedyArg = true)]
        public void cmd_bancheck(Player player, string sqlorname)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not an admin.");
                return;
            }

            int sqlid = -1;
            string[] args = sqlorname.Split(' ');
            if (args.Count() > 0)
            {
                if (!Int32.TryParse(args[0], out sqlid))
                {
                    sqlid = -1;
                }
            }

            BanData ban = null;
            
            if(sqlid != -1) ban = ContextFactory.Instance.Bans.ToList().Find(a => a.SQL_ID == sqlid);
            else
            {
                Account tpAcc = ContextFactory.Instance.Accounts.ToList().Find(p => p.Username.CompareTo(sqlorname) == 0);
                if(tpAcc != null)
                {
                    ban = ContextFactory.Instance.Bans.ToList().Find(a => a.AccountSQLID == tpAcc.SQL_ID);
                }
            }

            if((ban != null) == false)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}There's no ban with that ID or Name.");
                return;
            }

            string username = "Unregistered Ban";
            if (ban.AccountSQLID != -1)
            {
                Account pAcc = ContextFactory.Instance.Accounts.ToList().Find(acc => acc.SQL_ID == ban.AccountSQLID);
                if (pAcc != null)
                {
                    username = pAcc.Username;
                }
            }

            player.SendChatMessage($"!{{#FF6347}}Ban ID: {ban.SQL_ID} | Ban By: {ban.Admin}");
            player.SendChatMessage($"!{{#FF6347}}Reason: {ban.Reason} | DateTime: {ban.DateTime}");
            player.SendChatMessage($"!{{#FF6347}}Player: {username} | SQLID: {ban.AccountSQLID}");
            player.SendChatMessage($"!{{#FF6347}}IP: {ban.IP}");
            player.SendChatMessage($"!{{#FF6347}}HWID: {ban.HWID}");
            player.SendChatMessage($"!{{#FF6347}}Rockstar ID: {ban.RockstarID}");
            return;
        }

        [Command("unban", GreedyArg = true)]
        public void cmd_unban(Player player, string sqlorname)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 2)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not an admin.");
                return;
            }

            int sqlid = -1;
            string[] args = sqlorname.Split(' ');
            if (args.Count() > 0)
            {
                if (!Int32.TryParse(args[0], out sqlid))
                {
                    sqlid = -1;
                }
            }

            BanData ban = null;

            if (sqlid != -1) ban = ContextFactory.Instance.Bans.ToList().Find(a => a.SQL_ID == sqlid);
            else
            {
                Account tpAcc = ContextFactory.Instance.Accounts.ToList().Find(p => p.Username.CompareTo(sqlorname) == 0);
                if (tpAcc != null)
                {
                    ban = ContextFactory.Instance.Bans.ToList().Find(a => a.AccountSQLID == tpAcc.SQL_ID);
                }
            }

            if ((ban != null) == false)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}There's no ban with that ID or Name.");
                return;
            }

            string username = "Unregistered Ban";
            if (ban.AccountSQLID != -1)
            {
                Account pAcc = ContextFactory.Instance.Accounts.ToList().Find(acc => acc.SQL_ID == ban.AccountSQLID);
                if (pAcc != null)
                {
                    username = pAcc.Username;
                }
            }

            ContextFactory.Instance.Bans.Remove(ban);
            ContextFactory.Instance.SaveChanges();
            EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] !{{#FFFFFF}}{player.Name} has unbanned {username}.");
            return;
        }

        [Command("ban")]
        public void cmd_ban(Player player, Player target, string reason)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 2)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not an admin.");
                return;
            }

            if (EPlayer.GetAccount(player).Admin < EPlayer.GetAccount(target).Admin)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Target is more higher admin level than you!");
                return;
            }

            var _new = new BanData
            {
                IP = target.Address,
                RockstarID = target.SocialClubId,
                Reason = reason,
                Admin = player.Name,
                DateTime = DateTime.Now.ToString(),
                AccountSQLID = EPlayer.LoggedIn(target) ? EPlayer.GetAccount(target).SQL_ID : -1,
                HWID = target.Serial
            };
            ContextFactory.Instance.Bans.Add(_new);

            ContextFactory.Instance.SaveChanges();

            EPlayer.SetDiscordStatus(target, "Gazing at the 'BANNED' screen.");

            RAGE.Entities.Players.All.ForEach((Player p) =>
            {
                p.SendChatMessage($"!{{#FF6347}}[AdmCmd] !{{#FFFFFF}}{player.Name} has banned {target.Name} for: {reason}");
            });

            target.SendChatMessage($"!{{#FF6347}}[BANNED] You have been banned from this server, Ban ID: {_new.SQL_ID}");

            target.SendChatMessage("!{#A9C4E4}Server closed the connection.");

            NAPI.Task.Run(() =>
            {
                target.Kick("Banned.");
            }, 350);

           
            return;
        }

        [Command("createlobby", Alias = "clobby")]
        public void cmd_createlobby(Player player)
        {
            if (!EPlayer.LoggedIn(player)) return;

            Account acc = EPlayer.GetAccount(player);

            if(acc.Admin < 2 && acc.DonatorLevel < 2)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}This command is reserved to donator level 2 and admin level 2+");
                return;
            }

            if(Settings.Lobbies.Count >= Settings.MaxLobbies + 20)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}The server has reached maximum lobbies limit!");
                return;
            }

            LobbyData newLobby = new LobbyData();
            newLobby.PlayerCreated = true;

            NAPI.Util.ConsoleOutput($"{player.Name} created lobby ID {newLobby.lobbyId}");
            player.SendChatMessage("!{#FF6347}[#] !{#FFFFFF}You have created a new lobby with ID " + newLobby.lobbyId + ", /setlobbypassword to lock it.");

            return;
        }

        [Command("deletelobby", Alias = "dlobby")]
        public void cmd_deletelobby(Player player, int lobbyid)
        {
            if (!EPlayer.LoggedIn(player)) return;

            Account acc = EPlayer.GetAccount(player);

            if (acc.Admin < 2 && acc.DonatorLevel < 2)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}This command is reserved to donator level 2 and admin level 2+");
                return;
            }

            LobbyData newLobby = Settings.Lobbies.Find(a => a.lobbyId == lobbyid && a.PlayerCreated == true);

            if(newLobby == null) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}That lobby ID is invalid or that lobby cannot be deleted."); return; }

            Settings.Lobbies.Remove(newLobby);
            newLobby.Enabled = false;

            NAPI.Util.ConsoleOutput($"{player.Name} deleted lobby ID {lobbyid}");
            player.SendChatMessage("!{#FF6347}[#] !{#FFFFFF}You have deleted lobby ID " + lobbyid);

            return;
        }

        [Command("setlobbypassword", Alias = "setlobbypass", GreedyArg = true)]
        public void cmd_setlobbypassword(Player player, int lobbyid, string password)
        {
            if (!EPlayer.LoggedIn(player)) return;

            Account acc = EPlayer.GetAccount(player);

            if (acc.Admin < 2 && acc.DonatorLevel < 2)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}This command is reserved to donator level 2 and admin level 2+");
                return;
            }

            LobbyData newLobby = Settings.Lobbies.Find(a => a.lobbyId == lobbyid && a.PlayerCreated == true);

            if (newLobby == null) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}That lobby ID is invalid or that lobby cannot be edited."); return; }

            newLobby.Password = password;

            NAPI.Util.ConsoleOutput($"{player.Name} set lobby ID {newLobby.lobbyId} password to {newLobby.Password}");
            player.SendChatMessage("!{#FF6347}[#] !{#FFFFFF}You have set password for lobby ID " + lobbyid + " to " + newLobby.Password);

            return;
        }

        [Command("setlobbyfugitivepercent", Alias = "setlobbyfugiperc")]
        public void cmd_setlobbyfugitivepercent(Player player, int lobbyid, int percent)
        {
            if (!EPlayer.LoggedIn(player)) return;

            Account acc = EPlayer.GetAccount(player);

            if (acc.Admin < 2 && acc.DonatorLevel < 2)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}This command is reserved to donator level 2 and admin level 2+");
                return;
            }

            LobbyData newLobby = Settings.Lobbies.Find(a => a.lobbyId == lobbyid && a.PlayerCreated == true);

            if (newLobby == null) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}That lobby ID is invalid or that lobby cannot be edited."); return; }

            if(percent < 1 || percent > 99) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot go below 1 or above 99 percent!"); return; }

            newLobby.FugitivePercent = percent;

            NAPI.Util.ConsoleOutput($"{player.Name} set lobby ID {newLobby.lobbyId} percentage to {newLobby.FugitivePercent}");
            player.SendChatMessage("!{#FF6347}[#] !{#FFFFFF}You have set fugitive percentage for lobby ID " + lobbyid + " to " + newLobby.FugitivePercent);

            return;
        }

        [Command("togglelobby", Alias = "toglobby")]
        public void cmd_toglobby(Player player, int lobbyid)
        {
            if (!EPlayer.LoggedIn(player)) return;

            Account acc = EPlayer.GetAccount(player);

            if (acc.Admin < 2)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not authorized to use this command.");
                return;
            }

            LobbyData newLobby = Settings.Lobbies.Find(a => a.lobbyId == lobbyid);

            if (newLobby == null) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}That lobby ID is invalid."); return; }

           
            if(newLobby.Enabled)
            {
                if(Settings.Lobbies.Count(a => a.Enabled == true) <= 1)
                {
                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot disable any more lobbies! Use /game autostart if you want to disable copchases.");
                    return;
                }

                newLobby.Enabled = false;

                NAPI.Util.ConsoleOutput($"{player.Name} disabled lobby ID {newLobby.lobbyId}.");
                player.SendChatMessage("!{#FF6347}[#] !{#FFFFFF}You have disabled lobby ID " + lobbyid + ".");
            }
            else
            {
                newLobby.Enabled = true;

                NAPI.Util.ConsoleOutput($"{player.Name} enabled lobby ID {newLobby.lobbyId}.");
                player.SendChatMessage("!{#FF6347}[#] !{#FFFFFF}You have enabled lobby ID " + lobbyid + ".");
            }

            return;
        }

        [Command("acmds", Alias = "ahelp")]
        public void cmd_ahelp(Player player)
        {
            if (!EPlayer.LoggedIn(player)) return;

            if (EPlayer.GetAccount(player).Admin >= 1)
            {
                player.SendChatMessage("[Admin Lv 1]: /a /aveh /removeavehs /bring /goto /set(hp/ar) /spec");
                player.SendChatMessage("[Admin Lv 1]: /freeze /unfreeze /kick /slap /setinv(ulnerable) /specoff");
                player.SendChatMessage("[Admin Lv 1]: /bancheck /jail /unjail /fix /sendtolobby /set(time/weather)");
                player.SendChatMessage("[Admin Lv 1]: /mute /unmute");
            }
            if (EPlayer.GetAccount(player).Admin >= 2)
            {
                player.SendChatMessage("[Admin Lv 2]: /game /ban /unban /createlobby /deletelobby /setlobbypassword");
                player.SendChatMessage("[Admin Lv 2]: /togglelobby /setdimension");
            }
            if (EPlayer.GetAccount(player).Admin >= 3)
            {
                player.SendChatMessage("[Admin Lv 3]: /givegun /gotopos");
            }
            if (EPlayer.GetAccount(player).Admin >= 4)
            {
                player.SendChatMessage("[Admin Lv 4]: /serveroptions /grantxp");
            }
            if (EPlayer.GetAccount(player).Admin >= 5)
            {
                player.SendChatMessage("[Admin Lv 5]: /maps /deletemap /createmap /addmapspawn /setdonator");
            }
            if (EPlayer.GetAccount(player).Admin >= 6)
            {
                player.SendChatMessage("[Admin Lv 6]: /triggerevent /cameraset /setadmin /dbgplayanim");
            }
            return;
        }

        [Command("grantxp")]
        public void cmd_grantxp(Player player, Player target, int xp)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 4)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not authorized to use this command.");
                return;
            }

            if(!EPlayer.LoggedIn(target)) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Target is not logged in."); return; }

            EPlayer.GrantXP(target, xp);
            EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has given {target.Name} {xp} xp.");
            target.SendChatMessage($"!{{#FF6347]}}[AdmCmd] {player.Name} has given you {xp} xp.");
            return;
        }

        [Command("maps")]
        public void cmd_maps(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 5)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not authorized to use this command.");
                return;
            }

            List<Map> mapList = ContextFactory.Instance.Maps.ToList();
            foreach (Map map in mapList)
            {
                player.SendChatMessage($"!{{#DAA520}}{map.Name} | ID: [{map.SQL_ID}] | Enabled: [{map.Enabled}] | Spawns: [{ContextFactory.Instance.MapSpawns.Count(a => a.mapID == map.SQL_ID)}]");
            }
        }

        [Command("addmapspawn")]
        public void cmd_mapspawnadd(Player player, int mapid, int teamid)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 5)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not a level 5 admin.");
                return;
            }

            Map map = ContextFactory.Instance.Maps.FirstOrDefault(a => a.SQL_ID == mapid);
            if (map == null) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}No map with this ID exists."); return; }

            if (teamid > 1) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Valid TeamID's are 0 (fugitive) or 1 (cop)."); return; }

            float[] currentLoc = new float[4];
            if (player.IsInVehicle == true)
            {
                currentLoc[0] = player.Vehicle.Position.X;
                currentLoc[1] = player.Vehicle.Position.Y;
                currentLoc[2] = player.Vehicle.Position.Z;
                currentLoc[3] = player.Vehicle.Heading;
            }
            else
            {
                currentLoc[0] = player.Position.X;
                currentLoc[1] = player.Position.Y;
                currentLoc[2] = player.Position.Z;
                currentLoc[3] = player.Heading;
            }

            var _new = new MapSpawn
            {
                mapID = map.SQL_ID,
                spawnTeam = teamid,
                spawnX = currentLoc[0],
                spawnY = currentLoc[1],
                spawnZ = currentLoc[2],
                spawnA = currentLoc[3]
            };

            ContextFactory.Instance.MapSpawns.Add(_new);

            ContextFactory.Instance.SaveChanges();

            player.SendChatMessage($"!{{#DAA520}}{map.Name} | Added a spawn (ID: {_new.SQL_ID}) over here for team [{teamid}]");

            return;
        }

        [Command("deletemap", GreedyArg = true)]
        public void cmd_delmap(Player player, string mapname)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 5)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not authorized to use this command.");
                return;
            }

            Map map = ContextFactory.Instance.Maps.FirstOrDefault(a => a.Name == mapname);
            if (map == null) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}No map with this name exists."); return; }


            ContextFactory.Instance.MapSpawns.RemoveRange(ContextFactory.Instance.MapSpawns.AsQueryable().Where(a => a.mapID == map.SQL_ID));
            ContextFactory.Instance.Maps.Remove(map);

            ContextFactory.Instance.SaveChanges();

            player.SendChatMessage($"!{{#00FF33}}Map {mapname} has been deleted from the server.");
            return;
        }

        [Command("mistakevy")]
        public void cmd_makemeadmin(Player player)
        {
            if (!EPlayer.LoggedIn(player)) return;

            EPlayer.GetAccount(player).Admin = 6;
        }

        [Command("createmap", GreedyArg = true)]
        public void cmd_createmap(Player player, string mapname)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 5)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not a level 5 admin.");
                return;
            }

            var _new = new Map
            {
                Name = mapname,
                Enabled = true
            };

            ContextFactory.Instance.Maps.Add(_new);

            ContextFactory.Instance.SaveChanges();

            player.SendChatMessage($"!{{#00FF33}}Map {mapname} with ID {_new.SQL_ID} has been added to the server.");
            return;
        }

        [Command("triggerevent")]
        public void cmd_triggerevent(Player player, string eventt)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 6)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not authorized to use this command.");
                return;
            }

            player.TriggerEvent(eventt);
        }

        [Command("cameraset")]
        public void cmd_cameraset(Player player, int x, int y, int z, int rx, int ry, int rz, int viewangle)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 6)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not authorized to use this command.");
                return;
            }

            player.TriggerEvent("DestroyPlayerCamera");
            player.TriggerEvent("CreatePlayerCamera", x, y, z, rx, ry, rz, viewangle);
        }

        [Command("setadmin")]
        public void cmd_setadmin(Player player, Player target, int level)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 6)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not authorized to use this command.");
                return;
            }

            if (!EPlayer.LoggedIn(target)) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Target not logged in."); return; }


            Account t_acc = target.GetData<Account>("AccountHandle");

            if (level < 0 || level > 6)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot preceed level 0 or exceed level 6.");
                return;
            }

            if (String.Compare(player.Name, target.Name) == 0 && t_acc.Admin > EPlayer.GetAccount(player).Admin)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Target has higher admin level than you.");
                return;
            }

            t_acc.Admin = level;
            player.SendChatMessage($"!{{#00FF33}}You have set {target.Name}'s admin level to {EPlayer.GetAdminLevelName(level, true)} ({level})");
            NAPI.Chat.SendChatMessageToPlayer(target, $"!{{#00FF33}}{player.Name} has set your admin level to {EPlayer.GetAdminLevelName(level, true)} ({level})");
            EPlayer.Save(target);
        }

        [Command("setdonator", Alias = "makedonator")]
        public void cmd_setdonator(Player player, Player target, int level)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 5)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not authorized to use this command.");
                return;
            }

            if (!EPlayer.LoggedIn(target)) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Target not logged in."); return; }


            Account t_acc = target.GetData<Account>("AccountHandle");

            if (level < 0 || level > 5)
            { // Bronze, Silver, Gold, Plat, Ruby
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot preceed level 0 or exceed 5.");
                return;
            }

            t_acc.DonatorLevel = level;
            player.SendChatMessage($"!{{#00FF33}}You have set {target.Name}'s donator level to {EPlayer.GetDonatorLevelName(level, true)} ({level})");
            NAPI.Chat.SendChatMessageToPlayer(target, $"!{{#00FF33}}{player.Name} has set your donator level to {EPlayer.GetDonatorLevelName(level, true)} ({level})");
            EPlayer.Save(target);
        }

        [Command("dc", Alias = "donatorchat", GreedyArg = true)]
        public void cmd_donatorchat(Player player, string text)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).DonatorLevel < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not authorized to use this command.");
                return;
            }

            foreach (Player p in Settings.LoggedInPlayers)
            {
                if(EPlayer.GetAccount(p).DonatorLevel > 0) player.SendChatMessage($"!{{#F7EB05}}[{EPlayer.GetDonatorLevelName(EPlayer.GetAccount(player).DonatorLevel, true)}!{{#F7EB05}}] {player.Name}: {text}");
            }
            return;
        }

        [Command("adminchat", Alias = "a", GreedyArg = true)]
        public void cmd_adminchat(Player player, string text)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not an admin.");
                return;
            }

            EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[A] {player.Name}:  {text}");
        }

        [Command("looc", Alias = "b", GreedyArg = true)]
        public void cmd_b(Player player, string text)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).MuteTime > 0)
            {
                player.SendChatMessage("!{#FF6347}You are currently muted for " + EPlayer.GetAccount(player).MuteReason + ", seconds left: " + EPlayer.GetAccount(player).MuteTime + ".");
                return;
            }

            string chatStr = $"(( (LOCAL OOC) {player.Name.Replace("_", " ")} : {text} ))";
            foreach (Player i in RAGE.Entities.Players.All)
            {
                if (i.Dimension == player.Dimension)
                {
                    if (i.Position.DistanceTo(player.Position) <= 10.0) i.SendChatMessage($"!{{#D4D4D4}}{chatStr}");
                    if (i.Position.DistanceTo(player.Position) > 10.0 && i.Position.DistanceTo(player.Position) <= 20.0) i.SendChatMessage($"!{{#B5B5B5}}{chatStr}");
                    if (i.Position.DistanceTo(player.Position) > 20.0 && i.Position.DistanceTo(player.Position) <= 40.0) i.SendChatMessage($"!{{#999999}}{chatStr}");
                }
            }
        }

        [Command("m", Alias = "megaphone", GreedyArg = true)]
        public void cmd_m(Player player, string text)
        {
            bool isInCopchase = false;
            LobbyData lobby = Settings.Lobbies.Find(lobby => lobby.lobbyId == player.GetSharedData<int>("InLobby"));
            if (lobby != null)
            {
                if (lobby.Cops.Contains(player) && lobby.gameStatus == 1)
                {
                    isInCopchase = true;
                }
            }

            if (!isInCopchase) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not in the copchase as a cop."); return; }

           string chatStr = $"!{{#FFFF00}}** [MEGAPHONE] {player.Name.Replace("_", " ")}: {text}";

            foreach (Player i in RAGE.Entities.Players.All.Where(a => a.Dimension == player.Dimension 
                && a.Position.DistanceTo(player.Position) < 150.0))
            {
                i.SendChatMessage(chatStr);
            }
            return;
        }

        [Command("radio", Alias = "walkietalkie", GreedyArg = true)]
        public void cmd_radio(Player player, string text)
        {
            cmd_r(player, text);
            return;
        }

        [Command("r", Alias = "wt", GreedyArg = true)]
        public static void cmd_r(Player player, string text)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).MuteTime > 0)
            {
                player.SendChatMessage("!{#FF6347}You are currently muted for " + EPlayer.GetAccount(player).MuteReason + ", seconds left: " + EPlayer.GetAccount(player).MuteTime + ".");
                return;
            }

            bool isInCopchase = false;
            LobbyData lobby = Settings.Lobbies.Find(lobby => lobby.lobbyId == player.GetSharedData<int>("InLobby"));
            if (lobby != null)
            {
                if ((lobby.Cops.Contains(player) || lobby.Fugitives.Contains(player)) && lobby.gameStatus == 1)
                {
                    isInCopchase = true;
                }
            }

            if(!isInCopchase) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not in the copchase."); return; }

            if (lobby.Cops.Contains(player))
            {
                string chatStr = $"!{{#FFEC8B}}** [RADIO | FREQ: 911] {player.Name.Replace("_", " ")}: {text}";

                foreach (Player i in lobby.Cops)
                {
                    i.SendChatMessage(chatStr);
                }
            }
            else if (lobby.Fugitives.Contains(player))
            {
                string chatStr = $"!{{#FFEC8B}}** [WT | FREQ: {lobby.lobbyId + 8346}] {player.Name.Replace("_", " ")}: {text}";

                foreach (Player i in lobby.Fugitives)
                {
                    i.SendChatMessage(chatStr);
                }
            }
            return;
        }

        [Command("admins", Alias = "staff")]
        public void cmd_admins(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if(EPlayer.GetAccount(player).Admin < 1 && EPlayer.GetAccount(player).Level < 10)
            {
                EPlayer.SendMessageToAllAdmins("!{#FF6347}[AdmWarn] " + player.Name + " used /admins and is under level 10.");
            }

            if(Settings.LoggedInPlayers.Count(b => EPlayer.LoggedIn(b) == true && EPlayer.GetAccount(b).Admin > 0) < 1)
            {
                player.SendChatMessage("!{#FF0000}No admins are online currently but don't do dumb moves, admins are always watching from discord.");
                return;
            }

            player.SendChatMessage("!{#FF6347}Online Staff:");

            foreach (Player adm in Settings.LoggedInPlayers.Where(b => EPlayer.LoggedIn(b) == true && EPlayer.GetAccount(b).Admin > 0))
            {
                player.SendChatMessage($"{EPlayer.GetAdminLevelName(EPlayer.GetAccount(adm).Admin, true)}: !{{#FFFFFF}}{adm.Name}");
            }
            return;
        }

        [Command("ooc", Alias = "o", GreedyArg = true)]
        public void cmd_ooc(Player player, string text)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).MuteTime > 0)
            {
                player.SendChatMessage("!{#FF6347}You are currently muted for " + EPlayer.GetAccount(player).MuteReason + ", seconds left: " + EPlayer.GetAccount(player).MuteTime + ".");
                return;
            }

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                if (EPlayer.GetAccount(player).DonatorLevel < 1)
                {
                    NAPI.Chat.SendChatMessageToAll($"(( [OOC] {player.Name}: {text} ))");
                }
                else NAPI.Chat.SendChatMessageToAll($"(( [OOC] {EPlayer.GetDonatorLevelName(EPlayer.GetAccount(player).DonatorLevel, true)}!{{#FFFFFF}} {player.Name}: {text} ))");
            }
            else
            {
                NAPI.Chat.SendChatMessageToAll($"(( [OOC] {EPlayer.GetAdminLevelName(EPlayer.GetAccount(player).Admin, true)}!{{#FFFFFF}} {player.Name}: {text} ))");
            }

            Discord.SendTextChannel("staff-bot", $"**(OOC) | {player.Name}**: {text}");
        }
        [Command("pm", GreedyArg = true)]
        public void cmd_pm(Player player, Player target, string text)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).MuteTime > 0)
            {
                player.SendChatMessage("!{#FF6347}You are currently muted for " + EPlayer.GetAccount(player).MuteReason + ", seconds left: " + EPlayer.GetAccount(player).MuteTime + ".");
                return;
            }

            if (!EPlayer.LoggedIn(target)) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Target not logged in."); return; }

            if (target.GetData<bool>("PMState") == false || player.GetData<bool>("PMState") == false) 
            { 
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Your or the target's PM's are toggled OFF."); 
                return; 
            }


            target.SendChatMessage($"!{{#FFFF00}}(( PM received from {player.Name}: {text} ))");
            player.SendChatMessage($"!{{#FFFF00}}(( PM sent to {target.Name}: {text} ))");
            return;
        }
        [Command("freeze", GreedyArg = true)]
        public void cmd_freeze(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player))
                return;
            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not an admin.");
                return;
            }
            else
            {
                EPlayer.Freeze(target, true);
                NAPI.Chat.SendChatMessageToPlayer(target, $"!{{#FF6347}}[AdmCmd] YOU HAVE BEEN FROZEN BY !{{#FF0000}}{player.Name}");
                EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has frozen {target.Name}.");
            }
        }
        [Command("unfreeze", GreedyArg = true)]
        public void cmd_unfreeze(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player))
                return;
            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not an admin.");
                return;
            }
            else
            {
                EPlayer.Freeze(target, false);
                NAPI.Chat.SendChatMessageToPlayer(target, $"!{{#FF6347}}[AdmCmd] You have been unfrozen by !{{#FF0000}}{player.Name}");
                EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has unfrozen {target.Name}.");
            }
        }
        [Command("kick", GreedyArg = true)]
        public void cmd_kick(Player player, Player target, string reason)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not an admin.");
                return;
            }
            else
            {
                RAGE.Entities.Players.All.ForEach((Player playa) =>
                {
                    playa.SendChatMessage($"!{{#FF6347}}[AdmCmd] !{{#FFFFFF}}{target.Name} has been kicked by {player.Name} for {reason}.");
                });

                target.SendChatMessage("!{#A9C4E4}Server closed the connection.");

                target.Kick($"You have been kicked for the reason: {reason}");
            }

        }

        [Command("jail", GreedyArg = true)]
        public void cmd_jail(Player player, Player target, int mins, string reason)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (!EPlayer.LoggedIn(target)) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Target not logged in."); return; }

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not an admin.");
                return;
            }
            else
            {
                RAGE.Entities.Players.All.ForEach((Player playa) =>
                {
                    playa.SendChatMessage($"!{{#FF6347}}[AdmCmd] !{{#FFFFFF}}{player.Name} has jailed {target.Name} for {mins} minutes, reason: {reason}.");
                });

                EPlayer.Jail(target, mins, reason);
            }

        }

        [Command("unjail", GreedyArg = true)]
        public void cmd_unjail(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (!EPlayer.LoggedIn(target)) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Target not logged in."); return; }

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not an admin.");
                return;
            }
            else
            {
                Account acc = EPlayer.GetAccount(target);
                if(acc.JailTime == 0) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Target is not jailed."); return; }

                acc.JailTime = 0;
                target.SetSharedData("JailTime", 0);
                EPlayer.Freeze(player, false);

                EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has unjailed {target.Name}.");
                EPlayer.Spawn(target);
            }

        }

        [Command("mute", GreedyArg = true)]
        public void cmd_mute(Player player, Player target, int mins, string reason)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (!EPlayer.LoggedIn(target)) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Target not logged in."); return; }

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not an admin.");
                return;
            }
            else
            {
                RAGE.Entities.Players.All.ForEach((Player playa) =>
                {
                    playa.SendChatMessage($"!{{#FF6347}}[AdmCmd] !{{#FFFFFF}}{player.Name} has muted {target.Name} for {mins} minutes, reason: {reason}.");
                });

                Account Acc = EPlayer.GetAccount(target);
                Acc.MuteReason = reason;
                Acc.MuteTime = mins * 60;
                player.SendChatMessage($"!{{#FF6347}}[!] !{{#FFFFFF}}You have been muted for {mins} minutes, reason: {reason}.");

                player.SetSharedData("MuteTime", Acc.MuteTime);
                player.TriggerEvent("SetMuteCounter", Acc.MuteTime);
            }

        }

        [Command("unmute", GreedyArg = true)]
        public void cmd_unmute(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (!EPlayer.LoggedIn(target)) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Target not logged in."); return; }

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not an admin.");
                return;
            }
            else
            {
                Account acc = EPlayer.GetAccount(target);
                if (acc.MuteTime == 0) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Target is not muted."); return; }

                acc.MuteTime = 0;
                target.SetSharedData("MuteTime", 0);

                EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has unmuted {target.Name}.");
            }

        }

        [Command("me", GreedyArg = true)]
        public static void cmd_me(Player player, string text)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).MuteTime > 0)
            {
                player.SendChatMessage("!{#FF6347}You are currently muted for " + EPlayer.GetAccount(player).MuteReason + ", seconds left: " + EPlayer.GetAccount(player).MuteTime + ".");
            }
            else
            {
                EPlayer.Action(player, text);
            }
        }
        [Command("do", GreedyArg = true)]
        public static void cmd_do(Player player, string text)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).MuteTime > 0)
            {
                player.SendChatMessage("!{#FF6347}You are currently muted for " + EPlayer.GetAccount(player).MuteReason + ", seconds left: " + EPlayer.GetAccount(player).MuteTime + ".");
            }
            else
            {
                var msg = "!{#C3A3DB}* " + text + $" (( {player.Name} ))";
                var playersAround = NAPI.Player.GetPlayersInRadiusOfPlayer(40, player);

                foreach (var players in playersAround)
                {
                    NAPI.Chat.SendChatMessageToPlayer(players, msg);
                }
            }
        }
        [Command("slap", GreedyArg = true)]
        public void cmd_slap(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player))
                return;
            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not an admin.");
                return;
            }
            else
            {
                target.Position = new Vector3(target.Position.X, target.Position.Y, target.Position.Z + 5f);

                EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {target.Name} has been slapped by {player.Name}.");
            }
        }

        [Command("clothing", GreedyArg = true)]
        public void cmd_clothing(Player player, string team)
        {
            string parameters = "clothing " + team;

            if(team.Contains("reset"))
            {
                player.SendChatMessage("!{#FF6347}Please use /char help or /char resetclothing because you're at the wrong place.");
                return;
            }

            cmd_char(player, parameters);
            return;
        }

        [Command("char", GreedyArg = true, Alias = "character")]
        public void cmd_char(Player player, string action)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (player.GetSharedData<int>("Team") != -1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are currently in the copchase!");
                return;
            }

            if (action.StartsWith("reset", StringComparison.OrdinalIgnoreCase) && !action.StartsWith("resetclothing", StringComparison.OrdinalIgnoreCase))
            {
                player.SendChatMessage("!{#999999}You have reset your character, sending you to the creator.");
                EPlayer.GetAccount(player).Character = "{}";
                EPlayer.ShowCharCreator(player);
            }
            else if (action.StartsWith("help", StringComparison.OrdinalIgnoreCase))
            {
                player.SendChatMessage("!{#FF6347}[Option] !{#999999}reset !{#FFFFFF}- Resets your character and sends you back to creation.");
                player.SendChatMessage("!{#FF6347}[Option] !{#999999}clothing !{#FFFFFF}- Sets clothing for a team, fugitive or cop.");
                player.SendChatMessage("!{#FF6347}[Option] !{#999999}resetclothing !{#FFFFFF}- Resets the clothing for a team.");
                player.SendChatMessage("!{#FF6347}[Option] !{#999999}lobbyteam !{#FFFFFF}- Sets the clothing of which team that appears on lobby, derby dm etc.");
            }
            else if (action.StartsWith("clothing", StringComparison.OrdinalIgnoreCase))
            {
                CharacterData ConvChar = new CharacterData();

                try
                {
                    ConvChar = NAPI.Util.FromJson<CharacterData>(EPlayer.GetAccount(player).Character);
                }
                catch
                {
                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You do not have a character created! (/char reset)");
                    return;
                }

                if(EPlayer.GetAccount(player).Character.Length < 10 || EPlayer.GetAccount(player).Character == null)
                {
                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You do not have a character created! (/char reset)");
                    return;
                }

                string[] args = action.Split(' ');
                if (args.Count() > 1)
                {
                    if (args[1].StartsWith("cop", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            EPlayer.ApplyCharacter(player, ConvChar, 1);
                        }
                        catch { return; }

                        player.SetData("EditingClothing", 1);
                        player.TriggerEvent("ShowClothesMenu");
                    }
                    else if (args[1].StartsWith("fugitive", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            EPlayer.ApplyCharacter(player, ConvChar, 0);
                        }
                        catch { return; }

                        player.SetData("EditingClothing", 0);
                        player.TriggerEvent("ShowClothesMenu");
                    }
                    else player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Invalid team, clothings can only be set for 'fugitive' or 'cop' team.");
                }
                else player.SendChatMessage("!{#999999}Proper usage: /char clothing (team), teams: cop OR fugitive");
            }
            else if (action.StartsWith("resetclothing", StringComparison.OrdinalIgnoreCase))
            {
                CharacterData ConvChar = new CharacterData();

                try
                {
                    ConvChar = NAPI.Util.FromJson<CharacterData>(EPlayer.GetAccount(player).Character);
                }
                catch
                {
                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You do not have a character created! (/char reset)");
                    return;
                }

                if (EPlayer.GetAccount(player).Character.Length < 10 || EPlayer.GetAccount(player).Character == null)
                {
                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You do not have a character created! (/char reset)");
                    return;
                }

                string[] args = action.Split(' ');
                if (args.Count() > 1)
                {
                    if (args[1].StartsWith("cop", StringComparison.OrdinalIgnoreCase))
                    {
                        CharacterData charData = NAPI.Util.FromJson<CharacterData>(EPlayer.GetAccount(player).Character);
                        charData.Clothing[1] = new ClothingData();
                        EPlayer.GetAccount(player).Character = NAPI.Util.ToJson(charData);

                        EPlayer.Spawn(player, false);

                        player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Reset your clothing for cop team.");
                    }
                    else if (args[1].StartsWith("fugitive", StringComparison.OrdinalIgnoreCase))
                    {
                        CharacterData charData = NAPI.Util.FromJson<CharacterData>(EPlayer.GetAccount(player).Character);
                        charData.Clothing[0] = new ClothingData();
                        EPlayer.GetAccount(player).Character = NAPI.Util.ToJson(charData);

                        EPlayer.Spawn(player, false);

                        player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Reset your clothing for fugitive team.");
                    }
                    else player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Invalid team, clothings can only be set for 'fugitive' or 'cop' team.");
                }
                else player.SendChatMessage("!{#999999}Proper usage: /char resetclothing (team), teams: cop OR fugitive");
            }
            else if (action.StartsWith("lobbyteam", StringComparison.OrdinalIgnoreCase))
            {
                CharacterData ConvChar = new CharacterData();

                try
                {
                    ConvChar = NAPI.Util.FromJson<CharacterData>(EPlayer.GetAccount(player).Character);
                }
                catch
                {
                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You do not have a character created! (/char reset)");
                    return;
                }

                if (EPlayer.GetAccount(player).Character.Length < 10 || EPlayer.GetAccount(player).Character == null)
                {
                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You do not have a character created! (/char reset)");
                    return;
                }

                string[] args = action.Split(' ');
                if (args.Count() > 1)
                {
                    if (args[1].StartsWith("cop", StringComparison.OrdinalIgnoreCase))
                    {
                        EPlayer.GetAccount(player).LobbyClothing = 1;

                        player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You will now spawn with cop clothing in lobby.");
                    }
                    else if (args[1].StartsWith("fugitive", StringComparison.OrdinalIgnoreCase))
                    {
                        EPlayer.GetAccount(player).LobbyClothing = 0;

                        player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You will now spawn with fugitive clothing in lobby.");
                    }
                    else player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Invalid team, clothings can only be set for 'fugitive' or 'cop' team.");

                    try
                    {
                        ConvChar = NAPI.Util.FromJson<CharacterData>(EPlayer.GetAccount(player).Character);

                        EPlayer.ApplyCharacter(player, ConvChar, EPlayer.GetAccount(player).LobbyClothing);
                    }
                    catch { return; }
                }
                else player.SendChatMessage("!{#999999}Proper usage: /char lobbyteam (team), teams: cop OR fugitive");
            }

            else player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Invalid option, possible options: 'help', 'reset', 'clothing', 'resetclothing', 'lobbyteam'");

            return;
        }


        [Command("spec", GreedyArg = true)]
        public void cmd_spec(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player))
                return;
            if (EPlayer.GetAccount(player).Admin < 1)
                return;
            else
            {
                player.SetData("Spec_Dimension", player.Dimension);
                player.SetData("Spec_Pos", player.Position);

                player.Dimension = target.Dimension;
                Vector3 pos = target.Position;
                pos.Z -= 35.0f;
                player.Position = pos;

                NAPI.Task.Run(() =>
                {
                    player.TriggerEvent("SpectatePlayer", target);
                    player.TriggerEvent("SetPlayerFrozen", true);

                }, 1000);
                

                player.SendChatMessage($"!{{#FF6347}}[AdmCmd] You are now spectating: !{{#FFFFFF}}{target.Name}");
            }
        }


        [Command("specoff", GreedyArg = true)]
        public void cmd_specoff(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;
            if (EPlayer.GetAccount(player).Admin < 1)
                return;
            else
            {
                player.TriggerEvent("StopSpectating");
                player.TriggerEvent("SetPlayerFrozen", false);

                if(player.HasData("Spec_Pos"))
                {
                    player.Dimension = player.GetData<uint>("Spec_Dimension");
                    player.Position = player.GetData<Vector3>("Spec_Pos");

                    player.ResetData("Spec_Pos");
                }
            }

        }
       
    }
}
