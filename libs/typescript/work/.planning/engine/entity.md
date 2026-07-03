# [WORK_ENTITY]

A durable actor is an `@effect/rpc` protocol given sharded, persistent, singleton-per-id identity: `Entity.make(type, rpcs)` binds the message contract, `.toLayer(handlers)` registers the compiler-checked handler record with `Sharding`, and `entity.client` yields the typed per-id caller — the mailbox, the shard math, and the replay loop are engine capability this folder never re-authors. This page owns what the engine leaves open: the `Fence` vocabulary — per-tenant quota rows every registration must select, the shard-group derivation over the kernel `TenantContext`, and the typed refusal surface `edge/hook` ports against — and the `Engine` assembly — topology from the `iac` `StackOutputs → ShardingConfig` seam, runner health rows with K8s discovery held to discovery alone, the routing service, and the workflow bridge that lets `flow` definitions run sharded. Runner entrypoint transports are root data selected off the `host/exec` runtime rows; a `platform-node`/`-bun` import here, an unfenced registration, a hand fiber-per-key registry, and `K8sHttpClient` reached for provisioning are the named defects.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]    | [OWNS]                                                                            |
| :-----: | :----------- | :---------------------------------------------------------------------------------- |
|  [01]   | [FENCE_ROWS] | the per-tenant quota vocabulary, the shard-group derivation, the refusal port type   |
|  [02]   | [TOPOLOGY]   | the `iac` config seam, health rows, routing assembly, the `flow` engine bridge      |

## [2]-[FENCE_ROWS]

[FENCE_ROWS]:
- Owner: `Fence` — the registration policy vocabulary. Rows are keyed by service class — `interactive` (small mailbox, wide concurrency, short residency), `steady` (the workhorse defaults), `bulk` (deep mailbox, narrow concurrency, long residency) — and each row carries exactly the bounds `entity.toLayer` accepts: `mailboxCapacity`, `concurrency`, `maxIdleTime`. `Fence.options(kind)` projects a row as the registration options record, and `Fence.group(context)` derives the shard-group name from the kernel `TenantContext` scope so one tenant's shards partition away from another's.
- Law: every registration is fenced — the entry spelling composes annotation and bounds in one chain, `entity.annotateRpcs(ClusterSchema.Persisted, durable).toLayer(handlers, Fence.options(kind))` — so a tenant saturating its mailbox surfaces `MailboxFull` without starving a sibling, and an unfenced `.toLayer({})` is the review-visible defect; the package registration surface is used directly, because a wrapper renaming `toLayer` would forward without adding law.
- Law: tenant partition is the `ClusterSchema.ShardGroup` annotation fed `Fence.group` — the group derives from `TenantContext.scope`, the same branded partition key `store` scopes and `security` claims align on, so shard placement, storage tenancy, and entitlement speak one spelling.
- Law: the refusal surface is a type this folder exports — `Fence.Refusal` is the `MailboxFull | AlreadyProcessingMessage` union, so `edge/hook` types its fenced-quota admission against the work-owned port on the type plane and never imports `@effect/cluster`; `Storage.classify` already folds both tags (`exhausted`, `conflicted`), so admission backpressure inherits kernel-budget retry semantics for free.
- Law: message contracts are closed Schema families — an entity's Rpc payload, success, and error schemas are the wire contract, `Entity.HandlersFrom` makes the handler record exhaustive at compile time, and a new message is one Rpc row plus one handler row; out-of-band replies ride `Entity.Replier` (`succeed`/`fail`/`complete`) from the `toLayerMailbox` form when a streaming entity drains its own mailbox.
- Law: an entity-held resource that must survive shard-move restarts rides `EntityResource.make({ acquire, idleTimeToLive })` — released on idle or explicit close, never a module-level handle; per-key ordered processing is this mailbox's law, which is why `queue/job.md` families stay unordered.
- Boundary: `Entity.make`/`Entity.fromRpcGroup` consume `@effect/rpc` protocol values — the admission of that package inside `scope:work` is a ledger row the platform decision must carry; durability annotation semantics are `engine/storage.md`'s; the quota-port consumer is `edge/hook`.
- Entry: `entity.annotateRpcs(ClusterSchema.Persisted, durable).toLayer(handlers, Fence.options(kind))`; callers reach the actor through `entity.client` and route refusals with `catchTag`.
- Growth: a new service class is one row; a new bound axis (a per-tenant rate, a defect-retry policy) is one field on `Row` flowing into `options`.
- Packages: `@effect/cluster` (`ClusterError`, `ClusterSchema`, `Entity`, `EntityResource`), `effect` (`Duration`, `Types`), `@rasm/ts/kernel` (`TenantContext`).

