# [DATA_FACT]

Durable fact journal: audit evidence and usage metering as rows of ONE polymorphic `Fact` family draining through ONE buffered rail into ONE stream-discriminated table. `AuditFact` owns the actor/action/target vocabulary with typed diff evidence as a closed change family, `MeterFact` the `(app, tenant)`-keyed quantity against the closed resource vocabulary; both take their identity and their causal stamp on the rail and age under `journal/retain.md`'s retention classes — no second retention vocabulary exists.

Journal append is the system of record — a missing metric point is a dashboard gap, a missing journal row is an evidence or billing defect — so appends retry unbounded, content identity makes that retry safe, and the rail-stamped `Hlc` orders the stream over any database clock or sequence. Nothing sheds: back-pressure suspends the writer, shutdown flushes the offered window, and what a dead database refuses stays on the unlanded roster. Rating is exact end to end, an integral `bigint` total lifting into `BigDecimal`, and the engine-level session audit runs beside this stream.

## [01]-[INDEX]

- [02]-[FACT_FAMILY]: closed `Fact` union — audit row, meter row, change family, resource table.
- [03]-[JOURNAL_ROW]: stream-discriminated ensure, idempotent batch append, retention grooming.
- [04]-[RATING]: rollup as a keyed fold and rating as exact `BigDecimal` policy evaluation.
- [05]-[RAIL]: one buffered service — polymorphic record, stamped identity, bounded drain with its lossless close and unlanded roster, audit-port projection.

## [02]-[FACT_FAMILY]

- Owner: `AuditFact` and `MeterFact` — two `Schema.TaggedClass` rows of the closed `Fact` union; the `Change` diff family; the subject-and-sealed erasure pair every subject-bearing fact carries; the `_resources` table carrying each metered resource's unit and its metric-egress tenancy posture.
- Packages: `effect` (`Schema`, `Duration`); `@rasm/ts/core` (`AppIdentity`, `Hlc`, `TenantContext`); `journal/retain.md` (`Retain.Class` — the one retention vocabulary).
- Growth: a new evidence kind is one `Change` case with its arm in consumers' folds; a new metered resource is one `_resources` row with its rating rate row; a new fact stream is one more tagged member of the union — the table, the rail, and the grooming inherit it.
- Law: diff evidence is the closed `Change` family — `Assigned { path, next }`, `Shifted { path, prior, next }`, `Cleared { path, prior }` — with `path` a JSON-pointer-shaped brand; a free-form details bag is the rejected shape because policy cannot fold what it cannot type.
- Law: the audit `action` brand is the dotted verb path — pattern-refined at the field so the vocabulary stays greppable and dashboard-groupable without a central verb registry; attribute NAMES live on the observability convention owner, the fact SHAPE lives here, and the drain's own fan is what makes the verb groupable on a series rather than on a log search alone.
- Law: the metered `_RESOURCES` roster is a CROSS-BRANCH vocabulary the python journal owner (`libs/python/runtime/.planning/observability/journal.md` `Resource`) transcribes row for row, so a fifth resource lands as one row in BOTH spellings beside its rate row — a runtime-local addition forks the vocabulary the peer prices against and strands a billing window that folds one branch's rows against the other's rates.
- Law: metric-egress tenancy diverges from that peer BY DESIGN and the divergence is stated at both ends — this branch prices the tenant tag per resource on the `tenantTag` column, since the roster is small and each resource's tenant cardinality is its own fact, while the python owner's drain projects OUTSIDE any producer's context and its attribute fold reads tenancy from baggage, so that branch's journal series carry no tenant dimension at all and resolve tenancy on the row alone; the journal row carries tenancy identically on both, so only the lossy metric projection differs.
- Law: retention is `Retain.Class` — an audit fact references the class its policy demands (`regulatory` for compliance evidence, `operational` for routine trails), a meter fact is `regulatory` by constitution because it is billing truth; the retain grooming enforces the windows, this family only carries the key.
- Law: erasure rides the subject pair, never the diff family — an identifying value seals under the subject's data key and lands in `sealed` while `subject` carries the custody coordinate, so destroying that key redacts the identifier and leaves the verb, the actor class, the target, and the instant queryable; a subject spelled into a `Change` value survives erasure in plaintext, which is the defect this pair forecloses.
- Law: meter quantities are integral AND safe by schema — count, milliseconds, bytes, tokens, each bounded at `Number.MAX_SAFE_INTEGER` — so the `BigInt` lift in rating is provably exact per row; a fractional need is a smaller unit row, never a decimal quantity, and a magnitude past the safe ceiling refuses at the writer rather than arriving as a rounded double.
- Law: `stamp` is the one ORDERING coordinate the family carries and the rail is its sole mint — an `Hlc` whose physical half is epoch millis and whose logical half ticks per fact, so two facts minted inside one millisecond stay distinct, the stream sorts on a coordinate no database clock supplies, and the wall instant recovers exactly through `Hlc.physicalOf`. Caller-threaded stamps let two producers mint one content key and dedup genuine evidence away as a redelivery, so the field carries no draft column; `occurred` is the separable half a producer that lane-queued its record before offering it carries, and it never orders, groups, or settles anything.
- Law: identity fields ride the core brand anchors — `app` from `AppIdentity.fields.app`, `tenant` as `Option`-carried `TenantContext.fields.tenant` — so tenancy never travels as a bare string and an unattributed fact records absence, never forged tenancy.
- Receipt: the encoded twins derive (`typeof AuditFact.Encoded`, `typeof MeterFact.Encoded`) — the row shapes the journal persists and downstream rollups read; no hand wire twin exists.

