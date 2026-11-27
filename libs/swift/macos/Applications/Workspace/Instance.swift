package import AppKit
package import AppleEvents
import AsyncAlgorithms
import CasePathsCore
package import Processes
import System

// --- [MODELS] --------------------------------------------------------------------------

package struct Instance: Hashable, Sendable {
    package let application: NSRunningApplication
    package let process: ProcessInstance
}

extension Instance {
    package init(_ application: NSRunningApplication) throws(ApplicationFailure) {
        let pid = application.processIdentifier
        guard !application.isTerminated else { throw ApplicationFailure(subject: String(pid), reason: .system(.noSuchProcess)) }
        do throws(Errno) { process = try ProcessInstance(of: pid) } catch { throw ApplicationFailure(subject: String(pid), reason: .system(error)) }
        self.application = application
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

extension Watch {
    package enum QuitMode: String, Decodable, Sendable {
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

    package nonisolated func quit(
        _ mode: QuitMode,
        within deadline: Duration = replyDeadline,
        in dictionary: ScriptingDictionary? = nil,
        clock: some Clock<Duration> & Sendable = ContinuousClock(),
    ) async throws(ApplicationFailure) -> InstanceState {
        let subject = String(instance.process.pid)
        let saving: OSType? =
            dictionary?.commands.contains { (command: ScriptingDictionary.Command) -> Bool in
                command.eventClass == AEEventClass(kCoreEventClass) && command.eventID == AEEventID(kAEQuitApplication)
                    && command.parameters.contains { parameter in parameter.code == AEKeyword(keyAESaveOptions) }
            } == true ? mode.saveOptions : nil
        let (readiness, ready) = AsyncStream<Void>.makeStream(bufferingPolicy: .bufferingNewest(1))
        let refusals = readiness.prefix(1).compactMap { [instance] () async -> ApplicationFailure? in
            let signal: Int32
            if mode == .forcing {
                signal = SIGKILL
            } else {
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
                } catch  where instance.process.hasExited {
                    return nil
                } catch  where error.status == OSStatus(procNotFound) {
                    signal = SIGTERM
                } catch {
                    return ApplicationFailure(subject: subject, reason: .event(error))
                }
            }
            do throws(Errno) {
                try instance.process.signal(signal)
                return nil
            } catch Errno.noSuchProcess {
                return nil
            } catch {
                return ApplicationFailure(subject: subject, reason: .system(error))
            }
        }
        var (sent, asked) = (false, false)
        for await element in merge(
            await messages().map { message -> Result<Message, ApplicationFailure>? in .success(message) },
            refusals.map { refusal -> Result<Message, ApplicationFailure>? in .failure(refusal) },
            AsyncTimerSequence(interval: deadline, clock: clock).prefix(1).map { _ in Result<Message, ApplicationFailure>?.none },
        ) {
            guard let message = try element?.get() else { break }
            asked = asked || message[case: \.event]?.change.is(\.asking) == true
            guard let state = message[case: \.state] else { continue }
            switch (state, mode == .forcing, sent) {
                case (.exited, _, _): return await states().reduce(state) { _, next in next }
                case (.running(let running), false, _) where asked || !running.asking.isEmpty || running.status.is(\.unresponsive) && running.status.isIdle: return state
                case (.running(let running), false, false) where !running.status.isIdle && running.status != .unresponsive(thread: .runnable): continue
                case (_, _, false):
                    sent = true
                    ready.yield()
                case (_, _, true): continue
            }
        }
        return await current
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension NSWorkspace {
    func instances(of matching: (NSRunningApplication) -> Bool) throws(ApplicationFailure) -> [Instance] {
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
