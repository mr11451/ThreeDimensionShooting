using UnityEngine;

namespace ThreeDimensionShooter
{
    /// <summary>
    /// ボス撃破時に「CLEAR」をベクター(線画)風に画面中央へ表示する。
    /// </summary>
    public class ClearBanner : MonoBehaviour
    {
        private const float Duration = 4f;
        private const float ScatterTime = 0.8f;

        // 文字ごとのストローク。座標は文字セル内の 0〜1(左上原点)。
        private static readonly Vector2[][][] Glyphs =
        {
            // C
            new[] { new[] { new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f) } },
            // L
            new[] { new[] { new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f) } },
            // E
            new[]
            {
                new[] { new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f) },
                new[] { new Vector2(0f, 0.5f), new Vector2(0.8f, 0.5f) },
            },
            // A
            new[]
            {
                new[] { new Vector2(0f, 1f), new Vector2(0.5f, 0f), new Vector2(1f, 1f) },
                new[] { new Vector2(0.2f, 0.62f), new Vector2(0.8f, 0.62f) },
            },
            // R
            new[]
            {
                new[] { new Vector2(0f, 1f), new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0.5f), new Vector2(0f, 0.5f) },
                new[] { new Vector2(0.4f, 0.5f), new Vector2(1f, 1f) },
            },
        };

        private static Texture2D _pixel;
        private float _startTime;
        private Vector2[] _scatterDirections;
        private float[] _scatterDistances;

        public static void Show()
        {
            new GameObject("ClearBanner").AddComponent<ClearBanner>();
        }

        private void Awake()
        {
            _startTime = Time.unscaledTime;

            int segmentCount = 0;
            foreach (var glyph in Glyphs)
            {
                foreach (var stroke in glyph) segmentCount += stroke.Length - 1;
            }
            _scatterDirections = new Vector2[segmentCount];
            _scatterDistances = new float[segmentCount];
            for (int i = 0; i < segmentCount; i++)
            {
                _scatterDirections[i] = Random.insideUnitCircle * 0.8f;
                _scatterDistances[i] = Random.Range(0.5f, 1.2f);
            }
            if (_pixel == null)
            {
                _pixel = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                _pixel.SetPixel(0, 0, Color.white);
                _pixel.Apply();
            }
        }

        private void Update()
        {
            if (Time.unscaledTime - _startTime >= Duration) Destroy(gameObject);
        }

        private void OnGUI()
        {
            if (Event.current.type != EventType.Repaint) return;

            float elapsed = Time.unscaledTime - _startTime;
            // 出現は爆発の逆再生(散らばった状態から収束)、消滅は爆発(外へ散って消える)
            float scatter = 0f;
            if (elapsed < ScatterTime) scatter = 1f - elapsed / ScatterTime;
            else if (elapsed > Duration - ScatterTime) scatter = (elapsed - (Duration - ScatterTime)) / ScatterTime;
            scatter = Mathf.Clamp01(scatter);
            float travel = 1f - (1f - scatter) * (1f - scatter);
            float alpha = 1f - scatter;

            float letterHeight = Screen.height * 0.16f;
            float letterWidth = letterHeight * 0.7f;
            float gap = letterWidth * 0.45f;
            float totalWidth = Glyphs.Length * letterWidth + (Glyphs.Length - 1) * gap;
            float left = (Screen.width - totalWidth) * 0.5f;
            float top = Screen.height * 0.3f;
            float thickness = 1.5f;
            var bannerCenter = new Vector2(left + totalWidth * 0.5f, top + letterHeight * 0.5f);

            var glow = new Color(0.1f, 0.9f, 1f, 0.25f * alpha);
            var core = new Color(0.8f, 1f, 1f, alpha);

            int segment = 0;
            for (int i = 0; i < Glyphs.Length; i++)
            {
                var origin = new Vector2(left + i * (letterWidth + gap), top);
                foreach (var stroke in Glyphs[i])
                {
                    for (int p = 0; p < stroke.Length - 1; p++, segment++)
                    {
                        Vector2 a = origin + new Vector2(stroke[p].x * letterWidth, stroke[p].y * letterHeight);
                        Vector2 b = origin + new Vector2(stroke[p + 1].x * letterWidth, stroke[p + 1].y * letterHeight);

                        Vector2 outward = ((a + b) * 0.5f - bannerCenter).normalized;
                        Vector2 direction = (outward + _scatterDirections[segment]).normalized;
                        Vector2 offset = direction * (Screen.height * 0.45f * _scatterDistances[segment] * travel);

                        DrawLine(a + offset, b + offset, thickness * 2.5f, glow);
                        DrawLine(a + offset, b + offset, thickness, core);
                    }
                }
            }
        }

        private static void DrawLine(Vector2 a, Vector2 b, float thickness, Color color)
        {
            Vector2 delta = b - a;
            float length = delta.magnitude;
            if (length < 0.01f) return;

            var savedMatrix = GUI.matrix;
            var savedColor = GUI.color;
            GUI.color = color;
            GUIUtility.RotateAroundPivot(Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg, a);
            GUI.DrawTexture(new Rect(a.x - thickness * 0.5f, a.y - thickness * 0.5f, length + thickness, thickness), _pixel);
            GUI.matrix = savedMatrix;
            GUI.color = savedColor;
        }
    }
}
