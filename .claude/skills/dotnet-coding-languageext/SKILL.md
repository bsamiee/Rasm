---
name: dotnet-coding-languageext
description: "Use when calling a LanguageExt member, covering result types, errors, IO effects, traits, transformers, collections, shared state, and streams."
---

# [DOTNET_CODING_LANGUAGEEXT]

Covers LanguageExt types and their operations, from result types and their conversions to shared state.

[REFERENCES]:
- [01]-[TRAITS_AND_TRANSFORMERS](references/traits-and-transformers.md): Trait encoding, transformer stacks, and the domain monads over them
- [02]-[STREAMS](references/streams.md): Stream API from sources to pipes, with the buffer policies and their forking order
- [03]-[API](references/api.md): Public types and members by scope, with the notes each scope needs

Result, effect, and collection types come from `LanguageExt.Core`, the runtimes with their `ConsoleIO` and `FileIO` traits from `LanguageExt.Sys`, and `Source`, `Conduit`, and the pipes from `LanguageExt.Streaming`. Examples assume `using static LanguageExt.Prelude`, which binds constructors and module functions as unqualified names (`Some`, `None`, `Seq`, `toSeq`, `Range`, `parseInt`, `Pure`, `guard`, `use`, `atomic`, `memo`).

## [01]-[RESULT_TYPES]

Result and effect types and their runtime shapes:

| [INDEX] | [TYPE]                 | [SHAPE]               |
| :-----: | :--------------------- | :-------------------- |
|  [01]   | `Option<A>`            | readonly struct       |
|  [02]   | `Fin<A>`               | abstract class        |
|  [03]   | `Either<L, R>`         | abstract record class |
|  [04]   | `Validation<Error, A>` | abstract record class |
|  [05]   | `Try<A>`               | record class          |
|  [06]   | `IO<A>`                | abstract record class |
|  [07]   | `Eff<RT, A>`           | record class          |

Each type exposes `Match` with one function per case:
- `Match` on `IO` and `Eff` returns an effect
- `Option<A>` holds a flag and an inner value, exposes the flag as `IsSome` and `IsNone`, and ignores the inner value in the `None` state
- `Option<A>` and `Fin<A>` are closed, a `Match` over their cases is total
- Implicit conversion from `A` maps `null` to `None`, `Optional(x)` does the same for nullable input, `Some(x)` wraps the value as given
- `Option<A>` defines `operator true`, `left || right` evaluates the right operand only when the left is `None`, `left | right` evaluates both
- `IfNone(A)` takes a value, `IfNone(Func<A>)` takes a computation that runs only on `None`, both return `A`
- `Prelude` forms `match(option, Some, None)`, `ifNone(option, Func<A>)`, and `ifNone(option, A)` compile unqualified and qualified
- Values in the `None` slot of `match` are CS0201
- `Fin` has no module form, `match(fin, Succ:, Fail:)` and `ifFail(fin, f)` resolve to the `Eff` overloads (CS0411, CS0029)

Each conversion is a method on the source type named for the target, and converting from `Option` requires the `Error` it lacks:

```csharp
internal static class Conversions {
    public static Fin<int> Required(Option<int> value) => value.ToFin(new NotFound());
    public static Validation<Error, int> Checked(Option<int> value) => value.ToValidation<Error>(new NotFound());
    public static Option<Quantity> Present(Fin<Quantity> quantity) => quantity.ToOption();
    public static Either<Error, Quantity> Split(Fin<Quantity> quantity) => quantity.ToEither();
    public static Seq<int> Items(Option<int> value) => value.ToSeq();
    public static Fin<Item> Exit(Validation<Error, Item> form) => form.ToFin();
    public static Validation<Error, Quantity> Widen(Fin<Quantity> quantity) => quantity.ToValidation();
    public static Fin<int> Captured(Try<int> attempt) => attempt.Run();
    public static Fin<Item> Ran(IO<Item> effect) => effect.RunSafe();
    public static Fin<string> Answered(AppRuntime runtime) => Prompts.Ask<AppRuntime>("name").Run(runtime);
}
```

