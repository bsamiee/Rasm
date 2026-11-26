public import AppKit
public import AppleEvents
import AsyncAlgorithms
import CasePathsCore
public import Processes
import System

// --- [MODELS] --------------------------------------------------------------------------

public struct Instance: Hashable, Sendable {
    public let application: NSRunningApplication
    public let process: ProcessInstance
}

extension Instance {
    public init(_ application: NSRunningApplication) throws(ApplicationFailure) {
        let pid = application.processIdentifier
        do throws(Errno) { process = try ProcessInstance(of: pid) } catch { throw ApplicationFailure(subject: String(pid), reason: .system(error)) }
        guard !application.isTerminated else { throw ApplicationFailure(subject: String(pid), reason: .system(.noSuchProcess)) }
        self.application = application
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

extension Watch {
    public enum QuitMode: String, Decodable, Sendable {
        case asking
        case saving
        case discarding
        case forcing

        var saveOptions: OSType? {
            switch self {
                case .asking: kAEAsk
                case .saving: kAEYes
                case .discarding: kAENo
                case .forcing: nil
            }
        }
    }

    public nonisolated func quit(
        _ mode: QuitMode,
        within deadline: Duration,
        in dictionary: ScriptingDictionary? = nil,
        clock: some Clock<Duration> & Sendable = ContinuousClock(),
    ) async throws(ApplicationFailure) -> InstanceState {
        let subject = String(instance.process.pid)
        guard mode != .forcing else {
            guard instance.application.forceTerminate() else { throw ApplicationFailure(subject: subject, reason: instance.process.hasExited ? .system(.noSuchProcess) : .refused) }
            return await states().reduce(await current) { _, next in next }
        }
        let saving: OSType? =
            dictionary?.commands.contains { (command: ScriptingDictionary.Command) -> Bool in
                command.eventClass == AEEventClass(kCoreEventClass) && command.eventID == AEEventID(kAEQuitApplication)
                    && command.parameters.contains { parameter in parameter.code == AEKeyword(keyAESaveOptions) }
            } == true ? mode.saveOptions : nil
        let (readiness, ready) = AsyncStream<Void>.makeStream(bufferingPolicy: .bufferingNewest(1))
        let refusals = readiness.prefix(1).compactMap { [instance] () async -> ApplicationFailure? in
            let event = NSAppleEventDescriptor(
                eventClass: AEEventClass(kCoreEventClass),
                eventID: AEEventID(kAEQuitApplication),
                targetDescriptor: NSAppleEventDescriptor(processIdentifier: instance.process.pid),
                returnID: AEReturnID(kAutoGenerateReturnID),
                transactionID: AETransactionID(kAnyTransactionID),
            )
            if let saving { event.setParam(NSAppleEventDescriptor(enumCode: saving), forKeyword: keyAESaveOptions) }
            do throws(AppleEventFailure) {
                try await NSAppleEventDescriptor.send(event, within: deadline) { _ in }
                return nil
            } catch  where error.status == OSStatus(userCanceledErr) {
                do { try await clock.sleep(for: messagingTimeout) } catch { return nil }
                return ApplicationFailure(subject: subject, reason: .event(error))
            } catch  where instance.process.hasExited {
                return nil
            } catch  where error.status == OSStatus(procNotFound) {
                do throws(Errno) {
                    try instance.process.signal(SIGTERM)
                    return nil
                } catch Errno.noSuchProcess {
                    return nil
                } catch {
                    return ApplicationFailure(subject: subject, reason: .system(error))
                }
            } catch {
                return ApplicationFailure(subject: subject, reason: .event(error))
            }
        }
        var sent = false
        for await event in merge(await states().map(Result<InstanceState, ApplicationFailure>.success), refusals.map(Result<InstanceState, ApplicationFailure>.failure)) {
            let state = try event.get()
            switch (state, sent) {
                case (.exited, _): return await states().reduce(state) { _, next in next }
                case (.running(let running), _) where running.asking.isEmpty && (running.status == .responding || running.status == .unresponsive(thread: .runnable)):
                    sent = true
                    ready.yield()
                case (.running(let running), _) where running.asking.isEmpty && !running.status.isIdle: continue
                case (.failed, true): continue
                case (.running, _), (.failed, false): return state
            }
        }
        return await current
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension NSWorkspace {
    public func instances(of matching: (NSRunningApplication) -> Bool) throws(ApplicationFailure) -> [Instance] {
        try runningApplications.filter(matching)
            .map { application throws(ApplicationFailure) -> Instance? in
                do throws(ApplicationFailure) {
                    return try Instance(application)
                } catch  where error.reason == .system(.noSuchProcess) {
                    return nil
                }
            }
            .compactMap(\.self)
    }
}
