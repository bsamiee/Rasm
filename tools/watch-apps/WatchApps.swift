import AppKit
import Applications
import ArgumentParser
import CasePaths
import Encoding
import OrderedCollections
import Processes
import Results
import ScreenCaptureKit
import Synchronization
import System
import UniformTypeIdentifiers

// --- [CONSTANTS] -----------------------------------------------------------------------

let captures: URL = URL.temporaryDirectory.appending(component: ProcessInfo.processInfo.processName, directoryHint: .isDirectory)
let logVariable: String = "WATCH_APPS_LOG"
let sessionVariable: String = "CLAUDE_CODE_SESSION_ID"

// --- [MODELS] --------------------------------------------------------------------------

struct Screenshot: Encodable, Sendable {
    let file: String
    let windows: [CGWindowID]
}

@CasePathable
enum Line: Encodable, Sendable {
    case states(OrderedDictionary<NSRunningApplication, InstanceState>)
    case captured(OrderedDictionary<NSRunningApplication, Screenshot>)
    case pressure(SystemPressure)
    case fail(ApplicationFailure)

    func encode(to encoder: any Encoder) throws {
        var line: any SingleValueEncodingContainer = encoder.singleValueContainer()
        switch self {
            case .states(let states): try line.encode(states.byProcess)
            case .captured(let captured): try line.encode(captured.byProcess)
            case .pressure(let pressure): try line.encode(pressure)
            case .fail(let failure): try line.encode(["fail": failure])
        }
    }
}

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

extension Record: Encodable where Content: Encodable {}

extension Record: Decodable where Content: Decodable {}

