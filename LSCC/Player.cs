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
            player.SetData("AutoJoin", false);
        }

        public static void SetDiscordStatus(Client player, string status)
        {
            player.TriggerEvent("SetDiscordStatus", "Los Santos Copchase", status);
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
            player.Spectate();
            NAPI.Entity.SetEntityTransparency(player, 0);
            NAPI.Entity.SetEntityInvincible(player, true);
            player.TriggerEvent("TogglePlayerChat", false);
            player.TriggerEvent("SetPlayerFrozen", true);
            player.Position = new Vector3(Settings.LoginCameraX, Settings.LoginCameraY, Settings.LoginCameraZ);
            player.Dimension = 666;
            player.Freeze(true);
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
                Account playerAcc = player.GetData("AccountHandle");
                if (player.HasData("CharacterHandle") && player.GetData("CharacterHandle") != null)
                {
                    
                }
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

            player.StopSpectating();

            Vector3 SpawnArea = Settings.RandomSpawns[rand.Next(Settings.RandomSpawns.Length)];

            player.TriggerEvent("TogglePlayerChat", true);


            player.TriggerEvent("DestroyPlayerCamera");

            SetDiscordStatus(player, $"In lobby as {player.Name}");

            player.SetData("AutoJoin", true);
            NAPI.Notification.SendNotificationToPlayer(player, $"You have spawned in.", false);
            NAPI.Notification.SendNotificationToPlayer(player, $"~g~Autojoin has been enabled.", false);

            NAPI.Player.SpawnPlayer(player, new Vector3(SpawnArea.X, SpawnArea.Y, SpawnArea.Z));
            NAPI.Entity.SetEntityInvincible(player, false);
            NAPI.Entity.SetEntityTransparency(player, 255);
            player.TriggerEvent("SetPlayerFrozen", false);
            player.Freeze(false);
            NAPI.Player.SetPlayerNametagColor(player, 255, 255, 255);
            NAPI.Entity.SetEntityDimension(player, 1);
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
