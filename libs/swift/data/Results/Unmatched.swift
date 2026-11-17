public import Foundation

// --- [ERRORS] --------------------------------------------------------------------------

public struct Unmatched: Encodable, Equatable, LocalizedError {
    public let subject: String
    public let query: String?
    public let candidates: [String]

    public init(subject: String, query: String?, candidates: [String]) {
        self.subject = subject
        self.query = query
        self.candidates = candidates
    }

    private enum CodingKeys: CodingKey {
        case subject, query, candidates, message
    }

    public func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: CodingKeys.self)
        try fields.encode(subject, forKey: .subject)
        try fields.encodeIfPresent(query, forKey: .query)
        try fields.encode(candidates, forKey: .candidates)
        try fields.encodeIfPresent(errorDescription, forKey: .message)
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
