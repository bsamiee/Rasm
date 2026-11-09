import ArgumentParser
import Foundation
import System
import XcodeProjectFormat

// --- [OPERATIONS] ----------------------------------------------------------------------

func report(_ containers: [FilePath]) -> Report {
    let formats = Dictionary(grouping: containers) { container in container.extension.flatMap(ContainerFormat.init(rawValue:)) }
    let projects: [(FilePath, Result<XCSchema.Project, AnalyzerError>)] = formats[.xcodeproj, default: []]
        .map { container in (container, Result { () throws(AnalyzerError) in try decoded(container) }) }
    let identifiers = Dictionary(
        uniqueKeysWithValues: projects.compactMap { container, result in (try? result.get()).map { held in (container.string, Set(held.targets.map(\.commonProperties.objectID.rawValue))) } }
    )
    let outcomes: [(FilePath, Result<Container, AnalyzerError>)] =
        projects.map { container, result in (container, Result { () throws(AnalyzerError) in try project(container, result.get(), identifiers: identifiers) }) }
        + formats[.xcworkspace, default: []]
        .map { container in (container, Result { () throws(AnalyzerError) in try workspace(container, identifiers: identifiers) }) }
    return Report(
        containers: outcomes.compactMap { _, outcome in try? outcome.get() },
        failures: Dictionary(
            uniqueKeysWithValues: outcomes.compactMap { container, outcome in if case .failure(let error) = outcome { (container.string, error.localizedDescription) } else { nil } }
        ),
    )
}

func input() throws -> Data {
    guard let data: Data = try FileHandle.standardInput.readToEnd() else { throw AnalyzerError(subject: "standard input", reason: .empty) }
    return data
}

func encoded(_ value: some Encodable) throws -> Data {
    let encoder = JSONEncoder()
    encoder.outputFormatting = .sortedKeys
    return try encoder.encode(value)
}

// --- [COMPOSITION] ---------------------------------------------------------------------

struct Analyzer: ParsableCommand {
    static let configuration: CommandConfiguration = .init(
        commandName: "nx-apple-analyzer",
        abstract: "Reads Xcode projects, workspaces, and schemes for @rasm/nx-apple",
        subcommands: [Analyze.self, Version.self],
        defaultSubcommand: Analyze.self,
    )
}

struct Analyze: ParsableCommand {
    static let configuration: CommandConfiguration = .init(abstract: "Reports the containers whose files stdin lists as a JSON array")

    func run() throws {
        FileHandle.standardOutput.write(try encoded(report(try JSONDecoder().decode([String].self, from: input()).map { file in FilePath(file).removingLastComponent() })))
    }
}

struct Version: ParsableCommand {
    static let configuration: CommandConfiguration = .init(
        abstract: "Reports the MARKETING_VERSION of the project.xcproj on stdin, after writing the given one and the next CURRENT_PROJECT_VERSION"
    )

    @Option(help: "Version to write as MARKETING_VERSION, raising CURRENT_PROJECT_VERSION by one")
    var marketingVersion: String?

    func run() throws {
        let held = try XCSchema.Project(jsonRepresentation: input())
        let next: XCSchema.Project = try marketingVersion.map { version throws(AnalyzerError) in try versioned(held, to: version) } ?? held
        guard let project = String(validating: try next.jsonRepresentation(), as: UTF8.self) else { throw CocoaError(.fileWriteInapplicableStringEncoding) }
        FileHandle.standardOutput.write(try encoded(Versioned(version: next.buildSettings[VersionSetting.marketing.rawValue]?.text, project: project)))
    }
}

Analyzer.main()
