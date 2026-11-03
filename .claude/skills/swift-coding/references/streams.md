# [STREAMS]

## [01]-[SOURCES]

Host APIs delivering values over time return their own async sequence (`NotificationCenter.messages(of:for:)`, `URL.lines`, `FileHandle.bytes`). Consumers iterate it as declared. Callback APIs returning a registration token enter through one generic function that infers value and token types from host signatures:

```swift
func values<Value: Sendable, Token: Sendable>(
    bufferingPolicy: AsyncStream<Value>.Continuation.BufferingPolicy,
    observe: (@escaping @Sendable (Value) -> Void) -> Token,
    remove: @escaping @Sendable (Token) -> Void,
) -> AsyncStream<Value> {
    let (stream, continuation): (AsyncStream<Value>, AsyncStream<Value>.Continuation) = AsyncStream.makeStream(bufferingPolicy: bufferingPolicy)
    let token: Token = observe { continuation.yield($0) }
    continuation.onTermination = { _ in remove(token) }
    return stream
}
```

- Host members enter as `observe: monitor.observe, remove: monitor.remove`, and `values` registers at the call and removes when the consumer stops
- Wake signals are an `AsyncStream<Void>` at `.bufferingNewest(1)`, and wakes arriving between reads coalesce into one
- C callbacks pass `Unmanaged.passUnretained(owner).toOpaque()` as context and read through `takeUnretainedValue()`, and `deinit` removes the source
- `yield` returns `.terminated` after the consumer stops, and a producer on a thread of its own reads it to stop producing

## [02]-[TYPED_FAILURE]

`AsyncSequence` states its failure as its second primary associated type, and a `for try await` loop over it throws exactly that type:

```swift
func total(of url: URL, price: (String) throws(OrderError) -> Decimal) async throws(OrderError) -> Decimal {
    var sum: Decimal = 0
    for try await line: String in url.lines.mapError({ OrderError(subject: url.path(percentEncoded: false), reason: .failed($0)) }) {
        sum += try price(line)
    }
    return sum
}
```

`mapError` types the failure `URL.lines` throws untyped.

- Operators with a non-throwing closure keep `Failure`, a throwing transform closure widens it to `any Error`
- Sources failing only at start throw from the call and return an `AsyncStream`, `AsyncThrowingStream.makeStream` fixes `Failure` to `any Error`
- One element's failure in a source that continues is an element: `map` to `Result<T, E>` over a `Never` failure
- Any source of `merge`, `combineLatest`, or `zip` that fails cancels the others and ends the joined sequence with its failure

## [03]-[OPERATORS]

Pipelines are one expression from sources to consumed values. Operators hold the state their rule needs (`removeDuplicates` the last element, `reductions` a running fold, `adjacentPairs` the predecessor):

```swift
func overdrafts(
    of amounts: some AsyncSequence<Decimal, Never>,
    opening: Decimal,
    limit: Decimal = 0,
) -> some AsyncSequence<Decimal, Never> {
    chain([opening].async, amounts.reductions(opening, +))
        .adjacentPairs()
        .compactMap { previous, current in previous >= limit && current < limit ? current : nil }
}
```

`reductions` emits each running balance after its amount, and `chain` puts the opening balance first for the first amount to pair with. Operators joining sources differ in their first value and their end:

| [INDEX] | [OPERATOR]      | [FIRST_VALUE]                  | [ENDS_WHEN]                                          |
| :-----: | :-------------- | :----------------------------- | :--------------------------------------------------- |
|  [01]   | `merge`         | Any source's first element     | Every source ends                                    |
|  [02]   | `combineLatest` | Once every source produced one | Any source ends before its first element, or all end |
|  [03]   | `zip`           | Once every source produced one | Any source ends, the others are cancelled            |
|  [04]   | `flatMapLatest` | Inner sequence's first element | Outer source and its last inner sequence end         |

`flatMapLatest` cancels the running inner sequence on each outer element, and the consumer receives results of the latest query alone.

## [04]-[OBSERVATION]

`Observations` turns reads of `@Observable` properties into an async sequence, and `untilFinished` ends it on a state its closure reads. `Cart` is a `@MainActor` `@Observable` class holding `prices` and `isSubmitted`:

```swift
@MainActor
func totals(of cart: Cart) -> some AsyncSequence<Decimal, Never> {
    Observations.untilFinished {
        cart.isSubmitted ? .finish : .next(cart.prices.reduce(0, +))
    }
}
```

- `Observations` runs its closure in the isolation where the closure was formed, and reads a `@MainActor` model without a hop
- Tracking covers the properties the closure read on its latest run, a branch that stops reading a property stops tracking it
- Mutations from the first `willSet` to the next suspension point form one transaction and yield one element
- Each iterator yields the current value at its first `next()`, and a consumer slower than the model receives the newest value at its next `next()`
- One `Observations` value serves every consumer the same elements unbuffered, a closure failure ends every iterator, and cancellation ends one

## [05]-[BACK_PRESSURE]

Back pressure concerns push producers, and a pull source produces only when its consumer calls `next()`. The producer's ability to wait selects the form:

| [INDEX] | [PRODUCER]                                      | [FORM]                                                                 |
| :-----: | :---------------------------------------------- | :--------------------------------------------------------------------- |
|  [01]   | Async code that waits for its consumer          | `AsyncChannel`                                                         |
|  [02]   | Pull source allowed to run `n` elements ahead   | `buffer(policy: .bounded(n))`                                          |
|  [03]   | Producers pausing at a high mark, typed failure | `MultiProducerSingleConsumerAsyncChannel` with `.watermark(low:high:)` |
|  [04]   | Callback host that pauses and resumes           | Same channel, the host resumed in `send(_:onProduceMore:)`             |
|  [05]   | Callback that cannot wait                       | `values` with a dropping buffering policy                              |

`AsyncChannel.send` suspends until a consumer takes the element, and a producer runs at most one element ahead with no queue in the channel:

```swift
func drain<Item: Sendable>(
    produce: @escaping @Sendable (AsyncChannel<Item>) async -> Void,
    into store: (Item) async -> Void,
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

`produce` can send from its own child tasks, `finish()` after it returns ends the consumer's loop, and cancelling the caller cancels both sides.

## [06]-[SHARING]

Async sequences serve one consumer, and `AsyncStream` traps on a second concurrent iterator. `share(bufferingPolicy:)` runs one iteration of the source for every consumer:

```swift
func split<Element: Sendable>(
    _ source: AsyncStream<Element>,
    bufferingPolicy: AsyncBufferSequencePolicy,
    alarming isAlarm: @escaping @Sendable (Element) -> Bool,
) -> (alarms: some AsyncSequence<Element, Never>, log: some AsyncSequence<Element, Never>) {
    let shared: some AsyncSequence<Element, Never> & Sendable = source.share(bufferingPolicy: bufferingPolicy)
    return (shared.filter(isAlarm), shared)
}
```

- `.bounded(n)` delivers every element to every consumer, and the slowest consumer paces the source
- `.bufferingLatest(n)` and `.bufferingOldest(n)` let a slow consumer skip elements while the others continue
- Consumers starting late receive elements from their first `next()` on, with no replay of earlier ones
- Iteration of the source runs in a task of its own from the first demand, and cancelling one consumer removes that consumer alone
