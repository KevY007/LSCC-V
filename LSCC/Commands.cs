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
                NAPI.Chat.SendChatMessageToPlayer(player, "~r~Error: You are not an admin.");
                return;
            }

            if (action.CompareTo("stop") == 0)
            {
                if (!Main.StopGame(0))
                {
                    player.SendChatMessage("~r~Failed to stop game! Is there even a copchase running?");
                }
                else NAPI.Chat.SendChatMessageToAll($"~r~[AdmCmd] ~w~{player.Name} has forcefully stopped the copchase.");
            }
            else if (action.CompareTo("start") == 0)
            {
                if (!Main.StartGame())
                {
                    player.SendChatMessage("~r~Failed to start game! Is one already running?");
                }
                else NAPI.Chat.SendChatMessageToAll($"~r~[AdmCmd] ~w~{player.Name} has forcefully started the copchase.");
            }
            else if (action.CompareTo("autostart") == 0)
            {
                Settings.GameAutoStart = !Settings.GameAutoStart;
                if (Settings.GameAutoStart) NAPI.Notification.SendNotificationToAll($"~r~<C>{player.Name}</C> ~g~has enabled copchases.");
                else NAPI.Notification.SendNotificationToAll($"~r~<C>{player.Name}</C> ~w~has disabled copchases.");
            }
            else player.SendChatMessage("Invalid option, possible options: 'start', 'stop', 'autostart'");

            return;
        }

        [Command("aveh")]
        public void cmd_aveh(Client player, string carname)
        {
            if (!Player.LoggedIn(player))
                return;

            if (Player.GetAccount(player).Admin < 1)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "~r~Error: You are not an admin.");
                return;
            }

            Random rand = new Random();

            Vehicle aVehicle = NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(carname), player.Position.Around(5.0f), player.Heading, rand.Next(64), rand.Next(64), player.SocialClubName, 255, false, true, player.Dimension);
            player.SetIntoVehicle(aVehicle, -1);
            Settings.AdminVehicles.Add(aVehicle);
            return;
        }

        [Command("goto")]
        public void cmd_goto(Client player, Client target)
        {
            if (!Player.LoggedIn(player))
                return;

            if (Player.GetAccount(player).Admin < 1)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "~r~Error: You are not an admin.");
                return;
            }

            player.Position = target.Position.Around(3.0f);
            player.Dimension = target.Dimension;

            Player.SendMessageToAllAdmins($"~r~[AdmCmd] {player.Name} has teleported to {target.Name}.");
            return;
        }

        [Command("bring", Alias = "gethere")]
        public void cmd_bring(Client player, Client target)
        {
            if (!Player.LoggedIn(player))
                return;

            if (Player.GetAccount(player).Admin < 1)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "~r~Error: You are not an admin.");
                return;
            }

            target.Position = player.Position.Around(3.0f);
            target.Dimension = player.Dimension;

            Player.SendMessageToAllAdmins($"~r~[AdmCmd] {player.Name} has summoned {target.Name}.");
            return;
        }

        [Command("removeavehs")]
        public void cmd_removeavehs(Client player)
        {
            if (!Player.LoggedIn(player))
                return;

            if (Player.GetAccount(player).Admin < 1)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "~r~Error: You are not an admin.");
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
            Player.SendMessageToAllAdmins($"~r~[AdmCmd] {player.Name} removed all spawned admin vehicles.");
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
            NAPI.Notification.SendNotificationToPlayer(player, $"X: {player.Position.X} - Y: {player.Position.Y} - Z: {player.Position.Z}");
            NAPI.Util.ConsoleOutput($"{player.Name} Position log: X: {player.Position.X} - Y: {player.Position.Y} - Z: {player.Position.Z}");
        }

        [Command("cmds", Alias = "help")]
        public void cmd_help(Client player)
        {
            if (!Player.LoggedIn(player)) return;

            player.SendChatMessage("[Los Santos Copchase V BETA Commands]:"); 
            player.SendChatMessage("[General]: /ooc /autojoin /b");
            player.SendChatMessage("[Debug]: /pos");

            if(Player.GetAccount(player).Admin >= 1)
            {
                player.SendChatMessage("[Admin Lv 1]: /a /aveh /removeavehs /bring /goto");
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
                NAPI.Chat.SendChatMessageToPlayer(player, "~r~Error: You are not a level 5 admin.");
                return;
            }

            foreach(Map map in ContextFactory.Instance.Maps)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, $"~y~{map.Name} | ID: [{map.SQL_ID}] | Enabled: [{map.Enabled}] | Spawns: [{ContextFactory.Instance.MapSpawns.Count(a=> a.mapID == map.SQL_ID)}]");
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
                NAPI.Chat.SendChatMessageToPlayer(player, "~r~Error: You are not a level 5 admin.");
                return;
            }

            Map map = ContextFactory.Instance.Maps.FirstOrDefault(a => a.SQL_ID == mapid);
            if (map == null) { NAPI.Chat.SendChatMessageToPlayer(player, "~r~No map with this ID exists."); return; }
			
			if(teamid > 1) { NAPI.Chat.SendChatMessageToPlayer(player, "~r~Valid TeamID's are 0 (fugitive) or 1 (cop)."); return; }

            float[] currentLoc = new float[4];
            if(player.IsInVehicle == true)
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

            NAPI.Chat.SendChatMessageToPlayer(player, $"~y~{map.Name} | Added a spawn (ID: {_new.SQL_ID}) over here for team [{teamid}]");
            
            return;
        }

        [Command("deletemap", GreedyArg = true)]
        public void cmd_delmap(Client player, string mapname)
        {
            if (!Player.LoggedIn(player))
                return;

            if (Player.GetAccount(player).Admin < 5)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "~r~Error: You are not a level 5 admin.");
                return;
            }

            Map map = ContextFactory.Instance.Maps.FirstOrDefault(a => a.Name == mapname);
            if(map == null) { NAPI.Chat.SendChatMessageToPlayer(player, "~r~No map with this name exists."); return; }


            ContextFactory.Instance.MapSpawns.RemoveRange(ContextFactory.Instance.MapSpawns.Where(a => a.mapID == map.SQL_ID));
            ContextFactory.Instance.Maps.Remove(map);

            ContextFactory.Instance.SaveChanges();

            NAPI.Chat.SendChatMessageToPlayer(player, $"~g~Map {mapname} has been deleted from the server.");
            return;
        }

        [Command("createmap", GreedyArg = true)]
        public void cmd_createmap(Client player, string mapname)
        {
            if (!Player.LoggedIn(player))
                return;

            if (Player.GetAccount(player).Admin < 5)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "~r~Error: You are not a level 5 admin.");
                return;
            }

            var _new = new Map
            {
                Name = mapname,
                Enabled = true
            };

            ContextFactory.Instance.Maps.Add(_new);

            ContextFactory.Instance.SaveChanges();

            NAPI.Chat.SendChatMessageToPlayer(player, $"~g~Map {mapname} with ID {_new.SQL_ID} has been added to the server.");
            return;
        }

        [Command("triggerevent")]
        public void cmd_triggerevent(Client player, string eventt)
        {
            if (!Player.LoggedIn(player))
                return;

            if (Player.GetAccount(player).Admin < 6)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "~r~Error: You are not a level 6 admin.");
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
                NAPI.Chat.SendChatMessageToPlayer(player, "~r~Error: You are not a level 6 admin.");
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
                NAPI.Chat.SendChatMessageToPlayer(player, "~r~Error: You are not a level 6 admin.");
                return;
            }

            if (!Player.LoggedIn(target)) { NAPI.Chat.SendChatMessageToPlayer(player, "~r~Target not logged in."); return; }

      
            Account t_acc = target.GetData("AccountHandle");

            if (level < 0 || level > 6)
            {
                player.SendChatMessage("~r~Error: You cannot preceed level 0 or exceed level 6.");
                return;
            }

            if (String.Compare(player.Name, target.Name) == 0 && t_acc.Admin > Player.GetAccount(player).Admin)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "~r~Error: Target has higher admin level than you.");
                return;
            }

            t_acc.Admin = level;
            NAPI.Chat.SendChatMessageToPlayer(player, $"~g~You have set {target.Name}'s admin level to {level}");
            NAPI.Chat.SendChatMessageToPlayer(target, $"~g~{player.Name} has set your admin level to {level}");
            Player.Save(target);
        }

        [Command("amiadmin")]
        public void cmd_amiadmin(Client player)
        {
            if (!Player.LoggedIn(player))
                return;

            Account acc = player.GetData("AccountHandle");
            player.SendChatMessage($"Your admin level is: {acc.Admin}");
        }

        [Command("adminchat", Alias = "a", GreedyArg = true)]
        public void cmd_adminchat(Client player, string text)
        {
            if (!Player.LoggedIn(player))
                return;

            if (Player.GetAccount(player).Admin < 1)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "~r~Error: You are not an admin.");
                return;
            }

            Player.SendMessageToAllAdmins($"~r~[ADMIN] {player.Name}:  {text}");
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
                    if(i.Position.DistanceTo(player.Position) <= 10.0) i.SendChatMessage($"~s~{chatStr}");
                    if(i.Position.DistanceTo(player.Position) > 10.0 && i.Position.DistanceTo(player.Position) <= 20.0) i.SendChatMessage($"~c~{chatStr}");
                    if(i.Position.DistanceTo(player.Position) > 20.0 && i.Position.DistanceTo(player.Position) <= 40.0) i.SendChatMessage($"~m~{chatStr}");
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
                NAPI.Chat.SendChatMessageToAll($"~w~(( [OOC] {player.Name}: {text} ~w~))");
            }
            else
            {
                NAPI.Chat.SendChatMessageToAll($"~w~(( [OOC] ~r~{player.Name}~w~: {text} ~w~))");
            }
        }
    }
}
