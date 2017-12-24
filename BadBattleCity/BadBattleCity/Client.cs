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
            "res"       // Respawn request
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
                    connector.AllMessages.RemoveAt(0);
                    if (message[0] == "hi")
                    {
                        connector.Stop();
                        connector = new Connector(connector.LastReseivePoint, Port);
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
            Console.WriteLine("Подключение не подтверждено");
            connector.Stop();
            return false;
        }

        #endregion

        internal static void StartGame()
        {
            while (true)
            {
                string[] message = Encoding.UTF8.GetString(connector.SyncReceive().Message).Split(' ');
                ProcessReceivedCommands(message);
                if (thisPlayer != null)
                {
                    if (thisPlayer.Move())
                        connector.Send("move " +
                            thisPlayer.newCoords.X + " " +
                            thisPlayer.newCoords.Y + " " +
                            thisPlayer.direction,
                            connector.SenderDefaultEndPoint);
                    if (thisPlayer.Fire())
                        connector.Send("shot ",
                            connector.SenderDefaultEndPoint);
                    if (!thisPlayer.isAlive && thisPlayer.RemainingDeathPenalty == 0)
                    {
                        connector.Send("res ",
                            connector.SenderDefaultEndPoint);
                        thisPlayer.RemainingDeathPenalty = -1;
                    }
                }
                
            }
        }

        private static void InitThisPlayer()
        {
            string[] message = Encoding.UTF8.GetString(connector.SyncReceive().Message).Split(' ');
            if (message[0] == "init")
            {
                thisPlayer = new Player(int.Parse(message[1]), new Map.Point(int.Parse(message[2]), int.Parse(message[3])));
            }
            connector.AllMessages.RemoveAt(0);
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
                    InitThisPlayer();
                    break;
            }
            connector.AllMessages.RemoveAt(0);
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