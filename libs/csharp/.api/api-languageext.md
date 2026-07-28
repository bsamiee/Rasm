# [RASM_API_LANGUAGEEXT]

`LanguageExt.Core` is the branch functional substrate: `Fin<A>` is the rail every domain operation returns, and every other carrier — presence, accumulation, deferral, collection, cell, optic — names its conversion onto that rail. Its higher-kinded trait system makes one `Apply` fan-in, one `Traverse` inversion, and one operator set work across every carrier, so a new rail is a trait conformance rather than a new combinator family.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `LanguageExt.Core`
- package: `LanguageExt.Core` (MIT)
- assembly: `LanguageExt.Core` (`lib/net10.0`)
- namespace: `LanguageExt`, `LanguageExt.Common`, `LanguageExt.Traits`
- asset: pure managed library; `using static LanguageExt.Prelude;` carries the constructor vocabulary
- abi: every carrier implements `K<Self, A>`, so one trait extension binds uniformly across rails, collections, and transformers
- rail: functional substrate

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: result, validation, and effect rails

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]   | [CAPABILITY]                              |
| :-----: | :----------------- | :-------------- | :---------------------------------------- |
|  [01]   | `Fin<A>`           | abstract class  | short-circuiting `Succ`/`Fail` result     |
|  [02]   | `Validation<F, A>` | abstract record | accumulating verdict over `F : Monoid<F>` |
|  [03]   | `Option<A>`        | readonly struct | presence with nullable lift               |
|  [04]   | `Either<L, R>`     | abstract record | disjoint union crossing to `Fin`          |
|  [05]   | `Try<A>`           | record          | `Func<Fin<A>>` exception trap             |
|  [06]   | `Eff<A>`           | record          | runtime-free deferred effect              |
|  [07]   | `Eff<RT, A>`       | record          | reader-runtime deferred effect            |
|  [08]   | `IO<A>`            | abstract record | terminal effect with bracket and schedule |
|  [09]   | `Error`            | abstract record | `Monoid<Error>` failure vocabulary        |
|  [10]   | `Expected`         | record          | expected failure keyed by `Code`          |
|  [11]   | `Exceptional`      | record          | exception-derived failure                 |
|  [12]   | `ManyErrors`       | sealed record   | accumulated failure carrier               |
|  [13]   | `Guard<E, A>`      | readonly struct | predicate gate composing in a LINQ body   |
|  [14]   | `Pure<A>`          | record struct   | rail-agnostic success literal             |
|  [15]   | `Fail<E>`          | record struct   | rail-agnostic failure literal             |
|  [16]   | `CatchM<E, M, A>`  | record struct   | predicate-selected recovery handler       |

[PUBLIC_TYPE_SCOPE]: immutable carriers, state, and optics

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]   | [CAPABILITY]                             |
| :-----: | :---------------------- | :-------------- | :--------------------------------------- |
|  [01]   | `Seq<A>`                | readonly struct | default ordered carrier with `AsSpan`    |
|  [02]   | `Arr<A>`                | readonly struct | indexed immutable array                  |
|  [03]   | `Lst<A>`                | readonly struct | persistent linked list                   |
|  [04]   | `HashMap<K, V>`         | readonly struct | hashed persistent map                    |
|  [05]   | `Map<K, V>`             | readonly struct | ordered persistent map                   |
|  [06]   | `TrackingHashMap<K, V>` | readonly struct | map carrying its own change log          |
|  [07]   | `HashSet<A>`            | readonly struct | hashed persistent set                    |
|  [08]   | `Set<A>`                | readonly struct | ordered persistent set                   |
|  [09]   | `Stck<A>`               | readonly struct | persistent LIFO stack                    |
|  [10]   | `Que<A>`                | readonly struct | persistent FIFO queue                    |
|  [11]   | `Iterable<A>`           | abstract class  | lazy sync or async sequence              |
|  [12]   | `Atom<A>`               | sealed class    | lock-free CAS cell with `Change`         |
|  [13]   | `Atom<M, A>`            | sealed class    | CAS cell threading construction metadata |
|  [14]   | `Ref<A>`                | sealed class    | transactional cell `atomic` commits      |
|  [15]   | `Memo<A>`               | class           | resettable memoized thunk                |
|  [16]   | `Lens<A, B>`            | readonly struct | composable get and immutable set         |
|  [17]   | `Range<A>`              | record          | generated bounded sequence               |
|  [18]   | `AtomChangedEvent<A>`   | delegate        | `Atom.Change` handler over the new value |

