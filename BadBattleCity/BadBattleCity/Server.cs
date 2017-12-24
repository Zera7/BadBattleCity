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
            "map",      // Sending a map
            "tick",     // Command to start the next iteration
            "init"      // Player creation
            };
        // /Server commands
        public static bool isRunning = false;
        public static int numberOfPlayers;
        public static Connector connector = new Connector(new IPEndPoint(IPAddress.Broadcast, Client.Port), Port);

        public static Dictionary<IPEndPoint, Player> players = new Dictionary<IPEndPoint, Player>();
        public static List<Map.Point> spawners = new List<Map.Point>();
        public static List<Player>[] playersReadyToRespawn = new List<Player>[Game.NumberOfTeams];

        public static void Start()
        {
            connector.Start();
            CreateLobby();
            Map.DownloadMap();
            Map.movableObjects = new MovableObject[Map.MapWidth, Map.MapWidth];
            SendMessageToAllClients("map" + " " + Map.GetStringMap());
            CreateListOfSpawners();
            CreateDictionaryOfPlayers();
            InitPlayers();
            StartGameCycle();
        }

        private static void InitPlayers()
        {
            for (int i = 0; i < connector.Clients.Count; i++)
            {
                connector.Send("init " + players[connector.Clients[i]].team + " " +
                    players[connector.Clients[i]].coords.X + " " +
                    players[connector.Clients[i]].coords.Y
                    , connector.Clients[i]);
            }
        }

        private static void CreateListOfSpawners()
        {
            for (int i = 0; i < Map.MapWidth; i++)
            {
                for (int j = 0; j < Map.MapWidth; j++)
                {
                    if (Array.IndexOf(Map.spawners, (char)Map.Field[i, j]) >= 0)
                        spawners.Add(new Map.Point(j, i));
                }
            }
        }

        private static void CreateDictionaryOfPlayers()
        {
            for (int i = 0; i < connector.Clients.Count; i++)
            {
                players.Add(connector.Clients[i],
                    new Player(i % Game.NumberOfTeams, spawners[i % Game.NumberOfTeams]));
            }
        }

        private static void StartGameCycle()
        {
            Game.isStarted = true;
            DateTime time = DateTime.Now;
            while (Game.isStarted)
            {
                CreatePlayers();
                ExecuteClientsCommands();
                for (int i = 0; i < Game.NumberOfTeams; i++)
                    UpdateClientsData(MoveObjects(i));




                Thread.Sleep(
                    Math.Max(Game.GameSpeed - (DateTime.Now.Millisecond + DateTime.Now.Second * 1000) - 
                    time.Millisecond - time.Second * 1000,
                    0));
                time = DateTime.Now;
            }
        }

        private static void UpdateClientsData(string message)
        {
            for (int i = 0; i < players.Count; i++)
            {
                
            }
        }

        private static string MoveObjects(int team)
        {
            return "";
        }

        private static void ExecuteClientsCommands()
        {
            for (int i = 0; i < connector.AllMessages.Count;)
            {
                string[] message = Encoding.UTF8.GetString(connector.SyncReceive().Message).Split(' ');
                SelectAction(message, players[connector.AllMessages[0].Address]);
                connector.AllMessages.RemoveAt(0);
            }
        }

        private static void SelectAction(string[] message, Player player)
        {
            switch (message[0])
            {
                case "res":
                    playersReadyToRespawn[player.team].Add(player);
                    break;
            }
        }

        private static void CreatePlayers()
        {
            for (int i = 0; i < spawners.Count; i++)
            {
                if (Map.movableObjects[spawners[i].X, spawners[i].Y] == null &&
                     playersReadyToRespawn[i] != null &&
                    playersReadyToRespawn[i].Count > 0)
                {
                    Map.movableObjects[spawners[i].X, spawners[i].Y] = playersReadyToRespawn[i][0];
                }
            }
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
