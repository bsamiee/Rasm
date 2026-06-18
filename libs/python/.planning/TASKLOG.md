# [PYTHON_TASKLOG]

The cross-package Python work, distilled from `IDEAS.md`: tasks that couple two or more packages or land on a shared runtime owner the whole branch inherits. Per-folder work lives on the owning folder `TASKLOG.md`; cross-language work lives in the cross-`libs/` `TASKLOG.md`. Each card leads with a status marker — `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` open; `[COMPLETE]` or `[DROPPED]` closed — and carries the capability or file to build, the external packages to integrate, the integration points and boundaries, and the key considerations.

## [1]-[OPEN]

[QUEUED] Fold content identity into lane admission — from `CONTENT_ADDRESSED_REUSE_FABRIC`.
- Extend the runtime `concurrency/lanes` `LanePolicy.run` to accept work as `(ContentKey, Work[T])` pairs threading a session-local `frozendict[ContentKey, T]` cache, so a unit whose key already carries a result short-circuits, the `DrainReceipt` gaining a `hit` count distinct from `completed`.
- Integrate `xxhash` (via runtime `identity`), `expression` (`frozendict`/`Result`), `msgspec`.
- The fold lands inside the runtime `concurrency/` owner reading the `identity/content-identity` `ContentKey` shape; the four consumers inherit the elision by keying their `(ContentKey, Work)` admission, never a second cache owner; the cache is session-local, durable federation staying the C# owner consumed at the wire.
- The key folds the `IdentityPolicy` so a settings change misses correctly; only an `Ok` outcome caches; eviction is bounded by an LRU cap so the session cache never grows unbounded.

[QUEUED] Add the CPU-offload lane variant — from `GEOMETRY_KERNEL_OFFLOAD_LANE`.
- Extend the runtime `concurrency/lanes` `LanePolicy` with a CPU-bound offload variant over `anyio.to_interpreter.run_sync`, routing caller-supplied geometry and compute kernels into per-subinterpreter execution under the same `CapacityLimiter` and `DrainReceipt`.
- Integrate `anyio` (`to_interpreter.run_sync`, `to_process.run_sync` as the fallback when subinterpreters are unavailable).
- The lane lands in the runtime `concurrency/` owner; the offloaded kernels are the `geometry` registration/tessellation loops and the `compute` solver kernels handed in by the sibling — the lane offloads, never imports the kernel; the receipt drains through the one `DrainReceipt`.
- Subinterpreter availability is runtime-gated (PEP 734); the variant falls back to `to_process.run_sync` when `to_interpreter` is absent, and the offloaded callable must be picklable on the process path.

[QUEUED] Build the one measured-signal stream — from `ONE_MEASURED_SIGNAL_STREAM`.
- Author the runtime `observability/metrics` instrument set — a companion request-duration histogram, lane drain counters folded from `DrainReceipt`, and `psutil` RSS/CPU gauges — against one `MeterProvider` carrying the process/runtime semantic-conventions resource, exported over the same OTLP exporter the logs and traces use.
- Integrate `opentelemetry-api`, `opentelemetry-sdk` (`MeterProvider`/`ObservableGauge`/`ObservableCounter`), `opentelemetry-exporter-otlp-proto-http`, `psutil`.
- The instruments land in the runtime `observability/` owner; the drain-counter source is the `concurrency/lanes` `DrainReceipt` and the request source is the `server/serve` companion, so every consumer's measured signal rides one stream; async-observable callbacks read live state without holding a lane lock.
- The metric stream is local-evidence only — product telemetry export and health stay AppHost-owned across the wire, never re-minted on a Python surface.

[QUEUED] Extend the structural drift detector to the branch — from `CROSS_PACKAGE_DRIFT_GUARD`.
- Extend the runtime `evidence/evidence` `Structural` query family with an intra-branch drift query locating a re-minted canonical concept (a second content-identity seed, receipt rail, retry owner, or wire-projection name) across the `compute`/`data`/`geometry`/`artifacts` sources, returning the offending spans for the `assay code` rail.
- Integrate `tree-sitter`, `tree-sitter-python`.
- The query lands in the runtime `evidence/` owner; it consumes the shared canonical-name set and feeds the `assay code` rail and the one-owner-per-axis law, never minting a parallel registry; the cross-language case stays in the cross-`libs/` pool.
- A false positive on a legitimately distinct same-named concept in a distinct namespace is distinguishable by the query's namespace scope, never a blanket name match.

