# [RASM_API_LANGUAGEEXT]

`LanguageExt.Core` is THE functional substrate — every C# folder's typed error rail, optionality, accumulating validation, effect boundary, and immutable collection vocabulary is this one assembly. `Fin<A>` (`Succ`/`Fail(Error)`) is the corpus result rail; `Validation<F, A>` is the fan-in that ACCUMULATES failures applicatively where `Fin` short-circuits; `Option<A>` is presence; `Try<A>`/`Eff<A>`/`IO<A>` are the deferred/effectful tiers that all land back on `Fin<A>` at `Run()`; `Error` is the monoidal failure vocabulary every rail carries. `Seq`/`Arr`/`HashMap` are the immutable collection carriers and `Atom<A>` the lock-free mutable cell. The v5 trait system (`K<F, A>`, `Applicative<F>`, `Monad<M>`, `Monoid<A>`) is what lets one tuple `.Apply(...)` fan-in and one `.Traverse(...)` work across every rail — a `K<F, A>` result downcasts to its concrete carrier through the per-family `.As()`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `LanguageExt.Core`
- package: `LanguageExt.Core` (5.0.0-beta-77; license MIT)
- assembly: `LanguageExt.Core` (`lib/net10.0`)
- namespace: `LanguageExt`, `LanguageExt.Common` (`Error`), `LanguageExt.Traits` (`K<F, A>` + type classes)
- asset: runtime library (pure managed; `Prelude` is the `using static LanguageExt.Prelude;` constructor vocabulary)
- abi: `Option`/`Seq`/`Arr`/`HashMap`/`Guard` are `readonly struct`; `Fin<A>` and `Validation<F, A>` are abstract class/record hierarchies with sealed case types; `Error` is an abstract record `Monoid<Error>`; every carrier implements `K<Self, A>` so trait extensions apply uniformly
- rail: functional substrate

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: result, validation, and effect rails
- rail: functional substrate

| [INDEX] | [SYMBOL]              | [KIND]                     | [CAPABILITY]                                                                              |
| :-----: | :-------------------- | :------------------------- | :----------------------------------------------------------------------------------------- |
|  [01]   | `Fin<A>`              | abstract class, `K<Fin, A>` | short-circuiting result rail; sealed `Succ(A Value)` / `Fail(Error Error)` cases, `IsSucc`/`IsFail` |
|  [02]   | `Validation<F, A>`    | abstract record, `K<Validation<F>, A>` | ACCUMULATING validation; sealed `Success(A Value)` / `Fail(F Value)` cases — `F` must implement `Monoid<F>`/`Semigroup<F>` (enforced at trait resolution, `Error` qualifies) |
|  [03]   | `Option<A>`           | `readonly struct`, `K<Option, A>`, `Monoid<Option<A>>` | presence; `static readonly Option<A> None`, `IsSome`/`IsNone`, implicit from `A?`          |
|  [04]   | `Either<L, R>`        | rail                        | disjoint union; `Fin<A>` converts through `ToEither()` and implicit-lifts from `Either<Error, A>` |
|  [05]   | `Try<A>`              | `record Try<A>(Func<Fin<A>> runTry)`, `K<Try, A>` | deferred exception-trapping thunk; forced by the `Run()` extension into `Fin<A>`           |
|  [06]   | `Eff<A>` / `Eff<RT, A>` | effect monad              | deferred effect (runtime-free / reader-runtime); `Run()`/`Run(EnvIO)` → `Fin<A>`, `RunUnsafe` throws |
|  [07]   | `IO<A>`               | effect root                 | the terminal effect every `Eff` lowers to; `Run()`/`RunAsync()`, `Fork`, `Timeout`, repeat/retry schedules |
|  [08]   | `Error` (`LanguageExt.Common`) | abstract record, `Monoid<Error>` | failure vocabulary: `Code`/`Message`/`Inner`/`IsExceptional`/`IsExpected`; `+` combines into a many-error carrier walked by `Head`/`Tail`/`Count` |
|  [09]   | `Guard<E, A>`         | `readonly struct`          | predicate gate from `Prelude.guard`; `Flag` + lazy `OnFalse`, lands on a rail via `ToFin()` and LINQ `Bind`/`SelectMany` |
|  [10]   | `Pure<A>` / `Fail<E>` | lift carriers               | rail-agnostic success/failure literals; implicit-convert into `Fin`/`Option`/`Validation`/`Try` |

