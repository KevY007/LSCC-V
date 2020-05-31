using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using GTANetworkAPI;
using LSCC;

namespace LSCC
{
    public class ParentData
    {
        public byte Father { get; set; }
        public byte Mother { get; set; }
        public float Similarity { get; set; }
        public float SkinSimilarity { get; set; }

        public ParentData()
        {
            this.Father = 0;
            this.Mother = 0;

            this.Similarity = 1.0f;
            this.SkinSimilarity = 1.0f;
        }
    }
    public class HairData
    {
        public byte Hair { get; set; }
        public byte Color { get; set; }
        public byte HighlightColor { get; set; }

        public HairData()
        {
            this.Hair = 0;
            this.Color = 0;
            this.HighlightColor = 0;
        }
    }
    public class AppearanceData
    {
        public byte Value { get; set; }
        public float Opacity { get; set; }

        public AppearanceData()
        {
            this.Value = 255;
            this.Opacity = 1.0f;
        }
    }
    public class AccClothData
    {
        public int Drawable;
        public int Texture;

        public AccClothData()
        {
            this.Drawable = 0;
            this.Texture = -1;
        }
    }

    public class ClothingData
    {
        public AccClothData[] Clothes { get; set; }
        public int Hat { get; set; }
        public int HatTxt { get; set; }
        public int Glasses { get; set; }
        public int GlassesTxt { get; set; }

        public ClothingData()
        {
            this.Clothes = new AccClothData[12];

            for (int i = 0; i < 12; i++)
            {
                this.Clothes[i] = new AccClothData();
            }

            Hat = -1;
            HatTxt = -1;
            Glasses = -1;
            GlassesTxt = -1;
        }
    }
   

    public class CharacterData
    {
        public byte Gender { get; set; }
        public ParentData Parents { get; set; }
        public float[] Features { get; set; }
        public AppearanceData[] Appearance { get; set; }
        public HairData Hair { get; set; }
        public byte EyebrowColor { get; set; }
        public byte BeardColor { get; set; }
        public byte EyeColor { get; set; }
        public byte BlushColor { get; set; }
        public byte LipstickColor { get; set; }
        public byte ChestHairColor { get; set; }
        public ClothingData[] Clothing { get; set; }

        public CharacterData()
        {
            this.Gender = 0;
            this.Parents = new ParentData();
            this.Features = new float[20];
            this.Appearance = new AppearanceData[10];
            this.Clothing = new ClothingData[2];
            
            this.Clothing[0] = new ClothingData(); // Fugitive
            this.Clothing[1] = new ClothingData(); // Cop

            for (int i = 0; i < 20; i++)
            {
                this.Features[i] = 0.0f;
            }

            for (int i = 0; i < 10; i++)
            {
                this.Appearance[i] = new AppearanceData();
            }

            this.Hair = new HairData();

            this.EyebrowColor = 0;
            this.BeardColor = 0;
            this.EyeColor = 0;
            this.BlushColor = 0;
            this.LipstickColor = 0;
            this.ChestHairColor = 0;
        }
    }

    static class EPlayer
    {
        public static byte ColForOverlay(CharacterData character, int index)
        {
            byte color = 0;

            switch (index)
            {
                case 1:
                    color = character.BeardColor;
                    break;

                case 2:
                    color = character.EyebrowColor;
                    break;

                case 5:
                    color = character.BlushColor;
                    break;

                case 8:
                    color = character.LipstickColor;
                    break;

                case 10:
                    color = character.ChestHairColor;
                    break;
            }

            return color;
        }

