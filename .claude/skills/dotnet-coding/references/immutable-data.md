# [IMMUTABLE_DATA]

Covers the snapshot and transition model behind immutability rules, with persistent structures and their costs.

## [01]-[TRANSITIONS]

Programs represent real-world change without mutation:
- Mutation overwrites a value in place, an immutable update creates the next state's value
- States are snapshots at a point in time
- Transitions are functions from one snapshot to the next

```text
current state --transition--> next state
      |                          |
   unchanged                  new value
```

Entities keep their identity through successive immutable states, a frozen account stays the same account while its active and frozen states are distinct values. The model needs snapshots, transitions between them, and an association from entity identity to current snapshot. Avoiding mutation is a design discipline where transitions return new values, and enforcing immutability uses constructors, access restrictions, and immutable referenced values to prevent accidental violations of it.

## [02]-[SHARED_MUTATION]

Shared mutable state creates problems:
- Lost updates, concurrent operations read the same old value and overwrite one another's results
- Temporary invalid states, a multi-field update exposes intermediate combinations when fields change separately
- Hidden coupling, every reader depends on every code path that can change the shared object
- Loss of purity, changing state outside a function's local scope is an observable side effect

Locks protect one update, atomicity across objects or subsystems needs coordinated ownership. Asynchronous and parallel operations can interleave accesses. Mutation confined to a function keeps the function pure, `Fold` expresses a local accumulator hidden from callers.

## [03]-[VALUES_AND_ENTITIES]

Values decide identity of a value object, and changing a date, a number, or a geometric shape produces a different value. Framework primitives, `LocalDate`, and `string` are immutable, and their operations (`LocalDate.PlusDays`) return new values, as custom immutable operations do:

```csharp
internal sealed record Point(double X, double Y);

internal sealed record Circle(Point Center, double Radius);

internal static class Shapes {
    public static Circle Scaled(Circle circle, double factor) => new(circle.Center, circle.Radius * factor);
}
```

Entity identity persists while state changes, each allowed change is a function constructing another immutable snapshot, and the previous snapshot stays intact.

## [04]-[SNAPSHOTS]

Snapshots construct through factories that set initial values and copy a mutable input collection at the boundary out of reach of the caller's later changes:

```csharp
internal sealed record Entry(string Reference, decimal Amount);

internal sealed record Snapshot(string Code, decimal Limit, Seq<Entry> Entries) {
    public static Snapshot Opened(string code, IList<Entry> entries) => new(code, 0m, toSeq(entries));

    public Snapshot Add(Entry entry) => this with { Entries = entry.Cons(Entries) };
}

internal static class Transitions {
    public static Snapshot Limited(Snapshot snapshot, decimal limit) => snapshot with { Limit = limit };
}
```

`toSeq` in `Opened` copies the list argument into a `Seq<Entry>`. `Add` uses `Cons` to keep the newest entry at the front.

Partial immutability leaves mutation reachable:
- Public setters let callers replace properties, private setters let code inside the class reassign them
- Read-only interfaces over a mutable collection leave the graph mutable
- Immutable top-level objects holding a mutable list are mutable, a shallow copy is safe only when every shared referenced value is immutable
- Getter-only properties, constructors, immutable referenced values, and copy methods make the contract visible and prevent accidental mutation

## [05]-[LIMITS]

Reflection can alter private and read-only fields and no C# technique prevents all mutation, immutability prevents accidental mutation and communicates the intended model.

## [06]-[COST]

Immutable updates allocate a new top-level object and raise garbage collection. Copies are shallow, unchanged immutable children stay shared, and only changed values and the new parent allocate:
- In-place mutation is cheaper for one write
- Immutable updates improve safety, isolation, and reasoning
- Mutable designs can require locks and defensive copying
- Safety comes first, optimization goes to a measured hot path alone

## [07]-[PERSISTENT_SEQUENCES]

Persistence keeps earlier in-memory versions available after an update, independently of representation. `Head`, `Tail`, `Match`, and `Cons` describe sequence operations without implying linked nodes or constant-time prepends. Aggregate a long history with `Fold`, direct recursion grows the CLR stack.

Use `dotnet-coding-languageext` for collection representations and operation costs.

## [08]-[PERSISTENT_TREES]

`Map<K, V>` stores ordered keys in a balanced tree, and updates share unchanged subtrees. `Select` transforms values and `Fold` threads an accumulator through them.

`Add`, `Find`, and `SetItem` associate an entity identity with its current snapshot. `Add` throws for a present key and `SetItem` for an absent one, both programming errors:

```csharp
internal static class Registry {
    public static Map<string, Snapshot> Opened(Map<string, Snapshot> snapshots, string id, Snapshot state) => snapshots.Add(id, state);
    public static Option<Snapshot> Current(Map<string, Snapshot> snapshots, string id) => snapshots.Find(id);
    public static Map<string, Snapshot> Replaced(Map<string, Snapshot> snapshots, string id, Snapshot state) => snapshots.SetItem(id, state);
}
```

`Add` rebuilds only nodes from the root to the new key and shares every untouched subtree as structural sharing:

```text
old root                 new root
 /    \                   /    \
L      R         ->      L    rebuilt R
```

Balanced-tree insertion rebuilds an `O(log n)` path, rotations preserve balance while untouched subtrees stay shared. Immutable snapshots let components share values without coordinating changes.