[PUBLIC_TYPE_SCOPE]: traits and monad transformers (`LanguageExt.Traits`)

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]   | [CAPABILITY]                                |
| :-----: | :--------------------- | :-------------- | :------------------------------------------ |
|  [01]   | `K<F, A>`              | interface       | higher-kinded seam every carrier implements |
|  [02]   | `Functor<F>`           | interface       | `Map` conformance                           |
|  [03]   | `Applicative<F>`       | interface       | `Apply` fan-in conformance                  |
|  [04]   | `Monad<M>`             | interface       | `Bind` and tail-recursive `Recur`           |
|  [05]   | `MonadIO<M>`           | interface       | `IO` lifting into a carrier                 |
|  [06]   | `Semigroup<A>`         | interface       | associative `Combine`                       |
|  [07]   | `Monoid<A>`            | interface       | `Combine` with an identity                  |
|  [08]   | `Foldable<T>`          | interface       | fold, search, and aggregate conformance     |
|  [09]   | `Traversable<T>`       | interface       | effect and shape inversion                  |
|  [10]   | `Alternative<F>`       | interface       | first-success choice                        |
|  [11]   | `Fallible<E, F>`       | interface       | typed failure raise and recover             |
|  [12]   | `Readable<M, Env>`     | interface       | ambient-environment reads                   |
|  [13]   | `Stateful<M, S>`       | interface       | threaded-state reads and writes             |
|  [14]   | `ReaderT<Env, M, A>`   | record          | environment threaded over any `M`           |
|  [15]   | `StateT<S, M, A>`      | record          | state threaded over any `M`                 |
|  [16]   | `WriterT<W, M, A>`     | record          | monoidal output over any `M`                |
|  [17]   | `RWST<R, W, S, M, A>`  | record          | reader, writer, and state in one pass       |
|  [18]   | `FinT<M, A>`           | record          | `Fin` stacked over any `M`                  |
|  [19]   | `OptionT<M, A>`        | record          | `Option` stacked over any `M`               |
|  [20]   | `EitherT<L, M, A>`     | record          | `Either` stacked over any `M`               |
|  [21]   | `ValidationT<F, M, A>` | record          | `Validation` stacked over any `M`           |
|  [22]   | `Free<F, A>`           | abstract record | open interpreter over a functor             |
|  [23]   | `Schedule`             | abstract record | composable repeat and retry policy          |

## [03]-[ENTRYPOINTS]

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

