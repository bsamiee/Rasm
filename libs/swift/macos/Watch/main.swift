import AppKit
import ArgumentParser
import RasmMacOS
import System

// --- [MODELS] --------------------------------------------------------------------------

struct WatchApps: ParsableCommand {
    @Argument(help: "Bundle identifiers or display names of the apps to watch")
    var apps: [String]

    @Flag(help: "Print the crash reports of the apps and exit")
    var crashes: Bool = false

    @Option(help: "Text in the title or text of the element to answer")
    var answer: String?

    @Option(help: "Action to perform on the element --answer names")
    var action: String?

    var answering: (text: String, action: ElementAction)? {
        answer.flatMap { text in action.map { action in (text, ElementAction(label: action)) } }
    }

    func validate() throws(ValidationError) {
        guard (answer == nil) == (action == nil) else { throw ValidationError("--answer and --action go together") }
    }
}

struct Event: Encodable {
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

    var app: String?
    let event: Kind
    var pid: pid_t?
    var element: Element?
    var status: Int?
    var action: ElementAction?
    var report: String?
    var reason: Failure?
}

struct Followed: Decodable {
    let event: Event.Kind
    let pid: pid_t?
    let element: Element?
}

enum Line {
    case output(Event)
    case log(Event, FileHandle)
    case followed(Data)
}

struct Shown: Hashable, Comparable {
    let pid: pid_t
    let element: Element

