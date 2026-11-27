import AppKit
package import AppleEvents
import CasePathsCore
import Files
package import Foundation
import Processes

// --- [MODELS] --------------------------------------------------------------------------

private enum DefinitionSource {
    case running(pid_t)
    case sdef(URL)
    case scriptSuites(Bundle)
    case aete(URL)
}

// --- [SERVICES] ------------------------------------------------------------------------

package actor ScriptingDictionaries {
    private let cachedDefinition: @Sendable (Installation) -> URL?
    private var held: [URL: (build: String?, processes: Set<ProcessInstance>, reading: Task<Result<ScriptingDictionary, AppleEventFailure>, Never>)] = [:]

    package init(cachedDefinition: @escaping @Sendable (Installation) -> URL?) {
        self.cachedDefinition = cachedDefinition
    }

    package func dictionary(of installation: Installation) async throws(AppleEventFailure) -> ScriptingDictionary {
        let processes: Set<ProcessInstance>
        do throws(ApplicationFailure) {
            processes = Set(try NSWorkspace.shared.instances(of: installation.runs).map(\.process))
        } catch {
            throw AppleEventFailure(subject: error.subject, reason: .foundation(error as NSError))
        }
        let reading =
            switch held[installation.url] {
                case .some(let hit) where hit.build == installation.build && hit.processes == processes: hit.reading
                case .some, .none:
                    Task { [cachedDefinition] in
                        await Result { () async throws(AppleEventFailure) in
                            try ScriptingDictionary(definition: await installation.scriptingDefinition(running: processes.first?.pid, cachedDefinition: cachedDefinition(installation)))
                        }
                    }
            }
        held[installation.url] = (installation.build, processes, reading)
        let read = await reading.value
        if read.is(\.failure), held[installation.url]?.reading == reading { held[installation.url] = nil }
        return try read.get()
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Installation {
    func scriptingDefinition(running: pid_t?, cachedDefinition: URL?) async throws(AppleEventFailure) -> Data {
        let (bundle, named) = (Bundle(url: url), executable?.lastPathComponent)
        let cache = Bundle.main.bundleIdentifier.flatMap { server in build.map { build in URL.cachesDirectory.appending(components: server, identifier, "\(build).sdef") } }
        let statics: [DefinitionSource?] = [
            scriptingDefinitionFile.flatMap { name in bundle?.url(forResource: name, withExtension: URL(filePath: name).pathExtension.isEmpty ? "sdef" : nil) }.map(DefinitionSource.sdef),
            named.flatMap { name in bundle?.url(forResource: name, withExtension: "sdef") }.map(DefinitionSource.sdef),
            bundle?.url(forResource: nil, withExtension: "scriptSuite").flatMap { _ in bundle }.map(DefinitionSource.scriptSuites),
            named.flatMap { name in bundle?.url(forResource: name, withExtension: "rsrc") }.map(DefinitionSource.aete),
        ]
        let live: [DefinitionSource?] = [running.map(DefinitionSource.running), cache.map(DefinitionSource.sdef), cachedDefinition.map(DefinitionSource.sdef)]
        var refusal: AppleEventFailure?
        for source in (scriptingDefinitionIsDynamic ? live + statics : statics + live).compactMap(\.self) {
            let read: Result<Data?, AppleEventFailure> = await Result { () async throws(AppleEventFailure) -> Data? in
                switch source {
                    case .running(let process): return try await ScriptingDictionary.definition(of: NSAppleEventDescriptor(processIdentifier: process), within: messagingTimeout)
                    case .sdef(let file):
                        let contents: Result<Data?, any Error> = Result { () throws -> Data? in try ifPresent { () throws -> Data in try Data(contentsOf: file) } }
                        let refused: Result<Data?, AppleEventFailure> = contents.mapError { (error: any Error) -> AppleEventFailure in
                            AppleEventFailure(subject: file.path(percentEncoded: false), reason: .foundation(error as NSError))
                        }
                        return try refused.get()
                    case .scriptSuites(let bundle): return try ScriptingDictionary.definition(suites: bundle)
                    case .aete(let resource): return try await ScriptingDictionary.definition(terminology: resource)
                }
            }
            switch (read, source, cache) {
                case (.success(nil), _, _): continue
                case (.success(let definition?), .running, .some(let cache)):
                    do {
                        try FileManager.default.createDirectory(at: cache.deletingLastPathComponent(), withIntermediateDirectories: true)
                        try definition.write(to: cache, options: .atomic)
                    } catch {
                        throw AppleEventFailure(subject: cache.path(percentEncoded: false), reason: .foundation(error as NSError))
                    }
                    return definition
                case (.success(let definition?), _, _): return definition
                case (.failure(let failure), _, _) where failure.reason.is(\.cancelled): throw failure
                case (.failure(let failure), _, _) where failure.reason == .osa(OSStatus(resNotFound)): continue
                case (.failure(let failure), _, _): refusal = refusal ?? failure
            }
        }
        throw refusal ?? AppleEventFailure(subject: url.path(percentEncoded: false), reason: .undeclared)
    }
}
