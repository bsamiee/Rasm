# [COMPUTE_PLANNING]

Rasm.Compute has zero consumers; the implementation is full-capability with no holding back. These pages are decision-complete blueprints an implementation agent transcribes — never re-designed downstream. The package owns measured execution: intent and substrate selection, tensor and model lanes, the remote lane carrying the suite wire vocabulary, staging memory, scheduling, progress, the units boundary, and typed receipts — consuming AppHost ports and Persistence stores as settled vocabulary.

## [1]-[PAGE_INDEX]

| [INDEX] | [PAGE]                                                  | [OWNS]                                                              |  [STATE]  |
| :-----: | ------------------------------------------------------- | ------------------------------------------------------------------- | :-------: |
|   [1]   | [intent-and-selection](intent-and-selection.md)         | Typed intent family; substrate-selection rail; total dispatch       | finalized |
|   [2]   | [tensor-lane](tensor-lane.md)                           | CPU tensor substrate; shape algebra; geometry-to-tensor encoding    | finalized |
|   [3]   | [model-lane](model-lane.md)                             | ONNX identity, session capsule, EP rows, extension-op admission     | finalized |
|   [4]   | [remote-lane](remote-lane.md)                           | Proto wire vocabulary; transports; channel capsule; credential axis | finalized |
|   [5]   | [staging-and-streams](staging-and-streams.md)           | AllocationClass rows; pooled memory; recyclable streams             | finalized |
|   [6]   | [scheduling-and-lanes](scheduling-and-lanes.md)         | WorkLane channels; solve-path guard; drain participation            | finalized |
|   [7]   | [progress-and-observation](progress-and-observation.md) | Monotonic phases; zero-alloc capsules; observation seams            | finalized |
|   [8]   | [units-boundary](units-boundary.md)                     | QuantityFamily rows; conversion-at-admission; unit evidence         | finalized |
|   [9]   | [receipts-and-benchmarks](receipts-and-benchmarks.md)   | Receipt union; fold projections; benchmark claims                   | finalized |

## [2]-[WIRE_PAGES]

remote-lane · progress-and-observation · receipts-and-benchmarks (each carries exactly one TS_PROJECTION cluster).

## [3]-[CATALOGUE_PENDING]

App-root server packages (Grpc.AspNetCore trio) catalogued at app-root creation.

## [4]-[GAP_LEDGER]

