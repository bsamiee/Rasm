# [COMPUTE]

`Rasm.Compute` is the APP-PLATFORM measured-execution package. It admits every execution request once, routes it over a substrate axis, carries it on bounded lanes, and records every outcome on one typed receipt union. The Tensor folder owns the CPU tensor vocabulary, GPU residency, staging memory, and the dense-BLAS/sparse-factor/quadrature/sampling numeric core. The Symbolic folder owns the CAS expression tree, dimensional proof, lowering cache, and units boundary. The Model folder owns `ONNX` identity, sessions, providers, inference, embedding retrieval, the generative token-streaming run, and custom-op extension. The Solver folder owns the discretization, contract, optimizer, sweep, clash, and uncertainty lanes. The Stats folder owns the classical-statistics/statistical-learning estimator axis and the DSP signal axis. The Runtime folder owns the admission rail, scheduling, monotonic progress, receipt union, wire channels, and the field/result/geometry-delta codecs with the GPU-ready residency payload. The Analysis folder owns the C#-first discipline-assessment rail: one `AssessmentRequest` routed over the seam `Discipline` to a structural, physics, energy, or lifecycle runner. `Rasm.Compute` consumes the `Rasm` geometry kernel, the `Rasm.Element` element seam (the shared lower stratum it reads upward, never the AEC-domain peers `Rasm.Materials`/`Rasm.Bim`), AppHost ports, and Persistence stores as settled vocabulary and never reverses the dependency. The folder map and domain law live in `ARCHITECTURE.md`, the forward concept pool in `IDEAS.md`, and the work log in `TASKLOG.md`.

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
- [23]-[OPTIMIZER](.planning/Solver/optimizer.md)
- [24]-[SWEEP](.planning/Solver/sweep.md)
- [25]-[CLASH](.planning/Solver/clash.md)
- [26]-[UNCERTAINTY](.planning/Solver/uncertainty.md)
- [27]-[ESTIMATOR](.planning/Stats/estimator.md)
- [28]-[SIGNAL](.planning/Stats/signal.md)
- [29]-[ADMISSION](.planning/Runtime/admission.md)
- [30]-[SCHEDULING](.planning/Runtime/scheduling.md)
- [31]-[PROGRESS](.planning/Runtime/progress.md)
- [32]-[RECEIPTS](.planning/Runtime/receipts.md)
- [33]-[CHANNELS](.planning/Runtime/channels.md)
- [34]-[CODECS](.planning/Runtime/codecs.md)
- [35]-[PAYLOAD](.planning/Runtime/payload.md)
- [36]-[ASSESSMENT](.planning/Analysis/assessment.md)
- [37]-[AGGREGATOR](.planning/Analysis/aggregator.md)
- [38]-[STRUCTURAL](.planning/Analysis/structural.md)
- [39]-[PHYSICS](.planning/Analysis/physics.md)
- [40]-[ENERGY](.planning/Analysis/energy.md)
- [41]-[LIFECYCLE](.planning/Analysis/lifecycle.md)

## [02]-[DOMAIN_PACKAGES]

Every Compute-domain library the folder uses, planned or implemented. Versions are centralized in the one C# manifest and never pinned here; API evidence lives in the adjacent `.api/` folder. `GeneticSharp` carries the evolutionary/genetic-algorithm tier of the Solver `OptimizerKind` rows that the exact `Google.OrTools` CP-SAT/MILP lane does not reach — the chromosome encodings, selection/crossover/mutation/termination operator catalog, and multithreaded executor behind the metaheuristic optimizer cases. `TorchSharp` (with its `libtorch-cpu` meta-backend selecting the `libtorch-cpu-osx-arm64` native dylib) is the dual-leg `[CLASSICAL_ML_BLAS]` owner: it backs the iterative `Stats/estimator` `EstimatorKind` rows (Lasso, GLM/IRLS, kernel-SVM, GMM/EM, NMF, clustering, ARMA-MLE) via `torch.linalg` + autograd + `torch.optim`, and it supplies the native osx-arm64 dense linear-algebra substrate (native ATen GEMM/factorization) for the `Tensor/blas` lane, retaining the managed MathNet terminal as the cold-start path.

[TENSOR_NUMERIC]:
- `MathNet.Numerics`
- `MathNet.Numerics.Providers.MKL`
- `MathNet.Numerics.Providers.OpenBLAS`
- `CSparse`

[SYMBOLIC]:
- `MathNet.Symbolics`
- `FParsec`