- `ToOption` on a `Fin` drops the failure reason
- `Validation` becomes `Fin` at the end of input validation
- `Fin` from a `From` factory becomes `Validation` before it combines with independent validations
- `Try`, `IO`, and `Eff` return `Fin` when run

## [02]-[OPERATIONS]

`Option<A>`, `Seq<A>`, and `Fin<A>` share the operation names, `Fin<A>` has no `Filter`, `Seq<A>` has no `Pure`, and LINQ names `Map` as `Select`, `Bind` as `SelectMany`, and `Filter` as `Where`:

| [INDEX] | [OPERATION] | [SIGNATURE]                 | [BEHAVIOR]                                                   |
| :-----: | :---------- | :-------------------------- | :----------------------------------------------------------- |
|  [01]   | `Pure`      | `A -> F<A>`                 | Lifts a plain value, converts into `Option`, `Fin`, and `IO` |
|  [02]   | `Map`       | `(F<A>, A -> B) -> F<B>`    | Applies the function to each value and preserves `F`         |
|  [03]   | `Bind`      | `(F<A>, A -> F<B>) -> F<B>` | Applies the function and flattens the nested result          |
|  [04]   | `Filter`    | `(F<A>, A -> bool) -> F<A>` | Keeps the values that satisfy the predicate                  |
|  [05]   | `Iter`      | `(F<A>, Action<A>) -> Unit` | Runs the action for each value at once and returns `Unit`    |
|  [06]   | `Do`        | `(F<A>, Action<A>) -> F<A>` | Runs the action for each value and returns the input         |

- `Map` on `Option` applies the function once for `Some` and never for `None`, on `Seq` lazily to every element
- `Bind` on `Option` skips the function after `None`, on `Seq` concatenates every produced sequence into one
- `Filter` on `Option` turns a failed predicate into `None`, LINQ `where` works the same
- `Iter` has its own name, overload resolution cannot distinguish `Action<A>` from `Func<A, B>` by return type
- `Fin<A>` supplies `IfSucc` in place of `Do`, a side effect inside `IO` is a bound `IO.lift` step
- After `None` or a failure no later action runs
- `ToSeq` converts an option to a zero-or-one-element sequence, `Flatten` on the `ToSeq` of an `Option<Seq<A>>` yields the `Seq<A>`

Values in context have type `F<A>`, and the type constructor supplies the computational effect:
- `Option` adds absence, `Seq` zero or more values, `Func<A>` deferred evaluation
- `Fin` adds expected failure with a reason, `IO` a deferred side effect with a failure channel
- No general operation extracts one `A` from every `F<A>`, `Match`, `Count`, `Fold`, `IfNone`, and `RunSafe` are the type-specific extractions

## [03]-[ERRORS]

Domain errors are `sealed record`s extending `Expected` with a message and a code:
- `Exceptional` is the error `Try` and `IO` produce from a captured exception
- `ManyErrors` is the error `+` and `Validation` produce from accumulation
- `Errors` holds the shared values (`Errors.TimedOut`, `Errors.None`)

```csharp
internal static class Codes {
    public const int InvalidQuantity = 2101;
    public const int NotFound = 2103;
    public const int Rejected = 2106;
}

internal sealed record InvalidQuantity() : Expected("quantity out of range", Codes.InvalidQuantity);
internal sealed record NotFound() : Expected("item not found", Codes.NotFound);
internal sealed record Rejected : Expected {
    public Rejected(Error cause) : base("request rejected", Codes.Rejected, cause) { }
}

internal static class Classify {
    public static bool Retryable(Error error) => error.Is(Errors.TimedOut) || error.HasException<IOException>();
    public static bool IsRejection(Error error) => error.HasCode(Codes.InvalidQuantity) || error.IsType<NotFound>();
    public static int QuantityFaults(Error error) => error.Filter<InvalidQuantity>().Count;
}
```