| [INDEX] | [GAP]                                                                                                                                                                      | [CLOSED_BY]                           | [STATE] |
| :-----: | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------- | :-----: |
|   [1]   | Substrate collapse: one EP-parameterized Onnx row; wasm row deleted (platform predicate)                                                                                   | intent-and-selection                  | CLOSED  |
|   [2]   | DocumentService joins the wire vocabulary (Capabilities, DocumentEvents stream, ExecuteTransaction idempotency + dedup window, Query)                                      | remote-lane                           | CLOSED  |
|   [3]   | ControlService joins the wire vocabulary (capture-support, set-degradation, reload-options)                                                                                | remote-lane                           | CLOSED  |
|   [4]   | FaultDetail messages project typed faults into google.rpc.Status.details                                                                                                   | remote-lane + receipts-and-benchmarks | CLOSED  |
|   [5]   | Contract evolution by descriptor diff (additive tolerated, breaking rejected); checksum byte-equality deleted                                                              | remote-lane                           | CLOSED  |
|   [6]   | Streaming-capability column per transport row; ArtifactSync bidi structurally excluded on GrpcWeb                                                                          | remote-lane                           | CLOSED  |
|   [7]   | Keepalive policy rows (PooledConnectionIdleTimeout=Infinite, KeepAlivePing 60s/30s, EnableMultipleHttp2Connections)                                                        | remote-lane                           | CLOSED  |
|   [8]   | Payload law: 4 MiB caps; ArtifactSync 64 KiB Crc32 frames + XxHash128 whole-artifact; UnsafeByteOperations.UnsafeWrap zero-copy; frame law owned here, BlobRemote consumes | remote-lane + staging-and-streams     | CLOSED  |
|   [9]   | CredentialPolicy axis (insecure-loopback scoped to UDS, tls, mtls, bearer-for-browser); UDS row current with capability flags; peer-credential law                         | remote-lane                           | CLOSED  |
|  [10]   | Compression axis row (channel/per-call grpc-accept-encoding)                                                                                                               | remote-lane                           | CLOSED  |
|  [11]   | Interceptor/CallInvoker seam absorbed: correlation metadata, traceparent propagation                                                                                       | remote-lane                           | CLOSED  |
|  [12]   | One retry owner: gRPC ServiceConfig RetryPolicy refused by spelling; Conflict receipts emitted here                                                                        | remote-lane + receipts-and-benchmarks | CLOSED  |
|  [13]   | grpc.health.v1 adopted; node-selection affinity column for farm topologies                                                                                                 | remote-lane                           | CLOSED  |
|  [14]   | WorkLane name owned here (AppHost renamed to DrainQueue); solve-path guard makes synchronous GH2 execution unrepresentable                                                 | scheduling-and-lanes                  | CLOSED  |
|  [15]   | Progress subscriptions carry scheduler policy values; zero-alloc readonly record struct capsules; receipts materialize at the sink edge                                    | progress-and-observation              | CLOSED  |
|  [16]   | Geometry-to-tensor encoding cluster (point-cloud, mesh, voxel, symbolic-dim rows)                                                                                          | tensor-lane                           | CLOSED  |
|  [17]   | Tensor layout family verified surface (PermuteDimensions, FlattenTo, Squeeze/Unsqueeze, SetSlice, Split, Stack) replaces phantom factories                                 | tensor-lane                           | CLOSED  |
|  [18]   | CoreML EP row with verified AppendExecutionProvider spelling + OrtEnv.GetAvailableProviders probe; EP-context warm-start artifacts route to the Persistence blob lane      | model-lane                            | CLOSED  |
|  [19]   | ONNX Terminate-latch cancellation cadence (no native timeout) research row                                                                                                 | model-lane                            | CLOSED  |
|  [20]   | NodaTime-to-protobuf bridges at the wire edge (ToTimestamp/ToInstant family)                                                                                               | remote-lane + receipts-and-benchmarks | CLOSED  |
|  [21]   | Benchmark claims gated by environment fingerprint; profiling artifacts to blob lane                                                                                        | receipts-and-benchmarks               | CLOSED  |
|  [22]   | One canonical wire geometry: the proto geometry family; NTS/RhinoCommon/GeoJSON are boundary projections                                                                   | remote-lane                           | CLOSED  |
|  [23]   | UnitsNet 6.x QuantityInfo reshape research row (pins 5.75)                                                                                                                 | units-boundary                        | CLOSED  |
|  [24]   | Discovery manifest consumption (socket path, contractChecksum, storeEpoch) on the UDS transport row                                                                        | remote-lane                           | CLOSED  |

## [5]-[DENSITY_BAR]

Implementation lands at 25-35% of naive LOC: one owner per axis, one entrypoint family per rail, dispatch over row data through generated total Switches and frozen tables. A new feature is one row or one case on a budgeted owner — never a new surface. Seven comparer accessors exist, one per axis owner and package-local; the `WorkLane` key accessor is `ComputeKeyPolicy`, never the AppHost `LaneKeyPolicy` (suite ledger key-policy posture).

Axis owners (vocabulary budget, derived from the ledger CP-01 through CP-09 signature regions):

