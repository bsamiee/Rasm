import ArgumentParser
import Foundation
import System
import XcodeProjectFormat

// --- [OPERATIONS] ----------------------------------------------------------------------

func report(_ containers: [FilePath]) -> Report {
    let projects: [(FilePath, Result<XCSchema.Project, AnalyzerError>)] = containers.filter { container in container.extension == "xcodeproj" }
        .map { container in (container, Result { () throws(AnalyzerError) in try decoded(container) }) }
    let identifiers: [String: Set<String>] = Dictionary(
        uniqueKeysWithValues: projects.compactMap { container, result in (try? result.get()).map { held in (container.string, Set(held.targets.map(\.commonProperties.objectID.rawValue))) } }
    )
    let outcomes: [(FilePath, Result<Container, AnalyzerError>)] =
        projects.map { container, result in (container, Result { () throws(AnalyzerError) in try project(container, result.get(), identifiers: identifiers) }) }
        + containers.filter { container in container.extension == "xcworkspace" }
        .map { container in (container, Result { () throws(AnalyzerError) in try workspace(container, identifiers: identifiers) }) }
    return Report(
        containers: outcomes.compactMap { _, outcome in try? outcome.get() },
        failures: Dictionary(
            uniqueKeysWithValues: outcomes.compactMap { container, outcome in if case .failure(let error) = outcome { (container.string, error.localizedDescription) } else { nil } }
        ),
    )
}

func written(_ value: some Encodable) throws {
    let encoder: JSONEncoder = JSONEncoder()
    encoder.outputFormatting = .sortedKeys
    FileHandle.standardOutput.write(try encoder.encode(value))
}

// --- [COMPOSITION] ---------------------------------------------------------------------

struct Analyzer: ParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(
        commandName: "nx-apple-analyzer",
        abstract: "Reads Xcode projects, workspaces, and schemes for @rasm/nx-apple",
        subcommands: [Analyze.self, Version.self],
        defaultSubcommand: Analyze.self,
    )
}

struct Analyze: ParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(abstract: "Reports the containers whose files stdin lists as a JSON array")

    func run() throws {
        try written(report(try JSONDecoder().decode([String].self, from: FileHandle.standardInput.readToEnd() ?? Data()).map { file in FilePath(file).removingLastComponent() }))
    }
}

struct Version: ParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(
        abstract: "Reports the MARKETING_VERSION of the project.xcproj on stdin, after writing the given one and the next CURRENT_PROJECT_VERSION"
    )

    @Option(help: "Version to write as MARKETING_VERSION, raising CURRENT_PROJECT_VERSION by one")
    var marketingVersion: String?

    func run() throws {
        let held: XCSchema.Project = try XCSchema.Project(jsonRepresentation: FileHandle.standardInput.readToEnd() ?? Data())
        let next: XCSchema.Project = try marketingVersion.map { version throws(AnalyzerError) in try versioned(held, to: version) } ?? held
        guard let project = String(bytes: try next.jsonRepresentation(), encoding: .utf8) else { throw CocoaError(.fileWriteInapplicableStringEncoding) }
        try written(Versioned(version: next.buildSettings[VersionSetting.marketing.rawValue]?.text, project: project))
    }
}

Analyzer.main()
