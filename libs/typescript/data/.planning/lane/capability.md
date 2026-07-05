# [DATA_CAPABILITY]

The fail-closed capability rail of the folder: one closed row/ensure vocabulary, one `Capability` service that proves every extension row and every declared relation at Layer construction through request-batched probes, and one fault family for everything a probe refuses. Nothing in the folder assumes an extension, a version floor, or a relation — presence is proven or the capability does not exist, and the granted set is a value siblings gate on: `require` fails typed, `when` degrades to `Option`. The extension probes collapse to ONE statement per construction — a `RequestResolver`-batched lookup over the whole roster — and the same rail carries the DDL split: the provisioning plane applies the idempotent ensure rows, this service proves them at startup, runtime never mutates schema.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                                                |
| :-----: | :--------------- | :-------------------------------------------------------------------------------------- |
|  [01]   | `ROW_VOCABULARY` | the row and ensure shapes, the version-order fold, the one fault family                   |
|  [02]   | `BATCHED_PROBE`  | the probe request family and the one-statement `RequestResolver` over the roster          |
|  [03]   | `PROBE_SERVICE`  | the `Capability` service — construction-time proof, granted set, `require`/`when` gates   |

## [2]-[ROW_VOCABULARY]

- Owner: `Capability.Row` — the shape every `lane/postgres.md` matrix row inhabits, re-declared here only as the structural bound the service consumes; `Capability.Ensure` — the `{relation, pg, sqlite}` DDL row every table-owning page publishes as data; the segment-numeric version order; the one reason-discriminated fault.
- Packages: `effect` (`Data`, `Order`, `String`).
- Growth: a new probe posture is a `probeSql` override on the owning matrix row; a new ensure dialect is one field on the ensure shape — the shapes never widen per extension.
- Law: `floor` compares segment-numeric through `_byVersion`; `"0.0.0"` is the presence-only floor — the probe still fails closed on absence.
- Law: ensure rows are authored by the page owning the relation and collected by `lane/tenant.md` at Layer construction; this page owns the shape, never the roster.
- Fault: one `CapabilityFault` family, reason-discriminated — `absent` (extension not installed), `floor` (installed below floor), `schema` (declared relation missing); all three route identically (fail startup, repair provision), so one class carries them and no policy table is earned.

```typescript
import { Data, Order, String, pipe } from "effect"

declare namespace Capability {
  type Row = {
    readonly extension: string
    readonly floor: string
    readonly probeSql?: string
    readonly capabilities: ReadonlyArray<string>
    readonly layer: "image" | "core"
  }
  type Ensure = {
    readonly relation: string
    readonly pg: string
    readonly sqlite: string
  }
  type Reason = "absent" | "floor" | "schema"
  type Fault = _Fault
}

class _Fault extends Data.TaggedError("CapabilityFault")<{
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

## [3]-[BATCHED_PROBE]

- Owner: the `_Probe` request class and `_probeResolver` — one `RequestResolver.makeBatched` that folds every extension lookup queued in a construction pass into a single `pg_extension` scan keyed by `sql.in`, resolving each request to the installed version as `Option`.
- Packages: `effect` (`Effect`, `Option`, `Request`, `RequestResolver`); `@effect/sql` (`SqlClient`, `sql.in`).
- Entry: `[4]`'s construction issues `Effect.request(new _Probe({...}), resolver)` per matrix row under `{ batching: true }`, so the whole roster costs one round trip; a per-row `probeSql` override routes through `sql.unsafe` only when the row declares an exotic probe.
- Growth: a new probed catalog (a settings scan, a schema census) is a second request class over the same resolver pattern — the batching law is settled here once.
- Law: the batch statement is `SELECT extname, extversion FROM pg_extension WHERE extname IN (…)` — absent names simply return no row, and the resolver completes those requests with `Option.none`; refusal semantics live in the fold, never in the statement.
- Law: a statement failure is NOT absence — the window settles every request with the `SqlError` itself, so a broken connection fails Layer construction typed instead of silently shrinking the granted set; fail-closed refuses capabilities on evidence, never on transport accident.
- Law: `sql.unsafe` never meets the batch — the batched form is fully parameterized through `sql.in`; the unsafe escape exists only for sealed per-row overrides, and the override text is a row of the sealed matrix, never caller input.

```typescript
import { Effect, HashMap, Option, Request, RequestResolver } from "effect"
import { SqlClient, type SqlError } from "@effect/sql"

class _Probe extends Request.Class<Option.Option<string>, SqlError.SqlError, {
  readonly extension: string
}> {}

const _probeResolver = (sql: SqlClient.SqlClient): RequestResolver.RequestResolver<_Probe> =>
  RequestResolver.makeBatched((requests: ReadonlyArray<_Probe>) =>
    Effect.matchEffect(
      sql`SELECT extname, extversion FROM pg_extension WHERE ${sql.in("extname", requests.map((request) => request.extension))}`,
      {
        onFailure: (fault) =>
          Effect.forEach(requests, (request) => Request.fail(request, fault), { discard: true }),
        onSuccess: (rows) => {
          const found = HashMap.fromIterable(
            rows.map((row) => [globalThis.String(row["extname"]), globalThis.String(row["extversion"])] as const),
          )
          return Effect.forEach(
            requests,
            (request) => Request.succeed(request, HashMap.get(found, request.extension)),
            { discard: true },
          )
        },
      },
    ))
