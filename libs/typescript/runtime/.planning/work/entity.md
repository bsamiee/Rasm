# [RUNTIME_ENTITY]

The durable-actor plane: a cluster entity is an `@effect/rpc` `RpcGroup` given sharded, per-id, single-writer identity, and this page owns everything that gives it that identity — the `WorkClass` service-class vocabulary every work surface prices itself against, the `Actor` mint that binds a protocol to fenced bounds and durability annotations, the `Mailbox` durable-message port over the data wave's `SqlClient` with the one `ClusterError → FaultClass` bridge, and the `Grid` topology assembly — leaderless sharding over `RunnerStorage` advisory locks, K8s runner health, the runner entry rows, the cluster singleton, and the workflow-engine bridge `flow` runs on. Sharding has no manager election: runners acquire, refresh, and release shard locks against storage, so the topology is a table of peers and a runner death is a lock expiry, never a coordinator failover. `work` composes `MessageStorage` and `SqlClient` as Tags satisfied at the app root from the data wave's `Stores` scopes; no SQL driver import is spellable here. The module ships on the `./server` exports subpath as `runtime/src/work/entity.ts`.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]    | [OWNS]                                                                                | [PUBLIC]    |
| :-----: | :----------- | :------------------------------------------------------------------------------------ | :---------- |
|  [01]   | `WORK_CLASS` | the one service-class row table — concurrency, mailbox, idle, budget, attempts, priority | `WorkClass` |
|  [02]   | `ACTOR_MINT` | the entity mint: protocol, fenced bounds, durability annotations, client, exposure    | `Actor`     |
|  [03]   | `MAILBOX`    | the durable-message port, dedup receipt, the `ClusterError → FaultClass` bridge       | `Mailbox`   |
|  [04]   | `GRID`       | leaderless topology, runner health, entry rows, singleton, the workflow-engine bridge | `Grid`      |

## [02]-[WORK_CLASS]

[WORK_CLASS]:
- Owner: `WorkClass`, the assembled service-class vocabulary — the row table carries every axis a work surface reads, and the exported owner derives `kinds` and `schema` from that table under one `typeof`-derived annotation. Three seed rows ride the parameterized family: `interactive` (serialized handling, small mailbox, long residency, `pulse` budget, three attempts, urgency 0), `steady` (bounded parallel handling, mid mailbox, `lease` budget, five attempts, urgency 50), and `bulk` (wide handling, deep mailbox, short residency, `bulk` budget, eight attempts, urgency 100).
- Law: the row is the collapse point for three formerly parallel tables — entity fenced quotas, queue lane policy, and relay egress pacing all read these columns; a work surface that re-declares a `{ concurrency, retry }` pair beside this table is the named split-brain defect.
- Law: `concurrency` and `mailbox` are the entity fence — a tenant's actor saturates to `MailboxFull` at its own row's bound without starving a sibling; `idle` prices residency; `budget` names the `core/fault#RETRY_BUDGET` row; `attempts` is the durable lane's park ceiling; `urgency` is the integer the claim `ORDER BY` term reads — smaller claims first.
- Law: `defectRetry` derives from the row's budget — the entity's `defectRetryPolicy` IS `Budget.schedule(row.budget)`, so a defecting handler re-drives under the same geometry as a transient fault and no second retry vocabulary exists.
- Growth: a new service class is one tuple entry plus one row every fence, lane, and pacing fold inherits at compile time; a new axis (a hedge delay, a spend weight) is one `Row` field consumed by the surfaces that name it.
- Boundary: which class an actor or job family selects is that declaration's policy field; this table prices classes and never names consumers.
- Packages: `effect` (`Duration`, `Schema`); `@rasm/ts/core` (`Budget`).

