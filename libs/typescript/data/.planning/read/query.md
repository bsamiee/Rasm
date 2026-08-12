# [DATA_QUERY]

Typed CRUD rules the read side: every row leaving a relation enters domain code as a decoded value and every request reaching a statement proves against a schema first.

`SqlSchema` is the one polymorphic query surface — arity is the combinator (`findAll`, `findOne`, `single`, `void`), never a sibling name — and `SqlResolver` is its batched form, collapsing keyed N+1 fan-out into one round trip per window with write-through cache verbs riding the same resolver value. `Model.Class` is the one shape authority for every mutable relation: one field record derives all six wire variants through the field families, so per-variant struct spam is unspellable and every embedded payload column carries its own schema authority.

`Query.Relation` owns identifier evidence, span identity, and batch timing as one admitted value, so every identifier position derives its scalar evidence there. `Query.table(model, spec)` binds a model to its whole bound surface once at construction, so typed reads, repository, windowed loaders, and batch resolvers share one identity while the batch window and resolver cache survive across calls. Each spec's own shape selects the modality: a native case reads the ambient client and earns every verb, an ingress case names a foreign engine as an explicit Tag and earns the read half alone.

Truth records stay exempt by law: the journal never takes a repository, and this engine serves projection tables, ledgers, snapshots, and read models alone.

## [01]-[INDEX]

- [02]-[MODEL_FAMILY]: `Model.Class` field families — six variants from one declaration, exposure control.
- [03]-[READ_FAMILY]: `SqlSchema` typed-query surface — arity in the combinator, one decode rail.
- [04]-[RESOLVER_ROWS]: `SqlResolver` batch rows — ordered, grouped, findById, void, the cache verbs.
- [05]-[TABLE_BINDING]: `Query.Relation` and `Query.table` — identity, timing, and the case-projected verb set on one owner.
- [06]-[FOREIGN_RELATIONAL_READS]: `Query.table` ingress case — admitted foreign Tags, structural read-only, T-SQL parameters and procedures, dialect forks.
- [07]-[ORGANIZATION_ROWS]: model organization on the read side — entity relation, containment and view edge relations, resolver rows.

## [02]-[MODEL_FAMILY]

- Owner: the model-declaration law — every mutable relation in the folder is one `Model.Class` whose fields state the whole variant matrix; this cluster owns the field-family selection table, not any concrete model (each table-owning page declares its own).
- Packages: `@effect/sql` (`Model` — `Class`, `Generated`, `GeneratedByApp`, `Sensitive`, `FieldOption`, `DateTimeInsert`, `DateTimeUpdate`, `JsonFromString`, `UuidV4Insert`, `BooleanFromNumber`, `Field`, `FieldOnly`, `FieldExcept`, `fieldEvolve`, `fieldFromKey`, `fields`); `effect` (`Schema`).
- Entry: `class Row extends Model.Class<Row>("Row")({ ... })` — one declaration yields `Row` (select), `Row.insert`, `Row.update`, `Row.json`, `Row.jsonCreate`, `Row.jsonUpdate`; `Row.fields` is the resolved select-variant record a derived view re-anchors on — `Model.fields(Row)` yields variant `Field` objects `Schema.Struct` refuses.
- Growth: a new column is one field row — every variant, the repository, the loaders, and the JSON wire inherit it; a new exposure posture is a field-family swap, never a second model.
- Law: column origin is a field family — `Model.Generated` for engine-minted columns (identity sequences, `uuidv7()` defaults from the spine row) absent from insert, `Model.GeneratedByApp` for app-minted identity present in every database variant; a generated column hand-listed on an insert schema is the drift the family deletes.
- Law: exposure is structural — `Model.Sensitive` rides database variants and is stripped from every JSON variant, so a sealed payload or internal coordinate cannot reach the wire through any derived JSON shape; egress scrubbing at call sites is the rejected spelling.
- Law: temporal stamps are family rows — `Model.DateTimeInsert`/`Model.DateTimeUpdate` mint on the rail at write, serialized per column type; a hand-stamped `now` beside a model restates the family.
- Law: field names ARE column names — the folder's clients compose no name transforms, so every model field, `Result` struct key, and insert-row key carries the physical snake-case spelling; a camelCase field over a snake column is the silent-mismatch defect, and renaming for the wire is `Model.fieldFromKey` at the JSON variant, never a client transform.
- Law: embedded JSON rides `Model.JsonFromString` over the payload's OWN schema — TEXT in database variants, a decoded typed value in JSON variants — so the jsonb-versus-TEXT dialect difference lives in the model, no consumer parses a payload column, and no consumer meets `unknown` past the model boundary; `Schema.Unknown` inside a `JsonFromString` field is the deleted spelling, because it forces exactly the second admission this engine exists to prevent. Journal envelopes stay the ONE exemption — `journal/evolve.md`'s upcast fold holds their payload authority per event, and that posture never generalizes to a read model.
- Law: absence is `Model.FieldOption` — nullable in database variants, missing-key `Option` in JSON — one field, all variants optionalized; the sqlite boolean crossing is `Model.BooleanFromNumber`, dialect difference as a field fact.
- Law: the JSON variants are the edge's material — `Row.json`/`jsonCreate`/`jsonUpdate` are the wire shapes a serving surface encodes and admits; a hand-declared DTO beside a model is the parallel-shape defect the variant system exists to kill.
- Boundary: `journal_event` and `fact_journal` are append-only evidence — their models exist for row typing only and the repository ban on them is `journal/append.md`'s law; the retention `Sensitive` posture serves `journal/retain.md`'s DSAR export, which reads JSON variants and leaks nothing by construction.

