# [TRAITS_AND_TRANSFORMERS]

## [01]-[HIGHER_KINDS]

Static abstract interface members let a constraint describe operations that belong to a type rather than an instance, and the recursive constraint `where A : Monoid<A>` makes the implementing type pass its own concrete type to the trait, so generic code calls `A.Empty` and `+` through the type parameter and receives the concrete value, never a boxed interface. `Semigroup<A>` declares the associative `operator +`, `Monoid<A>` adds the identity `Empty`, and a type outside your control (`string`, an integer) cannot implement them retroactively, so a small owned wrapper implements the trait and converts where monoidal behavior is required:

```csharp
internal readonly record struct Total(decimal Value) : Monoid<Total> {
    public static Total Empty => new(0m);
    public static Total operator +(Total left, Total right) => new(left.Value + right.Value);
}

internal static class Monoids {
    public static B FoldMap<A, B>(Seq<A> items, Func<A, B> project) where B : Monoid<B> =>
        items.Fold(B.Empty, static (state, item) => state + project(item));
}
```

The self-typed trait works while every operation stays within one concrete type, and mapping breaks it: `Map` must change the element type while it keeps the surrounding shape, and a trait over `SELF` alone cannot connect the stored element to the input of the mapping function, while putting the element type on the trait fixes the whole result to `SELF`. C# can parameterize the `A` in `Option<A>` and cannot receive `Option` itself as a parameter `F` to form `F<A>`, which is why `Select`, `SelectMany`, `Where`, `GetEnumerator`, and `GetAwaiter` bind to compiler-recognized members and not to one trait. `K<F, A>` is the encoding that answers it: an empty interface with no members, where `F` is the type constructor and `A` the element, so `Map` is `K<F, B> Map<A, B>(Func<A, B> f, K<F, A> ma)` and replaces `A` with `B` without touching `F`.

## [02]-[WITNESSES]

The generic data type is the shape, and a non-generic sibling type, the witness, implements the capability, as a type-class instance does, so behavior stays out of the data representation and the structure crosses serialization and parallel-processing boundaries as data:

```csharp
internal sealed record Outcome<A>(Seq<string> Notes, A Value) : K<Outcome, A>;

internal sealed class Outcome : Functor<Outcome> {
    public static K<Outcome, B> Map<A, B>(Func<A, B> f, K<Outcome, A> ma) =>
        new Outcome<B>(ma.As().Notes, f(ma.As().Value));
}

internal static class OutcomeExtensions {
    public static Outcome<A> As<A>(this K<Outcome, A> ma) => (Outcome<A>)ma;
}
```

The `As` extension keeps the downcast in one place, and the cast relies on an invariant: exactly one concrete type derives from `K<Outcome, A>`, and a second representation for the same `F` and `A` makes the downcast fail on use. Every type that implements `K<F, A>` with a witness `F : Functor<F>` gains the same generic `Map` extension, the result stays `K<F, B>` and composes in that form, and the concrete type is recovered with one `.As()` only where it is needed, because staying in `K<F, A>` avoids repeated casts. Abstraction over type constructors removes the duplication that ordinary generics remove over value types: an operation over two constructors (`T<F<A>>` with `T` traversable and `F` applicative) has no general form in C#, and the encoding `K<T, K<F, A>>` gives it one, so a user-defined traversable or applicative composes with the library types instead of needing a cross-product of specialized functions.

## [03]-[FOLDABLES]

`Foldable<T>` abstracts aggregation over a structure: the structure decides which values participate and in what order, and the caller supplies a seed and a step `Func<S, A, S>`. `Fold` visits values first to last, `FoldBack` visits the same values last to first, the seed returns unchanged when the structure contributes no value, and direction matters whenever the step is not commutative, so `Fold("", (s, x) => $"{s}{x}")` over `a, b, c` yields `"abc"` and `FoldBack` yields `"cba"`. Each shape keeps its meaning: a sequence contributes every element in order, `Some` contributes its value once and `None` none, `Right` contributes its value and `Left` none, and for a zero-element or one-element shape both directions agree.

