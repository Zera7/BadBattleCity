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
            "upd",      // Update map
            "updd",     // Update movable map
            "moved"     // Object moved
            };
        // /Server commands
        public static bool isRunning = false;
        public static int numberOfPlayers;
        public static Connector connector = new Connector(new IPEndPoint(IPAddress.Broadcast, Client.Port), Port);

        public static Dictionary<IPEndPoint, Player> players = new Dictionary<IPEndPoint, Player>();
        public static List<Bullet>[] bullets = new List<Bullet>[Game.NumberOfTeams];

        public static List<Map.Point> spawners = new List<Map.Point>();
        public static List<Player>[] playersReadyToRespawn = new List<Player>[Game.NumberOfTeams];
        static StringBuilder staticMapUpdate = new StringBuilder();

        public static StringBuilder[] movedObjects = new StringBuilder[Game.NumberOfTeams];
            

        public static void Start()
        {
            for (int i = 0; i < playersReadyToRespawn.Length; i++)
            {
                playersReadyToRespawn[i] = new List<Player>();
                bullets[i] = new List<Bullet>();
            }

            connector.Start();
            CreateLobby();
            do {
                Map.DownloadMap();
            } while (!CreateListOfSpawners());
            Map.movableObjects = new MovableObject[Map.MapWidth, Map.MapWidth];
            SendMessageToAllClients("map" + " " + Map.GetStringMap());
            CreateDictionaryOfPlayers();
            InitPlayers();
            StartGameCycle();
        }

        #region PrepareGame

        private static void CreateLobby()
        {
            connector.Start();
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


        private static bool CreateListOfSpawners()
        {
            for (int i = 0; i < Map.MapWidth; i++)
            {
                for (int j = 0; j < Map.MapWidth; j++)
                {
                    if (Array.IndexOf(Map.spawners, (char)Map.Field[i, j]) >= 0)
                        spawners.Add(new Map.Point(j, i));
                }
            }
            if (spawners.Count == 0)
                Console.WriteLine("Карта не корректна: не обнаружено точек появления");
            return spawners.Count > 0;
        }

        private static void CreateDictionaryOfPlayers()
        {
            for (int i = 0; i < movedObjects.Length; i++)
                movedObjects[i] = new StringBuilder();

            for (int i = 0; i < connector.Clients.Count; i++)
            {
                players.Add(connector.Clients[i],
                    new Player(i % Game.NumberOfTeams, new Map.Point (spawners[i % Game.NumberOfTeams].X, spawners[i % Game.NumberOfTeams].Y)));
            }
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


        #endregion

        #region ClientCommandProcessing

        private static void MovePlayer(string[] message, Player player, IPEndPoint address)
        {
            Console.SetCursorPosition(0, 0);
            Console.WriteLine("YES" + player.team);
            try
            {
                player.newCoords.X = int.Parse(message[1]);
                player.newCoords.Y = int.Parse(message[2]);

                player.direction = (MovableObject.Direction)int.Parse(message[3]);

                char a = player.GetChar();
                Server.movedObjects[player.team].Append(
                    a + " " + player.coords.X + " " + player.coords.Y + " ");

                if (Map.Field[player.newCoords.Y, player.newCoords.X] == Map.Cells.empty)
                    if (Map.movableObjects[player.newCoords.Y, player.newCoords.X] == null ||
                        Map.movableObjects[player.newCoords.Y, player.newCoords.X].GetType().Name == "Bullet")
                    {
                        Map.movableObjects[player.coords.Y, player.coords.X] = null;
                        Map.movableObjects[player.newCoords.Y, player.newCoords.X] = player;

                        player.coords.X = player.newCoords.X;
                        player.coords.Y = player.newCoords.Y;

                        connector.Send("moved", address);
                    }
                Server.movedObjects[player.team].Append(player.coords.X + " " + player.coords.Y + " ");
            }
            catch (Exception)
            {

            }
        }

        private static void SelectAction(string[] message, Player player, IPEndPoint address)
        {
            switch (message[0])
            {
                case "res":
                    playersReadyToRespawn[player.team].Add(player);
                    break;
                case "move":
                    MovePlayer(message, player, address);
                    break;
                case "shot":
                    AddBullet(player);
                    break;
            }
        }

        private static void ExecuteClientsCommands()
        {
            for (int i = 0; i < connector.AllMessages.Count;)
            {
                string[] message = Encoding.UTF8.GetString(connector.AllMessages[0].Message).Split(' ');
                SelectAction(message, players[connector.AllMessages[0].Address], connector.AllMessages[0].Address);
                connector.AllMessages.RemoveAt(0);
            }
        }

        private static void AddBullet(Player player)
        {
            Map.Point deltaCoords = MovableObject.GetDeltaCoords(player.direction);
            bullets[player.team].Add(
                new Bullet(
                    player.team,
                    player.direction,
                    new Map.Point(player.coords.X + deltaCoords.X, player.coords.Y + deltaCoords.Y)));
        }

        #endregion

        #region GameCycle

        private static void StartGameCycle()
        {
            Game.isStarted = true;
            DateTime time = DateTime.Now;
            while (Game.isStarted)
            {
                RespawnPlayers();
                ExecuteClientsCommands();
                MoveAllBullets();
                RemoveDeadBullets();

                if (staticMapUpdate.Length > 0)
                {
                SendMessageToAllClients("upd " + staticMapUpdate.ToString());
                staticMapUpdate.Clear();
                }

                SendMessageToAllClients("tick");

                UpdateMovableMap();


                Thread.Sleep(Math.Max(Game.GameSpeed - (
                    DateTime.Now.Millisecond + DateTime.Now.Second * 1000 -
                    time.Millisecond - time.Second * 1000
                    ), 0));
                    
                time = DateTime.Now;
            }
        }

        private static void UpdateMovableMap()
        {
            for (int i = 0; i < Game.NumberOfTeams; i++)
                for (int j = 0; j < players.Count; j++)
                    connector.Send("updd " + i + " " + movedObjects[i].ToString(), connector.Clients[j]);

            for (int i = 0; i < movedObjects.Length; i++)
                movedObjects[i].Clear();
        }

        private static void MoveAllBullets()
        {
            for (int i = 0; i < bullets.Length; i++)
                for (int j = 0; j < bullets[i].Count; j++)
                    bullets[i][j].ContinueMovement(staticMapUpdate);
        }

        private static void RemoveDeadBullets()
        {
            for (int i = 0; i < bullets.Length; i++)
            {
                for (int j = 0; j < bullets[i].Count; j++)
                {
                    if (!bullets[i][j].isAlive)
                    {
                        if (Map.Field[bullets[i][j].coords.Y,bullets[i][j].coords.X] == Map.Cells.empty ||
                            Map.Field[bullets[i][j].coords.Y, bullets[i][j].coords.X] == Map.Cells.brick)
                        Server.movedObjects[bullets[i][j].team].Append(
                            "█ " + bullets[i][j].coords.X + " " + bullets[i][j].coords.Y + " " + -1 + " " + -1 + " ");

                        bullets[i].RemoveAt(j--);
                    }
                }
            }
        }

        private static void RespawnPlayers()
        {
            for (int i = 0; i < spawners.Count; i++)
            {
                if (Map.movableObjects[spawners[i].Y, spawners[i].X] == null &&
                    playersReadyToRespawn[i].Count > 0)
                {
                    Map.movableObjects[spawners[i].Y, spawners[i].X] = playersReadyToRespawn[i][0];
                    playersReadyToRespawn[i].RemoveAt(0);
                }
            }
        }

        #endregion

        private static void SendMessageToAllClients(string message)
        {
            for (int i = 0; i < connector.Clients.Count; i++)
            {
                connector.Send(message, connector.Clients[i]);
            }
        }
    }
}
