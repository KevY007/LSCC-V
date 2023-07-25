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

        public async Task Start()
        {
            client = new DiscordSocketClient();
            client.Log += Log;

            await client.LoginAsync(TokenType.Bot, Settings.DCBotToken);
            await client.StartAsync();

            client.Ready += () =>
            {
                Console.WriteLine($"[Discord] Bot started and logged in as {client.CurrentUser.Username}");
                return Task.CompletedTask;
            };
        }
    }
}

