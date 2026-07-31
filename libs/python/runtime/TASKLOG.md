# [PY_RUNTIME_TASKLOG]

Open and closed work for `runtime`, distilled from `IDEAS.md`. Each task card carries a status marker on its leader — `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` when open; `[COMPLETE]` or `[DROPPED]` when closed — and three to four bullets: the design page or `.api/` catalogue to build, the external packages to integrate, the integration points and boundaries/wires, and the key considerations. One idea spawns one or more tasks. Design-complete ideas close here; the downstream source-transcription mode is outside the planning task pool.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — concept grain only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <BLOCKED or gated cards only; the exact observable flipping it actionable — catalog row landing, member query evidence, package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart — cross-folder `pkg` `[SLUG]`, same-folder prerequisite `[SLUG]`; load-bearing build order prefixes follows/precedes/mirrors>.
- Atomic: <present only on a minor-scope task; names the small unit so a later session sizes its turn>.
Capability, Shape, Unlocks, and Anchors are required on every open card, Atomic included; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Tasks state landing-grain work decomposing an idea.
-->

[SECRET_TIER_ROWS]-[QUEUED]: land the Vault and Azure rows on the secret ladder.
- Capability: two `TierRow` entries with `SecretTier` provider discrimination and `Feature` gates; `NestedSecretsSettingsSource` on the settings source order; the async-vs-sync client ruling for the GCP arm (`SecretManagerServiceAsyncClient` against the held sync-in-thread spelling).
- Shape: two `TierRow` entries with `SecretTier` provider discrimination and `Feature` gates on `SECRET_LADDER` of `libs/python/runtime/.planning/execution/admission.md`, `NestedSecretsSettingsSource` on the settings source order, lazy provider imports deferred behind each gated arm.
- Unlocks: IDEAS.md [SECRET_BACKEND_FAMILY] — deployment-portable secret custody, one admitted boundary across GCP, Vault, and Azure estates with no second resolution surface.
- Anchors: `SECRET_LADDER` on `libs/python/runtime/.planning/execution/admission.md`; `libs/python/runtime/.api/pydantic-settings.md`; `libs/python/runtime/.api/google-cloud-secret-manager.md`; `hvac` and `azure-keyvault-secrets` (catalogs landed under `libs/python/runtime/.api/`).
- Tension: every provider import stays lazy behind its gated arm; `RetryClass.SECRET` is the one transient policy for all rows.

[ACQUISITION_CUSTODY_ROWS]-[QUEUED]: land proxy, custody, and batch rows on roots.
- Capability: one `httpx` `Proxy` policy row on the HTTP legs, `anyio` `ResourceGuard` custody on mutable root handles, and `fsspec.open_files` batch acquisition.
- Shape: one `httpx` `Proxy` policy row on the HTTP legs, `anyio` `ResourceGuard` custody on the mutable root handles, and `fsspec.open_files` batch acquisition, all on `libs/python/runtime/.planning/transport/roots.md`.
- Unlocks: IDEAS.md [ACQUISITION_POLICY_SURFACE] — enterprise-network egress and race-free root mutation for every sibling consumer, with batched acquisition on one call.
- Anchors: `libs/python/runtime/.planning/transport/roots.md`; `libs/python/runtime/.api/httpx.md`; `libs/python/.api/anyio.md`; `libs/python/.api/fsspec.md`.
- Atomic: three member rows on one page.

[PROBE_MEMBER_PINS]-[QUEUED]: pin the crossing admission gate and channel-evidence members.
- Capability: `wasmtime` `Module.validate` before instantiation on the WASM arm as a typed `config` refusal; `psutil` `Process.net_connections` evidence columns on REMOTE/DAEMON supervision verdicts.
- Shape: `wasmtime` `Module.validate` as the guest-admission gate before instantiation on the WASM arm, and `psutil` `Process.net_connections` evidence columns on REMOTE/DAEMON supervision verdicts, both on `libs/python/runtime/.planning/execution/workers.md`.
- Unlocks: IDEAS.md [CROSSING_ADMISSION_PROBES] — a malformed guest refuses at admission with a typed `config` fault instead of an instantiation trap, and supervision verdicts distinguish a dead channel from a saturated one.
- Anchors: the guest arm and `Supervisor` on `libs/python/runtime/.planning/execution/workers.md`; `libs/python/runtime/.api/wasmtime.md`; `libs/python/.api/psutil.md`.
- Atomic: one gate and two verdict columns.

