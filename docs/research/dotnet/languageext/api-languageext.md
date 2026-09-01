# [LANGUAGEEXT_API]

## [01]-[PUBLIC_TYPES]

[TYPE_SCOPE]: result, validation, and effect types

| [INDEX] | [SYMBOL]           | [DECLARATION]   | [DESCRIPTION]                                       |
| :-----: | :----------------- | :-------------- | :-------------------------------------------------- |
|  [01]   | `Fin<A>`           | abstract class  | short-circuiting `Succ`/`Fail` result               |
|  [02]   | `Validation<F, A>` | abstract record | accumulating result, `F` monoid resolved at runtime |
|  [03]   | `Option<A>`        | readonly struct | optional value with null-tolerant lift              |
|  [04]   | `Either<L, R>`     | abstract record | disjoint union, converts to `Fin`                   |
|  [05]   | `Try<A>`           | record          | `Func<Fin<A>>` exception normalization              |
|  [06]   | `Eff<A>`           | record          | runtime-free deferred effect                        |
|  [07]   | `Eff<RT, A>`       | record          | reader-runtime deferred effect                      |
|  [08]   | `IO<A>`            | abstract record | base effect type with bracket and schedule          |
|  [09]   | `Error`            | abstract record | `Monoid<Error>` failure type                        |
|  [10]   | `Expected`         | record          | expected failure keyed by `Code`                    |
|  [11]   | `Exceptional`      | record          | exception-derived failure                           |
|  [12]   | `ManyErrors`       | sealed record   | accumulated failure container                       |
|  [13]   | `Guard<E, A>`      | readonly struct | predicate guard used in a LINQ query                |
|  [14]   | `Pure<A>`          | record struct   | type-agnostic success literal                       |
|  [15]   | `Fail<E>`          | record struct   | type-agnostic failure literal                       |
|  [16]   | `CatchM<E, M, A>`  | record struct   | predicate-selected recovery handler                 |

[TYPE_SCOPE]: immutable collections, state, and optics

| [INDEX] | [SYMBOL]                      | [DECLARATION]   | [DESCRIPTION]                                             |
| :-----: | :---------------------------- | :-------------- | :-------------------------------------------------------- |
|  [01]   | `Seq<A>`                      | readonly struct | ordered immutable sequence with `AsSpan`                  |
|  [02]   | `Arr<A>`                      | readonly struct | indexed immutable array                                   |
|  [03]   | `Lst<A>`                      | readonly struct | persistent linked list                                    |
|  [04]   | `HashMap<K, V>`               | readonly struct | hashed persistent map                                     |
|  [05]   | `Map<K, V>`                   | readonly struct | ordered persistent map                                    |
|  [06]   | `TrackingHashMap<K, V>`       | readonly struct | map carrying its own change log                           |
|  [07]   | `HashSet<A>`                  | readonly struct | hashed persistent set                                     |
|  [08]   | `Set<A>`                      | readonly struct | ordered persistent set                                    |
|  [09]   | `Stck<A>`                     | readonly struct | persistent LIFO stack                                     |
|  [10]   | `Que<A>`                      | readonly struct | persistent FIFO queue                                     |
|  [11]   | `Iterable<A>`                 | abstract class  | lazy sync or async sequence                               |
|  [12]   | `Atom<A>`                     | sealed class    | lock-free CAS reference with `Change`                     |
|  [13]   | `Atom<M, A>`                  | sealed class    | CAS reference threading construction metadata             |
|  [14]   | `AtomHashMap<K, V>`           | class           | lock-free map that mutates in place                       |
|  [15]   | `HashMapPatch<K, V>`          | sealed class    | one map change set: `From`, `To`, `Changes`               |
|  [16]   | `AtomHashMapChangeEvent<K,V>` | delegate        | `AtomHashMap.Change` handler over a patch                 |
|  [17]   | `Ref<A>`                      | sealed class    | transactional reference that `atomic` commits             |
|  [18]   | `Memo<A>`                     | class           | resettable memoized thunk                                 |
|  [19]   | `Memo<F, A>`                  | class           | memoized `K<F, A>` CONSTRUCTION, not its run              |
|  [20]   | `Lens<A, B>`                  | readonly struct | composable get and immutable set                          |
|  [21]   | `Range<A>`                    | record          | generated bounded sequence, `Range.fromMinMax` creates it |
|  [22]   | `AtomChangedEvent<A>`         | delegate        | `Atom.Change` handler over the new value                  |
|  [23]   | `Change<A>`                   | abstract class  | `TrackingHashMap` change-log entry, cases below           |
|  [24]   | `IOptional`                   | interface       | non-generic interface every `Option<A>` implements        |

[TYPE_SCOPE]: traits and monad transformers (`LanguageExt.Traits`)

| [INDEX] | [SYMBOL]               | [DECLARATION]   | [DESCRIPTION]                                 |
| :-----: | :--------------------- | :-------------- | :-------------------------------------------- |
|  [01]   | `K<F, A>`              | interface       | higher-kinded interface every type implements |
|  [02]   | `Functor<F>`           | interface       | `Map` conformance                             |
|  [03]   | `Applicative<F>`       | interface       | `Apply` fan-in conformance                    |
|  [04]   | `Monad<M>`             | interface       | `Bind` and tail-recursive `Recur`             |
|  [05]   | `MonadIO<M>`           | interface       | `IO` lifting into another monad               |
|  [06]   | `Semigroup<A>`         | interface       | associative `Combine`                         |
|  [07]   | `Monoid<A>`            | interface       | `Combine` with an identity                    |
|  [08]   | `Foldable<T>`          | interface       | fold, search, and aggregate conformance       |
|  [09]   | `Traversable<T>`       | interface       | effect and shape inversion                    |
|  [10]   | `Alternative<F>`       | interface       | first-success choice                          |
|  [11]   | `Fallible<E, F>`       | interface       | typed failure raise and recover               |
|  [12]   | `Readable<M, Env>`     | interface       | ambient-environment reads                     |
|  [13]   | `Stateful<M, S>`       | interface       | threaded-state reads and writes               |
|  [14]   | `Writable<M, W>`       | interface       | monoidal-output conformance                   |
|  [15]   | `ReaderT<Env, M, A>`   | record          | environment threaded over any `M`             |
|  [16]   | `StateT<S, M, A>`      | record          | state threaded over any `M`                   |
|  [17]   | `Writer<W, A>`         | record          | monoidal output alone, no inner `M`           |
|  [18]   | `WriterT<W, M, A>`     | record          | monoidal output over any `M`                  |
|  [19]   | `Tell<W>`              | record          | type-agnostic output literal                  |
|  [20]   | `RWST<R, W, S, M, A>`  | record          | reader, writer, and state in one pass         |
|  [21]   | `FinT<M, A>`           | record          | `Fin` stacked over any `M`                    |
|  [22]   | `OptionT<M, A>`        | record          | `Option` stacked over any `M`                 |
|  [23]   | `EitherT<L, M, A>`     | record          | `Either` stacked over any `M`                 |
|  [24]   | `ValidationT<F, M, A>` | record          | `Validation` stacked over any `M`             |
|  [25]   | `Free<F, A>`           | abstract record | open interpreter over a functor               |
|  [26]   | `Schedule`             | abstract record | composable repeat and retry policy            |
|  [27]   | `ScheduleTransformer`  | readonly struct | `Schedule → Schedule`, composing under `+`    |

[TYPE_SCOPE]: domain modelling traits (`LanguageExt.Traits.Domain`)

| [INDEX] | [SYMBOL]                         | [DECLARATION] | [DESCRIPTION]                          |
| :-----: | :------------------------------- | :------------ | :------------------------------------- |
|  [01]   | `DomainType<SELF>`               | interface     | arity-one marker interface, NO members |
|  [02]   | `DomainType<SELF, REPR>`         | interface     | `From`/`FromUnsafe` in, `To()` out     |
|  [03]   | `Identifier<SELF>`               | interface     | equality-only domain identity          |
|  [04]   | `VectorSpace<SELF, SCALAR>`      | interface     | addition and scalar multiplication     |
|  [05]   | `Amount<SELF, SCALAR>`           | interface     | ordered vector space with comparison   |
|  [06]   | `Locus<SELF, DIST, DIST_SCALAR>` | interface     | affine position over a distance type   |

[TYPE_SCOPE]: trait derivation (`LanguageExt.Deriving`) — each interface lifts one `LanguageExt.Traits` conformance off the `Subtype` its `Supertype` wraps

