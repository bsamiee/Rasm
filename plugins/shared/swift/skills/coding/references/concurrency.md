# [CONCURRENCY]

## [01]-[ISOLATION]

Targets state isolation once: app targets set `.defaultIsolation(MainActor.self)` in `swiftSettings`, libraries stay nonisolated, and `NonisolatedNonsendingByDefault` runs a nonisolated async function on its caller's actor. Steps leaving the caller's actor are `@concurrent`:

```swift
extension Installation {
    @MainActor
    static func listing(of query: NSMetadataQuery) async -> [Result<Installation, ApplicationFailure>] {
        let bundles: [URL] = (0..<query.resultCount).compactMap { index in
            (query.value(ofAttribute: NSMetadataItemPathKey, forResultAt: index) as? String).map { path in URL(filePath: path, directoryHint: .isDirectory) }
        }
        return await read(bundles)
    }

    @concurrent
    private static func read(_ bundles: [URL]) async -> [Result<Installation, ApplicationFailure>] {
        bundles.map { bundle in Result { () throws(ApplicationFailure) in try Installation(at: bundle) } }
    }
}
```

`NSMetadataQuery` is main-actor host state read where it lives, and the `[URL]` crossing to `read` is `Sendable`. `Installation(at:)` reads a bundle's Info.plist from disk and throws `ApplicationFailure`, and the main actor stays free while every bundle reads:
- Nonisolated async functions take non-`Sendable` arguments from the calling actor, as they run on it
- `@concurrent` functions needing main-actor facts read them in one `await MainActor.run { (...) }` returning a tuple
- `InferIsolatedConformances` isolates a `@MainActor` type's conformances to the main actor, and its requirement implementations read its state directly
- `isolated deinit` releases actor or main-actor state on its executor

Non-`Sendable` values cross isolation once the compiler proves the sender keeps no reference to them:
- Fresh values, `sending` parameters, and `sending` results each form a region of their own and cross whole
- `Task`, `Task(executorPreference:)`, and `addTask` take `sending` closures, and a non-`Sendable` local they capture moves into the task
- Functions moving a non-`Sendable` host value into a task take it `sending` (`send(_ event: sending NSAppleEventDescriptor)`)
- Factories returning a class state a `sending` result, and the caller receives a value no other code reaches
- Actor state leaves an actor as a `Sendable` copy, and a non-`Sendable` value built inside a method leaves as its `sending` result
- Region and noncopyable-move errors come from SIL passes: `swiftc -typecheck` accepts code `swiftc -emit-sil` and `nx build` refuse

## [02]-[TASK_HIERARCHY]

Child tasks belong to the scope starting them: the scope returns after every child finishes, and cancelling it cancels every child. Loops ending together are children of one group, the first failure returns, and `defer` cancels the rest:

```swift
let ended: Result<Void, any Error>? = await withTaskGroup(of: Result<Void, any Error>.self) { group in
    group.addTask(name: "write") {
        await Result { () async throws in try await replies.reduce(()) { _, line in try await write(line) } }
    }
    group.addTask(name: "read") {
        defer { replies.finish() }
        return await Result { () async throws in try await lines().reduce(()) { _, line in await server.receive(line) } }
    }
    defer { group.cancelAll() }
    return await group.first { ending in ending.is(\.failure) }
}
```

`lines` is a `@Sendable` function returning the request lines, `server.receive` starts one answer per request, `replies` is the `AsyncStream<Data>` of encoded answers, and `write` sends one to the transport. A failed write ends the read, where an `async let` writer beside a sequential read left the next reply and the close waiting on a stream nothing consumed:
- `async let` binds a fixed number of steps of different result types, and one never awaited is cancelled and awaited at scope exit
- `async let _ = connection.read(lines, handler: handler)` reads for as long as the scope's body runs
- `withDiscardingTaskGroup` holds children whose results nobody reads (one answer per request, one observer per source), each given a `name:`
- Owners store each `Task` outliving its scope and cancel it in their teardown, and a `Task` stored nowhere ends only when its own work does
- Operations running one at a time on one model chain tasks: each new `Task` awaits the previous one's value and its cancel handler cancels it
- `Task.immediate` runs on the caller's actor up to its first suspension, the form for a synchronous event handler that starts async work
- Groups bounded to `width` children add the first `width` keys, then one pending key per finished child, and the parent alone writes the results

