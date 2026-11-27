import CustomDump
import Files
import Foundation
import Testing

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite(.timeLimit(.minutes(1)))
struct FileEventTests {
    let folder = FileManager.default.temporaryDirectory.appending(component: UUID().uuidString).resolvingSymlinksInPath()

    @Test
    func replacingTheWatchedFolderAsksForARescan() async throws {
        defer {
            do { try FileManager.default.removeItem(at: folder) } catch { Issue.record(error) }
        }
        let (watched, staged) = (folder.appending(component: "watched"), folder.appending(component: "staged"))
        try FileManager.default.createDirectory(at: watched, withIntermediateDirectories: true)
        try FileManager.default.createDirectory(at: staged, withIntermediateDirectories: true)
        try Data("moved in with its folder".utf8).write(to: staged.appending(component: "moved.ips"))
        var batches = FileEvent.events(of: [watched]).makeAsyncIterator()
        _ = try #require(await batches.next(isolation: #isolation)).get()
        try FileManager.default.moveItem(at: watched, to: folder.appending(component: "replaced"))
        try FileManager.default.moveItem(at: staged, to: watched)
        let sentinel = watched.appending(component: "sentinel")
        try Data().write(to: sentinel)
        var events: [FileEvent] = []
        while !events.contains(where: { event in event.url.lastPathComponent == sentinel.lastPathComponent }), let batch = await batches.next(isolation: #isolation) {
            events += try batch.get()
        }
        let rescans = events.filter(\.mustScanSubDirs)
        #expect(!rescans.isEmpty)
    }

    @Test
    func writesOnAHeldDescriptorArriveBeforeItCloses() async throws {
        defer {
            do { try FileManager.default.removeItem(at: folder) } catch { Issue.record(error) }
        }
        try FileManager.default.createDirectory(at: folder, withIntermediateDirectories: true)
        let file = folder.appending(component: "server.log")
        try Data().write(to: file)
        var events = FileEvent.events(of: try FileHandle(forReadingFrom: file), eventMask: [.write, .extend, .delete, .rename]).makeAsyncIterator()
        let registered = await events.next(isolation: #isolation)
        expectNoDifference(registered, [])
        let held = try FileHandle(forWritingTo: file)
        try held.write(contentsOf: Data("written while held\n".utf8))
        let written = try #require(await events.next(isolation: #isolation))
        try held.close()
        let touched: DispatchSource.FileSystemEvent = [.write, .extend]
        let reported: Bool = !written.isDisjoint(with: touched)
        #expect(reported)
    }

    @Test
    func aDanglingSymlinkIsPresent() async throws {
        defer {
            do { try FileManager.default.removeItem(at: folder) } catch { Issue.record(error) }
        }
        try FileManager.default.createDirectory(at: folder, withIntermediateDirectories: true)
        let lock = folder.appending(component: "file.lock")
        try FileManager.default.createSymbolicLink(at: lock, withDestinationURL: folder.appending(component: "holder"))
        var presence = FileEvent.presence(of: lock).makeAsyncIterator()
        #expect(try #require(await presence.next(isolation: #isolation)).get())
    }

    @Test
    func aPackageHoldingADanglingSymlinkReportsItsWrite() async throws {
        defer {
            do { try FileManager.default.removeItem(at: folder) } catch { Issue.record(error) }
        }
        let package = folder.appending(component: "document.rtfd")
        try FileManager.default.createDirectory(at: package, withIntermediateDirectories: true)
        try Data("before".utf8).write(to: package.appending(component: "TXT.rtf"))
        try FileManager.default.createSymbolicLink(at: package.appending(component: "attachment"), withDestinationURL: folder.appending(component: "missing"))
        let (_, written) = await FileEvent.written([package]) { () async throws(Never) in
            do { try Data("after".utf8).write(to: package.appending(component: "TXT.rtf")) } catch { Issue.record(error) }
        }
        expectNoDifference(try written.get(), [package])
    }
}
