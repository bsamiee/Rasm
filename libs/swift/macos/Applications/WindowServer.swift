package import AppKit
import AsyncAlgorithms
package import Darwin
import Dispatch
import Encoding
import Foundation
import OrderedCollections
import SPI
import Synchronization
import System

// --- [TYPES] ---------------------------------------------------------------------------

enum WindowNotification: UInt32, CaseIterable, CaseLabeled, Sendable {
    case moved = 806
    case resized = 807
    case reordered = 808
    case levelChanged = 811
    case shown = 815
    case hidden = 816
    case created = 1325
    case destroyed = 1326
    case activeSpaceChanged = 1401
    case appUnresponsive = 750
    case appResponsive = 751

    func window(in payload: UnsafeRawBufferPointer) -> CGWindowID? {
        let offset: Int? =
            switch self {
                case .created, .destroyed: MemoryLayout<UInt64>.size
                case .moved, .resized, .reordered, .levelChanged, .shown, .hidden: 0
                case .activeSpaceChanged, .appUnresponsive, .appResponsive: nil
            }
        return offset.flatMap { offset in payload.count >= offset + MemoryLayout<CGWindowID>.size ? unsafe payload.loadUnaligned(fromByteOffset: offset, as: CGWindowID.self) : nil }
    }

    func process(in payload: UnsafeRawBufferPointer) -> pid_t? {
        let offset: Int = MemoryLayout<(minorVersion: UInt16, majorVersion: UInt16, length: UInt32, connection: Int32)>.size
        let addressed: Bool = [.appUnresponsive, .appResponsive].contains(self) && payload.count >= offset + MemoryLayout<pid_t>.size
        return addressed ? unsafe payload.loadUnaligned(fromByteOffset: offset, as: pid_t.self) : nil
    }
}

