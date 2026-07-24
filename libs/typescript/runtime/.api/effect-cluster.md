# [TS_RUNTIME_API_EFFECT_CLUSTER]

`@effect/cluster` is the durable-actor runtime `work/entity` composes: an `Entity` gives an `@effect/rpc` `RpcGroup` sharded, persistent, singleton-per-id identity; `Sharding` routes messages to runners; `MessageStorage` replays them for at-least-once, deduped delivery. Message durability is a per-Rpc `ClusterSchema.Persisted` annotation, faults are the closed `ClusterError` family, and `SqlMessageStorage.layer` binds storage to the `store`-owned `SqlClient` at the app root.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/cluster`
- package: `@effect/cluster` (MIT)
- module: ESM; one subpath export per cluster concern
- runtime: node/bun durable lanes — runners ride `@effect/platform` `Socket`/`HttpClient`/`FileSystem`, storage rides a `store`-owned `@effect/sql` driver; no browser lane
- rail: durable-actor — the sharded persistent-actor + message-replay engine
- depends: `kubernetes-types` (the K8s pod shapes `K8sHttpClient` discovery types against)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the `Entity` + `Sharding` durable-actor core
- An `Entity` binds an `@effect/rpc` `RpcGroup` to a sharded per-id-singleton actor; `Sharding` registers entities, assigns ids to shards, and routes messages. `work/entity` builds one entity per durable-actor family.

| [INDEX] | [SYMBOL]                                                            | [TYPE_FAMILY]       | [CAPABILITY]                               |
| :-----: | :------------------------------------------------------------------ | :------------------ | :----------------------------------------- |
|  [01]   | `Entity.Entity<Type, Rpcs>`                                         | actor def           | `work/entity`; `.toLayer` registers        |
|  [02]   | `Entity.Replier<Rpcs>` (`succeed`/`fail`/`failCause`/`complete`)    | reply surface       | out-of-band reply; `complete` takes `Exit` |
|  [03]   | `Entity.Request<Rpc>` / `Entity.HandlersFrom<Rpcs>`                 | handler contract    | envelope + exhaustive handler map          |
|  [04]   | `Entity.CurrentAddress` / `Entity.CurrentRunnerAddress`             | context Tag         | entity + host address in a handler         |
|  [05]   | `EntityResource.EntityResource<A, E>` / `EntityResource.CloseScope` | entity resource     | resource surviving shard-move restart      |
|  [06]   | `Sharding.Sharding`                                                 | routing service Tag | register, assign, route, send              |
|  [07]   | `ShardingRegistrationEvent.ShardingRegistrationEvent`               | registration ADT    | registration event stream                  |
|  [08]   | `ShardingConfig.ShardingConfig`                                     | topology config     | address/weight, shard groups, budgets      |
|  [09]   | `EntityType.EntityType` / `SingletonAddress.SingletonAddress`       | identity            | branded type + `{shardId,name}` address    |
|  [10]   | `Snowflake.Snowflake` / `MachineId.MachineId`                       | distributed id      | message id; `Schema` codec + generator     |
|  [11]   | `DeliverAt.DeliverAt`                                               | delivery policy     | `work/deliver` — `toMillis` delivery time  |

- `EntityResource`: `get`/`close` a resource acquired inside an entity, released on idle TTL or `close`; `CloseScope` the explicit-close scope Tag.
- `Sharding.Sharding`: `registerEntity`/`registerSingleton`/`makeClient`/`send`/`notify`/`reset`/`activeEntityCount`.
- `ShardingRegistrationEvent`: `EntityRegistered` \| `SingletonRegistered`, streamed by `Sharding.getRegistrationEvents`, folded with `ShardingRegistrationEvent.match`.
- [IDENTITY]: `EntityAddress` `EntityId` `ShardId` `RunnerAddress` — the branded id + address family behind `EntityType`/`SingletonAddress`.

[PUBLIC_TYPE_SCOPE]: message storage, annotations, and the fault family
- `MessageStorage` is the persist-and-replay boundary (at-least-once + dedupe via `SaveResult`); `ClusterSchema` annotations mark per-Rpc durability, interruptibility, and shard-group; `ClusterError` is the closed typed-fault family every cluster op fails into.

| [INDEX] | [SYMBOL]                                                                               | [TYPE_FAMILY]         | [CAPABILITY]         |
| :-----: | :------------------------------------------------------------------------------------- | :-------------------- | :------------------- |
|  [01]   | `MessageStorage.MessageStorage`                                                        | storage Tag           | persist + replay     |
|  [02]   | `MessageStorage.SaveResult` = `Success` \| `Duplicate`                                 | dedupe result         | dedup verdict        |
|  [03]   | `ClusterSchema.Persisted`                                                              | durability annotation | durability switch    |
|  [04]   | `ClusterSchema.Uninterruptible` / `ShardGroup` / `ClientTracingEnabled`                | Rpc policy            | interrupt/shard/span |
|  [05]   | `ClusterError.MailboxFull` / `AlreadyProcessingMessage`                                | backpressure fault    | mailbox + in-flight  |
|  [06]   | `ClusterError.PersistenceError` / `MalformedMessage`                                   | storage fault         | storage/decode fault |
|  [07]   | `ClusterError.EntityNotAssignedToRunner` / `RunnerNotRegistered` / `RunnerUnavailable` | routing fault         | retry / re-route     |

- `SaveResult`: at-least-once dedup verdict keyed on `Snowflake` + primary key.
- `ClusterSchema` policy: interrupt policy (`boolean`\|`"client"`\|`"server"`), per-tenant shard-group fn, client-span toggle.
- [WIRE]: `Message` `Envelope` `Reply` — the persisted message wire types; `ClusterMetrics` the cluster telemetry surface.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: define an entity + register it with sharding
- `Entity.make`/`fromRpcGroup` declares the actor from its RPC protocol; `.toLayer(handlers, options)` registers it and bounds its mailbox/concurrency/idle-time; `.client` yields the typed per-id client.

| [INDEX] | [SURFACE]                                                                                     | [ENTRY_FAMILY]  | [CAPABILITY]          |
| :-----: | :-------------------------------------------------------------------------------------------- | :-------------- | :-------------------- |
|  [01]   | `Entity.make(type, rpcs)` / `Entity.fromRpcGroup(type, rpcGroup)`                             | declare         | from RPC protocol     |
|  [02]   | `entity.toLayer(handlers, options)`                                                           | register        | fenced entity `Layer` |
|  [03]   | `entity.toLayerMailbox((mailbox, replier) => Effect<never>, options)`                         | register (raw)  | mailbox-draining      |
|  [04]   | `entity.client` — `Effect<(entityId) => RpcClient, never, Sharding>`                          | invoke          | typed per-id client   |
|  [05]   | `entity.annotateRpcs(ClusterSchema.Persisted, true)` / `.annotate(...)`                       | annotate        | durability / shard    |
|  [06]   | `Entity.makeTestClient(entity, layer)` / `Entity.keepAlive(enabled)`                          | test / lifetime | in-proc test client   |
|  [07]   | `EntityResource.make({ acquire, idleTimeToLive? })` / `makeK8sPod(spec, { idleTimeToLive? })` | resource        | entity-held; K8s pod  |

- `entity.toLayer` `options`: `maxIdleTime`, `concurrency`, `mailboxCapacity`, `disableFatalDefects`, `defectRetryPolicy`, `spanAttributes` — `mailboxCapacity`/`concurrency` the fenced-quota bounds.
- `entity.client` faults are `MailboxFull`/`AlreadyProcessingMessage`/`PersistenceError`; `makeK8sPod` the pod form over `K8sHttpClient`, kept alive across restarts, released on idle TTL or `close`.

[ENTRYPOINT_SCOPE]: message storage + sharding config layers
- `SqlMessageStorage.layer` is durable storage over the store `SqlClient`; `ShardingConfig.layerFromEnv` reads runner topology from env/`Config` (the `iac/stack` seam); `Sharding.layer` composes the routing service.

| [INDEX] | [SURFACE]                                                                                     | [ENTRY_FAMILY] | [CAPABILITY]      |
| :-----: | :-------------------------------------------------------------------------------------------- | :------------- | :---------------- |
|  [01]   | `SqlMessageStorage.layer: Layer<MessageStorage, never, SqlClient \| ShardingConfig>`          | storage        | durable store     |
|  [02]   | `MessageStorage.layerMemory` / `layerNoop`                                                    | storage (spec) | memory / no-op    |
|  [03]   | `ShardingConfig.layerFromEnv(overrides?)` / `.layer(options?)` / `.config` / `.configFromEnv` | topology       | env runner config |
|  [04]   | `Sharding.layer` — `Layer<Sharding, never, ShardingConfig \| Runners \| MessageStorage \| …>` | routing        | routing service   |
|  [05]   | `SqlRunnerStorage.layer` / `RunnerStorage.layerMemory`                                        | runner store   | durable / memory  |

[ENTRYPOINT_SCOPE]: runners, health discovery, cron, singleton, and the workflow bridge
- Runner entrypoint Layers selected via `proc/exec` at the app root, K8s discovery, the durable scheduled job, the cluster singleton, and the `@effect/workflow` bridge.

| [INDEX] | [SURFACE]                                                                                           | [ENTRY_FAMILY]  | [CAPABILITY]  |
| :-----: | :-------------------------------------------------------------------------------------------------- | :-------------- | :------------ |
|  [01]   | `HttpRunner.layerHttp` / `SocketRunner.layer` / `SingleRunner.layer` / `TestRunner`                 | runner          | entrypoint    |
|  [02]   | `HttpRunner.layerWebsocket`                                                                         | runner          | websocket     |
|  [03]   | `HttpRunner.layerHttpClientOnly` / `layerWebsocketClientOnly`                                       | runner          | client-only   |
|  [04]   | `HttpRunner.layerHttpOptions({ path })` / `layerWebsocketOptions({ path })`                         | runner          | router mount  |
|  [05]   | `HttpRunner.layerClient` / `layerClientProtocolHttpDefault` / `layerClientProtocolWebsocketDefault` | runner client   | client stack  |
|  [06]   | `HttpRunner.layerClientProtocolHttp` / `layerClientProtocolWebsocket({ path, https? })`             | runner client   | RPC transport |
|  [07]   | `HttpRunner.toHttpEffect`                                                                           | runner client   | as `HttpApp`  |
|  [08]   | `RunnerServer.layer` / `layerWithClients` / `layerClientOnly` / `layerHandlers`                     | runner server   | RPC server    |
|  [09]   | `RunnerHealth.layerK8s(options?)` / `layerPing` / `layerNoop`                                       | discovery       | health        |
|  [10]   | `K8sHttpClient.layer` / `makeGetPods`                                                               | discovery       | K8s discovery |
|  [11]   | `ClusterCron.make({ name, cron, execute, ... })`                                                    | scheduled job   | durable cron  |
|  [12]   | `Singleton.make(name, run, options?)`                                                               | singleton       | drains outbox |
|  [13]   | `EntityProxy.toRpcGroup(entity)` / `toHttpApiGroup(name, entity)`                                   | wire expose     | `edge` group  |
|  [14]   | `ClusterWorkflowEngine.layer`                                                                       | workflow bridge | engine bridge |

- `RunnerServer.layer` family: the message-receiving RPC server over `RpcServer.Protocol`; `layerWithClients` bundles the `Runners`+`Sharding` clients, `layerClientOnly` embeds a cluster client without shard assignment.
- `K8sHttpClient.layer`: `Layer<K8sHttpClient, never, HttpClient | FileSystem>` over the service-account token mount, discovery never provisioning.
- `ClusterCron.make`: `shardGroup?`/`calculateNextRunFromPrevious?`/`skipIfOlderThan?` set the misfire + window policy; `ClusterWorkflowEngine.layer` is `Layer<WorkflowEngine, never, Sharding | MessageStorage>`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- An entity is an RPC group with durable identity: `Entity.make` binds an `@effect/rpc` `RpcGroup`'s payload/success/error `Schema`s as the message contract to a sharded per-id singleton, and `.toLayer(handlers)` registers the exhaustively-checked handler map with `Sharding`. `work/entity` codes the actor, never the mailbox or shard math.
- Storage is a swappable port: `MessageStorage` is a Tag with `layerMemory`/`layerNoop` for specs and `SqlMessageStorage.layer` for durable, and `work/entity` composes the Tag while the `store`-owned SQL driver satisfies `SqlClient` at the app root. Message durability is the per-Rpc `ClusterSchema.Persisted` annotation; dedupe is `SaveResult.Duplicate` keyed on `Snowflake` + primary key — at-least-once, exactly-once-effect.
- Topology is config, discovery is a Layer: `ShardingConfig.layerFromEnv` reads runner address/weight, shard groups, lock intervals, and entity mailbox/idle/timeout budgets from env/`Config` — the `iac/stack` `StackOutputs → ShardingConfig` `[SHAPE]` seam. Runner discovery is `RunnerHealth.layerK8s` over `K8sHttpClient`, and the runner entrypoint is selected via `proc/exec` at the app root, so `work` imports no `platform-node/bun` binding.
- Fenced quotas are entity bounds and shard groups: `mailboxCapacity`/`concurrency` on `.toLayer` and a `ClusterSchema.ShardGroup` function partition ids per tenant, so a tenant's mailbox saturates to `MailboxFull` without starving another. `edge/hook` types its fenced-quota port against these faults.
- Faults are the closed `ClusterError` family: every cluster op fails into a `Schema.TaggedError` routed through the `Effect` error channel with `catchTag`, so a routing fault (`EntityNotAssignedToRunner`) drives re-route/retry rather than a crash.

[STACKING]:
- `@effect/rpc` (`runtime/.api/effect-rpc.md`): an `Entity` IS an `RpcGroup` — the message protocol, handler exhaustiveness, and the typed `.client` are the RPC surface, and `EntityProxy.toRpcGroup`/`toHttpApiGroup` re-exposes an entity as a `serve` contribution group with its typed client for free.
- `@effect/sql` (`data/.api/effect-sql.md`): `SqlMessageStorage.layer`/`SqlRunnerStorage.layer` bind message + runner storage to the `SqlClient` Tag, which the `data`-owned driver (`effect-sql-pg`/`-sqlite-*`) satisfies at the app root; `work` composes the Tag as a `[PORT]`.
- `@effect/platform` (`.api/effect-platform.md`): runners ride `Socket`/`HttpClient`, `K8sHttpClient` rides `HttpClient` + `FileSystem` (the service-account token mount), and `HttpRunner.toHttpEffect` mounts the runner server as an `HttpApp`; the `platform-node`/`-bun` runtime binding is an app-root Layer selection.
- `@effect/workflow` (`runtime/.api/effect-workflow.md`): `ClusterWorkflowEngine.layer` satisfies the `WorkflowEngine` Tag over `Sharding` + `MessageStorage`, so durable workflows run sharded on this runtime — `work/flow` defines the workflow, `work/entity` binds the engine.
- `effect` (`.api/effect.md`): `ClusterCron`'s `cron` is an `effect/Cron`, `Snowflake` decodes through `Schema` and derives `DateTime`, the `ClusterError` family is `Schema.TaggedError`, and `Sharding.layer`/`SqlMessageStorage.layer` compose as `Layer`s; `Singleton.make` cluster-fences the same effect the app runs elsewhere to one instance.
- `iac/stack` (`iac`): `ShardingConfig.layerFromEnv` reads deployment topology — the sole `iac ↔ work` meeting, a `[SHAPE]` seam, never an `iac` import.

[LOCAL_ADMISSION]:
- Define an entity as an `@effect/rpc` `RpcGroup` via `Entity.make`/`fromRpcGroup`, the mailbox, shard assigner, and message loop owned by the runtime.
- Compose `MessageStorage` and `SqlClient` as Tags satisfied at the app root, the `store` SQL driver staying outside `work`.
- Mark durability with `ClusterSchema.Persisted` and partition tenants with `ShardGroup`, every entity bounded by `mailboxCapacity`/`concurrency`/`maxIdleTime` on `.toLayer`.
- Route faults through the `ClusterError` `catchTag` rail, and select the runner entrypoint at the app root via `proc/exec`, `K8sHttpClient` reserved for discovery.

[RAIL_LAW]:
- Package: `@effect/cluster`
- Owns: the `Entity` durable-actor definition and `Sharding` routing, the `MessageStorage`/`SqlMessageStorage` persist-and-replay boundary, the `ClusterSchema` durability/interrupt/shard-group annotations, the closed `ClusterError` fault family, `ShardingConfig` topology, the runner family (`HttpRunner`/`SocketRunner`/`SingleRunner`/`TestRunner`, `RunnerHealth`, `RunnerStorage`), `K8sHttpClient` discovery, `ClusterCron`, `Singleton`, `Snowflake`/`DeliverAt`, `EntityProxy`/`EntityProxyServer` wire exposure, and the `ClusterWorkflowEngine` bridge
- Accept: an entity as an `@effect/rpc` `RpcGroup` with `.toLayer` fenced bounds, `MessageStorage`/`SqlClient` as app-root-satisfied Tags, `ClusterSchema.Persisted`/`ShardGroup` for durability + tenant fencing, `ClusterError` `catchTag` handling, `ShardingConfig.layerFromEnv` from the `iac` seam, runner Layers selected via `proc/exec`, `K8sHttpClient` for discovery, `ClusterWorkflowEngine.layer` bridging `@effect/workflow`
- Reject: a hand-rolled actor mailbox, shard assigner, or message-replay store; a `store` SQL-driver import inside `work`; unconditional message persistence or an unfenced entity; a thrown cluster fault; a `platform-node/bun` binding imported into a `work` page; `K8sHttpClient` used for provisioning
