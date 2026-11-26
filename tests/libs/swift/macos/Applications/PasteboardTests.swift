import AppKit
import Applications
import CustomDump
import Testing
import UniformTypeIdentifiers

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite(.timeLimit(.minutes(1)))
struct PasteboardTests {
    @Test
    func readsTextAppKitWroteWithItsCountTypesAndDetectedNumber() async throws {
        let board = NSPasteboard.withUniqueName()
        defer { board.releaseGlobally() }
        board.clearContents()
        board.setString("4271", forType: .string)
        let read = try await Pasteboard(board.name)
        expectNoDifference(read.content, .text(value: "4271"))
        expectNoDifference([read.changeCount], [board.changeCount])
        expectNoDifference(read.types, board.types?.map(\.rawValue))
        #expect(read.patterns.contains(__NSPasteboardDetectionPattern.__number.rawValue))
    }

    @Test
    func readsTextOfALaterItemWhereTheFirstHoldsNoneToDetect() async throws {
        let board = NSPasteboard.withUniqueName()
        defer { board.releaseGlobally() }
        let image = NSPasteboardItem()
        let text = NSPasteboardItem()
        image.setData(Data([1]), forType: .png)
        text.setString("4271", forType: .string)
        board.clearContents()
        #expect(board.writeObjects([image, text]))
        let read = try await Pasteboard(board.name)
        expectNoDifference(read.content, .text(value: "4271"))
        expectNoDifference(read.patterns, [])
    }

    @Test
    func writesTextAppKitReadsBackAfterOneChange() async throws {
        let board = NSPasteboard.withUniqueName()
        defer { board.releaseGlobally() }
        board.clearContents()
        board.setString("before", forType: .string)
        let before = board.changeCount
        let written = try await Pasteboard(board.name, writing: "after 7b1e")
        expectNoDifference(board.string(forType: .string), "after 7b1e")
        expectNoDifference([board.changeCount, written.changeCount], [before + 1, before + 1])
    }

    @Test
    func statesTheTypeOfAReferencedFileWithNoText() async throws {
        let board = NSPasteboard.withUniqueName()
        defer { board.releaseGlobally() }
        let file = URL(filePath: #filePath)
        board.clearContents()
        #expect(board.setString(file.absoluteString, forType: .fileURL))
        let read = try await Pasteboard(board.name)
        expectNoDifference(read.contentType, UTType(filenameExtension: file.pathExtension)?.identifier)
        expectNoDifference(read.content, nil)
    }
}
