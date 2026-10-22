public import AppKit
import ApplicationServices
import AsyncAlgorithms
import Encoding
import Results
import Synchronization
import System

// --- [MODELS] --------------------------------------------------------------------------

public enum ApplicationStatus: Equatable, Sendable {
    case launching
    case responding([Element])
    case unresponsive
}

extension ApplicationStatus: Encodable {
    private enum Key: CodingKey {
        case elements
    }

    public func encode(to encoder: any Encoder) throws {
        switch self {
            case .launching: try encoder.encode(case: "launching", keyedBy: Key.self)
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
    var vanished: Errno? {
        var process: proc_bsdinfo = proc_bsdinfo()
        return unsafe proc_pidinfo(processIdentifier, PROC_PIDTBSDINFO, 0, &process, Int32(MemoryLayout<proc_bsdinfo>.size)) <= 0 && errno == ESRCH ? Errno(rawValue: errno) : nil
    }

    @concurrent
    public func status(walking walked: @Sendable (Element) -> Bool) async -> Result<ApplicationStatus, ElementFailure> {
        await withTaskExecutorPreference(accessibility) {
            let application: AXUIElement = AXUIElementCreateApplication(processIdentifier)
            let read: Result<ApplicationStatus, ElementFailure> = Result { () throws(ElementFailure) in
                try .responding(application.candidates().map { candidate throws(ElementFailure) in try candidate.described(walking: walked) }.compactMap(\.self))
            }
            return switch read {
                case .failure(.unanswered): vanished.map { error in .failure(.noProcess(error)) } ?? .success(isFinishedLaunching ? .unresponsive : .launching)
                case .success, .failure: read
            }
        }
    }

    public func statusChanges(walking walked: @escaping @Sendable (Element) -> Bool) -> Result<some AsyncSequence<Result<ApplicationStatus, ElementFailure>, Never>, ElementFailure> {
        let (ticks, tick): (AsyncStream<Void>, AsyncStream<Void>.Continuation) = AsyncStream.makeStream(bufferingPolicy: .bufferingNewest(1))
        let unresponsive: @Sendable (Result<ApplicationStatus, ElementFailure>) -> Bool = { read in if case .success(.unresponsive) = read { true } else { false } }
        return Result { () throws(ElementFailure) in
            let observation: Observation = try Observation(self, ticks: tick)
            tick.onTermination = { _ in withExtendedLifetime(observation) {} }
            return chain([()].async, ticks)
                .map { _ in await self.status(walking: walked) }
                .flatMap { read in
                    chain(
                        [read].async,
                        AsyncTimerSequence(interval: messagingTimeout, clock: .continuous).map { _ in await self.status(walking: walked) }.drop(while: unresponsive).prefix(unresponsive(read) ? 1 : 0),
                    )
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
