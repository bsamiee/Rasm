# [INTEROPERABILITY]

## [01]-[IMPORTS]

Root `Package.swift` builds every target under strict memory safety with warnings as errors. Pointer parameters, `Unmanaged`, and `unsafeDowncast` make an expression unsafe, and C functions taking CF objects and scalars alone need no marker (`CFGetTypeID`, `AXValueGetType`, `CFRunLoopAddSource`):

```swift
@unsafe @preconcurrency import ApplicationServices

extension AXUIElement {
    var process: pid_t? {
        var pid: pid_t = 0
        return unsafe AXUIElementGetPid(self, &pid) == .success ? pid : nil
    }
}
```

`@preconcurrency import` accepts a module's unannotated C types (`AXUIElement`, `AXObserver`, `CGEvent`, `CFRunLoopSource`) and global C `var` constants (`kAXTrustedCheckOptionPrompt`) as `Sendable` with no check, and strict memory safety refuses it until `@unsafe` marks the race it accepts:
- Files whose C callback stores a callback argument in `Mutex` state take `@unsafe @preconcurrency import`, as `withLock` refuses a non-`Sendable` value from outside its `inout sending` region
- Files keeping every handle in one isolation take a plain `import`, and the compiler checks each crossing

`unsafe` covers the expression to its right, and the expression's shape places it:

| [INDEX] | [SHAPE]                                    | [MARKER]                                                            |
| :-----: | :----------------------------------------- | :------------------------------------------------------------------ |
|  [01]   | Call, assignment, or `defer` body          | `unsafe` before it                                                  |
|  [02]   | Closure passed to `withUnsafeMutableBytes` | Own `unsafe` inside, as an outer marker stops at its braces         |
|  [03]   | `guard` or `if` condition list             | One `unsafe` per clause reading a pointer                           |
|  [04]   | Binary operator, `??`, or ternary          | `unsafe` leading the whole expression, refused right of an operator |
|  [05]   | Expression with no unsafe use              | None, an extra marker warns and the package raises it to error      |

## [02]-[CORE_FOUNDATION_VALUES]

Untyped host values (`CFTypeRef` from an attribute read or an array element) decode in one failable initializer switching on `CFGetTypeID` into a closed enum with one case per host type:

```swift
extension Fact {
    init?(_ value: CFTypeRef, interning intern: (AXUIElement) -> Int) {
        switch CFGetTypeID(value) {
            case CFBooleanGetTypeID(): self = .bool(value: CFBooleanGetValue(unsafe unsafeDowncast(value, to: CFBoolean.self)))
            case CFNumberGetTypeID():
                let number = unsafe unsafeDowncast(value, to: CFNumber.self)
                self = CFNumberIsFloatType(number) ? .real(value: (number as NSNumber).doubleValue) : .integer(value: (number as NSNumber).intValue)
            case CFStringGetTypeID(): self = .text(value: unsafe unsafeDowncast(value, to: CFString.self) as String)
            case AXUIElementGetTypeID(): self = .element(id: intern(unsafe unsafeDowncast(value, to: AXUIElement.self)))
            case CFArrayGetTypeID():
                guard let items = value as? [AnyObject] else { return nil }
                self = .list(values: items.compactMap { item in Fact(item, interning: intern) })
            case AXValueGetTypeID():
                let boxed = unsafe unsafeDowncast(value, to: AXValue.self)
                let fact: Fact? =
                    switch AXValueGetType(boxed) {
                        case .cgPoint: boxed.unpacked(CGPoint.zero).map { point in .point(value: point) }
                        case .cgSize: boxed.unpacked(CGSize.zero).map { size in .size(value: size) }
                        case .cgRect: boxed.unpacked(CGRect.zero).map { rect in .rect(value: rect) }
                        case .cfRange: boxed.unpacked(CFRange()).map { range in .range(location: range.location, length: range.length) }
                        case .axError: boxed.unpacked(AXError.success).map { error in .refused(error: Lenient(rawValue: error.rawValue)) }
                        case .illegal: nil
                        @unknown default: nil
                    }
                guard let fact else { return nil }
                self = fact
            default: return nil
        }
    }
}
```

