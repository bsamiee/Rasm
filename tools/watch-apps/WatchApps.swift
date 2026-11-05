import AppKit
import ApplicationServer
import Applications
import ArgumentParser
import Carbon.HIToolbox
import CasePaths
import CurrentProcess
import Encoding
import JSONRPC
import OrderedCollections
import Processes
import Results
import ScreenCaptureKit
import Subprocess
import Synchronization
import System
import UniformTypeIdentifiers
import os

// --- [CONSTANTS] -----------------------------------------------------------------------

let captures: URL = URL.temporaryDirectory.appending(component: ProcessInfo.processInfo.processName, directoryHint: .isDirectory)
let logVariable: String = "WATCH_APPS_LOG"
let sessionVariable: String = "CLAUDE_CODE_SESSION_ID"
let lineLimit: Int = 500
let appSwitches: KeyValuePairs<String, (added: String, unless: Set<String>)> = [
    "com.mcneel.rhinoceros": ("-nosplash", []), "org.blenderfoundation.blender": ("--no-window-focus", ["-b", "--background"]),
]

// --- [MODELS] --------------------------------------------------------------------------

struct Ending: Codable, Sendable {
    var status: Int32?
    var signal: Int32?
}

struct Record<Content> {
    let at: Date
    let pid: pid_t
    var session: String?
    var argv: [String]?
    var line: Content?
    var exit: Ending?
}

extension Record {
    init(session: String? = nil, argv: [String]? = nil, line: Content? = nil, exit: Ending? = nil) {
        self.init(at: .now, pid: getpid(), session: session, argv: argv, line: line, exit: exit)
    }
}

extension Record: Encodable where Content: Encodable {}

extension Record: Decodable where Content: Decodable {}

struct Screenshot: Encodable {
    let file: String
    let number: CGWindowID
}

struct Outline: Encodable {
    struct Row: Encodable {
        let kind: String
        let number: CGWindowID
        var options: [String]
        var text: [String]
        let title: String?
    }

    let active: Bool
    var asking: [Row]
    let menus: [String]
    let process: ProcessStatus?
    let status: String
    var windows: [CGWindowID]
}

extension Outline {
    init(_ running: InstanceState.Running) {
        self.init(
            active: running.active,
            asking: running.asking.map(Row.init),
            menus: running.menus.filter { menu in menu.role == kAXMenuRole }.compactMap(\.title),
            process: running.status[case: \.unresponsive] == .stopped ? .stopped : nil,
            status: running.status.name,
            windows: running.windows.filter { window in window.presented && !window.asking }.map(\.number),
        )
    }

    func fitted(measuring length: (Self) -> Int) -> Self {
        var fitted: Self = self
        while case let excess = length(fitted) - lineLimit, excess > 0 {
            if let row: Int = fitted.asking.lastIndex(where: { row in !row.text.isEmpty }), let last: String = fitted.asking[row].text.popLast() {
                if last.count > excess { fitted.asking[row].text.append(String(last.dropLast(excess))) }
            } else if let row: Int = fitted.asking.lastIndex(where: { row in !row.options.isEmpty }) {
                fitted.asking[row].options.removeLast()
            } else if fitted.windows.popLast() == nil {
                break
            }
        }
        return fitted
    }
}

extension Outline.Row {
    init(_ window: InstanceState.Window) {
        let element: Element? = window.description[case: \.element]
        self.init(
            kind: element?.kind.rawValue ?? "blocking",
            number: window.number,
            options: element?.answers?.buttons.compactMap(\.label) ?? [],
            text: element?.detail?.text ?? [],
            title: element?.title ?? window.title,
        )
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

final class Run: Sendable {
    private let encoder: JSONEncoder = .lines
    private let output: FileDescriptor
    private let log: FileDescriptor?
    private let held: Mutex<(status: ExitCode, shown: [pid_t: Data])> = Mutex((.success, [:]))

    init(output: FileDescriptor = .standardOutput) {
        let opening: Result<FileDescriptor, ApplicationFailure>? = ProcessInfo.processInfo.environment[logVariable].map { path in
            Result { () throws(ApplicationFailure) in
                do { return try FileDescriptor.open(FilePath(path), .writeOnly, options: [.append, .create], permissions: [.ownerReadWrite, .groupRead, .otherRead]) } catch {
                    throw ApplicationFailure(subject: path, reason: .host(error as NSError))
                }
            }
        }
        self.output = output
        log = opening?[case: \.success]
        append(Record<JSONValue>(session: ProcessInfo.processInfo.environment[sessionVariable], argv: CommandLine.arguments))
        if case .failure(let failure) = opening { fail(ProcessInfo.processInfo.processName, failure) }
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
                try output.writeAll(line)
                return true
            } catch Errno.brokenPipe {
                return false
            } catch {
                WatchApps.exit(withError: error)
            }
        }
        if !open { finish() }
    }

