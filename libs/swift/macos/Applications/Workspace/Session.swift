import AppKit
import AsyncAlgorithms
import Carbon.HIToolbox

// --- [MODELS] --------------------------------------------------------------------------

public struct Session: Encodable, Equatable, Sendable {
    public let onConsole: Bool
    public let locked: Bool
    public let asleep: Bool
    public let secureInput: Bool
    public let space: UInt64
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Session {
    public static func current() -> Self {
        let session = CGSessionCopyCurrentDictionary() as? [String: Any]
        return Self(
            onConsole: session?[kCGSessionOnConsoleKey as String] as? Bool == true,
            locked: session?["CGSSessionScreenIsLocked"] as? Bool == true,
            asleep: CGDisplayIsAsleep(CGMainDisplayID()) != 0,
            secureInput: IsSecureEventInputEnabled(),
            space: activeSpace(mainConnection()),
        )
    }

    public static func changes() -> some AsyncSequence<Self, Never> & Sendable {
        let (workspace, distributed) = (NSWorkspace.shared.notificationCenter, DistributedNotificationCenter.default())
        let notices: [NotificationCenter.Notifications] = [
            workspace.notifications(named: NSWorkspace.screensDidSleepNotification),
            workspace.notifications(named: NSWorkspace.screensDidWakeNotification),
            workspace.notifications(named: NSWorkspace.activeSpaceDidChangeNotification),
            workspace.notifications(named: NSWorkspace.sessionDidBecomeActiveNotification),
            workspace.notifications(named: NSWorkspace.sessionDidResignActiveNotification),
            distributed.notifications(named: Notification.Name("com.apple.screenIsLocked")),
            distributed.notifications(named: Notification.Name("com.apple.screenIsUnlocked")),
        ]
        let (changed, change) = AsyncStream<Void>.makeStream(bufferingPolicy: .bufferingNewest(1))
        let observing = Task {
            await withDiscardingTaskGroup { group in
                for notice in notices { group.addTask { for await _ in notice { change.yield() } } }
            }
        }
        change.onTermination = { _ in observing.cancel() }
        return chain([()].async, changed).map { current() }.removeDuplicates()
    }

    public func require(_ subject: String) throws(ApplicationFailure) {
        let reason: ApplicationFailure.Reason? =
            switch (onConsole, locked, asleep) {
                case (false, _, _): .notOnConsole
                case (true, true, _): .screenLocked
                case (true, false, true): .displayAsleep
                case (true, false, false): nil
            }
        if let reason { throw ApplicationFailure(subject: subject, reason: reason) }
    }
}

@_silgen_name("CGSGetActiveSpace")
private func activeSpace(_: Int32) -> UInt64
