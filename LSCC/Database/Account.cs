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
using System.Threading.Tasks;

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
        public int ClanID { get; set; }
        public int ClanTier { get; set; }
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

        private async static Task<bool> archiveAccount_Async(Account acc, List<OwnedVehData> ownedVehs, List<OwnedLoadoutData> ownedLoadouts, List<PlayerOutfit> savedOutfits, List<PlayerTunings> savedTunings, List<UnlockedAchievement> unlockedAchievements)
        {
            await BackupContextFactory.Instance.OwnedLoadouts.AddRangeAsync(ownedLoadouts);
            await BackupContextFactory.Instance.OwnedVehicles.AddRangeAsync(ownedVehs);
            await BackupContextFactory.Instance.PlayerOutfits.AddRangeAsync(savedOutfits);
            await BackupContextFactory.Instance.PlayerTunings.AddRangeAsync(savedTunings);
            await BackupContextFactory.Instance.UnlockedAchievements.AddRangeAsync(unlockedAchievements);
            await BackupContextFactory.Instance.SaveChangesAsync();
            NAPI.Task.Run(() => OnFinishArchiveAccountTask(acc, ownedVehs, ownedLoadouts, savedOutfits, savedTunings, unlockedAchievements));
            return true;
        }
        private static void OnFinishArchiveAccountTask(Account acc, List<OwnedVehData> ownedVehs, List<OwnedLoadoutData> ownedLoadouts, List<PlayerOutfit> savedOutfits, List<PlayerTunings> savedTunings, List<UnlockedAchievement> unlockedAchievements)
        {
            ContextFactory.Instance.PlayerOutfits.RemoveRange(savedOutfits);
            ContextFactory.Instance.OwnedLoadouts.RemoveRange(ownedLoadouts);
            ContextFactory.Instance.OwnedVehicles.RemoveRange(ownedVehs);
            ContextFactory.Instance.PlayerTunings.RemoveRange(savedTunings);
            ContextFactory.Instance.UnlockedAchievements.RemoveRange(unlockedAchievements);

            ContextFactory.Instance.Accounts.Remove(acc);

            List<Logs_Session> records = ContextFactory.Instance.Logs_Session.ToList().Where(a => a.AccountID == acc.SQL_ID).ToList();
            ContextFactory.Instance.Logs_Session.RemoveRange(records);
            ContextFactory.Instance.SaveChanges();
            NAPI.Util.ConsoleOutput($"[RnC ARCHIVE] Finished archiving account {acc.Username}.");
        }
        public static void ArchiveAccount(Account acc)
        {
            NAPI.Util.ConsoleOutput($"[RnC ARCHIVE] Called ArchiveAccount({acc.Username})");
            if (BackupContextFactory.Instance.Accounts.FirstOrDefault(a => a.Username == acc.Username) == null)
            {
                NAPI.Util.ConsoleOutput($"[RnC ARCHIVE] Started archiving account {acc.Username}.");
                BackupContextFactory.Instance.Accounts.Add(acc);

                List<OwnedVehData> ownedVehs = ContextFactory.Instance.OwnedVehicles.ToList().Where(v => v.AccountSQLID == acc.SQL_ID).ToList();
                List<OwnedLoadoutData> ownedLoadouts = ContextFactory.Instance.OwnedLoadouts.ToList().Where(v => v.AccountSQLID == acc.SQL_ID).ToList();
                List<PlayerOutfit> savedOutfits = ContextFactory.Instance.PlayerOutfits.ToList().Where(v => v.AccountID == acc.SQL_ID).ToList();
                List<PlayerTunings> savedTunings = ContextFactory.Instance.PlayerTunings.ToList().Where(v => v.AccountSQLID == acc.SQL_ID).ToList();
                List<UnlockedAchievement> unlockedAchievements = ContextFactory.Instance.UnlockedAchievements.ToList().Where(v => v.AccountID == acc.SQL_ID).ToList();

                Task.Run(async () =>
                {
                    await archiveAccount_Async(acc, ownedVehs, ownedLoadouts, savedOutfits, savedTunings, unlockedAchievements);
                });
            }
        }

        public async static Task<bool> RecoverAccount(Account acc, string password, Player player, bool isEmail, string username)
        {
            ContextFactory.Instance.Accounts.Add(acc);
            BackupContextFactory.Instance.Accounts.Remove(acc);

            List<OwnedVehData> ownedVehs = await BackupContextFactory.Instance.OwnedVehicles.AsAsyncEnumerable().Where(v => v.AccountSQLID == acc.SQL_ID).ToListAsync();
            List<OwnedLoadoutData> ownedLoadouts = await BackupContextFactory.Instance.OwnedLoadouts.AsAsyncEnumerable().Where(v => v.AccountSQLID == acc.SQL_ID).ToListAsync();
            List<PlayerOutfit> savedOutfits = await BackupContextFactory.Instance.PlayerOutfits.AsAsyncEnumerable().Where(v => v.AccountID == acc.SQL_ID).ToListAsync();
            List<PlayerTunings> savedTunings = await BackupContextFactory.Instance.PlayerTunings.AsAsyncEnumerable().Where(v => v.AccountSQLID == acc.SQL_ID).ToListAsync();
            List<UnlockedAchievement> unlockedAchievements = await BackupContextFactory.Instance.UnlockedAchievements.AsAsyncEnumerable().Where(v => v.AccountID == acc.SQL_ID).ToListAsync();

            Account toReturn = null;
            NAPI.Task.Run(() =>
            {
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


                ContextFactory.Instance.SaveChanges();
                NAPI.Util.ConsoleOutput("[ACCOUNT ARCHIVE] " + acc.Username + " has been recovered from the archive.");

                toReturn = ContextFactory.Instance.Accounts.FirstOrDefault(a => a.Username.Equals(acc.Username, StringComparison.OrdinalIgnoreCase));
                NAPI.Util.ConsoleOutput($"[RnC LOGIN - ARCHIVE] Returning account: {toReturn.Username} (DBID:{toReturn.SQL_ID})");

                Events.OnFinishAccountFindTask(toReturn, password, player, isEmail, username);
            });
            await BackupContextFactory.Instance.SaveChangesAsync();
            return true;
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
