import AppKit
public import Applications
import Carbon.OpenScripting
import CasePaths
public import Foundation

// --- [SERVICES] ------------------------------------------------------------------------

public actor ScriptingDefinitions {
    private var held: [URL: (processes: Set<pid_t>, reading: Task<Result<ScriptingDictionary, AppleEventFailure>, Never>)] = [:]

    public init() {}

    public func dictionary(of installation: Installation) async throws(AppleEventFailure) -> ScriptingDictionary {
        let processes: Set<pid_t> = Set(NSWorkspace.shared.instances(of: installation.runs).map(\.processIdentifier))
        let reading: Task<Result<ScriptingDictionary, AppleEventFailure>, Never> =
            switch held[installation.url] {
                case .some(let hit) where hit.processes == processes: hit.reading
                case .some, .none:
                    Task { await Result { () async throws(AppleEventFailure) in try ScriptingDictionary(definition: await installation.scriptingDefinition()) } }
            }
        held[installation.url] = (processes, reading)
        let read: Result<ScriptingDictionary, AppleEventFailure> = await reading.value
        if read.is(\.failure), held[installation.url]?.reading == reading { held[installation.url] = nil }
        return try read.get()
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Installation {
    @concurrent
    public func scriptingDefinition() async throws(AppleEventFailure) -> Data {
        let path: String = url.path(percentEncoded: false)
        guard !NSWorkspace.shared.instances(of: runs).isEmpty || scriptingDefinitionFile != nil else { throw AppleEventFailure(subject: path, reason: .requiresRunning) }
        return try await withTaskExecutorPreference(appleEvents) { () throws(AppleEventFailure) in
            var definition: Unmanaged<CFData>?
            let status: OSStatus = unsafe OSACopyScriptingDefinitionFromURL(url as CFURL, 0, &definition)
            guard status == noErr, let data: CFData = unsafe definition?.takeRetainedValue() else { throw AppleEventFailure(subject: path, reason: .osa(status)) }
            return data as Data
        }
    }
}
