public import AppKit
import ApplicationServices
import AsyncAlgorithms
import Encoding
import Results
import Synchronization

// --- [MODELS] --------------------------------------------------------------------------

public enum ApplicationStatus: Equatable, Sendable {
    case responding([Element])
    case unresponsive
}

extension ApplicationStatus: Encodable {
    private enum Key: CodingKey {
        case elements
    }

    public func encode(to encoder: any Encoder) throws {
        switch self {
            case .responding(let elements): try encoder.encode(case: "responding", keyedBy: Key.self) { fields in try fields.encode(elements, forKey: .elements) }
            case .unresponsive: try encoder.encode(case: "unresponsive", keyedBy: Key.self)
        }
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

private final class Observation: Sendable {
    let ticks: AsyncStream<Void>.Continuation
    let observer: Mutex<(observer: AXObserver, element: AXUIElement)>

    init(_ application: NSRunningApplication, ticks: AsyncStream<Void>.Continuation) throws(ElementFailure) {
        let names: [String] = [
            kAXWindowCreatedNotification, kAXSheetCreatedNotification, kAXMenuOpenedNotification, kAXMenuClosedNotification, kAXUIElementDestroyedNotification,
            kAXFocusedWindowChangedNotification, kAXMainWindowChangedNotification, kAXTitleChangedNotification, kAXWindowMiniaturizedNotification,
            kAXWindowDeminiaturizedNotification, kAXApplicationHiddenNotification, kAXApplicationShownNotification, kAXValueChangedNotification,
        ]
        let element: AXUIElement = AXUIElementCreateApplication(application.processIdentifier)
        var created: AXObserver?
        let creation: AXError = unsafe AXObserverCreate(application.processIdentifier, observed, &created)
        guard let observer: AXObserver = created else { throw .reading(creation, ["AXObserverCreate"], on: element) }
        self.ticks = ticks
        self.observer = Mutex((observer, element))
        try self.observer.withLock { observed throws(ElementFailure) in
            _ = try traverse(names) { name in
                let added: AXError = unsafe AXObserverAddNotification(observed.observer, observed.element, name as CFString, Unmanaged.passUnretained(self).toOpaque())
                return added == .success ? .success(()) : .failure(AggregateError(first: (notification: name, error: added)))
            }
            .mapError { refusals -> ElementFailure in refusals.first.error == .apiDisabled ? .untrusted : .unobservable(refusals) }.get()
            CFRunLoopAddSource(CFRunLoopGetMain(), AXObserverGetRunLoopSource(observed.observer), .defaultMode)
        }
    }

    deinit {
        observer.withLock { observed in CFRunLoopRemoveSource(CFRunLoopGetMain(), AXObserverGetRunLoopSource(observed.observer), .defaultMode) }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension NSRunningApplication {
    @concurrent
    public func status() async -> Result<ApplicationStatus, ElementFailure> {
        await withTaskExecutorPreference(accessibility) {
            let application: AXUIElement = AXUIElementCreateApplication(processIdentifier)
            let read: Result<ApplicationStatus, ElementFailure> = Result { () throws(ElementFailure) in
                try .responding(application.candidates().map { candidate throws(ElementFailure) in try candidate.described()?.element }.compactMap(\.self))
            }
            return switch read {
                case .failure(.unanswered): .success(.unresponsive)
                case .success, .failure: read
            }
        }
    }

    public func statusChanges() -> Result<some AsyncSequence<Result<ApplicationStatus, ElementFailure>, Never>, ElementFailure> {
        let (ticks, tick): (AsyncStream<Void>, AsyncStream<Void>.Continuation) = AsyncStream.makeStream(bufferingPolicy: .bufferingNewest(1))
        return Result { () throws(ElementFailure) in
            let observation: Observation = try Observation(self, ticks: tick)
            tick.onTermination = { _ in withExtendedLifetime(observation) {} }
            return merge(chain([true].async, ticks.map { _ in true }), AsyncTimerSequence(interval: messagingTimeout, clock: .continuous).map { _ in false })
                .reductions(Result<ApplicationStatus, ElementFailure>?.none) { last, notified in
                    let started: ContinuousClock.Instant = .now
                    let read: Result<ApplicationStatus, ElementFailure> =
                        switch (notified, last) {
                            case (false, .success(let status)) where status != .unresponsive: await self.answers() ? .success(status) : .success(.unresponsive)
                            case (_, _): await self.status()
                        }
                    return switch read {
                        case .success(.unresponsive) where started.duration(to: .now) < messagingTimeout: last
                        case .success, .failure: read
                    }
                }
                .compactMap(\.self)
                .removeDuplicates { last, next in
                    switch (last, next) {
                        case (.success(let last), .success(let next)): last == next
                        case (_, _): false
                    }
                }
        }
    }

    @concurrent
    private func answers() async -> Bool {
        await withTaskExecutorPreference(accessibility) {
            var role: CFTypeRef?
            return unsafe AXUIElementCopyAttributeValue(AXUIElementCreateApplication(processIdentifier), kAXRoleAttribute as CFString, &role) != .cannotComplete
        }
    }
}

private let observed: AXObserverCallback = { _, _, _, refcon in
    guard let refcon: UnsafeMutableRawPointer = unsafe refcon else { return }
    unsafe Unmanaged<Observation>.fromOpaque(refcon).takeUnretainedValue().ticks.yield(())
}