[CLOCK_FOLD_MOVE]-[QUEUED]: move the clock page into evidence and re-anchor the census.
- Capability: the clock owner folds into the evidence identity band with zero law change — a pure page-and-module move whose consumers re-import the same symbols.
- Shape: `libs/python/runtime/.planning/clock/clock.md` relocated to `libs/python/runtime/.planning/evidence/clock.md` with the module spelled inside the evidence namespace, router/codemap/seam-fence rosters and the import rail re-anchored on `libs/python/runtime/README.md` and `libs/python/runtime/ARCHITECTURE.md`.
- Unlocks: IDEAS.md [CLOCK_IDENTITY_FOLD] — evidence closes at four sibling pages and the folder census carries no unearned sub-folder.
- Anchors: `libs/python/runtime/README.md` router; `libs/python/runtime/ARCHITECTURE.md` codemap and import rail.
- Atomic: one page move and index re-anchoring.

[SERVICE_NAME_ROWS]-[QUEUED]: proto service names home once and the boot gate proves them.
- Capability: the wire's service identifiers live on the transport vocabulary with drift-gate proof, so a host-side service rename surfaces at boot instead of a dead dial.
- Shape: service-name constants beside `PROTO_VOCABULARY` on `libs/python/runtime/.planning/transport/shapes.md`, the `aligned` descriptor-pool gate widened to prove them, and the hard-coded literals on `libs/python/geometry/.planning/mesh/serve.md` swapped for the imported constants.
- Unlocks: the last unguarded wire spelling joins the drift-gated vocabulary.
- Anchors: `PROTO_VOCABULARY` and `aligned` on the shapes page; the two service literals on geometry serve.
- Atomic: two constants, one gate widening, two import swaps.

[BAND_PROBE_REGISTRATION]-[QUEUED]: the two process-wide bands join the occupancy series.
- Capability: `THREAD_BAND` and `WORKER_BAND` report live saturation on the band series every scoped limiter already reaches, so an operator reads thread-hop and pooled-crossing pressure without inferring either from latency.
- Shape: one registration site at the composition root threading each module-scope limiter's `borrowed_tokens` read into `Metrics.occupied` under `band="thread"` and `band="pool"`, on `libs/python/runtime/.planning/execution/lanes.md` and `libs/python/runtime/.planning/execution/workers.md`.
- Unlocks: IDEAS.md [BOUNDED_OCCUPANCY_ROSTER] — every named band the branch declares reports, not only the ones carrying a scoped lifetime.
- Anchors: `Metrics.occupied` band keying and `rasm.band.in_flight` on `libs/python/runtime/.planning/observability/metrics.md`; `THREAD_BAND` at `execution/lanes#LANE`; `WORKER_BAND` at `execution/workers#POOL`.
- Tension: both limiters are module-scope constants carrying no lifetime a context manager brackets, and the metrics owner imports no `anyio` and names no limiter — so the registration seats at the composition root, never at either band's own module top, which fires at import against the eager-install ban.

