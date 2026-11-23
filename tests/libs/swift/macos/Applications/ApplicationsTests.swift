import AppKit
import AppleEvents
import AsyncAlgorithms
import CustomDump
import Encoding
import Foundation
import Processes
import Testing

@testable import Applications

// --- [CONSTANTS] -----------------------------------------------------------------------

private let definition = Data(#"<dictionary title="Fixture"><suite name="Fixture" code="Fixt"/></dictionary>"#.utf8)

// --- [MODELS] --------------------------------------------------------------------------

private struct Fixture: ~Copyable {
    let url: URL

    init(_ info: [String: Any], resources: [String: Data] = [:]) throws {
        url = FileManager.default.temporaryDirectory.appending(components: UUID().uuidString, "Fixture.app")
        let contents = url.appending(component: "Contents")
        try FileManager.default.createDirectory(at: contents.appending(component: "Resources"), withIntermediateDirectories: true)
        try PropertyListSerialization.data(fromPropertyList: info.merging(["CFBundleIdentifier": "app.rasm.fixture"]) { kept, _ in kept }, format: .xml, options: 0)
            .write(to: contents.appending(component: "Info.plist"))
        for (name, data) in resources { try data.write(to: contents.appending(components: "Resources", name)) }
    }

    deinit {
        do { try FileManager.default.removeItem(at: url.deletingLastPathComponent()) } catch { Issue.record(error) }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

private func absence(of bundle: borrowing Fixture) async throws -> AppleEventFailure.Reason? {
    let installation = try Installation(at: bundle.url)
    do {
        _ = try await installation.scriptingDefinition(cached: nil)
        return nil
    } catch {
        return error.reason
    }
}

@Suite
struct ScriptingDefinitionTests {
    @Test
    func readsStaticDefinitionWithoutExtension() async throws {
        let bundle = try Fixture(["OSAScriptingDefinition": "Fixture"], resources: ["Fixture.sdef": definition])
        let read = try await Installation(at: bundle.url).scriptingDefinition(cached: nil)
        expectNoDifference(read, definition)
    }

    @Test
    func refusesDynamicDefinitionWithoutInstance() async throws {
        let bundle = try Fixture(["OSAScriptingDefinition": "Fixture.sdef", "OSAScriptingDefinitionIsDynamic": true], resources: ["Fixture.sdef": definition])
        let (dynamic, undeclared) = (try await absence(of: bundle), try await absence(of: Fixture([:])))
        expectNoDifference([dynamic, undeclared], [.undeclared, .undeclared])
    }

    @Test
    func readsCachedDefinitionWithoutInstance() async throws {
        let bundle = try Fixture(["OSAScriptingDefinitionIsDynamic": true], resources: ["Cached.sdef": definition])
        let cached = bundle.url.appending(components: "Contents", "Resources", "Cached.sdef")
        let read = try await Installation(at: bundle.url).scriptingDefinition(cached: cached)
        expectNoDifference(read, definition)
    }
}

@Suite
struct ValueTests {
    @Test
    func spellsTextOfStringsAndNumbers() {
        let values: [JSONValue] = [.string("A4"), .number(12), .number(0.5), .number(0.123456789), .bool(true)]
        expectNoDifference(values.map(\.text), ["A4", "12", "0.5", "0.123456789", nil])
        expectNoDifference(values.map { value in value.text.flatMap { text in JSONValue.string(text).number } }, [nil, 12, 0.5, 0.123456789, nil])
    }
}

@Suite
struct LifetimeTests {
    @Test(.timeLimit(.minutes(1)))
    func endsEachLifetimeWithItsExit() async throws {
        let lane = (0..<1000).async.map { (_: Int) async throws -> String? in
            let child = Process()
            child.executableURL = URL(filePath: "/bin/sleep")
            child.arguments = ["60"]
            try child.run()
            let process = try ProcessInstance(of: child.processIdentifier)
            let running = AsyncStream<InstanceState>(unfolding: {
                do { try process.signal(SIGTERM) } catch { Issue.record(error) }
                return nil
            })
            let states: [InstanceState] = await Instance(application: .current, process: process).lifetime(running, clock: ContinuousClock()).reduce(into: []) { states, state in states.append(state) }
            return String(bytes: try JSONEncoder().encode(states), encoding: .utf8)
        }
        let counts: [String?: Int] = try await withThrowingTaskGroup(of: [String?].self) { group in
            for _ in 0..<8 { group.addTask { try await Array(lane) } }
            return try await group.reduce(into: [:]) { counts, ended in for lifetime in ended { counts[lifetime, default: 0] += 1 } }
        }
        expectNoDifference(counts, [#"[{"exited":{"signal":15}}]"#: 8000])
    }
}
