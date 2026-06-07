# [RAILS_AND_EFFECTS]

LanguageExt owns result rails, effect execution, immutable collection flow, schedules, traversal, and managed boundary state. Use these surfaces after host, native, wire, or generated-shape inputs have been admitted into explicit domain values.

## [1][RAIL_CHOOSER]

Choose the narrowest carrier that preserves the real outcome. A wider rail is justified only when the operation needs the additional failure, runtime, resource, or polymorphic capability.

| [INDEX] | [SURFACE]             | [OWNS]                    | [REJECT]            |
| :-----: | :-------------------- | :------------------------ | :------------------ |
|   [1]   | `Option<T>`           | absence                   | hidden failure      |
|   [2]   | `Fin<T>`              | sync fallibility          | thrown control flow |
|   [3]   | `Validation<Error,T>` | independent failures      | early guard chain   |
|   [4]   | `Eff<RT,T>`           | runtime capability        | service location    |
|   [5]   | `IO<T>`               | deferred boundary work    | eager side effect   |
|   [6]   | `Schedule`            | retry or repeat policy    | ad-hoc delay loop   |
|   [7]   | `Seq<T>` and `Arr<T>` | immutable traversal       | mutable collection  |
|   [8]   | `HashMap<K,V>`        | immutable keyed lookup    | mutable dictionary  |
|   [9]   | `Atom<T>`             | boundary state cell       | domain accumulator  |
|  [10]   | `K<F,A>`              | carrier-polymorphic arrow | duplicate pipelines |

`Fin<T>` is the ordinary local fallible result. `Validation<Error,T>` is for independent checks that must accumulate before returning one boundary result. `Eff<RT,T>` is for runtime context, host effects, cancellation, resource lifetime, or asynchronous execution that belongs to the operation.

## [2][BOUNDARY_FLOW]

Generated admission:
    Input: generated value-object, complex-value-object, smart-enum, or union admission.
    Rail: convert generated success or failure once into `Fin<T>` or `Validation<Error,T>`.
    Boundary: generated shape policy stays in [domain shapes](domain-shapes.md).
    Reject: wrapper factories that only rename generated members.

Nullable or sentinel input:
    Input: host, native, UI, wire, or file value that can be absent or invalid.
    Rail: convert to `Option<T>` for absence or `Fin<T>` for failure before domain logic.
    Boundary: keep null and sentinel checks in adapters.
    Reject: null checks scattered inside collection transforms.

Exception capture:
    Input: native or host call that can throw.
    Rail: use `Try.lift<Fin<T>>(f).Run().MapFail(...)` only with a flattening step such as `Bind(static result => result)` or an explicit `Match`.
    Failure: preserve the captured error message or aggregate the captured error into the produced fault.
    Reject: discarding the captured error after `Try.lift`.

Terminal collapse:
    Input: `Fin<T>`, `Validation<E,T>`, `Eff<RT,T>`, `IO<T>`, or `Option<T>`.
    Rail: use `Match`, `IfFail`, `Run`, `RunAsync`, `RunIO`, or unsafe extraction only at host, UI, native, command, or wire edges.
    Boundary: reusable domain transforms keep the carrier.
    Reject: mid-pipeline collapse inside pure projections.

Boundary validation:
    Input: external DTO, config, UI form, or protocol payload.
    Rail: map boundary validation results into `Validation<Error,T>` or a declared batch-fault carrier before domain entry.
    Boundary: validation packages and rule sets stay outside domain modules.
    Reject: package-specific validators as domain policy.

## [3][TRAVERSAL_FLOW]

Traversal policy is part of rail policy because the collection shape decides how failures, effects, and resource boundaries compose.

Collection owner:
    Default: `Seq<T>` for domain sequence flow.
    Materialized output: `Arr<T>` when index stability or boundary transfer matters.
    Lookup: `HashMap<K,V>` when key policy is known.
    Set: immutable set surfaces when membership, graph edges, or uniqueness is the operation.

