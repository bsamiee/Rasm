# [SERVICES]

The host-free node tier of the TypeScript branch (Nx scope `scope:node`) and the deploy-time IaC that hosts it form one concern. The folder is lib-grade general and meta capability — durable execution, durable AI agents, the TS-owned Postgres store, hybrid search, internal RPC and addressable actors, transactional-outbox eventing, and provisioning — with no coupling to the AEC or Rhino pipelines; it consumes the C# wire only through interchange and projection and owns no geometry. This README routes the design pages under `.planning/` and registers every external package the folder uses. The domain folder-map is in `ARCHITECTURE.md`, forward concepts in `IDEAS.md`, and open work in `TASKLOG.md`.

## [1]-[ROUTER]

- [1]-[STORE](.planning/persistence/store.md): the single `PgClient`/`Migrator` boundary and the one-`Model.Class`-per-entity registry.
- [2]-[TENANCY](.planning/persistence/tenancy.md): the multi-tenant RLS axis over the `app.current_tenant` GUC, the lifecycle, and the purge-handler family.
- [3]-[WORK](.planning/persistence/work.md): jobs/DLQ, the event journal, notifications, and the feature-flag buckets (the asset-export codec fan-out moved to `persistence/object`).
- [4]-[REACTIVE](.planning/persistence/reactive.md): the read-side `ReactiveQuery` owner — `SqlClient.reactive` over the `@effect/experimental` `Reactivity` key-scoped invalidation, entity-keyed query re-run as a `Stream`.
- [5]-[IDEMPOTENCY](.planning/persistence/idempotency.md): the cross-surface exactly-once `IdempotencyLedger` `Model.Class` and the `ClaimOrReplay` claim-state fold, serving outbox/entity/session beyond the per-surface `SaveResult.Duplicate`/`idempotencyKey` primitives.
- [6]-[OBJECT](.planning/persistence/object.md): the one `ObjectStore` `Effect.Service` over `@effect-aws/client-s3`, the `ObjectKey` content-addressed brand, the `PresignGrant` verb axis, the lifecycle policy rows, and the asset-codec fan-out migrated from `persistence/work`.
- [7]-[RANK](.planning/search/rank.md): the RRF-over-BM25 semantic+lexical+trigram+phonetic fused-rank owner and the post-fusion `rerank` stage.
- [8]-[EMBEDDING](.planning/search/embedding.md): the `EmbeddingProfile` migration and the `EmbeddingModel.makeDataLoader` batch-embed orchestration feeding the fused-rank vector arm (profiles only the `pgvector` column, never the gated BM25 index).
- [9]-[ENGINE](.planning/execution/engine.md): the closed durable-unit family, the `ClusterEngine` wiring, and the `DurableFault` rail.
- [10]-[SAGA](.planning/execution/saga.md): the `SagaStep` chain, the `StepOutcome`/`SagaTerminal` fold, and the engine-compensated saga workflow.
- [11]-[AI](.planning/execution/ai.md): the `AiProvider` literal axis, the single AI activity, the `AgentJournal` ledger, and the `Resilience` primitives.
- [12]-[BACKPLANE](.planning/execution/backplane.md): the four-row runner backplane, the snowflake id source, the cluster singletons, and the shard-pinned cron.
- [13]-[OUTBOX](.planning/execution/outbox.md): the same-txn `Outbox` `Model.Class`, the `FOR UPDATE SKIP LOCKED` relay, the `LISTEN`/`NOTIFY` wake, and the `DeliverySink` tagged axis.
- [14]-[SLO](.planning/execution/slo.md): the closed `Objective` vocabulary, the multi-window multi-burn-rate fold over the `@effect/sql-pg` signal series, the `AlertRoute` over the outbox `DeliverySink`, the `@opentelemetry/sdk-metrics`/`@opentelemetry/sdk-trace-node` reader, and the `@effect/cluster` `ClusterMetrics` five-gauge saturation source.
- [15]-[RPC](.planning/messaging/rpc.md): the one `RpcGroup` and the `WorkflowProxy` projection over the durable workflows.
- [16]-[ENTITY](.planning/messaging/entity.md): the addressable `Entity` actor, single-writer-per-key placement, the `EntityProxy` projection, the CRDT op-log decode boundary, and the `DeliverAt` delayed-delivery and `EntityResource` per-key resource modality rows.
- [17]-[QUOTA](.planning/messaging/quota.md): the per-tenant aggregate-backpressure `QuotaGovernor` `Entity` placement over the `@effect/experimental` `RateLimiter`, above the per-key `@effect/workflow` `DurableRateLimiter`.
- [18]-[AUTH](.planning/security/auth.md): the one `Authn` `Effect.Service` over the `AuthCommand` request axis — TOTP (`otplib`), WebAuthn attestation/assertion (`@simplewebauthn/server`), OAuth exchange, API-key — with the replay-defeating counter/time-step advance; the server-side verifier the browser ceremony forwards to.
- [19]-[SECRET](.planning/security/secret.md): the one `SecretStore` `Effect.Service` over `ConfigProvider`, the `SecretRef` provider axis (EscEnv/Doppler/Static settled, AwsSecretsManager gated), the TTL-leased rotation-invalidated cache, and the audit receipt — built after `security/auth` since it feeds every owner's keys.
- [20]-[RUNTIME](.planning/agent/runtime.md): the `DurableAgent` composed from `Activity` + `@effect/ai` `Toolkit`/`Chat.layerPersisted` on `ClusterEngine`, journaling `AgentJournal`.
- [21]-[MCP](.planning/agent/mcp.md): the node MCP transport — host tool-catalog decode into an `@effect/ai` `Toolkit`, progress SSE fold, cost-preview gate, receipt reconstruction, resume-token replay, decoding the C# `Agent/mcp#TS_PROJECTION` shapes.
- [22]-[SESSION](.planning/agent/session.md): the `AgentSessionActor` `Entity` pinning one agent conversation single-writer-per-session and the durable session lifecycle.
- [23]-[CONTRACT](.planning/provisioning/contract.md): the data/compute/observe tier model, the cloud/self-hosted dispatch, secrets, `StackOutputs`, `PolicyGuard`, and the `ObservabilityStack`.
- [24]-[DRIFT](.planning/provisioning/drift.md): the `previewRefresh` drift fold, the typed `StackDriftSummary` receipt, the continuous `DriftSweep` cron, and the CI drift gate.