[OPTIMIZATION]:
- `Google.OrTools`
- `GeneticSharp`

[STRUCTURAL_SOLVERS]:
- `FEALiTE2D` — 2D frame/beam/truss solver backend for the Solver discretization lane, the `Analysis/structural` planar `FrameBackend`, and structural-analysis receipts.
- `FEALiTE2D.Plotting` — DXF export for FEALiTE2D internal-force, shear-force, bending-moment, and displacement diagrams as solver evidence artifacts.
- `BriefFiniteElement.Net` — the 3D `Analysis/structural` `FrameBackend`, confined to the sparse-factored `BarElement` frame path (its embedded dense `DenseLU` is binary-incompatible with the unified `CSparse 4.4.0` pin); the linear solve injects the Rasm CSparse-4.x `ISolverFactory` via `Solve(ISolverFactory)`, sharing the one `Tensor/factor` factorization owner. The `Analysis/structural` AISC 360 / EN 1993 / EN 1992 / NDS / ACI 318 / TMS 402 / AISI S100 design-code checks are HAND-ROLLED as a `DesignCode`×`LimitState` capacity table (no .NET package owns them); section properties resolve once via the `Rasm.Materials` VividOrange M7 one-hop and ride the seam graph, so Compute admits no VividOrange.
- `BriefFiniteElementNet.CustomElements`

[ENERGY_SIMULATION]:
- `NREL.OpenStudio.macOS-arm64` — the osx-arm64 SWIG OpenStudio SDK the `Analysis/energy` runner drives in-process: it BUILDS the OSM `Model` from the `Rasm.Element` `ElementGraph`, forward-translates to an EnergyPlus IDF via `EnergyPlusForwardTranslator`, and reads the results `SqlFile`; it neither runs nor bundles the EnergyPlus solver. The EnergyPlus binary is a PARAMETERIZED subprocess resolved through `EnergyToolchain` (env-var → configured-path → bundled-fallback) and version-locked to the OpenStudio SWIG version. This is the SIMULATION concern, distinct from the `Rasm.Bim/Energy/exchange` energy-model exchange owner.
- `PollinationSDK` — the Pollination cloud-compute transport backing the `Analysis/energy` `EnergyRoute.Cloud` provider row: the `Wrapper` job/run/asset orchestration (`JobInfo.RunJobAsync` → `ScheduledJobInfo.WatchJobStatusAsync` → `RunInfo.GetOutputAssets`/`DownloadRunAssetsAsync`) converging on the same `SqlFile` result fold the local subprocess uses. Sidecar-isolated (its vendored `LBT.RestSharp`/`LBT.Newtonsoft.Json` closure never meets the STJ rails and never loads in-Rhino); token auth is composition-root input; the durable half (presigned-grant object transfer, `ArtifactKind.CloudRun` reuse index, PROV attribution) is `Rasm.Persistence`'s, composed at the seam; catalog at `Rasm.Persistence/.api/api-pollination-sdk.md` (the Compute-scoped orchestration twin is pending).

[EMBODIED_CARBON]:
- `EC3` / openEPD REST — Building Transparency embodied-carbon service consumed hand-thin over `HttpClient` (no NuGet pin; API evidence in `.api/api-ec3.md`); the EN 15978 LCA Assessment lane reads per-EPD `gwp` measurements (kgCO2e per declared unit) and category GWP statistics, then writes content-keyed `Node.Assessment` nodes on the (input subgraph, route, carbon policy) key the `Analysis/assessment` spine mints.

[CLASSICAL_ML_BLAS]:
- `TorchSharp`
- `libtorch-cpu`

[GPU_COMPUTE]:
- `Silk.NET.WebGPU`
- `Silk.NET.WebGPU.Extensions.WGPU`

[ML_RUNTIME]:
- `Microsoft.ML.OnnxRuntime`
- `Microsoft.ML.OnnxRuntime.Extensions`
- `Microsoft.ML.OnnxRuntime.Gpu`
- `Microsoft.ML.OnnxRuntime.DirectML`
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
- `System.Numerics.Tensors`
- `UnitsNet`

[WIRE_CODEGEN]:
- `Google.Protobuf`
- `Grpc.Net.Client`
- `Grpc.Net.Client.Web`
- `Grpc.Net.Common`
- `Grpc.AspNetCore`
- `Grpc.Tools`
- `NodaTime.Serialization.Protobuf`

[TEST_SUBSTRATE]:
- `BenchmarkDotNet`
