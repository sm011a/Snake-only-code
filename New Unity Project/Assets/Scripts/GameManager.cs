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
        GameObject tailParent;
        Node playerNode;
        Node appleNode;
        Sprite playerSprite;

        private GameObject mapObject;
        private SpriteRenderer mapRenderer;

        Node[,] grid;
        List<Node> availableNodes = new List<Node>();
        List<SpecialNode> tail = new List<SpecialNode>();

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
            playerSprite = CreateSprite(playerColor);
            playerRender.sprite = playerSprite;
            playerRender.sortingOrder = 1;
            playerNode = GetNode(3, 3);
            playerObj.transform.position = playerNode.worldPosition;

            tailParent = new GameObject("tailParent");
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

                Node previousNode = playerNode;
                availableNodes.Add(previousNode);
                playerObj.transform.position = targetNode.worldPosition;
                playerNode = targetNode;
                availableNodes.Remove(playerNode);

                if (isScore)
                {
                    tail.Add(CreateTailNode(previousNode.x, previousNode.y));
                    availableNodes.Remove(previousNode);
                }

                MoveTail();

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

        void MoveTail()
        {
            Node prevNode = null;

            for(int i = 0; i < tail.Count; i++)
            {
                SpecialNode p = tail[i];
                availableNodes.Add(p.node);
                
                if(i == 0)
                {
                    prevNode = p.node;
                    p.node = playerNode;
                }
                else
                {
                    Node prev = p.node;
                    p.node = prevNode;
                    prevNode = prev;
                }

                availableNodes.Remove(p.node);
                p.obj.transform.position = p.node.worldPosition;
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

        SpecialNode CreateTailNode(int x, int y)
        {
            SpecialNode s = new SpecialNode();
            s.node = GetNode(x, y);
            s.obj = new GameObject();
            s.obj.transform.parent = tailParent.transform;
            s.obj.transform.position = s.node.worldPosition;
            SpriteRenderer r = s.obj.AddComponent<SpriteRenderer>();
            r.sprite = playerSprite;
            r.sortingOrder = 1;
            return s;
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
