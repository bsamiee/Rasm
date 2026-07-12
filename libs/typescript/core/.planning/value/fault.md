# [CORE_FAULT]

The one fault-policy owner of the branch: `FaultClass` is the severity-ordered ten-class vocabulary every rail inherits — each row carrying rank, retryability, and blame — `FaultCapture`/`FaultEnricher` are the crash-evidence value and the enrichment port, `Budget` compiles the retry/timeout rows once into jittered, class-gated `Schedule` values, and `Degrade` is the connection-degradation ladder long-lived feeds fold silence through. Taxonomy, evidence, schedule compilation, and degradation are four clusters of one module because they share one axis: the class table's `retryable` column IS the gate every compiled schedule carries, so routing, dominance folds, budget gates, and blame projections read one table instead of re-deriving semantics per folder. The three fault altitudes stay distinct — interchange reconstruction, per-folder `Data.TaggedError` rails, outbound status mapping — and this floor imports none of them. The module is `core/src/value/fault.ts`; a new fault class is one tuple entry plus one row, a new budget is one row, a new degradation posture is one ladder rung.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]           | [OWNS]                                                           | [PUBLIC]                        |
| :-----: | :------------------ | :--------------------------------------------------------------- | :------------------------------ |
|  [01]   | `CLASS_VOCABULARY`  | the ten-class table, classification fold, dominance lattice      | `FaultClass`                    |
|  [02]   | `ENRICHER_CONTRACT` | the capture evidence model and the enrichment port               | `FaultCapture`, `FaultEnricher` |
|  [03]   | `RETRY_BUDGET`      | the budget rows and their compiled class-gated `Schedule` values | `Budget`                        |
|  [04]   | `DEGRADE_LADDER`    | the silence-threshold ladder and its level fold                  | `Degrade`                       |

## [02]-[CLASS_VOCABULARY]

[CLASS_VOCABULARY]:
- Owner: `FaultClass`, the assembled vocabulary — the interior key tuple fixes severity order as iteration order (rank ascends with position, a load-bearing sequence), the interior row table carries the axes, the merged hub carries every derived type plus the guard pair, and the exported owner assembles rows, `kinds`, `schema`, and the operation members under a `typeof`-derived stated annotation.
- Law: the roster is sized by cross-language routing, never by cause — `absent`, `conflicted`, `invalid`, `malformed`, `denied`, `expired`, `exhausted`, `unavailable`, `breached`, `defect` — and a finer cause is a `reason` row inside the owning folder's fault class, never an eleventh entry minted for one surface.
- Law: rows carry three axes — `rank` (the dominance lattice a report folds under), `retryable` (the transient family a budget gate re-drives), `blame` (`"caller"` | `"system"`, the accountability split outbound status mapping and telemetry project) — and behavior variation across the branch reads these columns, never a `switch` over class names.
- Law: the classification convention is structural — a folder fault carries `readonly class: FaultClass.Kind` as a field, `FaultClass.of` probes any value for it and folds everything else to `"defect"`, so classification is total over `unknown` and an unclassified foreign throw lands at the correct terminal severity.
- Law: `dominant` collapses an accumulated non-empty fault set to its representative through the rank lattice — the fold `Effect.validateAll`-shaped reports feed — and `retryable` is the one gate `[04]`'s sibling `Budget` compiles into every schedule.
- Law: `schema` is the wire-facing literal union derived from the tuple spread — the non-empty overload keeps the exact literal tuple — so a class crossing a wire or a config row decodes against the same anchor the type plane derives from.
- Growth: a new class is one tuple entry plus one row — every guard, schema, fold, and budget gate inherits it at compile time; a new axis is one `Row` field plus its column on each row.
- Boundary: the class-to-status outbound mapping is the serving edge's governed record; the floor table stays transport-free.
- Packages: `effect` (`Schema`, `Order`, `Array`, `Predicate`).

