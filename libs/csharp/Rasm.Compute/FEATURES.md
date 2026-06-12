# [COMPUTE_FEATURES]

Feature atlas for measured execution. Every concept rides `Substrate`, `ModelSource`, `ExecutionProvider`, `RemoteTransport`, `CredentialPolicy`, `AllocationClass`, `WorkLane`, `ProgressPhase`, `QuantityFamily`, `ComputeFault`, and `ComputeReceipt` rows — a new concept is a row, never a surface. Mechanics live in the `.planning/` pages; the ANCHORS column cites page clusters.

## [1]-[EXECUTION_CONCEPTS]

| [INDEX] | [CONCEPT]                                                                | [MODALITIES]                  | [ANCHORS]                                                               |
| :-----: | ------------------------------------------------------------------------ | ----------------------------- | ----------------------------------------------------------------------- |
|   [1]   | Typed intent admission and total dispatch                                | all                           | intent-and-selection#INTENT_FAMILY, intent-and-selection#DISPATCH_SPINE |
|   [2]   | Substrate selection with fallback chains and boot-frozen benchmark ranks | all                           | intent-and-selection#SUBSTRATE_AXIS                                     |
|   [3]   | CPU tensor lane (TensorPrimitives SIMD families, 45 op rows)             | all                           | tensor-lane#TENSOR_VOCABULARY, tensor-lane#OPERATION_FAMILIES           |
|   [4]   | Layout algebra and plane staging views                                   | all                           | tensor-lane#LAYOUT_ALGEBRA, staging-and-streams#PLANE_VIEWS             |
|   [5]   | Geometry-to-tensor feature encoding                                      | all                           | tensor-lane#GEOMETRY_ENCODING                                           |
|   [6]   | Kernel equivalence proofs against Rasm baselines                         | all                           | tensor-lane#EQUIVALENCE_INTEROP                                         |
|   [7]   | Geometry ML inference (ONNX, EP-parameterized, OrtValue-only)            | all                           | model-lane#EP_AXIS, model-lane#INFERENCE_MODES                          |
|   [8]   | Apple-silicon ANE/GPU inference (CoreML EP row)                          | plugin, standalone, companion | model-lane#EP_AXIS                                                      |
|   [9]   | Model provenance and extension-op admission                              | all                           | model-lane#MODEL_IDENTITY, model-lane#EXTENSION_OPS                     |
|  [10]   | Shared session capsule with LRU eviction and warmup sweeps               | all                           | model-lane#SESSION_CAPSULE                                              |
|  [11]   | Deterministic model-result cache (version-stamped keys, policy rows)     | all                           | model-lane#RESULT_CACHE                                                 |
|  [12]   | Staging allocation classes and pooled recyclable streams                 | all                           | staging-and-streams#ALLOCATION_AXIS, staging-and-streams#STREAM_POOL    |
|  [13]   | Bounded work lanes with backpressure receipts                            | all                           | scheduling-and-lanes#LANE_AXIS                                          |
|  [14]   | Solve-path isolation (GH2 structural enqueue guard)                      | gh2, plugin                   | scheduling-and-lanes#SOLVE_GUARD                                        |
|  [15]   | One processor budget across the three concurrency axes                   | all                           | scheduling-and-lanes#CPU_BUDGET                                         |
|  [16]   | Units-aware AEC calculation with dual unit evidence                      | all                           | units-boundary#QUANTITY_TABLE, units-boundary#PARSE_FORMAT              |
|  [17]   | Compound dimensional consistency at composition                          | all                           | units-boundary#DIMENSIONAL_LAW                                          |

## [2]-[WIRE_AND_EVIDENCE_CONCEPTS]

