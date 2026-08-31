namespace Lab.F04;

internal sealed record NotFound() : Expected("not found", 4001);

internal static class Alternatives {
    public static string Describe(Option<int> option) =>
        option.Match(Some: static x => string.Create(CultureInfo.InvariantCulture, $"some {x}"), None: static () => "none");

    public static string Describe(Fin<int> fin) =>
        fin.Match(Succ: static x => string.Create(CultureInfo.InvariantCulture, $"succ {x}"), Fail: static e => e.Message);

    public static string Describe(Either<string, int> either) =>
        either.Match(Left: static l => l, Right: static r => string.Create(CultureInfo.InvariantCulture, $"right {r}"));

    public static string Describe(Validation<Error, int> validation) =>
        validation.Match(Fail: static e => e.Message, Succ: static x => string.Create(CultureInfo.InvariantCulture, $"valid {x}"));
}
