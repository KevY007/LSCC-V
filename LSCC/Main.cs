using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Resources;
using GTANetworkAPI;
using System.Linq;
using LSCC;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public static class Settings
{
    public const string Version = "0.1a";

    // MySQL Settings
    public const string MySQL_Host = "localhost";
    public const string MySQL_User = "root";
    public const string MySQL_Password = "";
    public const string MySQL_Database = "LSCCV";

    // Locations
    public const float LoginCameraX = -550;
    public const float LoginCameraY = -210;
    public const float LoginCameraZ = 90;
    public const float LoginCameraRx = -30;
    public const float LoginCameraRy = 0;
    public const float LoginCameraRz = -30;
    public const float LoginCameraViewAngle = 60;

    public static Vector3[] RandomSpawns = new Vector3[]
    {
        new Vector3(-592f, -98f, 43f)
    };
}

namespace LSCC
{
    public class Main : Script
    {
        Random rand = new Random();


        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            NAPI.Server.SetAutoRespawnAfterDeath(false);
            NAPI.Server.SetAutoSpawnOnConnect(false);
            NAPI.Server.SetGlobalServerChat(false);
            ContextFactory.Instance.Database.EnsureCreated();
            
            NAPI.Util.ConsoleOutput("[>===========|Los Santos Copchase|===========<]");
            NAPI.Util.ConsoleOutput("[Version]: " + Settings.Version);
            NAPI.Util.ConsoleOutput("[Developers]: KevY");
            NAPI.Util.ConsoleOutput($"[Accounts]: {ContextFactory.Instance.Accounts.Count()}");
            NAPI.Util.ConsoleOutput("[>===========|Los Santos Copchase|===========<]");

        }

        [ServerEvent(Event.PlayerConnected)]
        public void OnPlayerConnected(Client player)
        {
            Player.SetDiscordStatus(player, "Connecting to the server");
            NAPI.Util.ConsoleOutput($"[Connection] {player.Name} has joined the server. (IP: {player.Address} - Serial: {player.Serial} - SC: {player.SocialClubName})");
            NAPI.Notification.SendNotificationToPlayer(player, "~g~Welcome to ~w~Los Santos Copchase!");
            player.SetData("LastCryptAttempt", Environment.TickCount);
            Player.ResetPlayerData(player);

            NAPI.Task.Run(() =>
            {
                Player.LoginCameraSetup(player);
            }, delayTime: 1000);
            
            NAPI.Task.Run(() =>
            {
                Player.LoginBrowserSetup(player);
            }, delayTime: 2500);
        }

