# [RAILS_AND_EFFECTS]

The Effect ecosystem owns result rails, effect execution, immutable traversal, schedule policy, transactional state, and boundary cells. A carrier is chosen once at decode and never re-chosen mid-pipeline: the narrowest carrier that states the real outcome carries the value, reusable transforms keep it, and collapse to a bare value happens only at the runtime edge through `Effect.runPromise`, `Effect.runFork`, or a `Match` terminal. Decoded domain values enter these surfaces; raw provider, `Promise`, and JS-stdlib shapes do not.

## [1]-[RAIL_CHOOSER]

Choose the narrowest carrier that preserves the real outcome. A wider rail is earned only by a capability the narrower one cannot carry: typed failure, accumulated faults, required context, resource lifetime, schedule, transactional state, or concurrency.

| [INDEX] | [SURFACE]                  | [OWNS]                          | [REJECT]                       |
| :-----: | :------------------------- | :------------------------------ | :----------------------------- |
|   [1]   | `Option<T>`                | absence, no cause               | hidden failure, `null` leak    |
|   [2]   | `Either<E, A>`             | pure synchronous branching      | `throw` for control flow       |
|   [3]   | `Effect.Effect<A, E>`      | typed fallibility, deferral     | `Promise`, eager side effect   |
|   [4]   | `Effect.Effect<A, E, R>`   | required context capability     | service location, ambient global |
|   [5]   | `Cause<E>`                 | full failure tree at a boundary | flat error after composition   |
|   [6]   | `Schedule`                 | retry, repeat, backoff policy   | ad-hoc delay loop              |
|   [7]   | `Chunk<T>` / `Array` module | immutable traversal             | mutable `Array.push` flow      |
|   [8]   | `HashMap<K,V>` / `HashSet` | immutable keyed lookup          | `new Map()` `.set` mutation    |
|   [9]   | `Ref<T>` / `STM` / `TMap`  | managed and transactional state | `let` accumulator, shared `var` |
|  [10]   | `Stream<A, E, R>`          | back-pressured async sequence   | unbounded buffered array       |

`Option<T>` carries absence with zero failure semantics; promote to a typed `Effect` error when the caller must know why; the error channel `E` accumulates only when `Effect.all({ mode: "validate" })` or `Effect.validateAll` is the seam, otherwise it short-circuits.

[REPRESENTATION_DEFAULT]:
- Law: `Option<T>` is the domain absence carrier — `Option.fromNullable` at the boundary, `Option.match`/`Option.getOrElse` at consumption, never a bare `null` or `undefined` past the seam.
- Law: `Either<E, A>` carries pure synchronous branching without Effect overhead — `Either.right`/`Either.left`, `Either.match` — and lifts into the Effect error channel through `Effect.fromEither`.
- Boundary: an absence-carrying slot is `Option.none`; a fallible operation is a typed `Effect` failure; cause-bearing absence is a tagged family, never `Option`.

[CARRIER_IDENTITY]:
- Law: `Data.TaggedError` and `Data.taggedEnum` auto-derive `Equal` and `Hash`, so a `HashSet` of faults or a `HashMap` keyed on a tagged value uses structural equality without a custom comparator.
- Law: `Order.mapInput(Order.number, projector)` lifts a vocabulary ordinal into an `Order` over the fault, and `Order.max` produces the lattice join that reduces a cause tree to one fault.
- Use: an `Equivalence.make` for a compound cache key so field-wise comparison replaces delimiter-concatenated signature strings.

## [2]-[BOUNDARY_CONVERSION]

Every boundary converts once into the carrier that states the real outcome; reusable transforms keep that carrier and never re-project mid-pipeline.

[EXCEPTION_CAPTURE]:
- Use: `Effect.tryPromise({ try, catch })` to capture a throwing async host call, `Effect.try({ try, catch })` for the synchronous counterpart, mapping the unknown cause immediately into a bounded tagged error.
- Law: the object form of `tryPromise` threads the `AbortSignal` into the host call for fiber-interruption propagation; the thunk form runs to completion regardless of interruption.
- Law: `Schema.decodeUnknown(schema)(input)` is the one inbound funnel — it admits unknown material and its `ParseError` lifts into the error channel through `Effect.mapError` at a single entry, never a per-shape branch.
- Reject: discarding the captured cause; a bare `try`/`catch` wrapping a rail transform; `Promise` chaining where an Effect carries the boundary.

