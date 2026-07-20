# [PY_RUNTIME_TASKLOG]

Open and closed work for `runtime`, distilled from `IDEAS.md`. Each task card carries a status marker on its leader — `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` when open; `[COMPLETE]` or `[DROPPED]` when closed — and three to four bullets: the design page or `.api/` catalogue to build, the external packages to integrate, the integration points and boundaries/wires, and the key considerations. One idea spawns one or more tasks. A design-complete idea closes here; the downstream source-transcription mode is outside the planning task pool.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
- Atomic: <present only on a minor-scope task; one short phrase naming the small unit so a later session does not overscope its turn>.
-->

[WORKER_INSTALL_SEAM]-[QUEUED]: pin the worker-side telemetry install seam per pool arm.
- Capability: per-arm bootstrap rows — loky initializer, pebble initializer, daemon spawn environment, `remote_floor` far side — each delegating to `Telemetry.install` with a worker-shaped `SignalProfile`; flush law at settle, roll, and retire; `shipped` opens the kernel span under the resolved carrier.
- Anchors: initializer rows and `shipped` on `libs/python/runtime/.planning/execution/workers.md`; `resource=`/`signal_profile=` seams on `libs/python/runtime/.planning/observability/telemetry.md`.
- Tension: fork-inherited batch threads are dead — install runs post-spawn per worker process; the gRPC egress row stays refused in forking pools.

[PROFILER_ATTACH_ROWS]-[QUEUED]: pin the per-arm profiler attach rows and the subject tag vocabulary.
- Capability: agent configure, subject-tag, and shutdown rows per pool initializer arm on `libs/python/runtime/.planning/execution/workers.md`; tag vocabulary and span-profile correlation law on `libs/python/runtime/.planning/observability/profiles.md`.
- Anchors: `pyroscope-io`; the `[WORKER_INSTALL_SEAM]` bootstrap rows; idea `[WORKER_PROFILER_ATTACH]`.
- Ripple: `geometry` `[KERNEL_PROFILE_EVIDENCE]`.

[COST_DELTA_COLUMNS]-[QUEUED]: pin the cost capture brackets and evidence columns.
- Capability: `psutil` `Process.cpu_times`/`memory_info`/`num_ctx_switches` delta brackets around `shipped` and the pooled settle — context-switch deltas split contended kernels from starved ones; cost columns on the drain evidence; tenant key resolved off `TENANT_BAGGAGE`.
- Anchors: `libs/python/runtime/.planning/execution/workers.md`; the `DRAIN_COLUMNS` no-drift law on `libs/python/runtime/.planning/observability/receipts.md`; `libs/python/.api/psutil.md`.
- Tension: capture stays bracket-cheap — two process reads per crossing, never a sampling loop.

[COST_INSTRUMENT_ROWS]-[QUEUED]: land `rasm.cost.<measure>` instrument rows with the tenant fold.
- Capability: cost histogram rows on the one `INSTRUMENTS` table reaching `record`'s mapping arm; the `rasm.tenant` attribute joins through the existing fold.
- Anchors: the `INSTRUMENTS` growth law on `libs/python/runtime/.planning/observability/metrics.md`.
- Atomic: two instrument rows and one attribute fold.

[SCOPE_KEY_MAP]-[QUEUED]: map every class-held singleton to the composition-scope key.
- Capability: census of `Hooks._points`/`_taps`/`_rings`, the `Metrics` state carrier, `Telemetry._receipt`/`_installed`, `Instrumentation._receipt`, `Profiles._receipt`, and the receipts default-sink resolution; pin the scope-key shape and the default-scope spelling.
- Anchors: `libs/python/runtime/.planning/observability/hooks.md`; `libs/python/runtime/.planning/observability/metrics.md`; `libs/python/runtime/.planning/observability/telemetry.md`; `libs/python/runtime/.planning/observability/profiles.md`; `libs/python/runtime/.planning/observability/receipts.md`.
- Tension: OTel provider globals stay process-wide — the capsule partitions custody, never the exporter pipeline.

