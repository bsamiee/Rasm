# [RASM_COMPUTE_AGENTS]

Scope: `libs/csharp/Rasm.Compute/` only. Root policy and `libs/csharp/AGENTS.md` own universal C# and library-family rules; local README, `_ARCHITECTURE.md`, and `ROADMAP.md` own platform state, substrate details, package facts, and implementation sequence.

## [1][READ_ORDER]

- Before production work, read `README.md`, `_ARCHITECTURE.md`, and `ROADMAP.md` to decide scaffold state, substrate policy, and proof categories.
- Before changing algorithm, tensor, mesh, spectral, sampling, or projection behavior, read `Rasm/Vectors/_ARCHITECTURE.md`.
- Before changing scheduling, channels, runtime records, retry, or drain contracts, read `Rasm.AppHost/AGENTS.md`.
- Before changing progress-observable delivery, read `Rasm.AppUi/AGENTS.md`.
- Before changing benchmark, cache, model-result, or artifact persistence, read `Rasm.Persistence/AGENTS.md`.
- Before changing `ComputeRequest`, `.proto`, gRPC codegen, remote payloads, or companion remote contracts, read the shared-contracts owner where present; if no owner exists, stop and record the missing owner as a proof gap instead of defining the contract inside Compute.

## [2][OWNER_CONTRACT]

`Rasm.Compute` is the measured-execution platform over `Rasm.Vectors`. It adds runtime lift, timing, allocation measurement, cancellation, substrate selection, progress, and typed execution receipts; it does not own a geometry kernel, scheduler, channel writer, retry owner, UI scheduler, or durable store.

Call `Rasm.Vectors` for tensor, numeric, spectral, mesh, sampling, and projection bodies. Compute adds execution semantics around those calls and records proof through typed receipts.

## [3][EXTENSION_GRAMMAR]

- New substrate, model, remote, progress, allocation, tolerance, or execution-receipt behavior: extend `ComputeIntent`, substrate lifecycle lease, and `ExecutionReceipt`; do not add provider API, remote retry owner, shared-contract placeholder, benchmark claim, or parallel execution owner.
- New algorithm execution path: call the Vectors owner and wrap timing, allocation, cancellation, and progress.
- New benchmark or cache behavior: route persisted artifacts to Persistence and keep benchmark proof in the architecture route.
- New progress surface: keep progress cold, single-shot, typed, and fault-reporting through receipts rather than observable errors.
- Package-backed execution behavior: read `_ARCHITECTURE.md` and central manifests, then internalize approved package capability as substrate, model, remote, progress, allocation, tolerance, and execution-receipt behavior inside `ComputeIntent` and `ExecutionReceipt` before exposing provider APIs, remote knobs, benchmark claims, or compatibility aliases.

## [4][BOUNDARY_RULES]

| [INDEX] | [BOUNDARY]         | [RULE]                                                   |
| :-----: | :----------------- | :------------------------------------------------------- |
|   [1]   | `Rasm.Vectors`     | Owns algorithm bodies; Compute wraps and measures        |
|   [2]   | `Rasm.AppHost`     | Owns scheduling, channel writer, retry, and drain        |
|   [3]   | `Rasm.Compute`     | Owns substrate selection, receipts, and progress         |
|   [4]   | `Rasm.AppUi`       | Owns UI observation and scheduling                       |
|   [5]   | `Rasm.Persistence` | Owns persisted benchmarks, caches, and snapshots         |
|   [6]   | Remote service     | Read shared-contracts owner where present; otherwise record a proof gap |

## [5][REJECTIONS]

- No reimplementation of `Rasm.Vectors` algorithms or BCL numeric kernels.
- No scheduler, retry, channel writer, UI observation, or persistence ownership inside Compute.
- No generic receipt base, generic ledger, or separate compute receipt family when typed fields carry the lane.
- No `Task.Run`, PLINQ, or ad hoc thread topology inside the measured-execution rail.
- No provider shape, remote retry owner, shared-contract placeholder, benchmark claim, or package API as public platform API when `ComputeIntent` and `ExecutionReceipt` can carry the behavior.
- No package versions or provider API claims in docs without architecture or maintained-source proof.

## [6][STOP_RULES]

If native model loading, `libonnxruntime.dylib` resolution, CoreML fp16 equivalence, remote retry ownership, shared-contracts ownership, benchmark methodology, or cache artifact ownership is unproved, stop and route to architecture, AppHost, Persistence, or shared-contract proof before broadening the execution rail.
