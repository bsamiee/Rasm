# [DATA_FACT]

Durable fact journal: audit evidence and usage metering as rows of ONE polymorphic `Fact` family draining through ONE buffered rail into ONE stream-discriminated table. `AuditFact` owns the actor/action/target vocabulary with typed diff evidence as a closed change family, `MeterFact` the `(app, tenant)`-keyed quantity against the closed resource vocabulary; both stamp identity and time on the rail and age under `journal/retain.md`'s retention classes — no second retention vocabulary exists.

Journal append is the system of record — a missing metric point is a dashboard gap, a missing journal row is an evidence or billing defect — so appends retry and suspend while the metric emission beside them never blocks the drain. Rating is exact `BigDecimal` arithmetic — a float never touches money — and the engine-level session audit is the spine's `audit` grant running beside this stream, never replacing it.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]     | [OWNS]                                                                         |
| :-----: | :------------ | :----------------------------------------------------------------------------- |
|  [01]   | `FACT_FAMILY` | the closed `Fact` union — audit row, meter row, change family, resource table  |
|  [02]   | `JOURNAL_ROW` | the stream-discriminated ensure, the batch append, retention grooming          |
|  [03]   | `RATING`      | rollup as a keyed fold and rating as exact `BigDecimal` policy evaluation      |
|  [04]   | `RAIL`        | the one buffered service — polymorphic record, stamped identity, bounded drain |

## [02]-[FACT_FAMILY]

- Owner: `AuditFact` and `MeterFact` — two `Schema.TaggedClass` rows of the closed `Fact` union; the `Change` diff family; the `_resources` table carrying each metered resource's unit and its metric-egress tenancy posture.
- Packages: `effect` (`Schema`, `Duration`); `@rasm/ts/core` (`AppIdentity`, `TenantContext`); `journal/retain.md` (`Retain.Class` — the one retention vocabulary).
- Growth: a new evidence kind is one `Change` case with its arm in consumers' folds; a new metered resource is one `_resources` row with its rating rate row; a new fact stream is one more tagged member of the union — the table, the rail, and the grooming inherit it.
- Law: diff evidence is the closed `Change` family — `Assigned { path, next }`, `Shifted { path, prior, next }`, `Cleared { path, prior }` — with `path` a JSON-pointer-shaped brand; a free-form details bag is the rejected shape because policy cannot fold what it cannot type.
- Law: the audit `action` brand is the dotted verb path — pattern-refined at the field so the vocabulary stays greppable and dashboard-groupable without a central verb registry; attribute NAMES live on the observability convention owner, the fact SHAPE lives here.
- Law: retention is `Retain.Class` — an audit fact references the class its policy demands (`regulatory` for compliance evidence, `operational` for routine trails), a meter fact is `regulatory` by constitution because it is billing truth; the retain grooming enforces the windows, this family only carries the key.
- Law: meter quantities are integral by schema — count, milliseconds, bytes, tokens — so the exact-arithmetic crossing in rating never meets a float; a fractional need is a smaller unit row, never a decimal quantity.
- Law: identity fields ride the core brand anchors — `app` from `AppIdentity.fields.app`, `tenant` as `Option`-carried `TenantContext.fields.tenant` — so tenancy never travels as a bare string and an unattributed fact records absence, never forged tenancy.
- Receipt: the encoded twins derive (`typeof AuditFact.Encoded`, `typeof MeterFact.Encoded`) — the row shapes the journal persists and downstream rollups read; no hand wire twin exists.

