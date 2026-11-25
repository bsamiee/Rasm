public import AppKit
public import AppleEvents
import AsyncAlgorithms
import CasePathsCore
public import Processes
import System

// --- [MODELS] --------------------------------------------------------------------------

public struct Instance: Hashable, Sendable {
    public enum Quit: String, Decodable, Sendable {
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

    public let application: NSRunningApplication
    public let process: ProcessInstance
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Instance {
    public init(_ application: NSRunningApplication) throws(ApplicationFailure) {
        let pid = application.processIdentifier
        do throws(Errno) { process = try ProcessInstance(of: pid) } catch { throw ApplicationFailure(error, process: String(pid)) }
        guard !application.isTerminated else { throw ApplicationFailure(subject: String(pid), reason: .noProcess) }
        self.application = application
    }

    public func quit(
        _ ending: Quit = .asking,
        in dictionary: ScriptingDictionary? = nil,
        clock: some Clock<Duration> & Sendable = ContinuousClock(),
    ) -> any AsyncSequence<Result<InstanceState, ApplicationFailure>, Never> & Sendable {
        let states = states(clock: clock)
        guard ending != .forcing else {
            return if application.forceTerminate() {
                states.map(Result<InstanceState, ApplicationFailure>.success)
            } else {
                [Result<InstanceState, ApplicationFailure>.failure(ApplicationFailure(subject: String(process.pid), reason: process.hasExited ? .noProcess : .refused(nil)))].async
            }
        }
        let declared = dictionary?.commands.contains { command in
            command.eventClass == AEEventClass(kCoreEventClass) && command.eventID == AEEventID(kAEQuitApplication)
                && command.parameters.contains { parameter in parameter.code == AEKeyword(keyAESaveOptions) }
        }
        let saving = declared == true ? ending.saveOptions : nil
        return AsyncStream { output in
            let following = Task {
                let (replies, reply) = AsyncStream<Result<InstanceState, ApplicationFailure>>.makeStream()
                var request: Task<Void, Never>?
                events: for await event in merge(states.map(Result.success), replies) {
                    output.yield(event)
                    switch event {
                        case .success(.exited): if request == nil { reply.finish() }
                        case .success(.running(let running)) where running.asking.isEmpty && (running.status == .responding || running.status == .unresponsive(.runnable)):
                            request = request ?? Task { await requestQuit(saving: saving, answering: reply, clock: clock) }
                        case .success(.running(let running)) where running.asking.isEmpty && !running.status.settled: continue
                        case .failure, .success(.running), .success(.failed): break events
                    }
                }
                request?.cancel()
                output.finish()
            }
            output.onTermination = { _ in following.cancel() }
        }
    }

    private func requestQuit(saving: OSType?, answering reply: AsyncStream<Result<InstanceState, ApplicationFailure>>.Continuation, clock: some Clock<Duration> & Sendable) async {
        defer { reply.finish() }
        let request = NSAppleEventDescriptor(
            eventClass: AEEventClass(kCoreEventClass),
            eventID: AEEventID(kAEQuitApplication),
            targetDescriptor: NSAppleEventDescriptor(processIdentifier: process.pid),
            returnID: AEReturnID(kAutoGenerateReturnID),
            transactionID: AETransactionID(kAnyTransactionID),
        )
        if let saving { request.setParam(NSAppleEventDescriptor(enumCode: saving), forKeyword: keyAESaveOptions) }
        let subject = String(process.pid)
        let outcome: Result<InstanceState, ApplicationFailure>?
        do throws(AppleEventFailure) {
            try await NSAppleEventDescriptor.send(request, timeout: NSAppleEventTimeOutDefault) { _ in }
            outcome = nil
        } catch  where error.status == OSStatus(userCanceledErr) {
            _ = await Result { () async throws(any Error) in try await clock.sleep(for: messagingTimeout) }
            outcome =
                switch await state() {
                    case .running(let running) where !running.asking.isEmpty || running.status.settled && running.status.is(\.unresponsive): .success(.running(running))
                    case .failed: nil
                    case .running, .exited: .failure(ApplicationFailure(subject: subject, reason: .refused(nil)))
                }
        } catch  where process.hasExited {
            outcome = nil
        } catch  where error.status == OSStatus(procNotFound) {
            let signaled = Result { () throws(Errno) in try process.signal(SIGTERM) }
            outcome = if case .failure = signaled, !process.hasExited { .failure(ApplicationFailure(subject: subject, reason: .event(error))) } else { nil }
        } catch {
            outcome = .failure(ApplicationFailure(subject: subject, reason: .event(error)))
        }
        if let outcome { reply.yield(outcome) }
    }
}
