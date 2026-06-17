# [SERVICES_ARCHITECTURE]

The professional domain map of the `services` folder — the host-free node tier of the TypeScript branch and the deploy-time IaC that hosts it, as one concern. The map is a codemap tree of every sub-domain, including the planned-but-empty `eventing` sub-domain that holds no design page yet, each with a one-line charter. The sub-domains mirror the eventual source sub-trees; each page is one transcription unit. Boundaries and wires live on the tasks that build them, never a standalone ledger.

## [1]-[DOMAIN_MAP]

```text codemap
services/
├── durable-execution/              # request-driven durable work tier over the cluster-backed WorkflowEngine
│   ├── engine.md                   # the closed durable-unit family (workflow/activity/clock/deferred/queue/rate-limiter) + ClusterEngine wiring + the DurableFault rail
│   ├── saga.md                     # the SagaStep chain, the StepOutcome/SagaTerminal fold, the engine-compensated saga workflow
│   └── ai-activity.md              # the one AiProvider literal axis, the single AI activity, the AgentJournal ledger, the Resilience primitives
├── persistence/                    # the single sql-pg PgClient + Migrator boundary and the entity-model registry
│   ├── store-boundary.md           # the one PgClient/Migrator boundary + the ~15-entity one-Model.Class-per-entity registry
│   ├── tenancy.md                  # the multi-tenant RLS axis over the app.current_tenant GUC + the purge handler family
│   └── work-and-signals.md         # jobs/DLQ, event-journal, notifications, the asset-export codec axis, the feature-flag buckets
├── hybrid-search/                  # first-class fused retrieval distinct from raw persistence
│   └── fused-rank.md               # the fused semantic+lexical+trigram+phonetic weighted-rank owner + the post-fusion rerank stage
├── messaging/                      # the internal TS-to-TS RPC surface and the addressable-actor modality
│   └── internal-rpc.md             # the one RpcGroup + the WorkflowProxy projection over the durable workflows
├── runtime-backplane/              # the runner placement and durable-scheduling substrate beneath the cluster
│   └── backplane.md                # the four-row runner backplane + the snowflake id source + cluster singletons + shard-pinned cron
├── provisioning/                   # the two-mode IaC tier behind the ./provisioning subpath
│   ├── contract.md                 # the data/compute/observe tier model, the cloud/self-hosted Match dispatch, StackOutputs, secrets, PolicyGuard, the ObservabilityStack
│   └── drift.md                    # the previewRefresh drift fold, the typed StackDriftSummary receipt, the CI drift gate
└── eventing/                       # PLANNED — transactional-outbox reliable event publishing
                                    # atomic domain-write + outbox-row in one transaction, drained by a relay, CDC/logical-replication as the push variant
```

The `eventing/` sub-domain carries no page yet: it is the net-new transactional-outbox reliable-event-publishing concern surfaced in `IDEAS.md`, the atomic-publish guarantee `persistence/work-and-signals#WORK_AND_SIGNALS` `EventJournal` and `Notifications` lack. `messaging/` holds one page today; the addressable-`Entity` actor model is its OPEN second modality, distilled into a task rather than a present page.

## [2]-[FLAT_SOURCE_NOTE]

The eventual source tree is flat modules — `persistence.ts`, `hybrid-search.ts`, `durable-execution.ts`, plus one `provisioning/` sub-folder behind the `./provisioning` subpath — so a sub-domain owns one or more pages where a real taxonomy exists and one page where the source is one module. `durable-execution` splits engine, saga, and AI activity; `persistence` splits store, tenancy, and work-and-signals; `provisioning` splits the contract and the drift fold; `hybrid-search`, `messaging`, and `runtime-backplane` each carry one page. The build order is persistence-first (the cluster rides the one `PgClient`), then hybrid-search over the same client, then durable-execution composing the cluster over the runtime-backplane, then messaging deriving `WorkflowProxy` over the one `RpcGroup`, with `provisioning/` the leaf subpath the node entry consumes last.
