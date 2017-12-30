using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BadBattleCity
{
    class Bullet : MovableObject
    {
        public const int StartingSpeed = 10;
        Player substitutedPlayer;
        Map.Point deltaCoords;
        
        public Bullet(int team, MovableObject.Direction direction, Map.Point coords)
        { 
            deltaCoords = GetDeltaCoords(direction);
            this.team = team;
            this.direction = direction;
            this.coords = new Map.Point(coords.X, coords.Y);
            newCoords = new Map.Point(coords.X, coords.Y);
            isAlive = true;
            MoveFrequency = StartingSpeed;
        }
        public void ContinueMovement(StringBuilder message)
        {
            if (IsReadyToMove <= 0)
            {
                if (Map.Field[newCoords.Y, newCoords.X] != Map.Cells.empty)
                    CollideBulletWithField();
                else
                    CollideBulletWithMovableObject();
            }
            IsReadyToMove--;
        }

        private void CollideBulletWithMovableObject()
        {
            if (Map.movableObjects[newCoords.Y, newCoords.X] != null)
            {
                if (Map.movableObjects[newCoords.Y, newCoords.X].team == team)
                {
                    if (Map.movableObjects[newCoords.Y, newCoords.X].GetType().Name == "Player")
                        substitutedPlayer = (Player)Map.movableObjects[newCoords.Y, newCoords.X];
                    MoveBullet();
                }
                else
                {
                    isAlive = false;
                    Map.movableObjects[newCoords.Y, newCoords.X].isAlive = false;
                }
            }
            else
                MoveBullet();
        }

        private void CollideBulletWithField()
        {
            if (Map.Field[newCoords.Y, newCoords.X] == Map.Cells.brick)
                Map.Field[newCoords.Y, newCoords.X] = Map.Cells.empty;
            Map.movableObjects[coords.Y, coords.X] = null;
            isAlive = false;
        }

        private void MoveBullet()
        {

                if (substitutedPlayer != null && Map.Point.ComparePoints(substitutedPlayer.coords, coords))
                    Map.movableObjects[coords.Y, coords.X] = substitutedPlayer;
                else if (Map.movableObjects[coords.Y, coords.X] == this)
                    Map.movableObjects[coords.Y, coords.X] = null;
                substitutedPlayer = null;

                Map.movableObjects[newCoords.Y, newCoords.X] = this;
                coords.X = newCoords.X;
                coords.Y = newCoords.Y;

                newCoords.X += deltaCoords.X;
                newCoords.Y += deltaCoords.Y;
                IsReadyToMove = MoveFrequency;

        }
    }
}
