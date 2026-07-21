# [PY_RUNTIME_IDEAS]

`runtime`'s forward pool of higher-order folder concepts, grounded in the numeric-science domain and the monorepo purpose. `[1]-[OPEN]` carries the active ideas as cards; each card names the capability, what it unlocks, and the gap or technique it draws on. `[2]-[CLOSED]` carries the finished or dropped ideas with a one-line disposition so the same idea is never re-litigated. An idea drives one or more `TASKLOG.md` tasks.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — altitude only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <present only on a BLOCKED or gated card; the exact observable that flips it actionable — a catalog row landing, a member query returning evidence, a package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart card — cross-folder as `pkg` `[SLUG]` or a same-folder prerequisite `[SLUG]`, prefixed follows/precedes/mirrors when build order is load-bearing>.
Capability, Shape, Unlocks, and Anchors are required on every open card; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Ideas state higher-order concepts, never landing-grain tasks.
-->

[METRIC_DOMAIN_VOCABULARY]-[QUEUED]: metric domains close into one typed vocabulary with coverage proof.
- Capability: every metric domain is a member of one closed vocabulary and every recorded measure provably owns an instrument row, so a dangling record is a boot-time gate failure instead of a runtime fault.
- Shape: a `Domain` StrEnum on `libs/python/runtime/.planning/observability/metrics.md` typing `InstrumentSpec.domain` and the `Metrics.record` parameter, and a coverage proof over the recorded-measure roster at composition.
- Unlocks: a record site without its `INSTRUMENTS` row becomes unrepresentable; sibling record sites import the member, never a bare string.
- Anchors: `InstrumentSpec` and the `_DOMAIN_SLOT` derivation; the record-and-row same-pass law at `libs/python/runtime/RULINGS.md`; the `HOOK_ID` registration refusal as the guard pattern.

[SECRET_BACKEND_FAMILY]-[QUEUED]: the cloud secret tier closes as a multi-provider family.
- Capability: set completion on the secret ladder — Vault (`hvac`) and Azure Key Vault (`azure-keyvault-secrets`) rows land beside the GCP arm so the cloud tier is a closed provider family, each row behind its `Feature` gate and the `RetryClass.SECRET` row.
- Shape: `SecretTier` gains provider discrimination and `SECRET_LADDER` gains two rows on `libs/python/runtime/.planning/execution/admission.md`; lazy provider imports defer to the gated arm's first fire; `NestedSecretsSettingsSource` mounts nested secret trees on the settings source order.
- Unlocks: deployment-portable secret custody — one admitted boundary across GCP, Vault, and Azure estates with no second resolution surface.
- Anchors: `SECRET_LADDER`/`TierRow` on `execution/admission#SETTINGS`; the admitted `hvac` and `azure-keyvault-secrets` manifest rows; `NestedSecretsSettingsSource` and `SecretManagerServiceAsyncClient` on `libs/python/runtime/.api/pydantic-settings.md` and `libs/python/runtime/.api/google-cloud-secret-manager.md`.

[ACQUISITION_POLICY_SURFACE]-[QUEUED]: resource acquisition gains a policy-complete transport surface — caching, proxy egress, custody, and batch.
- Capability: the HTTP leg caches under RFC-9111 semantics beside the content-keyed elision, egress honors proxy policy, single-writer custody guards mutable roots, and batched acquisition rides one call.
- Shape: `hishel` cache transport wrapping the httpx legs with a local file/sqlite store, one `httpx` `Proxy` policy row, `anyio` `ResourceGuard` custody on mutable root handles, and `fsspec.open_files` batch acquisition — all on `libs/python/runtime/.planning/transport/roots.md`.
- Unlocks: bandwidth-proof repeated artifact acquisition, enterprise-network egress, and race-free root mutation for every sibling consumer.
- Anchors: `TransportResource`/`ResourceRoot` owners on `transport/roots#RESOURCE`; `Proxy` on `libs/python/runtime/.api/httpx.md`; `libs/python/.api/anyio.md` and `libs/python/.api/fsspec.md`; the admitted `hishel[httpx]` manifest row; the folder two-cache ruling at `libs/python/runtime/RULINGS.md`.

