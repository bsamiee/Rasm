import CustomDump
import Files
import Foundation
import Testing

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite
struct RotatingFileTests {
    let folder = FileManager.default.temporaryDirectory.appending(component: UUID().uuidString)

    @Test
    func boundsEachFileBySizeAndDropsTheOldestBackup() throws {
        defer {
            do { try FileManager.default.removeItem(at: folder) } catch { Issue.record(error) }
        }
        var file = RotatingFile(folder.appending(components: "nested", "records.log"))
        (file.size, file.backups) = (8, 2)
        for line in ["1111\n", "2222\n", "3333\n", "4444\n"] { try file.append(Data(line.utf8)) }
        expectNoDifference(try file.urls.map { url in try String(contentsOf: url, encoding: .utf8) }, ["2222\n", "3333\n", "4444\n"])
    }

    @Test
    func reopensThePathAfterAnotherWriterMovesTheFile() throws {
        defer {
            do { try FileManager.default.removeItem(at: folder) } catch { Issue.record(error) }
        }
        let file = RotatingFile(folder.appending(component: "records.log"))
        try file.append(Data("moved\n".utf8))
        try FileManager.default.moveItem(at: file.url, to: folder.appending(component: "moved.log"))
        try file.append(Data("fresh\n".utf8))
        expectNoDifference(try String(contentsOf: file.url, encoding: .utf8), "fresh\n")
    }

    @Test
    func removesBackupsLastWrittenBeforeTheAge() throws {
        defer {
            do { try FileManager.default.removeItem(at: folder) } catch { Issue.record(error) }
        }
        var file = RotatingFile(folder.appending(component: "records.log"))
        file.age = .seconds(60)
        try file.append(Data("stale\n".utf8))
        try file.append(Data("current\n".utf8), at: .now.addingTimeInterval(3_600))
        expectNoDifference(file.urls.filter { url in FileManager.default.fileExists(atPath: url.path(percentEncoded: false)) }, [file.url])
        expectNoDifference(try String(contentsOf: file.url, encoding: .utf8), "current\n")
    }
}
