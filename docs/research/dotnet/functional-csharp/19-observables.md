<!-- Fully integrated into dotnet-coding/SKILL.md [05.2] and references/streams.md -->
# [OBSERVABLES]

<!-- Integrated into .claude/skills/dotnet-coding/references/streams.md
## [01]-[MODEL]

`IObservable<T>` represents a sequence of values delivered over time. It combines the multiplicity of `IEnumerable<T>` with the delivery over time of `Task<T>`:

| [INDEX] | [CARDINALITY] | [SYNCHRONOUS]    | [ASYNCHRONOUS]   |
| :-----: | :------------ | :--------------- | :--------------- |
|  [01]   | One value     | `T`              | `Task<T>`        |
|  [02]   | Many values   | `IEnumerable<T>` | `IObservable<T>` |

View an `IEnumerable<T>` as an observable that produces all its values synchronously. View a `Task<T>` as an observable that produces one value.

Observables push notifications to an observer. The protocol is:

```text
OnNext* (OnCompleted | OnError)?
```

- `OnNext(T)` delivers zero or more values
- `OnCompleted()` ends the stream successfully
- `OnError(Exception)` ends the stream abnormally
- Streams can run forever, but must never emit again after either terminal notification

Subscription connects producer and consumer:

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
```

The returned `IDisposable` owns the subscription lifetime. Scope or dispose it, especially for sources that never complete.

`Subscribe` receives an `IObserver<T>`, and the observer implements `OnNext`, `OnError`, and `OnCompleted`. `Source.lift(IObservable<A>)` supplies that observer once and returns a `Source<A>`. `Reduce(seed, f)` on a `Source<A>` folds every value into one `IO<S>`.

```csharp
internal static class Model {
    public static Source<int> Observed => Source.lift(new Replay<int>(Seq(1, 2, 3)));
    public static IO<int> Total(Source<int> source) => source.Reduce(0, static (sum, item) => sum + item);
}
```
-->

<!-- Integrated into .claude/skills/dotnet-coding/references/streams.md
## [02]-[PROGRAM_STRUCTURE]

1. Acquire sources. Adapt callbacks, tasks, collections, or external event producers into observables.
2. Describe the dataflow. Transform and combine streams with operators. Keep this layer declarative and free of side effects.
3. Run effects at the edge. Subscribe only to the final streams and perform output, persistence, notifications, or diagnostics in observers. Reduce a `Source<A>` once at this layer, and the host runs the resulting `IO<S>`.

This separation keeps stream logic composable and shows where effects run and resources are managed.
-->

<!-- Integrated into .claude/skills/dotnet-coding/references/streams.md
## [03]-[STREAM_CREATION]

```csharp
internal static class Sources {
    private static Action<string> onMessage = static _ => { };

    private static readonly Event<string> Messages = Event.from(ref onMessage);

    public static Source<string> OneValue => Source.pure("ready");
    public static Source<int> FiniteValues => Source.lift(Seq(1, 2, 3));
    public static IO<string> FirstMessage =>
        from messages in Messages.Subscribe()
        from _ in IO.lift(static () => onMessage("hello"))
        from head in messages.Take(1).Last()
        select head;
}
```

The lifted single value emits immediately and completes. Lifted enumerables immediately emit their elements and complete. `Reduce` subscribes a lifted observable. Not every observable is lazy, subscription behavior depends on the source.

`Event.from(ref Action<A>)` adapts a callback-based producer (a message subscription). It adds its callback to the delegate's invocation list. `Subscribe()` returns an `IO<Source<A>>` that receives every later invocation of the delegate. The `Event` is `IDisposable`, and disposing it detaches the delegate.

`Subject<T>` is both an observer and an observable, imperative code can call its `OnNext`, `OnError`, and `OnCompleted` methods. Use it at callback or event boundaries. Prefer `Event.from` or a dedicated source when either expresses the source directly. This keeps calls to observer methods out of the stream definition.

`FromEvent` and `FromEventPattern` adapt event-based APIs.
-->

<!-- Integrated into .claude/skills/dotnet-coding/references/streams.md
## [04]-[STREAM_OPERATORS]

Operators produce new observables rather than handling individual events imperatively.

| [INDEX] | [OPERATOR]      | [DESCRIPTION]                                                                                       |
| :-----: | :-------------- | :-------------------------------------------------------------------------------------------------- |
|  [01]   | `Select`        | Map each emitted value                                                                              |
|  [02]   | `SelectMany`    | Map each value to an observable or task, then flatten all inner values into one stream              |
|  [03]   | `Where`         | Retain values satisfying a predicate                                                                |
|  [04]   | `Take`          | Retain the first count of values, or values produced within a given timespan                        |
|  [05]   | `Skip`          | Discard an initial portion of a stream                                                              |
|  [06]   | `First`         | Reduce the stream to its first value                                                                |
|  [07]   | `Concat`        | Emit the first stream, then subscribe to the second after the first completes                       |
|  [08]   | `StartWith`     | Prefix a stream with one or more initial values                                                     |
|  [09]   | `Merge`         | Interleave values from streams as they arrive                                                       |
|  [10]   | `CombineLatest` | After both sources have emitted, recompute from their latest values whenever either source changes  |
|  [11]   | `Scan`          | Emit every successive accumulated state                                                             |
|  [12]   | `GroupBy`       | Split one stream into keyed streams                                                                 |

`Source<A>` provides the equivalent operations: `Map`, `Filter`, `Take`, `Skip`, `Zip`, `Combine`, `Source.merge`, and a query that flattens an inner source.

Queries over `Source<A>` flatten the inner source of each value instead of blocking on it:

```csharp
internal static class Queries {
    private static readonly HashMap<string, Seq<decimal>> History = HashMap(("EURUSD", Seq(1.1m, 1.2m)), ("GBPUSD", Seq(1.3m)));

