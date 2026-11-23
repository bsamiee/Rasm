import AppleEvents
import ApplicationServer
import CustomDump
import Encoding
import Foundation
import Testing

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite
struct ExecutedTests {
    @Test
    func readsEachArtifactInOrderKeepingAMissingOneAsItsFailure() throws {
        let folder = FileManager.default.temporaryDirectory.appending(component: UUID().uuidString)
        try FileManager.default.createDirectory(at: folder, withIntermediateDirectories: true)
        defer {
            do { try FileManager.default.removeItem(at: folder) } catch { Issue.record(error) }
        }
        let (notes, missing, sizes) = (folder.appending(component: "notes.txt"), folder.appending(component: "absent.png"), folder.appending(component: "sizes.csv"))
        try Data("A4 proof".utf8).write(to: notes)
        try Data("w,h\n210,297\n".utf8).write(to: sizes)
        let executed = try JSONDecoder().decode([String: JSONValue].self, from: JSONEncoder().encode(Executed(reply: .text("done"), reading: [notes, missing, sizes])))
        let artifacts: [[String: JSONValue]] =
            if case .array(let artifacts)? = executed["artifacts"] { artifacts.map { artifact in if case .object(let fields) = artifact { fields } else { [:] } } } else { [] }
        let failure: [String]? = if case .object(let fields)? = artifacts.dropFirst().first?["failure"] { fields.keys.filter { key in ["name", "message"].contains(key) }.sorted() } else { nil }
        expectNoDifference(executed["reply"], .string("done"))
        expectNoDifference(artifacts.map { artifact in artifact["_tag"] }, [.string("Success"), .string("Failure"), .string("Success")])
        expectNoDifference(
            [artifacts.first?["success"], artifacts.last?["success"]],
            [
                .object(["uri": .string(notes.absoluteString), "mimeType": .string("text/plain"), "data": .string("QTQgcHJvb2Y=")]),
                .object(["uri": .string(sizes.absoluteString), "mimeType": .string("text/csv"), "data": .string("dyxoCjIxMCwyOTcK")]),
            ],
        )
        expectNoDifference(failure, ["message", "name"])
    }
}
