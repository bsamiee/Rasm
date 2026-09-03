<!-- Ideas integrated into .claude/skills/dotnet-languageext/references/traits-and-transformers.md on the real types, the toy Maybe, List, and MaybeT code enters no skill
# [MONAD_FLAVOURS]

## [01]-[EFFECTS_AS_BEHAVIOR]

`Option`, `Either`, `Fin`, and `Validation` enlarge a function's possible outputs with a `None`, `Left`, or `Fail` case. The added case is not the effect. The effect is the behavior their operations give those cases: sequencing `Option` computations terminates when one returns `None`. Monads capture effects, and side effects are a subset of effects. Monads let pure code sequence operations with between-step behavior `Bind` defines, while the whole remains one pure expression. Functors and applicatives carry the same effectfulness: `Option.Map` does not invoke its function for `None`.

## [02]-[FLATTEN_BIND_MAP]

`Flatten`, also called monad join, removes one layer from two nested instances of the same monad:

```csharp
K<M, A> Flatten<A>(K<M, K<M, A>> mma)
```

It evaluates the outer monad far enough to obtain the inner monad, `Bind` without the mapping function. `Flatten` derives from `Bind` and `identity`:

```csharp
public static K<Maybe, A> Flatten<A>(K<Maybe, K<Maybe, A>> mma) =>
    mma.Bind(identity);
```

For `Maybe`, a direct implementation exists:

```csharp
public static K<Maybe, A> Flatten<A>(K<Maybe, K<Maybe, A>> mma) =>
    mma switch
    {
        Just<K<Maybe, A>>(var ma) => ma,
        Nothing<K<Maybe, A>> => new Nothing<A>()
    };
```

With `Map`, the relationship reverses:

```csharp
public static K<Maybe, B> Bind<A, B>(
    K<Maybe, A> ma,
    Func<A, K<Maybe, B>> f) =>
    ma.Map(f).Flatten();
```

This is why `Bind` is also called `FlatMap`. Custom monads implement `Bind` from `Flatten` and `Map`, use the default `Flatten` derived from `Bind`, or provide bespoke versions of both, whichever is simpler or more efficient for the type.

## [03]-[BUILDING_MONADS]

Monad implementation is small. The raw functor, applicative, and monad operations unlock the generic LanguageExt functionality, LINQ included. The data type's supporting functions (`ask`, `tell`, `get`, `put`) form its practical API. Custom monads are ordinary application types with cross-cutting behavior: a database monad that manages connections, sub-spaces, security, and I/O, a service monad that manages third-party access, resources, configuration, and I/O, and a `Free`-based API monad that coordinates subsystems. The clearest place to see each monad's distinguishing behavior is its `Bind` implementation.

## [04]-[ALTERNATIVE_VALUE_MONADS]

### [04.1]-[MAYBE]

`Maybe<A>` has the role of `Option<A>`: `Just<A>` or `Nothing<A>`, with early termination in the absent case.

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

`Pure` constructs `Just`, `Map` and `Apply` inherit the absent-value behavior through `Bind`.

### [04.2]-[EITHER]

`Either<L, R>` holds a `Right` result or a `Left` failure value. `Bind` invokes the next operation for `Right` and carries the existing `Left` through without invoking it.

### [04.3]-[FIN]

`Fin<A>` matches `Either<Error, A>` with `Succ<A>` and `Fail<A>` cases. Fixing `Error` makes the error-or-result shape easier to use than supplying both type arguments to `Either`.

### [04.4]-[VALIDATION]

`Validation<F, A>` terminates monadic sequencing on `Fail`, like `Either`. Its failure type `F` is a monoid, which gives the applicative `Apply` an added behavior: when the function and argument validations both fail, `+` combines their errors.

```csharp
(mf, ma) switch
{
    (Succ<F, Func<A, B>>(var f), Succ<F, A>(var a)) => new Succ<F, B>(f(a)),
    (Fail<F, Func<A, B>>(var e1), Fail<F, A>(var e2)) => new Fail<F, B>(e1 + e2),
    (Fail<F, Func<A, B>>(var e), _) => new Fail<F, B>(e),
    (_, Fail<F, A>(var e)) => new Fail<F, B>(e)
};
```

The monadic effect is early termination, the applicative effect aggregates failures.

### [04.5]-[TRY]

`Try<A>` wraps a `Func<A>` and delays a computation that can throw. `Bind` builds another delayed computation: run the first thunk, then the thunk the next operation returns. `RunUnsafe` invokes without catching. `Run` catches and returns `Succ<A>` or `Fail<A>` as `Fin<A>`, moving exception behavior into the declared result.

## [05]-[COLLECTION_MONADS]

### [05.1]-[ITERABLE]

`Iterable<A>` wraps `IEnumerable<A>` with higher-kinded traits. Its effect is iteration over multiple values, an empty collection terminates that branch. Its `Bind` is nested iteration:

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

`Option` compares to a collection of zero or one item. `Pure` creates a one-item collection. Other collection monads bind with the same structure.

## [06]-[ENVIRONMENT_AND_STATE_MONADS]

### [06.1]-[READER]

`Reader<Env, A>` wraps a lazy `Func<Env, A>`. Its effect carries one read-only environment through a computation without an environment parameter on every function.

```csharp
public static K<Reader<Env>, B> Bind<A, B>(
    K<Reader<Env>, A> ma,
    Func<A, K<Reader<Env>, B>> f) =>
    new Reader<Env, B>(env => f(ma.Run(env)).Run(env));
```

Both stages receive the same environment. `ask<Env>()` obtains the whole environment, `asks` projects a value from it. Environments hold configuration or dependencies, which makes Reader a pure functional form of dependency injection.

### [06.2]-[WRITER]

`Writer<Out, A>` produces a result and an output log. `Out` is a monoid with `Empty` and `+` for appending. The representation accepts the current log and returns the result with the updated log. `Bind` runs the first computation with the input log, then gives its updated log to the next. `tell` appends one item:

```csharp
public static Writer<Out, Unit> tell<Out>(Out item)
    where Out : Monoid<Out> =>
    new(log => (default, log + item));
```

Representations that store output without accepting an input log concatenate collections during `Bind`, the threaded representation concentrates concatenation in `tell`.

### [06.3]-[STATE]

`State<S, A>` wraps `Func<S, (A Value, S State)>`. Its effect threads an updateable state through a computation without a state argument and state-bearing tuple on every function.

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

`get` returns the current state, `gets` projects from it, and `put` updates it. Writer and State share the binding structure, Writer specializes the threaded value to monoidal output and adds `tell`.

## [07]-[OTHER_MONADS]

`IO<A>` manages world-changing side effects, and `Eff<A>` is a compound monad built from other pieces. `Free<F, A>` turns a functor into a monad, `Cont<A>` handles continuations, `Identity<A>` has no effect, and the collection types `Seq<A>`, `Arr<A>`, `Lst<A>`, `Set<A>`, and `HashSet<A>` are monads.

## [08]-[COMPOSITION_LIMIT]

These are single-feature monads. Monadic expressions work with one constructor at a time, `Option` and `IO` do not combine directly in one expression. Dedicated types for specific pairings do not scale: the handwritten combinations grow with every monad and every larger combination. Monad transformers compose existing monads into combined monads.
-->
