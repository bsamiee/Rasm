# [COMPUTE]

`Rasm.Compute` is the APP-PLATFORM measured-execution package: it admits every execution request once, routes it over a substrate axis, carries it on bounded lanes, and records every outcome on one typed receipt union. It owns the CPU tensor lane, the dense and sparse numeric lane, the ONNX model lane with its generative token-streaming run, the suite wire vocabulary and content-address seed, the field/result and geometry-delta codecs with the two-hop tessellation bridge and 3D-Tiles partition, the discretization/solve/optimizer solver lane, staging memory, scheduling, monotonic progress, and the units boundary. It consumes the `Rasm` geometry kernel, AppHost ports, and Persistence stores as settled vocabulary and never reverses the dependency. This README routes the `.planning/` design pages and lists every external package the folder uses; the sub-domain map and domain law live in `ARCHITECTURE.md`, the forward concept pool in `IDEAS.md`, and the work in `TASKLOG.md`.

## [1]-[ROUTER]

The design pages under `.planning/`, grouped by sub-domain.

- intent: [admission](.planning/intent/admission.md)
- tensors: [operations](.planning/tensors/operations.md)
- numeric: [algebra](.planning/numeric/algebra.md)
- models: [sessions](.planning/models/sessions.md)
- remote: [channels](.planning/remote/channels.md)
- interchange: [codecs](.planning/interchange/codecs.md)
- solver: [lane](.planning/solver/lane.md)
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
- Grpc.AspNetCore
- Microsoft.AspNetCore.TestHost
- NodaTime.Serialization.Protobuf

[INTERCHANGE_STAGING]:
- Microsoft.IO.RecyclableMemoryStream
- SharpGLTF.Core
- SharpGLTF.Toolkit
- meshoptimizer

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
