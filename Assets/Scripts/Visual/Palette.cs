using UnityEngine;

namespace EcoDoTempo
{
    // Past = warm sepia, Future = cold neon.
    public static class Palette
    {
        // Near-black clears so the painted backdrop can fill the screen without letterbox bars.
        public static readonly Color PastBackground = new Color(0.05f, 0.04f, 0.03f);
        public static readonly Color FutureBackground = new Color(0.03f, 0.04f, 0.06f);

        public static readonly Color PastWall = new Color(0.55f, 0.40f, 0.25f);
        public static readonly Color FutureWall = new Color(0.22f, 0.30f, 0.45f);

        public static readonly Color PastBlock = new Color(0.95f, 0.65f, 0.30f);
        public static readonly Color FutureBlock = new Color(0.30f, 0.90f, 1.00f);

        public static readonly Color PastSpikes = new Color(0.80f, 0.30f, 0.20f);
        public static readonly Color FutureSpikes = new Color(1.00f, 0.25f, 0.55f);

        public static readonly Color PastSoil = new Color(0.40f, 0.25f, 0.12f);
        public static readonly Color FutureSoil = new Color(0.25f, 0.22f, 0.20f);
        public static readonly Color Tree = new Color(0.30f, 0.70f, 0.30f);
        public static readonly Color Sapling = new Color(0.50f, 0.85f, 0.35f);

        public static readonly Color PastGate = new Color(0.60f, 0.20f, 0.15f);
        public static readonly Color FutureGate = new Color(0.55f, 0.15f, 0.35f);

        public static readonly Color Lever = new Color(0.85f, 0.80f, 0.60f);
        public static readonly Color FutureLever = new Color(0.45f, 0.35f, 0.30f);

        public static readonly Color Fragment = new Color(1.00f, 0.92f, 0.45f);
        public static readonly Color FutureFragment = new Color(0.65f, 1.00f, 0.95f);

        public static readonly Color Inspectable = new Color(0.90f, 0.85f, 0.75f);
        public static readonly Color FutureInspectable = new Color(0.60f, 0.70f, 0.80f);

        public static readonly Color DoorLocked = new Color(0.35f, 0.30f, 0.30f);
        public static readonly Color DoorOpen = new Color(1.00f, 0.85f, 0.40f);

        public static readonly Color Player = new Color(0.98f, 0.95f, 0.90f);
        public static readonly Color PlayerScarf = new Color(0.85f, 0.20f, 0.30f);

        public static readonly Color Fire = new Color(1.00f, 0.50f, 0.15f);
        public static readonly Color Blade = new Color(0.85f, 0.88f, 0.95f);
        public static readonly Color PlateBase = new Color(0.20f, 0.18f, 0.16f);
        public static readonly Color ClockFacePast = new Color(0.96f, 0.88f, 0.68f);
        public static readonly Color ClockFaceFuture = new Color(0.55f, 0.60f, 0.68f);
        public static readonly Color Echo = new Color(0.55f, 0.92f, 1.00f, 0.55f);
        public static readonly Color Fermata = new Color(0.70f, 0.88f, 1.00f);

        // Levers, plates, clocks and gates of the same channel share a color.
        static readonly Color[] Channels =
        {
            new Color(1.00f, 0.76f, 0.30f),
            new Color(0.35f, 0.92f, 0.85f),
            new Color(0.80f, 0.55f, 1.00f),
        };

        public static Color Channel(int channel) => Channels[Mathf.Clamp(channel, 0, Channels.Length - 1)];

        public static Color Background(Era era) => era == Era.Past ? PastBackground : FutureBackground;
    }
}
