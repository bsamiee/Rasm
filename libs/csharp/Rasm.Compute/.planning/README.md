# [COMPUTE_PLANNING]

Rasm.Compute has zero consumers; the implementation is full-capability with no holding back. These pages are decision-complete blueprints an implementation agent transcribes — never re-designed downstream. The package owns measured execution: intent and substrate selection, tensor and model lanes, the remote lane carrying the suite wire vocabulary, staging memory, scheduling, progress, the units boundary, and typed receipts — consuming AppHost ports and Persistence stores as settled vocabulary.

## [1]-[PAGE_INDEX]

| [INDEX] | [PAGE]                                                  | [OWNS]                                                              |
| :-----: | ------------------------------------------------------- | ------------------------------------------------------------------- |
|   [1]   | [intent-and-selection](intent-and-selection.md)         | Typed intent family; substrate-selection rail; total dispatch       |
|   [2]   | [tensor-lane](tensor-lane.md)                           | CPU tensor substrate; shape algebra; geometry-to-tensor encoding    |
|   [3]   | [model-lane](model-lane.md)                             | ONNX identity, session capsule, EP rows, extension-op admission     |
|   [4]   | [remote-lane](remote-lane.md)                           | Proto wire vocabulary; transports; channel capsule; credential axis |
|   [5]   | [staging-and-streams](staging-and-streams.md)           | AllocationClass rows; pooled memory; recyclable streams             |
|   [6]   | [scheduling-and-lanes](scheduling-and-lanes.md)         | WorkLane channels; solve-path guard; drain participation            |
|   [7]   | [progress-and-observation](progress-and-observation.md) | Monotonic phases; zero-alloc capsules; observation seams            |
|   [8]   | [units-boundary](units-boundary.md)                     | QuantityFamily rows; conversion-at-admission; unit evidence         |
|   [9]   | [receipts-and-benchmarks](receipts-and-benchmarks.md)   | Receipt union; fold projections; benchmark claims                   |

## [2]-[WIRE_PAGES]

remote-lane · progress-and-observation · receipts-and-benchmarks (each carries exactly one TS_PROJECTION cluster).

## [3]-[CATALOGUE_PENDING]

- App-root server packages (Grpc.AspNetCore trio) catalogued at app-root creation.
- `Microsoft.AspNetCore.TestHost` — the `RemoteTransport.InProcess` row's `TestServer.CreateHandler` handler seam; lands as a test-only `<PackageReference>` in the spec project at the matched ASP.NET Core servicing line.
- `NodaTime.Serialization.Protobuf` — the `Google.Type` calendar-date wire bridges on remote-lane and receipts-and-benchmarks (`api-nodatime-protobuf.md`); lands as a direct `<PackageVersion>` only when the `Google.Api.CommonProtos` calendar surface does not resolve transitively through the admitted NodaTime and Grpc graph.

## [4]-[GAP_LEDGER]

`[OWNER]` names the page that absorbed the gap; `[STATE]` is the feature-state value — `FINALIZED` as a transcription-complete fence, `SPIKE` gated on a probe.

| [INDEX] | [GAP]                                     | [OWNER]                               | [STATE]   |
| :-----: | :---------------------------------------- | :------------------------------------ | :-------- |
|   [1]   | EP-parameterized substrate row            | intent-and-selection                  | FINALIZED |
|   [2]   | `DocumentService` wire vocabulary         | remote-lane                           | FINALIZED |
|   [3]   | `ControlService` wire vocabulary          | remote-lane                           | FINALIZED |
|   [4]   | `FaultDetail` status projection           | remote-lane + receipts-and-benchmarks | FINALIZED |
|   [5]   | descriptor-diff contract evolution        | remote-lane                           | FINALIZED |
|   [6]   | transport streaming-capability column     | remote-lane                           | FINALIZED |
|   [7]   | keepalive policy rows                     | remote-lane                           | FINALIZED |
|   [8]   | payload caps, frames, hashes, zero-copy   | remote-lane + staging-and-streams     | FINALIZED |
|   [9]   | credential policy and peer credentials    | remote-lane                           | FINALIZED |
|  [10]   | compression axis                          | remote-lane                           | FINALIZED |
|  [11]   | interceptor and call-invoker seam         | remote-lane                           | FINALIZED |
|  [12]   | single retry owner and conflict receipts  | remote-lane + receipts-and-benchmarks | FINALIZED |
|  [13]   | `grpc.health.v1` and node affinity        | remote-lane                           | FINALIZED |
|  [14]   | `WorkLane` name and solve-path guard      | scheduling-and-lanes                  | FINALIZED |
|  [15]   | scheduler-valued progress subscriptions   | progress-and-observation              | FINALIZED |
|  [16]   | geometry-to-tensor encoding cluster       | tensor-lane                           | FINALIZED |
|  [17]   | tensor layout family surface              | tensor-lane                           | FINALIZED |
|  [18]   | CoreML EP and EP-context artifacts        | model-lane                            | FINALIZED |
|  [19]   | ONNX terminate-latch cancellation cadence | model-lane                            | SPIKE     |
|  [20]   | NodaTime-protobuf wire bridges            | remote-lane + receipts-and-benchmarks | FINALIZED |
|  [21]   | benchmark claims and profiling artifacts  | receipts-and-benchmarks               | FINALIZED |
|  [22]   | canonical proto geometry                  | remote-lane                           | FINALIZED |
|  [23]   | UnitsNet `QuantityInfo` research row      | units-boundary                        | FINALIZED |
|  [24]   | discovery manifest on UDS transport       | remote-lane                           | FINALIZED |

