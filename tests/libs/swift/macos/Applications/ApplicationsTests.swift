import AppleEvents
import CustomDump
import Encoding
import Foundation
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

private func absence(of bundle: borrowing Fixture, cached: URL? = nil) async throws -> AppleEventFailure.Reason? {
    let installation = try Installation(at: bundle.url)
    do {
        _ = try await installation.scriptingDefinition(running: false, cached: cached)
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
        let read = try await Installation(at: bundle.url).scriptingDefinition(running: false)
        expectNoDifference(read, definition)
    }

    @Test
    func refusesDynamicDefinitionWithoutInstance() async throws {
        let bundle = try Fixture(["OSAScriptingDefinition": "Fixture.sdef", "OSAScriptingDefinitionIsDynamic": true], resources: ["Fixture.sdef": definition])
        let (dynamic, undeclared) = (try await absence(of: bundle), try await absence(of: Fixture([:])))
        expectNoDifference([dynamic, undeclared], [.dynamic, .undeclared])
    }

    @Test
    func readsCachedDefinitionWithoutInstance() async throws {
        let bundle = try Fixture(["OSAScriptingDefinitionIsDynamic": true], resources: ["Cached.sdef": definition])
        let cached = bundle.url.appending(components: "Contents", "Resources", "Cached.sdef")
        let read = try await Installation(at: bundle.url).scriptingDefinition(running: false, cached: cached)
        expectNoDifference(read, definition)
    }
}

@Suite
struct ValueTests {
    @Test
    func reachesTargetByValueOrNumber() {
        let cases: [(target: JSONValue, read: JSONValue?, reached: Bool)] = [
            (.string("Letter"), .string("Letter"), true),
            (.string("Letter"), .string("A4"), false),
            (.number(12), .string(" 12 "), true),
            (.string("12"), .number(12), true),
            (.number(12), .string("12 pt"), false),
            (.bool(true), .bool(true), true),
            (.number(3), nil, false),
        ]
        expectNoDifference(cases.map { row in row.target.reached(by: row.read) }, cases.map(\.reached))
    }

    @Test
    func spellsTextOfStringsAndNumbers() {
        expectNoDifference([JSONValue.string("A4"), .number(12), .number(0.5), .bool(true)].map(\.text), ["A4", "12", 0.5.formatted(.number.grouping(.never)), nil])
    }
}