```typescript signature
import { Schema } from "effect"
import { AppIdentity, TenantContext } from "@rasm/ts/core"
import { Retain } from "./retain.ts"

const _ACTORS = ["user", "service", "system"] as const
const _RESOURCES = ["compute", "request", "storage", "token"] as const

const _resources = {
  compute: { tenantTag: false, unit: "ms" },
  request: { tenantTag: true, unit: "count" },
  storage: { tenantTag: false, unit: "byte" },
  token: { tenantTag: true, unit: "count" },
} as const

const _Path = Schema.String.pipe(Schema.pattern(/^(\/[^/~]*(~[01][^/~]*)*)*$/), Schema.brand("ChangePath"))
const _Action = Schema.String.pipe(Schema.pattern(/^[a-z][a-z0-9]*(\.[a-z][a-z0-9]*)+$/), Schema.brand("AuditAction"))

const Assigned = Schema.TaggedStruct("Assigned", { next: Schema.String, path: _Path })
const Cleared = Schema.TaggedStruct("Cleared", { path: _Path, prior: Schema.String })
const Shifted = Schema.TaggedStruct("Shifted", { next: Schema.String, path: _Path, prior: Schema.String })

const Change: Schema.Union<[typeof Assigned, typeof Cleared, typeof Shifted]> = Schema.Union(Assigned, Cleared, Shifted)
type Change = typeof Change.Type

class AuditFact extends Schema.TaggedClass<AuditFact>()("AuditFact", {
  action: _Action,
  actor: Schema.Struct({ key: Schema.NonEmptyString, kind: Schema.Literal(..._ACTORS) }),
  app: AppIdentity.fields.app,
  at: Schema.DateTimeUtc,
  change: Schema.Array(Change),
  retention: Retain.Class,
  target: Schema.Struct({
    key: Schema.NonEmptyString,
    kind: Schema.NonEmptyString,
    parent: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  }),
  tenant: Schema.optionalWith(TenantContext.fields.tenant, { as: "Option" }),
  trace: Schema.optionalWith(Schema.String, { as: "Option" }),
}) {}

class MeterFact extends Schema.TaggedClass<MeterFact>()("MeterFact", {
  app: AppIdentity.fields.app,
  at: Schema.DateTimeUtc,
  quantity: Schema.Int.pipe(Schema.nonNegative()),
  resource: Schema.Literal(..._RESOURCES),
  surface: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  tenant: Schema.optionalWith(TenantContext.fields.tenant, { as: "Option" }),
}) {
  get unit(): string {
    return _resources[this.resource].unit
  }
}

const _Fact = Schema.Union(AuditFact, MeterFact)
type _FactValue = typeof _Fact.Type

declare namespace Fact {
  type Resource = keyof typeof _resources
  type Value = _FactValue
  type _Rows<T extends Record<(typeof _RESOURCES)[number], { readonly tenantTag: boolean; readonly unit: string }> = typeof _resources> = T
}
```

## [03]-[JOURNAL_ROW]

- Owner: the `fact_journal` ensure — one stream-discriminated table for every fact row — and `_append`, the batch insert the rail drains through; grooming rides the retain windows by class column.
- Packages: `@effect/sql` (`SqlClient`, `sql.insert`); `effect` (`Schema`, `Array`).
- Entry: only the rail writes; reads are projection material — a billing period reads the meter stream through `[4]`'s bound window read, an audit search reads a projection built from this table, and neither touches the intake.
- Growth: a new fact stream needs zero DDL — the `stream` column carries the union tag and the payload column carries the encoded member.
- Law: the table is append-only evidence with a retention column — grooming deletes rows past `Retain.Policy[class].window` as scheduled maintenance, and `permanent` rows never groom; erasure of subject-bearing audit evidence rides the same crypto-shredding spine as the event journal.
- Law: the payload persists the encoded twin through the union codec — one decode proves any historical row back into its live member, and stream dispatch is the `_tag` the codec already carries.
- Law: the tenant column mirrors the fact's tenancy for RLS predication — `Tenancy.rls("fact_journal")` applies where the scope demands it, and an absent tenant stores NULL, visible to operators only.

```typescript signature
import { Array, Effect, Option } from "effect"
import { SqlClient, type SqlError } from "@effect/sql"
import type { Capability } from "../lane/capability.ts"
import { Journal } from "./append.ts"

const _factDdl: Capability.Ensure = {
  relation: "fact_journal",
  pg: `CREATE TABLE IF NOT EXISTS fact_journal (
    sequence BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    stream TEXT NOT NULL,
    app TEXT NOT NULL, tenant TEXT,
    retention TEXT NOT NULL,
    payload JSONB NOT NULL,
    recorded_at TIMESTAMPTZ NOT NULL DEFAULT now());
  CREATE INDEX IF NOT EXISTS fact_journal_stream ON fact_journal (stream, app, recorded_at);`,
  sqlite: `CREATE TABLE IF NOT EXISTS fact_journal (
    sequence INTEGER PRIMARY KEY AUTOINCREMENT,
    stream TEXT NOT NULL,
    app TEXT NOT NULL, tenant TEXT,
    retention TEXT NOT NULL,
    payload TEXT NOT NULL,
    recorded_at TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now')));`,
}

const _retentionOf = (fact: Fact.Value): Retain.Class =>
  fact._tag === "AuditFact" ? fact.retention : "regulatory"

