namespace Lab.F22;

internal sealed record Runtime(ConsoleIO Console) : Has<Eff<Runtime>, ConsoleIO> {
    static K<Eff<Runtime>, ConsoleIO> Has<Eff<Runtime>, ConsoleIO>.Ask => Eff.runtime<Runtime>().Map(static rt => rt.Console);
}

internal static class Runtimes {
    public static Eff<RT, string> Greet<RT>(string prompt) where RT : Has<Eff<RT>, ConsoleIO> =>
        from _ in Console<RT>.writeLine(prompt)
        from line in Console<RT>.readLine
        select line;

    public static Eff<RT, string> RoundTrip<RT>(string path, string text) where RT : Has<Eff<RT>, FileIO>, Has<Eff<RT>, EncodingIO> =>
        from _ in File<RT>.writeAllText(path, text)
        from read in File<RT>.readAllText(path)
        select read;

    public static Eff<Runtime, int> Entered => Construction.Plain;

    public static Eff<Runtime, Runtime> Ask => Eff.runtime<Runtime>();
}
