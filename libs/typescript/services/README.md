# [SERVICES]

The host-free node tier of the TypeScript branch (Nx scope `scope:node`) and the deploy-time IaC that hosts it, as one concern. It is lib-grade general and meta capability — durable execution, the TS-owned Postgres store, hybrid search, internal RPC, provisioning — deliberately NOT coupled to the AEC or Rhino pipelines; it consumes the C# wire only through interchange and projection and owns no geometry. This README routes the design pages under `.planning/` and registers every external package the folder uses; the domain folder-map is in `ARCHITECTURE.md`, the forward concepts in `IDEAS.md`, open work in `TASKLOG.md`.

## [1]-[PAGE_ROUTER]

The design pages under `.planning/`, grouped by sub-domain in build order; `ARCHITECTURE.md` carries the flat-module mapping and the build-chain rationale.

- [persistence/store-boundary](.planning/persistence/store-boundary.md): the single `PgClient`/`Migrator` boundary and the one-`Model.Class`-per-entity registry.
- [persistence/tenancy](.planning/persistence/tenancy.md): the multi-tenant RLS axis over the `app.current_tenant` GUC, the lifecycle, and the purge-handler family.
- [persistence/work-and-signals](.planning/persistence/work-and-signals.md): jobs/DLQ, the event journal, notifications, the asset-export codec axis, the feature-flag buckets.
- [hybrid-search/fused-rank](.planning/hybrid-search/fused-rank.md): the fused semantic+lexical+trigram+phonetic weighted-rank owner and the post-fusion `rerank` stage.
- [runtime-backplane/backplane](.planning/runtime-backplane/backplane.md): the four-row runner backplane, the snowflake id source, the cluster singletons, the shard-pinned cron.
- [durable-execution/engine](.planning/durable-execution/engine.md): the closed durable-unit family, the `ClusterEngine` wiring, the `DurableFault` rail.
- [durable-execution/saga](.planning/durable-execution/saga.md): the `SagaStep` chain, the `StepOutcome`/`SagaTerminal` fold, the engine-compensated saga workflow.
- [durable-execution/ai-activity](.planning/durable-execution/ai-activity.md): the `AiProvider` literal axis, the single AI activity, the `AgentJournal` ledger, the `Resilience` primitives.
- [messaging/internal-rpc](.planning/messaging/internal-rpc.md): the one `RpcGroup` and the `WorkflowProxy` projection over the durable workflows.
- [provisioning/contract](.planning/provisioning/contract.md): the data/compute/observe tier model, the cloud/self-hosted dispatch, secrets, `StackOutputs`, `PolicyGuard`, the `ObservabilityStack`.
- [provisioning/drift](.planning/provisioning/drift.md): the `previewRefresh` drift fold, the typed `StackDriftSummary` receipt, the CI drift gate.

The `eventing/` sub-domain (transactional-outbox reliable event publishing) is planned and holds no page yet; it is mapped in `ARCHITECTURE.md` and driven from `IDEAS.md`.

## [2]-[PACKAGES]

Every external package the folder uses, planned or implemented, grouped by concern. Versions are centralized in the one workspace catalog; this registry carries no pin and no `.api` link.

- Effect runtime: `effect`, `@effect/platform-node`
- Durable execution: `@effect/cluster`, `@effect/workflow`, `@effect/experimental`
- Internal RPC: `@effect/rpc`
- Persistence: `@effect/sql`, `@effect/sql-pg`
- AI: `@effect/ai`, `@effect/ai-anthropic`, `@effect/ai-openai`, `@effect/ai-google`, `@effect/ai-amazon-bedrock`, `@effect/ai-openrouter`
- Resilience and sync: `rfc6902`
- Cache and metrics backplane: `ioredis`
- Asset transfer codecs: `exceljs`, `papaparse`, `jspdf`, `jszip`, `sharp`
- Object store and email: `@effect-aws/client-s3`, `@aws-sdk/client-s3`, `@aws-sdk/s3-request-presigner`, `@aws-sdk/client-sesv2`, `nodemailer`
- CLI binding: `@effect/cli`
- Provisioning IaC: `@pulumi/pulumi`, `@pulumi/aws`, `@pulumi/awsx`, `@pulumi/kubernetes`, `@pulumi/docker`, `@pulumi/command`, `@pulumi/random`, `@pulumi/policy`, `@pulumi/esc-sdk`, `@dopplerhq/node-sdk`
- Observability: `@effect/opentelemetry`, `@opentelemetry/sdk-trace-node`, `@opentelemetry/sdk-metrics`
- Test harness: `testcontainers`
