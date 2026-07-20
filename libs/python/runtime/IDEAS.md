# [PY_RUNTIME_IDEAS]

`runtime`'s forward pool of higher-order folder concepts, grounded in the numeric-science domain and the monorepo purpose. `[1]-[OPEN]` carries the active ideas as cards; each card names the capability, what it unlocks, and the gap or technique it draws on. `[2]-[CLOSED]` carries the finished or dropped ideas with a one-line disposition so the same idea is never re-litigated. An idea drives one or more `TASKLOG.md` tasks.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
-->

[WORKER_TELEMETRY_PARENTING]-[QUEUED]: every pooled, spawned, and remote worker becomes a telemetry-parented emitter instead of an unparented carrier.
- Capability: a worker-side lean install closes the standing boundary gap — the crossing carries the W3C carrier today while a pooled worker without its own install emits nothing — so kernel-interior spans, metrics, and profile links land parented under the offload span.
- Shape: one worker install path on `libs/python/runtime/.planning/execution/workers.md` pool initializer rows — loky initializer, pebble initializer, daemon spawn environment, `remote_floor` far side — delegating to `Telemetry.install` on `libs/python/runtime/.planning/observability/telemetry.md` with a worker-shaped `SignalProfile` (small queues, boundary-flush geometry) and a flush law at settle, roll, and retire; `shipped` resolves the carried context and opens the kernel span under it.
- Unlocks: distributed offloads trace whole — worker-interior evidence joins the one trace, per-kernel metrics attach exemplars, and pyroscope thread tags reach worker processes.
- Anchors: the fabric's stitch-and-resolve gate and pool initializer rows (tblib latch precedent) on `execution/workers#FABRIC`; `resource=`/`signal_profile=` injection seams on `observability/telemetry#TELEMETRY`; the `JobRun.bounded` flush precedent on `observability/profiles#JOB`.
- Tension: a forked worker inherits no live batch thread, so install runs post-spawn per worker process; the gRPC egress row stays refused in forking pools per the settled fork-hazard ruling.

[WORKER_PROFILER_ATTACH]-[QUEUED]: the continuous profiler attaches at the worker floor, so kernel flames come from the processes that burn the cycles.
- Capability: profiling-agent bootstrap inside every pool worker — configure at initializer, tag by subject, shut down at retirement — with span-profile correlation, so a slow offload span clicks through to the flame of the worker that ran it and sibling HOSTILE kernel subjects profile where they execute.
- Shape: one profiler-attach row per pool initializer arm on `libs/python/runtime/.planning/execution/workers.md` riding the `[WORKER_TELEMETRY_PARENTING]` install seam; agent lifecycle and the subject tag vocabulary on `libs/python/runtime/.planning/observability/profiles.md`; folder subject maps stay folder-owned rows.
- Unlocks: geometry and compute kernel hot-path answers from production traces; the profile leg of the worker fabric closes beside spans and metrics.
- Anchors: pyroscope push and span-profile correlation on `observability/profiles.md`; pool initializer rows on `execution/workers#FABRIC`; `pyroscope-io` (admission pending).
- Tension: attach runs post-spawn per worker process — a parent-side profiler never observes the floor.
- Ripple: `geometry` `[KERNEL_PROFILE_EVIDENCE]`.

[FABRIC_COST_ATTRIBUTION]-[QUEUED]: resource cost becomes attributed evidence at the crossing grain.
- Capability: every kernel crossing and lane drain folds CPU-time, RSS-peak, IO, and context-switch deltas into typed cost evidence keyed by the `rasm.tenant` baggage entry and the kernel identity, so tenant cost derives from the fabric that spent it, never from backend estimation.
- Shape: delta capture bracketing `shipped` and the pooled settle on `libs/python/runtime/.planning/execution/workers.md`, cost columns on the drain evidence of `libs/python/runtime/.planning/observability/receipts.md`, and `rasm.cost.<measure>` instrument rows with the tenant fold on `libs/python/runtime/.planning/observability/metrics.md`.
- Unlocks: per-tenant usage boards and reconciliation of the C#-minted capability `CostVector` (`estimated`/`charged` on `transport/serve#CAPABILITY_INVOKE`) against one measured truth.
- Anchors: `psutil` `Process.cpu_times`/`memory_info`/`num_ctx_switches` deltas (`libs/python/.api/psutil.md`), the `Supervisor` probe precedent, the `INSTRUMENTS` growth law, `TENANT_BAGGAGE`.
- Ripple: `libs/.planning` `[COST_ATTRIBUTION_BAGGAGE]`.

