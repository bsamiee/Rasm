# [MONAD_FLAVOURS]

## [01]-[EFFECTS_AS_BEHAVIOR]

A pure, total function maps every input to an output. Because it does not change the outside world, its transformation can be treated as having no event-like passage of time, even though it still takes processor time. An impure function changes the world, introducing cause and effect.

Types such as `Option`, `Either`, `Fin`, and `Validation` enlarge a function's possible outputs with cases such as `None`, `Left`, or `Fail`. Merely adding those cases is not the effect. The effect is the behavior that their operations give those cases. For example, sequencing `Option` computations terminates when one returns `None`.

Monads capture both effects and side effects; side effects are a subset of effects. A monad lets pure functional code sequence operations whose particular behavior between steps is defined by `Bind`, while producing one pure expression. Functors and applicatives participate in the same effectfulness: `Option.Map`, for example, does not invoke its function for `None`.

## [02]-[FLATTEN_BIND_MAP]

`Flatten`, also called monad join, removes one layer from two nested instances of the same monad:

```csharp
K<M, A> Flatten<A>(K<M, K<M, A>> mma)
```

It must evaluate the outer monad far enough to obtain the inner monad. It resembles `Bind` without the mapping function.

`Flatten` can be defined from `Bind` and `identity`:

```csharp
public static K<Maybe, A> Flatten<A>(K<Maybe, K<Maybe, A>> mma) =>
    mma.Bind(identity);
```

For `Maybe`, it can instead be implemented directly:

```csharp
public static K<Maybe, A> Flatten<A>(K<Maybe, K<Maybe, A>> mma) =>
    mma switch
    {
        Just<K<Maybe, A>>(var ma) => ma,
        Nothing<K<Maybe, A>> => new Nothing<A>()
    };
```

With `Map`, the relationship also works in the other direction:

```csharp
public static K<Maybe, B> Bind<A, B>(
    K<Maybe, A> ma,
    Func<A, K<Maybe, B>> f) =>
    ma.Map(f).Flatten();
```

This is why `Bind` is also called `FlatMap`. A custom monad can implement `Bind` from `Flatten` and `Map`, use the default `Flatten` derived from `Bind`, or provide bespoke versions of both. The best route depends on which implementation is simpler or more efficient for the type.

## [03]-[BUILDING_MONADS]

A monad implementation can be small. The raw functor, applicative, and monad operations are enough to use generic language-ext functionality, including LINQ, but a data type's supporting functions form its practical API. Functions such as `ask`, `tell`, `get`, and `put` make the particular effect usable.

Custom monads are ordinary application types with bespoke, cross-cutting behavior. A domain monad can enforce concerns such as security, resource management, configuration, logging, state, and I/O across a computation. Examples include a database monad that manages connections, sub-spaces, security, and I/O; a service monad that manages third-party access, resources, security, configuration, and I/O; and a `Free`-based API monad that coordinates subsystems, resource use, and configuration. The clearest place to see each monad's distinguishing behavior is its `Bind` implementation.

## [04]-[ALTERNATIVE_VALUE_MONADS]

### [04.1]-[MAYBE]

`Maybe<A>` has the same role as `Option<A>`: it contains either `Just<A>` or `Nothing<A>`. Its effect is early termination in the absent case.

```csharp
public static K<Maybe, B> Bind<A, B>(
    K<Maybe, A> ma,
    Func<A, K<Maybe, B>> f) =>
    ma switch
    {
        Just<A>(var value) => f(value),
        Nothing<A> => new Nothing<B>()
    };
```

`Pure` constructs `Just`; `Map` and `Apply` inherit the same absent-value behavior through `Bind`.

### [04.2]-[EITHER]

`Either<L, R>` holds either a `Right` result or a `Left` failure value. `Bind` invokes the next operation for `Right` and carries the existing `Left` value through without invoking it.

### [04.3]-[FIN]

`Fin<A>` has the same effect as `Either<Error, A>`, with `Succ<A>` and `Fail<A>` cases. Baking in `Error` makes the common error-or-result shape easier to use than supplying both type arguments to `Either`. In language-ext v5, it is the intended replacement for v4's `Result<A>`.

### [04.4]-[VALIDATION]

`Validation<F, A>` terminates monadic sequencing on `Fail`, like `Either`. Its failure type `F` must be a monoid, which gives its applicative `Apply` an additional behavior: when the function and argument validations both fail, their errors are combined with `+`.