| [INDEX] | [SYMBOL]                                     | [DECLARATION] | [DESCRIPTION]                                             |
| :-----: | :------------------------------------------- | :------------ | :-------------------------------------------------------- |
|  [01]   | `Deriving.Alternative<Supertype, Subtype>`   | interface     | `Choose`, `Pure`, `Apply`, `Map`; `Empty` stays abstract  |
|  [02]   | `Deriving.Applicative<Supertype, Subtype>`   | interface     | `Pure`, `Action`, both `Apply` arities                    |
|  [03]   | `Deriving.Choice<Supertype, Subtype>`        | interface     | `Choose` over a strict and a `Memo` right side            |
|  [04]   | `Deriving.Cofunctor<Supertype, Subtype>`     | interface     | `Comap` contravariant projection                          |
|  [05]   | `Deriving.Decidable<Supertype, Subtype>`     | interface     | `Lose` and `Route` over `Either<B, C>`                    |
|  [06]   | `Deriving.Divisible<Supertype, Subtype>`     | interface     | `Divide` and `Conquer`, constrained on `Decidable`        |
|  [07]   | `Deriving.Fallible<E, Supertype, Subtype>`   | interface     | `Fail` and predicate `Catch` in the failure type `E`      |
|  [08]   | `Deriving.Fallible<Supertype, Subtype>`      | interface     | `Fail` and `Catch` with `E` fixed to `Error`              |
|  [09]   | `Deriving.Final<Supertype, Subtype>`         | interface     | `Finally` teardown across either branch                   |
|  [10]   | `Deriving.Foldable<Supertype, Subtype>`      | interface     | every fold, search, aggregate, and `ToSeq` default        |
|  [11]   | `Deriving.Functor<Supertype, Subtype>`       | interface     | `Map`                                                     |
|  [12]   | `Deriving.Monad<Supertype, Subtype>`         | interface     | `Bind`, `Flatten`, tail-recursive `Recur`                 |
|  [13]   | `Deriving.MonadIO<Supertype, Subtype>`       | interface     | both `LiftIO` arities                                     |
|  [14]   | `Deriving.MonadT<Supertype, Subtype, M>`     | interface     | `Lift` from the inner `M`                                 |
|  [15]   | `Deriving.MonadUnliftIO<Supertype, Subtype>` | interface     | `ToIO`, `MapIO`, bracket, timeout, repeat, retry, fold-IO |
|  [16]   | `Deriving.MonoidK<Supertype, Subtype>`       | interface     | `Empty` identity element                                  |
|  [17]   | `Deriving.Readable<Supertype, Env, Subtype>` | interface     | `Ask`, `Asks`, `Local` over `Env`                         |
|  [18]   | `Deriving.SemigroupK<Supertype, Subtype>`    | interface     | `Combine`                                                 |
|  [19]   | `Deriving.Stateful<Supertype, Subtype, S>`   | interface     | `Get`, `Put`, `Modify`, `Gets` over `S`                   |
|  [20]   | `Deriving.Traversable<Supertype, Subtype>`   | interface     | `Traverse`, `Sequence`, their `M` twins, and `Foldable`   |
|  [21]   | `Deriving.Writable<Supertype, Subtype, W>`   | interface     | `Tell`, `Listen`, `Pass` over `W : Monoid<W>`             |

## [02]-[MEMBERS]

[MEMBER_SCOPE]: `Fin<A>` construction, fold, and conversion

| [INDEX] | [MEMBER]                                         | [KIND]   | [DESCRIPTION]                               |
| :-----: | :----------------------------------------------- | :------- | :------------------------------------------ |
|  [01]   | `Fin.Succ(A)`                                    | static   | success construction                        |
|  [02]   | `Fin.Fail(Error)`                                | static   | failure construction                        |
|  [03]   | `Fin.Match(Func<A,B>, Func<Error,B>)`            | instance | total value fold, `Succ` first              |
|  [04]   | `Fin.Match(Action<A>, Action<Error>)`            | instance | total effect fold                           |
|  [05]   | `Fin.Map(Func<A,B>)`                             | instance | success projection                          |
|  [06]   | `Fin.MapFail(Func<Error,Error>)`                 | instance | failure projection                          |
|  [07]   | `Fin.BiMap(Func<A,B>, Func<Error,Error>)`        | instance | both-branch projection                      |
|  [08]   | `Fin.Bind(Func<A,Fin<B>>)`                       | instance | monadic chain                               |
|  [09]   | `Fin.BindFail(Func<Error,Fin<A>>)`               | instance | failure-branch recovery chain               |
|  [10]   | `Fin.BiBind(Func<A,Fin<B>>, Func<Error,Fin<B>>)` | instance | both-branch chain                           |
|  [11]   | `Fin.SelectMany(Func<A,Guard<Error,Unit>>)`      | instance | `guard` inside a LINQ query                 |
|  [12]   | `Fin.IfFail(Func<Error,A>)`                      | instance | failure escape to a value                   |
|  [13]   | `Fin.IfSucc(Action<A>)`                          | instance | success effect tap                          |
|  [14]   | `Fin.Iter(Action<A>)`                            | instance | success iteration                           |
|  [15]   | `Fin.Fold(S, Func<S,A,S>)`                       | fold     | success state fold                          |
|  [16]   | `Fin.BiFold(S, Func<S,A,S>, Func<S,Error,S>)`    | fold     | both-branch state fold                      |
|  [17]   | `Fin.Exists(Func<A,bool>)`                       | instance | success predicate                           |
|  [18]   | `Fin.ForAll(Func<A,bool>)`                       | instance | total predicate                             |
|  [19]   | `Fin.Traverse(Func<A,K<F,B>>)`                   | instance | applicative effect distribution             |
|  [20]   | `Fin.TraverseM(Func<A,K<M,B>>)`                  | instance | monadic effect distribution                 |
|  [21]   | `Fin.ToOption()`                                 | instance | conversion to `Option`                      |
|  [22]   | `Fin.ToEither()`                                 | instance | conversion to `Either`                      |
|  [23]   | `Fin.ToValidation()`                             | instance | conversion to `Validation`                  |
|  [24]   | `Fin.ToEff()`                                    | instance | conversion to `Eff`                         |
|  [25]   | `Fin.ToSeq()`                                    | instance | conversion to `Seq`                         |
|  [26]   | `Fin.ThrowIfFail()`                              | instance | unwrap, throws on `Fail`                    |
|  [27]   | `FinExtensions.As(K<Fin,A>)`                     | static   | cast `K<Fin, A>` to `Fin<A>`                |
|  [28]   | `FinExtensions.ToFin(Either<Error,A>)`           | static   | conversion from `Either`                    |
|  [29]   | `FinExtensions.Partition()`                      | static   | split a foldable of `Fin`, no short-circuit |
|  [30]   | `FinExtensions.Succs()`                          | static   | success values of a foldable of `Fin`       |
|  [31]   | `FinExtensions.Fails()`                          | static   | failure values of a foldable of `Fin`       |
|  [32]   | `Fin operator \|`                                | operator | first-success alternative                   |
|  [33]   | `Fin operator \|` over `CatchM<Error,Fin,A>`     | operator | predicate-selected recovery                 |
|  [34]   | `Fin unary operator +`                           | operator | cast `K<Fin, A>` to `Fin<A>`                |
|  [35]   | `Fin operator *`                                 | operator | applicative apply                           |
|  [36]   | `Fin operator >>`                                | operator | monadic bind and sequence                   |

[MEMBER_SCOPE]: `Option<A>` and `Guard`

| [INDEX] | [MEMBER]                                           | [KIND]    | [DESCRIPTION]                       |
| :-----: | :------------------------------------------------- | :-------- | :---------------------------------- |
|  [01]   | `Prelude.Some(A)`                                  | static    | present-value construction          |
|  [02]   | `Prelude.Optional(A?)`                             | static    | null-tolerant construction          |
|  [03]   | `Option<A>.None`                                   | field     | absent literal                      |
|  [04]   | `Option.Match(Func<A,B>, Func<B>)`                 | instance  | total fold over both branches       |
|  [05]   | `Option.IfNone(A)`                                 | instance  | default escape                      |
|  [06]   | `Option.Filter(Func<A,bool>)`                      | instance  | predicate narrowing                 |
|  [07]   | `Option.Bind(Func<A,Option<B>>)`                   | instance  | monadic chain                       |
|  [08]   | `Option.ToFin(Error)`                              | instance  | conversion to `Fin`                 |
|  [09]   | `Option.ToValidation(L)`                           | instance  | conversion to `Validation`          |
|  [10]   | `Option.ToSeq()`                                   | instance  | conversion to `Seq`                 |
|  [11]   | `Option.ToEither(L)`                               | instance  | conversion to `Either`              |
|  [12]   | `Option.TraverseM(Func<A,K<M,B>>)`                 | instance  | effect inversion, total over `None` |
|  [13]   | `OptionExtensions.Somes(Seq<Option<A>>)`           | static    | drop `None` members in one pass     |
|  [14]   | `Prelude.guard(bool, Error)`                       | static    | predicate guard literal             |
|  [15]   | `FinGuardExtensions.ToFin(Guard<Error,Unit>)`      | static    | guard to `Fin<Unit>`                |
|  [16]   | `FinGuardExtensions.SelectMany(Func<Unit,Fin<B>>)` | static    | guard as a LINQ `from` clause       |
|  [17]   | `IOptional`                                        | interface | non-generic optional read           |
|  [18]   | `IOptional.IsSome` / `IsNone`                      | property  | presence on a BOXED `Option<A>`     |
|  [19]   | `Option.Exists(Func<A,bool>)`                      | instance  | predicate over the `Some` branch    |
|  [20]   | `Option.Map<B>(Func<A,B>) -> Option<B>`            | instance  | functor over the `Some` branch      |
|  [21]   | `Option.Iter(Action<A>) -> Unit`                   | instance  | side effect on the `Some` branch    |

