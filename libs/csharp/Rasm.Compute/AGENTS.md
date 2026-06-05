# [RASM_COMPUTE_AGENTS]

Scope: `libs/csharp/Rasm.Compute/` only. Root policy and `libs/csharp/AGENTS.md` own universal C# and library-family rules; local README, `_ARCHITECTURE.md`, and `ROADMAP.md` own platform state, substrate details, package facts, and implementation sequence.

## [1][READ_ORDER]

- Before production work, read `README.md`, `_ARCHITECTURE.md`, and `ROADMAP.md` to decide scaffold state, substrate policy, and proof categories.
- Before changing algorithm, tensor, mesh, spectral, sampling, or projection behavior, read `Rasm/Vectors/_ARCHITECTURE.md`.
- Before changing scheduling, channels, runtime records, retry, or drain contracts, read `Rasm.AppHost/AGENTS.md`.
- Before changing progress-observable delivery, read `Rasm.AppUi/AGENTS.md`.
- Before changing benchmark, cache, model-result, or artifact persistence, read `Rasm.Persistence/AGENTS.md`.

## [2][OWNER_CONTRACT]

`Rasm.Compute` is the measured-execution platform over `Rasm.Vectors`. It adds runtime lift, timing, allocation measurement, cancellation, substrate selection, progress, and typed execution receipts; it does not own a geometry kernel, scheduler, channel writer, retry owner, UI scheduler, or durable store.

Call `Rasm.Vectors` for tensor, numeric, spectral, mesh, sampling, and projection bodies. Compute adds execution semantics around those calls and records proof through typed receipts.

## [3][EXTENSION_GRAMMAR]

- New substrate or model lane: extend substrate selection, lifecycle lease, and typed receipt fields; do not add parallel execution owners.
- New algorithm execution path: call the Vectors owner and wrap timing, allocation, cancellation, and progress.
- New benchmark or cache behavior: route persisted artifacts to Persistence and keep benchmark proof in the architecture route.
- New progress surface: keep progress cold, single-shot, typed, and fault-reporting through receipts rather than observable errors.

## [4][BOUNDARY_RULES]

| [INDEX] | [BOUNDARY]         | [RULE]                                                   |
| :-----: | :----------------- | :------------------------------------------------------- |
|   [1]   | `Rasm.Vectors`     | Owns algorithm bodies; Compute wraps and measures        |
|   [2]   | `Rasm.AppHost`     | Owns scheduling, channel writer, retry, and drain        |
|   [3]   | `Rasm.Compute`     | Owns substrate selection, receipts, and progress         |
|   [4]   | `Rasm.AppUi`       | Owns UI observation and scheduling                       |
|   [5]   | `Rasm.Persistence` | Owns persisted benchmarks, caches, and snapshots         |
|   [6]   | Remote service     | Exists through companion contracts, not in-process logic |

## [5][REJECTIONS]

- No reimplementation of `Rasm.Vectors` algorithms or BCL numeric kernels.
- No scheduler, retry, channel writer, UI observation, or persistence ownership inside Compute.
- No generic receipt base, generic ledger, or separate compute receipt family when typed fields carry the lane.
- No `Task.Run`, PLINQ, or ad hoc thread topology inside the measured-execution rail.
- No package versions or provider API claims in docs without architecture or maintained-source proof.
