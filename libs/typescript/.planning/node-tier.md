# [TYPESCRIPT_NODE_TIER]

One page owns the two node-only execution surfaces under the single exports root — the deployment surface expressed as real code and the durable resumable work tier. Both live as node-side sub-entries because publication is singular even though execution targets differ; neither is promoted to a sibling package. The two domains share the node platform layer as their driver host, so their retained clusters own disjoint decision sets — resource components and lifecycle and policy versus durable workflows and the cluster runner and the internal RPC surface. The page consumes no wire cluster — it crosses no .NET wire — and owns the observability-stack provisioning and the internal TypeScript-to-TypeScript RPC surface.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]                         | [OWNS]                                                              |
| :-----: | :-------------------------------- | :------------------------------------------------------------------ |
|   [1]   | RESOURCE_COMPONENTS_AND_LIFECYCLE | the resource components, the automation driver, secrets, and policy |
|   [2]   | OBSERVABILITY_PROVISIONING        | the collector tier provisioned as code                              |
|   [3]   | DURABLE_WORK_AND_RPC              | the workflows, the cluster runner, and the internal RPC surface     |

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

- Owner: `WorkflowOwner` and `ActivityOwner`, the durable units; `ClusterEngine`, the cluster-workflow engine wiring; `RunnerBackplane`, the runner shard backplane; and `InternalRpc`, the internal RPC surface distinct from the C# wire.
- Cases: a workflow is a named unit with success and error schemas, a payload, and an idempotency key; activities are run-once units with retry and compensation finalizers; durable clocks pause without resources and durable deferreds await externally-signalled completion; persistence is backed by the SQL engine onto the Postgres adapter through the cluster workflow engine; the runner shards over the backplane over Redis or SQL; `InternalRpc` lands here and at the runtime-host page (host-to-worker, panel-to-host) and is never substituted for the C# wire.
- Entry: everything in this domain survives a process restart with exactly-once semantics; the durable tier is the node-side cluster, separate from the browser worker pool the runtime-host page owns.
- Packages: the cluster and workflow surfaces, the cluster workflow engine, the SQL and Postgres adapters, the Redis client, the node platform layer, the internal effect-RPC surface, and the container-based ephemeral test harness.
- Growth: a new durable unit lands as one workflow or activity definition; a new backplane lands as one runner row.
- Boundary: the combined cluster-workflow predecessor is absent and stands as a deletion candidate pending a real consumer; the successor cluster plus standalone workflow pair covers its concern; `InternalRpc` is the effect-RPC surface distinct from the C# wire, and its only consumers are this domain and the runtime-host page, which is why the core and RPC security-line annotation ties to those named consumers.

## [5]-[RESEARCH]

- [DURABLE_ENGINE_SURFACE]: the exact activity, runner, and backplane surface of `ClusterEngine` resolves against the stage-[2.C] per-package catalogue extraction; the successor cluster and standalone workflow pair are present at catalog frontier and the predecessor is absent, while the engine's wiring surface detail is a catalogue-extraction probe rather than a catalog-presence fact.
