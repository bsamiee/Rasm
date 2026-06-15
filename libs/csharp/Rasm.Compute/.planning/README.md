# [COMPUTE_PLANNING]

Rasm.Compute has zero consumers; the implementation is full-capability with no holding back. These pages are decision-complete blueprints an implementation agent transcribes — never re-designed downstream. The package owns measured execution: intent and substrate selection, tensor and model lanes, the remote lane carrying the suite wire vocabulary, staging memory, scheduling, progress, the units boundary, and typed receipts — consuming AppHost ports and Persistence stores as settled vocabulary.

## [1]-[PAGE_INDEX]

| [INDEX] | [PAGE]                                                  | [OWNS]                                                                            |
| :-----: | ------------------------------------------------------- | -------------------------------------------------------------------------------- |
|   [1]   | [intent-and-selection](intent-and-selection.md)         | Typed intent family; substrate-selection rail; total dispatch                    |
|   [2]   | [tensor-lane](tensor-lane.md)                           | Tensor shapes, dtype map, layout algebra, geometry encoding; TensorOpFamily table; tolerance vocabulary; kernel dispatch |
|   [3]   | [model-lane](model-lane.md)                             | ONNX identity, session capsule, EP rows, extension-op admission; OrtValue run modes; bound-loop hot path; result cache |
|   [4]   | [remote-lane](remote-lane.md)                           | Proto wire vocabulary; transports; channel capsule; credential axis; artifact frame law; buffer fast path; suite TS wire posture |
|   [5]   | [staging-and-streams](staging-and-streams.md)           | AllocationClass rows; pooled memory; recyclable streams                          |
|   [6]   | [scheduling-and-lanes](scheduling-and-lanes.md)         | WorkLane channels; solve-path guard; drain participation                         |
|   [7]   | [progress-and-observation](progress-and-observation.md) | Monotonic phases; zero-alloc capsules; observation seams                         |
|   [8]   | [units-boundary](units-boundary.md)                     | QuantityFamily rows; conversion-at-admission; unit evidence                      |
|   [9]   | [receipts-and-benchmarks](receipts-and-benchmarks.md)   | Receipt union; fold projections; benchmark claims                                |

## [2]-[WIRE_PAGES]

remote-lane · progress-and-observation · receipts-and-benchmarks (each carries exactly one TS_PROJECTION cluster).

## [3]-[CATALOGUE_PENDING]

- App-root server packages (Grpc.AspNetCore trio) catalogued at app-root creation.
- `Microsoft.AspNetCore.TestHost` — the `RemoteTransport.InProcess` row's `TestServer.CreateHandler` handler seam; lands as a test-only `<PackageReference>` in the spec project at the matched ASP.NET Core servicing line.
- `NodaTime.Serialization.Protobuf` — the `Google.Type` calendar-date wire bridges on remote-lane and receipts-and-benchmarks (`api-nodatime-protobuf.md`); lands as a direct package reference only when the `Google.Api.CommonProtos` calendar surface does not resolve transitively through the admitted NodaTime and Grpc graph.

## [4]-[GAP_LEDGER]

Every adversarial-verifier gap finding for the package; a row is present only when closed by its `[CLOSED_BY (page#cluster)]` page, so every row is CLOSED.

