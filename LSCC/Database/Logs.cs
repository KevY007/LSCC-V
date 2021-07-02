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
    public static class Logs
    {
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
