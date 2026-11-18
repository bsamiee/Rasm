import CustomDump
import Foundation
import System
import Testing

@testable import Processes

// --- [CONSTANTS] -----------------------------------------------------------------------

private let beta = "/Applications/Adobe Photoshop (Beta)/Adobe Photoshop (Beta).app/Contents/MacOS/Adobe Photoshop 2027"
private let release = "/Applications/Adobe Photoshop 2027/Adobe Photoshop 2027.app/Contents/MacOS/Adobe Photoshop 2027"
private let crash = #"{"bug_type":"309","bundleID":"com.adobe.Photoshop"}"# + "\n" + #"{"pid":4242,"procPath":"\#(beta)","procStartAbsTime":11388552083900}"#
private let hang = "Command: Atelier\nPath: /Users/USER/*/Atelier\nIdentifier: app.rasm.atelier\nPID: 4343\nEvent: hang\n"
private let atelier = "/Users/me/Applications/Atelier.app/Contents/MacOS/Atelier"
private let cases: [(name: String, text: String, executable: String, identifier: String, written: Bool)] = [
    ("crash.ips", crash, release, "com.adobe.Photoshop", false),
    ("crash.ips", crash, beta, "com.adobe.Photoshop", true),
    ("hang.hang", hang, atelier, "app.rasm.atelier", true),
    ("hang.hang", hang, atelier, "app.rasm.relay", false),
    ("hang.hang", hang, "/Users/me/Applications/Relay.app/Contents/MacOS/Relay", "app.rasm.atelier", false),
]

// --- [OPERATIONS] ----------------------------------------------------------------------

private func report(named name: String, holding text: String) throws -> DiagnosticReport? {
    let file = FileManager.default.temporaryDirectory.appending(components: UUID().uuidString, name)
    try FileManager.default.createDirectory(at: file.deletingLastPathComponent(), withIntermediateDirectories: true)
    defer {
        do { try FileManager.default.removeItem(at: file.deletingLastPathComponent()) } catch { Issue.record(error) }
    }
    try Data(text.utf8).write(to: file)
    return try DiagnosticReport(file, written: .now)
}

@Suite
struct DiagnosticReportTests {
    @Test(arguments: cases)
    func matchesExecutableOfOneInstallation(name: String, text: String, executable: String, identifier: String, written: Bool) throws {
        let parsed = try #require(try report(named: name, holding: text))
        expectNoDifference(parsed.written(by: FilePath(executable), identifier: identifier), written)
    }
}
