# [COMPUTE]

`Rasm.Compute` is the measured-execution engine and discipline-assessment authority over the `ElementGraph`: a solver farm engineering failure as rigorously as success, every verdict and failure a content-keyed fact with retry policy. Screening loops re-solve thousands of variants, so failure caching, keyed reuse, and dispatchable sweeps are load-bearing, and a declared discipline Compute cannot answer is a coverage defect.

One intent rail admits every execution request once, a substrate axis routes it over row data, bounded lanes carry it, and the one `ComputeReceipt` union records every outcome. It reads the `Rasm` kernel, the `Rasm.Element` seam, AppHost ports, and Persistence stores as settled lower-stratum vocabulary.

## [01]-[ROUTER]

[TENSOR]:
- [01]-[VOCABULARY](.planning/Tensor/vocabulary.md): CPU tensor vocabulary — shape, factory, dtype, and op-family axes every numeric lane reads.
- [02]-[LAYOUT](.planning/Tensor/layout.md): Layout forms and the one shape-edit request union.
- [03]-[DISPATCH](.planning/Tensor/dispatch.md): Arity-keyed kernel dispatch and the differentiable-adjoint law.
- [04]-[RESIDENCY](.planning/Tensor/residency.md): GPU residency lattice and the geometry-to-tensor encoding boundary.
- [05]-[MEMORY](.planning/Tensor/memory.md): Bounded staging memory and the zero-copy stream pool.
- [06]-[BLAS](.planning/Tensor/blas.md): Dense-BLAS, factorization, and spectral core.
- [07]-[FACTOR](.planning/Tensor/factor.md): Sparse ingestion and the criterion-stack iterative solve.
- [08]-[QUADRATURE](.planning/Tensor/quadrature.md): Measured integration over the kernel quadrature floor, trajectory driving, spectral operators.
- [09]-[SAMPLING](.planning/Tensor/sampling.md): Sobol/Halton sampling and radial-basis scatter reconstruction.

[SYMBOLIC]:
- [10]-[EXPRESSION](.planning/Symbolic/expression.md): CAS expression algebra over the `Entity` tree.
- [11]-[DIMENSIONAL](.planning/Symbolic/dimensional.md): ℚ⁷ SI base-dimension proof.
- [12]-[LOWERING](.planning/Symbolic/lowering.md): Content-keyed compiled-expression cache and the analytic-Jacobian arm.
- [13]-[UNITS](.planning/Symbolic/units.md): Units boundary admitting every unit-bearing input.

[MODEL]:
- [14]-[IDENTITY](.planning/Model/identity.md): Checksum model identity with acquisition, schema-snapshot, and drift-sentinel evidence.
- [15]-[SESSIONS](.planning/Model/sessions.md): One shared session per checksum with warm-start and its per-bucket warm roster.
- [16]-[PROVIDERS](.planning/Model/providers.md): Execution-provider axis with discovery, quantization posture, and the guaranteed floor.
- [17]-[INFERENCE](.planning/Model/inference.md): Run-mode inference fold, batching gate, tiled mosaic, stage-execution wire, and result cache.
- [18]-[EMBEDDING](.planning/Model/embedding.md): Retrieval half of the inference spine — encoding axis, metric axis, content-keyed vector carrier.
- [19]-[GENERATIVE](.planning/Model/generative.md): Token-streaming generation with the tool-call arm.
- [20]-[EXTENSION](.planning/Model/extension.md): Custom-op registration at the string-tensor boundary.

[SOLVER]:
- [21]-[DISCRETIZATION](.planning/Solver/discretization.md): Volumetric meshing with adaptive refinement and exact-predicate gates.
- [22]-[CONTRACT](.planning/Solver/contract.md): Physics-by-boundary-condition solve fold with adaptive recovery.
- [23]-[CONSTITUTIVE](.planning/Solver/constitutive.md): Per-Gauss-point stress-update axis and contact enforcement.
- [24]-[OPTIMIZER](.planning/Solver/optimizer.md): Design-space search axis with surrogate duality.
- [25]-[SWEEP](.planning/Solver/sweep.md): N-dim DOE sweep grid and sensitivity analysis.
- [26]-[CLASH](.planning/Solver/clash.md): Collision compute, occlusion rays, and the digital-twin loop.
- [27]-[UNCERTAINTY](.planning/Solver/uncertainty.md): Forward-UQ and reliability over the shared evaluate oracle.
- [28]-[SATISFY](.planning/Solver/satisfy.md): SMT rule satisfaction with witness and unsat-core explanation.

[STATS]:
- [29]-[ESTIMATOR](.planning/Stats/estimator.md): One Fit/Predict estimator axis across the statistical families.
- [30]-[SIGNAL](.planning/Stats/signal.md): Spectral-transform axis and filter design.
- [31]-[MONITOR](.planning/Stats/monitor.md): Streaming monitor capsules — EWMA limits, P² quantile sketch, detector rows, drift verdict.