```typescript signature
import { Schema } from "effect"
import { Model } from "@effect/sql"
import { Identity } from "@rasm/ts/core"

const _BoardState = Schema.Struct({
  // payload columns carry their own authority: consumers reach board.state.lanes typed, no second decode anywhere
  lanes: Schema.Array(Schema.Struct({ key: Schema.NonEmptyString, order: Schema.Array(Schema.NonEmptyString) })),
  theme: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
})

class Board extends Model.Class<Board>("Board")({
  id: Model.Generated(Schema.Number),
  app: Identity.App.fields.app,
  tenant: Identity.Tenant.fields.tenant,
  cell: Schema.NonEmptyString,
  title: Schema.NonEmptyString.pipe(Schema.maxLength(200)),
  state: Model.JsonFromString(_BoardState),
  pinned: Model.BooleanFromNumber,
  secret: Model.Sensitive(Schema.String),
  note: Model.FieldOption(Schema.NonEmptyString),
  created_at: Model.DateTimeInsert,
  revised_at: Model.DateTimeUpdate,
}) {}
```

## [03]-[READ_FAMILY]

- Owner: the typed-query law — `SqlSchema` at the package surface, one constructor over `{ Request, Result, execute }` whose arity member is the combinator choice; every decoded read in the folder is one of these four forms.
- Packages: `@effect/sql` (`SqlSchema.findAll`, `SqlSchema.findOne`, `SqlSchema.single`, `SqlSchema.void`); `effect` (`Schema`, `Option`).
- Entry: the bound accessor is minted once at the owning service construction and called thereafter — `const found = SqlSchema.findAll({ Request, Result, execute })` at build, `found(input)` per call; a fused accessor rebuilt inside a call body re-pays construction per call and is the named defect.
- Receipt: the four return contracts are the arity vocabulary — `findAll` a decoded array, `findOne` an `Option`, `single` exactly-one-or-typed-failure, `void` no result decode; a caller distinguishing zero-from-many reads the contract, never a length probe.
- Growth: a new read shape is one accessor with its own `Request`/`Result` pair — the statement varies, the law never does; a request axis (window, filter) is a `Request` field, never a sibling accessor.
- Law: both edges decode — the `Request` schema proves input before the statement binds, the `Result` schema proves every `Connection.Row` before domain code sees it, and both misses ride `ParseError` on the one admission rail; a `String(row["col"])`/`Number(...)` cast beside a statement is the untyped read this family deletes.
- Law: the `Result` schema of a model-backed read is the model itself or a projection re-anchored on the model's `.fields` — never a hand-declared row struct restating columns; a JSON column inside a non-model `Result` composes `journal/evolve.md`'s `Upcast.json(shape)` so the parse-if-string dialect difference stays one codec folder-wide.
- Law: each request key composes its owner schema; an unbranded string cannot address a keyed relation.

```typescript signature
import { Schema } from "effect"
import { SqlClient, SqlSchema } from "@effect/sql"

const _Window = Schema.Struct({
  app: Identity.App.fields.app,
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
    Request: Identity.App.fields.app,
    Result: Schema.Struct({ top: Schema.Number }),
    execute: (app) => sql`SELECT coalesce(max(id), 0) AS top FROM board WHERE app = ${app}`,
  }),
  retitle: SqlSchema.void({
    Request: Schema.Struct({ cell: Schema.NonEmptyString, title: Schema.NonEmptyString }),
    execute: (patch) => sql`UPDATE board SET title = ${patch.title} WHERE cell = ${patch.cell}`,
  }),
})
```

## [04]-[RESOLVER_ROWS]

- Owner: the batch-resolver vocabulary — the four `SqlResolver` rows, the bind-once identity law, and the write-through cache verbs; the general non-SQL batching engine is `read/batch.md`'s and these rows are its SQL specialization, fused with the decode law.
- Packages: `@effect/sql` (`SqlResolver.ordered`, `SqlResolver.grouped`, `SqlResolver.findById`, `SqlResolver.void`; the row members `execute`, `makeExecute`, `cachePopulate`, `cacheInvalidate`; `SqlError.ResultLengthMismatch`); `effect` (`Schema`, `Option`, `Effect`); `read/batch.md` (`Batch.Engine`, `Batch.windowed` — the one wall-clock geometry member).
- Entry: `resolver.execute(input)` is the one call surface — every caller in a flow shares the bound resolver, so concurrent keyed reads collapse into one statement window; `Effect.withRequestCaching(true)` composed at the flow boundary deduplicates repeated keys across the whole graph, and the request-cache Layer is `lane/cache.md`'s `dedup` row.
- Receipt: `cachePopulate(id, result)` seeds the resolver cache from a write's own returning row and `cacheInvalidate(id)` evicts on mutation — write-through coherence as resolver verbs, never a parallel cache map; the seed rides the write's own tap so a flow that inserts then reads never re-queries what it just proved.
- Growth: a new keyed lookup is one resolver row; a one-to-many axis is `grouped`'s key pair, never a per-parent loop.
- Law: row selection is the relation's answer shape — `ordered` for strict 1:1 position-matched batches where the statement echoes its inputs (`INSERT ... RETURNING` is the canonical form and `SqlError.ResultLengthMismatch` guards the integrity), `grouped` for 1:N regrouped by extracted key, `findById` for id-keyed `Option` lookups, `void` for batched writes; choosing `ordered` where the statement drops misses is the integrity fault the guard exists to surface — the `StreamHead` row rides `findById` for exactly this reason, because a stream with zero events is a lawful `Option.none` the caller folds to head zero, never a length mismatch.
- Law: resolvers bind once at the owning service construction — batch windows group by resolver identity, so a resolver minted per call defeats the window structurally; the same law governs the fused accessors of `[03]`.
- Law: the batch statement is one set-shaped query — `sql.in` over the window's keys, `GROUP BY`/window functions where the group row demands — never a per-request statement inside the resolver body.
- Law: window geometry reaches a SQL row through `Batch.windowed` and `makeExecute` — every row IS a `RequestResolver`, so the batch engine's one geometry member wraps the value unchanged and `makeExecute` re-binds the typed `execute` over the wrapped resolver; re-deriving the wall-clock wrap through `RequestResolver.dataLoader` at a second site mints a second geometry owner, and wrapping without the re-bind hands back a bare resolver over `SqlRequest` and forfeits the schema-proven call surface these rows exist to hold.
- Law: the durable band stops at this provider — `SqlRequest` is a plain request carrying no payload, success, or failure schema, so the persisted geometry composes over `read/batch.md`'s declared families alone and a SQL lookup wanting restart-survival caches its decoded answer at the cache lane; a row promising persistence here needs a second request declaration beside the one the resolver already mints.
- Boundary: the `StreamHead` row reads `journal_event` under `journal/append.md`'s published read contract — the columns it touches are the append page's declared evidence surface, the repository ban holds, and the fused resolver wins here because the provider IS the database.

