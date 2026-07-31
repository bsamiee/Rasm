# [PY_RUNTIME_IDEAS]

`runtime`'s forward pool of higher-order folder concepts, grounded in the numeric-science domain and the monorepo purpose. `[1]-[OPEN]` carries the active ideas as cards; each card names the capability, what it unlocks, and the gap or technique it draws on. `[2]-[CLOSED]` carries the finished or dropped ideas with a one-line disposition so the same idea is never re-litigated. Ideas drive one or more `TASKLOG.md` tasks.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — concept grain only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <BLOCKED or gated cards only; the exact observable flipping it actionable — catalog row landing, member query evidence, package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart — cross-folder `pkg` `[SLUG]`, same-folder prerequisite `[SLUG]`; load-bearing build order prefixes follows/precedes/mirrors>.
Capability, Shape, Unlocks, and Anchors are required on every open card; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Ideas state higher-order concepts, never landing-grain tasks.
-->

[EVIDENCE_PLANE_PRODUCERS]-[QUEUED]: the durable evidence plane gains the producers its whole thesis presumes.
- Capability: audit and metering facts become a recorded stream rather than a declared one — every state mutation a branch performs durably, and every crossing it prices, reaches the journal, so the retention classes, the resource vocabulary, the subject index, the exact-decimal rating, and the crypto-shredded erasure all price real rows instead of standing as an unreachable algebra.
- Shape: `Journal.record` call sites at the mutating and metered owners across the four sibling folders — the object-store mutation legs and the egress veto points at `libs/python/data`, the IFC authoring and model-write legs at `libs/python/geometry`, the graduation and study legs at `libs/python/compute`, the product-emit legs at `libs/python/artifacts` — each folder choosing its own `Retain` class per action and its own `Resource` per metered quantity.
- Unlocks: a portability export and an erasure that answer over real subjects, a settlement that prices real quantities, and a groom that reclaims real rows — none of which a plane with no producer can demonstrate.
- Anchors: the landed `Fact` family, `Retain`/`Resource`/`Actor` vocabularies, `RESOURCES`/`WINDOWS` tables, and census gate at `libs/python/runtime/.planning/observability/journal.md`; the implemented port at `libs/python/data/.planning/tabular/journal.md`; the bound pair the daemon root already threads at `libs/python/runtime/.planning/transport/serve.md`; the peer roster at `libs/python/typescript/data/.planning/journal/fact.md` fixing the cross-branch vocabulary.
- Tension: the plane is complete and the port is bound, so the gap is producers alone — but S0 records nothing of its own by the branch strata law, and `transport/roots` rules receipt semantics the composing tier's, so every call site seats at a sibling and none can be landed from the owning folder.
- Ripple: `data` `[JOURNAL_LEDGER_OWNER]` — follows.

[SECRET_BACKEND_FAMILY]-[QUEUED]: the cloud secret tier closes as a multi-provider family.
- Capability: set completion on the secret ladder — Vault (`hvac`) and Azure Key Vault (`azure-keyvault-secrets`) rows land beside the GCP arm so the cloud tier is a closed provider family, each row behind its `Feature` gate and the `RetryClass.SECRET` row.
- Shape: `SecretTier` gains provider discrimination and `SECRET_LADDER` gains two rows on `libs/python/runtime/.planning/execution/admission.md`; lazy provider imports defer to the gated arm's first fire; `NestedSecretsSettingsSource` mounts nested secret trees on the settings source order.
- Unlocks: deployment-portable secret custody — one admitted boundary across GCP, Vault, and Azure estates with no second resolution surface.
- Anchors: `SECRET_LADDER`/`TierRow` on `execution/admission#SETTINGS`; the admitted `hvac` and `azure-keyvault-secrets` manifest rows; `NestedSecretsSettingsSource` and `SecretManagerServiceAsyncClient` on `libs/python/runtime/.api/pydantic-settings.md` and `libs/python/runtime/.api/google-cloud-secret-manager.md`.

[ACQUISITION_POLICY_SURFACE]-[QUEUED]: resource acquisition gains a policy-complete transport surface — caching, proxy egress, custody, and batch.
- Capability: the HTTP leg caches and revalidates under RFC-9111 semantics per DESTINATION beside the content-keyed elision, egress honors proxy policy, single-writer custody guards mutable roots, and batched acquisition rides one call.
- Shape: the `hishel` cache transport wrapping the httpx legs over an `AsyncSqliteStorage` keyed under the admitted scratch root (LANDED), one `httpx` `Proxy` policy row, `anyio` `ResourceGuard` custody on mutable root handles, and `fsspec.open_files` batch acquisition — all on `libs/python/runtime/.planning/transport/roots.md`.
- Unlocks: bandwidth-proof repeated artifact acquisition, enterprise-network egress, and race-free root mutation for every sibling consumer.
- Anchors: `TransportResource`/`ResourceRoot` owners on `transport/roots#RESOURCE`; `Proxy` on `libs/python/runtime/.api/httpx.md`; `libs/python/.api/anyio.md` and `libs/python/.api/fsspec.md`; the admitted `hishel[httpx]` manifest row; the folder two-cache ruling at `libs/python/runtime/RULINGS.md`.

