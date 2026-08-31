namespace Lab.F24;

internal static class Equality {
    public static bool SameItems(Seq<int> left, Seq<int> right) => left == right;

    public static bool SamePairs(Seq<int> numbers, Seq<string> names) {
        Seq<(int First, string Second)> expected = [(1, "a"), (2, "b")];
        return numbers.Zip(names) == expected;
    }

    public static bool Has(Seq<string> names, string name) => names.Exists(item => string.Equals(item, name, StringComparison.Ordinal));

    public static int Sum(Seq<int> values) => values.Fold(0, static (sum, x) => sum + x);

    public static Seq<int> Empty() => Seq<int>();

    public static Seq<int> Ascending(Seq<int> values) => toSeq(values.Order());
}
