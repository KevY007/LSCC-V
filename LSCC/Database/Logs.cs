using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;
using GTANetworkAPI;
using System.Collections.Generic;
using System.Linq;
using LSCC;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net.Mail;

namespace LSCC
{
    public class Logs_Admin
    {
        [Key]
        public int SQL_ID { get; set; }
        public int AccountID { get; set; }
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }
    }
    public class Logs_AdminRecord
    {
        [Key]
        public int SQL_ID { get; set; }
        public int AccountID { get; set; }
        public int AdminID { get; set; }
        public string Type { get; set; }
        public string Reason { get; set; }
        public DateTime Timestamp { get; set; }
        public string Duration { get; set; }
    }
    public class Logs_Session
    {
        [Key]
        public int SQL_ID { get; set; }
        public int AccountID { get; set; }
        public DateTime TimestampJoin { get; set; }
        public DateTime TimestampLeave { get; set; }
        public string Duration { get; set; }
        public string IP { get; set; }
        public string HWID { get; set; }
    }
    public class Logs : Script
    {
        enum LogType
        {
            Admin = 1,
            Session = 2,
            AdminRecord = 3
        };

        [RemoteEvent("ShowLogsPanel")]
        public static void ShowLogsPanel(Player player, bool toggle)
        {
            player.TriggerEvent("ShowLogsCEF", toggle);
        }

        [RemoteEvent("Logs_GetAutoCompleteNames")]
        public static void GetAutoCompleteNames(Player player, string query)
        {
            List<Account> foundaccs = ContextFactory.Instance.Accounts.AsQueryable().Where(t => t.Username.Contains(query, StringComparison.OrdinalIgnoreCase)).Take(5).ToList();

            string jsonShit = "[";
            foreach(Account acc in foundaccs)
            {
                jsonShit += $"{{name: '{acc.Username}'}},";
            }
            jsonShit += "]";

            player.TriggerEvent("ReturnAutoCompleteNames", jsonShit);
        }
        [RemoteEvent("ReqLogFromSrv")]
        public static void RequestLogsFromServer(Player player, string targetName, int type)
        {
            player.SendChatMessage("REQUESTED LOGS FOR " + targetName + " TYPE " + type);

            Account target = ContextFactory.Instance.Accounts.FirstOrDefault<Account>(a => a.Username.Equals(targetName, StringComparison.OrdinalIgnoreCase));
            if (target == null)
            {
                return;
            }
            if (target.Admin >= EPlayer.GetAccount(player).Admin && target != EPlayer.GetAccount(player))
            {
                player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Nice try - that admin is a higher or equal level as you!");
                return;
            }

            switch (type)
            {
                case (int)LogType.AdminRecord:
                    {
                        List<Logs_AdminRecord> _logs = ContextFactory.Instance.Logs_AdminRecords.AsQueryable().Where(l => l.AccountID == target.SQL_ID).ToList();
                        string jsonShit = NAPI.Util.ToJson(_logs);
                        NAPI.Util.ConsoleOutput(jsonShit);
                        player.TriggerEvent("ReturnLogsToClient", type, jsonShit);
                        break;
                    }
                case (int)LogType.Admin:
                    {
                        List<Logs_Admin> _logs = ContextFactory.Instance.Logs_Admin.AsQueryable().Where(l => l.AccountID == target.SQL_ID).ToList();
                        string jsonShit = NAPI.Util.ToJson(_logs);
                        NAPI.Util.ConsoleOutput(jsonShit);
                        player.TriggerEvent("ReturnLogsToClient", type, jsonShit);
                        break;
                    }
                case (int)LogType.Session:
                    {
                        List<Logs_Session> _logs = ContextFactory.Instance.Logs_Session.AsQueryable().Where(l => l.AccountID == target.SQL_ID).ToList();
                        string jsonShit = NAPI.Util.ToJson(_logs);
                        NAPI.Util.ConsoleOutput(jsonShit);
                        player.TriggerEvent("ReturnLogsToClient", type, jsonShit);
                        break;
                    }
                default:
                    {
                        return;
                    }
            }
        }

            /*
            [RemoteEvent("ReqLogFromSrv")]
            public static void RequestLogsFromServer(Player player, string targetName, int type)
            {
                Account target = ContextFactory.Instance.Accounts.FirstOrDefault<Account>(a => a.Username.Equals(targetName, StringComparison.OrdinalIgnoreCase));
                if (target == null)
                {
                    return;
                }
                if(target.Admin >= EPlayer.GetAccount(player).Admin)
                {
                    player.SendChatMessage("!{#FF6347}[!] !{#FFFFFF}Nice try - that admin is a higher or equal level as you!");
                    return;
                }

                switch(type)
                {
                    case (int)LogType.AdminRecord:
                        {
                            List<Logs_AdminRecord> _logs = ContextFactory.Instance.Logs_AdminRecords.AsQueryable().Where(l => l.AccountID == target.SQL_ID).ToList();
                            object[] _toSend = new object[_logs.Count];
                            for (int i = 0; i < _logs.Count; i++)
                            {
                                (string, string, string, string) _toAdd = (_logs[i].Type, _logs[i].Reason, _logs[i].Timestamp.ToString(), _logs[i].Duration);
                                _toSend[i] = _toAdd;
                            }
                            string jsonShit = NAPI.Util.ToJson(_toSend);
                            player.TriggerEvent("ReturnLogsToClient", type, jsonShit);
                            break;
                        }
                    case (int)LogType.Admin:
                        {
                            List<Logs_Admin> _logs = ContextFactory.Instance.Logs_Admin.AsQueryable().Where(l => l.AccountID == target.SQL_ID).ToList();
                            object[] _toSend = new object[_logs.Count];
                            for (int i = 0; i < _logs.Count; i++)
                            {
                                (string, string) _toAdd = (_logs[i].Description, _logs[i].Timestamp.ToString());
                                _toSend[i] = _toAdd;
                            }
                            string jsonShit = NAPI.Util.ToJson(_toSend);
                            player.TriggerEvent("ReturnLogsToClient", type, jsonShit);
                            break;
                        }
                    case (int)LogType.Session:
                        {
                            List<Logs_Session> _logs = ContextFactory.Instance.Logs_Session.AsQueryable().Where(l => l.AccountID == target.SQL_ID).ToList();
                            object[] _toSend = new object[_logs.Count];
                            for (int i = 0; i < _logs.Count; i++)
                            {
                                (string, string, string, string, string) _toAdd = (_logs[i].TimestampJoin.ToString(), _logs[i].TimestampLeave.ToString(), _logs[i].Duration, _logs[i].IP, _logs[i].HWID);
                                _toSend[i] = _toAdd;
                            }
                            string jsonShit = NAPI.Util.ToJson(_toSend);
                            player.TriggerEvent("ReturnLogsToClient", type, jsonShit);
                            break;
                        }
                    default:
                        {
                            return;
                        }
                }
            }*/

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
                TimestampJoin = new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime().AddSeconds(_joinUnix),
                TimestampLeave = new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime().AddSeconds(_currentUnix),
                Duration = _duration,
                IP = player.Address,
                HWID = player.Serial
            };
            ContextFactory.Instance.Logs_Session.Add(_a);
            ContextFactory.Instance.SaveChanges();
        }
        public static void LogAdmin(Player player, string description)
        {
            description = description.Replace("{", "(");
            description = description.Replace("}", ")");

            Logs_Admin _a = new Logs_Admin()
            {
                AccountID = EPlayer.GetAccount(player).SQL_ID,
                Description = description,
                Timestamp = DateTime.Now
            };
            ContextFactory.Instance.Logs_Admin.Add(_a);
            ContextFactory.Instance.SaveChanges();
        }

        public static void RemoveAdminRecord(int recordID)
        {
            Logs_AdminRecord _rem = ContextFactory.Instance.Logs_AdminRecords.AsQueryable().FirstOrDefault(l => l.SQL_ID == recordID);
            if (_rem == null) return;

            ContextFactory.Instance.Logs_AdminRecords.Remove(_rem);
            ContextFactory.Instance.SaveChanges();
        }
        public static void AddToAdminRecord(int accountID, int adminID, string type, string reason, string duration)
        {
            reason = reason.Replace("{", "(");
            reason = reason.Replace("}", ")");

            Logs_AdminRecord _a = new Logs_AdminRecord()
            {
                AccountID = accountID,
                AdminID = adminID,
                Type = type,
                Reason = reason,
                Timestamp = DateTime.Now,
                Duration = duration
            };
            ContextFactory.Instance.Logs_AdminRecords.Add(_a);
            ContextFactory.Instance.SaveChanges();
        }
    }
}
