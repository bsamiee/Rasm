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

private let lines: [(state: InstanceState, line: String)] = [
    (.running(.launching, usage: nil), #"{"running":{"status":{"launching":{}}}}"#),
    (.running(.responding(elements: []), usage: nil), #"{"running":{"status":{"responding":{"elements":[]}}}}"#),
    (
        .running(.responding(elements: [sheet]), usage: nil),
        #"{"running":{"status":{"responding":{"elements":[{"id":"AQID","window":77,"role":"AXSheet","identifier":"_NS:87","description":"alert","actions":[],"requiresAnswer":true}]}}}}"#,
    ),
    (.running(.unresponsive(process: .sleeping), usage: nil), #"{"running":{"status":{"unresponsive":{"process":"sleeping"}}}}"#),
    (.running(.unresponsive(process: .running), usage: nil), #"{"running":{"status":{"unresponsive":{"process":"running"}}}}"#),
    (.running(.unresponsive(process: .stopped), usage: nil), #"{"running":{"status":{"unresponsive":{"process":"stopped"}}}}"#),
    (.exited(.success(.exited(0)), report: nil), #"{"exited":{"status":0}}"#),
    (.exited(.success(.exited(7)), report: nil), #"{"exited":{"status":7}}"#),
    (.exited(.success(.signaled(9)), report: nil), #"{"exited":{"signal":9}}"#),
    (.exited(.failure(.noSuchProcess), report: nil), #"{"exited":{"statusFailure":"\#(Errno.noSuchProcess.description)"}}"#),
]
private let statuses: [ApplicationStatus] = [.launching, .responding(elements: [sheet]), .unresponsive(process: .running)]

// --- [COMPOSITION] ---------------------------------------------------------------------

struct InstanceStates {
    @Test(arguments: lines)
    func stateEncodesTheWireStateShape(state: InstanceState, line: String) throws {
        expectNoDifference(try wire(state), try json(line))
    }

    @Test
    func failedStateCarriesTheFailureText() throws {
        let failure: ApplicationFailure = ApplicationFailure(subject: "Probe", reason: .untrusted)
        let expected: JSONValue = .object(["failed": .object(["reason": .string(try #require(failure.errorDescription))])])
        expectNoDifference(try wire(InstanceState.failed(failure)), expected)
    }

    @Test
    func exitedStateCarriesItsReportOrTheReportFailure() throws {
        let report: CrashReport = try #require(CrashReport.reports(for: "dev.rasm.reports.alpha", in: crashReports).reports.first { report in report.pid == 4242 })
        let failure: ApplicationFailure = ApplicationFailure(subject: "process 4242", reason: .absent("crash report"))
        let url: String = crashReports.appending(component: "Alpha-2026-11-01-120000.ips").absoluteString
        expectNoDifference(
            try wire(InstanceState.exited(.success(.signaled(11)), report: .success(report))),
            try json(#"{"exited":{"signal":11,"report":{"url":"\#(url)","pid":4242,"bundleIdentifier":"dev.rasm.reports.alpha","exception":"EXC_BAD_ACCESS"}}}"#),
        )
        expectNoDifference(
            try wire(InstanceState.exited(.success(.signaled(11)), report: .failure(failure))),
            .object(["exited": .object(["signal": .number(11), "reportFailure": .string(failure.localizedDescription)])]),
        )
    }

    @Test(arguments: statuses)
    func usageSitsBesideTheStatus(status: ApplicationStatus) throws {
        let usage: ResourceUsage = try ResourceUsage(of: getpid())
        let encoded: JSONValue = try wire(InstanceState.running(status, usage: usage))
        let expectedUsage: JSONValue = .object([
            "cpu": .number(Double(usage.cpu.components.seconds) + Double(usage.cpu.components.attoseconds) / 1e18), "footprint": .number(Double(usage.footprint)),
            "threads": .number(Double(usage.threads)),
        ])
        expectNoDifference(encoded, .object(["running": .object(["status": try wire(status), "usage": expectedUsage])]))
    }
}