| [INDEX] | [SURFACE]                                          | [SHAPE]  | [CAPABILITY]                    |
| :-----: | :------------------------------------------------- | :------- | :------------------------------ |
|  [01]   | `Prelude.Some(A)`                                  | static   | present-value construction      |
|  [02]   | `Prelude.Optional(A?)`                             | static   | nullable-aware admission        |
|  [03]   | `Option<A>.None`                                   | property | absent literal                  |
|  [04]   | `Option.Match(Func<A,B>, Func<B>)`                 | instance | total presence fold             |
|  [05]   | `Option.IfNone(A)`                                 | instance | default escape                  |
|  [06]   | `Option.Filter(Func<A,bool>)`                      | instance | predicate narrowing             |
|  [07]   | `Option.Bind(Func<A,Option<B>>)`                   | instance | monadic chain                   |
|  [08]   | `Option.ToFin(Error)`                              | instance | rail ingress                    |
|  [09]   | `Option.ToValidation(L)`                           | instance | accumulation ingress            |
|  [10]   | `Option.ToSeq()`                                   | instance | collection egress               |
|  [11]   | `Option.ToEither(L)`                               | instance | disjoint-union egress           |
|  [12]   | `Option.TraverseM(Func<A,K<M,B>>)`                 | instance | absence-total effect inversion  |
|  [13]   | `OptionExtensions.Somes(Seq<Option<A>>)`           | static   | drop absent members in one pass |
|  [14]   | `Prelude.guard(bool, Error)`                       | static   | predicate refusal literal       |
|  [15]   | `FinGuardExtensions.ToFin(Guard<Error,Unit>)`      | static   | standalone gate to the rail     |
|  [16]   | `FinGuardExtensions.SelectMany(Func<Unit,Fin<B>>)` | static   | gate as a LINQ `from` clause    |

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
|  [11]   | `ValidationExtensions.ToFin(Validation<Error,A>)` | static   | short-circuit rail egress          |
|  [12]   | `ValidationExtensions.As(K<Validation<F>,A>)`     | static   | trait-value re-anchor              |
|  [13]   | `ValidationExtensions.Successes()`                | static   | accepted branch of a roster        |
|  [14]   | `ValidationExtensions.Fails()`                    | static   | refused branch of a roster         |
|  [15]   | `Validation operator \|`                          | operator | failure-accumulating choice        |
|  [16]   | `ApplicativeExtensions.Apply(tuple, Func<A,B,R>)` | static   | K-kinded fan-in, arities 2–10      |
|  [17]   | `Error.New(int, string)`                          | static   | typed-failure construction         |
|  [18]   | `Error.Many(Seq<Error>)`                          | static   | accumulated-failure carrier        |
|  [19]   | `Error.Combine(Error)`                            | instance | monoidal failure join              |
|  [20]   | `Error operator +`                                | operator | terse monoidal failure join        |
|  [21]   | `Error.Head`                                      | property | first accumulated failure          |
|  [22]   | `Error.Tail`                                      | property | remaining accumulated failures     |
|  [23]   | `Error.Count`                                     | property | accumulated-failure cardinality    |
|  [24]   | `Error.AsIterable()`                              | instance | accumulated-failure enumeration    |
|  [25]   | `Error.Is(Error)`                                 | instance | failure identity test              |
|  [26]   | `Error.IsType<E>()`                               | instance | failure type test                  |
|  [27]   | `Error.HasCode(int)`                              | instance | failure code test                  |
|  [28]   | `Error.Filter<E>()`                               | instance | failure-subset selection           |
|  [29]   | `Error.Exception`                                 | property | optional exceptional payload       |
|  [30]   | `Error.Inner`                                     | property | optional cause chain               |
|  [31]   | `Error.ToException()`                             | instance | host-boundary projection           |
|  [32]   | `Error.Throw<R>()`                                | instance | host-boundary escape               |
|  [33]   | `Errors.Cancelled`                                | static   | token-trip identity, `-2000000001` |
|  [34]   | `Errors.TimedOut`                                 | static   | `Timeout` expiry, `-2000000002`    |

`Errors` seats the package's own failure identities so `Error.Is` classifies a cooperative token trip apart from a deadline cut; a message match over either re-classifies on any rephrasing.

[ENTRYPOINT_SCOPE]: `Try`, `Eff`, `IO` — the deferred tiers