- `IsType<E>` and `Filter<E>` search the leaves of a `ManyErrors`, `Count` returns the number of accumulated errors, `Head` the first leaf
- `HasCode` and `Catch(int)` select a code the same package declares
- Codes from many packages meet in one `ManyErrors` where `IsType<E>` separates them
- `Error.New(string, Error)` has code `0`
- `IsType`, `HasCode`, `Is`, and `Catch` do not descend into `Inner`, only the typed record that wraps a cause stays classifiable
- `Error` implements `Monoid<Error>`, a custom failure type implements `Monoid<F>` before `Validation<F, A>` accumulates it
- `&` on operands with one success type collects the successes into `Seq<A>` and accumulates the failures
- `|` returns the first success and combines the errors only when both fail

Recovery is a function from an error to the same result type, and the overloads select the error by code, by value, or by predicate:

```csharp
internal static class Recovery {
    public static Fin<Quantity> ByCode(Fin<Quantity> quantity) => quantity.Catch(Codes.InvalidQuantity, static _ => Quantity.From(0)).As();
    public static Fin<Quantity> ByValue(Fin<Quantity> quantity) => quantity.Catch(new InvalidQuantity(), static _ => Quantity.From(0)).As();
    public static Fin<Quantity> ByPredicate(Fin<Quantity> quantity) => quantity.Catch(static error => error.IsExpected, static _ => Quantity.From(0)).As();
    public static IO<Item> Cached(IO<Item> load, Item cached) => load.Catch(Codes.NotFound, _ => IO.pure(cached)).As();
    public static IO<Item> Fallback(IO<Item> primary, IO<Item> secondary) => primary | secondary;
    public static Fin<Quantity> Rebound(Fin<Quantity> quantity) => quantity.BindFail(static error => error.HasCode(Codes.InvalidQuantity) ? Quantity.From(0) : error);
    public static Fin<Quantity> WithContext(Fin<Quantity> quantity) => quantity.MapFail(static error => new Rejected(error));
    public static int AtHost(Fin<int> result) => result.IfFail(static _ => -1);
}
```

- Code and error-value overloads of `Catch` are extensions that return `K<F, A>`, `.As()` restores the concrete type
- `IO<A>` declares the predicate overload as an instance method that returns `IO<A>`
- `|` uses the right alternative when the left fails
- `BindFail` lets the recovery return either case, `MapFail` changes only the error, `BiMap` maps both sides, `IfFail` returns a plain value

In a LINQ query, `guard` raises an `Error` when its flag is false:
- `when` runs its alternative when the flag is true, `unless` when the flag is false
- Alternative is a failed `Fin<Unit>` or `IO<Unit>`
- `guard<Error>` names the type argument, an `Expected` subclass argument selects the generic overload `guard<E>(bool, E)`

```csharp
internal static class Guards {
    public static Fin<int> Bounded(int value) =>
        from v in Pure(value).ToFin()
        from _ in guard<Error>(v >= 0, new InvalidQuantity())
        from __ in when(v > 1_000, Reject(new InvalidQuantity()))
        select v;
    public static IO<int> Metered(IO<int> read) =>
        from v in read
        from _ in unless(v <= 1_000, IO.fail<Unit>(new InvalidQuantity()))
        select v;

    private static Fin<Unit> Reject(Error error) => error;
}
```

## [04]-[EFFECTS]

`IO.lift` reads the return type of its thunk to select an overload:
- `Func<A>` defers the value, `Func<Fin<A>>` converts a `Fail` to an `IO` failure
- Type argument in `IO.lift<Fin<A>>` keeps the `Fin` as the value
- `IO.lift(Fin<A>)` lifts an evaluated result, as does `IO.lift(Next)` over a `Fin<A> Next()` method group
- `Fin<A>` has no `ToIO`
- Bound `Fin` values lift through `.Bind(IO.lift)`, `.Bind(fin => IO.lift(() => fin))`, or `from x in IO.lift(fin)`, each typed `IO<A>`

