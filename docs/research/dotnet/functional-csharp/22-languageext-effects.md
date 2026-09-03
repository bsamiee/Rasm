<!-- Fully integrated into dotnet-coding-languageext, [08] code in its streams reference -->
# [LANGUAGEEXT_EFFECTS]

`IO<A>` is the effect type. It describes a side effect with a failure channel and performs nothing until a host runs it. It is chosen at the input boundary and preserved through the domain. `RunSafe`, `Run`, `RunAsync`, and `Match` are host operations. Domain functions never run an effect.

<!-- Integrated into .claude/skills/dotnet-coding-languageext/SKILL.md
## [01]-[CONSTRUCTION]

`IO.lift` takes a thunk and defers it. Overload resolution reads the return type of the thunk. `Func<Fin<A>>` selects its overload and converts a `Fail` to an `IO` failure. The type argument in `IO.lift<Fin<A>>` keeps the `Fin` as the value. `IO.lift(Fin<A>)` lifts an existing result. `IO.liftAsync` takes a `Task` thunk, and the `EnvIO` overload passes `env.Token` to the dependency. `IO.pure` lifts a value and `IO.fail` builds a failed effect from an `Error`. LINQ over `IO` binds dependent steps.

```csharp
internal sealed record Unavailable() : Expected("service unavailable", 2201);

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
```
-->

<!-- Integrated into .claude/skills/dotnet-coding-languageext/SKILL.md
## [02]-[HOST_EXECUTION]

`Run` and `RunAsync` throw on failure and belong to `Main`. They represent an `Expected` error as an `ErrorException` and rethrow the exception captured by an `Exceptional` error. `RunSafe` returns `Fin<A>` for translation at the host boundary. `Try.lift(io.Run).Run()` captures the thrown error and returns the original `Expected`. `EnvIO.New` carries the cancellation token. Cancelled tokens escape `RunSafe` as exceptions. Hosts supplying an `EnvIO` capture the exception with `Try.lift`. `Catch(code, f)` recovers one error code and `|` supplies an alternative effect.

```csharp
internal static class Exits {
    public static Fin<int> Safe => Construction.Failed.RunSafe();
    public static Fin<int> Thrown => Try.lift(Construction.Failed.Run).Run();
    public static Fin<int> Recovered => Construction.Failed.Catch(2201, static _ => IO.pure(9)).As().RunSafe();
    public static Fin<int> Alternative => (Construction.Failed | IO.pure(8)).RunSafe();
    public static Fin<int> Cancelled() {
        using EnvIO env = EnvIO.New(token: new CancellationToken(canceled: true));
        return Try.lift(() => Construction.TokenAware.Run(env)).Run();
    }
}
```
-->

<!-- Integrated into .claude/skills/dotnet-coding-languageext/SKILL.md
## [03]-[RESOURCES]

`use` acquires an `IDisposable` inside the effect and disposes it when the scope ends, on success and on failure. The `use` overload with a release action names the release step for the acquired value. `Bracket(Use:, Fin:)` runs `Fin` after `Use` on both paths. `Finally` attaches an effect that runs after the receiver. If `Finally` is applied to an existing `IO.fail`, the finalizer does not run. The finalizer runs when a deferred effect fails during execution.

```csharp
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
```
-->

<!-- Integrated into .claude/skills/dotnet-coding-languageext/SKILL.md
## [04]-[CONCURRENCY]

`Fork` starts the effect on one `TaskCreationOptions.LongRunning` thread and returns a `ForkIO` with `Await` and `Cancel`. `awaitAll` runs every effect of a `Seq` and collects the values. `awaitAny` returns the first value. `timeout` fails the effect after the duration. `Uninterruptible` masks cancellation for the effect.

`Traverse` under `IO` starts every element effect before awaiting any: effects built with `IO.liftAsync` run concurrently without a concurrency limit, and effects built with `IO.lift` run in order on the calling thread. `TraverseM` runs the effects one after another. For a large fan-out, chunk the collection first.

```csharp
internal static class Concurrency {
    public static IO<int> Forked =>
        from left in IO.pure(1).Fork()
        from right in IO.pure(2).Fork()
        from a in left.Await
        from b in right.Await
        select a + b;
    public static IO<Seq<int>> All(Seq<IO<int>> jobs) => awaitAll(jobs);
    public static IO<int> First(Seq<IO<int>> jobs) => awaitAny(jobs);
    public static IO<int> Deadline(IO<int> job) => timeout(TimeSpan.FromSeconds(1), job);
    public static IO<int> Masked => IO.pure(3).Uninterruptible();
    public static IO<Seq<int>> Chunked(Seq<int> items, int width, Func<int, IO<int>> work) =>
        toSeq(items.Chunk(width))
            .TraverseM(chunk => toSeq(chunk).Traverse(work).As())
            .As()
            .Map(static chunks => chunks.Flatten());
    public static IO<int> Drained(Buffer<int> buffer, Seq<int> items) {
        Conduit<int, int> queue = Conduit.make(buffer);
        return
            from running in queue.Reduce(0, static (total, item) => Reduced.ContinueIO(total + item)).Fork()
            from _ in items.TraverseM(queue.Post).As()
            from __ in queue.Complete()
            from total in running.Await
            select total;
    }
}
```
-->

