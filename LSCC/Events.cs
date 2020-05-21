using GTANetworkAPI;
using System;
using LSCC;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace LSCC.Events
{
    class Events : Script
    {
        [ServerEvent(Event.PlayerConnected)]
        public void OnPlayerConnected(Player player)
        {
            EPlayer.SetDiscordStatus(player, "Connecting to the server...");
            NAPI.Util.ConsoleOutput($"[Connection] {player.Name} has joined the server. (IP: {player.Address} - Serial: {player.Serial} - SC: {player.SocialClubName})");
            NAPI.Notification.SendNotificationToPlayer(player, "~g~Welcome to ~w~Los Santos Copchase!");
            player.SetData("LastCryptAttempt", Environment.TickCount);
            EPlayer.ResetPlayerData(player);
            EPlayer.Freeze(player, true);

            NAPI.Task.Run(() =>
            {
                EPlayer.LoginCameraSetup(player);
            }, delayTime: 1000);

            NAPI.Task.Run(() =>
            {
                EPlayer.LoginBrowserSetup(player);
            }, delayTime: 2500);

            player.TriggerEvent("InitChaseTimer");
        }

        [RemoteEvent("OnScriptedKeyPress")]
        public void OnScriptedKeyPress(Player player, int keyId)
        {
            if(keyId == 0x5A)
            {
                if(Settings.Cops.Contains(player)) LSCC.Commands.cmd_gps(player);
            }
            return;
        }

        [ServerEvent(Event.PlayerDisconnected)]
        public void OnPlayerDisconnected(Player player, DisconnectionType type, string reason)
        {
            NAPI.Util.ConsoleOutput($"[Connection] {player.Name} has left the server. (IP: {player.Address} - Serial: {player.Serial} - SC: {player.SocialClubName})");
            if (EPlayer.LoggedIn(player))
            {
                if (player.HasData("Blip"))
                {
                    Blip pBlip = player.GetData<Blip>("Blip");
                    pBlip.Delete();
                    player.ResetData("Blip");
                }

                switch (type)
                {
                    case DisconnectionType.Left:
                        NAPI.Chat.SendChatMessageToAll("!{#4343FA}[PART] !{#FFFFFF}" + player.Name + " has left the server. !{#4343FA}(Leaving)");
                        break;

                    case DisconnectionType.Timeout:
                        NAPI.Chat.SendChatMessageToAll("!{#4343FA}[PART] !{#FFFFFF}" + player.Name + " has left the server. !{#4343FA}(Timed Out)");
                        break;
                }

                EPlayer.Save(player);
            }

            EPlayer.ResetPlayerData(player);
        }

        [ServerEvent(Event.PlayerDeath)]
        public void OnPlayerDeath(Player player, Player killer, uint reason)
        {
            if (!EPlayer.LoggedIn(player)) return;

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
        public void OnPlayerText(Player player, string message)
        {
            if (!EPlayer.LoggedIn(player))
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You must be logged in to chat!");
                return;
            }

            string chatStr = $"{player.Name.Replace("_", " ")} says: {message}";
            foreach (Player i in RAGE.Entities.Players.All)
            {
                if (i.Dimension == player.Dimension)
                {
                    if (i.Position.DistanceTo(player.Position) <= 10.0) i.SendChatMessage($"!{{#D4D4D4}}{chatStr}");
                    if (i.Position.DistanceTo(player.Position) > 10.0 && i.Position.DistanceTo(player.Position) <= 15.0) i.SendChatMessage($"!{{#B5B5B5}}{chatStr}");
                    if (i.Position.DistanceTo(player.Position) > 15.0 && i.Position.DistanceTo(player.Position) <= 20.0) i.SendChatMessage($"!{{#999999}}{chatStr}");
                }
            }
        }

        public void OnPlayerLogin(Player player, Account acc)
        {
            EPlayer.SetDiscordStatus(player, $"Logging in as {acc.Username}");
            NAPI.Util.ConsoleOutput($"[Login] {player.Name} has logged in as {acc.Username}. (IP: {player.Address} - Serial: {player.Serial} - SC: {player.SocialClubName})");

            player.SetData("LoggedIn", true);
            player.SetData("AccountHandle", acc);

            player.Name = acc.Username;
            player.TriggerEvent("destroyBrowser");

            EPlayer.Spawn(player);
        }
        public void OnPlayerRegister(Player player, Account acc)
        {
            EPlayer.SetDiscordStatus(player, $"Registering as {acc.Username}");
            NAPI.Util.ConsoleOutput($"[Connection] {player.Name} has registered as {acc.Username}. (IP: {player.Address} - Serial: {player.Serial} - SC: {player.SocialClubName})");

            player.SetData("LoggedIn", true);
            player.SetData("AccountHandle", acc);

            player.Name = acc.Username;
            player.TriggerEvent("destroyBrowser");

            EPlayer.Spawn(player);
        }

        [RemoteEvent("OnPlayerLoginAttempt")]
        public void OnPlayerLoginAttempt(Player player, string username, string password)
        {
            NAPI.Util.ConsoleOutput($"[Login Attempt] Username {username}");
            if (Environment.TickCount - player.GetData<int>("LastCryptAttempt") < 1000)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~Please wait some time between each register attempt!", true);
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Please wait some time between each register attempt!");
                return;
            }
            player.SetData("LastCryptAttempt", Environment.TickCount);
            if (username.Contains("@"))
            {
                Account acc = ContextFactory.Instance.Accounts.FirstOrDefault(a => a.Email == username);
                if (acc != null)
                {
                    if (AccountManager.Login(acc, password))
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


        [RemoteEvent("RemoveVoiceListener")]
        public void RemoveVoiceListener(Player player, Player target)
        {
            if (!target.IsNull && target != null)
            {
                player.DisableVoiceTo(target);
            }
        }

        [RemoteEvent("AddVoiceListener")]
        public void AddVoiceListener(Player player, Player target)
        {
            if(!target.IsNull && target != null)
            {
                if (target.HasData("BlockList"))
                {
                    List<string> blockList = target.GetData<List<string>>("BlockList");
                    if (blockList.Contains(player.Name) && blockList.Count > 0) return;
                }

                player.EnableVoiceTo(target);
            }
        }

        [RemoteEvent("OnPlayerRegisterAttempt")]
        public void OnPlayerRegisterAttempt(Player player, string email, string username, string password, bool allcorrect, bool rules)
        {
            NAPI.Util.ConsoleOutput($"[Register Attempt] Username {username}");
            if (Environment.TickCount - player.GetData<int>("LastCryptAttempt") < 1000)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Please wait some time between each register attempt!");
                return;
            }
            if (!allcorrect || !rules)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You must agree to the rules and ensure all information is correct!");
                return;
            }
            if (password.Length < 5)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Your password is too small!");
                return;
            }
            if (username.Contains("@") || username.Length < 4)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Your username is too small or it cannot contain the character '@'");
                return;
            }
            Regex exp = new Regex(@"^[a-zA-Z0-9]*$");
            Match match = exp.Match(username);
            if (!match.Success)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Your username can only contain alphabets or numbers!");
                return;
            }
            if (!email.Contains("@") || email.Length < 5)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Your username cannot contain the character '@'");
                return;
            }
            Account acc = ContextFactory.Instance.Accounts.FirstOrDefault(a => a.Username == username);
            if (acc != null)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}An account with this name already exists!");
                return;
            }
            acc = ContextFactory.Instance.Accounts.FirstOrDefault(a => a.Email == email);
            if (acc != null)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}An account with this email already exists!");
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
                HWID = player.Serial,
                Level = 1,
                XP = 1,
                DonatorLevel = 0
            };
            AccountManager.Add(_new);
            OnPlayerRegister(player, _new);
        }

        [ServerEvent(Event.PlayerSpawn)]
        public void OnPlayerSpawn(Player player)
        {
            if (!EPlayer.LoggedIn(player))
            {
                EPlayer.LoginCameraSetup(player);
                EPlayer.LoginBrowserSetup(player);
                return;
            }

            if (player.HasData("Blip"))
            {
                Blip pBlip = player.GetData<Blip>("Blip");
                pBlip.Delete();
                player.ResetData("Blip");
            }

            EPlayer.SetDiscordStatus(player, $"In lobby as {player.Name}");

            player.RemoveAllWeapons();

            player.Dimension = 1;
            player.TriggerEvent("SetActorInvulnerable", true);
            player.TriggerEvent("CanFallOutOfCar", false);
            player.TriggerEvent("showHPBars", 0); // Never show HP Bars.
            EPlayer.SetColor(player, new Color(255, 255, 255, 255));
        }
    }
}
