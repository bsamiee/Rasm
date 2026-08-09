# [PY_RUNTIME_RULINGS]

`python/runtime` rulings settle package-scoped decisions.

## [01]-[PACKAGES]

- `loky`'s tracker skew closes at `_use_simple_format` before pool construction — its child spawns across an exec seam no parent rebind crosses.

## [02]-[SHAPE]

- `measured`'s free `scope` carries the branch telemetry grammar — a bare package-prefixed scope entering the exported weave forks one namespace.
- `runtime` mints every branch limiter — `THREAD_BAND`, `LanePolicy.limiter`, `_ISOLATION`, and `_PROBE_BAND` are the roster a sibling mint forks.
- `Hlc` samples this branch's clock and merges every inbound stamp before `tick` — the semilattice converges with no privileged physical authority.
- Above-`int64` wire ints carry `ge=0` alone and rail the ceiling at decode — msgspec spells no such bound, while in-band `I63` rides `Meta(le=…)`.
- Producer `optional` scalars spell `T | None` while no-presence scalars keep their zero — that keyword discriminates, never felt nullability.
- Telemetry install fails BEFORE publish — `_pipeline` completes construction before `_commit`, since every OTel global is set-once, never unset.
- OTLP is the one log egress door and the console stays armed — its `ProcessorFormatter` is the only seam a foreign stdlib record reaches the wire.
- Log posture FOLDS, never partitions — the stdlib root roster is process-global, so requests meet at the strictest floor and egress arms as a union.
- `LogLimits` stays a page-owned policy shape — importing the SDK record-limit type reifies the cold `sdk._logs` tier in every composition root.
- Payload bounds ride a CHAIN row, never a step in the wire projection — both renders read one narrowed value, so console and wire cannot disagree.
- Log egress arms by chain MEMBERSHIP, never per event — `shared_chain` rebuilds at configure, so an unarmed process allocates nothing per line.
- Wire-bound values coerce to shapes both renders keep — a mixed collection projects index-keyed, and leaf conversion rides the receipt encoder's.
- Terminal interpreter doors belong to the log egress and CHAIN their predecessor — a replacement deletes an embedding host's own hook.
- Foreign `extra` mappings stay untrusted for every chain-owned key — `trace_context` is the sole writer and sole eraser of the correlation roster.
- OTLP gRPC is refused on every forked floor — no channel survives `fork()`, so `GRPC_ELIGIBLE` admits the SIDECAR alone and install clamps the rest.
- Metric-stream shaping is data at the instrument owner and SDK objects at the install root — only that root names a `View` or an aggregation.
- Metric cardinality bounds at two tiers — the allow-list closes the KEY axis and a tenant budget the VALUE axis, folding at `otel.metric.overflow`.
- Temporality preferences key SDK FAMILY classes — monotonic sums and the histogram ride DELTA, non-monotonic sums CUMULATIVE; an API key raises.
- Tag-partitioned counters carry terminal partitions alone — the admitted count is a receipt line, and one more dimension value doubles every sum.
- Cross-cutting metric dimensions carry no domain segment — a discriminant meaningful everywhere names no capability subject, riding `rasm.<key>`.
- Measure shape decides the instrument class — a per-event delta records synchronously and a level observes, since a held delta republishes forever.
- Observable rows feed from a probe the EXPORT CYCLE calls — a producer knows a level only when it acts, so a pushed one reports a stale event.
- Declared aggregation intent picks the census row's kind — percentile, mean, and maximum ride one histogram whose data point already carries them.
- One instrumentation-scope triple homes at `reliability/faults#FAULT` — `identity`, `clock`, and `wire` open spans below every observability owner.
- Evidence never sheds and its derived projection always may — the journal AWAITS a bounded intake while line and series ride a droppable fence.
- Shutdown closes the intake and awaits the drain, retiring custody — a drain-deadline parameter re-threads the cancellation the scope already owns.
- `Ledger` is async WHOLE, proven at the bind emitter identity binds beside — one sync member stalls the loop and an unchecked bind defers its raise.
- Subject index rides the fact and its row — export and erasure key on one `(tenant, subject)` composite, so both rights compose by construction.
- Journal writers mint the `Hlc` stamp at admission over any caller slot — identity covers the stamped payload, so two producers never share one key.
- Erasure destroys the key and `open` is TOTAL — a destroyed key folds to `Nothing`, `InvalidTag` stays a fault, each envelope binding `SubjectKey`.
- Instrumentor train rows carry the driver they wrap and gate on presence before reifying — a contrib instrumentor imports its driver at module scope.
- Scope handles mint once per module, never per call — the API caches none, and the pre-install proxy upgrades at install with no invalidation.
- Every instrument row proves its `DOMAINS` segment at IMPORT over the whole table — filtering an unrostered row in the census defers the failure.
- Optional dimensions spell absence by OMITTING the key at every arm — an empty-string value identifies a series a board groups on and nobody fills.
- Supervision probes answer typed columns, WHICH ceiling tripped, and UNMEASURED — fences bind per COLUMN, so an all-refused reading actuates nothing.
- One `CapacityLimiter` publishes ONE occupancy probe that concurrent brackets REFCOUNT — `Metrics.occupied` sums live probes and retires by identity.
- Blocking native waits ride the library's OWN timeout in the thread — a cancel scope bounds the await while the abandoned thread keeps its token.
- Caller payload validates HOST-side onto a typed rail before the crossing — a defect reaching the far floor raises where no fault lift can name it.
- Secret resolution stays ONE sync provider union crossed through `anyio.to_thread.run_sync` under its own probe band — `hvac` ships no `aio` twin.
- Journal intake is TRI-STATE — an armed scope sends, retired custody refuses `config`, and an uninstalled one folds `Ok(0)` as a declaration.
- `BoundaryFault.wire` constructs only with a real protocol or status code — a registry miss lands `config`, a decoded-material gap `boundary`.
- Modality-polymorphic entrypoints whose return shape varies with input carry one `@overload` per shape — an unoverloaded arm type-checks nowhere.
- `HostRow` extends the descriptor with `lanes`/`recovery`/`scratch`/`document` and refuses `tenancy` — a host separates no tenant, the profile does.
- `ProviderRow` extends with `supplies`, refusing `tenancy` and `isolation` — `ConsumptionProfile.admit` carries that crossing as axis evidence.