Rail traversal:
    Use: `.TraverseM(f).As()` when each item produces `Fin<T>`, `Validation<E,T>`, `Eff<RT,T>`, or `IO<T>` and the collection should become one carrier.
    Use: `.TraverseM(identity).As()` when the collection already contains carrier values.
    Use: `.Traverse(identity)` only when the carrier and source shape make applicative traversal clearer.
    Reject: map to carriers followed by identity traversal when direct traversal can fuse the projection.

Filter-map and aggregation:
    Use: `.Choose(f)` for filter-map into `Option<T>`.
    Use: `.Fold(init, f)` for immutable aggregation.
    Use: prepend plus reverse, or an owning builder, when fold output preserves order.
    Reject: mutable accumulators, `.Filter(...).Map(...)`, and append-heavy fold state.

Indexed effectful traversal:
    Use: indexed `Map((value, index) => ...)` plus `TraverseM(identity).As()` when the algorithm needs the index and no native indexed `TraverseM` exists.
    Reject: index-threaded folds unless the fold carries additional algorithm state.

Boundary strictness:
    Use: `.Strict()` before boundary transfer when lazy traversal would outlive its owner.
    Reject: lazy sequence flow over disposed, borrowed, UI, native, or host-owned resources.

Prelude guards:
    Use: `guard`, `guardnot`, `Optional`, `Some`, `None`, `identity`, `toSeq`, and `toHashMap` where they keep a pipeline expression-shaped.
    Boundary: use `Optional(x).ToFin(error)` at nullable boundaries.
    Reject: boolean success/failure factories that duplicate `guard(...).ToFin()`.

## [4][FAILURE_HANDLING]

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

Validation uses a monoidal error carrier. Use a typed error, `StringM`, a declared batch-fault carrier, or another monoidal carrier whose combination law the owner controls. Do not use `Validation<string,T>` or ordinary domain/application `Validation<Seq<Error>,T>`.

Recovery projects typed failures. If a captured exception carries diagnostic context, bind the failure parameter and thread its message or aggregate into the produced fault.

## [5][EFFECT_RUNTIME]

Runtime record:
    Use: `Eff<RT,T>` when the operation needs host capability, cancellation, progress, clock, filesystem, UI, native document, or another runtime dependency.
    Access: materialize runtime capability through `Eff.runtime<RT>()` or an owner-local ask projection.
    Boundary: construct the runtime once at the composition edge.
    Reject: service-location wrappers and ambient host globals inside reusable transforms.

Effect lifting:
    Use: lift pure, `Fin<T>`, or simpler effect values into `Eff<RT,T>` where the pipeline owns runtime composition.
    Async: wrap async boundary work into an effect and collapse at the edge.
    Reject: `await` inside `Eff<RT,T>` returning methods when an effect lift can express the boundary.

Resource boundary:
    Use: `IO<T>.Bracket`, `BracketFail`, `Finally`, `Prelude.use`, or an owner-local disposable capsule when the effect owns acquisition.
    Cleanup: the owner that acquires, borrows, or transfers a resource also disposes losing branches and failure paths.
    Reject: resource lifetime hidden behind ordinary domain state.

Effect recovery:
    Use: `Prelude.catch`, `@catch`, `catchOf`, `catchOfFold`, `IfFailEff`, and verified catch combinators at effect boundaries.
    Rule: choose named recovery when operator meaning is not obvious from the local type.
    Reject: bare `eff1 | eff2` as fallback or retry semantics.

Schedule policy:
    Use: `Schedule`, `IO<T>.Retry(Schedule)`, `Prelude.retry`, and `repeat` for retry, repeat, delay, timeout, and backoff policy when the local owner admits retry.
    Builders: `recurs`, `spaced`, `linear`, `exponential`, `fibonacci`, `upto`, `jitter`, and `maxDelay`.
    Algebra: use schedule `|`, `union`, `intersect`, and schedule-transformer `+` only when schedule policy is the local owner.
    Proof: schedule capability is admitted; implementation examples are not implied by this page.

