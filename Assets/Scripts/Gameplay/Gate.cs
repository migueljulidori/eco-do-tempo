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

            // Blockade art is 64×64 (2×2 units); sit on the cell floor and cover ~2 units.
            Vector2 scale = art == "Props/blockade" ? new Vector2(1.4f, 0f) : Vector2.one;
            var block = LevelBuilder.Block(root, pos, EraPresence.Both, Palette.PastGate, Palette.FutureGate, art);
            if (art == "Props/blockade")
            {
                ArtCatalog.TryApply(block.gameObject, art, scale);
                ArtCatalog.SnapBottomTo(block.gameObject, ArtCatalog.FloorTop(pos));
                var col = block.GetComponent<BoxCollider2D>();
                if (col != null)
                {
                    col.size = new Vector2(1.2f, 1.8f);
                    col.offset = new Vector2(0f, 0.9f);
                }
            }

            var gate = block.gameObject.AddComponent<Gate>();
            gate.timeObject = block;
            GameManager.Instance.RegisterGate(gate);
        }

        public void Open() => timeObject.ForceIntangible = true;
    }
}
