# [PYTHON_TASKLOG]

The cross-package Python work, distilled from `IDEAS.md`: tasks that couple two or more packages or land on a shared runtime owner the whole branch inherits. Per-folder work lives on the owning folder `TASKLOG.md`; cross-language work lives in the cross-`libs/` `TASKLOG.md`. Each card uses `[ID]-[STATUS]:` plus `Capability`, `Shape`, `Unlocks`, `Anchors`, and optional `Tension`.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
- Atomic: <present only on a minor-scope task; one short phrase naming the small unit so a later session does not overscope its turn>.
-->

[CONTENT_ADDRESSED_REUSE_FABRIC]-[QUEUED]: fold content identity into lane admission.
- Capability: runtime lane admission accepts work as `(ContentKey, Work[T])` pairs, caches successful session-local results by key, and reports cache hits separately from completed work.
- Shape: extend `execution/lanes` `LanePolicy.run` and `DrainReceipt` around `ContentKey`, `IdentityPolicy`, `frozendict[ContentKey, T]`, `Result`, and `msgspec` payloads; only `Ok` outcomes enter the bounded LRU cache.
- Unlocks: `compute`, `data`, `geometry`, and `artifacts` inherit execution elision by keying their expensive outputs without minting package-local cache owners.
- Anchors: runtime `identity`, runtime `concurrency`, `xxhash`, `expression`, `msgspec`, and the C# content-identity wire seed.
- Tension: settings drift must miss correctly, and durable federation remains C#-owned across the wire rather than promoted into this session cache.

[GEOMETRY_KERNEL_OFFLOAD_LANE]-[QUEUED]: add the CPU-offload lane variant.
- Capability: runtime owns one CPU-bound offload lane for caller-supplied `geometry` registration/tessellation kernels and `compute` solver kernels.
- Shape: add a `LanePolicy` variant over `anyio.to_interpreter.run_sync` under the existing `CapacityLimiter` and `DrainReceipt`, with `to_process.run_sync` as the fallback path.
- Unlocks: heavy numeric and geometry work stops blocking the companion event loop while preserving one lane spine for I/O-bound and CPU-bound execution.
- Anchors: runtime `concurrency`, `anyio`, PEP 734 subinterpreters, `geometry` registration/tessellation loops, and `compute` solver kernels.
- Tension: the lane never imports sibling kernels, and the fallback path admits only callables that remain picklable for process execution.

[ONE_MEASURED_SIGNAL_STREAM]-[QUEUED]: build the one measured-signal stream.
- Capability: runtime emits one measured execution stream for companion requests, lane drains, and process health.
- Shape: author `observability/metrics` around one `MeterProvider`, request-duration histograms, `DrainReceipt` counters, `psutil` RSS/CPU gauges, and the OTLP HTTP exporter shared with logs and traces.
- Unlocks: every package contributes graduation latency, tessellation throughput, egress volume, render duration, lane saturation, and retry exhaustion through one observable surface.
- Anchors: runtime `observability`, `execution/lanes`, `transport/serve`, `opentelemetry-api`, `opentelemetry-sdk`, `opentelemetry-exporter-otlp-proto-http`, and `psutil`.
- Tension: the stream is local evidence only; product telemetry export and health remain AppHost-owned across the wire.

[SERVE_C_WIRE_CONSUME_GENERATED]-[BLOCKED]: serve the C# wire and consume the generated SDK — wire touchpoint for `libs/.planning` PYTHON_COMPANION_SERVES_WIRE / CAPABILITY_SDK_CODEGEN / CRDT_OPLOG_WIRE_AMENDMENT.
- Capability: runtime serves the C# `ComputeService`/`ArtifactSync` companion wire and derives command metadata from the generated capability SDK descriptor.
- Shape: build `transport/serve` over `grpc.aio`, `protobuf`, the UDS/InProcess leg, and companion-lane `grpcio-tools` codegen while decoding the amended CRDT op payload as the single C#-owned wire vocabulary.
- Unlocks: Python becomes the anchor for `PYTHON_COMPANION_SERVES_WIRE`, `CAPABILITY_SDK_CODEGEN`, and `CRDT_OPLOG_WIRE_AMENDMENT` without hand-written clients or branch-local op kinds.
- Anchors: runtime `transport/serve`, `grpcio`, `protobuf`, Forge companion `grpcio-tools`, and `csharp:Rasm.AppHost/Agent/capability#SDK_CODEGEN`.
- Tension: blocked on the upstream descriptor source and the CRDT op landing on the wire.

[PROVE_CONTENT_CAUSAL_IDENTITY_PARITY]-[BLOCKED]: prove the content and causal identity parity inbound — wire touchpoint for `libs/.planning` CONTENT_IDENTITY_PARITY / CAUSAL_TENANT_IDENTITY_WIRE.
- Capability: runtime proves inbound content identity, HLC causal stamp, and tenant frame parity against the C# source of truth.
- Shape: implement `evidence/identity` assertions for `XxHash128` seed zero/two-half order through `xxh3_128_intdigest`, `msgspec` receipt payloads, and the multi-runtime golden fixture; pure-Python HLC and tenant frames assert on the cp315 core.
- Unlocks: `CONTENT_IDENTITY_PARITY`, `CAUSAL_TENANT_IDENTITY_WIRE`, lane-admission reuse, and graduation evidence trust one verified inbound identity precondition.
- Anchors: runtime `evidence/identity`, `xxhash`, `msgspec`, the C# seed owner, and the multi-runtime golden fixture.
- Tension: blocked by the cp315 `xxhash` wheel gap for the content-seed leg, and the HLC half-order has no recovery signal if encoded in the wrong order.

