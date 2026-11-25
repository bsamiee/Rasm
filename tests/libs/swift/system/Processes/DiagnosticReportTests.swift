import CustomDump
import Foundation
import Testing

@testable import Processes

// --- [CONSTANTS] -----------------------------------------------------------------------

private let beta = "Adobe Photoshop (Beta)/Adobe Photoshop (Beta).app"
private let crash = #"{"bug_type":"309","bundleID":"com.adobe.Photoshop"}"# + "\n" + #"{"pid":4242,"procPath":"<root>/\#(beta)/Contents/MacOS/Adobe Photoshop 2027","procStartAbsTime":11388552083900}"#
private let hang = "Command: Atelier\nPath: /Users/USER/*/Atelier\nIdentifier: app.rasm.atelier\nPID: 4343\nEvent: hang\n"
private let cases: [(name: String, text: String, bundle: String, executable: String, identifier: String, written: Bool)] = [
    ("crash.ips", crash, "Adobe Photoshop 2027/Adobe Photoshop 2027.app", "Adobe Photoshop 2027", "com.adobe.Photoshop", false),
    ("crash.ips", crash, beta, "Adobe Photoshop 2027", "com.adobe.Photoshop", true),
    ("hang.hang", hang, "Atelier.app", "Atelier", "app.rasm.atelier", true),
    ("hang.hang", hang, "Atelier.app", "Atelier", "app.rasm.relay", false),
    ("hang.hang", hang, "Relay.app", "Relay", "app.rasm.atelier", false),
]

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite
struct DiagnosticReportTests {
    @Test(arguments: cases)
    func matchesExecutableOfOneInstallation(name: String, text: String, bundle: String, executable: String, identifier: String, written: Bool) throws {
        let root = FileManager.default.temporaryDirectory.appending(component: UUID().uuidString).resolvingSymlinksInPath()
        defer {
            do { try FileManager.default.removeItem(at: root) } catch { Issue.record(error) }
        }
        let contents = root.appending(path: bundle).appending(component: "Contents")
        try FileManager.default.createDirectory(at: contents.appending(component: "MacOS"), withIntermediateDirectories: true)
        try PropertyListSerialization.data(fromPropertyList: ["CFBundleIdentifier": identifier, "CFBundleExecutable": executable], format: .xml, options: 0)
            .write(to: contents.appending(component: "Info.plist"))
        try Data().write(to: contents.appending(components: "MacOS", executable))
        try Data(text.replacing("<root>", with: root.path(percentEncoded: false)).utf8).write(to: root.appending(component: name))
        let parsed = try #require(try DiagnosticReport(root.appending(component: name), written: .now))
        expectNoDifference(parsed.written(by: try #require(Bundle(url: root.appending(path: bundle)))), written)
    }
}
