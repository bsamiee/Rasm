import AppKit
import Carbon.OpenScripting
import ImageIO
import RasmMacOS
import ScreenCaptureKit
import UniformTypeIdentifiers

// --- [MODELS] --------------------------------------------------------------------------

struct Application: Encodable {
    // --- [DISCOVERY]

    let url: URL
    let identifier: String
    let name: String
    let version: String?
    let running: Bool
    let scriptable: Bool

    init?(url: URL, runningApplications: [NSRunningApplication]) throws {
        guard let bundle: Bundle = Bundle(url: url), let identifier: String = bundle.bundleIdentifier, identifier.hasPrefix("com.adobe.") else { return nil }
        self.url = url
        self.identifier = identifier
        name = FileManager.default.displayName(atPath: url.path(percentEncoded: false))
        version = bundle.object(forInfoDictionaryKey: "CFBundleShortVersionString") as? String
        running = !Self.instances(at: url, among: runningApplications).isEmpty
        scriptable = try url.resourceValues(forKeys: [.applicationIsScriptableKey]).applicationIsScriptable == true
    }

    static func instances(at url: URL, among runningApplications: [NSRunningApplication]) -> [NSRunningApplication] {
        runningApplications.filter { runningApplication in
            runningApplication.activationPolicy != .prohibited && runningApplication.bundleURL?.resolvingSymlinksInPath().path(percentEncoded: false) == url.path(percentEncoded: false)
        }
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

    static func all(running: Bool) -> Result<[Self], AggregateError<Failure>> {
        Result {
            let runningApplications: [NSRunningApplication] = NSWorkspace.shared.runningApplications
            let keys: Set<URLResourceKey> = [.isApplicationKey, .isSymbolicLinkKey]
            let bundles: [URL] =
                running
                ? runningApplications.compactMap(\.bundleURL)
                : try FileManager.default.urls(for: .applicationDirectory, in: .allDomainsMask).flatMap { directory in
                    try (FileManager.default.enumerator(at: directory, includingPropertiesForKeys: Array(keys), options: [.skipsPackageDescendants, .skipsHiddenFiles])?.allObjects ?? [])
                        .compactMap { entry in entry as? URL }
                        .filter { url in
                            let values: URLResourceValues = try url.resourceValues(forKeys: keys)
                            return values.isApplication == true || values.isSymbolicLink == true
                        }
                }
            return try Set(bundles.map { url in url.resolvingSymlinksInPath() })
                .compactMap { url in try url.resourceValues(forKeys: [.isApplicationKey]).isApplication == true ? Self(url: url, runningApplications: runningApplications) : nil }
                .sorted { left, right in left.url.absoluteString < right.url.absoluteString }
        }
        .mapError { error in AggregateError(first: .unreadable(error)) }
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

    func open() async -> Result<NSRunningApplication, AggregateError<Failure>> {
        await Result {
            let configuration: NSWorkspace.OpenConfiguration = NSWorkspace.OpenConfiguration()
            configuration.activates = false
            configuration.promptsUserIfNeeded = false
            configuration.allowsRunningApplicationSubstitution = false
            configuration.appleEvent = NSAppleEventDescriptor(
                eventClass: AEEventClass(kASAppleScriptSuite),
                eventID: AEEventID(kASLaunchEvent),
                targetDescriptor: NSAppleEventDescriptor(applicationURL: url),
                returnID: AEReturnID(kAutoGenerateReturnID),
                transactionID: AETransactionID(kAnyTransactionID),
            )
            return try await NSWorkspace.shared.openApplication(at: url, configuration: configuration)
        }
        .mapError { error in AggregateError(first: .unreadable(error)) }
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
