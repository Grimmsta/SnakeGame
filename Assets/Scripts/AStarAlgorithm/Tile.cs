using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AStar
{
    public class Tile //Is used to store data 
    {
        public int x;
        public int y;
        public Vector3 worldPosition;

        //A*
        public bool walkable;
        public int gCost;
        public int hCost;
        public Tile parent;

        public Tile(bool _walkable, Vector3 _worldPosition, int x, int y)
        {
            worldPosition = _worldPosition;
            walkable = _walkable;
            this.x = x;
            this.y = y;
        }

        public int fCost
        {
            get
            {
                return gCost + hCost;
            }
        }
    }
}