| [INDEX] | [GAP]                                     | [CLOSED_BY (page#cluster)]            |
| :-----: | :---------------------------------------- | :------------------------------------ |
|   [1]   | EP-parameterized substrate row            | intent-and-selection                  |
|   [2]   | `DocumentService` wire vocabulary         | remote-lane                           |
|   [3]   | `ControlService` wire vocabulary          | remote-lane                           |
|   [4]   | `FaultDetail` status projection           | remote-lane + receipts-and-benchmarks |
|   [5]   | descriptor-diff contract evolution        | remote-lane                           |
|   [6]   | transport streaming-capability column     | remote-lane                           |
|   [7]   | keepalive policy rows                     | remote-lane                           |
|   [8]   | payload caps, frames, hashes, zero-copy   | remote-lane + staging-and-streams     |
|   [9]   | credential policy and peer credentials    | remote-lane                           |
|  [10]   | compression axis                          | remote-lane                           |
|  [11]   | interceptor and call-invoker seam         | remote-lane                           |
|  [12]   | single retry owner and conflict receipts  | remote-lane + receipts-and-benchmarks |
|  [13]   | `grpc.health.v1` and node affinity        | remote-lane                           |
|  [14]   | `WorkLane` name and solve-path guard      | scheduling-and-lanes                  |
|  [15]   | scheduler-valued progress subscriptions   | progress-and-observation              |
|  [16]   | geometry-to-tensor encoding cluster       | tensor-lane                           |
|  [17]   | tensor layout family surface              | tensor-lane                           |
|  [18]   | CoreML EP and EP-context artifacts        | model-lane                            |
|  [19]   | ONNX terminate-latch cancellation cadence | model-lane                            |
|  [20]   | NodaTime-protobuf wire bridges            | remote-lane + receipts-and-benchmarks |
|  [21]   | benchmark claims and profiling artifacts  | receipts-and-benchmarks               |
|  [22]   | canonical proto geometry                  | remote-lane                           |
|  [23]   | UnitsNet `QuantityInfo` research row      | units-boundary                        |
|  [24]   | discovery manifest on UDS transport       | remote-lane                           |

## [5]-[DENSITY_BAR]

Implementation collapses to one owner per axis and one entrypoint family per rail; density means no parallel rails, no near-duplicate shapes, no re-derived logic — a file is as large as its owner's concern requires, never trimmed to a line count. A new feature is a row or case, never a new surface. Dispatch runs over row data through generated total Switches and frozen tables. Seven comparer accessors exist, one per axis owner and package-local; the `WorkLane` key accessor is `ComputeKeyPolicy`, never the AppHost `LaneKeyPolicy` (suite ledger key-policy posture). `[STATE]` carries `FINALIZED` where the owner is a transcription-complete fence with no open gate and `SPIKE` where the owner is fence-complete but its proof carries a residual native, bridge, or live-server probe named in the page's RESEARCH cluster — a SPIKE owner is fully shaped now, never a deferred surface.

Axis owners (vocabulary budget):

| [INDEX] | [AXIS/CONCERN]     | [OWNER]             | [KIND]                   |   [CASES]   |  [STATE]  |
| :-----: | :----------------- | :------------------ | :----------------------- | :---------: | :-------: |
|   [1]   | Intent family      | `ComputeIntent`     | [Union] + nested `Spec`  |      5      | FINALIZED |
|   [2]   | Substrate axis     | `Substrate`         | SmartEnum\<string>       |      3      | FINALIZED |
|   [3]   | Fault family       | `ComputeFault`      | [Union] fault, band 2200 |     13      | FINALIZED |
|   [4]   | Total dispatch     | `DispatchTable`     | record                   | 3 delegates | FINALIZED |
|   [5]   | Tensor dtypes      | `TensorDtype`       | SmartEnum\<string>       |     10      | FINALIZED |
|   [6]   | Tensor op kinds    | `TensorOpKind`      | SmartEnum\<string>       |     12      | FINALIZED |
|   [7]   | Tensor op families | `TensorOpFamily`    | SmartEnum\<string>       |     82      | FINALIZED |
|   [8]   | Tolerance classes  | `ToleranceClass`    | SmartEnum\<string>       |      4      | FINALIZED |
|   [9]   | Layout forms       | `LayoutForm`        | SmartEnum\<string>       |      5      | FINALIZED |
|  [10]   | Encoding channels  | `EncodingChannel`   | SmartEnum\<string>       |      6      | FINALIZED |
|  [11]   | Geometry encodings | `GeometryEncoding`  | [Union]                  |      3      | FINALIZED |
|  [12]   | Model acquisition  | `ModelSource`       | [Union]                  |      4      | FINALIZED |
|  [13]   | EP axis            | `ExecutionProvider` | SmartEnum\<string>       |      4      | FINALIZED |
|  [14]   | Cache postures     | `CachePolicy`       | SmartEnum\<string>       |      4      | FINALIZED |

| [INDEX] | [AXIS/CONCERN]    | [OWNER]                | [KIND]             | [CASES] |  [STATE]  |
| :-----: | :---------------- | :--------------------- | :----------------- | :-----: | :-------: |
|   [1]   | Contract drift    | `ContractDrift`        | [Union]            |    3    | FINALIZED |
|   [2]   | Transport axis    | `RemoteTransport`      | SmartEnum\<string> |    6    | FINALIZED |
|   [3]   | Stream shapes     | `StreamShape`          | [Flags] enum       |    4    | FINALIZED |
|   [4]   | Node selection    | `NodeSelection`        | enum               |    3    | FINALIZED |
|   [5]   | Credential axis   | `CredentialPolicy`     | SmartEnum\<string> |    4    | FINALIZED |
|   [6]   | Allocation axis   | `AllocationClass`      | SmartEnum\<string> |    5    | FINALIZED |
|   [7]   | Work lanes        | `WorkLane`             | SmartEnum\<string> |    5    | FINALIZED |
|   [8]   | Progress phases   | `ProgressPhase`        | SmartEnum\<string> |    9    | FINALIZED |
|   [9]   | Cadence rows      | `SubscriptionPolicy`   | record rows        |    3    | FINALIZED |
|  [10]   | Quantity families | `QuantityFamily`       | SmartEnum\<string> |   15    | FINALIZED |
|  [11]   | Receipt union     | `ComputeReceipt`       | [Union]            |   13    | FINALIZED |
|  [12]   | Claim bands       | `BenchmarkClaim.Bands` | frozen rows        |    4    | FINALIZED |
|  [13]   | Key policies      | `*KeyPolicy`           | comparer accessors |    7    | FINALIZED |

Rail surfaces (one entrypoint family per rail):

| [INDEX] | [AXIS/CONCERN]      | [OWNER]                              | [KIND]                | [CASES]                                         |  [STATE]  |
| :-----: | :------------------ | :----------------------------------- | :-------------------- | :---------------------------------------------- | :-------: |
|   [1]   | Admission rail      | `IntentAdmission`                    | static fold           | `Admit` — `Fin<AdmittedIntent>`                 | FINALIZED |
|   [2]   | Selection rail      | `SubstrateSelection`                 | static fold           | `Plan`/`Select` — `Fin`                         | FINALIZED |
|   [3]   | Kernel rail         | `TensorOps`                          | static table dispatch | 9 arity entrypoints — `Fin`                     | FINALIZED |
|   [4]   | Layout and encoding | `TensorLayout` / `EncodedTensor`     | static + record       | `Reform` / `Of` — `Fin`                         | FINALIZED |
|   [5]   | Equivalence rail    | `EquivalenceLaw`                     | static surface        | `Prove` — pure proof                            | FINALIZED |
|   [6]   | Session capsule     | `ModelSessions`                      | boundary capsule      | `Boot`/`Lease`/`Unload` — `Fin`                 |   SPIKE   |
|   [7]   | Inference rail      | `RunOps`                             | extension fold        | `Infer`/`InferBound` — `Fin` in bracket         |   SPIKE   |
|   [8]   | Cache rail          | `CacheOps`                           | extension fold        | `Through` — `ValueTask`                         | FINALIZED |
|   [9]   | Contract rail       | `ContractGuard`                      | static fold           | `Classify`/`AdditiveOnly` — `Fin`               | FINALIZED |
|  [10]   | Channel capsule     | `WireChannels`                       | boundary capsule      | `Attach`/`Open`/`Observe`/`Redial` — `Fin`/`IO` |   SPIKE   |
|  [11]   | Call edge           | `CallSpine`                          | interceptor           | `Options`/`Bounded` + 4 overrides               |   SPIKE   |
|  [12]   | Frame rail          | `FrameEdge`                          | static surface        | `Frames`/`Parse`/`Write`/`Prefixed`/`Merge`/`Valid` | FINALIZED |
|  [13]   | Stream capsule      | `StreamPool`                         | boundary capsule      | `Get` + 11-event fold                           | FINALIZED |
|  [14]   | Lane capsule        | `LaneRuntime`                        | boundary capsule      | `Enqueue`/`Pump`/`Drain` — `IO`                 | FINALIZED |
|  [15]   | Progress capsule    | `ProgressCell`                       | capsule               | `Advance`/`Subscribe`/`Cancel`                  | FINALIZED |
|  [16]   | Units rail          | `QuantityFamily.Admit`               | row members           | 4 arities — `Fin<UnitEvidence>`                 | FINALIZED |
|  [17]   | Receipt rail        | `ReceiptSurface` / `ReceiptFolds`    | static + fold         | `Emit` — `IO`; 6 fold views                     | FINALIZED |
|  [18]   | Claim rail          | `HostFingerprint` / `BenchmarkClaim` | records               | `Claim` — `Option`; `BandOf`/`Persist`/`Stale`  | FINALIZED |

## [6]-[BUILD_ORDER]

Vocabulary owners first, then shapes, rails, dispatch, boundaries, composition. Seam notes honored: the intent/dispatch spine composes its axis vocabularies (package-cumulative resolution ruling), so axis files land before `Intent.cs`; `SelectionContext` carries the host fingerprint as a bare string with the typed `HostFingerprint` owner landing in `Receipts.cs` (fingerprint-slot ordering, no forward reference); the `WorkLane` accessor is `ComputeKeyPolicy`; TS_PROJECTION clusters transcribe at the TS workspace, never as package source.

Cluster cells use page-local anchor names; proof cells name evidence beyond the standard static/spec gate.

| [INDEX] | [FILE]                  | [CLUSTERS]                               | [PROOF]          |
| :-----: | :---------------------- | :--------------------------------------- | :--------------- |
|   [1]   | `Faults.cs`             | faults and key policy                    | static           |
|   [2]   | `Tensors/Vocabulary.cs` | tensor vocabulary                        | specs            |
|   [3]   | `Tensors/Operations.cs` | operation families                       | specs            |
|   [4]   | `Tensors/Layout.cs`     | layout algebra                           | specs            |
|   [5]   | `Units.cs`              | quantities, dimensions, parse/format     | specs            |
|   [6]   | `Staging.cs`            | allocation, plane views, stream pool     | specs            |
|   [7]   | `Progress.cs`           | phases, progress cell, observation seams | specs            |
|   [8]   | `Lanes.cs`              | lane axis, handles, CPU budget           | specs            |
|   [9]   | `Protos/*.proto`        | services, faults, artifact frames        | restore + static |
|  [10]   | `Models/Providers.cs`   | execution providers                      | specs            |
|  [11]   | `Models/Identity.cs`    | model identity                           | specs            |
|  [12]   | `Models/Sessions.cs`    | sessions, extension ops, inference modes | specs + bridge   |
|  [13]   | `Models/Cache.cs`       | result cache                             | specs            |
|  [14]   | `Remote/Contract.cs`    | contract evolution, wire faults          | spec rail        |
|  [15]   | `Remote/Frames.cs`      | artifact frames                          | specs            |
|  [16]   | `Remote/Transports.cs`  | services, transport, call policy         | spec rail        |
|  [17]   | `Intent.cs`             | intent, substrate, dispatch              | specs            |
|  [18]   | `LaneRuntime.cs`        | work items, solve guard, drain cancel    | specs            |
|  [19]   | `Receipts.cs`           | receipts, folds, wire stamps             | specs            |
|  [20]   | `Benchmarks.cs`         | benchmark claims                         | specs            |

## [7]-[FILE_PROCESS]

1. Read this charter end-to-end: the file's BUILD_ORDER row, the DENSITY_BAR budget, and the PROHIBITIONS list.
2. Read every page named in the file's TRANSCRIBES cell end-to-end, plus the suite ledger SEAM_SPLITS rows naming those pages.
3. Resolve the file's open RESEARCH items before writing the gated fence content; proof runs on the PROOF_GATES rails.
4. Transcribe signature fences verbatim; add only file-organization scaffolding (section separators, usings, namespace).
5. Run the collapse scan per edit: 3 or more parallel types, sibling factories, repeated switch arms, or single-call helpers triggers in-place collapse.
6. Run `uv run python -m tools.assay static fix`, then `uv run python -m tools.assay static build` on the touched closure; build is authoritative.
7. Author specs per the `testing-cs` skill against the page laws (tolerance classes, monotonic ranks, partition exactness, fold algebra) and run the spec gate.
8. Bridge scenarios gate host seams (plugin-ALC native load, UDS attach) through `bridge verify` before the seam is declared landed.

## [8]-[PROOF_GATES]

Assay rows use `uv run python -m tools.assay`; proof runs at the planned phase gate, not after each edit.

| [GATE] | [RAIL]                         | [EVIDENCE]                                    |
| :----: | :----------------------------- | :-------------------------------------------- |
|  [G1]  | `dotnet restore` Compute       | lockfile unchanged; zero NU1004               |
|  [G2]  | `api doctor` + `api resolve`   | package keys resolve; catalogues current      |
|  [G3]  | `static plan` + `static build` | routed closure compiles                       |
|  [G4]  | `test run` Compute target      | CsCheck laws hold without tolerance loosening |
|  [G5]  | `bridge verify` scenarios      | plugin-ALC ONNX load and UDS attach pass      |
|  [G6]  | G4 spec rail                   | `Grpc.Core.Api` members compile               |
|  [G7]  | `docs check` Compute           | Mermaid blocks render through local `mmdc`    |

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

The executed admissions ledger maps each package to its consuming page, `.api` catalogue, and admission status. Versions live in `Directory.Packages.props`; this table never carries a pin. `[STATUS]` in {catalogue-pending, app-root-pending, tests-only, admitted}.

| [INDEX] | [PACKAGE]                            | [PAGE]                                        | [CATALOGUE]                        | [STATUS]          |
| :-----: | :----------------------------------- | :-------------------------------------------- | :--------------------------------- | :---------------- |
|   [1]   | CommunityToolkit.HighPerformance     | tensor-lane, staging-and-streams              | api-highperformance.md             | admitted          |
|   [2]   | Google.Protobuf                      | remote-lane, receipts-and-benchmarks          | api-protobuf.md                    | admitted          |
|   [3]   | Grpc.Net.Client                      | remote-lane                                   | api-grpc-client.md                 | admitted          |
|   [4]   | Grpc.Net.Client.Web                  | remote-lane                                   | api-grpc-client-web.md             | admitted          |
|   [5]   | Grpc.Tools                           | remote-lane                                   | api-grpc-tools.md                  | admitted          |
|   [6]   | Microsoft.AspNetCore.TestHost        | remote-lane                                   | api-microsoftaspnetcoretesthost.md | tests-only        |
|   [7]   | Microsoft.IO.RecyclableMemoryStream  | staging-and-streams, remote-lane              | api-recyclable-stream.md           | admitted          |
|   [8]   | Microsoft.ML.OnnxRuntime             | model-lane, tensor-lane, intent-and-selection | api-onnxruntime.md                 | admitted          |
|   [9]   | Microsoft.ML.OnnxRuntime.Extensions  | model-lane                                    | api-onnx-extensions.md             | admitted          |
|  [10]   | System.Numerics.Tensors              | tensor-lane                                   | api-tensors.md                     | admitted          |
|  [11]   | Thinktecture.Runtime.Extensions      | all pages                                     | doctrine (stack atlas)             | admitted          |
|  [12]   | Thinktecture.Runtime.Extensions.Json | receipts-and-benchmarks                       | doctrine (stack atlas)             | admitted          |
|  [13]   | UnitsNet                             | units-boundary                                | api-unitsnet.md                    | admitted          |
|  [14]   | SharpFuzz                            | remote-lane, tensor-lane                      | pending — NEEDS-ADMISSION          | catalogue-pending |
|  [15]   | BenchmarkDotNet                      | model-lane                                    | pending                            | catalogue-pending |

## [11]-[REFINEMENT_HORIZON]

Entry for the next deepening session: `libs/csharp/.planning/campaign-method.md` then `TASKLOG.md` then this charter. Folder-specific deepening targets: geometry-to-tensor encoding widened against real model zoos as the uncatalogued `TensorPrimitives` operator surface on `tensor-lane#OPERATION_TABLE` resolves into `TensorOpFamily` rows; the compute farm topology — node affinity, EP-context warm-start artifacts — rehearsed end-to-end against the designed-only `RemoteTransport` byte paths whose `PipeSecurity` ACL and loopback DACL member spellings live on `remote-lane#[PIPE_SECURITY]`; load/offload choreography between Rhino-hosted and companion processes proven over the wire vocabulary once the Kestrel `ListenUnixSocket` server leg and the `Grpc.Core.Api` transitive route land from START_GATES [3] and [4]; the ALC spikes — ONNX dylib native load and `libortextensions.dylib` RID resolution from TASKLOG §7[4] — executed with the `RunOptions.Terminate` latch propagation cadence on the CoreML and CPU rows resolved at `model-lane#INFERENCE_MODES`. The bar: apps move compute between host, companion, and farm rows without new surfaces, with receipts proving every hop.

Testing-infrastructure horizon: the `RemoteTransport.InProcess` transport seam proves the hand-off through `TestServer.CreateHandler` (`Microsoft.AspNetCore.TestHost`, admitted) without a live remote; the protobuf decode and tensor binary-frame admission ride `SharpFuzz` `Fuzzer.OutOfProcess.Run` (`NEEDS-ADMISSION`); durable kernel speed and the zero-alloc `OrtIoBinding` posture land on the BenchmarkDotNet rail.