The witness implements the primitives `FoldWhile` and `FoldBackWhile`, receives the derived operation family as `static virtual` defaults, and overrides only where the representation gives materially better behavior:
- `Count` and `Sum` derive from `Fold` with a zero seed, `ForAll` with `true`, `Exists` with `false`, and `IsEmpty` stays `true` until a value is met, so the seed decides the empty-structure result
- The seedless `Fold` over a `Monoid<A>` element uses `Empty` as the seed and `+` as the step
- A strict fold cannot stop the traversal, so the default `ForAll`, `Exists`, and `IsEmpty` visit the whole structure, and boolean short-circuiting inside the step skips predicate calls without stopping enumeration, while a witness override gives the early exit
- An array-backed witness reads `Count` and `IsEmpty` from the stored length, converts to an enumerable without copying, and runs `Fold` and `FoldBack` as index loops without an intermediate reversal

Generic functions target `Foldable<T>` and still reach the optimized witness members, where an `IEnumerable<A>` view loses the representation facts (a stored length, direct indexing), and a specialization preserves results, traversal direction, empty behavior, and predicate evaluation order.

## [04]-[APPLICATIVES]

`Map` lifts a unary function over a contextual value, and a multi-argument function enters through currying: `Map` supplies the first argument and leaves a unary function inside the context, and when that function returns another contextual value, `Map` wraps it in the outer context and nests (`K<Option, K<Option, int>>` for one step, four layers for `1 * 2 + 3 * 4`). `Apply` combines a contextual function with a contextual argument and keeps one layer, so the difference from `Map` is only the function: `Map` receives `Func<A, B>` and `Apply` receives it inside the same `K<F, ...>` as its argument. Function-first `Map` and `Apply` extensions give the left-to-right form, and the library's multi-argument overloads curry the delegate for the caller:

```csharp
internal static class Independent {
    private static readonly Func<int, int, int> Add = static (left, right) => left + right;

    public static Option<int> Summed(Option<int> left, Option<int> right) => Add.Map(left).Apply(right).As();
    public static IO<string> Joined(IO<string> first, IO<string> second, IO<string> third) =>
        fun((string a, string b, string c) => a + b + c).Map(first).Apply(second).Apply(third).As();
}
```

The equivalent query (`from a in first from b in second select a + b`) needs no currying, and the distinction is evaluation structure: a monadic expression is sequential, each operand arrives in order and a failure stops the rest, while an applicative expression states that its operands do not depend on one another, so an instance can evaluate the branches concurrently. The `IO` `Apply` forks both operands and awaits both, so chained `Apply` calls run every argument computation in parallel and the function runs after all arguments arrive, and because `Map` is called on the plain function only the `IO` arguments fork. The production `IO` represents itself as a DSL, unpacks the underlying tasks, and coordinates them with `Task.WhenAll`. Use applicatives where independence gives a capability (parallel effects, accumulated validation) or where the fluent form is clearer, and the monadic form otherwise.

## [05]-[TRAVERSABLES]

`Map` with an effect-returning function preserves the nesting (`Seq<Option<int>>`), and `Traverse` transforms every value and flips the structures, `(A -> F<B>) -> T<A> -> F<T<B>>`, where `T` is the traversed structure, `F` the effect per value, and the behavior that combines results comes from `F` alone, so a traversal exposes no accumulator. `Traversable<T>` extends `Functor<T>` and `Foldable<T>` with one `Traverse<F, A, B>(Func<A, K<F, B>> f, K<T, A> ta) where F : Applicative<F>`, and one implementation composes with every applicative:

| [INDEX] | [APPLICATIVE] | [BEHAVIOR]                                                    |
| :-----: | :------------ | :------------------------------------------------------------ |
|  [01]   | `Option`      | Any `None` makes the whole result `None`                      |
|  [02]   | `Validation`  | Every failure is collected                                    |
|  [03]   | `IO`          | Element effects overlap, and one failure fails the traversal  |

