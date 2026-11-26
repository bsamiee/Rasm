import CustomDump
import Foundation
import Testing

@testable import Files

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite(.timeLimit(.minutes(1)))
final class LogLineTests {
    let folder = FileManager.default.temporaryDirectory.appending(component: UUID().uuidString).resolvingSymlinksInPath()

    init() throws { try FileManager.default.createDirectory(at: folder, withIntermediateDirectories: true) }

    deinit {
        do { try FileManager.default.removeItem(at: folder) } catch { Issue.record(error) }
    }

    @Test
    func followReadsEachLineOnceAcrossARename() async throws {
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

    @Test
    func followReadsWhatAHeldDescriptorWrites() async throws {
        let current = folder.appending(component: "server.log")
        try Data().write(to: current)
        var batches = LogLine.follow(in: LogFolder(url: folder, glob: "*.log")).makeAsyncIterator()
        _ = await batches.next(isolation: #isolation)
        let held = try FileHandle(forWritingTo: current)
        defer {
            do { try held.close() } catch { Issue.record(error) }
        }
        try held.write(contentsOf: Data("first while held\n".utf8))
        try held.write(contentsOf: Data("second while held\n".utf8))
        var read: [String] = []
        while !read.contains("second while held"), let batch = await batches.next(isolation: #isolation) {
            read += batch.lines.map(\.text)
        }
        expectNoDifference(read, ["first while held", "second while held"])
    }

    @Test
    func followReadsLinesWrittenBeforeTheFirstPull() async throws {
        let current = folder.appending(component: "server.log")
        try Data("before follow\n".utf8).write(to: current)
        var batches = LogLine.follow(in: LogFolder(url: folder, glob: "*.log")).makeAsyncIterator()
        let appended = try FileHandle(forWritingTo: current)
        try appended.seekToEnd()
        try appended.write(contentsOf: Data("before the first pull\n".utf8))
        _ = await batches.next(isolation: #isolation)
        try appended.write(contentsOf: Data("after the first pull\n".utf8))
        try appended.close()
        var read: [String] = []
        while !read.contains("after the first pull"), let batch = await batches.next(isolation: #isolation) {
            read += batch.lines.map(\.text)
        }
        expectNoDifference(read, ["before the first pull", "after the first pull"])
    }

    @Test
    func followKeepsLinesWholeAndReadsTheFolderAlone() async throws {
        let (current, nested) = (folder.appending(component: "server.log"), folder.appending(components: "nested", "server.log"))
        try FileManager.default.createDirectory(at: nested.deletingLastPathComponent(), withIntermediateDirectories: true)
        try Data().write(to: current)
        var batches = LogLine.follow(in: LogFolder(url: folder, glob: "*.log")).makeAsyncIterator()
        _ = await batches.next(isolation: #isolation)
        try Data("in a nested folder\n".utf8).write(to: nested)
        var written = Data("separated\u{2028}within\n".utf8)
        written.append(try #require("café\nlast\n".data(using: .isoLatin1)))
        try written.write(to: current)
        var read: [LogLine] = []
        while !read.contains(where: { line in line.text == "last" }), let batch = await batches.next(isolation: #isolation) {
            read += batch.lines
        }
        expectNoDifference(read.map(\.text), ["separated\u{2028}within", "caf\u{FFFD}", "last"])
        expectNoDifference(Set(read.map(\.file)), [current])
    }

    @Test
    func newestReadsLinesHoldingInvalidUTF8() throws {
        try #require("café\nlast\n".data(using: .isoLatin1)).write(to: folder.appending(component: "server.log"))
        let newest = LogLine.newest(in: [LogFolder(url: folder, glob: "*.log")], since: nil, limit: 10) { _ in true }
        expectNoDifference(newest.lines.map(\.text), ["last", "caf\u{FFFD}"])
        #expect(newest.failures.isEmpty)
    }
}
