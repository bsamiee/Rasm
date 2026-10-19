import AppKit
import Carbon.OpenScripting
import ImageIO
import RasmMacOS
import ScreenCaptureKit
import UniformTypeIdentifiers

// --- [MODELS] --------------------------------------------------------------------------

struct Application: Encodable {
    // --- [DISCOVERY]

    struct Instance: Encodable {
        let pid: pid_t
        let finishedLaunching: Bool
    }

    let url: URL
    let identifier: String
    let name: String
    let version: String?
    let channel: String?
    let running: Instance?
    let scriptable: Bool

    init?(url: URL, runningApplications: [NSRunningApplication]) throws {
        guard let bundle: Bundle = Bundle(url: url), let identifier: String = bundle.bundleIdentifier, identifier.hasPrefix("com.adobe.") else { return nil }
        self.url = url
        self.identifier = identifier
        name = FileManager.default.displayName(atPath: url.path(percentEncoded: false))
        version = bundle.object(forInfoDictionaryKey: "CFBundleShortVersionString") as? String
        channel =
            (bundle.object(forInfoDictionaryKey: "RELEASECHANNEL") as? String)?.lowercased()
            ?? (bundle.object(forInfoDictionaryKey: "CFBundleName") as? String).flatMap { name in name.wholeMatch(of: /.*\((?<channel>[^)]+)\)/)?.channel.lowercased() }
        running = NSRunningApplication.instances(at: url, among: runningApplications).first.map { instance in
            Instance(pid: instance.processIdentifier, finishedLaunching: instance.isFinishedLaunching)
        }
        scriptable = try url.resourceValues(forKeys: [.applicationIsScriptableKey]).applicationIsScriptable == true
    }

    static func at(_ url: URL) -> Result<Self, AggregateError<Failure>> {
        Result {
            try url.isFileURL
                ? Self(url: URL(filePath: url.path(percentEncoded: false), directoryHint: .checkFileSystem).resolvingSymlinksInPath(), runningApplications: NSWorkspace.shared.runningApplications)
                : nil
        }
        .mapError { error in AggregateError(first: .unreadable(error)) }
        .flatMap { application in application.map(Result.success) ?? .failure(AggregateError(first: .notApplication(url))) }
    }

    static func all() -> Result<[Self], AggregateError<Failure>> {
        Result {
            let runningApplications: [NSRunningApplication] = NSWorkspace.shared.runningApplications
            return try NSWorkspace.shared.installedApplications().compactMap { url in try Self(url: url, runningApplications: runningApplications) }
        }
        .mapError { error in AggregateError(first: .unreadable(error)) }
    }

    // --- [LIFECYCLE]

    func open() async -> Result<NSRunningApplication, AggregateError<Failure>> {
        await Result { try await NSWorkspace.shared.launchApplication(at: url) }.mapError { error in AggregateError(first: .unreadable(error)) }
    }

    func launch() async -> Result<Self, AggregateError<Failure>> {
        await open().flatMap { _ in Self.at(url) }
    }

    var instance: Result<NSRunningApplication, AggregateError<Failure>> {
        Accessibility.trusted(prompting: true)
            ? NSRunningApplication.instances(at: url).first.map(Result.success) ?? .failure(AggregateError(first: .notRunning(url)))
            : .failure(AggregateError(first: .element(.untrusted)))
    }

    // --- [STATUS]

    func status() -> Result<Status, AggregateError<Failure>> {
        instance.flatMap { instance in Status.of(instance) }
    }

    @MainActor
    func watch() -> Result<AsyncStream<Status>, AggregateError<Failure>> {
        instance.flatMap { instance in
            Status.of(instance).map { first in
                let names: [String] = [
                    kAXWindowCreatedNotification, kAXSheetCreatedNotification, kAXMenuOpenedNotification, kAXUIElementDestroyedNotification, kAXMenuClosedNotification,
                    kAXFocusedWindowChangedNotification,
                ]
                return AsyncStream { continuation in
                    continuation.yield(first)
                    let task: Task<Void, Never> = Task { @MainActor in
                        _ = await instance.elementNotifications(names) { notifications in
                            var last: Status = first
                            for await _ in notifications {
                                guard case .success(let next) = Status.of(instance), next != last else { continue }
                                last = next
                                continuation.yield(next)
                            }
                        }
                        continuation.finish()
                    }
                    continuation.onTermination = { _ in task.cancel() }
                }
            }
        }
    }

    func press(_ button: String, on element: CGWindowID) -> Result<Status, AggregateError<Failure>> {
        instance.flatMap { instance in instance.press(button, on: element).mapError { error in AggregateError(first: .element(error)) }.flatMap { _ in Status.of(instance) } }
    }

    // --- [EXECUTION]

    static func send(_ event: NSAppleEventDescriptor, decoding dictionary: ScriptingDictionary) -> Result<AppleEventReply, AggregateError<Failure>> {
        let undelivered: Set<OSStatus> = [OSStatus(procNotFound), OSStatus(errAEEventNotPermitted)]
        return Result { try event.sendEvent(options: [.waitForReply, .neverInteract], timeout: TimeInterval(kNoTimeOut)) }
            .mapError { error in
                let status: OSStatus = OSStatus((error as NSError).code)
                return AggregateError(first: undelivered.contains(status) ? .undelivered(status: status) : .unanswered(status: status))
            }
            .flatMap { reply in
                switch reply.paramDescriptor(forKeyword: AEKeyword(keyErrorNumber))?.int32Value {
                    case .some(let status) where undelivered.contains(status):
                        .failure(AggregateError(first: .undelivered(status: status)))
                    case .some(let status) where status != noErr:
                        .failure(AggregateError(first: .reply(status: status, message: reply.paramDescriptor(forKeyword: AEKeyword(keyErrorString))?.stringValue)))
                    default:
                        .success(AppleEventReply(descriptor: reply.paramDescriptor(forKeyword: AEKeyword(keyDirectObject)) ?? .null(), dictionary: dictionary))
                }
            }
    }

    func execute(_ command: String, arguments: [String: JSONValue]) async -> Result<AppleEventReply, AggregateError<Failure>> {
        await open()
            .flatMap { runningApplication in
                ScriptingDictionary.read(self, process: runningApplication.processIdentifier).flatMap { dictionary in
                    dictionary.events(command, arguments: arguments, target: NSAppleEventDescriptor(processIdentifier: runningApplication.processIdentifier)).map { events in (dictionary, events) }
                }
            }
            .flatMap { dictionary, events in
                events.reduce(Result.success(AppleEventReply(descriptor: .null(), dictionary: dictionary))) { previous, event in
                    previous.flatMap { _ in Self.send(event, decoding: dictionary) }
                }
            }
    }
}