[MEMBER_SCOPE]: `Validation<F, A>` accumulation and the `Error` types

| [INDEX] | [MEMBER]                                          | [KIND]   | [DESCRIPTION]                           |
| :-----: | :------------------------------------------------ | :------- | :-------------------------------------- |
|  [01]   | `Validation.Success(A)`                           | static   | success construction                    |
|  [02]   | `Validation.Fail(F)`                              | static   | failure construction                    |
|  [03]   | `Validation.Match(Func<F,B>, Func<A,B>)`          | instance | total fold, `Fail` first                |
|  [04]   | `Validation.Map(Func<A,B>)`                       | instance | success projection                      |
|  [05]   | `Validation.MapFail(Func<F,F1>)`                  | instance | failure projection                      |
|  [06]   | `Validation.Bind(Func<A,Validation<F,B>>)`        | instance | monadic chain                           |
|  [07]   | `Validation.BiFold(S, Func<S,F,S>, Func<S,A,S>)`  | fold     | both-branch state fold                  |
|  [08]   | `Validation.ToOption()`                           | instance | conversion to `Option`                  |
|  [09]   | `Validation.ToEither()`                           | instance | conversion to `Either`                  |
|  [10]   | `Validation.ToSeq()`                              | instance | conversion to `Seq`                     |
|  [11]   | `ValidationExtensions.ToFin(Validation<Error,A>)` | static   | conversion to `Fin`                     |
|  [12]   | `ValidationExtensions.As(K<Validation<F>,A>)`     | static   | cast to `Validation<F, A>`              |
|  [13]   | `ValidationExtensions.Successes()`                | static   | success values of a collection          |
|  [14]   | `ValidationExtensions.Fails()`                    | static   | failure values of a collection          |
|  [15]   | `Validation operator \|`                          | operator | first-success choice, left failure kept |
|  [16]   | `ApplicativeExtensions.Apply(tuple, Func<A,B,R>)` | static   | higher-kinded fan-in, arities 2–10      |
|  [17]   | `Error.New(int, string)`                          | static   | `Expected` construction                 |
|  [18]   | `Error.New(string, Exception)`                    | static   | exception-preserving construction       |
|  [19]   | `Error.New(Exception)`                            | static   | `Exceptional` construction              |
|  [20]   | `Error.Many(Seq<Error>)`                          | static   | `ManyErrors` construction               |
|  [21]   | `Error.Combine(Error)`                            | instance | monoidal failure join                   |
|  [22]   | `Error operator +`                                | operator | terse monoidal failure join             |
|  [23]   | `Error.Head`                                      | property | first accumulated failure               |
|  [24]   | `Error.Tail`                                      | property | remaining accumulated failures          |
|  [25]   | `Error.Count`                                     | property | accumulated-failure count               |
|  [26]   | `Error.AsIterable()`                              | instance | accumulated-failure enumeration         |
|  [27]   | `Error.Is(Error)`                                 | instance | failure identity test                   |
|  [28]   | `Error.IsType<E>()`                               | instance | failure type test                       |
|  [29]   | `Error.HasCode(int)`                              | instance | failure code test                       |
|  [30]   | `Error.Filter<E>()`                               | instance | failure-subset selection                |
|  [31]   | `Error.Exception`                                 | property | optional exceptional payload            |
|  [32]   | `Error.Inner`                                     | property | optional cause chain                    |
|  [33]   | `Error.ToErrorException()`                        | instance | expected-error exception wrapper        |
|  [34]   | `Error.ToException()`                             | instance | conversion to `Exception`               |
|  [35]   | `Error.Throw<R>()`                                | instance | throw the error                         |
|  [36]   | `Errors.Cancelled`                                | static   | cancellation error, `-2000000001`       |
|  [37]   | `Errors.TimedOut`                                 | static   | timeout error, `-2000000002`            |
|  [38]   | `Errors.SequenceEmpty`                            | static   | empty-sequence error, `-2000000003`     |
|  [39]   | `Errors.Closed`                                   | static   | closed-resource, `-2000000004`          |
|  [40]   | `Errors.ValidationFailed`                         | static   | validation failure, `-2000000011`       |
|  [41]   | `Errors.SourceClosed` / `SourceCompleted`         | static   | `-2000000013` / `-2000000012`           |
|  [42]   | `Errors.SinkFull`                                 | static   | back-pressure error, `-2000000015`      |
|  [43]   | `Errors.EndOfStream`                              | static   | end-of-stream error, `-2000000010`      |
|  [44]   | `Errors.Bottom`                                   | static   | bottom-value error                      |
|  [45]   | `Errors.None`                                     | static   | the empty `ManyErrors` value            |
|  [46]   | `Errors.ParseError(string)`                       | static   | parse failure construction              |

- `Error.New(string, Exception)` requires an argument statically typed as `Exception`. A derived or generic exception also converts implicitly to `Error`, which makes the two-argument call ambiguous with `Error.New(string, Error)`. Widen or cast the argument before the call.
- `Errors` declares the package failure values as a closed negative-code block, `Error.HasCode` and `Error.Is` separate a cancellation from a timeout, an empty sequence from a validation failure, and a completed source from a closed one. A match on the message text re-classifies when the text changes. The block occupies the `-2000000001`..`-2000000015` span.

[MEMBER_SCOPE]: `Fallible<E, F>` — the recovery and partition members shared by every failing type

| [INDEX] | [MEMBER]                                                 | [KIND] | [DESCRIPTION]                              |
| :-----: | :------------------------------------------------------- | :----- | :----------------------------------------- |
|  [01]   | `Catch(Func<Error, K<F,A>>)`                             | static | unconditional effectful recovery           |
|  [02]   | `Catch(int Code, Func<Error, K<F,A>>)`                   | static | code-selected recovery                     |
|  [03]   | `Catch(Error Match, Func<Error, K<F,A>>)`                | static | identity-selected recovery                 |
|  [04]   | `Catch(Func<Error,bool> Predicate, Func<Error, K<F,A>>)` | static | predicate-selected recovery                |
|  [05]   | `Catch(Func<Error, Error>)`                              | static | failure reprojection, still failed         |
|  [06]   | `Catch(Func<Error, A>)`                                  | static | recovery to a value under `Applicative<F>` |
|  [07]   | `Catch(K<F,A>)` / `Catch(Pure<A>)` / `Catch(A)`          | static | unconditional alternative                  |
|  [08]   | `Catch(CatchM<Error, F, A>)`                             | static | a handler VALUE applied to the type        |
|  [09]   | `CatchIO(Func<Error, K<IO,A>>)`                          | static | recovery into `IO` under `MonadIO<M>`      |
|  [10]   | `PartitionFallible(Seq<K<M,A>>)`                         | static | `K<M, (Seq<Error> Fails, Seq<A> Succs)>`   |
|  [11]   | `PartitionFallible(K<F, K<M,A>>)`                        | static | the same over any `Foldable<F>`            |
|  [12]   | `Succs(Seq<K<M,A>>)`                                     | static | `K<M, Seq<A>>`, failures dropped           |
|  [13]   | `Fails(Seq<K<M,A>>)`                                     | static | `K<M, Seq<Error>>`, successes dropped      |

- `PartitionFallible` is the effectful counterpart of `FinExtensions.Partition`. `Partition` splits an already-evaluated collection of `Fin` values. `PartitionFallible` runs a collection of pending effects, does not short-circuit, and returns both branches inside one `M`. Its receivers are `Seq`, `Lst`, `Set`, `HashSet`, `Iterable`, `IEnumerable`, and any `K<F, K<M, A>>`. `Succs` and `Fails` are the one-branch projections over the same receivers.
- The result tuple is `(Seq<Error> Fails, Seq<A> Succs)` — FAILS FIRST, the opposite order of `Fin.Match(Succ, Fail)`. A positional deconstruction across the two reads the branches backwards. Both fields are named. Read them by name.
- `Catch` has three selector arities: `int Code` against an `Errors` value or a domain error code, `Error Match` against a value, and `Func<Error, bool>` where neither suffices. Each pairs with a value argument, an `Error` argument, or a `K<F, A>` argument. A recovery strategy composes as a value instead of a `try`/`catch` chain at the call site.
- `FallibleExtensionsE` carries the same members generalized over the failure type `E`, where `FallibleExtensions` fixes it to `Error`. A type that fails in a non-`Error` type reaches the identical operators by naming its own `E`.

