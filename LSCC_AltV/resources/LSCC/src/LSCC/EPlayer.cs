using AltV.Net.Elements.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using AltV.Net;
using AltV.Net.Data;
using System.Linq;
using AltV.Net.Enums;

namespace LSCC
{
    public class CarSelectorData
    {
        public string[] Cars { get; set; }
        public string[] Images { get; set; }
        public int[] Levels { get; set; }
        public string[] Desc { get; set; }
        public bool[] Locked { get; set; }

        public CarSelectorData(object[,] carData, int level)
        {
            int numcars = carData.GetLength(0);

            this.Cars = new string[numcars];
            this.Images = new string[numcars];
            this.Levels = new int[numcars];
            this.Desc = new string[numcars];
            this.Locked = new bool[numcars];

            for (int i = 0; i < carData.GetLength(0); i++)
            {
                this.Cars[i] = (string)carData[i, 0];
                this.Images[i] = (string)carData[i, 1];
                this.Levels[i] = (int)carData[i, 2];
                this.Desc[i] = (string)carData[i, 3];
                this.Locked[i] = (level >= this.Levels[i] ? false : true);
            }
        }
    }

    public static class EStatics
    {
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
                case 4:
                    if (colored) name = "!{#9DB7C9}";
                    name = name + "Platinum Donator";
                    break;
                case 5:
                    if (colored) name = "!{#E0115F}";
                    name = name + "Ruby Donator";
                    break;
                default:
                    name = "Player";
                    break;
            }
            return name;
        }
    }
    public class EPlayer : Player
    {
        public bool LoggedIn { get; set; }
        public Account Account { get; set; }
        public bool PMState { get; set; }
        public bool HasShot { get; set; }
        public int LobbyID { get; set; }
        public int DMID { get; set; }
        public int Team { get; set; }
        public bool AutoJoin { get; set; }
        public List<IVehicle> FreeroamVehs { get; set; }
        public IBlip Blip { get; set; }
        public IPlayer PlayerSquad { get; set; }
        public IPlayer AskedBySquad { get; set; }
        public IPlayer CuffingTarget { get; set; }
        public List<IPlayer> BlockList { get; set; }
        public bool Visible { get; set; }
        public bool GPS { get; set; }
        public int DerbyTime { get; set; }
        public bool Cuffed { get; set; }
        public IVehicle DerbyVehicle { get; set; }

        public void SendNotification(string message, bool flashing = false)
        {
            //TODO
        }

        public void SendChatMessage(string message)
        {
            //TODO
        }

        public void Jail(int minutes, string reason)
        {
            if (!LoggedIn) return;

            Account.JailReason = reason;
            Account.JailTime = minutes * 60;
            SendChatMessage($"!{{#FF6347}}[!] !{{#FFFFFF}}You have been jailed for {minutes} minutes, reason: {reason}.");
            foreach (LobbyData il in Settings.Lobbies)
            {
                il.Cops.Remove(this);
                il.Fugitives.Remove(this);
            }
            SetSyncedMetaData("JailTime", Account.JailTime);
            Emit("SetJailCounter", Account.JailTime);
            Spawn((Position)Settings.VectorJail[0], 1);
        }

        public void GiveCopWeapons()
        {
            if (!LoggedIn) return;

            if (Account.LoadoutCop.Length > 3)
            {
                LoadoutData loadout = Settings.Loadouts.Find(a => a.Team == 1 && a.Name.Equals(Account.LoadoutCop));

                if ((loadout != null) == false)
                {
                    Account.LoadoutCop = "?";
                    GiveCopWeapons();
                    return;
                }

                if (Account.Level < loadout.LevelRequired)
                {
                    Account.LoadoutCop = "?";
                    GiveCopWeapons();
                    return;
                }

                if (loadout.Weapons.Count() > 0)
                {
                    loadout.Weapons.ForEach((wepar) =>
                    {
                        this.GiveWeapon(wepar.Item1, wepar.Item2, true);
                    });
                }
            }
            else
            {
                this.GiveWeapon(WeaponModel.PumpShotgun, 20, true);
                this.GiveWeapon(WeaponModel.Pistol, 100, true);
            }

            this.GiveWeapon(WeaponModel.StunGun, 1, true);

            SendChatMessage("!{#FF6347}[TASER] !{#FFFFFF}You have been equipped with a stungun, shoot at a fugitive that hasn't shot to tase them.");
            return;
        }
        public void GiveFugitiveWeapons()
        {
            if (!LoggedIn) return;

            if (Account.LoadoutFugitive.Length > 3)
            {
                LoadoutData loadout = Settings.Loadouts.Find(a => a.Team == 0 && a.Name.Equals(Account.LoadoutFugitive));

                if ((loadout != null) == false)
                {
                    Account.LoadoutFugitive = "?";
                    GiveFugitiveWeapons();
                    return;
                }

                if (Account.Level < loadout.LevelRequired)
                {
                    Account.LoadoutFugitive = "?";
                    GiveFugitiveWeapons();
                    return;
                }

                if (loadout.Weapons.Count() > 0)
                {
                    loadout.Weapons.ForEach((wepar) =>
                    {
                        this.GiveWeapon(wepar.Item1, wepar.Item2, true);
                    });
                }
            }
            else
            {
                this.GiveWeapon(WeaponModel.Pistol, 100, true);
                this.GiveWeapon(WeaponModel.PumpShotgun, 20, true);
            }
            return;
        }

        public void UpdateBlip()
        {
            if (Blip != null)
            {
                Blip.Remove();
                Blip = null;
            }

            if ((Visible) ||
                (GPS == true && Team == 1))            
            {
                byte blipColor = 45;
                BlipType blipType = BlipType.Ped;

                if (Team == 0)
                {
                    blipColor = 75;

                    if (HasShot == true)
                    {
                        blipColor = 76;
                    }
                }

                if (Team == 1)
                {
                    blipColor = 3;
                    blipType = BlipType.Cop;
                }

                if (LobbyID == -3) blipColor = 15;
                if (LobbyID == -4) blipColor = 73;
                if (DMID != -1) blipColor = 39;

                if (IsInVehicle == true && Team != 1) blipType = BlipType.Vehicle;

                Blip = Alt.CreateBlip(blipType, this);
                Blip.Color = blipColor;
                Blip.Dimension = Dimension;
            }
        }

        public void Freeze(bool toggle)
        {
            Emit("SetPlayerFrozen", toggle);

            if (toggle)
            {

            }
            else
            {
               
            }
        }

        public void Action(string text)
        {
            var msg = "!{#C3A3DB}* " + Name + " " + text;

            foreach (EPlayer playa in Settings.LoggedInPlayers.Where(a => a.Position.Distance(Position) < 40.0))
            {
                playa.SendChatMessage(msg);
            }
        }

        public void Handcuff(bool toggle)
        {
            Emit("SetHandcuffs", toggle);
            Cuffed = true;
            if (!toggle) Cuffed = false;
        }

        public void SetColor(int r, int g, int b)
        {
            SetSyncedMetaData("Color_R", r);
            SetSyncedMetaData("Color_G", g);
            SetSyncedMetaData("Color_B", b);
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
        public void GrantXP(int xp)
        {
            if (!LoggedIn) return;

            Account.XP += xp;

            if (Account.XP < 0)
            {
                if (Account.Level > 0)
                {
                    Account.Level--;

                    Account.XP += GetMaxLevelXP(Account.Level);
                    Settings.LoggedInPlayers.ForEach((EPlayer pl) =>
                    {
                        pl.SendChatMessage($"!{{#00BA03}}[LEVEL DOWN] {Name} has levelled down to {Account.Level}!");
                    });
                    SendNotification($"~w~You have levelled down to ~r~{Account.Level}~w~!");
                }
            }

            while (CheckXPLevelUp(Account.Level, Account.XP) == true)
            {
                Account.XP -= GetMaxLevelXP(Account.Level);
                Account.Level++;
                Settings.LoggedInPlayers.ForEach((EPlayer pl) =>
                {
                    pl.SendChatMessage($"!{{#00BA03}}[LEVEL UP] {Name} has levelled up to {Account.Level}!");
                });
                SendNotification($"~w~You have levelled up to ~g~{Account.Level}~w~!");
            }
        }
        public void Save()
        {
            if (LoggedIn)
            {
                ContextFactory.Instance.SaveChangesAsync();
            }
        }

        public void Spawn(bool forceAutojoin = true, Position spawn = default(Position))
        {
            if (!LoggedIn) return;

            if (!Settings.LoggedInPlayers.Contains(this)) Settings.LoggedInPlayers.Add(this);

            Position SpawnArea = Settings.LobbySpawns[new Random().Next(Settings.LobbySpawns.Length)];

            Emit("DestroyPlayerCamera");
            Emit("TogglePlayerChat", true);
            
            if (DerbyVehicle != null)
            {
                DerbyVehicle.Remove();

                DerbyVehicle = null;
            }

            if (forceAutojoin && AutoJoin == false)
            {
                SendNotification($"~g~Autojoin has been enabled.", false);
                AutoJoin = true;
            }

            if (LobbyID == -1 || LobbyID == -5)
            {
                DMID = -1;
                int LobbyToBeIn = Settings.MaxLobbies;
                foreach (LobbyData lobby in Settings.Lobbies)
                {
                    if (Settings.LoggedInPlayers.Count(a => a.LobbyID == lobby.lobbyId) >= Settings.MaxPlayersInLobby)
                    {
                        continue;
                    }
                    if ((lobby.PlayerCreated && lobby.Password.Length > 1) || !lobby.Enabled) continue;

                    LobbyToBeIn = lobby.lobbyId;
                    break;
                }

                LobbyID = LobbyToBeIn;

                foreach (EPlayer targ in Settings.LoggedInPlayers.Where(a => a.LobbyID == LobbyToBeIn))
                {
                    targ.SendChatMessage($"[Lobby {LobbyToBeIn}] {Name} has joined this lobby.");
                }
            }



            foreach (LobbyData lobby in Settings.Lobbies)
            {
                if (lobby.gameStatus == 1)
                {
                    if (lobby.Fugitives.Contains(this))
                    {
                        lobby.Fugitives.Remove(this);
                    }
                    else if (lobby.Cops.Contains(this))
                    {
                        lobby.Cops.Remove(this);
                    }
                }
            }
            Team = -1;

            Position VecSpawn = new Position(SpawnArea.X, SpawnArea.Y, SpawnArea.Z);
            if (spawn != default(Position))
            {
                VecSpawn = spawn;
            }

            Spawn(VecSpawn, 1);

            Freeze(false);
            SetColor(255, 255, 255);

        }

        public EPlayer(IntPtr nativePointer, ushort id) : base(nativePointer, id)
        {
            Visible = false;
            GPS = false;
            AutoJoin = true;
            LoggedIn = false;
            LobbyID = -1;
            PMState = false;
            HasShot = false;
            DMID = -1;
            Team = -1;
            CuffingTarget = null;
            Cuffed = false;
            Account = null;
            FreeroamVehs = new List<IVehicle>();
            BlockList = new List<IPlayer>();
            PlayerSquad = null;
            AskedBySquad = null;
            Blip = null;
            DerbyVehicle = null;
            DerbyTime = 0;

            SetSyncedMetaData("JailTime", 0);
            SetSyncedMetaData("MuteTime", 0);
            SetSyncedMetaData("Level", 0);
            SetSyncedMetaData("Color_R", 255);
            SetSyncedMetaData("Color_G", 255);
            SetSyncedMetaData("Color_B", 255);
            SetSyncedMetaData("Ping", Ping);
            SetSyncedMetaData("Typing", false);

            foreach (LobbyData il in Settings.Lobbies)
            {
                il.Cops.Remove(this);
                il.Fugitives.Remove(this);
            }
            Settings.LoggedInPlayers.Remove(this);
        }
    }
    public class EPlayerFactory : IEntityFactory<IPlayer>
    {
        public IPlayer Create(IntPtr playerPointer, ushort id)
        {
            return new EPlayer(playerPointer, id);
        }
    }
}