[RUNTIME]:
- [32]-[ADMISSION](.planning/Runtime/admission.md): Typed intent admission with the substrate axis and total dispatch.
- [33]-[SCHEDULING](.planning/Runtime/scheduling.md): Bounded work-lanes and the dependency job-graph scheduler.
- [34]-[PROGRESS](.planning/Runtime/progress.md): Monotonic phase family and the progress capsule.
- [35]-[RECEIPTS](.planning/Runtime/receipts.md): One `ComputeReceipt` fact union projecting instruments, benchmarks, hooks, cost, alerts.
- [36]-[WIRE](.planning/Runtime/wire.md): Wire contract — proto vocabulary, evolution, and fault projection.
- [37]-[TRANSPORT](.planning/Runtime/transport.md): Channel mechanics — transport rows, tuning, and the artifact-frame law.
- [38]-[CODECS](.planning/Runtime/codecs.md): Field, result, waveform-interchange, and geometry-delta codecs and the tessellation bridge.
- [39]-[PAYLOAD](.planning/Runtime/payload.md): Residency-payload codec and the cluster-LOD chain.

[ANALYSIS]:
- [40]-[ASSESSMENT](.planning/Analysis/assessment.md): Lifecycle-aware assessment spine and reconciler.
- [41]-[AGGREGATOR](.planning/Analysis/aggregator.md): Multi-ply assembly aggregator over U, STC, GWP, and cost.
- [42]-[STRUCTURAL](.planning/Analysis/structural.md): Frame solve and the design-code capacity table.
- [43]-[PHYSICS](.planning/Analysis/physics.md): Closed-form thermal, acoustic, and fire folds.
- [44]-[ENERGY](.planning/Analysis/energy.md): Energy-route axis over the simulation toolchain.
- [45]-[LIFECYCLE](.planning/Analysis/lifecycle.md): Embodied-carbon and cost rollup over the EPD boundary.
- [46]-[CIRCULATION](.planning/Analysis/circulation.md): Egress and life-safety runner.
- [47]-[DAYLIGHT](.planning/Analysis/daylight.md): Sun hours, shadow fraction, sky view, and Perez diffuse irradiance off the kernel almanac.

## [02]-[DOMAIN_PACKAGES]

Domain-specific libraries admitted by this folder; versions centralize in `Directory.Packages.props` and corroborate against this folder's `.api/`.

[NUMERIC_KERNEL]:
- `MathNet.Numerics.Providers.MKL` — MKL native `LinearProvider` backend.
- `MathNet.Numerics.Providers.OpenBLAS` — OpenBLAS native `LinearProvider` backend.
- `cslsqp` — Source-vendored (oberbichler, ISC): span-based SLSQP the `OptimizerKind.slsqp` row binds.
- `HyperJet` — Hyper-dual scalar AD backing exact gradient and Hessian across the `Sensitivity` family.

[SOLVER_SEARCH]:
- `AngouriMath` — Managed CAS: `Entity` parse, simplify, solve, integrate, differentiate, and `Compile<>` in one owner.
- `Google.OrTools` — CP-SAT and MILP exact optimization lane.
- `GeneticSharp` — Evolutionary and metaheuristic `OptimizerKind` tier OR-Tools does not reach.
- `Microsoft.Z3` — SMT rule satisfaction returning SAT/UNSAT and unsat-core, where CP-SAT optimizes.

[MODEL_RUNTIME]:
- `Microsoft.ML.OnnxRuntime` — ONNX inference session core.
- `Microsoft.ML.OnnxRuntime.Extensions` — Custom-op and string-tensor extension surface.
- `Microsoft.ML.OnnxRuntimeGenAI` — Token-streaming generative run.
- `TorchSharp` — Native ATen dense linear algebra and the iterative `EstimatorKind` autograd fits.
- `libtorch-cpu` — osx-arm64 native backend behind TorchSharp.

[ARRAY_STORE]:
- `PureHDF` — Managed HDF5 behind the ONE `Runtime/codecs` archive owner; every consumer composes over that one session capsule.
- `PureHDF.Filters.BZip2.SharpZipLib` — Managed BZip2 codec registered on the HDF5 filter pipeline.
- `PureHDF.Filters.Lzf` — Managed LZF codec on the same pipeline; the accelerated native filter packages publish no osx RID.
- `Microsoft.IO.RecyclableMemoryStream` — Pooled-buffer stream behind the artifact frames and the tileset-manifest emit.

[REMOTE_TRANSPORT]:
- `Grpc.Net.Client.Web` — gRPC-Web handler for HTTP/1.1 and browser-constrained paths.
- `Grpc.Net.Common` — Shared compression and connectivity vocabulary beneath the gRPC rails.
- `EC3` — openEPD REST service consumed hand-thin over `HttpClient`; no manifest row.

