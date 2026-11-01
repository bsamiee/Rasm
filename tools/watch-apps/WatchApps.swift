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
]
let promptHosts: [String] = ["com.apple.UserNotificationCenter", "com.apple.coreservices.uiagent", "com.apple.accessibility.universalAccessAuthWarn"]
let captures: URL = URL.temporaryDirectory.appending(component: "watch-apps", directoryHint: .isDirectory)

// --- [MODELS] --------------------------------------------------------------------------

@CasePathable
enum Line: Encodable, Sendable {
    case states(OrderedDictionary<NSRunningApplication, InstanceState>)
    case captured(pid_t, file: URL, windows: [CGWindowID])
    case pressure(SystemPressure)
    case fail(String)

    func encode(to encoder: any Encoder) throws {
        switch self {
            case .states(let states): try Dictionary(uniqueKeysWithValues: states.map { instance, state in (String(instance.processIdentifier), state) }).encode(to: encoder)
            case .captured(let pid, let file, let windows):
                try JSONValue.object([String(pid): .object(["file": .string(file.path(percentEncoded: false)), "windows": .array(windows.map { window in .number(Double(window)) })])])
                    .encode(to: encoder)
            case .pressure(let pressure): try pressure.encode(to: encoder)
            case .fail(let reason): try ["fail": ["reason": reason]].encode(to: encoder)
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension InstanceState {
    func prompting(_ names: [String]) -> Self? {
        guard case .running(.responding(let elements), let usage) = self else { return nil }
        let asking: [Element] = elements.filter { element in
            element.requiresAnswer && ([element.title].compactMap(\.self) + (element.detail?.text ?? [])).contains { text in names.contains { name in text.localizedStandardContains(name) } }
        }
        return asking.isEmpty ? nil : .running(.responding(elements: asking), usage: usage)
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

@main
struct WatchApps: AsyncParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(
        abstract: "Observes and drives installed applications in background. Commands exit 1 after a fail line",
        subcommands: [Watch.self, Launch.self, Read.self, Capture.self, Perform.self, Write.self],
    )

    static func main() async {
        do {
            switch try parseAsRoot() {
                case let command as any AsyncParsableCommand: try await execute(command)
                case let command: try execute(command)
            }
        } catch {
            guard exitCode(for: error) != .success else { exit(withError: error) }
            do { try await run { output in output.yield((ProcessInfo.processInfo.processName, .fail(message(for: error)))) } } catch { exit(withError: error) }
        }
    }

    static func execute(_ command: consuming sending any AsyncParsableCommand) async throws {
        try await command.run()
    }

    static func execute(_ command: consuming any ParsableCommand) throws {
        try command.run()
    }

    @MainActor
    static func run(_ work: @escaping @MainActor (AsyncStream<(key: String, line: Line)>.Continuation) async -> Void) async throws {
        let (lines, output): (AsyncStream<(key: String, line: Line)>, AsyncStream<(key: String, line: Line)>.Continuation) = AsyncStream.makeStream()
        let parent: any DispatchSourceProcess = DispatchSource.makeProcessSource(identifier: getppid(), eventMask: .exit, queue: .main)
        parent.setEventHandler { output.finish() }
        parent.activate()
        if getppid() == 1 { output.finish() }
        AXUIElementSetMessagingTimeout(AXUIElementCreateSystemWide(), Float(messagingTimeout / .seconds(1)))
        let encoder: JSONEncoder = JSONEncoder()
        encoder.outputFormatting = [.sortedKeys, .withoutEscapingSlashes]
        async let _: Void = { @MainActor in
            defer { output.finish() }
            switch Result(catching: { () throws(ApplicationFailure) in try checkAccessibilityTrust() }) {
                case .success: await work(output)
                case .failure(let failure): output.yield((ProcessInfo.processInfo.processName, .fail(failure.localizedDescription)))
            }
        }()
        let failed: Bool = try await lines.reduce(into: (failed: false, printed: [String: Data]())) { @MainActor written, entry in
            let data: Data = try encoder.encodeLine([entry.key: entry.line])
            if entry.line.is(\.fail) { written.failed = true }
            if written.printed.updateValue(data, forKey: entry.key) != data { try FileHandle.standardOutput.write(contentsOf: data) }
        }
        .failed
        exit(withError: failed ? ExitCode.failure : nil)
    }

    @MainActor
    static func run(
        _ names: [String],
        _ work: @escaping @MainActor (OrderedDictionary<String, [Installation]>, AsyncStream<(key: String, line: Line)>.Continuation) async -> Void,
    ) async throws {
        try await run { output in
            let lookups: [(name: String, installations: Result<[Installation], ApplicationFailure>)] = await Array(
                OrderedSet(names).async.map { name in (name: name, installations: await Result { () async throws(ApplicationFailure) in try await Installation.installed(named: name) }) }
            )
            for (name, failure): (String, ApplicationFailure) in lookups.compactMap({ lookup in lookup.installations[case: \.failure].map { failure in (lookup.name, failure) } }) {
                output.yield((name, .fail(failure.localizedDescription)))
            }
            await work(OrderedDictionary(grouping: lookups.compactMap { lookup in lookup.installations[case: \.success] }.joined(), by: \.identifier), output)
        }
    }
}

struct Installed: ParsableArguments {
    @Argument(help: "Bundle identifiers or display names of installed applications, case and diacritics ignored")
    var names: [String]
}

struct Instance: ParsableArguments {
    @OptionGroup var installed: Installed

    @Option(help: "Process ID of the one instance to read")
    var pid: pid_t?

    func validate() throws(ValidationError) {
        guard pid == nil || installed.names.count == 1 else { throw ValidationError("--pid names an instance of one application") }
    }

    func instances(of id: String, output: AsyncStream<(key: String, line: Line)>.Continuation) -> [NSRunningApplication] {
        let running: [NSRunningApplication] = NSWorkspace.shared.instances { instance in instance.bundleIdentifier == id }
        let chosen: [NSRunningApplication] = running.filter { instance in pid.map { pid in pid == instance.processIdentifier } ?? true }
        if let pid: pid_t, chosen.isEmpty {
            let miss: Unmatched = Unmatched(subject: "running instance of \(id)", query: String(pid), candidates: running.map { instance in String(instance.processIdentifier) })
            output.yield((id, .fail(miss.localizedDescription)))
        }
        return chosen
    }
}

struct Watch: AsyncParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(
        abstract: "Prints windows, dialogs requiring an answer, hangs, exits, system prompts naming the applications, and system pressure, at start and on each change"
    )

    @OptionGroup var installed: Installed

    func run() async throws {
        try await WatchApps.run(installed.names) { installations, output in
            let names: [String] = installations.values.joined().map(\.name)
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
                for id: String in installations.keys {
                    group.addTask {
                        for await states: OrderedDictionary<NSRunningApplication, InstanceState> in await NSWorkspace.shared.instanceStates(
                            of: { instance in instance.bundleIdentifier == id },
                            walking: \.requiresAnswer,
                            notifications: notifications,
                            polling: messagingTimeout,
                        ) {
                            output.yield((id, .states(states)))
                        }
                    }
                }
                group.addTask {
                    let prompts: some AsyncSequence<OrderedDictionary<NSRunningApplication, InstanceState>, Never> = await NSWorkspace.shared.instanceStates(
                        of: { instance in instance.bundleIdentifier.map(promptHosts.contains) == true },
                        walking: \.requiresAnswer,
                        notifications: notifications,
                        polling: messagingTimeout,
                    )
                    _ = await prompts.reduce(into: Swift.Set<String>()) { shown, states in
                        for host: String in promptHosts {
                            let asking: OrderedDictionary<NSRunningApplication, InstanceState> = states.filter { instance, _ in instance.bundleIdentifier == host }
                                .compactMapValues { state in state.prompting(names) }
                            guard !asking.isEmpty || shown.contains(host) else { continue }
                            if asking.isEmpty { shown.remove(host) } else { shown.insert(host) }
                            output.yield((host, .states(asking)))
                        }
                    }
                }
            }
        }
    }
}

struct Launch: AsyncParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(
        abstract: "Opens each application in background and prints its state until it leaves launching. Document windows open only for --document files"
    )

    @OptionGroup var installed: Installed

    @Flag(help: "Opens another instance when one runs")
    var new: Bool = false

    @Flag(help: "Opens new instance hidden with windows and dialogs off screen")
    var hides: Bool = false

    @Option(
        help: "File to open, one per flag",
        transform: { path in
            let file: URL = URL(filePath: path)
            do { _ = try file.checkResourceIsReachable() } catch { throw ValidationError("\(path) is no reachable file: \(error.localizedDescription)") }
            return file
        },
    )
    var document: [URL] = []

    @Option(parsing: .unconditionalSingleValue, help: "Command-line argument of new instance, one per flag")
    var arguments: [String] = []

    @Option(
        help: "NAME=VALUE environment variable of new instance, one per flag",
        transform: { entry in
            guard let split: String.Index = entry.firstIndex(of: "=") else { throw ValidationError("\(entry) is no NAME=VALUE variable") }
            return (String(entry[..<split]), String(entry[entry.index(after: split)...]))
        },
    )
    var environment: [(String, String)] = []

    func run() async throws {
        try await WatchApps.run(installed.names) { installations, output in
            await withDiscardingTaskGroup { group in
                for (id, copies): (key: String, value: [Installation]) in installations {
                    group.addTask { await launch(id, among: copies, output: output) }
                }
            }
        }
    }

    func launch(_ id: String, among copies: [Installation], output: AsyncStream<(key: String, line: Line)>.Continuation) async {
        let launched: Result<NSRunningApplication, ApplicationFailure> = await Result { () async throws(ApplicationFailure) in
            let configuration: NSWorkspace.OpenConfiguration = NSWorkspace.OpenConfiguration()
            configuration.createsNewApplicationInstance = new
            configuration.hides = hides
            configuration.arguments = arguments
            configuration.environment = Dictionary(environment) { _, last in last }
            return try await NSWorkspace.shared.launch(Installation.single(of: id, among: copies), opening: document, configuration: configuration)
        }
        switch launched {
            case .success(let instance):
                for await state: InstanceState in instance.states(walking: \.requiresAnswer, notifications: notifications, polling: messagingTimeout) {
                    output.yield((id, .states([instance: state])))
                    guard case .running(.launching, _) = state else { break }
                }
                if hides, !document.isEmpty { instance.hide() }
            case .failure(let failure): output.yield((id, .fail(failure.localizedDescription)))
        }
    }
}

struct Read: AsyncParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(
        abstract: "Prints full state of each running instance once, with its resource usage"
    )

