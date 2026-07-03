# [STORE_REBUILD]

The maintenance plane of the read side, all of it capability-gated data: `pg_cron` schedule rows register rebuilds, compaction, ledger grooming, and retention drops as in-database cron facts; `pg_ivm` rows declare incrementally-maintained views where the fold is expressible as SQL — the in-database alternative to an async lane; and the rebuild fold itself re-derives any read model from the journal into a shadow table and swaps it atomically under a session advisory lock, so a schema-broken or drifted projection is repaired by replay, never by patching rows. Every row here degrades honestly: no `"cron"` grant means the host schedules (a `work` cron job through the port), no `"ivm"` grant means the async lane owns the fold — the degradation verdicts are `lane/sqlite.md` rows, consumed here through the same grant vocabulary.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                                        |
| :-----: | :--------------- | :------------------------------------------------------------------------------ |
|  [01]   | `SCHEDULED_ROWS` | the `cron.schedule` registration rows — grooming, drops, compaction as data      |
|  [02]   | `IVM_ROWS`       | the `create_immv` view rows — in-database incremental maintenance                |
|  [03]   | `REBUILD_FOLD`   | shadow-table replay, the atomic swap, the singleton session lock                 |

## [2]-[SCHEDULED_ROWS]

- Owner: the `Rebuild.schedule` registrar plus the standing schedule vocabulary — one `as const` table whose rows carry `{ name, spec, statement }`; registration is idempotent by name.
- Packages: `capability/row.md` (`Capability.require("cron")`); `@effect/sql` (`sql.unsafe` over closed-vocabulary statement literals).
- Entry: `Rebuild.schedule(rows)` runs at scope construction where the grant holds — schedules are provisioned state, verified like relations; apps never call cron directly.
- Growth: a new maintenance job is one row — the statement is the job; the spec is standard cron syntax; nothing else changes.
- Law: the standing rows read `journal/retain.md`'s policy vocabulary — ledger grooming deletes `idempotency_ledger` past its class window, outbox grooming deletes delivered rows past theirs, frontier drops execute `pg_partman` retention where `"partition"` also holds — windows are `Retain.Policy` projections, never literals in cron text.
- Law: `cron.schedule` and `create_immv` are extension SQL under `[R11]` floors — the spellings ride the granted rows and the probe gate; a refused grant means the row silently does not register and the host-schedule fallback owns the job.

```typescript
import { Duration, Effect, type ParseResult } from "effect"
import { SqlClient, type SqlError } from "@effect/sql"
import { Capability } from "../capability/row.ts"
import { Retain } from "../journal/retain.ts"

declare namespace Rebuild {
  type Job = {
    readonly name: string
    readonly spec: string
    readonly statement: string
  }
}

const _days = (window: Duration.Duration): number => Math.trunc(Duration.toMillis(window) / 86_400_000)

const _jobs: ReadonlyArray<Rebuild.Job> = [
  {
    name: "groom_ledger",
    spec: "0 4 * * *",
    statement: `DELETE FROM idempotency_ledger WHERE touched_at < now() - interval '${_days(Retain.Policy.operational.window)} days'`,
  },
  {
    name: "groom_outbox",
    spec: "30 4 * * *",
    statement: `DELETE FROM outbox WHERE delivered_at IS NOT NULL AND delivered_at < now() - interval '${_days(Retain.Policy.ephemeral.window)} days'`,
  },
]

const _schedule = (jobs: ReadonlyArray<Rebuild.Job>) =>
  Effect.flatMap(Capability, (capability) =>
    capability.when(
      "cron",
      Effect.flatMap(SqlClient.SqlClient, (sql) =>
        Effect.forEach(jobs, (job) =>
          sql`SELECT cron.schedule(${job.name}, ${job.spec}, ${job.statement})`, { discard: true })),
    ))
```

## [3]-[IVM_ROWS]

