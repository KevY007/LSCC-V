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
            Console.WriteLine("[DISCORD] " + msg.ToString());
            return Task.CompletedTask;
        }

        private Task Log(string msg)
        {
            Console.WriteLine("[DISCORD] " + msg);
            return Task.CompletedTask;
        }

        public static Task SendTextChannel(string channel, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null)
        {
            if (_client.ConnectionState != ConnectionState.Connected) return Task.CompletedTask;

            if(text != null)
            {
                text = "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + text;

                text = Regex.Replace(text, @"\!\{(.*?)}|\!\{(.*?)}", "");
            }

            foreach (var guild in Discord._client.Guilds.Where(a => a.Id == Settings.DCServerID))
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
            if (context.Channel.Name != "staff-bot") return Task.CompletedTask;

            if(context.Content.StartsWith("!players"))
            {
                if (RAGE.Entities.Players.Count < 1)
                {
                    context.Channel.SendMessageAsync(":negative_squared_cross_mark: No players are online!");
                    return Task.CompletedTask;
                }

                var builder = new EmbedBuilder()
                    .WithTitle("**Lobby/Mode | Player | Level**")
                    .WithDescription("```\n```")
                    .WithColor(255, 0, 0)
                    .WithFooter(footer => {
                        footer
                            .WithText("Online List");
                    })
                    .WithAuthor(author => {
                        author
                            .WithName("Players")
                            .WithIconUrl("https://i.imgur.com/nJPbaL7.png");
                    });

                foreach (Player pl in RAGE.Entities.Players.All)
                {
                    string lobby = "Lobby " + pl.GetSharedData<int>("InLobby");
                    switch (pl.GetSharedData<int>("InLobby"))
                    {
                        case -1:
                            lobby = "LOGIN";
                            break;
                        case -2:
                            lobby = "JAIL";
                            break;
                        case -3:
                            lobby = "FREEROAM";
                            break;
                        case -4:
                            lobby = "DERBY";
                            break;
                        case -5:
                            lobby = "CUSTOMIZER";
                            break;
                    }

                    builder.AddField("|", lobby, true);
                    builder.AddField("|", pl.Name, true);
                    builder.AddField("|", pl.GetSharedData<int>("Level").ToString(), true);
                }

                var embed = builder.Build();

                context.Channel.SendMessageAsync(
                    "Reporting...",
                    embed: embed)
                    .ConfigureAwait(false);
            }
            return Task.CompletedTask;
        }

        public async Task MainAsync()
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