extension ProcessSerialNumber {
    fileprivate var number: UInt64 { UInt64(highLongOfPSN) << 32 | UInt64(lowLongOfPSN) }
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
            let refused: CGError? = removed.values.map { (serial: ProcessSerialNumber) -> CGError in
                var removing = serial
                return unsafe removeFromPermittedFrontList(&removing)
            }
            .first { answer in answer != .success }
            guard let refused else { return }
            throw ApplicationFailure(subject: "\(removed.count) permitted front entries of excluded processes", reason: .windowServer(refused))
        }
    }

    package static let shared = PermittedFrontList()
    private static let path: String = URL.temporaryDirectory.appending(component: String(reflecting: PermittedFrontList.self)).path(percentEncoded: false)
    private let held: Mutex<Held?> = Mutex(nil)

    private init() {}

    package static var entries: Int { (metaInformation(-2, permittedFrontKey) as? [AnyObject])?.count ?? 0 }

    package static func recover() throws(ApplicationFailure) {
        guard shared.held.withLock({ (held: inout Held?) -> Bool in held == nil }) else { return }
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
        let id = UUID()
        let watched: FileHandle = try Self.open([.eventOnly])
        let (unlocks, unlocked) = AsyncStream<Void>.makeStream(bufferingPolicy: .bufferingNewest(1))
        let source = DispatchSource.makeFileSystemObjectSource(fileDescriptor: watched.fileDescriptor, eventMask: .funlock)
        source.setRegistrationHandler { unlocked.yield() }
        source.setEventHandler { unlocked.yield() }
        source.setCancelHandler { withExtendedLifetime(watched) {} }
        unlocked.onTermination = { _ in source.cancel() }
        source.activate()
        var registered = false
        for await _ in merge(unlocks.map { _ in true }, AsyncTimerSequence(interval: messagingTimeout * 2, clock: clock).map { _ in false }).prefix(while: \.self) {
            registered = try held.withLock { (held: inout Held?) throws(ApplicationFailure) -> Bool in
                if held == nil, let journal = try Self.lock() { held = Held(journal: journal) }
                guard held != nil else { return false }
                held?.holds[id] = (target, [])
                do throws(ApplicationFailure) { try held?.reconcile() } catch {
                    _ = release(id, from: &held)
                    throw error
                }
                return true
            }
            if registered { break }
        }
        guard registered else { throw ApplicationFailure(subject: Self.path, reason: Task.isCancelled ? .cancelled : .system(.wouldBlock)) }
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
        let listed: Int = entries
        guard listed == 0 else { throw ApplicationFailure(subject: "\(listed) permitted front entries another process holds", reason: .permittedFrontHeld) }
        return journal
    }

    private static func open(_ options: FileDescriptor.OpenOptions) throws(ApplicationFailure) -> FileHandle {
        do {
            let access: FileDescriptor.AccessMode = options.contains(.eventOnly) ? .readOnly : .readWrite
            let descriptor = try FileDescriptor.open(FilePath(path), access, options: options.union([.create, .closeOnExec]), permissions: .ownerReadWrite)
            return FileHandle(fileDescriptor: descriptor.rawValue, closeOnDealloc: true)
        } catch {
            throw ApplicationFailure(subject: path, reason: (error as? Errno).map(ApplicationFailure.Reason.system) ?? .foundation(error as NSError))
        }
    }

    private static func add(_ serials: [ProcessSerialNumber], to journal: FileHandle) throws(ApplicationFailure) {
        try append(serials, to: journal)
        for var serial in serials {
            let answer: CGError = unsafe addToPermittedFrontList(&serial)
            guard answer == .success else { throw ApplicationFailure(subject: "permitted front entry of process \(String(describing: processOfSerial(serial)))", reason: .windowServer(answer)) }
        }
    }

    private static func append(_ serials: [ProcessSerialNumber], to journal: FileHandle) throws(ApplicationFailure) {
        let words: [UInt32] = serials.flatMap { serial in [serial.highLongOfPSN, serial.lowLongOfPSN] }
        do {
            try journal.seekToEnd()
            try words.withUnsafeBytes { bytes in try unsafe journal.write(contentsOf: bytes) }
        } catch {
            throw ApplicationFailure(subject: path, reason: .foundation(error as NSError))
        }
    }

    private static func clear(_ journal: FileHandle) throws(ApplicationFailure) {
        let journaled: Data
        do {
            try journal.seek(toOffset: 0)
            journaled = try journal.readToEnd() ?? Data()
        } catch {
            throw ApplicationFailure(subject: path, reason: .foundation(error as NSError))
        }
        var front = ProcessSerialNumber()
        let fronted: Bool = unsafe frontSerial(&front) == .success
        let serials: OrderedDictionary<UInt64, ProcessSerialNumber> = OrderedDictionary(
            stride(from: 0, to: journaled.count - MemoryLayout<ProcessSerialNumber>.stride + 1, by: MemoryLayout<ProcessSerialNumber>.stride).reversed().map {
                (offset: Int) -> (UInt64, ProcessSerialNumber) in
                let serial = ProcessSerialNumber(
                    highLongOfPSN: journaled.bytes.load(fromByteOffset: offset, as: UInt32.self),
                    lowLongOfPSN: journaled.bytes.load(fromByteOffset: offset + MemoryLayout<UInt32>.stride, as: UInt32.self),
                )
                return (serial.number, serial)
            }
        ) { first, _ in first }
        let last: UInt64? = fronted ? front.number : nil
        let ordered: [ProcessSerialNumber] = serials.filter { number, _ in number != last }.values.elements + serials.filter { number, _ in number == last }.values.elements
        let kept: [(serial: ProcessSerialNumber, answer: CGError)] = ordered.map { (serial: ProcessSerialNumber) -> (serial: ProcessSerialNumber, answer: CGError) in
            var removing = serial
            return (serial, unsafe removeFromPermittedFrontList(&removing))
        }
        .filter { entry in entry.answer != .success }
        do { try journal.truncate(atOffset: 0) } catch { throw ApplicationFailure(subject: path, reason: .foundation(error as NSError)) }
        try append(kept.map(\.serial), to: journal)
        guard let refused = kept.first?.answer else { return }
        throw ApplicationFailure(subject: "\(kept.count) permitted front entries journaled in \(path)", reason: .windowServer(refused))
    }
}

