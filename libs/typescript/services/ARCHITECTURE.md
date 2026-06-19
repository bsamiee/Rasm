# [SERVICES_ARCHITECTURE]

The professional domain map of the `services` folder — the host-free node tier of the TypeScript branch and the deploy-time IaC that hosts it, as one concern. The map is a codemap tree of every sub-domain, each with a one-line charter. The sub-domains mirror the eventual source sub-trees; each page is one transcription unit. The flat-module mandate holds except for two subpath closures — `provisioning/` (the `@pulumi/*` deploy-time tier behind `./provisioning`) and `agent/` (the durable-AI-agent tier behind `./agent`). Boundaries and wires live on the tasks that build them, never a standalone ledger.

## [1]-[DOMAIN_MAP]

```text codemap
services/
├── agent/                          # the ./agent subpath closure — durable multi-step AI agents, host tool transport, agent-session actors
│   ├── agent-runtime.md            # the DurableAgent COMPOSED from Activity + @effect/ai Toolkit/Tool/Chat.layerPersisted on ClusterEngine, journaling AgentJournal (no stock DurableAgent primitive exists)
│   ├── mcp-transport.md            # the node MCP transport: decodes the C# mcp-projection#TS_PROJECTION host tool-catalog into an @effect/ai Toolkit, progress SSE fold via Sse.makeParser, cost-preview gate, ReceiptEnvelopeWire reconstruction, Last-Event-ID resume replay
│   └── session-actors.md           # the AgentSessionActor Entity pinning one agent conversation single-writer-per-session, the durable session lifecycle, the journal-cursor resume
├── durable-execution/              # request-driven durable work tier over the cluster-backed WorkflowEngine
│   ├── engine.md                   # the closed durable-unit family (workflow/activity/clock/deferred/queue/rate-limiter) + ClusterEngine wiring + the DurableFault rail
│   ├── saga.md                     # the SagaStep chain, the StepOutcome/SagaTerminal fold, the engine-compensated saga workflow
│   └── ai-activity.md              # the one AiProvider literal axis, the single AI activity, the AgentJournal ledger, the Resilience primitives
├── authn/                          # the server-side credential-verification tier the browser ceremony forwards to
│   └── verifier.md                 # the one Authn Effect.Service over the AuthCommand request axis — TOTP (otplib), WebAuthn attestation/assertion (@simplewebauthn/server), OAuth exchange, API-key, with the replay-defeating counter/time-step advance
├── persistence/                    # the single sql-pg PgClient + Migrator boundary and the entity-model registry
│   ├── store-boundary.md           # the one PgClient/Migrator boundary + the ~15-entity one-Model.Class-per-entity registry
│   ├── tenancy.md                  # the multi-tenant RLS axis over the app.current_tenant GUC + the purge handler family
│   ├── work-and-signals.md         # jobs/DLQ, event-journal, notifications, the feature-flag buckets (the asset-export codec fan-out now owned by object-store)
│   ├── reactive-query.md           # the read-side ReactiveQuery owner: SqlClient.reactive over the @effect/experimental Reactivity key-scoped invalidation, entity-keyed query re-run as a Stream
│   └── idempotency-ledger.md       # the cross-surface exactly-once IdempotencyLedger Model.Class + the ClaimOrReplay claim-state fold, serving outbox/entity/session beyond the per-surface SaveResult.Duplicate/idempotencyKey primitives
├── hybrid-search/                  # first-class fused retrieval distinct from raw persistence
│   ├── fused-rank.md               # the RRF-over-BM25 semantic+lexical+trigram+phonetic fused-rank owner + the post-fusion rerank stage
│   └── embedding-lifecycle.md      # the EmbeddingProfile migration + the EmbeddingModel.makeDataLoader batch-embed orchestration feeding the fused-rank vector arm (profiles only the pgvector column, never the gated BM25 index)
├── messaging/                      # the internal TS-to-TS RPC surface and the addressable-actor modality
│   ├── internal-rpc.md             # the one RpcGroup + the WorkflowProxy projection over the durable workflows
│   ├── entity.md                   # the addressable Entity actor — single-writer-per-key placement, ClusterSchema.Persisted, EntityProxy projection, the CRDT op-log decode boundary, plus the DeliverAt per-key delayed-delivery and the EntityResource restart-surviving per-key resource modality rows
│   └── quota-governor.md           # the per-tenant aggregate-backpressure QuotaGovernor Entity placement over the @effect/experimental RateLimiter, above the per-key @effect/workflow DurableRateLimiter
├── observability/                  # the SLO/error-budget read model — the node analog of platform/web-vitals
│   └── slo-budget.md               # the closed Objective vocabulary + the multi-window multi-burn-rate fold over the @effect/sql-pg signal series + the AlertRoute over the outbox DeliverySink + the @opentelemetry/sdk-metrics + @opentelemetry/sdk-trace-node reader + the @effect/cluster ClusterMetrics five-gauge saturation source
├── secrets/                        # the runtime secret-resolution and rotation owner the whole node tier reads
│   └── secret-store.md             # the one SecretStore Effect.Service over ConfigProvider + the SecretRef provider axis (EscEnv/Doppler/Static settled, AwsSecretsManager gated) + the TTL-leased rotation-invalidated cache + the audit receipt
├── object-store/                   # the first-class blob/object-store tier, collapsing the scattered S3 presign
│   └── object-store.md             # the one ObjectStore Effect.Service over @effect-aws/client-s3 + the ObjectKey content-addressed brand + the PresignGrant verb axis + the lifecycle policy rows + the asset-codec fan-out
├── runtime-backplane/              # the runner placement and durable-scheduling substrate beneath the cluster
│   └── backplane.md                # the four-row runner backplane + the snowflake id source + cluster singletons + shard-pinned cron
├── provisioning/                   # the two-mode IaC tier behind the ./provisioning subpath
│   ├── contract.md                 # the data/compute/observe tier model, the cloud/self-hosted Match dispatch, StackOutputs, secrets, PolicyGuard, the ObservabilityStack
│   └── drift.md                    # the previewRefresh drift fold, the typed StackDriftSummary receipt, the continuous DriftSweep cron, the CI drift gate
└── eventing/                       # transactional-outbox reliable event publishing
    └── transactional-outbox.md     # the same-txn Outbox Model.Class + the FOR UPDATE SKIP LOCKED relay + the LISTEN/NOTIFY wake + the DeliverySink tagged axis, logical-replication as the push variant
```

