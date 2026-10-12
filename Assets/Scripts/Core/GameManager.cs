using System;
using System.Collections.Generic;
using UnityEngine;

namespace EcoDoTempo
{
    public enum GameState { Title, Story, Playing, Inspecting, LevelComplete, Ending }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState State { get; private set; }
        public Era CurrentEra { get; private set; }
        public int LevelIndex { get; private set; }
        public LevelDefinition Level => LevelData.Levels[LevelIndex];
        public int FragmentsCollected { get; private set; }
        public int FragmentsTotal { get; private set; }
        public bool AllFragmentsCollected => FragmentsCollected >= FragmentsTotal;
        public PlayerController Player { get; private set; }
        public ToneSynth Synth { get; private set; }

        public readonly HashSet<int> CollectedNotes = new HashSet<int>();

        public string[] StoryPages { get; private set; }
        public int StoryPage { get; private set; }
        public string InspectText { get; private set; }
        public string Message { get; private set; }
        public float MessageAlpha => Mathf.Clamp01((messageUntil - Time.time) / 0.5f);
        public float AutoSwitchProgress => Level.AutoSwitchInterval > 0f ? autoSwitchTimer / Level.AutoSwitchInterval : 0f;

        readonly List<TimeObject> timeObjects = new List<TimeObject>();
        readonly List<Gate> gates = new List<Gate>();
        readonly HashSet<int> notesAtLevelStart = new HashSet<int>();
        Action onStoryDone;
        GameObject levelRoot;
        LevelBackground levelBackground;
        Camera cam;
        CameraFollow camFollow;
        Vector2Int levelSize;
        float messageUntil;
        float autoSwitchTimer;
        int stateChangedFrame;

