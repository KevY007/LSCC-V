using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Resources;
using GTANetworkAPI;
using System.Linq;
using LSCC;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using LSCC.Events;

public static class Settings
{
    public const string Version = "0.1a";

    // MySQL Settings
    public const string MySQL_Host = "localhost";
    public const string MySQL_User = "lsccv";
    public const string MySQL_Password = "perkovicMatko3035";
    public const string MySQL_Database = "lsccv";

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
            NAPI.Server.SetCommandErrorMessage("~r~[Error] ~w~This command was not found, refer to /help for a list of commands.");

            ContextFactory.Instance.Database.EnsureCreated();
            
            NAPI.Util.ConsoleOutput("[>===========|Los Santos Copchase|===========<]");
            NAPI.Util.ConsoleOutput("[Version]: " + Settings.Version);
            NAPI.Util.ConsoleOutput("[Developers]: KevY");
            NAPI.Util.ConsoleOutput($"[Accounts]: {ContextFactory.Instance.Accounts.Count()}");
            NAPI.Util.ConsoleOutput("[>===========|Los Santos Copchase|===========<]");

            NAPI.Task.Run(() =>
            {
                TimeUpdate();
            }, delayTime: 15000);
        }

        public void TimeUpdate()
        {
            NAPI.Util.ConsoleOutput("TimeUpdate called.");
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
    }
}