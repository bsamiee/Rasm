public import AppKit
public import Applications
import Carbon.OpenScripting
import CasePathsCore
public import Results

// --- [ERRORS] --------------------------------------------------------------------------

public enum DefinitionFailure: CustomNSError, LocalizedError {
    case requiresRunning(URL)
    case osa(OSStatus)
    case xml(any Error)
    case unmatched(Unmatched)

    public var errorDescription: String? {
        switch self {
            case .requiresRunning(let url): "\(url.path(percentEncoded: false)) has no OSAScriptingDefinition, launch it to read its dictionary"
            case .unmatched(let miss): miss.errorDescription
            case .osa(let status): "OSACopyScriptingDefinitionFromURL failed (\(status))"
            case .xml: "Scripting definition XML unreadable"
        }
    }

    public var errorUserInfo: [String: Any] {
        switch self {
            case .xml(let error), .unmatched(let error as any Error): [NSUnderlyingErrorKey: error]
            case .requiresRunning, .osa: [:]
        }
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

public actor ScriptingDefinitions {
    private struct Held {
        let processes: Set<pid_t>
        let reading: Task<Result<ScriptingDictionary, DefinitionFailure>, Never>
    }

    private var held: [URL: Held] = [:]

    public init() {}

    public func dictionary(of installation: Installation) async -> Result<ScriptingDictionary, DefinitionFailure> {
        let processes: Set<pid_t> = Set(NSRunningApplication.instances(at: installation.url).map(\.processIdentifier))
        let reading: Task<Result<ScriptingDictionary, DefinitionFailure>, Never> =
            switch held[installation.url] {
                case .some(let hit) where hit.processes == processes: hit.reading
                case .some, .none:
                    Task { await installation.scriptingDefinition().flatMap { data in Result { () throws(DefinitionFailure) in try ScriptingDictionary(definition: data) } } }
            }
        held[installation.url] = Held(processes: processes, reading: reading)
        let read: Result<ScriptingDictionary, DefinitionFailure> = await reading.value
        if read.is(\.failure), held[installation.url]?.reading == reading { held[installation.url] = nil }
        return read
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Installation {
    @concurrent
    public func scriptingDefinition() async -> Result<Data, DefinitionFailure> {
        await Result { () async throws(DefinitionFailure) in
            guard !NSRunningApplication.instances(at: url).isEmpty || scriptingDefinitionFile != nil else { throw .requiresRunning(url) }
            return try await withTaskExecutorPreference(appleEvents) { () throws(DefinitionFailure) in
                var definition: Unmanaged<CFData>?
                let status: OSStatus = unsafe OSACopyScriptingDefinitionFromURL(url as CFURL, 0, &definition)
                guard status == noErr, let data: CFData = unsafe definition?.takeRetainedValue() else { throw .osa(status) }
                return data as Data
            }
        }
    }
}
