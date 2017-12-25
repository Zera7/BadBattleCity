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
        public Game.Direction direction;
        public Map.Point coords;
        public Map.Point newCoords;
        public bool isAlive = false;
        public int MoveFrequency;
        public int IsReadyToMove = 0;

        public char GetChar()
        {
            if (GetType().Name == "Player")
                switch (direction)
                {
                    case Game.Direction.left:
                        return '<';
                    case Game.Direction.right:
                        return '>';
                    case Game.Direction.up:
                        return '^';
                    default:
                        return 'v';
                }
            else
                return '█';
        }
    }
}
