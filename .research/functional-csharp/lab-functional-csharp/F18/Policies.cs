namespace Lab.F18;

internal static class Policies {
    public static IO<Flight> Fallback(IO<Flight> primary, IO<Flight> secondary) =>
        primary | secondary;

    public static IO<Flight> FallbackOnOutage(IO<Flight> primary, IO<Flight> secondary) =>
        primary.Catch(Codes.ProviderDown, _ => secondary).As();

    public static IO<Flight> Retry(IO<Flight> attempt) =>
        attempt.Retry(Schedule.exponential(TimeSpan.FromMilliseconds(1)) | Schedule.recurs(3));
}
