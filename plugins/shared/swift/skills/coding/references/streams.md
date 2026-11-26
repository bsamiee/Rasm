# [STREAMS]

## [01]-[SOURCES]

Host APIs delivering values over time return their own async sequence (`NotificationCenter.messages(of:for:bufferSize:)`, `URL.lines`, `FileHandle.bytes`), and consumers iterate it as declared. Callback hosts enter through `makeStream`: the host signals a wake, the consumer reads host state at each wake, and `onTermination` releases the registration:

```swift
static func changes() -> some AsyncSequence<Result<Self, Errno>, Never> & Sendable {
    let (memory, signal) = AsyncStream<Void>.makeStream(bufferingPolicy: .bufferingNewest(1))
    let source = DispatchSource.makeMemoryPressureSource(eventMask: [.normal, .warning, .critical], queue: .global())
    source.setEventHandler { signal.yield() }
    signal.onTermination = { _ in source.cancel() }
    source.activate()
    return merge(chain([()].async, memory), NotificationCenter.default.messages(of: ProcessInfo.processInfo, for: .thermalStateDidChange).map { _ in () })
        .map { _ in Result { () throws(Errno) in try current() } }
        .removeDuplicates()
}
```

`current()` reads memory and thermal pressure and throws `Errno`. `chain` puts a first read before any wake, `.bufferingNewest(1)` coalesces wakes arriving between reads into one, and `removeDuplicates` drops reads that changed nothing:
- Messages conforming to `MainActorMessage` offer `addObserver` alone and no `messages`, and their changes enter through a wake
- `onTermination` releases the registration feeding its own continuation
- Producers running async work are one `Task` started after `makeStream` and cancelled in `onTermination`, in place of a task inside the `AsyncStream` builder
- `merge` takes a fixed count of sources, and a dynamic or larger set forwards through one producer task's `withDiscardingTaskGroup`, one child per source
- `yield` returns `.terminated` after the consumer stops, and a producer on a thread of its own reads it to stop producing

Use `ownership.md` for a host handle streams share, released by its owner's `deinit`.

Use `interoperability.md` for C callbacks feeding a wake through an `Unmanaged` context.

## [02]-[TYPED_FAILURE]

`AsyncSequence` states its failure as its second primary associated type, and `for try await` throws exactly that type. Failure scope selects the form:

| [INDEX] | [FAILURE]                              | [FORM]                                                 |
| :-----: | :------------------------------------- | :----------------------------------------------------- |
|  [01]   | One element's, source continues        | `Result<T, E>` element over `Never`                    |
|  [02]   | Ends the sequence                      | `some AsyncSequence<T, E>` consumed by `for try await` |
|  [03]   | Untyped host failure ending a sequence | `mapError` into the module error, host cause kept      |

- `AsyncThrowingStream` initializers and `makeStream` require `Failure == any Error`, so a typed producer yields `Result<T, E>` into an `AsyncStream`
- Non-throwing closures of `map`, `filter`, `compactMap`, `flatMap`, `drop(while:)`, and `prefix(while:)` keep `Failure`, a throwing closure widens it to `any Error`
- `reduce`, `first(where:)`, `contains(where:)`, and `allSatisfy` are untyped `rethrows`, and a consumer keeping `E` loops with `for try await`
- AsyncAlgorithms operators over `Never` sources stay `Never`, and one failing source of `merge`, `zip`, or `combineLatest` cancels the others and ends the join
- `combineLatest` drops values buffered and unread when a source fails

## [03]-[OPERATORS]

Pipelines are one expression from sources to consumed values. Operators hold the state their rule needs (`removeDuplicates` the last element, `reductions` a running fold, `adjacentPairs` the predecessor, `debounce` the pending element), and joining operators differ in their first value and their end:

| [INDEX] | [OPERATOR]      | [FIRST_VALUE]                  | [ENDS_WHEN]                                          |
| :-----: | :-------------- | :----------------------------- | :--------------------------------------------------- |
|  [01]   | `merge`         | Any source's first element     | Every source ends                                    |
|  [02]   | `combineLatest` | Once every source produced one | Any source ends before its first element, or all end |
|  [03]   | `zip`           | Once every source produced one | Any source ends, the others are cancelled            |
|  [04]   | `chain`         | First source's first element   | Last source ends, each source after the one before   |

Sequences ending on an event merge their elements with the event mapped to `nil` and take the prefix before it:

```swift
func lifetime(_ running: some AsyncSequence<InstanceState, Never> & Sendable) -> some AsyncSequence<InstanceState, Never> & Sendable {
    let exited = process.exits().map { _ in InstanceState?.none }
    return merge(running.map(InstanceState?.some), exited).prefix { state in state != nil }.compactMap(\.self)
}
```

`exits()` yields once when the process ends. `chain` appends what follows the event, and the merge form stands in for `flatMapLatest`, which in swift-async-algorithms 1.1.7 traps at a state precondition when an outer element races its inner sequence's end:
- Waits subscribe before the act causing their event (a reply continuation stored before its request is sent), and an early event still reaches them
- One-shot waits read `first { _ in true }` of the matching sequence, and `nil` is the cancelled outcome

