public import AppKit
import Carbon.OpenScripting
import CoreServices

// --- [ERRORS] --------------------------------------------------------------------------

public enum DefinitionFailure: LocalizedError {
    case requiresRunning(URL)
    case launch(LaunchFailure)
    case osa(OSStatus)
    case xml(any Error)

    public var errorDescription: String? {
        switch self {
            case .requiresRunning(let url): "\(url.path(percentEncoded: false)) declares no scripting definition file, launch it to read its dynamic dictionary"
            case .launch(let error): error.localizedDescription
            case .osa(let status): "Open Scripting read no scripting definition (\(status))"
            case .xml(let error): "The scripting definition is not readable XML: \(error.localizedDescription)"
        }
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

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Installation {
    public func scriptingDefinition() async -> Result<XMLDocument, DefinitionFailure> {
        let readable: Result<Void, DefinitionFailure> =
            switch (runningApplication, Bundle(url: url)?.object(forInfoDictionaryKey: "OSAScriptingDefinition")) {
                case (.success(let running), _): await running.finishedLaunching().mapError(DefinitionFailure.launch)
                case (.failure, .some): .success(())
                case (.failure, .none): .failure(.requiresRunning(url))
            }
        return readable.flatMap { _ in
            var definition: Unmanaged<CFData>?
            let status: OSStatus = unsafe OSACopyScriptingDefinitionFromURL(url as CFURL, 0, &definition)
            return if status == noErr, let data: CFData = unsafe definition?.takeRetainedValue() {
                Result { try XMLDocument(data: data as Data, options: .documentXInclude) }.mapError(DefinitionFailure.xml)
            } else {
                .failure(.osa(status))
            }
        }
    }
}

extension NSAppleEventDescriptor {
    public func sent() -> Result<NSAppleEventDescriptor, AppleEventFailure> {
        let undelivered: Set<OSStatus> = [OSStatus(procNotFound), OSStatus(errAEEventNotPermitted)]
        var reply: AppleEvent = unsafe AppleEvent()
        let status: OSStatus = unsafe AESendMessage(aeDesc, &reply, AESendMode(kAEWaitReply | kAENeverInteract), kNoTimeOut)
        guard status == noErr else { return .failure(undelivered.contains(status) ? .undelivered(status) : .unanswered(status)) }
        let answer: NSAppleEventDescriptor = unsafe NSAppleEventDescriptor(aeDescNoCopy: &reply)
        return switch answer.paramDescriptor(forKeyword: AEKeyword(keyErrorNumber))?.int32Value {
            case .some(let code) where undelivered.contains(code): .failure(.undelivered(code))
            case .some(let code) where code != noErr: .failure(.replied(code, answer.paramDescriptor(forKeyword: AEKeyword(keyErrorString))?.stringValue))
            case .some, .none: .success(answer.paramDescriptor(forKeyword: AEKeyword(keyDirectObject)) ?? .null())
        }
    }
}