    @OptionGroup var instance: Instance

    func run() async throws {
        try await WatchApps.run(instance.installed.names) { installations, output in
            for id: String in installations.keys {
                let chosen: [NSRunningApplication] = instance.instances(of: id, output: output)
                guard !chosen.isEmpty || instance.pid == nil else { continue }
                let states: [(NSRunningApplication, InstanceState)] = await Array(chosen.async.map { running in (running, await running.state(walking: { _ in true })) })
                output.yield((id, .states(OrderedDictionary(uniqueKeysWithValues: states))))
            }
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

    @Option(help: "Longest side in pixels, smaller captures keep native size")
    var longestSide: Int?

    func run() async throws {
        try await WatchApps.run(instance.installed.names) { installations, output in
            for id: String in installations.keys {
                for running: NSRunningApplication in instance.instances(of: id, output: output) {
                    let file: URL = captures.appending(component: "\(id)-\(running.processIdentifier).png")
                    switch await capture(running, to: file) {
                        case .success(let windows): output.yield((id, .captured(running.processIdentifier, file: file, windows: windows)))
                        case .failure(let failure): output.yield((id, .fail(failure.localizedDescription)))
                    }
                }
            }
        }
    }

    func capture(_ running: NSRunningApplication, to file: URL) async -> Result<[CGWindowID], ApplicationFailure> {
        await Result { () async throws(ApplicationFailure) in
            let (shot, windows): (SCScreenshotOutput, [CGWindowID]) = try await running.screenshot(of: window, longestSide: longestSide)
            guard let image: CGImage = shot.sdrImage else { throw ApplicationFailure(subject: "capture of process \(running.processIdentifier)", reason: .absent("SDR image")) }
            let data: Data = try image.data(as: .png)
            do {
                try FileManager.default.createDirectory(at: captures, withIntermediateDirectories: true)
                try data.write(to: file)
            } catch {
                throw ApplicationFailure(subject: file.path(percentEncoded: false), reason: .host(error))
            }
            return windows
        }
    }
}

struct Target: ParsableArguments {
    @Argument(
        help: "Element ID from read or watch line",
        transform: { id in
            guard let element: Data = Data(base64Encoded: id), !element.isEmpty else { throw ValidationError("\(id) is no element ID") }
            return element
        },
    )
    var element: Data

    func run(_ action: ElementAction) async throws {
        try await WatchApps.run { output in
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
        try await target.run(.perform(element: target.element, action: action))
    }
}

struct Write: AsyncParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(
        commandName: "set",
        abstract: "Sets element AXValue to text, silent unless the element holds no settable AXValue or the write fails",
    )

    @OptionGroup var target: Target

    @Argument(help: "Text for element AXValue")
    var value: String

    func run() async throws {
        try await target.run(.set(element: target.element, value: value))
    }
}
