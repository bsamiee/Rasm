import AppKit

// --- [MODELS] --------------------------------------------------------------------------

struct Application: Codable {
    // --- [DISCOVERY]

    let url: URL
    let identifier: String
    let name: String
    let version: String?
    let processIdentifiers: [pid_t]
    let scriptable: Bool

    init?(url: URL, runningApplications: [NSRunningApplication]) throws {
        guard let bundle: Bundle = Bundle(url: url), let identifier: String = bundle.bundleIdentifier, identifier.hasPrefix("com.adobe.") else { return nil }
        self.url = url
        self.identifier = identifier
        name = FileManager.default.displayName(atPath: url.path(percentEncoded: false))
        version = bundle.object(forInfoDictionaryKey: "CFBundleShortVersionString") as? String
        processIdentifiers = runningApplications.filter { runningApplication in
            runningApplication.activationPolicy != .prohibited && runningApplication.bundleURL?.resolvingSymlinksInPath().path(percentEncoded: false) == url.path(percentEncoded: false)
        }
        .map(\.processIdentifier)
        scriptable = try url.resourceValues(forKeys: [.applicationIsScriptableKey]).applicationIsScriptable == true
    }

    init(from decoder: any Decoder) throws {
        let container: any SingleValueDecodingContainer = try decoder.singleValueContainer()
        let url: URL = try container.decode(URL.self)
        let standardized: URL = URL(filePath: url.path(percentEncoded: false), directoryHint: .checkFileSystem).resolvingSymlinksInPath()
        guard url.isFileURL, let application: Self = try Self(url: standardized, runningApplications: NSWorkspace.shared.runningApplications) else {
            throw DecodingError.dataCorruptedError(in: container, debugDescription: "Not an Adobe application bundle: \(url.absoluteString)")
        }
        self = application
    }

    static func all() throws -> [Self] {
        let runningApplications: [NSRunningApplication] = NSWorkspace.shared.runningApplications
        let installed: [URL] = try FileManager.default.urls(for: .applicationDirectory, in: .allDomainsMask).flatMap { directory in
            try FileManager.default.enumerator(at: directory, includingPropertiesForKeys: [.isApplicationKey], options: [.skipsPackageDescendants, .skipsHiddenFiles])?
                .allObjects.compactMap { entry in entry as? URL }
                .filter { url in try url.resourceValues(forKeys: [.isApplicationKey]).isApplication == true } ?? []
        }
        return try Set((installed + runningApplications.compactMap(\.bundleURL)).map { url in url.resolvingSymlinksInPath() })
            .compactMap { url in try Self(url: url, runningApplications: runningApplications) }
            .sorted { left, right in left.url.absoluteString < right.url.absoluteString }
    }

    // --- [EXECUTION]

    func runningApplication() -> Result<NSRunningApplication, any Error> {
        guard processIdentifiers.count == 1, let runningApplication: NSRunningApplication = processIdentifiers.first.flatMap(NSRunningApplication.init(processIdentifier:)) else {
            return .failure(ScriptingFailure.runningInstances(url))
        }
        let observations: [NSKeyValueObservation] = [\NSRunningApplication.isFinishedLaunching, \.isTerminated].map { keyPath in
            runningApplication.observe(keyPath) { _, _ in CFRunLoopStop(CFRunLoopGetMain()) }
        }
        withExtendedLifetime(observations) {
            if !(runningApplication.isFinishedLaunching || runningApplication.isTerminated) { CFRunLoopRun() }
        }
        return runningApplication.isTerminated
            ? .failure(ScriptingFailure.terminated(url)) : .success(runningApplication)
    }

    func execute(_ command: String, arguments: [String: Descriptor]) -> Result<any Encodable, Failure> {
        runningApplication()
            .flatMap { runningApplication in
                Result { try ScriptingDictionary(application: url) }.flatMap { dictionary in
                    dictionary.event(command, arguments: arguments, target: NSAppleEventDescriptor(processIdentifier: runningApplication.processIdentifier)).map { event in (dictionary, event) }
                }
            }
            .mapError(Failure.request)
            .flatMap { dictionary, event in
                Result { try event.sendEvent(options: [.waitForReply, .neverInteract], timeout: TimeInterval(kNoTimeOut)) }.mapError(Failure.send).map { reply in (dictionary, reply) }
            }
            .flatMap { dictionary, reply in
                switch reply.paramDescriptor(forKeyword: AEKeyword(keyErrorNumber))?.int32Value {
                    case .some(let status) where status != noErr:
                        let message: String? = reply.paramDescriptor(forKeyword: AEKeyword(keyErrorString))?.stringValue
                        for keyword: AEKeyword in [AEKeyword(keyErrorNumber), AEKeyword(keyErrorString)] {
                            reply.removeParamDescriptor(withKeyword: keyword)
                        }
                        return .failure(
                            .reply(
                                ScriptingFailure.eventFailed(status, message: message),
                                reply.numberOfItems == 0 ? nil : dictionary.decode(reply),
                            )
                        )
                    default:
                        return .success(reply.paramDescriptor(forKeyword: AEKeyword(keyDirectObject)).map(dictionary.decode) ?? .null)
                }
            }
    }
}
