# [SERVICES_TASKLOG]

Open and closed work for the node tier, distilled from `IDEAS.md`. Each open card's leader carries a status marker — `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` — and three to four scoped bullets: the capability or file to build, the external packages to integrate, the integration points and boundaries/wires, and the key considerations. One idea spawns one or more tasks; closed work moves to `[2]-[CLOSED]`.

## [1]-[OPEN]

[QUEUED] Author the `eventing/transactional-outbox` page (from TRANSACTIONAL_OUTBOX)
- Build the new `eventing/` sub-domain's first page: one `Outbox` `Model.Class` written in the same transaction as the domain mutation, an `OutboxRelay` draining via `FOR UPDATE SKIP LOCKED` with a `LISTEN`/`NOTIFY` wake, and a `Data.TaggedEnum` over the delivery sink (internal RPC vs external), with logical replication as the push variant.
- Integrate `@effect/sql`/`@effect/sql-pg` for the transactional write, the claim, and the `LISTEN`/`NOTIFY` channel; `effect` for the relay loop and the sink fold.
- Internal: shares the one `PgClient` (`persistence/store-boundary#STORE_BOUNDARY`), wakes off the same `listen` channel pair, and gives `persistence/work-and-signals#WORK_AND_SIGNALS` `EventJournal`/`Notifications` their atomic-publish contract; publishes to `messaging/internal-rpc#INTERNAL_RPC`. No C# wire coupling.
- Key consideration: `NOTIFY` is at-most-once, 8 KB-capped, and non-persistent — the durable claim is the source of truth and `NOTIFY` is only the wake; the relay must be idempotent and exactly-once against re-delivery.

[QUEUED] Add the addressable-`Entity` actor page to `messaging/` (from ENTITY_ACTOR_MODEL)
- Build the second `messaging/` modality: an `Entity` owner with `RpcGroup`-backed behavior, `Entity.toLayer` handlers, a per-entity client, and `getShardGroup`/`getShardId` placement for stateful per-key actors (collaborative document sessions, per-tenant rate governors, live presence).
- Integrate `@effect/cluster` (the `Entity`/`Sharding` addressable-actor surface) and `@effect/rpc` for the entity behavior schema.
- Internal: rides the same `runtime-backplane/backplane#RUNNER_AND_SCHEDULING` shard manager the workflow engine layers onto, distinct from the request-driven `messaging/internal-rpc#INTERNAL_RPC` proxy; the actor persists per-key state through the one `persistence/store-boundary#STORE_BOUNDARY` `PgClient`, never a parallel store. Cross-language: a collaborative-session actor DECODES the op-log CRDT payload off the wire the C# branch owns and never re-mints it — alignment to the settled wire owner, consumed at the decoded boundary, not coupling to a C# interior.
- Key consideration: single-writer-per-key is the actor's contract — placement must pin one shard per key, and the actor must not reintroduce a parallel state store beside the one `PgClient`.

[QUEUED] Recompose `durable-execution/ai-activity` onto `DurableAgent` (from DURABLE_AGENT)
- Replace the hand-composed single-shot AI activity with a `DurableAgent` that maintains state across workflow steps, calls tools through an Effect toolkit, and survives interrupt/resume, over the existing one `AiProvider` literal axis.
- Integrate `@effect/workflow` (`DurableAgent`), the `@effect/ai*` set for the unified model and the five provider layers, `@effect/cluster` for the engine the agent resumes on.
- Internal: the agent's checkpoints become `AgentJournal` rows through `persistence/store-boundary#STORE_BOUNDARY`; the agent runs as a durable unit on `durable-execution/engine#ENGINE`; the `AiProvider` axis stays the sole declaration site. No C# wire coupling.
- Key consideration: the journal becomes the agent's real checkpoint ledger, not a side log — interrupt/resume must replay from the journal, and provider-agnostic agent logic must select the provider at runtime without a parallel agent per provider.