| [INDEX] | [SURFACE]                                                     | [SHAPE]  | [CAPABILITY]                         |
| :-----: | :------------------------------------------------------------ | :------- | :----------------------------------- |
|  [01]   | `Try.lift(Func<A>)`                                           | static   | exception-trapping thunk             |
|  [02]   | `TryExtensions.Run(K<Try,A>)`                                 | static   | force the thunk to `Fin<A>`          |
|  [03]   | `Try.ToFin()`                                                 | instance | rail conversion                      |
|  [04]   | `Try.ToIO()`                                                  | instance | terminal-tier conversion             |
|  [05]   | `Eff.lift(Func<A>)`                                           | static   | effect admission                     |
|  [06]   | `Prelude.liftEff(Func<Task<Fin<A>>>)`                         | static   | async fallible effect admission      |
|  [07]   | `Eff.runtime<RT>() -> Eff<RT, RT>`                            | static   | supplied-runtime reader effect       |
|  [08]   | `Eff.getState<RT>()`                                          | static   | runtime and `EnvIO` read             |
|  [09]   | `Eff.local(Func<OuterRT,InnerRT>, Eff<InnerRT,A>)`            | static   | scoped runtime override              |
|  [10]   | `Eff.localCancel(Eff<RT,A>)`                                  | static   | scoped cancellation source           |
|  [11]   | `EffExtensions.Run(K<Eff,A>)`                                 | static   | typed execution to `Fin<A>`          |
|  [12]   | `EffExtensions.RunAsync(K<Eff,A>)`                            | static   | `Task<Fin<A>>` execution             |
|  [13]   | `EffExtensions.RunIO(K<Eff,A>)`                               | static   | lower to the terminal `IO` tier      |
|  [14]   | `Eff.MapFail(Func<Error,Error>)`                              | instance | failure projection                   |
|  [15]   | `Eff.MapIO(Func<IO<A>,IO<B>>)`                                | instance | inner-effect projection              |
|  [16]   | `Eff.IfFailEff(Func<Error,Eff<A>>)`                           | instance | effectful recovery                   |
|  [17]   | `IO.pure(A)`                                                  | static   | lifted-value construction            |
|  [18]   | `IO.fail(Error)`                                              | static   | failed-effect construction           |
|  [19]   | `IO.lift(Func<A>)`                                            | static   | thunk admission                      |
|  [20]   | `IO.lift(Func<Fin<A>>)`                                       | static   | railed thunk onto the error channel  |
|  [21]   | `IO.lift(Fin<A>)`                                             | static   | settled rail lifted whole            |
|  [22]   | `IO.liftAsync(Func<Task<A>>)`                                 | static   | `Task` thunk admission               |
|  [23]   | `IO.liftVAsync(Func<ValueTask<A>>)`                           | static   | `ValueTask` thunk admission          |
|  [24]   | `IO.Run()`                                                    | instance | synchronous execution                |
|  [25]   | `IO.RunAsync()`                                               | instance | `ValueTask` execution                |
|  [26]   | `IO.Bracket(Func<A,IO<C>>, Func<A,IO<B>>)`                    | instance | acquire-use-release scope            |
|  [27]   | `IO.Bracket(Func<A,IO<C>>, Func<Error,IO<C>>, Func<A,IO<B>>)` | instance | scope with a failure arm             |
|  [28]   | `IO.Finally(K<IO,X>)`                                         | instance | unconditional release                |
|  [29]   | `IO.Repeat(Schedule)`                                         | instance | policy-driven repetition             |
|  [30]   | `IO.RepeatUntil(Func<A,bool>)`                                | instance | predicate-bounded repetition         |
|  [31]   | `IO.Retry(Schedule)`                                          | instance | policy-driven retry                  |
|  [32]   | `IO.RetryUntil(Func<Error,bool>)`                             | instance | predicate-bounded retry              |
|  [33]   | `IO.Fork(Option<TimeSpan>)`                                   | instance | concurrent execution handle          |
|  [34]   | `IO.Timeout(TimeSpan)`                                        | instance | bounded execution                    |
|  [35]   | `IO.Catch(Func<Error,bool>, Func<Error,K<IO,A>>)`             | instance | predicate-selected recovery          |
|  [36]   | `IO.Uninterruptible()`                                        | instance | cancellation masking                 |
|  [37]   | `Prelude.@catch(Func<Error,bool>, K<M,A>)`                    | static   | rail-generic recovery handler        |
|  [38]   | `Prelude.use(Func<A>, Action<A>)`                             | static   | resource-scoped acquisition          |
|  [39]   | `Prelude.tail(IO<A>)`                                         | static   | tail-recursion marker for deep binds |

[ENTRYPOINT_SCOPE]: `Seq`, `Arr`, `HashMap`, `Set` — immutable carriers

