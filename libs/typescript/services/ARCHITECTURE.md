# [SERVICES_ARCHITECTURE]

`services` is the node tier and its hosting infrastructure as one folder: one `SqlBoundary` is the single query owner, one `AiProvider` literal axis collapses the provider set, one `InternalRpc` `RpcGroup` carries every internal verb, and the IaC tier hides behind one `./provisioning` subpath off the runtime hot path. Mechanics live in the finalized `.planning/` pages; this page is the atlas — the source tree and build order, the owner registry (the one owner-state surface), dependency direction, cross-folder seams, boundaries, and prohibitions.

## [1]-[SOURCE_TREE]

The flat module layout IS the build order: persistence precedes durable (the cluster rides the one PgClient), the runner backplane composes under the cluster engine, provisioning is a leaf subpath consumed by the node entry, and the node entry lands last. Each leaf is one transcription unit annotated with the owners it transcribes and the owning page#cluster.

```text codemap
services/
├── persistence.ts                  # SqlBoundary, EntityRegistry, TenantScope, WorkQueue/EventJournal/Notifications, AssetTransfer, FeatureFlags — persistence#STORE_BOUNDARY, #TENANCY, #WORK_AND_SIGNALS
├── hybrid-search.ts                # HybridSearch fused weighted-rank owner — hybrid-search#HYBRID_SEARCH
├── durable-execution.ts           # WorkflowOwner/ActivityOwner, ClusterEngine, AiProvider, AgentJournal, Resilience, SagaOwner/SagaStep — durable-execution#DURABLE_EXECUTION
├── internal-rpc.ts                # InternalRpc, WorkflowProxy, RunnerBackplane, ScheduledWork — internal-rpc#INTERNAL_RPC, #RUNNER_AND_SCHEDULING
├── provisioning/                  # TierStack, AutomationDriver, StackOutputs, SecretResolver/PolicyGuard, ObservabilityStack — provisioning#PROVISIONING
├── node.ts                         # the node entry composing the durable cluster + runner + provisioning
└── index.ts                        # the "." export
```

`persistence.ts` lands first: the cluster engine rides the one PgClient and every entity is one `Model.Class` the durable units reference. `hybrid-search.ts` lands next as a first-class fused-rank owner over the same client. `durable-execution.ts` composes the cluster over `Sharding` + `MessageStorage`; `internal-rpc.ts` composes the runner backplane under the cluster engine and derives `WorkflowProxy` over the one `RpcGroup`. `provisioning/` is the leaf subpath the node entry consumes; its deploy-time closure stays behind the `./provisioning` exports subpath. `node.ts` composes the durable cluster + runner + provisioning; `index.ts` exports the one neutral subpath.

## [2]-[OWNER_REGISTRY]

The single owner-state surface for the folder. A new feature is a row or case, never a new surface: every provider is one literal, every entity is one `Model.Class`, every export format is one codec row, every runner topology is one protocol row, and the deploy mode is one dispatch row. `[STATE]` is `FINALIZED` where the owner is a transcription-complete fence with no open gate, `SPIKE` where the owner is fence-complete but its proof carries a residual probe named in the page RESEARCH cluster. This is the ONLY place owner state lives.

