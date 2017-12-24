using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BadBattleCity
{
    class Bullet : MovableObject
    {
        public Bullet(int team, Game.Direction direction, Map.Point coords)
        { 
            this.team = team;
            this.direction = direction;
            this.coords = new Map.Point(coords.X, coords.Y);
            newCoords = new Map.Point(coords.X, coords.Y);
        }
        public void ContinueMovement(StringBuilder message)
        {
            if (Map.Field[newCoords.Y, newCoords.X] != Map.Cells.empty)
            {
                if (Map.Field[newCoords.Y, newCoords.X] == Map.Cells.brick)
                {
                    Map.Field[newCoords.Y, newCoords.X] = Map.Cells.empty;
                    message.Append(newCoords.Y + " " + newCoords.X + " " + Map.Cells.empty + " ");
                }
                Map.movableObjects[coords.Y, coords.X] = null;
                isAlive = false;
            }
            else if (Map.movableObjects[newCoords.Y, newCoords.X] != null)
            {
                Map.movableObjects[coords.Y, coords.X] = null;
                Map.movableObjects[newCoords.Y, newCoords.X] = null;
                isAlive = false;
            }
        }
    }
}