const _encode = Schema.encode(Schema.parseJson(_Fact))

const _append = (facts: Array.NonEmptyReadonlyArray<Fact.Value>) =>
  Effect.gen(function* () {
    const sql = yield* SqlClient.SqlClient
    const rows = yield* Effect.forEach(facts, (fact) =>
      Effect.map(_encode(fact), (payload) => ({
        stream: fact._tag,
        app: fact.app,
        tenant: Option.getOrNull(fact.tenant),
        retention: _retentionOf(fact),
        payload,
      })), { concurrency: "unbounded" })
    yield* sql`INSERT INTO fact_journal ${sql.insert(rows)}`
  })
```

## [04]-[RATING]

- Owner: the rollup fold, the bound window read, and the rating evaluation — `rollup` is ONE entry whose modality is the input shape: a row array folds pure through `HashMap.modifyAt` over `(app, tenant, resource)` tuples accumulating `{ count, total }`, a `Fact.Window` value reads the meter stream through `_meters` — the one decoded windowed SELECT this page owns, so no billing consumer hand-mints SQL against `fact_journal` — then runs the same fold; `rate` folds a caller-supplied rating policy over the rolled aggregates into exact per-key cost.
- Packages: `effect` (`Array`, `BigDecimal`, `Data`, `HashMap`, `Option`); `@effect/sql` (`SqlSchema` — the window read); `journal/evolve.md` (`Upcast.json` — the dialect-honest payload decode).
- Entry: `Fact.rollup(facts)` over rows in hand, `Fact.rollup(window)` over a billing period read from the journal; `Fact.rate(rolled, rating)` at settlement; the at-scale replication of these windows into the OLAP lane is `lane/olap.md`'s ingestion row.
- Growth: a new charge model (tiered, floor, minimum) is one field on the rating row read inside `rate` — never a second rating function.
- Law: rates are caller-supplied policy — a `Rating` record keyed by the resource union, each row a `BigDecimal` unit price with its currency — because prices are app policy, never lib constants; the shape closes over the derived union, so a missing rate row is a compile error at the policy literal.
- Law: cost arithmetic is `BigDecimal` end to end — quantity lifts through `BigDecimal.make(BigInt(n), 0)` (safe: quantities are schema-integral), multiplies against the rate row, and rounds `half-even` at scale 4 exactly once at the terminal — a float never touches money and rounding never happens mid-fold.
- Law: aggregates merge by the component-wise additive fold — associative by construction, so window rollups fuse across drains and a billing period is a fold over persisted window aggregates.

```typescript signature
import { Array, BigDecimal, Data, DateTime, HashMap, Option, type ParseResult } from "effect"
import { SqlSchema } from "@effect/sql"
import { Upcast } from "./evolve.ts"

declare namespace Fact {
  type Key = readonly [app: AppIdentity.Key, tenant: Option.Option<TenantContext.Key>, resource: Resource]
  type Aggregate = { readonly count: number; readonly total: number }
  type Rate = { readonly currency: string; readonly per: BigDecimal.BigDecimal }
  type Rating = { readonly [R in Resource]: Rate }
  type Cost = { readonly amount: BigDecimal.BigDecimal; readonly currency: string }
  type Window = { readonly app: AppIdentity.Key; readonly from: DateTime.Utc; readonly to: DateTime.Utc }
}

const _fused = (left: Fact.Aggregate, right: Fact.Aggregate): Fact.Aggregate => ({
  count: left.count + right.count,
  total: left.total + right.total,
})

const _meters = (window: Fact.Window) =>
  Effect.flatMap(SqlClient.SqlClient, (sql) =>
    SqlSchema.findAll({
      Request: Schema.Struct({ app: AppIdentity.fields.app, from: Schema.DateTimeUtc, to: Schema.DateTimeUtc }),
      Result: Schema.Struct({ payload: Upcast.json(MeterFact) }),
      execute: (bounds) =>
        sql`SELECT payload FROM fact_journal
            WHERE stream = 'MeterFact' AND app = ${bounds.app}
              AND recorded_at >= ${bounds.from} AND recorded_at < ${bounds.to}`,
    })(window).pipe(Effect.map(Array.map((row) => row.payload))))

