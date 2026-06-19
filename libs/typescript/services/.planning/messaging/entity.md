# [SERVICES_ENTITY]

The addressable-actor modality of the messaging surface — `SessionEntity`, the `@effect/cluster` `Entity` whose `RpcGroup`-backed behavior is a stateful, sharded, single-writer-per-key actor, and `EntityProxy`-derived RPC the request tier dials. The entity is the second `messaging/` modality beside the request-driven `messaging/internal-rpc#INTERNAL_RPC` proxy: where the proxy is stateless host-to-worker dispatch, the entity is durable per-key state the shard manager pins to exactly one runner so concurrent writes to one key serialize through one mailbox. The actor persists its per-key state through the one `persistence/store-boundary#STORE_BOUNDARY` `PgClient`, never a parallel store. A collaborative-session actor DECODES the C# CRDT op-log wire off the `interchange` boundary and never re-mints it. This page rides the same shard manager the workflow engine layers onto and carries no .NET wire type beyond the decoded op-log payload.

## [1]-[INDEX]

One cluster: `[2]-[ENTITY]` owns the addressable `Entity` actor, the `ClusterSchema.Persisted` durable-entity flag, the single-writer-per-key placement, the `EntityProxy` projection, and the CRDT op-log decode boundary.

## [2]-[ENTITY]

- Owner: `SessionEntity`, the `@effect/cluster` `Entity` addressable actor — a typed `RpcGroup` protocol, a `toLayer` behavior-handler layer pinned single-writer-per-key, a per-key `client`, and `getShardId`/`getShardGroup` placement; `EntityProxy.toRpcGroup` derives the request-tier RPC over it. The actor is one polymorphic owner over the stateful per-key concerns (collaborative document sessions, per-tenant rate governors, live presence), discriminated by entity type and message tag, never a parallel entity per concern.
- Cases: the entity protocol is one `RpcGroup` of `Rpc.make` procedures — `Open` (hydrate per-key state from the one `PgClient`), `Apply` (fold a decoded op into the per-key state, the single-writer mutation), `Snapshot` (read the current state), `Close` (flush and release) — every procedure `annotateRpcs(ClusterSchema.Persisted, true)` so messages route through `MessageStorage` for exactly-once replay across a restart; `Entity.make(type, [...rpcs])` builds the entity and `entity.toLayer(handlers)` installs the behavior as one `Layer` requiring `Sharding`, the handler effect reaching the per-key state and the one `SqlClient` through `WorkflowInstance`-free entity context. The single-writer-per-key contract is the shard manager's own placement: `getShardId(entityId)` resolves the one shard a key pins to and the mailbox serializes concurrent `Apply` messages, so two writers to one session never race; `getShardGroup` routes a per-tenant governor to its tenant's shard group. A collaborative-session actor's `Apply` decodes the C# CRDT op-log payload off the `interchange` boundary into the actor's op vocabulary and folds it into the per-key document state — the op-log is the settled wire the C# branch owns, consumed at the decoded boundary, and the actor never re-mints the op union or re-derives the CRDT merge from the SDK frame.
- Entry: the entity rides the SAME shard manager the cluster-backed `WorkflowEngine` layers onto (`runtime-backplane/backplane#RUNNER_AND_SCHEDULING` `RunnerBackplane`), distinct from the request-driven `messaging/internal-rpc#INTERNAL_RPC` proxy — the entity is the stateful-actor modality, the proxy the stateless-dispatch modality, both over one `Sharding`; the per-key state persists through the one `persistence/store-boundary#STORE_BOUNDARY` `PgClient` (the `Open`/`Apply`/`Close` handlers issue the entity-state DML through the one `SqlClient`), never a second store; `EntityProxy.toRpcGroup(entity)` derives the tag-prefixed RPC group (a `${Type}.${Tag}` procedure plus a `${Type}.${Tag}Discard` fire-and-forget per rpc) and `EntityProxyServer.layerRpcHandlers(entity)` installs the matching handlers, so the request tier dials the actor over the one RPC surface rather than a hand-built parallel client.
- Wire: the only .NET-wire contact is the collaborative-session `Apply` decode — the C# CRDT op-log payload is decoded off the `interchange` boundary into the actor's op vocabulary, the wire owner is the C# branch, and the actor consumes the decoded shape, never reconstructs the op union from the raw frame; every other entity concern (rate governor, presence) carries no wire type and is node-internal.
- Packages: `@effect/cluster` for the `Entity`/`Sharding` addressable-actor surface (`Entity.make`/`toLayer`/`client`/`getShardId`/`getShardGroup`, `ClusterSchema.Persisted`, `EntityProxy.toRpcGroup`, `EntityProxyServer.layerRpcHandlers`), `@effect/rpc` for the entity behavior `RpcGroup` and the `Rpc.make` procedures, `@effect/sql` and `@effect/sql-pg` for the per-key state through `persistence/store-boundary#STORE_BOUNDARY`, and `@effect/platform-node` for the driver host.
- Growth: a new stateful per-key concern lands as one entity type with its own `RpcGroup` rows, never a parallel actor framework; a new entity message lands as one `Rpc.make` procedure on the existing protocol; a new placement policy lands as one `getShardGroup` row; the entity becomes request-callable by extending the `EntityProxy`-derived group, never a hand-built procedure.
- Boundary: the named defects — a parallel state store beside the one `PgClient`; a second addressable-actor framework beside the cluster `Entity`; a hand-built per-key client instead of the `Entity.client`/`EntityProxy` projection; a re-minted CRDT op union instead of the decoded wire payload; an unpinned multi-writer key breaking the single-writer-per-key contract; an entity whose messages are not `ClusterSchema.Persisted` and so replay non-exactly-once. This is a node-only surface, never browser-reachable.

