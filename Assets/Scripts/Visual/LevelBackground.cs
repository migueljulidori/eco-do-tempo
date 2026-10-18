using UnityEngine;

namespace EcoDoTempo
{
    // The map is only tiles. Painted plates stay hidden; empty space uses the camera color.
    public class LevelBackground : MonoBehaviour
    {
        SpriteRenderer sr;

        public static LevelBackground Create(Transform root, string backgroundKey, Vector2Int size, float floorTopY)
        {
            return null;
        }

        // A backdrop already saved in a prefab must not draw over the tiles.
        public void UsePlacedSprite()
        {
            HideRenderer();
        }

        public void Apply(Era era)
        {
            HideRenderer();
        }

        void HideRenderer()
        {
            if (sr == null) sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;
        }
    }
}
