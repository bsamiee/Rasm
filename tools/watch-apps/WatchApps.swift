import AppKit
import Applications
import ArgumentParser
import AsyncAlgorithms
import Encoding
import Files
import OrderedCollections
import Results
import Subprocess
import System

// --- [CONSTANTS] -----------------------------------------------------------------------

let encoder: JSONEncoder = {
    let encoder: JSONEncoder = JSONEncoder()
    encoder.outputFormatting = [.sortedKeys, .withoutEscapingSlashes]
    return encoder
}()

// --- [MODELS] --------------------------------------------------------------------------

enum Event: Codable, Hashable {
    case watch
    case launch(pid: pid_t)
    case exit(pid: pid_t, status: Int32?, signal: Int32?)
    case open(pid: pid_t, element: Element)
    case closed(pid: pid_t, element: Element)
    case unresponsive(pid: pid_t)
    case responding(pid: pid_t)
    case answered(pid: pid_t, action: ElementAction)
    case crash(report: CrashReport)
    case fail(pid: pid_t?, file: String?, reason: String)

    static func failed(_ error: any Error, pid: pid_t? = nil, file: URL? = nil) -> Self {
        .fail(pid: pid, file: file?.path(percentEncoded: false), reason: error.localizedDescription)
    }
}

extension Set<Event> {
    func changes(from event: InstanceEvent) -> [Event] {
        let pid: pid_t = event.instance.processIdentifier
        return switch event {
            case .launched: [.launch(pid: pid)]
            case .terminated(_, .success(.exited(let status))): [.exit(pid: pid, status: status, signal: nil)]
            case .terminated(_, .success(.signaled(let signal))): [.exit(pid: pid, status: nil, signal: signal)]
            case .terminated(_, .failure(let error)): [.exit(pid: pid, status: nil, signal: nil), .failed(error, pid: pid)]
            case .status(_, .failure(let error)): [.failed(error, pid: pid)]
            case .status(_, .success(.unresponsive)): [.unresponsive(pid: pid)]
            case .status(_, .success(.responding(let elements))):
                [.responding(pid: pid)]
                    + compactMap { fact in if case .open(pid, let element) = fact, !elements.contains(element) { element } else { nil } }
                    .sorted(using: KeyPathComparator(\.id)).map { element in .closed(pid: pid, element: element) }
                    + elements.map { element in .open(pid: pid, element: element) }
        }
    }

    func ran(_ pid: pid_t) -> Bool {
        contains { fact in
            switch fact {
                case .launch(pid), .exit(pid, _, _): true
                default: false
            }
        }
    }

