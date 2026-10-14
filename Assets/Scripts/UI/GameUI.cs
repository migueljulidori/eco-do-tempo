using UnityEngine;

namespace EcoDoTempo
{
    // Immediate-mode UI, designed for a 1280x720 virtual screen and scaled to fit.
    public class GameUI : MonoBehaviour
    {
        const float VirtualHeight = 720f;

        GUIStyle title, subtitle, body, small, center, eraLabel, prompt, message, heading;
        GUIStyle[] styles;
        int[] baseFontSizes;
        Texture2D white;
        float width;
        float uiScale = 1f;
        float fontScale = -1f;

        void InitStyles()
        {
            if (title != null) return;
            white = Texture2D.whiteTexture;
            title = new GUIStyle(GUI.skin.label) { fontSize = 72, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            subtitle = new GUIStyle(title) { fontSize = 26, fontStyle = FontStyle.Italic };
            body = new GUIStyle(GUI.skin.label) { fontSize = 26, wordWrap = true, alignment = TextAnchor.MiddleCenter };
            small = new GUIStyle(GUI.skin.label) { fontSize = 18, wordWrap = false };
            center = new GUIStyle(small) { alignment = TextAnchor.MiddleCenter };
            eraLabel = new GUIStyle(small) { fontSize = 26, fontStyle = FontStyle.Bold };
            prompt = new GUIStyle(center) { fontSize = 24 };
            message = new GUIStyle(body) { fontSize = 20 };
            heading = new GUIStyle(title) { fontSize = 42 };
            styles = new[] { title, subtitle, body, small, center, eraLabel, prompt, message, heading };
            baseFontSizes = new int[styles.Length];
            for (int i = 0; i < styles.Length; i++)
            {
                styles[i].normal.textColor = Color.white;
                baseFontSizes[i] = styles[i].fontSize;
            }
        }

        // Text is rendered at the real screen size (sharp at 1080p and 4K), not stretched.
        void ScaleFonts()
        {
            if (Mathf.Approximately(fontScale, uiScale)) return;
            fontScale = uiScale;
            for (int i = 0; i < styles.Length; i++)
                styles[i].fontSize = Mathf.Max(1, Mathf.RoundToInt(baseFontSizes[i] * uiScale));
        }

        // Layout is written for a 1280×720 virtual screen; this maps it to real pixels.
        Rect ToScreen(Rect r) => new Rect(r.x * uiScale, r.y * uiScale, r.width * uiScale, r.height * uiScale);

        void OnGUI()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            InitStyles();

            uiScale = Mathf.Max(0.1f, Screen.height / VirtualHeight);
            width = Screen.width / uiScale;
            GUI.matrix = Matrix4x4.identity;
            ScaleFonts();

            switch (gm.State)
            {
                case GameState.Title: DrawTitle(gm); break;
                case GameState.Story: DrawStory(gm); break;
                case GameState.Playing: DrawHud(gm); break;
                case GameState.Inspecting: DrawHud(gm); DrawInspect(gm); break;
                case GameState.Paused: DrawHud(gm); DrawPause(); break;
                case GameState.LevelComplete: DrawHud(gm); DrawLevelComplete(gm); break;
                case GameState.Ending: DrawEnding(gm); break;
            }

            if (gm.FlashAlpha > 0f)
            {
                var c = gm.FlashColor;
                c.a = gm.FlashAlpha;
                Fill(new Rect(0, 0, width, VirtualHeight), c);
            }
        }

        void DrawTitle(GameManager gm)
        {
            Fill(new Rect(0, 0, width, VirtualHeight), new Color(0, 0, 0, 0.6f));
            float pulse = 0.6f + 0.4f * Mathf.Sin(Time.time * 2f);
            Label(new Rect(0, 200, width, 100), "ECO DO TEMPO", title, Color.Lerp(Palette.PastBlock, Palette.FutureBlock, Mathf.PingPong(Time.time * 0.3f, 1f)));
            Label(new Rect(0, 300, width, 40), "Um quebra-cabeça entre passado e futuro", subtitle, Color.white);
            if (gm.HasSave)
            {
                Label(new Rect(0, 470, width, 40), $"ENTER para continuar (Fase {gm.SavedLevel + 1}: {LevelData.Levels[gm.SavedLevel].Name})",
                    body, new Color(1, 1, 1, pulse));
                Label(new Rect(0, 515, width, 30), "N para começar um jogo novo", center, new Color(1, 1, 1, 0.7f));
            }
            else
            {
                Label(new Rect(0, 480, width, 40), "Pressione ENTER para começar", body, new Color(1, 1, 1, pulse));
            }
            if (GameManager.LevelShortcutsEnabled)
                Label(new Rect(0, 640, width, 30), $"Teste: teclas 1–{LevelData.Levels.Length} abrem uma fase direto", center,
                    new Color(1, 1, 1, 0.45f));
        }

