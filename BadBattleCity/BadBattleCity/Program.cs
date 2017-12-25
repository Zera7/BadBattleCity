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
        // Game constants
        public const int ServerSearchTimeout = 3;
        public const int NumberOfTeams = 2;
        public const int GameSpeed = 25;
        // /Game constants

        public enum Direction
        {
            left,
            right,
            up,
            down
        }

        static public bool isReadyToStart = false;
        static public bool isStarted = false;
        static public bool isLobbyExists = false;

        static void Main()
        {
            Client.StartAsyncServerSearch();
            for (int i = 0; i < 3 && !Client.isConnectedToServer; i++) Thread.Sleep(1000);
            SuggestCreatingServer();

            while (Server.isRunning && !Game.isReadyToStart) Thread.Sleep(1000);

            Client.StartGame();
        }

        #region Start Server / Connect To Server

        private static void SuggestCreatingServer()
        {
            if (!Client.isConnectedToServer)
            {
                Thread creatingServer = new Thread(InputNumberOfPlayers);
                creatingServer.Start();
                while (!isLobbyExists) Thread.Sleep(200);
                if (!Server.isRunning)
                    if (creatingServer.IsAlive)
                        creatingServer.Abort();
            }
        }

        private static void InputNumberOfPlayers()
        {
            Console.WriteLine("Сервер не найден. Введите число игроков для создания сервера если не хотите ждать");
            while (!int.TryParse(Console.ReadLine(), out Server.numberOfPlayers)) ;
            if (!isLobbyExists && Server.numberOfPlayers > 0)
                ServerStart();
            else
                Console.WriteLine("Сервер не был создан");
        }

        private static void ServerStart()
        {
            Console.WriteLine("Сервер запущен");
            Server.isRunning = true;
            isLobbyExists = true;
            Client.isConnectedToServer = true;
            if (Client.ServerSearch.IsAlive)
                Client.ServerSearch.Abort();
            Client.connector.Stop();
            Client.connector = new Connector(new IPEndPoint(IPAddress.Loopback, Server.Port), Client.Port);
            Server.connector.Clients.Add(new IPEndPoint(IPAddress.Loopback, Client.Port));
            Server.Start();
        }

        #endregion


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