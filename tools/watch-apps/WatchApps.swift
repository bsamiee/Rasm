import AppKit
import Applications
import ArgumentParser
import AsyncAlgorithms
import CasePaths
import Encoding
import OrderedCollections
import Results

// --- [MODELS] --------------------------------------------------------------------------

@CasePathable
enum Line: Encodable, Sendable {
    case states(OrderedDictionary<NSRunningApplication, InstanceState>)
    case fail(String)

    static func answering(_ states: OrderedDictionary<NSRunningApplication, InstanceState>) -> Self {
        .states(
            states.filter { _, state in !state.is(\.running.launching) }.mapValues { state in
                state[case: \.running.responding].map { elements in .running(status: .responding(elements: elements.filter(\.requiresAnswer))) } ?? state
            }
        )
    }

    func encode(to encoder: any Encoder) throws {
        switch self {
            case .states(let states): try Dictionary(uniqueKeysWithValues: states.map { instance, state in (String(instance.processIdentifier), state) }).encode(to: encoder)
            case .fail(let reason): try ["fail": ["reason": reason]].encode(to: encoder)
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
    static func run(_ names: [String], _ work: @escaping @MainActor ([Installation], AsyncStream<(key: String, line: Line)>.Continuation) async -> Void) async throws {
        let lookups: [(name: String, rows: Task<Result<[Installation], AggregateError<DiscoveryFailure>>, Never>)] = OrderedSet(names).map { name in
            (name, Task { await Installation.installed(named: name) })
        }
        let resolved: Result<[[Installation]], AggregateError<(key: String, line: Line)>> = traverse(await Array(lookups.async.map { lookup in (lookup.name, await lookup.rows.value) })) {
            name,
            rows in
            rows.mapError { error in AggregateError(first: (name, .fail(error.localizedDescription))) }
        }
        try await run { output in
            switch resolved {
                case .success(let rows): await work(OrderedSet(rows.joined()).elements, output)
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
            let ids: OrderedSet<String> = OrderedSet(installations.map(\.identifier))
            guard AXIsProcessTrusted() else {
                for id: String in ids { output.yield((id, .fail(ElementFailure.untrusted.localizedDescription))) }
                return
            }
            await withDiscardingTaskGroup { group in
                for id: String in ids {
                    group.addTask {
                        for await states: OrderedDictionary<NSRunningApplication, InstanceState> in await NSWorkspace.shared.instanceStates(
                            of: { instance in instance.bundleIdentifier == id },
                            walking: \.requiresAnswer,
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
        let (new, hides, documents, arguments, environment): (Bool, Bool, [URL], [String], [String: String]) = (
            new, hides, document.map { path in URL(filePath: path) }, arguments,
            Dictionary(environment) { _, last in last },
        )
        try await WatchApps.run(installed.names) { installations, output in
            await withDiscardingTaskGroup { group in
                for id: String in OrderedSet(installations.map(\.identifier)) {
                    group.addTask {
                        let launched: Result<NSRunningApplication, any Error> = await Result {
                            let configuration: NSWorkspace.OpenConfiguration = NSWorkspace.OpenConfiguration()
                            configuration.createsNewApplicationInstance = new
                            configuration.hides = hides
                            configuration.arguments = arguments
                            configuration.environment = environment
                            return try await NSWorkspace.shared.launchApplication(
                                Installation.single(of: id, among: installations).get(),
                                opening: documents,
                                configuration: configuration,
                            )
                            .get()
                        }
                        switch launched {
                            case .success(let instance): output.yield((id, .answering([instance: InstanceState(await instance.status(walking: \.requiresAnswer))])))
                            case .failure(let error): output.yield((id, .fail(error.localizedDescription)))
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
        let pid: pid_t? = pid
        try await WatchApps.run(installed.names) { installations, output in
            for id: String in OrderedSet(installations.map(\.identifier)) {
                let running: [NSRunningApplication] = NSRunningApplication.instances(withBundleIdentifier: id)
                let chosen: [NSRunningApplication] = running.filter { instance in pid.map { pid in pid == instance.processIdentifier } ?? true }
                let states: [(NSRunningApplication, InstanceState)] = await Array(chosen.async.map { instance in (instance, InstanceState(await instance.status(walking: { _ in true }))) })
                let missed: String? = pid.flatMap { pid in
                    chosen.isEmpty
                        ? Unmatched(subject: "running instance of \(id)", query: String(pid), candidates: running.map { instance in String(instance.processIdentifier) }).localizedDescription : nil
                }
                output.yield((id, missed.map(Line.fail) ?? .states(OrderedDictionary(uniqueKeysWithValues: states))))
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
            let outcome: Result<Void, ElementFailure> = await NSRunningApplication.owning(action.element)?.perform(action) ?? .failure(.absent(action.element))
            if case .failure(let failure) = outcome { output.yield((action.element.base64EncodedString(), .fail(failure.localizedDescription))) }
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
    static let configuration: CommandConfiguration = CommandConfiguration(abstract: "Sets element AXValue to text, silent unless write fails")

    @OptionGroup var target: Target

    @Argument(help: "Text for element AXValue")
    var value: String

    func run() async throws {
        try await target.run(.set(element: target.element, value: value))
    }
}
