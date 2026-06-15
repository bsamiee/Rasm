# [TYPESCRIPT_NODE_TIER]

One page owns the node-only execution surfaces under the single exports root — the deployment surface expressed as real code, the durable resumable work tier, and the runner placement and scheduling tier beneath it. All live as node-side sub-entries because publication is singular even though execution targets differ; none is promoted to a sibling package. The domains share the node platform layer as their driver host, so the retained clusters own disjoint decision sets — resource components and lifecycle and policy, the request-driven durable units and their proxy-exposed internal RPC, and the runner topology with its storage, health, and durable scheduling. The page consumes no wire cluster — it crosses no .NET wire — and owns the observability-stack provisioning and the internal TypeScript-to-TypeScript RPC surface.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]                         | [OWNS]                                                                 |
| :-----: | :-------------------------------- | :--------------------------------------------------------------------- |
|   [1]   | RESOURCE_COMPONENTS_AND_LIFECYCLE | the resource components, the automation driver, secrets, and policy    |
|   [2]   | OBSERVABILITY_PROVISIONING        | the collector tier provisioned as code                                 |
|   [3]   | DURABLE_WORK_AND_RPC              | the request-driven durable units, the engine wiring, and internal RPC  |
|   [4]   | RUNNER_AND_SCHEDULING             | the runner topology, the durable storage, health, singletons, and cron |

## [2]-[RESOURCE_COMPONENTS_AND_LIFECYCLE]

- Owner: `ResourceComponent`, the resource component owners; `AutomationDriver`, the automation lifecycle driver; `SecretResolver`, the typed secret and config resolution; and `PolicyGuard`, the policy guardrails.
- Cases: `ResourceComponent` instances are classes that instantiate children and expose outputs, the one-rich-owner discipline the corpus uses; `AutomationDriver` drives up, preview, refresh, and destroy from an effect program on the node platform rather than a bare CLI; `SecretResolver` resolves environment and secret values at the composition root so config arrives as a domain value; `PolicyGuard` enforces policy-as-code; image build and cluster deploy of the worker tier are owned here; the monorepo orchestrator scopes deploys to the affected stacks.
- Entry: the lifecycle is driven programmatically from the node platform layer as the driver host.
- Packages: the deployment core, the cloud and networking providers, the container and orchestration providers, the command and random helpers, the secret-resolution SDK, the policy SDK, the node platform layer, and the monorepo deployment and container-image executors.
- Growth: a new stack lands as one component class; a new provider lands as one provider row; a new policy lands as one guardrail row.
- Boundary: this domain crosses no .NET wire and carries no wire type; it is a node sub-entry under the one exports root, never a sibling package.

## [3]-[OBSERVABILITY_PROVISIONING]

- Owner: `ObservabilityStack`, the metrics, trace, log, and dashboard collector tier provisioned as deployment-as-code.
- Cases: `ObservabilityStack` provisions the collector tier through the resource components; the only telemetry path is the host exporter rooting to the collector; dashboards read the collector and never a bespoke wire.
- Entry: the telemetry-boundary law holds end to end — telemetry crosses no wire contract, the host self-telemetry edge ships to the collector, the collector backs the dashboard panels, and provisioning that collector tier is this domain's concern.
- Packages: the deployment core and the container and orchestration providers that stand up the collector tier.
- Growth: a new signal sink lands as one provisioned collector row; a new dashboard backend lands as one provisioned component.
- Boundary: the provisioned stack references no wire type; the export-to-collector path is the single telemetry seam, owned here for provisioning and at the runtime-host page for emission.

## [4]-[DURABLE_WORK_AND_RPC]

- Owner: `WorkflowOwner` and `ActivityOwner`, the durable units; `ClusterEngine`, the resolved cluster-workflow engine wiring; and `InternalRpc`, the internal RPC surface distinct from the C# wire. Runner placement, durable storage, health, and scheduling move to RUNNER_AND_SCHEDULING so this cluster owns only request-driven durable work and its execution-exposure boundary.
- Cases: the durable-unit vocabulary is one closed family read by unit kind — a workflow is a named resumable unit with a payload schema, success and error schemas, and an execution id that is the idempotency key; an activity is a run-once unit carrying its own success and error schemas, an interrupt-retry schedule, and compensation finalizers; a durable clock pauses an in-flight workflow without holding resources; a durable deferred awaits an externally-signalled exit; a durable queue is a persisted fan-in whose worker drains payloads with the same exactly-once contract; a durable rate limiter caps a remote-lane or fan-out callout durably across restarts. The vocabulary is closed at workflow, activity, clock, deferred, queue, and rate-limiter — none named-but-unowned. `WorkflowOwner` holds the workflow definitions, durable clocks, and durable deferreds; `ActivityOwner` holds the activities, durable queues, and durable rate limiters because each is invoked from inside an activity body.
- Entry: every unit survives a process restart with exactly-once semantics; the durable tier is the node-side cluster, separate from the browser worker pool the runtime-host page owns. `ClusterEngine` resolves the durable kernel as a closed wiring set, not a research item: the cluster-backed `WorkflowEngine` layers over the shard manager and the message storage, the shard manager layers over the shard configuration with its runner, storage, and health dependencies sourced from RUNNER_AND_SCHEDULING, and a workflow `execute` or an activity body reaches the kernel through the layered `WorkflowEngine` it provides. The standalone successor pair — the cluster runtime and the extracted workflow algebra — supplies this wiring; the merged predecessor is deleted from the catalog.
- InternalRpc: `InternalRpc` is grounded as an effect-RPC group, never a bare assertion. Each procedure is a Schema-typed request with its own success and error schemas, aggregated into one request group; the serialization row selects the in-process codec for the TS-to-TS edge — Schema-serialized, the protobuf-and-msgpack C# wire structurally absent — and the transport binds the group over the platform socket or HTTP layer with a client and a server half. The durable workflows become callable over this surface through the workflow-proxy projection: the proxy converts the workflow set into the request group whose procedures start, resume, and poll each workflow by execution id, and the proxy server installs the handlers, so the host-to-worker dispatch and the panel-to-host start and resume signalling both ride one group rather than a parallel surface. The runtime-host host-to-worker edge consumes the same group; a second internal RPC surface is the named defect.
- Packages: the cluster and workflow surfaces, the cluster-backed workflow engine, the effect-RPC group, client, server, and serialization surfaces, the workflow-proxy projection, the SQL and Postgres adapters reached through the engine, the node platform layer, and the container-based ephemeral test harness.
- Growth: a new durable unit lands as one row on the closed unit vocabulary; a new internal procedure lands as one request on the existing group; a new workflow becomes callable by extending the proxied workflow set, never a hand-built procedure.
- Boundary: the combined cluster-workflow predecessor is absent and is the deletion candidate the charter records; the successor cluster plus standalone workflow pair covers its concern; `InternalRpc` is the effect-RPC surface distinct from the C# wire, its only consumers are this domain and the runtime-host page, and the workflow proxy is the sole mechanism by which a durable unit becomes a callable procedure.

