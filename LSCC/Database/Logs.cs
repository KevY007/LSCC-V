using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;
using GTANetworkAPI;

namespace LSCC
{
    public class Logs_Admin
    {
        [Key]
        public int SQL_ID { get; set; }
        public int AccountID { get; set; }
        public string Description { get; set; }
        public string Timestamp { get; set; }
    }
    public class Logs_AdminRecord
    {
        [Key]
        public int SQL_ID { get; set; }
        public int AccountID { get; set; }
        public int AdminID { get; set; }
        public string Type { get; set; }
        public string Reason { get; set; }
        public string Timestamp { get; set; }
        public string Duration { get; set; }
    }
    public class Logs_Session
    {
        [Key]
        public int SQL_ID { get; set; }
        public int AccountID { get; set; }
        public string TimestampJoin { get; set; }
        public string TimestampLeave { get; set; }
        public string Duration { get; set; }
        public string IP { get; set; }
        public string HWID { get; set; }
    }
    public static class Logs
    {
        public static void LogSession(Player player)
        {
            if (!player.HasData("JoinTimeUnix")) return;

            long _currentUnix = DateTimeOffset.Now.ToUnixTimeSeconds();
            long _joinUnix = player.GetData<long>("JoinTimeUnix");
            long _durationSeconds = _currentUnix - _joinUnix;

            TimeSpan _span = TimeSpan.FromSeconds(_durationSeconds);
            string _duration = _span.ToString(@"hh\:mm\:ss");

            Logs_Session _a = new Logs_Session()
            {
                AccountID = EPlayer.GetAccount(player).SQL_ID,
                TimestampJoin = new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime().AddSeconds(_joinUnix).ToString(),
                TimestampLeave = new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime().AddSeconds(_currentUnix).ToString(),
                Duration = _duration,
                IP = player.Address,
                HWID = player.Serial
            };
            ContextFactory.Instance.Logs_Session.Add(_a);
            ContextFactory.Instance.SaveChangesAsync();
        }
        public static void LogAdmin(Player player, string description)
        {
            Logs_Admin _a = new Logs_Admin()
            {
                AccountID = EPlayer.GetAccount(player).SQL_ID,
                Description = description,
                Timestamp = DateTime.Now.ToString()
            };
            ContextFactory.Instance.Logs_Admin.Add(_a);
            ContextFactory.Instance.SaveChangesAsync();
        }

        public async static void RemoveAdminRecord(int recordID)
        {
            Logs_AdminRecord _rem = await ContextFactory.Instance.Logs_AdminRecords.FirstOrDefaultAsync(t => t.SQL_ID == recordID);
            if (_rem == null) return;

            ContextFactory.Instance.Logs_AdminRecords.Remove(_rem);
            await ContextFactory.Instance.SaveChangesAsync();
        }
        public static void AddToAdminRecord(int accountID, int adminID, string type, string reason, string duration)
        {
            Logs_AdminRecord _a = new Logs_AdminRecord()
            {
                AccountID = accountID,
                AdminID = adminID,
                Type = type,
                Reason = reason,
                Timestamp = DateTime.Now.ToString(),
                Duration = duration
            };
            ContextFactory.Instance.Logs_AdminRecords.Add(_a);
            ContextFactory.Instance.SaveChangesAsync();
        }
    }
}
