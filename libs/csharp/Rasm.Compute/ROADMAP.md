# [RASM_COMPUTE_ROADMAP]

Implementation proceeds in the charter BUILD_ORDER: start gates and research probes run before the clusters they unblock, every task exits against named planning-page clusters, and every exit proves through a charter PROOF_GATES row. The charter is `.planning/README.md`; pages are transcribed, never re-designed.

## [1]-[START_GATES]

Bridge-proofed spikes from the campaign binding plan that belong to this package; each gate runs before its unblocked cluster transcribes.

| [INDEX] | [GATE]                                                        | [PROOF]                                                                                                  | [UNBLOCKS]                                |
| :-----: | :------------------------------------------------------------ | :-------------------------------------------------------------------------------------------------------- | :----------------------------------------- |
|   [1]   | ONNX Runtime native dylib load inside the RhinoWIP plugin ALC | `uv run python -m tools.assay bridge verify --pattern tests/csharp/libs/Rasm.Compute`                     | model-lane#SESSION_CAPSULE                 |
|   [2]   | `libortextensions.dylib` resolution under the portable RID graph | `dotnet build libs/csharp/Rasm.Compute/Rasm.Compute.csproj -p:UseRidGraph=true` with bin-output native-asset listing | model-lane#EXTENSION_OPS         |
|   [3]   | Kestrel `ListenUnixSocket` host inside the Rhino plugin ALC (server leg at the rhino-plugin app root) | bridge scenario at app-root creation                                              | remote-lane#TRANSPORT_AXIS                 |
|   [4]   | Grpc.Core.Api proof route (transitive package without an assay key) | `CallSpine` and `WireFault` specs compile on the spec rail until the assay source map registers the key | remote-lane#CALL_POLICY, remote-lane#FAULT_PROJECTION |

## [2]-[RESEARCH_PROBES]

Every page RESEARCH row resolves through its declared PROOF route before the gated cluster transcribes; the probe results bind option-key spellings, member shapes, and cadence values into the fences they gate.

| [INDEX] | [PAGE]                   | [ROWS] | [GATED_CLUSTERS]                                                          |
| :-----: | :----------------------- | :----: | :------------------------------------------------------------------------ |
|   [1]   | intent-and-selection     |   2    | SUBSTRATE_AXIS, INTENT_FAMILY                                              |
|   [2]   | tensor-lane              |   4    | OPERATION_FAMILIES, LAYOUT_ALGEBRA, TENSOR_VOCABULARY                      |
|   [3]   | model-lane               |   9    | MODEL_IDENTITY, SESSION_CAPSULE, EP_AXIS, EXTENSION_OPS, INFERENCE_MODES   |
|   [4]   | remote-lane              |   9    | CONTRACT_EVOLUTION, FAULT_PROJECTION, TRANSPORT_AXIS, CALL_POLICY, ARTIFACT_FRAMES |
|   [5]   | staging-and-streams      |   2    | STREAM_POOL                                                                |
|   [6]   | scheduling-and-lanes     |   2    | LANE_AXIS, SOLVE_GUARD                                                     |
|   [7]   | progress-and-observation |   1    | PROGRESS_CELL                                                              |
|   [8]   | units-boundary           |   2    | QUANTITY_TABLE, PARSE_FORMAT                                               |
|   [9]   | receipts-and-benchmarks  |   1    | RECEIPT_UNION                                                              |

## [3]-[IMPLEMENTATION_TASKS]

Ordered by the charter BUILD_ORDER. The PROOF column names charter PROOF_GATES rows; every task closes with `static fix`, `static build`, and specs per the `testing-cs` skill on the touched closure.

