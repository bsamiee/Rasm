import AppKit
import ApplicationServer
import Applications
import ArgumentParser
import AsyncAlgorithms
import Carbon.HIToolbox
import CasePaths
import CurrentProcess
import EffectRPC
import Encoding
import OrderedCollections
import Processes
import Results
import Subprocess
import Synchronization
import System
import os

// --- [CONSTANTS] -----------------------------------------------------------------------

private let program: String = ProcessInfo.processInfo.processName
private let variables: [String: String] = ProcessInfo.processInfo.environment
private let dateFormat: Date.ISO8601FormatStyle = .init(includingFractionalSeconds: true)
private let encoder: JSONEncoder = {
    let encoder = JSONEncoder()
    encoder.outputFormatting = [.sortedKeys, .withoutEscapingSlashes]
    encoder.dateEncodingStrategy = .custom { date, target in
        var value: any SingleValueEncodingContainer = target.singleValueContainer()
        try value.encode(dateFormat.format(date))
    }
    return encoder
}()
let logVariable: String = "WATCH_APPS_LOG"
let sessionVariable: String = "CLAUDE_CODE_SESSION_ID"
let lineLimit: Int = 500
private let installedNames: ArgumentHelp = "Installed application paths, bundle identifiers, display names, or process IDs of running instances. Names ignore case and diacritics"

// --- [MODELS] --------------------------------------------------------------------------

private struct Record<Line> {
    let at: Date
    let pid: pid_t
    var session: String?
    var argv: [String]?
    var line: Line?
    var exit: Exit?

    init(argv: [String]? = nil, line: Line? = nil, exit: Exit? = nil) {
        (at, pid, session, self.argv, self.line, self.exit) = (.now, getpid(), argv == nil ? nil : variables[sessionVariable], argv, line, exit)
    }
}

extension Record: Encodable where Line: Encodable {}
extension Record: Decodable where Line: Decodable {}

private struct Exit: Codable {
    let status: Int32
    let signal: Int32?
}

private enum Event: Encodable {
    case fail(ApplicationFailure)
    case launched(created: Bool)
    case running(Outline)
    case captured(Screenshot)

    private enum Key: CodingKey {
        case fail, launched, running, captured, created
    }

    func encode(to encoder: any Encoder) throws {
        var fields: KeyedEncodingContainer<Key> = encoder.container(keyedBy: Key.self)
        switch self {
            case .fail(let failure): try fields.encode(failure, forKey: .fail)
            case .launched(let created):
                var launched: KeyedEncodingContainer<Key> = fields.nestedContainer(keyedBy: Key.self, forKey: .launched)
                try launched.encode(created, forKey: .created)
            case .running(let outline): try fields.encode(outline, forKey: .running)
            case .captured(let screenshot): try fields.encode(screenshot, forKey: .captured)
        }
    }
}

private struct Screenshot: Encodable {
    let file: String
    let number: CGWindowID
}

struct Outline: Encodable {
    struct Row: Encodable {
        let kind: InstanceState.Window.Description
        let number: CGWindowID
        var options: [String]
        var text: [String]
        let title: String?
    }

    let active: Bool
    var asking: [Row]
    let menus: [String]
    let process: ProcessStatus?
    let status: InstanceState.Running.Status
    var windows: [CGWindowID]

