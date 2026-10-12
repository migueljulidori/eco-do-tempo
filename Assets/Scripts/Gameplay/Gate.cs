using UnityEngine;

namespace EcoDoTempo
{
    public class Gate : MonoBehaviour
    {
        TimeObject timeObject;

        public static void Create(Transform root, Vector2 pos)
        {
            var block = LevelBuilder.Block(root, pos, EraPresence.Both, Palette.PastGate, Palette.FutureGate, "Tiles/gate");
            if (ArtCatalog.Has("Tiles/gate"))
            {
                // Bottom-center pivot: move to cell floor and shift collider back up into the cell.
                block.transform.position = ArtCatalog.FloorPos(pos);
                ArtCatalog.TryApply(block.gameObject, "Tiles/gate", Vector2.one);
                var col = block.GetComponent<BoxCollider2D>();
                if (col != null) col.offset = new Vector2(0f, 0.5f);
            }
            var gate = block.gameObject.AddComponent<Gate>();
            gate.timeObject = block;
            GameManager.Instance.RegisterGate(gate);
        }

        public void Open() => timeObject.ForceIntangible = true;
    }
}
