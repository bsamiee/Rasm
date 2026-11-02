import CoreGraphics
import CustomDump
import Encoding
import Foundation
import Processes
import Subprocess
import System
import Testing

@testable import Applications

// --- [CONSTANTS] -----------------------------------------------------------------------

private let sheet: Element = Element(
    id: Data([1, 2, 3]),
    window: 77,
    role: "AXSheet",
    subrole: nil,
    modal: nil,
    identifier: "_NS:87",
    description: "alert",
    title: nil,
    document: nil,
    edited: nil,
    minimized: nil,
    actions: [],
    requiresAnswer: true,
    detail: nil,
)

private let titled: Window = Window(number: 78, title: "Prompt", frame: CGRect(x: 1, y: 2, width: 3, height: 4), layer: 0, onScreen: true)
private let untitled: Window = Window(number: 79, title: nil, frame: CGRect(x: 0, y: 0, width: 5, height: 6), layer: 3, onScreen: false)
private let windows: String = #"[{"number":78,"title":"Prompt","frame":[[1,2],[3,4]],"layer":0,"onScreen":true},{"number":79,"frame":[[0,0],[5,6]],"layer":3,"onScreen":false}]"#

private let lines: [(state: InstanceState, line: String)] = [
    (.running(.launching, usage: nil), #"{"running":{"status":{"launching":{}}}}"#),
    (.running(.responding(elements: [], windows: []), usage: nil), #"{"running":{"status":{"responding":{"elements":[],"windows":[]}}}}"#),
    (
        .running(.responding(elements: [sheet], windows: [titled, untitled]), usage: nil),
        #"""
        {"running":{"status":{"responding":{"elements":[{"id":"AQID","window":77,"role":"AXSheet","identifier":"_NS:87","description":"alert","actions":[],"requiresAnswer":true}],
        "windows":\#(windows)}}}}
        """#,
    ),
    (.running(.unresponsive(process: .sleeping, windows: []), usage: nil), #"{"running":{"status":{"unresponsive":{"process":"sleeping","windows":[]}}}}"#),
    (.running(.unresponsive(process: .running, windows: [titled, untitled]), usage: nil), #"{"running":{"status":{"unresponsive":{"process":"running","windows":\#(windows)}}}}"#),
    (.running(.unresponsive(process: .stopped, windows: []), usage: nil), #"{"running":{"status":{"unresponsive":{"process":"stopped","windows":[]}}}}"#),
    (.exited(.success(.exited(0)), report: nil), #"{"exited":{"status":0}}"#),
    (.exited(.success(.exited(7)), report: nil), #"{"exited":{"status":7}}"#),
    (.exited(.success(.signaled(9)), report: nil), #"{"exited":{"signal":9}}"#),
    (.exited(.failure(.noSuchProcess), report: nil), #"{"exited":{"statusFailure":"\#(Errno.noSuchProcess.description)"}}"#),
]
private let statuses: [ApplicationStatus] = [.launching, .responding(elements: [sheet], windows: [titled]), .unresponsive(process: .running, windows: [untitled])]

// --- [COMPOSITION] ---------------------------------------------------------------------

struct InstanceStates {
    @Test(arguments: lines)
    func stateEncodesWireStateShape(state: InstanceState, line: String) throws {
        expectNoDifference(try wire(state), try json(line))
    }

    @Test
    func failedStateHoldsFailureText() throws {
        let failure: ApplicationFailure = ApplicationFailure(subject: "Probe", reason: .untrusted)
        let expected: JSONValue = .object(["failed": .object(["reason": .string(try #require(failure.errorDescription))])])
        expectNoDifference(try wire(InstanceState.failed(failure)), expected)
    }

    @Test
    func exitedStateHoldsItsReportOrReportFailure() throws {
        let report: CrashReport = try #require(CrashReport.reports(for: "dev.rasm.reports.application", in: crashReports).reports.first { report in report.pid == 4242 })
        let failure: ApplicationFailure = ApplicationFailure(subject: "process 4242", reason: .absent("crash report"))
        let url: String = crashReports.appending(component: "Application-2026-11-01-120000.ips").absoluteString
        expectNoDifference(
            try wire(InstanceState.exited(.success(.signaled(11)), report: .success(report))),
            try json(#"{"exited":{"signal":11,"report":{"url":"\#(url)","pid":4242,"bundleIdentifier":"dev.rasm.reports.application","exception":"EXC_BAD_ACCESS"}}}"#),
        )
        expectNoDifference(
            try wire(InstanceState.exited(.success(.signaled(11)), report: .failure(failure))),
            .object(["exited": .object(["signal": .number(11), "reportFailure": .string(failure.localizedDescription)])]),
        )
    }

    @Test(arguments: statuses)
    func usageSitsBesideStatus(status: ApplicationStatus) throws {
        let usage: ResourceUsage = try ResourceUsage(of: getpid())
        let encoded: JSONValue = try wire(InstanceState.running(status, usage: usage))
        let expectedUsage: JSONValue = .object([
            "cpu": .number(Double(usage.cpu.components.seconds) + Double(usage.cpu.components.attoseconds) / 1e18), "footprint": .number(Double(usage.footprint)),
            "threads": .number(Double(usage.threads)),
        ])
        expectNoDifference(encoded, .object(["running": .object(["status": try wire(status), "usage": expectedUsage])]))
    }
}