    func fitted<Failure: Error>(measuring length: (Self) throws(Failure) -> Int) throws(Failure) -> Self {
        var cut: Self = self
        var excess: Int = try length(cut) - lineLimit
        while excess > 0 {
            switch (cut.asking.lastIndex { row in !row.text.isEmpty }, cut.asking.lastIndex { row in !row.options.isEmpty }, cut.windows.isEmpty) {
                case (nil, nil, true): return cut
                case (let row?, _, _):
                    let last: String = cut.asking[row].text.removeLast()
                    cut.asking[row].text += last.count > excess ? [String(last.dropLast(excess))] : []
                case (nil, let row?, _): cut.asking[row].options.removeLast()
                case (nil, nil, false): cut.windows.removeLast()
            }
            excess = try length(cut) - lineLimit
        }
        return cut
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

private final class Run: Sendable {
    private let output: FileDescriptor?
    private let log: FileDescriptor?
    private let held: Mutex<(status: ExitCode, shown: [pid_t: Data])>
    private let closed: (stream: AsyncStream<Int32?>, continuation: AsyncStream<Int32?>.Continuation) = AsyncStream.makeStream()
    private let claims: (stream: AsyncStream<NSRunningApplication?>, continuation: AsyncStream<NSRunningApplication?>.Continuation)?

    init(output: FileDescriptor? = .standardOutput, waiting: Bool = false) {
        let opening: Result<FileDescriptor, ApplicationFailure>? = variables[logVariable].map { path in
            Result { () throws(ApplicationFailure) in
                do { return try FileDescriptor.open(FilePath(path), .writeOnly, options: [.append, .create], permissions: [.ownerReadWrite, .groupRead, .otherRead]) } catch {
                    throw ApplicationFailure(subject: path, reason: .host(error as NSError))
                }
            }
        }
        self.output = output
        log = opening?[case: \.success]
        held = Mutex((waiting ? .failure : .success, [:]))
        claims = waiting ? AsyncStream.makeStream(bufferingPolicy: .bufferingNewest(1)) : nil
        append(Record<JSONValue>(argv: CommandLine.arguments))
        if case .failure(let failure) = opening { fail(program, failure) }
    }

    private func encoded(_ value: some Encodable) -> Data {
        do { return try encoder.encodeLine(value) } catch { WatchApps.exit(withError: error) }
    }

    private func append(_ record: Record<some Encodable>) {
        do { try log?.writeAll(encoded(record)) } catch { WatchApps.exit(withError: error) }
    }

    private func write(_ line: Data, logging whole: some Encodable) {
        let open: Bool = held.withLock { _ in
            append(Record(line: whole))
            do {
                try output?.writeAll(line)
                return true
            } catch Errno.brokenPipe {
                return false
            } catch {
                WatchApps.exit(withError: error)
            }
        }
        if !open { closed.continuation.yield(nil) }
    }

    func emit(_ key: String, _ value: some Encodable) { write(encoded([key: value]), logging: [key: value]) }

    func fail(_ key: String, _ failure: ApplicationFailure) {
        fail()
        emit(key, Event.fail(failure))
    }

    func fail() { held.withLock { held in if held.status.isSuccess { held.status = .failure } } }

    func set(_ status: ExitCode) { held.withLock { held in held.status = status } }

    func show(_ step: Launching, as key: String) {
        switch step {
            case .launched(let launched): emit(key, [String(launched.pid): Event.launched(created: launched.created)])
            case .state(let row): show(row.instance, row.state)
            case .prompts(let rows): for row: RunningInstance in rows { show(row.instance, row.state) }
            case .failed(let failure): fail(key, failure)
        }
    }

    func follow(_ feed: some AsyncSequence<OrderedDictionary<NSRunningApplication, InstanceState>, Never> & Sendable) async {
        _ = await feed.reduce(OrderedDictionary<NSRunningApplication, InstanceState>()) { last, next in
            for (instance, state): (NSRunningApplication, InstanceState) in next { show(instance, state) }
            for (instance, state): (NSRunningApplication, InstanceState) in last where next[instance] == nil && !state.is(\.exited) { emit(instance.key, [String: String]()) }
            return next
        }
    }

    func follow(launching steps: some AsyncSequence<Launching, Never> & Sendable, as key: String) async {
        for await step: Launching in steps {
            show(step, as: key)
            switch step {
                case .launched(let launched): claims?.continuation.yield(launched.created ? NSRunningApplication(processIdentifier: launched.pid) : nil)
                case .failed: claims?.continuation.yield(nil)
                case .state(let row) where claims != nil: settle(row.state)
                case .state, .prompts: break
            }
        }
    }

    func settle(_ state: InstanceState) {
        guard case .exited(let ended, _) = state else { return }
        claims?.continuation.yield(nil)
        switch ended.map(\.status) {
            case .success(.exited(let code)): set(ExitCode(code))
            case .success(.signaled(let signal)): set(ExitCode(128 + signal))
            case .failure: break
        }
    }

    func quit(_ instance: NSRunningApplication, _ steps: some AsyncSequence<Result<InstanceState, ApplicationFailure>, Never> & Sendable) async -> InstanceState? {
        await steps.reduce(InstanceState?.none) { last, step in
            switch step {
                case .success(let state):
                    show(instance, state)
                    return state
                case .failure(let failure):
                    fail(instance.key, failure)
                    return last
            }
        }
    }

    func release() async {
        guard case .some(.some(let instance)) = await claims?.stream.first(where: { _ in true }) else { return }
        let saved: InstanceState? = await quit(instance, instance.quit(saving: true))
        let ended: InstanceState? = saved?.is(\.exited) == true ? saved : await quit(instance, instance.quitting(force: true))
        if let ended { settle(ended) }
    }

    func recording(_ steps: some AsyncSequence<any Encodable & Sendable, Never> & Sendable) -> some AsyncSequence<any Encodable & Sendable, Never> & Sendable {
        steps.map { step in
            if case .launched(let launched)? = step as? Launching {
                self.show(.launched(launched), as: NSRunningApplication(processIdentifier: launched.pid)?.key ?? String(launched.pid))
            }
            return step
        }
    }

    func show(_ instance: NSRunningApplication, _ state: InstanceState, whole: Bool = false) {
        let (key, pid): (String, String) = (instance.key, String(instance.processIdentifier))
        let full: [String: [String: InstanceState]] = [key: [pid: state]]
        let line: Data
        if case .running(let running) = state, !whole {
            let outline: Outline = Outline(
                active: running.active,
                asking: running.asking.map { window in
                    let element: Element? = window.description[case: \.element]
                    return Outline.Row(
                        kind: window.description,
                        number: window.number,
                        options: element?.answers?.buttons.compactMap(\.label) ?? [],
                        text: element?.detail?.text ?? [],
                        title: element?.title ?? window.title,
                    )
                },
                menus: running.menus.filter { menu in menu.role == kAXMenuRole }.compactMap(\.title),
                process: running.status[case: \.unresponsive] == .stopped ? .stopped : nil,
                status: running.status,
                windows: running.windows.filter { window in window.presented && !window.asking }.map(\.number),
            )
            .fitted { outline in encoded([key: [pid: Event.running(outline)]]).count }
            line = encoded([key: [pid: Event.running(outline)]])
        } else {
            line = encoded(full)
        }
        guard held.withLock({ held in held.shown.updateValue(line, forKey: instance.processIdentifier) }) != line else { return }
        write(line, logging: full)
    }

    func finish(signal: Int32? = nil) -> Never {
        let status: ExitCode = held.withLock { held in held.status }
        append(Record<JSONValue>(exit: Exit(status: status.rawValue, signal: signal)))
        WatchApps.exit(withError: status)
    }

    static func execute(output: FileDescriptor? = .standardOutput, accessibility: Bool = true, waiting: Bool = false, _ work: @escaping @Sendable (Run) async throws(ApplicationFailure) -> Void) async
    {
        let run = Run(output: output, waiting: waiting)
        let endings: AsyncMerge2Sequence<AsyncStream<Int32?>, AsyncStream<Int32?>> = merge(processEndings(), run.closed.stream)
        await withTaskGroup(of: Int32??.self) { group in
            group.addTask {
                do throws(ApplicationFailure) {
                    if accessibility { try checkAccessibilityTrust() }
                    try await work(run)
                } catch {
                    run.fail(program, error)
                }
                return nil
            }
            group.addTask { await endings.first { _ in true } }
            switch await group.next() {
                case .some(.some(let ending)):
                    await run.release()
                    run.finish(signal: ending)
                case .some(.none), .none: run.finish()
            }
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension NSRunningApplication {
    fileprivate var key: String { bundleIdentifier ?? String(processIdentifier) }
}

private func installed(_ names: [String], _ run: Run) async -> [(name: String, installations: [Installation])] {
    await OrderedSet(names).async.compactMap { name -> (name: String, installations: [Installation])? in
        do throws(ApplicationFailure) { return (name, try await Installation.installed(named: name)) } catch {
            run.fail(name, error)
            return nil
        }
    }
    .reduce(into: []) { found, entry in found.append(entry) }
}

private func act(_ action: ElementAction, key: String) async {
    await Run.execute { run in
        do throws(ApplicationFailure) {
            let (instance, state): (NSRunningApplication, InstanceState) = try await action.perform()
            run.show(instance, state)
        } catch {
            run.fail(key, error)
        }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

@main
struct WatchApps: AsyncParsableCommand {
    static let configuration: CommandConfiguration = .init(
        abstract: "Observes and drives installed applications in the background. Exits 1 after printing any fail line",
        version: Bundle.main.object(forInfoDictionaryKey: "CFBundleShortVersionString") as? String ?? "",
        subcommands: [Find.self, Watch.self, Launch.self, Read.self, Capture.self, Perform.self, SetValue.self, Key.self, Quit.self, Log.self, Serve.self],
    )

    static func main() async {
        signal(SIGPIPE, SIG_IGN)
        do {
            var command: any ParsableCommand = try parseAsRoot()
            if var command = command as? any AsyncParsableCommand { try await command.run() } else { try command.run() }
        } catch  where exitCode(for: error).isSuccess {
            exit(withError: error)
        } catch {
            let run = Run()
            run.fail(program, ApplicationFailure(subject: CommandLine.arguments.dropFirst().joined(separator: " "), reason: .malformed(message(for: error))))
            run.finish()
        }
    }
}

private struct Running: ParsableArguments {
    @Argument(help: "Running application paths, bundle identifiers, display names, or process IDs from a line. Names ignore case and diacritics")
    var names: [String]

    func each(_ run: Run, _ body: @escaping @Sendable (NSRunningApplication) async throws(ApplicationFailure) -> Void) async {
        let named: [[NSRunningApplication]] = names.map { name in
            pid_t(name).map { pid in [NSRunningApplication(processIdentifier: pid)].compactMap(\.self) } ?? NSWorkspace.shared.instances(named: name)
        }
        for (name, found): (String, [NSRunningApplication]) in zip(names, named) where found.isEmpty { run.emit(name, [String: String]()) }
        let instances = OrderedSet(named.joined())
        await withDiscardingTaskGroup { group in
            for instance: NSRunningApplication in instances {
                group.addTask {
                    do throws(ApplicationFailure) { try await body(instance) } catch { run.fail(instance.key, error) }
                }
            }
        }
    }
}

private struct Find: AsyncParsableCommand {
    static let configuration: CommandConfiguration = .init(abstract: "Prints each name's installations, best match first, with no launch")

    @Argument(help: installedNames)
    var names: [String]

    func run() async {
        await Run.execute(accessibility: false) { run in
            for (name, installations): (String, [Installation]) in await installed(names, run) { run.emit(name, installations) }
        }
    }
}

private struct Watch: AsyncParsableCommand {
    static let configuration: CommandConfiguration = .init(abstract: "Prints one line per instance change, per system prompt naming an instance, and per system pressure change above nominal")

    @Argument(help: installedNames)
    var names: [String]

    func run() async {
        await Run.execute { run in
            let applications: [Installation] = await installed(names, run).flatMap(\.installations)
            guard !applications.isEmpty else { return }
            for identifier: String in OrderedSet(applications.map(\.identifier)) where NSWorkspace.shared.instances(of: { instance in instance.bundleIdentifier == identifier }).isEmpty {
                run.emit(identifier, [String: String]())
            }
            let matching: @Sendable (NSRunningApplication) -> Bool = { [applications] instance in applications.contains { installation in installation.runs(instance) } }
            await withDiscardingTaskGroup { group in
                group.addTask {
                    var calm = true
                    for await outcome: Result<SystemPressure, Errno> in SystemPressure.changes() {
                        do throws(Errno) {
                            let pressure: SystemPressure = try outcome.get()
                            let normal: Bool = pressure.memory.isDisjoint(with: [.warning, .critical]) && pressure.thermal == .nominal
                            if !(calm && normal) { run.emit("system", pressure) }
                            calm = normal
                        } catch {
                            run.fail("system", ApplicationFailure(subject: "system", reason: .system(error)))
                        }
                    }
                }
                group.addTask { await run.follow(NSWorkspace.shared.instanceStates(of: matching)) }
                group.addTask { await run.follow(NSWorkspace.shared.systemPrompts(naming: applications.map(\.name))) }
            }
        }
    }
}

private struct Launch: AsyncParsableCommand {
    static let configuration: CommandConfiguration = .init(
        abstract: "Opens applications or documents without activation, reusing a running instance. Prints states until each instance settles or asks, or until it exits with --wait"
    )

    @Argument(help: installedNames)
    var names: [String] = []

    @Flag(help: "Opens another instance when one runs, for applications that support several")
    var new: Bool = false

    @Flag(help: "Observes one named application until it exits and returns its exit status, or 128 plus its signal. Quits an instance it created when the run ends first")
    var wait: Bool = false

    @Option(parsing: .upToNextOption, help: ArgumentHelp("Files to open", valueName: "path"), transform: { path in URL(filePath: path) })
    var document: [URL] = []

    @Option(
        parsing: .upToNextOption,
        help: "NAME=VALUE environment variables of new instance",
        transform: { entry in
            let parts: [Substring] = entry.split(separator: "=", maxSplits: 1, omittingEmptySubsequences: false)
            guard parts.count == 2 else { throw ValidationError("\(entry) lacks =, pass NAME=VALUE") }
            return (String(parts[0]), String(parts[1]))
        },
    )
    var environment: [(String, String)] = []

    @Option(parsing: .remaining, help: ArgumentHelp("Command-line arguments of new instance, every input after it", valueName: "argument"))
    var arguments: [String] = []

    func validate() throws(ValidationError) {
        guard !names.isEmpty || !document.isEmpty else { throw ValidationError("Pass names or --document") }
        guard names.count < 2 || document.isEmpty else { throw ValidationError("--document belongs to one application") }
        guard !wait || names.count == 1 else { throw ValidationError("--wait belongs to one named application") }
    }

    func run() async {
        let variables: [String: String] = Dictionary(environment) { _, last in last }
        let (names, document, arguments, new, wait): ([String], [URL], [String], Bool, Bool) = (names, document, arguments, new, wait)
        let host: ArraySlice<String> = arguments.prefix { argument in argument != "--" }
        let switches: KeyValuePairs<String, (added: String, excluded: Set<String>)> = [
            "com.mcneel.rhinoceros": ("-nosplash", ["-nosplash"]),
            "org.blenderfoundation.blender": ("--no-window-focus", ["--no-window-focus", "-b", "--background"]),
        ]
        await Run.execute(waiting: wait) { run in
            let resolved: [(key: String, outcome: Result<(Installation, [URL]), ApplicationFailure>)] =
                if names.isEmpty {
                    document.map { file in (file.path(percentEncoded: false), Result { () throws(ApplicationFailure) in (try Installation(opening: file), [file]) }) }
                } else {
                    await installed(names, run).map { name, found in (name, Result { () throws(ApplicationFailure) in (try Installation.newest(of: name, among: found), document) }) }
                }
            for case (let key, .failure(let failure)) in resolved { run.fail(key, failure) }
            let applications: OrderedDictionary<Installation, [URL]> = OrderedDictionary(
                resolved.compactMap { entry -> (Installation, [URL])? in if case .success(let application) = entry.outcome { application } else { nil } }
            ) { first, next in
                names.isEmpty ? first + next : first
            }
            await withDiscardingTaskGroup { group in
                for (installation, documents): (Installation, [URL]) in applications {
                    let added: [String] = switches.compactMap { prefix, row in installation.identifier.hasPrefix(prefix) && row.excluded.isDisjoint(with: host) ? row.added : nil }
                    group.addTask {
                        await run.follow(
                            launching: NSWorkspace.shared.launching(installation, opening: documents, arguments: added + arguments, environment: variables, newInstance: new, waiting: wait),
                            as: installation.identifier,
                        )
                    }
                }
            }
        }
    }
}

private struct Read: AsyncParsableCommand {
    static let configuration: CommandConfiguration = .init(abstract: "Prints each instance's whole state with element ids once, --window walks one window")

    @OptionGroup var running: Running

    @Option(help: "Window number from a line")
    var window: CGWindowID?

    func run() async {
        await Run.execute { run in
            await running.each(run) { instance in run.show(instance, await instance.reading(window: window), whole: true) }
        }
    }
}

private struct Capture: AsyncParsableCommand {
    static let configuration: CommandConfiguration = .init(abstract: "Writes one PNG per presented window, or of --window, with no activation. Prints one line per file")

    @OptionGroup var running: Running

    @Option(help: "Window number from a line")
    var window: CGWindowID?

    @Option(help: ArgumentHelp("Longest side in pixels, smaller captures keep native size", valueName: "pixels"))
    var longestSide: Int?

    func validate() throws(ValidationError) {
        if let longestSide, longestSide <= 0 { throw ValidationError("\(longestSide) is not a positive integer") }
    }

    func run() async {
        await Run.execute { run throws(ApplicationFailure) in
            let directory: URL = URL.temporaryDirectory.appending(component: program, directoryHint: .isDirectory)
            do { try FileManager.default.createDirectory(at: directory, withIntermediateDirectories: true) } catch {
                throw ApplicationFailure(subject: directory.path(percentEncoded: false), reason: .host(error as NSError))
            }
            await running.each(run) { instance throws(ApplicationFailure) in
                let shots: [CGWindowID: Result<URL, ApplicationFailure>] = try await instance.capturing(into: directory, window: window, longestSide: longestSide)
                guard !shots.isEmpty else { throw ApplicationFailure(subject: instance.key, reason: .noWindow) }
                for (number, shot): (CGWindowID, Result<URL, ApplicationFailure>) in shots.sorted(by: { first, second in first.key < second.key }) {
                    do throws(ApplicationFailure) {
                        let file: URL = try shot.get()
                        run.emit(instance.key, [String(instance.processIdentifier): Event.captured(Screenshot(file: file.path(percentEncoded: false), number: number))])
                    } catch {
                        run.fail(instance.key, error)
                    }
                }
            }
        }
    }
}

private struct ElementOperand: ParsableArguments {
    @Argument(
        help: "Element ID from a read line",
        transform: { id in
            guard let element = Element.ID(base64Encoded: id) else { throw ValidationError("\(id) is not base64") }
            return element
        },
    )
    var id: Element.ID
}

private struct Perform: AsyncParsableCommand {
    static let configuration: CommandConfiguration = .init(abstract: "Performs one Accessibility action on an element. Prints its instance's next state")

    @OptionGroup var element: ElementOperand

    @Argument(help: "Accessibility action from element actions")
    var action: String

    func run() async { await act(.perform(element: element.id, action: action), key: element.id.description) }
}

private struct SetValue: AsyncParsableCommand {
    static let configuration: CommandConfiguration = .init(commandName: "set", abstract: "Sets an element's \(kAXValueAttribute) to text. Prints its instance's next state")

    @OptionGroup var element: ElementOperand

    @Argument(help: "Text for element \(kAXValueAttribute)")
    var value: String

    func run() async { await act(.set(element: element.id, value: value), key: element.id.description) }
}

private struct Key: AsyncParsableCommand {
    static let configuration: CommandConfiguration = .init(
        abstract: "Posts one key to a process whose window blocks its main thread, leaving the frontmost app and cursor unchanged. Prints its next state"
    )

    @Argument(help: "Process ID from a line with an asking row of kind blocking")
    var pid: pid_t

    @Argument(help: "Virtual key code, \(kVK_Return) Return, \(kVK_Escape) Escape")
    var code: CGKeyCode

    func run() async { await act(.key(process: pid, code: code), key: String(pid)) }
}

private struct Quit: AsyncParsableCommand {
    static let configuration: CommandConfiguration = .init(abstract: "Sends each running instance the quit event. Prints states and exits 1 unless each instance exited")

    @OptionGroup var running: Running

    @Flag(help: "Kills each instance without asking. Use when no answer or quit frees it")
    var force: Bool = false

    func run() async {
        await Run.execute { run in
            await running.each(run) { instance in
                if await run.quit(instance, instance.quitting(force: force))?.is(\.exited) != true { run.fail() }
            }
        }
    }
}

private struct Log: ParsableCommand {
    static let configuration: CommandConfiguration = .init(abstract: "Prints the last line per key of each run in this Claude Code session. A process ID prints every record of that run")

    @Argument(help: "Process ID of one watch-apps run")
    var pid: pid_t?

    func run() throws {
        let decoder = JSONDecoder()
        decoder.dateDecodingStrategy = .custom { decoder in try dateFormat.parse(decoder.singleValueContainer().decode(String.self)) }
        do {
            guard let path: String = variables[logVariable] else {
                throw ApplicationFailure(subject: logVariable, reason: .unmatched(Unmatched(subject: "environment variable", query: logVariable, candidates: [])))
            }
            let lines: [Data] = try Data(contentsOf: URL(filePath: path)).split(separator: UInt8(ascii: "\n"))
            let records: [Record<[String: JSONValue]>] = try lines.map { line in try decoder.decode(Record<[String: JSONValue]>.self, from: line) }
            if let pid {
                let start: Int = records.lastIndex { record in record.pid == pid && record.argv != nil } ?? records.endIndex
                let end: Int = records[start...].firstIndex { record in record.pid == pid && record.exit != nil }.map { end in end + 1 } ?? records.endIndex
                for index: Int in start..<end where records[index].pid == pid { try FileDescriptor.standardOutput.writeAll(lines[index] + [UInt8(ascii: "\n")]) }
                return
            }
            let runs: [Record<[String: JSONValue]>] = records.reduce(into: (runs: [Record<[String: JSONValue]>](), active: [pid_t: Int]())) { folded, record in
                switch (record.argv, folded.active[record.pid]) {
                    case (.some, _) where record.session == variables[sessionVariable]:
                        folded.active[record.pid] = folded.runs.endIndex
                        folded.runs.append(record)
                    case (.some, _): folded.active[record.pid] = nil
                    case (.none, .some(let index)):
                        folded.runs[index].line = (folded.runs[index].line ?? [:]).merging(record.line ?? [:]) { _, next in next }
                        folded.runs[index].exit = record.exit
                        folded.active[record.pid] = record.exit == nil ? index : nil
                    case (.none, .none): break
                }
            }
            .runs
            for run: Record<[String: JSONValue]> in runs { try FileDescriptor.standardOutput.writeAll(encoder.encodeLine(run)) }
        } catch {
            let failure: ApplicationFailure = error as? ApplicationFailure ?? ApplicationFailure(subject: logVariable, reason: .host(error as NSError))
            try FileDescriptor.standardOutput.writeAll(encoder.encodeLine([program: Event.fail(failure)]))
            WatchApps.exit(withError: ExitCode.failure)
        }
    }
}

private struct Serve: AsyncParsableCommand {
    static let configuration: CommandConfiguration = .init(abstract: "Serves ApplicationServer requests as JSON lines over standard input and output, under its parent's privacy grants")

    func run() async {
        let definitions = ScriptingDefinitions()
        let calls = DispatchQueue(label: "\(program).calls", attributes: .concurrent)
        let output = DispatchSerialQueue(label: "\(program).output")
        await Run.execute(output: nil, accessibility: false) { run throws(ApplicationFailure) in
            do throws(EffectRPCFailure) {
                try await serve(
                    reading: FileHandle.standardInput.bytes.jsonLines,
                    writing: { line in _ = try await withTaskExecutorPreference(output) { try FileDescriptor.standardOutput.writeAll(line) } },
                    dropped: { failure in Logger(for: Self.self).debug("Dropped \(failure, privacy: .public)") },
                ) { (request: Request) in
                    let response: Response = try await withTaskExecutorPreference(calls) {
                        try await request.perform(admitting: { NSPredicate(value: true) }, refusing: { _ in [] }, definitions: definitions)
                    }
                    return if case .launch = request, case .stream(let steps) = response { .stream(run.recording(steps)) } else { response }
                }
            } catch {
                throw ApplicationFailure(subject: "connection", reason: .host(error as NSError))
            }
        }
    }
}
