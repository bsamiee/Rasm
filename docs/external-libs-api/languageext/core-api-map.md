# [H1][LANGUAGEEXT_CORE_API_MAP]
>**Dictum:** *The core API is a small set of algebraic families that compose together.*

<br>

[IMPORTANT] Read this file as an API registry, not a tutorial. Implementation examples belong in owning code, with this map used to select the correct surface.

---
## [1][FOUNDATION]
>**Dictum:** *Prelude constructors and extension methods make the algebra usable at call sites.*

<br>

| [INDEX] | [SURFACE] | [USE] | [RASM_POSTURE] |
| :-----: | --------- | ----- | -------------- |
| **[1]** | `LanguageExt.Prelude` | Constructors and combinators: `Some`, `None`, `Seq`, `toSeq`, `Optional`, `guard`, `unit`, `runtime`, `liftEff`, `liftIO`. | Imported globally. |
| **[2]** | `LanguageExt.Common.Error` | Shared error base and expected failure composition. | Base for `Expected` and `Fault`. |
| **[3]** | LINQ operators | `Select`, `SelectMany`, `Where`-like guard composition. | Preferred for multi-step rails. |
| **[4]** | `Map`, `Bind`, `BiMap`, `BindFail`, `MapFail` | Context-preserving transforms. | Required before final boundary collapse. |
| **[5]** | `Fin.Succ`, `Fin.Fail`, `pure<F,A>`, `error<F,A>` | Explicit success, pure, and failure construction. | Prefer explicit rail constructors over exceptions. |

---
## [2][MONADS]
>**Dictum:** *Each monad owns one semantic state.*

<br>

| [INDEX] | [TYPE] | [SEMANTICS] | [USE_IN_RASM] |
| :-----: | ------ | ----------- | ------------- |
| **[1]** | `Option<T>` | Absence without failure. | Optional ports, missing values, shape probes. |
| **[2]** | `Fin<T>` | Success or `Error` failure. | Domain refinement, dispatch, bridge reads, query execution. |
| **[3]** | `Validation<Error,T>` | Applicative error accumulation. | Context creation, validation gates, public `Analyze.Run`. |
| **[4]** | `Either<L,R>` | Typed left/right result. | Use only when left type must not be `Error`. |
| **[5]** | `Try<T>` | Exception capture. | Avoid in domain code; prefer explicit error rails. |
| **[6]** | `Reader<E,T>` | Environment access. | Prefer `Eff<RT,T>` runtime records for effectful paths. |
| **[7]** | `State<S,T>` | Explicit state threading. | Reserve for pure state machines. |
| **[8]** | `Writer<W,T>` | Monoidal output accumulation. | Defer until a true log/value accumulation problem appears. |
| **[9]** | `Identity<T>` | Pure identity carrier. | Use only as a trait instantiation target. |
| **[10]** | `These<L,R>` | Inclusive-or result. | Defer until partial-success semantics are explicit. |

---
## [3][EFFECTS]
>**Dictum:** *Effects describe execution until a boundary collapses them.*

<br>

| [INDEX] | [TYPE] | [USE] | [BOUNDARY_RULE] |
| :-----: | ------ | ----- | --------------- |
| **[1]** | `Eff<T>` | Effect with built-in error channel and no runtime. | Use for closed effects only. |
| **[2]** | `Eff<RT,T>` | Runtime-record effect pipeline. | Default for application and geometry analysis effects. |
| **[3]** | `Eff.runtime<RT>()` | Reads the active runtime as an `Eff`. | Current v5.0.0-beta-77 environment accessor; wrap module fields such as `Env.Asks`. |
| **[4]** | `Run`, `RunAsync`, `RunUnsafe`, `RunUnsafeAsync` | Collapses `Eff` at a boundary. | Use only in GH2 adapters, host lifecycles, CLIs, or tests. |
| **[5]** | `IO<T>` and `EnvIO` | Deferred sync or async IO with runtime IO environment. | Build descriptions; collapse once. |
| **[6]** | `Schedule` | Retry, repeat, timeout, and fallback policy. | Use as algebraic policy value, usually in decorators. |
| **[7]** | `Resources`, `Bracket`, `bracketIO` | Acquire-use-release. | Prefer over manual cleanup outside protocol boundaries. |
| **[8]** | `liftIO`, `liftEff`, `Eff.lift`, `LiftIO` | Lift tasks, `Fin`, `Either`, and IO descriptions into effect rails. | Prefer these universal constructors over ad hoc conversion helpers. |

---
## [4A][CONCURRENCY]
>**Dictum:** *Core concurrency is explicit state, not incidental mutation.*

<br>