```typescript
import * as Monoid from "@effect/typeclass/Monoid"
import * as Semigroup from "@effect/typeclass/Semigroup"
import * as RecordInstances from "@effect/typeclass/data/Record"
import {
  ATTR_ERROR_TYPE, ATTR_EXCEPTION_ESCAPED, ATTR_EXCEPTION_MESSAGE, ATTR_EXCEPTION_STACKTRACE, ATTR_EXCEPTION_TYPE, EVENT_EXCEPTION,
} from "@opentelemetry/semantic-conventions"
import { Array, Context, Duration, Effect, Layer, Option, Order, Predicate, Schedule, Schema, type Types } from "effect"
import { Refined } from "./schema.ts"

const _kinds = [
  "absent",
  "conflicted",
  "invalid",
  "malformed",
  "denied",
  "expired",
  "exhausted",
  "unavailable",
  "breached",
  "defect",
] as const

const _rows = {
  absent: { rank: 1, retryable: false, blame: "caller" },
  conflicted: { rank: 2, retryable: true, blame: "caller" },
  invalid: { rank: 3, retryable: false, blame: "caller" },
  malformed: { rank: 4, retryable: false, blame: "caller" },
  denied: { rank: 5, retryable: false, blame: "caller" },
  expired: { rank: 6, retryable: true, blame: "system" },
  exhausted: { rank: 7, retryable: true, blame: "system" },
  unavailable: { rank: 8, retryable: true, blame: "system" },
  breached: { rank: 9, retryable: false, blame: "system" },
  defect: { rank: 10, retryable: false, blame: "system" },
} as const

const _Kind = Schema.Literal(..._kinds)
const _is = Schema.is(_Kind)
const _byRank = Order.mapInput(Order.number, (kind: FaultClass.Kind) => _rows[kind].rank)

const _of = (fault: unknown): FaultClass.Kind =>
  Predicate.hasProperty(fault, "class") && _is(fault.class) ? fault.class : "defect"

declare namespace FaultClass {
  type Kinds = typeof _kinds
  type Kind = keyof typeof _rows
  type Blame = "caller" | "system"
  type Row = { readonly rank: number; readonly retryable: boolean; readonly blame: Blame }
  type Contract = { readonly [K in Kinds[number]]: Row }
  type Shape = Types.Simplify<
    typeof _rows & {
      readonly kinds: Kinds
      readonly schema: typeof _Kind
      readonly is: (input: unknown) => input is Kind
      readonly of: (fault: unknown) => Kind
      readonly retryable: (fault: unknown) => boolean
      readonly dominant: (classes: Array.NonEmptyReadonlyArray<Kind>) => Kind
    }
  >
  type _Rows<T extends Contract = typeof _rows> = T
  type _Keys<K extends keyof Contract = Kind> = K
}

const FaultClass: FaultClass.Shape = {
  ..._rows,
  kinds: _kinds,
  schema: _Kind,
  is: _is,
  of: _of,
  retryable: (fault) => _rows[_of(fault)].retryable,
  dominant: (classes) => Array.max(classes, _byRank),
}
```

## [03]-[ENRICHER_CONTRACT]

