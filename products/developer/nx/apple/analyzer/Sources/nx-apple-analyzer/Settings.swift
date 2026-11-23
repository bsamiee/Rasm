import Foundation
import System
import XcodeProjectFormat

// --- [TYPES] ---------------------------------------------------------------------------

enum Resolved: String, CaseIterable {
    case infoPlist = "INFOPLIST_FILE"
    case entitlements = "CODE_SIGN_ENTITLEMENTS"
    case bundleIdentifier = "PRODUCT_BUNDLE_IDENTIFIER"
    case team = "DEVELOPMENT_TEAM"

    var locates: Bool {
        switch self {
            case .infoPlist, .entitlements: true
            case .bundleIdentifier, .team: false
        }
    }
}

enum Builtin: String, CaseIterable {
    case sourceRoot = "SRCROOT"
    case projectDirectory = "PROJECT_DIR"
    case sourceRootAlias = "SOURCE_ROOT"
    case projectName = "PROJECT_NAME"
    case targetName = "TARGET_NAME"
    case configuration = "CONFIGURATION"
}

// --- [CONSTANTS] -----------------------------------------------------------------------

let skipInstall = "SKIP_INSTALL"
let enabled = "YES"
let configurationCondition = "config"
let expansionDepth = 8

// --- [MODELS] --------------------------------------------------------------------------

struct Assignment {
    let name: String
    let conditions: [String: String]
    let value: String

    init?(key: String, value: String) {
        guard let match = key.wholeMatch(of: #/(?<name>[A-Za-z_][A-Za-z0-9_]*)(?<conditions>(?:\[[^\]=]+=[^\]]*\])*)/#) else { return nil }
        name = String(match.output.name)
        conditions = Dictionary(
            match.output.conditions.matches(of: #/\[(?<key>[^\]=]+)=(?<value>[^\]]*)\]/#).map { condition in (String(condition.output.key), String(condition.output.value)) }
        ) { _, last in last }
        self.value = value
    }

    init?(key: String, setting: XCSchema.BuildSetting) {
        switch setting {
            case .string(let text): self.init(key: key, value: text)
            case .array(let items): self.init(key: key, value: items.joined(separator: " "))
        }
    }

    func applies(to configuration: String) -> Bool {
        conditions.allSatisfy { key, pattern in key == configurationCondition && NSPredicate(format: "SELF LIKE %@", argumentArray: [pattern]).evaluate(with: configuration) }
    }
}

struct Source {
    let reference: Reference
    let assignments: [Assignment]
}

struct Settings {
    let layers: [[Assignment]]
    let configuration: String
    private let builtins: [String: String]

    init(layers: [[Assignment]], container: FilePath, target: String, configuration: String) {
        self.layers = layers
        self.configuration = configuration
        builtins = Dictionary(
            uniqueKeysWithValues: Builtin.allCases.map { builtin in
                let value: String =
                    switch builtin {
                        case .sourceRoot, .projectDirectory, .sourceRootAlias: "."
                        case .projectName: container.stem ?? container.string
                        case .targetName: target
                        case .configuration: configuration
                    }
                return (builtin.rawValue, value)
            }
        )
    }

    func value(of name: String) -> String? {
        layers.reversed().lazy.compactMap { layer in
            let matching = layer.filter { assignment in assignment.name == name && assignment.applies(to: configuration) }
            return (matching.last { assignment in !assignment.conditions.isEmpty } ?? matching.last)?.value
        }.first
    }

    func resolved(_ name: String) -> String?? {
        value(of: name).map { text in expanded(text, within: expansionDepth) }
    }

    private func expanded(_ text: String, within depth: Int) -> String? {
        let macro = #/\$[({](?<name>[A-Za-z0-9_]*)[)}]/#
        let names = Set(text.matches(of: macro).map { match in String(match.output.name) })
        let values = names.compactMap { name in
            (name.isEmpty ? "" : (builtins[name] ?? value(of: name)).flatMap { inner in depth > 0 ? expanded(inner, within: depth - 1) : nil }).map { held in (name, held) }
        }
        guard values.count == names.count else { return nil }
        let table = Dictionary(uniqueKeysWithValues: values)
        return text.replacing(macro) { match in table[String(match.output.name), default: ""] }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

func assignments(_ settings: [String: XCSchema.BuildSetting]) -> [Assignment] {
    settings.sorted { left, right in left.key < right.key }.compactMap { key, setting in Assignment(key: key, setting: setting) }
}

func included(_ file: FilePath?, owner: String, setting: String, seen: Set<FilePath> = []) throws(AnalyzerError) -> [Source] {
    switch file {
        case nil: return [Source(reference: Reference(owner: owner, setting: setting, path: nil), assignments: [])]
        case let held? where seen.contains(held): return []
        case let held? where !exists(held): return [Source(reference: Reference(owner: owner, setting: setting, path: held), assignments: [])]
        case let held?:
            guard let text = String(bytes: try read(held), encoding: .utf8) else {
                throw AnalyzerError(subject: held.string, reason: .undecodable(CocoaError(.fileReadInapplicableStringEncoding)))
            }
            let lines = text.split(whereSeparator: \.isNewline).map { (line: Substring) -> Substring in line.firstRange(of: "//").map { comment in line[..<comment.lowerBound] } ?? line }
            let includes = try lines.compactMap { line in line.wholeMatch(of: #/\s*#include(?<optional>\?)?\s*"(?<path>[^"]+)"\s*/#) }.map { match in
                (optional: match.output.optional != nil, at: joined(held.removingLastComponent(), String(match.output.path)))
            }
            .filter { include in !include.optional || exists(include.at) }
            .map { (include: (optional: Bool, at: FilePath)) throws(AnalyzerError) in try included(include.at, owner: held.string, setting: "#include", seen: seen.union([held])) }
            let own = lines.compactMap { line in line.wholeMatch(of: #/\s*(?<key>[A-Za-z_][A-Za-z0-9_]*(?:\[[^\]]*\])*)\s*=\s*(?<value>.*?)\s*;?\s*/#) }.compactMap { match in
                Assignment(key: String(match.output.key), value: String(match.output.value))
            }
            return includes.joined() + [Source(reference: Reference(owner: owner, setting: setting, path: held), assignments: own)]
    }
}
