# [STATE]

Covers computations that depend on earlier inputs without mutating a value in place: the transition shape, `State` and `StateT` for sequences of dependent transitions, explicit-seed generators, and the loop forms that advance a state until a runtime condition holds.

## [01]-[TRANSITIONS]

A program is stateful when behavior depends on previous inputs, and the label depends on the boundary: a server is stateless by itself while the server with its database forms a stateful system. State becomes an explicit input, and the operation returns its successor beside the value:

```text
stateless: Input -> Value
stateful:  Input -> State -> (Value, NewState)
```

The caller carries the returned state into the next operation, earlier state values stay unchanged, and the program remains stateful because each new state affects later behavior. The shape `S -> (A, S)` characterizes the function, not the architecture around it, and `State<S, A>` wraps a `Func<S, (A Value, S State)>` while `StateT<S, M, A>` wraps a `Func<S, K<M, (A Value, S State)>>` for a transition with an effect in `M`. Explicit state passing suits an isolated transition, and once several transitions are sequenced, extracting and forwarding the state by hand repeats itself, so the operations capture that protocol:
- `Map` transforms the produced value and preserves the returned state
- `Bind` runs the first computation, uses its value to choose the next computation, then runs that computation with the returned state
- `State.pure` lifts a value into a computation that returns the state unchanged, `State.gets` reads a projection, `State.put` replaces the state, and `State.modify` replaces it with a function of it, where `put` and `modify` produce `Unit`
- `Select` and `SelectMany` expose these to LINQ query syntax, which hides the extraction and forwarding and preserves the dependency and its order
- `Stateful.state` and `Stateful.local` are the trait forms for a domain wrapper over `State` or `StateT`, and `Stateful.local` restores the prior state after the nested computation

The produced value can be an `Option<A>` that the consumer matches at the boundary, or a function, which lets a stateful computation carry behavior beside data. Keep explicit tuple passing for a short state flow, use composition when many dependent transitions otherwise repeat the forwarding, and treat sequencing as semantic: each computation receives the state its predecessor produced.

## [02]-[CACHE]

A lookup that caches results is a state transition over the cache. A `HashMap<string, decimal>` from codes to quotes is the state, and the lookup is a `State<HashMap<string, decimal>, decimal>`:

```csharp
internal static class QuoteCache {
    public static State<HashMap<string, decimal>, decimal> Get(Func<string, decimal> fetch, string code) =>
        from cached in State.gets<HashMap<string, decimal>, Option<decimal>>(cache => cache.Find(code))
        from quote in cached.Match(
            Some: static hit => State.pure<HashMap<string, decimal>, decimal>(hit),
            None: () => Store(fetch(code), code))
        select quote;
    private static State<HashMap<string, decimal>, decimal> Store(decimal quote, string code) =>
        State.modify<HashMap<string, decimal>>(cache => cache.Add(code, quote)).Map(_ => quote);
}
```

A hit returns the cached value with the same state, and a miss fetches, returns a new map with the quote, and uses no global state. The signature changes from a stateless lookup into a state transition:

```text
remote lookup: string -> decimal
cached lookup: string -> HashMap<string, decimal> -> (decimal, HashMap<string, decimal>)
```

The application starts with an empty map, dependent lookups bind in one query, and the host supplies the start state through `Run(state)`, which returns the value with the final state. Removing mutation does not remove the network effect, and passing the fetch as a `Func<string, decimal>` makes it explicit and testable with a deterministic function. The effect and its failure belong in the type, so the fetch becomes `string -> IO<decimal>` and the cache becomes `StateT<HashMap<string, decimal>, IO, decimal>`:

```csharp
internal static class EffectfulQuoteCache {
    public static StateT<HashMap<string, decimal>, IO, decimal> Get(Func<string, IO<decimal>> fetch, string code) =>
        from cache in StateT.get<IO, HashMap<string, decimal>>()
        from quote in cache.Find(code).Match(
            Some: static hit => StateT.pure<HashMap<string, decimal>, IO, decimal>(hit),
            None: () => Fetch(fetch, code, cache))
        select quote;
    private static StateT<HashMap<string, decimal>, IO, decimal> Fetch(Func<string, IO<decimal>> fetch, string code, HashMap<string, decimal> cache) =>
        from quote in StateT.liftIO<HashMap<string, decimal>, IO, decimal>(fetch(code))
        from _ in StateT.put<IO, HashMap<string, decimal>>(cache.Add(code, quote))
        select quote;
}
```

