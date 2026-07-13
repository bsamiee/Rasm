# [DATA_CAPABILITY]

The fail-closed capability rail of the folder: one closed row/ensure vocabulary, one `Capability` service that proves every extension row, every declared relation, and every dependency demand at Layer construction through two roster-batched probes, and one fault family for everything a probe refuses. Nothing in the folder assumes an extension, a version floor, or a relation — presence is proven or the capability does not exist, and the granted set is a value siblings gate on: `require` fails typed, `when` degrades to `Option`. Extension proof is one `RequestResolver`-batched dialect-honest catalog lookup; relation proof is one schema-qualified census over the complete ensure roster. The same rail carries the DDL split: the provisioning plane applies the idempotent ensure rows, this service proves them at startup, runtime never mutates schema.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                                                  |
| :-----: | :--------------- | :--------------------------------------------------------------------------------------- |
|  [01]   | `ROW_VOCABULARY` | the row, ensure, and demand shapes, the version-order fold, the one fault family        |
|  [02]   | `BATCHED_PROBE`  | the probe request family and the one-statement `RequestResolver` over the roster        |
|  [03]   | `PROBE_SERVICE`  | the `Capability` service — construction-time proof, granted set, `require`/`when` gates |

## [02]-[ROW_VOCABULARY]

- Owner: `Capability.Row<G, F>` — the structural bound every `lane/postgres.md` matrix row inhabits, parameterized by its value-derived grant and flag vocabularies; `Capability.Ensure` — the `{relation, pg, sqlite}` DDL row every table-owning page publishes as data; `Capability.Demand<F, G>` — the `[flag, grant]` dependency pair; the segment-numeric version order; the one reason-discriminated fault.
- Packages: `effect` (`Data`, `Order`, `String`).
- Growth: a new probe posture is a `probeSql` override on the owning matrix row; a new ensure dialect is one field on the ensure shape; a new dependency edge is one demand pair — the shapes never widen per extension.
- Law: `floor` compares segment-numeric through `_byVersion`; `"0.0.0"` is the presence-only floor — the probe still fails closed on absence. The floor gate itself is a pg fact: the sqlite arm answers presence only, so its admission is membership and no pg-authored floor can refuse a present module.
- Law: ensure rows are authored by the page owning the relation and collected by `lane/tenant.md` at Layer construction; demand pairs are authored by the page owning the matrix (`Pg.demands`). `G` and `F` infer from those values through the service factory, so `require` and `when` accept only the roster's closed grant vocabulary and a misspelled gate is unrepresentable.
- Fault: one `CapabilityFault` family, reason-discriminated — `absent` (extension not installed), `floor` (installed below floor), `schema` (declared relation missing), `requires` (dependency grant refused); all four route identically (fail or shrink at startup, repair provision), so one class carries them and no policy table is earned.

```typescript
import { Data, Order, String, pipe } from "effect"

declare namespace Capability {
  type Row<G extends string = string, F extends string = string> = {
    readonly extension: string
    readonly floor: string
    readonly probeSql?: string
    readonly capabilities: ReadonlyArray<G>
    readonly layer: "image" | "core"
    readonly flags?: ReadonlyArray<F>
  }
  type Ensure = {
    readonly relation: string
    readonly pg: string
    readonly sqlite: string
  }
  type Demand<F extends string = string, G extends string = string> = readonly [flag: F, grant: G]
  type Reason = "absent" | "floor" | "schema" | "requires"
  type Fault = CapabilityFault
}

class CapabilityFault extends Data.TaggedError("CapabilityFault")<{
  readonly reason: Capability.Reason
  readonly subject: string
  readonly detail: string
}> {}

const _segments = (version: string): ReadonlyArray<number> =>
  pipe(version, String.split("."), (parts) => parts.map((part) => Number.parseInt(part, 10) || 0))

const _byVersion: Order.Order<string> = Order.mapInput(Order.array(Order.number), _segments)

const _meets = (installed: string, floor: string): boolean =>
  Order.greaterThanOrEqualTo(_byVersion)(installed, floor)
```

