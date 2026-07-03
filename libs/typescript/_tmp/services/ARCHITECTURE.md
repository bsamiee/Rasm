# [SERVICES_ARCHITECTURE]

The domain map of `services` — the host-free node durable interior of the TypeScript branch and its deploy-time IaC. Flat modules except two subpath closures: `provisioning/` (the `@pulumi/*` deploy-time tier behind `./provisioning`) and `agent/` (the durable-AI-agent tier behind `./agent`).

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
services/
├── persistence/        # SQL boundary, entity registry, and durable blob store
│   ├── store.ts        # One PgClient/Migrator boundary and entity-model registry
│   ├── tenancy.ts      # Multi-tenant RLS axis over current_tenant GUC
│   ├── work.ts         # Jobs/DLQ, event-journal, notifications, and feature-flag buckets
│   ├── reactive.ts     # Read-side ReactiveQuery owner over key-scoped invalidation
│   ├── idempotency.ts  # Cross-surface exactly-once IdempotencyLedger and ClaimOrReplay fold
│   └── object.ts       # One ObjectStore over @effect-aws/client-s3 with content-addressed keys
├── search/             # First-class fused retrieval distinct from raw persistence
│   ├── rank.ts         # RRF-over-BM25 fused-rank owner and post-fusion rerank stage
│   └── embedding.ts    # EmbeddingProfile migration and batch-embed orchestration
├── execution/          # Durable-runtime tier: engine, saga, AI, backplane, outbox, SLO
│   ├── engine.ts       # Closed durable-unit family, ClusterEngine wiring, and DurableFault rail
│   ├── saga.ts         # SagaStep chain and engine-compensated saga workflow
│   ├── ai.ts           # One AiProvider axis, AI activity, and AgentJournal ledger
│   ├── backplane.ts    # Runner placement/durable-scheduling substrate beneath cluster
│   ├── outbox.ts       # Same-txn Outbox with FOR UPDATE SKIP LOCKED relay and LISTEN/NOTIFY
│   └── slo.ts          # Objective vocabulary and multi-window multi-burn-rate fold
├── messaging/          # Internal TS-to-TS RPC surface and addressable-actor modality
│   ├── rpc.ts          # One RpcGroup and WorkflowProxy projection over durable workflows
│   ├── entity.ts       # Addressable Entity actor with single-writer-per-key placement
│   └── quota.ts        # Per-tenant aggregate-backpressure QuotaGovernor Entity
├── security/           # Credential/secret tier the whole node tier reads
│   ├── auth.ts         # One Authn service over AuthCommand axis (TOTP/WebAuthn/OAuth/API-key)
│   └── secret.ts       # One SecretStore over ConfigProvider with SecretRef provider axis
├── agent/              # ./agent subpath closure: durable AI agents, MCP transport, session actors
│   ├── runtime.ts      # DurableAgent composed from Activity + @effect/ai on ClusterEngine
│   ├── mcp.ts          # Node MCP transport decoding C# host tool-catalog into a Toolkit
│   └── session.ts      # AgentSessionActor Entity pinning one conversation single-writer
└── provisioning/       # Two-mode IaC tier behind ./provisioning subpath
    ├── contract.ts     # Data/compute/observe tier model and cloud/self-hosted Match dispatch
    └── drift.ts        # previewRefresh drift fold, StackDriftSummary receipt, and CI drift gate
```

`agent/` is the one `./agent` subpath closure — `runtime` the composed `DurableAgent` (no stock primitive exists in `@effect/ai`/`@effect/workflow`), `mcp` the C# MCP-wire decoder into an `@effect/ai` `Toolkit`, and `session` the single-writer-per-session agent `Entity`. Every other sub-domain is a flat module split into pages only where a real taxonomy exists, and `execution/outbox` closes the atomic-publish gap `persistence/work` `EventJournal`/`Notifications` lack.

## [02]-[SEAMS]

```text seams
*                 ←  csharp:Rasm.AppHost          # [WIRE]: CredentialPemWire redacted carrier
security/auth     ←  typescript:platform/session  # [PORT]: TOTP/WebAuthn ceremony verification
persistence       ←  typescript:platform/config   # [SHAPE]: FlagKey axis / FeatureFlags vocabulary
execution/engine  ⇄  typescript:platform/runtime  # [BOUNDARY]: app-service composition vs durable kernel
```

## [03]-[FLAT_SOURCE_NOTE]

The eventual source is flat modules — `persistence.ts`, `search.ts`, `execution.ts`, `messaging.ts`, `security.ts` — plus the two subpath sub-folders (`provisioning/`, `agent/`). The `security/auth` verifier is the server-side credential concern the `platform/Session/session` browser owner forwards its ceremony result to over the `interchange` `CommandGateway`; a browser-side `@simplewebauthn/server`/`otplib` verifier is the named defect. Build order is persistence-first (the cluster rides the one `PgClient`), then search, execution over the `backplane` substrate, messaging over the one `RpcGroup`, the `agent/` subpath over the execution tier, and `provisioning/` the leaf the node entry consumes last.
