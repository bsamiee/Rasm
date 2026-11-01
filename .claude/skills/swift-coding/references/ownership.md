# [OWNERSHIP]

Covers values with one owner, from a resource that closes itself to borrowed views and state guarded across tasks.

## [01]-[UNIQUE_RESOURCES]

Resources with a changing owner (stored, returned, handed to a task) are noncopyable structs holding the host handle alone. `deinit` releases it on every path a caller abandons, and a `consuming` method reporting a release error ends its value with `discard self`:

```swift
struct OpenFile: ~Copyable {
    private let descriptor: FileDescriptor

    init(creating path: FilePath, permissions: FilePermissions = .ownerReadWrite) throws(OrderError) {
        do { descriptor = try .open(path, .writeOnly, options: [.create, .truncate], permissions: permissions) } catch { throw OrderError(subject: path.string, reason: .unreadable(error)) }
    }

    borrowing func write(_ bytes: some Sequence<UInt8>) throws(OrderError) {
        do { try descriptor.writeAll(bytes) } catch { throw OrderError(subject: "descriptor \(descriptor.rawValue)", reason: .unreadable(error)) }
    }

    consuming func close() throws(OrderError) {
        let handle: FileDescriptor = descriptor
        discard self
        do { try handle.close() } catch { throw OrderError(subject: "descriptor \(handle.rawValue)", reason: .unreadable(error)) }
    }

    deinit {
        try? descriptor.close()
    }
}
```

`OrderError` is the module's error type. Copies, a write after `close`, and a second `close` are compile errors. `discard self` accepts trivially destroyed stored properties alone, so the type stores its handle and each failure names it. Generic code over a noncopyable value suppresses the implicit requirement (`<Value: ~Copyable>`), and a generic type conforms to `Copyable` where its parameter does, as `Optional` and `Result` do.

Noncopyable values sit in storage that moves its elements, `Array` and `Dictionary` require copyable ones:

| [INDEX] | [HOLDS]                         | [STORAGE]                                |
| :-----: | :------------------------------ | :--------------------------------------- |
|  [01]   | One value a later call releases | `Optional`, `take()`                     |
|  [02]   | Count the input decides         | `UniqueArray`, a `for` loop borrows each |
|  [03]   | Count fixed in the type         | `InlineArray`                            |
|  [04]   | One value on the heap           | `UniqueBox`, `consume()`                 |
|  [05]   | One value shared across tasks   | `Mutex`                                  |

## [02]-[TYPESTATE]

Workflow states known at compile time are noncopyable types. Each state holds its previous state and the fact its transition added, so a forward transition is the next state's memberwise init consuming its predecessor, no field repeats, and a backward transition returns an earlier state whole:

```swift
struct Draft: ~Copyable {
    let lines: [Line]
}

struct Submitted: ~Copyable {
    let draft: Draft
    let at: Date

    consuming func returned() -> Draft {
        draft
    }

    consuming func approved(by reviewer: Account.ID) -> Approval {
        Approval(lines: draft.lines, submitted: at, reviewer: reviewer)
    }
}
```

`Line`, `Account.ID`, and `Approval` are the module's line, account identifier, and copyable approval record. Approving a draft and submitting a draft twice are compile errors. `returned()` takes `draft` out of a consumed value through partial consumption, which holds for a state with no `deinit`. Final transitions return a copyable record, and ownership tracking ends with the workflow:
- Transitions take validated inputs and stay total, a `consuming` method that throws loses its consumed state with the error
- States read from storage or the network decode into an enum, and each `switch` arm constructs its typestate value

## [03]-[BORROWED_VIEWS]

Spans lend contiguous storage to a call without a copy or a pointer, and a span ends when its call returns. Operations on storage select the span:

| [INDEX] | [OPERATION]                | [SPAN]                                                                           |
| :-----: | :------------------------- | :------------------------------------------------------------------------------- |
|  [01]   | Read elements              | `Span<Element>` from `span`                                                      |
|  [02]   | Read a binary layout       | `RawSpan` from `bytes`, `span.bytes`                                             |
|  [03]   | Rewrite elements in place  | `MutableSpan<Element>` from `mutableSpan`                                        |
|  [04]   | Fill uninitialized storage | `OutputSpan` from `withTemporaryAllocation`, `Array(capacity:initializingWith:)` |

```swift
func values<Value: FixedWidthInteger & ConvertibleFromBytes>(in bytes: RawSpan, order: ByteOrder = .bigEndian) -> [Value] {
    let width: Int = MemoryLayout<Value>.size
    return (0..<bytes.byteCount / width).map { index in bytes.load(fromByteOffset: index * width, as: Value.self, order) }
}

let levels: [Int16] = values(in: frame.bytes)
```

`frame` is a `Data`. `load(fromByteOffset:as:_:)` checks bounds and takes byte order as a value, so one function decodes every fixed-width integer in either order, and the binding's type picks `Value`. `Data` lends `bytes`, `Array` and `InlineArray` lend `span.bytes`. Writers append a layout in a stated byte order through `OutputRawSpan.append(_:as:_:)`.

## [04]-[SYNCHRONIZED_STATE]

State that synchronous code updates from concurrent tasks sits in a `Mutex`. `Mutex` and `Atomic` bind to `let` alone, as a stored `let` of a `final class: Sendable` or a global `let` for process-wide state. `withLock` returns its closure's value and keeps its typed failure, so a check and its write are one critical section:

```swift
final class Ledger: Sendable {
    private let balances: Mutex<[Account.ID: Decimal]>

    init(opening balances: [Account.ID: Decimal]) {
        self.balances = Mutex(balances)
    }

    func debit(_ amount: Decimal, from account: Account.ID) throws(OrderError) -> Decimal {
        try balances.withLock { (entries: inout [Account.ID: Decimal]) throws(OrderError) -> Decimal in
            let remaining: Decimal = entries[account, default: 0] - amount
            guard remaining >= 0 else { throw OrderError(subject: "\(account)", reason: .insufficientFunds(shortBy: -remaining)) }
            entries[account] = remaining
            return remaining
        }
    }
}
```

Writes inside `withLock` before a throw stay, so the closure checks before it writes:
- `Mutex` stays `Sendable` over a non-`Sendable` value, and `withLock` lends it as `inout sending`, so no reference to it leaves the lock
- `withLockIfAvailable` returns `nil` while another thread holds the lock, for work a contended caller skips
- `Atomic` counters read outside any lock use `.relaxed`, a flag publishing other writes pairs a `.releasing` store with an `.acquiring` load
