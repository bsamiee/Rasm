import Foundation
import System

// --- [MODELS] --------------------------------------------------------------------------

struct Scheme: Encodable {
    let name: String
    let containers: [String]
    let plans: [String]
    let tests: Bool
}

struct Setting: Encodable {
    let target: String
    let setting: String
    let value: String
}

struct Container: Encodable {
    let path: String
    let schemes: [Scheme]
    let inputs: [String]
    let members: [String: [String]]
    let projects: [String]
    let unresolved: [Setting]
}

struct Report: Encodable {
    let containers: [Container]
    let failures: [String: String]
}

struct Reference {
    let owner: String
    let setting: String
    let path: FilePath?

    var missing: String? { path.map { file in exists(file) ? nil : "\(owner) \(setting): \(file.string)" } ?? "\(owner) \(setting) resolves to nothing" }
}

// --- [ERRORS] --------------------------------------------------------------------------

struct AnalyzerError: LocalizedError {
    enum Reason: Sendable {
        case unreadable(any Error)
        case undecodable(any Error)
        case missing([String])
    }

    let subject: String
    let reason: Reason

    var errorDescription: String? {
        switch reason {
            case .unreadable(let cause): "\(subject) is unreadable: \(cause.localizedDescription)"
            case .undecodable(let cause): "\(subject) is undecodable: \(cause.localizedDescription)"
            case .missing(let references): (["\(subject) references what does not exist:"] + references).joined(separator: "\n  ")
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

func exists(_ path: FilePath) -> Bool { FileManager.default.fileExists(atPath: path.string) }

func read(_ path: FilePath) throws(AnalyzerError) -> Data {
    do {
        return try Data(contentsOf: URL(filePath: path.string))
    } catch {
        throw AnalyzerError(subject: path.string, reason: .unreadable(error))
    }
}

func joined(_ base: FilePath, _ relative: String) -> FilePath {
    relative.hasPrefix("/") || relative.hasPrefix("~") ? FilePath(NSString(string: relative).expandingTildeInPath) : base.appending(relative).lexicallyNormalized()
}

func checked(_ container: Container, _ references: [Reference]) throws(AnalyzerError) -> Container {
    let missing = Set(references.compactMap(\.missing)).sorted()
    guard missing.isEmpty else { throw AnalyzerError(subject: container.path, reason: .missing(missing)) }
    return container
}
