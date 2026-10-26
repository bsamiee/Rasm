namespace Rasm.Rhino;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class Disposal<T>(T held, Action<T> release) : IDisposable {
    private Tuple<T>? holding = new(held);

    public Option<T> Held => Optional(Volatile.Read(ref holding)).Bind(static current => Optional(current.Item1));

    public void Dispose() => Optional(Interlocked.Exchange(ref holding, value: null)).Iter(taken => release(taken.Item1));

    public void Relinquish() => _ = Interlocked.Exchange(ref holding, value: null);
}

public sealed class Retained<T>(Func<Seq<T>, IO<Unit>> release) : IDisposable {
    private readonly Ref<Option<Seq<T>>> held = Ref(Some(Seq<T>()));

    public Fin<Seq<T>> Held => held.Value.ToFin(Errors.Cancelled);

    public IO<Unit> Drain => Exchanged(static prior => prior.Map(static _ => Seq<T>()), Released);

    public IO<Unit> Close => Exchanged(static _ => None, Released);

    public void Retain(T value) {
        if (atomic(() => held.Swap(items => items.Map(retained => retained.Add(value)))).IsNone) throw Errors.Cancelled.ToException();
    }

    public IO<Unit> Replace(T value) =>
        Exchanged(prior => prior.Map(_ => Seq(value)), prior => prior.Match(Some: release, None: () => Refused(value)));

    public IO<Unit> Admit(T value) =>
        Exchanged(prior => prior.Map(retained => retained.IsEmpty ? Seq(value) : retained), prior => prior.Match(
            Some: retained => unless(retained.IsEmpty, release(Seq(value))).As(),
            None: () => Refused(value)));

    public void Dispose() => Close.Run();

    private IO<Unit> Released(Option<Seq<T>> prior) => prior.Match(Some: release, None: static () => IO.pure(unit));

    private IO<Unit> Refused(T value) => release(Seq(value)).Bind(static _ => IO.fail<Unit>(Errors.Cancelled));

