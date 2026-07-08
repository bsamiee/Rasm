# [COMPUTE]

`Rasm.Compute` is the APP-PLATFORM measured-execution package. It admits every execution request once, routes it over a substrate axis, carries it on bounded lanes, and records every outcome on one typed receipt union. The Tensor folder owns the CPU tensor vocabulary, GPU residency, staging memory, and the dense-BLAS/sparse-factor/quadrature/sampling numeric core. The Symbolic folder owns the CAS expression tree, dimensional proof, lowering cache, and units boundary. The Model folder owns `ONNX` identity, sessions, providers, inference, embedding retrieval, the generative token-streaming run, and custom-op extension. The Solver folder owns the discretization, contract, optimizer, sweep, clash, and uncertainty lanes. The Stats folder owns the classical-statistics/statistical-learning estimator axis and the DSP signal axis. The Runtime folder owns the admission rail, scheduling, monotonic progress, receipt union, the wire CONTRACT (proto vocabulary, contract evolution, fault projection) and the TRANSPORT mechanics (channels, call policy, artifact frames) as two pages, and the field/result/geometry-delta codecs with the GPU-ready residency payload. The Solver folder additionally owns the per-Gauss-point constitutive axis and the Z3 rule-satisfaction owner. The Analysis folder owns the C#-first discipline-assessment rail: one `AssessmentRequest` routed over the seam `Discipline` to a structural, physics, energy, lifecycle, circulation, or daylight runner, reconciled by the lifecycle-aware `Sweep` over the `JobGraph`. `Rasm.Compute` consumes the `Rasm` geometry kernel, the `Rasm.Element` element seam (the shared lower stratum it reads upward, never the AEC-domain peers `Rasm.Materials`/`Rasm.Bim`), AppHost ports, and Persistence stores as settled vocabulary and never reverses the dependency. The folder map and domain law live in `ARCHITECTURE.md`, the forward concept pool in `IDEAS.md`, and the work log in `TASKLOG.md`.

## [01]-[ROUTER]

- [01]-[VOCABULARY](.planning/Tensor/vocabulary.md)
- [02]-[LAYOUT](.planning/Tensor/layout.md)
- [03]-[DISPATCH](.planning/Tensor/dispatch.md)
- [04]-[RESIDENCY](.planning/Tensor/residency.md)
- [05]-[MEMORY](.planning/Tensor/memory.md)
- [06]-[BLAS](.planning/Tensor/blas.md)
- [07]-[FACTOR](.planning/Tensor/factor.md)
- [08]-[QUADRATURE](.planning/Tensor/quadrature.md)
- [09]-[SAMPLING](.planning/Tensor/sampling.md)
- [10]-[EXPRESSION](.planning/Symbolic/expression.md)
- [11]-[DIMENSIONAL](.planning/Symbolic/dimensional.md)
- [12]-[LOWERING](.planning/Symbolic/lowering.md)
- [13]-[UNITS](.planning/Symbolic/units.md)
- [14]-[IDENTITY](.planning/Model/identity.md)
- [15]-[SESSIONS](.planning/Model/sessions.md)
- [16]-[PROVIDERS](.planning/Model/providers.md)
- [17]-[INFERENCE](.planning/Model/inference.md)
- [18]-[EMBEDDING](.planning/Model/embedding.md)
- [19]-[GENERATIVE](.planning/Model/generative.md)
- [20]-[EXTENSION](.planning/Model/extension.md)
- [21]-[DISCRETIZATION](.planning/Solver/discretization.md)
- [22]-[CONTRACT](.planning/Solver/contract.md)
- [23]-[CONSTITUTIVE](.planning/Solver/constitutive.md)
- [24]-[OPTIMIZER](.planning/Solver/optimizer.md)
- [25]-[SWEEP](.planning/Solver/sweep.md)
- [26]-[CLASH](.planning/Solver/clash.md)
- [27]-[UNCERTAINTY](.planning/Solver/uncertainty.md)
- [28]-[SATISFY](.planning/Solver/satisfy.md)
- [29]-[ESTIMATOR](.planning/Stats/estimator.md)
- [30]-[SIGNAL](.planning/Stats/signal.md)
- [31]-[ADMISSION](.planning/Runtime/admission.md)
- [32]-[SCHEDULING](.planning/Runtime/scheduling.md)
- [33]-[PROGRESS](.planning/Runtime/progress.md)
- [34]-[RECEIPTS](.planning/Runtime/receipts.md)
- [35]-[WIRE](.planning/Runtime/wire.md)
- [36]-[TRANSPORT](.planning/Runtime/transport.md)
- [37]-[CODECS](.planning/Runtime/codecs.md)
- [38]-[PAYLOAD](.planning/Runtime/payload.md)
- [39]-[ASSESSMENT](.planning/Analysis/assessment.md)
- [40]-[AGGREGATOR](.planning/Analysis/aggregator.md)
- [41]-[STRUCTURAL](.planning/Analysis/structural.md)
- [42]-[PHYSICS](.planning/Analysis/physics.md)
- [43]-[ENERGY](.planning/Analysis/energy.md)
- [44]-[LIFECYCLE](.planning/Analysis/lifecycle.md)
- [45]-[CIRCULATION](.planning/Analysis/circulation.md)
- [46]-[DAYLIGHT](.planning/Analysis/daylight.md)

