package import AppKit
import CasePathsCore
public import CoreGraphics
import CryptoKit
public import Encoding
import Synchronization

// --- [MODELS] --------------------------------------------------------------------------

public struct Pointer: Encodable, Equatable, Sendable {
    public enum Cursor: CaseTagged, Equatable, Sendable {
        package struct Image: Equatable, Sendable {
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

    public let tool: String?
    public let help: String?
    public let cursor: Cursor?
    public let draggingFrame: CGRect?
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Pointer {
    package init?(of application: NSRunningApplication, windows: [InstanceState.Window], containers: [Container]) {
        let focused: Set<Int> = Set(containers.compactMap { (container: Container) -> Int? in container.focused ?? false ? container.ref : nil })
        let toolbars: [Container] = containers.filter { (container: Container) -> Bool in container.kind == Container.Kind.toolbar }
        let choices: [(focused: Bool, control: Control)] = toolbars.flatMap { (toolbar: Container) -> [(focused: Bool, control: Control)] in
            let within: Bool = toolbar.parent.map { (parent: Int) -> Bool in focused.contains(parent) } ?? false
            let named: [Control] = toolbar.controls.filter { (control: Control) -> Bool in
                let text: String? = control.value?.text
                return control.kind == Control.Kind.choice && control.selected != nil && text != nil
            }
            return named.map { (control: Control) -> (focused: Bool, control: Control) in (focused: within, control: control) }
        }
        let anyFocused: Bool = choices.contains(where: \.focused)
        let candidates: [Control] = choices.compactMap { (choice: (focused: Bool, control: Control)) -> Control? in choice.focused || !anyFocused ? choice.control : nil }
        let chosen: Control? = candidates.count == 1 ? candidates.first : nil
        let tool: String? = chosen?.value?.text
        let help: String? = chosen?.options?.first { (option: Control.Option) -> Bool in option.label == tool }?.help
        let seed: Int32 = currentCursorSeed()
        let point: CGPoint? = application.isActive ? CGEvent(source: nil)?.location : nil
        let under: CGWindowID? = point.flatMap { (point: CGPoint) -> CGWindowID? in
            var (screen, local, window, connection): (CGPoint, CGPoint, CGWindowID, Int32) = (point, .zero, 0, 0)
            let answer: CGError = unsafe findWindowAndOwner(mainConnection(), 0, 1, 0, &screen, &local, &window, &connection)
            return answer == .success ? window : nil
        }
        let owned: Bool = under.map { (number: CGWindowID) -> Bool in windows.contains { (row: InstanceState.Window) -> Bool in row.number == number } } ?? false
        let shown: Cursor.Image? = owned ? Cursor.Image(.global) : nil
        let steady: Bool = shown != nil && Cursor.Image(.global) == shown && currentCursorSeed() == seed
        let cursor: Cursor? = steady ? shown.map { (image: Cursor.Image) -> Cursor in Cursor(image) } : nil
        let level = Int(CGWindowLevelForKey(.draggingWindow))
        let draggingFrame: CGRect? = windows.first { (row: InstanceState.Window) -> Bool in row.layer == level }?.frame
        guard tool != nil || cursor != nil || draggingFrame != nil else { return nil }
        self.init(tool: tool, help: help, cursor: cursor, draggingFrame: draggingFrame)
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
                return type.flatMap { type in Image(.core(type)) }.map { image in (digest: SHA256.hash(data: image.data), name: name) }
            }
            let defined: [(id: Int32, name: String)] = Array(
                (Int32(0)...).lazy
                    .map { id in unsafe systemCursorName(id).map { name in (id: id, name: unsafe String(cString: name)) } }
                    .prefix { cursor in cursor != nil }
                    .compactMap(\.self)
            )
            let system: [(digest: SHA256.Digest, name: String)] = defined.compactMap { cursor in Image(.system(cursor.id)).map { image in (digest: SHA256.hash(data: image.data), name: cursor.name) } }
            let built: [SHA256.Digest: String] = [system, appKit].reduce(into: [:]) { built, tier in
                built.merge(Dictionary(grouping: tier, by: \.digest).compactMapValues { named in named.count == 1 ? named.first?.name : nil }) { _, appKit in appKit }
            }
            held[scaled] = built
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

@_silgen_name("CGSCurrentCursorSeed")
private func currentCursorSeed() -> Int32

@_silgen_name("SLSFindWindowAndOwner")
private func findWindowAndOwner(
    _: Int32,
    _: Int32,
    _: Int32,
    _: Int32,
    _: UnsafeMutablePointer<CGPoint>,
    _: UnsafeMutablePointer<CGPoint>,
    _: UnsafeMutablePointer<CGWindowID>,
    _: UnsafeMutablePointer<Int32>,
) -> CGError

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
