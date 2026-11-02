import AppKit
import Applications
import ArgumentParser
import AsyncAlgorithms
import CasePaths
import Encoding
import OrderedCollections
import Processes
import Results
import ScreenCaptureKit
import System

// --- [CONSTANTS] -----------------------------------------------------------------------

let messagingTimeout: Duration = .seconds(1)
let notifications: Swift.Set<String> = [
    kAXWindowCreatedNotification, kAXSheetCreatedNotification, kAXUIElementDestroyedNotification, kAXFocusedWindowChangedNotification, kAXTitleChangedNotification,
    kAXWindowMiniaturizedNotification, kAXWindowDeminiaturizedNotification,
]
let captures: URL = URL.temporaryDirectory.appending(component: "watch-apps", directoryHint: .isDirectory)

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
    case fail(String)

    func encode(to encoder: any Encoder) throws {
        var line: any SingleValueEncodingContainer = encoder.singleValueContainer()
        switch self {
            case .states(let states): try line.encode(states.byProcess)
            case .captured(let captured): try line.encode(captured.byProcess)
            case .pressure(let pressure): try line.encode(pressure)
            case .fail(let reason): try line.encode(["fail": ["reason": reason]])
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension OrderedDictionary where Key == NSRunningApplication {
    var byProcess: [String: Value] {
        Dictionary(uniqueKeysWithValues: map { instance, value in (String(instance.processIdentifier), value) })
    }
}

extension Installation {
    @MainActor
    static func application(named name: String) async throws(ApplicationFailure) -> [Self] {
        let found: OrderedDictionary<String, [Self]> = OrderedDictionary(grouping: try await installed(named: name), by: \.identifier)
        return try Unmatched.single(Array(found.elements), subject: "installed application", query: name, candidates: []) { application in application.key }.map(\.value)
            .mapError { miss in ApplicationFailure(subject: name, reason: .unmatched(miss)) }.get()
    }
}

extension NSWorkspace {
    @MainActor
    func instanceStates(of id: String) -> some AsyncSequence<OrderedDictionary<NSRunningApplication, InstanceState>, Never> & Sendable {
        instanceStates(of: { instance in instance.bundleIdentifier == id }, walking: \.requiresAnswer, notifications: notifications, polling: messagingTimeout)
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

@main
struct WatchApps: AsyncParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(
        abstract: "Observes and drives installed applications in background. Commands exit 1 after a fail line",
        subcommands: [Watch.self, Launch.self, Read.self, Capture.self, Perform.self, Set.self],
    )

    static func main() async {
        do {
            var command: any ParsableCommand = try parseAsRoot()
            try checkAccessibilityTrust()
            AXUIElementSetMessagingTimeout(AXUIElementCreateSystemWide(), Float(messagingTimeout / .seconds(1)))
            if var asynchronous: any AsyncParsableCommand = command as? any AsyncParsableCommand { try await asynchronous.run() } else { try command.run() }
        } catch let error where exitCode(for: error) == .success {
            exit(withError: error)
        } catch {
            await run { output in output.yield((ProcessInfo.processInfo.processName, .fail(message(for: error)))) }
        }
    }

    @MainActor
    static func run(_ work: @escaping @MainActor (AsyncStream<(key: String, line: Line)>.Continuation) async -> Void) async -> Never {
        let (lines, output): (AsyncStream<(key: String, line: Line)>, AsyncStream<(key: String, line: Line)>.Continuation) = AsyncStream.makeStream()
        let parent: any DispatchSourceProcess = DispatchSource.makeProcessSource(identifier: getppid(), eventMask: .exit, queue: .main)
        let signals: [any DispatchSourceSignal] = [SIGTERM, SIGINT, SIGHUP].map { number in
            signal(number, SIG_IGN)
            return DispatchSource.makeSignalSource(signal: number, queue: .main)
        }
        for source: any DispatchSourceProtocol in signals + [parent] {
            source.setEventHandler { output.finish() }
            source.activate()
        }
        if getppid() == 1 { output.finish() }
        let encoder: JSONEncoder = JSONEncoder()
        encoder.outputFormatting = [.sortedKeys, .withoutEscapingSlashes]
        let written: Result<Bool, any Error> = await { @MainActor () async -> Result<Bool, any Error> in
            async let _: Void = { @MainActor in
                await work(output)
                output.finish()
            }()
            do {
                return .success(
                    try await lines.reduce(into: (failed: false, printed: [String: Data]())) { @MainActor written, entry in
                        let data: Data = try encoder.encodeLine([entry.key: entry.line])
                        written.failed = written.failed || entry.line.is(\.fail)
                        if written.printed.updateValue(data, forKey: entry.key) != data { try FileHandle.standardOutput.write(contentsOf: data) }
                    }.failed
                )
            } catch {
                return .failure(error)
            }
        }()
        withExtendedLifetime((parent, signals)) {
            switch written {
                case .success(let failed): exit(withError: failed ? ExitCode.failure : nil)
                case .failure(let error): exit(withError: error)
            }
        }
    }

    @MainActor
    static func run(
        _ names: [String],
        _ work: @escaping @MainActor (OrderedDictionary<String, [Installation]>, AsyncStream<(key: String, line: Line)>.Continuation) async -> Void,
    ) async -> Never {
        await run { output in
            let found: [(name: String, application: Result<[Installation], ApplicationFailure>)] = await Array(
                OrderedSet(names).async.map { name in (name, await Result { () async throws(ApplicationFailure) in try await Installation.application(named: name) }) }
            )
            for (name, failure): (String, ApplicationFailure) in found.compactMap({ name, application in application[case: \.failure].map { failure in (name, failure) } }) {
                output.yield((name, .fail(failure.localizedDescription)))
            }
            await work(OrderedDictionary(grouping: OrderedSet(found.compactMap { _, application in application[case: \.success] }.joined()), by: \.identifier), output)
        }
    }
}

struct Installed: ParsableArguments {
    @Argument(help: "Bundle identifiers or display names of installed applications, case and diacritics ignored")
    var names: [String]
}

struct Instance: ParsableArguments {
    @OptionGroup var installed: Installed

    @Option(parsing: .unconditional, help: "Process ID of one running instance")
    var pid: pid_t?

    func validate() throws(ValidationError) {
        guard pid == nil || installed.names.count == 1 else { throw ValidationError("--pid names an instance of one application") }
    }

    func each<Value>(
        of applications: some Collection<String>,
        output: AsyncStream<(key: String, line: Line)>.Continuation,
        _ read: @escaping @Sendable (_ id: String, _ instance: NSRunningApplication) async -> Result<Value, ApplicationFailure>,
        as line: (OrderedDictionary<NSRunningApplication, Value>) -> Line,
    ) async {
        for id: String in applications {
            let running: [NSRunningApplication] = NSWorkspace.shared.instances { instance in instance.bundleIdentifier == id }
            let chosen: Result<[NSRunningApplication], Unmatched> =
                pid.map { pid in
                    Unmatched.single(
                        running.filter { instance in instance.processIdentifier == pid },
                        subject: "running instance of \(id)",
                        query: String(pid),
                        candidates: running.map { instance in String(instance.processIdentifier) },
                    ) { instance in String(instance.processIdentifier) }
                    .map { instance in [instance] }
                } ?? .success(running)
            switch chosen {
                case .success(let chosen):
                    let reads: [(NSRunningApplication, Result<Value, ApplicationFailure>)] = await Array(chosen.async.map { instance in (instance, await read(id, instance)) })
                    let values: OrderedDictionary<NSRunningApplication, Value> = OrderedDictionary(
                        uniqueKeysWithValues: reads.compactMap { instance, read in read[case: \.success].map { value in (instance, value) } }
                    )
                    for failure: ApplicationFailure in reads.compactMap({ _, read in read[case: \.failure] }) { output.yield((id, .fail(failure.localizedDescription))) }
                    if !values.isEmpty || reads.isEmpty { output.yield((id, line(values))) }
                case .failure(let miss): output.yield((id, .fail(miss.localizedDescription)))
            }
        }
    }
}

struct Watch: AsyncParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(
        abstract: "Prints windows, dialogs requiring an answer, hangs, exits, system prompts naming the applications, and system pressure, at start and on each change"
    )

    @OptionGroup var installed: Installed

    func run() async throws {
        await WatchApps.run(installed.names) { applications, output in
            guard !applications.isEmpty else { return }
            let names: [String] = Array(OrderedSet(applications.values.joined().map(\.name)))
            await withDiscardingTaskGroup { group in
                group.addTask {
                    for await pressure: Result<SystemPressure, Errno> in SystemPressure.changes() {
                        let line: Line =
                            switch pressure {
                                case .success(let pressure): .pressure(pressure)
                                case .failure(let error): .fail(error.description)
                            }
                        output.yield(("system", line))
                    }
                }
                for id: String in applications.keys {
                    group.addTask {
                        for await states: OrderedDictionary<NSRunningApplication, InstanceState> in await NSWorkspace.shared.instanceStates(of: id) {
                            output.yield((id, .states(states)))
                        }
                    }
                }
                group.addTask {
                    let prompts: some AsyncSequence<(host: String, prompts: OrderedDictionary<NSRunningApplication, InstanceState>), Never> = await NSWorkspace.shared.systemPrompts(
                        naming: names,
                        notifications: notifications,
                        polling: messagingTimeout,
                    )
                    for await asking: (host: String, prompts: OrderedDictionary<NSRunningApplication, InstanceState>) in prompts { output.yield((asking.host, .states(asking.prompts))) }
                }
            }
        }
    }
}

struct Launch: AsyncParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(
        abstract:
            "Opens each application hidden without activation and prints its state until it leaves launching, then holds an instance it created until the starting process exits or a signal ends it, and quits that instance"
    )

    @OptionGroup var installed: Installed

    @Flag(help: "Opens another instance when one runs")
    var new: Bool = false

    @Option(
        help: ArgumentHelp("File to open, one per flag", valueName: "path"),
        transform: { path in
            let file: URL = URL(filePath: path)
            do { _ = try file.checkResourceIsReachable() } catch { throw ValidationError("\(path) is unreachable: \(error.localizedDescription)") }
            return file
        },
    )
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

    func run() async throws {
        await WatchApps.run(installed.names) { applications, output in
            await withDiscardingTaskGroup { group in
                for (id, copies): (key: String, value: [Installation]) in applications {
                    group.addTask { await launch(id, among: copies, output: output) }
                }
            }
        }
    }

    @MainActor
    func launch(_ id: String, among copies: [Installation], output: AsyncStream<(key: String, line: Line)>.Continuation) async {
        do throws(ApplicationFailure) {
            let configuration: NSWorkspace.OpenConfiguration = NSWorkspace.OpenConfiguration()
            configuration.createsNewApplicationInstance = new
            configuration.arguments = arguments
            configuration.environment = Dictionary(environment) { _, last in last }
            let launch: Applications.Launch = try await NSWorkspace.shared.launch(Installation.single(of: id, among: copies), opening: document, configuration: configuration)
            for await state: InstanceState in launch.instance.states(walking: \.requiresAnswer, notifications: notifications, polling: messagingTimeout) {
                output.yield((id, .states([launch.instance: state])))
                guard case .running(.launching, _) = state else { break }
            }
            if launch.created {
                for await _: Result<ProcessExit, Errno> in termination(of: launch.instance.processIdentifier) {}
            }
        } catch {
            output.yield((id, .fail(error.localizedDescription)))
        }
    }
}

struct Read: AsyncParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(
        abstract: "Prints full state of each running instance once, with its resource usage"
    )

    @OptionGroup var instance: Instance

    func run() async throws {
        await WatchApps.run(instance.installed.names) { applications, output in
            await instance.each(of: applications.keys, output: output, { _, running in .success(await running.state(walking: { _ in true })) }, as: Line.states)
        }
    }
}

