using UnityEngine;

namespace EcoDoTempo
{
    // Scene references to the placed levels, so they stay in the Hierarchy without Play.
    public class LevelCatalog : MonoBehaviour
    {
        public GameObject[] Levels = new GameObject[0];
    }
}
