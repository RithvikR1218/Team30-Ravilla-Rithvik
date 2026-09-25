using UnityEngine;

namespace BeatTiming
{
    /// <summary>Placeholder sprites generated in code so the prototype needs no art assets.</summary>
    public static class ProceduralSprites
    {
        const int Size = 128;
        static Sprite square, disc, ring;

        public static Sprite Square => square != null ? square : square = Make("Square", (x, y) => 1f);
        public static Sprite Disc => disc != null ? disc : disc = Make("Disc", (x, y) => Edge(Dist(x, y), 1f));

        /// <summary>A circle outline whose stroke is ~8% of its radius.</summary>
        public static Sprite Ring => ring != null ? ring : ring = Make("Ring", (x, y) =>
        {
            float d = Dist(x, y);
            return Mathf.Min(Edge(d, 1f), 1f - Edge(d, 0.92f));
        });

        static float Dist(int x, int y)
        {
            float half = Size * 0.5f;
            float dx = (x + 0.5f - half) / half;
            float dy = (y + 0.5f - half) / half;
            return Mathf.Sqrt(dx * dx + dy * dy);
        }

        // 1 inside radius r, 0 outside, antialiased over ~1 pixel.
        static float Edge(float d, float r) => Mathf.Clamp01((r - d) * Size * 0.5f);

        static Sprite Make(string name, System.Func<int, int, float> alpha)
        {
            var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false)
            {
                name = name,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };
            var px = new Color32[Size * Size];
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                    px[y * Size + x] = new Color32(255, 255, 255, (byte)(alpha(x, y) * 255));
            tex.SetPixels32(px);
            tex.Apply();
            // pixelsPerUnit = Size, so every sprite is exactly 1 world unit across.
            return Sprite.Create(tex, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), Size);
        }
    }
}
