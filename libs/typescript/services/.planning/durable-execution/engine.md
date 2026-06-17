# [SERVICES_ENGINE]

The durable-unit vocabulary and the cluster-backed engine wiring. `WorkflowOwner` and `ActivityOwner` are the closed durable-unit family — workflow, activity, clock, deferred, queue, rate-limiter — and `ClusterEngine` resolves every kind onto the SAME `@effect/cluster` `WorkflowEngine` layered over the shard manager and message storage. Every unit survives a process restart with exactly-once semantics. The durable tier is the node-side cluster, separate from the browser worker pool `platform` owns. This page crosses no .NET wire.

## [1]-[INDEX]

One cluster: `[2]-[ENGINE]` owns the closed durable-unit family, the cluster-engine wiring, and the `DurableFault` rail.

## [2]-[ENGINE]

- Owner: `WorkflowOwner` and `ActivityOwner`, the durable units; `ClusterEngine`, the cluster-backed `WorkflowEngine` wiring; `DurableFault`, the node-side durable fault family.
- Cases: the durable-unit vocabulary is one closed family read by unit kind. A workflow is a named resumable unit with a payload schema, success and error schemas, and an execution id that is the idempotency key. An activity is a run-once unit carrying its own success and error schemas, an interrupt-retry schedule, and compensation finalizers. A durable clock pauses an in-flight workflow without holding resources. A durable deferred awaits an externally-signalled exit. A durable queue is a persisted fan-in whose worker drains payloads with the same exactly-once contract. A durable rate limiter caps a remote-lane or fan-out callout durably across restarts. The vocabulary is the closed `DurableUnit` `Data.TaggedEnum` at workflow, activity, clock, deferred, queue, and rate-limiter; `DurableFault` (`CircuitOpen`/`Interrupted`/`Compensated`) is the node-side tagged family distinct from the wire-side `interchange` `FaultDetail`, so a durable rail rejects with a typed `DurableFault.CircuitOpen` rather than an inline anonymous `_tag` shape.
- Entry: `ClusterEngine` resolves the durable kernel as a closed wiring set. The cluster-backed `WorkflowEngine` layers over the shard manager and the message storage, the shard manager layers over the shard configuration with its runner, storage, and health dependencies sourced from `runtime-backplane/backplane#RUNNER_AND_SCHEDULING`, and a workflow `execute` or an activity body reaches the kernel through the layered `WorkflowEngine`. Every `DurableUnit` kind resolves the SAME cluster-backed `WorkflowEngine`: the durable clock, deferred, queue, and rate-limiter are `@effect/workflow` durable PRIMITIVES (`DurableClock.make`, `DurableDeferred.make`, `DurableQueue.makeWorker`, `DurableRateLimiter.make`) a workflow body composes over that one engine, not distinct engine layers — so `durableKernel` is one direct projection to `ClusterEngine.engine`, not a synthetic kind-dispatch that returns the identical layer from six arms. `DurableUnit` stays the closed vocabulary growth lands on; the kernel does not branch on it because the engine resolution does not.
- Packages: `@effect/cluster` for the runtime and shard manager, `@effect/workflow` for the workflow algebra and `WorkflowEngine`, `@effect/experimental` for the persistence substrate, `@effect/platform-node` for the driver host.
- Growth: a new durable unit lands as one row on the closed unit vocabulary, never a new engine layer.
- Boundary: the successor `@effect/cluster` plus standalone `@effect/workflow` split owns this concern, never the merged predecessor; `DurableUnit` is the live dispatch vocabulary `durableKernel` projects, never a dead type; the SQL boundary is reached only through `ClusterEngine` over `persistence/store-boundary#STORE_BOUNDARY`, never a second client; this page imports `interchange`, `projection`, and the persistence owner, never the browser domain.

```ts contract
type DurableUnit = Data.TaggedEnum<{
  readonly Workflow: { readonly name: string };
  readonly Activity: { readonly name: string };
  readonly DurableClock: { readonly name: string };
  readonly DurableDeferred: { readonly name: string };
  readonly DurableQueue: { readonly name: string };
  readonly DurableRateLimiter: { readonly name: string };
}>;
const DurableUnit = Data.taggedEnum<DurableUnit>();

type DurableFault = Data.TaggedEnum<{
  readonly CircuitOpen: { readonly name: string; readonly openedAt: DateTime.Utc };
  readonly Interrupted: { readonly name: string; readonly attempt: number };
  readonly Compensated: { readonly name: string };
}>;
const DurableFault = Data.taggedEnum<DurableFault>();

const durableKernel = (_: DurableUnit): Layer.Layer<WorkflowEngine, never, Sharding | MessageStorage> => ClusterEngine.engine;

interface ClusterEngine {
  readonly engine: Layer.Layer<WorkflowEngine, never, Sharding | MessageStorage>;
  readonly execute: <P, S, E>(
    workflow: Workflow.Workflow<string, P, S, E>,
    options: { readonly executionId: string; readonly payload: P },
  ) => Effect.Effect<S, E, WorkflowEngine>;
}
```
