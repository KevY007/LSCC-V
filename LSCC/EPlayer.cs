using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using GTANetworkAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;
using RNC;
using Microsoft.EntityFrameworkCore.Query;

namespace RNC
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

    public partial class wComponentData
    {
        [JsonProperty("Components")]
        public List<long> Components { get; set; }

        public wComponentData()
        {
            Components = new List<long>();
        }

        public static Dictionary<long, wComponentData> FromJson(string json) => JsonConvert.DeserializeObject<Dictionary<long, wComponentData>>(json, new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        });
    }
    public class LoadoutSelWepData
    {
        public string[] Images { get; set; }
        public string[] Weapons { get; set; }
        public int[] WeaponAmmos { get; set; }

        public LoadoutSelWepData(int numweps)
        {
            this.Images = new string[numweps];
            this.Weapons = new string[numweps];
            this.WeaponAmmos = new int[numweps];
        }
    }

    public class LoadoutSelectorData
    {
        public string[] Names { get; set; }
        public int[] Levels { get; set; }
        public int[] Teams { get; set; }
        public LoadoutSelWepData[] Weapons { get; set; }
        public bool[] Locked { get; set; }

        public LoadoutSelectorData(object[,] loadoutData, int level)
        {
            int numloadouts = loadoutData.GetLength(0);

            this.Names = new string[numloadouts];
            this.Levels = new int[numloadouts];
            this.Teams = new int[numloadouts];
            this.Weapons = new LoadoutSelWepData[numloadouts];
            this.Locked = new bool[numloadouts];

            for (int i = 0; i < loadoutData.GetLength(0); i++)
            {
                this.Names[i] = (string)loadoutData[i, 0];
                this.Levels[i] = (int)loadoutData[i, 1];
                this.Teams[i] = (int)loadoutData[i, 2];

                object[,] items = NAPI.Util.FromJson<object[,]>(loadoutData[i, 3]);
                this.Weapons[i] = new LoadoutSelWepData(items.GetLength(0));

                for (int j = 0; j < items.GetLength(0); j++) // Num weps
                {
                    this.Weapons[i].Images[j] = (string)items[j, 0];
                    this.Weapons[i].Weapons[j] = (string)items[j, 1];
                    this.Weapons[i].WeaponAmmos[j] = (int)((long)items[j, 2]);
                }

                this.Locked[i] = (level >= this.Levels[i] ? false : true);
            }
        }
    }

    static class EPlayer
    {
        static string Translate(Player player, string message_code, params object[] args)
        {
            return Translator.Translate(player, message_code, args);
        }


        public static string GetAdminLevelName(int level, bool colored = false)
        {
            string name = "";
            switch (level)
            {
                case 1:
                    if (colored) name = "!{#EE9A4D}";
                    name = name + "Moderator";
                    break;
                case 2:
                    if (colored) name = "!{#298EFF}";
                    name = name + "Trial Admin";
                    break;
                case 3:
                    if (colored) name = "!{#00FF00}";
                    name = name + "General Admin";
                    break;
                case 4:
                    if (colored) name = "!{#FF0000}";
                    name = name + "Senior Admin";
                    break;
                case 5:
                    if (colored) name = "!{#990000}";
                    name = name + "Lead Admin";
                    break;
                case 6:
                    if (colored) name = "!{#AB0707}";
                    name = name + "Management";
                    break;
                default:
                    name = "Player";
                    break;
            }
            return name;
        }

        public static string GetDonatorLevelName(int level, bool colored = false)
        {
            string name = "";
            switch (level)
            {
                case 1:
                    if (colored) name = "!{#CD7F32}";
                    name = name + "Bronze Donator";
                    break;
                case 2:
                    if (colored) name = "!{#C0C0C0}";
                    name = name + "Silver Donator";
                    break;
                case 3:
                    if (colored) name = "!{#FFD700}";
                    name = name + "Gold Donator";
                    break;
                default:
                    name = "Player";
                    break;
            }
            return name;
        }

        public static void SyncServerSettings(Player player)
        {
            lock (player)
            {
                player.TriggerEvent("SyncServerSettings", Settings.Dynamics.OnfootCollision, Settings.Dynamics.CameraSettingsUpdate);
            }
        }

        #region DM Functions
        public async static Task<KeyValuePair<CustomDMArenas, bool>> IsValidDMArena(int arenaID)
        {
            using (MainDatabase context = ContextFactory.CreateContext())
            {
                var arena = await context.CustomArenas.FindAsync(arenaID);

                if (arena == null || arena == default(CustomDMArenas)) return new KeyValuePair<CustomDMArenas, bool>(arena, false);
                return new KeyValuePair<CustomDMArenas, bool>(arena, true);
            }
        }

        public async static Task<KeyValuePair<CustomDMArenaTeams, bool>> IsValidDMTeam(int arenaID, int teamID)
        {
            var arena = await IsValidDMArena(arenaID);

            if (arena.Value == false) return new KeyValuePair<CustomDMArenaTeams, bool>(null, false);

            using (MainDatabase context = ContextFactory.CreateContext())
            {
                var team = await context.CustomArenas_Teams.AsQueryable().FirstOrDefaultAsync(a => a.ArenaID == arenaID && a.Team == teamID);

                if (team == null || team == default(CustomDMArenaTeams)) return new KeyValuePair<CustomDMArenaTeams, bool>(team, false);

                return new KeyValuePair<CustomDMArenaTeams, bool>(team, true);
            }
        }
        public async static Task<KeyValuePair<CustomDMArenaSpawns, bool>> IsValidDMSpawn(int arenaID, int teamID, int spawnID)
        {
            var team = await IsValidDMTeam(arenaID, teamID);
            if (team.Value == false) return new KeyValuePair<CustomDMArenaSpawns, bool>(null, false);

            using (MainDatabase context = ContextFactory.CreateContext())
            {
                var spawn = await context.CustomArenas_Spawns.AsQueryable().FirstOrDefaultAsync(a => a.ArenaID == arenaID && a.Team == teamID && a.SQL_ID == spawnID);

                if (spawn == null || spawn == default(CustomDMArenaSpawns)) return new KeyValuePair<CustomDMArenaSpawns, bool>(spawn, false);

                return new KeyValuePair<CustomDMArenaSpawns, bool>(spawn, true);
            }

        }
        public async static Task<KeyValuePair<CustomDMArenaSpawns, bool>> IsValidDMSpawn(int spawnID)
        {
            using (MainDatabase context = ContextFactory.CreateContext())
            {
                var spawn = await context.CustomArenas_Spawns.AsQueryable().FirstOrDefaultAsync(a => a.SQL_ID == spawnID);

                if (spawn == null || spawn == default(CustomDMArenaSpawns)) return new KeyValuePair<CustomDMArenaSpawns, bool>(spawn, false);

                return new KeyValuePair<CustomDMArenaSpawns, bool>(spawn, true);
            }
        }

        public async static Task<KeyValuePair<DM_WeaponSets, bool>> IsValidDMWeaponSet(int arenaID, int teamID, int index)
        {
            var team = await IsValidDMTeam(arenaID, teamID);
            if (team.Value == false) return new KeyValuePair<DM_WeaponSets, bool>(null, false);

            DM_WeaponSets[] weaponSets = NAPI.Util.FromJson<DM_WeaponSets[]>(team.Key.WeaponSets);
            if (index >= weaponSets.Length || index < weaponSets.Length - 1 || weaponSets.Length == 0) return new KeyValuePair<DM_WeaponSets, bool>(null, false);

            return new KeyValuePair<DM_WeaponSets, bool>(weaponSets.ElementAt(index), true);
        }
        #endregion

        class MenuItemObject
        {
            public ItemData itemData { get; set; }
            public int owned { get; set; }
            public int max { get; set; }
        }
        public static void Menu_UpdateAllItems(Player player)
        {
            if (!EPlayer.LoggedIn(player)) return;

            Account acc = EPlayer.GetAccount(player);

            List<MenuItemObject> itemsArray = new List<MenuItemObject>();
            foreach (ItemData item in ItemData.Items)
            {
                int owned = 0;
                int max = 0;
                switch (item.Type)
                {
                    case "medkit":
                        max = 4;
                        if (EPlayer.GetAccount(player).DonatorLevel == 1) max = 6;
                        else if (EPlayer.GetAccount(player).DonatorLevel == 2) max = 10;
                        else if (EPlayer.GetAccount(player).DonatorLevel == 3) max = 15;
                        owned = acc.Medkits;
                        break;
                    case "painkiller":
                        owned = acc.Painkillers;
                        max = 4;
                        if (EPlayer.GetAccount(player).DonatorLevel == 1) max = 6;
                        else if (EPlayer.GetAccount(player).DonatorLevel == 2) max = 10;
                        else if (EPlayer.GetAccount(player).DonatorLevel == 3) max = 15;
                        break;
                    case "lockpick":
                        owned = acc.Lockpicks;
                        max = 10;
                        if (EPlayer.GetAccount(player).DonatorLevel == 1) max = 15;
                        else if (EPlayer.GetAccount(player).DonatorLevel == 2) max = 20;
                        else if (EPlayer.GetAccount(player).DonatorLevel == 3) max = 25;
                        break;
                }

                MenuItemObject itemObject = new MenuItemObject()
                {
                    itemData = item,
                    max = max,
                    owned = owned
                };
                itemsArray.Add(itemObject);
            }
            string itemsArrayJson = JsonConvert.SerializeObject(itemsArray, Newtonsoft.Json.Formatting.Indented);
            player.TriggerEvent("UpdateMenuItemsTab", itemsArrayJson);
        }

        public static void Menu_AddItems(Player player)
        {
            if (!EPlayer.LoggedIn(player)) return;

            Menu_UpdateAllItems(player);
        }

        public static void UpdateMaxMenuItems(Player player)
        {
            if (!EPlayer.LoggedIn(player)) return;

            Menu_UpdateAllItems(player);    
        }

        public static void Menu_UpdateItem(Player player, int ID, int owned)
        {
            if (!EPlayer.LoggedIn(player)) return;

            Menu_UpdateAllItems(player);
        }

        public static string Name(Player player, bool override_anonymous = false)
        {
            if (override_anonymous) return player.Name;

            if(player.GetSharedDataSafe<bool>("FakeNameOn") == true && 
                player.GetSharedDataSafe<string>("FakeName").Length > 0 && 
                EPlayer.GetAccount(player).Admin >= 4)
            {
                return player.GetSharedDataSafe<string>("FakeName");
            }

            return player.Name;
        }

        public static void RemovePlayerFromCopchase(Player i, LobbyData lobby, int winnerTeam = -1, int reason = -1)
        {
            if (i.GetSharedDataSafe<int>("InLobby") != lobby.lobbyId) return;

            EPlayer.ResetCopchaseVariables(i);

            if(winnerTeam != -1)
            {
                if (lobby.Cops.Contains(i) && winnerTeam == 1)
                {
                    EPlayer.TryAdvanceChallenge(i, Challenge.ChallengeType.Win, "cop");
                }
                if (lobby.Fugitives.Contains(i) && winnerTeam == 0)
                {
                    EPlayer.TryAdvanceChallenge(i, Challenge.ChallengeType.Win, "fugitive");

                    if (lobby.gameMode != GameModes.BANKROBBERY && reason == 1)
                    {
                        EPlayer.TryAdvanceChallenge(i, Challenge.ChallengeType.Win, "first_escape");
                    }
                }
            }
   

            if (i.GetData<bool>("ShotsFired") == false)
            {
                if (lobby.Fugitives.Contains(i) && winnerTeam == 0)
                {
                    EPlayer.UnlockAchievement(i, "no_shooting_win");
                }
            }
            if (lobby.gameMode == GameModes.BUSTED && winnerTeam == 0 && reason == 1 && lobby.Fugitives.Contains(i))
            {
                EPlayer.UnlockAchievement(i, "first_busted");
            }

            i.SetData<bool>("ShotsFired", false);

            if (i.HasSharedData("Wounded"))
            {
                if (i.GetSharedDataSafe<int>("Wounded") == 2)
                {
                    Events.RequestClearAnimation(i);
                    i.StopAnimation();
                }
            }
            Events.OnPlayerWounded(i, 0, -1);

            if (lobby.gameMode == GameModes.BANKROBBERY)
            {
                Account a = EPlayer.GetAccount(i);

                Character.ApplyAppearance(i, Character.GetCharacter(i).SavedAppearance);
                Character.SetTeamOutfit(i, a.LobbyClothing);
            }
            if (i.HasData("Spectating") && EPlayer.GetAccount(i).Admin < 1)
            {
                Commands.cmd_specoff(i);
            }

            i.TriggerEvent("HideChaseTimer");
            if (lobby.Cops.Contains(i))
            {
                lobby.Cops.Remove(i);
                EPlayer.Spawn(i);
            }
            if (lobby.Fugitives.Contains(i))
            {
                lobby.Fugitives.Remove(i);
                EPlayer.Spawn(i);
            }
            i.SetSharedData("HeistLeader", false);
            EPlayer.Save(i);
        }

        public static void ResetCopchaseVariables(Player player)
        {
            player.TriggerEvent("EVENTS:SetBankRobberyActive", false);
            player.SetSharedData("HeistLeader", false);

            EPlayer.StopCarry(player);
            player.TriggerEvent("Teleport:ResetCopCanEnter");
            player.ResetSharedData("InteriorsEntered");

            player.SetSharedData("IsBeingCuffed", false);
            player.SetSharedData("IsCuffed", false);
            player.SetSharedData("CuffedBy", "n/a");
            player.TriggerEvent("forceStopCuffing");

            player.TriggerEvent("stopCopchaseTimerEarlyWarningEffect");
            player.TriggerEvent("PoliceHeliCam_ClearFugitives");

            player.TriggerEvent("hideMDC");
            player.TriggerEvent("MDC:RemoveAllBlips");

            player.TriggerEvent("Bleeding:ClearAllDecals");
            player.TriggerEvent("Bleeding:SetActive", false);

            player.TriggerEvent("ClearClimbTaseProtection", true);
            player.SetSharedData("ClimbProtection", false);
            player.SetData("LastClimbProtection", Environment.TickCount64 - 30000);
            player.SetData("ClimbProtectionProcs", 0);
            player.SetData("ClimbProtectionEnd", Environment.TickCount64);

            player.SetData("IsClanPartnered", false);

            player.TriggerEvent("resetBuildingBlips");      
            player.TriggerEvent("ResetKillFeed");

            player.SetSharedData("HeistLeader", false);
            player.TriggerEvent("BankRob_ClearFugiCarBlips");
            player.TriggerEvent("BankRob_ClearFinishMarker");
            player.TriggerEvent("BankRob_SetMusic", -1);

            player.ResetSharedData("Handgun_Hash");

            player.TriggerEvent("stopLockpicking");
            player.TriggerEvent("StopHotwire");

            player.ResetData("DragoutVehicle");
            player.ResetData("ExitedDragoutVehicle");

            player.SetSharedData("BustedProgress", 0.0);
            player.ResetData("BustSupporters");

            player.SetSharedData("Wounded", 0);
            player.SetSharedData("LastKiller", "n/a");
            player.SetSharedData("BeingRevived", false);
            player.SetSharedData("ReviveTarget", "n/a");

        }

        public static void ResetPlayerData(Player player)
        {
            lock (player)
            {
                player.SetSharedData("SQL_ID", -1);
                player.ResetData("Kicked");
                player.ResetData("Dead");
                player.SetSharedData("DonatorPoints", 0);
                player.SetSharedData("Invincible", false);
                player.SetSharedData("ShowDamaged", false);
                player.SetSharedData("IsChatOpen", false);
                player.SetData("SpawnTick", Environment.TickCount64);
                player.ResetData("AccountHandle");
                player.ResetData("CHARACTER:EditorOpen");
                player.ResetData("CompNoRefundWarn");
                player.ResetData("AttachedComponents");
                player.SetSharedData("NametagHack", false);
                player.SetSharedData("VehicleDL", false); // for /DL like in SAMP
                player.ResetData("LastPMFrom");
                player.SetSharedData("LastVehicleExit", 0);
                player.SetSharedData("Invincible", false);

                List<int> tracers = new List<int>();
                string tracersJSON = NAPI.Util.ToJson(tracers);
                player.SetSharedData("Admin_BulletTracers", tracersJSON);
                player.SetSharedData("Admin_TraceTarget", -1);

                player.SetSharedData("IsBeingCuffed", false);
                player.SetSharedData("IsCuffed", false);

                player.SetSharedData("BadgeData", "[]");

                player.SetSharedData("FakeNameOn", false);
                player.SetSharedData("FakeName", "");
                player.SetSharedData("FakeNamePing", 0);
                player.SetSharedData("FakeNameLevel", 0);

                player.SetSharedData("Menu_LastClanPageID", 0);
                player.SetSharedData("AutoOOC", false);

                player.SetSharedData("TaseProtection", false);
                player.SetSharedData("CharCreator_ShowClothingAfter", false);

                player.SetData<bool>("LoadingClanCreation", false);

                player.SetSharedData("LaserEquipped", false);

                player.SetSharedData("LastVehicleExit", 0);


                player.ResetData("LockpickTarget");
                player.SetSharedData("PlacingStingers", false);

                player.SetSharedData("global.WeaponEditorActive", false);
                player.SetData<bool>("ShotsFired", false);

                player.ResetData("DragoutVehicle");
                player.ResetData("ExitedDragoutVehicle");

                player.SetSharedData("Level", 0);
                player.SetSharedData("CurrentXP", 0);
                player.SetSharedData("NextLevelXP", 0);

                player.SetSharedData("ClanPublic", false);

                player.SetData<string>("BankRobberyUnit", "fib");
                player.SetSharedData("HeistLeader", false);

                player.ResetSharedData("IsAiming");
                player.ResetSharedData("CameraDirectionX");
                player.ResetSharedData("CameraDirectionY");
                player.ResetSharedData("CameraDirectionZ");
                player.ResetSharedData("FlashlightEquipped");
                player.ResetSharedData("UpdateCameraData");

                player.SetSharedData("Wounded", 0);
                player.SetSharedData("LastKiller", "n/a");
                player.SetSharedData("BeingRevived", false);
                player.SetSharedData("ReviveTarget", "n/a");

                player.SetSharedData("CarryingPlayer", -1);
                player.SetSharedData("CarriedBy", -1);

                player.ResetData("TunerNoRefundWarn");
                player.ResetData("ConvertingType");
                player.ResetData("ConvertingAmount");
                player.ResetData("GIF_Creating");
                player.ResetData("EditingDMArena");

                player.SetSharedData("Admin", 0);

                ResetShotStats(player);
                player.SetData<long>("JoinTimeUnix", 0);
                player.SetData("TeamShotWarning", 0);
                player.SetData("UnfairKillSpawn", false);
                player.SetData("InnocentShotWarning", 0);
                player.SetSharedData("AutoJoin", true);
                player.SetSharedData("LastVehEnterExit", 0);
                player.SetData("LastVehEnterSeat", (sbyte)-1);
                player.SetSharedData("DerbyTime", 0);
                player.SetSharedData("EntAlpha", 0);
                player.SetSharedData("Collisions", true);
                player.SetSharedData("WantsDriveby", true);
                player.SetSharedData("FPS", -1);
                player.SetData("DerbySpawnTick", 0);
                player.SetData("FreeroamTime", 12);
                player.SetData("FreeroamWeather", 0);
                player.SetData("FreeroamWorld", (uint)0);
                player.SetData("FreeroamGodmode", true);
                player.SetData("FreeroamBlip", true);
                player.ResetData("ViewingLoadoutSelector");
                player.SetSharedData("HasShot", false);
                player.SetSharedData("ClimbProtection", false);
                player.SetData("ClimbProtectionEnd", Environment.TickCount64);
                player.SetData("LastClimbProtection", Environment.TickCount64);
                player.SetData("ClimbProtectionProcs", 0);
                player.SetSharedData("VoiceMode", 0);
                player.SetData("Killstreak", 0);
                player.SetSharedData("VisibleBlip", false);
                player.SetData("PMState", true);
                player.SetData("OOCState", true);
                player.SetData("Anonymous", false);
                player.SetSharedData("Team", -1);
                player.TriggerEvent("ToggleHud", false);
                player.ResetSharedData("Walkstyle");
                player.TriggerEvent("Bleeding:ClearAllDecals");
                player.SetData("GPS", false);
                player.SetSharedData("Packetloss", 65535.0); // Unknown/Unable to count as this is calculated in JS serverside since the function player.PacketLoss doesnt exist in C# RAGEMP

                player.ResetData("Language");

                player.SetSharedData("InWeaponEditor", false);

                player.ResetData("AdminsShotListening");

                player.SetData("EditingClothing", -1);
                player.SetData("Creator_Using", false);
                player.SetData("Tuner_Using", false);
                player.SetData("Tuner_Pos", 0);
                player.ResetData("Tuner_Vehicle");
                player.SetData("LastEditedClothingTeam", 1);

                player.SetSharedData("ActionType", "none");
                player.SetSharedData("ActionTime", 0);

                player.ResetData("AskedBySquad");
                player.ResetData("PlayerSquad");
                player.SetSharedData("PlayerSquadID", -1);

                player.SetSharedData("InLobby", -1);
                player.SetSharedData("InDM", -1);
                player.SetSharedData("DMBlipColor", 39);

                player.SetSharedData("IsTabbed", false);
                player.SetSharedData("FugitiveChance", 10.0);

                player.SetSharedData("Points", 0);

                player.SetSharedData("BustedProgress", 0.0);

                player.SetSharedData("Color_R", 255);
                player.SetSharedData("Color_G", 255);
                player.SetSharedData("Color_B", 255);

                player.SetSharedData("Ping", player.Ping);
                player.SetSharedData("Level", 1);
                player.SetSharedData("JailTime", 0);
                player.SetSharedData("MuteTime", 0);
                player.ResetData("DraggingOut");

                player.SetSharedData("TasedTill", Environment.TickCount64);

                player.SetData("TUA_LT", 0);
                player.SetData("TUA_LP", new Vector3(0.0, 0.0, 0.0)); // AFK CHECKERS

                player.SetSharedData("UnstreamedState", 0);
                player.SetSharedData("UnstreamedX", 0.0f);
                player.SetSharedData("UnstreamedY", 0.0f);
                player.SetSharedData("UnstreamedZ", 0.0f);

                List<Vehicle> temp_FreeroamVeh = new List<Vehicle>();
                player.SetData("FreeroamVehicles", temp_FreeroamVeh);

                lock (Settings.Lobbies)
                {
                    foreach (LobbyData il in Settings.Lobbies)
                    {
                        il.Cops.Remove(player);
                        il.Fugitives.Remove(player);
                    }
                }

                lock (Settings.LoggedInPlayers)
                {
                    Settings.LoggedInPlayers.Remove(player);
                    player.SetSharedData("pLogged", false);
                    player.SetData("LoggedIn", false);
                }
            }
        }

        public static T GetSharedDataSafe<T>(this Entity player, string key)
        {
            if (player.HasSharedData(key))
            {
                return player.GetSharedData<T>(key);
            }
            else
            {
                Type type = typeof(T);
                if (type == typeof(int) || type == typeof(long)) return (T)(object)-1;
                else if (type == typeof(float)) return (T)(object)-1f;
                else if (type == typeof(double)) return (T)(object)-1.0f;
                else if (type == typeof(float)) return (T)(object)-1f;
                else if (type == typeof(bool)) return (T)(object)false;
                else if (type == typeof(string)) return (T)(object)"";
                else if (type == typeof(List<T>)) return (T)(object)new List<T>();
                else if (type == typeof(Vector3)) return (T)(object)new Vector3(0, 0, 0);
                else
                {
                    if(typeof(Entity) == typeof(Player))
                    {
                        Player pl = (Player)player;
                        NAPI.Util.ConsoleOutput($"[GETSHAREDDATA SAFE FAIL] Key {key} on {pl.Name} was of Type: {type.Name}!");
                    }
                    return default(T);
                }
            }

        }

        public static bool IsAFK(Player player)
        {
            if (!EPlayer.LoggedIn(player)) return true;

            if (player.GetData<int>("TUA_LT") >= 4) // 4 times TimeUpdate has ticked and the player remained stationary / same spot and not chatted.
            {
                //return true;
            }

            return false;
        }

        public static void SendToKickedScreen(Player player)
        {
            player.Dimension = 6969;
            player.SetData("Kicked", true);
            player.Position = new Vector3(0, 0, 400);
            player.TriggerEvent("startKickedScreen");
            NAPI.Task.Run(() => { try { player.KickSilent(); } catch { } }, 60000);
        }

        public static void Jail(Player player, int minutes, string reason, int punishmentId = -1, string adminName = "")
        {
            if (!EPlayer.LoggedIn(player)) return;

            lock (player)
            {
                Account Acc = EPlayer.GetAccount(player);
                Acc.JailReason = reason;
                Acc.JailTime = Acc.JailTime + (minutes * 60);
                player.SendChatMessage(Translator.Translate(player, "JAILED_PLAYER_MSG", minutes, reason));
                EPlayer.ShowPunishmentScreen(player, punishmentId, "jail", adminName, reason, $"{minutes} minutes.", true);
                lock (Settings.Lobbies)
                {
                    foreach (LobbyData il in Settings.Lobbies)
                    {
                        lock (il)
                        {
                            lock (il.Cops) il.Cops.Remove(player);
                            lock (il.Fugitives) il.Fugitives.Remove(player);
                        }
                    }
                }
                player.SetSharedData("JailTime", Acc.JailTime);
                player.TriggerEvent("SetJailCounter", Acc.JailTime);
                NAPI.Player.SpawnPlayer(player, (Vector3)Settings.VectorJail[0], (float)Settings.VectorJail[1]);
            }
        }

        public static void ShowAdminNotes(Player player, Account acc, AdminNote.ANoteType type, bool send_header = true)
        {
            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    List<AdminNote> _notes = null;

                    switch (type)
                    {
                        case AdminNote.ANoteType.OnSpectate:
                            _notes = await context.AdminNotes.AsQueryable().Where(a => a.AccountID == acc.SQL_ID && a.Type == AdminNote.ANoteType.OnSpectate).ToListAsync();
                            break;
                        case AdminNote.ANoteType.OnConnect:
                            _notes = await context.AdminNotes.AsQueryable().Where(a => a.AccountID == acc.SQL_ID && a.Type == AdminNote.ANoteType.OnConnect).ToListAsync();
                            break;
                        case AdminNote.ANoteType.Both:
                            _notes = await context.AdminNotes.AsQueryable().Where(a => a.AccountID == acc.SQL_ID && a.Type == AdminNote.ANoteType.Both).ToListAsync();
                            break;
                        default:
                            _notes = await context.AdminNotes.AsQueryable().Where(a => a.AccountID == acc.SQL_ID).ToListAsync();
                            break;
                    }
                    if (_notes == null || _notes.Count <= 0)
                    {
                        if (type != AdminNote.ANoteType.OnConnect && type != AdminNote.ANoteType.OnSpectate && type != AdminNote.ANoteType.Both)
                        {
                            NAPI.Task.Run(() =>
                            {
                                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Target does not have any admin notes on them!");
                            });
                        }
                        return;
                    }
                    if (send_header) NAPI.Task.Run(() => { player.SendChatMessage($"!{{#ffc852}}====================> !{{#ffffff}}{acc.Username}'s Admin Notes !{{#ffc852}}<===================="); });
                    foreach (AdminNote note in _notes)
                    {
                        Account adm = await context.Accounts.AsQueryable().FirstOrDefaultAsync(a => a.SQL_ID == note.AdminID);
                        string aName = adm.Username ?? "[Unknown]";
                        NAPI.Task.Run(() =>
                        {
                            player.SendChatMessage($"!{{#528bff}}[!{{#91b6ff}}ID:{note.SQL_ID} | {note.Timestamp} | {note.TypeToString()}!{{#528bff}}] !{{#928aff}}{aName}: !{{#ffffff}} {note.Text}");
                        });
                    }
                }
            });
        }

        public static void ScreenNotify(Player player, string text, string type, string position = "center")
        {
            if (player == null) return;

            player.TriggerEvent("screenNotify", text, type, position);
        }

        public static void ShowStats(Player target, Account acc)
        {
            if (!EPlayer.LoggedIn(target))
                return;

            double KDR = 0.0;
            if (acc.Kills == 0) acc.Kills = 1;
            if (acc.Deaths == 0) acc.Deaths = 1;
            if (acc.MinutesPlayed == 0.0) acc.MinutesPlayed = 0.1;
            KDR = (double)acc.Kills / (double)acc.Deaths;

            target.SendChatMessage($"!{{#ffffff}}_____| !{{#d1f797}}Statistics for {acc.Username} ({acc.SQL_ID}) at {DateTime.Now.ToString()} !{{#ffffff}}|_____");

            target.SendChatMessage($"!{{#ffffff}}| !{{#c7eb7f}}Account ID: !{{#cbcfc4}}[{acc.SQL_ID}] !{{#ffffff}}| !{{#c7eb7f}}Email: !{{#cbcfc4}}[{acc.Email}] !{{#ffffff}}| !{{#c7eb7f}}Admin Level: !{{#cbcfc4}}[{acc.Admin}]");

            target.SendChatMessage($"!{{#ffffff}}| !{{#c7eb7f}}Register Date: !{{#cbcfc4}}[{acc.RegisterDate.ToString()}] !{{#ffffff}}| !{{#c7eb7f}}Last Login: !{{#cbcfc4}}[{acc.LastLogin.ToString()}] !{{#ffffff}}");

            target.SendChatMessage($"!{{#ffffff}}| !{{#c7eb7f}}Donator Level: !{{#cbcfc4}}[{acc.DonatorLevel}] !{{#ffffff}}| !{{#c7eb7f}}Donator Points: !{{#cbcfc4}}[{acc.DonatorPoints}] !{{#ffffff}}| !{{#c7eb7f}}Derby Wins: !{{#cbcfc4}}[{acc.DerbyWins}]");

            target.SendChatMessage($"!{{#ffffff}}| !{{#c7eb7f}}Level: !{{#cbcfc4}}[{acc.Level}] !{{#ffffff}}| !{{#c7eb7f}}Experience: !{{#cbcfc4}}[{acc.XP}/{EPlayer.GetMaxLevelXP(acc.Level)}] !{{#ffffff}}| !{{#c7eb7f}}Escapes: !{{#cbcfc4}}[{acc.Escapes}]");

            target.SendChatMessage($"!{{#ffffff}}| !{{#c7eb7f}}Kills: !{{#cbcfc4}}[{acc.Kills}] !{{#ffffff}}| !{{#c7eb7f}}Deaths: !{{#cbcfc4}}[{acc.Deaths}] !{{#ffffff}}| !{{#c7eb7f}}K/D Ratio: !{{#cbcfc4}}[{KDR.ToString("N3")}]");

            target.SendChatMessage($"!{{#ffffff}}| !{{#c7eb7f}}Arrests: !{{#cbcfc4}}[{acc.Arrests}] !{{#ffffff}}| !{{#c7eb7f}}Time Played: !{{#cbcfc4}}[{(acc.MinutesPlayed / 60.0).ToString("N3")} hours] !{{#ffffff}}| !{{#c7eb7f}}Warns: !{{#cbcfc4}}[{acc.Warns}]");

            target.SendChatMessage($"!{{#ffffff}}| !{{#c7eb7f}}Points: !{{#cbcfc4}}[{acc.Points}] !{{#ffffff}}| !{{#c7eb7f}}Vehicle (Fugitive): !{{#cbcfc4}}[{acc.CarFugitive}] !{{#ffffff}}| !{{#c7eb7f}}Vehicle (Cop): !{{#cbcfc4}}[{acc.CarCop}]");

            target.SendChatMessage($"!{{#ffffff}}| !{{#c7eb7f}}Last Jail: !{{#cbcfc4}}[{acc.JailReason}] !{{#ffffff}}| !{{#c7eb7f}}Last Warn: !{{#cbcfc4}}[{acc.MuteReason}] !{{#ffffff}}| !{{#c7eb7f}}Weapon Components: !{{#cbcfc4}}[/mycomps]");

            target.SendChatMessage($"!{{#ffffff}}_____| !{{#d1f797}}Statistics for {acc.Username} ({acc.SQL_ID}) at {DateTime.Now.ToString()} !{{#ffffff}}|_____");

            if (EPlayer.GetAccount(target).DiscordID.Length < 5)
            {
                if (EPlayer.GetAccount(target) == acc)
                {
                    target.SendChatMessage("!{#fa9194}You haven't linked a Discord account yet! !{#ffffff}Please do so in order to activate some cool perks. !{#ffff00}(/discord, /link)");
                }
                else
                {
                    target.SendChatMessage("!{#fa9194}No Discord account is linked to this account.");
                }
            }
            else
            {
                if (EPlayer.GetAccount(target) == acc)
                {
                    target.SendChatMessage("!{#83ff75}Your Discord account is linked.");
                }
                else
                {
                    target.SendChatMessage($"!{{#83ff75}}Discord account is linked! (UID:!{{#ffffff}}{EPlayer.GetAccount(target).DiscordID}!{{#83ff75}})");
                }
            }

            EPlayer.GrantXP(target, 0);
            return;
        }

        public static void ResetShotStats(Player player)
        {
            player.TriggerEvent("ResetShotStats");
            Events.OnSynchronizeShotStatistics(player, 0, 0, 0, 0);
        }

        public static void ShowShotStats(Player target, Player player)
        {
            if (!EPlayer.LoggedIn(player))
                return;

            int TotalFired = player.GetData<int>("ShotStats.TotalFired");
            int PlayerHits = player.GetData<int>("ShotStats.HitPlayer");
            int VehicleHits = player.GetData<int>("ShotStats.HitVehicle");
            int Missed = player.GetData<int>("ShotStats.Missed");
            long LastSync = player.GetData<long>("ShotStats.LastSync");

            target.SendChatMessage($"!{{#ffffff}}_____| !{{#d1f797}}Shot Statistics of {player.Name} at {DateTime.Now.ToString()} !{{#ffffff}}|_____");

            target.SendChatMessage($"!{{#ffffff}}| !{{#c7eb7f}}Total Shots Fired: !{{#cbcfc4}}[{TotalFired}] !{{#ffffff}}| !{{#c7eb7f}}Shots Hit Players: !{{#cbcfc4}}[{PlayerHits}] !{{#ffffff}}| !{{#c7eb7f}}Shot Hit Vehicles: !{{#cbcfc4}}[{VehicleHits}]");

            target.SendChatMessage($"!{{#ffffff}}| !{{#c7eb7f}}Missed/Shots Hit Nothing: !{{#cbcfc4}}[{Missed}] !{{#ffffff}}| !{{#c7eb7f}}Last Updated: !{{#cbcfc4}}[{(Environment.TickCount64 - LastSync) / 1000}s ago]");

            string playerAccuracy = "Unavailable";
            string vehicleAccuracy = "Unavailable";

            if (TotalFired > 0)
            {
                if (PlayerHits > 0)
                {
                    double calc = (PlayerHits / (double)(TotalFired - VehicleHits)) * 100.0;
                    playerAccuracy = $"{calc.ToString("N1")}%";
                }
                if (VehicleHits > 0)
                {
                    double calc = (VehicleHits / (double)(TotalFired - PlayerHits)) * 100.0;
                    vehicleAccuracy = $"{calc.ToString("N1")}%";
                }
            }

            target.SendChatMessage($"!{{#ffffff}}| !{{#c7eb7f}}Player Accuracy: !{{#cbcfc4}}[{playerAccuracy}] !{{#ffffff}}| !{{#c7eb7f}}Vehicle Accuracy: !{{#cbcfc4}}[{vehicleAccuracy}]");
            return;
        }

        public static void ShowPunishmentScreen(Player player, int recordId, string type, string adminName, string reason, string duration, bool acknowledgable)
        {
            player.TriggerEvent("showPunishmentScreen", recordId, type, adminName, reason, duration, acknowledgable);
        }

        public static void Warn(Player player, int recordId = -1, string adminName = "Anonymous", string reason = "")
        {
            if (!EPlayer.LoggedIn(player)) return;

            lock (player)
            {
                Account Acc = EPlayer.GetAccount(player);
                Acc.Warns++;

                if (Acc.Warns < 10)
                {
                    player.SendChatMessage(Translate(player, "WARNED_PLAYER_MSG"));
                    EPlayer.ShowPunishmentScreen(player, recordId, "warn", adminName, reason, null, true);
                }
                else
                {
                    if (Acc.Warns >= 20)
                    {
                        player.SendChatMessage(Translate(player, "WARNED_PLAYER_MSG_DANGER"));
                        Acc.Warns = 0;
                    }
                    else
                    {
                        decimal jTime = (decimal)Acc.Warns / (decimal)2;

                        int jailTime = (int)Decimal.Round(jTime);

                        EPlayer.Jail(player, jailTime, $"Exceeding {Acc.Warns} warns.");

                        RAGE.Entities.Players.All.ForEach((Player p) =>
                        {
                            p.SendChatMessage(Translate(p, "JAILED_MSG", "SERVER", player.Name, jailTime, $"{Acc.Warns} Warns"));
                        });
                    }
                }
            }
        }
        public static void SendCustomMessage(Player target, string channel, string channelValue, string message)
        {

            target.TriggerEvent("chat:sendCustomMessage", $"{{\"name\":\"{channel}\", \"value\":\"{channelValue}\"}}",
                                "[]",
                                "", message);
        }
        public static void SendPlayerMessage(Player target, Player messageFrom, string channel, string channelValue, string message)
        {
            target.TriggerEvent("chat:sendCustomMessage", $"{{\"name\":\"{channel}\", \"value\":\"{channelValue}\"}}",
                                messageFrom.GetSharedDataSafe<string>("BadgeData"), 
                                messageFrom.Name, message);
        }
        public static void SendPlayerMessage(Player target, string messageFromName, string channel, string channelValue, string message, string badges = "[]")
        {
            target.TriggerEvent("chat:sendCustomMessage", $"{{\"name\":\"{channel}\", \"value\":\"{channelValue}\"}}",
                                badges,
                                messageFromName, message);
        }

        public static string GetPrefix(Player player, bool color = true)
        {
            if (player.GetSharedDataSafe<bool>("FakeNameOn")) return "~w~Player";

            string toReturn = "";
            Account acc = EPlayer.GetAccount(player);
            if(acc.Admin >= 1 && player.GetData<bool>("Anonymous") == false)
            {
                toReturn = EPlayer.GetAdminLevelName(acc.Admin, color);
                return toReturn;
            }
            if(acc.DonatorLevel >= 1 && acc.DonatorStatus == true)
            {
                toReturn = EPlayer.GetDonatorLevelName(acc.DonatorLevel, color);
                return toReturn;
            }
            if(player.HasSharedData("ClanID") && player.GetSharedDataSafe<int>("ClanID") != -1 && player.HasSharedData("ClanTag") && player.GetSharedDataSafe<string>("ClanTag") != "")
            {
                if (color == true) toReturn += "!{#218a3d}";
                toReturn += player.GetSharedDataSafe<string>("ClanTag");
                return toReturn;
            }
            if (color == true) toReturn += "~w~";
            toReturn += "Player";
            return toReturn;
        }

        public static void TryAdvanceChallenge(Player player, Challenge.ChallengeType challenge_type, string challenge_item)
        {
            TryAdvanceChallenge(player, (int)challenge_type, challenge_item);
        }
        public static void TryAdvanceChallenge(Player player, int challenge_type, string challenge_item)
        {
            if (player == null) return;
            Account acc = EPlayer.GetAccount(player);
            if (acc == null) return;

            List<ChallengeData> cInfo = NAPI.Util.FromJson<List<ChallengeData>>(acc.ChallengeInfo);
            if (cInfo == null) return;

            ChallengeData cd = cInfo.FirstOrDefault(c => c.ChallengeType == challenge_type && c.ChallengeItem == challenge_item);
            if (cd == null) return;
            if (cd.Completed) return;
            cInfo.Remove(cd);
            cd.ChallengeProgress++;

            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    DailyChallenge dc = await context.DailyChallenges.AsQueryable().FirstOrDefaultAsync(dc => dc.Item == challenge_item && dc.Type == challenge_type);
                    if (dc == null) return;

                    NAPI.Task.Run(() =>
                    {
                        player.TriggerEvent("advanceChallenge", 1, challenge_type, challenge_item, cd.ChallengeProgress);
                    });
                    if (cd.ChallengeProgress >= dc.Target)
                    {
                        cd.Completed = true;
                        CompleteChallenge(player, dc);
                    }
                    cInfo.Add(cd);
                    string backToJson = NAPI.Util.ToJson(cInfo);
                    acc.ChallengeInfo = backToJson;
                    await context.SaveChangesAsync();
                }
            });

        }

        public static void CompleteChallenge(Player player, DailyChallenge challenge)
        {
            NAPI.Task.Run(() =>
            {
                if (challenge.RewardXP > 0)
                    EPlayer.GrantXP(player, challenge.RewardXP);
                if (challenge.RewardPoints > 0)
                    EPlayer.GivePoints(player, challenge.RewardPoints);

                Account acc = EPlayer.GetAccount(player);
                if (!acc.CompletedDailyChallenge)
                {
                    acc.ChallengeStreak++;
                    acc.CompletedDailyChallenge = true;

                    if (acc.ChallengeStreak < 7)
                    {
                        player.TriggerEvent("sendAchievementNotify", "Weekly Challenge", $"Daily Challenge Streak: {acc.ChallengeStreak}/7",
                            "package://Artwork/Achievements/first_bankrob_fugitive.png", "", 7500, false);
                        player.TriggerEvent("advanceChallenge", 0, -1, -1, acc.ChallengeStreak);
                    }
                    else if (acc.ChallengeStreak == 7)
                    {
                        EPlayer.GrantXP(player, 70);
                        EPlayer.GivePoints(player, 50);

                        player.SendChatMessage("!{#c387ff}[DAILY CHALLENGES] ~w~You successfully completed the weekly challenge! Received ~y~70 XP and 50 points~w~.");
                        player.TriggerEvent("sendAchievementNotify", "Weekly Challenge", $"Complete at least one daily challenge for 7 days in a row.",
                        "package://Artwork/Achievements/reach_level_100.png", "+70 XP & +50 points", 10000, true);

                        acc.ChallengeStreak = 7;
                    }
                    player.TriggerEvent("addChallengesToMenu", acc.ChallengeStreak, (Settings.Dynamics.NextWeekUnix - new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()));

                }

                player.TriggerEvent("unlockChallenge", challenge.Type, challenge.Item);
            });
        }

        public async static Task<List<UnlockedAchievement>> GetUnlockedAchievements(Account account)
        {
            if (account == null) return null;

            using(MainDatabase context = ContextFactory.CreateContext())
            {
                List<UnlockedAchievement> achievements = await context.UnlockedAchievements.AsQueryable().Where(a => a.AccountID == account.SQL_ID).ToListAsync();
                return achievements;
            }
        }

        public async static Task<UnlockedAchievement> HasUnlockedAchievement(Account account, string achievement)
        {
            if (account == null) return null;

            using (MainDatabase context = ContextFactory.CreateContext())
            {
                UnlockedAchievement a = await context.UnlockedAchievements.AsQueryable().FirstOrDefaultAsync(u => u.AccountID == account.SQL_ID && u.Achievement.Equals(achievement, StringComparison.OrdinalIgnoreCase));
                if (a == null) return null;
                else return a;
            }
        } 
        public static void UnlockAchievement(Player player, string achievement)
        {
            NAPI.Task.Run(() =>
            {
                if (EPlayer.GetAccount(player) == null) return;
                Account acc = EPlayer.GetAccount(player);
                Task.Run(async () =>
                {
                    if (await HasUnlockedAchievement(acc, achievement) != null) return;
                    Achievement achdata = Settings.Achievements.FirstOrDefault(a => a.Code == achievement);
                    if (achdata == null) return;

                    using (MainDatabase context = ContextFactory.CreateContext())
                    {
                        UnlockedAchievement ach = new UnlockedAchievement()
                        {
                            AccountID = acc.SQL_ID,
                            Achievement = achievement,
                            Time = DateTime.Now
                        };
                        await context.UnlockedAchievements.AddAsync(ach);

                        NAPI.Task.Run(() => {
                            if (player == null) return;
                            if(achdata == null) { achdata = Settings.Achievements.FirstOrDefault(a => a.Code == achievement); }                            
                            if(achdata != null)
                                player.TriggerEvent("unlockAchievement", achdata.Name);
                        });

                        if (achdata.RewardXP > 0)
                            NAPI.Task.Run(() => { EPlayer.GrantXP(player, achdata.RewardXP); });

                        if (achdata.RewardPoints > 0)
                            NAPI.Task.Run(() =>
                            {
                                EPlayer.GivePoints(player, achdata.RewardPoints);
                            });

                        await context.SaveChangesAsync();

                        // TO-DO: triggering achievement unlock notification, awaiting rotx to make UI for it

                        if (achievement != "all_achievements")
                        {
                            bool hasUnlockedAllAchievements = (await HasUnlockedAchievement(acc, "all_achievements") != null);
                            if (!hasUnlockedAllAchievements)
                            {
                                // checking if all achievements are unlocked: is the count of player's unlocked achievements higher or equal than server achievement count?
                                int playerAchievements = await context.UnlockedAchievements.AsQueryable().CountAsync(a => a.AccountID == acc.SQL_ID);
                                if (playerAchievements >= Settings.Achievements.Count - 1)
                                {
                                    UnlockAchievement(player, "all_achievements");
                                }
                            }
                        }
                    }
                });
                
            });
            
        }

        public static void SyncComponents(Player tosyncfor, Player target, long specificWeapon = -1)
        {
            try
            {
                if (target.HasData("AttachedComponents"))
                {
                    List<KeyValuePair<long, long>> comps = target.GetData<List<KeyValuePair<long, long>>>("AttachedComponents");

                    foreach (var item in comps)
                    {
                        if (specificWeapon == -1 || (specificWeapon != -1 &&
                            item.Key == specificWeapon)) tosyncfor.TriggerEvent("AttachWeaponComponent", target, (int)item.Key, (int)item.Value);
                    }
                }
            }
            catch(Exception ex) 
            {
                NAPI.Util.ConsoleOutput("[EPlayer.SyncComponents null shit] " + ex.ToString());
            }
            return;
        }
        public static void AttachComponentPrivate(Player target, long weaponhash, long componenthash)
        {
            List<KeyValuePair<long, long>> comps = new List<KeyValuePair<long, long>>();
            if (target.HasData("AttachedComponents"))
            {
                comps = target.GetData<List<KeyValuePair<long, long>>>("AttachedComponents");
            }

            comps.Add(new KeyValuePair<long, long>(weaponhash, componenthash));
            target.SetData("AttachedComponents", comps);

            target.SetSharedData("ComponentSyncTrigger", new Random(DateTimeOffset.UtcNow.Millisecond).Next());
            SyncComponents(target, target, weaponhash);
        }
        public static void AttachComponent(Player target, long weaponhash, long componenthash)
        {
            List<KeyValuePair<long, long>> comps = new List<KeyValuePair<long, long>>();
            if (target.HasData("AttachedComponents"))
            {
                comps = target.GetData<List<KeyValuePair<long, long>>>("AttachedComponents");
            }

            comps.Add(new KeyValuePair<long, long>(weaponhash, componenthash));
            target.SetData("AttachedComponents", comps);

            target.SetSharedData("ComponentSyncTrigger", new Random(DateTimeOffset.UtcNow.Millisecond).Next());
            foreach (Player p in Settings.LoggedInPlayers)
            {
                if (p.Dimension == target.Dimension)
                {
                    SyncComponents(p, target);
                }
            }

            return;
        }

        public static void GiveCopWeapons(Player player)
        {
            if (!EPlayer.LoggedIn(player)) return;

            LobbyData lobby = Settings.Lobbies.Find(a => a.lobbyId == player.GetSharedDataSafe<int>("InLobby"));

            if (lobby == null) { return; }
            if (lobby.gameMode == GameModes.BANKROBBERY)
            {
                player.GiveWeaponAC(WeaponHash.Stungun, 1);
                if (player.HasData("BankRobberyUnit"))
                {
                    if(player.GetData<string>("BankRobberyUnit") == "fib")
                    {
                        player.Health = 100;
                        player.Armor = 100;

                        player.SetSharedData("Handgun_Hash", (uint)WeaponHash.Heavypistol);
                        player.GiveWeaponAC(WeaponHash.Heavypistol, 100);
                        player.GiveWeaponAC(WeaponHash.Assaultsmg, 200);
                        player.GiveWeaponAC(WeaponHash.Specialcarbine, 500);
                        player.GiveWeaponAC(WeaponHash.Bzgas, 3);
                        player.GiveWeaponAC(WeaponHash.Assaultshotgun, 500);
                    }
                    else
                    {
                        player.Health = 100;
                        player.Armor = 75;

                        player.SetSharedData("Handgun_Hash", (uint)WeaponHash.Combatpistol);
                        player.GiveWeaponAC(WeaponHash.Combatpistol, 100);
                        player.GiveWeaponAC(WeaponHash.Smg, 300);
                        player.GiveWeaponAC(WeaponHash.Carbinerifle, 500);
                        player.GiveWeaponAC(WeaponHash.Bzgas, 1);
                        player.GiveWeaponAC(WeaponHash.Pumpshotgun, 72);
                    }
                }
                return;
            }
            if (lobby.gameMode != GameModes.BUSTED)
            {
                player.GiveWeaponAC(WeaponHash.Stungun, 1);
                if (EPlayer.GetAccount(player).LoadoutCop.Length >= 2)
                {
                    LoadoutData loadout = Settings.Loadouts.Find(a => a.Team == 1 && a.Name.Equals(EPlayer.GetAccount(player).LoadoutCop));

                    if (loadout == null)
                    {
                        EPlayer.GetAccount(player).LoadoutCop = "?";
                        GiveCopWeapons(player);
                        return;
                    }

                    if (EPlayer.GetAccount(player).Level < loadout.LevelRequired)
                    {
                        EPlayer.GetAccount(player).LoadoutCop = "?";
                        GiveCopWeapons(player);
                        return;
                    }

                    if (loadout.Weapons.Count() > 0)
                    {
                        Account _a = EPlayer.GetAccount(player);
                        Task.Run(async () =>
                        {
                            using (MainDatabase context = ContextFactory.CreateContext())
                            {
                                OwnedLoadoutData _ld = await context.OwnedLoadouts.AsQueryable().FirstOrDefaultAsync(l =>
                                                   l.AccountSQLID == _a.SQL_ID &&
                                                   l.Name.Equals(_a.LoadoutCop, StringComparison.OrdinalIgnoreCase));
                                if (_ld != null)
                                    _ld.TimesUsed++;

                                NAPI.Task.Run(() =>
                                {
                                    loadout.Weapons.ForEach((wepar) =>
                                    {
                                        int extraAmmo = 0;
                                        jWeaponData wepdat;
                                        if (Settings.WeaponData.TryGetValue(Convert.ToInt64((WeaponHash)wepar[0]), out wepdat))
                                        {
                                            if (wepdat.Group.Equals("GROUP_PISTOL"))
                                            {
                                                player.SetSharedData("Handgun_Hash", wepar[0]);
                                            }

                                            if (EPlayer.GetAccount(player).PurchasedAmmo.Length >= 4)
                                            {
                                                var data = NAPI.Util.FromJson<Dictionary<string, int>>(EPlayer.GetAccount(player).PurchasedAmmo);
                                                if (wepdat.AmmoType != null && wepdat.AmmoType.Length >= 2)
                                                {
                                                    if (!data.TryGetValue(wepdat.AmmoType, out extraAmmo)) extraAmmo = 0;
                                                }
                                            }
                                        }

                                        if ((WeaponHash)wepar[0] == (WeaponHash)1)
                                        {
                                            player.Armor = (int)wepar[3];
                                        }
                                        else player.GiveWeaponAC((WeaponHash)wepar[0], (int)wepar[3] + extraAmmo);
                                    });
                                });
                            }
                        });
                        
                    }
                }
                else
                {
                    player.GiveWeaponAC(WeaponHash.Pumpshotgun, 20);
                    player.GiveWeaponAC(WeaponHash.Pistol, 40);
                }

                NAPI.Task.Run(() =>
                {
                    try
                    {
                        if (player != null && !player.IsNull && EPlayer.GetAccount(player).WeaponComponents != null && EPlayer.GetAccount(player).WeaponComponents.Length > 3)
                        {
                            Dictionary<long, wComponentData> weaponCompsData = new Dictionary<long, wComponentData>();
                            weaponCompsData = NAPI.Util.FromJson<Dictionary<long, wComponentData>>(EPlayer.GetAccount(player).WeaponComponents);

                            foreach (KeyValuePair<long, wComponentData> i in weaponCompsData)
                            {
                                if (player.GetWeaponAmmo((WeaponHash)i.Key) > 0)
                                {
                                    i.Value.Components.ForEach((long comphash) =>
                                    {
                                        EPlayer.AttachComponent(player, i.Key, comphash);
                                    });
                                }
                            }
                        }
                    }
                    catch { }
                }, 500 + player.Ping);
            }

            //player.GiveWeaponAC(WeaponHash.Nightstick, 1);
            player.SendChatMessage(Translate(player, "STUNGUN_MSG"));
            return;
        }
        public static void GiveFugitiveWeapons(Player player)
        {
            if (!EPlayer.LoggedIn(player)) return;

            LobbyData lobby = Settings.Lobbies.Find(a => a.lobbyId == player.GetSharedDataSafe<int>("InLobby"));

            if (lobby == null) { return; }


            if (lobby.gameMode != GameModes.BUSTED)
            {
                if (EPlayer.GetAccount(player).LoadoutFugitive.Length >= 2)
                {
                    LoadoutData loadout = Settings.Loadouts.Find(a => a.Team == 0 && a.Name.Equals(EPlayer.GetAccount(player).LoadoutFugitive));

                    if ((loadout != null) == false)
                    {
                        EPlayer.GetAccount(player).LoadoutFugitive = "?";
                        GiveFugitiveWeapons(player);
                        return;
                    }

                    if (EPlayer.GetAccount(player).Level < loadout.LevelRequired)
                    {
                        EPlayer.GetAccount(player).LoadoutFugitive = "?";
                        GiveFugitiveWeapons(player);
                        return;
                    }
                    
                    if (loadout.Weapons.Count() > 0)
                    {
                        Account _a = EPlayer.GetAccount(player);
                        Task.Run(async () =>
                        {
                            using (MainDatabase context = ContextFactory.CreateContext())
                            {
                               OwnedLoadoutData _ld = await context.OwnedLoadouts.AsQueryable().FirstOrDefaultAsync(l =>
                               l.AccountSQLID == _a.SQL_ID &&
                               l.Name.Equals(_a.LoadoutFugitive, StringComparison.OrdinalIgnoreCase));
                                if (_ld != null)
                                    _ld.TimesUsed++;

                                NAPI.Task.Run(() =>
                                {
                                    loadout.Weapons.ForEach((wepar) =>
                                    {
                                        int extraAmmo = 0;
                                        jWeaponData wepdat;
                                        if (Settings.WeaponData.TryGetValue(Convert.ToInt64((WeaponHash)wepar[0]), out wepdat))
                                        {
                                            if (wepdat.Group.Equals("GROUP_PISTOL"))
                                            {
                                                player.SetSharedData("Handgun_Hash", wepar[0]);
                                            }

                                            if (EPlayer.GetAccount(player).PurchasedAmmo.Length >= 4)
                                            {
                                                var data = NAPI.Util.FromJson<Dictionary<string, int>>(EPlayer.GetAccount(player).PurchasedAmmo);
                                                if (wepdat.AmmoType != null && wepdat.AmmoType.Length >= 2)
                                                {
                                                    if (!data.TryGetValue(wepdat.AmmoType, out extraAmmo)) extraAmmo = 0;
                                                }
                                            }
                                        }

                                        if ((WeaponHash)wepar[0] == (WeaponHash)1)
                                        {
                                            player.Armor = (int)wepar[3];
                                        }
                                        else player.GiveWeaponAC((WeaponHash)wepar[0], (int)wepar[3] + extraAmmo);
                                    });
                                });
                            }
                        });
                        
                    }
                }
                else
                {
                    player.GiveWeaponAC(WeaponHash.Pistol, 40);
                    player.GiveWeaponAC(WeaponHash.Pumpshotgun, 20);
                }

                NAPI.Task.Run(() =>
                {
                    try
                    {
                        if (player != null && !player.IsNull && EPlayer.GetAccount(player).WeaponComponents != null && EPlayer.GetAccount(player).WeaponComponents.Length > 3)
                        {
                            Dictionary<long, wComponentData> weaponCompsData = new Dictionary<long, wComponentData>();
                            weaponCompsData = NAPI.Util.FromJson<Dictionary<long, wComponentData>>(EPlayer.GetAccount(player).WeaponComponents);

                            foreach (KeyValuePair<long, wComponentData> i in weaponCompsData)
                            {
                                if (player.GetWeaponAmmo((WeaponHash)i.Key) > 0)
                                {
                                    i.Value.Components.ForEach((long comphash) =>
                                    {
                                        EPlayer.AttachComponent(player, i.Key, comphash);
                                    });
                                }
                            }
                        }
                    }
                    catch { }
                }, 500 + player.Ping);
            }

            //WeaponHash[] randomWeps = { WeaponHash.Bottle, WeaponHash.Knife, WeaponHash.Bat, WeaponHash.Battleaxe, WeaponHash.Crowbar };
            
            //player.GiveWeaponAC(randomWeps.OrderBy(a => Guid.NewGuid()).First(), 1);
            return;
        }

        public static void CreateAmmoBox(Player player)
        {
            if (!EPlayer.LoggedIn(player)) return;

            LobbyData l = Settings.Lobbies.FirstOrDefault(lb => lb.Cops.Contains(player) || lb.Fugitives.Contains(player));
            if(l != null)
            {
                if(l.gameMode == GameModes.BANKROBBERY)
                {
                    return; // no ammo boxes in bank robbery no no big bug
                }
            }

            List<WeaponHash> toNotPut = new List<WeaponHash>();
            toNotPut.Add(WeaponHash.Bzgas);
            toNotPut.Add(WeaponHash.Molotov);

            Dictionary<string, int> ammoData = new Dictionary<string, int>();
            foreach (KeyValuePair<long, jWeaponData> i in Settings.WeaponData)
            {
                if (!toNotPut.Contains((WeaponHash)i.Key))
                {
                    if (player.GetWeaponAmmo((WeaponHash)i.Key) > 0)
                    {
                        if (i.Value.AmmoType != null)
                        {
                            if (!ammoData.ContainsKey(i.Value.AmmoType)) // IF ammo has not been added before in the box already (If 2 same category guns are there, 2 rifles. They will dupe)
                            {
                                ammoData.Add(i.Value.AmmoType, player.GetWeaponAmmo((WeaponHash)i.Key));
                                player.SetWeaponAmmo((WeaponHash)i.Key, 0);
                            }
                        }
                    }
                }
            }
            if (ammoData.Count == 0) return; // No weapons.

            player.SetData("CreatingAmmoBox", ammoData);
            player.TriggerEvent("GetGroundZ", player.Position.X, player.Position.Y, player.Position.Z, player.Dimension, "ReturnAmmoPosZ");
        }

        public static void SendMessageToAllEx(string text)
        {
            foreach (Player pl in NAPI.Pools.GetAllPlayers())
            {
                pl.SendChatMessage(text);
            }
        }

        public static void SendMessage(Player player, string message)
        {
            player.SendChatMessage(message);
        }
        public static void SendMessageToAll(string message_code, params object[] args) { SendMessageAll(message_code, args); }
        public static void SendMessageAll(string message_code, params object[] args)
        {
            foreach(Player pl in NAPI.Pools.GetAllPlayers())
            {
                pl.SendChatMessage(Translator.Translate(pl, message_code, args));
            }
        }

        // This is for very specific cases. Make sure the message args go first, and the ENABLED/DISABLED part is LAST!!!
        // e.g. "Player has set copchase to ENABLED/DISABLED."
        public static void SendToggledMessage(string message_code, bool toggle, params object[] args)
        {
            foreach (Player pl in NAPI.Pools.GetAllPlayers())
            {
                string togmsg = (toggle ? Translator.Translate(pl, "ENABLED") : Translator.Translate(pl, "DISABLED"));

                pl.SendChatMessage(Translator.Translate(pl, message_code, args, togmsg));
            }
        }

        public static void SetDiscordStatus(Player player, string status)
        {
            player.TriggerEvent("SetDiscordStatus", "Race 'N' Chase", status);
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
                        player.GiveWeaponAC(wepTuple.Item1, wepTuple.Item2);
                    });

                    player.ResetData("FreezeWeapons");
                }
            }
        }

        public static string GetAdminName(Player player)
        {
            return ((EPlayer.GetAccount(player).Admin >= 2 && player.GetData<bool>("Anonymous") == true) ? "ANONYMOUS" : EPlayer.Name(player));
        }

        public static void Action(Player player, string message_code, params object[] args)
        {           
            foreach (Player playa in NAPI.Player.GetPlayersInRadiusOfPlayer(40, player))
            {
                var msg = "!{#C3A3DB}* " + EPlayer.Name(player) + " " + Translator.Translate(playa, message_code, args);

                if (player.Dimension == playa.Dimension) playa.SendChatMessage(msg);
            }
        }
        public static void ActionEx(Player player, string message)
        {
            var msg = "!{#C3A3DB}* " + EPlayer.Name(player) + " " + message;
            foreach (Player playa in NAPI.Player.GetPlayersInRadiusOfPlayer(40, player))
            {
                if (player.Dimension == playa.Dimension) playa.SendChatMessage(msg);
            }
        }

        public static void Handcuff(Player player, bool toggle)
        {
            player.TriggerEvent("SetHandcuffs", toggle);
            player.SetData("Cuffed", true);
            if(!toggle) player.ResetData("Cuffed");
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

        public static Color GetColor(Player player)
        {
            return new Color(
                player.GetSharedDataSafe<int>("Color_R"), 
                player.GetSharedDataSafe<int>("Color_G"),
                player.GetSharedDataSafe<int>("Color_B"));
        }

        public static String HexConverter(Color c)
        {
            return "#" + c.Red.ToString("X2") + c.Green.ToString("X2") + c.Blue.ToString("X2");
        }

        public async static Task<List<Account>> GetLinkedAccounts(Account pAcc) // Gets all linked accounts to a specific account, will always contain the input account.
        {
            using (MainDatabase context = ContextFactory.CreateContext())
            {
                var accounts = await context.Accounts.AsAsyncEnumerable().Where(acc =>
                    new ulong[] { pAcc.RegisterRID, pAcc.LastRID }.Intersect(new ulong[] { acc.LastRID, acc.RegisterRID }).Any() ||
                    new string[] { pAcc.RegisterHWID, pAcc.LastHWID }.Intersect(new string[] { acc.RegisterHWID, acc.LastHWID }).Any() ||
                    (acc.Email != null && acc.Email.Equals(pAcc.Email) && acc.Email.Length >= 5)).ToListAsync(); // BONKERS

                return accounts;
            }
        }

        public static bool LoggedIn(Player player)
        {
            if (player == null) return false;

            if (player.HasData("Kicked")) return false;

            if (player.HasData("AccountHandle") && player.GetData<Account>("AccountHandle") != null && player.HasData("LoggedIn") && player.GetData<bool>("LoggedIn") == true)
            {
                return true;
            }
            return false;
        }
        public static void ResetAllPlayerClothing(Player player)
        {
            // TO-DO: reset clothing function
        }

        public static void LoginCameraSetup(Player player)
        {
            player.TriggerEvent("TogglePlayerChat", false);
            EPlayer.Freeze(player, true);
            player.Position = new Vector3(Settings.LoginCameraX, Settings.LoginCameraY, Settings.LoginCameraZ+20f);
            player.Dimension = 666 + Convert.ToUInt32(RAGE.Entities.Players.Count);
            player.SetSharedData("EntAlpha", 0);
 
            player.TriggerEvent("DestroyPlayerCamera");
            player.TriggerEvent("CreatePlayerCamera", Settings.LoginCameraX, Settings.LoginCameraY, Settings.LoginCameraZ, Settings.LoginCameraRx, Settings.LoginCameraRy, Settings.LoginCameraRz, Settings.LoginCameraViewAngle);
            //NAPI.Task.Run(() =>
            //{
            //    if (player != null)
            //        player.TriggerEvent("MovePlayerCamera", Settings.LoginCameraXEnd, Settings.LoginCameraYEnd, Settings.LoginCameraZEnd, Settings.LoginCameraRxEnd, Settings.LoginCameraRyEnd, Settings.LoginCameraRzEnd, 15000, Settings.LoginCameraViewAngle); 
            //}, 1000);

            EPlayer.LoginBrowserSetup(player);
        }

        public static void StopPlayerAction(Player player, int reason)
        {
            player.TriggerEvent("StopPlayerActions");
            Events.OnStopPlayerAction(player, reason);
        }

        public static void SendAutoLoginData(Player player)
        {
            string scname = player.SocialClubName;
            ulong scid = player.SocialClubId;
            string pserial = player.Serial;
            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    List<Account> autoLoginChecks = await context.Accounts.AsQueryable().Where(a => a.LastHWID == pserial &&
                                                                              a.LastRID == scid &&
                                                                              a.LastSC == scname &&
                                                                              a.AutoLogin == true).ToListAsync();

                    if (autoLoginChecks.Count <= 0)
                    {
                        NAPI.Task.Run(() =>
                        {
                            player.TriggerEvent("sendAutoLoginDataToBrowser", "");
                            NAPI.Util.ConsoleOutput("Found 0 auto login data");
                        });
                    }
                    else
                    {
                        NAPI.Task.Run(() =>
                        {
                            autoLoginChecks.Sort((x, y) => y.LastLogin.CompareTo(x.LastLogin)); // sort by date, descending
                            Account toLogin = autoLoginChecks[0];
                            NAPI.Util.ConsoleOutput("Sending data: " + toLogin.Username);

                            player.TriggerEvent("sendAutoLoginDataToBrowser", toLogin.Username);
                        });
                    }
                }
            });
            
        }
        public static void SendForgotUsernameData(Player player)
        {
            string scname = player.SocialClubName;
            ulong scid = player.SocialClubId;
            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    List<Account> accounts = await context.Accounts.AsQueryable().Where(a => a.LastSC == scname || a.LastRID == scid
                                                                || a.RegisterRID == scid || a.RegisterSC == scname).ToListAsync();

                    NAPI.Task.Run(() =>
                    {
                        string dataToReturn = "[";
                        foreach (Account acc in accounts)
                        {
                            if (acc != accounts[accounts.Count - 1])
                                dataToReturn += $"\"{acc.Username}\",";
                            else dataToReturn += $"\"{acc.Username}\"";
                        }
                        dataToReturn += "]";
                        player.TriggerEvent("sendForgotUsernameToBrowser", dataToReturn);
                    });
                }
            });
            
        }

        public static void LoginBrowserSetup(Player player, bool show_login = true)
        {
            NAPI.Task.Run(() =>
            {
                if (show_login)
                {
                    player.TriggerEvent("showLoginBrowser", true);
                }

                player.TriggerEvent("InitiateCustomChat");
                player.TriggerEvent("TogglePlayerChat", false);
            }, 500);

            SetDiscordStatus(player, "On login screen");
            ResetPlayerData(player);
        }
        public static void GivePoints(Player player, int points)
        {
            NAPI.Task.Run(() =>
            {
                if (player == null) return;

                if (points > 0)
                    EPlayer.ScreenNotify(player, $"+{points} Points", "success", "bottom-center");
                else EPlayer.ScreenNotify(player, $"{points} Points", "error", "bottom-center");
                EPlayer.GetAccount(player).Points += points;
                player.SetSharedData("Points", EPlayer.GetAccount(player).Points);
                EPlayer.Save(player);
            });

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
        public async static Task<List<string>> GetOwnedVehicles(Account acc, int team)
        {
            List<string> ownedCars = new List<string>();
            using (MainDatabase context = ContextFactory.CreateContext())
            {
                var cars = await context.OwnedVehicles.AsQueryable().Where(a => a.AccountSQLID == acc.SQL_ID && a.Team == team).ToListAsync();
                foreach (var item in cars) ownedCars.Add(item.Model);
                return ownedCars;
            }
        }
        public async static Task<List<CarData>> GetOwnedVehicles(Account acc)
        {
            List<CarData> ownedCars = new List<CarData>();

            List<CarData> defaults = Settings.Cars.Where(l => l.Name == "Random").ToList();
            ownedCars.AddRange(defaults);

            using (MainDatabase context = ContextFactory.CreateContext())
            {
                List<OwnedVehData> ownedDBcars = await context.OwnedVehicles.AsQueryable().Where(a => a.AccountSQLID == acc.SQL_ID).ToListAsync();

                foreach(OwnedVehData ownedDBcar in ownedDBcars)
                {
                    CarData car = Settings.Cars.FirstOrDefault(c => c.Model == ownedDBcar.Model && c.Team == ownedDBcar.Team);
                    if(car != null)
                    {
                        ownedCars.Add(car);
                    }
                }
                return ownedCars;
            }
        }
        public async static Task<bool> UnlockVehicle(Account acc, Player player, string model, int team)
        {
            if (!LoggedIn(player)) return false;

            using (MainDatabase context = ContextFactory.CreateContext())
            {
                if (await HasOwnedVehicle(acc, model, team)) return false;

                var _new = new OwnedVehData
                {
                    AccountSQLID = acc.SQL_ID,
                    Model = model,
                    Team = team,
                    TimesUsed = 0,
                };
                await context.OwnedVehicles.AddAsync(_new);

                EPlayer.UnlockAchievement(player, "first_vehicle");
                await context.SaveChangesAsync();

                List<string> ccars = await GetOwnedVehicles(acc, 1);
                List<string> fcars = await GetOwnedVehicles(acc, 0);

                int totalOwned = ccars.Count + fcars.Count + 2;

                NAPI.Task.Run(() =>
                {
                    NAPI.Util.ConsoleOutput($"{player.Name} unlocked a {model}, they now have a total of {totalOwned}/{Settings.Cars.Count} vehicles");

                    if (totalOwned >= Settings.Cars.Count)
                        EPlayer.UnlockAchievement(player, "all_vehicles");
                });


                return true;
            }
        }
        public async static Task<bool> HasOwnedVehicle(Account acc, string model, int team)
        {
            if (model == "?" || model.Equals("default", StringComparison.OrdinalIgnoreCase)) return true;

            using (MainDatabase context = ContextFactory.CreateContext())
            {
                if (await context.OwnedVehicles.AsQueryable().CountAsync(a => a.AccountSQLID == acc.SQL_ID && a.Team == team && a.Model.Equals(model, StringComparison.OrdinalIgnoreCase)) > 0) return true;
                return false;
            }
        }

        public async static Task<List<LoadoutData>> GetOwnedLoadouts(Account acc)
        {
            List<LoadoutData> ownedLoadouts = new List<LoadoutData>();

            List<LoadoutData> defaults = Settings.Loadouts.Where(l => l.Name == "Default").ToList();
            ownedLoadouts.AddRange(defaults);

            using (MainDatabase context = ContextFactory.CreateContext())
            {
                List<OwnedLoadoutData> ownedDBloadouts = await context.OwnedLoadouts.AsQueryable().Where(l => l.AccountSQLID == acc.SQL_ID).ToListAsync();
                foreach(OwnedLoadoutData ownedDBloadout in ownedDBloadouts)
                {
                    LoadoutData ld = Settings.Loadouts.FirstOrDefault(l => l.Name.ToLower() == ownedDBloadout.Name.ToLower() && l.Team == ownedDBloadout.Team);
                    if (ld != null) ownedLoadouts.Add(ld);
                    else NAPI.Util.ConsoleOutput($"[OWNED LOADOUTS] Can't find loadout data for name {ownedDBloadout.Name}!");
                }

                return ownedLoadouts;
            }
        }

        public static void UpdateMenuPrivateLobby(Player player, LobbyData lobby)
        {
            if(lobby == null)
            {
                player.TriggerEvent("removePrivateLobby");
                return;
            }

            List<Player> lobbyPlayers = Settings.LoggedInPlayers.Where(a => a.GetSharedDataSafe<int>("InLobby") == lobby.lobbyId).ToList();
            int players = lobbyPlayers.Count;

            if (lobby.PlayerCreated == true && lobby.lobbyId == EPlayer.GetAccount(player).SQL_ID + 50000)
            {
                string playerListJSON = "[";
                foreach (Player pl in lobbyPlayers)
                {
                    playerListJSON += $"{{id:{pl.Id}, name:\"{pl.Name}\", ping:{pl.Ping}}},";
                }
                playerListJSON += "]";

                string bplayerListJSON = "[";
                foreach (Player pl in lobby.BannedPlayers)
                {
                    bplayerListJSON += $"{{id:{pl.Id}, name:\"{pl.Name}\", ping:{pl.Ping}}},";
                }
                bplayerListJSON += "]";

                player.TriggerEvent("updateMenuPrivateLobby", lobby.lobbyId, lobby.Name, lobby.Password, (int)lobby.gameMode, players, lobby.MaxPlayers, lobby.MaxPing, player.Name, playerListJSON, bplayerListJSON);
            }
        }
        
        public async static Task<List<string>> GetOwnedLoadouts(Account acc, int team)
        {
            List<string> ownedLoadouts = new List<string>();

            using (MainDatabase context = ContextFactory.CreateContext())
            {
                var loadouts = await context.OwnedLoadouts.AsQueryable().Where(a => a.AccountSQLID == acc.SQL_ID && a.Team == team).ToListAsync();
                foreach (var item in loadouts) ownedLoadouts.Add(item.Name);

                return ownedLoadouts;
            }     
        }
        public async static Task<bool> UnlockLoadout(Account acc, Player player, string name, int team)
        {
            if (!LoggedIn(player)) return false;

            if (await HasOwnedLoadout(acc, name, team)) return false;
            using (MainDatabase context = ContextFactory.CreateContext())
            {
                var _new = new OwnedLoadoutData
                {
                    AccountSQLID = acc.SQL_ID,
                    Name = name,
                    Team = team,
                    TimesUsed = 0
                };
                await context.OwnedLoadouts.AddAsync(_new);

                EPlayer.UnlockAchievement(player, "first_loadout");
                await context.SaveChangesAsync();

                List<string> cgun = await GetOwnedLoadouts(acc, 1);
                List<string> fgun = await GetOwnedLoadouts(acc, 0);
                int totalOwned = cgun.Count + fgun.Count + 2;

                NAPI.Task.Run(() =>
                {
                    NAPI.Util.ConsoleOutput($"{player.Name} unlocked loadout {name}, they now have a total of {totalOwned}/{Settings.Loadouts.Count} loadouts.");

                    if (totalOwned >= Settings.Loadouts.Count)
                        EPlayer.UnlockAchievement(player, "all_loadouts");
                });
                return true;
            }
        }
        public async static Task<bool> HasOwnedLoadout(Account acc, string name, int team)
        {
            if (name == "?" || name.Equals("default", StringComparison.OrdinalIgnoreCase)) return true;

            using (MainDatabase context = ContextFactory.CreateContext())
            {
                if (await context.OwnedLoadouts.AsQueryable().CountAsync(a => a.AccountSQLID == acc.SQL_ID && a.Team == team && a.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) > 0) return true;
                return false;
            }

        }
        public static void GrantXP(Player player, int xp)
        {
            NAPI.Task.Run(() =>
            {
                if (!EPlayer.LoggedIn(player)) return;

                if(xp > 0)
                    EPlayer.ScreenNotify(player, $"+{xp} XP", "success", "bottom-center");
                else EPlayer.ScreenNotify(player, $"{xp} XP", "error", "bottom-center");

                Account acc = EPlayer.GetAccount(player);
                acc.XP += xp;
                int lvlDiff = 0;

                while (acc.XP < 0)
                {
                    if (acc.Level > 0)
                    {
                        acc.Level--;

                        acc.XP += GetMaxLevelXP(acc.Level);
                        lvlDiff--;
                    }
                    else
                    {
                        acc.XP = 0;
                        acc.Level = 0;
                        break;
                    }
                }

                while (CheckXPLevelUp(acc.Level, acc.XP) == true)
                {
                    acc.XP -= GetMaxLevelXP(acc.Level);
                    acc.Level++;

                    lvlDiff++;
                }

                if (lvlDiff > 0)
                {
                    RAGE.Entities.Players.All.ForEach((Player pl) =>
                    {
                        if (lvlDiff > 1) pl.SendChatMessage(Translate(pl, "LEVEL_UP_MULTIPLE", EPlayer.Name(player), lvlDiff, acc.Level));
                        else pl.SendChatMessage(Translate(pl, "LEVEL_UP_ONE", EPlayer.Name(player), acc.Level));
                    });
                    EPlayer.Notify(player, Translate(player, "LEVEL_UP_MSG", acc.Level), "green", "fas fa-check", false, 1500, "bottomRight", "replace");

                    player.TriggerEvent("PlayMissionComplete", "FRANKLIN_BIG_01");
                    try { NAPI.Task.Run(() => player.TriggerEvent("ShakeCam", "VIBRATE_SHAKE", 7.0f, 2000), 1000); } catch { }
                }
                else if (lvlDiff < 0)
                {
                    RAGE.Entities.Players.All.ForEach((Player pl) =>
                    {
                        if (lvlDiff > 1) pl.SendChatMessage(Translate(pl, "LEVEL_DOWN_MULTIPLE", EPlayer.Name(player), lvlDiff, acc.Level));
                        else pl.SendChatMessage(Translate(pl, "LEVEL_DOWN_ONE", EPlayer.Name(player), acc.Level));
                    });
                    EPlayer.Notify(player, Translate(player, "LEVEL_DOWN_MSG", acc.Level), "red", "fas fa-check", false, 1500, "bottomRight", "replace");

                    player.TriggerEvent("PlayMissionComplete", "GENERIC_FAILED");
                    try { NAPI.Task.Run(() => player.TriggerEvent("ShakeCam", "VIBRATE_SHAKE", 3.0f, 1500), 1000); } catch { }
                }

                player.SetSharedData("Level", acc.Level);
                player.SetSharedData("CurrentXP", acc.XP);
                player.SetSharedData("NextLevelXP", EPlayer.GetMaxLevelXP(acc.Level));
                player.TriggerEvent("Menu_UpdateLevelXP");

                try
                {
                    NAPI.Task.Run(() =>
                    {
                        try
                        {
                            if (player != null)
                            {
                                if (player.GetSharedDataSafe<int>("Level") >= 100)
                                {
                                    EPlayer.UnlockAchievement(player, "reach_level_100");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            NAPI.Util.ConsoleOutput("EXCEPTION WHEN UNLOCKING REACH_LEVEL_100 INSIDE RUN(): " + ex.ToString());
                        }

                    }, 1000);
                }
                catch (Exception ex) { NAPI.Util.ConsoleOutput("EXCEPTION WHEN UNLOCKING REACH_LEVEL_100 OUTSIDE RUN(): " + ex.ToString()); }
            });
            
        }
        public static void Save(Player player, bool save_linked_entries = true)
        {
            NAPI.Task.Run(() =>
            {
                if (LoggedIn(player))
                {
                    if (player.HasData("PlaytimeTick"))
                    {
                        Account acc = EPlayer.GetAccount(player);
                        double minsPassed = (Environment.TickCount64 - player.GetData<long>("PlaytimeTick")) / 60000;
                        Task.Run(async () =>
                        {
                            using (MainDatabase context = ContextFactory.CreateContext())
                            {
                                acc.MinutesPlayed += minsPassed;
                                acc.LastSessionMins += minsPassed;

                                context.Entry(acc).State = EntityState.Modified;
                                int savedEntries = 1;
                                if (save_linked_entries)
                                {

                                    List<OwnedLoadoutData> guns = await context.OwnedLoadouts.AsQueryable().Where(v => v.AccountSQLID == acc.SQL_ID).ToListAsync();
                                    if(guns.Count > 0)
                                    {
                                        guns.ForEach(v =>
                                        {
                                            context.Entry(v).State = EntityState.Modified;
                                            savedEntries++;
                                        });
                                    }
                                    List<PlayerTunings> tunes = await context.PlayerTunings.AsQueryable().Where(t => t.AccountSQLID == acc.SQL_ID).ToListAsync();
                                    if(tunes.Count > 0)
                                    {
                                        tunes.ForEach(v =>
                                        {
                                            context.Entry(v).State = EntityState.Modified;
                                            savedEntries++;
                                        });
                                    }
                                    List<PlayerOutfit> fits = await context.PlayerOutfits.AsQueryable().Where(t => t.AccountID == acc.SQL_ID).ToListAsync();
                                    if(fits.Count > 0)
                                    {
                                        fits.ForEach(v =>
                                        {
                                            context.Entry(v).State = EntityState.Modified;
                                            savedEntries++;
                                        });
                                    }
                                }

                                await context.SaveChangesAsync();
                            }
                        });
                    }
                    player.SetData("PlaytimeTick", Environment.TickCount64);

                    // unnecessary :/
                    //ContextFactory.SaveDB();
                }
            });
        }
        public static Account GetAccount(Player player)
        {
            if (LoggedIn(player))
            {
                Account playerAcc = player.GetData<Account>("AccountHandle");
                return playerAcc;
            }
            Account nothing = new Account
            {
                SQL_ID = -1,
                Admin = 0,
                Level = 0,
            };
            return nothing;
        }

        public static Player GetByID(int ID)
        {
            return NAPI.Pools.GetAllPlayers().FirstOrDefault(p => p.Id == ID);
        }

        public static void CarryPlayer(Player player, Player target)
        {
            player.SetSharedData("CarryingPlayer", target.Id);
            target.SetSharedData("CarriedBy", player.Id);

            target.TriggerEvent("onGetCarried");

            Events.RequestClearAnimation(target);
            Events.RequestPlayAnimation(target, "nm", "firemans_carry", 1);
            Settings.LoggedInPlayers.ForEach(p =>
            {
                p.TriggerEvent("AttachPlayerToPlayer", target, player);
            });
        }
        public static void StopCarry(Player player)
        {
            int targetID = player.GetSharedDataSafe<int>("CarryingPlayer");
            if (targetID == -1) return;
            Player target = GetByID(targetID);
            if (target == null) return;

            player.TriggerEvent("onStopCarryPlayer");
            target.TriggerEvent("onStopGettingCarried");
            Settings.LoggedInPlayers.ForEach(p =>
            {
                p.TriggerEvent("DetachPlayer", target);
            });

            player.SetSharedData("CarryingPlayer", -1);
            target.SetSharedData("CarriedBy", -1);
        }

        public static void UpdateTeleports(Player player)
        {
            if (player.GetSharedDataSafe<int>("Team") != 0 && player.GetSharedDataSafe<int>("Team") != 1
                && player.GetSharedDataSafe<int>("InLobby") != -3) return;

            Task.Run(async () =>
            {
                using(MainDatabase context = ContextFactory.CreateContext())
                {
                    List<Teleport> teleports = await context.Teleports.AsQueryable().ToListAsync();
                    NAPI.Task.Run(() =>
                    {
                        UpdateTeleports(player, teleports);
                    });
                }
            });
        }
        public static void UpdateTeleports(Player player, List<Teleport> teleports)
        {
            if (player.GetSharedDataSafe<int>("Team") != 0 && player.GetSharedDataSafe<int>("Team") != 1
                && player.GetSharedDataSafe<int>("InLobby") != -3) return;

            string tpData = NAPI.Util.ToJson(teleports);
            player.SetSharedData("TELEPORT:Data", tpData);
        }

        public static void Spawn(Player player, bool forceAutojoin = true, Vector3 spawn = default(Vector3))
        {
            try
            {
                if (player == null) return;
                if (player.HasData("Kicked")) return;
                if (!LoggedIn(player)) return;

                if (player.HasData("CHARACTER:IsNew"))
                {
                    EPlayer.Error(player, "<strong>CHARACTER EDITOR</strong> - You need to edit your character's appearance!");
                    EPlayer.Notify(player, "You can also <strong>import your character</strong> from <strong>Menyoo</strong> with <strong>/char import</strong>!", "blue", "", false, 7500);

                    Character.OpenAppearanceEditor(player, Character.GetCharacter(player).CurrentAppearance);
                    return;
                }

                player.TriggerEvent("EVENTS:SetBankRobberyActive", false);
                player.ResetData("Seatbelt");
                player.TriggerEvent("ToggleSeatbelt", false);
                player.TriggerEvent("stopCopchaseTimerEarlyWarningEffect");

                player.TriggerEvent("clearKillTraces");

                player.TriggerEvent("resetBuildingBlips");

                if (player.HasData("Spectating"))
                {
                    Commands.cmd_specoff(player);
                }
                if (player.HasData("IsNoclipping"))
                {
                    player.SetSharedData("VisibleBlip", player.GetData<bool>("IsNoclipping"));
                    player.ResetData("IsNoclipping");
                    player.TriggerEvent("ForceNoclip", false);
                    player.SetSharedData("EntAlpha", 255);
                    if (EPlayer.GetAccount(player).Admin > 2)
                        Logs.LogAdmin(player, "Forced out of noclip mode.");

                    //// try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:noclip:stop"); } catch { }
                }

                if (!Settings.LoggedInPlayers.Contains(player)) Settings.LoggedInPlayers.Add(player);

                Character.ApplyAppearance(player, Character.GetCharacter(player).SavedAppearance);
                Character.SetTeamOutfit(player, EPlayer.GetAccount(player).LobbyClothing);

                Vector3 SpawnArea = Settings.LobbySpawns.OrderBy(c => Guid.NewGuid()).First();

                player.TriggerEvent("DestroyPlayerCamera");
                player.TriggerEvent("TogglePlayerChat", true);
                player.ResetData("Dead");

                player.ResetSharedData("Handgun_Hash");
                player.SetSharedData("CarryingPlayer", -1);
                player.SetSharedData("CarriedBy", -1);

                player.TriggerEvent("hideMDC");

                if (player.GetSharedDataSafe<int>("Team") == 0)
                {
                    Events.OnPlayerWounded(player, 0, -1);
                }
                else if(player.GetSharedDataSafe<int>("Team") != 0 && player.GetSharedDataSafe<int>("Team") != 1)
                {
                    Events.OnPlayerWounded(player, 0, -1);
                }

                if (player.HasData("DerbyVehicle"))
                {
                    try { player.GetData<Vehicle>("DerbyVehicle").Delete(); }
                    catch { }

                    player.ResetData("DerbyVehicle");
                }

                if (forceAutojoin && player.GetSharedDataSafe<bool>("AutoJoin") == false)
                {
                    EPlayer.Notify(player, Translate(player, "AUTOJOIN_ON"), "green", "fas fa-check", false, 1500, "bottomRight", "replace");
                    player.SetSharedData("AutoJoin", true);
                }

                if (((player.GetSharedDataSafe<int>("InLobby") >= 1 && Settings.Lobbies.Count(lob => lob.lobbyId == player.GetSharedDataSafe<int>("InLobby")) == 0) || // IN DELETED/NON EXISTENT LOBBY
                    player.GetSharedDataSafe<int>("InLobby") == -1 ||  // IN INVALID LOBBY PT2
                    (player.GetSharedDataSafe<int>("InLobby") == -5 && player.GetData<bool>("Creator_Using") == false && player.GetData<bool>("Tuner_Using") == false) ||  // IN CUSTOMIZER & DONE
                    (player.GetSharedDataSafe<int>("InLobby") == -6 && player.GetSharedDataSafe<int>("InDM") == -1)) && // IN CUSTOM ARENAS BUT NOT IN ANY ARENA
                    EPlayer.GetAccount(player).JailTime < 1) // NOT IN JAIL
                {
                    player.SetSharedData("InDM", -1);
                    int LobbyToBeIn = Settings.MaxLobbies;
                    foreach (LobbyData lobby in Settings.Lobbies.OrderByDescending(l => Settings.LoggedInPlayers.Count(a => a.HasSharedData("InLobby") && a.GetSharedDataSafe<int>("InLobby") == l.lobbyId)))
                    {
                        if (Settings.LoggedInPlayers.Count(a => a.GetSharedDataSafe<int>("InLobby") == lobby.lobbyId) >= Settings.Dynamics.MaxInLobby)
                        {
                            continue;
                        }
                        if ((lobby.PlayerCreated && lobby.Password.Length > 1) || !lobby.Enabled) continue;

                        LobbyToBeIn = lobby.lobbyId;
                        break;
                    }

                    player.SetSharedData("InLobby", LobbyToBeIn);

                    foreach (Player targ in Settings.LoggedInPlayers.Where(a => a.GetSharedDataSafe<int>("InLobby") == LobbyToBeIn))
                    {
                        targ.SendChatMessage(Translate(targ, "JOINED_LOBBY_MSG", LobbyToBeIn, EPlayer.Name(player)));
                    }
                }

                player.TriggerEvent("StopHotwire");
                player.TriggerEvent("stopLockpicking");

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

                player.TriggerEvent("ToggleHud", true);

                Vector3 VecSpawn = new Vector3(SpawnArea.X, SpawnArea.Y, SpawnArea.Z);
                if (spawn != default(Vector3))
                {
                    VecSpawn = spawn;
                }

                NAPI.Player.SpawnPlayer(player, VecSpawn);

                EPlayer.Freeze(player, false);
                EPlayer.SetColor(player, new Color(255, 255, 255, 255));
                EPlayer.Save(player, false);

                Events.SetWalkstyle(player, EPlayer.GetAccount(player).WalkstyleClipset, true);

            }
            catch(Exception ex)
            {
                NAPI.Util.ConsoleOutput($"EPLAYER.SPAWN EXCEPTION: {ex.ToString()}");
            }
            
        }

        public static void SendMessageInLobby(int lobbyid, string message_code, params object[] args)
        {
            foreach (Player player in Settings.LoggedInPlayers)
            {
                if (player.HasSharedData("InLobby") && player.GetSharedDataSafe<int>("InLobby") == lobbyid)
                {
                    player.SendChatMessage(Translate(player, message_code, args));
                }
            }
        }

        public static void SendPlayerMessageToAllAdmins(string from, string message, string badges = "[]")
        {
            Account acc;

            foreach (Player i in RAGE.Entities.Players.All)
            {
                if (EPlayer.LoggedIn(i))
                {
                    acc = i.GetData<Account>("AccountHandle");
                    if (acc.Admin > 0) EPlayer.SendPlayerMessage(i, from, "Staff Chat", "a", message, badges);
                }
            }
        }
        public static void SendPlayerMessageToAllAdmins(Player from, string message)
        {
            Account acc;

            foreach (Player i in RAGE.Entities.Players.All)
            {
                if (EPlayer.LoggedIn(i))
                {
                    acc = i.GetData<Account>("AccountHandle");
                    if (acc.Admin > 0) EPlayer.SendPlayerMessage(i, from, "Staff Chat", "a", message);
                }
            }
        }

        public static void UpdatePlayerBadges(Player player)
        {
            try
            {
                string badgeDataJSON = JsonConvert.SerializeObject(GetPlayerBadges(player));
                player.SetSharedData("BadgeData", badgeDataJSON);
            }
            catch(Exception e)
            {
                NAPI.Util.ConsoleOutput(e.ToString());
            }
        }
        
        public static List<Main.SelectedBadgeData> GetPlayerBadges(Player player)
        {
            try
            {
                List<Main.SelectedBadgeData> toReturn = new List<Main.SelectedBadgeData>();

                Account acc = EPlayer.GetAccount(player);

                if (acc.Admin > 0 && player.GetData<bool>("Anonymous") == false)
                {
                    Main.StaticBadgeData _b = Main.GetStaticBadgeData("Admin", acc.Admin);
                    if (_b != null)
                    {
                        string badgeName = EPlayer.GetAdminLevelName(acc.Admin, false);
                        toReturn.Add(new Main.SelectedBadgeData(_b.SQL_ID, badgeName, _b.Image));
                    }
                }
                if (acc.DonatorLevel > 0 && acc.DonatorStatus == true)
                {
                    Main.StaticBadgeData _b = Main.GetStaticBadgeData("Donator", acc.DonatorLevel);
                    if (_b != null)
                    {
                        string badgeName = EPlayer.GetDonatorLevelName(acc.DonatorLevel, false);
                        toReturn.Add(new Main.SelectedBadgeData(_b.SQL_ID, badgeName, _b.Image));
                    }
                }

                Main.SelectedBadgeData _bd = JsonConvert.DeserializeObject<Main.SelectedBadgeData>(acc.SelectedBadgeData);
                if (_bd.id > 0)
                    toReturn.Add(_bd);

                return toReturn;
            }
            catch (Exception e)
            {
                NAPI.Util.ConsoleOutput(e.ToString());

                List<Main.SelectedBadgeData> toReturn = new List<Main.SelectedBadgeData>();
                return toReturn;
            }
            
        }
        
        public static void SendMessageToAllAdmins(string message)
        {
            string messageFormatted = message.Replace("!{#FF6347}", "", StringComparison.OrdinalIgnoreCase);
            messageFormatted = messageFormatted.Replace("!{#FFFFFF}", "", StringComparison.OrdinalIgnoreCase);
            messageFormatted = messageFormatted.Replace("!{#DBCE40}", "", StringComparison.OrdinalIgnoreCase);
            messageFormatted = messageFormatted.Replace("!{#1E9913}", "", StringComparison.OrdinalIgnoreCase);
            messageFormatted = messageFormatted.Replace("!{#33C446}", "", StringComparison.OrdinalIgnoreCase);
            messageFormatted = messageFormatted.Replace("[AdmCmd]", "", StringComparison.OrdinalIgnoreCase);

            Account acc;

            foreach (Player i in RAGE.Entities.Players.All)
            {
                if (EPlayer.LoggedIn(i))
                {
                    acc = i.GetData<Account>("AccountHandle");
                    if (acc.Admin > 0) i.SendChatMessage($"!{{#FF6347}}[AdmCmd] !{{#DB8840}}{messageFormatted}");
                    //if (acc.Admin > 0) i.SendChatMessage($"[A] {message}");
                }
            }

            DiscordBot.SendChatMessage(DiscordBot.ChatMessageType.General, messageFormatted, "Admin");
            //Discord.SendTextChannel("staff-bot", $"(ADM) |** {message}**");
        }


        public static void UpdateUIVars(Player player)
        {
            player.TriggerEvent("CS_global.UpdateStatsVars");
        }

        public static void Notify(Player player, string title, string message, string alignment = "", string displaymode = "")
        {
            player.TriggerEvent("HUDNotify2", title, message, alignment, displaymode);
        }
        public static void Error(Player player, string message, string icon = "")
        {
            player.TriggerEvent("HUDError", message, icon);
        }
        public static void Notify(Player player, string msg, string color, string icon, bool close, int timeout, string alignment = "", string displaymode = "")
        {
            string type = "none";
            switch (color)
            {
                case "red": type = "error";
                    break;
                case "blue": type = "info";
                    break;
                case "orange": type = "warning";
                    break;
                case "yellow": type = "warning";
                    break;
                case "green": type = "success";
                    break;
                default: type = "none";
                    break;
            }
            player.TriggerEvent("HUDNotify", msg, type, icon, close, timeout, alignment, displaymode);
        }
    }
}