```ts contract
import type { PgClient } from "@effect/sql-pg"
import type { SqlError } from "@effect/sql"
import { SqlClient } from "@effect/sql"
import type { AlreadyProcessingMessage, MailboxFull, PersistenceError } from "@effect/cluster"
import { ClusterSchema, Entity, EntityProxy, EntityProxyServer, Sharding } from "@effect/cluster"
import { Rpc, RpcClient, RpcGroup } from "@effect/rpc"
import { Effect, Layer, Schema as S } from "effect"

const SessionOp = S.Union(
  S.Struct({ _tag: S.Literal("Insert"), at: S.Number, text: S.String }),
  S.Struct({ _tag: S.Literal("Delete"), at: S.Number, len: S.Number }),
)
type SessionOp = S.Schema.Type<typeof SessionOp>

class SessionState extends S.Class<SessionState>("SessionState")({
  sessionId: S.String,
  tenant: S.String,
  revision: S.Number,
  document: S.String,
}) {}

class SessionFault extends S.TaggedError<SessionFault>()("SessionFault", {
  sessionId: S.String,
  stage: S.Literal("open", "apply", "snapshot", "close", "decode"),
  cause: S.Unknown,
}) {}

const SessionRpcs = RpcGroup.make(
  Rpc.make("Open", { payload: { sessionId: S.String, tenant: S.String }, success: SessionState, error: SessionFault }),
  Rpc.make("Apply", { payload: { sessionId: S.String, op: SessionOp }, success: SessionState, error: SessionFault }),
  Rpc.make("Snapshot", { payload: { sessionId: S.String }, success: SessionState, error: SessionFault }),
  Rpc.make("Close", { payload: { sessionId: S.String }, success: S.Void, error: SessionFault }),
).annotateRpcs(ClusterSchema.Persisted, true)

const SessionEntity = Entity.fromRpcGroup("CollaborativeSession", SessionRpcs)

const sessionBehavior = SessionEntity.toLayer(
  Effect.gen(function* () {
    const sql = yield* SqlClient.SqlClient
    const load = (sessionId: string): Effect.Effect<SessionState, SessionFault> =>
      sql<SessionState>`SELECT session_id AS "sessionId", tenant, revision, document FROM session_state WHERE session_id = ${sessionId}`.pipe(
        Effect.flatMap((rows) => rows[0] ? Effect.succeed(rows[0]) : Effect.fail(new SessionFault({ sessionId, stage: "open", cause: "missing" }))),
        Effect.mapError((cause) => cause instanceof SessionFault ? cause : new SessionFault({ sessionId, stage: "open", cause })),
      )
    const fold = (state: SessionState, op: SessionOp): SessionState =>
      op._tag === "Insert"
        ? new SessionState({ ...state, revision: state.revision + 1, document: state.document.slice(0, op.at) + op.text + state.document.slice(op.at) })
        : new SessionState({ ...state, revision: state.revision + 1, document: state.document.slice(0, op.at) + state.document.slice(op.at + op.len) })
    const persist = (next: SessionState): Effect.Effect<SessionState, SessionFault> =>
      sql`INSERT INTO session_state (session_id, tenant, revision, document) VALUES (${next.sessionId}, ${next.tenant}, ${next.revision}, ${next.document})
          ON CONFLICT (session_id) DO UPDATE SET revision = ${next.revision}, document = ${next.document}`.pipe(
        Effect.as(next),
        Effect.mapError((cause) => new SessionFault({ sessionId: next.sessionId, stage: "apply", cause })),
      )
    return {
      Open: (envelope) => load(envelope.payload.sessionId),
      Apply: (envelope) => load(envelope.payload.sessionId).pipe(Effect.flatMap((state) => persist(fold(state, envelope.payload.op)))),
      Snapshot: (envelope) => load(envelope.payload.sessionId),
      Close: () => Effect.void,
    }
  }),
)

const sessionClient = SessionEntity.client

const SessionProxyGroup = EntityProxy.toRpcGroup(SessionEntity)
const SessionProxyHandlers = EntityProxyServer.layerRpcHandlers(SessionEntity)

const placeSession = (sessionId: string): Effect.Effect<unknown, never, Sharding.Sharding> =>
  SessionEntity.getShardId(sessionId)

type SessionRpc = RpcGroup.Rpcs<typeof SessionRpcs>

interface ActorModality {
  readonly entity: typeof SessionEntity
  readonly behavior: Layer.Layer<never, never, SqlClient.SqlClient | Sharding.Sharding>
  readonly client: Effect.Effect<(entityId: string) => RpcClient.RpcClient.From<SessionRpc, MailboxFull | AlreadyProcessingMessage | PersistenceError>, never, Sharding.Sharding>
  readonly proxyGroup: typeof SessionProxyGroup
  readonly proxyHandlers: Layer.Layer<unknown, never, Sharding.Sharding>
}
```
