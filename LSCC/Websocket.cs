using GTANetworkAPI;
using GTANetworkMethods;
using System;
using System.Collections.Generic;
using System.Text;
using WebSocketSharp.Server;
using WebSocketSharp;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using static RNC.Websocket;
using System.Threading.Tasks;

namespace RNC
{
    internal class Websocket : Script
    {

        public static async Task<Account> getAccountByUsername(string username)
        {
            using (MainDatabase dbcontext = ContextFactory.CreateContext())
            {
                Account account = await dbcontext.Accounts.AsQueryable().FirstOrDefaultAsync(a => a.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

                return account;
            }
        }

        public static T parseJson<T>(string jsonString)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(jsonString);
            }
            catch
            {
                return default(T);
            }
        }

        public class getAccount : WebSocketBehavior
        {
            protected override async void OnMessage(MessageEventArgs message)
            {

                if (message.Data == null || message.Data.Length < 1 || message.Data.Length > 128)
                {
                    Send("Invalid input!");
                    return;
                }

                using (MainDatabase dbcontext = ContextFactory.CreateContext())
                {
                    Account account = await dbcontext.Accounts.AsQueryable().FirstOrDefaultAsync(a => a.Username.Equals(message.Data, StringComparison.OrdinalIgnoreCase));

                    if (account == null)
                    {
                        Send("Account not found!");
                        return;
                    }

                    Send($"Username: {account.Username} | Level: {account.Level}");
                }
            }
        }

        public static void Start()
        {
            WebSocketServer webSocketServer = new WebSocketServer("ws://127.0.0.1:11082");

            // Services
            webSocketServer.AddWebSocketService<getAccount>("/getAccount");

            webSocketServer.Start();
            Console.WriteLine("[Websocket] Websocket started on port: 11082");
        }
    }
}
