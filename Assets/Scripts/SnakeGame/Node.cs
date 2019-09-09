using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Grimm
{
    public class Node //Is used to store data 
    {
        public int x;
        public int y;
        public Vector2 worldPosition;
        public GameObject obj;
        public Node node;

        //A*
        public bool walkable;
        public int gCost;
        public int hCost;
        public Node parent;

        public Node(bool _walkable, Vector2 _worldPosition, int x, int y)
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
