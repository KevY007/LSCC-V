using Discord;
using Discord.WebSocket;
using GTANetworkAPI;
using System;
using System.Threading.Tasks;

namespace RNC
{
    internal class DiscordBot : Script
    {
        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private DiscordSocketClient client;
        private ulong gameChatChannelId = 717768143140814928;

        public async Task Start()
        {
            client = new DiscordSocketClient();
            //client.Log += Log;

            await client.LoginAsync(TokenType.Bot, Settings.DCBotToken);
            await client.StartAsync();

            client.Ready += async () =>
            {
                Console.WriteLine($"[Discord] Bot started and logged in as {client.CurrentUser.Username}");
                await SendAlert("Server started.");
            };
        }
        public async Task SendMessage(string message)
        {
            var channel = client.GetChannel(gameChatChannelId) as IMessageChannel;
            if (channel == null)
            {
                Console.WriteLine($"[Discord] Game chat channel not found!");
                return;
            }
            await channel.SendMessageAsync(message);
        }
        public async Task SendAlert(string message, string type = "info")
        {
            var channel = client.GetChannel(gameChatChannelId) as IMessageChannel;
            if (channel == null)
            {
                Console.WriteLine($"[Discord] Game chat channel not found!");
                return;
            }

            var embed = new EmbedBuilder();
            var emoji = "";
            switch (type)
            {
                case "info":
                    embed.Color = Discord.Color.Blue;
                    emoji = ":blue_circle: ";
                    break;
                case "warning":
                    embed.Color = Discord.Color.Orange;
                    emoji = ":orange_circle: ​ ​ ";
                    break;
                case "error":
                    embed.Color = Discord.Color.Red;
                    emoji = ":red_circle:  ";
                    break;
                case "success":
                    embed.Color = Discord.Color.Green;
                    emoji = ":green_circle:  ";
                    break;
            }
            embed.Description = $"{emoji} \u200B \u200B {message}";

            await channel.SendMessageAsync("", false, embed.Build());
        }
    }
}

