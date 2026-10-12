using UnityEngine;

namespace EcoDoTempo
{
    // Anything whose existence depends on the current era.
    public class TimeObject : MonoBehaviour
    {
        const float GhostAlpha = 0.18f;
        const float IntangibleAlpha = 0.25f;

        public EraPresence Presence = EraPresence.Both;
        public Color PastColor = Color.white;
        public Color FutureColor = Color.white;
        public bool IsSolid;
        // Hidden objects ignore eras entirely (e.g. a tree that was never planted).
        public bool ForceHidden;
        // Visible but passable (e.g. an opened gate).
        public bool ForceIntangible;

        SpriteRenderer sr;
        Collider2D[] colliders;

        public static TimeObject Add(GameObject go, EraPresence presence, Color past, Color future, bool solid)
        {
            var obj = go.AddComponent<TimeObject>();
            obj.Presence = presence;
            obj.PastColor = past;
            obj.FutureColor = future;
            obj.IsSolid = solid;
            GameManager.Instance.Register(obj);
            return obj;
        }

        public bool IsPresentIn(Era era) => !ForceHidden && Presence.Includes(era);

        public bool BlocksIn(Era era) => IsSolid && !ForceIntangible && IsPresentIn(era);

        public Rect WorldRect
        {
            get
            {
                Vector2 size = transform.lossyScale;
                return new Rect((Vector2)transform.position - size * 0.5f, size);
            }
        }

        public void Apply(Era era)
        {
            if (sr == null) sr = GetComponent<SpriteRenderer>();
            if (colliders == null) colliders = GetComponents<Collider2D>();

            bool present = IsPresentIn(era);

            if (sr != null)
            {
                sr.enabled = !ForceHidden;
                var color = Presence == EraPresence.PastOnly ? PastColor
                          : Presence == EraPresence.FutureOnly ? FutureColor
                          : era == Era.Past ? PastColor : FutureColor;
                if (!present) color.a *= GhostAlpha;
                else if (ForceIntangible) color.a *= IntangibleAlpha;
                sr.color = color;
            }

            foreach (var col in colliders) col.enabled = present && !ForceIntangible;
        }
    }
}
