# [RASM_COMPUTE_ARCHITECTURE]

`Rasm.Compute` maps APP-PLATFORM measured execution over `{Rasm, Rasm.Element}`: one intent rail admits work once at the boundary, one substrate axis routes it over row data, bounded lanes carry it, and one `ComputeReceipt` union records every outcome across the Tensor, Symbolic, Model, Solver, Stats, Runtime, and Analysis folders. Each folder maps to exactly one namespace, and one polymorphic owner closes its axis over the `ComputeReceipt`/`ComputeFault` pair.

## [01]-[DOMAIN_MAP]

```text codemap
Rasm.Compute/
├── Tensor/                # CPU tensor vocabulary and BLAS-class numeric core
│   ├── Vocabulary.cs      # Tensor shape, factory, dtype, and op-family vocabulary
│   ├── Layout.cs          # Layout forms and the shape-edit request union
│   ├── Dispatch.cs        # Arity kernel-delegate tables, the differentiable-adjoint law, and WGSL device-kernel dispatch
│   ├── Residency.cs       # OrtValue C-data residency lattice and geometry-to-tensor encoding
│   ├── Memory.cs          # Bounded staging memory and the zero-copy stream pool
│   ├── Blas.cs            # Dense BLAS, factorization, spectral core, and damped nonlinear least squares
│   ├── Factor.cs          # Sparse ingestion and criterion-stack iterative solve
│   ├── Quadrature.cs      # Integration lane over the kernel quadrature/RK floor, trajectory driver, spectral operator
│   └── Sampling.cs        # Sobol/Halton sampling and radial-basis scatter reconstruction
├── Symbolic/              # Closed symbolic-expression CAS and unit boundary
│   ├── Expression.cs      # Symbolic-expression algebra over the CAS Entity
│   ├── Dimensional.cs     # ℚ⁷ SI base-dimension proof
│   ├── Lowering.cs        # Compiled-expression cache, analytic-Jacobian arm, interval enclosure, and column programs
│   └── Units.cs           # Units boundary admitting unit-bearing input
├── Model/                 # ONNX model identity, sessions, inference, and generative runs
│   ├── Identity.cs        # Checksum identity, acquisition union, schema snapshot, and drift sentinel
│   ├── Sessions.cs        # One shared session per checksum with warm-start and its per-bucket warm roster
│   ├── Providers.cs       # Execution-provider axis with discovery, quantization posture, and the floor ladder
│   ├── Inference.cs       # Run-mode inference fold, batching gate, tiled mosaic, stage-execution wire, result cache
│   ├── Embedding.cs       # Embedding-and-retrieval owner
│   ├── Generative.cs      # Token-streaming generation with the tool-call arm
│   └── Extension.cs       # Custom-op registration at the string-tensor boundary
├── Solver/                # Discretize-solve-optimize-sweep solve spine
│   ├── Discretization.cs  # Volumetric meshing with adaptive refinement and exact-predicate gates
│   ├── Contract.cs        # Physics-by-BC solve fold with adaptive recovery and the coupled-solve lane
│   ├── Constitutive.cs    # Per-Gauss-point stress-update axis and contact enforcement
│   ├── Optimizer.cs       # Design-space search axis with surrogate duality
│   ├── Sweep.cs           # N-dim DOE sweep grid and sensitivity analysis
│   ├── Clash.cs           # Collision compute, occlusion rays, and the digital-twin loop
│   ├── Satisfy.cs         # SMT rule satisfaction with witness and unsat-core explanation
│   └── Uncertainty.cs     # Forward-UQ and reliability over the shared evaluate oracle
├── Stats/                 # Classical statistics, statistical learning, and DSP
│   ├── Estimator.cs       # One Fit/Predict estimator axis across the statistical families
│   ├── Signal.cs          # Spectral-transform axis and filter design
│   └── Monitor.cs         # Streaming monitor capsules, receipt-channel extraction, and the drift verdict
├── Runtime/               # Admit-to-receipt boundary plane
│   ├── Admission.cs       # Typed intent admission with substrate axis and total dispatch
│   ├── Scheduling.cs      # Bounded work-lanes and the dependency job-graph scheduler
│   ├── Progress.cs        # Monotonic phase family and the progress capsule
│   ├── Receipts.cs        # ComputeReceipt fact union — instrument projection, replay folds, hook rail, tenant cost ledger
│   ├── Wire.cs            # Wire contract: proto vocabulary, evolution, and fault projection
│   ├── Transport.cs       # Channel mechanics: transport rows, tuning, and the artifact frame law
│   ├── Codecs.cs          # Field, result, and geometry-delta codecs and the tessellation bridge
│   └── Payload.cs         # Residency payload codec and the cluster-LOD chain
└── Analysis/              # C#-first discipline-assessment rail over the ElementGraph
    ├── Assessment.cs      # Lifecycle-aware assessment spine and reconciler
    ├── Aggregator.cs      # Multi-ply assembly aggregator over U/STC/GWP/cost
    ├── Structural.cs      # Frame solve and the design-code capacity table
    ├── Physics.cs         # Closed-form thermal, acoustic, and fire folds
    ├── Energy.cs          # Energy-route axis over the simulation toolchain
    ├── Lifecycle.cs       # Embodied-carbon and cost rollup over the EPD boundary
    ├── Circulation.cs     # Egress and life-safety runner
    └── Daylight.cs        # Perez sky-model daylight rows over the kernel solar almanac
```

