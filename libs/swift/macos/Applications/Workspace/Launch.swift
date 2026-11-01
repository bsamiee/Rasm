public import AppKit
import Carbon.OpenScripting
import Combine

// --- [OPERATIONS] ----------------------------------------------------------------------

extension NSWorkspace {
    public func launch(
        _ installation: Installation,
        opening documents: [URL] = [],
        configuration: sending OpenConfiguration = OpenConfiguration(),
    ) async throws(ApplicationFailure) -> NSRunningApplication {
        let url: URL = installation.url
        let path: String = url.path(percentEncoded: false)
        configuration.activates = false
        configuration.addsToRecentItems = false
        configuration.promptsUserIfNeeded = false
        configuration.allowsRunningApplicationSubstitution = false
        guard !(installation.agent && configuration.hides) else { throw ApplicationFailure(subject: path, reason: .hiddenAgent) }
        configuration.appleEvent =
            documents.isEmpty
            ? NSAppleEventDescriptor(
                eventClass: AEEventClass(kASAppleScriptSuite),
                eventID: AEEventID(kASLaunchEvent),
                targetDescriptor: NSAppleEventDescriptor(applicationURL: url),
                returnID: AEReturnID(kAutoGenerateReturnID),
                transactionID: AETransactionID(kAnyTransactionID),
            ) : nil
        let instance: NSRunningApplication = try await Result {
            try await documents.isEmpty
                ? openApplication(at: url, configuration: configuration) : open(documents, withApplicationAt: url, configuration: configuration)
        }
        .mapError { error in ApplicationFailure(subject: path, reason: .open(error)) }.get()
        try await instance.finishedLaunching()
        return instance
    }
}

extension NSRunningApplication {
    public func finishedLaunching() async throws(ApplicationFailure) {
        let finished: Bool? = await publisher(for: \.isFinishedLaunching).combineLatest(publisher(for: \.isTerminated)).compactMap { done, gone in done || gone ? done : nil }
            .buffer(size: 1, prefetch: .byRequest, whenFull: .dropOldest).values.first { _ in true }
        switch finished {
            case true?: return
            case false?: throw ApplicationFailure(subject: String(processIdentifier), reason: .terminated)
            case nil: throw ApplicationFailure(subject: "launch of process \(processIdentifier)", reason: .cancelled(CancellationError()))
        }
    }
}
