# [SERVICES_IDEMPOTENCY]

The cross-surface exactly-once owner — `IdempotencyLedger`, one keyed `Model.Class` over the `folder:persistence/store#STORE_BOUNDARY` `PgClient` recording an idempotency key plus its terminal outcome, and `ClaimOrReplay`, the claim-or-replay fold that short-circuits a re-delivered message to its prior result rather than re-executing it. It is the single shared dedup surface the `folder:execution/outbox#TRANSACTIONAL_OUTBOX` relay, the `folder:messaging/entity#ENTITY` actors, and the `folder:agent/session#SESSION_ACTORS` turns reach for the same exactly-once guarantee, never one re-derived ledger per consumer. The ledger admits ONLY the cross-surface dedup the per-surface primitives do not span: the `@effect/cluster` `MessageStorage` `SaveResult.Duplicate` already dedups one entity's mailbox replay and the `@effect/workflow` `idempotencyKey` already dedups one workflow's activity re-run — this owner spans the boundary where one logical operation crosses both, where neither per-surface primitive can see the other's prior outcome. The owner rides the one `PgClient` and crosses no .NET wire.

## [01]-[INDEX]

- [01]-[IDEMPOTENCY_LEDGER]: owns the keyed `IdempotencyLedger` `Model.Class`, the `ClaimState` claim vocabulary, the `ClaimOrReplay` state-threaded fold, and the per-surface boundary against `SaveResult.Duplicate`/`idempotencyKey`.

## [02]-[IDEMPOTENCY_LEDGER]

- Owner: `IdempotencyLedger`, one `Model.Class` keyed by `(idempotencyKey, surface)` recording the terminal outcome of a cross-surface operation; `ClaimOrReplay`, the fold over the `ClaimState` claim vocabulary (`fresh`/`claimed`/`replayed`) that either claims the key and runs the operation or replays the recorded terminal outcome. One owner over the cross-surface dedup concern, never a sibling ledger per consumer.
- Cases: `IdempotencyLedger` is one keyed row — the idempotency key, the producing surface (`outbox`/`entity`/`session`), a `pending`/`succeeded`/`failed` terminal status, the encoded terminal outcome, the attempt count, and the claim/settle timestamps — written through the one `PgClient` and tenant-scoped through the `folder:persistence/tenancy#TENANCY` GUC. A `claim` is one `INSERT … ON CONFLICT (idempotency_key, surface) DO UPDATE SET attempts = attempts + 1 RETURNING status, outcome, attempts, (xmax = 0) AS inserted` round-trip — the `DO UPDATE` form always returns the row (a `DO NOTHING` form returns no row on conflict and cannot distinguish claimed from replayed) and the `xmax = 0` system-column test discriminates the fresh insert from the conflicting update: an inserted row (`xmax = 0`) yields `fresh` (this caller owns the operation), a conflict on a still-`pending` row yields `claimed` (a concurrent caller is mid-flight, the caller waits or rejects), and a conflict on a terminal row yields `replayed` carrying the prior outcome. `ClaimOrReplay` is the state-threaded fold over those three claim states folded by `Match.tagsExhaustive` — `fresh` runs the operation then `settle`s the terminal outcome into the row, `replayed` short-circuits to the decoded prior outcome with no re-execution, `claimed` rejects with a typed in-flight fault the caller retries-after; a new claim state breaks every fold site at compile time, never a statement switch or an enumerated arm. `settle` is the terminal write — `UPDATE … SET status, outcome WHERE idempotency_key = … AND status = 'pending'` — so the terminal transition is single-writer against the claiming row and a crash between claim and settle leaves the row `pending` for the per-surface retry to re-claim.
- Entry: the ledger rides the one `folder:persistence/store#STORE_BOUNDARY` `PgClient` — the `IdempotencyLedger` is one entity on the `EntityRegistry`, the claim and settle ride the one `SqlClient`, and every row is tenant-scoped through the same `folder:persistence/tenancy#TENANCY` `app.current_tenant` GUC; the `folder:execution/outbox#TRANSACTIONAL_OUTBOX` relay claims by the outbox row's aggregate-plus-event key before publishing so a re-claimed row replays its delivery receipt, the `folder:messaging/entity#ENTITY` collaborative `Apply` claims by the decoded op's content key so a re-delivered CRDT op replays its prior state rather than re-folding, and the `folder:agent/session#SESSION_ACTORS` `Turn` claims by the turn's prompt key so a re-issued turn replays its agent result rather than re-running the `DurableAgent` step.
- Wire: the owner crosses no .NET wire — the idempotency key, the claim, and the terminal outcome are node-internal over the one `PgClient`; the keys the consumers claim by (the outbox aggregate key, the CRDT op content key, the turn prompt key) are themselves node-side projections, never a decoded wire contract.
- Packages: `@effect/sql` and `@effect/sql-pg` for the keyed `IdempotencyLedger` `Model.Class`, the atomic `ON CONFLICT DO UPDATE … RETURNING (xmax = 0)` claim, and the single-writer `settle` over the one `PgClient`; `effect` for the `ClaimState` `Data.TaggedEnum`, the `ClaimOrReplay` `Match.tagsExhaustive` fold, and the `Schema`-encoded terminal outcome.
- Growth: a new dedup consumer claims by its own content key against the one ledger, never a sibling ledger; a new claim state lands as one `ClaimState` `Data.TaggedEnum` variant breaking the `ClaimOrReplay` fold at compile time; a new terminal outcome type lands as one `Schema` the row's outcome column carries; the per-surface dedup primitives stay where they are — this owner never absorbs them.
- Boundary: the named defects — a per-consumer ledger instead of the one cross-surface owner; a `SELECT … THEN INSERT` claim racing two callers instead of the atomic `ON CONFLICT DO UPDATE … RETURNING (xmax = 0)`; a statement switch over the claim states instead of the `ClaimOrReplay` fold; a re-execution on `replayed` instead of the short-circuit to the recorded outcome; the ledger duplicating the per-surface primitives it does NOT replace — the `@effect/cluster` `MessageStorage` `SaveResult.Duplicate` (`Success | Duplicate<R>` carrying `originalId` + `lastReceivedReply`) still owns one entity's mailbox replay dedup and the `@effect/workflow` `idempotencyKey` still owns one workflow's activity re-run dedup; this ledger spans ONLY the boundary where one logical operation crosses both surfaces and neither primitive sees the other's outcome. This is a node-only surface, never browser-reachable.

