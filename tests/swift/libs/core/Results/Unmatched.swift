import CasePaths
import CustomDump
import Encoding
import Foundation
import Results
import Synchronization
import Testing

// --- [MODELS] --------------------------------------------------------------------------

private struct Window: Equatable {
    let id: Int
    let title: String
}

private final class Evaluations: Sendable {
    let count: Atomic<Int> = Atomic(0)

    func candidates() -> [String] {
        count.add(1, ordering: .sequentiallyConsistent)
        return ["Inbox", "Drafts"]
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

private func single(_ windows: [Window], query: String?, evaluations: Evaluations) -> Result<Window, Unmatched> {
    Unmatched.single(windows, subject: "window", query: query, candidates: evaluations.candidates(), named: \.title)
}

private func wire(_ unmatched: Unmatched) throws -> JSONValue {
    let encoder: JSONEncoder = JSONEncoder()
    encoder.outputFormatting = .sortedKeys
    let encoded: Data = try encoder.encode(unmatched)
    return switch try JSONDecoder().decode(JSONValue.self, from: encoded) {
        case .object(let fields): .object(fields.filter { key, _ in key != "_tag" })
        case let value: value
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

struct UnmatchedQueries {
    @Test
    func oneMatchSucceedsWithoutListingCandidates() throws {
        let evaluations: Evaluations = Evaluations()
        let window: Window = try single([Window(id: 4, title: "Inbox")], query: "Inbox", evaluations: evaluations).get()
        expectNoDifference(window, Window(id: 4, title: "Inbox"))
        expectNoDifference(evaluations.count.load(ordering: .sequentiallyConsistent), 0)
    }

    @Test
    func noMatchFailsWithSubjectAndCallerCandidates() throws {
        let evaluations: Evaluations = Evaluations()
        let failure: Unmatched = try #require(single([], query: "Sent", evaluations: evaluations)[case: \.failure])
        expectNoDifference(failure.subject, "window")
        expectNoDifference(failure.query, "Sent")
        expectNoDifference(failure.candidates, ["Inbox", "Drafts"])
        expectNoDifference(evaluations.count.load(ordering: .sequentiallyConsistent), 1)
    }

    @Test
    func manyMatchesFailAsAmbiguousListingEachMatchName() throws {
        let evaluations: Evaluations = Evaluations()
        let matches: [Window] = [Window(id: 1, title: "Inbox"), Window(id: 2, title: "Inbox — Archive"), Window(id: 3, title: "Inbox")]
        let failure: Unmatched = try #require(single(matches, query: "Inbox", evaluations: evaluations)[case: \.failure])
        expectNoDifference(failure.subject, "single window")
        expectNoDifference(failure.query, "Inbox")
        expectNoDifference(failure.candidates, ["Inbox", "Inbox — Archive", "Inbox"])
        expectNoDifference(evaluations.count.load(ordering: .sequentiallyConsistent), 0)
    }

    @Test
    func wireHoldsSubjectQueryAndCandidates() throws {
        let unmatched: Unmatched = Unmatched(subject: "single window", query: "Inbox", candidates: ["Inbox", "Drafts"])
        expectNoDifference(
            try wire(unmatched),
            .object(["subject": .string("single window"), "query": .string("Inbox"), "candidates": .array([.string("Inbox"), .string("Drafts")])]),
        )
    }

    @Test
    func wireOmitsAbsentQueryKey() throws {
        let unmatched: Unmatched = Unmatched(subject: "window", query: nil, candidates: [])
        expectNoDifference(try wire(unmatched), .object(["subject": .string("window"), "candidates": .array([])]))
    }
}
