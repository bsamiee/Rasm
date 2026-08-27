# [RASM_API_LANGUAGEEXT]

`LanguageExt.Core` is the branch functional substrate: `Fin<A>` is the result every domain operation returns, and every other carrier — presence, accumulation, deferral, collection, cell, optic — names its conversion onto that result. Its higher-kinded trait system makes one `Apply` fan-in, one `Traverse` inversion, and one operator set work across every carrier, so a new carrier is a trait conformance rather than a new combinator family.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: result, validation, and effect carriers

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]   | [CAPABILITY]                              |
| :-----: | :----------------- | :-------------- | :---------------------------------------- |
|  [01]   | `Fin<A>`           | abstract class  | short-circuiting `Succ`/`Fail` result     |
|  [02]   | `Validation<F, A>` | abstract record | accumulating verdict over `F : Monoid<F>` |
|  [03]   | `Option<A>`        | readonly struct | presence with nullable lift               |
|  [04]   | `Either<L, R>`     | abstract record | disjoint union crossing to `Fin`          |
|  [05]   | `Try<A>`           | record          | `Func<Fin<A>>` exception normalization    |
|  [06]   | `Eff<A>`           | record          | runtime-free deferred effect              |
|  [07]   | `Eff<RT, A>`       | record          | reader-runtime deferred effect            |
|  [08]   | `IO<A>`            | abstract record | terminal effect with bracket and schedule |
|  [09]   | `Error`            | abstract record | `Monoid<Error>` failure vocabulary        |
|  [10]   | `Expected`         | record          | expected failure keyed by `Code`          |
|  [11]   | `Exceptional`      | record          | exception-derived failure                 |
|  [12]   | `ManyErrors`       | sealed record   | accumulated failure carrier               |
|  [13]   | `Guard<E, A>`      | readonly struct | predicate gate composing in a LINQ body   |
|  [14]   | `Pure<A>`          | record struct   | carrier-agnostic success literal          |
|  [15]   | `Fail<E>`          | record struct   | carrier-agnostic failure literal          |
|  [16]   | `CatchM<E, M, A>`  | record struct   | predicate-selected recovery handler       |

[PUBLIC_TYPE_SCOPE]: immutable carriers, state, and optics

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]   | [CAPABILITY]                                            |
| :-----: | :---------------------------- | :-------------- | :------------------------------------------------------ |
|  [01]   | `Seq<A>`                      | readonly struct | default ordered carrier with `AsSpan`                   |
|  [02]   | `Arr<A>`                      | readonly struct | indexed immutable array                                 |
|  [03]   | `Lst<A>`                      | readonly struct | persistent linked list                                  |
|  [04]   | `HashMap<K, V>`               | readonly struct | hashed persistent map                                   |
|  [05]   | `Map<K, V>`                   | readonly struct | ordered persistent map                                  |
|  [06]   | `TrackingHashMap<K, V>`       | readonly struct | map carrying its own change log                         |
|  [07]   | `HashSet<A>`                  | readonly struct | hashed persistent set                                   |
|  [08]   | `Set<A>`                      | readonly struct | ordered persistent set                                  |
|  [09]   | `Stck<A>`                     | readonly struct | persistent LIFO stack                                   |
|  [10]   | `Que<A>`                      | readonly struct | persistent FIFO queue                                   |
|  [11]   | `Iterable<A>`                 | abstract class  | lazy sync or async sequence                             |
|  [12]   | `Atom<A>`                     | sealed class    | lock-free CAS cell with `Change`                        |
|  [13]   | `Atom<M, A>`                  | sealed class    | CAS cell threading construction metadata                |
|  [14]   | `AtomHashMap<K, V>`           | sealed class    | lock-free keyed cell mutating in place                  |
|  [15]   | `HashMapPatch<K, V>`          | sealed class    | one keyed-cell delta: `From`, `To`, `Changes`           |
|  [16]   | `AtomHashMapChangeEvent<K,V>` | delegate        | `AtomHashMap.Change` handler over a patch               |
|  [17]   | `Ref<A>`                      | sealed class    | transactional cell `atomic` commits                     |
|  [18]   | `Memo<A>`                     | class           | resettable memoized thunk                               |
|  [19]   | `Memo<F, A>`                  | class           | memoized `K<F, A>` CONSTRUCTION, not its run            |
|  [20]   | `Lens<A, B>`                  | readonly struct | composable get and immutable set                        |
|  [21]   | `Range<A>`                    | record          | generated bounded sequence, `Range.fromMinMax` mints it |
|  [22]   | `AtomChangedEvent<A>`         | delegate        | `Atom.Change` handler over the new value                |
|  [23]   | `Change<A>`                   | abstract class  | `TrackingHashMap` change-log entry, cases below         |
|  [24]   | `IOptional`                   | interface       | presence surface every `Option<A>` implements           |

- The `LanguageExt.Traits.Domain` axis layer is the algebraic value-trait tier over generated admission, and its inheritance arity is the trap: `Amount`, `VectorSpace`, `Locus`, and `Identifier` all inherit the arity-ONE marker `DomainType<SELF>`, which declares NO members, while the admission and egress pair lives on `DomainType<SELF, REPR>` alone. An owner declaring only its axis therefore compiles with no bridge at all and the omission is silent — every axis owner names BOTH (`: Amount<Offset, double>, DomainType<Offset, double>`), and every constraint consuming the bridge names both too.
- The default `Thinktecture.ValidationError` is not a LanguageExt `Error`, so each `DomainType.From` maps generated admission evidence to `KernelFault.InvalidValue` before returning `Fin<SELF>`; package fault unions never become message-only validation factories.
- `DomainType<SELF, REPR>` declares exactly three members and the middle one is derived: `static abstract Fin<SELF> From(REPR)`, `static virtual SELF FromUnsafe(REPR)` whose default body is `From(repr).ThrowIfFail()`, and the instance `REPR To()`. `FromUnsafe` is the ONLY host-boundary escape the axis publishes, and overriding it is how a hot constructor skips the carrier where the caller already proved the invariant.
- Each axis is a bundle of `System.Numerics` generic-math constraints, not a member set — every one declares zero members of its own and earns its algebra from what it inherits, so an owner satisfies an axis by writing the OPERATORS, not by implementing methods. `Identifier<SELF>` carries `IEquatable<SELF>` and `IEqualityOperators`; `VectorSpace<SELF, SCALAR>` adds `IUnaryNegationOperators`, `IAdditionOperators<SELF,SELF,SELF>`, `ISubtractionOperators<SELF,SELF,SELF>`, `IMultiplyOperators<SELF,SCALAR,SELF>`, and `IDivisionOperators<SELF,SCALAR,SELF>`; `Amount<SELF, SCALAR>` adds `IComparable<SELF>` and `IComparisonOperators` over that.
- `Locus<SELF, DISTANCE, DISTANCE_SCALAR>` is the affine axis and its operator arities are ASYMMETRIC by design: `IAdditionOperators<SELF, DISTANCE, SELF>` (position plus displacement is a position) beside `ISubtractionOperators<SELF, SELF, DISTANCE>` (two positions differ by a displacement), with `IAdditiveIdentity<SELF, SELF>` as the origin and `DISTANCE : Amount<DISTANCE, DISTANCE_SCALAR>` binding the displacement to its own measure axis. A `Locus` therefore cannot be added to a `Locus` and cannot be scaled — the type system carries the affine-versus-vector distinction a `double` triple erases.
- The axis roster is exactly six interfaces: `DomainType` at both arities, `Identifier`, `VectorSpace`, `Amount`, and `Locus`. A `Quantity` axis appears in the package's own prose and in NO metadata — an owner declaring one binds nothing.
- `Change<A>` cases: `NoChange<A>` (`NoChange<A>.Default`, also `Change<A>.None`), `EntryAdded<A>`, `EntryRemoved<A>`, and `EntryMapped<FROM, A>` — the value type sits SECOND, so a mapped entry is matched through the open `EntryMappedFrom<FROM>` and closed `EntryMappedTo<A>` views rather than by naming both arguments. Mints are `Change<A>.Added(value)`/`Removed(oldValue)`/`Mapped<FROM>(oldValue, value)`; the predicate columns `HasChanged`/`HasNoChange`/`HasAdded`/`HasRemoved`/`HasMapped`/`HasMappedFrom<FROM>()` read a case without a type test, and `ToOption()` projects the added-or-mapped-to value. `Change<A> : Monoid<Change<A>>`, so consecutive entries for one key `Combine` into the net change — a removal then an add folds to `Mapped`.