```ts contract
type DurableUnit =
  | { readonly _tag: "Workflow"; readonly name: string }
  | { readonly _tag: "Activity"; readonly name: string }
  | { readonly _tag: "DurableClock"; readonly name: string }
  | { readonly _tag: "DurableDeferred"; readonly name: string }
  | { readonly _tag: "DurableQueue"; readonly name: string }
  | { readonly _tag: "DurableRateLimiter"; readonly name: string };

interface ClusterEngine {
  readonly engine: Layer.Layer<WorkflowEngine, never, Sharding | MessageStorage>;
  readonly execute: <P, S, E>(
    workflow: Workflow<string, P, S, E>,
    options: { readonly executionId: string; readonly payload: P },
  ) => Effect.Effect<S, E, WorkflowEngine>;
}

interface InternalRpc<Procedures extends Rpc.Any> {
  readonly group: RpcGroup.RpcGroup<Procedures>;
  readonly serialization: Layer.Layer<RpcSerialization>;
  readonly server: Layer.Layer<never, never, Protocol>;
  readonly client: Effect.Effect<RpcClient.RpcClient<Procedures>, never, Protocol>;
}
```

## [5]-[RUNNER_AND_SCHEDULING]

- Owner: `RunnerBackplane`, the runner placement and durable storage backplane, and `ScheduledWork`, the cluster-wide singleton and durable-cron scheduling owner. The cluster splits from DURABLE_WORK_AND_RPC because runner topology, storage, and health are a placement decision set disjoint from request-driven units, and durable scheduling is a third decision set distinct from both.
- Cases: `RunnerBackplane` owns four explicit rows rather than one Redis-or-SQL hand-wave. The runner protocol is the placement transport — the HTTP runner for production k8s topologies, the socket runner for node-to-node clusters, the single runner for one-process deployments, and the test runner for the ephemeral harness — one closed protocol vocabulary. Message storage and runner storage are the durable backing — the SQL message store and the SQL runner store over the Postgres adapter as the production rows, with the in-memory and no-op stores as the sibling test rows. Runner health and discovery is the liveness row — ping-based health for socket and single-process clusters and the k8s HTTP client for native runner-address resolution and pod liveness. The distributed id source threading message and entity identity is the snowflake generator the shard manager exposes.
- ScheduledWork: scheduled and singleton work is the jobs half of the durable tier the request-driven cluster does not cover. A cluster singleton registers an exactly-one-runner background loop — the leader-elected sweep, reconcile, and garbage-collection jobs — pinned to a single shard so it never double-runs. A durable cron registers a shard-pinned scheduled execution on a cron expression with the same exactly-once contract as a workflow. Singletons register through the shard manager; cron registers as a durable scheduled unit.
- Entry: the backplane is the placement and durability substrate beneath `ClusterEngine`; the shard configuration, the chosen runner protocol layer, the message and runner storage layers, and the runner-health layer compose into the shard manager that the cluster-backed workflow engine layers onto. Cluster telemetry — shard counts, entity counts, runner counts, and runner health — emits through the cluster metrics source that the OBSERVABILITY_PROVISIONING collector reads.
- Packages: the cluster sharding, shard-configuration, runner-protocol, message-storage, runner-storage, runner-health, k8s-http-client, snowflake, singleton, and cluster-cron surfaces; the SQL and Postgres adapters backing the durable stores; the Redis client for the multi-node backplane and the cluster-metrics source.
- Growth: a new runner protocol lands as one protocol row; a new durable store lands as one storage row; a new background loop lands as one singleton; a new scheduled job lands as one durable cron.
- Boundary: this cluster crosses no .NET wire and carries no wire type; runner placement and durable scheduling are node-only concerns beneath the durable units, and the cluster-metrics signal reaches dashboards only through the collector, never a bespoke wire.