## [03]-[DEADLINES]

Steps with a time budget race the work against a sleep in one group, and the first outcome decides. `Results` declares the race once for every module:

```swift
public func withTimeout<Value: Sendable, Failure: Error>(
    _ timeout: Duration,
    expired: Failure,
    clock: some Clock<Duration> = ContinuousClock(),
    _ work: @escaping @Sendable () async throws(Failure) -> Value,
) async throws(Failure) -> Value {
    try await race(work) { (try? await clock.sleep(for: timeout)).map { .failure(expired) } }
}

private func race<Value: Sendable, Failure: Error>(
    _ work: @escaping @Sendable () async throws(Failure) -> Value,
    against rival: @escaping @Sendable () async -> Result<Value, Failure>?,
) async throws(Failure) -> Value {
    try await withTaskGroup(of: Result<Value, Failure>?.self) { group in
        group.addTask(name: "work") { await Result(catching: work) }
        group.addTask(name: "rival") { await rival() }
        defer { group.cancelAll() }
        for await case let outcome? in group { return outcome }
        preconditionFailure("Task group ended without a work outcome")
    }
    .get()
}
```

`expired` is the caller's own failure (`ProcessFailure(subject: subject, reason: .timedOut(timeout))`), its reason holding the budget. `cancelAll()` stops the losing child, and the group returns once that child ends, so work that never suspends holds the group until it checks cancellation. Cancelling the caller ends the sleep with no outcome, and the cancelled work's own outcome decides. The work child always yields, and a fallback outcome naming `expired` in place of `preconditionFailure` reports a timeout that never happened:
- Public `race(_:against:)` takes a rival ending only by failing (a lock heartbeat), and the work's outcome ends the rival
- Waits for one event pass the event's stream as the work (`replies.first { _ in true }`), in place of a `merge` with a timer mapped to `nil`
- Deadlines handed down a call tree are one `C.Instant` every nested step sleeps `until:`, and a step with its own budget takes a `Duration`

## [04]-[ACTORS]

Actors serialize calls, each `await` inside an actor method lets another call run, and a method reads state again after an `await`. One actor per host process owns every non-`Sendable` handle of that host (`AXUIElement`, `AXObserver`, an event tap) with its tables, and values leaving it are `Sendable` records holding interned ids. Actors blocking on host calls run on their own serial executor:

```swift
actor Watcher {
    private let queue = DispatchSerialQueue(label: String(reflecting: Watcher.self))

    nonisolated var unownedExecutor: UnownedSerialExecutor { unsafe queue.asUnownedSerialExecutor() }

    nonisolated func enqueue(_ work: @escaping @Sendable (isolated Watcher) -> Void) {
        queue.async { [self] in assumeIsolated(work) }
    }
}
```

`FSEventStreamSetDispatchQueue` delivers the stream's callbacks on `queue`, and `enqueue` runs them inside the actor with no hop. Actors with their own executor run there whatever executor preference a caller's task holds. Default actors run where the calling task runs: on its `withTaskExecutorPreference` queue during a request, and on the cooperative pool for each `Task` the actor started once that task resumes, where each blocking call holds one pool thread:
- Nonisolated blocking calls run in a `@concurrent` function under `withTaskExecutorPreference(<queue>)` (`OSACopyScriptingDefinitionFromURL`)
- `Task(executorPreference:)` starts a blocking call without awaiting it, and its closure runs nonisolated on that executor
- Functions taking an `isolated` actor parameter run inside the actor, and their reads and writes take one hop

Actors sharing one host read store the running `Task` per key, and callers arriving during the read await the same one:

