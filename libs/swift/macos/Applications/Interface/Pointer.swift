package import AppKit
import CasePathsCore
public import CoreGraphics
import CryptoKit
public import Encoding
import SPI
import Synchronization

// --- [MODELS] --------------------------------------------------------------------------

public struct Pointer: Encodable, Equatable, Sendable {
    public enum Cursor: CaseTagged, Equatable, Sendable {
        package struct Image: Sendable {
            package enum Source: Sendable {
                case global
                case system(Int32)
                case core(Int32)
            }

            package let data: Data
            package let hotSpot: CGPoint
            package let size: CGSize

            package init(data: Data, hotSpot: CGPoint, size: CGSize) {
                self.data = data
                self.hotSpot = hotSpot
                self.size = size
            }
        }

        case system(name: String)
        case custom(digest: String, hotSpot: CGPoint, size: CGSize)
    }

    public let cursor: Cursor?
    public let draggingFrame: CGRect?
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Pointer {
    private static let shown: Mutex<(seed: Int32, cursor: Cursor)?> = Mutex(nil)

    package init?(of application: NSRunningApplication, windows: [InstanceState.Window]) {
        let connection: Int32 = mainConnection()
        var (point, local, window, owner): (CGPoint, CGPoint, CGWindowID, Int32) = (.zero, .zero, kCGNullWindowID, 0)
        let located: Bool = unsafe application.isActive && currentCursorLocation(connection, &point) == .success
        let found: Bool = unsafe located && findWindowAndOwner(connection, 0, 1, 0, &point, &local, &window, &owner) == .success
        let owned: Bool = found && windows.contains { (row: InstanceState.Window) -> Bool in row.number == window }
        let cursor: Cursor? = Self.shown.withLock { shown -> Cursor? in
            guard owned else { return nil }
            let seed: Int32 = currentCursorSeed()
            guard shown?.seed != seed else { return shown?.cursor }
            guard let image = Cursor.Image(.global), currentCursorSeed() == seed else { return nil }
            shown = (seed: seed, cursor: Cursor(image))
            return shown?.cursor
        }
        let draggingFrame: CGRect? = windows.first { (row: InstanceState.Window) -> Bool in row.band == .dragging && row.onScreen }?.frame
        guard cursor != nil || draggingFrame != nil else { return nil }
        self.init(cursor: cursor, draggingFrame: draggingFrame)
    }
}

extension Pointer.Cursor {
    private static let catalogs: Mutex<[Float?: [SHA256.Digest: String]]> = Mutex([:])

    package init(_ image: Image) {
        var scale: Float = 0
        let scaled: Float? = unsafe cursorScale(mainConnection(), &scale) == .success ? scale : nil
        let names: [SHA256.Digest: String] = Self.catalogs.withLock { catalogs in
            if let built = catalogs[scaled] { return built }
            let coreType: String = "_coreCursorType"
            var count: UInt32 = 0
            let properties: UnsafeMutablePointer<objc_property_t>? = unsafe class_copyPropertyList(object_getClass(NSCursor.self), &count)
            defer { unsafe free(properties) }
            let typed: [(type: Int32, name: String)] = unsafe UnsafeBufferPointer(start: properties, count: Int(count)).compactMap { property in
                let name: String = unsafe String(cString: property_getName(property))
                let cursor: NSCursor? = Selector(name) == #selector(getter: NSCursor.current) ? nil : NSCursor.value(forKey: name) as? NSCursor
                let type: Int32? = cursor?.responds(to: Selector(coreType)) == true ? cursor?.value(forKey: coreType) as? Int32 : nil
                return type.map { type in (type: type, name: name) }
            }
            let appKit: [(digest: SHA256.Digest, name: String)] = Dictionary(grouping: typed, by: \.type).compactMap { type, aliases -> (digest: SHA256.Digest, name: String)? in
                guard let name = aliases.map(\.name).min(), let image = Image(.core(type)) else { return nil }
                return (digest: SHA256.hash(data: image.data), name: name)
            }
            let system: [(digest: SHA256.Digest, name: String)] = Array(
                (Int32(0)...).lazy
                    .map { id in unsafe systemCursorName(id).map { name in (id: id, name: unsafe String(cString: name)) } }
                    .prefix { cursor in cursor != nil }
                    .compactMap(\.self)
                    .compactMap { cursor in Image(.system(cursor.id)).map { image in (digest: SHA256.hash(data: image.data), name: cursor.name) } }
            )
            let built: [SHA256.Digest: String] = [system, appKit].reduce(into: [:]) { built, tier in
                built.merge(Dictionary(grouping: tier, by: \.digest).compactMapValues { named in named.count == 1 ? named.first?.name : nil }) { _, appKit in appKit }
            }
            catalogs[scaled] = built
            return built
        }
        let digest: SHA256.Digest = SHA256.hash(data: image.data)
        self = names[digest].map { name in .system(name: name) } ?? .custom(digest: Data(digest).base64EncodedString(), hotSpot: image.hotSpot, size: image.size)
    }
}

extension Pointer.Cursor.Image {
    package init?(_ source: Source) {
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
