public import Foundation

// --- [TYPES] ---------------------------------------------------------------------------

public protocol DeadlineFailure: Error {
    static func deadline(_ deadline: Deadline) -> Self
}

public protocol TaggedError: Error, Encodable {}

// --- [ERRORS] --------------------------------------------------------------------------

public enum Deadline: Error, Equatable {
    case timedOut(Duration)
    case cancelled
}

public struct AggregateError<Element: Sendable>: Error {
    public let first: Element
    public let remaining: [Element]

    public init(first: Element, remaining: [Element] = []) {
        self.first = first
        self.remaining = remaining
    }

    public init?(_ errors: [Element]) {
        guard let first: Element = errors.first else { return nil }
        self.init(first: first, remaining: Array(errors.dropFirst()))
    }

    public var errors: [Element] { [first] + remaining }

    public func map<Next>(_ transform: (Element) -> Next) -> AggregateError<Next> {
        AggregateError<Next>(first: transform(first), remaining: remaining.map(transform))
    }
}

extension AggregateError: CustomNSError, LocalizedError where Element: Error {
    public var errorDescription: String? { errors.map(\.localizedDescription).joined(separator: "\n") }

    public var errorUserInfo: [String: Any] { [NSMultipleUnderlyingErrorsKey: errors] }
}

public struct Unmatched: LocalizedError, TaggedError {
    public let subject: String
    public let query: String?
    public let candidates: [String]

    public init(subject: String, query: String?, candidates: [String]) {
        self.subject = subject
        self.query = query
        self.candidates = candidates
    }

    public var errorDescription: String? {
        "No \(subject)\(query.map { query in " matching \(query)" } ?? "")\(candidates.isEmpty ? "" : ", candidates: \(candidates.joined(separator: ", "))")"
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Result {
    public func aggregated<Next: Sendable>(_ transform: (Failure) -> Next) -> Result<Success, AggregateError<Next>> {
        mapError { error in AggregateError(first: transform(error)) }
    }
}

public func combine<each Value, Failure>(
    _ results: repeat Result<each Value, AggregateError<Failure>>
) -> Result<(repeat each Value), AggregateError<Failure>> {
    var errors: [Failure] = []
    for case .failure(let error) in repeat each results {
        errors += error.errors
    }
    return AggregateError(errors).map(Result.failure)
        ?? Result { () throws(AggregateError<Failure>) in (repeat try (each results).get()) }
}

public func traverse<Element, Value, Failure>(
    _ elements: some Sequence<Element>,
    _ transform: (Element) -> Result<Value, AggregateError<Failure>>,
) -> Result<[Value], AggregateError<Failure>> {
    let split: (values: [Value], errors: [Failure]) = elements.reduce(into: ([], [])) { split, element in
        switch transform(element) {
            case .success(let value): split.values.append(value)
            case .failure(let error): split.errors.append(contentsOf: error.errors)
        }
    }
    return AggregateError(split.errors).map(Result.failure) ?? .success(split.values)
}

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