`StateT.get` reads the whole state, `StateT.liftIO` lifts the fetch into the transformer, and `StateT.put` writes the new map. A fetch that fails on the `IO` error channel leaves the host with the original cache, and only a successful lookup yields a cache containing the new quote. `Run(state)` on a `StateT` returns `K<IO, (Value, State)>`, `.As()` converts it, `RunSafe()` executes it, and the host maps the `Fin` cases to its own result type. An `IO` forked inside the transformer reads no state and writes none back.

## [03]-[GENERATORS]

Composable generators serve property-based testing, load testing, and simulation. A conventional pseudo-random generator is deterministic but hides its state: the seed is an implicit input to the next call and the updated seed an implicit output. A `State<int, A>` makes both explicit as `int -> (A, int)`, generation becomes repeatable and testable, the host chooses the seed through `Run(seed)`, and seeding from the clock is impure and not testable. The primitive generator scrambles its input seed and returns the result as both the value and the next seed, and every derived generator reuses it:

```csharp
internal static class Generator {
    public static State<int, int> NextInt { get; } = new(static seed => {
        int shifted = seed ^ (seed >> 13);
        int mixed = shifted ^ (shifted << 18);
        int result = mixed & 0x7fffffff;
        return (result, result);
    });
    public static State<int, bool> NextBool => NextInt.Map(static i => (i % 2) == 0);
    public static State<int, (int First, int Second)> Pair =>
        from first in NextInt
        from second in NextInt
        select (first, second);
    public static State<int, Option<int>> OptionalInt =>
        from present in NextBool
        from value in NextInt
        select present ? Some(value) : Option<int>.None;
}
```

`Map` changes only the value and keeps the next seed, so a `char` generator reduces the integer modulo `char.MaxValue + 1` and casts. `Bind` threads the seed: the second generator always consumes the seed the first returned, and binding order determines the sequence of generator states. A recursive generator chooses between an empty result and a generated head followed by another list, where `State.pure` supplies the empty result without consuming state:

```csharp
internal static class ListGenerator {
    public static State<int, Seq<int>> Empty => State.pure<int, Seq<int>>(Seq<int>());
    public static State<int, Seq<int>> Ints =>
        from empty in Generator.NextBool
        from list in empty ? Empty : NonEmpty
        select list;
    public static State<int, Seq<int>> NonEmpty =>
        from head in Generator.NextInt
        from tail in Ints
        select head.Cons(tail);
}
```

This policy yields an empty list half the time, one element a quarter of the time, and longer lists with halving probability, so long lists are unlikely. For another distribution, generate a bounded length first and then that many values, and for a string, generate a character sequence and construct the string.

## [04]-[GENERALIZATION]

`State<int, A>` specializes `State<S, A>` with an integer seed, and other state types use the same `Map`, `Bind`, and `State.pure`. Numbering the leaves of a tree in traversal order uses an integer counter as the state: a leaf pairs its value with the current count and returns the incremented count, and a branch numbers its left subtree, then its right subtree with the state the left returned:

```csharp
internal static class Numbering {
    public static State<int, int> GetAndIncrement { get; } = new(static count => (count, count + 1));
    public static Tree<(int Number, T Value)> Numbered<T>(Tree<T> tree) => tree.Number().Run(0).Value;
}

internal abstract record Tree<T> {
    public abstract State<int, Tree<(int Number, T Value)>> Number();
}

internal sealed record Leaf<T>(T Value) : Tree<T> {
    public override State<int, Tree<(int Number, T Value)>> Number() => Numbering.GetAndIncrement.Map(count => new Leaf<(int, T)>((count, Value)));
}
internal sealed record Branch<T>(Tree<T> Left, Tree<T> Right) : Tree<T> {
    public override State<int, Tree<(int Number, T Value)>> Number() =>
        from left in Left.Number()
        from right in Right.Number()
        select (Tree<(int Number, T Value)>)new Branch<(int Number, T Value)>(left, right);
}
```

The numbering function returns a computation, supplying the initial counter runs it, `Run(0)` returns the numbered tree with the next counter, and `.Value` selects the tree. LINQ sequences the recursive transitions in a branch, and for a simpler case explicit state passing is clearer. Simulations and parsers use the same shape: a functional parser treats the input text as state and returns the parsed value with the unconsumed remainder, which is the model of `LanguageExt.Parsec`, where `Parser<T>` maps a `PString` to a `ParserResult<T>` carrying the unconsumed input.

## [05]-[LOOPS]

