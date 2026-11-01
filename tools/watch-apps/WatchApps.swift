import AppKit
import Applications
import ArgumentParser
import AsyncAlgorithms
import CasePaths
import Encoding
import OrderedCollections
import Results

// --- [CONSTANTS] -----------------------------------------------------------------------

let messagingTimeout: Duration = .seconds(1)

let watched: Swift.Set<String> = [
    kAXWindowCreatedNotification, kAXSheetCreatedNotification, kAXUIElementDestroyedNotification, kAXFocusedWindowChangedNotification, kAXMainWindowChangedNotification,
    kAXTitleChangedNotification, kAXWindowMiniaturizedNotification, kAXWindowDeminiaturizedNotification, kAXApplicationHiddenNotification, kAXApplicationShownNotification,
    kAXValueChangedNotification,
]

// --- [MODELS] --------------------------------------------------------------------------

@CasePathable
enum Line: Encodable, Sendable {
    case states(OrderedDictionary<NSRunningApplication, InstanceState>)
    case fail(any Error)

    static func answering(_ states: OrderedDictionary<NSRunningApplication, InstanceState>) -> Self {
        .states(
            states.filter { _, state in !state.is(\.running.launching) }.mapValues { state in
                state[case: \.running.responding].map { elements in .running(.responding(elements: elements.filter(\.requiresAnswer))) } ?? state
            }
        )
    }

    func encode(to encoder: any Encoder) throws {
        switch self {
            case .states(let states): try Dictionary(uniqueKeysWithValues: states.map { instance, state in (String(instance.processIdentifier), state) }).encode(to: encoder)
            case .fail(let error): try ["fail": ["reason": error.localizedDescription]].encode(to: encoder)
        }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

@main
struct WatchApps: AsyncParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(
        abstract: "Observes and drives installed applications in background. Commands exit 1 after a fail line",
        subcommands: [Watch.self, Launch.self, Read.self, Perform.self, Set.self],
    )

    @MainActor
    static func run(_ work: @escaping @MainActor (AsyncStream<(key: String, line: Line)>.Continuation) async -> Void) async throws {
        AXUIElementSetMessagingTimeout(AXUIElementCreateSystemWide(), Float(messagingTimeout / .seconds(1)))
        let (lines, output): (AsyncStream<(key: String, line: Line)>, AsyncStream<(key: String, line: Line)>.Continuation) = AsyncStream.makeStream()
        let parent: any DispatchSourceProcess = DispatchSource.makeProcessSource(identifier: getppid(), eventMask: .exit, queue: .main)
        parent.setEventHandler { output.finish() }
        parent.activate()
        let encoder: JSONEncoder = JSONEncoder()
        encoder.outputFormatting = [.sortedKeys, .withoutEscapingSlashes]
        async let _: Void = { @MainActor in
            defer { output.finish() }
            await work(output)
        }()
        let written: (failed: Bool, printed: [String: Data]) = try await lines.reduce(into: (false, [:])) { @MainActor written, entry in
            let data: Data = try encoder.encodeLine([entry.key: entry.line])
            if entry.line.is(\.fail) { written.failed = true }
            if written.printed.updateValue(data, forKey: entry.key) != data { try FileHandle.standardOutput.write(contentsOf: data) }
        }
        Self.exit(withError: written.failed ? ExitCode.failure : nil)
    }

    @MainActor
    static func run(_ names: [String], _ work: @escaping @MainActor (OrderedDictionary<String, [Installation]>, AsyncStream<(key: String, line: Line)>.Continuation) async -> Void) async throws {
        let lookups: [Task<Result<[Installation], AggregateError<(key: String, line: Line)>>, Never>] = OrderedSet(names).map { name in
            Task { await Result { () async throws(AggregateError<ApplicationFailure>) in try await Installation.installed(named: name) }.aggregated { error in (key: name, line: Line.fail(error)) } }
        }
        let resolved: Result<[[Installation]], AggregateError<(key: String, line: Line)>> = traverse(await Array(lookups.async.map { lookup in await lookup.value }), \.self)
        try await run { output in
            switch resolved {
                case .success(let rows): await work(OrderedDictionary(grouping: OrderedSet(rows.joined()), by: \.identifier), output)
                case .failure(let failed): for line: (key: String, line: Line) in failed.errors { output.yield(line) }
            }
        }
    }
}

struct Installed: ParsableArguments {
    @Argument(help: "Bundle identifiers or display names of installed applications, case and diacritics ignored")
    var names: [String]
}

struct Watch: AsyncParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(
        abstract: "Prints dialogs and sheets requiring an answer, hangs, and exits of running instances at start and on each change"
    )