const _folded = (facts: ReadonlyArray<MeterFact>): HashMap.HashMap<Fact.Key, Fact.Aggregate> =>
  Array.reduce(facts, HashMap.empty<Fact.Key, Fact.Aggregate>(), (held, fact) =>
    HashMap.modifyAt(held, Data.tuple(fact.app, fact.tenant, fact.resource), (slot) =>
      Option.some(
        Option.match(slot, {
          onNone: (): Fact.Aggregate => ({ count: 1, total: fact.quantity }),
          onSome: (row) => _fused(row, { count: 1, total: fact.quantity }),
        }),
      )))

function _rollup(facts: ReadonlyArray<MeterFact>): HashMap.HashMap<Fact.Key, Fact.Aggregate>
function _rollup(window: Fact.Window): Effect.Effect<
  HashMap.HashMap<Fact.Key, Fact.Aggregate>,
  SqlError.SqlError | ParseResult.ParseError,
  SqlClient.SqlClient
>
function _rollup(input: ReadonlyArray<MeterFact> | Fact.Window) {
  return Array.isArray(input) ? _folded(input) : Effect.map(_meters(input), _folded)
}

const _rate = (
  rolled: HashMap.HashMap<Fact.Key, Fact.Aggregate>,
  rating: Fact.Rating,
): HashMap.HashMap<Fact.Key, Fact.Cost> =>
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

## [05]-[RAIL]