Implementation collapses to one owner per axis and one entrypoint family per rail: a new feature is a row or case on a budgeted owner, and a public type outside an owner region is the named defect. Rail is named in the return type — `Fin<T>` aborts at admission, `Validation<Error,T>` accumulates (the monoidal `Error` carrier; typed `ComputeFault` arms lift onto it through their `Expected` base, since `ComputeFault` is not itself a monoid), `IO<T>` carries effects, `Option<T>` carries absence.

`ComputeFault` projects through `FaultDetail` at the wire edge; receipts stamp NodaTime `Instant`/`Duration`, and AppHost `ClockPolicy` owns both clocks.

## [02]-[STRATA]

Five strata order the seven sub-domains; `Runtime` seats lowest as the vocabulary mint while its dispatch table routes to the lane owners and its `ComputeReceipt` union gains cases as partials declared by the owning stratum — co-ownership, never an upward import — so every consumption edge points down.

- S0 `Runtime` — mints the admit-to-receipt substrate once: `ComputeIntent`, `ComputeReceipt`, `ComputeFault`, the `Substrate` axis.
- S0 `Runtime` — `LaneProfiles` keys on the spine `WorkLane` roster.
- S1 `Tensor` — `TensorOps`, `OrtResidency`, and the `LowDiscrepancy` sampler, peers over the substrate.
- S1 `Symbolic` — `QuantityFamily`, `DimensionMonomial`, and the `CompiledExpr` cache.
- S2 `Model` — `ModelIdentity`, `ModelSessions`, and the `GraduationEnvelope` admission gate.
- S2 `Stats` — the `EstimatorKind` fit axis, the spectral rail, and the `StreamMonitor` capsule family.
- S3 `Solver` — the discretize-solve-optimize-sweep spine over tensors, symbols, surrogates, and estimators.
- S3 `Solver` — `MeshKernel`, `OptimizerKind`, `SweepLane`, the `ClashScale` collision fold, and the `DoeDataset` wire shape.
- S4 `Analysis` — the discipline-assessment rail nothing composes: `AssessmentRoute`, `AssemblyAggregator`, `DaylightAnalysis`.
- S4 reach — `Analysis` reads the `ElementGraph` and writes content-keyed deltas.

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart TB
    accTitle: Rasm.Compute interior strata
    accDescr: Five stacked strata from the analysis rail through the solve spine, the model and stats stratum, and the tensor-symbolic peers onto the runtime substrate, every consumption edge downward and solid naming one sourced type, and one labeled forbidden upward edge.
    subgraph S4["S4 ANALYSIS"]
        Assessment[AssessmentRoute]
        Daylight[DaylightAnalysis]
    end
    subgraph S3["S3 SOLVER"]
        Sweep[SweepLane]
        Mesh[MeshKernel]
        Optimizer[OptimizerKind]
        Clash[ClashScale]
    end
    subgraph S2["S2 MODEL + STATS"]
        Envelope[GraduationEnvelope]
        Identity[ModelIdentity]
        Estimator[EstimatorKind]
    end
    subgraph S1["S1 TENSOR + SYMBOLIC"]
        Sampling[LowDiscrepancy]
        Ops[TensorOps]
        Compiled[CompiledExpr]
    end
    subgraph S0["S0 RUNTIME"]
        Receipt[ComputeReceipt]
        Lane[LaneProfiles]
    end
    Assessment e1@-->|"[IMPORT]: ComputeReceipt"| Receipt
    Daylight e2@-->|"[IMPORT]: ClashScale"| Clash
    Sweep e3@-->|"[IMPORT]: GraduationEnvelope"| Envelope
    Optimizer e4@-->|"[IMPORT]: CompiledExpr"| Compiled
    Sweep e5@-->|"[IMPORT]: LowDiscrepancy"| Sampling
    Estimator e6@-->|"[IMPORT]: TensorOps"| Ops
    Identity e7@-->|"[IMPORT]: ComputeReceipt"| Receipt
    Mesh e8@--> Ops
    Sweep e9@-->|"[IMPORT]: LaneProfiles"| Lane
    Receipt f1@-->|"forbidden: substrate upward"| S4
