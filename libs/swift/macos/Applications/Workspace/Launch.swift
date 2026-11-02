public import AppKit
import AsyncAlgorithms
import Processes
import System

// --- [MODELS] --------------------------------------------------------------------------

private enum HostSetting {
    case argument(String)
    case preference(domain: String?, key: String, path: [String] = [], value: Int)
}

public struct Launch: ~Copyable, Sendable {
    public let instance: NSRunningApplication
    public let created: Bool
    private var quits: Bool

    init(instance: NSRunningApplication, created: Bool) {
        self.instance = instance
        self.created = created
        quits = created
    }

    deinit {
        if quits {
            _ = try? NSAppleEventDescriptor(processIdentifier: instance.processIdentifier).coreEvent(kAEQuitApplication, declining: keyAESaveOptions)
                .sendEvent(options: [.noReply, .neverInteract], timeout: NSAppleEventTimeOutDefault)
        }
    }

    public consuming func detached() -> NSRunningApplication {
        quits = false
        return instance
    }
}

// --- [TABLES] --------------------------------------------------------------------------

private let hostSettings: KeyValuePairs<String, [HostSetting]> = [
    "com.mcneel.rhinoceros": [.argument("-nosplash")],
    "com.adobe.Acrobat": [.preference(domain: nil, key: "DC", path: ["ShowSplashScreen"], value: 0)],
    "com.adobe.": [.preference(domain: "com.adobe.crashreporter", key: "always_never_send", value: 2)],
]

// --- [OPERATIONS] ----------------------------------------------------------------------

extension NSAppleEventDescriptor {
    fileprivate func coreEvent(_ event: AEEventID, declining option: AEKeyword) -> NSAppleEventDescriptor {
        let core: NSAppleEventDescriptor = NSAppleEventDescriptor(
            eventClass: AEEventClass(kCoreEventClass),
            eventID: event,
            targetDescriptor: self,
            returnID: AEReturnID(kAutoGenerateReturnID),
            transactionID: AETransactionID(kAnyTransactionID),
        )
        core.setParam(NSAppleEventDescriptor(enumCode: OSType(kAENo)), forKeyword: option)
        return core
    }
}

private func nested(_ value: Int, at path: ArraySlice<String>, in dictionary: [String: Any]) -> Any {
    path.first.map { key in dictionary.merging([key: nested(value, at: path.dropFirst(), in: dictionary[key] as? [String: Any] ?? [:])]) { _, written in written } } ?? value
}

extension NSWorkspace {
    public func launch(
        _ installation: Installation,
        opening documents: [URL] = [],
        configuration: sending OpenConfiguration = OpenConfiguration(),
    ) async throws(ApplicationFailure) -> Launch {
        let url: URL = installation.url
        let running: [NSRunningApplication] = runningApplications.filter(installation.runs)
        let creating: Bool = configuration.createsNewApplicationInstance || running.isEmpty
        if let reused: NSRunningApplication = running.first, !creating, documents.isEmpty { return Launch(instance: reused, created: false) }
        let settings: [HostSetting] = creating ? hostSettings.filter { prefix, _ in installation.identifier.hasPrefix(prefix) }.flatMap(\.value) : []
        for setting: HostSetting in settings {
            switch setting {
                case .argument(let argument): configuration.arguments.append(argument)
                case .preference(let domain, let key, let path, let value):
                    let defaults: UserDefaults? = UserDefaults(suiteName: domain ?? installation.identifier)
                    defaults?.set(nested(value, at: path[...], in: defaults?.dictionary(forKey: key) ?? [:]), forKey: key)
            }
        }
        configuration.activates = false
        configuration.hides = creating && !installation.agent
        configuration.addsToRecentItems = false
        configuration.promptsUserIfNeeded = false
        configuration.allowsRunningApplicationSubstitution = false
        if documents.isEmpty {
            configuration.appleEvent = NSAppleEventDescriptor(applicationURL: url).coreEvent(kAEOpenApplication, declining: keyAERestoreAppState)
        }
        do {
            let instance: NSRunningApplication =
                try await documents.isEmpty
                ? openApplication(at: url, configuration: configuration) : open(documents, withApplicationAt: url, configuration: configuration)
            return Launch(instance: instance, created: !running.contains(instance))
        } catch {
            throw ApplicationFailure(subject: "Launch Services open \(url.path(percentEncoded: false))", reason: .host(error))
        }
    }
}

extension NSRunningApplication {
    public func finishedLaunching() async throws(ApplicationFailure) {
        let (launches, launch): (AsyncStream<Result<Bool, Errno>>, AsyncStream<Result<Bool, Errno>>.Continuation) = AsyncStream.makeStream(bufferingPolicy: .bufferingOldest(1))
        let observation: NSKeyValueObservation = observe(\.isFinishedLaunching, options: [.initial, .new]) { _, change in
            if change.newValue == true { launch.yield(.success(true)) }
        }
        defer { observation.invalidate() }
        let exits: some AsyncSequence<Result<Bool, Errno>, Never> & Sendable = termination(of: processIdentifier).map { exit in exit.map { _ in false } }
        switch await merge(launches, exits).first(where: { _ in true }) {
            case .success(true): return
            case .success(false): throw ApplicationFailure(.noSuchProcess, process: processIdentifier)
            case .failure(let error): throw ApplicationFailure(error, process: processIdentifier)
            case nil: throw ApplicationFailure(subject: "launch of process \(processIdentifier)", reason: .cancelled(CancellationError()))
        }
    }
}
