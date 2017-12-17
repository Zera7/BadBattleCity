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
    static class Game
    {
        public const int ServerPort = 15000;
        public const int ClientPort = 14000;

        static bool IsServerRunning = false;
        static bool GameStarted = false;
        static int NumberOfPlayers;

        static Connector Client = new Connector(new IPEndPoint(IPAddress.Broadcast, ServerPort), ClientPort);
        static Connector Server;

        static void Main()
        {
            Thread offerCreateServerThread = new Thread(OfferCreateServer);
            offerCreateServerThread.Start();
            BeginSearchServer();
            if (!IsServerRunning)
                if (offerCreateServerThread.IsAlive)
                    offerCreateServerThread.Abort();
            //Дальше подключаемся к серверу
            Console.WriteLine("Типо подключился");
        }

        private static void OfferCreateServer()
        {
            do
            {
                Console.Clear();
                Console.WriteLine("Enter the number of players to create the server");
            } while (!int.TryParse(Console.ReadLine(), out NumberOfPlayers));
            Console.WriteLine("Server start");
            IsServerRunning = true;
            StartServer();
        }

        private static void StartServer()
        {
            Server = new Connector(new IPEndPoint(IPAddress.Broadcast, ClientPort), ServerPort);
            Server.Start();

            while (!GameStarted)
            {
                Server.Send("hi", Server.SenderDefaultEndPoint);
                for (int i = 0; i < Server.AllMessages.Count;)
                {
                    string[] message = Encoding.UTF8.GetString(Server.AllMessages[0].Message).Split(' ');
                    if (message[0] == "new")
                    {
                        Server.Send("+new", Server.AllMessages[0].Address);
                        Server.Clients.Add(Server.AllMessages[0].Address);
                        Console.WriteLine("К серверу добавлен новый клиент");
                    }
                    Server.AllMessages.RemoveAt(0);
                    if (Client.Clients.Count >= NumberOfPlayers)
                        break;
                }
                Thread.Sleep(500);
            }
            StartGame();
        }

        private static void StartGame()
        {
            string map = GetStringMap();
            SendMessageToAllClients("map" + " " + map);
        }

        private static void SendMessageToAllClients(string message)
        {
            for (int i = 0; i < Server.Clients.Count; i++)
            {
                Server.Send(message, Server.Clients[i]);
            }
        }

        private static string GetStringMap()
        {
            StringBuilder MapString = new StringBuilder();
            for (int i = 0; i < Map.MapWidth; i++)
                for (int j = 0; j < Map.MapWidth; j++)
                {
                    MapString.Append(Map.Field[i, j]);
                }
            return MapString.ToString();
        }

    private static void BeginSearchServer()
        {
            bool StopSearchingServer = false;

            Client.Start();
            while (!StopSearchingServer || !CheckConnect())
            {
                StopSearchingServer = false;
                for (int i = 0; i < Client.AllMessages.Count;)
                {
                    string[] message = Encoding.UTF8.GetString(Client.AllMessages[0].Message).Split(' ');
                    Client.AllMessages.RemoveAt(0);
                    if (message[0] == "hi")
                    {
                        StopSearchingServer = true;
                        Client.Stop();
                        Client = new Connector(Client.LastReseivePoint, ClientPort);
                        Client.Start();
                        Thread.Sleep(100);
                        Client.Send("new", Client.SenderDefaultEndPoint);
                        Client.AllMessages.Clear();
                    }
                }
                Thread.Sleep(1000);
            }
        }

        private static bool CheckConnect()
        {
            for (int i = 0; i < Client.AllMessages.Count;)
            {
                string[] message = Encoding.UTF8.GetString(Client.AllMessages[0].Message).Split(' ');
                Client.AllMessages.RemoveAt(0);
                if (message[0] == "+new")
                    return true;
            }
            Client = new Connector(new IPEndPoint(IPAddress.Broadcast, ServerPort), ClientPort);
            return false;
        }
    }

    static class Map
    {
        public static Cells[,] Field;
        public enum Cells
        {
            empty = '0',
            flag = '1',
            spawner = '2',
            booster = '3',
            water = '4',
            brick = '5',
            wall = '6',
            tank = '7',
            bullet = '8',
            boom = '9'
        }
        public static int LineWidth = 1;
        public static int MapWidth;
    }
}