struct Run: Encodable {
    let pid: pid_t
    let at: Date
    let argv: [String]
    var exit: Ending?
    var last: [String: JSONValue] = [:]
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension OrderedDictionary where Key == NSRunningApplication {
    var byProcess: [Int: Value] {
        Dictionary(uniqueKeysWithValues: map { instance, value in (Int(instance.processIdentifier), value) })
    }
}

extension NSRunningApplication {
    var key: String { bundleIdentifier ?? String(processIdentifier) }
}

extension Observation {
    var line: (key: String, line: Line) {
        switch self {
            case .instance(let instance, let state): (instance.key, .states([instance: state]))
            case .states(let key, let states): (key, .states(states))
            case .failure(let key, let failure): (key, .fail(failure))
        }
    }
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

extension JSONValue {
    var outlined: JSONValue {
        switch self {
            case .object(let fields):
                .object(
                    fields.reduce(into: [:]) { outline, field in
                        switch field {
                            case ("usage", _): break
                            case ("status", .object(let held)) where held.count == 1: outline[field.key] = held.keys.first.map(Self.string)
                            case (let key, let value): outline[key] = value.outlined
                        }
                    }
                )
            case .array(let values): .array(values.map(\.outlined))
            case .null, .bool, .number, .string: self
        }
    }
}

extension JSONDecoder {
    static var lines: JSONDecoder {
        let decoder: JSONDecoder = JSONDecoder()
        decoder.dateDecodingStrategy = .custom { decoder in
            try Date.ISO8601FormatStyle(includingFractionalSeconds: true).parse(decoder.singleValueContainer().decode(String.self))
        }
        return decoder
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

@main
struct WatchApps: AsyncParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(
        abstract: "Observes and drives installed applications in background. Commands exit 1 after a fail line",
        subcommands: [Watch.self, Launch.self, Show.self, Read.self, Capture.self, Perform.self, SetValue.self, Quit.self, Log.self],
    )

    static func main() async {
        do {
            var command: any ParsableCommand = try parseAsRoot()
            if var asynchronous: any AsyncParsableCommand = command as? any AsyncParsableCommand { try await asynchronous.run() } else { try command.run() }
        } catch let error where exitCode(for: error) == .success {
            exit(withError: error)
        } catch {
            let failure: ApplicationFailure = ApplicationFailure(subject: CommandLine.arguments.dropFirst().joined(separator: " "), reason: .malformed(message(for: error)))
            await run(trusted: false) { output in output.yield((ProcessInfo.processInfo.processName, .fail(failure))) }
        }
    }

    @MainActor
    static func run(trusted: Bool = true, _ work: @escaping @MainActor (AsyncStream<(key: String, line: Line)>.Continuation) async -> Void) async -> Never {
        let (lines, output): (AsyncStream<(key: String, line: Line)>, AsyncStream<(key: String, line: Line)>.Continuation) = AsyncStream.makeStream()
        let received: Mutex<Int32?> = Mutex(nil)
        signal(SIGPIPE, SIG_IGN)
        let signals: [any DispatchSourceSignal] = [SIGTERM, SIGINT, SIGHUP].map { number in
            signal(number, SIG_IGN)
            let source: any DispatchSourceSignal = DispatchSource.makeSignalSource(signal: number, queue: .main)
            source.setEventHandler {
                received.withLock { held in held = number }
                output.finish()
            }
            source.activate()
            return source
        }
        let parent: any DispatchSourceProcess = DispatchSource.makeProcessSource(identifier: getppid(), eventMask: .exit, queue: .main)
        parent.setEventHandler { output.finish() }
        parent.activate()
        let encoder: JSONEncoder = JSONEncoder.lines
        let process: pid_t = getpid()
        let log: Result<FileDescriptor, ApplicationFailure>? = ProcessInfo.processInfo.environment[logVariable].map { path in
            Result { () throws(ApplicationFailure) in
                do { return try FileDescriptor.open(FilePath(path), .writeOnly, options: [.append, .create], permissions: [.ownerReadWrite, .groupRead, .otherRead]) } catch {
                    throw ApplicationFailure(subject: path, reason: .host(error))
                }
            }
        }
        let record: (Record<[String: Line]>) throws -> Void = { record in try log?[case: \.success]?.writeAll(encoder.encodeLine(record)) }
        let written: Result<Bool, any Error> = await { @MainActor () async -> Result<Bool, any Error> in
            async let _: Void = { @MainActor in
                if case .failure(let failure) = log { output.yield((ProcessInfo.processInfo.processName, .fail(failure))) }
                do throws(ApplicationFailure) {
                    if trusted { try checkAccessibilityTrust() }
                    await work(output)
                } catch {
                    output.yield((ProcessInfo.processInfo.processName, .fail(error)))
                }
                output.finish()
            }()
            return await Result { () async throws -> Bool in
                try record(Record(at: .now, pid: process, session: ProcessInfo.processInfo.environment[sessionVariable], argv: CommandLine.arguments))
                var failed: Bool = false
                for await entry: (key: String, line: Line) in lines {
                    failed = failed || entry.line.is(\.fail)
                    try record(Record(at: .now, pid: process, line: [entry.key: entry.line]))
                    do { try FileDescriptor.standardOutput.writeAll(encoder.encodeLine([entry.key: entry.line])) } catch .brokenPipe { break }
                }
                return failed
            }
        }()
        let ending: Ending = received.withLock { held in
            held.map { number in Ending(signal: number) } ?? Ending(status: written[case: \.success].map { failed in failed ? ExitCode.failure.rawValue : ExitCode.success.rawValue })
        }
        let closed: Result<Void, any Error> = Result { try record(Record(at: .now, pid: process, exit: ending)) }
        withExtendedLifetime((parent, signals)) {
            switch (written, closed) {
                case (.failure(let error), _), (.success, .failure(let error)): exit(withError: error)
                case (.success(let failed), .success): exit(withError: failed ? ExitCode.failure : nil)
            }
        }
    }

    @MainActor
    static func run(
        _ names: [String],
        _ work: @escaping @MainActor (OrderedDictionary<String, [Installation]>, AsyncStream<(key: String, line: Line)>.Continuation) async -> Void,
    ) async -> Never {
        await run { output in
            var found: OrderedDictionary<String, [Installation]> = [:]
            for name: String in OrderedSet(names) {
                do throws(ApplicationFailure) { found[name] = try await Installation.installed(named: name) } catch { output.yield((name, .fail(error))) }
            }
            await work(found, output)
        }
    }
}

struct Installed: ParsableArguments {
    @Argument(help: "Bundle identifiers or display names of installed applications, case and diacritics ignored")
    var names: [String]
}

struct Running: ParsableArguments {
    @Argument(help: "Bundle identifiers, display names, or bundle paths of running applications, case and diacritics ignored")
    var names: [String] = []