[CROSSING_ADMISSION_PROBES]-[QUEUED]: the worker crossing gains pre-flight guest admission and channel evidence.
- Capability: a WASM kernel admits only validated guest bytes, and supervision verdicts carry channel-level evidence for the dialed and spawned kinds.
- Shape: `wasmtime` `Module.validate` as the guest admission gate before instantiation on the WASM arm, and `psutil` `Process.net_connections` as REMOTE/DAEMON probe evidence columns — both on `libs/python/runtime/.planning/execution/workers.md`.
- Unlocks: a malformed guest refuses at admission with a typed `config` fault instead of an instantiation trap; supervision verdicts distinguish a dead channel from a saturated one.
- Anchors: the guest arm and `Supervisor` probe rows on `execution/workers`; `libs/python/runtime/.api/wasmtime.md`; `libs/python/.api/psutil.md`.

[CLOCK_IDENTITY_FOLD]-[QUEUED]: the clock page folds into the evidence identity band, dissolving the one-page folder.
- Capability: structural law lands — a sub-folder is earned by 2+ non-eponymous sibling pages, `clock/` holds one eponymous page, and the S1 identity band (clock, identity, shapes) already binds the owners.
- Shape: `libs/python/runtime/.planning/clock/clock.md` moves to `libs/python/runtime/.planning/evidence/clock.md` unchanged in law; the clock module spells inside the evidence namespace, matching the page path, and wire/admission consumers re-import the same symbols; router, codemap, seam rosters, and the import rail re-anchor on `libs/python/runtime/README.md` and `libs/python/runtime/ARCHITECTURE.md`.
- Unlocks: evidence closes at four sibling pages and the folder census carries no unearned sub-folder.
- Anchors: the no-eponymous-folder law; ARCHITECTURE's S1–S3 identity band.