```csharp
internal static class Construction {
    public static IO<int> Plain => IO.lift(static () => 1);
    public static IO<int> Folded => IO.lift(static () => Quantity.From(2).Map(static q => (int)q));
    public static IO<bool> Carried => IO.lift<Fin<int>>(static () => Pure(3)).Map(static fin => fin.IsSucc);
    public static IO<int> TokenAware => IO.liftAsync(static env => Remote.FetchAsync(6, env.Token));
}
```

- `Run` and `RunAsync` represent an `Expected` error as an `ErrorException` and rethrow the exception an `Exceptional` error captured
- `Try.lift(io.Run).Run()` captures the thrown error and returns the original `Expected`
- Disposable `EnvIO.New(token:)` passes the cancellation token into `Run(env)`
- Cancelled tokens escape `RunSafe` as an exception that a host captures with `Try.lift`

### [04.1]-[RESOURCES]

`use(Func<A>)` acquires an `IDisposable` inside the effect and disposes it when the scope ends on either path:
- `use(Func<A>, Action<A>)` names the release step and runs it on every exit
- `Bracket(Use:, Fin:)` runs `Fin` after `Use` on both paths
- `Finally` attaches an effect that runs after a deferred receiver on both paths, an `IO.fail` receiver returns itself without it

```csharp
internal static class Resources {
    public static IO<int> Released =>
        from connection in use(static () => new Connection(), static c => c.Dispose())
        select connection.Query();
    public static IO<int> Audited(IO<int> work, Atom<int> closed) =>
        work.Finally(IO.lift(() => closed.Swap(static n => n + 1)));
}
```

### [04.2]-[CONCURRENCY]

`Fork` starts the effect on one `TaskCreationOptions.LongRunning` thread and returns a `ForkIO` with `Await` and `Cancel`:
- `awaitAll` runs every effect of a `Seq<IO<A>>` and collects the values, `awaitAny` returns the first value
- `timeout(duration, effect)` fails the effect after the duration, `Uninterruptible` masks cancellation
- `PartitionFallible` takes a `Seq<K<IO, A>>` (`Map<K<IO, A>>` sets that element type) and runs every effect without a short-circuit
- `PartitionFallible` returns `(Seq<Error> Fails, Seq<A> Succs)`, `Succs` and `Fails` return one side
- `Traverse` under `IO` starts every element effect before awaiting any
- Effects built with `IO.liftAsync` overlap without a bound, effects built with `IO.lift` run in order on the calling thread
- Bounded parallelism chunks the collection and traverses the chunks with `TraverseM`

```csharp
internal static class Concurrency {
    public static IO<Seq<int>> Chunked(Seq<int> items, int width, Func<int, IO<int>> work) =>
        toSeq(items.Chunk(width))
            .TraverseM(chunk => toSeq(chunk).Traverse(work).As())
            .As()
            .Map(static chunks => chunks.Flatten());
    public static IO<(Seq<Error> Fails, Seq<int> Succs)> BestEffort(Seq<int> items, Func<int, IO<int>> work) =>
        items.Map<K<IO, int>>(item => work(item)).PartitionFallible().As();
}
```

### [04.3]-[RECURSION]

`tail` marks the last bind continuation after a deferred effect and keeps the stack constant:
- `tail`-recursive `IO` exits through `Run()` or `RunAsync()` alone
- `RunSafe()`, `Try()`, `Map`, or a later `Bind` after the tail call throws `NotSupportedException`
- `Monad.recur<M, A, B>` loops a state `A` with `Next.Loop<A, B>` and finishes with `Next.Done<A, B>` holding a `B`
- `Monad.recur` works with every host operation and returns `K<IO, B>` that `.As()` narrows
- `Monad.recur` checks the state before it advances, an already-finished initial state returns unchanged
- `RepeatUntil` polls one effect until its value satisfies the predicate, `RepeatWhile` polls while it does

