# [COMPUTE]

`Rasm.Compute` is the federation's measured-execution engine: the discipline-assessment authority over the `ElementGraph` — a solver farm engineering failure as rigorously as success, every verdict and failure a content-keyed fact with retry policy — over a building-scale numeric substrate, an optimization and uncertainty spine over one evaluate oracle, a symbolic CAS lane, and the ONNX model plane. Screening loops re-solve thousands of variants, so failure caching, keyed reuse, and dispatchable sweeps are load-bearing, and a declared discipline Compute cannot answer is a coverage defect.

One intent rail admits every execution request once, a substrate axis routes it over row data, bounded lanes carry it, and the one `ComputeReceipt` union records every outcome. It reads the `Rasm` kernel, the `Rasm.Element` seam, AppHost ports, and Persistence stores as settled lower-stratum vocabulary and never reverses the dependency onto the AEC-domain peers.

## [01]-[ROUTER]

[TENSOR]:
- [01]-[VOCABULARY](.planning/Tensor/vocabulary.md): CPU tensor vocabulary — shape, factory, dtype, and op-family axes every numeric lane reads.
- [02]-[LAYOUT](.planning/Tensor/layout.md): Layout forms and the one shape-edit request union.
- [03]-[DISPATCH](.planning/Tensor/dispatch.md): Arity-keyed kernel dispatch and the differentiable-adjoint law.
- [04]-[RESIDENCY](.planning/Tensor/residency.md): GPU residency lattice and the geometry-to-tensor encoding boundary.
- [05]-[MEMORY](.planning/Tensor/memory.md): Bounded staging memory and the zero-copy stream pool.
- [06]-[BLAS](.planning/Tensor/blas.md): Dense-BLAS, factorization, and spectral core.
- [07]-[FACTOR](.planning/Tensor/factor.md): Sparse ingestion and the criterion-stack iterative solve.
- [08]-[QUADRATURE](.planning/Tensor/quadrature.md): Accuracy-routed adaptive quadrature and the spectral operator.
- [09]-[SAMPLING](.planning/Tensor/sampling.md): Sobol/Halton sampling and radial-basis scatter reconstruction.

[SYMBOLIC]:
- [10]-[EXPRESSION](.planning/Symbolic/expression.md): CAS expression algebra over the `Entity` tree.
- [11]-[DIMENSIONAL](.planning/Symbolic/dimensional.md): ℚ⁷ SI base-dimension proof.
- [12]-[LOWERING](.planning/Symbolic/lowering.md): Content-keyed compiled-expression cache and the analytic-Jacobian arm.
- [13]-[UNITS](.planning/Symbolic/units.md): Units boundary admitting every unit-bearing input.

[MODEL]:
- [14]-[IDENTITY](.planning/Model/identity.md): Checksum model identity with acquisition, schema-snapshot, and drift-sentinel evidence.
- [15]-[SESSIONS](.planning/Model/sessions.md): One shared session per checksum with warm-start.
- [16]-[PROVIDERS](.planning/Model/providers.md): Execution-provider axis with discovery and quantization posture.
- [17]-[INFERENCE](.planning/Model/inference.md): Run-mode inference fold, cross-request batching gate, and result cache.
- [18]-[EMBEDDING](.planning/Model/embedding.md): Embedding-and-retrieval owner.
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

[RUNTIME]:
- [31]-[ADMISSION](.planning/Runtime/admission.md): Typed intent admission with the substrate axis and total dispatch.
- [32]-[SCHEDULING](.planning/Runtime/scheduling.md): Bounded work-lanes and the dependency job-graph scheduler.
- [33]-[PROGRESS](.planning/Runtime/progress.md): Monotonic phase family and the progress capsule.
- [34]-[RECEIPTS](.planning/Runtime/receipts.md): One `ComputeReceipt` fact union, its instrument projection, and the benchmark-claim table.
- [35]-[WIRE](.planning/Runtime/wire.md): Wire contract — proto vocabulary, evolution, and fault projection.
- [36]-[TRANSPORT](.planning/Runtime/transport.md): Channel mechanics — transport rows, tuning, and the artifact-frame law.
- [37]-[CODECS](.planning/Runtime/codecs.md): Field, result, and geometry-delta codecs and the tessellation bridge.
- [38]-[PAYLOAD](.planning/Runtime/payload.md): Residency-payload codec and the cluster-LOD chain.

[ANALYSIS]:
- [39]-[ASSESSMENT](.planning/Analysis/assessment.md): Lifecycle-aware assessment spine and reconciler.
- [40]-[AGGREGATOR](.planning/Analysis/aggregator.md): Multi-ply assembly aggregator over U, STC, GWP, and cost.
- [41]-[STRUCTURAL](.planning/Analysis/structural.md): Frame solve and the design-code capacity table.
- [42]-[PHYSICS](.planning/Analysis/physics.md): Closed-form thermal, acoustic, and fire folds.
- [43]-[ENERGY](.planning/Analysis/energy.md): Energy-route axis over the simulation toolchain.
- [44]-[LIFECYCLE](.planning/Analysis/lifecycle.md): Embodied-carbon and cost rollup over the EPD boundary.
- [45]-[CIRCULATION](.planning/Analysis/circulation.md): Egress and life-safety runner.
- [46]-[DAYLIGHT](.planning/Analysis/daylight.md): Solar-position kernel and sky-model daylight rows.

