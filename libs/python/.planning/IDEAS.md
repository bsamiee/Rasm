# [PYTHON_IDEAS]

The cross-package Python concert: higher-order concepts that couple two or more of the five packages or deepen a shared branch owner, distilled from the folder ideas. A concept grounded in one folder's domain stays on that folder's `IDEAS.md`; a concept crossing a language boundary stays in the cross-`libs/` `IDEAS.md` and is referenced as a wire seam, never restated here. `[1]-[OPEN]` holds the live concert as cards — a bracketed slug, the capability, what it unlocks, and the cross-package gap it draws on; `[2]-[CLOSED]` records dispositions.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
-->

[CONTENT_ADDRESSED_REUSE_FABRIC]-[QUEUED]: content keys become the branch-wide execution reuse fabric.
- Capability: branch-wide execution reuse keyed by the same `ContentIdentity` seed that names portable artifacts.
- Shape: runtime lane admission accepts `(ContentKey, Work[T])` pairs and short-circuits when the session cache already holds an `Ok` result for the key; `compute`, `geometry`, `data`, and `artifacts` key their most expensive outputs by that same identity.
- Unlocks: by-reference reuse across companion sessions and package boundaries, with determinism and cache hits derived from the key instead of re-declared by each owner.
- Anchors: runtime `identity`, `execution/lanes`, `DrainReceipt`, `expression` `frozendict`/`Result`, `msgspec`, and the branch rule that the four consumers compose runtime-owned shapes.
- Tension: the cache is session-local and bounded; durable federation remains the C# persistence owner consumed at the wire.

[GEOMETRY_KERNEL_OFFLOAD_LANE]-[QUEUED]: CPU-bound kernels run through one runtime offload lane.
- Capability: one runtime-owned CPU offload lane for geometry and numerical kernels.
- Shape: `LanePolicy` gains a CPU-bound variant over `anyio.to_interpreter.run_sync`; geometry registration/tessellation loops and compute solver kernels enter as caller-supplied functions under the same `CapacityLimiter` and `DrainReceipt`, while the lane never imports sibling kernels.
- Unlocks: true-parallel offline geometry and numeric work on subinterpreter-capable runtimes without creating separate process-pool owners per package.
- Anchors: PEP 734 subinterpreters, `anyio.to_interpreter`, the existing `to_process` isolation seam, geometry registration stacks, and compute solver routes.
- Tension: the process fallback requires picklable callables, so the callable boundary is part of the lane contract rather than a hidden implementation detail.

[ONE_MEASURED_SIGNAL_STREAM]-[QUEUED]: every package contributes measured execution signals to one runtime stream.
- Capability: one runtime metrics stream for branch execution evidence.
- Shape: runtime `observability/metrics` owns the companion request-duration histogram, `DrainReceipt` lane counters, and `psutil` process gauges behind one `MeterProvider`; package signals such as graduation latency, tessellation throughput, egress volume, and render duration flow through that stream.
- Unlocks: host-readable execution telemetry without per-package metric reinvention, making lane saturation, retry exhaustion, companion latency, and heavy-output throughput visible as shared metrics.
- Anchors: `opentelemetry-api`, `opentelemetry-sdk`, `opentelemetry-exporter-otlp-proto-http`, `psutil`, runtime receipt logs, and the companion server.
- Tension: product telemetry and health semantics stay AppHost-owned across the wire; Python emits local evidence, not a second product-health surface.

[ONE_GRADUATION_RAIL_OUTWARD]-[QUEUED]: offline results graduate outward through one evidence rail.
- Capability: one outward graduation rail for offline Python evidence.
- Shape: the compute `graduation` rail remains the only evidence path for useful results from `geometry`, `compute`, and `data`; registration transforms, reconstructed meshes, topology graphs, network graphs, and form-finding cross through `HandoffAxis` cases instead of per-package handoffs.
- Unlocks: one C#-consumable outward contract where every graduated result references the same determinism key and rail shape.
- Anchors: compute `graduation`, `HandoffAxis`, runtime `ContentIdentity`, geometry registration/tessellation results, and data egress bundles.
- Tension: geometry and data produce graduatable evidence but never own outward handoff families; the compute rail is singular.

[CONTENT_IDENTITY_PARITY_GATE]-[QUEUED]: digest parity proves the Python content key against the C# seed.
- Capability: content-key parity proof for the one seed every Python package trusts.
- Shape: runtime `evidence/identity` proves `XxHash128`/`XxHash3` digest-endianness against the C# `System.IO.Hashing` seed on the companion interpreter where `xxhash` resolves; the cp315 core confirms the same parity only after an upstream cp315 or abi3 wheel exists.
- Unlocks: the reuse fabric, graduation rail, geometry GLB key, data egress key, and artifacts document key as consumers of one verified content identity rather than package-local assumptions.
- Anchors: `xxhash`, `xxh3_128_intdigest`, `xxh3_64_intdigest`, `to_bytes(16, "little")`, C# `BinaryPrimitives.WriteUInt128LittleEndian`, the multi-runtime golden fixture, and runtime `ContentIdentity`.
- Tension: `xxhash>=3.7.0` ships cp38-cp314 wheels without cp315/abi3 coverage, so the cp315 leg is upstream-blocked rather than host-sync-pending.

[GRADIENT_DRIVEN_INVERSE_DESIGN]-[QUEUED]: inverse design becomes the apex of the differentiable solver stack.
- Capability: gradient-driven inverse design over the differentiable solver stack.
- Shape: `compute/optimization` owns one `DesignProblem`-discriminated optimizer that drives Equinox-parameterized objectives through Optimistix `minimise`/`least_squares`, reads the implicit-adjoint gradient from `solvers/sensitivity`, and folds an `OptimizationReceipt` over iterates, objective trace, KKT residual, and `ContentIdentity`.
- Unlocks: PDE-constrained optimal design, inverse identification, topology/shape/size optimization, sparse-observation parameter recovery, and design-of-experiment warm starts as first-class offline science.
- Anchors: `optimistix`, `equinox`, `jax`, `scikit-fem`, `diffrax`, `solvers/linear`, `solvers/nonlinear`, `solvers/differential`, `solvers/sensitivity`, `solvers/mesh`, `experiments/study`, and the `solver` `HandoffAxis` case.
- Tension: `optax` remains a conditional first-order-descent axis row carried by the task, not a phantom registry dependency.

[CF_FIELD_DATASET_OWNER]-[QUEUED]: CF-labelled field cubes become a first-class data owner.
- Capability: CF-conventioned labelled field cubes as a data owner.
- Shape: `data/gridded` owns one `FieldDataset` over `xarray` that reads and writes `netcdf4`/HDF5/Zarr CF-metadata field cubes, exposes coordinate selection, label-indexed slicing, grouped/resampled reductions, and unit/coordinate-reference metadata, and materializes through content-keyed `pyarrow`/Zarr egress.
- Unlocks: first-class environmental, simulation, scientific, sensor-grid, and geophysical field interchange for offline AEC companion work.
- Anchors: `xarray`, `netcdf4`, `h5py`, `pyarrow`, Zarr, `gridded/tensor`, runtime `ContentIdentity`, and the present `data/.api/xarray.md`, `data/.api/netcdf4.md`, and `data/.api/h5py.md` catalogues.
- Tension: the CF-labelled field owner is distinct from the dense chunk-grid owner; labels, coordinates, and CF metadata do not become a second tensor store.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
