using UnityEngine;

namespace EcoDoTempo
{
    public class Gate : MonoBehaviour
    {
        TimeObject timeObject;

        public static void Create(Transform root, Vector2 pos)
        {
            string art = ArtCatalog.Has("Props/blockade") ? "Props/blockade"
                       : ArtCatalog.Has("Tiles/gate") ? "Tiles/gate"
                       : null;

            var block = LevelBuilder.Block(root, pos, EraPresence.Both, Palette.PastGate, Palette.FutureGate, art);

            if (art == "Props/blockade")
            {
                // Design: 64×64 → 2×2 world units, bottom on the floor cell.
                ArtCatalog.TryApply(block.gameObject, art, new Vector2(2f, 0f));
                ArtCatalog.SetEraArt(block.gameObject, art, art, new Vector2(2f, 0f));
                ArtCatalog.SnapBottomTo(block.gameObject, ArtCatalog.FloorTop(pos), lift: 0.02f);
                var col = block.GetComponent<BoxCollider2D>();
                var sr = block.GetComponent<SpriteRenderer>();
                if (col != null && sr != null && sr.sprite != null)
                {
                    col.size = new Vector2(1.6f, sr.bounds.size.y * 0.92f);
                    col.offset = new Vector2(0f, sr.bounds.center.y - block.transform.position.y);
                }
            }

            var gate = block.gameObject.AddComponent<Gate>();
            gate.timeObject = block;
            GameManager.Instance.RegisterGate(gate);
        }

        public void Open() => timeObject.ForceIntangible = true;
    }
}
