using UnityEngine;

namespace EcoDoTempo
{
    // Full-level backdrop from Resources/Backgrounds/{key}_past|_future.
    public class LevelBackground : MonoBehaviour
    {
        SpriteRenderer sr;
        string key;
        Vector2Int levelSize;

        public static LevelBackground Create(Transform root, string backgroundKey, Vector2Int size)
        {
            if (string.IsNullOrEmpty(backgroundKey)) return null;

            var go = new GameObject("Background");
            go.transform.SetParent(root, false);
            var bg = go.AddComponent<LevelBackground>();
            bg.sr = go.AddComponent<SpriteRenderer>();
            bg.sr.sortingOrder = -100;
            bg.key = backgroundKey;
            bg.levelSize = size;
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
            transform.localScale = new Vector3(levelSize.x / spriteW, levelSize.y / spriteH, 1f);
            transform.position = new Vector3((levelSize.x - 1) * 0.5f, (levelSize.y - 1) * 0.5f, 0f);
        }

        Sprite LoadSprite(Era era)
        {
            string suffix = era == Era.Past ? "past" : "future";
            return ArtCatalog.Get($"Backgrounds/{key}_{suffix}")
                ?? ArtCatalog.Get($"Backgrounds/{key}")
                ?? ArtCatalog.Get($"Backgrounds/{key}_past");
        }
    }
}
