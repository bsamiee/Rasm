# [IMMUTABLE_DATA]

## [01]-[CORE_SHIFT]

Programs must represent real-world change, but change does not require mutation.

- Mutation overwrites an existing value in place
- Immutable update creates a new value representing the next state
- State is a snapshot at a point in time
- Transition is a function from one snapshot to the next

Entities retain identity through many immutable states. Freezing keeps a bank account the same account, but its active and frozen states are distinct values. The model needs snapshots, transitions between them, and an association from the entity identity to its current snapshot.

Avoiding mutation and enforcing immutability are separate concerns. The first is a design discipline: transitions return new values. The second uses constructors, access restrictions, and immutable referenced values to prevent accidental violations of that discipline.

```text
current state --transition--> next state
      |                          |
   unchanged                  new value
```

## [02]-[SHARED_MUTATION]

Shared mutable state creates these problems:
1. Lost updates: concurrent operations can read the same old value and overwrite one another's results
2. Temporary invalid states: a multi-field update exposes intermediate combinations when fields are changed separately
3. Hidden coupling: every reader depends on every code path capable of changing the shared object
4. Loss of purity: changing state outside a function's local scope is an observable side effect

Locks protect one update, but coordination becomes difficult when one business action affects several objects or subsystems. The larger the scope of shared mutation, the harder it is to reason about atomicity and correctness.

The concurrency source does not have to be multiple threads. The same shared-state hazards arise with asynchronous and parallel execution.

Systems combining concurrency with state mutation cannot be proved free of race conditions. Strong correctness comes from removing mutation from shared state, not from coordinating access.

Mutation confined to a function is different. Local accumulators hidden from callers do not make the function impure. `Sum` and `Aggregate` express the intent directly.

## [03]-[VALUES_AND_IDENTITY]

For a value object, value determines identity. Changing a date, number, or geometric shape produces a different value. Framework primitives are immutable, as are `DateTime` and `String`. Their operations (`DateTime.AddDays`) return new values instead of altering the receiver. Custom immutable operations follow the same pattern:

```csharp
internal readonly record struct Point(double X, double Y);

internal sealed record Circle(Point Center, double Radius);

internal static class Shapes {
    public static Circle Scale(Circle circle, double factor) =>
        new(circle.Center, circle.Radius * factor);
}
```

Keep structs immutable. Value types are copied when passed between functions. Mutating one makes changes propagate down, but not back up, the call stack.

Entities are different: their identity persists while their state changes. Model an entity's state as an immutable snapshot and model each allowed change as a function that constructs another snapshot.

```csharp
internal static class Transitions {
    public static AccountState Frozen(AccountState active) => active.With(AccountStatus.Frozen);
}
```

The previous state remains intact. Transitions preserve the input instead of updating in place.

## [04]-[IMMUTABLE_DOMAIN_STATE]

Immutable objects need more than getter-only properties. Immutability must extend through every referenced value.

```csharp
internal readonly record struct CurrencyCode(string Code);

internal enum AccountStatus { Requested = 0, Active = 1, Frozen = 2 }

internal sealed record Transaction(string Reference, decimal Amount);

internal sealed record AccountState(CurrencyCode Currency, AccountStatus Status, decimal AllowedOverdraft, Seq<Transaction> Transactions) {
    public static AccountState Requested(CurrencyCode currency) =>
        new(currency, AccountStatus.Requested, 0m, Seq<Transaction>());
    public static AccountState Opened(CurrencyCode currency, IList<Transaction> transactions) =>
        new(currency, AccountStatus.Active, 0m, toSeq(transactions));

    public AccountState With(Option<AccountStatus> status = default, Option<decimal> allowedOverdraft = default) =>
        this with { Status = status.IfNone(Status), AllowedOverdraft = allowedOverdraft.IfNone(AllowedOverdraft) };
    public AccountState Add(Transaction transaction) =>
        this with { Transactions = transaction.Cons(Transactions) };
}
```

