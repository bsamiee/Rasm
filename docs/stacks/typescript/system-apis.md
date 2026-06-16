# [SYSTEM_APIS]

Effect-ecosystem modules replace JS stdlib machinery whenever they own the concern. They do not replace `Schema` shapes, `Match` dispatch, `Layer` wiring, or the typed Effect rails. The default is the Effect data structure with structural equality, persistent update, and `Equal`/`Hash` integration; JS stdlib `Map`/`Set`/`Array`/`Object`/`null` survive only at FFI and serialization boundaries marked `// BOUNDARY ADAPTER`.

## [1]-[SMELL_LOOKUP]

This table is a lookup by repeated local smell.

| [INDEX] | [SMELL]                                  | [OWNER]                              |
| :-----: | :--------------------------------------- | :----------------------------------- |
|   [1]   | `new Map()` with `.set` mutation         | `HashMap`                            |
|   [2]   | `new Set()` with `.add` mutation         | `HashSet`                            |
|   [3]   | `Array.push` accumulation in a fold      | `Chunk` / `Array.reduce`             |
|   [4]   | `Object.keys`/`entries`/`fromEntries`    | the `Record` module                  |
|   [5]   | manual object pick/omit                  | the `Struct` module                  |
|   [6]   | `null`/`undefined` for domain absence    | `Option`                             |
|   [7]   | raw millisecond literal for a duration   | `Duration`                           |
|   [8]   | hand-written comparator                  | `Order` / `Equivalence`              |
|   [9]   | inline boolean predicate composition     | the `Predicate` module               |
|  [10]   | `.filter(p).map(f)` two-pass             | `Array.filterMap` / `Array.getSomes` |
|  [11]   | division producing `NaN`/`Infinity`      | `Number.divide` returning `Option`   |
|  [12]   | delimiter-concatenated equality key      | structured tuple + `Equivalence`     |
|  [13]   | manual `JSON.parse`/`stringify` codec    | `Schema` decode/encode               |
|  [14]   | `Promise` chain for async flow           | `Effect`                             |
|  [15]   | `Date.now()` delta or `setTimeout` delay | `Clock` / `Effect.sleep`             |
|  [16]   | `node:crypto` hash inline in domain flow | content-address boundary owner       |
|  [17]   | unbounded `new Map()` mutated as a cache | bounded `Queue` / sized `HashMap`    |

## [2]-[COLLECTIONS]

[IMMUTABLE_STRUCTURES]:
- Owner: `HashMap<K,V>` for keyed lookup and accumulator state, `HashSet<T>` for membership, `Chunk<T>` for streaming and batching, the `Array` module for small fixed collections.
- Replace: `new Map()`/`new Set()` with `.set`/`.add` mutation, `Array.push` accumulation, and `[...arr]` spread-copy update; `HashMap.set` and `Chunk.append` return new structures with structural sharing.
- Gate: `HashMap.fromIterable` admits a foreign iterable once at the boundary; `Chunk.toReadonlyArray` and `HashMap` entry iteration project back to an array only at a transfer edge.
- Rule: a keyed accumulator threaded through `Stream.mapAccum` or `Array.reduce` is a `HashMap` — mutation inside a fold breaks referential transparency under retry; `Equal`/`Hash` integration makes a `HashSet` of tagged values compare structurally with no comparator.

[RECORD_AND_STRUCT]:
- Owner: the `Record` module (`Record.keys`, `Record.map`, `Record.toEntries`, `Record.fromEntries`, `Record.filter`) over `Object.keys`/`entries`/`fromEntries`; the `Struct` module (`Struct.pick`, `Struct.omit`, `Struct.evolve`) over manual object picking.
- Replace: an `Object.entries(...).map(...)` round trip, a manual `{ ...obj, key: value }` projection, and a hand-written key-preserving fold.
- Rule: `Record.map` is the key-preserving (structural) fold; `Effect.mergeAll` is the key-collapsing (statistical) fold — the retention contract is the selection axis. `Record.keys(V)` spreads into a `Schema.Literal(...)` so the wire vocabulary derives from the same anchor.

```ts conceptual
import { Array as A, HashMap, Number as N, Option } from "effect"

const tally = (rows: ReadonlyArray<{ readonly key: string; readonly count: number }>): HashMap.HashMap<string, number> =>
  A.reduce(rows, HashMap.empty<string, number>(), (acc, { key, count }) =>
    HashMap.modify(acc, key, (prior) => N.sum(prior, count)).pipe(
      (next) => HashMap.has(next, key) ? next : HashMap.set(acc, key, count),
    ))
```

## [3]-[VALUES_AND_PREDICATES]

