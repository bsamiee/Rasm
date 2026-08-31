namespace Lab.F21;

internal static class Guards {
    public static Fin<int> Bounded(int value) =>
        from v in Pure(value).ToFin()
        from _ in guard<Error>(v >= 0, new InvalidAge())
        from __ in when(v > 100, Reject(new TooLarge()))
        select v;

    public static IO<int> Metered(IO<int> read) =>
        from v in read
        from _ in unless(v <= 100, IO.fail<Unit>(new TooLarge()))
        select v;

    private static Fin<Unit> Reject(Error error) => error;
}
