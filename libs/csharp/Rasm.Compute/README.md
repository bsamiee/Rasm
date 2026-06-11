# [RASM_COMPUTE]

`Rasm.Compute` is the measured execution package for vector, tensor, model, remote, units, staging, stream, progress, receipt, and benchmark lanes. It owns execution intent, substrate selection, typed receipts, progress observation, cancellation use, measurement, allocation classification, and lane policy as one execution rail.

## [1]-[PURPOSE]

Compute consumes kernel/vector operations, AppHost runtime policy, and Persistence cache/index contracts. Tensor primitives, ONNX/CoreML models, gRPC companion work, unit conversion, staging memory, pooled streams, and benchmark evidence enter as substrate rows inside one execution surface.

It is not a tensor wrapper, ONNX wrapper, gRPC wrapper, ML.NET training pipeline, job framework, queue owner, UI scheduler, or replacement for `Rasm` and `Rasm.Vectors`.

## [2]-[STATUS]

This table is a lookup by package surface.

| [INDEX] | [SURFACE]          | [STATE]                                    |
| :-----: | :----------------- | :----------------------------------------- |
|   [1]   | Project file       | present in `Workspace.slnx`                |
|   [2]   | Package manifest   | compute execution packages admitted        |
|   [3]   | Project contracts  | Rasm, AppHost, Persistence                 |
|   [4]   | Lockfile           | restored package closure tracked           |
|   [5]   | Production source  | absent                                     |
|   [6]   | Package law        | documented in this folder                  |

## [3]-[DOCUMENTS]

This table routes package documents by reader action.

| [INDEX] | [READ_FOR]              | [OPEN]                      |
| :-----: | :---------------------- | :-------------------------- |
|   [1]   | current structure       | [architecture](ARCHITECTURE.md) |
|   [2]   | implementation sequence | [roadmap](ROADMAP.md)       |
|   [3]   | package API catalogue   | [.reports/api](.reports/api/README.md) |

## [4]-[CONSTRAINTS]

- Compute owns one substrate-selection rail. New execution lanes add substrate rows, typed intent fields, receipt cases, progress states, cache keys, and benchmark proof.
- Compute consumes AppHost runtime policy. AppHost does not reference Compute.
- Compute consumes Persistence cache/index contracts for deterministic model-result cache and benchmark artifact metadata.
- AppUi observes progress and schedules presentation on its own UI scheduler.
- Substrate, model identity, endpoint identity, provider options, payload bounds, deadline, allocation class, unit policy, and progress observation are parameterized data.
- Progress is subscription-gated; Compute does not allocate observable state when progress is unobserved.
- Model and remote work never run synchronously inside GH2 solve paths.
- Benchmark claims are input-class claims with timing, allocation, equivalence, and artifact evidence.
