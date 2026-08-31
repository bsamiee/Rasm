namespace Lab.F15;

internal static partial class Composition {
    public static IO<int> Count(Atom<int> reads) => IO.lift(() => reads.Swap(static n => n + 1));

    public static IO<int> Doubled(Atom<int> reads) => Count(reads).Map(static n => n * 2);
}

internal static partial class Composition {
    public static IO<int> Summed(Atom<int> reads) => Count(reads).Bind(first => Count(reads).Map(second => first + second));
}
