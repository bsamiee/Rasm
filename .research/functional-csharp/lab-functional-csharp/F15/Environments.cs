namespace Lab.F15;

internal sealed record Settings(string Target, int Timeout);

internal sealed record AppSettings(string Name, Settings Database);

internal static partial class Environments {
    public static Reader<Settings, string> Target => Reader.asks<Settings, string>(static settings => settings.Target);

    public static Reader<Settings, string> Described =>
        from settings in Reader.ask<Settings>()
        from target in Target
        select string.Create(CultureInfo.InvariantCulture, $"{target} within {settings.Timeout}s");

    public static Reader<Settings, string> Patient => Reader.local(static (Settings settings) => settings with { Timeout = settings.Timeout * 2 }, Described);

    public static Reader<AppSettings, string> FromApp => Described.With(static (AppSettings app) => app.Database);
}

internal static partial class Environments {
    public static ReaderT<Settings, IO, int> Queried(Atom<int> queries) =>
        from settings in ReaderT.ask<IO, Settings>()
        from count in IO.lift(() => queries.Swap(static n => n + 1))
        select count * settings.Timeout;

    public static ReaderT<AppSettings, IO, int> QueriedFromApp(Atom<int> queries) => ReaderT.with(static (AppSettings app) => app.Database, Queried(queries));
}
