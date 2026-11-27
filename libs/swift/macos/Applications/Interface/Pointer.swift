import AppKit
import CasePathsCore
import CoreGraphics
import CryptoKit
import Encoding
import SPI
import Synchronization

// --- [MODELS] --------------------------------------------------------------------------

struct Pointer: Encodable, Equatable, Sendable {
    enum Cursor: CaseTagged, Equatable, Sendable {
        struct Image: Sendable {
            enum Source: Sendable {
                case global
                case system(Int32)
                case core(Int32)
            }

            let data: Data
            let hotSpot: CGPoint
            let size: CGSize
        }

        case system(name: String)
        case custom(digest: String, hotSpot: CGPoint, size: CGSize)
    }

    let cursor: Cursor?
    let draggingFrame: CGRect?
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Pointer {
    private static let shown: Mutex<(seed: Int32, cursor: Cursor)?> = Mutex(nil)

    init?(of application: NSRunningApplication, windows: [InstanceState.Window]) {
        let owned: Bool =
            application.isActive
            && CGEvent(source: nil).map { (event: CGEvent) -> Bool in
                var (point, local, window, owner): (CGPoint, CGPoint, CGWindowID, Int32) = (event.location, .zero, kCGNullWindowID, 0)
                let found: Bool = unsafe findWindowAndOwner(mainConnection(), 0, 1, 0, &point, &local, &window, &owner) == .success
                return found && windows.contains { (row: InstanceState.Window) -> Bool in row.number == window }
            } ?? false
        let cursor: Cursor? =
            owned
            ? Self.shown.withLock { shown -> Cursor? in
                let seed: Int32 = currentCursorSeed()
                guard shown?.seed != seed else { return shown?.cursor }
                guard let image = Cursor.Image(.global), currentCursorSeed() == seed else { return nil }
                shown = (seed: seed, cursor: Cursor(image))
                return shown?.cursor
            } : nil
        let draggingFrame: CGRect? = windows.first { (row: InstanceState.Window) -> Bool in row.band == .dragging && row.onScreen }?.frame
        guard cursor != nil || draggingFrame != nil else { return nil }
        self.init(cursor: cursor, draggingFrame: draggingFrame)
    }
}

extension Pointer.Cursor {
    private static let catalogs: Mutex<[Float?: [SHA256.Digest: String]]> = Mutex([:])

