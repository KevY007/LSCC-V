using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RNC
{
    class Discord : Script
    {
        public static DiscordSocketClient _client;
        public static long UpCooldown { get; set; }
        public static void Main()
        => new Discord().MainAsync().GetAwaiter().GetResult();

        private Task Log(LogMessage msg)
        {
            if (!Settings.Dynamics.Discord) return Task.CompletedTask;
            Console.WriteLine("[DISCORD] " + msg.ToString());
            return Task.CompletedTask;
        }

        private Task Log(string msg)
        {
            if (!Settings.Dynamics.Discord) return Task.CompletedTask;
            Console.WriteLine("[DISCORD] " + msg);
            return Task.CompletedTask;
        }

        public async static Task<int> GetIngameSQLID(SocketUser user)
        {
            if (user == null)
            {
                NAPI.Task.Run(() => { NAPI.Util.ConsoleOutput($"[DISCORD] Attempted to get SQLID - User was null!"); });
                return -1;
            }
            if(user.Id == 922461597866295397 || user.Id == 1027945798069133443)
            {
                return 1; // if GM bot return KevY's ID, cba to make a new account for the bot now
            }

            using (MainDatabase context = ContextFactory.CreateContext())
            {
                Account _a = await context.Accounts.AsQueryable().FirstOrDefaultAsync(a => a.DiscordInfo.Contains(user.Id.ToString()));
                if (_a == null) return -1;
                return _a.SQL_ID;
            }
        }

        public static int GetIngameAdminLevel(SocketUser user)
        {
            if (user == null)
            {
                NAPI.Task.Run(() => { NAPI.Util.ConsoleOutput($"[DISCORD] Attempted to get SQLID - User was null!"); });
                return 0;
            }
            if(user.Id == 922461597866295397 || user.Id == 1027945798069133443) // if GM bot
            {
                return 6;
            }

            SocketGuildUser realUser = user as SocketGuildUser;

            int level = 0;
            var role = (realUser as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Id == 714822199881760798); // mod
            if (realUser.Roles.Contains(role))
            {
                level = 1;
            }
            role = (realUser as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Id == 714821592701599814); // trial
            if (realUser.Roles.Contains(role))
            {
                level = 2;
            }
            role = (realUser as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Id == 800001726043127848); // general
            if (realUser.Roles.Contains(role))
            {
                level = 3;
            }
            role = (realUser as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Id == 713734312335376465); // senior
            if (realUser.Roles.Contains(role))
            {
                level = 4;
            }
            role = (realUser as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Id == 886616979912421387); // lead
            if (realUser.Roles.Contains(role))
            {
                level = 5;
            }
            role = (realUser as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Id == 777641183512887316); // owner/management
            if (realUser.Roles.Contains(role))
            {
                level = 6;
            }
            role = (realUser as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Id == 713798856680276079); // owner/management
            if (realUser.Roles.Contains(role))
            {
                level = 6;
            }
            return level;
        }

        public static Task SendTextChannel(string channel, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null)
        {
            if (!Settings.Dynamics.Discord) return Task.CompletedTask;
            if (_client == null) return Task.CompletedTask;
            if (_client.ConnectionState != ConnectionState.Connected) return Task.CompletedTask;

            if (text != null)
            {
                text = "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + text;

                text = Regex.Replace(text, @"\!\{(.*?)}|\!\{(.*?)}", "");
            }

            foreach (var guild in Discord._client.Guilds.Where(a => a.Id == Settings.Dynamics.DCServerID))
            {
                var botchan = guild.TextChannels.First(a => a.Id == channel);

                if (botchan != null)
                {
                    botchan.SendMessageAsync(text, isTTS, embed, options);
                }
            }
            return Task.CompletedTask;
        }

        public static async Task MessageReceived(SocketMessage context)
        {
            try
            {
                if (!Settings.Dynamics.Discord) return;
                if (_client == null) return;
                if (_client.ConnectionState != ConnectionState.Connected) return;

                /*if(context.Content.Equals("boombaamclap") && context.Author.Username.Contains("KevY", StringComparison.OrdinalIgnoreCase))
                {
                    var msgs = context.Channel.GetMessagesAsync(context.Id, Direction.Before).FlattenAsync();
                    var loop = msgs.Result.ToList();
                    int count = 0;
                    Console.Write($"Taking deleting command from {context.Author.Username.ToString()}");
                    await context.DeleteAsync();
                    await Task.Delay(1500);
                    Console.Write($" | Now beginning!" + Environment.NewLine);
                    foreach(var msg in loop)
                    {
                        if(msg.Content.StartsWith("!up") || msg.Content.Contains("Server is online with"))
                        {
                            count++;
                            await msg .DeleteAsync();
                            Console.WriteLine($"Deleting message from {msg.Author.Username.ToString()} | Num: {count}");
                            await Task.Delay(1000);
                        }
                    }
                }*/

                if (context.Content.Equals("!up") || context.Content.Equals("!status"))
                {
                    if (Environment.TickCount64 - UpCooldown <= 7000) return;

                    await context.DeleteAsync();
                    await Task.Run(async () =>
                    { // WE WANT THE CODE TO EXECUTE SEPARATELY AND NOT BLOCK MESSAGE RECEIVED SO DONT AWAIT THIS SH
                        var msg = await context.Channel.SendMessageAsync($":white_check_mark: Server is online with **{RAGE.Entities.Players.Count} players** and **{Settings.Lobbies.Where(l => l.gameStatus == 1).Count()} copchases** active.", false, null, null, null);
                        await Task.Delay(5000);
                        await msg.DeleteAsync();
                    });

                    UpCooldown = Environment.TickCount64;

                    return;
                }

                if (context.Channel.Name != "staff-bot")
                {
                    // allow GM bot to write anywhere
                    if (context.Author.Id != 1027945798069133443 && context.Author.Id != 922461597866295397 && context.Author.Id != 253938879554387969
                        && context.Author.Id != 367360197900894209) return;
                }

                if (context.Content.StartsWith("!dbgtimecheck"))
                {
                    int aLevel = GetIngameAdminLevel(context.Author);
                    if (aLevel < 6)
                    {
                        await context.Channel.SendMessageAsync("https://cms.qz.com/wp-content/uploads/2018/06/new-pride-flag-01.jpg");
                        return;
                    }
                    NAPI.Task.Run(() =>
                    {
                        Events.RunWeeklyMonthlyChecks();
                        context.Channel.SendMessageAsync("Done!");
                    }, 50);
                }
                else if (context.Content.StartsWith("!lookup"))
                {
                    string[] args = context.Content.Split(' ');
                    if (args.Count() >= 3)
                    {
                        string type = args[1];
                        string value = args[2];

                        if(type != "email" && type != "rockstarname")
                        {
                            await context.Channel.SendMessageAsync("**[!]** Invalid type! You must use either `email` or `rockstarname` as the type.");
                            return;
                        }

                        int aLevel = GetIngameAdminLevel(context.Author);

                        int aSQLID = await GetIngameSQLID(context.Author);

                        if (aSQLID == -1)
                        {
                            await context.Channel.SendMessageAsync("**[!]** You must link your Discord & RnC account to use this!");
                            return;
                        }
                        if (aLevel < 2)
                        {
                            await context.Channel.SendMessageAsync("**[!]** You must be a level 2+ admin to use this!");
                            return;
                        }

                        if(type == "email")
                        {
                            using(MainDatabase dbcontext = ContextFactory.CreateContext())
                            {
                                List<Account> accounts = await dbcontext.Accounts.AsQueryable().Where(a => a.Email.Equals(value, StringComparison.OrdinalIgnoreCase)).ToListAsync();
                                if (accounts.Count > 0)
                                {
                                    var builder = new EmbedBuilder()
                                   .WithTitle("**SQL ID | Username | Last Login**")
                                   .WithDescription("```\n```")
                                   .WithColor(255, 0, 0)
                                   .WithFooter(footer =>
                                   {
                                       footer
                                           .WithText("Lookup results");
                                   })
                                   .WithAuthor(author =>
                                   {
                                       author
                                           .WithName("Accounts list")
                                           .WithIconUrl("https://i.imgur.com/pc6pzkq.png");
                                   });

                                    int proc = 0;
                                    foreach (Account acc in accounts)
                                    {
                                        proc++;
                                        builder.AddField($"{acc.SQL_ID} | {acc.Username} | {acc.LastLogin.ToString("F")}", true);

                                        if (proc > 10) break;
                                    }

                                    var embed = builder.Build();

                                    await context.Channel.SendMessageAsync(
                                        "Reporting...",
                                        embed: embed)
                                        .ConfigureAwait(false);
                                }
                                else await context.Channel.SendMessageAsync(":negative_squared_cross_mark: No accounts found linked to that e-mail! *Try asking the user for their Rockstar Social Club username...*");
                            }
                        }
                        else if (type == "rockstarname")
                        {
                            using (MainDatabase dbcontext = ContextFactory.CreateContext())
                            {
                                List<Account> accounts = await dbcontext.Accounts.AsQueryable().Where(a => a.RegisterSC.Equals(value, StringComparison.OrdinalIgnoreCase) || a.LastSC.Equals(value, StringComparison.OrdinalIgnoreCase)).ToListAsync();
                                if (accounts.Count > 0)
                                {
                                    var builder = new EmbedBuilder()
                                   .WithTitle("**SQL ID | Username | Last Login**")
                                   .WithDescription("```\n```")
                                   .WithColor(255, 0, 0)
                                   .WithFooter(footer =>
                                   {
                                       footer
                                           .WithText("Lookup results");
                                   })
                                   .WithAuthor(author =>
                                   {
                                       author
                                           .WithName("Accounts list")
                                           .WithIconUrl("https://i.imgur.com/pc6pzkq.png");
                                   });

                                    int proc = 0;
                                    foreach (Account acc in accounts)
                                    {
                                        proc++;
                                        builder.AddField($"{acc.SQL_ID} | {acc.Username} | {acc.LastLogin.ToString("F")}", true);

                                        if (proc > 10) break;
                                    }

                                    var embed = builder.Build();

                                    await context.Channel.SendMessageAsync(
                                        "Reporting...",
                                        embed: embed)
                                        .ConfigureAwait(false);
                                }
                                else await context.Channel.SendMessageAsync(":negative_squared_cross_mark: No accounts found linked to that Rockstar username! *Try asking the user for their e-mail...*");
                            }
                        }
                    }
                    else await context.Channel.SendMessageAsync("USAGE: !lookup (type: email|rockstarname) (value)");
                }
                else if (context.Content.StartsWith("!changepass"))
                {
                    string[] args = context.Content.Split(' ');
                    if (args.Count() >= 3)
                    {
                        string name = args[1];
                        string newpass = args[2];

                        int aLevel = GetIngameAdminLevel(context.Author);

                        int aSQLID = await GetIngameSQLID(context.Author);
                        if (context.Author.Id == 1027945798069133443 || context.Author.Id == 922461597866295397) aSQLID = 1;

                        if (aSQLID == -1)
                        {
                            await context.Channel.SendMessageAsync("**[!]** You must link your Discord & RnC account to use this!");
                            return;
                        }
                        if (aLevel < 2)
                        {
                            await context.Channel.SendMessageAsync("**[!]** You must be a level 2+ admin to use this!");
                            return;
                        }
                        using (MainDatabase dbcontext = ContextFactory.CreateContext())
                        {
                            Account tpAcc = await dbcontext.Accounts.AsQueryable().FirstOrDefaultAsync(p => p.Username.Equals(name, StringComparison.OrdinalIgnoreCase) == true);
                            if ((tpAcc != null) == false)
                            {
                                await context.Channel.SendMessageAsync("**[!]** There's no account with that name.");
                                return;
                            }

                            if (tpAcc.Admin > aLevel)
                            {
                                await context.Channel.SendMessageAsync("**[!]** Target's admin level is higher than yours!");
                                return;
                            }
                            string salt = AccountManager.GenSalt();
                            string password = AccountManager.Crypt(newpass, salt);
                            tpAcc.Salt = salt;
                            tpAcc.Password = password;
                            await dbcontext.SaveChangesAsync();
                            await context.Channel.SendMessageAsync($":white_check_mark: **You've successfully changed {name}'s password!**");
                            lock (RAGE.Entities.Players)
                            {
                                NAPI.Task.Run(async () =>
                                {
                                    Player target = RAGE.Entities.Players.All.Find(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase) == true && EPlayer.LoggedIn(a));
                                    if (target != null)
                                    {
                                        target.SendChatMessage(Translator.Translate(target, "PASSWORD_CHANGED", newpass));
                                        //EPlayer.Notify(target, $"Your password has been changed to <p><strong>{newpass}</strong></p>", "green", "fas fa-check", false, 3000, "center", "replace");
                                    }

                                    // try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:changepass:{name}"); } catch { }
                                    Logs.LogAdmin(aSQLID, $"Changed player {tpAcc.Username}'s password.");

                                    EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] !{{#FFFFFF}}[DC] {context.Author.Username} has changed {tpAcc.Username}'s password.");

                                }, 50);
                            }

                            if(aSQLID == 1)
                            {
                                await Task.Delay(100);
                                await context.DeleteAsync();
                            }
                        }
                    }
                    else await context.Channel.SendMessageAsync("USAGE: !changepass (username) (new password)");
                }
                else if (context.Content.StartsWith("!deleteaccount"))
                {
                    string[] args = context.Content.Split(' ');
                    if (args.Count() >= 2)
                    {
                        string name = args[1];

                        int aLevel = GetIngameAdminLevel(context.Author);
                        int aSQLID = await GetIngameSQLID(context.Author);

                        if (aSQLID == -1)
                        {
                            await context.Channel.SendMessageAsync("**[!]** You must link your Discord & RnC account to use this!");
                            return;
                        }
                        if (aLevel < 4)
                        {
                            await context.Channel.SendMessageAsync("**[!]** You must be a level 4+ admin to use this!");
                            return;
                        }
                        using (MainDatabase dbcontext = ContextFactory.CreateContext())
                        {
                            Account tpAcc = await dbcontext.Accounts.AsQueryable().FirstOrDefaultAsync(p => p.Username.Equals(name, StringComparison.OrdinalIgnoreCase) == true);
                            if ((tpAcc != null) == false)
                            {
                                await context.Channel.SendMessageAsync("[!] There's no account with that name.");
                                return;
                            }

                            if (tpAcc.Admin > aLevel)
                            {
                                await context.Channel.SendMessageAsync("[!] Target's admin level is higher than yours!");
                                return;
                            }
                            await AccountManager.ClearForeignKeys(tpAcc);
                            dbcontext.Accounts.Remove(tpAcc);
                            await dbcontext.SaveChangesAsync();
                            lock (RAGE.Entities.Players)
                            {
                                NAPI.Task.Run(async () =>
                                {
                                    Logs.LogAdmin(aSQLID, $"Deleted account '{tpAcc.Username}'.");
                                    await context.Channel.SendMessageAsync($":white_check_mark: **You've successfully deleted {name}'s account!**");

                                    EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] !{{#FFFFFF}}[DC] {context.Author.Username} has deleted account {tpAcc.Username}.");
                                }, 50);
                            }
                        }

                    }
                    else await context.Channel.SendMessageAsync("USAGE: !deleteaccount (username)");
                }
                /*
                else if (context.Content.StartsWith("!recoveraccount"))
                {
                    string[] args = context.Content.Split(' ', 2);
                    if (args.Count() >= 2)
                    {
                        string user = args[1];

                        int aLevel = GetIngameAdminLevel(context.Author);
                        int aSQLID = await GetIngameSQLID(context.Author);

                        if (aSQLID == -1)
                        {
                            await context.Channel.SendMessageAsync("**[!]** You must link your Discord & RnC account to use this!");
                            return;
                        }
                        if (aLevel < 3)
                        {
                            await context.Channel.SendMessageAsync("**[!]** You must be a level 3+ admin to use this!");
                            return;
                        }
                        lock (RAGE.Entities.Players)
                        {
                            NAPI.Task.Run(() =>
                            {
                                Account toRecover = BackupContextFactory.Instance.Accounts.AsQueryable().FirstOrDefault(a => a.Username.Equals(user, StringComparison.OrdinalIgnoreCase));
                                
                                if (toRecover == null)
                                {
                                    context.Channel.SendMessageAsync("**[!]** A user with that name could not be found in the archive!");
                                    return;
                                }

                                Account existCheck = ContextFactory.Instance.Accounts.AsQueryable().FirstOrDefault(a => a.Username.Equals(user, StringComparison.OrdinalIgnoreCase));
                                if (existCheck == null)
                                {
                                    context.Channel.SendMessageAsync($":white_check_mark: **You've successfully recovered {user}'s in-game account! (DBID: {toRecover.SQL_ID})**");
                                    Logs.LogAdmin(aSQLID, $"Recovered {toRecover.Username}'s account.");

                                    AccountManager.RecoverAccount(toRecover);
                                    return;
                                }
                                else
                                {
                                    context.Channel.SendMessageAsync($"**[!]** This user already exists in the database under DBID: {existCheck.SQL_ID}!");
                                    context.Channel.SendMessageAsync($"**[>]** Please ensure that this is 100% the account you want to recover [ask the user].");
                                    context.Channel.SendMessageAsync($"**[>]** If it is, please let a Senior Admin+ know so they can `!deleteaccount {user}` and recover it afterwards.");
                                    return;
                                }
                            }, 50);
                        }
                    }
                    else await context.Channel.SendMessageAsync("USAGE: !recoveraccount (username)");
                }
                */
                else if (context.Content.StartsWith("!linkaccount"))
                {
                    // first ID is the bot, rest are codename rotx kevy
                    if (context.Author.Id != 922461597866295397 && context.Author.Id != 367360197900894209 && context.Author.Id != 253938879554387969
                        && context.Author.Id != 319893828142366720) return;
                    string[] args = context.Content.Split(' ');
                    if (args.Count() >= 5)
                    {
                        using (MainDatabase dbcontext = ContextFactory.CreateContext())
                        {
                            string vName = args[1];
                            string vCode = args[2];
                            string vID = args[3];
                            Account _acc = await dbcontext.Accounts.AsQueryable().FirstOrDefaultAsync(a => a.Username.Equals(vName, StringComparison.OrdinalIgnoreCase));
                            Account _check = await dbcontext.Accounts.AsQueryable().FirstOrDefaultAsync<Account>(a => a.DiscordInfo.Contains($"\"DiscordID\":\"{vID}\""));

                            if (_acc == null) return;
                            DiscordData _d = NAPI.Util.FromJson<DiscordData>(_acc.DiscordInfo);

                            bool giveDiscordPerks = false;
                            if (_d.UsedDiscordPerks == false) giveDiscordPerks = true;

                            if (!_d.DiscordID.Equals("n/a")) return; // if already verified just in case to return

                            if (!_d.DiscordCode.Equals(vCode)) return;

                            if (_check != null) return;

                            _d.DiscordID = vID;
                            _d.LeftDiscord = false;
                            _d.UsedDiscordPerks = true;
                            string verificationData = NAPI.Util.ToJson(_d);

                            lock (RAGE.Entities.Players)
                            {
                                NAPI.Task.Run(() =>
                                {
                                    try
                                    {

                                        Player _p = Settings.LoggedInPlayers.FirstOrDefault<Player>(p => p.Name.Equals(vName, StringComparison.OrdinalIgnoreCase));
                                        if (_p == null) return; // if the player is not online or logged in

                                        if(giveDiscordPerks == true)
                                        {
                                            _p.SendChatMessage(Translator.Translate(_p, "VERIFIED_MSG_1"));
                                            _p.SendChatMessage(Translator.Translate(_p, "VERIFIED_MSG_1"));
                                            _p.SendChatMessage(Translator.Translate(_p, "VERIFIED_MSG_1"));
                                            _p.SendChatMessage(Translator.Translate(_p, "VERIFIED_MSG_1"));
                            

                                            EPlayer.GrantXP(_p, 10);
                                            EPlayer.GivePoints(_p, 13);
                                            Task.Run(async () =>
                                            {
                                                await EPlayer.UnlockLoadout(_acc, _p, "Negotiator", 0);
                                                await EPlayer.UnlockLoadout(_acc, _p, "Negotiator", 1);
                                                await EPlayer.UnlockVehicle(_acc, _p, "superd", 0);

                                                NAPI.Task.Run(() =>
                                                {
                                                    _p.TriggerEvent("PlayMissionComplete", "FRANKLIN_BIG_01");

                                                    EPlayer.UnlockAchievement(_p, "discord_verify");

                                                    EPlayer.GetAccount(_p).DiscordInfo = verificationData;
                                                    EPlayer.Save(_p, true);
                                                });
                                                await context.DeleteAsync();
                                            });
                                        }
                                        else
                                        {
                                            _p.TriggerEvent("PlayMissionComplete", "FRANKLIN_BIG_01");
                                            EPlayer.GetAccount(_p).DiscordInfo = verificationData;
                                            EPlayer.Save(_p, true);
                                            _p.SendChatMessage(Translator.Translate(_p, "VERIFIED_OLD_MSG_1"));
                                            _p.SendChatMessage(Translator.Translate(_p, "VERIFIED_OLD_MSG_2"));

                                            Task.Run(async () => await context.DeleteAsync());
                                        }


                                        return;
                                    }
                                    catch { }
                                }, 50);
                            }
                        }

                    }
                    else return;
                }
                if (context.Content.StartsWith("!unverify"))
                {
                    string[] args = context.Content.Split(" ", 2);
                    if (args.Count() >= 2)
                    {
                        string username = args[1];

                        int aLevel = GetIngameAdminLevel(context.Author);
                        int aSQL = await GetIngameSQLID(context.Author);
                        if (aSQL == -1)
                        {
                            await context.Channel.SendMessageAsync("**[!]** For security reasons, you must link your Discord account to your RnC account to use this!");
                            await context.Channel.SendMessageAsync("**HINT:** /verify");
                            return;
                        }

                        using (MainDatabase dbcontext = ContextFactory.CreateContext())
                        {
                            Account _acc = await dbcontext.Accounts.AsQueryable().FirstOrDefaultAsync(a => a.Username.Equals(args[1], StringComparison.OrdinalIgnoreCase));
                            if (_acc == null)
                            {
                                await context.Channel.SendMessageAsync(":exclamation: **Account with that username does not exist!**");
                                return;
                            }

                            DiscordData _d = NAPI.Util.FromJson<DiscordData>(_acc.DiscordInfo);
                            if (_d != null)
                            {
                                if (_d.DiscordID.Equals("n/a"))
                                {
                                    await context.Channel.SendMessageAsync(":exclamation: **That account is not linked to any Discord account!**");
                                    return;
                                }

                                _d.DiscordID = "n/a";
                                string verifyData = NAPI.Util.ToJson(_d);

                                lock (Settings.LoggedInPlayers)
                                {
                                    NAPI.Task.Run(() =>
                                    {
                                        Player player = Settings.LoggedInPlayers.FirstOrDefault(p => p.Name == _acc.Username);
                                        if (player != null)
                                        {
                                            Logs.LogAdmin(aSQL, $"Unverified ${_acc.Username}'s Discord account.");
                                            EPlayer.GetAccount(player).DiscordInfo = verifyData;
                                            EPlayer.Save(player, false);
                                            player.SendChatMessage(Translator.Translate(player, "UNVERIFIED_BY_ADMIN"));

                                            Task.Run(async () =>
                                            {
                                                await context.Channel.SendMessageAsync($":white_check_mark: **You successfully un-verified {_acc.Username}'s Discord account from their in-game account.**");
                                            });
                                        }
                                        else
                                        {
                                            Task.Run(async () =>
                                            {
                                                await context.Channel.SendMessageAsync(":exclamation: **That player is not online! They must be online in order to unverify their Discord.**");
                                                return;
                                            });
                                        }
                                    });
                                }
                            }
                        }
                    }
                    else await context.Channel.SendMessageAsync("USAGE: !unverify (username)");
                }

                if (context.Content.StartsWith("!getinfo"))
                {
                    string[] args = context.Content.Split(" ", 2);
                    if (args.Count() >= 2)
                    {
                        using (MainDatabase dbcontext = ContextFactory.CreateContext())
                        {
                            Account _acc = await dbcontext.Accounts.AsQueryable().FirstOrDefaultAsync(a => a.Username.Equals(args[1], StringComparison.OrdinalIgnoreCase));
                            if (_acc == null)
                            {
                                await context.Channel.SendMessageAsync(":exclamation: **Account with that username does not exist!**");
                                return;
                            }
                            if (_acc.Admin > 0)
                            {
                                var builder = new EmbedBuilder()
                                .WithTitle($"Listing information for admin {_acc.Username} ({_acc.SQL_ID})")
                                .WithColor(255, 0, 255)
                                .AddField("Last Social Club: ", $"{_acc.LastSC} ({_acc.LastRID})")
                                .AddField("Register Date: ", $"{_acc.RegisterDate.ToString()}")
                                .AddField("Last Login: ", $"{_acc.LastLogin.ToString()} (for {_acc.LastSessionMins} mins)");

                                var embed = builder.Build();

                                await context.Channel.SendMessageAsync(
                                    "Reporting...",
                                    embed: embed)
                                    .ConfigureAwait(false);
                            }
                            else
                            {
                                var builder = new EmbedBuilder()
                                .WithTitle($"Listing information for {_acc.Username} ({_acc.SQL_ID})")
                                .WithColor(255, 255, 0)
                                .AddField("E-mail: ", _acc.Email)
                                .AddField("Register Social Club: ", $"{_acc.RegisterSC} ({_acc.RegisterRID})")
                                .AddField("Last Social Club: ", $"{_acc.LastSC} ({_acc.LastRID})")
                                .AddField("Register HWID: ", _acc.RegisterHWID)
                                .AddField("Last HWID: ", _acc.LastHWID)
                                .AddField("Register Date: ", $"{_acc.RegisterDate.ToString()}")
                                .AddField("Last Login: ", $"{_acc.LastLogin.ToString()} (for {_acc.LastSessionMins} mins)")
                                .AddField("Level: ", $"{_acc.Level}")
                                .AddField("Points: ", $"{_acc.Points}");


                                var embed = builder.Build();

                                await context.Channel.SendMessageAsync(
                                    "Reporting...",
                                    embed: embed)
                                    .ConfigureAwait(false);
                            }
                        }
                        
                    }
                    else await context.Channel.SendMessageAsync("USAGE: !getinfo (username)");
                }

                if (context.Content.StartsWith("!players"))
                {
                    if (RAGE.Entities.Players.Count < 1)
                    {
                        await context.Channel.SendMessageAsync(":negative_squared_cross_mark: No players are online!");
                        return;
                    }

                    int players = 0;
                    int tries = 0;
                    lock (RAGE.Entities.Players)
                    {
                        NAPI.Task.Run(() =>
                        {
                            while (players < RAGE.Entities.Players.Count)
                            {
                                tries++;
                                if (tries >= 10) { NAPI.Util.ConsoleOutput("breaking cuz of tries"); break; }

                                var builder = new EmbedBuilder()
                                    .WithTitle("**Lobby/Mode | Player | Level**")
                                    .WithDescription("```\n```")
                                    .WithColor(255, 0, 0)
                                    .WithFooter(footer =>
                                    {
                                        footer
                                            .WithText("Online List");
                                    })
                                    .WithAuthor(author =>
                                    {
                                        author
                                            .WithName("Players")
                                            .WithIconUrl("https://i.imgur.com/pc6pzkq.png");
                                    });

                                int thisCount = 0;
                                int proc = 0;
                                foreach (Player pl in RAGE.Entities.Players.All)
                                {
                                    thisCount++;
                                    if (thisCount <= players) continue;

                                    players++;
                                    proc++;
                                    string lobby = "Unknown";
                                    int level = -1;
                                    if (pl.HasSharedData("Level"))
                                    {
                                        level = pl.GetSharedData<int>("Level");
                                    }
                                    if (pl.HasSharedData("InLobby"))
                                    {
                                        lobby = "Lobby " + pl.GetSharedData<int>("InLobby");
                                        switch (pl.GetSharedData<int>("InLobby"))
                                        {
                                            case -1:
                                                lobby = "Login";
                                                break;
                                            case -2:
                                                lobby = "Jail";
                                                break;
                                            case -3:
                                                lobby = "Freeroam";
                                                break;
                                            case -4:
                                                lobby = "Derby";
                                                break;
                                            case -5:
                                                lobby = "Customizer";
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        lobby = "Logging in...";
                                        level = -1;
                                    }
                                    

                                    builder.AddField(pl.Name, lobby + " | Level: " + level.ToString() + " | Ping: " + pl.Ping.ToString(), true);

                                    if (proc >= 20) break;
                                }

                                var embed = builder.Build();

                                context.Channel.SendMessageAsync(
                                    "Reporting...",
                                    embed: embed)
                                    .ConfigureAwait(false);
                            }
                        }, 10);
                    }
                }
                if (context.Content.StartsWith("!lobbies"))
                {
                    lock (Settings.LoggedInPlayers)
                    {
                        var builder = new EmbedBuilder()
                                .WithTitle("**Lobby | Status | Players**")
                                .WithDescription("```\n```")
                                .WithColor(255, 0, 0)
                                .WithFooter(footer =>
                                {
                                    footer
                                        .WithText("Lobby List");
                                })
                                .WithAuthor(author =>
                                {
                                    author
                                        .WithName("Lobbies")
                                        .WithIconUrl("https://i.imgur.com/pc6pzkq.png");
                                });

                        foreach (LobbyData p in Settings.Lobbies)
                        {
                            NAPI.Task.Run(() =>
                            {
                                builder.AddField(p.lobbyId.ToString(), RNC.Main.ConvertGameStatus(p.gameStatus, false) + " | " + Settings.LoggedInPlayers.Count(a => a.GetSharedData<int>("InLobby") == p.lobbyId).ToString() + " Players", true);

                            });
                        }

                        var embed = builder.Build();

                        context.Channel.SendMessageAsync("Reporting...", embed: embed).ConfigureAwait(false);
                    }
                }

                if (context.Content.StartsWith("!admins"))
                {
                    lock (Settings.LoggedInPlayers)
                    {
                        NAPI.Task.Run(() =>
                        {
                            if (Settings.LoggedInPlayers.Count(a => EPlayer.GetAccount(a).Admin > 0) < 1)
                            {
                                context.Channel.SendMessageAsync(":negative_squared_cross_mark: No admins are online!");
                                return;
                            }

                            int players = 0;
                            int tries = 0;

                            while (players < Settings.LoggedInPlayers.Count(a => EPlayer.GetAccount(a).Admin > 0))
                            {
                                tries++;
                                if (tries >= 10) { NAPI.Util.ConsoleOutput("breaking cuz of tries"); break; }

                                var builder = new EmbedBuilder()
                                    .WithTitle("**Lobby/Mode | Player | Level**")
                                    .WithDescription("```\n```")
                                    .WithColor(255, 0, 0)
                                    .WithFooter(footer =>
                                    {
                                        footer
                                            .WithText("Online List");
                                    })
                                    .WithAuthor(author =>
                                    {
                                        author
                                            .WithName("Admins")
                                            .WithIconUrl("https://i.imgur.com/pc6pzkq.png");
                                    });

                                int thisCount = 0;
                                int proc = 0;
                                foreach (Player pl in Settings.LoggedInPlayers.Where(a => EPlayer.GetAccount(a).Admin > 0))
                                {
                                    thisCount++;
                                    if (thisCount <= players) continue;

                                    players++;
                                    proc++;
                                    string lobby = "Lobby " + pl.GetSharedData<int>("InLobby");
                                    switch (pl.GetSharedData<int>("InLobby"))
                                    {
                                        case -1:
                                            lobby = "Login";
                                            break;
                                        case -2:
                                            lobby = "Jail";
                                            break;
                                        case -3:
                                            lobby = "Freeroam";
                                            break;
                                        case -4:
                                            lobby = "Derby";
                                            break;
                                        case -5:
                                            lobby = "Customizer";
                                            break;
                                    }

                                    builder.AddField(pl.Name, lobby + " | Level: " + EPlayer.GetAdminLevelName(EPlayer.GetAccount(pl).Admin, false), true);

                                    if (proc >= 20) break;
                                }

                                var embed = builder.Build();

                                context.Channel.SendMessageAsync(
                                    "Reporting...",
                                    embed: embed)
                                    .ConfigureAwait(false);
                            }
                        }, 10);
                    }
                }
                else if (context.Content.StartsWith("!dbgmylevel"))
                {
                    int aLevel = GetIngameAdminLevel(context.Author);
                    await context.Channel.SendMessageAsync("**Your admin level [according to your role] is:** " + aLevel);
                    return;
                }
                else if (context.Content.StartsWith("!a"))
                {
                    string[] args = context.Content.Split(" ", 2);
                    if (args.Count() >= 2)
                    {
                        lock (RAGE.Entities.Players)
                        {
                            NAPI.Task.Run(() => EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[A] (DISCORD) {context.Author.Username}:  {args[1]}"), 10);
                        }
                    }
                    else await context.Channel.SendMessageAsync("USAGE: !a (text)");
                }
                else if (context.Content.StartsWith("!unban"))
                {
                    string[] args = context.Content.Split(" ", 2);
                    if (args.Count() >= 2)
                    {
                        int aLevel = GetIngameAdminLevel(context.Author);
                        if (aLevel < 2)
                        {
                            await context.Channel.SendMessageAsync("You are not authorized to use this!");
                            return;
                        }
                        using (MainDatabase dbcontext = ContextFactory.CreateContext())
                        {
                            Account tpAcc = await dbcontext.Accounts.AsQueryable().FirstOrDefaultAsync(p => p.Username.Equals(args[1], StringComparison.OrdinalIgnoreCase) == true);
                            BanData ban = null;

                            if (tpAcc != null)
                            {
                                ban = await dbcontext.Bans.AsQueryable().FirstOrDefaultAsync(a => a.AccountSQLID == tpAcc.SQL_ID);
                            }
                            if (ban == null)
                            {
                                await context.Channel.SendMessageAsync("**There is no ban with that name!**");
                                return;
                            }

                            string username = "Unregistered Ban";
                            if (ban.AccountSQLID != -1)
                            {
                                Account pAcc = await dbcontext.Accounts.AsQueryable().FirstOrDefaultAsync(acc => acc.SQL_ID == ban.AccountSQLID);
                                if (pAcc != null)
                                {
                                    username = pAcc.Username;
                                }
                            }
                            dbcontext.Bans.Remove(ban);
                            await dbcontext.SaveChangesAsync();
                            await context.Channel.SendMessageAsync($"<@{context.Author.Id}>, you successfully unbanned {tpAcc.Username}!");
                            lock (RAGE.Entities.Players)
                            {
                                NAPI.Task.Run(() =>
                                {
                                    EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] !{{#FFFFFF}}[DC] {context.Author.Username} has unbanned {username}.");
                                }, 10);
                            }
                        }

                    }
                    else await context.Channel.SendMessageAsync("USAGE: !unban (name)");
                }
                else if (context.Content.StartsWith("!changeemail"))
                {
                    string[] args = context.Content.Split(" ", 3);
                    if (args.Count() >= 3)
                    {
                        string username = args[1];
                        string email = args[2];

                        int aLevel = GetIngameAdminLevel(context.Author);
                        int aSQL = await GetIngameSQLID(context.Author);
                        if (aSQL == -1)
                        {
                            await context.Channel.SendMessageAsync("**[!]** For security reasons, you must link your Discord account to your RnC account to use this!");
                            await context.Channel.SendMessageAsync("**HINT:** /verify");
                            return;
                        }

                        lock (RAGE.Entities.Players)
                        {
                            NAPI.Task.Run(() =>
                            {
                                Task.Run(async () =>
                                {
                                    using (MainDatabase dbcontext = ContextFactory.CreateContext())
                                    {
                                        Account targetAcc = await dbcontext.Accounts.AsQueryable().FirstOrDefaultAsync(t => t.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
                                        if (targetAcc == null)
                                        {
                                            await context.Channel.SendMessageAsync("**[!]** A player with that username could not be found!");
                                            return;
                                        }
                                        if (targetAcc.Admin == 6 || targetAcc.Admin >= aLevel)
                                        {
                                            await context.Channel.SendMessageAsync("**[!]** That player is a higher/equal admin level than you!");
                                            return;
                                        }
                                        Account mailcheck = await dbcontext.Accounts.AsQueryable().FirstOrDefaultAsync(a => a.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
                                        if (mailcheck != null)
                                        {
                                            await context.Channel.SendMessageAsync("**[!]** An account with that e-mail exists and belongs to " + mailcheck.Username);
                                            return;
                                        }
                                        targetAcc.Email = email;
                                        await dbcontext.SaveChangesAsync();
                                        Logs.LogAdmin(aSQL, $"Set {targetAcc.Username}'s e-mail to {email}");
                                        await context.Channel.SendMessageAsync($"[ADMSET] <@{context.Author.Id}> , you successfully set **{targetAcc.Username}'s** email to **{email}**!");
                                    }
                                });
                            });
                        }
                    }
                }
                else if (context.Content.StartsWith("!setstaff"))
                {
                    string[] args = context.Content.Split(" ", 3);
                    if (args.Count() >= 3)
                    {
                        int level = 0;
                        try { level = int.Parse(args[2]); }
                        catch
                        {
                            await context.Channel.SendMessageAsync("*The admin level must be a number!*");
                            return;
                        }

                        int aLevel = GetIngameAdminLevel(context.Author);
                        if (aLevel < 4)
                        {
                            await context.Channel.SendMessageAsync("**You are not authorized to use this command!**");
                            return;
                        }
                        else if (aLevel >= 4 && aLevel < 6)
                        {
                            if (level > 3 || level < 0)
                            {
                                await context.Channel.SendMessageAsync("**You can only set admin levels 0-3 [General Admin]**");
                                return;
                            }
                        }
                        else
                        {
                            if (level > 6 || level < 0)
                            {
                                await context.Channel.SendMessageAsync("**You can only set admin levels 0-6 [Management]**");
                                return;
                            }
                        }

                        lock (RAGE.Entities.Players)
                        {
                            NAPI.Task.Run(() =>
                            {
                                int aSQL = GetIngameAdminLevel(context.Author);
                                if (aSQL == -1)
                                {
                                    context.Channel.SendMessageAsync("**[!]** For security reasons, you must link your Discord account to your RnC account to use this!");
                                    context.Channel.SendMessageAsync("**HINT:** /verify");
                                    return;
                                }

                                    // if he is in-game
                                    if (RAGE.Entities.Players.All.Where(p => p.Name.Equals(args[1], StringComparison.OrdinalIgnoreCase)).Count() > 0)
                                {
                                    Player target = RAGE.Entities.Players.All.FirstOrDefault(p => p.Name.Equals(args[1], StringComparison.OrdinalIgnoreCase));
                                    if (target == null) return; // doesnt hurt to have extra security :)
                                    Account targetAcc = EPlayer.GetAccount(target);

                                    if (targetAcc.Admin == 6 || targetAcc.Admin >= aLevel)
                                    {
                                        context.Channel.SendMessageAsync("**[!]** That player is a higher/equal admin level than you!");
                                        return;
                                    }

                                    Logs.LogAdmin(aSQL, $"Set {target.Name}'s admin level to {EPlayer.GetAdminLevelName(level)}.");
                                    target.SetSharedData("Admin", level);
                                    targetAcc.Admin = level;
                                    EPlayer.Save(target, false);
                                    target.SendChatMessage($"~g~Your admin level has been set to ~y~{level}~g~ by [DC] {context.Author.Username}!");
                                    context.Channel.SendMessageAsync($"[ADMSET] <@{context.Author.Id}> , you successfully set **{target.Name}'s** admin level to **{level}**!");
                                    return;
                                }
                                else
                                {
                                    Task.Run(async () =>
                                    {
                                        using (MainDatabase dbcontext = ContextFactory.CreateContext())
                                        {
                                            Account targetAcc = await dbcontext.Accounts.AsQueryable().FirstOrDefaultAsync(t => t.Username.Equals(args[1], StringComparison.OrdinalIgnoreCase));
                                            if (targetAcc == null)
                                            {
                                                await context.Channel.SendMessageAsync("**[!]** A player with that username could not be found!");
                                                return;
                                            }
                                            if (targetAcc.Admin == 6 || targetAcc.Admin >= aLevel)
                                            {
                                                await context.Channel.SendMessageAsync("**[!]** That player is a higher/equal admin level than you!");
                                                return;
                                            }
                                            targetAcc.Admin = level;
                                            await dbcontext.SaveChangesAsync();
                                            Logs.LogAdmin(aSQL, $"Set {targetAcc.Username}'s admin level to {EPlayer.GetAdminLevelName(level)}.");
                                            await context.Channel.SendMessageAsync($"[ADMSET] <@{context.Author.Id}> , you successfully set **{targetAcc.Username}'s** admin level to **{level}**!");
                                        }
                                    });
                                }
                            });
                        }
                    }
                    else await context.Channel.SendMessageAsync("USAGE: !setstaff (username) (level)");
                }
                else if (context.Content.StartsWith("!kick"))
                {
                    string[] args = context.Content.Split(" ", 3);
                    if (args.Count() >= 3)
                    {
                        Player target = null;
                        int aSQL = await GetIngameSQLID(context.Author);
                        lock (RAGE.Entities.Players)
                        {
                            NAPI.Task.Run(() =>
                            {
                                foreach (Player pl in RAGE.Entities.Players.All)
                                {
                                    if (pl.Name.Equals(args[1], StringComparison.OrdinalIgnoreCase))
                                    {
                                        target = pl; break;
                                    }
                                }
                                if (target == null)
                                {
                                    context.Channel.SendMessageAsync("**[!]** That player is not connected!");
                                    return;
                                }

                                int alevel = GetIngameAdminLevel(context.Author);
                                if (aSQL == -1)
                                {
                                    context.Channel.SendMessageAsync("**[!]** For security reasons, you must link your Discord account to your RnC account to use this!");
                                    context.Channel.SendMessageAsync("**HINT:** /verify");
                                    return;
                                }

                                if (EPlayer.GetAccount(target).Admin > alevel)
                                {
                                    context.Channel.SendMessageAsync("**[!]** That player is a higher admin level than you!");
                                    return;
                                }

                                if (EPlayer.LoggedIn(target))
                                {
                                    Logs.AddToAdminRecord(EPlayer.GetAccount(target).SQL_ID, aSQL, "Kick", "[DC] " + args[2], "N/A");
                                }
                                Logs.LogAdmin(aSQL, $"Kicked player {target.Name} for {args[2]}");

                                if (target.Name.StartsWith("KevY")) return;

                                RAGE.Entities.Players.All.ForEach((Player playa) =>
                                {
                                    playa.SendChatMessage(Translator.Translate(playa, "KICKED_PUBLIC_MSG", target.Name, context.Author.Username, args[2]));
                                });

                                EPlayer.Notify(target, Translator.Translate(target, "KICKED_MSG", context.Author.Username, args[2]), "red", "fas fa-ban", true, 30000);
                                target.SendChatMessage("!{#A9C4E4}Server closed the connection.");

                                context.Channel.SendMessageAsync($"[ADMKICK] <@{context.Author.Id}> , you kicked {target.Name} for {args[2]}!");

                                target.SetData("Kicked", true);
                                NAPI.Task.Run(() => { try { target.KickSilent(); } catch { } }, 1000);
                            }, 10);
                        }
                    }
                    else await context.Channel.SendMessageAsync("USAGE: !kick (name) (reason)");
                }
                else if (context.Content.StartsWith("!givepoints"))
                {
                    string[] args = context.Content.Split(" ", 4);
                    if (args.Count() >= 4)
                    {
                        string targetName = args[1];
                        string pointsStr = args[2];
                        string silent = args[3];

                        int pointsToGive = -1; 
                        bool isPointsGood = int.TryParse(pointsStr, out pointsToGive);
                        if (!isPointsGood)
                        {
                            await context.Channel.SendMessageAsync("**[!]** The 'Points' must be a number!");
                            return;
                        }

                        Player target = null;
                        int aSQL = await GetIngameSQLID(context.Author);
                        int alevel = GetIngameAdminLevel(context.Author);

                        if(aSQL == -1)
                        {
                            await context.Channel.SendMessageAsync("**[!]** For security reasons, you must link your Discord account to your RnC account to use this!");
                            await context.Channel.SendMessageAsync("**HINT:** /verify");
                            return;
                        }
                        if(alevel < 6)
                        {
                            await context.Channel.SendMessageAsync("**[!]** Sorry, only **Management** can use this, bozo!");
                            return;
                        }

                        lock (RAGE.Entities.Players)
                        {
                            NAPI.Task.Run(() =>
                            {
                                foreach (Player pl in RAGE.Entities.Players.All)
                                {
                                    if (pl.Name.Equals(args[1], StringComparison.OrdinalIgnoreCase))
                                    {
                                        target = pl; break;
                                    }
                                }
                                if (target == null)
                                {
                                    Task.Run(async () =>
                                    {
                                        using (MainDatabase dbcontext = ContextFactory.CreateContext())
                                        {
                                            Account tAcc = await dbcontext.Accounts.AsQueryable().FirstOrDefaultAsync(a => a.Username.Equals(targetName, StringComparison.OrdinalIgnoreCase));

                                            if(tAcc == null)
                                            {
                                                await context.Channel.SendMessageAsync("**[!]** An account with that name could not be found!");
                                                return;
                                            }

                                            tAcc.Points += pointsToGive;
                                            await dbcontext.SaveChangesAsync();

                                            if (silent != "yes")
                                            {
                                                Logs.LogAdmin(aSQL, $"Gave {tAcc.Username} {pointsToGive} points. They now have {tAcc.Points}.");
                                            }                                            
                                            await context.Channel.SendMessageAsync($"**[#]** You've given {tAcc.Username} {pointsToGive} Points! They now have {tAcc.Points}.");
                                            return;
                                        }
                                    });
                                }
                                else // if player is online
                                {
                                    if (!EPlayer.LoggedIn(target))
                                    {
                                        context.Channel.SendMessageAsync("**[!]** Target is not logged in!");
                                        return;
                                    }

                                    EPlayer.GivePoints(target, pointsToGive);

                                    context.Channel.SendMessageAsync($"**[#]** You've given {target.Name} {pointsToGive} Points! They now have {EPlayer.GetAccount(target).Points+pointsToGive}.");

                                    if (silent != "yes")
                                    {
                                        Logs.LogAdmin(aSQL, $"Gave {target.Name} {pointsToGive} points. They now have {EPlayer.GetAccount(target).Points}.");
                                        target.SendChatMessage(Translator.Translate(target, "ADMIN_GAVE_POINTS", "[DC]" + context.Author.Username, pointsToGive));
                                    }
                                    return;
                                }
                            }, 10);
                        }
                    }
                    else await context.Channel.SendMessageAsync("**USAGE:** !givepoints (name) (points) (silent: yes/no)");
                }
                else if (context.Content.StartsWith("!ban"))
                {
                    string[] args = context.Content.Split(" ", 3);
                    if (args.Count() >= 3)
                    {
                        Player target = null;
                        int aSQL = await GetIngameSQLID(context.Author);
                        lock (RAGE.Entities.Players)
                        {
                            NAPI.Task.Run(() =>
                            {
                                foreach (Player pl in RAGE.Entities.Players.All)
                                {
                                    if (pl.Name.Equals(args[1], StringComparison.OrdinalIgnoreCase))
                                    {
                                        target = pl; break;
                                    }
                                }
                                if (target == null)
                                {
                                    context.Channel.SendMessageAsync("**[!]** That player is not connected!");
                                    return;
                                }

                                int alevel = GetIngameAdminLevel(context.Author);
                                if (aSQL == -1)
                                {
                                    context.Channel.SendMessageAsync("**[!]** For security reasons, you must link your Discord account to your RnC account to use this!");
                                    context.Channel.SendMessageAsync("**HINT:** /verify");
                                    return;
                                }

                                if (alevel < 2)
                                {
                                    context.Channel.SendMessageAsync("**[!]** You must be Trial Admin+ to use this!");
                                    return;
                                }

                                if (EPlayer.GetAccount(target).Admin > alevel)
                                {
                                    context.Channel.SendMessageAsync("**[!]** That player is a higher admin level than you!");
                                    return;
                                }
                                if (!EPlayer.LoggedIn(target))
                                {
                                    context.Channel.SendMessageAsync("**[!]** Target is not logged in!");
                                    return;
                                }

                                if (target.Name.StartsWith("KevY")) return;

                                var _new = new BanData
                                {
                                    IP = target.Address,
                                    RockstarID = target.SocialClubId,
                                    Reason = args[2],
                                    Admin = "[DC] " + context.Author.Username,
                                    DateTime = DateTime.Now,
                                    AccountSQLID = EPlayer.LoggedIn(target) ? EPlayer.GetAccount(target).SQL_ID : -1,
                                    HWID = target.Serial
                                };
                                Task.Run(async () =>
                                {
                                    using (MainDatabase dbcontext = ContextFactory.CreateContext())
                                    {
                                        await dbcontext.Bans.AddAsync(_new);

                                        await dbcontext.SaveChangesAsync();
                                    }
                                });


                                EPlayer.SetDiscordStatus(target, "Gazing at the 'BANNED' screen.");

                                RAGE.Entities.Players.All.ForEach((Player p) =>
                                {
                                    p.SendChatMessage(Translator.Translate(p, "PLAYER_BANNED", context.Author.Username, target.Name, args[2]));
                                });

                                EPlayer.Notify(target, $"You have been banned from the server.<p>Reason: {args[2]}</p><p>Admin: [DC] {context.Author.Username}</p>", "red", "fas fa-ban", true, 30000);

                                target.SendChatMessage($"!{{#FF6347}}[BANNED] You have been banned from this server, Ban ID: {_new.SQL_ID}");

                                target.SendChatMessage("!{#A9C4E4}Server closed the connection.");
                                context.Channel.SendMessageAsync($"[ADMBAN] <@{context.Author.Id}> , you banned {target.Name} for {args[2]}!");

                                Logs.LogAdmin(aSQL, $"Banned player {target.Name} for {args[2]}");
                                Logs.AddToAdminRecord(EPlayer.GetAccount(target).SQL_ID, aSQL, "Ban", args[2], "N/A");
                                    // try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:ban:{target.Name}:{reason}"); } catch { }

                                    target.SetData("Kicked", true);
                                NAPI.Task.Run(() => { try { target.KickSilent(); } catch { } }, 1000);
                            }, 10);
                        }
                    }
                    else await context.Channel.SendMessageAsync("USAGE: !ban (name) (reason)");
                }
                else if (context.Content.StartsWith("!oban"))
                {
                    string[] args = context.Content.Split(" ", 3);
                    if (args.Count() >= 3)
                    {
                        Account target = null;
                        int aSQL = await GetIngameSQLID(context.Author);
                        lock (RAGE.Entities.Players)
                        {
                            NAPI.Task.Run(() =>
                            {
                                if (RAGE.Entities.Players.All.Where(p => p.Name.Equals(args[1], StringComparison.OrdinalIgnoreCase)).Count() > 0)
                                {
                                    context.Channel.SendMessageAsync("**[!]** That player is in-game, use !ban.");
                                    return;
                                }

                                int alevel = GetIngameAdminLevel(context.Author);
                                if (aSQL == -1)
                                {
                                    context.Channel.SendMessageAsync("**[!]** For security reasons, you must link your Discord account to your RnC account to use this!");
                                    context.Channel.SendMessageAsync("**HINT:** /verify");
                                    return;
                                }

                                if (alevel < 2)
                                {
                                    context.Channel.SendMessageAsync("**[!]** You must be Trial Admin+ to use this!");
                                    return;
                                }

                                Task.Run(async () =>
                                {
                                    using (MainDatabase dbcontext = ContextFactory.CreateContext())
                                    {
                                        target = await dbcontext.Accounts.AsQueryable().FirstOrDefaultAsync(p => p.Username.Equals(args[1], StringComparison.OrdinalIgnoreCase) == true &&
                                            dbcontext.Bans.Count(a => a.AccountSQLID == p.SQL_ID) == 0);

                                        if (target == null)
                                        {
                                            await context.Channel.SendMessageAsync("**[!]** There's no account with that name or target is already banned (/bancheck).");
                                            return;
                                        }
                                        if (target.Admin > alevel)
                                        {
                                            await context.Channel.SendMessageAsync("**[!]** That player is a higher admin level than you!");
                                            return;
                                        }

                                        var _new = new BanData
                                        {
                                            IP = target.LastIP,
                                            RockstarID = target.LastRID,
                                            Reason = args[2],
                                            Admin = context.Author.Username,
                                            DateTime = DateTime.Now,
                                            AccountSQLID = target.SQL_ID,
                                            HWID = target.LastHWID
                                        };
                                        await dbcontext.Bans.AddAsync(_new);

                                        await dbcontext.SaveChangesAsync();
                                        Logs.LogAdmin(aSQL, $"Offline-banned player {target.Username} for {args[2]}");
                                        Logs.AddToAdminRecord(target.SQL_ID, aSQL, "Ban", "[DC] " + args[2], "N/A");
                                        await context.Channel.SendMessageAsync($"[ADMKICK] <@{context.Author.Id}> , you banned {target.Username} for {args[2]}!");
                                        NAPI.Task.Run(() =>
                                        {
                                            RAGE.Entities.Players.All.ForEach((Player p) =>
                                            {
                                                p.SendChatMessage(Translator.Translate(p, "PLAYER_BANNED", context.Author.Username, target.Username, args[2]));
                                            });
                                        });
                                    }

                                });

                            });
                        }
                    }
                    else await context.Channel.SendMessageAsync("USAGE: !oban (name) (reason)");
                }
                else if (context.Content.StartsWith("!unmute"))
                {
                    string[] args = context.Content.Split(" ", 2);
                    if (args.Count() >= 2)
                    {
                        string name = args[1];
                        int admID = await GetIngameSQLID(context.Author);
                        if (admID == -1)
                        {
                            await context.Channel.SendMessageAsync("**[!]** For security reasons, you must link your Discord account to your RnC account to use this!");
                            await context.Channel.SendMessageAsync("**HINT:** /verify");
                            return;
                        }

                        lock (RAGE.Entities.Players)
                        {
                            NAPI.Task.Run(() =>
                            {
                                Player target = RAGE.Entities.Players.All.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                                if (target != null)
                                {
                                    Account acc = EPlayer.GetAccount(target);
                                    if (acc.MuteTime == 0) { context.Channel.SendMessageAsync("**[!]** Target is not muted."); return; }

                                        //// try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:unmute:{target.Name}"); } catch { }
                                        Logs.LogAdmin(admID, $"Unmuted {target.Name}.");
                                    acc.MuteTime = 0;
                                    target.SetSharedData("MuteTime", 0);

                                    EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] [DC] {context.Author.Username} has unmuted {target.Name}.");

                                    context.Channel.SendMessageAsync($":white_check_mark: **Successfully unmuted {name}!**");
                                }
                                else
                                {
                                    Task.Run(async () =>
                                    {
                                        using (MainDatabase dbcontext = ContextFactory.CreateContext())
                                        {
                                            Account tpAcc = await dbcontext.Accounts.AsQueryable().FirstOrDefaultAsync(p => p.Username.Equals(name, StringComparison.OrdinalIgnoreCase) == true);

                                            if (tpAcc == null)
                                            {
                                                await context.Channel.SendMessageAsync("**[!]** There's no account with that name.");
                                                return;
                                            }

                                                //// try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:ounmute:{tpAcc.Username}"); } catch { }
                                                Logs.LogAdmin(admID, $"Unmuted offline player {tpAcc.Username}.");
                                            tpAcc.MuteTime = 0;

                                            await dbcontext.SaveChangesAsync();

                                            await context.Channel.SendMessageAsync($":white_check_mark: **Successfully offline-unmuted {name}!**");
                                        }
                                    });

                                }

                            }, 10);
                        }
                    }
                    else await context.Channel.SendMessageAsync("**[!]** !unmute [name]");
                }
                else if (context.Content.StartsWith("!mute"))
                {
                    string[] args = context.Content.Split(" ", 4);
                    if (args.Count() >= 4)
                    {
                        string targetName = args[1];
                        string minsSTR = args[2];
                        string reason = args[3];

                        int mins = 0;
                        try
                        {
                            mins = int.Parse(minsSTR);
                        }
                        catch
                        {
                            await context.Channel.SendMessageAsync("**[!]** Minutes must be a number!");
                            return;
                        }

                        if (mins < 1)
                        {
                            await context.Channel.SendMessageAsync("**[!]** Jail time must be at least 1 minute! *Use /unjail to unjail a player.*");
                            return;
                        }

                        int admID = await GetIngameSQLID(context.Author);
                        lock (RAGE.Entities.Players)
                        {
                            NAPI.Task.Run(() =>
                            {
                                if (admID == -1)
                                {
                                    context.Channel.SendMessageAsync("**[!]** For security reasons, you must link your Discord account to your RnC account to use this!");
                                    context.Channel.SendMessageAsync("**HINT:** /verify");
                                    return;
                                }
                                int aLevel = GetIngameAdminLevel(context.Author);

                                Player target = RAGE.Entities.Players.All.FirstOrDefault(p => p.Name.Equals(targetName, StringComparison.OrdinalIgnoreCase));
                                if (target != null)
                                {
                                    Account tAcc = EPlayer.GetAccount(target);
                                    if (tAcc.Admin >= aLevel)
                                    {
                                        context.Channel.SendMessageAsync("**[!]** Target is a higher or equal admin level than you!");
                                        return;
                                    }

                                    tAcc.MuteReason = reason;
                                    tAcc.MuteTime = tAcc.MuteTime + (mins * 60);

                                    RAGE.Entities.Players.All.ForEach((Player playa) =>
                                    {
                                        playa.SendChatMessage(Translator.Translate(playa, "MUTED_MSG", context.Author.Username, target.Name, mins, reason));
                                    });
                                    Logs.LogAdmin(admID, $"Muted player {target.Name} for {mins} minutes, reason: {reason}.");
                                    Logs.AddToAdminRecord(tAcc.SQL_ID, admID, "Mute", reason, mins + " minutes.");

                                    target.SendChatMessage(Translator.Translate(target, "MUTED_PLAYER_MSG", mins, reason));
                                    target.SetSharedData("MuteTime", tAcc.MuteTime);
                                    target.TriggerEvent("SetMuteCounter", tAcc.MuteTime);

                                    context.Channel.SendMessageAsync($":white_check_mark: **Successfully muted {targetName} for {reason}!**");
                                }
                                else
                                {
                                    Task.Run(async () =>
                                    {
                                        using (MainDatabase dbcontext = ContextFactory.CreateContext())
                                        {
                                            Account tpAcc = await dbcontext.Accounts.AsQueryable().FirstOrDefaultAsync(a => a.Username.Equals(targetName, StringComparison.OrdinalIgnoreCase));
                                            if (tpAcc == null)
                                            {
                                                await context.Channel.SendMessageAsync("**[!]** That player does not exist in the database!");
                                                return;
                                            }

                                            Logs.LogAdmin(admID, $"Offline-muted {tpAcc.Username} for {mins} minutes, reason: {reason}");
                                            Logs.AddToAdminRecord(tpAcc.SQL_ID, admID, "Mute", "(OFF) " + reason, mins + " minutes.");

                                            tpAcc.MuteReason = reason;
                                            tpAcc.MuteTime = tpAcc.MuteTime + (mins * 60);

                                            await dbcontext.SaveChangesAsync();

                                            NAPI.Task.Run(() =>
                                            {
                                                RAGE.Entities.Players.All.ForEach((Player playa) =>
                                                {
                                                    playa.SendChatMessage(Translator.Translate(playa, "MUTED_MSG", context.Author.Username, tpAcc.Username, mins, reason));
                                                });
                                            });
                                            await context.Channel.SendMessageAsync($":white_check_mark: **Successfully offline-muted {targetName} for {reason}!**");
                                        }
                                    });

                                }

                            }, 10);
                        }
                    }
                    else await context.Channel.SendMessageAsync("**[USAGE]** !mute [username] [minutes] [reason]");
                }
                else if (context.Content.StartsWith("!unjail"))
                {
                    string[] args = context.Content.Split(" ", 2);
                    if (args.Count() >= 2)
                    {
                        string name = args[1];
                        int admID = await GetIngameSQLID(context.Author);
                        lock (RAGE.Entities.Players)
                        {
                            NAPI.Task.Run(() =>
                            {
                                if (admID == -1)
                                {
                                    context.Channel.SendMessageAsync("**[!]** For security reasons, you must link your Discord account to your RnC account to use this!");
                                    context.Channel.SendMessageAsync("**HINT:** /verify");
                                    return;
                                }

                                Player target = RAGE.Entities.Players.All.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                                if (target != null)
                                {
                                    Account acc = EPlayer.GetAccount(target);
                                    if (acc.JailTime == 0) { context.Channel.SendMessageAsync("[!] Target is not jailed."); return; }

                                    Logs.LogAdmin(admID, $"Unjailed player {target.Name} who had {acc.JailTime} seconds remaining in jail.");
                                    acc.JailTime = 0;
                                    target.SetSharedData("JailTime", 0);
                                    EPlayer.Freeze(target, false);

                                        //// try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:unjail:{target.Name}"); } catch { }             
                                        EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] [DC] {context.Author.Username} has unjailed {target.Name}.");
                                    EPlayer.Spawn(target);

                                    context.Channel.SendMessageAsync($":white_check_mark: **Successfully unjailed {name}!**");
                                }
                                else
                                {
                                    Task.Run(async () =>
                                    {
                                        using (MainDatabase dbcontext = ContextFactory.CreateContext())
                                        {
                                            Account tpAcc = await dbcontext.Accounts.AsQueryable().FirstOrDefaultAsync(p => p.Username.Equals(name, StringComparison.OrdinalIgnoreCase) == true);

                                            if ((tpAcc != null) == false)
                                            {
                                                await context.Channel.SendMessageAsync("[!] There's no account with that name.");
                                                return;
                                            }

                                                //// try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:ounjail:{tpAcc.Username}"); } catch { }
                                                Logs.LogAdmin(admID, $"Unjailed offline player {tpAcc.Username} who had {tpAcc.JailTime} seconds remaining in jail.");
                                            tpAcc.JailTime = 0;

                                            await dbcontext.SaveChangesAsync();

                                            NAPI.Task.Run(() =>
                                            {
                                                EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] [DC] {context.Author.Username} has offline unjailed {tpAcc.Username}.");
                                            });

                                            await context.Channel.SendMessageAsync($":white_check_mark: **Successfully offline-unjailed {name}!**");
                                        }
                                    });

                                }
                            }, 10);
                        }
                    }
                    else await context.Channel.SendMessageAsync("**[!]** !unjail [name]");
                }
                else if(context.Content.StartsWith("!reloaddb"))
                {
                    int admID = await GetIngameSQLID(context.Author);
                    if (admID == -1)
                    {
                        await context.Channel.SendMessageAsync("**[!]** For security reasons, you must link your Discord account to your RnC account to use this!");
                        await context.Channel.SendMessageAsync("**HINT:** /verify");
                        return;
                    }
                    int aLevel = GetIngameAdminLevel(context.Author);
                    if(aLevel < 6)
                    {
                        await context.Channel.SendMessageAsync("**[!]** You are not authorized to use this!");
                        return;
                    }
                    await Task.Run(async () =>
                    {
                        using(MainDatabase dbcontext = ContextFactory.CreateContext())
                        {
                            long startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                            await dbcontext.Accounts.ForEachAwaitAsync(async (account) =>
                            {
                                await dbcontext.Entry(account).ReloadAsync();
                            });
                            long endTime = DateTimeOffset.Now.ToUnixTimeMilliseconds()-startTime;
                            await context.Channel.SendMessageAsync($"**[>]** Reload finished! ({endTime}ms)");
                        }
                    });
                }
                else if (context.Content.StartsWith("!jail"))
                {
                    string[] args = context.Content.Split(" ", 4);
                    if (args.Count() >= 4)
                    {
                        string targetName = args[1];
                        string minsSTR = args[2];
                        string reason = args[3];

                        int mins = 0;
                        try
                        {
                            mins = int.Parse(minsSTR);
                        }
                        catch
                        {
                            await context.Channel.SendMessageAsync("**[!]** Minutes must be a number!");
                            return;
                        }

                        if (mins < 1)
                        {
                            await context.Channel.SendMessageAsync("**[!]** Jail time must be at least 1 minute! *Use /unjail to unjail a player.*");
                            return;
                        }

                        int admID = await GetIngameSQLID(context.Author);
                        lock (RAGE.Entities.Players)
                        {
                            NAPI.Task.Run(() =>
                            {
                                if (admID == -1)
                                {
                                    context.Channel.SendMessageAsync("**[!]** For security reasons, you must link your Discord account to your RnC account to use this!");
                                    context.Channel.SendMessageAsync("**HINT:** /verify");
                                    return;
                                }
                                int aLevel = GetIngameAdminLevel(context.Author);

                                Player target = RAGE.Entities.Players.All.FirstOrDefault(p => p.Name.Equals(targetName, StringComparison.OrdinalIgnoreCase));
                                if (target != null)
                                {
                                    Account tAcc = EPlayer.GetAccount(target);
                                    if (tAcc.Admin >= aLevel)
                                    {
                                        context.Channel.SendMessageAsync("**[!]** Target is a higher or equal admin level than you!");
                                        return;
                                    }

                                    RAGE.Entities.Players.All.ForEach((Player playa) =>
                                    {
                                        playa.SendChatMessage(Translator.Translate(playa, "JAILED_MSG", context.Author.Username, target.Name, mins, reason));
                                    });
                                    Logs.LogAdmin(admID, $"Jailed player {target.Name} for {mins} minutes, reason: {reason}.");
                                    EPlayer.Jail(target, mins, reason);
                                    Logs.AddToAdminRecord(tAcc.SQL_ID, admID, "Jail", reason, mins + " minutes.");

                                    context.Channel.SendMessageAsync($":white_check_mark: **Successfully jailed {targetName} for {reason}!**");
                                }
                                else
                                {
                                    Task.Run(async () =>
                                    {
                                        using (MainDatabase dbcontext = ContextFactory.CreateContext())
                                        {
                                            Account tpAcc = await dbcontext.Accounts.AsQueryable().FirstOrDefaultAsync(a => a.Username.Equals(targetName, StringComparison.OrdinalIgnoreCase));
                                            if (tpAcc == null)
                                            {
                                                await context.Channel.SendMessageAsync("**[!]** That player does not exist in the database!");
                                                return;
                                            }

                                            Logs.LogAdmin(admID, $"Offline-jailed {tpAcc.Username} for {mins} minutes, reason: {reason}");
                                            Logs.AddToAdminRecord(tpAcc.SQL_ID, admID, "Jail", "(OFF) " + reason, mins + " minutes.");

                                            tpAcc.JailReason = reason;
                                            tpAcc.JailTime = tpAcc.JailTime + (mins * 60);

                                            await dbcontext.SaveChangesAsync();

                                            NAPI.Task.Run(() =>
                                            {
                                                RAGE.Entities.Players.All.ForEach((Player playa) =>
                                                {
                                                    playa.SendChatMessage(Translator.Translate(playa, "JAILED_MSG", context.Author.Username, tpAcc.Username, mins, reason));
                                                });
                                            });

                                            await context.Channel.SendMessageAsync($":white_check_mark: **Successfully offline-jailed {targetName} for {reason}!**");
                                        }
                                    });

                                }

                            }, 10);
                        }
                    }
                    else await context.Channel.SendMessageAsync("**[USAGE]** !jail [username] [minutes] [reason]");
                }
                /* Fuck it, cba to edit this, they will buy a namechange
                else if (context.Content.StartsWith("!changename"))
                {
                    string[] args = context.Content.Split(" ", 3);
                    if (args.Count() >= 3)
                    {
                        string name = args[1];
                        string newname = args[2];
                        int aLevel = GetIngameAdminLevel(context.Author);
                        lock (RAGE.Entities.Players)
                        {
                            NAPI.Task.Run(() =>
                            {
                                if (aLevel < 4)
                                {
                                    context.Channel.SendMessageAsync("**[!]** You cannot use this command.");
                                    return;
                                }


                                Account tpAcc = ContextFactory.Instance.Accounts.ToList().FirstOrDefault(p => p.Username.Equals(name, StringComparison.OrdinalIgnoreCase) == true);

                                if (tpAcc == null)
                                {
                                    context.Channel.SendMessageAsync("**[!]** No account with this name could be found!");
                                    return;
                                }

                                if (ContextFactory.Instance.Accounts.ToList().FirstOrDefault(p => p.Username.Equals(newname, StringComparison.OrdinalIgnoreCase) == true) != null)
                                {
                                    context.Channel.SendMessageAsync("**[!]** An account with this name already exists!");
                                    return;
                                }

                                if (tpAcc.Admin > aLevel)
                                {
                                    context.Channel.SendMessageAsync("**[!]** Target is a higher admin level than you!");
                                    return;
                                }

                                    foreach (var arena in ContextFactory.Instance.CustomArenas.ToList().Where(a => a.Creator.Equals(name, StringComparison.OrdinalIgnoreCase)))
                                    {
                                        arena.Creator = newname;
                                    }
                                foreach (var arena in ContextFactory.Instance.Bans.ToList().Where(a => a.Admin.Equals(name, StringComparison.OrdinalIgnoreCase)))
                                {
                                    arena.Admin = newname;
                                }

                                Player target = RAGE.Entities.Players.All.FirstOrDefault(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase) == true && EPlayer.LoggedIn(a));
                                if (target != null)
                                {
                                    target.SendChatMessage("!{#FF6347} Reconnect with your new name: !{#FFFFFF}" + newname);
                                    EPlayer.Notify(target, $"Please reconnect with your new name: {newname}", "green", "fas fa-check", false, 3000, "center", "replace");

                                    EPlayer.Save(target);
                                    target.ResetData("AccountHandle");
                                    target.ResetData("LoggedIn");

                                    NAPI.Task.Run(() => { try { target.KickSilent(); } catch { } }, 1000);
                                }

                                    try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:changename:{name}:{newname}"); } catch { }
                                    context.Channel.SendMessageAsync($"**[ADMNAME]** <@{context.Author.Id}> , you changed {name}'s name to {newname}!");

                                EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] !{{#FFFFFF}}[DC] {context.Author.Username} has changed {tpAcc.Username}'s name to {newname}.");

                                tpAcc.Username = newname;
                                ContextFactory.Instance.SaveChanges();
                                return;
                            }, 10);
                        }
                    }
                    else await context.Channel.SendMessageAsync("USAGE: !changename (name) (newname)");
                }
                */

                if (context.Content.StartsWith("!o "))
                {
                    string[] args = context.Content.Split(" ", 2);
                    if (args.Count() >= 2)
                    {
                        NAPI.Task.Run(() =>
                        {
                            EPlayer.SendMessageToAllEx($"(( [DOOC] !{{#FF6347}}{context.Author.Username}!{{#FFFFFF}}: {args[1]} ))");
                            context.Channel.SendMessageAsync($"**(DOOC) | {context.Author.Username}**: {args[1]}");
                        }, 100);
                    }
                    else await context.Channel.SendMessageAsync("USAGE: !o (text)");
                }
                if (context.Content.StartsWith("!cmds"))
                {
                    var builder = new EmbedBuilder()
                        .WithTitle("**Command & Information**")
                        .WithDescription("```\n```")
                        .WithColor(255, 0, 0)
                        .WithFooter(footer =>
                        {
                            footer
                                .WithText("List");
                        })
                        .WithAuthor(author =>
                        {
                            author
                                .WithName("Help")
                                .WithIconUrl("https://i.imgur.com/pc6pzkq.png");
                        });


                    builder.AddField("cmds", "Displays list of all the commands");
                    builder.AddField("o", "Sends a message in OOC chat");
                    builder.AddField("a", "Sends a message in ADM chat");
                    builder.AddField("players", "Gives a list of all the players online");
                    builder.AddField("admins", "Gives a list of all the admins online");
                    builder.AddField("lobbies", "Reports back lobbies and their statuses.");
                    builder.AddField("getinfo", "Reports back information about a user.");
                    builder.AddField("mute", "Mutes a player. | Works for both offline and online players.");
                    builder.AddField("unmute", "Unmutes a player. | Works for both offline and online players.");
                    builder.AddField("jail", "Jails a player. | Works for both offline and online players.");
                    builder.AddField("unjail", "Unjails a player. | Works for both offline and online players.");
                    builder.AddField("lookup", "Look up a player by their E-mail or Rockstar Username. (trial+)");
                    builder.AddField("changepass", "Change a player's password. (trial+)");
                    builder.AddField("ban", "Bans a player. (trial+)");
                    builder.AddField("oban", "Bans an offline player. (trial+)");
                    builder.AddField("unban", "Bans a player. (trial+)");
                    builder.AddField("kick", "Kicks a player.");
                    builder.AddField("setstaff", "Sets a player's admin level, senior admins+ can use this. | Works for both offline and online players.");

                    var embed = builder.Build();

                    await context.Channel.SendMessageAsync(
                        "Reporting...",
                        embed: embed)
                        .ConfigureAwait(false);
                }
              
            }
            catch { }

            return;
        }

        public async Task MainAsync()
        {
            if (Settings.Dynamics.Discord)
            {
                _client = new DiscordSocketClient();

                _client.Log += Log;
                _client.MessageReceived += MessageReceived;

                var token = Settings.DCBotToken;
                UpCooldown = Environment.TickCount64-30000;

                await _client.LoginAsync(TokenType.Bot, token);
                await _client.StartAsync();
            }
        }
    }
}