```typescript
import { ClusterError, ClusterMetrics, ClusterSchema, ClusterWorkflowEngine, Entity, EntityProxy, EntityProxyServer, MessageStorage, RunnerHealth, Sharding, ShardingConfig, ShardingRegistrationEvent, Snowflake, SqlMessageStorage, SqlRunnerStorage } from "@effect/cluster"
import type { HttpApi } from "@effect/platform"
import { type Rpc, RpcGroup } from "@effect/rpc"
import { Array, Duration, Effect, Layer, Metric, Schema, Stream, Struct, type Types } from "effect"
import { Budget, FaultClass, type TenantContext } from "@rasm/ts/core"
import { Setting } from "../proc/config.ts"

const _classRows = {
  interactive: { concurrency: 1, mailbox: 64, idle: Duration.minutes(30), budget: "pulse", attempts: 3, urgency: 0 },
  steady: { concurrency: 4, mailbox: 512, idle: Duration.minutes(10), budget: "lease", attempts: 5, urgency: 50 },
  bulk: { concurrency: 16, mailbox: 4096, idle: Duration.minutes(1), budget: "bulk", attempts: 8, urgency: 100 },
} as const

const _classes = Struct.keys(_classRows)

declare namespace WorkClass {
  type Kinds = typeof _classes
  type Kind = keyof typeof _classRows
  type Row = {
    readonly concurrency: number
    readonly mailbox: number
    readonly idle: Duration.Duration
    readonly budget: Budget.Kind
    readonly attempts: number
    readonly urgency: number
  }
  type Contract = { readonly [K in Kinds[number]]: Row }
  type Shape = Types.Simplify<
    typeof _classRows & {
      readonly kinds: Kinds
      readonly schema: Schema.Literal<[...Kinds]>
      readonly defectRetry: (kind: Kind) => Budget.Gated
    }
  >
  type _Rows<T extends Contract = typeof _classRows> = T
}

const WorkClass: WorkClass.Shape = {
  ..._classRows,
  kinds: _classes,
  schema: Schema.Literal(..._classes),
  defectRetry: (kind) => Budget.schedule(_classRows[kind].budget),
}
```

## [03]-[ACTOR_MINT]

