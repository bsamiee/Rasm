namespace Lab.F24;

internal static class Folds {
    public static int Total(Seq<int> values) => values.Fold(0, static (sum, x) => sum + x);

    public static string Forward(Seq<int> values) => values.Fold("", static (text, x) => string.Create(CultureInfo.InvariantCulture, $"{text}{x}"));

    public static string Backward(Seq<int> values) => values.FoldBack("", static (text, x) => string.Create(CultureInfo.InvariantCulture, $"{text}{x}"));

    public static int WhileUnderTen(Seq<int> values) => values.FoldWhile(0, static (sum, x) => sum + x, static pair => pair.State < 10);

    public static int UntilNegative(Seq<int> values) => values.FoldUntil(0, static (sum, x) => sum + x, static pair => pair.Value < 0);

    public static Option<string> MonadicTailFirst(Seq<int> values) =>
        values.FoldM("", static (string text, int x) => Some(string.Create(CultureInfo.InvariantCulture, $"{text}{x}"))).As();

    public static Option<string> MonadicHeadFirst(Seq<int> values) =>
        values.FoldBackM("", static (string text, int x) => Some(string.Create(CultureInfo.InvariantCulture, $"{text}{x}"))).As();

    public static Seq<int> Joined(Seq<Seq<int>> groups) => groups.Fold();

    public static bool AnyEven(Seq<int> values, Atom<int> visited) => values.Exists(x => visited.Swap(static n => n + 1) > 0 && x % 2 == 0);

    public static bool AllPositive(Seq<int> values, Atom<int> visited) => values.ForAll(x => visited.Swap(static n => n + 1) > 0 && x > 0);
}