Loops that stop on a condition run as `contains`, which returns whether the loop stopped:

```swift
var acknowledgements = acks.makeAsyncIterator()
let interrupted = await chunks.contains { chunk in
    await send(chunk)
    return await acknowledgements.next() == nil
}
```

Finished acknowledgement streams stop the loop and read as an interruption, where a `for await` loop discarding `next()` read it as an acknowledgement and sent a buffered chunk.

## [04]-[OBSERVATION]

`Observations` turns reads of `@Observable` properties into an async sequence, and `withContinuousObservation(options:apply:)` runs a closure on each change in place of a value:
- `Observations { … }` yields its closure's value once per transaction, from a first `willSet` to the next suspension point
- `untilFinished` ends the sequence when its closure returns `.finish`, and a `throws(Failure)` closure ends it with that failure
- Closures run in the isolation they were formed in (`@isolated(any)`), and a `@MainActor` model is read without a hop
- Tracking covers the properties a closure read on its latest run, a branch that stops reading a property stops tracking it
- `withContinuousObservation` reports `willSet`, `didSet`, and `deinit` events that `matches(_:)` tests against a key path, until its noncopyable `Token` is cancelled

## [05]-[BACK_PRESSURE]

Back pressure concerns push producers, as a pull source produces only when its consumer calls `next()`. Producer ability to wait selects the form:

| [INDEX] | [PRODUCER]                                      | [FORM]                                                                 |
| :-----: | :---------------------------------------------- | :--------------------------------------------------------------------- |
|  [01]   | Async code that waits for its consumer          | `AsyncChannel`                                                         |
|  [02]   | Pull source allowed to run `n` elements ahead   | `buffer(policy: .bounded(n))`                                          |
|  [03]   | Producers pausing at a high mark, typed failure | `MultiProducerSingleConsumerAsyncChannel` with `.watermark(low:high:)` |
|  [04]   | Callback host that pauses and resumes           | Same channel, host resumed in `send(_:onProduceMore:)`                 |
|  [05]   | Callback that cannot wait                       | `makeStream` with `.bufferingNewest(n)` or `.bufferingOldest(n)`       |

- `AsyncChannel.send` suspends until a consumer's `next()` takes the element, and a sender runs one element ahead with no queue in the channel
- Senders cancelled while suspended resume without sending, and `finish()` resumes every suspended sender
- Many senders and one writer share one channel: each handler awaits `send`, one child reduces the channel into the transport and calls `finish()` in its `defer`
- `makeChannel(of:throwing:backpressureStrategy:)` returns a noncopyable channel and source, and `send` returns `.produceMore` or a callback handle at the high mark
- `makeAdditionalSource()` adds a producer, the channel finishes once every source is dropped or `finish(throwing:)` runs, and `elements()` hands the consumer a sequence
- `AsyncStream` spells its dropping policies `.bufferingNewest(n)` and `.bufferingOldest(n)`, AsyncAlgorithms `buffer` and `share` spell `.bufferingLatest(n)` and `.bufferingOldest(n)`

## [06]-[SHARING]

Async sequences serve one consumer, and `AsyncStream` traps when a second task awaits `next()`. Consumers needing the current value first register inside the owning actor, which yields its state and stores the continuation in one isolated step:

```swift
actor Watch {
    private var state: InstanceState
    private let subscribers: Mutex<[UUID: AsyncStream<Message>.Continuation]?> = Mutex([:])

    init(state: InstanceState) { self.state = state }

    func messages() -> AsyncStream<Message> {
        let (stream, continuation) = AsyncStream<Message>.makeStream()
        let id = UUID()
        continuation.yield(.state(state))
        continuation.onTermination = { _ in self.subscribers.withLock { held in held?[id] = nil } }
        let listening: Bool = subscribers.withLock { held in
            held?[id] = continuation
            return held != nil
        }
        if !listening { continuation.finish() }
        return stream
    }

    private func send(_ message: Message) {
        subscribers.withLock { held in
            for subscriber in (held ?? [:]).values { subscriber.yield(message) }
        }
    }

    private func finish() {
        let ending: [UUID: AsyncStream<Message>.Continuation] = subscribers.withLock { held in held.take() } ?? [:]
        for subscriber in ending.values { subscriber.finish() }
    }
}
```

`Message` is the actor's message enum with a `state` case. Isolation places every `send` before the replayed state or after the registration, so no message falls between them. `onTermination` runs outside the actor, and the table sits in a `Mutex` both sides reach. `nil` marks a finished source, and a consumer arriving after `finish()` ends at once.

`share(bufferingPolicy:)` runs one iteration of a source for every consumer that needs no current value:
- `.bounded(n)`, default `.bounded(1)`, delivers every element to every consumer, and the slowest consumer paces the source
- `.bufferingLatest(n)` and `.bufferingOldest(n)` let a slow consumer skip elements while the others continue
- Consumers join at `makeAsyncIterator()`, start at the oldest element another consumer holds unread, and receive no current value
- Source iteration runs in a task of its own from the first `next()`, and cancelling one consumer removes that consumer alone
- Releasing a shared sequence no consumer still iterates cancels its source iteration
