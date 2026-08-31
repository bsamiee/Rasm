namespace Lab.F24;

internal static class Partition {
    public static Seq<int> Strict => Seq(1, 2, 3);

    public static Seq<int> Converted(IEnumerable<int> source) => toSeq(source);

    public static Arr<int> Indexed => Array(10, 20, 30);

    public static int Third(Arr<int> items) => items[2];

    public static Lst<int> Editable => List(1, 2, 3);

    public static Lst<int> Edited(Lst<int> items) => items.Insert(1, 9).RemoveAt(0).SetItem(0, 7);

    public static Map<string, int> Ordered => Map(("b", 2), ("a", 1));

    public static HashMap<string, int> Hashed => HashMap(("a", 1), ("b", 2));

    public static HashMap<string, int> HashedFrom(Seq<(string, int)> pairs) => toHashMap(pairs);

    public static Set<int> Sorted => Set(3, 1, 2);

    public static Set<int> SortedFrom(Seq<int> items) => toSet(items);

    public static LanguageExt.HashSet<int> Unordered => HashSet(3, 1, 2);

    public static Seq<int> Doubled(IEnumerable<int> source) {
        Iterable<int> lazy = source.AsIterable().Map(static x => x * 2);
        return lazy.ToSeq();
    }

    public static IterableNE<int> NonEmpty => IterableNE.create(1, 2, 3);

    public static Option<IterableNE<int>> NonEmptyFrom(Seq<int> items) => items.AsIterableNE();

    public static Seq<int> Counted => toSeq(Range(1, 3));
}