[PUBLIC_TYPE_SCOPE]: traits and monad transformers (`LanguageExt.Traits`)

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY]   | [CAPABILITY]                                     |
| :-----: | :------------------------------- | :-------------- | :----------------------------------------------- |
|  [01]   | `K<F, A>`                        | interface       | higher-kinded interface every carrier implements |
|  [02]   | `Functor<F>`                     | interface       | `Map` conformance                                |
|  [03]   | `Applicative<F>`                 | interface       | `Apply` fan-in conformance                       |
|  [04]   | `Monad<M>`                       | interface       | `Bind` and tail-recursive `Recur`                |
|  [05]   | `MonadIO<M>`                     | interface       | `IO` lifting into a carrier                      |
|  [06]   | `Semigroup<A>`                   | interface       | associative `Combine`                            |
|  [07]   | `Monoid<A>`                      | interface       | `Combine` with an identity                       |
|  [08]   | `Foldable<T>`                    | interface       | fold, search, and aggregate conformance          |
|  [09]   | `Traversable<T>`                 | interface       | effect and shape inversion                       |
|  [10]   | `Alternative<F>`                 | interface       | first-success choice                             |
|  [11]   | `Fallible<E, F>`                 | interface       | typed failure raise and recover                  |
|  [12]   | `Readable<M, Env>`               | interface       | ambient-environment reads                        |
|  [13]   | `Stateful<M, S>`                 | interface       | threaded-state reads and writes                  |
|  [14]   | `Writable<M, W>`                 | interface       | monoidal-output conformance                      |
|  [15]   | `ReaderT<Env, M, A>`             | record          | environment threaded over any `M`                |
|  [16]   | `StateT<S, M, A>`                | record          | state threaded over any `M`                      |
|  [17]   | `Writer<W, A>`                   | record          | monoidal output alone, no inner `M`              |
|  [18]   | `WriterT<W, M, A>`               | record          | monoidal output over any `M`                     |
|  [19]   | `Tell<W>`                        | record          | carrier-agnostic output literal                  |
|  [20]   | `RWST<R, W, S, M, A>`            | record          | reader, writer, and state in one pass            |
|  [21]   | `FinT<M, A>`                     | record          | `Fin` stacked over any `M`                       |
|  [22]   | `OptionT<M, A>`                  | record          | `Option` stacked over any `M`                    |
|  [23]   | `EitherT<L, M, A>`               | record          | `Either` stacked over any `M`                    |
|  [24]   | `ValidationT<F, M, A>`           | record          | `Validation` stacked over any `M`                |
|  [25]   | `Free<F, A>`                     | abstract record | open interpreter over a functor                  |
|  [26]   | `Schedule`                       | abstract record | composable repeat and retry policy               |
|  [27]   | `ScheduleTransformer`            | readonly struct | `Schedule → Schedule`, composing under `+`       |
|  [28]   | `DomainType<SELF>`               | interface       | arity-one axis marker, NO members                |
|  [29]   | `DomainType<SELF, REPR>`         | interface       | `From`/`FromUnsafe` admission, `To()` egress     |
|  [30]   | `Identifier<SELF>`               | interface       | equality-only domain identity                    |
|  [31]   | `VectorSpace<SELF, SCALAR>`      | interface       | additive plus scalar-multiply axis               |
|  [32]   | `Amount<SELF, SCALAR>`           | interface       | ordered vector-space measure axis                |
|  [33]   | `Locus<SELF, DIST, DIST_SCALAR>` | interface       | affine position over a distance axis             |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Fin<A>` construction, fold, and egress

| [INDEX] | [SURFACE]                                        | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :----------------------------------------------- | :------- | :--------------------------------- |
|  [01]   | `Fin.Succ(A)`                                    | static   | success construction               |
|  [02]   | `Fin.Fail(Error)`                                | static   | failure construction               |
|  [03]   | `Fin.Match(Func<A,B>, Func<Error,B>)`            | instance | total value fold, `Succ` first     |
|  [04]   | `Fin.Match(Action<A>, Action<Error>)`            | instance | total effect fold                  |
|  [05]   | `Fin.Map(Func<A,B>)`                             | instance | success projection                 |
|  [06]   | `Fin.MapFail(Func<Error,Error>)`                 | instance | failure projection                 |
|  [07]   | `Fin.BiMap(Func<A,B>, Func<Error,Error>)`        | instance | both-branch projection             |
|  [08]   | `Fin.Bind(Func<A,Fin<B>>)`                       | instance | monadic chain                      |
|  [09]   | `Fin.BindFail(Func<Error,Fin<A>>)`               | instance | failure-branch recovery chain      |
|  [10]   | `Fin.BiBind(Func<A,Fin<B>>, Func<Error,Fin<B>>)` | instance | both-branch chain                  |
|  [11]   | `Fin.SelectMany(Func<A,Guard<Error,Unit>>)`      | instance | `guard` admission inside LINQ      |
|  [12]   | `Fin.IfFail(Func<Error,A>)`                      | instance | failure escape to a value          |
|  [13]   | `Fin.IfSucc(Action<A>)`                          | instance | success effect tap                 |
|  [14]   | `Fin.Iter(Action<A>)`                            | instance | success iteration                  |
|  [15]   | `Fin.Fold(S, Func<S,A,S>)`                       | fold     | success state fold                 |
|  [16]   | `Fin.BiFold(S, Func<S,A,S>, Func<S,Error,S>)`    | fold     | both-branch state fold             |
|  [17]   | `Fin.Exists(Func<A,bool>)`                       | instance | success predicate                  |
|  [18]   | `Fin.ForAll(Func<A,bool>)`                       | instance | total predicate                    |
|  [19]   | `Fin.Traverse(Func<A,K<F,B>>)`                   | instance | applicative effect distribution    |
|  [20]   | `Fin.TraverseM(Func<A,K<M,B>>)`                  | instance | monadic effect distribution        |
|  [21]   | `Fin.ToOption()`                                 | instance | presence egress                    |
|  [22]   | `Fin.ToEither()`                                 | instance | disjoint-union egress              |
|  [23]   | `Fin.ToValidation()`                             | instance | accumulation ingress               |
|  [24]   | `Fin.ToEff()`                                    | instance | effect-tier ingress                |
|  [25]   | `Fin.ToSeq()`                                    | instance | collection egress                  |
|  [26]   | `Fin.ThrowIfFail()`                              | instance | host-boundary unwrap               |
|  [27]   | `FinExtensions.As(K<Fin,A>)`                     | static   | trait-value re-anchor              |
|  [28]   | `FinExtensions.ToFin(Either<Error,A>)`           | static   | `Either` ingress                   |
|  [29]   | `FinExtensions.Partition()`                      | static   | split a `Fin` foldable, no exit    |
|  [30]   | `FinExtensions.Succs()`                          | static   | success branch of a `Fin` foldable |
|  [31]   | `FinExtensions.Fails()`                          | static   | failure branch of a `Fin` foldable |
|  [32]   | `Fin operator \|`                                | operator | first-success alternative          |
|  [33]   | `Fin operator \|` over `CatchM<Error,Fin,A>`     | operator | predicate-selected recovery        |
|  [34]   | `Fin unary operator +`                           | operator | terse `K<Fin, A>` re-anchor        |
|  [35]   | `Fin operator *`                                 | operator | applicative apply                  |
|  [36]   | `Fin operator >>`                                | operator | monadic bind and sequence          |

[ENTRYPOINT_SCOPE]: `Option<A>` presence and `Guard` admission

| [INDEX] | [SURFACE]                                          | [SHAPE]   | [CAPABILITY]                     |
| :-----: | :------------------------------------------------- | :-------- | :------------------------------- |
|  [01]   | `Prelude.Some(A)`                                  | static    | present-value construction       |
|  [02]   | `Prelude.Optional(A?)`                             | static    | nullable-aware admission         |
|  [03]   | `Option<A>.None`                                   | property  | absent literal                   |
|  [04]   | `Option.Match(Func<A,B>, Func<B>)`                 | instance  | total presence fold              |
|  [05]   | `Option.IfNone(A)`                                 | instance  | default escape                   |
|  [06]   | `Option.Filter(Func<A,bool>)`                      | instance  | predicate narrowing              |
|  [07]   | `Option.Bind(Func<A,Option<B>>)`                   | instance  | monadic chain                    |
|  [08]   | `Option.ToFin(Error)`                              | instance  | carrier ingress                  |
|  [09]   | `Option.ToValidation(L)`                           | instance  | accumulation ingress             |
|  [10]   | `Option.ToSeq()`                                   | instance  | collection egress                |
|  [11]   | `Option.ToEither(L)`                               | instance  | disjoint-union egress            |
|  [12]   | `Option.TraverseM(Func<A,K<M,B>>)`                 | instance  | absence-total effect inversion   |
|  [13]   | `OptionExtensions.Somes(Seq<Option<A>>)`           | static    | drop absent members in one pass  |
|  [14]   | `Prelude.guard(bool, Error)`                       | static    | predicate refusal literal        |
|  [15]   | `FinGuardExtensions.ToFin(Guard<Error,Unit>)`      | static    | standalone gate to the carrier   |
|  [16]   | `FinGuardExtensions.SelectMany(Func<Unit,Fin<B>>)` | static    | gate as a LINQ `from` clause     |
|  [17]   | `IOptional`                                        | interface | non-generic presence read        |
|  [18]   | `IOptional.IsSome` / `IsNone`                      | property  | presence off a BOXED `Option<A>` |
|  [19]   | `Option.Exists(Func<A,bool>)`                      | instance  | predicate over the present arm   |
|  [20]   | `Option.Map<B>(Func<A,B>) -> Option<B>`            | instance  | functor over the present arm     |
|  [21]   | `Option.Iter(Action<A>) -> Unit`                   | instance  | side effect on the present arm   |

[ENTRYPOINT_SCOPE]: `Validation<F, A>` accumulation and the `Error` vocabulary

| [INDEX] | [SURFACE]                                         | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :------------------------------------------------ | :------- | :--------------------------------- |
|  [01]   | `Validation.Success(A)`                           | static   | accepted-verdict construction      |
|  [02]   | `Validation.Fail(F)`                              | static   | refused-verdict construction       |
|  [03]   | `Validation.Match(Func<F,B>, Func<A,B>)`          | instance | total fold, `Fail` first           |
|  [04]   | `Validation.Map(Func<A,B>)`                       | instance | success projection                 |
|  [05]   | `Validation.MapFail(Func<F,F1>)`                  | instance | failure projection                 |
|  [06]   | `Validation.Bind(Func<A,Validation<F,B>>)`        | instance | monadic chain                      |
|  [07]   | `Validation.BiFold(S, Func<S,F,S>, Func<S,A,S>)`  | fold     | both-branch state fold             |
|  [08]   | `Validation.ToOption()`                           | instance | presence egress                    |
|  [09]   | `Validation.ToEither()`                           | instance | disjoint-union egress              |
|  [10]   | `Validation.ToSeq()`                              | instance | collection egress                  |
|  [11]   | `ValidationExtensions.ToFin(Validation<Error,A>)` | static   | short-circuit carrier egress       |
|  [12]   | `ValidationExtensions.As(K<Validation<F>,A>)`     | static   | trait-value re-anchor              |
|  [13]   | `ValidationExtensions.Successes()`                | static   | accepted branch of a roster        |
|  [14]   | `ValidationExtensions.Fails()`                    | static   | refused branch of a roster         |
|  [15]   | `Validation operator \|`                          | operator | failure-accumulating choice        |
|  [16]   | `ApplicativeExtensions.Apply(tuple, Func<A,B,R>)` | static   | K-kinded fan-in, arities 2–10      |
|  [17]   | `Error.New(int, string)`                          | static   | package `Expected` construction    |
|  [18]   | `Error.New(string, Exception)`                    | static   | evidence-preserving capture        |
|  [19]   | `Error.New(Exception)`                            | static   | package error normalization        |
|  [20]   | `Error.Many(Seq<Error>)`                          | static   | accumulated-failure carrier        |
|  [21]   | `Error.Combine(Error)`                            | instance | monoidal failure join              |
|  [22]   | `Error operator +`                                | operator | terse monoidal failure join        |
|  [23]   | `Error.Head`                                      | property | first accumulated failure          |
|  [24]   | `Error.Tail`                                      | property | remaining accumulated failures     |
|  [25]   | `Error.Count`                                     | property | accumulated-failure cardinality    |
|  [26]   | `Error.AsIterable()`                              | instance | accumulated-failure enumeration    |
|  [27]   | `Error.Is(Error)`                                 | instance | failure identity test              |
|  [28]   | `Error.IsType<E>()`                               | instance | failure type test                  |
|  [29]   | `Error.HasCode(int)`                              | instance | failure code test                  |
|  [30]   | `Error.Filter<E>()`                               | instance | failure-subset selection           |
|  [31]   | `Error.Exception`                                 | property | optional exceptional payload       |
|  [32]   | `Error.Inner`                                     | property | optional cause chain               |
|  [33]   | `Error.ToErrorException()`                        | instance | expected-error exception wrapper   |
|  [34]   | `Error.ToException()`                             | instance | host-boundary projection           |
|  [35]   | `Error.Throw<R>()`                                | instance | host-boundary escape               |
|  [36]   | `Errors.Cancelled`                                | static   | token-trip identity, `-2000000001` |
|  [37]   | `Errors.TimedOut`                                 | static   | `Timeout` expiry, `-2000000002`    |
|  [38]   | `Errors.SequenceEmpty`                            | static   | empty-run identity, `-2000000003`  |
|  [39]   | `Errors.Closed`                                   | static   | closed-resource, `-2000000004`     |
|  [40]   | `Errors.ValidationFailed`                         | static   | refused-verdict, `-2000000011`     |
|  [41]   | `Errors.SourceClosed` / `SourceCompleted`         | static   | `-2000000013` / `-2000000012`      |
|  [42]   | `Errors.SinkFull`                                 | static   | back-pressure, `-2000000015`       |
|  [43]   | `Errors.EndOfStream`                              | static   | drain terminus, `-2000000010`      |
|  [44]   | `Errors.Bottom`                                   | static   | unevaluable-expression identity    |
|  [45]   | `Errors.None`                                     | static   | the empty `ManyErrors` identity    |
|  [46]   | `Errors.ParseError(string)`                       | static   | text-admission failure mint        |

`Error.New(string, Exception)` requires an argument statically typed as `Exception`: a derived or generic exception also converts implicitly to `Error`, making the two-argument call ambiguous with `Error.New(string, Error)`, so the capture site widens or casts before calling.

`Errors` seats the package's own failure identities as a CLOSED negative-code block, so `Error.HasCode` and `Error.Is` classify a cooperative token trip apart from a deadline cut, an empty run apart from a refused verdict, and a drained source apart from a closed one; a message match over any of them re-classifies on rephrasing. A domain fault family therefore keeps its own codes clear of the `-2000000001`..`-2000000015` span, and the `Fallible.Catch(int Code, …)` arity below is the recovery form these codes exist to select.

[ENTRYPOINT_SCOPE]: `Fallible<E, F>` — one recovery and roster-partition family binding every failing carrier

| [INDEX] | [SURFACE]                                                | [SHAPE] | [CAPABILITY]                               |
| :-----: | :------------------------------------------------------- | :------ | :----------------------------------------- |
|  [01]   | `Catch(Func<Error, K<F,A>>)`                             | static  | unconditional effectful recovery           |
|  [02]   | `Catch(int Code, Func<Error, K<F,A>>)`                   | static  | code-selected recovery                     |
|  [03]   | `Catch(Error Match, Func<Error, K<F,A>>)`                | static  | identity-selected recovery                 |
|  [04]   | `Catch(Func<Error,bool> Predicate, Func<Error, K<F,A>>)` | static  | predicate-selected recovery                |
|  [05]   | `Catch(Func<Error, Error>)`                              | static  | failure reprojection, still failed         |
|  [06]   | `Catch(Func<Error, A>)`                                  | static  | recovery to a value under `Applicative<F>` |
|  [07]   | `Catch(K<F,A>)` / `Catch(Pure<A>)` / `Catch(A)`          | static  | unconditional alternative                  |
|  [08]   | `Catch(CatchM<Error, F, A>)`                             | static  | a handler VALUE applied to the carrier     |
|  [09]   | `CatchIO(Func<Error, K<IO,A>>)`                          | static  | recovery into `IO` under `MonadIO<M>`      |
|  [10]   | `PartitionFallible(Seq<K<M,A>>)`                         | static  | `K<M, (Seq<Error> Fails, Seq<A> Succs)>`   |
|  [11]   | `PartitionFallible(K<F, K<M,A>>)`                        | static  | the same over any `Foldable<F>`            |
|  [12]   | `Succs(Seq<K<M,A>>)`                                     | static  | `K<M, Seq<A>>`, failures dropped           |
|  [13]   | `Fails(Seq<K<M,A>>)`                                     | static  | `K<M, Seq<Error>>`, successes dropped      |

- `PartitionFallible` is the EFFECTFUL twin of `FinExtensions.Partition`: `Partition` splits an already-settled `Fin` roster, while `PartitionFallible` runs a roster of PENDING effects, refuses to short-circuit, and lands both branches inside one `M`. Its receivers are `Seq`, `Lst`, `Set`, `HashSet`, `Iterable`, `IEnumerable`, and any `K<F, K<M, A>>`, so a fan-out already on a carrier never leaves it to be split. `Succs` and `Fails` are the one-branch projections over the same receivers.
- The result tuple is `(Seq<Error> Fails, Seq<A> Succs)` — FAILS FIRST, the opposite slot order of `Fin.Match(Succ, Fail)` — so a positional destructure across the two reads the branches backwards; both slots are named and the read goes by name.
- `Catch`'s three selector arities are the whole classification vocabulary: `int Code` against an `Errors` identity or a domain fault code, `Error Match` against a value, and `Func<Error, bool>` where neither suffices. Each pairs with a value arm, an `Error` arm, or a `K<F, A>` arm, so a recovery posture composes as a value instead of a `try`/`catch` ladder at the call site.
- `FallibleExtensionsE` carries the same family generalized over the failure type `E` where `FallibleExtensions` fixes it to `Error`, so a carrier failing in a non-`Error` currency reaches the identical operators by naming its own `E`.

[ENTRYPOINT_SCOPE]: `Try`, `Eff`, `IO` — the deferred tiers

| [INDEX] | [SURFACE]                                                                    | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :--------------------------------------------------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `Try.lift(Func<A>)`                                                          | static   | exception-normalizing thunk               |
|  [02]   | `TryExtensions.Run(K<Try,A>)`                                                | static   | force the thunk to `Fin<A>`               |
|  [03]   | `Try.ToFin()`                                                                | instance | carrier conversion                        |
|  [04]   | `Try.ToIO()`                                                                 | instance | terminal-tier conversion                  |
|  [05]   | `Eff.lift(Func<A>)`                                                          | static   | effect admission                          |
|  [06]   | `Prelude.liftEff(Func<Task<Fin<A>>>)`                                        | static   | async fallible effect admission           |
|  [07]   | `Eff.runtime<RT>() -> Eff<RT, RT>`                                           | static   | supplied-runtime reader effect            |
|  [08]   | `Eff.getState<RT>()`                                                         | static   | runtime and `EnvIO` read                  |
|  [09]   | `Eff.local(Func<OuterRT,InnerRT>, Eff<InnerRT,A>)`                           | static   | scoped runtime override                   |
|  [10]   | `Eff.localCancel(Eff<RT,A>)`                                                 | static   | scoped cancellation source                |
|  [11]   | `EffExtensions.Run(K<Eff,A>)`                                                | static   | typed execution to `Fin<A>`               |
|  [12]   | `EffExtensions.RunAsync(K<Eff,A>)`                                           | static   | `Task<Fin<A>>` execution                  |
|  [13]   | `EffExtensions.RunIO(K<Eff,A>)`                                              | static   | lower to the terminal `IO` tier           |
|  [14]   | `Eff.MapFail(Func<Error,Error>)`                                             | instance | failure projection                        |
|  [15]   | `Eff.MapIO(Func<IO<A>,IO<B>>)`                                               | instance | inner-effect projection                   |
|  [16]   | `Eff.IfFailEff(Func<Error,Eff<A>>)`                                          | instance | effectful recovery                        |
|  [17]   | `IO.pure(A)`                                                                 | static   | lifted-value construction                 |
|  [18]   | `IO.fail(Error)`                                                             | static   | failed-effect construction                |
|  [19]   | `IO.lift(Func<A>)`                                                           | static   | thunk admission                           |
|  [20]   | `IO.lift(Func<Fin<A>>)`                                                      | static   | result-typed thunk onto the error channel |
|  [21]   | `IO.lift(Fin<A>)`                                                            | static   | settled result lifted whole               |
|  [22]   | `IO.liftAsync(Func<Task<A>>)`                                                | static   | `Task` thunk admission                    |
|  [23]   | `IO.liftVAsync(Func<ValueTask<A>>)`                                          | static   | `ValueTask` thunk admission               |
|  [24]   | `IO.Run()`                                                                   | instance | synchronous execution                     |
|  [25]   | `IO.RunAsync()`                                                              | instance | `ValueTask` execution                     |
|  [26]   | `IO.Bracket(Func<A,IO<C>>, Func<A,IO<B>>)`                                   | instance | acquire-use-release scope                 |
|  [27]   | `IO.Bracket(Func<A,IO<C>>, Func<Error,IO<C>>, Func<A,IO<B>>)`                | instance | scope with a failure arm                  |
|  [28]   | `IO.Finally(K<IO,X>)`                                                        | instance | unconditional release                     |
|  [29]   | `IO.Repeat()`                                                                | instance | unconditional repetition                  |
|  [30]   | `IO.Repeat(Schedule)`                                                        | instance | policy-driven repetition                  |
|  [31]   | `IO.RepeatWhile(Func<A,bool>)`                                               | instance | state-advancing repetition                |
|  [32]   | `IO.RepeatWhile(Schedule, Func<A,bool>)`                                     | instance | scheduled state-advancing repetition      |
|  [33]   | `IO.RepeatUntil(Func<A,bool>)` / `RepeatUntil(Schedule, Func<A,bool>)`       | instance | predicate-bounded repetition              |
|  [34]   | `IO.Retry()`                                                                 | instance | unconditional retry                       |
|  [35]   | `IO.Retry(Schedule)`                                                         | instance | policy-driven retry                       |
|  [36]   | `IO.RetryWhile(Func<Error,bool>)`                                            | instance | classified retry                          |
|  [37]   | `IO.RetryWhile(Schedule, Func<Error,bool>)`                                  | instance | scheduled classified retry                |
|  [38]   | `IO.RetryUntil(Func<Error,bool>)` / `RetryUntil(Schedule, Func<Error,bool>)` | instance | predicate-bounded retry                   |
|  [39]   | `IO.Fork(Option<TimeSpan>)`                                                  | instance | concurrent execution handle               |
|  [40]   | `IO.Timeout(TimeSpan)`                                                       | instance | bounded execution                         |
|  [41]   | `IO.Catch(Func<Error,bool>, Func<Error,K<IO,A>>)`                            | instance | predicate-selected recovery               |
|  [42]   | `IO.Uninterruptible()`                                                       | instance | cancellation masking                      |
|  [43]   | `Prelude.@catch(Func<Error,bool>, K<M,A>)`                                   | static   | carrier-generic recovery handler          |
|  [44]   | `Prelude.use(Func<A>, Action<A>)`                                            | static   | resource-scoped acquisition               |
|  [45]   | `Prelude.tail(IO<A>)`                                                        | static   | tail-recursion marker for deep binds      |

- `Try.lift(...).Run()` normalizes thrown cancellation and timeout exceptions to package `Expected` identities and expands `AggregateException` into `ManyErrors`; it is a normalization pass, not an evidence-preserving capture boundary.
- `IO.lift` rethrows cancellation during execution, so a token-aware boundary captures before lifting.
- `IO.Bracket` three-arm form: the middle `Catch` arm receives the `Error` ALONE, never the acquired value, so a release keyed to the resource rides the trailing `Fin` arm.
- `IO.lift` overload selection for a `Fin`-returning thunk is silent, not ambiguous: `Func<Fin<A>>` is the more specific candidate, so `IO.lift(() => <Fin<T>>)` resolves to the result-typed row [20] and lands `IO<T>` with the `Fail` folded onto the error channel — NEVER `IO<Fin<T>>`. A body that means to carry the `Fin` as its value spells the type argument (`IO.lift<Fin<T>>(…)`); a downstream `Bind` treating the payload as a `Fin` after the bare spelling is the defect this row forecloses.
- `IO.Fork` spins one DEDICATED `TaskCreationOptions.LongRunning` thread per fork — forked IOs overlap fully before the await (measured: 16×200ms forks complete in ~206ms wall) and the pool imposes NO concurrency bound, so an unbounded fan-out is an unbounded thread count. A fan-out fold chunks its forked width to its own worker budget; one fork per element over an unbounded population is the defect this row forecloses.

[ENTRYPOINT_SCOPE]: `Schedule` — the repeat and retry cadence every `IO.Repeat`/`Retry` overload takes as a value

| [INDEX] | [SURFACE]                                                    | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :----------------------------------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `Schedule.Forever` / `Never` / `Once`                        | static   | the three degenerate policies             |
|  [02]   | `Schedule.spaced(Duration)`                                  | static   | one constant delay, unbounded             |
|  [03]   | `Schedule.linear(Duration seed, double factor)`              | static   | arithmetic growth                         |
|  [04]   | `Schedule.exponential(Duration seed, double factor)`         | static   | geometric growth, factor defaults `2.0`   |
|  [05]   | `Schedule.fibonacci(Duration seed)`                          | static   | Fibonacci growth                          |
|  [06]   | `Schedule.fixedInterval(Duration, Func<DateTime>?)`          | static   | wall-clock cadence net of work time       |
|  [07]   | `Schedule.windowed(Duration, Func<DateTime>?)`               | static   | next window boundary                      |
|  [08]   | `Schedule.upto(Duration max, Func<DateTime>?)`               | static   | run until a wall-clock deadline           |
|  [09]   | `Schedule.secondOfMinute` / `minuteOfHour` / `hourOfDay`     | static   | calendar-aligned cadence                  |
|  [10]   | `Schedule.dayOfWeek(DayOfWeek, Func<DateTime>?)`             | static   | weekly-aligned cadence                    |
|  [11]   | `Schedule.TimeSeries(Seq<Duration>)`                         | static   | a literal delay roster                    |
|  [12]   | `Schedule.recurs(int) -> ScheduleTransformer`                | static   | cap the ATTEMPT count                     |
|  [13]   | `Schedule.repeat(int) -> ScheduleTransformer`                | static   | replay the WHOLE schedule n times         |
|  [14]   | `Schedule.jitter(double factor, Option<int> seed)`           | static   | proportional randomization                |
|  [15]   | `Schedule.jitter(Duration min, Duration max, Option<int>)`   | static   | absolute-window randomization             |
|  [16]   | `Schedule.decorrelate(double factor, Option<int> seed)`      | static   | de-correlated jitter for parallel callers |
|  [17]   | `Schedule.maxDelay(Duration)`                                | static   | per-step ceiling                          |
|  [18]   | `Schedule.maxCumulativeDelay(Duration)`                      | static   | stop once the total budget is spent       |
|  [19]   | `Schedule.resetAfter(Duration)`                              | static   | restart the policy past a cumulative max  |
|  [20]   | `Schedule.intersperse(Schedule)`                             | static   | weave a second policy between steps       |
|  [21]   | `Schedule.Transform(Func<Schedule, Schedule>)`               | static   | mint a transformer from a function        |
|  [22]   | `Schedule.Identity` / `NoDelayOnFirst` / `RepeatForever`     | static   | the built-in transformers                 |
|  [23]   | `Union(Schedule)` / `operator \|`                            | instance | MIN delay, runs while EITHER runs         |
|  [24]   | `Intersect(Schedule)` / `operator &`                         | instance | MAX delay, stops when EITHER stops        |
|  [25]   | `Combine(Schedule)` / `operator +`                           | instance | append the second after the first         |
|  [26]   | `Interleave(Schedule)`                                       | instance | alternate the two step by step            |
|  [27]   | `Take(int)` / `Skip(int)` / `Tail`                           | instance | positional narrowing                      |
|  [28]   | `Filter(Func<Duration,bool>)` / `Where`                      | instance | drop steps by their delay                 |
|  [29]   | `Map(Func<Duration,Duration>)` / `Map(Func<Duration,int,_>)` | instance | reshape each delay, index second          |
|  [30]   | `Prepend(Duration)` / `PrependZero`                          | instance | lead-in step before the policy            |
|  [31]   | `Bind` / `SelectMany`                                        | instance | LINQ composition over the delay series    |
|  [32]   | `Run() -> Iterable<Duration>`                                | instance | realize the delay series                  |
|  [33]   | `ScheduleTransformer.Apply(Schedule)`                        | instance | the one application member                |
|  [34]   | `operator +(ScheduleTransformer, ScheduleTransformer)`       | operator | transformer composition                   |

- `|` and `&` mean DIFFERENT things by operand kind and the spellings look identical: between two `Schedule` values they are union and intersection, but wherever one side is a `ScheduleTransformer` — in EITHER argument order — both operators collapse to `Apply`. `Forever | jitter(0.5)` and `Forever & jitter(0.5)` are therefore the SAME schedule, while `spaced(1s) | spaced(3s)` and `spaced(1s) & spaced(3s)` are not. A transformer never intersects; only a schedule does.
- `ScheduleTransformer` converts implicitly to `Schedule` by applying itself to `Forever`, so `IO.Retry(Schedule.recurs(3))` compiles bare and means `Forever.Take(3)` — exactly three attempts with no delay. A transformer handed to a `Schedule` parameter is never a type error and never a no-op, which is why a transformer meant to CAP an existing policy must be applied to it explicitly rather than passed alone.
- `recurs(n)` caps attempts while `repeat(n)` replays the entire schedule n times, so `exponential(1s) | repeat(3)` runs the backoff series three times over and `exponential(1s) | recurs(3)` truncates it to three steps.
- `maxCumulativeDelay` STOPS the schedule once the accumulated delay crosses its budget while `resetAfter` restarts the policy at that same crossing, so the pair is exhaustion versus renewal over one measurement.
- Every constructor and transformer carries a PascalCase twin (`Spaced`/`spaced`, `Exponential`/`exponential`, `Fibonacci`/`fibonacci`, `Recurs`/`recurs`) over one implementation; the lowercase spelling is the Prelude-style form carrier code uses.
- Each wall-clock constructor takes an optional `Func<DateTime>?` clock, which is the hook a deterministic test drives instead of sleeping through the real cadence.

[ENTRYPOINT_SCOPE]: `FinT<M, A>` — the `Fin`-over-`M` transformer

| [INDEX] | [SURFACE]                               | [SHAPE]     | [CAPABILITY]                                            |
| :-----: | :-------------------------------------- | :---------- | :------------------------------------------------------ |
|  [01]   | `new FinT<M, A>(K<M, Fin<A>> runFin)`   | constructor | the carrier's own `IO<Fin<A>>`-shaped ingress           |
|  [02]   | `FinT.Succ(A)` / `FinT.Fail(Error)`     | static      | settled construction                                    |
|  [03]   | `FinT.lift(Fin<A>)`                     | static      | settled-result ingress                                  |
|  [04]   | `FinT.lift(K<M, A>)`                    | static      | bare-monad ingress                                      |
|  [05]   | `FinT.lift(K<M, Fin<A>>)`               | static      | named twin of the constructor                           |
|  [06]   | `FinT.liftIO(IO<A>)`                    | static      | bare-`IO` ingress under `MonadIO<M>`                    |
|  [07]   | `FinT.liftIO(IO<Fin<A>>)`               | static      | `Fin`-`IO` ingress — the carrier shape, lifted directly |
|  [08]   | `FinT.runFin`                           | property    | `K<M, Fin<A>>` egress the queries end on                |
|  [09]   | `FinT.Bind` / `SelectMany` overload set | instance    | binds `FinT`, `K<FinT<M>,B>`, `K<M,B>`, bare `Fin<B>`   |
|  [10]   | `FinT.Match(Succ, Fail)` / `MapFail`    | instance    | `K<M, B>` fold / failure map                            |

- `FinT.Bind`/`SelectMany` also bind `Pure<B>` and `Fail<Error>`, so a bare `Fin` step inside a `FinT` query needs no lift.

[ENTRYPOINT_SCOPE]: `Writer<W, A>` — monoidal output accumulated beside a value, `W : Monoid<W>` throughout

| [INDEX] | [SURFACE]                                                   | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :---------------------------------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `Writer.pure<W, A>(A)`                                      | static   | value with the empty output               |
|  [02]   | `Writer.tell<W>(W item)`                                    | static   | emit output, value is `Unit`              |
|  [03]   | `Writer.write<W, A>(A value, W item)`                       | static   | emit value and output together            |
|  [04]   | `Writer.listen(Writer<W, A>)`                               | static   | surface the accumulated output as a value |
|  [05]   | `Writer.listens(Func<W, B>, Writer<W, A>)`                  | static   | surface a projection of the output        |
|  [06]   | `Writer.censor(Func<W, W>, Writer<W, A>)`                   | static   | rewrite the output, value untouched       |
|  [07]   | `Writer.pass(Writer<W, (A, Func<W, W>)>)`                   | static   | the step supplies its own output rewriter |
|  [08]   | `Writer.Run() -> (A Value, W Output)`                       | instance | the one egress, total                     |
|  [09]   | `Writer.Listen()` / `Listens(Func<W, B>)` / `Censor`        | instance | the instance twins of rows [04]–[06]      |
|  [10]   | `Writer.Bind` / `SelectMany` / `Map`                        | instance | LINQ composition, outputs `Combine`       |
|  [11]   | `Writer.Write(A, W)` / `Write((A Value, W Output))`         | instance | append output to a running computation    |
|  [12]   | `Tell<W>.ToWriter()` / `ToWriterT<M>()` / `ToWritable<M>()` | instance | the literal landing on each carrier       |
|  [13]   | `WriterT<W, M, A>`                                          | record   | the same accumulation over any `Monad<M>` |

- `W : Monoid<W>` is the entire contract: each bind `Combine`s the two outputs, so the accumulator IS the monoid and no mutable list is threaded beside the computation. A `Seq<A>` output makes the writer an append-only evidence log, and an `Error` output makes it a warning channel that never fails.
- `Writer<W, A>` publishes NO failure arm — it accumulates without refusing, which is exactly what separates it from `Validation<F, A>`: `Validation` accumulates failures and refuses at the fold, `Writer` accumulates evidence and always succeeds. A step needing both stacks them as `WriterT<W, Fin, A>` rather than folding evidence into the failure channel.
- `Run()` is the only exit and it is total; the output is otherwise unreadable mid-computation except through `Listen`/`Listens`, which hand it back as a VALUE so a later step can branch on what has accumulated. `Censor` and `Pass` are the two rewrite forms — `Censor` decides the rewrite from outside, `Pass` lets the step itself carry the rewriter in its value slot.
- `Tell<W>` is the carrier-agnostic output literal beside `Pure<A>` and `Fail<E>`, so a `tell` step binds inside a `Writer`, a `WriterT`, an `RWST`, or any `Writable<M, W>` body without naming which carrier it landed in.

[ENTRYPOINT_SCOPE]: `Seq`, `Arr`, `HashMap`, `Set` — immutable carriers

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]                         |
| :-----: | :----------------------------------------------------------------- | :------- | :----------------------------------- |
|  [01]   | `Prelude.Seq(A, A)`                                                | static   | ordered-carrier construction         |
|  [02]   | `Prelude.toSeq(IEnumerable<A>)`                                    | static   | enumerable admission                 |
|  [03]   | `Seq.Map(Func<A,B>)`                                               | instance | element projection                   |
|  [04]   | `Seq.Map(Func<A,int,B>)`                                           | instance | indexed `(value, index)` projection  |
|  [05]   | `LanguageExt.Seq.map(Seq<A>, Func<int,A,B>)`                       | static   | indexed `(index, value)` twin        |
|  [06]   | `Seq.Bind(Func<A,Seq<B>>)`                                         | instance | monadic expansion                    |
|  [07]   | `Seq.Filter(Func<A,bool>)`                                         | instance | predicate narrowing                  |
|  [08]   | `Seq.Partition(Func<A,bool>)`                                      | instance | one-pass two-way split               |
|  [09]   | `Seq.Exists(Func<A,bool>)`                                         | instance | any-member predicate probe           |
|  [10]   | `Seq.ForAll(Func<A,bool>)`                                         | instance | every-member predicate probe         |
|  [11]   | `SeqExtensions.Choose(Func<A,Option<B>>)`                          | static   | one-pass filter-map                  |
|  [12]   | `SeqExtensions.Choose(Func<int,A,Option<B>>)`                      | static   | indexed one-pass filter-map          |
|  [13]   | `SeqExtensions.Zip(Seq<B>, Func<A,B,C>)`                           | static   | projected pairwise join              |
|  [14]   | `SeqExtensions.Scan(S, Func<S,A,S>)`                               | static   | running-state projection             |
|  [15]   | `Seq.Head`                                                         | property | `Option<A>` first read               |
|  [16]   | `Seq.Last`                                                         | property | `Option<A>` final read               |
|  [17]   | `Seq.Tail`                                                         | property | all but the first member             |
|  [18]   | `Seq.Init`                                                         | property | all but the final member             |
|  [19]   | `Seq.Tails`                                                        | property | every suffix                         |
|  [20]   | `Seq.Inits`                                                        | property | every prefix                         |
|  [21]   | `Seq.Add(A)`                                                       | instance | append one member                    |
|  [22]   | `Seq.Concat(Seq<A>)`                                               | instance | cross-collection join                |
|  [23]   | `Seq.Intersperse(A)`                                               | instance | separator weave                      |
|  [24]   | `Seq.Strict()`                                                     | instance | force a lazily-built sequence        |
|  [25]   | `Seq.AsSpan()`                                                     | instance | zero-copy contiguous read            |
|  [26]   | `Seq.AsIterable()`                                                 | instance | lazy-view lift                       |
|  [27]   | `Seq.Traverse(Func<A,K<F,B>>)`                                     | instance | applicative shape inversion          |
|  [28]   | `Seq.TraverseM(Func<A,K<M,B>>)`                                    | instance | short-circuiting shape inversion     |
|  [29]   | `FoldableExtensions.Fold(S, Func<S,A,S>)`                          | fold     | carrier-generic state fold           |
|  [30]   | `FoldableExtensions.FoldM` / `FoldBackM(S, Func<S,A,K<M,S>>)`      | fold     | monadic fold, tail-to-head / head-to-tail |
|  [31]   | `FoldableExtensions.FoldWhile(S, Func<S,A,S>, Func<(S,A),bool>)`   | fold     | predicate-bounded fold               |
|  [32]   | `FoldableExtensions.FoldMap(Func<A,B>)`                            | fold     | monoidal aggregation                 |
|  [33]   | `FoldableExtensions.Find(Func<A,bool>)`                            | static   | `Option`-shaped search               |
|  [34]   | `FoldableExtensions.FindAll(Func<A,bool>)`                         | static   | every match as a `Seq`               |
|  [35]   | `Arr.create(A[])`                                                  | static   | immutable-array construction         |
|  [36]   | `Arr.createRange(IEnumerable<A>)`                                  | static   | immutable-array admission            |
|  [37]   | `HashMap.Find(K)`                                                  | instance | `Option<V>` lookup                   |
|  [38]   | `HashMap.Find(K, Func<V,R>, Func<R>)`                              | instance | matched lookup fold                  |
|  [39]   | `HashMap.FindOrAdd(K, Func<V>)`                                    | instance | lookup with insert-on-miss           |
|  [40]   | `HashMap.Add(K, V)`                                                | instance | persistent insert                    |
|  [41]   | `HashMap.AddOrUpdate(K, Func<V,V>, Func<V>)`                       | instance | persistent matched upsert            |
|  [42]   | `HashMap.AddOrUpdate(K, Func<V,V>, V)`                             | instance | matched upsert, value fills the miss |
|  [43]   | `HashMap.SetItem(K, V)`                                            | instance | persistent replace                   |
|  [44]   | `HashMap.Remove(K)`                                                | instance | persistent delete                    |
|  [45]   | `HashMap.Union(IEnumerable<(K,V)>, WhenMatched<K,V,V,V>)`          | instance | merge with a collision rule          |
|  [46]   | `HashMap.ContainsKey(K)`                                           | instance | total key membership                 |
|  [47]   | `HashMap.ToTrackingHashMap()`                                      | instance | change-logged map lift               |
|  [48]   | `HashMap.AddOrUpdate(K, V)`                                        | instance | persistent unconditional upsert      |
|  [49]   | `HashMap.AsIterable()`                                             | instance | `(K Key, V Value)` pair carrier      |
|  [50]   | `Set.Add(A)`                                                       | instance | persistent set insertion             |
|  [51]   | `Set.TryAdd(A)`                                                    | instance | insertion tolerating a duplicate     |
|  [52]   | `IterableExtensions.AsIterable(IEnumerable<A>)`                    | static   | lazy sync lift                       |
|  [53]   | `IterableExtensions.AsIterable(IAsyncEnumerable<A>)`               | static   | lazy async lift                      |
|  [54]   | `Iterable<A>.FromSpan(ReadOnlySpan<A>)`                            | static   | `params` span into the carrier       |
|  [55]   | `LanguageExt.List.unfold(S, Func<S,Option<(A,S)>>)`                | static   | state-seeded lazy generation         |
|  [56]   | `Prelude.toSet(IEnumerable<A>)`                                    | static   | ordered-set enumerable admission     |
|  [57]   | `Set(IEnumerable<A>)`                                              | ctor     | ordered-set construction             |
|  [58]   | `Prelude.toHashMap(IEnumerable<(K,V)>)`                            | static   | hashed-map pair admission            |
|  [59]   | `Seq.Iter(Action<A>)`                                              | instance | side-effecting element walk          |
|  [60]   | `Seq.Skip(int)`                                                    | instance | drop a leading run                   |
|  [61]   | `Seq.Take(int)`                                                    | instance | keep a leading run                   |
|  [62]   | `Seq.Count`                                                        | property | materialized member count            |
|  [63]   | `SeqExtensions.Rev(Seq<A>)`                                        | static   | reversed carrier                     |
|  [64]   | `Seq.TakeWhile(Func<A,bool>)`                                      | instance | predicate-bounded leading run        |
|  [65]   | `Seq.TakeWhile(Func<A,int,bool>)`                                  | instance | indexed predicate-bounded run        |
|  [66]   | `FoldableExtensions.FoldWhileM(S, Func<S,A,K<M,S>>, Func<A,bool>)` | fold     | monadic predicate-bounded fold       |
|  [67]   | `FoldableExtensions.FoldUntilM(S, Func<S,A,K<M,S>>, Func<A,bool>)` | fold     | monadic fold to a stop condition     |
|  [68]   | `Prelude.foldWhileM(f, pred, state, ta)`                           | fold     | the argument-flipped module twin     |
|  [69]   | `FoldableExtensions.FoldUntil(S, Func<S,A,S>, Func<(S,A),bool>)`   | fold     | pure fold to a stop condition        |
|  [70]   | `FoldableExtensions.FoldBackWhile` / `FoldBackUntil`               | fold     | the right-to-left bounded twins      |
|  [71]   | `FoldableExtensions.FoldMaybe(S, Func<S,A,Option<S>>) -> S`        | fold     | the folder itself decides the stop   |
|  [72]   | `FoldableExtensions.FoldMapWhileT` / `FoldMapUntilT`               | fold     | bounded monoidal aggregation, nested |
|  [73]   | `FoldableExtensions.FoldT` / `FoldWhileT` / `FoldUntilT`           | fold     | one pass over `K<T, K<U, A>>`        |

- The bounded folds split their predicate arity by carrier and the two do not look different at the call site: the PURE `FoldWhile`/`FoldUntil` take `Func<(S State, A Value), bool>` — the running state AND the element — while the MONADIC `FoldWhileM`/`FoldUntilM` take `Func<A, bool>` over the element ALONE. A state-reading stop condition therefore has no monadic form; it either folds pure and lifts afterwards, or carries the condition into the effect and returns a settled state the next step reads. `foldWhileM` is the same operator with the arguments flipped to `(f, pred, state, ta)`, so a mechanical rewrite between the instance and module spellings silently transposes them.
- The monadic fold pair is DIRECTION-SWAPPED against the pure pair on the landed release: `Fold` walks head-to-tail, but `FoldM` walks TAIL-TO-HEAD (a string-append fold over `[1, 2, 3]` answers `321`) while `FoldBackM` walks head-to-tail (`123`) — so an order-dependent monadic fold over an ascending run (a running parameter renormalization, a prefix-dependent admission) spells `FoldBackM`, and a `FoldM` there silently feeds the step its input reversed.
- `FoldMaybe` is the fold whose STOP lives in the folder rather than beside it: the step answers `Option<S>` and a `None` ends the walk, returning the last committed state. It is the form a search-and-accumulate takes when the decision to continue is the same computation as the accumulation, and it retires the paired `FoldWhile` predicate that would re-derive that decision a second time. Every bounded fold has a `FoldBack*` right-to-left twin and a `*T` twin folding one pass over a nested `K<T, K<U, A>>`, so a foldable of foldables never flattens first.
- `Seq<A>` publishes NO `Option`-returning positional read: its one index member is the throwing `this[Index]`, and neither the type nor `SeqExtensions` carries an `At`, so a bounded positional lookup composes `Skip(n).Head`, which answers `None` past the tail.
- `LanguageExt.List.unfold` runs the state seed until the unfolder answers `None`, returning `IEnumerable<A>`; five overloads cover `Func<S,Option<S>>` (state-only) and one-to-four state slots as a tuple seed. A host walk over a linked native cursor (`node.Next`) is the generation this replaces, so no `while` loop accumulates into a mutable list before `toSeq`.

[ENTRYPOINT_SCOPE]: `TrackingHashMap<K, V>` — the change-logged map (`HashMap.ToTrackingHashMap()` lifts into it). `TrackingHashMap<EqK, K, V>` is the same surface under an explicit `EqK` witness, and the static `TrackingHashMap` module mints one (`empty`/`create`/`createRange`/`singleton` over `(K, V)` tuples, `KeyValuePair`s, or a `ReadOnlySpan`) where no source map exists to lift.

| [INDEX] | [SURFACE]                                                         | [SHAPE]  | [CAPABILITY]                                        |
| :-----: | :---------------------------------------------------------------- | :------- | :-------------------------------------------------- |
|  [01]   | `TrackingHashMap.Changes`                                         | property | `HashMap<K, Change<V>>` log since the last snapshot |
|  [02]   | `TrackingHashMap.Snapshot()`                                      | instance | zeroes the change log, holds the data               |
|  [03]   | `TrackingHashMap.AddOrUpdate(K, V)`                               | instance | logged unconditional upsert                         |
|  [04]   | `TrackingHashMap.AddOrUpdate(K, Func<V,V>, Func<V>)`              | instance | logged matched upsert, `None` mints the miss        |
|  [05]   | `TrackingHashMap.AddOrUpdate(K, Func<V,V>, V)`                    | instance | logged matched upsert, value fills the miss         |
|  [06]   | `TrackingHashMap.AddOrUpdateRange(IEnumerable<(K Key, V Value)>)` | instance | logged bulk upsert                                  |
|  [07]   | `TrackingHashMap.Remove(K)`                                       | instance | logged delete                                       |
|  [08]   | `TrackingHashMap.Find(K)`                                         | instance | `Option<V>` lookup, logs nothing                    |
|  [09]   | `TrackingHashMap.ContainsKey(K)`                                  | instance | total key membership                                |
|  [10]   | `TrackingHashMap.TryGetValue(K, out V)`                           | instance | `IReadOnlyDictionary` probe                         |
|  [11]   | `TrackingHashMap.ToHashMap()`                                     | instance | drops the log, holds the data                       |

[ENTRYPOINT_SCOPE]: state, optics, and the prelude vocabulary

| [INDEX] | [SURFACE]                                | [SHAPE]  | [CAPABILITY]                        |
| :-----: | :--------------------------------------- | :------- | :---------------------------------- |
|  [01]   | `Prelude.Atom(A, Func<A,bool>)`          | static   | validated lock-free cell            |
|  [02]   | `Atom.Value`                             | property | current-state snapshot read         |
|  [03]   | `Atom.ValueIO`                           | property | repeating read on the IO carrier    |
|  [04]   | `Atom.Swap(Func<A,A>) -> A`              | instance | CAS update, post-state return       |
|  [05]   | `Atom.SwapMaybe(Func<A,Option<A>>) -> A` | instance | CAS update, refusal keeps state     |
|  [06]   | `Atom.SwapIO(Func<A,A>)`                 | instance | CAS update on the effect carrier    |
|  [07]   | `Atom.Change`                            | event    | accepted-swap notification          |
|  [08]   | `Prelude.AtomHashMap(HashMap<K,V>)`      | static   | lock-free keyed cell                |
|  [09]   | `Prelude.Ref(A, Func<A,bool>)`           | static   | transactional cell construction     |
|  [10]   | `Prelude.atomic(Func<R>, Isolation)`     | static   | multi-`Ref` transaction             |
|  [11]   | `Prelude.swap(Ref<A>, Func<A,A>)`        | static   | in-transaction update               |
|  [12]   | `Prelude.commute(Ref<A>, Func<A,A>)`     | static   | order-free in-transaction update    |
|  [13]   | `Lens.New(Func<A,B>, Func<B,Func<A,A>>)` | static   | optic construction                  |
|  [14]   | `Lens.Set(B, A)`                         | instance | immutable focused write             |
|  [15]   | `Lens.Update(Func<B,B>, A)`              | instance | immutable focused edit              |
|  [16]   | `Lens.fst<A,B>()`                        | static   | first-slot tuple optic              |
|  [17]   | `Lens.snd<A,B>()`                        | static   | second-slot tuple optic             |
|  [18]   | `Lens.tuple(Lens<A,C>, Lens<B,D>)`       | static   | composed tuple optic                |
|  [19]   | `Seq<A>.headOrNone`                      | property | first-slot optic over a `Seq`       |
|  [20]   | `Seq<A>.lastOrNone`                      | property | final-slot optic over a `Seq`       |
|  [21]   | `Prelude.memo(Func<A,B>)`                | static   | memoized pure function              |
|  [22]   | `Prelude.memo(Func<A>)`                  | static   | memoized nullary thunk              |
|  [23]   | `Prelude.memo(IEnumerable<A>)`           | static   | replay-cached lazy enumeration      |
|  [24]   | `Prelude.memoUnsafe(Func<A,B>)`          | static   | unsynchronized memo table           |
|  [25]   | `Prelude.memoK(Func<K<F,A>>)`            | static   | caches the `K<F,A>` CONSTRUCTION    |
|  [26]   | `Prelude.memoK(K<F,A>)` / `memoK(A)`     | static   | preloaded memo over a carrier value |
|  [27]   | `Memo.Reset()`                           | instance | drop a memoized value               |
|  [28]   | `Range.fromMinMax(A, A, A)`              | static   | generated bounded sequence          |
|  [29]   | `Prelude.Range(int\|long from, count)`   | static   | `Range<A>` from origin and count    |
|  [30]   | `Prelude.unit`                           | property | the `Unit` literal                  |
|  [31]   | `Prelude.identity(A)`                    | static   | the identity projection             |

- `memoK` caches the CONSTRUCTION of a `K<F, A>`, never its execution — a memoized `IO` or `Eff` is built once and then run on every call, so a `memoK`ed effect is not a cached RESULT and a body expecting one recomputes silently. Caching a result means memoizing past the run (`memo` over the executed value), and the `memoK(K<F,A>)`/`memoK(A)` arities are the preloaded forms where the value already exists.
- `memo(IEnumerable<A>)` retains each item as it is first enumerated, so a second walk replays from the cache and an expensive generator is traversed once — the lazy counterpart to forcing into a `Seq`, where forcing pays the whole cost up front.

[ENTRYPOINT_SCOPE]: `AtomHashMap<K, V>` — the lock-free keyed cell (`Prelude.AtomHashMap(…)` or `HashMap.ToAtom()` mints one). Every mutation returns `Unit`, publishes on `Change`, and commits under a CAS retry loop; `AtomHashMap<EqK, K, V>` is the same surface under an explicit `EqK` witness.

| [INDEX] | [SURFACE]                                                 | [SHAPE]  | [CAPABILITY]                                      |
| :-----: | :-------------------------------------------------------- | :------- | :------------------------------------------------ |
|  [01]   | `Swap(Func<TrackingHashMap<K,V>, TrackingHashMap<K,V>>)`  | instance | whole-map CAS update seeing its own change log    |
|  [02]   | `SwapKey(K, Func<V, V>)`                                  | instance | one-key CAS update                                |
|  [03]   | `SwapKey(K, Func<Option<V>, Option<V>>)`                  | instance | one-key CAS covering insert, edit, and delete     |
|  [04]   | `Add(K, V)` / `TryAdd(K, V)`                              | instance | insert, throwing or tolerating a duplicate        |
|  [05]   | `AddOrUpdate(K, V)` / `(K, Func<V,V>, Func<V>)`           | instance | upsert, unconditional or matched                  |
|  [06]   | `SetItem(K, V)` / `SetItem(K, Func<V,V>)`                 | instance | replace an existing key                           |
|  [07]   | `TrySetItem(K, V)` / `TrySetItems(IEnumerable<K>, …)`     | instance | replace only where the key is present             |
|  [08]   | `AddRange` / `AddOrUpdateRange` / `TryAddRange`           | instance | one commit per bulk write                         |
|  [09]   | `Remove(K)` / `RemoveRange(IEnumerable<K>)`               | instance | delete one key or a roster                        |
|  [10]   | `Clear()`                                                 | instance | drop every entry                                  |
|  [11]   | `FilterInPlace(Func<K,V,bool>)` / `MapInPlace(Func<V,V>)` | instance | narrow or reshape without rebuilding              |
|  [12]   | `Append` / `Subtract` / `Except` / `SymmetricExcept`      | instance | set algebra against a map or a pair roster        |
|  [13]   | `Union(rhs, WhenMatched)` / `Intersect(rhs, WhenMatched)` | instance | merge under an explicit collision rule            |
|  [14]   | `Change`                                                  | event    | one `HashMapPatch<K,V>` per accepted commit       |
|  [15]   | `Find(K)` / `FindOrMaybeAdd(K, Func<Option<V>>)`          | instance | `Option<V>` read, optionally seeding on a miss    |
|  [16]   | `ToHashMap()` / `ToSeq()` / `AsIterable()`                | instance | immutable snapshot at the read                    |
|  [17]   | `Fold(S, Func<S,K,V,S>)` / `Iter(Action<K,V>)`            | fold     | key-and-value walk over a snapshot                |
|  [18]   | `HashMapPatch.From` / `To` / `Changes`                    | property | the two snapshots and the `HashMap<K, Change<V>>` |

- `AtomHashMap<K, V>` is the ONE carrier where mutation is in place and the value is shared: `Atom<HashMap<K,V>>` would make every keyed write a whole-map `Swap` returning a new map, while `SwapKey` commits one key under the same CAS discipline. The cost is that a mutation returns `Unit`, so nothing about the commit is readable from the return — the verdict rides `Change` or a subsequent `Find`.
- `Swap` hands the transition function a `TrackingHashMap<K, V>`, so a whole-map update can read the deltas it is itself producing and decide from them; the log is what the emitted `HashMapPatch.Changes` is built from.
- `Change` fires ONCE per accepted commit with a `HashMapPatch<K, V>` carrying `From`, `To`, and a `HashMap<K, Change<V>>` of per-key deltas, which makes the cell an observable keyed store rather than a mutable dictionary a watcher polls. A bulk member commits one patch covering every touched key, so a range write is one notification and not one per entry.
- Every mutating member re-runs its transition inside the CAS loop exactly as `Atom<A>.Swap` does, so the same side-effect prohibition binds: a dispose, a counter bump, or a log inside a swap runs once per losing attempt.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Domain operations return `Fin<A>`; `Fin.Succ` and `Fin.Fail` are the construction spellings, and an `Error`-derived domain fault record is the failure payload.
- Accumulation is a mode, not a second carrier: independent gates lift into `Validation<Error, A>`, fan in through the tuple `Apply`, and exit `ToFin` — `Validation` lives exactly between those two conversions.
- Tuple `Apply` binds on `(K<F, A>, …)` receivers across arities 2–10 and the join re-anchors through `As()` or the unary `+`, yet a gate slot declares the CONCRETE `Validation<Error, Unit>` return — the lift IS a user-defined implicit conversion and C# excludes interface targets, so a `K<Validation<Error>, A>` return rejects both ternary arms (`CS0029`); the concrete carrier converts to `K<Validation<Error>, Unit>` by implicit reference conversion, so `Apply` and `Traverse` bind on it as written, and the `K`-typed `Accumulate(Seq<K<Validation<Error>, Unit>>)` arity exists for the INPUT side alone, where `Seq<A>` invariance blocks a caller's already-K-typed slot run.
- `Fin.Match(Succ, Fail)` against `Validation.Match(Fail, Succ)`: named lambda arguments (`Succ:`, `Fail:`) bind by name, so the argument order stops deciding the fold.
- Tier choice is when the effect runs, never which failure type it carries — `Try` synchronously NORMALIZES throws through `Run`; evidence-preserving admission happens in `Op.Catch` before an `Eff` or terminal `IO` defers the already-carried work. All three land on `Fin<A>`, but only the capture funnel retains the raised exception unchanged.
- `guard(condition, error)` is the admission form: it composes inside a `Fin` or `Validation` LINQ body through the `SelectMany` overload over `Guard<E, Unit>`, and stands alone through `ToFin`.
- `Seq<A>` crosses carrier boundaries as `Fin<Seq<A>>`, and `AsSpan` is its zero-copy contiguous read.
- `Arr<A>` is the indexed carrier collection expressions build, and its member surface is NEAR-EMPTY: `Reverse`, and a `Skip` answering `Iterable<A>` — no `Zip`, `Take`, `Concat`, `Distinct`, `Select`, `Where`, and NO indexed `Map` (`Map` publishes only `Func<A, B>`; an indexed lambda is `CS0411`), so adjacent-pair and slice chains re-enter through `toSeq(arr)` first. `Iterable<A>` is the lazy sync-or-async view materializing through `ToSeq` — and its instance `Iter` is VALUE-FIRST while the Foldable extension `Iter` is INDEX-FIRST, so the indexed-lambda order flips with the receiver.
- `Iterable<A>.FromSpan` is the one lift a `params ReadOnlySpan<A>` parameter takes to reach the carrier, because a span cannot cross into a lambda or an iterator; it copies at the call, so the returned carrier outlives the frame.
- Lookups return `Option`: `HashMap.Find`, `Seq.Head`, `Seq.Last` — `Head`/`Last` are PROPERTIES answering `Option<A>`, so a method-call spelling or a direct field deref (`xs.Head.Field`) does not compile; the read goes through `Match`/`IfNone`/`Map`. `Option<A>.Value` is `internal` — the proof-carrying read is the `{ IsSome: true, Case: T value }` probe.
- `HashMap<K, V>` declares TWO `IEnumerable<T>` constructions, so `toSeq(map)` and a direct LINQ operator over the map fail inference (`CS0411`) — the instance `AsIterable()` answers `Iterable<(K Key, V Value)>` with NAMED elements and is the pair walk on `Map` and `HashMap` alike; `ToSeq()` on either folds the VALUES alone (`Seq<V>`), so a body reading `row.Key`/`row.Value` off a `ToSeq()` result does not compile; `Values` answers `Iterable<V>`, a carrier. `Map`/`HashMap` carry their own two-parameter forms — instance `Iter(Action<K, V>)` KEY-FIRST, `Filter`/`Choose` as `Func<K, V, …>` (`Choose` answers the mapped `Map<K, U>`) — that outrank the Foldable extensions.
- `ToMap`/`ToHashMap` take pre-shaped pairs or the two-lambda form — no single-selector overload exists on either; `Somes` has exactly two overloads, both over `Option<A>` streams, so a nullable-reference stream binds neither. No generic pipe `Apply(x => …)` exists — `Apply` is applicative; `Traverse` has NO indexed overload; `.Count(pred)` on a carrier is LEGAL (`Count` is the `int` property, the predicate form falls through to `Enumerable.Count`). `.Map(f).Sequence()` does not land a concrete inner — `Sequence` answers `K<F, K<T, A>>` with an ABSTRACT inner, so `.As()` alone yields `Fin<K<Seq, B>>`; the landing FUSES to `.Traverse(f).As()` (the carriers' instance `Traverse` answers `K<F, Seq<B>>`), or `.Traverse(identity).As()` where the map must stay.
- Neither `Seq<A>` nor `SeqExtensions` declares an ordering member, so `OrderBy`/`OrderByDescending`/`ThenBy` bind the LINQ extensions and answer `IOrderedEnumerable<A>` — a shape carrying no `K<Seq, A>`, and therefore reaching neither the carrier-generic `Fold` family nor the `Option`-shaped `Head`/`Last` properties. An ordered run re-enters through `toSeq(…)` before any carrier member reads it; chaining straight off `OrderBy` resolves to the throwing `Enumerable.Last()` or fails to compile, and the two failures look nothing alike.
- Every LINQ shape leaves the carrier the same way `OrderBy` does — `OfType`, `Cast`, `Reverse`, `Order`, `Select`, `Where` — so `Head`, `Last`, `Find`, `Iter`, `Exists`, `Fold`, `Traverse`, and `Choose` reach none of them, and `Prelude.toSeq(…)` is the ONE re-entry. `ToSeq()` is `FoldableExtensions.ToSeq<F, A>(K<F, A>)` and binds no `IEnumerable`, so `linqShape.ToSeq()` does not compile; `Option<A>.ToSeq()` and `Fin<A>.ToSeq()` are the types' own members and do. The assembly's `this IEnumerable<A>` extension roster is exact and SHORT — `Bind`, `Concat`, `Flatten`, `Distinct`, `Somes`, `Successes`/`Fails`, `Sort`, `Scan`/`Reduce`, `Match`, `ToHashMap`/`ToMap`, `AsIterable` — so `Map`, `Iter`, `Filter`, `Choose`, `Exists`, `ForAll`, `Find`, and `Fold` bind NO LINQ shape, and `GroupBy` is the recurring exit: `x.GroupBy(…)` answers `IEnumerable<IGrouping<K, A>>` on every carrier receiver, so a chained `.Map`/`.Filter`/`.Exists`/`.ToSeq` off it does not compile and the group set re-enters as `toSeq(x.GroupBy(…))`.
- The carrier keeps the members it declares for itself, so the exit list is exact rather than a rule about names: `SeqExtensions.Distinct` takes a `Seq<A>` receiver and answers `Seq<T>` across all three overloads (bare, `Eq`-parameterized, key-selector), and `Seq<A>` publishes `Map`, `Filter`, `Skip`, and `Take` as instance members answering `Seq<A>` — so `.Distinct().Count`, `.Skip(n).Take(n).Traverse(…)`, and `.Map(…).Fold(…)` all stay on the carrier, while the same spellings off an `ImmutableArray`, a `FrozenDictionary.Values`, or an ordered run bind `Enumerable` or nothing at all.
- An UNSEEDED extremum over a carrier reaches neither surface: `FoldableExtensions.Max<T, A>(K<T, A>)`/`Min` answer `Option<A>` while `Enumerable.Max`/`Min` answer the bare scalar, both bind the same receiver by an interface conversion neither of which is better, and `seq.Max()` and `seq.Min()` are therefore `CS0121` on every element type — the numeric non-generic overloads and the `IComparable` generic one alike. The escapes are the SEEDED foldable pair `Max(A initialMax)`/`Min(A initialMin)`, which answer `A` and are total over the empty run, the explicit-`Ord` witness form, the selector-taking `Enumerable.Max(f)`/`Min(f)` where no foldable twin competes, and a plain `Fold` for a total. The seeded form is the one to reach for, because it retires the emptiness guard the unseeded reduction needed beside it. `Sum` is NOT in the class — `FoldableExtensions` carries only the nested `SumT`, so an argless `.Sum()` binds `Enumerable` alone and is legal on a carrier, with `Fold(zero, +)` the carrier-idiomatic total. `Average` is DOUBLY in it — `Average<T, A>(K<T, A>)` and the selector `Average<T, A, B>(K<T, A>, Func<A, B>)` both compete with their `Enumerable` twins and no seeded form exists — so a carrier mean is a `Fold`-then-divide.
- `FoldableExtensions.Iter` publishes `Action<A>` and `Action<int, A>` — the indexed overload takes the INDEX FIRST, the opposite of the instance indexed `Map`'s `(value, index)`, so a `(value, index)` lambda handed to `Iter` binds the value to the ordinal slot and fails at the first member read rather than at the call. The flip repeats between spellings of ONE operation: the module `LanguageExt.Seq.map` is index-first while the instance `Seq<A>.Map` is value-first, and `Iterable<A>`'s instance `Iter` is value-first against the Foldable extension's index-first — prove which form binds before writing the lambda. `ForAll` publishes NO indexed overload at all, so an index-bearing predicate pairs the ordinal in through `Map((value, index) => …)` first. On the K carrier, `Match` exists only for `ValidationT` and `Coproduct` — a `K<Fin, …>` chain lands `.As()` before any `Match`.
- `Option<A>` publishes no `MatchUnsafe`; a null-answering projection is the pattern probe `option is { IsSome: true, Case: T value } ? value : null`, which also carries the `IsSome` proof the boundary law requires of any `Case` read.
- The v4 `*Unsafe` family is otherwise GONE — `IfNoneUnsafe`, `IfSomeUnsafe`, `IfFailUnsafe`, `HeadUnsafe` exist on no type — and the ONE survivor is `LanguageExt.UnsafeValueAccess.UnsafeValueAccessExtensions.ValueUnsafe` over `Option<A>` and `Either<L, R>`, answering `A?`: the null-answering unwrap for a REFERENCE element, while a struct element answers `default(A)` rather than null, so a `Nullable`-target read off a struct-element `Option` spells `Match<T?>(Some: static v => v, None: static () => null)` instead. A value fallback is the type's own `IfNone(A)`/`IfNone(Func<A>)`, which also carries a throwing fallback.
- `Prelude.Range(from, count)` answers `Range<A>`, a Foldable that reaches `Iter`, `Fold`, and `Count` but publishes NO `Map` — a `Map` off it binds `ValueTuple1Extensions.Map` and fails inference — so it is the bounded-fixpoint driver a counter loop retires into, and a projection over an integer span crosses `toSeq(Enumerable.Range(…))` instead.
- `SeqExtensions` carries the members `Seq<A>` itself does not: `Append(Seq<T>)` and `Append(Seq<Seq<T>>)` answer `Seq<T>` where `Enumerable.Append` takes a single element, and `Zip(Seq<U>)` answers `Seq<(T First, U Second)>` beside the projecting arity — so a pairwise walk stays on the carrier and reads its halves by those two names.
- `HeadOrNone` survives only on `IQueryable<T>` and `AtomSeq` — gone from `Seq`/`Lst`/`Arr`, where the name lingers from v4-era prose. `Seq<A>` publishes `Head` and `Last` as `Option` properties and `headOrNone`/`lastOrNone` as static optics, so an optional first read over a filtered enumerable is `toSeq(rows.OfType<T>()).Head`.
- `Seq<A>.Map<B>` takes `Func<A, int, B>` — value first, index second — while the indexed `Choose` on `SeqExtensions` and the `Seq` module's `map`/`choose` take `Func<int, A, …>`, index first. The instance twin is the one the carrier spells; a `(value, index)` lambda handed to the module form binds the index to the value slot.
- `HashMap<K, V>` declares NO fold of its own; the carrier-generic `Fold` reaches it through `K<HashMap<K>, V>`, whose element is `V` ALONE. Folding with the key runs over `AsIterable()`, whose element is the `(K Key, V Value)` pair — `map.Fold(seed, (state, pair) => … pair.Key …)` does not type, and the three-argument `Fold(S, Func<S,K,V,S>)` belongs to the `Eq`-parameterized `HashMap<EqK, K, V>`, not to the two-parameter map.
- `TrackingHashMap<K, V>` accumulates its own delta: each `AddOrUpdate*` and `Remove*` writes a `Change<V>` entry beside the value, `Changes` reads that log as a `HashMap<K, Change<V>>`, and `Snapshot()` returns the SAME data with the log zeroed — so a delta between two points is `snapshot` then mutate then `Changes`, never a diff of two maps. `Find`, `ContainsKey`, and `TryGetValue` log nothing, and `ToHashMap()` drops the log at the point where the delta stops mattering.
- A `Change<A>` reads through `HasNoChange`/`HasChanged`/`HasAdded`/`HasRemoved`/`HasMapped` or the open `HasMappedFrom<FROM>()`, never a `switch` over the sealed case classes; `HasMapped` answers the mapped-TO side, and `ToOption()` projects the post-change value — `Some` for `EntryAdded` and a mapped-to entry, `None` for `EntryRemoved` and `NoChange`.
- Indexed enumeration is the instance `Map((value, index) => …)`; the module `LanguageExt.Seq.map(seq, (index, value) => …)` transposes, so a mechanical rewrite between the two silently swaps the lambda arguments.
- `Traverse` inverts effect and shape applicatively (`Seq<Fin<A>>` to `Fin<Seq<A>>`); `TraverseM` inverts monadically and short-circuits on the first failure; `Partition` inverts without exiting, keeping both branches.
- `Option : Traversable<Option>`, so traversing an optional value is total over absence — `None` yields the applicative's own `Pure`, which makes `option.TraverseM(f).As()` the fold an optional payload's conditional effect takes and deletes the `Match` arm pair a carrier forbids mid-pipeline.
- `Error : Monoid<Error>` is why `Validation<Error, A>` accumulates: `Combine` and `+` join failures into one carrier that `Head`, `Tail`, `Count`, and `AsIterable` re-enumerate.
- `Atom<A>.Swap` owns lock-free shared state and publishes each accepted swap on `Change`; `AtomHashMap<K, V>` owns the same discipline at KEY grain, so a shared index takes per-key commits and publishes a per-key delta instead of replacing a whole map per write; `Ref<A>` owns the transactional cell that `atomic` commits across several refs in one isolation scope.
- `Atom<A>.Swap` returns the NEW value, so a take-and-clear spelled as `cell.Swap(_ => empty)` hands back the empty value it just installed — an evidence or tally cell drained that way reports zero forever. Hand-off reads need a member returning the prior value; `Value` is the honest snapshot where none exists.
- `Atom<A>.SwapMaybe` returns `A`, never `Option<A>`: a refused transition hands back the CURRENT state, which is byte-identical to what a committed no-op transition hands back, so the return alone cannot tell a refusal from a commit. The verdict rides a frame-local capture inside the transition function — the last invocation is the committing one — or the value itself carries an owner column the caller compares.
- `Atom<A>.Swap`/`SwapMaybe` re-run their function inside a `SpinWait` CAS loop, so the function must be free of side effects — a dispose, a counter bump, or a log inside a swap runs once per losing attempt, and a handle released on an attempt that then loses the exchange is a live cell holding a dead native. A release therefore rides the state the swap ANSWERS: the transition records what it unlinked on the value it installs, and the caller drains that roster once after the swap returns, which also makes a losing attempt recompute against the winner's state and select again.

[STACKING]:
- `Thinktecture.Runtime.Extensions`(`.api/api-thinktecture-runtime-extensions.md`): a generated `IObjectFactory.Validate` returns its `TValidationError`, which the admission gate maps to `Error` and lands on `Fin<A>`, or on `Validation<Error, A>` when several value objects admit at once; `ISmartEnum.TryGet` lifts to `Option<T>`.
- `Riok.Mapperly`(`.api/api-mapperly.md`): a generated mapper method returns the bare target and throws per its null policy, so `Op.Catch` preserves any thrown exception and keeps the carrier outside the generated body.
- `CSparse`(`.api/api-csparse.md`): `Create` and `Solve` enter `Op.Catch`, which preserves a foreign exceptional `Error` unless the boundary maps a documented provider refusal.
- `System.Threading.Channels`(`.api/api-bcl-channels.md`): a rejected `TryWrite` and the `itemDropped` delegate fold into one `Atom<A>.Swap` loss counter, and a `ReadAllAsync` drain body lands on `Fin<A>` or `Eff<A>`.
- `System.Runtime.InteropServices`(`.api/api-bcl-interop.md`): throwing `Create`, `Load`, and `GetExport` enter `Op.Catch` before landing on `Fin<A>`; registered handles collect in an `Atom<Seq<IDisposable>>` released in reverse-registration order.
- Within-library composition runs at operator depth: `+ma` re-anchors a `K<F, A>`, `ma | mb` chooses, `mf * ma` applies, `ma >> f` binds, and `ma | @catch(pred, recover)` recovers by predicate.
- Lifetime and cadence are values: a resource acquires through `use` or `IO.Bracket`, and a repeat or retry composes an `IO` with a `Schedule` built from a constructor, a bound, and a jitter transformer rather than a hand delay ladder.
- Recovery is a value too: `Catch`'s code, identity, and predicate selectors classify a failure at the `Fallible<E, F>` interface, so one posture composes across every failing carrier instead of per-call-site `try`/`catch`.
- `FinT<M, A>` and `ReaderT<Env, M, A>` stack a result over another carrier, so a nested generic never needs a hand fold.
- Evidence and refusal are different carriers: `Validation<F, A>` accumulates what refuses, `Writer<W, A>` accumulates what merely happened, and stacking them (`WriterT<W, Fin, A>`) is how one pass carries both without folding a log into a failure payload.

[LOCAL_ADMISSION]:
- Carriers, collections, traits, and transformers compose directly; a domain failure type derives `Error` so it rides `Fin` and `Validation` natively.
- `using static LanguageExt.Prelude;` is in force in carrier code: `Some`, `None`, `Optional`, `guard`, `Seq`, `toSeq`, `unit`, `Atom`, `AtomHashMap`, `Ref`, `atomic`, `memo`, `memoK`, `tell`, `foldWhileM`, and `use` are unqualified vocabulary.
- Every public signature carries the concrete carrier; a `K<F, A>` and the trait interfaces stay inside one composition body.
