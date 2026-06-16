# [SERVICES_PLANNING]

`services` owns the complete node services tier AND the infrastructure that hosts it as ONE concern — durable execution, the typed SQL persistence boundary with its entity registry and multi-tenant RLS, the fused hybrid-search owner, the internal RPC surface and the runner/scheduling backplane, and the two-mode IaC provisioning tier. Zero consumers exist; implementation is full-capability with no holding back; pages are transcribed, not re-designed. It is the NODE publication entry (Nx tag `scope:node`), participates in the C# topology as one peer over the same proto vocabulary (remote/companion/sidecar/hub/service), dials the capture-event client-stream that is structurally non-dialable from the browser, and integrates with C# only through the wire contracts and the companion seams. The `./provisioning` exports subpath keeps the `@pulumi/*` deploy-time closure off the durable runtime hot path.

## [1]-[PAGE_INDEX]

| [INDEX] | [PAGE]               | [OWNS]                                                                                                                       | [STATE]     |
| :-----: | :------------------- | :--------------------------------------------------------------------------------------------------------------------------- | :---------- |
|   [1]   | durable-execution.md | WorkflowOwner + ActivityOwner + ClusterEngine + the AiProvider literal axis + the agent journal + resilience                 | provisional |
|   [2]   | persistence.md       | SqlBoundary + the entity-model registry + multi-tenant RLS + jobs/DLQ + events + notifications + assets/export-codec + flags | provisional |
|   [3]   | hybrid-search.md     | the semantic+lexical+trigram+phonetic fused weighted-rank search owner                                                       | provisional |
|   [4]   | internal-rpc.md      | InternalRpc RpcGroup + WorkflowProxy projection + RunnerBackplane + ScheduledWork                                            | provisional |
|   [5]   | provisioning.md      | the data/compute/observe tier model + two-mode dispatch + the ./provisioning subpath + StackOutputs + bootstrap              | provisional |

## [2]-[WIRE_PAGES]

The domain authors no .NET wire shape; it participates as one peer over the proto vocabulary and consumes the eleven contracts as settled. The wire-relevant consumer surface:

- internal-rpc.md: the capture-event client-stream (DocumentService.captureEvents) dialed on node, structurally non-dialable from the browser; the hub-peer op-log HLC changefeed participation.
- persistence.md: consumes `TenantContextWire` as the RLS predicate input (`app.current_tenant` GUC), never re-minting a tenant key.

## [3]-[CATALOGUE_PENDING]

- The `@pulumi/*` set + @dopplerhq/node-sdk: catalogued as `optionalDependencies` behind the `./provisioning` exports subpath so the durable runtime never transitively loads them on the hot path.
- The export-codec set (exceljs/papaparse/jspdf/jszip) + sharp: catalogued; the asset-transfer codec axis on persistence.md consumes them, not admitted-but-orphaned.
- The @effect/ai set: catalogued; the AiProvider literal axis is the sole declaration site, the five provider satellites each exporting a LanguageModel Layer.

## [4]-[GAP_LEDGER]

