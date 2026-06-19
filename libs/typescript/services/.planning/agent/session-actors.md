# [SERVICES_SESSION_ACTORS]

The agent-session-as-actor binding — `AgentSessionActor`, the addressable `@effect/cluster` `Entity` that pins one long-lived agent conversation to one shard single-writer-per-key, so a multi-turn agent session is a sharded actor whose mailbox serializes concurrent turns rather than racing two writers on one conversation. It is the third member of the one `./agent` subpath closure: where `agent/agent-runtime#AGENT_RUNTIME` owns the per-step durable agent composition and `agent/mcp-transport#MCP_TRANSPORT` owns the host tool transport, this page owns the SESSION lifecycle as a placed actor. The actor reuses the `messaging/entity#ENTITY` addressable-actor placement mechanics (`Entity.make`/`toLayer`/`getShardId`, `ClusterSchema.Persisted`) and the `agent/agent-runtime#AGENT_RUNTIME` `DurableAgent` step; the session state — the persisted `Chat` handle and the `AgentJournal` cursor — persists through the one `persistence/store-boundary#STORE_BOUNDARY` `PgClient`, never a parallel store. This page is node-only and crosses no .NET wire beyond the agent's own tool transport.

## [1]-[INDEX]

One cluster: `[2]-[SESSION_ACTORS]` owns the agent-session `Entity`, the single-writer-per-session placement, the durable session lifecycle, and the journal-cursor resume.

## [2]-[SESSION_ACTORS]