```

## [4]-[PROBE_SERVICE]

- Owner: the `Capability` service — one `Effect.Service` whose parameterized `Default(rows, ensures, core)` Layer factory batch-probes the roster, verifies every ensure relation, seeds the granted set with the caller's dialect core grants, and publishes the granted set, the installed-version report, and the refused evidence.
- Packages: `effect` (`Effect.Service`, `HashMap`, `HashSet`, `Option`); `@effect/sql` (`SqlClient`, `sql.onDialectOrElse`); `lane/postgres.md` (`Pg.rows`, `Pg.core` arrive as arguments, never imports — the service is roster-agnostic).
- Entry: `Capability.Default(Pg.rows, ensures, Pg.core.pg)` composed under every scope by `lane/tenant.md`; the sqlite profiles pass their dialect core keys, so `require("channel")` reads one vocabulary and an unprovisioned scope fails at the Layer, never at first query.
- Receipt: `Capability.Report` — `granted`, `versions`, `refused` — the typed probe evidence startup logging and the fact journal consume; a tally beside it restates what the report carries.
- Growth: a consumer needing a new gate composes `require`/`when`, never a second probe path; a new dialect's relation probe is one `onDialectOrElse` arm.
- Law: probes are fail-closed — an absent row is refused, never assumed; a refused ensure relation fails Layer construction (the DDL split was violated), a refused extension row only shrinks the granted set and consumers degrade through `when`.
- Law: `require` is the hard gate (`Effect<void, CapabilityFault>`), `when` the soft gate (refusal reifies as `Option.none`); a boolean read of the granted set outside these two members is the smuggled knob.
- Law: probing runs once per Layer construction and the report is immutable thereafter — a live re-probe is scope invalidation on the owning `Stores` map, never a poll.
- Boundary: which rows exist is `lane/postgres.md`'s; where the Layer composes and which ensures collect is `lane/tenant.md`'s; the image that makes `"image"` rows probe true is the deployment plane's.

```typescript
import { HashSet } from "effect"

class Capability extends Effect.Service<Capability>()("data/Capability", {
  effect: (rows: ReadonlyArray<Capability.Row>, ensures: ReadonlyArray<Capability.Ensure>, core: ReadonlyArray<string>) =>
    Effect.gen(function* () {
      const sql = yield* SqlClient.SqlClient
      const resolver = _probeResolver(sql)
      const probed = yield* Effect.forEach(
        rows,
        (row) =>
          Effect.map(
            row.probeSql === undefined
              ? Effect.request(new _Probe({ extension: row.extension }), resolver)
              : Effect.map(sql.unsafe(row.probeSql).values, (cells) =>
                  Option.map(Option.fromNullable(cells[0]?.[0]), globalThis.String)),
            (version) => [row, version] as const,
          ),
        { batching: true },
      )
      yield* Effect.forEach(ensures, (ensure) =>
        Effect.filterOrFail(
          sql.onDialectOrElse({
            orElse: () => sql`SELECT 1 FROM sqlite_master WHERE name = ${ensure.relation}`,
            pg: () => sql`SELECT 1 FROM information_schema.tables WHERE table_name = ${ensure.relation}`,
          }).values,
          (cells) => cells.length > 0,
          () => new _Fault({ reason: "schema", subject: ensure.relation, detail: ensure.pg }),
        ), { discard: true })
      const report: Capability.Report = probed.reduce<Capability.Report>(
        (held, [row, version]) =>
          Option.match(version, {
            onNone: () => ({
              ...held,
              refused: [...held.refused, new _Fault({ reason: "absent", subject: row.extension, detail: row.floor })],
            }),
            onSome: (installed) =>
              _meets(installed, row.floor)
                ? {
                    granted: row.capabilities.reduce((set, key) => HashSet.add(set, key), held.granted),
                    versions: HashMap.set(held.versions, row.extension, installed),
                    refused: held.refused,
                  }
                : {
                    ...held,
                    refused: [...held.refused, new _Fault({ reason: "floor", subject: row.extension, detail: installed })],
                  },
          }),
        { granted: HashSet.fromIterable(core), versions: HashMap.empty(), refused: [] },
      )
      return {
        report,
        granted: report.granted,
        require: (key: string): Effect.Effect<void, _Fault> =>
          HashSet.has(report.granted, key)
            ? Effect.void
            : Effect.fail(new _Fault({ reason: "absent", subject: key, detail: "<ungranted>" })),
        when: <A, E, R>(key: string, effect: Effect.Effect<A, E, R>): Effect.Effect<Option.Option<A>, E, R> =>
          Effect.when(effect, () => HashSet.has(report.granted, key)),
      }
    }),
}) {}

declare namespace Capability {
  type Report = {
    readonly granted: HashSet.HashSet<string>
    readonly versions: HashMap.HashMap<string, string>
    readonly refused: ReadonlyArray<_Fault>
  }
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Capability }
```
