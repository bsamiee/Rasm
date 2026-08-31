namespace Lab.F16;

internal static partial class Generator {
    public static State<int, int> NextInt { get; } = new(static seed => {
        int shifted = seed ^ (seed >> 13);
        int mixed = shifted ^ (shifted << 18);
        int result = mixed & 0x7fffffff;
        return (result, result);
    });
}

internal static partial class Generator {
    public static State<int, bool> NextBool => NextInt.Map(static i => (i % 2) == 0);
}

internal static partial class Generator {
    public static State<int, (int First, int Second)> PairOfInts =>
        from first in NextInt
        from second in NextInt
        select (first, second);

    public static State<int, Option<int>> OptionInt =>
        from some in NextBool
        from value in NextInt
        select some ? Some(value) : Option<int>.None;
}

internal static partial class Generator {
    public static State<int, Seq<int>> Empty => State.pure<int, Seq<int>>(Seq<int>());
}

internal static partial class Generator {
    public static State<int, Seq<int>> IntList =>
        from empty in NextBool
        from list in empty ? Empty : NonEmpty
        select list;

    public static State<int, Seq<int>> NonEmpty =>
        from head in NextInt
        from tail in IntList
        select head.Cons(tail);
}
