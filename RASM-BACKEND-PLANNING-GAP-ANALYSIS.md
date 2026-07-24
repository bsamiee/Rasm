# Rasm backend planning gap analysis

Repository root: [Rasm](/Users/bardiasamiee/Documents/99.Github/Rasm)

Audit date: 2026-07-24

## Executive verdict

Rasm already plans a broad backend platform rather than a collection of isolated utility libraries. C# owns durable AEC state, host composition, orchestration, coordination, and compute scheduling. TypeScript owns web/server runtime, PostgreSQL-backed journal and durable work, security, and the deploy plane. Python owns host-free execution, supervised worker isolation, numerical workloads, analytical data, and the companion boundary. Four-signal telemetry, typed faults, resilience, content identity, receipts, tenancy, and wire contracts already run through those owners.

Core runtime capability is not the principal deficit. Operational closure is.

Five gaps dominate the remaining backend plan:

1. PostgreSQL capability authority diverges between C# Persistence and TypeScript data/IaC. C# lacks eight TypeScript extension rows; TypeScript lacks ten C# rows. TypeScript IaC builds and provisions the server C# later verifies, and Python lacks the complete capability vocabulary it needs to price external PostgreSQL lanes.
2. Database creation exists, but schema change is framed as an incomplete chronological migration system. Rasm instead needs one current desired-state contract, immutable database generations, replay/rebuild/cutover orchestration, and fail-closed drift enforcement across PostgreSQL, PGLite, SQLite, Marten, EF, raw DDL, tenant databases, and deployment epochs.
3. Runtime work systems exist, but TypeScript IaC provisions one service-shaped Deployment with CPU HPA. It does not yet provision worker-class deployments, backlog-driven scale, scale-to-zero, or worker-safe rollout behavior.
4. Backups exist for the primary CNPG cluster, but recovery is not a complete deploy-plane concern. Cluster-per-tenant parity, restore bootstrap, restore drills, object-plane recovery, managed-cloud recovery, and recovery evidence remain incomplete.
5. Backend support is broader than backend operability. Several clients and execution engines are admitted without an explicit, uniform contract for provision, converge, replace, probe, observe, back up, restore, rotate, scale, and run locally.

No broad queue, workflow, or actor framework should be added. Existing owners already cover those semantics:

- TypeScript: `@effect/cluster`, `@effect/workflow`, the PostgreSQL journal/outbox, `WorkClass`, and the sole dead-letter owner.
- C#: `Runtime/orchestration`, `Wire/outbox`, `SchedulePort`, Persistence coordination, and Compute `WorkLane`/`JobGraph`.
- Python: `LanePolicy`, `WorkerKind`, `KernelTrait`, `WorkerPool`, `Supervisor`, and APScheduler for local cadence.

BullMQ is not a missing Rasm component. It would add Redis as another durable job authority beside the PostgreSQL journal and Effect durable-work state. Its useful ideas—delayed work, priorities, retries, leases, stalled-job recovery, workers, flow dependencies, metrics, and dead-letter handling—are already represented in Rasm’s planned vocabulary. BullMQ belongs only behind an interoperability adapter when a downstream application must participate in an existing BullMQ estate.

## Audit boundary

Review covers the complete library architecture and planning topology, with backend emphasis on:

- [central library architecture](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/.planning/ARCHITECTURE.md)
- C# kernel and AEC seam roles, with deep inspection of [Rasm.AppHost](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.AppHost), [Rasm.Persistence](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.Persistence), and [Rasm.Compute](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.Compute)
- Python [runtime](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/python/runtime), [data](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/python/data), [compute](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/python/compute), and workload-producing seams in geometry and artifacts
- TypeScript [core](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/typescript/core), [security](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/typescript/security), [data](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/typescript/data), [runtime](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/typescript/runtime), and [iac](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/typescript/iac)
- central package manifests: [Directory.Packages.props](/Users/bardiasamiee/Documents/99.Github/Rasm/Directory.Packages.props), [pyproject.toml](/Users/bardiasamiee/Documents/99.Github/Rasm/pyproject.toml), and [pnpm-workspace.yaml](/Users/bardiasamiee/Documents/99.Github/Rasm/pnpm-workspace.yaml)
- every package `README.md`, `ARCHITECTURE.md`, `RULINGS.md`, `IDEAS.md`, `TASKLOG.md`, backend design page, and relevant package/API catalogue

This report evaluates planned design, not implemented runtime behavior. “Complete” means decision-complete in the planning corpus. “Partial” means the owner exists but a tracked task, blocked research item, missing operational arm, or cross-owner contradiction prevents closure.

## Architectural model

### Platform strata

| Stratum | Primary owners | Backend responsibility |
| --- | --- | --- |
| C# kernel | `Rasm` | Content identity, causal and receipt vocabulary, typed faults, geometry source, shared wire seeds |
| C# AEC domain | `Rasm.Element`, `Rasm.Bim`, `Rasm.Materials`, `Rasm.Fabrication` | Durable domain shapes and workload inputs; no infrastructure ownership |
| C# app platform | `Rasm.AppHost`, `Rasm.Persistence`, `Rasm.Compute`, `Rasm.AppUi` | Process boot, orchestration, durable state, coordination, compute scheduling, operator surfaces |
| C# host boundary | `Rasm.Rhino`, `Rasm.Grasshopper` | Host capture, event production, UI-thread and document boundaries |
| Python runtime | `runtime` | Host-free execution, companion serving, worker isolation, resilience, observability |
| Python workload libraries | `data`, `compute`, `geometry`, `artifacts` | Analytical, simulation, geometry, rendering, and scientific work over runtime rails |
| TypeScript core/security/data/runtime | `core`, `security`, `data`, `runtime` | Web/server vocabulary, identity, journal/object/read planes, transport, durable work, service edge |
| TypeScript deploy plane | `iac` | Pulumi program, provider dispatch, Kubernetes and container estate, secrets, policy, observability |

Cross-language coupling is correctly restricted to C#-owned wire values, content identity, capability descriptors, HLC, receipts, and the companion/offline seams. Libraries align without importing one another’s internals. This law must also govern backend capabilities: one C#-minted wire descriptor carries the full PostgreSQL capability superset and the schema-generation contract; C#, Python, and TypeScript generate or decode complete local projections without importing one another’s internals.

### Complete library estate relevance

Every library participates at a different altitude. Backend completeness does not require every package to own infrastructure.

| Language | Library | Backend relation |
| --- | --- | --- |
| C# | [Rasm](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm) | Kernel authority for content identity, causal values, faults, receipts, geometry payloads, and policies every backend owner consumes |
| C# | [Rasm.Element](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.Element) | Shared `ElementGraph` seam persisted by Persistence and consumed by Compute; domain shape, not infrastructure |
| C# | [Rasm.Bim](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.Bim) | BIM/IFC semantic producer and interchange consumer; emits durable domain facts and remote-analysis inputs |
| C# | [Rasm.Materials](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.Materials) | Material and section data feeding durable graph, analysis, evidence, and cost/impact workloads |
| C# | [Rasm.Fabrication](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.Fabrication) | Machine/process observations and fabrication workflows feeding AppHost wire, telemetry, and persistence |
| C# | [Rasm.Persistence](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.Persistence) | Primary durable-state, coordination, query, object, CDC, recovery, and database capability owner |
| C# | [Rasm.Compute](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.Compute) | Bounded execution, dependency scheduling, remote compute, broker ingest, and compute evidence |
| C# | [Rasm.AppHost](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.AppHost) | Process composition, durable orchestration, scheduling, outbox, transport, security brokering, health, and telemetry |
| C# | [Rasm.AppUi](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.AppUi) | Operator and application projection over command, health, progress, query, and evidence surfaces; no durable authority |
| C# | [Rasm.Rhino](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.Rhino) | Host-bound capture and mutation producer; emits content-keyed and causal facts into platform owners |
| C# | [Rasm.Grasshopper](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.Grasshopper) | Host-bound graph execution and async result producer; enters Compute/AppHost lanes rather than owning workers |
| Python | [runtime](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/python/runtime) | Execution, isolation, transport, supervision, reliability, telemetry, and companion server |
| Python | [data](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/python/data) | Analytical query, Arrow/ADBC, lakehouse, object egress, spatial data, and data contracts |
| Python | [compute](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/python/compute) | Numerical workload and evidence producer over runtime workers |
| Python | [geometry](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/python/geometry) | Host-free geometry, IFC, scan, tessellation, and simulation workload producer |
| Python | [artifacts](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/python/artifacts) | Report, media, visualization, and render workloads; consumes runtime execution and object egress |
| TypeScript | [core](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/typescript/core) | Fault, budget, observability, content, wire-decode, and platform vocabulary |
| TypeScript | [security](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/typescript/security) | Authentication, authorization, cryptography, audit ports, secret and lease contracts |
| TypeScript | [data](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/typescript/data) | Journal, relational engines, object plane, query, live reads, search, and tenancy |
| TypeScript | [runtime](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/typescript/runtime) | Process, service, transport, durable work, worker thread, AI, and telemetry execution |
| TypeScript | [iac](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/typescript/iac) | Provision, policy, secrets realization, observability backend, deployment, networking, and cloud arms |
| TypeScript | [ui](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/typescript/ui) | Browser/operator projections and served assets; no database, queue, or deployment authority |

Backend work should therefore land only in a package whose altitude owns the concern. A domain producer may publish a descriptor, receipt, or workload value; AppHost/runtime/data/IaC owners decide how it persists, executes, scales, and recovers.

### Backend control and data flow