//String host = ;
// Получение ip-адреса.
//System.Net.IPAddress ip = System.Net.Dns.GetHostByName(System.Net.Dns.GetHostName()).AddressList[0];




//public enum Direction
//{
//    left,
//    right,
//    up,
//    down
//}

//struct Point
//{
//    public int X, Y;
//    public Point(int x, int y)
//    {
//        X = x;
//        Y = y;
//    }
//}

//class Bullet
//{
//    private Map.Cells ReplacedСell = Map.Cells.empty;
//    public Point Coords;
//    public int Speed = 1;
//    Point OldCoords;
//    Direction Direction;
//    public bool IsMobile = true;
//    public bool Removable = false;

//    public Bullet(Point coords, Direction direction)
//    {
//        Coords = coords;
//        Direction = direction;
//        OldCoords = new Point(coords.X, coords.Y);
//    }

//    public static void RemoveInactiveBullets(List<Bullet> bullets)
//    {
//        for (int i = 0; i < bullets.Count; i++)
//        {
//            if (bullets[i].Removable)
//            {
//                bullets.RemoveAt(i--);
//            }
//        }
//    }

//    public void Move()
//    {
//        OldCoords.X = Coords.X;
//        OldCoords.Y = Coords.Y;

//        if (IsMobile)
//            switch (Direction)
//            {
//                case Direction.left:
//                    Coords.X -= 1;
//                    break;
//                case Direction.right:
//                    Coords.X += 1;
//                    break;
//                case Direction.up:
//                    Coords.Y -= 1;
//                    break;
//                case Direction.down:
//                    Coords.Y += 1;
//                    break;
//            }
//        HandlingPassedCell();
//    }

//    public void DrawBullet()
//    {
//        if (IsMobile)
//        {
//            Map.DrawCell(OldCoords, Map.GetColor(ReplacedСell));
//            ReplacedСell = Map.Field[Coords.Y, Coords.X];
//            Map.DrawCell(Coords, Map.GetColor(Map.Cells.bullet));
//        }
//        else if (!Removable)
//        {
//            // TODO 
//            // Сделать обработку взрывов
//            Map.DrawCell(OldCoords, Map.GetColor(ReplacedСell));
//            Map.DrawCell(Coords, Map.GetColor(Map.Cells.boom));
//            Removable = true;
//        }
//        else
//        {
//            Map.DrawCell(OldCoords, Map.GetColor(ReplacedСell));
//        }
//    }

//    private void HandlingPassedCell()
//    {
//        switch (Map.Field[Coords.Y, Coords.X])
//        {
//            case Map.Cells.flag:
//                break;
//            case Map.Cells.spawner:
//                break;
//            case Map.Cells.booster:
//                break;
//            case Map.Cells.water:
//                break;
//            case Map.Cells.brick:
//                IsMobile = false;
//                break;
//            case Map.Cells.wall:
//                IsMobile = false;
//                Removable = true;
//                break;
//            case Map.Cells.tank:
//                break;
//            case Map.Cells.bullet:
//                break;
//            case Map.Cells.boom:
//                break;
//            default:
//                break;
//        }
//    }
//}

//static class Map
//{
//    public enum Cells
//    {
//        empty = '0',
//        flag = '1',
//        spawner = '2',
//        booster = '3',
//        water = '4',
//        brick = '5',
//        wall = '6',
//        tank = '7',
//        bullet = '8',
//        boom = '9'
//    }

//    public static int LineWidth = 1;
//    public static Cells[,] Field;
//    public static int MapWidth;

//    public static bool DownloadMap()
//    {
//        if (!File.Exists("map1.txt"))
//        {
//            Console.WriteLine("Error. There is no map");
//            return false;
//        }
//        else
//        {
//            try
//            {
//                string[] textMap = File.ReadAllLines("map1.txt");
//                MapWidth = textMap[0].Length;
//                Field = new Cells[MapWidth, MapWidth];