        void DrawStory(GameManager gm)
        {
            Fill(new Rect(0, 0, width, VirtualHeight), new Color(0, 0, 0, 0.85f));
            int page = Mathf.Min(gm.StoryPage, gm.StoryPages.Length - 1);
            Label(new Rect(width * 0.15f, 160, width * 0.7f, 360), gm.StoryPages[page], body, Color.white);
            Label(new Rect(0, 600, width, 30), $"{page + 1}/{gm.StoryPages.Length}     ENTER para continuar     ESC pula", center, new Color(1, 1, 1, 0.7f));
        }

        void DrawHud(GameManager gm)
        {
            var eraColor = gm.CurrentEra == Era.Past ? Palette.PastBlock : Palette.FutureBlock;

            if (gm.TimeFrozen)
            {
                // Fermata: the whole frame goes pale and still.
                Fill(new Rect(0, 0, width, VirtualHeight), new Color(0.75f, 0.9f, 1f, 0.12f));
                var bar = new Rect(width * 0.5f - 160, VirtualHeight - 160, 320, 10);
                Fill(bar, new Color(0, 0, 0, 0.5f));
                Fill(new Rect(bar.x, bar.y, bar.width * gm.FermataLeft, bar.height), Palette.Fermata);
                Label(new Rect(bar.x, bar.y - 28, bar.width, 24), "FERMATA — C para soltar", center, Palette.Fermata);
            }

            Fill(new Rect(10, 10, 380, 126), new Color(0, 0, 0, 0.45f));
            Label(new Rect(22, 14, 360, 26), $"Fase {gm.LevelIndex + 1}: {gm.Level.Name}", small, Color.white);
            Label(new Rect(22, 40, 360, 30), gm.CurrentEra.DisplayName(), eraLabel, eraColor);
            Label(new Rect(22, 74, 360, 26), $"Fragmentos: {gm.FragmentsCollected}/{gm.FragmentsTotal}", small, Palette.Fragment);
            DrawPowers(gm, new Rect(22, 102, 360, 26));

            DrawMelody(gm);

            if (gm.Echo.Recording)
            {
                var bar = new Rect(width * 0.5f - 200, VirtualHeight - 230, 400, 12);
                float blink = 0.6f + 0.4f * Mathf.Sin(Time.time * 8f);
                Fill(bar, new Color(0, 0, 0, 0.55f));
                Fill(new Rect(bar.x, bar.y, bar.width * gm.Echo.RecordProgress, bar.height), new Color(1f, 0.35f, 0.35f));
                Label(new Rect(width * 0.5f - 340, bar.y - 28, 680, 24),
                    $"REC  GRAVANDO ECO  {gm.Echo.RecordedSeconds:0.0}s / {EchoSystem.MaxSeconds:0}s   —   F para voltar no tempo",
                    center, new Color(1f, 0.5f, 0.5f, blink));
            }

            if (gm.Level.AutoSwitchInterval > 0f)
            {
                // Above the controls line, away from the melody panel at the top.
                var bar = new Rect(width * 0.5f - 200, VirtualHeight - 72, 400, 14);
                Fill(bar, new Color(0, 0, 0, 0.5f));
                Fill(new Rect(bar.x, bar.y, bar.width * gm.AutoSwitchProgress, bar.height), eraColor);
                Label(new Rect(bar.x, bar.yMax + 2, bar.width, 22), "ritmo do tempo", center, new Color(1, 1, 1, 0.6f));
            }

            if (gm.Player != null && gm.Player.Nearby != null && gm.State == GameState.Playing)
                Label(new Rect(0, VirtualHeight - 120, width, 30), gm.Player.Nearby.Prompt(gm.CurrentEra),
                    prompt, Color.white);

            if (gm.MessageAlpha > 0f)
            {
                var rect = new Rect(width * 0.2f, 150, width * 0.6f, 60);
                Fill(rect, new Color(0, 0, 0, 0.55f * gm.MessageAlpha));
                Label(rect, gm.Message, message, new Color(1, 1, 1, gm.MessageAlpha));
            }

            Label(new Rect(0, VirtualHeight - 36, width, 28), Controls(gm), center, new Color(1, 1, 1, 0.6f));
        }

        static string Controls(GameManager gm)
        {
            var text = "A/D mover   ESPAÇO pular   E interagir";
            if (gm.EraSwitchByHand) text += "   Q era";
            if (gm.HasPower(Power.Echo)) text += "   F eco";
            if (gm.HasPower(Power.Fermata)) text += "   C fermata";
            text += "   R reiniciar   ESC pausa";
            if (gm.Level.AutoSwitchInterval > 0f) text += "   (Q desativado)";
            return text;
        }

