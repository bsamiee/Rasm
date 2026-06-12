# [COMPUTE_FEATURES]

Feature atlas for measured execution. Every concept rides Substrate, ModelSource, ExecutionProvider, RemoteTransport, CredentialPolicy, AllocationClass, WorkLane, ProgressPhase, QuantityFamily, Fault, and Receipt rows — a new concept is a row, never a surface. Mechanics live in `.planning/` pages.

## [1]-[CONCEPTS]

| [INDEX] | [CONCEPT] | [MODALITIES] | [PAGES] |
| :-----: | --------- | ------------ | ------- |
| [1] | Geometry ML inference (ONNX, EP-parameterized) | all | model-lane, tensor-lane |
| [2] | Apple-silicon ANE/GPU inference (CoreML EP row) | plugin, standalone, companion | model-lane |
| [3] | Remote compute services (companion farm, attach, web) | companion, paired, web-fed | remote-lane, progress-and-observation |
| [4] | Units-aware AEC calculation with receipt evidence | all | units-boundary, receipts-and-benchmarks |
| [5] | CPU tensor lane (TensorPrimitives SIMD families) | all | tensor-lane, staging-and-streams |
| [6] | Benchmark + profiling evidence farm | service, test-host | receipts-and-benchmarks |
| [7] | Progress observation + cancellation spine | all | progress-and-observation, scheduling-and-lanes |
| [8] | Solve-path isolation (GH2 structural enqueue guard) | gh2, plugin | scheduling-and-lanes |
| [9] | Model provenance + extension-op admission | all | model-lane |
| [10] | Geometry-to-tensor feature encoding | all | tensor-lane |

## [2]-[CAPABILITY_ROWS]

- Wire vocabulary: the proto suite is suite-canonical — ComputeService, DocumentService (Capabilities, DocumentEvents stream, ExecuteTransaction with idempotency + dedup window, Query), ControlService (capture-support, set-degradation, reload-options), ArtifactSync (64 KiB Crc32 frames, XxHash128 whole-artifact), grpc.health.v1; FaultDetail messages project typed faults into google.rpc.Status.details; contract evolution discriminates additive vs breaking by descriptor diff.
- Transports: Http2 (keepalive policy rows), GrpcWeb (unary + server-stream capability column), UnixDomainSocket (current row; discovery manifest law), InProcess (test harness); node-selection affinity column for farm topologies; NodaTime↔protobuf Timestamp bridges at the wire edge.
- Substrates: CpuTensor, Onnx (one EP-parameterized row: Cpu, CoreML, growth rows for Cuda/DirectML), RemoteGrpc; wasm is a platform predicate; fallback chains are declared rows.
- Memory: AllocationClass rows (SpanStack, PooledMemory, RecyclableStream, NativeOrt, EdgeCopy) with rent/return delegates and receipt slots; zero-alloc progress capsules.
- Receipts: 13-case typed receipt union with fold projections; benchmark claims gated by environment fingerprint; EP-context and profiling artifacts route to the Persistence blob lane.

## [3]-[GAPS_TRACKED]

- ONNX dylib load in the RhinoWIP ALC and Kestrel-in-plugin-ALC hosting are bridge-proofed implementation-start gates; ONNX Terminate-latch polling cadence and GH2 async result readback ceilings are research rows on their pages.