[COMPOSITION_SCOPED_CAPSULE]-[QUEUED]: observability registries scope per composition, so two apps composing the runtime in one process never fight.
- Capability: app-neutrality lands structurally — `Hooks` tables, `Metrics` state, and the `Telemetry`/`Instrumentation`/`Profiles` latch receipts key by a composition scope with a default scope preserving today's call shape, while OTel globals stay the SDK's process singletons.
- Shape: one composition-scope key threading the class-held frozen Maps on `libs/python/runtime/.planning/observability/hooks.md`, `libs/python/runtime/.planning/observability/metrics.md`, `libs/python/runtime/.planning/observability/telemetry.md`, and `libs/python/runtime/.planning/observability/profiles.md`, and the default-sink resolution on `libs/python/runtime/.planning/observability/receipts.md`; a second composition registers its own subscriber tables and receives its own install receipts without shadowing the first.
- Unlocks: two rasm-composed tools embedded in one host process — plugin hosts included — with partitioned hook fan-out and per-composition install evidence.
- Anchors: package-qualified hook id grammar (already collision-refusing), the atomically swapped frozen-Map idiom every registry already rides, `latched`.
- Tension: provider install is process-global by SDK law — the capsule partitions subscriber and instrument custody, never the exporter pipeline; the first emitting install owns the process pipeline.

[HISTOGRAM_WIRE_PARITY]-[QUEUED]: histogram geometry reaches the estate wire law — base2-exponential default with advisory fallback.
- Capability: branch histograms export exponential-bucket aggregation matching the C# and TS legs, so cross-runtime latency panels compare one bucket algebra; the API-level advisory stays the fallback where an explicit shape is ruled.
- Shape: meter construction on `libs/python/runtime/.planning/observability/telemetry.md` gains the exponential default-aggregation row; `libs/python/runtime/.planning/observability/metrics.md` keeps advisory rows as the fallback contract.
- Unlocks: merged cross-language histograms on the dashboard plane with no translation-time re-bucketing.
- Anchors: `ExponentialBucketHistogramAggregation` on `libs/python/.api/opentelemetry-sdk.md`; the wire law's base2-exponential ruling; the `views=` seam the telemetry boundary reserves.

[EGRESS_TRANSPORT_ROW]-[QUEUED]: OTLP egress transport becomes a selectable axis on the telemetry install.
- Capability: `opentelemetry-exporter-otlp-proto-grpc` joins as a daemon-only transport row beside the proto-http default — persistent-channel export where streaming throughput dominates.
- Shape: one `EgressTransport` vocabulary keying a per-transport exporter-factory triple on `libs/python/runtime/.planning/observability/telemetry.md`; `SignalProfile` gains a `transport` column with the SIDECAR row alone eligible for the gRPC value; http rows keep the `/v1/<signal>` path derivation, the gRPC row targets `host:port` with no path.
- Unlocks: channel-reuse export for the long-lived serve daemon with the flush law and job envelope untouched.
- Anchors: the `SIGNAL_SPECS` exporter-factory seam and `SIGNAL_PROFILE` table on `observability/telemetry#TELEMETRY`; `.api/opentelemetry-exporter-otlp-proto-grpc.md`.
- Tension: a gRPC channel does not survive `fork()`, so every worker-forking profile refuses the row structurally; proto-http stays the sole estate default by settled ruling.