## [03]-[COLLAPSE]

- Content-key elision and RFC-9111 revalidation stay two owners — lanes cuts before transport, HTTP revalidates at it, acquisition reading key first.
- Survivor and ordering decisions read `evidence/clock#CLOCK`'s `compare` and fold its `Ordering` — two spellings drift when either bound flips.
- Trace spans partition by `SpanKind` at its four boundaries — a unified aspect mis-kinds them and re-parents the serve interceptor's spans.
- `loky` and `pebble` stay two executors — crash-respawning warm reuse against terminal wall-clock kill, and neither carries the other's guarantee.
- Log-record exception semantics stay the SDK's — the emit seam takes the RAISED OBJECT, so the chain resolves `exc_info` to it ahead of the renderer.
- Stdlib-handler bridging collapses into the chain's terminal row — a handler reads a rendered record, carrying neither redaction nor caps.
- `BoundaryFault` grows by ingress class or case alone — a sibling fault type needing an adapter breaks the cross-tier `combine`/`aggregate` fold.

## [04]-[STRUCTURE]

- `workers` stays one module over crossing, pool, and supervision — a split forces the closed `WorkerKind`/`KernelTrait` vocabulary across a seam.

## [05]-[PROCESS]

- Every `Metrics.record` lands with its `INSTRUMENTS` row — `MEASURES` raises on an unregistered pair, so a dangling record kills its producer.
- Total constructors returning a VALUE refuse self-contradiction by RAISE — a rail obliges every call site to unwrap a gate that cannot fail.
- Structure proves first at every boot — vocabulary and layout gates seat ahead of any install claiming process custody, and read no installed state.
