using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace AStar //In case someone else is writing scripts with i.e the same methods (not likley here but...)
{
    public class Grid : MonoBehaviour
    {
        //Map
        public int maxHeight = 15;
        public int maxWidth = 17;
        public Color colour1;
        public Color colour2;
        GameObject mapObject;
        SpriteRenderer mapRenderer;

        //A*
        public int gridWorldSize;
        public float nodeRadius;
        public LayerMask unwalkableMask;
  
        //Tiles and Nodes
        public Tile[,] grid; //2D array
        List<Tile> availableNodes = new List<Tile>();

        //Misc.
        public Transform cameraHolder;

        void Awake()
        {
            gridWorldSize = maxHeight * maxWidth;

            CreateMap();
            PlaceCamera();
        }

        void CreateMap() // Creates the map
        {
            mapObject = new GameObject("Map"); //Adds the map the the hierarchy in Unity 
            mapRenderer = mapObject.AddComponent<SpriteRenderer>(); //Since we are not working in the editor, we are adding (assigning) a component rather return information from an already existing component 

            grid = new Tile[maxWidth, maxHeight]; //Initzialising the grid with the given witdh and hight, containing x and y values for all the nodes

            Texture2D texture = new Texture2D(maxWidth, maxHeight); //Makes a texture based on the parameters we gave

            for (int x = 0; x < maxWidth; x++) //nested for loop. For every row, for every column, draw a rectangle  
            {
                for (int y = 0; y < maxHeight; y++)
                {
                    Vector3 tp = new Vector3 (x, y, 0);
                    tp.x = x;
                    tp.y = y;

                    bool walkable = !(Physics.CheckSphere(tp, .5f));

                    grid[x, y] = new Tile(true, tp, x, y);
                    
                    availableNodes.Add(grid[x, y]); //Adds nodes to the available nodes list

                    #region Pixel colours //Makes the chess pattern on the grid
                    if (x % 2 != 0) //If the x coordinate of the node is odd
                    {
                        if (y % 2 != 0) //If the y coordinate of the node is odd, set it to colour 1
                        {
                            texture.SetPixel(x, y, colour1);
                        }
                        else //If the y coordinate of the node is even
                        {
                            texture.SetPixel(x, y, colour2);
                        }
                    }
                    else //If the x coordinate of the node is even
                    {
                        if (y % 2 != 0) //If the y coordinate of the node is odd, set it to colour 2
                        {
                            texture.SetPixel(x, y, colour2);
                        }
                        else //if the y coordinate is even, set it to colour 1
                        {
                            texture.SetPixel(x, y, colour1);
                        }
                    }
                    #endregion 
                }
            }

            texture.filterMode = FilterMode.Point; //By default it's billinear, set it to point to make it crisp

            texture.Apply(); //Applies all SetPixel changes      
            Rect rectangle = new Rect(0, 0, maxWidth, maxHeight); //Makes a rectangle on the coordinates 0,0 with the witdh and height we declared at the start of the script
            Sprite sprite = Sprite.Create(texture, rectangle, Vector2.zero, 1, 0, SpriteMeshType.FullRect); //Creates a new sprite
            mapRenderer.sprite = sprite; //Renders the map with the sprite we just created
        }
        void PlaceCamera() //Places the camera correctly when starting the game
        {
            Tile n = GetNode(maxWidth / 2, maxHeight / 2); //Gets the center of the grid
            Vector3 p = n.worldPosition;
            p += Vector3.one * .5f; //Makes it center in-game
            cameraHolder.position = p;
        }


        #region Update
        private void Update() //Cheks for inputs or the state of the game 
        {
           
        }
        #endregion Update


        public Tile GetNode(int x, int y) //If we want to place an object onto our map or get information about a node
        {
            //Todo: update here
            if (x < 0 || x > maxWidth - 1 || y < 0 || y > maxHeight - 1)
            {
                return null;
            }
            return grid[x, y];
        }
    }
}
