namespace Rasm.Rhino.Document;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class Disposal(Action dispose) : IDisposable {
    // --- [LIFECYCLE]
    private int disposed;

    public bool IsDisposed => Volatile.Read(ref disposed) != 0;

    public void Dispose() {
        if (Interlocked.Exchange(ref disposed, 1) == 0)
            dispose();
    }

    // --- [SCOPES]
    public static IO<TValue> Bracketed<T, TValue>(IO<T> acquire, Func<T, IO<Unit>> release, Func<T, IO<TValue>> body) =>
        use(acquire, release).Bind(body).Bracket();

    public static IO<TValue> Bracketed<TValue>(Action begin, Action end, IO<TValue> body) =>
        Bracketed(IO.lift(begin), _ => IO.lift(end), _ => body);

    public static IO<TValue> Using<T, TValue>(Func<T> acquire, Func<T, IO<TValue>> body) where T : IDisposable =>
        Using(IO.lift(acquire), body);

    public static IO<TValue> Using<T, TValue>(IO<T> acquire, Func<T, IO<TValue>> body) where T : IDisposable =>
        Bracketed(acquire, static held => IO.lift(held.Dispose), body);

    public static IO<TValue> Using<T, TValue>(IO<Seq<T>> acquire, Func<Seq<T>, IO<TValue>> body) where T : IDisposable =>
        Bracketed(acquire, Release, body);

    // --- [RELEASE]
    public static IO<Seq<T>> AcquireAll<T>(Seq<IO<T>> acquire) where T : IDisposable =>
        AcquireAll(acquire.Map(static next => next.Map(static acquired => Seq(acquired))));

    public static IO<Seq<T>> AcquireAll<T>(Seq<IO<Seq<T>>> acquire) where T : IDisposable =>
        acquire.Fold(
            IO.pure(Seq<T>()),
            static (held, next) =>
                from earlier in held
                from acquired in GeometryOps.OnFailure(next, Release(earlier))
                select earlier + acquired);

    public static IO<Unit> Release<T>(Seq<T> held) where T : IDisposable =>
        held.Rev()
            .Map<K<IO, Unit>>(static item => IO.lift(item.Dispose))
            .Fails()
            .As()
            .Bind(static fails => unless(fails.IsEmpty, IO.fail<Unit>(Error.Many(fails))).As());
}
