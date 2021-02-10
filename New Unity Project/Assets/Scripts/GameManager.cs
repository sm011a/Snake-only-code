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
        public Color playerColor = Color.black;

        GameObject playerObj;

        private GameObject mapObject;
        private SpriteRenderer mapRenderer;

        Node[,] grid;

        private void Start()
        {
            CreateMap();
            PlacePlayer();
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
            Sprite sprite = Sprite.Create(texture, rect, Vector2.one * 0.5f, 1, 0, SpriteMeshType.FullRect);
            mapRenderer.sprite = sprite;
        }

        private void PlacePlayer()
        {
            playerObj = new GameObject("Player");
            SpriteRenderer playerRender = playerObj.AddComponent<SpriteRenderer>();
            playerRender.sprite = CreateSprite(playerColor);
            playerRender.sortingOrder = 1;

            playerObj.transform.position = GetNode(3, 3).worldPosition;
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
            return Sprite.Create(texture, rect, Vector2.one * 0.5f, 1, 0, SpriteMeshType.FullRect);
        }
    }
}
