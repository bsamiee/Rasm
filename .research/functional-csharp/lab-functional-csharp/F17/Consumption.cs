namespace Lab.F17;

internal static class Consumption {
    public static Option<Session> Final(Seq<Session> states) => states.Last;

    public static Seq<string> Messages(Seq<Session> states) => states.Map(Describe);

    public static (Seq<string> Messages, Session State) Report(Seq<Session> states, Session initial) =>
        states.Fold(
            (Messages: Seq<string>(), State: initial),
            static (acc, state) => (acc.Messages.Add(Describe(state)), state));

    private static string Describe(Session state) =>
        string.Create(CultureInfo.InvariantCulture, $"remaining {state.Remaining}");
}
