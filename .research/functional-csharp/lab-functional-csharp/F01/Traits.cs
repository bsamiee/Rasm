namespace Lab.F01;

internal static partial class Traits {
    public static K<F, int> Tripled<F>(K<F, int> values)
        where F : Functor<F> =>
        values.Map(static x => x * 3);

    public static Option<int> TripledOption() => Tripled(Some(2)).As(); // Some(6)

    public static Seq<int> TripledSeq() => Tripled(Seq(1, 2, 3)).As(); // 3, 6, 9
}

internal static partial class Traits {
    public static K<M, int> Doubled<M>(K<M, int> values)
        where M : Monad<M> =>
        values.Bind(static x => M.Pure(x * 2));

    public static K<F, int> Lifted<F>(int value)
        where F : Applicative<F> =>
        F.Pure(value);

    public static Option<int> Absent() => None;

    public static Option<int> Parsed(string text) => parseInt(text);

    public static Map<string, int> Ages() => Map(("Ada", 36));

    public static HashMap<string, int> HashedAges() => HashMap(("Ada", 36));

    public static Set<int> Digits() => Set(1, 2, 3);
}
