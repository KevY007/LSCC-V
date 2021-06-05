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

namespace LSCC
{ 
    class Discord : Script
    {
        public static DiscordSocketClient _client;
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

        public static Task MessageReceived(SocketMessage context)
        {
            try
            {
                if (!Settings.Dynamics.Discord) return Task.CompletedTask;
                if (_client == null) return Task.CompletedTask;
                if (_client.ConnectionState != ConnectionState.Connected) return Task.CompletedTask;

                if (context.Content.Equals("!up") || context.Content.Equals("!status"))
                {
                    context.Channel.SendMessageAsync($":white_check_mark: Server is online with {RAGE.Entities.Players.Count} players.");
                    return Task.CompletedTask;
                }

                if (context.Channel.Name != "staff-bot") return Task.CompletedTask;

                if (context.Content.StartsWith("!players"))
                {
                    if (RAGE.Entities.Players.Count < 1)
                    {
                        context.Channel.SendMessageAsync(":negative_squared_cross_mark: No players are online!");
                        return Task.CompletedTask;
                    }

                    int players = 0;
                    int tries = 0;
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
                if (context.Content.StartsWith("!admins"))
                {
                    if (Settings.LoggedInPlayers.Count(a => EPlayer.GetAccount(a).Admin > 0) < 1)
                    {
                        context.Channel.SendMessageAsync(":negative_squared_cross_mark: No admins are online!");
                        return Task.CompletedTask;
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
                if (context.Content.StartsWith("!o"))
                {
                    string[] args = context.Content.Split(" ", 2);
                    if (args.Count() >= 2)
                    {
                        NAPI.Chat.SendChatMessageToAll($"(( [DOOC] !{{#FF6347}}{context.Author.Username}!{{#FFFFFF}}: {args[1]} ))");
                        context.Channel.SendMessageAsync($"**(DOOC) | {context.Author.Username}**: {args[1]}");
                    }
                    else context.Channel.SendMessageAsync("USAGE: !o (text)");
                }
                if (context.Content.StartsWith("!a"))
                {
                    string[] args = context.Content.Split(" ", 2);
                    if (args.Count() >= 2)
                    {
                        NAPI.Task.Run(() => EPlayer.SendMessageToAllAdmins($"!{{#FF6347}}[A] (DISCORD) {context.Author.Username}:  {args[1]}"), 10);
                    }
                    else context.Channel.SendMessageAsync("USAGE: !a (text)");
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

                    var embed = builder.Build();

                    context.Channel.SendMessageAsync(
                        "Reporting...",
                        embed: embed)
                        .ConfigureAwait(false);
                }
            }
            catch { }

            return Task.CompletedTask;
        }

        public async Task MainAsync()
        {
            if (Settings.Dynamics.Discord)
            {
                _client = new DiscordSocketClient();

                _client.Log += Log;
                _client.MessageReceived += MessageReceived;

                var token = Settings.DCBotToken;

                await _client.LoginAsync(TokenType.Bot, token);
                await _client.StartAsync();
            }
        }
    }
}