```typescript signature
import { Schema } from "effect"
import { AppIdentity, Hlc, TenantContext } from "@rasm/ts/core"
import { SealedEnvelope } from "@rasm/ts/security"
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

// Quantities lift to `BigInt` exactly once per row in rating, so the ceiling is a schema fact rather than a comment:
// past it a JSON number has already lost precision at parse and the writer is the last seam that can still refuse.
const _Quantity = Schema.Int.pipe(Schema.between(0, Number.MAX_SAFE_INTEGER))

class AuditFact extends Schema.TaggedClass<AuditFact>()("AuditFact", {
  action: _Action,
  actor: Schema.Struct({ key: Schema.NonEmptyString, kind: Schema.Literal(..._ACTORS) }),
  app: AppIdentity.fields.app,
  change: Schema.Array(Change),
  // Upstream occurrence instant, where a producer sealed one: the rail's stamp is an ADMISSION coordinate, so a
  // record that queued behind a saturated lane before reaching it dates to the drain and not to its own event.
  // Absence means the rail's stamp IS the occurrence, so no consumer folds a forged instant.
  occurred: Schema.optionalWith(Schema.DateTimeUtc, { as: "Option" }),
  retention: Retain.Class,
  // Subject-bearing evidence rides the shred spine, never the diff family: `subject` is the erasure coordinate the
  // custody ledger keys on and `sealed` the ciphertext its data key opens, so destroying that key redacts the
  // identifier while the append-only row and its verb, actor class, and target stay queryable forever.
  sealed: Schema.optionalWith(SealedEnvelope, { as: "Option" }),
  stamp: Hlc,
  subject: Schema.optionalWith(Retain.Subject, { as: "Option" }),
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
  quantity: _Quantity,
  resource: Schema.Literal(..._RESOURCES),
  stamp: Hlc,
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

- Owner: the `fact_journal` ensure — one stream-discriminated table for every fact row — `_rowed`, the encode-and-key projection, and `_append`, the idempotent batch insert the rail drains through, returning the `Landing` that accounts for every offered row; grooming rides the retain windows by class column, and the partial subject index is what makes a DSAR scan and an erasure sweep read the stream by custody coordinate.
- Packages: `@effect/sql` (`SqlClient`, `sql.insert`, `RecordInsertHelper.returning`); `@rasm/ts/core` (`Digest` — the one content-identity mint); `../lane/tenant.ts` (`Tenancy.rls`); `effect` (`Schema`, `Array`, `HashSet`).
- Entry: only the rail writes; reads are projection material — a billing period reads the meter stream through `[4]`'s bound window read, an audit search reads a projection built from this table, and neither touches the intake.
- Growth: a new fact stream needs zero DDL — the `stream` column carries the union tag and the payload column carries the encoded member.
- Law: the table is append-only evidence with a retention column — grooming deletes rows past `Retain.Policy[class].window` as scheduled maintenance, and `permanent` rows never groom; erasure of subject-bearing audit evidence rides the same crypto-shredding spine as the event journal.
- Law: the payload persists the encoded twin through the union codec — one decode proves any historical row back into its live member, and stream dispatch is the `_tag` the codec already carries.
- Law: `key` is `Digest.mint("content", …)` over the row's own encoded payload under a `(app, key)` uniqueness constraint, so an at-least-once landing dedups structurally and the drain's unbounded retry is safe by construction rather than by hope — the stamp rides INSIDE that payload, so two genuinely distinct facts never collide on one key. Retrying an insert with no dedup precondition double-bills the moment a driver loses an acknowledgement it already committed.
- Law: `ON CONFLICT (app, key) DO NOTHING … RETURNING key` makes the landing self-accounting — the statement is atomic and admits exactly two outcomes per offered row, so the returned keys partition the offered pairs into `accepted` and `duplicate` and no third state remains for a short write to hide in. Both halves carry their facts rather than tallies, because the drain projects the accepted half and tags the duplicate half by stream; one merged number claims zero redelivery, and zero redelivery is what a wedged retry re-offering one window forever looks like from the drain's own series.
- Law: `stamp_physical` and `stamp_logical` lift the causal coordinate out of the opaque payload because every settlement, ordering, and grooming predicate reads it — `recorded_at` stays the durability stamp alone and no query predicates on it, so a row landing hours late under a dead database still settles in the period its fact occurred in.
- Law: the tenant column mirrors the fact's tenancy for RLS predication — the ensure registers `Tenancy.rls("fact_journal")` like every tenant-carrying relation, so the registration is structural rather than a scope-dependent option, and an absent tenant stores NULL, visible to operators only.
- Law: rowing and appending are SEPARATE rails because their faults have opposite lifetimes — `_rowed` fails only on encode, which a constructed member cannot reach, while `_append` fails only on `SqlError`, which is transient by definition. Splitting them is what lets the drain retry the append without bound and still stay total: one shared rail seats a permanent fault under an unbounded schedule and wedges every later fact behind one row no database can ever accept.

```typescript signature
import { Array, Effect, HashSet, Option, type ParseResult } from "effect"
import { SqlClient, type SqlError } from "@effect/sql"
import { type ContentKey, Digest } from "@rasm/ts/core"
import type { Capability } from "../lane/capability.ts"
import { Tenancy } from "../lane/tenant.ts"

declare namespace Fact {
  // Durable projection: every coordinate a reader PREDICATES on is lifted out of the opaque payload — the content key
  // for dedup, the causal halves for ordering and settlement, the class for grooming, the subject for DSAR — so a
  // billing window is an index read rather than a per-row decode wearing a pushdown's name.
  type Row = {
    readonly app: AppIdentity.Key
    readonly key: ContentKey
    readonly payload: string
    readonly retention: Retain.Class
    readonly stamp_logical: Hlc.Logical
    readonly stamp_physical: Hlc.Physical
    readonly stream: _FactValue["_tag"]
    readonly subject: string | null
    readonly tenant: string | null
  }
  // `Landing` partitions every offered row: `accepted` names the rows the plane did not already hold and
  // `duplicate` names the redeliveries the content key matched. Both halves carry facts rather than tallies, so
  // accepted evidence projects its own fan and the redelivery series resolves its declared stream tag — a bare
  // count satisfies neither, and one merged tally claims zero redelivery.
  type Pair = { readonly fact: _FactValue; readonly row: Row }
  type Landing = { readonly accepted: ReadonlyArray<Pair>; readonly duplicate: ReadonlyArray<Pair> }
}

