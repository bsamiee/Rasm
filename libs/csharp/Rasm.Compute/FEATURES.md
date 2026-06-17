# [COMPUTE_FEATURES]

The realized capability list for measured execution. Every feature is a row or case on a budgeted owner, never a new surface; mechanics live at the `.planning/` page#cluster anchor named on each row, and the owner's realization state is read from `ARCHITECTURE.md` `[OWNER_REGISTRY]`.

## [1]-[ISOLATION_CONCEPTS]

The capabilities Rasm.Compute uniquely owns: a downstream app dev reaches for none of these by hand — each is a row or case on a budgeted owner.

| [INDEX] | [FEATURE]                                                                | [PAGE#CLUSTER]                              |
| :-----: | :----------------------------------------------------------------------- | :------------------------------------------ |
|   [1]   | Typed intent admission, once at the boundary                             | intent-and-selection#INTENT_FAMILY          |
|   [2]   | Substrate selection as a fold over row data                              | intent-and-selection#SUBSTRATE_AXIS         |
|   [3]   | Boot-frozen benchmark-rank routing with static fall-through              | intent-and-selection#SUBSTRATE_AXIS         |
|   [4]   | Total dispatch with compile-broken row coverage                          | intent-and-selection#DISPATCH_SPINE         |
|   [5]   | CPU tensor lane over the SIMD primitive surface                          | tensor-lane#OPERATION_TABLE                 |
|   [6]   | Designed convolution and pooling on Rasm routes                          | tensor-lane#EQUIVALENCE_INTEROP             |
|   [7]   | Layout algebra and bare plane staging views                              | tensor-lane#LAYOUT_ALGEBRA                  |
|   [8]   | Geometry-to-tensor feature encoding with conformance triad               | tensor-lane#GEOMETRY_ENCODING               |
|   [9]   | Kernel equivalence proofs against Rasm baselines                         | tensor-lane#EQUIVALENCE_INTEROP             |
|  [10]   | ONNX inference, EP-parameterized, OrtValue-only                          | model-lane#INFERENCE_MODES                  |
|  [11]   | Apple-silicon ANE/GPU inference, CoreML EP                               | model-lane#EP_AXIS                          |
|  [12]   | Designed GPU inference rows, CUDA and DirectML                           | model-lane#EP_AXIS                          |
|  [13]   | Model provenance and extension-op admission                              | model-lane#MODEL_IDENTITY                   |
|  [14]   | One shared session per checksum with LRU residency                       | model-lane#SESSION_CAPSULE                  |
|  [15]   | EP-context warm-start blob, content-addressed                            | model-lane#SESSION_CAPSULE                  |
|  [16]   | Bound-batch inference over OrtIoBinding                                  | model-lane#INFERENCE_MODES                  |
|  [17]   | ONNX terminate-latch cancellation cadence                                | model-lane#INFERENCE_MODES                  |
|  [18]   | Deterministic version-stamped model-result cache                         | model-lane#RESULT_CACHE                     |
|  [19]   | Staging allocation classes with admission predicate + evidence           | staging-and-streams#ALLOCATION_AXIS         |
|  [20]   | Bit-packed occupancy and in-place codec growth                           | staging-and-streams#ALLOCATION_AXIS         |
|  [21]   | One-per-process recyclable stream pool, evidence-folded                  | staging-and-streams#STREAM_POOL             |
|  [22]   | Bounded work lanes with backpressure receipts                            | scheduling-and-lanes#LANE_AXIS              |
|  [23]   | Solve-path isolation, GH2 structural enqueue guard                       | scheduling-and-lanes#SOLVE_GUARD            |
|  [24]   | One processor budget across the three concurrency axes                   | scheduling-and-lanes#CPU_BUDGET             |
|  [25]   | Band-200 drain participation with provenance-preserved cancel            | scheduling-and-lanes#DRAIN_CANCEL           |
|  [26]   | Monotonic progress phases with CAS rank guard                            | progress-and-observation#PHASE_FAMILY       |
|  [27]   | Cadence-gated observation seams, UI marshal and wire stream              | progress-and-observation#OBSERVATION_SEAMS  |
|  [28]   | Units-aware AEC admission with dual unit evidence                        | units-boundary#QUANTITY_TABLE               |
|  [29]   | AEC successor quantity rows                                              | units-boundary#QUANTITY_TABLE               |
|  [30]   | Compound dimensional consistency at composition                          | units-boundary#DIMENSIONAL_LAW              |
|  [31]   | NaN-as-missing min/max reductions, distinct from Min/Max                 | tensor-lane#OPERATION_TABLE                 |
|  [32]   | Claim-gated parallel partition column over the SIMD lane                 | tensor-lane#KERNEL_DISPATCH                 |
|  [33]   | RID-keyed BLAS provider table with managed fallback                      | numeric-lane#DENSE_ALGEBRA                  |
|  [34]   | Dense GEMM/solve over a five-case factorization union                    | numeric-lane#DENSE_ALGEBRA                  |
|  [35]   | Sparse-format ingestion and direct/iterative solve                       | numeric-lane#SPARSE_SOLVE                   |
|  [36]   | Kernel lowering binding for tensor matrix/structural rows                | numeric-lane#KERNEL_LOWERING                |
|  [37]   | Row-block shard fan-out over the dense fold                              | numeric-lane#KERNEL_LOWERING                |
|  [38]   | Claim-gated numeric provider rank                                        | numeric-lane#PROVIDER_CLAIMS                |
|  [39]   | Token-streaming generative run over the session/EP spine                 | model-lane#GENERATIVE_RUN                   |
|  [40]   | Search-option and native chat-template prompt assembly                   | model-lane#GENERATIVE_RUN                   |
|  [41]   | Grammar-constrained structured output at generation                      | model-lane#GENERATIVE_RUN                   |
|  [42]   | Generative and numeric-solve intent cases                                | intent-and-selection#INTENT_FAMILY          |
|  [43]   | IFC-to-geometry two-hop tessellation over the companion rpc              | interchange#TWO_HOP_TESSELLATION            |
|  [44]   | Content-addressed interchange artifacts, deflection-folded               | interchange#CONTENT_ADDRESSING              |
|  [45]   | 3D-Tiles streamable-LOD octree partition over the content-keyed geometry | interchange#TILE_PARTITION                  |
|  [46]   | Chunked error-bounded field/result codec, zero-copy                      | interchange#FIELD_RESULT_CODEC              |
|  [47]   | FastCDC structural geometry-delta codec, progressive                     | interchange#GEOMETRY_DELTA                  |
|  [48]   | Provider-determinism solve-dedup fingerprint                             | numeric-lane#DENSE_ALGEBRA                  |
|  [49]   | Reverse-mode adjoint through differentiable DDG operators                | tensor-lane#EQUIVALENCE_INTEROP             |
|  [50]   | B-rep/NURBS canonical geometry encoding                                  | tensor-lane#GEOMETRY_ENCODING               |
|  [51]   | BIM inference reuse: classification, symbol, clash scoring               | model-lane#INFERENCE_MODES                  |
|  [52]   | Dependency job-graph scheduler, dirty-subgraph re-solve                  | scheduling-and-lanes#JOB_GRAPH              |
|  [53]   | Volumetric mesher: tet/hex/poly, boundary-layer, h/p adaptive            | solver-and-optimization#DISCRETIZATION_MESH |
|  [54]   | Integration-point/nodal scalar/vector/tensor field rep                   | solver-and-optimization#DISCRETIZATION_MESH |
|  [55]   | Uniform physics×BC×element solve contract, typed receipts                | solver-and-optimization#SOLVE_CONTRACT      |
|  [56]   | Design-space optimizer: NSGA/Bayesian/adjoint/topology/SA                | solver-and-optimization#OPTIMIZER_LANE      |
|  [57]   | Surrogate/reduced-order duality behind the solve contract                | solver-and-optimization#OPTIMIZER_LANE      |
|  [58]   | Pareto front as a queryable artifact                                     | solver-and-optimization#OPTIMIZER_LANE      |
|  [59]   | N-dim DOE sweep grid with sensitivity tornado                            | solver-and-optimization#SWEEP_AND_BUDGET    |
|  [60]   | Frame-budgeted progressive solve, coarse-then-refine                     | solver-and-optimization#SWEEP_AND_BUDGET    |
|  [61]   | Acceleration-structure clash + clearance compute over federated geometry | solver-and-optimization#CLASH_AND_TWIN      |
|  [62]   | ROM digital-twin loop: anomaly scoring, control suggestion               | solver-and-optimization#CLASH_AND_TWIN      |

## [2]-[WIRE_AND_EVIDENCE_CONCEPTS]

The suite wire vocabulary, the contract guard, the transport axis, and the receipt and benchmark evidence rails — the only cross-package wire seam plus the receipt and TS map.

| [INDEX] | [FEATURE]                                                     | [PAGE#CLUSTER]                           |
| :-----: | :------------------------------------------------------------ | :--------------------------------------- |
|   [1]   | Suite wire vocabulary, five proto services                    | remote-lane#PROTO_VOCABULARY             |
|   [2]   | Canonical proto geometry, one binary wire shape               | remote-lane#PROTO_VOCABULARY             |
|   [3]   | Contract evolution by descriptor diff                         | remote-lane#CONTRACT_EVOLUTION           |
|   [4]   | Open-envelope option negotiation under additive-only contract | remote-lane#CONTRACT_EVOLUTION           |
|   [5]   | Typed faults across the wire through FaultDetail              | remote-lane#FAULT_PROJECTION             |
|   [6]   | Four-row transport axis with streaming-capability columns     | remote-lane#TRANSPORT_AXIS               |
|   [7]   | In-process transport for the deterministic test host          | remote-lane#TRANSPORT_AXIS               |
|   [8]   | UnixDomainSocket byte-path transport with peer-credential law | remote-lane#TRANSPORT_AXIS               |
|   [9]   | Warm-start node affinity as a selection tie-breaker           | remote-lane#TRANSPORT_AXIS               |
|  [10]   | Credential axis behind one stamping interceptor               | remote-lane#CALL_POLICY                  |
|  [11]   | Compression flip behind a winning benchmark claim             | remote-lane#CALL_POLICY                  |
|  [12]   | Artifact frame law: 64 KiB, Crc32, zero-copy wrap             | remote-lane#ARTIFACT_FRAMES              |
|  [13]   | Zero-alloc IBufferMessage frame steady state                  | remote-lane#ARTIFACT_FRAMES              |
|  [14]   | Progress observation and cancellation spine                   | progress-and-observation#PHASE_FAMILY    |
|  [15]   | Twenty-one-case receipt union with fold projections           | receipts-and-benchmarks#RECEIPT_UNION    |
|  [16]   | Unified telemetry contribution through AppHost ports          | receipts-and-benchmarks#RECEIPT_UNION    |
|  [17]   | NodaTime-protobuf wire stamps at the temporal edge            | receipts-and-benchmarks#WIRE_STAMPS      |
|  [18]   | Benchmark and profiling evidence farm, fingerprint-gated      | receipts-and-benchmarks#BENCHMARK_CLAIMS |
|  [19]   | TS dashboard projections, wire, progress, receipts            | receipts-and-benchmarks#TS_PROJECTION    |
|  [20]   | Numeric solve and remote-generate rpc legs                    | remote-lane#PROTO_VOCABULARY             |
|  [21]   | Graph-diff and subtree-fetch wire message family              | remote-lane#PROTO_VOCABULARY             |
|  [22]   | Factorization and generate receipt cases                      | receipts-and-benchmarks#RECEIPT_UNION    |

## [3]-[CONCEPT_SEEDS]

Higher-order isolation concepts this folder uniquely enables, each riding an existing owner — no new surface.

- Farm-affinity routing as a `Substrate`-fold column: warm-start affinity reorders only the rank-equal tier, so host-vs-companion-vs-farm is the same fold that picks cpu-vs-onnx — no `FarmRouter` (intent-and-selection#SUBSTRATE_AXIS · remote-lane#TRANSPORT_AXIS).
- Content-addressed acquisition unification: warm-start blob, result cache, and model acquisition collapse onto one checksum identity so a cold companion warms from the blob the host wrote (model-lane#SESSION_CAPSULE · model-lane#RESULT_CACHE · model-lane#MODEL_IDENTITY).
- `ToleranceClass` as a proof-by-data column on every `TensorOpFamily` row: a designed-only kernel inherits proof coverage the moment its route lands, with no `Prove` argument (tensor-lane#OPERATION_TABLE · tensor-lane#EQUIVALENCE_INTEROP).
- Zero-alloc frame spine: `IBufferMessage` + `CreateWithLimits` + `FieldCodec<T>` make the steady-state frame path copy-free, the send-side `CalculateSize` pre-check symmetric with the receive-side bounded reader (remote-lane#ARTIFACT_FRAMES · staging-and-streams#STREAM_POOL).
- One processor budget as the suite's anti-oversubscription invariant: lane readers, ORT global pool, partition cap, and spin posture all derive from two inputs, so one budget edit re-caps every concurrency axis coherently (scheduling-and-lanes#CPU_BUDGET).
- Claim-gated performance as a closed conjunction: equivalence proof AND fingerprint-matched speed claim — a fast wrong kernel is poisoned regardless of ratio, and an empty claims table is the correct cold start (receipts-and-benchmarks#BENCHMARK_CLAIMS · tensor-lane#EQUIVALENCE_INTEROP).
- Every measured hop self-proves on one receipt rail: a farm hop, a tensor run, and a unit projection all materialize `ComputeReceipt` cases at the sink edge, so cross-process provenance joins by correlation with no per-view store (receipts-and-benchmarks#RECEIPT_UNION).
- Generative run modes collapse onto one streaming owner: chunked decode, embedding extraction, and the tool-emit→dispatch→re-feed loop are run-mode cases on the same `GenerativeRun` capsule rather than parallel chat surfaces, so a batch of prompts and a single grammar-constrained stream share the session/EP spine (model-lane#GENERATIVE_RUN · model-lane#INFERENCE_MODES).
- Load-aware substrate placement as a fold column: the `Substrate.GenAi` row reorders only behind a live placement probe, so generative load offloads to the companion or farm through the same selection fold that picks cpu-vs-onnx, never a generative-specific router (intent-and-selection#SUBSTRATE_AXIS · model-lane#GENERATIVE_RUN).
- One content-keyed IFC artifact, two projections: the `Rasm.Bim` IFC semantic graph and the two-hop tessellated GLB join by the same `XxHash128` content-key the suite hash law owns, so a coarse and a fine tessellation key distinctly and a re-import keys identically — deflection and tolerance partition the key, never a cross-setting hit (interchange#TWO_HOP_TESSELLATION · interchange#CONTENT_ADDRESSING).
- Diff-set proto wire as a Compute frame, Persistence algebra: `GraphDiff`/`SubtreeFetch` carry the content-key delta wire shape only, so the diff computation stays Persistence sync-collaboration mechanics and Compute owns the frame, never re-deriving the diff (remote-lane#PROTO_VOCABULARY).
