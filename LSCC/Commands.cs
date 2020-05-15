using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSCC
{
    class Commands : Script
    {
        Random rand = new Random();

        [Command("aveh")]
        public void Cmd_car(Client player, string carname)
        {
            if (!Player.LoggedIn(player))
                return;

            if (Player.GetAccount(player).Admin < 1)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "~r~Error: You are not an admin.");
                return;
            }

            player.SetIntoVehicle(NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(carname), player.Position.Around(5.0f), player.Heading, rand.Next(64), rand.Next(64), player.SocialClubName, 255, false, true, player.Dimension), -1);
        }

        [Command("pos")]
        public void Cmd_poslog(Client player)
        {
            NAPI.Notification.SendNotificationToPlayer(player, $"X: {player.Position.X} - Y: {player.Position.Y} - Z: {player.Position.Z}");
            NAPI.Util.ConsoleOutput($"{player.Name} Position log: X: {player.Position.X} - Y: {player.Position.Y} - Z: {player.Position.Z}");
        }

        [Command("cmds", Alias = "help")]
        public void Cmd_help(Client player)
        {
            if (!Player.LoggedIn(player)) return;

            player.SendChatMessage("[Los Santos Copchase V BETA Commands]:"); 
            player.SendChatMessage("[General]: /ooc"); 
            player.SendChatMessage("[Debug]: /pos");

            if(Player.GetAccount(player).Admin >= 1)
            {
                player.SendChatMessage("[Admin Lv 1]: /a /aveh");
            }
            if (Player.GetAccount(player).Admin >= 6)
            {
                player.SendChatMessage("[Admin Lv 6]: /triggerevent /cameraset /setadmin");
            }
        }

        [Command("triggerevent")]
        public void Cmd_triggerevent(Client player, string eventt)
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
        public void Cmd_cameraset(Client player, int x, int y, int z, int rx, int ry, int rz, int viewangle)
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
        public void Cmd_setadmin(Client player, Client target, int level)
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
        public void Cmd_amiadmin(Client player)
        {
            if (!Player.LoggedIn(player))
                return;

            Account acc = player.GetData("AccountHandle");
            player.SendChatMessage($"Your admin level is: {acc.Admin}");
        }

        [Command("adminchat", Alias = "a", GreedyArg = true)]
        public void Cmd_adminchat(Client player, string text)
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

        [Command("ooc", Alias = "o", GreedyArg = true)]
        public void Cmd_ooc(Client player, string text)
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
