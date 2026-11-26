# [DATA]

## [01]-[STATE_MACHINES]

State reduced from events of independent sources is one fold: each source maps into a case of one event enum, `merge` joins the sources, and `reductions(into:)` applies each event to a state record holding the parts events replace:

```swift
enum Change: Sendable {
    case listed(OrderedDictionary<Instance, InstanceState>)
    case watched(instance: Instance, state: InstanceState)
}

func states(of changes: some AsyncSequence<Change, Never> & Sendable) -> some AsyncSequence<OrderedDictionary<Instance, InstanceState>, Never> & Sendable {
    changes.reductions(into: (listed: OrderedDictionary<Instance, InstanceState>(), watched: OrderedDictionary<Instance, InstanceState>())) { held, change in
        held.watched.removeAll { _, state in state.is(\.exited) }
        switch change {
            case .listed(let listing): held.listed = listing
            case .watched(let instance, let state): held.watched[instance] = state
        }
    }
    .map { held in held.listed.merging(held.watched) { _, watched in watched } }
    .removeDuplicates()
}
```

`Instance` names one running process, and `InstanceState` is a `@CasePathable` enum of `running`, `failed`, and `exited`. A periodic listing of every process and the reports of watched processes merge into `changes`, and a watched report wins over the listing:
- Arms write the part their event replaces, and a rule every event applies (dropping exited watches) runs before the `switch`
- Held state is cases and keyed tables, and a record of flags each arm tests and resets becomes one case per state
- `reductions` emits after each event and never its initial value, `chain([initial].async, <fold>)` serves a consumer reading state before the first event
- State lives in the iterator, so the fold takes no lock and each consumer folds its own copy from the initial value
- Consumers read a projection of the state, and `removeDuplicates()` over its `Equatable` value drops events that changed nothing

## [02]-[GRAPH_WALKS]

Walks over links another process or file reports (Accessibility children and parents, scripting definition `inherits`) visit each node once, as such links repeat and cycle. Depth-first preorder yields each node with the parent that reached it, a container ahead of its members in document order:

```swift
func preorder<Node: Hashable>(from roots: [Node], children: @escaping (Node) -> [Node]) -> some Sequence<(node: Node, parent: Node?)> {
    sequence(state: (seen: Set<Node>(), pending: roots.reversed().map { root in (node: root, parent: Node?.none) })) { state in
        while let next = state.pending.popLast() {
            guard state.seen.insert(next.node).inserted else { continue }
            state.pending += children(next.node).reversed().map { child in (node: child, parent: next.node) }
            return next
        }
        return nil
    }
}

let walk: [(node: Int, parent: Int?)] = Array(preorder(from: roots) { id in held[id]?.children ?? [] })
let parents: [Int: Int] = Dictionary(uniqueKeysWithValues: walk.compactMap { step in step.parent.map { parent in (step.node, parent) } })
```

`held` is an `[Int: Element]` of elements by interned id, and `roots` the ids of window and menu roots. `insert(_:)` returns `inserted`, one call testing and recording a visit, and each edge enters `pending` once per visit of its source, so a cycle ends the walk with no depth cap:
- `OrderedSet(walk.map(\.node))` holds visit order with constant-time membership, and `OrderedSet.append(_:)` returns `(inserted, index)` where a walk records order as it visits
- Breadth-first walks keep one `OrderedSet` as visited record and queue: `sequence(state: (found: OrderedSet([start]), next: 0))` reads `found[next]` and appends its successors through `append(contentsOf:)`, which skips members
- Parent chains read upward lazily as `sequence(first: id) { child in parents[child] }.dropFirst().prefix(held.count)`, the node count bounding links that can cycle
- `.first { scope in kinds[scope] != nil }` over a chain stops at the nearest owner and reads no link above it

## [03]-[RECURSIVE_FOLDS]

Recursive values are enums with cases holding `Self` or a collection of it. `indirect case` marks each case holding `Self` by value, as `indirect enum` boxes every payload case on the heap, and a `[Self]` or `[String: Self]` payload stores its elements out of line with no marker. Interpretations recurse in one member that passes each child the context its parent decides, independent children accumulate every failure through `traverse` and `combine`, and a step reading their results binds after them:

```swift
enum AppleEventValue: Sendable {
    case number(Double)
    case text(String)
    case list([Self])
    indirect case specifier(want: DescType, key: Self, from: Self)

    func descriptor(as types: Set<String>) -> Result<NSAppleEventDescriptor, AggregateError<AppleEventFailure>> {
        switch self {
            case .number(let value): .success((types.contains("integer") ? Int32(exactly: value) : nil).map(NSAppleEventDescriptor.init(int32:)) ?? NSAppleEventDescriptor(double: value))
            case .text(let text): .success(NSAppleEventDescriptor(string: text))
            case .list(let items):
                traverse(items) { item in item.descriptor(as: types) }.map { items in
                    items.reduce(into: NSAppleEventDescriptor.list()) { list, item in list.insert(item, at: list.numberOfItems + 1) }
                }
            case .specifier(let want, let key, let from):
                combine(key.descriptor(as: ["integer"]), from.descriptor(as: [])).flatMap { key, container in
                    [(keyAEDesiredClass, NSAppleEventDescriptor(typeCode: want)), (keyAEKeyData, key), (keyAEContainer, container)]
                        .reduce(into: NSAppleEventDescriptor.record()) { record, field in record.setDescriptor(field.1, forKeyword: AEKeyword(field.0)) }
                        .coerce(toDescriptorType: DescType(typeObjectSpecifier))
                        .map(Result.success) ?? .failure(AggregateError(first: AppleEventFailure(subject: "\(self)", reason: .unencodable)))
                }
        }
    }
}
```

`AppleEventFailure` is the module error with an `unencodable` reason. A list reports every item that fails, a specifier its key and container failures together, and the record coerces once both arrive:
- `types` comes from the scripting dictionary entry the parent resolved, and a key with no declared type reads as an integer
- Interpretations reading only their children's results take one `fold` member with one replacement per case, generic over `E: Error`
- `fold` recurses through a local function capturing the non-escaping replacements, and replacements that never throw bind `E` to `Never` with no `try`
- Decoders of a recursive wire type refuse nesting past a stated depth of `decoder.codingPath`, as each level takes a stack frame
