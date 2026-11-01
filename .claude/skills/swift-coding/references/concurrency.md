# [CONCURRENCY]

Concurrent work under compiler-checked isolation, from where a function runs to how tasks share, race, and stop.

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
    text.split(whereSeparator: \.isWhitespace).reduce(into: [:]) { counts, word in counts[word, default: 0] += 1 }
}
```

`load` suspends on the main actor while `frequencies` runs on the global executor, then resumes on the main actor to assign.

- Nonisolated async functions and closures run on their caller's actor, and a main-actor method passes them non-`Sendable` values
- Conformances of a `@MainActor` type are isolated to the main actor, and its requirement implementations read its state directly
- `isolated deinit` releases actor or main-actor state on its executor
- Blocking host calls under `withTaskExecutorPreference(<queue>)` block a `DispatchQueue` thread while the cooperative pool keeps every thread

Non-`Sendable` values cross isolation once the compiler proves the sender keeps no reference to them:
- Fresh values, `sending` parameters, and `sending` results each form a region of their own and cross whole
- `Task` and `addTask` closures are `sending`, and a non-`Sendable` local they capture moves into the task
- Factories returning a class state a `sending` result, and the caller receives a value no other code reaches
- Actor state leaves an actor as a `Sendable` copy, and a non-`Sendable` value built inside a method leaves as its `sending` result

## [02]-[TASK_TREES]

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

Each finished read starts the next pending key, and at most `width` reads run at once. Children return values, and the parent alone writes the dictionary. A typed read passes `Result` as `Value`.

- `async let` binds a fixed count of steps with different result types, and one never awaited is cancelled and awaited at scope exit
- `Task.immediate` runs on the caller's actor up to its first suspension, the form for a synchronous event handler that starts async work

## [03]-[DEADLINES]

Deadlines are instants, and a deadline passed down a call tree bounds every nested step at one moment. The work races a sleep to the deadline in one group, and the first outcome decides:

```swift
func value<Value: Sendable, Failure: Error, C: Clock>(
    of work: @escaping @Sendable () async throws(Failure) -> Value,
    by deadline: C.Instant,
    on clock: C,
    expired: Failure,
) async throws(Failure) -> Value {
    try await withTaskGroup(of: Result<Value, Failure>?.self) { group in
        group.addTask { await Result(catching: work) }
        group.addTask { (try? await clock.sleep(until: deadline, tolerance: nil)).map { .failure(expired) } }
        defer { group.cancelAll() }
        return await group.compacted().first { _ in true } ?? .failure(expired)
    }
    .get()
}
```

`expired` is the caller's deadline failure, and the call throws one typed failure for both outcomes. A wait an event ends early passes the event's stream as the work and its own deadline reason as `expired`. `cancelAll()` stops the losing child, and the group returns once that child ends, with work that never suspends holding the group until it checks cancellation. Cancelling the caller ends the sleep with no outcome, and the cancelled work's own outcome decides.

## [04]-[ACTORS]

Actors serialize calls, each `await` inside an actor method lets another call run, and a method reads state again after an `await`. An actor sharing remote values stores the running `Task` per key, and callers arriving during the first request await one fetch:

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

Entries written before the `await` serve the next caller. After the `await`, eviction compares the stored task with its own, and a newer task another caller stored stays. The fetch is an unstructured `Task` serving every waiting caller, and cancelling one caller leaves it running.

- Functions taking an `isolated` actor parameter run inside the actor, and their reads and writes take one hop
- Hosts calling back on a `DispatchSerialQueue` serve an actor whose `unownedExecutor` returns `unsafe queue.asUnownedSerialExecutor()`, and each callback enters through `assumeIsolated`

## [05]-[CANCELLATION_AND_BRIDGES]

Cancellation sets a flag on a task and every child, and code observes it where it checks:
- `withTaskCancellationHandler` runs its handler at once in a cancelled task, else on the cancelling thread beside the operation
- Handlers call the host's cancel alone
- Cleanup runs on every exit as `defer { await withTaskCancellationShield { await session.close() } }`, and inside the shield `Task.isCancelled` reads `false`, sleeps finish, handlers stay silent
- Children started inside a shield begin uncancelled, and their group's `cancelAll()` cancels them

Callback APIs with one completion enter through a typed continuation, and the cancel handler pairs the host's cancel with it:

```swift
func completion<Value: Sendable, Failure: Error>(
    _ start: (@escaping @Sendable (Result<Value, Failure>) -> Void) -> Void,
    cancel: @Sendable () -> Void,
) async throws(Failure) -> Value {
    try await withTaskCancellationHandler(
        operation: { () async throws(Failure) in
            try await withCheckedThrowingContinuation { (continuation: CheckedContinuation<Value, Failure>) in
                start { continuation.resume(with: $0) }
            }
        },
        onCancel: cancel,
    )
}
```

Callers write `try await completion({ host.lookup(key, then: $0) }, cancel: host.cancel)`, and the call throws the callback's typed failure. The host completes its callback after a cancel, and the completion resumes the continuation once. Delegate APIs that complete from a later method park the noncopyable `Continuation` from `withContinuation(of:throwing:)` in a `Mutex<Continuation<Value, Failure>?>`, and the delegate method resumes what `take()` returns, `nil` for a second completion.
