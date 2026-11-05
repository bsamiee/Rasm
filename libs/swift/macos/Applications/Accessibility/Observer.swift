import AppKit
import ApplicationServices
import Synchronization

// --- [CONSTANTS] -----------------------------------------------------------------------

private let notifications: [String] = [
    kAXWindowCreatedNotification, kAXSheetCreatedNotification, kAXUIElementDestroyedNotification, kAXFocusedWindowChangedNotification, kAXTitleChangedNotification,
    kAXWindowMiniaturizedNotification, kAXWindowDeminiaturizedNotification, kAXMenuOpenedNotification, kAXMenuClosedNotification,
]

// --- [SERVICES] ------------------------------------------------------------------------

final class Observer: Sendable {
    private let ticks: AsyncStream<Void>.Continuation
    private let observed: Mutex<(observer: AXObserver, unregistered: [String], instance: [NSKeyValueObservation])>

    init(_ instance: NSRunningApplication, ticks: AsyncStream<Void>.Continuation) throws(ApplicationFailure) {
        var created: AXObserver?
        let creation: AXError = unsafe AXObserverCreate(
            instance.processIdentifier,
            { _, _, _, refcon in
                guard let refcon: UnsafeMutableRawPointer = unsafe refcon else { return }
                unsafe Unmanaged<Observer>.fromOpaque(refcon).takeUnretainedValue().ticks.yield(())
            },
            &created,
        )
        guard let observer: AXObserver = created else { throw AXUIElement.application(instance.processIdentifier).failure(creation, reading: ["AXObserverCreate"]) }
        CFRunLoopAddSource(CFRunLoopGetMain(), AXObserverGetRunLoopSource(observer), .defaultMode)
        self.ticks = ticks
        observed = Mutex(
            (
                observer, notifications,
                [
                    instance.observe(\.isFinishedLaunching) { _, _ in ticks.yield(()) },
                    instance.observe(\.isActive) { _, _ in ticks.yield(()) },
                ],
            )
        )
        ticks.onTermination = { [self] _ in stop() }
    }

    func register(on element: AXUIElement) throws(ApplicationFailure) {
        try observed.withLock { observed throws(ApplicationFailure) in
            observed.unregistered = try observed.unregistered.map { notification throws(ApplicationFailure) -> String? in
                let added: AXError = unsafe AXObserverAddNotification(observed.observer, element, notification as CFString, Unmanaged.passUnretained(self).toOpaque())
                return switch added {
                    case .success, .notificationAlreadyRegistered, .notificationUnsupported: nil
                    case .apiDisabled, .cannotComplete: throw element.failure(added, reading: [notification])
                    default: notification
                }
            }
            .compactMap(\.self)
        }
    }

    private func stop() {
        observed.withLock { observed in
            for token: NSKeyValueObservation in observed.instance { token.invalidate() }
            observed.instance = []
            CFRunLoopRemoveSource(CFRunLoopGetMain(), AXObserverGetRunLoopSource(observed.observer), .defaultMode)
        }
    }
}