    @Option(help: "Process ID of one running instance, in place of names")
    var pid: pid_t?

    func validate() throws(ValidationError) {
        guard names.isEmpty != (pid == nil) else { throw ValidationError("Name running applications or pass --pid alone") }
    }

    @MainActor
    func resolved(_ output: AsyncStream<(key: String, line: Line)>.Continuation) -> OrderedDictionary<String, [NSRunningApplication]> {
        guard let pid else {
            return names.reduce(into: OrderedDictionary()) { resolved, name in
                let running: [NSRunningApplication] = NSWorkspace.shared.instances(named: name)
                if running.isEmpty { resolved[name] = [] }
                for instance: NSRunningApplication in running { resolved[instance.key, default: []].append(instance) }
            }
        }
        guard let instance: NSRunningApplication = NSRunningApplication(processIdentifier: pid) else {
            output.yield((String(pid), .fail(ApplicationFailure(subject: String(pid), reason: .noProcess))))
            return [:]
        }
        return [instance.key: [instance]]
    }

    @MainActor
    func each<Value: Sendable>(
        output: AsyncStream<(key: String, line: Line)>.Continuation,
        _ read: @escaping @Sendable (NSRunningApplication) async -> Result<Value, ApplicationFailure>,
        as line: (OrderedDictionary<NSRunningApplication, Value>) -> Line?,
    ) async {
        let resolved: OrderedDictionary<String, [NSRunningApplication]> = resolved(output)
        let reads: [NSRunningApplication: Result<Value, ApplicationFailure>] = await withTaskGroup { group in
            for instance: NSRunningApplication in resolved.values.joined() { group.addTask { (instance, await read(instance)) } }
            var reads: [NSRunningApplication: Result<Value, ApplicationFailure>] = [:]
            for await (instance, read): (NSRunningApplication, Result<Value, ApplicationFailure>) in group { reads[instance] = read }
            return reads
        }
        for (key, instances): (String, [NSRunningApplication]) in resolved {
            let values: OrderedDictionary<NSRunningApplication, Value> = OrderedDictionary(
                uniqueKeysWithValues: instances.compactMap { instance in reads[instance]?[case: \.success].map { value in (instance, value) } }
            )
            for failure: ApplicationFailure in instances.compactMap({ instance in reads[instance]?[case: \.failure] }) { output.yield((key, .fail(failure))) }
            if let line: Line = line(values), !values.isEmpty || instances.isEmpty { output.yield((key, line)) }
        }
    }
}

struct Watch: AsyncParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(
        abstract: "Prints windows, dialogs requiring an answer, hangs, exits, system prompts naming the applications, and system pressure, at start and on each change"
    )

    @OptionGroup var installed: Installed

    func run() async {
        await WatchApps.run(installed.names) { applications, output in
            guard !applications.isEmpty else { return }
            let observations: AsyncStream<Observation> = NSWorkspace.shared.observed(.watch(OrderedSet(applications.values.joined()).elements))
            await withDiscardingTaskGroup { group in
                group.addTask {
                    for await pressure: Result<SystemPressure, Errno> in SystemPressure.changes() {
                        switch pressure {
                            case .success(let pressure): output.yield(("system", .pressure(pressure)))
                            case .failure(let error): output.yield(("system", .fail(ApplicationFailure(subject: "system", reason: .system(error)))))
                        }
                    }
                }
                group.addTask { for await observation: Observation in observations { output.yield(observation.line) } }
            }
        }
    }
}

struct Launch: AsyncParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(
        abstract:
            "Opens each application, or each document's default application, hidden without activation and prints its state until it responds, exits, or a system prompt naming it shows, then exits leaving the instance running"
    )

    @Argument(help: "Bundle identifiers or display names of installed applications, case and diacritics ignored")
    var names: [String] = []

    @Flag(help: "Opens another instance when one runs")
    var new: Bool = false

    @Option(help: ArgumentHelp("File to open, one per flag", valueName: "path"), transform: { path in URL(filePath: path) })
    var document: [URL] = []

    @Option(parsing: .unconditionalSingleValue, help: ArgumentHelp("Command-line argument of new instance, one per flag", valueName: "argument"))
    var arguments: [String] = []