[LOKY_TRACKER_FORMAT_SKEW]-[BLOCKED]: pooled crossings stop flooding stderr with tracker parse failures.
- Capability: the warm pool's leaked semaphores reclaim automatically again and a long-lived daemon spawning pools stops writing unbounded parse tracebacks past the whole handler roster from a child process no terminal door reaches.
- Shape: one `execution/workers` note beside the loky arm and the `loky` manifest floor at `pyproject.toml`, both landing with the upstream release.
- Unlocks: pooled crossings leave no unreclaimed semaphore and no child-process stderr flood on the CPython 3.15 floor.
- Anchors: `WorkerKind.HOSTILE` loky arm at `execution/workers#POOL`; `libs/python/.api/loky.md`.
- Arms: a loky release whose vendored `backend/resource_tracker.main` reads the JSON record format; verified on disk — the interpreter rewrote `multiprocessing.resource_tracker` onto `json.dumps`/`json.loads`, the admitted loky subclasses that writer while its own tracker process still colon-splits the line, so every register and unregister raises `ValueError: unknown resource type` in the tracker. Executor semantics stand unaffected: a live `get_reusable_executor` submit, result, and shutdown all return clean.
- Tension: pinning loky buys nothing — the skew rides the interpreter's own format change, so only an upstream reader adopting JSON closes it.