[PUBLIC_TYPE_SCOPE]: collections and state
- rail: functional substrate

| [INDEX] | [SYMBOL]         | [KIND]                    | [CAPABILITY]                                                                              |
| :-----: | :--------------- | :------------------------ | :----------------------------------------------------------------------------------------- |
|  [01]   | `Seq<A>`         | `readonly struct`, `K<Seq, A>` | immutable cons-friendly sequence; the corpus's default ordered carrier                     |
|  [02]   | `Arr<A>`         | `readonly struct`, `[CollectionBuilder(typeof(Arr), "create")]` | immutable array; collection expressions (`Arr<double> xs = [..source]`) route the builder |
|  [03]   | `HashMap<K, V>`  | `readonly struct`, `IReadOnlyDictionary<K, V>` | immutable hash map; `Find → Option<V>` is the lookup rail                                 |
|  [04]   | `HashSet<A>` / `Set<A>` / `Lst<A>` / `Map<K, V>` / `Stck<A>` | immutable collections | hashed set / ordered set / list / ordered map / stack — `Seq<A>.Concat` accepts each directly |
|  [05]   | `Iterable<A>`    | lazy sequence, `K<Iterable, A>` | the LINQ-shaped lazy carrier; `AsIterable()` lifts any `IEnumerable<A>`/`IAsyncEnumerable<A>` |
|  [06]   | `Atom<A>`        | class                     | lock-free CAS cell; `Value`, `A Swap(Func<A, A>)`, `IO<A> SwapIO(...)`, `Change` event, implicit to `A` |

[PUBLIC_TYPE_SCOPE]: trait system (`LanguageExt.Traits`)
- rail: functional substrate

