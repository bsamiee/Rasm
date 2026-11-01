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
    case fail(reason: String)

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
        let lookups: [(name: String, rows: Task<Result<[Installation], AggregateError<ApplicationFailure>>, Never>)] = OrderedSet(names).map { name in
            (name, Task { await Result { () async throws(AggregateError<ApplicationFailure>) in try await Installation.installed(named: name) } })
        }
        let resolved: Result<[[Installation]], AggregateError<(key: String, line: Line)>> = traverse(await Array(lookups.async.map { lookup in (lookup.name, await lookup.rows.value) })) {
            name,
            rows in
            rows.aggregated { error in (name, .fail(reason: error.localizedDescription)) }
        }
        try await run { output in
            switch resolved {
                case .success(let rows): await work(OrderedDictionary(grouping: OrderedSet(rows.joined()), by: \.identifier), output)
                case .failure(let failed): for line: (key: String, line: Line) in failed.errors { output.yield(line) }
            }
        }
    }
}
