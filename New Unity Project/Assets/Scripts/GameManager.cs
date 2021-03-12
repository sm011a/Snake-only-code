using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

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
        Node prevPlayerNode;
        Node appleNode;
        Sprite playerSprite;

        private GameObject mapObject;
        private SpriteRenderer mapRenderer;

        Node[,] grid;
        List<Node> availableNodes = new List<Node>();
        List<SpecialNode> tail = new List<SpecialNode>();

        bool up, left, right, down;

        int currentScore;
        int highScore;

        public bool isGameOver;
        public bool isFirstInput;
        public float moveRate = 0.5f;
        float timer;

        Direction targetDirection;
        Direction curDirection;

        public Text currentScoreText;
        public Text highScoreText;

        public enum Direction
        {
            up, down, left, right
        }

        public UnityEvent onStart;
        public UnityEvent onGameOver;
        public UnityEvent firstInput;
        public UnityEvent onScore;


        #region Init
        private void Start()
        {
            onStart.Invoke();
        }

        public void StartNewGame()
        {
            ClearReferences();
            CreateMap();
            PlacePlayer();
            PlaceCamera();
            CreateApple();
            targetDirection = Direction.right;
            isGameOver = false;
            currentScore = 0;
            UpdateScore();
        }

        public void ClearReferences()
        {
            if (mapObject != null)
                Destroy(mapObject);

            if (playerObj != null)
                Destroy(playerObj);

            if (appleObj != null)
                Destroy(appleObj);

            foreach (var t in tail)
            {
                if (t.obj != null)
                    Destroy(t.obj);
            }

            tail.Clear();
            availableNodes.Clear();
            grid = null;
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

            PlacePlayerObject(playerObj, playerNode.worldPosition);
            playerObj.transform.localScale = Vector3.one * 1.2f;

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
            if (isGameOver)
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    onStart.Invoke();
                }
                return;
            }

            GetInput();
            if (isFirstInput)
            {
                SetPlayerDirection();

                timer += Time.deltaTime;
                if (timer > moveRate)
                {
                    timer = 0;
                    curDirection = targetDirection;
                    MovePlayer();
                }
            }
            else
            {
                if (up || down || left || right)
                {
                    isFirstInput = true;
                    firstInput.Invoke();
                }
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
                SetDirection(Direction.up);
            }
            else if (down)
            {
                SetDirection(Direction.down);
            }
            else if (left)
            {
                SetDirection(Direction.left);
            }
            else if (right)
            {
                SetDirection(Direction.right);
            }
        }

        void SetDirection(Direction d)
        {
            if (!isOpposite(d))
            {
                targetDirection = d;
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
                onGameOver.Invoke();
            }
            else
            {
                if (isTailNode(targetNode))
                {
                    //GameOver
                    onGameOver.Invoke();
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

                    if (isScore)
                    {
                        tail.Add(CreateTailNode(previousNode.x, previousNode.y));
                        availableNodes.Remove(previousNode);
                    }

                    MoveTail();

                    PlacePlayerObject(playerObj, targetNode.worldPosition);
                    playerNode = targetNode;
                    availableNodes.Remove(playerNode);

                    if (isScore)
                    {
                        currentScore++;
                        if(currentScore>= highScore)
                        {
                            highScore = currentScore;
                        }

                        onScore.Invoke();

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
        }

        void MoveTail()
        {
            Node prevNode = null;

            for (int i = 0; i < tail.Count; i++)
            {
                SpecialNode p = tail[i];
                availableNodes.Add(p.node);

                if (i == 0)
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
                PlacePlayerObject(p.obj, p.node.worldPosition);
            }
        }
        #endregion

        #region Utilities

        public void GameOver()
        {
            isGameOver = true;
            isFirstInput = false;
        }

        public void UpdateScore()
        {
            currentScoreText.text = currentScore.ToString();
            highScoreText.text = highScore.ToString();
        }

        bool isOpposite(Direction d)
        {
            switch (d)
            {
                default:
                case Direction.up:
                    if (curDirection == Direction.down)
                        return true;
                    else
                        return false;
                case Direction.down:
                    if (curDirection == Direction.up)
                        return true;
                    else
                        return false;
                case Direction.left:
                    if (curDirection == Direction.right)
                        return true;
                    else
                        return false;
                case Direction.right:
                    if (curDirection == Direction.left)
                        return true;
                    else
                        return false;
            }

        }

        bool isTailNode(Node n)
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

        void PlacePlayerObject(GameObject obj, Vector3 pos)
        {
            pos += Vector3.one * .5f;
            obj.transform.position = pos;
        }

        void RandomlyPlaceApple()
        {
            int ran = Random.Range(0, availableNodes.Count);
            Node n = availableNodes[ran];
            PlacePlayerObject(appleObj, n.worldPosition);
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
            s.obj.transform.localScale = Vector3.one * .95f;
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
            return Sprite.Create(texture, rect, Vector2.one * .5f, 1, 0, SpriteMeshType.FullRect);
        }
        #endregion
    }
}