[MEMBER_SCOPE]: `Try`, `Eff`, `IO` — the deferred effect types

| [INDEX] | [MEMBER]                                                                     | [KIND]   | [DESCRIPTION]                             |
| :-----: | :--------------------------------------------------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `Try.lift(Func<A>)`                                                          | static   | exception-normalizing thunk               |
|  [02]   | `TryExtensions.Run(K<Try,A>)`                                                | static   | force the thunk to `Fin<A>`               |
|  [03]   | `Try.ToFin()`                                                                | instance | conversion to `Fin`                       |
|  [04]   | `Try.ToIO()`                                                                 | instance | conversion to `IO`                        |
|  [05]   | `Eff.lift(Func<A>)`                                                          | static   | effect construction from a thunk          |
|  [06]   | `Prelude.liftEff(Func<Task<Fin<A>>>)`                                        | static   | async fallible effect construction        |
|  [07]   | `Eff.runtime<RT>() -> Eff<RT, RT>`                                           | static   | supplied-runtime reader effect            |
|  [08]   | `Eff.getState<RT>()`                                                         | static   | runtime and `EnvIO` read                  |
|  [09]   | `Eff.local(Func<OuterRT,InnerRT>, Eff<InnerRT,A>)`                           | static   | scoped runtime override                   |
|  [10]   | `Eff.localCancel(Eff<RT,A>)`                                                 | static   | scoped cancellation source                |
|  [11]   | `EffExtensions.Run(K<Eff,A>)`                                                | static   | typed execution to `Fin<A>`               |
|  [12]   | `EffExtensions.RunAsync(K<Eff,A>)`                                           | static   | `Task<Fin<A>>` execution                  |
|  [13]   | `EffExtensions.RunIO(K<Eff,A>)`                                              | static   | lower to `IO`                             |
|  [14]   | `Eff.MapFail(Func<Error,Error>)`                                             | instance | failure projection                        |
|  [15]   | `Eff.MapIO(Func<IO<A>,IO<B>>)`                                               | instance | inner-effect projection                   |
|  [16]   | `Eff.IfFailEff(Func<Error,Eff<A>>)`                                          | instance | effectful recovery                        |
|  [17]   | `IO.pure(A)`                                                                 | static   | lifted-value construction                 |
|  [18]   | `IO.fail(Error)`                                                             | static   | failed-effect construction                |
|  [19]   | `IO.lift(Func<A>)`                                                           | static   | thunk lift                                |
|  [20]   | `IO.lift(Func<Fin<A>>)`                                                      | static   | result-typed thunk onto the error channel |
|  [21]   | `IO.lift(Fin<A>)`                                                            | static   | evaluated result lifted whole             |
|  [22]   | `IO.liftAsync(Func<Task<A>>)`                                                | static   | `Task` thunk lift                         |
|  [23]   | `IO.liftVAsync(Func<ValueTask<A>>)`                                          | static   | `ValueTask` thunk lift                    |
|  [24]   | `IO.Run()`                                                                   | instance | synchronous execution                     |
|  [25]   | `IO.RunAsync()`                                                              | instance | `ValueTask` execution                     |
|  [26]   | `IO.Bracket(Func<A,IO<C>>, Func<A,IO<B>>)`                                   | instance | acquire-use-release scope                 |
|  [27]   | `IO.Bracket(Func<A,IO<C>>, Func<Error,IO<C>>, Func<A,IO<B>>)`                | instance | scope with a failure branch               |
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
|  [43]   | `Prelude.@catch(Func<Error,bool>, K<M,A>)`                                   | static   | type-generic recovery handler             |
|  [44]   | `Prelude.use(Func<A>, Action<A>)`                                            | static   | resource-scoped acquisition               |
|  [45]   | `Prelude.tail(IO<A>)`                                                        | static   | tail-recursion marker for deep binds      |

- `Try.lift(...).Run()` normalizes thrown cancellation and timeout exceptions to the package `Expected` values and expands `AggregateException` into `ManyErrors`. It is a normalization pass. An `Error` thrown by the effect returns unchanged.
- `IO.lift` rethrows cancellation during execution, a token-aware boundary must capture the cancellation before the lift.
- In the three-argument `IO.Bracket` form, the `Catch` argument receives the `Error` alone, never the acquired value. A release that needs the resource uses the trailing `Fin` argument.
- `IO.lift` overload selection for a `Fin`-returning thunk is silent, not ambiguous. `Func<Fin<A>>` is the more specific candidate, `IO.lift(() => <Fin<T>>)` resolves to the result-typed overload and returns `IO<T>` with the `Fail` folded onto the error channel — NEVER `IO<Fin<T>>`. To carry the `Fin` as the value, write the type argument: `IO.lift<Fin<T>>(…)`.
- `Prelude.tail` wraps the recursive call as the last bind continuation of a deferred effect. The run loop unwraps it, and any `Map`, `Bind`, `Try()`, or `RunSafe()` placed after the recursion fails with `NotSupportedException` ("You can't map a tail call"). A `tail`-recursive effect exits through `Run()` or `RunAsync()` alone.
- `IO.Fork` starts one dedicated `TaskCreationOptions.LongRunning` thread per fork. Forked effects overlap fully before the await, and the pool imposes NO concurrency bound. An unbounded fan-out creates an unbounded thread count.

[MEMBER_SCOPE]: `Schedule` — the repeat and retry policy every `IO.Repeat`/`Retry` overload accepts

| [INDEX] | [MEMBER]                                                     | [KIND]            | [DESCRIPTION]                             |
| :-----: | :----------------------------------------------------------- | :---------------- | :---------------------------------------- |
|  [01]   | `Schedule.Forever` / `Never` / `Once`                        | static            | the three degenerate policies             |
|  [02]   | `Schedule.spaced(Duration)`                                  | static            | one constant delay, unbounded             |
|  [03]   | `Schedule.linear(Duration seed, double factor)`              | static            | arithmetic growth                         |
|  [04]   | `Schedule.exponential(Duration seed, double factor)`         | static            | geometric growth, factor defaults `2.0`   |
|  [05]   | `Schedule.fibonacci(Duration seed)`                          | static            | Fibonacci growth                          |
|  [06]   | `Schedule.fixedInterval(Duration, Func<DateTime>?)`          | static            | wall-clock cadence net of work time       |
|  [07]   | `Schedule.windowed(Duration, Func<DateTime>?)`               | static            | next window boundary                      |
|  [08]   | `Schedule.upto(Duration max, Func<DateTime>?)`               | static            | run until a wall-clock deadline           |
|  [09]   | `Schedule.secondOfMinute` / `minuteOfHour` / `hourOfDay`     | static            | calendar-aligned cadence                  |
|  [10]   | `Schedule.dayOfWeek(DayOfWeek, Func<DateTime>?)`             | static            | weekly-aligned cadence                    |
|  [11]   | `Schedule.TimeSeries(Seq<Duration>)`                         | static            | an explicit delay sequence                |
|  [12]   | `Schedule.recurs(int) -> ScheduleTransformer`                | static            | cap the ATTEMPT count                     |
|  [13]   | `Schedule.repeat(int) -> ScheduleTransformer`                | static            | replay the WHOLE schedule n times         |
|  [14]   | `Schedule.jitter(double factor, Option<int> seed)`           | static            | proportional randomization                |
|  [15]   | `Schedule.jitter(Duration min, Duration max, Option<int>)`   | static            | absolute-window randomization             |
|  [16]   | `Schedule.decorrelate(double factor, Option<int> seed)`      | static            | de-correlated jitter for parallel callers |
|  [17]   | `Schedule.maxDelay(Duration)`                                | static            | per-step ceiling                          |
|  [18]   | `Schedule.maxCumulativeDelay(Duration)`                      | static            | stop once the total budget is spent       |
|  [19]   | `Schedule.resetAfter(Duration)`                              | static            | restart the policy past a cumulative max  |
|  [20]   | `Schedule.intersperse(Schedule)`                             | static            | weave a second policy between steps       |
|  [21]   | `Schedule.Transform(Func<Schedule, Schedule>)`               | static            | create a transformer from a function      |
|  [22]   | `Schedule.Identity` / `NoDelayOnFirst` / `RepeatForever`     | static            | the built-in transformers                 |
|  [23]   | `Union(Schedule)` / `operator \|`                            | instance/operator | MIN delay, runs while EITHER runs         |
|  [24]   | `Intersect(Schedule)` / `operator &`                         | instance/operator | MAX delay, stops when EITHER stops        |
|  [25]   | `Combine(Schedule)` / `operator +`                           | instance/operator | append the second after the first         |
|  [26]   | `Interleave(Schedule)`                                       | instance          | alternate the two step by step            |
|  [27]   | `Take(int)` / `Skip(int)` / `Tail`                           | instance/property | positional narrowing, `Tail` a property   |
|  [28]   | `Filter(Func<Duration,bool>)` / `Where`                      | instance          | drop steps by their delay                 |
|  [29]   | `Map(Func<Duration,Duration>)` / `Map(Func<Duration,int,_>)` | instance          | reshape each delay, index second          |
|  [30]   | `Prepend(Duration)` / `PrependZero`                          | instance/property | lead-in step, `PrependZero` a property    |
|  [31]   | `Bind` / `SelectMany`                                        | instance          | LINQ composition over the delay series    |
|  [32]   | `Run() -> Iterable<Duration>`                                | instance          | realize the delay series                  |
|  [33]   | `ScheduleTransformer.Apply(Schedule)`                        | instance          | the one application member                |
|  [34]   | `operator +(ScheduleTransformer, ScheduleTransformer)`       | operator          | transformer composition                   |

