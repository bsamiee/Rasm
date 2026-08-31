namespace Lab.F01;

internal static class Signatures {
    public static (Seq<DayOfWeek> Days, Seq<DayOfWeek> WeekendStarts) Closure() {
        Seq<DayOfWeek> days = toSeq(Enum.GetValues<DayOfWeek>());

        Seq<DayOfWeek> DaysStartingWith(string pattern) =>
            days.Filter(day => day.ToString().StartsWith(pattern, StringComparison.Ordinal));

        Seq<DayOfWeek> weekendStarts = DaysStartingWith("S"); // Sunday, Saturday
        return (days, weekendStarts);
    }
}