| [INDEX] | [AXIS/RAIL]                      | [OWNER]                                    | [KIND]                  | [CASES]                                                               | [PAGE#CLUSTER]                          |  [STATE]  |
| :-----: | :------------------------------ | :----------------------------------------- | :---------------------- | :------------------------------------------------------------------- | :-------------------------------------- | :-------: |
|   [1]   | durable units                   | `WorkflowOwner`/`ActivityOwner`            | closed family           | workflow/activity/clock/deferred/queue/rate-limiter                   | durable-execution#DURABLE_EXECUTION     | FINALIZED |
|   [2]   | cluster engine                  | `ClusterEngine`                            | Layer wiring            | WorkflowEngine over Sharding + MessageStorage                         | durable-execution#DURABLE_EXECUTION     | FINALIZED |
|   [3]   | AI provider axis                | `AiProvider`                               | Schema.Literal          | anthropic/openai/google/amazon-bedrock/openrouter                     | durable-execution#DURABLE_EXECUTION     | FINALIZED |
|   [4]   | agent journal                   | `AgentJournal`                             | Model.Class             | session_start/tool_call/checkpoint/session_complete                   | durable-execution#DURABLE_EXECUTION     | FINALIZED |
|   [5]   | resilience primitives           | `Resilience`                               | combinator rows         | circuit-breaker/retry/rfc6902 JSON-patch over the DurableFault family | durable-execution#DURABLE_EXECUTION     | FINALIZED |
|   [6]   | saga compensation               | `SagaOwner`/`SagaStep`                     | fold over StepOutcome   | forward/compensating pairs, reverse-order rollback, Committed/RolledBack/Aborted | durable-execution#DURABLE_EXECUTION | FINALIZED |
|   [7]   | SQL boundary                    | `SqlBoundary`                              | PgClient/Model/Migrator | one client, one Model.Class per entity                                | persistence#STORE_BOUNDARY              | FINALIZED |
|   [8]   | entity registry                 | `EntityRegistry`                           | Model.Class rows        | ~15 entities, projections via Model.fields/Schema.pick                | persistence#STORE_BOUNDARY              | FINALIZED |
|   [9]   | multi-tenancy                   | `TenantScope`                              | RLS axis                | app.current_tenant GUC + lifecycle + purge family                    | persistence#TENANCY                     | FINALIZED |
|  [10]   | jobs/DLQ + events + notifications | `WorkQueue`/`EventJournal`/`Notifications` | Model.Class rows        | priority/status, journal, channel matrix                             | persistence#WORK_AND_SIGNALS            | FINALIZED |
|  [11]   | asset transfer                  | `AssetTransfer`                            | codec axis              | csv/xlsx/pdf/archive export                                           | persistence#WORK_AND_SIGNALS            | FINALIZED |
|  [12]   | feature flags                   | `FeatureFlags`                             | percentage buckets      | 0-100 rollout integer                                                | persistence#WORK_AND_SIGNALS            | FINALIZED |
|  [13]   | hybrid search                   | `HybridSearch`                             | fused weighted-rank     | semantic/lexical/trigram/phonetic                                     | hybrid-search#HYBRID_SEARCH             | FINALIZED |
|  [14]   | internal RPC                    | `InternalRpc`                              | one RpcGroup            | WorkflowProxy derived over the same Schema                            | internal-rpc#INTERNAL_RPC               | FINALIZED |
|  [15]   | runner backplane                | `RunnerBackplane`                          | 4-row backplane         | protocol/message-storage/runner-storage/runner-health                | internal-rpc#RUNNER_AND_SCHEDULING      | FINALIZED |
|  [16]   | scheduled work                  | `ScheduledWork`                            | singleton + cron        | exactly-one-runner pin, shard-pinned cron                            | internal-rpc#RUNNER_AND_SCHEDULING      | FINALIZED |
|  [17]   | IaC tier model                  | `TierStack`                                | ComponentResource       | data/compute/observe × cloud/self-hosted                             | provisioning#PROVISIONING               | FINALIZED |
|  [18]   | deploy lifecycle                | `AutomationDriver`                         | @effect/cli verbs       | up/preview/refresh/destroy                                            | provisioning#PROVISIONING               | FINALIZED |
|  [19]   | cross-stack topology            | `StackOutputs`                             | StackReference          | DSN/object-store/redis/otlp/spa-origin                                | provisioning#PROVISIONING               | FINALIZED |
|  [20]   | secret + policy boundary        | `SecretResolver`/`PolicyGuard`             | resolver + PolicyPack   | Doppler/ESC/config resolution; CrossGuard advisory/mandatory rules    | provisioning#PROVISIONING               | FINALIZED |
|  [21]   | observability stack             | `ObservabilityStack`                       | provisioned tier        | Alloy OTLP -> Prometheus -> Grafana                                  | provisioning#PROVISIONING               | FINALIZED |

## [3]-[DEPENDENCY_DIRECTION]

| [INDEX] | [FOLDER]      | [MAY_REFERENCE_SERVICES] | [SERVICES_MAY_REFERENCE] | [BOUNDARY]                                          |
| :-----: | :------------ | :----------------------: | :----------------------: | :------------------------------------------------- |
|   [1]   | `interchange` |            no            |           yes            | participates over the same wire vocabulary as a peer |
|   [2]   | `projection`  |            no            |           yes            | folds the same stores on the node tier              |
|   [3]   | `ui`          |            no            |            no            | browser stratum is out of the node tier             |
|   [4]   | `platform`    |            no            |            no            | the browser entry is out of the node tier           |

`services` is the node-stratum publication: the browser folders never import it. It consumes the neutral `interchange`/`projection` surfaces, dials the capture-event client-stream non-dialable from the browser, and the `@pulumi/*` deploy-time closure stays behind the `./provisioning` subpath off the runtime hot path.

## [4]-[SEAMS]

Every two-folder fact splits by altitude: mechanics live at the named owner cluster, consequences land at the consumer. Intra-TypeScript seams ride `pkg/page#CLUSTER`; the wire contracts and the companion peer participation route through the Tier-0 seam ledger.

| [INDEX] | [SEAM]                | [MECHANICS_AT]                                | [CONSEQUENCE_AT]                                                       |
| :-----: | :-------------------- | :-------------------------------------------- | :-------------------------------------------------------------------- |
|   [1]   | tenant predicate      | persistence#TENANCY                            | the tenant context is consumed as the RLS predicate input, never re-minted, routed through the Tier-0 seam ledger |
|   [2]   | capture-event stream  | internal-rpc#INTERNAL_RPC                       | the node-dialed client-stream over the document service routed through the Tier-0 seam ledger |
|   [3]   | hub-peer changefeed   | internal-rpc#RUNNER_AND_SCHEDULING             | the op-log HLC changefeed participation as a peer routed through the Tier-0 seam ledger |
|   [4]   | flag vocabulary       | persistence#WORK_AND_SIGNALS                   | platform/feature-flags-config#FEATURE_FLAGS_CONFIG references the FeatureFlags bucket/variant axis as settled |
|   [5]   | provisioning subpath  | provisioning#PROVISIONING                       | node.ts consumes the `./provisioning` subpath; the deploy-time closure stays off the runtime path |
|   [6]   | wire contract source  | the Tier-0 seam ledger                         | every wire participation transcribes the upstream fence as a peer       |

## [5]-[BOUNDARIES]

- `services` is the node tier AND its hosting infrastructure as one concern; it is not a browser package, a view package, or a wire-authoring package.
- `SqlBoundary` is the single query owner; one `Model.Class` per entity with projections via `Model.fields`/`Schema.pick`, never N parallel schemas.
- The AI provider set collapses to one `AiProvider` literal axis; the five provider satellites each export a model Layer.
- The tenant key is consumed as the RLS predicate input through the Tier-0 seam, never re-minted branch-side.
- `InternalRpc` is the one internal RPC surface; `WorkflowProxy` derives over the same `RpcGroup`.
- The `@pulumi/*` deploy-time closure stays behind the `./provisioning` exports subpath off the durable runtime hot path.

## [6]-[PROHIBITIONS]

The closed NEVER list — the deleted patterns the owner registry forecloses.

- NEVER a parallel AI provider service set beside the one `AiProvider` literal axis.
- NEVER a second SQL owner beside `SqlBoundary`, and no drizzle/kysely parallel query surface.
- NEVER N parallel schemas per entity; one `Model.Class` per entity with projections via `Model.fields`/`Schema.pick`.
- NEVER a second internal RPC surface beside `InternalRpc`.
- NEVER a `@pulumi/*` import on the runtime hot path outside the `./provisioning` subpath.
- NEVER a re-minted tenant key; the tenant context is consumed as the RLS predicate input through the Tier-0 seam.
- NEVER hybrid search folded into raw persistence; it is a first-class fused weighted-rank owner.
- NEVER a wire shape authored branch-side; wire participation transcribes the upstream fence as a peer.
- NEVER a comment carrying task or process narration.
