# [STREAMS]

## [01]-[SOURCES]

`Source.lift` accepts an `IObservable<A>` or an `IEnumerable<A>`, and an observable enters by implementing `Subscribe`, which receives the `IObserver<A>` the source supplies once, delivers `OnNext` per value, ends with `OnCompleted`, and returns the `IDisposable` that owns the subscription lifetime:

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

internal static class Sources {
    private static Action<string> onMessage = static _ => { };
    private static readonly Event<string> Messages = Event.from(ref onMessage);

    public static Source<int> Observed => Source.lift(new Replay<int>(Seq(1, 2, 3)));
    public static Source<int> Merged => Source.merge(Source.lift(Seq(1, 2)), Source.lift(Seq(3)));
    public static Source<(int First, string Second)> Zipped => Source.lift(Seq(1, 2)).Zip(Source.lift(Seq("a", "b")));
    public static IO<string> FirstMessage =>
        from messages in Messages.Subscribe()
        from _ in IO.lift(static () => onMessage("hello"))
        from head in messages.Take(1).Last()
        select head;
}
```

A lifted single value emits and completes at once, a lifted enumerable emits its elements and completes, `Reduce` subscribes a lifted observable, and subscription behavior depends on the source, so not every observable is lazy. `Event.from(ref Action<A>)` adds its callback to the delegate's invocation list, `Subscribe()` returns an `IO<Source<A>>` that receives every later invocation, and disposing the `Event` detaches the delegate. `Subject<T>` is both observer and observable for imperative code that calls `OnNext`, `OnError`, and `OnCompleted` at a callback boundary, and `Event.from` or a dedicated source keeps those calls out of the stream definition. `Combine` depends on completion, so a first source that never completes hides the second, and `Source.pure(value).Combine(source)` prefixes an initial value.

## [02]-[REDUCTION]

`Reduce(seed, f)` folds every value into one `IO<S>`, `ReduceIO` lets the reducer stop with `Reduced.DoneIO` or continue with `Reduced.ContinueIO`, and `Fold` on a lifted finite sequence emits nothing:

```csharp
internal static class Reductions {
    public static IO<int> Sum(Source<int> source) => source.Reduce(0, static (total, item) => total + item);
    public static IO<int> UntilLimit(Source<int> source, int limit) =>
        source.ReduceIO(0, (total, item) => item == limit ? Reduced.DoneIO(total + item) : Reduced.ContinueIO(total + item));
}
```

## [03]-[CONDUITS]

`Conduit.make(Buffer<A>)` builds a `Sink<A>` and `Source<A>` pair, `Post` writes to the sink, `Comap` adapts the sink's input type, `Complete()` closes it and a later `Post` fails, and the buffer policy decides what happens when production outpaces consumption, because the consumer cannot slow the producer by requesting the next item:

| [INDEX] | [BUFFER]              | [BEHAVIOR]                                                         |
| :-----: | :-------------------- | :----------------------------------------------------------------- |
|  [01]   | `Unbounded`           | Keeps every value                                                  |
|  [02]   | `Bounded(n)`, `Single` | Holds `n` values or one, and `Post` waits when full               |
|  [03]   | `Latest(seed)`        | Starts from the seed and keeps only the last value                 |
|  [04]   | `Newest(n)`           | Keeps the last `n` values                                          |

With an unbounded buffer the producer posts, completes, and the reduction reads afterward, while a bounded buffer needs the consumer forked before the producer posts, or `Post` blocks on the full buffer:

```csharp
internal static class Queues {
    public static IO<Seq<int>> Retained(Buffer<int> buffer, Seq<int> items) {
        Conduit<int, int> queue = Conduit.make(buffer);
        return
            from _ in items.TraverseM(queue.Post).As()
            from __ in queue.Complete()
            from kept in queue.Reduce(Seq<int>(), static (kept, item) => Reduced.ContinueIO(kept.Add(item)))
            select kept;
    }
    public static IO<Seq<int>> Drained(Buffer<int> buffer, Seq<int> items) {
        Conduit<int, int> queue = Conduit.make(buffer);
        return
            from running in queue.Reduce(Seq<int>(), static (kept, item) => Reduced.ContinueIO(kept.Add(item))).Fork()
            from _ in items.TraverseM(queue.Post).As()
            from __ in queue.Complete()
            from kept in running.Await
            select kept;
    }
    public static IO<Unit> PostLength(Sink<int> sink, string text) => sink.Comap(static (string s) => s.Length).Post(text);
}
```

Rx names the time-based and grouping-based policies `Sample`, `Throttle`, `Debounce`, `Buffer`, and `Window`, and the chosen policy states whether intermediate values are dropped, delayed, grouped, or preserved. A conduit reduced under `Fork()` while a client posts is a message queue, and a second conduit carries a reply that `Source.Take(1).Last()` reads.
- See `dotnet-coding/references/streams.md` for the agent loop, the reply pattern, and entity processes built on conduits

## [04]-[PIPES]

`ProducerT`, `PipeT`, and `ConsumerT` are the streaming roles with explicit queues and exact sequencing, `|` fuses them into one `EffectT`, and its `Run()` returns the underlying `K<IO, A>` for the host:

```csharp
internal static class Pipelines {
    public static ProducerT<int, IO, Unit> Numbers => ProducerT.yieldAll<IO, int>(Seq(1, 2, 3));
    public static PipeT<int, int, IO, Unit> Doubled => PipeT.map<IO, int, int>(static x => x * 2);
    public static ConsumerT<int, IO, Unit> Accumulate(Atom<int> total) =>
        ConsumerT.repeat(ConsumerT.awaiting<IO, int>().Bind(x => IO.lift(() => ignore(total.Swap(n => n + x)))));
    public static IO<int> Run(Atom<int> total) => (Numbers | Doubled | Accumulate(total)).Run().As().Map(_ => total.Value);
}
```

`ProducerT.yieldAll` emits a sequence, `PipeT.map` transforms each value in transit, `ConsumerT.awaiting` receives one value and `ConsumerT.repeat` loops it, and the roles also include `SinkT`, `SourceT`, and `ConduitT`.
