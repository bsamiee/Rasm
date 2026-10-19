import AppKit
import RasmMacOS
import System

// --- [MODELS] --------------------------------------------------------------------------

enum Failure: Codable {
    case recorded(String)
    case unnamed
    case unknownFlag(String)
    case answerForm
    case notInstalled
    case unanswered
    case element(ElementFailure)
    case discovery(DiscoveryFailure)
    case log(any Error)
    case crashReports(any Error)

    init(from decoder: any Decoder) throws {
        self = try .recorded(decoder.singleValueContainer().decode(String.self))
    }

    func encode(to encoder: any Encoder) throws {
        var reason: any SingleValueEncodingContainer = encoder.singleValueContainer()
        let text: String =
            switch self {
                case .recorded(let text): text
                case .unnamed: "No application named"
                case .unknownFlag(let flag): "Unknown flag \(flag)"
                case .answerForm: "--answer takes '<text>=<action>'"
                case .notInstalled: "No installed application has this bundle id or name"
                case .unanswered: "No instance answered"
                case .element(let error as any Error), .discovery(let error as any Error): error.localizedDescription
                case .log(let error): "The events log failed: \(error.localizedDescription)"
                case .crashReports(let error): "Crash reports are unreadable: \(error.localizedDescription)"
            }
        try reason.encode(text)
    }
}

struct Event: Codable {
    enum Kind: String, Codable {
        case watch
        case launch
        case exit
        case open
        case closed
        case answered
        case fail
        case crash
    }

    let app: String?
    let event: Kind
    let pid: pid_t?
    let element: Element?
    let status: Int?
    let action: ElementAction?
    let report: String?
    let reason: Failure?

    init(app: String?, event: Kind, pid: pid_t? = nil, element: Element? = nil, status: Int? = nil, action: ElementAction? = nil, report: String? = nil, reason: Failure? = nil) {
        self.app = app
        self.event = event
        self.pid = pid
        self.element = element
        self.status = status
        self.action = action
        self.report = report
        self.reason = reason
    }
}

enum Sink {
    case output
    case log(FileHandle)
}

struct Shown: Hashable {
    let pid: pid_t
    let element: Element
}

// --- [SERVICES] ------------------------------------------------------------------------

@MainActor
final class Watch {
    // --- [STATE]
    let app: String
    let log: FileHandle
    var visible: Set<Shown> = []
    var attached: [pid_t: Task<Void, Never>] = [:]

    init(app: String) throws {
        self.app = app
        log = try logHandle(app, .writeOnly)
    }

    // --- [FOLLOW]
    func run() async {
        show(Event(app: app, event: .watch))
        for instance: NSRunningApplication in NSRunningApplication.runningApplications(withBundleIdentifier: app) {
            let pid: pid_t = instance.processIdentifier
            let opened: [Event] =
                switch instance.status() {
                    case .success(let status): status.elements.map { element in Event(app: app, event: .open, pid: pid, element: element) }
                    case .failure(let error): [Event(app: app, event: .fail, pid: pid, reason: .element(error))]
                }
            for event: Event in opened { show(event) }
        }
        switch Result(catching: { try logHandle(app, .readOnly) }) {
            case .success(let reader):
                await withDiscardingTaskGroup { group in
                    group.addTask { await self.follow(reader) }
                    group.addTask { await self.take() }
                }
            case .failure(let error): show(Event(app: app, event: .fail, reason: .log(error)))
        }
    }

    func take() async {
        switch await locked(log) {
            case .some(let failure): show(Event(app: app, event: .fail, reason: .log(failure)))
            case .none: await hold()
        }
    }

    func follow(_ reader: FileHandle) async {
        var buffer: Data = Data()
        let followed: Result<Void, any Error> = await Result {
            for try await _ in FileWatch.events(of: logURL(app)) {
                buffer.append(reader.availableData)
                let lines: [Data] = buffer.split(separator: UInt8(ascii: "\n"), omittingEmptySubsequences: false)
                buffer = lines.last ?? Data()
                for line: Data in lines.dropLast() {
                    switch Result(catching: { try JSONDecoder().decode(Event.self, from: line) }) {
                        case .success(let event): show(event)
                        case .failure(let error): show(Event(app: app, event: .fail, reason: .log(error)))
                    }
                }
            }
        }
        if case .failure(let error) = followed { show(Event(app: app, event: .fail, reason: .log(error))) }
    }

