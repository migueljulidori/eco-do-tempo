using System;
using UnityEngine;

namespace EcoDoTempo
{
    // Generates simple shapes at runtime so the project needs no art assets yet.
    public static class SpriteFactory
    {
        const int Size = 16;

        static Sprite square, triangle, circle, diamond, softCircle;
        static Material material;

        public static Sprite Square => square != null ? square : square = Make((x, y) => true);

        public static Sprite Triangle => triangle != null ? triangle : triangle = Make((x, y) => y <= 1f - Mathf.Abs(x - 0.5f) * 2f);

        public static Sprite Circle => circle != null ? circle : circle = Make((x, y) =>
            (x - 0.5f) * (x - 0.5f) + (y - 0.5f) * (y - 0.5f) <= 0.25f);

        public static Sprite Diamond => diamond != null ? diamond : diamond = Make((x, y) =>
            Mathf.Abs(x - 0.5f) + Mathf.Abs(y - 0.5f) <= 0.5f);

        // Soft radial disc for a light halo behind pickups.
        public static Sprite SoftCircle => softCircle != null ? softCircle : softCircle = MakeSoft();

        // Unlit so sprites look the same in Built-in, URP and URP 2D projects.
        static Material Material
        {
            get
            {
                if (material == null)
                {
                    var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
                    if (shader == null) shader = Shader.Find("Sprites/Default");
                    material = new Material(shader);
                }
                return material;
            }
        }

        public static SpriteRenderer AddRenderer(GameObject go, Sprite sprite, Color color, int order = 0)
        {
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = order;
            sr.sharedMaterial = Material;
            return sr;
        }

        public static GameObject CreateObject(string name, Transform parent, Vector2 position, Vector2 scale,
            Sprite sprite, Color color, int order = 0)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.transform.localScale = new Vector3(scale.x, scale.y, 1f);
            AddRenderer(go, sprite, color, order);
            return go;
        }

        static Sprite Make(Func<float, float, bool> inside)
        {
            var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
            };
            var pixels = new Color32[Size * Size];
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                    pixels[y * Size + x] = inside((x + 0.5f) / Size, (y + 0.5f) / Size)
                        ? new Color32(255, 255, 255, 255)
                        : new Color32(0, 0, 0, 0);
            tex.SetPixels32(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), Size);
        }

        static Sprite MakeSoft()
        {
            const int n = 48;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
            };
            var pixels = new Color[n * n];
            float c = (n - 1) * 0.5f;
            for (int y = 0; y < n; y++)
            {
                for (int x = 0; x < n; x++)
                {
                    float dx = (x - c) / c;
                    float dy = (y - c) / c;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = Mathf.Clamp01(1f - d);
                    a *= a;
                    pixels[y * n + x] = new Color(1f, 1f, 1f, a);
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        }
    }
}
