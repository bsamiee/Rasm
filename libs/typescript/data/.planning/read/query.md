# [DATA_QUERY]

The typed CRUD engine of the read side: every row that leaves a relation enters domain code as a decoded value and every request that reaches a statement is schema-proven first. `SqlSchema` is the one polymorphic query surface — arity is the combinator (`findAll`, `findOne`, `single`, `void`), never a sibling name — and `SqlResolver` is its batched form, collapsing keyed N+1 fan-out into one round trip per window. `Model.Class` is the one shape authority for every mutable relation: one field record derives all six wire variants through the field families, so the per-variant struct spam the naive read side mints is unspellable. `Query.table(model, spec)` binds a model to its whole bound surface once at service construction — typed reads, the repository, the windowed loaders, and the batch resolvers share one identity so the batch window and the resolver cache survive across calls. The record of truth is exempt by law: the journal never takes a repository, and this engine serves projection tables, ledgers, snapshots, and read models only.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]       | [OWNS]                                                                             |
| :-----: | :-------------- | :------------------------------------------------------------------------------------ |
|  [01]   | `MODEL_FAMILY`  | the `Model.Class` field families — six variants from one declaration, exposure control  |
|  [02]   | `READ_FAMILY`   | the `SqlSchema` typed-query surface — arity in the combinator, one decode rail          |
|  [03]   | `RESOLVER_ROWS` | the `SqlResolver` batch rows — ordered, grouped, findById, void, the bind-once law      |
|  [04]   | `TABLE_BINDING` | `Query.table` — model, reads, repository, loaders, resolvers assembled as one owner     |

## [2]-[MODEL_FAMILY]

- Owner: the model-declaration law — every mutable relation in the folder is one `Model.Class` whose fields state the whole variant matrix; this cluster owns the field-family selection table, not any concrete model (each table-owning page declares its own).
- Packages: `@effect/sql` (`Model` — `Class`, `Generated`, `GeneratedByApp`, `Sensitive`, `FieldOption`, `DateTimeInsert`, `DateTimeUpdate`, `JsonFromString`, `UuidV4Insert`, `BooleanFromNumber`, `Field`, `FieldOnly`, `FieldExcept`, `fieldEvolve`, `fieldFromKey`, `fields`); `effect` (`Schema`).
- Entry: `class Row extends Model.Class<Row>("Row")({ ... })` — one declaration yields `Row` (select), `Row.insert`, `Row.update`, `Row.json`, `Row.jsonCreate`, `Row.jsonUpdate`; `Model.fields(Row)` projects the field record where a derived view re-anchors.
- Growth: a new column is one field row — every variant, the repository, the loaders, and the JSON wire inherit it; a new exposure posture is a field-family swap, never a second model.
- Law: column origin is a field family — `Model.Generated` for engine-minted columns (identity sequences, `uuidv7()` defaults from the spine row) absent from insert, `Model.GeneratedByApp` for app-minted identity present in every database variant; a generated column hand-listed on an insert schema is the drift the family deletes.
- Law: exposure is structural — `Model.Sensitive` rides database variants and is stripped from every JSON variant, so a sealed payload or internal coordinate cannot reach the wire through any derived JSON shape; egress scrubbing at call sites is the rejected spelling.
- Law: temporal stamps are family rows — `Model.DateTimeInsert`/`Model.DateTimeUpdate` mint on the rail at write, serialized per column type; a hand-stamped `now` beside a model restates the family.
- Law: embedded JSON is `Model.JsonFromString` — TEXT in database variants, native object in JSON variants — so the jsonb-versus-TEXT dialect difference lives in the model and no consumer parses a payload column; the journal's event row already states this law and this cluster generalizes it to every read-model payload.
- Law: absence is `Model.FieldOption` — nullable in database variants, missing-key `Option` in JSON — one field, all variants optionalized; the sqlite boolean crossing is `Model.BooleanFromNumber`, dialect difference as a field fact.
- Law: the JSON variants are the edge's material — `Row.json`/`jsonCreate`/`jsonUpdate` are the wire shapes a serving surface encodes and admits; a hand-declared DTO beside a model is the parallel-shape defect the variant system exists to kill.
- Boundary: `journal_event` and `fact_journal` are append-only evidence — their models exist for row typing only and the repository ban on them is `journal/append.md`'s law; the retention `Sensitive` posture serves `journal/retain.md`'s DSAR export, which reads JSON variants and leaks nothing by construction.

```typescript
import { Schema } from "effect"
import { Model } from "@effect/sql"
import { AppIdentity, TenantContext } from "@rasm/ts/core"

class Board extends Model.Class<Board>("Board")({
  id: Model.Generated(Schema.Number),
  app: AppIdentity.fields.app,
  tenant: TenantContext.fields.tenant,
  cell: Schema.NonEmptyString,
  title: Schema.NonEmptyString.pipe(Schema.maxLength(200)),
  state: Model.JsonFromString(Schema.Unknown),
  pinned: Model.BooleanFromNumber,
  secret: Model.Sensitive(Schema.String),
  note: Model.FieldOption(Schema.NonEmptyString),
  createdAt: Model.DateTimeInsert,
  revisedAt: Model.DateTimeUpdate,
}) {}
```

