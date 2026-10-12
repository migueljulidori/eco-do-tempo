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

        // Top Y of the solid floor under a walkable cell.
        public static float FloorTop(Vector2 walkableCell) => walkableCell.y - 0.5f;

        // Stable pick among existing resources.
        public static string Pick(Vector2 pos, params string[] paths)
        {
            int count = 0;
            for (int i = 0; i < paths.Length; i++)
                if (Has(paths[i])) count++;
            if (count == 0) return paths.Length > 0 ? paths[0] : null;

            int index = Mathf.Abs((int)pos.x + (int)pos.y * 3) % count;
            for (int i = 0; i < paths.Length; i++)
            {
                if (!Has(paths[i])) continue;
                if (index == 0) return paths[i];
                index--;
            }
            return paths[0];
        }

        // Cycle floors A/B/C by column so the ground never looks like one stamp.
        public static string PickFloor(Vector2 pos)
        {
            string[] floors = { "Tiles/floor_a", "Tiles/floor_b", "Tiles/floor_c" };
            int available = 0;
            for (int i = 0; i < floors.Length; i++)
                if (Has(floors[i])) available++;

            if (available == 0) return Pick(pos, "Tiles/wall", "Tiles/wall_b", "Tiles/wall_c");

            int slot = Mathf.Abs((int)pos.x) % available;
            for (int i = 0; i < floors.Length; i++)
            {
                if (!Has(floors[i])) continue;
                if (slot == 0) return floors[i];
                slot--;
            }
            return floors[0];
        }

        public static string PickWall(Vector2 pos) =>
            Pick(pos, "Tiles/wall", "Tiles/wall_b", "Tiles/wall_c");

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

        // Works with Center or Bottom pivots: rests the sprite bottom on floorTopY.
        public static void SnapBottomTo(GameObject go, float floorTopY, float lift = 0.02f)
        {
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr == null || sr.sprite == null) return;
            float dy = floorTopY + lift - sr.bounds.min.y;
            go.transform.position += new Vector3(0f, dy, 0f);
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
