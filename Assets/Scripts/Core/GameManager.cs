using System;
using System.Collections.Generic;
using UnityEngine;

namespace EcoDoTempo
{
    public enum GameState { Title, Story, Playing, Inspecting, Paused, LevelComplete, Ending }

    public class GameManager : MonoBehaviour
    {
        public const int ChannelCount = 3;
        public const float FermataSeconds = 3f;
        public const float FermataCooldown = 1.5f;

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
        public EchoSystem Echo { get; private set; }
        public Transform LevelRoot => levelRoot != null ? levelRoot.transform : null;

        public readonly HashSet<int> CollectedNotes = new HashSet<int>();

        public string[] StoryPages { get; private set; }
        public int StoryPage { get; private set; }
        public string InspectText { get; private set; }
        public string Message { get; private set; }
        public float MessageAlpha => Mathf.Clamp01((messageUntil - Time.time) / 0.5f);
        public float AutoSwitchProgress => Level.AutoSwitchInterval > 0f ? Mathf.Clamp01(autoSwitchTimer / Level.AutoSwitchInterval) : 0f;

        // Fermata: time holds still for everything except Lia.
        public bool TimeFrozen => fermataTimer > 0f;
        public float FermataLeft => Mathf.Clamp01(fermataTimer / FermataSeconds);
        public bool FermataReady => fermataTimer <= 0f && fermataCooldown <= 0f;

        // Short full-screen tint: era switch, rewind, death.
        public Color FlashColor { get; private set; }
        public float FlashAlpha => Mathf.Clamp01((flashUntil - Time.unscaledTime) / FlashSeconds) * FlashColor.a;

        // Saved progress: the level the player last reached and the notes held when it began.
        public int SavedLevel => PlayerPrefs.GetInt(SaveLevelKey, 0);
        public bool HasSave => SavedLevel > 0 && SavedLevel < LevelData.Levels.Length;

        public bool HasPower(Power power) => (Level.Powers & power) != 0;
        public bool EraSwitchByHand => HasPower(Power.Era) && Level.AutoSwitchInterval <= 0f;

        readonly List<TimeObject> timeObjects = new List<TimeObject>();
        readonly List<Gate> gates = new List<Gate>();
        readonly List<PressurePlate> plates = new List<PressurePlate>();
        readonly bool[] latched = new bool[ChannelCount];
        readonly HashSet<int> notesAtLevelStart = new HashSet<int>();
        Action onStoryDone;
        GameObject levelRoot;
        LevelBackground levelBackground;
        Camera cam;
        CameraFollow camFollow;
        Vector2Int levelSize;
        float messageUntil;
        float autoSwitchTimer;
        float fermataTimer;
        float fermataCooldown;
        int stateChangedFrame;
        float flashUntil;

        const float FlashSeconds = 0.35f;
        const string SaveLevelKey = "eco.level";
        const string SaveNotesKey = "eco.notes";

        void Awake()
        {
            Instance = this;
            Time.timeScale = 1f;
            Synth = gameObject.AddComponent<ToneSynth>();
            Echo = gameObject.AddComponent<EchoSystem>();
            gameObject.AddComponent<GameUI>();
            SetupCamera();
            HideAuthoringLevels();
            SetState(GameState.Title);
        }