`Fact` is the module's enum of attribute values ending in `refused(error: Lenient<AXError>)`, and `unpacked(_:)` reads an `AXValue` into a `BitwiseCopyable` zero value through `AXValueGetValue`:
- Conditional casts to a CF type always succeed and the compiler refuses them, so the type id is the check and `unsafeDowncast` follows a matched id
- `CFBoolean` keeps its own case, where a bridged `as? NSNumber` reads it as a number, and `CFNumberIsFloatType` splits integer from real
- `AXValueGetType` splits boxed geometry, and a boxed `AXError` becomes a refused case holding its code in place of a missing key
- `CFAttributedString`, `CFDate`, and `CFURL` arms bridge the same way (`CFAttributedStringGetString`, `as Date`, `as URL`)
- Values already bridged to `Any` (a defaults domain) test the id in the cast arm: `case let number as NSNumber where CFGetTypeID(number) == CFBooleanGetTypeID()`
- `intern` maps a handle to the id its owner's table holds, and the decoder never holds the table
- CF handles hash and compare through `CFHash` and `CFEqual` (`_CFObject`), so each callback's new `AXUIElement` keys its element's entry

## [03]-[OUT_PARAMETERS]

C functions write results through pointer parameters, and Swift passes a local `var` with `&` and reads it after the returned status. Reads whose size the host reports call its size function, fill a `Data(count:)` buffer, and keep the length the fill wrote:

```swift
extension Image {
    init?(connection: Int32) {
        var length: Int = 0
        guard unsafe globalCursorDataSize(connection, &length) == .success, length > 0 else { return nil }
        var (rowBytes, bounds, hotSpot, depth, components, bits): (Int, CGRect, CGPoint, Int, Int, Int) = (0, .zero, .zero, 0, 0, 0)
        var bytes = Data(count: length)
        let read: CGError = unsafe bytes.withUnsafeMutableBytes { buffer in
            unsafe globalCursorData(connection, buffer.baseAddress, &length, &rowBytes, &bounds, &hotSpot, &depth, &components, &bits)
        }
        guard read == .success else { return nil }
        self.init(bytes: bytes.prefix(length), hotSpot: hotSpot, size: bounds.size)
    }
}
```

`globalCursorDataSize` and `globalCursorData` bind `CGSGetGlobalCursorDataSize` and `CGSGetGlobalCursorData`, and `Image` holds bytes, hot spot, and size:
- Out parameters of one call declare as one typed tuple `var`, and each element passes with `&`
- `Copy` and `Create` results a framework imports unannotated arrive as `Unmanaged<T>?` and leave through `takeRetainedValue()` (`OSACopyScriptingDefinitionFromURL`), `Get` results through `takeUnretainedValue()` (`TISGetInputSourceProperty`)
- Structs a C call types as another pointer pass through `withUnsafeMutablePointer(to:)` and `withMemoryRebound(to:capacity:)` (`proc_pid_rusage` takes `rusage_info_t?` for a `rusage_info_current`)
- C calls outside System fail by returned status (`posix_spawn*`) or by `-1` or `NULL` with global `errno`, read as `Errno(rawValue:)`
- NUL-terminated pointers read through `String(cString:)`, and a fixed C array passes `buffer.prefix { unit in unit != 0 }` to `String(validating:as:)`

C enumerations take the form their host shape fixes:

| [INDEX] | [HOST_SHAPE]                      | [FORM]                                                                  |
| :-----: | :-------------------------------- | :---------------------------------------------------------------------- |
|  [01]   | Copied array, count out parameter | `defer { unsafe free(<array>) }` beside the copy, `UnsafeBufferPointer` |
|  [02]   | Lookup by index until `NULL`      | `sequence(state:)` advancing the index in `defer`                       |
|  [03]   | Block called per item             | Non-escaping closure appending to a local                               |

