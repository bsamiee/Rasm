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
    id: Element.ID(token: Data([1, 2, 3])),
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
    detail: nil,
    helper: false,
)
private let frame: CGRect = CGRect(x: 1, y: 2, width: 3, height: 4)
private let described: InstanceState.Window = InstanceState.Window(number: 77, title: nil, frame: frame, layer: 0, onScreen: true, description: .element(sheet), arrived: false)
private let titled: InstanceState.Window = InstanceState.Window(number: 78, title: "Prompt", frame: frame, layer: 0, onScreen: true, description: .undescribed, arrived: true)
private let helper: InstanceState.Window = InstanceState.Window(number: 79, title: nil, frame: frame, layer: 3, onScreen: false, description: .undescribed, arrived: false)
private let alert: InstanceState.Window = InstanceState.Window(number: 20917, title: "Adobe InDesign", frame: frame, layer: 8, onScreen: false, description: .blocking, arrived: false)
private let sheetRow: String =
    #"{"asking":true,"element":{"id":"AQID","role":"AXSheet","identifier":"_NS:87","description":"alert","actions":[],"kind":"sheet","asking":true},"frame":[[1,2],[3,4]],"kind":"sheet","layer":0,"number":77,"onScreen":true}"#
private let titledRow: String = #"{"frame":[[1,2],[3,4]],"kind":"undescribed","layer":0,"number":78,"onScreen":true,"title":"Prompt"}"#
private let helperRow: String = #"{"frame":[[1,2],[3,4]],"kind":"undescribed","layer":3,"number":79,"onScreen":false,"presented":false}"#
private let alertRow: String = #"{"asking":true,"frame":[[1,2],[3,4]],"kind":"blocking","layer":8,"number":20917,"onScreen":false,"title":"Adobe InDesign"}"#
private let lines: [(state: InstanceState, line: String)] = [
    (running(.launching, windows: [helper]), #"{"running":{"active":false,"status":"launching","windows":[\#(helperRow)]}}"#),
    (running(.responding, windows: []), #"{"running":{"active":false,"status":"responding","windows":[]}}"#),
    (running(.responding, windows: [titled, described]), #"{"running":{"active":false,"status":"responding","windows":[\#(sheetRow),\#(titledRow)]}}"#),
    (
        running(.unresponsive(.sleeping), windows: [helper, alert]), #"{"running":{"active":false,"process":"sleeping","status":"unresponsive","windows":[\#(alertRow),\#(helperRow)]}}"#,
    ),
    (running(.unresponsive(.stopped), windows: []), #"{"running":{"active":false,"process":"stopped","status":"unresponsive","windows":[]}}"#),
    (.exited(.success(ProcessExit(status: .exited(0), detail: [])), report: nil), #"{"exited":{"status":0}}"#),
    (.exited(.success(ProcessExit(status: .exited(7), detail: [])), report: nil), #"{"exited":{"status":7}}"#),
    (.exited(.success(ProcessExit(status: .signaled(9), detail: .memory)), report: nil), #"{"exited":{"signal":9,"detail":["memory"]}}"#),
    (.exited(.success(ProcessExit(status: .signaled(11), detail: [])), report: nil), #"{"exited":{"signal":11,"reporting":true}}"#),
    (.exited(.success(ProcessExit(status: .signaled(9), detail: .csError)), report: nil), #"{"exited":{"signal":9,"detail":["codeSigning"],"reporting":true}}"#),
    (.exited(.failure(.noSuchProcess), report: nil), #"{"exited":{"statusFailure":"\#(Errno.noSuchProcess.description)"}}"#),
]
private let silent: [(previous: InstanceState, next: InstanceState)] = [
    (running(.unresponsive(.running), windows: [titled]), running(.unresponsive(.sleeping), windows: [titled])),
    (running(.unresponsive(.sleeping), windows: [titled]), running(.unresponsive(.sleeping), windows: [titled], usage: .failure(.noSuchProcess))),
    (
        running(.responding, windows: [titled]),
        running(.responding, windows: [InstanceState.Window(number: 78, title: "Prompt", frame: .zero, layer: 0, onScreen: false, description: .undescribed, arrived: true)]),
    ),
    (running(.responding, windows: [titled]), running(.responding, windows: [titled, helper])),
]
private let printed: [(previous: InstanceState, next: InstanceState)] = [
    (running(.responding, windows: [titled]), running(.unresponsive(.sleeping), windows: [titled])),
    (running(.unresponsive(.sleeping), windows: [titled]), running(.unresponsive(.stopped), windows: [titled])),
    (running(.responding, windows: [titled]), running(.responding, windows: [titled], active: true)),
    (running(.responding, windows: []), running(.responding, windows: [titled])),
    (running(.responding, windows: [helper]), running(.responding, windows: [helper, alert])),
    (running(.responding, windows: [described]), running(.responding, windows: [described.described(.undescribed, arrived: true)])),
]

// --- [OPERATIONS] ----------------------------------------------------------------------

private func running(_ status: InstanceState.Running.Status, windows: [InstanceState.Window], active: Bool = false, usage: Result<ResourceUsage, Errno>? = nil) -> InstanceState {
    .running(InstanceState.Running(status: status, active: active, windows: windows, menus: [], usage: usage))
}

// --- [COMPOSITION] ---------------------------------------------------------------------

struct InstanceStates {
    @Test(arguments: lines)
    func stateEncodesFlatWithAskingRowsFirst(state: InstanceState, line: String) throws {
        expectNoDifference(try wire(state), try json(line))
    }

    @Test(arguments: silent)
    func processUsageFrameAndUnpresentedRowsPrintNothing(previous: InstanceState, next: InstanceState) {
        #expect(previous.printsSame(as: next))
    }

    @Test(arguments: printed)
    func statusActiveRowsAndAskingPrint(previous: InstanceState, next: InstanceState) {
        #expect(!previous.printsSame(as: next))
    }

    @Test
    func promptRowsJoinAskingAndEncodeInOneWindowList() throws {
        var state: InstanceState.Running = InstanceState.Running(status: .responding, active: false, windows: [titled], menus: [], usage: nil)
        state.prompts = [described]
        expectNoDifference(state.asking.map(\.number), [77])
        expectNoDifference(try wire(state), try json(#"{"active":false,"status":"responding","windows":[\#(sheetRow),\#(titledRow)]}"#))
    }

    @Test
    func undescribedRowsArePresentedOnceArrivedAndBlockingAsks() {
        #expect(titled.presented)
        #expect(!helper.presented)
        #expect(described.presented)
        #expect(alert.presented)
        #expect(alert.asking)
        #expect(!titled.asking)
    }

    @Test
    func failedStateHoldsFailureFields() throws {
        let failure: ApplicationFailure = ApplicationFailure(subject: "Probe", reason: .untrusted)
        let expected: JSONValue = .object(["failed": .object(["reason": .string("untrusted"), "subject": .string("Probe"), "message": .string(try #require(failure.errorDescription))])])
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
            try json(#"{"exited":{"signal":11,"report":{"url":"\#(url)","pid":4242,"bundleIdentifier":"dev.rasm.reports.application","exception":"EXC_BAD_ACCESS"}}}"#),
        )
        expectNoDifference(
            try wire(InstanceState.exited(.success(exit), report: .failure(failure))),
            .object(["exited": .object(["signal": .number(11), "reportFailure": .string(failure.name)])]),
        )
    }

    @Test
    func usageSitsBesideStatus() throws {
        let usage: ResourceUsage = try ResourceUsage(of: getpid())
        let expected: JSONValue = .object([
            "cpu": .number(Double(usage.cpu.components.seconds) + Double(usage.cpu.components.attoseconds) / 1e18), "footprint": .number(Double(usage.footprint)),
            "threads": .number(Double(usage.threads)),
        ])
        let base: [String: JSONValue] = ["active": .bool(false), "status": .string("responding"), "windows": .array([])]
        expectNoDifference(try wire(running(.responding, windows: [], usage: .success(usage))), .object(["running": .object(base.merging(["usage": expected]) { _, next in next })]))
        expectNoDifference(
            try wire(running(.responding, windows: [], usage: .failure(.noSuchProcess))),
            .object(["running": .object(base.merging(["usageFailure": .string(Errno.noSuchProcess.description)]) { _, next in next })]),
        )
    }
}