## [2]-[DOMAIN_PACKAGES]

Every services-domain package the folder uses, planned or implemented; versions are centralized in the one workspace catalog. API evidence lives under `.api/`.

[DURABLE_EXECUTION]:
- `@effect/platform-node`
- `@effect/cluster`
- `@effect/workflow`
- `@effect/experimental`
- `@effect/cli`

[RPC_MESSAGING]:
- `@effect/rpc`
- `rfc6902`
- `ioredis`

[SQL]:
- `@effect/sql`
- `@effect/sql-pg`

[AI_PROVIDERS]:
- `@effect/ai`
- `@effect/ai-anthropic`
- `@effect/ai-openai`
- `@effect/ai-google`
- `@effect/ai-amazon-bedrock`
- `@effect/ai-openrouter`

[OTEL_SDK]:
- `@opentelemetry/sdk-trace-node`
- `@opentelemetry/sdk-metrics`

[OBJECT_STORE_EMAIL]:
- `@effect-aws/client-s3`
- `@aws-sdk/client-s3`
- `@aws-sdk/s3-request-presigner`
- `@aws-sdk/client-sesv2`
- `nodemailer`

[ASSET_CODECS]:
- `exceljs`
- `papaparse`
- `jspdf`
- `jszip`
- `sharp`

[AUTH]:
- `@simplewebauthn/server`
- `otplib`

[IAC]:
- `@pulumi/pulumi`
- `@pulumi/aws`
- `@pulumi/awsx`
- `@pulumi/kubernetes`
- `@pulumi/docker`
- `@pulumi/command`
- `@pulumi/random`
- `@pulumi/policy`
- `@pulumi/esc-sdk`
- `@dopplerhq/node-sdk`

[TEST_HARNESS]:
- `testcontainers`

## [3]-[SUBSTRATE_PACKAGES]

Branch-level substrate packages consumed by this folder; charters and API evidence live in `libs/typescript/.planning/README.md` and `libs/typescript/.api/`.

[RUNTIME_CORE]:
- `effect`

[OBSERVABILITY]:
- `@effect/opentelemetry`

[IDENTITY]:
- `hash-wasm`

[TEST_SUBSTRATE]:
- `@effect/vitest`
- `fast-check`
- `@stryker-mutator/core`
