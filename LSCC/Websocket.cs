using GTANetworkAPI;
using GTANetworkMethods;
using System;
using System.Collections.Generic;
using System.Text;
using WebSocketSharp.Server;
using WebSocketSharp;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Threading.Tasks;
using static RNC.Functions;

namespace RNC
{
    internal class Websocket : Script
    {
        public class Request
        {
            public string path { get; set; }
        }

        public class GetPlayersData : Request
        {
            public string? name { get; set; }
            public int? level { get; set; }
        }
        private static string getPlayers(GetPlayersData req)
        {
            Console.WriteLine($"[Websocket] getPlayers called name: {req.name} | level: {req.level} | next level: {req.level + 1}");
            return "Pass!";
        }

        public class Default : WebSocketBehavior
        {
            protected override void OnMessage(MessageEventArgs messageEvent)
            {
                Request req = parseJson<Request>(messageEvent.Data);
                if (req == null || req.path == null) return;

                switch (req.path)
                {
                    case "getPlayers":
                        Send(getPlayers(parseJson<GetPlayersData>(messageEvent.Data)));
                        break;

                    case "getPlayer":
                        Send("Test player");
                        break;

                }

            }
        }

        public static void Start()
        {
            WebSocketServer webSocketServer = new WebSocketServer("ws://127.0.0.1:11082");

            // Services
            webSocketServer.AddWebSocketService<Default>("/");

            webSocketServer.Start();
            Console.WriteLine("[Websocket] Websocket started on port: 11082");
        }
    }
}
