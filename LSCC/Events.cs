using GTANetworkAPI;
using System;
using LSCC;

namespace LSCC.Events
{
    class Events : Script
    {
        Random rand = new Random();

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
                Vector3 SpawnArea = Settings.LobbySpawns[rand.Next(Settings.LobbySpawns.Length)];
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
            foreach (Client i in NAPI.Pools.GetAllPlayers())
            {
                if (i.Dimension == player.Dimension && i.Position.DistanceTo(player.Position) < 35.0)
                {
                    i.SendChatMessage(chatStr);
                }
            }
        }

        [ServerEvent(Event.PlayerSpawn)]
        public void OnPlayerSpawn(Client player)
        {
            if (!Player.LoggedIn(player))
            {
                Player.LoginCameraSetup(player);
                Player.LoginBrowserSetup(player);
                return;
            }

            if(player.GetData("InGame") == false) player.Dimension = 1;
        }
    }
}
