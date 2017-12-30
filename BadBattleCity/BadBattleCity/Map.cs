using System;
using System.IO;
using System.Text;

namespace BadBattleCity
{
    static class Map
    {
        public const ConsoleColor TeammateColor = ConsoleColor.Blue;
        public const ConsoleColor EnemyColor = ConsoleColor.Red;

        public static Cells[,] Field;
        public static MovableObject[,] movableObjects;
        public static char[] spawners = { 'a', 's', 'd', 'f' };
        public static char[] flags = { 'q', 'w', 'e', 'r' };

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

        public struct Point
        {
            public int X, Y;
            public Point(int x, int y)
            {
                X = x;
                Y = y;
            }
            static public bool ComparePoints(Point a, Point b)
            {
                if (a.X == b.X && a.Y == b.Y) return true;
                return false;
            }
        }
        public static int LineWidth = 1;
        public static int MapWidth;

        internal static void UpdateMap(string[] message)
        {
            for (int i = 1; i < message.Length / 3;)
            {
                Point point = new Point(int.Parse(message[i++]), int.Parse(message[i++]));
                char type = message[i++][0];
                DrawCell(point, GetColor((Cells)type), ConsoleColor.Black);
            }
        }

        public static void DownloadMap()
        {
            string fileName = "";
            Console.WriteLine("Введите название карты");

            do
            {
                fileName = Console.ReadLine();
                if (File.Exists(fileName))
                    break;
                else
                    Console.WriteLine("Error. There is no map");
            } while (true);
            try
            {
                string[] textMap = File.ReadAllLines(fileName);
                MapWidth = textMap[0].Length;
                Field = new Cells[MapWidth, MapWidth];

                for (int i = 0; i < MapWidth; i++)
                    for (int j = 0; j < MapWidth; j++)
                    {
                        Field[i, j] = (Cells)textMap[i][j];
                    }
            }
            catch (Exception error)
            {
                Console.WriteLine("Map read error: \n{0}", error);
            }
        }

        public static void RedrawMap(string[] message)
        {
            Console.Clear();
            MapWidth = (int)Math.Sqrt(message[1].Length);

            for (int i = 0; i < MapWidth; i++)
            {
                for (int j = 0; j < MapWidth; j++)
                {
                    Console.ForegroundColor = GetColor((Cells)message[1][MapWidth * i + j]);
                    for (int k = 0; k < 2 * LineWidth; k++)
                        Console.Write('█');
                }
                Console.WriteLine();
            }
        }

        public static ConsoleColor GetColor(Cells cell)
        {
            switch (cell)
            {
                case Cells.empty:
                    return ConsoleColor.Black;
                case Cells.flag:
                    return ConsoleColor.Magenta;
                case Cells.spawner:
                    return ConsoleColor.White;
                case Cells.booster:
                    return ConsoleColor.Yellow;
                case Cells.water:
                    return ConsoleColor.Blue;
                case Cells.brick:
                    return ConsoleColor.DarkRed;
                case Cells.wall:
                    return ConsoleColor.Gray;
                case Cells.bullet:
                    return ConsoleColor.DarkRed;
                default:
                    return ConsoleColor.Red;
            }
        }

        internal static void DrawCell(Point coords, ConsoleColor backgroundColor, ConsoleColor color, char c = ' ')
        {
            Console.BackgroundColor = backgroundColor;
            Console.ForegroundColor = color;
            int maxY = coords.Y * LineWidth + LineWidth;
            int maxX = coords.X * LineWidth * 2 + LineWidth * 2;
            for (int i = coords.Y * LineWidth; i < maxY; i++)
                for (int j = coords.X * LineWidth * 2; j < maxX; j++)
                {
                    Console.SetCursorPosition(j, i);
                    Console.Write(c);
                }
        }

        public static string GetStringMap()
        {
            StringBuilder MapString = new StringBuilder();
            for (int i = 0; i < Map.MapWidth; i++)
                for (int j = 0; j < Map.MapWidth; j++)
                {
                    MapString.Append((char)Map.Field[i, j]);
                }
            return MapString.ToString();
        }
    }

}
