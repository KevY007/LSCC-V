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
using System.Threading.Tasks;
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

        public string OldPassword { get; set; }
        public string OldSalt { get; set; }

        public string Password { get; set; }
        public string Salt { get; set; }
        public bool ForcePasswordChange { get; set; }

        public string DiscordID { get; set; }
        public string DiscordCode { get; set; }
        public bool UsedDiscordPerks { get; set; }
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
        public bool DonatorStatus { get; set; }
        public int Medkits { get; set; }
        public int Painkillers { get; set; }
        public int Language { get; set; }
        public int JailTime { get; set; }
        public string JailReason { get; set; }
        public int MuteTime { get; set; }
        public string MuteReason { get; set; }
        public long CopchaseBanTime { get; set; }
        public string CopchaseBanReason { get; set; }
        public long DeathmatchBanTime { get; set; }
        public string DeathmatchBanReason { get; set; }
        public long FreeroamBanTime { get; set; }
        public string FreeroamBanReason { get; set; }
        public string WalkstyleClipset { get; set; }
        public int Warns { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Arrests { get; set; }
        public int Escapes { get; set; }
        public bool FinishedTutorial { get; set; }
        public int Lockpicks { get; set; }
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
        public string SelectedBadgeData { get; set; }
        public bool ClanPartnering { get; set; }
        public bool ClanTag { get; set; }

        public string TattooList { get; set; }

        public string ActiveCrosshairProfile { get; set; }
        public string CrosshairProfiles { get; set; }
    }

    public class ChallengeData
    {
        public int ChallengeType;
        public string ChallengeItem;
        public int ChallengeProgress;
        public bool Completed;
        public long Expire;

        public ChallengeData()
        {
            ChallengeType = 0;
            ChallengeItem = "police";
            ChallengeProgress = 0;
            Completed = false;
            Expire = 0;
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
        public static string OldGenSalt()
        {
            return Crypter.Sha256.GenerateSalt();
        }
        public static string OldCrypt(string pass, string salt)
        {
            return Crypter.Sha256.Crypt(pass, salt);
        }

        public static string GenSalt()
        {
            return BCrypt.Net.BCrypt.GenerateSalt();
        }
        public static string Crypt(string pass, string salt)
        {
            return BCrypt.Net.BCrypt.HashPassword(pass, salt);
        }

        public async static Task<bool> RecoverAccount(Account acc)
        {
            using (MainDatabase context = ContextFactory.CreateContext())
            {
                await context.Accounts.AddAsync(acc);
                using (BackupDatabase backup = BackupContextFactory.CreateContext())
                {
                    List<OwnedVehData> ownedVehs = await backup.OwnedVehicles.AsQueryable().Where(v => v.AccountSQLID == acc.SQL_ID).ToListAsync();
                    List<OwnedLoadoutData> ownedLoadouts = await backup.OwnedLoadouts.AsQueryable().Where(v => v.AccountSQLID == acc.SQL_ID).ToListAsync();
                    List<PlayerOutfit> savedOutfits = await backup.PlayerOutfits.AsQueryable().Where(v => v.AccountID == acc.SQL_ID).ToListAsync();
                    List<PlayerTunings> savedTunings = await backup.PlayerTunings.AsQueryable().Where(v => v.AccountSQLID == acc.SQL_ID).ToListAsync();
                    List<UnlockedAchievement> unlockedAchievements = await backup.UnlockedAchievements.AsQueryable().Where(v => v.AccountID == acc.SQL_ID).ToListAsync();

                    await context.OwnedLoadouts.AddRangeAsync(ownedLoadouts);
                    await context.OwnedVehicles.AddRangeAsync(ownedVehs);
                    await context.PlayerOutfits.AddRangeAsync(savedOutfits);
                    await context.PlayerTunings.AddRangeAsync(savedTunings);
                    await context.UnlockedAchievements.AddRangeAsync(unlockedAchievements);

                    backup.PlayerOutfits.RemoveRange(savedOutfits);
                    backup.OwnedLoadouts.RemoveRange(ownedLoadouts);
                    backup.OwnedVehicles.RemoveRange(ownedVehs);
                    backup.PlayerTunings.RemoveRange(savedTunings);
                    backup.UnlockedAchievements.RemoveRange(unlockedAchievements);
                    backup.Accounts.Remove(acc);

                    await context.SaveChangesAsync();
                    await backup.SaveChangesAsync();

                    NAPI.Util.ConsoleOutput("[ACCOUNT ARCHIVE] " + acc.Username + " has been recovered from the archive.");
                }

            }
            return true;
        }
        

        public async static Task<bool> ClearForeignKeys(Account acc)
        {
            using (MainDatabase context = ContextFactory.CreateContext())
            {
                List<Logs_Admin> aLogs = await context.Logs_Admin.AsQueryable().Where(l => l.AccountID == acc.SQL_ID).ToListAsync();
                context.Logs_Admin.RemoveRange(aLogs);
                List<Logs_AdminRecord> arLogs = await context.Logs_AdminRecords.AsQueryable().Where(l => l.AccountID == acc.SQL_ID || l.AdminID == acc.SQL_ID).ToListAsync();
                context.Logs_AdminRecords.RemoveRange(arLogs);
                List<Logs_Session> sLogs = await context.Logs_Session.AsQueryable().Where(l => l.AccountID == acc.SQL_ID).ToListAsync();
                context.Logs_Session.RemoveRange(sLogs);
                List<OwnedVehData> oVehData = await context.OwnedVehicles.AsQueryable().Where(l => l.AccountSQLID == acc.SQL_ID).ToListAsync();
                context.OwnedVehicles.RemoveRange(oVehData);
                List<OwnedLoadoutData> oLData = await context.OwnedLoadouts.AsQueryable().Where(l => l.AccountSQLID == acc.SQL_ID).ToListAsync();
                context.OwnedLoadouts.RemoveRange(oLData);
                List<BanData> bData = await context.Bans.AsQueryable().Where(l => l.AccountSQLID == acc.SQL_ID).ToListAsync();
                context.Bans.RemoveRange(bData);
                List<AdminNote> anotedata = await context.AdminNotes.AsQueryable().Where(l => l.AccountID == acc.SQL_ID || l.AdminID == acc.SQL_ID).ToListAsync();
                context.AdminNotes.RemoveRange(anotedata);
                List<PlayerTunings> ptune = await context.PlayerTunings.AsQueryable().Where(l => l.AccountSQLID == acc.SQL_ID).ToListAsync();
                context.PlayerTunings.RemoveRange(ptune);
                List<PlayerOutfit> pout = await context.PlayerOutfits.AsQueryable().Where(l => l.AccountID == acc.SQL_ID).ToListAsync();
                context.PlayerOutfits.RemoveRange(pout);
            }
            return true;
        }

        public static bool MatchPass(string a, string b) { if (a.CompareTo(b) == 0) { return true; } return false; }

        public static void Add(Account _a)
        {
            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    await context.Accounts.AddAsync(_a);
                    await context.SaveChangesAsync();
                }
            });

        }
        public static bool Login(Account _target, string password, bool _crypted = false, bool _useOldPassword = false)
        {
            if (_useOldPassword)
            {
                foreach (Player player in Settings.LoggedInPlayers)
                {
                    if (EPlayer.GetAccount(player) != null && EPlayer.GetAccount(player).SQL_ID == _target.SQL_ID) return false; // Return FALSE if another PLAYER is online from SAME ACC.
                }

                if (_target.Suspended == 1 && _target.Admin < 4) return false;

                string crypted = password;
                if (!_crypted) crypted = OldCrypt(password, _target.OldSalt);
                if (MatchPass(crypted, _target.OldPassword))
                {
                    return true;
                }
                return false;
            }
            else
            {
                foreach (Player player in Settings.LoggedInPlayers)
                {
                    if (EPlayer.GetAccount(player) != null && EPlayer.GetAccount(player).SQL_ID == _target.SQL_ID) return false; // Return FALSE if another PLAYER is online from SAME ACC.
                }

                if (_target.Suspended == 1 && _target.Admin < 4) return false;

                string crypted = password;
                if (!_crypted) crypted = Crypt(password, _target.Salt);
                if (MatchPass(crypted, _target.Password))
                {
                    return true;
                }
                return false;
            }

        }
    }
}
