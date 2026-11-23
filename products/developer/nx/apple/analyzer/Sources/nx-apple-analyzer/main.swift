import ArgumentParser
import Foundation
import System
import XcodeProjectFormat

// --- [OPERATIONS] ----------------------------------------------------------------------

func report(_ containers: [FilePath]) -> Report {
    let formats = Dictionary(grouping: containers) { container in container.extension.flatMap(ContainerFormat.init(rawValue:)) }
    let projects = formats[.xcodeproj, default: []]
        .map { container in (container, Result { () throws(AnalyzerError) in try analysis(container, decoded(container)) }) }
    let built = Dictionary(uniqueKeysWithValues: projects.compactMap { container, result in (try? result.get()).map { held in (container.string, held.built) } })
    let outcomes =
        projects.map { container, result in (container, Result { () throws(AnalyzerError) in try project(result.get(), built: built) }) }
        + formats[.xcworkspace, default: []]
        .map { container in (container, Result { () throws(AnalyzerError) in try workspace(container, built: built) }) }
    return Report(
        containers: outcomes.compactMap { _, outcome in try? outcome.get() },
        failures: Dictionary(
            uniqueKeysWithValues: outcomes.compactMap { container, outcome in if case .failure(let error) = outcome { (container.string, error.localizedDescription) } else { nil } }
        ),
    )
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
    )
}

struct Analyze: ParsableCommand {
    static let configuration: CommandConfiguration = .init(abstract: "Reports the containers of the given project.xcproj and contents.xcworkspacedata files")

    @Argument(help: "project.xcproj and contents.xcworkspacedata files", transform: { file in FilePath(file).removingLastComponent() })
    var containers: [FilePath]

    func run() throws {
        try FileHandle.standardOutput.write(contentsOf: encoded(report(containers)))
    }
}

struct Version: ParsableCommand {
    static let configuration: CommandConfiguration = .init(
        abstract: "Reports the MARKETING_VERSION of the project.xcproj on stdin, after writing the given one"
    )

    @Option(help: "Version to write as MARKETING_VERSION")
    var marketingVersion: String?

    func run() throws {
        guard let data = try FileHandle.standardInput.readToEnd() else { throw AnalyzerError(subject: "standard input", reason: .empty) }
        let next = try versioned(data, to: marketingVersion)
        guard let project = String(validating: try next.jsonRepresentation(), as: UTF8.self) else { throw CocoaError(.fileWriteInapplicableStringEncoding) }
        try FileHandle.standardOutput.write(contentsOf: encoded(Versioned(version: next.buildSettings[marketingSetting]?.text, project: project)))
    }
}

Analyzer.main()
