using GTANetworkAPI;
using System;
using LSCC;

namespace LSCC.Events
{
    class Events : Script
    {
        [ServerEvent(Event.PlayerConnected)]
        public void OnPlayerConnected(Client player)
        {
            Player.SetDiscordStatus(player, "Connecting to the server");
            NAPI.Util.ConsoleOutput($"[Connection] {player.Name} has joined the server. (IP: {player.Address} - Serial: {player.Serial} - SC: {player.SocialClubName})");
            NAPI.Notification.SendNotificationToPlayer(player, "~g~Welcome to ~w~Los Santos Copchase!");
            player.SetData("LastCryptAttempt", Environment.TickCount);
            Player.ResetPlayerData(player);
            player.Freeze(true);

            NAPI.Task.Run(() =>
            {
                Player.LoginCameraSetup(player);
            }, delayTime: 1000);

            NAPI.Task.Run(() =>
            {
                Player.LoginBrowserSetup(player);
            }, delayTime: 2500);

            player.TriggerEvent("InitChaseTimer");
        }

        [ServerEvent(Event.PlayerDisconnected)]
        public void OnPlayerDisconnected(Client player, DisconnectionType type, string reason)
        {
            NAPI.Util.ConsoleOutput($"[Connection] {player.Name} has left the server. (IP: {player.Address} - Serial: {player.Serial} - SC: {player.SocialClubName})");
            if (Player.LoggedIn(player))
            {
                if (player.HasData("Blip"))
                {
                    Blip pBlip = player.GetData("Blip");
                    pBlip.Delete();
                    player.ResetData("Blip");
                }

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

            if (!killer.IsNull)
            {
                NAPI.Util.ConsoleOutput($"[Death] {player.Name} has been killed by {killer.Name}, reason: {reason}.");
            }
            else
            {
                NAPI.Util.ConsoleOutput($"[Death] {player.Name} has died, reason: {reason}.");
            }

            if (Settings.GameRunning == 1)
            {
                if(Settings.Fugitives.Contains(player))
                {
                    if (!killer.IsNull)
                    {
                        NAPI.Notification.SendNotificationToAll($"Fugitive ~r~<C>{player.Name}</C> ~w~has been taken out by ~b~<C>{killer.Name}</C>~w~.");
                    }
                    else
                    {
                        NAPI.Notification.SendNotificationToAll($"Fugitive ~r~<C>{player.Name}</C> ~w~has died.");
                    }

                    Settings.Fugitives.Remove(player);
                }
                if (Settings.Cops.Contains(player))
                {
                    if (!killer.IsNull)
                    {
                        NAPI.Notification.SendNotificationToAll($"Cop ~b~<C>{player.Name}</C> ~w~has been taken out by ~r~<C>{killer.Name}</C>~w~.");
                    }
                    else
                    {
                        NAPI.Notification.SendNotificationToAll($"Cop ~b~<C>{player.Name}</C> ~w~has died.");
                    }

                    Settings.Cops.Remove(player);
                }
            }

            player.RemoveAllWeapons();

            NAPI.Task.Run(() =>
            {
                Random rand = new Random();
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
                if (i.Dimension == player.Dimension)
                {
                    if (i.Position.DistanceTo(player.Position) <= 5.0) i.SendChatMessage($"~s~{chatStr}");
                    if (i.Position.DistanceTo(player.Position) > 5.0 && i.Position.DistanceTo(player.Position) <= 10.0) i.SendChatMessage($"~c~{chatStr}");
                    if (i.Position.DistanceTo(player.Position) > 10.0 && i.Position.DistanceTo(player.Position) <= 20.0) i.SendChatMessage($"~m~{chatStr}");
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

            if (player.HasData("Blip"))
            {
                Blip pBlip = player.GetData("Blip");
                pBlip.Delete();
                player.ResetData("Blip");
            }

            Player.SetDiscordStatus(player, $"In lobby as {player.Name}");

            player.RemoveAllWeapons();

            player.Dimension = 1;
            player.TriggerEvent("SetActorInvulnerable", true);
            player.NametagColor = new Color(255, 255, 255);
        }
    }
}
