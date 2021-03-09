using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SA
{
    public class GameManager : MonoBehaviour
    {
        public int maxHeigtMap = 15;
        public int maxWidthMap = 17;

        public Color greenLight;
        public Color greenDark;
        public Color appleColor = Color.red;
        public Color playerColor = Color.black;

        public Transform cameraHolder;

        GameObject playerObj;
        GameObject appleObj;
        Node playerNode;
        Node appleNode;

        private GameObject mapObject;
        private SpriteRenderer mapRenderer;

        Node[,] grid;
        List<Node> availableNodes = new List<Node>();

        bool up, left, right, down;

        public float moveRate = 0.5f;
        float timer;

        Direction curDirection;

        public enum Direction
        {
            up, down, left, right
        }

        #region Init
        private void Start()
        {
            CreateMap();
            PlacePlayer();
            PlaceCamera();
            CreateApple();
            curDirection = Direction.right;
        }

        private void CreateMap()
        {
            mapObject = new GameObject("Map");
            mapRenderer = mapObject.AddComponent<SpriteRenderer>();

            grid = new Node[maxWidthMap, maxHeigtMap];

            Texture2D texture = new Texture2D(maxWidthMap, maxHeigtMap);
            for (int x = 0; x < maxWidthMap; x++)
            {
                for (int y = 0; y < maxHeigtMap; y++)
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

                    availableNodes.Add(n);

                    #region Visual
                    if (x % 2 != 0)
                    {
                        if (y % 2 != 0)
                        {
                            texture.SetPixel(x, y, greenLight);
                        }
                        else
                        {
                            texture.SetPixel(x, y, greenDark);
                        }
                    }
                    else
                    {
                        if (y % 2 != 0)
                        {
                            texture.SetPixel(x, y, greenDark);
                        }
                        else
                        {
                            texture.SetPixel(x, y, greenLight);
                        }
                    }
                    #endregion
                }
            }
            texture.filterMode = FilterMode.Point;

            texture.Apply();
            Rect rect = new Rect(0, 0, maxWidthMap, maxHeigtMap);
            Sprite sprite = Sprite.Create(texture, rect, Vector2.zero, 1, 0, SpriteMeshType.FullRect);
            mapRenderer.sprite = sprite;
        }

        private void PlacePlayer()
        {
            playerObj = new GameObject("Player");
            SpriteRenderer playerRender = playerObj.AddComponent<SpriteRenderer>();
            playerRender.sprite = CreateSprite(playerColor);
            playerRender.sortingOrder = 1;
            playerNode = GetNode(3, 3);
            playerObj.transform.position = playerNode.worldPosition;
        }

        private void PlaceCamera()
        {
            Node n = GetNode(maxWidthMap / 2, maxHeigtMap / 2);
            Vector3 p = n.worldPosition;
            p += Vector3.one * .5f;

            cameraHolder.position = p;
        }

        void CreateApple()
        {
            appleObj = new GameObject("Apple");
            SpriteRenderer appleRenderer = appleObj.AddComponent<SpriteRenderer>();
            appleRenderer.sprite = CreateSprite(appleColor);
            appleRenderer.sortingOrder = 1;
            RandomlyPlaceApple();
        }

        #endregion

        #region Update
        private void Update()
        {
            GetInput();
            SetPlayerDirection();

            timer += Time.deltaTime;
            if (timer > moveRate)
            {
                timer = 0;
                MovePlayer();
            }
        }

        void GetInput()
        {
            up = Input.GetButtonDown("Up");
            down = Input.GetButtonDown("Down");
            left = Input.GetButtonDown("Left");
            right = Input.GetButtonDown("Right");
        }

        private void SetPlayerDirection()
        {
            if (up)
            {
                curDirection = Direction.up;

            }
            else if (down)
            {
                curDirection = Direction.down;

            }
            else if (left)
            {
                curDirection = Direction.left;

            }
            else if (right)
            {
                curDirection = Direction.right;
            }
        }

        private void MovePlayer()
        {
            int x = 0;
            int y = 0;

            switch (curDirection)
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

            Node targetNode = GetNode(playerNode.x + x, playerNode.y + y);
            if (targetNode == null)
            {
                //Game Over
            }
            else
            {
                bool isScore = false;

                if (targetNode == appleNode)
                {
                    isScore = true;
                }

                availableNodes.Remove(playerNode);
                playerObj.transform.position = targetNode.worldPosition;
                playerNode = targetNode;
                availableNodes.Add(playerNode);

                //Move Tail

                if (isScore)
                {
                    if (availableNodes.Count > 0)
                    {
                        RandomlyPlaceApple();
                    }
                    else
                    {
                        // you won
                    }
                }
            }
        }

        #endregion

        #region Utilities

        void RandomlyPlaceApple()
        {
            int ran = Random.Range(0, availableNodes.Count);
            Node n = availableNodes[ran];
            appleObj.transform.position = n.worldPosition;
            appleNode = n;
        }

        Node GetNode(int x, int y)
        {
            if (x < 0 || x > maxWidthMap - 1 || y < 0 || y > maxHeigtMap - 1)
                return null;

            return grid[x, y];
        }

        Sprite CreateSprite(Color targetColor)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, targetColor);
            texture.Apply();
            texture.filterMode = FilterMode.Point;
            Rect rect = new Rect(0, 0, 1, 1);
            return Sprite.Create(texture, rect, Vector2.zero, 1, 0, SpriteMeshType.FullRect);
        }
        #endregion
    }
}
