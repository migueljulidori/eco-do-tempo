using UnityEngine;

namespace EcoDoTempo
{
    // Backdrop from Resources/Backgrounds/{key}_past|_future.
    // Keeps aspect ratio and aligns the painted ledge with the gameplay floor.
    public class LevelBackground : MonoBehaviour
    {
        SpriteRenderer sr;
        string key;
        Vector2Int levelSize;
        float floorWorldY;

        public static LevelBackground Create(Transform root, string backgroundKey, Vector2Int size, float floorCellY)
        {
            if (string.IsNullOrEmpty(backgroundKey)) return null;

            var go = new GameObject("Background");
            go.transform.SetParent(root, false);
            var bg = go.AddComponent<LevelBackground>();
            bg.sr = go.AddComponent<SpriteRenderer>();
            bg.sr.sortingOrder = -100;
            bg.key = backgroundKey;
            bg.levelSize = size;
            bg.floorWorldY = floorCellY - 0.5f; // top of the solid floor under the walkway
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

            // Fit width to the level; never stretch.
            float scale = levelSize.x / spriteW;
            transform.localScale = new Vector3(scale, scale, 1f);

            float scaledH = spriteH * scale;
            float centerX = (levelSize.x - 1) * 0.5f;

            // Painted floor ledge sits ~22% up from the bottom of the plate.
            const float LedgeFromBottom = 0.22f;
            float centerY = floorWorldY - (LedgeFromBottom - 0.5f) * scaledH;
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
