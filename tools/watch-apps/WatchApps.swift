import AppKit
import Applications
import ArgumentParser
import Encoding
import Files
import Results
import System

// --- [MODELS] --------------------------------------------------------------------------

enum Event: Codable {
    case watch
    case launch(pid: pid_t)
    case exit(pid: pid_t, status: Int?)
    case open(pid: pid_t, element: Element)
    case closed(pid: pid_t, element: Element)
    case answered(pid: pid_t, action: ElementAction)
    case fail(pid: pid_t?, reason: Failure)
    case crash(report: String)
}

struct Line: Codable {
    let app: String
    let event: Event
}

// --- [ERRORS] --------------------------------------------------------------------------

enum Failure: Codable {
    case described(any Error)
    case file(URL, any Error)
    case followed(String)

    init(from decoder: any Decoder) throws {
        self = try .followed(decoder.singleValueContainer().decode(String.self))
    }

    func encode(to encoder: any Encoder) throws {
        var reason: any SingleValueEncodingContainer = encoder.singleValueContainer()
        let text: String =
            switch self {
                case .described(let error): error.localizedDescription
                case .file(let file, let error): "\(file.path(percentEncoded: false)): \(error.localizedDescription)"
                case .followed(let text): text
            }
        try reason.encode(text)
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

@MainActor
final class Watcher {
    // --- [STATE]
    let app: String
    let output: AsyncStream<Line>.Continuation
    var visible: [pid_t: Set<Element>] = [:]

    init(app: String, output: AsyncStream<Line>.Continuation) {
        self.app = app
        self.output = output
    }

    // --- [FOLLOW]
    func run() async {
        let followed: Result<FileHandle, any Error> = await logHandle(app, .readOnly, options: [.create, .closeOnExec]).flatMap { handle in
            Result { try handle.seekToEnd() }.map { _ in handle }
        }
        show(.watch)
        for instance: NSRunningApplication in NSRunningApplication.instances(withBundleIdentifier: app) {
            let pid: pid_t = instance.processIdentifier
            let opened: [Event] =
                switch await instance.status() {
                    case .success(.responding(let elements)): elements.map { element in .open(pid: pid, element: element) }
                    case .success(.unresponsive): []
                    case .failure(let error): [.fail(pid: pid, reason: .described(error))]
                }
            for event: Event in opened { show(event) }
        }
        await withDiscardingTaskGroup { group in
            switch followed {
                case .success(let handle): group.addTask { await self.follow(FileWatch.lines(following: handle)) }
                case .failure(let error): group.addTask { await self.show(.fail(pid: nil, reason: .file(logURL(self.app), error))) }
            }
            group.addTask {
                if case .failure(let error) = await Result(catching: { try await self.hold(logHandle(self.app, .writeOnly, options: [.create, .append, .closeOnExec, .exclusiveLock]).get()) }) {
                    await self.show(.fail(pid: nil, reason: .file(logURL(self.app), error)))
                }
            }
        }
    }

    func follow(_ lines: AsyncStream<Result<Data, FileWatchFailure>>) async {
        let decoder: JSONDecoder = JSONDecoder()
        for await read: Result<Data, FileWatchFailure> in lines {
            switch Result(catching: { try decoder.decode(Line.self, from: read.get()) }) {
                case .success(let line):
                    track(line.event)
                    output.yield(line)
                case .failure(let error): show(.fail(pid: nil, reason: .file(logURL(app), error)))
            }
        }
    }

    func show(_ event: Event) {
        track(event)
        output.yield(Line(app: app, event: event))
    }

    func track(_ event: Event) {
        switch event {
            case .open(let pid, let element): visible[pid, default: []].insert(element)
            case .closed(let pid, let element): visible[pid]?.remove(element)
            case .exit(let pid, _): visible[pid] = nil
            case .watch, .launch, .answered, .fail, .crash: break
        }
    }

    // --- [HOLD]
    func hold(_ log: FileHandle) async throws {
        let encoder: JSONEncoder = JSONEncoder()
        let write: ([Event]) throws -> Void = { [app] events in
            try log.write(contentsOf: Data(events.map { event in try encoder.encodeLine(Line(app: app, event: event)) }.joined()))
        }
        let ordered: KeyPathComparator<Element> = KeyPathComparator(\.id)
        let running: Set<pid_t> = Set(NSRunningApplication.instances(withBundleIdentifier: app).map(\.processIdentifier))
        try write(
            visible.filter { pid, _ in !running.contains(pid) }.sorted { left, right in left.key < right.key }
                .flatMap { pid, elements in elements.sorted(using: ordered).map { element in .closed(pid: pid, element: element) } }
        )
        var previous: [pid_t: Set<Element>] = visible.filter { pid, _ in running.contains(pid) }
        for await event: InstanceEvent in NSWorkspace.shared.instanceEvents(of: { [app] instance in instance.bundleIdentifier == app }) {
            let pid: pid_t = event.instance.processIdentifier
            let known: Set<Element> = previous[pid, default: []]
            let (now, written): (Set<Element>, [Event]) =
                switch event {
                    case .launched: (known, [.launch(pid: pid)])
                    case .terminated(_, let status): ([], [.exit(pid: pid, status: status)])
                    case .status(_, .success(.responding(let elements))):
                        (
                            Set(elements),
                            known.subtracting(elements).sorted(using: ordered).map { element in .closed(pid: pid, element: element) }
                                + Set(elements).subtracting(known).sorted(using: ordered).map { element in .open(pid: pid, element: element) },
                        )
                    case .status(_, .success(.unresponsive)): (known, [])
                    case .status(_, .failure(let error)): (known, [.fail(pid: pid, reason: .described(error))])
                }
            previous[pid] = now
            try write(written)
        }
    }
}

let log: DispatchQueue = DispatchQueue(label: "watch-apps.log", attributes: .concurrent)

// --- [OPERATIONS] ----------------------------------------------------------------------

func logURL(_ app: String) -> URL {
    URL.temporaryDirectory.appending(path: "\(app).events")
}

@concurrent
func logHandle(_ app: String, _ access: FileDescriptor.AccessMode, options: FileDescriptor.OpenOptions) async -> Result<FileHandle, any Error> {
    await withTaskExecutorPreference(log) {
        Result {
            try FileHandle(
                fileDescriptor: FileDescriptor.open(FilePath(logURL(app).path(percentEncoded: false)), access, options: options, permissions: [.ownerReadWrite]).rawValue,
                closeOnDealloc: true,
            )
        }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

@main
struct WatchApps: AsyncParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(
        abstract: "Follow, answer, and list crash reports of installed applications",
        subcommands: [Watch.self, Answer.self, Crashes.self],
    )

    @MainActor
    static func run(_ names: [String], _ work: @escaping @MainActor ([String], AsyncStream<Line>.Continuation) async throws -> ExitCode) async throws {
        let predicate: NSPredicate = NSCompoundPredicate(
            orPredicateWithSubpredicates: names.flatMap { name in [kMDItemCFBundleIdentifier as String, NSMetadataItemDisplayNameKey].map { key in NSPredicate(format: "%K == %@", key, name) } }
        )
        guard let listed: Result<[Installation], AggregateError<DiscoveryFailure>> = await NSWorkspace.shared.applications(matching: predicate).first(where: { @Sendable _ in true }) else {
            throw CancellationError()
        }
        let (ids, failures): ([String], [Line]) =
            switch listed {
                case .failure(let error): ([], names.flatMap { name in error.errors.map { failure in Line(app: name, event: .fail(pid: nil, reason: .described(failure))) } })
                case .success(let rows):
                    (
                        Set(rows.map(\.identifier)).sorted(),
                        names.filter { name in !rows.contains { row in [row.identifier, row.name].contains(name) } }
                            .map { name in Line(app: name, event: .fail(pid: nil, reason: .described(Unmatched(subject: "installed application", query: name, candidates: [])))) },
                    )
            }
        let parent: any DispatchSourceProcess = DispatchSource.makeProcessSource(identifier: getppid(), eventMask: .exit, queue: .main)
        parent.setEventHandler { exit() }
        parent.activate()
        let (lines, output): (AsyncStream<Line>, AsyncStream<Line>.Continuation) = AsyncStream.makeStream()
        async let code: ExitCode = { @MainActor () async throws -> ExitCode in
            defer { output.finish() }
            for line: Line in failures { output.yield(line) }
            return if failures.isEmpty { try await work(ids, output) } else { .failure }
        }()
        let encoder: JSONEncoder = JSONEncoder()
        encoder.outputFormatting = [.sortedKeys, .withoutEscapingSlashes]
        for await line: Line in lines { try FileHandle.standardOutput.write(contentsOf: encoder.encodeLine(line)) }
        throw try await code
    }
}

struct Installed: ParsableArguments {
    @Argument(help: "Bundle identifiers or display names of installed applications")
    var names: [String]
}

struct Watch: AsyncParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(abstract: "Print each window, dialog, sheet, and menu as it opens and closes, and each launch and exit")

    @OptionGroup var installed: Installed

    func run() async throws {
        try await WatchApps.run(installed.names) { ids, output in
            await withDiscardingTaskGroup { group in
                for watcher: Watcher in ids.map({ id in Watcher(app: id, output: output) }) { group.addTask { await watcher.run() } }
            }
            return .failure
        }
    }
}

struct Answer: AsyncParsableCommand {
    struct Prompt: ParsableArguments {
        @OptionGroup var installed: Installed

        @Option(help: "Text in the title or static text of the element to answer")
        var text: String

        func answer(_ action: ElementAction) async throws {
            let text: String = text
            try await WatchApps.run(installed.names) { ids, output in
                let shows: @Sendable (Element) -> Bool = { element in ([element.title].compactMap(\.self) + element.text).contains { line in line.contains(text) } }
                let found: (instance: NSRunningApplication, target: Result<Element, ElementFailure>)? = await NSWorkspace.shared
                    .instanceEvents(of: { instance in instance.bundleIdentifier.map(ids.contains) == true })
                    .compactMap { event -> (instance: NSRunningApplication, target: Result<Element, ElementFailure>)? in
                        switch event {
                            case .status(let instance, .success(.responding(let elements))): elements.first(where: shows).map { element in (instance, .success(element)) }
                            case .status(let instance, .failure(let error)): (instance, .failure(error))
                            case .launched, .terminated, .status(_, .success(.unresponsive)): nil
                        }
                    }
                    .first { @Sendable _ in true }
                guard let found, let app: String = found.instance.bundleIdentifier else { throw CancellationError() }
                let pid: pid_t = found.instance.processIdentifier
                let line: Line =
                    switch await Result(catching: { () async throws(ElementFailure) in try await found.instance.perform(action, on: found.target.get().id).get() }) {
                        case .success: Line(app: app, event: .answered(pid: pid, action: action))
                        case .failure(let error): Line(app: app, event: .fail(pid: pid, reason: .described(error)))
                    }
                if case .failure(let error) = await Result(catching: {
                    try await logHandle(app, .writeOnly, options: [.create, .append, .closeOnExec]).get().write(contentsOf: JSONEncoder().encodeLine(line))
                }) {
                    output.yield(Line(app: app, event: .fail(pid: nil, reason: .file(logURL(app), error))))
                }
                output.yield(line)
                return if case .answered = line.event { .success } else { .failure }
            }
        }
    }

    struct Press: AsyncParsableCommand {
        static let configuration: CommandConfiguration = CommandConfiguration(abstract: "Press the control of the element with a label from its actions")

        @OptionGroup var prompt: Prompt

        @Option(help: "Control label from the element's actions")
        var action: String

        func run() async throws {
            try await prompt.answer(.press(action))
        }
    }

    struct Set: AsyncParsableCommand {
        static let configuration: CommandConfiguration = CommandConfiguration(abstract: "Set the text of the element's control with a label")

        @OptionGroup var prompt: Prompt

        @Option(help: "Label of the control")
        var field: String

        @Option(help: "Text the control receives")
        var value: String

        func run() async throws {
            try await prompt.answer(.set(field: field, value: value))
        }
    }

    static let configuration: CommandConfiguration = CommandConfiguration(
        abstract: "Wait for an element showing text, perform an action on it, and exit",
        subcommands: [Press.self, Set.self],
    )
}

struct Crashes: AsyncParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(abstract: "Print the crash reports naming each application, newest first, and exit")

    @OptionGroup var installed: Installed

    func run() async throws {
        try await WatchApps.run(installed.names) { ids, output in
            let found: [(lines: [Line], code: ExitCode)] = ids.map { id in
                switch crashReports(for: id) {
                    case .success(let reports): (reports.map { report in Line(app: id, event: .crash(report: report.path(percentEncoded: false))) }, .success)
                    case .failure(let error): (error.errors.map { failed in Line(app: id, event: .fail(pid: nil, reason: .file(failed.url, failed.error))) }, .failure)
                }
            }
            for line: Line in found.flatMap(\.lines) { output.yield(line) }
            return found.contains { found in found.code == .failure } ? .failure : .success
        }
    }
}