- `|` and `&` mean DIFFERENT things by operand type and the two forms look identical. Between two `Schedule` values they are union and intersection. Wherever one side is a `ScheduleTransformer` — in EITHER argument order — both operators collapse to `Apply`. `Forever | jitter(0.5)` and `Forever & jitter(0.5)` are the SAME schedule, while `spaced(1s) | spaced(3s)` and `spaced(1s) & spaced(3s)` are not. A transformer never intersects. Only a schedule does.
- `ScheduleTransformer` converts implicitly to `Schedule` by applying itself to `Forever`, `IO.Retry(Schedule.recurs(3))` compiles bare and means `Forever.Take(3)` — exactly three attempts with no delay. A transformer passed to a `Schedule` parameter is never a type error and never a no-op. To cap an existing policy, apply the transformer to it explicitly.
- `recurs(n)` caps attempts while `repeat(n)` replays the entire schedule n times, `exponential(1s) | repeat(3)` runs the backoff series three times over and `exponential(1s) | recurs(3)` truncates it to three steps.
- `maxCumulativeDelay` STOPS the schedule once the accumulated delay crosses its budget while `resetAfter` restarts the policy at that same crossing.
- Each wall-clock constructor takes an optional `Func<DateTime>?` clock. A deterministic test supplies that clock instead of waiting through the real cadence.

[MEMBER_SCOPE]: `FinT<M, A>` — the `Fin`-over-`M` transformer

| [INDEX] | [MEMBER]                                | [KIND]      | [DESCRIPTION]                                         |
| :-----: | :-------------------------------------- | :---------- | :---------------------------------------------------- |
|  [01]   | `new FinT<M, A>(K<M, Fin<A>> runFin)`   | constructor | construction from `K<M, Fin<A>>`                      |
|  [02]   | `FinT.Succ(A)` / `FinT.Fail(Error)`     | static      | construction from an evaluated value                  |
|  [03]   | `FinT.lift(Fin<A>)`                     | static      | conversion from an evaluated `Fin`                    |
|  [04]   | `FinT.lift(K<M, A>)`                    | static      | lift from `K<M, A>`                                   |
|  [05]   | `FinT.lift(K<M, Fin<A>>)`               | static      | named twin of the constructor                         |
|  [06]   | `FinT.liftIO(IO<A>)`                    | static      | lift from `IO<A>` under `MonadIO<M>`                  |
|  [07]   | `FinT.liftIO(IO<Fin<A>>)`               | static      | lift from `IO<Fin<A>>` directly                       |
|  [08]   | `FinT.runFin`                           | property    | the `K<M, Fin<A>>` result                             |
|  [09]   | `FinT.Bind` / `SelectMany` overload set | instance    | binds `FinT`, `K<FinT<M>,B>`, `K<M,B>`, bare `Fin<B>` |
|  [10]   | `FinT.Match(Succ, Fail)` / `MapFail`    | instance    | `K<M, B>` fold / failure map                          |

- `FinT.Bind` also binds `Pure<B>` and `Fail<Error>`. `SelectMany` binds `Pure<B>` but NOT `Fail<Error>`, and LINQ query syntax lowers to `SelectMany`. A bare `Fail<Error>` step inside a `FinT` query needs an explicit lift.

[MEMBER_SCOPE]: `Writer<W, A>` — monoidal output accumulated beside a value, `W : Monoid<W>` throughout

| [INDEX] | [MEMBER]                                                    | [KIND]   | [DESCRIPTION]                             |
| :-----: | :---------------------------------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `Writer.pure<W, A>(A)`                                      | static   | value with the empty output               |
|  [02]   | `Writer.tell<W>(W item)`                                    | static   | emit output, value is `Unit`              |
|  [03]   | `Writer.write<W, A>(A value, W item)`                       | static   | emit value and output together            |
|  [04]   | `Writer.listen(Writer<W, A>)`                               | static   | surface the accumulated output as a value |
|  [05]   | `Writer.listens(Func<W, B>, Writer<W, A>)`                  | static   | surface a projection of the output        |
|  [06]   | `Writer.censor(Func<W, W>, Writer<W, A>)`                   | static   | rewrite the output, value untouched       |
|  [07]   | `Writer.pass(Writer<W, (A, Func<W, W>)>)`                   | static   | the step supplies its own output rewriter |
|  [08]   | `Writer.Run() -> (A Value, W Output)`                       | instance | the only extraction, total                |
|  [09]   | `Writer.Listen()` / `Listens(Func<W, B>)` / `Censor`        | instance | the instance forms of rows [04]–[06]      |
|  [10]   | `Writer.Bind` / `SelectMany` / `Map`                        | instance | LINQ composition, outputs `Combine`       |
|  [11]   | `Writer.Write(A, W)` / `Write((A Value, W Output))`         | instance | append output to a running computation    |
|  [12]   | `Tell<W>.ToWriter()` / `ToWriterT<M>()` / `ToWritable<M>()` | instance | the literal converted to each type        |
|  [13]   | `WriterT<W, M, A>`                                          | record   | the same accumulation over any `Monad<M>` |

- `W : Monoid<W>` is the entire contract. Each bind `Combine`s the two outputs, the accumulator IS the monoid and no mutable list is threaded beside the computation. A `Seq<A>` output makes the writer an append-only log, and an `Error` output makes it a warning channel that never fails.
- `Writer<W, A>` publishes NO failure branch. It accumulates without failing, which separates it from `Validation<F, A>`: `Validation` accumulates failures and fails at the fold, `Writer` accumulates output and always succeeds. A computation that needs both stacks them as `WriterT<W, Fin, A>` instead of folding the output into the failure channel.
- `Run()` is the only exit and it is total. The output is otherwise unreadable mid-computation except through `Listen`/`Listens`, which return it as a VALUE. A later step can branch on what has accumulated. `Censor` and `Pass` are the two rewrite forms. `Censor` decides the rewrite from outside. `Pass` lets the step itself carry the rewriter in its value position.
- `Tell<W>` is the type-agnostic output literal beside `Pure<A>` and `Fail<E>`, a `tell` step binds inside a `Writer`, a `WriterT`, an `RWST`, or any `Writable<M, W>` body without naming the concrete type.

[MEMBER_SCOPE]: `Seq`, `Arr`, `HashMap`, `Set` — immutable collections