[DBAPI_TRAIN_ROW]-[QUEUED]: the instrumentor train gains a generic PEP-249 arm, so every unwrapped driver emits query spans.
- Capability: `opentelemetry-instrumentation-dbapi` wraps drivers the contrib train has no dedicated instrumentor for — duckdb and ADBC DBAPI legs first — narrowing the ruled "no DuckDB instrumentation exists" to span emission beside the engine-profile harvest.
- Shape: one wrap seam beside `TRAIN` on `libs/python/runtime/.planning/observability/metrics.md` — a connection-factory registration a data-side consumer threads its driver through, never an import-time patch of a module this folder does not admit.
- Unlocks: query spans for every store the data folder rides, one seam per driver, with `QueryReceipt.profile` staying the data owner's truth.
- Anchors: `opentelemetry-instrumentation-dbapi` (admission pending through the campaign lane); the psycopg/sqlite3 row precedent on `TRAIN`.
- Tension: span emission complements the receipts data plane, never replaces it; the wrap activates at the composition root only.

[SECRET_BACKEND_FAMILY]-[QUEUED]: the cloud secret tier closes as a multi-provider family.
- Capability: set completion on the secret ladder — Vault (`hvac`) and Azure Key Vault (`azure-keyvault-secrets`) rows land beside the GCP arm so the cloud tier is a closed provider family, each row behind its `Feature` gate and the `RetryClass.SECRET` row.
- Shape: `SecretTier` gains provider discrimination and `SECRET_LADDER` gains two rows on `libs/python/runtime/.planning/execution/admission.md`; lazy provider imports defer to the gated arm's first fire; `NestedSecretsSettingsSource` mounts nested secret trees on the settings source order.
- Unlocks: deployment-portable secret custody — one admitted boundary across GCP, Vault, and Azure estates with no second resolution surface.
- Anchors: `SECRET_LADDER`/`TierRow` on `execution/admission#SETTINGS`; `hvac` and `azure-keyvault-secrets` (admission pending); `NestedSecretsSettingsSource` and `SecretManagerServiceAsyncClient` on `libs/python/runtime/.api/pydantic-settings.md` and `libs/python/runtime/.api/google-cloud-secret-manager.md`.

[ACQUISITION_POLICY_SURFACE]-[QUEUED]: resource acquisition gains a policy-complete transport surface — caching, proxy egress, custody, and batch.
- Capability: the HTTP leg caches under RFC-9111 semantics beside the content-keyed elision, egress honors proxy policy, single-writer custody guards mutable roots, and batched acquisition rides one call.
- Shape: `hishel` cache transport wrapping the httpx legs with a local file/sqlite store, one `httpx` `Proxy` policy row, `anyio` `ResourceGuard` custody on mutable root handles, and `fsspec.open_files` batch acquisition — all on `libs/python/runtime/.planning/transport/roots.md`.
- Unlocks: bandwidth-proof repeated artifact acquisition, enterprise-network egress, and race-free root mutation for every sibling consumer.
- Anchors: `TransportResource`/`ResourceRoot` owners on `transport/roots#RESOURCE`; `Proxy` on `libs/python/runtime/.api/httpx.md`; `libs/python/.api/anyio.md` and `libs/python/.api/fsspec.md`; `hishel` (admission pending).
- Tension: cache truth stays RFC-9111 revalidation; a content-keyed hit short-circuits earlier on the lanes cache — two elisions, two owners, no overlap.

[FABRIC_ADMISSION_PROBES]-[QUEUED]: the worker fabric gains pre-flight guest admission and channel evidence.
- Capability: a WASM kernel admits only validated guest bytes, and supervision verdicts carry channel-level evidence for the dialed and spawned kinds.
- Shape: `wasmtime` `Module.validate` as the guest admission gate before instantiation on the WASM arm, and `psutil` `Process.net_connections` as REMOTE/DAEMON probe evidence columns — both on `libs/python/runtime/.planning/execution/workers.md`.
- Unlocks: a malformed guest refuses at admission with a typed `config` fault instead of an instantiation trap; supervision verdicts distinguish a dead channel from a saturated one.
- Anchors: the guest arm and `Supervisor` probe rows on `execution/workers`; `libs/python/runtime/.api/wasmtime.md`; `libs/python/.api/psutil.md`.

