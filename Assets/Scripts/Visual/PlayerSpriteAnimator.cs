using UnityEngine;

namespace EcoDoTempo
{
    // Plays a horizontal walk sheet loaded from Resources/Characters/lia_walk.
    public class PlayerSpriteAnimator : MonoBehaviour
    {
        public const string ResourcePath = "Characters/lia_walk";
        const float FramesPerSecond = 10f;

        SpriteRenderer sr;
        Sprite[] frames;
        float timer;
        int index;
        bool facingRight = true;

        public static PlayerSpriteAnimator TryAttach(GameObject body)
        {
            var sprites = Resources.LoadAll<Sprite>(ResourcePath);
            if (sprites == null || sprites.Length == 0) return null;

            var sr = body.GetComponent<SpriteRenderer>();
            if (sr == null) return null;

            sr.sprite = sprites[0];
            sr.color = Color.white;
            body.transform.localScale = Vector3.one;

            var anim = body.AddComponent<PlayerSpriteAnimator>();
            anim.sr = sr;
            anim.frames = sprites;
            return anim;
        }

        public void Tick(float horizontal, bool grounded)
        {
            if (sr == null || frames == null || frames.Length == 0) return;

            if (horizontal > 0.01f) facingRight = true;
            else if (horizontal < -0.01f) facingRight = false;
            sr.flipX = !facingRight;

            bool walking = grounded && Mathf.Abs(horizontal) > 0.01f;
            if (!walking)
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