```csharp
internal static class Recursion {
    public static IO<int> CountTo(int current, int limit) =>
        current >= limit ? IO.pure(current) : IO.lift(() => current + 1).Bind(next => tail(CountTo(next, limit)));
    public static IO<Session> Play(Session initial, IO<int> readMove) =>
        Monad.recur<IO, Session, Session>(initial, session =>
            session.HasExited
                ? IO.pure(Next.Done<Session, Session>(session))
                : readMove.Map(move => Next.Loop<Session, Session>(session.Apply(move)))).As();
    public static IO<int> Poll(Atom<int> polls) => IO.lift(() => polls.Swap(static n => n + 1)).RepeatUntil(static n => n >= 3);
}
```

### [04.4]-[SCHEDULES]

`Schedule.spaced` and `Schedule.exponential` build delay sequences:
- `recurs`, `repeat`, `jitter`, and `maxDelay` are `ScheduleTransformer` values
- `recurs` caps the attempt count, `repeat` replays the whole schedule, `jitter` randomizes each delay, `maxDelay` caps each delay
- Each delay is `LanguageExt.Duration`, which converts implicitly from the `TimeSpan` a NodaTime `Duration` returns through `ToTimeSpan()`
- Between schedules `|` is a union that takes the shorter delay and `&` an intersection that takes the longer
- When one side is a transformer both operators apply it
- Transformers passed alone convert to `Schedule.Forever` with the transformer applied
- `Retry(Schedule)` reruns a deferred effect that failed, `Repeat(Schedule)` reruns a successful one
- `Repeat(Schedule.recurs(2))` runs the effect once and then twice more

```csharp
internal static class Schedules {
    public static Schedule Backoff => Schedule.exponential(Duration.FromMilliseconds(1).ToTimeSpan()) | Schedule.recurs(3) | Schedule.jitter();
    public static Schedule Capped => Schedule.exponential(Duration.FromMilliseconds(1).ToTimeSpan()) & Schedule.maxDelay(Duration.FromMilliseconds(4).ToTimeSpan());
    public static Schedule Replayed => Schedule.spaced(Duration.FromMilliseconds(1).ToTimeSpan()) | Schedule.recurs(2) | Schedule.repeat(2);
}
```

### [04.5]-[RUNTIMES]

Runtime records implement `Has<Eff<RT>, T>` once per capability with `Eff.runtime<RT>().Map(static rt => rt.Capability)`:
- Consumers generic over `RT` read the capability through `RT.Ask` or through a module constrained on the trait
- `Console<RT>.writeLine` and `Console<RT>.readLine` need `ConsoleIO`, `File<RT>` needs `FileIO` and `EncodingIO`
- `Run(rt)` returns `Fin<A>`, `RunAsync(rt)` returns `Task<Fin<A>>`

```csharp
internal sealed record AppRuntime(ConsoleIO Console) : Has<Eff<AppRuntime>, ConsoleIO> {
    static K<Eff<AppRuntime>, ConsoleIO> Has<Eff<AppRuntime>, ConsoleIO>.Ask => Eff.runtime<AppRuntime>().Map(static rt => rt.Console);
}

internal static class Prompts {
    public static Eff<RT, string> Ask<RT>(string prompt) where RT : Has<Eff<RT>, ConsoleIO> =>
        from _ in Console<RT>.writeLine(prompt)
        from line in Console<RT>.readLine
        select line;
}
```

- `LanguageExt.Sys.Test.Runtime.New()` supplies a `MemoryConsole` and a file system rooted at a temporary directory under `Env.RootPath`
- Disposing the test runtime deletes that directory
- `WriteKeyLine` feeds console input, enumerating the console returns the written lines
- `LanguageExt.Sys.Live.Runtime.New()` supplies the live host services

## [05]-[TRAITS_AND_TRANSFORMERS]

`K<F, A>` is an empty interface that pairs the witness `F` for a type constructor with the element type `A`:
- Witness is the concrete type without its last type argument, `Option<A>` implements `K<Option, A>` and `Either<L, R>` implements `K<Either<L>, R>`
- Traits are interfaces with static abstract members that the witness implements
- Generic code names the witness in a constraint, `.As()` restores the concrete type at the API boundary