[ENRICHER_CONTRACT]:
- Owner: `FaultCapture`, the floor-shaped crash-evidence model — class, fault tag, owning surface, detail, optional `Refined.Guid` correlation, capture instant, and an open string attribute band — the value the runtime crash owner constructs from a folded `Cause` and every enrichment round-trips; `policy` projects the class row so severity and blame are recoverable from any capture, `enriched` is the successor constructor merging attribute bands, and `forensic` is the exception-evidence successor writing the well-known crash rows through the typed key vocabulary.
- Owner: `FaultEnricher`, the enrichment port — one `Context.Tag` whose service is a single endo-arrow `enrich: (capture) => Effect<FaultCapture>` — the interchange codec provides the Layer that reconstructs wire-grade forensics into the attribute band, the runtime crash owner yields the Tag, and the app root wires them; the interchange-owned reconstruction names never appear in this contract, keeping the adopted-verbatim vocabulary on its owning side.
- Law: enrichment is total by signature — the error channel is `never`, so a failing enricher implementation resolves its own faults internally (degrade to the unenriched capture) and crash capture can never be broken by its own forensics.
- Law: `FaultEnricher.identity` is the shipped no-wire Layer — pass-through enrichment for the archetypes that select no interchange — so every composition root wires the port and absence of an implementation is a selection, never a crash.
- Law: the attribute band is string-to-string data — identifier-grade context rides it per-occurrence; bounded dimensions for metrics derive from `class` and `blame` columns, never from band values.
- Law: the well-known crash rows are typed constants, never free strings — `FaultCapture.Forensic` anchors `ATTR_EXCEPTION_TYPE`/`ATTR_EXCEPTION_MESSAGE`/`ATTR_EXCEPTION_STACKTRACE`/`ATTR_EXCEPTION_ESCAPED`/`ATTR_ERROR_TYPE` and `FaultCapture.event` anchors `EVENT_EXCEPTION`, so the enricher writes the exact keys the crash event emits under and a misspelled forensic key is a compile error; the band stays open for context beyond the vocabulary, and `observe/convention` still owns attribute-space naming for its own projections.
- Law: band merging is one instance, never a per-site combine — `getSemigroupUnion(Semigroup.last())` declares the last-write-wins keyed merge once, `Monoid.fromSemigroup` names the empty band as its lawful identity, and `enriched` and `forensic` both project the one instance so enrichment stages fold with no emptiness guard.
- Growth: a new evidence field is one `FaultCapture` field; a new well-known key is one `Forensic` row; a second enrichment stage is a Layer composing the same Tag, never a second port.
- Boundary: which captures reach the enricher, redaction-at-capture, and OTLP egress encoding are the runtime telemetry owners' policies; reconstruction internals are the interchange codec's; this floor declares the shapes, the key vocabulary, and the seam.
- Packages: `effect` (`Schema`, `Context`, `Effect`, `Layer`); `@effect/typeclass` (`Monoid`, `Semigroup`, `data/Record`); `@opentelemetry/semantic-conventions` (`ATTR_EXCEPTION_*`, `ATTR_ERROR_TYPE`, `EVENT_EXCEPTION`); `schema#REFINED_FLOOR` (`Refined.Guid`).

```typescript
const _FORENSIC = {
  errorType: ATTR_ERROR_TYPE,
  escaped: ATTR_EXCEPTION_ESCAPED,
  message: ATTR_EXCEPTION_MESSAGE,
  stacktrace: ATTR_EXCEPTION_STACKTRACE,
  type: ATTR_EXCEPTION_TYPE,
} as const

const _Band: Monoid.Monoid<{ readonly [key: string]: string }> = Monoid.fromSemigroup(
  RecordInstances.getSemigroupUnion(Semigroup.last<string>()), // one keyed merge law: keys union, collisions last-write-wins
  {},                                                          // the empty band is the lawful identity, named explicitly — last() alone admits none
)

class FaultCapture extends Schema.Class<FaultCapture>("FaultCapture")({
  class: _Kind,
  tag: Schema.NonEmptyString,
  surface: Schema.NonEmptyString,
  detail: Schema.String,
  correlation: Schema.optionalWith(Refined.Guid, { as: "Option" }),
  at: Schema.DateTimeUtcFromSelf,
  attributes: Schema.Record({ key: Schema.String, value: Schema.String }),
}) {
  static readonly Forensic: typeof _FORENSIC = _FORENSIC
  static readonly event: typeof EVENT_EXCEPTION = EVENT_EXCEPTION
  get policy(): FaultClass.Row {
    return _rows[this.class]
  }
  enriched(added: { readonly [key: string]: string }): FaultCapture {
    return new FaultCapture({ ...this, attributes: _Band.combine(this.attributes, added) })
  }
  forensic(evidence: FaultCapture.Evidence): FaultCapture {
    return this.enriched({
      [_FORENSIC.errorType]: evidence.type,
      [_FORENSIC.escaped]: String(evidence.escaped),
      [_FORENSIC.message]: evidence.message,
      [_FORENSIC.type]: evidence.type,
      ...(evidence.stacktrace !== undefined && { [_FORENSIC.stacktrace]: evidence.stacktrace }),
    })
  }
}

declare namespace FaultCapture {
  type Evidence = { readonly type: string; readonly message: string; readonly escaped: boolean; readonly stacktrace?: string }
  type Forensic = (typeof _FORENSIC)[keyof typeof _FORENSIC]
}

class FaultEnricher extends Context.Tag("@rasm/ts/core/FaultEnricher")<FaultEnricher, {
  readonly enrich: (capture: FaultCapture) => Effect.Effect<FaultCapture>
}>() {
  static readonly identity: Layer.Layer<FaultEnricher> = Layer.succeed(FaultEnricher, { enrich: Effect.succeed })
}
```