`Sequence` is `Traverse` with the identity function for input that is already nested, `TraverseM` and `SequenceM` perform the same flip under `Monad<M>` and the type overrides them where it guarantees a different order (`Seq<A>` overrides `TraverseM` for serial evaluation). C# cannot convert a concrete nested value (`Seq<Option<int>>`) to `K<Seq, K<Option, int>>`, so `values.Traverse(x => x).As()` is the working form of `Sequence` on concrete input. A sequence implements `Traverse` as a `foldBack` that starts from `F.Pure(empty)` and applicatively prepends each transformed item, keeping the inner `Seq<B>` concrete during the fold and widening once with an outer `F.Map`, and a type with success and failure cases traverses only the case that holds a value, lifting the other case unchanged with `F.Pure`:

```csharp
internal static class OutcomeTraversal {
    public static K<F, K<Outcome, B>> Traverse<F, A, B>(Func<A, K<F, B>> f, K<Outcome, A> ma) where F : Applicative<F> =>
        F.Map<B, K<Outcome, B>>(value => new Outcome<B>(ma.As().Notes, value), f(ma.As().Value));
}
```

The generic extension returns two nested `K` interfaces, so a traversable type declares member methods `Traverse` and `TraverseM` that return `K<F, Seq<B>>` with only the outer layer abstract, and the caller adds one `.As()` where the outer concrete type is needed, while `Map` and `Apply` stay available on the `K` before that conversion.

## [06]-[MONADS]

A monad is a pattern for sequencing computations in a context, its operation is `Bind : M<A> -> (A -> M<B>) -> M<B>`, and the function receives the value of one contextual computation and returns the next computation in the same context, so the `Bind` implementation is the programmable semicolon that decides what happens between the steps. It answers needs that purity creates: an effect is represented as a value and composed by pure code (`IO<A>` wraps the computation and reads the clock on every run, never at construction), and a later computation can depend on an earlier result while the whole stays one expression that LINQ renders as a line-by-line sequence. `Map` is `Bind` followed by `Pure`, `Apply` derives from `Bind` and `Map`, `Flatten` derives from `Bind` with the identity function and `Bind` derives from `Map` followed by `Flatten`, and a type implements whichever pair is simpler or more efficient. No general operation has the shape `M<A> -> A`, because a context can hold no `A`, and lowering is type-specific through `Match` or a default, so `Bind` keeps the composition lifted and the monad preserves its no-value case. Each monad gives the same `Bind` shape a different between-step behavior:

| [INDEX] | [MONAD]                    | [BEHAVIOR]                                                                 |
| :-----: | :------------------------- | :------------------------------------------------------------------------- |
|  [01]   | `Option<A>`                | Continues from `Some`, preserves `None` without invoking the continuation  |
|  [02]   | `Either<L, R>`, `Fin<A>`   | Continues from `Right` or `Succ`, carries `Left` or `Fail` through         |
|  [03]   | `Validation<F, A>`         | Terminates on `Fail`, and its `Apply` combines two failures with `+`       |
|  [04]   | `Try<A>`                   | Builds another delayed thunk, `Run` moves the exception into `Fin`         |
|  [05]   | `Iterable<A>`, `Seq<A>`    | Nested iteration, an empty collection terminates that branch               |
|  [06]   | `Reader<Env, A>`           | Runs both stages with the same environment                                 |
|  [07]   | `Writer<W, A>`             | Threads the accumulated output to the next stage                           |
|  [08]   | `State<S, A>`              | Runs the next stage with the state the previous one returned               |
|  [09]   | `IO<A>`                    | Runs the deferred computation, then the one the continuation returns       |

The monad in a return type marks the expression with its behavior, `IO<A>` declares that it performs I/O and `Option<A>` that it can produce no value, and a visible context separates effectful from non-effectful code while it preserves composition. Custom monads are ordinary application types with cross-cutting behavior (a database monad that manages connections and I/O, a service monad that manages configuration and third-party access), the trait operations unlock the generic library behavior including LINQ, and the type's supporting functions (`ask`, `tell`, `get`, `put`) form its practical API. These are single-feature monads, `Option` and `IO` do not combine in one expression, and handwritten combined types grow with every pairing, which transformers replace.

## [07]-[LAWS]

