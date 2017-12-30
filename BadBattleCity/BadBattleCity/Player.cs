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

        public static ConsoleKey GetKeystrokes()
        {
            if (Console.KeyAvailable)
                return Console.ReadKey(true).Key;
            else
                return 0;
        }

        public bool Move(MovableObject.Direction direction)
        {
            if (IsReadyToMove == 0)
            {
                if (direction == this.direction)
                {
                    Map.Point deltaCoords = GetDeltaCoords(direction);
                    newCoords = new Map.Point(coords.X + deltaCoords.X, coords.Y + deltaCoords.Y);
                }
                else
                    this.direction = direction;
                IsReadyToMove = MoveFrequency;
                return true;
            }
            return false;
    }

        public bool Fire()
        {
            if (isReadyToShot == 0)
            {
                isReadyToShot = ShotFrequency;
                return true;
            }
            return false;
        }
    }

}