//                for (int i = 0; i < MapWidth; i++)
//                    for (int j = 0; j < MapWidth; j++)
//                    {
//                        Field[i, j] = (Cells)textMap[i][j];
//                    }
//                return true;
//            }
//            catch (Exception error)
//            {
//                Console.WriteLine("Map read error: \n{0}", error);
//                return false;
//            }
//        }
//    }

//    public static void FirstDrawMap()
//    {
//        for (int item = 0; item < MapWidth; item++)
//        {
//            for (int i = 0; i < LineWidth; i++)
//            {
//                for (int item2 = 0; item2 < MapWidth; item2++)
//                {
//                    if (item2 != ' ')
//                    {
//                        Console.ForegroundColor = GetColor(Field[item, item2]);
//                        for (int j = 0; j < 2 * LineWidth; j++)
//                            Console.Write('█');
//                    }
//                }
//                Console.WriteLine();
//            }
//        }
//    }

//    public static ConsoleColor GetColor(Cells cell)
//    {
//        switch (cell)
//        {
//            case Cells.empty:
//                return ConsoleColor.Black;
//            case Cells.flag:
//                return ConsoleColor.Magenta;
//            case Cells.spawner:
//                return ConsoleColor.White;
//            case Cells.booster:
//                return ConsoleColor.Yellow;
//            case Cells.water:
//                return ConsoleColor.Blue;
//            case Cells.brick:
//                return ConsoleColor.DarkRed;
//            case Cells.wall:
//                return ConsoleColor.Gray;
//            case Cells.bullet:
//                return ConsoleColor.DarkRed;
//            default:
//                return ConsoleColor.Red;
//        }
//    }

//    internal static void DrawCell(Point coords, ConsoleColor color, char c = '█')
//    {
//        Console.ForegroundColor = color;
//        int maxY = coords.Y * LineWidth + LineWidth;
//        int maxX = coords.X * LineWidth * 2 + LineWidth * 2;
//        for (int i = coords.Y * LineWidth; i < maxY; i++)
//            for (int j = coords.X * LineWidth * 2; j < maxX; j++)
//            {
//                Console.SetCursorPosition(j, i);
//                Console.Write(c);
//            }
//    }
//}

//static class Game
//{
//    public static List<Bullet> ActiveBullets = new List<Bullet>();

//    public static bool GameStarted = false;


//    static void Main(string[] args)
//    {
//        Console.CursorVisible = false;
//        Console.SetWindowSize(Map.LineWidth * 30 * 2, Math.Min(30 * Map.LineWidth, 44));


//        //StartServer();
//        //ConnectToServer();

//        //StartGameCycle();
//        //Initialization();
//        ////////////////////////////////////////////////////////////////////////////
//        if (!Map.DownloadMap())
//        {
//            Console.Read();
//            return;
//        }
//        Map.FirstDrawMap();
//        ActiveBullets.Add(new Bullet(new Point(10, 12), Direction.right));

//        DateTime time = DateTime.Now;
//        while(true)
//        {
//            if ((DateTime.Now - time).Milliseconds > 100)
//            {
//                time = DateTime.Now;
//                for (int i = 0; i < ActiveBullets.Count; i++)
//                {
//                    if (ActiveBullets[i].IsMobile)
//                        ActiveBullets[i].Move();
//                    ActiveBullets[i].DrawBullet();

//                    // TODO 
//                    // Сделать удаление взорвавшихся снарядов
//                    // Учитывать двойную толщину вертикальных линий
//                }
//                Bullet.RemoveInactiveBullets(ActiveBullets);
//            }
//        }
//        ////////////////////////////////////////////////////////////////////////////

//        //DrawMap();
//        Console.Read();
//        //ClearMap();

//        //SendData();
//        //GetData();
//    }


//}


//class NewAr
//{
//    List<Bullet> AllBullets = new List<Bullet>();

//}

// █





/*
 * Классы: 
 * Карта
 * Объект карты
 * Игрок, пуля, 
 */


//ConnectToServer
//Init
//GetTheMap
//