[DURATION_AND_ABSENCE]:
- Owner: `Duration` for every timeout, delay, schedule interval, and vocabulary policy field; `Option<T>` for every domain absence with `Option.fromNullable` at the boundary.
- Replace: a raw millisecond literal (`5000`), a `null`/`undefined` domain return, and an `x ?? fallback` where `Option.getOrElse` carries the canonical default from a vocabulary row.
- Rule: `Duration.seconds(5)` over `5000`, `Duration.greaterThan`/`toMillis` for comparison and egress; `undefined` survives only as the decoded shape under `exactOptionalPropertyTypes`, projected to `Option` at admission.

[ORDER_AND_EQUIVALENCE]:
- Owner: `Order` (`Order.struct`, `Order.combine`, `Order.mapInput`, `Order.max`) and `Equivalence` (`Equivalence.make`, `Equivalence.mapInput`) over hand-written comparators.
- Replace: a multi-field `(a, b) => a.x - b.x || a.y - b.y` comparator, a delimiter-concatenated equality key, and an inline `>=` lattice merge.
- Rule: `Order.mapInput(Order.number, projector)` lifts a vocabulary ordinal into an `Order` over a fault, and `Order.max` is the lattice join; `Equivalence.make` over a compound cache key compares fields directly, deleting the `${a}:${b}` signature string.

[PREDICATE_AND_NUMBER]:
- Owner: the `Predicate` module (`Predicate.and`, `Predicate.or`, `Predicate.not`) over inline boolean composition; the `Number` module (`Number.divide`, `Number.clamp`, `Number.sum`, `Number.multiply`) over raw arithmetic on a fallible path.
- Replace: a chained `&&`/`||`/`!` predicate, a `.filter(p).map(f)` two-pass with `Array.filterMap`, and a bare `a / b` that can produce `NaN`/`Infinity`.
- Rule: `Number.divide` returns `Option<number>`, forcing an explicit `Option.getOrElse` at every zero-denominator site so no `NaN` propagates; `Number.clamp({ minimum, maximum })` post-gates a normalized scalar.

```ts conceptual
import { Number as N, Option, Order, Predicate, pipe } from "effect"

const _byPriority = Order.mapInput(Order.number, (r: { readonly priority: number }) => r.priority)
const _live = Predicate.and((r: { readonly active: boolean }) => r.active, (r: { readonly weight: number }) => r.weight > 0)

const _saturation = (k: number, cap: number): number =>
  pipe(N.divide(k, cap), Option.map((r) => 1 - N.min(r, 1)), Option.getOrElse(() => 1))
```

## [4]-[FUNCTION_AND_CODEC]

[COMPOSITION]:
- Owner: `Function.pipe`/`Function.flow` over manual composition; `Effect` over a `Promise` chain for async flow.
- Replace: a nested call composition `f(g(h(x)))`, a `const tmp = ...; tmp = ...` reassignment chain, and a `.then(...).catch(...)` Promise pipeline.
- Rule: `pipe(value, f, g)` threads a value, `flow(f, g)` composes point-free; a `Promise` enters domain flow only through `Effect.tryPromise` at the boundary.

[CODEC]:
- Owner: `Schema` decode/encode over manual `JSON.parse`/`stringify`; `Schema.transform` over a hand-built wire-to-domain projection.
- Replace: a `JSON.parse` followed by manual field validation, a `JSON.stringify` over an unvalidated shape, and a regex extraction from a rendered string where the structured form is available.
- Rule: `Schema.decodeUnknown` admits untrusted input and lifts its `ParseError` into the error channel; the codec is the single source for the shape, never a parse step plus a separate validation step.

## [5]-[RUNTIME_AND_INTEGRITY]

[TIME]:
- Owner: `Clock` (`Clock.currentTimeMillis`, `Clock.currentTimeNanos`, `Clock.clockWith`) read through the ambient service, `Effect.sleep`/`Effect.timeout`/`Schedule` for delay and pacing, and `TestClock.adjust`/`setTime` as the deterministic test seam.
- Replace: a `Date.now()` delta, a raw `setTimeout`/`setInterval` in domain flow, a `performance.now()` benchmark threaded by hand, and a wall-clock comparison that cannot be advanced under test.
- Rule: `Clock` is the single testable time seam — every time-accepting primitive (`Effect.sleep`, `Effect.timeout`, `Schedule.exponential`, `Effect.repeat`) reads the same provider, so `TestClock.adjust(Duration.seconds(n))` advances all of them at once; `Date.now()` survives only inside a `// BOUNDARY ADAPTER` wire timestamp.
- Reject: ambient wall-clock reads inside a retry predicate, a schedule, or any transform whose test must be deterministic.

