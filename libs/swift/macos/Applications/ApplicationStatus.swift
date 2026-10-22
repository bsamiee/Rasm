public import AppKit
import ApplicationServices
import AsyncAlgorithms
import Results
import Synchronization

// --- [MODELS] --------------------------------------------------------------------------

public enum ApplicationStatus: Encodable, Equatable, Sendable {
    case launching
    case responding(elements: [Element])
    case unresponsive
}

// --- [SERVICES] ------------------------------------------------------------------------

private final class Observation: Sendable {
    let ticks: AsyncStream<Void>.Continuation
    let observer: Mutex<(observer: AXObserver, element: AXUIElement, unregistered: [String])>

    init(_ application: NSRunningApplication, ticks: AsyncStream<Void>.Continuation) throws(ElementFailure) {
        let element: AXUIElement = AXUIElementCreateApplication(application.processIdentifier)
        var created: AXObserver?
        let creation: AXError = unsafe AXObserverCreate(application.processIdentifier, observed, &created)
        guard let observer: AXObserver = created else { throw .reading(creation, ["AXObserverCreate"], on: element) }
        CFRunLoopAddSource(CFRunLoopGetMain(), AXObserverGetRunLoopSource(observer), .defaultMode)
        self.ticks = ticks
        self.observer = Mutex(
            (
                observer, element,
                [
                    kAXWindowCreatedNotification, kAXSheetCreatedNotification, kAXMenuOpenedNotification, kAXMenuClosedNotification, kAXUIElementDestroyedNotification,
                    kAXFocusedWindowChangedNotification, kAXMainWindowChangedNotification, kAXTitleChangedNotification, kAXWindowMiniaturizedNotification,
                    kAXWindowDeminiaturizedNotification, kAXApplicationHiddenNotification, kAXApplicationShownNotification, kAXValueChangedNotification,
                ],
            )
        )
    }

    func register() throws(ElementFailure) {
        try observer.withLock { observed throws(ElementFailure) in
            let refusals: [(notification: String, error: AXError)] = observed.unregistered.compactMap { name in
                let added: AXError = unsafe AXObserverAddNotification(observed.observer, observed.element, name as CFString, Unmanaged.passUnretained(self).toOpaque())
                return [.success, .notificationAlreadyRegistered].contains(added) ? nil : (name, added)
            }
            observed.unregistered = refusals.map(\.notification)
            switch AggregateError(refusals) {
                case .none: return
                case .some(let refused) where refused.first.error == .apiDisabled: throw .untrusted
                case .some(let refused) where refused.errors.allSatisfy({ refusal in refusal.error == .cannotComplete }): throw .unanswered(nil)
                case .some(let refused): throw .unobservable(refused)
            }
        }
    }

    deinit {
        observer.withLock { observed in CFRunLoopRemoveSource(CFRunLoopGetMain(), AXObserverGetRunLoopSource(observed.observer), .defaultMode) }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension AXUIElement {
    func responding(walking walked: (Element) -> Bool) throws(ElementFailure) -> ApplicationStatus {
        try .responding(elements: candidates().map { candidate throws(ElementFailure) in try candidate.described(walking: walked) }.compactMap(\.self))
    }
}

extension NSRunningApplication {
    var vanished: Bool {
        var process: proc_bsdinfo = proc_bsdinfo()
        return unsafe proc_pidinfo(processIdentifier, PROC_PIDTBSDINFO, 0, &process, Int32(MemoryLayout<proc_bsdinfo>.size)) <= 0 && errno == ESRCH
    }

    @concurrent
    public func status(walking walked: @Sendable (Element) -> Bool) async -> Result<ApplicationStatus, ElementFailure> {
        await status { application throws(ElementFailure) in try application.responding(walking: walked) }
    }

    @concurrent
    private func status(_ read: @Sendable (AXUIElement) throws(ElementFailure) -> ApplicationStatus) async -> Result<ApplicationStatus, ElementFailure> {
        await withTaskExecutorPreference(accessibility) {
            let application: AXUIElement = AXUIElementCreateApplication(processIdentifier)
            return switch Result(catching: { () throws(ElementFailure) in try read(application) }) {
                case .failure(.unanswered) where vanished: .failure(.noProcess)
                case .failure(.unanswered): .success(isFinishedLaunching ? .unresponsive : .launching)
                case let other: other
            }
        }
    }

    public func statusChanges(walking walked: @escaping @Sendable (Element) -> Bool) -> Result<some AsyncSequence<Result<ApplicationStatus, ElementFailure>, Never>, ElementFailure> {
        let (ticks, tick): (AsyncStream<Void>, AsyncStream<Void>.Continuation) = AsyncStream.makeStream(bufferingPolicy: .bufferingNewest(1))
        let unresponsive: @Sendable (Result<ApplicationStatus, ElementFailure>) -> Bool = { read in if case .success(.unresponsive) = read { true } else { false } }
        return Result { () throws(ElementFailure) in
            let observation: Observation = try Observation(self, ticks: tick)
            tick.onTermination = { _ in withExtendedLifetime(observation) {} }
            let current: @Sendable () async -> Result<ApplicationStatus, ElementFailure> = {
                await self.status { application throws(ElementFailure) in
                    try observation.register()
                    return try application.responding(walking: walked)
                }
            }
            return chain([()].async, ticks)
                .map { _ in await current() }
                .flatMap { read in
                    chain([read].async, AsyncTimerSequence(interval: messagingTimeout, clock: .continuous).map { _ in await current() }.drop(while: unresponsive).prefix(unresponsive(read) ? 1 : 0))
                }
                .prefix { read in if case .failure(.noProcess) = read { false } else { true } }
                .removeDuplicates { last, next in
                    switch (last, next) {
                        case (.success(let last), .success(let next)): last == next
                        case (_, _): false
                    }
                }
        }
    }
}

private let observed: AXObserverCallback = { _, _, _, refcon in
    guard let refcon: UnsafeMutableRawPointer = unsafe refcon else { return }
    unsafe Unmanaged<Observation>.fromOpaque(refcon).takeUnretainedValue().ticks.yield(())
}