## [3]-[READ_FAMILY]

- Owner: the typed-query law — `SqlSchema` at the package surface, one constructor over `{ Request, Result, execute }` whose arity member is the combinator choice; every decoded read in the folder is one of these four forms.
- Packages: `@effect/sql` (`SqlSchema.findAll`, `SqlSchema.findOne`, `SqlSchema.single`, `SqlSchema.void`); `effect` (`Schema`, `Option`).
- Entry: the bound accessor is minted once at the owning service construction and called thereafter — `const found = SqlSchema.findAll({ Request, Result, execute })` at build, `found(input)` per call; a fused accessor rebuilt inside a call body re-pays construction per call and is the named defect.
- Receipt: the four return contracts are the arity vocabulary — `findAll` a decoded array, `findOne` an `Option`, `single` exactly-one-or-typed-failure, `void` no result decode; a caller distinguishing zero-from-many reads the contract, never a length probe.
- Growth: a new read shape is one accessor with its own `Request`/`Result` pair — the statement varies, the law never does; a request axis (window, filter) is a `Request` field, never a sibling accessor.
- Law: both edges decode — the `Request` schema proves input before the statement binds, the `Result` schema proves every `Connection.Row` before domain code sees it, and both misses ride `ParseError` on the one admission rail; a `String(row["col"])`/`Number(...)` cast beside a statement is the untyped read this family deletes.
- Law: the `Result` schema of a model-backed read is the model itself or a projection re-anchored on `Model.fields` — never a hand-declared row struct restating columns.
- Law: the request schema carries the domain brand — a read keyed by `StreamKey` fields, `ContentKey`, or a tenant brand admits through the owning schema, so an unbranded string cannot address a keyed relation.

```typescript
import { Option, Schema, pipe } from "effect"
import { SqlClient, SqlSchema } from "@effect/sql"

const _Window = Schema.Struct({
  app: AppIdentity.fields.app,
  floor: Schema.optionalWith(Schema.Number, { default: () => 0 }),
  take: Schema.Number.pipe(Schema.int(), Schema.between(1, 500)),
})

const _reads = (sql: SqlClient.SqlClient) => ({
  page: SqlSchema.findAll({
    Request: _Window,
    Result: Board,
    execute: (window) =>
      sql`SELECT * FROM board WHERE app = ${window.app} AND id > ${window.floor} ORDER BY id LIMIT ${window.take}`,
  }),
  byCell: SqlSchema.findOne({
    Request: Schema.NonEmptyString,
    Result: Board,
    execute: (cell) => sql`SELECT * FROM board WHERE cell = ${cell}`,
  }),
  head: SqlSchema.single({
    Request: AppIdentity.fields.app,
    Result: Schema.Struct({ top: Schema.Number }),
    execute: (app) => sql`SELECT coalesce(max(id), 0) AS top FROM board WHERE app = ${app}`,
  }),
  retitle: SqlSchema.void({
    Request: Schema.Struct({ cell: Schema.NonEmptyString, title: Schema.NonEmptyString }),
    execute: (patch) => sql`UPDATE board SET title = ${patch.title} WHERE cell = ${patch.cell}`,
  }),
})
```

## [4]-[RESOLVER_ROWS]

- Owner: the batch-resolver vocabulary — the four `SqlResolver` rows, the bind-once identity law, and the cache verbs; the general non-SQL batching engine is `read/batch.md`'s and these rows are its SQL specialization, fused with the decode law.
- Packages: `@effect/sql` (`SqlResolver.ordered`, `SqlResolver.grouped`, `SqlResolver.findById`, `SqlResolver.void`, `ResultLengthMismatch`); `effect` (`Schema`, `Option`).
- Entry: `resolver.execute(input)` is the one call surface — every caller in a flow shares the bound resolver, so concurrent keyed reads collapse into one statement window; `Effect.withRequestCaching(true)` composed at the flow boundary deduplicates repeated keys across the whole graph, and the request-cache Layer is `lane/cache.md`'s `dedup` row.
- Receipt: `cachePopulate(id, result)` seeds the resolver cache from a write's own returning row and `cacheInvalidate(id)` evicts on mutation — write-through coherence as resolver verbs, never a parallel cache map.
- Growth: a new keyed lookup is one resolver row; a one-to-many axis is `grouped`'s key pair, never a per-parent loop.
- Law: row selection is the relation's answer shape — `ordered` for strict 1:1 position-matched batches (`ResultLengthMismatch` guards the integrity), `grouped` for 1:N regrouped by extracted key, `findById` for id-keyed `Option` lookups, `void` for batched writes; choosing `ordered` where the statement drops misses is the integrity fault the guard exists to surface — the `StreamHead` row rides `findById` for exactly this reason, because a stream with zero events is a lawful `Option.none` the caller folds to head zero, never a length mismatch.
- Law: resolvers bind once at the owning service construction — batch windows group by resolver identity, so a resolver minted per call defeats the window structurally; the same law governs the fused accessors of `[3]`.
- Law: the batch statement is one set-shaped query — `sql.in` over the window's keys, `GROUP BY`/window functions where the group row demands — never a per-request statement inside the resolver body.