| [INDEX] | [AXIS/CONCERN]     | [OWNER]             | [KIND]                   |   [CASES]   |
| :-----: | :----------------- | :------------------ | :----------------------- | :---------: |
|   [1]   | Intent family      | `ComputeIntent`     | [Union] + nested `Spec`  |      5      |
|   [2]   | Substrate axis     | `Substrate`         | SmartEnum\<string>       |      3      |
|   [3]   | Fault family       | `ComputeFault`      | [Union] fault, band 2200 |     13      |
|   [4]   | Total dispatch     | `DispatchTable`     | record                   | 3 delegates |
|   [5]   | Tensor dtypes      | `TensorDtype`       | SmartEnum\<string>       |     10      |
|   [6]   | Tensor op kinds    | `TensorOpKind`      | SmartEnum\<string>       |      9      |
|   [7]   | Tensor op families | `TensorOpFamily`    | SmartEnum\<string>       |     45      |
|   [8]   | Tolerance classes  | `ToleranceClass`    | SmartEnum\<string>       |      4      |
|   [9]   | Layout forms       | `LayoutForm`        | SmartEnum\<string>       |      5      |
|  [10]   | Encoding channels  | `EncodingChannel`   | SmartEnum\<string>       |      3      |
|  [11]   | Geometry encodings | `GeometryEncoding`  | [Union]                  |      3      |
|  [12]   | Model acquisition  | `ModelSource`       | [Union]                  |      4      |
|  [13]   | EP axis            | `ExecutionProvider` | SmartEnum\<string>       |      2      |
|  [14]   | Cache postures     | `CachePolicy`       | SmartEnum\<string>       |      4      |

| [INDEX] | [AXIS/CONCERN]    | [OWNER]                | [KIND]             | [CASES] |
| :-----: | :---------------- | :--------------------- | :----------------- | :-----: |
|   [1]   | Contract drift    | `ContractDrift`        | [Union]            |    3    |
|   [2]   | Transport axis    | `RemoteTransport`      | SmartEnum\<string> |    4    |
|   [3]   | Stream shapes     | `StreamShape`          | [Flags] enum       |    4    |
|   [4]   | Node selection    | `NodeSelection`        | enum               |    3    |
|   [5]   | Credential axis   | `CredentialPolicy`     | SmartEnum\<string> |    4    |
|   [6]   | Allocation axis   | `AllocationClass`      | SmartEnum\<string> |    5    |
|   [7]   | Work lanes        | `WorkLane`             | SmartEnum\<string> |    5    |
|   [8]   | Progress phases   | `ProgressPhase`        | SmartEnum\<string> |    9    |
|   [9]   | Cadence rows      | `SubscriptionPolicy`   | record rows        |    3    |
|  [10]   | Quantity families | `QuantityFamily`       | SmartEnum\<string> |   15    |
|  [11]   | Receipt union     | `ComputeReceipt`       | [Union]            |   13    |
|  [12]   | Claim bands       | `BenchmarkClaim.Bands` | frozen rows        |    4    |
|  [13]   | Key policies      | `*KeyPolicy`           | comparer accessors |    7    |

Rail surfaces (one entrypoint family per rail):

| [INDEX] | [AXIS/CONCERN]      | [OWNER]                              | [KIND]                | [CASES]                                         |
| :-----: | :------------------ | :----------------------------------- | :-------------------- | :---------------------------------------------- |
|   [1]   | Admission rail      | `IntentAdmission`                    | static fold           | `Admit` — `Fin<AdmittedIntent>`                 |
|   [2]   | Selection rail      | `SubstrateSelection`                 | static fold           | `Plan`/`Select` — `Fin`                         |
|   [3]   | Kernel rail         | `TensorOps`                          | static table dispatch | 9 arity entrypoints — `Fin`                     |
|   [4]   | Layout and encoding | `TensorLayout` / `EncodedTensor`     | static + record       | `Reform` / `Of` — `Fin`                         |
|   [5]   | Equivalence rail    | `EquivalenceLaw`                     | static surface        | `Prove` — pure proof                            |
|   [6]   | Session capsule     | `ModelSessions`                      | boundary capsule      | `Boot`/`Lease`/`Unload` — `Fin`                 |
|   [7]   | Inference rail      | `RunOps`                             | extension fold        | `Infer`/`InferBound` — `Fin` in bracket         |
|   [8]   | Cache rail          | `CacheOps`                           | extension fold        | `Through` — `ValueTask`                         |
|   [9]   | Contract rail       | `ContractGuard`                      | static fold           | `Classify`/`AdditiveOnly` — `Fin`               |
|  [10]   | Channel capsule     | `WireChannels`                       | boundary capsule      | `Attach`/`Open`/`Observe`/`Redial` — `Fin`/`IO` |
|  [11]   | Call edge           | `CallSpine`                          | interceptor           | `Options`/`Bounded` + 4 overrides               |
|  [12]   | Frame rail          | `FrameEdge`                          | static surface        | `Frames`/`Parse`/`Write`/`Valid`                |
|  [13]   | Stream capsule      | `StreamPool`                         | boundary capsule      | `Get` + 11-event fold                           |
|  [14]   | Lane capsule        | `LaneRuntime`                        | boundary capsule      | `Enqueue`/`Pump`/`Drain` — `IO`                 |
|  [15]   | Progress capsule    | `ProgressCell`                       | capsule               | `Advance`/`Subscribe`/`Cancel`                  |
|  [16]   | Units rail          | `QuantityFamily.Admit`               | row members           | 4 arities — `Fin<UnitEvidence>`                 |
|  [17]   | Receipt rail        | `ReceiptSurface` / `ReceiptFolds`    | static + fold         | `Emit` — `IO`; 6 fold views                     |
|  [18]   | Claim rail          | `HostFingerprint` / `BenchmarkClaim` | records               | `Claim` — `Option`; `BandOf`/`Persist`/`Stale`  |

