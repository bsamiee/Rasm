# [RASM_COMPUTE_ROADMAP]

`Rasm.Compute` implementation starts from a manifest-backed execution package and proceeds through one measured execution rail.

## [1]-[CURRENT_POSITION]

| [INDEX] | [SURFACE]         | [STATE]                         |
| :-----: | :---------------- | :------------------------------ |
|   [1]   | Project graph     | solution node present           |
|   [2]   | Package graph     | compute lane packages admitted  |
|   [3]   | Production source | absent                          |
|   [4]   | API catalogues    | package lookup pages maintained |
|   [5]   | Benchmarks        | shared benchmark route selected |

## [2]-[IMPLEMENTATION_TASKS]

[COMPUTE_FOLDER_ARCHITECTURE]:
- Status: QUEUED
- Exit: owner folders, rail entrypoints, generated protocol shapes, model receipts,
  progress contracts, cache keys, benchmark artifacts, and boundaries are planned before production source.
- Proof: architecture plan consumes every Compute package API catalogue and names the measured execution rail owners.

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

## [3]-[CATALOGUE_USE]

[TENSOR_CATALOGUES]:
- Status: REQUIRED
- Action: tensor design consumes the tensors catalogue before source shape selection.
- Exit: tensor owners name numeric primitives, spans, receipts, and benchmark evidence.

[MODEL_CATALOGUES]:
- Status: REQUIRED
- Action: model design consumes ONNX Runtime and ONNX extension asset catalogues.
- Exit: session owners name model load, extension-op admission, inference, allocation, and cache receipts.

[REMOTE_CATALOGUES]:
- Status: REQUIRED
- Action: remote design consumes gRPC client, protobuf, and generator catalogues.
- Exit: remote owners name generated contracts, channel policy, deadline policy, and payload bounds.

[UNITS_CATALOGUES]:
- Status: REQUIRED
- Action: unit design consumes the UnitsNet catalogue before numeric boundary design.
- Exit: unit owners name quantity family, conversion policy, and receipt projection.

[STAGING_CATALOGUES]:
- Status: REQUIRED
- Action: staging design consumes high-performance memory and recyclable-stream catalogues.
- Exit: staging owners name memory ownership, stream reuse, payload bounds, and allocation receipts.
