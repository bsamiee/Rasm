# [OWNERSHIP]

Values with one owner at a time and the views and locks over their storage.

## [01]-[NONCOPYABLE_RESOURCES]

Resources with a changing owner (stored, returned, handed to a task) are noncopyable structs holding the host handle alone and adding its lifetime. A `consuming` method reporting a release error ends its value with `discard self`:

```swift
struct OpenFile: ~Copyable {
    let descriptor: FileDescriptor

    consuming func close() throws(OrderError) {
        let descriptor: FileDescriptor = descriptor
        discard self
        do { try descriptor.close() } catch { throw OrderError(subject: "descriptor \(descriptor.rawValue)", reason: .failed(error)) }
    }

    deinit {
        try? descriptor.close()
    }
}
```

Callers acquire through the memberwise init over the host call (`try OpenFile(descriptor: .open(path, .readOnly))`) and call host members on `descriptor` directly. `OrderError.Reason.failed(any Error)` holds the cause the untyped `close()` throws. `discard self` accepts trivially destroyed stored properties alone, and the owner stores its handle and nothing else:
- Generic code over a noncopyable value suppresses the implicit requirement (`<Value: ~Copyable>`), and a generic type conforms to `Copyable` where its parameter does, as `Optional` and `Result` do
- `FileDescriptor.closeAfter` scopes a descriptor one call reads, a noncopyable owner holds one that outlives the call

Noncopyable values sit in storage that moves its elements, and `Array` and `Dictionary` require copyable ones:

| [INDEX] | [HOLDS]                         | [STORAGE]                |
| :-----: | :------------------------------ | :----------------------- |
|  [01]   | One value a later call releases | `Optional`, `take()`     |
|  [02]   | Count the input decides         | `UniqueArray`            |
|  [03]   | Count fixed in the type         | `[N of T]`               |
|  [04]   | One value on the heap           | `UniqueBox`, `consume()` |
|  [05]   | One value shared across tasks   | `Mutex`                  |

Functions reading elements in order take `borrowing some Iterable<Element, Never> & ~Copyable & ~Escapable`, and one `for` loop borrows each element of a span, `[N of T]`, or `UniqueArray`.

## [02]-[TYPESTATE]

Workflow states known at compile time are noncopyable types. Each state holds its previous state and the fact its transition added: a forward transition is the next state's memberwise init consuming its predecessor with no field repeated, and a backward transition returns an earlier state whole:

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

`Line`, `Account.ID`, and `Approval` are the module's line, account identifier, and copyable approval record. `returned()` takes `draft` out of a consumed value through partial consumption, legal in a state with no `deinit`. Final transitions return a copyable record, and ownership tracking ends with the workflow:
- Transitions take validated inputs and stay total, a `consuming` method that throws loses its consumed state with the error
- States read from storage or the network decode into an enum, and each `switch` arm constructs its typestate value

## [03]-[BORROWED_VIEWS]

Spans lend contiguous storage to a call without a copy or an escaping pointer and end when the call returns. Functions reading contiguous values take a span, every container lends to one signature, and the operation selects the span:

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

`frame` is a `Data`. `load(fromByteOffset:as:_:)` checks bounds, reads at any alignment, and takes byte order as a value, one function decodes every fixed-width integer in either order, and the binding's type picks `Value`. `Data` lends `bytes`, `Array` and `InlineArray` lend `span.bytes`. Writers state the order through `storeBytes(of:toByteOffset:as:_:)` on `Data.mutableBytes`.

## [04]-[SYNCHRONIZED_STATE]

State that synchronous code updates from concurrent tasks sits in a `Mutex`. `Mutex` and `Atomic` bind to `let` alone, as a stored `let` of a `final class: Sendable` or a global `let` for process-wide state. Each update is a function over `inout` state that `withLock` runs as one critical section, and an actor method or a local `var` runs the same function:

```swift
func debit(_ amount: Decimal, from account: Account.ID, in balances: inout [Account.ID: Decimal]) throws(OrderError) {
    let balance: Decimal = balances[account, default: 0]
    guard balance >= amount else { throw OrderError(subject: "debit \(amount) against \(balance)", reason: .insufficientFunds(shortBy: amount - balance)) }
    balances[account] = balance - amount
}

try ledger.withLock { balances throws(OrderError) in try debit(amount, from: account, in: &balances) }
```

`ledger` is a `Mutex<[Account.ID: Decimal]>`. `withLock` returns its closure's value and keeps the typed failure its closure states. Writes before a throw stay, and the function checks before it writes:
- `Mutex` stays `Sendable` over a non-`Sendable` value, and `withLock` lends it as `inout sending` with no reference leaving the lock
- `withLockIfAvailable` returns `nil` while another thread holds the lock, for work a contended caller skips
- `Atomic` counters read outside any lock use `.relaxed`, a flag publishing other writes pairs a `.releasing` store with an `.acquiring` load