## [6]-[BUILD_ORDER]

Vocabulary owners first, then shapes, rails, dispatch, boundaries, composition. Seam notes honored: the intent/dispatch spine composes its axis vocabularies (C19 package-cumulative ruling), so axis files land before `Intent.cs`; `SelectionContext` carries the host fingerprint as a bare string with the typed `HostFingerprint` owner landing in `Receipts.cs` (fingerprint-slot ordering, no forward reference); the `WorkLane` accessor is `ComputeKeyPolicy`; TS_PROJECTION clusters transcribe at the TS workspace, never as package source.

| [INDEX] | [FILE]                  | [TRANSCRIBES]                                                                                                                                                  | [GATE]                 |
| :-----: | :---------------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------- | :--------------------- |
|   [1]   | `Faults.cs`             | intent-and-selection#DISPATCH_SPINE (`ComputeFault`), intent-and-selection#SUBSTRATE_AXIS (`ComputeKeyPolicy`)                                                 | static                 |
|   [2]   | `Tensors/Vocabulary.cs` | tensor-lane#TENSOR_VOCABULARY                                                                                                                                  | static + spec          |
|   [3]   | `Tensors/Operations.cs` | tensor-lane#OPERATION_FAMILIES                                                                                                                                 | static + spec          |
|   [4]   | `Tensors/Layout.cs`     | tensor-lane#LAYOUT_ALGEBRA                                                                                                                                     | static + spec          |
|   [5]   | `Units.cs`              | units-boundary#QUANTITY_TABLE, units-boundary#DIMENSIONAL_LAW, units-boundary#PARSE_FORMAT                                                                     | static + spec          |
|   [6]   | `Staging.cs`            | staging-and-streams#ALLOCATION_AXIS, staging-and-streams#PLANE_VIEWS, staging-and-streams#STREAM_POOL                                                          | static + spec          |
|   [7]   | `Progress.cs`           | progress-and-observation#PHASE_FAMILY, progress-and-observation#PROGRESS_CELL, progress-and-observation#OBSERVATION_SEAMS                                      | static + spec          |
|   [8]   | `Lanes.cs`              | scheduling-and-lanes#LANE_AXIS (`WorkLane`, `LaneHandle`), scheduling-and-lanes#CPU_BUDGET                                                                     | static + spec          |
|   [9]   | `Protos/*.proto`        | remote-lane#PROTO_VOCABULARY (service and message tables), remote-lane#FAULT_PROJECTION (`FaultDetail` row), remote-lane#ARTIFACT_FRAMES (`ArtifactFrame` row) | restore + static       |
|  [10]   | `Models/Providers.cs`   | model-lane#EP_AXIS                                                                                                                                             | static + spec          |
|  [11]   | `Models/Identity.cs`    | model-lane#MODEL_IDENTITY                                                                                                                                      | static + spec          |
|  [12]   | `Models/Sessions.cs`    | model-lane#SESSION_CAPSULE, model-lane#EXTENSION_OPS, model-lane#INFERENCE_MODES                                                                               | static + spec + bridge |
|  [13]   | `Models/Cache.cs`       | model-lane#RESULT_CACHE                                                                                                                                        | static + spec          |
|  [14]   | `Remote/Contract.cs`    | remote-lane#CONTRACT_EVOLUTION, remote-lane#FAULT_PROJECTION (`WireFault`)                                                                                     | static + spec-rail     |
|  [15]   | `Remote/Frames.cs`      | remote-lane#ARTIFACT_FRAMES (`FrameEdge`)                                                                                                                      | static + spec          |
|  [16]   | `Remote/Transports.cs`  | remote-lane#PROTO_VOCABULARY (`WireServices`), remote-lane#TRANSPORT_AXIS, remote-lane#CALL_POLICY                                                             | static + spec-rail     |
|  [17]   | `Intent.cs`             | intent-and-selection#INTENT_FAMILY, intent-and-selection#SUBSTRATE_AXIS, intent-and-selection#DISPATCH_SPINE (remaining owners)                                | static + spec          |
|  [18]   | `LaneRuntime.cs`        | scheduling-and-lanes#LANE_AXIS (`WorkItem`), scheduling-and-lanes#SOLVE_GUARD, scheduling-and-lanes#DRAIN_CANCEL                                               | static + spec          |
|  [19]   | `Receipts.cs`           | receipts-and-benchmarks#RECEIPT_UNION, receipts-and-benchmarks#FOLD_PROJECTIONS, receipts-and-benchmarks#WIRE_STAMPS                                           | static + spec          |
|  [20]   | `Benchmarks.cs`         | receipts-and-benchmarks#BENCHMARK_CLAIMS                                                                                                                       | static + spec          |

