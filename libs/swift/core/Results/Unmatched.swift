public import Foundation

// --- [ERRORS] --------------------------------------------------------------------------

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

    public static func single<Match>(_ matches: [Match], subject: String, query: String?, candidates: @autoclosure () -> [String], named: (Match) -> String) -> Result<Match, Self> {
        switch (matches.first, matches.count) {
            case (.some(let match), 1): .success(match)
            case (.none, _): .failure(Self(subject: subject, query: query, candidates: candidates()))
            case (.some, _): .failure(Self(subject: "single \(subject)", query: query, candidates: matches.map(named)))
        }
    }
}
