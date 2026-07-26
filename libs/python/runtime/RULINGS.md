# [PY_RUNTIME_RULINGS]

`python/runtime` rulings settle package-scoped decisions.

## [01]-[PACKAGES]

- (none)

## [02]-[SHAPE]

- `measured`'s free `scope` parameter carries the branch telemetry grammar the hook, meter, and instrument owners already enforce — `rasm.`-rooted lowercase dot-segments; a bare package-prefixed scope entering the exported weave forks the branch's one telemetry namespace.
- Every new blocking concern earns its own named `CapacityLimiter` sized by its owner, and the anyio ambient default thread limiter stays refused branch-wide — an unnamed shared limiter lets one concern silently oversubscribe the host against every other.
- `Hlc` samples this branch's own admitted clock for local events and merges every inbound stamp before `tick` — the join-semilattice over `(physical, logical)` converges under independent physical mints, so a privileged single physical authority buys no ordering guarantee while making a Python-only deployment impossible; re-litigation opens only if merge stops dominating on the physical half.
- Wire integers above the `int64` band carry the `ge=0` floor alone with the ceiling enforced by the railed decode — msgspec meta expresses no above-`int64` bound, so an above-band ceiling annotation is a phantom the decoder never enforces; an in-band ceiling (`I63`) rides `Meta(le=...)` and is enforced.
- Telemetry install failure policy is fail-before-publish — `_pipeline` completes provider construction before `_commit` publishes, because the OpenTelemetry globals (`set_meter_provider`, `set_tracer_provider`, `set_logger_provider`, `propagate.set_global_textmap`) are set-once surfaces with no unset member; a demanded post-publication rollback compensates through a phantom inverse and is void, the real hardening moving work before publication; re-opens only when the SDK ships a public unset.
- OTLP gRPC egress is refused on every forked or spawned worker floor — a persistent gRPC channel never survives `fork()` — so HTTP proto is the fork-safe default, `GRPC_ELIGIBLE` admits the gRPC transport only for the non-forking SIDECAR daemon, and install clamps every other profile's injected gRPC row back to HTTP with the receipt carrying the effective geometry, never a silent downgrade.

## [03]-[COLLAPSE]

- Content-key elision and RFC-9111 revalidation stay two cache owners with no overlap — the lanes cache short-circuits recomputation by content key before any transport, the HTTP cache revalidates freshness at the transport leg by protocol; neither substitutes for the other, and an acquisition consults the content key first.
- Trace-span ownership stays partitioned by `SpanKind` at its four boundaries — serve interceptor SERVER, capability-invoke interceptors CLIENT, wire decode CONSUMER/PRODUCER, `traced_kernel` INTERNAL; one unified tracing aspect mis-kinds boundary spans and re-parents spans the serve interceptor already parented, breaking the OTLP parent-child topology every backend reads.
- `loky` and `pebble` stay two admitted executors — neither package carries the other's capability (crash-respawning warm reuse; terminal wall-clock kill with worker recycle), so one merged executor forfeits one of the two guarantees; re-opens only when one package gains the other's guarantee.
- `BoundaryFault` stays the one union every package returns through — per-package fault types break the cross-tier `combine`/`aggregate` fold at composition roots, so a new failure family lands as an ingress class or case on the one union, never a sibling type needing a translation adapter.

## [04]-[STRUCTURE]

- `workers` stays one module spanning fabric, pool, and supervision — the closed `WorkerKind`/`KernelTrait` vocabulary is the single discriminant all three regions project, and a file split forces that closed vocabulary across a module seam, re-deriving the kind family at three sites.

## [05]-[PROCESS]

- Every `Metrics.record` call lands with its `INSTRUMENTS` row in the same pass — `_DOMAIN_SLOT` raises on an unregistered measure name, so a dangling record is a producer-killing runtime fault, never a silent no-op; the row is the record's admission, not an optional follow-up.
