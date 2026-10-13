import AppKit
import ImageIO
import Subprocess
import System
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
        .mapError { error in AggregateError(.unreadable(error)) }
        .flatMap { application in application.map(Result.success) ?? .failure(AggregateError(.notApplication(url))) }
    }

    static func all() -> Result<[Self], AggregateError<Failure>> {
        Result {
            let runningApplications: [NSRunningApplication] = NSWorkspace.shared.runningApplications
            return try Set(
                FileManager.default.urls(for: .applicationDirectory, in: .allDomainsMask).flatMap { directory in
                    FileManager.default.enumerator(at: directory, includingPropertiesForKeys: [.isApplicationKey], options: [.skipsPackageDescendants, .skipsHiddenFiles])?
                        .allObjects.compactMap { entry in (entry as? URL)?.resolvingSymlinksInPath() } ?? []
                }
            )
            .compactMap { url in try url.resourceValues(forKeys: [.isApplicationKey]).isApplication == true ? Self(url: url, runningApplications: runningApplications) : nil }
            .sorted { left, right in left.url.absoluteString < right.url.absoluteString }
        }
        .mapError { error in AggregateError(.unreadable(error)) }
    }

    // --- [EXECUTION]

    static func send(_ event: NSAppleEventDescriptor, decoding dictionary: ScriptingDictionary, as types: Set<String>) -> Result<Descriptor, AggregateError<Failure>> {
        let undelivered: Set<OSStatus> = [OSStatus(procNotFound), OSStatus(errAEEventNotPermitted)]
        return Result { try event.sendEvent(options: [.waitForReply, .neverInteract], timeout: TimeInterval(kNoTimeOut)) }
            .mapError { error in
                let status: OSStatus = OSStatus((error as NSError).code)
                return AggregateError(undelivered.contains(status) ? .undelivered(status: status) : .unanswered(status: status))
            }
            .flatMap { reply in
                switch reply.paramDescriptor(forKeyword: AEKeyword(keyErrorNumber))?.int32Value {
                    case .some(let status) where undelivered.contains(status):
                        .failure(AggregateError(.undelivered(status: status)))
                    case .some(let status) where status != noErr:
                        .failure(AggregateError(.reply(status: status, message: reply.paramDescriptor(forKeyword: AEKeyword(keyErrorString))?.stringValue)))
                    default:
                        .success(reply.paramDescriptor(forKeyword: AEKeyword(keyDirectObject)).map { value in dictionary.decode(value, as: types) } ?? .null)
                }
            }
    }

    func runningApplication() -> Result<NSRunningApplication, AggregateError<Failure>> {
        let instances: [NSRunningApplication] = Self.instances(at: url, among: NSWorkspace.shared.runningApplications)
        guard instances.count == 1, let runningApplication: NSRunningApplication = instances.first else {
            return .failure(AggregateError(.instances(count: instances.count)))
        }
        let settled: DispatchSemaphore = DispatchSemaphore(value: 0)
        let observations: [NSKeyValueObservation] = [\NSRunningApplication.isFinishedLaunching, \.isTerminated].map { keyPath in
            runningApplication.observe(keyPath) { _, _ in settled.signal() }
        }
        withExtendedLifetime(observations) {
            if !(runningApplication.isFinishedLaunching || runningApplication.isTerminated) { settled.wait() }
        }
        return runningApplication.isTerminated ? .failure(AggregateError(.instances(count: 0))) : .success(runningApplication)
    }

    func execute(_ command: String, arguments: [String: Descriptor]) -> Result<Descriptor, AggregateError<Failure>> {
        runningApplication()
            .flatMap { runningApplication in
                ScriptingDictionary.read(self, process: runningApplication.processIdentifier).flatMap { dictionary in
                    dictionary.events(command, arguments: arguments, target: NSAppleEventDescriptor(processIdentifier: runningApplication.processIdentifier)).map { events in (dictionary, events) }
                }
            }
            .flatMap { dictionary, events in
                events.reduce(Result.success(.null)) { previous, request in
                    previous.flatMap { _ in Self.send(request.event, decoding: dictionary, as: request.result) }
                }
            }
    }
}

struct Window: Encodable {
    // --- [DISCOVERY]

    private struct Row: Decodable {
        private enum Key: CodingKey { case kCGWindowOwnerPID, kCGWindowNumber, kCGWindowName, kCGWindowLayer, kCGWindowBounds }
        private enum Bounds: String, CodingKey {
            case width = "Width"
            case height = "Height"
        }