```typescript
import type { ClusterError } from "@effect/cluster"
import type { TenantContext } from "@rasm/ts/kernel"
import { Duration, type Types } from "effect"

const _kinds = ["interactive", "steady", "bulk"] as const
const _rows = {
  interactive: { mailboxCapacity: 64, concurrency: 16, maxIdleTime: Duration.minutes(2) },
  steady: { mailboxCapacity: 256, concurrency: 8, maxIdleTime: Duration.minutes(10) },
  bulk: { mailboxCapacity: 1024, concurrency: 2, maxIdleTime: Duration.hours(1) },
} as const

declare namespace Fence {
  type Kinds = typeof _kinds
  type Kind = keyof typeof _rows
  type Row = {
    readonly mailboxCapacity: number
    readonly concurrency: number
    readonly maxIdleTime: Duration.Duration
  }
  type Contract = { readonly [K in Kinds[number]]: Row }
  type Refusal = ClusterError.AlreadyProcessingMessage | ClusterError.MailboxFull
  type Shape = Types.Simplify<
    typeof _rows & {
      readonly kinds: Kinds
      readonly options: <K extends Kind>(kind: K) => (typeof _rows)[K]
      readonly group: (context: TenantContext) => string
    }
  >
  type _Rows<T extends Contract = typeof _rows> = T
  type _Keys<K extends keyof Contract = Kind> = K
}

const Fence: Fence.Shape = {
  ..._rows,
  kinds: _kinds,
  options: (kind) => _rows[kind],
  group: (context) => context.scope,
}
```

## [3]-[TOPOLOGY]

[TOPOLOGY]:
- Owner: `Engine` — the assembled engine rows the composition root selects. `topology` is `ShardingConfig.layerFromEnv`, the sole `iac ↔ work` meeting: the Pulumi `StackOutputs` land as environment the config layer reads, so runner address, weight, shard groups, and lock intervals are deployment facts, never source. `health` rows select liveness — `k8s` (`RunnerHealth.layerK8s` over `K8sHttpClient`, pod discovery only), `ping` (`RunnerHealth.layerPing`), `noop` for the single-node lane. `routing` is `Sharding.layer`, the service that registers entities and routes messages over `ShardingConfig | Runners | MessageStorage`. `workflow` is `ClusterWorkflowEngine.layer`, satisfying the `@effect/workflow` `WorkflowEngine` Tag over `Sharding` and `MessageStorage` so every `flow` definition runs durable and sharded by Layer selection.
- Law: runner entrypoint transports are root data — `HttpRunner.layerHttp`, `SocketRunner.layer`, and `SingleRunner.layer` are binding-tier Layers the app root selects beside the `host/exec` runtime row, and `TestRunner` is the kit lane; this module names none of them, which is what keeps `work` free of every `platform-node`/`-bun` import.
- Law: discovery is never provisioning — `K8sHttpClient` reads pods through the injected `HttpClient` and service-account mount; creating, scaling, and tearing runners is `iac`'s plane, and a provisioning verb spelled through the discovery client is the named breach.
- Law: cluster-wide singletons and durable crons ride this assembly — `Singleton.make(name, run)` fences one instance across shards (the `deliver/relay.md` drain is the standing consumer) and `ClusterCron` rows register through the same `Sharding` service (`queue/schedule.md` owns their vocabulary); both are rows over the engine, never sibling runtimes.
- Law: an entity crosses to the public edge only by projection — `EntityProxy.toHttpApiGroup`/`toRpcGroup` re-expose a protocol as an `edge` contribution group with the typed client derived, so the actor's contract and its public surface cannot drift.
- Boundary: storage rows and the fault fold are `engine/storage.md`'s; the runtime row table is `host/exec`'s; stack outputs are `iac`'s; this page owns the engine's own composition rows.
- Entry: `Layer.mergeAll(Engine.routing, Engine.workflow, Engine.health.k8s).pipe(Layer.provide(Engine.topology()))` at the composition root, beside the selected storage row and runner transport.
- Growth: a new health probe is one row; a new engine-level bridge is one member on the owner.
- Packages: `@effect/cluster` (`ClusterWorkflowEngine`, `RunnerHealth`, `Sharding`, `ShardingConfig`), `effect` (`Types`).

```typescript
import { ClusterWorkflowEngine, RunnerHealth, Sharding, ShardingConfig } from "@effect/cluster"

const _health = {
  k8s: RunnerHealth.layerK8s(),
  ping: RunnerHealth.layerPing,
  noop: RunnerHealth.layerNoop,
} as const

declare namespace Engine {
  type Health = keyof typeof _health
  type Shape = Types.Simplify<{
    readonly health: typeof _health
    readonly routing: typeof Sharding.layer
    readonly topology: typeof ShardingConfig.layerFromEnv
    readonly workflow: typeof ClusterWorkflowEngine.layer
  }>
  type _Probes<T extends Record<Health, unknown> = typeof _health> = T
}

const Engine: Engine.Shape = {
  health: _health,
  routing: Sharding.layer,
  topology: ShardingConfig.layerFromEnv,
  workflow: ClusterWorkflowEngine.layer,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Engine, Fence }
```