    @Option(
        help: "NAME=VALUE environment variable of new instance, one per flag",
        transform: { entry in
            guard let split: String.Index = entry.firstIndex(of: "=") else { throw ValidationError("\(entry) lacks =") }
            return (String(entry[..<split]), String(entry[entry.index(after: split)...]))
        },
    )
    var environment: [(String, String)] = []

    func validate() throws(ValidationError) {
        guard !names.isEmpty || !document.isEmpty else { throw ValidationError("Name an application or pass --document") }
        guard names.count < 2 || document.isEmpty else { throw ValidationError("--document belongs to one application") }
    }

    func run() async {
        await WatchApps.run(names) { applications, output in
            var launches: OrderedDictionary<Installation, [URL]> = [:]
            for file: URL in document where names.isEmpty {
                do throws(ApplicationFailure) { launches[try Installation(opening: file), default: []].append(file) } catch { output.yield((file.path(percentEncoded: false), .fail(error))) }
            }
            for (name, releases): (key: String, value: [Installation]) in applications {
                do throws(ApplicationFailure) { launches[try Installation.newest(of: name, among: releases)] = document } catch { output.yield((name, .fail(error))) }
            }
            guard !launches.isEmpty else { return }
            let target: Target = .launch(
                launches.map { installation, documents in (installation, documents) },
                arguments: arguments,
                environment: Dictionary(environment) { _, last in last },
                newInstance: new,
            )
            for await observation: Observation in NSWorkspace.shared.observed(target) { output.yield(observation.line) }
        }
    }
}

struct Show: AsyncParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(
        abstract: "Unhides each running instance without activation so capture reaches its windows, silent unless an instance stays hidden"
    )

    @OptionGroup var running: Running

    func run() async {
        await WatchApps.run { output in
            await running.each(output: output, { instance in await Result { () async throws(ApplicationFailure) in try await instance.setHidden(false) } }, as: { _ in nil })
        }
    }
}

struct Read: AsyncParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(
        abstract: "Prints full state of each running instance once, with its resource usage"
    )

    @OptionGroup var running: Running

    func run() async {
        await WatchApps.run { output in
            await running.each(output: output, { instance in .success(await instance.state(walking: { _ in true })) }, as: Line.states)
        }
    }
}

struct Capture: AsyncParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(
        abstract: "Writes a PNG of each running instance's on-screen windows and prints its file and window numbers"
    )

    @OptionGroup var running: Running

    @Option(help: "CoreGraphics window number from a read or watch line")
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
        await WatchApps.run { output in
            await running.each(output: output, capture, as: Line.captured)
        }
    }

    func capture(_ instance: NSRunningApplication) async -> Result<Screenshot, ApplicationFailure> {
        await Result { () async throws(ApplicationFailure) in
            let file: URL = captures.appending(component: [instance.key, String(instance.processIdentifier), window.map(String.init) ?? "windows"].joined(separator: "-"))
                .appendingPathExtension(for: .png)
            do { try FileManager.default.createDirectory(at: captures, withIntermediateDirectories: true) } catch {
                throw ApplicationFailure(subject: captures.path(percentEncoded: false), reason: .host(error))
            }
            let configuration: SCScreenshotConfiguration = SCScreenshotConfiguration()
            configuration.showsCursor = false
            let captured: (output: SCScreenshotOutput, windows: [CGWindowID]) = try await instance.screenshot(of: window, longestSide: longestSide, configuration: configuration)
            try captured.output.write(to: file)
            return Screenshot(file: file.path(percentEncoded: false), windows: captured.windows)
        }
    }
}

struct ElementID: ParsableArguments {
    @Argument(
        help: "Element ID from read or watch line",
        transform: { id in
            guard let element: Data = Data(base64Encoded: id), !element.isEmpty else { throw ValidationError("\(id) is not base64") }
            return element
        },
    )
    var id: Data

    func run(_ action: ElementAction) async {
        await WatchApps.run { output in
            for await observation: Observation in NSWorkspace.shared.observed(.act(action)) { output.yield(observation.line) }
        }
    }
}

struct Perform: AsyncParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(
        abstract: "Performs one element Accessibility action and prints the state its instance settles in"
    )

    @OptionGroup var element: ElementID

    @Argument(help: "Accessibility action from element actions")
    var action: String

    func run() async {
        await element.run(.perform(element: element.id, action: action))
    }
}

