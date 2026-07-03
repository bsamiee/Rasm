# [TELEMETRY_METER]

Usage metering is the billing and cost-attribution source every multi-tenant archetype rolls up, and it is one fact family under one law: `MeterFact` carries an `(app, tenant)`-keyed quantity against the closed resource vocabulary — `request`, `compute`, `storage`, `token` — with an `Option`-carried surface attribution; durability rides the same fact-journal port law `signal/audit` declares (`Audit.Journal<MeterFact>` under its own Tag, the shared `JournalFault`); rating is exact arithmetic — `BigDecimal` rate rows folded over rollup aggregates, never a float — and the OTLP metric egress is a deliberately lossy aggregate beside the journal: the resource-kind tag is always bounded, the tenant tag operates only under the export policy's declared cardinality budget, and the journal remains the sole billing truth.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]  | [OWNS]                                                                       |
| :-----: | :--------- | :----------------------------------------------------------------------------- |
|  [01]   | [FACT]     | the resource vocabulary, the `MeterFact` owner, the meter journal Tag           |
|  [02]   | [RATING]   | rollup as a keyed fold and rating as exact `BigDecimal` policy evaluation       |
|  [03]   | [RAIL]     | the `Meter` service: modal `charge`, buffered drain, bounded metric egress      |

## [2]-[FACT]

[FACT]:
- Owner: the `_rows` resource vocabulary — one row per metered resource carrying its unit and its OTLP-egress tenancy posture — and the `MeterFact` class: resource key, integral `quantity` in the row's unit, identity stamp (`app`, `tenant`), `Option` surface attribution, `at` instant.
- Law: quantities are integral by schema (`Schema.Int` + nonnegative) — a count of requests, milliseconds of compute, bytes of storage, tokens consumed — so the exact-arithmetic crossing in rating never meets a float; a fractional metering need is a smaller unit row, never a decimal quantity.
- Law: the resource union derives from the table (`keyof typeof _rows`) and closes the family — admission at the schema literal, dispatch at the rating fold, and the board panels all read the same anchor; a new metered resource is one row plus a rating rate row.
- Law: the meter journal Tag instantiates the audit-declared port law — `Audit.Journal<MeterFact>` under `"telemetry/MeterJournal"` with the shared `JournalFault` (`stream: "meter"`) — one port shape for the folder, two stream Tags, satisfied at the app root by `store` journal Layers.
- Receipt: the encoded twin derives (`typeof MeterFact.Encoded`) — the row shape the store journal Layer persists and the billing rollup reads.
- Growth: a new resource is one `_rows` row; a new attribution axis is one fact field consumed by the rollup key.

```typescript
import { Context, Schema } from "effect"
import { Audit } from "./audit.ts"

const _KINDS = ["compute", "request", "storage", "token"] as const

const _rows = {
  compute: { tenantTag: false, unit: "ms" },
  request: { tenantTag: true, unit: "count" },
  storage: { tenantTag: false, unit: "byte" },
  token: { tenantTag: true, unit: "count" },
} as const

declare namespace MeterFact {
  type Resource = keyof typeof _rows
  type Row = { readonly tenantTag: boolean; readonly unit: string }
  type Wire = typeof MeterFact.Encoded
  type _Rows<T extends Record<(typeof _KINDS)[number], Row> = typeof _rows> = T
  type _Keys<K extends (typeof _KINDS)[number] = Resource> = K
}

class MeterFact extends Schema.Class<MeterFact>("MeterFact")({
  app: Schema.NonEmptyString,
  at: Schema.DateTimeUtc,
  quantity: Schema.Int.pipe(Schema.nonNegative()),
  resource: Schema.Literal(..._KINDS),
  surface: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  tenant: Schema.NonEmptyString,
}) {
  get unit(): string {
    return _rows[this.resource].unit
  }
}

class _MeterJournal extends Context.Tag("telemetry/MeterJournal")<_MeterJournal, Audit.Journal<MeterFact>>() {}
```

## [3]-[RATING]

[RATING]:
- Owner: the rollup fold and the rating evaluation — `rollup` is one `HashMap.modifyAt` keyed fold over `Data.tuple(app, tenant, resource)` keys accumulating `{ count, total }`, and `rate` folds a rating policy over the rolled aggregates into exact per-key cost.
- Law: rates are caller-supplied policy — a `Rating` record keyed by the resource union, each row a `BigDecimal` unit price with its currency — because prices are app policy, never lib constants; the shape is closed by the derived union, so a missing rate row is a compile error at the policy literal.
- Law: cost arithmetic is `BigDecimal` end to end — quantity lifts through `BigDecimal.make(BigInt(n), 0)` (safe: quantities are schema-integral), multiplies against the rate row, sums across keys, and rounds `half-even` at scale 4 exactly once at the terminal — a float never touches money, and rounding never happens mid-fold.
- Law: aggregates merge by the component-wise additive fold — `count` and `total` both sum, stated once as the `_fused` combine — associative by construction so window rollups fuse across drains and a billing period is a fold over persisted window aggregates; the `@effect/typeclass` instance form is ledger-banned outside `scope:state`, so the combine is the sanctioned site-local spelling.
- Entry: `Meter.rollup(facts)`; `Meter.rate(rolled, rating)`.
- Growth: a new charge model (tiered, floor, minimum) is one field on the rating row read inside `rate` — never a second rating function.