```typescript signature
import { Effect, Option, Schema } from "effect"
import { SqlClient, SqlResolver } from "@effect/sql"
import { Journal, StreamKey } from "../journal/append.ts"
import { Batch } from "./batch.ts"

const _resolverRows = (sql: SqlClient.SqlClient) => ({
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
  minted: SqlResolver.ordered("MintBoard", {
    // echo statements answer in insert order, and a dropped row raises SqlError.ResultLengthMismatch rather than passing silently
    Request: Board.insert,
    Result: Board,
    execute: (rows) => sql`INSERT INTO board ${sql.insert(rows)} RETURNING *`,
  }),
  touch: SqlResolver.void("TouchBoard", {
    Request: Schema.NonEmptyString,
    execute: (cells) => sql`UPDATE board SET revised_at = ${Journal.now(sql)} WHERE ${sql.in("cell", cells)}`,
  }),
  heads: SqlResolver.findById("StreamHead", {
    Id: Schema.String,
    Result: Schema.Struct({ id: Schema.String, head: Journal.Version }),
    ResultId: (row) => row.id,
    execute: (ids) =>
      // Append owner spells the composed identity once as its own fragment, composed twice here — so the id this
      // resolver keys on and the string the advisory lock hashes cannot drift apart on a separator or an order.
      sql`SELECT ${StreamKey.identityColumn(sql)} AS id, coalesce(max(version), 0) AS head
          FROM journal_event WHERE (${StreamKey.identityColumn(sql)}) IN ${sql.in(ids)}
          GROUP BY app, tenant, aggregate`,
  }),
})

type _Resolvers = Query.Resolved<ReturnType<typeof _resolverRows>>

const _grown = (resolvers: _Resolvers, draft: typeof Board.insert.Type) =>
  Effect.tap(resolvers.minted.execute(draft), (row) => resolvers.boards.cachePopulate(row.cell, row)) // write-through: the returning row seeds the read cache in the same flow

const _retired = (resolvers: _Resolvers, cell: Board["cell"]) =>
  Effect.zipRight(resolvers.touch.execute(cell), resolvers.boards.cacheInvalidate(cell)) // mutation evicts: the next read re-proves against the relation

// Realizing the `board` lane of `read/batch.md`'s census: the engine's one geometry member wraps the row (a
// `SqlResolver` IS a `RequestResolver`) answering a bare resolver over `SqlRequest`, and `makeExecute` re-binds its
// typed call surface — the collapse window widens to the wall clock and the decode contract does not move.
const _windowed = (resolvers: _Resolvers, engine: Batch.Engine) =>
  Effect.map(Batch.windowed(resolvers.boards, engine), (wrapped) => resolvers.boards.makeExecute(wrapped))
```

## [05]-[TABLE_BINDING]