[INTEGRITY]:
- Owner: `Hash` (`Hash.hash`, the `Hash`/`Equal` integration on every Effect data structure) for in-memory structural identity and `HashMap`/`HashSet` keying; `node:crypto` (`createHash`, `timingSafeEqual`) behind a `// BOUNDARY ADAPTER` capture for content-addressed artifact bytes, signatures, and idempotency keys.
- Replace: a `JSON.stringify`-then-compare equality, an XOR/prime hash combine, a `===` on two secret buffers (timing-leaking), and `Math.random()` for any non-cosmetic identifier.
- Rule: `Hash`/`Equal` are process-local and non-stable — never persist a `Hash.hash` output or use it as a content address; a content-addressed GLB or wire artifact crosses as a `Uint8Array` plus its `node:crypto` digest captured once at the boundary, never parsed and re-serialized between verification and forwarding (the byte-identity law in `boundaries.md`).
- Gate: a checksum over a streamed artifact computes incrementally over the `Stream`'s `Chunk`s at the wire fence, not after a full-buffer materialization.

[BACKPRESSURE]:
- Owner: `Queue.bounded`/`Queue.dropping`/`Queue.sliding` and the matching `PubSub` constructors, drained through `Stream.fromQueue` with `Stream.groupedWithin` for batch pacing.
- Replace: a `new Map()`/array mutated as an unbounded in-flight buffer, a `Promise.all` over an open-ended producer that retains every pending result, and a hand-rolled semaphore loop.
- Rule: the carrier capacity and full-mode are the declared backpressure policy stated at construction — `bounded` suspends the producer fiber, `dropping` discards the newest offer, `sliding` evicts the oldest; the unbounded form is admitted only when the producer is provably finite.
- Boundary: the producer/consumer handoff topology — latest-value cell versus full log — is the `[HANDOFF_DRAIN]` decision owned by `boundaries.md`; this site owns the capacity-and-eviction policy.

```ts conceptual
import { Cause, Chunk, Clock, Data, Duration, Effect, Equal, Hash, Queue, Scope, Stream } from "effect"

const _staleness = (recordedAt: number): Effect.Effect<Duration.Duration> =>
  Clock.currentTimeMillis.pipe(Effect.map((now) => Duration.millis(Math.max(0, now - recordedAt))))

const _digest = (bytes: Uint8Array): Effect.Effect<string, Cause.UnknownException> =>
  Effect.tryPromise(async () => {
    const { createHash } = await import("node:crypto") // BOUNDARY ADAPTER: content-address capture
    return createHash("sha256").update(bytes).digest("hex")
  })

const _idempotencyKey = (frame: Data.Data<{ readonly tenant: string; readonly seq: number }>): number => Hash.hash(frame)
const _sameFrame = (left: Data.Data<{ readonly tenant: string; readonly seq: number }>, right: typeof left): boolean => Equal.equals(left, right)

const _meter = <A>(source: Stream.Stream<A>): Effect.Effect<Queue.Dequeue<ReadonlyArray<A>>, never, Scope.Scope> =>
  Queue.sliding<ReadonlyArray<A>>(256).pipe(
    Effect.tap((q) => source.pipe(Stream.groupedWithin(64, Duration.millis(100)), Stream.map(Chunk.toReadonlyArray), Stream.runForEach((b) => Queue.offer(q, b)), Effect.forkScoped)),
  )
```

## [6]-[ABSTRACTION_COLLAPSE_TESTS]

Use these tests before keeping local machinery beside an ecosystem primitive.

[STDLIB_LEAKAGE]:
- Smell: `new Map()`/`new Set()`/`Array.push`/`Object.entries`/`null` appear in domain code.
- Collapse: use the Effect data structure that carries structural equality and persistent update; project to a JS stdlib shape only at a marked boundary.
- Done when: domain flow holds no mutable stdlib collection and no `null` past the decode seam.

[MAGIC_VALUE]:
- Smell: a duration, an ordinal, a threshold, or a status is a bare literal scattered across call sites.
- Collapse: move the value into an `as const satisfies Record` vocabulary row read by indexed access, or the `Duration`/`Order`/`Number` primitive that carries its semantics.
- Done when: the value carries its own meaning from one anchor and no inline literal restates it.

[STRING_RECOVERY]:
- Smell: code parses a rendered string, a delimiter-joined key, or a JSON blob by hand.
- Collapse: ask `Schema` for the decoded structure or compare structured fields through `Equivalence`.
- Done when: the implementation consumes typed components, not reconstructed text.
</content>
