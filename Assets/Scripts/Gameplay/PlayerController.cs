using System.Collections.Generic;
using UnityEngine;

namespace EcoDoTempo
{
    public class PlayerController : MonoBehaviour
    {
        static readonly Vector2 BodySize = new Vector2(0.8f, 0.9f);
        public static Vector2 BodyColliderSize => new Vector2(BodySize.x - 0.06f, BodySize.y - 0.08f);
        const float Speed = 6f;
        const float JumpVelocity = 12.5f;
        const float GravityScale = 3f;
        const float CoyoteTime = 0.1f;
        const float JumpBufferTime = 0.12f;
        const float InteractRadius = 0.8f;
        const float FallLimit = -5f;

        Rigidbody2D rb;
        BoxCollider2D body;
        Transform visual;
        Transform scarf;
        PlayerSpriteAnimator spriteAnim;
        Vector2 spawn;
        float coyoteTimer;
        float jumpBufferTimer;
        Mover groundMover;
        bool groundIsSafe;
        // Last safe spot: solid in every era, not moving, away from hazards. Deaths respawn here.
        Vector2 checkpoint;
        Era checkpointEra;
        float safeTime;
        const float CheckpointDelay = 0.3f;
        const float HazardClearance = 2f;
        readonly List<Collider2D> hits = new List<Collider2D>();
        static readonly ContactFilter2D AnyCollider = new ContactFilter2D().NoFilter();
        SpriteRenderer visualRenderer;

        public IInteractable Nearby { get; private set; }

        public Rect WorldRect
        {
            get
            {
                var size = BodySize - Vector2.one * 0.1f;
                return new Rect((Vector2)transform.position - size * 0.5f, size);
            }
        }

        Vector2 Velocity
        {
#if UNITY_6000_0_OR_NEWER
            get => rb.linearVelocity;
            set => rb.linearVelocity = value;
#else
            get => rb.velocity;
            set => rb.velocity = value;
#endif
        }

        public static PlayerController Create(Vector2 spawn, Transform parent)
        {
            var go = new GameObject("Lia");
            go.transform.SetParent(parent, false);
            // Spawn above the cell center so physics settles on the floor lip.
            go.transform.position = spawn + Vector2.up * 0.2f;

            var visual = SpriteFactory.CreateObject("Body", go.transform, spawn, BodySize, SpriteFactory.Square, Palette.Player, 20);
            visual.transform.localPosition = Vector3.zero;
            var anim = PlayerSpriteAnimator.TryAttach(visual);

            GameObject scarfGo = null;
            if (anim == null)
            {
                scarfGo = SpriteFactory.CreateObject("Scarf", go.transform, spawn, new Vector2(0.9f, 0.15f), SpriteFactory.Square, Palette.PlayerScarf, 11);
                scarfGo.transform.localPosition = new Vector3(0f, 0.1f, 0f);
            }

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = GravityScale;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var body = go.AddComponent<BoxCollider2D>();
            body.size = BodyColliderSize;
            body.offset = Vector2.zero;
            body.edgeRadius = 0.01f;
            body.sharedMaterial = new PhysicsMaterial2D("NoFriction") { friction = 0f, bounciness = 0f };

            float colliderBottom = body.offset.y - body.size.y * 0.5f;
            if (anim != null)
                ArtCatalog.AlignFeet(visual.transform, colliderBottom, ArtCatalog.PlayerFootClearance);

            var player = go.AddComponent<PlayerController>();
            player.rb = rb;
            player.body = body;
            player.visual = visual.transform;
            player.scarf = scarfGo != null ? scarfGo.transform : null;
            player.spriteAnim = anim;
            player.spawn = spawn + Vector2.up * 0.2f;
            player.checkpoint = player.spawn;
            player.checkpointEra = GameManager.Instance != null ? GameManager.Instance.Level.StartEra : Era.Past;
            return player;
        }

        void Awake() => EnsureBound();

        // Prefab copies do not keep the private references assigned in Create.
        public void EnsureBound()
        {
            if (rb == null) rb = GetComponent<Rigidbody2D>();
            if (body == null) body = GetComponent<BoxCollider2D>();
            if (visual == null && transform.childCount > 0) visual = transform.GetChild(0);
            if (spriteAnim == null && visual != null) spriteAnim = visual.GetComponent<PlayerSpriteAnimator>();
            if (spawn == Vector2.zero) spawn = transform.position;
            if (checkpoint == Vector2.zero)
            {
                checkpoint = spawn;
                checkpointEra = GameManager.Instance != null ? GameManager.Instance.Level.StartEra : Era.Past;
            }
        }

        void LateUpdate()
        {
            if (visual == null || body == null) return;
            float colliderBottom = body.offset.y - body.size.y * 0.5f;
            if (spriteAnim != null)
                ArtCatalog.AlignFeet(visual, colliderBottom, ArtCatalog.PlayerFootClearance);
        }

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm.State != GameState.Playing || !gm.InputReady)
            {
                Velocity = new Vector2(0f, Velocity.y);
                Nearby = null;
                return;
            }

            float dt = Time.deltaTime;
            float input = GameInput.Horizontal;
            bool grounded = IsGrounded();

            coyoteTimer = grounded ? CoyoteTime : coyoteTimer - dt;
            UpdateCheckpoint(grounded, gm.CurrentEra, dt);
            jumpBufferTimer = GameInput.JumpPressed ? JumpBufferTime : jumpBufferTimer - dt;