- Owner: `Query.Relation` and `Query.table(model, spec)` — the admitted per-relation identity, the closed two-case spec family, and the verb set each case earns: identifier evidence, span identity, batch timing, caller-owned typed reads and resolver rows (`spec.reads(sql)`/`spec.resolvers(sql)` built once against the resolved client), and on the native case the repository, the windowed loaders, and the ensure row; `SqlSchema` and `SqlResolver` are consumed at the package surface directly — no alias table forwards them.
- Cases: `Query.Native` carries `id` and `ensure` and reads the ambient `SqlClient.SqlClient`; `Query.Ingress` carries a `client` Tag off the closed `Query.Foreign` union and carries neither, because this branch owns no DDL and no identity on a foreign relation. `[06]` owns the ingress lane's law.
- Packages: `@effect/sql` (`Model.makeRepository`, `Model.makeDataLoaders`, `SqlClient.SqlClient`); `effect` (`Context.Tag`, `Effect`, `Schema.Class`, `Schema.DurationFromSelf`, `Scope`).
- Entry: an owning service admits `new Query.Relation({ table, spanPrefix: "board", window })`, constructs `Query.table(Board, { relation, id: "cell", ensure: _boardDdl, reads: _reads, resolvers: _resolverRows })` inside its build effect, and returns members that close over the binding; consumers reach rows through the service, never through a loose repository const.
- Receipt: every member is span-instrumented under `relation.spanPrefix` by the underlying helpers — per-relation read/write telemetry arrives with zero per-call wiring — and the returned binding carries the admitted `relation` beside the verbs, so lag meters and admin surfaces read `binding.relation` instead of re-threading configuration.
- Growth: a new relation is one `Query.table` call in its owning service; a new access pattern on an existing relation is a `reads`/`resolvers` row in the spec, landing on the same bound identity; a new engine is one member on `Query.Foreign`.
- Law: the spec's own shape IS the modality — a `client` slot naming a concrete driver Tag selects ingress, an `id`-plus-`ensure` pair selects native, and one overload set over that discriminated input serves both; a `foreign` boolean, an arity twin, or a sibling `Query.ingress` re-describes what the value already carries.
- Law: the product is PROJECTED off the case, never widened to a union each consumer re-narrows — `Query.Bound<M, S>` infers the id column from the native arm and answers `repository`, `loaders`, and `ensure` only there, so an ingress binding has no write member to reach for and read-only is a missing slot rather than a guarded one. `Query.Bound<M, S>` narrows its requirement channel on the same discriminant: native leases `SqlClient.SqlClient | Scope.Scope` for the loaders, ingress resolves its own driver Tag and needs no scope because it mints none.
- Law: identifier evidence is a field refinement on `Query.Relation`, never a standalone branded export — `Schema.decodeSync(Query.Relation.fields.table)("board")` over a page-authored literal is total by construction, a caller-derived string has no road to an identifier position, and every sibling page that interpolates a relation or column name into DDL or a fragment (`read/fold.md`'s lane tables, `read/search.md`'s corpus and facet dims) derives that field schema from `Query.Relation`; one lexical class, one owner, zero second spellings.
- Law: the `id` field is typed against the model — `Query.Column<M>` is `keyof M["Type"] & keyof M["update"]["Type"] & keyof M["fields"] & string`, the exact bound `Model.makeRepository` and `Model.makeDataLoaders` demand — so a misspelled id column is a compile error at the spec, never a runtime miss.
- Law: the repository serves projection, ledger, snapshot, and read-model tables — `insert`, `update`, `findById`, `delete` with variant-schema I/O; the event journal and the fact journal never take one, and erasure remains key destruction under `journal/retain.md`.
- Law: the loaders are the windowed write/read collapse — `makeDataLoaders` mints `insert`/`findById`/`delete` loaders over `SqlClient | Scope` whose `window` batches across fibers; the window value is the spec's, sourced from configuration, never a literal.
- Law: resolver constructors are scoped effects, not resolver values — `Query.table` settles every resolver row concurrently at binding and publishes `Query.Resolved<Rows>`; calling `execute`, `cachePopulate`, or `cacheInvalidate` on an unsettled constructor effect is an impossible surface. Both cases settle identically, so a foreign lane pays the same one-window law.
- Law: one binding per relation per scope — the binding constructs inside the tenancy scope's Layer (`lane/tenant.md`'s `Stores` family), so resolver identity, loader windows, and span prefixes are scope-local and cross-tenant batching is unrepresentable.

```typescript signature
import { Context, Effect, Schema, Scope } from "effect"
import { Model, SqlClient } from "@effect/sql"
import type { MssqlClient } from "@effect/sql-mssql"
import type { MysqlClient } from "@effect/sql-mysql2"
import type { Capability } from "../lane/capability.ts"

const _Ident = Schema.NonEmptyString.pipe(Schema.pattern(/^[a-z_][a-z0-9_]*$/), Schema.brand("SqlIdent"))

class _Relation extends Schema.Class<_Relation>("Query.Relation")({
  table: _Ident,
  spanPrefix: Schema.NonEmptyString,
  window: Schema.DurationFromSelf,
}) {}

declare namespace Query {
  type Relation = _Relation
  type ResolverRows = Readonly<Record<string, Effect.Effect<unknown, unknown, unknown>>>
  type Resolved<Rows extends ResolverRows> = { readonly [K in keyof Rows]: Effect.Effect.Success<Rows[K]> }
  type Column<M extends Model.AnyNoContext> =
    keyof M["Type"] & keyof M["update"]["Type"] & keyof M["fields"] & string
  // `Foreign` closes the engine roster: a third engine lands as ONE member here and one optional arm wherever a
  // statement forks — never a second entrypoint, a second product, or a second binding law.
  type Foreign = MysqlClient.MysqlClient | MssqlClient.MssqlClient
  type Native<M extends Model.AnyNoContext, Id extends Column<M>, RD, RS extends ResolverRows> = {
    readonly relation: Relation
    readonly id: Id
    readonly ensure: Capability.Ensure
    readonly reads: (sql: SqlClient.SqlClient) => RD
    readonly resolvers: (sql: SqlClient.SqlClient) => RS
  }
  // No `id` and no `ensure` slot exists to give: this branch owns no DDL and no identity on a foreign relation, so
  // `Capability.Ensure` never widens to carry a dialect it cannot apply.
  type Ingress<C extends Foreign, RD, RS extends ResolverRows> = {
    readonly relation: Relation
    readonly client: Context.Tag<C, C>
    readonly reads: (sql: C) => RD
    readonly resolvers: (sql: C) => RS
  }
  type Spec<M extends Model.AnyNoContext> =
    | Native<M, Column<M>, unknown, ResolverRows>
    | Ingress<Foreign, unknown, ResolverRows>
  type Client<S> = S extends { readonly client: Context.Tag<infer C, infer C> } ? C : SqlClient.SqlClient
  // One product owner, projected by the case rather than unioned across it: the native arm infers its id column
  // here, so the repository and loader shapes stay keyed to the exact column the spec proved, and the ingress arm
  // simply HAS no write member — read-only is unspellable rather than refused at run time.
  type Bound<M extends Model.AnyNoContext, S extends Spec<M>> =
    & {
      readonly model: M
      readonly relation: Relation
      readonly reads: ReturnType<S["reads"]>
      readonly resolvers: Resolved<ReturnType<S["resolvers"]>>
    }
    & (S extends { readonly id: infer Id extends Column<M> } ? {
        readonly ensure: Capability.Ensure
        readonly repository: Effect.Effect.Success<ReturnType<typeof Model.makeRepository<M, Id>>>
        readonly loaders: Effect.Effect.Success<ReturnType<typeof Model.makeDataLoaders<M, Id>>>
      }
      : Record<never, never>)
}

// Whole verb set: DDL owned, identity bound, repository and loaders minted beside the reads.
const _native = <M extends Model.AnyNoContext, const S extends Query.Native<M, Query.Column<M>, unknown, Query.ResolverRows>>(
  model: M,
  spec: S,
): Effect.Effect<Query.Bound<M, S>, never, SqlClient.SqlClient | Scope.Scope> =>
  Effect.gen(function* () {
    const sql = yield* SqlClient.SqlClient
    const [repository, loaders, resolvers] = yield* Effect.all([
      Model.makeRepository(model, {
        tableName: spec.relation.table,
        spanPrefix: spec.relation.spanPrefix,
        idColumn: spec.id,
      }),
      Model.makeDataLoaders(model, {
        tableName: spec.relation.table,
        spanPrefix: spec.relation.spanPrefix,
        idColumn: spec.id,
        window: spec.relation.window,
      }),
      Effect.all(spec.resolvers(sql), { concurrency: "unbounded" }),
    ], { concurrency: "unbounded" })
    return { model, relation: spec.relation, ensure: spec.ensure, repository, loaders, reads: spec.reads(sql), resolvers }
  })

// Read half alone. The engine resolves from the spec's OWN Tag, so the ambient `SqlClient` — the pg spine every
// tenancy scope binds — is never rebound to a foreign engine to serve one ingress relation.
const _ingress = <M extends Model.AnyNoContext, const S extends Query.Ingress<Query.Foreign, unknown, Query.ResolverRows>>(
  model: M,
  spec: S,
): Effect.Effect<Query.Bound<M, S>, never, Query.Client<S>> =>
  Effect.gen(function* () {
    const sql = yield* spec.client
    return {
      model,
      relation: spec.relation,
      reads: spec.reads(sql),
      resolvers: yield* Effect.all(spec.resolvers(sql), { concurrency: "unbounded" }),
    }
  })

// One entrypoint, one overload set over the discriminated spec: the `client` slot is the whole discriminant, each
// signature answers the product its own case earns, and the implementation is a single terminal two-way probe.
function _table<M extends Model.AnyNoContext, const S extends Query.Native<M, Query.Column<M>, unknown, Query.ResolverRows>>(
  model: M,
  spec: S,
): Effect.Effect<Query.Bound<M, S>, never, SqlClient.SqlClient | Scope.Scope>
function _table<M extends Model.AnyNoContext, const S extends Query.Ingress<Query.Foreign, unknown, Query.ResolverRows>>(
  model: M,
  spec: S,
): Effect.Effect<Query.Bound<M, S>, never, Query.Client<S>>
function _table<M extends Model.AnyNoContext>(model: M, spec: Query.Spec<M>) {
  return "client" in spec ? _ingress(model, spec) : _native(model, spec)
}

const Query = {
  Relation: _Relation,
  table: _table,
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Query }
```

