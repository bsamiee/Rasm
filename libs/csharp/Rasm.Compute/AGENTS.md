# [H1][RASM_COMPUTE_AGENTS]

`Rasm.Compute` is the measured-execution platform: it wraps `Rasm.Vectors` algorithms in `Eff<RT,ExecutionReceipt>` and adds timing, allocation, cancellation, substrate-selection, and progress — never a kernel of its own. Scaffold-state facts (no `.csproj`, absent from `Workspace.slnx`, references planned, package adoption gated by `docs/host-libraries.md`) are owned by `libs/csharp/AGENTS.md` `[5][LIB_TOPOLOGY]` — do not restate. Every type shape, receipt field, package table, substrate predicate, and ORT API call is owned by co-located `_ARCHITECTURE.md` — point to it, never copy it. This overlay states only the behavioral deltas an agent acts on here.

[CRITICAL] Before any production C#: run `libs/csharp/AGENTS.md` `[1][SCAFFOLDING_PROTOCOL]` (read `_ARCHITECTURE.md` + `ROADMAP.md` Phase 0 first), then validate the `.csproj`/`Workspace.slnx`/`Directory.*.props` landing with `uv run python -m tools.quality static full` (trigger-file change, per `CLAUDE.md` §5.2).

## [1][OWNER_CONTRACT]

Boundaries an agent could not infer from one file (full surface in `_ARCHITECTURE.md` §2, §3.3, §10):

- **Zero kernel duplication.** Call `Rasm.Vectors` for all tensor/numeric/spectral/mesh/sampling work. Compute's only additions over a `Rasm.Vectors` call are: `Eff<RT,ExecutionReceipt>` lift, timing via `RT.Time`, allocation via `MemoryOwner<T>`/`ArrayPoolBufferWriter<T>`, cancellation via `RT.Token`, substrate selection, `ComputeProgress`. Re-implementing any `TensorPrimitives` kernel or `Rasm.Vectors` algorithm is a blocker.
- **Runtime constraint, not trait.** Operations are `Eff<RT,ExecutionReceipt>` where `RT : IComputeRuntime` (`CancellationToken Token`, `TimeProvider Time` only) so Compute compiles/tests without AppHost; `RasmRuntime` (AppHost) adds per-capability accessors (e.g. model-cache path). Resolve via `Eff.runtime<RT>()` — `Has<RT,_>` traits and `Readable.asks` are forbidden v4 vocabulary.
- **Compute owns no executor, no channel, no retry.** AppHost owns the `Channel<ComputeRequest>` writer, the drain scheduler, and remote-hop retry; Compute executes a drained request when called. `ComputeRequest` is defined in the shared-contracts project, never in `Rasm.Compute` type definitions.
- **Progress is cold and single-shot.** Own a per-operation cold `Subject<ComputeProgress>`; expose `.AsObservable()`; emit `OnCompleted` on success AND cancellation; never emit `OnError` (faults surface via `ExecutionReceipt.Failure` only); never call `ObserveOn` (AppUi owns `ObserveOn(RasmUiScheduler.RxScheduler)`). No-subscriber GH2 = cold no-op at zero cost.
- **Allocation budget never throws.** Measure with `GC.GetAllocatedBytesForCurrentThread()` diff around the body; overrun sets `ExecutionReceipt.Failure = AllocationBudgetExceeded` and returns the partial receipt.
- **GH2 never blocks the solve thread.** In-process (`Rasm.Vectors`) → `GH_TaskCapableComponent` + cached receipt + `ExpireSolution(true)`. Model/remote (> 100 ms) → submit to AppHost's channel, cache last receipt, `ExpireSolution(true)` on drain delivery.
- **Typed receipts only.** Lane receipts are `Option<>` fields on `ExecutionReceipt` (no `ComputeReceipt` union, no `IReceipt` base, no generic ledger). Receipt fields are owned in `_ARCHITECTURE.md` §3.4.

## [2][MODEL_LANE_GOTCHAS]

Provider-behavior constraints — violating one fails at runtime, not build. Source of truth: `_ARCHITECTURE.md` §6 + ONNX Runtime CoreML EP docs. Last verified: 2026-06-04.

