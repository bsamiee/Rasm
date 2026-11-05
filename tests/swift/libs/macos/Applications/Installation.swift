import AppKit
import Applications
import CasePaths
import CoreServices
import CustomDump
import Encoding
import Foundation
import Results
import TestSupport
import Testing

// --- [CONSTANTS] -----------------------------------------------------------------------

private let bundles: [(info: [String: any Sendable], line: String)] = [
    (
        [
            "CFBundleIdentifier": "dev.rasm.installation.full", "CFBundleName": "Full", "CFBundleShortVersionString": "1.2.3", "NSAppleScriptEnabled": true,
            "OSAScriptingDefinition": "Full.sdef", "LSUIElement": true, "CFBundleVersion": "1.2.3.4",
        ],
        #"{"identifier":"dev.rasm.installation.full","name":"Full","version":"1.2.3","build":"1.2.3.4","scriptable":true,"scriptingDefinitionFile":"Full.sdef","agent":true}"#,
    ),
    (
        ["CFBundleIdentifier": "dev.rasm.installation.defaults", "CFBundleName": "Defaults"],
        #"{"identifier":"dev.rasm.installation.defaults","name":"Defaults","scriptable":false,"agent":false}"#,
    ),
    (
        ["CFBundleIdentifier": "dev.rasm.installation.spelled", "CFBundleName": "Spelled", "NSAppleScriptEnabled": "Yes", "OSAScriptingDefinition": "Spelled.sdef"],
        #"{"identifier":"dev.rasm.installation.spelled","name":"Spelled","scriptable":true,"scriptingDefinitionFile":"Spelled.sdef","agent":false}"#,
    ),
    (
        ["CFBundleIdentifier": "dev.rasm.installation.strings", "CFBundleName": "Strings", "LSUIElement": "1"],
        #"{"identifier":"dev.rasm.installation.strings","name":"Strings","scriptable":false,"agent":true}"#,
    ),
    (
        ["CFBundleIdentifier": "dev.rasm.installation.numbers", "CFBundleName": "Numbers", "LSUIElement": 1],
        #"{"identifier":"dev.rasm.installation.numbers","name":"Numbers","scriptable":false,"agent":true}"#,
    ),
]

// --- [OPERATIONS] ----------------------------------------------------------------------

private func bundle(named name: String, in folder: String, info: [String: Any]) throws -> URL {
    let directory: URL = try #require(TestDirectory.current)
    let url: URL = directory.appending(components: folder, "\(name).app", directoryHint: .isDirectory)
    let contents: URL = url.appending(component: "Contents", directoryHint: .isDirectory)
    try FileManager.default.createDirectory(at: contents.appending(component: "MacOS"), withIntermediateDirectories: true)
    try PropertyListSerialization.data(fromPropertyList: info, format: .xml, options: 0).write(to: contents.appending(component: "Info.plist"))
    return url
}

// --- [COMPOSITION] ---------------------------------------------------------------------

struct Installations {
    @Test(
        .testDirectory,
        arguments: bundles,
    )
    func installationReadsBundleInfo(info: [String: any Sendable], line: String) throws {
        let name: String = try #require(info["CFBundleName"] as? String)
        let url: URL = try bundle(named: name, in: "Applications", info: info)
        let installation: Installation = try Installation(at: url)
        let expected: JSONValue = try json(#"{"url":"\#(url.resolvingSymlinksInPath().absoluteString)",\#(line.dropFirst())"#)
        let kept: Set<String> = ["url", "identifier", "name", "version", "build", "scriptable", "scriptingDefinitionFile", "agent"]
        let encoded: JSONValue =
            switch try wire(installation) {
                case .object(let fields): .object(fields.filter { key, _ in kept.contains(key) })
                case let other: other
            }
        expectNoDifference(encoded, expected)
    }

    @Test(.testDirectory)
    func folderWithoutBundleInfoIsRejected() throws {
        let directory: URL = try #require(TestDirectory.current).appending(component: "Empty.app", directoryHint: .isDirectory)
        try FileManager.default.createDirectory(at: directory, withIntermediateDirectories: true)
        let failure: ApplicationFailure = try #require(throws: ApplicationFailure.self) { try Installation(at: directory) }
        #expect(failure.subject.contains("Empty.app"), "subject \(failure.subject) names rejected folder")
    }

    @Test(.testDirectory)
    func newestInstallationResolvesAndEqualVersionsNameEachPath() throws {
        let release: [String: Any] = ["CFBundleIdentifier": "dev.rasm.installation.twice", "CFBundleName": "Twice", "CFBundleShortVersionString": "21.6.0.57"]
        let first: Installation = try Installation(at: try bundle(named: "Twice", in: "First", info: release))
        let second: Installation = try Installation(
            at: try bundle(named: "Twice", in: "Second", info: release.merging(["CFBundleShortVersionString": "21.6.0.57.0"]) { _, next in next })
        )
        let beta: Installation = try Installation(
            at: try bundle(named: "Beta", in: "Third", info: ["CFBundleIdentifier": "dev.rasm.installation.beta", "CFBundleName": "Beta", "CFBundleShortVersionString": "22.0.0.101"])
        )
        expectNoDifference(try Installation.newest(of: "Twice", among: [first, beta]), beta)
        let missing: ApplicationFailure = try #require(throws: ApplicationFailure.self) { try Installation.newest(of: "Twice", among: []) }
        let absent: Unmatched = try #require(missing.reason[case: \.unmatched])
        expectNoDifference(absent.query, "Twice")
        let ambiguous: ApplicationFailure = try #require(throws: ApplicationFailure.self) { try Installation.newest(of: "Twice", among: [first, second]) }
        let unmatched: Unmatched = try #require(ambiguous.reason[case: \.unmatched])
        expectNoDifference(unmatched.subject, "single \(absent.subject)")
        expectNoDifference(Set(unmatched.candidates), [first.url.path(percentEncoded: false), second.url.path(percentEncoded: false)])
    }

    @Test(.timeLimit(.minutes(1)))
    func unknownNameFailsUnmatchedOnceSpotlightGathers() async throws {
        let name: String = "dev.rasm.absent.\(UUID().uuidString)"
        let failure: ApplicationFailure = try await #require(throws: ApplicationFailure.self) { try await Installation.installed(named: name) }
        let unmatched: Unmatched = try #require(failure.reason[case: \.unmatched])
        expectNoDifference(unmatched.query, name)
        expectNoDifference(unmatched.candidates, [])
    }

    @MainActor
    @Test(.timeLimit(.minutes(1)))
    func searchListsRegisteredBundleAtItsPath() async throws {
        let identifier: String = "com.apple.calculator"
        let registered: URL = try #require(NSWorkspace.shared.urlForApplication(withBundleIdentifier: identifier))
        let predicate: NSPredicate = NSPredicate(format: "%K == %@", kMDItemCFBundleIdentifier as String, identifier)
        let listing: Result<[Installation], AggregateError<ApplicationFailure>>? = await Installation.search(predicate, in: [registered.deletingLastPathComponent()]).first { @Sendable _ in true }
        expectNoDifference(try #require(listing).get(), [try Installation(at: registered)])
    }
}