| [INDEX] | [MEMBER]                                                           | [KIND]   | [DESCRIPTION]                             |
| :-----: | :----------------------------------------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `Prelude.Seq(A, A)`                                                | static   | ordered-sequence construction             |
|  [02]   | `Prelude.toSeq(IEnumerable<A>)`                                    | static   | construction from `IEnumerable<A>`        |
|  [03]   | `Seq.Map(Func<A,B>)`                                               | instance | element projection                        |
|  [04]   | `Seq.Map(Func<A,int,B>)`                                           | instance | indexed `(value, index)` projection       |
|  [05]   | `LanguageExt.Seq.map(Seq<A>, Func<int,A,B>)`                       | static   | indexed `(index, value)` twin             |
|  [06]   | `Seq.Bind(Func<A,Seq<B>>)`                                         | instance | monadic expansion                         |
|  [07]   | `Seq.Filter(Func<A,bool>)`                                         | instance | predicate narrowing                       |
|  [08]   | `Seq.Partition(Func<A,bool>)`                                      | instance | one-pass two-way split                    |
|  [09]   | `Seq.Exists(Func<A,bool>)`                                         | instance | any-member predicate test                 |
|  [10]   | `Seq.ForAll(Func<A,bool>)`                                         | instance | every-member predicate test               |
|  [11]   | `SeqExtensions.Choose(Func<A,Option<B>>)`                          | static   | one-pass filter-map                       |
|  [12]   | `SeqExtensions.Choose(Func<int,A,Option<B>>)`                      | static   | indexed one-pass filter-map               |
|  [13]   | `SeqExtensions.Zip(Seq<B>, Func<A,B,C>)`                           | static   | projected pairwise join                   |
|  [14]   | `SeqExtensions.Scan(S, Func<S,A,S>)`                               | static   | running-state projection                  |
|  [15]   | `Seq.Head`                                                         | property | `Option<A>` first read                    |
|  [16]   | `Seq.Last`                                                         | property | `Option<A>` final read                    |
|  [17]   | `Seq.Tail`                                                         | property | all but the first member                  |
|  [18]   | `Seq.Init`                                                         | property | all but the final member                  |
|  [19]   | `Seq.Tails`                                                        | property | every suffix                              |
|  [20]   | `Seq.Inits`                                                        | property | every prefix                              |
|  [21]   | `Seq.Add(A)`                                                       | instance | append one member                         |
|  [22]   | `Seq.Concat(Seq<A>)`                                               | instance | cross-collection join                     |
|  [23]   | `Seq.Intersperse(A)`                                               | instance | separator weave                           |
|  [24]   | `Seq.Strict()`                                                     | instance | force a lazily-built sequence             |
|  [25]   | `Seq.AsSpan()`                                                     | instance | zero-copy contiguous read                 |
|  [26]   | `Seq.AsIterable()`                                                 | instance | lazy-view lift                            |
|  [27]   | `Seq.Traverse(Func<A,K<F,B>>)`                                     | instance | applicative shape inversion               |
|  [28]   | `Seq.TraverseM(Func<A,K<M,B>>)`                                    | instance | short-circuiting shape inversion          |
|  [29]   | `FoldableExtensions.Fold(S, Func<S,A,S>)`                          | fold     | type-generic state fold                   |
|  [30]   | `FoldableExtensions.FoldM` / `FoldBackM(S, Func<S,A,K<M,S>>)`      | fold     | monadic fold, tail-to-head / head-to-tail |
|  [31]   | `FoldableExtensions.FoldWhile(S, Func<S,A,S>, Func<(S,A),bool>)`   | fold     | predicate-bounded fold                    |
|  [32]   | `FoldableExtensions.FoldMap(Func<A,B>)`                            | fold     | monoidal aggregation                      |
|  [33]   | `FoldableExtensions.Find(Func<A,bool>)`                            | static   | `Option`-shaped search                    |
|  [34]   | `FoldableExtensions.FindAll(Func<A,bool>)`                         | static   | every match as a `Seq`                    |
|  [35]   | `Arr.create(A[])`                                                  | static   | immutable-array construction              |
|  [36]   | `Arr.createRange(IEnumerable<A>)`                                  | static   | construction from `IEnumerable<A>`        |
|  [37]   | `HashMap.Find(K)`                                                  | instance | `Option<V>` lookup                        |
|  [38]   | `HashMap.Find(K, Func<V,R>, Func<R>)`                              | instance | matched lookup fold                       |
|  [39]   | `HashMap.FindOrAdd(K, Func<V>)`                                    | instance | lookup with insert-on-miss                |
|  [40]   | `HashMap.Add(K, V)`                                                | instance | persistent insert                         |
|  [41]   | `HashMap.AddOrUpdate(K, Func<V,V>, Func<V>)`                       | instance | persistent matched upsert                 |
|  [42]   | `HashMap.AddOrUpdate(K, Func<V,V>, V)`                             | instance | matched upsert, value fills the miss      |
|  [43]   | `HashMap.SetItem(K, V)`                                            | instance | persistent replace                        |
|  [44]   | `HashMap.Remove(K)`                                                | instance | persistent delete                         |
|  [45]   | `HashMap.Union(IEnumerable<(K,V)>, WhenMatched<K,V,V,V>)`          | instance | merge with a collision rule               |
|  [46]   | `HashMap.ContainsKey(K)`                                           | instance | total key membership                      |
|  [47]   | `HashMap.ToTrackingHashMap()`                                      | instance | change-logged map lift                    |
|  [48]   | `HashMap.AddOrUpdate(K, V)`                                        | instance | persistent unconditional upsert           |
|  [49]   | `HashMap.AsIterable()`                                             | instance | `(K Key, V Value)` pair sequence          |
|  [50]   | `Set.Add(A)`                                                       | instance | persistent set insertion                  |
|  [51]   | `Set.TryAdd(A)`                                                    | instance | insertion tolerating a duplicate          |
|  [52]   | `IterableExtensions.AsIterable(IEnumerable<A>)`                    | static   | lazy sync lift                            |
|  [53]   | `IterableExtensions.AsIterable(IAsyncEnumerable<A>)`               | static   | lazy async lift                           |
|  [54]   | `Iterable<A>.FromSpan(ReadOnlySpan<A>)`                            | static   | construction from a `ReadOnlySpan<A>`     |
|  [55]   | `LanguageExt.List.unfold(S, Func<S,Option<(A,S)>>)`                | static   | state-seeded lazy generation              |
|  [56]   | `Prelude.toSet(IEnumerable<A>)`                                    | static   | ordered set from `IEnumerable<A>`         |
|  [57]   | `Set(IEnumerable<A>)`                                              | ctor     | ordered-set construction                  |
|  [58]   | `Prelude.toHashMap(IEnumerable<(K,V)>)`                            | static   | hashed map from pairs                     |
|  [59]   | `FoldableExtensions.Iter(Action<A>)`                               | static   | side-effecting element iteration          |
|  [60]   | `Seq.Skip(int)`                                                    | instance | drop a leading run                        |
|  [61]   | `Seq.Take(int)`                                                    | instance | keep a leading run                        |
|  [62]   | `Seq.Count`                                                        | property | materialized member count                 |
|  [63]   | `SeqExtensions.Rev(Seq<A>)`                                        | static   | reversed sequence                         |
|  [64]   | `Seq.TakeWhile(Func<A,bool>)`                                      | instance | predicate-bounded leading run             |
|  [65]   | `Seq.TakeWhile(Func<A,int,bool>)`                                  | instance | indexed predicate-bounded run             |
|  [66]   | `FoldableExtensions.FoldWhileM(S, Func<S,A,K<M,S>>, Func<A,bool>)` | fold     | monadic predicate-bounded fold            |
|  [67]   | `FoldableExtensions.FoldUntilM(S, Func<S,A,K<M,S>>, Func<A,bool>)` | fold     | monadic fold to a stop condition          |
|  [68]   | `Prelude.foldWhileM(f, pred, state, ta)`                           | fold     | the argument-flipped module twin          |
|  [69]   | `FoldableExtensions.FoldUntil(S, Func<S,A,S>, Func<(S,A),bool>)`   | fold     | pure fold to a stop condition             |
|  [70]   | `FoldableExtensions.FoldBackWhile` / `FoldBackUntil`               | fold     | the right-to-left bounded twins           |
|  [71]   | `FoldableExtensions.FoldMaybe(S, Func<S,A,Option<S>>) -> S`        | fold     | the folder itself decides the stop        |
|  [72]   | `FoldableExtensions.FoldMapWhileT` / `FoldMapUntilT`               | fold     | bounded monoidal aggregation, nested      |
|  [73]   | `FoldableExtensions.FoldT` / `FoldWhileT` / `FoldUntilT`           | fold     | one pass over `K<T, K<U, A>>`             |

- The bounded folds split their predicate arity, and the two look the same at the call site. The PURE `FoldWhile`/`FoldUntil` take `Func<(S State, A Value), bool>` — the running state AND the element. The MONADIC `FoldWhileM`/`FoldUntilM` take `Func<A, bool>` over the element ALONE. A state-reading stop condition has no monadic form. It either folds pure and lifts afterwards, or carries the condition into the effect and returns a state the next step reads. `foldWhileM` is the same operator with the arguments flipped to `(f, pred, state, ta)`, a mechanical rewrite between the instance and module forms silently transposes them.
- The monadic fold pair is DIRECTION-SWAPPED against the pure pair. `Fold` walks head-to-tail, but `FoldM` walks TAIL-TO-HEAD (a string-append fold over `[1, 2, 3]` answers `321`) while `FoldBackM` walks head-to-tail (`123`). An order-dependent monadic fold over an ascending sequence uses `FoldBackM`. A `FoldM` there silently feeds the step its input reversed.
- `FoldMaybe` is the fold whose STOP lives in the folder rather than beside it. The step answers `Option<S>` and a `None` ends the traversal, returning the last committed state. It is the form a search-and-accumulate takes when the decision to continue is the same computation as the accumulation. `FoldMaybe` has a `FoldBackMaybe` right-to-left twin but no `*T` twin. Only the `Fold`/`FoldWhile`/`FoldUntil` family carries `FoldT`/`FoldWhileT`/`FoldUntilT`, which fold one pass over a nested `K<T, K<U, A>>`. A foldable of foldables never flattens first.
- `Seq<A>` carries the throwing `this[Index]` as its only instance index member. The `Option`-returning positional read is `FoldableExtensions.At(K<T, A>, Index) : Option<A>`, which applies to `Seq` through `Foldable<Seq>`. `seq.At(n)` answers `None` past the end.
- `LanguageExt.List.unfold` runs the state seed until the unfolder answers `None`. The static import of `Prelude` binds the simple name `List` to `Prelude.List<T>()`, the call is spelled `LanguageExt.List.unfold`. Five overloads exist. The state-only `Func<S, Option<S>>` overload returns `IEnumerable<S>`. The other four return `IEnumerable<A>`: one takes a plain `S` seed, and the other three take a two-, three-, or four-slot tuple seed.

