using UnityEngine;

namespace ThreeDimensionShooter
{
    /// <summary>
    /// UI 用のスプライトをランタイムで生成する。
    /// コード生成テクスチャはシーンにシリアライズされないため、実行時に生成して割り当てる。
    /// </summary>
    public static class UISpriteFactory
    {
        /// <summary>円スプライト。outlineOnly=true で輪郭のみ。</summary>
        public static Sprite Circle(int size, bool outlineOnly)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float c = size / 2f;
            float r = c - 1f;
            float thickness = Mathf.Max(1.5f, size * 0.03f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(c, c));
                    bool on = outlineOnly
                        ? Mathf.Abs(d - r) < thickness
                        : d <= r;
                    tex.SetPixel(x, y, on ? Color.white : Color.clear);
                }
            }
            tex.Apply();
            tex.name = "CircleSprite";
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        /// <summary>ダイヤ形スプライト。</summary>
        public static Sprite Diamond(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float c = size / 2f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool on = Mathf.Abs(x - c) + Mathf.Abs(y - c) <= c - 1f;
                    tex.SetPixel(x, y, on ? Color.white : Color.clear);
                }
            }
            tex.Apply();
            tex.name = "DiamondSprite";
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        /// <summary>上向き矢印スプライト。</summary>
        public static Sprite Arrow(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float c = size / 2f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float relY = y / (float)size;
                    float halfW = relY * c;
                    bool on = Mathf.Abs(x - c) <= halfW && y > size * 0.15f;
                    tex.SetPixel(x, y, on ? Color.white : Color.clear);
                }
            }
            tex.Apply();
            tex.name = "ArrowSprite";
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        /// <summary>十字照準スプライト。</summary>
        public static Sprite Crosshair(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float c = size / 2f;
            int th = Mathf.Max(1, size / 24);
            int gap = size / 5;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool h = Mathf.Abs(y - c) < th && (x < c - gap || x > c + gap);
                    bool v = Mathf.Abs(x - c) < th && (y < c - gap || y > c + gap);
                    tex.SetPixel(x, y, (h || v) ? Color.white : Color.clear);
                }
            }
            tex.Apply();
            tex.name = "CrosshairSprite";
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }
    }
}