        void Awake()
        {
            Instance = this;
            Synth = gameObject.AddComponent<ToneSynth>();
            gameObject.AddComponent<GameUI>();
            SetupCamera();
            SetState(GameState.Title);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void SetupCamera()
        {
            cam = Camera.main;
            if (cam == null)
            {
                var go = new GameObject("Main Camera") { tag = "MainCamera" };
                cam = go.AddComponent<Camera>();
            }
            cam.orthographic = true;
            cam.orthographicSize = 6f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Palette.Background(Era.Past);
            camFollow = cam.GetComponent<CameraFollow>();
            if (camFollow == null) camFollow = cam.gameObject.AddComponent<CameraFollow>();
        }

        void SetState(GameState state)
        {
            State = state;
            stateChangedFrame = Time.frameCount;
        }

        bool InputReady => Time.frameCount > stateChangedFrame;

        void Update()
        {
            if (!InputReady) return;

            switch (State)
            {
                case GameState.Title:
                    if (GameInput.ConfirmPressed)
                    {
                        CollectedNotes.Clear();
                        ShowStory(LevelData.Intro, () => LoadLevel(0));
                    }
                    break;

                case GameState.Story:
                    if (GameInput.ConfirmPressed)
                    {
                        StoryPage++;
                        stateChangedFrame = Time.frameCount;
                        if (StoryPage >= StoryPages.Length) onStoryDone?.Invoke();
                    }
                    break;

                case GameState.Playing:
                    UpdatePlaying();
                    break;

                case GameState.Inspecting:
                    if (GameInput.ConfirmPressed || GameInput.InteractPressed) SetState(GameState.Playing);
                    break;

                case GameState.LevelComplete:
                    if (GameInput.ConfirmPressed) NextLevel();
                    break;

                case GameState.Ending:
                    if (GameInput.ConfirmPressed)
                    {
                        ClearLevel();
                        cam.backgroundColor = Palette.Background(Era.Past);
                        SetState(GameState.Title);
                    }
                    break;
            }
        }

        void UpdatePlaying()
        {
            if (GameInput.RestartPressed)
            {
                LoadLevel(LevelIndex, showIntro: false);
                return;
            }

            if (Level.AutoSwitchInterval > 0f)
            {
                autoSwitchTimer += Time.deltaTime;
                if (autoSwitchTimer >= Level.AutoSwitchInterval)
                {
                    autoSwitchTimer = 0f;
                    TrySwitchEra(silentWhenBlocked: true);
                }
                if (GameInput.TimeSwitchPressed) ShowMessage("O diapasão não responde. O tempo oscila sozinho aqui.");
            }
            else if (GameInput.TimeSwitchPressed)
            {
                TrySwitchEra(silentWhenBlocked: false);
            }
        }

        // ---------- Story ----------

        public void ShowStory(string[] pages, Action onDone)
        {
            StoryPages = pages;
            StoryPage = 0;
            onStoryDone = onDone;
            SetState(GameState.Story);
        }

        // ---------- Levels ----------

        void LoadLevel(int index, bool showIntro = true)
        {
            ClearLevel();
            LevelIndex = index;

            // Restarting a level forgets the notes gathered during that attempt.
            if (showIntro)
            {
                notesAtLevelStart.Clear();
                notesAtLevelStart.UnionWith(CollectedNotes);
            }
            else
            {
                CollectedNotes.Clear();
                CollectedNotes.UnionWith(notesAtLevelStart);
            }

            levelRoot = new GameObject("Level " + (index + 1));
            var result = LevelBuilder.Build(Level, levelRoot.transform);
            levelSize = result.Size;
            FragmentsTotal = result.FragmentCount;
            FragmentsCollected = 0;

            levelBackground = LevelBackground.Create(levelRoot.transform, Level.Background, levelSize);
            Player = PlayerController.Create(result.Spawn, levelRoot.transform);
            camFollow.SetTarget(Player.transform, levelSize);

            CurrentEra = Level.StartEra;
            autoSwitchTimer = 0f;
            ApplyEra();

            if (showIntro)
            {
                var pages = new string[Level.Intro.Length + 1];
                pages[0] = $"Fase {index + 1}: {Level.Name}";
                Array.Copy(Level.Intro, 0, pages, 1, Level.Intro.Length);
                ShowStory(pages, () => SetState(GameState.Playing));
            }
            else
            {
                SetState(GameState.Playing);
                ShowMessage("A fase recomeçou.");
            }
        }

        void ClearLevel()
        {
            timeObjects.Clear();
            gates.Clear();
            Player = null;
            levelBackground = null;
            if (levelRoot != null) Destroy(levelRoot);
            levelRoot = null;
        }

        void CompleteLevel()
        {
            SetState(GameState.LevelComplete);
            Synth.PlayMelody(CollectedNotes);
        }

        void NextLevel()
        {
            if (LevelIndex + 1 < LevelData.Levels.Length)
            {
                LoadLevel(LevelIndex + 1);
                return;
            }

            ClearLevel();
            Synth.PlayMelody(CollectedNotes);
            ShowStory(LevelData.Ending, () => SetState(GameState.Ending));
        }

        // ---------- Time ----------

        public void Register(TimeObject obj) => timeObjects.Add(obj);
        public void RegisterGate(Gate gate) => gates.Add(gate);

        public bool TrySwitchEra(bool silentWhenBlocked)
        {
            var target = CurrentEra.Other();
            if (Player != null && PlayerBlockedIn(target))
            {
                if (!silentWhenBlocked)
                {
                    ShowMessage($"O eco está bloqueado: algo ocupa este espaço no {target.DisplayName().ToLower()}.");
                    Synth.PlayBlocked();
                }
                return false;
            }

            CurrentEra = target;
            ApplyEra();
            Synth.PlaySweep(target);
            return true;
        }

        bool PlayerBlockedIn(Era era)
        {
            var rect = Player.WorldRect;
            foreach (var obj in timeObjects)
                if (obj != null && obj.BlocksIn(era) && obj.WorldRect.Overlaps(rect)) return true;
            return false;
        }

        public void ApplyEra()
        {
            timeObjects.RemoveAll(obj => obj == null);
            foreach (var obj in timeObjects) obj.Apply(CurrentEra);
            levelBackground?.Apply(CurrentEra);
            cam.backgroundColor = Palette.Background(CurrentEra);
        }

        // ---------- Gameplay events ----------

        public void CollectFragment(MelodyFragment fragment)
        {
            FragmentsCollected++;
            int note = LevelIndex * LevelData.FragmentsPerLevel + fragment.Order;
            CollectedNotes.Add(note);
            Synth.PlayNote(LevelData.MelodyNote(note), 0.8f);

            ShowMessage(AllFragmentsCollected
                ? "Todos os fragmentos desta fase! A porta ressoa e se abre."
                : $"Fragmento da melodia ({FragmentsCollected}/{FragmentsTotal})");
        }

        public void OnPlayerReachedDoor()
        {
            if (State != GameState.Playing) return;
            if (AllFragmentsCollected) CompleteLevel();
            else ShowMessage($"A porta está selada. Faltam {FragmentsTotal - FragmentsCollected} fragmento(s).", 1.5f);
        }

        public void OnPlayerDied()
        {
            Synth.PlayBlocked();
            CurrentEra = Level.StartEra;
            autoSwitchTimer = 0f;
            ApplyEra();
            ShowMessage("O eco te trouxe de volta ao início.");
        }

        public void OpenGates()
        {
            foreach (var gate in gates) gate.Open();
            ApplyEra();
        }

        public void Inspect(string text)
        {
            InspectText = text;
            SetState(GameState.Inspecting);
        }

        public void ShowMessage(string text, float seconds = 3f)
        {
            Message = text;
            messageUntil = Time.time + seconds;
        }
    }
}
