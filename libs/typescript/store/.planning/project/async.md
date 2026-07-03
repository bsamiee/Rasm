# [STORE_ASYNC]

The async projection lane: a checkpointed, LISTEN/NOTIFY-woken, SKIP-LOCKED drain daemon that folds journal events past its checkpoint into a read model on its own cadence — competing consumers claim the lane's checkpoint row with `FOR UPDATE SKIP LOCKED`, so N replicas cooperate with zero coordination and exactly one drains at a time; the wake stream merges the outbox NOTIFY channel with a schedule-paced poll so a lost notification costs latency, never correctness. Poison events divert to a quarantine ledger as typed evidence and the checkpoint advances past them — the lane never wedges — and replaying quarantine after a fix is one statement, because the divert kept everything.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]           | [OWNS]                                                                     |
| :-----: | :------------------ | :----------------------------------------------------------------------------- |
|  [01]   | `CHECKPOINT_LEDGER` | the checkpoint row, the SKIP-LOCKED claim, the quarantine ledger                 |
|  [02]   | `DRAIN_DAEMON`      | the wake-merge, the batched drain fold, the daemon Layer, poison divert          |

## [2]-[CHECKPOINT_LEDGER]

- Owner: the `projection_checkpoint` and `projection_quarantine` ensure rows plus the claim statement — one row per lane name, claimed inside the drain transaction, advanced only when the batch commits.
- Packages: `@effect/sql` (`sql.onDialectOrElse` for the claim pair).
- Receipt: `AsyncLane.Mark` — `{ lane, checkpoint, drained }` — each drain cycle's evidence; `telemetry` meters lag as `journal head − checkpoint`, both readable in one query.
- Growth: a new lane is one more checkpoint row keyed by its name — the ledger schema never changes; a second consumer of one lane is a replica, not a row.
- Law: the claim is the coordination — `SELECT … FOR UPDATE SKIP LOCKED` on the lane's single row; a replica that misses the claim skips the cycle instead of blocking, so competing consumers are safe by construction and the sqlite arm degrades to the single-writer file through `onDialectOrElse`.
- Law: checkpoint and apply commit together — the fold's upserts and the checkpoint advance share the drain transaction, so a crash replays from the checkpoint and the upsert's idempotence (keyed by cell, stamped by version) absorbs the overlap.
- Law: quarantine is a typed divert, never a drop — the poisoned sequence, its raw envelope, and the fault's rendering land as a row; the checkpoint advances past it, and `replay(lane, sequence)` re-enters the row after repair.

```typescript
const _checkpointDdl: Capability.Ensure = {
  relation: "projection_checkpoint",
  pg: `CREATE TABLE IF NOT EXISTS projection_checkpoint (
    lane TEXT PRIMARY KEY,
    checkpoint BIGINT NOT NULL DEFAULT 0,
    claimed_at TIMESTAMPTZ);`,
  sqlite: `CREATE TABLE IF NOT EXISTS projection_checkpoint (
    lane TEXT PRIMARY KEY,
    checkpoint INTEGER NOT NULL DEFAULT 0,
    claimed_at TEXT);`,
}

const _quarantineDdl: Capability.Ensure = {
  relation: "projection_quarantine",
  pg: `CREATE TABLE IF NOT EXISTS projection_quarantine (
    lane TEXT NOT NULL, sequence BIGINT NOT NULL,
    envelope JSONB NOT NULL, fault TEXT NOT NULL,
    diverted_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    replayed_at TIMESTAMPTZ,
    PRIMARY KEY (lane, sequence));`,
  sqlite: `CREATE TABLE IF NOT EXISTS projection_quarantine (
    lane TEXT NOT NULL, sequence INTEGER NOT NULL,
    envelope TEXT NOT NULL, fault TEXT NOT NULL,
    diverted_at TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now')),
    replayed_at TEXT,
    PRIMARY KEY (lane, sequence));`,
}
```

## [3]-[DRAIN_DAEMON]

- Owner: `AsyncLane.of(spec)` — the lane value bundling fold, apply target, batch policy, and quarantine posture — and `AsyncLane.daemon(lane)` — the `Layer<never>` registration node that forks the wake-drain loop under the graph `Scope`.
- Packages: `effect` (`Effect`, `Stream`, `Schedule`, `Layer`, `Either`, `Option`); `@effect/sql-pg` (`PgClient.listen` — the pg wake); `journal/upcast.md` (the lift), `journal/outbox.md` (`Outbox.channel`).
- Entry: the app composes `AsyncLane.daemon(lane)` into its root — lifetime is the Layer's; there is no start/stop verb, because the scope closing IS stop; `lane.replay(sequence)` is the quarantine re-entry.
- Receipt: each cycle emits `Mark` through the lane's span (`store.drain` with lane and count attributes) — observability is the declaration's, not the body's.
- Growth: a new wake source (a second channel, a cron edge) is one more stream merged into `_wake`; a new batch policy axis is a field on `spec.batch`; the fold and apply stay untouched.
- Law: the wake merges LISTEN with a spaced poll — `Stream.merge(listen, Stream.repeatEffectWithSchedule(Effect.void, Schedule.spaced(spec.batch.patience)))` — the pg lane wakes on NOTIFY within the poll gap, the sqlite lane rides the poll alone through the same `serviceOption` read that makes the listen arm optional; correctness never depends on delivery.
- Law: the drain is a bounded fold per cycle — read events `sequence > checkpoint LIMIT batch.size` ordered by sequence, decode through the plan, apply per event with `Either`-diverted poison, advance the checkpoint to the last seen sequence — and repeat until a short page proves the lane is caught up.
- Law: per-event apply is the quarantine boundary — decode or apply failure quarantines THAT sequence and continues; a `SqlError` on the transaction itself retries the cycle under the lane's `Schedule` (the fault family's retry row), because infrastructure faults are transient where poison is not.
- Boundary: rebuilds and IVM alternatives are `project/rebuild.md`'s; the sqlite degradation verdicts (`ivm` → these lanes, `channel` → poll) are `lane/sqlite.md` rows consumed here through the optional-service read.