## [06]-[FOREIGN_RELATIONAL_READS]

- Owner: the ingress case of `[05]`'s binding — the two admitted foreign driver Tags, their composition-root admission, the structural read-only boundary, SQL Server's typed-parameter and stored-procedure rows, and the dialect fork an ingress statement composes. `[05]` owns the spec family and the product projection; this cluster owns what the ingress case may reach.
- Cases: `MysqlClient.MysqlClient` reads an enterprise MySQL an app already owns; `MssqlClient.MssqlClient` reads SQL Server and admits `param` and `call` beyond that. Both satisfy `[03]`'s and `[04]`'s decode law unchanged — `SqlSchema` and `SqlResolver` compose on any client, so the read surface is the folder's, never a per-engine family.
- Packages: `@effect/sql-mysql2` (`MysqlClient.MysqlClient`, `MysqlClient.layerConfig`, `MysqlClient.MysqlClientConfig`); `@effect/sql-mssql` (`MssqlClient.MssqlClient`, `MssqlClient.layerConfig`, `MssqlClient.param`, `MssqlClient.call`, `MssqlClient.defaultParameterTypes`, `Procedure.make`, `Procedure.param`, `Procedure.outputParam`, `Procedure.withRows`, `Procedure.compile`, `Procedure.Result`, `Parameter.make`, `MssqlTypes`); `@effect/sql` (`SqlSchema.findAll`, `SqlResolver.grouped`, `sql.onDialect`, `sql.onDialectOrElse`, `Statement.Fragment`, `Statement.PrimitiveKind`); `effect` (`Config`, `Duration`, `Effect`, `Schema`).
- Entry: the composition root provides `MysqlClient.layerConfig(...)` or `MssqlClient.layerConfig(...)` over a `Config.Config.Wrap` of the driver config; an owning service then constructs `Query.table(Row, { relation, client: MssqlClient.MssqlClient, reads, resolvers })` and publishes the read verbs. `MssqlTypes` resolves through the package root alone — the distribution ships no `MssqlTypes` subpath.
- Law: a foreign engine admits at the composition ROOT and nowhere else. Each `layerConfig` Layer publishes its concrete Tag beside `SqlClient.SqlClient`, so the Layer is provided outside every tenancy scope and reaches this page only as the ingress spec's explicit `client`; the ambient `SqlClient` Tag stays the pg spine, and rebinding it to a foreign engine silently re-points every native binding in the same scope.
- Law: ingress is read-only, and the ruling is the reason — neither driver's statement arm carries this folder's tenancy GUC, so a write path drops isolation. Ingress product mints no repository and no loaders, so no write verb exists to call and the refusal is structural rather than disciplinary. Reads therefore cross tenant-unscoped and the consuming flow prices that at its own boundary, and a foreign fact becomes durable truth only through `journal/append.md`'s append on the pg spine.
- Law: neither foreign driver ships a pool-adoption row — `layer`, `layerConfig`, `make`, and `makeCompiler` are the entire construction surface, so no counterpart to `lane/postgres.md`'s `Pg.fromPool` exists. Each foreign lane therefore runs ONE pool per composition, and the per-tenant fan-out `PgClient.layerFromPool` gives the spine costs a whole pool per Layer here; an ingress read that wants tenant separation partitions in its predicate, never in its pool.
- Law: T-SQL's own shape rides the mssql case as typed rows — `MssqlClient.param(type, value, options?)` binds a `MssqlTypes` `DataType` onto a value and answers a `Statement.Fragment` the template splices, so a scalar crossing into T-SQL carries its declared type instead of the compiler's inference off the runtime value; `MssqlClient.defaultParameterTypes` is the `Record<Statement.PrimitiveKind, DataType>` that inference falls back to, and `MssqlClientConfig.parameterTypes` REPLACES that map whole rather than merging into it — a partial override drops every kind it omits, so a lane wanting one different binding states the `param` at the value instead.
- Law: a stored procedure is a `Pipeable` accretion, never a call-site string — `Procedure.make(name)` seeds, `Procedure.param<A>()(name, type, options?)` and `Procedure.outputParam<A>()(...)` widen the input and output records in the type, `Procedure.withRows<A>()` names the result-set element, `Procedure.compile(self)(input)` binds one concrete input record, and `sql.call` runs it for a `Procedure.Result<O, A>` carrying the decoded `output` record beside `rows`.
- Law: `withRows` is a PHANTOM the wire never proved — the builder re-types the result set and decodes nothing against it — so a procedure's `rows` and its `output` scalars re-enter domain code through the page's own `Schema` exactly as `SqlSchema` proves a `Connection.Row`; taking the phantom as evidence is the untyped read `[03]` deletes, wearing a type.
- Law: a statement forking on engine composes `sql.onDialectOrElse` — `orElse` carries the shared form and the `mysql`/`mssql` arms are optional, so a fork names only the engines it meets. Bare `sql.onDialect` is total over all five arms (`sqlite`, `pg`, `mysql`, `mssql`, `clickhouse`), so an ingress statement written through it must spell three bodies no foreign client will ever compile.
- Growth: a third foreign engine is one spec-case row — one member on `Query.Foreign`, one `layerConfig` row at the root, one optional arm wherever a statement already forks; the entrypoint, the product, and the read law are untouched.
- Boundary: which foreign Layer composes where is the composition root's; folding an ingress read into durable truth is `journal/append.md`'s; the driver's own construction and config surface is the `.api` catalogues'. This cluster mints no DDL and grants no write.

