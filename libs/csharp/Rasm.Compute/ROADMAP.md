# [RASM_COMPUTE_ROADMAP]

`Rasm.Compute` implements execution lanes with source, package graph, benchmark evidence, and receipts in one rail.

## [1]-[CAPABILITY_RAILS]

| [INDEX] | [RAIL]     | [EXIT_STATE]                                                                                |
| :-----: | ---------- | ------------------------------------------------------------------------------------------- |
|   [1]   | Intent     | Operation, payload, model, remote, deadline are typed                                       |
|   [2]   | Selection  | Ordered substrate predicates are implementation-backed                                      |
|   [3]   | Lifecycle  | Accepted through completed/cancelled/faulted states are explicit                            |
|   [4]   | Receipts   | Execution, benchmark, model, remote, progress, and failure evidence fold through one family |
|   [5]   | Progress   | Subscription-gated progress contract                                                        |
|   [6]   | Runtime    | Uses AppHost-owned cancellation and time policy                                             |
|   [7]   | Benchmarks | Rows land in `tests/csharp/_benchmarks`                                                     |

## [2]-[LANE_CONTRACT]

| [INDEX] | [LANE]  | [PACKAGE_SET]                                       | [PROOF]                              |
| :-----: | ------- | --------------------------------------------------- | ------------------------------------ |
|   [1]   | Tensor  | `System.Numerics.Tensors`, staging packages as used | Benchmarked tensor consumer          |
|   [2]   | Model   | `Microsoft.ML.OnnxRuntime`                          | Native probe and CoreML baseline     |
|   [3]   | Remote  | `Grpc.Net.Client`, `Google.Protobuf`, `Grpc.Tools`  | Companion contract and payload proof |
|   [4]   | Units   | `UnitsNet`                                          | External unit boundary proof         |
|   [5]   | Streams | `Microsoft.IO.RecyclableMemoryStream`               | Stream-shaped hot path proof         |

Execution lanes are provider-rich and single-rail. Tensor, ONNX/CoreML, gRPC, units, stream pooling, and staging-memory packages add substrate rows, typed receipts, cache keys, progress states, and benchmark proof inside the Compute rail; they do not add parallel execution services.

## [3]-[IMPLEMENTATION_DOCTRINE]

- Compute uses one substrate-selection rail. New lanes add rows and receipts to that rail; they do not add parallel execution services.
- Execution logic is bounded by typed intent, typed payload limits, typed lane selection, and typed receipt projection.
- Progress is data derived from lifecycle and measurement, not a second control plane.
- Model and remote work never run synchronously inside GH2 solve paths.
- Retry ownership is singular. Remote lanes use AppHost outbound policy rather than stacking compute-local retries.
- Benchmark claims are input-class claims with allocation and equivalence evidence, not one-off timing anecdotes.
- Public vocabulary stays provider-neutral; ONNX, gRPC, and tensor APIs stay inside the lane that owns them.

## [4]-[INTEGRATION]

- AppHost owns dispatch, drain, retry, and outbound-hop policy.
- Persistence owns deterministic model-result cache and benchmark artifact index.
- AppUi observes progress and schedules onto its UI scheduler.
- Rhino/GH2 solve paths submit requests and consume cached receipts; they do not block on ONNX or remote calls.

## [5]-[VALIDATION]

| [INDEX] | [GATE]       | [REQUIRED_STATE]                                |
| :-----: | ------------ | ----------------------------------------------- |
|   [1]   | Restore      | Compute project lockfile is current             |
|   [2]   | Build        | Compute package scaffold builds                 |
|   [3]   | Architecture | AppHost is not dependent on Compute             |
|   [4]   | Package      | Direct/transitive package checks are clean      |
|   [5]   | Benchmark    | Measured lanes have artifacts in benchmark rail |