| [INDEX] | [SURFACE]                                                        | [SHAPE]  | [CAPABILITY]                        |
| :-----: | :--------------------------------------------------------------- | :------- | :---------------------------------- |
|  [01]   | `Prelude.Seq(A, A)`                                              | static   | ordered-carrier construction        |
|  [02]   | `Prelude.toSeq(IEnumerable<A>)`                                  | static   | enumerable admission                |
|  [03]   | `Seq.Map(Func<A,B>)`                                             | instance | element projection                  |
|  [04]   | `Seq.Map(Func<A,int,B>)`                                         | instance | indexed `(value, index)` projection |
|  [05]   | `Seq.map(Seq<A>, Func<int,A,B>)`                                 | static   | indexed `(index, value)` twin       |
|  [06]   | `Seq.Bind(Func<A,Seq<B>>)`                                       | instance | monadic expansion                   |
|  [07]   | `Seq.Filter(Func<A,bool>)`                                       | instance | predicate narrowing                 |
|  [08]   | `Seq.Partition(Func<A,bool>)`                                    | instance | one-pass two-way split              |
|  [09]   | `Seq.Exists(Func<A,bool>)`                                       | instance | any-member predicate probe          |
|  [10]   | `Seq.ForAll(Func<A,bool>)`                                       | instance | every-member predicate probe        |
|  [11]   | `SeqExtensions.Choose(Func<A,Option<B>>)`                        | static   | one-pass filter-map                 |
|  [12]   | `SeqExtensions.Choose(Func<int,A,Option<B>>)`                    | static   | indexed one-pass filter-map         |
|  [13]   | `SeqExtensions.Zip(Seq<B>, Func<A,B,C>)`                         | static   | projected pairwise join             |
|  [14]   | `SeqExtensions.Scan(S, Func<S,A,S>)`                             | static   | running-state projection            |
|  [15]   | `Seq.Head`                                                       | property | `Option<A>` first read              |
|  [16]   | `Seq.Last`                                                       | property | `Option<A>` final read              |
|  [17]   | `Seq.Tail`                                                       | property | all but the first member            |
|  [18]   | `Seq.Init`                                                       | property | all but the final member            |
|  [19]   | `Seq.Tails`                                                      | property | every suffix                        |
|  [20]   | `Seq.Inits`                                                      | property | every prefix                        |
|  [21]   | `Seq.Add(A)`                                                     | instance | append one member                   |
|  [22]   | `Seq.Concat(Seq<A>)`                                             | instance | cross-collection join               |
|  [23]   | `Seq.Intersperse(A)`                                             | instance | separator weave                     |
|  [24]   | `Seq.Strict()`                                                   | instance | force a lazily-built sequence       |
|  [25]   | `Seq.AsSpan()`                                                   | instance | zero-copy contiguous read           |
|  [26]   | `Seq.AsIterable()`                                               | instance | lazy-seam lift                      |
|  [27]   | `Seq.Traverse(Func<A,K<F,B>>)`                                   | instance | applicative shape inversion         |
|  [28]   | `Seq.TraverseM(Func<A,K<M,B>>)`                                  | instance | short-circuiting shape inversion    |
|  [29]   | `FoldableExtensions.Fold(S, Func<S,A,S>)`                        | fold     | carrier-generic state fold          |
|  [30]   | `FoldableExtensions.FoldM(S, Func<S,A,K<M,S>>)`                  | fold     | monadic state fold                  |
|  [31]   | `FoldableExtensions.FoldWhile(S, Func<S,A,S>, Func<(S,A),bool>)` | fold     | predicate-bounded fold              |
|  [32]   | `FoldableExtensions.FoldMap(Func<A,B>)`                          | fold     | monoidal aggregation                |
|  [33]   | `FoldableExtensions.Find(Func<A,bool>)`                          | static   | `Option`-shaped search              |
|  [34]   | `FoldableExtensions.FindAll(Func<A,bool>)`                       | static   | every match as a `Seq`              |
|  [35]   | `Arr.create(A[])`                                                | static   | immutable-array construction        |
|  [36]   | `Arr.createRange(IEnumerable<A>)`                                | static   | immutable-array admission           |
|  [37]   | `HashMap.Find(K)`                                                | instance | `Option<V>` lookup                  |
|  [38]   | `HashMap.Find(K, Func<V,R>, Func<R>)`                            | instance | matched lookup fold                 |
|  [39]   | `HashMap.FindOrAdd(K, Func<V>)`                                  | instance | lookup with insert-on-miss          |
|  [40]   | `HashMap.Add(K, V)`                                              | instance | persistent insert                   |
|  [41]   | `HashMap.AddOrUpdate(K, Func<V,V>, Func<V>)`                     | instance | persistent matched upsert           |
|  [42]   | `HashMap.SetItem(K, V)`                                          | instance | persistent replace                  |
|  [43]   | `HashMap.Remove(K)`                                              | instance | persistent delete                   |
|  [44]   | `HashMap.Union(IEnumerable<(K,V)>, WhenMatched<K,V,V,V>)`        | instance | merge with a collision rule         |
|  [45]   | `HashMap.ContainsKey(K)`                                         | instance | total key membership                |
|  [46]   | `HashMap.ToTrackingHashMap()`                                    | instance | change-logged map lift              |
|  [47]   | `HashMap.AddOrUpdate(K, V)`                                      | instance | persistent unconditional upsert     |
|  [48]   | `HashMap.AsIterable()`                                           | instance | `(K Key, V Value)` pair carrier     |
|  [49]   | `Set.Add(A)`                                                     | instance | persistent set insertion            |
|  [50]   | `Set.TryAdd(A)`                                                  | instance | insertion tolerating a duplicate    |
|  [51]   | `IterableExtensions.AsIterable(IEnumerable<A>)`                  | static   | lazy sync lift                      |
|  [52]   | `IterableExtensions.AsIterable(IAsyncEnumerable<A>)`             | static   | lazy async lift                     |
|  [53]   | `Iterable<A>.FromSpan(ReadOnlySpan<A>)`                          | static   | `params` span into the carrier rail |

