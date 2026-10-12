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
        ArtSpriteMarker art;
        string appliedArt;

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
            if (art == null) art = GetComponent<ArtSpriteMarker>();

            bool present = IsPresentIn(era);

            if (sr != null)
            {
                if (art != null && art.HideRenderer)
                {
                    sr.enabled = false;
                }
                else
                {
                    sr.enabled = !ForceHidden;
                    SwapEraArt(era);
                    bool hasArt = art != null || GetComponent<ArtSpriteMarker>() != null;
                    var color = hasArt
                        ? Color.white
                        : Presence == EraPresence.PastOnly ? PastColor
                        : Presence == EraPresence.FutureOnly ? FutureColor
                        : era == Era.Past ? PastColor : FutureColor;
                    if (!present) color.a *= GhostAlpha;
                    else if (ForceIntangible) color.a *= IntangibleAlpha;
                    sr.color = color;
                }
            }

            foreach (var col in colliders) col.enabled = present && !ForceIntangible;
        }

        void SwapEraArt(Era era)
        {
            if (art == null) return;
            string path = era == Era.Past ? art.PastArt : art.FutureArt;
            if (string.IsNullOrEmpty(path)) path = art.PastArt;
            if (string.IsNullOrEmpty(path) || path == appliedArt) return;
            if (path == "Tiles/bridge_deck" || path == "Tiles/bridge_past" || path == "Tiles/bridge_plank")
            {
                if (path == "Tiles/bridge_plank")
                {
                    if (ArtCatalog.TryApply(gameObject, path, art.ArtScale))
                        appliedArt = path;
                }
                else if (ArtCatalog.TryApplyBridge(gameObject, art.ArtScale.x))
                {
                    appliedArt = path;
                }
                var anchor = GetComponent<BridgeAnchor>();
                if (anchor != null) anchor.Align();
                return;
            }

            if (!ArtCatalog.Has(path)) return;
            if (ArtCatalog.TryApply(gameObject, path, art.ArtScale))
            {
                appliedArt = path;
                if (Mathf.Abs(art.ArtScale.x - 1f) < 0.01f && Mathf.Abs(art.ArtScale.y - 1f) < 0.01f)
                    ArtCatalog.FitToCell(gameObject);
            }
        }
    }
}
