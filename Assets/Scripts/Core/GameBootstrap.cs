using UnityEngine;

namespace EcoDoTempo
{
    // Starts the game in any scene: just press Play.
    static class GameBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init()
        {
            if (GameManager.Instance != null) return;
            new GameObject("EcoDoTempo").AddComponent<GameManager>();
        }
    }
}
