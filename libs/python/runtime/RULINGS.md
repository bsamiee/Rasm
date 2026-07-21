# [PY_RUNTIME_RULINGS]

Per-folder decision registry for the execution foundation — the settled rulings agents re-litigate for lack of a home. A row earns its place as a package admission or rejection with the reason it holds, an owner-choice discriminant, or a collapse refusal naming the guarantee the denser form loses. Append-mostly; a row earns its seat while its why stays homeless, and dies only when fact and why both live at one durable surface.

## [01]-[SHAPE]

- `measured`'s free `scope` parameter carries the branch telemetry grammar the hook, meter, and instrument owners already enforce — `rasm.`-rooted lowercase dot-segments; a bare package-prefixed scope entering the exported weave forks the branch's one telemetry namespace.
- A new blocking concern earns its own named `CapacityLimiter` sized by its owner, and the anyio ambient default thread limiter stays refused branch-wide — an unnamed shared limiter lets one concern silently oversubscribe the host against every other.
- Physical time mints once at the C# host port — the companion's clock and admission decode the host-minted causal stamp and `tick` advances only the logical half; a Python-side physical mint is a second time origin forking the physical-dominant HLC order and the prefix-replay join, so convergence and seed-reproduction parity hold only under one physical authority.
- Wire integers above the `int64` band carry the `ge=0` floor alone with the ceiling enforced by the railed decode — msgspec meta expresses no above-`int64` bound, so an above-band ceiling annotation is a phantom the decoder never enforces; an in-band ceiling (`I63`) rides `Meta(le=...)` and is enforced.
- Telemetry install failure policy is fail-before-publish — `_pipeline` completes provider construction before `_commit` publishes, because the OpenTelemetry globals (`set_meter_provider`, `set_tracer_provider`, `set_logger_provider`, `propagate.set_global_textmap`) are set-once surfaces with no unset member; a demanded post-publication rollback compensates through a phantom inverse and is void, the real hardening moving work before publication; re-opens only when the SDK ships a public unset.

## [02]-[COLLAPSE]

- Content-key elision and RFC-9111 revalidation stay two cache owners with no overlap — the lanes cache short-circuits recomputation by content key before any transport, the HTTP cache revalidates freshness at the transport leg by protocol; neither substitutes for the other, and an acquisition consults the content key first.
- Trace-span ownership stays partitioned by `SpanKind` at its four boundaries — serve interceptor SERVER, capability-invoke interceptors CLIENT, wire decode CONSUMER/PRODUCER, `traced_kernel` INTERNAL; one unified tracing aspect mis-kinds boundary spans and re-parents spans the serve interceptor already parented, breaking the OTLP parent-child topology every backend reads.
- `loky` and `pebble` stay two admitted executors — neither package carries the other's capability (crash-respawning warm reuse; terminal wall-clock kill with worker recycle), so one merged executor forfeits one of the two guarantees; re-opens only when one package gains the other's guarantee.
- `BoundaryFault` stays the one union every package returns through — per-package fault types break the cross-tier `combine`/`aggregate` fold at composition roots, so a new failure family lands as an ingress class or case on the one union, never a sibling type needing a translation adapter.

## [03]-[STRUCTURE]

- `workers` stays one module spanning fabric, pool, and supervision — the closed `WorkerKind`/`KernelTrait` vocabulary is the single discriminant all three regions project, and a file split forces that closed vocabulary across a module seam, re-deriving the kind family at three sites.

## [04]-[PROCESS]

- A `Metrics.record` call and its `INSTRUMENTS` row land in the same pass — `_DOMAIN_SLOT` raises on an unregistered measure name, so a dangling record is a producer-killing runtime fault, never a silent no-op; the row is the record's admission, not an optional follow-up.