[ENTRYPOINT_SCOPE]: state, optics, and the prelude vocabulary

| [INDEX] | [SURFACE]                                | [SHAPE]  | [CAPABILITY]                     |
| :-----: | :--------------------------------------- | :------- | :------------------------------- |
|  [01]   | `Prelude.Atom(A, Func<A,bool>)`          | static   | validated lock-free cell         |
|  [02]   | `Atom.Value`                             | property | current-state snapshot read      |
|  [03]   | `Atom.ValueIO`                           | property | repeating read on the IO rail    |
|  [04]   | `Atom.Swap(Func<A,A>)`                   | instance | CAS update                       |
|  [05]   | `Atom.SwapMaybe(Func<A,Option<A>>)`      | instance | CAS update with refusal          |
|  [06]   | `Atom.SwapIO(Func<A,A>)`                 | instance | CAS update on the effect rail    |
|  [07]   | `Atom.Change`                            | event    | accepted-swap notification       |
|  [08]   | `Prelude.AtomHashMap(HashMap<K,V>)`      | static   | lock-free keyed cell             |
|  [09]   | `Prelude.Ref(A, Func<A,bool>)`           | static   | transactional cell construction  |
|  [10]   | `Prelude.atomic(Func<R>, Isolation)`     | static   | multi-`Ref` transaction          |
|  [11]   | `Prelude.swap(Ref<A>, Func<A,A>)`        | static   | in-transaction update            |
|  [12]   | `Prelude.commute(Ref<A>, Func<A,A>)`     | static   | order-free in-transaction update |
|  [13]   | `Lens.New(Func<A,B>, Func<B,Func<A,A>>)` | static   | optic construction               |
|  [14]   | `Lens.Set(B, A)`                         | instance | immutable focused write          |
|  [15]   | `Lens.Update(Func<B,B>, A)`              | instance | immutable focused edit           |
|  [16]   | `Lens.fst<A,B>()`                        | static   | first-slot tuple optic           |
|  [17]   | `Lens.snd<A,B>()`                        | static   | second-slot tuple optic          |
|  [18]   | `Lens.tuple(Lens<A,C>, Lens<B,D>)`       | static   | composed tuple optic             |
|  [19]   | `Seq<A>.headOrNone`                      | property | first-slot optic over a `Seq`    |
|  [20]   | `Seq<A>.lastOrNone`                      | property | final-slot optic over a `Seq`    |
|  [21]   | `Prelude.memo(Func<A,B>)`                | static   | memoized pure function           |
|  [22]   | `Memo.Reset()`                           | instance | drop a memoized value            |
|  [23]   | `Range.fromMinMax(A, A, A)`              | static   | generated bounded sequence       |
|  [24]   | `Prelude.unit`                           | property | the `Unit` literal               |
|  [25]   | `Prelude.identity(A)`                    | static   | the identity projection          |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Domain operations return `Fin<A>`; `Fin.Succ` and `Fin.Fail` are the construction spellings, and an `Error`-derived domain fault record is the failure payload.
- Accumulation is a mode, not a second rail: independent gates lift into `Validation<Error, A>`, fan in through the tuple `Apply`, and exit `ToFin` — `Validation` lives exactly between those two conversions.
- Tuple `Apply` binds on `(K<F, A>, …)` receivers across arities 2–10, so each gate slot declares a `K<Validation<Error>, A>` return and the join re-anchors through `As()` or the unary `+`.
- `Fin.Match(Succ, Fail)` against `Validation.Match(Fail, Succ)`: named lambda arguments (`Succ:`, `Fail:`) bind by name, so the argument order stops deciding the fold.
- Tier choice is when the effect runs, never which failure type it carries — `Try` traps a throwing boundary synchronously through `Run`, `Eff` defers the same shape for composed and async execution, and `IO` is terminal, carrying bracket, schedule, fork, and timeout. All three land on `Fin<A>`.
- `guard(condition, error)` is the admission form: it composes inside a `Fin` or `Validation` LINQ body through the `SelectMany` overload over `Guard<E, Unit>`, and stands alone through `ToFin`.
- `Seq<A>` crosses rail seams as `Fin<Seq<A>>`, and `AsSpan` is its zero-copy contiguous read.
- `Arr<A>` is the indexed carrier collection expressions build; `Iterable<A>` is the lazy sync-or-async seam materializing through `ToSeq`.
- `Iterable<A>.FromSpan` is the one lift a `params ReadOnlySpan<A>` parameter takes to reach the carrier rail, because a span cannot cross into a lambda or an iterator; it copies at the call, so the returned carrier outlives the frame.
- Lookups return `Option`: `HashMap.Find`, `Seq.Head`, `Seq.Last`.
- `HashMap<K, V>` declares NO fold of its own; the carrier-generic `Fold` reaches it through `K<HashMap<K>, V>`, whose element is `V` ALONE. A fold needing the key runs over `AsIterable()`, whose element is the `(K Key, V Value)` pair — `map.Fold(seed, (state, pair) => … pair.Key …)` does not type, and the three-argument `Fold(S, Func<S,K,V,S>)` belongs to the `Eq`-parameterized `HashMap<EqK, K, V>`, not to the two-parameter map.
- Indexed enumeration is the instance `Map((value, index) => …)`; the module `Seq.map(seq, (index, value) => …)` transposes, so a mechanical rewrite between the two silently swaps the lambda arguments.
- `Traverse` inverts effect and shape applicatively (`Seq<Fin<A>>` to `Fin<Seq<A>>`); `TraverseM` inverts monadically and short-circuits on the first failure; `Partition` inverts without exiting, keeping both branches.
- `Option : Traversable<Option>`, so traversing an optional value is total over absence — `None` yields the applicative's own `Pure`, which makes `option.TraverseM(f).As()` the fold an optional payload's conditional effect takes and deletes the `Match` arm pair a rail forbids mid-pipeline.
- `Error : Monoid<Error>` is why `Validation<Error, A>` accumulates: `Combine` and `+` join failures into one carrier that `Head`, `Tail`, `Count`, and `AsIterable` re-enumerate.
- `Atom<A>.Swap` owns lock-free shared state and publishes each accepted swap on `Change`; `Ref<A>` owns the transactional cell that `atomic` commits across several refs in one isolation scope.
- `Atom<A>.Swap` returns the NEW value, so a take-and-clear spelled as `cell.Swap(_ => empty)` hands back the empty value it just installed — an evidence or tally cell drained that way reports zero forever. Hand-off reads need a member returning the prior value; `Value` is the honest snapshot where none exists.

