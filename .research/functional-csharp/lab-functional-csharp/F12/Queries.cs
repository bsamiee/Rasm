namespace Lab.F12;

internal sealed record NotANumber() : Expected("not a number", 1200);

internal static class Queries {
    public static Validation<Error, int> ValidInt(string text) =>
        parseInt(text).ToValidation<Error>(new NotANumber());

    public static Option<int> Total(string first, string second) =>
        from a in parseInt(first)
        from b in parseInt(second)
        select a + b;

    public static Validation<Error, int> Sum(string first, string second) =>
        from a in ValidInt(first)
        from b in ValidInt(second)
        select a + b;
}

internal static class QueriesProbe {
    public static Fin<Unit> Run() {
        Option<int> total = Queries.Total("3", "4");
        Option<int> missing = Queries.Total("3", "x");
        Validation<Error, int> sum = Queries.Sum("3", "4");
        int errorCount = Queries.Sum("x", "y").Match(Fail: static e => e.Count, Succ: static _ => 0);
        return Samples.Check(
            nameof(Queries),
            ("total == Some(7)", total == Some(7)),
            ("missing.IsNone", missing.IsNone),
            ("sum == Pure(7)", sum == Pure(7)),
            ("errorCount == 1", errorCount == 1));
    }
}
