package import AppKit
import CasePathsCore
package import Darwin
import Dispatch
import Files
import Foundation
import OrderedCollections
import Results
import SPI
import Synchronization
import System

// --- [TYPES] ---------------------------------------------------------------------------

enum WindowNotification: String, CaseIterable, Encodable, Sendable {
    case created
    case destroyed
    case moved
    case resized
    case reordered
    case levelChanged
    case shown
    case hidden
    case addedToSpace
    case removedFromSpace
    case activeSpaceChanged
    case appUnresponsive
    case appResponsive
    case secureInputEnabled
    case secureInputDisabled

    fileprivate static let posted: [UInt32: Self] = Dictionary(uniqueKeysWithValues: allCases.compactMap { notification in notification.post.map { post in (post.type, notification) } })
    fileprivate static let windowed: Set<WindowNotification> = Set(allCases.filter { notification in notification.post?.window != nil })

    fileprivate var post: (type: UInt32, window: Int?, process: Int?, space: Int?)? {
        let spaced: Int = MemoryLayout<UInt64>.size
        let addressed: Int = MemoryLayout<(minorVersion: UInt16, majorVersion: UInt16, length: UInt32, connection: Int32)>.size
        return switch self {
            case .created: (811, 0, nil, nil)
            case .destroyed: (804, 0, nil, nil)
            case .moved: (806, 0, nil, nil)
            case .resized: (807, 0, nil, nil)
            case .reordered: (808, 0, nil, nil)
            case .shown: (815, 0, nil, nil)
            case .hidden: (816, 0, nil, nil)
            case .addedToSpace: (1325, spaced, nil, 0)
            case .removedFromSpace: (1326, spaced, nil, 0)
            case .activeSpaceChanged: (1401, nil, nil, nil)
            case .appUnresponsive: (750, nil, addressed, nil)
            case .appResponsive: (751, nil, addressed, nil)
            case .secureInputEnabled: (752, nil, nil, nil)
            case .secureInputDisabled: (753, nil, nil, nil)
            case .levelChanged: nil
        }
    }
}

// --- [MODELS] --------------------------------------------------------------------------

extension ProcessSerialNumber {
    fileprivate var number: UInt64 { UInt64(highLongOfPSN) << 32 | UInt64(lowLongOfPSN) }

    fileprivate var applicationName: String {
        processOfSerial(self).flatMap(NSRunningApplication.init(processIdentifier:)).flatMap { application in application.localizedName ?? application.bundleURL?.lastPathComponent }
            ?? "an application without a window server connection"
    }
}

