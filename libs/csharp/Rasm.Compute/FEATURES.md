# [COMPUTE_FEATURES]

Isolation-concept atlas for measured execution. Every concept rides a row on an owned axis — a new concept is a row, never a surface. Each row names the one budgeted owner and the page#cluster that legislates the mechanics; owner realization state lives on the charter DENSITY_BAR `[STATE]` column, reached through the `[OWNER]` cell.

## [1]-[ISOLATION_CONCEPTS]

The capabilities Rasm.Compute uniquely owns: a downstream app dev reaches for none of these by hand — each is a row or case on a budgeted owner.

| [INDEX] | [FEATURE]                                                       | [OWNER]             | [PAGE#CLUSTER]                          |
| :-----: | :------------------------------------------------------------- | :------------------ | :-------------------------------------- |
|   [1]   | Typed intent admission, once at the boundary                   | `IntentAdmission`   | intent-and-selection#INTENT_FAMILY      |
|   [2]   | Substrate selection as a fold over row data                    | `SubstrateSelection`| intent-and-selection#SUBSTRATE_AXIS     |
|   [3]   | Boot-frozen benchmark-rank routing with static fall-through    | `SubstrateSelection`| intent-and-selection#SUBSTRATE_AXIS     |
|   [4]   | Total dispatch with compile-broken row coverage                | `DispatchTable`     | intent-and-selection#DISPATCH_SPINE     |
|   [5]   | CPU tensor lane over the SIMD primitive surface                | `TensorOps`         | tensor-lane#OPERATION_TABLE             |
|   [6]   | Designed convolution and pooling on Rasm routes                | `TensorOpFamily`    | tensor-lane#EQUIVALENCE_INTEROP         |
|   [7]   | Layout algebra and bare plane staging views                    | `LayoutForm`        | tensor-lane#LAYOUT_ALGEBRA              |
|   [8]   | Geometry-to-tensor feature encoding with conformance triad     | `GeometryEncoding`  | tensor-lane#GEOMETRY_ENCODING           |
|   [9]   | Kernel equivalence proofs against Rasm baselines               | `EquivalenceLaw`    | tensor-lane#EQUIVALENCE_INTEROP         |
|  [10]   | ONNX inference, EP-parameterized, OrtValue-only                | `RunOps`            | model-lane#INFERENCE_MODES              |
|  [11]   | Apple-silicon ANE/GPU inference, CoreML EP                     | `ExecutionProvider` | model-lane#EP_AXIS                      |
|  [12]   | Designed GPU inference rows, CUDA and DirectML                 | `ExecutionProvider` | model-lane#EP_AXIS                      |
|  [13]   | Model provenance and extension-op admission                    | `ModelSource`       | model-lane#MODEL_IDENTITY               |
|  [14]   | One shared session per checksum with LRU residency             | `ModelSessions`     | model-lane#SESSION_CAPSULE              |
|  [15]   | EP-context warm-start blob, content-addressed                  | `ModelSessions`     | model-lane#SESSION_CAPSULE              |
|  [16]   | Bound-batch inference over OrtIoBinding                        | `RunOps`            | model-lane#INFERENCE_MODES              |
|  [17]   | ONNX terminate-latch cancellation cadence                     | `RunOps`            | model-lane#INFERENCE_MODES              |
|  [18]   | Deterministic version-stamped model-result cache              | `CacheOps`          | model-lane#RESULT_CACHE                 |
|  [19]   | Staging allocation classes with admission predicate + evidence | `AllocationClass`   | staging-and-streams#ALLOCATION_AXIS     |
|  [20]   | Bit-packed occupancy and in-place codec growth                 | `AllocationClass`   | staging-and-streams#ALLOCATION_AXIS     |
|  [21]   | One-per-process recyclable stream pool, evidence-folded        | `StreamPool`        | staging-and-streams#STREAM_POOL         |
|  [22]   | Bounded work lanes with backpressure receipts                  | `WorkLane`          | scheduling-and-lanes#LANE_AXIS          |
|  [23]   | Solve-path isolation, GH2 structural enqueue guard             | `LaneRuntime`       | scheduling-and-lanes#SOLVE_GUARD        |
|  [24]   | One processor budget across the three concurrency axes         | `WorkLane`          | scheduling-and-lanes#CPU_BUDGET         |
|  [25]   | Band-200 drain participation with provenance-preserved cancel  | `LaneRuntime`       | scheduling-and-lanes#DRAIN_CANCEL       |
|  [26]   | Monotonic progress phases with CAS rank guard                  | `ProgressCell`      | progress-and-observation#PROGRESS_CELL  |
|  [27]   | Cadence-gated observation seams, UI marshal and wire stream    | `SubscriptionPolicy`| progress-and-observation#OBSERVATION_SEAMS |
|  [28]   | Units-aware AEC admission with dual unit evidence              | `QuantityFamily`    | units-boundary#QUANTITY_TABLE           |
|  [29]   | AEC successor quantity rows                                    | `QuantityFamily`    | units-boundary#QUANTITY_TABLE           |
|  [30]   | Compound dimensional consistency at composition               | `QuantityFamily`    | units-boundary#DIMENSIONAL_LAW          |
|  [31]   | NaN-as-missing min/max reductions, distinct from Min/Max      | `TensorOpFamily`    | tensor-lane#OPERATION_TABLE             |
|  [32]   | Claim-gated parallel partition column over the SIMD lane      | `TensorOps`         | tensor-lane#KERNEL_DISPATCH             |
|  [33]   | RID-keyed BLAS provider table with managed fallback           | `LinearProvider`    | numeric-lane#DENSE_ALGEBRA              |
|  [34]   | Dense GEMM/solve over a five-case factorization union         | `DenseOps`          | numeric-lane#DENSE_ALGEBRA              |
|  [35]   | Sparse-format ingestion and direct/iterative solve            | `SparseOps`         | numeric-lane#SPARSE_SOLVE               |
|  [36]   | Kernel lowering binding for tensor matrix/structural rows     | `KernelLowering`    | numeric-lane#KERNEL_LOWERING            |
|  [37]   | Row-block shard fan-out over the dense fold                   | `ShardPlan`         | numeric-lane#KERNEL_LOWERING            |
|  [38]   | Claim-gated numeric provider rank                             | `LinearProvider`    | numeric-lane#PROVIDER_CLAIMS            |
|  [39]   | Token-streaming generative run over the session/EP spine      | `GenerativeRun`     | model-lane#GENERATIVE_RUN               |
|  [40]   | Search-option and native chat-template prompt assembly        | `GenerationPolicy`  | model-lane#GENERATIVE_RUN               |
|  [41]   | Grammar-constrained structured output at generation           | `GuidanceKind`      | model-lane#GENERATIVE_RUN               |
|  [42]   | Generative and numeric-solve intent cases                     | `ComputeIntent`     | intent-and-selection#INTENT_FAMILY      |
|  [43]   | glTF/GLB managed mesh-and-scene import and export             | `InterchangeIo`     | interchange#IMPORT_RAIL                 |
|  [44]   | In-process IFC semantic-graph ingest, never tessellated BRep  | `InterchangeIo`     | interchange#IMPORT_RAIL                 |
|  [45]   | IFC STEP/XML/JSON managed model serialization                | `InterchangeIo`     | interchange#EXPORT_RAIL                 |
|  [46]   | IFC-to-geometry two-hop tessellation over the companion rpc   | `TessellationRequest`| interchange#TWO_HOP_TESSELLATION       |
|  [47]   | Content-addressed interchange artifacts, deflection-folded    | `InterchangeIdentity`| interchange#CONTENT_ADDRESSING         |

## [2]-[WIRE_AND_EVIDENCE_CONCEPTS]

| [INDEX] | [FEATURE]                                                      | [OWNER]            | [PAGE#CLUSTER]                          |
| :-----: | :------------------------------------------------------------ | :----------------- | :-------------------------------------- |
|   [1]   | Suite wire vocabulary, five proto services                   | `WireServices`     | remote-lane#PROTO_VOCABULARY            |
|   [2]   | Canonical proto geometry, one binary wire shape              | `WireServices`     | remote-lane#PROTO_VOCABULARY            |
|   [3]   | Contract evolution by descriptor diff                         | `ContractGuard`    | remote-lane#CONTRACT_EVOLUTION          |
|   [4]   | Open-envelope option negotiation under additive-only contract | `ContractGuard`    | remote-lane#CONTRACT_EVOLUTION          |
|   [5]   | Typed faults across the wire through FaultDetail              | `ContractGuard`    | remote-lane#FAULT_PROJECTION            |
|   [6]   | Four-row transport axis with streaming-capability columns     | `RemoteTransport`  | remote-lane#TRANSPORT_AXIS              |
|   [7]   | In-process transport for the deterministic test host         | `RemoteTransport`  | remote-lane#TRANSPORT_AXIS              |
|   [8]   | UnixDomainSocket byte-path transport with peer-credential law | `RemoteTransport`  | remote-lane#TRANSPORT_AXIS              |
|   [9]   | Warm-start node affinity as a selection tie-breaker           | `WireChannels`     | remote-lane#TRANSPORT_AXIS              |
|  [10]   | Credential axis behind one stamping interceptor              | `CredentialPolicy` | remote-lane#CALL_POLICY                 |
|  [11]   | Compression flip behind a winning benchmark claim            | `CallSpine`        | remote-lane#CALL_POLICY                 |
|  [12]   | Artifact frame law: 64 KiB, Crc32, zero-copy wrap            | `FrameEdge`        | remote-lane#ARTIFACT_FRAMES             |
|  [13]   | Zero-alloc IBufferMessage frame steady state                 | `FrameEdge`        | remote-lane#ARTIFACT_FRAMES             |
|  [14]   | Progress observation and cancellation spine                  | `ProgressCell`     | progress-and-observation#PROGRESS_CELL  |
|  [15]   | Fifteen-case receipt union with fold projections             | `ComputeReceipt`   | receipts-and-benchmarks#RECEIPT_UNION   |
|  [16]   | Unified telemetry contribution through AppHost ports         | `ReceiptSurface`   | receipts-and-benchmarks#RECEIPT_UNION   |
|  [17]   | NodaTime-protobuf wire stamps at the temporal edge           | `ReceiptSurface`   | receipts-and-benchmarks#WIRE_STAMPS     |
|  [18]   | Benchmark and profiling evidence farm, fingerprint-gated     | `BenchmarkClaim`   | receipts-and-benchmarks#BENCHMARK_CLAIMS |
|  [19]   | TS dashboard projections, wire, progress, receipts           | `ComputeReceipt`   | receipts-and-benchmarks#TS_PROJECTION   |
|  [20]   | Numeric solve and remote-generate rpc legs                   | `WireServices`     | remote-lane#PROTO_VOCABULARY            |
|  [21]   | Graph-diff and subtree-fetch wire message family             | `WireServices`     | remote-lane#PROTO_VOCABULARY            |
|  [22]   | Factorization and generate receipt cases                    | `ComputeReceipt`   | receipts-and-benchmarks#RECEIPT_UNION   |

## [3]-[CONCEPT_SEEDS]

Higher-order isolation concepts this folder uniquely enables, each riding an existing owner — no new surface.

- Farm-affinity routing as a `Substrate`-fold column: warm-start affinity reorders only the rank-equal tier, so host-vs-companion-vs-farm is the same fold that picks cpu-vs-onnx — no `FarmRouter` (intent-and-selection#SUBSTRATE_AXIS · remote-lane#TRANSPORT_AXIS).
- Content-addressed acquisition unification: warm-start blob, result cache, and model acquisition collapse onto one checksum identity so a cold companion warms from the blob the host wrote (model-lane#SESSION_CAPSULE · model-lane#RESULT_CACHE · model-lane#MODEL_IDENTITY).
- `ToleranceClass` as a proof-by-data column on every `TensorOpFamily` row: a designed-only kernel inherits proof coverage the moment its route lands, with no `Prove` argument (tensor-lane#OPERATION_TABLE · tensor-lane#EQUIVALENCE_INTEROP).
- Zero-alloc frame spine: `IBufferMessage` + `CreateWithLimits` + `FieldCodec<T>` make the steady-state frame path copy-free, the send-side `CalculateSize` pre-check symmetric with the receive-side bounded reader (remote-lane#ARTIFACT_FRAMES · staging-and-streams#STREAM_POOL).
- One processor budget as the suite's anti-oversubscription invariant: lane readers, ORT global pool, partition cap, and spin posture all derive from two inputs, so one budget edit re-caps every concurrency axis coherently (scheduling-and-lanes#CPU_BUDGET).
- Claim-gated performance as a closed conjunction: equivalence proof AND fingerprint-matched speed claim — a fast wrong kernel is poisoned regardless of ratio, and an empty claims table is the correct cold start (receipts-and-benchmarks#BENCHMARK_CLAIMS · tensor-lane#EQUIVALENCE_INTEROP).
- Every measured hop self-proves on one receipt rail: a farm hop, a tensor run, and a unit projection all materialize `ComputeReceipt` cases at the sink edge, so cross-process provenance joins by correlation with no per-view store (receipts-and-benchmarks#RECEIPT_UNION · receipts-and-benchmarks#FOLD_PROJECTIONS).
- Generative run modes collapse onto one streaming owner: chunked decode, embedding extraction, and the tool-emit→dispatch→re-feed loop are run-mode cases on the same `GenerativeRun` capsule rather than parallel chat surfaces, so a batch of prompts and a single grammar-constrained stream share the session/EP spine (model-lane#GENERATIVE_RUN · model-lane#INFERENCE_MODES).
- Load-aware substrate placement as a fold column: the `Substrate.GenAi` row reorders only behind a live placement probe, so generative load offloads to the companion or farm through the same selection fold that picks cpu-vs-onnx, never a generative-specific router (intent-and-selection#SUBSTRATE_AXIS · model-lane#GENERATIVE_RUN).
- One content-keyed IFC artifact, two projections: the in-process semantic graph and the two-hop tessellated GLB join by the same `XxHash128` content-key the suite hash law owns, so a coarse and a fine tessellation key distinctly and a re-import keys identically — deflection and tolerance partition the key, never a cross-setting hit (interchange#TWO_HOP_TESSELLATION · interchange#CONTENT_ADDRESSING).
- Diff-set proto wire as a Compute frame, Persistence algebra: `GraphDiff`/`SubtreeFetch` carry the content-key delta wire shape only, so the diff computation stays Persistence sync-collaboration mechanics and Compute owns the frame, never re-deriving the diff (remote-lane#PROTO_VOCABULARY).

## [4]-[ROUTING]

- Concept mechanics: `Rasm.Compute/.planning/<page>.md` at the cluster anchor named on each concept.
- Owner budget and realization state: `.planning/README.md` DENSITY_BAR via the `[OWNER]` cell; prohibitions: `.planning/README.md` PROHIBITIONS; gap ledger and admissions: `.planning/README.md` GAP_LEDGER / ADMISSIONS_RECORD.
- External-API grounding: `.api/*.md` per the ADMISSIONS_RECORD CATALOGUE column; RESEARCH rows carry the page-local research-cluster name.
- Consumed and provided seams: `ARCHITECTURE.md` CONSUMED_SEAMS / PROVIDED_SEAMS; cross-package topologies: `libs/csharp/.planning/FEATURES.md`.