[MEMBER_SCOPE]: `TrackingHashMap<K, V>` — the change-logged map (`HashMap.ToTrackingHashMap()` lifts into it). `TrackingHashMap<EqK, K, V>` is the same surface with an explicit `EqK` equality trait type parameter, and the static `TrackingHashMap` module creates one (`empty`/`create`/`createRange`/`singleton` over `(K, V)` tuples, `KeyValuePair`s, or a `ReadOnlySpan`) where no source map exists to lift.

| [INDEX] | [MEMBER]                                                          | [KIND]   | [DESCRIPTION]                                       |
| :-----: | :---------------------------------------------------------------- | :------- | :-------------------------------------------------- |
|  [01]   | `TrackingHashMap.Changes`                                         | property | `HashMap<K, Change<V>>` log since the last snapshot |
|  [02]   | `TrackingHashMap.Snapshot()`                                      | instance | zeroes the change log, holds the data               |
|  [03]   | `TrackingHashMap.AddOrUpdate(K, V)`                               | instance | logged unconditional upsert                         |
|  [04]   | `TrackingHashMap.AddOrUpdate(K, Func<V,V>, Func<V>)`              | instance | logged matched upsert, `Func<V>` fills the miss     |
|  [05]   | `TrackingHashMap.AddOrUpdate(K, Func<V,V>, V)`                    | instance | logged matched upsert, value fills the miss         |
|  [06]   | `TrackingHashMap.AddOrUpdateRange(IEnumerable<(K Key, V Value)>)` | instance | logged bulk upsert                                  |
|  [07]   | `TrackingHashMap.Remove(K)`                                       | instance | logged delete                                       |
|  [08]   | `TrackingHashMap.Find(K)`                                         | instance | `Option<V>` lookup, logs nothing                    |
|  [09]   | `TrackingHashMap.ContainsKey(K)`                                  | instance | total key membership                                |
|  [10]   | `TrackingHashMap.TryGetValue(K, out V)`                           | instance | `IReadOnlyDictionary` lookup                        |
|  [11]   | `TrackingHashMap.ToHashMap()`                                     | instance | drops the log, holds the data                       |

[MEMBER_SCOPE]: state, optics, and `Prelude` members

| [INDEX] | [MEMBER]                                 | [KIND]   | [DESCRIPTION]                         |
| :-----: | :--------------------------------------- | :------- | :------------------------------------ |
|  [01]   | `Prelude.Atom(A, Func<A,bool>)`          | static   | validated lock-free reference         |
|  [02]   | `Atom.Value`                             | property | current-state snapshot read           |
|  [03]   | `Atom.ValueIO`                           | property | the same read as an `IO<A>`           |
|  [04]   | `Atom.Swap(Func<A,A>) -> A`              | instance | CAS update, post-state return         |
|  [05]   | `Atom.SwapMaybe(Func<A,Option<A>>) -> A` | instance | CAS update, `None` keeps the state    |
|  [06]   | `Atom.SwapIO(Func<A,A>)`                 | instance | CAS update as an `IO`                 |
|  [07]   | `Atom.Change`                            | event    | accepted-swap notification            |
|  [08]   | `Prelude.AtomHashMap(HashMap<K,V>)`      | static   | lock-free map construction            |
|  [09]   | `Prelude.Ref(A, Func<A,bool>)`           | static   | transactional reference               |
|  [10]   | `Prelude.atomic(Func<R>, Isolation)`     | static   | multi-`Ref` transaction               |
|  [11]   | `Prelude.swap(Ref<A>, Func<A,A>)`        | static   | in-transaction update                 |
|  [12]   | `Prelude.commute(Ref<A>, Func<A,A>)`     | static   | order-free in-transaction update      |
|  [13]   | `Lens.New(Func<A,B>, Func<B,Func<A,A>>)` | static   | lens construction                     |
|  [14]   | `Lens.Set(B, A)`                         | instance | immutable focused write               |
|  [15]   | `Lens.Update(Func<B,B>, A)`              | instance | immutable focused edit                |
|  [16]   | `Lens.fst<A,B>()`                        | static   | first-item tuple lens                 |
|  [17]   | `Lens.snd<A,B>()`                        | static   | second-item tuple lens                |
|  [18]   | `Lens.tuple(Lens<A,C>, Lens<B,D>)`       | static   | composed tuple lens                   |
|  [19]   | `Seq<A>.headOrNone`                      | property | first-item lens over a `Seq`          |
|  [20]   | `Seq<A>.lastOrNone`                      | property | final-item lens over a `Seq`          |
|  [21]   | `Prelude.memo(Func<A,B>)`                | static   | memoized pure function                |
|  [22]   | `Prelude.memo(Func<A>)`                  | static   | memoized nullary thunk                |
|  [23]   | `Prelude.memo(IEnumerable<A>)`           | static   | replay-cached lazy enumeration        |
|  [24]   | `Prelude.memoUnsafe(Func<A,B>)`          | static   | unsynchronized memo table             |
|  [25]   | `Prelude.memoK(Func<K<F,A>>)`            | static   | caches the `K<F,A>` CONSTRUCTION      |
|  [26]   | `Prelude.memoK(K<F,A>)` / `memoK(A)`     | static   | preloaded memo over an existing value |
|  [27]   | `Memo.Reset()`                           | instance | drop a memoized value                 |
|  [28]   | `Range.fromMinMax(A, A, A)`              | static   | generated bounded sequence            |
|  [29]   | `Prelude.Range(int\|long from, count)`   | static   | `Range<A>` from origin and count      |
|  [30]   | `Prelude.unit`                           | property | the `Unit` literal                    |
|  [31]   | `Prelude.identity(A)`                    | static   | the identity function                 |

- `memoK` caches the CONSTRUCTION of a `K<F, A>`, never its execution. A memoized `IO` or `Eff` is built once and then runs on every call, a `memoK` effect is not a cached RESULT. To cache a result, memoize past the run (`memo` over the executed value). The `memoK(K<F,A>)` and `memoK(A)` arities are the preloaded forms where the value already exists.
- `memo(IEnumerable<A>)` retains each item as it is first enumerated, a second traversal replays from the cache and an expensive generator runs once. It is the lazy counterpart to forcing into a `Seq`, where forcing pays the whole cost up front.

[MEMBER_SCOPE]: `AtomHashMap<K, V>` — the lock-free map (`Prelude.AtomHashMap(…)` or `AtomHashMap.ToAtom()` creates one). Every mutation returns `Unit`, publishes on `Change`, and commits under a CAS retry loop. `AtomHashMap<EqK, K, V>` is the same surface with an explicit `EqK` equality trait type parameter.

| [INDEX] | [MEMBER]                                                  | [KIND]   | [DESCRIPTION]                                     |
| :-----: | :-------------------------------------------------------- | :------- | :------------------------------------------------ |
|  [01]   | `Swap(Func<TrackingHashMap<K,V>, TrackingHashMap<K,V>>)`  | instance | whole-map CAS update seeing its own change log    |
|  [02]   | `SwapKey(K, Func<V, V>)`                                  | instance | one-key CAS update                                |
|  [03]   | `SwapKey(K, Func<Option<V>, Option<V>>)`                  | instance | one-key CAS covering insert, edit, and delete     |
|  [04]   | `Add(K, V)` / `TryAdd(K, V)`                              | instance | insert, throwing or tolerating a duplicate        |
|  [05]   | `AddOrUpdate(K, V)` / `(K, Func<V,V>, Func<V>)`           | instance | upsert, unconditional or matched                  |
|  [06]   | `SetItem(K, V)` / `SetItem(K, Func<V,V>)`                 | instance | replace an existing key                           |
|  [07]   | `TrySetItem(K, V)` / `TrySetItems(IEnumerable<K>, …)`     | instance | replace only where the key is present             |
|  [08]   | `AddRange` / `AddOrUpdateRange` / `TryAddRange`           | instance | one commit per bulk write                         |
|  [09]   | `Remove(K)` / `RemoveRange(IEnumerable<K>)`               | instance | delete one key or a set of keys                   |
|  [10]   | `Clear()`                                                 | instance | drop every entry                                  |
|  [11]   | `FilterInPlace(Func<K,V,bool>)` / `MapInPlace(Func<V,V>)` | instance | narrow or reshape without rebuilding              |
|  [12]   | `Append` / `Subtract` / `Except` / `SymmetricExcept`      | instance | set algebra against a map or a pair sequence      |
|  [13]   | `Union(rhs, WhenMatched)` / `Intersect(rhs, WhenMatched)` | instance | merge under an explicit collision rule            |
|  [14]   | `Change`                                                  | event    | one `HashMapPatch<K,V>` per accepted commit       |
|  [15]   | `Find(K)` / `FindOrMaybeAdd(K, Func<Option<V>>)`          | instance | `Option<V>` read, optionally seeding on a miss    |
|  [16]   | `ToHashMap()` / `ToSeq()` / `AsIterable()`                | instance | immutable snapshot at the read                    |
|  [17]   | `Fold(S, Func<S,K,V,S>)` / `Iter(Action<K,V>)`            | fold     | key-and-value iteration over a snapshot           |
|  [18]   | `HashMapPatch.From` / `To` / `Changes`                    | property | the two snapshots and the `HashMap<K, Change<V>>` |

