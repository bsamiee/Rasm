public import AppKit
@unsafe @preconcurrency import ApplicationServices
import CryptoKit
import Synchronization

// --- [MODELS] --------------------------------------------------------------------------

public struct Pointer: Encodable, Equatable, Sendable {
    public enum Cursor: Encodable, Equatable, Sendable {
        package struct Image: Equatable, Sendable {
            package enum Source: Sendable {
                case global
                case system(Int32)
                case core(Int32)
            }

            package let bytes: Data
            package let hotSpot: CGPoint
            package let size: CGSize

            package init(bytes: Data, hotSpot: CGPoint, size: CGSize) {
                self.bytes = bytes
                self.hotSpot = hotSpot
                self.size = size
            }
        }

        case system(name: String)
        case custom(digest: String, hotSpot: CGPoint, size: CGSize)
    }

    public let cursor: Cursor?
    public let dragging: CGRect?
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Pointer {
    package init?(of application: NSRunningApplication, windows: [InstanceState.Window]) {
        var element: AXUIElement?
        let point: CGPoint? = application.isActive ? CGEvent(source: nil)?.location : nil
        let hit: AXError? = point.map { point in unsafe AXUIElementCopyElementAtPosition(AXUIElementCreateSystemWide(), Float(point.x), Float(point.y), &element) }
        let seed: Int32 = currentCursorSeed()
        let shown: Cursor.Image? = hit == .success && element?.process == application.processIdentifier ? Cursor.Image(.global) : nil
        let cursor: Cursor? = shown.flatMap { shown in Cursor.Image(.global) == shown && currentCursorSeed() == seed ? Cursor(shown) : nil }
        let dragging: CGRect? = windows.first { [level = Int(CGWindowLevelForKey(.draggingWindow))] row in row.layer == level }?.frame
        guard cursor != nil || dragging != nil else { return nil }
        self.init(cursor: cursor, dragging: dragging)
    }
}

extension Pointer.Cursor {
    private static let catalogs: Mutex<[Float?: [SHA256.Digest: String]]> = Mutex([:])

    package init(_ image: Image) {
        var scale: Float = 0
        let scaled: Float? = unsafe cursorScale(mainConnection(), &scale) == .success ? scale : nil
        let names: [SHA256.Digest: String] = Self.catalogs.withLock { held in
            if let built = held[scaled] { return built }
            _ = NSCursor.current
            var count: UInt32 = 0
            let properties: UnsafeMutablePointer<objc_property_t>? = unsafe class_copyPropertyList(object_getClass(NSCursor.self), &count)
            defer { unsafe free(properties) }
            let appKit: [(digest: SHA256.Digest, name: String)] = unsafe UnsafeBufferPointer(start: properties, count: Int(count)).compactMap { property in
                let name: String = unsafe String(cString: property_getName(property))
                let type: Int32? = Selector(name) == #selector(getter: NSCursor.current) ? nil : (NSCursor.value(forKey: name) as? NSCursor)?.value(forKey: "_coreCursorType") as? Int32
                return type.flatMap { type in Image(.core(type)) }.map { image in (digest: SHA256.hash(data: image.bytes), name: name) }
            }
            let system: [(digest: SHA256.Digest, name: String)] = Swift.sequence(state: Int32(0)) { (id: inout Int32) -> (id: Int32, name: String)? in
                defer { id += 1 }
                return unsafe systemCursorName(id).map { name in (id: id, name: unsafe String(cString: name)) }
            }
            .compactMap { cursor in Image(.system(cursor.id)).map { image in (digest: SHA256.hash(data: image.bytes), name: cursor.name) } }
            let built: [SHA256.Digest: String] = [system, appKit].reduce(into: [:]) { built, tier in
                built.merge(Dictionary(grouping: tier, by: \.digest).compactMapValues { named in named.count == 1 ? named.first?.name : nil }) { _, appKit in appKit }
            }
            held[scaled] = built
            return built
        }
        let digest: SHA256.Digest = SHA256.hash(data: image.bytes)
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
        var bytes = Data(count: length)
        let read: CGError = unsafe bytes.withUnsafeMutableBytes { buffer in
            switch source {
                case .global: unsafe globalCursorData(connection, buffer.baseAddress, &length, &rowBytes, &bounds, &hotSpot, &depth, &components, &bits)
                case .system(let id): unsafe systemDefinedCursorData(connection, id, buffer.baseAddress, &length, &rowBytes, &bounds, &hotSpot, &depth, &components, &bits)
                case .core(let type): unsafe coreCursorData(connection, type, buffer.baseAddress, &length, &rowBytes, &bounds.size, &hotSpot, &depth, &components, &bits, &frames, &duration)
            }
        }
        guard read == .success else { return nil }
        self.init(bytes: bytes.prefix(length), hotSpot: hotSpot, size: bounds.size)
    }
}

@_silgen_name("CGSCurrentCursorSeed")
private func currentCursorSeed() -> Int32

@_silgen_name("CGSGetCursorScale")
private func cursorScale(_: Int32, _: UnsafeMutablePointer<Float>) -> CGError

@_silgen_name("CGSCursorNameForSystemCursor")
private func systemCursorName(_: Int32) -> UnsafePointer<CChar>?

@_silgen_name("CGSGetGlobalCursorDataSize")
private func globalCursorDataSize(_: Int32, _: UnsafeMutablePointer<Int>) -> CGError

@_silgen_name("CGSGetGlobalCursorData")
private func globalCursorData(
    _: Int32,
    _: UnsafeMutableRawPointer?,
    _: UnsafeMutablePointer<Int>,
    _: UnsafeMutablePointer<Int>,
    _: UnsafeMutablePointer<CGRect>,
    _: UnsafeMutablePointer<CGPoint>,
    _: UnsafeMutablePointer<Int>,
    _: UnsafeMutablePointer<Int>,
    _: UnsafeMutablePointer<Int>,
) -> CGError

@_silgen_name("CGSGetSystemDefinedCursorDataSize")
private func systemDefinedCursorDataSize(_: Int32, _: Int32, _: UnsafeMutablePointer<Int>) -> CGError

@_silgen_name("CGSGetSystemDefinedCursorData")
private func systemDefinedCursorData(
    _: Int32,
    _: Int32,
    _: UnsafeMutableRawPointer?,
    _: UnsafeMutablePointer<Int>,
    _: UnsafeMutablePointer<Int>,
    _: UnsafeMutablePointer<CGRect>,
    _: UnsafeMutablePointer<CGPoint>,
    _: UnsafeMutablePointer<Int>,
    _: UnsafeMutablePointer<Int>,
    _: UnsafeMutablePointer<Int>,
) -> CGError

@_silgen_name("CoreCursorGetDataSize")
private func coreCursorDataSize(_: Int32, _: Int32, _: UnsafeMutablePointer<Int>) -> CGError

@_silgen_name("CoreCursorGetData")
private func coreCursorData(
    _: Int32,
    _: Int32,
    _: UnsafeMutableRawPointer?,
    _: UnsafeMutablePointer<Int>,
    _: UnsafeMutablePointer<Int>,
    _: UnsafeMutablePointer<CGSize>,
    _: UnsafeMutablePointer<CGPoint>,
    _: UnsafeMutablePointer<Int>,
    _: UnsafeMutablePointer<Int>,
    _: UnsafeMutablePointer<Int>,
    _: UnsafeMutablePointer<Int>,
    _: UnsafeMutablePointer<Int>,
) -> CGError