```ts conceptual
import { Data, Effect, Schema as S } from "effect"

class IngressFault extends Data.TaggedError("IngressFault")<{ readonly stage: "decode" | "upstream"; readonly cause: unknown }> {}

const admit = <A, I>(schema: S.Schema<A, I>, dispatch: (a: A, signal: AbortSignal) => Promise<Response>) =>
  (input: unknown): Effect.Effect<Response, IngressFault> =>
    S.decodeUnknown(schema)(input).pipe(
      Effect.mapError((cause) => new IngressFault({ stage: "decode", cause })),
      Effect.flatMap((a) => Effect.tryPromise({ try: (signal) => dispatch(a, signal), catch: (cause) => new IngressFault({ stage: "upstream", cause }) })),
    )
```

[CROSS_RAIL_PROJECTION]:
- Use: `Effect.either` to migrate a failure into an `Either<E, A>` data value inside a fold; `Effect.option` to discard the failure side into `Option<A>`; `Effect.fromEither`/`Effect.fromOption` to widen back into the carrier.
- Law: widening supplies the missing structure — `Option → Effect` needs an `onNone` error, `Either → Effect` lifts the left into the error channel.
- Reject: round trips that stamp a generic error over the original; the diagnostic identity of a typed fault survives every projection.

[TERMINAL_COLLAPSE]:
- Use: `Effect.runPromise`, `Effect.runFork`, `Match`-terminal collapse, or `Effect.runSync` only at the runtime edge — the composition root, a test, or a host callback.
- Law: reusable domain transforms keep the carrier; an `Effect.runSync` inside a pure projection or an `Option.match` mid-pipeline that loses composition is the rejected exit.
- Reject: mid-pipeline collapse inside a transform; `Match.exhaustive` called mid-pipeline where `map`/`flatMap` keep the rail.

## [3]-[TRAVERSAL_FLOW]

Traversal is rail policy: the collection shape and the sequencing combinator together decide how failures, effects, strictness, and concurrency compose.

[COLLECTION_OWNER]:
- Law: `HashMap<K,V>` owns immutable keyed accumulator state threaded through `Stream.mapAccum` or `Array.reduce`; `Chunk<T>` owns `Stream` operations (`Sink`, batching, `mapAccum` emissions) with O(1)-amortized append; the `Array` module owns small fixed collections with richer combinators (`Array.getSomes`, `Array.match`, `Array.groupBy`, `Array.dedupeWith`).
- Law: a `HashMap.set` returns a new map — persistent update, structural sharing, `Equal`/`Hash` integration; `new Map().set` mutation breaks referential transparency and is a boundary-only form.
- Use: `Chunk.toReadonlyArray` at the boundary transfer where a downstream consumer needs an array; `Array.fromIterable` to admit a foreign iterable once.
- Reject: `new Map()`/`new Set()`/`Array.push` mutation in domain flow; lazy flow over a disposed or host-owned resource.

[RAIL_TRAVERSAL]:
- Use: `Effect.forEach(items, f)` to abort on the first failure; `Effect.all(struct, { mode: "validate" })` to accumulate every independent failure; `Effect.partition(items, f)` to collect both rejected and accepted with `E = never`.
- Law: `{ concurrency: "inherit" | number | "unbounded" }` is the parallelism policy, not a performance tweak; `Effect.all` over effects launches per the concurrency, `Effect.forEach` is sequential at `concurrency: 1`.
- Law: `Effect.mergeAll(effects, zero, f)` is the N-way concurrent fold with positional index — pre-wrap each effect with `Effect.either` to convert failures to data, or the first failure short-circuits.
- Use: `Effect.validateAll` for a `NonEmptyArray<E>` of every failure; `Effect.validateFirst` for the first success.
- Reject: `Effect.allSuccesses` where a discarded failure is a silent correctness loss; an index-threaded fold unless the fold carries algorithm state.

