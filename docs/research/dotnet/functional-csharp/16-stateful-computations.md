# [STATEFUL_COMPUTATIONS]

## [01]-[STATE_WITHOUT_MUTATION]

A program is stateful when its behavior depends on previous inputs or events. The label depends on the boundary: a server can be stateless by itself while the server and its database form a stateful system.

State does not have to be changed in place. Make it an explicit input and return its successor with the operation's value:

```text
stateless: Input -> Value
stateful:  Input -> State -> (Value, NewState)
```

The caller carries the returned state into the next operation. Earlier immutable state values are not altered. The program remains stateful because each new state affects later behavior.

## [02]-[IMMUTABLE_CACHE]

A currency-rate lookup can cache results to avoid repeated network requests. Its state can be a `HashMap<string, decimal>` from currency-pair names to rates, and the lookup is a `State<HashMap<string, decimal>, decimal>`:

```csharp
internal static class RateCache {
    public static State<HashMap<string, decimal>, decimal> GetRate(Func<string, decimal> fetch, string currencyPair) =>
        from cached in State.gets<HashMap<string, decimal>, Option<decimal>>(cache => cache.Find(currencyPair))
        from rate in cached.Match(
            Some: static hit => State.pure<HashMap<string, decimal>, decimal>(hit),
            None: () => Store(fetch(currencyPair), currencyPair))
        select rate;
    private static State<HashMap<string, decimal>, decimal> Store(decimal rate, string currencyPair) =>
        State.modify<HashMap<string, decimal>>(cache => cache.Add(currencyPair, rate)).Map(_ => rate);
}
```

A hit returns the cached value and the same state. A miss performs the lookup and returns a new map containing the rate. `State.gets` reads a projection of the state, `State.pure` keeps the state unchanged, and `State.modify` replaces the state with a function of it. `GetRate` changes the signature from a stateless lookup into a state transition:

```text
remote lookup: string -> decimal
cached lookup: string -> HashMap<string, decimal> -> (decimal, HashMap<string, decimal>)
```

The application starts with an empty `HashMap<string, decimal>`. Dependent lookups bind in one query, and the host supplies the start state through `Run(state)`, which returns the value with the final state. The implementation uses no global state.

### [02.1]-[SEPARATING_EFFECTS]

Removing mutation does not remove network and console I/O. Passing the network operation as a `Func<string, decimal>` makes that dependency explicit and lets the cache logic be tested with a deterministic function. Console functions can also be supplied explicitly.

The network effect and its failure belong in the function's type:

```text
before: (string -> decimal)     -> string -> State<HashMap<string, decimal>, decimal>
after:  (string -> IO<decimal>) -> string -> StateT<HashMap<string, decimal>, IO, decimal>
```

```csharp
internal static class EffectfulRateCache {
    public static StateT<HashMap<string, decimal>, IO, decimal> GetRate(Func<string, IO<decimal>> fetch, string currencyPair) =>
        from cache in StateT.get<IO, HashMap<string, decimal>>()
        from rate in cache.Find(currencyPair).Match(
            Some: static hit => StateT.pure<HashMap<string, decimal>, IO, decimal>(hit),
            None: () => Fetch(fetch, currencyPair, cache))
        select rate;
    private static StateT<HashMap<string, decimal>, IO, decimal> Fetch(Func<string, IO<decimal>> fetch, string currencyPair, HashMap<string, decimal> cache) =>
        from rate in StateT.liftIO<HashMap<string, decimal>, IO, decimal>(fetch(currencyPair))
        from _ in StateT.put<IO, HashMap<string, decimal>>(cache.Add(currencyPair, rate))
        select rate;
}
```

`StateT.get` reads the whole state, `StateT.liftIO` lifts the fetch into the transformer, and `StateT.put` writes the new map. If a fetch fails on the `IO` error channel, the host retains the original cache. Only a successful lookup yields a cache containing the new rate. `Run(state)` on a `StateT` returns `K<IO, (Value, State)>`. `.As()` converts it, `RunSafe()` executes it, and the host maps the resulting `Fin` cases to its own result type. An `IO` started with `Fork` inside the transformer reads no state and writes none back. `IO` makes failure handling explicit without scattered `try/catch` blocks.

## [03]-[STATE_TRANSITIONS]

A stateful computation, also called a state transition, has the general shape:

```text
S -> (A, S)
```

`S` is the state before and after the operation, and `A` is the produced value. `State<S, A>` wraps a `Func<S, (A Value, S State)>`, and `StateT<S, M, A>` wraps a `Func<S, K<M, (A Value, S State)>>` for a transition with an effect in `M`. A transition may also accept other arguments. The shape can occur inside a stateful or stateless application: it characterizes the function, not the architecture around it.

Use explicit state passing for an isolated transition. Repeatedly extracting and forwarding state adds repetition when several transitions must be sequenced. `Map`, `Bind`, and `State.pure` capture that protocol:
- `Map` transforms the produced value and preserves the returned state.
- `Bind` runs the first computation, uses its value to choose the next computation, then runs that computation with the first computation's returned state.
- `State.pure` lifts a value into a computation that returns the state unchanged.

`Select` and `SelectMany` expose these operations to LINQ query syntax. The syntax hides state extraction and forwarding; it preserves the dependency and its order.

The produced value can be an `Option<A>`, as `OptionInt` shows. The seed advances on every bind, and the consumer matches the option at the boundary. The produced value can also be a function, which lets a stateful computation carry behavior and data.

`State.put` replaces the state. Both `State.put` and `State.modify` produce `Unit`. The module functions take explicit type arguments. `Stateful.state` and `Stateful.local` are the trait forms for a domain wrapper over `State` or `StateT`. `Stateful.local` restores the prior state after the nested computation.