```swift
func cursorNames() -> (appKit: [String], system: [(id: Int32, name: String)]) {
    var count: UInt32 = 0
    let properties: UnsafeMutablePointer<objc_property_t>? = unsafe class_copyPropertyList(object_getClass(NSCursor.self), &count)
    defer { unsafe free(properties) }
    let appKit: [String] = unsafe UnsafeBufferPointer(start: properties, count: Int(count)).map { property in unsafe String(cString: property_getName(property)) }
    let system: [(id: Int32, name: String)] = Array(
        sequence(state: Int32(0)) { (id: inout Int32) -> (id: Int32, name: String)? in
            defer { id += 1 }
            return unsafe systemCursorName(id).map { name in (id: id, name: unsafe String(cString: name)) }
        }
    )
    return (appKit, system)
}
```

`systemCursorName` binds `CGSCursorNameForSystemCursor`, which returns `NULL` past the last system cursor.

## [04]-[CALLBACKS]

Block parameters import as Swift closures that capture, and a block a header marks non-escaping mutates the caller's locals:

```swift
func architectures(of executable: FilePath) throws(Errno) -> [String] {
    var slices: [String] = []
    let status: Int32 = unsafe macho_for_each_slice(executable.string) { slice, _, _, _ in
        slices += unsafe macho_arch_name_for_mach_header(slice).map { name in [unsafe String(cString: name)] } ?? []
    }
    guard status == 0 else { throw Errno(rawValue: status) }
    return slices
}
```

`macho_for_each_slice` from `MachO.dyld.utils` takes a `__MACHO_NOESCAPE` block, maps each slice for the block's duration alone, and returns an `errno` value (`EFTYPE` for a file that is not Mach-O). C function pointers import as `@convention(c)` closures that capture nothing, so the owner reaches its callback through the registration's context pointer:

```swift
actor Watch {
    private let dirty: Mutex<[AXUIElement: Set<String>]> = Mutex([:])
    private let ticks: (stream: AsyncStream<Void>, continuation: AsyncStream<Void>.Continuation) = AsyncStream.makeStream(bufferingPolicy: .bufferingNewest(1))

    static let callback: AXObserverCallbackWithInfo = { _, element, notification, _, refcon in
        guard let refcon = unsafe refcon else { return }
        let watch: Watch = unsafe Unmanaged<Watch>.fromOpaque(refcon).takeUnretainedValue()
        watch.dirty.withLock { held in _ = held[element, default: []].insert(notification as String) }
        watch.ticks.continuation.yield()
    }

    func observe(_ observer: AXObserver, on application: AXUIElement, for names: [String]) -> [String: AXError] {
        let refcon = unsafe Unmanaged.passUnretained(self).toOpaque()
        return Dictionary(uniqueKeysWithValues: names.map { name in (name, unsafe AXObserverAddNotification(observer, application, name as CFString, refcon)) })
    }

    func drain(quiet: Duration) async {
        for await _ in ticks.stream.debounce(for: quiet) {
            settle(dirty.withLock { held in exchange(&held, with: [:]) })
        }
    }
}
```

`settle(_:)` is the actor's reconcile pass over changed elements:
- `passUnretained` hands over the owner's address with no retain, and `fromOpaque(_:).takeUnretainedValue()` recovers the owner in the callback
- Callbacks run outside the owner's isolation and touch its `Sendable` `let` members alone: a `Mutex` table of pending changes and a wake continuation
- Owners drain on their own executor under `debounce(for:)`, and `exchange(&held, with: [:])` takes the table in one critical section, so a burst becomes one pass
- Registrations keep each returned status, where `.notificationUnsupported` reads as absent and any other `AXError` as a refusal
- Event tap callbacks return `Unmanaged<CGEvent>?`: `passUnretained(event)` passes an event on, `nil` drops it, and `tapDisabledByTimeout` and `tapDisabledByUserInput` pass on
- Scalar contexts (a pid) pass as `UnsafeMutableRawPointer(bitPattern: Int(pid))` and read back as `pid_t(Int(bitPattern: refcon))`, with no object to keep alive
- FSEvents callbacks read `eventPaths` as C strings through `assumingMemoryBound(to: UnsafeMutablePointer<CChar>.self)`, in place of `kFSEventStreamCreateFlagUseCFTypes` and a CF array cast

