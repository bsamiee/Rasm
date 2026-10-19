public import AppKit
import Carbon.OpenScripting

// --- [OPERATIONS] ----------------------------------------------------------------------

extension NSWorkspace {
    // --- [DISCOVERY]

    public func installedApplications() throws -> [URL] {
        let keys: Set<URLResourceKey> = [.isApplicationKey, .isSymbolicLinkKey]
        let entries: [URL] = try FileManager.default.urls(for: .applicationDirectory, in: .allDomainsMask).flatMap { directory in
            try (FileManager.default.enumerator(at: directory, includingPropertiesForKeys: Array(keys), options: [.skipsPackageDescendants, .skipsHiddenFiles])?.allObjects ?? [])
                .compactMap { entry in entry as? URL }
                .filter { url in
                    let values: URLResourceValues = try url.resourceValues(forKeys: keys)
                    return values.isApplication == true || values.isSymbolicLink == true
                }
        }
        return try Set(entries.map { url in url.resolvingSymlinksInPath() })
            .filter { url in try url.resourceValues(forKeys: [.isApplicationKey]).isApplication == true }
            .sorted { left, right in left.absoluteString < right.absoluteString }
    }

    // --- [LIFECYCLE]

    public func launchApplication(at url: URL) async throws -> NSRunningApplication {
        let configuration: OpenConfiguration = OpenConfiguration()
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
        let instance: NSRunningApplication = try await openApplication(at: url, configuration: configuration)
        await instance.finishedLaunching()
        return instance
    }
}

extension NSRunningApplication {
    public static func instances(at url: URL, among runningApplications: [NSRunningApplication] = NSWorkspace.shared.runningApplications) -> [NSRunningApplication] {
        let path: String = url.path(percentEncoded: false)
        return runningApplications.filter { instance in instance.activationPolicy != .prohibited && instance.bundleURL?.resolvingSymlinksInPath().path(percentEncoded: false) == path }
    }

    public func finishedLaunching() async {
        let (states, sink): (AsyncStream<Void>, AsyncStream<Void>.Continuation) = AsyncStream.makeStream()
        let observations: [NSKeyValueObservation] = [\Self.isFinishedLaunching, \.isTerminated].map { keyPath in
            observe(keyPath, options: [.initial, .new]) { instance, _ in
                if instance.isFinishedLaunching || instance.isTerminated { sink.yield(()) }
            }
        }
        defer {
            for observation: NSKeyValueObservation in observations { observation.invalidate() }
            sink.finish()
        }
        _ = await states.first { _ in true }
    }
}