[FABRIC_ADMISSION_PROBES]-[QUEUED]: the worker fabric gains pre-flight guest admission and channel evidence.
- Capability: a WASM kernel admits only validated guest bytes, and supervision verdicts carry channel-level evidence for the dialed and spawned kinds.
- Shape: `wasmtime` `Module.validate` as the guest admission gate before instantiation on the WASM arm, and `psutil` `Process.net_connections` as REMOTE/DAEMON probe evidence columns — both on `libs/python/runtime/.planning/execution/workers.md`.
- Unlocks: a malformed guest refuses at admission with a typed `config` fault instead of an instantiation trap; supervision verdicts distinguish a dead channel from a saturated one.
- Anchors: the guest arm and `Supervisor` probe rows on `execution/workers`; `libs/python/runtime/.api/wasmtime.md`; `libs/python/.api/psutil.md`.

[CLOCK_IDENTITY_FOLD]-[QUEUED]: the clock page folds into the evidence identity band, dissolving the one-page folder.
- Capability: structural law lands — a sub-folder is earned by 2+ non-eponymous sibling pages, `clock/` holds one eponymous page, and the S1 identity band (clock, identity, shapes) already binds the owners.
- Shape: `libs/python/runtime/.planning/clock/clock.md` moves to `libs/python/runtime/.planning/evidence/clock.md` unchanged in law; the clock module spells inside the evidence namespace, matching the page path, and wire/admission consumers re-import the same symbols; router, codemap, seam rosters, and the import rail re-anchor on `libs/python/runtime/README.md` and `libs/python/runtime/ARCHITECTURE.md`.
- Unlocks: evidence closes at four sibling pages and the folder census carries no unearned sub-folder.
- Anchors: the no-eponymous-folder law; ARCHITECTURE's S1–S3 identity band.

[PRODUCER_DISTRIBUTIONS]-[QUEUED]: geometry distribution rows on the one `INSTRUMENTS` table.
- Capability: deviation magnitude, mesh genus/aspect, and EUI land as `rasm.geometry.<measure>` histogram rows beside `rasm.geometry.evidence.duration`.
- Shape: one `InstrumentSpec` row with one `SyncInstruments` field per charter-named measure on `libs/python/runtime/.planning/observability/metrics.md`, reaching `record`'s mapping arm through `_DOMAIN_SLOT` with zero entrypoint edits.
- Unlocks: geometry-domain dashboards aggregate distributions without receipt post-processing.
- Anchors: the `INSTRUMENTS` growth law and the `record` mapping arm on `observability/metrics#METRIC`; the landed geometry charter table on `libs/python/geometry/.planning/graduation.md` spelling exact measure names, units, and aggregations.
- Tension: receipts stay the data plane by ruling — a row lands only for a measure the charter table spells, nothing beyond its roster.
- Ripple: `geometry` `[PRODUCER_DISTRIBUTIONS]`.

