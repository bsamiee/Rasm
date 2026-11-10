import Foundation
import System

// --- [TYPES] ---------------------------------------------------------------------------

enum VersionSetting: String {
    case marketing = "MARKETING_VERSION"
    case project = "CURRENT_PROJECT_VERSION"
}

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

// --- [MODELS] --------------------------------------------------------------------------

struct Versioned: Encodable {
    let version: String?
    let project: String
}

struct Scheme: Encodable {
    let name: String
    let containers: [String]
    let plans: [String]
    let tests: Bool
    let launch: String?
}

struct Container: Encodable {
    let path: String
    let schemes: [Scheme]
    let configurations: [String]
    let inputs: [String]
    let projects: [String]
    let buildFiles: [String]
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
        case nonInteger(String)
        case empty
    }

    let subject: String
    let reason: Reason

    var errorDescription: String? {
        switch reason {
            case .unreadable(let cause): "\(subject) is unreadable: \(cause.localizedDescription)"
            case .undecodable(let cause): "\(subject) is undecodable: \(cause.localizedDescription)"
            case .missing(let references): (["\(subject) has missing references:"] + references).joined(separator: "\n  ")
            case .nonInteger(let value): "\(subject) holds \(value), not an integer"
            case .empty: "\(subject) is empty"
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
    return expanded.isAbsolute ? expanded : base.appending(relative).lexicallyNormalized()
}

func checked(_ container: Container, _ references: [Reference]) throws(AnalyzerError) -> Container {
    let missing = Set(references.compactMap(\.missing)).sorted()
    guard missing.isEmpty else { throw AnalyzerError(subject: container.path, reason: .missing(missing)) }
    return container
}
