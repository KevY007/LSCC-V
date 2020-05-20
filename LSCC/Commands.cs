using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSCC
{
    class Commands : Script
    {
        [Command("game", GreedyArg = true)]
        public void cmd_game(Client player, string action)
        {
            if (!Player.LoggedIn(player))
                return;

            if (Player.GetAccount(player).Admin < 2)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "!{#FF6347}[!] !{#FFFFFF}You are not an admin.");
                return;
            }

            if (action.StartsWith("stop", StringComparison.OrdinalIgnoreCase))
            {
                if (!Main.StopGame(0))
                {
                    player.SendChatMessage("!{#FF0000}Failed to stop game! Is there even a copchase running?");
                }
                else NAPI.Chat.SendChatMessageToAll($"!{{#FF6347}}[AdmCmd] !{{#FFFFFF}}{player.Name} has forcefully stopped the copchase.");
            }
            else if (action.StartsWith("start", StringComparison.OrdinalIgnoreCase))
            {
                if (!Main.StartGame())
                {
                    player.SendChatMessage("!{#FF0000}Failed to start game! Is one already running?");
                }
                else NAPI.Chat.SendChatMessageToAll($"!{{#FF6347}}[AdmCmd] !{{#FFFFFF}}{player.Name} has forcefully started the copchase.");
            }
            else if (action.StartsWith("autostart", StringComparison.OrdinalIgnoreCase))
            {
                Settings.GameAutoStart = !Settings.GameAutoStart;
                if (Settings.GameAutoStart) NAPI.Notification.SendNotificationToAll($"~r~<C>{player.Name}</C> ~g~has enabled copchases.");
                else NAPI.Notification.SendNotificationToAll($"~r~<C>{player.Name}</C> ~w~has disabled copchases.");
            }
            else if (action.StartsWith("teambalance", StringComparison.OrdinalIgnoreCase))
            {
                string[] args = action.Split(' ');
                if (args.Count() > 1)
                {
                    int value = 0;
                    if (!Int32.TryParse(args[1], out value) || value < 1 || value > 99) 
                    { 
                        player.SendChatMessage("!{#999999}Team balance must be in numbers between 1-99!"); 
                        return; 
                    }

                    Settings.FugitivePercent = value;

                    NAPI.Chat.SendChatMessageToAll($"!{{#FF6347}}[AdmCmd] !{{#FFFFFF}}{player.Name} has set fugitives percentage to {Settings.FugitivePercent}%.");
                }
                else player.SendChatMessage("!{#999999}Proper usage: /game teambalance <percentage of fugitives>");
            }
            else player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Invalid option, possible options: 'start', 'stop', 'autostart', 'teambalance'");

            return;
        }

        [Command("tog", GreedyArg = true)]
        public void cmd_tog(Client player, string action)
        {
            if (!Player.LoggedIn(player))
                return;

            if (action.StartsWith("hud", StringComparison.OrdinalIgnoreCase))
            {
                bool state = player.GetData("HudState");
                player.SetData("HudState", !state);
                player.TriggerEvent("ToggleHud", !state);
               
                player.SendChatMessage("Toggled HUD.");
            }
            else if (action.StartsWith("pm", StringComparison.OrdinalIgnoreCase))
            {
                bool state = player.GetData("PMState");
                state = !state;
                player.SetData("PMState", state);
                if(state)
                {
                    player.SendChatMessage("!{#FF6347}[#] !{#FFFFFF}You have enabled your PMs.");
                }
                else player.SendChatMessage("!{#FF6347}[#] !{#FFFFFF}You have disabled your PMs.");
            }
            else player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Invalid option, possible options: 'hud', 'pm'");

            return;
        }

        [Command("aveh")]
        public void cmd_aveh(Client player, string carname)
        {
            if (!Player.LoggedIn(player))
                return;

            if (Player.GetAccount(player).Admin < 1)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "!{#FF6347}[!] !{#FFFFFF}You are not an admin.");
                return;
            }

            Random rand = new Random();

            Vehicle aVehicle = NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(carname), player.Position.Around(5.0f), player.Heading, rand.Next(64), rand.Next(64), player.SocialClubName, 255, false, true, player.Dimension);
            player.SetIntoVehicle(aVehicle, -1);
            Settings.AdminVehicles.Add(aVehicle);
            return;
        }

        [Command("gps", Alias = "bk")]
        public static void cmd_gps(Client player)
        {
            if (!Player.LoggedIn(player))
                return;

            if (Settings.GameRunning != 1 || !Settings.Cops.Contains(player))
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "!{#FF6347}[!] !{#FFFFFF}You are not a cop.");
                return;
            }

            bool state = player.GetData("GPS");
            state = !state;
            player.SetData("GPS", state);

            player.SendChatMessage("!{#FF6347}[#] !{#FFFFFF}You have " + (state == true ? "enabled" : "disabled") + " your emergency GPS beacon.");
            return;
        }

        [Command("goto")]
        public void cmd_goto(Client player, Client target)
        {
            if (!Player.LoggedIn(player))
                return;

            if (Player.GetAccount(player).Admin < 1)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "!{#FF6347}[!] !{#FFFFFF}You are not an admin.");
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

            Player.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has teleported to {target.Name}.");
            return;
        }

        [Command("bring", Alias = "gethere")]
        public void cmd_bring(Client player, Client target)
        {
            if (!Player.LoggedIn(player))
                return;

            if (Player.GetAccount(player).Admin < 1)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "!{#FF6347}[!] !{#FFFFFF}You are not an admin.");
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

            Player.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has summoned {target.Name}.");
            return;
        }

        [Command("sethp")]
        public void cmd_sethp(Client player, Client target, int health)
        {
            if (!Player.LoggedIn(player))
                return;

            if (Player.GetAccount(player).Admin < 1)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "!{#FF6347}[!] !{#FFFFFF}You are not an admin.");
                return;
            }

            target.Health = health;
            return;
        }

        [Command("setinv", Alias = "setinvulnerable")]
        public void cmd_setinv(Client player, Client target, int status)
        {
            if (!Player.LoggedIn(player))
                return;

            if (Player.GetAccount(player).Admin < 1)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "!{#FF6347}[!] !{#FFFFFF}You are not an admin.");
                return;
            }

            player.TriggerEvent("SetActorInvulnerable", status > 0 ? true : false);
            NAPI.Chat.SendChatMessageToPlayer(player, "!{#FF6347}[!] !{#FFFFFF}You have toggled " + target.Name + "'s invulnerability.");
            return;
        }

        [Command("setar")]
        public void cmd_setar(Client player, Client target, int armour)
        {
            if (!Player.LoggedIn(player))
                return;

            if (Player.GetAccount(player).Admin < 1)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "!{#FF6347}[!] !{#FFFFFF}You are not an admin.");
                return;
            }

            target.Armor = armour;
            return;
        }

        [Command("removeavehs")]
        public void cmd_removeavehs(Client player)
        {
            if (!Player.LoggedIn(player))
                return;

            if (Player.GetAccount(player).Admin < 1)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "!{#FF6347}[!] !{#FFFFFF}You are not an admin.");
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
            Player.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} removed all spawned admin vehicles.");
            return;
        }

        [Command("autojoin")]
        public void cmd_autojoin(Client player)
        {
            if (!Player.LoggedIn(player))
                return;

            if (player.GetData("AutoJoin") == true)
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

        [Command("pos")]
        public void cmd_poslog(Client player)
        {
            player.SendChatMessage($"X: {player.Position.X} - Y: {player.Position.Y} - Z: {player.Position.Z}");
            NAPI.Util.ConsoleOutput($"{player.Name} Position log: X: {player.Position.X} - Y: {player.Position.Y} - Z: {player.Position.Z}");
        }

        [Command("voiceblock", GreedyArg = true)]
        public void cmd_vblock(Client player, Client target)
        {
            List<string> blockList = new List<string>();
            if (player.HasData("BlockList"))
            {
                blockList = player.GetData("BlockList");
            }

            if(!blockList.Contains(target.Name) || blockList.Count < 1)
            {
                blockList.Add(target.Name);
                player.SendChatMessage($"!{{#FF6347}}[!] !{{#FFFFFF}}You have muted {target.Name} on voice chat, use this command again to unmute them.");
            }
            else
            {
                blockList.Remove(target.Name);
                player.SendChatMessage($"!{{#FF6347}}[#] !{{#FFFFFF}}You have unmuted {target.Name} on voice chat, use this command again to unmute them.");
            }

            player.SetData("BlockList", blockList);
            

            return;
        }

        [Command("cmds", Alias = "help")]
        public void cmd_help(Client player)
        {
            if (!Player.LoggedIn(player)) return;

            player.SendChatMessage("[Los Santos Copchase V BETA Commands]:");
            player.SendChatMessage("[General]: /ooc /autojoin /b /pm /tog");
            player.SendChatMessage("[Voice]: Hold N to speak, /voiceblock");
            player.SendChatMessage("[Cop]: /gps (Key Z)");
            player.SendChatMessage("[Roleplay]: /me /do");
            player.SendChatMessage("[Debug]: /pos");

            if (Player.GetAccount(player).Admin >= 1)
            {
                player.SendChatMessage("[Admin Lv 1]: /a /aveh /removeavehs /bring /goto /sethp /setar /spec");
                player.SendChatMessage("[Admin Lv 1]: /freeze /unfreeze /kick /slap /setinv(ulnerable) /specoff");
            }
            if (Player.GetAccount(player).Admin >= 2)
            {
                player.SendChatMessage("[Admin Lv 2]: /game");
            }
            if (Player.GetAccount(player).Admin >= 5)
            {
                player.SendChatMessage("[Admin Lv 5]: /maps /deletemap /createmap /addmapspawn");
            }
            if (Player.GetAccount(player).Admin >= 6)
            {
                player.SendChatMessage("[Admin Lv 6]: /triggerevent /cameraset /setadmin");
            }
        }

        [Command("maps")]
        public void cmd_maps(Client player)
        {
            if (!Player.LoggedIn(player))
                return;

            if (Player.GetAccount(player).Admin < 5)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "!{#FF6347}[!] !{#FFFFFF}You are not authorized to use this command.");
                return;
            }

            foreach (Map map in ContextFactory.Instance.Maps)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, $"!{{#DAA520}}{map.Name} | ID: [{map.SQL_ID}] | Enabled: [{map.Enabled}] | Spawns: [{ContextFactory.Instance.MapSpawns.Count(a => a.mapID == map.SQL_ID)}]");
            }
            return;
        }

        [Command("addmapspawn")]
        public void cmd_mapspawnadd(Client player, int mapid, int teamid)
        {
            if (!Player.LoggedIn(player))
                return;

            if (Player.GetAccount(player).Admin < 5)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "!{#FF6347}[!] !{#FFFFFF}You are not a level 5 admin.");
                return;
            }

            Map map = ContextFactory.Instance.Maps.FirstOrDefault(a => a.SQL_ID == mapid);
            if (map == null) { NAPI.Chat.SendChatMessageToPlayer(player, "!{#FF6347}[!] !{#FFFFFF}No map with this ID exists."); return; }

            if (teamid > 1) { NAPI.Chat.SendChatMessageToPlayer(player, "!{#FF6347}[!] !{#FFFFFF}Valid TeamID's are 0 (fugitive) or 1 (cop)."); return; }

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

            NAPI.Chat.SendChatMessageToPlayer(player, $"!{{#DAA520}}{map.Name} | Added a spawn (ID: {_new.SQL_ID}) over here for team [{teamid}]");

            return;
        }

        [Command("deletemap", GreedyArg = true)]
        public void cmd_delmap(Client player, string mapname)
        {
            if (!Player.LoggedIn(player))
                return;

            if (Player.GetAccount(player).Admin < 5)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "!{#FF6347}[!] !{#FFFFFF}You are not authorized to use this command.");
                return;
            }

            Map map = ContextFactory.Instance.Maps.FirstOrDefault(a => a.Name == mapname);
            if (map == null) { NAPI.Chat.SendChatMessageToPlayer(player, "!{#FF6347}[!] !{#FFFFFF}No map with this name exists."); return; }


            ContextFactory.Instance.MapSpawns.RemoveRange(ContextFactory.Instance.MapSpawns.Where(a => a.mapID == map.SQL_ID));
            ContextFactory.Instance.Maps.Remove(map);

            ContextFactory.Instance.SaveChanges();

            NAPI.Chat.SendChatMessageToPlayer(player, $"!{{#00FF33}}Map {mapname} has been deleted from the server.");
            return;
        }

        [Command("createmap", GreedyArg = true)]
        public void cmd_createmap(Client player, string mapname)
        {
            if (!Player.LoggedIn(player))
                return;

            if (Player.GetAccount(player).Admin < 5)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "!{#FF6347}[!] !{#FFFFFF}You are not a level 5 admin.");
                return;
            }

            var _new = new Map
            {
                Name = mapname,
                Enabled = true
            };

            ContextFactory.Instance.Maps.Add(_new);

            ContextFactory.Instance.SaveChanges();

            NAPI.Chat.SendChatMessageToPlayer(player, $"!{{#00FF33}}Map {mapname} with ID {_new.SQL_ID} has been added to the server.");
            return;
        }

        [Command("triggerevent")]
        public void cmd_triggerevent(Client player, string eventt)
        {
            if (!Player.LoggedIn(player))
                return;

            if (Player.GetAccount(player).Admin < 6)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "!{#FF6347}[!] !{#FFFFFF}You are not authorized to use this command.");
                return;
            }

            player.TriggerEvent(eventt);
        }

        [Command("cameraset")]
        public void cmd_cameraset(Client player, int x, int y, int z, int rx, int ry, int rz, int viewangle)
        {
            if (!Player.LoggedIn(player))
                return;

            if (Player.GetAccount(player).Admin < 6)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "!{#FF6347}[!] !{#FFFFFF}You are not authorized to use this command.");
                return;
            }

            player.TriggerEvent("DestroyPlayerCamera");
            player.TriggerEvent("CreatePlayerCamera", x, y, z, rx, ry, rz, viewangle);
        }

        [Command("setadmin")]
        public void cmd_setadmin(Client player, Client target, int level)
        {
            if (!Player.LoggedIn(player))
                return;

            if (Player.GetAccount(player).Admin < 6)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "!{#FF6347}[!] !{#FFFFFF}You are not authorized to use this command.");
                return;
            }

            if (!Player.LoggedIn(target)) { NAPI.Chat.SendChatMessageToPlayer(player, "!{#FF6347}[!] !{#FFFFFF}Target not logged in."); return; }


            Account t_acc = target.GetData("AccountHandle");

            if (level < 0 || level > 6)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot preceed level 0 or exceed level 6.");
                return;
            }

            if (String.Compare(player.Name, target.Name) == 0 && t_acc.Admin > Player.GetAccount(player).Admin)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "!{#FF6347}[!] !{#FFFFFF}Target has higher admin level than you.");
                return;
            }

            t_acc.Admin = level;
            NAPI.Chat.SendChatMessageToPlayer(player, $"!{{#00FF33}}You have set {target.Name}'s admin level to {level}");
            NAPI.Chat.SendChatMessageToPlayer(target, $"!{{#00FF33}}{player.Name} has set your admin level to {level}");
            Player.Save(target);
        }

        [Command("adminchat", Alias = "a", GreedyArg = true)]
        public void cmd_adminchat(Client player, string text)
        {
            if (!Player.LoggedIn(player))
                return;

            if (Player.GetAccount(player).Admin < 1)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "!{#FF6347}[!] !{#FFFFFF}You are not an admin.");
                return;
            }

            Player.SendMessageToAllAdmins($"!{{#FF6347}}[A] {player.Name}:  {text}");
        }

        [Command("looc", Alias = "b", GreedyArg = true)]
        public void cmd_b(Client player, string text)
        {
            if (!Player.LoggedIn(player))
                return;

            string chatStr = $"(( (LOCAL OOC) {player.Name.Replace("_", " ")} : {text} ))";
            foreach (Client i in NAPI.Pools.GetAllPlayers())
            {
                if (i.Dimension == player.Dimension)
                {
                    if (i.Position.DistanceTo(player.Position) <= 10.0) i.SendChatMessage($"!{{#D4D4D4}}{chatStr}");
                    if (i.Position.DistanceTo(player.Position) > 10.0 && i.Position.DistanceTo(player.Position) <= 20.0) i.SendChatMessage($"!{{#B5B5B5}}{chatStr}");
                    if (i.Position.DistanceTo(player.Position) > 20.0 && i.Position.DistanceTo(player.Position) <= 40.0) i.SendChatMessage($"!{{#999999}}{chatStr}");
                }
            }
        }

        [Command("ooc", Alias = "o", GreedyArg = true)]
        public void cmd_ooc(Client player, string text)
        {
            if (!Player.LoggedIn(player))
                return;

            if (Player.GetAccount(player).Admin < 1)
            {
                NAPI.Chat.SendChatMessageToAll($"(( [OOC] {player.Name}: {text} ))");
            }
            else
            {
                NAPI.Chat.SendChatMessageToAll($"(( [OOC] !{{#FF6347}}{player.Name}!{{#FFFFFF}}: {text} ))");
            }
        }
        [Command("pm", GreedyArg = true)]
        public void cmd_pm(Client player, Client target, string text)
        {
            if (!Player.LoggedIn(player))
                return;

            if (target.GetData("PMState") == false || player.GetData("PMState") == false) 
            { 
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Your or the target's PM's are toggled OFF."); 
                return; 
            }


            NAPI.Chat.SendChatMessageToPlayer(target, $"!{{#FFFF00}}(( PM received from {player.Name}: {text} ))");
            NAPI.Chat.SendChatMessageToPlayer(player, $"!{{#FFFF00}}(( PM sent to {target.Name}: {text} ))");
            return;
        }
        [Command("freeze", GreedyArg = true)]
        public void cmd_freeze(Client player, Client target)
        {
            if (!Player.LoggedIn(player))
                return;
            if (Player.GetAccount(player).Admin < 1)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "!{#FF6347}[!] !{#FFFFFF}You are not an admin.");
                return;
            }
            else
            {
                Player.Freeze(target, true);
                NAPI.Chat.SendChatMessageToPlayer(target, $"!{{#FF6347}}[AdmCmd] YOU HAVE BEEN FROZEN BY !{{#FF0000}}{player.Name}");
                Player.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has frozen {target.Name}.");
            }
        }
        [Command("unfreeze", GreedyArg = true)]
        public void cmd_unfreeze(Client player, Client target)
        {
            if (!Player.LoggedIn(player))
                return;
            if (Player.GetAccount(player).Admin < 1)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "!{#FF6347}[!] !{#FFFFFF}You are not an admin.");
                return;
            }
            else
            {
                Player.Freeze(target, false);
                NAPI.Chat.SendChatMessageToPlayer(target, $"!{{#FF6347}}[AdmCmd] You have been unfrozen by !{{#FF0000}}{player.Name}");
                Player.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} has unfrozen {target.Name}.");
            }
        }
        [Command("kick", GreedyArg = true)]
        public void cmd_kick(Client player, Client target, string reason)
        {
            if (!Player.LoggedIn(player))
                return;
            if (Player.GetAccount(player).Admin < 1)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "!{#FF6347}[!] !{#FFFFFF}You are not an admin.");
                return;
            }
            else
            {
                NAPI.Chat.SendChatMessageToAll($"!{{#FF6347}}[AdmCmd] !{{#FFFFFF}}{target.Name} has been kicked by {player.Name} for {reason}.");

                target.SendChatMessage("!{#A9C4E4}Server closed the connection.");

                target.Kick($"You have been kicked for the reason: {reason}");
            }

        }
        [Command("me", GreedyArg = true)]
        public void cmd_me(Client player, string text)
        {
            if (!Player.LoggedIn(player))
                return;
            else
            {
                var msg = "!{#C3A3DB}* " + player.Name + " " + text;
                var playersAround = NAPI.Player.GetPlayersInRadiusOfPlayer(40, player);

                foreach (var players in playersAround)
                {
                    NAPI.Chat.SendChatMessageToPlayer(players, msg);
                }
            }
        }
        [Command("do", GreedyArg = true)]
        public void cmd_do(Client player, string text)
        {
            if (!Player.LoggedIn(player))
                return;
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
        public void cmd_slap(Client player, Client target)
        {
            if (!Player.LoggedIn(player))
                return;
            if (Player.GetAccount(player).Admin < 1)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "!{#FF6347}[!] !{#FFFFFF}You are not an admin.");
                return;
            }
            else
            {
                target.Position = new Vector3(target.Position.X, target.Position.Y, target.Position.Z + 5f);

                Player.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {target.Name} has been slapped by {player.Name}.");
            }
        }
        [Command("spec", GreedyArg = true)]
        public void cmd_spec(Client player, Client target)
        {
            if (!Player.LoggedIn(player))
                return;
            if (Player.GetAccount(player).Admin < 1)
                return;
            else
            {
                player.Dimension = target.Dimension;

                player.TriggerEvent("DestroyPlayerCamera");
                player.TriggerEvent("CreatePlayerCamera", target.Position.X, target.Position.Y, target.Position.Z, 0, 0, 0, 90);
                
                player.TriggerEvent("SpectatePlayerCamera", target);
                player.TriggerEvent("SetPlayerFrozen", true);

                player.Spectate(target);
                NAPI.Chat.SendChatMessageToPlayer(player, $"!{{#FF6347}}[AdmCmd] You are now spectating: !{{#FFFFFF}}{target.Name}");
            }
        }
        [Command("specoff", GreedyArg = true)]
        public void cmd_specoff(Client player)
        {
            if (!Player.LoggedIn(player))
                return;
            if (Player.GetAccount(player).Admin < 1)
                return;
            else
            {
                player.TriggerEvent("DestroyPlayerCamera");
                player.TriggerEvent("SetPlayerFrozen", false);

                player.StopSpectating();
            }

        }
       
    }
}
