# [COMPUTE]

`Rasm.Compute` is the APP-PLATFORM measured-execution package. It admits every execution request once, routes it over a substrate axis, carries it on bounded lanes, and records every outcome on one typed receipt union. The Tensor folder owns the CPU tensor vocabulary, GPU residency, staging memory, and the dense-BLAS/sparse-factor/quadrature/sampling numeric core. The Symbolic folder owns the CAS expression tree, dimensional proof, lowering cache, and units boundary. The Model folder owns `ONNX` identity, sessions, providers, inference, embedding retrieval, the generative token-streaming run, and custom-op extension. The Solver folder owns the discretization, contract, optimizer, sweep, clash, and uncertainty lanes. The Stats folder owns the classical-statistics/statistical-learning estimator axis and the DSP signal axis. The Runtime folder owns the admission rail, scheduling, monotonic progress, receipt union, wire channels, and the field/result/geometry-delta codecs with the GPU-ready residency payload. `Rasm.Compute` consumes the `Rasm` geometry kernel, AppHost ports, and Persistence stores as settled vocabulary and never reverses the dependency. The folder map and domain law live in `ARCHITECTURE.md`, the forward concept pool in `IDEAS.md`, and the work log in `TASKLOG.md`.

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
- `FEALiTE2D` — 2D frame/beam/truss solver backend for the Solver discretization lane and structural-analysis receipts.
- `FEALiTE2D.Plotting` — DXF export for FEALiTE2D internal-force, shear-force, bending-moment, and displacement diagrams as solver evidence artifacts.
- `BriefFiniteElement.Net`
- `BriefFiniteElementNet.CustomElements`

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
