using Discord;
using Discord.WebSocket;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RNC
{
    public class DiscordBot : Script
    {
        private static Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private static DiscordSocketClient client;
        private static ulong gameChatChannelId = 717768143140814928;
        private static ulong guildId = 713731146072064040;
        private static List<CommandDefinition> commands = new List<CommandDefinition>();

        public static async Task Start()
        {
            client = new DiscordSocketClient();
            //client.Log += Log;

            await client.LoginAsync(TokenType.Bot, Settings.DCBotToken);
            await client.StartAsync();

            client.Ready += async () =>
            {
                Console.WriteLine($"[Discord] Bot started and logged in as {client.CurrentUser.Username}");
                await SendAlert("Server started.");

                await RegisterCommands(client);
            };
            client.SlashCommandExecuted += SlashCommandHandler;
        }
        public enum EmbedType
        {
            Info,
            Warning,
            Error,
            Success
        }
        public static Embed embed(string message, EmbedType type = EmbedType.Info)
        {
            var embed = new EmbedBuilder();
            var emoji = "";
            switch (type)
            {
                case EmbedType.Info:
                    embed.Color = Discord.Color.Blue;
                    emoji = ":blue_circle: ";
                    break;
                case EmbedType.Warning:
                    embed.Color = Discord.Color.Orange;
                    emoji = ":orange_circle: ​ ​ ";
                    break;
                case EmbedType.Error:
                    embed.Color = Discord.Color.Red;
                    emoji = ":red_circle:  ";
                    break;
                case EmbedType.Success:
                    embed.Color = Discord.Color.Green;
                    emoji = ":green_circle:  ";
                    break;
            }
            embed.Description = $"{emoji} \u200B \u200B {message}";
            return embed.Build();
        }
        public static async Task SendMessage(string message)
        {
            var channel = client.GetChannel(gameChatChannelId) as IMessageChannel;
            if (channel == null)
            {
                Console.WriteLine($"[Discord] Game chat channel not found!");
                return;
            }
            await channel.SendMessageAsync(message);
        }

        public static async Task SendAlert(string message, EmbedType type = EmbedType.Info)
        {
            var channel = client.GetChannel(gameChatChannelId) as IMessageChannel;
            if (channel == null)
            {
                Console.WriteLine($"[Discord] Game chat channel not found!");
                return;
            }

            await channel.SendMessageAsync("", false, embed(message, type));
        }
        private static async Task SlashCommandHandler(SocketSlashCommand command)
        {
            var foundCommand = commands.Find(cmd => cmd.Meta.Name.Value == command.Data.Name);

            if (foundCommand != null)
            {
                await foundCommand.Handler(command);
            }
            else
            {
                await command.RespondAsync("", new Embed[] { embed("Unkwnown command!", EmbedType.Error) });
            }
        }
        public static SocketGuild GetGuild(DiscordSocketClient client)
        {
            var guild = client.GetGuild(guildId);

            if (guild == null)
            {
                Console.WriteLine($"[Discord] Guild not found!");
            }

            return guild;
        }
        public static async Task RegisterCommands(DiscordSocketClient client)
        {
            var guild = GetGuild(client);
            if (guild == null) return;

            // Commands
            commands.Add(new CommandDefinition(new SlashCommandBuilder().WithName("players").WithDescription("View online players.").Build(), PlayersCommandHandler));

            await guild.BulkOverwriteApplicationCommandAsync(commands.Select(cmd => cmd.Meta).ToArray());
        }

        // Commands
        private static async Task PlayersCommandHandler(SocketSlashCommand command)
        {
            NAPI.Task.Run(() =>
            {
                List<EmbedBuilder> playerListEmbeds = new List<EmbedBuilder>();

                EmbedBuilder embed = new EmbedBuilder();
                embed.Title = $"Online Players: {RAGE.Entities.Players.Count}";
                embed.Description = "**Lobby/Mode | Level**";
                embed.Color = new Discord.Color(100, 0, 200);
                embed.ThumbnailUrl = "https://media.discordapp.net/attachments/747725642350723172/1149001447946604625/logo.webp";

                playerListEmbeds.Add(embed);

                int count = 0;
                lock (RAGE.Entities.Players)
                {
                    foreach (Player player in RAGE.Entities.Players.All)
                    {
                        if (count >= 6)
                        {
                            count = 0;
                            EmbedBuilder newEmbed = new EmbedBuilder();
                            newEmbed.Color = new Discord.Color(100, 0, 200);
                            playerListEmbeds.Add(newEmbed);
                        }
                        if(!player.HasSharedData("InLobby") || !player.HasSharedData("Team") || !player.HasSharedData("InDM") || !player.HasSharedData("Level"))
                        {
                            playerListEmbeds[playerListEmbeds.Count - 1].AddField(player.Name, "Connecting/Logging in...");
                        }
                        else
                        {
                            int lobbyID = player.GetSharedData<int>("InLobby");
                            int teamID = player.GetSharedData<int>("Team");
                            int dmID = player.GetSharedData<int>("InDM");
                            string modeDisplay = $"Lobby {lobbyID}";
                            if (lobbyID > 0 && teamID != -1) modeDisplay = $"Copchase ({lobbyID})";
                            else if (lobbyID > 0 && dmID != -1) modeDisplay = $"Deathmatch ({lobbyID})";
                            else
                            {
                                switch (lobbyID)
                                {
                                    case -1:
                                        modeDisplay = "Logging in";
                                        break;
                                    case -2:
                                        modeDisplay = "Admin Jail";
                                        break;
                                    case -3:
                                        modeDisplay = "Freeroam";
                                        break;
                                    case -4:
                                        modeDisplay = "Derby";
                                        break;
                                    case -5:
                                        modeDisplay = "Character Creator";
                                        break;
                                }
                            }
                            playerListEmbeds[playerListEmbeds.Count - 1].AddField(player.Name, $"{modeDisplay} | Level {player.GetSharedData<int>("Level")}", true);
                        }

                        count++;
                    }
                }       
                Task.Run(async () =>
                {
                    foreach (EmbedBuilder e in playerListEmbeds)
                    {
                        await command.Channel.SendMessageAsync("", false, e.Build());
                    }
                    await command.RespondAsync("Online player list.");
                });
               
            });
            
        }
    }
    class CommandDefinition
    {
        public ApplicationCommandProperties Meta { get; }
        public Func<SocketSlashCommand, Task> Handler { get; }

        public CommandDefinition(ApplicationCommandProperties meta, Func<SocketSlashCommand, Task> handler)
        {
            Meta = meta;
            Handler = handler;
        }
    }
}