## [7]-[FILE_PROCESS]

1. Read this charter end-to-end: the file's BUILD_ORDER row, the DENSITY_BAR budget, and the PROHIBITIONS list.
2. Read every page named in the file's TRANSCRIBES cell end-to-end, plus the suite ledger SEAM_SPLITS rows naming those pages.
3. Resolve the file's gating RESEARCH rows through their PROOF routes before writing the gated fence content.
4. Transcribe signature fences verbatim; add only file-organization scaffolding (section separators, usings, namespace).
5. Run the collapse scan per edit: 3 or more parallel types, sibling factories, repeated switch arms, or single-call helpers triggers in-place collapse.
6. Run `uv run python -m tools.assay static fix`, then `uv run python -m tools.assay static build` on the touched closure; build is authoritative.
7. Author specs per the `testing-cs` skill against the page laws (tolerance classes, monotonic ranks, partition exactness, fold algebra) and run the spec gate.
8. Bridge scenarios gate host seams (plugin-ALC native load, UDS attach) through `bridge verify` before the seam is declared landed.

## [8]-[PROOF_GATES]

| [GATE]    | [COMMAND]                                                                                                 | [EVIDENCE]                                                                        |
| :-------- | :-------------------------------------------------------------------------------------------------------- | :-------------------------------------------------------------------------------- |
| restore   | `dotnet restore libs/csharp/Rasm.Compute/Rasm.Compute.csproj`                                             | `packages.lock.json` unchanged; zero NU1004                                       |
| catalogue | `uv run python -m tools.assay api doctor` and `api resolve <key>`                                         | every Compute package key reports assembly present; catalogue pages current       |
| static    | `uv run python -m tools.assay static plan --language csharp libs/csharp/Rasm.Compute` then `static build` | routed closure compiles; zero `': error '` diagnostics                            |
| spec      | `uv run python -m tools.assay test run --target Rasm.Compute.Tests`                                       | green run; CsCheck laws hold without tolerance loosening                          |
| bridge    | `uv run python -m tools.assay bridge verify --pattern tests/csharp/libs/Rasm.Compute`                     | host-seam scenarios pass (plugin-ALC ONNX load, UDS attach)                       |
| spec-rail | `uv run python -m tools.assay test run --target Rasm.Compute.Tests` (CallSpine, WireFault specs)          | Grpc.Core.Api members compile on the spec rail (transitive package, no assay key) |
| render    | `uv run python -m tools.assay docs check libs/csharp/Rasm.Compute`                                        | every Mermaid block renders through the local `mmdc` route                        |

## [9]-[PROHIBITIONS]

