# [SERVICES_IDEAS]

The forward pool of higher-order concepts for the node tier and its hosting IaC. Each open card is a bracketed slug leader plus the capability, what it unlocks, and the gap or modern technique it draws on; an idea drives one or more `TASKLOG.md` tasks. Closed ideas move to `[2]-[CLOSED]` with a one-line disposition so the same idea is never re-litigated.

## [1]-[OPEN]

[TRANSACTIONAL_OUTBOX]
- A reliable-event-publishing owner: a single outbox table written in the SAME Postgres transaction as the domain mutation, drained by a durable relay using a `FOR UPDATE SKIP LOCKED` claim plus a `LISTEN`/`NOTIFY` wake, publishing to the internal RPC surface and external sinks, with logical replication as the push variant.
- Closes the dual-write gap so no domain change ever commits without its event eventually publishing; gives `EventJournal` and `Notifications` the atomic-publish contract they lack and a foundation for trustworthy event-driven node flows.
- The transactional-outbox-plus-CDC pattern the durable-messaging field treats as the dual-write remedy; current persistence has no atomic domain-write-plus-event guarantee. Net-new `eventing/` sub-domain.

[ENTITY_ACTOR_MODEL]
- Adopt the cluster addressable-`Entity` actor surface — `RpcGroup`-backed entities with `toLayer` behavior handlers, a per-entity client, and `getShardGroup`/`getShardId` placement — for stateful per-key actors such as collaborative document sessions, per-tenant rate governors, and live presence; distinct from the request-driven workflow engine.
- A whole class of stateful, sharded, single-writer-per-key concurrency the workflow engine cannot model; makes `messaging/` a real two-modality surface of request-RPC plus addressable-actor instead of RPC-only.
- The cluster `Entity` actor model is fully shipped and verified but entirely unused; the pages use only `ClusterWorkflowEngine`.

[DURABLE_AGENT]
- Recompose the AI activity onto workflow `DurableAgent`: an AI agent that maintains state across workflow steps, calls tools through an Effect toolkit, and survives interrupt/resume, over the existing one `AiProvider` literal axis and the `AgentJournal` ledger.
- Multi-step, tool-using, durably-resumable AI agents rather than one-shot `generateText`/`generateObject`, with `AgentJournal` becoming the agent's real checkpoint ledger and provider-agnostic agent logic chosen at runtime.
- `DurableAgent` is the current Effect AI durable-agent primitive; `durable-execution/ai-activity` hand-composes a single-shot activity instead.

[RRF_BM25_FUSION]
- Upgrade hybrid-search fusion from hand-weighted weighted-sum over `ts_rank_cd` to Reciprocal Rank Fusion over true BM25 (pg_search/ParadeDB or Tiger pg_textsearch + pgvector), keeping the four-signal scaffold and the cross-encoder rerank as the terminal stage.
- Rank quality at Elasticsearch-hybrid-search level: RRF is scale-invariant across heterogeneous signal scores where weighted-sum needs fragile per-signal normalization, and BM25 is materially stronger lexical relevance than `ts_rank_cd`.
- RRF-over-BM25 is the current Postgres hybrid-search technique; `hybrid-search/fused-rank` rests on the weaker weighted-sum over `ts_rank_cd`.

[CONTINUOUS_DRIFT_SWEEP]
- Close the one-shot-vs-continuous gap in `StackDrift`: the `previewRefresh` drift fold, the typed `StackDriftSummary` receipt, and the CI `drift` verb are already designed; the net-new is the SCHEDULED sweep — a `runtime-backplane` cluster singleton or shard-pinned cron that re-runs the fold off the deploy CLI and emits the typed receipt into the `ObservabilityStack` collector, plus ESC policy-pack environment resolution at sweep time.
- Day-2 operations: out-of-band infra changes are detected continuously and self-report a typed receipt to dashboards, not only on a manual CI invocation, so a divergence surfaces between deploys rather than at the next deploy.
- Pulumi's first-class refresh-preview drift detection and ESC policy-pack environment resolution; the authored fold runs only on the `drift` verb and emits no receipt to observability on a schedule.

## [2]-[CLOSED]

None.
