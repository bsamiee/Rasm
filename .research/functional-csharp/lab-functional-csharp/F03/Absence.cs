namespace Lab.F03;

internal static class Greetings {
    public static string GreetingFor(Option<string> name) =>
        name.Match(
            Some: static value => $"Dear {value.ToUpperInvariant()},",
            None: static () => "Dear Subscriber,");
}

internal static class Construction {
    public static Option<string> Present(string value) => Some(value);

    public static Option<string> Absent() => None;

    public static Option<string> AtTheBoundary(string? external) => Optional(external);

    public static Option<string> Lifted(string? external) => external;
}
