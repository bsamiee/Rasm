public import AppKit
import AsyncAlgorithms

// --- [OPERATIONS] ----------------------------------------------------------------------

extension NSAppleEventDescriptor {
    fileprivate func coreEvent(_ event: AEEventID, setting option: AEKeyword, to value: OSType) -> NSAppleEventDescriptor {
        let core: NSAppleEventDescriptor = NSAppleEventDescriptor(
            eventClass: AEEventClass(kCoreEventClass),
            eventID: event,
            targetDescriptor: self,
            returnID: AEReturnID(kAutoGenerateReturnID),
            transactionID: AETransactionID(kAnyTransactionID),
        )
        core.setParam(NSAppleEventDescriptor(enumCode: value), forKeyword: option)
        return core
    }
}

extension NSWorkspace {
    public func launch(
        _ installation: Installation,
        opening documents: [URL] = [],
        configuration: sending OpenConfiguration = OpenConfiguration(),
    ) async throws(ApplicationFailure) -> (instance: NSRunningApplication, created: Bool) {
        let url: URL = installation.url
        let running: [NSRunningApplication] = runningApplications.filter(installation.runs)
        let creating: Bool = configuration.createsNewApplicationInstance || running.isEmpty
        if let reused: NSRunningApplication = running.first, !creating, documents.isEmpty { return (reused, false) }
        configuration.activates = false
        configuration.hides = creating && !installation.agent
        configuration.addsToRecentItems = false
        configuration.promptsUserIfNeeded = false
        configuration.allowsRunningApplicationSubstitution = false
        if documents.isEmpty {
            configuration.appleEvent = NSAppleEventDescriptor(applicationURL: url).coreEvent(kAEOpenApplication, setting: keyAERestoreAppState, to: kAENo)
        }
        do {
            let instance: NSRunningApplication =
                try await documents.isEmpty
                ? openApplication(at: url, configuration: configuration) : open(documents, withApplicationAt: url, configuration: configuration)
            return (instance, !running.contains(instance))
        } catch {
            throw ApplicationFailure(subject: "Launch Services open \(url.path(percentEncoded: false))", reason: .host(error))
        }
    }
}

extension NSRunningApplication {
    public func launchStates(notifications: Set<String>, polling interval: Duration) -> some AsyncSequence<InstanceState, Never> {
        states(walking: \.requiresAnswer, notifications: notifications, polling: interval)
            .reductions((state: InstanceState.running(.launching, usage: nil), ended: false)) { last, next in (next, !last.state.isLaunching) }
            .prefix { held in !held.ended }
            .map(\.state)
    }

    public func show() throws(ApplicationFailure) {
        guard !unhide(), isHidden else { return }
        throw ApplicationFailure(subject: String(processIdentifier), reason: isTerminated ? .noProcess : .hidden)
    }
}
