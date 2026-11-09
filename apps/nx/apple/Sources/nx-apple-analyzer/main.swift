import Foundation
import System
import XcodeProjectFormat

// --- [OPERATIONS] ----------------------------------------------------------------------

func checked(_ analysis: Analysis) throws(AnalyzerError) -> Container {
    let missing = Set(analysis.references.compactMap(\.missing)).sorted()
    guard missing.isEmpty else { throw AnalyzerError(subject: analysis.container.path, reason: .missing(missing)) }
    return analysis.container
}

func report(_ containers: [FilePath]) -> Report {
    let projects = containers.filter { container in container.extension == "xcodeproj" }.map { container in (container, Result { () throws(AnalyzerError) in try decoded(container) }) }
    let identifiers = Dictionary(
        uniqueKeysWithValues: projects.compactMap { container, result in (try? result.get()).map { project in (container.string, Set(project.targets.map(\.commonProperties.objectID.rawValue))) } }
    )
    let outcomes =
        projects.map { container, result in (container, Result { () throws(AnalyzerError) in try checked(project(container, result.get(), identifiers: identifiers)) }) }
        + containers.filter { container in container.extension == "xcworkspace" }.map { container in (container, Result { () throws(AnalyzerError) in try checked(workspace(container)) }) }
    return Report(
        containers: outcomes.compactMap { _, outcome in try? outcome.get() },
        failures: outcomes.compactMap { container, outcome in if case .failure(let error) = outcome { Failure(path: container.string, reason: error.localizedDescription) } else { nil } },
    )
}

// --- [COMPOSITION] ---------------------------------------------------------------------

do {
    let containers = try JSONDecoder().decode([String].self, from: FileHandle.standardInput.readToEnd() ?? Data()).map { file in FilePath(file).removingLastComponent() }
    FileHandle.standardOutput.write(try JSONEncoder().encode(report(containers)))
} catch {
    FileHandle.standardError.write(Data(error.localizedDescription.utf8))
    exit(1)
}
