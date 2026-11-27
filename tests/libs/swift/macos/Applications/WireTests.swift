@unsafe @preconcurrency import ApplicationServices
import CoreGraphics
import CustomDump
import Encoding
import Foundation
import Processes
import Testing
import UniformTypeIdentifiers

@testable import Applications

// --- [OPERATIONS] ----------------------------------------------------------------------

private func written(_ value: some Encodable) throws -> String? {
    let encoder = JSONEncoder()
    encoder.outputFormatting = .sortedKeys
    return String(data: try encoder.encode(value), encoding: .utf8)
}

private func read<Value: Decodable>(_: Value.Type, _ text: String) throws -> Value { try JSONDecoder().decode(Value.self, from: Data(text.utf8)) }

@Suite
struct WireTests {
    @Test
    func writesEachAccessibilityFactWithItsTypeAsTag() throws {
        let facts: [Fact] = [.bool(value: false), .range(location: 2, length: 5), .list(values: [.element(id: 9), .text(value: "Untitled")]), .refused(error: Lenient(rawValue: -25_204))]
        expectNoDifference(
            try written(facts),
            #"[{"_tag":"bool","value":false},{"_tag":"range","length":5,"location":2},{"_tag":"list","values":[{"_tag":"element","id":9},{"_tag":"text","value":"Untitled"}]},{"_tag":"refused","error":-25204}]"#,
        )
    }