        let owner: pid_t
        let window: Window

        init(from decoder: any Decoder) throws {
            let row: KeyedDecodingContainer<Key> = try decoder.container(keyedBy: Key.self)
            let bounds: KeyedDecodingContainer<Bounds> = try row.nestedContainer(keyedBy: Bounds.self, forKey: .kCGWindowBounds)
            owner = try row.decode(pid_t.self, forKey: .kCGWindowOwnerPID)
            window = try Window(
                id: row.decode(Int.self, forKey: .kCGWindowNumber),
                title: row.decodeIfPresent(String.self, forKey: .kCGWindowName),
                level: Level(layer: row.decode(Int.self, forKey: .kCGWindowLayer)),
                width: bounds.decode(Double.self, forKey: .width),
                height: bounds.decode(Double.self, forKey: .height),
            )
        }
    }

    enum Level: String, Encodable {
        case normal, modal, other

        init(layer: Int) {
            self =
                switch layer {
                    case Int(CGWindowLevelForKey(.normalWindow)): .normal
                    case Int(CGWindowLevelForKey(.modalPanelWindow)): .modal
                    default: .other
                }
        }
    }

    let id: Int
    let title: String?
    let level: Level
    let width: Double
    let height: Double

    static func onScreen(of application: NSRunningApplication) -> Result<[Self], AggregateError<Failure>> {
        switch CGWindowListCopyWindowInfo(.optionOnScreenOnly, kCGNullWindowID).flatMap({ rows in unsafe CFPropertyListCreateData(nil, rows, .binaryFormat_v1_0, 0, nil)?.takeRetainedValue() }) {
            case .some(let rows):
                Result { try PropertyListDecoder().decode([Row].self, from: rows as Data) }
                    .mapError { error in AggregateError(.unreadable(error)) }
                    .map { rows in rows.filter { row in row.owner == application.processIdentifier }.map(\.window) }
            case .none:
                .failure(AggregateError(.sessionless))
        }
    }

    // --- [CAPTURE]

    struct Size: Encodable {
        let width: Int
        let height: Int
    }

    static func capture(_ id: Int, into url: URL, longest: Int) async -> Result<Size, AggregateError<Failure>> {
        var options: PlatformOptions = PlatformOptions()
        options.createSession = true
        options.teardownSequence = [.gracefulShutDown(toProcessGroup: true, allowedDurationToNextStep: .seconds(2))]
        let tool: FilePath = "/usr/sbin/screencapture"
        let captured: Result<Void, AggregateError<Failure>> = await Result {
            try await Subprocess.run(
                Configuration(executable: .path(tool), arguments: ["-x", "-o", "-t", "png", "-l", String(id), url.path(percentEncoded: false)], platformOptions: options),
                output: .discarded,
                error: .string(limit: Int(BIG_PIPE_SIZE)),
            )
        }
        .mapError { error in AggregateError(.unreadable(error)) }
        .flatMap { result in
            result.terminationStatus.isSuccess
                ? .success(()) : .failure(AggregateError(.exited(tool: tool, status: result.terminationStatus, message: result.standardError.trimmingCharacters(in: .newlines))))
        }
        return captured.flatMap { _ in
            let thumbnail: [CFString: Any] = [kCGImageSourceCreateThumbnailFromImageAlways: true, kCGImageSourceThumbnailMaxPixelSize: longest, kCGImageSourceCreateThumbnailWithTransform: true]
            guard let source: CGImageSource = CGImageSourceCreateWithURL(url as CFURL, nil), let image: CGImage = CGImageSourceCreateThumbnailAtIndex(source, 0, thumbnail as CFDictionary),
                let destination: CGImageDestination = CGImageDestinationCreateWithURL(url as CFURL, UTType.jpeg.identifier as CFString, 1, nil)
            else { return .failure(AggregateError(.unreadable(CocoaError(.fileReadCorruptFile, userInfo: [NSURLErrorKey: url])))) }
            CGImageDestinationAddImage(destination, image, nil)
            return CGImageDestinationFinalize(destination)
                ? .success(Size(width: image.width, height: image.height)) : .failure(AggregateError(.unreadable(CocoaError(.fileWriteUnknown, userInfo: [NSURLErrorKey: url]))))
        }
    }
}
