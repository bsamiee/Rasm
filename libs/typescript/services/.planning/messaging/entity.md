# [SERVICES_ENTITY]

The addressable-actor modality of the messaging surface — `SessionEntity`, the `@effect/cluster` `Entity` whose `RpcGroup`-backed behavior is a stateful, sharded, single-writer-per-key actor, and `EntityProxy`-derived RPC the request tier dials. The entity is the second `messaging/` modality beside the request-driven `folder:messaging/internal-rpc#INTERNAL_RPC` proxy: where the proxy is stateless host-to-worker dispatch, the entity is durable per-key state the shard manager pins to exactly one runner so concurrent writes to one key serialize through one mailbox. The actor persists its per-key state through the one `folder:persistence/store-boundary#STORE_BOUNDARY` `PgClient`, never a parallel store. A collaborative-session actor DECODES the C# CRDT op-log wire off the `interchange` boundary and never re-mints it. Two further per-key modalities ride the same actor placement: the `DelayedDelivery` ROW schedules an entity payload for a future `DateTime` through the cluster's own `DeliverAt` protocol — the per-key future-message rail beneath the cluster-wide cron — and the `EntityResource` ROW holds a scoped resource that survives runner restarts until its idle-TTL elapses, the resource-durability modality beside the state-durability the actor already owns. This page rides the same shard manager the workflow engine layers onto and carries no .NET wire type beyond the decoded op-log payload.

## [1]-[INDEX]

| [CLUSTER]                | [OWNS]                                                                                                                                  |
| :----------------------- | :------------------------------------------------------------------------------------------------------------------------------------- |
| `[2]-[ENTITY]`           | the addressable `Entity` actor, the `ClusterSchema.Persisted` durable-entity flag, the single-writer-per-key placement, the `EntityProxy` projection, and the CRDT op-log decode boundary |
| `[3]-[DELAYED_DELIVERY]` | the `DeliverAt` per-key delayed-delivery rail — a `DeliverAt`-implementing payload scheduling its own future-`DateTime` message through the shard manager, beneath the cluster-wide cron |
| `[4]-[ENTITY_RESOURCE]`  | the `EntityResource` restart-surviving per-key resource — `make({ acquire, idleTimeToLive })`, the `makeK8sPod` managed-pod arm, and the `CloseScope` release |

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

## [3]-[DELAYED_DELIVERY]

- Owner: `DelayedDelivery`, the per-key future-message rail — a `DeliverAt`-implementing entity payload whose `[DeliverAt.symbol]()` returns the delivery `DateTime`, so the shard manager schedules the message for that instant rather than dispatching it immediately. One timer modality on the entity, never a parallel timer table beside the actor; the rail composes with every entity type that admits a deferred message — a per-key retry-after, a deferred saga step, an agent-turn cooldown, a scheduled tenant action.
- Cases: a deferred message is one payload class implementing the `DeliverAt` protocol — the payload carries its target `DateTime` and `[DeliverAt.symbol]()` returns it, `DeliverAt.isDeliverAt(payload)` refines an unknown payload to the protocol, and `DeliverAt.toMillis(payload)` projects the instant to the epoch-millis the storage rail records (`SqlMessageStorage` persists `deliverAt: number | null` on the saved envelope, so the durable mailbox parks the message until the instant elapses and replays it exactly-once like every other cluster message). The `Schedule` verb is one `client(entityId).MessageTag(payload)` send where the payload's `DeliverAt` instant is in the future — the shard manager pins the message to the key's shard, persists it through `MessageStorage` with the `deliverAt` set, and the runner that owns the shard delivers it when the instant arrives; a runner restart re-loads the parked message from the durable mailbox so the delay survives a crash. The delivery instant derives from `DateTime` arithmetic on the entity's clock — a retry-after is `DateTime.add(now, backoff)`, a cooldown is `DateTime.add(lastTurn, window)` — so the timer is one `DateTime`-valued field on the payload, never an external scheduler or an `Effect.timeoutTo` parked on a fiber.
- Entry: the rail rides the SAME shard manager (`folder:runtime-backplane/backplane#RUNNER_AND_SCHEDULING` `RunnerBackplane`) and the SAME `MessageStorage` the immediate `Apply` routes through — `DeliverAt` is the per-key future-message rail beneath the cluster-wide `ScheduledWork.cron` pinned sweep, COMPOSING with it rather than replacing it: the cron stays the cluster-wide periodic sweep, `DeliverAt` becomes the per-message per-key delay; the `folder:durable-execution/saga#SAGA` deferred-step delay and the `folder:durable-execution/ai-activity#AI_ACTIVITY` retry cooldown schedule their own future entity message through this rail rather than a saga-side `Effect.timeoutTo` or a parallel activity timer.
- Wire: the rail carries no .NET wire type — the deferred payload and its `DeliverAt` instant are node-internal cluster messages; a collaborative-session deferred op still decodes its content off the `interchange` boundary, but the delay itself is the cluster's own protocol.
- Packages: `@effect/cluster` for the `DeliverAt` symbol protocol (`DeliverAt.isDeliverAt`/`DeliverAt.toMillis`, the `[DeliverAt.symbol]()` interface, the `SqlMessageStorage` `deliverAt` storage rail); `effect` for the `DateTime` delivery instant.
- Growth: a new deferred concern lands as one `DeliverAt`-implementing payload on an existing entity, never a second scheduling substrate; a new delay policy lands as one `DateTime` projection on the payload; the cron stays the cluster-wide sweep and `DeliverAt` the per-key rail beneath it.
- Boundary: the named defects — an `Effect.timeoutTo` or a fiber-parked delay instead of the durable `DeliverAt` rail (a parked fiber dies with the runner; `DeliverAt` survives in the durable mailbox); a parallel timer table beside the actor instead of the `DeliverAt` protocol on the payload; an external scheduler instead of the cluster's own exactly-once shard-placed delivery; collapsing the cluster-wide cron into the per-key rail or vice versa — they compose, the cron the periodic sweep, `DeliverAt` the per-message delay. This is a node-only surface, never browser-reachable.

