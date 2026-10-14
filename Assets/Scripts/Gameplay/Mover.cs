using System.Collections.Generic;
using UnityEngine;

namespace EcoDoTempo
{
    public enum MoverKind
    {
        PastPlatform,  // m: workshop machinery, runs only in the past and rusts where it stopped
        Platform,      // M: the clock's heart, runs in every era; only Fermata stops it
        Blade,         // v: pendulum blade, runs in every era; deadly
    }

    // Ping-pongs along a track drawn in the map with '=' (horizontal) or '|' (vertical).
    public class Mover : MonoBehaviour
    {
        const float PlatformSpeed = 2f;
        const float BladeSpeed = 3f;
        const float EndPause = 0.5f;

        [SerializeField] MoverKind kind;
        [SerializeField] Vector2 start;
        [SerializeField] Vector2 end;
        [SerializeField] Vector2 size = Vector2.one;

        Rigidbody2D rb;
        Transform spinner;
        float travel;
        int direction = 1;
        float pause;
        readonly List<Collider2D> hits = new List<Collider2D>();

        public Vector2 Velocity { get; private set; }

        public bool Running
        {
            get
            {
                var gm = GameManager.Instance;
                if (gm == null || gm.State != GameState.Playing || gm.TimeFrozen) return false;
                return kind != MoverKind.PastPlatform || gm.CurrentEra == Era.Past;
            }
        }

        public static Mover Create(Transform root, Vector2 startCenter, int cells, Vector2 offset, MoverKind kind)
        {
            bool blade = kind == MoverKind.Blade;
            var go = new GameObject(blade ? "Lamina" : kind == MoverKind.PastPlatform ? "Elevador" : "Engrenagem");
            go.transform.SetParent(root, false);
            go.transform.position = startCenter;

            var mover = go.AddComponent<Mover>();
            mover.kind = kind;
            mover.start = startCenter;
            mover.end = startCenter + offset;
            mover.size = blade ? Vector2.one * 0.8f : new Vector2(cells, 1f);

            var body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            mover.rb = body;

            DrawTrack(root, startCenter, cells, offset);

            if (blade)
            {
                var saw = ArtCatalog.Create("Serra", go.transform, startCenter, Vector2.one * 0.8f,
                    SpriteFactory.Diamond, Palette.Blade, 6, "Props/blade", Vector2.one * 0.9f);
                mover.spinner = saw.transform;
                var hub = SpriteFactory.CreateObject("Eixo", saw.transform, startCenter, Vector2.one * 0.3f,
                    SpriteFactory.Circle, new Color(0.6f, 0.2f, 0.2f), 7);
                hub.transform.localScale = Vector3.one * 0.35f;
                var col = go.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                col.radius = 0.36f;
                go.AddComponent<Hazard>();
                SoftGlow.Attach(saw, new Color(1f, 0.35f, 0.3f, 0.45f), 1.4f, 5);
                return mover;
            }

            var box = go.AddComponent<BoxCollider2D>();
            box.size = mover.size;
            string pastArt = ArtCatalog.FirstExisting("Tiles/platform_past", "Tiles/block_metal_past", "Tiles/block_past");
            string futureArt = ArtCatalog.FirstExisting("Tiles/platform_future", "Tiles/block_metal_future", "Tiles/block_future");
            for (int i = 0; i < cells; i++)
            {
                var cellPos = startCenter + Vector2.right * (i - (cells - 1) * 0.5f);
                var tile = SpriteFactory.CreateObject("Peca", go.transform, cellPos, Vector2.one, SpriteFactory.Square,
                    Palette.PastBlock, 1);
                if (ArtCatalog.TryApply(tile, pastArt, Vector2.one))
                    ArtCatalog.SetEraArt(tile, pastArt, futureArt, Vector2.one);
                tile.transform.position = cellPos;
                TimeObject.Add(tile, EraPresence.Both, Palette.PastBlock,
                    kind == MoverKind.PastPlatform ? Palette.FutureWall : Palette.FutureBlock, solid: false);
            }
            // Gear teeth on the edges so moving pieces read differently from the walls.
            foreach (float side in new[] { -1f, 1f })
            {
                var cog = SpriteFactory.CreateObject("Dente", go.transform,
                    startCenter + new Vector2(side * cells * 0.5f, 0f), Vector2.one * 0.35f,
                    SpriteFactory.Circle, Palette.Channel(0), 2);
                cog.transform.position = startCenter + new Vector2(side * cells * 0.5f, 0f);
            }
            return mover;
        }

        static void DrawTrack(Transform root, Vector2 startCenter, int cells, Vector2 offset)
        {
            int steps = Mathf.RoundToInt(offset.magnitude);
            if (steps == 0) return;
            var dir = offset / steps;
            for (int s = 0; s <= steps; s++)
            {
                for (int c = 0; c < cells; c++)
                {
                    var p = startCenter + dir * s + Vector2.right * (c - (cells - 1) * 0.5f);
                    SpriteFactory.CreateObject("Trilho", root, p, Vector2.one * 0.12f, SpriteFactory.Circle,
                        new Color(1f, 1f, 1f, 0.22f), -5);
                }
            }
        }

        void Awake()
        {
            if (rb == null) rb = GetComponent<Rigidbody2D>();
            if (spinner == null && kind == MoverKind.Blade && transform.childCount > 0) spinner = transform.GetChild(0);
        }

        void Update()
        {
            if (spinner != null && Running) spinner.Rotate(0f, 0f, -540f * Time.deltaTime);
        }

        void FixedUpdate()
        {
            float length = (end - start).magnitude;
            if (length < 0.01f || !Running)
            {
                Velocity = Vector2.zero;
                return;
            }

            float dt = Time.fixedDeltaTime;
            if (pause > 0f)
            {
                pause -= dt;
                Velocity = Vector2.zero;
                return;
            }

            float speed = kind == MoverKind.Blade ? BladeSpeed : PlatformSpeed;
            float next = travel + direction * speed * dt;
            int nextDirection = direction;
            bool turn = false;
            if (next >= length) { next = length; nextDirection = -1; turn = true; }
            else if (next <= 0f) { next = 0f; nextDirection = 1; turn = true; }

            var target = Vector2.Lerp(start, end, next / length);
            if (kind != MoverKind.Blade && Blocked(target))
            {
                // Never crush: back off the way it came.
                direction = -direction;
                Velocity = Vector2.zero;
                return;
            }

            Velocity = (target - rb.position) / dt;
            rb.MovePosition(target);
            travel = next;
            direction = nextDirection;
            if (turn) pause = EndPause;
        }

        bool Blocked(Vector2 target)
        {
            float top = target.y + size.y * 0.5f;
            int count = Physics2D.OverlapBox(target, size * 0.96f, 0f, TimeBodies.AllContacts, hits);
            for (int i = 0; i < count; i++)
            {
                var col = hits[i];
                if (col.isTrigger || col.attachedRigidbody == rb) continue;
                if (!TimeBodies.IsBody(col)) continue;
                // Riders sit on top and get lifted, not crushed.
                if (col.bounds.min.y >= top - 0.2f) continue;
                return true;
            }
            return false;
        }
    }
}
