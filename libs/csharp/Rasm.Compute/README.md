# [RASM_COMPUTE]

`Rasm.Compute` is the measured-compute platform for long-running, tensor, model, and remote execution lanes. It wraps the existing `Rasm.Vectors` tensor/numeric substrate in `Eff<RT,ExecutionReceipt>` — adding timing, allocation, cancellation, substrate selection, and progress — without duplicating a single `TensorPrimitives` kernel that `Rasm.Vectors` already owns.

## [1][PURPOSE]

`Rasm.Compute` coordinates substrate selection, cancellation/progress, benchmark receipts, model lifecycle, remote dispatch, and failure taxonomy for work that outgrows direct `Rasm` operations. `Rasm.Vectors` tensor/numeric algorithms are the default substrate; Compute calls them and does not re-implement them.

It is not a tensor wrapper, ONNX Runtime wrapper, gRPC wrapper, job framework, acceleration claim, or replacement for existing `Rasm`/`Rasm.Vectors` numerics.

## [2][STATUS]

| [INDEX] | [SURFACE]            | [STATE]                                                         |
| :-----: | -------------------- | --------------------------------------------------------------- |
|   [1]   | Project file         | Create in Phase 0                                               |
|   [2]   | Production API       | In progress                                                     |
|   [3]   | Package references   | Add centrally in Phase 0                                        |
|   [4]   | Compute substrate    | Rasm.Vectors default; ONNX Runtime model lane; gRPC remote lane |
|   [5]   | Performance evidence | Per measured input class                                        |

Add packages centrally at the newest viable versions during Phase 0. Do not pin version numbers in documentation.

## [3][MANUAL]

| [INDEX] | [FILE]             | [READ_FOR]                                                                             |
| :-----: | ------------------ | -------------------------------------------------------------------------------------- |
|   [1]   | `_ARCHITECTURE.md` | Type shapes, substrate selection, packages, boundary rules, failure model, measurement |
|   [2]   | `AGENTS.md`        | Build rules, boundary enforcement, and package rejections                              |
|   [3]   | `ROADMAP.md`       | Build sequence and scoped lanes                                                        |

## [4][CONSTRAINTS]

- Substrate choice stays internal to Compute operations; `Rasm.Vectors` owns algorithm bodies.
- Speed and allocation claims carry benchmark evidence under `.artifacts/compute/benchmarks/`.
- Models enter only as named, versioned, lifecycle-managed assets keyed by `ModelKey`.
- Remote compute runs only through an out-of-process companion contract.
- `ComputeRequest` is defined in the shared-contracts project, not here.
- `IObservable<ComputeProgress>` is cold; `Subject` is internal; `OnCompleted` fires on success and cancel; `OnError` never fires — faults surface via receipt only. GH2 components with no subscriber incur zero cost. AppUi consumes via `ObserveOn(RasmUiScheduler.RxScheduler)`; Compute never calls `ObserveOn`.
