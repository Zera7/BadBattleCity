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
    static class Client
    {
        // Client constants
        public const int Port = 14000;
        // /Client constants

        // Client commands
        static public string[] commands = {
            "new",      // Request to connect to the server
            "res",      // Respawn request
            "shot"
            };
        // /Client commands

        public static Thread ServerSearch;
        public static bool isConnectedToServer = false;
        public static Connector connector = new Connector(new IPEndPoint(IPAddress.Broadcast, Server.Port), Port);
        public static Player thisPlayer;

        #region ConnectingToTheServer

        public static void StartAsyncServerSearch()
        {
            Console.WriteLine("Начат поиск сервера");
            ServerSearch = new Thread(BeginSearchServer);
            ServerSearch.Start();
        }

        private static void BeginSearchServer()
        {
            while (!isConnectedToServer && !Game.isLobbyExists)
            {
                connector.SyncReceive();
                for (int i = 0; i < connector.AllMessages.Count;)
                {
                    string[] message = Encoding.UTF8.GetString(connector.AllMessages[0].Message).Split(' ');
                    IPEndPoint address = connector.AllMessages[0].Address;
                    connector.AllMessages.RemoveAt(0);
                    if (message[0] == "hi")
                    {
                        connector.Stop();
                        connector = new Connector(address, Port);
                        connector.Send("new", connector.SenderDefaultEndPoint);
                        Console.WriteLine("Сервер найден");
                        Console.WriteLine("Попытка подключения");
                        if (CheckConnection()) break;
                    }
                }
                Thread.Sleep(1000);
            }
            Console.WriteLine("Клиент подключен к серверу");
        }

        private static bool CheckConnection()
        {
            connector.Start();
            for (int k = 0; k < 5; k++)
            {
                for (int i = 0; i < connector.AllMessages.Count; i++)
                {
                    string[] message = Encoding.UTF8.GetString(connector.AllMessages[i].Message).Split(' ');
                    if (message[0] == "+new")
                    {
                        Console.WriteLine("Подключение подтверждено");
                        connector.AllMessages.RemoveAt(i);
                        connector.Stop();
                        connector = new Connector(connector.LastReseivePoint, Port);
                        isConnectedToServer = true;
                        Game.isLobbyExists = true;
                        return true;
                    }
                }
                Thread.Sleep(1000);
            }
            Console.WriteLine("Подключение не подтверждено. Поиск сервера продолжен");
            connector.Stop();
            connector = new Connector(new IPEndPoint(IPAddress.Broadcast, Server.Port), Port);
            return false;
        }

        #endregion

        internal static void StartGame()
        {
            Console.CursorVisible = false;
            while (true)
            {
                string[] message = Encoding.UTF8.GetString(connector.SyncReceive().Message).Split(' ');

                ProcessReceivedCommands(message);

                if (thisPlayer != null)
                {
                    thisPlayer.UpdatePlayerCommands();
                    SendCommandsToServer();
                }
                    

            }
        }

        private static void SendCommandsToServer()
        {
            ProcessPressedKeys();

            if (!thisPlayer.isAlive && thisPlayer.RemainingDeathPenalty == 0)
            {
                connector.Send("res",
                    connector.SenderDefaultEndPoint);
                thisPlayer.RemainingDeathPenalty = -1;
            }
        }

        private static void ProcessPressedKeys()
        {
            if (thisPlayer.isShot)
                if (thisPlayer.Fire())
                    connector.Send("shot",
                        connector.SenderDefaultEndPoint);

            if (thisPlayer.key != 0)
            {
                MovableObject.Direction direction = MovableObject.Direction.left;
                bool moved = false;
                if (thisPlayer.key == ConsoleKey.UpArrow) { direction = MovableObject.Direction.up; moved = true; }
                if (thisPlayer.key == ConsoleKey.DownArrow) { direction = MovableObject.Direction.down; moved = true; }
                if (thisPlayer.key == ConsoleKey.LeftArrow) { direction = MovableObject.Direction.left; moved = true; }
                if (thisPlayer.key == ConsoleKey.RightArrow) { direction = MovableObject.Direction.right; moved = true; }
                if (moved)
                    if (thisPlayer.Move(direction))
                        connector.Send("move " +
                thisPlayer.newCoords.X + " " +
                thisPlayer.newCoords.Y + " " +
                (int)thisPlayer.direction,
                connector.SenderDefaultEndPoint);
            }
        }

        private static void InitThisPlayer(string[] message)
        {
            if (message[0] == "init")
            {
                thisPlayer = new Player(int.Parse(message[1]), new Map.Point(int.Parse(message[2]), int.Parse(message[3])));
            }
        }

        internal static void ProcessReceivedCommands(string[] message)
        {
            switch (message[0])
            {
                case "map":
                    Map.RedrawMap(message);
                    break;
                case "tick":
                    thisPlayer.TickTreatment();
                    break;
                case "init":
                    InitThisPlayer(message);
                    break;
                case "upd":
                    UpdateField(message);
                    break;
                case "updd":
                    UpdateMovableMap(message);
                    break;
                case "moved":
                    thisPlayer.coords.X = thisPlayer.newCoords.X;
                    thisPlayer.coords.Y = thisPlayer.newCoords.Y;
                    break;
            }
            connector.AllMessages.RemoveAt(0);
        }

        private static void UpdateMovableMap(string[] message)
        {
            if (message.Length < 7)
                return;

            ConsoleColor color = int.Parse(message[1]) == thisPlayer.team ? Map.TeammateColor : Map.EnemyColor;

            for (int i = 2; i < message.Length - 2; i += 5)
            {
                char a = message[i][0];
                int x = int.Parse(message[i + 1]);
                int y = int.Parse(message[i + 2]);
                int newX = int.Parse(message[i + 3]);
                int newY = int.Parse(message[i + 4]);

                Map.DrawCell(new Map.Point(x, y),
                    ConsoleColor.Black, ConsoleColor.Black);

                if (newX != -1)
                    Map.DrawCell(new Map.Point(newX, newY),
                    ConsoleColor.Black, color, a);
            }
        }

        private static void UpdateField(string[] message)
        {
            for (int i = 1; i < message.Length;)
            {
                Map.DrawCell(new Map.Point(int.Parse(message[i++]), int.Parse(message[i++])), ConsoleColor.Black, ConsoleColor.Black);
            }
        }

        // /Connecting to the server
    }
}



    
    
    
    

    
/*


    //Это надо куда-то переместить
    //static public List<Map.Point> Spawners = new List<Map.Point>();





    private static void StartServerGame()
    {
        Map.DownloadMap();
        SendMessageToAllClients("map" + " " + GetStringMap());
        //for (int i = 0; i < Server.Clients.Count; i++)
        //    Server.Send("command" + " " + i % NumberOfTeams, Server.Clients[i]);
        //FindSpawners();
        //ServerGamingCycle();
    }

    private static void CreatePlayers()
    {
        throw new NotImplementedException();
    }

    private static void UpdateClientsData()
    {
        throw new NotImplementedException();
    }

    private static void MoveObjects()
    {
        throw new NotImplementedException();
    }

    private static void ExecuteClientsCommands()
    {
        throw new NotImplementedException();
    }

    private static void FindSpawners()
    {
        char[] spawnersChars = { 'z', 'x', 'c', 'v' };
        for (int i = 0; i < Map.MapWidth; i++)
        {
            for (int j = 0; j < Map.MapWidth; j++)
            {
                if (Array.IndexOf(spawnersChars, Map.Field[i, j]) >= 0)
                    Spawners.Add(new Map.Point(j, i));
            }
        }
    }

    //Дальше подключаемся к серверу
    //Тут должна быть обработка команд полученных от сервера
    private static void StartClientGame()
    {
        Thread HandlingPlayerActionsThread = new Thread(Player.HandlingPlayerActions);
        HandlingPlayerActionsThread.Start();
        while (true)
        {
            CommandProcessing();
        }
    }

    private static void CommandProcessing()
    {
        for (int i = 0; i < Client.AllMessages.Count;)
        {
            string[] message = Encoding.UTF8.GetString(Client.AllMessages[0].Message).Split(' ');
            Client.AllMessages.RemoveAt(0);

            switch (message[0])
            {
                case "setcommand":
                    Player.Command = int.Parse(message[1]);
                    break;
                case "nexttick":
                    Player.TickTreatment();
                    break;
                case "map":
                    Map.RedrawMap(message);
                    break;
                case "updatemap":
                    Map.UpdateMap(message);
                    break;
                default:
                    break;
            }
        }
    }





   

    // Подтверждение подключения
    // Прием клиентом команды +new

}
}
*/