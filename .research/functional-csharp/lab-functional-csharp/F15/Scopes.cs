namespace Lab.F15;

internal sealed record Rejected() : Expected("statement rejected", 1501);

internal sealed class Connection : IDisposable {
    public static int Released { get; private set; }

    public bool Disposed { get; private set; }

    public Transaction BeginTransaction() => new(this);

    public int Execute(string statement) => Disposed ? 0 : statement.Length;

    public int Execute(string statement, Transaction transaction) => transaction.Open ? Execute(statement) : 0;

    public void Dispose() {
        Disposed = true;
        Released++;
    }
}

internal sealed class Transaction(Connection connection) : IDisposable {
    public static int Committed { get; private set; }

    public static int RolledBack { get; private set; }

    public bool Open { get; private set; } = !connection.Disposed;

    public void Commit() {
        Open = false;
        Committed++;
    }

    public void Dispose() {
        if (Open) RolledBack++;
        Open = false;
    }
}

internal static partial class Scopes {
    public static IO<A> Time<A>(Atom<TimeSpan> elapsed, IO<A> work) =>
        IO.lift(System.Diagnostics.Stopwatch.StartNew).Bracket(
            Use: _ => work,
            Fin: watch => IO.lift(() => elapsed.Swap(_ => watch.Elapsed)));
}

internal static partial class Scopes {
    public static IO<int> DeleteOldLogs(Atom<TimeSpan> elapsed) =>
        Time(
            elapsed,
            from connection in use(static () => new Connection())
            select connection.Execute("DELETE Logs WHERE Timestamp < @upTo"));

    public static IO<int> DeleteOrder =>
        from connection in use(static () => new Connection())
        from transaction in use(connection.BeginTransaction)
        from affected in IO.lift(() =>
            connection.Execute("DELETE OrderLines WHERE OrderId = @Id", transaction)
            + connection.Execute("DELETE Orders WHERE OrderId = @Id", transaction))
        from _ in IO.lift(fun(transaction.Commit))
        select affected;
}

internal static partial class Scopes {
    public static IO<int> DeleteRejected =>
        from connection in use(static () => new Connection())
        from transaction in use(connection.BeginTransaction)
        from affected in IO.fail<int>(new Rejected())
        from _ in IO.lift(fun(transaction.Commit))
        select affected;
}
