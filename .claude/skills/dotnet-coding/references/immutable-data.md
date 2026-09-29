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

Locks protect one update, and atomicity grows harder to reason about as one business action reaches more objects or subsystems. Asynchronous and parallel execution raise the hazards of threads, and a system combining concurrency with shared mutation cannot be shown free of race conditions. Mutation confined to a function keeps the function pure, and `Fold` expresses a local accumulator hidden from callers.

## [03]-[VALUES_AND_ENTITIES]

Values decide identity of a value object, and changing a date, a number, or a geometric shape produces a different value. Framework primitives, `LocalDate`, and `string` are immutable, and their operations (`LocalDate.PlusDays`) return new values, as custom immutable operations do. Value types copy between functions, a mutation of the copy propagates down the call stack and never back up:

```csharp
internal readonly record struct Point(double X, double Y);

internal sealed record Circle(Point Center, double Radius);

internal static class Shapes {
    public static Circle Scaled(Circle circle, double factor) => new(circle.Center, circle.Radius * factor);
}
```

Entity identity persists while state changes, each allowed change is a function constructing another immutable snapshot, and the previous snapshot stays intact.

## [04]-[DOMAIN_STATE]

Snapshots construct through factories that set initial values, expose only transitions the domain permits, and copy a mutable input collection at the boundary out of reach of the caller's later changes:

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

`toSeq` in `Opened` copies the list argument into a `Seq<Entry>`. `With` updates permitted fields in one allocation, its `Option` parameters distinguish "not supplied" from a value, and `IfNone` keeps the current value for each absent one. Status and limit can change, code and entry history cannot. `Add` uses `Cons` to keep the newest entry at the front.

Partial immutability leaves mutation reachable:
- Public setters let callers replace properties, private setters let code inside the class reassign them
- Read-only interfaces over a mutable collection leave the graph mutable
- Immutable top-level objects holding a mutable list are mutable, a shallow copy is safe only when every shared referenced value is immutable
- Compilers cannot enforce setters for initialization alone and copy methods for every later change
- Getter-only properties, constructors, immutable referenced values, and copy methods make the contract visible and prevent accidental mutation

## [05]-[COPIES]

Copy forms beyond a `with` expression:
- Lenses update a nested field without a chain of `with` expressions
- Reflection can copy an object and replace one backing field, less boilerplate at the cost of speed and control over legal transitions
- F# data with C# behavior gets immutable defaults and copy-and-update expressions at the cost of a mixed-language solution and an extra assembly

Explicit copy methods stay preferred. Reflection can alter private and read-only fields and no C# technique prevents all mutation, immutability prevents accidental mutation and communicates the intended model.

## [06]-[COST]

Immutable updates allocate a new top-level object and raise garbage collection. Copies are shallow, unchanged immutable children stay shared, and only changed values and the new parent allocate:
- In-place mutation is cheaper for one write
- Immutable updates improve safety, isolation, and reasoning
- Mutable designs can require locks and defensive copying
- Safety comes first, optimization goes to a measured hot path alone

## [07]-[PERSISTENT_LISTS]

Functional singly linked lists are recursive, and persistent means earlier in-memory versions stay available after an update, with nothing written to disk:

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

Prepends share the whole existing list, and the immutable tail lets the original and every derived list coexist:

```text
original:       A -> B -> C
prepend X: X -> A -> B -> C
prepend Y: Y -> A -> B -> C
```

Operation costs stay within the order of magnitude of the mutable structure:
- Prepend is `O(1)` with one new node, and removing the head is `O(1)` by returning the tail
- `Map`, `Filter`, and a full aggregation are `O(n)`
- Inserting or removing at index `m` is `O(m)` traversal with `m` rebuilt prefix nodes
- Indexed operations belong on `Lst<A>` with `Insert`, `RemoveAt`, and `SetItem` over a balanced tree
- Repeated appends at the end fit poorly, a queue-like workload takes another structure

When emptiness matters, consume the sequence through `Match`. Recursion can overflow the stack on a long list, and a long history folds with `Fold`.

## [08]-[PERSISTENT_TREES]

Binary trees are defined recursively, `Map<K, V>` implements the model, `Select` rebuilds the same shape with transformed values, and `Fold` threads an accumulator through the tree:

```text
Tree<T> = Leaf(value: T) | Branch(left: Tree<T>, right: Tree<T>)
```

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

Insertions into a balanced tree of `n` elements create about `log n + 2` objects with the tree's arity as logarithm base, and a higher-arity tree stays shallow for a large collection. `Map<K, V>` balances itself on every `Add` to keep the rebuilt path within the bound. Immutable snapshots and persistent structures remove time-dependent behavior from data access, and components share values without coordinating changes.