    init(_ image: Image) {
        var scale: Float = 0
        let scaled: Float? = unsafe cursorScale(mainConnection(), &scale) == .success ? scale : nil
        let names: [SHA256.Digest: String] = Self.catalogs.withLock { catalogs in
            if let built = catalogs[scaled] { return built }
            let properties: [(getter: Selector, cursor: NSCursor)] = [
                (#selector(getter: NSCursor.arrow), NSCursor.arrow),
                (#selector(getter: NSCursor.crosshair), NSCursor.crosshair),
                (#selector(getter: NSCursor.disappearingItem), NSCursor.disappearingItem),
                (#selector(getter: NSCursor.operationNotAllowed), NSCursor.operationNotAllowed),
                (#selector(getter: NSCursor.dragLink), NSCursor.dragLink),
                (#selector(getter: NSCursor.dragCopy), NSCursor.dragCopy),
                (#selector(getter: NSCursor.contextualMenu), NSCursor.contextualMenu),
                (#selector(getter: NSCursor.pointingHand), NSCursor.pointingHand),
                (#selector(getter: NSCursor.closedHand), NSCursor.closedHand),
                (#selector(getter: NSCursor.openHand), NSCursor.openHand),
                (#selector(getter: NSCursor.iBeam), NSCursor.iBeam),
                (#selector(getter: NSCursor.iBeamCursorForVerticalLayout), NSCursor.iBeamCursorForVerticalLayout),
                (#selector(getter: NSCursor.zoomIn), NSCursor.zoomIn),
                (#selector(getter: NSCursor.zoomOut), NSCursor.zoomOut),
                (#selector(getter: NSCursor.columnResize), NSCursor.columnResize),
                (#selector(getter: NSCursor.rowResize), NSCursor.rowResize),
            ]
            let columns: [(name: String, cursor: NSCursor)] = NSHorizontalDirection.allCases.map { (direction: NSHorizontalDirection) -> (name: String, cursor: NSCursor) in
                (name: "\(NSStringFromSelector(#selector(getter: NSCursor.columnResize))) \(direction)", cursor: NSCursor.columnResize(directions: NSHorizontalDirection.Set(direction)))
            }
            let rows: [(name: String, cursor: NSCursor)] = NSVerticalDirection.allCases.map { (direction: NSVerticalDirection) -> (name: String, cursor: NSCursor) in
                (name: "\(NSStringFromSelector(#selector(getter: NSCursor.rowResize))) \(direction)", cursor: NSCursor.rowResize(directions: NSVerticalDirection.Set(direction)))
            }
            let singles: [[NSCursor.FrameResizeDirection]] = NSCursor.FrameResizeDirection.allCases.map { (direction: NSCursor.FrameResizeDirection) -> [NSCursor.FrameResizeDirection] in
                [direction]
            }
            let directions: [[NSCursor.FrameResizeDirection]] = singles + [NSCursor.FrameResizeDirection.allCases]
            let frames: [(name: String, cursor: NSCursor)] = NSCursor.FrameResizePosition.allCases.flatMap { (position: NSCursor.FrameResizePosition) -> [(name: String, cursor: NSCursor)] in
                let edge: String =
                    switch position {
                        case .top: "top"
                        case .left: "left"
                        case .bottom: "bottom"
                        case .right: "right"
                        case .topLeft: "topLeft"
                        case .topRight: "topRight"
                        case .bottomLeft: "bottomLeft"
                        case .bottomRight: "bottomRight"
                    }
                return directions.map { (set: [NSCursor.FrameResizeDirection]) -> (name: String, cursor: NSCursor) in
                    let resized = NSCursor.FrameResizeDirection.Set(
                        set.map { (direction: NSCursor.FrameResizeDirection) -> NSCursor.FrameResizeDirection.Set in NSCursor.FrameResizeDirection.Set(direction) }
                    )
                    let words: [String] = ["frameResizeCursor", edge] + set.map { (direction: NSCursor.FrameResizeDirection) -> String in "\(direction)" }
                    return (name: words.joined(separator: " "), cursor: NSCursor.frameResize(position: position, directions: resized))
                }
            }
            let named: [(name: String, cursor: NSCursor)] = properties.map { (getter: Selector, cursor: NSCursor) -> (name: String, cursor: NSCursor) in
                (name: NSStringFromSelector(getter), cursor: cursor)
            }
            let members: [(name: String, cursor: NSCursor)] = named + columns + rows + frames
            let types: [Int: (order: Int, name: String)] = Dictionary(
                members.enumerated()
                    .map { (order: Int, member: (name: String, cursor: NSCursor)) -> (Int, (order: Int, name: String)) in (member.cursor.coreCursorType(), (order: order, name: member.name)) }
                    .filter { (type: Int, _: (order: Int, name: String)) -> Bool in type != NSNotFound },
                uniquingKeysWith: { (first: (order: Int, name: String), _: (order: Int, name: String)) -> (order: Int, name: String) in first },
            )
            let appKit: [SHA256.Digest: (order: Int, name: String)] = Dictionary(
                types.compactMap { (type: Int, member: (order: Int, name: String)) -> (SHA256.Digest, (order: Int, name: String))? in
                    Image(.core(Int32(type))).map { (image: Image) -> (SHA256.Digest, (order: Int, name: String)) in (SHA256.hash(data: image.data), member) }
                },
                uniquingKeysWith: { (one: (order: Int, name: String), other: (order: Int, name: String)) -> (order: Int, name: String) in one.order < other.order ? one : other },
            )
            let system: [SHA256.Digest: String?] = Dictionary(
                (Int32(0)...).lazy
                    .map { (id: Int32) -> (id: Int32, name: String)? in
                        unsafe systemCursorName(id).map { (name: UnsafePointer<CChar>) -> (id: Int32, name: String) in (id: id, name: unsafe String(cString: name)) }
                    }
                    .prefix { (cursor: (id: Int32, name: String)?) -> Bool in cursor != nil }
                    .compactMap(\.self)
                    .compactMap { (cursor: (id: Int32, name: String)) -> (SHA256.Digest, String?)? in
                        Image(.system(cursor.id)).map { (image: Image) -> (SHA256.Digest, String?) in (SHA256.hash(data: image.data), cursor.name) }
                    },
                uniquingKeysWith: { (_: String?, _: String?) -> String? in nil },
            )
            let built: [SHA256.Digest: String] = system.compactMapValues(\.self).merging(appKit.mapValues(\.name)) { (_: String, named: String) -> String in named }
            catalogs[scaled] = built
            return built
        }
        let digest: SHA256.Digest = SHA256.hash(data: image.data)
        self = names[digest].map { (name: String) -> Self in .system(name: name) } ?? .custom(digest: Data(digest).base64EncodedString(), hotSpot: image.hotSpot, size: image.size)
    }
}

extension Pointer.Cursor.Image {
    init?(_ source: Source) {
        let connection: Int32 = mainConnection()
        var length: Int = 0
        let sized: CGError =
            switch source {
                case .global: unsafe globalCursorDataSize(connection, &length)
                case .system(let id): unsafe systemDefinedCursorDataSize(connection, id, &length)
                case .core(let type): unsafe coreCursorDataSize(connection, type, &length)
            }
        guard sized == .success, length > 0 else { return nil }
        var (rowBytes, bounds, hotSpot, depth, components, bits, frames, duration): (Int, CGRect, CGPoint, Int, Int, Int, Int, Int) = (0, .zero, .zero, 0, 0, 0, 0, 0)
        var data = Data(count: length)
        let read: CGError = unsafe data.withUnsafeMutableBytes { buffer in
            switch source {
                case .global: unsafe globalCursorData(connection, buffer.baseAddress, &length, &rowBytes, &bounds, &hotSpot, &depth, &components, &bits)
                case .system(let id): unsafe systemDefinedCursorData(connection, id, buffer.baseAddress, &length, &rowBytes, &bounds, &hotSpot, &depth, &components, &bits)
                case .core(let type): unsafe coreCursorData(connection, type, buffer.baseAddress, &length, &rowBytes, &bounds.size, &hotSpot, &depth, &components, &bits, &frames, &duration)
            }
        }
        guard read == .success else { return nil }
        self.init(data: data.prefix(length), hotSpot: hotSpot, size: bounds.size)
    }
}