| Concern | Authority | Execution or projection |
| --- | --- | --- |
| AEC durable truth | C# `Rasm.Persistence` | Marten/PostgreSQL, embedded SQLite, object store, version engine |
| TypeScript application truth | TypeScript `data` journal | PostgreSQL in service deployments; SQLite-family and planned PGLite profiles locally |
| C# durable workflows | C# `Rasm.AppHost/Runtime/orchestration` | Persistence-owned fenced step state, outbox, schedule port |
| TypeScript durable workflows | TypeScript `runtime/work` | Effect cluster/workflow state and PostgreSQL journal/outbox semantics |
| Python execution | Python `runtime/execution` | Threads, subinterpreters, processes, GPU, WASM, daemons, remote hosts |
| C# compute execution | C# `Rasm.Compute/Runtime` | Bounded `WorkLane`, dependency `JobGraph`, remote gRPC farm |
| Infrastructure | TypeScript `iac` | Pulumi Automation API, Kubernetes, Docker, AWS, GCP, Cloudflare |
| Telemetry | Per-language SDK owner, canonical central meaning | OTLP gateway and Grafana stack from TypeScript IaC |
| Secrets | TypeScript security vocabulary and IaC custody | Doppler canonical store; provider-specific delivery and rotation |

Rasm is intentionally not gbrain’s Markdown/Git-plus-database dual plane. Rasm’s repository is the design and source corpus; application state is owned by the runtime libraries. Architectural discipline is the useful gbrain lesson: distinguish durable authority, derived indexes, execution state, worker state, and recoverable artifacts explicitly. Rasm should not introduce a second authority merely because a package makes a feature convenient.

## Existing backend capability

### Cross-cutting platform substrate

Rasm already plans the concerns commonly missing from reusable libraries:

- typed configuration and admission before resource construction
- Doppler-backed secret custody, generated credentials, workload identity work, rotation evidence, and lease contracts
- four-signal telemetry with aligned metric, trace, log, and profile meaning
- typed fault bands, retry classes, deadlines, budgets, and degradation
- health snapshots, dependency probes, readiness, gRPC health, drain, and graceful shutdown
- tenant identity and row-level isolation vocabulary
- content-addressed artifacts and object-store conditional-create law
- CloudEvents and Protobuf wire boundaries
- transactional outbox, idempotency, fencing tokens, HLC order, watermarks, and dead letters
- durable scheduling, signals, compensation, crash recovery, and orphan reclaim in C#
- actors, workflows, durable queues, schedules, delivery, and report workers in TypeScript
- bounded structured concurrency, worker placement, kill semantics, supervision, and remote execution in Python
- policy-as-code, drift evidence, image digest enforcement, network fences, secrets, observability, and hosted automation in IaC

Existing designs are not placeholders. Planning pages contain concrete types, state transitions, package members, fault mappings, storage boundaries, and composition rules. Missing work should extend these owners rather than import parallel frameworks.

### TypeScript data plane

[TypeScript data](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/typescript/data/README.md) is a coherent application data plane:

- `lane/` defines backend guarantees and fail-closed capability proof.
- `journal/` owns append-only truth, outbox, idempotency, upcasting, retention, and facts.
- `object/` owns content-addressed file, stream, remote, and store behavior.
- `read/` owns query, batch, fold, live, and search projections.
- `Tenant.within` pins transaction-local tenant context.
- PostgreSQL primitives include `SKIP LOCKED`, advisory locks, `LISTEN/NOTIFY`, conflict claims, `MERGE`, `COPY`, and partitioning.
- Queue semantics remain journal semantics. `LISTEN/NOTIFY` reduces poll latency but never carries durable delivery.
- PostgreSQL extension capability is a typed matrix consumed by runtime verification and IaC image/provisioning.
- SQLite, LibSQL, D1, MySQL, MSSQL, ClickHouse, DuckDB, Flight SQL, Arrow, Parquet, S3, TUS, FTP, WebDAV, and SSH already have admitted package lanes or planned rows.

PostgreSQL and PGLite should be one profile family, not synchronized databases. PGLite supplies an embedded PostgreSQL-compatible local engine. Production PostgreSQL supplies multi-process, pooling, operator, backup, extension, and tenancy capabilities. A caller receives one `SqlClient`-shaped lane and the capability table determines what the selected profile can uphold.

Root TypeScript manifest already carries `@electric-sql/pglite` as a development dependency, but the data package does not yet admit `@effect/sql-pglite`. Planned `RELATIONAL_SET_COMPLETION` and `INTEROP_LANE_PAGE` work is therefore a real incomplete integration, not a new direction.

### TypeScript runtime

[TypeScript runtime](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/typescript/runtime/README.md) already contains the BullMQ-class functionality relevant to Rasm:

- `work/entity.md`: durable actors and serialized single-writer entity mailboxes
- `work/flow.md`: durable workflow steps, suspension, replay, and durable deadlines
- `work/queue.md`: typed durable queue families, dedup, class-priced concurrency, retry schedules, claim leases, parking, dead-letter evidence, and replay
- `work/schedule.md`: durable schedules
- `work/deliver.md`: transactional-outbox delivery for mail and webhooks
- `work/report.md`: bounded report materialization and off-thread render work
- `net/pubsub.md`: NATS JetStream and Kafka fanout/replay adapters
- `net/coordinate.md`: NATS KV and browser coordination
- `proc/worker.md`: off-thread worker protocol for CPU-heavy JavaScript

`WorkClass` is the singular service-class economy. Queue concurrency, workflow budgets, delivery pacing, and entity execution read the same rows. Adding BullMQ or another workflow product would create another retry, priority, dedup, scheduler, worker, and persistence vocabulary.

### TypeScript deploy plane

[TypeScript IaC](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/typescript/iac/README.md) is a real deploy library:

- one decoded `StackSpec`
- one arm-keyed provider dispatch
- Pulumi Automation API as the sole executor
- no `Pulumi.yaml`
- self-hosted Kubernetes and Docker
- AWS serverless and EKS
- GCP object and Cloud SQL resources
- Cloudflare object, Pages, DNS, and edge resources
- Doppler as canonical secrets
- Pulumi policy packs and drift evidence
- Pulumi Cloud/ESC integration
- observability store and Grafana provisioning
- CNPG, Barman Cloud plugin, MinIO or Ceph, NATS JetStream, PgBouncer, database roles, extensions, logical replication CRs, and ensure jobs
- Kubernetes service account, RBAC, Deployment, PDB, HPA, Service, CronJob, traffic, TLS, DNS, and network policy

Provider capability mapping is already explicit: an absent cell means an absent capability. That is the correct substrate for closing operational coverage.

### C# AppHost

[Rasm.AppHost](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.AppHost/README.md) is the process and service spine:

- host profiles for Rhino, Grasshopper, desktop, companion, sidecar, headless service, web service, and test host
- config, secrets, time, lifecycle, modules, features, deterministic execution, and resource budgets
- `LaneGuard` bulkhead, adaptive concurrency, load shedding, hedging, and circuit behavior
- crash-durable orchestration with activities, timers, signals, persistent jobs, compensation, retry, fencing, resume, and orphan reclaim
- one transactional outbox and one schedule port
- capability brokering, command dispatch, MCP, agents, model middleware, and tool federation
- process and WASM isolation
- outbound transport adapters and cluster coordination
- health, degradation, alerts, telemetry, profiles, benchmarks, and support bundles
- post-fetch updates with drain, canary, blue-green, linear waves, and health-gated fleet progression

This design already occupies the problem space of Hangfire, Quartz.NET, MassTransit, Wolverine, Orleans, and Temporal. Those packages would not fill a missing primitive; they would create competing histories, schedulers, transports, retries, and storage tables.

### C# Persistence

[Rasm.Persistence](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.Persistence/README.md) is broader than a repository layer:

- Marten event append and authoritative graph projections
- PostgreSQL/EF relational bindings
- embedded SQLite floor
- commit DAG, op-log, CRDT merge, time travel, provenance, retention, and recovery
- CDC ingress and egress
- authoritative, analytical, graph, vector, text, cache, columnar, and federated query lanes
- content-addressed object storage
- fenced coordination, leases, membership, watermarks, outbox, and workflow step-state operations
- PostgreSQL extension verification and capability degradation
- ClickHouse, Scylla/Cassandra, Qdrant, Delta Lake, RocksDB, and LMDB scale-out or embedded rows
- Kafka, NATS, RabbitMQ, Pulsar, MQTT, Redis Streams, S3, Azure Blob, GCS, MinIO, and KMS provider rows
- Arrow, Flight, Flight SQL, ADBC, Parquet, Substrait, Avro, Protobuf, JSON Schema, and CloudEvents interchange

Package coverage is already large enough that additional package admission should be exceptional.

### C# Compute

[Rasm.Compute](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.Compute/README.md) supplies service-bearing compute:

- bounded `WorkLane` channels with admission, backpressure, drops, drains, receipts, and depth
- `JobGraph` dependency scheduling with content-key reconciliation, speculative work, preemption, fair share, accelerator affinity, spill, and rolled-up progress
- local tensor/model/solver dispatch
- remote gRPC farm
- broker-fed sensor ingest
- compute receipts, progress streams, cost, and telemetry
- planned sharded solve partition and merge evidence

Its local lanes are not durable job queues. Durability enters through AppHost orchestration and Persistence. This separation is sound: Compute owns execution topology; AppHost/Persistence own durable intent and state.

### Python runtime

[Python runtime](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/python/runtime/README.md) is no longer a minimal worker fabric.

`WorkerKind` and `KernelTrait` cover:

| Axis | Planned cases |
| --- | --- |
| Placement | thread, subinterpreter, process, GPU, WASM, daemon, remote |
| Kernel character | inline, pure, releasing, hostile, sandboxed |
| Enforcement | cooperative and terminal |
| Process behavior | reusable crash-respawning pools and kill-capable deadline pools |
| Transport | cloudpickle/tblib, shared-memory arrays, framed SSH process sessions |
| Isolation | Wasmtime guest, spawned process, remote interpreter |
| Operations | warm, roll, drain, abort, shutdown, restart |
| Evidence | trace carrier, resource identity, cost, profile, pulse, receipt |
| Supervision | psutil probes, resource ceilings, restart windows, health projection |

`anyio`, `loky`, and `pebble` are intentionally distinct: structured concurrency, reusable crash-respawning pools, and terminal wall-clock kill are separate guarantees. APScheduler owns local cron/interval/date scheduling. Python runtime intentionally has no durable run ledger; durable reuse and application authority remain on C# Persistence or a caller’s wire-bound service.

### Python data and compute