The `agent/` sub-domain is the one `./agent` subpath closure — three pages: `agent-runtime` (the composed `DurableAgent`, since no stock `DurableAgent` primitive exists in `@effect/ai`/`@effect/workflow`), `mcp-transport` (decodes the C# `mcp-projection#TS_PROJECTION` MCP wire shapes into an `@effect/ai` `Toolkit` and an SSE progress fold), and `session-actors` (the agent conversation as a single-writer-per-session `Entity`, holding its `Chat` resource through the `messaging/entity#ENTITY` `EntityResource` ROW). `persistence/` holds four pages — the write-side `store-boundary`, the `tenancy` RLS axis, the durable `work-and-signals`, the read-side `reactive-query`, and the cross-surface `idempotency-ledger`. `hybrid-search/` holds two — the `fused-rank` retrieval owner and the `embedding-lifecycle` migration/batch-embed orchestration feeding its vector arm. `messaging/` holds three pages — the request-driven `internal-rpc` proxy, the addressable-`Entity` actor modality `entity` (carrying the `DeliverAt` and `EntityResource` per-key modality ROWs), and the per-tenant `quota-governor` Entity placement. `observability/`, `secrets/`, and `object-store/` each hold their one flat-module page. `eventing/` holds its first page — the transactional-outbox owner closing the atomic-publish gap `persistence/work-and-signals#WORK_AND_SIGNALS` `EventJournal`/`Notifications` lack.

## [2]-[FLAT_SOURCE_NOTE]

The eventual source tree is flat modules — `persistence.ts`, `hybrid-search.ts`, `durable-execution.ts`, `messaging.ts`, `eventing.ts`, `authn.ts`, plus two sub-folders behind their own subpaths (`provisioning/` behind `./provisioning`, `agent/` behind `./agent`) — so a sub-domain owns one or more pages where a real taxonomy exists and one page where the source is one module. `durable-execution` splits engine, saga, and AI activity; `persistence` splits store, tenancy, work-and-signals, and the read-side reactive-query; `provisioning` splits the contract and the drift fold; `messaging` splits the request-driven proxy and the addressable-entity actor; `agent` splits the agent runtime, the MCP transport, and the session actors; `authn`, `hybrid-search`, `runtime-backplane`, and `eventing` each carry one page. The `authn` verifier is the server-side credential concern the `platform/identity-session/auth-session` browser owner forwards its ceremony result to over the `interchange` `CommandGateway` — the browser owns only the ceremony, the attestation/secret verification is owned here, and a browser-side `@simplewebauthn/server`/`otplib` verifier is the named defect. The build order is persistence-first (the cluster rides the one `PgClient`), then hybrid-search over the same client, then durable-execution composing the cluster over the runtime-backplane, then messaging deriving `WorkflowProxy` over the one `RpcGroup` and the addressable `Entity` over the same shard manager, then eventing's outbox over the same `PgClient`, then the `agent/` subpath composing the durable agent over durable-execution, with `provisioning/` the leaf subpath the node entry consumes last.
