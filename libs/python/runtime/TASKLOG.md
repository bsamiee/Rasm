# [PY_RUNTIME_TASKLOG]

Open and closed work for `runtime`, distilled from `IDEAS.md`. Each task card carries a status marker on its leader — `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` when open; `[COMPLETE]` or `[DROPPED]` when closed — and three to four bullets: the design page or `.api/` catalogue to build, the external packages to integrate, the integration points and boundaries/wires, and the key considerations. One idea spawns one or more tasks. A design-complete idea closes here; the downstream source-transcription mode is outside the planning task pool.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — altitude only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <present only on a BLOCKED or gated card; the exact observable that flips it actionable — a catalog row landing, a member query returning evidence, a package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart card — cross-folder as `pkg` `[SLUG]` or a same-folder prerequisite `[SLUG]`, prefixed follows/precedes/mirrors when build order is load-bearing>.
- Atomic: <present only on a minor-scope task; names the small unit so a later session sizes its turn>.
Capability, Shape, Unlocks, and Anchors are required on every open card, Atomic included; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Tasks state landing-grain work decomposing an idea.
-->

[BUNDLE_MOUNT_GATE]-[QUEUED]: daemon boot survives the absent bundle proto rows.
- Capability: serve composition mounts the diagnostic capsule route only when its wire vocabulary resolves — an absent proto row degrades to an unmounted diagnostic, never a failed boot.
- Shape: one availability gate on the `_supervised` capsule mount in `libs/python/runtime/.planning/transport/serve.md` — the `BUNDLE_WIRE` rows probe `PROTO_VOCABULARY` membership before `host.register`, a miss logging the deferral instead of early-returning the boot.
- Unlocks: every daemon boots while `[BUNDLE_PAGE_SPINE]` stays blocked on the C# proto mint; the capsule mounts unchanged the moment the rows land.
- Anchors: `libs/python/runtime/.planning/transport/serve.md` `_supervised`; `libs/python/runtime/.planning/transport/shapes.md` `PROTO_VOCABULARY` and its blocked bundle-proto research row.
- Ripple: precedes `[BUNDLE_PAGE_SPINE]`.
- Atomic: one gate on one mount site.

[SECRET_TIER_ROWS]-[QUEUED]: land the Vault and Azure rows on the secret ladder.
- Capability: two `TierRow` entries with `SecretTier` provider discrimination and `Feature` gates; `NestedSecretsSettingsSource` on the settings source order; the async-vs-sync client ruling for the GCP arm (`SecretManagerServiceAsyncClient` against the held sync-in-thread spelling).
- Shape: two `TierRow` entries with `SecretTier` provider discrimination and `Feature` gates on `SECRET_LADDER` of `libs/python/runtime/.planning/execution/admission.md`, `NestedSecretsSettingsSource` on the settings source order, lazy provider imports deferred behind each gated arm.
- Unlocks: IDEAS.md [SECRET_BACKEND_FAMILY] — deployment-portable secret custody, one admitted boundary across GCP, Vault, and Azure estates with no second resolution surface.
- Anchors: `SECRET_LADDER` on `libs/python/runtime/.planning/execution/admission.md`; `libs/python/runtime/.api/pydantic-settings.md`; `libs/python/runtime/.api/google-cloud-secret-manager.md`; `hvac` and `azure-keyvault-secrets` (catalogs landed under `libs/python/runtime/.api/`).
- Tension: every provider import stays lazy behind its gated arm; `RetryClass.SECRET` is the one transient policy for all rows.

[CACHE_TRANSPORT_LEG]-[QUEUED]: land the RFC-9111 cache leg on the HTTP acquisition.
- Capability: the HTTP acquisition leg revalidates under RFC-9111 semantics, closing the orphaned transport admission into the roots client.
- Shape: `hishel` cache transport wrapping the bare `httpx.AsyncHTTPTransport` construction in `libs/python/runtime/.planning/transport/roots.md`, local file/sqlite store placed on the admitted scratch root.
- Unlocks: IDEAS.md [ACQUISITION_POLICY_SURFACE] — bandwidth-proof repeated artifact acquisition for every sibling consumer.
- Anchors: `TransportResource` HTTP legs on `libs/python/runtime/.planning/transport/roots.md`; the catalog at `libs/python/runtime/.api/hishel.md`; the folder two-cache ruling at `libs/python/runtime/RULINGS.md`.
- Tension: cache store location rides the admitted scratch root, never a hardcoded path.

