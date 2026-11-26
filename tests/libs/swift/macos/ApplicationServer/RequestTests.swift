import AppKit
import Applications
import CasePaths
import CoreImage
import CustomDump
import Encoding
import Foundation
import JSONRPC
import Results
import ScreenCapture
import Testing

@testable import ApplicationServer

// --- [MODELS] --------------------------------------------------------------------------

private struct Scaled: Decodable {
    let png: Data
    let width: Int
    let height: Int
}

private struct Listed: Decodable {
    struct Row: Decodable {
        let url: URL
    }

    let installations: [Row]
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
    ("dictionary", [:]),
    ("launch", ["documents": .array([]), "arguments": .array([]), "environment": .object([:]), "new": .bool(true), "opening": .object([:]), "wait": .bool(false)]),
]

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite
struct ScaleTests {
    @Test(
        arguments: [
            (CGSize(width: 96, height: 240), 60, CGSize(width: 24, height: 60)),
            (CGSize(width: 240, height: 96), 120, CGSize(width: 120, height: 48)),
            (CGSize(width: 96, height: 240), 500, CGSize(width: 96, height: 240)),
        ] as [(CGSize, Int, CGSize)]
    )
    func scalesTheLongestSideDownToTheLimitAndNeverUp(size: CGSize, longestSide: Int, scaled: CGSize) async throws {
        let space = try #require(CGColorSpace(name: CGColorSpace.sRGB))
        let drawn = CIImage(color: CIColor(red: 0.8, green: 0.3, blue: 0.1)).cropped(to: CGRect(origin: .zero, size: size))
        let encoded: Data? = CIContext().pngRepresentation(of: drawn, format: .RGBA8, colorSpace: space)
        let image: Data = try #require(encoded)
        let request = try JSONDecoder().decode(Request.self, from: Data(#"{"scale":{"image":"\#(image.base64EncodedString())","longestSide":\#(longestSide)}}"#.utf8))
        guard
            case .value(let value?) = try await request.perform(
                matching: { NSPredicate(value: false) },
                dictionaries: ScriptingDictionaries(cachedDefinition: { _ in nil }),
                watches: Watches(recognize: { watched, window throws(ApplicationFailure) in try await watched.recognizeText(in: window) }),
            )
        else {
            Issue.record("No scaled value")
            return
        }
        let answer = try JSONDecoder().decode(Scaled.self, from: try JSONEncoder().encode(value))
        let decoded: CIImage? = CIImage(data: answer.png)
        let drawnSize: CGSize = try #require(decoded).extent.size
        expectNoDifference([CGSize(width: answer.width, height: answer.height), drawnSize], [scaled, scaled])
    }

    @Test(arguments: [0, -60])
    func refusesALongestSideBelowOnePixel(longestSide: Int) throws {
        let refusal = #expect(throws: DecodingError.self) {
            try JSONDecoder().decode(Request.self, from: Data(#"{"scale":{"image":"","longestSide":\#(longestSide)}}"#.utf8))
        }
        let path: [String]? = if case .dataCorrupted(let context)? = refusal { context.codingPath.map(\.stringValue) } else { nil }
        expectNoDifference(path, ["scale", "longestSide"])
    }
}

@Suite(.timeLimit(.minutes(1)))
struct InstallationsTests {
    @Test
    func listsTheBundleOfEveryRegularApplicationRunningThroughoutTheListing() async throws {
        let running = {
            Set(
                NSWorkspace.shared.runningApplications.filter { application in application.activationPolicy == .regular }.compactMap { application in
                    application.bundleURL?.resolvingSymlinksInPath().pathComponents
                }
            )
        }
        let before = running()
        let request = try JSONDecoder().decode(Request.self, from: Data(#"{"installations":{}}"#.utf8))
        guard
            case .stream(let listings) = try await request.perform(
                matching: { NSPredicate(value: true) },
                dictionaries: ScriptingDictionaries(cachedDefinition: { _ in nil }),
                watches: Watches(recognize: { watched, window throws(ApplicationFailure) in try await watched.recognizeText(in: window) }),
            )
        else {
            Issue.record("No installations stream")
            return
        }
        let listing = try #require(await listings.first { _ in true })
        let listed = try JSONDecoder().decode(Listed.self, from: JSONEncoder().encode(listing))
        let throughout = before.intersection(running())
        try #require(!throughout.isEmpty)
        expectNoDifference(throughout.subtracting(listed.installations.map(\.url.pathComponents)), [])
    }
}

@Suite
struct MatchingTests {
    @Test(arguments: requests)
    func refusesAnApplicationThePredicateDoesNotMatchBeforeReadingIt(method: String, params: [String: JSONValue]) async throws {
        let bundle = try Fixture()
        let (reads, read) = AsyncStream<Void>.makeStream()
        let request = JSONValue.object([method: .object(params.merging(["url": .string(bundle.url.absoluteString)]) { kept, _ in kept })])
        let outcome = await Result {
            try await JSONDecoder().decode(Request.self, from: JSONEncoder().encode(request))
                .perform(
                    matching: { NSPredicate(format: "kMDItemCFBundleIdentifier == %@", "app.rasm.other") },
                    dictionaries: ScriptingDictionaries(cachedDefinition: { _ in
                        read.yield()
                        return nil
                    }),
                    watches: Watches(recognize: { watched, window throws(ApplicationFailure) in try await watched.recognizeText(in: window) }),
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
    func readsTheDefinitionOfAMatchingApplication() async throws {
        let bundle = try Fixture()
        let cached = bundle.url.appending(components: "Contents", "Resources", "Cached.sdef")
        let (asked, ask) = AsyncStream<String>.makeStream()
        let response = try await JSONDecoder().decode(Request.self, from: Data(#"{"dictionary":{"url":"\#(bundle.url.absoluteString)"}}"#.utf8))
            .perform(
                matching: { NSPredicate(format: "kMDItemCFBundleIdentifier == %@", "app.rasm.fixture") },
                dictionaries: ScriptingDictionaries(cachedDefinition: { installation in
                    ask.yield(installation.identifier)
                    return cached
                }),
                watches: Watches(recognize: { watched, window throws(ApplicationFailure) in try await watched.recognizeText(in: window) }),
            )
        ask.finish()
        let identifiers = await asked.reduce(into: [String]()) { identifiers, identifier in identifiers.append(identifier) }
        expectNoDifference(identifiers, ["app.rasm.fixture"])
        guard case .value(let value?) = response else {
            Issue.record("No dictionary value")
            return
        }
        let listing = try JSONDecoder().decode([String: JSONValue].self, from: JSONEncoder().encode(value))
        expectNoDifference(listing["suite"], .array([.object(["name": .string("Fixture"), "code": .string("Fixt")])]))
    }
}