    func emit(_ key: String, _ value: some Encodable) { write(encoded([key: value]), logging: [key: value]) }

    func fail(_ key: String, _ failure: some Encodable) {
        fail()
        emit(key, ["fail": failure])
    }

    func fail() { held.withLock { held in if held.status.isSuccess { held.status = .failure } } }

    func set(_ status: ExitCode) { held.withLock { held in held.status = status } }

    func show(_ instance: NSRunningApplication, _ state: InstanceState, whole: Bool = false) {
        let (key, pid): (String, String) = (instance.key, String(instance.processIdentifier))
        let line: Data =
            if case .running(let running) = state, !whole {
                encoded([key: [pid: ["running": Outline(running).fitted { outline in encoded([key: [pid: ["running": outline]]]).count }]]])
            } else {
                encoded([key: [pid: state]])
            }
        guard held.withLock({ held in held.shown.updateValue(line, forKey: instance.processIdentifier) }) != line else { return }
        write(line, logging: [key: [pid: state]])
    }

    func follow(_ states: some AsyncSequence<OrderedDictionary<NSRunningApplication, InstanceState>, Never>) async {
        var last: OrderedDictionary<NSRunningApplication, InstanceState> = [:]
        for await next: OrderedDictionary<NSRunningApplication, InstanceState> in states {
            for (instance, state): (NSRunningApplication, InstanceState) in next { show(instance, state) }
            for (instance, state): (NSRunningApplication, InstanceState) in last where next[instance] == nil && !state.is(\.exited) { emit(instance.key, [String: String]()) }
            last = next
        }
    }

    func recordingLaunches(from url: URL, in steps: some AsyncSequence<any Encodable & Sendable, Never> & Sendable) -> some AsyncSequence<any Encodable & Sendable, Never> & Sendable {
        steps.map { step in
            guard case .launched(let launched)? = step as? Launching else { return step }
            let pid: String = String(launched.pid)
            self.held.withLock { _ in
                self.append(Record<JSONValue>(session: ProcessInfo.processInfo.environment[sessionVariable], argv: [ProcessInfo.processInfo.processName, "launch", url.path(percentEncoded: false)]))
                self.append(Record(line: [NSRunningApplication(processIdentifier: launched.pid)?.key ?? pid: [pid: ["launched": ["created": launched.created]]]]))
                self.append(Record<JSONValue>(exit: Ending(status: 0)))
            }
            return step
        }
    }

