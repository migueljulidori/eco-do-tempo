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

        static bool running;

        static LevelSceneSetup()
        {
            EditorApplication.delayCall += Ensure;
        }

        [MenuItem("Eco do Tempo/Colocar fases na Hierarchy")]
        public static void Ensure()
        {
            if (running || EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (PrefabStageUtility.GetCurrentPrefabStage() != null) return;

            running = true;
            try
            {
                Directory.CreateDirectory("Assets/Resources/Levels");
                Directory.CreateDirectory("Assets/Scenes");
                BuildMissingPrefabs();
                RepairSavedPrefabs();
                EnsureHierarchy();
                RepairOpenScene();
            }
            finally
            {
                running = false;
            }
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

        // Prefabs saved with a runtime material keep the sprite and lose the material,
        // so the Scene shows only the background. Put the engine sprite material back.
        static void RepairSavedPrefabs()
        {
            var mat = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
            SliceCharacterSheets();
            for (int i = 0; i < LevelData.Levels.Length; i++)
            {
                string path = "Assets/Resources/Levels/Level" + (i + 1) + ".prefab";
                if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null) continue;
                var root = PrefabUtility.LoadPrefabContents(path);
                bool dirty = RepairRenderers(root, mat);
                if (dirty) PrefabUtility.SaveAsPrefabAsset(root, path);
                PrefabUtility.UnloadPrefabContents(root);
            }
            AssetDatabase.SaveAssets();
        }

        static void RepairOpenScene()
        {
            var mat = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
            var renderers = Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include);
            bool dirty = false;
            foreach (var sr in renderers)
                if (RepairRenderer(sr, mat)) dirty = true;
            if (dirty) EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            SceneView.RepaintAll();
        }

        static bool RepairRenderers(GameObject root, Material mat)
        {
            bool dirty = false;
            foreach (var sr in root.GetComponentsInChildren<SpriteRenderer>(true))
                if (RepairRenderer(sr, mat)) dirty = true;
            return dirty;
        }

        static bool RepairRenderer(SpriteRenderer sr, Material mat)
        {
            bool dirty = false;
            if (mat != null && sr.sharedMaterial != mat)
            {
                // A missing material, or a material that only exists in memory.
                bool persistent = sr.sharedMaterial != null && EditorUtility.IsPersistent(sr.sharedMaterial);
                if (!persistent)
                {
                    sr.sharedMaterial = mat;
                    dirty = true;
                }
            }

            var so = new SerializedObject(sr);
            var cull = so.FindProperty("m_SmallMeshCulling");
            if (cull != null && cull.boolValue)
            {
                cull.boolValue = false;
                so.ApplyModifiedPropertiesWithoutUndo();
                dirty = true;
            }

            // Era ghosts are stored at 0.18 alpha. In edit mode that reads as "nothing there".
            if (sr.gameObject.name != "Glow"
                && (Mathf.Abs(sr.color.a - 0.18f) < 0.01f || Mathf.Abs(sr.color.a - 0.25f) < 0.01f))
            {
                var color = sr.color;
                color.a = 1f;
                sr.color = color;
                dirty = true;
            }

            if (sr.sprite == null)
            {
                var marker = sr.GetComponent<ArtSpriteMarker>();
                string resource = marker != null ? marker.PastArt : null;
                if (string.IsNullOrEmpty(resource) && sr.gameObject.name == "Body")
                    resource = "Characters/lia_idle";
                var sprite = LoadFirstSprite(resource);
                if (sprite != null)
                {
                    sr.sprite = sprite;
                    if (sr.color.a <= 0.001f) sr.color = Color.white;
                    dirty = true;
                }
            }
            return dirty;
        }

        static Sprite LoadFirstSprite(string resourcePath)
        {
            if (string.IsNullOrEmpty(resourcePath)) return null;
            string assetPath = "Assets/Resources/" + resourcePath + ".png";
            var assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            Sprite fallback = null;
            foreach (var asset in assets)
            {
                if (asset is not Sprite sprite) continue;
                if (sprite.name.EndsWith("_0")) return sprite;
                if (fallback == null) fallback = sprite;
            }
            return fallback;
        }

        static void SliceCharacterSheets()
        {
            SliceSheet("Assets/Resources/Characters/lia_idle.png", 32);
            SliceSheet("Assets/Resources/Characters/lia_jump.png", 32);
        }

#pragma warning disable CS0618
        static void SliceSheet(string path, int frame)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (importer == null || tex == null || frame <= 0) return;
            int count = Mathf.Max(1, tex.width / frame);
            if (importer.spriteImportMode == SpriteImportMode.Multiple
                && importer.spritesheet != null
                && importer.spritesheet.Length == count)
                return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsToUnits = 32;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            var sheet = new SpriteMetaData[count];
            string baseName = Path.GetFileNameWithoutExtension(path);
            for (int i = 0; i < count; i++)
            {
                sheet[i] = new SpriteMetaData
                {
                    name = baseName + "_" + i,
                    rect = new Rect(i * frame, 0, frame, tex.height),
                    alignment = (int)SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f),
                };
            }
            importer.spritesheet = sheet;
            importer.SaveAndReimport();
        }
#pragma warning restore CS0618

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
