using UnityEngine;

namespace EcoDoTempo
{
    public class Gate : MonoBehaviour
    {
        TimeObject timeObject;

        public static void Create(Transform root, Vector2 pos)
        {
            var block = LevelBuilder.Block(root, pos, EraPresence.Both, Palette.PastGate, Palette.FutureGate, "Tiles/gate");
            var gate = block.gameObject.AddComponent<Gate>();
            gate.timeObject = block;
            GameManager.Instance.RegisterGate(gate);
        }

        public void Open() => timeObject.ForceIntangible = true;
    }
}
