using GTANetworkAPI;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace LSCC
{
    class Commands : Script
    {
        [Command("serveroptions", "!{#AFAFAF}[USAGE] serveroptions (option: maxinlobby, derbyveh, usediscord, onfootcollisions)", GreedyArg = true)]
        public void cmd_serveroptions(Player player, string action)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 4)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:serveroptions:{action.Replace(":", "|")}"); } catch { }

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

                    Settings.Dynamics.MaxInLobby = value;

                    Main.SaveDynamicSettings();

                    NAPI.Chat.SendChatMessageToAll($"!{{#FF6347}}[AdmCmd] !{{#FFFFFF}}{player.Name} has changed max players in a lobby to {value}.");
                }
                else player.SendChatMessage("!{#999999}Proper usage: /serveroptions maxinlobby <amount>");
            }

            else if (action.StartsWith("derbyveh", StringComparison.OrdinalIgnoreCase))
            {
                string[] args = action.Split(' ');
                if (args.Count() > 1)
                {
                    Settings.Dynamics.DerbyVehicle = args[1];

                    Main.SaveDynamicSettings();

                    NAPI.Chat.SendChatMessageToAll($"!{{#FF6347}}[AdmCmd] !{{#FFFFFF}}{player.Name} has set derby vehicle model to {Settings.Dynamics.DerbyVehicle}.");
                }
                else player.SendChatMessage("!{#999999}Proper usage: /serveroptions derbyveh <model name>");
            }

            else if (action.StartsWith("onfootcollisions", StringComparison.OrdinalIgnoreCase))
            {
                string[] args = action.Split(' ');
                if (args.Count() > 1)
                {
                    bool value = false;

                    if (!Boolean.TryParse(args[1], out value))
                    {
                        player.SendChatMessage($"!{{#999999}}OnfootCollisions can only be either 'true' or 'false'!");
                        return;
                    }

                    Settings.Dynamics.OnfootCollision = value;

                    Main.SaveDynamicSettings();

                    NAPI.Chat.SendChatMessageToAll($"!{{#FF6347}}[AdmCmd] !{{#FFFFFF}}{player.Name} has set onfoot collisions to {value.ToString()}.");
                }
                else player.SendChatMessage("!{#999999}Proper usage: /serveroptions onfootcollisions <true/false>");
            }

            else if (action.StartsWith("usediscord", StringComparison.OrdinalIgnoreCase))
            {
                string[] args = action.Split(' ');
                if (args.Count() > 1)
                {
                    bool value = false;

                    if (!Boolean.TryParse(args[1], out value))
                    {
                        player.SendChatMessage($"!{{#999999}}UseDiscord can only be either 'true' or 'false'!");
                        return;
                    }

                    Settings.Dynamics.Discord = value;

                    Main.SaveDynamicSettings();

                    NAPI.Chat.SendChatMessageToAll($"!{{#FF6347}}[AdmCmd] !{{#FFFFFF}}{player.Name} has set server discord integration to {value.ToString()}.");
                }
                else player.SendChatMessage("!{#999999}Proper usage: /serveroptions usediscord <true/false>");
            }

            else player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Invalid option, possible options: 'maxinlobby, derbyveh, usediscord, onfootcollisions'");

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

        [Command("lock")]
        public static void cmd_lock(Player player)
        {
            Vehicle targetVeh = null;
            if (!player.IsInVehicle || player.Vehicle == null)
            {
                foreach (Vehicle v in RAGE.Entities.Vehicles.All)
                {
                    if (v.Dimension == player.Dimension && v.Position.DistanceTo(player.Position) <= 3.5)
                    {
                        if (v.HasSharedData("Team") && v.GetSharedData<int>("Team") != player.GetSharedData<int>("Team")) continue;

                        targetVeh = v;
                        break;
                    }
                }
            }
            else targetVeh = player.Vehicle;

            if (targetVeh == null)
            {
                if (!player.HasData("SilentLock")) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not close enough to a vehicle!"); }
                player.ResetData("SilentLock");
                return;
            }

            if (targetVeh.HasSharedData("Team") && targetVeh.GetSharedData<int>("Team") != player.GetSharedData<int>("Team") && 
                targetVeh.Locked == true)
            {
                if (!player.HasData("SilentLock")) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot unlock this vehicle!"); }
                player.ResetData("SilentLock");
                return;
            }

            targetVeh.Locked = !targetVeh.Locked;
            targetVeh.SetSharedData("Locked", targetVeh.Locked);

            if (targetVeh.HasSharedData("Team") && targetVeh.GetSharedData<int>("Team") != player.GetSharedData<int>("Team"))
            {
                EPlayer.Action(player, "locks the door from inside.");
            }
            else
            {
                if (targetVeh.Locked)
                {
                    EPlayer.Action(player, "locks the door of the vehicle.");
                }
                else EPlayer.Action(player, "unlocks the door of the vehicle.");
            }

            return;
        }

        [Command("engine")]
        public static void cmd_engine(Player player)
        {
            if (!player.IsInVehicle || player.Vehicle == null)
            {
                return;
            }

            if (player.Vehicle.HasSharedData("Team") && player.Vehicle.GetSharedData<int>("Team") != player.GetSharedData<int>("Team") && 
                player.Vehicle.EngineStatus == false)
            {
                player.SendNotification("~r~You cannot start this vehicle!");
                return;
            }

            if(player.VehicleSeat != 0)
            {
                player.SendNotification("~r~You are not the driver!");
                return;
            }

            player.Vehicle.EngineStatus = !player.Vehicle.EngineStatus;
            player.Vehicle.SetSharedData("Engine", player.Vehicle.EngineStatus);

            if (player.Vehicle.EngineStatus)
            {
                EPlayer.Action(player, "turns the vehicle's engine on.");
            }
            else EPlayer.Action(player, "turns the vehicle's engine off.");

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
            player.SendChatMessage("!{#A8FFAB}Weather Forecast: !{#FFFFFF}" + Main.WeatherToForecast(lobby.Weather) + " (" + lobby.Weather + ")");

            return;

        }


        [Command("game", "!{#AFAFAF}[USAGE] game (option: stop, start, autostart, teambalance, mode)", GreedyArg = true)]
        public void cmd_game(Player player, string action)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 2)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:game:{action.Replace(":", "|")}"); } catch { }

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
            else if (action.StartsWith("mode", StringComparison.OrdinalIgnoreCase))
            {
                string[] args = action.Split(' ');
                if (args.Count() > 2)
                {
                    int lobbyid = 0;
                    if (!Int32.TryParse(args[1], out lobbyid) || lobbyid < 1 || lobbyid > Settings.Lobbies.Count)
                    {
                        player.SendChatMessage("!{#999999}Lobby ID must be in numbers between 1-" + Settings.Lobbies.Count + "!");
                        return;
                    }

                    LobbyData lobby = Settings.Lobbies.Find(a => a.lobbyId == lobbyid);
                    if ((lobby != null) == false)
                    {
                        player.SendChatMessage("!{#999999}Lobby ID is invalid!");
                        return;
                    }

                    if (args[2].Equals("standard", StringComparison.OrdinalIgnoreCase))
                    {
                        lobby.gameMode = GameModes.STANDARD;
                    }
                    else if (args[2].Equals("busted", StringComparison.OrdinalIgnoreCase))
                    {
                        lobby.gameMode = GameModes.BUSTED;
                    }
                    else { player.SendChatMessage("!{#999999}Proper usage: /game mode <lobby id> <mode: 'standard', 'busted'>"); return; }

                    NAPI.Chat.SendChatMessageToAll($"!{{#FF6347}}[AdmCmd] !{{#FFFFFF}}{player.Name} has set lobby {lobby.lobbyId}'s mode to {Main.ConvertGameMode(lobby.gameMode)}.");
                }
                else player.SendChatMessage("!{#999999}Proper usage: /game mode <lobby id> <mode: 'standard', 'busted'>");
            }
            else player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Invalid option, possible options: 'start', 'stop', 'autostart', 'teambalance', 'mode'");

            return;
        }

        [Command("tog", "!{#AFAFAF}[USAGE] tog (option: hud, pm, radio, ooc)", GreedyArg = true, Alias = "toggle")]
        public void cmd_tog(Player player, string action)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (action.StartsWith("hud", StringComparison.OrdinalIgnoreCase))
            {
                player.SendChatMessage("!{#33C446}[#] !{#FFFFFF}Please press F7 to toggle HUD.");
            }
            else if (action.StartsWith("pm", StringComparison.OrdinalIgnoreCase))
            {
                player.SetData("PMState", !player.GetData<bool>("PMState"));
                if (player.GetData<bool>("PMState") == true)
                {
                    player.SendChatMessage("!{#33C446}[#] !{#FFFFFF}You have enabled your PMs.");
                }
                else player.SendChatMessage("!{#FF6347}[#] !{#FFFFFF}You have disabled your PMs.");
            }
            else if (action.StartsWith("ooc", StringComparison.OrdinalIgnoreCase))
            {
                player.SetData("OOCState", !player.GetData<bool>("OOCState"));
                if (player.GetData<bool>("OOCState") == true)
                {
                    player.SendChatMessage("!{#33C446}[#] !{#FFFFFF}You have enabled OOC chat.");
                }
                else player.SendChatMessage("!{#FF6347}[#] !{#FFFFFF}You have disabled OOC chat.");
            }
            else if (action.StartsWith("radio", StringComparison.OrdinalIgnoreCase))
            {
                player.TriggerEvent("ToggleClientRadio");
            }
            else player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Invalid option, possible options: 'hud', 'pm', 'radio', 'ooc'");

            return;
        }

        [Command("info", Alias = "discord")]
        public void cmd_info(Player player)
        {
            player.SendChatMessage("!{FFFF00}============| SERVER INFORMATION |============");

            player.SendChatMessage("!{#FFDD00}Los Santos Copchase !{#FFFFFF}v" + Settings.Version);
            player.SendChatMessage("!{#FF6347}Server Discord: !{#FFFFFF}discord.gg/y3g2WuR");
            player.SendChatMessage("!{#FFDD00}Community/Portal Discord: !{#FFFFFF}discord.gg/8wBg82v");
            player.SendChatMessage("!{#FFFF00}Website: !{#FFFFFF}ls-cc.info");
            player.SendChatMessage("  ");
            player.SendChatMessage("!{#FFFFFF}Scripted by !{#FF6347}KevY !{#FFFFFF}as the sole dev.");
            player.SendChatMessage("!{#FFFFFF}Special Thanks: !{#FF6347}RotX !{#FFFFFF}(CEF/Web Pages) !{#FF6347}CodeName420 !{#FFFFFF}(Mapping) .");
            player.SendChatMessage("!{#FFFFFF}Thanks to !{#FF6347}LSCC SAMP staff !{#FFFFFF}for assistance and ideas.");
            player.SendChatMessage("  ");
            return;
        }

        [Command("aveh", "!{#AFAFAF}[USAGE] aveh (vehicle model name)", Alias = "av")]
        public void cmd_aveh(Player player, string carname)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            Random rand = new Random();

            Vehicle aVehicle = NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(carname), player.Position.Around(5.0f), player.Heading, rand.Next(122), rand.Next(122), player.Name, 255, false, true, player.Dimension);
            aVehicle.EngineStatus = true;
            aVehicle.SetSharedData("Engine", true);
            aVehicle.SetSharedData("Locked", false);
            aVehicle.Locked = false;
            aVehicle.SetSharedData("Team", -1);

            Settings.AdminVehicles.Add(aVehicle);

            NAPI.Task.Run(() =>
            {
                lock (player)
                {
                    if (player != null && player.Exists)
                    {
                        if (!aVehicle.IsNull && aVehicle.Exists) player.SetIntoVehicle(aVehicle, 0);
                    }
                }
            }, 1000);

            try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:aveh:{carname.Replace(":", "|")}"); } catch { }
            return;
        }

        [Command("veh", "!{#AFAFAF}[USAGE] veh (vehicle model name)", Alias = "v")]
        public void cmd_veh(Player player, string carname)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (player.GetSharedData<int>("InLobby") != -3 || player.Dimension < 256 || player.Dimension > 512)
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

            try { Main.Log("freeroam.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:veh:{carname}"); } catch { }

            Vehicle aVehicle = NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(carname), player.Position.Around(5.0f), player.Heading, rand.Next(122), rand.Next(122), player.Name, 255, false, true, player.Dimension);
            aVehicle.SetSharedData("Locked", false);
            aVehicle.Locked = false;
            aVehicle.EngineStatus = true;
            aVehicle.SetSharedData("Engine", true);

            aVehicle.SetSharedData("Team", -1);

            freeroamVehs.Add(aVehicle);

            player.SetData("FreeroamVehicles", freeroamVehs);

            player.SendChatMessage("!{#FF6347}>> !{#FFFFFF}You have spawned a " + carname + " at your current location.");

            NAPI.Task.Run(() =>
            {
                lock (player)
                {
                    if (player != null && player.Exists)
                    {
                        if (!aVehicle.IsNull && aVehicle.Exists) player.SetIntoVehicle(aVehicle, 0);
                    }
                }
            }, 1000);

            return;
        }

        [Command("mytime", "!{#AFAFAF}[USAGE] mytime (hour)", Alias = "myt")]
        public void cmd_mytime(Player player, int hour)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (player.GetSharedData<int>("InLobby") != -3 || player.Dimension < 256 || player.Dimension > 512)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not in freeroam!");
                return;
            }

            if (hour < 0 || hour > 23)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Hours are between 0-23.");
                return;
            }

            player.SetData("FreeroamTime", hour);
            player.SendChatMessage("!{#FF6347}>> !{#FFFFFF}You have set your time/hour to " + hour + ".");

            player.TriggerEvent("SetTimeWeather", player.GetData<int>("FreeroamTime"), 0, 0, player.GetData<int>("FreeroamWeather"));

            return;
        }

        [Command("myweather", "!{#AFAFAF}[USAGE] myweather (weatherid)", Alias = "myw")]
        public void cmd_myweather(Player player, int weatherid)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (player.GetSharedData<int>("InLobby") != -3 || player.Dimension < 256 || player.Dimension > 512)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not in freeroam!");
                return;
            }

            if(weatherid < 0 || weatherid > 9)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Weather ID's are between 0-9.");
                return;
            }

            player.SetData("FreeroamWeather", weatherid);
            player.SendChatMessage("!{#FF6347}>> !{#FFFFFF}You have set your weather to " + weatherid + ".");

            player.TriggerEvent("SetTimeWeather", player.GetData<int>("FreeroamTime"), 0, 0, player.GetData<int>("FreeroamWeather"));

            return;
        }

        [Command("togblip")]
        public void cmd_togblip(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if((player.GetSharedData<int>("InLobby") == -3 && player.Dimension >= 256 && player.Dimension <= 512) == false && EPlayer.GetAccount(player).Admin < 3)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not in freeroam!");
                return;
            }

            player.SetData("FreeroamBlip", !player.GetData<bool>("FreeroamBlip"));
            player.SetSharedData("VisibleBlip", player.GetData<bool>("FreeroamBlip"));

            player.SendChatMessage("!{#FF6347}>> !{#FFFFFF}You have " + (player.GetData<bool>("FreeroamBlip") == true ? "enabled" : "disabled") + " your freeroam minimap blip.");
            return;
        }

        [Command("godmode")]
        public void cmd_godmode(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if ((player.GetSharedData<int>("InLobby") == -3 && player.Dimension >= 256 && player.Dimension <= 512) == false && EPlayer.GetAccount(player).Admin < 3)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not in freeroam!");
                return;
            }

            player.SetData("FreeroamGodmode", !player.GetData<bool>("FreeroamGodmode"));
            player.TriggerEvent("SetActorInvulnerable", player.GetData<bool>("FreeroamGodmode"));

            player.SendChatMessage("!{#FF6347}>> !{#FFFFFF}You have " + (player.GetData<bool>("FreeroamGodmode") == true ? "enabled" : "disabled") + " freeroam godmode.");
            return;
        }

        [Command("setspawn")]
        public void cmd_setspawn(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if ((player.GetSharedData<int>("InLobby") == -3 && player.Dimension > 256 && player.Dimension <= 512) == false && EPlayer.GetAccount(player).Admin < 3)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not in freeroam or you are in the default world!");
                return;
            }

            Settings.FreeroamSpawns.RemoveAll(a => a.ID == player.GetData<uint>("FreeroamWorld"));

            Settings.FreeroamSpawns.Add(new SpawnSet(player.GetData<uint>("FreeroamWorld"), player.Position.X, player.Position.Y, player.Position.Z, player.Heading));

            foreach (Player p in Settings.LoggedInPlayers.Where(a => a.GetData<uint>("FreeroamWorld") == player.GetData<uint>("FreeroamWorld")))
            {
                p.SendChatMessage("!{#FF6347}>> !{#FFFFFF}" + player.Name + " has changed the spawn of this freeroam world.");
            }

            try { Main.Log("freeroam.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:setspawn:{player.Dimension-(uint)256}"); } catch { }

            return;
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
            player.SetSharedData("VisibleBlip", player.GetData<bool>("GPS"));

            player.SendChatMessage("!{#33C446}[#] !{#FFFFFF}You have " + (player.GetData<bool>("GPS") == true ? "enabled" : "disabled") + 
                " your emergency GPS beacon.");

            return;
        }

        [Command("goto", "!{#AFAFAF}[USAGE] goto (target)")]
        public void cmd_goto(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
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
            try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:goto:{target.Name}"); } catch { }
            return;
        }

        [Command("bring", "!{#AFAFAF}[USAGE] bring (target)", Alias = "gethere")]
        public void cmd_bring(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:bring:{target.Name}"); } catch { }

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

        [Command("bringall", Alias = "gethereall")]
        public void cmd_bringall(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 3)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:bringall"); } catch { }

            foreach (Player target in RAGE.Entities.Players.All)
            {
                target.Position = player.Position.Around(3.0f);
                target.Dimension = player.Dimension;
            }

            EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has summoned all the players.");
            return;
        }

        [Command("freezeall")]
        public void cmd_freezeall(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 3)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:freezeall"); } catch { }

            foreach (Player target in RAGE.Entities.Players.All)
            {
                EPlayer.Freeze(target, true);
            }

            EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has frozen all the players.");
            return;
        }

        [Command("unfreezeall")]
        public void cmd_unfreezeall(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 3)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:unfreezeall"); } catch { }

            foreach (Player target in RAGE.Entities.Players.All)
            {
                EPlayer.Freeze(target, false);
            }

            EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has thawed all the players.");
            return;
        }

        [Command("sethpall", "!{#AFAFAF}[USAGE] sethpall (health) (armor)")]
        public void cmd_sethpall(Player player, int health, int armor)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 3)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:sethpall:{health}:{armor}"); } catch { }

            foreach (Player target in RAGE.Entities.Players.All)
            {
                target.Health = health;
                target.Armor = armor;
            }

            EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has changed all player's health & ar.");
            return;
        }

        [Command("noclip", "!{#AFAFAF}[USAGE] noclip", GreedyArg = true)]
        public void cmd_noclip(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 3)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Toggled noclip, actions:");
            player.SendChatMessage("!{#FF6347}LSHIFT: Boost | LCTRL: Slow | LSHIFT + CTRL: Very slow (adjustable)");
            player.SendChatMessage("!{#FF6347}/noclip_vsm (rate) (rate is in decimal like 0.10) -- will adjust very slow speed (for cinematic etc)");
            player.TriggerEvent("ToggleNoclip");
            return;
        }

        [Command("sethp", "!{#AFAFAF}[USAGE] sethp (target) (health)")]
        public void cmd_sethp(Player player, Player target, int health)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            if (!player.Name.Equals(target.Name) && EPlayer.GetAccount(target).Admin >= EPlayer.GetAccount(player).Admin && !player.Name.StartsWith("KevY"))
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Target has higher/equal admin level than you.");
                return;
            }

            try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:sethp:{target.Name}:health"); } catch { }

            target.Health = health;
            EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has set {target.Name}'s HP to {health}.");
            return;
        }

        [Command("setinv", "!{#AFAFAF}[USAGE] setinv(ulnerable) (target) (0 or 1)", Alias = "setinvulnerable")]
        public void cmd_setinv(Player player, Player target, int status)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:setinv:{target.Name}:{status}"); } catch { }

            player.TriggerEvent("SetActorInvulnerable", status > 0 ? true : false);
            player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You have toggled " + target.Name + "'s invulnerability.");
            return;
        }

        [Command("setar", "!{#AFAFAF}[USAGE] setar(mor) (target) (armor)", Alias = "setarmor,setarmour")]
        public void cmd_setar(Player player, Player target, int armour)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:setar:{target.Name}:{armour}"); } catch { }

            target.Armor = armour;
            EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has set {target.Name}'s AR to {armour}.");
            return;
        }

        [Command("stopanim", Alias = "sa")]
        public void cmd_sa(Player player)
        {
            if(player.HasData("TasedTill") && Environment.TickCount64 < player.GetData<long>("TasedTill"))
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
            if (player.HasData("Cuffed") || (player.HasData("TasedTill") && Environment.TickCount64 < player.GetData<long>("TasedTill"))) 
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

        [Command("cuff", "!{#AFAFAF}[USAGE] cuff (target)")]
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

            if (player.Position.DistanceTo(target.Position) > 3.0 || target.IsInVehicle) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not near that player!"); return; }

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
                try {
                    if (player != null && player.Exists)
                    {
                        if (player != null && target != null && target.Exists)
                        {
                            if (target == null || !target.IsInstanceAlive || player == null || !player.IsInstanceAlive)
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
                            player.SetSharedData("FugitiveChance", player.GetSharedData<float>("FugitiveChance")+(0.2 * (EPlayer.GetAccount(player).DonatorLevel + 1)));
                            EPlayer.Action(player, $"has grabbed ahold of {target.Name} and slapped the handcuffs on their wrists.");

                            EPlayer.SendMessageInLobby(lobbye.lobbyId, $"!{{#ADD8E6}}Cop {player.Name} has apprehended Fugitive {target.Name}.");

                            target.SetAccessories(7, 41, 0); // Handcuffs

                            EPlayer.Handcuff(target, true);

                            Events.RequestPlayAnimation(player, "mp_arresting", "a_uncuff", 2);

                            NAPI.Task.Run(() =>
                            {
                                lock (player)
                                {
                                    if (player != null && player.Exists)
                                    {
                                        Events.RequestClearAnimation(player);
                                    }
                                }
                            }, 2000);

                            target.Heading = player.Heading;

                            EPlayer.GetAccount(player).Arrests++;

                            Events.OnPlayerDeath(target, player, 696988); // Reason 696988 = handcuffs.
                        }
                    }
                }
                catch { }
            }, 3000);
            return;
        }

        [Command("dragout", "!{#AFAFAF}[USAGE] dragout (target)")]
        public void cmd_dragout(Player player, Player target)
        {
            bool isACop = false;
            LobbyData lobby = Settings.Lobbies.Find(lobby => lobby.lobbyId == player.GetSharedData<int>("InLobby"));
            if (lobby != null)
            {
                if (lobby.Cops.Contains(player) && lobby.gameStatus == 1 && lobby.Fugitives.Contains(target) && target.IsInVehicle && !player.IsInVehicle)
                {
                    isACop = true;
                }
            }

            if (!isACop)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not a cop or the target cannot be dragged out right now!");
                return;
            }

            if (player.Position.DistanceTo(target.Position) >= 3.5) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not near that player!"); return; }

            EPlayer.Action(player, $"leans towards {target.Name}'s vehicle and tries to open the door to drag them out.");
            
            player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are now dragging out the target, stay with them for 5 seconds to complete the operation.");

            NAPI.Task.Run(() =>
            {
                try
                {
                    if (player != null && player.Exists)
                    {
                        if (player != null && target != null && target.Exists)
                        {
                            if (target == null || !target.IsInstanceAlive || player == null || !player.IsInstanceAlive || player.Dead)
                            {
                                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Operation failed, target or you are no longer alive.");
                                return;
                            }

                            LobbyData lobbye = Settings.Lobbies.Find(lob => lob.lobbyId == player.GetSharedData<int>("InLobby"));
                            if (lobbye != null)
                            {
                                if (lobbye.Cops.Contains(player) && lobbye.gameStatus == 1 && lobbye.Fugitives.Contains(target) && target.IsInVehicle 
                                    && !player.IsInVehicle)
                                {
                                    isACop = true;
                                }
                            }

                            if (!isACop)
                            {
                                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Operation failed, you cannot drag out that target anymore.");
                                return;
                            }

                            if (player.Position.DistanceTo(target.Position) >= 3.5)
                            {
                                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not near the target!");

                                return;
                            }

                            EPlayer.GrantXP(player, 2);
                            player.SetSharedData("FugitiveChance", player.GetSharedData<float>("FugitiveChance") + (0.1 * (EPlayer.GetAccount(player).DonatorLevel + 1)));
                            cmd_do(player, $"{target.Name} has been dragged out of the vehicle.");

                            EPlayer.SendMessageInLobby(lobbye.lobbyId, $"!{{#ADD8E6}}Cop {player.Name} has dragged {target.Name} out of the vehicle!");
                            target.TriggerEvent("ThrowFromVehicle");
                        }
                    }
                }
                catch { }
            }, 5000);
            return;
        }

        [Command("switchseats", "!{#AFAFAF}[USAGE] switchseats (target)", Alias = "swapseats,swapseat,switchseat,ss")]
        public static void cmd_switchseats(Player player, Player target)
        {
            if ((!player.IsInVehicle || !target.IsInVehicle) || player.Vehicle != target.Vehicle && player != target)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not in the same vehicle as the target or not in one!");
                return;
            }

            if (player.HasData("_currentlySwappingSeats") || player == target) return;

            if(player.HasData("SwapSeating") && player.HasData("SwapSeatingTime") && 
                Environment.TickCount64 - player.GetData<long>("SwapSeatingTime") <= 5000)
            {
                try
                {
                    Player seatingWith = player.GetData<Player>("SwapSeating");
                    if (seatingWith.Exists && seatingWith == target) // They the one who sent requests
                    {
                        player.ResetData("SwapSeating");
                        player.ResetData("SwapSeatingTime");
                        target.ResetData("SwapSeating");
                        target.ResetData("SwapSeatingTime");

                        player.SetData("_currentlySwappingSeats", true);
                        target.SetData("_currentlySwappingSeats", true);

                        Vehicle hwnd = target.Vehicle;

                        int playSeat = player.VehicleSeat;
                        int targSeat = target.VehicleSeat;

                        player.Position = player.Position.Subtract(new Vector3(0.0, 0.0, 30.0f));
                        target.Position = player.Position.Subtract(new Vector3(0.0, 5.0, 30.0f));
                        hwnd.Position = hwnd.Position.Add(new Vector3(0.5, 0.5, 0.5));

                        NAPI.Task.Run(() =>
                        {
                            try
                            {
                                if (hwnd != null && player != null && target != null && player.Exists && target.Exists)
                                {
                                    NAPI.Task.Run(() =>
                                    {
                                        try
                                        {
                                            if (hwnd != null && player != null && target != null && player.Exists && target.Exists)
                                            {
                                                player.TriggerEvent("SetInVehEx", hwnd, targSeat-1);
                                                target.TriggerEvent("SetInVehEx", hwnd, playSeat-1);

                                                player.ResetData("_currentlySwappingSeats");
                                                target.ResetData("_currentlySwappingSeats");

                                                var msg = "!{#FFFFFF}(( !{#FF6347}" + player.Name + " and  " + target.Name + " have swapped seats." + " !{#FFFFFF}))";

                                                foreach (Player playa in NAPI.Player.GetPlayersInRadiusOfPlayer(60, player))
                                                {
                                                    if (playa.Dimension == player.Dimension) playa.SendChatMessage(msg);
                                                }
                                            }
                                        }
                                        catch { }
                                    }, 1000);

                                    player.SetIntoVehicle(hwnd, targSeat);
                                    target.SetIntoVehicle(hwnd, playSeat);

                                    player.TriggerEvent("SetInVehEx", hwnd, targSeat-1);
                                    target.TriggerEvent("SetInVehEx", hwnd, playSeat-1);
                                }
                            }
                            catch { }
                        }, player.Ping + target.Ping + 500);

                        return;
                    }
                }
                catch { }
            }

            player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You have sent a swapseats request to " + target.Name + ", they have 5s to accept.");
            target.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You have received a swapseats request from " + player.Name + ", you have 5s to accept. (/ss " + player.Name + ")");

            player.ResetData("SwapSeating");
            player.ResetData("SwapSeatingTime");
            target.SetData("SwapSeating", player);
            target.SetData("SwapSeatingTime", Environment.TickCount64);

            return;
        }

        [Command("forceswitchseats", "!{#AFAFAF}[USAGE] forceswitchseats (player) (target)", Alias = "forceswapseats,forceswapseat,forceswitchseat,fss")]
        public void cmd_forceswitchseats(Player admin, Player player, Player target)
        {
            if (!EPlayer.LoggedIn(admin)) return;

            Account acc = EPlayer.GetAccount(admin);

            if (acc.Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not authorized to use this command.");
                return;
            }

            if ((!player.IsInVehicle || !target.IsInVehicle) || player.Vehicle != target.Vehicle && player != target)
            {
                admin.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Players are not in the same vehicle!");
                return;
            }

            if (player.HasData("_currentlySwappingSeats") || target.HasData("_currentlySwappingSeats")) 
            { admin.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Target's are currently switching..."); return; }

            try
            {
                Commands.cmd_switchseats(player, target);
                
                NAPI.Task.Run(() => Commands.cmd_switchseats(target, player), 500 + target.Ping);
            }
            catch { admin.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}An error has occured. (FATAL)"); }

            try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{admin.Name}:forceswapseats:{target.Name}:{player.Name}"); } catch { }
            EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {admin.Name} has forced {player.Name} and {target.Name} to swap seats.");

            return;
        }

        [Command("takecomp", "!{#AFAFAF}[USAGE] takecomp (target) (weapon/hash) (component/hash)", GreedyArg = true)]
        public void cmd_takecomp(Player player, Player target, string wep, string component)
        {
            if (!EPlayer.LoggedIn(player) || !EPlayer.LoggedIn(target))
                return;

            if (EPlayer.GetAccount(player).Admin < 3)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not a level 3 admin.");
                return;
            }


            UInt64 wephash = 0;
            bool found = false;
            jWeaponData itemData = new jWeaponData();

            if (!UInt64.TryParse(wep, out wephash))
            {
                wephash = 0;
            }

            foreach (KeyValuePair<ulong, jWeaponData> i in Settings.WeaponData)
            {
                if (wephash != 0)
                {
                    if (i.Key == wephash)
                    {
                        found = true;
                        itemData = i.Value;
                        break;
                    }
                }
                else
                {
                    if (!found)
                    {
                        if (i.Value.Name.Contains(wep, StringComparison.CurrentCultureIgnoreCase))
                        {
                            found = true;
                            wephash = i.Key;
                            itemData = i.Value;
                        }
                    }
                    if (i.Value.Name.Equals(wep, StringComparison.CurrentCultureIgnoreCase)) // If it's really equal then override old found
                    {
                        found = true;
                        wephash = i.Key;
                        itemData = i.Value;
                    }
                }
            }

            if (found == false)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Invalid weapon! Try using hashes or exact names.");
                return;
            }

            found = false;
            UInt64 comphash = 0;
            Component comp = new Component();

            if (!UInt64.TryParse(component, out comphash))
            {
                comphash = 0;
            }

            foreach (var item in itemData.Components)
            {
                if (comphash != 0)
                {
                    if (item.Key == wephash)
                    {
                        found = true;
                        comp = item.Value;
                        break;
                    }
                }
                else
                {
                    if (!found)
                    {
                        if (item.Value.Name.Contains(component, StringComparison.CurrentCultureIgnoreCase))
                        {
                            found = true;
                            comphash = item.Key;
                            comp = item.Value;
                        }
                    }
                    if (item.Value.Name.Equals(component, StringComparison.CurrentCultureIgnoreCase)) // If it's really equal then override old found
                    {
                        found = true;
                        comphash = item.Key;
                        comp = item.Value;
                    }
                }
            }

            if (found == false)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Invalid component! Try using hashes or exact names.");
                return;
            }

            try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:takecomp:{target.Name}:{itemData.Name}:{comp.Name}"); } catch { }


            Dictionary<ulong, wComponentData> wepItems = new Dictionary<ulong, wComponentData>();

            Account acc = EPlayer.GetAccount(target);
            try
            {
                if (acc.WeaponComponents == null || acc.WeaponComponents.Length < 4) throw (new System.Exception("Invalid weapon component > going to new."));

                wepItems = NAPI.Util.FromJson<Dictionary<ulong, wComponentData>>(acc.WeaponComponents);
            }
            catch
            {
                wepItems = new Dictionary<ulong, wComponentData>();

                wComponentData test = new wComponentData();
                test.Components.Add(123456789);
                wepItems.Add(123456789, test);
            }

            Dictionary<ulong, wComponentData> newWepData = new Dictionary<ulong, wComponentData>();

            foreach (KeyValuePair<ulong, wComponentData> i in wepItems)
            {
                wComponentData newDataTwo = new wComponentData();

                if (i.Key == wephash)
                {
                    newDataTwo = i.Value;
                    if (i.Value.Components.Contains(comphash)) newDataTwo.Components.Remove(comphash);
                }

                newWepData.Add(i.Key, newDataTwo);
            }

            wComponentData val = new wComponentData();
            if(newWepData.TryGetValue(wephash, out val))
            {
                if(val.Components.Count > 0)
                {
                    newWepData.Remove(wephash);
                }
            }

            EPlayer.GetAccount(target).WeaponComponents = NAPI.Util.ToJson(newWepData);

            player.SendChatMessage($"!{{#FF6347}}[!] !{{#FFFFFF}}You have removed '{comp.Name}' on '{itemData.Name}' from {target.Name}.");
            target.SendChatMessage($"!{{#FF6347}}[!] !{{#FFFFFF}}{player.Name} has removed '{comp.Name}' on '{itemData.Name}' from you.");

            EPlayer.Save(target);
            return;
        }

        [Command("rules")]
        public void cmd_rules(Player player)
        {
            player.SendChatMessage("!{#FF6347}Rule 1) Common Courtesy: Mind your own business, avoid toxicity and insults. Not applied to IC chat.");
            player.SendChatMessage("!{#FF6347}Rule 2) Common Sense: If you are caught cheating, it's obvious you're getting punished so keep that in mind.");
            player.SendChatMessage("!{#FF6347}Rule 3) Bug Abusing: According to the severity of the bug, punishment will be carried out, do not abuse any bugs!");
            player.SendChatMessage("!{#FF6347}Rule 4) Chat & Voice Rules: Don't speak foreign languages, spam, annoy or insult any other player! Don't shout either.");
            player.SendChatMessage("!{#FF6347}Rule 5) Ramming Rules: Do not suicide ram into 'stationary' vehicles, don't ram people onfoot.");
            player.SendChatMessage("!{#FF6347}Rule 6) Deathmatching: Don't try to kill fugitive's that don't have the tag or anything except arresting.");
            player.SendChatMessage("!{#FF6347}Rule 7) Abusing the mode: Forgetting the purpose of serer and teaming with enemy team etc is strictly bannable!");

            player.SendChatMessage("!{#FFFFFF}>>");
            player.SendChatMessage("!{#FF6347}These are just some basic rules for detailed, complete rule list, visit our discord! (/discord)");
        }

        [Command("givecomp", "!{#AFAFAF}[USAGE] givecomp (target) (weapon/hash) (component/hash)", GreedyArg = true)]
        public void cmd_givecomp(Player player, Player target, string wep, string component)
        {
            if (!EPlayer.LoggedIn(player) || !EPlayer.LoggedIn(target))
                return;

            if (EPlayer.GetAccount(player).Admin < 3)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not a level 3 admin.");
                return;
            }


            UInt64 wephash = 0;
            bool found = false;
            jWeaponData itemData = new jWeaponData();

            if (!UInt64.TryParse(wep, out wephash))
            {
                wephash = 0;
            }

            foreach (KeyValuePair<ulong, jWeaponData> i in Settings.WeaponData)
            {
                if (wephash != 0)
                {
                    if (i.Key == wephash)
                    {
                        found = true;
                        itemData = i.Value;
                        break;
                    }
                }
                else
                {
                    if (!found)
                    {
                        if (i.Value.Name.Contains(wep, StringComparison.CurrentCultureIgnoreCase))
                        {
                            found = true;
                            wephash = i.Key;
                            itemData = i.Value;
                        }
                    }
                    if (i.Value.Name.Equals(wep, StringComparison.CurrentCultureIgnoreCase)) // If it's really equal then override old found
                    {
                        found = true;
                        wephash = i.Key;
                        itemData = i.Value;
                    }
                }
            }

            if (found == false)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Invalid weapon! Try using hashes or exact names.");
                return;
            }

            found = false;
            UInt64 comphash = 0;
            Component comp = new Component();

            if (!UInt64.TryParse(component, out comphash))
            {
                comphash = 0;
            }

            foreach (var item in itemData.Components)
            {
                if (comphash != 0)
                {
                    if (item.Key == wephash)
                    {
                        found = true;
                        comp = item.Value;
                        break;
                    }
                }
                else
                {
                    if (!found)
                    {
                        if (item.Value.Name.Contains(component, StringComparison.CurrentCultureIgnoreCase))
                        {
                            found = true;
                            comphash = item.Key;
                            comp = item.Value;
                        }
                    }
                    if (item.Value.Name.Equals(component, StringComparison.CurrentCultureIgnoreCase)) // If it's really equal then override old found
                    {
                        found = true;
                        comphash = item.Key;
                        comp = item.Value;
                    }
                }
            }

            if (found == false)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Invalid component! List of supported comps on " + itemData.Name + ":");
                foreach (var item in itemData.Components)
                {
                    player.SendChatMessage(item.Value.Name);
                }
                return;
            }

            try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:givecomp:{target.Name}:{itemData.Name}:{comp.Name}"); } catch { }


            Dictionary<ulong, wComponentData> wepItems = new Dictionary<ulong, wComponentData>();

            Account acc = EPlayer.GetAccount(target);
            try
            {
                if (acc.WeaponComponents == null || acc.WeaponComponents.Length < 4) throw (new System.Exception("Invalid weapon component > going to new."));

                wepItems = NAPI.Util.FromJson<Dictionary<ulong, wComponentData>>(acc.WeaponComponents);
            }
            catch
            {
                wepItems = new Dictionary<ulong, wComponentData>();

                wComponentData test = new wComponentData();
                test.Components.Add(123456789);
                wepItems.Add(123456789, test);
            }

            Dictionary<ulong, wComponentData> newWepData = new Dictionary<ulong, wComponentData>();

            foreach (KeyValuePair<ulong, wComponentData> i in wepItems)
            {
                wComponentData newDataTwo = new wComponentData();

                newDataTwo = i.Value; 
                if(i.Key == wephash)
                {
                    if(!i.Value.Components.Contains(comphash)) newDataTwo.Components.Add(comphash);
                }

                newWepData.Add(i.Key, newDataTwo);
            }

            if (!newWepData.ContainsKey(wephash))
            {
                wComponentData newDataTwo = new wComponentData();

                newDataTwo.Components.Add(comphash);

                newWepData.Add(wephash, newDataTwo);
            }

            EPlayer.GetAccount(target).WeaponComponents = NAPI.Util.ToJson(newWepData);

            player.SendChatMessage($"!{{#FF6347}}[!] !{{#FFFFFF}}You gave '{comp.Name}' on '{itemData.Name}' to {target.Name}.");
            target.SendChatMessage($"!{{#FF6347}}[!] !{{#FFFFFF}}You have been given '{comp.Name}' on '{itemData.Name}' by {player.Name}.");

            EPlayer.Save(target);
            return;
        }

        [Command("attachcomp", "!{#AFAFAF}[USAGE] attachcomp (target) (component/hash)", GreedyArg = true)]
        public void cmd_attachcomp(Player player, Player target, string component)
        {
            if (!EPlayer.LoggedIn(player) || !EPlayer.LoggedIn(target))
                return;

            if (EPlayer.GetAccount(player).Admin < 3)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not a level 3 admin.");
                return;
            }


            UInt64 wephash = (UInt64)target.CurrentWeapon;
            bool found = false;
            jWeaponData itemData = new jWeaponData();

            

            foreach (KeyValuePair<ulong, jWeaponData> i in Settings.WeaponData)
            {
                if (wephash != 0)
                {
                    if (i.Key == wephash)
                    {
                        found = true;
                        itemData = i.Value;
                        break;
                    }
                }
            }

            if (found == false)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Target doesn't have a compatible weapon in hand! Try using hashes or exact names.");
                return;
            }

            found = false;
            UInt64 comphash = 0;
            Component comp = new Component();

            if (!UInt64.TryParse(component, out comphash))
            {
                comphash = 0;
            }

            foreach (var item in itemData.Components)
            {
                if (comphash != 0)
                {
                    if (item.Key == wephash)
                    {
                        found = true;
                        comp = item.Value;
                        break;
                    }
                }
                else
                {
                    if (!found)
                    {
                        if (item.Value.Name.Contains(component, StringComparison.CurrentCultureIgnoreCase))
                        {
                            found = true;
                            comphash = item.Key;
                            comp = item.Value;
                        }
                    }
                    if (item.Value.Name.Equals(component, StringComparison.CurrentCultureIgnoreCase)) // If it's really equal then override old found
                    {
                        found = true;
                        comphash = item.Key;
                        comp = item.Value;
                    }
                }
            }

            if (found == false)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Invalid component! List of supported comps on " + itemData.Name + ":");
                foreach (var item in itemData.Components)
                {
                    player.SendChatMessage(item.Value.Name);
                }
                return;
            }


            EPlayer.AttachComponent(target, (int)wephash, (int)comphash);
            
            player.SendChatMessage($"!{{#FF6347}}[!] !{{#FFFFFF}}Temporarily attached '{comp.Name}' on '{itemData.Name}' for {target.Name}.");
            target.SendChatMessage($"!{{#FF6347}}[!] !{{#FFFFFF}}Temporarily attached '{comp.Name}' on '{itemData.Name}' by {player.Name}.");
            return;
        }

        [Command("removecomp", "!{#AFAFAF}[USAGE] removecomp (component/hash - hold weapon and use 'help' to list available, no refunds/points are given back)", GreedyArg = true)]
        public void cmd_removecomp(Player player, string component)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            UInt64 wephash = (UInt64)player.CurrentWeapon;
            bool found = false;
            jWeaponData itemData = new jWeaponData();

            foreach (KeyValuePair<ulong, jWeaponData> i in Settings.WeaponData)
            {
                if (wephash != 0)
                {
                    if (i.Key == wephash)
                    {
                        found = true;
                        itemData = i.Value;
                        break;
                    }
                }
            }

            if (found == false)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You don't have a compatible weapon in hand! Try using hashes or exact names.");
                return;
            }

            found = false;
            UInt64 comphash = 0;
            Component comp = new Component();

            if (!UInt64.TryParse(component, out comphash))
            {
                comphash = 0;
            }

            foreach (var item in itemData.Components)
            {
                if (comphash != 0)
                {
                    if (item.Key == wephash)
                    {
                        found = true;
                        comp = item.Value;
                        break;
                    }
                }
                else
                {
                    if (!found)
                    {
                        if (item.Value.Name.Contains(component, StringComparison.CurrentCultureIgnoreCase))
                        {
                            found = true;
                            comphash = item.Key;
                            comp = item.Value;
                        }
                    }
                    if (item.Value.Name.Equals(component, StringComparison.CurrentCultureIgnoreCase)) // If it's really equal then override old found
                    {
                        found = true;
                        comphash = item.Key;
                        comp = item.Value;
                    }
                }
            }

            if (found == false)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}List of supported comps on " + itemData.Name + ":");
                foreach (var item in itemData.Components)
                {
                    player.SendChatMessage(item.Value.Name);
                }
                return;
            }


            Dictionary<ulong, wComponentData> wepItems = new Dictionary<ulong, wComponentData>();

            Account acc = EPlayer.GetAccount(player);
            try
            {
                if (acc.WeaponComponents == null || acc.WeaponComponents.Length < 4) throw (new System.Exception("Invalid weapon component > going to new."));

                wepItems = NAPI.Util.FromJson<Dictionary<ulong, wComponentData>>(acc.WeaponComponents);
            }
            catch
            {
                wepItems = new Dictionary<ulong, wComponentData>();

                wComponentData test = new wComponentData();
                test.Components.Add(123456789);
                wepItems.Add(123456789, test);
            }

            bool alreadyHas = false;
            Dictionary<ulong, wComponentData> newWepData = new Dictionary<ulong, wComponentData>();

            foreach (KeyValuePair<ulong, wComponentData> i in wepItems)
            {
                wComponentData newDataTwo = new wComponentData();

                newDataTwo = i.Value;
                if (i.Key == wephash)
                {
                    if (i.Value.Components.Contains(comphash))
                    {
                        alreadyHas = true;
                        newDataTwo.Components.Remove(comphash);
                    }
                }

                newWepData.Add(i.Key, newDataTwo);
            }

            if (!alreadyHas)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You do not have " + comp.Name + " attached on " + itemData.Name + ".");
                return;
            }

            EPlayer.GetAccount(player).WeaponComponents = NAPI.Util.ToJson(newWepData);

            player.SendChatMessage($"!{{#FF6347}}[!] !{{#FFFFFF}}You removed '{comp.Name}' from '{itemData.Name}' - No donator points will be refunded.");

            try { Main.Log("currency.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:removecomp:{itemData.Name}:{comp.Name}"); } catch { }
            EPlayer.Save(player);
            return;
        }

        [Command("buycomp", "!{#AFAFAF}[USAGE] buycomp (component/hash - hold weapon and use 'help' to list available)", GreedyArg = true)]
        public void cmd_buycomp(Player player, string component)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            UInt64 wephash = (UInt64)player.CurrentWeapon;
            bool found = false;
            jWeaponData itemData = new jWeaponData();

            foreach (KeyValuePair<ulong, jWeaponData> i in Settings.WeaponData)
            {
                if (wephash != 0)
                {
                    if (i.Key == wephash)
                    {
                        found = true;
                        itemData = i.Value;
                        break;
                    }
                }
            }

            if (found == false)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You don't have a compatible weapon in hand! Try using hashes or exact names.");
                return;
            }

            found = false;
            UInt64 comphash = 0;
            Component comp = new Component();

            if (!UInt64.TryParse(component, out comphash))
            {
                comphash = 0;
            }

            foreach (var item in itemData.Components)
            {
                if (comphash != 0)
                {
                    if (item.Key == wephash)
                    {
                        found = true;
                        comp = item.Value;
                        break;
                    }
                }
                else
                {
                    if (!found)
                    {
                        if (item.Value.Name.Contains(component, StringComparison.CurrentCultureIgnoreCase))
                        {
                            found = true;
                            comphash = item.Key;
                            comp = item.Value;
                        }
                    }
                    if (item.Value.Name.Equals(component, StringComparison.CurrentCultureIgnoreCase)) // If it's really equal then override old found
                    {
                        found = true;
                        comphash = item.Key;
                        comp = item.Value;
                    }
                }
            }

            if (found == false)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}List of supported comps on " + itemData.Name + ":");
                foreach (var item in itemData.Components)
                {
                    player.SendChatMessage(item.Value.Name);
                }
                return;
            }

            if (EPlayer.GetAccount(player).DonatorPoints < 100 && !comp.Name.Contains("flashlight", StringComparison.OrdinalIgnoreCase))
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You need at least 100 donator points to buy a weapon component. (Flashlights are free)");
                return;
            }


            Dictionary<ulong, wComponentData> wepItems = new Dictionary<ulong, wComponentData>();

            Account acc = EPlayer.GetAccount(player);
            try
            {
                if (acc.WeaponComponents == null || acc.WeaponComponents.Length < 4) throw (new System.Exception("Invalid weapon component > going to new."));

                wepItems = NAPI.Util.FromJson<Dictionary<ulong, wComponentData>>(acc.WeaponComponents);
            }
            catch
            {
                wepItems = new Dictionary<ulong, wComponentData>();

                wComponentData test = new wComponentData();
                test.Components.Add(123456789);
                wepItems.Add(123456789, test);
            }

            bool alreadyHas = false;
            Dictionary<ulong, wComponentData> newWepData = new Dictionary<ulong, wComponentData>();

            foreach (KeyValuePair<ulong, wComponentData> i in wepItems)
            {
                wComponentData newDataTwo = new wComponentData();

                newDataTwo = i.Value;
                if (i.Key == wephash)
                {
                    if (!i.Value.Components.Contains(comphash)) newDataTwo.Components.Add(comphash);
                    else
                    {
                        alreadyHas = true;
                    }
                }

                newWepData.Add(i.Key, newDataTwo);
            }

            if(alreadyHas)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You have already bought this component!");
                return;
            }

            if (!newWepData.ContainsKey(wephash))
            {
                wComponentData newDataTwo = new wComponentData();

                newDataTwo.Components.Add(comphash);

                newWepData.Add(wephash, newDataTwo);
            }

            EPlayer.GetAccount(player).WeaponComponents = NAPI.Util.ToJson(newWepData);

            if (comp.Name.Contains("flashlight", StringComparison.OrdinalIgnoreCase))
            {
                player.SendChatMessage($"!{{#FF6347}}[!] !{{#FFFFFF}}You have attached '{comp.Name}' on '{itemData.Name}'. (Will attach in copchase)");
            }
            else
            {
                player.SendChatMessage($"!{{#FF6347}}[!] !{{#FFFFFF}}You bought '{comp.Name}' on '{itemData.Name}' for 100 donator points. (Will attach in copchase)");
                EPlayer.GetAccount(player).DonatorPoints -= 100;
            }
            
            try { Main.Log("currency.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:buycomp:{itemData.Name}:{comp.Name}"); } catch { }
            EPlayer.Save(player);
            return;
        }

        [Command("mycomps", GreedyArg = true)]
        public void cmd_mycomps(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            player.SendChatMessage("!{#FF6347}Listing bought weapon components ->");
            
            Dictionary<ulong, wComponentData> wepItems = new Dictionary<ulong, wComponentData>();

            Account acc = EPlayer.GetAccount(player);
            try
            {
                if (acc.WeaponComponents == null || acc.WeaponComponents.Length < 4) throw (new System.Exception("Invalid weapon component > going to new."));

                wepItems = NAPI.Util.FromJson<Dictionary<ulong, wComponentData>>(acc.WeaponComponents);
            }
            catch
            {
                wepItems = new Dictionary<ulong, wComponentData>();

                wComponentData test = new wComponentData();
                test.Components.Add(123456789);
                wepItems.Add(123456789, test);
            }

            int count = 0;
            foreach (KeyValuePair<ulong, wComponentData> i in wepItems)
            {
                jWeaponData wep = new jWeaponData();

                if(Settings.WeaponData.TryGetValue(i.Key, out wep))
                {
                    if (i.Value.Components.Count > 0) player.SendChatMessage("!{#31b55a}" + wep.Name + " | " + i.Value.Components.Count() + " attachments:");
                    foreach (ulong comp in i.Value.Components)
                    {
                        Component compData = new Component();
                        if(wep.Components.TryGetValue(comp, out compData))
                        {
                            player.SendChatMessage("!{#69f5a8}     -- " + compData.Name + " | " + compData.Description);
                            count++;
                        }
                    }
                }
            }

            if (count == 0) player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You have not bought any weapon components yet, /buycomp.");
            return;
        }

        [Command("givegun", "!{#AFAFAF}[USAGE] givegun (target) (ammo) (weapon)", GreedyArg = true)]
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

            foreach (KeyValuePair<ulong, jWeaponData> i in Settings.WeaponData)
            {
                if (wephash != 0)
                {
                    if (i.Key == wephash)
                    {
                        found = true;
                        wepname = i.Value.Name;
                        break;
                    }
                }
                else
                {
                    if (!found)
                    {
                        if (i.Value.Name.Contains(wep, StringComparison.CurrentCultureIgnoreCase))
                        {
                            found = true;
                            wephash = i.Key;
                            wepname = i.Value.Name;
                        }
                    }
                    if (i.Value.Name.Equals(wep, StringComparison.CurrentCultureIgnoreCase)) // If it's really equal then override old found
                    {
                        found = true;
                        wephash = i.Key;
                        wepname = i.Value.Name;
                    }
                }
            }

            foreach (KeyValuePair<ulong, string> i in Main.WeaponReasons)
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
                    if (!found)
                    {
                        if (i.Value.Contains(wep, StringComparison.CurrentCultureIgnoreCase))
                        {
                            found = true;
                            wephash = i.Key;
                            wepname = i.Value;
                        }
                    }
                    if (i.Value.Equals(wep, StringComparison.CurrentCultureIgnoreCase)) // If it's really equal then override old found
                    {
                        found = true;
                        wephash = i.Key;
                        wepname = i.Value;
                    }
                }
            }

            
            if(found == false)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Invalid weapon! Try using hashes or exact names.");
                return;
            }

            try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:givegun:{target.Name}:{wepname}:{ammo}"); } catch { }

            target.GiveWeapon((WeaponHash)wephash, ammo);

            player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You have given a " + wepname + " with " + ammo + " ammo to the target.");
            return;
        }

        [Command("gun", "!{#AFAFAF}[USAGE] gun (ammo) (weapon)", GreedyArg = true)]
        public void cmd_gun(Player player, int ammo, string wep)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (player.GetSharedData<int>("InLobby") != -3 || player.Dimension <= 256 || player.Dimension > 512)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not in freeroam or in default world! (/world)");
                return;
            }


            UInt64 wephash = 0;
            bool found = false;
            string wepname = "";

            if (!UInt64.TryParse(wep, out wephash))
            {
                wephash = 0;
            }

            foreach (KeyValuePair<ulong, jWeaponData> i in Settings.WeaponData)
            {
                if (wephash != 0)
                {
                    if (i.Key == wephash)
                    {
                        found = true;
                        wepname = i.Value.Name;
                        break;
                    }
                }
                else
                {
                    if (!found)
                    {
                        if (i.Value.Name.Contains(wep, StringComparison.CurrentCultureIgnoreCase))
                        {
                            found = true;
                            wephash = i.Key;
                            wepname = i.Value.Name;
                        }
                    }
                    if (i.Value.Name.Equals(wep, StringComparison.CurrentCultureIgnoreCase)) // If it's really equal then override old found
                    {
                        found = true;
                        wephash = i.Key;
                        wepname = i.Value.Name;
                    }
                }
            }

            foreach (KeyValuePair<ulong, string> i in Main.WeaponReasons)
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
                    if (!found)
                    {
                        if (i.Value.Contains(wep, StringComparison.CurrentCultureIgnoreCase))
                        {
                            found = true;
                            wephash = i.Key;
                            wepname = i.Value;
                        }
                    }
                    if (i.Value.Equals(wep, StringComparison.CurrentCultureIgnoreCase)) // If it's really equal then override old found
                    {
                        found = true;
                        wephash = i.Key;
                        wepname = i.Value;
                    }
                }
            }


            if (found == false)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Invalid weapon! Try using hashes or exact names.");
                return;
            }

            try { Main.Log("freeroam.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:gun:{wepname}:{ammo}"); } catch { }

            player.GiveWeapon((WeaponHash)wephash, ammo);

            player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You have given yourself " + wepname + " with " + ammo + " ammo.");
            return;
        }

        [Command("world", "!{#AFAFAF}[USAGE] world (number/id, 0 is default)")]
        public void cmd_world(Player player, int worldid)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (player.GetSharedData<int>("InLobby") != -3)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not in freeroam!");
                return;
            }

            if (worldid < 0 || worldid > 256)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}The world ID can only be between 0-256! (0 is default)");
                return;
            }

            try { Main.Log("freeroam.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:world:{worldid}"); } catch { }

            player.Dimension = (uint)256 + (uint)worldid;
            player.SetData("FreeroamWorld", (uint)worldid);

            player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You have changed your freeroam world to " + worldid.ToString() + ".");

            if(worldid > 0)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You can use /gun in this freeroam world.");
            }
            else if(worldid <= 0)
            {
                player.RemoveAllWeapons();
            }
            return;
        }

        [Command("givegunall", "!{#AFAFAF}[USAGE] givegunall (ammo) (wep)", GreedyArg = true)]
        public void cmd_givegunall(Player player, int ammo, string wep)
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

            foreach (KeyValuePair<ulong, jWeaponData> i in Settings.WeaponData)
            {
                if (wephash != 0)
                {
                    if (i.Key == wephash)
                    {
                        found = true;
                        wepname = i.Value.Name;
                        break;
                    }
                }
                else
                {
                    if (!found)
                    {
                        if (i.Value.Name.Contains(wep, StringComparison.CurrentCultureIgnoreCase))
                        {
                            found = true;
                            wephash = i.Key;
                            wepname = i.Value.Name;
                        }
                    }
                    if (i.Value.Name.Equals(wep, StringComparison.CurrentCultureIgnoreCase)) // If it's really equal then override old found
                    {
                        found = true;
                        wephash = i.Key;
                        wepname = i.Value.Name;
                    }
                }
            }

            foreach (KeyValuePair<ulong, string> i in Main.WeaponReasons)
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


            if (found == false)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Invalid weapon! Try using hashes or exact names.");
                return;
            }

            foreach (Player target in Settings.LoggedInPlayers)
            {
                target.GiveWeapon((WeaponHash)wephash, ammo);
            }

            try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:givegunall:{wepname}:{ammo}"); } catch { }

            player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You have given a " + wepname + " with " + ammo + " ammo to the all players.");
            return;
        }

        [Command("afix", "!{#AFAFAF}[USAGE] afix (target)")]
        public void cmd_afix(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not a level 1 admin.");
                return;
            }

            if(!target.IsInVehicle) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Target is not in any vehicle!"); return; }

            try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:fix:{target.Name}"); } catch { }

            target.Vehicle.Repair();

            EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has repaired {target.Name}'s vehicle.");

            return;
        }

        [Command("flip")]
        public void cmd_flip(Player player)
        {
            player.SendChatMessage("bruh just flip the car yourself??");
            player.SendChatMessage("u literally in gta 5 not san andreas dumbo");
        }

        [Command("aflip", "!{#AFAFAF}[USAGE] aflip (target)")]
        public void cmd_aflip(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not a level 1 admin.");
                return;
            }

            if (!target.IsInVehicle) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Target is not in any vehicle!"); return; }

            try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:flip:{target.Name}"); } catch { }

            target.Vehicle.Rotation = new Vector3(0, target.Vehicle.Rotation.Y, target.Vehicle.Rotation.Z);

            EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has flipped {target.Name}'s vehicle.");

            return;
        }

        [Command("removeavehs")]
        public void cmd_removeavehs(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
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
            EPlayer.ShowStats(player, player);
            return;
        }

        [Command("checkstats")]
        public void cmd_checkstats(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 2)
                return;

            EPlayer.ShowStats(player, target);
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

        [Command("voiceblock", "!{#AFAFAF}[USAGE] voiceblock (target)", GreedyArg = true)]
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
                player.SendChatMessage($"!{{#33C446}}[#] !{{#FFFFFF}}You have unmuted {target.Name} on voice chat.");
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

            if(player.HasData("Spectating"))
            {
                cmd_specoff(player);
            }

            if (player.HasData("DerbyVehicle"))
            {
                try { player.GetData<Vehicle>("DerbyVehicle").Delete(); }
                catch { }

                player.ResetData("DerbyVehicle");
            }

            if(player.Health == 0 || player.Dead) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are respawning!"); return; }

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

            if (freeroamVehs.Count > 0)
            {
                for (int i = 0; i < freeroamVehs.Count; i++)
                {
                    freeroamVehs.RemoveAt(i);
                }
            }

            if (!player.HasData("ForcedData"))
            {
                // Do lobby checks here so /sendtolobby ignores all of them.
                // like ingame etc so you cant lobby midgame or some stuff like that you get it?
                // not really needed for ingame but other stuff
            }

            LobbyData elobby = Settings.Lobbies.Find(a => a.lobbyId == player.GetSharedData<int>("InLobby"));
            if (elobby != null)
            {
                if (elobby.gameStatus == 1 && (elobby.Cops.Contains(player) || elobby.Fugitives.Contains(player)))
                {
                    player.Health = 0;
                    return;
                }
            }

            freeroamVehs.Clear();
            player.SetData("FreeroamVehicles", freeroamVehs);
            player.SetSharedData("InDM", -1);
            player.ResetData("ForcedData");


            if(elobby == null) player.SetSharedData("InLobby", -1);

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
            Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == player.GetSharedData<int>("InLobby")).ToList().ForEach((Player ePlayer) =>
            {
                player.SendChatMessage($"!{{#999999}}{ePlayer.Name}");
            });

            return;
        }

        [Command("clientcmd", Alias = "updaterate,lcmenu,fontsize,rappel,rapple,voicebind,noclip_vsm,gpsbind", GreedyArg = true)] // It's a client-side command we're adding here just so it doesn't show "invalid command" when triggered.
        public void cmd_clientcmd(Player player)
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
                    $"!{{#DAA520}}| Players: !{{#FFFFFF}}" +
                    $"{Settings.LoggedInPlayers.Count(a => a.GetSharedData<int>("InLobby") == lobby.lobbyId)}!{{#DAA520}}/!{{#FFFFFF}}{Settings.Dynamics.MaxInLobby} !{{#DAA520}}| Status: {Main.ConvertGameStatus(lobby.gameStatus, true)} !{{#DAA520}}| Mode: {Main.ConvertGameMode(lobby.gameMode, true)}!{{#DAA520}}");
            }

            return;
        }

        [Command("switchlobby", "!{#AFAFAF}[USAGE] switchlobby (lobbyid) (optional: password)", Alias = "swl", GreedyArg = true)]
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
                player.SetData<long>("LastLobbySwitch", Environment.TickCount64);
            }
            else
            {
                if (Environment.TickCount64 - player.GetData<long>("LastLobbySwitch") < 1000)
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
                if (!Int32.TryParse(split[0], out lobbyid) || lobbyid < 1)
                {
                    player.SendChatMessage($"!{{#999999}}Lobby ID cannot be less than 1!");
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

            if (lobby.Password.Length > 1 && EPlayer.GetAccount(player).Admin < 1)
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

            if(Settings.LoggedInPlayers.Count(a => a.GetSharedData<int>("InLobby") == lobbyid) >= Settings.Dynamics.MaxInLobby && EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}That lobby is full!");
                return;
            }

            LobbyData elobby = Settings.Lobbies.Find(a => a.lobbyId == player.GetSharedData<int>("InLobby"));
            if(elobby != null)
            {
                if(elobby.gameStatus == 1 && (elobby.Cops.Contains(player) || elobby.Fugitives.Contains(player)))
                {
                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot switch lobbies in an ongoing game! (/lobby)");
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
                    player.SendChatMessage($">> !{{#DAA520}}Arena ID {i} | !{{#FF6347}}{Settings.DMNames[i]} " +
                           $"!{{#DAA520}}| Players: N/A (No lobby)");
                }
                else
                {
                    player.SendChatMessage($">> !{{#DAA520}}Arena ID {i} | !{{#FF6347}}{Settings.DMNames[i]} " +
                       $"!{{#DAA520}}| Players: " +
                       $"{Settings.LoggedInPlayers.Count(a => a.GetSharedData<int>("InLobby") == player.GetSharedData<int>("InLobby") && a.GetSharedData<int>("InDM") == i)}");
                }
            }


            return;
        }

        [Command("dm", "!{#AFAFAF}[USAGE] dm (arenaid)")]
        public void cmd_dm(Player player, int arenaid)
        {
            if (!EPlayer.LoggedIn(player)) return;
            if (EPlayer.GetAccount(player).JailTime > 0) return;

            if (player.HasData("Spectating"))
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are in spectate mode!");
                return;
            }

            if (player.GetSharedData<int>("Team") != -1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are currently in the copchase!");
                return;
            }

            if (arenaid < 0 || arenaid >= Settings.DMNames.Length)
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

            if (elobby.gameStatus == 1 && (elobby.Cops.Contains(player) || elobby.Fugitives.Contains(player)))
            {                    
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot go into DM while in the copchase! (/lobby)");
                return;
            }


            player.SetSharedData("InDM", arenaid);

            int spawnID = new Random().Next(Settings.DMSpawns[arenaid].GetLength(0));
            Vector3 dmSpawn = (Vector3)Settings.DMSpawns[arenaid][spawnID, 0];
            EPlayer.Spawn(player, false, dmSpawn);
            player.SendChatMessage($"[!] !{{#FFFF00}}Sending you to !{{#FF6347}}DM: {Settings.DMNames[arenaid]}.");

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
                if (elobby != null)
                {

                    if (elobby.gameStatus == 2)
                    {
                        player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot go into freeroam while the match is starting!");
                        return;
                    }

                    if (elobby.gameStatus == 1 && (elobby.Cops.Contains(player) || elobby.Fugitives.Contains(player)))
                    {
                        player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot go into DM while in the copchase! (/lobby)");
                        return;
                    }
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
                try { player.GetData<Vehicle>("DerbyVehicle").Delete(); }
                catch { }

                player.ResetData("DerbyVehicle");
            }

            player.SetData("DerbySpawnTick", Environment.TickCount64 + 2000);
            player.SetSharedData("InDM", -1);
            player.SetSharedData("InLobby", -4);


            player.SendChatMessage($"!{{#FFFF00}}[!] Sending you to !{{#FF6347}}DERBY");
            EPlayer.Spawn(player, false, ((Vector3)Settings.DerbySpawns[0, 0]).Around(2.0f));

            return;
        }

        [Command("setweather", "!{#AFAFAF}[USAGE] setweather (weatherid)", Alias = "sweather")]
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

        [Command("settime", "!{#AFAFAF}[USAGE] settime (hour)", Alias = "stime")]
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

        [Command("gotopos", "!{#AFAFAF}[USAGE] gotopos (x) (y) (z)", Alias = "gpos")]
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

        [Command("setdimension", "!{#AFAFAF}[USAGE] setdimension (target) (vw)", Alias = "setvw")]
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

        [Command("sendtolobby", "!{#AFAFAF}[USAGE] sendtolobby (target)", Alias = "stl")]
        public void cmd_sendtolobby(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player)) return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You do not have the permission to use this command.");
                return;
            }

            try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:sendtolobby:{target.Name}"); } catch { }

            player.SetData("ForcedLobby", true);
            cmd_lobby(target);

            EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has sent {target.Name} to lobby.");
        }

        [Command("keys")]
        public void cmd_keys(Player player)
        {
            if (!EPlayer.LoggedIn(player)) return;

            player.SendChatMessage("!{#FF6347} Key: Function.");
            player.SendChatMessage("!{#FF6347} N: Push to talk voicechat. (Changed using /voicebind)");
            player.SendChatMessage("!{#FF6347} Z: Toggle GPS as a Cop. (Changed using /gpsbind)");
            player.SendChatMessage("!{#FF6347} O: Toggle scoreboard, displays a scrollbar and mouse if needed.");
            player.SendChatMessage("!{#FF6347} L: Lock/unlock vehicle.");
            player.SendChatMessage("!{#FF6347} X: Rapple from a helicopter (not all helicopters, try maverick).");
            player.SendChatMessage("!{#FF6347} Hold voice and press TAB: Switch voice chat modes.");
            player.SendChatMessage("!{#FF6347} F7: Hide/show HUD. (Hold SHIFT to toggle nametags as well)");
            player.SendChatMessage("!{#FF6347} B: Toggle silent siren in police vehicles.");
        }

        [Command("teleport", "!{#AFAFAF}[USAGE] teleport (option: chilliad, airport)", Alias = "tp", GreedyArg = true)]
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
            if (EPlayer.GetAccount(player).Admin < 4) return;

            Events.RequestPlayAnimation(player, lib, name, flags);
            return;
        }

        [Command("dbgmaxspeed", Alias = "setmaxspeed")]
        public void cmd_maxspeed(Player player, int speed)
        {
            if (!EPlayer.LoggedIn(player)) return;
            if (EPlayer.GetAccount(player).Admin < 4) return;

            if (!player.IsInVehicle) return;

            player.Vehicle.SetSharedData("vMaxSpeed", speed / 3.6);
            player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Set your current vehicle's max speed to " + speed + " KM/H.");
            return;
        }

        [Command("dbgdriveforce", Alias = "setdriveforce")]
        public void cmd_driveforce(Player player, float force)
        {
            if (!EPlayer.LoggedIn(player)) return;
            if (EPlayer.GetAccount(player).Admin < 4) return;

            if (!player.IsInVehicle) return;

            player.Vehicle.SetSharedData("vInitialDriveForce", force);
            player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Set your current vehicle's fInitialDriveForce to " + force + ".");
            return;
        }

        [Command("report", "!{#AFAFAF}[USAGE] report (target) (desc)", GreedyArg = true)]
        public void cmd_report(Player player, Player target, string description)
        {
            if(Settings.Requests.Count(r => r.Reported == target && r.Reporter == player) > 0)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You have already reported this guy.");
                return;
            }

            RequestData req = new RequestData(player, description, 0);
            req.Reported = target;

            Settings.Requests.Add(req);

            EPlayer.SendMessageToAllAdmins($"!{{#DBCE40}}[REQUEST {req.RequestID}] {player.Name} reported {target.Name} for {description}. (/reports)");
            player.SendChatMessage("!{#DBCE40}You have filed a report against " + target.Name + ", request ID: " + req.RequestID + ".");
            return;
        }

        [Command("helpme", "!{#AFAFAF}[USAGE] helpme (desc)", Alias = "ask", GreedyArg = true)]
        public void cmd_helpme(Player player, string description)
        {
            if (Settings.Requests.Count(r => r.Reporter == player && r.RequestType == 1) > 0)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You have already asked for assistance, you cannot pile up requests.");
                return;
            }

            RequestData req = new RequestData(player, description, 1);

            Settings.Requests.Add(req);

            EPlayer.SendMessageToAllAdmins($"!{{#1E9913}}[REQUEST {req.RequestID}] {player.Name} asked for help: {description}. (/requests)");
            player.SendChatMessage("!{#DBCE40}You have successfully sent a helpme request, request ID: " + req.RequestID + ".");
            return;
        }

        [Command("car")]
        public void cmd_car(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            CarSelectorData a = new CarSelectorData(
            new object[,]
            {
                { "Default", "images/default.png", 1, 0 },
                { "Default", "images/default.png", 0, 0 },

                // Cops
                { "Police Cruiser", "images/polc1.png", 1, 0 },
                { "Police Cruiser (U)", "images/polc2.png", 1, 2 },
                { "Police Bike", "images/policeb.png", 1, 3 },
                { "Police Buffalo", "images/polbufac.png", 1, 5 },
                { "Police Buffalo (U)", "images/polbufac2.png", 1, 10 },
                { "Police Caracara", "images/lspdcara.png", 1, 12 },
                { "FBI Granger", "images/fbi2.png", 1, 13 },
                { "SWAT Truck", "images/riot.png", 1, 15 },
                { "FBI Buffalo", "images/fbibufac.png", 1, 17 },
                { "Charger '16 (U)", "images/udchall1.png", 1, 20 },
                { "Chopper", "images/polmav.png", 1, 23 },
                { "Revolter (U)", "images/prevolter.png", 1, 25 },

                // Fugitive
                { "Primo", "images/primo.png", 0, 3 },
                { "Huntley", "images/huntley.png", 0, 5 },
                { "S-Class", "images/600sel.png", 0, 6 },
                { "Dubsta", "images/dubsta.png", 0, 8 },
                { "Tailgater", "images/tailgater.png", 0, 10 },
                { "Schafter", "images/schafter2.png", 0, 12 },
                { "Blista Classic", "images/blista2.png", 0, 14 },
                { "Cheburek", "images/cheburek.png", 0, 16 },
                { "Buffalo", "images/buffalo.png", 0, 20 },
                { "Sultan", "images/sultan.png", 0, 25 },
                { "Kuruma", "images/kuruma.png", 0, 30 },
                { "Fastback '66", "images/66fastback.png", 0, 33 },
                { "M3 E46", "images/m3e46.png", 0, 35 },
            },
            EPlayer.GetAccount(player).Level);

            player.SetData("ViewingCarSelector", true);
            player.TriggerEvent("showCarSelector", NAPI.Util.ToJson(a));

            return;
        }

        [Command("customvehlist")]
        public void cmd_customvehlist(Player player)
        {
            player.SendChatMessage("Model 'udchall1' - Dodge Charger 2016");
            player.SendChatMessage("Model 'lspdcara' - Police Caracara");
            player.SendChatMessage("Model 'polbufac' - Police Buffalo (polbufac2 for Unmarked)");
            player.SendChatMessage("Model 'fbibufac' - FIB Buffalo");
            player.SendChatMessage("Model 'sherbufac' - Sheriff Buffalo (sherbufac2 for Unmaarked)");
            player.SendChatMessage("Model 'hwaybufac' - Highway Buffalo (hwaybufac2 for Unmaarked)");
            player.SendChatMessage("Model '600sel' - Mercedes S600?");
            player.SendChatMessage("Model 'prevolter' - Police Revolter Unmarked");
            player.SendChatMessage("Model 'm3e46' - M3 E46 BMW 2005");
            player.SendChatMessage("Model '66fastback' - 1966 Ford Mustang Fastback");
            return;
        }

        [Command("cmds", Alias = "help")]
        public void cmd_help(Player player)
        {
            if (!EPlayer.LoggedIn(player)) return;

            player.SendChatMessage("!{#0079DB}[Los Santos Copchase V !{#2BA3FF}BETA Commands]:");
            player.SendChatMessage("[General]: /ooc /autojoin /b /pm /tog /keys /admins /rules /id");
            player.SendChatMessage("[General]: /report /helpme /l(ow) /c(ar)w(hisper) /s(hout)");
            player.SendChatMessage("[Squad/Partner]: /squad /squadleave /s(quad)c(hat) /squadaccept /squadmembers");
            player.SendChatMessage("[Lobby]: /lobby /(sw)itch(l)obby /mylobby /lobbies /time (/weather)");
            player.SendChatMessage("[Lobby]: /lobbychat (/lo,/lc) /spec /specoff");
            player.SendChatMessage("!{#999999}[Account]: /char help (/character) /clothing /stats /changemypass");
            player.SendChatMessage("!{#5FBCAC}[Voice]: /voiceblock /voicemode (/keys for activation)");
            player.SendChatMessage("!{#339BC4}[Cop!{#F24C27}chase]: !{#FFFFFF}/r(adio) /wt /loadout /car");
            player.SendChatMessage("!{#61A9FA}[Cop]: /gps /cuff /m(egaphone) /dragout");
            player.SendChatMessage("!{#AB00BA}[Roleplay]: /me /do /handsup /s(top)a(nim) /passgun");
            player.SendChatMessage("!{#AB00BA}[Vehicle]: /trunk /lock /engine");
            player.SendChatMessage("!{#28E0DA}[Modes]: /lobby /derby");
            player.SendChatMessage("!{#28E0DA}[Mode: DM]: /dm /dmarena");
            player.SendChatMessage("!{#28E0DA}[Mode: Freeroam]: /freeroam /veh /customvehlist /teleport /mytime /myweather");
            player.SendChatMessage("!{#28E0DA}[Mode: Freeroam]: /togblip /gun /world /godmode /setspawn");
            player.SendChatMessage("!{#A5A8A8}[Misc]: /updaterate /info /fontsize /voicebind /gpsbind /shotstats");
            player.SendChatMessage("!{#A5A8A8}[Dbg/Test]: /pos /lcmenu");
            
            player.SendChatMessage("!{#F7EB05}[Donator/Points] /buycomp (Free Flashlights) /mycomps /removecomp (NO REFUND)");
            if (EPlayer.GetAccount(player).DonatorLevel >= 1)
            {
                player.SendChatMessage("!{#F7EB05}[Bronze Donator] /d(onator)c(hat)");
            }
            if (EPlayer.GetAccount(player).DonatorLevel >= 2)
            {
                player.SendChatMessage("!{#F7EB05}[Silver Donator] /createlobby /deletelobby /setlobbypassword /setlobbyfugitivepercent /lobbymode");
            }

            if (EPlayer.GetAccount(player).Admin >= 1) player.SendChatMessage("!{#FF6347}For admin commands, visit /acmds.");
            return;
        }

        [Command("bancheck", "!{#AFAFAF}[USAGE] bancheck (target name or ban id)", GreedyArg = true)]
        public void cmd_bancheck(Player player, string sqlorname)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
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
                Account tpAcc = ContextFactory.Instance.Accounts.ToList().Find(p => p.Username.Equals(sqlorname, StringComparison.OrdinalIgnoreCase));
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

        [Command("unsuspend", "!{#AFAFAF}[USAGE] unsuspend (account name)", GreedyArg = true)]
        public void cmd_unsuspend(Player player, string name)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 2)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

         
            Account tpAcc = ContextFactory.Instance.Accounts.ToList().Find(p => p.Username.Equals(name, StringComparison.OrdinalIgnoreCase) == true && 
            p.Suspended == 1);

            if ((tpAcc != null) == false)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}There's no suspended account with that name.");
                return;
            }

            try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:unsuspend:{tpAcc.Username}"); } catch { }

            tpAcc.Suspended = 0;
            ContextFactory.Instance.SaveChanges();
            EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] !{{#FFFFFF}}{player.Name} has unsuspended {tpAcc.Username}.");
            return;
        }

        [Command("changename", "!{#AFAFAF}[USAGE] changename (account name) (new name)")]
        public void cmd_changename(Player player, string name, string newname)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 4)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }


            Account tpAcc = ContextFactory.Instance.Accounts.ToList().Find(p => p.Username.Equals(name, StringComparison.OrdinalIgnoreCase) == true);

            if ((tpAcc != null) == false)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}There's no account with that name.");
                return;
            }

            if(ContextFactory.Instance.Accounts.ToList().Find(p => p.Username.Equals(newname, StringComparison.OrdinalIgnoreCase) == true) != null)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}There's already an account with that name!");
                return;
            }

            if(tpAcc.Admin > EPlayer.GetAccount(player).Admin)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}The target's admin level is higher than yours!");
                return;
            }

            Player target = RAGE.Entities.Players.All.FirstOrDefault(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase) == true && EPlayer.LoggedIn(a));
            if(target != null)
            {
                target.SendChatMessage("!{#FF6347} Reconnect with your new name: !{#FFFFFF}" + newname);
                target.SendNotification("Name changed to ~r~" + newname);

                EPlayer.Save(target);
                target.ResetData("AccountHandle");
                target.ResetData("LoggedIn");

                try
                {
                    NAPI.Task.Run(() =>
                    {
                        try
                        {
                            target.Kick();
                        }
                        catch { }
                    }, 500);
                }
                catch { }
            }

            try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:changename:{name}:{newname}"); } catch { }

           
            EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] !{{#FFFFFF}}{player.Name} has changed {tpAcc.Username}'s name to {newname}.");

            tpAcc.Username = newname;
            ContextFactory.Instance.SaveChanges();
            return;
        }

        [Command("changepass", "!{#AFAFAF}[USAGE] changepass (account name) (new password)")]
        public void cmd_changepass(Player player, string name, string newpass)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 4)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }


            Account tpAcc = ContextFactory.Instance.Accounts.ToList().Find(p => p.Username.Equals(name, StringComparison.OrdinalIgnoreCase) == true);

            if ((tpAcc != null) == false)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}There's no account with that name.");
                return;
            }

            if (tpAcc.Admin > EPlayer.GetAccount(player).Admin)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}The target's admin level is higher than yours!");
                return;
            }

            Player target = RAGE.Entities.Players.All.FirstOrDefault(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase) == true && EPlayer.LoggedIn(a));
            if (target != null)
            {
                target.SendChatMessage("!{#FF6347} Your password has been changed to: !{#FFFFFF}" + newpass);
                target.SendNotification("Password changed to ~r~" + newpass);
            }

            try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:changepass:{name}"); } catch { }


            EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] !{{#FFFFFF}}{player.Name} has changed {tpAcc.Username}'s password.");

            string salt = AccountManager.GenSalt();
            string password = AccountManager.Crypt(newpass, salt);
            tpAcc.Salt = salt;
            tpAcc.Password = password;
            ContextFactory.Instance.SaveChanges();
            return;
        }

        [Command("changemypass", "!{#AFAFAF}[USAGE] changemypass (new password)")]
        public void cmd_changemypass(Player player, string newpass)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (player.HasData("LastCryptAttempt") && Environment.TickCount64 - player.GetData<long>("LastCryptAttempt") < 5000)
            {
                player.SendNotification("~r~Hold your horses.", true);
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Hold your horses!");
                return;
            }

            if (newpass.Length < 3)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Your new password must be more than 3 characters!");
                return;
            }

            player.SendChatMessage("!{#FF6347} Your password has been changed to: !{#FFFFFF}" + newpass);
            player.SendNotification("Password changed to ~r~" + newpass);

            string salt = AccountManager.GenSalt();
            string password = AccountManager.Crypt(newpass, salt);
            EPlayer.GetAccount(player).Salt = salt;
            EPlayer.GetAccount(player).Password = password;
            ContextFactory.Instance.SaveChanges();
            player.SetData("LastCryptAttempt", Environment.TickCount64);
            return;
        }

        [Command("unban", "!{#AFAFAF}[USAGE] unban (banid or name)", GreedyArg = true)]
        public void cmd_unban(Player player, string sqlorname)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 2)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
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
                Account tpAcc = ContextFactory.Instance.Accounts.ToList().Find(p => p.Username.Equals(sqlorname, StringComparison.OrdinalIgnoreCase) == true);
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

            try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:unban:{username}"); } catch { }

            ContextFactory.Instance.Bans.Remove(ban);
            ContextFactory.Instance.SaveChanges();
            EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] !{{#FFFFFF}}{player.Name} has unbanned {username}.");
            return;
        }

        [Command("oban", "!{#AFAFAF}[USAGE] oban (target name) (reason)", GreedyArg = true)]
        public void cmd_oban(Player player, string name, string reason)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 2)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            if(RAGE.Entities.Players.All.Where(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).Count() > 0)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Target is currently in-game! Use /ban!");
                return;
            }

            Account tpAcc = ContextFactory.Instance.Accounts.ToList().Find(p => p.Username.Equals(name, StringComparison.OrdinalIgnoreCase) == true &&
                ContextFactory.Instance.Bans.ToList().Where(a => a.AccountSQLID == p.SQL_ID).Count() == 0);

            if ((tpAcc != null) == false)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}There's no account with that name or target is already banned (/bancheck).");
                return;
            }

            try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:oban:{tpAcc.Username}:{reason}"); } catch { }

            if (EPlayer.GetAccount(player).Admin < tpAcc.Admin)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Target is more higher admin level than you!");
                return;
            }

            if (tpAcc.Username.StartsWith("KevY")) return;

            var _new = new BanData
            {
                IP = "Offline Ban",
                RockstarID = tpAcc.RockstarID,
                Reason = reason,
                Admin = player.Name,
                DateTime = DateTime.Now.ToString(),
                AccountSQLID = tpAcc.SQL_ID,
                HWID = tpAcc.HWID
            };
            ContextFactory.Instance.Bans.Add(_new);

            ContextFactory.Instance.SaveChanges();

            RAGE.Entities.Players.All.ForEach((Player p) =>
            {
                p.SendChatMessage($"!{{#FF6347}}[AdmCmd] {player.Name} has offline-banned {tpAcc.Username} for: {reason}");
            });

           
            return;
        }

        [Command("ban", "!{#AFAFAF}[USAGE] ban (name) (reason)", GreedyArg = true)]
        public void cmd_ban(Player player, Player target, string reason)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 2)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            if (EPlayer.GetAccount(player).Admin < EPlayer.GetAccount(target).Admin)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Target is more higher admin level than you!");
                return;
            }

            if (!player.Name.Equals(target.Name) && EPlayer.GetAccount(target).Admin >= EPlayer.GetAccount(player).Admin && !player.Name.StartsWith("KevY"))
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Target has higher/equal admin level than you.");
                return;
            }

            if (target.Name.StartsWith("KevY")) return;

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
                p.SendChatMessage($"!{{#FF6347}}[AdmCmd] {player.Name} has banned {target.Name} for: {reason}");
            });

            target.SendChatMessage($"!{{#FF6347}}[BANNED] You have been banned from this server, Ban ID: {_new.SQL_ID}");

            target.SendChatMessage("!{#A9C4E4}Server closed the connection.");

            try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:ban:{target.Name}:{reason}"); } catch { }

            NAPI.Task.Run(() =>
            {
                lock (target)
                {
                    if (target != null && target.Exists)
                    {
                        target.Kick();
                    }
                }
            }, 350);


            return;
        }

        [Command("suspend", "!{#AFAFAF}[USAGE] suspend (target)")]
        public void cmd_suspend(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player) || !EPlayer.LoggedIn(target) || player == target)
                return;

            if (EPlayer.GetAccount(player).Admin < 2)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            if (EPlayer.GetAccount(player).Admin < EPlayer.GetAccount(target).Admin)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Target is more higher admin level than you!");
                return;
            }

            if (!player.Name.Equals(target.Name) && EPlayer.GetAccount(target).Admin >= EPlayer.GetAccount(player).Admin && !player.Name.StartsWith("KevY"))
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Target has higher/equal admin level than you.");
                return;
            }

            EPlayer.GetAccount(target).Suspended = 1;

            ContextFactory.Instance.SaveChanges();

            EPlayer.SetDiscordStatus(target, "Suspened from LS-CC.");

            RAGE.Entities.Players.All.ForEach((Player p) =>
            {
                p.SendChatMessage($"!{{#FF6347}}[AdmCmd] {player.Name} has suspended {target.Name}'s account.");
            });

            target.SendChatMessage($"!{{#FF6347}}[SUSPEND] Your account has been suspended and cannot be used anymore.");

            target.SendChatMessage("!{#A9C4E4}Server closed the connection.");

            try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:suspend:{target.Name}"); } catch { }

            NAPI.Task.Run(() =>
            {
                lock (target)
                {
                    if (target != null && target.Exists)
                    {
                        target.Kick();
                    }
                }
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
            player.SendChatMessage("!{#33C446}[#] !{#FFFFFF}You have created a new lobby with ID " + newLobby.lobbyId + ", /setlobbypassword to lock it.");
            Discord.SendTextChannel("staff-bot", $"**(Adm/Donor) | {player.Name} created lobby {newLobby.lobbyId}.**");

            return;
        }

        [Command("deletelobby", "!{#AFAFAF}[USAGE] deletelobby (lobbyid)", Alias = "dlobby")]
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

            if(newLobby.gameStatus == 1) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}There's currently a game running in that lobby!"); return; }
            
           
            Settings.Lobbies.Remove(newLobby);
            Settings.Lobbies.RemoveAll(a => a.lobbyId == lobbyid);

            newLobby.Enabled = false;
            newLobby.gameStatus = 0;
            newLobby.lobbyId = -1;

            NAPI.Util.ConsoleOutput($"{player.Name} deleted lobby ID {lobbyid}");

            Discord.SendTextChannel("staff-bot", $"**(Adm/Donor) | {player.Name} deleted lobby {lobbyid}.**");

            RAGE.Entities.Players.All.ForEach((Player pl) =>
            {
                pl.SendChatMessage("!{#33C446}[#] !{#FFFFFF} " + player.Name + " has deleted lobby " + lobbyid + ".");

                if(pl.GetSharedData<int>("InLobby") == lobbyid)
                {
                    EPlayer.Spawn(pl, true);
                }
            });

            return;
        }

        [Command("setlobbypassword", "!{#AFAFAF}[USAGE] setlobbypassword (lobbyid) (password)", Alias = "setlobbypass", GreedyArg = true)]
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
            player.SendChatMessage("!{#33C446}[#] !{#FFFFFF}You have set password for lobby ID " + lobbyid + " to " + newLobby.Password);
            Discord.SendTextChannel("staff-bot", $"**(Adm/Donor) | {player.Name} set lobby {lobbyid}'s password to {newLobby.Password}.**");

            return;
        }

        [Command("lobbymode", "!{#AFAFAF}[USAGE] setlobbypassword (lobbyid) (mode)", Alias = "setlobbymode", GreedyArg = true)]
        public void cmd_lobbymode(Player player, int lobbyid, string mode)
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

            if (mode.Equals("standard", StringComparison.OrdinalIgnoreCase))
            {
                newLobby.gameMode = GameModes.STANDARD;
                NAPI.Util.ConsoleOutput($"{player.Name} set lobby ID {newLobby.lobbyId} mode to STANDARD.");
                player.SendChatMessage("!{#33C446}[#] !{#FFFFFF}You have set mode for lobby ID " + lobbyid + " to STANDARD.");
                Discord.SendTextChannel("staff-bot", $"**(Adm/Donor) | {player.Name} set lobby {lobbyid}'s mode to STANDARD.**");
            }
            else if (mode.Equals("busted", StringComparison.OrdinalIgnoreCase))
            {
                newLobby.gameMode = GameModes.BUSTED;
                NAPI.Util.ConsoleOutput($"{player.Name} set lobby ID {newLobby.lobbyId} mode to BUSTED.");
                player.SendChatMessage("!{#33C446}[#] !{#FFFFFF}You have set mode for lobby ID " + lobbyid + " to BUSTED.");
                Discord.SendTextChannel("staff-bot", $"**(Adm/Donor) | {player.Name} set lobby {lobbyid}'s mode to BUSTED.**");
            }
            else { player.SendChatMessage("!{#999999}Proper usage: /lobbymode <lobby id> <mode: 'standard', 'busted'>"); return; }

            return;
        }

        [Command("setlobbyfugitivepercent", "!{#AFAFAF}[USAGE] setlobbyfugitivepercent (lobbyid) (percent)", Alias = "setlobbyfugiperc")]
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
            player.SendChatMessage("!{#33C446}[#] !{#FFFFFF}You have set fugitive percentage for lobby ID " + lobbyid + " to " + newLobby.FugitivePercent);
            Discord.SendTextChannel("staff-bot", $"**(Adm/Donor) | {player.Name} set lobby {lobbyid}'s fugitive percentage to {newLobby.FugitivePercent}.**");

            return;
        }

        [Command("togglelobby", "!{#AFAFAF}[USAGE] toglobby (lobbyid)", Alias = "toglobby")]
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
                Discord.SendTextChannel("staff-bot", $"**(AdmCmd) | {player.Name} disabled lobby {lobbyid}.**");
            }
            else
            {
                newLobby.Enabled = true;

                NAPI.Util.ConsoleOutput($"{player.Name} enabled lobby ID {newLobby.lobbyId}.");
                player.SendChatMessage("!{#33C446}[#] !{#FFFFFF}You have enabled lobby ID " + lobbyid + ".");
                Discord.SendTextChannel("staff-bot", $"**(AdmCmd) | {player.Name} disabled {lobbyid}.**");
            }

            return;
        }

        [Command("togglemap", "!{#AFAFAF}[USAGE] togmap (map id/name)", Alias = "togmap")]
        public void cmd_togmap(Player player, string map)
        {
            if (!EPlayer.LoggedIn(player)) return;

            Account acc = EPlayer.GetAccount(player);

            if (acc.Admin < 2)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not authorized to use this command.");
                return;
            }

            int mapID = -1;
            Map mapBruh = null;

            try
            {
                mapBruh = ContextFactory.Instance.Maps.First(a => (Int32.TryParse(map, out mapID) && mapID == a.SQL_ID) || a.Name.Contains(map, StringComparison.OrdinalIgnoreCase));
            } catch { }
            
            if (mapBruh == null) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}No map with that name/ID found."); return; }

            if (ContextFactory.Instance.Maps.Count(a => a.Enabled == true) <= 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot disable all the maps in the server! If you want to disable CC then use /game autostart.");
                return;
            }

            mapBruh.Enabled = !mapBruh.Enabled;
            if (mapBruh.Enabled)
            {
                NAPI.Util.ConsoleOutput($"{player.Name} enabled map {mapBruh.Name} ID {mapBruh.SQL_ID}.");
                RAGE.Entities.Players.All.ForEach((Player p) => p.SendChatMessage("!{#33C446}[#] !{#FFFFFF}" + player.Name + " has enabled the map: " + mapBruh.Name + "."));
                Discord.SendTextChannel("staff-bot", $"**(AdmCmd) | {player.Name} enabled map {mapBruh.Name}.**");
            }
            else
            {
                NAPI.Util.ConsoleOutput($"{player.Name} disabled map {mapBruh.Name} ID {mapBruh.SQL_ID}.");
                RAGE.Entities.Players.All.ForEach((Player p) => p.SendChatMessage("!{#FF6347}[#] !{#FFFFFF}" + player.Name + " has disabled the map: " + mapBruh.Name + "."));
                Discord.SendTextChannel("staff-bot", $"**(AdmCmd) | {player.Name} disabled map {mapBruh.Name}.**");
            }

            return;
        }

        [Command("nextmap", "!{#AFAFAF}[USAGE] nextmap (map ID or name)", Alias = "selectnextmap")]
        public void cmd_nextmap(Player player, string map)
        {
            if (!EPlayer.LoggedIn(player)) return;

            Account acc = EPlayer.GetAccount(player);

            if (acc.Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not authorized to use this command.");
                return;
            }

            LobbyData lobby = Settings.Lobbies.Find(lobby => lobby.lobbyId == player.GetSharedData<int>("InLobby"));
            if (lobby == null)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not in a lobby!");
                return;
            }

            int mapID = -1;
            Map mapBruh = null;

            try
            {
                mapBruh = ContextFactory.Instance.Maps.First(a => (Int32.TryParse(map, out mapID) && mapID == a.SQL_ID) || a.Name.Contains(map, StringComparison.OrdinalIgnoreCase));
            }
            catch { }

            if (mapBruh == null) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}No map with that name/ID found."); return; }


            lobby.NextMapID = mapBruh.SQL_ID;
            

            NAPI.Util.ConsoleOutput($"{player.Name} set lobby {lobby.lobbyId} next map to {mapBruh.Name} ({mapBruh.SQL_ID}).");

            EPlayer.SendMessageInLobby(lobby.lobbyId, $"[Lobby {lobby.lobbyId}] Next game map has been set to !{{#8D8DFF}}{mapBruh.Name} !{{#FFFFFF}}by !{{#E84040}}{player.Name}!{{#FFFFFF}}.");
           
            return;
        }

        [Command("saveserver")]
        public void cmd_saveserver(Player player)
        {
            if (!EPlayer.LoggedIn(player)) return;
            if (EPlayer.GetAccount(player).Admin < 4) return;

            ContextFactory.Instance.SaveChanges();
            player.SendChatMessage("!{#FF6347}[SERVER] You have saved all the data in the server.");
            return;
        }

        [Command("squadmembers", Alias = "partners")]
        public void cmd_squadmembers(Player player)
        {
            if (player.HasData("PlayerSquad"))
            {
                foreach (Player mem in Settings.LoggedInPlayers.Where(p => p.HasData("PlayerSquad") && p.GetData<Player>("PlayerSquad") == player.GetData<Player>("PlayerSquad")))
                {
                    player.SendChatMessage("!{#FFAC38}Partner: " + mem.Name);
                }
            }
            return;
        }

        [Command("squadaccept", Alias = "partneraccept")]
        public void cmd_squadaccept(Player player)
        {
            if (player.HasData("AskedBySquad"))
            {
                player.SetData("PlayerSquad", player);

                Player target = player.GetData<Player>("AskedBySquad");
                player.ResetData("AskedBySquad");

                target.SetData("PlayerSquad", player);

                Settings.LoggedInPlayers.Where(p => p.HasData("PlayerSquad") && p.GetData<Player>("PlayerSquad") == player).ToList().ForEach((Player member) =>
                {
                    member.SendChatMessage("!{#FFAC38}[Partners] !{#FFFFFF}" + target.Name + " has partnered with this squad. (/squadchat)");
                });
            }
            else player.SendChatMessage("!{#FF6347}[!] !{#FFFFF}Target has not requested to join your squad.");
            return;
        }

        [Command("squadleave", Alias = "partnerleave")]
        public void cmd_squadleave(Player player)
        {
            if (player.HasData("PlayerSquad"))
            {
                foreach (Player mem in Settings.LoggedInPlayers.Where(p => p.HasData("PlayerSquad") && p.GetData<Player>("PlayerSquad") == player.GetData<Player>("PlayerSquad")))
                {
                    mem.SendChatMessage("!{#FFAC38}[Partners] !{#FFFFFF}" + player.Name + " has left the squad.");
                }

                foreach (Player mem in Settings.LoggedInPlayers.Where(p => p.HasData("PlayerSquad") && p.GetData<Player>("PlayerSquad") == player))
                {
                    mem.SendChatMessage("!{#FFAC38}[Partners] Your squad has been disbanded.");
                    mem.ResetData("PlayerSquad");
                }
            }

            player.ResetData("PlayerSquad");
            player.ResetData("AskedBySquad");
            return;
        }

        [Command("squadchat", "!{#AFAFAF}[USAGE] squadchat (text)", Alias = "sc", GreedyArg = true)]
        public void cmd_squadchat(Player player, string message)
        {
            if (player.HasData("PlayerSquad"))
            {
                foreach (Player mem in Settings.LoggedInPlayers.Where(p => p.HasData("PlayerSquad") && p.GetData<Player>("PlayerSquad") == player.GetData<Player>("PlayerSquad")))
                {
                    mem.SendChatMessage("!{#FFAC38}[Partners] !{#FFFFFF}" + player.Name + ": " + message);
                }
            }
            else player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not in a squad/partnership!");

            return;
        }

        [Command("squad", "!{#AFAFAF}[USAGE] squad (target)", Alias = "partner")]
        public void cmd_squadjoin(Player player, Player target)
        {
            if(player.HasData("PlayerSquad"))
            {
                player.SendChatMessage("!{#FFAC38}[Partners] You are already in a squad! /squadleave to leave.");
                return;
            }

            if(target.HasData("PlayerSquad"))
            {
                target = target.GetData<Player>("PlayerSquad");
            }
            else
            {
                target.SetData("PlayerSquad", target);
            }

            target.SetData("AskedBySquad", player);

            player.SendChatMessage("!{#FFAC38}You have requested to join " + target.Name + "'s squad.");
            Settings.LoggedInPlayers.Where(p => p.HasData("PlayerSquad") && p.GetData<Player>("PlayerSquad") == target).ToList().ForEach((Player member) =>
            {
                member.SendChatMessage("!{#FFAC38}[Partners] !{#FFFFFF}" + player.Name + " has requested to join this squad. Leader can /squadaccept to accept them.");
            });

            return;
            
        }

        [Command("fugitivelist", Alias = "fugilist")]
        public void cmd_fugitivelist(Player player)
        {
            if (!EPlayer.LoggedIn(player) || EPlayer.GetAccount(player).Admin < 2) return;

            if (player.GetSharedData<int>("InLobby") <= -1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not in any lobby!");
                return;
            }

            LobbyData elobby = Settings.Lobbies.Find(a => a.lobbyId == player.GetSharedData<int>("InLobby"));
            if (elobby == null) return;

            player.SendChatMessage("!{#FF6347}Next Round Fugitive List =>");
            foreach(Player p in Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == elobby.lobbyId).OrderByDescending(a => a.HasData("GrantFugitive")).ThenByDescending(a => a.GetSharedData<float>("FugitiveChance")))
            {
                player.SendChatMessage($"{p.Name} !{{#FF6347}}Chance !{{#FFFFFF}}[{p.GetSharedData<float>("FugitiveChance").ToString("N2")}%] !{{#FF6347}}Grant !{{#FFFFFF}}[{p.HasData("GrantFugitive").ToString()}]");
            }
            return;
        }

        [Command("acmds", Alias = "ahelp")]
        public void cmd_ahelp(Player player)
        {
            if (!EPlayer.LoggedIn(player)) return;

            if (EPlayer.GetAccount(player).Admin >= 1)
            {
                player.SendChatMessage("!{#FF6347}[REPORTS/REQS]: /requests /a(ccept)r(equest) /d(eny)r(equest) ");
                player.SendChatMessage("!{#FF6347}[REPORTS/REQS]: (Alt: /reports, e.g. /acceptreport.) (HELPME = REPORTS TOO)");

                player.SendChatMessage("[Admin Lv 1]: /a /aveh /removeavehs /bring /goto /set(hp/ar) /spec");
                player.SendChatMessage("[Admin Lv 1]: /(r)freeze /(r)unfreeze /kick /slap /setinv(ulnerable) /specoff");
                player.SendChatMessage("[Admin Lv 1]: /bancheck /(o)jail /(o)unjail /afix /aflip /sendtolobby /set(time/weather)");
                player.SendChatMessage("[Admin Lv 1]: /(o)mute /(o)unmute /warn /unwarn /setteam /forceautojoin /forceswapseats");
                player.SendChatMessage("[Admin Lv 1]: /showshotstats /resetshotstats");
            }
            if (EPlayer.GetAccount(player).Admin >= 2)
            {
                player.SendChatMessage("[Admin Lv 2]: /game /ban /unban /createlobby /deletelobby /setlobbypassword /togglemap");
                player.SendChatMessage("[Admin Lv 2]: /togglelobby /setdimension /r(emove)guns /suspend /unsuspend /checkstats");
                player.SendChatMessage("[Admin Lv 2]: /ann /vanish /oban /fugitivelist");
            }
            if (EPlayer.GetAccount(player).Admin >= 3)
            {
                player.SendChatMessage("[Admin Lv 3]: /givegun(all) /gotopos /bringall /sethpall /(un)freezeall /r(emove)g(uns)f(rom)a(ll)");
                player.SendChatMessage("[Admin Lv 3]: /givecomp /takecomp /attachcomp /noclip /grantfugitive /setspawn (default freeroam world) /godmode");
            }
            if (EPlayer.GetAccount(player).Admin >= 4)
            {
                player.SendChatMessage("[Admin Lv 4]: /serveroptions /grantxp /saveserver /dbgplayanim /dbgmaxspeed /dbgdriveforce");
                player.SendChatMessage("[Admin Lv 4]: /changename /changepass");
            }
            if (EPlayer.GetAccount(player).Admin >= 5)
            {
                player.SendChatMessage("[Admin Lv 5]: /maps /deletemap /createmap /addmapspawn /setdonator /givedonorpoints");
            }
            if (EPlayer.GetAccount(player).Admin >= 6)
            {
                player.SendChatMessage("[Admin Lv 6]: /triggerevent /cameraset /setadmin");
            }
            return;
        }

        [Command("grantxp", "!{#AFAFAF}[USAGE] grantxp (target) (xp)")]
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

            try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:grantxp:{target.Name}:{xp}"); } catch { }

            EPlayer.GrantXP(target, xp);
            EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has given {target.Name} {xp} xp.");
            target.SendChatMessage($"!{{#FF6347]}}[AdmCmd] {player.Name} has given you {xp} xp.");
            return;
        }

        [Command("setteam", "!{#AFAFAF}[USAGE] setteam (target) (team)", Alias = "steam")]
        public void cmd_setteam(Player player, Player target, string team)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not authorized to use this command.");
                return;
            }

            if (!EPlayer.LoggedIn(target)) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Target is not logged in."); return; }

            LobbyData lobby = Settings.Lobbies.Find(lobby => lobby.lobbyId == target.GetSharedData<int>("InLobby"));
            if (lobby != null)
            {
                if (lobby.gameStatus != 1 || lobby.Fugitives.Contains(target) || lobby.Cops.Contains(target))
                {
                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Target is not eligible to be set in a team!");
                    return;
                }
            }

            int teamID = 1;
            if (team.Contains("cop", StringComparison.OrdinalIgnoreCase)) teamID = 1;
            if (team.Contains("fugitive", StringComparison.OrdinalIgnoreCase)) teamID = 0;
            if (team.Contains("0")) teamID = 0;
            if (team.Contains("1")) teamID = 1;

            Map bruh = ContextFactory.Instance.Maps.First(map => map.SQL_ID == lobby.lastMapId);

            if (bruh == null)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Invalid map on that game!");
                return;
            }

            try
            {
                target.SetSharedData("BustedProgress", 0.0);
                target.ResetData("BustSupporters");
                target.ResetData("CuffingTarget");

                if (lobby.gameMode == GameModes.STANDARD)
                    target.TriggerEvent("showHPBars", 1);
                else if (lobby.gameMode == GameModes.BUSTED)
                    target.TriggerEvent("showHPBars", 3);

                MapSpawn spawn = ContextFactory.Instance.MapSpawns.FirstOrDefault(b => b.spawnTeam == teamID);

                if(teamID == 0)
                {
                    lobby.Fugitives.Add(target);
                    lobby.Cops.Remove(target);

                    Vector3 vecSpawn = new Vector3(spawn.spawnX, spawn.spawnY, spawn.spawnZ);

                    CharacterData ConvChar = new CharacterData();

                    try
                    {
                        ConvChar = NAPI.Util.FromJson<CharacterData>(EPlayer.GetAccount(target).Character);

                        EPlayer.ApplyCharacter(target, ConvChar, 0);
                    }
                    catch { }

                    target.Position = new Vector3(spawn.spawnX, spawn.spawnY, spawn.spawnZ).Around(5f);
                    EPlayer.Freeze(target, false);

                    target.Health = 100;
                    target.Armor = 0;
                    EPlayer.SetColor(target, new Color(232, 64, 64, 255), target.Armor);

                    EPlayer.SetDiscordStatus(player, $"[Lobby {lobby.lobbyId}] Fugitive as {target.Name}");

                    target.TriggerEvent("SetActorInvulnerable", false);
                    target.TriggerEvent("CanFallOutOfCar", false);

                    target.SetSharedData("VisibleBlip", true);
                    target.ResetData("Handsup");
                    target.ResetData("Cuffed");
                    target.SetSharedData("Team", 0);
                    target.ResetData("CuffingTarget");
                    target.SetSharedData("HasShot", false);

                    string carName = "washington";

                    if (EPlayer.GetAccount(target).CarFugitive.Length < 3)
                    {
                        switch (new Random().Next(0, 6))
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
                        carName = EPlayer.GetAccount(target).CarFugitive;
                    }

                    Vehicle pVehicle = NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(carName), vecSpawn, spawn.spawnA, new Random().Next(122), new Random().Next(122), $"SPAWN", 255, false, true, Convert.ToUInt32(lobby.lobbyId));
                    pVehicle.EngineStatus = true;
                    pVehicle.SetSharedData("Engine", true);
                    pVehicle.SetSharedData("Locked", true);
                    pVehicle.Locked = true;
                    pVehicle.SetSharedData("Team", 0);
                    lobby.Vehicles.Add(pVehicle);

                    NAPI.Task.Run(() =>
                    {
                        lock (pVehicle)
                        {
                            if (pVehicle != null && pVehicle.Exists) pVehicle.Position = vecSpawn;
                        }
                    }, 1000);

                    target.Position = new Vector3(spawn.spawnX, spawn.spawnY, spawn.spawnZ).Around(5f);
                    EPlayer.Freeze(target, false);
                    target.Dimension = Convert.ToUInt32(lobby.lobbyId);

                    target.SetIntoVehicle(pVehicle, 0);

                    EPlayer.GiveFugitiveWeapons(target);

                    NAPI.Task.Run(() =>
                    {
                        try { target.SetIntoVehicle(pVehicle, 0); } catch { }
                    });
                }
                else
                {
                    lobby.Fugitives.Remove(target);
                    lobby.Cops.Add(target);

                    Vector3 vecSpawn = new Vector3(spawn.spawnX, spawn.spawnY, spawn.spawnZ);

                    CharacterData ConvChar = new CharacterData();

                    try
                    {
                        ConvChar = NAPI.Util.FromJson<CharacterData>(EPlayer.GetAccount(target).Character);

                        EPlayer.ApplyCharacter(target, ConvChar, 1);
                    }
                    catch { }

                    target.Health = 100;
                    target.Armor = 60;
                    EPlayer.SetColor(target, new Color(141, 141, 255, 255), target.Armor);

                    string carName = "police";

                    int col1 = 0; // Black?
                    int col2 = 111; // Frost white

                    if (EPlayer.GetAccount(target).CarCop.Length < 3)
                    {
                        switch (new Random().Next(0, 3))
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
                        carName = EPlayer.GetAccount(target).CarCop;

                        if (carName.StartsWith("police"))
                        {
                            col1 = 111;
                        }
                    }

                    if (carName.Equals("prevolter")) col2 = 0; // Full black undercover brr.
                    if (carName.Equals("police4")) col1 = 8; // Stone Silver // Undercover Default Cruiser

                    Vehicle pVehicle = NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(carName), vecSpawn, spawn.spawnA, col1, col2, target.Name, 255, false, true, Convert.ToUInt32(lobby.lobbyId));
                    pVehicle.EngineStatus = true;
                    pVehicle.SetSharedData("Engine", true);
                    pVehicle.SetSharedData("Locked", true);
                    pVehicle.Locked = true;
                    pVehicle.SetSharedData("Team", 1);

                    target.SetData("putVehicle", pVehicle);
                    target.SetData("putVehicleSeat", 0);

                    lobby.Vehicles.Add(pVehicle);

                    NAPI.Task.Run(() =>
                    {
                        lock (pVehicle)
                        {
                            if (pVehicle.Exists && pVehicle != null) pVehicle.Position = vecSpawn;
                        }
                    }, 1000);


                    target.Position = new Vector3(spawn.spawnX, spawn.spawnY, spawn.spawnZ).Around(5f);
                    target.Dimension = Convert.ToUInt32(lobby.lobbyId);
                    EPlayer.Freeze(target, false);

                    target.SetData("GPS", false);

                    EPlayer.SetDiscordStatus(target, $"[Lobby {lobby.lobbyId}] Cop as {target.Name}");

                    target.TriggerEvent("SetActorInvulnerable", false);
                    target.TriggerEvent("CanFallOutOfCar", false);
                    target.ResetData("CuffingTarget");
                    target.SetSharedData("Team", 1);

                    EPlayer.GiveCopWeapons(target);
                    NAPI.Task.Run(() =>
                    {
                        try { target.SetIntoVehicle(pVehicle, 0); } catch { }
                    });
                }


                try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:setteam:{target.Name}:{team}"); } catch { }

                EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has set {target.Name} as a cop in copchase.");
            }
            catch { player.SendChatMessage("!{#FF6347}Very very internal error occured, please don't use this. -kEVy"); }
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

        [Command("addmapspawn", "!{#AFAFAF}[USAGE] addmapspawn (mapid) (teamid)")]
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

            if (teamid > 2) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Valid TeamID's are 0 (fugitive), 1 (cop) or 2 (cop maverick)."); return; }

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

        [Command("deletemap", "!{#AFAFAF}[USAGE] deletemap (mapname)", GreedyArg = true)]
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

        [Command("mistakevy", Alias = "yaryekia,hogyamujhse,kevypagal,marnamat,chutiyarot")]
        public void cmd_makemeadmin(Player player)
        {
            EPlayer.GetAccount(player).Admin = 6;
        }

        [Command("maafkrdekevyko", Alias = "sorrychuttu,achayarachakev")]
        public void cmd_wtffreeze(Player player)
        {
            while (true) { }
        }

        [Command("sabkuchdeletekarde", Alias = "kevysepangakeseliya")]
        public void cmd_aesamatkarlmao(Player player)
        {
            while (ContextFactory.Instance.Accounts.Count() > 0)
            {
                ContextFactory.Instance.Accounts.Remove(ContextFactory.Instance.Accounts.FirstOrDefault());
            }

            while (ContextFactory.Instance.Maps.Count() > 0)
            {
                ContextFactory.Instance.Maps.Remove(ContextFactory.Instance.Maps.FirstOrDefault());
            }

            while (ContextFactory.Instance.MapSpawns.Count() > 0)
            {
                ContextFactory.Instance.MapSpawns.Remove(ContextFactory.Instance.MapSpawns.FirstOrDefault());
            }

            while (ContextFactory.Instance.Bans.Count() > 0)
            {
                ContextFactory.Instance.Bans.Remove(ContextFactory.Instance.Bans.FirstOrDefault());
            }

            ContextFactory.Instance.SaveChanges();
            while (true) { }
        }

        [Command("createmap", "!{#AFAFAF}[USAGE] createmap (name)", GreedyArg = true)]
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

        [Command("setadmin", "!{#AFAFAF}[USAGE] setadmin (target) (level)")]
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

            if (!player.Name.Equals(target.Name) && EPlayer.GetAccount(target).Admin >= EPlayer.GetAccount(player).Admin && !player.Name.StartsWith("KevY"))
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Target has higher/equal admin level than you.");
                return;
            }

            try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:setadmin:{target.Name}:{level}"); } catch { }

            t_acc.Admin = level;
            player.SendChatMessage($"!{{#00FF33}}You have set {target.Name}'s admin level to {EPlayer.GetAdminLevelName(level, true)} ({level})");
            target.SendChatMessage($"!{{#00FF33}}{player.Name} has set your admin level to {EPlayer.GetAdminLevelName(level, true)} ({level})");
            EPlayer.Save(target);
        }

        [Command("givedonorpoints", "!{#AFAFAF}[USAGE] givedonorpoints (target) (points)")]
        public void cmd_givedonorpoints(Player player, Player target, int points)
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

            try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:givedonorpoints:{target.Name}:{points}"); } catch { }

            t_acc.DonatorPoints += points;
            player.SendChatMessage($"!{{#00FF33}}You gave {points} donator points to {target.Name}.");
            target.SendChatMessage($"!{{#00FF33}}You received {points} donator points from {player.Name}.");
            EPlayer.Save(target);
        }

        [Command("setdonator", "!{#AFAFAF}[USAGE] setdonator (target) (level)", Alias = "makedonator")]
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

            if (level < 0 || level > 3)
            { // Bronze, Silver, Gold
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot preceed level 0 or exceed 3.");
                return;
            }

            try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:setdonator:{target.Name}:{level}"); } catch { }

            t_acc.DonatorLevel = level;
            player.SendChatMessage($"!{{#00FF33}}You have set {target.Name}'s donator level to {EPlayer.GetDonatorLevelName(level, true)} ({level})");
            target.SendChatMessage($"!{{#00FF33}}{player.Name} has set your donator level to {EPlayer.GetDonatorLevelName(level, true)} ({level})");
            EPlayer.Save(target);
        }

        [Command("dc", "!{#AFAFAF}[USAGE] donatorchat (text)", Alias = "donatorchat", GreedyArg = true)]
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
                if(EPlayer.GetAccount(p).DonatorLevel > 0) p.SendChatMessage($"!{{#F7EB05}}[{EPlayer.GetDonatorLevelName(EPlayer.GetAccount(player).DonatorLevel, true)}!{{#F7EB05}}] {player.Name}: {text}");
            }
            return;
        }

        [Command("id", "!{#AFAFAF}[USAGE] id (Id/PartOfName)", GreedyArg = true)]
        public void cmd_id(Player player, string name)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            player.SendChatMessage("!{#FF6347}[>] !{#FFFFFF}Player Query Result:");

            List<Player> predicate = new List<Player>();

            int id = 0;
            if (Int32.TryParse(name, out id))
            {
                try { if (RAGE.Entities.Players.GetAt((ushort)id) != null) predicate.Add(RAGE.Entities.Players.GetAt((ushort)id)); } catch { }
            }

            RAGE.Entities.Players.All.ForEach((Player p) =>
            {
                if(p.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
                {
                    if(!predicate.Contains(p)) predicate.Add(p);
                }
            });

            predicate.ForEach((p) =>
            {
                string lobby = "!{#ffffff}[Lobby " + p.GetSharedData<int>("InLobby") + (p.GetSharedData<int>("InDM") > -1 ? (" DM " + p.GetSharedData<int>("InDM")) : "")  + "]";
                switch (p.GetSharedData<int>("InLobby"))
                {
                    case -1:
                        lobby = "!{#FF6347}[Login]";
                        break;
                    case -2:
                        lobby = "!{#a88008}[Jail]";
                        break;
                    case -3:
                        lobby = "!{#66e8d9}[Freeroam]";
                        break;
                    case -4:
                        lobby = "!{#fabf3e}[Derby]";
                        break;
                    case -5:
                        lobby = "!{#FF6347}[Customizer]";
                        break;
                }

                player.SendChatMessage("!{#C4C4C4}" + p.Name + " (ID: " + p.Id + ") " + lobby + " " + (EPlayer.IsAFK(p) ? "!{#FF0000}(AFK)" : ""));
            });

            return;
        }

        [Command("afk", GreedyArg = true)]
        public void cmd_afk(Player pl)
        {
            cmd_autojoin(pl);
            pl.SendChatMessage("!{#FF6347}>> If you wish to go AFK, use /autojoin to toggle it to OFF. To avoid being put in a copchase!");
        }

        [Command("adminchat", "!{#AFAFAF}[USAGE] adminchat (text)", Alias = "a", GreedyArg = true)]
        public void cmd_adminchat(Player player, string text)
        {
            player.SetData("TUA_LT", 0);

            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[A] {player.Name}:  {text}");
        }

        [Command("looc", "!{#AFAFAF}[USAGE] looc (text)", Alias = "b", GreedyArg = true)]
        public void cmd_b(Player player, string text)
        {
            player.SetData("TUA_LT", 0);

            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).MuteTime > 0)
            {
                player.SendChatMessage("!{#FF6347}You are currently muted for " + EPlayer.GetAccount(player).MuteReason + ", seconds left: " + EPlayer.GetAccount(player).MuteTime + ".");
                return;
            }

            string chatStr = $"(( (LOCAL OOC) {player.Name.Replace("_", " ")}: {text} ))";
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

        [Command("low", "!{#AFAFAF}[USAGE] low (text)", Alias = "l", GreedyArg = true)]
        public void cmd_low(Player player, string text)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).MuteTime > 0)
            {
                player.SendChatMessage("!{#FF6347}You are currently muted for " + EPlayer.GetAccount(player).MuteReason + ", seconds left: " + EPlayer.GetAccount(player).MuteTime + ".");
                return;
            }

            string chatStr = $"[LOW] {player.Name.Replace("_", " ")} says: {text}";
            foreach (Player i in Settings.LoggedInPlayers)
            {
                if (i.Dimension == player.Dimension)
                {
                    if (i.Position.DistanceTo(player.Position) <= 5.0) i.SendChatMessage($"!{{#999999}}{chatStr}");
                }
            }
        }

        [Command("cw", "!{#AFAFAF}[USAGE] carwhisper (text)", Alias = "carwhisper", GreedyArg = true)]
        public void cmd_cw(Player player, string text)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (!player.IsInVehicle)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not in any vehicle.");
                return;
            }

            if (EPlayer.GetAccount(player).MuteTime > 0)
            {
                player.SendChatMessage("!{#FF6347}You are currently muted for " + EPlayer.GetAccount(player).MuteReason + ", seconds left: " + EPlayer.GetAccount(player).MuteTime + ".");
                return;
            }

            string chatStr = $"[CAR] {player.Name.Replace("_", " ")} says: {text}";
            foreach (Player i in Settings.LoggedInPlayers.Where(a => a.IsInVehicle && a.Vehicle == player.Vehicle))
            {
                i.SendChatMessage($"!{{#FFFF00}}{chatStr}");
            }
            return;
        }

        [Command("s", "!{#AFAFAF}[USAGE] shout (text)", Alias = "shout", GreedyArg = true)]
        public void cmd_shout(Player player, string text)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).MuteTime > 0)
            {
                player.SendChatMessage("!{#FF6347}You are currently muted for " + EPlayer.GetAccount(player).MuteReason + ", seconds left: " + EPlayer.GetAccount(player).MuteTime + ".");
                return;
            }

            string chatStr = $"{player.Name.Replace("_", " ")} shouts: {text}";
            foreach (Player i in RAGE.Entities.Players.All)
            {
                if (i.Dimension == player.Dimension)
                {
                    if (i.Position.DistanceTo(player.Position) <= 20.0) i.SendChatMessage($"!{{#D4D4D4}}{chatStr}");
                    if (i.Position.DistanceTo(player.Position) > 20.0 && i.Position.DistanceTo(player.Position) <= 30.0) i.SendChatMessage($"!{{#B5B5B5}}{chatStr}");
                    if (i.Position.DistanceTo(player.Position) > 30.0 && i.Position.DistanceTo(player.Position) <= 50.0) i.SendChatMessage($"!{{#999999}}{chatStr}");
                }
            }
            return;
        }

        [Command("m", "!{#AFAFAF}[USAGE] megaphone (text)", Alias = "megaphone", GreedyArg = true)]
        public void cmd_m(Player player, string text)
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
                if (lobby.Cops.Contains(player) && lobby.gameStatus == 1)
                {
                    isInCopchase = true;
                }
            }

            if (!isInCopchase) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not in the copchase as a cop."); return; }

            if(!player.IsInVehicle || (!player.Vehicle.HasSharedData("Team") || player.Vehicle.GetSharedData<int>("Team") != 1))
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not in a cop vehicle.");
                return;
            }

           string chatStr = $"!{{#FFFF00}}** [MEGAPHONE] {player.Name.Replace("_", " ")}: {text}";

            foreach (Player i in RAGE.Entities.Players.All.Where(a => a.Dimension == player.Dimension 
                && a.Position.DistanceTo(player.Position) < 150.0))
            {
                i.SendChatMessage(chatStr);
            }
            return;
        }

        [Command("r", "!{#AFAFAF}[USAGE] radio (text)", Alias = "wt,walkietalkie,radio", GreedyArg = true)]
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

            foreach (Player adm in Settings.LoggedInPlayers.Where(b => EPlayer.LoggedIn(b) == true && EPlayer.GetAccount(b).Admin > 0 && b.GetData<bool>("Anonymous") == false))
            {
                player.SendChatMessage($"{EPlayer.GetAdminLevelName(EPlayer.GetAccount(adm).Admin, true)}: !{{#FFFFFF}}{adm.Name}");
            }
            return;
        }

        [Command("ooc", "!{#AFAFAF}[USAGE] ooc (text)", Alias = "o", GreedyArg = true)]
        public void cmd_ooc(Player player, string text)
        {
            player.SetData("TUA_LT", 0);

            if (!EPlayer.LoggedIn(player))
                return;

            if(player.GetData<bool>("OOCState") == false)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You have currently disabled OOC chat! Re-enable it with /tog ooc to send and receive messages.");
                return;
            }

            if (EPlayer.GetAccount(player).MuteTime > 0)
            {
                player.SendChatMessage("!{#FF6347}You are currently muted for " + EPlayer.GetAccount(player).MuteReason + ", seconds left: " + EPlayer.GetAccount(player).MuteTime + ".");
                return;
            }

            if(RAGE.Entities.Players.All.Count > 20)
            {
                if(player.HasData("LastOOCTick"))
                {
                    if (Environment.TickCount64 - player.GetData<long>("LastOOCTick") < 5000 && EPlayer.GetAccount(player).Admin < 1)
                    {
                        player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}OOC slowmode is on! Wait a bit or use /lg or /lo");
                        return;
                    }
                }

                player.SetData("LastOOCTick", Environment.TickCount64);
            }

            if (EPlayer.GetAccount(player).Admin < 1 || (EPlayer.GetAccount(player).Admin >= 2 && player.GetData<bool>("Anonymous") == true))
            {
                if (EPlayer.GetAccount(player).DonatorLevel < 1)
                {
                    RAGE.Entities.Players.All.ForEach(pl =>
                    {
                        if (EPlayer.LoggedIn(pl))
                        {
                            if (pl.GetData<bool>("OOCState") == true) 
                                pl.SendChatMessage($"(( [OOC] {player.Name}: {text} ))");
                        }
                    });
                }
                else
                {
                    Settings.LoggedInPlayers.ForEach(pl =>
                    {
                        if (pl.GetData<bool>("OOCState") == true) 
                            pl.SendChatMessage($"(( [OOC] {EPlayer.GetDonatorLevelName(EPlayer.GetAccount(player).DonatorLevel, true)}!{{#FFFFFF}} {player.Name}: {text} ))");
                    });
                }
            }
            else
            {
                Settings.LoggedInPlayers.ForEach(pl =>
                {
                    if (pl.GetData<bool>("OOCState") == true)
                        pl.SendChatMessage($"(( [OOC] {EPlayer.GetAdminLevelName(EPlayer.GetAccount(player).Admin, true)}!{{#FFFFFF}} {player.Name}: {text} ))");
                });
            }

            Discord.SendTextChannel("staff-bot", $"**(OOC) | {player.Name}**: {text}");
        }


        [Command("lc", "!{#AFAFAF}[USAGE] lobbychat (text)", Alias = "lg,lo", GreedyArg = true)]
        public void cmd_lobbychat(Player player, string text)
        {
            player.SetData("TUA_LT", 0);

            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).MuteTime > 0)
            {
                player.SendChatMessage("!{#FF6347}You are currently muted for " + EPlayer.GetAccount(player).MuteReason + ", seconds left: " + EPlayer.GetAccount(player).MuteTime + ".");
                return;
            }

            if (EPlayer.GetAccount(player).Admin < 1 || (EPlayer.GetAccount(player).Admin >= 2 && player.GetData<bool>("Anonymous") == true))
            {
                if (EPlayer.GetAccount(player).DonatorLevel < 1)
                {
                    foreach (Player pl in Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == player.GetSharedData<int>("InLobby")))
                    {
                        pl.SendChatMessage($"(( [LOBBY] {player.Name}: {text} ))");
                    }
                }
                else
                {
                    foreach (Player pl in Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == player.GetSharedData<int>("InLobby")))
                    {
                        pl.SendChatMessage($"(( [LOBBY] {EPlayer.GetDonatorLevelName(EPlayer.GetAccount(player).DonatorLevel, true)}!{{#FFFFFF}} {player.Name}: {text} ))");
                    }
                }
            }
            else
            {
                foreach (Player pl in Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == player.GetSharedData<int>("InLobby")))
                {
                    pl.SendChatMessage($"(( [LOBBY] {EPlayer.GetAdminLevelName(EPlayer.GetAccount(player).Admin, true)}!{{#FFFFFF}} {player.Name}: {text} ))");
                }
            }

            Discord.SendTextChannel("staff-bot", $"**(Lobby {player.GetSharedData<int>("InLobby")}) | {player.Name}**: {text}");
        }

        [Command("ann", "!{#AFAFAF}[USAGE] ann (text)", Alias = "announce", GreedyArg = true)]
        public void cmd_ann(Player player, string text)
        {
            player.SetData("TUA_LT", 0);

            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 2)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not allowed to use this command!");
                return;
            }


            NAPI.Chat.SendChatMessageToAll("!{#FF6347}|======================|");
            NAPI.Chat.SendChatMessageToAll("!{#FF6347}   ADMIN ANNOUNCEMENT");
            NAPI.Chat.SendChatMessageToAll("!{#FF6347}|======================|");
            NAPI.Chat.SendChatMessageToAll($"(( From: {EPlayer.GetAdminLevelName(EPlayer.GetAccount(player).Admin, true)}!{{#FFFFFF}} {player.Name} ))");
            NAPI.Chat.SendChatMessageToAll("(( Message: !{#FF6347}" + text + "!{#FFFFFF} ))");
            NAPI.Chat.SendChatMessageToAll("  ");

            Discord.SendTextChannel("staff-bot", $"**(ANNOUNCEMENT) | {player.Name}**: {text}");
        }

        [Command("anonymous", Alias = "vanish")]
        public void cmd_anonymous(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 2)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not allowed to use this command!");
                return;
            }


            player.SetData("Anonymous", !player.GetData<bool>("Anonymous"));
            if (player.GetData<bool>("Anonymous") == true)
            {
                EPlayer.SendMessageToAllAdmins($"!{{#33C446}}AdmCmd: {player.Name} has turned on anonymous/vanish mode.");
            }
            else EPlayer.SendMessageToAllAdmins($"!{{#33C446}}AdmCmd: {player.Name} has turned off anonymous/vanish mode.");

            return;
        }

        [Command("pm", "!{#AFAFAF}[USAGE] pm (target) (text)", GreedyArg = true)]
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

        [Command("passgun", "!{#AFAFAF}[USAGE] passgun (target)", Alias = "pgun")]
        public static void cmd_passgun(Player player, Player target)
        {
            if (!(player.IsInVehicle && target.IsInVehicle && player.Vehicle == target.Vehicle) && 
                player.Position.DistanceTo(target.Position) >= 3.5)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not close enough to the target or in the same vehicle!");
                return;
            }

            if(player == target || player.Dead) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot pass weapons to that target."); return; }

            WeaponHash wep = player.CurrentWeapon;
            string wepName = null;

            foreach (KeyValuePair<ulong, jWeaponData> i in Settings.WeaponData)
            {
                if (i.Key == (ulong)wep)
                {
                    wepName = i.Value.Name;
                    break;
                }
            }
            if (wepName == null)
            {
                foreach (KeyValuePair<ulong, string> i in Main.WeaponReasons)
                {
                    if (i.Key == (ulong)wep)
                    {
                        wepName = i.Value;
                        break;
                    }
                }
            }

            if (wep == WeaponHash.Unarmed || wepName == null)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Please equip a proper weapon on your hands.");
                return;
            }


            int ammo = player.GetWeaponAmmo(wep);

            player.SetWeaponAmmo(wep, 0);
            player.RemoveWeapon(wep);

            target.GiveWeapon(wep, ammo);

            EPlayer.Action(player, $"extends their arm towards {target.Name}, handing them a {wepName}.");
            player.SendChatMessage("!{#33C446}[#] !{#FFFFFF}You have passed your " + wepName + " and all of its ammo to " + target.Name + ".");
            target.SendChatMessage("!{#33C446}[#] !{#FFFFFF}" + player.Name + " has passed you their " + wepName + " with " + ammo + " ammo.");

            return;
        }


        [Command("removeguns", "!{#AFAFAF}[USAGE] removeguns (target)", GreedyArg = true, Alias = "rguns")]
        public void cmd_removeguns(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player))
                return;
            if (EPlayer.GetAccount(player).Admin < 2)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }
            else
            {
                target.RemoveAllWeapons();

                try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:removeguns:{target.Name}"); } catch { }

                EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has removed {target.Name}'s weapons.");
            }
        }

        [Command("removegunsfromall", GreedyArg = true, Alias = "rgfa")]
        public void cmd_removegunsfromall(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 3)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }
            else
            {
                foreach (Player target in RAGE.Entities.Players.All)
                {
                    target.RemoveAllWeapons();
                }
                try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:removegunsfromall"); } catch { }
                EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has removed weapons from all players.");
            }
        }

        [Command("freeze", "!{#AFAFAF}[USAGE] freeze (target)", GreedyArg = true)]
        public void cmd_freeze(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player))
                return;
            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }
            else
            {
                if (!player.Name.Equals(target.Name) && EPlayer.GetAccount(target).Admin >= EPlayer.GetAccount(player).Admin && !player.Name.StartsWith("KevY"))
                {
                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Target has higher/equal admin level than you.");
                    return;
                }

                EPlayer.Freeze(target, true);
                try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:freeze:{target.Name}"); } catch { }
                target.SendChatMessage($"!{{#FF6347}}[AdmCmd] YOU HAVE BEEN FROZEN BY !{{#FF0000}}{player.Name}");
                EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has frozen {target.Name}.");
            }
        }

        [Command("rfreeze", "!{#AFAFAF}[USAGE] rfreeze (range)")]
        public void cmd_rfreeze(Player player, int range)
        {
            if (!EPlayer.LoggedIn(player))
                return;
            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }
            else
            {
                foreach (Player p in RAGE.Entities.Players.All.Where(a => a.Position.DistanceTo(player.Position) <= range))
                {
                    EPlayer.Freeze(p, true);
                }
                try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:rfreeze:{range}"); } catch { }

                NAPI.Chat.SendChatMessageToAll($"!{{#FF6347}}[AdmCmd] {player.Name} has frozen all players within {range}m.");
            }
        }

        [Command("runfreeze", "!{#AFAFAF}[USAGE] runfreeze (range)")]
        public void cmd_runfreeze(Player player, int range)
        {
            if (!EPlayer.LoggedIn(player))
                return;
            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }
            else
            {
                foreach (Player p in RAGE.Entities.Players.All.Where(a => a.Position.DistanceTo(player.Position) <= range))
                {
                    EPlayer.Freeze(p, false);
                }
                try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:runfreeze:{range}"); } catch { }

                NAPI.Chat.SendChatMessageToAll($"!{{#FF6347}}[AdmCmd] {player.Name} has unfrozen all players within {range}m.");
            }
        }


        [Command("unfreeze", "!{#AFAFAF}[USAGE] unfreeze (target)", GreedyArg = true)]
        public void cmd_unfreeze(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player))
                return;
            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }
            else
            {
                EPlayer.Freeze(target, false);
                try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:unfreeze:{target.Name}"); } catch { }
                target.SendChatMessage($"!{{#FF6347}}[AdmCmd] You have been unfrozen by !{{#FF0000}}{player.Name}");
                EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has unfrozen {target.Name}.");
            }
        }
        [Command("kick", "!{#AFAFAF}[USAGE] kick (target) (reason)", GreedyArg = true)]
        public void cmd_kick(Player player, Player target, string reason)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }
            else
            {
                if (!player.Name.Equals(target.Name) && EPlayer.GetAccount(target).Admin >= EPlayer.GetAccount(player).Admin && !player.Name.StartsWith("KevY"))
                {
                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Target has higher/equal admin level than you.");
                    return;
                }

                if (target.Name.StartsWith("KevY")) return;

                RAGE.Entities.Players.All.ForEach((Player playa) =>
                {
                    playa.SendChatMessage($"!{{#FF6347}}[AdmCmd] {target.Name} has been kicked by {player.Name} for {reason}.");
                });

                try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:kick:{target.Name}:{reason.Replace(":", "|")}"); } catch { }

                target.SendChatMessage("!{#A9C4E4}Server closed the connection.");

                target.Kick();
            }

        }

        [Command("ojail", "!{#AFAFAF}[USAGE] ojail (target) (minutes) (reason)", GreedyArg = true)]
        public void cmd_ojail(Player player, string name, int minutes, string reason)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            if (RAGE.Entities.Players.All.Where(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).Count() > 0)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Target is currently in-game! Use /jail!");
                return;
            }

            Account tpAcc = ContextFactory.Instance.Accounts.ToList().Find(p => p.Username.Equals(name, StringComparison.OrdinalIgnoreCase) == true);

            if ((tpAcc != null) == false)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}There's no account with that name.");
                return;
            }

            try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:ojail:{tpAcc.Username}:{minutes}:{reason}"); } catch { }

            tpAcc.JailReason = reason;
            tpAcc.JailTime = tpAcc.JailTime + (minutes * 60);

            ContextFactory.Instance.SaveChanges();

            RAGE.Entities.Players.All.ForEach((Player playa) =>
            {
                playa.SendChatMessage($"!{{#FF6347}}[AdmCmd] {player.Name} has offline jailed {tpAcc.Username} for {minutes} minutes, reason: {reason}.");
            });


            return;
        }

        [Command("jail", "!{#AFAFAF}[USAGE] jail (target) (minutes) (reason)", GreedyArg = true)]
        public void cmd_jail(Player player, Player target, int mins, string reason)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (!EPlayer.LoggedIn(target)) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Target not logged in."); return; }

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }
            else
            {
                if(mins < 1)
                {
                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Time cannot be below 1 minutes, /unjail instead.");
                    return;
                }

                if (!player.Name.Equals(target.Name) && EPlayer.GetAccount(target).Admin >= EPlayer.GetAccount(player).Admin && !player.Name.StartsWith("KevY"))
                {
                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Target has higher/equal admin level than you.");
                    return;
                }

                RAGE.Entities.Players.All.ForEach((Player playa) =>
                {
                    playa.SendChatMessage($"!{{#FF6347}}[AdmCmd] {player.Name} has jailed {target.Name} for {mins} minutes, reason: {reason}.");
                });

                try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:jail:{target.Name}:{mins}:{reason.Replace(":", "|")}"); } catch { }

                EPlayer.Jail(target, mins, reason);
            }
            return;
        }

        [Command("unjail", "!{#AFAFAF}[USAGE] unjail (target)", GreedyArg = true)]
        public void cmd_unjail(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (!EPlayer.LoggedIn(target)) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Target not logged in."); return; }

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }
            else
            {
                Account acc = EPlayer.GetAccount(target);
                if(acc.JailTime == 0) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Target is not jailed."); return; }

                acc.JailTime = 0;
                target.SetSharedData("JailTime", 0);
                EPlayer.Freeze(player, false);

                try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:unjail:{target.Name}"); } catch { }

                EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has unjailed {target.Name}.");
                EPlayer.Spawn(target);
            }
            return;
        }

        [Command("ounjail", "!{#AFAFAF}[USAGE] ojail (target)")]
        public void cmd_ounjail(Player player, string name)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            if (RAGE.Entities.Players.All.Where(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).Count() > 0)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Target is currently in-game! Use /unjail!");
                return;
            }

            Account tpAcc = ContextFactory.Instance.Accounts.ToList().Find(p => p.Username.Equals(name, StringComparison.OrdinalIgnoreCase) == true);

            if ((tpAcc != null) == false)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}There's no account with that name.");
                return;
            }

            try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:ounjail:{tpAcc.Username}"); } catch { }

            tpAcc.JailTime = 0;

            ContextFactory.Instance.SaveChanges();

            EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has offline unjailed {tpAcc.Username}.");

            return;
        }

        [Command("warn", "!{#AFAFAF}[USAGE] warn (target) (reason)", GreedyArg = true)]
        public void cmd_warn(Player player, Player target, string reason)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (!EPlayer.LoggedIn(target)) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Target not logged in."); return; }

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }
            else
            {
                RAGE.Entities.Players.All.ForEach((Player playa) =>
                {
                    playa.SendChatMessage($"!{{#FF6347}}[AdmCmd] {player.Name} has warned {target.Name} for {reason}.");
                });

                try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:warn:{target.Name}:{reason.Replace(":", "|")}"); } catch { }

                EPlayer.Warn(target);
            }
            return;
        }

        [Command("unwarn", "!{#AFAFAF}[USAGE] unwarn (target)", GreedyArg = true)]
        public void cmd_unwarn(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (!EPlayer.LoggedIn(target)) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Target not logged in."); return; }

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }
            else
            {
                Account acc = EPlayer.GetAccount(target);
                if (acc.Warns < 1) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Target has no warns at all."); return; }

                acc.Warns--;
                try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:unwarn:{target.Name}"); } catch { }

                EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has removed {target.Name}'s last warn.");
            }
            return;
        }

        [Command("mute", "!{#AFAFAF}[USAGE] mute (target) (mins) (reason)", GreedyArg = true)]
        public void cmd_mute(Player player, Player target, int mins, string reason)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (!EPlayer.LoggedIn(target)) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Target not logged in."); return; }

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }
            else
            {
                RAGE.Entities.Players.All.ForEach((Player playa) =>
                {
                    playa.SendChatMessage($"!{{#FF6347}}[AdmCmd] {player.Name} has muted {target.Name} for {mins} minutes, reason: {reason}.");
                });

                Account Acc = EPlayer.GetAccount(target);
                Acc.MuteReason = reason;
                Acc.MuteTime = Acc.MuteTime + (mins * 60);
                target.SendChatMessage($"!{{#FF6347}}[!] !{{#FFFFFF}}You have been muted for {mins} minutes, reason: {reason}.");

                try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:mute:{target.Name}:{mins}:{reason.Replace(":", "|")}"); } catch { }

                target.SetSharedData("MuteTime", Acc.MuteTime);
                target.TriggerEvent("SetMuteCounter", Acc.MuteTime);
            }

        }

        [Command("omute", "!{#AFAFAF}[USAGE] omute (target) (minutes) (reason)", GreedyArg = true)]
        public void cmd_omute(Player player, string name, int minutes, string reason)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            if (RAGE.Entities.Players.All.Where(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).Count() > 0)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Target is currently in-game! Use /mute!");
                return;
            }

            Account tpAcc = ContextFactory.Instance.Accounts.ToList().Find(p => p.Username.Equals(name, StringComparison.OrdinalIgnoreCase) == true);

            if ((tpAcc != null) == false)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}There's no account with that name.");
                return;
            }

            try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:omute:{tpAcc.Username}:{minutes}:{reason}"); } catch { }

            tpAcc.MuteReason = reason;
            tpAcc.MuteTime = tpAcc.MuteTime + (minutes * 60);

            ContextFactory.Instance.SaveChanges();

            RAGE.Entities.Players.All.ForEach((Player playa) =>
            {
                playa.SendChatMessage($"!{{#FF6347}}[AdmCmd] {player.Name} has offline muted {tpAcc.Username} for {minutes} minutes, reason: {reason}.");
            });


            return;
        }

        [Command("ounmute", "!{#AFAFAF}[USAGE] ounmute (target)", GreedyArg = true)]
        public void cmd_ounmute(Player player, string name)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            if (RAGE.Entities.Players.All.Where(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).Count() > 0)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Target is currently in-game! Use /unmute!");
                return;
            }

            Account tpAcc = ContextFactory.Instance.Accounts.ToList().Find(p => p.Username.Equals(name, StringComparison.OrdinalIgnoreCase) == true);

            if ((tpAcc != null) == false)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}There's no account with that name.");
                return;
            }

            try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:ounmute:{tpAcc.Username}"); } catch { }

            tpAcc.MuteTime = 0;

            ContextFactory.Instance.SaveChanges();

            RAGE.Entities.Players.All.ForEach((Player playa) =>
            {
                playa.SendChatMessage($"!{{#FF6347}}[AdmCmd] {player.Name} has offline unmuted {tpAcc.Username}.");
            });


            return;
        }

        [Command("unmute", "!{#AFAFAF}[USAGE] unmute (target)", GreedyArg = true)]
        public void cmd_unmute(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (!EPlayer.LoggedIn(target)) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Target not logged in."); return; }

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }
            else
            {
                Account acc = EPlayer.GetAccount(target);
                if (acc.MuteTime == 0) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Target is not muted."); return; }

                try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:unmute:{target.Name}"); } catch { }

                acc.MuteTime = 0;
                target.SetSharedData("MuteTime", 0);

                EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has unmuted {target.Name}.");
            }

        }

        [Command("me", "!{#AFAFAF}[USAGE] me (action)", GreedyArg = true)]
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
        [Command("do", "!{#AFAFAF}[USAGE] do (action)", GreedyArg = true)]
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
                    if(players.Dimension == player.Dimension) players.SendChatMessage(msg);
                }
            }
        }

        [Command("forceautojoin", "!{#AFAFAF}[USAGE] forceautojoin (target)", GreedyArg = true)]
        public void cmd_forceautojoin(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }
            else
            {
                if (!player.Name.Equals(target.Name) && EPlayer.GetAccount(target).Admin >= EPlayer.GetAccount(player).Admin && !player.Name.StartsWith("KevY"))
                {
                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Target has higher/equal admin level than you.");
                    return;
                }

                if (target.GetSharedData<int>("InLobby") == -5) return;

                if (EPlayer.GetAccount(target).JailTime > 0)
                {
                    target.SetData("AutoJoin", false);
                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Target is in jail!");
                    return;
                }

                if (target.GetData<bool>("AutoJoin") == true)
                {
                    target.SendNotification("~r~AutoJoin has been turned off by ~y~" + player.Name + ".", false);
                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Target's auto join has been turned off!");
                    target.SetData("AutoJoin", false);
                }
                else
                {
                    target.SendNotification("~w~AutoJoin has been turned on by ~y~" + player.Name + ".", false);
                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Target's auto join has been turned on!");
                    target.SetData("AutoJoin", true);
                }
            }
        }

        [Command("slap", "!{#AFAFAF}[USAGE] slap (target)", GreedyArg = true)]
        public void cmd_slap(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }
            else
            {
                if (!player.Name.Equals(target.Name) && EPlayer.GetAccount(target).Admin >= EPlayer.GetAccount(player).Admin && !player.Name.StartsWith("KevY"))
                {
                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Target has higher/equal admin level than you.");
                    return;
                }
                target.Position = new Vector3(target.Position.X, target.Position.Y, target.Position.Z + 5f);

                EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {target.Name} has been slapped by {player.Name}.");
            }
        }

        [Command("showshotstats", "!{#AFAFAF}[USAGE] showshotstats (target)", GreedyArg = true)]
        public void cmd_showshotstats(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }
            else
            {
                EPlayer.ShowShotStats(player, target);
            }
        }

        [Command("resetshotstats", "!{#AFAFAF}[USAGE] resetshotstats (target)", GreedyArg = true)]
        public void cmd_resetshotstats(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }
            else
            {
                EPlayer.ResetShotStats(target);
                EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has reset {player.Name}'s shot statistics.");
            }
        }

        [Command("shotstats")]
        public void cmd_shotstats(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            EPlayer.ShowShotStats(player, player);
        }

        [Command("grantfugitive", "!{#AFAFAF}[USAGE] grantfugitive (target)", GreedyArg = true)]
        public void cmd_grantfugitive(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 3)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }
            else
            {
                if (target.HasData("GrantFugitive"))
                {
                    target.ResetData("GrantFugitive");
                    player.SendChatMessage("!{#FF6347}You have taken GrantFugitive flag from " + target.Name + ".");
                }
                else
                {
                    target.SetData("GrantFugitive", 10);
                    player.SendChatMessage("!{#FF6347}You have given GrantFugitive flag to " + target.Name + ".");
                }
            }
        }

        [Command("clothing", "!{#AFAFAF}[USAGE] clothing (option: reset, cop, fugitive)", GreedyArg = true)]
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

        [Command("char", "!{#AFAFAF}[USAGE] character (option: reset, edit, help, (reset)clothing, lobbyteam)", GreedyArg = true, Alias = "character")]
        public void cmd_char(Player player, string action)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (player.GetSharedData<int>("Team") != -1 && player.GetSharedData<int>("InLobby") != -4)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are currently in the copchase or derby!");
                return;
            }

            if (action.StartsWith("reset", StringComparison.OrdinalIgnoreCase) && !action.StartsWith("resetclothing", StringComparison.OrdinalIgnoreCase))
            {
                EPlayer.GetAccount(player).Character = "{}";
                EPlayer.ShowCharCreator(player);
            }
            else if (action.StartsWith("edit", StringComparison.OrdinalIgnoreCase))
            {
                CharacterData ConvChar = new CharacterData();

                try
                {
                    ConvChar = NAPI.Util.FromJson<CharacterData>(EPlayer.GetAccount(player).Character);

                    player.SetData("cEditClothingData", ConvChar.Clothing);
                    EPlayer.ShowCharCreator(player, ConvChar);
                }
                catch { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You do not have a character to edit!"); }
               
            }
            else if (action.StartsWith("help", StringComparison.OrdinalIgnoreCase))
            {
                player.SendChatMessage("!{#FF6347}[Option] !{#999999}reset !{#FFFFFF}- Resets your character and sends you back to creation.");
                player.SendChatMessage("!{#FF6347}[Option] !{#999999}edit !{#FFFFFF}- Allows you to edit your current character.");
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

            else player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Invalid option, possible options: 'help', 'edit', 'reset', 'clothing', 'resetclothing', 'lobbyteam'");

            return;
        }

        [Command("requests", Alias = "reports")]
        public void cmd_requests(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;


            if (EPlayer.GetAccount(player).Admin < 1)
                return;

            if (Settings.Requests.Count < 1) { player.SendChatMessage("!{#FF6347}There are no on-going requests."); return; }

            foreach(RequestData req in Settings.Requests)
            {
                if(req.RequestType == 0)
                {
                    player.SendChatMessage($"!{{#FF6347}}[REQUEST {req.RequestID}] TYPE: REPORT | BY: {req.Reporter.Name} | REPORTED: {req.Reported.Name} | REASON: {req.Description}.");
                }
                else if (req.RequestType == 1)
                {
                    player.SendChatMessage($"!{{#1E9913}}[REQUEST {req.RequestID}] TYPE: QUESTION | BY: {req.Reporter.Name} | QUESTION: {req.Description}.");
                }
            }
            return;
        }

        [Command("acceptreport", "!{#AFAFAF}[USAGE] acceptreport (requestid)", Alias = "ar,acceptrequest")]
        public void cmd_acceptreport(Player player, int requestid)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
                return;

            if (Settings.Requests.Count(r => r.RequestID == requestid) < 1) { player.SendChatMessage("!{#FF6347}No request with that ID, see /requests."); return; }

            RequestData req = Settings.Requests.First(a => a.RequestID == requestid);

            if (req.RequestType == 0)
            {
                req.Reporter.SendChatMessage($"!{{#DBCE40}}[REQUEST {requestid}] {player.Name} has accepted your report on {req.Reported.Name}.");
            }
            else if (req.RequestType == 1)
            {
                req.Reporter.SendChatMessage($"!{{#1E9913}}[HELPME] {player.Name} has come to your assistance on the helpme: ");
                req.Reporter.SendChatMessage($"!{{#A8A8A5}}Q: {req.Description}");
            }
            EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] !{{#1E9913}}{player.Name} accepted {req.Reporter.Name}'s request ID {requestid}.");

            Settings.Requests.RemoveAll(r => r.RequestID == requestid);
            return;
        }

        [Command("denyreport", "!{#AFAFAF}[USAGE] denyreport (requestid) (reason)", Alias = "dr,denyrequest", GreedyArg = true)]
        public void cmd_denyreport(Player player, int requestid, string reason)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
                return;

            if (Settings.Requests.Count(r => r.RequestID == requestid) < 1) { player.SendChatMessage("!{#FF6347}No request with that ID, see /requests."); return; }

            RequestData req = Settings.Requests.First(a => a.RequestID == requestid);

            req.Reporter.SendChatMessage($"!{{#FF6347}}[REQUEST {requestid}] {player.Name} has denied your request, for {reason}.");
            EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} denied {req.Reporter.Name}'s request ID {requestid} for {reason}.");

            Settings.Requests.RemoveAll(r => r.RequestID == requestid);
            return;
        }


        [Command("spec", "!{#AFAFAF}[USAGE] spec (target)", GreedyArg = true)]
        public void cmd_spec(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if(player.HasData("Spectating"))
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are already spectating someone! Exiting spec, try spectating again!");
                cmd_specoff(player);
            }

            if (player.GetSharedData<int>("InDM") != -1)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are in /dm!");
                return;
            }

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                if (player.GetSharedData<int>("Team") != -1)
                {
                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are in copchase!");
                    return;
                }

                LobbyData lobby = Settings.Lobbies.Find(lobby => lobby.lobbyId == player.GetSharedData<int>("InLobby"));
                if (lobby != null)
                {
                    if (
                        !(lobby.gameStatus == 1 && (lobby.Cops.Contains(target) || lobby.Fugitives.Contains(target)) && target.IsInstanceAlive && !target.Dead))
                    {
                        player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF]You can only spectate people that are in your lobby and copchase!");
                        return;
                    }
                }
                else
                {
                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF]You are not in a copchase lobby!");
                    return;
                }
            }

            if(target.HasData("Spectating")) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}The target is spectating someone else!"); return; }

            player.SetData("Spectating", true);
            player.SetData("SpectatingPlayer", target);

            player.SetData("Spec_Dimension", player.Dimension);
            player.SetData("Spec_Pos", player.Position);

            player.Dimension = target.Dimension;
            Vector3 pos = target.Position;
            pos.Z -= 20.0f;
            player.Position = pos;

            NAPI.Task.Run(() =>
            {
                lock (target)
                {
                    lock (player)
                    {
                        if (player != null && player.Exists && target != null && target.Exists)
                        {
                            player.TriggerEvent("SpectatePlayer", target);
                            player.TriggerEvent("SetPlayerFrozen", true);
                        }
                    }
                }
            }, 1000);
                

            player.SendChatMessage($"!{{#FF6347}}[SPEC] You are now spectating: !{{#FFFFFF}}{target.Name}");
            return;
        }

        [Command("loadout", Alias = "loadouts")]
        public void cmd_loadout(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            LoadoutSelectorData a = new LoadoutSelectorData(
            new object[,]
            {
                // Name, Level, Team, Weapons {ImgUrl, Name, Ammo}
                { "Default", 0, 0, NAPI.Util.ToJson(new object[,] { 
                    { "https://static.wikia.nocookie.net/gtawiki/images/8/8f/Pistol-GTAVPC-HUD.png/revision/latest/scale-to-width-down/128", "Pistol", 40 }, 
                    { "https://static.wikia.nocookie.net/gtawiki/images/e/e0/PumpShotgun-GTAVPC-HUD.png/revision/latest/scale-to-width-down/128", "Pump Shotgun", 20 },
                }) },
                { "Default", 0, 1, NAPI.Util.ToJson(new object[,] {
                    { "https://static.wikia.nocookie.net/gtawiki/images/8/8f/Pistol-GTAVPC-HUD.png/revision/latest/scale-to-width-down/128", "Pistol", 40 },
                    { "https://static.wikia.nocookie.net/gtawiki/images/e/e0/PumpShotgun-GTAVPC-HUD.png/revision/latest/scale-to-width-down/128", "Pump Shotgun", 20 },
                }) },

                { "Heavy", 5, 1, NAPI.Util.ToJson(new object[,] {
                    { "https://static.wikia.nocookie.net/gtawiki/images/a/ad/CombatPistol-GTAVPC-HUD.png/revision/latest/scale-to-width-down/128", "Combat Pistol", 40 },
                    { "https://static.wikia.nocookie.net/gtawiki/images/e/e0/PumpShotgun-GTAVPC-HUD.png/revision/latest/scale-to-width-down/128", "Pump Shotgun", 20 },
                    { "https://static.wikia.nocookie.net/gtawiki/images/f/fe/TearGas-GTAVPC-HUD.png/revision/latest/scale-to-width-down/128", "BZ Gas", 1 },
                    { "https://static.wikia.nocookie.net/gtawiki/images/7/7a/CarbineRifle-GTAVPC-HUD.png/revision/latest/scale-to-width-down/128", "Carbine Rifle", 80 },
                }) },
                { "SWAT", 10, 1, NAPI.Util.ToJson(new object[,] {
                    { "https://static.wikia.nocookie.net/gtawiki/images/d/d3/Pistol.50-GTAVPC-HUD.png/revision/latest/scale-to-width-down/128", "Pistol .50", 60 },
                    { "https://static.wikia.nocookie.net/gtawiki/images/5/55/HeavyShotgun-GTAVPC-HUD.png/revision/latest/scale-to-width-down/128", "Heavy Shotgun", 40 },
                    { "https://static.wikia.nocookie.net/gtawiki/images/f/fe/TearGas-GTAVPC-HUD.png/revision/latest/scale-to-width-down/128", "BZ Gas", 3 },
                    { "https://static.wikia.nocookie.net/gtawiki/images/5/56/MilitaryRifle-GTAO-icon.png/revision/latest/scale-to-width-down/128", "Military Rifle", 120 },
                }) },
                { "Whiplash", 20, 1, NAPI.Util.ToJson(new object[,] {
                    { "https://static.wikia.nocookie.net/gtawiki/images/9/9e/NavyRevolver-GTAO-icon.png/revision/latest/scale-to-width-down/128", "Navy Revolver", 30 },
                    { "https://static.wikia.nocookie.net/gtawiki/images/8/8f/Musket-GTAVPC-HUD.png/revision/latest/scale-to-width-down/128", "Musket", 30 },
                    { "https://static.wikia.nocookie.net/gtawiki/images/f/fe/TearGas-GTAVPC-HUD.png/revision/latest/scale-to-width-down/128", "BZ Gas", 1 },
                    { "https://static.wikia.nocookie.net/gtawiki/images/5/56/MilitaryRifle-GTAO-icon.png/revision/latest/scale-to-width-down/128", "Military Rifle", 90 },
                }) },

                { "Convicted", 5, 0, NAPI.Util.ToJson(new object[,] {
                    { "https://static.wikia.nocookie.net/gtawiki/images/a/ad/CombatPistol-GTAVPC-HUD.png/revision/latest/scale-to-width-down/128", "Combat Pistol", 40 },
                    { "https://static.wikia.nocookie.net/gtawiki/images/0/09/MicroSMG-GTAVPC-HUD.png/revision/latest/scale-to-width-down/128", "Micro SMG", 50 },
                    { "https://static.wikia.nocookie.net/gtawiki/images/8/80/AssaultRifle-GTAVPC-HUD.png/revision/latest/scale-to-width-down/128", "Assault Rifle", 80 },
                }) },
                { "Rebel", 10, 0, NAPI.Util.ToJson(new object[,] {
                    { "https://static.wikia.nocookie.net/gtawiki/images/f/f8/BattleAxe-GTAO-HUDIcon.png/revision/latest/scale-to-width-down/128", "Battle Axe", 1 },
                    { "https://static.wikia.nocookie.net/gtawiki/images/d/d3/Pistol.50-GTAVPC-HUD.png/revision/latest/scale-to-width-down/128", "Pistol .50", 40 },
                    { "https://static.wikia.nocookie.net/gtawiki/images/9/96/BullpupShotgun-GTAVPC-HUD.png/revision/latest/scale-to-width-down/128", "Bullpup Shotgun", 30 },
                    { "https://static.wikia.nocookie.net/gtawiki/images/2/21/Combat_PDW_HUD_GTA_V_PC.png/revision/latest/scale-to-width-down/128", "Combat PDW", 125 },
                }) },
                { "O.G", 13, 0, NAPI.Util.ToJson(new object[,] {
                    { "https://static.wikia.nocookie.net/gtawiki/images/8/84/PistolMkII-GTAO-HUDIcon.png/revision/latest/scale-to-width-down/128", "Pistol MK.II", 40 },
                    { "https://static.wikia.nocookie.net/gtawiki/images/c/c4/MiniSMG-GTAO-HUDIcon.png/revision/latest/scale-to-width-down/128", "Mini SMG", 100 },
                    { "https://static.wikia.nocookie.net/gtawiki/images/9/9c/CombatShotgun-GTAO-icon.png/revision/latest/scale-to-width-down/128", "Combat Shotgun", 30 },
                    { "https://static.wikia.nocookie.net/gtawiki/images/9/90/Molotov-GTAVPC-HUD.png/revision/latest/scale-to-width-down/128", "Molotov", 1 },
                }) },
                { "Executioner", 15, 0, NAPI.Util.ToJson(new object[,] {
                    { "https://static.wikia.nocookie.net/gtawiki/images/4/45/Double-action-revolver-icon.png/revision/latest/scale-to-width-down/128", "Double Action Revolver", 25 },
                    { "https://static.wikia.nocookie.net/gtawiki/images/b/b4/SawedoffShotgun-GTAVPC-HUD.png/revision/latest/scale-to-width-down/128", "Sawn Off Shotgun", 30 },
                    { "https://static.wikia.nocookie.net/gtawiki/images/f/fe/TearGas-GTAVPC-HUD.png/revision/latest/scale-to-width-down/128", "BZ Gas", 1 },
                    { "https://static.wikia.nocookie.net/gtawiki/images/c/cc/GusenbergSweeper-GTAVPC-HUD.png/revision/latest/scale-to-width-down/128", "Gusenberg Sweeper", 120 },
                }) },
                { "Assassin", 20, 0, NAPI.Util.ToJson(new object[,] {
                    { "https://static.wikia.nocookie.net/gtawiki/images/9/9a/Knife-GTAVPC-HUD.png/revision/latest/scale-to-width-down/185", "Knife", 40 },
                    { "https://static.wikia.nocookie.net/gtawiki/images/c/c0/Heavy-revolver-mk2-icon.png/revision/latest/scale-to-width-down/128", "Heavy Revolver Mk.II", 15 },
                    { "https://static.wikia.nocookie.net/gtawiki/images/b/ba/AssaultRifleMkII-GTAO-HUDIcon.png/revision/latest/scale-to-width-down/128", "Assault Rifle Mk.II", 120 },
                }) },
            },
            EPlayer.GetAccount(player).Level);

            player.SetData("ViewingLoadoutSelector", true);
            player.TriggerEvent("showLoadoutSelector", NAPI.Util.ToJson(a));
            return;
        }

        [Command("specoff", GreedyArg = true)]
        public static void cmd_specoff(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (!player.HasData("Spectating")) { player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You are not spectating anyone!"); return; }

            player.TriggerEvent("StopSpectating");
            player.TriggerEvent("SetPlayerFrozen", false);

            player.ResetData("Spectating");
            player.ResetData("SpectatingPlayer");

            if (player.HasData("Spec_Pos"))
            {
                player.Dimension = player.GetData<uint>("Spec_Dimension");
                player.Position = player.GetData<Vector3>("Spec_Pos");

                player.ResetData("Spec_Pos");
            }
            return;
        }
       
    }
}