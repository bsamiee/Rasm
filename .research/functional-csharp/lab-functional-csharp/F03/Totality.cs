namespace Lab.F03;

internal static class Totality {
    public static Option<int> ParseAge(string text) => parseInt(text);

    public static Option<string> Setting(HashMap<string, string> settings, string key) => settings.Find(key);

    public static Option<Age> AgeOf(HashMap<string, string> settings) =>
        Setting(settings, "age").Bind(ParseAge).Bind(static value => Age.From(value).ToOption());
}