| [INDEX] | [TASK]                          | [EXITS_AGAINST]                                                                            | [PROOF]                                  |
| :-----: | :------------------------------ | :------------------------------------------------------------------------------------------ | :---------------------------------------- |
|   [1]   | Fault rail and key policies     | intent-and-selection#DISPATCH_SPINE (`ComputeFault`), intent-and-selection#SUBSTRATE_AXIS (`ComputeKeyPolicy`) | static                       |
|   [2]   | Tensor lane                     | tensor-lane#TENSOR_VOCABULARY, tensor-lane#OPERATION_FAMILIES, tensor-lane#LAYOUT_ALGEBRA, tensor-lane#GEOMETRY_ENCODING, tensor-lane#EQUIVALENCE_INTEROP | static + spec (45-row partition, equivalence laws) |
|   [3]   | Units boundary                  | units-boundary#QUANTITY_TABLE, units-boundary#DIMENSIONAL_LAW, units-boundary#PARSE_FORMAT | static + spec (relation sweep, admission arities) |
|   [4]   | Staging axis and stream pool    | staging-and-streams#ALLOCATION_AXIS, staging-and-streams#PLANE_VIEWS, staging-and-streams#STREAM_POOL | static + spec (eleven-event fold `Check.Faster`) |
|   [5]   | Progress family and cell        | progress-and-observation#PHASE_FAMILY, progress-and-observation#PROGRESS_CELL, progress-and-observation#OBSERVATION_SEAMS | static + spec (`SampleParallel` rank-regress law) |
|   [6]   | Lane vocabulary and CPU budget  | scheduling-and-lanes#LANE_AXIS (`WorkLane`, `LaneHandle`), scheduling-and-lanes#CPU_BUDGET  | static + spec (reader clamp arithmetic)    |
|   [7]   | Proto vocabulary                | remote-lane#PROTO_VOCABULARY (service and message tables), remote-lane#FAULT_PROJECTION (`FaultDetail` row), remote-lane#ARTIFACT_FRAMES (`ArtifactFrame` row) | restore + static (Grpc.Tools client compile) |
|   [8]   | Model lane                      | model-lane#MODEL_IDENTITY, model-lane#EP_AXIS, model-lane#SESSION_CAPSULE, model-lane#EXTENSION_OPS, model-lane#INFERENCE_MODES, model-lane#RESULT_CACHE | static + spec + bridge (start gates 1-2)   |
|   [9]   | Remote contract and channels    | remote-lane#CONTRACT_EVOLUTION, remote-lane#FAULT_PROJECTION (`WireFault`), remote-lane#TRANSPORT_AXIS, remote-lane#CALL_POLICY, remote-lane#ARTIFACT_FRAMES (`FrameEdge`) | static + spec-rail (start gate 4)          |
|  [10]   | Intent and selection spine      | intent-and-selection#INTENT_FAMILY, intent-and-selection#SUBSTRATE_AXIS, intent-and-selection#DISPATCH_SPINE | static + spec (selection fold, digest laws) |
|  [11]   | Lane runtime and drain          | scheduling-and-lanes#LANE_AXIS (`WorkItem`), scheduling-and-lanes#SOLVE_GUARD, scheduling-and-lanes#DRAIN_CANCEL | static + spec (fence race evidence, drop-row pressure) |
|  [12]   | Receipts, folds, and wire stamps | receipts-and-benchmarks#RECEIPT_UNION, receipts-and-benchmarks#FOLD_PROJECTIONS, receipts-and-benchmarks#WIRE_STAMPS | static + spec (STJ polymorphic emission)  |
|  [13]   | Benchmark claims                | receipts-and-benchmarks#BENCHMARK_CLAIMS                                                    | static + spec (fingerprint gate, band classification) |
|  [14]   | TS projection handoff           | remote-lane#TS_PROJECTION, progress-and-observation#TS_PROJECTION, receipts-and-benchmarks#TS_PROJECTION | descriptor-set emission at app-root creation; contracts transcribe at the TS workspace |

## [4]-[EXIT]

The package exits implementation when every BUILD_ORDER file is transcribed, every PROOF_GATES row is green, the GAP_LEDGER stays fully CLOSED, and `uv run python -m tools.assay test run --target Rasm.Compute.Tests` passes on the full suite.