        public static void ApplyCharacter(Player player, CharacterData playerChar, int teamID = 1)
        {
            if(playerChar == null)
            {
                playerChar = new CharacterData();
                NAPI.Util.ConsoleOutput($"{player.Name} has no char, applying default");
            }

            player.SetAccessories(0, 0, 0);
            player.SetAccessories(1, 0, 0);
            player.SetAccessories(2, 0, 0);

            for (int i = 0; i < 12; i++)
            {
                Decoration deco;
                deco.Collection = 0;
                deco.Overlay = 0;

                player.SetDecoration(deco);
            }
           

            player.ClearAccessory(0);
            player.ClearAccessory(1);
            player.ClearAccessory(2);

            player.ClearDecorations();

            player.TriggerEvent("char_ResetItems");

            if (playerChar.Gender == 0) player.SetSkin(PedHash.FreemodeMale01);
            else player.SetSkin(PedHash.FreemodeFemale01);

            HeadBlend a;

            a.ShapeFirst = playerChar.Parents.Mother;
            a.ShapeSecond = playerChar.Parents.Father;
            a.ShapeThird = 0;

            a.SkinFirst = playerChar.Parents.Mother;
            a.SkinSecond = playerChar.Parents.Father;
            a.SkinThird = 0;

            a.SkinMix = playerChar.Parents.SkinSimilarity;
            a.ShapeMix = playerChar.Parents.Similarity;
            a.ThirdMix = 0.0f;

            Dictionary<int, HeadOverlay> headOverlays = new Dictionary<int, HeadOverlay>();
            for (byte i = 0; i < 10; i++)
            {
                HeadOverlay b;
                b.Color = ColForOverlay(playerChar, i);
                b.Index = playerChar.Appearance[i].Value;
                b.Opacity = playerChar.Appearance[i].Opacity;
                
                b.SecondaryColor = 0;
                headOverlays.Add(i, b);
            }

            player.SetCustomization(playerChar.Gender == 0, a, playerChar.EyeColor, playerChar.Hair.Color, playerChar.Hair.HighlightColor, playerChar.Features, 
                headOverlays, new Decoration[0]);


            var clothDictionary = new Dictionary<int, ComponentVariation>();

            for (int i = 0; i < 12; i++)
            {
                if (playerChar.Clothing[teamID].Clothes[i].Texture != -1)
                {
                    clothDictionary.Add(i, new ComponentVariation { Drawable = playerChar.Clothing[teamID].Clothes[i].Drawable, 
                        Texture = playerChar.Clothing[teamID].Clothes[i].Texture });
                }
            }
            
            player.SetClothes(clothDictionary);
            player.SetClothes(2, playerChar.Hair.Hair, 0);


            if (playerChar.Clothing[teamID].Hat != -1 && playerChar.Clothing[teamID].HatTxt != -1)
            { 
                player.SetAccessories(0, playerChar.Clothing[teamID].Hat, playerChar.Clothing[teamID].HatTxt);
            }
            if (playerChar.Clothing[teamID].Glasses != -1 && playerChar.Clothing[teamID].GlassesTxt != -1)
            {
                player.SetAccessories(1, playerChar.Clothing[teamID].Glasses, playerChar.Clothing[teamID].GlassesTxt);
            }
        }

        public static void ShowCharCreator(Player player)
        {
            if (!EPlayer.LoggedIn(player)) return;

            foreach (LobbyData il in Settings.Lobbies)
            {
                il.Cops.Remove(player);
                il.Fugitives.Remove(player);
            }

            player.SetSharedData("Team", -1);
            player.SetSharedData("InLobby", -5);

            player.SetData("Creator_Using", true);

            player.Dimension = (uint)player.Id + 699;

            player.SendChatMessage("!{#FF6347}[INFO] !{#FFFFFF}Switch gender between Male and Female if the preview does not show.");

            CharacterData defaultChar = new CharacterData();
            EPlayer.ApplyCharacter(player, defaultChar);

            player.Position = new Vector3(402.8664, -996.4108, -99.00027);
            player.Heading = -185.0f;

            NAPI.Task.Run(() =>
            {
                player.TriggerEvent("toggleCreator", true, NAPI.Util.ToJson(defaultChar));

                player.SetSkin(PedHash.FreemodeMale01);
                player.TriggerEvent("creator_Change");

                player.Position = new Vector3(402.8664, -996.4108, -99.00027);
                player.Heading = -185.0f;
            }, 2000);

            return;
        }

        public static Random rand = new Random();

