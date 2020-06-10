using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSCC
{
    class Events : IScript
    {
        [ScriptEvent(ScriptEventType.PlayerConnect)]
        public void OnPlayerConnect(EPlayer player, string reason)
        {
            BanData ban = ContextFactory.Instance.Bans.ToList().Find(ban => ban.HWID.CompareTo(player.HardwareIdHash.ToString()) == 0 || ban.IP.CompareTo(player.Ip) == 0 ||
                ban.RockstarID == player.SocialClubId);

            if (ban != null)
            {
                player.SendChatMessage($"!{{#FF6347}}[BANNED] You are not allowed to play on this server, Ban ID: {ban.SQL_ID}");

                player.SendChatMessage("!{#A9C4E4}Server closed the connection.");


                player.Kick("Banned.");

                return;
            }

            foreach (EPlayer pl in Settings.LoggedInPlayers)
            {
                pl.BlockList.Remove(player);
            }

            Alt.Log($"[Connection] {player.Name} has joined the server. (IP: {player.Ip} - Serial: {player.HardwareIdHash} - SC: {player.SocialClubId})");
            player.SendNotification("~g~Welcome to ~w~Los Santos Copchase!");
            player.SetData("LastCryptAttempt", Environment.TickCount);

            player.Freeze(true);


            player.LoginCameraSetup();

            player.LoginBrowserSetup();
        }

        [ClientEvent("OnScriptedKeyPress")]
        public static void OnScriptedKeyPress(EPlayer player, int keyId, bool keyState) // keyState == true? Down
        {
            if (keyId == 0x5A)
            {
                LobbyData lobby = Settings.Lobbies.Find(lobby => lobby.lobbyId == player.LobbyID);
                if (lobby != null)
                {
                    if (lobby.Cops.Contains(player)) LSCC.Commands.cmd_gps(player);
                }
            }
            return;
        }

        [ScriptEvent(ScriptEventType.PlayerDisconnect)]
        public void OnPlayerDisconnect(EPlayer player, string reason)
        {
            Alt.Log($"[Connection] {player.Name} has left the server. ({reason})");

            Settings.Requests.RemoveAll(a => a.Reporter == player || a.Reported == player);

            try
            {
                Settings.LoggedInPlayers.ForEach((EPlayer pl) =>
                {
                    if (pl.AskedBySquad != null && pl.AskedBySquad == player)
                    {
                        pl.AskedBySquad = null;
                    }
                });
            }
            catch { }

            if (player.LoggedIn)
            {
                if (player.DerbyVehicle != null)
                {
                    player.DerbyVehicle.Remove();

                    player.DerbyVehicle = null;
                }

                if (player.PlayerSquad != null)
                {
                    player.PlayerSquad = null;
                }

                foreach (EPlayer mem in Settings.LoggedInPlayers.Where(p => p.PlayerSquad != null && p.PlayerSquad == player))
                {
                    mem.SendChatMessage("!{#FFAC38}[Partners] This squad has been disbanded as the leader left.");
                    mem.PlayerSquad = null;
                }

                player.Visible = false;

                for (int i = player.FreeroamVehs.Count - 1; i >= 0; i--)
                {
                    if (player.FreeroamVehs[i].Exists && player.FreeroamVehs[i] != null)
                    {
                        player.FreeroamVehs[i].Remove();
                        player.FreeroamVehs.RemoveAt(i);
                    }
                }

                player.FreeroamVehs.Clear();

                
                Settings.LoggedInPlayers.ForEach((EPlayer p) => p.SendChatMessage("!{#4343FA}[PART] !{#FFFFFF}" + player.Name + " has left the server. !{#4343FA}(Timed Out)"));
                  

                player.Save();
            }
        }

        [ScriptEvent(ScriptEventType.PlayerDead)]
        public static void OnPlayerDeath(EPlayer player, IEntity killer, uint weapon)
        {
            if (!player.LoggedIn) return;
            player.CuffingTarget = null;

            string reasonStr;
            bool wasInGame = false;
            if (!Main.WeaponReasons.TryGetValue(weapon, out reasonStr))
            {
                reasonStr = null;
            }
            if (weapon == 696988) reasonStr = "handcuffs";

            if (reasonStr != null)
            {
                reasonStr.Replace(".", "");
            }

            if (player.LobbyID == -4 && Settings.DerbyTimeLeft > 0 && Settings.DerbyTimeLeft != 180)
            {
                foreach (Player indm in Settings.LoggedInPlayers.Where(a => a.LobbyID == -4))
                {
                    indm.Emit("pushToKillFeed", $"DERBY> {player.Name} fell down");
                }
            }

            if (killer != null && killer.Type == BaseObjectType.Player && killer.Exists && killer != player)
            {
                EPlayer pkiller = (EPlayer)killer.NetworkOwner;
                Alt.Log($"[Death] {player.Name} has been killed by {pkiller.Name} using {(reasonStr == null ? "N/A" : reasonStr)}.");
             
                if (pkiller.LobbyID > -1 && pkiller.DMID > -1)
                {
                    int arenaID = pkiller.DMID;

                    foreach (EPlayer indm in Settings.LoggedInPlayers.Where(a => a.LobbyID == pkiller.LobbyID && a.DMID == arenaID))
                    {
                        if (reasonStr == null)
                        {
                            indm.Emit("pushToKillFeed", $"DM> {pkiller.Name} killed {player.Name}");
                        }
                        else
                        {
                            indm.Emit("pushToKillFeed", $"DM> {pkiller.Name} killed {player.Name} with {reasonStr}");
                        }
                    }

                    pkiller.GrantXP(1);
                }
            }
            else
            {
                Alt.Log($"[Death] {player.Name} has died of {(reasonStr == null ? "N/A" : reasonStr)}.");
            }

            LobbyData lobby = Settings.Lobbies.Find(lobby => lobby.lobbyId == player.LobbyID);
            if (lobby != null)
            {
                if (lobby.gameStatus == 1)
                {
                    if (lobby.Fugitives.Contains(player))
                    {
                        wasInGame = true;

                        if (killer != null && killer.Exists)
                        {
                            EPlayer pkiller = (EPlayer)killer.NetworkOwner;
                            if (pkiller.Team == 1) pkiller.GrantXP(2);
                        }

                        foreach (EPlayer ay in Settings.LoggedInPlayers.Where(a => a.LobbyID == lobby.lobbyId))
                        {
                            if (killer != null && !killer.Exists && killer != player)
                            {
                                EPlayer pkiller = (EPlayer)killer.NetworkOwner;
                                if (reasonStr == null)
                                {
                                    ay.Emit("pushToKillFeed", $"{pkiller.Name} killed {player.Name}");
                                }
                                else
                                {
                                    if (weapon == 696988)
                                    {
                                        ay.Emit("pushToKillFeed", $"{pkiller.Name} has apprehended {player.Name}");
                                    }
                                    else ay.Emit("pushToKillFeed", $"{pkiller.Name} killed {player.Name} with {reasonStr}");
                                }
                            }
                            else
                            {
                                ay.SendNotification($"Fugitive ~r~<C>{player.Name}</C> ~w~has died.");

                                if (reasonStr == null)
                                {
                                    ay.Emit("pushToKillFeed", $"{player.Name} died");
                                }
                                else
                                {
                                    ay.Emit("pushToKillFeed", $"{player.Name} died of {reasonStr}");
                                }
                            }
                        }

                        lobby.Fugitives.Remove(player);
                    }

                    if (lobby.Cops.Contains(player))
                    {
                        wasInGame = true;

                        if (killer != null && killer.Exists)
                        {
                            EPlayer pkiller = (EPlayer)killer.NetworkOwner;
                            if (pkiller.Team == 0) pkiller.GrantXP(2);
                        }

                        foreach (EPlayer ay in Settings.LoggedInPlayers.Where(a => a.LobbyID == lobby.lobbyId))
                        {
                            if (killer != null && killer.Exists && player != killer)
                            {
                                EPlayer pkiller = (EPlayer)killer.NetworkOwner;
                                ay.SendNotification($"Cop ~b~<C>{player.Name}</C> ~w~has been taken out by ~r~<C>{pkiller.Name}</C>~w~.");

                                if (reasonStr == null)
                                {
                                    ay.Emit("pushToKillFeed", $"{pkiller.Name} killed {player.Name}");
                                }
                                else
                                {
                                    ay.Emit("pushToKillFeed", $"{pkiller.Name} killed {player.Name} with {reasonStr}");
                                }
                            }
                            else
                            {
                                ay.SendNotification($"Cop ~r~<C>{player.Name}</C> ~w~has died.");

                                if (reasonStr == null)
                                {
                                    ay.Emit("pushToKillFeed", $"{player.Name} died");
                                }
                                else
                                {
                                    ay.Emit("pushToKillFeed", $"{player.Name} died of {reasonStr}");
                                }
                            }
                        }

                        lobby.Cops.Remove(player);
                    }
                }
            }

            player.RemoveAllWeapons();

            AltAsync.Do(() =>
            {
                Task.Delay(3000);
                if (player != null)
                {
                    Random rand = new Random();
                    Position SpawnArea = Settings.LobbySpawns[rand.Next(Settings.LobbySpawns.Length)];
                    if (player.LobbyID == -4) SpawnArea = ((Position)Settings.DerbySpawns[0, 0]).Around(30.0f);
                    if (player.LobbyID > -1 && lobby != null)
                    {
                        int arenaID = player.DMID;
                        if (arenaID > -1)
                        {
                            int spawnID = rand.Next(Settings.DMSpawns[arenaID].GetLength(0));
                            Position dmSpawn = (Position)Settings.DMSpawns[arenaID][spawnID, 0];
                            SpawnArea = (Position)dmSpawn;
                        }
                    }

                    player.Spawn(SpawnArea, 1);
                }

            }, wasInGame == true ? 10000 : 3000);
        }

        [ServerEvent(Event.ChatMessage)]
        public void OnPlayerText(Player player, string message)
        {
            if (!EPlayer.LoggedIn(player))
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You must be logged in to chat!");
                return;
            }

            if (EPlayer.GetAccount(player).MuteTime > 0)
            {
                player.SendChatMessage("!{#FF6347}You are currently muted for " + EPlayer.GetAccount(player).MuteReason + ", seconds left: " + EPlayer.GetAccount(player).MuteTime + ".");
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

        public static void OnPlayerLogin(Player player, Account acc)
        {
            Discord.SendTextChannel("staff-bot", $"*{player.Name} has logged in as {acc.Username}.*");

            EPlayer.SetDiscordStatus(player, $"Logging in as {acc.Username}");
            NAPI.Util.ConsoleOutput($"[Login] {player.Name} has logged in as {acc.Username}. (IP: {player.Address} - Serial: {player.Serial} - SC: {player.SocialClubName})");


            BanData ban = ContextFactory.Instance.Bans.ToList().Find(ban => ban.AccountSQLID == acc.SQL_ID);

            if (ban != null)
            {
                EPlayer.SetDiscordStatus(player, "Gazing at the 'BANNED' screen.");

                player.SendChatMessage($"!{{#FF6347}}[BANNED] You are not allowed to play on this server, Ban ID: {ban.SQL_ID}");
                player.SendChatMessage("!{#A9C4E4}KevY is surprised how you made it this far though.");

                player.SendChatMessage("!{#A9C4E4}Server closed the connection.");

                NAPI.Task.Run(() =>
                {
                    if (player != null) player.Kick("Banned.");
                }, 350);

                return;
            }

            player.Name = acc.Username;
            player.TriggerEvent("destroyBrowser");
            player.SetData("LoggedIn", true);
            player.SetData("AccountHandle", acc);
            player.SetSharedData("pLogged", true);

            CharacterData ConvChar = new CharacterData();

            try
            {
                if (acc.Character == null || acc.Character.Length < 10) throw (new System.Exception("Invalid characterdata- sending to creation."));

                ConvChar = NAPI.Util.FromJson<CharacterData>(acc.Character);
            }
            catch
            {
                player.TriggerEvent("DestroyPlayerCamera");
                player.TriggerEvent("TogglePlayerChat", true);

                EPlayer.ShowCharCreator(player);

                return;
            }

            EPlayer.ApplyCharacter(player, ConvChar, acc.LobbyClothing);

            EPlayer.Spawn(player);
        }


        [RemoteEvent("creator_GenderChange")]
        public static void GenderChange(Player player, int gender)
        {
            if (!EPlayer.LoggedIn(player)) return;

            if (gender == 0) player.SetSkin(PedHash.FreemodeMale01);
            else player.SetSkin(PedHash.FreemodeFemale01);

            player.Position = new Vector3(402.8664, -996.4108, -99.00027);
            player.Heading = -185.0f;
        }

        [RemoteEvent("creator_Save")]
        public void CreatorSave(Player player, byte gender, string parentData, string featureData, string appearanceData, string hairAndColorData)
        {
            if (!EPlayer.LoggedIn(player)) return;

            CharacterData pChar = new CharacterData();
            pChar.Gender = gender;
            ParentData tParentData = NAPI.Util.FromJson<ParentData>(parentData);

            pChar.Parents.Father = tParentData.Father;
            pChar.Parents.Mother = tParentData.Mother;
            pChar.Parents.Similarity = tParentData.Similarity;
            pChar.Parents.SkinSimilarity = tParentData.SkinSimilarity;

            float[] features = NAPI.Util.FromJson<float[]>(featureData);

            for (int i = 0; i < 20; i++)
            {
                try
                {
                    pChar.Features[i] = features[i];
                }
                catch { }
            }

            pChar.Appearance = NAPI.Util.FromJson<AppearanceData[]>(appearanceData);

            byte[] hairAndCol = NAPI.Util.FromJson<byte[]>(hairAndColorData);
            pChar.Hair.Hair = hairAndCol[0];
            pChar.Hair.Color = hairAndCol[1];
            pChar.Hair.HighlightColor = hairAndCol[2];
            pChar.EyebrowColor = hairAndCol[3];
            pChar.BeardColor = hairAndCol[4];
            pChar.EyeColor = hairAndCol[5];
            pChar.BlushColor = hairAndCol[6];
            pChar.LipstickColor = hairAndCol[7];
            pChar.ChestHairColor = hairAndCol[8];

            EPlayer.GetAccount(player).Character = NAPI.Util.ToJson(pChar);
            EPlayer.ApplyCharacter(player, pChar);

            player.SetData("Creator_Using", false);
            player.TriggerEvent("toggleCreator", false);

            EPlayer.Save(player);

            EPlayer.Spawn(player);

            player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You can edit your character's clothing with /char clothing.");
        }

        [RemoteEvent("creator_Leave")]
        public void CreatorLeave(Player player)
        {
            if (!EPlayer.LoggedIn(player)) return;

            CharacterData ConvChar = new CharacterData();
            string PlayerChar = EPlayer.GetAccount(player).Character;

            try
            {
                if (PlayerChar == null || PlayerChar.Length < 10) throw (new System.Exception("Invalid characterdata- sending to creation."));
                ConvChar = NAPI.Util.FromJson<CharacterData>(PlayerChar);

                EPlayer.ApplyCharacter(player, ConvChar);
            }
            catch
            {
                CharacterData charp = new CharacterData();
                EPlayer.GetAccount(player).Character = "{}";
                EPlayer.ApplyCharacter(player, charp); // Give them the default CHAR.
            }

            player.SetData("Creator_Using", false);
            player.TriggerEvent("toggleCreator", false);

            EPlayer.Spawn(player);
        }

        [RemoteEvent("RequestSetClothes")]
        public void OnRequestSetClothes(Player player, int slot, int drawable, int texture)
        {
            if (!EPlayer.LoggedIn(player)) return;

            Account acc = EPlayer.GetAccount(player);

            if ((slot == 1) && acc.DonatorLevel < 1)
            {
                player.SendNotification("~r~Only donators can use this itemgroup (masks).");
                return;
            }

            if (player.GetData<int>("EditingClothing") != -1)
            {
                try
                {
                    if (acc.Character == null || acc.Character.Length < 10) throw (new System.Exception("Invalid characterdata- edit clothes."));

                    CharacterData charData = NAPI.Util.FromJson<CharacterData>(acc.Character);

                    charData.Clothing[player.GetData<int>("EditingClothing")].Clothes[slot].Drawable = drawable;
                    charData.Clothing[player.GetData<int>("EditingClothing")].Clothes[slot].Texture = texture;

                    acc.Character = NAPI.Util.ToJson(charData);
                }
                catch { }
            }

            player.SetClothes(slot, drawable, texture);

        }

        [RemoteEvent("RequestSetGlasses")]
        public void OnRequestSetGlasses(Player player, int drawable, int texture)
        {
            if (!EPlayer.LoggedIn(player)) return;

            if (player.GetData<int>("EditingClothing") != -1)
            {
                Account acc = EPlayer.GetAccount(player);
                try
                {
                    if (acc.Character == null || acc.Character.Length < 10) throw (new System.Exception("Invalid characterdata- edit props."));

                    CharacterData charData = NAPI.Util.FromJson<CharacterData>(acc.Character);

                    charData.Clothing[player.GetData<int>("EditingClothing")].Glasses = drawable;
                    charData.Clothing[player.GetData<int>("EditingClothing")].GlassesTxt = texture;

                    acc.Character = NAPI.Util.ToJson(charData);
                }
                catch { }
            }

            player.SetAccessories(1, drawable, texture);
        }

        [RemoteEvent("RequestSetHat")]
        public void OnRequestSetHat(Player player, int drawable, int texture)
        {
            if (!EPlayer.LoggedIn(player)) return;

            if (player.GetData<int>("EditingClothing") != -1)
            {
                Account acc = EPlayer.GetAccount(player);
                try
                {
                    if (acc.Character == null || acc.Character.Length < 10) throw (new System.Exception("Invalid characterdata- edit props."));

                    CharacterData charData = NAPI.Util.FromJson<CharacterData>(acc.Character);

                    charData.Clothing[player.GetData<int>("EditingClothing")].Hat = drawable;
                    charData.Clothing[player.GetData<int>("EditingClothing")].HatTxt = texture;

                    acc.Character = NAPI.Util.ToJson(charData);
                }
                catch { }
            }

            player.SetAccessories(0, drawable, texture);
        }

        [RemoteEvent("UpdateChatState")]
        public void OnRequestUpdateChatState(Player player, bool toggle)
        {
            if (!EPlayer.LoggedIn(player)) return;

            player.SetSharedData("IsTyping", toggle);
        }

        public static void OnPlayerRegister(Player player, Account acc)
        {
            Discord.SendTextChannel("staff-bot", $"*{player.Name} has registered.*");

            EPlayer.SetDiscordStatus(player, $"Registering as {acc.Username}");
            NAPI.Util.ConsoleOutput($"[Connection] {player.Name} has registered as {acc.Username}. (IP: {player.Address} - Serial: {player.Serial} - SC: {player.SocialClubName})");

            player.SetData("LoggedIn", true);
            player.SetData("AccountHandle", acc);
            player.SetSharedData("pLogged", true);

            player.Name = acc.Username;
            player.TriggerEvent("destroyBrowser");

            player.TriggerEvent("DestroyPlayerCamera");
            player.TriggerEvent("TogglePlayerChat", true);

            EPlayer.ShowCharCreator(player);

            //EPlayer.Spawn(player);
        }

        [RemoteEvent("OnPlayerLoginAttempt")]
        public static void OnPlayerLoginAttempt(Player player, string username, string password)
        {
            NAPI.Util.ConsoleOutput($"[Login Attempt] Username {username}");
            if (Environment.TickCount - player.GetData<int>("LastCryptAttempt") < 1000)
            {
                player.SendNotification("~r~Please wait some time between each login attempt!", true);
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Please wait some time between each login attempt!");
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
            player.SendNotification("~r~Invalid credentials or unable to log in!", true);
            player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Invalid credentials or unable to log in!");

        }

        [RemoteEvent("UpdateStreamData")]
        public void OnPlayerUpdateStreamData(Player player)
        {
            if (player.IsInVehicle) player.SetSharedData("UnstreamedState", 1);
            else player.SetSharedData("UnstreamedState", 0);

            if (player.GetSharedData<int>("Team") == 0 && player.GetSharedData<int>("InLobby") > 0 && player.IsInVehicle && player.Vehicle.HasSharedData("Team"))
            {
                if (player.Vehicle.GetSharedData<int>("Team") == 1)
                {
                    player.SetSharedData("VisibleBlip", true);
                }
            }

            player.SetSharedData("UnstreamedX", player.Position.X);
            player.SetSharedData("UnstreamedY", player.Position.Y);
            player.SetSharedData("UnstreamedZ", player.Position.Z);

            if (EPlayer.LoggedIn(player) && player.GetSharedData<int>("InLobby") == -4)
            {
                if (player.Position.Z <= 15.0f && player.IsInstanceAlive && player.Health > 10)
                {
                    player.Health = 0;
                }

                if (Environment.TickCount - player.GetData<int>("DerbySpawnTick") > 2000)
                {
                    if (player.IsInstanceAlive && player.Health > 10 && !player.IsInVehicle)
                    {
                        player.Health = 0;
                        player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Please do not get out of your vehicle in derby!");
                    }
                }
            }

            return;
        }

        [RemoteEvent("SentLastSeen")]
        public void OnReceiveLastSeen(Player player, string streetName, string zoneName)
        {
            if (!EPlayer.LoggedIn(player) || (player.HasData("VisibleBlip") && player.GetSharedData<bool>("VisibleBlip") == true))
                return;

            bool isGood = false;
            LobbyData lobby = Settings.Lobbies.Find(lobby => lobby.lobbyId == player.GetSharedData<int>("InLobby"));
            if (lobby != null)
            {
                if (lobby.Fugitives.Contains(player) && lobby.gameStatus == 1)
                {
                    isGood = true;
                }
            }

            if (!isGood)
            {
                return;
            }

            string chatStr = $"!{{#FFEC8B}}** [RADIO | FREQ: 911] DISPATCH: Felon {player.Name} last spotted at {streetName}, {zoneName}";

            lobby.Cops.ForEach((Player cop) =>
            {
                cop.SendChatMessage(chatStr);
            });
            return;
        }

        [RemoteEvent("GivePlayerShotTag")]
        public void PlayerRequestShotTag(Player player)
        {
            if (player.GetSharedData<int>("Team") == 0)
            {
                player.SetSharedData("HasShot", true);
                player.SendNotification("~r~You have now shot, opposite team can now safely shoot you and detect you.");
                player.SetSharedData("VisibleBlip", true);
            }
        }

        [RemoteEvent("PlayerShotAtWrongPlayer")]
        public void PlayerShootWrongPlayer(Player player, int caseid)
        {
            if (caseid == 0)
            {
                EPlayer.Freeze(player, true);

                NAPI.Task.Run(() =>
                {
                    if (player != null)
                    {
                        if (!player.IsNull && player != null && player.IsInstanceAlive)
                        {
                            EPlayer.Freeze(player, false);
                        }
                    }
                }, player.IsInVehicle == true ? 500 : 2500);

                int warnings = 0;
                if (player.HasData("TeamShotWarning")) warnings = player.GetData<int>("TeamShotWarning");

                warnings++;
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Do not shoot at teammates, repeatedly doing this will result in punishment!");

                if (warnings >= 7)
                {
                    player.Health = 0;
                    EPlayer.GrantXP(player, -1);
                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You have lost 1XP due to team attacking.");
                }
                if (warnings >= 14)
                {
                    RAGE.Entities.Players.All.ForEach((Player i) =>
                    {
                        i.SendChatMessage($"!{{#FF6347}}AdmCmd: !{{#FFFFFF}}HoeHunter has jailed {player.Name} for 5 minutes due to team-killing.");
                    });
                    EPlayer.Jail(player, 5, "Team Killing (HoeHunter)");
                    warnings = 0;
                }
                player.SetData("TeamShotWarning", warnings);
            }
            else if (caseid == 1 && player.CurrentWeapon != WeaponHash.Stungun)
            {
                EPlayer.Freeze(player, true);

                NAPI.Task.Run(() =>
                {
                    if (player != null)
                    {
                        if (!player.IsNull && player != null && player.IsInstanceAlive)
                        {
                            EPlayer.Freeze(player, false);
                        }
                    }
                }, player.IsInVehicle == true ? 500 : 2500);

                int warnings = 0;
                if (player.HasData("InnocentShotWarning")) warnings = player.GetData<int>("InnocentShotWarning");

                warnings++;
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Do not shoot at fugitives that haven't shot, repeatedly doing this will result in punishment!");

                if (warnings >= 7)
                {
                    player.Health = 0;
                    EPlayer.GrantXP(player, -1);
                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You have lost 1XP due to shooting at a fugitive without shot tag.");
                }
                if (warnings >= 14)
                {
                    RAGE.Entities.Players.All.ForEach((Player i) =>
                    {
                        i.SendChatMessage($"!{{#FF6347}}AdmCmd: !{{#FFFFFF}}HoeHunter has jailed {player.Name} for 5 minutes due to shooting at fugitives that haven't shot.");
                    });
                    EPlayer.Jail(player, 5, "Shooting at fugitives that haven't shot yet (HoeHunter)");
                    warnings = 0;
                }
                player.SetData("InnocentShotWarning", warnings);
            }
        }

        [RemoteEvent("OnPlayerRegisterAttempt")]
        public static void OnPlayerRegisterAttempt(Player player, string email, string username, string password, string confirmpass)
        {
            NAPI.Util.ConsoleOutput($"[Register Attempt] Username {username}");
            if (Environment.TickCount - player.GetData<int>("LastCryptAttempt") < 1000)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Please wait some time between each register attempt!");
                return;
            }
            if (!password.Equals(confirmpass))
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Passwords do not match!");
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

            if (ContextFactory.Instance.Accounts.Count(a => a.HWID.Equals(player.Serial) || a.RockstarID == player.SocialClubId) >= 2)
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You can only have TWO accounts per computer or rockstar ID!");
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
                MuteTime = 0,
                MuteReason = "N/A",
                JailTime = 0,
                JailReason = "N/A",
                Level = 1,
                XP = 1,
                DonatorLevel = 0,
                RockstarID = player.SocialClubId,
                LobbyClothing = 1,
                LoadoutCop = "?",
                LoadoutFugitive = "?",
                CarCop = "?"
            };
            AccountManager.Add(_new);
            OnPlayerRegister(player, _new);
        }

        [RemoteEvent("OnCarSelectorSelect")]
        public void OnSelectorSelectCar(Player player, string carname)
        {
            if (!EPlayer.LoggedIn(player)) return;

            if (player.HasData("ViewingCarSelector") && player.GetData<bool>("ViewingCarSelector") == true)
            {
                if (player.HasData("ViewingCarSelectorTeam"))
                {
                    if (player.GetData<int>("ViewingCarSelectorTeam") == 1)
                    {
                        switch (carname)
                        {
                            case "Default":
                                {
                                    EPlayer.GetAccount(player).CarCop = "?";
                                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Cop car set to Random/Default.");
                                    break;
                                }
                            case "Police Cruiser":
                                {
                                    EPlayer.GetAccount(player).CarCop = "police";
                                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Cop car set to Police Cruiser.");
                                    break;
                                }
                            case "Police Cruiser (U)":
                                {
                                    EPlayer.GetAccount(player).CarCop = "police4";
                                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Cop car set to Unmarked Cruiser.");
                                    break;
                                }
                            case "Police Bike":
                                {
                                    EPlayer.GetAccount(player).CarCop = "policeb";
                                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Cop car set to Police Bike.");
                                    break;
                                }
                            case "Police Buffalo":
                                {
                                    EPlayer.GetAccount(player).CarCop = "polbufac";
                                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Cop car set to Police Buffalo.");
                                    break;
                                }
                            case "Police Buffalo (U)":
                                {
                                    EPlayer.GetAccount(player).CarCop = "polbufac2";
                                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Cop car set to Unmarked Police Buffalo.");
                                    break;
                                }
                            case "FBI Buffalo":
                                {
                                    EPlayer.GetAccount(player).CarCop = "fbibufac";
                                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Cop car set to FBI Buffalo.");
                                    break;
                                }
                            case "FBI Granger":
                                {
                                    EPlayer.GetAccount(player).CarCop = "fbi2";
                                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Cop car set to FBI Granger.");
                                    break;
                                }
                            case "Chopper":
                                {
                                    EPlayer.GetAccount(player).CarCop = "polmav";
                                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Cop car set to Police Chopper.");
                                    break;
                                }
                            case "SWAT Truck":
                                {
                                    EPlayer.GetAccount(player).CarCop = "riot";
                                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Cop car set to SWAT Truck.");
                                    break;
                                }
                        }
                    }
                }
            }
            player.ResetData("ViewingCarSelectorTeam");
            player.ResetData("ViewingCarSelector");
            return;
        }

        [RemoteEvent("RequestCounterUpdate")]
        public void OnPlayerRequestCounterUpdate(Player player, int currentTime, int type)
        {
            if (!EPlayer.LoggedIn(player)) return;

            if (type == 0)
            {
                if (player.GetSharedData<int>("JailTime") <= 0) return;

                Account acc = EPlayer.GetAccount(player);
                acc.JailTime = currentTime;

                player.SetSharedData("JailTime", acc.JailTime);

                if (currentTime <= 0)
                {
                    EPlayer.Freeze(player, false);
                    EPlayer.Spawn(player);
                    ContextFactory.Instance.SaveChanges();
                    player.SendChatMessage("!{#00FF33}You have been unjailed as you have served your time.");
                }
            }
            else if (type == 1)
            {
                if (player.GetSharedData<int>("MuteTime") <= 0) return;

                Account acc = EPlayer.GetAccount(player);
                acc.MuteTime = currentTime;

                player.SetSharedData("MuteTime", acc.MuteTime);

                if (currentTime <= 0)
                {
                    ContextFactory.Instance.SaveChanges();
                    player.SendChatMessage("!{#00FF33}You have been unmuted as you have served your time.");
                }
            }
            return;
        }

        [RemoteEvent("OnPlayerShotPlayer")] // wont trigger incar btw 
        public void OnPlayerShootPlayer(Player player, Vector3 targetPosition, Player target)
        {
            if (player.CurrentWeapon == WeaponHash.Stungun)
            {
                if (target.GetSharedData<bool>("HasShot") != true)
                {
                    LobbyData lobby = Settings.Lobbies.Find(lobby => lobby.lobbyId == player.GetSharedData<int>("InLobby"));
                    if (lobby != null)
                    {
                        if (lobby.Cops.Contains(player) && lobby.Cops.Contains(target))
                        {
                            player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Do not tase your own teammates! You have been given a teamattack warning.");
                            EPlayer.Freeze(player, true);

                            NAPI.Task.Run(() =>
                            {
                                if (player != null)
                                {
                                    if (!player.IsNull && player != null && player.IsInstanceAlive)
                                    {
                                        EPlayer.Freeze(player, false);
                                    }
                                }
                            }, 1500);

                            int warnings = 0;
                            if (player.HasData("TeamShotWarning")) warnings = player.GetData<int>("TeamShotWarning");

                            warnings++;
                            return;
                        }
                    }

                    if (target.HasData("TasedTill") && Environment.TickCount - (target.GetData<int>("TasedTill") - 30000) >= 2000)
                    {
                        int time = 30000;
                        target.TriggerEvent("setToRagdoll", time, time * 2, 0, false, false, false);
                        target.SetData("TasedTill", Environment.TickCount + 30000);
                        Commands.cmd_do(player, $"{target.Name} has been tased and has been rendered immobile.");
                    }
                }
                else
                {
                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You cannot tase a player that has shot.");
                }
            }
        }

        [RemoteEvent("DerbyIncreaseCount")]
        public static void OnIncreaseDerbyCount(Player player)
        {
            if (!EPlayer.LoggedIn(player)) return;
            if (player.GetSharedData<int>("InLobby") != -4) return;

            player.SetSharedData("DerbyTime", player.GetSharedData<int>("DerbyTime") + 1);

            if (Environment.TickCount - player.GetData<int>("DerbySpawnTick") > 2000)
            {
                if (player.IsInstanceAlive && player.Health > 10 && !player.IsInVehicle)
                {
                    player.Health = 0;
                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Please do not get out of your vehicle in derby!");
                }
            }

            if (Environment.TickCount - Settings.LastDerbyTick >= 1000 && Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == -4).Count() >= 2)
            {
                Settings.DerbyTimeLeft--;
                Settings.LastDerbyTick = Environment.TickCount;

                if (Settings.DerbyTimeLeft <= 0)
                {
                    Main.CheckDerbyWinner();
                }
            }
            return;
        }

        [ServerEvent(Event.PlayerSpawn)]
        public static void OnPlayerSpawn(Player player)
        {
            player.ResetData("CuffingTarget");

            if (player.HasData("Cuffed"))
            {
                player.TriggerEvent("SetHandcuffs", false);

                CharacterData ConvChar = new CharacterData();

                Account acc = EPlayer.GetAccount(player);
                try
                {
                    if (acc.Character == null || acc.Character.Length < 10) throw (new System.Exception("Invalid characterdata- sending to creation."));

                    ConvChar = NAPI.Util.FromJson<CharacterData>(acc.Character);
                }
                catch
                {
                    ConvChar = new CharacterData();
                }

                EPlayer.ApplyCharacter(player, ConvChar, acc.LobbyClothing);
                player.ResetData("Cuffed");
            }
            if (player.HasData("Handsup"))
            {
                player.TriggerEvent("SetHandsup", false);
                player.ResetData("Handsup");
            }

            player.SetSharedData("Team", -1);

            LobbyData lobby = Settings.Lobbies.Find(lobby => lobby.lobbyId == player.GetSharedData<int>("InLobby"));
            if (lobby != null)
            {
                if (lobby.Cops.Contains(player)) player.SetSharedData("Team", 1);
                else if (lobby.Fugitives.Contains(player)) player.SetSharedData("Team", 0);
            }

            if (!EPlayer.LoggedIn(player))
            {
                EPlayer.LoginCameraSetup(player);
                EPlayer.LoginBrowserSetup(player);
                return;
            }

            if (player.HasData("Creator_Using") && player.GetData<bool>("Creator_Using") == true)
            {
                player.SetSharedData("InLobby", -5);
                return;
            }


            player.ResetData("FreezeWeapons");

            player.SetSharedData("DerbyTime", 0);

            player.SetSharedData("Level", EPlayer.GetAccount(player).Level);
            player.SetSharedData("Ping", player.Ping);
            player.SetSharedData("HasShot", false);

            if (EPlayer.GetAccount(player).JailTime > 0)
            {
                EPlayer.SetDiscordStatus(player, $"Serving jail time as {player.Name}");

                Account acc = EPlayer.GetAccount(player);

                player.SetData("AutoJoin", false);
                player.SetSharedData("InLobby", -2);
                player.SetSharedData("JailTime", acc.JailTime);
                EPlayer.SetColor(player, new Color(168, 128, 8, 255));
                player.Position = (Vector3)Settings.VectorJail[0];
                player.Heading = (float)Settings.VectorJail[1];
                player.Dimension = 200 + Convert.ToUInt32(player.Id);
                EPlayer.Freeze(player, true);
                player.SendChatMessage($"!{{#FF6347}}[JAILED] !{{#FFFFFF}}You are in jail for {acc.JailTime} seconds, reason: {acc.JailReason}.");

                return;
            }


            if (player.GetData<bool>("UnfairKillSpawn") == true)
            {
                player.SetData("UnfairKillSpawn", false);
                return;
            }

            player.TriggerEvent("HideChaseTimer");
            player.TriggerEvent("SetTimeWeather", 12, 0, 0, -1);

            player.SetSharedData("VisibleBlip", false);

            player.TriggerEvent("SetActorInvulnerable", true);
            player.TriggerEvent("CanFallOutOfCar", false);
            player.TriggerEvent("showHPBars", 0); // Never show HP Bars.

            EPlayer.SetColor(player, new Color(255, 255, 255, 255));


            player.TriggerEvent("ToggleHud", player.GetData<bool>("HUDState"));



            if (player.GetSharedData<int>("InLobby") == -3) // Freeroam
            {
                player.Position = new Vector3(1061.547f, -720.113f, 56.786f);
                player.Heading = 130.497f;
                EPlayer.SetDiscordStatus(player, $"In freeroam as {player.Name}");

                EPlayer.SetColor(player, new Color(102, 232, 217), 0);

                player.TriggerEvent("SetActorInvulnerable", true);

                player.SetSharedData("VisibleBlip", true);

                player.RemoveAllWeapons();

                player.Dimension = 256;
            }

            if (player.GetSharedData<int>("InLobby") == -4) // Derby
            {
                player.SetData("DerbySpawnTick", Environment.TickCount);

                Random rand = new Random();
                int spawnID = rand.Next(Settings.DerbySpawns.GetLength(0));

                if (player.HasData("DerbyVehicle"))
                {
                    player.GetData<Vehicle>("DerbyVehicle").Delete();

                    player.ResetData("DerbyVehicle");
                }

                Vector3 PlayerSpawn = (Vector3)Settings.DerbySpawns[spawnID, 0];
                PlayerSpawn.Z += 30.0f;
                player.Position = PlayerSpawn;
                player.Heading = (float)Settings.DerbySpawns[spawnID, 1];
                player.Dimension = 500;


                EPlayer.SetDiscordStatus(player, $"In derby as {player.Name}");

                player.TriggerEvent("SetActorInvulnerable", true);

                player.SetSharedData("VisibleBlip", true);

                player.RemoveAllWeapons();

                player.TriggerEvent("SetDerbyCounting", false, Settings.DerbyTimeLeft);

                if (Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == -4).Count() >= 2)
                {
                    foreach (Player p in Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == -4))
                    {
                        p.TriggerEvent("SetDerbyCounting", true, Settings.DerbyTimeLeft);
                    }
                }
                else
                {
                    player.SendNotification("~r~Derby time will not be counted unless there are enough players.");
                    Settings.DerbyTimeLeft = 180;
                    Settings.LastDerbyTick = Environment.TickCount;
                }

                Vehicle derbyVeh = NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey("brioso"), (Vector3)Settings.DerbySpawns[spawnID, 0], (float)Settings.DerbySpawns[spawnID, 1], rand.Next(255), rand.Next(255), "DERBY", 255, false, true, 500);
                player.SetData("DerbyVehicle", derbyVeh);

                player.SetIntoVehicle(derbyVeh, 0);

                NAPI.Task.Run(() =>
                {
                    if (player != null)
                    {
                        player.SetIntoVehicle(derbyVeh, 0);
                        EPlayer.SetColor(player, new Color(250, 191, 62), 0);
                    }
                }, 750);
            }

            if (player.GetSharedData<int>("InLobby") > -1) // Copchase Lobby
            {
                EPlayer.SetDiscordStatus(player, $"In lobby {player.GetSharedData<int>("InLobby")} as {player.Name}");

                player.RemoveAllWeapons();

                player.Dimension = 100 + Convert.ToUInt32(player.GetSharedData<int>("InLobby"));

                if (lobby != null)
                {
                    player.TriggerEvent("SetTimeWeather", lobby.Time[0], lobby.Time[1], lobby.Time[2], lobby.Weather);

                    if (lobby.gameStatus == 1)
                    {
                        player.TriggerEvent("StartChaseTimer", lobby.GameProgress, Settings.GameRunTime, lobby.CollisionCounter);
                    }
                }

                int arenaID = player.GetSharedData<int>("InDM");
                if (arenaID > -1 && lobby != null && lobby.gameStatus == 2)
                {
                    arenaID = -1;
                    player.SetSharedData("InDM", -1);
                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}You have been removed from DM as it's locked while copchase starts.");
                }

                if (arenaID > -1)
                {
                    Random rand = new Random();

                    int spawnID = rand.Next(Settings.DMSpawns[arenaID].GetLength(0));
                    Vector3 dmSpawn = (Vector3)Settings.DMSpawns[arenaID][spawnID, 0];
                    player.Position = dmSpawn;
                    player.Heading = (float)Settings.DMSpawns[arenaID][spawnID, 1];

                    player.Health = 100;
                    player.Armor = 100;

                    player.TriggerEvent("SetActorInvulnerable", false);
                    player.SetSharedData("VisibleBlip", true);
                    player.TriggerEvent("showHPBars", 1); // Show if AIMing

                    player.RemoveAllWeapons();

                    player.Dimension = (uint)arenaID + 356 + Convert.ToUInt32(player.GetSharedData<int>("InLobby"));

                    EPlayer.SetDiscordStatus(player, $"In DM {Settings.DMNames[arenaID]} (lobby {player.GetSharedData<int>("InLobby")}) as {player.Name}");
                    EPlayer.SetColor(player, new Color(179, 179, 179), 100);

                    player.GiveWeapon(WeaponHash.Revolver_mk2, 1000);
                    player.GiveWeapon(WeaponHash.Pumpshotgun, 1000);
                    player.GiveWeapon(WeaponHash.Smg_mk2, 1000);
                    player.GiveWeapon(WeaponHash.Assaultrifle_mk2, 1000);
                }
            }
        }
    }
}
