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
using LSCC;
using System.Security.Cryptography;
using GTANetworkAPI;

namespace LSCC
{
    public class Account
    {
        [Key]
        public int SQL_ID { get; set; }

        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
        public string HWID { get; set; }
        public ulong RockstarID { get; set; }
        public int Admin { get; set; }
        public int Level { get; set; }
        public int XP { get; set; }
        public int DonatorLevel { get; set; }
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
        public int Suspended { get; set; }
        public int LobbyClothing { get; set; }
        public string Character { get; set; }
        public string LoadoutCop { get; set; }
        public string LoadoutFugitive { get; set; }
        public string CarCop { get; set; }
        public string WeaponComponents { get; set; }
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
