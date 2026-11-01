# [DATA]

## [01]-[STATE_MACHINES]

State machines are an enum of states with their inputs as a nested `Event` enum. One transition returns the next state from a tuple switch over state and event, `nil` where the state rejects the event:

```swift
enum Connection: Equatable, Sendable {
    enum Event: Sendable {
        case dial
        case opened(session: UUID)
        case dropped
        case close
    }

    case idle
    case connecting(attempt: Int)
    case connected(session: UUID)
    case failed(attempts: Int)

    func applying(_ event: Event, maxAttempts: Int = 3) -> Self? {
        switch (self, event) {
            case (.idle, .dial), (.failed, .dial), (.connected, .dropped): .connecting(attempt: 1)
            case (.connecting, .opened(let session)): .connected(session: session)
            case (.connecting(let attempt), .dropped) where attempt < maxAttempts: .connecting(attempt: attempt + 1)
            case (.connecting(let attempt), .dropped): .failed(attempts: attempt)
            case (.connecting, .close), (.connected, .close): .idle
            case (.idle, _), (.connecting, _), (.connected, _), (.failed, _): nil
        }
    }
}
```

- Last arm lists each state with a wildcard event in place of `default`
- Rejections return `nil`, and the caller holds the state and the event that name the rejection
- `events.reductions(Connection.idle) { state, event in state.applying(event) ?? state }.removeDuplicates()` streams accepted states

## [02]-[GRAPH_WALKS]

Graph walks are lazy sequences over one `OrderedSet` that is both visited set and queue:

```swift
func reachable<Node: Hashable, Successors: Sequence<Node>>(from start: Node, successors: @escaping (Node) -> Successors) -> some Sequence<Node> {
    sequence(state: (found: OrderedSet([start]), next: 0)) { state in
        guard state.next < state.found.count else { return nil }
        let node: Node = state.found[state.next]
        state.found.append(contentsOf: successors(node))
        state.next += 1
        return node
    }
}
```

`append(contentsOf:)` skips present members, `found` holds each node once in discovery order, and `next` reads it breadth-first without recursion. `reachable(from: root) { node in graph[node, default: []] }` walks a `[Node: Set<Node>]` adjacency map as it stands.

## [03]-[RECURSIVE_FOLDS]

Each interpretation of a recursive enum (evaluation, rendering, simplification) passes `fold` one replacement per case. Cases differing in one operation are one case holding an operator enum:

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

let evaluated: Double = try expression.fold(
    number: { value throws(OrderError) in value },
    variable: { name throws(OrderError) in
        guard let value: Double = bindings[name] else { throw OrderError(subject: "variable \(name)", reason: .missing) }
        return value
    },
    apply: { op, left, right throws(OrderError) in op(left, right) },
)
let rendered: String = expression.fold(number: \.description, variable: \.self, apply: { op, left, right in "(\(left) \(op.rawValue) \(right))" })
```

`expression` is an `Expression` and `bindings` a `[String: Double]`. `Operator` holds its symbol as raw value and arithmetic in `callAsFunction`. New operators add one case and one arm and change no interpretation. Local functions capture non-escaping replacements a closure cannot, and `walk` recurses without passing them again. `fold` throws what its replacements throw: `evaluated` stops at the first unbound variable and names it, `rendered` infers `Never` and takes no `try`.
