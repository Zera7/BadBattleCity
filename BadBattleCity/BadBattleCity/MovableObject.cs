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
    }
}