```

## [03]-[SEAMS]

`e32` `[WIRE]: StageResult` carries TWO measured columns beside the frozen preimage — `ParityFresh`, true only on the arm that leased a floor session and ran both probes, and `Coverage`, the overlap-add weight floor. Without the freshness discriminant a residual series counts single-measurement requests; without coverage a mosaic reassembled at 0.001 publishes as healthy. `Rasm.Materials` owns the wire record, so `StageResult`, its `StageResultWire` key roster, and the `InferGolden` tap all take the two columns at the Materials end, the tap gating on freshness.

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: Compute AEC-domain and storage seams
    accDescr: Compute sub-domain owners exchanging content keys, neutral shapes, wires, projections, transport, and tessellation with the kernel, the Element/Materials/Bim/Fabrication AEC peers, and the persistence store, one edge per contract family labeled by kind and shared shape.
    subgraph compute[RASM.COMPUTE]
        Model[Model runtime]
        Tensor[Tensor core]
        Solver[Solve spine]
        Symbolic[Symbolic CAS]
        Analysis[Analysis rail]
        Runtime[Runtime plane]
    end
    Rasm{{Rasm}}
    Persistence[(Rasm.Persistence)]
    Element{{Rasm.Element}}
    Materials[Rasm.Materials]
    Fabrication[Rasm.Fabrication]
    Bim[Rasm.Bim]
    Rasm e1@-->|"[CONTENT_KEY]: ContentHash"| Model
    Rasm e22@-->|"[PORT]: ReceiptSinkPort + InstrumentSpec + SpanBand + Slo"| Runtime
    Rasm e2@-->|"[SHAPE]: Predicate"| Solver
    Tensor e3@<-->|"[SHAPE]: DiscreteCalculus"| Rasm
    Rasm e4@-->|"[WIRE]: SliceStack"| Analysis
    Rasm e18@-->|"[SHAPE]: MeshAdjointSnapshot"| Tensor
    Rasm e19@-->|"[WIRE]: SpatialIndex"| Solver
    Rasm e21@-->|"[WIRE]: EncodedGeometry"| Tensor
    Rasm e34@-->|"[SHAPE]: FieldIntegrator + IntegrationDomain"| Tensor
    Rasm e36@-->|"[SHAPE]: SunPosition"| Analysis
    Model e15@<-->|"[CONTENT_KEY]: ArtifactIndexRow"| Persistence
    Model e35@-->|"[CONTENT_KEY]: ParityVerdict"| Persistence
    Model e27@<-->|"[CONTENT_KEY]: VectorCodebook"| Persistence
    Tensor e16@-->|"[CONTENT_KEY]: ShardPlan"| Persistence
    Symbolic e14@-->|"[CONTENT_KEY]: CompiledExpr"| Persistence
    Analysis e13@-->|"[CONTENT_KEY]: AssessmentPayload"| Persistence
    Runtime e17@<-->|"[CONTENT_KEY]: InterchangeIdentity"| Persistence
    Runtime e28@<-->|"[CONTENT_KEY]: GeometryHash"| Persistence
    Runtime e29@-->|"[WIRE]: LakeGeneration"| Persistence
    Rasm e30@-->|"[WIRE]: EncodedGeometry"| Runtime
    Solver e8@<-->|"[SHAPE]: MaterialPropertySet"| Element
    Element e7@-->|"[SHAPE]: Dimension"| Symbolic
    Element e5@-->|"[SHAPE]: ElementGraph"| Analysis
    Element e40@-->|"[SHAPE]: ElementGraph"| Solver
    Element e37@-->|"[SHAPE]: AssessmentPayload"| Analysis
    Element e41@-->|"[SHAPE]: ObservationSeries"| Analysis
    Runtime e42@-->|"[PROJECTION]: GraphDelta"| Element
    Runtime e6@<-->|"[CONTENT_KEY]: RepresentationContentHash"| Element
    Element e33@-->|"[SHAPE]: ImportedGeometry"| Runtime
    Element e10@-->|"[SHAPE]: AssemblyAggregator"| Analysis
    Materials e9@-->|"[WIRE]: MaterialPropertySet"| Analysis
    Materials e23@-->|"[WIRE]: SectionCapacity"| Analysis
    Materials e31@-->|"[WIRE]: StageRequest"| Model
    Model e32@-->|"[WIRE]: StageResult"| Materials
    Fabrication e12@-->|"[PROJECTION]: NestYield"| Analysis
    Bim e11@<-->|"[TESSELLATION]: TessellationOutcome"| Runtime
    Bim e24@<-->|"[TRANSPORT]: IdsVerdict"| Runtime
    Bim e25@-->|"[CONTENT_KEY]: RepresentationContentHash"| Runtime
    Bim e26@-->|"[CONTENT_KEY]: CostSchedule"| Analysis
    Bim e38@-->|"[CONTENT_KEY]: EnergyArtifact"| Analysis
    Analysis e39@-->|"[WIRE]: EnergyResult"| Bim
```

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: Compute platform and cross-runtime seams
    accDescr: Compute owners exchanging port, receipt, projection, wire, content-key, and graduation contracts with platform and cross-runtime peers.
    subgraph compute[RASM.COMPUTE]
        Model[Model runtime]
        Tensor[Tensor core]
        Solver[Solve spine]
        Runtime[Runtime plane]
        Symbolic[Symbolic CAS]
    end
    AppHost{{Rasm.AppHost}}
    AppUi{{Rasm.AppUi}}
    Data{{python:data}}
    Geometry{{python:geometry}}
    PyRuntime{{python:runtime}}
    Compute{{python:compute}}
    Core{{typescript:core}}
    AppHost e1@-->|"[PORT]: WorkLane"| Runtime
    AppHost e2@-->|"[PORT]: IChatClient"| Model
    AppHost e17@-->|"[PORT]: ShedVerdict"| Runtime
    AppHost e21@-->|"[PORT]: Spec"| Runtime
    Solver e3@-->|"[RECEIPT]: DigitalTwin"| AppHost
    Tensor e4@<-->|"[SHAPE]: PackKind"| AppHost
    Runtime e15@-->|"[PORT]: ComputeHookRail"| AppHost
    Runtime e5@-->|"[PROJECTION]: ResidencyPayload"| AppUi
    Tensor e6@<-->|"[SHAPE]: WgpuDevice"| AppUi
    Runtime e8@<-->|"[WIRE]: ComputeService"| Geometry
    Runtime e18@<-->|"[CONTENT_KEY]: ContentIdentity"| Geometry
    Runtime e9@<-->|"[WIRE]: ProtoVocabulary"| PyRuntime
    Runtime e19@-->|"[WIRE]: XxHash128"| PyRuntime
    Compute e10@-->|"[GRADUATION]: HandoffAxis"| Runtime
    Model e11@-->|"[GRADUATION]: GraduationEvidence"| Compute
    Symbolic e12@<-->|"[WIRE]: QuantityFamily"| Compute
    Solver e13@-->|"[SHAPE]: DoeDataset"| Data
    Data e20@-->|"[SHAPE]: GeoArrow"| Runtime
    Runtime e14@-->|"[WIRE]: ReceiptEnvelopeWire"| Core
    Symbolic e16@<-->|"[WIRE]: QuantityFamily"| Core