Operator boundaries:
    Domain `+` and `|`: application-defined operators on domain types.
    `[Flags] |`: enum bitwise OR, unrelated to LanguageExt.
    Validation `&`: independent validation product through a monoidal carrier.
    Schedule `|`: schedule union.
    Effect/finally `|`: effect-finally composition where the local type proves that owner.
    Rule: use named methods when the owner is not obvious from the local type.

## [6][RECEIPTS_STATE]

State belongs at a boundary or session owner, not inside pure domain accumulation.

Atom state:
    Use: `Atom<T>.Swap(f)` for synchronous state transition.
    Use: `Atom<T>.SwapMaybe(f)` for optional state transition.
    Use: `Atom<T>.SwapIO` or `SwapMaybeIO` for IO-backed state transition.
    Rule: swap functions must be safe to retry under contention.
    Reject: hiding native lifetime, host tree mutation, or ordinary aggregation behind `Atom<T>`.

Boundary state families:
    Use: `Atom<T>`, `AtomHashMap`, `AtomSeq`, and `AtomQue` at UI, session, memoization, or concurrent boundary owners.
    Gate: cache only successful values unless the owner explicitly models failed entries.
    Reject: global mutable state disguised as functional flow.

Operational receipts:
    Rule: when repeated mutation buckets share construction, count, status, or slot semantics, fold them into one fact stream with slot or kind metadata.
    Projection: derive counts, summaries, and status from the fact stream.
    Reject: parallel receipt fields that must be kept in sync manually.

Algorithm receipts:
    Rule: keep typed receipts when fields carry solver, sampling, route, status, metric, spectral, mesh, extraction, or proof evidence.
    Projection: carry evidence beside the algorithm result.
    Reject: collapsing algorithm proof into a generic receipt ledger.

## [7][INTEROP]

Carrier-polymorphic algorithms:
    Use: `K<F,A>` and trait-based algorithms when one implementation genuinely works across carriers.
    Gate: keep carrier choice at instantiation, not inside every arrow.
    Reject: duplicated `Fin`, `Validation`, `Eff`, and `IO` pipelines for the same transform.

Unproven trait helpers:
    Rule: absent or unproven helpers such as `ComposeK`, `HyloM`, and `FoldArrows` stay rejected unless maintained API evidence proves availability.
    Replacement: write the owner-local composition directly with available carrier operations.

Host collections:
    Rule: convert host arrays, lists, and tree values at adapter edges.
    GH2 trees: preserve tree semantics at the host boundary; project values into rails after the path owner is known.
    Reject: BCL collection signatures as domain flow.

Numeric and span boundaries:
    MathNet: keep vectors and matrices internal to algorithm execution.
    Span: keep span work inside measured primitive kernels or boundary adapters.
    Reject: using rail policy to choose BCL or system replacements.

Composition and proof:
    Composition roots: runtime wiring, container scan, decorators, and host boot policy stay outside reusable rail transforms.
    Executable checks: prove behavior; this page states carrier policy.
    Adoption state: versions, references, and globals do not change carrier choice.

## [8][VALIDATION]

- [ ] The rail is the narrowest carrier that preserves the outcome.
- [ ] Generated admission, nullable input, sentinels, and exceptions convert once at the boundary.
- [ ] `Try.lift<Fin<T>>(...).Run().MapFail(...)` is flattened with `Bind(static result => result)` or explicit `Match`.
- [ ] `Run`, `RunAsync`, `RunIO`, `Match`, and unsafe collapse stay at the consuming edge.
- [ ] Traversal uses `.TraverseM(...).As()`, `.TraverseM(identity).As()`, `Choose`, and `Fold` before manual loops or accumulators.
- [ ] Schedule policy is stated as admitted target capability without claiming local examples where none exist.
- [ ] `Atom<T>` stays at boundary, session, memoization, or concurrent state owners.
- [ ] Operational receipts fold shared mutation facts, and algorithm receipts stay typed.
- [ ] Package, BCL, composition, and proof-tool facts stay out of rail policy unless they change carrier choice.