## [04]-[RETRY_BUDGET]

[RETRY_BUDGET]:
- Owner: `Budget`, the assembled budget vocabulary — the interior key tuple anchors the roster, the row table carries every axis as `Duration` policy values, the merged hub carries derived types plus the guard pair, and the exported owner assembles rows, `kinds`, and the `schedule` lookup under a `typeof`-derived stated annotation; the ingress decode ceilings are `schema#INGRESS_CEILING`'s `Ingress` — the two vocabularies never share a concept.
- Law: four rows ride the floor — `pulse` (interactive point ops: 40ms base, 4 attempts, 2s window), `lease` (infrastructure ops: 250ms base, 6 attempts, 20s window), `bulk` (batch work: 1s base, 8 attempts, 5m window), `feed` (long-lived reconnection: 500ms base, 64 attempts, 2m window, 90s reset) — floors a folder policy references by kind; a genuinely novel envelope is a new row, never a per-site literal.
- Law: every row carries the two deadline budgets the rails layering law consumes — `attempt` composes below the retry transformer (per-try), `total` above it (whole-call) — so the interchange invocation client and runtime work activities read `Budget[kind].attempt`/`.total` and the budget's whole geometry lives in one row.
- Law: compilation is fixed-form and total — `exponential(base, factor)` → `jittered` → `resetAfter(reset)` → `intersect(recurs(attempts))` → `upTo(window)` — jitter is unconditional (a bare curve synchronizes a fleet into waves), `resetAfter` re-arms base delay after quiet so the next outage never inherits the last one's escalated tail, and the attempt/elapsed bounds stack because a budget names both.
- Law: every compiled schedule is class-gated through `Schedule.whileInput` over `FaultClass.retryable` — only the transient family re-drives, the gate travels with the policy value, and a call-site predicate re-deriving retryability is policy leakage; the compile happens once at module init into a governed record whose stated annotation demands a schedule per row.
- Law: the schedule input is `unknown` — one policy value serves every fault channel in the branch, and classification, not typing, decides re-drive.
- Growth: a new budget is one tuple entry plus one row — the governed compile record breaks at compile time until its schedule lands; a new axis (a fleet-cost weight, a hedge delay) is one `Row` field consumed by the surfaces that name it.
- Boundary: which budget a surface selects is that folder's policy row; deadline transformers (`Effect.timeoutFail`) compose at the owning `Effect.fn` seam with the row's durations — the floor ships values, never wrappers.
- Packages: `effect` (`Duration`, `Schedule`).

```typescript
const _budgets = ["pulse", "lease", "bulk", "feed"] as const
const _budgetRows = {
  pulse: {
    base: Duration.millis(40),
    factor: 2,
    attempts: 4,
    window: Duration.seconds(2),
    reset: Duration.seconds(30),
    attempt: Duration.seconds(1),
    total: Duration.seconds(8),
  },
  lease: {
    base: Duration.millis(250),
    factor: 2,
    attempts: 6,
    window: Duration.seconds(20),
    reset: Duration.seconds(90),
    attempt: Duration.seconds(5),
    total: Duration.seconds(45),
  },
  bulk: {
    base: Duration.seconds(1),
    factor: 2,
    attempts: 8,
    window: Duration.minutes(5),
    reset: Duration.minutes(10),
    attempt: Duration.minutes(2),
    total: Duration.minutes(15),
  },
  feed: {
    base: Duration.millis(500),
    factor: 2,
    attempts: 64,
    window: Duration.minutes(2),
    reset: Duration.seconds(90),
    attempt: Duration.seconds(10),
    total: Duration.minutes(30),
  },
} as const

declare namespace Budget {
  type Kinds = typeof _budgets
  type Kind = keyof typeof _budgetRows
  type Row = {
    readonly base: Duration.Duration
    readonly factor: number
    readonly attempts: number
    readonly window: Duration.Duration
    readonly reset: Duration.Duration
    readonly attempt: Duration.Duration
    readonly total: Duration.Duration
  }
  type Contract = { readonly [K in Kinds[number]]: Row }
  type Gated = Schedule.Schedule<[Duration.Duration, number], unknown>
  type Shape = Types.Simplify<
    typeof _budgetRows & {
      readonly kinds: Kinds
      readonly schedule: (kind: Kind) => Gated
    }
  >
  type _Rows<T extends Contract = typeof _budgetRows> = T
  type _Keys<K extends keyof Contract = Kind> = K
}

const _gated = (row: Budget.Row): Budget.Gated =>
  Schedule.exponential(row.base, row.factor).pipe(
    Schedule.jittered,
    Schedule.resetAfter(row.reset),
    Schedule.intersect(Schedule.recurs(row.attempts)),
    Schedule.upTo(row.window),
    Schedule.whileInput(FaultClass.retryable),
  )

const _compiled: { readonly [K in Budget.Kind]: Budget.Gated } = {
  pulse: _gated(_budgetRows.pulse),
  lease: _gated(_budgetRows.lease),
  bulk: _gated(_budgetRows.bulk),
  feed: _gated(_budgetRows.feed),
}

const Budget: Budget.Shape = {
  ..._budgetRows,
  kinds: _budgets,
  schedule: (kind) => _compiled[kind],
}
```

