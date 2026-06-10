# [RAILS_AND_EFFECTS]

LanguageExt owns result rails, effect execution, immutable collection flow, schedules, traversal, and managed boundary state. Use these surfaces after host, native, wire, or generated-shape inputs have been admitted into explicit domain values.

## [1]-[RAIL_CHOOSER]

Choose the narrowest carrier that preserves the real outcome. A wider rail is justified only when the operation needs the additional failure, runtime, resource, schedule, state, or carrier-polymorphic capability.

| [INDEX] | [SURFACE]             | [OWNS]                         | [REJECT]                    |
| :-----: | :-------------------- | :----------------------------- | :-------------------------- |
|   [1]   | `Option<T>`           | absence                        | hidden failure              |
|   [2]   | `Fin<T>`              | synchronous fallibility        | thrown control flow         |
|   [3]   | `Validation<E,T>`     | independent accumulated faults | early guard chain           |
|   [4]   | `Eff<RT,T>`           | runtime capability             | service location            |
|   [5]   | `IO<T>`               | deferred boundary work         | eager side effect           |
|   [6]   | `Schedule`            | retry or repeat policy         | ad-hoc delay loop           |
|   [7]   | `Seq<T>` and `Arr<T>` | immutable traversal            | mutable collection flow     |
|   [8]   | `HashMap<K,V>`        | immutable keyed lookup         | mutable dictionary policy   |
|   [9]   | `Atom<T>`             | boundary state cell            | domain accumulator          |
|  [10]   | `K<F,A>`              | carrier-polymorphic arrow      | duplicate carrier pipelines |

`Fin<T>` is the ordinary local fallible result. `Validation<E,T>` is for independent checks that must accumulate before returning one boundary result. `Eff<RT,T>` is for runtime context, host effects, cancellation, resource lifetime, or asynchronous execution that belongs to the operation.

## [2]-[BOUNDARY_CONVERSION]

Every boundary converts once into the carrier that states the real outcome. Reusable domain transforms keep the carrier; terminal collapse belongs at host, UI, native, command, or wire edges.

[GENERATED_ADMISSION]:
- Input: generated value-object, complex-value-object, smart-enum, or union admission.
- Rail: convert generated success or failure once into `Fin<T>` or `Validation<Error,T>`.
- Boundary: generated shape policy stays with the domain-shape owner.
- Reject: wrapper factories that only rename generated members.

[NULLABLE_OR_SENTINEL_INPUT]:
- Input: host, native, UI, wire, file, or configuration value that can be absent or invalid.
- Rail: convert to `Option<T>` for absence or `Fin<T>` for failure before domain logic.
- Boundary: keep null and sentinel checks in adapters.
- Reject: null checks scattered inside collection transforms.

[EXCEPTION_CAPTURE]:
- Input: native or host call that can throw.
- Rail: use `Try.lift<Fin<T>>(f).Run().MapFail(...)` only with a flattening step such as `Bind(static result => result)` or an explicit `Match`.
- Failure: preserve the captured error message or aggregate the captured error into the produced fault.
- Reject: discarding the captured error after `Try.lift`.

```csharp conceptual
public static Fin<Receipt> Capture(Func<Fin<Receipt>> native) {
    ArgumentNullException.ThrowIfNull(native);

    return Try.lift<Fin<Receipt>>(f: native)
        .Run()
        .MapFail(error => new Fault.NativeRejected(Detail: error.Message))
        .Bind(static result => result);
}
```

[TERMINAL_COLLAPSE]:
- Input: `Fin<T>`, `Validation<E,T>`, `Eff<RT,T>`, `IO<T>`, or `Option<T>`.
- Rail: use `Match`, `IfFail`, `Run`, `RunAsync`, `RunIO`, or unsafe extraction only at consuming edges.
- Boundary: reusable domain transforms keep the carrier.
- Reject: mid-pipeline collapse inside pure projections.

[BOUNDARY_VALIDATION]:
- Input: external DTO, config, UI form, or protocol payload.
- Rail: map boundary validation results into `Validation<Error,T>` or a declared batch-fault carrier before domain entry.
- Boundary: validation packages and rule sets stay outside domain modules.
- Reject: package-specific validators as domain policy.

## [3]-[TRAVERSAL_FLOW]

Traversal policy is rail policy because the collection shape decides how failures, effects, strictness, and resource boundaries compose.

[COLLECTION_OWNER]:
- Default: `Seq<T>` for domain sequence flow.
- Materialized output: `Arr<T>` when index stability or boundary transfer matters.
- Lookup: `HashMap<K,V>` when key policy is known.
- Set: immutable set surfaces when membership, graph edges, or uniqueness is the operation.