[GENERATION_RECOVERY_CONTRACT]-[QUEUED]: this branch's backend generation carries its own recovery verdict.
- Capability: a restored store admits on evidence — the observed generation proves contract identity while the measured data frontier proves recency — so a Python-only application verdicts a point-in-time restore, a promoted replica, and a rebuilt embedded store on one rail with no peer branch present.
- Shape: recovery columns on the contract owner at `libs/python/runtime/.planning/execution/admission.md` `[03]-[BACKEND_CONTRACT]` — the declared recovery objective admission reads, and the measured-window comparison `admit` folds beside its realized-observation evidence.
- Unlocks: `libs/.planning` `[GENERATION_RECOVERY_CONTRACT]` — the Python mint of the corpus recovery rows, so a merged-generation restore verdicts without this branch's runbook standing in for the law.
- Anchors: `BackendGeneration` `compose`/`admit`/`merge` on `execution/admission#BACKEND_CONTRACT`; `tests/contracts/MANIFEST.md` `BACKEND_CONTRACT` and its peer minters; the `RuntimeContext` profile rows admission already gates on.
- Tension: recovery evidence is time-shaped where the generation is content-shaped — the verdict separates contract identity from data recency without minting a second generation notion.
- Ripple: `libs/.planning` `[GENERATION_RECOVERY_CONTRACT]`.

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[BOUNDED_OCCUPANCY_ROSTER]-[COMPLETE]: landed as the band-keyed probe registry — `Metrics.occupied` takes a required `band`, `MetricState.occupancy` keys `Map[str, Block[Occupancy]]`, `_inflight` emits one observation per band under `Dimension.BAND`, and `rasm.lane.in_flight` collapsed into `rasm.band.in_flight` beside a `band` `DOMAINS` row. Sibling rows folding one block stayed refused: they publish one number once per bound, where a band dimension makes every future bound a VALUE. Registration for the two process-wide bands rides `[BAND_PROBE_REGISTRATION]`, a module-scope limiter carrying no lifetime a context manager can bracket.
[SUPPORT_BUNDLE_CAPSULE]-[COMPLETE]: recorded blocker refuted on disk — `libs/csharp/Rasm.Compute/.planning/Runtime/wire.md` already carries the `Diagnostic`/`CaptureBundle` method row and binds `SupportBundleRequest`/`SupportBundleReply` onto the Python vocabulary, so the two `PROTO_VOCABULARY` rows landed on `transport/shapes` and the capsule closes end to end.
[METRIC_DOMAIN_VOCABULARY]-[COMPLETE]: `DOMAINS` closes the segment vocabulary as capability subject beside charter, `MEASURES` reads `DOMAINS[spec.domain]` totally across the instrument table so an unrostered segment refuses at IMPORT, and each cross-folder producer's composition leg proves its own `(domain, measure)` pairs against that one public map. `Domain` as a StrEnum stays refused: `InstrumentSpec.domain` derives from `name`, an enum member drops the subject charter peer rosters compare byte-identical, and a vocabulary declared beside that `Map` lags it on the next row.
[CONSUMPTION_AXIS_SPELLING]-[COMPLETE]: the axis roster `libs/.planning/ARCHITECTURE.md` `[10]-[CONSUMPTION_MODEL]` fixes landed at every branch minter with identical closed-axis vocabularies and one common open-axis descriptor shape; refusal is one axis/value/reason grammar everywhere, and the corpus entry's roster blocker is discharged.
[PRODUCER_DISTRIBUTIONS]-[COMPLETE]: the census closed over every producer domain the `DOMAINS` roster holds — the geometry charter rows, the compute evidence rows, and the runtime pulse rows — with the instrument kind selected by each producer's declared aggregation intent rather than forced onto one histogram family.
[WORKER_TELEMETRY_PARENTING]-[COMPLETE]: landed as the `WorkerBoot` capture + one `_worker_boot` initializer on `.planning/execution/workers.md` — parented `worker.<name>` span in `traced_kernel`, `WORKER_SIGNAL_PROFILE` geometry through the telemetry injection seams, seal-carried remote-floor boot, daemon spawn-env forwarding, and the exit-owned flush law.
[WORKER_PROFILER_ATTACH]-[COMPLETE]: landed as `_worker_boot`'s profiler arm with the `worker.kind` install tag and kernel-subject `phase` window in `traced_kernel`; `Profiles.install` samples on-CPU Python and GIL-releasing native kernels, `Profiles.phase` supplies the null-window no-op, and `Profiles.receipt` supplies the boot-capture read on `.planning/observability/profiles.md`.
[CROSSING_COST_ATTRIBUTION]-[COMPLETE]: landed as the receipts-minted `Cost` evidence (`sampled`/`delta`/`combined`/`measures`), including signed RSS change without a false peak claim, the two-read `traced_kernel` bracket, the lane drain-window envelope on `DrainReceipt.cost`, and four `rasm.cost.<measure>` rows under `domain="cost"` on `.planning/observability/metrics.md` with the tenant fold riding `_attributed`.
[COMPOSITION_SCOPED_CAPSULE]-[COMPLETE]: already realized on disk — `ScopeKey`/`DEFAULT_SCOPE` mint on receipts, scope-keyed `Hooks._points`/`_taps`/`_rings`, `Metrics._state`/`_receipts`, and the `Telemetry`/`Instrumentation`/`Profiles` receipt maps beside their process latches.
[HISTOGRAM_WIRE_PARITY]-[COMPLETE]: already realized on disk — telemetry `WIRE_AGGREGATION` sets `ExponentialBucketHistogramAggregation` on both metric exporter rows, advisory rows staying the deployment-`View` fallback.
[EGRESS_TRANSPORT_ROW]-[COMPLETE]: already realized on disk — `EgressTransport`, the `SignalProfile.transport` column, SIDECAR-only `GRPC_ELIGIBLE`, per-transport `EGRESS` factory triples, and the HTTP path derivation on telemetry.
[DBAPI_TRAIN_ROW]-[COMPLETE]: already realized on disk — `DbapiSeam`, `_DBAPI_POSTURE`, and the polymorphic `Instrumentation.dbapi` wrap-or-retrofit entry beside `TRAIN` on metrics.
[OBSERVABILITY_SPINE_WAVE]-[COMPLETE]: landed as three observability owners and the metrics deepening — `observability/logging` chain owner, since rebuilt onto the OTLP wire projection with scope-keyed configure custody and a `LogReceipt`, so the stdout ship law that clause named no longer stands; `observability/hooks` scoped registry (`rasm.<pkg>.<domain>.<point>` ids, veto/observe/replay modalities, telemetry taps); `observability/profiles` pyroscope push, benchmark-receipt family, and the offline-job flush envelope; metrics gained `rasm.<domain>.<measure>` wire naming, the tenant baggage dimension, and the composition-root instrumentor train.
[REMOTE_WORKER_DISPATCH]-[COMPLETE]: landed as `WorkerKind.REMOTE` on `execution/workers` — `KIND_POLICY(fidelity=True, restart=Some(RetryClass.SSH))`, the `WorkerPool` remote arm over one `transport/roots` `RemoteEndpoint` channel with `remote_floor` far-side, shm-wire refusal, channel-liveness supervision; roots scope law widened one seam.
[SHARED_MEMORY_CHANNEL]-[COMPLETE]: landed as the `Wire.SHARED_MEMORY` span channel on `execution/workers#CROSSING` — `ShmSpan` named blocks, exporter-owned unlink, worker-side `numpy.frombuffer` reconstruction, ingress-only law.
