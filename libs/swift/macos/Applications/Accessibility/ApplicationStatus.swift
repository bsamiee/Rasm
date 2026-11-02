import AppKit
import ApplicationServices
import AsyncAlgorithms
public import CasePaths
public import Processes
import Synchronization

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
    nonisolated let instance: NSRunningApplication
    nonisolated let ticks: AsyncStream<Void>.Continuation
    nonisolated let observed: Mutex<(observer: AXObserver, unregistered: [String], launch: NSKeyValueObservation)>

    nonisolated init(_ instance: NSRunningApplication, notifications: Set<String>, ticks: AsyncStream<Void>.Continuation) throws(ApplicationFailure) {
        let element: AXUIElement = AXUIElementCreateApplication(instance.processIdentifier)
        var created: AXObserver?
        let creation: AXError = unsafe AXObserverCreate(
            instance.processIdentifier,
            { _, _, _, refcon in
                guard let refcon: UnsafeMutableRawPointer = unsafe refcon else { return }
                unsafe Unmanaged<Observer>.fromOpaque(refcon).takeUnretainedValue().ticks.yield(())
            },
            &created,
        )
        guard let observer: AXObserver = created else { throw element.failure(creation, reading: ["AXObserverCreate"]) }
        CFRunLoopAddSource(CFRunLoopGetMain(), AXObserverGetRunLoopSource(observer), .defaultMode)
        self.instance = instance
        self.ticks = ticks
        observed = Mutex((observer, Array(notifications), instance.observe(\.isFinishedLaunching) { _, _ in ticks.yield(()) }))
    }

    nonisolated func register() throws(ApplicationFailure) {
        let element: AXUIElement = AXUIElementCreateApplication(instance.processIdentifier)
        try observed.withLock { observed throws(ApplicationFailure) in
            while let notification: String = observed.unregistered.first {
                let added: AXError = unsafe AXObserverAddNotification(observed.observer, element, notification as CFString, Unmanaged.passUnretained(self).toOpaque())
                guard ![.apiDisabled, .cannotComplete].contains(added) else { throw element.failure(added, reading: [notification]) }
                observed.unregistered.removeFirst()
            }
        }
    }

    isolated deinit {
        observed.withLock { observed in
            observed.launch.invalidate()
            CFRunLoopRemoveSource(CFRunLoopGetMain(), AXObserverGetRunLoopSource(observed.observer), .defaultMode)
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension NSRunningApplication {
    func processStatus() throws(ApplicationFailure) -> ProcessStatus {
        do { return try ProcessStatus(of: processIdentifier) } catch { throw ApplicationFailure(error, process: processIdentifier) }
    }

    @concurrent
    func status(
        walking walked: @Sendable (Element) -> Bool,
        observer: Result<Observer, ApplicationFailure>? = nil,
        after last: Result<ApplicationStatus, ApplicationFailure>? = nil,
    ) async -> Result<ApplicationStatus, ApplicationFailure> {
        await withTaskExecutorPreference(accessibility) {
            Result { () throws(ApplicationFailure) in
                let application: AXUIElement = AXUIElementCreateApplication(processIdentifier)
                switch (try processStatus(), last) {
                    case (.stopped, _): return .unresponsive(process: .stopped)
                    case (_, .success(.responding(let elements))):
                        _ = try application.values([kAXRoleAttribute])
                        return .responding(elements: elements)
                    case (_, _):
                        let elements: [Element] = try application.candidates().map { candidate throws(ApplicationFailure) in try candidate.described(walking: walked) }.compactMap(\.self)
                        try observer?.get().register()
                        return isFinishedLaunching || elements.contains(where: \.requiresAnswer) ? .responding(elements: elements) : .launching
                }
            }
            .flatMapError { failure in
                failure.reason.is(\.unanswered)
                    ? Result { () throws(ApplicationFailure) in isFinishedLaunching ? .unresponsive(process: try processStatus()) : .launching } : .failure(failure)
            }
        }
    }

    func statusChanges(
        walking walked: @escaping @Sendable (Element) -> Bool,
        notifications: Set<String>,
        polling interval: Duration,
        clock: some Clock<Duration> & Sendable,
    ) -> some AsyncSequence<Result<ApplicationStatus, ApplicationFailure>, Never> {
        let (ticks, tick): (AsyncStream<Void>, AsyncStream<Void>.Continuation) = AsyncStream.makeStream(bufferingPolicy: .bufferingNewest(1))
        let observer: Result<Observer, ApplicationFailure> = Result { () throws(ApplicationFailure) in try Observer(self, notifications: notifications, ticks: tick) }
        let reason: @Sendable (ApplicationFailure) -> PartialCaseKeyPath<ApplicationFailure.Reason> = { failure in ApplicationFailure.Reason.allCasePaths[failure.reason] }
        return merge(chain([()].async, ticks).map { _ in false }, AsyncTimerSequence(interval: interval, clock: clock).map { _ in true })
            .reductions(Result<ApplicationStatus, ApplicationFailure>.success(.launching)) { last, beat in
                await self.status(walking: walked, observer: observer, after: beat ? last : nil)
            }
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