struct SetValue: AsyncParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(
        commandName: "set",
        abstract: "Sets element \(kAXValueAttribute) to text and prints the state its instance settles in, failing on an element without settable \(kAXValueAttribute)",
    )

    @OptionGroup var element: ElementID

    @Argument(help: "Text for element \(kAXValueAttribute)")
    var value: String

    func run() async {
        await element.run(.set(element: element.id, value: value))
    }
}

struct Quit: AsyncParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(
        abstract:
            "Asks each running instance to quit and prints its state until it exits, an element requires an answer, it refuses, or it stops answering, sending nothing while an element requires an answer"
    )

    @OptionGroup var running: Running

    @Flag(help: "Kills each instance without asking, for one no answer or quit frees")
    var force: Bool = false

    func run() async {
        await WatchApps.run { output in
            let resolved: OrderedDictionary<String, [NSRunningApplication]> = running.resolved(output)
            for (key, instances): (String, [NSRunningApplication]) in resolved where instances.isEmpty { output.yield((key, .states([:]))) }
            let instances: [NSRunningApplication] = Array(resolved.values.joined())
            guard !instances.isEmpty else { return }
            for await observation: Observation in NSWorkspace.shared.observed(force ? .kill(instances) : .quit(instances)) { output.yield(observation.line) }
        }
    }
}

struct Log: ParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(
        abstract: "Prints one line per watch-apps run of the current session with its last line per key, or every record of one run"
    )

    @Option(help: "Process ID of one watch-apps run")
    var pid: pid_t?

    func run() throws {
        let encoder: JSONEncoder = JSONEncoder.lines
        let decoder: JSONDecoder = JSONDecoder.lines
        let printed: Result<[Data], ApplicationFailure> = Result { () throws(ApplicationFailure) -> [Data] in
            guard let path: String = ProcessInfo.processInfo.environment[logVariable] else {
                throw ApplicationFailure(subject: logVariable, reason: .unmatched(Unmatched(subject: "environment variable", query: logVariable, candidates: [])))
            }
            let lines: [Data]
            let records: [Record<JSONValue>]
            do {
                lines = try Data(contentsOf: URL(filePath: path)).split(separator: UInt8(ascii: "\n"))
                records = try lines.map { line in try decoder.decode(Record<JSONValue>.self, from: line) }
            } catch {
                throw ApplicationFailure(subject: path, reason: .host(error))
            }
            let output: [Data]
            if let pid {
                let start: Int = records.lastIndex { record in record.pid == pid && record.argv != nil } ?? records.endIndex
                let end: Int = records[start...].firstIndex { record in record.pid == pid && record.exit != nil }.map { end in end + 1 } ?? records.endIndex
                output = (start..<end).filter { index in records[index].pid == pid }.map { index in lines[index] + [UInt8(ascii: "\n")] }
            } else {
                let session: String? = ProcessInfo.processInfo.environment[sessionVariable]
                var runs: [Run] = []
                var active: [pid_t: Int] = [:]
                for record: Record<JSONValue> in records {
                    switch (record.argv, record.line, record.exit, active[record.pid]) {
                        case (.some(let argv), _, _, _) where record.session == session:
                            active[record.pid] = runs.endIndex
                            runs.append(Run(pid: record.pid, at: record.at, argv: argv))
                        case (.some, _, _, _): active[record.pid] = nil
                        case (.none, .some(.object(let line)), _, .some(let run)): runs[run].last.merge(line.mapValues(\.outlined)) { _, next in next }
                        case (.none, _, .some(let exit), .some(let run)):
                            runs[run].exit = exit
                            active[record.pid] = nil
                        case (.none, _, _, _): continue
                    }
                }
                do { output = try runs.map(encoder.encodeLine) } catch { throw ApplicationFailure(subject: path, reason: .host(error)) }
            }
            return output
        }
        switch printed {
            case .success(let lines): for line: Data in lines { try FileHandle.standardOutput.write(contentsOf: line) }
            case .failure(let failure):
                try FileHandle.standardOutput.write(contentsOf: encoder.encodeLine([ProcessInfo.processInfo.processName: Line.fail(failure)]))
                throw ExitCode.failure
        }
    }
}
