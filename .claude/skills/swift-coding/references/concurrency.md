# [CONCURRENCY]

Covers concurrent work under compiler-checked isolation, from where a function runs to how tasks share, race, and stop.

## [01]-[ISOLATION]

Models the interface reads are `@MainActor` and `@Observable`, and each heavy step is a `@concurrent` function the model awaits:

```swift
@MainActor
@Observable
final class Document {
    private(set) var counts: [Substring: Int] = [:]

    func load(_ text: String) async {
        counts = await frequencies(of: text)
    }
}

@concurrent
func frequencies(of text: String) async -> [Substring: Int] {
    Dictionary(text.split(whereSeparator: \.isWhitespace).map { ($0, 1) }, uniquingKeysWith: +)
}
```

`load` suspends on the main actor while `frequencies` runs on the global executor, then resumes on the main actor to assign, and the annotations state every hop.

- Async closure parameters run on their caller's actor, so a main-actor method calls a non-`Sendable` one
- `@concurrent` parameters and results cross isolation, each one `Sendable` or `sending`
- Conformances of a `@MainActor` type are isolated to the main actor, so its requirement implementations read its state directly
- `isolated deinit` releases actor or main-actor state on its executor
- Blocking host calls run in a `@concurrent` function under `withTaskExecutorPreference(<queue>)`, so a `DispatchQueue` thread blocks while the cooperative pool keeps every thread

## [02]-[REGIONS]

Non-`Sendable` values cross isolation once the compiler proves the sender keeps no reference to them:
- Fresh values, `sending` parameters, and `sending` results each form a region of their own and cross whole
- `sending` on a parameter states the callee takes the value, and a later read by the caller is a compile error
- `sending` on a result states the caller receives a value no other code reaches, the form for a factory returning a class
- `Task` and `addTask` closures are `sending`, so a non-`Sendable` local they capture moves into the task
- Actor state leaves an actor as a `Sendable` copy, and a non-`Sendable` value built inside a method leaves as its `sending` result

## [03]-[TASK_TREES]

Child tasks belong to the scope that starts them: the scope returns after every child finishes, and cancelling it cancels every child. A count the input decides is a task group, its width a parameter:

```swift
func values<Key: Hashable & Sendable, Value: Sendable>(
    for keys: [Key],
    width: Int,
    read: @escaping @Sendable (Key) async -> Value,
) async -> [Key: Value] {
    await withTaskGroup(of: (Key, Value).self) { group in
        for key: Key in keys.prefix(width) {
            group.addTask { (key, await read(key)) }
        }
        var pending: ArraySlice<Key>.Iterator = keys.dropFirst(width).makeIterator()
        var found: [Key: Value] = [:]
        for await (key, value): (Key, Value) in group {
            found[key] = value
            if let next: Key = pending.next() {
                group.addTask { (next, await read(next)) }
            }
        }
        return found
    }
}
```

Each finished read starts the next pending key, so at most `width` reads run at once. Children return values, and the parent alone writes the dictionary.

- `async let` binds a fixed count of steps with different result types, and one never awaited is cancelled and awaited at scope exit
- Children with no result run in `withDiscardingTaskGroup`, which frees each finished child
- `Task.immediate` runs on the caller's actor up to its first suspension, the form for a synchronous event handler that starts async work

## [04]-[DEADLINES]

Deadlines are instants, so a deadline passed down a call tree bounds every nested step at one moment. The work races a sleep to the deadline in one group, and the first child to finish decides:

```swift
func value<Value: Sendable, Failure: Error, C: Clock>(
    of work: @escaping @Sendable () async throws(Failure) -> Value,
    by deadline: C.Instant,
    on clock: C,
    expired: Failure,
) async throws(Failure) -> Value {
    let first: Result<Value, Failure> = await withTaskGroup { group in
        group.addTask { await Result(catching: work) }
        group.addTask {
            try? await clock.sleep(until: deadline, tolerance: nil)
            return .failure(expired)
        }
        defer { group.cancelAll() }
        return await group.next() ?? .failure(expired)
    }
    return try first.get()
}
```

`expired` is the caller's failure for a passed deadline, so the outcome keeps one typed failure. `cancelAll()` stops the losing child, and the group returns once that child ends, so work that runs without suspending holds the group until it checks cancellation. Cancelling the caller ends the sleep at once, and the caller receives the outcome of the first child to finish.

## [05]-[ACTORS]

Actors serialize calls, and each `await` inside an actor method lets another call run. An actor sharing remote values stores the running `Task` per key, so callers arriving during the first request await one fetch:

```swift
actor Cache<Key: Hashable & Sendable, Value: Sendable, Failure: Error> {
    private var entries: [Key: Task<Result<Value, Failure>, Never>] = [:]
    private let fetch: @Sendable (Key) async throws(Failure) -> Value

    init(fetch: @escaping @Sendable (Key) async throws(Failure) -> Value) {
        self.fetch = fetch
    }

    func value(for key: Key) async throws(Failure) -> Value {
        let task: Task<Result<Value, Failure>, Never> = entries[key] ?? Task { [fetch] in await Result { () async throws(Failure) in try await fetch(key) } }
        entries[key] = task
        let result: Result<Value, Failure> = await task.value
        if case .failure = result, entries[key] == task {
            entries[key] = nil
        }
        return try result.get()
    }
}
```

Entries are written before the `await`, so the next caller finds one. After the `await`, eviction compares the stored task with its own, and a newer task another caller stored stays. The fetch is an unstructured `Task` serving every waiting caller, so cancelling one caller leaves it running.

- Methods with no `await` run as one transaction
- Computation inside an actor is a `@concurrent` function the actor awaits, so other calls run while it computes
- Functions taking an `isolated` actor parameter run inside the actor, so its reads and writes take one hop
- `nonisolated` members read `let` state of a `Sendable` type synchronously

## [06]-[CANCELLATION_AND_BRIDGES]

Cancellation sets a flag on a task and every child, and code observes it where it checks:
- Sleeps throw `CancellationError` at cancellation, so a loop that sleeps stops at its sleep
- `withTaskCancellationHandler` runs its handler at once for a task cancelled already, so the host's cancel holds for an operation started after it
- Handlers run concurrently with the operation on the cancelling thread and call the host's cancel alone

Callback APIs with one completion enter through a typed continuation, and the cancel handler pairs the host's cancel with it:

```swift
func completion<Value: Sendable, Failure: Error>(
    _ start: (@escaping @Sendable (Result<Value, Failure>) -> Void) -> Void,
    cancel: @Sendable () -> Void,
) async throws(Failure) -> Value {
    try await withTaskCancellationHandler { () async throws(Failure) in
        try await withCheckedThrowingContinuation { (continuation: CheckedContinuation<Value, Failure>) in
            start { continuation.resume(with: $0) }
        }
    } onCancel: {
        cancel()
    }
}
```

Callers write `try await completion { host.lookup(key, then: $0) } cancel: { host.cancel() }`, and the call throws the callback's typed failure. The host completes its callback after a cancel, which resumes the continuation once. Delegate APIs that complete from a later method park the noncopyable `Continuation` from `withContinuation(of:throwing:)` in a `Mutex<Continuation<Value, Failure>?>`, and the delegate method resumes what `take()` returns, so a second completion finds `nil`.
