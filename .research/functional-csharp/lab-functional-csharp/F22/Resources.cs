namespace Lab.F22;

internal sealed class Connection : IDisposable {
    public static int Released { get; private set; }

    public bool Disposed { get; private set; }

    public int Query() => Disposed ? 0 : 42;

    public void Dispose() {
        Disposed = true;
        Released++;
    }
}

internal static class Resources {
    public static IO<int> Disposed =>
        from connection in use(static () => new Connection())
        select connection.Query();

    public static IO<int> Released =>
        from connection in use(static () => new Connection(), static c => c.Dispose())
        select connection.Query();

    public static IO<int> Bracketed =>
        IO.lift(static () => new Connection()).Bracket(
            Use: static c => IO.pure(c.Query()),
            Fin: static c => IO.lift(fun(c.Dispose)));

    public static IO<int> Audited(Atom<int> closed) =>
        Construction.FoldedFail.Finally(IO.lift(() => closed.Swap(static n => n + 1)));
}
