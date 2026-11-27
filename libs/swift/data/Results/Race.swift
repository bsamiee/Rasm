// --- [OPERATIONS] ----------------------------------------------------------------------

package func race<Value: Sendable, Failure: Error>(
    _ work: @escaping @Sendable () async throws(Failure) -> Value,
    against rival: @escaping @Sendable () async throws(Failure) -> Value,
) async throws(Failure) -> Value {
    try await race(work) { await Result(catching: rival) }
}

package func withTimeout<Value: Sendable, Failure: Error>(
    _ timeout: Duration,
    expired: Failure,
    clock: some Clock<Duration> = SuspendingClock(),
    _ work: @escaping @Sendable () async throws(Failure) -> Value,
) async throws(Failure) -> Value {
    try await race(work) { (try? await clock.sleep(for: timeout)).map { .failure(expired) } }
}

package func withTimeout<Value: Sendable, Failure: Error>(
    _ timeout: Duration,
    expired: Failure,
    cancelled: Failure,
    clock: some Clock<Duration> = SuspendingClock(),
    _ work: @escaping @Sendable (_ completionHandler: @escaping @Sendable (Result<Value, Failure>) -> Void) -> Void,
) async throws(Failure) -> Value {
    try await withTimeout(timeout, expired: expired, clock: clock) { () async throws(Failure) -> Value in
        let (outcomes, outcome) = AsyncStream<Result<Value, Failure>>.makeStream(bufferingPolicy: .bufferingOldest(1))
        if !Task.isCancelled { work { completed in outcome.yield(completed) } }
        for await completed in outcomes { return try completed.get() }
        throw cancelled
    }
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
