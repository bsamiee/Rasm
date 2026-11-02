public import AppKit
import AsyncAlgorithms
import Carbon.OpenScripting
import Processes
import System

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
        guard !(installation.multipleInstancesProhibited && configuration.createsNewApplicationInstance && runningApplications.contains(where: installation.runs)) else {
            throw ApplicationFailure(subject: path, reason: .multipleInstancesProhibited)
        }
        configuration.appleEvent =
            documents.isEmpty
            ? NSAppleEventDescriptor(
                eventClass: AEEventClass(kASAppleScriptSuite),
                eventID: AEEventID(kASLaunchEvent),
                targetDescriptor: NSAppleEventDescriptor(applicationURL: url),
                returnID: AEReturnID(kAutoGenerateReturnID),
                transactionID: AETransactionID(kAnyTransactionID),
            ) : nil
        do {
            return try await documents.isEmpty
                ? openApplication(at: url, configuration: configuration) : open(documents, withApplicationAt: url, configuration: configuration)
        } catch {
            throw ApplicationFailure(subject: "Launch Services open \(path)", reason: .host(error))
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