[BLOCKED] Serve the C# wire and consume the generated SDK — wire touchpoint for `libs/.planning` PYTHON_COMPANION_SERVES_WIRE / CAPABILITY_SDK_CODEGEN / CRDT_OPLOG_WIRE_AMENDMENT.
- Stand up the runtime `server/serve` companion serving the C# `ComputeService`/`ArtifactSync` gRPC over the UDS/InProcess leg and deriving its command surface from the upstream `csharp:Rasm.AppHost/capability/registry#SDK_CODEGEN` descriptor, decoding the amended CRDT op payload as the one wire vocabulary — never a hand-written client and never a second op kind the wire does not carry.
- Integrate `grpcio`/`protobuf` (the `grpc.aio` server-runtime, transitively present on the cp315 core via `specklepy`); `grpcio-tools` (the proto codegen compiler) rides the Forge companion lane.
- The companion reaches no C# interior; it decodes the one C#-owned wire vocabulary and consumes the generated descriptor as settled command metadata; this is the Python anchor the cross-`libs/` PYTHON_COMPANION_SERVES_WIRE, CAPABILITY_SDK_CODEGEN, and CRDT_OPLOG_WIRE_AMENDMENT seams consume, never restated cross-language.
- Blocked on the upstream descriptor source and the CRDT op landing on the wire.

[QUEUED] Prove the content and causal identity parity inbound — wire touchpoint for `libs/.planning` CONTENT_IDENTITY_PARITY / CAUSAL_TENANT_IDENTITY_WIRE.
- Reproduce the C#-owned `XxHash128` content seed (seed zero, two-half order) bit-identically in the runtime `identity/content-identity` owner via `xxh3_128_intdigest`, and propagate the same HLC two-half causal stamp and tenant frame on the receipt inbound, asserting both against the multi-runtime golden fixture.
- Integrate `xxhash` (`xxh3_128_intdigest`), `msgspec`.
- This is the Python anchor the cross-`libs/` CONTENT_IDENTITY_PARITY and CAUSAL_TENANT_IDENTITY_WIRE seams consume; the seed and stamp are the single C#-owned source the runtime reproduces, never a second mint; the lane-admission reuse fabric and the graduation rail both trust this parity as their precondition.
- The HLC two-64-bit-half order rides the same parity fixture as the content seed; a logical off-by-one-half corrupts ordering with no other signal.

[QUEUED] Graduate offline evidence on the one content-keyed rail — wire touchpoint for `libs/.planning` GRADUATION_EVIDENCE_INWARD.
- Pack every graduatable offline result (the `geometry` registration transforms, reconstructed meshes, topology graphs, network graphs, form-finding, the ONNX surrogate fit) into the compute `graduation` `HandoffAxis` evidence shape keyed by `ContentIdentity`, the single outward contract the C# determinism closure re-imports by the one key.
- Integrate `onnx` (the surrogate-fit artifact), `msgspec` (the evidence shape); composes the runtime `identity` content key.
- The rail stays singular — geometry, compute, and data egress reach it through the `HandoffAxis` geometry/result case, never a parallel per-package handoff; this is the Python anchor the cross-`libs/` GRADUATION_EVIDENCE_INWARD seam consumes, the C# side running ONNX inference over the artifact, never an in-process training loop.
- Depends on the content-identity parity touchpoint so the outward key matches the inward closure key.

[BLOCKED] Land `xxhash` in the synced `.venv` and prove the digest-endianness parity gate — from `CONTENT_IDENTITY_PARITY_GATE`.
- `xxhash` is the one manifest-declared package (`xxhash>=3.7.0`, abi3 wheel) not yet installed on the provisioning host, so the runtime `identity/content-identity` `xxh3_128_intdigest`/`xxh3_64_intdigest` spellings and the `XxHash128`/`XxHash3` digest-endianness parity against the C# `System.IO.Hashing` seam cannot be reflection-verified or asserted until it syncs.
- Integrate `xxhash` (the install), the runtime `evidence/evidence` `ApiPackage.reflect` capture, and the multi-runtime golden fixture the C# seed owner produces.
- This gate is the precondition the whole branch inherits: the content-identity parity touchpoint, the content-addressed lane-admission reuse fabric, and the graduation rail all trust the seed reproduces bit-identically, so the digest-endianness mismatch surfaces here before any consumer keys an output by a corrupt key.
- Blocked on the `xxhash` wheel landing in the synced `.venv`; until then the `identity` spellings and the parity assertion stay a marked `RESEARCH` seam, never a fabricated signature, and the cross-`libs/` `CONTENT_IDENTITY_PARITY` seam consumes this gate as its Python-side precondition.

