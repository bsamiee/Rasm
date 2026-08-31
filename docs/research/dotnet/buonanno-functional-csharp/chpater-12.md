# Stateful Programs and Stateful Computations

## Statefulness without mutation

A program is stateful when its behavior depends on previous inputs or events. The label depends on the boundary: a server can be stateless by itself while the server and its database form a stateful system.

State does not have to be changed in place. Make it an explicit input and return its successor with the operation's value:

```text
stateless: Input -> Value
stateful:  Input -> State -> (Value, NewState)
```

The caller carries the returned state into the next operation. Earlier immutable state values are not altered, yet the program remains stateful because each new state affects later behavior.

## An immutable cache

Consider a currency-rate lookup that should avoid repeated network requests. Its state can be an immutable dictionary from currency-pair names to rates:

```csharp
using Rates = ImmutableDictionary<string, decimal>;

static (decimal Rate, Rates NewState) GetRate(
    Func<string, decimal> fetch,
    string currencyPair,
    Rates cache)
{
    if (cache.ContainsKey(currencyPair))
        return (cache[currencyPair], cache);

    var rate = fetch(currencyPair);
    return (rate, cache.Add(currencyPair, rate));
}
```

A hit returns the cached value and the same state. A miss performs the lookup and returns a new dictionary containing the rate. This changes the signature from a stateless lookup into a state transition:

```text
remote lookup: string -> decimal
cached lookup: string -> Rates -> (decimal, Rates)
```

The application starts with `Rates.Empty`. A recursive loop accepts the current cache, calls `GetRate`, and passes the returned cache to its next invocation. This avoids even local mutation, but unbounded recursion is risky in C# because enough calls can exhaust the stack. A loop may instead reassign a local variable to each new immutable state; the state values themselves and all global state remain unmutated.

### Separate state mutation from other effects

Removing mutation does not remove network and console I/O. Passing the network operation as a `Func<string, decimal>` makes that dependency explicit and lets the cache logic be exercised with a predictable function. Console functions can be supplied in the same way.

Failure can also be represented in the function's type:

```text
before: (string -> decimal)      -> string -> Rates -> (decimal, Rates)
after:  (string -> Try<decimal>) -> string -> Rates -> Try<(decimal, Rates)>
```

```csharp
static Try<(decimal Rate, Rates NewState)> GetRate(
    Func<string, Try<decimal>> getRate,
    string currencyPair,
    Rates cache)
{
    if (cache.ContainsKey(currencyPair))
        return Try(() => (cache[currencyPair], cache));

    return from rate in getRate(currencyPair)
           select (rate, cache.Add(currencyPair, rate));
}
```

On retrieval failure, the caller reports the error and recurs with the original cache. Only a successful lookup yields a cache containing the new rate. Testability and error handling come from explicit function relationships and richer signatures rather than added interfaces or scattered `try/catch` blocks.

## Stateful computations

A stateful computation, also called a state transition, has the general shape:

```csharp
public delegate (T Value, S State)
    StatefulComputation<S, T>(S state);
```

```text
S -> (T, S)
```

`S` is the state before and after the operation; `T` is the produced value. A transition may also accept other arguments. The shape can occur inside a stateful or stateless application: it characterizes the function, not the architecture around it.

Passing state explicitly is often clearest for an isolated transition. Repeatedly extracting and forwarding it becomes noise when several transitions must be sequenced. `Map`, `Bind`, and `Return` capture that protocol:
- `Map` transforms the produced value and preserves the returned state.
- `Bind` runs the first computation, uses its value to choose the next computation, then runs that computation with the first computation's returned state.
- `Return` lifts a value into a computation that returns the state unchanged.

`Select` and `SelectMany` can expose these operations to LINQ query syntax. The syntax hides state plumbing; it does not remove the dependency or change its order.

## Pure pseudo-random generation

A composable generator is useful for property-based testing, load testing, and simulations such as Monte Carlo methods.

A conventional pseudo-random generator is deterministic but hides state. Its current seed is an implicit input to `Next`, and the updated seed is an implicit output affecting the next call. Make both explicit:

```csharp
public delegate (T Value, int Seed) Generator<T>(int seed);
```

```text
int -> (T, int)
```

An explicit seed makes generation repeatable and testable. A convenience runner may use the clock as a seed, but that overload is impure and not testable:

