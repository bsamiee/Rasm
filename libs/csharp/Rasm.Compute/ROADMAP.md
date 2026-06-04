# [H1][RASM_COMPUTE_ROADMAP]

This roadmap sequences the build. The measured-compute core wraps `Rasm.Vectors` algorithms in `Eff<RT,ExecutionReceipt>`; model and remote lanes integrate with the concern that owns them.

## [1][PHASE_0]

- Add every core package to root `Directory.Packages.props` at the newest viable versions; project references stay versionless; no version numbers in this document. Adjacent packages for Compute: `CommunityToolkit.HighPerformance`, `UnitsNet`, `System.Reactive`. Scoped: `Microsoft.ML.OnnxRuntime`, `Grpc.Net.Client`, `Google.Protobuf`.
- Conditional (prove before adding): `Microsoft.ML.OnnxRuntime.Extensions` (custom geometry ops), `Grpc.Net.ClientFactory` (DI bootstrap), `Microsoft.IO.RecyclableMemoryStream` (proven `Stream` path only).
- Create `Rasm.Compute.csproj`, wire it into `Workspace.slnx` and the central build, and resolve host assemblies as the sibling libs do. Add a reference to the shared-contracts project (which carries `ComputeRequest`, `.proto`, and `Grpc.Tools`).
- Create `Rasm.Compute.Benchmarks.csproj` under `tests/csharp/libs/Rasm.Compute/`; reference `BenchmarkDotNet` (already centrally managed).
- Scaffold the flat-file skeleton: `Intent.cs`, `Substrate.cs`, `Model.cs`, `Remote.cs`, `Progress.cs` with canonical section order.
- Land the `IComputeRuntime` constraint (`CancellationToken`, `TimeProvider`) so Compute compiles and tests standalone.
- Call `NativeLibrary.SetDllImportResolver` for `libonnxruntime.dylib` and probe the `runtimes/osx-arm64/native/` path before any session.
- Configure the shared ORT thread pool (`OrtEnvironment.GetInstance().SetGlobalIntraOpNumThreads(cap)`, cap 2–4) once at process startup.

Phase 0 is complete when restore and build pass clean and the native dylib probe succeeds inside RhinoWIP.

## [2][MEASURED_CORE]

Build the compute rail with cancellation, measurement, and equivalence integrated:

| [INDEX] | [SURFACE]                                   | [BASIS]                                                         |
| :-----: | ------------------------------------------- | --------------------------------------------------------------- |
|   [1]   | Substrate selection predicate               | `SelectSubstrate(intent)` — ordered rule table in `_ARCHITECTURE.md` |
|   [2]   | Vectors boundary                            | Call `Rasm.Vectors`; wrap in `Eff<RT,ExecutionReceipt>`; zero kernel duplication |
|   [3]   | Cancellation / deadline / progress          | `RT.Token` → `RunOptions.Terminate`; `RT.Time` → elapsed; cold `Subject<ComputeProgress>` |
|   [4]   | Span/TensorPrimitives kernels               | `System.Numerics.Tensors`; stage via `MemoryOwner<T>` / `ArrayPoolBufferWriter<T>` |
|   [5]   | Allocation budget measurement               | `GC.GetAllocatedBytesForCurrentThread()` diff; overrun → `AllocationBudgetExceeded` receipt |
|   [6]   | Physical scalars                            | `UnitsNet` for all dimension-bearing inputs and outputs         |
|   [7]   | Baseline timing and equivalence receipts    | `BenchmarkReceipt` with `BenchmarkDotNet` evidence              |
|   [8]   | Failure taxonomy                            | Typed `ExecutionReceipt.Failure` discriminant                   |

## [3][SCOPED_LANES]

| [INDEX] | [LANE]               | [INTEGRATES_WITH]                                                                                                            |
| :-----: | -------------------- | ---------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | ONNX Runtime model lane | `Microsoft.ML.OnnxRuntime` (newest; CoreML EP bundled for `osx-arm64`); `AppendExecutionProvider("CoreML", dict)` API; `InferenceSession` lease via `Atom<HashMap<ModelKey, SemaphoreSlim>>`; content-hash key; Persistence cache lookup |
|   [2]   | CoreML ANE safety    | Validate new models against `CPUOnly` with `AllowLowPrecisionAccumulationOnGPU=0`; record `ModelReceipt.Fp16Downcast`       |
|   [3]   | gRPC companion lane  | `Grpc.Net.Client` + `Google.Protobuf`; `.proto` in shared-contracts project; `SocketsHttpHandler` keep-alive configuration  |
|   [4]   | Remote retry policy  | AppHost outbound-hop ownership; Compute emits `RemoteReceipt.RetryOwnerConflict` if a second retry owner appears            |

## [4][MEASUREMENT_EVIDENCE]

Performance claims are scoped to the measured input class. Receipts identify substrate, input class, output equivalence, timing, allocation category, cancellation, timeout, and failure path. Benchmark artifacts land under `.artifacts/compute/benchmarks/<substrate>/<run-id>/`; Persistence indexes them. CoreML fp16 downcast is a declared equivalence caveat on `ModelReceipt` — not silently ignored.
