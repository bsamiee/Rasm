# [RASM_API_LANGUAGEEXT]

`LanguageExt.Core` is THE functional substrate — every C# folder's typed error rail, optionality, accumulating validation, effect boundary, and immutable collection vocabulary is this one assembly. `Fin<A>` (`Succ`/`Fail(Error)`) is the corpus result rail; `Validation<F, A>` is the fan-in that ACCUMULATES failures applicatively where `Fin` short-circuits; `Option<A>` is presence; `Try<A>`/`Eff<A>`/`IO<A>` are the deferred/effectful tiers that all land back on `Fin<A>` at `Run()`; `Error` is the monoidal failure vocabulary every rail carries. `Seq`/`Arr`/`HashMap` are the immutable collection carriers and `Atom<A>` the lock-free mutable cell. The v5 trait system (`K<F, A>`, `Applicative<F>`, `Monad<M>`, `Monoid<A>`) is what lets one tuple `.Apply(...)` fan-in and one `.Traverse(...)` work across every rail — a `K<F, A>` result downcasts to its concrete carrier through the per-family `.As()`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `LanguageExt.Core`
- package: `LanguageExt.Core` (; license MIT)
- assembly: `LanguageExt.Core` (`lib/net10.0`)
- namespace: `LanguageExt`, `LanguageExt.Common` (`Error`), `LanguageExt.Traits` (`K<F, A>` + type classes)
- asset: runtime library (pure managed; `Prelude` is the `using static LanguageExt.Prelude;` constructor vocabulary)
- abi: `Option`/`Seq`/`Arr`/`HashMap`/`Guard` are `readonly struct`; `Fin<A>` and `Validation<F, A>` are abstract class/record hierarchies with sealed case types; `Error` is an abstract record `Monoid<Error>`; every carrier implements `K<Self, A>` so trait extensions apply uniformly
- rail: functional substrate

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: result, validation, and effect rails
- rail: functional substrate

| [INDEX] | [SYMBOL]                       | [SHAPE]                         | [CAPABILITY]                                   |
| :-----: | :----------------------------- | :------------------------------ | :--------------------------------------------- |
|  [01]   | `Fin<A>`                       | abstract `K<Fin, A>`            | short-circuiting `Succ`/`Fail` result          |
|  [02]   | `Validation<F, A>`             | abstract `K<Validation<F>, A>`  | accumulating `Success`/`Fail` validation       |
|  [03]   | `Option<A>`                    | readonly `K<Option, A>` monoid  | `None`/`Some` presence and nullable lift       |
|  [04]   | `Either<L, R>`                 | disjoint-union rail             | `Fin` conversion and `Either<Error, A>` lift   |
|  [05]   | `Try<A>`                       | `Func<Fin<A>>`, `K<Try, A>`     | deferred exception trap forced by `Run`        |
|  [06]   | `Eff<A>` / `Eff<RT, A>`        | effect monad                    | runtime-free or reader-runtime deferred effect |
|  [07]   | `IO<A>`                        | terminal effect                 | run, fork, timeout, repeat, and retry          |
|  [08]   | `Error` (`LanguageExt.Common`) | abstract `Monoid<Error>` record | composable typed-failure vocabulary            |
|  [09]   | `Guard<E, A>`                  | readonly predicate gate         | lazy refusal lifted through `ToFin`            |
|  [10]   | `Pure<A>` / `Fail<E>`          | polymorphic lift carriers       | rail-agnostic success and failure literals     |

[PUBLIC_TYPE_SCOPE]: collections and state
- rail: functional substrate

| [INDEX] | [SYMBOL]                                   | [SHAPE]                         | [CAPABILITY]                        |
| :-----: | :----------------------------------------- | :------------------------------ | :---------------------------------- |
|  [01]   | `Seq<A>`                                   | readonly `K<Seq, A>`            | default immutable ordered carrier   |
|  [02]   | `Arr<A>`                                   | readonly collection-built array | indexed immutable carrier           |
|  [03]   | `HashMap<K, V>`                            | readonly dictionary             | `Find → Option<V>` lookup rail      |
|  [04]   | `HashSet` / `Set` / `Lst` / `Map` / `Stck` | immutable collections           | direct `Seq.Concat` sources         |
|  [05]   | `Iterable<A>`                              | lazy `K<Iterable, A>`           | sync or async enumerable lift       |
|  [06]   | `Atom<A>`                                  | lock-free CAS cell              | `Value`, `Swap`, `SwapIO`, `Change` |

