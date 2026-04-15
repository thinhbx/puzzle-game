using UnityEngine;
using PuzzleGame.Game.Board;
using PuzzleGame.Game.Config;

namespace PuzzleGame.Game.Utils
{
    /// <summary>
    /// Helper để tạo block prefabs đơn giản cho demo (sử dụng colored sprites).
    /// Attach lên một GameObject trong scene, nhấn Play để auto-generate prefabs.
    /// </summary>
    public class DemoBlockGenerator : MonoBehaviour
    {
        [SerializeField] private GameConfig config;

        private static readonly Color[] DefaultColors = new Color[]
        {
            new Color(1f, 0.2f, 0.2f),    // Red
            new Color(0.2f, 0.6f, 1f),    // Blue
            new Color(0.2f, 0.9f, 0.2f),  // Green
            new Color(1f, 0.8f, 0.1f),    // Yellow
            new Color(0.8f, 0.2f, 0.9f),  // Purple
            new Color(1f, 0.5f, 0.1f),    // Orange
        };

        /// <summary>
        /// Tạo một block GameObject đơn giản với SpriteRenderer.
        /// Sử dụng method này trong Editor script hoặc runtime để tạo block prefabs.
        /// </summary>
        public static GameObject CreateSimpleBlock(int typeIndex, float size = 0.9f)
        {
            var go = new GameObject($"Block_{typeIndex}");

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSquareSprite();
            sr.color = typeIndex < DefaultColors.Length ? DefaultColors[typeIndex] : Color.white;

            go.transform.localScale = Vector3.one * size;

            // Add BlockPiece component
            go.AddComponent<BlockPiece>();

            // Add collider for raycasting (optional)
            var col = go.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;

            return go;
        }

        /// <summary>
        /// Tạo một cell background đơn giản.
        /// </summary>
        public static GameObject CreateSimpleCell(float size = 1f)
        {
            var go = new GameObject("Cell");

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSquareSprite();
            sr.color = new Color(0.9f, 0.9f, 0.9f, 0.3f);
            sr.sortingOrder = -1;

            go.transform.localScale = Vector3.one * size;

            return go;
        }

        /// <summary>
        /// Tạo sprite vuông trắng đơn giản bằng code.
        /// </summary>
        public static Sprite CreateSquareSprite(int size = 64)
        {
            var tex = new Texture2D(size, size);
            var colors = new Color[size * size];

            for (int i = 0; i < colors.Length; i++)
            {
                // Tạo rounded corners
                int x = i % size;
                int y = i / size;
                float dx = Mathf.Abs(x - size / 2f) / (size / 2f);
                float dy = Mathf.Abs(y - size / 2f) / (size / 2f);
                float cornerRadius = 0.15f;

                if (dx > 1 - cornerRadius && dy > 1 - cornerRadius)
                {
                    float cdx = (dx - (1 - cornerRadius)) / cornerRadius;
                    float cdy = (dy - (1 - cornerRadius)) / cornerRadius;
                    float dist = Mathf.Sqrt(cdx * cdx + cdy * cdy);
                    colors[i] = dist <= 1 ? Color.white : Color.clear;
                }
                else
                {
                    colors[i] = Color.white;
                }
            }

            tex.SetPixels(colors);
            tex.Apply();
            tex.filterMode = FilterMode.Bilinear;

            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }
    }
}