- **CoreML EP API.** Use `AppendExecutionProvider("CoreML", dict)`. The native P/Invoke `OrtSessionOptionsAppendExecutionProvider_CoreML` was deprecated in ORT 1.20 — must not appear in new code.
- **`ModelCacheDirectory` is mandatory** (set on every `SessionOptions`); without it ORT recompiles the MLProgram subgraph on every load. ORT does not detect model changes — a stale cache returns the old compiled program silently, so `ModelKey` includes the `.onnx` content hash.
- **fp16 downcast hazard.** CoreML ANE silently downcasts fp32→fp16. Validate every new model against `CPUOnly` EP with `AllowLowPrecisionAccumulationOnGPU=0`; record on `ModelReceipt.Fp16Downcast`.
- **Sync `Run` only.** `RunAsync` is crash-prone inside Rhino's process; use synchronous `Run` + `RunOptions` with `token.Register(() => ro.Terminate = true)`, lifted via `liftIO`/`Eff.liftIO`. Never `Task.Run`.
- **Native dylib probe before first session.** `NativeLibrary.SetDllImportResolver` for `runtimes/osx-arm64/native/libonnxruntime.dylib` at startup; a missing native asset fails at runtime.
- **Session lease.** One `InferenceSession` per `ModelKey` serialized by `SemaphoreSlim` in `Atom<HashMap<ModelKey,SemaphoreSlim>>`; created off the Rhino UI thread; disposed only at AppHost shutdown drain. Cap `IntraOpNumThreads` at 2–4 (not `ProcessorCount`) to avoid contending with Rhino's mesh/display pipelines; configure the shared pool once via `OrtEnvironment.GetInstance().SetGlobalIntraOpNumThreads(cap)` before any session.
- **gRPC idle connections.** Configure `SocketsHttpHandler` keep-alive (`KeepAlivePingDelay`, `KeepAlivePingTimeout`, `EnableMultipleHttp2Connections = true`) so idle timeouts do not surface as transient faults; emit `RemoteReceipt.RetryOwnerConflict` if a second retry owner appears (AppHost owns retry).

## [3][BOUNDARY_RULES]

Durable cross-folder ownership (full DAG in parent `[5][LIB_TOPOLOGY]`; this is the per-concern split):

| [INDEX] | [BOUNDARY]         | [RULE]                                                                                                           |
| :-----: | ------------------ | ---------------------------------------------------------------------------------------------------------------- |
|   [1]   | `Rasm.Vectors`     | Owns all tensor/numeric algorithm bodies; Compute calls, never duplicates                                        |
|   [2]   | `Rasm.AppHost`     | Owns `Channel<ComputeRequest>` writer + executor + remote retry + drain order                                    |
|   [3]   | `Rasm.Compute`     | Owns substrate-selection predicate, lifecycle lease, receipt shapes, progress emission                           |
|   [4]   | Shared-contracts   | Owns `ComputeRequest`, `.proto`, `Grpc.Tools` (`PrivateAssets=all`); AppHost + Compute reference                 |
|   [5]   | `Rasm.Persistence` | Indexes benchmark artifacts; serves `Option<ModelResult>` cache lookup                                           |
|   [6]   | `Rasm.AppUi`       | Calls `ObserveOn(RasmUiScheduler.RxScheduler)`; Compute never schedules UI                                       |
|   [7]   | Remote service     | Exists only through companion contract; Compute is client-only                                                   |
|   [8]   | Benchmarks         | Required for any speed/allocation claim; live in `tests/csharp/libs/Rasm.Compute/Rasm.Compute.Benchmarks.csproj` |

## [4][REJECTIONS]

Forbidden patterns specific to this directory (rationale in `_ARCHITECTURE.md` §5.2):

- No acceleration/speed/allocation claim without a `BenchmarkReceipt` under `.artifacts/compute/benchmarks/<substrate>/<run-id>/`.
- No re-implementation of any `Rasm.Vectors` algorithm, `TensorPrimitives` kernel, or spectral operator.
- No ML.NET (training); no TorchSharp; no ComputeSharp; no Metal/GPU direct compute beyond the CoreML EP.
- No ONNX Runtime `.Gpu` / `.DirectML` packages (Windows-only); no gRPC server packages (`Grpc.AspNetCore`, health check, reflection) — Compute is client-only.
- No `Task.Run`, PLINQ, or `System.Threading.Tasks.Dataflow` inside Compute — AppHost owns thread scheduling and topology; lift IO via `liftIO`/`Eff.liftIO`.
- No model lane without a named, versioned, content-hashed `.onnx` asset and `ModelKey`.
- No MessagePack/MemoryPack (Persistence owns snapshot codecs); no `IReceipt` base or generic ledger; no `OnError` on the progress observable.
- No `Has<RT,_>` traits or `Readable.asks` (v4 vocabulary); use `Eff.runtime<RT>()`.
- No version numbers on package names in documentation text.

## [5][EVIDENCE]

Proof categories specific to Compute (rails owned by `CLAUDE.md` §5.2; `coding-csharp`/`testing-cs` skills per `CLAUDE.md` §1):

- Output equivalence vs. baseline (incl. fp16-downcast validation against `CPUOnly` EP).
- Benchmark timing + allocation profile under `.artifacts/compute/benchmarks/<substrate>/<run-id>/`.
- Cancellation / timeout / failure receipts; allocation-budget overrun receipt.
- `MemoryOwner<T>` staging; `UnitsNet` typed scalars at the domain boundary.
- Model identity/version/content-hash/load/dispose/lease receipts; native dylib probe success receipt before first session.
- gRPC deadline/payload/retry-owner/keep-alive failure receipts.
- GH2 non-blocking path (`GH_TaskCapableComponent` + `ExpireSolution`); `ComputeProgress` sequence (monotone `Fraction`, `OnCompleted` on success/cancel, no `OnError`).
- Runtime proof of the ORT model lane inside RhinoWIP routes through `uv run python -m tools.quality bridge verify` (see `testing-cs`).