- `AtomHashMap<K, V>` is the ONE type where mutation is in place and the value is shared. `Atom<HashMap<K,V>>` would make every keyed write a whole-map `Swap` returning a new map, while `SwapKey` commits one key under the same CAS discipline. The cost is that a mutation returns `Unit`, nothing about the commit is readable from the return value. Read the result through `Change` or a later `Find`.
- `Swap` hands the transition function a `TrackingHashMap<K, V>`, a whole-map update can read the deltas it is itself producing and decide from them. The emitted `HashMapPatch.Changes` is built from that log.
- `Change` fires ONCE per accepted commit with a `HashMapPatch<K, V>` carrying `From`, `To`, and a `HashMap<K, Change<V>>` of per-key deltas, which makes the type an observable keyed store rather than a mutable dictionary that a watcher polls. A bulk member commits one patch covering every touched key, a range write is one notification and not one per entry.
- Every mutating member re-runs its transition inside the CAS loop exactly as `Atom<A>.Swap` does, the same restriction on side effects applies: a dispose, a counter increment, or a log inside a swap runs once per failed attempt.

[MEMBER_SCOPE]: `Deriving.*` defaults declared on the `Supertype` — `Transform` unwraps to the `Subtype`, `CoTransform` rewraps, and every member below routes through that pair

| [INDEX] | [MEMBER]                                                              | [KIND]   | [DESCRIPTION]                                      |
| :-----: | :-------------------------------------------------------------------- | :------- | :------------------------------------------------- |
|  [01]   | `Transform(K<Supertype,A>) -> K<Subtype,A>`                           | static   | unwrap to the inner type, hand-written             |
|  [02]   | `CoTransform(K<Subtype,A>) -> K<Supertype,A>`                         | static   | rewrap the inner result, hand-written              |
|  [03]   | `Map(Func<A,B>, K<Supertype,A>)`                                      | static   | `Functor` projection                               |
|  [04]   | `Pure(A)`                                                             | static   | `Applicative` lift                                 |
|  [05]   | `Apply(K<Supertype,Func<A,B>>, K<Supertype,A>)`                       | static   | fan-in, `Memo` arity beside it                     |
|  [06]   | `Action(K<Supertype,A>, K<Supertype,B>)`                              | static   | sequence, keeping the right                        |
|  [07]   | `Bind(K<Supertype,A>, Func<A,K<Supertype,B>>)`                        | static   | `Monad` chain                                      |
|  [08]   | `Flatten(K<Supertype,K<Supertype,A>>)`                                | static   | `Monad` join                                       |
|  [09]   | `Recur(A, Func<A,K<Supertype,Next<A,B>>>)`                            | static   | tail-recursive loop                                |
|  [10]   | `LiftIO(IO<A>)` / `LiftIO(K<IO,A>)`                                   | static   | `MonadIO` lift                                     |
|  [11]   | `ToIO(K<Supertype,A>)` / `MapIO(K<Supertype,A>, Func<IO<A>,IO<B>>)`   | static   | `MonadUnliftIO` window                             |
|  [12]   | `BracketIO(K<Supertype,A>, Func<A,IO<C>>, Func<A,IO<B>>)`             | static   | acquire-use-release, `Catch` arity beside it       |
|  [13]   | `RepeatIO(K<Supertype,A>, Schedule)`                                  | static   | cadence; `RepeatWhileIO`/`RepeatUntilIO` alongside |
|  [14]   | `RetryIO(K<Supertype,A>, Schedule)`                                   | static   | retry; `RetryWhileIO`/`RetryUntilIO` alongside     |
|  [15]   | `FoldIO(K<Supertype,A>, Schedule, S, Func<S,A,S>)`                    | static   | streaming fold; `FoldWhileIO`/`FoldUntilIO` beside |
|  [16]   | `LocalIO(K)` / `PostIO(K)` / `TimeoutIO(K, TimeSpan)`                 | static   | `IO` scope, ordering, and deadline                 |
|  [17]   | `Fail(E)`                                                             | static   | `Fallible` raise                                   |
|  [18]   | `Catch(K<Supertype,A>, Func<E,bool>, Func<E,K<Supertype,A>>)`         | static   | `Fallible` predicate recovery                      |
|  [19]   | `Finally(K<Supertype,A>, K<Supertype,X>)`                             | static   | `Final` teardown                                   |
|  [20]   | `Choose(K<Supertype,A>, K<Supertype,A>)`                              | static   | first-success, `Memo` arity beside it              |
|  [21]   | `Combine(K<Supertype,A>, K<Supertype,A>)`                             | static   | `SemigroupK` join                                  |
|  [22]   | `Empty()`                                                             | static   | `MonoidK` identity                                 |
|  [23]   | `Ask`                                                                 | property | `Readable` environment read                        |
|  [24]   | `Asks(Func<Env,A>)` / `Local(Func<Env,Env>, K<Supertype,A>)`          | static   | environment projection and shadowing               |
|  [25]   | `Get`                                                                 | property | `Stateful` state read                              |
|  [26]   | `Put(S)` / `Modify(Func<S,S>)` / `Gets(Func<S,A>)`                    | static   | state write and projection                         |
|  [27]   | `Tell(W)` / `Listen(K<Supertype,A>)`                                  | static   | `Writable` output and its capture                  |
|  [28]   | `Pass(K<Supertype,(A,Func<W,W>)>)`                                    | static   | rewrite the captured output                        |
|  [29]   | `Lift(K<M,A>)`                                                        | static   | `MonadT` inner-type lift                           |
|  [30]   | `Traverse(Func<A,K<F,B>>, K<Supertype,A>)` / `TraverseM`              | static   | effect and shape inversion                         |
|  [31]   | `Sequence(K<Supertype,K<F,A>>)` / `SequenceM`                         | static   | inversion with the map already applied             |
|  [32]   | `TraverseDefault(Func<A,K<F,B>>, K<Supertype,A>)`                     | static   | `Traverse` through `Sequence`                      |
|  [33]   | `Comap(Func<A,B>, K<Supertype,B>)`                                    | static   | `Cofunctor` contramap                              |
|  [34]   | `Divide(Func<A,(B,C)>, K<Supertype,B>, K<Supertype,C>)` / `Conquer()` | static   | `Divisible` split and unit                         |
|  [35]   | `Lose(Func<A,Void>)` / `Route(Func<A,Either<B,C>>, K, K)`             | static   | `Decidable` absurd and branch                      |
|  [36]   | `Fold(Func<A,Func<S,S>>, S, K<Supertype,A>)`                          | static   | `Foldable` fold, forwarded to the `Subtype`        |

- `Transform` and `CoTransform` are the only required members. A wrapper declaring `Deriving.MonadUnliftIO<Supertype, Subtype>`, `Deriving.Fallible<Supertype, Subtype>`, and `Deriving.Final<Supertype, Subtype>` compiles with those two members alone. `Deriving.Alternative` carries no body of its own — it composes the `Choice` and `Applicative` derivations. `Alternative<Supertype>.Empty` stays abstract. The wrapper must implement it, or the compiler reports `CS0535`.
- `Supertype` heads every parameter list while `Subtype` moves. `Deriving.Readable<Supertype, Env, Subtype>` places it LAST. `Deriving.Stateful<Supertype, Subtype, S>`, `Deriving.Writable<Supertype, Subtype, W>`, and `Deriving.MonadT<Supertype, Subtype, M>` place it SECOND. `Deriving.Fallible<E, Supertype, Subtype>` puts the failure type FIRST, and `Deriving.Fallible<Supertype, Subtype>` is that interface with `E` fixed to `Error`.
- `LanguageExt.Deriving` is a static CLASS, `using LanguageExt.Deriving;` fails with `CS0138` and every derivation writes `Deriving.Monad<Supertype, Subtype>` through the `LanguageExt` namespace import. `LanguageExt.Traits.Deriving<Supertype, Subtype>` is a separate arity-two interface aliasing `NaturalIso<Supertype, Subtype>`. The arity difference resolves both names unqualified with both namespaces imported.