```ts conceptual
import { Array as A, Effect } from "effect"

const traverseRaw = (raw: ReadonlyArray<string>) =>
  Effect.forEach(raw, (value, index) => _admitCode(value).pipe(Effect.map((code) => ({ code, score: index + 1 }))), { concurrency: "inherit" }).pipe(
    Effect.flatMap((inputs) => Effect.forEach(inputs, (input) => _strict(input))),
    Effect.map((receipts) => A.dedupeWith(receipts, (l, r) => l.code === r.code)),
  )
```

[FILTER_MAP_AND_AGGREGATION]:
- Use: `Array.filterMap` / `Array.getSomes` for atomic single-pass filter-map into `Option`, replacing `.filter(p).map(f)`; `Array.reduce` for immutable aggregation; `Array.groupBy` for keyed partition.
- Law: `Number.divide` returns `Option<number>`, forcing an explicit `Option.getOrElse` at every zero-denominator site, so no `NaN`/`Infinity` propagates through an arithmetic pipeline.
- Reject: a mutable accumulator, an append-heavy fold, and a `zip` across unequal lengths without first equalizing from domain knowledge.

[GUARDS]:
- Use: `Effect.filterOrFail(predicate, onFalse)` for a mid-pipeline boolean invariant producing a typed failure; `Effect.liftPredicate(value, predicate, onFalse)` to lift a value at entry; `Option.fromNullable(x)` then `Effect.fromOption` at a nullable boundary.
- Law: `filterOrDie`/`Effect.orDie` elevate a guard to a defect only at a boundary where the failure is a non-recoverable invariant, never in domain flow.
- Reject: boolean success/failure factories duplicating `filterOrFail`; a guard that throws instead of failing the rail.

## [4]-[FAILURE_HANDLING]

Apply carrier-qualified failure transforms before collapse; a rail transform never throws.

| [INDEX] | [COMBINATOR]                | [USE]                                  |
| :-----: | :-------------------------- | :------------------------------------- |
|   [1]   | `Effect.mapError(f)`        | reshape the typed failure              |
|   [2]   | `Effect.catchTag(tag, f)`   | recover one tagged variant             |
|   [3]   | `Effect.catchTags({...})`   | recover several tags with coverage     |
|   [4]   | `Effect.catchIf(refine, f)` | recover by `$is` refinement            |
|   [5]   | `Effect.tapError(f)`        | observe without channel widening       |
|   [6]   | `Effect.catchAllCause(f)`   | fold the full cause at a boundary      |
|   [7]   | `Effect.matchEffect(...)`   | branch both channels into one rail     |
|   [8]   | `Effect.either` / `option`  | project failure into data              |
|   [9]   | `Effect.sandbox`/`unsandbox` | lift `Cause<E>` and re-promote        |

[CAUSE_NORMALIZATION]:
- Law: parallel and sequential composition produce composite cause trees that `catchTag` cannot dispatch; `Cause.match` is the exhaustive catamorphism with `onEmpty`/`onFail`/`onDie`/`onInterrupt`/`onSequential`/`onParallel`, and `onSequential`/`onParallel` receive already-reduced values.
- Law: `Effect.sandbox` surfaces `Cause<unknown>` into the error channel for cause-aware retry impossible at the `E` level; `Effect.unsandbox` re-elevates, and the sandwich is lossless.
- Law: `Cause.isInterruptedOnly` is pure cancellation, `Cause.keepDefects` returns `Some` when a defect exists, `Cause.pretty` preserves tree topology where `Cause.squash` is lossy; a commutative fold combiner uses the same callback for both composition modes.
- Reject: a hand-rolled `_tag` switch over a cause that drops nested causes; `catchAll` absorbing a defect into the success channel.

[FAILURE_VS_DEFECT]:
- Law: a recoverable failure rides the typed `E` channel; an invariant violation is a defect through `Effect.die`/`Effect.dieMessage`, surfaced only by `sandbox` or `catchAllCause` — laundering a defect into a failure destroys postmortem fidelity.
- Law: only a boundary function translates a defect to a failure (`Cause.fail`) or a failure to a defect (`Effect.orDie`); domain code never catches a defect with `catchAll`.
- Boundary: `Schedule.whileInput` gates retry on a cause-level predicate — retry only on pure failures, never on interruption or a defect.