    mutating func record(_ event: Event) -> Bool {
        switch event {
            case .launch, .open, .unresponsive, .crash: return insert(event).inserted
            case .closed(let pid, let element): return remove(.open(pid: pid, element: element)) != nil
            case .responding(let pid): return remove(.unresponsive(pid: pid)) != nil
            case .exit(let pid, _, _):
                guard remove(.launch(pid: pid)) != nil else { return false }
                self = filter { fact in
                    switch fact {
                        case .open(pid, _), .unresponsive(pid), .exit(pid, _, _): false
                        default: true
                    }
                }
                return insert(event).inserted
            case .watch, .answered, .fail: return true
        }
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

@MainActor
final class Watcher {
    // --- [STATE]
    let app: String
    let output: AsyncStream<(app: String, event: Event)>.Continuation
    var shown: Set<Event> = []

    init(app: String, output: AsyncStream<(app: String, event: Event)>.Continuation) {
        self.app = app
        self.output = output
    }

    // --- [LOG]
    static func log(of app: String) -> URL {
        URL.temporaryDirectory.appending(path: "\(app).events")
    }

    nonisolated static func open(_ log: URL, _ access: FileDescriptor.AccessMode, options: FileDescriptor.OpenOptions) throws -> FileHandle {
        try FileHandle(fileDescriptor: FileDescriptor.open(FilePath(log.path(percentEncoded: false)), access, options: options, permissions: [.ownerReadWrite]).rawValue, closeOnDealloc: true)
    }

    @concurrent
    static func lock(_ log: URL) async throws -> FileHandle {
        try await withTaskExecutorPreference(DispatchQueue(label: "watch-apps.log", attributes: .concurrent)) {
            try open(log, .writeOnly, options: [.create, .append, .closeOnExec, .exclusiveLock])
        }
    }

    // --- [FOLLOW]
    func run() async {
        let reader: Result<FileHandle, any Error> = Result { try Self.open(Self.log(of: app), .readOnly, options: [.create, .closeOnExec]) }
            .flatMap { handle in Result { try handle.seekToEnd() }.map { _ in handle } }
        show([.watch])
        for instance: NSRunningApplication in NSRunningApplication.instances(withBundleIdentifier: app) {
            show([.launch(pid: instance.processIdentifier)] + shown.changes(from: .status(instance, await instance.status())))
        }
        await withDiscardingTaskGroup { group in
            group.addTask { await self.follow(reader) }
            group.addTask { await self.hold() }
            group.addTask { await self.crashes() }
        }
    }

    func show(_ events: [Event]) {
        for event: Event in events where shown.record(event) { output.yield((app, event)) }
    }

    func follow(_ reader: Result<FileHandle, any Error>) async {
        switch reader {
            case .failure(let error): show([.failed(error, file: Self.log(of: app))])
            case .success(let handle):
                let decoder: JSONDecoder = JSONDecoder()
                for await read: Result<Data, FileWatchFailure> in FileWatch.lines(following: handle) {
                    let events: [Event] =
                        switch Result(catching: { try decoder.decode([String: Event].self, from: read.get()) }) {
                            case .success(let line): Array(line.values)
                            case .failure(let error): [.failed(error, file: Self.log(of: app))]
                        }
                    show(events)
                }
        }
    }

    func crashes() async {
        for await reads: [Result<CrashReport, CrashReportFailure>] in CrashReport.following() {
            show(
                reads.compactMap { read in
                    switch read {
                        case .success(let report) where shown.ran(report.pid): .crash(report: report)
                        case .success: nil
                        case .failure(let error): .failed(error)
                    }
                }
            )
        }
    }

    // --- [HOLD]
    func hold() async {
        let log: URL = Self.log(of: app)
        let held: Result<Void, any Error> = await Result {
            let handle: FileHandle = try await Self.lock(log)
            let write: ([Event]) throws -> Void = { [app] events in
                try handle.write(contentsOf: Data(events.map { event in try encoder.encodeLine([app: event]) }.joined()))
            }
            var logged: Set<Event> = shown
            let running: Set<pid_t> = Set(NSRunningApplication.instances(withBundleIdentifier: app).map(\.processIdentifier))
            try write(
                logged.compactMap { fact in if case .launch(let pid) = fact, !running.contains(pid) { pid } else { nil } }.sorted().map { pid in .exit(pid: pid, status: nil, signal: nil) }
                    .filter { event in logged.record(event) }
            )
            for await change: InstanceEvent in NSWorkspace.shared.instanceEvents(of: { [app] instance in instance.bundleIdentifier == app }) {
                try write(logged.changes(from: change).filter { event in logged.record(event) })
            }
        }
        if case .failure(let error) = held { show([.failed(error, file: log)]) }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

@main
struct WatchApps: AsyncParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(
        abstract: "Follow, launch, answer, and list crash reports of installed applications, exiting 1 after printing a fail line",
        subcommands: [Watch.self, Launch.self, Answer.self, Crashes.self],
    )

    @MainActor
    static func listing(_ name: String, _ comparison: NSComparisonPredicate.Operator) async -> Result<[Installation], AggregateError<DiscoveryFailure>>? {
        let keys: [String] = [kMDItemCFBundleIdentifier as String, NSMetadataItemDisplayNameKey]
        let matching: NSPredicate = NSCompoundPredicate(
            orPredicateWithSubpredicates: keys.map { key in
                NSComparisonPredicate(
                    leftExpression: NSExpression(forKeyPath: key),
                    rightExpression: NSExpression(forConstantValue: name),
                    modifier: .direct,
                    type: comparison,
                    options: [.caseInsensitive, .diacriticInsensitive],
                )
            }
        )
        return await NSWorkspace.shared.applications(matching: matching).first { @Sendable _ in true }
    }

    @MainActor
    static func run(_ names: [String], _ work: @escaping @MainActor ([Installation], AsyncStream<(app: String, event: Event)>.Continuation) async -> Void) async throws {
        let lookups: [(name: String, rows: Task<Result<[Installation], any Error>, Never>)] = OrderedSet(names).map { name in
            (
                name,
                Task {
                    switch await Self.listing(name, .equalTo) {
                        case .success([]):
                            switch await Self.listing(name, .contains) {
                                case .success(let rows): .failure(Unmatched(subject: "installed application", query: name, candidates: rows.map(\.identifier)))
                                case .failure(let error): .failure(error)
                                case nil: .failure(CancellationError())
                            }
                        case .success(let rows): .success(rows)
                        case .failure(let error): .failure(error)
                        case nil: .failure(CancellationError())
                    }
                },
            )
        }
        let resolved: Result<[[Installation]], AggregateError<(app: String, event: Event)>> = traverse(await Array(lookups.async.map { lookup in (lookup.name, await lookup.rows.value) })) {
            name,
            rows in
            rows.mapError { error in AggregateError(first: (name, .failed(error))) }
        }
        let (lines, output): (AsyncStream<(app: String, event: Event)>, AsyncStream<(app: String, event: Event)>.Continuation) = AsyncStream.makeStream()
        let parent: any DispatchSourceProcess = DispatchSource.makeProcessSource(identifier: getppid(), eventMask: .exit, queue: .main)
        parent.setEventHandler { output.finish() }
        parent.activate()
        async let _: Void = { @MainActor in
            defer { output.finish() }
            switch resolved {
                case .success(let rows): await work(OrderedSet(rows.joined()).elements, output)
                case .failure(let failed): for line: (app: String, event: Event) in failed.errors { output.yield(line) }
            }
        }()
        var failed: Bool = false
        for await (app, event): (app: String, event: Event) in lines {
            try FileHandle.standardOutput.write(contentsOf: encoder.encodeLine([app: event]))
            if case .fail = event { failed = true }
        }
        Self.exit(withError: failed ? ExitCode.failure : nil)
    }
}

struct Installed: ParsableArguments {
    @Argument(help: "Bundle identifiers or display names of installed applications, matched without case or diacritics")
    var names: [String]
}

struct Watch: AsyncParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(
        abstract: "Print each running instance, each window, dialog, sheet, and menu as it opens and closes, each launch, exit, and hang, and each crash report of a watched instance"
    )

    @OptionGroup var installed: Installed

    func run() async throws {
        try await WatchApps.run(installed.names) { installations, output in
            await withDiscardingTaskGroup { group in
                for watcher: Watcher in OrderedSet(installations.map(\.identifier)).map({ id in Watcher(app: id, output: output) }) { group.addTask { await watcher.run() } }
            }
        }
    }
}

struct Launch: AsyncParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(
        abstract: "Open each application in the background without a document window, and exit when it finishes launching"
    )

    @OptionGroup var installed: Installed

    @Flag(help: "Open a new instance when one is running")
    var new: Bool = false

    @Option(help: "Seconds the application gets to finish launching, unbounded when absent")
    var within: Int?

    func run() async throws {
        let (new, timeout): (Bool, Duration?) = (new, within.map { seconds in .seconds(seconds) })
        try await WatchApps.run(installed.names) { installations, output in
            await withDiscardingTaskGroup { group in
                for (id, rows): (String, [Installation]) in OrderedDictionary(grouping: installations, by: \.identifier) {
                    group.addTask {
                        let launched: Result<NSRunningApplication, any Error> = await Result {
                            let installation: Installation = try Unmatched.single(rows, subject: "installation of \(id)", query: nil, candidates: []) { row in row.url.path(percentEncoded: false) }
                                .get()
                            let configuration: NSWorkspace.OpenConfiguration = NSWorkspace.OpenConfiguration()
                            configuration.createsNewApplicationInstance = new
                            return try await NSWorkspace.shared.launchApplication(at: installation.url, within: timeout, configuration: configuration).get()
                        }
                        let event: Event =
                            switch launched {
                                case .success(let instance): .launch(pid: instance.processIdentifier)
                                case .failure(let error): .failed(error)
                            }
                        output.yield((id, event))
                    }
                }
            }
        }
    }
}

struct Answer: AsyncParsableCommand {
    struct Instance: ParsableArguments {
        @OptionGroup var installed: Installed