## [5]-[DENSITY_BAR]

Implementation lands at 25-35% of naive LOC: one owner per axis, one entrypoint family per rail, dispatch over row data through generated total Switches and frozen tables. A new feature is one row or one case on a budgeted owner — never a new surface. Seven comparer accessors exist, one per axis owner and package-local; the `WorkLane` key accessor is `ComputeKeyPolicy`, never the AppHost `LaneKeyPolicy` (suite ledger key-policy posture).

Axis owners (vocabulary budget):

| [INDEX] | [AXIS/CONCERN]     | [OWNER]             | [KIND]                   |   [CASES]   |
| :-----: | :----------------- | :------------------ | :----------------------- | :---------: |
|   [1]   | Intent family      | `ComputeIntent`     | [Union] + nested `Spec`  |      5      |
|   [2]   | Substrate axis     | `Substrate`         | SmartEnum\<string>       |      3      |
|   [3]   | Fault family       | `ComputeFault`      | [Union] fault, band 2200 |     13      |
|   [4]   | Total dispatch     | `DispatchTable`     | record                   | 3 delegates |
|   [5]   | Tensor dtypes      | `TensorDtype`       | SmartEnum\<string>       |     10      |
|   [6]   | Tensor op kinds    | `TensorOpKind`      | SmartEnum\<string>       |     10      |
|   [7]   | Tensor op families | `TensorOpFamily`    | SmartEnum\<string>       |     52      |
|   [8]   | Tolerance classes  | `ToleranceClass`    | SmartEnum\<string>       |      4      |
|   [9]   | Layout forms       | `LayoutForm`        | SmartEnum\<string>       |      5      |
|  [10]   | Encoding channels  | `EncodingChannel`   | SmartEnum\<string>       |      6      |
|  [11]   | Geometry encodings | `GeometryEncoding`  | [Union]                  |      3      |
|  [12]   | Model acquisition  | `ModelSource`       | [Union]                  |      4      |
|  [13]   | EP axis            | `ExecutionProvider` | SmartEnum\<string>       |      4      |
|  [14]   | Cache postures     | `CachePolicy`       | SmartEnum\<string>       |      4      |

| [INDEX] | [AXIS/CONCERN]    | [OWNER]                | [KIND]             | [CASES] |
| :-----: | :---------------- | :--------------------- | :----------------- | :-----: |
|   [1]   | Contract drift    | `ContractDrift`        | [Union]            |    3    |
|   [2]   | Transport axis    | `RemoteTransport`      | SmartEnum\<string> |    6    |
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

| [PACKAGE]                            | [PAGE]                                        | [CATALOGUE]              |
| :----------------------------------- | :-------------------------------------------- | :----------------------- |
| CommunityToolkit.HighPerformance     | tensor-lane, staging-and-streams              | api-highperformance.md   |
| Google.Protobuf                      | remote-lane, receipts-and-benchmarks          | api-protobuf.md          |
| Grpc.Net.Client                      | remote-lane                                   | api-grpc-client.md       |
| Grpc.Net.Client.Web                  | remote-lane                                   | api-grpc-client-web.md   |
| Grpc.Tools                           | remote-lane                                   | api-grpc-tools.md        |
| Microsoft.IO.RecyclableMemoryStream  | staging-and-streams, remote-lane              | api-recyclable-stream.md |
| Microsoft.ML.OnnxRuntime             | model-lane, tensor-lane, intent-and-selection | api-onnxruntime.md       |
| Microsoft.ML.OnnxRuntime.Extensions  | model-lane                                    | api-onnx-extensions.md   |
| System.Numerics.Tensors              | tensor-lane                                   | api-tensors.md           |
| Thinktecture.Runtime.Extensions      | all pages                                     | doctrine (stack atlas)   |
| Thinktecture.Runtime.Extensions.Json | receipts-and-benchmarks                       | doctrine (stack atlas)   |
| UnitsNet                             | units-boundary                                | api-unitsnet.md          |

## [11]-[REFINEMENT_HORIZON]

Entry for the next deepening session: `libs/csharp/.planning/campaign-method.md` then `TASKLOG.md` then this charter. Folder-specific deepening targets: geometry-to-tensor encoding widened against real model zoos; the compute farm topology (node affinity, warm-start artifacts) rehearsed end-to-end; load/offload choreography between Rhino-hosted and companion processes proven over the wire vocabulary; the ALC spikes (ONNX dylib, Kestrel) executed from TASKLOG. The bar: apps move compute between host, companion, and farm rows without new surfaces, with receipts proving every hop.