## [02]-[DOMAIN_PACKAGES]

Compute-domain libraries admitted by this folder; versions centralize in the C# manifest and corroborate against the adjacent `.api/`. Source-vendored solvers compile from pinned upstreams outside NuGet restore, their natives Forge-provisioned.

[NUMERIC_ACCELERATION]:
- `MathNet.Numerics.Providers.MKL` — MKL native `LinearProvider` backend.
- `MathNet.Numerics.Providers.OpenBLAS` — OpenBLAS native `LinearProvider` backend.
- `cslsqp` — source-vendored (oberbichler, ISC): span-based SLSQP the `OptimizerKind.slsqp` row binds.
- `HyperJet` — hyper-dual scalar AD backing exact gradient and Hessian across the `Sensitivity` family.
- `TorchSharp` — native ATen dense linear algebra and the iterative `EstimatorKind` autograd fits.
- `libtorch-cpu` — osx-arm64 native backend behind TorchSharp.
- `Silk.NET.WebGPU` — WebGPU compute-shader device substrate.
- `Silk.NET.WebGPU.Extensions.WGPU` — wgpu-native extension surface.

[SYMBOLIC_CAS]:
- `AngouriMath` — managed CAS: `Entity` parse, simplify, solve, integrate, differentiate, and `Compile<>` in one owner.
- `PeterO.Numbers` — exact-rational `ERational` carrier the ℚ⁷ dimension exponent vector stores.

[OPTIMIZATION]:
- `Google.OrTools` — CP-SAT and MILP exact optimization lane.
- `GeneticSharp` — evolutionary and metaheuristic `OptimizerKind` tier OR-Tools does not reach.
- `Microsoft.Z3` — SMT rule satisfaction returning SAT/UNSAT and unsat-core, where CP-SAT optimizes.

[MODEL_RUNTIME]:
- `Microsoft.ML.OnnxRuntime` — ONNX inference session core.
- `Microsoft.ML.OnnxRuntime.Extensions` — custom-op and string-tensor extension surface.
- `Microsoft.ML.OnnxRuntimeGenAI` — token-streaming generative run.
- `Microsoft.Extensions.Caching.Hybrid` — one `HybridCache` per lane.

[SIMULATION_CARBON]:
- `Microsoft.Data.Sqlite` — read-only eplusout.sql tabular reader.
- `NREL.OpenStudio.macOS-arm64` — in-process SWIG SDK lowering `ElementGraph` to OSM and IDF and reading `SqlFile`.
- `PollinationSDK` — `EnergyRoute.Cloud` transport onto the same `SqlFile` fold.
- `EC3` — openEPD REST service, no NuGet row: consumed hand-thin over `HttpClient`; the adjacent `.api` catalog is the integration contract.

[INTERCHANGE_TRANSPORT]:
- `SharpGLTF.Core` — glTF core read and write.
- `SharpGLTF.Ext.3DTiles` — 3D Tiles egress extension.
- `SharpGLTF.Toolkit` — mesh-building toolkit.
- `Alimer.Bindings.MeshOptimizer` — meshoptimizer simplification and cluster-LOD bindings.
- `Microsoft.IO.RecyclableMemoryStream` — pooled-buffer stream behind the artifact frames.
- `Grpc.Net.Client.Web` — gRPC-Web handler for HTTP/1.1 and browser-constrained paths.
- `Grpc.Net.Common` — shared compression and connectivity vocabulary beneath the gRPC rails.

## [03]-[SUBSTRATE_PACKAGES]

Shared substrate Compute consumes from the C# registry; the registry and its charters own the full contracts, and `libs/csharp/.api/` holds the shared API evidence.

[FUNCTIONAL_CORE]:
- `LanguageExt.Core`
- `Thinktecture.Runtime.Extensions`
- `Thinktecture.Runtime.Extensions.Json`
- `JetBrains.Annotations`

[NUMERIC]:
- `CommunityToolkit.HighPerformance`
- `CSparse` — managed sparse direct-factor terminal.
- `MathNet.Numerics` — quadrature, distributions, and the MKL/OpenBLAS provider hooks.
- `System.Numerics.Tensors`
- `UnitsNet`

[IDENTITY_TIME]:
- `NodaTime`
- `System.IO.Hashing`

[GRAPH_GEOMETRY]:
- `QuikGraph` — `Analysis/circulation` path and topology algebra over the space-adjacency view.
- `NetTopologySuite` — isovist and visibility polygons at the circulation planar boundary.
- `Clipper2` — corridor-clearance offset algebra at the same boundary.

[MODEL_ABSTRACTION]:
- `Microsoft.Extensions.AI.Abstractions` — `IChatClient` abstraction the AppHost provider binds.

[MAPPING_WIRE]:
- `Riok.Mapperly` — per-case proto↔domain transcription at the `Runtime/wire` boundary.
- `Generator.Equals` — `[Equatable]` structural equality where a class-root wire shape surrenders record equality.
- `Google.Protobuf`
- `Grpc.Net.Client`
- `Grpc.AspNetCore`
- `Grpc.Tools`
- `NodaTime.Serialization.Protobuf`

[TEST]:
Rows bind in branch test and benchmark projects, never the package csproj.
- `BenchmarkDotNet`
- `Microsoft.AspNetCore.TestHost` — in-memory server the `RemoteTransport.InProcess` row injects in the transport test harness.
