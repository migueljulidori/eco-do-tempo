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

        // Chaotic mix of floor slabs + stone blocks (not a repeating stripe).
        public static string PickFloor(Vector2 pos)
        {
            return Pick(pos,
                "Tiles/block_stone_past", "Tiles/floor_a", "Tiles/floor_b", "Tiles/floor_c",
                "Tiles/block_metal_past", "Tiles/wall_b");
        }

        public static string PickWall(Vector2 pos)
        {
            return Pick(pos,
                "Tiles/block_stone_past", "Tiles/wall", "Tiles/wall_b", "Tiles/wall_c",
                "Tiles/block_metal_past");
        }

        public static string PickPastBlock(Vector2 pos) =>
            Pick(pos, "Tiles/block_wood_past", "Tiles/block_past", "Tiles/bridge_past");

        public static string PickFutureBlock(Vector2 pos) =>
            Pick(pos, "Tiles/block_wood_future", "Tiles/block_stone_future", "Tiles/block_metal_future", "Tiles/block_future");

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

            // Extra variety for tiles: occasional horizontal flip.
            if (resourcePath.StartsWith("Tiles/floor") || resourcePath.StartsWith("Tiles/wall"))
            {
                int h = go.transform.position.GetHashCode();
                sr.flipX = (h & 1) == 0;
            }

            if (go.GetComponent<ArtSpriteMarker>() == null)
                go.AddComponent<ArtSpriteMarker>();
            return true;
        }

        // Works with Center or Bottom pivots: rests the sprite bottom on floorTopY.
        // lift keeps feet/props above the tile lip so they don't sink into the floor art.
        public const float FloorClearance = 0.22f;

        public static void SnapBottomTo(GameObject go, float floorTopY, float lift = -1f)
        {
            if (lift < 0f) lift = FloorClearance;
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