    func finish(signal: Int32? = nil) -> Never {
        let status: ExitCode = held.withLock { held in held.status }
        append(Record<JSONValue>(exit: signal.map { number in Ending(signal: number) } ?? Ending(status: status.rawValue)))
        WatchApps.exit(withError: status)
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension NSRunningApplication {
    var key: String { bundleIdentifier ?? String(processIdentifier) }
}

extension JSONEncoder {
    static var lines: JSONEncoder {
        let encoder: JSONEncoder = JSONEncoder()
        encoder.outputFormatting = [.sortedKeys, .withoutEscapingSlashes]
        encoder.dateEncodingStrategy = .custom { date, encoder in
            var instant: any SingleValueEncodingContainer = encoder.singleValueContainer()
            try instant.encode(Date.ISO8601FormatStyle(includingFractionalSeconds: true).format(date))
        }
        return encoder
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

@main
struct WatchApps: AsyncParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(
        abstract: "Observes and drives installed applications in background. Commands fail after a fail line",
        subcommands: [Watch.self, Launch.self, Read.self, Capture.self, Perform.self, SetValue.self, Key.self, Quit.self, Log.self, Serve.self],
    )

    static func main() async {
        do {
            var command: any ParsableCommand = try parseAsRoot()
            if var asynchronous: any AsyncParsableCommand = command as? any AsyncParsableCommand { try await asynchronous.run() } else { try command.run() }
        } catch let error where exitCode(for: error) == .success {
            exit(withError: error)
        } catch {
            let run: Run = Run()
            run.fail(ProcessInfo.processInfo.processName, ["message": message(for: error), "reason": "malformed", "subject": CommandLine.arguments.dropFirst().joined(separator: " ")])
            run.finish()
        }
    }

    static func driving(_ work: @escaping @Sendable (Run) async throws(ApplicationFailure) -> Void) async {
        signal(SIGPIPE, SIG_IGN)
        let run: Run = Run()
        await withDiscardingTaskGroup { group in
            group.addTask {
                do throws(ApplicationFailure) {
                    try checkAccessibilityTrust()
                    try await work(run)
                } catch {
                    run.fail(ProcessInfo.processInfo.processName, error)
                }
                run.finish()
            }
            for await ending: Int32? in processEndings() { run.finish(signal: ending) }
        }
    }

    static func installed(_ names: [String], _ run: Run) async -> OrderedDictionary<String, [Installation]> {
        var found: OrderedDictionary<String, [Installation]> = [:]
        for name: String in OrderedSet(names) {
            do throws(ApplicationFailure) { found[name] = try await Installation.installed(named: name) } catch { run.fail(name, error) }
        }
        return found
    }

    static func act(_ action: ElementAction, keyedBy key: String) async {
        await driving { run in
            do throws(ApplicationFailure) {
                let owner: pid_t = try await action.owner()
                guard let instance: NSRunningApplication = NSRunningApplication(processIdentifier: owner) else { throw ApplicationFailure(subject: String(owner), reason: .noProcess) }
                run.show(instance, try await instance.performing(action))
            } catch {
                run.fail(key, error)
            }
        }
    }
}

struct Running: ParsableArguments {
    @Argument(help: "Running application paths, bundle identifiers, or display names. Names ignore case and diacritics")
    var names: [String] = []

    @Option(help: "One running instance's process ID, in place of names")
    var pid: pid_t?

    func validate() throws(ValidationError) {
        guard names.isEmpty != (pid == nil) else { throw ValidationError("Name running applications or pass --pid alone") }
    }

    func each(_ run: Run, _ body: @escaping @Sendable (NSRunningApplication) async throws(ApplicationFailure) -> Void) async {
        let instances: OrderedSet<NSRunningApplication> =
            if let pid {
                OrderedSet([NSRunningApplication(processIdentifier: pid)].compactMap(\.self))
            } else {
                OrderedSet(
                    names.flatMap { name in
                        let found: [NSRunningApplication] = NSWorkspace.shared.instances(named: name)
                        if found.isEmpty { run.emit(name, [String: String]()) }
                        return found
                    }
                )
            }
        if let pid, instances.isEmpty { run.fail(String(pid), ApplicationFailure(subject: String(pid), reason: .noProcess)) }
        await withDiscardingTaskGroup { group in
            for instance: NSRunningApplication in instances {
                group.addTask {
                    do throws(ApplicationFailure) { try await body(instance) } catch { run.fail(instance.key, error) }
                }
            }
        }
    }
}

struct Watch: AsyncParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(abstract: "Prints one outline line per changed instance, system prompts naming it, and system pressure off normal")

    @Argument(help: "Installed application paths, bundle identifiers, or display names. Names ignore case and diacritics")
    var names: [String]

    func run() async {
        await WatchApps.driving { run in
            let installations: [Installation] = await WatchApps.installed(names, run).values.flatMap(\.self)
            guard !installations.isEmpty else { return }
            for identifier: String in OrderedSet(installations.map(\.identifier)) where NSWorkspace.shared.instances(of: { instance in instance.bundleIdentifier == identifier }).isEmpty {
                run.emit(identifier, [String: String]())
            }
            await withDiscardingTaskGroup { group in
                group.addTask {
                    var calm: Bool = true
                    for await pressure: Result<SystemPressure, Errno> in SystemPressure.changes() {
                        switch pressure {
                            case .success(let pressure):
                                let now: Bool = pressure.memory.isDisjoint(with: [.warning, .critical]) && pressure.thermal == .nominal
                                if !(calm && now) { run.emit("system", pressure) }
                                calm = now
                            case .failure(let error): run.fail("system", ApplicationFailure(subject: "system", reason: .system(error)))
                        }
                    }
                }
                group.addTask { await run.follow(NSWorkspace.shared.instanceStates(of: { instance in installations.contains { installation in installation.runs(instance) } })) }
                group.addTask { await run.follow(NSWorkspace.shared.systemPrompts(naming: installations.map(\.name))) }
            }
        }
    }
}

struct Launch: AsyncParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(
        abstract: "Opens applications or documents without activation, reusing a running instance. Prints until settled, asking, or exit with --wait"
    )

    @Argument(help: "Installed application paths, bundle identifiers, or display names. Names ignore case and diacritics")
    var names: [String] = []

    @Flag(help: "Opens another instance when one runs, for applications that support several")
    var new: Bool = false

    @Flag(help: "Observes one named application until it exits and returns its exit status, or 128 plus its signal")
    var wait: Bool = false

    @Option(help: ArgumentHelp("File to open, one per flag", valueName: "path"), transform: { path in URL(filePath: path) })
    var document: [URL] = []

    @Option(parsing: .unconditionalSingleValue, help: ArgumentHelp("Command-line argument of new instance, one per flag", valueName: "argument"))
    var arguments: [String] = []

    @Option(
        help: "NAME=VALUE environment variable of new instance, one per flag",
        transform: { entry in
            guard let split: String.Index = entry.firstIndex(of: "=") else { throw ValidationError("\(entry) lacks =, pass NAME=VALUE") }
            return (String(entry[..<split]), String(entry[entry.index(after: split)...]))
        },
    )
    var environment: [(String, String)] = []

    func validate() throws(ValidationError) {
        guard !names.isEmpty || !document.isEmpty else { throw ValidationError("Name an application or pass --document") }
        guard names.count < 2 || document.isEmpty else { throw ValidationError("--document belongs to one application") }
        guard !wait || names.count == 1 else { throw ValidationError("--wait belongs to one named application") }
    }

    func run() async {
        let (wait, new, arguments): (Bool, Bool, [String]) = (wait, new, arguments)
        let variables: [String: String] = Dictionary(environment) { _, last in last }
        let (names, document): ([String], [URL]) = (names, document)
        await WatchApps.driving { run in
            if wait { run.set(.failure) }
            var launches: OrderedDictionary<Installation, [URL]> = [:]
            for file: URL in names.isEmpty ? document : [] {
                do throws(ApplicationFailure) { launches[try Installation(opening: file), default: []].append(file) } catch { run.fail(file.path(percentEncoded: false), error) }
            }
            for (name, found): (String, [Installation]) in await WatchApps.installed(names, run) {
                do throws(ApplicationFailure) { launches[try Installation.newest(of: name, among: found)] = document } catch { run.fail(name, error) }
            }
            await withDiscardingTaskGroup { group in
                for (installation, documents): (Installation, [URL]) in launches {
                    let host: ArraySlice<String> = arguments.prefix { argument in argument != "--" }
                    let switches: [String] = appSwitches.filter { prefix, row in installation.identifier.hasPrefix(prefix) && row.unless.union([row.added]).isDisjoint(with: host) }.map(\.value.added)
                    group.addTask {
                        var last: RunningInstance?
                        for await step: Launching in NSWorkspace.shared.launching(installation, opening: documents, arguments: switches + arguments, environment: variables, newInstance: new) {
                            switch step {
                                case .launched(let launched): run.emit(installation.identifier, [String(launched.pid): ["launched": ["created": launched.created]]])
                                case .state(let state):
                                    run.show(state.instance, state.state)
                                    last = state
                                case .prompts(let prompts): for prompt: RunningInstance in prompts { run.show(prompt.instance, prompt.state) }
                                case .failed(let failure): run.fail(installation.identifier, failure)
                            }
                        }
                        guard wait, let last else { return }
                        var ended: InstanceState = last.state
                        if !ended.is(\.exited) {
                            for await state: InstanceState in last.instance.states() {
                                run.show(last.instance, state)
                                ended = state
                            }
                        }
                        guard case .exited(.success(let exit), _) = ended else { return }
                        switch exit.status {
                            case .exited(let code): run.set(ExitCode(code))
                            case .signaled(let signal): run.set(ExitCode(128 + signal))
                        }
                    }
                }
            }
        }
    }
}

struct Read: AsyncParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(abstract: "Prints each instance's whole state with element ids once, --window walks one window")