```csharp
internal static class Generic {
    public static K<F, decimal> Amounts<F>(K<F, Item> items) where F : Functor<F> => items.Map(static item => item.Amount);
    public static Option<decimal> OptionAmount(Option<Item> item) => Amounts(item).As();
    public static Seq<decimal> SeqAmounts(Seq<Item> items) => Amounts(items).As();
}
```

| [INDEX] | [TRAIT]            | [MEMBERS]                                    |
| :-----: | :----------------- | :------------------------------------------- |
|  [01]   | `Functor<F>`       | `Map`                                        |
|  [02]   | `Applicative<F>`   | `Pure`, `Apply`, tuple `Apply`               |
|  [03]   | `Monad<M>`         | `Bind`, LINQ query syntax                    |
|  [04]   | `Foldable<T>`      | `Fold`, `FoldBack`, `Exists`, `ForAll`, `At` |
|  [05]   | `Traversable<T>`   | `Traverse`, `TraverseM`                      |
|  [06]   | `Fallible<E, F>`   | `Fail`, `Catch`                              |
|  [07]   | `Readable<M, Env>` | `ask`, `asks`, `local`                       |
|  [08]   | `Stateful<M, S>`   | `get`, `put`, `modify`, `state`, `local`     |
|  [09]   | `Writable<M, W>`   | `tell`                                       |
|  [10]   | `Alternative<F>`   | `Empty`, `Choose`, the alternative operator  |

- `Map`, `Bind`, `Fold`, `FoldBack`, `Exists`, `ForAll`, `At`, `Catch`, tuple `Apply`, and LINQ query syntax are constraint extensions
- `F.Pure`, `F.Apply`, `F.Fail`, `F.Empty`, `F.Choose`, `T.Traverse`, and `T.TraverseM` are calls on the witness
- `Readable.ask`, `Stateful.get`, and `Writable.tell` are module functions that take the witness as a type argument
- `Fallible<F>` fixes `E` to `Error`
- `Alternative<F>` extends `Choice<F>` and makes `Choose` the generic form of `|`
- `Reader<Env>`, `ReaderT<Env, M>`, and `Eff<RT>` implement `Readable`, `State<S>` and `StateT<S, M>` implement `Stateful`
- `Writer<W>` and `WriterT<W, M>` implement `Writable`

Transformers stack one concern over an inner monad `M`, and the wrapped representation decides what each `Run` returns:

| [INDEX] | [TRANSFORMER]              | [HOLDS]                      | [RUN]                         |
| :-----: | :------------------------- | :--------------------------- | :---------------------------- |
|  [01]   | `OptionT<M, A>`            | `K<M, Option<A>>`            | Returns the wrapped value     |
|  [02]   | `FinT<M, A>`               | `K<M, Fin<A>>` as `runFin`   | Returns the wrapped value     |
|  [03]   | `EitherT<L, M, A>`         | `K<M, Either<L, A>>`         | Returns the wrapped value     |
|  [04]   | `ValidationT<Error, M, A>` | Accumulates inside an effect | Returns the wrapped value     |
|  [05]   | `ReaderT<Env, M, A>`       | `Func<Env, K<M, A>>`         | Applies the function to `Env` |
|  [06]   | `StateT<S, M, A>`          | `Func<S, K<M, (A, S)>>`      | Applies the function to `S`   |
|  [07]   | `WriterT<W, M, A>`         | `W` beside the value         | Returns the value with `W`    |
|  [08]   | `RWST<R, W, S, M, A>`      | `ask`, `tell`, `get`, `put`  | Combines the inner runs       |

- `lift` adds a layer to an evaluated value (`Fin<A>`, `Either<L, A>`, `Validation<Error, A>`, or the inner `K<M, A>`)
- `liftIO` passes an `IO<A>` through every layer to the `IO` at the bottom
- `Run` removes one layer, the host runs the layers from the outside in
- `ValidationT` serves errors that must accumulate inside an effect alone
- Domain wrapper `record Wrapper<A>(StateT<S, IO, A> Inner) : K<Wrapper, A>` gains the stack's capabilities through `Deriving`
- `Deriving.Monad<Wrapper, StateT<S, IO>>` and `Deriving.Stateful<Wrapper, StateT<S, IO>, S>` need `Transform` and `CoTransform` alone
- `Deriving.MonadIO` needs a stack that implements `MonadIO`, `StateT` implements `Maybe.MonadIO` and `Maybe.MonadUnliftIO`
- Wrapper lifts an effect through `CoTransform` over `StateT.liftIO`

