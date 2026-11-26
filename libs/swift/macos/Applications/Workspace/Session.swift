import AppKit
import AsyncAlgorithms
import Carbon.HIToolbox
import SPI

// --- [TYPES] ---------------------------------------------------------------------------

private enum SecureInputNotification: UInt32, CaseIterable {
    case enabled = 752
    case disabled = 753
}

// --- [MODELS] --------------------------------------------------------------------------

public struct Session: Encodable, Equatable, Sendable {
    public let onConsole: Bool
    public let locked: Bool
    public let asleep: Bool
    public let secureInput: Bool
    public let spaces: [UInt64]
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Session {
    private static var console: (onConsole: Bool, locked: Bool, asleep: Bool) {
        let session = CGSessionCopyCurrentDictionary() as? [String: Any]
        return (session?[kCGSessionOnConsoleKey as String] as? Bool == true, session?["CGSSessionScreenIsLocked"] as? Bool == true, CGDisplayIsAsleep(CGMainDisplayID()) != 0)
    }

    public static func current() -> Self {
        let (onConsole, locked, asleep) = console
        return Self(
            onConsole: onConsole,
            locked: locked,
            asleep: asleep,
            secureInput: IsSecureEventInputEnabled(),
            spaces: (copySpaces(mainConnection(), [.includesCurrent, .includesUser]) as? [UInt64]) ?? [],
        )
    }

    public static func changes() -> some AsyncSequence<Result<Self, ApplicationFailure>, Never> & Sendable {
        let (changed, change) = AsyncStream<Void>.makeStream(bufferingPolicy: .bufferingNewest(1))
        let (workspace, distributed): (NotificationCenter, NotificationCenter) = (NSWorkspace.shared.notificationCenter, DistributedNotificationCenter.default())
        let notices: [(center: NotificationCenter, name: Notification.Name)] = [
            (workspace, NSWorkspace.screensDidSleepNotification), (workspace, NSWorkspace.screensDidWakeNotification), (workspace, NSWorkspace.activeSpaceDidChangeNotification),
            (workspace, NSWorkspace.sessionDidBecomeActiveNotification), (workspace, NSWorkspace.sessionDidResignActiveNotification),
            (distributed, Notification.Name("com.apple.screenIsLocked")), (distributed, Notification.Name("com.apple.screenIsUnlocked")),
        ]
        nonisolated(unsafe) let observers: [(center: NotificationCenter, token: any NSObjectProtocol)] = notices.map { center, name in
            (center, center.addObserver(forName: name, object: nil, queue: nil) { _ in change.yield() })
        }
        var (connection, events): (Int32, mach_port_t) = (0, 0)
        nonisolated(unsafe) let context: Unmanaged<AnyObject> = unsafe Unmanaged.passRetained(change as AnyObject)
        let notified: @convention(c) (UInt32, UnsafeMutableRawPointer?, Int, UnsafeMutableRawPointer?) -> Void = { _, _, _, context in
            unsafe (context.map { context in unsafe Unmanaged<AnyObject>.fromOpaque(context).takeUnretainedValue() } as? AsyncStream<Void>.Continuation)?.yield()
        }
        let answers: [CGError] =
            unsafe [newConnection(0, &connection), eventPort(connection, &events)]
            + SecureInputNotification.allCases.map { notification in unsafe registerNotifyProc(connection, notified, notification.rawValue, context.toOpaque()) }
        let opened: Int32 = connection
        var described = unsafe CFMachPortContext(version: 0, info: UnsafeMutableRawPointer(bitPattern: Int(opened)), retain: nil, release: nil, copyDescription: nil)
        let drain: CFMachPortCallBack = { _, _, _, info in
            let connection = Int32(Int(bitPattern: info))
            while nextEvent(connection) != nil {}
        }
        nonisolated(unsafe) let source: CFRunLoopSource? = unsafe CFMachPortCreateWithPort(nil, events, drain, &described, nil).flatMap { port in CFMachPortCreateRunLoopSource(nil, port, 0) }
        if let source = unsafe source { CFRunLoopAddSource(CFRunLoopGetMain(), source, .commonModes) }
        change.onTermination = { _ in
            for observer in unsafe observers { observer.center.removeObserver(observer.token) }
            DispatchQueue.main.async {
                if let source = unsafe source { CFRunLoopSourceInvalidate(source) }
                _ = releaseConnection(opened)
                unsafe context.release()
            }
        }
        let refused: [Result<Self, ApplicationFailure>] =
            answers.first { answer in answer != .success }.map { answer in
                [.failure(ApplicationFailure(subject: "secure input notifications of window server connection \(opened)", reason: .windowServer(answer)))]
            }
            ?? []
        let readings = chain([()].async, changed).map { _ in current() }.removeDuplicates().map { session -> Result<Self, ApplicationFailure> in .success(session) }
        return chain(refused.async, readings)
    }

    public static func require(_ subject: String) throws(ApplicationFailure) {
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