[COMPLETE] Add the async fault-lift sibling and route the gated artifacts producers — from `ASYNC_BOUNDARY_ONE_RAIL`.
- The runtime `reliability/faults` owner carries `async_boundary[T](subject, thunk: Callable[[], Awaitable[T]])` beside the synchronous `boundary`, one rail family lifting an awaited thunk into `Error(BoundaryFault.of(...))` exactly once; `anyio` is named on the fault card as the subprocess-seam the async lift closes over.
- The five gated `artifacts` producers — `charts/chart-spec` `ChartExport.render`, `scene3d/scene` `Scene3d.render`, `documents/document-plan` `DocumentPlan.produce`, `compression/bundle` `Bundle.pack`, `imaging/preview` `Preview.of` — compose `await async_boundary(...)` over their `async _emit`, where the gated arm crosses `anyio.to_process.run_sync`; the synchronous producers (`tables/table-plan`, `typography/conformance`, `reporting/report-plan`, `color-management/colorimetry`) keep the sync `boundary`.
- Design landed; the async lift is the one boundary every gated-band crossing inherits, never a parallel async fault rail, and the synchronous `boundary` stays the in-line compute lift.

[QUEUED] Author the missing `.api/` catalogues for FINALIZED-fence code spans — from `API_CATALOGUE_COVERAGE`.
- Author each absent folder `.api/<pkg>.md` catalogue every finalized fence names as a load-bearing code span yet no catalogue covers, one per external library under `[1]-[DOC_SET]`: compute `compute/.api/{optimistix,lineax,diffrax,equinox,numpyro,nutpie,pymc,arviz,SALib,python-flint,mpmath,array-api-compat,array-api-extra,scikit-fem,skl2onnx}.md` and data `data/.api/{rustworkx,zarr,cubed,awkward,narwhals,arro3-core,nanoarrow}.md`, drawing each real surface from a uv-sync reflection pass rather than guessing.
- Integrate `importlib.metadata` and the runtime `evidence/evidence` `ApiPackage.reflect` surface; the capture runs on the interpreter each package resolves on — the cp315 core for the pure-Python rows (`mpmath`/`array-api-compat`/`array-api-extra`/`narwhals`/`SALib`/`rustworkx`/`zarr`/`cubed`/`awkward`/`arro3-core`/`nanoarrow`), the gated `python_version<'3.15'` jaxlib band for `optimistix`/`lineax`/`diffrax`/`equinox`/`numpyro`, the numba marker floor for `nutpie`, and the gated FEM/Arb band for `scikit-fem`/`python-flint`.
- Each catalogue lands in the owning folder `.api/` (bare `<pkg>.md`, never an `api-`-prefixed filename); until it lands the fence members it backs stay a marked `RESEARCH` seam on the owning page — the compute FEM `skfem.asm`/`Mesh`/`Basis`, the JAX solver `Solution.stats`, the inference `SamplerBackend`, the data interop `narwhals`/`arro3-core`/`nanoarrow` and graph `rustworkx` spellings — never settled fence code.
- The `[6]-[REVIEW]` fence-truthfulness rule fails any fence whose external member has no present catalogue, so this coverage task is the precondition the compute solver/inference/FEM and data interop/graph/tensor pages finalize against; the compute and data folder `.api/` already carry the rows their finalized fences settle (`jax`/`scipy`/`scikit-learn`/`sympy`/`numba`/`onnx`/`meshio` on compute; `polars`/`duckdb`/`pyarrow`/`networkx` on data), so the absent rows above are the residual gap the branch tracks.

[QUEUED] Reclassify the runtime `API_CATALOGUE_CAPTURE` provenance — from `API_CATALOGUE_COVERAGE`.
- Split the runtime `[API_CATALOGUE_CAPTURE]` quartet by resolvability: `obstore`/`apscheduler`/`keyring` resolve on the cp315 core (spellings live-verified), so their catalogue capture is a pending uv-sync reflection pass, not a gated dependency; `xxhash` alone stays blocked on install behind the `[CONTENT_IDENTITY_PARITY_GATE]` card above.
- Integrate the runtime `evidence/evidence` `ApiPackage.reflect` surface against the synced `.venv`; no member signature is authored without reflection evidence.
- The reclassify keeps the runtime folder `TASKLOG` honest — three core-resolvable captures are RESEARCH-capture-pending and one is install-gated — so a page is never blocked on an environment floor a package does not actually need.

## [2]-[CLOSED]

[COMPLETE] Admit the companion environment floor — resolved by Forge. The `python_version<'3.13'` companion environment is provided by the Forge companion lane (`forge-companion-env`, python312) for the `grpcio-tools` proto compiler and the native geometry/IFC cores (`ifcopenshell`/`open3d`/`small-gicp`/`topologicpy`); the `grpc.aio` runtime leg (`grpcio`/`protobuf`) resolves transitively on the cp315 core through `specklepy`, so only proto codegen rides the lane. `compas`/`compas_dr`/`compas_tna`/`manifold3d` are manifest-declared on the `<'3.15'` gated band and `vtk`/`pyvista` are manifest-gated `<'3.13'` (both Rasm-owned, distinct from the lane); `pythonocc-core` has no PyPI distribution (honest deferral). The residual is documentation ownership — keeping each folder registry's provenance honest — not an environment gate.