[RAIL_TRAVERSAL]:
- Use: `.TraverseM(f).As()` when each item produces `Fin<T>`, `Validation<E,T>`, `Eff<RT,T>`, or `IO<T>` and the collection should become one carrier.
- Use: `.TraverseM(identity).As()` when the collection already contains carrier values.
- Use: `.Traverse(identity)` only when the carrier and source shape make applicative traversal clearer.
- Reject: map to carriers followed by identity traversal when direct traversal can fuse the projection.

[INDEXED_TRAVERSAL]:
- Use: indexed `Map((value, index) => ...)` plus `TraverseM(identity).As()` when the algorithm needs the index and no native indexed `TraverseM` exists.
- Reject: index-threaded folds unless the fold carries additional algorithm state.

[FILTER_MAP_AND_AGGREGATION]:
- Use: `.Choose(f)` for filter-map into `Option<T>`.
- Use: `.Fold(init, f)` for immutable aggregation.
- Use: prepend plus reverse, or an owning builder, when fold output preserves order.
- Reject: mutable accumulators, `.Filter(...).Map(...)`, and append-heavy fold state.

[BOUNDARY_STRICTNESS]:
- Use: `.Strict()` before boundary transfer when lazy traversal would outlive its owner.
- Reject: lazy sequence flow over disposed, borrowed, UI, native, or host-owned resources.

```csharp conceptual
public static Fin<Seq<Receipt>> TraverseRaw(Seq<string> raw) =>
    raw.Map((value, index) => AdmitCode(value).Map(code => new Input(Code: code, Score: index + 1)))
        .TraverseM(identity)
        .As()
        .Bind(inputs => inputs.TraverseM(input => Mode.Strict.Apply(input)).As())
        .Map(static receipts => receipts.Strict());
```

[PRELUDE_GUARDS]:
- Use: `guard`, `guardnot`, `Optional`, `Some`, `None`, `identity`, `toSeq`, and `toHashMap` where they keep a pipeline expression-shaped.
- Boundary: use `Optional(x).ToFin(error)` at nullable boundaries.
- Reject: boolean success/failure factories that duplicate `guard(...).ToFin()`.

## [4]-[FAILURE_HANDLING]

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

[VALIDATION_MONOID]:
- Rule: `Validation<E,T>` requires an error carrier with an owned combination law.
- Use: typed error, `StringM`, a declared batch-fault carrier, `Seq<TFault>` at boundary/UI aggregation, or another monoidal carrier the owner controls.
- Reject: ordinary domain/application `Validation<Seq<Error>,T>` and `Validation<string,T>`.

[RECOVERY]:
- Rule: recovery projects typed failures and preserves diagnostic context.
- Use: bind the failure parameter when producing a replacement fault or fallback rail.
- Reject: losing exception messages, raw validation messages, or failure categories during conversion.

```csharp conceptual
public static Validation<Error, RangeValue> AdmitRange(string raw, int start, int end) =>
    (AdmitCode(raw).ToValidation(), ValidateIndex(start).ToValidation(), ValidateIndex(end).ToValidation())
        .Apply(static (code, s, e) => RangeValue.TryCreate(
            code: code,
            start: s,
            end: e,
            obj: out RangeValue value)
            ? Fin.Succ(value).ToValidation()
            : Fin.Fail<RangeValue>(new Fault.InvalidRange(Start: s, End: e)).ToValidation())
        .Bind(static validation => validation)
        .As();
```

## [5]-[EFFECT_RUNTIME]

[RUNTIME_RECORD]:
- Use: `Eff<RT,T>` when the operation needs host capability, cancellation, progress, clock, filesystem, UI, native document, or another runtime dependency.
- Access: materialize runtime capability through `Eff.runtime<RT>()` or an owner-local ask projection.
- Boundary: construct the runtime once at the composition edge.
- Reject: service-location wrappers and ambient host globals inside reusable transforms.

[EFFECT_LIFTING]:
- Use: lift pure, `Fin<T>`, or simpler effect values into `Eff<RT,T>` where the pipeline owns runtime composition.
- Async: wrap async boundary work into an effect and collapse at the edge.
- Reject: `await` inside `Eff<RT,T>` returning methods when an effect lift can express the boundary.

