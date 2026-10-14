using UnityEngine;

namespace EcoDoTempo
{
    // Iron crate. It only slides in the past; in the future rust holds it where the past left it.
    // Fermata pins it in place, even in mid-air.
    public class TimeCrate : MonoBehaviour
    {
        public const float PushSpeed = 2.6f;
        const float Size = 0.94f;
        const float FallLimit = -6f;

        Rigidbody2D rb;
        Vector2 home;
        float pushTimer;
        float pushDir;
        bool pinned;
        Vector2 velocityBeforePin;

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

        public static bool CanMoveNow
        {
            get
            {
                var gm = GameManager.Instance;
                return gm != null && gm.CurrentEra == Era.Past && !gm.TimeFrozen;
            }
        }

        public static TimeCrate Create(Transform root, Vector2 pos)
        {
            // Optional own art (Props/crate, Props/crate_future); the metal tile is the fallback.
            string pastArt = ArtCatalog.FirstExisting("Props/crate", "Tiles/block_metal_past");
            string futureArt = ArtCatalog.FirstExisting("Props/crate_future", "Props/crate", "Tiles/block_metal_future");
            var go = SpriteFactory.CreateObject("Caixa", root, pos, Vector2.one * Size, SpriteFactory.Square,
                Palette.PastBlock, 3);
            if (ArtCatalog.TryApply(go, pastArt, Vector2.one * Size))
                ArtCatalog.SetEraArt(go, pastArt, futureArt, Vector2.one * Size);
            if (!ArtCatalog.Has("Props/crate"))
            {
                // Pale rim so a movable crate never reads as a wall block.
                var rim = SpriteFactory.CreateObject("Contorno", go.transform, pos, Vector2.one, SpriteFactory.Square,
                    new Color(0.75f, 0.9f, 1f, 0.75f), 2);
                rim.transform.localScale = Vector3.one * 1.1f;
                rim.transform.position = pos;
            }

            var body = go.AddComponent<Rigidbody2D>();
            body.gravityScale = 3f;
            body.mass = 5f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;

            // Collider in local space; the transform already scales it to Size.
            var col = go.AddComponent<BoxCollider2D>();
            col.size = Vector2.one * 0.96f;
            col.edgeRadius = 0.02f;
            col.sharedMaterial = new PhysicsMaterial2D("CrateNoFriction") { friction = 0f, bounciness = 0f };

            var crate = go.AddComponent<TimeCrate>();
            crate.rb = body;
            crate.home = pos;
            TimeObject.Add(go, EraPresence.Both, Palette.PastBlock, Palette.FutureWall, solid: false);
            return crate;
        }

        void Awake()
        {
            if (rb == null) rb = GetComponent<Rigidbody2D>();
            if (home == Vector2.zero) home = transform.position;
        }

        public void Push(float dir)
        {
            pushDir = Mathf.Sign(dir);
            pushTimer = 0.08f;
        }

        void FixedUpdate()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            bool hold = gm.TimeFrozen || gm.State != GameState.Playing;
            if (hold)
            {
                if (!pinned)
                {
                    velocityBeforePin = Velocity;
                    rb.constraints = RigidbodyConstraints2D.FreezeAll;
                    pinned = true;
                }
                return;
            }
            if (pinned)
            {
                pinned = false;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
                Velocity = new Vector2(0f, velocityBeforePin.y);
            }

            pushTimer -= Time.fixedDeltaTime;
            bool pushing = pushTimer > 0f && CanMoveNow;
            // Only an actual push moves it sideways; bumping into it never does.
            rb.constraints = pushing
                ? RigidbodyConstraints2D.FreezeRotation
                : RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
            Velocity = new Vector2(pushing ? pushDir * PushSpeed : 0f, Velocity.y);

            if (rb.position.y < FallLimit)
            {
                rb.position = home;
                transform.position = home;
                Velocity = Vector2.zero;
            }
        }
    }
}
