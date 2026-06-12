# [COMPUTE_PLANNING]

Rasm.Compute has zero consumers; the implementation is full-capability with no holding back. These pages are decision-complete blueprints an implementation agent transcribes — never re-designed downstream. The package owns measured execution: intent and substrate selection, tensor and model lanes, the remote lane carrying the suite wire vocabulary, staging memory, scheduling, progress, the units boundary, and typed receipts — consuming AppHost ports and Persistence stores as settled vocabulary.

## [1]-[PAGE_INDEX]

| [INDEX] | [PAGE] | [OWNS] | [STATE] |
| :-----: | ------ | ------ | :-----: |
| [1] | [intent-and-selection](intent-and-selection.md) | Typed intent family; substrate-selection rail; total dispatch | finalized |
| [2] | [tensor-lane](tensor-lane.md) | CPU tensor substrate; shape algebra; geometry-to-tensor encoding | finalized |
| [3] | [model-lane](model-lane.md) | ONNX identity, session capsule, EP rows, extension-op admission | finalized |
| [4] | [remote-lane](remote-lane.md) | Proto wire vocabulary; transports; channel capsule; credential axis | finalized |
| [5] | [staging-and-streams](staging-and-streams.md) | AllocationClass rows; pooled memory; recyclable streams | finalized |
| [6] | [scheduling-and-lanes](scheduling-and-lanes.md) | WorkLane channels; solve-path guard; drain participation | finalized |
| [7] | [progress-and-observation](progress-and-observation.md) | Monotonic phases; zero-alloc capsules; observation seams | finalized |
| [8] | [units-boundary](units-boundary.md) | QuantityFamily rows; conversion-at-admission; unit evidence | finalized |
| [9] | [receipts-and-benchmarks](receipts-and-benchmarks.md) | Receipt union; fold projections; benchmark claims | finalized |

## [2]-[WIRE_PAGES]

remote-lane · progress-and-observation · receipts-and-benchmarks (each carries exactly one TS_PROJECTION cluster).

## [3]-[CATALOGUE_PENDING]

App-root server packages (Grpc.AspNetCore trio) catalogued at app-root creation.

## [4]-[GAP_LEDGER]

| [INDEX] | [GAP] | [CLOSED_BY] | [STATE] |
| :-----: | ----- | ----------- | :-----: |
| [1] | Substrate collapse: one EP-parameterized Onnx row; wasm row deleted (platform predicate) | intent-and-selection | CLOSED |
| [2] | DocumentService joins the wire vocabulary (Capabilities, DocumentEvents stream, ExecuteTransaction idempotency + dedup window, Query) | remote-lane | CLOSED |
| [3] | ControlService joins the wire vocabulary (capture-support, set-degradation, reload-options) | remote-lane | CLOSED |
| [4] | FaultDetail messages project typed faults into google.rpc.Status.details | remote-lane + receipts-and-benchmarks | CLOSED |
| [5] | Contract evolution by descriptor diff (additive tolerated, breaking rejected); checksum byte-equality deleted | remote-lane | CLOSED |
| [6] | Streaming-capability column per transport row; ArtifactSync bidi structurally excluded on GrpcWeb | remote-lane | CLOSED |
| [7] | Keepalive policy rows (PooledConnectionIdleTimeout=Infinite, KeepAlivePing 60s/30s, EnableMultipleHttp2Connections) | remote-lane | CLOSED |
| [8] | Payload law: 4 MiB caps; ArtifactSync 64 KiB Crc32 frames + XxHash128 whole-artifact; UnsafeByteOperations.UnsafeWrap zero-copy; frame law owned here, BlobRemote consumes | remote-lane + staging-and-streams | CLOSED |
| [9] | CredentialPolicy axis (insecure-loopback scoped to UDS, tls, mtls, bearer-for-browser); UDS row current with capability flags; peer-credential law | remote-lane | CLOSED |
| [10] | Compression axis row (channel/per-call grpc-accept-encoding) | remote-lane | CLOSED |
| [11] | Interceptor/CallInvoker seam absorbed: correlation metadata, traceparent propagation | remote-lane | CLOSED |
| [12] | One retry owner: gRPC ServiceConfig RetryPolicy refused by spelling; Conflict receipts emitted here | remote-lane + receipts-and-benchmarks | CLOSED |
| [13] | grpc.health.v1 adopted; node-selection affinity column for farm topologies | remote-lane | CLOSED |
| [14] | WorkLane name owned here (AppHost renamed to DrainQueue); solve-path guard makes synchronous GH2 execution unrepresentable | scheduling-and-lanes | CLOSED |
| [15] | Progress subscriptions carry scheduler policy values; zero-alloc readonly record struct capsules; receipts materialize at the sink edge | progress-and-observation | CLOSED |
| [16] | Geometry-to-tensor encoding cluster (point-cloud, mesh, voxel, symbolic-dim rows) | tensor-lane | CLOSED |
| [17] | Tensor layout family verified surface (PermuteDimensions, FlattenTo, Squeeze/Unsqueeze, SetSlice, Split, Stack) replaces phantom factories | tensor-lane | CLOSED |
| [18] | CoreML EP row with verified AppendExecutionProvider spelling + OrtEnv.GetAvailableProviders probe; EP-context warm-start artifacts route to the Persistence blob lane | model-lane | CLOSED |
| [19] | ONNX Terminate-latch cancellation cadence (no native timeout) research row | model-lane | CLOSED |
| [20] | NodaTime-to-protobuf bridges at the wire edge (ToTimestamp/ToInstant family) | remote-lane + receipts-and-benchmarks | CLOSED |
| [21] | Benchmark claims gated by environment fingerprint; profiling artifacts to blob lane | receipts-and-benchmarks | CLOSED |
| [22] | One canonical wire geometry: the proto geometry family; NTS/RhinoCommon/GeoJSON are boundary projections | remote-lane | CLOSED |
| [23] | UnitsNet 6.x QuantityInfo reshape research row (pins 5.75) | units-boundary | CLOSED |
| [24] | Discovery manifest consumption (socket path, contractChecksum, storeEpoch) on the UDS transport row | remote-lane | CLOSED |

Sections [DENSITY_BAR], [BUILD_ORDER], [FILE_PROCESS], [PROOF_GATES], [PROHIBITIONS], [ADMISSIONS_RECORD] complete after page finalization.