[SUPPORT_BUNDLE_CAPSULE]-[QUEUED]: one diagnostic capsule folds the daemon's whole evidence state into a servable support bundle.
- Capability: the C# support-bundle peer at Python grain — faulthandler stack dumps, gated tracemalloc snapshots, the hook replay window, install and train receipts, admitted-context render, and supervision verdicts fold into one content-keyed archive.
- Shape: a new `libs/python/runtime/.planning/observability/bundle.md` owner — one collectors table, one archive fold keyed through content identity — served as one `Route` row on `libs/python/runtime/.planning/transport/serve.md`.
- Unlocks: one-call incident capture from a live daemon the C# host or an operator pulls over the standing wire; offline jobs attach the same capsule on the fault arm.
- Anchors: stdlib `faulthandler`/`tracemalloc`; hook REPLAY rings on `observability/hooks#HOOKS`; `InstallReceipt`/`TrainReceipt`/`ProfilesReceipt`; `ContentKey` on `evidence/identity#IDENTITY`; the serve `Route` growth law.
- Tension: collection is pull-driven and bounded — no standing sampling loop lands beside the admitted profilers.

[CLOCK_IDENTITY_FOLD]-[QUEUED]: the clock page folds into the evidence identity band, dissolving the one-page folder.
- Capability: structural law lands — a sub-folder is earned by 2+ non-eponymous sibling pages, `clock/` holds one eponymous page, and the S1 identity band (clock, identity, shapes) already binds the owners.
- Shape: `libs/python/runtime/.planning/clock/clock.md` moves to `libs/python/runtime/.planning/evidence/clock.md` unchanged in law; router, codemap, seam rosters, and the import rail re-anchor on `libs/python/runtime/README.md` and `libs/python/runtime/ARCHITECTURE.md`.
- Unlocks: evidence closes at four sibling pages and the folder census carries no unearned sub-folder.
- Anchors: the no-eponymous-folder law; ARCHITECTURE's S1–S3 identity band.
- Tension: wire and admission consumers import the clock owner — the module-spelling ruling (standalone module beside the evidence namespace, or inside it) lands with the move.

[PRODUCER_DISTRIBUTIONS]-[BLOCKED]: geometry distribution rows on the one `INSTRUMENTS` table.
- Capability: deviation magnitude, mesh genus/aspect, and EUI land as `rasm.geometry.<measure>` histogram rows beside `rasm.geometry.evidence.duration`.
- Shape: one `InstrumentSpec` row with one `SyncInstruments` field per measure on `libs/python/runtime/.planning/observability/metrics.md`, reaching `record`'s mapping arm through `_DOMAIN_SLOT` with zero entrypoint edits.
- Unlocks: geometry-domain dashboards aggregate distributions without receipt post-processing.
- Anchors: the `INSTRUMENTS` growth law and the `record` mapping arm on `observability/metrics#METRIC`.
- Tension: receipts stay the data plane by ruling; the blocker is one answerable question — which geometry measures does the dashboard charter name? — resolved against the dashboard-compile charter on `libs/typescript/iac/.planning/operate/observe.md` before a row lands.
- Ripple: `geometry` `[PRODUCER_DISTRIBUTIONS]`.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

[OBSERVABILITY_SPINE_WAVE]-[COMPLETE]: landed as three observability owners and the metrics deepening — `observability/logging` chain owner under the stdout ship law with the in-process OTLP escape hatch gated by `LogShip`; `observability/hooks` scoped registry (`rasm.<pkg>.<domain>.<point>` ids, veto/observe/replay modalities, telemetry taps); `observability/profiles` pyroscope push, benchmark-receipt family, and the offline-job flush envelope; metrics gained `rasm.<domain>.<measure>` wire naming, the tenant baggage dimension, and the composition-root instrumentor train.

[REMOTE_WORKER_DISPATCH]-[COMPLETE]: landed as `WorkerKind.REMOTE` on `execution/workers` — `KIND_POLICY(fidelity=True, restart=RetryClass.SSH)`, the `WorkerPool` remote arm over one `transport/roots` `RemoteEndpoint` channel with `remote_floor` far-side, shm-wire refusal, channel-liveness supervision; roots scope law widened one seam.

[SHARED_MEMORY_CHANNEL]-[COMPLETE]: landed as the `Wire.SHARED_MEMORY` span channel on `execution/workers#FABRIC` — `ShmSpan` named blocks, exporter-owned unlink, worker-side `numpy.frombuffer` reconstruction, ingress-only law.
