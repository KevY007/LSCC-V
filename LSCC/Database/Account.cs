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
using System.Numerics;

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
        [Column(TypeName = "VARCHAR")]
        [StringLength(255)]
        public string RegisterHWID { get; set; }
        public ulong RegisterRID { get; set; }
        public string RegisterIP { get; set; }
        public string RegisterSC { get; set; }
        [Column(TypeName = "VARCHAR")]
        [StringLength(255)]
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
        public int CopOutfitID { get; set; }
        public int FugitiveOutfitID { get; set; }
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

        public bool EmailVerified { get; set; }
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
            return BCrypt.Net.BCrypt.HashPassword(pass);
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
        

        public async static Task BanEvadeCheck(Player player, Account acc, List<Account> cachedLinkedAccs = null)
        {
            List<Account> linkedAccs = new List<Account>();
            if (cachedLinkedAccs == null)
            {
                linkedAccs = await EPlayer.GetLinkedAccounts(acc);
            }
            else linkedAccs = cachedLinkedAccs;
            var linkedSQLs = linkedAccs.Select(accs => accs.SQL_ID).ToList(); // BONKERS
            using (MainDatabase context = ContextFactory.CreateContext())
            {
                bool banEvadingAccountNotFound = false;
                IEnumerable<int> linkedSQLList = linkedSQLs.Intersect(context.Bans.AsQueryable().Select(bans => bans.AccountSQLID));
                if (linkedSQLList.Any())
                {
                    BanData _banInfo = await context.Bans.AsQueryable().FirstOrDefaultAsync(b => linkedSQLList.Contains(b.AccountSQLID));
                    if (_banInfo != null)
                    {
                        Account _accData = await context.Accounts.AsQueryable().FirstOrDefaultAsync(a => a.SQL_ID == _banInfo.AccountSQLID);
                        if (_accData != null)
                        {
                            NAPI.Task.Run(() =>
                            {
                                EPlayer.SetDiscordStatus(player, "Gazing at the 'BANNED' screen.");

                                player.SendChatMessage($"!{{#FF6347}}[BANNED] You are not allowed to play on this server.");

                                EPlayer.Notify(player, $"You are banned from the server.<p>Please appeal on our forums (www.racenchase.com) or consult on discord server at discord.gg/y3g2WuR</p><p>Use your account name as a reference.</p>", "red", "fas fa-ban", true, 30000);

                                player.SendChatMessage("!{#A9C4E4}Server closed the connection.");

                                EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}ANTI SUS SYSTEM (ASS) has kicked {acc.Username} for ban evading/outstanding bans. (Banned Account: {_accData.Username})");
                                NAPI.Util.ConsoleOutput($"[BAN SYSTEM] Kicking on login: {player.Name} (account name) as they're banned.");

                                Logs.AddToAdminRecord(_accData.SQL_ID, -1, "Ban Evading", $"Attempted to ban evade under username: {acc.Username}", "Permanent");

                                player.SetData("Kicked", true);
                                NAPI.Task.Run(() => { try { player.Kick(); } catch { } }, 1000);
                            });
                        }
                        else
                        {
                            banEvadingAccountNotFound = true;
                        }
                    }
                    else banEvadingAccountNotFound = true;

                    if (banEvadingAccountNotFound)
                    {
                        NAPI.Task.Run(() =>
                        {
                            EPlayer.SetDiscordStatus(player, "Gazing at the 'BANNED' screen.");

                            player.SendChatMessage($"!{{#FF6347}}[BANNED] You are not allowed to play on this server.");

                            EPlayer.Notify(player, $"You are banned from the server.<p>Please appeal on our forums (www.racenchase.com) or consult on discord server at discord.gg/y3g2WuR</p><p>Use your account name as a reference.</p>", "red", "fas fa-ban", true, 30000);

                            player.SendChatMessage("!{#A9C4E4}Server closed the connection.");

                            EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}ANTI SUS SYSTEM (ASS) has kicked {acc.Username} for ban evading/outstanding bans.");
                            NAPI.Util.ConsoleOutput($"[BAN SYSTEM] Kicking on login: {player.Name} (account name) as they're banned.");


                            player.SetData("Kicked", true);
                            NAPI.Task.Run(() => { try { player.Kick(); } catch { } }, 1000);
                        });
                    }
                }
            }
           
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

        public static Account CreateNewAccount(Player player, string password, string email, string username)
        {
            player.SetData("LastCryptAttempt", Environment.TickCount64);
            string salt = AccountManager.GenSalt();
            string newPass = AccountManager.Crypt(password, salt);

            Dictionary<long, wComponentData> wepItems = new Dictionary<long, wComponentData>();

            wComponentData test = new wComponentData();
            test.Components.Add(123456789);
            wepItems.Add(123456789, test);

            DiscordData _discordData = new DiscordData();
            string jsonDiscordData = NAPI.Util.ToJson(_discordData);

            List<ChallengeData> challengeData = new List<ChallengeData>();
            string jsonChallengeData = NAPI.Util.ToJson(challengeData);

            var _new = new Account
            {
                Email = email,
                Password = newPass,
                OldPassword = "",
                OldSalt = "",
                ForcePasswordChange = false,
                Salt = salt,
                Username = username,
                DiscordCode = _discordData.DiscordCode,
                DiscordID = null,
                UsedDiscordPerks = false,
                ChallengeInfo = jsonChallengeData,
                AutoLogin = true,
                Admin = 0,
                WalkstyleClipset = "null",
                RegisterHWID = player.Serial,
                RegisterIP = player.Address,
                RegisterRID = player.SocialClubId,
                RegisterSC = player.SocialClubName,
                LastHWID = player.Serial,
                LastIP = player.Address,
                LastRID = player.SocialClubId,
                LastSC = player.SocialClubName,
                MuteTime = 0,
                MuteReason = "N/A",
                JailTime = 0,
                JailReason = "N/A",
                CopchaseBanReason = "N/A",
                CopchaseBanTime = 0,
                DeathmatchBanReason = "N/A",
                DeathmatchBanTime = 0,
                FreeroamBanReason = "N/A",
                FreeroamBanTime = 0,
                DerbyWins = 0,
                FinishedTutorial = false,
                Level = 1,
                Medkits = 0,
                Painkillers = 0,
                Language = -1,
                DonatorStatus = true,
                XP = 1,
                Escapes = 0,
                Arrests = 0,
                Kills = 1,
                Lockpicks = 0,
                Deaths = 1,
                MinutesPlayed = 1.0,
                LastSessionMins = 0.0,
                DonatorPoints = 0,
                Points = 10,
                Warns = 0,
                Suspended = 0,
                DonatorLevel = 0,
                LobbyClothing = 1,
                LoadoutCop = "?",
                LoadoutFugitive = "?",
                CarCop = "?",
                CarFugitive = "?",
                WeaponComponents = NAPI.Util.ToJson(wepItems),
                MOTD = "none",
                RegisterDate = DateTime.Now,
                LastLogin = DateTime.Now,
                Character = "{}",
                PurchasedAmmo = "{}",
                ChallengeStreak = 0,
                CompletedDailyChallenge = false,
                Namechanges = 0,
                ClanTier = -1,
                ClanID = -1,
                CrosshairProfiles = "[]",
                ActiveCrosshairProfile = "{}",
                TattooList = "[]",
                SelectedBadgeData = "{id:0, name:\"\"}",
                EmailVerified = false
            };
            return _new;
        }

        public static bool MatchPass(string plaintext, string hashed) 
        {
            bool isGood = BCrypt.Net.BCrypt.Verify(plaintext, hashed);
            return isGood;
        }

        public async static Task<bool> VerifyEmail(int accountSQLID, string code)
        {
            NAPI.Util.ConsoleOutput($"[VERIFY] Verifying email for account {accountSQLID} code is {code}");
            using(MainDatabase context = ContextFactory.CreateContext())
            {
                EmailVerificationRequest _e = await context.EmailVerificationRequests.AsQueryable().FirstOrDefaultAsync(e => e.AccountID == accountSQLID && e.Code == code);
                if (_e == null) return false;

                if(_e.Code.Equals(code, StringComparison.OrdinalIgnoreCase))
                {
                    List<EmailVerificationRequest> allRequests = await context.EmailVerificationRequests.AsQueryable().Where(e => e.AccountID == accountSQLID).ToListAsync();

                    // If code is correct, remove the verification request entity from DB
                    // And change their EmailVerified variable to true
                    context.EmailVerificationRequests.RemoveRange(allRequests);
                    Account _a = await context.Accounts.AsQueryable().FirstOrDefaultAsync(acc => acc.SQL_ID == accountSQLID);
                    _a.EmailVerified = true;
                    NAPI.Util.ConsoleOutput($"[VERIFY] {_a.Username} set verified to TRUE!");
                    await context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
        }
        public async static Task SendVerificationEmail(Account _a)
        {
            string code = Main.GenerateRandomCode(5, false);
            using(MainDatabase context = ContextFactory.CreateContext())
            {
                EmailVerificationRequest _e = new EmailVerificationRequest()
                {
                    AccountID = _a.SQL_ID,
                    Code = code,
                    CreatedAt = DateTime.Now
                };
                await context.EmailVerificationRequests.AddAsync(_e);
                await context.SaveChangesAsync();

                EmailController.SendEmail(_a.Email, "Race 'N' Chase - Email Verification", 
                    RNC.EmailTemplates.EmailVerification.HTML
                    .Replace("{{name}}", _a.Username)
                    .Replace("{{code}}", code), 
                    true);
            }
        }
        public async static Task Add(Account _a)
        {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    await context.Accounts.AddAsync(_a);
                    await context.SaveChangesAsync();
                }
        }
        public static bool Login(Account _target, string password, bool _crypted = false, bool _useOldPassword = false)
        {
            if (_useOldPassword)
            {
                return false;
                //foreach (Player player in Settings.LoggedInPlayers)
                //{
                //    if (EPlayer.GetAccount(player) != null && EPlayer.GetAccount(player).SQL_ID == _target.SQL_ID) return false; // Return FALSE if another PLAYER is online from SAME ACC.
                //}
                //
                //if (_target.Suspended == 1 && _target.Admin < 4) return false;
                //
                //if (MatchPass(password, _target.OldPassword))
                //{
                //    return true;
                //}
                //return false;
            }
            else
            {
                foreach (Player player in Settings.LoggedInPlayers)
                {
                    if (EPlayer.GetAccount(player) != null && EPlayer.GetAccount(player).SQL_ID == _target.SQL_ID) return false; // Return FALSE if another PLAYER is online from SAME ACC.
                }

                if (_target.Suspended == 1 && _target.Admin < 4) return false;

                if (MatchPass(password, _target.Password))
                {
                    return true;
                }
                return false;
            }

        }
    }
}