[ACQUISITION_CUSTODY_ROWS]-[QUEUED]: land proxy, custody, and batch rows on roots.
- Capability: one `httpx` `Proxy` policy row on the HTTP legs, `anyio` `ResourceGuard` custody on mutable root handles, and `fsspec.open_files` batch acquisition.
- Shape: one `httpx` `Proxy` policy row on the HTTP legs, `anyio` `ResourceGuard` custody on the mutable root handles, and `fsspec.open_files` batch acquisition, all on `libs/python/runtime/.planning/transport/roots.md`.
- Unlocks: IDEAS.md [ACQUISITION_POLICY_SURFACE] — enterprise-network egress and race-free root mutation for every sibling consumer, with batched acquisition on one call.
- Anchors: `libs/python/runtime/.planning/transport/roots.md`; `libs/python/runtime/.api/httpx.md`; `libs/python/.api/anyio.md`; `libs/python/.api/fsspec.md`.
- Atomic: three member rows on one page.

[PROBE_MEMBER_PINS]-[QUEUED]: pin the fabric admission gate and channel-evidence members.
- Capability: `wasmtime` `Module.validate` before instantiation on the WASM arm as a typed `config` refusal; `psutil` `Process.net_connections` evidence columns on REMOTE/DAEMON supervision verdicts.
- Shape: `wasmtime` `Module.validate` as the guest-admission gate before instantiation on the WASM arm, and `psutil` `Process.net_connections` evidence columns on REMOTE/DAEMON supervision verdicts, both on `libs/python/runtime/.planning/execution/workers.md`.
- Unlocks: IDEAS.md [FABRIC_ADMISSION_PROBES] — a malformed guest refuses at admission with a typed `config` fault instead of an instantiation trap, and supervision verdicts distinguish a dead channel from a saturated one.
- Anchors: the guest arm and `Supervisor` on `libs/python/runtime/.planning/execution/workers.md`; `libs/python/runtime/.api/wasmtime.md`; `libs/python/.api/psutil.md`.
- Atomic: one gate and two verdict columns.

[FAULT_SPELLING_SETS]-[QUEUED]: provider-exception spellings single-source at the faults tier.
- Capability: one canonical owner for the module-qualified provider-exception name sets both reliability tables match on, so a provider rename edits one surface.
- Shape: `libs/python/runtime/.planning/reliability/faults.md` exports the pool-death and ssh terminal/transient `frozenset[str]` name sets beside its classify table; the policy rows of `libs/python/runtime/.planning/reliability/resilience.md` import them, DAG-legal since resilience already imports faults.
- Unlocks: fault classification and retry policy stay spelling-aligned by construction across loky, pebble, and asyncssh renames.
- Anchors: the faults classify frozensets; the resilience transient/refuse rows; the shared dotted-spelling convention both pages note without a shared owner.
- Atomic: set exports and two import substitutions.

[CLOCK_FOLD_MOVE]-[QUEUED]: move the clock page into evidence and re-anchor the census.
- Capability: the clock owner folds into the evidence identity band with zero law change — a pure page-and-module move whose consumers re-import the same symbols.
- Shape: `libs/python/runtime/.planning/clock/clock.md` relocated to `libs/python/runtime/.planning/evidence/clock.md` with the module spelled inside the evidence namespace, router/codemap/seam-fence rosters and the import rail re-anchored on `libs/python/runtime/README.md` and `libs/python/runtime/ARCHITECTURE.md`.
- Unlocks: IDEAS.md [CLOCK_IDENTITY_FOLD] — evidence closes at four sibling pages and the folder census carries no unearned sub-folder.
- Anchors: `libs/python/runtime/README.md` router; `libs/python/runtime/ARCHITECTURE.md` codemap and import rail.
- Atomic: one page move and index re-anchoring.

[GEOMETRY_MEASURE_CHARTER]-[QUEUED]: land the charter-named geometry instrument rows.
- Capability: every charter-named geometry measure records through the one metrics table with zero entrypoint edits.
- Shape: one `InstrumentSpec` row with its `SyncInstruments` field per charter-named measure on `libs/python/runtime/.planning/observability/metrics.md`, reaching `record`'s mapping arm through `_DOMAIN_SLOT`.
- Unlocks: IDEAS.md [PRODUCER_DISTRIBUTIONS] — geometry-domain dashboards aggregate distributions without receipt post-processing.
- Anchors: the `INSTRUMENTS` growth law and `_DOMAIN_SLOT` derivation; the landed charter table on `libs/python/geometry/.planning/graduation.md` with exact measure names, units, and aggregations.
- Tension: a row lands only for a measure the charter table spells — nothing beyond its roster.
- Ripple: `geometry` `[PRODUCER_DISTRIBUTIONS]`.