[ACTOR_MINT]:
- Owner: `Actor` — the one entity mint: `Actor.Spec` binds a name, a typed protocol generator, a `WorkClass` kind, tenant partition, and per-Rpc posture sets. The generator receives the mint's polymorphic annotation function and calls `RpcGroup.make` over its declaration tuple, preserving the exact Rpc union without an assertion while compiling each Rpc's `Persisted` and `ClientTracingEnabled` verdicts. `Entity.fromRpcGroup` then declares the actor, shard-group annotation partitions ids by tenant, and `toLayer` registers the exhaustive handler map under the class fence.
- Law: the protocol IS the contract — payload, success, and error `Schema`s on each `Rpc` are the message wire, the handler signature, and the client return at once; a message shape declared beside the group is unspellable. An agent session, a delivery drain, and a projection worker are all instances of this one mint.
- Law: single-writer is an entity fact — messages to one id serialize on one live instance cluster-wide, so per-key ordering needs no lock, version, or queue beside the actor; concurrency inside the row bounds parallel messages across ids, never within one.
- Law: an ephemeral message family is a per-Rpc annotation verdict, not a second mint — `spec.ephemeral` names the Rpc tags whose `Persisted` annotation compiles `false` at the mint's per-Rpc fold, so a heartbeat or a poll rides the same protocol with no storage write, and a tag outside the set keeps the durable-and-replayed contract; a group-wide `annotateRpcs(ClusterSchema.Persisted, true)` that ignores the set is the inert-policy defect this fold exists to prevent.
- Law: interrupt and trace posture are annotation rows on the same mint — `spec.interrupt` annotates `ClusterSchema.Uninterruptible` (`boolean | "client" | "server"`) group-wide so a must-settle message survives client disconnect by declaration, and `spec.untraced` names the chatty poll tags whose `ClusterSchema.ClientTracingEnabled` compiles `false` in the same per-Rpc fold; neither is a handler-interior branch.
- Law: delayed delivery is native — a message whose payload carries the `DeliverAt.DeliverAt` `toMillis` interface delivers at its instant on the actor plane, so a one-shot deferred job is a scheduled message, never a timer or a poll beside the mailbox; `schedule`'s one-shot deferral rides this row.
- Law: the mailbox-drain reply seam is `Entity.Replier` — a `toLayerMailbox` handler answers out-of-band through `succeed`/`fail`/`failCause`/`complete(Exit)` on the handed replier, so a streaming-batch drain settles each message exactly once without occupying the serialized lane.
- Law: locality is span evidence — `Entity.CurrentAddress` and `Entity.CurrentRunnerAddress` are in-handler context Tags whose `EntityAddress`/`RunnerAddress` stamp the message span, so which shard and which runner handled a message reads off every trace, not just the registration census.
- Law: `Actor.expose(entity)` projects the entity as `serve/api#CONTRIBUTION` pairing material — `EntityProxy.toRpcGroup(entity)` beside `EntityProxyServer.layerRpcHandlers(entity)` is exactly the `Contribution.rpc(group, handlers)` pair, and `EntityProxy.toHttpApiGroup(name, entity)` beside the api-reading builder `(api) => EntityProxyServer.layerHttpApi(api, name, entity)` is exactly the `Contribution.http(group, handlers)` pair — so the app mounts an actor through the same two pairing constructors as every other group and the typed client derives for free; a bare group projection whose handler binding the app must rediscover is the half-pairing defect. The mailbox-draining `toLayerMailbox` form is the streaming-batch escape hatch and carries the same bounds.
- Law: a per-actor external handle rides `EntityResource.make({ acquire, idleTimeToLive })` — acquired once, surviving shard-move restarts, released on idle expiry — and the K8s pod form is `EntityResource.makeK8sPod`; a handle opened inside a handler body leaks across replays and is the rejected form.
- Entry: `Actor.make` at the owning page; `Entity.makeTestClient(entity, layer)` binds the kit-driven spec client with no runner.
- Growth: a new actor family is one `Spec` value; a new message is one `Rpc` row on its group; a new per-Rpc posture axis is one exemption set folded at `_annotated`; a new modality (streaming reply) is `toLayerMailbox` on the same spec.
- Packages: `@effect/cluster` (`Entity`, `EntityProxy`, `EntityProxyServer`, `EntityResource`, `ClusterSchema`); `@effect/rpc` (`Rpc`, `RpcGroup`); `@effect/platform` (`HttpApi` — the pairing builder's api parameter); `effect` (`Array`, `Layer`, `Effect`).

```typescript
declare namespace Actor {
  type Spec<Type extends string, Rpcs extends Rpc.Any> = {
    readonly name: Type
    readonly protocol: (annotate: <Current extends Rpcs>(rpc: Current) => Current) => RpcGroup.RpcGroup<Rpcs>
    readonly clazz: WorkClass.Kind
    readonly tenant: (entityId: string) => TenantContext.Key
    readonly ephemeral: ReadonlyArray<Rpcs["_tag"]>
    readonly untraced: ReadonlyArray<Rpcs["_tag"]>
    readonly interrupt: boolean | "client" | "server"
  }
}

const _annotated = <Rpcs extends Rpc.Any>(spec: Actor.Spec<string, Rpcs>): RpcGroup.RpcGroup<Rpcs> =>
  spec.protocol(<Current extends Rpcs>(rpc: Current): Current =>
    rpc
      .annotate(ClusterSchema.Persisted, !Array.contains(spec.ephemeral, rpc._tag))
      .annotate(ClusterSchema.ClientTracingEnabled, !Array.contains(spec.untraced, rpc._tag)))

const _make = <Type extends string, Rpcs extends Rpc.Any>(spec: Actor.Spec<Type, Rpcs>) => {
  const row = WorkClass[spec.clazz]
  const entity = Entity.fromRpcGroup(spec.name, _annotated(spec)).pipe(
    (e) => e.annotateRpcs(ClusterSchema.ShardGroup, (entityId: string) => spec.tenant(entityId)),
    (e) => e.annotateRpcs(ClusterSchema.Uninterruptible, spec.interrupt),
  )
  const registered = (handlers: Parameters<typeof entity.toLayer>[0]) =>
    entity.toLayer(handlers, {
      concurrency: row.concurrency,
      mailboxCapacity: row.mailbox,
      maxIdleTime: row.idle,
      defectRetryPolicy: WorkClass.defectRetry(spec.clazz),
    })
  return { entity, registered, client: entity.client } as const
}

const _expose = <Type extends string, Rpcs extends Rpc.Any>(entity: Entity.Entity<Type, Rpcs>) => ({
  rpc: EntityProxy.toRpcGroup(entity),
  rpcHandlers: EntityProxyServer.layerRpcHandlers(entity),
  http: (name: string) => EntityProxy.toHttpApiGroup(name, entity),
  httpHandlers: (name: string) => <Api extends HttpApi.HttpApi.Any>(api: Api) => EntityProxyServer.layerHttpApi(api, name, entity),
})

const Actor = { make: _make, expose: _expose }
```

## [04]-[MAILBOX]

[MAILBOX]:
- Owner: `Mailbox` — the durable-message port composition and its fault fold. `SqlMessageStorage.layer` persists every `Persisted` message on the `SqlClient` Tag the app root satisfies from the data wave's `Stores` scopes; `Snowflake.layerGenerator` mints the monotonic message identity dedup keys on; `MessageStorage.layerMemory` is the single-node/spec tier and `layerNoop` the ephemeral tier — three tier rows behind one Tag, selected at the root.
- Law: delivery is at-least-once folded to exactly-once effect — `SaveResult.Duplicate`, keyed on `Snowflake` plus the Rpc primary key, re-subscribes a replayed send to the prior result and never re-executes the handler; the sender needs no idempotency wrapper because dedup is the storage contract.
- Law: the fault bridge is one governed record — every `ClusterError` tag maps to its `FaultClass` kind (`MailboxFull → exhausted`, `AlreadyProcessingMessage → conflicted`, `PersistenceError → unavailable`, `MalformedMessage → malformed`, `EntityNotAssignedToRunner → unavailable`, `RunnerNotRegistered → unavailable`, `RunnerUnavailable → unavailable`) — so cluster faults enter the branch rail with rank, blame, and the retryable column already decided, and `Mailbox.classify` is total over the family. Re-drive reads `FaultClass.retryable` through the caller's `Budget` row; no cluster-specific retry predicate exists.
- Law: cluster topology is observed through the package's own instruments — `Grid.metrics` reads `ClusterMetrics.entities`, `ClusterMetrics.singletons`, `ClusterMetrics.runners`, `ClusterMetrics.runnersHealthy`, and `ClusterMetrics.shards` as one concurrent snapshot, so runner and shard topology stays aligned with the cluster runtime's registered gauges. Mailbox depth and drain rate belong to the queue and journal owners because `ClusterMetrics` exposes neither; attributing those signals to this package is a phantom contract.
- Boundary: the journal, outbox, and idempotency-ledger relations belong to the data wave; this port persists cluster envelopes in cluster-owned relations on the same scope, and atomicity with a domain aggregate is the data journal's transaction, reached by enqueuing from inside it — never by threading this storage into a domain write.
- Growth: a new durability tier is one row on the tier record; a new cluster fault tag is one bridge row the governed record demands at compile time.
- Packages: `@effect/cluster` (`SqlMessageStorage`, `MessageStorage`, `Snowflake`, `ClusterError`, `ClusterMetrics`); `effect` (`Layer`, `Metric`); `@rasm/ts/core` (`FaultClass`).

```typescript
declare namespace Mailbox {
  type Tier = "durable" | "memory" | "noop"
}

const _tiers = {
  durable: Layer.provideMerge(SqlMessageStorage.layer, Snowflake.layerGenerator),
  memory: MessageStorage.layerMemory,
  noop: MessageStorage.layerNoop,
} as const

const _bridge = {
  MailboxFull: "exhausted",
  AlreadyProcessingMessage: "conflicted",
  PersistenceError: "unavailable",
  MalformedMessage: "malformed",
  EntityNotAssignedToRunner: "unavailable",
  RunnerNotRegistered: "unavailable",
  RunnerUnavailable: "unavailable",
} as const satisfies Record<ClusterError.ClusterError["_tag"], FaultClass.Kind>

const _classify = (fault: ClusterError.ClusterError): FaultClass.Kind => _bridge[fault._tag]

const Mailbox = {
  tier: (tier: Mailbox.Tier) => _tiers[tier],
  classify: _classify,
  retryable: (fault: ClusterError.ClusterError) => FaultClass[_classify(fault)].retryable,
}
```

## [05]-[GRID]

[GRID]:
- Owner: `Grid` — the topology core: `ShardingConfig.layerFromEnv` reads lock intervals from `Setting`; `SqlRunnerStorage.layer` is the leaderless rebalancing substrate; `RunnerHealth` is a kind row (`k8s`, `ping`, `noop`); and `Grid.workflow` is the package's `ClusterWorkflowEngine.layer`. The runner binding remains the `proc/exec#RUNTIME_ROWS` selection — `NodeClusterHttp.layer`/`NodeClusterSocket.layer` and their Bun peers compose beside this core at boot, while single-node selects `Mailbox.tier("memory")` and `WorkflowEngine.layerMemory`.
- Law: `K8sHttpClient` is discovery only — it reads pod state through the service-account mount; provisioning, scaling, and image facts belong to the deploy plane, and a write-shaped call against it is unspellable here.
- Law: `Singleton.make(name, run)` is the one cluster-wide-instance form, reached directly at the package surface — the relay drain, a maintenance sweep, a horizon groom each run as a singleton that migrates on rebalance; a leader flag, a lock table, a "primary" config row, or a local rename of the package member is the rejected form.
- Law: the workflow bridge is `ClusterWorkflowEngine.layer` at the package surface — it satisfies the `WorkflowEngine` Tag over `Sharding` plus `MessageStorage`, so every `flow` definition runs durable and sharded by the same Layer selection that boots the grid; the spec engine swap happens at the root, never in a definition.
- Receipt: `Grid.census` folds `Sharding.getRegistrationEvents` through `ShardingRegistrationEvent.match` into typed rows — entity type or singleton name per registration — so the booted actor census lands beside the capability report as shaped startup evidence, never raw events.
- Growth: a new runner transport is one entry row — the websocket runner is `HttpRunner.layerWebsocket`, the served-with-clients form `RunnerServer.layerWithClients`; a new health mode is one kind row; a topology axis change is a `ShardingConfig` field the environment stamps.
- Packages: `@effect/cluster` (`Sharding`, `ShardingConfig`, `SqlRunnerStorage`, `RunnerHealth`, `K8sHttpClient`, `Singleton`, `ClusterWorkflowEngine`); `../proc/config.ts` (`Setting`); `../proc/exec.ts` (`Runtime` rows at the boot module).

```typescript
declare namespace Grid {
  type Health = "k8s" | "ping" | "noop"
}

const _health = {
  k8s: RunnerHealth.layerK8s(),
  ping: RunnerHealth.layerPing,
  noop: RunnerHealth.layerNoop,
} as const

const _topology = Layer.unwrapEffect(
  Effect.map(Setting, (setting) =>
    ShardingConfig.layerFromEnv({
      shardLockRefreshInterval: setting.cluster.lockRefresh,
      shardLockExpiration: setting.cluster.lockExpiry,
    })),
)

const _grid = (health: Grid.Health) =>
  Sharding.layer.pipe(
    Layer.provideMerge(SqlRunnerStorage.layer),
    Layer.provideMerge(_health[health]),
    Layer.provideMerge(_topology),
  )

const _rostered = ShardingRegistrationEvent.match({
  onEntityRegistered: (event) => ({ kind: "entity" as const, name: event.entity.type }),
  onSingletonRegistered: (event) => ({ kind: "singleton" as const, name: event.name }),
})

const _metrics = Effect.all({
  entities: Metric.value(ClusterMetrics.entities),
  singletons: Metric.value(ClusterMetrics.singletons),
  runners: Metric.value(ClusterMetrics.runners),
  runnersHealthy: Metric.value(ClusterMetrics.runnersHealthy),
  shards: Metric.value(ClusterMetrics.shards),
}, { concurrency: "unbounded" })

const Grid = {
  layer: _grid,
  workflow: ClusterWorkflowEngine.layer,
  census: Effect.map(Sharding.Sharding, (sharding) => Stream.map(sharding.getRegistrationEvents, _rostered)),
  metrics: _metrics,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Actor, Grid, Mailbox, WorkClass }
```

```mermaid
---
config:
  theme: base
  look: classic
  layout: elk
  flowchart:
    curve: linear
    padding: 25
  themeVariables:
    darkMode: true
    fontFamily: "SF Mono, Menlo, Cascadia Mono, Segoe UI Mono, Consolas, monospace"
    useGradient: false
    dropShadow: "none"
    background: "#282A36"
    primaryColor: "#44475A"
    primaryTextColor: "#F8F8F2"
    primaryBorderColor: "#BD93F9"
    lineColor: "#FF79C6"
    textColor: "#F8F8F2"
    titleColor: "#D6BCFA"
    clusterBkg: "#21222C"
    clusterBorder: "#D6BCFA"
    edgeLabelBackground: "#21222C"
    labelBackgroundColor: "#21222C"
  themeCSS: ".nodeLabel{font-size:13px;font-weight:500}.edgeLabel{font-size:12px;font-weight:500}.cluster-label .nodeLabel{font-size:13.5px;font-weight:700;letter-spacing:.08em}.edge-thickness-normal{stroke-width:2px}.edge-thickness-thick{stroke-width:3px}.edge-pattern-dashed,.edge-pattern-dotted{stroke-width:1.5px;stroke-dasharray:4 6}.node rect,.node circle,.node polygon,.node path,.node .outer-path{stroke-width:1.5px;filter:none!important}.cluster rect{stroke-width:1px!important;stroke-dasharray:5 4!important;filter:none!important}.marker path{transform:scale(.8);transform-origin:5px 5px}.marker circle{transform:scale(.48);transform-origin:5px 5px}.edgeLabel rect{transform-box:fill-box;transform-origin:center;transform:scale(1.1,1.2)}"
---
flowchart LR
  accTitle: Durable actor grid composition
  accDescr: Actor declarations register into sharding, which composes mailbox persistence, advisory-lock ownership, runner health, configuration, and the cluster workflow engine.
  subgraph runner[each runner]
    A[Actor.make spec] --> B[registered Layer]
    B --> S[Sharding]
    S --> M[Mailbox tier]
  end
  S <--> L[(RunnerStorage advisory locks)]
  M --> Q[(SqlClient — data Stores scope)]
  H[RunnerHealth row] --> S
  E[ShardingConfig from Setting] --> S
  S --> W[ClusterWorkflowEngine.layer → WorkflowEngine]
  classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
  classDef data fill:#FFB86CBF,stroke:#FFB86C,color:#282A36
  classDef external fill:#8BE9FD66,stroke:#8BE9FD,color:#282A36
  classDef success fill:#50FA7B66,stroke:#50FA7B,color:#282A36
  class A,B,S primary
  class M,L,Q data
  class H,E external
  class W success
```
