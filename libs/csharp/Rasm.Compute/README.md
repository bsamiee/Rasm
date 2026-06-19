# [COMPUTE]

`Rasm.Compute` is the APP-PLATFORM measured-execution package: it admits every execution request once, routes it over a substrate axis, carries it on bounded lanes, and records every outcome on one typed receipt union. It owns the CPU tensor lane, the dense and sparse numeric lane, the ONNX model lane with its generative token-streaming run, the suite wire vocabulary and content-address seed, the field/result and geometry-delta codecs with the two-hop tessellation bridge and 3D-Tiles partition, the discretization/solve/optimizer solver lane, staging memory, scheduling, monotonic progress, and the units boundary. It consumes the `Rasm` geometry kernel, AppHost ports, and Persistence stores as settled vocabulary and never reverses the dependency. This README routes the `.planning/` design pages and lists every external package the folder uses; the sub-domain map and domain law live in `ARCHITECTURE.md`, the forward concept pool in `IDEAS.md`, and the work in `TASKLOG.md`.

## [1]-[ROUTER]

The design pages under `.planning/`, grouped by sub-domain.

- intent: [admission](.planning/intent/admission.md)
- tensors: [vocabulary](.planning/tensors/vocabulary.md), [residency](.planning/tensors/residency.md), [layout](.planning/tensors/layout.md), [dispatch](.planning/tensors/dispatch.md)
- numeric: [algebra](.planning/numeric/algebra.md)
- symbolic: `expression` (planned — `T-SYMBOLIC-OWNER`), `dimensional` (planned — `T-SYMBOLIC-DIMENSIONAL`), `lowering` (planned — `T-SYMBOLIC-LOWERING`)
- models: [identity](.planning/models/identity.md), [sessions](.planning/models/sessions.md), [providers](.planning/models/providers.md), [extension-ops](.planning/models/extension-ops.md), [inference](.planning/models/inference.md), [generative](.planning/models/generative.md), `embedding` (planned — `T-EMBEDDING-LANE`)
- remote: [channels](.planning/remote/channels.md)
- interchange: [codecs](.planning/interchange/codecs.md), `residency` (planned — `T-STREAMING-RESIDENCY`)
- solver: [index](.planning/solver/index.md), [discretization](.planning/solver/discretization.md), [solve-contract](.planning/solver/solve-contract.md), [optimizer](.planning/solver/optimizer.md), [sweep](.planning/solver/sweep.md), [clash](.planning/solver/clash.md), `uncertainty` (planned — `T-UNCERTAINTY-LANE`)
- staging: [memory](.planning/staging/memory.md)
- scheduling: [runtime](.planning/scheduling/runtime.md)
- progress: [cell](.planning/progress/cell.md)
- units: [quantities](.planning/units/quantities.md)
- receipts: [union](.planning/receipts/union.md)

## [2]-[PACKAGES]

Every external library the folder uses, planned or implemented, as a flat list. Versions are centralized in the one C# manifest and never pinned here; admissions land here from the folder's ideas and tasks.

[TENSORS_NUMERIC]:
- System.Numerics.Tensors
- CommunityToolkit.HighPerformance
- MathNet.Numerics
- MathNet.Numerics.Providers.MKL
- MathNet.Numerics.Providers.OpenBLAS
- CSparse

[SYMBOLIC]:
- MathNet.Symbolics
- MathNet.Numerics.FSharp
- FParsec

[MODELS]:
- Microsoft.ML.OnnxRuntime
- Microsoft.ML.OnnxRuntime.Extensions
- Microsoft.ML.OnnxRuntime.Gpu
- Microsoft.ML.OnnxRuntime.DirectML
- Microsoft.ML.OnnxRuntimeGenAI
- Microsoft.Extensions.AI.Abstractions
- Microsoft.Extensions.Caching.Hybrid

[REMOTE_WIRE]:
- Google.Protobuf
- Grpc.Tools
- Grpc.Net.Client
- Grpc.Net.Client.Web
- Grpc.Net.Common
- Grpc.Core.Api
- Grpc.AspNetCore
- Grpc.AspNetCore.HealthChecks
- Grpc.AspNetCore.Web
- Microsoft.AspNetCore.TestHost
- NodaTime.Serialization.Protobuf

[INTERCHANGE_STAGING]:
- Microsoft.IO.RecyclableMemoryStream
- SharpGLTF.Core
- SharpGLTF.Ext.3DTiles
- SharpGLTF.Toolkit
- Alimer.Bindings.MeshOptimizer

[FUNCTIONAL_CORE]:
- LanguageExt.Core
- Thinktecture.Runtime.Extensions
- Thinktecture.Runtime.Extensions.Json
- NodaTime
- System.IO.Hashing

[UNITS]:
- UnitsNet

[TESTING]:
- SharpFuzz
- BenchmarkDotNet
