using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;
using GTANetworkAPI;
using System.Collections.Generic;
using System.Linq;
using RNC;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net.Mail;
using System.Threading.Tasks;

namespace RNC
{
    public class Logs_Admin
    {
        [Key]
        public int SQL_ID { get; set; }
        public int AccountID { get; set; }
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }
    }
    public class Logs_Damage
    {
        [Key]
        public int SQL_ID { get; set; }
        public DateTime Timestamp { get; set; }
        public int AccountID { get; set; }
        public int DealerID { get; set; } // who dealt the dammage
        public long Weapon { get; set; }
        public int Damage { get; set; }
        public int Health { get; set; } // HP after getting damaged
        public int Bodypart { get; set; }
        public string Notes { get; set; }
        public int Armor { get; set; } // Armor after getting damaged
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
            AdminRecord = 3,
            IncomingDamage = 4,
            OutgoingDamage = 5,
        };

        [RemoteEvent("Logs_GetAutoCompleteNames")]
        public static void GetAutoCompleteNames(Player player, string query)
        {
            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    List<Account> accountsList = await context.Accounts.AsQueryable().ToListAsync();
                    List<Account> foundaccs = (List<Account>)accountsList.Where(t => t.Username.Contains(query, StringComparison.OrdinalIgnoreCase)).Take(5);

                    string jsonShit = "[";
                    foreach (Account acc in foundaccs)
                    {
                        jsonShit += $"{{name: '{acc.Username}'}},";
                    }
                    jsonShit += "]";

                    NAPI.Task.Run(() =>
                    {
                        player.TriggerEvent("ReturnAutoCompleteNames", jsonShit);
                    });
                }
            });

        }
        /*
        [RemoteEvent("ReqLogFromSrv")]
        public static void RequestLogsFromServer(Player player, string targetName, int type)
        {
            player.SendChatMessage("REQUESTED LOGS FOR " + targetName + " TYPE " + type);

            Account target = ContextFactory.Instance.Accounts.ToList().Find(a => a.Username.Equals(targetName, StringComparison.OrdinalIgnoreCase));
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
        */ // Ignore this shit, old logs



        [RemoteEvent("ReqLogFromSrv")]
        public static void RequestLogsFromServer(Player player, string targetName, int type)
        {
            if (targetName == "")
            {
                EPlayer.Notify(player, $"You must enter a player name! (nice try)",
                    "red", "fas fa-hand-middle-finger", false, 1500, "topCenter", "replace");
                player.TriggerEvent("Menu_SetLogsTabLoading", false);
                return;
            }
            int pAdmin = EPlayer.GetAccount(player).Admin;
            int pSQL = EPlayer.GetAccount(player).SQL_ID;
            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    Account target = await context.Accounts.AsQueryable().FirstOrDefaultAsync(a => a.Username.Equals(targetName, StringComparison.OrdinalIgnoreCase));
                    if (target == null)
                    {
                        NAPI.Task.Run(() =>
                        {
                            EPlayer.Notify(player, $"There is no player with that name in the database!",
                            "red", "fas fa-exclamation-triangle", false, 1500, "topCenter", "replace");
                            player.TriggerEvent("Menu_SetLogsTabLoading", false);
                        });
                        return;
                    }
                    if (target.Admin >= pAdmin && target.SQL_ID != pSQL)
                    {
                        NAPI.Task.Run(() =>
                        {
                            EPlayer.Notify(player, $"That player is a higher or equal admin level than you!",
                            "red", "fas fa-ban", false, 1500, "topCenter", "replace");
                            player.TriggerEvent("Menu_SetLogsTabLoading", false);
                        });
                        return;
                    }

                    switch (type)
                    {
                        case (int)LogType.AdminRecord:
                            {
                                List<Logs_AdminRecord> _logs = await context.Logs_AdminRecords.AsQueryable().Where(l => l.AccountID == target.SQL_ID).ToListAsync();

                                object[] _toSend = new object[_logs.Count];
                                for (int i = 0; i < _logs.Count; i++)
                                {
                                    Account acc = await context.Accounts.AsQueryable().FirstOrDefaultAsync(a => a.SQL_ID == _logs[i].AdminID);
                                    string aName = acc.Username;
                                    if (aName == "null") aName = "UNKNOWN";

                                    (string, string, string, string) _toAdd = (_logs[i].Timestamp.ToString(), aName, _logs[i].Duration, _logs[i].Reason);
                                    _toSend[i] = _toAdd;
                                }
                                NAPI.Task.Run(() =>
                                {
                                    string jsonShit = NAPI.Util.ToJson(_toSend);
                                    player.TriggerEvent("ReturnLogsToClient", type, jsonShit);
                                });
                                break;
                            }
                        case (int)LogType.Admin:
                            {

                                List<Logs_Admin> _logs = await context.Logs_Admin.AsQueryable().Where(l => l.AccountID == target.SQL_ID).ToListAsync();
                                object[] _toSend = new object[_logs.Count];
                                for (int i = 0; i < _logs.Count; i++)
                                {
                                    (string, string) _toAdd = (_logs[i].Timestamp.ToString(), _logs[i].Description);
                                    _toSend[i] = _toAdd;
                                }
                                NAPI.Task.Run(() =>
                                {
                                    string jsonShit = NAPI.Util.ToJson(_toSend);
                                    player.TriggerEvent("ReturnLogsToClient", type, jsonShit);
                                });

                                break;
                            }
                        case (int)LogType.Session:
                            {

                                List<Logs_Session> _logs = await context.Logs_Session.AsQueryable().Where(l => l.AccountID == target.SQL_ID).ToListAsync();
                                object[] _toSend = new object[_logs.Count];
                                for (int i = 0; i < _logs.Count; i++)
                                {
                                    (string, string, string, string, string) _toAdd = (_logs[i].TimestampJoin.ToString(), _logs[i].TimestampLeave.ToString(), _logs[i].Duration, _logs[i].IP, _logs[i].HWID);
                                    _toSend[i] = _toAdd;
                                }
                                NAPI.Task.Run(() =>
                                {
                                    string jsonShit = NAPI.Util.ToJson(_toSend);
                                    player.TriggerEvent("ReturnLogsToClient", type, jsonShit);
                                });
                                break;
                            }

                        case (int)LogType.IncomingDamage:
                            {
                                List<Logs_Damage> _logs = await context.Logs_Damages.AsQueryable().Where(l => l.AccountID == target.SQL_ID).ToListAsync();
                                object[] _toSend = new object[_logs.Count];
                                for (int i = 0; i < (_logs.Count > 250 ? 250 : _logs.Count); i++)
                                {
                                    Account dealer = await context.Accounts.AsQueryable().FirstOrDefaultAsync(a => a.SQL_ID == _logs[i].DealerID);
                                    string dealerName = (dealer == null ? "[UNKNOWN]" : dealer.Username);
                                    (string, string, long, string, string, int, int) _toAdd = (_logs[i].Timestamp.ToString(), dealerName, _logs[i].Weapon, _logs[i].Damage.ToString(), _logs[i].Health.ToString(), _logs[i].Bodypart, _logs[i].Armor);
                                    _toSend[i] = _toAdd;
                                }
                                NAPI.Task.Run(() =>
                                {
                                    string jsonShit = NAPI.Util.ToJson(_toSend);
                                    player.TriggerEvent("ReturnLogsToClient", type, jsonShit);
                                });
                                break;
                            }
                        case (int)LogType.OutgoingDamage:
                            {
                                List<Logs_Damage> _logs = await context.Logs_Damages.AsQueryable().Where(l => l.DealerID == target.SQL_ID).ToListAsync();
                                object[] _toSend = new object[_logs.Count];
                                for (int i = 0; i < (_logs.Count > 250 ? 250 : _logs.Count); i++)
                                {
                                    Account victim = await context.Accounts.AsQueryable().FirstOrDefaultAsync(a => a.SQL_ID == _logs[i].AccountID);
                                    string victimName = (victim == null ? "[UNKNOWN]" : victim.Username);
                                    (string, string, long, string, string, int, int) _toAdd = (_logs[i].Timestamp.ToString(), victimName, _logs[i].Weapon, _logs[i].Damage.ToString(), _logs[i].Health.ToString(), _logs[i].Bodypart, _logs[i].Armor);
                                    _toSend[i] = _toAdd;
                                }
                                NAPI.Task.Run(() =>
                                {
                                    string jsonShit = NAPI.Util.ToJson(_toSend);
                                    player.TriggerEvent("ReturnLogsToClient", type, jsonShit);
                                });
                                break;
                            }
                        default:
                            {
                                return;
                            }
                    }
                }
            });
        }

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
                HWID = player.Serial.Substring(0, 50)
            };

            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    await context.Logs_Session.AddAsync(_a);
                    await context.SaveChangesAsync();
                }
            });
        }
        public static void LogAdmin(Player player, string description)
        {
            LogAdmin(EPlayer.GetAccount(player).SQL_ID, description);
        }
        public static void LogDamage(int accSQLID, int dealerSQLID, long weapon, int damage, int health, int bodypart, string notes, int armor)
        {
            Task.Run(async () =>
            {
                Logs_Damage _d = new Logs_Damage()
                {
                    Timestamp = DateTime.Now,
                    AccountID = accSQLID,
                    DealerID = dealerSQLID,
                    Weapon = weapon,
                    Damage = damage,
                    Health = health,
                    Bodypart = bodypart,
                    Notes = notes,
                    Armor = armor
                };

                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    await context.Logs_Damages.AddAsync(_d);
                    await context.SaveChangesAsync();
                }
            });
        }
        public static void LogAdmin(int aSQLID, string description)
        {
            description = description.Replace("{", "(");
            description = description.Replace("}", ")");

            Logs_Admin _a = new Logs_Admin()
            {
                AccountID = aSQLID,
                Description = description,
                Timestamp = DateTime.Now
            };
            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    await context.Logs_Admin.AddAsync(_a);
                    await context.SaveChangesAsync();
                }
            });
        }

        public static void RemoveAdminRecord(int recordID)
        {
            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    Logs_AdminRecord _rem = await context.Logs_AdminRecords.AsQueryable().FirstOrDefaultAsync(l => l.SQL_ID == recordID);
                    if (_rem == null) return;

                    context.Logs_AdminRecords.Remove(_rem);
                    await context.SaveChangesAsync();
                }
            });
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
            Task.Run(async () =>
            {
                using (MainDatabase context = ContextFactory.CreateContext())
                {
                    await context.Logs_AdminRecords.AddAsync(_a);
                    await context.SaveChangesAsync();
                }
            });
        }
    }
}
