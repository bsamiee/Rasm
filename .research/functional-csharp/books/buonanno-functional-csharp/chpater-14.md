# Data Streams with `IObservable`

## The model

`IObservable<T>` represents a sequence of values delivered over time. It combines the multiplicity of `IEnumerable<T>` with the time-oriented delivery of `Task<T>`:

| Shape | Synchronous | Asynchronous |
| --- | --- | --- |
| One value | `T` | `Task<T>` |
| Many values | `IEnumerable<T>` | `IObservable<T>` |

It is the more general shape: an `IEnumerable<T>` can be viewed as an observable that produces all its values synchronously, while a `Task<T>` can be viewed as one that produces a single value.

An observable pushes notifications to an observer. Its protocol is:

```text
OnNext* (OnCompleted | OnError)?
```

- `OnNext(T)` delivers zero or more values.
- `OnCompleted()` ends the stream successfully.
- `OnError(Exception)` ends the stream abnormally.
- A stream may never terminate, but after either terminal notification it must never emit again.

Subscription connects producer and consumer:

```csharp
IDisposable subscription = source.Subscribe(
    onNext: HandleValue,
    onError: HandleTerminalFailure,
    onCompleted: HandleCompletion);

subscription.Dispose(); // unsubscribe
```

The returned `IDisposable` owns the subscription lifetime. Scope or dispose it deliberately, especially for sources that never complete.

The object-oriented alternative is to pass an `IObserver<T>` to `Subscribe`; the observer implements `OnNext`, `OnError`, and `OnCompleted` directly.

## Structure a reactive program in three layers

1. **Acquire sources.** Adapt timers, callbacks, tasks, collections, or external event producers into observables.
2. **Describe the dataflow.** Transform and combine streams with operators. Keep this layer declarative and free of observable side effects.
3. **Run effects at the edge.** Subscribe only to the final streams and perform output, persistence, notifications, or diagnostics in observers.

This separation keeps stream logic composable while making effect ownership and resource lifetime visible.

## Creating streams

```csharp
IObservable<long> ticks =
    Observable.Interval(TimeSpan.FromSeconds(1));

IObservable<string> oneValue =
    Observable.Return("ready");

IObservable<Rate> oneAsyncValue =
    Observable.FromAsync(() => GetRate());

IObservable<int> finiteValues =
    new[] { 1, 2, 3 }.ToObservable();
```

`Interval` is lazy: once subscribed, it waits one interval, emits `0`, increments on each interval, and does not complete by itself. The lifted single value emits immediately and completes. A lifted task emits its result when available and then completes. A lifted enumerable immediately emits its elements and completes. Not every observable is lazy; subscription behavior depends on the source.

Use `Observable.Create<T>` to adapt a callback-based producer such as a message subscription. Its subscription function receives an observer: register callbacks that forward values to `OnNext`, failures to `OnError`, and normal termination to `OnCompleted`, then return the subscription resource. Disposing that resource must detach the callbacks.

`Subject<T>` is both an observer and an observable, so imperative code can call its `OnNext`, `OnError`, and `OnCompleted` methods. It is useful at unavoidable push boundaries, but prefer `Observable.Create` or a dedicated source operator when they can express the source directly. This keeps imperative signaling out of the stream definition.

Other source adapters include `FromEvent` and `FromEventPattern` for event-based APIs.

## Transforming and combining streams

Operators produce new observables rather than handling individual events imperatively.

| Operator | Meaning |
| --- | --- |
| `Select` | Map each emitted value. |
| `SelectMany` | Map each value to an observable or task, then flatten all inner values into one stream. |
| `Where` | Retain values satisfying a predicate. |
| `Take` | Retain the first count of values, or values produced within a given timespan. |
| `Skip` | Discard an initial portion of a stream. |
| `First` | Reduce the stream to its first value. |
| `Concat` | Emit the first stream, then subscribe to the second after the first completes. |
| `StartWith` | Prefix a stream with one or more initial values. |
| `Merge` | Interleave values from streams as they arrive. |
| `CombineLatest` | After both sources have emitted, recompute from their latest values whenever either source changes. |
| `Scan` | Emit every successive accumulated state. |
| `GroupBy` | Split one stream into keyed streams. |

Use `SelectMany` instead of blocking on an asynchronous operation:

```csharp
IObservable<decimal> rates =
    from pair in currencyPairs
    from rate in GetRate(pair) // returns Task<decimal>
    select rate;
```

`Concat` depends on completion. If its first source never completes, the second source is never observed. `StartWith` is the direct choice when the requirement is simply to emit initial values before the source.

Partitioning is the stream equivalent of branching:

```csharp
public static (IObservable<T> Passed, IObservable<T> Failed)
    Partition<T>(this IObservable<T> source, Func<T, bool> predicate) =>
    (
        source.Where(predicate),
        source.Where(value => !predicate(value))
    );
```