```ts conceptual
import { Cause, Data, Duration, Effect, Option, Schedule } from "effect"

class StoreFault extends Data.TaggedError("StoreFault")<{ readonly op: "append" | "compact"; readonly origin: "fail" | "defect" | "interrupt" }> {}

const boundaryClose = (op: StoreFault["op"]) =>
  <A, E, R>(self: Effect.Effect<A, E, R>): Effect.Effect<A, StoreFault, R> =>
    self.pipe(
      Effect.sandbox,
      Effect.retry(Schedule.exponential(Duration.millis(50)).pipe(
        Schedule.intersect(Schedule.recurs(2)),
        Schedule.whileInput((cause: Cause.Cause<E>) => !Cause.isInterruptedOnly(cause) && Option.isNone(Cause.keepDefects(cause))),
      )),
      Effect.mapError((cause) => new StoreFault({ op, origin: Cause.isInterruptedOnly(cause) ? "interrupt" : Option.isSome(Cause.keepDefects(cause)) ? "defect" : "fail" })),
      Effect.unsandbox,
      Effect.withSpan(`boundary.${op}`),
    )
```

## [5]-[EFFECT_RUNTIME]

An Effect carries its required context `R`; the runtime resolves it once at the composition edge.

[CONTEXT_ACCESS]:
- Law: the capability shape selects the access tier — an `Effect.Service` (with `dependencies` and `accessors`) owns a capability with construction, lifecycle, or a method family; a bare `Context.GenericTag` owns a value-only capability the layer supplies with no behavior; a `Context.Reference` with `defaultValue` owns ambient configuration that must resolve with zero `R` when unprovided (a request id, a feature flag, a `Clock` substitute) — promoting a `Reference` to a `Service` adds a phantom requirement edge, and demoting a `Service` to a `Reference` strands its finalizers.
- Use: `Effect.Service` accessors or a `yield*` inside the service constructor to read a capability; `Effect.serviceOption(Tag)` for an optional capability with zero `R`; `Context.Reference` with a `defaultValue` for the unprovided-resolves-to-default case.
- Access: a method closing over a yielded dependency inside the scoped constructor carries `R = never` for callers within scope; `accessors: true` generates static delegates observing `R = Self` for callers outside.
- Boundary: the layer graph constructs the context once at the root; context-derivation arithmetic (`Exclude<R, Tag>`) belongs to `boundaries.md`.
- Reject: a service-location wrapper, an ambient global, an `interface IService` beside an `Effect.Service` class (the class is both value and type), three parallel capability-access spellings for one capability, or a `R` tail threaded by hand where the constructor closure eliminates it.

[RESOURCE_BOUNDARY]:
- Use: `Effect.acquireRelease(acquire, release)` for paired lifetime, `Effect.acquireUseRelease` to seal the use phase against external interruption, `Effect.addFinalizer` for unpaired cleanup, and `Effect.ensuring`/`Effect.onExit` for exit-aware teardown.
- Law: finalizers run LIFO on success, failure, and interruption — the last-registered resource releases first; `acquireRelease` guarantees exactly-once release on every exit path.
- Law: `Effect.forkScoped` binds a drain fiber to the enclosing scope so scope closure interrupts it; `Effect.fork` inside a scoped generator leaks the fiber past teardown.
- Exemption: a `// BOUNDARY ADAPTER` callback inside a capsule that wires a foreign event handler is the named statement seam; it never appears in domain flow.
- Reject: resource lifetime hidden behind ordinary domain state; a finalizer running long token-aware work.

```ts conceptual
import { Duration, Effect, Queue, Schedule } from "effect"
import { SqlClient } from "@effect/sql"

class StoreService extends Effect.Service<StoreService>()("domain/Store", {
  scoped: Effect.gen(function* () {
    const sql = yield* SqlClient.SqlClient
    yield* Effect.acquireRelease(sql`SELECT pg_advisory_lock(42)`, () => sql`SELECT pg_advisory_unlock(42)`.pipe(Effect.ignore))
    const channel = yield* Effect.acquireRelease(Queue.bounded<string>(512), Queue.shutdown)
    const read = Effect.fn("Store.read")(
      (key: string) => sql<{ readonly value: string }>`SELECT value FROM kv WHERE key = ${key}`,
      (effect) => effect.pipe(
        Effect.retry(Schedule.exponential(Duration.millis(50)).pipe(Schedule.intersect(Schedule.recurs(3)), Schedule.jittered)),
        Effect.timeoutFail({ duration: Duration.seconds(5), onTimeout: () => new _StoreFault({ op: "query" }) }),
      ))
    return { read, channel } as const
  }),
}) {}
```

