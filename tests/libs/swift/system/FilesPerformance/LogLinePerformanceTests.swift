import Files
import Foundation
import XCTest

// --- [OPERATIONS] ----------------------------------------------------------------------

final class LogLinePerformanceTests: XCTestCase {
    func testNewestReadsTheLastLinesOfALargeLog() throws {
        let folder = FileManager.default.temporaryDirectory.appending(component: UUID().uuidString)
        try FileManager.default.createDirectory(at: folder, withIntermediateDirectories: true)
        addTeardownBlock { try FileManager.default.removeItem(at: folder) }
        try Data((0..<1_000_000).map { index in "line \(index)\n" }.joined().utf8).write(to: folder.appending(component: "server.log"))
        let options = XCTMeasureOptions.default
        options.iterationCount = 10
        measure(metrics: [XCTClockMetric(), XCTCPUMetric(limitingToCurrentThread: true), XCTMemoryMetric()], options: options) {
            _ = LogLine.newest(in: [LogFolder(url: folder)], since: nil, limit: 10_000, where: { _ in true })
        }
    }
}