<!-- Integrated into .claude/skills/dotnet-coding-languageext/SKILL.md
## [05]-[RECURSION]

`tail` marks the last bind continuation after a deferred effect and uses constant stack space. `tail`-recursive `IO` exits through `Run()` or `RunAsync()` only. `RunSafe()`, `Try()`, `Map`, and a later `Bind` are incompatible with this form and cause it to fail. `Monad.recur` loops a state with `Next.Loop` and `Next.Done` and can use any host operation. `Next.Done` can return any result type. `RepeatUntil` polls one effect until its value satisfies the predicate, and `RepeatWhile` polls while the value satisfies it.

```csharp
internal static class Recursion {
    public static IO<int> CountTo(int current, int limit) =>
        current >= limit ? IO.pure(current) : IO.lift(() => current + 1).Bind(next => tail(CountTo(next, limit)));
    public static IO<int> Recur(int limit) =>
        Monad.recur<IO, int, int>(0, i => i >= limit ? IO.pure(Next.Done<int, int>(i)) : IO.pure(Next.Loop<int, int>(i + 1))).As();
    public static IO<int> Poll(Atom<int> polls) =>
        IO.lift(() => polls.Swap(static n => n + 1)).RepeatUntil(static n => n >= 3);
    public static IO<int> Drain(Atom<int> pending) =>
        IO.lift(() => pending.Swap(static n => n - 1)).RepeatWhile(static n => n > 0);
}
```
-->

<!-- Integrated into .claude/skills/dotnet-coding-languageext/SKILL.md
## [06]-[SCHEDULES]

`Schedule.spaced` and `Schedule.exponential` build delay sequences. `recurs` caps the attempt count, `repeat` replays the whole schedule, `jitter` randomizes each delay, and `maxDelay` caps each delay. `recurs`, `repeat`, `jitter`, and `maxDelay` are `ScheduleTransformer` values. Between two schedules `|` is a union that takes the shorter delay and `&` is an intersection that takes the longer delay. Where one side is a transformer, `|` and `&` both apply the transformer. Transformers passed alone convert to `Schedule.Forever` with the transformer applied.

`Retry(Schedule)` reruns a deferred effect that failed and `Repeat(Schedule)` reruns a successful one. `Repeat(Schedule.recurs(2))` runs the effect once and then twice more.

```csharp
internal static class Schedules {
    public static Schedule Policy => Schedule.spaced(TimeSpan.FromMilliseconds(1)) | Schedule.recurs(5);
    public static Schedule Backoff => Schedule.exponential(TimeSpan.FromMilliseconds(1)) | Schedule.recurs(3) | Schedule.jitter();
    public static Schedule Capped => Schedule.exponential(TimeSpan.FromMilliseconds(1)) & Schedule.maxDelay(TimeSpan.FromMilliseconds(4));
    public static Schedule Replayed => Schedule.spaced(TimeSpan.FromMilliseconds(1)) | Schedule.recurs(2) | Schedule.repeat(2);
    public static Schedule Union => Schedule.spaced(TimeSpan.FromMilliseconds(1)) | Schedule.spaced(TimeSpan.FromMilliseconds(3));
    public static Schedule Intersection => Schedule.spaced(TimeSpan.FromMilliseconds(1)) & Schedule.spaced(TimeSpan.FromMilliseconds(3));
    public static IO<int> Retried(Atom<int> attempts) =>
        IO.lift(() => attempts.Swap(static n => n + 1))
            .Bind(static n => n < 3 ? IO.fail<int>(new Unavailable()) : IO.pure(7))
            .Retry(Policy);
    public static IO<int> Repeated(Atom<int> ticks) =>
        IO.lift(() => ticks.Swap(static n => n + 1)).Repeat(Schedule.recurs(2));
}
```
-->

<!-- Integrated into .claude/skills/dotnet-coding-languageext/SKILL.md
## [07]-[RUNTIMES]

`Eff<RT, A>` reads a capability from a runtime. Runtime records implement `Has<Eff<RT>, ConsoleIO>` with `Eff.runtime<RT>().Map(static rt => rt.Console)`. `Console<RT>.writeLine` and `Console<RT>.readLine` compile against that constraint, and `File<RT>` needs `FileIO` and `EncodingIO`. `Run(rt)` returns `Fin<A>`. `IO<A>` converts implicitly to `Eff<RT, A>`.

`LanguageExt.Sys.Test.Runtime.New()` supplies a `MemoryConsole` and a file system rooted at a temporary directory under `Env.RootPath`. The test runtime is disposable and deletes that directory. `WriteKeyLine` feeds console input, and enumeration of the console returns the written lines only. `LanguageExt.Sys.Live.Runtime.New()` supplies live host services.