    private IO<Unit> Exchanged(Func<Option<Seq<T>>, Option<Seq<T>>> next, Func<Option<Seq<T>>, IO<Unit>> settle) =>
        atomicIO(() => {
            Option<Seq<T>> prior = held.Value;
            held.Value = next(prior);
            return prior;
        }).Bind(settle).Uninterruptible();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class DisposalOps {
    // --- [ACQUISITION]
    public static IO<TResult> Transacted<TOpen, TValue, TResult>(IO<TOpen> open, Func<TOpen, IO<TValue>> body, Func<TOpen, IO<Unit>> rollback, Func<TOpen, TValue, IO<TResult>> commit) =>
        from environment in envIO
        from result in (from held in open
                        from value in OnFailure(IO.liftVAsync(() => IO.pure(held).Bind(body).RunAsync(environment)), IO.pure(held).Bind(rollback))
                        from committed in commit(held, value)
                        select committed).Uninterruptible()
        select result;

    public static IO<T> Acquire<T, TLease>(IO<T> acquire, Func<T, TLease> lease, Action<TLease> retain) where TLease : IDisposable =>
        (from held in acquire
         let owned = lease(held)
         from retained in OnFailure(IO.lift(() => retain(owned)), IO.lift(owned.Dispose))
         select held).Uninterruptible();

    public static IO<T> Acquire<T>(IO<T> acquire, Func<T, IO<Unit>> release, Action<IDisposable> retain) =>
        Acquire(acquire, held => new Disposal<T>(held, value => release(value).Run()), retain);

    public static IO<T> Acquire<T>(IO<T> acquire, Action<T> retain) where T : IDisposable =>
        Acquire(acquire, static held => held, retain);

    public static IO<TResult> Acquire<TResult>(Func<Action<IDisposable>, IO<TResult>> body, Action<IDisposable> retain) =>
        Transacted(
            from held in IO.lift(static () => new Retained<IDisposable>(Release))
            from retained in OnFailure(IO.lift(() => retain(held)), held.Close)
            select held,
            held => body(held.Retain),
            static held => held.Close,
            static (_, value) => IO.pure(value));

    public static IO<TResult> Acquire<TResult>(Func<Action<IDisposable>, IO<TResult>> body, Action<TResult, IDisposable> retain) =>
        Acquire(body, (value, lease) => IO.lift(() => retain(value, lease)));

    public static IO<TResult> Acquire<TResult>(Func<Action<IDisposable>, IO<TResult>> body, Func<TResult, IDisposable, IO<Unit>> publish) =>
        Transacted(
            IO.lift(static () => new Retained<IDisposable>(Release)),
            held => body(held.Retain),
            static held => held.Close,
            (held, value) => OnFailure(IO.lift(() => publish(value, held)).Bind(static published => published), held.Close).Map(_ => value));

    public static IO<TResult> Within<T, TResult>(Func<T> acquire, Func<T, IO<TResult>> body) where T : IDisposable =>
        Within(acquire, body, static held => IO.lift(held.Dispose));

    public static IO<TResult> Within<T, TResult>(Func<T> acquire, Func<T, IO<TResult>> body, Func<T, IO<Unit>> release) =>
        Within(IO.lift(acquire), body, release);

    public static IO<TResult> Within<T, TResult>(IO<T> acquire, Func<T, IO<TResult>> body, Func<T, IO<Unit>> release) =>
        Transacted(acquire, body, release, (held, value) => release(held).Map(_ => value));

    public static IO<TResult> Within<T, TResult>(Func<Action<T>, IO<TResult>> body, Func<Seq<T>, IO<Unit>> release) =>
        Transacted(IO.lift(() => new Retained<T>(release)), held => body(held.Retain), static held => held.Close, static (held, value) => held.Close.Map(_ => value));

    public static IO<TResult> Found<T, TResult>(Func<T?> acquire, string member, Func<T, IO<TResult>> body) where T : class, IDisposable =>
        Within(acquire, held => IO.lift(Refused.UnlessFound(held, member)).Bind(body), static held => Release(held));

    public static IO<ForkIO<TResult>> Fork<TResult>(IO<TResult> body, Action<IDisposable> retain) =>
        from environment in envIO
        from result in (from fork in body.Fork()
                        from retained in OnFailure(Acquire(IO.lift(() => environment.Token.Register(() => fork.Cancel.Run())),
                            registration => Callbacks.Each(Seq(fork.Cancel, IO.lift(registration.Dispose))).Map(unit), retain), fork.Cancel)
                        select fork).Uninterruptible()
        select result;

    public static IO<T> OnFailure<T>(IO<T> effect, IO<Unit> release) =>
        OnFailure(effect, _ => release);

    public static IO<T> OnFailure<T>(IO<T> effect, Func<Error, IO<Unit>> release) =>
        envIO
            .Bind(environment => IO.liftVAsync(() => effect.RunAsync(environment))
                .IfFail(error => IO.pure(error).Bind(release).MapFail(fault => error + fault).Bind(_ => IO.fail<T>(error)))
                .Uninterruptible());

    public static IO<Unit> Change<T>(Func<T> read, Action<T> write, T value, Action<IDisposable> retain) =>
        IO.lift(read).Bind(prior => Acquire(OnFailure(IO.lift(() => write(value)), IO.lift(() => write(prior))), _ => IO.lift(() => write(prior)), retain));

    public static IO<Unit> Published<TMember>(Retained<IDisposable> owned, TMember value, IDisposable lease, Func<TMember> read, Action<TMember> write) =>
        from prior in IO.lift(read)
        from written in OnFailure(IO.lift(() => write(value)), IO.lift(() => write(prior)).Bind(_ => Release(lease)))
        from replaced in owned.Replace(lease)
        select replaced;

    // --- [RELEASE]
    public static IO<Unit> Release<T>(params Seq<T> held) where T : IDisposable? =>
        Callbacks.Each(held.Rev().Bind(static item => item is { } present ? Seq(IO.lift(present.Dispose)) : [])).Map(unit).Uninterruptible();

    public static bool Present<T>(T? owned) where T : class, IDisposable => Optional(owned).Do(static copy => copy.Dispose()).IsSome;
}