[PULSE_INSTRUMENT_ROWS]-[QUEUED]: the pulse drop telemetry gains its instrument rows.
- Capability: authorized lossy-drop accounting records through the one metrics table instead of raising `KeyError` inside the pulse-drain actor.
- Shape: two `InstrumentSpec` rows with their `SyncInstruments` fields — `rasm.runtime.pulse.dropped` and `rasm.runtime.pulse.rejected` under `domain="runtime"` — on `libs/python/runtime/.planning/observability/metrics.md`, matching the two `Metrics.record` calls the lanes page already makes.
- Unlocks: the telemetry-never-faults-a-kernel guarantee holds on the authorized drop paths; the pulse-drain actor survives its own accounting.
- Anchors: the lanes pulse-drain recordings on `libs/python/runtime/.planning/execution/lanes.md`; the `INSTRUMENTS` growth law and `_DOMAIN_SLOT` derivation.
- Atomic: two instrument rows and their fields.

[BENCH_MODE_MINT]-[QUEUED]: the bench owner mints the mode its consumers already import.
- Capability: `BenchMode` and mode-carrying run and receipt signatures exist at the one bench owner, so three sibling import sites resolve as written.
- Shape: `BenchMode` minted on `libs/python/runtime/.planning/observability/profiles.md` with the `mode` parameter on `Bench.run` and the mode field and parameter on `BenchmarkReceipt`/`BenchmarkReceipt.of`, matching the call shapes the artifacts bench corpus, compute study fold, and data query bench already spell.
- Unlocks: the three consumer pages compile as written; bench evidence carries its mode.
- Anchors: the three importing call sites on `libs/python/artifacts/.planning/core/bench.md`, `libs/python/compute/.planning/experiments/study.md`, and `libs/python/data/.planning/tabular/query.md`; the single-writer bench authority.
- Atomic: one enum and two signature widenings.

[HOOKS_BATCH_FOLD]-[QUEUED]: the hook registry owns the batch register and tap fold.
- Capability: registering and tapping a package point-table is one substrate call, collapsing four folder-local fold idioms.
- Shape: `Hooks.register_all(points, scope, disposition)` and `Hooks.tap_all` on `libs/python/runtime/.planning/observability/hooks.md`; the artifacts, compute, data, and geometry registration folds become one-call composes at landing.
- Unlocks: one fold semantics for point registration; a disposition change lands once.
- Anchors: the four folder registration folds; `Hooks.register`/`tap_receipts`/`tap_metrics`.
- Atomic: two registry members.

[COMPUTE_INSTRUMENT_ROWS]-[QUEUED]: compute's evidence measures gain their instrument rows.
- Capability: compute's graduation evidence records through the one metrics table instead of faulting on an unregistered domain.
- Shape: four `InstrumentSpec` rows with their `SyncInstruments` fields under `domain="compute"` on `libs/python/runtime/.planning/observability/metrics.md` — `rasm.compute.evidence.duration`, `rasm.compute.evidence.cpu_time`, `rasm.compute.evidence.rss_growth`, `rasm.compute.graduation.residual_count` — matching the record calls the compute observability page already makes.
- Unlocks: compute-domain dashboards aggregate without receipt post-processing; the compute hook-rail deferral closes.
- Anchors: the compute measure ids on `libs/python/compute/.planning/graduation/observability.md`; the `INSTRUMENTS` growth law and `_DOMAIN_SLOT` derivation; the record-and-row same-pass law at `libs/python/runtime/RULINGS.md`.
- Tension: the evidence-cost tail vocabulary — geometry `cpu`/`rss_delta` versus compute `cpu_time`/`rss_growth` for one measure family — resolves to one spelling at landing.
- Atomic: four instrument rows and their fields.

