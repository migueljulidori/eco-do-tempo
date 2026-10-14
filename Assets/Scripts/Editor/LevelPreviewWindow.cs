#if UNITY_EDITOR
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
            EditorGUILayout.HelpBox(
                "Mostra a fase escolhida em \"" + LevelSceneSetup.RootName + "\" na Hierarchy. " +
                "É só uma prévia: não é salva e some antes do Play. Para mudar o mapa, edite LevelData.cs.",
                MessageType.Info);
            using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
            {
                if (GUILayout.Button("Gerar / atualizar prévia")) LevelSceneSetup.Rebuild(levelIndex, era, planted, frame: true);
                if (GUILayout.Button("Remover prévia")) PreviewLifecycle.Clear();
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
            // Only remove marked previews in loaded scenes; never assets or real objects.
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;
                foreach (var root in scene.GetRootGameObjects())
                    if (root.GetComponent<EditorPreviewMarker>() != null) Object.DestroyImmediate(root);
            }
        }
    }
}
#endif
