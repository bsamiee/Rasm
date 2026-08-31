namespace Lab.F18;

internal static class Independence {
    public static IO<Flight> Best(IO<Flight> first, IO<Flight> second) =>
        (first, second).Apply(PickCheaper).As();

    public static IO<Flight> BestForked(IO<Flight> first, IO<Flight> second) =>
        from f1 in first.Fork()
        from f2 in second.Fork()
        from a in f1.Await
        from b in f2.Await
        select PickCheaper(a, b);

    public static IO<Seq<Flight>> All(Seq<IO<Flight>> requests) =>
        awaitAll(requests);

    private static Flight PickCheaper(Flight a, Flight b) =>
        a.Price <= b.Price ? a : b;
}
