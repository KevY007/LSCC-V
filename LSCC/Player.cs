using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using GTANetworkAPI;
using LSCC;

namespace LSCC
{
    static class Player
    {
        public static Random rand = new Random();

        public static void ResetPlayerData(Client player)
        {
            player.ResetData("AccountHandle");
            player.SetData("AccountHandle", null);
            player.SetData("CharacterHandle", null);
            player.SetData("LoggedIn", false);
            player.SetData("AutoJoin", true);

            Settings.LoggedInPlayers.Remove(player);
            Settings.Cops.Remove(player);
            Settings.Fugitives.Remove(player);
        }

        public static void SetDiscordStatus(Client player, string status)
        {
            player.TriggerEvent("SetDiscordStatus", "Los Santos Copchase", status);
        }
        public static void Freeze(Client player, bool toggle)
        {
            player.TriggerEvent("SetPlayerFrozen", toggle);
        }
        public static void SetColor(Client player, Color col, int armour = 0)
        {
            player.NametagColor = col;
            foreach(Client p in NAPI.Pools.GetAllPlayers())
            {
                p.TriggerEvent("playerNametagSet", player, col.Red, col.Green, col.Blue, col.Alpha, armour);
            }
        }

        public static bool LoggedIn(Client player)
        {
            if (player.HasData("AccountHandle") && player.GetData("AccountHandle") != null && player.GetData("LoggedIn") == true)
            {
                return true;
            }
            return false;
        }

        public static void LoginCameraSetup(Client player)
        {
            NAPI.Entity.SetEntityTransparency(player, 0);

            player.TriggerEvent("TogglePlayerChat", false);
            Player.Freeze(player, true);
            player.Position = new Vector3(Settings.LoginCameraX, Settings.LoginCameraY, Settings.LoginCameraZ);
            player.Dimension = 666;
 
            player.TriggerEvent("DestroyPlayerCamera");
            player.TriggerEvent("CreatePlayerCamera", Settings.LoginCameraX, Settings.LoginCameraY, Settings.LoginCameraZ, Settings.LoginCameraRx, Settings.LoginCameraRy, Settings.LoginCameraRz, Settings.LoginCameraViewAngle);
        }

        public static void LoginBrowserSetup(Client player)
        {
            SetDiscordStatus(player, "On login screen");
            ResetPlayerData(player);
            player.TriggerEvent("showBrowser", "package://LoginRegister/login.html");
        }
        public static void Save(Client player)
        {
            if (LoggedIn(player))
            {
                ContextFactory.Instance.SaveChanges();
            }
        }
        public static Account GetAccount(Client player)
        {
            if (LoggedIn(player))
            {
                Account playerAcc = player.GetData("AccountHandle");
                return playerAcc;
            }

            return null;
        }
        public static void Spawn(Client player)
        {
            if (!LoggedIn(player)) return;

            if(!Settings.LoggedInPlayers.Contains(player)) Settings.LoggedInPlayers.Add(player);

            Vector3 SpawnArea = Settings.LobbySpawns[rand.Next(Settings.LobbySpawns.Length)];

            player.TriggerEvent("TogglePlayerChat", true);

            player.TriggerEvent("DestroyPlayerCamera");
            
            if(player.GetData("AutoJoin") == false) NAPI.Notification.SendNotificationToPlayer(player, $"~g~Autojoin has been enabled.", false);

            player.SetData("AutoJoin", true);

            if (Settings.GameRunning == 1)
            {
                Settings.Cops.Remove(player);
                Settings.Fugitives.Remove(player);
            }

            NAPI.Player.SpawnPlayer(player, new Vector3(SpawnArea.X, SpawnArea.Y, SpawnArea.Z));

            NAPI.Entity.SetEntityTransparency(player, 255);

            Player.Freeze(player, false);
            Player.SetColor(player, new Color(255, 255, 255, 255));
            player.Dimension = 1;

        }
        public static void SendMessageToAllAdmins(string message)
        {
            Account acc;

            foreach (Client i in NAPI.Pools.GetAllPlayers())
            {
                if (Player.LoggedIn(i))
                {
                    acc = i.GetData("AccountHandle");
                    if (acc.Admin > 0) i.SendChatMessage(message);
                }
            }
        }
    }
}
