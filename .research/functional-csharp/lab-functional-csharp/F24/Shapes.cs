namespace Lab.F24;

internal static class Shapes {
    public static Seq<int> Parsed(Seq<string> texts) => texts.Choose(static text => parseInt(text));

    public static (Seq<int> Evens, Seq<int> Odds) Split(Seq<int> values) => values.Partition(static x => x % 2 == 0);

    public static Seq<(int First, string Second)> Paired(Seq<int> numbers, Seq<string> names) => numbers.Zip(names);

    public static Seq<string> Labelled(Seq<int> numbers, Seq<string> names) =>
        numbers.Zip(names, static (n, name) => string.Create(CultureInfo.InvariantCulture, $"{n}:{name}"));

    public static Seq<int> Running(Seq<int> values) => values.Scan(0, static (sum, x) => sum + x);

    public static Option<int> Second(Seq<int> values) => values.At(1);

    public static Option<int> First(Seq<int> values) => values.Head;

    public static Seq<int> Rest(Seq<int> values) => values.Tail;

    public static Option<int> Final(Seq<int> values) => values.Last;

    public static Seq<int> Offset(Seq<int> values) => values.Map(static (x, i) => x + i);

    public static Seq<int> Positive(Seq<int> values) => values.Filter(static x => x > 0);

    public static Seq<int> Flattened(Seq<Seq<int>> groups) => groups.Bind(static group => group);

    public static Seq<int> Present(Seq<Option<int>> values) => values.Somes();

    public static Seq<int> Reversed(Seq<int> values) => values.Rev();

    public static Seq<int> Doubling(int limit) =>
        toSeq(LanguageExt.List.unfold(1, state => state <= limit ? Some((state, state * 2)) : Option<(int, int)>.None));

    public static Seq<int> Prepended(int head, Seq<int> tail) => head.Cons(tail);
}
