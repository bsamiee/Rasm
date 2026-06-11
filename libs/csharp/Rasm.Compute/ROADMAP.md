# [RASM_COMPUTE_ROADMAP]

`Rasm.Compute` implementation starts from a manifest-backed execution package and proceeds through one measured execution rail.

## [1]-[CURRENT_POSITION]

This table is a lookup by implementation surface.

| [INDEX] | [SURFACE]         | [STATE]                         |
| :-----: | :---------------- | :------------------------------ |
|   [1]   | Project graph     | solution node present           |
|   [2]   | Package graph     | compute lane packages admitted  |
|   [3]   | Production source | absent                          |
|   [4]   | API catalogues    | package lookup pages maintained |
|   [5]   | Benchmarks        | shared benchmark route selected |

## [2]-[IMPLEMENTATION_TASKS]

[COMPUTE_INTENT_SELECTION]:
- Status: QUEUED
- Exit: typed intent, payload bounds, substrate, deadline, allocation class, and failure cases enter one dispatch surface.
- Proof: selection law specs and dependency tests.

[COMPUTE_TENSOR_MODEL]:
- Status: QUEUED
- Exit: tensor and ONNX/CoreML lanes emit equivalence, load, inference, allocation, and cache receipts.
- Proof: CPU baseline, model native load, and benchmark evidence.

[COMPUTE_REMOTE]:
- Status: QUEUED
- Exit: gRPC companion lane carries endpoint identity, payload bounds, deadline, retry-owner evidence, and generated contract proof.
- Proof: proto generation proof and remote contract specs.

[COMPUTE_PROGRESS_BENCHMARK]:
- Status: QUEUED
- Exit: progress is subscription-gated and benchmark claims are input-class rows with artifacts indexed through Persistence.
- Proof: progress monotonicity specs and benchmark artifact receipts.

## [3]-[PACKAGE_PROOF]

This table is a lookup by package rail.

| [INDEX] | [RAIL]    | [REQUIRED_STATE]                    |
| :-----: | :-------- | :---------------------------------- |
|   [1]   | Tensor    | tensor primitives admitted          |
|   [2]   | Model     | ONNX Runtime and extensions admitted |
|   [3]   | Remote    | gRPC client, protobuf, tools        |
|   [4]   | Units     | physical-unit boundary package      |
|   [5]   | Staging   | memory helpers and pooled streams   |
