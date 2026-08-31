namespace Lab.F06;

internal sealed class Connection : IDisposable {
    public static int Released { get; private set; }

    public bool Disposed { get; private set; }

    public int Query() => Disposed ? 0 : 42;

    public void Dispose() {
        Disposed = true;
        Released++;
    }
}

internal static class Lifecycles {
    public static IO<int> Scoped =>
        from connection in use(static () => new Connection())
        select connection.Query();

    public static IO<int> Bracketed =>
        IO.lift(static () => new Connection()).Bracket(Use: static c => IO.pure(c.Query()), Fin: static c => IO.lift(fun(c.Dispose)));
}