```ts contract
import type { PgClient } from "@effect/sql-pg"
import type { SqlError } from "@effect/sql"
import { Model, SqlClient } from "@effect/sql"
import { Data, Effect, Layer, Match, Schema as S } from "effect"

// --- [MODELS] --------------------------------------------------------------------------

class IdempotencyLedger extends Model.Class<IdempotencyLedger>("IdempotencyLedger")({
  idempotencyKey: S.String,
  surface: S.Literal("outbox", "entity", "session"),
  tenant: S.String,
  status: S.Literal("pending", "succeeded", "failed"),
  outcome: Model.FieldOption(S.parseJson(S.Unknown)),
  attempts: S.Number,
  claimedAt: Model.DateTimeInsert,
  settledAt: Model.FieldOption(Model.DateTimeFromDate),
}) {}

type Surface = S.Schema.Type<typeof IdempotencyLedger.fields.surface>

// --- [TYPES] ---------------------------------------------------------------------------

type ClaimState = Data.TaggedEnum<{
  readonly Fresh:    { readonly key: string; readonly surface: Surface }
  readonly Claimed:  { readonly key: string; readonly surface: Surface; readonly attempts: number }
  readonly Replayed: { readonly key: string; readonly surface: Surface; readonly outcome: unknown; readonly status: "succeeded" | "failed" }
}>
const ClaimState = Data.taggedEnum<ClaimState>()

// --- [ERRORS] --------------------------------------------------------------------------

class LedgerFault extends S.TaggedError<LedgerFault>()("LedgerFault", {
  key: S.String,
  surface: S.Literal("outbox", "entity", "session"),
  reason: S.Literal("in-flight", "store"),
  cause: S.Unknown,
}) {}

// --- [SERVICES] ------------------------------------------------------------------------

class IdempotencyClaim extends Effect.Service<IdempotencyClaim>()("services/IdempotencyClaim", {
  accessors: true,
  effect: Effect.gen(function* () {
    const sql = yield* SqlClient.SqlClient

    const claim = (key: string, surface: Surface, tenant: string): Effect.Effect<ClaimState, SqlError.SqlError> =>
      sql<{ status: IdempotencyLedger["status"]; outcome: unknown; attempts: number; inserted: boolean }>`
        INSERT INTO idempotency_ledger (idempotency_key, surface, tenant, status, attempts, claimed_at)
        VALUES (${key}, ${surface}, ${tenant}, 'pending', 1, now())
        ON CONFLICT (idempotency_key, surface) DO UPDATE SET attempts = idempotency_ledger.attempts + 1
        RETURNING status, outcome, attempts, (xmax = 0) AS inserted
      `.pipe(
        Effect.map((rows) => rows[0]!),
        Effect.map((row) =>
          row.inserted || row.status === "pending"
            ? row.inserted
              ? ClaimState.Fresh({ key, surface })
              : ClaimState.Claimed({ key, surface, attempts: row.attempts })
            : ClaimState.Replayed({ key, surface, outcome: row.outcome, status: row.status as "succeeded" | "failed" }),
        ),
      )

    const settle = (key: string, surface: Surface, status: "succeeded" | "failed", outcome: unknown): Effect.Effect<void, SqlError.SqlError> =>
      sql`
        UPDATE idempotency_ledger SET status = ${status}, outcome = ${JSON.stringify(outcome)}, settled_at = now()
        WHERE idempotency_key = ${key} AND surface = ${surface} AND status = 'pending'
      `.pipe(Effect.asVoid)

    const claimOrReplay = <A>(
      key: string,
      surface: Surface,
      tenant: string,
      run: Effect.Effect<A, LedgerFault>,
    ): Effect.Effect<A, LedgerFault | SqlError.SqlError> =>
      claim(key, surface, tenant).pipe(
        Effect.flatMap(
          Match.tagsExhaustive({
            Fresh: () =>
              run.pipe(
                Effect.tap((a) => settle(key, surface, "succeeded", a)),
                Effect.tapError(() => settle(key, surface, "failed", null)),
              ),
            Replayed: ({ outcome, status }) =>
              status === "succeeded"
                ? Effect.succeed(outcome as A)
                : Effect.fail(new LedgerFault({ key, surface, reason: "store", cause: outcome })),
            Claimed: ({ attempts }) =>
              Effect.fail(new LedgerFault({ key, surface, reason: "in-flight", cause: `claimed, attempt ${attempts}` })),
          }),
        ),
      )

    return { claim, settle, claimOrReplay } as const
  }),
}) {}

// --- [COMPOSITION] ---------------------------------------------------------------------

const IdempotencyClaimLayer: Layer.Layer<IdempotencyClaim, never, SqlClient.SqlClient | PgClient.PgClient> =
  IdempotencyClaim.Default
```
