import AppIntents
import AppKit
import CustomDump
import Testing

@testable import Applications

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite
struct InstallationTests {
    static func declare(_ entries: String, in bundle: URL) throws -> URL {
        try FileManager.default.createDirectory(at: bundle.appending(component: "Contents"), withIntermediateDirectories: true)
        try Data(#"<?xml version="1.0" encoding="UTF-8"?><plist version="1.0"><dict><key>CFBundleIdentifier</key><string>app.rasm.fixture</string>\#(entries)</dict></plist>"#.utf8)
            .write(to: bundle.appending(components: "Contents", "Info.plist"))
        return bundle
    }

    @Test(
        arguments: [
            ("", .regular),
            ("<key>LSUIElement</key><true/>", .accessory),
            ("<key>LSUIElement</key><integer>1</integer>", .accessory),
            ("<key>LSUIElement</key><string>YES</string>", .accessory),
            ("<key>LSUIElement</key><string>0</string>", .regular),
            ("<key>LSBackgroundOnly</key><string>1</string><key>LSUIElement</key><true/>", .prohibited),
        ] as [(String, NSApplication.ActivationPolicy)]
    )
    func readsTheActivationPolicyOfEachBooleanFormLaunchServicesReads(entries: String, policy: NSApplication.ActivationPolicy) throws {
        let bundle = try Self.declare(entries, in: FileManager.default.temporaryDirectory.appending(components: UUID().uuidString, "Fixture.app"))
        let read = try Installation(at: bundle).activationPolicy
        try FileManager.default.removeItem(at: bundle.deletingLastPathComponent())
        expectNoDifference(read, policy)
    }

    @Test
    func readsTheVersionAnUpdateWritesAfterAnEarlierReadOfTheSameBundle() throws {
        let bundle = try Self.declare("<key>CFBundleShortVersionString</key><string>1</string>", in: FileManager.default.temporaryDirectory.appending(components: UUID().uuidString, "Fixture.app"))
        let before = try Installation(at: bundle).version
        let after = try Installation(at: Self.declare("<key>CFBundleShortVersionString</key><string>2</string>", in: bundle)).version
        try FileManager.default.removeItem(at: bundle.deletingLastPathComponent())
        expectNoDifference([before, after], ["1", "2"])
    }

    @Test
    func listsTheAppIntentsOfTheBundleItsExtensionsAndThePackagesItIncludesAlone() throws {
        let bundle = try Self.declare("", in: FileManager.default.temporaryDirectory.appending(components: UUID().uuidString, "Fixture.app"))
        let contents = bundle.appending(component: "Contents")
        let files: [(URL, String)] = [
            (
                contents.appending(components: "Resources", "Metadata.appintents", "extract.actionsdata"),
                #"{"actions":{"Open":{"identifier":"Open","title":{"key":"OPEN_TITLE","table":"AppIntents","alternatives":[]},"openAppWhenRun":true,"supportedModes":2,"isDiscoverable":true,"parameters":[]}}}"#,
            ),
            (contents.appending(components: "Resources", "Metadata.appintents", "extract.packagedata"), #"{"version":1,"includes":["8Included0aB7PackageV"]}"#),
            (contents.appending(components: "Resources", "en.lproj", "AppIntents.strings"), #""OPEN_TITLE" = "Open Item";"#),
            (
                contents.appending(components: "PlugIns", "Widget.appex", "Contents", "Resources", "Metadata.appintents", "extract.actionsdata"),
                #"{"actions":{"Configure":{"identifier":"Configure","openAppWhenRun":false,"supportedModes":8,"isDiscoverable":false,"parameters":[]}}}"#,
            ),
            (
                contents.appending(components: "Frameworks", "Included.framework", "Resources", "Metadata.appintents", "extract.actionsdata"),
                #"{"actions":{"Play":{"identifier":"Play","openAppWhenRun":false,"parameters":[{"name":"item","isOptional":true}]}}}"#,
            ),
            (
                contents.appending(components: "Frameworks", "Excluded.framework", "Resources", "Metadata.appintents", "extract.actionsdata"),
                #"{"actions":{"Probe":{"identifier":"Probe","openAppWhenRun":true,"supportedModes":2,"parameters":[]}}}"#,
            ),
        ]
        for (file, text) in files {
            try FileManager.default.createDirectory(at: file.deletingLastPathComponent(), withIntermediateDirectories: true)
            try Data(text.utf8).write(to: file)
        }
        let intents = try Installation(at: bundle).appIntents
        try FileManager.default.removeItem(at: bundle.deletingLastPathComponent())
        expectNoDifference(intents.map(\.identifier), ["Open", "Configure", "Play"])
        expectNoDifference(intents.map(\.title), ["Open Item", nil, nil])
        expectNoDifference(intents.map(\.supportedModes), [.foreground, .foreground(.dynamic), .background])
        expectNoDifference(intents.map(\.isDiscoverable), [true, false, true])
    }

    @Test
    func listsABundleWhoseAppIntentsMetadataIsUnreadableWithTheFailureInPlaceOfTheActions() throws {
        let bundle = try Self.declare("<key>CFBundleShortVersionString</key><string>3</string>", in: FileManager.default.temporaryDirectory.appending(components: UUID().uuidString, "Fixture.app"))
        let actions = bundle.appending(components: "Contents", "Resources", "Metadata.appintents", "extract.actionsdata")
        try FileManager.default.createDirectory(at: actions.deletingLastPathComponent(), withIntermediateDirectories: true)
        try Data(#"{"actions":"#.utf8).write(to: actions)
        let installation = try Installation(at: bundle)
        let encoded = try JSONSerialization.jsonObject(with: JSONEncoder().encode(installation)) as? [String: Any]
        let failure = #expect(throws: ApplicationFailure.self) { try installation.appIntents }
        try FileManager.default.removeItem(at: bundle.deletingLastPathComponent())
        expectNoDifference(installation.version, "3")
        expectNoDifference(failure?.subject.hasSuffix("Metadata.appintents/"), true)
        expectNoDifference(encoded?["appIntents"] == nil, true)
        expectNoDifference((encoded?["unread"] as? [[String: Any]])?.compactMap { unread in unread["subject"] as? String }, [failure?.subject].compactMap(\.self))
    }
}
