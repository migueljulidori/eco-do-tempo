using UnityEngine;

namespace EcoDoTempo
{
    // Optional pixel art under Assets/Resources. Missing files keep the geometric look.
    public static class ArtCatalog
    {
        public static Sprite Get(string resourcePath)
        {
            var sprites = Resources.LoadAll<Sprite>(resourcePath);
            if (sprites != null && sprites.Length > 0) return sprites[0];
            return Resources.Load<Sprite>(resourcePath);
        }

        public static bool Has(string resourcePath) => Get(resourcePath) != null;

        // Bottom of a map cell (for sprites with Bottom Center pivot).
        public static Vector2 FloorPos(Vector2 cellCenter) => cellCenter + Vector2.down * 0.5f;

        // Stable pick among existing resources (breaks tile repetition).
        public static string Pick(Vector2 pos, params string[] paths)
        {
            int count = 0;
            for (int i = 0; i < paths.Length; i++)
                if (Has(paths[i])) count++;
            if (count == 0) return paths.Length > 0 ? paths[0] : null;

            int index = Mathf.Abs((int)(pos.x * 17f + pos.y * 31f + 7f)) % count;
            for (int i = 0; i < paths.Length; i++)
            {
                if (!Has(paths[i])) continue;
                if (index == 0) return paths[i];
                index--;
            }
            return paths[0];
        }

        // Replaces the runtime shape with a drawn sprite when available.
        // If worldScale.y <= 0, keeps the sprite aspect (scales by width only).
        public static bool TryApply(GameObject go, string resourcePath, Vector2 worldScale)
        {
            var sprite = Get(resourcePath);
            if (sprite == null) return false;

            var sr = go.GetComponent<SpriteRenderer>();
            if (sr == null) return false;

            sr.sprite = sprite;
            sr.color = Color.white;

            float nativeW = Mathf.Max(0.01f, sprite.bounds.size.x);
            float nativeH = Mathf.Max(0.01f, sprite.bounds.size.y);
            float sx = worldScale.x / nativeW;
            float sy = worldScale.y > 0f ? worldScale.y / nativeH : sx;
            go.transform.localScale = new Vector3(sx, sy, 1f);

            if (go.GetComponent<ArtSpriteMarker>() == null)
                go.AddComponent<ArtSpriteMarker>();
            return true;
        }

        public static GameObject Create(string name, Transform parent, Vector2 position, Vector2 fallbackScale,
            Sprite fallbackSprite, Color color, int order, string resourcePath, Vector2 artScale)
        {
            var go = SpriteFactory.CreateObject(name, parent, position, fallbackScale, fallbackSprite, color, order);
            if (!string.IsNullOrEmpty(resourcePath) && TryApply(go, resourcePath, artScale))
                go.GetComponent<SpriteRenderer>().color = Color.white;
            return go;
        }
    }
}
