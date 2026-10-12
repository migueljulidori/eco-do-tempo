using System;
using System.Linq;
using UnityEngine;

namespace EcoDoTempo
{
    // Plays a walk sheet from Resources/Characters (any PNG, prefers lia_walk).
    public class PlayerSpriteAnimator : MonoBehaviour
    {
        static readonly string[] PreferredPaths =
        {
            "Characters/lia_walk",
            "Characters/Lia_walk",
            "Characters/lia",
            "Characters/player",
        };

        const float FramesPerSecond = 10f;
        const float TargetHeight = 0.95f;

        SpriteRenderer sr;
        Sprite[] frames;
        float timer;
        int index;
        bool facingRight = true;

        public static PlayerSpriteAnimator TryAttach(GameObject body)
        {
            var sprites = LoadWalkSprites();
            if (sprites == null || sprites.Length == 0)
            {
                Debug.LogWarning(
                    "[Eco do Tempo] Sprite da Lia não encontrado. Coloque o PNG em:\n" +
                    "  Assets/Resources/Characters/lia_walk.png\n" +
                    "Depois no Inspector: Texture Type = Sprite (2D and UI), Sprite Mode = Multiple, " +
                    "Filter Mode = Point, Apply → Sprite Editor → Slice (Grid) → Apply. Veja o Console.");
                return null;
            }

            var sr = body.GetComponent<SpriteRenderer>();
            if (sr == null) return null;

            Debug.Log($"[Eco do Tempo] Lia usando {sprites.Length} frame(s) de '{sprites[0].texture.name}'.");

            sr.sprite = sprites[0];
            sr.color = Color.white;
            FitHeight(body.transform, sr, sprites[0]);

            var anim = body.AddComponent<PlayerSpriteAnimator>();
            anim.sr = sr;
            anim.frames = sprites;
            return anim;
        }

        static void FitHeight(Transform body, SpriteRenderer sr, Sprite sprite)
        {
            float nativeH = Mathf.Max(0.01f, sprite.bounds.size.y);
            float s = TargetHeight / nativeH;
            body.localScale = new Vector3(s, s, 1f);
            sr.sprite = sprite;
        }

        static Sprite[] LoadWalkSprites()
        {
            foreach (var path in PreferredPaths)
            {
                var sprites = Resources.LoadAll<Sprite>(path);
                if (sprites != null && sprites.Length > 0)
                    return SortFrames(sprites);
            }

            // Any sprite sheet inside Resources/Characters/
            var any = Resources.LoadAll<Sprite>("Characters");
            if (any == null || any.Length == 0) return Array.Empty<Sprite>();

            var byTexture = any.GroupBy(s => s.texture).OrderByDescending(g => g.Count());
            return SortFrames(byTexture.First().ToArray());
        }

        static Sprite[] SortFrames(Sprite[] sprites)
        {
            return sprites
                .OrderBy(s => s.name, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        public void Tick(float horizontal, bool grounded)
        {
            if (sr == null || frames == null || frames.Length == 0) return;

            if (horizontal > 0.01f) facingRight = true;
            else if (horizontal < -0.01f) facingRight = false;
            sr.flipX = !facingRight;

            bool walking = grounded && Mathf.Abs(horizontal) > 0.01f;
            if (!walking || frames.Length == 1)
            {
                index = 0;
                timer = 0f;
                sr.sprite = frames[0];
                return;
            }

            timer += Time.deltaTime;
            float step = 1f / FramesPerSecond;
            while (timer >= step)
            {
                timer -= step;
                index = (index + 1) % frames.Length;
                sr.sprite = frames[index];
            }
        }
    }
}