## [03]-[BATCHED_PROBE]

- Owner: the `_Probe` request class and `_probeResolver` — one `RequestResolver.makeBatched` that folds every extension lookup queued in a construction pass into a single decoded catalog scan keyed by `sql.in`, resolving each request to the installed version as `Option`; the statement is dialect-honest through `sql.onDialectOrElse` inside the one execute.
- Packages: `effect` (`Effect`, `Option`, `Request`, `RequestResolver`); `@effect/sql` (`SqlClient`, `SqlSchema`, `sql.in`, `sql.onDialectOrElse`).
- Entry: `[4]`'s construction issues `Effect.request(new _Probe({...}), resolver)` per matrix row under `{ batching: true }`, so the whole roster costs one round trip; a sealed per-row `probeSql` override routes through `sql.unsafe` and `SqlSchema.findOne` only when the row declares an exotic probe, with `{ version }` as its required result shape.
- Growth: a new probed catalog (a settings scan, a schema census) is a second request class over the same resolver pattern — the batching law is settled here once.
- Law: the spine arm is `SELECT extname, extversion FROM pg_extension WHERE extname IN (…)`; the sqlite arm scans `pragma_module_list` for the registered module names a runtime `loadExtension` installed, answering the presence-only floor `'0.0.0'` because version floors are pg facts — absent names simply return no row on either arm, and the resolver completes those requests with `Option.none`; refusal semantics live in the fold, never in the statement.
- Law: a statement failure is NOT absence — the window settles every request with the `SqlError` itself, so a broken connection fails Layer construction typed instead of silently shrinking the granted set; fail-closed refuses capabilities on evidence, never on transport accident.
- Law: `sql.unsafe` never meets the batch — the batched form is fully parameterized through `sql.in` and decodes through the one `SqlSchema` scan (the folder's typed-read law holds even on catalog tables); the unsafe escape exists only for sealed per-row overrides, its `{ version }` result still decodes through `SqlSchema`, and the override text is a row of the sealed matrix, never caller input.

```typescript
import { Effect, HashMap, Option, Request, RequestResolver, Schema } from "effect"
import type { ParseResult } from "effect"
import { SqlClient, SqlSchema, type SqlError } from "@effect/sql"

class _Probe extends Request.Class<Option.Option<string>, SqlError.SqlError | ParseResult.ParseError, {
  readonly extension: string
}> {}

const _Installed = Schema.Struct({ extname: Schema.String, extversion: Schema.String })

const _probeResolver = (sql: SqlClient.SqlClient): RequestResolver.RequestResolver<_Probe> => {
  const scan = SqlSchema.findAll({
    Request: Schema.Array(Schema.String),
    Result: _Installed,
    execute: (names) =>
      sql.onDialectOrElse({
        orElse: () => sql`SELECT name AS extname, '0.0.0' AS extversion FROM pragma_module_list WHERE ${sql.in("name", names)}`,
        pg: () => sql`SELECT extname, extversion FROM pg_extension WHERE ${sql.in("extname", names)}`,
      }),
  })
  return RequestResolver.makeBatched((requests: ReadonlyArray<_Probe>) =>
    Effect.matchEffect(scan(requests.map((request) => request.extension)), {
      onFailure: (fault) =>
        Effect.forEach(requests, (request) => Request.fail(request, fault), { discard: true }),
      onSuccess: (rows) => {
        const found = HashMap.fromIterable(rows.map((row) => [row.extname, row.extversion] as const))
        return Effect.forEach(
          requests,
          (request) => Request.succeed(request, HashMap.get(found, request.extension)),
          { discard: true },
        )
      },
    }))
}
```

## [04]-[PROBE_SERVICE]

- Owner: the `Capability` service — one `Effect.Service` whose parameterized `Default(build)` Layer factory batch-probes the extension roster, scans the complete ensure roster at the build's physical schema coordinate in one statement, seeds the granted set with the caller's dialect core grants, computes the dependency closure, and publishes the closed granted set, the installed-version report, and the refused evidence.
- Packages: `effect` (`Effect.Service`, `HashMap`, `HashSet`, `Array`, `Either`, `Option`); `@effect/sql` (`SqlClient`, `sql.onDialectOrElse`); `lane/postgres.md` (`Pg.rows`, `Pg.core`, `Pg.demands` arrive as arguments, never imports — the service is roster-agnostic).
- Entry: `Capability.Default({ rows: Pg.rows, ensures, core: Pg.core.pg, demands: Pg.demands, schema: locus.schema })` composes under every scope in `lane/tenant.md`; sqlite profiles pass their dialect core keys, an empty demand set, and `main`, so `require("channel")` reads one vocabulary and an unprovisioned scope fails at the Layer, never at first query.
- Receipt: `Capability.Report` — `granted`, `versions`, `refused` — the typed probe evidence startup logging and the fact journal consume; a tally beside it restates what the report carries.
- Growth: a consumer needing a new gate composes `require`/`when`, never a second probe path; a new dialect's relation probe is one `onDialectOrElse` arm.
- Law: probes are fail-closed — an absent row is refused, never assumed; a refused ensure relation fails Layer construction (the DDL split was violated), a refused extension row only shrinks the granted set and consumers degrade through `when`.
- Law: demands compute a least fixed point from core grants — each pass admits only rows whose demanded grants already exist in the accepted set, then widens that set with their capabilities; convergence refuses unresolved and cyclic rows with reason `requires`, so one starved row can never satisfy another row transiently. The pair table arrives as data, and the deploy plane's image derivation reads the same pairs at provision.
- Law: `require` is the hard gate (`Effect<void, CapabilityFault>`), `when` the soft gate (refusal reifies as `Option.none`); a boolean read of the granted set outside these two members is the smuggled knob.
- Law: probing runs once per Layer construction and the report is immutable thereafter — a live re-probe is scope invalidation on the owning `Stores` map, never a poll.
- Boundary: which rows exist is `lane/postgres.md`'s; where the Layer composes and which ensures collect is `lane/tenant.md`'s; the image that makes `"image"` rows probe true is the deployment plane's.

```typescript
import { Array, Either, HashSet } from "effect"

class Capability extends Effect.Service<Capability>()("data/Capability", {
  effect: <G extends string, F extends string>(build: {
    readonly rows: ReadonlyArray<Capability.Row<G, F>>
    readonly ensures: ReadonlyArray<Capability.Ensure>
    readonly core: ReadonlyArray<G>
    readonly demands: ReadonlyArray<Capability.Demand<F, G>>
    readonly schema: string
  }) =>
    Effect.gen(function* () {
      const { core, demands, ensures, rows, schema } = build
      const sql = yield* SqlClient.SqlClient
      const resolver = _probeResolver(sql)
      const relations = SqlSchema.findAll({
        Request: Schema.Struct({ schema: Schema.String, relations: Schema.Array(Schema.String) }),
        Result: Schema.Struct({ relation: Schema.String }),
        execute: (coordinate) =>
          sql.onDialectOrElse({
            orElse: () => sql`SELECT name AS relation FROM sqlite_master WHERE ${sql.in("name", coordinate.relations)}`,
            pg: () => sql`SELECT table_name AS relation FROM information_schema.tables
                          WHERE table_schema = ${coordinate.schema} AND ${sql.in("table_name", coordinate.relations)}`,
          }),
      })
      const probed = yield* Effect.forEach(
        rows,
        (row) =>
          Effect.map(
            row.probeSql === undefined
              ? Effect.request(new _Probe({ extension: row.extension }), resolver)
              : SqlSchema.findOne({
                  Request: Schema.Void,
                  Result: Schema.Struct({ version: Schema.String }),
                  execute: () => sql.unsafe(row.probeSql),
                })(undefined).pipe(Effect.map(Option.map((found) => found.version))),
            (version) => [row, version] as const,
          ),
        { batching: true },
      )
      const present = yield* (ensures.length === 0
        ? Effect.succeed(HashSet.empty<string>())
        : Effect.map(
            relations({ schema, relations: Array.map(ensures, (ensure) => ensure.relation) }),
            (found) => HashSet.fromIterable(Array.map(found, (row) => row.relation)),
          ))
      const absent = Array.filter(ensures, (ensure) => !HashSet.has(present, ensure.relation))
      yield* Effect.when(
        Effect.fail(new CapabilityFault({
          reason: "schema",
          subject: Array.map(absent, (ensure) => ensure.relation).join(","),
          detail: Array.map(absent, (ensure) =>
            sql.onDialectOrElse({ orElse: () => ensure.sqlite, pg: () => ensure.pg })).join("\n"),
        })),
        () => Array.isNonEmptyReadonlyArray(absent),
      )
      const floored = sql.onDialectOrElse({ orElse: () => false, pg: () => true }) // version floors are pg facts: the sqlite arm admits present modules on membership alone
      const [missing, held] = Array.partitionMap(probed, ([row, version]) =>
        Option.match(version, {
          onNone: () =>
            Either.left(new CapabilityFault({ reason: "absent", subject: row.extension, detail: row.floor })),
          onSome: (installed) =>
            !floored || _meets(installed, row.floor)
              ? Either.right([row, installed] as const)
              : Either.left(new CapabilityFault({ reason: "floor", subject: row.extension, detail: installed })),
        }))
      const resolve = (
        accepted: typeof held,
        pending: typeof held,
      ): readonly [accepted: typeof held, refused: ReadonlyArray<CapabilityFault>] => {
        const available = HashSet.union(
          HashSet.fromIterable(core),
          HashSet.fromIterable(Array.flatMap(accepted, ([row]) => row.capabilities)),
        )
        const [waiting, ready] = Array.partitionMap(pending, (entry) => {
          const [row] = entry
          const unmet = Array.findFirst(demands, ([flag, grant]) =>
            Array.contains(row.flags ?? [], flag) && !HashSet.has(available, grant))
          return Option.isNone(unmet) ? Either.right(entry) : Either.left(entry)
        })
        return ready.length > 0
          ? resolve([...accepted, ...ready], waiting)
          : [
              accepted,
              Array.map(waiting, ([row]) => {
                const demand = Array.findFirst(demands, ([flag, grant]) =>
                  Array.contains(row.flags ?? [], flag) && !HashSet.has(available, grant))
                return new CapabilityFault({
                  reason: "requires",
                  subject: row.extension,
                  detail: Option.match(demand, { onNone: () => "<cycle>", onSome: ([, grant]) => grant }),
                })
              }),
            ]
      }
      const [granted, starved] = resolve([], held)
      const report: Capability.Report<G> = {
        granted: HashSet.union(
          HashSet.fromIterable(core),
          HashSet.fromIterable(Array.flatMap(granted, ([row]) => row.capabilities)),
        ),
        versions: HashMap.fromIterable(Array.map(granted, ([row, installed]) => [row.extension, installed] as const)),
        refused: [...missing, ...starved],
      }
      return {
        report,
        granted: report.granted,
        require: (key: G): Effect.Effect<void, CapabilityFault> =>
          HashSet.has(report.granted, key)
            ? Effect.void
            : Effect.fail(new CapabilityFault({ reason: "absent", subject: key, detail: "<ungranted>" })),
        when: <A, E, R>(key: G, effect: Effect.Effect<A, E, R>): Effect.Effect<Option.Option<A>, E, R> =>
          Effect.when(effect, () => HashSet.has(report.granted, key)),
      }
    }),
}) {}

declare namespace Capability {
  type Report<G extends string = string> = {
    readonly granted: HashSet.HashSet<G>
    readonly versions: HashMap.HashMap<string, string>
    readonly refused: ReadonlyArray<CapabilityFault>
  }
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Capability, CapabilityFault }
```