The design relies on these rules:
- Remove property setters and require construction through a constructor or factory
- Enforce required values and business invariants during construction, invalid snapshots cannot exist
- Seal the type to prevent a mutable subclass from weakening the guarantee
- Store collections in immutable collection types (`Seq<A>`)
- Defensively copy mutable input collections at the boundary
- Ensure element types (`Transaction`, `CurrencyCode`) are immutable
- Expose only meaningful transitions, not unrestricted copy methods

Immutable top-level objects containing a mutable list are mutable. Shallow copies are safe only if every shared referenced value is immutable. `Opened` calls `toSeq`, which copies the list argument into a `Seq<Transaction>`. Later changes to the caller's mutable list do not reach the snapshot.

Public setters allow callers to replace properties. Private setters prevent caller replacement, but code inside the class can still reassign properties. Read-only interfaces over a mutable collection do not make the graph immutable.

## [05]-[COPY_AND_UPDATE]

### [05.1]-[CONVENTION]

Public setters or mutable collections remain available, but callers follow the discipline of using them only during initialization and use copy methods afterward. The compiler cannot prevent accidental mutation.

### [05.2]-[EXPLICIT_TYPES]

Getter-only properties, constructors, immutable referenced values, and copy methods make the contract visible and prevent accidental mutation.

One `With` method updates several permitted fields in one allocation. Optional `Option` parameters distinguish “not supplied” from a value, and `IfNone` keeps the current value for each absent one. Limit its parameters to domain-permitted changes: status and overdraft can change, while currency and transaction history cannot.

Records provide copy-and-update with a `with` expression over `init`-only properties, and a `readonly struct` prevents reassignment of its fields.

`Lens<A, B>.New` pairs a getter with a curried setter for one field, and `Set` and `Update` return the rebuilt snapshot. `lens(outer, inner)` composes two lenses to update a nested field.

```csharp
internal sealed record Customer(string Name, AccountState Account);

internal static class Lenses {
    public static readonly Lens<Customer, AccountState> Account =
        Lens<Customer, AccountState>.New(static customer => customer.Account, static account => customer => customer with { Account = account });
    public static readonly Lens<AccountState, decimal> AllowedOverdraft =
        Lens<AccountState, decimal>.New(static account => account.AllowedOverdraft, static overdraft => account => account.With(allowedOverdraft: overdraft));
    public static readonly Lens<Customer, decimal> CustomerOverdraft = lens(Account, AllowedOverdraft);

    public static Customer Raise(Customer customer, decimal amount) =>
        CustomerOverdraft.Update(overdraft => overdraft + amount, customer);
    public static Customer Reset(Customer customer) =>
        CustomerOverdraft.Set(0m, customer);
}
```

### [05.3]-[REFLECTION_COPYING]

Reflection can copy an object and replace one backing field. It removes boilerplate, but it is slower and bypasses control over legal transitions. Prefer explicit copy methods.

### [05.4]-[FSHARP_TYPES]

Data can live in F#, where declarations are immutable by default and support copy-and-update expressions, while C# implements the behavior. The cost is a mixed-language solution and an extra assembly boundary.

No C# technique can prevent all mutation because reflection can alter private or read-only fields. The goal is to prevent accidental mutation and communicate the intended model.

## [06]-[COST_MODEL]

Immutable updates create new top-level objects, increasing allocations and garbage collection. The copy is shallow: unchanged immutable children are shared, while only changed values and the new parent are allocated.

This creates a tradeoff:
- In-place mutation is cheaper for the individual write
- Immutable updates improve safety, isolation, and reasoning
- Mutable designs can require locks and defensive copying
- Prefer safety and optimize only measured hot paths

`Add` uses `Cons` to keep the newest transaction at the front. Choose a collection type that matches frequent domain operations.

## [07]-[PERSISTENT_LISTS]

Functional singly linked lists are recursively defined:

```text
List<T> = Empty | Cons(head: T, tail: List<T>)
```

