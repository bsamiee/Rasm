# [RASM_COMPUTE]

`Rasm.Compute` is the measured execution package for vector, tensor, model, and remote compute lanes. It owns execution intent, substrate selection doctrine, typed receipts, progress observation, cancellation use, measurement, benchmark evidence, and lane policy.

## [1]-[PURPOSE]

Compute wraps existing kernel and vector operations with runtime measurement, cancellation, substrate selection, progress, and receipts. `Rasm.Vectors` owns numeric algorithms; Compute selects vector calls, tensor primitives, ONNX/CoreML models, and remote companion work through one execution rail.

It is not a tensor wrapper, ONNX wrapper, gRPC wrapper, ML.NET training pipeline, job framework, queue owner, or replacement for `Rasm`/`Rasm.Vectors`.

## [2]-[STATUS]

| [INDEX] | [SURFACE]          | [STATE]                                    |
| :-----: | ------------------ | ------------------------------------------ |
|   [1]   | Project file       | Present in `Workspace.slnx`                |
|   [2]   | Production source  | Compute rail contract defined              |
|   [3]   | Package references | Project-reference based setup              |
|   [4]   | Runtime spine      | Consumes AppHost-owned runtime policy      |
|   [5]   | Benchmarks         | Routed through `tests/csharp/_benchmarks`  |

## [3]-[CONSTRAINTS]

- Compute owns execution intent, substrate selection, typed execution receipts, progress contracts, cancellation handling, measurement, and allocation classification.
- Compute is built as a complete execution package for host-submitted work, AppHost-dispatched work, companion processors, sidecar services, model execution, remote execution, benchmark lanes, and UI-observed progress through the same execution rail.
- AppHost owns dispatch, drain, runtime composition, outbound retry ownership, and shutdown coordination.
- Persistence owns deterministic model-result cache and benchmark artifact index storage.
- AppUi observes progress only; UI scheduling belongs to AppUi.
- Folder architecture is rail-first: vector, tensor, staging, model, remote, units, stream pooling, cache keys, benchmark evidence, progress, and failures add substrate rows, typed intent fields, receipt cases, and measurement records instead of adding lane-specific service families or result systems.
- Substrate, model identity, endpoint identity, provider options, payload bounds, deadline, allocation class, unit policy, and progress observation are parameterized inputs to the execution rail, not hardcoded provider branches.
- Progress is subscription-gated. Compute does not call `ObserveOn` or allocate Rx state when progress is unobserved.
- `System.Numerics.Tensors` is the tensor-lane package; it is not an in-box .NET 10 API in this repo.
- ONNX Runtime/CoreML is the model lane. Compute does not use ML.NET or `MLContext`.
- gRPC/protobuf is the remote companion lane. `.proto` generation and `Grpc.Tools` belong to proto-owning source projects.