```csharp conceptual
public sealed record Runtime(Mode Mode, Atom<HashMap<CodeValue, Receipt>> Cache, CancellationToken Cancel);

public static readonly Eff<Runtime, Mode> AskMode =
    Eff.runtime<Runtime>().Map(static runtime => runtime.Mode).As();
```

[RESOURCE_BOUNDARY]:
- Use: `IO<T>.Bracket`, `BracketFail`, `Finally`, `Prelude.use`, or an owner-local disposable capsule when the effect owns acquisition.
- Cleanup: the owner that acquires, borrows, or transfers a resource also disposes losing branches and failure paths.
- Exemption: `using` acquisition inside a boundary capsule is the named statement exemption; it never appears in domain flow.
- Reject: resource lifetime hidden behind ordinary domain state.

```csharp conceptual
internal static Fin<Unit> UseResource(Func<Resource> acquire, Func<Resource, Fin<Unit>> use) {
    ArgumentNullException.ThrowIfNull(acquire);
    ArgumentNullException.ThrowIfNull(use);

    using Resource resource = acquire();
    return use(resource);
}
```

[EFFECT_RECOVERY]:
- Use: `Prelude.catch`, `@catch`, `catchOf`, `catchOfFold`, `IfFailEff`, and verified catch combinators at effect boundaries.
- Rule: choose named recovery when operator meaning is not obvious from the local type.
- Reject: bare `eff1 | eff2` as fallback or retry semantics.

[SCHEDULE_POLICY]:
- Use: `Schedule`, `IO<T>.Retry(Schedule)`, `Prelude.retry`, and `repeat` for retry, repeat, delay, timeout, and backoff policy when the local owner admits retry.
- Carrier split: `.Retry(Schedule)` is the `IO<T>` form; `Eff` retry is `Prelude.retry(schedule, eff)`, never a method on the effect.
- Builders: `recurs`, `spaced`, `linear`, `exponential`, `fibonacci`, `upto`, `jitter`, and `maxDelay`.
- Algebra: use schedule `|`, `union`, `intersect`, and schedule-transformer `+` only when schedule policy is the local owner.
- Reject: ad-hoc delay loops and operator-heavy retry code where named schedule policy is clearer.

```csharp conceptual
public static IO<Receipt> Retry(IO<Receipt> work) {
    ArgumentNullException.ThrowIfNull(work);

    return work.Retry(
        Schedule.exponential(seed: 10 * LanguageExt.UnitsOfMeasure.ms)
        | Schedule.recurs(times: 2));
}
```

[OPERATOR_BOUNDARIES]:

| [INDEX] | [OPERATOR]            | [CARRIER]                | [OWNS]                            |
| :-----: | :-------------------- | :----------------------- | :-------------------------------- |
|   [1]   | `>>`                  | `K<F,A>`                 | Kleisli sequence, discard-first   |
|   [2]   | `>>>`                 | `K<F,A>`                 | applicative sequence              |
|   [3]   | `*`                   | `K<F,A>`                 | functor map, applicative apply    |
|   [4]   | `>> lower`, unary `+` | `K<F,A>`                 | downcast to the concrete rail     |
|   [5]   | `\|`                  | `Validation` choice      | first success wins                |
|   [6]   | `\|`                  | `Fallible` with `CatchM` | catch and recovery                |
|   [7]   | `\|`                  | `Eff` with `Finally`     | finally composition, not choice   |
|   [8]   | `\|`                  | `Schedule`               | schedule union                    |
|   [9]   | `&`                   | `Validation`             | applicative product, fault append |
|  [10]   | `+`                   | `Error`, monoidal `E`    | failure append                    |
|  [11]   | `+`                   | `ScheduleTransformer`    | transformer composition           |
|  [12]   | `\|`                  | `[Flags]` enum           | BCL bitwise OR                    |
|  [13]   | `+`, `\|`             | domain owners            | application-defined algebra       |

- Rule: use named methods when the owner is not obvious from the local type; `union` and `intersect` on `Schedule` are prelude functions, and schedule intersect is not `&`.
- Rule: `>> lower` and unary `+` downcast `K<F,A>` to the concrete rail; `.As()` is the named equivalent.

## [6]-[STATE_RECEIPTS]

State belongs at a boundary or session owner, not inside pure domain accumulation.

[ATOM_STATE]:
- Use: `Atom<T>.Value` for point reads; never read-modify-write outside `Swap`.
- Use: `Atom<T>.Swap(f)` for synchronous state transition; `Swap` returns the post-transition state.
- Use: `Atom<T>.SwapMaybe(f)` for optional state transition.
- Use: `Atom<T>.SwapIO` or `SwapMaybeIO` for IO-backed state transition.
- Rule: swap functions must be safe to retry under contention; idempotent transitions such as `TryAdd` make first-writer-wins explicit.
- Reject: hiding native lifetime, host tree mutation, or ordinary aggregation behind `Atom<T>`.

