# [IMMUTABLE_DATA]

Covers the material behind the immutability rules of `dotnet-coding`: the snapshot and transition model, the hazards of shared mutation, values against entities, an immutable domain state with its permitted transitions, the copy techniques and their limits, the cost model, and the persistent list and tree structures with their operation costs.

## [01]-[TRANSITIONS]

Programs represent real-world change without mutation: mutation overwrites a value in place, an immutable update creates the value of the next state, a state is a snapshot at a point in time, and a transition is a function from one snapshot to the next.

```text
current state --transition--> next state
      |                          |
   unchanged                  new value
```

Entities keep their identity through many immutable states, freezing keeps an account the same account while its active and frozen states are distinct values, the model needs snapshots, transitions between them, and an association from the entity identity to its current snapshot. Avoiding mutation and enforcing immutability are separate concerns: the first is a design discipline where transitions return new values, and the second uses constructors, access restrictions, and immutable referenced values to prevent accidental violations of that discipline.

## [02]-[SHARED_MUTATION]

Shared mutable state creates these problems:
1. Lost updates, where concurrent operations read the same old value and overwrite one another's results
2. Temporary invalid states, where a multi-field update exposes intermediate combinations when fields change separately
3. Hidden coupling, where every reader depends on every code path that can change the shared object
4. Loss of purity, because changing state outside a function's local scope is an observable side effect

Locks protect one update, and coordination becomes difficult when one business action affects many objects or subsystems, the larger the scope of shared mutation, the harder atomicity and correctness are to reason about. The concurrency source need not be threads, because asynchronous and parallel execution raise the same hazards, and a system that combines concurrency with state mutation cannot be proved free of race conditions, correctness comes from removing mutation from shared state and not from coordinating access. Mutation confined to a function is different, a local accumulator hidden from callers does not make the function impure, and `Fold` expresses that intent directly.

## [03]-[VALUES_AND_ENTITIES]

For a value object, the value determines identity: changing a date, a number, or a geometric shape produces a different value, framework primitives, `LocalDate`, and `string` are immutable, and their operations (`LocalDate.PlusDays`) return new values instead of altering the receiver. Custom immutable operations follow the same shape, and a struct stays immutable, because a value type is copied between functions and a mutation of the copy propagates down the call stack and never back up:

```csharp
internal readonly record struct Point(double X, double Y);

internal sealed record Circle(Point Center, double Radius);

internal static class Shapes {
    public static Circle Scaled(Circle circle, double factor) => new(circle.Center, circle.Radius * factor);
}
```

Entities differ: their identity persists while their state changes, the state is an immutable snapshot and each allowed change is a function that constructs another snapshot, and the previous snapshot stays intact.

## [04]-[DOMAIN_STATE]

The snapshot constructs through factories that establish the initial values, exposes only the transitions the domain permits, and copies a mutable input collection at the boundary so later changes to the caller's list never reach it:

```csharp
internal readonly record struct Code(string Value);

internal enum Status { Requested = 0, Active = 1, Frozen = 2 }

internal sealed record Entry(string Reference, decimal Amount);

internal sealed record Snapshot(Code Code, Status Status, decimal Limit, Seq<Entry> Entries) {
    public static Snapshot Requested(Code code) => new(code, Status.Requested, 0m, Seq<Entry>());
    public static Snapshot Opened(Code code, IList<Entry> entries) => new(code, Status.Active, 0m, toSeq(entries));

    public Snapshot With(Option<Status> status = default, Option<decimal> limit = default) =>
        this with { Status = status.IfNone(Status), Limit = limit.IfNone(Limit) };
    public Snapshot Add(Entry entry) => this with { Entries = entry.Cons(Entries) };
}

internal static class Transitions {
    public static Snapshot Frozen(Snapshot active) => active.With(Status.Frozen);
}
```

`Opened` calls `toSeq`, which copies the list argument into a `Seq<Entry>`. `With` updates the permitted fields in one allocation, its `Option` parameters distinguish "not supplied" from a value, and `IfNone` keeps the current value for each absent one, status and limit can change while the code and the entry history cannot. `Add` uses `Cons` to keep the newest entry at the front.

Public setters let callers replace properties, private setters still let code inside the class reassign them, a read-only interface over a mutable collection does not make the graph immutable, and an immutable top-level object that holds a mutable list is mutable, a shallow copy is safe only when every shared referenced value is immutable. The convention that setters serve only initialization and copy methods serve every later change cannot be enforced by the compiler, and getter-only properties, constructors, immutable referenced values, and copy methods make the contract visible and prevent accidental mutation.