[GRADUATE_OFFLINE_EVIDENCE_CONTENT_KEYED]-[QUEUED]: graduate offline evidence on the one content-keyed rail — wire touchpoint for `libs/.planning` GRADUATION_EVIDENCE_INWARD.
- Capability: compute graduates every useful offline result outward through one content-keyed evidence rail.
- Shape: pack `geometry` registration transforms, reconstructed meshes, topology graphs, network graphs, form-finding, and ONNX surrogate fits into the compute `graduation` `HandoffAxis` evidence shape keyed by runtime `ContentIdentity`.
- Unlocks: C# determinism closure re-imports Python offline evidence by one key and one rail instead of a per-package handoff family.
- Anchors: compute `graduation`, runtime `identity`, `HandoffAxis`, `onnx`, `msgspec`, and `GRADUATION_EVIDENCE_INWARD`.
- Tension: the rail depends on content-identity parity so the outward key matches the inward closure key.

[CONTENT_IDENTITY_PARITY_GATE]-[BLOCKED]: prove the digest-endianness parity gate, cp315 leg blocked on an upstream `xxhash` wheel.
- Capability: runtime verifies digest-endianness parity for `XxHash128` and `XxHash3` before any consumer trusts a Python content key.
- Shape: use companion-resolved `xxhash`, `xxh3_128_intdigest`, `xxh3_64_intdigest`, `ApiPackage.reflect`, and the C# golden fixture to prove little-endian child serialization against `System.IO.Hashing`.
- Unlocks: the content-identity touchpoint, lane-admission reuse fabric, and graduation rail inherit one verified seed-parity gate.
- Anchors: runtime `evidence/identity`, runtime `evidence/evidence`, `xxhash>=3.7.0`, C# `WriteUInt128LittleEndian`, and cross-`libs/` `CONTENT_IDENTITY_PARITY`.
- Tension: blocked on an upstream cp315/abi3 `xxhash` wheel, not a local environment sync; the companion-interpreter proof is assertable now, and the cp315 fence remains the single install-gated `RESEARCH` link.

[GRADIENT_DRIVEN_INVERSE_DESIGN]-[QUEUED]: seed the gradient-driven inverse-design optimization owner.
- Capability: compute owns gradient-driven inverse design as the apex of the differentiable solver stack.
- Shape: seed `compute/optimization` in compute `ARCHITECTURE.md`, `IDEAS.md`, and `compute/.planning/optimization/design.md` around one `DesignProblem` optimizer, Equinox objectives, Optimistix `minimise`/`least_squares`, `solvers/sensitivity#SENSITIVITY`, and a content-keyed `OptimizationReceipt`.
- Unlocks: PDE-constrained optimal design, inverse identification, parameter recovery, and design-of-experiment warm starts graduate as the `solver` `HandoffAxis` case on the single evidence rail.
- Anchors: `optimistix`, `equinox`, `jax`, optional `optax`, compute solver/sensitivity pages, runtime `ContentIdentity`/`RuntimeRail`/`Receipt`, and the `python_version<'3.15'` jaxlib band.
- Tension: optional `optax.OptaxMinimiser` remains tied to the `compute/.api/optax.md` coverage task if the first-order-descent axis admits.

[CF_FIELD_DATASET_OWNER]-[QUEUED]: seed the CF-conventioned field owner.
- Capability: data owns CF-conventioned labelled field cubes as a first-class gridded data owner.
- Shape: seed `data/gridded` in data `ARCHITECTURE.md`, `IDEAS.md`, and `data/.planning/gridded/field.md` around one `FieldDataset` over `xarray`, `netcdf4`, HDF5/Zarr, CF-aware coordinates, label slicing, group/resample reductions, units, CRS metadata, and content-keyed `pyarrow`/Zarr egress.
- Unlocks: environmental, CFD, sensor-grid, and geophysical field interchange becomes a branch-owned data capability rather than an incidental compute study input.
- Anchors: `data/.api/xarray.md`, `data/.api/netcdf4.md`, `data/.api/h5py.md`, `gridded/tensor.md`, runtime `ContentIdentity`, runtime `TransportResource`, and the `python_version<'3.15'` native-dependency band.
- Tension: the owner is distinct from dense chunk-grid tensor storage, and gated `xarray`/`netcdf4` arms dispatch through the runtime subprocess seam.

[PYE57_SCAN_INGESTION]-[QUEUED]: route the admitted `pye57` E57 reader into scan ingestion.
- Capability: the admitted `pye57` E57 reader becomes an ingestion arm for structured terrestrial-laser-scan data.
- Shape: fold `E57.read_scan_raw`, `scan_count`, and header pose data into the existing data mesh-exchange owner or `scan/registration.md` cloud-source admission, producing the `o3d.t.geometry.PointCloud` shape the registration owner already consumes.
- Unlocks: E57 scan files enter the existing point-cloud registration and `RegistrationResult` transform spine without a parallel reader file.
- Anchors: `geometry/.api/pye57.md`, `scan/registration.md`, `data/spatial/mesh.md`, `open3d`, `kiss_matcher`, `small_gicp`, `laspy`, `pdal`, and the companion native geometry floor.
- Tension: this is a catalogue-present consumer gap, not an environment block; ownership lands on the data/geometry seam rather than a new package owner.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