        // Old scenes may still hold placed copies of the levels. Play always builds from LevelData.
        static void HideAuthoringLevels()
        {
            var catalogs = UnityEngine.Object.FindObjectsByType<LevelCatalog>(FindObjectsInactive.Include);
            foreach (var catalog in catalogs)
                catalog.gameObject.SetActive(false);
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

        // False on the frame the state changed, so one key press is never handled twice.
        public bool InputReady => Time.frameCount > stateChangedFrame;

        // Testing shortcuts: in the Inspector, ⋮ on GameManager, or keys 1–6 on the title screen.
        [ContextMenu("Debug/Abrir fase 1 sem introdução")] void DebugLevel1() => DebugLoadLevel(0);
        [ContextMenu("Debug/Abrir fase 2 sem introdução")] void DebugLevel2() => DebugLoadLevel(1);
        [ContextMenu("Debug/Abrir fase 3 sem introdução")] void DebugLevel3() => DebugLoadLevel(2);
        [ContextMenu("Debug/Abrir fase 4 sem introdução")] void DebugLevel4() => DebugLoadLevel(3);
        [ContextMenu("Debug/Abrir fase 5 sem introdução")] void DebugLevel5() => DebugLoadLevel(4);
        [ContextMenu("Debug/Abrir fase 6 sem introdução")] void DebugLevel6() => DebugLoadLevel(5);

        public static bool LevelShortcutsEnabled => Application.isEditor || Debug.isDebugBuild;

        void DebugLoadLevel(int index)
        {
            if (!Application.isPlaying || index < 0 || index >= LevelData.Levels.Length) return;
            CollectedNotes.Clear();
            notesAtLevelStart.Clear();
            LoadLevel(index, showIntro: false);
        }

        void Update()
        {
            if (!InputReady) return;

            switch (State)
            {
                case GameState.Title:
                    if (GameInput.ConfirmPressed && HasSave)
                    {
                        ContinueSavedGame();
                    }
                    else if (GameInput.ConfirmPressed || (GameInput.NewGamePressed && HasSave))
                    {
                        ClearSave();
                        CollectedNotes.Clear();
                        ShowStory(LevelData.Intro, () => LoadLevel(0));
                    }
                    else if (LevelShortcutsEnabled && GameInput.NumberPressed >= 0)
                    {
                        DebugLoadLevel(GameInput.NumberPressed);
                    }
                    break;

                case GameState.Story:
                    if (GameInput.PausePressed && StoryPage < StoryPages.Length - 1)
                    {
                        // Esc skips to the last page; Enter then starts as usual.
                        StoryPage = StoryPages.Length - 1;
                        stateChangedFrame = Time.frameCount;
                    }
                    else if (GameInput.ConfirmPressed)
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
                    if (GameInput.ConfirmPressed || GameInput.InteractPressed || GameInput.PausePressed)
                        SetState(GameState.Playing);
                    break;

                case GameState.Paused:
                    if (GameInput.PausePressed || GameInput.ConfirmPressed) Resume();
                    else if (GameInput.RestartPressed)
                    {
                        Time.timeScale = 1f;
                        LoadLevel(LevelIndex, showIntro: false);
                    }
                    else if (GameInput.MenuPressed)
                    {
                        Time.timeScale = 1f;
                        ClearLevel();
                        cam.backgroundColor = Palette.Background(Era.Past);
                        SetState(GameState.Title);
                    }
                    break;

                case GameState.LevelComplete:
                    if (GameInput.ConfirmPressed) NextLevel();
                    break;

                case GameState.Ending:
                    if (GameInput.ConfirmPressed)
                    {
                        ClearSave();
                        ClearLevel();
                        cam.backgroundColor = Palette.Background(Era.Past);
                        SetState(GameState.Title);
                    }
                    break;
            }
        }

        void UpdatePlaying()
        {
            if (GameInput.PausePressed)
            {
                Time.timeScale = 0f;
                SetState(GameState.Paused);
                return;
            }

            if (GameInput.RestartPressed)
            {
                LoadLevel(LevelIndex, showIntro: false);
                return;
            }

            UpdateFermata();

            if (GameInput.EchoPressed)
            {
                if (HasPower(Power.Echo)) Echo.Toggle();
                else ShowMessage(PowerLearnedBefore(Power.Echo)
                    ? "O eco não responde aqui: o tempo está instável demais."
                    : "O diapasão ainda não sabe ecoar.", 2f);
            }

            if (Level.AutoSwitchInterval > 0f)
            {
                if (!TimeFrozen) autoSwitchTimer += Time.deltaTime;
                // If Lia blocks the switch, keep trying every frame instead of waiting a full beat.
                if (autoSwitchTimer >= Level.AutoSwitchInterval && TrySwitchEra(silentWhenBlocked: true))
                    autoSwitchTimer = 0f;
                if (GameInput.TimeSwitchPressed)
                    ShowMessage(HasPower(Power.Fermata)
                        ? "O tempo oscila sozinho aqui. Segure-o com a fermata (C)."
                        : "O diapasão não responde. O tempo oscila sozinho aqui.");
            }
            else if (GameInput.TimeSwitchPressed)
            {
                if (HasPower(Power.Era)) TrySwitchEra(silentWhenBlocked: false);
            }

            UpdateChannels();
        }

        void UpdateFermata()
        {
            if (fermataTimer > 0f)
            {
                fermataTimer -= Time.deltaTime;
                if (fermataTimer <= 0f || GameInput.FermataPressed) EndFermata();
                return;
            }

            if (fermataCooldown > 0f) fermataCooldown -= Time.deltaTime;
            if (!GameInput.FermataPressed) return;
            if (!HasPower(Power.Fermata))
            {
                ShowMessage(PowerLearnedBefore(Power.Fermata)
                    ? "A fermata não responde aqui."
                    : "O diapasão ainda não sabe segurar uma nota.", 2f);
                return;
            }
            if (fermataCooldown > 0f) return;

            fermataTimer = FermataSeconds;
            Synth.PlayFermata();
            ShowMessage("Fermata: o tempo prende a respiração.", 1.5f);
        }

        void EndFermata()
        {
            fermataTimer = 0f;
            fermataCooldown = FermataCooldown;
            Synth.PlayNote(392f, 0.25f, 0.3f);
        }

        bool PowerLearnedBefore(Power power)
        {
            for (int i = 0; i < LevelIndex; i++)
                if ((LevelData.Levels[i].Powers & power) != 0) return true;
            return false;
        }

        void Resume()
        {
            Time.timeScale = 1f;
            SetState(GameState.Playing);
        }

        public void Flash(Color color)
        {
            FlashColor = color;
            flashUntil = Time.unscaledTime + FlashSeconds;
        }

        // ---------- Save ----------

        void SaveProgress()
        {
            PlayerPrefs.SetInt(SaveLevelKey, LevelIndex);
            PlayerPrefs.SetString(SaveNotesKey, string.Join(",", notesAtLevelStart));
            PlayerPrefs.Save();
        }

        void ContinueSavedGame()
        {
            CollectedNotes.Clear();
            foreach (var part in PlayerPrefs.GetString(SaveNotesKey, "").Split(','))
                if (int.TryParse(part, out int note)) CollectedNotes.Add(note);
            LoadLevel(SavedLevel);
        }

        static void ClearSave()
        {
            PlayerPrefs.DeleteKey(SaveLevelKey);
            PlayerPrefs.DeleteKey(SaveNotesKey);
            PlayerPrefs.Save();
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
                SaveProgress();
            }
            else
            {
                CollectedNotes.Clear();
                CollectedNotes.UnionWith(notesAtLevelStart);
            }

            FragmentsCollected = 0;
            Synth.StopMelody();
            levelRoot = new GameObject("Level " + (index + 1));
            var result = LevelBuilder.Build(Level, levelRoot.transform);
            levelSize = result.Size;
            FragmentsTotal = result.FragmentCount;

            CameraFollow.FitToLevel(cam, levelSize.x);
            levelBackground = LevelBackground.Create(levelRoot.transform, Level.Background, levelSize, result.FloorTopY);
            Player = PlayerController.Create(result.Spawn, levelRoot.transform);
            camFollow.SetTarget(Player.transform, levelSize, result.FloorTopY);

            CurrentEra = Level.StartEra;
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
            plates.Clear();
            Array.Clear(latched, 0, latched.Length);
            Echo.ResetAll();
            autoSwitchTimer = 0f;
            fermataTimer = 0f;
            fermataCooldown = 0f;
            messageUntil = 0f;
            Player = null;
            levelBackground = null;
            if (levelRoot != null) Destroy(levelRoot);
            levelRoot = null;
        }

