namespace EcoDoTempo
{
    public enum Era { Past, Future }

    public enum EraPresence { Both, PastOnly, FutureOnly }

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
