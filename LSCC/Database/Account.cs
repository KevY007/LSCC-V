using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Pomelo.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql;
using Pomelo.EntityFrameworkCore.MySql.Design;
using System.Reflection;
using CryptSharp;
using RNC;
using System.Security.Cryptography;
using GTANetworkAPI;
using System.Linq;

namespace RNC
{
    public class Account
    {
        [Key]
        public int SQL_ID { get; set; }

        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
        public string DiscordInfo { get; set; }
        public string ChallengeInfo { get; set; }
        public DateTime RegisterDate { get; set; }
        public string RegisterHWID { get; set; }
        public ulong RegisterRID { get; set; }
        public string RegisterIP { get; set; }
        public string RegisterSC { get; set; }
        public string LastHWID { get; set; }
        public ulong LastRID { get; set; }
        public string LastIP { get; set; }
        public string LastSC { get; set; }
        public bool AutoLogin { get; set; }
        public DateTime LastLogin { get; set; }
        public int Admin { get; set; }
        public int Level { get; set; }
        public int XP { get; set; }
        public int DonatorLevel { get; set; }
        public int Medkits { get; set; }
        public int Painkillers { get; set; }
        public int Language { get; set; }
        public int JailTime { get; set; }
        public string JailReason { get; set; }
        public int MuteTime { get; set; }
        public string MuteReason { get; set; }
        public int Warns { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Arrests { get; set; }
        public int Escapes { get; set; }
        public int DerbyWins { get; set; }
        public int DonatorPoints { get; set; }
        public int Points { get; set; }
        public double MinutesPlayed { get; set; }
        public int Suspended { get; set; }
        public int LobbyClothing { get; set; }
        public string Character { get; set; }
        public string LoadoutCop { get; set; }
        public string LoadoutFugitive { get; set; }
        public string CarFugitive { get; set; }
        public string CarCop { get; set; }
        public string WeaponComponents { get; set; }
        public string MOTD { get; set; }
        public double LastSessionMins { get; set; }
        public string PurchasedAmmo { get; set; }
        public int ChallengeStreak { get; set; }
        public bool CompletedDailyChallenge { get; set; }
        public int Namechanges { get; set; }
    }

    public class ChallengeData
    {
        public int ChallengeType;
        public string ChallengeItem;
        public int ChallengeProgress;
        public bool Completed;

        public ChallengeData()
        {
            ChallengeType = 0;
            ChallengeItem = "police";
            ChallengeProgress = 0;
            Completed = false;
        }
    }

    public class DiscordData
    {
        public string DiscordID;
        public string DiscordCode;
        public bool LeftDiscord;
        public bool UsedDiscordPerks;
        public DiscordData()
        {
            DiscordID = "n/a";
            LeftDiscord = false;
            UsedDiscordPerks = false;

            Random random = new Random();
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            string generatedCode = new string(Enumerable.Repeat(chars, 5)
              .Select(s => s[random.Next(s.Length)]).ToArray());

            DiscordCode = generatedCode;
        }
    }

    public static class AccountManager
    {
        public static string GenSalt()
        {
            return Crypter.Sha256.GenerateSalt();
        }

        public static string Crypt(string pass, string salt)
        {
            return Crypter.Sha256.Crypt(pass, salt);
        }

        public static void RecoverAccount(Account acc)
        {
            ContextFactory.Instance.Accounts.Add(acc);

            List<OwnedVehData> ownedVehs = BackupContextFactory.Instance.OwnedVehicles.ToList().Where(v => v.AccountSQLID == acc.SQL_ID).ToList();
            List<OwnedLoadoutData> ownedLoadouts = BackupContextFactory.Instance.OwnedLoadouts.ToList().Where(v => v.AccountSQLID == acc.SQL_ID).ToList();
            List<PlayerOutfit> savedOutfits = BackupContextFactory.Instance.PlayerOutfits.ToList().Where(v => v.AccountID == acc.SQL_ID).ToList();
            List<PlayerTunings> savedTunings = BackupContextFactory.Instance.PlayerTunings.ToList().Where(v => v.AccountSQLID == acc.SQL_ID).ToList();
            List<UnlockedAchievement> unlockedAchievements = BackupContextFactory.Instance.UnlockedAchievements.ToList().Where(v => v.AccountID == acc.SQL_ID).ToList();

            ContextFactory.Instance.OwnedLoadouts.AddRange(ownedLoadouts);
            ContextFactory.Instance.OwnedVehicles.AddRange(ownedVehs);
            ContextFactory.Instance.PlayerOutfits.AddRange(savedOutfits);
            ContextFactory.Instance.PlayerTunings.AddRange(savedTunings);
            ContextFactory.Instance.UnlockedAchievements.AddRange(unlockedAchievements);

            BackupContextFactory.Instance.PlayerOutfits.RemoveRange(savedOutfits);
            BackupContextFactory.Instance.OwnedLoadouts.RemoveRange(ownedLoadouts);
            BackupContextFactory.Instance.OwnedVehicles.RemoveRange(ownedVehs);
            BackupContextFactory.Instance.PlayerTunings.RemoveRange(savedTunings);
            BackupContextFactory.Instance.UnlockedAchievements.RemoveRange(unlockedAchievements);

            BackupContextFactory.Instance.Accounts.Remove(acc);

            ContextFactory.Instance.SaveChanges();
            BackupContextFactory.Instance.SaveChanges();
            NAPI.Util.ConsoleOutput("[ACCOUNT ARCHIVE] " + acc.Username + " has been recovered from the archive.");
        }

