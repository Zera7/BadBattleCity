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
            "init",     // Player creation
            "upd"       // Update map
            };
        // /Server commands
        public static bool isRunning = false;
        public static int numberOfPlayers;
        public static Connector connector = new Connector(new IPEndPoint(IPAddress.Broadcast, Client.Port), Port);

        public static Dictionary<IPEndPoint, Player> players = new Dictionary<IPEndPoint, Player>();
        public static List<Bullet>[] bullets = new List<Bullet>[Game.NumberOfTeams];

        public static List<Map.Point> spawners = new List<Map.Point>();
        public static List<Player>[] playersReadyToRespawn = new List<Player>[Game.NumberOfTeams];
        static StringBuilder mapUpdate = new StringBuilder();

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

        #region PrepareServerData

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

        #endregion

        #region ClientCommandProcessing

        #endregion

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


        

        private static void StartGameCycle()
        {
            Game.isStarted = true;
            DateTime time = DateTime.Now;
            while (Game.isStarted)
            {
                CreatePlayers();
                ExecuteClientsCommands();
                CheckAllBulletsForCollisions();
                RemoveDeadBullets();

                SendMessageToAllClients(mapUpdate.ToString());
                mapUpdate.Clear();




                Thread.Sleep(
                    Math.Max(Game.GameSpeed - (DateTime.Now.Millisecond + DateTime.Now.Second * 1000) - 
                    time.Millisecond - time.Second * 1000,
                    0));
                time = DateTime.Now;
            }
        }

        private static void CheckAllBulletsForCollisions()
        {
            for (int i = 0; i < bullets.Length; i++)
            {
                for (int j = 0; j < bullets[i].Count; j++)
                {

                }
            }
                //Дописать метод проверки столкновений для динамических объектов
        }

        private static void RemoveDeadBullets()
        {
            for (int i = 0; i < bullets.Length; i++)
            {
                for (int j = 0; j < bullets[i].Count; j++)
                {
                    if (!bullets[i][j].isAlive)
                        bullets[i].RemoveAt(j--);
                }
            }
        }

        private static void MovePlayer(string[] message, Player player)
        {
            if (Map.Field[player.newCoords.Y, player.newCoords.X] == Map.Cells.empty)
                if (Map.movableObjects[player.newCoords.Y, player.newCoords.X] == null ||
                    Map.movableObjects[player.newCoords.Y, player.newCoords.X].GetType().Name == "Bullet")
                {
                    Map.movableObjects[player.coords.Y, player.coords.X] = null;
                    Map.movableObjects[player.newCoords.Y, player.newCoords.X] = player;

                    player.coords.X = player.newCoords.X;
                    player.coords.Y = player.newCoords.Y;
                }
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
                case "move":
                    MovePlayer(message, player);
                    break;
                case "shot":
                    AddBullet(player);
                    break;
            }
        }

        private static void AddBullet(Player player)
        {
            int deltaX = 0;
            int deltaY = 0;
            if (player.direction == Game.Direction.left) deltaX = -1;
            if (player.direction == Game.Direction.right) deltaX = 1;
            if (player.direction == Game.Direction.up) deltaY = -1;
            if (player.direction == Game.Direction.down) deltaY = 1;

            if (Map.Field[player.coords.Y + deltaY, player.coords.X + deltaX] !=)
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