const _factDdl: Capability.Ensure = {
  relation: "fact_journal",
  pg: `CREATE TABLE IF NOT EXISTS fact_journal (
    sequence BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    key TEXT NOT NULL,
    stream TEXT NOT NULL,
    app TEXT NOT NULL, tenant TEXT,
    retention TEXT NOT NULL,
    subject TEXT,
    stamp_physical BIGINT NOT NULL, stamp_logical BIGINT NOT NULL,
    payload JSONB NOT NULL,
    recorded_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT fact_journal_content UNIQUE (app, key));
  CREATE INDEX IF NOT EXISTS fact_journal_stream ON fact_journal (stream, app, stamp_physical, stamp_logical);
  CREATE INDEX IF NOT EXISTS fact_journal_subject ON fact_journal (app, tenant, subject) WHERE subject IS NOT NULL;
  ${Tenancy.rls("fact_journal")}`,
  sqlite: `CREATE TABLE IF NOT EXISTS fact_journal (
    sequence INTEGER PRIMARY KEY AUTOINCREMENT,
    key TEXT NOT NULL,
    stream TEXT NOT NULL,
    app TEXT NOT NULL, tenant TEXT,
    retention TEXT NOT NULL,
    subject TEXT,
    stamp_physical INTEGER NOT NULL, stamp_logical INTEGER NOT NULL,
    payload TEXT NOT NULL,
    recorded_at TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now')),
    CONSTRAINT fact_journal_content UNIQUE (app, key));
  CREATE INDEX IF NOT EXISTS fact_journal_stream ON fact_journal (stream, app, stamp_physical, stamp_logical);
  CREATE INDEX IF NOT EXISTS fact_journal_subject ON fact_journal (app, tenant, subject) WHERE subject IS NOT NULL;`,
}

// Two columns the stream discriminant decides together: a meter fact is billing truth at `regulatory` and bears no
// subject, an audit fact carries its own class and its erasure coordinate — one fold, so neither column drifts alone.
const _keyed = (fact: Fact.Value): { readonly retention: Retain.Class; readonly subject: string | null } =>
  fact._tag === "AuditFact"
    ? { retention: fact.retention, subject: Option.getOrNull(fact.subject) }
    : { retention: "regulatory", subject: null }

const _encode = Schema.encode(Schema.parseJson(_Fact))
const _utf8 = new TextEncoder()

// Row and originating fact travel together, because the drain projects metrics and log lines for the rows the plane
// ACCEPTED and never for the redeliveries it matched — pairing here is what lets the landing filter downstream.
const _rowed = (fact: Fact.Value): Effect.Effect<Fact.Pair, ParseResult.ParseError> =>
  Effect.flatMap(_encode(fact), (payload) =>
    Effect.map(Digest.mint("content", _utf8.encode(payload)), (key) => ({
      fact,
      row: {
        key,
        stream: fact._tag,
        app: fact.app,
        tenant: Option.getOrNull(fact.tenant),
        ..._keyed(fact),
        stamp_physical: fact.stamp.physical,
        stamp_logical: fact.stamp.logical,
        payload,
      },
    })))

// `DO NOTHING` with `RETURNING` is the whole landing account: the statement is atomic, so an offered row either
// inserts and comes back or conflicts and does not — there is no third outcome for a short write to hide in, and
// `duplicate` is therefore exact rather than inferred. Both dialects carry this form (sqlite since 3.35).
// `_append` carries `SqlError` ALONE, rowing having already run, so its one reachable fault is the transient one
// an unbounded schedule may sit on.
const _append = (
  pairs: Array.NonEmptyReadonlyArray<Fact.Pair>,
): Effect.Effect<Fact.Landing, SqlError.SqlError, SqlClient.SqlClient> =>
  Effect.gen(function* () {
    const sql = yield* SqlClient.SqlClient
    const landed = yield* sql<{ readonly key: string }>`
      INSERT INTO fact_journal ${sql.insert(Array.map(pairs, (pair) => pair.row))}
      ON CONFLICT (app, key) DO NOTHING
      RETURNING key`
    const landedKeys = HashSet.fromIterable(Array.map(landed, (row) => row.key))
    const [accepted, duplicate] = Array.partition(pairs, (pair) => !HashSet.has(landedKeys, pair.row.key))
    return { accepted, duplicate }
  })
