import Applications
import CasePaths
import CoreImage
import CustomDump
import Encoding
import Foundation
import JSONRPC
import Results
import Testing

@testable import ApplicationServer

// --- [MODELS] --------------------------------------------------------------------------

private struct Previewed: Decodable {
    let width: Int
    let height: Int
    let preview: Data
}

private struct Fixture: ~Copyable {
    let url: URL

    init() throws {
        url = FileManager.default.temporaryDirectory.appending(components: UUID().uuidString, "Fixture.app")
        let resources = url.appending(components: "Contents", "Resources")
        try FileManager.default.createDirectory(at: resources, withIntermediateDirectories: true)
        let info: [String: Any] = ["CFBundleIdentifier": "app.rasm.fixture", "OSAScriptingDefinitionIsDynamic": true]
        try PropertyListSerialization.data(fromPropertyList: info, format: .xml, options: 0).write(to: url.appending(components: "Contents", "Info.plist"))
        try Data(#"<dictionary title="Fixture"><suite name="Fixture" code="Fixt"/></dictionary>"#.utf8).write(to: resources.appending(component: "Cached.sdef"))
    }

    deinit {
        do { try FileManager.default.removeItem(at: url.deletingLastPathComponent()) } catch { Issue.record(error) }
    }
}

private let requests: [(String, [String: JSONValue])] = [
    ("terms", [:]),
    ("launch", ["documents": .array([]), "arguments": .array([]), "environment": .object([:]), "new": .bool(true), "opening": .object([:]), "wait": .bool(false)]),
]

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite
struct PreviewTests {
    @Test(
        arguments: [
            (CGSize(width: 96, height: 240), 60, CGSize(width: 24, height: 60)),
            (CGSize(width: 240, height: 96), 120, CGSize(width: 120, height: 48)),
            (CGSize(width: 96, height: 240), 500, CGSize(width: 96, height: 240)),
        ] as [(CGSize, Int, CGSize)]
    )
    func scalesTheLongestSideDownToTheLimitAndNeverUp(size: CGSize, longestSide: Int, previewed: CGSize) async throws {
        let file = FileManager.default.temporaryDirectory.appending(component: "\(UUID().uuidString).png")
        let space = try #require(CGColorSpace(name: CGColorSpace.sRGB))
        try CIContext().writePNGRepresentation(
            of: CIImage(color: CIColor(red: 0.8, green: 0.3, blue: 0.1)).cropped(to: CGRect(origin: .zero, size: size)),
            to: file,
            format: .RGBA8,
            colorSpace: space,
        )
        defer {
            do { try FileManager.default.removeItem(at: file) } catch { Issue.record(error) }
        }
        let request = try JSONDecoder().decode(Request.self, from: Data(#"{"preview":{"file":"\#(file.absoluteString)","longestSide":\#(longestSide)}}"#.utf8))
        guard case .value(let value?) = try await request.perform(admitting: { NSPredicate(value: false) }, definitions: ScriptingDefinitions(cached: { _ in nil })) else {
            Issue.record("No preview value")
            return
        }
        let answer = try JSONDecoder().decode(Previewed.self, from: try JSONEncoder().encode(value))
        expectNoDifference([CGSize(width: answer.width, height: answer.height), try #require(CIImage(data: answer.preview)).extent.size], [size, previewed])
    }

    @Test(arguments: [0, -60])
    func refusesALongestSideBelowOnePixel(longestSide: Int) throws {
        let refusal = #expect(throws: DecodingError.self) {
            try JSONDecoder().decode(Request.self, from: Data(#"{"preview":{"file":"file:///tmp/preview.png","longestSide":\#(longestSide)}}"#.utf8))
        }
        let path: [String]? = if case .dataCorrupted(let context)? = refusal { context.codingPath.map(\.stringValue) } else { nil }
        expectNoDifference(path, ["preview", "longestSide"])
    }
}

@Suite
struct AdmissionTests {
    @Test(arguments: requests)
    func refusesAnApplicationThePredicateDoesNotAdmitBeforeReadingIt(method: String, params: [String: JSONValue]) async throws {
        let bundle = try Fixture()
        let (reads, read) = AsyncStream<Void>.makeStream()
        let request = JSONValue.object([method: .object(params.merging(["url": .string(bundle.url.absoluteString)]) { kept, _ in kept })])
        let outcome = await Result {
            try await JSONDecoder().decode(Request.self, from: JSONEncoder().encode(request))
                .perform(
                    admitting: { NSPredicate(format: "kMDItemCFBundleIdentifier == %@", "app.rasm.other") },
                    definitions: ScriptingDefinitions(cached: { _ in
                        read.yield()
                        return nil
                    }),
                )
        }
        read.finish()
        let refusal: Unmatched? =
            switch outcome {
                case .success: nil
                case .failure(let failure): (failure as? ApplicationFailure)?.reason[case: \.unmatched]
            }
        expectNoDifference(refusal?.query, "app.rasm.fixture")
        let counted = await reads.reduce(0) { count, _ in count + 1 }
        expectNoDifference(counted, 0)
    }

    @Test
    func readsTheDefinitionOfAnAdmittedApplication() async throws {
        let bundle = try Fixture()
        let cached = bundle.url.appending(components: "Contents", "Resources", "Cached.sdef")
        let (asked, ask) = AsyncStream<String>.makeStream()
        let response = try await JSONDecoder().decode(Request.self, from: Data(#"{"terms":{"url":"\#(bundle.url.absoluteString)"}}"#.utf8))
            .perform(
                admitting: { NSPredicate(format: "kMDItemCFBundleIdentifier == %@", "app.rasm.fixture") },
                definitions: ScriptingDefinitions(cached: { installation in
                    ask.yield(installation.identifier)
                    return cached
                }),
            )
        ask.finish()
        let identifiers = await asked.reduce(into: [String]()) { identifiers, identifier in identifiers.append(identifier) }
        expectNoDifference(identifiers, ["app.rasm.fixture"])
        guard case .value(let value?) = response else {
            Issue.record("No terms value")
            return
        }
        let listing = try JSONDecoder().decode([String: JSONValue].self, from: JSONEncoder().encode(value))
        expectNoDifference(listing["suite"], .array([.object(["name": .string("Fixture"), "code": .string("Fixt")])]))
    }
}
