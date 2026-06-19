# [SERVICES]

The host-free node tier of the TypeScript branch (Nx scope `scope:node`) and the deploy-time IaC that hosts it, as one concern. It is lib-grade general and meta capability — durable execution, durable AI agents, the TS-owned Postgres store, hybrid search, internal RPC and addressable actors, transactional-outbox eventing, provisioning — deliberately NOT coupled to the AEC or Rhino pipelines; it consumes the C# wire only through interchange and projection and owns no geometry. This README routes the design pages under `.planning/` and registers every external package the folder uses; the domain folder-map is in `ARCHITECTURE.md`, the forward concepts in `IDEAS.md`, open work in `TASKLOG.md`.

## [1]-[ROUTER]

The design pages under `.planning/`, grouped by sub-domain in build order; `ARCHITECTURE.md` carries the flat-module mapping and the build-chain rationale.

- [authn/verifier](.planning/authn/verifier.md): the one `Authn` `Effect.Service` over the `AuthCommand` request axis — TOTP (`otplib`), WebAuthn attestation/assertion (`@simplewebauthn/server`), OAuth exchange, API-key — with the replay-defeating counter/time-step advance; the server-side verifier the browser ceremony forwards to.
- [persistence/store-boundary](.planning/persistence/store-boundary.md): the single `PgClient`/`Migrator` boundary and the one-`Model.Class`-per-entity registry.
- [persistence/tenancy](.planning/persistence/tenancy.md): the multi-tenant RLS axis over the `app.current_tenant` GUC, the lifecycle, and the purge-handler family.
- [persistence/work-and-signals](.planning/persistence/work-and-signals.md): jobs/DLQ, the event journal, notifications, the asset-export codec axis, the feature-flag buckets.
- [hybrid-search/fused-rank](.planning/hybrid-search/fused-rank.md): the RRF-over-BM25 semantic+lexical+trigram+phonetic fused-rank owner and the post-fusion `rerank` stage.
- [runtime-backplane/backplane](.planning/runtime-backplane/backplane.md): the four-row runner backplane, the snowflake id source, the cluster singletons, the shard-pinned cron.
- [durable-execution/engine](.planning/durable-execution/engine.md): the closed durable-unit family, the `ClusterEngine` wiring, the `DurableFault` rail.
- [durable-execution/saga](.planning/durable-execution/saga.md): the `SagaStep` chain, the `StepOutcome`/`SagaTerminal` fold, the engine-compensated saga workflow.
- [durable-execution/ai-activity](.planning/durable-execution/ai-activity.md): the `AiProvider` literal axis, the single AI activity, the `AgentJournal` ledger, the `Resilience` primitives.
- [messaging/internal-rpc](.planning/messaging/internal-rpc.md): the one `RpcGroup` and the `WorkflowProxy` projection over the durable workflows.
- [messaging/entity](.planning/messaging/entity.md): the addressable `Entity` actor, single-writer-per-key placement, the `EntityProxy` projection, the CRDT op-log decode boundary.
- [eventing/transactional-outbox](.planning/eventing/transactional-outbox.md): the same-txn `Outbox` `Model.Class`, the `FOR UPDATE SKIP LOCKED` relay, the `LISTEN`/`NOTIFY` wake, the `DeliverySink` tagged axis.
- [agent/agent-runtime](.planning/agent/agent-runtime.md): the `DurableAgent` composed from `Activity` + `@effect/ai` `Toolkit`/`Chat.layerPersisted` on `ClusterEngine`, journaling `AgentJournal`.
- [agent/mcp-transport](.planning/agent/mcp-transport.md): the node MCP transport — host tool-catalog decode into an `@effect/ai` `Toolkit`, progress SSE fold, cost-preview gate, receipt reconstruction, resume-token replay, decoding the C# `mcp-projection#TS_PROJECTION` shapes.
- [agent/session-actors](.planning/agent/session-actors.md): the `AgentSessionActor` `Entity` pinning one agent conversation single-writer-per-session, the durable session lifecycle.
- [provisioning/contract](.planning/provisioning/contract.md): the data/compute/observe tier model, the cloud/self-hosted dispatch, secrets, `StackOutputs`, `PolicyGuard`, the `ObservabilityStack`.
- [provisioning/drift](.planning/provisioning/drift.md): the `previewRefresh` drift fold, the typed `StackDriftSummary` receipt, the continuous `DriftSweep` cron, the CI drift gate.

The `agent/` sub-domain is the one `./agent` subpath closure (agent-runtime + mcp-transport + session-actors); `provisioning/` is the `./provisioning` subpath closure. Both keep the `@pulumi/*`/agent deploy- and AI-time closures off the durable runtime hot path through their subpath `exports`.

## [2]-[PACKAGES]

Every external package the folder uses, planned or implemented, grouped by concern. Versions are centralized in the one workspace catalog; this registry carries no pin and no `.api` link.

- Effect runtime: `@effect/platform-node`
- Durable execution: `@effect/cluster`, `@effect/workflow`, `@effect/experimental`
- Internal RPC: `@effect/rpc`
- Persistence: `@effect/sql`, `@effect/sql-pg`
- AI: `@effect/ai`, `@effect/ai-anthropic`, `@effect/ai-openai`, `@effect/ai-google`, `@effect/ai-amazon-bedrock`, `@effect/ai-openrouter`
- Resilience and sync: `rfc6902`
- Cache and metrics backplane: `ioredis`
- Asset transfer codecs: `exceljs`, `papaparse`, `jspdf`, `jszip`, `sharp`
- Object store and email: `@effect-aws/client-s3`, `@aws-sdk/client-s3`, `@aws-sdk/s3-request-presigner`, `@aws-sdk/client-sesv2`, `nodemailer`
- Auth verifiers: `@simplewebauthn/server`
- CLI binding: `@effect/cli`
- Provisioning IaC: `@pulumi/pulumi`, `@pulumi/aws`, `@pulumi/awsx`, `@pulumi/kubernetes`, `@pulumi/docker`, `@pulumi/command`, `@pulumi/random`, `@pulumi/policy`, `@pulumi/esc-sdk`, `@dopplerhq/node-sdk`
- Observability: `@opentelemetry/sdk-trace-node`, `@opentelemetry/sdk-metrics`
- Test harness: `testcontainers`

## [3]-[CROSS_CUTTING]

Branch-level cross-cutting packages consumed by this folder.

- `effect` — the core runtime, `Effect.Service`, `Schema`, `Layer`, `Config`, and `Stream` substrate
- `@effect/opentelemetry` — the OTel trace/metric exporter edge
- `@effect/vitest` — the Effect-aware test runner binding the property spine to the durable and persistence owners
- `fast-check` — the algebraic property-testing arbitrary spine driving the durable law harnesses
- `@stryker-mutator/core` — the mutation kill-ratio gate over the node law spines, with its co-admitted `typescript-checker`/`vitest-runner` plugins registered once at the branch
- `otplib` — the TOTP/HOTP one-time-password verifier the `authn/verifier#VERIFIER` `Authn` service owns server-side; branch-cross-cutting, also consumed by `platform`