```

## [04]-[RATING]

- Owner: the rollup fold, the bound window read, and the rating evaluation — `rollup` is ONE entry whose modality is the input shape: a row array folds pure through `HashMap.modifyAt` over `(app, tenant, resource)` tuples accumulating `{ count, total }`, a `Fact.Window` value reads the meter stream through `_meters` — the one decoded windowed SELECT this page owns, so no billing consumer hand-mints SQL against `fact_journal` — then runs the same fold; `rate` folds a caller-supplied rating policy over the rolled aggregates into exact per-key cost.
- Packages: `effect` (`Array`, `BigDecimal`, `Data`, `HashMap`, `Option`); `@rasm/ts/core` (`Hlc` — the settlement coordinate); `@effect/sql` (`SqlSchema` — the window read); `journal/evolve.md` (`Upcast.json` — the dialect-honest payload decode).
- Entry: `Fact.rollup(facts)` over rows in hand, `Fact.rollup(window)` over a billing period read from the journal; `Fact.rate(rolled, rating)` at settlement; the at-scale replication of these windows into the OLAP lane is `lane/olap.md`'s ingestion row.
- Growth: a new charge model (tiered, floor, minimum) is one field on the rating row read inside `rate` — never a second rating function.
- Law: rates are caller-supplied policy — a `Rating` record keyed by the resource union, each row a `BigDecimal` unit price with its currency — because prices are app policy, never lib constants; the shape closes over the derived union, so a missing rate row is a compile error at the policy literal.
- Law: the accumulator is `bigint`, not a JS number — a `number` total silently rounds past 2^53, and `BigInt` accepts that rounded double without complaint, so the exact-arithmetic law dies in the fold rather than at the multiply where the page states it. Each quantity lifts exactly under the schema's safe-integer ceiling, the sum stays exact at any magnitude, and `BigDecimal.make(total, 0)` is a widening rather than a conversion.
- Law: cost arithmetic is exact end to end — the integral total lifts through `BigDecimal.make(bigint, 0)`, multiplies against the rate row, and rounds `half-even` at scale 4 exactly once at the terminal — a float never touches money and rounding never happens mid-fold, which is the sibling `python:runtime/observability/journal#RATING` law spelled in this branch's types.
- Law: `_meters` predicates on `stamp_physical`, the coordinate the RAIL minted, never on `recorded_at` — a database write clock is a durability fact, and settling on it files every retry-delayed row into whichever period its landing fell in, a misattribution no later query recovers because the true instant survives only inside the payload. `Hlc.physicalOf` is the one crossing from the wall-clock instants a settlement names into that coordinate, so window bound and row stamp share one time base by construction.
- Law: aggregates merge by the component-wise additive fold — associative by construction, so window rollups fuse across drains and a billing period is a fold over persisted window aggregates.

```typescript signature
import { Array, BigDecimal, Data, DateTime, HashMap, Option, type ParseResult } from "effect"
import { SqlSchema } from "@effect/sql"
import { Hlc } from "@rasm/ts/core"
import { Upcast } from "./evolve.ts"

declare namespace Fact {
  type Key = readonly [app: AppIdentity.Key, tenant: Option.Option<TenantContext.Key>, resource: Resource]
  // `total` is `bigint` because it is money's preimage: a `number` sum rounds past 2^53 and `BigInt` widens that
  // rounded double without complaint, so the exactness the rating law claims has to hold in the ACCUMULATOR.
  type Aggregate = { readonly count: number; readonly total: bigint }
  type Rate = { readonly currency: string; readonly per: BigDecimal.BigDecimal }
  type Rating = { readonly [R in Resource]: Rate }
  type Cost = { readonly amount: BigDecimal.BigDecimal; readonly currency: string }
  type Window = { readonly app: AppIdentity.Key; readonly from: DateTime.Utc; readonly to: DateTime.Utc }
}

const _fused = (left: Fact.Aggregate, right: Fact.Aggregate): Fact.Aggregate => ({
  count: left.count + right.count,
  total: left.total + right.total,
})

// Settlement bounds cross from the wall-clock instants a billing period names into the stamp coordinate the rail
// minted, so cutoff and row share one time base; predicating on `recorded_at` instead settles a retry-delayed row
// into the period its landing fell in, stranding the true instant inside the opaque payload.
const _meters = (window: Fact.Window) =>
  Effect.flatMap(SqlClient.SqlClient, (sql) =>
    SqlSchema.findAll({
      Request: Schema.Struct({ app: AppIdentity.fields.app, from: Schema.DateTimeUtc, to: Schema.DateTimeUtc }),
      Result: Schema.Struct({ payload: Upcast.json(MeterFact) }),
      execute: (bounds) =>
        sql`SELECT payload FROM fact_journal
            WHERE stream = 'MeterFact' AND app = ${bounds.app}
              AND stamp_physical >= ${Hlc.physicalOf(bounds.from)} AND stamp_physical < ${Hlc.physicalOf(bounds.to)}`,
    })(window).pipe(Effect.map(Array.map((row) => row.payload))))

const _folded = (facts: ReadonlyArray<MeterFact>): HashMap.HashMap<Fact.Key, Fact.Aggregate> =>
  Array.reduce(facts, HashMap.empty<Fact.Key, Fact.Aggregate>(), (held, fact) =>
    HashMap.modifyAt(held, Data.tuple(fact.app, fact.tenant, fact.resource), (slot) =>
      Option.some(
        Option.match(slot, {
          onNone: (): Fact.Aggregate => ({ count: 1, total: BigInt(fact.quantity) }),
          onSome: (row) => _fused(row, { count: 1, total: BigInt(fact.quantity) }),
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
        BigDecimal.multiply(BigDecimal.make(aggregate.total, 0), rate.per),
        { mode: "half-even", scale: 4 },
      ),
      currency: rate.currency,
    }
  })
```

## [05]-[RAIL]