[STACKING]:
- `Thinktecture.Runtime.Extensions`(`.api/api-thinktecture-runtime-extensions.md`): a generated `IObjectFactory.Validate` returns its `TValidationError`, which the admission gate maps to `Error` and lands on `Fin<A>`, or on `Validation<Error, A>` when several value objects admit at once; `ISmartEnum.TryGet` lifts to `Option<T>`.
- `Riok.Mapperly`(`.api/api-mapperly.md`): a generated mapper method returns the bare target and throws per its null policy, so the seam traps the call through `Try.lift(...).Run()` and keeps the rail outside the generated body.
- `CSparse`(`.api/api-csparse.md`): `Create` and `Solve` trap a singular or indefinite factorization through `Try.lift(...).Run()` onto `Fin<A>` carrying a domain `Error`.
- `System.Threading.Channels`(`.api/api-bcl-channels.md`): a rejected `TryWrite` and the `itemDropped` delegate fold into one `Atom<A>.Swap` receipt cell, and a `ReadAllAsync` drain body lands on `Fin<A>` or `Eff<A>`.
- `System.Runtime.InteropServices`(`.api/api-bcl-interop.md`): throwing `Create`, `Load`, and `GetExport` enter `Try` or `Eff` and land on `Fin<A>`; registered handles collect in an `Atom<Seq<IDisposable>>` released in reverse-registration order.
- Within-library composition runs at operator depth: `+ma` re-anchors a `K<F, A>`, `ma | mb` chooses, `mf * ma` applies, `ma >> f` binds, and `ma | @catch(pred, recover)` recovers by predicate.
- Lifetime and cadence are values: a resource acquires through `use` or `IO.Bracket`, and a repeat or retry composes an `IO` with a `Schedule`.
- `FinT<M, A>` and `ReaderT<Env, M, A>` stack a rail over another carrier, so a nested generic never needs a hand fold.

