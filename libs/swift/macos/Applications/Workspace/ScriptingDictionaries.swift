import AppKit
public import AppleEvents
import CasePathsCore
public import Foundation
import Processes

// --- [SERVICES] ------------------------------------------------------------------------

public actor ScriptingDictionaries {
    private let cachedDefinition: @Sendable (Installation) -> URL?
    private var held: [Installation: (processes: Set<ProcessInstance>, reading: Task<Result<ScriptingDictionary, AppleEventFailure>, Never>)] = [:]

    public init(cachedDefinition: @escaping @Sendable (Installation) -> URL?) {
        self.cachedDefinition = cachedDefinition
    }

    public func dictionary(of installation: Installation) async throws(AppleEventFailure) -> ScriptingDictionary {
        let processes: Set<ProcessInstance>
        do throws(ApplicationFailure) {
            processes = Set(try NSWorkspace.shared.instances(of: installation.runs).map(\.process))
        } catch {
            throw AppleEventFailure(subject: error.subject, reason: .foundation(error as NSError))
        }
        let reading =
            switch held[installation] {
                case .some(let hit) where hit.processes == processes: hit.reading
                case .some, .none:
                    Task { [cachedDefinition] in
                        await Result { () async throws(AppleEventFailure) in
                            try ScriptingDictionary(definition: await installation.scriptingDefinition(cachedDefinition: cachedDefinition(installation)))
                        }
                    }
            }
        held[installation] = (processes, reading)
        let read = await reading.value
        if read.is(\.failure), held[installation]?.reading == reading { held[installation] = nil }
        return try read.get()
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Installation {
    func scriptingDefinition(cachedDefinition: URL?) async throws(AppleEventFailure) -> Data {
        let path = url.path(percentEncoded: false)
        func read(_ file: URL?, named name: String) throws(AppleEventFailure) -> Data {
            do {
                guard let file else { throw CocoaError(.fileReadNoSuchFile, userInfo: [NSFilePathErrorKey: name]) }
                return try Data(contentsOf: file)
            } catch {
                throw AppleEventFailure(subject: "\(name) of \(path)", reason: .foundation(error as NSError))
            }
        }
        let bundle = Bundle(url: url)
        let suited = bundle.flatMap { bundle in bundle.url(forResource: nil, withExtension: "scriptSuite").map { _ in bundle } }
        let resource = executable.flatMap { executable in bundle?.url(forResource: executable.lastPathComponent, withExtension: "rsrc") }
        let live = scriptingDefinitionIsDynamic || (scriptingDefinitionFile == nil && suited == nil && resource == nil)
        let cache = Bundle.main.bundleIdentifier.flatMap { server in build.map { build in URL.cachesDirectory.appending(components: server, identifier, "\(build).sdef") } }
        let cached = [cache, cachedDefinition].compactMap(\.self).first { file in FileManager.default.fileExists(atPath: file.path(percentEncoded: false)) }
        let running = live ? NSWorkspace.shared.runningApplications.first(where: runs)?.processIdentifier : nil
        let asked: Result<Data, AppleEventFailure>? =
            if let running {
                await Result { () async throws(AppleEventFailure) in try await ScriptingDictionary.definition(of: NSAppleEventDescriptor(processIdentifier: running), within: messagingTimeout) }
            } else { nil }
        switch (asked, cached, scriptingDefinitionFile, suited, resource) {
            case (.some(.success(let definition)), _, _, _, _):
                do {
                    if let cache {
                        try FileManager.default.createDirectory(at: cache.deletingLastPathComponent(), withIntermediateDirectories: true)
                        try definition.write(to: cache, options: .atomic)
                    }
                } catch {
                    throw AppleEventFailure(subject: "cached definition of \(path)", reason: .foundation(error as NSError))
                }
                return definition
            case (.some(.failure(let failure)), _, _, _, _) where failure.reason.is(\.cancelled): throw failure
            case (_, .some(let cached), _, _, _) where live: return try read(cached, named: cached.path(percentEncoded: false))
            case (_, _, .some(let name), _, _): return try read(bundle?.url(forResource: name, withExtension: URL(filePath: name).pathExtension.isEmpty ? "sdef" : nil), named: name)
            case (_, _, .none, .some(let suited), _): return try ScriptingDictionary.definition(suites: suited)
            case (_, _, .none, .none, .some(let resource)): return try await ScriptingDictionary.definition(terminology: resource)
            case (.some(.failure(let failure)), _, .none, .none, .none): throw failure
            case (.none, _, .none, .none, .none): throw AppleEventFailure(subject: path, reason: .undeclared)
        }
    }
}