```csharp conceptual
public static Fin<Receipt> Memoized(Runtime runtime, Input input) =>
    runtime.Cache.Value.Find(input.Code) is { IsSome: true, Case: Receipt cached }
        ? Fin.Succ(cached)
        : runtime.Mode.Apply(input).Map(receipt =>
            runtime.Cache
                .Swap(state => state.TryAdd(input.Code, receipt))
                .Find(input.Code)
                .IfNone(receipt));
```

[BOUNDARY_STATE_FAMILIES]:
- Use: `Atom<T>`, `AtomHashMap`, `AtomSeq`, and `AtomQue` at UI, session, memoization, or concurrent boundary owners.
- Gate: cache only successful values unless the owner explicitly models failed entries.
- Reject: global mutable state disguised as functional flow.

[OPERATIONAL_RECEIPTS]:
- Rule: when repeated mutation buckets share construction, count, status, or slot semantics, fold them into one fact stream with slot or kind metadata.
- Projection: derive counts, summaries, and status from the fact stream.
- Reject: parallel receipt fields that must be kept in sync manually.

[ALGORITHM_RECEIPTS]:
- Rule: keep typed receipts when fields carry solver, sampling, route, status, metric, spectral, mesh, extraction, or proof evidence.
- Projection: carry evidence beside the algorithm result.
- Reject: collapsing algorithm proof into a generic receipt ledger.

```csharp conceptual
public readonly record struct Receipt(CodeValue Code, int Count) {
    public static readonly Receipt Empty = new(Code: CodeValue.Create(value: "SUM"), Count: 0);

    public static Receipt operator +(Receipt left, Receipt right) =>
        new(Code: left.Code, Count: left.Count + right.Count);
}
```

## [7]-[INTEROP]

[CARRIER_POLYMORPHIC_ALGORITHMS]:
- Use: `K<F,A>` and trait-based algorithms when one implementation genuinely works across carriers.
- Gate: keep carrier choice at instantiation, not inside every arrow.
- Reject: duplicated `Fin`, `Validation`, `Eff`, and `IO` pipelines for the same transform.

[UNPROVEN_TRAIT_HELPERS]:
- Rule: absent or unproven helpers such as `ComposeK`, `HyloM`, and `FoldArrows` stay rejected unless maintained API evidence proves availability.
- Replacement: write the owner-local composition directly with available carrier operations.

[HOST_COLLECTIONS]:
- Rule: convert host arrays, lists, and tree values at adapter edges.
- Trees: preserve tree semantics at the host boundary; project values into rails after the path owner is known.
- Reject: BCL collection signatures as domain flow.

[NUMERIC_AND_SPAN_BOUNDARIES]:
- MathNet: keep vectors and matrices internal to algorithm execution.
- Span: keep span work inside measured primitive kernels or boundary adapters.
- Reject: using rail policy to choose BCL or system replacements.

[COMPOSITION_AND_PROOF]:
- Composition roots: runtime wiring, container scan, decorators, and host boot policy stay outside reusable rail transforms.
- Executable checks: prove behavior; this page states carrier policy.
- Adoption state: versions, references, and globals do not change carrier choice.

## [8]-[VALIDATION]

- [ ] The rail is the narrowest carrier that preserves the outcome.
- [ ] Generated admission, nullable input, sentinels, and exceptions convert once at the boundary.
- [ ] `Try.lift<Fin<T>>(...).Run().MapFail(...)` is flattened with `Bind(static result => result)` or explicit `Match`.
- [ ] `Run`, `RunAsync`, `RunIO`, `Match`, and unsafe collapse stay at the consuming edge.
- [ ] Traversal uses `.TraverseM(...).As()`, `.TraverseM(identity).As()`, indexed `Map`, `Choose`, and `Fold` before manual loops or accumulators.
- [ ] `.Strict()` is used before lazy values cross resource or boundary lifetimes.
- [ ] Schedule policy uses admitted schedule builders and named retry/repeat owners.
- [ ] `Atom<T>` stays at boundary, session, memoization, or concurrent state owners.
- [ ] Operational receipts fold shared mutation facts, and algorithm receipts stay typed.
- [ ] Package, BCL, composition, and proof-tool facts stay out of rail policy unless they change carrier choice.
