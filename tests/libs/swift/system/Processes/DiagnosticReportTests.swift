import CustomDump
import Foundation
import Testing

@testable import Processes

// --- [CONSTANTS] -----------------------------------------------------------------------

private let beta = "Adobe Photoshop (Beta)/Adobe Photoshop (Beta).app"
private let crash =
    #"{"bug_type":"309","name":"Adobe Photoshop 2027"}"# + "\n"
    + #"{"pid":4242,"captureTime":"2026-11-25 23:32:24.5338 -0600","procPath":"<root>/\#(beta)/Contents/MacOS/Adobe Photoshop 2027","procLaunch":"2026-11-25 23:30:00.6484 -0600","bundleInfo":{"CFBundleIdentifier":"com.adobe.Photoshop"}}"#
private let fault =
    #"{"is_simulated":1,"bug_type":"309","name":"nsattributedstringagent"}"# + "\n"
    + #"{"pid":97527,"captureTime":"2026-11-25 23:32:24.5338 -0600","isSimulated":1,"procName":"nsattributedstringagent","exception":{"type":"EXC_GUARD"}}"#
private let hang =
    "Date/Time:        2026-11-25 09:04:18.118 -0600\nCommand:          Atelier\nPath:             /Users/USER/*/Atelier\nIdentifier:       app.rasm.atelier\nPID:              4343\n\nEvent:            hang\n"
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
    let root = FileManager.default.temporaryDirectory.appending(component: UUID().uuidString).resolvingSymlinksInPath()

    init() throws { try FileManager.default.createDirectory(at: root, withIntermediateDirectories: true) }

    @Test(arguments: cases)
    func matchesExecutableOfOneInstallation(name: String, text: String, bundle: String, executable: String, identifier: String, written: Bool) throws {
        defer { remove() }
        let contents = root.appending(path: bundle).appending(component: "Contents")
        try FileManager.default.createDirectory(at: contents.appending(component: "MacOS"), withIntermediateDirectories: true)
        try PropertyListSerialization.data(fromPropertyList: [kCFBundleIdentifierKey as String: identifier, kCFBundleExecutableKey as String: executable], format: .xml, options: 0)
            .write(to: contents.appending(component: "Info.plist"))
        try Data().write(to: contents.appending(components: "MacOS", executable))
        try Data(text.replacing("<root>", with: root.path(percentEncoded: false)).utf8).write(to: root.appending(component: name))
        let parsed = try #require(try DiagnosticReport(root.appending(component: name)))
        expectNoDifference(parsed.isWritten(by: try #require(Bundle(url: root.appending(path: bundle)))), written)
    }

    @Test(arguments: [(4242, 1_795_671_000_648_493, true), (4242, 1_795_671_000_649_000, false), (4243, 1_795_671_000_648_493, false)])
    func matchesProcessByLaunchToTheMillisecond(pid: pid_t, started: UInt64, written: Bool) throws {
        defer { remove() }
        try Data(crash.utf8).write(to: root.appending(component: "crash.ips"))
        let parsed = try #require(try DiagnosticReport(root.appending(component: "crash.ips")))
        expectNoDifference(parsed.isWritten(by: ProcessInstance(pid: pid, started: started)), written)
    }

    @Test
    func readsSimulatedCrashAsNoCrash() throws {
        defer { remove() }
        try Data(fault.utf8).write(to: root.appending(component: "ExcUserFault_nsattributedstringagent.ips"))
        #expect(try DiagnosticReport(root.appending(component: "ExcUserFault_nsattributedstringagent.ips")) == nil)
    }

    @Test
    func listsReportsCapturedSinceNewestFirstWhateverTheFileTime() throws {
        defer { remove() }
        let earlier = hang.replacing("2026-11-25 09:04:18.118", with: "2026-11-20 09:04:18.118")
        for (name, text) in [("crash.ips", crash), ("hang.hang", hang), ("earlier.hang", earlier)] { try Data(text.utf8).write(to: root.appending(component: name)) }
        let found = DiagnosticReport.reports(since: try Date("2026-11-25T12:00:00Z", strategy: .iso8601), in: [root]) { _ in true }
        expectNoDifference(found.reports.map(\.url.lastPathComponent), ["crash.ips", "hang.hang"])
        expectNoDifference(found.failures, [])
    }

    private func remove() {
        do { try FileManager.default.removeItem(at: root) } catch { Issue.record(error) }
    }
}
