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
    static class EPlayer
    {
        public static Random rand = new Random();

        public static void ResetPlayerData(Player player)
        {
            player.ResetData("AccountHandle");
            player.ResetData("LoggedIn");
            player.SetData("LoggedIn", false);
            player.SetData("AutoJoin", true);
            player.SetData("HudState", false);
            player.SetData("PMState", true);
            player.SetSharedData("Team", -1);
            player.TriggerEvent("ToggleHud", false);
            player.SetData("GPS", false);
            player.ResetData("BlockList");
            player.SetData("InLobby", -1);

            Settings.LoggedInPlayers.Remove(player);

            foreach(LobbyData lobby in Settings.Lobbies)
            { 
                if (lobby.gameStatus == 1)
                {
                    if (lobby.Fugitives.Contains(player))
                    {
                        lobby.Fugitives.Remove(player);
                    }
                    else if (lobby.Cops.Contains(player))
                    {
                        lobby.Cops.Remove(player);
                    }
                }
            }
        }

        public static void SetDiscordStatus(Player player, string status)
        {
            player.TriggerEvent("SetDiscordStatus", "Los Santos Copchase", status);
        }
        public static void Freeze(Player player, bool toggle)
        {
            player.TriggerEvent("SetPlayerFrozen", toggle);
        }
        public static void SetColor(Player player, Color col, int armour = 0)
        {
            foreach(Player p in RAGE.Entities.Players.All)
            {
                p.TriggerEvent("playerNametagSet", player, col.Red, col.Green, col.Blue, col.Alpha, armour);
            }
        }

        public static bool LoggedIn(Player player)
        {
            if (player.HasData("AccountHandle") && player.GetData<Account>("AccountHandle") != null && player.HasData("LoggedIn") && player.GetData<bool>("LoggedIn") == true)
            {
                return true;
            }
            return false;
        }

        public static void LoginCameraSetup(Player player)
        {
            NAPI.Entity.SetEntityTransparency(player, 0);

            player.TriggerEvent("TogglePlayerChat", false);
            EPlayer.Freeze(player, true);
            player.Position = new Vector3(Settings.LoginCameraX, Settings.LoginCameraY, Settings.LoginCameraZ);
            player.Dimension = 666 + Convert.ToUInt32(RAGE.Entities.Players.Count);
 
            player.TriggerEvent("DestroyPlayerCamera");
            player.TriggerEvent("CreatePlayerCamera", Settings.LoginCameraX, Settings.LoginCameraY, Settings.LoginCameraZ, Settings.LoginCameraRx, Settings.LoginCameraRy, Settings.LoginCameraRz, Settings.LoginCameraViewAngle);
        }

        public static void LoginBrowserSetup(Player player)
        {
            player.TriggerEvent("InitiateCustomChat");
            player.TriggerEvent("TogglePlayerChat", false);

            SetDiscordStatus(player, "On login screen");
            ResetPlayerData(player);
            player.TriggerEvent("showBrowser", "package://LoginRegister/login.html");
        }
        public static void Save(Player player)
        {
            if (LoggedIn(player))
            {
                ContextFactory.Instance.SaveChangesAsync();
            }
        }
        public static Account GetAccount(Player player)
        {
            if (LoggedIn(player))
            {
                Account playerAcc = player.GetData<Account>("AccountHandle");
                return playerAcc;
            }

            return null;
        }
        public static void Spawn(Player player)
        {
            if (!LoggedIn(player)) return;

            if(!Settings.LoggedInPlayers.Contains(player)) Settings.LoggedInPlayers.Add(player);

            Vector3 SpawnArea = Settings.LobbySpawns[rand.Next(Settings.LobbySpawns.Length)];

            player.TriggerEvent("DestroyPlayerCamera");
            player.TriggerEvent("TogglePlayerChat", true);


            if (player.GetData<bool>("AutoJoin") == false) NAPI.Notification.SendNotificationToPlayer(player, $"~g~Autojoin has been enabled.", false);

            if (player.GetData<int>("InLobby") == -1)
            {
                int LobbyToBeIn = Settings.MaxLobbies;
                foreach (LobbyData lobby in Settings.Lobbies)
                {
                    if (Settings.LoggedInPlayers.Count(a => a.GetData<int>("InLobby") == lobby.lobbyId) >= Settings.MaxPlayersInLobby)
                    {
                        continue;
                    }

                    LobbyToBeIn = lobby.lobbyId;
                    break;
                }

                player.SetData("InLobby", LobbyToBeIn);

                foreach (Player targ in Settings.LoggedInPlayers.Where(a => a.GetData<int>("InLobby") == LobbyToBeIn))
                {
                    targ.SendChatMessage($"[Lobby {LobbyToBeIn}] {player.Name} has joined this lobby.");
                }
            }

            player.SetData("AutoJoin", true);

            foreach (LobbyData lobby in Settings.Lobbies)
            {
                if (lobby.gameStatus == 1)
                {
                    if (lobby.Fugitives.Contains(player))
                    {
                        lobby.Fugitives.Remove(player);
                    }
                    else if (lobby.Cops.Contains(player))
                    {
                        lobby.Cops.Remove(player);
                    }
                }
            }
            player.SetSharedData("Team", -1);


            player.SetData("HudState", true);
            player.TriggerEvent("ToggleHud", true);

            NAPI.Player.SpawnPlayer(player, new Vector3(SpawnArea.X, SpawnArea.Y, SpawnArea.Z));

            NAPI.Entity.SetEntityTransparency(player, 255);

            EPlayer.Freeze(player, false);
            EPlayer.SetColor(player, new Color(255, 255, 255, 255));

        }

        public static void SendMessageInLobby(int lobbyid, string message)
        {
            foreach (Player player in Settings.LoggedInPlayers)
            {
                if (player.GetData<int>("InLobby") == lobbyid && player.GetData<bool>("AutoJoin") == true)
                {
                    player.SendChatMessage(message);
                }
            }
        }
        
        public static void SendMessageToAllAdmins(string message)
        {
            Account acc;

            foreach (Player i in RAGE.Entities.Players.All)
            {
                if (EPlayer.LoggedIn(i))
                {
                    acc = i.GetData<Account>("AccountHandle");
                    if (acc.Admin > 0) i.SendChatMessage(message);
                }
            }
        }
    }
}