## [06]-[COLLECTIONS]

Collection types, their purpose, and their construction:

| [INDEX] | [TYPE]          | [PURPOSE]                       | [CONSTRUCTION]                                 |
| :-----: | :-------------- | :------------------------------ | :--------------------------------------------- |
|  [01]   | `Seq<A>`        | Ordered, memoized               | `Seq(1, 2, 3)`, `toSeq(source)`                |
|  [02]   | `Arr<A>`        | Indexed reads                   | `Array(10, 20, 30)`                            |
|  [03]   | `Lst<A>`        | `Insert`, `RemoveAt`, `SetItem` | `List(1, 2, 3)`                                |
|  [04]   | `Map<K, V>`     | Keyed, ordered by key           | `Map(("b", 2), ("a", 1))`                      |
|  [05]   | `HashMap<K, V>` | Keyed, hashed                   | `HashMap(("a", 1))`, `toHashMap(pairs)`        |
|  [06]   | `Set<A>`        | Unique, ordered                 | `Set(3, 1, 2)`, `toSet(items)`                 |
|  [07]   | `HashSet<A>`    | Unique, hashed                  | `HashSet(3, 1, 2)`                             |
|  [08]   | `Iterable<A>`   | Lazy over `IEnumerable`         | `source.AsIterable()`, `ToSeq()` forces it     |
|  [09]   | `IterableNE<A>` | Non-empty, `Head` is a value    | `IterableNE.create(1, 2, 3)`, `AsIterableNE()` |

- `Seq<A>` reads its source once and memoizes every item, `toSeq` copies an array, list, or collection eagerly
- `Map` and `Filter` on a `Seq` defer until enumeration
- `Iterable<A>` does not memoize and reruns its source on each enumeration
- `AsIterableNE` returns `Option<IterableNE<A>>`, `None` for an empty source
- `Range(from, count)` takes a count, `Range(1, 3)` yields `1, 2, 3`
- Declared type of a hash set is `LanguageExt.HashSet<A>`, the simple name collides with the BCL type

`Fold` folds left to right with a seed and `FoldBack` right to left:
- `FoldWhile` and `FoldUntil` read the state and the next element before each step, both predicates receive a `(State, Value)` tuple
- `FoldWhile` stops when its predicate returns `false`, `FoldUntil` when it returns `true`
- `FoldM` binds each step through a monad and folds right to left, `FoldBackM` folds left to right, both return `K<M, S>` that `.As()` converts
- Seedless `Fold()` combines a monoid and returns nested groups in reverse order
- `Exists` stops at the first match, `ForAll` at the first failure

```csharp
internal static class Folds {
    public static int WhileUnderTen(Seq<int> values) => values.FoldWhile(0, static (sum, x) => sum + x, static pair => pair.State < 10);
    public static Option<string> Joined(Seq<int> values) =>
        values.FoldM("", static (string text, int x) => Some(string.Create(CultureInfo.InvariantCulture, $"{text}{x}"))).As();
}
```

- `Choose` maps each element to an `Option<B>` and keeps the `Some` values in one pass, `Somes` does the same for a `Seq<Option<A>>`
- `Partition` splits by a predicate into a deconstructable tuple
- `Zip` pairs sequences into tuples named `First` and `Second`, its projection overload takes a function
- `Scan` emits the seed first, the result has one more element than the source
- `At(index)`, `Head`, and `Last` are `Option<A>`, `Tail` is empty for an empty source
- Indexed `Map` passes the item first and the index second, `Rev` reverses
- `LanguageExt.List.unfold` runs a state seed until the step returns `None`, the static import binds the simple name `List` to `Prelude.List`
- `Cons` is written `head.Cons(tail)`, the simple name `Cons` binds to the `LanguageExt.Pretty.Cons<A>` type