        @Option(help: "Process identifier of the instance to answer, among several running")
        var pid: pid_t?
    }

    struct Prompt: ParsableArguments {
        @OptionGroup var instance: Instance

        @Option(help: "Text in the title or static text of the element to answer")
        var text: String

        func answer(_ action: ElementAction) async throws {
            let (text, pid): (String, pid_t?) = (text, instance.pid)
            try await WatchApps.run(instance.installed.names) { installations, output in
                let ids: OrderedSet<String> = OrderedSet(installations.map(\.identifier))
                guard
                    let found: (app: String, instance: NSRunningApplication, target: Result<Element, ElementFailure>) = await NSWorkspace.shared
                        .instanceEvents(of: { instance in instance.bundleIdentifier.map(ids.contains) == true && (pid == nil || pid == instance.processIdentifier) })
                        .compactMap({ event -> (app: String, instance: NSRunningApplication, target: Result<Element, ElementFailure>)? in
                            switch event {
                                case .status(let instance, .success(.responding(let elements))):
                                    instance.bundleIdentifier.flatMap { app in
                                        elements.first { element in ([element.title].compactMap(\.self) + element.text).contains { line in line.contains(text) } }.map { element in
                                            (app, instance, .success(element))
                                        }
                                    }
                                case .status(let instance, .failure(let error)): instance.bundleIdentifier.map { app in (app, instance, .failure(error)) }
                                case .launched, .terminated, .status(_, .success(.unresponsive)): nil
                            }
                        })
                        .first(where: { @Sendable _ in true })
                else { return }
                await Answer.perform(action, on: found.target.map { element in .element(element.id) }, of: found.instance, app: found.app, output: output)
            }
        }
    }