Python data already carries a dense analytical backend:

- Arrow, ADBC, Flight SQL, DuckDB, Daft, Dask, Polars, pandas, Ibis-adjacent SQL lowering through SQLGlot, Substrait, Parquet, Delta/Iceberg-style interchange, object storage, GeoArrow, xarray, tensor stores, and lakehouse patterns
- Postgres, SQLite, Snowflake, BigQuery, Spark/Hive/Impala, and Flight driver rows through ADBC or remote query surfaces
- data contracts, profiling, cost, materialization, object egress, and content-keyed receipts

Python compute already has the numerical workloads that exercise the worker fabric. Its open work concerns new solver, mesh, experiment, and evidence arms rather than missing backend infrastructure.

## Capability matrix

Legend:

- `Owned`: decision-complete owner exists.
- `Partial`: owner exists, but a tracked task or missing operational arm prevents closure.
- `External`: library consumes a caller- or deploy-provided service by design.
- `Refused`: concern intentionally belongs elsewhere.

| Capability | C# | Python | TypeScript runtime/data | TypeScript IaC |
| --- | --- | --- | --- | --- |
| Config admission | Owned: AppHost | Owned: runtime | Owned: core/runtime/security | Owned: `StackSpec` |
| Secret custody | Owned ports; consumes settled custody | Partial provider family | Owned vocabulary and guards | Partial leases/workload identity; Doppler owned |
| PostgreSQL client | Owned: Npgsql/Marten/EF | Read/interop through ADBC; external authority | Owned: Effect SQL PG | Owned: CNPG and managed rows |
| Embedded relational | Owned: SQLite | External through data drivers | Owned SQLite family; PGLite partial | Local carrier only |
| Schema creation | Partial: verifier plus framework schema | Refused as shared authority | Partial ensure roster | Owned one-shot ensure execution |
| Schema convergence | Partial current-state verifier; replacement absent | Disposable analytical stores; wire consumer absent | Partial current-state roster; replacement absent | Missing generation orchestrator |
| Extension capability | Partial C# roster | Consumer lacks full descriptor | Partial TS roster | Executes TS roster only |
| Object store | Owned provider rows | Owned egress/roots | Owned object plane | Partial recovery and parity |
| Cache | Owned hybrid/Redis | HTTP/session-local | Owned data/runtime lanes | External service not broadly provisioned |
| Transactional outbox | Owned | Refused as durable authority | Owned | Provisions substrate |
| Durable workflow | Owned | Refused by current charter | Owned | Missing worker topology |
| Durable queue | Owned outbox/workflow substrate | Refused by current charter | Owned PG/Effect plane | Missing backlog scaling |
| Local worker execution | Owned Compute/AppHost | Owned deeply | Owned worker-thread protocol | N/A |
| Distributed compute | Partial sharding | Owned remote placement; caller authority | Owned cluster/workflow | Partial placement realization |
| Scheduling | Owned SchedulePort | Owned local APScheduler | Owned durable schedule | Owned CronJob, partial workload mapping |
| Messaging | Owned broad provider family | Transport-focused | Owned NATS/Kafka/MQTT | NATS provisioned; others external |
| Schema registry | Owned C# Confluent family | Wire consumer | Missing Kafka registry client | Not provisioned |
| Coordination | Owned fenced store | Supervisor/local runtime | NATS KV/browser/PG semantics | NATS and PG provisioned |
| Health/readiness | Owned AppHost projection | Owned supervisor and gRPC health | Owned lifecycle/telemetry concepts | K8s probes owned |
| Provider-specific health | Partial by admitted/deployed backend | Partial | Partial | No support-contract matrix |
| Telemetry | Owned four signals | Owned four signals | Owned four signals | Planned collector/stores/boards |
| Backup | Owned recovery vocabulary | Artifact receipts only | Retention/object semantics | Partial CNPG primary backup |
| Restore/PITR | Owned C# choreography | Consumer | Missing application-store choreography | Partial/missing restore realization |
| DR drills | Partial evidence concepts | External | Missing | Missing |
| Tenancy | Owned store/authority | Tenant carried on wire | Owned transaction pin and security | Partial cluster-tier parity |
| Autoscaling | Local adaptive concurrency | Local policy/remote placement | `WorkClass` policy | CPU HPA only |
| Progressive delivery | AppHost fleet update owned | External | Runtime drain/probes | Missing Kubernetes advanced rollout |
| Supply-chain policy | Owned plugin/update gate | Runtime evidence | Security/runtime validation | Digest policy owned; admission closure partial |
| Drift | Capability probe and provision verdict | Admission evidence | Fail-closed probes | Pulumi preview/refresh evidence |

## Tracked partial work

### Central cross-language register

Backend-relevant central work already tracked:

- `ESTATE_OTLP_BACKEND`: library-side OTLP and Grafana design exists; live estate placement remains externally blocked.
- `PROFILE_SIGNAL_OTLP`: all three runtimes retain vendor profile push until OTLP profiles stabilizes across SDKs.
- `OPLOG_ENTRY_SCHEMA` and `HOST_OPLOG_CRDT_PRODUCER`: causal operation wire is queued.
- shared HLC carrier key drift gates remain split across C# AppHost, Python runtime, and TypeScript consumers.

Each item is a genuine partial, but none justifies another telemetry or event framework.

### TypeScript data

Tracked data-plane work includes:

- PGLite, MySQL, and MSSQL completion in a new relational interop page
- `fact_journal` RLS registration
- object reference read and owner-namespace contracts
- cold archive tiers and typed restore
- SSE live modality
- Parquet codec
- durable satisfaction of the security `AuditJournal` port
- retry schedule composition through the one budget owner
- single-owned stream identity SQL
- fault-class conformance
- cache census instrumentation
- Flight SQL analytical ingress
- layer-topology and op-log wire consumers
- native profile-adapter completion

PGLite is therefore already recognized, but the plan remains incomplete until `@effect/sql-pglite`, capability degradation, current-schema parity, fresh-generation replacement, data export/replay, and local concurrency limits land together.

### TypeScript runtime

Tracked runtime work includes:

- offline EventLog server synchronization
- store-backed rate-limit rows shared by serving and work throttles
- workload-identity credential projection
- exact CloudEvents HTTP bindings
- Connect/gRPC interceptor and guarded mount
- benchmark sampling

Durable work itself is not a missing concern. Deployment of its workers is.

### TypeScript security

Tracked backend-relevant work includes:

- a new `authn/workload.md` page for client credentials, token exchange, DPoP, and machine principals
- cookie session guard
- API-key subject projection into entitlements
- typed Doppler coordinates
- typed `LeaseSpec`
- deploy-plane lease custody
- auth-throttle rows
- audit-journal satisfaction through TypeScript data

This work should precede broad managed-cloud worker deployment because machine identity and credential projection are the proper alternative to long-lived static credentials.

### TypeScript IaC

Tracked IaC work includes:

- a shared environment-key catalogue
- generated provider-credential roster
- lease custody cells
- Grafana builder field completion
- runtime pulse and producer descriptor ingestion
- typed UI asset identity

No existing task closes worker deployments, queue-depth autoscaling, schema-generation replacement, cluster-per-tenant parity, or recovery drills.

### C# AppHost

Tracked backend-relevant work includes:

- outbound carrier setters
- MQTT receive
- offline telemetry retry binding
- profile sample capture
- HLC header key mint
- outbox owner spelling and envelope alignment
- corpus-gate producer projections

One research block remains especially important: dead-letter replay requires Persistence-owned read-and-requeue primitives. AppHost correctly refuses to invent them.

### C# Persistence

Tracked backend-relevant work includes:

- model-qualified federated element sets
- search wire projection
- trace stamping at the egress port
- Flight SQL serving
- MQTT egress
- Arrow partition restore
- point-cloud codec admission
- solver memo persistence

Schema-generation replacement and full cross-runtime PostgreSQL capability parity are not present as first-class tracked owners.

### C# Compute

Tracked backend-relevant work includes:

- sharded distributed solve partition
- shard placement and merge evidence
- broker ingest rows
- NATS/MQTT signal admission loop
- Arrow dataset projection
- Flight push
- energy result wire
- parallel budget binding

Compute’s execution substrate is deep; its distributed partition and broker ingress arms remain partial.

### Python runtime

Tracked runtime work includes:

- worker pre-flight guest admission
- process/channel evidence
- Vault and Azure secret-provider completion
- HTTP cache, proxy, custody, and batch acquisition
- metric-domain typing
- geometry and compute instrument rows
- HLC header drift gate
- support-bundle wire completion
- service-name ownership and bundle boot gate

Python worker execution is substantially complete in design. Remaining work concerns admission, evidence, and composition closure—not a missing queue package.

### Python data and compute

Backend-relevant tracked work includes:

- ADBC Postgres, SQLite, and Snowflake rows
- Substrait plan validation
- GeoArrow and arro3 family completion
- object-carrier compression
- per-app store write guards
- network-flow data page
- provider-deep impact plane
- query profile adapters
- compute resource/evidence composition

Analytical and workload capabilities remain distinct from transactional authority. They do not establish Python as another transactional system-of-record or durable-work authority.

## Untracked architectural gaps

### 1. PostgreSQL capability authority is split

#### Evidence

