// --- [OPERATIONS] ----------------------------------------------------------------------

public func race<Value: Sendable, Failure: Error>(
    _ work: @escaping @Sendable () async throws(Failure) -> Value,
    against rival: @escaping @Sendable () async throws(Failure) -> Value,
) async throws(Failure) -> Value {
    try await withTaskGroup(of: Result<Value, Failure>.self) { group in
        group.addTask(name: "work") { await Result(catching: work) }
        group.addTask(name: "rival") { await Result(catching: rival) }
        defer { group.cancelAll() }
        for await outcome: Result<Value, Failure> in group { return outcome }
        preconditionFailure("A group holding the work yields its outcome")
    }
    .get()
}

public func withTimeout<Value: Sendable, Failure: Error>(
    _ timeout: Duration,
    expired: Failure,
    clock: some Clock<Duration> = ContinuousClock(),
    _ work: @escaping @Sendable () async throws(Failure) -> Value,
) async throws(Failure) -> Value {
    try await withTaskGroup(of: Result<Value, Failure>?.self) { group in
        group.addTask(name: "work") { await Result(catching: work) }
        group.addTask(name: "timeout") {
            do { try await clock.sleep(for: timeout) } catch { return nil }
            return .failure(expired)
        }
        defer { group.cancelAll() }
        for await case let outcome? in group { return outcome }
        preconditionFailure("A group holding the work yields its outcome")
    }
    .get()
}
