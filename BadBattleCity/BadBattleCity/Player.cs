using System;

namespace BadBattleCity
{
    static class Player
    {
        static public int Command;
        static public Game.Direction Direction = Game.Direction.left;
        static public Map.Point Coords;
        static public int ShotFrequency = 3;
        static public int MoveFrequency = 5;

        static public int IsReadyToShot = 0;
        static public int IsReadyToMove = 0;

        static public bool Moved = false;
        static public bool Fired = false;

        public static void TickTreatment()
        {
            if (IsReadyToShot > 0) IsReadyToShot--;
            if (IsReadyToMove > 0) IsReadyToMove--;
            Moved = false;
            Fired = false;
        }

        public static void HandlingPlayerActions()
        {
            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                        TryToMove(Game.Direction.up);
                        break;
                    case ConsoleKey.DownArrow:
                        TryToMove(Game.Direction.down);
                        break;
                    case ConsoleKey.LeftArrow:
                        TryToMove(Game.Direction.left);
                        break;
                    case ConsoleKey.RightArrow:
                        TryToMove(Game.Direction.right);
                        break;
                    case ConsoleKey.Spacebar:
                        TryToShot();
                        break;
                }
            }
        }

        private static void TryToShot()
        {
            if (IsReadyToShot == 0)
            {
                Fired = true;
                IsReadyToShot = ShotFrequency;
            }
        }

        private static void TryToMove(Game.Direction direction)
        {
            if (IsReadyToMove == 0)
            {
                if (Direction != direction)
                    Direction = direction;
                else
                    Moved = true;
                IsReadyToMove = MoveFrequency;
            }
        }
    }

}
