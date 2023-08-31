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
            commands.Add(new CommandDefinition(new SlashCommandBuilder().WithName("ping").WithDescription("Ping the server").Build(), PingCommandHandler));

            await guild.BulkOverwriteApplicationCommandAsync(commands.Select(cmd => cmd.Meta).ToArray());
        }

        // Commands
        private static async Task PingCommandHandler(SocketSlashCommand command)
        {
            await command.RespondAsync("Pong!");
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