struct Capture: AsyncParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(
        abstract: "Writes a PNG of each running instance's on-screen windows and prints its file and window numbers"
    )

    @OptionGroup var instance: Instance

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

    func run() async throws {
        await WatchApps.run(instance.installed.names) { applications, output in
            await instance.each(of: applications.keys, output: output, capture, as: Line.captured)
        }
    }

    func capture(_ id: String, _ running: NSRunningApplication) async -> Result<Screenshot, ApplicationFailure> {
        await Result { () async throws(ApplicationFailure) in
            let file: URL = captures.appending(component: [id, String(running.processIdentifier), window.map(String.init) ?? "windows"].joined(separator: "-") + ".png")
            do { try FileManager.default.createDirectory(at: captures, withIntermediateDirectories: true) } catch {
                throw ApplicationFailure(subject: captures.path(percentEncoded: false), reason: .host(error))
            }
            let configuration: SCScreenshotConfiguration = SCScreenshotConfiguration(file: file)
            configuration.showsCursor = false
            return try await Screenshot(file: file.path(percentEncoded: false), windows: running.screenshot(of: window, longestSide: longestSide, configuration: configuration).windows)
        }
    }
}

struct Target: ParsableArguments {
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
            if case .failure(let failure) = await Result(catching: { () async throws(ApplicationFailure) in try await action.perform() }) {
                output.yield((action.element.base64EncodedString(), .fail(failure.localizedDescription)))
            }
        }
    }
}

struct Perform: AsyncParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(
        abstract: "Performs one element Accessibility action, silent unless it fails"
    )

    @OptionGroup var target: Target

    @Argument(help: "Accessibility action from element actions")
    var action: String

    func run() async throws {
        await target.run(.perform(element: target.id, action: action))
    }
}

struct Set: AsyncParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(
        abstract: "Sets element AXValue to text, silent unless the element holds no settable AXValue or the write fails"
    )

    @OptionGroup var target: Target

    @Argument(help: "Text for element AXValue")
    var value: String

    func run() async throws {
        await target.run(.set(element: target.id, value: value))
    }
}
