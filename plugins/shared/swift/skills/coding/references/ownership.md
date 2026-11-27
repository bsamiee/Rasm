# [OWNERSHIP]

## [01]-[NONCOPYABLE_RESOURCES]

Host registrations a release undoes (a run-loop source on the main run loop, an event tap's port, an FSEvents stream) are noncopyable structs: `init` registers, `deinit` undoes, and the owner's stored field is its one release path:

```swift
struct RunLoopSource: ~Copyable {
    let source: CFRunLoopSource
    let port: CFMachPort?

    init(_ source: CFRunLoopSource, port: CFMachPort? = nil) {
        CFRunLoopAddSource(CFRunLoopGetMain(), source, .commonModes)
        (self.source, self.port) = (source, port)
    }

    init(port: CFMachPort) {
        self.init(CFMachPortCreateRunLoopSource(nil, port, 0), port: port)
    }

    deinit {
        CFRunLoopRemoveSource(CFRunLoopGetMain(), source, .commonModes)
        port.map(CFMachPortInvalidate)
    }
}
```

AX observers enter as `RunLoopSource(AXObserverGetRunLoopSource(observer))`, event taps and window-server ports as `RunLoopSource(port:)`, which also invalidates the port. `.commonModes` keeps a source firing while the main run loop tracks a menu. Assigning `nil` or a new owner to a field releases its old one, in place of a `didSet` removing sources by hand or a release hung on another resource's `onTermination`:
- `Unmanaged.passUnretained` contexts keep their owner alive in no way, so the source owner is that owner's field, set to `nil` when its work ends
- `FSEventStreamRef` owners are `@safe` over pointer storage strict memory safety refuses unmarked, and stop, invalidate, and release the stream in `deinit`
- `discard self` accepts trivially destroyed stored properties alone (`FileDescriptor`, `FSEventStreamRef`), so an owner of a CF reference releases in `deinit` alone
- `FileDescriptor.closeAfter` scopes a descriptor one call reads, and an owner holds one past the call

Owners sit in storage that moves them, and `Array`, `Dictionary`, tuples, and CasePaths take copyable values alone:

| [INDEX] | [HOLDS]                         | [STORAGE]                               |
| :-----: | :------------------------------ | :-------------------------------------- |
|  [01]   | One owner a later call releases | Stored `Optional`, `nil` or `take()`    |
|  [02]   | Owner or its host refusal       | `Result<Owner, E>`, read by `switch`    |
|  [03]   | Owners registered together      | `[N of Owner]`                          |
|  [04]   | Owner shared across isolation   | `Mutex<Owner?>`, `take()` in `withLock` |

## [02]-[SPANS]

Binary records read and write through spans: offsets and byte order are values, every access checks bounds, and no `unsafe` pointer appears. Each container lends the span its operation needs:

| [INDEX] | [OPERATION]           | [SPAN]                                           |
| :-----: | :-------------------- | :----------------------------------------------- |
|  [01]   | Read a `Data` layout  | `RawSpan` from `bytes`                           |
|  [02]   | Read an array layout  | `RawSpan` from `span.bytes`                      |
|  [03]   | Write a `Data` layout | `MutableRawSpan` from `mutableBytes`             |
|  [04]   | Write an array layout | `MutableRawSpan` from `mutableSpan.mutableBytes` |

```swift
func focusRecord(window number: CGWindowID, subtype: NSEvent.EventSubtype) -> [UInt8] {
    var record = [UInt8](repeating: 0, count: 0xF8)
    var span = record.mutableSpan
    var bytes = span.mutableBytes
    bytes.storeBytes(of: 0xF8, toByteOffset: 0x04, as: UInt8.self)
    bytes.storeBytes(of: UInt8(NSEvent.EventType.appKitDefined.rawValue), toByteOffset: 0x08, as: UInt8.self)
    bytes.storeBytes(of: number, toByteOffset: 0x3C, as: CGWindowID.self, .littleEndian)
    bytes.storeBytes(of: UInt8(subtype.rawValue), toByteOffset: 0x8A, as: UInt8.self)
    return record
}
```

One store per field replaces a switch over every offset and an `unsafe withUnsafeBytes(of:)` copy of the window number:
- `load(fromByteOffset:as:_:)` reads a `FixedWidthInteger & ConvertibleFromBytes` at any alignment in either byte order, and one generic member decodes every integer width a descriptor holds
- `storeBytes(of:toByteOffset:as:_:)` writes a `ConvertibleToBytes` value, and byte order is its last argument
- Spans are `~Escapable`: a binding or non-escaping closure reads one inside the call, and no field or return value holds one
- C functions filling a buffer take `unsafe withUnsafeMutableBytes` at the call, and spans read its result

## [03]-[SYNCHRONIZED_STATE]

`Mutex` binds to a `let` alone: a stored `let` of a class or actor, or a `static let` for process-wide state. `withLock` runs a synchronous closure over `inout sending` state, returns its value as `sending`, and keeps the closure's typed failure:

```swift
final class Connection: Sendable {
    private struct State {
        var calls: [String: AsyncStream<Reply>.Continuation] = [:]
        var closed: ConnectionFailure?
    }

    private let state = Mutex(State())

    func register(_ id: String, _ reply: AsyncStream<Reply>.Continuation) throws(ConnectionFailure) {
        try state.withLock { state throws(ConnectionFailure) in
            if let closed = state.closed { throw closed }
            state.calls[id] = reply
        }
    }

    func close(_ ending: ConnectionFailure) {
        let calls = state.withLock { state in state.closed == nil ? exchange(&state, with: State(closed: ending)).calls : [:] }
        for reply in calls.values { reply.yield(.failure(ending)) }
    }
}
```

`Reply` and `ConnectionFailure` are the connection's reply and failure types. Writes before a throw stay, and `register` checks `closed` before it writes:
- Drains take and reset in one step through `exchange(&state, with:)` or `take()`, in place of a `defer` resetting state after `return`
- Work that suspends or runs a caller's handler reads what `withLock` returned, after the lock
- Lookups memoized under the lock read and fill one dictionary in one `withLock`
- `OSAllocatedUnfairLock` holds the `Copyable` state of a copyable struct, and noncopyable state takes `Mutex`
- `Atomic` binds to a `let` as `Mutex` does, counters read outside any lock use `.relaxed`, and a flag publishing other writes pairs `.releasing` with `.acquiring`