[LOCAL_ADMISSION]:
- Rails, collections, traits, and transformers compose directly; a domain failure type derives `Error` so it rides `Fin` and `Validation` natively.
- `using static LanguageExt.Prelude;` is in force in rail code: `Some`, `None`, `Optional`, `guard`, `Seq`, `toSeq`, `unit`, `Atom`, `Ref`, `atomic`, `memo`, and `use` are unqualified vocabulary.
- Every public signature carries the concrete carrier; a `K<F, A>` and the trait interfaces stay inside one composition body.

[RAIL_LAW]:
- Package: `LanguageExt.Core`
- Owns: result, accumulation, presence, deferral, failure vocabulary, immutable carriers, lock-free and transactional state, optics, memoization, and the higher-kinded trait system that unifies them.
- Accept: `Fin<A>`-returning domain operations; `Validation` fan-ins exiting `ToFin`; `Try`/`Eff`/`IO` boundary traps; `Seq` and `Arr` seam carriers; `Option`-shaped lookups; `guard` admission gates; `Atom` and `Ref` shared state; `Schedule`-driven repeat and retry.
- Reject: a local result, option, or either re-mint; exception control flow in domain logic; a throwing lookup where `Find` or `Head` returns `Option`; a `lock`ed cell beside `Atom.Swap`; a hand-rolled early-exit loop where `Traverse` or `TraverseM` inverts the shape; a wrapper renaming a rail member.