## [05]-[COPIES]

Lenses update a nested field without a chain of `with` expressions.
- See `dotnet-coding-languageext` for the `Lens<A, B>` API and its composition through `lens(outer, inner)`

Reflection can copy an object and replace one backing field, and it removes boilerplate at the cost of speed and of the control over legal transitions, explicit copy methods stay preferred. Data can be declared in F#, where declarations are immutable by default and support copy-and-update expressions while C# implements the behavior, at the cost of a mixed-language solution and an extra assembly boundary. No C# technique prevents all mutation, because reflection can alter private and read-only fields, and the goal is to prevent accidental mutation and to communicate the intended model.

## [06]-[COST]

Immutable updates allocate a new top-level object and raise garbage collection, the copy is shallow, unchanged immutable children are shared and only the changed values and the new parent are allocated, and the tradeoff is:
- In-place mutation is cheaper for the individual write
- Immutable updates improve safety, isolation, and reasoning
- Mutable designs can require locks and defensive copying
- Safety comes first, and only a measured hot path is optimized

## [07]-[PERSISTENT_LISTS]

The functional singly linked list is defined recursively, and persistent means that earlier in-memory versions stay available after an update, not that anything reaches a disk:

```text
List<T> = Empty | Cons(head: T, tail: List<T>)
```

`Seq<A>` represents both cases through `Head`, `Tail`, and `Match`, a traversal recurses through the tail, and `Seq(a, b, c)` and `toSeq` build a sequence in the caller's order:

```csharp
internal static class Histories {
    public static Seq<Entry> Prepend(Entry entry, Seq<Entry> history) => entry.Cons(history);
    public static Option<Entry> Newest(Seq<Entry> history) => history.Head;
    public static Seq<Entry> Older(Seq<Entry> history) => history.Tail;
    public static decimal Balance(Seq<Entry> history) =>
        history.Match(
            Empty: static () => 0m,
            Tail: static (head, tail) => head.Amount + Balance(tail));
    public static Lst<Entry> Corrected(Lst<Entry> ledger, int index, Entry corrected) => ledger.SetItem(index, corrected);
}
```

Prepends share the whole existing list, the original and every derived list coexist because the shared tail cannot change:

```text
original:       A -> B -> C
prepend X: X -> A -> B -> C
prepend Y: Y -> A -> B -> C
```

The operation costs stay within the order of magnitude of the mutable structure:
- Prepend is `O(1)` with one new node, and removing the head is `O(1)` by returning the tail
- `Map`, `Filter`, and a full aggregation are `O(n)`
- Inserting or removing at index `m` is `O(m)` traversal with `m` rebuilt prefix nodes, and indexed operations belong on `Lst<A>`, which supplies `Insert`, `RemoveAt`, and `SetItem` over a balanced tree
- Repeated appends at the end fit poorly, a queue-like workload takes another structure

When emptiness matters, the sequence is consumed through `Match`, and a recursive implementation can overflow the stack on a long list, a long history folds with `Fold`.

## [08]-[PERSISTENT_TREES]

Binary trees are defined recursively, `Map<K, V>` implements this model, `Select` rebuilds the same shape with transformed values, and `Fold` threads an accumulator through the tree:

```text
Tree<T> = Leaf(value: T) | Branch(left: Tree<T>, right: Tree<T>)
```

`Add`, `Find`, and `SetItem` associate an entity identity with its current snapshot, and `Add` throws for a present key while `SetItem` throws for an absent one, both programming errors:

```csharp
internal static class Registry {
    public static Map<string, Snapshot> Opened(Map<string, Snapshot> snapshots, string id, Snapshot state) => snapshots.Add(id, state);
    public static Option<Snapshot> Current(Map<string, Snapshot> snapshots, string id) => snapshots.Find(id);
    public static Map<string, Snapshot> Replaced(Map<string, Snapshot> snapshots, string id, Snapshot state) => snapshots.SetItem(id, state);
}
```

`Add` rebuilds only the nodes from the root to the new key and shares every untouched subtree, which is structural sharing:

```text
old root                 new root
 /    \                   /    \
L      R         ->      L    rebuilt R
```

In a balanced tree of `n` elements an insertion creates about `log n + 2` objects, the logarithm's base is the tree's arity, a higher-arity tree stays shallow for a large collection, and `Map<K, V>` balances itself on every `Add`, which keeps the rebuilt path within that bound. Immutable snapshots and persistent structures remove time-dependent behavior from data access, components share values without coordinating changes.
