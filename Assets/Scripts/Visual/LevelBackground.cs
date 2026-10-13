using UnityEngine;

namespace EcoDoTempo
{
    // Backdrop from Resources/Backgrounds/{key}_past|_future.
    // Covers the full camera view (no letterbox bars) and aligns the painted ledge with the floor.
    public class LevelBackground : MonoBehaviour
    {
        SpriteRenderer sr;
        string key;
        Vector2Int levelSize;
        float floorTopY;

        // Lit lip of the painted floor on salao_past (fraction up from the bottom).
        const float PastLedgeFromBottom = 0.26f;
        const float FutureLedgeFromBottom = 0.26f;
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

            float spriteW = Mathf.Max(0.01f, sprite.bounds.size.x);
            float spriteH = Mathf.Max(0.01f, sprite.bounds.size.y);

            // Cover the camera: at least the level width and the full orthographic view.
            float halfH = 6f;
            float aspect = 16f / 9f;
            var cam = Camera.main;
            if (cam != null && cam.orthographic)
            {
                halfH = cam.orthographicSize;
                aspect = Mathf.Max(0.1f, cam.aspect);
            }
            float coverW = Mathf.Max(levelSize.x, halfH * aspect * 2f) + CoverPad;
            float coverH = halfH * 2f + CoverPad;
            float scale = Mathf.Max(coverW / spriteW, coverH / spriteH);
            transform.localScale = new Vector3(scale, scale, 1f);

            float scaledH = spriteH * scale;
            float centerX = (levelSize.x - 1) * 0.5f;

            float ledgeFromBottom = era == Era.Past ? PastLedgeFromBottom : FutureLedgeFromBottom;
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
