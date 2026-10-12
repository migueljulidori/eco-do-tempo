using UnityEngine;

namespace EcoDoTempo
{
    // Drawn pixel art: optional era swap + permanent hide (e.g. invisible bridge supports).
    public class ArtSpriteMarker : MonoBehaviour
    {
        public string PastArt;
        public string FutureArt;
        public Vector2 ArtScale = Vector2.one;
        public bool HideRenderer;
    }
}
