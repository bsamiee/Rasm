import Applications
import CasePaths
import CustomDump
import Encoding
import Foundation
import TestSupport
import Testing

// --- [CONSTANTS] -----------------------------------------------------------------------

private let application: String = "dev.rasm.reports.application"
private let listings: [(identifier: String, reports: [(file: String, fields: String)])] = [
    (
        application,
        [
            ("Application-2026-11-01-120000.ips", #""pid":4242,"bundleIdentifier":"dev.rasm.reports.application","exception":"EXC_BAD_ACCESS""#),
            ("Application-2026-11-01-120100.ips", #""pid":4243,"bundleIdentifier":"dev.rasm.reports.application","exception":"EXC_CRASH""#),
        ],
    ),
    ("dev.rasm.reports.other", [("Other-2026-11-01-120200.ips", #""pid":5000,"bundleIdentifier":"dev.rasm.reports.other","exception":"EXC_BREAKPOINT""#)]),
    ("dev.rasm.reports.bundle-info", [("BundleInfo-2026-11-01-120600.ips", #""pid":7000,"bundleIdentifier":"dev.rasm.reports.bundle-info","exception":"EXC_CRASH""#)]),
    ("dev.rasm.reports.absent", []),
    ("", []),
]

// --- [OPERATIONS] ----------------------------------------------------------------------

private func expected(_ report: (file: String, fields: String), in directory: URL = crashReports) throws -> JSONValue {
    try json(#"{"url":"\#(directory.appending(component: report.file).absoluteString)",\#(report.fields)}"#)
}

private func copy(_ name: String, into directory: URL, as copied: String, dated date: Date? = nil) throws -> URL {
    var url: URL = directory.appending(component: copied)
    try FileManager.default.copyItem(at: crashReports.appending(component: name), to: url)
    var dates: URLResourceValues = URLResourceValues()
    dates.creationDate = date
    dates.contentModificationDate = date
    try date.map { _ in try url.setResourceValues(dates) }
    return url
}

// --- [COMPOSITION] ---------------------------------------------------------------------

@Suite(.testDirectory)
struct CrashReports {
    @Test(arguments: listings)
    func listingReadsCrashReportsOfOneBundle(identifier: String, reports: [(file: String, fields: String)]) throws {
        let listed: (reports: [CrashReport], failures: [ApplicationFailure]) = CrashReport.reports(for: identifier, in: crashReports)
        let encoded: [JSONValue] = try listed.reports.sorted(using: KeyPathComparator(\.pid)).map(wire)
        expectNoDifference(encoded, try reports.map { report in try expected(report) })
    }

    @Test
    func listingNamesTruncatedReportAsItsOneFailure() {
        let listed: (reports: [CrashReport], failures: [ApplicationFailure]) = CrashReport.reports(for: application, in: crashReports)
        expectNoDifference(listed.failures.map(\.subject), [crashReports.appending(component: "Application-2026-11-01-120400.ips").path(percentEncoded: false)])
        let decoding: [Bool] = listed.failures.map { failure in failure.errorUserInfo[NSUnderlyingErrorKey] is DecodingError }
        expectNoDifference(decoding, [true])
    }

    @Test
    func listingAbsentDirectoryFindsNothing() {
        let listed: (reports: [CrashReport], failures: [ApplicationFailure]) = CrashReport.reports(for: application, in: crashReports.appending(component: "absent"))
        expectNoDifference(listed.reports, [])
        expectNoDifference(listed.failures.map(\.subject), [])
    }

    @Test
    func reportOfToolWithoutBundleOmitsIdentifier() async throws {
        let directory: URL = try #require(TestDirectory.current)
        _ = try copy("tool-2026-11-01-120300.ips", into: directory, as: "tool-2026-11-01-120300.ips")
        let resolved: CrashReport = try await CrashReport.written(by: 6000, since: .distantPast, within: .seconds(10), in: directory).get()
        expectNoDifference(try wire(resolved), try expected(("tool-2026-11-01-120300.ips", #""pid":6000,"exception":"EXC_BAD_ACCESS""#), in: directory))
    }

    @Test
    func writtenReportArrivingWithinLimitResolvesThatReport() async throws {
        let directory: URL = try #require(TestDirectory.current)
        let launch: Date = .now
        async let found: Result<CrashReport, ApplicationFailure> = CrashReport.written(by: 4242, since: launch, within: .seconds(10), in: directory)
        let url: URL = try copy("Application-2026-11-01-120000.ips", into: directory, as: "Application-2026-11-01-120000.ips", dated: .now)
        let resolved: CrashReport = try await found.get()
        let fields: String = #""pid":4242,"bundleIdentifier":"dev.rasm.reports.application","exception":"EXC_BAD_ACCESS""#
        expectNoDifference(try wire(resolved), try expected((url.lastPathComponent, fields), in: url.deletingLastPathComponent()))
    }

    @Test
    func writtenReportAlreadyPresentAfterLaunchResolvesAtOnce() async throws {
        let directory: URL = try #require(TestDirectory.current)
        let launch: Date = .now.addingTimeInterval(-5)
        _ = try copy("Application-2026-11-01-120000.ips", into: directory, as: "Application-2026-11-01-120000.ips", dated: .now)
        let resolved: CrashReport = try await CrashReport.written(by: 4242, since: launch, within: .seconds(10), in: directory).get()
        expectNoDifference(resolved.pid, 4242)
    }

    @Test(arguments: [(4243, nil), (4242, -60), (4244, nil)] as [(pid_t, TimeInterval?)])
    func writtenReportOfAnotherProcessEarlierLaunchOrAnotherKindTimesOut(pid: pid_t, age: TimeInterval?) async throws {
        let directory: URL = try #require(TestDirectory.current)
        let launch: Date = .now
        _ = try copy("Application-2026-11-01-120000.ips", into: directory, as: "Application-2026-11-01-115900.ips", dated: age.map(launch.addingTimeInterval))
        _ = try copy("Application-2026-11-01-120500.ips", into: directory, as: "Application-2026-11-01-120500.ips")
        let clock: ContinuousClock = ContinuousClock()
        let start: ContinuousClock.Instant = clock.now
        let found: Result<CrashReport, ApplicationFailure> = await CrashReport.written(by: pid, since: launch, within: .milliseconds(800), in: directory)
        let elapsed: Duration = clock.now - start
        let failure: ApplicationFailure = try #require(found[case: \.failure])
        expectNoDifference(failure.subject, "process \(pid)")
        let waited: Bool = elapsed >= .milliseconds(800)
        #expect(waited, "timed out after \(elapsed), limit 800 ms")
    }
}