## [04]-[RANDOM_GENERATION]

Composable generators support property-based testing, load testing, and simulations such as Monte Carlo methods.

A conventional pseudo-random generator is deterministic but hides state. Its current seed is an implicit input to `Next`, and the updated seed is an implicit output affecting the next call. Make both explicit with a `State<int, A>`:

```text
int -> (A, int)
```

An explicit seed makes generation repeatable and testable. `Run(seed)` returns the value with the next seed, and the host chooses the seed. A runner that seeds from the clock is impure and not testable.

### [04.1]-[PRIMITIVE_GENERATOR]

The primitive generator scrambles its input seed and returns the result as both the value and the next seed:

```csharp
internal static partial class Generator {
    public static State<int, int> NextInt { get; } = new(static seed => {
        int shifted = seed ^ (seed >> 13);
        int mixed = shifted ^ (shifted << 18);
        int result = mixed & 0x7fffffff;
        return (result, result);
    });
}
```

`State<int, int>` is constructed from a `Func<int, (int Value, int State)>`. The scrambling algorithm does not affect composition. Every call exposes the state required by the next call.

### [04.2]-[DERIVING_WITH_MAP]

`Map` reuses both the integer generator and its next seed while changing only the value:

```csharp
internal static partial class Generator {
    public static State<int, bool> NextBool => NextInt.Map(static i => (i % 2) == 0);
}
```

`NextChar` uses `Map` to reduce an integer modulo `char.MaxValue + 1` and cast it to `char`.

### [04.3]-[SEQUENCING_WITH_BIND]

Generating a pair manually requires feeding the first generation's seed into the second. `Bind` centralizes that threading. With LINQ, composite generators describe the value being built:

```csharp
internal static partial class Generator {
    public static State<int, (int First, int Second)> PairOfInts =>
        from first in NextInt
        from second in NextInt
        select (first, second);
    public static State<int, Option<int>> OptionInt =>
        from some in NextBool
        from value in NextInt
        select some ? Some(value) : Option<int>.None;
}
```

The second generator always consumes the seed returned by the first. Binding order determines the sequence of generator states.

### [04.4]-[RECURSIVE_GENERATION]

`State.pure` supplies a fixed value without consuming state:

```csharp
internal static partial class Generator {
    public static State<int, Seq<int>> Empty => State.pure<int, Seq<int>>(Seq<int>());
}
```

A recursive integer-list generator chooses between an empty result and a generated head followed by another generated list:

```csharp
internal static partial class Generator {
    public static State<int, Seq<int>> IntList =>
        from empty in NextBool
        from list in empty ? Empty : NonEmpty
        select list;
    public static State<int, Seq<int>> NonEmpty =>
        from head in NextInt
        from tail in IntList
        select head.Cons(tail);
}
```

This produces empty lists half the time, one-element lists one quarter of the time, and progressively longer lists with halving probability. Long lists are unlikely.

Generation policy determines the size distribution. For a different distribution, first generate a bounded length and then generate that number of values. To generate a string, generate a character sequence and construct the string.

## [05]-[GENERALIZATION]

`State<int, A>` specializes `State<S, A>` with an integer seed. Other state types use the same `Map`, `Bind`, and `State.pure` operations.

Tree numbering uses an integer counter as state:

```csharp
internal static class Numbering {
    public static State<int, int> GetAndIncrement { get; } = new(static count => (count, count + 1));
    public static Tree<(int Number, T Value)> Numbered<T>(Tree<T> tree) => tree.Number().Run(0).Value;
}
```

For a leaf, the computation creates a new leaf containing the original value and current count, then returns the incremented count as its new state. For a branch, it numbers the left subtree, then numbers the right subtree with the state returned by the left. The result is a new tree whose leaves carry their traversal number:

```csharp
internal abstract record Tree<T> {
    public abstract State<int, Tree<(int Number, T Value)>> Number();
}

internal sealed record Leaf<T>(T Value) : Tree<T> {
    public override State<int, Tree<(int Number, T Value)>> Number() => Numbering.GetAndIncrement.Map(count => Tree.Leaf((count, Value)));
}
internal sealed record Branch<T>(Tree<T> Left, Tree<T> Right) : Tree<T> {
    public override State<int, Tree<(int Number, T Value)>> Number() =>
        from left in Left.Number()
        from right in Right.Number()
        select Tree.Branch(left, right);
}

internal static class Tree {
    public static Tree<T> Leaf<T>(T value) => new Leaf<T>(value);
    public static Tree<T> Branch<T>(Tree<T> left, Tree<T> right) => new Branch<T>(left, right);
}
```

The numbering function returns a computation rather than a numbered tree immediately. Supplying the initial counter runs it: `Numbered` calls `Run(0)`, which returns the numbered tree with the next counter, and `.Value` selects the tree. LINQ sequences recursive transitions in a branch. For simpler cases, explicit state passing is clearer.

Simulations and parsers can also use state transitions. A functional parser can treat its input text as state: it returns a structured parsed value and the unconsumed remainder for the next parser. `LanguageExt.Parsec` follows this model: its `Parser<T>` maps a `PString` to a `ParserResult<T>` that carries the unconsumed input.

## [06]-[REPRESENTATION_CHOICE]

- Keep state visible in inputs and outputs to expose dependencies and sequencing.
- Use immutable state values; the caller advances by selecting the returned value as the next state.
- Keep explicit tuple passing for short state flows.
- Use composition when many dependent transitions would otherwise repeat manual state extraction and forwarding.
- Treat sequencing as semantic: each computation receives the state produced by its predecessor.
