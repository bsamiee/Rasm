# [H1][RASM_COMPUTE_AGENTS]
>**Dictum:** *Measure before promoting compute surface.*

<br>

[CRITICAL] Build `Rasm.Compute` now as one measured-execution platform. Call `Rasm.Vectors` for all tensor/numeric work; wrap in `Eff<RT,ExecutionReceipt>`; add no algorithm that `Rasm.Vectors` already owns. Add package references centrally, create the `.csproj`, scaffold the folder structure and the shared-contracts project in Phase 0, then build the measured core. Add model and remote lanes when their assets and contracts exist.

---
## [1][OWNER_CONTRACT]
>**Dictum:** *Compute owns execution evidence, not domain meaning.*

<br>

- Call `Rasm.Vectors` for all tensor/numeric work. Zero kernel duplication — Compute only wraps in `Eff<RT,ExecutionReceipt>`, adds timing/allocation/cancellation/substrate-selection/progress.
- Use `System.Numerics.Tensors` (in-box net10.0, no pin) for span/`TensorPrimitives` calls; stage buffers with `MemoryOwner<T>` / `ArrayPoolBufferWriter<T>` (`CommunityToolkit.HighPerformance`).
- Use `UnitsNet` for all dimension-bearing CAD scalars; no bare `double` for length, angle, or force.
- Own `IObservable<ComputeProgress>` via a cold `Subject<ComputeProgress>`; expose `.AsObservable()`; never call `ObserveOn` — AppUi calls `ObserveOn(RasmUiScheduler.RxScheduler)` at its boundary. Emit `OnCompleted` on success and cancellation. Never emit `OnError`; faults surface via `ExecutionReceipt.Failure` only. GH2 no-subscriber = cold no-op at zero cost.
- Use ONNX Runtime (newest; CoreML EP bundled for `osx-arm64`) for the model lane: versioned `.onnx` assets and `InferenceSession` lifecycle. Use the current `AppendExecutionProvider("CoreML", dict)` API — the old native P/Invoke was deprecated in ORT 1.20. No ML.NET; no TorchSharp.
- Validate all new models against `CPUOnly` EP with `AllowLowPrecisionAccumulationOnGPU=0`; record fp16 downcast on `ModelReceipt.Fp16Downcast`. Set `ModelCacheDirectory` on every session options object.
- Create one `InferenceSession` per `ModelKey` behind a per-key `SemaphoreSlim` (`Atom<HashMap<ModelKey, SemaphoreSlim>>`), always off the Rhino UI thread. Cap `IntraOpNumThreads` at 2–4 via the shared global ORT thread pool configured once at process startup. Use synchronous `Run` + `RunOptions.Terminate` via `token.Register`; lift via `liftIO`/`Eff.liftIO`. Never `Task.Run`.
- Compute content-hash–keyed `ModelKey`; look up Persistence cache before inference; emit `ModelReceipt.CacheHit`.
- Use `Grpc.Net.Client` for out-of-process companion compute; configure `SocketsHttpHandler` keep-alive to prevent idle-connection faults.
- `ComputeRequest` is defined in the shared-contracts project, not in `Rasm.Compute`. Reference that project; do not define `ComputeRequest` locally.
- Operations are `Eff<RT, ExecutionReceipt>` on `IComputeRuntime` (minimal) / `RasmRuntime` (full); own no executor; AppHost owns the `Channel<ComputeRequest>`, drains it on its background scheduler, and calls Compute to execute each drained request.
- Measure allocation per operation with `GC.GetAllocatedBytesForCurrentThread()` diff; overrun → `AllocationBudgetExceeded` receipt case.
- GH2 surfacing: `GH_TaskCapableComponent` for in-process; `Channel<ComputeRequest>` + cached receipt + `ExpireSolution(true)` for model/remote (> 100 ms). Never block the solve thread on ORT `Run` or gRPC.

---
## [2][BOUNDARY_RULES]
>**Dictum:** *Scheduling is not execution semantics.*

<br>

| [INDEX] | [BOUNDARY]          | [RULE]                                                                                          |
| :-----: | ------------------- | ----------------------------------------------------------------------------------------------- |
|   [1]   | `Rasm.Vectors`      | Owns all tensor/numeric algorithm bodies; Compute calls, never duplicates                       |
|   [2]   | `Rasm.AppHost`      | Owns `Channel<ComputeRequest>` writer and executor; owns remote retry; owns drain order         |
|   [3]   | `Rasm.Compute`      | Owns substrate selection predicate, lifecycle lease, receipt shapes, progress emission          |
|   [4]   | Shared-contracts    | Owns `ComputeRequest`, `.proto`, `Grpc.Tools`; both AppHost and Compute reference it           |
|   [5]   | `Rasm.Persistence`  | Indexes benchmark artifacts; serves `Option<ModelResult>` cache lookup                         |
|   [6]   | `Rasm.AppUi`        | Calls `ObserveOn(RasmUiScheduler.RxScheduler)` on `IObservable<ComputeProgress>`; Compute never schedules UI |
|   [7]   | Remote service      | Exists only through companion contract; Compute is client-only                                 |
|   [8]   | Benchmarks          | Required for speed/allocation claims; live in `Rasm.Compute.Benchmarks.csproj`                 |

---
## [3][EVIDENCE]
>**Dictum:** *Source slices produce measurement evidence.*

<br>

Executable proof comes from source, benchmarks, and runtime scenarios. Evidence categories:

- Output equivalence against baseline (incl. fp16 downcast validation for CoreML ANE).
- Benchmark timing and allocation profile under `.artifacts/compute/benchmarks/`.
- Cancellation, timeout, and failure receipts.
- `MemoryOwner<T>` staging; allocation-budget measurement and overrun receipt.
- `UnitsNet` typed scalars at domain boundary.
- Model identity / version / content hash / load / dispose / `InferenceSession` lease receipts.
- gRPC deadline / payload / retry-owner / keep-alive failure receipts.
- Native dylib probe (`NativeLibrary.SetDllImportResolver`) success receipt before first session.
- GH2 component non-blocking evidence (`GH_TaskCapableComponent` + `ExpireSolution` path).
- `ComputeProgress` emission sequence: monotone fraction, `OnCompleted` on success/cancel, no `OnError`.

---
## [4][REJECTIONS]
>**Dictum:** *No deep-learning lane by default.*

<br>

- No acceleration claim without benchmark evidence.
- No re-implementation of any `Rasm.Vectors` algorithm, `TensorPrimitives` kernel, or spectral operator.
- No ML.NET training pipeline; the model lane consumes pre-trained `.onnx` assets only.
- No TorchSharp; no ComputeSharp; no Metal direct compute beyond CoreML EP.
- No ONNX Runtime `.Gpu` or `.DirectML` packages — Windows-only.
- No `Task.Run` inside any `Eff<RT,T>` pipeline; use `liftIO`/`Eff.liftIO`.
- No PLINQ; AppHost executor owns thread scheduling.
- No Dataflow inside Compute; AppHost owns topology.
- No model lane without a named, versioned, content-hashed model asset.
- No gRPC server packages (`Grpc.AspNetCore`, health check, reflection) — Compute is client-only.
- No MessagePack/MemoryPack in Compute — Persistence owns snapshot codecs.
- No `IReceipt` base or generic ledger; receipts are typed per capability.
- No `OnError` on the progress observable; all faults go via `ExecutionReceipt.Failure`.
- No direct GPU/Metal compute lane beyond CoreML EP.
- No `Has<RT,_>` traits or `Readable.asks` — v4 vocabulary; use `Eff.runtime<RT>()`.
- No version numbers on package names in documentation text.