Forms the compiler rejects:
- Comparing a `Zip` result with a `Seq` of unnamed tuples is ambiguous (CS9342), the expected value declares `First` and `Second`
- `Contains`, `Sum`, and `Average` on a `Seq` are ambiguous with the LINQ extensions (CS0121), membership is `Exists` and a sum is `Fold`
- `Seq<A>.Empty` in expression context fails (CS0119), the simple name `Seq` binds to the `Prelude` function, the empty value is `Seq<A>()`
- `Seq<A>` has no `Sort` instance, sorting is LINQ `Order()` followed by `toSeq`
- `SeqExtensions.Choose<A, B>` takes two type arguments and `Seq<A>` has no instance form, `Choose<int>(Parse)` is CS1061
- `Choose` takes the indexed `Func<int, A, Option<B>>` with the index first, and a two-parameter `delegate` makes the call ambiguous (CS0121)
- `Choose(x => x > 0 ? x : None)` is CS1660, the conditional has no target type, and `Map<Option<int>>(same).Somes()` compiles

## [07]-[LENSES_AND_SHARED_STATE]

`Lens<A, B>.New` takes a getter and a curried setter:
- `Get` reads the focus, `Set` writes a value and `Update` applies a function to it, both return a new `A`
- `lens(outer, inner)` composes lenses into one that focuses on a value in a nested record

```csharp
internal static class Lenses {
    public static readonly Lens<Item, Detail> DetailOf =
        Lens<Item, Detail>.New(static item => item.Detail, static detail => item => item with { Detail = detail });
    public static readonly Lens<Detail, string> CodeOf =
        Lens<Detail, string>.New(static detail => detail.Code, static code => detail => detail with { Code = code });
    public static readonly Lens<Item, string> ItemCode = lens(DetailOf, CodeOf);

    public static Item Recoded(Item item, string code) => ItemCode.Set(code, item);
    public static Item Uppercased(Item item) => ItemCode.Update(static code => code.ToUpperInvariant(), item);
}
```

`Atom<A>` manages one value with compare-and-swap:
- `Swap` returns the new value and reruns its function on conflict, `SwapMaybe` keeps the state on `None` and returns the current value
- `AtomHashMap<K, V>` updates in place, `TryAdd` ignores a present key
- `SwapKey(key, Func<V, V>)` updates a present key, `SwapKey(key, Func<Option<V>, Option<V>>)` inserts, updates, and removes
- `Find` reads, `FindOrAdd` adds a missing value or returns the existing one in one atomic step
- `Ref<A>` updates run inside `atomic(Func<R>)`, which returns the function result from the transaction
- `swap` reads the transactional value
- `commute` applies its function inside the transaction and again at the commit point against the last committed value
- `Isolation.Serialisable` sets serializable isolation
- `TrackingHashMap<K, V>` records each key change in `Changes`, `Snapshot()` clears the log and keeps the entries
- `memo(Func<A, B>)` caches one result per argument, `memo(Func<A>)` returns a `Memo<A>` that runs the thunk once on `Value`
- `memoK` caches the construction of a `K<F, A>` and not its execution, a memoized `IO` is constructed once and runs each time `Value` is read

```csharp
internal static class SharedState {
    public static int Capped(Atom<int> counter, int limit) => counter.SwapMaybe(n => n < limit ? Some(n + 1) : Option<int>.None);
    public static Unit BumpOrStart(AtomHashMap<string, int> registry, string key) => registry.SwapKey(key, static n => n.Map(static v => v + 1) | Some(1));
    public static decimal Move(Ref<decimal> source, Ref<decimal> target, decimal amount) =>
        atomic(() => {
            _ = swap(source, balance => balance - amount);
            return commute(target, balance => balance + amount);
        }, Isolation.Serialisable);
}
```
