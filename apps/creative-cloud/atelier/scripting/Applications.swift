import AppKit
import RasmMacOS
import Subprocess
import System

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

    static func send(_ event: NSAppleEventDescriptor, decoding dictionary: ScriptingDictionary, as types: Set<String>) -> Result<Descriptor, AggregateError<Failure>> {
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
                        .success(reply.paramDescriptor(forKeyword: AEKeyword(keyDirectObject)).map { value in dictionary.decode(value, as: types) } ?? .null)
                }
            }
    }

    func runningApplication() -> Result<NSRunningApplication, AggregateError<Failure>> {
        let instances: [NSRunningApplication] = Self.instances(at: url, among: NSWorkspace.shared.runningApplications)
        guard instances.count == 1, let runningApplication: NSRunningApplication = instances.first else {
            return .failure(AggregateError(first: .instances(count: instances.count)))
        }
        let settled: DispatchSemaphore = DispatchSemaphore(value: 0)
        let observations: [NSKeyValueObservation] = [\NSRunningApplication.isFinishedLaunching, \.isTerminated].map { keyPath in
            runningApplication.observe(keyPath) { _, _ in settled.signal() }
        }
        withExtendedLifetime(observations) {
            if !(runningApplication.isFinishedLaunching || runningApplication.isTerminated) { settled.wait() }
        }
        return runningApplication.isTerminated ? .failure(AggregateError(first: .instances(count: 0))) : .success(runningApplication)
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
                    .mapError { error in AggregateError(first: .unreadable(error)) }
                    .map { rows in rows.filter { row in row.owner == application.processIdentifier }.map(\.window) }
            case .none:
                .failure(AggregateError(first: .sessionless))
        }
    }

    // --- [CAPTURE]

    static func capture(_ id: Int) async -> Result<Data, AggregateError<Failure>> {
        let tool: FilePath = "/usr/sbin/screencapture"
        let url: URL = URL.temporaryDirectory.appending(path: "\(UUID().uuidString).jpg")
        defer { try? FileManager.default.removeItem(at: url) }
        let captured: Result<Void, AggregateError<Failure>> = await ProcessRun.collect(
            ProcessRun.configuration(
                executable: .path(tool),
                arguments: ["-x", "-o", "-t", "jpg", "-l", String(id), url.path(percentEncoded: false)],
                environment: ProcessInfo.processInfo.environment,
                workingDirectory: url.deletingLastPathComponent(),
            ),
            deadline: .seconds(30),
        )
        .mapError { error in AggregateError(first: .unreadable(error)) }
        .flatMap { result in
            result.terminationStatus.isSuccess
                ? .success(())
                : .failure(AggregateError(first: .exited(tool: tool, status: result.terminationStatus, message: result.standardError.trimmingCharacters(in: .newlines))))
        }
        return captured.flatMap { _ in Result { try Data(contentsOf: url) }.mapError { error in AggregateError(first: .unreadable(error)) } }
    }
}
