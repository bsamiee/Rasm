# [GENERICS]

## [01]-[CONSTRAINTS]

Constraints name the narrowest protocol a body calls, a standard protocol before a declared one, and layout protocols admit reads of C memory. `BitwiseCopyable` admits every trivially copied struct as a `proc_pidinfo` out parameter:

```swift
func information<Info: BitwiseCopyable>(of process: pid_t, _ flavor: Int32, _ empty: Info, argument: UInt64 = 0) throws(Errno) -> Info {
    var info = empty
    guard unsafe proc_pidinfo(process, flavor, argument, &info, Int32(MemoryLayout<Info>.size)) > 0 else { throw Errno(rawValue: errno) }
    return info
}
```

- C structs import with a zero initializer, and a caller's zero value (`proc_bsdinfo()`, `UInt64.zero`) fixes `Info` and seeds the read
- `FixedWidthInteger & ConvertibleFromBytes` admits `load(fromByteOffset:as:)` at any width, a metatype (`exact(Int64.self)`) picks one
- Loops differing in element type and per-element fact are one generic member: `next<Matched>` takes `(Change) -> Matched?` for every event
- Primary associated types constrain inside brackets (`AsyncIteratorProtocol<UInt8, Failure>`), `where` holds the rest (`Known.RawValue: Codable`)

## [02]-[FAILURE_PARAMETERS]

Members that never construct a failure take `Failure: Error` and pass their body's failure through (`race`, `withTimeout`, `ifPresent`). Typed throws admits one error type, so a scoped member whose own failures join its body's declares each constructor it calls as a protocol requirement, and the caller's module error conforms through its own reason cases. `Lockfile.withLocks<Value, Failure: LockFailure>` throws the body's `Failure` for a held file, a cancelled wait, and a failed release:

```swift
public protocol LockFailure: Error {
    static func lock(_ failure: FileFailure) -> Self
    static func cancelled(_ error: CancellationError) -> Self
    func releasing(_ release: Result<Void, Self>) -> Self
}

extension AccountFailure: LockFailure {
    static func lock(_ failure: FileFailure) -> Self { Self(subject: failure.subject, reason: .lock(failure)) }

    static func cancelled(_: CancellationError) -> Self { Self(subject: "Lock", reason: .cancelled) }

    func releasing(_ release: Result<Void, Self>) -> Self {
        switch release {
            case .success: self
            case .failure(let error): Self(subject: subject, reason: .cleanup(operation: self, release: error))
        }
    }
}
```

- Static requirements are function values at the call, `mapError(Failure.lock)` lifts each release failure with no closure
- `releasing` holds a body failure beside its release failure in `cleanup(operation:release:)`, and a caller reads its cause from `operation`
- Libraries declare no error wrapping the body's, and a caller switches over its own reasons, `lock(FileFailure)` among them

## [03]-[OPAQUE_TYPES]

Members returning an adapter state `some P<A>` with every primary associated type, and the caller keeps element and failure while the concrete type stays hidden. `jsonLines` splits any byte sequence into lines and keeps its typed failure:

```swift
struct JSONLines<Bytes: AsyncIteratorProtocol<UInt8, Failure>, Failure: Error>: AsyncSequence, AsyncIteratorProtocol {
    var bytes: Bytes

    func makeAsyncIterator() -> Self { self }

    mutating func next(isolation actor: isolated any Actor?) async throws(Failure) -> Data? {
        var line = Data()
        repeat {
            switch try await bytes.next(isolation: actor) {
                case UInt8(ascii: "\n"): return line
                case let byte?: line.append(byte)
                case nil: return line.isEmpty ? nil : line
            }
        } while true
    }
}

extension AsyncSequence where Element == UInt8 {
    public var jsonLines: some AsyncSequence<Data, Failure> { JSONLines(bytes: makeAsyncIterator()) }
}
```

- Types serving as their own iterator return `self` from `makeAsyncIterator()`, one type in place of a sequence and iterator pair
- `AsyncIteratorProtocol` extensions derive `next()` from `next(isolation:)` and the reverse, a conformer writes the typed `next(isolation:)` alone
- Every return path yields one concrete type, and a choice between adapters becomes a parameter of one adapter

## [04]-[PROTOCOL_EXTENSIONS]

Requirements dispatch through the conformance and other extension members bind statically, so an operation a conformer replaces is a requirement with its default in an extension. A protocol refining `Encodable` supplies `encode(to:)` in its extension, that witness stands in place of synthesis, and a conformer states the protocol alone:

```swift
public protocol CaseTagged: Encodable {}

extension CaseTagged {
    public func encode(to encoder: any Encoder) throws {
        let held: Mirror.Child? = Mirror(reflecting: self).children.first
        var fields = encoder.container(keyedBy: Field.self)
        try fields.encode(held?.label ?? String(describing: self), forKey: .tag)
        switch held?.value {
            case .none: return
            case .some(let value as any Encodable): try value.encode(to: encoder)
            case .some(let value): throw EncodingError.invalidValue(value, EncodingError.Context(codingPath: encoder.codingPath, debugDescription: "\(Self.self) case \(self) holds no Encodable value"))
        }
    }
}
```

- `Mirror` reads the case label and payload at run time, `Field` is a `CodingKey` built from any string, and a keyed payload's own `encode(to:)` writes its fields beside `_tag`
- Labeled payloads read as a tuple whose `Mirror` children carry each label, so each child encodes under its label and an absent optional goes
- `any P` opens at a `some P` or generic parameter, so `value.encode(to:)` and `fields.encode(value, forKey:)` take an `any Encodable` directly
- Existentials in `??`, a ternary, or a literal open nothing, and a value opens once inside `map` or a local function

## [05]-[CONDITIONAL_CONFORMANCES]

Generic types conform where their parameters do, one extension per capability, and an empty extension lets synthesis write the members. One JSON-RPC message type serves requests, replies, and errors in both directions:

```swift
struct Message<Payload: Sendable, Fault: Sendable>: Sendable {
    var id: JSONValue?
    var method: String?
    var params: Payload?
    var result: Payload?
    var error: Fault?
}

extension Message: Encodable where Payload: Encodable, Fault: Encodable {}
```

- `Never` is `Encodable` and `Decodable`, so `Message<_, Never>` encodes a request and `Message<Never, Cause>` an error with no payload type
- `Decodable` takes a hand `init(from:)` in its own conditional extension, as the wire tells `result: null` apart from a missing `result`
- `Lenient` is `Equatable` and `Hashable` where its known type and raw value are, `AggregateError` a `LocalizedError` where its element is an `Error`
- Under `.defaultIsolation(MainActor.self)`, values decoded or compared off the main actor are a `nonisolated struct`, as a nonisolated `JSONDecoder.decode` refuses an isolated conformance
- Witnesses of a `nonisolated` conformance on a main-actor class (`Draft: nonisolated Decodable`) initialize or read `let` properties alone

## [06]-[CASE_KEY_PATHS]

`@CasePathable` gives each case a `CaseKeyPath<Root, Value>`, a value one generic member takes in place of an extracting closure per case:

```swift
public func next<Value>(_ event: CaseKeyPath<Event, Value>) async throws -> Value {
    for try await read in events {
        if let value = read[case: event] { return value }
    }
    throw StreamFailure(subject: String(describing: event))
}
```

- Callers write `reader.next(\.opened)`, and the key path fixes `Value` to the case's payload
- Matches testing a payload field (`.opened(let item, .primary)`) stay an extracting closure, as a case key path reads its case whole
- `Case<Value>` requires a copyable `Value`, so a `~Copyable` payload (a run-loop source in a `Result`) reads through `switch`