[SERVICE_NAME_ROWS]-[QUEUED]: proto service names home once and the boot gate proves them.
- Capability: the wire's service identifiers live on the transport vocabulary with drift-gate proof, so a host-side service rename surfaces at boot instead of a dead dial.
- Shape: service-name constants beside `PROTO_VOCABULARY` on `libs/python/runtime/.planning/transport/shapes.md`, the `aligned` descriptor-pool gate widened to prove them, and the hard-coded literals on `libs/python/geometry/.planning/mesh/serve.md` swapped for the imported constants.
- Unlocks: the last unguarded wire spelling joins the drift-gated vocabulary.
- Anchors: `PROTO_VOCABULARY` and `aligned` on the shapes page; the two service literals on geometry serve.
- Atomic: two constants, one gate widening, two import swaps.

[MEASURED_NAME_SPLIT]-[QUEUED]: one canonical name per evidence weave.
- Capability: the exported span/fault/receipt weave keeps the `measured` name alone — the serve-handler duration aspect carries its own name, so one spelling never binds two bounded concepts.
- Shape: the `Metrics` serve-timing classmethod on `libs/python/runtime/.planning/observability/metrics.md` renames to `Metrics.timed` with its call sites; the receipts weave keeps `measured` unchanged.
- Unlocks: grep-true seam census across the branch — every `measured` hit is the evidence weave.
- Anchors: the receipts weave owner on `libs/python/runtime/.planning/observability/receipts.md`; the one-canonical-name-per-concept law.
- Atomic: one rename and its call sites.

[SCOPE_GRAMMAR_GUARD]-[QUEUED]: the measured weave refuses off-grammar scopes.
- Capability: the exported span weave admits only `rasm.`-rooted scopes, so a bare package-prefixed scope refuses at the seam instead of forking the branch namespace.
- Shape: one `HOOK_ID`-style structural refusal on the free `scope` parameter of the `measured` weave in `libs/python/runtime/.planning/observability/receipts.md`.
- Unlocks: the folder scope-grammar ruling made structural at the one ingress every sibling composes.
- Anchors: the folder scope-grammar ruling at `libs/python/runtime/RULINGS.md`; the `HOOK_ID` regex on `libs/python/runtime/.planning/observability/hooks.md`.
- Ripple: mirrors `compute` `[EVIDENCE_SCOPE_GRAMMAR]`.
- Atomic: one refusal on one parameter.

[HLC_HEADER_DRIFT_GATE]-[BLOCKED]: the HLC carrier headers join the boot-proved wire vocabulary.
- Capability: the four `SLOTS` carrier keys prove against the C#-minted header contract at boot, so a host-side header rename surfaces as a gate failure instead of a silently-defaulted clock forking the causal order.
- Shape: one boot-time assertion beside `aligned` in the serve boot fold proving the `SLOTS` keys of `libs/python/runtime/.planning/clock/clock.md` against the host header contract; `CausalFrame.decode`'s absent-slot defaults stay, guarded upstream by the gate.
- Unlocks: the HLC header family stops being an unguarded hand mirror — a causal-order fork dies at boot, never in silently-zeroed stamps.
- Anchors: `SLOTS` and `CausalFrame.decode` on `libs/python/runtime/.planning/clock/clock.md`; the `aligned` descriptor gate on `libs/python/runtime/.planning/transport/shapes.md`; `libs/csharp/Rasm.AppHost/.planning/Observability/telemetry.md` `[CORRELATION_SPINE]`.
- Arms: the C# correlation spine spells the four carrier header keys as its minted contract.
- Ripple: `csharp Rasm.AppHost` `[HLC_HEADER_KEY_MINT]` — follows.
- Atomic: one gate row on one boot fold.