| [INDEX] | [GAP]                                                            | [CLOSED_BY (page#cluster)]                                                                        |
| :-----: | :--------------------------------------------------------------- | :------------------------------------------------------------------------------------------------ |
|   [1]   | five parallel AI provider Effect.Service owners                  | durable-execution#DURABLE_EXECUTION (one AiProvider literal axis)                                 |
|   [2]   | N parallel schemas per entity breaking isolatedDeclarations emit | persistence#STORE_BOUNDARY (one Model.Class per entity, projections via Model.fields/Schema.pick) |
|   [3]   | drizzle/kysely becoming parallel query surfaces                  | persistence#STORE_BOUNDARY (native PgClient the single query owner)                               |
|   [4]   | hybrid search half-built or folded into raw persistence          | hybrid-search#HYBRID_SEARCH (first-class fused weighted-rank owner)                               |
|   [5]   | a second internal RPC surface                                    | internal-rpc#INTERNAL_RPC (one RpcGroup, WorkflowProxy derived)                                   |
|   [6]   | @pulumi/* loading on the durable runtime hot path                | provisioning#PROVISIONING (the ./provisioning exports subpath)                                    |
|   [7]   | the export-codec libs admitted but orphaned                      | persistence#WORK_AND_SIGNALS (the AssetTransfer codec axis)                                       |

## [5]-[DENSITY_BAR]

| [INDEX] | [AXIS/CONCERN]                    | [OWNER]                                    | [KIND]                  | [CASES]                                                               | [STATE]   |
| :-----: | :-------------------------------- | :----------------------------------------- | :---------------------- | :-------------------------------------------------------------------- | :-------- |
|   [1]   | durable units                     | `WorkflowOwner`/`ActivityOwner`            | closed family           | workflow/activity/clock/deferred/queue/rate-limiter                   | FINALIZED |
|   [2]   | cluster engine                    | `ClusterEngine`                            | Layer wiring            | WorkflowEngine over Sharding + MessageStorage                         | FINALIZED |
|   [3]   | AI provider axis                  | `AiProvider`                               | Schema.Literal          | anthropic/openai/google/amazon-bedrock/openrouter                     | FINALIZED |
|   [4]   | agent journal                     | `AgentJournal`                             | Model.Class             | session_start/tool_call/checkpoint/session_complete                   | FINALIZED |
|   [5]   | resilience primitives             | `Resilience`                               | combinator rows         | circuit-breaker/retry/rfc6902 JSON-patch over the DurableFault family | FINALIZED |
|   [6]   | SQL boundary                      | `SqlBoundary`                              | PgClient/Model/Migrator | one client, one Model.Class per entity                                | FINALIZED |
|   [7]   | entity registry                   | `EntityRegistry`                           | Model.Class rows        | ~15 entities, projections via Model.fields/Schema.pick                | FINALIZED |
|   [8]   | multi-tenancy                     | `TenantScope`                              | RLS axis                | app.current_tenant GUC + lifecycle + purge family                     | FINALIZED |
|   [9]   | jobs/DLQ + events + notifications | `WorkQueue`/`EventJournal`/`Notifications` | Model.Class rows        | priority/status, journal, channel matrix                              | FINALIZED |
|  [10]   | asset transfer                    | `AssetTransfer`                            | codec axis              | csv/xlsx/pdf/archive export                                           | FINALIZED |
|  [11]   | feature flags                     | `FeatureFlags`                             | percentage buckets      | 0-100 rollout integer                                                 | FINALIZED |
|  [12]   | hybrid search                     | `HybridSearch`                             | fused weighted-rank     | semantic/lexical/trigram/phonetic                                     | FINALIZED |
|  [13]   | internal RPC                      | `InternalRpc`                              | one RpcGroup            | WorkflowProxy derived over the same Schema                            | FINALIZED |
|  [14]   | runner backplane                  | `RunnerBackplane`                          | 4-row backplane         | protocol/message-storage/runner-storage/runner-health                 | FINALIZED |
|  [15]   | scheduled work                    | `ScheduledWork`                            | singleton + cron        | exactly-one-runner pin, shard-pinned cron                             | FINALIZED |
|  [16]   | IaC tier model                    | `TierStack`                                | ComponentResource       | data/compute/observe × cloud/self-hosted                              | FINALIZED |
|  [17]   | deploy lifecycle                  | `AutomationDriver`                         | @effect/cli verbs       | up/preview/refresh/destroy                                            | FINALIZED |
|  [18]   | cross-stack topology              | `StackOutputs`                             | StackReference          | DSN/object-store/redis/otlp/spa-origin                                | FINALIZED |
|  [19]   | secret + policy boundary          | `SecretResolver`/`PolicyGuard`             | resolver + PolicyPack   | Doppler/ESC/config resolution; CrossGuard advisory/mandatory rules    | FINALIZED |
|  [20]   | observability stack               | `ObservabilityStack`                       | provisioned tier        | Alloy OTLP -> Prometheus -> Grafana                                   | FINALIZED |

## [6]-[BUILD_ORDER]

Persistence precedes durable (the cluster rides the one PgClient); the runner backplane composes under the cluster engine; provisioning is a leaf subpath consumed by the node entry.

| [INDEX] | [FILE]                   | [TRANSCRIBES]                                                                   | [GATE]                                 |
| :-----: | :----------------------- | :------------------------------------------------------------------------------ | :------------------------------------- |
|   [1]   | src/persistence.ts       | persistence#STORE_BOUNDARY + persistence#TENANCY + persistence#WORK_AND_SIGNALS | tsgo + isolatedDeclarations emit       |
|   [2]   | src/hybrid-search.ts     | hybrid-search#HYBRID_SEARCH                                                     | tsgo + container harness               |
|   [3]   | src/durable-execution.ts | durable-execution#DURABLE_EXECUTION                                             | tsgo + exactly-once harness            |
|   [4]   | src/internal-rpc.ts      | internal-rpc#INTERNAL_RPC                                                       | tsgo + runner-restart harness          |
|   [5]   | src/provisioning/        | provisioning#PROVISIONING                                                       | tsgo + ./provisioning subpath resolves |
|   [6]   | src/node.ts              | the node entry composing the durable cluster + runner + provisioning            | bundle builds                          |
|   [7]   | src/index.ts             | the "." export                                                                  | exports resolve                        |

## [7]-[PROOF_GATES]

| [GATE]          | [COMMAND]                              | [EVIDENCE]                                         |
| :-------------- | :------------------------------------- | :------------------------------------------------- |
| catalog resolve | `pnpm install`                         | catalogMode strict resolves @rasm/ts               |
| typecheck       | tsgo `--noEmit`                        | zero diagnostics; isolatedDeclarations emits .d.ts |
| subpath split   | `node -e import('@rasm/ts/node')`      | @pulumi/* not loaded on the runtime path           |
| durable harness | testcontainers Postgres + Redis        | exactly-once, durable-replay, RLS-scope prove      |
| search harness  | testcontainers Postgres + pg_trgm/HNSW | fused weighted-rank ranks correctly                |

## [8]-[PROHIBITIONS]

No parallel AI provider service set beside the one `AiProvider` literal axis; no second SQL owner beside `SqlBoundary` and no drizzle/kysely parallel query surface; no N parallel schemas per entity — one `Model.Class` per entity with projections via `Model.fields`/`Schema.pick`; no second internal RPC surface beside `InternalRpc`; no `@pulumi/*` import on the runtime hot path outside the `./provisioning` subpath; no re-minted tenant key — `TenantContextWire` is consumed as the RLS predicate input; no hybrid search folded into raw persistence; no .NET wire shape authored branch-side; no comment carrying task or process narration.

## [9]-[ADMISSIONS_RECORD]

| [INDEX] | [PACKAGE]                                               | [PAGE]                                | [CATALOGUE]                              | [STATUS]                    |
| :-----: | :------------------------------------------------------ | :------------------------------------ | :--------------------------------------- | :-------------------------- |
|   [1]   | @effect/cluster + @effect/workflow                      | durable-execution.md                  | api-effect-cluster + api-effect-workflow | admitted                    |
|   [2]   | @effect/ai + the five provider satellites               | durable-execution.md                  | api-effect-ai + provider pages           | admitted                    |
|   [3]   | @effect/experimental                                    | durable-execution.md, internal-rpc.md | api-effect-experimental                  | admitted                    |
|   [4]   | rfc6902                                                 | durable-execution.md                  | api-infra-data                           | admitted                    |
|   [5]   | @effect/sql + @effect/sql-pg                            | persistence.md, hybrid-search.md      | api-effect-sql + api-effect-sql-pg       | admitted                    |
|   [6]   | exceljs/papaparse/jspdf/jszip/sharp                     | persistence.md                        | api-infra-data                           | admitted                    |
|   [7]   | @effect/rpc                                             | internal-rpc.md                       | api-effect-rpc                           | admitted                    |
|   [8]   | ioredis                                                 | internal-rpc.md                       | api-infra-data                           | admitted                    |
|   [9]   | @effect/cli                                             | provisioning.md                       | api-effect-cli                           | admitted                    |
|  [10]   | @pulumi/* + @dopplerhq/node-sdk + @effect-aws/client-s3 | provisioning.md, persistence.md       | api-infra-data                           | admitted (optional/subpath) |
|  [11]   | @effect/opentelemetry + @opentelemetry/sdk-trace-node   | provisioning.md                       | api-effect-opentelemetry                 | admitted                    |

## [10]-[REFINEMENT_HORIZON]

The next deepening drives deeper durable saga composition (compensating transactions across activity chains), hybrid-search re-ranking (learned-rank fusion over the four signals), and the IaC lifecycle (drift detection and policy-as-code expansion). The GraphFork CRDT op vocabulary the durable hub peer would fold waits on the C# `sync-collaboration#MERGE_LAW` op-log amendment. Closed by the bar: every provider is one literal, every entity is one `Model.Class`, every export format is one codec row, every runner topology is one protocol row, and the deploy mode is one dispatch row — the domain admits no parallel surface.
