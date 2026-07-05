# [@effect/cluster] — cluster Entities, sharding, runner discovery, and `MessageStorage`; the in-process durable-actor engine

`@effect/cluster` is the durable-actor runtime `work/entity` composes: an `Entity` is an `@effect/rpc` `RpcGroup` given sharded, persistent, singleton-per-id identity; `Sharding` assigns entity ids to runners and routes messages; `MessageStorage` persists and replays messages for at-least-once delivery with dedupe; and the runner family (`HttpRunner`/`SocketRunner`/`SingleRunner`, `RunnerHealth`, `K8sHttpClient`) discovers and connects the cluster nodes. Every message is a Schema-typed RPC whose durability is a per-Rpc `ClusterSchema.Persisted` annotation, whose faults are the closed `ClusterError` `Schema.TaggedError` family, and whose storage is the swappable `MessageStorage` Tag — `SqlMessageStorage.layer` binds it to the `store`-owned `SqlClient` driver at the app root, so `work` composes the Tag as a `[PORT]` and never imports `store`. `ClusterCron` is the durable scheduled job, `Singleton` the cluster-wide singleton, and `ClusterWorkflowEngine.layer` the bridge that lets `@effect/workflow` run durable and sharded on this runtime.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/cluster`
- package: `@effect/cluster`
- version: `0.59.0`
- license: `MIT`
- effect-peer: `effect ^3.21.x`, `@effect/platform ^0.96.x`, `@effect/rpc ^0.75.x`, `@effect/sql ^0.51.x`, `@effect/workflow ^0.18.x` (universal-tier + sibling substrate; `.api/effect.md`, `.api/effect-platform.md`, `work/.api/effect-workflow.md`)
- deps: `kubernetes-types` (the K8s pod shapes `K8sHttpClient` discovery types against)
- runtime: node/bun durable lanes — runners use `@effect/platform` `Socket`/`HttpClient`/`FileSystem`; `MessageStorage`/`RunnerStorage` persist on `@effect/sql` (a store-owned driver); no browser lane
- catalog-verdict: KEEP — the one durable-actor + sharding engine; a hand-rolled actor mailbox, shard assigner, or message-replay store is the named reinvention defect
- port-law: `MessageStorage` and `SqlClient` are Tags `work` composes; the `store`-owned SQL driver Layer satisfies them at the app root — the `-pg`/`-sqlite-*` drivers stay banned outside `store`
- modules: `Entity`, `EntityResource`, `Sharding`, `ShardingConfig`, `ShardingRegistrationEvent`, `MessageStorage`, `SqlMessageStorage`, `ClusterSchema`, `ClusterError`, `ClusterCron`, `ClusterWorkflowEngine`, `Singleton`, `EntityProxy`/`EntityProxyServer`, `Runner`/`Runners`, `RunnerHealth`, `RunnerServer`, `RunnerStorage`/`SqlRunnerStorage`, `HttpRunner`/`SocketRunner`/`SingleRunner`/`TestRunner`, `K8sHttpClient`, `Snowflake`/`MachineId`, `DeliverAt`, `Message`/`Envelope`/`Reply`, `ClusterMetrics`, and the `EntityAddress`/`EntityId`/`EntityType`/`ShardId`/`RunnerAddress`/`SingletonAddress` identity family

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the `Entity` + `Sharding` durable-actor core
- rail: durable-actor
- An `Entity` binds an `@effect/rpc` `RpcGroup` (its message protocol) to a sharded, per-id-singleton actor; `Sharding` is the service that registers entities, assigns ids to shards, and routes messages. `work/entity` builds one entity per durable-actor family.

| [INDEX] | [SYMBOL]                                                     | [TYPE_FAMILY]   | [CONSUMER / BOUNDARY]                                          |
| :-----: | :--------------------------------------------------------- | :-------------- | :------------------------------------------------------------- |
|  [01]   | `Entity.Entity<Type, Rpcs>`                                | actor def       | `work/entity` — one entity per durable-actor family; `.toLayer(handlers)` registers it |
|  [02]   | `Entity.Replier<Rpcs>` (`succeed`/`fail`/`failCause`/`complete`) | reply surface | streaming/mailbox handlers reply out-of-band; `complete` takes an `Exit` |
|  [03]   | `Entity.Request<Rpc>` / `Entity.HandlersFrom<Rpcs>`       | handler contract | the per-message envelope + the exhaustive handler map the entity checks |
|  [04]   | `Entity.CurrentAddress` / `Entity.CurrentRunnerAddress`   | context Tag     | the running entity's `EntityAddress` and its host `RunnerAddress` inside a handler |
|  [05]   | `EntityResource.EntityResource<A, E>` / `EntityResource.CloseScope` | entity resource | `get`/`close` handle acquired inside an entity; survives shard-move restarts, released on idle TTL or `close`; `CloseScope` the explicit-close scope Tag |
|  [06]   | `Sharding.Sharding`                                        | routing service Tag | `registerEntity`/`registerSingleton`/`makeClient`/`send`/`notify`/`reset`/`activeEntityCount` |
|  [07]   | `ShardingRegistrationEvent.ShardingRegistrationEvent` = `EntityRegistered` \| `SingletonRegistered` | registration ADT | streamed by `Sharding.getRegistrationEvents`; fold with `ShardingRegistrationEvent.match` |
|  [08]   | `ShardingConfig.ShardingConfig`                           | topology config | runner address/weight, shard groups, lock intervals, entity mailbox/idle/timeout budgets |
|  [09]   | `EntityType.EntityType` / `SingletonAddress.SingletonAddress` | identity        | the branded entity-type string + the `{ shardId, name }` singleton address (`Equal`/`Hash` `Schema.Class`) |
|  [10]   | `Snowflake.Snowflake` / `MachineId.MachineId`             | distributed id  | the message/request id (timestamp+machineId+sequence); `Schema` codecs + a `layerGenerator` |
|  [11]   | `DeliverAt.DeliverAt`                                      | delivery policy | a payload's `toMillis` delivery time — the scheduled/delayed-delivery + egress-quota interface `work/deliver` reads |

[PUBLIC_TYPE_SCOPE]: message storage, annotations, and the fault family
- rail: durable-actor/rails
- `MessageStorage` is the persist-and-replay boundary (at-least-once + dedupe via `SaveResult`); `ClusterSchema` annotations mark per-Rpc durability/interruptibility/shard-group/tracing; `ClusterError` is the closed typed-fault family every cluster op fails into.

| [INDEX] | [SYMBOL]                                                     | [TYPE_FAMILY]   | [CONSUMER / BOUNDARY]                                          |
| :-----: | :--------------------------------------------------------- | :-------------- | :------------------------------------------------------------- |
|  [01]   | `MessageStorage.MessageStorage`                            | storage Tag     | `work/entity` — `saveRequest`/`saveReply`/`unprocessedMessages`/`repliesFor`; satisfied by `SqlMessageStorage.layer` |
|  [02]   | `MessageStorage.SaveResult` = `Success` \| `Duplicate`    | dedupe result   | the at-least-once dedup verdict keyed on `Snowflake` + primary key |
|  [03]   | `ClusterSchema.Persisted`                                  | durability annotation | mark an Rpc's messages saved + replayed; the durable-vs-ephemeral message switch |
|  [04]   | `ClusterSchema.Uninterruptible` / `ShardGroup` / `ClientTracingEnabled` | Rpc policy | interrupt policy (`boolean`\|`"client"`\|`"server"`), per-tenant shard-group fn, client-span toggle |
|  [05]   | `ClusterError.MailboxFull` / `AlreadyProcessingMessage`   | backpressure fault | the mailbox-capacity + in-flight-dedup faults `edge/hook` fenced-quota rows read |
|  [06]   | `ClusterError.PersistenceError` / `MalformedMessage`      | storage fault   | the `MessageStorage` failure rail + the decode-failure fault |
|  [07]   | `ClusterError.EntityNotAssignedToRunner` / `RunnerNotRegistered` / `RunnerUnavailable` | routing fault | the shard-reassignment + runner-down faults driving retry/re-route |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: define an entity + register it with sharding
- rail: durable-actor
- `Entity.make`/`fromRpcGroup` declares the actor from its RPC protocol; `.toLayer(handlers, options)` registers it and bounds its mailbox/concurrency/idle-time (the per-tenant fenced-quota rows). `.client` yields the typed per-id client.

| [INDEX] | [SURFACE]                                                                                       | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                     |
| :-----: | :---------------------------------------------------------------------------------------------- | :------------- | :-------------------------------------------------------- |
|  [01]   | `Entity.make(type, rpcs)` / `Entity.fromRpcGroup(type, rpcGroup)`                              | declare        | `work/entity` — the actor from its `@effect/rpc` protocol |
|  [02]   | `entity.toLayer(handlers, { maxIdleTime, concurrency, mailboxCapacity, disableFatalDefects, defectRetryPolicy, spanAttributes })` | register | the entity `Layer`; `mailboxCapacity`/`concurrency` are the fenced-quota bounds |
|  [03]   | `entity.toLayerMailbox((mailbox, replier) => Effect<never>, options)`                          | register (raw) | the mailbox-draining form for streaming/batched entities  |
|  [04]   | `entity.client` — `Effect<(entityId) => RpcClient, never, Sharding>`                           | invoke         | the typed per-id client; faults are `MailboxFull`/`AlreadyProcessingMessage`/`PersistenceError` |
|  [05]   | `entity.annotateRpcs(ClusterSchema.Persisted, true)` / `.annotate(...)`                        | annotate       | apply durability/shard-group/interrupt policy to the Rpcs above the call |
|  [06]   | `Entity.makeTestClient(entity, layer)` / `Entity.keepAlive(enabled)`                          | test / lifetime | in-process test client for kit-driven specs; keep an idle entity resident |
|  [07]   | `EntityResource.make({ acquire, idleTimeToLive? })` / `makeK8sPod(spec, { idleTimeToLive? })` | resource       | an entity-held resource kept alive across restarts; released on idle TTL or `close`; `makeK8sPod` the K8s pod form over `K8sHttpClient` |

[ENTRYPOINT_SCOPE]: message storage + sharding config layers
- rail: durable-actor/system
- `SqlMessageStorage.layer` is the durable storage over the store `SqlClient`; `ShardingConfig.layerFromEnv` reads the runner topology from env/`Config` (the `iac/stack` seam). `Sharding.layer` composes the routing service.

| [INDEX] | [SURFACE]                                                                                       | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                     |
| :-----: | :---------------------------------------------------------------------------------------------- | :------------- | :-------------------------------------------------------- |
|  [01]   | `SqlMessageStorage.layer: Layer<MessageStorage, never, SqlClient \| ShardingConfig>`           | storage        | `work/entity` — the durable message store; `SqlClient` satisfied by the store driver at the app root |
|  [02]   | `MessageStorage.layerMemory` / `layerNoop`                                                     | storage (spec) | in-memory / no-op storage for specs and single-node dev  |
|  [03]   | `ShardingConfig.layerFromEnv(overrides?)` / `.layer(options?)` / `.config` / `.configFromEnv`  | topology       | `iac/stack` `StackOutputs → ShardingConfig` `[SHAPE]` seam; env-driven runner config |
|  [04]   | `Sharding.layer` — `Layer<Sharding, never, ShardingConfig \| Runners \| MessageStorage \| …>`  | routing        | the composed routing service the app root wires |
|  [05]   | `SqlRunnerStorage.layer` / `RunnerStorage.layerMemory`                                          | runner store   | durable/memory runner registry backing |

[ENTRYPOINT_SCOPE]: runners, health discovery, cron, singleton, and the workflow bridge
- rail: durable-actor/composition
- The runner entrypoint Layers (selected via `proc/exec` at the app root), K8s discovery, the durable scheduled job, the cluster singleton, and the `@effect/workflow` bridge.

| [INDEX] | [SURFACE]                                                                                       | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                     |
| :-----: | :---------------------------------------------------------------------------------------------- | :------------- | :-------------------------------------------------------- |
|  [01]   | `HttpRunner.layerHttp` / `SocketRunner.layer` / `SingleRunner.layer` / `TestRunner`             | runner         | the runner entrypoint `Layer`s; `SingleRunner` single-node, `TestRunner` for kit-driven specs |
|  [02]   | `HttpRunner.layerWebsocket` / `layerHttpClientOnly` / `layerWebsocketClientOnly` / `layerHttpOptions({ path })` / `layerWebsocketOptions({ path })` | runner | the websocket-transport, client-only-embed, and mount-on-an-existing-`HttpRouter` forms of the HTTP runner |
|  [03]   | `HttpRunner.layerClient` / `layerClientProtocolHttp({ path, https? })` / `layerClientProtocolHttpDefault` / `layerClientProtocolWebsocket({ path, https? })` / `layerClientProtocolWebsocketDefault` / `toHttpEffect` | runner client | the inter-runner RPC transport; `layerClient` the client stack over a supplied `RpcClientProtocol`, `*Default` the fixed-path forms; `toHttpEffect` mounts the runner server as an `HttpApp` |
|  [04]   | `RunnerServer.layer` / `layerWithClients` / `layerClientOnly` / `layerHandlers`                 | runner server  | the message-receiving RPC server over `RpcServer.Protocol`; `layerWithClients` bundles the `Runners`+`Sharding` clients, `layerClientOnly` embeds a cluster client without shard assignment |
|  [05]   | `RunnerHealth.layerK8s(options?)` / `layerPing` / `layerNoop`                                   | discovery      | health/liveness; `layerK8s` discovers runners via `K8sHttpClient` |
|  [06]   | `K8sHttpClient.layer` — `Layer<K8sHttpClient, never, HttpClient \| FileSystem>` / `makeGetPods` | discovery      | `work/entity` — K8s pod discovery, NEVER provisioning |
|  [07]   | `ClusterCron.make({ name, cron, execute, shardGroup?, calculateNextRunFromPrevious?, skipIfOlderThan? })` | scheduled job | `work/schedule` — the durable cron; misfire/window policy as fields |
|  [08]   | `Singleton.make(name, run, options?)`                                                          | singleton      | `work/deliver` — one instance across shards drains the outbox |
|  [09]   | `EntityProxy.toRpcGroup(entity)` / `toHttpApiGroup(name, entity)`                               | wire expose    | an entity → an `@effect/rpc` `RpcGroup` / `@effect/platform` `HttpApiGroup` for `edge` |
|  [10]   | `ClusterWorkflowEngine.layer: Layer<WorkflowEngine, never, Sharding \| MessageStorage>`         | workflow bridge | satisfies `@effect/workflow`'s `WorkflowEngine` Tag — durable sharded workflows |

## [04]-[IMPLEMENTATION_LAW]

[CLUSTER_TOPOLOGY]:
- an entity is an RPC group with durable identity: `Entity.make(type, rpcs)` binds an `@effect/rpc` `RpcGroup` to a sharded per-id singleton. The RPC protocol's payload/success/error `Schema`s ARE the message contract; `.toLayer(handlers)` registers the handler map (checked exhaustively) with `Sharding`. `work/entity` codes the actor, never the mailbox or shard math.
- storage is a swappable port: `MessageStorage` is a Tag with `layerMemory`/`layerNoop` (spec) and `SqlMessageStorage.layer` (durable, over `SqlClient` + `ShardingConfig`). `work/entity` composes the Tag; the `store`-owned SQL driver satisfies `SqlClient` at the app root — `work` never imports a `-pg`/`-sqlite-*` driver. Durability of a given message is the per-Rpc `ClusterSchema.Persisted` annotation, and dedupe is `SaveResult.Duplicate` keyed on `Snowflake` + primary key (at-least-once, exactly-once-effect).
- topology is config, discovery is a Layer: `ShardingConfig.layerFromEnv` reads runner address/weight, shard groups, lock intervals, and the entity mailbox/idle/timeout budgets from env/`Config` — the `iac/stack` `StackOutputs → ShardingConfig` `[SHAPE]` seam. Runner discovery is `RunnerHealth.layerK8s` over `K8sHttpClient` (`HttpClient` + `FileSystem`), discovery-only. The runner entrypoint (`HttpRunner.layerHttp`/`SocketRunner.layer`/`SingleRunner`) is selected via `proc/exec` at the app root — `work` imports no `platform-node/bun` binding.
- fenced quotas are entity bounds + shard groups: the per-tenant fenced-quota rows are `mailboxCapacity`/`concurrency` on `.toLayer` plus a `ClusterSchema.ShardGroup` function partitioning ids per tenant — a tenant's mailbox saturates to `MailboxFull` without starving another. `edge/hook` types its fenced-quota port against these faults.
- faults are the closed `ClusterError` family: `MailboxFull`/`AlreadyProcessingMessage`/`PersistenceError`/`EntityNotAssignedToRunner`/`RunnerNotRegistered`/`RunnerUnavailable`/`MalformedMessage` are `Schema.TaggedError` — routed through the `Effect` error channel with `catchTag`, never exceptions. A routing fault (`EntityNotAssignedToRunner`) drives re-route/retry, not a crash.

[STACKS_WITH]:
- `@effect/rpc` (`runtime/.api/effect-rpc.md`): an `Entity` IS an `RpcGroup` — the message protocol, handler exhaustiveness, and the typed `.client` are the RPC surface. `EntityProxy.toRpcGroup`/`toHttpApiGroup` re-exposes an entity as a `serve` contribution group with the typed client for free.
- `@effect/sql` (`data/.api/effect-sql.md`): `SqlMessageStorage.layer`/`SqlRunnerStorage.layer` bind message + runner storage to the `SqlClient` Tag; the `data`-owned driver (`effect-sql-pg`/`-sqlite-*`) satisfies it at the app root. `work` composes the Tag as a `[PORT]` and never imports the driver.
- `@effect/platform` (`.api/effect-platform.md`): runners ride `Socket`/`HttpClient`; `K8sHttpClient` rides `HttpClient` + `FileSystem` (the service-account token mount); `HttpRunner.toHttpEffect` mounts the runner server as an `HttpApp`. The runtime binding (`platform-node`/`-bun`) is an app-root Layer selection.
- `@effect/workflow` (`runtime/.api/effect-workflow.md`): `ClusterWorkflowEngine.layer` satisfies the `WorkflowEngine` Tag over `Sharding` + `MessageStorage` — durable workflows run sharded on this runtime. `work/flow` defines the workflow; `work/entity` provides the engine.
- `effect` (`.api/effect.md`): `ClusterCron`'s `cron` is an `effect/Cron`; `Snowflake` decodes through `Schema` and derives `DateTime`; the `ClusterError` family is `Schema.TaggedError`; `Sharding.layer`/`SqlMessageStorage.layer` compose as `Layer`s. `Singleton.make` uses the same effect the app runs elsewhere, cluster-fenced to one instance.
- `iac/stack` (`iac`): `ShardingConfig.layerFromEnv` reads deployment topology — the sole `iac ↔ work` meeting, a `[SHAPE]` seam, never an `iac` import.

[LOCAL_ADMISSION]:
- Define an entity as an `@effect/rpc` `RpcGroup` via `Entity.make`/`fromRpcGroup`; never hand-roll an actor mailbox, shard assigner, or message loop.
- Compose `MessageStorage` (and `SqlClient`) as a Tag satisfied at the app root; never import a `store` SQL driver inside `work`.
- Mark durability with `ClusterSchema.Persisted` and partition tenants with `ShardGroup`; never persist every message unconditionally or leave a tenant unbounded.
- Bound each entity with `mailboxCapacity`/`concurrency`/`maxIdleTime` on `.toLayer`; never register an unfenced entity.
- Route faults through the `ClusterError` `catchTag` rail; never throw or swallow a routing/storage fault.
- Select the runner entrypoint (`HttpRunner`/`SocketRunner`/`SingleRunner`) at the app root via `proc/exec`; never import a `platform-node/bun` binding in a `work` page. Use `K8sHttpClient` for discovery only, never provisioning.

[RAIL_LAW]:
- Package: `@effect/cluster`
- Owns: the `Entity` durable-actor definition + sharded routing (`Sharding`), the `MessageStorage`/`SqlMessageStorage` persist-and-replay boundary, the `ClusterSchema` durability/interrupt/shard-group annotations, the closed `ClusterError` fault family, `ShardingConfig` topology, the runner family (`HttpRunner`/`SocketRunner`/`SingleRunner`/`TestRunner`, `RunnerHealth`, `RunnerStorage`), `K8sHttpClient` discovery, `ClusterCron` durable scheduled jobs, `Singleton`, `Snowflake`/`DeliverAt`, `EntityProxy` wire exposure, and the `ClusterWorkflowEngine` workflow bridge
- Accept: an entity as an `@effect/rpc` `RpcGroup` with `.toLayer` fenced bounds, `MessageStorage`/`SqlClient` as app-root-satisfied Tags, `ClusterSchema.Persisted`/`ShardGroup` for durability + tenant fencing, `ClusterError` `catchTag` fault handling, `ShardingConfig.layerFromEnv` from the `iac` seam, runner Layers selected via `proc/exec`, `K8sHttpClient` for discovery, `ClusterWorkflowEngine.layer` bridging `@effect/workflow`
- Reject: a hand-rolled actor mailbox/shard-assigner/message-replay store, a `store` SQL-driver import inside `work`, unconditional message persistence or an unfenced entity, a thrown cluster fault, a `platform-node/bun` binding imported into a `work` page, `K8sHttpClient` used for provisioning
