import AppKit
public import AppleEvents
import CasePathsCore
public import Foundation
import Processes

// --- [SERVICES] ------------------------------------------------------------------------

public actor ScriptingDefinitions {
    private let cached: @Sendable (Installation) -> URL?
    private var held: [URL: (processes: Set<ProcessInstance>, reading: Task<Result<ScriptingDictionary, AppleEventFailure>, Never>)] = [:]

    public init(cached: @escaping @Sendable (Installation) -> URL?) {
        self.cached = cached
    }

    public func dictionary(of installation: Installation) async throws(AppleEventFailure) -> ScriptingDictionary {
        let processes = Set(NSWorkspace.shared.instances(of: installation.runs).map(\.process))
        let reading =
            switch held[installation.url] {
                case .some(let hit) where hit.processes == processes: hit.reading
                case .some, .none:
                    Task { [cached] in
                        await Result { () async throws(AppleEventFailure) in
                            try ScriptingDictionary(definition: await installation.scriptingDefinition(cached: cached(installation)))
                        }
                    }
            }
        held[installation.url] = (processes, reading)
        let read = await reading.value
        if read.is(\.failure), held[installation.url]?.reading == reading { held[installation.url] = nil }
        return try read.get()
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Installation {
    public func scriptingDefinition(cached: URL?) async throws(AppleEventFailure) -> Data {
        let path = url.path(percentEncoded: false)
        func read(_ file: URL?, named name: String) throws(AppleEventFailure) -> Data {
            do {
                guard let file else { throw CocoaError(.fileReadNoSuchFile, userInfo: [NSFilePathErrorKey: name]) }
                return try Data(contentsOf: file)
            } catch {
                throw AppleEventFailure(subject: "\(name) of \(path)", reason: .host(error as NSError))
            }
        }
        return switch (scriptingDefinitionIsDynamic ? nil : scriptingDefinitionFile, NSWorkspace.shared.runningApplications.contains(where: runs), cached) {
            case (.some(let name), _, _): try read(Bundle(url: url)?.url(forResource: name, withExtension: URL(filePath: name).pathExtension.isEmpty ? "sdef" : nil), named: name)
            case (.none, true, _): try await ScriptingDictionary.definition(of: url)
            case (.none, false, .some(let cached)): try read(cached, named: cached.path(percentEncoded: false))
            case (.none, false, .none): throw AppleEventFailure(subject: path, reason: .undeclared)
        }
    }
}