- Owner: the `Fact` service — a Layer factory over the app's `AppIdentity` (`Fact.Default(identity)`), holding one bounded intake, one scoped drain fiber, one stamp cell, and one unlanded roster; `record` is the ONE entrypoint, modal over its input — an audit draft, a meter charge, or a proven-non-empty batch of either — discriminated on the value, never a sibling per stream; `Fact.audits` satisfies the security `AuditJournal` port by projecting each record through the `_AUDITED` row table onto that same entrypoint.
- Packages: `effect` (`Effect.Service`, `Layer`, `Mailbox`, `Fiber`, `Stream`, `Chunk`, `Schedule`, `Metric`, `Ref`, `HashMap`, `DateTime`, `Option`, `Predicate`, `Record`, `Array`, `Schema`); `@effect/sql` (`SqlClient`); `@rasm/ts/core` (`Convention` — the metric and attribute name rows; `Hlc` — the stamp algebra; `ContentKey` — the roster key); `@rasm/ts/security` (`AuditFault`, `AuditJournal`, `AuditRecord`, `SecurityFact`, `Shredder`); `./retain.ts` (`Retain.seal`, `SubjectKey`).
- Entry: `Fact.record(draft)` for evidence, `Fact.record(charge)` for usage, `Fact.record(batch)` for either in bulk; `Fact.close` is the lossless shutdown a root awaits before its scope falls; `Fact.pending` reads the rows the plane still owes; wiring is `Fact.Default(identity)` provided a scope's `SqlClient` at the root.
- Growth: a new stamped dimension is one line in the stamp; a new drain posture is one `_FLOW` field; a new fact stream extends the union and the discriminant fold, nothing else.
- Law: the service stamps what the caller must not control — `app` from the identity, `stamp` from the rail's own `Hlc` successor cell, tenancy resolved pinned-first so a single-tenant process overrides the draft's key; construction runs the schema filters, so a malformed draft fails the writer, never the drain.
- Law: `Ref<Hlc>` ticked under `Ref.modify` is the stamp cell, so two fibers recording inside one millisecond take distinct successors — an unsynchronized read-modify-write mints one stamp twice, and two facts sharing a stamp share a content key and dedup genuine evidence away as a redelivery. Wall readings enter as `Hlc.physicalOf(now)` and `Hlc.tick` owns the rest, so the branch's one stamp algebra governs and this rail spells none of it.
- Law: drafts are derived schema projections, never hand-declared patches — `Fact.AuditDraft`/`Fact.Charge` re-anchor on the owning field records through `omit("_tag", "app", "stamp")`, then `Schema.attachPropertySignature` restores the decoded family tag without requiring it on encoded ingress; the family's `Option`-carried tenancy survives untouched, the union stays discriminated even when future members acquire overlapping fields, and an edge-arriving draft decodes through the owning projection before it reaches `record`.
- Law: the drain never load-sheds — evidence and billing truth suspend under backpressure and retry unbounded; egress quota belongs to the correctness-adjacent replication seams (`lane/olap.md`'s `ingest`), never to this rail.
- Law: the intake is a `Mailbox` under the `suspend` strategy, never a `Queue`, because `end` is the only lossless stop this branch has — a consumer signalled done still drains the messages already offered, where `Queue.shutdown` cancels pending operations and discards the buffer, and interrupting the drain fiber at scope close abandons both the window in flight and everything queued behind it. `close` ends the intake then joins the drain, and the same pair rides `Effect.addFinalizer` seated AFTER the fork: scope finalizers run last-registered-first, so the flush completes and the fork's own interrupt then reaches a fiber that already finished.
- Law: the unlanded roster is the plane's own accounting — every batch owes its pairs before the append and settles exactly the keys the landing covered, so a bounded shutdown, a dead database, or an intake that refused an offer after `end` all leave those rows readable through `pending` instead of vanishing. Settling by key rather than clearing the slot is what lets a re-armed root still owe what a prior drain never landed.
- Law: the drain is TOTAL by construction — the row projection dies on a fault a constructed member cannot reach, the append's transient rail retries without bound, and `Effect.option` folds the unreachable exhaustion arm so unsettled pairs stay owed. Totality is not a nicety here: it is what types the drain fiber's error channel at `never` and therefore what makes the joining finalizer spellable at all.
- Law: the drain is one pipeline — `Mailbox.toStream` over the intake, `Stream.groupedWithin(width, patience)` so a quiet surface still flushes on latency, the batch appended under an unbounded jittered retry whose delay caps through `Schedule.union` (evidence is never dropped: a dead database suspends the drain, the bounded intake suspends writers, and pressure propagates instead of silently losing billing truth), each deferral counted and logged, then the landing's ACCEPTED rows alone projecting their counters, log lines, and usage in the same pass; counter updates read those rows — each fact's stream, its audit verb and actor class where the stream carries them, and each meter fact's resource and tenant tags — so every axis the instrument census declares is stamped in the same fold and none rides a per-effect metric decorator.
- Law: projection gates on the landing because the retry is unbounded — a batch re-offered after an unacknowledged commit projects nothing the second time, so a lost acknowledgement costs one duplicate INSERT the key absorbs rather than a doubled billing counter and a doubled audit log line. Matched redeliveries carry their own series instead of vanishing, which is what makes a wedged retry visible at all.
- Law: the audit port is a projection, never a second write path — every security record folds onto one `Fact.AuditDraft` and enters through `record`, so the drain, the unbounded retry, the retention window, the subject index, and the DSAR fold serve breach evidence exactly as they serve every other fact; a security-side store is the forked plane this satisfaction deletes.
- Law: the row table is mapped over the security union's own tag set, so a new point fails at the table rather than reaching the journal unclassified; `action` DECODES the record's registry point through the audit brand on the rail the projection already carries — that key spells the dotted verb path the brand refines, and a cast onto a refinement forges the very evidence the fact exists to hold — and `retention` reads the point's own lane class, so neither is a column a row disagrees with. `occurred` carries the record's OWN instant across, because a breached-class record backpressures on its lane before this projection runs and the rail's stamp dates that breach to the drain admitting it, never to the arm that saw it.
- Law: the diff render is total over the scalar shapes the security union admits — text, number, boolean, `bigint`, and an `Option` of any of them, the wrapper flattened rather than stringified — so a case widening with a counter, a version, or an attempt tally lands that value as evidence. Covering the string half alone drops the next non-text field silently, the one failure mode a compliance fold cannot detect from its own output; a structured value stays the seal's, never the diff's.
- Law: a change path is an ESCAPED pointer, never an interpolated field name — the two reserved characters fold to their escapes before the brand admits the value, so a field carrying either lands one location rather than a path a reader resolves into a document that has no such nesting; the escape is what lets the admission be a decode, which is what keeps the brand's refinement true of every row this fold writes.
- Law: the port layer declares AHEAD of the service — the service's static field reads that value at class evaluation, so a `const` seated after the class body is read inside its temporal dead zone and every composition importing this module dies at load rather than at the first append.
- Law: custody is `(app, tenant, subject)` structurally, so a fact carrying no tenant coordinate has no custody row — its `bearing` fields are DROPPED rather than landed unsealed, and the verb, actor class, target, and instant still record the event; sealing runs before the draft leaves, so an identifier reaches the rail already unreadable.
- Law: every audit fact also emits one structured log annotated with the convention's audit rows — observability beside durability; the meter's metric egress is deliberately lossy and bounded — resource tag always, tenant tag only where the resource row's posture admits it — and the journal remains the sole truth for both streams.

```typescript signature
import { Array, Chunk, DateTime, Effect, Fiber, HashMap, Layer, Mailbox, Metric, Option, Predicate, Record, Ref, Schedule, Schema, Stream } from "effect"
import { SqlClient, type SqlError } from "@effect/sql"
import { type ContentKey, Convention, Hlc } from "@rasm/ts/core"
import { AuditFault, AuditJournal, AuditRecord, SecurityFact, Shredder } from "@rasm/ts/security"
import { Retain, SubjectKey } from "./retain.ts"

const _FLOW = { intake: 512, patience: "2 seconds", width: 128 } as const

const _deduped = Convention.mount(Convention.metric.factDeduped)
const _deferred = Convention.mount(Convention.metric.factDeferred)
const _drained = Convention.mount(Convention.metric.factDrained)
const _usage = Convention.mount(Convention.metric.meterUsage)

// Deliberately UNBOUNDED — never a `Budget` row: every compiled budget exhausts, and the drain's own totality law
// depends on a schedule that never does (an unsettled batch stays owed; `pending` names every row). The union caps
// the delay at a 10s cadence, jitter decorrelates the fleet, and the price is stated here, not smuggled.
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

// Every row tags its stream and an audit row adds the verb and actor class the census declares, so the drain counter
// carries exactly its own fan while identifier-grade keys stay on the log record beside it. Counting per fact is what
// makes that fan spellable at all — a batch-sized increment has no row to read an axis off.
const _counted = (fact: Fact.Value): Effect.Effect<void> =>
  Metric.increment(
    fact._tag === "AuditFact"
      ? Metric.tagged(
          Metric.tagged(
            Metric.tagged(_drained, Convention.rasm.factStream, fact._tag),
            Convention.rasm.auditAction,
            fact.action,
          ),
          Convention.rasm.auditActorKind,
          fact.actor.kind,
        )
      : Metric.tagged(_drained, Convention.rasm.factStream, fact._tag),
  )

// Redeliveries the content key matched, tagged by the stream the census declares. The half is exact rather than
// inferred — `DO NOTHING` admits exactly two outcomes per offered row — so a non-zero value is genuine at-least-once
// evidence and a permanently non-zero value is a wedged retry re-offering one window forever, the shape an inflated
// `accepted` would erase.
const _reoffered = (landing: Fact.Landing): Effect.Effect<void> =>
  Effect.forEach(landing.duplicate, (pair) =>
    Metric.increment(Metric.tagged(_deduped, Convention.rasm.factStream, pair.fact._tag)), {
    concurrency: "inherit",
    discard: true,
  })

const _emitted = (fact: Fact.Value): Effect.Effect<void> =>
  Effect.zipRight(
    _counted(fact),
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
      : _metered(fact),
  )

const _AuditDraft = Schema.Struct(AuditFact.fields).omit("_tag", "app", "stamp").pipe(
  Schema.attachPropertySignature("_tag", "AuditFact"),
)

const _Charge = Schema.Struct(MeterFact.fields).omit("_tag", "app", "stamp").pipe(
  Schema.attachPropertySignature("_tag", "MeterFact"),
)

// --- [AUDIT_PORT]

// One row per security fact kind, mapped over the union's own tag set so a new point fails HERE rather than reaching
// an unclassified journal. Derivation drops two columns: `action` reads the record's registry point, already spelling
// its dotted verb path this audit brand refines, and `retention` reads that point's own class. `bearing` names each
// field whose value identifies a person, so the diff fold skips it and the seal takes it.
const _AUDITED: {
  readonly [K in SecurityFact["_tag"]]: {
    readonly actor: (typeof _ACTORS)[number]
    readonly bearing: ReadonlyArray<keyof Extract<SecurityFact, { readonly _tag: K }>>
    readonly subject: (fact: Extract<SecurityFact, { readonly _tag: K }>) => Option.Option<string>
    readonly target: string
  }
} = {
  Admission: { actor: "system", bearing: ["kid"], subject: ({ kid }) => kid, target: "key" },
  Ceremony: { actor: "user", bearing: [], subject: ({ subject }) => Option.some(subject), target: "passkey" },
  Clone: { actor: "user", bearing: ["passkey"], subject: ({ subject }) => Option.some(subject), target: "passkey" },
  Deny: { actor: "user", bearing: [], subject: ({ subject }) => Option.some(subject), target: "policy" },
  Reuse: { actor: "user", bearing: ["sid"], subject: ({ subject }) => Option.some(subject), target: "session" },
  Rotation: { actor: "service", bearing: [], subject: () => Option.none(), target: "secret" },
  ShredOpen: { actor: "system", bearing: [], subject: () => Option.none(), target: "subject" },
}

// Breach evidence outlives routine trails, so the security lane class IS the retention class and no second policy
// vocabulary appears on either side of the seam.
const _RETAINED = { breached: "regulatory", notice: "operational" } as const satisfies Record<SecurityFact.Class, Retain.Class>

// Total over the scalar shapes the union admits, wrapper flattened rather than stringified: a case that widens with a
// signature counter or an attempt tally lands that value instead of disappearing from the diff, which is the one loss
// a compliance fold reading only this output can never detect.
const _rendered = (value: unknown): Option.Option<string> =>
  Option.isOption(value)
    ? Option.flatMap(value, _rendered)
    : Predicate.isString(value)
      ? Option.liftPredicate(value, (held) => held.length > 0)
      : Predicate.isNumber(value) || Predicate.isBoolean(value) || Predicate.isBigInt(value)
        ? Option.some(String(value))
        : Option.none()

// A field name is an identifier only by convention, so the pointer ESCAPES its two reserved characters before the
// brand admits it: an unescaped `/` mints a path a compliance fold reads as a nested location the document has none
// of, and `~` collides with the escape prefix itself. Escaping is what makes the admission a DECODE rather than an
// assertion over a shape the field never promised — the brand is a refinement, and a cast onto one forges evidence.
const _pointer = (field: string): Option.Option<typeof _Path.Type> =>
  Schema.decodeOption(_Path)(`/${field.replaceAll("~", "~0").replaceAll("/", "~1")}`)

// Non-identifying payload fields land as `Assigned` change rows so a compliance fold reads typed evidence rather than
// a rendered blob; an identifying field never joins them, because a `Change` value survives key destruction.
const _changed = (fact: SecurityFact, bearing: ReadonlyArray<string>): ReadonlyArray<Change> =>
  Array.filterMap(
    Record.toEntries(Record.filter(fact as Record.ReadonlyRecord<string, unknown>, (_value, field) =>
      field !== "_tag" && field !== "subject" && field !== "tenant" && !Array.contains(bearing, field))),
    ([field, value]) => Option.zipWith(_rendered(value), _pointer(field), (next, path) => Assigned.make({ next, path })),
  )

// Custody is `(app, tenant, subject)` structurally, so a fact carrying no tenant coordinate has no custody row to key
// on: its identifying fields are DROPPED rather than landed unsealed, and the verb, actor class, target, and instant
// still record the event. Sealing runs before the draft leaves, so the identifier reaches the rail already unreadable
// and an erase after this point redacts the row with no rewrite of the append-only log.
const _audited = (
  record: AuditRecord,
): Effect.Effect<Fact.AuditDraft, SqlError.SqlError | ParseResult.ParseError, Shredder | SqlClient.SqlClient> =>
  Effect.gen(function* () {
    const row = _AUDITED[record.fact._tag]
    const tenant = "tenant" in record.fact ? record.fact.tenant : Option.none()
    const shredder = yield* Shredder
    // The registry point spells the dotted verb path this brand refines, so the projection DECODES it on the rail it
    // already carries rather than asserting the refinement: a point that stops conforming refuses here as evidence
    // instead of landing an unrefined string every downstream grouping then reads as a valid verb.
    const action = yield* Schema.decodeUnknown(_Action)(record.point)
    // Each row's subject accessor already answers `Option`, so the decode runs INSIDE it: handing the wrapper
    // straight to `Schema.decodeOption` parses an `Option` value against a string schema and answers `none` for
    // every fact alike, which silently kills custody, sealing, the subject index, and the DSAR leg while every law
    // reading those columns still assumes them populated.
    const custody = Option.zipWith(
      Option.flatMap(row.subject(record.fact), Schema.decodeOption(Retain.Subject)),
      tenant,
      (subject, held) => new SubjectKey({ app: record.app, subject, tenant: held }),
    )
    const sealed = yield* Effect.transposeOption(Option.zipWith(
      custody,
      Option.liftPredicate(
        Record.filter(record.fact as Record.ReadonlyRecord<string, unknown>, (_value, field) => Array.contains(row.bearing, field)),
        (held) => Record.size(held) > 0,
      ),
      (key, borne) => Retain.seal(shredder, key, new TextEncoder().encode(JSON.stringify(borne))),
    ))
    return {
      _tag: "AuditFact",
      action,
      actor: { key: Option.getOrElse(Option.map(custody, (key) => key.subject), () => record.app), kind: row.actor },
      change: _changed(record.fact, row.bearing),
      occurred: Option.some(record.at),
      retention: _RETAINED[SecurityFact.classOf(record.fact)],
      sealed,
      subject: Option.map(custody, (key) => key.subject),
      target: { key: record.point, kind: row.target, parent: Option.none() },
      tenant,
      trace: Option.none(),
    }
  })

// Satisfaction is a projection, never a second write path: every security record folds onto one audit draft and enters
// through `Fact.record`, so the drain, the retry posture, the retention window, the subject index, and the DSAR fold
// serve security evidence exactly as they serve every other fact. Sealing is the only fault this seam raises, and it
// lands as the port's own `append` reason rather than a foreign rail crossing the Tag. Declaration order is load-
// bearing: the service's static field reads this value at class evaluation, so a `const` seated after the class body
// evaluates in its temporal dead zone and every composition importing this module dies at load.
const _audits: Layer.Layer<AuditJournal, never, Fact | Shredder | SqlClient.SqlClient> = Layer.effect(
  AuditJournal,
  Effect.map(Effect.context<Fact | Shredder | SqlClient.SqlClient>(), (context) => ({
    append: (record: AuditRecord) =>
      Effect.provide(
        Effect.flatMap(_audited(record), (draft) => Fact.record(draft)),
        context,
      ).pipe(Effect.mapError((fault) => new AuditFault({ reason: "append", detail: fault.message }))),
  })),
)

class Fact extends Effect.Service<Fact>()("data/Fact", {
  scoped: (identity: AppIdentity) =>
    Effect.gen(function* () {
      const intake = yield* Mailbox.make<Fact.Value>({ capacity: _FLOW.intake, strategy: "suspend" })
      const clock = yield* Ref.make(Hlc.genesis)
      const owed = yield* Ref.make(HashMap.empty<ContentKey, Fact.Pair>())
      const owing = (pairs: ReadonlyArray<Fact.Pair>): Effect.Effect<void> =>
        Ref.update(owed, (held) => Array.reduce(pairs, held, (map, pair) => HashMap.set(map, pair.row.key, pair)))
      const settling = (pairs: ReadonlyArray<Fact.Pair>): Effect.Effect<void> =>
        Ref.update(owed, (held) => Array.reduce(pairs, held, (map, pair) => HashMap.remove(map, pair.row.key)))
      const landed = (landing: Fact.Landing): Effect.Effect<void> =>
        // ACCEPTED rows alone project: the append retries without bound, so a batch replayed after an unacknowledged
        // commit must cost one absorbed duplicate rather than a second charge and a second audit line, and the
        // matched redeliveries carry their own series instead of vanishing. Both halves settle off the roster,
        // because a matched duplicate is a landed row exactly as an accepted one is.
        settling([...landing.accepted, ...landing.duplicate]).pipe(
          Effect.zipRight(
            Effect.forEach(landing.accepted, (pair) => _emitted(pair.fact), { concurrency: "inherit", discard: true })),
          Effect.zipRight(_reoffered(landing)),
        )
      const drained = (facts: Array.NonEmptyReadonlyArray<Fact.Value>): Effect.Effect<void, never, SqlClient.SqlClient> =>
        Effect.gen(function* () {
          // `record` already ran the family's own filters, so an encode fault here names a declaration defect and
          // never an input one — it dies loudly rather than parking as evidence no database could ever accept.
          const pairs = yield* Effect.orDie(Effect.forEach(facts, _rowed, { concurrency: "unbounded" }))
          yield* owing(pairs)
          const settled = yield* _append(pairs).pipe(
            Effect.tapError((fault) =>
              Metric.increment(_deferred).pipe(Effect.zipRight(
                Effect.logError("fact drain deferred").pipe(Effect.annotateLogs({ count: pairs.length, fault: fault._tag }))))),
            Effect.retry(_RETRY),
            // `_RETRY` never exhausts, so this fold makes the unreachable arm total rather than shedding:
            // an unsettled batch stays owed and `pending` names every row of it.
            Effect.option,
          )
          yield* Effect.transposeOption(Option.map(settled, landed))
        })
      const drain = yield* Effect.forkScoped(
        Mailbox.toStream(intake).pipe(
          Stream.groupedWithin(_FLOW.width, _FLOW.patience),
          Stream.runForEach((batch) => {
            const rows = Chunk.toReadonlyArray(batch)
            return Array.isNonEmptyReadonlyArray(rows) ? drained(rows) : Effect.void
          }),
        ),
      )
      // `end` lets the consumer finish what was already offered where `shutdown` cancels it, and scope finalizers run
      // last-registered-first, so this flush precedes the fork's own interrupt and that interrupt then reaches a
      // fiber which already completed. Both halves are idempotent, so an explicit `close` costs the scope nothing.
      const close = Effect.zipRight(intake.end, Fiber.join(drain))
      yield* Effect.addFinalizer(() => close)
      const stamped = (draft: Fact.Draft): Effect.Effect<void> =>
        Effect.gen(function* () {
          const now = yield* DateTime.now
          // One successor per admitted fact under a single atomic modify, so concurrent recorders never share a
          // coordinate; the branch's stamp algebra owns the physical/logical decision and this rail spells none of it.
          const stamp = yield* Ref.modify(clock, (held) => {
            const next = Hlc.tick(held, Hlc.physicalOf(now))
            return [next, next] as const
          })
          const tenant = Option.orElse(identity.tenant, () => draft.tenant)
          const fact: Fact.Value = draft._tag === "MeterFact"
            ? new MeterFact({
                app: identity.app,
                quantity: draft.quantity,
                resource: draft.resource,
                stamp,
                surface: draft.surface,
                tenant,
              })
            : new AuditFact({
                action: draft.action,
                actor: draft.actor,
                app: identity.app,
                change: draft.change,
                occurred: draft.occurred,
                retention: draft.retention,
                // Custody travels with the draft: sealing runs at that writer, so dropping these two here strands
                // an unindexed subject beside a discarded ciphertext — the erasure spine's whole coordinate —
                // while every downstream law still reads the column as populated.
                sealed: draft.sealed,
                stamp,
                subject: draft.subject,
                target: draft.target,
                tenant,
                trace: draft.trace,
              })
          // `end` makes every later offer answer `false` rather than suspend, so the losing writer in a shutdown
          // race parks its fact on the roster — that boolean is evidence, never a discard.
          const taken = yield* intake.offer(fact)
          yield* taken ? Effect.void : Effect.flatMap(Effect.orDie(_rowed(fact)), (pair) => owing([pair]))
        })
      return {
        record: (input: Fact.Draft | Array.NonEmptyReadonlyArray<Fact.Draft>): Effect.Effect<void> =>
          Effect.forEach(Array.ensure(input), stamped, { concurrency: 1, discard: true }),
        close,
        pending: Effect.map(Ref.get(owed), (held) => Array.fromIterable(HashMap.values(held))),
      }
    }),
  accessors: true,
}) {
  static readonly rollup = _rollup
  static readonly rate = (rolled: HashMap.HashMap<Fact.Key, Fact.Aggregate>, rating: Fact.Rating): HashMap.HashMap<Fact.Key, Fact.Cost> => _rate(rolled, rating)
  static readonly AuditDraft = _AuditDraft
  static readonly Charge = _Charge
  static readonly audits = _audits
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
