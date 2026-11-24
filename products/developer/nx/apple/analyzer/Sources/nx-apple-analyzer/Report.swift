import Foundation
import System

// --- [TYPES] ---------------------------------------------------------------------------

enum WorkspaceLocation: String {
    case group
    case container
    case absolute
}

enum ContainerFormat: String {
    case xcodeproj
    case xcworkspace
}

enum WorkspaceElement: String {
    case group = "Group"
    case fileReference = "FileRef"
}

// --- [CONSTANTS] -----------------------------------------------------------------------

let marketingSetting = "MARKETING_VERSION"

// --- [MODELS] --------------------------------------------------------------------------

struct Versioned: Encodable {
    let version: String?
    let project: String
}

struct Archive: Encodable, Equatable {
    let configuration: String
    let bundles: [String]
}

struct Scheme: Encodable, Equatable {
    let name: String
    let containers: [String]
    let plans: [String]
    let tests: Bool
    let launch: String?
    let archive: Archive?
}

struct Container: Encodable, Equatable {
    let path: String
    let schemes: [Scheme]
    let configurations: [String]
    let inputs: [String]
    let projects: [String]
    let products: [String: [String]]
    let buildFiles: [String]
}

struct Report: Encodable, Equatable {
    let containers: [Container]
    let failures: [String: String]
}

struct Built {
    let configuration: String
    let products: [String: [String: String]]
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
        case empty
        case rewritten
    }

    let subject: String
    let reason: Reason

    var errorDescription: String? {
        switch reason {
            case .unreadable(let cause): "\(subject) is unreadable: \(cause.localizedDescription)"
            case .undecodable(let cause): "\(subject) is undecodable: \(cause.localizedDescription)"
            case .missing(let references): (["\(subject) has missing references:"] + references).joined(separator: "\n  ")
            case .empty: "\(subject) is empty"
            case .rewritten: "\(subject) differs from the form Xcode writes, so rewriting it would change more than MARKETING_VERSION"
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
    let expanded = FilePath(NSString(string: relative).expandingTildeInPath)
    let path = expanded.isAbsolute ? expanded : base.appending(relative).lexicallyNormalized()
    return path.isEmpty ? "." : path
}

func checked(_ container: Container, _ references: [Reference]) throws(AnalyzerError) -> Container {
    let missing = Set(references.compactMap(\.missing)).sorted()
    guard missing.isEmpty else { throw AnalyzerError(subject: container.path, reason: .missing(missing)) }
    return container
}