An indefinite loop advances a state until a runtime condition holds, and its length is not known in advance, so `Map` and `Fold` over a fixed collection do not supply the stopping rule. The model separates 4 concerns, each an explicit value or function:
- State transition: `Func<S, S>` produces the next state from the current one
- Termination: `Func<S, bool>` decides whether a state is final
- Execution: apply the transition until termination
- Consumption: retain only the final state, or every intermediate state

The library keeps the mutable loop variable inside the execution mechanism and leaves the transition and stopping rule explicit. A tail-recursive function returns the final value or makes its recursive call last, and each call can add a stack frame because C# provides no tail-call optimization, so a condition that takes many iterations can overflow the stack, and a small but unbounded iteration count does not remove the risk. `Trampoline.More` returns the recursive call as a deferred value and `Run()` evaluates the calls in a loop, and a reusable trampolined loop checks the stopping predicate before each transition:

```csharp
internal static class Trampolined {
    public static Trampoline<S> RunUntil<S>(S state, Func<S, bool> stop, Func<S, S> next) =>
        stop(state)
            ? Trampoline.Pure(state)
            : Trampoline.More(() => RunUntil(next(state), stop, next));
    public static Trampoline<Option<int>> FirstZero(Seq<int> deltas, int current, int nextIndex) =>
        current == 0
            ? Trampoline.Pure(Some(nextIndex - 1))
            : deltas.At(nextIndex).Match(
                Some: delta => Trampoline.More(() => FirstZero(deltas, current + delta, nextIndex + 1)),
                None: static () => Trampoline.Pure(Option<int>.None));
}
```

`FirstZero` handles exhaustion explicitly: `At` returns `None` past the end and the result is `None`, and an unreachable base case loops indefinitely. `Bind` chains a second `Trampoline` onto the final state with the stack still constant, `next` must produce a state satisfying `stop`, and a `next` that performs I/O or mutation keeps the expression impure. When the transition is an effect, `Monad.recur` returns only the terminal state, and the state must contain everything both delegates need, so a termination that depends on the last action or the latest random outcome belongs in the returned state, not in a mutable flag:

```csharp
internal sealed record Session(int Remaining, bool HasExited);

internal static class Sessions {
    public static IO<Session> Play(Session initial, IO<int> readMove) =>
        Monad.recur<IO, Session, Session>(initial, session =>
            session.HasExited
                ? IO.pure(Next.Done<Session, Session>(session))
                : readMove.Map(move => Next.Loop<Session, Session>(Advance(session, move)))).As();
    private static Session Advance(Session session, int move) {
        int remaining = session.Remaining - move;
        return new Session(remaining, remaining <= 0);
    }
}
```

The stopping predicate reads the state and the transition is an `IO<int>` the caller supplies. `Monad.recur` checks the initial state before advancing and performs zero or more transitions, and the host runs the result with `RunSafe` and receives the final state as `Fin<Session>`.
- See `dotnet-languageext` for `tail` recursion in `IO`, its exit restrictions, and `RepeatUntil` and `RepeatWhile`

When the intermediate states are meaningful, `LanguageExt.List.unfold` produces them lazily from an initial state and a step that returns `Some((emitted, next))` or `None` at the terminal state, and `toSeq` wraps the result as a `Seq` that reads each state on demand and keeps it:

```csharp
internal static class Traces {
    public static Seq<Session> Of(Session initial, Func<Session, Session> advance) =>
        toSeq(LanguageExt.List.unfold(initial, session => Step(session, advance)));
    private static Option<(Session, Session)> Step(Session session, Func<Session, Session> advance) =>
        session.HasExited ? None : Emit(advance(session));
    private static Option<(Session, Session)> Emit(Session next) => Some((next, next));
}
```

`Step` returns `Some` for the transition that first produces the terminal state and `None` on the following call, the sequence yields each state after a transition and not the initial state, and an already-terminal initial state yields an empty `Seq`, so decide whether enumeration is empty, emits the initial state, or advances once when the initial state can be terminal. Constructing the sequence does not run the loop, reading it does:
- `Last` reads until the sequence ends and returns the terminal state as an `Option`, `None` for an empty sequence
- `Map` defines a lazy transformation of every yielded state, and `Fold` retains accumulated output beside the latest state
- `Head`, `Take`, and similar operators stop enumeration before the loop's own condition, so they serve only intentional early termination (limiting a participant to a fixed number of actions)
- `foreach` is a consumer, and mutating an outer variable inside it reintroduces imperative state at the call site

One `Seq` keeps every state it read, and a second pass does not rerun `advance`, while each `unfold` call constructs a new producer that reruns the whole process, so when `advance` reads input or randomness, build the `Seq` once and read every required result from it.