[EXP_AGGREGATION_ROW]-[QUEUED]: wire the exponential default aggregation on the meter build.
- Capability: `ExponentialBucketHistogramAggregation` as the histogram default at meter construction, with the advisory rows ruled the explicit-shape fallback.
- Anchors: `_meter_provider` on `libs/python/runtime/.planning/observability/telemetry.md`; `libs/python/.api/opentelemetry-sdk.md`.
- Atomic: one aggregation row and one fallback ruling.

[DBAPI_WRAP_SEAM]-[QUEUED]: pin the generic DBAPI wrap seam and its member spellings.
- Capability: a connection-factory registration the composition root activates for duckdb and ADBC DBAPI legs; exact wrap-member spellings verified against the admission-landed catalog `libs/python/runtime/.api/opentelemetry-instrumentation-dbapi.md`.
- Anchors: `TRAIN` on `libs/python/runtime/.planning/observability/metrics.md`; `opentelemetry-instrumentation-dbapi` (catalog landed at `libs/python/runtime/.api/opentelemetry-instrumentation-dbapi.md`).
- Tension: no import-time patching of modules this folder does not admit — the wrap is per-connection, threaded by the data-side consumer.

[SECRET_TIER_ROWS]-[QUEUED]: land the Vault and Azure rows on the secret ladder.
- Capability: two `TierRow` entries with `SecretTier` provider discrimination and `Feature` gates; `NestedSecretsSettingsSource` on the settings source order; the async-vs-sync client ruling for the GCP arm (`SecretManagerServiceAsyncClient` against the held sync-in-thread spelling).
- Anchors: `SECRET_LADDER` on `libs/python/runtime/.planning/execution/admission.md`; `libs/python/runtime/.api/pydantic-settings.md`; `libs/python/runtime/.api/google-cloud-secret-manager.md`; `hvac` and `azure-keyvault-secrets` (catalogs landed under `libs/python/runtime/.api/`).
- Tension: every provider import stays lazy behind its gated arm; `RetryClass.SECRET` is the one transient policy for all rows.

[CACHE_TRANSPORT_LEG]-[QUEUED]: land the RFC-9111 cache leg on the HTTP acquisition.
- Capability: `hishel` cache transport over the httpx legs with local file/sqlite store placement; revalidation law stated beside the content-keyed lanes elision so the two owners never overlap.
- Anchors: `TransportResource` HTTP legs on `libs/python/runtime/.planning/transport/roots.md`; `hishel` (catalog landed at `libs/python/runtime/.api/hishel.md`).
- Tension: cache store location rides the admitted scratch root, never a hardcoded path.

[ACQUISITION_CUSTODY_ROWS]-[QUEUED]: land proxy, custody, and batch rows on roots.
- Capability: one `httpx` `Proxy` policy row on the HTTP legs, `anyio` `ResourceGuard` custody on mutable root handles, and `fsspec.open_files` batch acquisition.
- Anchors: `libs/python/runtime/.planning/transport/roots.md`; `libs/python/runtime/.api/httpx.md`; `libs/python/.api/anyio.md`; `libs/python/.api/fsspec.md`.
- Atomic: three member rows on one page.

[PROBE_MEMBER_PINS]-[QUEUED]: pin the fabric admission gate and channel-evidence members.
- Capability: `wasmtime` `Module.validate` before instantiation on the WASM arm as a typed `config` refusal; `psutil` `Process.net_connections` evidence columns on REMOTE/DAEMON supervision verdicts.
- Anchors: the guest arm and `Supervisor` on `libs/python/runtime/.planning/execution/workers.md`; `libs/python/runtime/.api/wasmtime.md`; `libs/python/.api/psutil.md`.
- Atomic: one gate and two verdict columns.