`FunctorLaw<F>.validate(fa)`, `ApplicativeLaw<F>.validate()`, and `MonadLaw<F>.validate()` return `Validation<Error, Unit>`, a failed law contributes an accumulated `Error`, and `IsSuccess` reads the outcome. The functor check tests identity and composition on one value, the applicative check runs the functor laws and then identity, composition, homomorphism, and interchange, and the monad check runs the applicative laws, both identities, associativity, and the equivalence of `Monad.recur` with `Bind`. The checks hold for `Option` and `Fin`, and `MonadLaw<IO>.validate()` throws inside the library and is not run:

```csharp
internal static class Laws {
    public static Validation<Error, Unit> OptionFunctor => FunctorLaw<Option>.validate(Some(1));
    public static Validation<Error, Unit> FinApplicative => ApplicativeLaw<Fin>.validate();
    public static Validation<Error, Unit> FinMonad => MonadLaw<Fin>.validate();
}
```

- See `dotnet-coding/references/results.md` for the law equations and property-based tests over generated values

## [08]-[TRANSFORMERS]

`Bind` continues only in the same higher-kinded type, so `Option<A>` computations cannot bind `IO<B>`, and a nested `IO<Option<A>>` compiles while the caller inspects the inner `Option` by hand and reproduces its branching inside `IO`. A transformer packages that nested behavior under the contract `MonadT<T, M> : Monad<T>` with `Lift<A>(K<M, A>) : K<T, A>`, where `T` is itself a monad and stacks inside another transformer, and `OptionT` lifts an `IO` continuation through its LINQ `Bind` and `SelectMany` extensions without an explicit lift:

```csharp
internal static class Lookups {
    public static OptionT<IO, Seq<string>> Lines(string input, Func<string, Option<string>> validatePath, Func<string, IO<Seq<string>>> readLines) =>
        from path in OptionT.lift(validatePath(input))
        from lines in readLines(path)
        select lines;
}
```

A regular monad becomes a transformer only through a bespoke implementation, the reverse holds through `Identity` (`OptionT<Identity, A>` corresponds to `Option<A>`) while the dedicated regular type stays preferable for performance, and transformer types carry the `T` suffix. The known and lifted monads occupy no universal nesting order, `OptionT<M, A>` stores `K<M, Option<A>>` with `M` outside and `ReaderT<Env, M, A>` stores `Func<Env, K<M, A>>`, so the concrete wrapped type and not the suffix decides what each `Run` returns, and a consumer stops after any layer when the partially unwrapped result serves another expression. No `IOT` exists: `IO<A>` is the innermost monad of an `IO` stack, repeated `lift(lift(lift(io)))` exposes the depth, and `liftIO` forwards the action through every transformer to the `IO` layer, where lifting is the identity. C# cannot make a transformer implement a trait only for the `M` that contains `IO`, so `MonadIO<M>` declares that a type supports `IO` and serves as a constraint, every transformer implements it and passes `IO` operations to the lifted `M` unless `IO` is deliberately barred from every stack that uses it, and specialized `Bind` and `SelectMany` extensions let an `IO<B>` continuation appear directly in a `MonadIO<M>` expression:

```csharp
internal static class Stacks {
    public static FinT<IO, Item> Charged(IO<Item> load, Func<Item, Fin<Item>> charge) =>
        from item in FinT.liftIO<IO, Item>(load)
        from charged in FinT.lift<IO, Item>(charge(item))
        select charged;
    public static IO<Item> Settled(FinT<IO, Item> charged) => charged.runFin.As().Bind(static fin => IO.lift(fin));
    public static ReaderT<Settings, OptionT<IO>, int> Priced(Option<int> count, IO<int> unitPrice) =>
        from settings in ReaderT.ask<OptionT<IO>, Settings>()
        from quantity in ReaderT.lift<Settings, OptionT<IO>, int>(OptionT.lift<IO, int>(count))
        from price in ReaderT.liftIO<Settings, OptionT<IO>, int>(unitPrice)
        select quantity * price * settings.Factor;
    public static Fin<Option<int>> Exit(ReaderT<Settings, OptionT<IO>, int> priced, Settings settings) =>
        priced.Run(settings).As().Run().As().RunSafe();
}
```