    @Test
    func readsEachValueTheTypeScriptSideWritesAsItsJSONType() throws {
        expectNoDifference(try read([Fact.Scalar].self, #"[true,1,1.5,"1.5"]"#), [.bool(true), .number(1), .number(1.5), .text("1.5")])
    }

    @Test(arguments: ["null", "[true]", #"{"value":true}"#])
    func refusesAValueThatIsNotABooleanNumberOrString(text: String) {
        #expect(throws: DecodingError.self) { try read(Fact.Scalar.self, text) }
    }

    @Test
    func writesRoutesPresentationsAndPasteboardContentWithTheirCaseAsTag() throws {
        expectNoDifference(try written([Route.window(number: 41), .system]), #"[{"_tag":"window","number":41},{"_tag":"system"}]"#)
        expectNoDifference(try written([Watch.Presentation.forward, .behind]), #"[{"_tag":"forward"},{"_tag":"behind"}]"#)
        expectNoDifference(try written([Pasteboard.Content.text(value: "copied"), .withheld]), #"[{"_tag":"text","value":"copied"},{"_tag":"withheld"}]"#)
    }

    @Test
    func writesEachInstanceChangeWithItsLabelsBesideItsTag() throws {
        let changes: [InstanceEvent.Change] = [
            .frontmost(active: false), .hidden(hidden: true), .started(identifier: nil, name: "Finder"), .status(status: .unresponsive(thread: .sleeping)), .status(status: .launching),
            .asking(container: 4, kind: .dialog, title: "Save", text: ["Save changes?"], answers: [], default: 7, cancel: 8, close: nil, secure: nil, presentation: nil),
            .announced(text: "Exported", priority: .high),
        ]
        expectNoDifference(
            try written(changes),
            #"[{"_tag":"frontmost","active":false},{"_tag":"hidden","hidden":true},{"_tag":"started","name":"Finder"},{"_tag":"status","status":{"_tag":"unresponsive","thread":"sleeping"}},{"_tag":"status","status":{"_tag":"launching"}},{"_tag":"asking","answers":[],"cancel":8,"container":4,"default":7,"kind":"dialog","text":["Save changes?"],"title":"Save"},{"_tag":"announced","priority":"high","text":"Exported"}]"#,
        )
    }

    @Test
    func writesAWindowChangeByItsCaseName() throws {
        let row: InstanceState.Window = try read(
            InstanceState.Window.self,
            #"{"number":43,"frame":[[10,30],[800,600]],"layer":0,"band":"normal","spaces":[1],"onScreen":true,"hidden":false,"shared":true,"asking":false,"presented":true}"#,
        )
        let written = try JSONSerialization.jsonObject(with: JSONEncoder().encode(InstanceEvent.Change.window(change: .moved, window: row))) as? [String: Any]
        expectNoDifference(written?["change"] as? String, "moved")
    }

    @Test
    func readsEachScopeTheTypeScriptSideWritesByItsTag() throws {
        let scopes: [Model.Scope] = try read(
            [Model.Scope].self,
            #"[{"_tag":"application"},{"_tag":"container","ref":4},{"_tag":"menu","ref":6},{"_tag":"rows","ref":8,"from":20,"count":10},{"_tag":"text","ref":5},{"_tag":"search","text":"Export","key":"AXHeadingSearchKey","limit":3},{"_tag":"search","text":"","limit":2},{"_tag":"menus"}]"#,
        )
        expectNoDifference(
            scopes.map(String.init(describing:)),
            [
                "application", "container(ref: 4)", "menu(ref: 6)", "rows(ref: 8, from: 20, count: 10)", "text(ref: 5)",
                #"search(text: "Export", key: __C.NSAccessibilitySearchKey(_rawValue: AXHeadingSearchKey), limit: 3)"#,
                #"search(text: "", key: __C.NSAccessibilitySearchKey(_rawValue: AXAnyTypeSearchKey), limit: 2)"#, "menus",
            ],
        )
    }

    @Test
    func readsEachCaptureSourceTheTypeScriptSideWritesByItsTag() throws {
        expectNoDifference(
            try read([Source].self, #"[{"_tag":"windows","numbers":[12,13]},{"_tag":"element","ref":5},{"_tag":"region","frame":[[10,20],[300,200]]}]"#),
            [.windows(numbers: [12, 13]), .element(ref: 5), .region(frame: CGRect(x: 10, y: 20, width: 300, height: 200))],
        )
    }

    @Test
    func writesEachStoredLocatorBackAsTheTextTheInterfaceMapKeysItBy() throws {
        let stored: [String] = [
            #"[{"key":{"identifier":{"value":"toolbar"}},"role":"AXWindow"},{"key":{"document":{"type":"public.plain-text"}},"role":"AXTextArea"}]"#,
            #"[{"key":{"label":{"value":"saveas"}},"role":"AXMenuItem"},{"key":{"sole":{}},"role":"AXMenu"},{"key":{"members":{}},"role":"AXRadioButton"}]"#,
            #"[{"key":{"offset":{"by":2}},"role":"AXButton"},{"key":{"offset":{"by":1,"from":{"key":{"sole":{}},"role":"AXGroup"}}},"role":"AXRow"}]"#,
        ]
        let locators: [Locator] = try read([Locator].self, String(decoding: try JSONEncoder().encode(stored), as: UTF8.self))
        expectNoDifference(try read([String].self, written(locators) ?? ""), stored)
    }

    @Test
    func keysEachSiblingByTheFirstKeyNoSiblingLeftAtItsLevelShares() {
        let steps: [Locator.Step] = Locator.Step.distinguish([
            (role: kAXButtonRole, keys: [.identifier(value: "save"), .label(value: "save"), .sole]), (role: kAXButtonRole, keys: [.label(value: "save"), .sole]),
            (role: kAXButtonRole, keys: [.sole]),
        ])
        expectNoDifference(steps.map(\.key), [.identifier(value: "save"), .label(value: "save"), .sole])
    }

    @Test
    func printsTwoDocumentsOfOneTypeAsThatTypeAloneWhileTheirStepsStayApart() throws {
        let urls: [URL] = ["A", "B"].map { name in URL(filePath: "/Users/Shared").appending(component: name).appendingPathExtension(for: .plainText) }
        let locators: [Locator] = Locator.Step.distinguish(urls.map { url in (role: kAXWindowRole, keys: [.document(at: url), .sole]) }).map { step in Locator(steps: [step]) }
        #expect(locators[0] != locators[1])
        expectNoDifference(try locators.map { locator in try locator.text() }, Array(repeating: #"[{"key":{"document":{"type":"public.plain-text"}},"role":"AXWindow"}]"#, count: 2))
    }

    @Test(arguments: [#"{"_tag":"window","ref":4}"#, #"{"container":{"ref":4}}"#])
    func refusesAScopeWithNoTagNamingOneOfItsCases(text: String) {
        #expect(throws: DecodingError.self) { try read(Model.Scope.self, text) }
    }
}
