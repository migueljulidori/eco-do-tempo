using System.Collections.Generic;
using UnityEngine;

namespace EcoDoTempo
{
    public class PlayerController : MonoBehaviour
    {
        static readonly Vector2 BodySize = new Vector2(0.8f, 0.9f);
        const float Speed = 6f;
        const float JumpVelocity = 12.5f;
        const float GravityScale = 3f;
        const float CoyoteTime = 0.1f;
        const float JumpBufferTime = 0.12f;
        const float InteractRadius = 0.8f;
        const float FallLimit = -5f;

        Rigidbody2D rb;
        BoxCollider2D body;
        Transform scarf;
        Vector2 spawn;
        float coyoteTimer;
        float jumpBufferTimer;
        readonly List<Collider2D> hits = new List<Collider2D>();
        static readonly ContactFilter2D AnyCollider = new ContactFilter2D().NoFilter();

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
            go.transform.position = spawn;

            var visual = SpriteFactory.CreateObject("Body", go.transform, spawn, BodySize, SpriteFactory.Square, Palette.Player, 10);
            visual.transform.localPosition = Vector3.zero;
            var scarf = SpriteFactory.CreateObject("Scarf", go.transform, spawn, new Vector2(0.9f, 0.15f), SpriteFactory.Square, Palette.PlayerScarf, 11);
            scarf.transform.localPosition = new Vector3(0f, 0.1f, 0f);

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = GravityScale;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var body = go.AddComponent<BoxCollider2D>();
            body.size = BodySize - Vector2.one * 0.04f;
            body.edgeRadius = 0.02f;
            body.sharedMaterial = new PhysicsMaterial2D("NoFriction") { friction = 0f, bounciness = 0f };

            var player = go.AddComponent<PlayerController>();
            player.rb = rb;
            player.body = body;
            player.scarf = scarf.transform;
            player.spawn = spawn;
            return player;
        }

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm.State != GameState.Playing)
            {
                Velocity = new Vector2(0f, Velocity.y);
                Nearby = null;
                return;
            }

            float dt = Time.deltaTime;
            float input = GameInput.Horizontal;
            bool grounded = IsGrounded();

            coyoteTimer = grounded ? CoyoteTime : coyoteTimer - dt;
            jumpBufferTimer = GameInput.JumpPressed ? JumpBufferTime : jumpBufferTimer - dt;

            var v = Velocity;
            v.x = input * Speed;
            if (jumpBufferTimer > 0f && coyoteTimer > 0f)
            {
                v.y = JumpVelocity;
                jumpBufferTimer = 0f;
                coyoteTimer = 0f;
            }
            if (GameInput.JumpReleased && v.y > 0f) v.y *= 0.5f;
            Velocity = v;

            if (input != 0f) scarf.localPosition = new Vector3(-0.1f * Mathf.Sign(input), 0.1f, 0f);

            Nearby = FindInteractable(gm.CurrentEra);
            if (Nearby != null && GameInput.InteractPressed) Nearby.Interact(gm.CurrentEra);

            if (transform.position.y < FallLimit) Die();
        }

        bool IsGrounded()
        {
            var feet = (Vector2)transform.position + Vector2.down * (BodySize.y * 0.5f + 0.05f);
            int count = Physics2D.OverlapBox(feet, new Vector2(BodySize.x * 0.85f, 0.1f), 0f, AnyCollider, hits);
            for (int i = 0; i < count; i++)
                if (hits[i] != body && !hits[i].isTrigger) return true;
            return false;
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
            rb.position = spawn;
            transform.position = spawn;
            Velocity = Vector2.zero;
            GameManager.Instance.OnPlayerDied();
        }
    }
}