        // The tuning fork's notes: which time powers this level allows and whether they are ready.
        void DrawPowers(GameManager gm, Rect rect)
        {
            float x = rect.x;
            x = PowerTag(gm, x, rect.y, "Q Eras", gm.EraSwitchByHand, true, Palette.PastBlock);
            x = PowerTag(gm, x, rect.y, gm.Echo.HasEcho ? "F Eco (ativo)" : "F Eco", gm.HasPower(Power.Echo),
                !gm.Echo.Recording, Palette.Echo);
            PowerTag(gm, x, rect.y, "C Fermata", gm.HasPower(Power.Fermata), gm.FermataReady || gm.TimeFrozen,
                Palette.Fermata);
        }

        float PowerTag(GameManager gm, float x, float y, string text, bool available, bool ready, Color color)
        {
            if (!available) return x;
            float w = small.CalcSize(new GUIContent(text)).x / uiScale + 14f;
            var c = color;
            c.a = ready ? 0.9f : 0.35f;
            Fill(new Rect(x, y + 2, w, 22), new Color(0, 0, 0, 0.35f));
            Label(new Rect(x + 7, y, w, 26), text, small, c);
            return x + w + 6f;
        }

        void DrawMelody(GameManager gm)
        {
            int total = LevelData.MelodyLength;
            float size = 18f, gap = 6f;
            float x = width - 20 - total * (size + gap);
            Fill(new Rect(x - 10, 10, total * (size + gap) + 14, 56), new Color(0, 0, 0, 0.45f));
            Label(new Rect(x, 12, 300, 22), "Canção do Eco", small, Color.white);
            for (int i = 0; i < total; i++)
            {
                var color = gm.CollectedNotes.Contains(i) ? Palette.Fragment : new Color(1, 1, 1, 0.15f);
                Fill(new Rect(x + i * (size + gap), 38, size, size), color);
            }
        }

        void DrawPause()
        {
            Fill(new Rect(0, 0, width, VirtualHeight), new Color(0, 0, 0, 0.6f));
            Label(new Rect(0, 230, width, 60), "PAUSADO", heading, Color.white);
            Label(new Rect(0, 320, width, 32), "ESC ou ENTER   continuar", body, Color.white);
            Label(new Rect(0, 362, width, 32), "R   reiniciar a fase", body, new Color(1, 1, 1, 0.85f));
            Label(new Rect(0, 404, width, 32), "M   voltar ao título (o jogo salva no início de cada fase)", body, new Color(1, 1, 1, 0.85f));
        }

        void DrawInspect(GameManager gm)
        {
            var rect = new Rect(width * 0.2f, 220, width * 0.6f, 260);
            Fill(rect, new Color(0.05f, 0.04f, 0.03f, 0.92f));
            Label(new Rect(rect.x + 30, rect.y + 20, rect.width - 60, rect.height - 70), gm.InspectText, body, Color.white);
            Label(new Rect(rect.x, rect.yMax - 44, rect.width, 30), "E ou ENTER para fechar", center, new Color(1, 1, 1, 0.6f));
        }

        void DrawLevelComplete(GameManager gm)
        {
            var rect = new Rect(width * 0.25f, 240, width * 0.5f, 220);
            Fill(rect, new Color(0, 0, 0, 0.85f));
            Label(new Rect(rect.x, rect.y + 30, rect.width, 60), "Fase concluída!", heading, Palette.DoorOpen);
            Label(new Rect(rect.x, rect.y + 100, rect.width, 40), $"A melodia cresce: {gm.CollectedNotes.Count}/{LevelData.MelodyLength} notas", body, Color.white);
            Label(new Rect(rect.x, rect.yMax - 50, rect.width, 30), "ENTER para continuar", center, new Color(1, 1, 1, 0.7f));
        }

        void DrawEnding(GameManager gm)
        {
            Fill(new Rect(0, 0, width, VirtualHeight), new Color(0, 0, 0, 0.9f));
            Label(new Rect(0, 200, width, 100), "ECO DO TEMPO", title, Palette.DoorOpen);
            Label(new Rect(0, 310, width, 40), $"Notas recuperadas: {gm.CollectedNotes.Count}/{LevelData.MelodyLength}", body, Color.white);
            Label(new Rect(0, 360, width, 40), "Obrigado por jogar!", subtitle, Color.white);
            Label(new Rect(0, 520, width, 40), "ENTER para voltar ao início", body, new Color(1, 1, 1, 0.7f));
        }

        void Fill(Rect rect, Color color)
        {
            var old = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(ToScreen(rect), white);
            GUI.color = old;
        }

        void Label(Rect rect, string text, GUIStyle style, Color color)
        {
            var old = style.normal.textColor;
            style.normal.textColor = color;
            GUI.Label(ToScreen(rect), text, style);
            style.normal.textColor = old;
        }
    }
}
