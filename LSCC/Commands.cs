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
        public void cmd_game(Player player, string action)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 2)
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

        [Command("aveh")]
        public void cmd_aveh(Player player, string carname)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "!{#FF6347}[!] !{#FFFFFF}You are not an admin.");
                return;
            }

            Random rand = new Random();

            Vehicle aVehicle = NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(carname), player.Position.Around(5.0f), player.Heading, rand.Next(64), rand.Next(64), player.SocialClubName, 255, false, true, player.Dimension);

            aVehicle.SetSharedData("Team", -1);
       
            Settings.AdminVehicles.Add(aVehicle);

            player.SetIntoVehicle(aVehicle, 0);
            return;
        }

        [Command("gps", Alias = "bk")]
        public static void cmd_gps(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (Settings.GameRunning != 1 || !Settings.Cops.Contains(player))
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "!{#FF6347}[!] !{#FFFFFF}You are not a cop.");
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
                NAPI.Chat.SendChatMessageToPlayer(player, "!{#FF6347}[!] !{#FFFFFF}You are not an admin.");
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
                NAPI.Chat.SendChatMessageToPlayer(player, "!{#FF6347}[!] !{#FFFFFF}You are not an admin.");
                return;
            }

            player.TriggerEvent("SetActorInvulnerable", status > 0 ? true : false);
            NAPI.Chat.SendChatMessageToPlayer(player, "!{#FF6347}[!] !{#FFFFFF}You have toggled " + target.Name + "'s invulnerability.");
            return;
        }

        [Command("setar")]
        public void cmd_setar(Player player, Player target, int armour)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "!{#FF6347}[!] !{#FFFFFF}You are not an admin.");
                return;
            }

            target.Armor = armour;
            return;
        }

        [Command("removeavehs")]
        public void cmd_removeavehs(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
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
            EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {player.Name} removed all spawned admin vehicles.");
            return;
        }

        [Command("autojoin")]
        public void cmd_autojoin(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

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

        [Command("pos")]
        public void cmd_poslog(Player player)
        {
            player.SendChatMessage($"X: {player.Position.X} - Y: {player.Position.Y} - Z: {player.Position.Z}");
            NAPI.Util.ConsoleOutput($"{player.Name} Position log: X: {player.Position.X} - Y: {player.Position.Y} - Z: {player.Position.Z}");
        }

        [Command("voiceblock", GreedyArg = true)]
        public void cmd_vblock(Player player, Player target)
        {
            List<string> blockList = new List<string>();
            if (player.HasData("BlockList"))
            {
                blockList = player.GetData<List<string>>("BlockList");
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
        public void cmd_help(Player player)
        {
            if (!EPlayer.LoggedIn(player)) return;

            player.SendChatMessage("[Los Santos Copchase V BETA Commands]:");
            player.SendChatMessage("[General]: /ooc /autojoin /b /pm /tog");
            player.SendChatMessage("[Voice]: Hold N to speak, /voiceblock");
            player.SendChatMessage("[Cop]: /gps (Key Z)");
            player.SendChatMessage("[Roleplay]: /me /do");
            player.SendChatMessage("[Debug]: /pos");

            if (EPlayer.GetAccount(player).Admin >= 1)
            {
                player.SendChatMessage("[Admin Lv 1]: /a /aveh /removeavehs /bring /goto /sethp /setar /spec");
                player.SendChatMessage("[Admin Lv 1]: /freeze /unfreeze /kick /slap /setinv(ulnerable) /specoff");
            }
            if (EPlayer.GetAccount(player).Admin >= 2)
            {
                player.SendChatMessage("[Admin Lv 2]: /game");
            }
            if (EPlayer.GetAccount(player).Admin >= 5)
            {
                player.SendChatMessage("[Admin Lv 5]: /maps /deletemap /createmap /addmapspawn");
            }
            if (EPlayer.GetAccount(player).Admin >= 6)
            {
                player.SendChatMessage("[Admin Lv 6]: /triggerevent /cameraset /setadmin");
            }
        }

        [Command("maps")]
        public void cmd_maps(Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 5)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "!{#FF6347}[!] !{#FFFFFF}You are not authorized to use this command.");
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
        public void cmd_delmap(Player player, string mapname)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 5)
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
        public void cmd_triggerevent(Player player, string eventt)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 6)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "!{#FF6347}[!] !{#FFFFFF}You are not authorized to use this command.");
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
                NAPI.Chat.SendChatMessageToPlayer(player, "!{#FF6347}[!] !{#FFFFFF}You are not authorized to use this command.");
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
                NAPI.Chat.SendChatMessageToPlayer(player, "!{#FF6347}[!] !{#FFFFFF}You are not authorized to use this command.");
                return;
            }

            if (!EPlayer.LoggedIn(target)) { NAPI.Chat.SendChatMessageToPlayer(player, "!{#FF6347}[!] !{#FFFFFF}Target not logged in."); return; }


            Account t_acc = target.GetData<Account>("AccountHandle");

            if (level < 0 || level > 6)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot preceed level 0 or exceed level 6.");
                return;
            }

            if (String.Compare(player.Name, target.Name) == 0 && t_acc.Admin > EPlayer.GetAccount(player).Admin)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "!{#FF6347}[!] !{#FFFFFF}Target has higher admin level than you.");
                return;
            }

            t_acc.Admin = level;
            NAPI.Chat.SendChatMessageToPlayer(player, $"!{{#00FF33}}You have set {target.Name}'s admin level to {level}");
            NAPI.Chat.SendChatMessageToPlayer(target, $"!{{#00FF33}}{player.Name} has set your admin level to {level}");
            EPlayer.Save(target);
        }

        [Command("adminchat", Alias = "a", GreedyArg = true)]
        public void cmd_adminchat(Player player, string text)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "!{#FF6347}[!] !{#FFFFFF}You are not an admin.");
                return;
            }

            EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[A] {player.Name}:  {text}");
        }

        [Command("looc", Alias = "b", GreedyArg = true)]
        public void cmd_b(Player player, string text)
        {
            if (!EPlayer.LoggedIn(player))
                return;

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

        [Command("ooc", Alias = "o", GreedyArg = true)]
        public void cmd_ooc(Player player, string text)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (EPlayer.GetAccount(player).Admin < 1)
            {
                NAPI.Chat.SendChatMessageToAll($"(( [OOC] {player.Name}: {text} ))");
            }
            else
            {
                NAPI.Chat.SendChatMessageToAll($"(( [OOC] !{{#FF6347}}{player.Name}!{{#FFFFFF}}: {text} ))");
            }
        }
        [Command("pm", GreedyArg = true)]
        public void cmd_pm(Player player, Player target, string text)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            if (target.GetData<bool>("PMState") == false || player.GetData<bool>("PMState") == false) 
            { 
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Your or the target's PM's are toggled OFF."); 
                return; 
            }


            NAPI.Chat.SendChatMessageToPlayer(target, $"!{{#FFFF00}}(( PM received from {player.Name}: {text} ))");
            NAPI.Chat.SendChatMessageToPlayer(player, $"!{{#FFFF00}}(( PM sent to {target.Name}: {text} ))");
            return;
        }
        [Command("freeze", GreedyArg = true)]
        public void cmd_freeze(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player))
                return;
            if (EPlayer.GetAccount(player).Admin < 1)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "!{#FF6347}[!] !{#FFFFFF}You are not an admin.");
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
                NAPI.Chat.SendChatMessageToPlayer(player, "!{#FF6347}[!] !{#FFFFFF}You are not an admin.");
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
        public void cmd_me(Player player, string text)
        {
            if (!EPlayer.LoggedIn(player))
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
        public void cmd_do(Player player, string text)
        {
            if (!EPlayer.LoggedIn(player))
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
        public void cmd_slap(Player player, Player target)
        {
            if (!EPlayer.LoggedIn(player))
                return;
            if (EPlayer.GetAccount(player).Admin < 1)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "!{#FF6347}[!] !{#FFFFFF}You are not an admin.");
                return;
            }
            else
            {
                target.Position = new Vector3(target.Position.X, target.Position.Y, target.Position.Z + 5f);

                EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] {target.Name} has been slapped by {player.Name}.");
            }
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
                player.Dimension = target.Dimension;

                player.TriggerEvent("DestroyPlayerCamera");
                player.TriggerEvent("CreatePlayerCamera", target.Position.X, target.Position.Y, target.Position.Z, 0, 0, 0, 90);
                
                player.TriggerEvent("SpectatePlayerCamera", target);
                player.TriggerEvent("SetPlayerFrozen", true);

                NAPI.Chat.SendChatMessageToPlayer(player, $"!{{#FF6347}}[AdmCmd] You are now spectating: !{{#FFFFFF}}{target.Name}");
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
                player.TriggerEvent("DestroyPlayerCamera");
                player.TriggerEvent("SetPlayerFrozen", false);
            }

        }
       
    }
}