## [02]-[DOMAIN_PACKAGES]

Every Compute-domain library the folder uses, planned or implemented. Versions are centralized in the one C# manifest and never pinned here; API evidence lives in the adjacent `.api/` folder. `GeneticSharp` carries the evolutionary/genetic-algorithm tier of the Solver `OptimizerKind` rows that the exact `Google.OrTools` CP-SAT/MILP lane does not reach — the chromosome encodings, selection/crossover/mutation/termination operator catalog, and multithreaded executor behind the metaheuristic optimizer cases. `TorchSharp` (with its `libtorch-cpu` meta-backend selecting the `libtorch-cpu-osx-arm64` native dylib) is the dual-leg `[CLASSICAL_ML_BLAS]` owner: it backs the iterative `Stats/estimator` `EstimatorKind` rows (Lasso, GLM/IRLS, kernel-SVM, GMM/EM, NMF, clustering, ARMA-MLE) via `torch.linalg` + autograd + `torch.optim`, and it supplies the native osx-arm64 dense linear-algebra substrate (native ATen GEMM/factorization) for the `Tensor/blas` lane, retaining the managed MathNet terminal as the cold-start path.

[TENSOR_NUMERIC]:
- `MathNet.Numerics.Providers.MKL`
- `MathNet.Numerics.Providers.OpenBLAS`
- `CSparse`

