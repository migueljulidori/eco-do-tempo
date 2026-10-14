#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EcoDoTempo
{
    // Puts every phase in the Hierarchy as a real object, without entering Play.
    [InitializeOnLoad]
    public static class LevelSceneSetup
    {
        const string ScenePath = "Assets/Scenes/EcoDoTempo.unity";

        static LevelSceneSetup()
        {
            EditorApplication.delayCall += Ensure;
        }

        [MenuItem("Eco do Tempo/Colocar fases na Hierarchy")]
        public static void Ensure()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (PrefabStageUtility.GetCurrentPrefabStage() != null) return;

            Directory.CreateDirectory("Assets/Resources/Levels");
            Directory.CreateDirectory("Assets/Scenes");
            BuildMissingPrefabs();
            EnsureHierarchy();
        }

        static void BuildMissingPrefabs()
        {
            for (int i = 0; i < LevelData.Levels.Length; i++)
            {
                string path = "Assets/Resources/Levels/Level" + (i + 1) + ".prefab";
                if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) continue;

                var level = LevelData.Levels[i];
                var root = new GameObject("Fase " + (i + 1));
                try
                {
                    var result = LevelBuilder.Build(level, root.transform);
                    LevelBackground.Create(root.transform, level.Background, result.Size, result.FloorTopY);
                    PlayerController.Create(result.Spawn, root.transform);
                    foreach (var obj in root.GetComponentsInChildren<TimeObject>(true))
                        obj.Apply(Era.Past);
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally
                {
                    Object.DestroyImmediate(root);
                }
            }
            AssetDatabase.SaveAssets();
        }

        static void EnsureHierarchy()
        {
            var scene = SceneManager.GetActiveScene();
            var catalog = Object.FindAnyObjectByType<LevelCatalog>();
            if (catalog != null && catalog.Levels != null && catalog.Levels.Length == LevelData.Levels.Length)
            {
                bool complete = true;
                foreach (var level in catalog.Levels)
                    if (level == null) complete = false;
                if (complete) return;
            }

            var parent = new GameObject("Níveis");
            catalog = parent.AddComponent<LevelCatalog>();
            catalog.Levels = new GameObject[LevelData.Levels.Length];
            for (int i = 0; i < catalog.Levels.Length; i++)
            {
                string path = "Assets/Resources/Levels/Level" + (i + 1) + ".prefab";
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                instance.name = "Fase " + (i + 1);
                instance.transform.SetParent(parent.transform, false);
                instance.SetActive(i == 0);
                catalog.Levels[i] = instance;
            }

            if (Object.FindAnyObjectByType<GameManager>() == null)
                new GameObject("EcoDoTempo").AddComponent<GameManager>();

            var view = SceneView.lastActiveSceneView;
            if (view != null)
            {
                view.in2DMode = true;
                view.Frame(new Bounds(new Vector3(20f, 6f, 0f), new Vector3(40f, 16f, 1f)), false);
            }

            if (string.IsNullOrEmpty(scene.path))
                EditorSceneManager.SaveScene(scene, ScenePath);
            else
                EditorSceneManager.MarkSceneDirty(scene);

            var listed = EditorBuildSettings.scenes;
            bool known = false;
            string saved = string.IsNullOrEmpty(scene.path) ? ScenePath : scene.path;
            for (int i = 0; i < listed.Length; i++)
                if (listed[i].path == saved) known = true;
            if (!known)
            {
                var next = new EditorBuildSettingsScene[listed.Length + 1];
                listed.CopyTo(next, 0);
                next[listed.Length] = new EditorBuildSettingsScene(saved, true);
                EditorBuildSettings.scenes = next;
            }
        }
    }
}
#endif
