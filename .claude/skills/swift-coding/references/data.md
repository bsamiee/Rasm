# [DATA]

Covers modeling values, from state machines to graph walks and recursive folds.

## [01]-[STATE_MACHINES]

Inputs a state machine receives are an `Event` enum, and one `consuming` transition returns its value with the next `phase` from a tuple switch over phase and event, its last arm throwing the rejection:

```swift
struct Connection: Equatable, Sendable {
    enum Phase: Equatable, Sendable {
        case idle
        case connecting(attempt: Int)
        case connected(session: UUID)
        case failed(attempts: Int)
    }

    enum Event: Sendable {
        case dial
        case opened(session: UUID)
        case dropped
        case close
    }

    let endpoint: URL
    private(set) var phase: Phase = .idle

    consuming func applying(_ event: Event, maxAttempts: Int = 3) throws(OrderError) -> Self {
        phase =
            switch (phase, event) {
                case (.idle, .dial), (.failed, .dial), (.connected, .dropped): .connecting(attempt: 1)
                case (.connecting, .opened(let session)): .connected(session: session)
                case (.connecting(let attempt), .dropped) where attempt < maxAttempts: .connecting(attempt: attempt + 1)
                case (.connecting(let attempt), .dropped): .failed(attempts: attempt)
                case (.connecting, .close), (.connected, .close): .idle
                case (.idle, _), (.connecting, _), (.connected, _), (.failed, _): throw OrderError(subject: "\(event) in \(phase)", reason: .invalid)
            }
        return self
    }
}
```

- Last arm lists each phase with a wildcard event in place of `default`, so a new phase fails to compile until its transitions are written
- Rejections name the event and the phase it arrived in, and a caller that kept its connection holds it unchanged
- Transitions assign `phase` on the value they consume, so a new shared field changes no transition
- `events.reductions(into: connection) { state, event in try? state = state.applying(event) }.removeDuplicates()` folds events into each accepted state

## [02]-[GRAPH_WALKS]

Graph walks are lazy sequences over one `OrderedSet` serving as both visited set and queue:

```swift
func reachable<Node: Hashable>(from start: Node, successors: @escaping (Node) -> [Node]) -> some Sequence<Node> {
    sequence(state: (found: OrderedSet([start]), next: 0)) { state in
        guard state.next < state.found.count else { return nil }
        let node: Node = state.found[state.next]
        state.found.append(contentsOf: successors(node))
        state.next += 1
        return node
    }
}
```

`append(contentsOf:)` skips members, so `found` holds each node once in discovery order, and `next` reads it breadth-first without recursion. Walks run as far as their consumer reads: `first(where:)` ends a search at its first match, `Array(_:)` collects every reachable node.

## [03]-[RECURSIVE_FOLDS]

Each interpretation of a recursive enum (evaluation, rendering, simplification) passes `fold` one replacement per case, and cases differing in one operation are one case holding an operator enum:

```swift
indirect enum Expression: Sendable {
    enum Operator: String, Sendable {
        case sum = "+"
        case product = "*"

        func callAsFunction(_ left: Double, _ right: Double) -> Double {
            switch self {
                case .sum: left + right
                case .product: left * right
            }
        }
    }

    case number(Double)
    case variable(String)
    case apply(Operator, Self, Self)

    func fold<Value, E: Error>(
        number: (Double) throws(E) -> Value,
        variable: (String) throws(E) -> Value,
        apply: (Operator, Value, Value) throws(E) -> Value,
    ) throws(E) -> Value {
        func walk(_ node: Self) throws(E) -> Value {
            switch node {
                case .number(let value): try number(value)
                case .variable(let name): try variable(name)
                case .apply(let op, let left, let right): try apply(op, walk(left), walk(right))
            }
        }
        return try walk(self)
    }
}

func evaluate(_ expression: Expression, in bindings: [String: Double]) throws(OrderError) -> Double {
    try expression.fold(
        number: { $0 },
        variable: { name throws(OrderError) in
            guard let value: Double = bindings[name] else { throw OrderError(subject: "variable \(name)", reason: .missing) }
            return value
        },
        apply: { op, left, right in op(left, right) },
    )
}

func rendered(_ expression: Expression) -> String {
    expression.fold(number: \.description, variable: { $0 }, apply: { op, left, right in "(\(left) \(op.rawValue) \(right))" })
}
```

`Operator` holds its symbol as its raw value and its arithmetic in `callAsFunction`, so a new operator is one case and one arm, and `evaluate` and `rendered` stay unchanged. A local function captures the non-escaping replacements a closure cannot, so `walk` recurses without passing them again. `fold` throws what its replacements throw: `evaluate` stops at the first unbound variable and names it, and `rendered` infers `Never`.
