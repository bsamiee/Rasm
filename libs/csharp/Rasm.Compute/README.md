# [COMPUTE]

`Rasm.Compute` is the APP-PLATFORM measured-execution package. It admits every execution request once, routes it over a substrate axis, carries it on bounded lanes, and records every outcome on one typed receipt union. The Tensor folder owns the CPU tensor vocabulary, GPU residency, staging memory, and the dense-BLAS/sparse-factor/quadrature/sampling numeric core. The Symbolic folder owns the CAS expression tree, dimensional proof, lowering cache, and units boundary. The Model folder owns `ONNX` identity, sessions, providers, and inference, including the generative token-streaming run and embedding retrieval. The Solver folder owns the discretization, contract, optimizer, sweep, clash, and uncertainty lanes. The Runtime folder owns the admission rail, scheduling, monotonic progress, receipt union, wire channels, and the field/result/geometry-delta codecs with the GPU-ready residency payload. `Rasm.Compute` consumes the `Rasm` geometry kernel, AppHost ports, and Persistence stores as settled vocabulary and never reverses the dependency. The folder map and domain law live in `ARCHITECTURE.md`, the forward concept pool in `IDEAS.md`, and the work log in `TASKLOG.md`.

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
- [27]-[ADMISSION](.planning/Runtime/admission.md)
- [28]-[SCHEDULING](.planning/Runtime/scheduling.md)
- [29]-[PROGRESS](.planning/Runtime/progress.md)
- [30]-[RECEIPTS](.planning/Runtime/receipts.md)
- [31]-[CHANNELS](.planning/Runtime/channels.md)
- [32]-[CODECS](.planning/Runtime/codecs.md)
- [33]-[PAYLOAD](.planning/Runtime/payload.md)

## [02]-[DOMAIN_PACKAGES]

Every Compute-domain library the folder uses, planned or implemented. Versions are centralized in the one C# manifest and never pinned here; API evidence lives in the adjacent `.api/` folder.

[TENSOR_NUMERIC]:
- `CommunityToolkit.HighPerformance`
- `MathNet.Numerics`
- `MathNet.Numerics.Providers.MKL`
- `MathNet.Numerics.Providers.OpenBLAS`
- `CSparse`
- `Silk.NET.WebGPU`

[SYMBOLIC]:
- `MathNet.Symbolics`
- `FParsec`

[OPTIMIZATION]:
- `Google.OrTools`

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

[UNITS]:
- `UnitsNet`

[CACHE_AI]:
- `Microsoft.Extensions.Caching.Hybrid`
- `Microsoft.Extensions.AI.Abstractions`

## [03]-[SUBSTRATE_PACKAGES]

Cross-cutting C# substrate libraries Compute consumes; these are owned at the monorepo substrate layer. Package charters and API evidence live in `libs/csharp/.planning/README.md` and the adjacent `.api/` folder.

[FUNCTIONAL_CORE]:
- `LanguageExt.Core`
- `Thinktecture.Runtime.Extensions`
- `Thinktecture.Runtime.Extensions.Json`
- `JetBrains.Annotations`

[TIME_IDENTITY]:
- `NodaTime`
- `System.IO.Hashing`

[NUMERIC_SUBSTRATE]:
- `System.Numerics.Tensors`

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
- `SharpFuzz`
