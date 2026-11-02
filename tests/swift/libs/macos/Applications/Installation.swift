import Applications
import CasePaths
import CustomDump
import Encoding
import Foundation
import Results
import TestSupport
import Testing

// --- [CONSTANTS] -----------------------------------------------------------------------

private let bundles: [(info: [String: any Sendable], line: String, prohibited: Bool)] = [
    (
        [
            "CFBundleIdentifier": "dev.rasm.installation.full", "CFBundleName": "Full", "CFBundleShortVersionString": "1.2.3", "NSAppleScriptEnabled": true,
            "OSAScriptingDefinition": "Full.sdef", "LSUIElement": true, "LSMultipleInstancesProhibited": true,
        ],
        #"{"identifier":"dev.rasm.installation.full","name":"Full","version":"1.2.3","scriptable":true,"scriptingDefinitionFile":"Full.sdef","agent":true}"#, true,
    ),
    (
        ["CFBundleIdentifier": "dev.rasm.installation.bare", "CFBundleName": "Bare"],
        #"{"identifier":"dev.rasm.installation.bare","name":"Bare","scriptable":false,"agent":false}"#, false,
    ),
    (
        ["CFBundleIdentifier": "dev.rasm.installation.spelled", "CFBundleName": "Spelled", "NSAppleScriptEnabled": "Yes", "OSAScriptingDefinition": "Spelled.sdef"],
        #"{"identifier":"dev.rasm.installation.spelled","name":"Spelled","scriptable":true,"scriptingDefinitionFile":"Spelled.sdef","agent":false}"#, false,
    ),
    (
        ["CFBundleIdentifier": "dev.rasm.installation.strings", "CFBundleName": "Strings", "LSUIElement": "1", "LSMultipleInstancesProhibited": "YES"],
        #"{"identifier":"dev.rasm.installation.strings","name":"Strings","scriptable":false,"agent":true}"#, true,
    ),
    (
        ["CFBundleIdentifier": "dev.rasm.installation.numbers", "CFBundleName": "Numbers", "LSUIElement": 1, "LSMultipleInstancesProhibited": 0],
        #"{"identifier":"dev.rasm.installation.numbers","name":"Numbers","scriptable":false,"agent":true}"#, false,
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
    func installationReadsTheBundleInfo(info: [String: any Sendable], line: String, prohibited: Bool) throws {
        let name: String = try #require(info["CFBundleName"] as? String)
        let url: URL = try bundle(named: name, in: "Applications", info: info)
        let installation: Installation = try Installation(at: url)
        let expected: JSONValue = try json(#"{"url":"\#(url.resolvingSymlinksInPath().absoluteString)",\#(line.dropFirst())"#)
        let kept: Set<String> = ["url", "identifier", "name", "version", "scriptable", "scriptingDefinitionFile", "agent"]
        let encoded: JSONValue =
            switch try wire(installation) {
                case .object(let fields): .object(fields.filter { key, _ in kept.contains(key) })
                case let other: other
            }
        expectNoDifference(encoded, expected)
        expectNoDifference(installation.multipleInstancesProhibited, prohibited)
    }

    @Test(.testDirectory)
    func folderWithoutBundleInfoIsRejected() throws {
        let directory: URL = try #require(TestDirectory.current).appending(component: "Empty.app", directoryHint: .isDirectory)
        try FileManager.default.createDirectory(at: directory, withIntermediateDirectories: true)
        let failure: ApplicationFailure = try #require(throws: ApplicationFailure.self) { try Installation(at: directory) }
        #expect(failure.subject.contains("Empty.app"), "subject \(failure.subject) names the rejected folder")
    }

    @Test(.testDirectory)
    func singleInstallationOfAnIdentifierResolvesAndAmbiguityNamesEachPath() throws {
        let info: [String: Any] = ["CFBundleIdentifier": "dev.rasm.installation.twice", "CFBundleName": "Twice"]
        let first: Installation = try Installation(at: try bundle(named: "Twice", in: "First", info: info))
        let second: Installation = try Installation(at: try bundle(named: "Twice", in: "Second", info: info))
        let other: Installation = try Installation(at: try bundle(named: "Other", in: "Third", info: ["CFBundleIdentifier": "dev.rasm.installation.other", "CFBundleName": "Other"]))
        expectNoDifference(try Installation.single(of: "dev.rasm.installation.twice", among: [first, other]), first)
        let missing: ApplicationFailure = try #require(throws: ApplicationFailure.self) { try Installation.single(of: "dev.rasm.installation.twice", among: [other]) }
        let absent: Unmatched = try #require(missing.reason[case: \.unmatched])
        let named: Bool = absent.subject.hasSuffix("dev.rasm.installation.twice")
        #expect(named, "subject \(absent.subject) names the identifier")
        let ambiguous: ApplicationFailure = try #require(throws: ApplicationFailure.self) {
            try Installation.single(of: "dev.rasm.installation.twice", among: [first, second, other])
        }
        let unmatched: Unmatched = try #require(ambiguous.reason[case: \.unmatched])
        expectNoDifference(unmatched.subject, "single \(absent.subject)")
        expectNoDifference(Set(unmatched.candidates), [first.url.path(percentEncoded: false), second.url.path(percentEncoded: false)])
    }
}