| [INDEX] | [CONCEPT]                                                                     | [MODALITIES]               | [ANCHORS]                                                                                                |
| :-----: | ----------------------------------------------------------------------------- | -------------------------- | -------------------------------------------------------------------------------------------------------- |
|   [1]   | Remote compute services on the suite wire vocabulary                          | companion, paired, web-fed | remote-lane#PROTO_VOCABULARY, remote-lane#TRANSPORT_AXIS                                                 |
|   [2]   | Contract evolution by descriptor diff (additive tolerated, breaking rejected) | companion, paired, web-fed | remote-lane#CONTRACT_EVOLUTION                                                                           |
|   [3]   | Typed faults across the wire (FaultDetail through status details)             | companion, paired, web-fed | remote-lane#FAULT_PROJECTION, intent-and-selection#DISPATCH_SPINE                                        |
|   [4]   | Credential axis behind one stamping interceptor                               | companion, paired, web-fed | remote-lane#CALL_POLICY                                                                                  |
|   [5]   | Artifact frame law (64 KiB frames, Crc32, XxHash128, zero-copy wrap)          | companion, paired, web-fed | remote-lane#ARTIFACT_FRAMES                                                                              |
|   [6]   | Progress observation and cancellation spine                                   | all                        | progress-and-observation#PROGRESS_CELL, scheduling-and-lanes#DRAIN_CANCEL                                |
|   [7]   | Observation seams: UI marshal, wire stream, sink-edge receipts                | all                        | progress-and-observation#OBSERVATION_SEAMS                                                               |
|   [8]   | Receipt union with fold projections and provenance chains                     | all                        | receipts-and-benchmarks#RECEIPT_UNION, receipts-and-benchmarks#FOLD_PROJECTIONS                          |
|   [9]   | NodaTime-protobuf wire stamps at the temporal edge                            | all                        | receipts-and-benchmarks#WIRE_STAMPS                                                                      |
|  [10]   | Benchmark and profiling evidence farm with fingerprint-gated claims           | service, test-host         | receipts-and-benchmarks#BENCHMARK_CLAIMS                                                                 |
|  [11]   | TS dashboard projections (wire, progress, receipts)                           | web-fed                    | remote-lane#TS_PROJECTION, progress-and-observation#TS_PROJECTION, receipts-and-benchmarks#TS_PROJECTION |

## [3]-[CAPABILITY_ROWS]

- Wire vocabulary (remote-lane#PROTO_VOCABULARY): the proto suite is suite-canonical — ComputeService, DocumentService (Capabilities, DocumentEvents stream, ExecuteTransaction with idempotency key and dedup window, Query, CaptureEvents), ControlService (capture-support, set-degradation, reload-options), ArtifactSync (64 KiB Crc32 frames, XxHash128 whole-artifact), grpc.health.v1; FaultDetail messages project typed faults into google.rpc.Status details; contract evolution discriminates additive vs breaking by descriptor diff (remote-lane#CONTRACT_EVOLUTION).
- Transports (remote-lane#TRANSPORT_AXIS): Http2 (keepalive policy rows), GrpcWeb (unary and server-stream capability column), UnixDomainSocket (current row; discovery-manifest and peer-credential law), InProcess (test harness); node-selection affinity column for farm topologies; NodaTime-protobuf Timestamp bridges at the wire edge (receipts-and-benchmarks#WIRE_STAMPS).
- Substrates (intent-and-selection#SUBSTRATE_AXIS): CpuTensor, Onnx (one EP-parameterized row: Cpu, CoreML, growth rows for Cuda and DirectML), RemoteGrpc; wasm is a platform predicate; fallback successors and payload caps are row columns.
- Tensor vocabulary (tensor-lane#TENSOR_VOCABULARY, tensor-lane#OPERATION_FAMILIES): 10 dtype rows, 45 op-family rows across 9 kinds with tolerance classes, 5 layout forms, 3 geometry-encoding cases.
- Memory (staging-and-streams#ALLOCATION_AXIS): AllocationClass rows (SpanStack, PooledMemory, RecyclableStream, NativeOrt, EdgeCopy) with admission predicates and evidence slots; one process stream pool with eleven-event evidence (staging-and-streams#STREAM_POOL); zero-alloc progress capsules (progress-and-observation#PROGRESS_CELL).
- Lanes (scheduling-and-lanes#LANE_AXIS): five bounded channel rows (interactive, background, bulk, benchmark, capture-ingest) with capacity, full-mode, readers, and rank as row data; band-200 drain participation (scheduling-and-lanes#DRAIN_CANCEL).
- Units (units-boundary#QUANTITY_TABLE): fifteen frozen quantity rows with conversion exactly once at admission; seven compound dimensional relations swept at composition (units-boundary#DIMENSIONAL_LAW).
- Receipts (receipts-and-benchmarks#RECEIPT_UNION): 13-case typed receipt union with fold projections; benchmark claims gated by host fingerprint; EP-context and profiling artifacts route to the Persistence blob lane (receipts-and-benchmarks#BENCHMARK_CLAIMS).

## [4]-[GAPS_TRACKED]

- ONNX dylib load in the RhinoWIP ALC and Kestrel-in-plugin-ALC hosting are bridge-proofed start gates in the roadmap START_GATES table.
- Per-page research rows (32 across the nine pages) resolve through the roadmap RESEARCH_PROBES routes before their gated clusters transcribe; the ONNX Terminate-latch polling cadence and the GH2 async result readback ceiling sit on model-lane and scheduling-and-lanes.