[SCHEDULE_POLICY]:
- Use: `Schedule` with `Effect.retry(schedule)`, `Effect.repeat(schedule)`, and `Effect.schedule` for retry, repeat, delay, timeout, and backoff when the local owner admits retry.
- Law: `Schedule.intersect` runs to the tighter of two policies (terminates when either exhausts), `Schedule.union` to the looser; unioning a finite curve onto an infinite one does not bound it.
- Law: `Schedule.whileInput`/`untilInput` gate on the error or output value, `Schedule.jittered` decorrelates, `Schedule.resetAfter` is the circuit-breaker primitive halting after N failures within a window, and `Schedule.addDelay(f)` derives an error-driven delay.
- Builders: `recurs`, `spaced`, `fixed`, `exponential`, `fibonacci`, `jittered`, `compose`, `intersect`, `union`, `resetAfter`, `whileInput`, `addDelay`.
- Reject: an ad-hoc delay loop; trusting an infinite backoff to stop itself.

## [6]-[STATE_AND_RECEIPTS]

State belongs at a boundary or service owner, not inside pure domain accumulation.

[MANAGED_STATE]:
- Use: `Ref<T>` for single-fiber mutable state with `Ref.update`/`Ref.modify`/`Ref.get`; `SynchronizedRef` for an effectful update; `FiberRef` for fiber-local context threaded through a subtree without a parameter.
- Law: `Ref.modify(f)` returns a value and the new state in one atomic step; `Ref.update` re-runs `f` on contention, so the transition is idempotent.
- Law: a `HashMap` threaded through `Stream.mapAccum` is the immutable accumulator — never a `new Map()` with `.set` mutation, which breaks referential transparency under retry.
- Reject: a `let` accumulator, a shared `var`, or a hidden global cell disguised as functional flow.

[TRANSACTIONAL_STATE]:
- Use: `STM` with `TRef`, `TMap`, `TQueue`, `TSemaphore`, and `TDeferred` when two or more cells must transition atomically; `STM.commit` wraps the whole read-update-derive cycle as one linearizable transaction that retries on conflict.
- Law: a multi-cell transition that must observe a consistent snapshot is STM, not a single `Ref`; a `TMap.getOrElse → STM.tap(TMap.set) → STM.map` pipeline composes with no interleaving between read and write.
- Boundary: contention admission — `Queue.bounded`/`dropping`/`sliding`, `Semaphore.withPermits`/`withPermitsIfAvailable` — is the same admit-or-shed algebra driven by one policy vocabulary; the queue strategy is the data-loss contract.
- Reject: a lock or a race window where STM composes the atomic commit.

```ts conceptual
import { Effect, Option, STM, TMap, TQueue } from "effect"

const claim = (queue: TQueue.TQueue<string>, state: TMap.TMap<string, "active">) =>
  STM.commit(
    TQueue.take(queue).pipe(
      STM.tap((id) => TMap.set(state, id, "active")),
      STM.flatMap((id) => STM.map(TMap.size(state), (active) => ({ id, active }))),
    ),
  )
```

[RECEIPTS]:
- Law: the split is capability — a `Stream` answers what happened and when, a typed receipt answers how this computation resolved; collapsing a receipt into a stream, or a generic ledger over typed proof, erases the typed evidence.
- Law: one receipt `Data.Class` carries a kind discriminant, a slot identifier, and a payload; adds, updates, removals, and errors are tag variants over one `Ref<Chunk<Receipt>>`, never parallel record types, and projections are pure folds over that one stream.
- Law: keep a typed receipt when fields carry solver, sampling, route, status, metric, or proof evidence; `Ref<Receipt>` holds the latest, history escalates to `Ref<Chunk<Receipt>>`.

```ts conceptual
import { Data, Number as N } from "effect"

class Receipt extends Data.Class<{ readonly code: string; readonly count: number }> {
  static readonly empty = new Receipt({ code: "SUM", count: 0 })
  static readonly combine = (l: Receipt, r: Receipt) => new Receipt({ code: l.code, count: N.sum(l.count, r.count) })
}
```
</content>
