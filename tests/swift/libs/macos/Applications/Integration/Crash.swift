import AppKit
import Applications
import CasePaths
import CustomDump
import Encoding
import Foundation
import Subprocess
import TestSupport
import Testing

@testable import AppKitFixture

// --- [CONSTANTS] -----------------------------------------------------------------------

private let crashes: [(scenario: Scenario, signal: Int32, exception: String)] = [(.fault, SIGSEGV, "EXC_BAD_ACCESS"), (.abort, SIGABRT, "EXC_CRASH")]

// --- [COMPOSITION] ---------------------------------------------------------------------

extension Integration {
    @Suite
    struct Crashes {
        @Test(
            .application(FixtureApplication.self, arguments: Settings().arguments),
            arguments: crashes,
        )
        func crashReadsSignalThenItsReportAndEnds(scenario: Scenario, signal: Int32, exception: String) async throws {
            let (installation, instance): (Installation, NSRunningApplication) = try await launched()
            let seen: [Observed] = try await observe(instance, within: .seconds(90), until: { _ in false }, acting: { try await run(scenario, in: instance) })
            expectNoDifference(phases(seen), [.responding, .exited(.signaled(signal)), .reported(.signaled(signal), exception: exception)])
            let outcome: Result<CrashReport, ApplicationFailure>? = try #require(seen.last).state[case: \.exited]?.report
            let report: CrashReport = try #require(outcome).get()
            expectNoDifference(report.pid, instance.processIdentifier)
            expectNoDifference(report.bundleIdentifier, installation.identifier)
            expectNoDifference(report.url.deletingLastPathComponent().resolvingSymlinksInPath().pathComponents, CrashReport.directory.resolvingSymlinksInPath().pathComponents)
            let contents: Data = try Data(contentsOf: report.url)
            let header: [String: JSONValue] = try JSONDecoder().decode(JSONValue.self, from: contents.prefix { byte in byte != UInt8(ascii: "\n") }).decode()
            let body: [String: JSONValue] = try JSONDecoder().decode(JSONValue.self, from: contents.drop { byte in byte != UInt8(ascii: "\n") }.dropFirst()).decode()
            expectNoDifference([header["bundleID"], header["bug_type"], body["pid"]], [.string(installation.identifier), .string("309"), .number(Double(instance.processIdentifier))])
        }
    }
}
