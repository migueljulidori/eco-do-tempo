using UnityEngine;

namespace EcoDoTempo
{
    // Opens while its channel is open: a pulled lever or solved clock (for good),
    // or every pressure plate of the channel held down (only while held).
    public class Gate : MonoBehaviour
    {
        [SerializeField] TimeObject timeObject;
        [SerializeField] int channel;
        bool isOpen;

        public int Channel => channel;
        public bool IsOpen => isOpen;

        public static void Create(Transform root, Vector2 pos, int channel = 0)
        {
            string art = ArtCatalog.Has("Props/blockade") ? "Props/blockade"
                       : ArtCatalog.Has("Tiles/gate") ? "Tiles/gate"
                       : null;

            var block = LevelBuilder.Block(root, pos, EraPresence.Both, Palette.PastGate, Palette.FutureGate, art);
            var sr = block.GetComponent<SpriteRenderer>();
            var col = block.GetComponent<BoxCollider2D>();

            if (art == "Props/blockade")
            {
                // Design: 64×64 → 2×2 world units, bottom on the floor cell.
                ArtCatalog.TryApply(block.gameObject, art, new Vector2(2f, 0f));
                ArtCatalog.SetEraArt(block.gameObject, art, art, new Vector2(2f, 0f));
                ArtCatalog.SnapBottomTo(block.gameObject, ArtCatalog.FloorTop(pos), lift: 0.02f);
                if (col != null && sr != null && sr.sprite != null)
                {
                    // Collider is in local space: convert the drawn world size back.
                    var scale = block.transform.lossyScale;
                    col.size = new Vector2(1.6f / scale.x, sr.bounds.size.y * 0.92f / scale.y);
                    col.offset = new Vector2(0f, (sr.bounds.center.y - block.transform.position.y) / scale.y);
                }
            }
            else
            {
                // Without the 2×2 art, stretch the plain block over two cells so nobody slips above it.
                block.transform.position = new Vector3(pos.x, pos.y + 0.5f, 0f);
                block.transform.localScale = new Vector3(block.transform.localScale.x, block.transform.localScale.y * 2f, 1f);
            }

            var gate = block.gameObject.AddComponent<Gate>();
            gate.timeObject = block;
            gate.channel = channel;
            float top = sr != null && sr.sprite != null ? sr.bounds.max.y : pos.y + 1.5f;
            TimeBodies.AddChannelBadge(block.gameObject, new Vector2(pos.x, top + 0.2f), channel);
            if (GameManager.Instance != null) GameManager.Instance.RegisterGate(gate);
        }

        public void SetOpen(bool open)
        {
            if (open == isOpen) return;
            // A closing gate waits until nobody (Lia, echo, crate) stands inside it.
            if (!open && SomethingInside()) return;

            isOpen = open;
            if (timeObject == null) timeObject = GetComponent<TimeObject>();
            timeObject.ForceIntangible = open;
            var gm = GameManager.Instance;
            if (gm == null) return;
            timeObject.Apply(gm.CurrentEra);
            gm.Synth.PlayNote(open ? 196f : 147f, 0.3f, 0.35f);
        }

        bool SomethingInside()
        {
            var col = GetComponent<BoxCollider2D>();
            if (col == null) return false;
            var scale = transform.lossyScale;
            var center = (Vector2)transform.TransformPoint(col.offset);
            // The whole collider: closing never switches it back on inside someone.
            var size = new Vector2(col.size.x * Mathf.Abs(scale.x), col.size.y * Mathf.Abs(scale.y)) * 0.95f;
            return TimeBodies.AnyBodyIn(center, size);
        }
    }
}