[BUNDLE_PAGE_SPINE]-[QUEUED]: author the bundle owner page and its serve route.
- Capability: `libs/python/runtime/.planning/observability/bundle.md` — one collectors table (faulthandler dump, gated tracemalloc snapshot, hook replay window, install/train/profile receipts, admitted-context render, supervision verdicts), one content-keyed archive fold, one serve `Route` row.
- Anchors: the `Route` growth law on `libs/python/runtime/.planning/transport/serve.md`; REPLAY rings on `libs/python/runtime/.planning/observability/hooks.md`; `ContentKey` on `libs/python/runtime/.planning/evidence/identity.md`.
- Tension: every collector is pull-driven and bounded; redaction rides the receipts-owned `Redaction` before any archive byte lands.

[CLOCK_FOLD_MOVE]-[QUEUED]: move the clock page into evidence and re-anchor the census.
- Capability: `libs/python/runtime/.planning/clock/clock.md` relocates to `libs/python/runtime/.planning/evidence/clock.md`; router, codemap, seam-fence rosters, and the import rail re-anchor; the module-spelling ruling lands with the move.
- Anchors: `libs/python/runtime/README.md` router; `libs/python/runtime/ARCHITECTURE.md` codemap and import rail.
- Atomic: one page move and index re-anchoring.

[EGRESS_TRANSPORT_TABLE]-[QUEUED]: land the egress-transport axis on the telemetry install.
- Capability: `EgressTransport` vocabulary and per-transport exporter-factory rows on `libs/python/runtime/.planning/observability/telemetry.md`, http the standing value on every profile row.
- Anchors: `opentelemetry-exporter-otlp-proto-grpc` beside `opentelemetry-exporter-otlp-proto-http`; the `SIGNAL_SPECS` exporter seam.
- Tension: SIDECAR-only gRPC eligibility; fork-hazard fence on every worker-forking profile; compression vocabularies stay per-transport.
- Atomic: one vocabulary, one `SignalProfile` column, factory rows.

[GEOMETRY_MEASURE_CHARTER]-[BLOCKED]: resolve which geometry measures the dashboard charter names.
- Capability: the answerable question gating `[PRODUCER_DISTRIBUTIONS]` — each charter-named measure lands as one `InstrumentSpec` row with its `SyncInstruments` field on `libs/python/runtime/.planning/observability/metrics.md`.
- Anchors: the dashboard-compile charter on `libs/typescript/iac/.planning/operate/observe.md`; the `INSTRUMENTS` growth law and `_DOMAIN_SLOT` derivation.
- Tension: receipts stay the data plane by ruling — a row lands only for a measure the charter names.
- Ripple: `geometry` `[PRODUCER_DISTRIBUTIONS]`.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

[OBSERVABILITY_PAGES]-[COMPLETE]: landed in `.planning/observability/{logging,hooks,profiles}.md` with the `metrics.md`/`telemetry.md`/`receipts.md` deepening — chain ownership moved to the logging page, `SCHEMA_URL` pin and `resource`/`signal_profile`/`ship` injection seams on the telemetry install, instrument names renamed to `rasm.*`, query/geometry/bench domain rows added, and the instrumentation train — `opentelemetry-instrumentation-jinja2` (artifacts template render/compile/load spans), `opentelemetry-instrumentation-system-metrics` (`_SYSTEM_SLICE`: `system.*` and `cpython.gc.*` alone, process family stays on `rasm.process.*`), `opentelemetry-instrumentation-threading` (cross-thread context propagation) — composed on `metrics.md` `[02]-[INSTRUMENTATION]` and catalogued under `.api/`.

[REMOTE_KIND_ROW]-[COMPLETE]: landed in `.planning/execution/workers.md` — `WorkerKind.REMOTE` + `KIND_POLICY` SSH restart row, `WorkerPool` remote arm sealing the kernel over `asyncssh` `create_process`, `remote_floor` entry, Supervisor channel probe; `transport/roots` scope law amended with the `RemoteEndpoint` dial owner.

[SHM_CHANNEL_OWNER]-[COMPLETE]: landed in `.planning/execution/workers.md` — `ShmSpan` + `exported`/`released` on the fabric, decode inside `shipped`, exporter-owned unlink after the offload settles.