[PUBLIC_TYPE_SCOPE]: trait system (`LanguageExt.Traits`)
- rail: functional substrate

| [INDEX] | [SYMBOL]                      | [KIND]      | [CAPABILITY]                             |
| :-----: | :---------------------------- | :---------- | :--------------------------------------- |
|  [01]   | `K<F, A>`                     | kind marker | common rail and collection seam          |
|  [02]   | `Applicative<F>` / `Monad<M>` | type class  | `Apply`, `Bind`, and traversal           |
|  [03]   | `Monoid<A>` / `Semigroup<A>`  | type class  | validation and failure accumulation      |
|  [04]   | `Fallible<E, F>`              | type class  | typed-failure recovery                   |
|  [05]   | `*Extensions.As()`            | downcast    | family-specific concrete-carrier landing |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Fin` construction, matching, and conversion
- rail: functional substrate

| [INDEX] | [SURFACE]                                          | [SHAPE]    | [CAPABILITY]                  |
| :-----: | :------------------------------------------------- | :--------- | :---------------------------- |
|  [01]   | `Fin.Succ<A>` / `Fin.Fail<A>` / `Fail<A>(string)`  | factory    | canonical result construction |
|  [02]   | `Match<B>(Succ, Fail)` / `Match(Action, Action)`   | instance   | total value or effect fold    |
|  [03]   | `Map<B>` / `MapFail`                               | instance   | success or failure projection |
|  [04]   | `Bind<B>` / `BindFail`                             | instance   | monadic chain or recovery     |
|  [05]   | `IfFail` / `IfSucc` / `Iter`                       | instance   | escape and effect taps        |
|  [06]   | `Fold<S>` / `Exists` / `ForAll`                    | instance   | result fold and predicates    |
|  [07]   | `Traverse<F, B>` / `TraverseM<M, B>`               | instance   | effect distribution           |
|  [08]   | `ToOption` / `ToEither` / `ToValidation` / `ToEff` | instance   | rail conversion               |
|  [09]   | implicit lifts / `operator \|`                     | conversion | first-success alternative     |

[ENTRYPOINT_SCOPE]: `Option`, `Validation`, and `Error`
- rail: functional substrate

`Validation.Match` binds `Fail` FIRST (`Match<B>(Func<F, B> Fail, Func<A, B> Succ)`) — the mirror image of `Fin.Match`; positional lambdas that assume success-first are the named defect.

| [INDEX] | [SURFACE]                                             | [SHAPE]    | [CAPABILITY]                 |
| :-----: | :---------------------------------------------------- | :--------- | :--------------------------- |
|  [01]   | `Some` / `Optional` / `Option<A>.None`                | factory    | nullable-aware presence      |
|  [02]   | `Option.Match` / `IfNone` / `Filter`                  | instance   | fold, default, and narrowing |
|  [03]   | `Option.ToFin` / `ToSeq` / `ToArray` / `ToEither`     | instance   | presence egress              |
|  [04]   | `Validation.Success` / `Validation.Fail`              | factory    | monoidal validation creation |
|  [05]   | `Validation.Map` / `MapFail` / `Bind` / `operator \|` | instance   | projection and accumulation  |
|  [06]   | `Validation.ToEither` / `ToOption` / `ToFin`          | conversion | short-circuit egress         |
|  [07]   | `Error.New` overload family                           | factory    | typed-failure construction   |
|  [08]   | `Error.Combine` / `+` / `Head` / `Tail` / `Count`     | instance   | accumulation and enumeration |
|  [09]   | `Error.Is` / `IsType` / `HasCode` / `Filter`          | instance   | classification and egress    |

[ENTRYPOINT_SCOPE]: `Try`, `Eff`, `IO` — the deferred tiers
- rail: functional substrate

| [INDEX] | [SURFACE]                                            | [SHAPE]   | [CAPABILITY]                    |
| :-----: | :--------------------------------------------------- | :-------- | :------------------------------ |
|  [01]   | `Try.lift` / `Try.Succ` / `Try.Fail`                 | factory   | exception-trapping thunk        |
|  [02]   | `TryExtensions.Run`                                  | extension | force thunk to `Fin`            |
|  [03]   | `Eff.lift` / `Prelude.liftEff`                       | factory   | sync or async effect admission  |
|  [04]   | `EffExtensions.Run` / `RunUnsafe`                    | extension | typed or throwing execution     |
|  [05]   | `Eff.Map` / `MapFail` / `MapIO` / `Bind`             | instance  | effect composition and recovery |
|  [06]   | `IO.pure` / `IO.fail` / `IO.lift`                    | factory   | terminal-effect construction    |
|  [07]   | `IO.Run` / `RunAsync`                                | instance  | sync or `ValueTask` execution   |
|  [08]   | `IO.Fork` / `Timeout` / `RepeatUntil` / `RetryUntil` | instance  | structured scheduled execution  |

[ENTRYPOINT_SCOPE]: `Seq` — construction, transform, and the INDEXED-MAP argument-order law
- rail: functional substrate

The instance and module indexed maps take their lambda arguments in OPPOSITE order: `Seq<A>.Map<B>(Func<A, int, B> f)` is `(value, index)`; the module `Seq.map<A, B>(Seq<A> list, Func<int, A, B> map)` is `(index, value)`. Kernel pages compose the INSTANCE form — `points.Map((p, i) =>...)` — so a mechanical rewrite to the module spelling silently transposes the arguments.

| [INDEX] | [SURFACE]                                         | [SHAPE]   | [CAPABILITY]                        |
| :-----: | :------------------------------------------------ | :-------- | :---------------------------------- |
|  [01]   | `Prelude.Seq` / `Seq(span\|enumerable)` / `toSeq` | factory   | ordered-carrier construction        |
|  [02]   | `Seq.Map(Func<A, B>)`                             | instance  | element projection                  |
|  [03]   | `Seq.Map(Func<A, int, B>)`                        | instance  | indexed `(value, index)` projection |
|  [04]   | `Seq.map` module overloads                        | module    | indexed `(index, value)` twin       |
|  [05]   | `Add` / `Concat`                                  | instance  | cross-collection append             |
|  [06]   | `Head` / `Last` / `Tail` / `Init` / `Count`       | instance  | structural reads                    |
|  [07]   | `Bind` / `Filter` / `Fold` / `Traverse`           | instance  | monadic and predicate composition   |
|  [08]   | `FoldM<M, S>`                                     | extension | monadic state fold                  |
|  [09]   | `Choose<B>`                                       | instance  | one-pass filter-map                 |
|  [10]   | `Zip` / `SeqExtensions.Zip`                       | instance  | tuple or projected pairwise join    |
|  [11]   | no `FindIndex`                                    | absence   | compose indexed `Map` and `Choose`  |

[ENTRYPOINT_SCOPE]: `Arr`, `HashMap`, `Atom`, `Iterable`, prelude gates, and the applicative fan-in
- rail: functional substrate

| [INDEX] | [SURFACE]                                            | [SHAPE]   | [CAPABILITY]                     |
| :-----: | :--------------------------------------------------- | :-------- | :------------------------------- |
|  [01]   | `Arr.create` / `createRange` / `empty` / `singleton` | factory   | immutable-array construction     |
|  [02]   | `HashMap.Find`                                       | instance  | optional lookup                  |
|  [03]   | `HashMap.Add` / `AddOrUpdate` / `SetItem` / `Remove` | instance  | persistent map edits             |
|  [04]   | `Prelude.Atom` / `Atom.Swap` / `SwapIO`              | instance  | lock-free shared state           |
|  [05]   | `AsIterable`                                         | extension | lazy sync or async lift          |
|  [06]   | `Prelude.guard` / `Guard.ToFin`                      | prelude   | predicate admission              |
|  [07]   | tuple `Apply`                                        | extension | applicative fan-in, arities 2–10 |
|  [08]   | `Some` / `None` / `unit` / `identity`                | prelude   | literal vocabulary               |

## [04]-[IMPLEMENTATION_LAW]

[RAIL_TOPOLOGY]:
- one result rail: domain operations return `Fin<A>`; `Fin.Succ`/`Fin.Fail` are the only success/failure spellings, and `Error` (usually a domain `Fault` record deriving it) is the only failure payload
- accumulation is a mode switch, not a second rail: independent gates lift `.ToValidation`, fan in through the tuple `.Apply(...)`, and exit `.ToFin` — `Validation<Error, A>` exists exactly between those two conversions
- deferral tiers: `Try.lift(...).Run()` traps a throwing boundary into `Fin<A>` synchronously; `Eff`/`IO` defer the same shape for composed/async execution and land on `Fin<A>` at `Run()` — the tier is chosen by WHEN the effect runs, never by which failure type it carries
- `Guard<Error, Unit>` is the predicate form: `guard(condition, error).ToFin()` replaces `condition ? Succ: Fail` ternaries at admission gates
- `Match` argument order is per-rail law: `Fin.Match(Succ, Fail)` but `Validation.Match(Fail, Succ)` — named lambdas (`Succ:`, `Fail:`) make the transposition impossible

[COLLECTION_TOPOLOGY]:
- `Seq<A>` is the default ordered carrier crossing rail seams (`Fin<Seq<A>>`); `Arr<A>` is the indexed/random-access carrier built by collection expressions; `Iterable<A>` is the lazy LINQ seam that materializes via `ToSeq()`
- lookups are `Option`-shaped: `HashMap.Find`, `Seq.Head`/`Last` — an indexer or `First()` that can throw is the deleted form
- the indexed enumerate is the INSTANCE `Map((value, index) =>...)`; module `Seq.map` transposes to `(index, value)` and is not interchangeable
- `Atom<A>.Swap` owns shared mutable state — a `lock` block around an immutable-collection swap re-implements it

[TRAIT_TOPOLOGY]:
- `K<F, A>` is the seam that makes `Apply`/`Traverse`/`Bind` rail-generic; concrete code re-lands with the family `.As()` immediately — trait-shaped values do not travel through domain signatures
- `Traverse` inverts effect and shape (`Seq<Fin<A>> → Fin<Seq<A>>` via `Traverse(identity)`-shaped calls); a hand-rolled loop that folds a result list with early-exit re-implements it
- `Error: Monoid<Error>` is why `Validation<Error, A>` accumulates: `+`/`Combine` join failures into one carrier that `Head`/`Tail`/`Count` re-enumerate

[STACK]:
- boundary trap → rail: `Try.lift(() => hostCall()).Run()` is the one spelling that moves a throwing host/native call onto `Fin<A>`; catching and re-wrapping by hand is the deleted form
- null-gate: `Optional(candidate).ToFin(error)` admits a nullable in one expression — `candidate is null ? Fin.Fail...: Fin.Succ...` is its re-derivation
- accumulate-then-exit: `(gateA.ToValidation(), gateB.ToValidation()).Apply(static (a, b) => shape(a, b)).As().ToFin()` reports every failed gate in one verdict, then rejoins the short-circuit rail
- enumerate-and-lift: `source.AsIterable().ToSeq().Traverse(item => Gate(item)).As()` gates every element and inverts to `Fin<Seq<A>>` — index-aware bodies use the instance `Map((value, index) =>...)` before the traverse
- alternatives: `Fin`'s `operator \|` takes the first success (fallback chains); `Validation`'s `operator \|` accumulates — pick by whether the failures must survive

[LOCAL_ADMISSION]:
- Rails, collections, and traits are composed directly — no local `Result<T>`/`Maybe<T>`/`Either` re-mints, no wrapper that renames `Fin` members
- Domain failure types derive `Error` (record inheritance) so they ride `Fin`/`Validation` natively; a parallel exception hierarchy beside the rail is the deleted form
- `using static LanguageExt.Prelude;` is assumed in rail code — `Some`/`None`/`Optional`/`guard`/`Seq`/`toSeq`/`unit`/`Atom` are unqualified vocabulary

[RAIL_LAW]:
- Package: `LanguageExt.Core`
- Owns: the typed error rail (`Fin`), accumulating validation (`Validation`), presence (`Option`), deferred/effectful execution (`Try`/`Eff`/`IO`), failure vocabulary (`Error`), immutable collections (`Seq`/`Arr`/`HashMap`/`Set`/`Lst`/`Iterable`), lock-free state (`Atom`), and the trait system that unifies them
- Accept: `Fin<A>`-returning domain operations, `Validation` fan-ins that exit `.ToFin()`, `Try.lift(...).Run()` boundary traps, `Seq`/`Arr` carriers across seams, `Option`-shaped lookups, `guard(...).ToFin()` admission gates
- Reject: a local result/option/either re-mint, exception-style domain control flow beside the rail, a throwing lookup where `Find`/`Head` give `Option`, a `lock`ed mutable cell beside `Atom.Swap`, a phantom single-type-argument trait call where the family `.As()` is required, the module `Seq.map` spelling where the instance indexed `Map((value, index) =>...)` is composed
