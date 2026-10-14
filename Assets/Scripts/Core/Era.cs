namespace EcoDoTempo
{
    public enum Era { Past, Future }

    public enum EraPresence { Both, PastOnly, FutureOnly }

    // Lia's time powers. Each level lists the ones the tuning fork has already learned.
    [System.Flags]
    public enum Power
    {
        None = 0,
        Era = 1,      // Q: cross between past and future
        Echo = 2,     // F: record a few seconds, rewind, and leave an echo that repeats them
        Fermata = 4,  // C: hold time still for a moment (machines, blades, rhythm, crates)
        All = Era | Echo | Fermata,
    }

    public static class EraExtensions
    {
        public static Era Other(this Era era) => era == Era.Past ? Era.Future : Era.Past;

        public static string DisplayName(this Era era) => era == Era.Past ? "PASSADO" : "FUTURO";

        public static bool Includes(this EraPresence presence, Era era) =>
            presence == EraPresence.Both
            || (presence == EraPresence.PastOnly && era == Era.Past)
            || (presence == EraPresence.FutureOnly && era == Era.Future);
    }
}