        public static void ResetPlayerData(Player player)
        {
            player.ResetData("AccountHandle");
            player.SetData("LoggedIn", false);
            player.SetData("TeamShotWarning", 0);
            player.SetData("UnfairKillSpawn", false);
            player.SetData("InnocentShotWarning", 0);
            player.SetSharedData("pLogged", false);
            player.SetData("AutoJoin", true);
            player.SetData("HUDState", true);
            player.SetSharedData("DerbyTime", 0);
            player.SetData("DerbySpawnTick", 0);
            player.SetSharedData("HasShot", false);
            player.SetSharedData("VoiceMode", 0);
            player.SetSharedData("VisibleBlip", false);
            player.SetData("PMState", true);
            player.SetSharedData("Team", -1);
            player.TriggerEvent("ToggleHud", false);
            player.SetData("GPS", false);
            player.SetData("EditingClothing", -1);
            player.SetData("Creator_Using", false);
            player.ResetData("BlockList");
            player.SetSharedData("InLobby", -1);
            player.SetSharedData("InDM", -1);
            player.SetSharedData("IsTyping", false);
            player.SetSharedData("Color_R", 255);
            player.SetSharedData("Color_G", 255);
            player.SetSharedData("Color_B", 255);
            player.SetSharedData("Ping", player.Ping);
            player.SetSharedData("Level", 1);
            player.SetSharedData("JailTime", 0);
            player.SetSharedData("MuteTime", 0);
            player.SetSharedData("UnstreamedState", 0);
            player.SetSharedData("UnstreamedX", 0.0f);
            player.SetSharedData("UnstreamedY", 0.0f);
            player.SetSharedData("UnstreamedZ", 0.0f);

            List<Vehicle> temp_FreeroamVeh = new List<Vehicle>();
            player.SetData("FreeroamVehicles", temp_FreeroamVeh);

            foreach (LobbyData il in Settings.Lobbies)
            {
                il.Cops.Remove(player);
                il.Fugitives.Remove(player);
            }
            Settings.LoggedInPlayers.Remove(player);
        }

        public static void Jail(Player player, int minutes, string reason)
        {
            if (!EPlayer.LoggedIn(player)) return;

            Account Acc = EPlayer.GetAccount(player);
            Acc.JailReason = reason;
            Acc.JailTime = minutes * 60;
            player.SendChatMessage($"!{{#FF6347}}[!] !{{#FFFFFF}}You have been jailed for {minutes} minutes, reason: {reason}.");
            foreach (LobbyData il in Settings.Lobbies)
            {
                il.Cops.Remove(player);
                il.Fugitives.Remove(player);
            }
            player.SetSharedData("JailTime", Acc.JailTime);
            player.TriggerEvent("SetJailCounter", Acc.JailTime);
            NAPI.Player.SpawnPlayer(player, (Vector3)Settings.VectorJail[0], (float)Settings.VectorJail[1]);
        }

        public static void SetDiscordStatus(Player player, string status)
        {
            player.SetSharedData("Ping", player.Ping);
            player.TriggerEvent("SetDiscordStatus", "Los Santos Copchase", status);
        }
        public static void Freeze(Player player, bool toggle)
        {
            player.TriggerEvent("SetPlayerFrozen", toggle);

            if (toggle)
            {
                List<Tuple<WeaponHash, int>> wepData = new List<Tuple<WeaponHash, int>>();

                if (player.HasData("FreezeWeapons"))
                {
                    wepData = player.GetData<List<Tuple<WeaponHash, int>>>("FreezeWeapons");
                }

                var weapons = Enum.GetValues(typeof(WeaponHash)).Cast<WeaponHash>();

                weapons.ToList().ForEach(wep =>
                {
                    int ammo = player.GetWeaponAmmo(wep);
                    if (ammo > 0)
                    {
                        wepData.Add(new Tuple<WeaponHash, int>(wep, ammo));
                    }
                });

                player.SetData("FreezeWeapons", wepData);

                player.RemoveAllWeapons();
            }
            else
            {
                if (player.HasData("FreezeWeapons"))
                {
                    List<Tuple<WeaponHash, int>> wepData = player.GetData<List<Tuple<WeaponHash, int>>>("FreezeWeapons");

                    wepData.ForEach(wepTuple =>
                    {
                        player.GiveWeapon(wepTuple.Item1, wepTuple.Item2);
                    });

                    player.ResetData("FreezeWeapons");
                }
            }
        }

        public static void Action(Player player, string text)
        {
            var msg = "!{#C3A3DB}* " + player.Name + " " + text;
           
            foreach (Player playa in NAPI.Player.GetPlayersInRadiusOfPlayer(40, player))
            {
                playa.SendChatMessage(msg);
            }
        }

        public static void Handcuff(Player player, bool toggle)
        {
            player.TriggerEvent("SetHandcuffs", toggle);
            player.SetData("Cuffed", true);

            if(toggle) player.PlayAnimation("mp_arresting", "idle", 0);
        }