[SUPPORT_BUNDLE_CAPSULE]-[BLOCKED]: support-bundle capture completes over the C#-minted wire vocabulary.
- Capability: capsule capture and the diagnostic `Route` — already landed — gain their wire vocabulary, closing the bundle capture loop across the runtime/Compute channel.
- Shape: the two restored `PROTO_VOCABULARY` rows on `libs/python/runtime/.planning/transport/shapes.md`.
- Unlocks: end-to-end support-bundle capture across the runtime/Compute wire.
- Anchors: the landed capsule capture and diagnostic `Route`; `rasm.runtime._pb2.channels_pb2`.
- Arms: `SupportBundleRequest`, `SupportBundleReply`, and `CaptureBundle` minted at `libs/csharp/Rasm.Compute/.planning/Runtime/wire.md`, then `rasm.runtime._pb2.channels_pb2` regenerated.

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[WORKER_TELEMETRY_PARENTING]-[COMPLETE]: landed as the `WorkerBoot` capture + one `_worker_boot` initializer on `.planning/execution/workers.md` — parented `worker.<name>` span in `traced_kernel`, `WORKER_SIGNAL_PROFILE` geometry through the telemetry injection seams, seal-carried remote-floor boot, daemon spawn-env forwarding, and the exit-owned flush law.
[WORKER_PROFILER_ATTACH]-[COMPLETE]: landed as `_worker_boot`'s profiler arm with the `worker.kind` install tag and kernel-subject `phase` window in `traced_kernel`; `Profiles.install` samples on-CPU Python and GIL-releasing native kernels, `Profiles.phase` supplies the null-window no-op, and `Profiles.receipt` supplies the boot-capture read on `.planning/observability/profiles.md`.
[FABRIC_COST_ATTRIBUTION]-[COMPLETE]: landed as the receipts-minted `Cost` evidence (`sampled`/`delta`/`combined`/`measures`), including signed RSS change without a false peak claim, the two-read `traced_kernel` bracket, the lane drain-window envelope on `DrainReceipt.cost`, and four `rasm.cost.<measure>` rows under `domain="cost"` on `.planning/observability/metrics.md` with the tenant fold riding `_attributed`.
[COMPOSITION_SCOPED_CAPSULE]-[COMPLETE]: already realized on disk — `ScopeKey`/`DEFAULT_SCOPE` mint on receipts, scope-keyed `Hooks._points`/`_taps`/`_rings`, `Metrics._state`/`_receipts`, and the `Telemetry`/`Instrumentation`/`Profiles` receipt maps beside their process latches.
[HISTOGRAM_WIRE_PARITY]-[COMPLETE]: already realized on disk — telemetry `WIRE_AGGREGATION` sets `ExponentialBucketHistogramAggregation` on both metric exporter rows, advisory rows staying the deployment-`View` fallback.
[EGRESS_TRANSPORT_ROW]-[COMPLETE]: already realized on disk — `EgressTransport`, the `SignalProfile.transport` column, SIDECAR-only `GRPC_ELIGIBLE`, per-transport `EGRESS` factory triples, and the HTTP path derivation on telemetry.
[DBAPI_TRAIN_ROW]-[COMPLETE]: already realized on disk — `DbapiSeam`, `_DBAPI_POSTURE`, and the polymorphic `Instrumentation.dbapi` wrap-or-retrofit entry beside `TRAIN` on metrics.
[OBSERVABILITY_SPINE_WAVE]-[COMPLETE]: landed as three observability owners and the metrics deepening — `observability/logging` chain owner under the stdout ship law with the in-process OTLP escape hatch gated by `LogShip`; `observability/hooks` scoped registry (`rasm.<pkg>.<domain>.<point>` ids, veto/observe/replay modalities, telemetry taps); `observability/profiles` pyroscope push, benchmark-receipt family, and the offline-job flush envelope; metrics gained `rasm.<domain>.<measure>` wire naming, the tenant baggage dimension, and the composition-root instrumentor train.
[REMOTE_WORKER_DISPATCH]-[COMPLETE]: landed as `WorkerKind.REMOTE` on `execution/workers` — `KIND_POLICY(fidelity=True, restart=RetryClass.SSH)`, the `WorkerPool` remote arm over one `transport/roots` `RemoteEndpoint` channel with `remote_floor` far-side, shm-wire refusal, channel-liveness supervision; roots scope law widened one seam.
[SHARED_MEMORY_CHANNEL]-[COMPLETE]: landed as the `Wire.SHARED_MEMORY` span channel on `execution/workers#FABRIC` — `ShmSpan` named blocks, exporter-owned unlink, worker-side `numpy.frombuffer` reconstruction, ingress-only law.