[BUNDLE_PAGE_SPINE]-[BLOCKED]: the bundle spine settles end to end over the restored wire vocabulary.
- Capability: capsule page, scope-correct replay, archive fold, and diagnostic `Route` — already landed — complete with the proto vocabulary restored; drives from IDEAS `[SUPPORT_BUNDLE_CAPSULE]`.
- Shape: the two `PROTO_VOCABULARY` rows restored on `libs/python/runtime/.planning/transport/shapes.md`.
- Unlocks: IDEAS.md [SUPPORT_BUNDLE_CAPSULE] — support-bundle capture crosses the runtime/Compute wire typed.
- Anchors: the landed capsule page, replay, and archive fold; `rasm.runtime._pb2.channels_pb2`.
- Arms: `SupportBundleRequest`, `SupportBundleReply`, and `CaptureBundle` minted at `libs/csharp/Rasm.Compute/.planning/Runtime/wire.md`, then `rasm.runtime._pb2.channels_pb2` regenerated.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[WORKER_INSTALL_SEAM]-[COMPLETE]: landed on `.planning/execution/workers.md` — `WorkerBoot.captured` off `Telemetry.receipt`/`Profiles.receipt`, one `_worker_boot` initializer on every process arm, `WORKER_SIGNAL_PROFILE` geometry, seal-carried remote-floor boot, daemon spawn-env forwarding, and the parented kernel span in `traced_kernel`; graceful settle and roll drain through atexit, and named kill paths bound their forfeited tail to one worker export window.
[PROFILER_ATTACH_ROWS]-[COMPLETE]: landed as `_worker_boot`'s profiler arm (`worker.kind` tag, captured tenant) and the kernel-subject `phase` window in `traced_kernel`; `Profiles.install` samples GIL-releasing native kernels, and the `Profiles.phase` null-window and `Profiles.receipt` landed on `.planning/observability/profiles.md`.
[COST_DELTA_COLUMNS]-[COMPLETE]: landed as the receipts-minted `Cost` (`sampled`/`delta`/`combined`/`facts`/`measures`, signed RSS change, platform-gated `io_counters`), the two-read `traced_kernel` bracket, `DrainReceipt.cost` with the lane drain-window envelope, and the drained-line cost facts.
[COST_INSTRUMENT_ROWS]-[COMPLETE]: landed as four `rasm.cost.<measure>` histogram rows under `domain="cost"` with their `SyncInstruments` fields on `.planning/observability/metrics.md`; tenant joins through the standing `_attributed` fold.
[SCOPE_KEY_MAP]-[COMPLETE]: already realized on disk — the receipts-minted `ScopeKey`/`DEFAULT_SCOPE` threads `Hooks._points`/`_taps`/`_rings`, `Metrics._state`/`_receipts`, `Telemetry._receipts`, `Instrumentation._receipts`, `Profiles._receipts`, and the `_sink` default-scope resolution.
[EXP_AGGREGATION_ROW]-[COMPLETE]: already realized on disk — telemetry `WIRE_AGGREGATION` carries `ExponentialBucketHistogramAggregation` on both metric exporter rows, metrics advisory rows ruled the deployment-`View` fallback.
[DBAPI_WRAP_SEAM]-[COMPLETE]: already realized on disk — `Instrumentation.dbapi` calls the catalog spellings `wrap_connect`/`instrument_connection` through one `DbapiSeam` discriminator under `_DBAPI_POSTURE`.
[EGRESS_TRANSPORT_TABLE]-[COMPLETE]: already realized on disk — telemetry `EgressTransport`, the `SignalProfile.transport` column, `GRPC_ELIGIBLE`, endpoint derivation, and both `EGRESS` factory triples.
[OBSERVABILITY_PAGES]-[COMPLETE]: landed in `.planning/observability/{logging,hooks,profiles}.md` with the `metrics.md`/`telemetry.md`/`receipts.md` deepening — chain ownership moved to the logging page, `SCHEMA_URL` pin and `resource`/`signal_profile`/`ship` injection seams on the telemetry install, instrument names renamed to `rasm.*`, query/geometry/bench domain rows added, and the instrumentation train — `opentelemetry-instrumentation-jinja2` (artifacts template render/compile/load spans), `opentelemetry-instrumentation-system-metrics` (`_SYSTEM_SLICE`: `system.*` and `cpython.gc.*` alone, process family stays on `rasm.process.*`), `opentelemetry-instrumentation-threading` (cross-thread context propagation) — composed on `metrics.md` `[02]-[INSTRUMENTATION]` and catalogued under `.api/`.
[REMOTE_KIND_ROW]-[COMPLETE]: landed in `.planning/execution/workers.md` — `WorkerKind.REMOTE` + `KIND_POLICY` SSH restart row, `WorkerPool` remote arm sealing the kernel over `asyncssh` `create_process`, `remote_floor` entry, Supervisor channel probe; `transport/roots` scope law amended with the `RemoteEndpoint` dial owner.
[SHM_CHANNEL_OWNER]-[COMPLETE]: landed in `.planning/execution/workers.md` — `ShmSpan` + `exported`/`released` on the fabric, decode inside `shipped`, exporter-owned unlink after the offload settles.