final class WindowServerConnection: Sendable {
    struct Notice: Sendable {
        let notification: WindowNotification
        let window: CGWindowID?
        let owner: pid_t?
    }

    private let id: Int32
    private let source: Mutex<RunLoopSource?> = Mutex(nil)
    private let held:
        Mutex<(owners: [CGWindowID: pid_t], subscribers: [UUID: (notifications: Set<WindowNotification>, owner: pid_t?, continuation: AsyncStream<Result<Notice, ApplicationFailure>>.Continuation)])>

    init() throws(ApplicationFailure) {
        var (id, events): (Int32, mach_port_t) = (0, 0)
        let opened: CGError = unsafe newConnection(0, &id)
        guard opened == .success else { throw ApplicationFailure(subject: "window server connection", reason: .windowServer(opened)) }
        let rows: [[String: Any]] = CGWindowListCopyWindowInfo(.optionAll, kCGNullWindowID) as? [[String: Any]] ?? []
        let owners: [CGWindowID: pid_t] = Dictionary(
            rows.compactMap { row in (row[kCGWindowNumber as String] as? CGWindowID).flatMap { number in (row[kCGWindowOwnerPID as String] as? pid_t).map { owner in (number, owner) } } }
        ) { first, _ in first }
        self.id = id
        held = Mutex((owners, [:]))
        let callback: @convention(c) (UInt32, UnsafeMutableRawPointer?, Int, UnsafeMutableRawPointer?) -> Void = { type, data, length, context in
            guard let notification = WindowNotification(rawValue: type),
                let connection = unsafe context.map({ context in unsafe Unmanaged<WindowServerConnection>.fromOpaque(context).takeUnretainedValue() })
            else { return }
            let payload: UnsafeRawBufferPointer? = unsafe data.map { data in unsafe UnsafeRawBufferPointer(start: data, count: length) }
            connection.deliver(
                notification,
                window: unsafe payload.flatMap { payload in unsafe notification.window(in: payload) },
                process: unsafe payload.flatMap { payload in unsafe notification.process(in: payload) },
            )
        }
        let context = unsafe Unmanaged.passUnretained(self).toOpaque()
        let answers: [CGError] =
            unsafe [eventPort(id, &events)] + WindowNotification.allCases.map { notification in unsafe registerNotifyProc(id, callback, notification.rawValue, context) }
            + [request(Array(owners.keys)), setUnresponsiveNotificationTimeout(id, messagingTimeout / .seconds(1))]
        let refused: CGError? = answers.first { answer in answer != .success }
        let drained: Bool = source.withLock { source in
            var described = unsafe CFMachPortContext(version: 0, info: UnsafeMutableRawPointer(bitPattern: Int(id)), retain: nil, release: nil, copyDescription: nil)
            let drain: CFMachPortCallBack = { _, _, _, info in
                let connection = Int32(Int(bitPattern: info))
                while nextEvent(connection) != nil {}
            }
            source = unsafe CFMachPortCreateWithPort(nil, events, drain, &described, nil).flatMap { port in CFMachPortCreateRunLoopSource(nil, port, 0) }.map(RunLoopSource.init)
            return source != nil
        }
        guard refused == nil, drained else { throw ApplicationFailure(subject: "window server notifications of connection \(id)", reason: .windowServer(refused ?? .failure)) }
    }

    deinit {
        _ = releaseConnection(id)
        let ending: [AsyncStream<Result<Notice, ApplicationFailure>>.Continuation] = held.withLock { held in held.subscribers.values.map(\.continuation) }
        for subscriber in ending { subscriber.finish() }
    }

    func notices(of notifications: Set<WindowNotification>, from owner: pid_t? = nil) -> AsyncStream<Result<Notice, ApplicationFailure>> {
        let (stream, continuation) = AsyncStream<Result<Notice, ApplicationFailure>>.makeStream()
        let id = UUID()
        continuation.onTermination = { _ in self.held.withLock { held in held.subscribers[id] = nil } }
        held.withLock { held in held.subscribers[id] = (notifications, owner, continuation) }
        return stream
    }