[HLC_HEADER_DRIFT_GATE]-[BLOCKED]: the HLC carrier headers join the boot-proved wire vocabulary.
- Capability: the four `SLOTS` keys prove against the shared header contract at boot, so schema drift fails before causal admission.
- Shape: one boot-time assertion beside `aligned` in the serve boot fold proving the `SLOTS` keys of `libs/python/runtime/.planning/clock/clock.md` against the host header contract; `CausalFrame.decode`'s absent-slot defaults stay, guarded upstream by the gate.
- Unlocks: the HLC header family stops being an unguarded hand mirror — a causal-order fork dies at boot, never in silently-zeroed stamps.
- Anchors: `SLOTS` and `CausalFrame.decode` on `libs/python/runtime/.planning/clock/clock.md`; the `aligned` descriptor gate on `libs/python/runtime/.planning/transport/shapes.md`; `libs/csharp/Rasm.AppHost/.planning/Observability/telemetry.md` `[CORRELATION_SPINE]`.
- Arms: the C# correlation spine spells the four `SLOTS` carrier header keys as its minted contract; verified absent — `csharp:Rasm.AppHost/Observability/telemetry#CORRELATION_SPINE` carries the two-half stamp as `HlcStampWire` on the RECEIPT ENVELOPE and names no carrier header, so `rasm-hlc-physical`, `rasm-hlc-logical`, `rasm-tenant`, and `rasm-hlc` have no counterpart to gate against and the blocker stands.
- Ripple: `csharp Rasm.AppHost` `[HLC_HEADER_KEY_MINT]` — follows.
- Atomic: one gate row on one boot fold.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[HOOKS_TAP_FOLD]-[COMPLETE]: landed on `.planning/observability/hooks.md` as a POLYMORPHIC `Hooks.subscribe` rather than the card's `tap_all` sibling — a second roster-grain entry beside `subscribe` is the prefixed-sibling shape the collapse scan deletes, so subscription now folds arity off the request value exactly as `register` does (one point id or one claimed `Block`), the per-point attach retires to `_subscribed` returning its ATTACHED MEMBER so a roster unwind detaches the REPLAY barrier rather than the caller's tap, and the roster arm is transactional by unwind because a REPLAY row's retained drain runs outside the registry gate and no swap covers it; the serve boot leg's per-point `traversed` fold is gone.
[BUNDLE_PAGE_SPINE]-[COMPLETE]: recorded blocker refuted on disk — the C# mint already carries the `Diagnostic`/`CaptureBundle` row and binds both messages onto this vocabulary, so `support_bundle` and `support_bundle_reply` landed on `.planning/transport/shapes.md` and `aligned` now proves them against the compiled descriptors at boot.
[BUNDLE_MOUNT_GATE]-[DROPPED]: the condition it degraded around is gone — with both rows landed the descriptor gate refuses drift at boot, and an availability probe before `host.register` masks exactly the rename that gate exists to surface.
[MEASURED_NAME_SPLIT]-[COMPLETE]: landed as `Metrics.timed` with both `transport/serve` registration sites and the prose; the receipts weave keeps `measured`, so a `measured` census now returns the span/fault/receipt weave alone.
[SCOPE_GRAMMAR_GUARD]-[COMPLETE]: landed as `SCOPE_ID` beside a refusal hoisted ABOVE the span mint on the receipts `measured` weave — an off-grammar scope rails `config` before `_tracer` opens anything, and the hoisted modality probe carries one refusal into both arms through `_lifted`.
[EVIDENCE_JOURNAL_OWNER]-[COMPLETE]: landed as `.planning/observability/journal.md` — the closed `AuditFact`/`MeterFact` family carrying its own stream, gate, retention, and measure projections, the writer-owned `Hlc` mint, the `Retain` window table over a tick-derived horizon, the never-shedding bounded drain against the composition-bound `Ledger` port with its re-armable close and unlanded roster, exact-decimal rating, and AAD-bound crypto-shredding over one native crossing; the `Series` vocabulary proves against the census at install, the `rasm.journal.*` rows and the `journal` domain landed on `metrics.md`, and `cryptography` admitted with its folder catalog.
[TRAIN_PRESENCE_GATE]-[COMPLETE]: landed on `.planning/observability/metrics.md` as the `TrainRow` `wraps` column and its `find_spec` gate — the `psycopg` row reified an instrumentor whose driver the resolved environment never installs, raising `ModuleNotFoundError` out of the composition root and abandoning every later row; the probe now skips an absent driver, keeps it out of the receipt, and activates it unchanged once the driver resolves.
[BENCH_MODE_MINT]-[COMPLETE]: landed on `.planning/observability/profiles.md` as the `BenchMode` enum, the `mode` field on `BenchmarkReceipt`, the `mode` argument on `BenchmarkReceipt.of` and `Bench.run`, and the mode fact on the contributed row; the artifacts corpus, compute study fold, and data query bench now resolve as written.
[WORKER_INSTALL_SEAM]-[COMPLETE]: landed on `.planning/execution/workers.md` — `WorkerBoot.captured` off `Telemetry.receipt`/`Profiles.receipt`, one `_worker_boot` initializer on every process arm, `WORKER_SIGNAL_PROFILE` geometry, seal-carried remote-floor boot, daemon spawn-env forwarding, and the parented kernel span in `traced_kernel`; graceful settle and roll drain through atexit, and named kill paths bound their forfeited tail to one worker export window.
[PROFILER_ATTACH_ROWS]-[COMPLETE]: landed as `_worker_boot`'s profiler arm (`worker.kind` tag, captured tenant) and the kernel-subject `phase` window in `traced_kernel`; `Profiles.install` samples GIL-releasing native kernels, and the `Profiles.phase` null-window and `Profiles.receipt` landed on `.planning/observability/profiles.md`.
[COST_DELTA_COLUMNS]-[COMPLETE]: landed as the receipts-minted `Cost` (`sampled`/`delta`/`combined`/`facts`/`measures`, signed RSS change, platform-gated `io_counters`), the two-read `traced_kernel` bracket, `DrainReceipt.cost` with the lane drain-window envelope, and the drained-line cost facts.
[COST_INSTRUMENT_ROWS]-[COMPLETE]: landed as four `rasm.cost.<measure>` histogram rows under `domain="cost"` on `.planning/observability/metrics.md`; tenant joins through the standing `_attributed` fold.
[CACHE_TRANSPORT_LEG]-[COMPLETE]: landed on `.planning/transport/roots.md` as the `AsyncCacheTransport` wrap over the pooled `AsyncHTTPTransport`, an `AsyncSqliteStorage` whose `database_path` derives from the `HttpEndpoint.cache_root` the composition admits, and a `CachePosture` row per destination; the card's "local file/sqlite store" Shape named a backend `hishel` does not ship and its own catalog `[RAIL_LAW]` rejects, and its second obligation was refuted on source — `AsyncClient.aclose` reaches `AsyncCacheTransport.aclose`, which closes the wrapped pool AND the storage, so a `.close()` beside it in the drain fold is the doubled teardown rather than the missing one.
[FAULT_SPELLING_SETS]-[COMPLETE]: landed as `spelled` at `reliability/faults#FAULT` — the module-qualified MRO spelling set both reliability matchers now intersect against, so the `CLASSIFY` frozenset row and `resilience`'s `_transient` target/refuse rosters share one derivation and a provider rename edits one surface; the duplicated MRO comprehension the card was minted against is gone from both pages.
[GEOMETRY_MEASURE_CHARTER]-[COMPLETE]: landed as ten `rasm.geometry.<measure>` rows on `.planning/observability/metrics.md` transcribing the charter's UCUM units verbatim, each kind selected by the charter's own aggregation intent — the eui last-value row taking the synchronous gauge and the cpu sum row a counter.
[PULSE_INSTRUMENT_ROWS]-[COMPLETE]: landed as `rasm.runtime.pulse.dropped`/`.rejected` counter rows under a new `runtime` domain, so the drain actor's authorized drops count instead of raising at their own accounting.
[COMPUTE_INSTRUMENT_ROWS]-[COMPLETE]: landed as four `rasm.compute.*` rows under a new `compute` domain; the geometry-versus-compute evidence-tail spelling divergence stays open at both producer pages, which own those constants.
[SCOPE_KEY_MAP]-[COMPLETE]: already realized on disk — the receipts-minted `ScopeKey`/`DEFAULT_SCOPE` threads `Hooks._points`/`_taps`/`_rings`, `Metrics._state`/`_receipts`, `Telemetry._receipts`, `Instrumentation._receipts`, `Profiles._receipts`, and the `_sink` default-scope resolution.
[EXP_AGGREGATION_ROW]-[COMPLETE]: already realized on disk — telemetry `WIRE_AGGREGATION` carries `ExponentialBucketHistogramAggregation` on both metric exporter rows, metrics advisory rows ruled the deployment-`View` fallback.
[DBAPI_WRAP_SEAM]-[COMPLETE]: already realized on disk — `Instrumentation.dbapi` calls the catalog spellings `wrap_connect`/`instrument_connection` through one `DbapiSeam` discriminator under `_DBAPI_POSTURE`.
[EGRESS_TRANSPORT_TABLE]-[COMPLETE]: already realized on disk — telemetry `EgressTransport`, the `SignalProfile.transport` column, `GRPC_ELIGIBLE`, endpoint derivation, and both `EGRESS` factory triples.
[OBSERVABILITY_PAGES]-[COMPLETE]: landed in `.planning/observability/{logging,hooks,profiles}.md` with the `metrics.md`/`telemetry.md`/`receipts.md` deepening — chain ownership moved to the logging page, the semconv pin (since re-homed beside the receipts scope stamp) and `resource`/`signal_profile`/`ship` injection seams on the telemetry install, instrument names renamed to `rasm.*`, query/geometry/bench domain rows added, and the instrumentation train — `opentelemetry-instrumentation-jinja2` (artifacts template render/compile/load spans), `opentelemetry-instrumentation-system-metrics` (`_SYSTEM_SLICE`: `system.*` and `cpython.gc.*` alone, process family stays on `rasm.process.*`), `opentelemetry-instrumentation-threading` (cross-thread context propagation) — composed on `metrics.md` `[02]-[INSTRUMENTATION]` and catalogued under `.api/`.
[REMOTE_KIND_ROW]-[COMPLETE]: landed in `.planning/execution/workers.md` — `WorkerKind.REMOTE` + `KIND_POLICY` SSH restart row, `WorkerPool` remote arm sealing the kernel over `asyncssh` `create_process`, `remote_floor` entry, Supervisor channel probe; `transport/roots` scope law amended with the `RemoteEndpoint` dial owner.
[SHM_CHANNEL_OWNER]-[COMPLETE]: landed in `.planning/execution/workers.md` — `ShmSpan` + `exported`/`released` on the crossing, decode inside `shipped`, exporter-owned unlink after the offload settles.
