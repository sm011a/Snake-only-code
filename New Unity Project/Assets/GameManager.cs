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

        private GameObject mapObject;
        private SpriteRenderer mapRenderer;

        private void Start()
        {
            CreateMap();
        }

        private void CreateMap()
        {
            mapObject = new GameObject("Map");
            mapRenderer = mapObject.AddComponent<SpriteRenderer>();

            Texture2D texture = new Texture2D(maxWidthMap, maxHeigtMap);
            for (int x = 0; x < maxWidthMap; x++)
            {
                for (int y = 0; y < maxHeigtMap; y++)
                {
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

    }
}