```ts contract
import { DeliverAt, Entity, Sharding } from "@effect/cluster"
import { Rpc, RpcGroup } from "@effect/rpc"
import { DateTime, Duration, Effect, Schema as S } from "effect"

// --- [MODELS] --------------------------------------------------------------------------

class DeferredAction extends S.Class<DeferredAction>("DeferredAction")({
  key: S.String,
  kind: S.Literal("retry", "sagaStep", "agentCooldown", "tenantAction"),
  deliverAt: S.DateTimeUtc,
  payload: S.parseJson(S.Unknown),
}) {
  [DeliverAt.symbol](): DateTime.DateTime {
    return this.deliverAt
  }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const at = (kind: DeferredAction["kind"], key: string, when: DateTime.DateTime, payload: unknown): DeferredAction =>
  new DeferredAction({ key, kind, deliverAt: when, payload })

const after = (kind: DeferredAction["kind"], key: string, delay: Duration.DurationInput, payload: unknown): Effect.Effect<DeferredAction> =>
  DateTime.now.pipe(Effect.map((now) => at(kind, key, DateTime.addDuration(now, delay), payload)))

const millisOf = (action: DeferredAction): number | null => DeliverAt.toMillis(action)

const isDeferred = (payload: unknown): payload is DeferredAction => DeliverAt.isDeliverAt(payload)

const DeferredRpcs = RpcGroup.make(
  Rpc.make("Schedule", { payload: { action: DeferredAction }, success: S.Void }),
)

const DeferredEntity = Entity.fromRpcGroup("DeferredAction", DeferredRpcs)

const schedule = (entityId: string, action: DeferredAction): Effect.Effect<void, never, Sharding.Sharding> =>
  DeferredEntity.client.pipe(
    Effect.flatMap((client) => client(entityId).Schedule({ action })),
    Effect.asVoid,
  )
```

## [4]-[ENTITY_RESOURCE]