```typescript
import { type Duration, Effect, Either, Layer, Option, Schedule, Stream, type ParseResult } from "effect"
import { SqlClient, type SqlError } from "@effect/sql"
import { PgClient } from "@effect/sql-pg"
import type { AppKey } from "@rasm/ts/kernel"
import type { Capability } from "../capability/row.ts"
import { Journal } from "../journal/append.ts"
import { Outbox } from "../journal/outbox.ts"
import type { Upcast } from "../journal/upcast.ts"

declare namespace AsyncLane {
  type Spec<A> = {
    readonly name: string
    readonly app: AppKey
    readonly plan: Upcast.Plan<A>
    readonly apply: (event: A, sequence: number) => Effect.Effect<void, SqlError.SqlError | ParseResult.ParseError, SqlClient.SqlClient>
    readonly batch: { readonly size: number; readonly patience: Duration.DurationInput }
  }
  type Mark = {
    readonly lane: string
    readonly checkpoint: number
    readonly drained: number
  }
}

const _claim = (sql: SqlClient.SqlClient, lane: string) =>
  sql.onDialectOrElse({
    orElse: () => sql`SELECT checkpoint FROM projection_checkpoint WHERE lane = ${lane}`,
    pg: () => sql`SELECT checkpoint FROM projection_checkpoint WHERE lane = ${lane} FOR UPDATE SKIP LOCKED`,
  })

const _cycle = <A>(spec: AsyncLane.Spec<A>) =>
  Effect.flatMap(SqlClient.SqlClient, (sql) =>
    sql.withTransaction(
      Effect.gen(function* () {
        yield* sql`INSERT INTO projection_checkpoint ${sql.insert([{ lane: spec.name, checkpoint: 0 }])}
          ON CONFLICT (lane) DO NOTHING`
        const claimed = yield* _claim(sql, spec.name).values
        if (claimed.length === 0) return { lane: spec.name, checkpoint: -1, drained: 0 } satisfies AsyncLane.Mark
        const checkpoint = Number(claimed[0]![0])
        const page = yield* sql`SELECT sequence, tag, event_version, payload FROM journal_event
          WHERE app = ${spec.app} AND sequence > ${checkpoint}
          ORDER BY sequence LIMIT ${spec.batch.size}`
        const verdicts = yield* Effect.forEach(page, (row) =>
          spec.plan.decode({
            tag: String(row.tag),
            version: Number(row.event_version),
            payload: typeof row.payload === "string" ? JSON.parse(row.payload) : row.payload,
          }).pipe(
            Effect.flatMap((event) => spec.apply(event, Number(row.sequence))),
            Effect.as(Either.right(Number(row.sequence))),
            Effect.catchAll((fault) =>
              Effect.as(
                sql`INSERT INTO projection_quarantine ${sql.insert([{
                  lane: spec.name,
                  sequence: Number(row.sequence),
                  envelope: typeof row.payload === "string" ? row.payload : JSON.stringify(row.payload),
                  fault: String(fault),
                }])} ON CONFLICT (lane, sequence) DO NOTHING`,
                Either.left(Number(row.sequence)),
              )),
          ))
        const advanced = page.length === 0 ? checkpoint : Number(page[page.length - 1]!.sequence)
        yield* sql`UPDATE projection_checkpoint SET checkpoint = ${advanced}, claimed_at = ${Journal.now(sql)} WHERE lane = ${spec.name}`
        return { lane: spec.name, checkpoint: advanced, drained: verdicts.length } satisfies AsyncLane.Mark
      }),
    ))

const _wake = <A>(spec: AsyncLane.Spec<A>) =>
  Stream.unwrap(
    Effect.map(Effect.serviceOption(PgClient.PgClient), (pg) =>
      Stream.merge(
        Option.match(pg, {
          onNone: () => Stream.empty,
          onSome: (client) => Stream.orDie(client.listen(Outbox.channel(spec.app))),
        }),
        Stream.repeatEffectWithSchedule(Effect.succeed("<poll>"), Schedule.spaced(spec.batch.patience)),
        { haltStrategy: "either" },
      )),
  )

const AsyncLane = {
  of: <A>(spec: AsyncLane.Spec<A>) => spec,
  ddl: [_checkpointDdl, _quarantineDdl],
  daemon: <A>(spec: AsyncLane.Spec<A>): Layer.Layer<never, never, SqlClient.SqlClient> =>
    Layer.scopedDiscard(
      Effect.forkScoped(
        Stream.runForEach(_wake(spec), () =>
          Effect.repeat(_cycle(spec).pipe(Effect.withSpan("store.drain", { attributes: { lane: spec.name } })), {
            until: (mark) => mark.drained < spec.batch.size,
          })).pipe(Effect.orDie),
      ),
    ),
  replay: (lane: string, sequence: number) =>
    Effect.flatMap(SqlClient.SqlClient, (sql) =>
      sql`UPDATE projection_quarantine SET replayed_at = ${Journal.now(sql)}
          WHERE lane = ${lane} AND sequence = ${sequence} AND replayed_at IS NULL
          RETURNING envelope`),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { AsyncLane }
```
