namespace Lab.F12;

internal static partial class Validators {
    public static Func<T, Validation<Error, T>> FailFast<T>(Seq<Func<T, Validation<Error, T>>> validators) =>
        value => validators.TraverseM(validate => validate(value)).As().Map(_ => value);
}

internal static partial class Validators {
    public static Func<T, Validation<Error, T>> HarvestErrors<T>(Seq<Func<T, Validation<Error, T>>> validators) =>
        value => validators.Traverse(validate => validate(value)).As().Map(_ => value);
}

internal static class ValidatorsProbe {
    private static Validation<Error, string> Digits(string text) =>
        text.All(char.IsAsciiDigit) ? text : Error.New("not digits");

    private static Validation<Error, string> Short(string text) =>
        text.Length < 4 ? text : Error.New("too long");

    public static Fin<Unit> Run() {
        Seq<Func<string, Validation<Error, string>>> checks = [Digits, Short];
        Func<string, Validation<Error, string>> failFast = Validators.FailFast(checks);
        Func<string, Validation<Error, string>> harvest = Validators.HarvestErrors(checks);
        Validation<Error, string> passed = harvest("123");
        Validation<Error, string> emptyList = Validators.FailFast(Seq<Func<string, Validation<Error, string>>>())("abcd");
        int failFastCount = failFast("abcd").Match(Fail: static e => e.Count, Succ: static _ => 0);
        int harvestCount = harvest("abcd").Match(Fail: static e => e.Count, Succ: static _ => 0);
        return Samples.Check(
            nameof(Validators),
            ("passed == Pure(\"123\")", passed == Pure("123")),
            ("emptyList == Pure(\"abcd\")", emptyList == Pure("abcd")),
            ("failFastCount == 1", failFastCount == 1),
            ("harvestCount == 2", harvestCount == 2));
    }
}