```csharp
public static T Run<T>(this Generator<T> generator, int seed)
    => generator(seed).Value;

public static T Run<T>(this Generator<T> generator)
    => generator(Environment.TickCount).Value;
```

Both runners discard the returned seed because they only expose the generated value.

### The primitive generator

The basic generator scrambles its input seed and returns the result as both the value and the next seed:

```csharp
public static Generator<int> NextInt = seed =>
{
    seed ^= seed >> 13;
    seed ^= seed << 18;
    var result = seed & 0x7fffffff;
    return (result, result);
};
```

The particular scrambling algorithm is not important to composition. What matters is that every call exposes the state needed by the next call.

### Deriving values with `Map`

`Map` reuses both the integer generator and its next seed while changing only the value:

```csharp
public static Generator<R> Map<T, R>(
    this Generator<T> generator,
    Func<T, R> map)
    => seed =>
    {
        var (value, nextSeed) = generator(seed);
        return (map(value), nextSeed);
    };

public static Generator<bool> NextBool
    => NextInt.Map(i => i % 2 == 0);
```

The same technique produces `NextChar` by reducing an integer modulo `char.MaxValue + 1` and casting it to `char`.

### Sequencing with `Bind`

Generating a pair manually requires feeding the first generation's seed into the second. `Bind` centralizes that threading:

```csharp
public static Generator<R> Bind<T, R>(
    this Generator<T> generator,
    Func<T, Generator<R>> next)
    => seed0 =>
    {
        var (value, seed1) = generator(seed0);
        return next(value)(seed1);
    };
```

With LINQ adapters, composite generators describe the value being built:

```csharp
Generator<(int First, int Second)> pairOfInts =
    from first in NextInt
    from second in NextInt
    select (first, second);

Generator<Option<int>> optionInt =
    from some in NextBool
    from value in NextInt
    select some ? Some(value) : None;
```

The second generator always consumes the seed returned by the first, so binding order determines the path taken through generator state.

### Recursive structures and generation policy

`Return` supplies a fixed value without consuming state:

```csharp
public static Generator<T> Return<T>(T value)
    => seed => (value, seed);
```

A recursive integer-list generator chooses between an empty result and a generated head followed by another generated list:

```csharp
Generator<IEnumerable<int>> IntList =>
    from empty in NextBool
    from list in empty ? Empty : NonEmpty
    select list;

Generator<IEnumerable<int>> Empty
    => Return(Enumerable.Empty<int>());

Generator<IEnumerable<int>> NonEmpty =>
    from head in NextInt
    from tail in IntList
    select List(head).Concat(tail);
```

This produces empty lists half the time, one-element lists one quarter of the time, and progressively longer lists with halving probability. It is therefore unlikely to produce long lists.

Generation policy is part of generator design. If a different size distribution is required, first generate a bounded length and then populate that many values. Strings follow the same compositional idea: generate a sequence of character values and construct a string from it.

## Generalizing beyond integer seeds

`Generator<T>` is `StatefulComputation<int, T>` specialized to an integer seed. The general form permits any state type while using the same `Map`, `Bind`, and `Return` behavior.

Tree numbering uses an integer counter as state:

```csharp
static StatefulComputation<int, int> GetAndIncrement
    = count => (count, count + 1);
```

For a leaf, the computation creates a new leaf containing the original value and current count, then returns the incremented count as its new state. For a branch, it numbers the left subtree, then numbers the right subtree with the state returned by the left. The result is a new tree whose leaves carry their traversal number.

The numbering function returns a computation rather than a numbered tree immediately. Supplying the initial counter runs it:

```csharp
var numberedTree = Number(tree).Run(0);
```

LINQ is especially useful in a branch, where multiple recursive transitions must be sequenced. For simpler cases, explicit state passing can remain clearer.

The same model is useful in simulations and parsers. A functional parser can treat its input text as state: it returns a structured parsed value together with the unconsumed remainder for the next parser.

## Choosing the representation

- Keep state visible in inputs and outputs so dependencies and sequencing are explicit.
- Use immutable state values when avoiding mutation; the caller advances by selecting the returned value as the next state.
- Keep explicit tuple passing when the state flow is short and clear.
- Use composition when many dependent transitions would otherwise repeat the same extract-and-forward plumbing.
- Treat sequencing as semantic: each computation receives exactly the state produced by its predecessor.
