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
            case .launch(let error as any Error), .xml(let error): [NSUnderlyingErrorKey: error]
            case .requiresRunning, .osa, .unmatched: [:]
        }
    }
}

public enum DictionaryFailure: CustomNSError, LocalizedError {
    case unmatched(Unmatched)
    case unencodable(DescType)
    case undated(String, any Error)

    public var errorDescription: String? {
        switch self {
            case .unmatched(let miss): miss.errorDescription
            case .unencodable(let type): "No Apple Event descriptor of type \(type.sdef) holds the value"
            case .undated(let text, let error): "\(text) is no ISO 8601 date: \(error.localizedDescription)"
        }
    }

    public var errorUserInfo: [String: Any] {
        if case .undated(_, let error) = self { [NSUnderlyingErrorKey: error] } else { [:] }
    }
}

public enum AppleEventFailure: LocalizedError {
    case undelivered(OSStatus)
    case unanswered(OSStatus)
    case replied(OSStatus, String?)

    public var errorDescription: String? {
        switch self {
            case .undelivered(OSStatus(errAEEventNotPermitted)):
                "macOS Automation does not let this process control the application, nothing ran: allow it under System Settings, Privacy & Security, Automation"
            case .undelivered(let status): "The application did not receive the event (\(status)), nothing ran"
            case .unanswered(let status): "The application did not answer the event (\(status)), its outcome in the application is unknown"
            case .replied(let status, let message): "\(message ?? "The application answered with an error") (\(status))"
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
            return try await withTaskExecutorPreference(DispatchQueue(label: "AppleEvents.definitions", attributes: .concurrent)) { () throws(DefinitionFailure) in
                var definition: Unmanaged<CFData>?
                let status: OSStatus = unsafe OSACopyScriptingDefinitionFromURL(url as CFURL, 0, &definition)
                guard status == noErr, let data: CFData = unsafe definition?.takeRetainedValue() else { throw .osa(status) }
                return data as Data
            }
        }
    }
}

extension NSAppleEventDescriptor {
    public func sent(within timeout: Duration) async -> Result<NSAppleEventDescriptor, AppleEventFailure> {
        let undelivered: Set<OSStatus> = [OSStatus(procNotFound), OSStatus(errAEEventNotPermitted)]
        return await withTaskExecutorPreference(DispatchQueue(label: "AppleEvents.sending", attributes: .concurrent)) {
            Result { try sendEvent(options: [.waitForReply, .neverInteract], timeout: timeout / .seconds(1)) }
                .mapError { error -> AppleEventFailure in
                    let status: OSStatus = OSStatus((error as NSError).code)
                    return undelivered.contains(status) ? .undelivered(status) : .unanswered(status)
                }
                .flatMap { answer in
                    switch answer.paramDescriptor(forKeyword: AEKeyword(keyErrorNumber))?.int32Value {
                        case .some(let code) where undelivered.contains(code): .failure(.undelivered(code))
                        case .some(let code) where code != noErr: .failure(.replied(code, answer.paramDescriptor(forKeyword: AEKeyword(keyErrorString))?.stringValue))
                        case .some, .none: .success(answer.paramDescriptor(forKeyword: AEKeyword(keyDirectObject)) ?? .null())
                    }
                }
        }
    }
}

extension FixedWidthInteger where Self: UnsignedInteger {
    public init?(sdef text: String) {
        switch (text.wholeMatch(of: /0x([0-9A-Fa-f]+)/)?.1, text.data(using: .macOSRoman)) {
            case (.some(let hex), _) where hex.count == Self.bitWidth / 4: self.init(hex, radix: 16)
            case (_, .some(let bytes)) where bytes.count == Self.bitWidth / 8: self = bytes.reduce(0) { code, byte in code << 8 | Self(byte) }
            default: return nil
        }
    }

    public var sdef: String {
        let bytes: [UInt8] = stride(from: Self.bitWidth - 8, through: 0, by: -8).map { shift in UInt8(truncatingIfNeeded: self >> shift) }
        let hex: String = String(self, radix: 16, uppercase: true)
        return (bytes.contains { byte in byte < 0x20 || byte == 0x7F } ? nil : String(bytes: bytes, encoding: .macOSRoman))
            ?? "0x" + String(repeating: "0", count: Self.bitWidth / 4 - hex.count) + hex
    }
}