`Settled` converts the `Fin` from `runFin` into an `IO` result through `IO.lift(Fin<A>)` and keeps a rejection as its typed `Expected`, and `Exit` shows the layer order: `Run(settings)` yields `K<OptionT<IO>, int>`, the second `Run()` yields `K<IO, Option<int>>`, and `RunSafe` yields `Fin<Option<int>>`. Two `ValidationT<Error, IO, A>` values combine with the tuple `Apply`, and both effects run before the errors accumulate.

## [09]-[READERS]

`ReaderT<Env, M, A>` wraps `Func<Env, K<M, A>>`, its `Bind` runs both dependent stages with the same environment and lets `M.Bind` sequence their inner computations, and `ask` retrieves no global state, it builds a function that lifts its eventual input into `M` with `M.Pure`, so everything before `Run` stays lazy. The environment is supplied once when the transformer runs, and the `IO` it returns runs at the edge of the application. `Readable<M, Env>` abstracts environment access away from `Reader`, `ReaderT`, `Eff`, and any wrapper, declares `Asks`, `Ask` (a default over `Asks(identity)`), and `Local`, does not require `Monad`, and its module functions (`Readable.ask`, `asks`, `asksM`, `local`) take the witness as a type argument, so a function requires only the capabilities it uses and stays ignorant of how the monad stores the environment or which other capabilities (I/O, retries, cleanup) its arguments carry:

```csharp
internal sealed record Session(Identity Current, Seq<Right> Rights);
internal sealed record AccessDenied() : Expected("access denied", 9001);

internal static class Access {
    public static K<M, Identity> Current<M>() where M : Readable<M, Session> =>
        Readable.asks<M, Session, Identity>(static session => session.Current);
    public static K<M, Unit> Require<M, R>() where R : Right where M : Readable<M, Session>, Monad<M>, Fallible<M> =>
        Readable.ask<M, Session>().Bind(session =>
            session.Rights.Exists(static right => right is R)
                ? M.Pure(unit)
                : M.Fail<Unit>(new AccessDenied()));
}
```

Running the request computation with `Run(session)` threads the read-only context through the stack, and authorization written once serves every monad that exposes the session through `Readable`. `local(f, ma)` maps `Env` to another `Env` of the same type for `ma` alone and later computation sees the original, `with(f, ma)` maps an outer environment to a different type (`AppConfig -> DbConfig`) so a data layer receives only its configuration, and `with` is not part of `Readable` because `Env` is fixed in the trait, so `Reader.with` and `ReaderT.with` supply it and a wrapper exposes an equivalent where mapping is useful. Any monad lifts into `ReaderT`, lifting `Validation<F, A>` gives validators an environment, and `ReaderT` sits outermost in most stacks so the inner monads reach the environment, as an effective placement and not a mandatory one.

## [10]-[STATE_AND_WRITER]

`StateT<S, M, A>` wraps `Func<S, K<M, (A Value, S State)>>`, differs from `ReaderT` in its return type, and its `Bind` runs the next computation with the state the previous one returned, so an update propagates through every later operation where `Readable.local` alters an environment only for a nested scope. A stack combines the concerns the domain needs without a bespoke type: `StateT` threads the current pool, `OptionT` stops the computation when nothing remains, and `IO` holds randomness and console interaction:

```csharp
internal sealed record Pool(Seq<Item> Items);

internal static class Pools {
    public static StateT<Pool, OptionT<IO>, Pool> Current => StateT.get<OptionT<IO>, Pool>();
    public static StateT<Pool, OptionT<IO>, Unit> Replace(Pool pool) => StateT.put<OptionT<IO>, Pool>(pool);
    public static StateT<Pool, OptionT<IO>, int> Remaining => StateT.gets<OptionT<IO>, Pool, int>(static pool => pool.Items.Count);
    public static StateT<Pool, OptionT<IO>, Item> Take =>
        from pool in Current
        from item in OptionT<IO>.lift(pool.Items.Head)
        from _ in Replace(new Pool(pool.Items.Tail))
        select item;
}
```