```

## [04]-[INTERNAL]

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: Rasm.Compute measured execution spine
    accDescr: Typed intent admits once at the boundary, substrate selection folds over row data and lands a receipt, bounded lanes enqueue work, total dispatch routes to the tensor, model, or remote lane, every outcome materializes as a compute receipt the sink-bound surface projects and emits, and progress cells deliver cadence-gated marks to observers.
    ComputeIntent(["ComputeIntent"]) e1@-->|Admit| AdmittedIntent["AdmittedIntent.Admit"]
    AdmittedIntent e3@-.->|Fin fail| ComputeFault["ComputeFault"]
    AdmittedIntent e4@-->|Plan| SubstrateSelection["SubstrateSelection"]
    SubstrateSelection e5@--> SelectionReceipt["SelectionReceipt"]
    SubstrateSelection e6@-.->|Fin fail| ComputeFault
    AdmittedIntent e7@-->|Enqueue| LaneRuntime["LaneRuntime"]
    LaneRuntime e8@-->|Pump| DispatchTable["DispatchTable"]
    SelectionReceipt e9@-->|Run| DispatchTable
    DispatchTable e10@--> TensorOps["TensorOps"]
    DispatchTable e11@--> ModelSessions["ModelSessions"]
    DispatchTable e12@--> WireChannels["WireChannels"]
    TensorOps e13@--> ComputeReceipt["ComputeReceipt"]
    ModelSessions e14@--> ComputeReceipt
    WireChannels e15@--> ComputeReceipt
    ComputeReceipt e16@-->|Emit| ReceiptSurface["ReceiptSurface"]
    ReceiptSurface e19@-->|Send| ReceiptSinkPort(["ReceiptSinkPort"])
    LaneRuntime e17@-->|Advance| ProgressCell["ProgressCell"]
    ProgressCell e18@-->|Observe / Stream / Instrument| Observers(["UiSchedulerPort / wire / InstrumentSet"])
```