- Owner: `EntityResource`, the restart-surviving per-key resource — `EntityResource.make({ acquire, idleTimeToLive })` acquires a scoped resource inside an entity that survives runner restarts until the idle-TTL elapses or the `CloseScope` tag fires, and `EntityResource.makeK8sPod(spec)` places a managed Kubernetes pod resource per key. The resource-durability modality beside the state-durability the actor already owns: the actor persists STATE through the `PgClient` but holds a live RESOURCE bound to the key — a persisted `Chat` handle, an external tool-host session, a connection pool — re-acquired only after an idle eviction, never per message.
- Cases: a per-key resource is one `EntityResource.make({ acquire, idleTimeToLive })` row — `acquire` is the scoped `Effect` building the resource (a `Chat.layerPersisted` handle, a pooled client, a sub-process), `idleTimeToLive` is the `Duration` after which an unused resource releases, and the returned `EntityResource<A, E>` carries `get` (the scoped read that re-acquires after eviction) and `close` (the explicit release); the resource's scope outlives a single message and a runner restart, so two `Turn` messages on one session share one live `Chat` handle rather than re-hydrating it each turn. The `makeK8sPod(spec)` arm is the same lifetime over a managed pod — `spec` is the `v1.Pod` the deploy tier defines, the resource is the `K8sHttpClient.PodStatus` the pod resolves to, and the pod is created on first `get` and torn down on idle eviction or `CloseScope`, so a per-tenant or per-session compute resource is one pod the actor key owns. The `CloseScope` tag is the release seam — firing it closes the resource's scope ahead of the idle-TTL, so an explicit `End`/`Interrupt` releases the resource deterministically while the idle-TTL is the safety floor for a silently abandoned key.
- Entry: the resource rides the SAME `Sharding`/`Entity.CurrentAddress` context the actor's handlers run in — `make` requires `Scope | Sharding | Entity.CurrentAddress` (the `makeK8sPod` arm additionally requires `K8sHttpClient`), so the resource is acquired inside the entity behavior and bound to the entity's address; the `folder:agent/session-actors#SESSION_ACTORS` `AgentSessionActor` COMPOSES this ROW for the per-session persisted `Chat` handle and the host tool-transport session bound to the session key (acquired once on `Start`, shared across every `Turn`, released on `End` or idle eviction, never re-acquired per `Turn`), and the collaborative `SessionEntity` composes it for an external sink connection; the `makeK8sPod` arm's `v1.Pod` spec arrives from the `folder:provisioning/contract#PROVISIONING` deploy tier, never minted by the actor.
- Wire: the resource carries no .NET wire type of its own — the persisted `Chat` handle, the pool, and the pod are node-internal; the only wire contact is transitively the `folder:agent/mcp-transport#MCP_TRANSPORT` host tool-transport session a `Turn` may drive, which decodes the host MCP projection at the `interchange` boundary.
- Packages: `@effect/cluster` for `EntityResource.make`/`makeK8sPod`, the `CloseScope` tag, and the `K8sHttpClient` backing the pod arm; `effect` for the `Scope`/`Duration` resource scope and idle-TTL.
- Growth: a new per-key live resource lands as one `EntityResource.make` row, never a re-acquire-per-message cost or an unbounded leak; a new managed-compute placement lands as one `makeK8sPod` arm over a `provisioning`-defined `v1.Pod`; a deterministic release fires the `CloseScope` tag ahead of the idle-TTL.
- Boundary: the named defects — re-acquiring the resource per message instead of binding it to the key across messages and restarts; an unbounded resource with no idle-TTL leaking a handle for an abandoned key; a hand-rolled pod-lifecycle beside the `makeK8sPod` arm instead of the cluster's managed-pod resource; minting the `v1.Pod` spec in the actor instead of receiving it from `provisioning/contract`; conflating the resource-durability the ROW owns with the state-durability the actor already persists through the `PgClient` — they are distinct modalities on one key. This is a node-only surface, never browser-reachable.

```ts contract
import { EntityResource, K8sHttpClient, Sharding } from "@effect/cluster"
import type { Entity } from "@effect/cluster"
import type { Chat } from "@effect/ai"
import type { v1 } from "@kubernetes/client-node"
import { Duration, Effect, Exit, Scope } from "effect"

// --- [SERVICES] ------------------------------------------------------------------------

const sessionChatResource = (
  acquire: Effect.Effect<Chat.Chat, never, Scope.Scope>,
): Effect.Effect<EntityResource.EntityResource<Chat.Chat>, never, Scope.Scope | Sharding.Sharding | Entity.CurrentAddress> =>
  EntityResource.make({ acquire, idleTimeToLive: Duration.minutes(30) })

const sessionPodResource = (
  spec: v1.Pod,
): Effect.Effect<EntityResource.EntityResource<K8sHttpClient.PodStatus>, never, Scope.Scope | Sharding.Sharding | Entity.CurrentAddress | K8sHttpClient.K8sHttpClient> =>
  EntityResource.makeK8sPod(spec, { idleTimeToLive: Duration.minutes(15) })

// --- [OPERATIONS] ----------------------------------------------------------------------

const useChat = <A, E>(
  resource: EntityResource.EntityResource<Chat.Chat>,
  use: (chat: Chat.Chat) => Effect.Effect<A, E>,
): Effect.Effect<A, E, Scope.Scope> =>
  resource.get.pipe(Effect.flatMap(use))

const release = (resource: EntityResource.EntityResource<unknown>): Effect.Effect<void> => resource.close

const closeAhead: Effect.Effect<void, never, EntityResource.CloseScope> =
  EntityResource.CloseScope.pipe(Effect.flatMap((scope) => Scope.close(scope, Exit.void)))
```
