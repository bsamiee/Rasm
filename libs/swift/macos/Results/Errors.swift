public import Foundation

// --- [TYPES] ---------------------------------------------------------------------------

public protocol DeadlineFailure: Error {
    static func timedOut(_ timeout: Duration) -> Self
    static var cancelled: Self { get }
}

// --- [ERRORS] --------------------------------------------------------------------------

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

public struct Unmatched: LocalizedError {
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