```typescript
import { SqlResolver } from "@effect/sql"
import { Journal } from "../journal/append.ts"

const _resolvers = (sql: SqlClient.SqlClient) => ({
  boards: SqlResolver.findById("BoardByCell", {
    Id: Schema.NonEmptyString,
    Result: Board,
    ResultId: (row) => row.cell,
    execute: (cells) => sql`SELECT * FROM board WHERE ${sql.in("cell", cells)}`,
  }),
  members: SqlResolver.grouped("MembersByBoard", {
    Request: Schema.NonEmptyString,
    RequestGroupKey: (cell) => cell,
    Result: Schema.Struct({ cell: Schema.NonEmptyString, member: Schema.NonEmptyString }),
    ResultGroupKey: (row) => row.cell,
    execute: (cells) => sql`SELECT cell, member FROM board_member WHERE ${sql.in("cell", cells)}`,
  }),
  touch: SqlResolver.void("TouchBoard", {
    Request: Schema.NonEmptyString,
    execute: (cells) => sql`UPDATE board SET revised_at = ${Journal.now(sql)} WHERE ${sql.in("cell", cells)}`,
  }),
  heads: SqlResolver.findById("StreamHead", {
    Id: Schema.String,
    Result: Schema.Struct({ id: Schema.String, head: Schema.Number }),
    ResultId: (row) => row.id,
    execute: (ids) =>
      sql`SELECT app || ':' || tenant || ':' || aggregate AS id, coalesce(max(version), 0) AS head
          FROM journal_event WHERE (app || ':' || tenant || ':' || aggregate) IN ${sql.in(ids)}
          GROUP BY app, tenant, aggregate`,
  }),
})
```

## [5]-[TABLE_BINDING]

- Owner: `Query.table(model, spec)` — the assembled per-relation owner: the repository verbs, the windowed data loaders, the caller's typed reads and resolver rows (`spec.reads(sql)`/`spec.resolvers(sql)` built once against the leased client), and the relation's ensure row, bound once and published as one value; `SqlSchema` and `SqlResolver` are consumed at the package surface directly — no alias table forwards them.
- Packages: `@effect/sql` (`Model.makeRepository`, `Model.makeDataLoaders`); `effect` (`Effect`, `Duration`, `Scope`).
- Entry: an owning service constructs `Query.table(Board, { tableName: "board", idColumn: "cell", spanPrefix: "board", window: "20 millis", ensure: _boardDdl, reads: _reads, resolvers: _resolvers })` inside its build effect and returns members that close over the binding; consumers reach rows through the service, never through a loose repository const.
- Receipt: every member is span-instrumented under `spanPrefix` by the underlying helpers — per-relation read/write telemetry arrives with zero per-call wiring.
- Growth: a new relation is one `Query.table` call in its owning service; a new access pattern on an existing relation is a `reads`/`resolvers` row in the spec, landing on the same bound identity.
- Law: the repository serves projection, ledger, snapshot, and read-model tables — `insert`, `update`, `findById`, `delete` with variant-schema I/O; the event journal and the fact journal never take one, and erasure remains key destruction under `journal/retain.md`.
- Law: the loaders are the windowed write/read collapse — `makeDataLoaders` mints `insert`/`findById`/`delete` loaders over `SqlClient | Scope` whose `window` batches across fibers; the window value is the spec's, sourced from configuration, never a literal.
- Law: one binding per relation per scope — the binding constructs inside the tenancy scope's Layer (`lane/tenant.md`'s `Stores` family), so resolver identity, loader windows, and span prefixes are scope-local and cross-tenant batching is unrepresentable.

```typescript
import { Duration, Effect, type Scope } from "effect"
import type { Capability } from "../lane/capability.ts"

declare namespace Query {
  type Spec<RD, RS> = {
    readonly tableName: string
    readonly idColumn: string
    readonly spanPrefix: string
    readonly window: Duration.DurationInput
    readonly ensure: Capability.Ensure
    readonly reads: (sql: SqlClient.SqlClient) => RD
    readonly resolvers: (sql: SqlClient.SqlClient) => RS
  }
}

const _table = <M extends Model.Any, RD, RS>(model: M, spec: Query.Spec<RD, RS>) =>
  Effect.gen(function* () {
    const sql = yield* SqlClient.SqlClient
    const repository = yield* Model.makeRepository(model, {
      tableName: spec.tableName,
      spanPrefix: spec.spanPrefix,
      idColumn: spec.idColumn,
    })
    const loaders = yield* Model.makeDataLoaders(model, {
      tableName: spec.tableName,
      spanPrefix: spec.spanPrefix,
      idColumn: spec.idColumn,
      window: spec.window,
    })
    return { repository, loaders, reads: spec.reads(sql), resolvers: spec.resolvers(sql), ensure: spec.ensure }
  })

const Query = {
  table: _table,
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Query }
```
