using System;

namespace BadBattleCity
{
    class Player : MovableObject
    {
        public const int StartingSpeed = 5;
        public int deathPenalty = 0;
        public int RemainingDeathPenalty = 0;

        public int ShotFrequency = 30;
        public int beforeShot = 0;

        public ConsoleKey key;
        public bool isShot = false;

        public Player(int team, Map.Point coords)
        {
            this.team = team;
            this.coords = coords;
            moveFrequency = StartingSpeed;
        }

        public void TickTreatment()
        {
            if (beforeShot > 0) beforeShot--;
            if (beforeMoving > 0) beforeMoving--;
            if (RemainingDeathPenalty > 0) RemainingDeathPenalty--;
        }

        public void UpdatePlayerCommands()
        {
            if (Console.KeyAvailable)
            {
                ConsoleKey key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.Spacebar)
                    isShot = true;
                else
                    this.key = key;
            }
        }

        public bool Move(MovableObject.Direction direction)
        {
            if (beforeMoving == 0)
            {
                if (direction == this.direction)
                {
                    Map.Point deltaCoords = GetDeltaCoords(direction);
                    newCoords = new Map.Point(coords.X + deltaCoords.X, coords.Y + deltaCoords.Y);
                }
                else
                    this.direction = direction;

                beforeMoving = moveFrequency;
                key = 0;
                return true;
            }
            return false;
    }

        public bool Fire()
        {
            if (beforeShot == 0)
            {
                beforeShot = ShotFrequency;
                isShot = false;
                return true;
            }
            return false;
        }
    }

}