        public void OnPlayerLogin(Client player, Account acc)
        {
            Player.SetDiscordStatus(player, $"Logging in as {acc.Username}");
            NAPI.Util.ConsoleOutput($"[Login] {player.Name} has logged in as {acc.Username}. (IP: {player.Address} - Serial: {player.Serial} - SC: {player.SocialClubName})");

            player.SetData("LoggedIn", true);
            player.SetData("AccountHandle", acc);

            player.Name = acc.Username;
            player.TriggerEvent("destroyBrowser");

            Player.Spawn(player);
        }
        public void OnPlayerRegister(Client player, Account acc)
        {
            Player.SetDiscordStatus(player, $"Registering as {acc.Username}");
            NAPI.Util.ConsoleOutput($"[Connection] {player.Name} has registered as {acc.Username}. (IP: {player.Address} - Serial: {player.Serial} - SC: {player.SocialClubName})");

            player.SetData("LoggedIn", true);
            player.SetData("AccountHandle", acc);

            player.Name = acc.Username;
            player.TriggerEvent("destroyBrowser");

            Player.Spawn(player);
        }
        [RemoteEvent("OnPlayerLoginAttempt")]
        public void OnPlayerLoginAttempt(Client player, string username, string password)
        {
            NAPI.Util.ConsoleOutput($"[Login Attempt] Username {username}");
            if (Environment.TickCount - player.GetData("LastCryptAttempt") < 1000)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~Please wait some time between each register attempt!", true);
                player.SendChatMessage("~r~Please wait some time between each register attempt!");
                return;
            }
            player.SetData("LastCryptAttempt", Environment.TickCount);
            if (username.Contains("@"))
            {
                Account acc = ContextFactory.Instance.Accounts.FirstOrDefault(a => a.Email == username);
                if (acc != null)
                {
                    if(AccountManager.Login(acc, password))
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
            NAPI.Notification.SendNotificationToPlayer(player, "~r~Invalid password/username!", true);

        }

        [RemoteEvent("OnPlayerRegisterAttempt")]
        public void OnPlayerRegisterAttempt(Client player, string email, string username, string password, bool allcorrect, bool rules)
        {
            NAPI.Util.ConsoleOutput($"[Register Attempt] Username {username}");
            if(Environment.TickCount - player.GetData("LastCryptAttempt") < 1000)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~Please wait some time between each register attempt!", true);
                player.SendChatMessage("~r~Please wait some time between each register attempt!");
                return;
            }
            if(!allcorrect || !rules)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~You must agree to the rules and ensure all information is correct!", true);
                player.SendChatMessage("~r~You must agree to the rules and ensure all information is correct!");
                return;
            }
            if(password.Length < 5)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~Your password is too small!", true);
                player.SendChatMessage("~r~Your password is too small!");
                return;
            }
            if (username.Contains("@") || username.Length < 4)
            { 
                NAPI.Notification.SendNotificationToPlayer(player, "~r~Your username is too small or it cannot contain the character '@'", true);
                player.SendChatMessage("~r~Your username is too small or it cannot contain the character '@'");
                return;
            }
            Regex exp = new Regex(@"^[a-zA-Z0-9]*$");
            Match match = exp.Match(username);
            if (!match.Success)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~Your username can only contain alphabets or numbers!", true);
                player.SendChatMessage("~r~Your username can only contain alphabets or numbers!");
                return;
            }
            if (!email.Contains("@") || email.Length < 5)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~Your username cannot contain the character '@'", true);
                player.SendChatMessage("~r~Your username cannot contain the character '@'");
                return;
            }
            Account acc = ContextFactory.Instance.Accounts.FirstOrDefault(a => a.Username == username);
            if (acc != null)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~An account with this name already exists!", true);
                player.SendChatMessage("~r~An account with this name already exists!");
                return;
            }
            acc = ContextFactory.Instance.Accounts.FirstOrDefault(a => a.Email == email);
            if (acc != null)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~An account with this email already exists!", true);
                player.SendChatMessage("~r~An account with this email already exists!");
                return;
            }
            player.SetData("LastCryptAttempt", Environment.TickCount);
            string salt = AccountManager.GenSalt();
            string newPass = AccountManager.Crypt(password, salt);
            var _new = new Account
            {
                Email = email,
                Password = newPass,
                Salt = salt,
                Username = username,
                Admin = 0,
                HWID = player.Serial
            };
            AccountManager.Add(_new);
            OnPlayerRegister(player, _new);
        }

        [Command("aveh")]
        public void Cmd_car(Client player, string carname)
        {
            if (!Player.LoggedIn(player))
                return;

            Account acc = player.GetData("AccountHandle");

            if (acc.Admin != 6 && String.Compare(player.Name, "KevY") != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "~r~Error: You are not a admin.");
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

        [Command("triggerevent")]
        public void Cmd_triggerevent(Client player, string eventt)
        {
            if (!Player.LoggedIn(player))
                return;

            Account acc = player.GetData("AccountHandle");

            if (acc.Admin != 6 && String.Compare(player.Name, "KevY") != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "~r~Error: You are not a admin.");
                return;
            }
            player.TriggerEvent(eventt);
        }

        [Command("cameraset")]
        public void Cmd_cameraset(Client player, int x, int y, int z, int rx, int ry, int rz, int viewangle)
        {
            if (!Player.LoggedIn(player))
                return;

            Account acc = player.GetData("AccountHandle");

            if (acc.Admin != 6 && String.Compare(player.Name, "KevY") != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "~r~Error: You are not a admin.");
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

            if (!Player.LoggedIn(target)) { NAPI.Chat.SendChatMessageToPlayer(player, "~r~Target not logged in."); return; }

            Account acc = player.GetData("AccountHandle");
            Account t_acc = target.GetData("AccountHandle");

            if (acc.Admin != 6 && String.Compare(player.Name, "KevY") != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "~r~Error: You are not a admin.");
                return;
            }

            if (level < 0 || level > 6)
            {
                player.SendChatMessage("~r~Error: You cannot preceed level 0 or exceed level 6.");
                return;
            }

            if (String.Compare(player.Name, target.Name) == 0 && t_acc.Admin > acc.Admin)
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
            player.SendChatMessage($"Your level is: {acc.Admin}");
        }

        [Command("adminchat", Alias = "a")]
        public void Cmd_adminchat(Client player, string text)
        {
            if (!Player.LoggedIn(player))
                return;
            
            Account acc = player.GetData("AccountHandle");

            if (acc.Admin < 0)
            {
                player.SendChatMessage("~r~Error: You aren't an admin!");
                return;
            }

            Player.SendMessageToAllAdmins($"~r~[ADMIN] {player.Name}:  {text}");
        }

        [Command("ooc", Alias = "o")]
        public void Cmd_ooc(Client player, string text)
        {
            if (!Player.LoggedIn(player))
                return;

            Account acc = player.GetData("AccountHandle");

            foreach (Client i in NAPI.Pools.GetAllPlayers())
            {
                i.SendChatMessage($"~w~(( [OOC] {player.Name}: {text} ))");
            }
        }

        [ServerEvent(Event.PlayerDisconnected)]
        public void OnPlayerDisconnected(Client player, DisconnectionType type, string reason)
        {
            NAPI.Util.ConsoleOutput($"[Connection] {player.Name} has left the server. (IP: {player.Address} - Serial: {player.Serial} - SC: {player.SocialClubName})");
            if (Player.LoggedIn(player))
            {
                Player.Save(player);
            }
            switch (type)
            {
                case DisconnectionType.Left:
                    NAPI.Chat.SendChatMessageToAll("~b~" + player.Name + "~w~ has quit the server.");
                    break;

                case DisconnectionType.Timeout:
                    NAPI.Chat.SendChatMessageToAll("~b~" + player.Name + "~w~ has timed out.");
                    break;

                case DisconnectionType.Kicked:
                    NAPI.Chat.SendChatMessageToAll("~b~" + player.Name + "~w~ has been kicked for " + reason);
                    break;
            }
            Player.ResetPlayerData(player);
        }

        [ServerEvent(Event.PlayerDeath)]
        public void OnPlayerDeath(Client player, Client killer, uint reason)
        {
            if (!Player.LoggedIn(player)) return;
           
            NAPI.Notification.SendNotificationToPlayer(player, $"~r~You died!", false);
            NAPI.Task.Run(() =>
            {
                Vector3 SpawnArea = Settings.RandomSpawns[rand.Next(Settings.RandomSpawns.Length)];
                NAPI.Player.SpawnPlayer(player, SpawnArea);
            }, delayTime: 4000);
            
        }

        [ServerEvent(Event.ChatMessage)]
        public void OnPlayerText(Client player, string message)
        {
            if (!Player.LoggedIn(player))
            {
                player.SendChatMessage("~r~You must be logged in to chat!");
                return;
            }
           
            string chatStr = $"{player.Name.Replace("_", " ")} says: {message}";
            foreach(Client i in NAPI.Pools.GetAllPlayers())
            {
                if(i.Dimension == player.Dimension && i.Position.DistanceTo(player.Position) < 35.0)
                {
                    i.SendChatMessage(chatStr);
                }
            }
        }

        [ServerEvent(Event.PlayerSpawn)]
        public void OnPlayerSpawn(Client player)
        {
            if(!Player.LoggedIn(player))
            {
                Player.LoginCameraSetup(player);
                Player.LoginBrowserSetup(player);
                return;
            }
        }
    }
}