| [INDEX] | [SYMBOL]                          | [KIND]          | [CAPABILITY]                                                                       |
| :-----: | :--------------------------------- | :-------------- | :---------------------------------------------------------------------------------- |
|  [01]   | `K<F, A>`                          | kind marker     | higher-kinded carrier seam every rail/collection implements; trait extensions target it |
|  [02]   | `Applicative<F>` / `Monad<M>`      | type class      | power the tuple `.Apply` fan-in, `Bind`, and `Traverse<F, B>`/`TraverseM<M, B>`     |
|  [03]   | `Monoid<A>` / `Semigroup<A>`       | type class      | `Error` and `Option<A>` combine under them; `Validation<F, A>` requires them on `F` |
|  [04]   | `Fallible<E, F>`                   | type class      | typed-failure capability; backs `Catch` recovery combinators across rails           |
|  [05]   | `*Extensions.As()` (per family)    | downcast        | `K<Fin, A> → Fin<A>`, `K<Validation<F>, A> → Validation<F, A>`, `K<Try, A> → Try<A>`, `K<Iterable, A> → Iterable<A>` — the one spelling that lands a trait-shaped result on its concrete carrier |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Fin` construction, matching, and conversion
- rail: functional substrate

| [INDEX] | [SURFACE]                                                  | [CALL_SHAPE]     | [CAPABILITY]                                                     |
| :-----: | :---------------------------------------------------------- | :--------------- | :----------------------------------------------------------------- |
|  [01]   | `Fin.Succ<A>(A value)` / `Fin.Fail<A>(Error error)` / `Fail<A>(string)` | static factory | the corpus result constructors                                    |
|  [02]   | `B Match<B>(Func<A, B> Succ, Func<Error, B> Fail)`          | instance         | total fold; `Unit Match(Action<A>, Action<Error>)` side-effect twin |
|  [03]   | `Map<B>(Func<A, B>)` / `MapFail(Func<Error, Error>)`        | instance         | success / failure projection                                       |
|  [04]   | `Bind<B>(Func<A, K<Fin, B>>)` / `BindFail(Func<Error, Fin<A>>)` | instance     | monadic chain / failure recovery chain; `Pure<B>`/`Fail<Error>` lambda overloads |
|  [05]   | `IfFail(Func<Error, A>)` / `IfFail(A)` / `IfSucc(Action<A>)` / `Iter(Action<A>)` | instance | escape hatches and side-effect taps                              |
|  [06]   | `Fold<S>(S, Func<S, A, S>)` / `Exists` / `ForAll`           | instance         | state fold and predicates over the zero-or-one success             |
|  [07]   | `Traverse<F, B>(Func<A, K<F, B>>) where F : Applicative<F>` / `TraverseM<M, B>` | instance | effect distribution — `Seq<Fin<A>>`-shaped work inverts through it |
|  [08]   | `ToOption()` / `ToEither()` / `ToValidation()` / `ToEff()` / `ToSeq()` | instance | rail conversions; `ToValidation()` is the accumulation on-ramp     |
|  [09]   | implicit from `A` / `Error` / `Either<Error, A>` / `Pure<A>` / `Fail<Error>`; `operator \|(Fin<A>, Fin<A>)` | conversion / alternative | literal lifts; `\|` takes the first success                       |

[ENTRYPOINT_SCOPE]: `Option`, `Validation`, and `Error`
- rail: functional substrate

`Validation.Match` binds `Fail` FIRST (`Match<B>(Func<F, B> Fail, Func<A, B> Succ)`) — the mirror image of `Fin.Match`; positional lambdas that assume success-first are the named defect.

| [INDEX] | [SURFACE]                                                     | [CALL_SHAPE]     | [CAPABILITY]                                                     |
| :-----: | :------------------------------------------------------------- | :--------------- | :----------------------------------------------------------------- |
|  [01]   | `Some(value)` / `Optional(value)` / `Option<A>.None`           | prelude / static | presence constructors; `Optional` maps null to `None` (class + nullable-struct overloads) |
|  [02]   | `Option<A>.Match<B>(Func<A, B> Some, Func<B> None)` / `IfNone(A \| Func<A>)` / `Filter(Func<A, bool>)` | instance | fold, default, and predicate narrowing                            |
|  [03]   | `Option<A>.ToFin()` / `ToFin(Error Fail)` / `ToSeq()` / `ToArray()` / `ToEither<L>(...)` | instance | rail lifts; `Optional(x).ToFin(error)` is the corpus null-gate idiom |
|  [04]   | `Validation.Success<F, A>(A)` / `Validation.Fail<F, A>(F)` / `Fail<F, A>(Seq<F>)` | static factory | validation constructors (`where F : Monoid<F>`)                    |
|  [05]   | `Validation<F, A>.Map` / `MapFail<F1>(Func<F, F1>) where F1 : Monoid<F1>` / `Bind<B>` / `operator \|` | instance | projection, failure re-shaping, chain, accumulating alternative    |
|  [06]   | `Validation<F, A>.ToEither()` / `ToOption()`; `ValidationExtensions.ToFin<A>(this Validation<Error, A>)` | instance / extension | exits back to the short-circuit rails                             |
|  [07]   | `Error.New(string)` / `New(int code, string)` / `New(Exception)` / `New(string, Exception)` / `New(int, string, Error inner)` / `New(string, Error inner)` | static factory | the failure constructors                                           |
|  [08]   | `Error.Combine(Error)` / `operator +` / `Head` / `Tail` / `Count` / `IsEmpty` | instance | monoidal accumulation and the many-error walk                      |
|  [09]   | `Error.Is(Error)` / `IsType<E>()` / `HasCode(int)` / `HasException<E>()` / `Filter<E>()` / `ToException()` | instance | failure classification and exception egress                        |

[ENTRYPOINT_SCOPE]: `Try`, `Eff`, `IO` — the deferred tiers
- rail: functional substrate

| [INDEX] | [SURFACE]                                                     | [CALL_SHAPE]     | [CAPABILITY]                                                     |
| :-----: | :------------------------------------------------------------- | :--------------- | :----------------------------------------------------------------- |
|  [01]   | `Try.lift<A>(Func<A>)` / `lift(Func<Fin<A>>)` / `lift(Action)` / `Try.Succ` / `Try.Fail` | static factory | exception-trapping thunk over host/native calls                    |
|  [02]   | `TryExtensions.Run<A>(this K<Try, A>)`                         | extension        | forces the thunk → `Fin<A>`; `Try.lift(() => host()).Run()` is the boundary idiom |
|  [03]   | `Eff.lift<A>(Func<A> \| Func<Fin<A>> \| Func<Either<Error, A>> \| Func<Task<A>> \| Func<Task<Fin<A>>> \| IO<A>)` / `Prelude.liftEff` | static factory | effect admission from every sync/async shape                       |
|  [04]   | `EffExtensions.Run<A>(this K<Eff, A>[, EnvIO])` / `RunUnsafe`  | extension        | executes → `Fin<A>`; `RunUnsafe` throws the `Error`                |
|  [05]   | `Eff<A>.Map` / `MapFail` / `MapIO` / `Bind` / `IfFail(Func<Error, A>)` / `IfFailEff` | instance | effect composition and typed recovery                              |
|  [06]   | `IO.pure<A>(A)` / `IO.fail<A>(Error \| string)` / `IO.lift(...)` / `IO.liftAsync(Func<Task<A>>)` | static factory | terminal-effect constructors                                       |
|  [07]   | `IO<A>.Run([EnvIO])` / `RunAsync([EnvIO])`                     | instance         | synchronous / `ValueTask<A>` execution                             |
|  [08]   | `IO<A>.Fork(Option<TimeSpan>) → IO<ForkIO<A>>` / `Timeout(TimeSpan)` / `RepeatUntil(...)` / `RetryUntil(...)` (+ `Schedule` overloads) | instance | structured concurrency, deadline, and schedule-driven repetition   |

[ENTRYPOINT_SCOPE]: `Seq` — construction, transform, and the INDEXED-MAP argument-order law
- rail: functional substrate

The instance and module indexed maps take their lambda arguments in OPPOSITE order: `Seq<A>.Map<B>(Func<A, int, B> f)` is `(value, index)`; the module `Seq.map<A, B>(Seq<A> list, Func<int, A, B> map)` is `(index, value)`. Kernel pages compose the INSTANCE form — `points.Map((p, i) => ...)` — so a mechanical rewrite to the module spelling silently transposes the arguments.

| [INDEX] | [SURFACE]                                                     | [CALL_SHAPE]     | [CAPABILITY]                                                     |
| :-----: | :------------------------------------------------------------- | :--------------- | :----------------------------------------------------------------- |
|  [01]   | `Prelude.Seq(a, b, …)` / `Seq(ReadOnlySpan<A>)` / `Seq(IEnumerable<A>)` / `toSeq(...)` | prelude factory | sequence construction (arities 0–8 + params tail + span + enumerable) |
|  [02]   | `Seq<A>.Map<B>(Func<A, B> f)`                                  | instance         | elementwise projection                                             |
|  [03]   | `Seq<A>.Map<B>(Func<A, int, B> f)`                             | instance         | INDEXED projection — `(value, index)`; the kernel's enumerate idiom |
|  [04]   | `Seq.map<A, B>(Seq<A>, Func<A, B>)` / `Seq.map<A, B>(Seq<A>, Func<int, A, B>)` | module | module twins; the indexed module form is `(index, value)`          |
|  [05]   | `Add(A)` / `Concat(Seq<A> \| IEnumerable<A> \| ReadOnlySpan<A> \| Lst \| Set \| HashSet \| Stck)` | instance | append and cross-collection concatenation                          |
|  [06]   | `Head` / `Last` (`Option<A>`) / `Tail` / `Init` / `this[Index]` / `Count` / `IsEmpty` | instance | structural reads; `Head`/`Last` are `Option`-safe                  |
|  [07]   | `Bind<B>` / `Filter(Func<A, bool>)` / `Fold` / `ForAll` / `Exists` / `Distinct` / `Traverse` | instance | monadic and predicate combinators                                  |
|  [08]   | `FoldM<M, S>(S initialState, Func<S, A, K<M, S>> f) where M : Monad<M>` (curried twin `Func<A, Func<S, K<M, S>>>`) | foldable extension | the MONADIC state fold — each step yields `K<M, S>` (an `IO<S>` step chain short-circuits on the first failure); the effectful-accumulator law for verify/restore choreographies (`AttestedLedger.Verify`, `PointInTimeRestore.Run`) |

[ENTRYPOINT_SCOPE]: `Arr`, `HashMap`, `Atom`, `Iterable`, prelude gates, and the applicative fan-in
- rail: functional substrate

| [INDEX] | [SURFACE]                                                     | [CALL_SHAPE]     | [CAPABILITY]                                                     |
| :-----: | :------------------------------------------------------------- | :--------------- | :----------------------------------------------------------------- |
|  [01]   | `Arr.create<T>(params T[] \| ReadOnlySpan<T>)` / `createRange(IEnumerable<T>)` / `empty` / `singleton`; collection expressions | static factory | immutable-array construction; `[.. source]` routes `create`        |
|  [02]   | `HashMap<K, V>.Find(K) → Option<V>` / `Find(K, Some, None)`    | instance         | the lookup rail — never the throwing `this[K]` on a miss           |
|  [03]   | `HashMap<K, V>.Add` / `AddOrUpdate(K, V \| K, Some, None)` / `SetItem` / `Remove` / `Filter` / `Map` | instance | persistent-map edits returning new maps                            |
|  [04]   | `Prelude.Atom<A>(A value)` → `Atom<A>.Swap(Func<A, A>)` / `SwapIO` / `Value` / `Change` | prelude / instance | lock-free shared state; `Swap` is the ONLY mutation spelling       |
|  [05]   | `IterableExtensions.AsIterable<A>(this IEnumerable<A> \| IAsyncEnumerable<A>)` | extension | lazy lift; `xs.AsIterable().ToSeq()` is the materialization idiom  |
|  [06]   | `Prelude.guard(bool flag, Error \| Func<Error> False) → Guard<Error, Unit>` | prelude | predicate gate; `FinGuardExtensions.ToFin(this Guard<Error, Unit>)` lands it on the rail |
|  [07]   | `(fa, fb).Apply(Func<A, B, R> f)` — `ApplicativeExtensions`, tuple arities 2–10, `where Fnctr : Applicative<Fnctr>` | extension | the accumulating fan-in: over `Validation` EVERY failed component reports; over `Fin` it short-circuits |
|  [08]   | `Prelude.Some` / `None` / `unit` / `identity`                  | prelude          | literal vocabulary (`None` is a `Fail<Unit>` that implicit-converts into `Option<A>.None`) |

## [04]-[IMPLEMENTATION_LAW]

[RAIL_TOPOLOGY]:
- one result rail: domain operations return `Fin<A>`; `Fin.Succ`/`Fin.Fail` are the only success/failure spellings, and `Error` (usually a domain `Fault` record deriving it) is the only failure payload
- accumulation is a mode switch, not a second rail: independent gates lift `.ToValidation()`, fan in through the tuple `.Apply(...)`, and exit `.ToFin()` — `Validation<Error, A>` exists exactly between those two conversions
- deferral tiers: `Try.lift(...).Run()` traps a throwing boundary into `Fin<A>` synchronously; `Eff`/`IO` defer the same shape for composed/async execution and land on `Fin<A>` at `Run()` — the tier is chosen by WHEN the effect runs, never by which failure type it carries
- `Guard<Error, Unit>` is the predicate form: `guard(condition, error).ToFin()` replaces `condition ? Succ : Fail` ternaries at admission gates
- `Match` argument order is per-rail law: `Fin.Match(Succ, Fail)` but `Validation.Match(Fail, Succ)` — named lambdas (`Succ:`, `Fail:`) make the transposition impossible

[COLLECTION_TOPOLOGY]:
- `Seq<A>` is the default ordered carrier crossing rail seams (`Fin<Seq<A>>`); `Arr<A>` is the indexed/random-access carrier built by collection expressions; `Iterable<A>` is the lazy LINQ seam that materializes via `ToSeq()`
- lookups are `Option`-shaped: `HashMap.Find`, `Seq.Head`/`Last` — an indexer or `First()` that can throw is the deleted form
- the indexed enumerate is the INSTANCE `Map((value, index) => ...)`; module `Seq.map` transposes to `(index, value)` and is not interchangeable
- `Atom<A>.Swap` owns shared mutable state — a `lock` block around an immutable-collection swap re-implements it

[TRAIT_TOPOLOGY]:
- `K<F, A>` is the seam that makes `Apply`/`Traverse`/`Bind` rail-generic; concrete code re-lands with the family `.As()` immediately — trait-shaped values do not travel through domain signatures
- `Traverse` inverts effect and shape (`Seq<Fin<A>> → Fin<Seq<A>>` via `Traverse(identity)`-shaped calls); a hand-rolled loop that folds a result list with early-exit re-implements it
- `Error : Monoid<Error>` is why `Validation<Error, A>` accumulates: `+`/`Combine` join failures into one carrier that `Head`/`Tail`/`Count` re-enumerate

[STACK]:
- boundary trap → rail: `Try.lift(() => hostCall()).Run()` is the one spelling that moves a throwing host/native call onto `Fin<A>`; catching and re-wrapping by hand is the deleted form
- null-gate: `Optional(candidate).ToFin(error)` admits a nullable in one expression — `candidate is null ? Fin.Fail... : Fin.Succ...` is its re-derivation
- accumulate-then-exit: `(gateA.ToValidation(), gateB.ToValidation()).Apply(static (a, b) => shape(a, b)).As().ToFin()` reports every failed gate in one verdict, then rejoins the short-circuit rail
- enumerate-and-lift: `source.AsIterable().ToSeq().Traverse(item => Gate(item)).As()` gates every element and inverts to `Fin<Seq<A>>` — index-aware bodies use the instance `Map((value, index) => ...)` before the traverse
- alternatives: `Fin`'s `operator |` takes the first success (fallback chains); `Validation`'s `operator |` accumulates — pick by whether the failures must survive

[LOCAL_ADMISSION]:
- Rails, collections, and traits are composed directly — no local `Result<T>`/`Maybe<T>`/`Either` re-mints, no wrapper that renames `Fin` members
- Domain failure types derive `Error` (record inheritance) so they ride `Fin`/`Validation` natively; a parallel exception hierarchy beside the rail is the deleted form
- `using static LanguageExt.Prelude;` is assumed in rail code — `Some`/`None`/`Optional`/`guard`/`Seq`/`toSeq`/`unit`/`Atom` are unqualified vocabulary

[RAIL_LAW]:
- Package: `LanguageExt.Core`
- Owns: the typed error rail (`Fin`), accumulating validation (`Validation`), presence (`Option`), deferred/effectful execution (`Try`/`Eff`/`IO`), failure vocabulary (`Error`), immutable collections (`Seq`/`Arr`/`HashMap`/`Set`/`Lst`/`Iterable`), lock-free state (`Atom`), and the trait system that unifies them
- Accept: `Fin<A>`-returning domain operations, `Validation` fan-ins that exit `.ToFin()`, `Try.lift(...).Run()` boundary traps, `Seq`/`Arr` carriers across seams, `Option`-shaped lookups, `guard(...).ToFin()` admission gates
- Reject: a local result/option/either re-mint, exception-style domain control flow beside the rail, a throwing lookup where `Find`/`Head` give `Option`, a `lock`ed mutable cell beside `Atom.Swap`, a phantom single-type-argument trait call where the family `.As()` is required, the module `Seq.map` spelling where the instance indexed `Map((value, index) => ...)` is composed