Spine admits once, selects substrate over row data, enqueues on bounded lanes, dispatches to the tensor, model, or remote lane, and lands every outcome on a `ComputeReceipt` case at the sink while admission and selection failures fall to `ComputeFault` and `ProgressCell` streams cadence-gated marks. Per-stage guards, conditioning, and rails each lane composes live on the owning implementation pages.

## [05]-[ROUTING]

| [INDEX] | [CHANGE]                           | [OWNER_SURFACE]          | [SHAPE_OF_THE_EDIT]                                            |
| :-----: | :--------------------------------- | :----------------------- | :------------------------------------------------------------- |
|  [01]   | a new execution device or backend  | `Tensor/residency.md`    | one `Substrate` row                                            |
|  [02]   | a new sparse tensor operation      | `Tensor/factor.md`       | one `SparseTensorOpFamily` row                                 |
|  [03]   | a new differentiable primitive     | `Tensor/dispatch.md`     | one `DifferentiableOp` case at `[03]-[EQUIVALENCE_INTEROP]`    |
|  [04]   | a new estimator, optimizer, or UQ  | `Solver/optimizer.md`    | one `EstimatorKind`/`OptimizerKind`/`UncertaintyMethod` row    |
|  [05]   | a new material stress-update law   | `Solver/constitutive.md` | one `ConstitutiveModel` case                                   |
|  [06]   | a new discipline assessment        | `Analysis/assessment.md` | one `AssessmentResult` runner over the shared fact stream      |
|  [07]   | a new fault arm                    | `Runtime/admission.md`   | one arm at the 2200-band free frontier on its custody lane     |
|  [08]   | a new execution provider           | `Model/providers.md`     | one `ExecutionProvider` row; `Resolve` already answers absence |
|  [09]   | a new tile border, seam, or layout | `Model/inference.md`     | one `PadMode`, `TileBlend`, or `TileLayout` row                |
|  [10]   | a new sparse GEMV modality         | `Tensor/factor.md`       | one `GemvForm` case                                            |

## [06]-[BOUNDARIES]

