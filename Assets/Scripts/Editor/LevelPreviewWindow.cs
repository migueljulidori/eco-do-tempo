#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EcoDoTempo
{
    public sealed class LevelPreviewWindow : EditorWindow
    {
        int levelIndex;
        Era era = Era.Past;
        bool planted;

        [MenuItem("Eco do Tempo/Visualizar fase na Scene")]
        static void Open() => GetWindow<LevelPreviewWindow>("Prévia da fase");

        void OnGUI()
        {
            var levels = LevelData.Levels;
            var names = new string[levels.Length];
            for (int i = 0; i < levels.Length; i++) names[i] = (i + 1) + " — " + levels[i].Name;
            levelIndex = Mathf.Clamp(levelIndex, 0, levels.Length - 1);
            levelIndex = EditorGUILayout.Popup("Fase", levelIndex, names);
            era = (Era)EditorGUILayout.EnumPopup("Era", era);
            planted = EditorGUILayout.Toggle("Mostrar árvores plantadas", planted);
            EditorGUILayout.HelpBox("Prévia temporária para visualizar e selecionar objetos. Alterações nela não mudam o mapa em LevelData. Ela é removida antes do Play e não é salva na cena.", MessageType.Info);
            using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
            {
                if (GUILayout.Button("Gerar / atualizar prévia")) Generate();
                if (GUILayout.Button("Remover prévia")) PreviewLifecycle.Clear();
            }
        }

        void Generate()
        {
            PreviewLifecycle.Clear();
            GameObject root = null;
            try
            {
                root = new GameObject("PRÉVIA — " + LevelData.Levels[levelIndex].Name);
                root.AddComponent<EditorPreviewMarker>();
                var level = LevelData.Levels[levelIndex];
                var result = LevelBuilder.Build(level, root.transform);
                var background = LevelBackground.Create(root.transform, level.Background, result.Size, result.FloorTopY);
                background?.Apply(era);
                PlayerController.Create(result.Spawn, root.transform);
                foreach (var obj in root.GetComponentsInChildren<TimeObject>(true))
                {
                    if (planted && obj.ForceHidden) obj.ForceHidden = false;
                    obj.Apply(era);
                }
                // Freeze a visual snapshot: do not retain gameplay scripts or physics.
                foreach (var behaviour in root.GetComponentsInChildren<MonoBehaviour>(true))
                    if (!(behaviour is EditorPreviewMarker)) DestroyImmediate(behaviour);
                foreach (var body in root.GetComponentsInChildren<Rigidbody2D>(true)) body.simulated = false;
                foreach (var collider in root.GetComponentsInChildren<Collider2D>(true)) collider.enabled = false;
                foreach (var t in root.GetComponentsInChildren<Transform>(true))
                    t.gameObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                Selection.activeGameObject = root;
                var view = SceneView.lastActiveSceneView;
                if (view == null) view = GetWindow<SceneView>();
                view.in2DMode = true;
                view.Frame(new Bounds(new Vector3((result.Size.x - 1) * 0.5f, (result.Size.y - 1) * 0.5f, 0f),
                    new Vector3(result.Size.x + 2f, result.Size.y + 2f, 1f)), false);
                SceneView.RepaintAll();
                EditorApplication.RepaintHierarchyWindow();
            }
            catch (Exception ex)
            {
                if (root != null) DestroyImmediate(root);
                Debug.LogException(ex);
                EditorUtility.DisplayDialog("Prévia não gerada", "Confira o primeiro erro na Console.", "OK");
            }
        }
    }

    [InitializeOnLoad]
    static class PreviewLifecycle
    {
        static PreviewLifecycle()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.ExitingEditMode) Clear();
            };
            AssemblyReloadEvents.beforeAssemblyReload += Clear;
        }

        public static void Clear()
        {
            // Only remove marked previews in loaded scenes; never assets or real levels.
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;
                foreach (var root in scene.GetRootGameObjects())
                    if (root.GetComponent<EditorPreviewMarker>() != null) UnityEngine.Object.DestroyImmediate(root);
            }
        }
    }
}
#endif
