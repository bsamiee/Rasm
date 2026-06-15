# [RASM_COMPUTE_ROADMAP]

The implementation path and its start gates for a design-finalized corpus. Implementation proceeds in the charter BUILD_ORDER: start gates resolve before the clusters they unblock, every task exits against named planning-page clusters, and every exit proves through a charter PROOF_GATES row. Owner realization state lives on the charter DENSITY_BAR `[STATE]` column; this roadmap routes to it and never mirrors per-owner values. The charter is `.planning/README.md`; pages are transcribed, never re-designed.

## [1]-[CURRENT_POSITION]

- Surface: no transcription has landed; the package is pre-source with a finalized planning corpus.
- Owner state: read the charter DENSITY_BAR `[STATE]` column; FINALIZED owners transcribe directly, SPIKE owners transcribe their full shape now and prove the residual host probe at the gate.
- Next move: run START_GATES probes, then transcribe `Faults.cs` as the first BUILD_ORDER file.

## [2]-[START_GATES]

Bridge-proofed spikes; each gate runs before its unblocked cluster transcribes.

| [INDEX] | [GATE]                                                                                                | [PROOF]                                                                                                              | [UNBLOCKS]                                            |
| :-----: | :---------------------------------------------------------------------------------------------------- | :------------------------------------------------------------------------------------------------------------------- | :--------------------------------------------------- |
|   [1]   | ONNX Runtime native dylib load inside the RhinoWIP plugin ALC                                         | charter `bridge` gate                                                                                                | model-lane#SESSION_CAPSULE                            |
|   [2]   | `libortextensions.dylib` resolution under the portable RID graph                                      | `dotnet build libs/csharp/Rasm.Compute/Rasm.Compute.csproj -p:UseRidGraph=true` with bin-output native-asset listing | model-lane#EXTENSION_OPS                              |
|   [3]   | Kestrel `ListenUnixSocket` host inside the Rhino plugin ALC (server leg at the rhino-plugin app root) | bridge scenario at app-root creation                                                                                | remote-lane#TRANSPORT_AXIS                            |
|   [4]   | Grpc.Core.Api proof route (transitive package without an assay key)                                   | charter `spec-rail` gate until the assay source map registers the key                                                | remote-lane#CALL_POLICY, remote-lane#FAULT_PROJECTION |

## [3]-[IMPLEMENTATION_TASKS]

Ordered by the charter BUILD_ORDER; the PROOF column names charter PROOF_GATES rows.

| [INDEX] | [TASK]                           | [EXITS_AGAINST]                                                                                                                                                            | [PROOF]                                                                                |
| :-----: | :------------------------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------------------------------------------------------------------------------- |
|   [1]   | Fault rail and key policies      | intent-and-selection#DISPATCH_SPINE (`ComputeFault`), intent-and-selection#SUBSTRATE_AXIS (`ComputeKeyPolicy`)                                                             | static                                                                                 |
|   [2]   | Tensor lane                      | tensor-lane#TENSOR_VOCABULARY, tensor-lane#OPERATION_TABLE, tensor-lane#KERNEL_DISPATCH, tensor-lane#LAYOUT_ALGEBRA, tensor-lane#GEOMETRY_ENCODING, tensor-lane#EQUIVALENCE_INTEROP | static + spec (82-row partition, equivalence laws)                          |
|   [3]   | Units boundary                   | units-boundary#QUANTITY_TABLE, units-boundary#DIMENSIONAL_LAW, units-boundary#PARSE_FORMAT                                                                                 | static + spec (relation sweep, admission arities)                                      |
|   [4]   | Staging axis and stream pool     | staging-and-streams#ALLOCATION_AXIS, staging-and-streams#PLANE_VIEWS, staging-and-streams#STREAM_POOL                                                                      | static + spec (eleven-event fold `Check.Faster`)                                       |
|   [5]   | Progress family and cell         | progress-and-observation#PHASE_FAMILY, progress-and-observation#PROGRESS_CELL, progress-and-observation#OBSERVATION_SEAMS                                                  | static + spec (`SampleParallel` rank-regress law)                                      |
|   [6]   | Lane vocabulary and CPU budget   | scheduling-and-lanes#LANE_AXIS (`WorkLane`, `LaneHandle`), scheduling-and-lanes#CPU_BUDGET                                                                                 | static + spec (reader clamp arithmetic)                                                |
|   [7]   | Proto vocabulary                 | remote-lane#PROTO_VOCABULARY (service and message tables), remote-lane#FAULT_PROJECTION (`FaultDetail` row), remote-lane#ARTIFACT_FRAMES (`ArtifactFrame` row)             | restore + static (Grpc.Tools client compile)                                           |
|   [8]   | Model lane                       | model-lane#MODEL_IDENTITY, model-lane#EP_AXIS, model-lane#SESSION_CAPSULE, model-lane#EXTENSION_OPS, model-lane#INFERENCE_MODES, model-lane#RESULT_CACHE                   | static + spec + bridge (start gates 1-2)                                               |
|   [9]   | Remote contract and channels     | remote-lane#CONTRACT_EVOLUTION, remote-lane#FAULT_PROJECTION (`WireFault`), remote-lane#TRANSPORT_AXIS, remote-lane#CALL_POLICY, remote-lane#ARTIFACT_FRAMES (`FrameEdge`) | static + spec-rail (start gate 4)                                                      |
|  [10]   | Intent and selection spine       | intent-and-selection#INTENT_FAMILY, intent-and-selection#SUBSTRATE_AXIS, intent-and-selection#DISPATCH_SPINE                                                               | static + spec (selection fold, digest laws)                                            |
|  [11]   | Lane runtime and drain           | scheduling-and-lanes#LANE_AXIS (`WorkItem`), scheduling-and-lanes#SOLVE_GUARD, scheduling-and-lanes#DRAIN_CANCEL                                                           | static + spec (fence race evidence, drop-row pressure)                                 |
|  [12]   | Receipts, folds, and wire stamps | receipts-and-benchmarks#RECEIPT_UNION, receipts-and-benchmarks#FOLD_PROJECTIONS, receipts-and-benchmarks#WIRE_STAMPS                                                       | static + spec (STJ polymorphic emission)                                               |
|  [13]   | Benchmark claims                 | receipts-and-benchmarks#BENCHMARK_CLAIMS                                                                                                                                   | static + spec (fingerprint gate, band classification)                                  |
|  [14]   | TS projection handoff            | remote-lane#TS_PROJECTION, progress-and-observation#TS_PROJECTION, receipts-and-benchmarks#TS_PROJECTION                                                                   | descriptor-set emission at app-root creation; contracts transcribe at the TS workspace |

