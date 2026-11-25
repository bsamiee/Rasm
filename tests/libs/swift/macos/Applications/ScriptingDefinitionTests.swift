import AppleEvents
import CasePathsCore
import CustomDump
import Foundation
import Testing

@testable import Applications

// --- [MODELS] --------------------------------------------------------------------------

private struct Fixture: ~Copyable {
    let url: URL

    init(declaring name: String?, dynamic: Bool) throws {
        url = FileManager.default.temporaryDirectory.appending(components: UUID().uuidString, "Fixture.app")
        let resources = url.appending(components: "Contents", "Resources")
        try FileManager.default.createDirectory(at: resources, withIntermediateDirectories: true)
        let info: [String: Any] = ["CFBundleIdentifier": "app.rasm.fixture", "OSAScriptingDefinitionIsDynamic": dynamic].merging(name.map { name in ["OSAScriptingDefinition": name] } ?? [:]) {
            kept,
            _ in kept
        }
        try PropertyListSerialization.data(fromPropertyList: info, format: .xml, options: 0).write(to: url.appending(components: "Contents", "Info.plist"))
        try Data("static".utf8).write(to: resources.appending(component: "Fixture.sdef"))
        try Data("cached".utf8).write(to: resources.appending(component: "Cached.sdef"))
    }

    deinit {
        do { try FileManager.default.removeItem(at: url.deletingLastPathComponent()) } catch { Issue.record(error) }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite
struct ScriptingDefinitionTests {
    @Test(
        arguments: [
            ("Fixture", false, true, "static"),
            ("Fixture.sdef", false, false, "static"),
            ("Fixture.sdef", true, false, "undeclared"),
            (nil, true, true, "cached"),
        ] as [(String?, Bool, Bool, String)]
    )
    func readsTheDeclaredDefinitionElseTheCachedOneOfAnApplicationNotRunning(declared: String?, dynamic: Bool, cached: Bool, read: String) async throws {
        let bundle = try Fixture(declaring: declared, dynamic: dynamic)
        let cache = cached ? bundle.url.appending(components: "Contents", "Resources", "Cached.sdef") : nil
        let installation = try Installation(at: bundle.url)
        let outcome: String?
        do throws(AppleEventFailure) {
            outcome = String(bytes: try await installation.scriptingDefinition(cachedDefinition: cache), encoding: .utf8)
        } catch {
            outcome = error.reason.is(\.undeclared) ? "undeclared" : error.localizedDescription
        }
        expectNoDifference(outcome, read)
    }
}
