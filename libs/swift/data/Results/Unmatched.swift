package import Foundation

// --- [ERRORS] --------------------------------------------------------------------------

package struct Unmatched: Encodable, Equatable, LocalizedError {
    package let subject: String
    package let query: String?
    package let candidates: [String]

    package init(subject: String, query: String?, candidates: [String]) {
        self.subject = subject
        self.query = query
        self.candidates = candidates
    }

    package var errorDescription: String? {
        switch (query, candidates.isEmpty) {
            case (.some(let query), true): "No \(subject) matching \(query)"
            case (.some(let query), false): "No \(subject) matching \(query), candidates: \(candidates.formatted())"
            case (.none, true): "No \(subject)"
            case (.none, false): "No \(subject), candidates: \(candidates.formatted())"
        }
    }

    package static func single<Match>(_ matches: [Match], subject: String, query: String?, candidates: @autoclosure () -> [String], named: (Match) -> String) -> Result<Match, Self> {
        switch (matches.first, matches.count) {
            case (.some(let match), 1): .success(match)
            case (.none, _): .failure(Self(subject: subject, query: query, candidates: candidates()))
            case (.some, _): .failure(Self(subject: "single \(subject)", query: query, candidates: matches.map(named)))
        }
    }
}