```swift
actor ScriptingDictionaries {
    private var held: [URL: (processes: Set<ProcessInstance>, reading: Task<Result<ScriptingDictionary, AppleEventFailure>, Never>)] = [:]

    func dictionary(of installation: Installation) async throws(AppleEventFailure) -> ScriptingDictionary {
        let processes = Set(NSWorkspace.shared.instances(of: installation.runs).map(\.process))
        let reading =
            switch held[installation.url] {
                case .some(let hit) where hit.processes == processes: hit.reading
                case .some, .none:
                    Task { await Result { () async throws(AppleEventFailure) in try ScriptingDictionary(definition: await installation.scriptingDefinition()) } }
            }
        held[installation.url] = (processes, reading)
        let read = await reading.value
        if read.is(\.failure), held[installation.url]?.reading == reading { held[installation.url] = nil }
        return try read.get()
    }
}
```

`scriptingDefinition()` reads the application's sdef and throws `AppleEventFailure`. Entries hold the fact keeping them valid, the running processes, and a changed set starts a new read. Entries written before the `await` serve the next caller. After the `await`, eviction compares the stored task with its own, and a newer task another caller stored stays. Cancelling one caller leaves the shared read running:
- Callers holding the only interest in a shared read await it under `withTaskCancellationHandler { await read.value } onCancel: { read.cancel() }`
- Actors spacing calls to a host that answers wrongly when called too often hold the last call's instant and sleep the rest of a measured gap

## [05]-[CANCELLATION_AND_CONTINUATIONS]

Cancellation sets a flag on a task and every child, and code observes it where it checks:
- `withTaskCancellationHandler` runs its handler before the operation in a task already cancelled, else on the cancelling thread beside the operation
- Handlers call the host's cancel alone, and the typed form takes the handler `sending`
- `withTaskCancellationShield` holds cleanup a cancelled task must finish: inside it `Task.isCancelled` reads `false`, sleeps finish, and handlers stay silent
- Children started inside a shield begin uncancelled, their group's `cancelAll()` cancels them, and a `Task` handle reads the task's real flag
- Cleanup that neither fails nor reports runs as `defer { await withTaskCancellationShield { await <cleanup> } }`

Cleanup that can fail catches the body's outcome into a `Result`, runs under a shield, throws its own failure, then returns the body's outcome:

```swift
nonisolated func presenting<Value>(
    _ ref: Int?,
    clock: some Clock<Duration>,
    _ body: ([Container]) async throws(ApplicationFailure) -> Value,
) async throws(ApplicationFailure) -> Value {
    var levels: [Container] = []
    let value: Result<Value, ApplicationFailure>
    do throws(ApplicationFailure) {
        while let level = try await popUp(toward: ref, past: levels.count, clock: clock) { levels.append(level) }
        value = .success(try await body(levels))
    } catch {
        value = .failure(error)
    }
    try await withTaskCancellationShield { () async throws(ApplicationFailure) in try await cancel(levels.first?.ref, clock: clock) }
    return try value.get()
}
```

`popUp` opens the next menu level toward `ref`, and `cancel` sends `kAXCancelAction` and awaits the menu's closed event. Outside a shield a cancelled task's wait ends at once and `cancel` throws for an action it sent, and a `defer` body can neither throw nor report a menu left open.

Callback APIs with one completion enter through `withContinuation(of:throwing:)`: its `Continuation` is noncopyable, `resume` consumes it, and its failure is typed. Escaping callbacks cannot consume a captured noncopyable value, and the continuation waits in a `Mutex` the callback takes it from:

```swift
func completion<Value: Sendable, Failure: Error>(
    _ start: (@escaping @Sendable (Result<Value, Failure>) -> Void) -> Void,
    cancel: @Sendable () -> Void,
) async throws(Failure) -> Value {
    try await withTaskCancellationHandler(
        operation: { () async throws(Failure) in
            try await withContinuation(of: Value.self, throwing: Failure.self) { continuation in
                let waiting = Mutex(Optional(continuation))
                start { result in waiting.withLock { waiting in waiting.take() }?.resume(with: result) }
            }
        },
        onCancel: cancel,
    )
}
```

Callers write `try await completion({ host.lookup(key, then: $0) }, cancel: host.cancel)`, and the call throws the callback's typed failure. Hosts completing after a cancel, or twice, find `nil` on the second `take()`. Delegate APIs store the `Mutex<Continuation<Value, Failure>?>` in the delegate, and each completing delegate method resumes what `take()` returns.
