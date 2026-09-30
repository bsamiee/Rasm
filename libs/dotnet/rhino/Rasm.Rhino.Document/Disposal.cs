namespace Rasm.Rhino.Document;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class Disposal(Action dispose) : IDisposable {
    private int disposed;

    public void Dispose() {
        if (Interlocked.Exchange(ref disposed, 1) == 0)
            dispose();
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class DisposalOps {
    // --- [SCOPES]
    public static IO<TValue> Using<T, TValue>(Func<T> acquire, Func<T, IO<TValue>> body) where T : IDisposable =>
        Using(IO.lift(acquire), body);

    public static IO<TValue> Using<T, TValue>(IO<T> acquire, Func<T, IO<TValue>> body) where T : IDisposable =>
        acquire.Bracket(Use: body, Fin: static held => IO.lift(held.Dispose));

    public static IO<TValue> Using<T, TValue>(IO<Seq<T>> acquire, Func<Seq<T>, IO<TValue>> body) where T : IDisposable =>
        acquire.Bracket(Use: body, Fin: Release);

    public static IO<TValue> Using<TLeft, TRight, TValue>(IO<(Seq<TLeft> Left, Seq<TRight> Right)> acquire, string member, Func<Seq<(TLeft Left, TRight Right)>, IO<TValue>> body)
        where TLeft : IDisposable where TRight : IDisposable =>
        acquire.Bracket(
            Use: held => IO.lift(() => CountMismatch.Unless(held.Left.Count, held.Right.Count, member)).Bind(_ => body(held.Left.Zip(held.Right))),
            Fin: static held => Release<IDisposable>([.. held.Left, .. held.Right]));

    // --- [RELEASE]
    public static IO<Seq<T>> AcquireAll<T>(Seq<IO<T>> acquire, Func<Seq<T>, IO<Unit>> release) =>
        acquire.Fold(
            IO.pure(Seq<T>()),
            (held, next) =>
                from earlier in held
                from acquired in OnFailure(next, release(earlier))
                select earlier.Add(acquired));

    public static IO<Seq<T>> AcquireAll<T>(Seq<IO<T>> acquire) where T : IDisposable =>
        AcquireAll(acquire, Release);

    public static IO<Unit> Release<T>(Seq<T> held) where T : IDisposable =>
        held.Rev()
            .Map<K<IO, Unit>>(static item => IO.lift(item.Dispose))
            .Fails()
            .As()
            .Bind(static fails => unless(fails.IsEmpty, IO.fail<Unit>(Error.Many(fails))).As());

    public static bool Present<T>(T? owned) where T : class, IDisposable =>
        Optional(owned).Do(static held => held.Dispose()).IsSome;

    public static IO<T> OnFailure<T>(IO<T> effect, IO<Unit> release) =>
        effect.IfFail(error => release.MapFail(fault => error + fault).Bind(_ => IO.fail<T>(error)));
}
