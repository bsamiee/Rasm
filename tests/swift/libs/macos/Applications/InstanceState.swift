import ApplicationServices
import CustomDump
import Encoding
import Foundation
import Subprocess
import System
import Testing

@testable import Applications
@testable import Processes

// --- [CONSTANTS] -----------------------------------------------------------------------

private let sheet: Element = Element(
    id: Data([1, 2, 3]),
    window: 77,
    role: kAXSheetRole,
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
    (
        .running(InstanceState.Running(status: .launching(windows: [titled]), usage: nil)),
        #"{"running":{"status":{"launching":{"windows":[{"number":78,"title":"Prompt","frame":[[1,2],[3,4]],"layer":0,"onScreen":true}]}},"next":{"wait":{}}}}"#,
    ),
    (
        .running(InstanceState.Running(status: .responding(elements: [], windows: []), usage: nil)),
        #"{"running":{"status":{"responding":{"elements":[],"windows":[]}},"next":{"drive":{}}}}"#,
    ),
    (
        .running(InstanceState.Running(status: .responding(elements: [sheet], windows: [titled, untitled]), usage: nil)),
        #"""
        {"running":{"status":{"responding":{"elements":[{"id":"AQID","window":77,"role":"AXSheet","identifier":"_NS:87","description":"alert","actions":[],"requiresAnswer":true}],
        "windows":\#(windows)}},"next":{"answer":{"id":"AQID","buttons":[]}}}}
        """#,
    ),
    (
        .running(InstanceState.Running(status: .unresponsive(process: .sleeping, windows: []), usage: nil)),
        #"{"running":{"status":{"unresponsive":{"process":"sleeping","windows":[]}},"next":{"wait":{}}}}"#,
    ),
    (
        .running(InstanceState.Running(status: .unresponsive(process: .running, windows: [titled, untitled]), usage: nil)),
        #"{"running":{"status":{"unresponsive":{"process":"running","windows":\#(windows)}},"next":{"wait":{}}}}"#,
    ),
    (
        .running(InstanceState.Running(status: .unresponsive(process: .stopped, windows: []), usage: nil)),
        #"{"running":{"status":{"unresponsive":{"process":"stopped","windows":[]}},"next":{"resume":{}}}}"#,
    ),
    (.exited(.success(ProcessExit(status: .exited(0), detail: [])), report: nil), #"{"exited":{"status":0}}"#),
    (.exited(.success(ProcessExit(status: .exited(7), detail: [])), report: nil), #"{"exited":{"status":7}}"#),
    (.exited(.success(ProcessExit(status: .signaled(9), detail: .memory)), report: nil), #"{"exited":{"signal":9,"detail":["memory"]}}"#),
    (.exited(.failure(.noSuchProcess), report: nil), #"{"exited":{"statusFailure":"\#(Errno.noSuchProcess.description)"}}"#),
]
private let statuses: [ApplicationStatus] = [
    .launching(windows: []), .responding(elements: [sheet], windows: [titled]), .unresponsive(process: .running, windows: [untitled]),
]

// --- [COMPOSITION] ---------------------------------------------------------------------

struct InstanceStates {
    @Test(arguments: lines)
    func stateEncodesWireStateShape(state: InstanceState, line: String) throws {
        expectNoDifference(try wire(state), try json(line))
    }

    @Test
    func failedStateHoldsFailureFieldsAndNext() throws {
        let failure: ApplicationFailure = ApplicationFailure(subject: "Probe", reason: .untrusted)
        let expected: JSONValue = .object([
            "failed": .object([
                "reason": .string("untrusted"), "subject": .string("Probe"), "message": .string(try #require(failure.errorDescription)), "next": .object(["grant": .object([:])]),
            ])
        ])
        expectNoDifference(try wire(InstanceState.failed(failure)), expected)
    }

    @Test
    func exitedStateHoldsItsReportOrReportFailure() throws {
        let report: CrashReport = try #require(CrashReport.reports(for: "dev.rasm.reports.application", in: crashReports).reports.first { report in report.pid == 4242 })
        let failure: ApplicationFailure = ApplicationFailure(subject: "4242", reason: .noReport(.seconds(60)))
        let exit: ProcessExit = ProcessExit(status: .signaled(11), detail: [])
        let url: String = crashReports.appending(component: "Application-2026-11-01-120000.ips").absoluteString
        expectNoDifference(
            try wire(InstanceState.exited(.success(exit), report: .success(report))),
            try json(
                #"{"exited":{"signal":11,"report":{"url":"\#(url)","pid":4242,"bundleIdentifier":"dev.rasm.reports.application","exception":"EXC_BAD_ACCESS"},"next":{"report":{"url":"\#(url)"}}}}"#
            ),
        )
        expectNoDifference(
            try wire(InstanceState.exited(.success(exit), report: .failure(failure))),
            .object(["exited": .object(["signal": .number(11), "reportFailure": .string(failure.name)])]),
        )
    }

    @Test(arguments: statuses)
    func usageSitsBesideStatus(status: ApplicationStatus) throws {
        let usage: ResourceUsage = try ResourceUsage(of: getpid())
        let sampled: InstanceState = .running(InstanceState.Running(status: status, usage: .success(usage)))
        let next: JSONValue = try wire(try #require(sampled.next))
        let expectedUsage: JSONValue = .object([
            "cpu": .number(Double(usage.cpu.components.seconds) + Double(usage.cpu.components.attoseconds) / 1e18), "footprint": .number(Double(usage.footprint)),
            "threads": .number(Double(usage.threads)),
        ])
        expectNoDifference(try wire(sampled), .object(["running": .object(["status": try wire(status), "next": next, "usage": expectedUsage])]))
        expectNoDifference(
            try wire(InstanceState.running(InstanceState.Running(status: status, usage: .failure(.noSuchProcess)))),
            .object(["running": .object(["status": try wire(status), "next": next, "usageFailure": .string(Errno.noSuchProcess.description)])]),
        )
    }
}