`Head` returns `None` for an empty pool, lifting it into `OptionT` stops the computation, so the update runs only when an item exists, and `gets(f)` equals mapping `f` over `get` while a domain-named accessor concentrates knowledge of the state shape. Console operations lift into `IO` and compose in the same query, the `>>` operator expresses `ma.Bind(_ => mb)` when an earlier result is irrelevant, `when(condition, mb)` keeps a conditional step inside the workflow, and a recursive loop over the stack stays stack-safe because `IO` runs it without growing the CLR stack. Removing explicit state arguments also hides which operations modify state and lets application-wide state approach a global variable, so keep state queries and updates small and named, partition the domain rules into pure functions over the state, keep I/O separate from those rules, use `Stateful.local` for a temporary context that is restored afterward and a propagating update for a durable change, and hide a deep stack behind a domain type. With `IO` inside `StateT`, a forked computation (including the parallel branches of `Traverse`) inherits the current state and evolves an independent copy, parents at `0` fork two counters that each reach `10` and stay `0`, so a change comes back only when the fork returns the required value, the parent awaits it, and sets the state explicitly.

`WriterT<W, M, A>` is `StateT` with the threaded value renamed to output and constrained to `Monoid<W>`, and the distinct name declares that the threaded value is accumulated output. A representation that returns `(W Output, A Value)` and combines outputs on every `Bind` wastes work when either output is empty and rebuilds growing immutable outputs on every step, so the threaded representation `Func<W, (W Output, A Value)>` passes the accumulated output forward and `tell` alone combines, `tell(value) = output => (output.Combine(value), unit)`, which needs the `Monoid<W>` constraint only there. The same operation is `StateT.modify<M, W>(output => output.Combine(value))` or `Stateful.modify` on any `Stateful<M, W>`. `RWST<R, W, S, M, A>` wraps `ReaderT<R, WriterT<W, StateT<S, M>>, A>` and implements `MonadT`, `Readable`, `Writable`, and `Stateful` by lifting the behaviors the wrapped types already provide.

## [11]-[DOMAIN_MONADS]

Application code composes the supplied transformers and hides the stack behind a stable type with a domain-focused API, so a workflow reads as a sequence of domain operations and does not change when the private representation changes. Without derivation the wrapper forwards `Map`, `Pure`, `Apply`, `Bind`, the `IO` operations, and the environment operations to the stack, and `Deriving` removes that forwarding: each exposed capability names its deriving trait, and the wrapper supplies `Transform` to unwrap and `CoTransform` to rewrap:

```csharp
internal sealed record Settings(int Factor);
internal sealed record App<A>(ReaderT<Settings, IO, A> Run) : K<App, A>;

internal sealed class App : Deriving.MonadIO<App, ReaderT<Settings, IO>>, Deriving.Readable<App, Settings, ReaderT<Settings, IO>> {
    public static K<ReaderT<Settings, IO>, A> Transform<A>(K<App, A> fa) => fa.As().Run;
    public static K<App, A> CoTransform<A>(K<ReaderT<Settings, IO>, A> fa) => new App<A>(fa.As());
}

internal static class AppExtensions {
    public static App<A> As<A>(this K<App, A> ma) => (App<A>)ma;
    public static IO<A> Run<A>(this K<App, A> ma, Settings settings) => ma.As().Run.Run(settings).As();
}
```

Domain wrappers expose only the operations meaningful to one layer: a `Db<A>` over `StateT<DbEnv, IO, A>` fixes the database state and exposes connection, transaction, read, and write operations, a `Service<A>` over `ReaderT<ServiceEnv, IO, A>` fixes a configuration for external calls, and each implements only its domain-specific compromises (converting a stateful database computation to plain `IO` discards the returned state, valid only where the domain accepts it). A higher-level `Api<A>` over `Free<ApiDsl, A>` joins them with DSL cases for a failure, a `Db<A>` action, and a `Service<A>` action, lifts database work through explicit read-only or read-write operations and service work separately so transactional work stays distinct from sending an external message, and runs through an interpreter where `Pure` completes, `Bind` continues, a failure becomes failed `IO`, and each case runs its hidden stack with its environment, so the interpreter returns `IO<A>`. Nested transformers add lambdas, allocations, and CPU cost, so build from the pieces, hide the stack, prioritize correctness, and when profiling shows the stack is a bottleneck, replace its private implementation with one bespoke monad behind the same domain API.