[SYMBOLIC]:
- `AngouriMath` — the categorical-best managed CAS: `Entity` parse/simplify/solve/integrate/differentiate/limit/LaTeX/`Compile<>` in one owner; replaced the four-dependency MathNet.Symbolics F# stack.
- `PeterO.Numbers` — the exact-rational `ERational` carrier the `Symbolic/dimensional` Q⁷ exponent vector stores (the AngouriMath number tower's own arithmetic).

[OPTIMIZATION]:
- `Google.OrTools`
- `GeneticSharp`

[AUTODIFF]:
- `HyperJet` — the hyper-dual scalar-AD THIRD leg of the one `Sensitivity` family: exact gradient/Hessian for the estimator temporal fits, FORM/SORM, the Levenberg-Marquardt canonical Jacobian arm, and the slsqp `Smooth` objective — the finite-difference fall is deleted.

[SATISFACTION]:
- `Microsoft.Z3` — SMT rule-satisfaction: `SymbolicExpr` lowered to NRA/NIA assertions returning SAT/UNSAT + unsat-core; verifies where CP-SAT optimizes

[STRUCTURAL_SOLVERS]:
- `csparse-interop` — SOURCE-VENDORED (wo80, BSD-3; not on NuGet): the ARPACK shift-invert sparse eigensolver (`fea-modal`/`fea-buckling`/seismic at building DOF) and the native CHOLMOD/SuperLU/UMFPACK direct rows beside the managed CSparse terminals; natives Forge-provisioned, fault-at-init. The BFE/FEALiTE frame backends are RETIRED — the owned beam/frame `ElementClass` rows on the `Solver/contract` spine replaced them.
- `cslsqp` — SOURCE-VENDORED (oberbichler, ISC; feed-verified absent from NuGet): the span-based SLSQP the `OptimizerKind.slsqp` smooth constrained-NLP row binds.

[ENERGY_SIMULATION]:
- `Microsoft.Data.Sqlite` — the read-only eplusout.sql TABULAR reader (the `TabularDataWithStrings` setpoint-not-met rows the SWIG `SqlFile` exposes no accessor for).
- `NREL.OpenStudio.macOS-arm64` — in-process SWIG SDK: `ElementGraph` → OSM → IDF, reads `SqlFile`; EnergyPlus runs as the `EnergyToolchain` subprocess.
- `PollinationSDK` — `EnergyRoute.Cloud` transport, `Wrapper` orchestration onto the same `SqlFile` fold; sidecar-isolated, durable half Persistence-owned.

[EMBODIED_CARBON]:
- `EC3` — openEPD REST consumed hand-thin over `HttpClient`, no pin; the EN 15978 lane reads per-EPD `gwp` and writes content-keyed `Node.Assessment` rows.

[CLASSICAL_ML_BLAS]:
- `TorchSharp`
- `libtorch-cpu`

[GPU_COMPUTE]:
- `Silk.NET.WebGPU`
- `Silk.NET.WebGPU.Extensions.WGPU`

[ML_RUNTIME]:
- `Microsoft.ML.OnnxRuntime`
- `Microsoft.ML.OnnxRuntime.Extensions`
- `Microsoft.ML.OnnxRuntimeGenAI`

[INTERCHANGE]:
- `SharpGLTF.Core`
- `SharpGLTF.Ext.3DTiles`
- `SharpGLTF.Toolkit`
- `Alimer.Bindings.MeshOptimizer`

[PERF]:
- `Microsoft.IO.RecyclableMemoryStream`

[CACHE_AI]:
- `Microsoft.Extensions.Caching.Hybrid`
- `Microsoft.Extensions.AI.Abstractions`

[WIRE_CLIENT]:
- `Grpc.Net.Client.Web` — the gRPC-Web message handler translating client calls to `application/grpc-web`/`application/grpc-web-text` for HTTP/1.1 and browser-constrained paths.
- `Grpc.Net.Common` — the shared compression-provider contracts and connectivity vocabulary beneath the `Grpc.Net.Client`/`Grpc.AspNetCore` rails.
- `Microsoft.AspNetCore.TestHost` — test-only: the in-memory server whose `CreateHandler` handler the `RemoteTransport.InProcess` row injects into `GrpcChannelOptions.HttpHandler`; binds in the transport test harness, never this csproj.

## [03]-[SUBSTRATE_PACKAGES]

Cross-cutting C# substrate libraries Compute consumes; package charters live in `libs/csharp/.planning/README.md` and shared API evidence lives in `libs/csharp/.api/`.

[FUNCTIONAL_CORE]:
- `LanguageExt.Core`
- `Thinktecture.Runtime.Extensions`
- `Thinktecture.Runtime.Extensions.Json`
- `JetBrains.Annotations`

[TIME_IDENTITY]:
- `NodaTime`
- `System.IO.Hashing`

[NUMERIC_SUBSTRATE]:
- `CommunityToolkit.HighPerformance`
- `MathNet.Numerics` — solver quadrature, distributions, and the MKL/OpenBLAS provider hooks admitted as folder additions
- `System.Numerics.Tensors`
- `UnitsNet`

[GRAPH_ALGORITHM]:
- `QuikGraph` — the `Analysis/circulation` path/topology algebra (Dijkstra/A*, SCC) over the per-request space-adjacency view.

[PLANAR_GEOMETRY]:
- `NetTopologySuite` — isovist/visibility polygons and occupant areas at the circulation planar boundary (float production-plane, never a second exact rail).
- `Clipper2` — the corridor-clearance offset algebra (`InflatePaths` collapse test) at the same boundary.

[BOUNDARY_GENERATORS]:
- `Riok.Mapperly` — the per-case proto↔domain field transcription at the `Runtime/wire` oneof boundary.
- `Generator.Equals` — `[Equatable]` structural equality where a class-root wire shape surrenders record-root equality.

[WIRE_CODEGEN]:
- `Google.Protobuf`
- `Grpc.Net.Client`
- `Grpc.AspNetCore`
- `Grpc.Tools`
- `NodaTime.Serialization.Protobuf`

[TEST_SUBSTRATE]:
Rows bind in the branch benchmark project, never the package csproj.
- `BenchmarkDotNet`
