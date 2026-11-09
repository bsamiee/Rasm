import Foundation
import System

// --- [MODELS] --------------------------------------------------------------------------

struct Entry: Encodable {
    let id: String
    let container: String
}

struct Scheme: Encodable {
    let name: String
    let entries: [Entry]
    let plans: [String]
    let tests: Bool
}

struct Member: Encodable, Hashable {
    let product: String
    let packages: [String]
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
    let members: [Member]
    let projects: [String]
    let unresolved: [Setting]
}

struct Failure: Encodable {
    let path: String
    let reason: String
}

struct Report: Encodable {
    let containers: [Container]
    let failures: [Failure]
}

struct Reference {
    let owner: String
    let setting: String
    let path: FilePath?

    var missing: String? { path.map { file in exists(file) ? nil : "\(owner) \(setting): \(file.string)" } ?? "\(owner) \(setting) resolves to nothing" }
}

struct Analysis {
    let container: Container
    let references: [Reference]
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
