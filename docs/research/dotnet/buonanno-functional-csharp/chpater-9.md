# Thinking About Data Functionally

## The Core Shift

Programs must represent real-world change, but change does not require mutation.

- Mutation overwrites an existing value in place.
- Immutable change creates a new value representing the next state.
- State is a snapshot at a point in time.
- Transition is a function from one snapshot to the next.

An entity may retain its identity while passing through many immutable states. A bank account is still the same account after it is frozen, but its active and frozen states are distinct values. A complete model therefore needs the snapshots, the transitions between them, and often an association from the enduring identity to its current snapshot.

Avoiding mutation and enforcing immutability are separate concerns. The first is a design discipline: transitions return new values. The second uses constructors, access restrictions, and immutable constituents to prevent accidental violations of that discipline.

```text
current state --transition--> next state
      |                          |
   unchanged                  new value
```

## Why Shared Mutation Is Dangerous

Shared mutable state creates several connected problems:
1. Lost updates: concurrent operations can read the same old value and overwrite one another's results.
2. Temporary invalid states: a multi-field update exposes intermediate combinations when fields are changed separately.
3. Hidden coupling: every reader depends on every code path capable of changing the shared object.
4. Loss of purity: changing state outside a function's local scope is an observable side effect.

A lock can protect a simple update, but coordination becomes difficult when one business action affects several objects or subsystems. The larger the mutation boundary, the harder it is to reason about atomicity and correctness.

The concurrency source does not have to be multiple threads; the same shared-state hazards arise with asynchronous and parallel execution.

A system that combines concurrency with state mutation cannot be proved free of race conditions. Strong correctness guarantees therefore require removing mutation from shared state, not merely trying to coordinate every possible access.

Mutation confined to a function is different. A local accumulator that cannot escape or be observed by callers does not make the function impure. It may still be unnecessarily low-level when an operation such as `Sum` or `Aggregate` expresses the intent directly.

## Values, Identity, and Snapshots

For a value object, value determines identity. Changing a date, number, or geometric shape produces a different value. Framework primitives are immutable, as are familiar types such as `DateTime` and `String`; operations such as `DateTime.AddDays` return new values instead of altering the receiver. Custom immutable operations follow the same pattern:

```csharp
static Circle Scale(Circle circle, double factor) =>
    new Circle(circle.Center, circle.Radius * factor);
```

C# structs should be immutable. Value types are copied when passed between functions; mutating one makes changes propagate down, but not back up, the call stack.

Entities are different: their identity persists while their state changes. Model an entity's state as an immutable snapshot and model each allowed change as a function that constructs another snapshot.

```csharp
var frozen = active.WithStatus(AccountStatus.Frozen);
```

The previous state remains intact. A transition no longer destroys its input as an in-place update would. Immutable snapshots alone do not manage which snapshot is current; that identity-to-current-state association is a separate part of modeling an evolving entity.

## Building an Immutable Domain State

An immutable object needs more than getter-only properties. Immutability must extend through every referenced value.

```csharp
public sealed class AccountState
{
    public CurrencyCode Currency { get; }
    public AccountStatus Status { get; }
    public decimal AllowedOverdraft { get; }
    public ImmutableList<Transaction> Transactions { get; }

    public AccountState(
        CurrencyCode currency,
        AccountStatus status = AccountStatus.Requested,
        decimal allowedOverdraft = 0,
        IEnumerable<Transaction> transactions = null)
    {
        Currency = currency;
        Status = status;
        AllowedOverdraft = allowedOverdraft;
        Transactions = ImmutableList.CreateRange(
            transactions ?? Enumerable.Empty<Transaction>());
    }

    public AccountState With(
        AccountStatus? status = null,
        decimal? allowedOverdraft = null) =>
        new AccountState(
            Currency,
            status ?? Status,
            allowedOverdraft ?? AllowedOverdraft,
            Transactions);

    public AccountState Add(Transaction transaction) =>
        new AccountState(
            Currency,
            Status,
            AllowedOverdraft,
            Transactions.Insert(0, transaction));
}
```

The design relies on several rules:
- Remove property setters and require construction through a constructor or factory.
- Enforce required values and business invariants during construction so invalid snapshots cannot exist.
- Seal the type so a mutable subclass cannot weaken the guarantee.
- Store collections in immutable collection types.
- Defensively copy mutable input collections at the boundary.
- Ensure element types such as `Transaction` and `CurrencyCode` are also immutable.
- Expose only meaningful transitions; do not provide arbitrary setters disguised as copy methods.

An immutable top-level object containing a mutable list is still mutable. A shallow copy is safe only when every shared referenced value is itself immutable. Creating an `ImmutableList` from constructor input prevents later changes to the caller's mutable list from leaking into the snapshot; when the input is already an `ImmutableList`, `CreateRange` can reuse it.

## Copy-Update Strategies in C#

### Immutability by convention

Public setters or mutable collections remain available, but callers follow the discipline of using them only during initialization and use copy methods afterward. This costs little to implement, but the compiler cannot prevent accidental mutation.