- Owner: the `Fact` service — a Layer factory over the app's `AppIdentity` (`Fact.Default(identity)`), holding one bounded intake and one scoped drain fiber; `record` is the ONE entrypoint, modal over its input — an audit draft, a meter charge, or a proven-non-empty batch of either — discriminated on the value, never a sibling per stream.
- Packages: `effect` (`Effect.Service`, `Queue`, `Stream`, `Chunk`, `Schedule`, `Metric`, `DateTime`, `Option`, `Array`, `Schema`); `@rasm/ts/core` (`Convention` — the metric and attribute name rows).
- Entry: `Fact.record(draft)` for evidence, `Fact.record(charge)` for usage, `Fact.record(batch)` for either in bulk; wiring is `Fact.Default(identity)` provided a scope's `SqlClient` at the root.
- Growth: a new stamped dimension is one line in the stamp; a new drain posture is one `_FLOW` field; a new fact stream extends the union and the discriminant fold, nothing else.
- Law: the service stamps what the caller must not control — `app` from the identity, `at` from the clock on the rail, tenancy resolved pinned-first so a single-tenant process overrides the draft's key; construction runs the schema filters, so a malformed draft fails the writer, never the drain.
- Law: drafts are derived schema projections, never hand-declared patches — `Fact.AuditDraft`/`Fact.Charge` re-anchor on the owning field records through `omit("_tag", "app", "at")`, then `Schema.attachPropertySignature` restores the decoded family tag without requiring it on encoded ingress; the family's `Option`-carried tenancy survives untouched, the union stays discriminated even when future members acquire overlapping fields, and an edge-arriving draft decodes through the owning projection before it reaches `record`.
- Law: the drain never load-sheds — evidence and billing truth suspend under backpressure and retry unbounded; egress quota belongs to the correctness-adjacent replication seams (`lane/olap.md`'s `ingest`), never to this rail.
- Law: the drain is one pipeline — `Stream.fromQueue` over the intake, `Stream.groupedWithin(width, patience)` so a quiet surface still flushes on latency, the batch appended under an unbounded jittered retry whose delay caps through `Schedule.union` (evidence is never dropped: a dead database suspends the drain, the bounded intake suspends writers, and pressure propagates instead of silently losing billing truth), each deferral logged, the drained count and per-resource usage emitted through the convention instrument rows in the same pass; counter updates read batch facts — the drained row count and each fact's resource and tenant tags — so they ride the drain fold, never a per-effect metric decorator.
- Law: every audit fact also emits one structured log annotated with the convention's audit rows — observability beside durability; the meter's metric egress is deliberately lossy and bounded — resource tag always, tenant tag only where the resource row's posture admits it — and the journal remains the sole truth for both streams.

```typescript signature
import { Array, Chunk, DateTime, Effect, Metric, Option, Queue, Schedule, Schema, Stream } from "effect"
import { Convention } from "@rasm/ts/core"

const _FLOW = { intake: 512, patience: "2 seconds", width: 128 } as const

const _drained = Metric.counter(Convention.instrument.factDrained.name, {
  description: Convention.instrument.factDrained.description,
  incremental: true,
})
const _usage = Metric.counter(Convention.instrument.meterUsage.name, {
  description: Convention.instrument.meterUsage.description,
  incremental: true,
})

const _RETRY = Schedule.exponential("100 millis").pipe(
  Schedule.jittered,
  Schedule.union(Schedule.spaced("10 seconds")),
)

const _metered = (fact: MeterFact): Effect.Effect<void> => {
  const byResource = Metric.tagged(_usage, Convention.rasm.meterResource, fact.resource)
  return Metric.incrementBy(
    _resources[fact.resource].tenantTag
      ? Option.match(fact.tenant, {
          onNone: () => byResource,
          onSome: (tenant) => Metric.tagged(byResource, Convention.rasm.tenant, tenant),
        })
      : byResource,
    fact.quantity,
  )
}

const _emitted = (fact: Fact.Value): Effect.Effect<void> =>
  fact._tag === "AuditFact"
    ? Effect.logInfo("audit").pipe(
        Effect.annotateLogs({
          [Convention.rasm.auditAction]: fact.action,
          [Convention.rasm.auditActorKey]: fact.actor.key,
          [Convention.rasm.auditActorKind]: fact.actor.kind,
          [Convention.rasm.auditRetention]: fact.retention,
          [Convention.rasm.auditTargetKey]: fact.target.key,
          [Convention.rasm.auditTargetKind]: fact.target.kind,
        }),
      )
    : _metered(fact)

const _AuditDraft = Schema.Struct(AuditFact.fields).omit("_tag", "app", "at").pipe(
  Schema.attachPropertySignature("_tag", "AuditFact"),
)

const _Charge = Schema.Struct(MeterFact.fields).omit("_tag", "app", "at").pipe(
  Schema.attachPropertySignature("_tag", "MeterFact"),
)

class Fact extends Effect.Service<Fact>()("data/Fact", {
  scoped: (identity: AppIdentity) =>
    Effect.gen(function* () {
      const intake = yield* Queue.bounded<Fact.Value>(_FLOW.intake)
      yield* Effect.forkScoped(
        Stream.fromQueue(intake).pipe(
          Stream.groupedWithin(_FLOW.width, _FLOW.patience),
          Stream.runForEach((batch) => {
            const rows = Chunk.toReadonlyArray(batch)
            return Array.isNonEmptyReadonlyArray(rows)
              ? _append(rows).pipe(
                  Effect.tapError((fault) =>
                    Effect.logError("fact drain deferred").pipe(Effect.annotateLogs({ count: rows.length, fault: fault._tag }))),
                  Effect.retry(_RETRY),
                  Effect.andThen(Metric.incrementBy(_drained, rows.length)),
                  Effect.andThen(Effect.forEach(rows, _emitted, { concurrency: "inherit", discard: true })),
                )
              : Effect.void
          }),
        ),
      )
      const stamped = (draft: Fact.Draft): Effect.Effect<void> =>
        Effect.gen(function* () {
          const at = yield* DateTime.now
          const tenant = Option.orElse(identity.tenant, () => draft.tenant)
          const fact: Fact.Value = draft._tag === "MeterFact"
            ? new MeterFact({
                app: identity.app,
                at,
                quantity: draft.quantity,
                resource: draft.resource,
                surface: draft.surface,
                tenant,
              })
            : new AuditFact({
                action: draft.action,
                actor: draft.actor,
                app: identity.app,
                at,
                change: draft.change,
                retention: draft.retention,
                target: draft.target,
                tenant,
                trace: draft.trace,
              })
          yield* Queue.offer(intake, fact)
        })
      return {
        record: (input: Fact.Draft | Array.NonEmptyReadonlyArray<Fact.Draft>): Effect.Effect<void> =>
          Effect.forEach(Array.ensure(input), stamped, { concurrency: 1, discard: true }),
      }
    }),
  accessors: true,
}) {
  static readonly rollup = _rollup
  static readonly rate = (rolled: HashMap.HashMap<Fact.Key, Fact.Aggregate>, rating: Fact.Rating): HashMap.HashMap<Fact.Key, Fact.Cost> => _rate(rolled, rating)
  static readonly AuditDraft = _AuditDraft
  static readonly Charge = _Charge
  static readonly resources = _resources
  static readonly ddl = [_factDdl]
}

declare namespace Fact {
  type AuditDraft = typeof _AuditDraft.Type
  type Charge = typeof _Charge.Type
  type Draft = AuditDraft | Charge
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { AuditFact, Change, Fact, MeterFact }
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