Use `ownership.md` for noncopyable owners of run-loop sources, event tap ports, and FSEvents streams, and for the lifetime of an unretained context.

## [05]-[PRIVATE_SYMBOLS]

Private C symbols bind at link time as a Swift declaration with no body, named by `@_silgen_name`:

```swift
@_silgen_name("CGSMainConnectionID")
package func mainConnection() -> Int32

@_silgen_name("CGSGetConnectionIDForPSN")
private func connectionOfSerial(_: Int32, _: UnsafePointer<ProcessSerialNumber>, _: UnsafeMutablePointer<Int32>) -> CGError

@_silgen_name("SLEventSetAuthenticationMessage")
private func setAuthenticationMessage(_: CGEvent, _: AnyObject)
```

CoreGraphics re-exports SkyLight's `CGS*` and `CPS*` names, and a binding under that spelling links with CoreGraphics alone:
- Names SkyLight alone exports (`SLPSPostEventRecordTo`) link the SDK's `SkyLight.tbd` through target `linkerSettings` of `-F/System/Library/PrivateFrameworks` and `.linkedFramework("SkyLight")`, with no `dlsym` table
- Bindings take the narrowest access their readers need, `private` beside one reader and `package` in one shared file
- `@_silgen_name` declares the Swift calling convention (`@convention(thin)` in SIL), and it matches C on arm64 for integer, pointer, and floating-point aggregate arguments
- Class results are `@owned` and the caller releases them, so a CF result binds a `Copy` or `Create` function, and a `Get` result binds as `Unmanaged<T>?`
- `@_extern(c, "<symbol>")` declares the C convention under `-enable-experimental-feature Extern` and refuses CF types, `Unmanaged`, and `AnyObject` as not representable in C, so bindings passing a `CGEvent` or an object stay `@_silgen_name`

Private Objective-C classes resolve by `NSClassFromString`, and a private `@objc protocol` declares the selector that `AnyClass` dynamic lookup calls through optional chaining:

```swift
@objc private protocol EventAuthenticationMessage {
    @objc(messageWithEventRecord:pid:version:)
    static func message(eventRecord: UnsafeRawPointer, pid: pid_t, version: UInt32) -> AnyObject?
}

func message(signing record: [UInt8], for process: pid_t) -> AnyObject? {
    let authentication: AnyClass? = NSClassFromString("SLSEventAuthenticationMessage")
    return unsafe authentication?.message?(eventRecord: record, pid: process, version: 0) ?? nil
}
```

No type adopts the protocol, and its declaration makes the selector visible to `AnyObject` lookup in place of `perform(_:with:)`, which passes objects alone, or a `method(for:)` cast. A missing class or selector reads `nil`, and `?? nil` flattens the method's own optional.

## [06]-[OVERLAYS]

Headers marked `NS_REFINED_FOR_SWIFT` and apinotes rows with `SwiftPrivate: true` import their members under a `__` prefix, and the framework's Swift overlay declares a typed member over them. A search of the framework's `.swiftinterface` for the name without `__` finds it:

| [INDEX] | [REFINED]                                                | [OVERLAY]                                                |
| :-----: | :------------------------------------------------------- | :------------------------------------------------------- |
|  [01]   | `NSPasteboard.__detectMetadata(forTypes:)`, `as? UTType` | `detectedMetadata(for: [\.contentType]).contentType`     |
|  [02]   | `NSPasteboard.__detectPatterns(forPatterns:)`            | `detectedPatterns(for:)` over `DetectedValues` key paths |
|  [03]   | `CGEvent.__data(allocator:)`                             | `CGEvent.data`                                           |

Code calls the overlay, and the `__` form stays where the overlay drops a value its consumer reads: a wire listing pattern names reads `__NSPasteboardDetectionPattern.rawValue`, a value no `DetectedValues` key path holds.