            float speed = Speed;
            if (grounded && input != 0f)
            {
                var crate = CrateAhead(Mathf.Sign(input));
                if (crate != null && TimeCrate.CanMoveNow)
                {
                    crate.Push(input);
                    speed = TimeCrate.PushSpeed;
                }
            }

            // Moving platforms carry whoever stands on them.
            var carry = groundMover != null ? groundMover.Velocity : Vector2.zero;

            var v = Velocity;
            v.x = input * speed + carry.x;
            bool jumped = false;
            if (jumpBufferTimer > 0f && coyoteTimer > 0f)
            {
                v.y = JumpVelocity + Mathf.Max(0f, carry.y);
                jumpBufferTimer = 0f;
                coyoteTimer = 0f;
                jumped = true;
            }
            if (!jumped && carry.y < 0f && v.y <= 0.01f) v.y = carry.y;
            if (GameInput.JumpReleased && v.y > 0f) v.y *= 0.5f;
            Velocity = v;

            if (scarf != null && input != 0f)
                scarf.localPosition = new Vector3(-0.1f * Mathf.Sign(input), 0.1f, 0f);
            if (spriteAnim != null) spriteAnim.Tick(input, grounded, Velocity.y);

            Nearby = FindInteractable(gm.CurrentEra);
            if (GameInput.InteractPressed)
            {
                gm.Echo.NoteInteract();
                if (Nearby != null) Nearby.Interact(gm.CurrentEra);
            }

            if (transform.position.y < FallLimit) Die();
        }

        void UpdateCheckpoint(bool grounded, Era era, float dt)
        {
            if (!grounded || !groundIsSafe || Mathf.Abs(Velocity.y) > 0.1f || HazardNearby())
            {
                safeTime = 0f;
                return;
            }
            safeTime += dt;
            if (safeTime < CheckpointDelay) return;
            checkpoint = rb.position;
            checkpointEra = era;
        }

        bool HazardNearby()
        {
            int count = Physics2D.OverlapCircle(rb.position, HazardClearance, AnyCollider, hits);
            for (int i = 0; i < count; i++)
                if (hits[i].enabled && hits[i].GetComponentInParent<Hazard>() != null) return true;
            return false;
        }

        // Ground that is there in every era and never moves on its own.
        static bool IsSafeGround(Collider2D col)
        {
            if (col.GetComponentInParent<Mover>() != null || col.GetComponentInParent<TimeCrate>() != null
                || col.GetComponentInParent<EchoGhost>() != null || col.GetComponent<Gate>() != null)
                return false;
            var timed = col.GetComponent<TimeObject>();
            return timed == null || (timed.Presence == EraPresence.Both && !timed.ForceHidden);
        }

        bool IsGrounded()
        {
            groundMover = null;
            groundIsSafe = true;
            bool grounded = false;
            var feet = (Vector2)transform.position + Vector2.down * (BodySize.y * 0.5f + 0.05f);
            int count = Physics2D.OverlapBox(feet, new Vector2(BodySize.x * 0.85f, 0.1f), 0f, AnyCollider, hits);
            for (int i = 0; i < count; i++)
            {
                if (hits[i] == body || hits[i].isTrigger) continue;
                grounded = true;
                if (!IsSafeGround(hits[i])) groundIsSafe = false;
                var mover = hits[i].GetComponentInParent<Mover>();
                if (mover != null) groundMover = mover;
            }
            return grounded;
        }

        TimeCrate CrateAhead(float dir)
        {
            var probe = (Vector2)transform.position + new Vector2(dir * (BodySize.x * 0.5f + 0.08f), -0.05f);
            int count = Physics2D.OverlapBox(probe, new Vector2(0.12f, BodySize.y * 0.6f), 0f, AnyCollider, hits);
            for (int i = 0; i < count; i++)
            {
                if (hits[i].isTrigger) continue;
                var crate = hits[i].GetComponentInParent<TimeCrate>();
                if (crate != null) return crate;
            }
            return null;
        }

        // One frame of Lia for the echo to replay.
        public EchoFrame Snapshot(bool interact)
        {
            if (visualRenderer == null && visual != null) visualRenderer = visual.GetComponent<SpriteRenderer>();
            return new EchoFrame
            {
                Position = rb.position,
                Sprite = visualRenderer != null ? visualRenderer.sprite : null,
                FlipX = visualRenderer != null && visualRenderer.flipX,
                VisualOffset = visual != null ? (Vector2)visual.localPosition : Vector2.zero,
                VisualScale = visual != null ? (Vector2)visual.localScale : Vector2.one,
                Interact = interact,
                Era = GameManager.Instance.CurrentEra,
            };
        }

        public void TeleportTo(Vector2 position)
        {
            rb.position = position;
            transform.position = position;
            Velocity = Vector2.zero;
        }

        IInteractable FindInteractable(Era era)
        {
            int count = Physics2D.OverlapCircle(transform.position, InteractRadius, AnyCollider, hits);
            IInteractable best = null;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                var interactable = hits[i].GetComponent<IInteractable>();
                if (interactable == null) continue;
                float d = Vector2.Distance(transform.position, hits[i].transform.position);
                if (d < bestDistance) { best = interactable; bestDistance = d; }
            }
            return best;
        }

        public void Die()
        {
            TeleportTo(checkpoint);
            safeTime = 0f;
            GameManager.Instance.OnPlayerDied(checkpointEra);
        }
    }
}
