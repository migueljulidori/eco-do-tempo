using System.Collections.Generic;
using UnityEngine;

namespace EcoDoTempo
{
    // Optional pixel art under Assets/Resources. Missing files keep the geometric look.
    public static class ArtCatalog
    {
        const float ImportPPU = 32f;
        static readonly Dictionary<string, Sprite> RuntimeSprites = new Dictionary<string, Sprite>();

        public static Sprite Get(string resourcePath)
        {
            var sprites = Resources.LoadAll<Sprite>(resourcePath);
            if (sprites != null && sprites.Length > 0) return sprites[0];

            var sprite = Resources.Load<Sprite>(resourcePath);
            if (sprite != null) return sprite;

            if (RuntimeSprites.TryGetValue(resourcePath, out var cached) && cached != null)
                return cached;

            var tex = Resources.Load<Texture2D>(resourcePath);
            if (tex == null) return null;

            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;
            Vector2 pivot = BottomPivotPath(resourcePath) ? new Vector2(0.5f, 0f) : new Vector2(0.5f, 0.5f);
            var created = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), pivot, ImportPPU);
            created.name = tex.name;
            RuntimeSprites[resourcePath] = created;
            return created;
        }

        // Bridge trim uses Center so it sits on the same grid cell as wood planks.
        static bool BottomPivotPath(string path) =>
            path == "Props/blockade" || path == "Props/door"
            || path == "Props/door_open" || path == "Props/spikes" || path == "Props/lever"
            || path == "Props/lever_pulled" || path == "Props/sapling" || path == "Props/inspect";

        public static bool Has(string resourcePath) => Get(resourcePath) != null;

        public static float FloorTop(Vector2 walkableCell) => walkableCell.y - 0.5f;

        public static string FirstExisting(params string[] paths)
        {
            for (int i = 0; i < paths.Length; i++)
                if (Has(paths[i])) return paths[i];
            return paths.Length > 0 ? paths[0] : null;
        }

        // Coherent walkable surface: two-tone checker, not a random mix.
        public static void PickFloorPair(Vector2 pos, out string past, out string future)
        {
            bool alt = ((int)pos.x & 1) == 0;
            past = alt
                ? FirstExisting("Tiles/floor_a", "Tiles/block_stone_past", "Tiles/wall")
                : FirstExisting("Tiles/floor_b", "Tiles/floor_a", "Tiles/block_stone_past");
            future = alt
                ? FirstExisting("Tiles/block_stone_future", "Tiles/block_metal_future", "Tiles/block_future")
                : FirstExisting("Tiles/block_metal_future", "Tiles/block_stone_future", "Tiles/block_future");
        }

        // Fill under the ledge: one family per era.
        public static void PickWallPair(Vector2 pos, out string past, out string future)
        {
            bool alt = (((int)pos.x + (int)pos.y) & 1) == 0;
            past = alt
                ? FirstExisting("Tiles/wall", "Tiles/block_stone_past")
                : FirstExisting("Tiles/wall_b", "Tiles/wall", "Tiles/block_stone_past");
            future = alt
                ? FirstExisting("Tiles/block_stone_future", "Tiles/block_metal_future")
                : FirstExisting("Tiles/block_metal_future", "Tiles/block_stone_future");
        }

        public static string PickPastBlock(Vector2 pos) =>
            FirstExisting("Tiles/block_stone_past", "Tiles/block_wood_past", "Tiles/block_past");

        public static string PickFutureBlock(Vector2 pos) =>
            FirstExisting("Tiles/block_stone_future", "Tiles/block_metal_future", "Tiles/block_future");

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

            var marker = go.GetComponent<ArtSpriteMarker>();
            if (marker == null) marker = go.AddComponent<ArtSpriteMarker>();
            return true;
        }

        // Fill exactly one grid cell and center the sprite on the transform position.
        public static void FitToCell(GameObject go, float cell = 1f)
        {
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr == null || sr.sprite == null) return;

            Vector3 cellCenter = go.transform.position;
            float nativeW = Mathf.Max(0.01f, sr.sprite.bounds.size.x);
            float nativeH = Mathf.Max(0.01f, sr.sprite.bounds.size.y);
            go.transform.localScale = new Vector3(cell / nativeW, cell / nativeH, 1f);

            Vector3 delta = cellCenter - sr.bounds.center;
            go.transform.position += delta;
        }

        public static ArtSpriteMarker SetEraArt(GameObject go, string past, string future, Vector2 scale)
        {
            var marker = go.GetComponent<ArtSpriteMarker>();
            if (marker == null) marker = go.AddComponent<ArtSpriteMarker>();
            marker.PastArt = past;
            marker.FutureArt = string.IsNullOrEmpty(future) ? past : future;
            marker.ArtScale = scale;
            return marker;
        }

        public const float FloorClearance = 0.12f; // props / pickups
        public const float PlayerFootClearance = 0.05f;

        // Prefer pre-cropped bridge_deck (no transparent pad). Fallback crops bridge_past at runtime.
        static Sprite bridgeSprite;

        public static Sprite GetBridgeSprite()
        {
            if (bridgeSprite != null) return bridgeSprite;

            var deck = Get("Tiles/bridge_deck");
            if (deck != null)
            {
                bridgeSprite = deck;
                return bridgeSprite;
            }

            Texture2D tex = Resources.Load<Texture2D>("Tiles/bridge_past");
            if (tex == null)
            {
                var full = Get("Tiles/bridge_past");
                if (full != null) tex = full.texture;
            }
            if (tex == null) return null;

            tex.filterMode = FilterMode.Point;
            // Opaque content of bridge_past.png: Unity rect (1,1,126,21) on 128×32.
            bridgeSprite = Sprite.Create(
                tex,
                new Rect(1f, 1f, 126f, 21f),
                new Vector2(0.5f, 0.5f),
                21f);
            bridgeSprite.name = "bridge_past_cropped";
            return bridgeSprite;
        }

        public static bool TryApplyBridge(GameObject go, float worldWidth)
        {
            var sprite = GetBridgeSprite();
            if (sprite == null) return false;

            var sr = go.GetComponent<SpriteRenderer>();
            if (sr == null) return false;

            sr.sprite = sprite;
            sr.color = Color.white;

            float nativeW = Mathf.Max(0.01f, sprite.bounds.size.x);
            float nativeH = Mathf.Max(0.01f, sprite.bounds.size.y);
            // Exact cell height; width matches the pit span.
            go.transform.localScale = new Vector3(worldWidth / nativeW, 1f / nativeH, 1f);

            if (go.GetComponent<ArtSpriteMarker>() == null)
                go.AddComponent<ArtSpriteMarker>();
            return true;
        }

        // Put bridge deck + its support colliders flush with a stone ledge top.
        public static void AlignBridgeToLedge(GameObject bridge, GameObject[] supports, float ledgeTop)
        {
            if (bridge != null)
            {
                SnapTopTo(bridge, ledgeTop);
                var sr = bridge.GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite != null)
                    ledgeTop = sr.bounds.max.y;
            }

            if (supports == null) return;
            for (int i = 0; i < supports.Length; i++)
            {
                var s = supports[i];
                if (s == null) continue;
                var col = s.GetComponent<BoxCollider2D>();
                float half = col != null ? col.size.y * 0.5f : 0.5f;
                float ox = col != null ? col.offset.y : 0f;
                // transform.y + offset.y + half = ledgeTop
                s.transform.position = new Vector3(
                    s.transform.position.x,
                    ledgeTop - ox - half,
                    s.transform.position.z);
            }
        }

        public static void SnapBottomTo(GameObject go, float floorTopY, float lift = -1f)
        {
            if (lift < 0f) lift = FloorClearance;
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr == null || sr.sprite == null) return;
            float dy = floorTopY + lift - sr.bounds.min.y;
            go.transform.position += new Vector3(0f, dy, 0f);
        }

        public static void SnapTopTo(GameObject go, float topY)
        {
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr == null || sr.sprite == null) return;
            float dy = topY - sr.bounds.max.y;
            go.transform.position += new Vector3(0f, dy, 0f);
        }

        // bridge_past.png: ~10px empty pad above the walking deck on a 32px canvas.
        const float BridgeDeckPadFromTop = 10f / 32f;

        public static void SnapBridgeDeckTo(GameObject go, float deckY)
        {
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr == null || sr.sprite == null) return;
            float h = sr.bounds.size.y;
            // Canvas top sits above the deck by the transparent pad.
            SnapTopTo(go, deckY + h * BridgeDeckPadFromTop);
        }

        public static void AlignFeet(Transform visual, float colliderBottomLocal, float clearance = -1f)
        {
            if (clearance < 0f) clearance = FloorClearance;
            var sr = visual.GetComponent<SpriteRenderer>();
            if (sr == null || sr.sprite == null) return;

            float parentY = visual.parent != null ? visual.parent.position.y : 0f;
            float targetWorldY = parentY + colliderBottomLocal + clearance;
            float dy = targetWorldY - sr.bounds.min.y;
            visual.position += new Vector3(0f, dy, 0f);
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