extension RawSpan {
    fileprivate func load<T: ConvertibleFromBytes>(at offset: Int?, as type: T.Type) -> T? {
        guard let offset, byteCount >= offset + MemoryLayout<T>.size else { return nil }
        return load(fromByteOffset: offset, as: type)
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

extension ApplicationFailure {
    fileprivate init(_ answer: CGError, subject: String) {
        self.init(subject: answer == .invalidContext ? "\(subject) before NSApplication loads this process's window server connection" : subject, reason: .windowServer(answer))
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

package final class PermittedFrontList: Sendable {
    private struct Held {
        let journal: FileHandle
        let launches: any NSObjectProtocol
        var holds: [UUID: (excludes: @Sendable (NSRunningApplication) -> Bool, admitted: Set<pid_t>)] = [:]
        var listed: OrderedDictionary<UInt64, ProcessSerialNumber> = [:]
        var joined: Result<Void, ApplicationFailure> = .success(())

        init(journal: FileHandle) {
            self.journal = journal
            launches = NSWorkspace.shared.notificationCenter.addObserver(forName: NSWorkspace.willLaunchApplicationNotification, object: nil, queue: nil) { notification in
                let high = notification.userInfo?["NSApplicationProcessSerialNumberHigh"] as? UInt32
                let low = notification.userInfo?["NSApplicationProcessSerialNumberLow"] as? UInt32
                guard let application = notification.userInfo?[NSWorkspace.applicationUserInfoKey] as? NSRunningApplication, let high, let low else { return }
                let serial = ProcessSerialNumber(highLongOfPSN: high, lowLongOfPSN: low)
                PermittedFrontList.shared.held.withLock { (held: inout Self?) in
                    guard let joining = held, joining.permits(application), joining.listed[serial.number] == nil else { return }
                    let added: Result<Void, ApplicationFailure> = Result { () throws(ApplicationFailure) in try PermittedFrontList.add([serial], to: joining.journal) }
                    held?.listed[serial.number] = serial
                    held?.joined = joining.joined.flatMap { _ in added }
                }
            }
        }

        func permits(_ application: NSRunningApplication) -> Bool {
            !holds.values.contains { hold in hold.excludes(application) && !hold.admitted.contains(application.processIdentifier) }
        }

        mutating func reconcile() throws(ApplicationFailure) {
            let focused = focusedSerials()
            let holders: [ProcessSerialNumber] = [try focused.front.get(), try focused.keyFocus.get()].compactMap(\.self)
            let running: [ProcessSerialNumber] = NSWorkspace.shared.runningApplications.filter { application in application.activationPolicy != .prohibited && permits(application) }
                .compactMap { (application: NSRunningApplication) -> ProcessSerialNumber? in
                    var serial = ProcessSerialNumber()
                    return unsafe findProcess(application.processIdentifier, &serial) ? serial : nil
                }
            let wanted: OrderedDictionary<UInt64, ProcessSerialNumber> = OrderedDictionary((holders + running).map { serial in (serial.number, serial) }) { first, _ in first }
            let removed: OrderedDictionary<UInt64, ProcessSerialNumber> = listed.filter { number, _ in wanted[number] == nil }
            let added: OrderedDictionary<UInt64, ProcessSerialNumber> = wanted.filter { number, _ in listed[number] == nil }
            try PermittedFrontList.add(added.values.elements, to: journal)
            listed.merge(added) { kept, _ in kept }
            listed.removeAll { number, _ in removed[number] != nil }
            try PermittedFrontList.remove(removed.values.elements).removed.get()
        }
    }

    package static let shared = PermittedFrontList()
    private static let path: String = URL.temporaryDirectory.appending(component: String(reflecting: PermittedFrontList.self)).path(percentEncoded: false)
    private static let subject: String = "Permitted front list journal at \(path)"
    private let held: Mutex<Held?> = Mutex(nil)

    private init() {}

    package static var entries: Int { (metaInformation(-2, permittedFrontKey) as? [AnyObject])?.count ?? 0 }

    package static func recover() throws(ApplicationFailure) {
        do throws(ApplicationFailure) { try clear(try open([.exclusiveLock, .nonBlocking])) } catch  where error.reason == .system(.wouldBlock) {}
    }

    package func holding<Value, Failure: Error>(
        excluding target: @escaping @Sendable (NSRunningApplication) -> Bool,
        for lifetime: Duration,
        clock: some Clock<Duration> = ContinuousClock(),
        _ body: () async throws(Failure) -> Value,
    ) async throws(ApplicationFailure) -> (value: Result<Value, Failure>, released: Result<Void, ApplicationFailure>) {
        let (front, keyFocus): (pid_t?, pid_t?) = focusedProcesses()
        let holders: [NSRunningApplication] = [front, keyFocus].compactMap { (process: pid_t?) -> NSRunningApplication? in process.flatMap(NSRunningApplication.init(processIdentifier:)) }
        guard !holders.contains(where: target) else { return (await Result { () async throws(Failure) -> Value in try await body() }, .success(())) }
        let (id, watched): (UUID, FileHandle) = try (UUID(), Self.open([.eventOnly]))
        let waited: Duration = messagingTimeout * 2
        let span: String = waited.formatted(.units(allowed: [.seconds], width: .wide))
        let expired = ApplicationFailure(subject: "Another process held the permitted front list lock for \(span)", reason: .system(.wouldBlock))
        do throws(ApplicationFailure) {
            try await withTimeout(waited, expired: expired, clock: clock) { () async throws(ApplicationFailure) in
                for await _ in FileEvent.events(of: watched, eventMask: .funlock) {
                    let registered: Bool = try self.held.withLock { (held: inout Held?) throws(ApplicationFailure) -> Bool in
                        if held == nil, let journal = try Self.lock() { held = Held(journal: journal) }
                        guard held != nil else { return false }
                        held?.holds[id] = (target, [])
                        try held?.reconcile()
                        return true
                    }
                    if registered { return }
                }
                throw ApplicationFailure(subject: "holding the permitted front list", reason: .cancelled)
            }
        } catch {
            _ = held.withLock { (held: inout Held?) -> Result<Void, ApplicationFailure> in release(id, from: &held) }
            throw error
        }
        return await withTaskGroup(of: Result<Void, ApplicationFailure>.self, returning: (value: Result<Value, Failure>, released: Result<Void, ApplicationFailure>).self) { group in
            group.addTask {
                try? await clock.sleep(for: lifetime)
                return self.held.withLock { (held: inout Held?) -> Result<Void, ApplicationFailure> in self.release(id, from: &held) }
            }
            let value: Result<Value, Failure> = await Result { () async throws(Failure) -> Value in try await body() }
            group.cancelAll()
            return (value, await group.next() ?? .success(()))
        }
    }

    func admit(_ process: pid_t) -> Result<Void, ApplicationFailure> {
        held.withLock { (held: inout Held?) -> Result<Void, ApplicationFailure> in
            guard var admitting = held else { return .success(()) }
            admitting.holds = admitting.holds.mapValues { hold in (hold.excludes, hold.admitted.union([process])) }
            let reconciled: Result<Void, ApplicationFailure> = Result { () throws(ApplicationFailure) in try admitting.reconcile() }
            held = admitting
            return reconciled
        }
    }

    private func release(_ hold: UUID, from held: inout Held?) -> Result<Void, ApplicationFailure> {
        guard var releasing = held, releasing.holds.removeValue(forKey: hold) != nil else { return .success(()) }
        guard releasing.holds.isEmpty else {
            let reconciled: Result<Void, ApplicationFailure> = Result { () throws(ApplicationFailure) in try releasing.reconcile() }
            held = releasing
            return reconciled
        }
        held = nil
        NSWorkspace.shared.notificationCenter.removeObserver(releasing.launches)
        let cleared: Result<Void, ApplicationFailure> = Result { () throws(ApplicationFailure) in try Self.clear(releasing.journal) }
        return releasing.joined.flatMap { _ in cleared }
    }

    private static func lock() throws(ApplicationFailure) -> FileHandle? {
        let journal: FileHandle
        do throws(ApplicationFailure) { journal = try open([.exclusiveLock, .nonBlocking]) } catch  where error.reason == .system(.wouldBlock) { return nil }
        try clear(journal)
        guard entries == 0 else { throw ApplicationFailure(subject: "Holding the permitted front list", reason: .permittedFrontHeld) }
        return journal
    }

    private static func open(_ options: FileDescriptor.OpenOptions) throws(ApplicationFailure) -> FileHandle {
        do {
            let access: FileDescriptor.AccessMode = options.contains(.eventOnly) ? .readOnly : .readWrite
            let descriptor = try FileDescriptor.open(FilePath(path), access, options: options.union([.create, .append, .closeOnExec]), permissions: .ownerReadWrite)
            return FileHandle(fileDescriptor: descriptor.rawValue, closeOnDealloc: true)
        } catch {
            throw ApplicationFailure(subject: subject, reason: (error as? Errno).map(ApplicationFailure.Reason.system) ?? .foundation(error as NSError))
        }
    }

    private static func add(_ serials: [ProcessSerialNumber], to journal: FileHandle) throws(ApplicationFailure) {
        try append(serials, to: journal)
        for var serial in serials {
            let answer: CGError = unsafe addToPermittedFrontList(&serial)
            guard answer == .success else { throw ApplicationFailure(answer, subject: "Adding \(serial.applicationName) to the permitted front list") }
        }
    }

    private static func remove(_ serials: [ProcessSerialNumber]) -> (kept: [ProcessSerialNumber], removed: Result<Void, ApplicationFailure>) {
        let refusals: [(serial: ProcessSerialNumber, answer: CGError)] = serials.compactMap { (serial: ProcessSerialNumber) -> (serial: ProcessSerialNumber, answer: CGError)? in
            var removing = serial
            let answer: CGError = unsafe removeFromPermittedFrontList(&removing)
            return answer == .success ? nil : (serial, answer)
        }
        let removed: Result<Void, ApplicationFailure> =
            refusals.first.map { refusal -> Result<Void, ApplicationFailure> in
                .failure(ApplicationFailure(refusal.answer, subject: "Removing \(refusal.serial.applicationName) from the permitted front list"))
            } ?? .success(())
        return (refusals.map(\.serial), removed)
    }

    private static func append(_ serials: [ProcessSerialNumber], to journal: FileHandle) throws(ApplicationFailure) {
        let (stride, word): (Int, Int) = (MemoryLayout<ProcessSerialNumber>.stride, MemoryLayout<UInt32>.size)
        var journaled = Data(count: serials.count * stride)
        var bytes = journaled.mutableBytes
        for (index, serial) in serials.enumerated() {
            bytes.storeBytes(of: serial.highLongOfPSN, toByteOffset: index * stride, as: UInt32.self)
            bytes.storeBytes(of: serial.lowLongOfPSN, toByteOffset: index * stride + word, as: UInt32.self)
        }
        do { try journal.write(contentsOf: journaled) } catch { throw ApplicationFailure(subject: subject, reason: .foundation(error as NSError)) }
    }

    private static func clear(_ journal: FileHandle) throws(ApplicationFailure) {
        let journaled: Data
        do {
            try journal.seek(toOffset: 0)
            journaled = try journal.readToEnd() ?? Data()
        } catch {
            throw ApplicationFailure(subject: subject, reason: .foundation(error as NSError))
        }
        var front = ProcessSerialNumber()
        let fronted: Bool = unsafe frontSerial(&front) == .success
        let (stride, word): (Int, Int) = (MemoryLayout<ProcessSerialNumber>.stride, MemoryLayout<UInt32>.size)
        let recorded: [ProcessSerialNumber] = (0..<journaled.count / stride).map { index in
            ProcessSerialNumber(
                highLongOfPSN: journaled.bytes.load(fromByteOffset: index * stride, as: UInt32.self),
                lowLongOfPSN: journaled.bytes.load(fromByteOffset: index * stride + word, as: UInt32.self),
            )
        }
        let serials: OrderedDictionary<UInt64, ProcessSerialNumber> = OrderedDictionary(recorded.reversed().map { serial in (serial.number, serial) }) { first, _ in first }
        let last: UInt64? = fronted ? front.number : nil
        let ordered: [ProcessSerialNumber] = serials.filter { number, _ in number != last }.values.elements + serials.filter { number, _ in number == last }.values.elements
        let removal: (kept: [ProcessSerialNumber], removed: Result<Void, ApplicationFailure>) = remove(ordered)
        do { try journal.truncate(atOffset: 0) } catch { throw ApplicationFailure(subject: subject, reason: .foundation(error as NSError)) }
        try append(removal.kept, to: journal)
        try removal.removed.get()
    }
}

final class WindowServerConnection: Sendable {
    struct Notice: Sendable {
        let notification: WindowNotification
        let window: CGWindowID?
        let owner: pid_t?
        let space: UInt64?
    }

    private struct Held {
        var owners: [CGWindowID: pid_t] = [:]
        var requested: Result<Void, ApplicationFailure> = .success(())
        var subscribers: [UUID: (notifications: Set<WindowNotification>, owner: pid_t?, continuation: AsyncStream<Result<Notice, ApplicationFailure>>.Continuation)] = [:]

        mutating func request(on connection: Int32) {
            let rows: [[String: Any]] = CGWindowListCopyWindowInfo(.optionAll, kCGNullWindowID) as? [[String: Any]] ?? []
            owners = Dictionary(
                rows.compactMap { row in (row[kCGWindowNumber as String] as? CGWindowID).flatMap { number in (row[kCGWindowOwnerPID as String] as? pid_t).map { owner in (number, owner) } } }
            ) { first, _ in first }
            let windows: [CGWindowID] = Array(owners.keys)
            let answer: CGError = requestNotifications(connection, windows.span)
            let answered: Result<Void, ApplicationFailure> =
                answer == .success
                ? .success(())
                : .failure(ApplicationFailure(subject: "Requesting move, resize, order, visibility, and close notices of \(windows.count.formatted()) windows", reason: .windowServer(answer)))
            guard case .success = exchange(&requested, with: answered), case .failure(let failure) = answered else { return }
            for subscriber in subscribers.values where !subscriber.notifications.isDisjoint(with: WindowNotification.windowed) { subscriber.continuation.yield(.failure(failure)) }
        }
    }

    private final class Subscriptions: Sendable {
        let connection: Int32
        let held: Mutex<Held> = Mutex(Held())

        init(connection: Int32) {
            self.connection = connection
        }

        func deliver(_ notification: WindowNotification, _ payload: borrowing RawSpan) {
            let post = notification.post
            let window: CGWindowID? = payload.load(at: post?.window, as: CGWindowID.self)
            let addressed: pid_t? = payload.load(at: post?.process, as: pid_t.self)
            let space: UInt64? = payload.load(at: post?.space, as: UInt64.self)
            held.withLock { held in
                if let window, held.owners[window] == nil { held.request(on: connection) }
                let owner: pid_t? = addressed ?? window.flatMap { window in held.owners[window] }
                let responsible: pid_t? = owner.map(responsiblePID)
                let broadcast: Bool = window == nil && addressed == nil
                let reached = held.subscribers.values.filter { subscriber in
                    subscriber.notifications.contains(notification) && (broadcast || subscriber.owner.map { process in process == owner || process == responsible } != false)
                }
                for subscriber in reached { subscriber.continuation.yield(.success(Notice(notification: notification, window: window, owner: owner, space: space))) }
            }
        }
    }

    private let subscriptions: Subscriptions
    private let source: Mutex<RunLoopSource?> = Mutex(nil)

    init() throws(ApplicationFailure) {
        var (connection, events): (Int32, mach_port_t) = (0, 0)
        let opened: CGError = unsafe newConnection(0, &connection)
        guard opened == .success else { throw ApplicationFailure(subject: "Opening a window server connection", reason: .windowServer(opened)) }
        subscriptions = Subscriptions(connection: connection)
        let context = unsafe Unmanaged.passRetained(subscriptions).toOpaque()
        let callback: @convention(c) (UInt32, UnsafeMutableRawPointer?, Int, UnsafeMutableRawPointer?) -> Void = { type, data, length, context in
            guard let notification = WindowNotification.posted[type], let context = unsafe context else { return }
            let buffer = unsafe UnsafeRawBufferPointer(start: data, count: length)
            let payload = unsafe RawSpan(_unsafeBytes: buffer)
            unsafe Unmanaged<Subscriptions>.fromOpaque(context).takeUnretainedValue().deliver(notification, payload)
        }
        let answers: [CGError] = unsafe [eventPort(connection, &events)] + WindowNotification.posted.keys.map { type in unsafe registerNotifyProc(connection, callback, type, context) }
        let refused: CGError? = answers.first { answer in answer != .success }
        let drained: Bool = source.withLock { source in
            var described = unsafe CFMachPortContext(version: 0, info: UnsafeMutableRawPointer(bitPattern: Int(connection)), retain: nil, release: nil, copyDescription: nil)
            let drain: CFMachPortCallBack = { _, _, _, info in
                let connection = Int32(Int(bitPattern: info))
                while nextEvent(connection) != nil {}
            }
            source = unsafe CFMachPortCreateWithPort(nil, events, drain, &described, nil).flatMap { port in CFMachPortCreateRunLoopSource(nil, port, 0) }.map(RunLoopSource.init)
            return source != nil
        }
        guard refused == nil, drained else { throw ApplicationFailure(subject: "Registering for window server notices", reason: .windowServer(refused ?? .failure)) }
    }

    deinit {
        let ending: [AsyncStream<Result<Notice, ApplicationFailure>>.Continuation] = subscriptions.held.withLock { held in held.subscribers.values.map(\.continuation) }
        for subscriber in ending { subscriber.finish() }
        source.withLock { source in source = nil }
        DispatchQueue.main.async { [subscriptions] in
            _ = releaseConnection(subscriptions.connection)
            unsafe Unmanaged.passUnretained(subscriptions).release()
        }
    }

    func notices(of notifications: Set<WindowNotification>, from owner: pid_t? = nil) -> AsyncStream<Result<Notice, ApplicationFailure>> {
        let (stream, continuation) = AsyncStream<Result<Notice, ApplicationFailure>>.makeStream()
        let id = UUID()
        continuation.onTermination = { [subscriptions] _ in subscriptions.held.withLock { held in held.subscribers[id] = nil } }
        let windowed: Bool = !notifications.isDisjoint(with: WindowNotification.windowed)
        subscriptions.held.withLock { held in
            if windowed { held.request(on: subscriptions.connection) }
            held.subscribers[id] = (notifications, owner, continuation)
            if windowed, case .failure(let failure) = held.requested { continuation.yield(.failure(failure)) }
        }
        return stream
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

func pendingEventAge(of serial: ProcessSerialNumber) throws(ApplicationFailure) -> Duration? {
    var (serial, seconds, flags): (ProcessSerialNumber, Double, UInt32) = (serial, 0, 0)
    let answer: CGError = unsafe unresponsiveStatus(mainConnection(), &serial, &seconds, &flags)
    guard answer == .success else { throw ApplicationFailure(subject: "Unresponsive status of \(serial.applicationName)", reason: .windowServer(answer)) }
    return flags == 0 ? nil : .seconds(seconds)
}

package func focusedProcesses() -> (front: pid_t?, keyFocus: pid_t?) {
    let focused = focusedSerials()
    return (focused.front[case: \.success].flatMap(\.self).flatMap(processOfSerial), focused.keyFocus[case: \.success].flatMap(\.self).flatMap(processOfSerial))
}

func windowDescription(of number: CGWindowID) -> (bounds: CGRect, owner: pid_t)? {
    var listed: UnsafeRawPointer? = unsafe UnsafeRawPointer(bitPattern: UInt(number))
    let row: [String: Any]? = unsafe (CGWindowListCreateDescriptionFromArray(CFArrayCreate(nil, &listed, 1, nil)) as? [[String: Any]])?.first
    guard let owner = row?[kCGWindowOwnerPID as String] as? pid_t, let bounds = row?[kCGWindowBounds as String] as? [String: Any] else { return nil }
    return CGRect(dictionaryRepresentation: bounds as CFDictionary).map { (bounds: CGRect) -> (bounds: CGRect, owner: pid_t) in (bounds, owner) }
}

private func focusedSerials() -> (front: Result<ProcessSerialNumber?, ApplicationFailure>, keyFocus: Result<ProcessSerialNumber?, ApplicationFailure>) {
    var (front, key, flag): (ProcessSerialNumber, ProcessSerialNumber, UInt8) = (ProcessSerialNumber(), ProcessSerialNumber(), 0)
    let read: (front: CGError, key: CGError) = unsafe (frontSerial(&front), keyFocusSerial(&key, &flag))
    let focus = { (answer: CGError, serial: ProcessSerialNumber, subject: String) -> Result<ProcessSerialNumber?, ApplicationFailure> in
        switch (answer, answer.rawValue) {
            case (.success, _): .success(serial)
            case (_, Int32(procNotFound)): .success(nil)
            case (_, _): .failure(ApplicationFailure(answer, subject: subject))
        }
    }
    return (focus(read.front, front, "Reading the front process"), focus(read.key, key, "Reading the key-focus process"))
}

private func processOfSerial(_ serial: ProcessSerialNumber) -> pid_t? {
    var (serial, connection, process): (ProcessSerialNumber, Int32, pid_t) = (serial, 0, 0)
    guard unsafe connectionOfSerial(mainConnection(), &serial, &connection) == .success, unsafe processOfConnection(connection, &process) == .success else { return nil }
    return process
}