## [05]-[DEGRADE_LADDER]

[DEGRADE_LADDER]:
- Owner: `Degrade`, the connection-degradation ladder — an interior level tuple in escalation order (a load-bearing sequence: each rung's silence threshold exceeds its predecessor's), a row table carrying per-level entry threshold and probe cadence, and the exported owner assembling rows, `levels`, and the `level` fold under a stated annotation.
- Law: three rungs ride the ladder — `live` (healthy: zero threshold, 30s heartbeat cadence), `lagging` (10s of silence: 5s probe cadence), `severed` (2m of silence: 30s probe cadence) — and `level(silence)` folds an observed silence span to its rung by walking the tuple from the top, so the ladder's shape is data and the fold never spells a level name.
- Law: the ladder is a reconnection BUDGET, not evidence — event-log sync, live flag feeds, and presence streams fold their silence through it to pick probe cadence; the wire-decoded degradation-level evidence vocabulary is the `state` evidence family's sibling concern and the two never merge.
- Law: cadence at a rung is a `Duration` policy value a consumer hands to `Schedule.spaced` or `Stream.repeatEffectWithSchedule` at its own seam — the ladder prices the probe, the surface owns the loop.
- Growth: a new rung is one tuple entry plus one row in threshold order; a per-surface ladder override is a caller-composed row set folded through the same `level` shape.
- Boundary: what counts as silence — missed heartbeats, an idle socket, a stalled pull — is the consuming surface's measurement; the ladder folds the span it is handed. The ladder is class-free by design and composes nothing from `[02]`.
- Packages: `effect` (`Duration`, `Option`, `Array`).

```typescript
const _levels = ["live", "lagging", "severed"] as const
const _ladder = {
  live: { after: Duration.zero, cadence: Duration.seconds(30) },
  lagging: { after: Duration.seconds(10), cadence: Duration.seconds(5) },
  severed: { after: Duration.minutes(2), cadence: Duration.seconds(30) },
} as const

declare namespace Degrade {
  type Levels = typeof _levels
  type Kind = keyof typeof _ladder
  type Row = { readonly after: Duration.Duration; readonly cadence: Duration.Duration }
  type Contract = { readonly [K in Levels[number]]: Row }
  type Shape = Types.Simplify<
    typeof _ladder & {
      readonly levels: Levels
      readonly level: (silence: Duration.DurationInput) => Kind
    }
  >
  type _Rows<T extends Contract = typeof _ladder> = T
  type _Keys<K extends keyof Contract = Kind> = K
}

const _leveled = (silence: Duration.DurationInput): Degrade.Kind =>
  Option.getOrElse(
    Array.findLast(_levels, (kind) => Duration.greaterThanOrEqualTo(silence, _ladder[kind].after)),
    () => "live",
  )

const Degrade: Degrade.Shape = {
  ..._ladder,
  levels: _levels,
  level: _leveled,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Budget, Degrade, FaultCapture, FaultClass, FaultEnricher }
```