## [03]-[SUBSTRATE_PACKAGES]

Shared substrate consumed from the C# registry, whose charters own the full contracts; `libs/csharp/.api/` holds the shared API evidence.

[CORE_SUBSTRATE]:
- `LanguageExt.Core`
- `Thinktecture.Runtime.Extensions`
- `Thinktecture.Runtime.Extensions.Json`
- `Generator.Equals` — `[Equatable]` structural equality and `Inequalities` diff rails for class roots and collection members; union CASES only.
- `JetBrains.Annotations`
- `NodaTime`
- `System.IO.Hashing`
- `CommunityToolkit.HighPerformance`
- `System.Numerics.Tensors`
- `UnitsNet` — `Analysis/energy` result-unit coercion; `Analysis/aggregator` ISO 6946 surface-film binding.
- `QuikGraph` — Adjacency, dependency, and partition graph algebra: traversal, condensation, contention colouring, and cut-minimizing bisection.
- `Riok.Mapperly` — Reader-free boundary transcription, every mapping compiler-proof under `RequiredMappingStrategy.Both`.

[NUMERIC_SUBSTRATE]:
- `CSparse` — Managed sparse direct-factor terminal.
- `MathNet.Numerics` — Quadrature, distributions, and the MKL/OpenBLAS provider hooks.
- `PeterO.Numbers` — Exact-rational `ERational` ℚ⁷ dimension carrier and the `EFloat` criterion-sum accumulator.

[GPU_DEVICE]:
- `Silk.NET.WebGPU` — GPGPU dispatch over the AppUi-minted device; this lane acquires none of its own.
- `Silk.NET.WebGPU.Extensions.WGPU` — `QueueSubmitForIndex` and `DevicePoll` deterministic completion beside pipeline statistics.

[GEOMETRY_INTERCHANGE]:
- `SharpGLTF.Core` — glTF core read and write beneath the tile-content lane.
- `SharpGLTF.Toolkit` — Mesh-building toolkit.
- `SharpGLTF.Ext.3DTiles` — 3D Tiles extension registration the `Runtime/codecs` tile partition seats at composition.

[MESH_PROCESSING]:
- `Alimer.Bindings.MeshOptimizer` — Residency-pyramid simplification and cluster-LOD bindings.

[PLANAR_GEOSPATIAL]:
- `NetTopologySuite` — Isovist and visibility polygons at the circulation planar boundary.
- `Clipper2` — Corridor-clearance offset algebra at the same boundary.

[ENERGY_SIMULATION]:
- `NREL.OpenStudio.macOS-arm64` — In-process SWIG SDK lowering `ElementGraph` to OSM and IDF and reading `SqlFile`.
- `PollinationSDK` — `EnergyRoute.Cloud` transport onto the same `SqlFile` fold.

[DATA_SUBSTRATE]:
- `Apache.Arrow` — Columnar `RecordBatch` construction for surrogate-training and billing egress; the egress train stays Persistence-side.
- `Microsoft.Data.Sqlite` — Read-only eplusout.sql tabular reader.

[SERVICE_CONTRACTS]:
- `Microsoft.Extensions.Caching.Hybrid` — One `HybridCache` per cache lane.
- `Microsoft.Extensions.AI` — `IChatClient` abstraction the AppHost provider binds; runtime `Microsoft.Extensions.AI.Abstractions`.

[EVENT_TRANSPORT]:
- `CloudNative.CloudEvents` — Envelope type the branch-owned MQTT 5.0 and NATS bindings raise; `Rasm/Domain/event` owns grammar, roster, and decode.
- `NATS.Net` — NATS Core subscription seam for broker sensor ingest and the request/reply compute leg.

[MACHINE_CONNECTIVITY]:
- `MQTTnet` — MQTT v5 carrier beneath the branch-owned MQTT binding the sensor ingest decodes through.

[WIRE_CODEGEN]:
- `Google.Protobuf`
- `Grpc.Net.Client`
- `Grpc.AspNetCore`
- `Grpc.Core.Api` — `ServerCallContext`, `IServerStreamWriter<T>`, and `Metadata` on the served compute endpoints.
- `Grpc.Tools`
- `NodaTime.Serialization.Protobuf`

[RUNTIME_INBOX]:
- `System.Net.Http` — `SocketsHttpHandler` policy the remote transport binds beneath its gRPC channel and probe legs.
- `System.Text.Json` — Generated contexts and hand-written `Utf8JsonWriter` codecs beside protobuf on receipt, descriptor, drift, and evidence lanes.

[TEST_SUBSTRATE]: Rows bind in branch test and benchmark projects, never the package csproj.
- `BenchmarkDotNet`
- `Microsoft.AspNetCore.TestHost` — In-memory server the `RemoteTransport.InProcess` row injects in the transport test harness.