Seam graph carries which owner exchanges which shape; the load-bearing cross-boundary invariants each Compute owner holds are:
- `Substrate.DeviceWgpu` binds the AppUi-owned wgpu device and holds compute-only resources; no second device or residency lattice.
- `Tensor/residency` consumes the host-neutral `EncodedGeometry` whole as `EncodedTensor` for the model lane.
- `Runtime/codecs` reads the same `EncodedGeometry` carrier for the lake landing.
- Geometry consumers compose the kernel's dtype-dispatched channel readers rather than re-slicing its arena.
- Host geometry folds at the kernel and AppHost capsules; no host type reaches an interior `Tensor`/`Solve`/`Estimator` signature.
- Compute owns the channel and companion-rpc orchestration; `Rasm.Bim` owns every semantic read, and neither crosses the seam.
- Strata run one direction: the AEC peers admit `UnitsNet` in-folder rather than reference the app-platform unit and solve owners downward.
- `Analysis` reads the concrete `ElementGraph` upward and writes a content-keyed assessment `GraphDelta` the caller applies; it mutates nothing.
- C# owns inference and classical fit; Python compute owns offline-learned models exchanged by content key over graduation evidence.
- `Rasm.Materials` SPECIFIES photo-to-PBR inference and `Model/inference` EXECUTES it; the branch-interior wire mints no corpus contract entry.
- Stage, model-card, and role identities cross as opaque keys this side dispatches on none of.
- Licence spellings resolve here to a grant verdict on the Compute-owned roster, fail-closed on an unrostered spelling.
- Every plane crosses as a content address injected ports resolve.
- Strata forbid a reference in either direction; admitting a model at the specifying end moves no Compute surface.
- `EnergyToolchain` resolves EnergyPlus by env var, configured path, or bundle; no hardcoded path or token column enters the policy.
- `EnergyRoute` converges local and cloud runs on the one `SqlFile` fold.
- Closed-form ISO/EN folds and the multi-ply `AssemblyAggregator` live in `Analysis`; single-material folds stay seam-owned, composed here.
- Design codes ride the `DesignCode`×`LimitState` capacity table.
- `Analysis/daylight` consumes the kernel `Spatial.Apply(SpatialOp.Wire)` decoded scene as the app-staged `ObstructionScene` payload.
- Daylight content key folds the assessment content key, so a re-shaded site re-keys; site evidence is the EPW header or the explicit `SolarSite`.
- `Runtime/receipts` descriptor and chargeback rows stay Compute-owned data a composition owner encodes onward; Compute owns no IaC surface.
- Every ledger fold reads the kernel `TenantContext` stamped on the envelope as its tenant partition, never a Compute-minted tenancy.
- `Runtime/transport` decodes MQTT and NATS CloudEvents onto the kernel `TraceCarrier` — MQTT from composition, NATS inline from `NatsMsg.Headers`.
- NATS Core pump drains `SubscribeAsync<byte[]>`; `BrokerChannels.Capture` admits samples as `ComputeIntent.SensorAdmit` on `WorkLane.CaptureIngest`.
- MQTT's event-delivered receive loop bridges through one bounded channel onto that same stream, its ack riding a successful enqueue alone.
- Parent adoption off that carrier is the kernel causal-frame band's; neither pump opens a span nor re-mints the pair.
- `Runtime/codecs` builds every columnar `RecordBatch` Compute produces over the kernel encode.
- Persistence `api-arrow` overlay carries IPC, LZ4/Zstd, ADBC, and Flight-SQL; its `Query/columnar` `Land` port redeems the batch.
- Compute holds one core `Apache.Arrow` reference and opens no Flight listener.

## [07]-[OWNER_LAW]

`System.Numerics.Tensors` `Tensor<T>` is the tensor, device-ness the `OrtResidency.DeviceResident` discriminant, and `TensorBridge` the sole `OrtValue` C-data factory feeding the single `BoundFlow` capsule; `LinearProvider`/`DenseOps`/`LevenbergMarquardt` and `SparseOps`/`SparseTensorOps` own the dense and sparse math. Solver, optimizer, UQ, and constitutive oracle couples only through the `Func<DesignPoint, Fin<Seq<double>>>` contract, an OR-Tools `CpModel` builds through the typed model-builder API, one `HybridCache` binds per lane, and one session binds per model identity.

Assessment outcome is the one `ComputeReceipt.Assessment` case declared as a `Runtime/receipts` partial by `Analysis/assessment`, every discipline runner returns the uniform `AssessmentResult` fact stream, and design codes ride the `DesignCode`×`LimitState` capacity table.

`ComputeFault` is one 2200-band union `Runtime/admission` custodies across partial lanes owned by `Symbolic/expression`, `Symbolic/dimensional`, `Analysis/assessment`, and `Runtime/scheduling`; each lane appends its arm at the band's free frontier, the EC3 boundary reuses the transport `EndpointUnreachable` arm rather than minting a carbon code, and every fault crosses the wire through the one `FaultDetail` family whose `Bands` registry mirrors the custody map. Compute's second custody is the Remote `WireFault` sub-band pinned reciprocally in the AppHost/AppUi/Persistence registries.