    public static Source<decimal> Quotes(string pair) => Source.lift(History.Find(pair).IfNone(Seq<decimal>()));
    public static Source<decimal> Rates(Source<string> currencyPairs) =>
        from pair in currencyPairs
        from rate in Quotes(pair)
        select rate;
}
```

`Combine` depends on completion. If its first source never completes, the second source is never observed. If the requirement is to emit an initial value before the source, use `Source.pure(value).Combine(source)`.

Partitioning is the stream equivalent of branching:

```csharp
internal static class Branches {
    public static (Source<A> Passed, Source<A> Failed) Partition<A>(this Source<A> source, Func<A, bool> predicate) =>
        (source.Filter(predicate), source.Filter(value => !predicate(value)));
    public static IO<int> Rejoined(Source<int> source) {
        (Source<int> passed, Source<int> failed) = source.Partition(static item => item > 1);
        return Source.merge(passed.Map(static item => item * 10), failed).Reduce(0, static (sum, item) => sum + item);
    }
}
```

Each branch can be transformed independently, normalized to a common type, and merged with `Source.merge`.
-->

<!-- Integrated into .claude/skills/dotnet-coding/references/streams.md
## [05]-[FAILURE_HANDLING]

`OnError` is terminal. When a derived stream reports an error, that stream and every downstream stream terminate permanently. Upstream streams can continue. Only part of the dataflow then remains active.

Do not use the terminal error channel for an expected per-item failure. Instead:
1. Apply the operation to each input with `Map`
2. The operation returns a `Fin<R>`, which represents either the computed value or the `Error`
3. `Reduce` that stream into computed values and errors
4. Translate both cases to a common output type with `Match` and keep them in one stream

Expected failures remain ordinary stream values:

```csharp
internal sealed record UnknownPair() : Expected("unknown currency pair", 1901);

internal static class Failures {
    private static readonly HashMap<string, decimal> Table = HashMap(("EURUSD", 1.1m), ("GBPUSD", 1.3m));

