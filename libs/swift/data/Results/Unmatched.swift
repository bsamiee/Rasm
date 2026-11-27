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
        let refused: String = query.map { query in "No \(subject) matching \(query)" } ?? "No \(subject)"
        let nearest: [String] = query.map { query in candidates.filter { candidate in candidate.localizedStandardContains(query) } } ?? []
        let listing: ([String], Int) -> String = { (items: [String], limit: Int) -> String in
            let more: [String] = items.count > limit ? ["\(items.count - limit) more"] : []
            let shown: [String] = Array(items.prefix(limit)) + more
            return shown.formatted(.list(type: .and).locale(Locale(identifier: "en")))
        }
        let listed: String =
            switch (query, nearest.isEmpty, candidates.count > 8) {
                case (.none, _, _): "candidates: \(listing(candidates, 100))"
                case (.some, false, _): "nearest: \(listing(nearest, 8))"
                case (.some, true, true): "none of \(candidates.count) candidates is near"
                case (.some, true, false): "candidates: \(listing(candidates, 8))"
            }
        return candidates.isEmpty ? refused : "\(refused), \(listed)"
    }

    package static func single<Match>(_ matches: [Match], subject: String, query: String?, candidates: @autoclosure () -> [String], named: (Match) -> String) -> Result<Match, Self> {
        switch (matches.first, matches.count) {
            case (.some(let match), 1): .success(match)
            case (.none, _): .failure(Self(subject: subject, query: query, candidates: candidates()))
            case (.some, _): .failure(Self(subject: "single \(subject)", query: query, candidates: matches.map(named)))
        }
    }
}
