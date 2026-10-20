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

    @Option(help: "Control label on the element --answer names to receive --value")
    var field: String?

    @Option(help: "Text the --field control receives")
    var value: String?

    var answering: (text: String, action: ElementAction)? {
        let chosen: ElementAction? =
            switch (action, field, value) {
                case (.some(let label), .none, .none): ElementAction(label: label)
                case (.none, .some(let field), .some(let value)): .set(field: field, value: value)
                default: nil
            }
        return answer.flatMap { text in chosen.map { action in (text, action) } }
    }

    func validate() throws(ValidationError) {
        guard [answer, action, field, value].allSatisfy({ option in option == nil }) || answering != nil else { throw ValidationError("--answer takes --action, or --field with --value") }
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

    init(app: String) throws {
        self.app = app
        log = try logHandle(app, .writeOnly)
    }

    // --- [FOLLOW]
    func run(in group: inout DiscardingTaskGroup) {
        show(Event(app: app, event: .watch))
        for instance: NSRunningApplication in NSRunningApplication.runningApplications(withBundleIdentifier: app) {
            let pid: pid_t = instance.processIdentifier
            let opened: [Event] =
                switch instance.status() {
                    case .success(.responding(_, let elements)): elements.map { element in Event(app: app, event: .open, pid: pid, element: element) }
                    case .success(.unresponsive): []
                    case .failure(let error): [Event(app: app, event: .fail, pid: pid, reason: .element(error))]
                }
            for event: Event in opened { show(event) }
        }
        switch Result(catching: { try logHandle(app, .readOnly) }) {
            case .success(let reader):
                group.addTask { await self.follow(reader) }
                group.addTask { await self.take() }
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
        for await next: Result<Void, FileWatchFailure> in FileWatch.events(of: logURL(app)) {
            if case .failure(let error) = next {
                show(Event(app: app, event: .fail, reason: .log(error)))
                return
            }
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
        let pids: Set<pid_t> = Set(NSRunningApplication.runningApplications(withBundleIdentifier: app).map(\.processIdentifier))
        for shown: Shown in visible.filter({ shown in !pids.contains(shown.pid) }).sorted() { write(Event(app: app, event: .closed, pid: shown.pid, element: shown.element)) }
        var previous: [pid_t: Set<Shown>] = Dictionary(grouping: visible.filter { shown in pids.contains(shown.pid) }, by: \.pid).mapValues(Set.init)
        for await event: InstanceEvent in NSWorkspace.shared.instanceEvents(of: { [app] instance in instance.bundleIdentifier == app }) {
            switch event {
                case .instance(.launched(let instance)): write(Event(app: app, event: .launch, pid: instance.processIdentifier))
                case .instance(.terminated(let instance, let status)):
                    previous[instance.processIdentifier] = nil
                    write(Event(app: app, event: .exit, pid: instance.processIdentifier, status: status))
                case .status(let instance, .success(.responding(_, let elements))):
                    let pid: pid_t = instance.processIdentifier
                    let known: Set<Shown> = previous[pid] ?? []
                    let current: Set<Shown> = Set(elements.map { element in Shown(pid: pid, element: element) })
                    for shown: Shown in known.subtracting(current).sorted() { write(Event(app: app, event: .closed, pid: pid, element: shown.element)) }
                    for shown: Shown in current.subtracting(known).sorted() { write(Event(app: app, event: .open, pid: pid, element: shown.element)) }
                    previous[pid] = current
                case .status(_, .success(.unresponsive)): continue
                case .status(let instance, .failure(let error)): write(Event(app: app, event: .fail, pid: instance.processIdentifier, reason: .element(error)))
            }
        }
    }

    nonisolated func write(_ event: Event) {
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
    let shows: (Element) -> Bool = { element in ([element.title].compactMap(\.self) + element.text).contains { line in line.contains(text) } }
    for await case .status(let instance, let read) in NSWorkspace.shared.instanceEvents(of: { instance in instance.bundleIdentifier.map(ids.contains) == true }) {
        let outcome: Result<Void, ElementFailure>? =
            switch read {
                case .success(.responding(_, let elements)): elements.first(where: shows).map { element in instance.perform(action, on: element.id) }
                case .success(.unresponsive): nil
                case .failure(let error): .failure(error)
            }
        if let outcome {
            return Event(
                app: instance.bundleIdentifier,
                event: outcome.failure == nil ? .answered : .fail,
                pid: instance.processIdentifier,
                action: action,
                reason: outcome.failure.map(Failure.element),
            )
        }
    }
    return Event(event: .fail, action: action, reason: .unanswered)
}

@MainActor func watched(_ ids: [String]) async {
    let watches: [Result<Watch, any Error>] = ids.map { id in Result { try Watch(app: id) } }
    for event: Event in zip(ids, watches).compactMap({ id, watch in watch.failure.map { error in Event(app: id, event: .fail, reason: .log(error)) } }) { output.yield(.output(event)) }
    await withDiscardingTaskGroup { group in for case .success(let watch) in watches { watch.run(in: &group) } }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

let (lines, output): (AsyncStream<Line>, AsyncStream<Line>.Continuation) = AsyncStream.makeStream(of: Line.self)
let encoder: JSONEncoder = JSONEncoder()
encoder.outputFormatting = [.sortedKeys, .withoutEscapingSlashes]
let encoded: (Event) throws -> Data = { event in try encoder.encode(event) + [UInt8(ascii: "\n")] }
let command: WatchApps
switch Result(catching: { try WatchApps.parse() }) {
    case .success(let parsed): command = parsed
    case .failure(let error) where WatchApps.exitCode(for: error) == .success: WatchApps.exit(withError: error)
    case .failure(let error):
        try FileHandle.standardOutput.write(contentsOf: encoded(Event(event: .fail, reason: .arguments(error))))
        exit(EXIT_FAILURE)
}
let (names, naming): (AsyncStream<String>, AsyncStream<String>.Continuation) = AsyncStream.makeStream()
for name: String in command.apps { naming.yield(name) }
naming.finish()
let apps: [(name: String, ids: Result<[String], DiscoveryFailure>?)] = await names.map { name in
    (name, await NSWorkspace.shared.applications(matching: .identifier(name), .name(name)).first { _ in true }?.map { found in found.map(\.identifier) })
}
.reduce(into: []) { apps, app in apps.append(app) }
let failures: [Event] = apps.compactMap { app in
    switch app.ids {
        case .success([]), .none: Event(app: app.name, event: .fail, reason: .notInstalled)
        case .success: nil
        case .failure(let error): Event(app: app.name, event: .fail, reason: .discovery(error))
    }
}
let ids: [String] = Set(apps.compactMap { app in if case .success(let ids) = app.ids { ids } else { nil } }.joined()).sorted()
let parent: any DispatchSourceProcess = DispatchSource.makeProcessSource(identifier: getppid(), eventMask: .exit, queue: .main)
parent.setEventHandler { exit(EXIT_SUCCESS) }
parent.activate()
for event: Event in failures { output.yield(.output(event)) }
let work: Task<Int32, Never> = Task { @MainActor in
    defer { output.finish() }
    switch (failures.isEmpty, command.answering, command.crashes) {
        case (false, _, _):
            return EXIT_FAILURE
        case (true, .some(let answer), _):
            let outcome: Event = await answered(ids, answer.text, answer.action)
            if let app: String = outcome.app, case .success(let log) = Result(catching: { try logHandle(app, .writeOnly) }) { output.yield(.log(outcome, log)) }
            output.yield(.output(outcome))
            return outcome.event == .answered ? EXIT_SUCCESS : EXIT_FAILURE
        case (true, .none, true):
            let reports: [Event] = ids.flatMap { id in
                switch crashReports(for: id) {
                    case .success(let reports): reports.map { report in Event(app: id, event: .crash, report: report.path(percentEncoded: false)) }
                    case .failure(let error): [Event(app: id, event: .fail, reason: .crashReports(error))]
                }
            }
            for event: Event in reports { output.yield(.output(event)) }
            return EXIT_SUCCESS
        case (true, .none, false):
            await watched(ids)
            return EXIT_SUCCESS
    }
}
for await line: Line in lines {
    switch line {
        case .output(let event): try FileHandle.standardOutput.write(contentsOf: encoded(event))
        case .log(let event, let log): try log.write(contentsOf: encoded(event))
        case .followed(let line): try FileHandle.standardOutput.write(contentsOf: line + [UInt8(ascii: "\n")])
    }
}
exit(await work.value)