| [INDEX] | [TYPE] | [USE] | [RASM_POSTURE] |
| :-----: | ------ | ----- | -------------- |
| **[1]** | `Atom<T>` | Lock-free single-value CAS with optional validator. | Use for boundary-owned shared state only. |
| **[2]** | `AtomHashMap<K,V>`, `AtomSeq<T>`, `AtomQue<T>` | Atomic wrappers over persistent collections. | Defer until host state needs atomic collection operations. |
| **[3]** | `Ref<T>` | STM reference. | Use with `atomic`, `snapshot`, or `serial` for multi-value transactions. |
| **[4]** | `atomic`, `atomicIO`, `snapshot`, `serial` | Transaction boundaries over refs. | Keep effects pure inside transaction functions. |

---
## [4][COLLECTIONS]
>**Dictum:** *Immutable collections encode traversal semantics.*

<br>

| [INDEX] | [TYPE] | [USE] | [RASM_POSTURE] |
| :-----: | ------ | ----- | -------------- |
| **[1]** | `Seq<T>` | General immutable sequence and traversal. | Default collection in analysis and GH2 bridge. |
| **[2]** | `Arr<T>` | Strict immutable array-like collection. | Use for indexed value batches when strictness matters. |
| **[3]** | `Lst<T>` | Immutable list with list-specific operations. | Use only when list operations beat `Seq<T>`. |
| **[4]** | `HashMap<K,V>` | Immutable hash map. | Prefer for domain maps outside Rhino reflection hazards. |
| **[5]** | `Map<K,V>` | Ordered map. | Use when ordering is semantic. |
| **[6]** | `Set<T>` and `HashSet<T>` | Unique value collections. | Use for closed vocabularies and de-duplication. |
| **[7]** | `Que<T>` and `Stck<T>` | Immutable queue and stack. | Use for worklists when direction matters. |

---
## [5][TRAITS]
>**Dictum:** *Traits make algorithms generic over computation shape.*

<br>

| [INDEX] | [TRAIT] | [CAPABILITY] | [ADVANCED_USE] |
| :-----: | ------- | ------------ | -------------- |
| **[1]** | `Functor<F>` | Map over `K<F,A>`. | Pure projection generic over effect type. |
| **[2]** | `Applicative<F>` | Independent composition. | Parallel validation and sequencing. |
| **[3]** | `Monad<F>` | Dependent sequencing. | Kleisli composition and effect-polymorphic algorithms. |
| **[4]** | `Fallible<F>` | Failure-aware computation. | Generic recovery and fail-channel handling. |
| **[5]** | `Foldable<T>` | Aggregation. | Structure-generic folds. |
| **[6]** | `Traversable<T>` | Sequence effects through structures. | `Traverse` and `Sequence` across collections. |
| **[7]** | `Eq<T>`, `Ord<T>`, `Hashable<T>` | Typeclass comparison. | Strong collection keys and ordered projections. |
| **[8]** | `LanguageExt.Traits.Domain.DomainType`, `Identifier`, `Amount`, `Locus`, `VectorSpace` | Value trait algebra. | Advanced numeric/domain atoms. |
| **[9]** | `Alternative<F>`, `Choice<F>`, `SemigroupK<F>`, `MonoidK<F>` | Alternative and fallback composition. | Use for folded fallbacks, not local branching. |
| **[10]** | `Bifunctor<F>` | Dual-channel mapping. | Use when both success and failure/value sides transform. |
| **[11]** | `MonadIO<F>`, `MonadUnliftIO<F>` | IO lifting and IO boundary operations in abstract carriers. | Reserve for advanced effect-polymorphic infrastructure. |

---
## [5A][SPECIALIZED_CORE]
>**Dictum:** *Specialized APIs stay dormant until they remove real code.*

<br>

| [INDEX] | [SURFACE] | [CAPABILITY] | [POSTURE] |
| :-----: | --------- | ------------ | --------- |
| **[1]** | `Lens<S,A>` and `Prism<S,A>` | Composable field and variant focus. | Defer; Thinktecture dispatch and records currently cover domain shape. |
| **[2]** | `Pretty.Doc` and `DocAnn` | Structured pretty-printing. | Defer until diagnostics need layout-aware text. |
| **[3]** | `UnitsOfMeasure` | Unit constants such as `ms`, `sec`, `m`, `mm`. | Use for `Schedule` durations; keep Rhino unit policy in `Context`. |
| **[4]** | `Memo<T>` and `memoK` | Thread-safe memoization. | Use only for stable expensive projections; avoid caching Rhino-owned mutable objects. |

---
## [6][RULES]
>**Dictum:** *Select the smallest surface that preserves semantics.*

<br>

- Use `Option<T>` for absence and `Fin<T>` for failure.
- Use `Validation<Error,T>` when independent checks must accumulate.
- Use `Eff<RT,T>` when runtime context, cancellation, progress, IO, or host services participate.
- Use `K<F,A>` and trait constraints only when one algorithm must run over multiple effect shapes.
- Use `Eff.runtime<RT>().Map(...).As()` or module-level runtime accessors for pinned v5.0.0-beta-77.