### Explicit immutable C# types

Getter-only properties, constructors, immutable constituents, and copy methods make the contract visible and prevent ordinary accidental mutation. The cost is constructor and copy-method boilerplate.

A single `With` method can update several permitted fields in one allocation. Optional nullable parameters distinguish “not supplied” from a value for non-nullable fields. Because `null` is the “not supplied” marker, this method cannot assign `null`; absence should be modeled explicitly instead. Limit its parameters to changes the domain permits: status and overdraft may change, while currency and transaction history should not be replaced arbitrarily.

### Generic reflection-based copying

A generic helper can identify a property and replace its backing field in a copied object. It reduces boilerplate, but reflection is slower and removes fine-grained control over which transitions are legal. Prefer explicit copy methods where performance or domain restrictions matter.

### Immutable data types from F#

Data can be defined separately from behavior. F# data declarations are immutable by default and provide copy-and-update expressions, while C# can continue to implement behavior. The tradeoff is a mixed-language solution and an additional assembly boundary.

No C# technique can make mutation absolutely impossible because reflection can alter private or read-only fields. The practical goal is to prevent accidental mutation and communicate the intended model clearly.

## Cost Model of Immutable Updates

An immutable update creates another top-level object, increasing allocations and eventual garbage collection. It normally performs a shallow copy: unchanged immutable children are shared, while only changed values and the new parent are allocated.

This makes the usual tradeoff:
- In-place mutation is cheaper for the individual write.
- Immutable updates improve safety, isolation, and reasoning.
- Mutable designs may later pay for locks and defensive copying.
- Prefer safety first; optimize only measured hot paths.

Keeping frequently added items at the front of an immutable list can make updates efficient. Collection shape should match the dominant domain operations.

## Persistent Linked Lists

A functional singly linked list is recursively defined as:

```text
List<T> = Empty | Cons(head: T, tail: List<T>)
```

Here, persistent means that earlier in-memory versions remain available after an update; it does not mean storage on disk. A persistent structure should also keep its principal operations within the same order of magnitude as the corresponding mutable structure.

All operations distinguish the two cases. Traversals recurse through the tail:

```csharp
static int Length<T>(List<T> list) =>
    list.Match(
        Empty: () => 0,
        Cons: (_, tail) => 1 + Length(tail));
```

A convenient initializer can build a list by reversing its input and aggregating from `Empty`, prepending each item; the reversal preserves the caller's order.

Adding to the front allocates one node whose tail points to the original list:

```text
original:       A -> B -> C
prepend X: X -> A -> B -> C
prepend Y: Y -> A -> B -> C
```

The original and both derived lists coexist safely because the shared tail cannot change.

- Prepend: `O(1)`, one new node.
- Remove the head: `O(1)`, return the tail.
- `Map`, `Where`, or full aggregation: `O(n)`.
- Insert or remove at index `m`: `O(m)` traversal and `m` rebuilt prefix nodes.
- Appending to the end repeatedly is a poor fit; choose another structure for queue-like workloads.

Calling `Tail` on an empty list is a programming error. When emptiness is expected, consume the list through pattern matching instead. Naive recursive implementations are also not stack-safe for sufficiently long lists; production code should use a proven immutable collection implementation.

## Persistent Trees and Structural Sharing

A binary tree can also be defined recursively:

```text
Tree<T> = Leaf(value: T) | Branch(left: Tree<T>, right: Tree<T>)
```

`Map` reconstructs the same shape with transformed leaf values. `Aggregate` threads an accumulator through both subtrees. An insertion reconstructs only the nodes on the path to the insertion point and reuses every untouched subtree.

```csharp
static Tree<T> Insert<T>(Tree<T> tree, T value) =>
    tree.Match(
        Leaf: _ => Branch(tree, Leaf(value)),
        Branch: (left, right) =>
            Branch(left, Insert(right, value)));
```

```text
old root                 new root
 /    \                   /    \
L      R         ->      L    rebuilt R
```

The insertion descends through the right subtree. At a leaf it creates a branch containing the existing leaf and the new leaf; on the way back it rebuilds only that path while sharing each untouched left subtree. This reuse is structural sharing. In a balanced tree containing `n` elements, insertion creates about `log n + 2` objects. The logarithm's base is the tree's arity, so a higher-arity production tree can remain shallow even for a large collection. This sample always inserts on the right and therefore becomes unbalanced; efficient production structures need balancing.

## Practical Decision Rules

1. Represent every domain state as a complete, valid snapshot.
2. Express change as named state-transition functions returning new snapshots.
3. Make immutability deep before sharing references between versions.
4. Use immutable collections rather than copying whole mutable collections manually.
5. Shape data structures around their frequent operations.
6. Treat hand-built list and tree implementations as conceptual models, not production containers.
7. Choose explicit copy APIs when the domain must restrict which changes are legal.
8. Accept local mutation only when it is fully encapsulated and unobservable.

The essential payoff is not syntax. Immutable snapshots and persistent structures remove time-dependent behavior from ordinary data access, allowing components to share values without coordinating who may change them.