## [4]-[TESTING_APPROACH]

Universal rails share the legend in the package ROADMAP corpus (owner plus resolved member identical across the four packages); versions live in `Directory.Packages.props`.

Universal-rail concept differentiator:

| [RAIL]                  | [CONCEPT PROVEN]                                                                                                                                  |
| :---------------------- | :----------------------------------------------------------------------------------------------------------------------------------------------- |
| xUnit v3 managed law    | substrate-fold route receipt (taken-row plus vetoed-trail), tensor-gate cover-gap rejection, unit-seam canonical admission, claim-gate conjunction (equivalence ∧ speed) |
| CsCheck PBT             | kernel-row tolerance classes across the scalar-tail boundary (empty / one / sub-width / exact-multiple / multiple-plus-remainder), unit round-trip, dtype-narrowing dual-evidence, fused-triad rounding (metamorphic), session-cache races; zero-alloc local algorithm selection via `Check.Faster` (managed only — durable claims route to BenchmarkDotNet) |
| coverlet.MTP coverage   | managed reachability of fold / gate / admission surfaces; ONNX native execution classified out                                                   |
| dotnet-stryker mutation | killing oracle over substrate-veto gradient, NaN-policy columns (`Min` vs `MinNumber`), claim-margin hysteresis, closed intent fold               |
| Verify.XunitV3 snapshot | route-receipt projection (taken / vetoed trail) plus admission-receipt fingerprint as normalized evidence JSON — never floating numeric kernel output (tolerance law instead) |
| ArchUnitNET architecture | solve / lifecycle vocabulary disjointness, one-budget-record reachability, "compute never sees a wire type" boundary                              |

Package-specific rails:

| [RAIL]                              | [OWNER]                  | [CONCEPT PROVEN]                                                                                  | [RESOLVED MEMBER / TOKEN]                                                                              |
| :---------------------------------- | :----------------------- | :----------------------------------------------------------------------------------------------- | :---------------------------------------------------------------------------------------------------- |
| BenchmarkDotNet                     | `specialized-rails.md [2]` | durable-speed / allocation home for the `TensorPrimitives` kernels the managed `Check.Faster` pre-selects; zero-alloc `OrtIoBinding` posture via `MemoryDiagnoser` | `ManualConfig`, `MemoryDiagnoser`, `BenchmarkSwitcher`; rail `tests/csharp/_benchmarks`              |
| SharpFuzz                           | `specialized-rails.md [3]` | protobuf decode on `RemoteTransport` (`Parser.ParseFrom` / `CodedInputStream.CreateWithLimits`) plus tensor / encoding binary-frame admission; import-decoder grammar crash safety | `Fuzzer.OutOfProcess.Run(Action<Stream>)`; rail `tests/csharp/_fuzz`                                 |
| host/runtime scenarios (transport)  | `transport.md`           | proves the `RemoteTransport.InProcess` hand-off and UDS / loopback transport without a live remote | `TestServer`, `TestServer.CreateHandler`, `TestServer.CreateClient` (`Microsoft.AspNetCore.TestHost`) |

## [5]-[EXIT]

The package exits implementation when every BUILD_ORDER file is transcribed, every PROOF_GATES row is green, the charter GAP_LEDGER stays fully CLOSED, and the charter `spec` gate passes on the full suite.

Residual host-bridge work is the close-out surface. The SPIKE owners on the charter DENSITY_BAR carry the live probes named in their page RESEARCH clusters: `ModelSessions`, `RunOps` against model-lane#RESEARCH (`[EP_OPTIONS]`, `[CANCELLATION]`); `WireChannels`, `CallSpine` against remote-lane#RESEARCH (`[TRANSPORTS]`, `[PIPE_SECURITY]`, `[REQUEST_COMPRESSION]`, `[COMPOSED_CREDENTIAL]`). Each SPIKE owner is fully shaped at transcription; its exit is the residual native, bridge, or live-server probe passing on the gate named beside the START_GATES row that unblocks it.
