namespace Rasm.Rhino;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class Disposal<T>(T held, Action<T> release) : IDisposable where T : notnull {
    private Tuple<T>? holding = new(held);

    public Option<T> Held => Optional(Volatile.Read(ref holding)).Map(static current => current.Item1);

    public void Dispose() => Optional(Interlocked.Exchange(ref holding, value: null)).Iter(taken => release(taken.Item1));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class DisposalOps {
    // --- [ACQUISITION]
    public static IO<Seq<T>> AcquireAll<T>(Seq<IO<T>> acquire, Func<Seq<T>, IO<Unit>> release) =>
        acquire.FoldBackM(Seq<T>(), (held, next) => OnFailure(next, release(held)).Map(held.Add)).As();

    public static IO<T> OnFailure<T>(IO<T> effect, IO<Unit> release) =>
        effect.IfFail(error => release.MapFail(fault => error + fault).Bind(_ => IO.fail<T>(error)));

    // --- [RELEASE]
    public static IO<Unit> Release<T>(Seq<T> held) where T : IDisposable =>
        Callbacks.Each(held.Rev().Map(static item => IO.lift(item.Dispose))).Map(static _ => unit);

    public static IDisposable Composite(Seq<IDisposable> held, CallbackSite site) =>
        new Disposal<Seq<IDisposable>>(held, items => Callbacks.Succeeded(Release(items), site));

    public static bool Present<T>(T? owned) where T : class, IDisposable => Optional(owned).Do(static copy => copy.Dispose()).IsSome;
}
