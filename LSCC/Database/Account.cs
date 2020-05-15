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
        public int Admin { get; set; }
        public int DonatorLevel { get; set; }
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