```typescript
import { Array, BigDecimal, Data, HashMap, Option } from "effect"

declare namespace Meter {
  type Key = readonly [app: string, tenant: string, resource: MeterFact.Resource]
  type Aggregate = { readonly count: number; readonly total: number }
  type Rate = { readonly currency: string; readonly per: BigDecimal.BigDecimal }
  type Rating = { readonly [R in MeterFact.Resource]: Rate }
  type Cost = { readonly amount: BigDecimal.BigDecimal; readonly currency: string }
  type Charge = Omit<typeof MeterFact.Type, "app" | "at" | "tenant">
}

const _fused = (left: Meter.Aggregate, right: Meter.Aggregate): Meter.Aggregate => ({
  count: left.count + right.count,
  total: left.total + right.total,
})

const _rollup = (facts: ReadonlyArray<MeterFact>): HashMap.HashMap<Meter.Key, Meter.Aggregate> =>
  Array.reduce(facts, HashMap.empty<Meter.Key, Meter.Aggregate>(), (held, fact) =>
    HashMap.modifyAt(held, Data.tuple(fact.app, fact.tenant, fact.resource), (slot) =>
      Option.some(
        Option.match(slot, {
          onNone: (): Meter.Aggregate => ({ count: 1, total: fact.quantity }),
          onSome: (row) => _fused(row, { count: 1, total: fact.quantity }),
        }),
      )))

const _rate = (
  rolled: HashMap.HashMap<Meter.Key, Meter.Aggregate>,
  rating: Meter.Rating,
): HashMap.HashMap<Meter.Key, Meter.Cost> =>
  HashMap.map(rolled, (aggregate, key) => {
    const rate = rating[key[2]]
    return {
      amount: BigDecimal.round(
        BigDecimal.multiply(BigDecimal.make(BigInt(aggregate.total), 0), rate.per),
        { mode: "half-even", scale: 4 },
      ),
      currency: rate.currency,
    }
  })
```

## [4]-[RAIL]

[RAIL]:
- Owner: the `Meter` service — the same Layer-factory-over-identity shape as `Audit` (`Meter.Default(identity)`), one bounded intake, one scoped drain; `charge` is the one entrypoint and its input is modal — a single `Charge` or a proven-non-empty batch — discriminated on the value, never a `chargeMany` sibling.
- Law: the drain window batches through `Stream.groupedWithin`, appends the raw facts through the meter journal Tag under the shared retry gate, and emits the bounded OTLP aggregate in the same pass: the `Convention.metric.meterUsage` counter tagged by the resource row always, and by tenant only where the row's `tenantTag` posture admits it — the row-declared surface of the cardinality budget `otlp/export` enforces at the reader.
- Law: the journal is the billing truth and the metric is the observability rollup — a missing metric point is a dashboard gap, a missing journal row is a billing defect, and the retry/backpressure posture follows that asymmetry: journal appends retry and suspend, metric emission never blocks the drain.
- Entry: `Meter.charge(charge)` / `Meter.charge(charges)`; wiring is `Meter.Default(identity)` provided the store-owned journal Layer at the root.
- Growth: a new egress posture is one `tenantTag`-style row column; the rail is closed.

```typescript
import { Chunk, DateTime, Effect, Metric, Predicate, Queue, Schedule, Stream } from "effect"
import type { AppIdentity } from "@rasm/ts/kernel"
import { Convention } from "./convention.ts"

const _FLOW = { intake: 512, patience: "2 seconds", width: 128 } as const

const _usage = Metric.counter(Convention.metric.meterUsage, { incremental: true })
const _windowDrained = Metric.counter(Convention.metric.meterDrained, { incremental: true })

const _RETRY = Schedule.exponential("100 millis").pipe(
  Schedule.jittered,
  Schedule.intersect(Schedule.recurs(5)),
  Schedule.whileInput((fault: Audit.JournalFault) => fault.retryable),
)

const _emitted = (fact: MeterFact): Effect.Effect<void> => {
  const byResource = Metric.tagged(_usage, Convention.rasm.meterResource, fact.resource)
  return Metric.increment(
    _rows[fact.resource].tenantTag ? Metric.tagged(byResource, Convention.rasm.tenant, fact.tenant) : byResource,
  )
}

class Meter extends Effect.Service<Meter>()("telemetry/Meter", {
  scoped: (identity: AppIdentity) =>
    Effect.gen(function* () {
      const journal = yield* _MeterJournal
      const intake = yield* Queue.bounded<MeterFact>(_FLOW.intake)
      yield* Effect.forkScoped(
        Stream.fromQueue(intake).pipe(
          Stream.groupedWithin(_FLOW.width, _FLOW.patience),
          Stream.runForEach((batch) => {
            const rows = Chunk.toReadonlyArray(batch)
            return Array.isNonEmptyReadonlyArray(rows)
              ? journal.append(rows).pipe(
                  Effect.retry(_RETRY),
                  Effect.andThen(Metric.increment(_windowDrained)),
                  Effect.andThen(Effect.forEach(rows, _emitted, { discard: true })),
                  Effect.catchAll((fault) =>
                    Effect.logError("meter drain refused").pipe(Effect.annotateLogs({ count: fault.count }))),
                )
              : Effect.void
          }),
        ),
      )
      const stamped = (charge: Meter.Charge): Effect.Effect<void> =>
        Effect.flatMap(DateTime.now, (at) =>
          Queue.offer(intake, new MeterFact({ ...charge, app: identity.app, at, tenant: identity.tenant })).pipe(
            Effect.asVoid,
          ))
      return {
        charge: (input: Meter.Charge | Array.NonEmptyReadonlyArray<Meter.Charge>): Effect.Effect<void> =>
          Predicate.hasProperty(input, "resource")
            ? stamped(input)
            : Effect.forEach(input, stamped, { discard: true }),
      }
    }),
  accessors: true,
}) {
  static readonly Journal = _MeterJournal
  static readonly rate = _rate
  static readonly rollup = _rollup
  static readonly rows = _rows
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Meter, MeterFact }
```
