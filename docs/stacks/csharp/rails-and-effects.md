# [RAILS_AND_EFFECTS]

LanguageExt owns result rails, effect execution, immutable collection flow, schedules, traversal, and managed boundary state. Use these surfaces after raw host, native, wire, or generated-shape inputs have been admitted into explicit domain values.

Package graph and workspace global usings route to [build and packages](platform/build-and-packages.md). Generated domain admission routes to [domain shapes](domain-shapes.md).

## [1][RAIL_SELECTION]

Use the narrowest rail that carries the real outcome.

| [INDEX] | [SURFACE]             | [USE]                          |
| :-----: | :-------------------- | :----------------------------- |
|   [1]   | `Option<T>`           | absence without failure        |
|   [2]   | `Fin<T>`              | synchronous fallible result    |
|   [3]   | `Validation<Error,T>` | independent error accumulation |
|   [4]   | `Eff<RT,T>`           | runtime-record effect          |
|   [5]   | `IO<T>`               | deferred side-effect           |
|   [6]   | `Schedule`            | retry or repeat policy         |
|   [7]   | `Seq<T>` and `Arr<T>` | immutable traversal            |
|   [8]   | `HashMap<K,V>`        | immutable lookup               |
|   [9]   | `Atom<T>`             | managed boundary state         |
|  [10]   | `K<F,A>`              | carrier-polymorphic algorithm  |

Use `Fin<T>` for local admission and native call results. Use `Validation<Error,T>` when independent failures should accumulate before one boundary result is returned. Use `Eff<RT,T>` when runtime context, resource lifetime, host effects, or asynchronous execution belongs to the operation.

## [2][BOUNDARY_FLOW]

Generated admission:
    Owner: generated `Create`, `TryCreate`, and validation partials.
    Rail: convert once into `Fin<T>` or `Validation<Error,T>`.
    Reject: wrapper factories that only rename generated factory members.

Host or native nullable input:
    Owner: boundary adapter.
    Rail: convert nulls and sentinels into `Option<T>` or `Fin<T>` before domain logic.
    Reject: null checks scattered inside collection transforms.

Exception boundary:
    Owner: native or host call site.
    Rail: use `Try.lift<Fin<T>>(f).Run().MapFail(...)` when the captured exception must become a typed domain failure.
    Reject: discarding the captured error after `Try.lift`.

Terminal collapse:
    Owner: host, UI, native, or wire boundary.
    Rail: keep `Match`, `IfFail`, `Run`, `RunAsync`, `RunIO`, and unsafe collapse at the consuming edge.
    Reject: terminal collapse inside pure domain projections.

## [3][PIPELINES]

Collection owner:
    Default: `Seq<T>` for domain sequence flow.
    Materialized output: `Arr<T>` when index stability or boundary transfer matters.
    Lookup: `HashMap<K,V>` when key policy is known.
    Set: `HashSet<T>` for immutable capability or graph sets.

Traversal owner:
    Use: `.TraverseM(f) >> lower` for `Fin`, `Validation`, or `Option`.
    Use: `.TraverseM(f).As()` for `Eff` or `IO`.
    Use: `.Traverse(identity) >> lower` when the collection already contains rail values.
    Use: `.Choose(f)` for filter-map to `Option`.
    Use: `.Fold(init, f)` for immutable aggregation.
    Use: `.Strict()` before boundary transfer when lazy traversal would outlive its owner.

Indexed effectful traversal:
    Use: indexed `Map((value, index) => ...)` plus `TraverseM(identity)` when an algorithm needs the index and no native indexed `TraverseM` exists.
    Reject: index-threaded folds unless the fold carries additional algorithm state.

Prelude guards:
    Use: `guard(condition, error)`, `guardnot(condition, error)`, `Optional(x)`, `Some(x)`, `None`, `identity`, `toSeq`, and `toHashMap` where they keep the pipeline expression-shaped.
    Boundary: use `Optional(x).ToFin(error)` at nullable boundaries.

## [4][FAILURE_TRANSFORMS]

Use carrier-qualified failure transforms before collapse. Do not throw inside rail transforms.

