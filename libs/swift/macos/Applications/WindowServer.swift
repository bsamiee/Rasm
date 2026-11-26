package import AppKit
package import Darwin
import Dispatch
import Foundation
import OrderedCollections
import Results
import Synchronization
import System

// --- [TYPES] ---------------------------------------------------------------------------

enum WindowNotification: UInt32, CaseIterable, Sendable {
    case reordered = 808
    case shown = 815
    case created = 1325

    var windowOffset: Int {
        switch self {
            case .reordered, .shown: 0
            case .created: MemoryLayout<UInt64>.stride
        }
    }
}

extension ProcessSerialNumber {
    fileprivate var number: UInt64 { UInt64(highLongOfPSN) << 32 | UInt64(lowLongOfPSN) }
}

// --- [SERVICES] ------------------------------------------------------------------------

package final class PermittedFrontList: Sendable {
    private struct Held {
        let journal: FileHandle
        var excluded: [UUID: @Sendable (NSRunningApplication) -> Bool] = [:]
        var admitted: Set<pid_t> = []
        var listed: OrderedDictionary<UInt64, ProcessSerialNumber> = [:]
        var joined: Result<Void, ApplicationFailure> = .success(())

        func permits(_ application: NSRunningApplication) -> Bool {
            admitted.contains(application.processIdentifier) || !excluded.values.contains { excluding in excluding(application) }
        }

        mutating func reconcile() throws(ApplicationFailure) {
            var (front, key, flag): (ProcessSerialNumber, ProcessSerialNumber, UInt8) = (ProcessSerialNumber(), ProcessSerialNumber(), 0)
            let read: (front: CGError, key: CGError) = unsafe (frontSerial(&front), keyFocusSerial(&key, &flag))
            guard read.front == .success, read.key == .success else {
                throw ApplicationFailure(subject: "front and key-focus processes of the permitted front list", reason: .windowServer(read.front == .success ? read.key : read.front))
            }
            let running: [ProcessSerialNumber] = NSWorkspace.shared.runningApplications.filter { application in application.activationPolicy != .prohibited && permits(application) }
                .compactMap { (application: NSRunningApplication) -> ProcessSerialNumber? in
                    var serial = ProcessSerialNumber()
                    return unsafe findProcess(application.processIdentifier, &serial) ? serial : nil
                }
            let wanted: OrderedDictionary<UInt64, ProcessSerialNumber> = OrderedDictionary(([front, key] + running).map { serial in (serial.number, serial) }) { first, _ in first }
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
    private let held: Mutex<Held?> = Mutex(nil)
    private let launches: Mutex<any NSObjectProtocol>

    deinit {
        launches.withLock { launches in NSWorkspace.shared.notificationCenter.removeObserver(launches) }
    }

    private init() {
        launches = Mutex(
            NSWorkspace.shared.notificationCenter.addObserver(forName: NSWorkspace.willLaunchApplicationNotification, object: nil, queue: nil) { notification in
                let high = notification.userInfo?["NSApplicationProcessSerialNumberHigh"] as? UInt32
                let low = notification.userInfo?["NSApplicationProcessSerialNumberLow"] as? UInt32
                guard let application = notification.userInfo?[NSWorkspace.applicationUserInfoKey] as? NSRunningApplication, let high, let low else { return }
                let serial = ProcessSerialNumber(highLongOfPSN: high, lowLongOfPSN: low)
                PermittedFrontList.shared.held.withLock { (held: inout Held?) in
                    guard let joining = held, joining.permits(application), joining.listed[serial.number] == nil else { return }
                    let added: Result<Void, ApplicationFailure> = Result { () throws(ApplicationFailure) in try Self.add([serial], to: joining.journal) }
                    held?.listed[serial.number] = serial
                    held?.joined = joining.joined.flatMap { _ in added }
                }
            }
        )
    }

    private static var path: String { URL.temporaryDirectory.appending(component: String(reflecting: PermittedFrontList.self)).path(percentEncoded: false) }

    package static var entries: Int { unsafe (metaInformation(-2, Unmanaged<CFString>.fromOpaque(permittedFrontKey).takeUnretainedValue()) as? [AnyObject])?.count ?? 0 }

    package static func recover() throws(ApplicationFailure) {
        do throws(ApplicationFailure) { try clear(try open([.exclusiveLock, .nonBlocking])) } catch  where error.reason == .system(.wouldBlock) {}
    }

    package func holding<Value, Failure: Error>(
        excluding process: pid_t,
        clock: some Clock<Duration> = ContinuousClock(),
        _ body: () async throws(Failure) -> Value,
    ) async throws(ApplicationFailure) -> Result<Value, Failure> {
        let id: UUID? = try await hold(excluding: { application in application.processIdentifier == process }, clock: clock)
        let value: Result<Value, Failure> = await Result { () async throws(Failure) -> Value in try await body() }
        try release(id).get()
        return value
    }

    func hold(excluding target: @escaping @Sendable (NSRunningApplication) -> Bool, clock: some Clock<Duration>) async throws(ApplicationFailure) -> UUID? {
        let (front, keyFocus): (pid_t?, pid_t?) = focusedProcesses()
        let holders: [NSRunningApplication] = [front, keyFocus].compactMap { (process: pid_t?) -> NSRunningApplication? in process.flatMap(NSRunningApplication.init(processIdentifier:)) }
        guard !holders.contains(where: target) else { return nil }
        let id = UUID()
        let watched: FileHandle = try Self.open([.eventOnly])
        let (unlocks, unlocked) = AsyncStream<Void>.makeStream(bufferingPolicy: .bufferingNewest(1))
        let source = DispatchSource.makeFileSystemObjectSource(fileDescriptor: watched.fileDescriptor, eventMask: .funlock)
        source.setRegistrationHandler { unlocked.yield() }
        source.setEventHandler { unlocked.yield() }
        source.setCancelHandler { withExtendedLifetime(watched) {} }
        unlocked.onTermination = { _ in source.cancel() }
        source.activate()
        return try await withTimeout(messagingTimeout * 2, expired: ApplicationFailure(subject: Self.path, reason: .system(.wouldBlock)), clock: clock) {
            () async throws(ApplicationFailure) -> UUID? in
            for await _ in unlocks {
                let registered: Bool = try self.held.withLock { (held: inout Held?) throws(ApplicationFailure) -> Bool in
                    if held == nil, let journal = try Self.lock() { held = Held(journal: journal) }
                    guard held != nil else { return false }
                    held?.excluded[id] = target
                    do throws(ApplicationFailure) { try held?.reconcile() } catch {
                        _ = self.release(id, from: &held)
                        throw error
                    }
                    return true
                }
                if registered { return id }
            }
            throw ApplicationFailure(subject: Self.path, reason: .cancelled)
        }
    }

    @discardableResult
    func release(_ hold: UUID?) -> Result<Void, ApplicationFailure> {
        held.withLock { (held: inout Held?) -> Result<Void, ApplicationFailure> in release(hold, from: &held) }
    }

    func admit(_ process: pid_t) -> Result<Void, ApplicationFailure> {
        held.withLock { (held: inout Held?) -> Result<Void, ApplicationFailure> in
            guard var admitting = held else { return .success(()) }
            admitting.admitted.insert(process)
            let reconciled: Result<Void, ApplicationFailure> = Result { () throws(ApplicationFailure) in try admitting.reconcile() }
            held = admitting
            return reconciled
        }
    }

    private func release(_ hold: UUID?, from held: inout Held?) -> Result<Void, ApplicationFailure> {
        guard let hold, var releasing = held, releasing.excluded.removeValue(forKey: hold) != nil else { return .success(()) }
        guard releasing.excluded.isEmpty else {
            let reconciled: Result<Void, ApplicationFailure> = Result { () throws(ApplicationFailure) in try releasing.reconcile() }
            held = releasing
            return reconciled
        }
        held = nil
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

struct WindowServerConnection: ~Copyable {
    private let id: Int32
    private let port: CFMachPort
    private let source: RunLoopSource

    init(
        notifying notifications: [WindowNotification],
        context: UnsafeMutableRawPointer,
        _ callback: @escaping @convention(c) (UInt32, UnsafeMutableRawPointer?, Int, UnsafeMutableRawPointer?) -> Void,
    ) throws(ApplicationFailure) {
        var (id, events): (Int32, mach_port_t) = (0, 0)
        let opened: CGError = unsafe newConnection(0, &id)
        guard opened == .success else { throw ApplicationFailure(subject: "window server connection", reason: .windowServer(opened)) }
        let answers: [CGError] = unsafe [eventPort(id, &events)] + notifications.map { notification in unsafe registerNotifyProc(id, callback, notification.rawValue, context) }
        var described = unsafe CFMachPortContext(version: 0, info: UnsafeMutableRawPointer(bitPattern: Int(id)), retain: nil, release: nil, copyDescription: nil)
        let drain: CFMachPortCallBack = { _, _, _, info in
            let connection = Int32(Int(bitPattern: info))
            while let event = unsafe nextEvent(connection) { unsafe event.release() }
        }
        let refused: CGError? = answers.first { answer in answer != .success }
        guard refused == nil, let port = unsafe CFMachPortCreateWithPort(nil, events, drain, &described, nil), let source = CFMachPortCreateRunLoopSource(nil, port, 0) else {
            _ = releaseConnection(id)
            throw ApplicationFailure(subject: "window server notifications of connection \(id)", reason: .windowServer(refused ?? .failure))
        }
        self.id = id
        self.port = port
        self.source = RunLoopSource(source)
    }

    deinit { _ = releaseConnection(id) }

    func request(_ windows: [CGWindowID]) throws(ApplicationFailure) {
        let answer: CGError = unsafe requestNotifications(id, windows, Int32(windows.count))
        guard answer == .success else { throw ApplicationFailure(subject: "notifications of \(windows.count) windows on connection \(id)", reason: .windowServer(answer)) }
    }

    func owner(of window: CGWindowID) -> pid_t? {
        var (connection, process): (Int32, pid_t) = (0, 0)
        guard unsafe windowOwner(id, window, &connection) == .success, unsafe processOfConnection(connection, &process) == .success else { return nil }
        return process
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

@_silgen_name("CGSMainConnectionID")
package func mainConnection() -> Int32

package func focusedProcesses() -> (front: pid_t?, keyFocus: pid_t?) {
    var (front, key, flag): (ProcessSerialNumber, ProcessSerialNumber, UInt8) = (ProcessSerialNumber(), ProcessSerialNumber(), 0)
    let read: (front: CGError, key: CGError) = unsafe (frontSerial(&front), keyFocusSerial(&key, &flag))
    return (read.front == .success ? processOfSerial(front) : nil, read.key == .success ? processOfSerial(key) : nil)
}

private func processOfSerial(_ serial: ProcessSerialNumber) -> pid_t? {
    var (serial, connection, process): (ProcessSerialNumber, Int32, pid_t) = (serial, 0, 0)
    guard unsafe connectionOfSerial(mainConnection(), &serial, &connection) == .success, unsafe processOfConnection(connection, &process) == .success else { return nil }
    return process
}

@_silgen_name("CPSFindProcessByPID")
func findProcess(_: pid_t, _: UnsafeMutablePointer<ProcessSerialNumber>) -> Bool

@_silgen_name("CPSGetKeyFocusProcess")
private func keyFocusSerial(_: UnsafeMutablePointer<ProcessSerialNumber>, _: UnsafeMutablePointer<UInt8>) -> CGError

@_silgen_name("CPSGetFrontProcess")
private func frontSerial(_: UnsafeMutablePointer<ProcessSerialNumber>) -> CGError

@_silgen_name("CGSGetConnectionIDForPSN")
private func connectionOfSerial(_: Int32, _: UnsafePointer<ProcessSerialNumber>, _: UnsafeMutablePointer<Int32>) -> CGError

@_silgen_name("CGSConnectionGetPID")
private func processOfConnection(_: Int32, _: UnsafeMutablePointer<pid_t>) -> CGError

@_silgen_name("SLPSAddToPermittedFrontPSNList")
private func addToPermittedFrontList(_: UnsafePointer<ProcessSerialNumber>) -> CGError

@_silgen_name("SLPSRemoveFromPermittedFrontPSNList")
private func removeFromPermittedFrontList(_: UnsafePointer<ProcessSerialNumber>) -> CGError

@_silgen_name("_LSCopyMetaApplicationInformationItem")
private func metaInformation(_ session: Int32, _: CFString) -> CFTypeRef?

@_silgen_name("SLSNewConnection")
private func newConnection(_: Int32, _: UnsafeMutablePointer<Int32>) -> CGError

@_silgen_name("SLSReleaseConnection")
private func releaseConnection(_: Int32) -> CGError

@_silgen_name("SLSGetEventPort")
private func eventPort(_: Int32, _: UnsafeMutablePointer<mach_port_t>) -> CGError

@_silgen_name("SLEventCreateNextEvent")
private func nextEvent(_: Int32) -> Unmanaged<CGEvent>?

@_silgen_name("SLSRegisterConnectionNotifyProc")
private func registerNotifyProc(_: Int32, _: @convention(c) (UInt32, UnsafeMutableRawPointer?, Int, UnsafeMutableRawPointer?) -> Void, _: UInt32, _: UnsafeMutableRawPointer?) -> CGError

@_silgen_name("SLSRequestNotificationsForWindows")
private func requestNotifications(_: Int32, _: UnsafePointer<CGWindowID>, _: Int32) -> CGError

@_silgen_name("SLSGetWindowOwner")
private func windowOwner(_: Int32, _: CGWindowID, _: UnsafeMutablePointer<Int32>) -> CGError

@_extern(c, "_kLSPermittedFrontASNsArrayKey")
private nonisolated(unsafe) var permittedFrontKey: UnsafeRawPointer
