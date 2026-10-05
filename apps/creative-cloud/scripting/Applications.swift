import AppKit

// --- [MODELS] --------------------------------------------------------------------------

struct Application: Encodable {
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

    static func at(_ url: URL) -> Result<Self, AggregateError<Failure>> {
        Result {
            try url.isFileURL
                ? Self(url: URL(filePath: url.path(percentEncoded: false), directoryHint: .checkFileSystem).resolvingSymlinksInPath(), runningApplications: NSWorkspace.shared.runningApplications)
                : nil
        }
        .mapError { error in AggregateError(.unreadable(code: (error as NSError).code)) }
        .flatMap { application in application.map(Result.success) ?? .failure(AggregateError(.notApplication)) }
    }

    static func all() -> Result<[Self], AggregateError<Failure>> {
        Result {
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
        .mapError { error in AggregateError(.unreadable(code: (error as NSError).code)) }
    }

    // --- [EXECUTION]

    static func send(_ event: NSAppleEventDescriptor, decoding dictionary: ScriptingDictionary, as types: Set<String>) -> Result<Descriptor, AggregateError<Failure>> {
        Result { try event.sendEvent(options: [.waitForReply, .neverInteract], timeout: TimeInterval(kNoTimeOut)) }
            .mapError { error in
                let status: OSStatus = OSStatus((error as NSError).code)
                return AggregateError([OSStatus(procNotFound), OSStatus(errAEEventNotPermitted)].contains(status) ? .undelivered(status: status) : .unanswered(status: status))
            }
            .flatMap { reply in
                switch reply.paramDescriptor(forKeyword: AEKeyword(keyErrorNumber))?.int32Value {
                    case .some(let status) where status != noErr:
                        let message: String? = reply.paramDescriptor(forKeyword: AEKeyword(keyErrorString))?.stringValue
                        for keyword: AEKeyword in [AEKeyword(keyErrorNumber), AEKeyword(keyErrorString)] {
                            reply.removeParamDescriptor(withKeyword: keyword)
                        }
                        return .failure(AggregateError(.reply(status: status, message: message, reply: reply.numberOfItems == 0 ? nil : dictionary.decode(reply, as: []))))
                    default:
                        return .success(reply.paramDescriptor(forKeyword: AEKeyword(keyDirectObject)).map { value in dictionary.decode(value, as: types) } ?? .null)
                }
            }
    }

    func runningApplication() -> Result<NSRunningApplication, AggregateError<Failure>> {
        guard processIdentifiers.count == 1, let runningApplication: NSRunningApplication = processIdentifiers.first.flatMap(NSRunningApplication.init(processIdentifier:)) else {
            return .failure(AggregateError(.runningInstances(count: processIdentifiers.count)))
        }
        let observations: [NSKeyValueObservation] = [\NSRunningApplication.isFinishedLaunching, \.isTerminated].map { keyPath in
            runningApplication.observe(keyPath) { _, _ in CFRunLoopStop(CFRunLoopGetMain()) }
        }
        withExtendedLifetime(observations) {
            if !(runningApplication.isFinishedLaunching || runningApplication.isTerminated) { CFRunLoopRun() }
        }
        return runningApplication.isTerminated ? .failure(AggregateError(.runningInstances(count: 0))) : .success(runningApplication)
    }

    func execute(_ command: String, arguments: [String: Descriptor]) -> Result<Descriptor, AggregateError<Failure>> {
        runningApplication()
            .flatMap { runningApplication in
                ScriptingDictionary.read(self).flatMap { dictionary in
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
