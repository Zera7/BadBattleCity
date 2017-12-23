using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace BadBattleCity
{
    static class Server
    {
        // Server constants
        public const int Port = 15000;
        // /Server constants

        // Server commands
        static public string[] commands = {
            "hi",       // Message about a free server
            "+new",     // Client connection agreement
            "map"       // Sending a map
            };
        // /Server commands

        public static bool isRunning = false;
        public static int numberOfPlayers;
        public static Connector connector = new Connector(new IPEndPoint(IPAddress.Broadcast, Client.Port), Port);

        public static void Start()
        {
            connector.Start();
            CreateLobby();
            Map.DownloadMap();
            SendMessageToAllClients("map" + " " + Map.GetStringMap());
        }

        private static void CreateLobby()
        {
            while (!Game.isStarted)
            {
                if (connector.Clients.Count < numberOfPlayers)
                {
                    connector.Send("hi", connector.SenderDefaultEndPoint);
                    for (int i = 0; i < connector.AllMessages.Count;)
                    {
                        string[] message = Encoding.UTF8.GetString(connector.AllMessages[0].Message).Split(' ');
                        if (message[0] == "new")
                        {
                            connector.Send("+new", connector.AllMessages[0].Address);
                            connector.Clients.Add(connector.AllMessages[0].Address);
                            Console.WriteLine("К серверу добавлен новый клиент:" + connector.AllMessages[0].Address);
                        }
                        connector.AllMessages.RemoveAt(0);

                    }
                }
                else
                {
                    Game.isReadyToStart = true;
                    connector.AllMessages.Clear();
                    break;
                }
                Thread.Sleep(500);
            }
            Console.WriteLine("Лобби собрано");
        }

        private static void SendMessageToAllClients(string message)
        {
            for (int i = 0; i < connector.Clients.Count; i++)
            {
                connector.Send(message, connector.Clients[i]);
            }
        }
    }
}
