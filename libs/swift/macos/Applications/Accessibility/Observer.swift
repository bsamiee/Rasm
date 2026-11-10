import AppKit
import ApplicationServices
import Synchronization

// --- [SERVICES] ------------------------------------------------------------------------

final class Observer: Sendable {
    private let ticks: AsyncStream<Void>.Continuation
    private let observed: Mutex<(observer: AXObserver, unregistered: [String], instance: [NSKeyValueObservation])>

    init(_ instance: NSRunningApplication, ticks: AsyncStream<Void>.Continuation) throws(ApplicationFailure) {
        var created: AXObserver?
        let creation = unsafe AXObserverCreate(
            instance.processIdentifier,
            { _, _, _, refcon in
                guard let refcon = unsafe refcon else { return }
                unsafe Unmanaged<Observer>.fromOpaque(refcon).takeUnretainedValue().ticks.yield(())
            },
            &created,
        )
        guard let observer = created else { throw AXUIElement.application(instance.processIdentifier).failure(creation, reading: ["AXObserverCreate"]) }
        CFRunLoopAddSource(CFRunLoopGetMain(), AXObserverGetRunLoopSource(observer), .defaultMode)
        self.ticks = ticks
        observed = Mutex(
            (
                observer,
                [
                    kAXWindowCreatedNotification, kAXSheetCreatedNotification, kAXUIElementDestroyedNotification, kAXFocusedWindowChangedNotification, kAXTitleChangedNotification,
                    kAXWindowMiniaturizedNotification, kAXWindowDeminiaturizedNotification, kAXMenuOpenedNotification, kAXMenuClosedNotification,
                ],
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
                let added = unsafe AXObserverAddNotification(observed.observer, element, notification as CFString, Unmanaged.passUnretained(self).toOpaque())
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
            for token in observed.instance { token.invalidate() }
            observed.instance = []
            CFRunLoopRemoveSource(CFRunLoopGetMain(), AXObserverGetRunLoopSource(observed.observer), .defaultMode)
        }
    }
}