Each branch can evolve independently and later be normalized to a common type and merged.

## Keep recoverable failures inside the stream

`OnError` is terminal. When a derived stream reports an error, that stream and every downstream stream terminate permanently; upstream streams may continue, leaving only part of the dataflow alive.

Do not use the terminal error channel for an expected per-item failure. Instead:

1. Apply the asynchronous operation to each input.
2. Map the resulting `Task<R>` into a `Task<Exceptional<R>>`, where `Exceptional<R>` explicitly represents either the computed value or the exception, so an expected failed computation becomes data rather than a terminal stream message.
3. Use `SelectMany` to flatten the tasks into one stream of explicit outcomes.
4. Partition that stream into computed values and exceptions.
5. Process both branches, translate them to a common output type, and merge them.

The resulting helper keeps the two cases alive as ordinary streams:

```csharp
var (rates, errors) = inputs.Safely(GetRate);

IObservable<string> outputs = rates
    .Select(Decimal.ToString)
    .Merge(errors.Select(error => error.Message))
    .StartWith("Enter a currency pair");
```

Reserve `OnError` for a failure that truly ends the whole stream. This distinction prevents one malformed message from killing a long-lived processing branch.

## Logic across events

Reactive streams are most valuable when the treatment of a new event depends on earlier events or on another event source.

### Adjacent values and timed patterns

Pairing every value with its successor makes transitions explicit:

```csharp
public static IObservable<(T Previous, T Current)>
    PairWithPrevious<T>(this IObservable<T> source) =>
    from previous in source
    from current in source.Take(1)
    select (previous, current);
```

For a live source whose subscriptions observe values from their subscription time, the inner `Take(1)` selects the next value after each outer value. Add a time-bounded `Take` to require the pair to occur within a window:

```csharp
IObservable<(KeyInfo First, KeyInfo Second)> quickPairs =
    from first in keys
    from second in keys.Take(1).Take(maxDelay)
    select (first, second);
```

Filtering `quickPairs` can recognize a multi-key sequence without an explicit mutable state machine.

This implementation subscribes to `source` again for every outer value, and each inner subscription observes values produced in the future. Its meaning therefore depends on the source's subscription behavior; do not assume every observable is lazy or behaves identically when subscribed more than once.

### Multiple sources and backpressure

`CombineLatest` is appropriate when either input invalidates a derived value:

```csharp
IObservable<decimal> balanceInUsd =
    euroBalance.CombineLatest(eurUsdRate, (balance, rate) => balance * rate);
```

If one source is much more volatile than the required output, reduce it before combining:

```csharp
var sampler = Observable.Interval(TimeSpan.FromMinutes(10));
var sampledRate = eurUsdRate.Sample(sampler);
var balanceInUsd = euroBalance.CombineLatest(
    sampledRate,
    (balance, rate) => balance * rate);
```

An observable pushes; the consumer cannot slow the producer by requesting the next item at its own pace. When production outpaces consumption, choose a policy explicitly:

- `Sample` emits the latest source value when a sampler signals.
- `Throttle`, `Debounce`, `Buffer`, and `Window` provide other time- or grouping-based policies, such as consuming fixed-size batches or retaining only the last value in a rapid cluster.

The policy must reflect whether intermediate values may be dropped, delayed, grouped, or preserved.

### Stateful business logic without mutable state

`Scan` is a running fold: unlike `Aggregate`, which waits for completion, it emits each new accumulated state.

```csharp
IObservable<Guid> overdrawnAccounts =
    from account in transactions.GroupBy(t => t.AccountId)
    from transition in account
        .Scan(0m, (balance, transaction) => balance + transaction.Amount)
        .StartWith(0m)
        .PairWithPrevious()
    where transition.Previous >= 0m && transition.Current < 0m
    select account.Key;
```

`GroupBy` creates one transaction stream per account, the second `from` flattens those streams, `Scan` carries each balance forward, and transition filtering emits only when a balance crosses from nonnegative to negative. The initial value is emitted explicitly so the first transaction can form a transition from the opening balance.

## Fit and limits

Use `IObservable` when:

- values arrive asynchronously over time;
- logic detects sequences, transitions, windows, or relationships across sources;
- the system naturally forms a one-way dataflow, such as queue-to-database processing or fire-and-forget messaging.

Avoid it when:

- events are independent and ordinary callbacks or tasks are clearer;
- every input needs a directly correlated response, as in request-response protocols;
- synchronization requires finer control than available operators provide.

`OnNext` returns no value, so information flows downstream only. For complex coordination built around explicit queues and precise sequencing, a dataflow abstraction may be a better fit.

Important Rx details remain beyond these core operators: schedulers determine how calls to observers are dispatched, not every observable is lazy, and different subjects have different behaviors. These details are not expressed by the `IObservable<T>` type alone.
