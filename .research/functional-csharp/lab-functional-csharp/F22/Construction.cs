namespace Lab.F22;

internal sealed record Unavailable() : Expected("service unavailable", 503);

internal static class Remote {
    public static async Task<int> FetchAsync(int id, CancellationToken token) {
        await Task.Yield();
        token.ThrowIfCancellationRequested();
        return id * 10;
    }
}

internal static class Construction {
    public static IO<int> Plain => IO.lift(static () => 1);

    public static IO<int> Folded => IO.lift(static () => Pure(2).ToFin());

    public static IO<int> FoldedFail => IO.lift(Reject);

    public static IO<bool> Carried => IO.lift<Fin<int>>(static () => Pure(3)).Map(static fin => fin.IsSucc);

    public static IO<int> Evaluated => IO.lift(Pure(4).ToFin());

    public static IO<int> Fetched => IO.liftAsync(static () => Remote.FetchAsync(5, CancellationToken.None));

    public static IO<int> TokenAware => IO.liftAsync(static env => Remote.FetchAsync(6, env.Token));

    public static IO<int> Failed => IO.fail<int>(new Unavailable());

    public static IO<int> Total =>
        from a in Plain
        from b in Folded
        from c in Evaluated
        from d in IO.pure(10)
        select a + b + c + d;

    private static Fin<int> Reject() => new Unavailable();
}