    func show(_ event: Event) {
        switch (event.event, event.pid, event.element) {
            case (.open, .some(let pid), .some(let element)): visible.insert(Shown(pid: pid, element: element))
            case (.closed, .some(let pid), .some(let element)): visible.remove(Shown(pid: pid, element: element))
            case (.exit, .some(let pid), _): visible = visible.filter { shown in shown.pid != pid }
            default: break
        }
        output.yield((event, .output))
    }

    // --- [HOLD]
    func hold() async {
        let running: [NSRunningApplication] = NSRunningApplication.runningApplications(withBundleIdentifier: app)
        let pids: Set<pid_t> = Set(running.map(\.processIdentifier))
        for shown: Shown in visible.filter({ shown in !pids.contains(shown.pid) }).sorted(by: order) { write(Event(app: app, event: .closed, pid: shown.pid, element: shown.element)) }
        for instance: NSRunningApplication in running { attach(instance, known: visible.filter { shown in shown.pid == instance.processIdentifier }) }
        for await change: InstanceChange in NSWorkspace.shared.instanceChanges(of: { [app] instance in instance.bundleIdentifier == app }) {
            switch change {
                case .launched(let instance):
                    write(Event(app: app, event: .launch, pid: instance.processIdentifier))
                    attach(instance, known: [])
                case .terminated(let instance, let status):
                    attached.removeValue(forKey: instance.processIdentifier)?.cancel()
                    write(Event(app: app, event: .exit, pid: instance.processIdentifier, status: status))
            }
        }
    }

    func attach(_ instance: NSRunningApplication, known: Set<Shown>) {
        let pid: pid_t = instance.processIdentifier
        attached[pid] = Task { @MainActor in
            switch instance.statusChanges() {
                case .failure(let error): write(Event(app: app, event: .fail, pid: pid, reason: .element(error)))
                case .success(let changes):
                    var previous: Set<Shown> = known
                    for await status: ApplicationStatus in changes {
                        let current: Set<Shown> = Set(status.elements.map { element in Shown(pid: pid, element: element) })
                        for shown: Shown in previous.subtracting(current).sorted(by: order) { write(Event(app: app, event: .closed, pid: pid, element: shown.element)) }
                        for shown: Shown in current.subtracting(previous).sorted(by: order) { write(Event(app: app, event: .open, pid: pid, element: shown.element)) }
                        previous = current
                    }
            }
        }
    }

