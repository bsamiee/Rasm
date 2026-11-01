import AppKit
import ApplicationServices
import AsyncAlgorithms
public import CasePaths
import Results
import Synchronization

// --- [MODELS] --------------------------------------------------------------------------

@CasePathable
public enum ApplicationStatus: Encodable, Equatable, Sendable {
    case launching
    case responding(elements: [Element])
    case unresponsive
}

// --- [SERVICES] ------------------------------------------------------------------------

final class Observer: Sendable {
    let process: pid_t
    let ticks: AsyncStream<Void>.Continuation
    let observed: Mutex<(observer: AXObserver, element: AXUIElement, unregistered: [String])>

    init(_ process: pid_t, notifications: Set<String>, ticks: AsyncStream<Void>.Continuation) throws(ApplicationFailure) {
        let element: AXUIElement = AXUIElementCreateApplication(process)
        var created: AXObserver?
        let creation: AXError = unsafe AXObserverCreate(
            process,
            { _, _, _, refcon in
                guard let refcon: UnsafeMutableRawPointer = unsafe refcon else { return }
                unsafe Unmanaged<Observer>.fromOpaque(refcon).takeUnretainedValue().ticks.yield(())
            },
            &created,
        )
        guard let observer: AXObserver = created else { throw element.failure(creation, reading: ["AXObserverCreate"]) }
        CFRunLoopAddSource(CFRunLoopGetMain(), AXObserverGetRunLoopSource(observer), .defaultMode)
        self.process = process
        self.ticks = ticks
        observed = Mutex((observer, element, Array(notifications)))
    }

    func register() throws(ApplicationFailure) {
        try observed.withLock { observed throws(ApplicationFailure) in
            let refusals: [(notification: String, error: AXError)] = observed.unregistered.compactMap { name in
                let added: AXError = unsafe AXObserverAddNotification(observed.observer, observed.element, name as CFString, Unmanaged.passUnretained(self).toOpaque())
                return [.success, .notificationAlreadyRegistered].contains(added) ? nil : (name, added)
            }
            observed.unregistered = refusals.map(\.notification)
            switch AggregateError(refusals) {
                case .none: return
                case .some(let refused) where refused.first.error == .apiDisabled: throw ApplicationFailure(subject: ProcessInfo.processInfo.processName, reason: .untrusted)
                case .some(let refused) where refused.errors.allSatisfy({ refusal in refusal.error == .cannotComplete }):
                    throw ApplicationFailure(subject: String(process), reason: .unanswered(nil))
                case .some(let refused): throw ApplicationFailure(subject: String(process), reason: .unobservable(refused))
            }
        }
    }

    deinit {
        observed.withLock { observed in CFRunLoopRemoveSource(CFRunLoopGetMain(), AXObserverGetRunLoopSource(observed.observer), .defaultMode) }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension NSRunningApplication {
    @concurrent
    func status(walking walked: @Sendable (Element) -> Bool, observer: Observer? = nil) async -> Result<ApplicationStatus, ApplicationFailure> {
        await withTaskExecutorPreference(accessibility) {
            let application: AXUIElement = AXUIElementCreateApplication(processIdentifier)
            let read: Result<ApplicationStatus, ApplicationFailure> = Result { () throws(ApplicationFailure) in
                try observer?.register()
                return try .responding(elements: application.candidates().map { candidate throws(ApplicationFailure) in try candidate.described(walking: walked) }.compactMap(\.self))
            }
            return switch read {
                case .failure(let failure) where failure.reason.is(\.unanswered) && vanished(processIdentifier):
                    .failure(ApplicationFailure(subject: String(processIdentifier), reason: .noProcess))
                case .failure(let failure) where failure.reason.is(\.unanswered): .success(isFinishedLaunching ? .unresponsive : .launching)
                case let other: other
            }
        }
    }

    func statusChanges(
        walking walked: @escaping @Sendable (Element) -> Bool,
        notifications: Set<String>,
        polling interval: Duration,
    ) -> some AsyncSequence<Result<ApplicationStatus, ApplicationFailure>, Never> {
        let (ticks, tick): (AsyncStream<Void>, AsyncStream<Void>.Continuation) = AsyncStream.makeStream(bufferingPolicy: .bufferingNewest(1))
        let observer: Result<Observer, ApplicationFailure> = Result { () throws(ApplicationFailure) in try Observer(processIdentifier, notifications: notifications, ticks: tick) }
        tick.onTermination = { _ in withExtendedLifetime(observer) {} }
        let unresponsive: @Sendable (Result<ApplicationStatus, ApplicationFailure>) -> Bool = { read in read.is(\.success.unresponsive) }
        let current: @Sendable () async -> Result<ApplicationStatus, ApplicationFailure> = {
            await Result { () async throws(ApplicationFailure) in try await self.status(walking: walked, observer: observer.get()).get() }
        }
        return chain([()].async, ticks)
            .map { _ in await current() }
            .flatMap { read in
                chain([read].async, AsyncTimerSequence(interval: interval, clock: .continuous).map { _ in await current() }.drop(while: unresponsive).prefix(unresponsive(read) ? 1 : 0))
            }
            .prefix { read in read[case: \.failure]?.reason.is(\.noProcess) != true }
            .removeDuplicates { last, next in
                switch (last, next) {
                    case (.success(let last), .success(let next)): last == next
                    case (_, _): false
                }
            }
    }
}
