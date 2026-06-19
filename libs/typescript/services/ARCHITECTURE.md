# [SERVICES_ARCHITECTURE]

The professional domain map of `services` — the host-free node durable interior of the TypeScript branch and its deploy-time IaC. Flat modules except two subpath closures: `provisioning/` (the `@pulumi/*` deploy-time tier behind `./provisioning`) and `agent/` (the durable-AI-agent tier behind `./agent`).

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [1]-[DOMAIN_MAP]

```text codemap
services/
├── persistence/        # the SQL boundary, entity registry, and durable blob store
│   ├── store.ts        # the one PgClient/Migrator boundary and the entity-model registry
│   ├── tenancy.ts      # the multi-tenant RLS axis over the current_tenant GUC
│   ├── work.ts         # jobs/DLQ, event-journal, notifications, and feature-flag buckets
│   ├── reactive.ts     # the read-side ReactiveQuery owner over key-scoped invalidation
│   ├── idempotency.ts  # the cross-surface exactly-once IdempotencyLedger and ClaimOrReplay fold
│   └── object.ts       # the one ObjectStore over @effect-aws/client-s3 with content-addressed keys
├── search/             # first-class fused retrieval distinct from raw persistence
│   ├── rank.ts         # the RRF-over-BM25 fused-rank owner and post-fusion rerank stage
│   └── embedding.ts    # the EmbeddingProfile migration and batch-embed orchestration
├── execution/          # the durable-runtime tier: engine, saga, AI, backplane, outbox, SLO
│   ├── engine.ts       # the closed durable-unit family, ClusterEngine wiring, and DurableFault rail
│   ├── saga.ts         # the SagaStep chain and the engine-compensated saga workflow
│   ├── ai.ts           # the one AiProvider axis, the AI activity, and the AgentJournal ledger
│   ├── backplane.ts    # the runner placement/durable-scheduling substrate beneath the cluster
│   ├── outbox.ts       # the same-txn Outbox with FOR UPDATE SKIP LOCKED relay and LISTEN/NOTIFY
│   └── slo.ts          # the Objective vocabulary and multi-window multi-burn-rate fold
├── messaging/          # the internal TS-to-TS RPC surface and the addressable-actor modality
│   ├── rpc.ts          # the one RpcGroup and the WorkflowProxy projection over durable workflows
│   ├── entity.ts       # the addressable Entity actor with single-writer-per-key placement
│   └── quota.ts        # the per-tenant aggregate-backpressure QuotaGovernor Entity
├── security/           # the credential/secret tier the whole node tier reads
│   ├── auth.ts         # the one Authn service over the AuthCommand axis (TOTP/WebAuthn/OAuth/API-key)
│   └── secret.ts       # the one SecretStore over ConfigProvider with the SecretRef provider axis
├── agent/              # the ./agent subpath closure: durable AI agents, MCP transport, session actors
│   ├── runtime.ts      # the DurableAgent composed from Activity + @effect/ai on ClusterEngine
│   ├── mcp.ts          # the node MCP transport decoding the C# host tool-catalog into a Toolkit
│   └── session.ts      # the AgentSessionActor Entity pinning one conversation single-writer
└── provisioning/       # the two-mode IaC tier behind the ./provisioning subpath
    ├── contract.ts     # the data/compute/observe tier model and cloud/self-hosted Match dispatch
    └── drift.ts        # the previewRefresh drift fold, StackDriftSummary receipt, and CI drift gate
```

`agent/` is the one `./agent` subpath closure — `runtime` the composed `DurableAgent` (no stock primitive exists in `@effect/ai`/`@effect/workflow`), `mcp` the C# MCP-wire decoder into an `@effect/ai` `Toolkit`, and `session` the single-writer-per-session agent `Entity`. Every other sub-domain is a flat module split into pages only where a real taxonomy exists, and `execution/outbox` closes the atomic-publish gap `persistence/work` `EventJournal`/`Notifications` lack.

## [2]-[SEAMS]

```text seams
*                 ←  csharp:Rasm.AppHost          # CredentialPemWire redacted carrier (wire)
security/auth     ←  typescript:platform/session  # TOTP/WebAuthn ceremony verification (port)
persistence       ←  typescript:platform/config   # FlagKey axis / FeatureFlags vocabulary (shape)
execution/engine  ⇄  typescript:platform/runtime  # app-service composition vs durable kernel (boundary)
```

## [3]-[FLAT_SOURCE_NOTE]

The eventual source is flat modules — `persistence.ts`, `search.ts`, `execution.ts`, `messaging.ts`, `security.ts` — plus the two subpath sub-folders (`provisioning/`, `agent/`). The `security/auth` verifier is the server-side credential concern the `platform/Session/session` browser owner forwards its ceremony result to over the `interchange` `CommandGateway`; a browser-side `@simplewebauthn/server`/`otplib` verifier is the named defect. Build order is persistence-first (the cluster rides the one `PgClient`), then search, execution over the `backplane` substrate, messaging over the one `RpcGroup`, the `agent/` subpath over the execution tier, and `provisioning/` the leaf the node entry consumes last.
