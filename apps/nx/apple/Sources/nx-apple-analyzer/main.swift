import Foundation
import System
import XcodeProjectFormat

// --- [OPERATIONS] ----------------------------------------------------------------------

func report(_ containers: [FilePath]) -> Report {
    let projects = containers.filter { container in container.extension == "xcodeproj" }.map { container in (container, Result { () throws(AnalyzerError) in try decoded(container) }) }
    let identifiers = Dictionary(
        uniqueKeysWithValues: projects.compactMap { container, result in (try? result.get()).map { project in (container.string, Set(project.targets.map(\.commonProperties.objectID.rawValue))) } }
    )
    let outcomes =
        projects.map { container, result in (container, Result { () throws(AnalyzerError) in try project(container, result.get(), identifiers: identifiers) }) }
        + containers.filter { container in container.extension == "xcworkspace" }.map { container in (container, Result { () throws(AnalyzerError) in try workspace(container, identifiers: identifiers) }) }
    return Report(
        containers: outcomes.compactMap { _, outcome in try? outcome.get() },
        failures: Dictionary(
            uniqueKeysWithValues: outcomes.compactMap { container, outcome in if case .failure(let error) = outcome { (container.string, error.localizedDescription) } else { nil } }
        ),
    )
}

// --- [COMPOSITION] ---------------------------------------------------------------------

do {
    let containers = try JSONDecoder().decode([String].self, from: FileHandle.standardInput.readToEnd() ?? Data()).map { file in FilePath(file).removingLastComponent() }
    let encoder = JSONEncoder()
    encoder.outputFormatting = .sortedKeys
    FileHandle.standardOutput.write(try encoder.encode(report(containers)))
} catch {
    FileHandle.standardError.write(Data(error.localizedDescription.utf8))
    exit(1)
}
