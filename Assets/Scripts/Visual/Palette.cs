using UnityEngine;

namespace EcoDoTempo
{
    // Past = warm sepia, Future = cold neon.
    public static class Palette
    {
        public static readonly Color PastBackground = new Color(0.20f, 0.13f, 0.08f);
        public static readonly Color FutureBackground = new Color(0.04f, 0.06f, 0.13f);

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

        public static Color Background(Era era) => era == Era.Past ? PastBackground : FutureBackground;
    }
}