    func write(_ event: Event) {
        output.yield((event, .log(log)))
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

func logURL(_ app: String) -> URL {
    URL.temporaryDirectory.appending(path: "\(app).events")
}

func logHandle(_ app: String, _ mode: FileDescriptor.AccessMode) throws -> FileHandle {
    let descriptor: FileDescriptor = try FileDescriptor.open(FilePath(logURL(app).path(percentEncoded: false)), mode, options: [.create, .append, .closeOnExec], permissions: [.ownerReadWrite])
    _ = try descriptor.seek(offset: 0, from: .end)
    return FileHandle(fileDescriptor: descriptor.rawValue, closeOnDealloc: true)
}

func order(_ left: Shown, _ right: Shown) -> Bool {
    (left.pid, left.element.id) < (right.pid, right.element.id)
}

@concurrent
func locked(_ log: FileHandle) async -> Errno? {
    var lock: flock = flock(l_start: 0, l_len: 0, l_pid: 0, l_type: Int16(F_WRLCK), l_whence: Int16(SEEK_SET))
    return unsafe fcntl(log.fileDescriptor, F_SETLKW, &lock) == 0 ? nil : Errno(rawValue: errno)
}

@MainActor
func answered(_ ids: [String], _ text: String, _ action: ElementAction) async -> Event {
    let (outcomes, finish): (AsyncStream<Event>, AsyncStream<Event>.Continuation) = AsyncStream.makeStream()
    let launched: AsyncStream<InstanceChange> = NSWorkspace.shared.instanceChanges { instance in instance.bundleIdentifier.map(ids.contains) == true }
    let racing: Task<Void, Never> = Task { @MainActor in
        await withDiscardingTaskGroup { group in
            func race(_ instance: NSRunningApplication) {
                group.addTask {
                    let app: String? = instance.bundleIdentifier
                    let outcome: Event =
                        switch await instance.answer(text, with: action) {
                            case .success: Event(app: app, event: .answered, pid: instance.processIdentifier, action: action)
                            case .failure(let error): Event(app: app, event: .fail, pid: instance.processIdentifier, action: action, reason: .element(error))
                        }
                    finish.yield(outcome)
                }
            }
            for instance: NSRunningApplication in ids.flatMap(NSRunningApplication.runningApplications(withBundleIdentifier:)) { race(instance) }
            for await case .launched(let instance) in launched { race(instance) }
        }
    }
    let outcome: Event = await outcomes.first { @Sendable _ in true } ?? Event(app: nil, event: .fail, action: action, reason: .unanswered)
    racing.cancel()
    return outcome
}

// --- [COMPOSITION] ---------------------------------------------------------------------

let (lines, output): (AsyncStream<(Event, Sink)>, AsyncStream<(Event, Sink)>.Continuation) = AsyncStream.makeStream(of: (Event, Sink).self)
let encoder: JSONEncoder = JSONEncoder()
encoder.outputFormatting = [.sortedKeys, .withoutEscapingSlashes]
let arguments: ArraySlice<String> = CommandLine.arguments.dropFirst()
let flag: Int? = arguments.firstIndex(of: "--answer")
let answer: (text: String, action: ElementAction)? = flag.flatMap { at in arguments[(at + 1)...].first }
    .flatMap { pair in pair.lastIndex(of: "=").map { at in (String(pair[..<at]), ElementAction(label: String(pair[pair.index(after: at)...]))) } }
let crashes: Bool = arguments.contains("--crashes")
let names: [String] = (flag.map { at in arguments[..<at] + arguments[(at + 1)...].dropFirst() } ?? arguments).filter { name in name != "--crashes" }
let apps: [(name: String, ids: Result<[String], DiscoveryFailure>)] = names.filter { name in !name.hasPrefix("--") }
    .map { name in
        (name, NSWorkspace.shared.installedApplications(matching: "kMDItemCFBundleIdentifier == '\(name)' || kMDItemDisplayName == '\(name)'", in: nil).map { found in found.map(\.identifier) })
    }
let failures: [Event] =
    (apps.isEmpty ? [Event(app: nil, event: .fail, reason: .unnamed)] : [])
    + names.filter { name in name.hasPrefix("--") }.map { flag in Event(app: nil, event: .fail, reason: .unknownFlag(flag)) }
    + (flag != nil && answer == nil ? [Event(app: nil, event: .fail, reason: .answerForm)] : [])
    + apps.compactMap { app in
        switch app.ids {
            case .success(let ids) where ids.isEmpty: Event(app: app.name, event: .fail, reason: .notInstalled)
            case .success: nil
            case .failure(let error): Event(app: app.name, event: .fail, reason: .discovery(error))
        }
    }
let ids: [String] = failures.isEmpty ? Set(apps.compactMap { app in if case .success(let ids) = app.ids { ids } else { nil } }.joined()).sorted() : []
let parent: any DispatchSourceProcess = DispatchSource.makeProcessSource(identifier: getppid(), eventMask: .exit, queue: .main)
parent.setEventHandler { output.finish() }
parent.activate()
for event: Event in failures { output.yield((event, .output)) }
let work: Task<Void, Never> = Task { @MainActor in
    switch (failures.isEmpty, answer, crashes) {
        case (false, _, _): break
        case (true, .some(let answer), _):
            let outcome: Event = await answered(ids, answer.text, answer.action)
            if let app: String = outcome.app, case .success(let log) = Result(catching: { try logHandle(app, .writeOnly) }) { output.yield((outcome, .log(log))) }
            output.yield((outcome, .output))
        case (true, .none, true):
            let reports: [Event] = ids.flatMap { id in
                switch crashReports(for: id) {
                    case .success(let reports): reports.map { report in Event(app: id, event: .crash, report: report.path(percentEncoded: false)) }
                    case .failure(let error): [Event(app: id, event: .fail, reason: .crashReports(error))]
                }
            }
            for event: Event in reports { output.yield((event, .output)) }
        case (true, .none, false):
            await withDiscardingTaskGroup { group in
                for id: String in ids {
                    switch Result(catching: { try Watch(app: id) }) {
                        case .success(let watch): group.addTask { await watch.run() }
                        case .failure(let error): output.yield((Event(app: id, event: .fail, reason: .log(error)), .output))
                    }
                }
            }
    }
    output.finish()
}
var last: Event.Kind?
for await (event, sink): (Event, Sink) in lines {
    let line: Data = try encoder.encode(event) + [UInt8(ascii: "\n")]
    switch sink {
        case .output: try FileHandle.standardOutput.write(contentsOf: line)
        case .log(let log): try log.write(contentsOf: line)
    }
    last = event.event
}
exit(failures.isEmpty && (answer == nil || last == .answered) ? EXIT_SUCCESS : EXIT_FAILURE)
