using System;

namespace BadBattleCity
{
    class Player : MovableObject
    {
        public Player(int team, Map.Point coords)
        {
            this.team = team;
            this.coords = coords;
            MoveFrequency = 3;
        }

        public int deathPenalty = 0;
        public int RemainingDeathPenalty = 0;

        public int ShotFrequency = 3;


        public int isReadyToShot = 0;

        public void TickTreatment()
        {
            if (isReadyToShot > 0) isReadyToShot--;
            if (IsReadyToMove > 0) IsReadyToMove--;
            if (RemainingDeathPenalty > 0) RemainingDeathPenalty--;
        }

        public bool Move()
        {
            if (IsReadyToMove == 0)
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    switch (key.Key)
                    {
                        case ConsoleKey.UpArrow:
                            if (direction == Game.Direction.up)
                            {
                                newCoords.X = coords.X;
                                newCoords.Y = coords.Y - 1;
                            }
                            else
                                direction = Game.Direction.up;
                            IsReadyToMove = MoveFrequency;
                            return true;
                        case ConsoleKey.DownArrow:
                            if (direction == Game.Direction.down)
                            {
                                newCoords.X = coords.X;
                                newCoords.Y = coords.Y + 1;
                            }
                            else
                                direction = Game.Direction.down;
                            IsReadyToMove = MoveFrequency;
                            return true;
                        case ConsoleKey.LeftArrow:
                            if (direction == Game.Direction.left)
                            {
                                newCoords.X = coords.X - 1;
                                newCoords.Y = coords.Y;
                            }
                            else
                                direction = Game.Direction.left;
                            IsReadyToMove = MoveFrequency;
                            return true;
                        case ConsoleKey.RightArrow:
                            if (direction == Game.Direction.right)
                            {
                                newCoords.X = coords.X + 1;
                                newCoords.Y = coords.Y;
                            }
                            else
                                direction = Game.Direction.right;
                            IsReadyToMove = MoveFrequency;
                            return true;
                    }
                }
            return false;
        }

        public bool Fire()
        {
            if (isReadyToShot == 0)
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Spacebar)
                    {
                        isReadyToShot = ShotFrequency;
                        return true;
                    }
                }
            return false;
        }
    }

}