        public static void ClearForeignKeys(Account acc)
        {
            List<Logs_Admin> aLogs = ContextFactory.Instance.Logs_Admin.ToList().Where(l => l.AccountID == acc.SQL_ID).ToList();
            ContextFactory.Instance.Logs_Admin.RemoveRange(aLogs);
            List<Logs_AdminRecord> arLogs = ContextFactory.Instance.Logs_AdminRecords.ToList().Where(l => l.AccountID == acc.SQL_ID || l.AdminID == acc.SQL_ID).ToList();
            ContextFactory.Instance.Logs_AdminRecords.RemoveRange(arLogs);
            List<Logs_Session> sLogs = ContextFactory.Instance.Logs_Session.ToList().Where(l => l.AccountID == acc.SQL_ID).ToList();
            ContextFactory.Instance.Logs_Session.RemoveRange(sLogs);
            List<OwnedVehData> oVehData = ContextFactory.Instance.OwnedVehicles.ToList().Where(l => l.AccountSQLID == acc.SQL_ID).ToList();
            ContextFactory.Instance.OwnedVehicles.RemoveRange(oVehData);
            List<OwnedLoadoutData> oLData = ContextFactory.Instance.OwnedLoadouts.ToList().Where(l => l.AccountSQLID == acc.SQL_ID).ToList();
            ContextFactory.Instance.OwnedLoadouts.RemoveRange(oLData);
            List<BanData> bData = ContextFactory.Instance.Bans.ToList().Where(l => l.AccountSQLID == acc.SQL_ID).ToList();
            ContextFactory.Instance.Bans.RemoveRange(bData);
            List<AdminNote> anotedata = ContextFactory.Instance.AdminNotes.ToList().Where(l => l.AccountID == acc.SQL_ID || l.AdminID == acc.SQL_ID).ToList();
            ContextFactory.Instance.AdminNotes.RemoveRange(anotedata);
            List<PlayerTunings> ptune = ContextFactory.Instance.PlayerTunings.ToList().Where(l => l.AccountSQLID == acc.SQL_ID).ToList();
            ContextFactory.Instance.PlayerTunings.RemoveRange(ptune);
            List<PlayerOutfit> pout = ContextFactory.Instance.PlayerOutfits.ToList().Where(l => l.AccountID == acc.SQL_ID).ToList();
            ContextFactory.Instance.PlayerOutfits.RemoveRange(pout);
        }

        public static bool MatchPass(string a, string b) { if (a.CompareTo(b) == 0) { return true; } return false; }

        public static void Add(Account _a)
        {
            ContextFactory.Instance.Accounts.Add(_a);

            ContextFactory.Instance.SaveChanges();
        }
        public static bool Login(Account _target, string password, bool _crypted = false)
        {
            foreach(Player player in Settings.LoggedInPlayers)
            {
                if (EPlayer.GetAccount(player) != null && EPlayer.GetAccount(player).SQL_ID == _target.SQL_ID) return false; // Return FALSE if another PLAYER is online from SAME ACC.
            }

            if (_target.Suspended == 1 && _target.Admin < 4) return false;

            string crypted = password;
            if (!_crypted) crypted = Crypt(password, _target.Salt);
            if(MatchPass(crypted, _target.Password))
            {
                return true;
            }
            return false;
        }
    }
}
