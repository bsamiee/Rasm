import AppKit
import AsyncAlgorithms
import Carbon.HIToolbox
import SPI

// --- [MODELS] --------------------------------------------------------------------------

package struct Session: Encodable, Equatable, Sendable {
    let onConsole: Bool
    let locked: Bool
    let asleep: Bool
    let secureInput: Bool
    let spaces: [UInt64]
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Session {
    private static var console: (onConsole: Bool, locked: Bool, asleep: Bool) {
        let session = CGSessionCopyCurrentDictionary() as? [String: Any]
        return (session?[kCGSessionOnConsoleKey] as? Bool == true, session?[kCGSessionScreenIsLockedKey] as? Bool == true, CGDisplayIsAsleep(CGMainDisplayID()) != 0)
    }

    static func current() -> Self {
        let (onConsole, locked, asleep) = console
        return Self(
            onConsole: onConsole,
            locked: locked,
            asleep: asleep,
            secureInput: IsSecureEventInputEnabled(),
            spaces: (copySpaces(mainConnection(), [.includesCurrent, .includesUser]) as? [UInt64]) ?? [],
        )
    }

    package static func changes(from watches: Watches) -> some AsyncSequence<Result<Self, ApplicationFailure>, Never> & Sendable {
        let (noticed, notice) = AsyncStream<Void>.makeStream(bufferingPolicy: .bufferingNewest(1))
        let (workspace, distributed): (NotificationCenter, NotificationCenter) = (NSWorkspace.shared.notificationCenter, DistributedNotificationCenter.default())
        let names: [(center: NotificationCenter, name: Notification.Name)] = [
            (workspace, NSWorkspace.screensDidSleepNotification), (workspace, NSWorkspace.screensDidWakeNotification), (workspace, NSWorkspace.activeSpaceDidChangeNotification),
            (workspace, NSWorkspace.sessionDidBecomeActiveNotification), (workspace, NSWorkspace.sessionDidResignActiveNotification),
            (distributed, Notification.Name(kScreenIsLockedNotification)), (distributed, Notification.Name(kScreenIsUnlockedNotification)),
        ]
        nonisolated(unsafe) let observers: [(center: NotificationCenter, token: any NSObjectProtocol)] = names.map { center, name in
            (center, center.addObserver(forName: name, object: nil, queue: nil) { _ in notice.yield() })
        }
        notice.onTermination = { _ in
            for observer in unsafe observers { observer.center.removeObserver(observer.token) }
        }
        let secureNotices: Set<WindowNotification> = [WindowNotification.secureInputEnabled, WindowNotification.secureInputDisabled]
        let secureInput: AsyncStream<Result<WindowServerConnection.Notice, ApplicationFailure>> = watches.notices(of: secureNotices)
        let secureChanges = secureInput.map { (delivered: Result<WindowServerConnection.Notice, ApplicationFailure>) -> Result<Void, ApplicationFailure> in
            delivered.map { (_: WindowServerConnection.Notice) -> Void in () }
        }
        let sessionChanges = noticed.map { (_: Void) -> Result<Void, ApplicationFailure> in .success(()) }
        let changed = chain([Result<Void, ApplicationFailure>.success(())].async, merge(sessionChanges, secureChanges))
        let readings = changed.map { (change: Result<Void, ApplicationFailure>) -> Result<Self, ApplicationFailure> in change.map { (_: Void) -> Self in current() } }
        return readings.removeDuplicates()
    }

    package static func require(_ subject: String) throws(ApplicationFailure) {
        let reason: ApplicationFailure.Reason? =
            switch console {
                case (false, _, _): .notOnConsole
                case (true, true, _): .screenLocked
                case (true, false, true): .displayAsleep
                case (true, false, false): nil
            }
        if let reason { throw ApplicationFailure(subject: subject, reason: reason) }
    }
}
