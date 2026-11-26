import AppKit
import AppleEvents
import CasePathsCore
import CustomDump
import Foundation
import Testing

@testable import Applications

// --- [MODELS] --------------------------------------------------------------------------

private struct Fixture: ~Copyable {
    let url: URL

    init(declaring name: String?, dynamic: Bool, terminology: URL? = nil) throws {
        url = FileManager.default.temporaryDirectory.appending(components: UUID().uuidString, "Fixture.app")
        let (contents, resources) = (url.appending(component: "Contents"), url.appending(components: "Contents", "Resources"))
        try FileManager.default.createDirectory(at: resources, withIntermediateDirectories: true)
        try FileManager.default.createDirectory(at: contents.appending(component: "MacOS"), withIntermediateDirectories: true)
        let info: [String: Any] = ["CFBundleIdentifier": "app.rasm.fixture", "CFBundleExecutable": "Fixture", "OSAScriptingDefinitionIsDynamic": dynamic].merging(
            name.map { name in ["OSAScriptingDefinition": name] } ?? [:]
        ) { kept, _ in kept }
        try PropertyListSerialization.data(fromPropertyList: info, format: .xml, options: 0).write(to: contents.appending(component: "Info.plist"))
        try Data().write(to: contents.appending(components: "MacOS", "Fixture"))
        try FileManager.default.setAttributes([.posixPermissions: 0o755], ofItemAtPath: contents.appending(components: "MacOS", "Fixture").path(percentEncoded: false))
        try Data("static".utf8).write(to: resources.appending(component: "Fixture.sdef"))
        try Data("cached".utf8).write(to: resources.appending(component: "Cached.sdef"))
        if let terminology { try FileManager.default.copyItem(at: terminology, to: resources.appending(component: "Fixture.rsrc")) }
    }

    deinit {
        do { try FileManager.default.removeItem(at: url.deletingLastPathComponent()) } catch { Issue.record(error) }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite
struct ScriptingDictionariesTests {
    @Test(
        arguments: [
            ("Fixture", false, false, true, "static"),
            ("Fixture.sdef", false, false, false, "static"),
            ("Fixture.sdef", true, false, false, "static"),
            ("Fixture.sdef", true, false, true, "cached"),
            (nil, true, true, true, "cached"),
            (nil, false, false, false, "undeclared"),
        ] as [(String?, Bool, Bool, Bool, String)]
    )
    func readsTheCachedDefinitionOfADynamicApplicationNotRunningBeforeItsStaticFiles(declared: String?, dynamic: Bool, terminology: Bool, cached: Bool, read: String) async throws {
        let speech = URL(filePath: "/System/Library/PrivateFrameworks/SpeechObjects.framework/Versions/A/SpeechRecognitionServer.app/Contents/Resources/SpeechRecognitionServer.rsrc")
        let bundle = try Fixture(declaring: declared, dynamic: dynamic, terminology: terminology ? speech : nil)
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

    @Test
    func translatesTheScriptSuitesOfAnApplicationWithNoStaticDefinitionWithoutLaunchingIt() async throws {
        let installation = try Installation(at: URL(filePath: "/System/Applications/TextEdit.app"))
        let running = NSWorkspace.shared.runningApplications.count(where: installation.runs)
        let dictionary = try ScriptingDictionary(definition: try await installation.scriptingDefinition(cachedDefinition: nil))
        let close = try #require(dictionary.commands.first { command in command.name == "close" })
        let document = try #require(dictionary.declarations[.class]?.first { term in term.name == "document" })
        let saving = try #require(dictionary.declarations[.enumeration]?.first { term in term.name == "savo" })
        expectNoDifference(close.code.sdef, "coreclos")
        expectNoDifference(Set(close.parameters.map { term in "\(term.name) \(term.code.sdef) \(term.types.sorted())" }), ["saving savo [\"savo\"]", "saving in kfil [\"alias\"]"])
        expectNoDifference(Set(document.members.map { term in "\(term.name) \(term.code.sdef)" }), ["class pcls", "properties pALL", "modified imod", "name pnam", "path ppth", "text ctxt"])
        expectNoDifference(Set(saving.members.map { term in "\(term.name) \(term.code.sdef)" }), ["ask ask ", "no no  ", "yes yes "])
        expectNoDifference(dictionary.declarations[.class]?.first { term in term.name == "paragraph" }?.code.sdef, "cpar")
        expectNoDifference(NSWorkspace.shared.runningApplications.count(where: installation.runs), running)
    }

    @Test
    func namesEachCocoaTypeForTheCodeFoundationRegistersForIt() throws {
        let events = try #require(Bundle(url: URL(filePath: "/System/Library/CoreServices/Database Events.app")))
        let dictionary = try ScriptingDictionary(definition: try ScriptingDictionary.definition(suites: events))
        let properties = dictionary.declarations[.class, default: []].flatMap(\.members).filter { term in ["qdel", "iloc"].contains(term.code.sdef) }
        expectNoDifference(Set(properties.map { term in "\(term.name) \(term.types.sorted())" }), ["quit delay [\"unsigned integer\"]", "location [\"alias\"]"])
    }

    @Test
    func translatesTheAeteResourceNamedForTheExecutable() async throws {
        let speech = URL(filePath: "/System/Library/PrivateFrameworks/SpeechObjects.framework/Versions/A/SpeechRecognitionServer.app/Contents/Resources/SpeechRecognitionServer.rsrc")
        let bundle = try Fixture(declaring: nil, dynamic: false, terminology: speech)
        let dictionary = try ScriptingDictionary(definition: try await Installation(at: bundle.url).scriptingDefinition(cachedDefinition: nil))
        let listen = try #require(dictionary.commands.first { command in command.name == "listen for" })
        let direct = try #require(listen.parameters.first { term in term.name == "direct" })
        expectNoDifference(listen.code.sdef, "sprcsrls")
        expectNoDifference("\(direct.types.sorted()) \(direct.list)", "[\"text\"] true")
        expectNoDifference(listen.parameters.first { term in term.name == "giving up after" }.map { term in "\(term.code.sdef) \(term.types.sorted())" }, "ktmo [\"integer\"]")
    }
}
