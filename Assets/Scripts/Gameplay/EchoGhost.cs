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
        readonly List<RaycastHit2D> castHits = new List<RaycastHit2D>();
        bool settled;

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

            if (!settled)
            {
                settled = true;
                SettleOnGround();
            }
            // Become a step once nothing solid with weight (Lia, a crate) stands inside it.
            if (col.isTrigger && !OverlapsBody()) col.isTrigger = false;
        }

        // A recording can end mid-jump. The echo lands on whatever is below instead of hovering.
        void SettleOnGround()
        {
            var solids = new ContactFilter2D();
            solids.useTriggers = false;
            int count = Physics2D.BoxCast(rb.position, col.size * 0.98f, 0f, Vector2.down, solids, castHits, 30f);
            float best = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                var hit = castHits[i];
                if (hit.collider == col || hit.collider.GetComponentInParent<PlayerController>() != null) continue;
                // Land only on ground that stays put: not on moving parts, not on one-era blocks.
                if (hit.collider.GetComponentInParent<Mover>() != null) continue;
                var timed = hit.collider.GetComponent<TimeObject>();
                if (timed != null && timed.Presence != EraPresence.Both) continue;
                if (hit.distance < best) best = hit.distance;
            }
            if (best < float.MaxValue && best > 0.001f)
            {
                var landed = rb.position + Vector2.down * best;
                rb.position = landed;
                transform.position = landed;
            }
        }

        void Update()
        {
            if (sr == null) return;
            float flicker = 0.85f + Mathf.Sin(Time.time * 7f) * 0.1f;
            var c = Palette.Echo;
            c.a *= flicker * (Replaying ? 1f : 0.85f);
            sr.color = c;
        }

        bool OverlapsBody()
        {
            int count = Physics2D.OverlapBox(rb.position, col.size * 0.98f, 0f, TimeBodies.AllContacts, hits);
            for (int i = 0; i < count; i++)
            {
                if (hits[i] == col || hits[i].isTrigger) continue;
                if (hits[i].GetComponentInParent<PlayerController>() != null
                    || hits[i].GetComponentInParent<TimeCrate>() != null) return true;
            }
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
                // The echo pulls levers and plants seeds; reading notes or turning clocks stays with Lia.
                if (!(interactable is Lever) && !(interactable is SeedSpot)) continue;
                float d = Vector2.Distance(at, hits[i].transform.position);
                if (d < bestDistance) { best = interactable; bestDistance = d; }
            }
            best?.Interact(gm.CurrentEra);
        }
    }
}
