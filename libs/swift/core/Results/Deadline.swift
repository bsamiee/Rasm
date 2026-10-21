// --- [TYPES] ---------------------------------------------------------------------------

public protocol DeadlineFailure: Error {
    static func deadline(_ deadline: Deadline) -> Self
}

// --- [ERRORS] --------------------------------------------------------------------------

public enum Deadline: Error, Equatable {
    case timedOut(Duration)
    case cancelled
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public func race<Value: Sendable, Failure: DeadlineFailure>(
    _ work: @escaping @Sendable () async -> Result<Value, Failure>,
    against rival: @escaping @Sendable () async -> Result<Value, Failure>,
) async -> Result<Value, Failure> {
    await withTaskGroup { group in
        group.addTask(name: "work") { await work() }
        group.addTask(name: "rival") { await rival() }
        defer { group.cancelAll() }
        return await group.next() ?? .failure(.deadline(.cancelled))
    }
}

public func withTimeout<Value: Sendable, Failure: DeadlineFailure>(
    _ timeout: Duration,
    _ work: @escaping @Sendable () async -> Result<Value, Failure>,
) async -> Result<Value, Failure> {
    await race(work) {
        await Result { try await Task.sleep(for: timeout) }
            .mapError { _ in Failure.deadline(.cancelled) }
            .flatMap { _ in .failure(.deadline(.timedOut(timeout))) }
    }
}
