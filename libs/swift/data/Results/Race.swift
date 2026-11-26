// --- [OPERATIONS] ----------------------------------------------------------------------

public func race<Value: Sendable, Failure: Error>(
    _ work: @escaping @Sendable () async throws(Failure) -> Value,
    against rival: @escaping @Sendable () async throws(Failure) -> Value,
) async throws(Failure) -> Value {
    try await race(work) { await Result(catching: rival) }
}

public func withTimeout<Value: Sendable, Failure: Error>(
    _ timeout: Duration,
    expired: Failure,
    clock: some Clock<Duration> = ContinuousClock(),
    _ work: @escaping @Sendable () async throws(Failure) -> Value,
) async throws(Failure) -> Value {
    try await race(work) { (try? await clock.sleep(for: timeout)).map { .failure(expired) } }
}

public func withTimeout<Value: Sendable, Failure: Error>(
    _ timeout: Duration,
    expired: Failure,
    cancelled: Failure,
    clock: some Clock<Duration> = ContinuousClock(),
    answering start: @escaping @Sendable (@escaping @Sendable (Result<Value, Failure>) -> Void) -> Void,
) async throws(Failure) -> Value {
    try await withTimeout(timeout, expired: expired, clock: clock) { () async throws(Failure) -> Value in
        let answers = AsyncStream<Result<Value, Failure>>(bufferingPolicy: .bufferingOldest(1)) { answered in
            start { answer in
                answered.yield(answer)
                answered.finish()
            }
        }
        guard let answer = await answers.first(where: { _ in true }) else { throw cancelled }
        return try answer.get()
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
