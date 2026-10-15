namespace Rasm.Rhino;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class Disposal<T>(T held, Action<T> release) : IDisposable {
    private Tuple<T>? holding = new(held);

    public Option<T> Held => Optional(Volatile.Read(ref holding)).Bind(static current => Optional(current.Item1));

    public void Dispose() => Optional(Interlocked.Exchange(ref holding, value: null)).Iter(taken => release(taken.Item1));

    public void Relinquish() => _ = Interlocked.Exchange(ref holding, value: null);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class DisposalOps {
    // --- [ACQUISITION]
    public static IO<T> Acquire<T>(IO<T> acquire, Func<T, IO<Unit>> release, Action<T> retain) =>
        (from held in acquire
         from retained in OnFailure(IO.lift(() => retain(held)), IO.pure(held).Bind(release))
         select held).Uninterruptible();

    public static IO<TResult> Acquire<TResult>(Func<Action<IDisposable>, IO<TResult>> body, Action<IDisposable> retain) =>
        from environment in envIO
        from result in (from held in Journal<IDisposable>(Release)
                        from retained in OnFailure(IO.lift(() => retain(held.Release)), held.Close)
                        from value in OnFailure(IO.liftVAsync(() => IO.pure(held.Retain).Bind(body).RunAsync(environment)), held.Close)
                        select value).Uninterruptible()
        select result;

    public static IO<TResult> Acquire<TResult>(Func<Action<IDisposable>, IO<TResult>> body, Action<TResult, IDisposable> retain) =>
        from environment in envIO
        from result in (from held in Journal<IDisposable>(Release)
                        from value in OnFailure(IO.liftVAsync(() => IO.pure(held.Retain).Bind(body).RunAsync(environment)), held.Close)
                        from retained in OnFailure(IO.lift(() => retain(value, held.Release)), held.Close)
                        select value).Uninterruptible()
        select result;

    public static IO<TResult> Replace<TResult>(Func<Action<IDisposable>, IO<TResult>> body, Func<TResult, IDisposable, IO<Unit>> publish) =>
        from environment in envIO
        from result in (from held in Journal<IDisposable>(Release)
                        from value in OnFailure(IO.liftVAsync(() => IO.pure(held.Retain).Bind(body).RunAsync(environment)), held.Close)
                        from retire in OnFailure(IO.lift(() => publish(value, held.Release)), held.Close)
                        from retired in retire
                        select value).Uninterruptible()
        select result;

    public static IO<Seq<TResult>> AcquireAll<T, TResult>(Seq<Func<Action<T>, IO<TResult>>> acquire, Func<Seq<T>, IO<Unit>> release, Action<Seq<TResult>, IDisposable> retain) =>
        from environment in envIO
        from result in (from held in Journal(release)
                        let acquisitions = acquire.TraverseM(factory => IO.pure(held.Retain).Bind(factory)).As()
                        from values in OnFailure(IO.liftVAsync(() => acquisitions.RunAsync(environment)), held.Close)
                        from retained in OnFailure(IO.lift(() => retain(values, held.Release)), held.Close)
                        select values).Uninterruptible()
        select result;

    public static IO<TResult> Within<T, TResult>(Func<T> acquire, Func<T, IO<TResult>> body, Func<T, IO<Unit>> release) =>
        from environment in envIO
        from result in (from held in IO.lift(acquire)
                        let cleanup = IO.pure(held).Bind(release)
                        from value in OnFailure(IO.liftVAsync(() => IO.pure(held).Bind(body).RunAsync(environment)), cleanup)
                        from released in cleanup
                        select value).Uninterruptible()
        select result;

    public static IO<TResult> Within<T, TResult>(Func<Action<T>, IO<TResult>> body, Func<Seq<T>, IO<Unit>> release) =>
        from environment in envIO
        from result in (from held in Journal(release)
                        from value in OnFailure(IO.liftVAsync(() => IO.pure(held.Retain).Bind(body).RunAsync(environment)), held.Close)
                        from released in held.Close
                        select value).Uninterruptible()
        select result;

    public static IO<ForkIO<TResult>> Fork<TResult>(IO<TResult> body, Action<IDisposable> retain) =>
        from environment in envIO
        from result in (from fork in body.Fork()
                        from retained in OnFailure(Attach(fork, environment, retain), fork.Cancel)
                        select fork).Uninterruptible()
        select result;

    public static IO<T> OnFailure<T>(IO<T> effect, IO<Unit> release) =>
        envIO
            .Bind(environment => IO.liftVAsync(() => effect.RunAsync(environment))
                .IfFail(error => release.MapFail(fault => error + fault).Bind(_ => IO.fail<T>(error)))
                .Uninterruptible());

    public static IO<T> OnFailure<T>(IO<T> effect, Func<Error, IO<Unit>> release) =>
        envIO
            .Bind(environment => IO.liftVAsync(() => effect.RunAsync(environment))
                .IfFail(error => IO.pure(error).Bind(release).MapFail(fault => error + fault).Bind(_ => IO.fail<T>(error)))
                .Uninterruptible());

    public static IO<Unit> Change<T>(Func<T> read, Action<T> write, T value, Action<IDisposable> retain) =>
        (from prior in IO.lift(read)
         from lease in IO.lift(() => new Disposal<T>(prior, write))
         from changed in OnFailure(IO.lift(() => { write(value); retain(lease); }), IO.lift(lease.Dispose))
         select changed).Uninterruptible();

    private static IO<(Action<T> Retain, IO<Unit> Close, IDisposable Release)> Journal<T>(Func<Seq<T>, IO<Unit>> release) =>
        IO.lift(static () => Ref(Some(Seq<T>()))).Map(held => {
            Action<T> retain = value => {
                if (!atomic(() => {
                    Option<Seq<T>> prior = held.Value;
                    held.Value = prior.Map(items => items.Add(value));
                    return prior.IsSome;
                })) throw Errors.Cancelled.ToException();
            };
            IO<Unit> close = atomicIO(() => {
                Option<Seq<T>> prior = held.Value;
                held.Value = None;
                return prior;
            }).Bind(prior => prior.Match(Some: release, None: static () => IO.pure(unit))).Uninterruptible();
            IDisposable lease = new Disposal<IO<Unit>>(close, static effect => effect.Run());
            return (retain, close, lease);
        });

    private static IO<Unit> Attach<TResult>(ForkIO<TResult> fork, EnvIO environment, Action<IDisposable> retain) =>
        from registration in IO.lift(() => environment.Token.Register(() => fork.Cancel.Run()))
        from attached in OnFailure(IO.lift(() => retain(Composite(Seq<IDisposable>(registration,
            new Disposal<ForkIO<TResult>>(fork, static held => held.Cancel.Run()))))), IO.lift(registration.Dispose))
        select attached;

    // --- [RELEASE]
    public static IO<Unit> Release<T>(Seq<T> held) where T : IDisposable =>
        Callbacks.Each(held.Rev().Map(static item => IO.lift(item.Dispose))).Map(unit).Uninterruptible();

    public static IDisposable Composite(Seq<IDisposable> held) =>
        new Disposal<Seq<IDisposable>>(held, static items => Release(items).Run());

    public static bool Present<T>(T? owned) where T : class, IDisposable => Optional(owned).Do(static copy => copy.Dispose()).IsSome;
}