        void CompleteLevel()
        {
            Echo.Cancel();
            fermataTimer = 0f;
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
        public void RegisterPlate(PressurePlate plate) => plates.Add(plate);

        public bool TrySwitchEra(bool silentWhenBlocked)
        {
            var target = CurrentEra.Other();
            if (Player != null && PlayerBlockedIn(target))
            {
                if (!silentWhenBlocked)
                {
                    ShowMessage($"Não dá para mudar de era aqui: algo ocupa este lugar no {target.DisplayName().ToLower()}.");
                    Synth.PlayBlocked();
                }
                return false;
            }

            CurrentEra = target;
            ApplyEra();
            Synth.PlaySweep(target);
            Flash(target == Era.Past ? new Color(1f, 0.75f, 0.4f, 0.22f) : new Color(0.45f, 0.85f, 1f, 0.22f));
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

        // ---------- Channels (levers, clocks, plates → gates) ----------

        public void LatchChannel(int channel)
        {
            if (channel < 0 || channel >= ChannelCount) return;
            latched[channel] = true;
            UpdateChannels();
        }

        public bool ChannelOpen(int channel)
        {
            if (channel < 0 || channel >= ChannelCount) return false;
            if (latched[channel]) return true;
            bool any = false;
            foreach (var plate in plates)
            {
                if (plate == null || plate.Channel != channel) continue;
                if (!plate.Pressed) return false;
                any = true;
            }
            return any;
        }

        void UpdateChannels()
        {
            foreach (var gate in gates)
                if (gate != null) gate.SetOpen(ChannelOpen(gate.Channel));
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
            else
            {
                int left = FragmentsTotal - FragmentsCollected;
                ShowMessage(left == 1 ? "A porta está selada. Falta 1 fragmento."
                    : $"A porta está selada. Faltam {left} fragmentos.", 1.5f);
            }
        }

        public void OnPlayerDied(Era respawnEra)
        {
            Synth.PlayBlocked();
            Flash(new Color(0.8f, 0.1f, 0.1f, 0.45f));
            Echo.Cancel();
            if (fermataTimer > 0f) EndFermata();
            // Back to the last safe spot, in the era Lia stood there (it is free in that era).
            CurrentEra = respawnEra;
            autoSwitchTimer = 0f;
            ApplyEra();
            camFollow.Snap();
            ShowMessage("O diapasão trouxe você de volta. (R reinicia a fase inteira.)");
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
