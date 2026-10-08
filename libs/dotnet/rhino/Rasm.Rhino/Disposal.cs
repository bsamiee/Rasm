namespace Rasm.Rhino;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class Disposal<T>(T held, Action<T> release) : IDisposable where T : notnull {
    private Holding? holding = new(held, release);

    public Option<T> Held => Optional(Volatile.Read(ref holding)).Map(static current => current.Value);

    public void Dispose() => Optional(Interlocked.Exchange(ref holding, value: null)).Iter(static taken => taken.Release(taken.Value));

    private sealed record Holding(T Value, Action<T> Release);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class DisposalOps {
    // --- [ACQUISITION]
    public static IO<Seq<T>> AcquireAll<T>(Seq<IO<T>> acquire, Func<Seq<T>, IO<Unit>> release) =>
        acquire.FoldBackM(Seq<T>(), (held, next) => OnFailure(next, release(held)).Map(held.Add)).As();

    public static IO<Seq<T>> AcquireAll<T>(Seq<IO<T>> acquire) where T : IDisposable => AcquireAll(acquire, Release);

    public static IO<T> OnFailure<T>(IO<T> effect, IO<Unit> release) =>
        IO.pure(unit).Bind(_ => effect).Catch(error => release.MapFail(fault => error + fault).Bind(_ => IO.fail<T>(error))).As();

    // --- [RELEASE]
    public static IO<Unit> Release<T>(Seq<T> held) where T : IDisposable =>
        Callbacks.Each(held.Rev().Map(static item => IO.lift(item.Dispose)));

    public static Disposal<Seq<IDisposable>> Composite(Seq<IDisposable> held, IPlugInSink sink) =>
        new(held, items => Callbacks.Answer(Release(items), static () => unit, new CallbackSite(sink, typeof(DisposalOps), nameof(Composite))));

    public static bool Present<T>(T? owned) where T : class, IDisposable => Optional(owned).Do(static copy => copy.Dispose()).IsSome;
}
