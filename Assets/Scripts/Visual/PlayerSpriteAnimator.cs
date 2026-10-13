using System;
using System.Linq;
using UnityEngine;

namespace EcoDoTempo
{
    // Walk sheet plus the five-frame jump sheet (prep, rise, apex, fall, land).
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
        const int JumpFrameSize = 32;

        SpriteRenderer sr;
        Sprite[] frames;
        Sprite[] jumpFrames;
        Sprite[] idleFrames;
        float timer;
        float idleTimer;
        int idleIndex;
        float airborneTime;
        float landTimer;
        int index;
        bool facingRight = true;
        bool wasGrounded = true;

        public static PlayerSpriteAnimator TryAttach(GameObject body)
        {
            var sprites = LoadWalkSprites();
            var jumps = LoadJumpFrames();
            var idles = LoadIdleFrames();
            if ((sprites == null || sprites.Length == 0) && (jumps == null || jumps.Length == 0)
                && (idles == null || idles.Length == 0))
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

            var first = sprites != null && sprites.Length > 0 ? sprites[0]
                : idles != null && idles.Length > 0 ? idles[0]
                : jumps[0];
            sr.sprite = first;
            sr.color = Color.white;
            FitHeight(body.transform, sr, first);

            var anim = body.AddComponent<PlayerSpriteAnimator>();
            anim.sr = sr;
            anim.frames = sprites ?? Array.Empty<Sprite>();
            anim.jumpFrames = jumps ?? Array.Empty<Sprite>();
            anim.idleFrames = idles ?? Array.Empty<Sprite>();
            Debug.Log($"[Eco do Tempo] Lia: {anim.frames.Length} andar, {anim.idleFrames.Length} parada, {anim.jumpFrames.Length} pulo.");
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
                if (sprites != null && sprites.Length > 1)
                    return SortFrames(sprites);
                if (sprites != null && sprites.Length == 1 && sprites[0].rect.width <= JumpFrameSize + 1f)
                    return sprites;
            }

            var any = Resources.LoadAll<Sprite>("Characters");
            if (any == null || any.Length == 0) return Array.Empty<Sprite>();
            var walk = any.Where(s => s.texture != null
                && s.texture.name.IndexOf("jump", StringComparison.OrdinalIgnoreCase) < 0
                && s.texture.name.IndexOf("idle", StringComparison.OrdinalIgnoreCase) < 0).ToArray();
            if (walk.Length == 0) return Array.Empty<Sprite>();
            var byTexture = walk.GroupBy(s => s.texture).OrderByDescending(g => g.Count());
            return SortFrames(byTexture.First().ToArray());
        }

        static Sprite[] LoadJumpFrames() => LoadSheet("Characters/lia_jump", 5);

        static Sprite[] LoadIdleFrames() => LoadSheet("Characters/lia_idle", 4);

        static Sprite[] LoadSheet(string path, int minFrames)
        {
            var sliced = Resources.LoadAll<Sprite>(path);
            if (sliced != null && sliced.Length >= minFrames && sliced[0].rect.width <= JumpFrameSize + 1f)
                return SortFrames(sliced);

            var tex = Resources.Load<Texture2D>(path);
            if (tex == null) return Array.Empty<Sprite>();
            return SliceHorizontal(tex, JumpFrameSize);
        }

        static Sprite[] SliceHorizontal(Texture2D tex, int frame)
        {
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;
            int count = Mathf.Max(1, tex.width / frame);
            var frames = new Sprite[count];
            for (int i = 0; i < count; i++)
            {
                frames[i] = Sprite.Create(tex, new Rect(i * frame, 0, frame, frame), new Vector2(0.5f, 0.5f), frame);
                frames[i].name = tex.name + "_" + i;
            }
            return frames;
        }

        static Sprite[] SortFrames(Sprite[] sprites)
        {
            return sprites.OrderBy(s => s.name, StringComparer.OrdinalIgnoreCase).ToArray();
        }

        public void Tick(float horizontal, bool grounded, float verticalVelocity)
        {
            if (sr == null) return;

            if (horizontal > 0.01f) facingRight = true;
            else if (horizontal < -0.01f) facingRight = false;
            sr.flipX = !facingRight;

            if (jumpFrames != null && jumpFrames.Length >= 5 && ShowJump(grounded, verticalVelocity))
                return;

            wasGrounded = grounded;
            if (grounded && Mathf.Abs(horizontal) < 0.01f && idleFrames != null && idleFrames.Length > 0)
            {
                idleTimer += Time.deltaTime;
                float idleStep = 1f / 4f;
                while (idleTimer >= idleStep)
                {
                    idleTimer -= idleStep;
                    idleIndex = (idleIndex + 1) % idleFrames.Length;
                }
                sr.sprite = idleFrames[idleIndex];
                return;
            }
            idleTimer = 0f;
            idleIndex = 0;

            if (frames == null || frames.Length == 0)
            {
                if (jumpFrames != null && jumpFrames.Length > 0)
                    sr.sprite = jumpFrames[Mathf.Min(4, jumpFrames.Length - 1)];
                return;
            }

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

        // prep, rise, apex, fall, then a short land pose.
        bool ShowJump(bool grounded, float verticalVelocity)
        {
            if (!grounded)
            {
                if (wasGrounded) airborneTime = 0f;
                wasGrounded = false;
                landTimer = 0f;
                airborneTime += Time.deltaTime;
                int frame = airborneTime < 0.08f ? 0
                    : verticalVelocity > 1.2f ? 1
                    : verticalVelocity < -1.2f ? 3
                    : 2;
                sr.sprite = jumpFrames[frame];
                return true;
            }

            if (!wasGrounded)
            {
                wasGrounded = true;
                landTimer = 0.08f;
            }
            if (landTimer > 0f)
            {
                landTimer -= Time.deltaTime;
                sr.sprite = jumpFrames[4];
                return true;
            }
            return false;
        }
    }
}
