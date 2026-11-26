public import AppKit
public import AppleEvents
import AsyncAlgorithms
import CasePathsCore
public import Processes
import System

// --- [MODELS] --------------------------------------------------------------------------

public struct Instance: Hashable, Sendable {
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
        _ ending: QuitMode,
        watch: Watch,
        in dictionary: ScriptingDictionary? = nil,
        clock: some Clock<Duration> & Sendable = ContinuousClock(),
    ) async -> any AsyncSequence<Result<InstanceState, ApplicationFailure>, Never> & Sendable {
        let states = await watch.states()
        guard ending != .forcing else {
            return if application.forceTerminate() {
                states.map(Result<InstanceState, ApplicationFailure>.success)
            } else {
                [Result<InstanceState, ApplicationFailure>.failure(ApplicationFailure(subject: String(process.pid), reason: process.hasExited ? .noProcess : .refused(nil)))].async
            }
        }
        let saving =
            dictionary?.commands.contains { command in
                command.eventClass == AEEventClass(kCoreEventClass) && command.eventID == AEEventID(kAEQuitApplication)
                    && command.parameters.contains { parameter in parameter.code == AEKeyword(keyAESaveOptions) }
            } == true ? ending.saveOptions : nil
        return AsyncStream { output in
            let following = Task {
                let (refusals, refusal) = AsyncStream<ApplicationFailure>.makeStream()
                var request: Task<Void, Never>?
                events: for await event in merge(states.map(Result.success), refusals.map(Result<InstanceState, ApplicationFailure>.failure)) {
                    output.yield(event)
                    switch event {
                        case .success(.exited): if request == nil { refusal.finish() }
                        case .success(.running(let running)) where running.asking.isEmpty && (running.status == .responding || running.status == .unresponsive(.runnable)):
                            request = request ?? Task { await requestQuit(saving: saving, reporting: refusal, watch: watch, clock: clock) }
                        case .success(.running(let running)) where running.asking.isEmpty && !running.status.isIdle: continue
                        case .failure, .success(.running), .success(.failed): break events
                    }
                }
                request?.cancel()
                output.finish()
            }
            output.onTermination = { _ in following.cancel() }
        }
    }

    private func requestQuit(saving: OSType?, reporting refusals: AsyncStream<ApplicationFailure>.Continuation, watch: Watch, clock: some Clock<Duration> & Sendable) async {
        defer { refusals.finish() }
        let request = NSAppleEventDescriptor(
            eventClass: AEEventClass(kCoreEventClass),
            eventID: AEEventID(kAEQuitApplication),
            targetDescriptor: NSAppleEventDescriptor(processIdentifier: process.pid),
            returnID: AEReturnID(kAutoGenerateReturnID),
            transactionID: AETransactionID(kAnyTransactionID),
        )
        if let saving { request.setParam(NSAppleEventDescriptor(enumCode: saving), forKeyword: keyAESaveOptions) }
        let subject = String(process.pid)
        let refusal: ApplicationFailure?
        do throws(AppleEventFailure) {
            try await NSAppleEventDescriptor.send(request, timeout: NSAppleEventTimeOutDefault) { _ in }
            refusal = nil
        } catch  where error.status == OSStatus(userCanceledErr) {
            _ = await Result { () async throws(any Error) in try await clock.sleep(for: messagingTimeout) }
            refusal =
                switch await watch.current {
                    case .running(let running) where running.asking.isEmpty && !(running.status.isIdle && running.status.is(\.unresponsive)):
                        ApplicationFailure(subject: subject, reason: .refused(nil))
                    case .running, .failed, .exited: nil
                }
        } catch  where process.hasExited {
            refusal = nil
        } catch  where error.status == OSStatus(procNotFound) {
            let signaled = Result { () throws(Errno) in try process.signal(SIGTERM) }
            refusal = if case .failure = signaled, !process.hasExited { ApplicationFailure(subject: subject, reason: .event(error)) } else { nil }
        } catch {
            refusal = ApplicationFailure(subject: subject, reason: .event(error))
        }
        if let refusal { refusals.yield(refusal) }
    }
}