| [INDEX] | [COMBINATOR]            | [CARRIERS]                        | [USE]                  |
| :-----: | :---------------------- | :-------------------------------- | :--------------------- |
|   [1]   | `.MapFail(f)`           | `Fin`, `Validation`, `Eff`, `Try` | map failure            |
|   [2]   | `.BindFail(f)`          | `Fin`, `Validation`, `Try`        | recover rail           |
|   [3]   | `.IfFail(f)`            | `Validation`, `Try`, `Eff`        | terminal fallback      |
|   [4]   | `.BiBind(Succ:, Fail:)` | `Fin`, `Validation`               | branch both sides      |
|   [5]   | `.BiMap(succ, fail)`    | `Fin`, `Validation`, `Option`     | map both sides         |
|   [6]   | `.ToFin()`              | cross-rail                        | project to `Fin`       |
|   [7]   | `.ToValidation()`       | cross-rail                        | project to validation  |
|   [8]   | `.ToOption()`           | cross-rail                        | discard failure detail |
|   [9]   | `.Match(Succ:, Fail:)`  | `Fin`, `Option`, `Either`         | terminal collapse      |

Validation uses a monoidal error carrier. Use a typed error, `StringM`, `Validation<Seq<UiFault>,T>` at declared UI or batch fault owners, or another monoidal carrier. Do not use `Validation<string,T>` or ordinary domain/application `Validation<Seq<Error>,T>`.

## [5][EFFECTS_SCHEDULES_STATE]

Effects:
    Use: `Eff<RT,T>` for runtime-record effects and `IO<T>` for deferred side-effects or resource work.
    Use: `Eff.runtime<RT>()` when the runtime itself must be materialized.
    Use: `Prelude.liftEff` to lift pure or simpler effect values.
    Collapse: pick `Run`, `RunAsync`, or `RunIO` at the composition boundary.

Effect recovery:
    Use: `Prelude.catch`, `@catch`, `catchOf`, `catchOfFold`, `IfFailEff`, and verified catch combinators at effect boundaries.
    Resource: use `IO<T>.Bracket`, `BracketFail`, `Finally`, and `Prelude.use` for cleanup.
    Reject: bare `eff1 | eff2` as fallback or retry semantics.

Schedules:
    Use: `Schedule`, `IO<T>.Retry(Schedule)`, `Prelude.retry`, and `repeat` for retry, repeat, delay, timeout, and backoff policy.
    Builders: `recurs`, `spaced`, `linear`, `exponential`, `fibonacci`, `upto`, `jitter`, and `maxDelay`.
    Algebra: use schedule `|`, `union`, `intersect`, and schedule-transformer `+` only when schedule policy is the local owner.
    Duration: import `LanguageExt.UnitsOfMeasure` when duration literals carry the policy.

Operators:
    Domain `+` and `|`: application-defined operators on domain types.
    `[Flags] |`: enum bitwise OR, unrelated to LanguageExt.
    Validation `&`: independent validation product through a monoidal carrier.
    Schedule `|`: schedule union.
    Effect/finally `|`: effect-finally composition where the local type proves that owner.
    Rule: use named methods when the owner is not obvious from the local type.

State:
    Use: `Atom<T>.Swap(f)` for synchronous state transition.
    Use: `Atom<T>.SwapMaybe(f)` for optional state transition.
    Use: `Atom<T>.SwapIO` or `SwapMaybeIO` for IO-backed state transition.
    Boundary: keep `Atom<T>`, `AtomHashMap`, `AtomSeq`, and `AtomQue` at UI, session, or boundary concurrent state owners.
    Reject: hiding native lifetime, host tree mutation, or ordinary domain accumulation behind LanguageExt state.

## [6][TRAITS_AND_INTEROP]

Use `K<F,A>` and trait-based algorithms when one implementation genuinely works across carriers. Keep file-local query syntax unless repeated algorithms prove a trait abstraction.

Absent or unproven trait helpers such as `ComposeK`, `HyloM`, and `FoldArrows` stay rejected unless local XML or source proves availability.

Interop rules:
- Convert host arrays, lists, and tree values at adapter edges.
- Preserve GH2 tree semantics at the host boundary; project values into rails after the path owner is known.
- Keep MathNet vectors and matrices internal to algorithm execution.
- Keep span work inside measured primitive kernels or boundary adapters.