    static func < (left: Self, right: Self) -> Bool {
        (left.pid, left.element.id) < (right.pid, right.element.id)
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

enum Failure: Encodable {
    case arguments(any Error)
    case notInstalled
    case unanswered
    case element(ElementFailure)
    case discovery(DiscoveryFailure)
    case log(any Error)
    case crashReports(any Error)

    func encode(to encoder: any Encoder) throws {
        var reason: any SingleValueEncodingContainer = encoder.singleValueContainer()
        let text: String =
            switch self {
                case .arguments(let error): WatchApps.fullMessage(for: error)
                case .notInstalled: "No installed application has this bundle id or name"
                case .unanswered: "No instance answered"
                case .element(let error as any Error), .discovery(let error as any Error): error.localizedDescription
                case .log(let error): "The events log failed: \(error.localizedDescription)"
                case .crashReports(let error): "Crash reports are unreadable: \(error.localizedDescription)"
            }
        try reason.encode(text)
    }
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
                    switch Result(catching: { try JSONDecoder().decode(Followed.self, from: line) }) {
                        case .success(let event):
                            track(event.event, event.pid, event.element)
                            output.yield(.followed(line))
                        case .failure(let error): show(Event(app: app, event: .fail, reason: .log(error)))
                    }
                }
            }
        }
        if case .failure(let error) = followed { show(Event(app: app, event: .fail, reason: .log(error))) }
    }

    func show(_ event: Event) {
        track(event.event, event.pid, event.element)
        output.yield(.output(event))
    }

    func track(_ kind: Event.Kind, _ pid: pid_t?, _ element: Element?) {
        switch (kind, pid, element) {
            case (.open, .some(let pid), .some(let element)): visible.insert(Shown(pid: pid, element: element))
            case (.closed, .some(let pid), .some(let element)): visible.remove(Shown(pid: pid, element: element))
            case (.exit, .some(let pid), _): visible = visible.filter { shown in shown.pid != pid }
            default: break
        }
    }

    // --- [HOLD]
    func hold() async {
        let running: [NSRunningApplication] = NSRunningApplication.runningApplications(withBundleIdentifier: app)
        let pids: Set<pid_t> = Set(running.map(\.processIdentifier))
        for shown: Shown in visible.filter({ shown in !pids.contains(shown.pid) }).sorted() { write(Event(app: app, event: .closed, pid: shown.pid, element: shown.element)) }
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
            var previous: Set<Shown> = known
            for await next: Result<ApplicationStatus, ElementFailure> in instance.statusChanges() {
                switch next {
                    case .success(let status):
                        let current: Set<Shown> = Set(status.elements.map { element in Shown(pid: pid, element: element) })
                        for shown: Shown in previous.subtracting(current).sorted() { write(Event(app: app, event: .closed, pid: pid, element: shown.element)) }
                        for shown: Shown in current.subtracting(previous).sorted() { write(Event(app: app, event: .open, pid: pid, element: shown.element)) }
                        previous = current
                    case .failure(let error): write(Event(app: app, event: .fail, pid: pid, reason: .element(error)))
                }
            }
        }
    }

    func write(_ event: Event) {
        output.yield(.log(event, log))
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

@concurrent
func locked(_ log: FileHandle) async -> Errno? {
    await withTaskExecutorPreference(DispatchQueue(label: "watch-apps.lock")) {
        var lock: flock = flock(l_start: 0, l_len: 0, l_pid: 0, l_type: Int16(F_WRLCK), l_whence: Int16(SEEK_SET))
        return unsafe fcntl(log.fileDescriptor, F_SETLKW, &lock) == 0 ? nil : Errno(rawValue: errno)
    }
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
    let outcome: Event = await outcomes.first { @Sendable _ in true } ?? Event(event: .fail, action: action, reason: .unanswered)
    racing.cancel()
    return outcome
}

// --- [COMPOSITION] ---------------------------------------------------------------------

let (lines, output): (AsyncStream<Line>, AsyncStream<Line>.Continuation) = AsyncStream.makeStream(of: Line.self)
let encoder: JSONEncoder = JSONEncoder()
encoder.outputFormatting = [.sortedKeys, .withoutEscapingSlashes]
let parsed: Result<WatchApps, any Error> = Result { try WatchApps.parse() }
if case .failure(let error) = parsed, WatchApps.exitCode(for: error) == .success { WatchApps.exit(withError: error) }
let resolved: Result<(command: WatchApps, apps: [(name: String, ids: Result<[String], DiscoveryFailure>)]), any Error> = parsed.map { command in
    (command, command.apps.map { name in (name, NSWorkspace.shared.installedApplications(matching: [.identifier(name), .name(name)], in: nil).map { found in found.map(\.identifier) }) })
}
let failures: [Event] =
    switch resolved {
        case .success(let resolved):
            resolved.apps.compactMap { app in
                switch app.ids {
                    case .success(let ids) where ids.isEmpty: Event(app: app.name, event: .fail, reason: .notInstalled)
                    case .success: nil
                    case .failure(let error): Event(app: app.name, event: .fail, reason: .discovery(error))
                }
            }
        case .failure(let error): [Event(event: .fail, reason: .arguments(error))]
    }
let parent: any DispatchSourceProcess = DispatchSource.makeProcessSource(identifier: getppid(), eventMask: .exit, queue: .main)
parent.setEventHandler { exit(EXIT_SUCCESS) }
parent.activate()
for event: Event in failures { output.yield(.output(event)) }
let work: Task<Int32, Never> = Task { @MainActor in
    defer { output.finish() }
    switch (resolved, failures.isEmpty) {
        case (.failure, _), (.success, false):
            return EXIT_FAILURE
        case (.success(let resolved), true):
            let ids: [String] = Set(resolved.apps.compactMap { app in if case .success(let ids) = app.ids { ids } else { nil } }.joined()).sorted()
            switch (resolved.command.answering, resolved.command.crashes) {
                case (.some(let answer), _):
                    let outcome: Event = await answered(ids, answer.text, answer.action)
                    if let app: String = outcome.app, case .success(let log) = Result(catching: { try logHandle(app, .writeOnly) }) { output.yield(.log(outcome, log)) }
                    output.yield(.output(outcome))
                    return outcome.event == .answered ? EXIT_SUCCESS : EXIT_FAILURE
                case (.none, true):
                    let reports: [Event] = ids.flatMap { id in
                        switch crashReports(for: id) {
                            case .success(let reports): reports.map { report in Event(app: id, event: .crash, report: report.path(percentEncoded: false)) }
                            case .failure(let error): [Event(app: id, event: .fail, reason: .crashReports(error))]
                        }
                    }
                    for event: Event in reports { output.yield(.output(event)) }
                    return EXIT_SUCCESS
                case (.none, false):
                    await withDiscardingTaskGroup { group in
                        for id: String in ids {
                            switch Result(catching: { try Watch(app: id) }) {
                                case .success(let watch): group.addTask { await watch.run() }
                                case .failure(let error): output.yield(.output(Event(app: id, event: .fail, reason: .log(error))))
                            }
                        }
                    }
                    return EXIT_SUCCESS
            }
    }
}
for await line: Line in lines {
    switch line {
        case .output(let event): try FileHandle.standardOutput.write(contentsOf: encoder.encode(event) + [UInt8(ascii: "\n")])
        case .log(let event, let log): try log.write(contentsOf: encoder.encode(event) + [UInt8(ascii: "\n")])
        case .followed(let line): try FileHandle.standardOutput.write(contentsOf: line + [UInt8(ascii: "\n")])
    }
}
exit(await work.value)
