using UnityEngine;

namespace EcoDoTempo
{
    // Backdrop from Resources/Backgrounds/{key}_past|_future.
    // Covers the full camera view (no letterbox bars) and aligns the painted ledge with the floor.
    public class LevelBackground : MonoBehaviour
    {
        SpriteRenderer sr;
        [SerializeField] string key;
        [SerializeField] Vector2Int levelSize;
        [SerializeField] float floorTopY;
        bool preservePlacement;
        Vector3 placedPosition;
        Vector3 placedScale;

        // Keeps a hand-placed backdrop where it was saved. The era only swaps the sprite.
        public void UsePlacedSprite()
        {
            if (sr == null) sr = GetComponent<SpriteRenderer>();
            preservePlacement = true;
            placedPosition = transform.position;
            placedScale = transform.localScale;
        }

        // Lit lip of the painted floor on salao_past (fraction up from the bottom).
        const float PastLedgeFromBottom = 0.26f;
        const float FutureLedgeFromBottom = 0.26f;
        // oficina_past/future are 2900×1154. The lit lip is pixel row 664 from the top,
        // so 490 px sit below it. Pits in that art line up with the level columns only
        // when the image is scaled to the level width exactly.
        const float OficinaLedgeFromBottom = 490f / 1154f;
        // Extra padding so the plate still covers after camera clamp / aspect changes.
        const float CoverPad = 2f;

        public static LevelBackground Create(Transform root, string backgroundKey, Vector2Int size, float floorTopY)
        {
            if (string.IsNullOrEmpty(backgroundKey)) return null;

            var go = new GameObject("Background");
            go.transform.SetParent(root, false);
            var bg = go.AddComponent<LevelBackground>();
            bg.sr = go.AddComponent<SpriteRenderer>();
            bg.sr.sortingOrder = -100;
            bg.key = backgroundKey;
            bg.levelSize = size;
            bg.floorTopY = floorTopY;
            bg.Apply(Era.Past);
            return bg;
        }

        public void Apply(Era era)
        {
            var sprite = LoadSprite(era);
            if (sprite == null)
            {
                sr.enabled = false;
                return;
            }

            sr.enabled = true;
            sr.sprite = sprite;
            sr.color = Color.white;
            if (preservePlacement)
            {
                transform.position = placedPosition;
                transform.localScale = placedScale;
                return;
            }

            float spriteW = Mathf.Max(0.01f, sprite.bounds.size.x);
            float spriteH = Mathf.Max(0.01f, sprite.bounds.size.y);

            // Fit the level width. Avoid height-upscaling: that thickens the painted
            // under-floor masonry into a brown bar. Side/top gaps use the near-black clear.
            float halfH = 6f;
            float aspect = 16f / 9f;
            var cam = Camera.main;
            if (cam != null && cam.orthographic)
            {
                halfH = cam.orthographicSize;
                aspect = Mathf.Max(0.1f, cam.aspect);
            }
            // Oficina art is one pixel column per map cell. Extra bleed would slide the pits.
            bool columnFit = key == "oficina";
            float coverW = columnFit
                ? levelSize.x
                : Mathf.Max(levelSize.x, halfH * aspect * 2f) + CoverPad;
            float scale = coverW / spriteW;
            transform.localScale = new Vector3(scale, scale, 1f);

            float scaledH = spriteH * scale;
            float centerX = (levelSize.x - 1) * 0.5f;

            float ledgeFromBottom = key == "oficina"
                ? OficinaLedgeFromBottom
                : era == Era.Past ? PastLedgeFromBottom : FutureLedgeFromBottom;
            // Place the plate so the painted ledge sits on the gameplay floor top.
            float centerY = floorTopY - (ledgeFromBottom - 0.5f) * scaledH;
            transform.position = new Vector3(centerX, centerY, 0f);
        }

        Sprite LoadSprite(Era era)
        {
            string suffix = era == Era.Past ? "past" : "future";
            return ArtCatalog.Get($"Backgrounds/{key}_{suffix}_1472x576")
                ?? ArtCatalog.Get($"Backgrounds/{key}_{suffix}")
                ?? ArtCatalog.Get($"Backgrounds/{key}")
                ?? ArtCatalog.Get($"Backgrounds/{key}_past_1472x576")
                ?? ArtCatalog.Get($"Backgrounds/{key}_past");
        }
    }
}
