using UnityEngine;

namespace EcoDoTempo
{
    // Forge coals: a past-only hazard that breathes up and down. Cold ash in the future.
    public class FireFlicker : MonoBehaviour
    {
        Vector3 baseScale;
        Vector3 basePosition;
        float seed;

        public static void Create(Transform root, Vector2 pos)
        {
            float floor = ArtCatalog.FloorTop(pos);
            var go = ArtCatalog.Create("Brasa", root, new Vector2(pos.x, floor + 0.3f), new Vector2(0.9f, 0.6f),
                SpriteFactory.Triangle, Palette.Fire, 4, "Props/fire", Vector2.one);
            if (ArtCatalog.Has("Props/fire")) ArtCatalog.SnapBottomTo(go, floor);
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            // Burns only where the coals are drawn: a low band on the pit floor.
            var scale = go.transform.lossyScale;
            float sx = Mathf.Max(0.01f, Mathf.Abs(scale.x)), sy = Mathf.Max(0.01f, Mathf.Abs(scale.y));
            col.size = new Vector2(0.9f / sx, 0.55f / sy);
            col.offset = new Vector2(0f, (floor + 0.275f - go.transform.position.y) / sy);
            go.AddComponent<Hazard>();
            go.AddComponent<FireFlicker>();
            SoftGlow.Attach(go, new Color(1f, 0.45f, 0.1f, 0.6f), 1.6f, 3);
            TimeObject.Add(go, EraPresence.PastOnly, Palette.Fire, Palette.Fire, solid: false);
        }

        void Start()
        {
            baseScale = transform.localScale;
            basePosition = transform.position;
            seed = transform.position.x * 1.7f;
        }

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm != null && gm.TimeFrozen) return;
            float k = 1f + Mathf.Sin(Time.time * 9f + seed) * 0.12f + Mathf.Sin(Time.time * 23f + seed) * 0.05f;
            transform.localScale = new Vector3(baseScale.x, baseScale.y * k, baseScale.z);
            transform.position = basePosition + Vector3.up * (baseScale.y * (k - 1f) * 0.3f);
        }
    }
}