    @OptionGroup var running: Running

    @Option(help: "Window number from a line")
    var window: CGWindowID?

    func run() async {
        await WatchApps.driving { run in
            await running.each(run) { instance in run.show(instance, await instance.reading(window: window), whole: true) }
        }
    }
}

struct Capture: AsyncParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(abstract: "Writes one PNG per presented window, or of --window, with no activation. Prints one line per file")

    @OptionGroup var running: Running

    @Option(help: "Window number from a line")
    var window: CGWindowID?

    @Option(
        help: ArgumentHelp("Longest side in pixels, smaller captures keep native size", valueName: "pixels"),
        transform: { pixels in
            guard let side: Int = Int(pixels), side > 0 else { throw ValidationError("\(pixels) is not a positive integer") }
            return side
        },
    )
    var longestSide: Int?

    func run() async {
        await WatchApps.driving { run throws(ApplicationFailure) in
            do { try FileManager.default.createDirectory(at: captures, withIntermediateDirectories: true) } catch {
                throw ApplicationFailure(subject: captures.path(percentEncoded: false), reason: .host(error as NSError))
            }
            await running.each(run) { instance throws(ApplicationFailure) in
                let shots: [CGWindowID: Result<SCScreenshotOutput, ApplicationFailure>] = try await instance.capturing(window: window, longestSide: longestSide)
                guard !shots.isEmpty else { throw ApplicationFailure(subject: instance.key, reason: .noWindow) }
                for (number, shot): (CGWindowID, Result<SCScreenshotOutput, ApplicationFailure>) in shots.sorted(by: { first, second in first.key < second.key }) {
                    do throws(ApplicationFailure) {
                        let file: URL = captures.appending(component: "\(instance.key)-\(instance.processIdentifier)-\(number)").appendingPathExtension(for: .png)
                        try shot.get().write(to: file)
                        run.emit(instance.key, [String(instance.processIdentifier): ["captured": Screenshot(file: file.path(percentEncoded: false), number: number)]])
                    } catch {
                        run.fail(instance.key, error)
                    }
                }
            }
        }
    }
}

