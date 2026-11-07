public import AppKit
import Processes

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Installation {
    public init(opening document: URL) throws(ApplicationFailure) {
        let path: String = document.path(percentEncoded: false)
        do { _ = try document.checkResourceIsReachable() } catch { throw ApplicationFailure(subject: path, reason: .host(error as NSError)) }
        guard let handler: URL = NSWorkspace.shared.urlForApplication(toOpen: document) else { throw ApplicationFailure(subject: path, reason: .noHandler) }
        try self.init(at: handler)
    }
}

extension NSWorkspace {
    public func launch(
        _ installation: Installation,
        opening documents: [URL] = [],
        arguments: [String] = [],
        environment: [String: String] = [:],
        newInstance: Bool = false,
    ) async throws(ApplicationFailure) -> (instance: NSRunningApplication, created: Bool) {
        let url: URL = installation.url
        let running: [NSRunningApplication] = runningApplications.filter(installation.runs)
        let creating: Bool = newInstance || running.isEmpty
        if let reused: NSRunningApplication = running.first, !creating, documents.isEmpty { return (reused, false) }
        let executable: String? = Bundle(url: url)?.executableURL?.path(percentEncoded: false)
        let before: Set<pid_t> = executable.map(processes(executing:)) ?? []
        let configuration: OpenConfiguration = OpenConfiguration()
        configuration.activates = false
        configuration.addsToRecentItems = false
        configuration.promptsUserIfNeeded = false
        configuration.allowsRunningApplicationSubstitution = false
        configuration.createsNewApplicationInstance = newInstance
        configuration.arguments = arguments
        configuration.environment = environment
        if documents.isEmpty {
            configuration.appleEvent = NSAppleEventDescriptor(
                eventClass: AEEventClass(kCoreEventClass),
                eventID: kAEOpenApplication,
                targetDescriptor: NSAppleEventDescriptor(applicationURL: url),
                returnID: AEReturnID(kAutoGenerateReturnID),
                transactionID: AETransactionID(kAnyTransactionID),
            )
            configuration.appleEvent?.setParam(NSAppleEventDescriptor(enumCode: kAENo), forKeyword: keyAERestoreAppState)
        }
        let opened: NSRunningApplication
        do {
            opened = try await documents.isEmpty ? openApplication(at: url, configuration: configuration) : open(documents, withApplicationAt: url, configuration: configuration)
        } catch {
            throw ApplicationFailure(subject: url.path(percentEncoded: false), reason: .host(error as NSError))
        }
        guard opened.processIdentifier == -1, let executable else { return (opened, !running.contains(opened)) }
        let after: Set<pid_t> = processes(executing: executable)
        let started: pid_t? = after.subtracting(before).first
        guard let instance: NSRunningApplication = (started ?? after.first).flatMap(NSRunningApplication.init(processIdentifier:)) else {
            throw ApplicationFailure(subject: url.path(percentEncoded: false), reason: .noProcess)
        }
        return (instance, started != nil)
    }
}