    private func deliver(_ notification: WindowNotification, window: CGWindowID?, process addressed: pid_t?) {
        held.withLock { held in
            var (connection, process): (Int32, pid_t) = (0, 0)
            let known: pid_t? = window.flatMap { window in held.owners[window] }
            let unread: CGWindowID? = known == nil && notification != .destroyed ? window : nil
            let read: pid_t? = unread.flatMap { window in unsafe windowOwner(id, window, &connection) == .success && processOfConnection(connection, &process) == .success ? process : nil }
            if let window, let read { held.owners[window] = read }
            let requested: CGError = read == nil ? .success : request(Array(held.owners.keys))
            let owner: pid_t? = addressed ?? known ?? read
            let responsible: pid_t? = owner.map(responsiblePID)
            let notice: Result<Notice, ApplicationFailure> =
                requested == .success
                ? .success(Notice(notification: notification, window: window, owner: owner))
                : .failure(ApplicationFailure(subject: "notifications of \(held.owners.count) windows on connection \(id)", reason: .windowServer(requested)))
            let broadcast: Bool = window == nil && addressed == nil
            let reached = held.subscribers.values.filter { subscriber in
                requested != .success || subscriber.notifications.contains(notification) && (broadcast || subscriber.owner.map { process in process == owner || process == responsible } != false)
            }
            for subscriber in reached { subscriber.continuation.yield(notice) }
            if notification == .destroyed, let window { held.owners[window] = nil }
        }
    }

    private func request(_ windows: [CGWindowID]) -> CGError { unsafe requestNotifications(id, windows, Int32(windows.count)) }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

func pendingEventAge(of serial: ProcessSerialNumber) throws(ApplicationFailure) -> Duration? {
    var (serial, seconds, flags): (ProcessSerialNumber, Double, UInt32) = (serial, 0, 0)
    let answer: CGError = unsafe unresponsiveStatus(mainConnection(), &serial, &seconds, &flags)
    guard answer == .success else { throw ApplicationFailure(subject: "unresponsive status of process \(String(describing: processOfSerial(serial)))", reason: .windowServer(answer)) }
    return flags == 0 ? nil : .seconds(seconds)
}

package func focusedProcesses() -> (front: pid_t?, keyFocus: pid_t?) {
    let focused = focusedSerials()
    let process = { (read: Result<ProcessSerialNumber?, ApplicationFailure>) -> pid_t? in
        switch read {
            case .success(let serial): serial.flatMap(processOfSerial)
            case .failure: nil
        }
    }
    return (process(focused.front), process(focused.keyFocus))
}

private func focusedSerials() -> (front: Result<ProcessSerialNumber?, ApplicationFailure>, keyFocus: Result<ProcessSerialNumber?, ApplicationFailure>) {
    var (front, key, flag): (ProcessSerialNumber, ProcessSerialNumber, UInt8) = (ProcessSerialNumber(), ProcessSerialNumber(), 0)
    let read: (front: CGError, key: CGError) = unsafe (frontSerial(&front), keyFocusSerial(&key, &flag))
    let held = { (answer: CGError, serial: ProcessSerialNumber, role: String) -> Result<ProcessSerialNumber?, ApplicationFailure> in
        switch (answer, answer.rawValue) {
            case (.success, _): .success(serial)
            case (_, Int32(procNotFound)): .success(nil)
            case (_, _): .failure(ApplicationFailure(subject: "\(role) process of the permitted front list", reason: .windowServer(answer)))
        }
    }
    return (held(read.front, front, "front"), held(read.key, key, "key-focus"))
}

private func processOfSerial(_ serial: ProcessSerialNumber) -> pid_t? {
    var (serial, connection, process): (ProcessSerialNumber, Int32, pid_t) = (serial, 0, 0)
    guard unsafe connectionOfSerial(mainConnection(), &serial, &connection) == .success, unsafe processOfConnection(connection, &process) == .success else { return nil }
    return process
}
