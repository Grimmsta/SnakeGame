﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Grimm //In case someone else is writing scripts with i.e the same methods (not likley here but...)
{
    public class GameManager : MonoBehaviour
    {
        public int maxHeight = 15;
        public int maxWidth = 17;

        public Color colour1;
        public Color colour2;
        public Color playerColour;
        public Color appleColour;

        public Transform cameraHolder;

        GameObject playerObj;
        GameObject appleObj;
        GameObject tailParent;
        Node playerNode;
        Node appleNode;
        Node prevPlayerNode;

        Sprite playerSprite;

        GameObject mapObject;
        SpriteRenderer mapRenderer;

        Node[,] grid; //2D array
        List<Node> availableNodes = new List<Node>();
        List<SpecialNode> tail = new List<SpecialNode>();

        bool up, down, left, right;

        int currentScore;
        int highScore;

        public bool isGameOver;
        public bool isPause;
        public bool isFirstInput;
        public bool isMoving;

        public float moveRate = 0.1f;
        float timer;

        public UnityEvent onStart;
        public UnityEvent onGameOver;
        public UnityEvent firstInput;
        public UnityEvent onScore;
        public UnityEvent onPause;

        public enum Direction
        {
            up, down, left, right
        }
        Direction targetDirection;
        Direction currentDirection;

        public Text currentScoreText;
        public Text highScoreText;
        public Text currentScorePause;
        public Text highScorePause;

        #region Initzialation
        void Start()
        {
            onStart.Invoke();
        }
        public void StartNewGame() //Runs all the methods when you start the game, things like creating the map, placing the player on the scene,  etc.
        {
            ClearRefrences();
            CreateMap();
            PlacePlayer();
            PlaceCamera();
            CreateCollectible();
            targetDirection = Direction.right;
            isGameOver = false;
            isPause = false;
            isMoving = true;
            currentScore = 0;
            UpdateScore();
        }
        public void ClearRefrences() // Clears all current refrences like the player, map, apples so you start on a clean slate in the begining
        {
            if (mapObject != null)
            {
                Destroy(mapObject);
            }

            if (playerObj != null)
            {
                Destroy(playerObj);
            }

            if (appleObj != null)
            {
                Destroy(appleObj);
            }

            Destroy(tailParent);

            foreach (var t in tail)
            {
                if (t.obj != null)
                {
                    Destroy(t.obj);
                }
            }
            tail.Clear();
            availableNodes.Clear();
            grid = null;
        }
        void CreateMap() // Creates the map
        {
            mapObject = new GameObject("Map"); //Adds the map the the hierarchy in Unity 
            mapRenderer = mapObject.AddComponent<SpriteRenderer>(); //Since we are not working in the editor, we are adding (assigning) a component rather return information from an already existing component 

            grid = new Node[maxWidth, maxHeight]; //Initzialising the grid with the given witdh and hight, containing x and y values for all the nodes

            Texture2D texture = new Texture2D(maxWidth, maxHeight); //Makes a texture based on the parameters we gave

            for (int x = 0; x < maxWidth; x++) //nested for loop. For every row, for every column, draw a rectangle  
            {
                for (int y = 0; y < maxHeight; y++)
                {
                    Vector3 tp = Vector3.zero;
                    tp.x = x;
                    tp.y = y;

                    Node n = new Node()
                    {
                        x = x,
                        y = y,
                        worldPosition = tp
                    };

                    grid[x, y] = n;

                    availableNodes.Add(n); //Adds nodes to the available nodes list

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
        void PlacePlayer() //Creates and places the player obj on the map
        {
            playerObj = new GameObject("Player"); //Adds the Player to the heirarchy in Unity
            SpriteRenderer playerRenderer = playerObj.AddComponent<SpriteRenderer>(); //Adds a sprite renderer
            playerSprite = CreateSprite(playerColour); //Creates the player sprite with the given player colour
            playerRenderer.sprite = playerSprite; //Rendrers the sprite
            playerRenderer.sortingOrder = 1; //Set the sprite to number 1 in the order of layer
            playerNode = GetNode(3, 3); //Places the player at 4,4 on the grid (0=1, 3=4) 
            playerObj.transform.position = playerNode.worldPosition; //the position of the player becombs the position of the player node (3, 3). Worldposition is a vector3 declared in Node.cs
            tailParent = new GameObject("tailParent"); // Adds the tail parent to the heirarchy in Unity
        }
        void PlaceCamera() //Places the camera correctly when starting the game
        {
            Node n = GetNode(maxWidth / 2, maxHeight / 2); //Gets the center of the grid
            Vector3 p = n.worldPosition;
            p += Vector3.one * .5f; //Makes it center in-game
            cameraHolder.position = p;
        }
        void CreateCollectible() //Creates the apples we consume 
        {
            appleObj = new GameObject("Apple"); //Adds the Apple in the heirarchy
            SpriteRenderer appleRenderer = appleObj.AddComponent<SpriteRenderer>(); //Gives our apple a sprite renderer
            appleRenderer.sprite = CreateSprite(appleColour); //Creates/Renders the apple with the colour we gave 
            appleRenderer.sortingOrder = 1; //Sets the apple to sortinglayer 1 so it doesn't spwan behind the map
            RandomlyPlaceCollectible(); //Calls the method so we can spawn th apple on the amp
        }
        #endregion Initzialation


        #region Update
        private void Update() //Cheks for inputs or the state of the game 
        {
            if (Input.GetKey(KeyCode.P))
            {
                onPause.Invoke();
            }

            if (isPause)
            {
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    Debug.Log("You've quit the game");
                    Application.Quit();
                }

                if (Input.GetKeyDown(KeyCode.R))
                {
                    isPause = false;
                    isMoving = true;
                    firstInput.Invoke();
                }
            }

            if (isGameOver)
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    onStart.Invoke();
                }
                return;
            }

            GetInput();

            SetPlayerDirection();

            timer += Time.deltaTime; //Timer that always count up
            if (timer > moveRate) //Basically makes it move with time and not based on how fast your computer is(based on FPS?)
                                  //when the tímer is greater than your moverate, you'll move
            {
                timer = 0;
                currentDirection = targetDirection;
                MovePlayer();
            }
        }
        void GetInput() //GEts the input so the engine knows which buttons you can use
        {
            //Since we gonna use a switch statement we tell which integer goes which direction 
            up = Input.GetButtonDown("Up"); 
            down = Input.GetButtonDown("Down");
            left = Input.GetButtonDown("Left");
            right = Input.GetButtonDown("Right");
        }
        void CheckDirection(Direction d) //Checks so your direction isn't opposite from what you want to move to 
        {
            if (!IsOpposite(d))
            {
                targetDirection = d;
            }
        }
        void SetPlayerDirection() //Sets the direction you want to move 
        {
            if (up)
            {
                CheckDirection(Direction.up);
            }
            else if (down)
            {
                CheckDirection(Direction.down);
            }
            else if (left)
            {
                CheckDirection(Direction.left);
            }
            else if (right)
            {
                CheckDirection(Direction.right);
            }
        }
        void MovePlayer() //Moves the player
        {
            int x = 0;
            int y = 0;

            if (isMoving)
            {
                switch (currentDirection) //(it's cheaper for the computer to use a switch statement if I remember correctly)
                {
                    case Direction.up:
                        y = 1;
                        break;
                    case Direction.down:
                        y = -1;
                        break;
                    case Direction.left:
                        x = -1;
                        break;
                    case Direction.right:
                        x = 1;
                        break;
                }

            }

            Node targetNode = GetNode(playerNode.x + x, playerNode.y + y);//Get the nodes around you
            if (targetNode == null) //If there isn't a node in the direction you're going (meaning you've hit a wall), you lose
            {
                onGameOver.Invoke();
                Debug.Log("Ok u lost");
            }
            else //If there is a target node around the player, the position of that node becomes the new position of the player
            {
                if (IsTailNode(targetNode)) //Compares the target node with the tail list
                {
                    Debug.Log("You ate your tail");
                    onGameOver.Invoke();
                }
                else
                {
                    bool isScore = false;

                    if (targetNode == appleNode) 
                    {
                        isScore = true;
                    }

                    Node previousNode = playerNode; //Is storing storing the data of the palyerNode (x value, int value, and the worldposition)

                    availableNodes.Add(previousNode);

                    if (isScore)
                    {
                        tail.Add(CreateTailNode(previousNode.x, previousNode.y)); //Adds a node to your tail
                        availableNodes.Remove(previousNode); //Removes the amount of available nodes since your tail is growing 
                    }

                    MoveTail();

                    playerObj.transform.position = targetNode.worldPosition; //Makes the player position to the target position (the node you are moving towards)
                    playerNode = targetNode;
                    availableNodes.Remove(playerNode);

                    if (isScore)
                    {
                        currentScore++;
                        if (currentScore >= PlayerPrefs.GetInt("Highscore", 0)) //Player prefs stores data on your computer, not safe for commercial games, but works in this case
                        {
                            highScore = currentScore;

                            PlayerPrefs.SetInt("Highscore", highScore);
                        }

                        onScore.Invoke();

                        if (availableNodes.Count > 0)
                        {
                            RandomlyPlaceCollectible();
                        }
                        else
                        {
                            //You won!
                        }
                    }
                }
            }
        }
        void MoveTail() //Moves the "tail"
        {
            //Todo: update here
            Node previousNode = null;

            if (isMoving)
            {
                for (int i = 0; i < tail.Count; i++)
                {
                    SpecialNode p = tail[i];

                    availableNodes.Add(p.node);

                    if (i == 0)
                    {
                        previousNode = p.node;
                        p.node = playerNode;
                    }
                    else
                    {
                        Node previous = p.node;
                        p.node = previousNode;
                        previousNode = previous;
                    }

                    availableNodes.Remove(p.node); //Remove the amount of available node since our tail is growing
                    p.obj.transform.position = p.node.worldPosition;
                }
            }
        }
        #endregion Update


        #region Utilities
        public void UpdateScore() //Updates the score
        {
            currentScoreText.text = currentScore.ToString(); //Sets the score text to the amount of points we scored
            currentScorePause.text = currentScore.ToString();

            highScorePause.text = PlayerPrefs.GetInt("Highscore", 0).ToString(); 
            highScoreText.text = PlayerPrefs.GetInt("Highscore", 0).ToString();
        }
        public void IsGameOver() //Game over function to call via Unity Events
        {
            isGameOver = true;
        }
        public void IsPaused() //Paused function to call via Unity Events
        {
            isPause = true;
            isMoving = false; //Disables moving so we don't hit the wall when paused
        }
        bool IsOpposite(Direction d) //Sets the opposite direction 
        {
            switch (d)
            {
                default:

                case Direction.up:
                    if (currentDirection == Direction.down)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case Direction.down:
                    if (currentDirection == Direction.up)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case Direction.left:
                    if (currentDirection == Direction.right)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case Direction.right:
                    if (currentDirection == Direction.left)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
            }
        }
        bool IsTailNode(Node n) //Compares the target node with the tail list
        {
            for (int i = 0; i < tail.Count; i++)
            {
                if (tail[i].node == n)
                {
                    return true;
                }
            }
            return false;
        }
        void RandomlyPlaceCollectible() //Places an apple node randomly on the map
        {
            int rnd = Random.Range(0, availableNodes.Count); //declares a random integer?(Note: shouldn't it be +1?)
            Node n = availableNodes[rnd]; //The node n will be a random node picked from the nodes list
            appleObj.transform.position = n.worldPosition; //the apple object will be placed on the position of n
            appleNode = n; //Sets n to apple node so we can check if the target nope is an apple or not 
        }
        Node GetNode(int x, int y) //If we want to place an object onto our map or get information about a node
        {
            //Todo: update here
            if (x < 0 || x > maxWidth - 1 || y < 0 || y > maxHeight - 1)
            {
                return null;
            }
            return grid[x, y];
        }
        SpecialNode CreateTailNode(int x, int y) //Creates the node for the tail
        {
            SpecialNode s = new SpecialNode();
            s.node = GetNode(x, y); //Gets the node based on tha position where we scored
            s.obj = new GameObject("Tail"); //Makes the node to an object in the scene
            s.obj.transform.parent = tailParent.transform; //Makes the tailnode a child to the tailParent
            s.obj.transform.position = s.node.worldPosition; //Sets so the object gets the nodes position

            SpriteRenderer r = s.obj.AddComponent<SpriteRenderer>();
            r.sprite = playerSprite;
            r.sortingOrder = 1;
            availableNodes.Remove(s.node);

            return s;
        }
        Sprite CreateSprite(Color targetColour) //Creating the sprite for the different objects in the scene
        {
            Texture2D texture = new Texture2D(1, 1); //Make a new texture with the sixe 1x1
            texture.SetPixel(0, 0, targetColour); //Changes the colour of the texture we just made an places it at 0,0
            texture.Apply(); //We have to apply the changes we just made
            texture.filterMode = FilterMode.Point; //Changes the filter mode to point instead of billinear which is set by default, make the pixels look crisp 
            Rect rectangle = new Rect(0, 0, 1, 1); //Creates the shape used to create the sprite
            return Sprite.Create(texture, rectangle, Vector2.zero, 1, 0, SpriteMeshType.FullRect);
        }
        #endregion Utilities
    }
}
