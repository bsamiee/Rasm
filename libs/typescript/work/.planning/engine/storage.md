# [WORK_STORAGE]

Message persistence is a port, never an import: `work` composes the `@effect/cluster` `MessageStorage` Tag and the `@effect/sql` core `SqlClient` Tag, and the app root satisfies both with the `store`-owned driver Layer — the `-pg`/`-sqlite-*` drivers stay banned inside this folder, and the bootstrap posture is that cluster storage self-manages its own tables over the leased client. This page owns the composition law: one `Storage` vocabulary whose rows select the message and runner stores by durability tier, whose classification table folds every closed `ClusterError` tag into the kernel `FaultClass` vocabulary so one gate predicate re-drives exactly the transient family, and whose durability facts — the per-Rpc `ClusterSchema.Persisted` switch, the `Snowflake`-keyed `SaveResult.Duplicate` dedupe — are stated once and inherited by every entity, workflow, queue, and deliver surface downstream. A `store` import, a driver name in this folder, a hand actor-mailbox replay store, and a call-site retryability predicate are the named defects.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]      | [OWNS]                                                                       |
| :-----: | :------------- | :---------------------------------------------------------------------------- |
|  [01]   | [STORAGE_ROWS] | the message/runner store rows, the port posture, the durability + dedupe law  |
|  [02]   | [CLASS_FOLD]   | the `ClusterError` → `FaultClass` classification table and the transient gate |

## [2]-[STORAGE_ROWS]

[STORAGE_ROWS]:
- Owner: `Storage` — the storage vocabulary. The interior row table is keyed by durability tier: `durable` binds `SqlMessageStorage.layer` and `SqlRunnerStorage.layer` over the composed `SqlClient` and `ShardingConfig` requirements; `memory` binds `MessageStorage.layerMemory` and `RunnerStorage.layerMemory` for kit-driven specs and single-node development; `noop` binds `MessageStorage.layerNoop` beside the memory runner registry for lanes that provably persist nothing. The exported owner assembles rows, `kinds`, and the classification members under a `typeof`-derived stated annotation.
- Law: the port posture is the row's requirement tail — the `durable` row's layers hold `SqlClient` in `RIn`, the app root provides the `store`-owned driver Layer, and no spelling inside this folder can name a driver; wave-legality is structural, not disciplinary.
- Law: cluster storage owns its own tables — `SqlMessageStorage`/`SqlRunnerStorage` ensure and manage their message and runner relations over the leased client, so no `work`-authored DDL exists and the journal/outbox relations stay `store`'s; the two planes meet only at the `SqlClient` connection.
- Law: durability is a per-Rpc annotation, never a blanket — `ClusterSchema.Persisted` marks exactly the messages whose loss matters, an unannotated Rpc stays ephemeral, and dedupe is `MessageStorage.SaveResult` — `Duplicate` keyed on the `Snowflake` request id plus the payload primary key — so delivery is at-least-once with exactly-once effect and no consumer re-derives idempotency.
- Law: engine substitution is a row swap at one edit site — a spec block provides `Storage.memory`, the composition root provides `Storage.durable`, and every entity, cron, singleton, and workflow above the Tag is untouched; a fourth tier is one row.
- Boundary: which entity applies `Persisted` and how quotas fence a tenant are `engine/entity.md`'s; the driver Layer, tenancy scopes, and the outbox relation are `store`'s; this page owns only the composition rows and the port law.
- Entry: `Storage.durable` / `Storage.memory` / `Storage.noop`, read by the composition root only.
- Growth: a new persistence tier is one row; a new storage axis (a lease table, a poll cadence) is a field on the owning row.
- Packages: `@effect/cluster` (`MessageStorage`, `RunnerStorage`, `SqlMessageStorage`, `SqlRunnerStorage`), `effect` (`Types`).

