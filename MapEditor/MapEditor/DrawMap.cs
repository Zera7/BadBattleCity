using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

namespace MapEditor
{
    public struct Point
    {
        public int X, Y;

        public Point(int x = 0, int y = 0) {
            X = x;
            Y = y;
        }
    }

    static class Field
    {
        public static Graphics graphics;
        public static int LineWidth = 3;
        public static Pen MatrixPen;
        public static Pen ActivePen;

        public static Brush[] brushes =
        {
            Brushes.White,
            Brushes.Orange,
            Brushes.LightBlue,
            Brushes.Green,
            Brushes.DarkGray,
            Brushes.Yellow,
            Brushes.Purple,
            Brushes.Cyan,
            Brushes.GreenYellow
        };
        public static int activeBrush = 1;

        public static int Size = 40;
        public static int RectWidth = 20;
        public static byte[,] Matrix = new byte[Size, Size];
        public static bool Active = false;

        internal static void InitializeField(Graphics graphics, int size = 40, int rectWidth = 20, int lineWidth = 3)
        {
            Field.graphics = graphics;
            Field.Size = size;
            Field.RectWidth = rectWidth;
            Field.LineWidth = lineWidth;

            MatrixPen = new Pen(Brushes.DarkKhaki, LineWidth);
            ActivePen = new Pen(Brushes.Black, LineWidth);
        }

        internal static void MouseDown(Form sender, MouseEventArgs e)
        {
            Active = true;
            FillNewRectanle(new Point(e.X, e.Y));
        }

        internal static void MouseUp(Form sender, MouseEventArgs ev)
        {
            Active = false;
        }

        internal static void MouseMove(Form sender, MouseEventArgs ev)
        {
            if (Active) FillNewRectanle(new Point(ev.X, ev.Y));
        }

        internal static void DrawMatrix()
        {
            for (int i = 0; i <= Size; i++)
            {
                graphics.DrawLine(MatrixPen,
                    0,
                    i * RectWidth,
                    RectWidth * Size,
                    i * RectWidth
                    );

                graphics.DrawLine(MatrixPen,
                    i * RectWidth,
                    0,
                    i * RectWidth,
                    RectWidth * Size
                    );
            }
            
        }

        internal static void FillAllRectangles()
        {
            for (int i = 0; i < Size; i++)
                for (int j = 0; j < Size; j++)
                    FillRectangle(new Point(j, i), Matrix[i, j]);

            for (int i = 0; i < brushes.Length; i++)
            {
                FillRectangle(new Point(Size + 2, i), i);
            }
        }

        internal static void FillNewRectanle(Point coords)
        {
            coords.X /= RectWidth;
            coords.Y /= RectWidth;

            if (coords.X >= 0 && coords.Y >= 0 && coords.X < Size && coords.Y < Size)
            {
                Matrix[coords.Y, coords.X] = Convert.ToByte(activeBrush);
                FillRectangle(coords);
            }
        }

        private static void FillRectangle(Point coords, int numberBrush = -1)
        {
            graphics.FillRectangle(
                brushes[numberBrush == -1 ? activeBrush : numberBrush],
                coords.X * RectWidth + LineWidth / 2 + 1,
                coords.Y * RectWidth + LineWidth / 2 + 1,
                RectWidth - LineWidth,
                RectWidth - LineWidth
                );
        }

        internal static void KeyDown(Form sender, KeyEventArgs e)
        {
            if (e.KeyValue > 48 && e.KeyValue <= 48 + 9)
            {
                activeBrush = e.KeyValue - 49;
            }
            else
                switch (e.KeyCode)
                {
                    case Keys.Left:
                        {
                            break;
                        }
                    case Keys.Right:
                        {
                            break;
                        }
                    case Keys.Up:
                        {
                            break;
                        }
                    case Keys.Down:
                        {
                            break;
                        }
                    case Keys.Space:
                        {
                            SaveMap();
                            break;
                        }
                }
        }

        private static void SaveMap()
        {
            StringBuilder a = new StringBuilder();
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    a.Append(Matrix[i, j]);
                }
                a.Append(Environment.NewLine);
            }
            Save(a.ToString());
        }


        private static void Save(string data)
        {
            string time = DateTime.Now.ToString().Replace(":", "-"); ;
            StreamWriter writer = new StreamWriter(time + ".txt", true, Encoding.GetEncoding("windows-1251"));
            writer.Write(data);
            writer.Close();
        }
    }
}
