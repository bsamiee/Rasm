import AppKit
import ApplicationServices
import AsyncAlgorithms
public import CasePaths
public import Processes
import Synchronization
import System

// --- [MODELS] --------------------------------------------------------------------------

@CasePathable
public enum ApplicationStatus: Encodable, Equatable, Sendable {
    case launching
    case responding(elements: [Element])
    case unresponsive(process: ProcessStatus)
}

// --- [SERVICES] ------------------------------------------------------------------------

@MainActor
final class Observer {
    nonisolated let ticks: AsyncStream<Void>.Continuation
    nonisolated let observed: Mutex<(observer: AXObserver, element: AXUIElement, unregistered: [String])>

    nonisolated init(_ process: pid_t, notifications: Set<String>, ticks: AsyncStream<Void>.Continuation) throws(ApplicationFailure) {
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
        self.ticks = ticks
        observed = Mutex((observer, element, Array(notifications)))
    }

    nonisolated func register() throws(ApplicationFailure) {
        try observed.withLock { observed throws(ApplicationFailure) in
            while let notification: String = observed.unregistered.first {
                let added: AXError = unsafe AXObserverAddNotification(observed.observer, observed.element, notification as CFString, Unmanaged.passUnretained(self).toOpaque())
                guard ![.apiDisabled, .cannotComplete].contains(added) else { throw observed.element.failure(added, reading: [notification]) }
                observed.unregistered.removeFirst()
            }
        }
    }

    isolated deinit {
        observed.withLock { observed in CFRunLoopRemoveSource(CFRunLoopGetMain(), AXObserverGetRunLoopSource(observed.observer), .defaultMode) }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension NSRunningApplication {
    func processStatus() throws(ApplicationFailure) -> ProcessStatus {
        do { return try ProcessStatus(of: processIdentifier) } catch { throw ApplicationFailure(error, process: processIdentifier) }
    }

    func unanswered() -> Result<ApplicationStatus, ApplicationFailure> {
        Result { () throws(ApplicationFailure) in isFinishedLaunching ? .unresponsive(process: try processStatus()) : .launching }
    }

    @concurrent
    func status(walking walked: @Sendable (Element) -> Bool, observer: Observer? = nil) async -> Result<ApplicationStatus, ApplicationFailure> {
        await withTaskExecutorPreference(accessibility) {
            Result { () throws(ApplicationFailure) in
                guard try processStatus() != .stopped else { return .unresponsive(process: .stopped) }
                let elements: [Element] = try AXUIElementCreateApplication(processIdentifier).candidates()
                    .map { candidate throws(ApplicationFailure) in try candidate.described(walking: walked) }.compactMap(\.self)
                try observer?.register()
                return isFinishedLaunching || elements.contains(where: \.requiresAnswer) ? .responding(elements: elements) : .launching
            }
            .flatMapError { failure in failure.reason.is(\.unanswered) ? unanswered() : .failure(failure) }
        }
    }

    @concurrent
    func heartbeat() async -> Result<ApplicationStatus?, ApplicationFailure> {
        await withTaskExecutorPreference(accessibility) {
            Result { () throws(ApplicationFailure) -> ApplicationStatus? in
                guard try processStatus() != .stopped else { return .unresponsive(process: .stopped) }
                _ = try AXUIElementCreateApplication(processIdentifier).values([kAXRoleAttribute])
                return nil
            }
            .flatMapError { failure in failure.reason.is(\.unanswered) ? unanswered().map(Optional.some) : .failure(failure) }
        }
    }

    func statusChanges(
        walking walked: @escaping @Sendable (Element) -> Bool,
        notifications: Set<String>,
        polling interval: Duration,
        clock: some Clock<Duration> & Sendable,
    ) -> some AsyncSequence<Result<ApplicationStatus, ApplicationFailure>, Never> {
        typealias Step = @Sendable (Result<ApplicationStatus, ApplicationFailure>?) async -> Result<ApplicationStatus, ApplicationFailure>
        let (ticks, tick): (AsyncStream<Void>, AsyncStream<Void>.Continuation) = AsyncStream.makeStream(bufferingPolicy: .bufferingNewest(1))
        let observer: Result<Observer, ApplicationFailure> = Result { () throws(ApplicationFailure) in try Observer(processIdentifier, notifications: notifications, ticks: tick) }
        let reread: Step = { _ in await Result { () async throws(ApplicationFailure) in try await self.status(walking: walked, observer: observer.get()).get() } }
        let beat: Step = { last in
            switch (await self.heartbeat(), last) {
                case (.success(.none), .some(let last)) where last.is(\.success.responding): last
                case (.success(.none), _): await reread(last)
                case (.success(.some(let status)), _): .success(status)
                case (.failure(let failure), _): .failure(failure)
            }
        }
        let reason: @Sendable (ApplicationFailure) -> PartialCaseKeyPath<ApplicationFailure.Reason> = { failure in ApplicationFailure.Reason.allCasePaths[failure.reason] }
        return merge(chain([()].async, ticks).map { _ in reread }, AsyncTimerSequence(interval: interval, clock: clock).map { _ in beat })
            .reductions(nil) { last, step in await step(last) }
            .compactMap(\.self)
            .prefix { read in read[case: \.failure]?.reason.is(\.noProcess) != true }
            .removeDuplicates { last, next in
                switch (last, next) {
                    case (.success(let last), .success(let next)): last == next
                    case (.failure(let last), .failure(let next)): last.subject == next.subject && reason(last) == reason(next)
                    case (_, _): false
                }
            }
    }
}
