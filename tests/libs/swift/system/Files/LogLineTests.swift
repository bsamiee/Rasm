import CustomDump
import Foundation
import Testing

@testable import Files

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite(.timeLimit(.minutes(1)))
struct LogLineTests {
    @Test
    func followReadsEachLineOnceAcrossARename() async throws {
        let folder = FileManager.default.temporaryDirectory.appending(component: UUID().uuidString).resolvingSymlinksInPath()
        defer {
            do { try FileManager.default.removeItem(at: folder) } catch { Issue.record(error) }
        }
        try FileManager.default.createDirectory(at: folder, withIntermediateDirectories: true)
        let (current, rotated) = (folder.appending(component: "server.log"), folder.appending(component: "server.log.1"))
        try Data("before start\n".utf8).write(to: current)
        var batches = LogLine.follow(in: LogFolder(url: folder, glob: "server.log*")).makeAsyncIterator()
        _ = await batches.next(isolation: #isolation)
        let appended = try FileHandle(forWritingTo: current)
        try appended.seekToEnd()
        try appended.write(contentsOf: Data("written before the rename\n".utf8))
        try FileManager.default.moveItem(at: current, to: rotated)
        try appended.write(contentsOf: Data("written after the rename\n".utf8))
        try appended.close()
        try Data("written to the new file\n".utf8).write(to: current)
        var read: [String] = []
        while !Set(read).isSuperset(of: ["written after the rename", "written to the new file"]), let batch = await batches.next(isolation: #isolation) {
            read += batch.lines.map(\.text)
        }
        expectNoDifference(read.sorted(), ["written after the rename", "written before the rename", "written to the new file"])
    }
}
