import AppKit
import Applications
import CryptoKit
import CustomDump
import Testing

// --- [CONSTANTS] -----------------------------------------------------------------------

private let members: [(type: Int32, name: String)] = [
    (0, "arrowCursor"), (1, "IBeamCursor"), (2, "dragLinkCursor"), (3, "operationNotAllowedCursor"), (5, "dragCopyCursor"), (11, "closedHandCursor"),
    (12, "openHandCursor"), (13, "pointingHandCursor"), (19, "columnResizeCursor"), (20, "crosshairCursor"), (23, "rowResizeCursor"), (24, "contextualMenuCursor"),
    (25, "disappearingItemCursor"), (26, "IBeamCursorForVerticalLayout"), (42, "zoomInCursor"), (43, "zoomOutCursor"),
]
private let drawn = Data((0..<4480).map { (index: Int) -> UInt8 in UInt8(index * 7 % 251) })
private let unnamed: [(label: String, image: Pointer.Cursor.Image?)] = [
    (label: "an image several system cursors share", image: Pointer.Cursor.Image(.system(2))),
    (label: "an image an application draws", image: Pointer.Cursor.Image(data: drawn, hotSpot: CGPoint(x: 3, y: 9), size: CGSize(width: 28, height: 40))),
]
private let drags: [(label: String, rows: String, draggingFrame: CGRect?)] = [
    (
        label: "a row at the dragging window level", rows: #"[{"number":41,"frame":[[100,200],[64,48]],"layer":500,"onScreen":true,"asking":false,"presented":true}]"#,
        draggingFrame: CGRect(x: 100, y: 200, width: 64, height: 48),
    ),
    (label: "a row above the dragging window level", rows: #"[{"number":42,"frame":[[0,0],[1512,982]],"layer":2147483630,"onScreen":true,"asking":false,"presented":true}]"#, draggingFrame: nil),
    (label: "a document window", rows: #"[{"number":43,"frame":[[10,30],[800,600]],"layer":0,"onScreen":true,"asking":false,"presented":true}]"#, draggingFrame: nil),
]

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite(.timeLimit(.minutes(1)))
struct PointerTests {
    @Test(arguments: members)
    func namesTheImageOfEachStandardCursorByItsAppKitClassProperty(type: Int32, name: String) throws {
        let image: Pointer.Cursor.Image = try #require(Pointer.Cursor.Image(.core(type)), "CoreCursor type \(type) gives no image")
        let named = Pointer.Cursor(image)
        let expected: Pointer.Cursor = .system(name: name)
        expectNoDifference(named, expected)
    }

    @Test
    func namesASystemCursorNoAppKitMemberDrawsByItsWindowServerName() throws {
        let image: Pointer.Cursor.Image = try #require(Pointer.Cursor.Image(.system(7)), "system cursor 7 gives no image")
        let named = Pointer.Cursor(image)
        let expected: Pointer.Cursor = .system(name: "com.apple.coregraphics.Wait")
        expectNoDifference(named, expected)
    }

    @Test(arguments: unnamed)
    func statesAnImageNoSingleSystemCursorDrawsByItsDigest(label: String, image: Pointer.Cursor.Image?) throws {
        let image: Pointer.Cursor.Image = try #require(image, "\(label) gives no image")
        let digest: String = Data(SHA256.hash(data: image.data)).base64EncodedString()
        let named = Pointer.Cursor(image)
        let expected: Pointer.Cursor = .custom(digest: digest, hotSpot: image.hotSpot, size: image.size)
        expectNoDifference(named, expected, label)
    }

    @Test(arguments: drags)
    func statesADragOnlyWhileTheApplicationOwnsAWindowAtTheDraggingLevel(label: String, rows: String, draggingFrame: CGRect?) throws {
        let windows: [InstanceState.Window] = try JSONDecoder().decode([InstanceState.Window].self, from: Data(rows.utf8))
        let stated: CGRect? = Pointer(of: .current, windows: windows, containers: [])?.draggingFrame
        expectNoDifference(stated, draggingFrame, label)
    }
}