```typescript signature
import { Config, Duration, Effect, Schema } from "effect"
import { Model, SqlClient, SqlResolver, SqlSchema } from "@effect/sql"
import { MssqlClient, MssqlTypes, Procedure } from "@effect/sql-mssql"
import { MysqlClient } from "@effect/sql-mysql2"
import { Query } from "./query.ts"

const _ident = Schema.decodeSync(Query.Relation.fields.table)

// Modelled for ROW TYPING alone: no origin family, no temporal stamp, no insert variant any consumer reaches,
// because the ingress product mints neither repository nor loaders to consume one. Both drivers hand a datetime
// column back as a JS `Date`, so the temporal column admits through `DateTimeUtcFromDate` and no `Date` survives
// past the model boundary.
class _Invoice extends Model.Class<_Invoice>("Ingress.Invoice")({
  invoice_no: Schema.NonEmptyString,
  account: Schema.NonEmptyString,
  amount_cents: Schema.Number.pipe(Schema.int()),
  posted_on: Schema.DateTimeUtcFromDate,
}) {}

// Typed against the NEUTRAL client, so one statement definition serves both ingress rows: a parameter position
// accepts any supertype of the concrete Tag, and the dialect fork states only the arms this lane can meet.
const _reads = (sql: SqlClient.SqlClient) => ({
  ledger: SqlSchema.findAll({
    Request: Schema.Struct({
      account: Schema.NonEmptyString,
      take: Schema.Number.pipe(Schema.int(), Schema.between(1, 500)),
    }),
    Result: _Invoice,
    execute: (window) =>
      sql.onDialectOrElse({
        orElse: () =>
          sql`SELECT invoice_no, account, amount_cents, posted_on FROM invoice
              WHERE account = ${window.account} ORDER BY posted_on DESC LIMIT ${window.take}`,
        mssql: () =>
          sql`SELECT TOP (${window.take}) invoice_no, account, amount_cents, posted_on FROM invoice
              WHERE account = ${window.account} ORDER BY posted_on DESC`,
      }),
  }),
})

// `window` on the relation drives the resolver geometry alone on an ingress row — no loader exists to batch —
// while `spanPrefix` still names the lane every ingress span lands under.
const _resolvers = (sql: SqlClient.SqlClient) => ({
  byAccount: SqlResolver.grouped("IngressInvoices", {
    Request: Schema.NonEmptyString,
    RequestGroupKey: (account) => account,
    Result: _Invoice,
    ResultGroupKey: (row) => row.account,
    execute: (accounts) =>
      sql`SELECT invoice_no, account, amount_cents, posted_on FROM invoice
          WHERE ${sql.in("account", accounts)} ORDER BY account, posted_on DESC`,
  }),
})

// T-SQL scalars carry their DECLARED type across the wire: `param` answers a Fragment the template splices, so the
// compiler never infers `NVarChar` width or an integer kind off the runtime value at the edge of a foreign index.
const _aged = (sql: MssqlClient.MssqlClient) =>
  SqlSchema.findAll({
    Request: Schema.Struct({ account: Schema.NonEmptyString, days: Schema.Number.pipe(Schema.int()) }),
    Result: _Invoice,
    execute: (window) =>
      sql`SELECT invoice_no, account, amount_cents, posted_on FROM invoice
          WHERE account = ${sql.param(MssqlTypes.NVarChar, window.account, { length: 64 })}
            AND posted_on < DATEADD(day, -${sql.param(MssqlTypes.Int, window.days)}, SYSUTCDATETIME())`,
  })

const _reconcile = Procedure.make("dbo.reconcile_account").pipe(
  Procedure.param<string>()("account", MssqlTypes.NVarChar, { length: 64 }),
  Procedure.param<number>()("through_days", MssqlTypes.Int),
  Procedure.outputParam<number>()("unmatched", MssqlTypes.Int),
  Procedure.withRows<{ readonly invoice_no: string; readonly amount_cents: number }>(),
)

// `withRows` re-types the result set and proves nothing, so the procedure's own answer decodes here exactly as a
// `Connection.Row` does — output scalars and rows through one schema, `ParseError` on the same admission rail.
const _Reconciled = Schema.Struct({
  unmatched: Schema.Number.pipe(Schema.int(), Schema.nonNegative()),
  rows: Schema.Array(Schema.Struct({
    invoice_no: Schema.NonEmptyString,
    amount_cents: Schema.Number.pipe(Schema.int()),
  })),
})

const _reconciled = (sql: MssqlClient.MssqlClient) =>
(input: { readonly account: string; readonly through_days: number }) =>
  Effect.flatMap(
    sql.call(Procedure.compile(_reconcile)(input)),
    (result) => Schema.decodeUnknown(_Reconciled)({ unmatched: result.output.unmatched, rows: result.rows }),
  )

// Both ingress rows on one entrypoint: the engine arrives as its own Tag, no `id` and no `ensure` exists to give,
// and the bound value carries `reads` and `resolvers` alone — `.repository` is a compile error, not a refusal.
const _mysqlInvoices = (window: Duration.Duration) =>
  Query.table(_Invoice, {
    relation: new Query.Relation({ table: _ident("invoice"), spanPrefix: "ingress.mysql", window }),
    client: MysqlClient.MysqlClient,
    reads: _reads,
    resolvers: _resolvers,
  })

const _mssqlInvoices = (window: Duration.Duration) =>
  Query.table(_Invoice, {
    relation: new Query.Relation({ table: _ident("invoice"), spanPrefix: "ingress.mssql", window }),
    client: MssqlClient.MssqlClient,
    // SQL Server's own capability lands as rows BESIDE the shared reads, not as a second binding for one engine
    reads: (sql: MssqlClient.MssqlClient) => ({ ..._reads(sql), aged: _aged(sql), reconciled: _reconciled(sql) }),
    resolvers: _resolvers,
  })

// `layerConfig` publishes `MysqlClient` beside `SqlClient`, so the composition root provides this Layer outside
// every tenancy scope where a native binding reads that ambient Tag. No pool-adoption entrypoint exists on either
// foreign driver, so this Layer IS the lane's one pool.
const _mysqlIngress = MysqlClient.layerConfig({
  url: Config.redacted("INGRESS_MYSQL_URL"),
  maxConnections: Config.integer("INGRESS_MYSQL_POOL"),
  connectionTTL: Config.duration("INGRESS_MYSQL_TTL"),
})
```