[QUEUED] Upgrade `hybrid-search/fused-rank` fusion to RRF-over-BM25 (from RRF_BM25_FUSION)
- Replace the weighted-sum-over-`ts_rank_cd` lexical signal with true BM25 and fuse the four signals by Reciprocal Rank Fusion, keeping the four-signal scaffold and the `RerankModel` cross-encoder terminal stage intact.
- Integrate the BM25 Postgres extension (pg_search/ParadeDB or Tiger pg_textsearch) alongside `pgvector`, through `@effect/sql-pg`; the extension is provisioned by `provisioning/contract#PROVISIONING`.
- Internal: rides the one `PgClient` (`persistence/store-boundary#STORE_BOUNDARY`) and the same tenant GUC (`persistence/tenancy#TENANCY`); the `hybridQuery` set-algebraic statement changes the fusion arm only, not the round-trip count. No C# wire coupling — the C# `federation#FUSION_RANK` is a distinct algebra.
- Key consideration: RRF is scale-invariant so the per-signal normalization the weighted-sum needs is dropped; the extension choice is a real admission (pg_search vs Tiger) gated on the provisioned Postgres image carrying it.

[QUEUED] Wire the continuous drift sweep in `provisioning/drift` (from CONTINUOUS_DRIFT_SWEEP)
- Promote the `StackDrift` fold into a continuous owner: register a scheduled drift sweep and emit the typed `StackDriftSummary` receipt to the `ObservabilityStack`, beyond the one-shot `drift` CLI verb already authored.
- Integrate `@pulumi/pulumi` (`previewRefresh`, ESC policy-pack environment resolution), `@pulumi/esc-sdk`, `@effect/cli` for the verb, and the `runtime-backplane` cluster-singleton/cron for the sweep.
- Internal: the sweep registers as a `runtime-backplane/backplane#RUNNER_AND_SCHEDULING` singleton or shard-pinned cron; the receipt emits through the `provisioning/contract#PROVISIONING` `ObservabilityStack` collector. The closure stays behind the `./provisioning` subpath off the runtime hot path. No C# wire coupling.
- Key consideration: the sweep must stay deploy-time-only — the `@pulumi/*` types stay inside the subpath closure and only the primitive-carrying receipt crosses to observability, so no `@pulumi/*` type escapes onto the durable hot path.

[BLOCKED] Add the MCP agent-transport consumer to `messaging/` (from MCP_AGENT_TRANSPORT)
- Build the node-tier MCP agent transport: read the decoded `McpToolWire` tool catalog, drain the `ProgressNotificationWire` SSE server-stream into a progress fold, read the `CostPreviewWire` dry-run pricing before a call, and reconstruct the structured tool result through the existing `ReceiptEnvelopeWire`, with the resume token replayed through the SDK `Last-Event-ID` cursor.
- Integrate `@effect/ai` (the agent toolkit consuming the tool catalog), `@effect/rpc` for the in-process edge, `@effect/experimental` for the SSE persistence/replay substrate, `effect` for the progress `Stream` fold and the cost-preview gate.
- Internal: rides the same `runtime-backplane/backplane#RUNNER_AND_SCHEDULING` substrate; the agent runs as a durable unit on `durable-execution/engine#ENGINE` and journals through `durable-execution/ai-activity#AI_ACTIVITY` `AgentJournal`; the transport consumes the C# `csharp:Rasm.AppHost/agent/mcp-projection#TS_PROJECTION` `McpToolWire`/`ProgressNotificationWire`/`CostPreviewWire` decoded shapes off the `interchange` boundary, never re-minting the tool catalog or the frame union.
- Blocked on the upstream C# MCP `TS_PROJECTION` fence promoting the `McpToolWire`/`ProgressNotificationWire`/`CostPreviewWire` shapes into the projection fence; reading the notification directly rather than reconstructing the frame union is the contract, and a branch-side MCP tool definition divorced from the host descriptor is the named defect.

## [2]-[CLOSED]

[DROPPED] Declare the implementation-time subpath `exports`
- Folded into `provisioning/contract#PROVISIONING`: the design page's Entry line already owns the `./provisioning` subpath `exports` declaration in the one workspace manifest as a transcription-time mechanic. A pure build-fence step traces to no forward idea and duplicates a settled design decision, so it is not a standalone idea-driven TASKLOG card; the `@pulumi/*`-off-the-hot-path fence is the page's boundary, enforced at transcription.