struct Status: Encodable, Equatable {
    let responding: Bool
    let frontWindow: CGWindowID?
    let elements: [Element]

    static func of(_ instance: NSRunningApplication) -> Result<Self, AggregateError<Failure>> {
        switch instance.elements() {
            case .success(let elements): .success(Self(responding: true, frontWindow: elements.front, elements: elements.open))
            case .failure(.unresponsive): .success(Self(responding: false, frontWindow: nil, elements: []))
            case .failure(let error): .failure(AggregateError(first: .element(error)))
        }
    }
}

struct Screenshot: Encodable {
    private enum Key: CodingKey { case window, windows, image, id, title, width, height }

    let window: SCWindow
    let windows: [SCWindow]
    let image: SCScreenshotOutput

    func encode(to encoder: any Encoder) throws {
        func encode(_ window: SCWindow, to encoder: any Encoder) throws {
            var container: KeyedEncodingContainer<Key> = encoder.container(keyedBy: Key.self)
            try container.encode(window.windowID, forKey: .id)
            try container.encodeIfPresent(window.title, forKey: .title)
        }
        var container: KeyedEncodingContainer<Key> = encoder.container(keyedBy: Key.self)
        try encode(window, to: container.superEncoder(forKey: .window))
        var windows: any UnkeyedEncodingContainer = container.nestedUnkeyedContainer(forKey: .windows)
        for window: SCWindow in self.windows { try encode(window, to: windows.superEncoder()) }
        guard let image: CGImage = image.sdrImage,
            let data: CFMutableData = CFDataCreateMutable(nil, 0),
            let destination: CGImageDestination = CGImageDestinationCreateWithData(data, UTType.png.identifier as CFString, 1, nil)
        else { throw CocoaError(.coderInvalidValue) }
        CGImageDestinationAddImage(destination, image, [kCGImageDestinationOptimizeColorForSharing: true] as CFDictionary)
        guard CGImageDestinationFinalize(destination) else { throw CocoaError(.coderInvalidValue) }
        try container.encode(image.width, forKey: .width)
        try container.encode(image.height, forKey: .height)
        try container.encode(data as Data, forKey: .image)
    }
}