Persistent means that earlier in-memory versions remain available after an update. It does not refer to disk storage. Persistent structures keep the main operations within the same order of magnitude as the mutable structure.

`Seq<A>` represents these cases through `Head`, `Tail`, and `Match`. Traversals recurse through the tail:

```csharp
internal static class Histories {
    public static Seq<Transaction> Prepend(Transaction transaction, Seq<Transaction> history) => transaction.Cons(history);
    public static Option<Transaction> Newest(Seq<Transaction> history) => history.Head;
    public static Seq<Transaction> Older(Seq<Transaction> history) => history.Tail;
    public static decimal Balance(Seq<Transaction> history) =>
        history.Match(
            Empty: static () => 0m,
            Tail: static (head, tail) => head.Amount + Balance(tail));
    public static Lst<Transaction> Correct(Lst<Transaction> ledger, int index, Transaction corrected) =>
        ledger.SetItem(index, corrected);
}
```

`Seq(a, b, c)` and `toSeq` build a sequence in the caller's order.

Prepends produce this structure:

```text
original:       A -> B -> C
prepend X: X -> A -> B -> C
prepend Y: Y -> A -> B -> C
```

The original and both derived lists coexist because the shared tail cannot change.

- Prepend: `O(1)`, one new node
- Remove the head: `O(1)`, return the tail
- `Map`, `Where`, or full aggregation: `O(n)`
- Insert or remove at index `m`: `O(m)` traversal and `m` rebuilt prefix nodes
- Appending to the end repeatedly is a poor fit, choose another structure for queue-like workloads

Indexed operations use `Lst<A>`, which provides `Insert`, `RemoveAt`, and `SetItem` over a balanced tree.

`Head` on an empty `Seq<A>` is `None`, and `Tail` on an empty `Seq<A>` is empty. When emptiness matters, consume the sequence through `Match`. Recursive implementations can overflow the stack on long lists. Use `Fold` for long histories.

## [08]-[PERSISTENT_TREES]

Binary trees can be defined recursively:

```text
Tree<T> = Leaf(value: T) | Branch(left: Tree<T>, right: Tree<T>)
```

`Map<K, V>` implements this model. `Select` rebuilds the same shape with transformed values. `Fold` threads an accumulator through the tree.

`Add`, `Find`, and `SetItem` associate an account identity with its current snapshot. `Add` throws for a present key and `SetItem` throws for an absent key, both programming errors.

```csharp
internal static class Ledgers {
    public static Map<string, AccountState> Open(Map<string, AccountState> accounts, string id, AccountState state) =>
        accounts.Add(id, state);
    public static Option<AccountState> Current(Map<string, AccountState> accounts, string id) =>
        accounts.Find(id);
    public static Map<string, AccountState> Replace(Map<string, AccountState> accounts, string id, AccountState state) =>
        accounts.SetItem(id, state);
}
```

```text
old root                 new root
 /    \                   /    \
L      R         ->      L    rebuilt R
```

`Add` rebuilds only the nodes from the root to the new key and shares every untouched subtree. This reuse is structural sharing. In a balanced tree containing `n` elements, insertion creates about `log n + 2` objects. The logarithm's base is the tree's arity, a higher-arity tree can remain shallow for a large collection. `Map<K, V>` balances itself on every `Add`, the rebuilt path stays within that bound.

## [09]-[DECISION_RULES]

1. Represent every domain state as a complete, valid snapshot
2. Express change as named state-transition functions returning new snapshots
3. Make immutability deep before sharing references between versions
4. Use immutable collections rather than copying whole mutable collections manually
5. Shape data structures around their frequent operations
6. Treat the list and tree definitions as conceptual models and use the library collection types `Seq`, `Lst`, `Map`, and `HashMap`
7. Choose explicit copy APIs when the domain must restrict which changes are legal
8. Accept local mutation only when it is fully encapsulated and unobservable

Immutable snapshots and persistent structures remove time-dependent behavior from data access. Components can share values without coordinating changes.
