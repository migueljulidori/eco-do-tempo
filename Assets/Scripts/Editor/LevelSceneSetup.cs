#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EcoDoTempo
{
    // Shows every phase in the Hierarchy without Play, built straight from LevelData.
    // It is a snapshot: never saved, rebuilt after every recompile and after leaving Play.
    // To change a phase for real, edit its map in LevelData.cs.
    [InitializeOnLoad]
    public static class LevelSceneSetup
    {
        public const string RootName = "Níveis (gerados do LevelData)";

        static int shownLevel;
        static Era shownEra = Era.Past;
        static bool shownPlanted;

        static LevelSceneSetup()
        {
            EditorApplication.delayCall += Ensure;
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredEditMode) EditorApplication.delayCall += Ensure;
            };
        }

        [MenuItem("Eco do Tempo/Recriar fases na Hierarchy")]
        static void RebuildFromMenu() => Rebuild(shownLevel, shownEra, shownPlanted, frame: true);

        public static void Ensure()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (PrefabStageUtility.GetCurrentPrefabStage() != null) return;
            RemoveLegacyLevels();
            if (FindRoot() != null) return;
            Rebuild(shownLevel, shownEra, shownPlanted, frame: false);
        }

        public static void Rebuild(int levelIndex, Era era, bool planted, bool frame)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            shownLevel = Mathf.Clamp(levelIndex, 0, LevelData.Levels.Length - 1);
            shownEra = era;
            shownPlanted = planted;

            PreviewLifecycle.Clear();
            var root = new GameObject(RootName);
            root.AddComponent<EditorPreviewMarker>();
            Vector2Int framed = new Vector2Int(30, 10);
            for (int i = 0; i < LevelData.Levels.Length; i++)
            {
                var level = LevelData.Levels[i];
                var holder = new GameObject($"Fase {i + 1} — {level.Name}");
                holder.transform.SetParent(root.transform, false);
                try
                {
                    var result = LevelBuilder.Build(level, holder.transform);
                    var background = LevelBackground.Create(holder.transform, level.Background, result.Size, result.FloorTopY);
                    background?.Apply(era);
                    PlayerController.Create(result.Spawn, holder.transform);
                    foreach (var obj in holder.GetComponentsInChildren<TimeObject>(true))
                    {
                        if (planted && obj.ForceHidden) obj.ForceHidden = false;
                        obj.Apply(era);
                    }
                    if (i == shownLevel) framed = result.Size;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[Eco do Tempo] A fase {i + 1} não foi montada. Confira o mapa em LevelData.cs.");
                    Debug.LogException(ex);
                }
                holder.SetActive(i == shownLevel);
            }

            Freeze(root);
            EditorApplication.RepaintHierarchyWindow();
            if (!frame) return;

            Selection.activeGameObject = root.transform.GetChild(shownLevel).gameObject;
            var view = SceneView.lastActiveSceneView;
            if (view == null) return;
            view.in2DMode = true;
            view.Frame(new Bounds(new Vector3((framed.x - 1) * 0.5f, (framed.y - 1) * 0.5f, 0f),
                new Vector3(framed.x + 2f, framed.y + 2f, 1f)), false);
            SceneView.RepaintAll();
        }

        // Visual snapshot only: no gameplay scripts, no physics, nothing saved.
        static void Freeze(GameObject root)
        {
            foreach (var behaviour in root.GetComponentsInChildren<MonoBehaviour>(true))
                if (!(behaviour is EditorPreviewMarker)) Object.DestroyImmediate(behaviour);
            foreach (var body in root.GetComponentsInChildren<Rigidbody2D>(true)) body.simulated = false;
            foreach (var collider in root.GetComponentsInChildren<Collider2D>(true)) collider.enabled = false;
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
                t.gameObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
        }

        static GameObject FindRoot()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;
                foreach (var go in scene.GetRootGameObjects())
                    if (go.name == RootName && go.GetComponent<EditorPreviewMarker>() != null) return go;
            }
            return null;
        }

        // Earlier versions saved prefab copies of the levels in the scene ("Níveis").
        // The game no longer reads them, so they would only show an outdated map.
        static void RemoveLegacyLevels()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;
                bool removed = false;
                foreach (var go in scene.GetRootGameObjects())
                {
                    if (go.GetComponent<LevelCatalog>() == null) continue;
                    Object.DestroyImmediate(go);
                    removed = true;
                }
                if (!removed) continue;
                EditorSceneManager.MarkSceneDirty(scene);
                Debug.Log("[Eco do Tempo] Removi o objeto antigo \"Níveis\" da cena (as fases agora vêm do LevelData). Salve a cena com Ctrl+S.");
            }
        }
    }
}
#endif
