using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BadBattleCity
{
    class MovableObject
    {
        public int team;
        public Direction direction;
        public Map.Point coords;
        public Map.Point newCoords;
        public bool isAlive = false;
        public int MoveFrequency;
        public int IsReadyToMove = 0;

        public enum Direction
        {
            left,
            right,
            up,
            down
        }

        static public Map.Point GetDeltaCoords(Direction direction)
        {
            Map.Point deltaCoords = new Map.Point();
            if (direction == Direction.left) deltaCoords.X -= 1;
            if (direction == Direction.right) deltaCoords.X += 1;
            if (direction == Direction.up) deltaCoords.Y -= 1;
            if (direction == Direction.down) deltaCoords.Y += 1;
            return deltaCoords;
        }

        public char GetChar()
        {
            if (GetType().Name == "Player")
                switch (direction)
                {
                    case Direction.left:
                        return '<';
                    case Direction.right:
                        return '>';
                    case Direction.up:
                        return '^';
                    default:
                        return 'v';
                }
            else
                return '█';
        }
    }
}
