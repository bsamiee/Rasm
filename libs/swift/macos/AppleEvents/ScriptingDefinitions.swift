public import AppKit
public import Applications
import Carbon.OpenScripting
public import Results

// --- [ERRORS] --------------------------------------------------------------------------

public enum DefinitionFailure: CustomNSError, LocalizedError {
    case requiresRunning(URL)
    case launch(LaunchFailure)
    case osa(OSStatus)
    case xml(any Error)
    case unmatched(Unmatched)

    public var errorDescription: String? {
        switch self {
            case .requiresRunning(let url): "\(url.path(percentEncoded: false)) declares no scripting definition file, launch it to read its dynamic dictionary"
            case .launch(let error as any Error), .unmatched(let error as any Error): error.localizedDescription
            case .osa(let status): "Open Scripting read no scripting definition (\(status))"
            case .xml(let error): "The scripting definition is not readable XML: \(error.localizedDescription)"
        }
    }

    public var errorUserInfo: [String: Any] {
        switch self {
            case .launch(let error as any Error), .xml(let error), .unmatched(let error as any Error): [NSUnderlyingErrorKey: error]
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

    public func dictionary(of installation: Installation, within timeout: Duration) async -> Result<ScriptingDictionary, DefinitionFailure> {
        let processes: Set<pid_t> = Set(installation.instances)
        let reading: Task<Result<ScriptingDictionary, DefinitionFailure>, Never> =
            switch held[installation.url] {
                case .some(let hit) where hit.processes == processes: hit.reading
                case .some, .none:
                    Task { await installation.scriptingDefinition(within: timeout).flatMap { data in Result { () throws(DefinitionFailure) in try ScriptingDictionary(definition: data) } } }
            }
        held[installation.url] = Held(processes: processes, reading: reading)
        let read: Result<ScriptingDictionary, DefinitionFailure> = await reading.value
        if case .failure = read, held[installation.url]?.reading == reading { held[installation.url] = nil }
        return read
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Installation {
    @concurrent
    public func scriptingDefinition(within timeout: Duration) async -> Result<Data, DefinitionFailure> {
        await Result { () async throws(DefinitionFailure) in
            guard !instances.isEmpty || scriptingDefinitionFile != nil else { throw .requiresRunning(url) }
            for instance: NSRunningApplication in instances.compactMap(NSRunningApplication.init(processIdentifier:)) {
                try await instance.finishedLaunching(within: timeout).mapError(DefinitionFailure.launch).get()
            }
            return try await withTaskExecutorPreference(appleEvents) { () throws(DefinitionFailure) in
                var definition: Unmanaged<CFData>?
                let status: OSStatus = unsafe OSACopyScriptingDefinitionFromURL(url as CFURL, 0, &definition)
                guard status == noErr, let data: CFData = unsafe definition?.takeRetainedValue() else { throw .osa(status) }
                return data as Data
            }
        }
    }
}