[C# Persistence provisioning](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.Persistence/.planning/Store/provisioning.md) declares `ServerExtension` authoritative. [TypeScript PostgreSQL](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/typescript/data/.planning/lane/postgres.md) separately declares `_rows` authoritative. Their shared set contains ten SQL extensions:

| Capability key | SQL extension |
| --- | --- |
| TimescaleDB | `timescaledb` |
| pg_cron | `pg_cron` |
| pg_duckdb | `pg_duckdb` |
| pg_graphql | `pg_graphql` |
| pg_jsonschema | `pg_jsonschema` |
| pg_partman | `pg_partman` |
| pgaudit | `pgaudit` |
| pgvector | `vector` |
| PostGIS | `postgis` |
| H3 | `h3` |

TypeScript omits ten C# rows:

| C# capability | SQL extension | Capability lost from TypeScript data/IaC |
| --- | --- | --- |
| TimescaleDB Toolkit | `timescaledb_toolkit` | hyperfunctions, time-weighted aggregates, and Timescale analytical helpers |
| Apache AGE | `age` | optional in-PostgreSQL openCypher and `agtype` |
| PostGIS Raster | `postgis_raster` | raster storage and raster/geometry operations |
| PostGIS SFCGAL | `postgis_sfcgal` | exact and advanced 3D geometry operations |
| pgvectorscale | `vectorscale` | DiskANN vector index strategy |
| ParadeDB search | `pg_search` | Tantivy/BM25 search lane |
| H3/PostGIS bridge | `h3_postgis` | direct H3 cell and PostGIS geometry interoperation |
| pgRouting | `pgrouting` | graph routing over PostGIS topology |
| pg_squeeze | `pg_squeeze` | online bloat reclamation |
| pg_net | `pg_net` | database-originated asynchronous HTTP egress |

C# omits eight TypeScript rows:

| TypeScript capability | SQL extension | Capability lost from C# verification and descriptor projection |
| --- | --- | --- |
| VectorChord | `vchord` | alternative vector index engine over the pgvector contract |
| VectorChord BM25 | `vchord_bm25` | BM25 search paired with the VectorChord image |
| incremental view maintenance | `pg_ivm` | incrementally maintained views |
| incremental batch maintenance | `pg_incremental` | checkpointed incremental processing |
| Parquet | `pg_parquet` | PostgreSQL Parquet import/export surface |
| statement statistics | `pg_stat_statements` | cumulative query statistics and reset-aware deltas |
| trigram search | `pg_trgm` | similarity, fuzzy matching, and trigram indexes |
| phonetic matching | `fuzzystrmatch` | Soundex, Metaphone, and edit-distance helpers |

TypeScript IaC derives the PostgreSQL image, preloads, Database CR extension list, and runtime grants from the TypeScript matrix. A server can therefore satisfy the deploy plane while failing C# Persistence’s authoritative verifier, or satisfy C# while exposing capabilities TypeScript never describes.

Row presence is only the first gap. Current metadata also disagrees:

| Concern | Current divergence | Required ruling |
| --- | --- | --- |
| Identity | project names and SQL names are conflated (`pgvector`/`vector`, `apache-age`/`age`, `pgvectorscale`/`vectorscale`, `h3-pg`/`h3`) | carry separate canonical capability, artifact, preload-library, and SQL-extension identities |
| Preload | C# marks `pg_duckdb`, `pg_search`, `pg_partman_bgw`, `pg_squeeze`, `pgaudit`, `pg_net`, and Timescale-related rows; TypeScript only derives explicit flags for a smaller set and special-cases pgaudit through CNPG | one pinned-artifact fact determines preload, carrier, restart class, and CNPG-managed behavior |
| Dependencies | C# models several single base-type gates; TypeScript models only flag-to-grant demands | carry a dependency set and activation order, including multi-parent bridges such as `h3_postgis` |
| Scope | image installation, cluster preload, database extension creation, and per-session loading are compressed into one row | distinguish image, cluster, database, schema, and session realization |
| Compatibility | PostgreSQL-major, extension-version, license, and mutual-exclusion facts do not share one authority | gate every selected profile before image construction |
| Failure | C# has required/degradable/observational rank; TypeScript has grant presence and image flags | share one failure and degradation vocabulary |
| Verification | probes and floor parsing are branch-local | share probe identity, expected result, privilege, floor parser, and verification epoch |

#### Required contract

One canonical `PostgresCapabilityDescriptor` must contain the full 28-row union. `portable`, `csharp`, and `typescript` rosters are deleted concepts because they permit ignorance to become branch-local policy. Full parity means:

1. C#, Python, TypeScript data, and TypeScript IaC know every row and every field.
2. TypeScript IaC can build or select an image carrier for every row admitted by an estate profile.
3. C# verifies the exact selected profile rather than a C#-private roster.
4. TypeScript gates queries and maintenance against the same selected profile.
5. Python decodes the full descriptor for ADBC/Flight/external PostgreSQL admission and reports unavailable grants without becoming another provisioning authority.
6. Database profiles select activation subsets from the complete union. An inactive row remains understood, not deleted.
7. Incompatible or license-distinct combinations become separate fully described image profiles. They never become separate language rosters.

Every row carries:

- canonical capability key
- project and image artifact identity
- SQL extension identity
- minimum accepted version and comparison law
- PostgreSQL-major and image compatibility
- license and distribution posture
- preload library, restart class, and CNPG-managed-preload fact
- dependency set and activation order
- incompatibility set
- image, cluster, database, schema, or session scope
- create/drop intent as deployment data
- capability and semantic probes
- failure rank and degraded behavior
- runtime consumers

C# remains the wire-vocabulary owner under central architecture. One generated fixture projects the same descriptor into C#, Python, and TypeScript. Local tables may add executable delegates, but no local table may add, remove, rename, or weaken a capability row. IaC image capability is the union projected through the selected profile; database activation is the profile subset. Full parity therefore adds every missing row without forcing all 28 extensions into every database.

#### Exact planning changes

- Extend [libs/.planning/ARCHITECTURE.md](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/.planning/ARCHITECTURE.md) with one full-superset PostgreSQL capability contract and the catalog-parity versus activation distinction.
- Extend [C# Store/provisioning.md](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.Persistence/.planning/Store/provisioning.md) with all eight TypeScript-only rows, multi-dependency gates, generated descriptor projection, and selected-profile verification.
- Extend [TypeScript lane/postgres.md](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/typescript/data/.planning/lane/postgres.md) with all ten C#-only rows and the complete descriptor fields.
- Extend [TypeScript kube/data.md](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/typescript/iac/.planning/kube/data.md) so image, preload, Database CR, role, and restart construction consume the descriptor instead of treating every row as `{name, version}` plus local exceptions.
- Extend [Python tabular/query.md](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/python/data/.planning/tabular/query.md) with full-descriptor decoding, external-server capability admission, and typed degradation; Python never installs or activates the extensions.
- Add one generated cross-language fixture under the existing wire-contract proof owner; do not create a shared implementation package.

No new package is required.

### 2. Database change must become schema convergence, not migration history

#### Can a modern backend have no migrations?

Yes, if “no migrations” means no chronological migration files, no applied-migration ledger, no framework-owned migration authority, no application-startup mutation, and no chain of compatibility shims.

No, if it means durable data can change physical representation without any physical operation. A changed column shape, constraint, index, extension ABI, event representation, or engine version must be reconciled, transformed, replayed, copied, or discarded. Calling generated `ALTER` statements “declarative” does not remove that fact.

Rasm can enforce a stricter and cleaner alternative: intentional schema changes never mutate the active generation. Every desired-state digest change creates a replacement generation from the current declaration, rebuilds its data from canonical truth, verifies it, and atomically cuts traffic over. In-place repair remains legal only when the desired digest did not change and the observed database drifted from it.

#### Selected model

One `SchemaContract` wire value carries:

- authority and engine profile
- normalized current desired state
- desired-state content digest
- PostgreSQL capability-profile digest
- database and cluster generation identity
- canonical data source
- rebuild strategy
- validation probes
- cutover strategy
- rollback retention
- runtime compatibility generation
- evidence and recovery policy

Each durable store declares one rebuild strategy:

| Strategy | Use |
| --- | --- |
| `Disposable` | caches, local analytical stores, test stores, and derived materializations recreate from nothing |
| `Replay` | Marten projections, TypeScript journal projections, outbox/read models, and content-keyed derived state rebuild from immutable canonical facts |
| `CopyProject` | authoritative row state projects through one current typed mapping into a freshly created target |
| `ReplicateCutover` | large PostgreSQL stores seed a replacement, stream changes through logical replication or CDC, drain briefly, catch up, and switch |
| `RestoreVerify` | disaster recovery restores an unchanged schema generation; it does not change representation |

Intentional change follows one system-enforced transition:

```text
current desired-state manifest
  -> content digest
    -> digest unchanged
       -> inspect active generation
       -> repair only missing or drifted declared objects in place
       -> verify exact digest and probes
    -> digest changed
       -> choose database-generation or cluster-generation replacement
       -> create empty target from the complete current manifest
       -> activate the selected PostgreSQL capability profile
       -> replay, copy-project, or replicate canonical data
       -> verify constraints, RLS, counts, content hashes, projections, and semantic probes
       -> drain writes and settle the final delta
       -> atomically switch the connection/schema pointer
       -> retain the old generation for the bounded rollback window
       -> garbage-collect the old generation
```

No ordered change set survives. Evidence records the requested digest, source generation, target generation, plan digest, observations, cutover, and result, but future state derives only from the current desired declaration. This evidence ledger is an audit receipt, not schema history or an execution authority.

#### Engine realization

| Engine or framework | No-migration realization |
| --- | --- |
| PostgreSQL on CNPG | create a new database generation when cluster image, preload set, engine major, locale, or extension ABI remains unchanged; create a new CNPG cluster generation when any cluster-scoped fact changes; seed and catch up through existing backup, publication, subscription, and direct-connection primitives |
| Marten | treat events as canonical facts and projections as replaceable generations; rebuild projections through Marten’s projection daemon; replace the event-store database when Marten’s own physical schema or active event representation changes |
| EF Core | use the model only as a current desired-state contributor; ban `Database.Migrate`; do not mistake `EnsureCreated` for existing-database convergence; create the replacement schema from the current model projection |
| Raw PostgreSQL DDL | store one current idempotent desired-state declaration; deploy it only into an empty generation or use it to repair unchanged-digest drift |
| TypeScript journal | retain append-only facts as canonical truth and rebuild relational projections into the replacement generation |
| PGLite | create a new data directory/IndexedDB generation, apply the current schema, copy-project or replay data, close the old worker/instance, and atomically switch the active generation pointer |
| SQLite | create a new database file, apply the current schema, copy-project or replay rows, verify, close handles, and atomically replace the active file pointer |
| Python analytical stores | remain disposable or externally authoritative; rebuild local DuckDB/SQLite materializations and validate the received `SchemaContract` generation |

PGlite’s `pgDump` can snapshot and restore an unchanged schema generation or assist an engine-version rebuild. A full SQL dump recreates the old schema and therefore cannot be the schema-change mechanism. Structural replacement uses data-only export or typed table projection into a target already created from the current manifest.

#### Live-system consequences

- Runtime processes remain verify-only. No C#, Python, or TypeScript request path mutates schema.
- Workers stamp claims, payloads, and receipts with the admitted schema generation and refuse a mismatched generation.
- Atomic service deployment removes the need for an expand/contract window. A deployment that intentionally mixes old and new binaries cannot avoid a compatibility interval; Rasm should reject mixed-generation rollout instead of preserving compatibility machinery.
- Replacement becomes a recovery concern. Backup/restore, logical replication, projection replay, content hashing, and cutover evidence move from optional operations to the schema-change spine.
- Every sole source of truth needs a current projection into a new generation. Data that is neither disposable, replayable, nor projectable cannot be preserved under a no-migration law.
- Extension-profile changes select database or cluster replacement according to scope. An extension image, preload, engine-major, or ABI change never mutates the active cluster.
- Drift repair and intentional change remain distinct. Drift restores the same digest in place; a new digest replaces the generation.

#### Tool and package decision

Atlas, `pg-schema-diff`, and `psqldef` all support current-state schema comparison. Atlas can inspect, diff, plan, and declaratively apply a desired schema; its Kubernetes operator accepts an `AtlasSchema` desired-state resource and destructive-change policy. Stripe’s `pg-schema-diff` generates online-aware DDL, including concurrent indexes and deferred constraint validation. These tools remove handwritten chronological files, but they still generate and execute live schema-change plans. They are therefore not the authority under Rasm’s strict replacement law.

No C#, Python, or TypeScript migration library should be added. Specifically:

- do not add FluentMigrator, DbUp, EF migrations, Alembic, Flyway, Liquibase, or a TypeScript migration runner
- do not use Effect SQL migration support as the schema authority
- do not adopt the Atlas operator as an active schema mutator
- retain Atlas or `pg-schema-diff` only as an optional read-only design/proof tool if the corpus later needs an independent diff oracle
- add `@effect/sql-pglite` for the Effect client integration
- add `@electric-sql/pglite-tools` for explicit PGLite dump/restore evidence; use data-only or typed projection for schema replacement
- reuse CNPG `Database`, `Publication`, `Subscription`, backup/restore, immutable cluster construction, existing Pulumi/Kubernetes support, Npgsql catalogs, Marten projection rebuilds, PostgreSQL `COPY`, and SQLite/PGLite file-generation primitives

#### New files

- `libs/typescript/data/.planning/lane/schema.md`
  - owns current desired-state values, normalization, digest, engine degradation, canonical source, rebuild strategy, and runtime generation admission
- `libs/typescript/iac/.planning/operate/converge.md`
  - owns drift repair, replacement-generation construction, tenant targeting, replay/copy/replication execution, validation, cutover, rollback retention, and evidence
- `libs/csharp/Rasm.Persistence/.planning/Store/schema.md`
  - owns C# desired-state projection across Marten, EF, raw PostgreSQL, extensions, embedded SQLite, canonical-source classification, rebuild, and generation verification

Existing README and architecture routers must admit each page. Existing [TypeScript kube/data.md](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/typescript/iac/.planning/kube/data.md) replaces the epoch-keyed monolithic ensure Job with an empty-generation initializer plus unchanged-digest drift repair. Existing [C# Store/provisioning.md](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.Persistence/.planning/Store/provisioning.md) replaces “first-opener migration” and queued extension admission with verify-only generation admission. Existing [Python tabular/query.md](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/python/data/.planning/tabular/query.md) consumes generation and capability descriptors without acquiring durable schema authority.

### 3. CNPG cluster-per-tenant does not reproduce the primary data plane

[kube/data.md](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/typescript/iac/.planning/kube/data.md) gives the primary cluster:

- shared preload libraries
- high-availability replication slots
- superuser access
- Barman Cloud plugin and archive
- backup retention
- ScheduledBackup
- PgBouncer Pooler
- managed application and analyst roles
- full extension database

Its `cluster-per-tenant` arm creates a dedicated `Cluster` and `Database`, but omits:

- shared preload libraries
- replication slots
- Barman plugin
- backup retention
- ScheduledBackup
- Pooler
- explicit superuser access
- independent archive server name/destination posture
- connection-limit parity

This is not merely a tuning difference. A tenant moved to the highest isolation tier loses backup, pool, preload, and failover semantics while the prose claims a dedicated WAL and failure domain.

#### Required repair

Factor one internal cluster-spec projection and one cluster adjunct constructor. Both the primary and every tenant cluster consume:

- profile sizing
- image and extension/preload projection
- managed roles
- replication slots
- backup plugin and retention
- unique archive identity
- ScheduledBackup
- Pooler
- direct and pooled endpoints
- protection and policy dependencies

Tenant differences remain row data: name, database, archive server, credentials, sizing override, region, and recovery objective.

No new file or package is required. Existing [kube/data.md](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/typescript/iac/.planning/kube/data.md) is the correct owner.

### 4. Recovery is not a deploy-plane domain

C# Persistence already defines recovery objectives and verified PITR choreography. TypeScript IaC creates CNPG backups. No deploy owner closes the path from an objective to a proven restored service.

CloudNativePG recovery bootstraps a new cluster from a backup, object store, or volume snapshot. PITR requires WAL. Recovery does not repair an existing cluster in place. A restored cluster also needs new archive identity, secrets, roles, extensions, pooler, traffic, and post-recovery verification. Backup configuration alone is therefore insufficient.

Object storage is also durable truth for content-addressed artifacts. MinIO/Ceph creation and conditional put do not establish:

- versioning or object-lock posture
- cross-site replication
- bucket inventory and integrity evidence
- credential and encryption-key recovery
- deletion-marker handling
- restore to a clean bucket
- lifecycle policy validation
- provider parity across S3, GCS, R2, MinIO, and Ceph

#### New folder

`libs/typescript/iac/.planning/recovery/` is justified as a new sub-domain because recovery spans Kubernetes, Docker, managed clouds, PostgreSQL, object storage, secrets, and evidence. It contains at least three files:

- `database.md`
  - CNPG backup source, recovery bootstrap, PITR target, replica promotion, new archive identity, role and secret restoration, Pooler recreation, extension verification, managed Cloud SQL recovery, and Docker restore carriers
- `object.md`
  - provider capability rows for versioning, object lock, replication, inventory, restore, lifecycle, encryption, and content-key verification
- `drill.md`
  - `RecoveryObjective` input, scheduled restore exercise, isolated target creation, integrity probes, measured RPO/RTO, evidence receipt, cleanup ownership, and failure escalation

Add the folder to [TypeScript IaC architecture](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/typescript/iac/ARCHITECTURE.md) and [README](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/typescript/iac/README.md). `program/provider.md` composes the selected recovery rows; it does not absorb their mechanics.

No npm package is required. CNPG, Barman, cloud providers, Kubernetes, and Pulumi already supply the resources. Recovery adds typed CRDs, charts, and provider resources.

### 5. Worker deployment does not express the runtime work model

Runtime owns `WorkClass`, durable queue families, entity workers, workflow workers, outbox drains, and render workers. IaC owns one generic Deployment, CPU HPA, Service, and CronJob.

Missing deployment semantics:

- service versus worker process role
- one or multiple `WorkClass` subscriptions per deployment
- worker command and handler roster
- queue/backlog metric
- scale-to-zero eligibility
- minimum warm workers
- maximum concurrency per pod
- CPU, memory, GPU, and ephemeral-storage placement
- affinity and topology by worker class
- drain behavior for claimed work
- lease visibility versus pod termination grace
- disruption budget by worker semantics
- backlog-driven scaling
- poison-rate and oldest-age scale guards
- worker rollout compatibility
- scheduled one-shot versus standing worker selection
- GPU node pool and runtime class
- remote compute endpoint registration

CPU utilization is a weak proxy for queue pressure. An idle worker can have a large backlog while CPU remains low; an expensive active job can saturate CPU with no backlog. Autoscaling must read work evidence.

#### New file

`libs/typescript/iac/.planning/kube/worker.md` should own:

- `WorkerSpec` rows projected from runtime worker descriptors
- `WorkClass` subscription, command, concurrency, resources, placement, and drain
- Deployment versus Job/ScaledJob selection
- Service omission for non-serving workers
- queue metric and scaler construction
- scale-to-zero and warm-floor policy
- PDB and rollout constraints
- worker-specific health and readiness
- output coordinates and telemetry resource attributes

`kube/workload.md` remains the general service identity and pod substrate. `worker.md` composes it or shares an internal pod-template projection without widening the public surface.

#### KEDA

KEDA is appropriate as an optional deploy dependency, not a TypeScript runtime package:

- `ScaledObject` targets standing worker Deployments.
- `ScaledJob` targets one-job-per-event workloads.
- PostgreSQL and NATS JetStream scalers exist.
- Prometheus can scale from the runtime’s own queue-depth, oldest-age, and in-flight metrics.

Prometheus is the preferred Rasm default because it preserves runtime abstraction and uses the existing observability plane. A PostgreSQL scaler is a lower-level fallback for the journal queue. NATS JetStream scaling applies only to NATS-backed work; Rasm’s durable queue truth is PostgreSQL, so the NATS scaler must not silently become the queue authority.

KEDA arrives as a pinned Helm chart plus generated typed CRDs. No npm package is necessary beyond `@pulumi/kubernetes`.

### 6. Kubernetes rollout behavior is less complete than AppHost fleet rollout

AppHost plans canary, blue-green, linear-wave, health gates, drain, restart, and rollback evidence for its fleet update path. Kubernetes IaC currently emits a normal Deployment.

Service deployments need:

- rolling, blue-green, and canary policy rows
- schema compatibility gate before mixed-version rollout
- pre-promotion and post-promotion analysis
- automated rollback
- deployment annotations on Grafana timelines
- rollout evidence folded into the existing receipt vocabulary

Workers need a different policy:

- blue-green or drain-then-replace is safe by default
- canary worker pods can consume real queue messages and cannot be traffic-weighted by an HTTP router
- old and new workers must not concurrently process an incompatible payload or schema generation
- claimed-work lease and termination grace must align

#### New file

`libs/typescript/iac/.planning/kube/rollout.md` should own the deployment-strategy rows and compose the existing runtime drain, probe, telemetry, and schema-compatibility values.

Argo Rollouts is a justified optional chart/CRD dependency for HTTP services that require blue-green, canary, metric analysis, and automatic rollback. Queue workers should default to blue-green or native rolling replacement with explicit drain. Argo’s own documentation distinguishes queue-worker limitations for canary traffic control.

No npm package is required; use the controller chart and `crd2pulumi`-generated types.

### 7. Backend admission lacks an operational conformance contract

Rasm currently uses “supported” in several distinct senses:

- client package admitted
- planning page exists
- runtime adapter exists
- IaC can provision it
- health can probe it
- telemetry can observe it
- backup and restore exist
- local development can run it
- managed-cloud arm exists
- wire semantics align across runtimes

One explicit matrix must distinguish these meanings.

#### Required descriptor

Every backend row should declare:

| Field | Meaning |
| --- | --- |
| `role` | system of record, analytical replica, cache, fanout, object, coordination, external interop |
| `authority` | owner of durable truth and mutation |
| `client` | runtime package and adapter |
| `provision` | self-host, managed, external-only, or embedded |
| `schema` | current desired-state owner, convergence policy, canonical data source, generation replacement, and cutover |
| `health` | liveness, readiness, and semantic probe |
| `observe` | spans, metrics, logs, profiles, engine statistics |
| `backup` | physical/logical/object mechanism |
| `restore` | verified restore carrier |
| `tenancy` | shared, database, cluster, namespace, account |
| `scale` | vertical, replicas, shards, partitions, serverless |
| `local` | local-dev carrier and capability degradation |
| `security` | identity, TLS, encryption, secret rotation |
| `wire` | cross-runtime envelope and schema compatibility |
| `status` | complete, external, partial, experimental |

A backend may be deliberately `external` for provisioning. That remains a complete contract when health, credentials, topology, and responsibility are explicit. A client package alone never yields `complete`.

#### Owner

Add this contract to [libs/.planning/ARCHITECTURE.md](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/.planning/ARCHITECTURE.md). Each package maintains its own backend rows under that schema. No new central package or direct dependency arises.

### 8. Managed-cloud arms are intentionally but insufficiently asymmetric

Current IaC realization:

- self-hosted Kubernetes: object, NATS, PostgreSQL, observability, tenants, workload, traffic
- self-hosted Docker: object, NATS, PostgreSQL, app, observability, edge
- AWS cluster: whole Kubernetes estate
- AWS serverless: VPC, ECR, Fargate/ALB, S3
- GCP: GCS, Cloud SQL, optional static assets
- Cloudflare: R2, Pages, DNS

Provider equivalence correctly treats absent constructions as absent capabilities. Package README wording—“fully realized multi-cloud deployment”—is broader than the actual provider arms.

Required improvement:

- define deploy profiles by required capability set, not provider name alone
- fail admission when an application asks for durable work, fanout, observability, recovery, or tenancy on an arm lacking that cell
- let `external` coordinates satisfy a missing managed cell explicitly
- expose a typed capability result in `StackOutputs`
- add recovery, schema convergence, generation replacement, health, and worker columns to the provider equivalence map

Do not add every cloud SDK or managed broker merely to fill a table. Add a provider resource only when a downstream profile requires it.

### 9. Kafka schema governance is asymmetric

C# Persistence admits:

- Confluent Schema Registry client
- Avro, Protobuf, and JSON serdes
- compatibility and subject governance

TypeScript runtime admits `@confluentinc/kafka-javascript` but no Schema Registry client. Cross-runtime Kafka use can therefore preserve transport and CloudEvents while diverging on registry-governed payload evolution.

#### Package candidate

Add `@confluentinc/schemaregistry` only when a TypeScript Kafka row promises registry-governed Avro, Protobuf, or JSON Schema. Confluent documents it as the JavaScript Schema Registry client used alongside its Kafka client.

Required planning owner:

- extend `libs/typescript/runtime/.planning/net/pubsub.md` with a schema-mode row:
  - opaque C#-minted Protobuf/CloudEvents payload
  - registry-governed payload
- define subject strategy, compatibility mode, schema identity, registration authority, cache, outage behavior, and receipt evidence
- keep schema registration out of the hot producer path unless explicitly authorized

If Kafka remains a transport for opaque C#-owned wire payloads, do not add the package.

### 10. Health follows deployed defaults but not the full backend catalogue

AppHost health currently has explicit rows for PostgreSQL, the L2 cache, NATS, Kafka, Redis, upstream HTTP, disk, and allocations. Persistence admits additional backends including RabbitMQ, Pulsar, MQTT, ClickHouse, Scylla/Cassandra, Qdrant, and several object stores.

Every admitted backend does not need an AppHost package dependency. It does need a health contract when used:

- transport liveness is insufficient for a durable store
- probe traffic must reuse the production client or pool
- readiness must test the minimum semantic operation the application requires
- health failure maps to an existing degradation class
- probe cadence and cost remain bounded
- probe failure emits evidence but does not create another state owner

Preferred landing:

- extend Persistence `Store/observability.md` with backend-native semantic probe receipts
- project those receipts through AppHost’s existing `HealthContributorRow.Driver`
- add a health-check package only when the provider SDK cannot express a bounded semantic probe cleanly

This closes coverage without importing one Xabaril package per backend.

### 11. Object storage is a durable authority without a complete custody plane

Content-addressed object storage is not merely a cache. Geometry, reports, snapshots, datasets, and recovery artifacts depend on it. Current design strongly covers conditional create, content identity, multipart and provider access, but needs:

- provider-side versioning and object-lock capability
- bucket policy and public-access denial
- server-side encryption and KMS/key recovery
- replication target and lag
- inventory and orphan reconciliation
- multipart-abort lifecycle
- legal hold and retention-class mapping
- restore and content-key verification
- destructive lifecycle dry-run evidence
- tenant boundary and per-tenant quota
- backup destination separation from primary object truth

Proposed `iac/recovery/object.md` is the deploy owner. TypeScript data and C# Persistence retain object semantics and verification; IaC realizes provider controls.

### 12. PGLite needs a declared degradation profile

PGLite preserves PostgreSQL SQL and enables a unified local client family. It does not make local and production topology identical.

PGLite profile rows must state:

- single-process or owner-process access
- worker concurrency ceiling
- supported extensions
- unsupported preload/background-worker extensions
- advisory and session-lock behavior
- `LISTEN/NOTIFY` behavior
- admitted schema generation
- backup/export and import
- browser, Node, and Bun persistence carriers
- current-schema parity gate against the production desired-state manifest
- size and memory ceilings
- absence of CNPG, PgBouncer, replication, PITR, and cluster tenancy

This belongs in the planned TypeScript relational interop page and the new schema page. It should reuse the same application operations while failing or degrading unavailable capabilities at construction. A persisted PGLite generation is never altered to reach a new desired-state digest: a new IndexedDB or filesystem generation is created, populated, verified, and selected.

### 13. Provisioning and verification language conflicts in C# Persistence

`Store/provisioning.md` states that Rasm processes never mutate PostgreSQL and that provisioning is verification-only. Its `ServerExtension` rows also carry `CreateSql`, and surrounding text can be read as queuing `CREATE EXTENSION` through an application-controlled transaction.

Resolve the distinction:

- application runtime: verify only
- desired-state artifact: carries declarative extension intent
- operator: installs image artifacts and preloads
- empty-generation initializer: creates database-level extensions after cluster capability proof
- unchanged-digest drift reconciler: restores a declared extension only when observed state lost it

`CreateSql` remains deployment data in `SchemaContract`, but the runtime verifier never executes it. New `Store/schema.md` owns generation and verification semantics. TypeScript IaC’s CNPG `Database` CR remains the creation carrier; intentional capability-profile changes replace the database or cluster generation according to descriptor scope.

### 14. Barman plugin TLS dependency is not explicit

Deploy plane installs the CNPG operator and Barman Cloud plugin as charts. Current CloudNativePG guidance recommends cert-manager for secure operator-plugin communication, while allowing supplied certificate bundles.

Planning should parameterize:

- cert-manager-provided certificate mode
- externally supplied bundle mode
- chart dependencies and readiness
- certificate rotation
- failure behavior when plugin TLS is not ready

This is an extension to `kube/data.md` or the proposed `recovery/database.md`, not a new npm package. cert-manager is a pinned chart and typed CRD dependency when selected.

## Package recommendations

### Add

| Language/plane | Package or dependency | Why | Exact owner |
| --- | --- | --- | --- |
| TypeScript data | `@effect/sql-pglite` | Existing PGLite completion task needs the Effect SQL adapter rather than direct PGLite calls | `data/.planning/lane/interop.md` |
| TypeScript data | `@electric-sql/pglite-tools` | Explicit PGLite dump/restore evidence and data-only export for fresh-generation reconstruction | `data/.planning/lane/schema.md` |
| TypeScript runtime | `@confluentinc/schemaregistry` | Only when TypeScript Kafka promises registry-governed Avro/Protobuf/JSON parity with C# | `runtime/.planning/net/pubsub.md` |
| IaC external dependency | KEDA chart and typed CRDs | Backlog-driven worker autoscaling and scale-to-zero | `iac/.planning/kube/worker.md` |
| IaC external dependency | Argo Rollouts chart and typed CRDs | Optional HTTP-service blue-green/canary analysis and rollback | `iac/.planning/kube/rollout.md` |
| IaC external dependency | cert-manager chart and typed CRDs, or supplied plugin bundle | Explicit Barman/operator TLS mode | `iac/.planning/recovery/database.md` |

KEDA, Argo Rollouts, and cert-manager are infrastructure components. They do not belong in `pnpm-workspace.yaml` unless a real JavaScript SDK is imported. Pulumi uses existing Kubernetes chart and generated-CRD support.

### Evaluate only after the owner requires it

| Candidate | Gate |
| --- | --- |
| provider-specific C# health-check packages | Existing provider client cannot implement a bounded semantic probe through `HealthContributorRow.Driver` |
| Atlas or `pg-schema-diff` as a read-only independent oracle | Catalog normalization or desired/observed diff evidence cannot be made complete through existing PostgreSQL catalog and provision probes |
| managed Kafka/Pulsar/RabbitMQ/Redis provisioning resources | A declared deploy profile requires that service instead of an explicit external coordinate |
| Temporal SDK in any language | A standalone library is explicitly required to own a separate durable workflow authority |
| Ray or Dask Distributed for Python execution | A workload needs their distributed data/computation semantics, not merely remote worker placement |

### Do not add

| Candidate | Rejection |
| --- | --- |
| BullMQ | Adds Redis and a second TypeScript job authority beside PostgreSQL journal plus Effect durable work |
| Inngest / Trigger.dev / Temporal TypeScript | Duplicates TypeScript workflow history, retry, scheduling, and worker ownership |
| Hangfire / Quartz.NET | Duplicates AppHost persistent jobs, schedule port, retries, and step state |
| MassTransit / Wolverine | Duplicates C# outbox, broker adapters, handler dispatch, saga, and retry ownership |
| Orleans | Duplicates entity, grain, persistence, placement, and cluster vocabulary |
| Celery / Dramatiq / RQ / Arq | Turns Python execution into a second broker-backed durable work authority |
| Redis solely for queues | Redis remains a cache or explicitly selected transport; PostgreSQL remains durable queue truth |
| another C# event store | Marten is the append substrate and version engine source |
| another TypeScript ORM | Effect SQL and explicit lane semantics already own the relational boundary |
| another Python dataframe/query framework | Existing Arrow/ADBC/DuckDB/Daft/Dask/Polars stack already covers the backend role |
| FluentMigrator / DbUp / EF migrations / Alembic / Flyway / Liquibase | Chronological schema history conflicts with current-state generation replacement |
| Atlas Operator / `atlas schema apply` / `pg-schema-diff apply` / `psqldef` | These are live DDL reconcilers; Rasm intentionally replaces active generations on desired-state change |

## Proposed file topology

### New files in existing folders

| Path | Purpose | Depends on | Feeds |
| --- | --- | --- | --- |
| `libs/typescript/data/.planning/lane/schema.md` | Current desired state, digest, canonical source, rebuild strategy, engine degradation, and generation admission | PostgreSQL/SQLite/PGLite lanes | IaC convergence owner, runtime capability probe |
| `libs/typescript/iac/.planning/operate/converge.md` | Drift repair, generation replacement, rebuild/replication, validation, cutover, rollback retention, and evidence | data schema contract, CNPG direct endpoint, recovery | workload rollout, app readiness |
| `libs/typescript/iac/.planning/kube/worker.md` | Worker deployment, placement, drain, and KEDA scale | runtime `WorkClass`, queue metrics, workload pod substrate | durable work execution |
| `libs/typescript/iac/.planning/kube/rollout.md` | Native and Argo rollout strategies | workload/worker, traffic, observe, schema-generation compatibility | service and worker releases |
| `libs/csharp/Rasm.Persistence/.planning/Store/schema.md` | Current Marten/EF/raw SQL/extension/SQLite schema contract, canonical-source policy, rebuild, and generation admission | provisioning, store profiles, version recovery | AppHost boot and deploy artifacts |

Each belongs to an established folder with multiple peers; no new folder is needed.

### New folder justified by three independent files

```text
libs/typescript/iac/.planning/recovery/
├── database.md
├── drill.md
└── object.md
```

`recovery/` crosses provider and carrier boundaries and therefore does not fit inside `kube/`. Three owners justify the folder:

- database recovery
- object recovery
- recovery exercise and evidence

### Existing files to extend

| Path | Extension |
| --- | --- |
| `libs/.planning/ARCHITECTURE.md` | Backend operational conformance, full PostgreSQL capability parity, and schema-generation contracts |
| `libs/typescript/data/.planning/lane/postgres.md` | Full 28-row extension descriptor and profile-selected activation |
| `libs/typescript/iac/.planning/kube/data.md` | Tenant-cluster parity, full extension realization, empty-generation initialization, plugin TLS mode |
| `libs/typescript/iac/.planning/kube/workload.md` | Shared pod-template projection and worker-safe lifecycle inputs |
| `libs/typescript/iac/.planning/program/provider.md` | Recovery, convergence/replacement, worker, rollout, and external-service capability cells |
| `libs/typescript/iac/.planning/operate/policy.md` | Backup/restore, generation, worker, rollout, and convergence invariants |
| `libs/typescript/runtime/.planning/net/pubsub.md` | Optional Schema Registry payload mode |
| `libs/csharp/Rasm.Persistence/.planning/Store/provisioning.md` | Full 28-row verifier, verify-only runtime boundary, and descriptor projection |
| `libs/csharp/Rasm.Persistence/.planning/Store/observability.md` | Provider-native semantic health receipts |
| `libs/csharp/Rasm.AppHost/.planning/Observability/health.md` | Projection of new semantic probes through the existing driver row |
| `libs/python/runtime/.planning/execution/workers.md` | Complete existing admission/channel evidence tasks only |
| `libs/python/data/.planning/tabular/query.md` | Full PostgreSQL descriptor decode, external-server admission, typed degradation, and schema-generation stamp |

### Files not justified

- No Python `queue/` or `workflow/` folder. Durable authority remains external by charter.
- No C# `Messaging/` folder. AppHost `Wire` and Persistence `Version` already own transport and CDC.
- No TypeScript `bullmq.md`. Queue semantics already live in `runtime/work/queue.md`.
- No central shared implementation package for PostgreSQL capability. Cross-language alignment remains descriptor/wire conformance.
- No new folder with fewer than three pages.

## Dependency and information map

### Database convergence

```text
current desired-state manifest + PostgreSQL capability profile
  -> content digest
    -> unchanged digest + observed drift
       -> in-place drift repair
       -> exact verification
    -> changed digest
       -> new database or cluster generation
       -> current schema initialization
       -> replay / copy-project / logical catch-up
       -> semantic and content verification
       -> atomic connection-pointer cutover
       -> bounded rollback retention
       -> old-generation collection

C# Persistence / TypeScript data schema contract
  -> TypeScript IaC convergence owner or embedded-store replacement
    -> SchemaGeneration receipt
      -> C#, Python, and TypeScript runtime admission
      -> service and worker generation gate
```

No runtime mutates the cluster opportunistically at request time. No chronological change chain participates.

### Durable work

```text
TypeScript journal / Effect workflow state
  -> runtime WorkClass and worker descriptors
    -> IaC WorkerSpec
      -> Deployment or ScaledJob
        -> KEDA metric from Prometheus or PostgreSQL
          -> pod count
            -> drain + claim lease + settlement

C# AppHost workflow / outbox state
  -> Persistence fenced rows
    -> SchedulePort / OutboundHop
      -> Compute WorkLane or external transport
        -> receipt
          -> workflow commit

Python caller intent
  -> LanePolicy
    -> Kernel + WorkerKind
      -> local, device, WASM, daemon, or remote execution
        -> RuntimeRail + receipt
          -> caller-owned durable authority at the wire
```

### Recovery

```text
AppHost RecoveryObjective
  -> IaC recovery profile
    -> database/object restore construction
      -> isolated restored target
        -> schema, extension, content, and wire probes
          -> measured RPO/RTO receipt
            -> observability and policy evidence
```

### Backend support

```text
backend row
  -> client
  -> provision or external coordinate
  -> declare current schema
  -> converge unchanged-digest drift
  -> replace changed-digest generation
  -> health
  -> observe
  -> backup
  -> restore
  -> tenancy
  -> scale
  -> local profile
  -> security
  -> wire
```

A backend is operationally complete only when every required edge has an owner.

## Priority order

### Immediate planning closure

1. Establish the backend operational-conformance contract.
2. Establish the full 28-row PostgreSQL capability descriptor across C#, Python, TypeScript data, and TypeScript IaC.
3. Define schema-contract and generation-replacement owners in TypeScript data, TypeScript IaC, and C# Persistence.
4. Repair cluster-per-tenant parity in `kube/data.md`.
5. Add the IaC recovery folder.
6. Add worker deployment and backlog autoscaling.

Listed changes settle authority and state before adding new service adapters.

### Next operational tier

1. Add rollout strategy and schema-generation admission gates.
2. Close object-store recovery and custody.
3. Complete workload identity and lease realization.
4. Expand semantic health through existing health ports.
5. Make provider-arm capability admission exact.
6. Decide whether TypeScript Kafka requires Schema Registry.

### Existing tracked execution work

1. C# Compute sharding and broker ingest.
2. C# AppHost dead-letter replay seam after Persistence admits primitives.
3. Python worker admission/channel evidence.
4. TypeScript PGLite and foreign relational interop.
5. Live estate OTLP placement.

## Concrete acceptance conditions

### PostgreSQL

- One full 28-row descriptor governs C#, Python, TypeScript data, and TypeScript IaC.
- C# contains the eight rows currently present only in TypeScript; TypeScript contains the ten rows currently present only in C#; Python decodes the whole descriptor.
- No runtime-local roster can add, remove, rename, or weaken a canonical row.
- Every extension names project, image artifact, SQL identity, preload, dependency set, incompatibility set, probe, version, license, scope, restart class, failure rank, and consumer.
- IaC can realize every capability in the union and provisions every capability selected by the estate profile.
- Database activation remains profile-selected; inactive capabilities remain catalogued and verifiable.
- Tenant clusters receive the same durability, pool, preload, and backup semantics as the primary unless a profile explicitly changes them.
- PGLite declares a strict degraded capability set.

### Schema convergence

- One current desired-state manifest and content digest exist per store profile.
- Intentional digest change always creates a replacement database or cluster generation.
- Unchanged-digest drift repair is the only in-place schema mutation.
- Every durable table is disposable, replayable, copy-projectable, or logically replicable.
- Target generation validation covers extensions, constraints, indexes, roles, grants, RLS, counts, content hashes, projections, and semantic probes.
- Cutover is atomic, rollback retention is bounded, and workers reject mismatched generations.
- Runtime verifies and never changes schema.
- Marten, EF, raw SQL, extensions, SQLite, PostgreSQL, and PGLite cannot create competing schema or history authorities.
- No chronological change files, applied-change ledger, startup mutator, compatibility shim chain, or migration framework exists.

### Workers

- Runtime worker descriptors lower into deployable worker specs.
- Backlog, oldest age, in-flight work, and poison rate are observable.
- Scaling uses work evidence rather than CPU alone.
- Drain grace exceeds or coordinates with claim visibility.
- Worker releases cannot process incompatible payload or schema generations.
- GPU and remote placement are explicit profile data.

### Recovery

- Every durable database and object profile names backup and restore.
- Restore creates an isolated target and never overwrites the source.
- A drill verifies content identity, schema, extensions, roles, RLS, and application probes.
- Measured RPO/RTO compares against `RecoveryObjective`.
- Cluster-per-tenant and managed-cloud arms participate.
- Recovery credentials and encryption keys have explicit custody.

### Provider coverage

- `StackSpec` requirements fail before resource construction when an arm lacks a capability.
- External services satisfy missing provider cells through typed coordinates.
- `StackOutputs` publishes realized capability, not implied provider promises.
- README claims match realized cells.

## BullMQ decision for Rasm

BullMQ is a TypeScript/JavaScript package built on Redis. It provides queues, workers, delayed and repeatable work, priorities, rate limits, retries, stalled-job handling, job dependencies, events, and operational controls. Python and C# have unrelated queue products with similar features, but they are not BullMQ implementations sharing its runtime contract.

BullMQ is relevant to Rasm as a comparative design source and an integration boundary, not as core infrastructure.

| BullMQ concern | Rasm owner |
| --- | --- |
| Job declaration | TypeScript `DurableQueue.make`; C# command/workflow descriptors |
| Queue persistence | TypeScript Effect message storage plus PostgreSQL journal; C# Persistence rows |
| Worker | TypeScript `DurableQueue.worker`; C# Compute/AppHost dispatch; Python WorkerPool |
| Retry | shared budget/retry tables |
| Priority | TypeScript `WorkClass` urgency; C# `WorkLane` rank |
| Delay/schedule | TypeScript work schedule; C# SchedulePort; Python APScheduler locally |
| Stalled work | leases, visibility, orphan reclaim, supervisor |
| Dead letter | sole queue/outbox dead-letter owners |
| Dependencies | TypeScript workflow; C# JobGraph/workflow steps |
| Rate limit | TypeScript queue throttle; C# grant/budget/laneguard |
| Metrics | shared four-signal telemetry and receipts |
| Deployment | proposed IaC worker and KEDA owners |

Add a BullMQ adapter only when a downstream system already exposes Redis/BullMQ work that Rasm must consume or publish. Adapter rules:

- BullMQ job id maps to a boundary identity, never Rasm’s internal content identity by assumption.
- BullMQ state remains external transport state.
- Rasm stores its own settlement receipt and idempotency evidence.
- retries do not nest silently across BullMQ and Rasm budgets.
- trace, tenant, classification, and causal headers cross explicitly.
- adapter lives on a runtime transport boundary, not in TypeScript data or IaC core.

## Final assessment

Rasm does not need a larger dependency list to become a world-class backend corpus. It needs explicit closure between already-strong owners.

Highest-value work:

- unify what “backend supported” means
- establish full PostgreSQL capability parity across C#, Python, TypeScript data, and TypeScript IaC
- replace migration history with current desired state, immutable generations, rebuild, verification, and cutover
- make recovery as concrete as backup
- lower runtime work classes into deployable workers
- scale workers from queue evidence
- give Kubernetes releases the same rigor as AppHost fleet updates
- preserve Python as an execution fabric rather than another durable authority

Package additions remain surgical:

- `@effect/sql-pglite` for the already-planned embedded PostgreSQL profile
- `@electric-sql/pglite-tools` for explicit PGLite dump/restore evidence and data-only reconstruction
- `@confluentinc/schemaregistry` only if TypeScript Kafka promises registry-governed schemas
- KEDA, Argo Rollouts, and cert-manager as optional, pinned infrastructure dependencies with generated typed CRDs

Everything else should extend the existing tables, ports, rails, descriptors, receipts, and provider cells.

## Source index

### Central

- [libs/.planning/ARCHITECTURE.md](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/.planning/ARCHITECTURE.md)
- [libs/.planning/IDEAS.md](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/.planning/IDEAS.md)
- [libs/.planning/TASKLOG.md](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/.planning/TASKLOG.md)

### TypeScript

- [data README](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/typescript/data/README.md)
- [data architecture](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/typescript/data/ARCHITECTURE.md)
- [PostgreSQL lane](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/typescript/data/.planning/lane/postgres.md)
- [tenant lane](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/typescript/data/.planning/lane/tenant.md)
- [journal append](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/typescript/data/.planning/journal/append.md)
- [runtime architecture](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/typescript/runtime/ARCHITECTURE.md)
- [durable queue](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/typescript/runtime/.planning/work/queue.md)
- [durable workflow](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/typescript/runtime/.planning/work/flow.md)
- [durable entities](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/typescript/runtime/.planning/work/entity.md)
- [work schedule](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/typescript/runtime/.planning/work/schedule.md)
- [delivery](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/typescript/runtime/.planning/work/deliver.md)
- [pub/sub](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/typescript/runtime/.planning/net/pubsub.md)
- [IaC architecture](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/typescript/iac/ARCHITECTURE.md)
- [provider arms](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/typescript/iac/.planning/program/provider.md)
- [Kubernetes data plane](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/typescript/iac/.planning/kube/data.md)
- [Kubernetes workload](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/typescript/iac/.planning/kube/workload.md)
- [policy and drift](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/typescript/iac/.planning/operate/policy.md)
- [observability deployment](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/typescript/iac/.planning/operate/observe.md)
- [security tasks](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/typescript/security/TASKLOG.md)

### C#

- [AppHost architecture](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.AppHost/ARCHITECTURE.md)
- [AppHost orchestration](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.AppHost/.planning/Runtime/orchestration.md)
- [AppHost outbox](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.AppHost/.planning/Wire/outbox.md)
- [AppHost health](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.AppHost/.planning/Observability/health.md)
- [AppHost telemetry](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.AppHost/.planning/Observability/telemetry.md)
- [Persistence architecture](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.Persistence/ARCHITECTURE.md)
- [Persistence provisioning](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.Persistence/.planning/Store/provisioning.md)
- [Persistence coordination](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.Persistence/.planning/Store/coordination.md)
- [Persistence observability](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.Persistence/.planning/Store/observability.md)
- [Persistence recovery](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.Persistence/.planning/Version/recovery.md)
- [Compute architecture](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.Compute/ARCHITECTURE.md)
- [Compute scheduling](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.Compute/.planning/Runtime/scheduling.md)
- [Compute transport](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.Compute/.planning/Runtime/transport.md)

### Python

- [runtime architecture](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/python/runtime/ARCHITECTURE.md)
- [worker fabric](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/python/runtime/.planning/execution/workers.md)
- [execution lanes](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/python/runtime/.planning/execution/lanes.md)
- [recipe execution](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/python/runtime/.planning/execution/recipe.md)
- [runtime tasks](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/python/runtime/TASKLOG.md)
- [data architecture](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/python/data/ARCHITECTURE.md)
- [data query plane](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/python/data/.planning/tabular/query.md)
- [data lakehouse](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/python/data/.planning/tabular/lakehouse.md)
- [compute architecture](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/python/compute/ARCHITECTURE.md)

### External package and operator references

- [Effect SQL PGLite package](https://github.com/Effect-TS/effect/tree/main/packages/sql/pglite)
- [PGLite tools and pg_dump](https://pglite.dev/docs/pglite-tools)
- [PGLite fresh-instance upgrade path](https://pglite.dev/docs/upgrade)
- [Marten projection rebuilding](https://github.com/JasperFx/marten/blob/master/docs/events/projections/rebuilding.md)
- [Marten event versioning](https://github.com/JasperFx/marten/blob/master/docs/events/versioning.md)
- [Atlas declarative schema apply](https://atlasgo.io/declarative/apply)
- [Atlas Kubernetes declarative schema resource](https://atlasgo.io/integrations/kubernetes/declarative)
- [Stripe pg-schema-diff](https://github.com/stripe/pg-schema-diff)
- [PostgreSQL table modification](https://www.postgresql.org/docs/current/ddl-alter.html)
- [PostgreSQL explicit locking](https://www.postgresql.org/docs/current/explicit-locking.html)
- [PostgreSQL concurrent index construction](https://www.postgresql.org/docs/current/sql-createindex.html)
- [CloudNativePG current documentation](https://cloudnative-pg.io/docs/1.28/)
- [CloudNativePG bootstrap](https://cloudnative-pg.io/docs/1.28/bootstrap/)
- [CloudNativePG logical replication](https://cloudnative-pg.io/docs/1.28/logical_replication/)
- [CloudNativePG recovery](https://cloudnative-pg.io/documentation/current/recovery/)
- [CloudNativePG Barman Cloud plugin](https://cloudnative-pg.io/plugin-barman-cloud/docs/intro/)
- [KEDA scalers](https://keda.sh/docs/latest/scalers/)
- [KEDA PostgreSQL scaler](https://keda.sh/docs/latest/scalers/postgresql/)
- [Argo Rollouts concepts](https://argoproj.github.io/argo-rollouts/concepts/)
- [Argo Rollouts analysis](https://argoproj.github.io/argo-rollouts/features/analysis/)
- [Confluent JavaScript client and Schema Registry guidance](https://docs.confluent.io/kafka-clients/javascript/current/migration.html)