- Owner: the `Rebuild.immv` registrar — incrementally-maintained view rows for folds expressible as SQL aggregation, registered where the `"ivm"` grant holds.
- Packages: `capability/row.md` (`Capability.when("ivm", …)`).
- Entry: a read model whose fold is a SQL aggregate over journal or projection rows declares an `Immv` row instead of an async lane — the database maintains it on every commit, read-your-writes for free at pg altitude.
- Growth: a new in-database view is one row `{ name, query }`; the degradation to an async lane is automatic because the lane version of the same fold is the app's fallback value, selected by the same grant read.
- Law: IVM is an accelerator, not truth — an `immv` derives from journal rows and is always droppable and re-creatable; nothing writes to it, and consumers read it like any projection table.
- Law: registration is presence-checked — `create_immv` runs only when `pg_ivm`'s catalog lacks the view, so scope construction stays idempotent under the ensure law.

```typescript
declare namespace Rebuild {
  type Immv = {
    readonly name: string
    readonly query: string
  }
}

const _immv = (views: ReadonlyArray<Rebuild.Immv>) =>
  Effect.flatMap(Capability, (capability) =>
    capability.when(
      "ivm",
      Effect.flatMap(SqlClient.SqlClient, (sql) =>
        Effect.forEach(views, (view) =>
          Effect.flatMap(
            sql`SELECT 1 FROM pg_class WHERE relname = ${view.name}`.values,
            (held) => held.length > 0 ? Effect.void : sql`SELECT create_immv(${view.name}, ${view.query})`.pipe(Effect.asVoid),
          ), { discard: true })),
    ))
```

## [4]-[REBUILD_FOLD]

- Owner: `Rebuild.replay` — the shadow-table rebuild: create shadow, re-drain the journal from origin through the lane's own apply, swap names atomically, drop the old table — all under a session advisory lock so exactly one rebuilder runs per lane cluster-wide.
- Packages: `@effect/sql` (`sql.reserve` — the pinned connection a session lock demands); `project/async.md` (the lane's apply is the replay body).
- Entry: `Rebuild.replay(lane, table)` — operations invoke it after an upcast-chain fix, a fold change, or a quarantine drain; the read side stays serving the old table until the swap, so a rebuild is invisible to readers.
- Receipt: the replay's span carries `store.rebuild` with lane, rows folded, and swap timestamp — the audit fact stream records the swap as an operator action.
- Growth: a compaction variant (rebuild snapshot then drop frontier partitions) is the same fold with a `Retain` tail — one more step in the body, not a second machine.
- Law: the session lock is the singleton guarantee — `pg_advisory_lock` on the lane's hash over a `sql.reserve`d connection held for the whole rebuild, released explicitly; a transaction lock cannot span the multi-transaction replay, which is exactly why the session form exists here and only here.
- Law: the swap is one transaction — `ALTER TABLE x RENAME TO x_retired; ALTER TABLE x_shadow RENAME TO x; DROP TABLE x_retired` — readers see old rows or new rows, never a mix; the sqlite arm rides the same rename family on its single writer.

```typescript
const _replay = (lane: string, table: string, drain: (into: string) => Effect.Effect<number, SqlError.SqlError | ParseResult.ParseError, SqlClient.SqlClient>) =>
  Effect.flatMap(SqlClient.SqlClient, (sql) =>
    Effect.scoped(
      Effect.gen(function* () {
        const _conn = yield* sql.reserve
        yield* sql`SELECT pg_advisory_lock(hashtextextended(${lane}, 0))`
        const shadow = `${table}_shadow`
        yield* sql`CREATE TABLE IF NOT EXISTS ${sql(shadow)} (LIKE ${sql(table)} INCLUDING ALL)`
        const folded = yield* drain(shadow)
        yield* sql.withTransaction(
          Effect.gen(function* () {
            yield* sql`ALTER TABLE ${sql(table)} RENAME TO ${sql(`${table}_retired`)}`
            yield* sql`ALTER TABLE ${sql(shadow)} RENAME TO ${sql(table)}`
            yield* sql`DROP TABLE ${sql(`${table}_retired`)}`
          }),
        )
        yield* sql`SELECT pg_advisory_unlock(hashtextextended(${lane}, 0))`
        return folded
      }).pipe(Effect.withSpan("store.rebuild", { attributes: { lane } })),
    ))

const Rebuild = {
  jobs: _jobs,
  schedule: _schedule,
  immv: _immv,
  replay: _replay,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Rebuild }
```