    @OptionGroup var installed: Installed

    func run() async throws {
        try await WatchApps.run(installed.names) { installations, output in
            if case .failure(let failure) = Result(catching: { () throws(ApplicationFailure) in try checkAccessibilityTrust() }) {
                for id: String in installations.keys { output.yield((id, .fail(failure))) }
                return
            }
            await withDiscardingTaskGroup { group in
                for id: String in installations.keys {
                    group.addTask {
                        for await states: OrderedDictionary<NSRunningApplication, InstanceState> in await NSWorkspace.shared.instanceStates(
                            of: { instance in instance.bundleIdentifier == id },
                            walking: \.requiresAnswer,
                            notifications: watched,
                            polling: messagingTimeout,
                        ) {
                            output.yield((id, .answering(states)))
                        }
                    }
                }
            }
        }
    }
}

struct Launch: AsyncParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(
        abstract: "Opens each application in background and prints its state once launched. Document windows open only for --document files"
    )

    @OptionGroup var installed: Installed

    @Flag(help: "Opens another instance when one runs")
    var new: Bool = false

    @Flag(help: "Opens new instance hidden with windows and dialogs off screen")
    var hides: Bool = false

    @Option(help: "File to open, one per flag")
    var document: [String] = []

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
        let documents: [URL] = document.map { path in URL(filePath: path) }
        try await WatchApps.run(installed.names) { installations, output in
            await withDiscardingTaskGroup { group in
                for (id, copies): (key: String, value: [Installation]) in installations {
                    group.addTask {
                        let launched: Result<NSRunningApplication, ApplicationFailure> = await Result { () async throws(ApplicationFailure) in
                            let configuration: NSWorkspace.OpenConfiguration = NSWorkspace.OpenConfiguration()
                            configuration.createsNewApplicationInstance = new
                            configuration.hides = hides
                            configuration.arguments = arguments
                            configuration.environment = Dictionary(environment) { _, last in last }
                            return try await NSWorkspace.shared.launch(Installation.single(of: id, among: copies), opening: documents, configuration: configuration)
                        }
                        switch launched {
                            case .success(let instance): output.yield((id, .answering([instance: await instance.state(walking: \.requiresAnswer)])))
                            case .failure(let failure): output.yield((id, .fail(failure)))
                        }
                    }
                }
            }
        }
    }
}

struct Read: AsyncParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(
        abstract: "Prints full state of each running instance once"
    )

    @OptionGroup var installed: Installed

    @Option(help: "Process ID of one instance to read")
    var pid: pid_t?

    func run() async throws {
        try await WatchApps.run(installed.names) { installations, output in
            for id: String in installations.keys {
                let running: [NSRunningApplication] = NSWorkspace.shared.instances { instance in instance.bundleIdentifier == id }
                let chosen: [NSRunningApplication] = running.filter { instance in pid.map { pid in pid == instance.processIdentifier } ?? true }
                switch (pid, chosen.isEmpty) {
                    case (.some(let pid), true):
                        output.yield((id, .fail(Unmatched(subject: "running instance of \(id)", query: String(pid), candidates: running.map { instance in String(instance.processIdentifier) }))))
                    case (_, _):
                        output.yield((id, .states(OrderedDictionary(uniqueKeysWithValues: await Array(chosen.async.map { instance in (instance, await instance.state(walking: { _ in true })) })))))
                }
            }
        }
    }
}

struct Target: ParsableArguments {
    @Argument(
        help: "Element ID from read or watch line",
        transform: { id in
            guard let element: Data = Data(base64Encoded: id) else { throw ValidationError("\(id) is no element ID") }
            return element
        },
    )
    var element: Data

    func run(_ action: ElementAction) async throws {
        try await WatchApps.run { output in
            if case .failure(let failure) = await Result(catching: { () async throws(ApplicationFailure) in try await action.perform() }) {
                output.yield((action.element.base64EncodedString(), .fail(failure)))
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

struct Set: AsyncParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(
        abstract: "Sets element AXValue to text, silent unless write fails"
    )

    @OptionGroup var target: Target

    @Argument(help: "Text for element AXValue")
    var value: String

    func run() async throws {
        try await target.run(.set(element: target.element, value: value))
    }
}
