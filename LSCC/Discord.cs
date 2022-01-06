using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GTANetworkAPI;
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

        public static int GetIngameSQLID(SocketGuildUser user)
        {
            if (user == null) return -1;
            Account _a = ContextFactory.Instance.Accounts.FirstOrDefault(a => a.DiscordInfo.Contains(user.Id.ToString()));
            if (_a == null) return -1;
            return _a.SQL_ID;
        }

        public static int GetIngameAdminLevel(SocketGuildUser user)
        {
            if (user == null) return 0;
            int level = 0;
            var role = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Id == 714822199881760798); // mod
            if (user.Roles.Contains(role))
            {
                level = 1;
            }
            role = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Id == 714821592701599814); // trial
            if (user.Roles.Contains(role))
            {
                level = 2;
            }
            role = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Id == 800001726043127848); // general
            if (user.Roles.Contains(role))
            {
                level = 3;
            }
            role = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Id == 713734312335376465); // senior
            if (user.Roles.Contains(role))
            {
                level = 4;
            }
            role = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Id == 886616979912421387); // lead
            if (user.Roles.Contains(role))
            {
                level = 5;
            }
            role = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Id == 777641183512887316); // owner/management
            if (user.Roles.Contains(role))
            {
                level = 6;
            }
            role = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Id == 713798856680276079); // owner/management
            if (user.Roles.Contains(role))
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

            if(text != null)
            {
                text = "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + text;

                text = Regex.Replace(text, @"\!\{(.*?)}|\!\{(.*?)}", "");
            }

            foreach (var guild in Discord._client.Guilds.Where(a => a.Id == Settings.Dynamics.DCServerID))
            {
                var botchan = guild.TextChannels.First(a => a.Name == channel);

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
                    await Task.Run(async () => { // WE WANT THE CODE TO EXECUTE SEPARATELY AND NOT BLOCK MESSAGE RECEIVED SO DONT AWAIT THIS SH
                        var msg = await context.Channel.SendMessageAsync($":white_check_mark: Server is online with **{RAGE.Entities.Players.Count} players** and **{Settings.Lobbies.Where(l => l.gameStatus == 1).Count()} copchases** active.", false, null, null, null);
                        await Task.Delay(5000);
                        await msg.DeleteAsync();
                    });

                    UpCooldown = Environment.TickCount64;

                    return;
                }

                if (context.Channel.Name != "staff-bot") return;

                if (context.Content.StartsWith("!verify"))
                {
                    // first ID is the bot, rest are codename rotx kevy
                    if (context.Author.Id != 922461597866295397 && context.Author.Id != 367360197900894209 && context.Author.Id != 253938879554387969
                        && context.Author.Id != 319893828142366720) return;
                    string[] args = context.Content.Split(' ');
                    if (args.Count() >= 5)
                    {
                        await context.DeleteAsync();
                        NAPI.Task.Run(() => {
                            try
                            {
                                string vName = args[1];
                                string vCode = args[2];
                                string vID = args[3];
                                //string msgID = args[4];
                                string chanID = args[4];
                                Account _acc = ContextFactory.Instance.Accounts.FirstOrDefault(a => a.Username.Equals(vName, StringComparison.OrdinalIgnoreCase));
                                if (_acc == null)
                                {
                                    context.Channel.SendMessageAsync($"!kuracback false {chanID} {vID} An account with that name could not be found!");
                                    return;
                                }
                                DiscordData _d = NAPI.Util.FromJson<DiscordData>(_acc.DiscordInfo);
                                if (!_d.DiscordID.Equals("n/a")) // if already verified just in case to return
                                {
                                    context.Channel.SendMessageAsync($"!kuracback false {chanID} {vID} An account with that name is already linked to <@{_d.DiscordID}>!");
                                    return;
                                }
                                Player _p = Settings.LoggedInPlayers.FirstOrDefault<Player>(p => p.Name.Equals(vName, StringComparison.OrdinalIgnoreCase));
                                if (_p == null) // if the player is not online or logged in
                                {
                                    context.Channel.SendMessageAsync($"!kuracback false {chanID} {vID} You must be in-game and logged in to link your account!");
                                    return;
                                }
                                if (!_d.DiscordCode.Equals(vCode))
                                {
                                    context.Channel.SendMessageAsync($"!kuracback false {chanID} {vID} The code you have entered is invalid! Please use **(/verify)** in-game to check your code.");
                                    return;
                                }
                                Account _check = ContextFactory.Instance.Accounts.FirstOrDefault<Account>(a => a.DiscordInfo.Contains($"\"DiscordID\":\"{vID}\""));
                                if(_check != null)
                                {
                                    context.Channel.SendMessageAsync($"!kuracback false {chanID} {vID} This Discord account was already used to verify account **{_check.Username}**!");
                                    return;
                                }

                                _d.DiscordID = vID;
                                _d.LeftDiscord = false;
                                _d.UsedDiscordPerks = true;

                                _acc.DiscordInfo = NAPI.Util.ToJson(_d);

                                _p.SendChatMessage("!{#83ff75}Congratulations! You successfully linked your Discord account to your Race 'N' Chase account.");
                                _p.SendChatMessage("!{#ffff00}[>] !{#ffffff}You now have access to the !{#ffff00}Negotiator !{#ffffff}loadout for !{#0097ff}cops !{#ffffff}& !{#ffae00}fugitives!{#ffffff}.");
                                _p.SendChatMessage("!{#ffff00}[>] !{#ffffff}You unlocked a new !{#ffae00}fugitive !{#ffffff}vehicle: !{#ffff00}Super Diamond!{#ffffff}.");
                                _p.SendChatMessage("!{#ffff00}[>] !{#ffffff}You also received !{#ffff00}13 points & 10XP !{#ffffff}as a bonus.");

                                EPlayer.GrantXP(_p, 10);
                                EPlayer.GivePoints(_p, 13);
                                EPlayer.UnlockLoadout(_p, "Negotiator", 0);
                                EPlayer.UnlockLoadout(_p, "Negotiator", 1);
                                EPlayer.UnlockVehicle(_p, "superd", 0);
                                _p.TriggerEvent("PlayMissionComplete", "FRANKLIN_BIG_01");

                                ContextFactory.Instance.SaveChanges(); 
                                context.Channel.SendMessageAsync($"!kuracback true {chanID} {vID} You successfully linked your Discord account to your Race 'N' Chase account!");
                                return;
                            }
                            catch { }
                        }, 50);
                    }
                    else return;
                }

                if (context.Content.StartsWith("!getinfo"))
                {
                    string[] args = context.Content.Split(" ", 2);
                    if (args.Count() >= 2)
                    {
                       Account _acc = ContextFactory.Instance.Accounts.FirstOrDefault(a => a.Username.Equals(args[1], StringComparison.OrdinalIgnoreCase));
                       if(_acc == null)
                        {
                            await context .Channel.SendMessageAsync(":exclamation: **Account with that username does not exist!**");
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
                                            .WithIconUrl("https://i.imgur.com/nJPbaL7.png");
                                    });

                                int thisCount = 0;
                                int proc = 0;
                                foreach (Player pl in RAGE.Entities.Players.All)
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

                                    builder.AddField(pl.Name, lobby + " | Level: " + pl.GetSharedData<int>("Level").ToString() + " | Ping: " + pl.Ping.ToString(), true);

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
                                        .WithIconUrl("https://i.imgur.com/nJPbaL7.png");
                                });

                        foreach (LobbyData p in Settings.Lobbies)
                        {
                            builder.AddField(p.lobbyId.ToString(), RNC.Main.ConvertGameStatus(p.gameStatus, false) + " | " + Settings.LoggedInPlayers.Count(a => a.GetSharedData<int>("InLobby") == p.lobbyId).ToString() + " Players", true);
                        }

                        var embed = builder.Build();

                        context.Channel.SendMessageAsync("Reporting...", embed: embed).ConfigureAwait(false);
                    }
                }

                if (context.Content.StartsWith("!admins"))
                {
                    lock (Settings.LoggedInPlayers)
                    {
                        if (Settings.LoggedInPlayers.Count(a => EPlayer.GetAccount(a).Admin > 0) < 1)
                        {
                            context.Channel.SendMessageAsync(":negative_squared_cross_mark: No admins are online!");
                            return;
                        }

                        int players = 0;
                        int tries = 0;
                        NAPI.Task.Run(() =>
                        {
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
                                            .WithIconUrl("https://i.imgur.com/nJPbaL7.png");
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
                    int aLevel = GetIngameAdminLevel(context.Author as SocketGuildUser);
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
                        int aLevel = GetIngameAdminLevel(context.Author as SocketGuildUser);
                        if(aLevel < 2)
                        {
                            await context.Channel.SendMessageAsync("You are not authorized to use this!");
                            return;
                        }
                        lock (RAGE.Entities.Players)
                        {
                            NAPI.Task.Run(() =>
                            {
                                BanData ban = null;

                                Account tpAcc = ContextFactory.Instance.Accounts.ToList().Find(p => p.Username.Equals(args[1], StringComparison.OrdinalIgnoreCase) == true);
                                if (tpAcc != null)
                                {
                                    ban = ContextFactory.Instance.Bans.ToList().Find(a => a.AccountSQLID == tpAcc.SQL_ID);
                                }


                                if ((ban != null) == false)
                                {
                                    context.Channel.SendMessageAsync("**There is no ban with that name!**");
                                    return;
                                }

                                string username = "Unregistered Ban";
                                if (ban.AccountSQLID != -1)
                                {
                                    Account pAcc = ContextFactory.Instance.Accounts.ToList().Find(acc => acc.SQL_ID == ban.AccountSQLID);
                                    if (pAcc != null)
                                    {
                                        username = pAcc.Username;
                                    }
                                }

                                // try { Main.Log("admin.txt", $"{DateTime.Now.ToString().Replace(":", "|")}:{player.Name}:unban:{username}"); } catch { }

                                ContextFactory.Instance.Bans.Remove(ban);
                                ContextFactory.Instance.SaveChanges();
                                EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[AdmCmd] !{{#FFFFFF}}[DC] {context.Author.Username} has unbanned {username}.");
                                context.Channel.SendMessageAsync($"<@{context.Author.Id}>, you successfully unbanned {tpAcc.Username}!");
                            }, 10);
                        }
                    }
                    else await context.Channel.SendMessageAsync("USAGE: !unban (name)");
                }
                else if (context.Content.StartsWith("!setstaff"))
                {
                    string[] args = context.Content.Split(" ", 3);
                    if (args.Count() >= 3)
                    {
                        int level = 0;
                        try { level = int.Parse(args[2]); } catch
                        {
                            await context.Channel.SendMessageAsync("*The admin level must be a number!*");
                            return;
                        }

                        int aLevel = GetIngameAdminLevel(context.Author as SocketGuildUser);
                        if(aLevel < 4)
                        {
                            await context.Channel.SendMessageAsync("**You are not authorized to use this command!**");
                            return;
                        }
                        else if(aLevel >= 4 && aLevel < 6)
                        {
                            if(level > 3 || level < 0)
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

                                    target.SetSharedData("Admin", level);
                                    targetAcc.Admin = level;
                                    target.SendChatMessage($"~g~Your admin level has been set to ~y~{level}~g~ by [DC] {context.Author.Username}!");
                                    context.Channel.SendMessageAsync($"[ADMSET] <@{context.Author.Id}> , you successfully set **{target.Name}'s** admin level to **{level}**!");
                                    return;
                                }
                                else
                                {
                                    Account targetAcc = ContextFactory.Instance.Accounts.FirstOrDefault(t => t.Username.Equals(args[1], StringComparison.OrdinalIgnoreCase));
                                    if (targetAcc == null)
                                    {
                                        context.Channel.SendMessageAsync("**[!]** A player with that username could not be found!");
                                        return;
                                    }
                                    if (targetAcc.Admin == 6 || targetAcc.Admin >= aLevel)
                                    {
                                        context.Channel.SendMessageAsync("**[!]** That player is a higher/equal admin level than you!");
                                        return;
                                    }
                                    targetAcc.Admin = level;
                                    context.Channel.SendMessageAsync($"[ADMSET] <@{context.Author.Id}> , you successfully set **{targetAcc.Username}'s** admin level to **{level}**!");
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

                                int alevel = GetIngameAdminLevel(context.Author as SocketGuildUser);
                                int aSQL = GetIngameSQLID(context.Author as SocketGuildUser);
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

                                if (target.Name.StartsWith("KevY")) return;

                                RAGE.Entities.Players.All.ForEach((Player playa) =>
                                {
                                    playa.SendChatMessage($"!{{#FF6347}}AdmCmd: {target.Name} has been kicked by [DC] {context.Author.Username} for {args[2]}.");
                                });

                                EPlayer.Notify(target, $"You have been kicked from the server by [DC] {context.Author.Username} for <p>{args[2]}</p>", "red", "fas fa-ban", true, 30000);
                                target.SendChatMessage("!{#A9C4E4}Server closed the connection.");

                                target.SetData("Kicked", true);
                                NAPI.Task.Run(() => { try { target.KickSilent(); } catch { } }, 1000);
                            }, 10);
                        }
                    }
                    else await context.Channel.SendMessageAsync("USAGE: !kick (name) (reason)");
                }
                else if (context.Content.StartsWith("!ban"))
                {
                    string[] args = context.Content.Split(" ", 3);
                    if (args.Count() >= 3)
                    {
                        Player target = null;
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

                                int alevel = GetIngameAdminLevel(context.Author as SocketGuildUser);
                                int aSQL = GetIngameSQLID(context.Author as SocketGuildUser);
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
                                ContextFactory.Instance.Bans.Add(_new);

                                ContextFactory.Instance.SaveChanges();

                                EPlayer.SetDiscordStatus(target, "Gazing at the 'BANNED' screen.");

                                RAGE.Entities.Players.All.ForEach((Player p) =>
                                {
                                    p.SendChatMessage($"!{{#FF6347}}AdmCmd: [DC] {context.Author.Username} has banned {target.Name} for: {args[2]}");
                                });

                                EPlayer.Notify(target, $"You have been banned from the server.<p>Reason: {args[2]}</p><p>Admin: [DC] {context.Author.Username}</p>", "red", "fas fa-ban", true, 30000);

                                target.SendChatMessage($"!{{#FF6347}}[BANNED] You have been banned from this server, Ban ID: {_new.SQL_ID}");

                                target.SendChatMessage("!{#A9C4E4}Server closed the connection.");

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
                        lock (RAGE.Entities.Players)
                        {
                            NAPI.Task.Run(() =>
                            {
                                if (RAGE.Entities.Players.All.Where(p => p.Name.Equals(args[1], StringComparison.OrdinalIgnoreCase)).Count() > 0)
                                {
                                    context.Channel.SendMessageAsync("**[!]** That player is in-game, use !ban.");
                                    return;
                                }

                                int alevel = GetIngameAdminLevel(context.Author as SocketGuildUser);
                                int aSQL = GetIngameSQLID(context.Author as SocketGuildUser);
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

                                target = ContextFactory.Instance.Accounts.ToList().Find(p => p.Username.Equals(args[1], StringComparison.OrdinalIgnoreCase) == true &&
                                    ContextFactory.Instance.Bans.ToList().Where(a => a.AccountSQLID == p.SQL_ID).Count() == 0);

                                if (target.Admin > alevel)
                                {
                                    context.Channel.SendMessageAsync("**[!]** That player is a higher admin level than you!");
                                    return;
                                }
                                if (target == null)
                                {
                                    context.Channel.SendMessageAsync("**[!]** There's no account with that name or target is already banned (/bancheck).");
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
                                ContextFactory.Instance.Bans.Add(_new);

                                ContextFactory.Instance.SaveChanges();
                                Logs.AddToAdminRecord(target.SQL_ID, aSQL, "Ban", "[DC] " + args[2], "N/A");
                                RAGE.Entities.Players.All.ForEach((Player p) =>
                                {
                                    p.SendChatMessage($"!{{#FF6347}}AdmCmd: [DC] {context.Author.Username} has offline-banned {target.Username} for: {args[2]}");
                                });
                            });
                        }
                    }
                    else await context.Channel.SendMessageAsync("USAGE: !oban (name) (reason)");
                }

                if (context.Content.StartsWith("!o"))
                {
                    string[] args = context.Content.Split(" ", 2);
                    if (args.Count() >= 2)
                    {
                        NAPI.Chat.SendChatMessageToAll($"(( [DOOC] !{{#FF6347}}{context.Author.Username}!{{#FFFFFF}}: {args[1]} ))");
                        await context.Channel.SendMessageAsync($"**(DOOC) | {context.Author.Username}**: {args[1]}");
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
                                .WithIconUrl("https://i.imgur.com/nJPbaL7.png");
                        });


                    builder.AddField("cmds", "Displays list of all the commands");
                    builder.AddField("o", "Sends a message in OOC chat");
                    builder.AddField("a", "Sends a message in ADM chat");
                    builder.AddField("players", "Gives a list of all the players online");
                    builder.AddField("admins", "Gives a list of all the admins online");
                    builder.AddField("lobbies", "Reports back lobbies and their statuses.");
                    builder.AddField("getinfo", "Reports back information about a user.");
                    builder.AddField("ban", "Bans a player. (trial+)");
                    builder.AddField("oban", "Bans an offline player. (trial+)");
                    builder.AddField("unban", "Bans a player. (trial+)");
                    builder.AddField("kick", "Kicks a player.");
                    builder.AddField("setstaff", "Sets a player's admin level, senior admins+ can use this.");

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