```typescript
import { MessageStorage, RunnerStorage, SqlMessageStorage, SqlRunnerStorage } from "@effect/cluster"
import type { Types } from "effect"

const _kinds = ["durable", "memory", "noop"] as const
const _rows = {
  durable: { messages: SqlMessageStorage.layer, runners: SqlRunnerStorage.layer },
  memory: { messages: MessageStorage.layerMemory, runners: RunnerStorage.layerMemory },
  noop: { messages: MessageStorage.layerNoop, runners: RunnerStorage.layerMemory },
} as const

declare namespace Storage {
  type Kinds = typeof _kinds
  type Kind = keyof typeof _rows
  type Row = { readonly messages: unknown; readonly runners: unknown }
  type Contract = { readonly [K in Kinds[number]]: Row }
  type _Rows<T extends Contract = typeof _rows> = T
  type _Keys<K extends keyof Contract = Kind> = K
}
```

## [3]-[CLASS_FOLD]

[CLASS_FOLD]:
- Owner: the classification table — one interior `as const` row set mapping every closed `ClusterError` tag to its kernel `FaultClass` kind: `MailboxFull` and `AlreadyProcessingMessage` are backpressure facts (`exhausted`, `conflicted`), `PersistenceError`, `EntityNotAssignedToRunner`, `RunnerNotRegistered`, and `RunnerUnavailable` are routing/storage transients (`unavailable`), and `MalformedMessage` is a decode terminal (`malformed`). `Storage.classify` folds any fault through the table — a tagged cluster fault reads its row, everything else falls to the kernel's own structural probe — and `Storage.transient` is the one gate predicate `Effect.retry`'s `while` consumes.
- Law: cluster faults carry no `class` field, so the kernel budget gate alone would fold them to `defect` and never re-drive; this table is the bridge that makes `Budget`-grade retry semantics total over the engine's own fault family, and a call-site predicate re-deriving retryability beside it is policy leakage.
- Law: the fold is total over `unknown` — a foreign throw, a defect, and a work-folder fault (which carries `class` by the folder convention `flow/activity.md` seals) all land at their correct severity through one expression; no arm of the folder switches on a cluster tag the table already maps.
- Law: routing faults drive re-route, never crash — `EntityNotAssignedToRunner` during shard movement is `unavailable` and retryable, so a client call under any kernel-budget gate rides out a rebalance; `MalformedMessage` is terminal because replaying an undecodable payload cannot succeed.
- Boundary: the ten-class vocabulary, its rank lattice, and `retryable` semantics are `kernel`'s; which budget row a surface selects is that surface's policy; this page owns only the tag-to-class correspondence.
- Entry: `Storage.classify(fault)` / `Storage.transient(fault)`.
- Growth: a new cluster fault tag is one table row; every gate inherits it with zero new branches.
- Packages: `effect` (`Option`, `Predicate`, `Record`), `@rasm/ts/kernel` (`FaultClass`).

```typescript
import { FaultClass } from "@rasm/ts/kernel"
import { Option, Predicate, Record } from "effect"

const _classes = {
  AlreadyProcessingMessage: "conflicted",
  EntityNotAssignedToRunner: "unavailable",
  MailboxFull: "exhausted",
  MalformedMessage: "malformed",
  PersistenceError: "unavailable",
  RunnerNotRegistered: "unavailable",
  RunnerUnavailable: "unavailable",
} as const satisfies Record.ReadonlyRecord<string, FaultClass.Kind>

const _lookup: Record.ReadonlyRecord<string, FaultClass.Kind> = _classes

const _classified = (fault: unknown): FaultClass.Kind =>
  Predicate.hasProperty(fault, "_tag") && Predicate.isString(fault._tag)
    ? Option.getOrElse(Record.get(_lookup, fault._tag), () => FaultClass.of(fault))
    : FaultClass.of(fault)

declare namespace Storage {
  type ClusterTag = keyof typeof _classes
  type Shape = Types.Simplify<
    typeof _rows & {
      readonly kinds: Kinds
      readonly classify: (fault: unknown) => FaultClass.Kind
      readonly transient: (fault: unknown) => boolean
    }
  >
}

const Storage: Storage.Shape = {
  ..._rows,
  kinds: _kinds,
  classify: _classified,
  transient: (fault) => FaultClass[_classified(fault)].retryable,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Storage }
```