- [NEVER] add a public surface beside the budgeted owners; a new capability is a row or case on an existing axis.
- [NEVER] create wrappers, rename adapters, helpers, or utility files — no TensorService, no package-local tensor or plane wrappers, no preprocessing or tokenizer service family.
- [NEVER] introduce a generic IReceipt/ledger abstraction; `ComputeReceipt` cases stay typed.
- [NEVER] propagate sentinels inward; NaN and sentinel outputs project to `Option` at the boundary.
- [NEVER] use `DateTime.UtcNow`; BCL `DateTime` never sits between wire and rail — NodaTime bridges own the temporal edge.
- [NEVER] add a second cache owner: `CacheSurface` over `CacheLane.ModelResult` is the single cache; hand-rolled memoization beside it is the named defect.
- [NEVER] add a second retry owner: `GrpcChannelOptions.ServiceConfig` is never set; the AppHost keyed Polly hop owns retry; a detected second owner raises `RetryOwnerConflict`.
- [NEVER] mint a second correlation, HLC stamp, health probe, or wire phase vocabulary.
- [NEVER] write the superseded ONNX spellings: `NamedOnnxValue`, `DisposableNamedOnnxValue`, `FixedBufferOnnxValue`, `AppendExecutionProvider_CoreML(CoreMLFlags)`; OrtValue-only law holds.
- [NEVER] pool sessions: one shared `InferenceSession` per checksum; per-dtype kernel copies and phantom `Tensor.CreateFrom*` factory spellings stay deleted.
- [NEVER] admit `System.IO.Pipelines`, naked `ArrayPool<T>.Shared` rents, raw `MemoryStream` construction, unbounded channels, Dataflow lanes, or `IProgress<T>` plumbing — the owning axes delete those forms.
- [NEVER] re-declare settled constants: channel policy values, frame constants, drain budgets, and the dedup window quote their owners.
- [NEVER] let a quantity type cross an interior signature or a wire; conversion runs exactly once at admission.
- [NEVER] loosen a `ToleranceClass` bound to pass equivalence — fix the kernel; performance routes bind only behind a winning fingerprint-matched claim row.
- [NEVER] set `UnsafeUseInsecureChannelCallCredentials` or `ThrowOperationCanceledOnCancellation`; `RpcException` conversion lives in the one `WireFault.Classify` arm.
- [NEVER] treat CSP analyzer diagnostics as suppression targets; they are architecture pressure.

## [10]-[ADMISSIONS_RECORD]

| [PACKAGE]                            | [VERSION] | [PAGE]                                        | [CATALOGUE]              |
| :----------------------------------- | :-------- | :-------------------------------------------- | :----------------------- |
| CommunityToolkit.HighPerformance     | 8.4.2     | tensor-lane, staging-and-streams              | api-highperformance.md   |
| Google.Protobuf                      | 3.35.1    | remote-lane, receipts-and-benchmarks          | api-protobuf.md          |
| Grpc.Net.Client                      | 2.80.0    | remote-lane                                   | api-grpc-client.md       |
| Grpc.Net.Client.Web                  | 2.80.0    | remote-lane                                   | api-grpc-client-web.md   |
| Grpc.Tools                           | 2.81.1    | remote-lane                                   | api-grpc-tools.md        |
| Microsoft.IO.RecyclableMemoryStream  | 3.0.1     | staging-and-streams, remote-lane              | api-recyclable-stream.md |
| Microsoft.ML.OnnxRuntime             | 1.26.0    | model-lane, tensor-lane, intent-and-selection | api-onnxruntime.md       |
| Microsoft.ML.OnnxRuntime.Extensions  | 0.14.0    | model-lane                                    | api-onnx-extensions.md   |
| NodaTime.Serialization.Protobuf      | 2.0.2     | remote-lane, receipts-and-benchmarks          | api-nodatime-protobuf.md |
| System.Numerics.Tensors              | 10.0.9    | tensor-lane                                   | api-tensors.md           |
| Thinktecture.Runtime.Extensions.Json | 10.2.0    | receipts-and-benchmarks                       | doctrine (stack atlas)   |
| UnitsNet                             | 5.75.0    | units-boundary                                | api-unitsnet.md          |