struct ElementOperand: ParsableArguments {
    @Argument(
        help: "Element ID from a read line",
        transform: { id in
            guard let element: Element.ID = Element.ID(base64Encoded: id) else { throw ValidationError("\(id) is not base64") }
            return element
        },
    )
    var id: Element.ID
}

struct Perform: AsyncParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(abstract: "Performs one Accessibility action on an element. Prints its instance's next state")

    @OptionGroup var element: ElementOperand

    @Argument(help: "Accessibility action from element actions")
    var action: String

    func run() async { await WatchApps.act(.perform(element: element.id, action: action), keyedBy: element.id.description) }
}

struct SetValue: AsyncParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(commandName: "set", abstract: "Sets an element's \(kAXValueAttribute) to text. Prints its instance's next state")

    @OptionGroup var element: ElementOperand

    @Argument(help: "Text for element \(kAXValueAttribute)")
    var value: String

    func run() async { await WatchApps.act(.set(element: element.id, value: value), keyedBy: element.id.description) }
}

struct Key: AsyncParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(abstract: "Posts one key to a process holding a blocking row, frontmost and cursor unchanged. Prints its next state")

    @Option(help: "Process ID from a line whose asking row reads blocking")
    var pid: pid_t

    @Argument(help: "Virtual key code, \(kVK_Return) Return, \(kVK_Escape) Escape")
    var code: CGKeyCode

    func run() async { await WatchApps.act(.key(process: pid, code: code), keyedBy: String(pid)) }
}

struct Quit: AsyncParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(abstract: "Sends each running instance the quit event. Prints states and exits 1 unless each instance exited")

    @OptionGroup var running: Running

    @Flag(help: "Kills each instance without asking. Use when no answer or quit frees it")
    var force: Bool = false

    func run() async {
        await WatchApps.driving { run in
            await running.each(run) { instance throws(ApplicationFailure) in
                var last: InstanceState?
                for await step: Result<InstanceState, ApplicationFailure> in instance.quitting(force: force) {
                    last = try step.get()
                    if let last { run.show(instance, last) }
                }
                if last?.is(\.exited) != true { run.fail() }
            }
        }
    }
}