    public static Fin<decimal> Rate(string pair) => Table.Find(pair).ToFin(new UnknownPair());
    public static Source<Fin<decimal>> Outcomes(Source<string> pairs) => pairs.Map(Rate);
    public static Source<string> Outputs(Source<string> pairs) =>
        Source.pure("Enter a currency pair").Combine(
            Outcomes(pairs).Map(static outcome => outcome.Match(
                Succ: static rate => rate.ToString(CultureInfo.InvariantCulture),
                Fail: static error => error.Message)));
    public static IO<(Seq<decimal> Rates, Seq<Error> Errors)> Partitioned(Source<string> pairs) =>
        Outcomes(pairs).Reduce(
            (Rates: Seq<decimal>(), Errors: Seq<Error>()),
            static (state, outcome) => outcome.Match(
                Succ: rate => (state.Rates.Add(rate), state.Errors),
                Fail: error => (state.Rates, state.Errors.Add(error))));
}
```

This distinction prevents one malformed message from terminating a long-lived processing branch.
-->

<!-- Integrated into .claude/skills/dotnet-coding/references/streams.md
the `Drained` conduit example of [06.2] belongs to dotnet-languageext and its streams reference
## [06]-[LOGIC_ACROSS_EVENTS]

Reactive streams express logic in which processing a new event depends on earlier events or another event source.

### [06.1]-[TIMED_PATTERNS]

Pairing every value with its successor makes transitions explicit:

```csharp
internal static class Transitions {
    public static Source<(A Previous, A Current)> PairWithPrevious<A>(this Source<A> source) =>
        source.Zip(source.Skip(1)).Map(static pair => (Previous: pair.First, Current: pair.Second));
}
```

`Skip(1)` shifts the second subscription by one value, `Zip` pairs each value with its successor. Filtering the pairs can recognize a multi-key sequence without an explicit mutable state machine.

This implementation subscribes to `source` twice, and each subscription observes values produced from its subscription time. Its meaning depends on how the source behaves when subscribed to more than once.

### [06.2]-[SOURCES_AND_BACKPRESSURE]

If one source emits more frequently than the output requires, reduce it before combining. The conduit's `Sink` receives values through `Post`, and `Comap` adapts it to the producer's value type. Use `CombineLatest` when either input invalidates the derived value. `Zip` is the choice when each value has one matching partner.

```csharp
internal static class Backpressure {
    public static Source<decimal> BalanceInUsd(Source<decimal> euroBalance, Source<decimal> eurUsdRate) =>
        euroBalance.Zip(eurUsdRate).Map(static pair => pair.First * pair.Second);
    public static IO<Seq<decimal>> Retained(Buffer<decimal> buffer, Seq<decimal> rates) {
        Conduit<decimal, decimal> quotes = Conduit.make(buffer);
        return
            from _ in rates.TraverseM(quotes.Post).As()
            from __ in quotes.Complete()
            from kept in quotes.Reduce(Seq<decimal>(), static (kept, rate) => Reduced.ContinueIO(kept.Add(rate)))
            select kept;
    }
    public static IO<Seq<decimal>> Drained(Buffer<decimal> buffer, Seq<decimal> rates) {
        Conduit<decimal, decimal> quotes = Conduit.make(buffer);
        return
            from running in quotes.Reduce(Seq<decimal>(), static (kept, rate) => Reduced.ContinueIO(kept.Add(rate))).Fork()
            from _ in rates.TraverseM(quotes.Post).As()
            from __ in quotes.Complete()
            from kept in running.Await
            select kept;
    }
    public static IO<Unit> PostLength(Sink<int> sink, string text) => sink.Comap(static (string s) => s.Length).Post(text);
}
```

The consumer cannot slow the producer by requesting the next item. When production outpaces consumption, choose a policy explicitly through the `Buffer<A>` given to `Conduit.make`:
- `Buffer<A>.Unbounded` keeps every value
- `Buffer<A>.Bounded(n)` and `Buffer<A>.Single` hold `n` values or one value and block `Post` when full, the consumer is forked before the producer posts
- `Buffer<A>.Latest(value)` keeps only the last value, and `Buffer<A>.Newest(n)` keeps the last `n` values

Rx names the time-based and grouping-based counterparts `Sample`, `Throttle`, `Debounce`, `Buffer`, and `Window`. The policy must reflect whether intermediate values can be dropped, delayed, grouped, or preserved.

### [06.3]-[STATEFUL_LOGIC]

`Scan` is a running fold: unlike `Aggregate`, which waits for completion, it emits each accumulated state.

```csharp
internal sealed record Transaction(Guid AccountId, decimal Amount);

internal static class Ledger {
    public static IO<Seq<Guid>> Overdrawn(Source<Transaction> transactions) =>
        transactions
            .Reduce(HashMap<Guid, Seq<decimal>>(), static (ledger, transaction) =>
                ledger.AddOrUpdate(transaction.AccountId, amounts => amounts.Add(transaction.Amount), Seq(transaction.Amount)))
            .Map(static ledger => toSeq(ledger.Filter(Crossed).Keys));
    private static bool Crossed(Seq<decimal> amounts) {
        Seq<decimal> balances = amounts.Scan(0m, static (balance, amount) => balance + amount);
        return balances.Zip(balances.Tail).Exists(static step => step.First >= 0m && step.Second < 0m);
    }
}
```

`Reduce` groups the amounts of each account in a `HashMap`, and `Scan` carries each balance forward. `Zip` with `Tail` pairs each balance with its predecessor, the filter sees only a crossing from nonnegative to negative. The seed is emitted first, the first transaction can form a transition from the opening balance.
-->

<!-- Integrated into .claude/skills/dotnet-coding/SKILL.md
## [07]-[FIT_AND_LIMITS]

Use `IObservable` when:
- Values arrive asynchronously over time
- Logic detects sequences, transitions, windows, or relationships across sources
- The system forms a one-way dataflow (queue-to-database processing, fire-and-forget messaging)

Avoid it when:
- Events are independent and callbacks or tasks are clearer
- Every input needs a directly correlated response, as in request-response protocols
- Synchronization requires finer control than available operators provide

`OnNext` returns no value, information flows downstream only. For coordination that requires explicit queues and exact sequencing, `Conduit` is the queue and `Pipes` is the pipeline. `ProducerT`, `PipeT`, and `ConsumerT` fuse with `|` into one `EffectT` that the host runs.

`IObservable<T>` does not specify how schedulers dispatch observer calls or how subject types behave.
-->