- Owner: `AgentSessionActor`, the `@effect/cluster` `Entity` that places one agent conversation per key single-writer — a `Start`/`Turn`/`Interrupt`/`End` `RpcGroup` behavior, a `toLayer` handler pinned to one shard per session id, and the `getShardId`/`getShardGroup` placement; the session lifecycle composes the `agent/agent-runtime#AGENT_RUNTIME` `DurableAgent` step over the placed per-key state.
- Cases: the actor protocol is one `RpcGroup` of `Rpc.make` procedures — `Start` (open a session, hydrate the persisted `Chat` and the `AgentJournal` cursor), `Turn` (run one `agent/agent-runtime#AGENT_RUNTIME` `DurableAgent` step against the session's conversation, the single-writer mutation), `Interrupt` (suspend the in-flight agent workflow by execution id), `End` (flush and release) — every procedure `annotateRpcs(ClusterSchema.Persisted, true)` so the session messages route through `MessageStorage` for exactly-once replay; `Entity.make("AgentSession", [...rpcs])` builds the entity and `entity.toLayer(handlers)` installs the behavior, the handler reaching the persisted `Chat`, the `AgentJournal`, and the one `SqlClient`. The single-writer-per-session contract is the shard manager's placement: `getShardId(sessionId)` pins one shard per session so two concurrent `Turn` messages on one session serialize through one mailbox and never race the conversation, the property a request-driven proxy cannot model; `getShardGroup` routes a per-tenant agent pool to its tenant shard group. Interrupt/resume is the agent's own resume composed with the actor's durability — `Interrupt` suspends the `ClusterEngine` agent workflow, and a later `Turn` or an external resume re-hydrates the persisted `Chat` and replays from the `AgentJournal` cursor, so a restarted runner reattaches the exact session conversation.
- Entry: the actor rides the SAME `runtime-backplane/backplane#RUNNER_AND_SCHEDULING` `RunnerBackplane` shard manager the cluster engine and the `messaging/entity#ENTITY` actors layer onto — the agent session is the agent-tier actor modality, distinct from the `messaging/entity#ENTITY` collaborative-document actor; the `agent/agent-runtime#AGENT_RUNTIME` `DurableAgent` runs inside the `Turn` handler as a durable unit on `durable-execution/engine#ENGINE`; the per-session state (the persisted `Chat` handle, the `AgentJournal` cursor) persists through the one `persistence/store-boundary#STORE_BOUNDARY` `PgClient`, never a second store; the `EntityProxy.toRpcGroup(actor)` projection derives the request-tier RPC the panel-to-host session dispatch dials, so a session is started and advanced over the one RPC surface rather than a hand-built client.
- Wire: the actor carries no .NET wire type of its own — its only wire contact is transitively through the `agent/mcp-transport#MCP_TRANSPORT` host tool transport a `Turn` may drive, which consumes the decoded host MCP projection at the `interchange` boundary; the session state and the placement are node-internal.
- Packages: `@effect/cluster` for the `Entity`/`Sharding` placement (`Entity.make`/`toLayer`/`client`/`getShardId`/`getShardGroup`, `ClusterSchema.Persisted`, `EntityProxy.toRpcGroup`, `EntityProxyServer.layerRpcHandlers`), `@effect/rpc` for the session behavior `RpcGroup`, `@effect/ai` for the persisted `Chat` session the `DurableAgent` step drives, `@effect/sql`/`@effect/sql-pg` for the per-session state through `persistence/store-boundary#STORE_BOUNDARY`, `@effect/platform-node` for the driver host.
- Growth: a new session message lands as one `Rpc.make` procedure on the existing protocol; a new placement policy lands as one `getShardGroup` row; a new agent-session concern lands as one entity type with its own `RpcGroup`, never a parallel actor framework; the session becomes request-callable by extending the `EntityProxy`-derived group.
- Boundary: the named defects — a parallel session store beside the one `PgClient`; an unpinned multi-writer session breaking the single-writer-per-key contract; a second addressable-actor framework beside the cluster `Entity`; a session whose messages are not `ClusterSchema.Persisted` and so replay non-exactly-once; a hand-built session client instead of the `EntityProxy` projection. This is a node-only surface, never browser-reachable.

```ts contract
import type { SqlError } from "@effect/sql"
import type { AlreadyProcessingMessage, MailboxFull, PersistenceError } from "@effect/cluster"
import { SqlClient } from "@effect/sql"
import { ClusterSchema, Entity, EntityProxy, EntityProxyServer, Sharding } from "@effect/cluster"
import { Rpc, RpcClient, RpcGroup } from "@effect/rpc"
import { Effect, Layer, Schema as S } from "effect"

class AgentSessionState extends S.Class<AgentSessionState>("AgentSessionState")({
  sessionId: S.String,
  tenant: S.String,
  storeId: S.String,
  executionId: S.String,
  journalCursor: S.Number,
  status: S.Literal("running", "completed", "failed", "interrupted"),
}) {}

class AgentSessionFault extends S.TaggedError<AgentSessionFault>()("AgentSessionFault", {
  sessionId: S.String,
  stage: S.Literal("start", "turn", "interrupt", "end"),
  cause: S.Unknown,
}) {}

const AgentSessionRpcs = RpcGroup.make(
  Rpc.make("Start", { payload: { sessionId: S.String, tenant: S.String, storeId: S.String, prompt: S.String }, success: AgentSessionState, error: AgentSessionFault }),
  Rpc.make("Turn", { payload: { sessionId: S.String, prompt: S.String }, success: AgentSessionState, error: AgentSessionFault }),
  Rpc.make("Interrupt", { payload: { sessionId: S.String }, success: AgentSessionState, error: AgentSessionFault }),
  Rpc.make("End", { payload: { sessionId: S.String }, success: S.Void, error: AgentSessionFault }),
).annotateRpcs(ClusterSchema.Persisted, true)

const AgentSessionActor = Entity.fromRpcGroup("AgentSession", AgentSessionRpcs)

type AgentSessionRpc = RpcGroup.Rpcs<typeof AgentSessionRpcs>

const AgentSessionProxyGroup = EntityProxy.toRpcGroup(AgentSessionActor)
const AgentSessionProxyHandlers = EntityProxyServer.layerRpcHandlers(AgentSessionActor)

interface AgentSessionModality {
  readonly entity: typeof AgentSessionActor
  readonly behavior: Layer.Layer<never, never, SqlClient.SqlClient | Sharding.Sharding>
  readonly client: Effect.Effect<(entityId: string) => RpcClient.RpcClient.From<AgentSessionRpc, MailboxFull | AlreadyProcessingMessage | PersistenceError>, never, Sharding.Sharding>
  readonly proxyGroup: typeof AgentSessionProxyGroup
  readonly proxyHandlers: Layer.Layer<unknown, never, Sharding.Sharding>
  readonly place: (sessionId: string) => Effect.Effect<unknown, never, Sharding.Sharding>
}
```