        public static void SetColor(Player player, Color col, int armour = 0)
        {
            player.SetSharedData("Color_R", col.Red);
            player.SetSharedData("Color_G", col.Green);
            player.SetSharedData("Color_B", col.Blue);
            foreach (Player p in RAGE.Entities.Players.All)
            {
                p.TriggerEvent("playerNametagSet", player, col.Red, col.Green, col.Blue, col.Alpha, armour);
            }
        }

        public static bool LoggedIn(Player player)
        {
            player.SetSharedData("Ping", player.Ping);
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
        public static int GetMaxLevelXP(int level)
        {
            return (level * 8);
        }
        public static bool CheckXPLevelUp(int level, int xp)
        {
            if (xp == 0) return false;

            return (xp >= GetMaxLevelXP(level) ? true : false);
        }
        public static void GrantXP(Player player, int xp)
        {
            if (!EPlayer.LoggedIn(player)) return;

            Account acc = EPlayer.GetAccount(player); 
            acc.XP += xp;
               
            if(acc.XP < 0)
            {
                if (acc.Level > 0)
                {
                    acc.Level--;

                    acc.XP += GetMaxLevelXP(acc.Level);
                    RAGE.Entities.Players.All.ForEach((Player pl) =>
                    {
                        pl.SendChatMessage($"!{{#00BA03}}[LEVEL DOWN] {player.Name} has levelled down to {acc.Level}!");
                    });
                    player.SendNotification($"~w~You have levelled down to ~r~{acc.Level}~w~!");
                }
            }

            while(CheckXPLevelUp(acc.Level, acc.XP) == true)
            {
                acc.XP -= GetMaxLevelXP(acc.Level);
                acc.Level++;
                RAGE.Entities.Players.All.ForEach((Player pl) =>
                {
                    pl.SendChatMessage($"!{{#00BA03}}[LEVEL UP] {player.Name} has levelled up to {acc.Level}!");
                });
                player.SendNotification($"~w~You have levelled up to ~g~{acc.Level}~w~!");
            }

            player.SetSharedData("Level", acc.Level);
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
        public static void Spawn(Player player, bool forceAutojoin = true)
        {
            if (!LoggedIn(player)) return;

            if(!Settings.LoggedInPlayers.Contains(player)) Settings.LoggedInPlayers.Add(player);

            CharacterData ConvChar = new CharacterData();

            try
            {
                ConvChar = NAPI.Util.FromJson<CharacterData>(EPlayer.GetAccount(player).Character);
               
                EPlayer.ApplyCharacter(player, ConvChar, EPlayer.GetAccount(player).LobbyClothing);
            }
            catch { }

            Vector3 SpawnArea = Settings.LobbySpawns[rand.Next(Settings.LobbySpawns.Length)];

            player.TriggerEvent("DestroyPlayerCamera");
            player.TriggerEvent("TogglePlayerChat", true);


            if (forceAutojoin && player.GetData<bool>("AutoJoin") == false)
            {
                NAPI.Notification.SendNotificationToPlayer(player, $"~g~Autojoin has been enabled.", false);
                player.SetData("AutoJoin", true);
            }

            if (player.GetSharedData<int>("InLobby") == -1 || player.GetSharedData<int>("InLobby") == -5 && player.GetData<bool>("Creator_Using") == false)
            {
                player.SetSharedData("InDM", -1);
                int LobbyToBeIn = Settings.MaxLobbies;
                foreach (LobbyData lobby in Settings.Lobbies)
                {
                    if (Settings.LoggedInPlayers.Count(a => a.GetSharedData<int>("InLobby") == lobby.lobbyId) >= Settings.MaxPlayersInLobby)
                    {
                        continue;
                    }
                    if ((lobby.PlayerCreated && lobby.Password.Length > 1) || !lobby.Enabled) continue;

                    LobbyToBeIn = lobby.lobbyId;
                    break;
                }

                player.SetSharedData("InLobby", LobbyToBeIn);

                foreach (Player targ in Settings.LoggedInPlayers.Where(a => a.GetSharedData<int>("InLobby") == LobbyToBeIn))
                {
                    targ.SendChatMessage($"[Lobby {LobbyToBeIn}] {player.Name} has joined this lobby.");
                }
            }

            

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
                if (player.GetSharedData<int>("InLobby") == lobbyid && player.GetData<bool>("AutoJoin") == true)
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
