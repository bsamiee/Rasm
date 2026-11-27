import AppKit
import CryptoKit
import CustomDump
import Testing

@testable import Applications

// --- [CONSTANTS] -----------------------------------------------------------------------

private let members: [(type: Int32, name: String)] = [
    (0, "arrowCursor"), (1, "IBeamCursor"), (2, "dragLinkCursor"), (3, "operationNotAllowedCursor"), (5, "dragCopyCursor"), (11, "closedHandCursor"), (12, "openHandCursor"),
    (13, "pointingHandCursor"), (17, "columnResizeCursor left"), (18, "columnResizeCursor right"), (19, "columnResizeCursor"), (20, "crosshairCursor"), (21, "rowResizeCursor up"),
    (22, "rowResizeCursor down"), (23, "rowResizeCursor"), (24, "contextualMenuCursor"), (25, "disappearingItemCursor"), (26, "IBeamCursorForVerticalLayout"), (42, "zoomInCursor"),
    (43, "zoomOutCursor"),
]
private let drawn = Data((0..<4480).map { (index: Int) -> UInt8 in UInt8(index * 7 % 251) })
private let unnamed: [(label: String, image: Pointer.Cursor.Image?)] = [
    (label: "an image several system cursors share", image: Pointer.Cursor.Image(.system(2))),
    (label: "an image an application draws", image: Pointer.Cursor.Image(data: drawn, hotSpot: CGPoint(x: 3, y: 9), size: CGSize(width: 28, height: 40))),
]
private let drags: [(label: String, frame: CGRect, layer: Int, onScreen: Bool, draggingFrame: CGRect?)] = [
    (label: "a row at the dragging window level", frame: CGRect(x: 100, y: 200, width: 64, height: 48), layer: 500, onScreen: true, draggingFrame: CGRect(x: 100, y: 200, width: 64, height: 48)),
    (label: "a row above the dragging window level", frame: CGRect(x: 0, y: 0, width: 1512, height: 982), layer: 2_147_483_630, onScreen: true, draggingFrame: nil),
    (label: "a document window", frame: CGRect(x: 10, y: 30, width: 800, height: 600), layer: 0, onScreen: true, draggingFrame: nil),
    (label: "a row at the dragging window level no longer on screen", frame: CGRect(x: 100, y: 200, width: 64, height: 48), layer: 500, onScreen: false, draggingFrame: nil),
]

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite(.timeLimit(.minutes(1)))
struct PointerTests {
    init() async { await NSApplication.loadApplication() }

    @Test(arguments: members)
    func namesTheImageOfEachStandardCursorByTheAppKitMemberThatDrawsIt(type: Int32, name: String) throws {
        let image: Pointer.Cursor.Image = try #require(Pointer.Cursor.Image(.core(type)), "CoreCursor type \(type) gives no image")
        let named = Pointer.Cursor(image)
        let expected: Pointer.Cursor = .system(name: name)
        expectNoDifference(named, expected, "CoreCursor type \(type)")
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
    func statesADragOnlyWhileTheApplicationShowsAWindowAtTheDraggingLevel(label: String, frame: CGRect, layer: Int, onScreen: Bool, draggingFrame: CGRect?) {
        let row = InstanceState.Window(
            number: 41,
            title: nil,
            frame: frame,
            layer: layer,
            band: InstanceState.Window.Band(layer: layer),
            parent: nil,
            spaces: [1],
            onScreen: onScreen,
            hidden: false,
            shared: true,
            remote: nil,
            asking: false,
            presented: onScreen,
        )
        let stated: CGRect? = Pointer(of: .current, windows: [row])?.draggingFrame
        expectNoDifference(stated, draggingFrame, label)
    }
}