```csharp
internal sealed record Runtime(ConsoleIO Console) : Has<Eff<Runtime>, ConsoleIO> {
    static K<Eff<Runtime>, ConsoleIO> Has<Eff<Runtime>, ConsoleIO>.Ask => Eff.runtime<Runtime>().Map(static rt => rt.Console);
}

internal static class Runtimes {
    public static Eff<RT, string> Greet<RT>(string prompt) where RT : Has<Eff<RT>, ConsoleIO> =>
        from _ in Console<RT>.writeLine(prompt)
        from line in Console<RT>.readLine
        select line;
    public static Eff<RT, string> RoundTrip<RT>(string path, string text) where RT : Has<Eff<RT>, FileIO>, Has<Eff<RT>, EncodingIO> =>
        from _ in File<RT>.writeAllText(path, text)
        from read in File<RT>.readAllText(path)
        select read;
    public static Eff<Runtime, int> Entered => Construction.Plain;
    public static Eff<Runtime, Runtime> Ask => Eff.runtime<Runtime>();
}
```
-->

<!-- Integrated into .claude/skills/dotnet-coding-languageext/SKILL.md
## [08]-[STREAMS]

`Source<A>` is a stream of values. `Source.lift` accepts an `IObservable<A>` or an `IEnumerable<A>`, `Source.merge` joins sources, and `Zip` pairs them. `Reduce(seed, f)` returns `IO<S>` and is the fold that yields a value. `Fold` on a lifted finite sequence emits nothing. `ReduceIO` stops with `Reduced.DoneIO` and continues with `Reduced.ContinueIO`. `Sink<A>` receives with `Post`, adapts its input with `Comap`, and rejects `Post` after `Complete()`.

`Conduit.make` takes a `Buffer` policy. `Unbounded` keeps every item, `Bounded(n)` and `Single` make `Post` wait, `Newest(n)` keeps the last n items, and `Latest(seed)` starts from a seed and keeps the last item. Conduits act as message queues when `Reduce` runs under `Fork()` while a client posts. `Source.Take(1).Last()` reads a reply from a second conduit. `ProducerT`, `PipeT`, and `ConsumerT` compose with `|` into an `EffectT`, and its `Run()` returns the underlying `K<IO, A>`.
-->

<!-- Integrated into .claude/skills/dotnet-coding-languageext/references/streams.md
```csharp
internal sealed class Replay<A>(Seq<A> items) : IObservable<A> {
    public IDisposable Subscribe(IObserver<A> observer) {
        _ = items.Iter(observer.OnNext);
        observer.OnCompleted();
        return new Subscription();
    }

    private sealed class Subscription : IDisposable {
        public void Dispose() {
        }
    }
}

internal static class Streams {
    public static Source<int> Observed => Source.lift(new Replay<int>(Seq(1, 2, 3)));
    public static Source<int> Merged => Source.merge(Source.lift(Seq(1, 2)), Source.lift(Seq(3)));
    public static Source<(int First, string Second)> Zipped => Source.lift(Seq(1, 2)).Zip(Source.lift(Seq("a", "b")));
    public static IO<int> Sum(Source<int> source) => source.Reduce(0, static (total, item) => total + item);
    public static IO<int> UntilTwo(Source<int> source) =>
        source.ReduceIO(0, static (total, item) => item == 2 ? Reduced.DoneIO(total + item) : Reduced.ContinueIO(total + item));
    public static IO<Unit> PostLength(Sink<int> sink, string text) => sink.Comap(static (string s) => s.Length).Post(text);
    public static IO<Unit> Closed(Sink<int> sink) =>
        from _ in sink.Complete()
        from __ in sink.Post(1)
        select unit;
    public static IO<int> Retained(Buffer<int> buffer, Seq<int> items) {
        Conduit<int, int> queue = Conduit.make(buffer);
        return
            from _ in items.TraverseM(queue.Post).As()
            from __ in queue.Complete()
            from total in queue.Reduce(0, static (sum, item) => Reduced.ContinueIO(sum + item))
            select total;
    }
    public static IO<(int Reply, int Final)> Session(Conduit<int, int> inbox, Conduit<int, int> replies) =>
        from running in inbox.Reduce(0, (state, message) => replies.Post(state + message).Map(_ => Reduced.Continue(state + message))).Fork()
        from _ in inbox.Post(5)
        from reply in replies.Source.Take(1).Last()
        from __ in inbox.Complete()
        from final in running.Await
        select (reply, final);
    public static ProducerT<int, IO, Unit> Numbers => ProducerT.yieldAll<IO, int>(Seq(1, 2, 3));
    public static PipeT<int, int, IO, Unit> Doubled => PipeT.map<IO, int, int>(static x => x * 2);
    public static ConsumerT<int, IO, Unit> Accumulate(Atom<int> total) =>
        ConsumerT.repeat(ConsumerT.awaiting<IO, int>().Bind(x => IO.lift(() => ignore(total.Swap(n => n + x)))));
    public static IO<int> Pipeline(Atom<int> total) =>
        (Numbers | Doubled | Accumulate(total)).Run().As().Map(_ => total.Value);
}
```
-->
