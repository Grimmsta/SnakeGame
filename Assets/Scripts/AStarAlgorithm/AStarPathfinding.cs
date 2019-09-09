using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Grimm
{
    public class AStarPathfinding : MonoBehaviour
    {
        //Grid grid;
        Node[,] grid;
        public List<Node> path;
        bool allowDiagonal;
        private void Awake()
        {
            //grid = GetComponent<Grid>();
        }

        public bool FindPath(Node[,] grid, Vector2 startPos, Vector2 targetPos)
        {
            this.grid = grid;
            Node startNode = grid[(int)startPos.x, (int)startPos.y];
            Node targetNode = grid[(int)targetPos.x, (int)targetPos.y];
            //Tile startNode = grid.GetNode(Mathf.RoundToInt(startPos.x), Mathf.RoundToInt(startPos.y));
            //Tile targetNode = grid.GetNode(Mathf.RoundToInt(targetPos.x), Mathf.RoundToInt(targetPos.y));

            List <Node> openSet = new List<Node>();
            HashSet<Node> closedSet = new HashSet<Node>();
            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                Node currentNode = openSet[0];

                for (int i = 0; i < openSet.Count; i++)
                {
                    if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost)
                    {
                        currentNode = openSet[i];
                    }
                }

                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                if (currentNode == targetNode)
                {
                    RetracePath(startNode, targetNode);
                    return true;
                }

                foreach (Node neighbour in GetNeighbour(currentNode))
                {
                    if (neighbour.walkable == false || closedSet.Contains(neighbour))
                    {
                        continue;
                    }

                    int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                    
                    if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                    {
                        neighbour.gCost = newMovementCostToNeighbour;
                        neighbour.hCost = GetDistance(currentNode, targetNode);
                        neighbour.parent = currentNode;
                        if (!openSet.Contains(neighbour))
                        {
                            openSet.Add(neighbour);
                        }
                    }
                }
            }return false;
        }

        private void RetracePath(Node start, Node end)
        {
            path = new List<Node>();
            Node current = end;

            while (current != start)
            {
                path.Add(current);
                current = current.parent;
            }
            path.Reverse();
        }

        int GetDistance (Node a, Node b)
        {
            int distX = Mathf.Abs(a.x - b.x);
            int distY = Mathf.Abs(a.y - b.y);

            if (allowDiagonal)
            {
                if (distX > distY)
                {
                    return 14 * distX + 10 * (distY - distX);
                }

                return 14 * distY + 10 * (distX - distY);
            }

            if (distX > distY)
            {
                return  distX + 1* (distY - distX);
            }

            return distY + 1 * (distX - distY);
        }

        public List<Node> GetNeighbour(Node tile)
        {
            List<Node> neighbours = new List<Node>();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (allowDiagonal)
                    {
                        if (x == 0 && y == 0)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (x == 0 && y ==0 || x == 1 && y == 1|| x == -1 && y == 1 || x == 1 && y == -1 || x == -1 && y == -1)
                        {
                            continue;
                        }
                    }

                    int checkX = tile.x + x;
                    int checkY = tile.y + y;

                    if (checkX > 0 && checkX < grid.GetLength(0) && checkY > 0 && checkY < grid.GetLength(1))
                    {
                        neighbours.Add(grid[checkX, checkY]);
                    }
                }
            }
            return neighbours;
        }
    }
}