## [07]-[ORGANIZATION_ROWS]

- Owner: model organization as read-side relations — `Organization.Entity` the addressed entity row carrying label, sibling ordinal, resolved visibility and locking, and its container address; `organization_member` and `organization_view` the two one-to-many edge relations reached through grouped resolvers. `Organization.rows` binds the entity relation through `Query.table` and settles both edge resolvers beside it.
- Packages: `@effect/sql` (`Model.Class`, `Model.FieldOption`, `Model.BooleanFromNumber`, `Model.DateTimeInsert`, `SqlResolver.grouped`, `SqlSchema.findAll`); `effect` (`Schema`, `Duration`); `lane/capability.md` (`Capability.Ensure`).
- Entry: `Organization.rows(window)` inside the owning service build; callers reach entities through the bound repository and the four grouped resolvers, so a subtree walk collapses into one statement window per level.
- Law: `address` is the ENTITY key and `member` a FEDERATION key the producing authority issued, so the two never share a column. Nesting rides `container` on the entity row because an entity has exactly one container, while membership and view overrides are the genuine one-to-many axes and earn their own relations.
- Law: content-key columns carry the lowercase hex face this branch already reads, so a join against any peer's address lowers and never uppercases; the producer's 16 big-endian bytes lower exactly once, at the core landing this page consumes.
- Law: sibling rank is the producer's DENSE `ordinal`, so `ORDER BY ordinal` reproduces the source order without a second comparison, and no client re-breaks a tie the producer already resolved.
- Law: the edge relations take NO repository — they carry no independent identity and mutate only through the organization lane's whole-source replacement, which is exactly the posture the journal relations hold.
- Boundary: rows arrive decoded from the wire and this page derives nothing — no address minted, no ordinal recomputed, no container inferred from a label chain, and no host handle anywhere in the schema.
- Growth: one appended wire field is one column here and one row in the lane's projection; a new containment relation is one `kind` value beside one resolver row.