struct Log: ParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(abstract: "Prints each session run's last line per key. --pid prints every record of one run")

    @Option(help: "Process ID of one watch-apps run")
    var pid: pid_t?

    func run() throws {
        let encoder: JSONEncoder = .lines
        let decoder: JSONDecoder = JSONDecoder()
        decoder.dateDecodingStrategy = .custom { decoder in try Date.ISO8601FormatStyle(includingFractionalSeconds: true).parse(decoder.singleValueContainer().decode(String.self)) }
        do {
            guard let path: String = ProcessInfo.processInfo.environment[logVariable] else {
                throw ApplicationFailure(subject: logVariable, reason: .unmatched(Unmatched(subject: "environment variable", query: logVariable, candidates: [])))
            }
            let lines: [Data] = try Data(contentsOf: URL(filePath: path)).split(separator: UInt8(ascii: "\n"))
            let records: [Record<[String: JSONValue]>] = try lines.map { line in try decoder.decode(Record<[String: JSONValue]>.self, from: line) }
            let printed: [Data]
            if let pid {
                let start: Int = records.lastIndex { record in record.pid == pid && record.argv != nil } ?? records.endIndex
                let end: Int = records[start...].firstIndex { record in record.pid == pid && record.exit != nil }.map { end in end + 1 } ?? records.endIndex
                printed = (start..<end).filter { index in records[index].pid == pid }.map { index in lines[index] + [UInt8(ascii: "\n")] }
            } else {
                let session: String? = ProcessInfo.processInfo.environment[sessionVariable]
                printed = try records.reduce(into: (runs: [Record<[String: JSONValue]>](), active: [pid_t: Int]())) { folded, record in
                    switch (record.argv, folded.active[record.pid]) {
                        case (.some, _) where record.session == session:
                            folded.active[record.pid] = folded.runs.endIndex
                            folded.runs.append(record)
                        case (.some, _): folded.active[record.pid] = nil
                        case (.none, .some(let run)):
                            folded.runs[run].line = (folded.runs[run].line ?? [:]).merging(record.line ?? [:]) { _, next in next }
                            folded.runs[run].exit = record.exit
                            if record.exit != nil { folded.active[record.pid] = nil }
                        case (.none, .none): break
                    }
                }
                .runs.map(encoder.encodeLine)
            }
            for line: Data in printed { try FileHandle.standardOutput.write(contentsOf: line) }
        } catch {
            let failure: ApplicationFailure = error as? ApplicationFailure ?? ApplicationFailure(subject: logVariable, reason: .host(error as NSError))
            try FileHandle.standardOutput.write(contentsOf: encoder.encodeLine([ProcessInfo.processInfo.processName: ["fail": failure]]))
            WatchApps.exit(withError: ExitCode.failure)
        }
    }
}

struct Serve: AsyncParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(abstract: "Serves the ApplicationServer wire over standard input and output, holding its parent's grants")

    func run() async throws {
        let run: Run = Run(output: .standardError)
        let logger: Logger = Logger(for: Self.self)
        let definitions: ScriptingDefinitions = ScriptingDefinitions()
        let calls: DispatchQueue = DispatchQueue(label: "watch-apps.calls", attributes: .concurrent)
        let output: DispatchSerialQueue = DispatchSerialQueue(label: "watch-apps.output")
        nonisolated(unsafe) let everything: NSPredicate = NSPredicate(value: true)
        try await serve(
            reading: FileHandle.standardInput.bytes.jsonLines,
            writing: { line in try await withTaskExecutorPreference(output) { try FileHandle.standardOutput.write(contentsOf: line) } },
            dropped: { failure in logger.debug("Dropped \(failure, privacy: .public)") },
            { (request: Request) in
                let response: Response = try await withTaskExecutorPreference(calls) { try await request.perform(admitting: unsafe everything, refusing: { _ in [] }, definitions: definitions) }
                guard case .launch(let url, _, _, _, _) = request, case .stream(let steps) = response else { return response }
                return .stream(run.recordingLaunches(from: url, in: steps))
            },
        )
        run.finish()
    }
}
