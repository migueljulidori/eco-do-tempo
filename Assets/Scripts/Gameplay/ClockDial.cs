using UnityEngine;

namespace EcoDoTempo
{
    // Cube Escape style code lock: turn the hand to the right hour in the past.
    // In the future the hand is rusted at whatever hour the past left it.
    public class ClockDial : MonoBehaviour, IInteractable
    {
        const float FaceSize = 1.25f;

        [SerializeField] int hour = 12;
        [SerializeField] int target = 12;
        [SerializeField] int channel;
        [SerializeField] bool solved;
        [SerializeField] Transform hand;

        public static ClockDial Create(Transform root, Vector2 pos, ClockPuzzle puzzle)
        {
            var go = SpriteFactory.CreateObject("Relogio", root, pos, Vector2.one * FaceSize, SpriteFactory.Circle,
                Palette.ClockFacePast, 2);
            var dial = go.AddComponent<ClockDial>();
            dial.hour = Mathf.Clamp(puzzle.Start, 1, 12);
            dial.target = Mathf.Clamp(puzzle.Target, 1, 12);
            dial.channel = puzzle.Channel;

            var rim = SpriteFactory.CreateObject("Aro", go.transform, pos, Vector2.one, SpriteFactory.Circle,
                new Color(0.25f, 0.17f, 0.10f), 1);
            rim.transform.localScale = Vector3.one * 1.12f;
            for (int h = 0; h < 12; h++)
            {
                float a = h * Mathf.PI / 6f;
                var tick = SpriteFactory.CreateObject("Marca", go.transform, pos, Vector2.one, SpriteFactory.Square,
                    new Color(0.25f, 0.17f, 0.10f), 3);
                tick.transform.localPosition = new Vector3(Mathf.Sin(a) * 0.4f, Mathf.Cos(a) * 0.4f, 0f);
                tick.transform.localScale = new Vector3(0.07f, h % 3 == 0 ? 0.14f : 0.08f, 1f);
                tick.transform.localRotation = Quaternion.Euler(0f, 0f, -h * 30f);
            }

            var pivot = new GameObject("Ponteiro");
            pivot.transform.SetParent(go.transform, false);
            var needle = SpriteFactory.CreateObject("Agulha", pivot.transform, pos, Vector2.one, SpriteFactory.Square,
                new Color(0.12f, 0.08f, 0.05f), 4);
            needle.transform.localScale = new Vector3(0.06f, 0.36f, 1f);
            needle.transform.localPosition = new Vector3(0f, 0.16f, 0f);
            dial.hand = pivot.transform;
            dial.UpdateHand();

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = Vector2.one;
            TimeBodies.AddChannelBadge(go, pos + Vector2.up * (FaceSize * 0.5f + 0.25f), dial.channel);

            // Optional drawn face without hands (Props/clock_face, Props/clock_face_future).
            // The game keeps drawing the hand on top so it can turn.
            if (ArtCatalog.TryApply(go, "Props/clock_face", Vector2.one * FaceSize))
            {
                ArtCatalog.SetEraArt(go, "Props/clock_face",
                    ArtCatalog.FirstExisting("Props/clock_face_future", "Props/clock_face"), Vector2.one * FaceSize);
                rim.SetActive(false);
                foreach (Transform child in go.transform)
                    if (child.name == "Marca") child.gameObject.SetActive(false);
            }
            TimeObject.Add(go, EraPresence.Both, Palette.ClockFacePast, Palette.ClockFaceFuture, solid: false);
            return dial;
        }

        void UpdateHand()
        {
            if (hand != null) hand.localRotation = Quaternion.Euler(0f, 0f, -30f * hour);
        }

        public string Prompt(Era era)
        {
            if (era == Era.Future) return $"Relógio parado às {hour}h";
            return solved ? $"Os ponteiros marcam {hour}h" : $"E: girar o ponteiro (agora {hour}h)";
        }

        public void Interact(Era era)
        {
            var gm = GameManager.Instance;
            if (era == Era.Future)
            {
                gm.ShowMessage($"A ferrugem travou os ponteiros às {hour}h, onde alguém os deixou no passado.");
                gm.Synth.PlayBlocked();
                return;
            }

            hour = hour % 12 + 1;
            UpdateHand();
            gm.Synth.PlayNote(880f, 0.08f, 0.35f);
            if (solved || hour != target) return;

            solved = true;
            gm.LatchChannel(channel);
            gm.Synth.PlayNote(659f, 0.6f);
            gm.ShowMessage($"{hour} badaladas. Um ferrolho se solta em algum lugar... em todas as eras.");
        }
    }
}
