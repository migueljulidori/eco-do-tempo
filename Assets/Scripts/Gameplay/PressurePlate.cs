using UnityEngine;

namespace EcoDoTempo
{
    // Holds every gate of its channel open while Lia, her echo or a crate stands on it.
    // A channel with several plates opens only when all of them are pressed.
    public class PressurePlate : MonoBehaviour
    {
        [SerializeField] int channel;
        [SerializeField] float width = 1f;
        Transform pad;
        SpriteRenderer padRenderer;
        float padRestY;

        public int Channel => channel;
        public bool Pressed { get; private set; }

        public static PressurePlate Create(Transform root, Vector2 firstCell, int cells, int channel)
        {
            float floorTop = ArtCatalog.FloorTop(firstCell);
            var center = new Vector2(firstCell.x + (cells - 1) * 0.5f, floorTop);
            var go = new GameObject("Placa");
            go.transform.SetParent(root, false);
            go.transform.position = center;

            var plate = go.AddComponent<PressurePlate>();
            plate.channel = channel;
            plate.width = cells;

            var frame = SpriteFactory.CreateObject("Base", go.transform, center + Vector2.up * 0.06f,
                new Vector2(cells * 0.92f, 0.12f), SpriteFactory.Square, Palette.PlateBase, 3);
            var padGo = SpriteFactory.CreateObject("Pad", go.transform, center + Vector2.up * 0.16f,
                new Vector2(cells * 0.78f, 0.1f), SpriteFactory.Square, Palette.Channel(channel), 4);
            plate.pad = padGo.transform;
            plate.padRenderer = padGo.GetComponent<SpriteRenderer>();
            plate.padRestY = 0.16f;
            TimeBodies.AddChannelBadge(frame, center + new Vector2(0f, 0.45f), channel);

            if (GameManager.Instance != null) GameManager.Instance.RegisterPlate(plate);
            return plate;
        }

        void Awake()
        {
            if (pad == null && transform.childCount > 1)
            {
                pad = transform.GetChild(1);
                padRenderer = pad.GetComponent<SpriteRenderer>();
                padRestY = pad.localPosition.y;
            }
        }

        void FixedUpdate()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.State != GameState.Playing) return;

            var center = (Vector2)transform.position + Vector2.up * 0.35f;
            bool pressed = TimeBodies.AnyBodyIn(center, new Vector2(width * 0.85f, 0.5f));
            if (pressed == Pressed) return;

            Pressed = pressed;
            if (pad != null)
                pad.localPosition = new Vector3(pad.localPosition.x, pressed ? padRestY - 0.07f : padRestY, 0f);
            if (padRenderer != null)
                padRenderer.color = pressed ? Color.Lerp(Palette.Channel(channel), Color.white, 0.5f) : Palette.Channel(channel);
            gm.Synth.PlayNote(pressed ? 523f : 392f, 0.18f, 0.3f);
        }
    }
}