```csharp
(mf, ma) switch
{
    (Succ<F, Func<A, B>>(var f), Succ<F, A>(var a)) => new Succ<F, B>(f(a)),
    (Fail<F, Func<A, B>>(var e1), Fail<F, A>(var e2)) => new Fail<F, B>(e1 + e2),
    (Fail<F, Func<A, B>>(var e), _) => new Fail<F, B>(e),
    (_, Fail<F, A>(var e)) => new Fail<F, B>(e)
};
```

The monadic effect is early termination; the applicative effect can aggregate multiple failures.

### [04.5]-[TRY]

`Try<A>` wraps a `Func<A>`, delaying a computation that may throw. `Bind` builds another delayed computation by running the first thunk and then the thunk returned by the next operation.

`RunUnsafe` invokes the thunk without catching. `Run` catches an exception and returns `Succ<A>` or `Fail<A>` as a `Fin<A>`, moving exception behavior into the computation's declared result.

## [05]-[COLLECTION_MONADS]

### [05.1]-[ITERABLE]

`Iterable<A>` wraps `IEnumerable<A>` with higher-kinded traits. Its effect is to iterate multiple values; an empty collection terminates that branch of the computation.

Its `Bind` is nested iteration:

```csharp
IEnumerable<B> Go()
{
    foreach (var x in ma.As())
    {
        foreach (var y in f(x).As())
        {
            yield return y;
        }
    }
}
```

This makes `Option` comparable to a collection of zero or one item. `Pure` creates a one-item collection. Other collection monads use essentially the same binding structure.

## [06]-[ENVIRONMENT_AND_STATE_MONADS]

### [06.1]-[READER]

`Reader<Env, A>` wraps a lazy `Func<Env, A>`. Its effect is to carry one read-only environment through a computation without adding an explicit environment argument to every function.

```csharp
public static K<Reader<Env>, B> Bind<A, B>(
    K<Reader<Env>, A> ma,
    Func<A, K<Reader<Env>, B>> f) =>
    new Reader<Env, B>(env => f(ma.Run(env)).Run(env));
```

Both stages receive the same environment. `ask<Env>()` obtains the whole environment, while `asks` projects a value from it. An environment can hold configuration or dependencies, making Reader a pure functional form of dependency injection.

### [06.2]-[WRITER]

`Writer<Out, A>` produces a result and an output log. `Out` must be a monoid, providing `Empty` and `+` for appending output.

This representation accepts the current log and returns the result with the updated log. `Bind` runs the first computation with the input log, then gives its updated log to the next computation. `tell` appends one item:

```csharp
public static Writer<Out, Unit> tell<Out>(Out item)
    where Out : Monoid<Out> =>
    new(log => (default, log + item));
```

An alternative representation that stores output without accepting an input log generally has to concatenate collections during `Bind`; the threaded representation concentrates concatenation in `tell`.

### [06.3]-[STATE]

`State<S, A>` wraps `Func<S, (A Value, S State)>`. Its effect is to thread an updateable state through a computation without adding a state argument and state-bearing tuple result to every participating function.

```csharp
public static K<State<S>, B> Bind<A, B>(
    K<State<S>, A> ma,
    Func<A, K<State<S>, B>> f) =>
    new State<S, B>(state =>
    {
        var (value, state1) = ma.Run(state);
        return f(value).Run(state1);
    });
```

`get` returns the current state, `gets` projects from it, and `put` updates it. Writer and State have nearly identical binding structure; Writer specializes the threaded value to monoidal output and provides `tell` to append to it.

## [07]-[OTHER_MONADS]

`IO<A>` manages world-changing side effects. `Eff<A>` is a compound monad that needs more explanation than a raw implementation. Other monads include `Free<F, A>`, which turns a functor into a monad; `Cont<A>` for continuations; `Identity<A>`, which has no effect; and collection types such as `Seq<A>`, `Arr<A>`, `Lst<A>`, `Set<A>`, and `HashSet<A>`.

## [08]-[COMPOSITION_LIMIT]

These are single-feature monads. A monadic expression works with one constructor at a time, so `Option` and `IO` cannot be combined directly in one monadic expression.

Earlier language-ext versions addressed specific pairings with manually implemented types such as `OptionAsync`, `EitherAsync`, `TryAsync`, `TryOption`, and `TryOptionAsync`. That approach does not scale: the number of handwritten combinations grows rapidly as more monads and larger combinations are needed.

Monad transformers provide the general solution in language-ext v5 by composing existing monads into combined monads.
