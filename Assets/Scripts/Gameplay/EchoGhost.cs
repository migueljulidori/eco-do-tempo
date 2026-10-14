using System.Collections.Generic;
using UnityEngine;

namespace EcoDoTempo
{
    public struct EchoFrame
    {
        public Vector2 Position;
        public Sprite Sprite;
        public bool FlipX;
        public Vector2 VisualOffset;
        public Vector2 VisualScale;
        public bool Interact;
    }

    // Lia's echo: replays a recording frame by frame, then stays where it ended.
    // While replaying it passes through things; once it stops it is solid enough to stand on.
    public class EchoGhost : MonoBehaviour
    {
        const float InteractRadius = 0.8f;

        EchoFrame[] frames;
        int index;
        Rigidbody2D rb;
        BoxCollider2D col;
        SpriteRenderer sr;
        Transform visual;
        readonly List<Collider2D> hits = new List<Collider2D>();

        public bool Replaying => frames != null && index < frames.Length;

        public static EchoGhost Create(Transform parent, EchoFrame[] frames, Vector2 bodySize)
        {
            var go = new GameObject("Eco da Lia");
            go.transform.SetParent(parent, false);
            go.transform.position = frames[0].Position;

            var body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;

            var box = go.AddComponent<BoxCollider2D>();
            box.size = bodySize;
            box.isTrigger = true;

            var visualGo = new GameObject("Corpo");
            visualGo.transform.SetParent(go.transform, false);
            var renderer = SpriteFactory.AddRenderer(visualGo, frames[0].Sprite != null ? frames[0].Sprite : SpriteFactory.Square,
                Palette.Echo, 19);

            var ghost = go.AddComponent<EchoGhost>();
            ghost.frames = frames;
            ghost.rb = body;
            ghost.col = box;
            ghost.sr = renderer;
            ghost.visual = visualGo.transform;
            ghost.ShowFrame(frames[0]);
            SoftGlow.Attach(visualGo, new Color(0.5f, 0.9f, 1f, 0.5f), 1.6f, 18);
            return ghost;
        }

        void ShowFrame(EchoFrame frame)
        {
            if (frame.Sprite != null) sr.sprite = frame.Sprite;
            sr.flipX = frame.FlipX;
            visual.localPosition = frame.VisualOffset;
            visual.localScale = new Vector3(frame.VisualScale.x, frame.VisualScale.y, 1f);
        }

        void FixedUpdate()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.State != GameState.Playing || gm.TimeFrozen) return;

            if (Replaying)
            {
                var frame = frames[index++];
                rb.MovePosition(frame.Position);
                ShowFrame(frame);
                if (frame.Interact) TryInteract(gm, frame.Position);
                return;
            }

            // Settled: become a step, unless Lia is standing inside it right now.
            if (col.isTrigger && !OverlapsPlayer()) col.isTrigger = false;
        }

        void Update()
        {
            if (sr == null) return;
            float flicker = 0.85f + Mathf.Sin(Time.time * 7f) * 0.1f;
            var c = Palette.Echo;
            c.a *= flicker * (Replaying ? 1f : 0.85f);
            sr.color = c;
        }

        bool OverlapsPlayer()
        {
            int count = Physics2D.OverlapBox(rb.position, col.size * 0.98f, 0f, TimeBodies.AllContacts, hits);
            for (int i = 0; i < count; i++)
                if (hits[i].GetComponentInParent<PlayerController>() != null) return true;
            return false;
        }

        void TryInteract(GameManager gm, Vector2 at)
        {
            int count = Physics2D.OverlapCircle(at, InteractRadius, TimeBodies.AllContacts, hits);
            IInteractable best = null;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                var interactable = hits[i].GetComponent<IInteractable>();
                if (interactable == null) continue;
                float d = Vector2.Distance(at, hits[i].transform.position);
                if (d < bestDistance) { best = interactable; bestDistance = d; }
            }
            best?.Interact(gm.CurrentEra);
        }
    }
}