```typescript signature
import { Duration, Schema } from "effect"
import { Model, SqlClient, SqlResolver, SqlSchema } from "@effect/sql"
import type { Capability } from "../lane/capability.ts"
import { Query } from "./query.ts"

// Identifier evidence derives from the relation owner over page-authored literals, so all three names are total by
// construction and no caller-derived string reaches an identifier position.
const _ident = Schema.decodeSync(Query.Relation.fields.table)
const _ENTITY = _ident("organization_entity")
const _MEMBER = _ident("organization_member")
const _VIEW = _ident("organization_view")

const _Address = Schema.NonEmptyString.pipe(Schema.pattern(/^[0-9a-f]{32}$/), Schema.brand("OrgAddress"))

class _Entity extends Model.Class<_Entity>("Organization.Entity")({
  address: _Address,
  source: _Address,
  authority: Schema.NonEmptyString,
  name: Schema.NonEmptyString,
  ordinal: Schema.Number.pipe(Schema.int(), Schema.nonNegative()),
  container: Model.FieldOption(_Address),
  visible: Model.BooleanFromNumber,
  locked: Model.BooleanFromNumber,
  folded_at: Model.DateTimeInsert,
}) {}

// Contained children re-anchor on the class's own resolved `.fields` (`Model.fields` yields variant Field objects
// `Schema.Struct` refuses) with `container` re-typed non-optional, because the statement's own predicate proves
// presence: reading the model's `Option` here forces a group key to unwrap a value the WHERE clause already
// guaranteed, and unwrapping by throw is the exception path this engine deletes.
const _Child = Schema.Struct({ ..._Entity.fields, container: _Address })

// Edge relations carry no independent identity, so they take resolvers and no repository: an organization read
// replaces a source's whole edge set, never one row. `member` and `view` stay unbranded strings because the
// producing authority issues them — branding either here claims a grammar no peer promised.
const _resolverRows = (sql: SqlClient.SqlClient) => ({
  // one grouped row answers a whole level, so a depth-first walk pays one statement window per level, never per parent
  children: SqlResolver.grouped("OrganizationChildren", {
    Request: _Address,
    RequestGroupKey: (address) => address,
    Result: _Child,
    ResultGroupKey: (row) => row.container,
    execute: (containers) =>
      sql`SELECT * FROM ${sql(_ENTITY)} WHERE ${sql.in("container", containers)} ORDER BY container, ordinal`,
  }),
  roots: SqlResolver.grouped("OrganizationRoots", {
    Request: _Address,
    RequestGroupKey: (source) => source,
    Result: _Entity,
    ResultGroupKey: (row) => row.source,
    execute: (sources) =>
      sql`SELECT * FROM ${sql(_ENTITY)} WHERE ${sql.in("source", sources)} AND container IS NULL ORDER BY ordinal`,
  }),
  members: SqlResolver.grouped("OrganizationMembers", {
    Request: _Address,
    RequestGroupKey: (address) => address,
    Result: Schema.Struct({ address: _Address, member: Schema.NonEmptyString }),
    ResultGroupKey: (row) => row.address,
    execute: (addresses) =>
      sql`SELECT address, member FROM ${sql(_MEMBER)} WHERE ${sql.in("address", addresses)} ORDER BY member`,
  }),
  views: SqlResolver.grouped("OrganizationViews", {
    Request: _Address,
    RequestGroupKey: (address) => address,
    Result: Schema.Struct({ address: _Address, view: Schema.NonEmptyString, visible: Model.BooleanFromNumber }),
    ResultGroupKey: (row) => row.address,
    execute: (addresses) =>
      sql`SELECT address, view, visible FROM ${sql(_VIEW)} WHERE ${sql.in("address", addresses)}`,
  }),
})

// Whole-source roster: the lane replaces a source's projection as one unit, so its diff reads every address the
// prior fold landed rather than probing per entity.
const _reads = (sql: SqlClient.SqlClient) => ({
  roster: SqlSchema.findAll({
    Request: _Address,
    Result: Schema.Struct({ address: _Address }),
    execute: (source) => sql`SELECT address FROM ${sql(_ENTITY)} WHERE source = ${source}`,
  }),
})

const _ddl: Capability.Ensure = {
  relation: _ENTITY,
  pg: `CREATE TABLE IF NOT EXISTS organization_entity (
    address TEXT PRIMARY KEY, source TEXT NOT NULL, authority TEXT NOT NULL, name TEXT NOT NULL,
    ordinal INTEGER NOT NULL, container TEXT, visible BOOLEAN NOT NULL, locked BOOLEAN NOT NULL,
    folded_at TIMESTAMPTZ NOT NULL DEFAULT now());
  CREATE INDEX IF NOT EXISTS organization_entity_container ON organization_entity (container, ordinal);
  CREATE TABLE IF NOT EXISTS organization_member (
    address TEXT NOT NULL, member TEXT NOT NULL, PRIMARY KEY (address, member));
  CREATE TABLE IF NOT EXISTS organization_view (
    address TEXT NOT NULL, view TEXT NOT NULL, visible BOOLEAN NOT NULL, PRIMARY KEY (address, view));`,
  sqlite: `CREATE TABLE IF NOT EXISTS organization_entity (
    address TEXT PRIMARY KEY, source TEXT NOT NULL, authority TEXT NOT NULL, name TEXT NOT NULL,
    ordinal INTEGER NOT NULL, container TEXT, visible INTEGER NOT NULL, locked INTEGER NOT NULL,
    folded_at TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now')));
  CREATE INDEX IF NOT EXISTS organization_entity_container ON organization_entity (container, ordinal);
  CREATE TABLE IF NOT EXISTS organization_member (
    address TEXT NOT NULL, member TEXT NOT NULL, PRIMARY KEY (address, member));
  CREATE TABLE IF NOT EXISTS organization_view (
    address TEXT NOT NULL, view TEXT NOT NULL, visible INTEGER NOT NULL, PRIMARY KEY (address, view));`,
}

const _rows = (window: Duration.Duration) =>
  Query.table(_Entity, {
    relation: new Query.Relation({ table: _ENTITY, spanPrefix: "organization", window }),
    id: "address",
    ensure: _ddl,
    reads: _reads,
    resolvers: _resolverRows,
  })

const Organization = { Entity: _Entity, Address: _Address, rows: _rows } as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Organization }
```

## [08]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
