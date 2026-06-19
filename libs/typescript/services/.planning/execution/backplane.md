# [SERVICES_BACKPLANE]

The runner placement and durable-scheduling substrate beneath the cluster — `RunnerBackplane`, the four-row protocol/message-storage/runner-storage/runner-health backplane with the snowflake id source; and `ScheduledWork`, the cluster-singleton and shard-pinned durable-cron owner. The backplane is the placement and durability substrate the cluster-backed `WorkflowEngine` layers onto; the runner protocol is selected by topology row, never branched in code. This cluster crosses no .NET wire and carries no wire type.

## [1]-[INDEX]

- [1]-[RUNNER_AND_SCHEDULING]: owns the four-row runner backplane, the snowflake id source, the cluster singletons, and the shard-pinned durable cron.

## [2]-[RUNNER_AND_SCHEDULING]

- Owner: `RunnerBackplane`, the runner placement and durable storage backplane, and `ScheduledWork`, the cluster-wide singleton and durable-cron scheduling owner.
- Cases: `RunnerBackplane` owns four explicit rows rather than one Redis-or-SQL hand-wave. The runner protocol is the placement transport — the HTTP runner for production k8s topologies, the socket runner for node-to-node clusters, the single runner for one-process deployments, and the test runner for the ephemeral harness — one closed protocol vocabulary read by topology. Message storage and runner storage are the durable backing — the SQL message store and the SQL runner store over the `persistence/store#STORE_BOUNDARY` Postgres client as the production rows, with the in-memory and no-op stores as the sibling test rows. Runner health and discovery is the liveness row — ping-based health for socket and single-process clusters and the k8s health client for native runner-address resolution and pod liveness, with the no-op health as the test row. The distributed id source threading message and entity identity is the runner-unique `Snowflake.Snowflake` each draw resolves through `Sharding.getSnowflake`, the shard manager's own id source, never a parallel generator handle. `ScheduledWork` is the jobs half the request-driven cluster does not cover — a cluster singleton (`Sharding.registerSingleton`) registers an exactly-one-runner background loop (the leader-elected sweep, reconcile, and garbage-collection jobs) pinned to one shard group so it never double-runs, and a durable cron registers a shard-pinned scheduled execution with the same exactly-once contract as a workflow.
- Entry: the backplane is the placement and durability substrate beneath `ClusterEngine` (`engine#ENGINE`) — the shard configuration, the chosen runner protocol layer, the message and runner storage layers, and the runner-health layer compose into the shard manager that the cluster-backed workflow engine layers onto; the runner protocol layer is selected by topology row, never branched in code; the capture-event client-stream (`DocumentService.captureEvents`) structurally non-dialable from the browser is dialed HERE on node through the `interchange` clients; cluster telemetry — shard counts, entity counts, runner counts, and runner health — emits through the cluster metrics source the `provisioning/contract#PROVISIONING` `ObservabilityStack` collector reads.
- Packages: `@effect/cluster` for the sharding, shard-configuration, runner-protocol, message-storage, runner-storage, runner-health, k8s health-client, snowflake, singleton, and cluster-cron surfaces; `@effect/sql` and `@effect/sql-pg` for the durable stores backing through `persistence/store#STORE_BOUNDARY`; `ioredis` for the multi-node backplane and the cluster-metrics source; and `@effect/platform-node` for the driver host.
- Growth: a new runner protocol lands as one protocol-layer row; a new durable store lands as one storage-layer row; a new background loop lands as one singleton layer; a new scheduled job lands as one durable-cron layer.
- Boundary: this cluster crosses no .NET wire and carries no wire type; runner placement and durable scheduling are node-only concerns beneath the durable units, and the cluster-metrics signal reaches dashboards only through the collector; the SQL stores ride the one Postgres client `persistence/store#STORE_BOUNDARY` owns, never a second SQL surface.

```ts contract
type RunnerProtocol = "http" | "socket" | "single" | "test";

interface RunnerBackplane {
  readonly config: Layer.Layer<ShardingConfig.ShardingConfig, ConfigError.ConfigError>;
  readonly protocol: (kind: RunnerProtocol) => Layer.Layer<
    Sharding.Sharding | Runners.Runners,
    never,
    MessageStorage.MessageStorage | RunnerStorage.RunnerStorage | RunnerHealth.RunnerHealth
  >;
  readonly messageStore: Layer.Layer<MessageStorage.MessageStorage, never, SqlClient.SqlClient | ShardingConfig.ShardingConfig>;
  readonly runnerStore: Layer.Layer<RunnerStorage.RunnerStorage, SqlError.SqlError, SqlClient.SqlClient | ShardingConfig.ShardingConfig>;
  readonly health: (kind: "ping" | "k8s" | "noop") => Layer.Layer<RunnerHealth.RunnerHealth, never, Runners.Runners>;
  readonly snowflake: Effect.Effect<Snowflake.Snowflake, never, Sharding.Sharding>;
}

interface ScheduledWork {
  readonly singleton: <E, R>(name: string, run: Effect.Effect<void, E, R>, options?: { readonly shardGroup?: string }) => Layer.Layer<never, never, Sharding.Sharding | Exclude<R, Scope.Scope>>;
  readonly cron: <E, R>(options: {
    readonly name: string;
    readonly cron: Cron.Cron;
    readonly execute: Effect.Effect<void, E, R>;
    readonly shardGroup?: string;
  }) => Layer.Layer<never, never, Sharding.Sharding | Exclude<R, Scope.Scope>>;
}
```
