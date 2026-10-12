using UnityEngine;

namespace EcoDoTempo
{
    // Keeps bridge art + support colliders glued to a stone ledge top.
    public class BridgeAnchor : MonoBehaviour
    {
        public float LedgeTop;
        public Transform[] Supports;
        public const string Version = "bridge-v4-deck";

        void Start()
        {
            Align();
            Debug.Log($"[Eco do Tempo] {Version} alinhada em y={LedgeTop:0.00}");
        }

        public void Align()
        {
            ArtCatalog.TryApplyBridge(gameObject, ArtScaleWidth());
            ArtCatalog.AlignBridgeToLedge(gameObject, SupportsToArray(), LedgeTop);
        }

        float ArtScaleWidth()
        {
            var marker = GetComponent<ArtSpriteMarker>();
            return marker != null ? Mathf.Max(1f, marker.ArtScale.x) : 1f;
        }

        GameObject[] SupportsToArray()
        {
            if (Supports == null || Supports.Length == 0) return null;
            var arr = new GameObject[Supports.Length];
            for (int i = 0; i < Supports.Length; i++)
                arr[i] = Supports[i] != null ? Supports[i].gameObject : null;
            return arr;
        }
    }
}
