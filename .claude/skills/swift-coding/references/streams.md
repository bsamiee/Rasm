# [STREAMS]

Covers values over time as async sequences, from a host source through operators to pacing and sharing.

## [01]-[SOURCES]

Host APIs that deliver values over time return an async sequence of their own (`NotificationCenter.messages(of:for:)`, `URL.lines`, `FileHandle.bytes`), and a consumer iterates it as declared. Callback APIs that register a handler and return a token enter through one generic bridge, which infers value and token types from the host's signatures:

```swift
func values<Value: Sendable, Token: Sendable>(
    bufferingPolicy: AsyncStream<Value>.Continuation.BufferingPolicy = .bufferingNewest(1),
    observe: (@escaping @Sendable (Value) -> Void) -> Token,
    remove: @escaping @Sendable (Token) -> Void
) -> AsyncStream<Value> {
    let (stream, continuation): (AsyncStream<Value>, AsyncStream<Value>.Continuation) = AsyncStream.makeStream(bufferingPolicy: bufferingPolicy)
    let token: Token = observe { continuation.yield($0) }
    continuation.onTermination = { _ in remove(token) }
    return stream
}
```

- `values(observe: monitor.observe, remove: monitor.remove)` registers at the call, and the stream holds the token until its consumer stops
- `onTermination` runs once, on `finish()`, on cancellation, or when the consumer drops its iterator
- Wake signals are an `AsyncStream<Void>` at `.bufferingNewest(1)`, so wakes arriving between reads coalesce into one
- `yield` returns `.terminated` after the consumer stops, and a producer on a thread of its own reads it to stop producing

## [02]-[TYPED_FAILURE]

`AsyncSequence` states its failure as its second primary associated type, and `for try await` throws exactly that type, so a loop inside a `throws(E)` function keeps `E`:

```swift
func total(of url: URL, price: (String) throws(OrderError) -> Decimal) async throws(OrderError) -> Decimal {
    var sum: Decimal = 0
    for try await line: String in url.lines.mapError({ OrderError(subject: url.path(percentEncoded: false), reason: .unreadable($0)) }) {
        sum += try price(line)
    }
    return sum
}
```

`OrderError` is the module's error type with an `unreadable(any Error)` reason, and typed steps inside the loop throw the same type.

- Non-throwing `map`, `filter`, `prefix`, and AsyncAlgorithms operators keep `Failure`, a signature names it as `some AsyncSequence<T, E>`
- Throwing closures and reductions (`reduce`, `contains`, `min`, `max`, `first(where:)`) widen `Failure` to `any Error`, a typed fold is a loop

## [03]-[OPERATORS]

Pipelines read as one expression from sources to the values a consumer receives, each operator holding the state its rule needs (`removeDuplicates` the last element, `reductions` a running fold, `adjacentPairs` the predecessor):

```swift
func batches<Element: Equatable & Sendable>(
    of primary: some AsyncSequence<Element, Never> & Sendable,
    and backup: some AsyncSequence<Element, Never> & Sendable,
    count: Int = 100,
    every interval: Duration = .seconds(1),
    on clock: some Clock<Duration> = ContinuousClock()
) -> some AsyncSequence<[Element], Never> {
    merge(primary, backup)
        .removeDuplicates()
        .chunks(ofCount: count, or: .repeating(every: interval, clock: clock))
}
```

`chunks(ofCount:or:)` emits a batch at `count` elements or at each tick, whichever comes first, and a tick with nothing collected emits no batch. Operators joining sources differ in their first value and their end:

| [INDEX] | [OPERATOR]      | [FIRST_VALUE]                  | [ENDS_WHEN]                                          |
| :-----: | :-------------- | :----------------------------- | :--------------------------------------------------- |
|  [01]   | `merge`         | Any source's first element     | Every source ends                                    |
|  [02]   | `combineLatest` | Once every source produced one | Any source ends before its first element, or all end |
|  [03]   | `zip`           | Once every source produced one | Any source ends, the others are cancelled            |
|  [04]   | `flatMapLatest` | Inner sequence's first element | Outer source and its last inner sequence end         |

`flatMapLatest` cancels the running inner sequence on each outer element, so the consumer receives results of the latest query alone.

## [04]-[OBSERVATION]

`Observations` turns reads of `@Observable` properties into an async sequence, and `untilFinished` ends it on a state its closure reads:

```swift
@MainActor
@Observable
final class Order {
    var prices: [Decimal] = []
    var isSubmitted: Bool = false
}

@MainActor
func totals(of order: Order) -> some AsyncSequence<Decimal, Never> {
    Observations.untilFinished {
        order.isSubmitted ? .finish : .next(order.prices.reduce(0, +))
    }
}
```

- `Observations` runs its closure in the isolation where the closure was formed, so a `@MainActor` model is read without a hop
- Tracking covers the properties the closure read on its latest run, a branch that stops reading a property stops tracking it
- Mutations from the first `willSet` to the next suspension point form one transaction and yield one element
- Iteration yields the current value first, and a consumer slower than the model receives the newest value at its next `next()`
- Models hold state as properties alone, and each async consumer forms its own `Observations` over the properties it reads

## [05]-[PACING]

Sequences over a pull source produce only when their consumer calls `next()`, so pacing concerns push producers, and the producer's ability to wait selects the form:

| [INDEX] | [PRODUCER]                                      | [FORM]                                                                 |
| :-----: | :---------------------------------------------- | :--------------------------------------------------------------------- |
|  [01]   | Async code that waits for its consumer          | `AsyncChannel`                                                         |
|  [02]   | Pull source allowed to run `n` elements ahead   | `buffer(policy: .bounded(n))`                                          |
|  [03]   | Producers pausing at a high mark, typed failure | `MultiProducerSingleConsumerAsyncChannel` with `.watermark(low:high:)` |
|  [04]   | Callback that cannot wait                       | `values` with a dropping buffering policy                              |

`AsyncChannel.send` suspends until a consumer takes the element, so a producer runs at most one element ahead and the channel holds no queue:

```swift
func drain<Item: Sendable>(
    produce: @escaping @Sendable (AsyncChannel<Item>) async -> Void,
    into store: (Item) async -> Void
) async {
    let channel: AsyncChannel<Item> = AsyncChannel()
    await withDiscardingTaskGroup { group in
        group.addTask {
            await produce(channel)
            channel.finish()
        }
        for await item: Item in channel {
            await store(item)
        }
    }
}
```

`produce` sends from any number of child tasks of its own, `finish()` after it returns ends the consumer's loop, and cancelling the caller cancels both sides.

## [06]-[SHARING]

Async sequences serve one consumer, and `AsyncStream` traps on a second concurrent iterator. `share(bufferingPolicy:)` runs one iteration of the source for every consumer:

```swift
func split<Element: Sendable>(
    _ source: AsyncStream<Element>,
    bufferingPolicy: AsyncBufferSequencePolicy = .bounded(1),
    alarming isAlarm: @escaping @Sendable (Element) -> Bool
) -> (alarms: some AsyncSequence<Element, Never>, log: some AsyncSequence<Element, Never>) {
    let shared: some AsyncSequence<Element, Never> & Sendable = source.share(bufferingPolicy: bufferingPolicy)
    return (shared.filter(isAlarm), shared)
}
```

- `.bounded(n)` delivers every element to every consumer, and the slowest consumer paces the source
- `.bufferingLatest(n)` and `.bufferingOldest(n)` let a slow consumer skip elements while the others continue
- Consumers starting late receive elements from their first `next()` on, with no replay of earlier ones
- Iteration of the source runs in a task of its own from the first demand, and cancelling one consumer removes that consumer alone