    @MainActor
    static func perform(
        _ action: ElementAction,
        on target: Result<ElementTarget, ElementFailure>,
        of instance: NSRunningApplication,
        app: String,
        output: AsyncStream<(app: String, event: Event)>.Continuation,
    ) async {
        let pid: pid_t = instance.processIdentifier
        let log: URL = Watcher.log(of: app)
        let event: Event =
            switch await Result(catching: { () async throws(ElementFailure) in try await instance.perform(action, on: target.get()).get() }) {
                case .success: .answered(pid: pid, action: action)
                case .failure(let error): .failed(error, pid: pid)
            }
        output.yield((app, event))
        if case .failure(let error) = Result(catching: { try Watcher.open(log, .writeOnly, options: [.create, .append, .closeOnExec]).write(contentsOf: encoder.encodeLine([app: event])) }) {
            output.yield((app, .failed(error, file: log)))
        }
    }

    struct Press: AsyncParsableCommand {
        static let configuration: CommandConfiguration = CommandConfiguration(abstract: "Press the control of the element with a label from its actions, or an item reached through them")

        @OptionGroup var prompt: Prompt

        @Option(help: "Label of an action the element lists, or of the item the --under labels reach")
        var action: String

        @Option(help: "Label of each item leading to the action, outermost first: the first an action the element lists, each next a child of the one before")
        var under: [String] = []

        func run() async throws {
            try await prompt.answer(.press(label: action, under: under))
        }
    }

    struct Menu: AsyncParsableCommand {
        static let configuration: CommandConfiguration = CommandConfiguration(
            abstract: "Press an item of the running instance's menu bar through the menus leading to it, without waiting for an element"
        )

        @OptionGroup var instance: Instance

        @Option(help: "Label of each menu leading to the item, outermost first, starting at a menu bar item")
        var menu: [String]

        @Option(help: "Label of the menu item to press")
        var item: String

        func run() async throws {
            let (pid, action): (pid_t?, ElementAction) = (instance.pid, .press(label: item, under: menu))
            try await WatchApps.run(instance.installed.names) { installations, output in
                let ids: OrderedSet<String> = OrderedSet(installations.map(\.identifier))
                let running: [(app: String, instance: NSRunningApplication)] = ids.flatMap { app in NSRunningApplication.instances(withBundleIdentifier: app).map { instance in (app, instance) } }
                switch Unmatched.single(
                    running.filter { entry in pid == nil || pid == entry.instance.processIdentifier },
                    subject: "running instance",
                    query: pid.map(String.init),
                    candidates: running.map { entry in String(entry.instance.processIdentifier) },
                    named: { entry in String(entry.instance.processIdentifier) },
                ) {
                    case .success(let found): await Answer.perform(action, on: .success(.menuBar), of: found.instance, app: found.app, output: output)
                    case .failure(let miss): output.yield((ids.joined(separator: ", "), .failed(miss)))
                }
            }
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
        abstract: "Wait for an element showing text and perform an action on it, or press a menu bar item, and exit",
        subcommands: [Press.self, Set.self, Menu.self],
    )
}

struct Crashes: AsyncParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(abstract: "Print the crash reports naming each application, newest first, and exit")

    @OptionGroup var installed: Installed

    func run() async throws {
        try await WatchApps.run(installed.names) { installations, output in
            for id: String in OrderedSet(installations.map(\.identifier)) {
                let (reports, failures): ([CrashReport], [CrashReportFailure]) = CrashReport.reports(for: id)
                for event: Event in reports.map(Event.crash) + failures.map({ failure in .failed(failure) }) { output.yield((id, event)) }
            }
        }
    }
}
