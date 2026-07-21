# [FABRICATION_ARCHITECTURE]

`Rasm.Fabrication` maps host-neutral production fabrication over `{Rasm, Rasm.Element}`. Each sub-domain owns one namespace and one polymorphic owner over `FabricationPolicy`/`FabricationResult`. Every flagship terminates in a content-keyed machine artifact; `EgressKind` collapses egress onto entry vocabulary, and its fold seeds `ContentHash.Of`. `FabricationProjector : IElementProjection` is the sole Element dependency; AEC alignment crosses seam contracts and the content-keyed wire.

## [01]-[DOMAIN_MAP]

```text codemap
Rasm.Fabrication/
├── Process/                 # Entry vocabulary, axes, physics, rail, and plan orchestrator
│   ├── Owner.cs             # Fabrication entry owner and atoms vocabulary
│   ├── Family.cs            # ProcessKind and Machine axis families
│   ├── Physics.cs           # Material identity carrying per-modality physics and the removal budget
│   ├── Faults.cs            # FabricationFault registry over the FaultBand.Fabrication band
│   ├── Derivation.cs        # Derivation.Apply plan orchestrator
│   └── Telemetry.cs         # FabricationFact union, rasm.fabrication.* instrument roster, projection fan, classification rows, engine spans, hook roster, SLO rows
├── Tooling/                 # ISO-13399 tool intelligence, machinability, and wear
│   ├── Magazine.cs          # Provider-detached ToolAssembly owner, correspondence tables, typed-shortfall kitting, and ordered life scheduling
│   ├── CuttingData.cs       # Kienzle seeds, evidence-domain guard, power-law fit, and cutter-form projection on typed evidence rails
│   └── Wear.cs              # Taylor flank-wear, per-edge budgets, and condition-based remaining-life estimation
├── Geometry2D/              # 2D substrate: line, arc, and parametric-curve lanes
│   ├── Algebra.cs           # Clipper2 line-space operation algebra: topology, open runs, morphology, inspection, and field planes
│   ├── Arcs.cs              # CavalierContours arc-space owner with kerf, lead, and adaptive offsets
│   └── Curves.cs            # Parametric-curve substrate owner
├── Ingress/                 # Everything entering as geometry
│   ├── Profile.cs           # DXF/DWG census, lane resolution, OCS-correct contour healing, region nesting, and projected receipt ingress
│   ├── Solid.cs             # STEP/IGES/STL/3DM/3MF unit-resolved mesh admission, conditioning, topology evidence, and kernel repair
│   ├── Steel.cs             # DSTV NC1 path, text, or byte admission into arc-aware steel, topology, and face placement
│   └── Element.cs           # ElementGraph single or batch bake into component, connection, relation, and fact receipts
├── Toolpath/                # Subtractive CAM
│   ├── Motion.cs            # ProcessModality and CutStrategy generator arms
│   ├── Surface.cs           # OpenCAMLib cutter positioning over kernel on-mesh path layout
│   ├── Partition.cs         # Seeded Voronoi cells with border, centroid, and Lloyd-residual evidence
│   ├── Guard.cs             # Scope-stamped planar, medial, voxel, and robot collision receipt
│   ├── Skeleton.cs          # Per-component constant-engagement walk over the kernel clearance family
│   ├── Turning.cs           # Controller-neutral lathe algebra: CutSide-owned sweep, plunge, axial, thread, knurl, transfer
│   ├── Wire.cs              # Wire-EDM demand: closed cycle, registered guides, wire bow, retention, recovery, simultaneous blocks
│   ├── Link.cs              # Precedence-aware closed tour, tool/setup-aware objective, volumetric keepouts, guarded segment routing
│   └── Bevel.cs             # Station-varying section law, thermal/abrasive head compensation, coupled THC pass evidence
├── Kinematics/              # Motion topology, the decoded observation slice, and the fleet registry
│   ├── Cell.cs              # Robot targets, placement optimization over one loaded cell, compilation, library, and controller boundaries
│   ├── Machine.cs           # Parameterized machine-chain inverse by bounded least squares, TCP/RTCP, continuity, and motion dynamics
│   ├── Observation.cs       # MachineObservation decoded-telemetry union, execution and condition vocabularies, and the machine-scoped window
│   └── Fleet.cs             # Typed shop-capability, availability, tooling-state, and measured-performance registry
├── Additive/                # Production 3DP
│   ├── Slicing.cs           # FFF/DED planar slicing and the deposition-seed modality roster
│   ├── Implicit.cs          # PicoGK implicit voxel TPMS, lattice, VDB round-trip, and resin-powder lanes
│   ├── Production.cs        # Build orientation, machine profiles, and 3MF egress
│   ├── ScanPath.cs          # LPBF hatch union: meander, stripe, island, hexagon
│   └── Support.cs           # Overhang census, accumulation, and interface carve
├── Nesting/                 # Layout, yield, offcut lifecycle, and cut linking
│   ├── Nfp.cs               # NFP-feasibility true-shape nesting over stock inventory
│   ├── Stock.cs             # Rectangular cutting-stock yield engine
│   ├── Remnant.cs           # Offcut lifecycle: reconcile, lease, transition, retire, and yield
│   └── Linking.cs           # Cut-linking union: common-line, chain-cut, bridge, skeleton
├── Fixturing/               # Keep-out, setup, and assembly planning
│   ├── Workholding.cs       # Clamp and exclusion-zone keep-out family and the conditioning fold
│   ├── Setups.cs            # QuikGraph precedence scheduler owning setup-to-WCS assignment
│   └── Assembly.cs          # Join-precedence planning
├── Posting/                 # Machine-code emission
│   ├── Program.cs           # Dialect-neutral CutProgram AST, program admission, modal interpretation, and cut conditioning
│   ├── Dialect.cs           # Per-dialect emit over the PostDialect grammar family
│   └── Optimization.cs      # Feedrate, corner smoothing, and block-cap compaction over the AST
├── Verify/                  # Program-level truth
│   ├── Removal.cs           # PicoGK voxel material-removal verify into gouge/uncut/overcut receipts
│   ├── Probing.cs           # In-process metrology: probe rows, ICP datum best-fit, conformance verdicts
│   ├── Simulate.cs          # Modal-state execution walk over the parsed CutProgram
│   ├── Estimation.cs        # Cost estimation from the fabrication result
│   └── Audit.cs             # Additive-owned layer-stack pre-flight
├── Spec/                    # Production specs
│   ├── Tolerance.cs         # ISO 286 limits, admitted GD&T frames, datum targets, composites, general classes, texture, and ranked stackup
│   ├── Capability.cs        # Capability intervals, variables SPC, fitted dependence, correlated stackup, and history gates
│   └── Manufacturability.cs # Provenance-graded DfM evidence, severity-gated verdicts, and objective-row ranked routing
├── Documentation/           # Shop documentation
│   ├── Projection.cs        # Kernel multi-view projection — hidden-line, silhouette, outline, and section runs over a watertight source
│   ├── Traveler.cs          # DAG-normalized content-keyed traveler over the typed receipt corpus
│   └── Report.cs            # Sampled inspection, EN 10204, NDT reconciliation, NCR lifecycle, calibration recall, and signed passport egress
├── Forming/                 # Sheet forming
│   ├── Sheet.cs             # One unfold owner
│   ├── Brake.cs             # Best-first bend-sequence planning over the feasibility matrix
│   └── Tube.cs              # Tube centerline fold, elongation carry, and cope development
└── Joining/                 # Weld engineering
    ├── Weld.cs              # Joint-by-prep composition over boundary-resolved groove facts
    ├── Sequence.cs          # Distortion ordering: backstep, skip-weld, balanced, block
    └── Procedure.cs         # WPS/PQR essential-variable rows and the heat-input compliance gate
```

Sub-domain dependencies are acyclic. Split packages declare ledger nodes without splitting pages: `Process` places atoms at S0, terminal derivation at S4, and the telemetry fact fan at S5; `Kinematics` places motion at S1 and its consuming fleet at S3, and motion never reads fleet policy. Shared discriminants mint on atoms, while residual and verdict state flow forward as policy-case input. Per-flagship pipelines live on owning implementation pages.

## [02]-[STRATA]

Six strata order the sub-domains; split-package ledger nodes preserve one direction: `Process` places atoms at the floor and `Derivation` beside the CAM plane, while `Kinematics` places motion at S1 and its consuming fleet at S3. `Verify` parses the `CutProgram` AST `Posting` emits as a same-stratum fact; every cross-stratum consumption edge points down.

- S0 `Process` atoms — the one vocabulary floor: `FabricationPolicy`, `FabricationResult`, `EgressKind`, `ContentKey`, `Move`, `MotionDirective`, `SpecializedToolpathEnvelope`, `Loop`, `MaterialSpec`, `ProcessRange`, `EquipmentEnvelope`, and `FabricationFault`; every plane reads it, and it reads no sibling.
- S1 `Geometry2D` + `Ingress` + `Kinematics` motion and observation — substrate lanes over the atoms alone: `PolygonAlgebra`, `ArcAlgebra`, and `CurveAlgebra`; the `Ingress.Admit` fold and `AdmittedGeometry`; `MachineTool`, `MachineKinematics`, `RobotProgram`, and the `MachineObservation` decoded slice its measured consumers at S2 and above fold.
- S2 `Tooling` + `Nesting` + `Additive` — capability owners over the 2D algebra: `ToolAssembly`, `ToolSelection`, `CuttingData`, `PowerLawFit`, and `ToolWear`; `Nest`, `StockNest`, and `NoFitPolygon`; `Slice`, `SupportPolicy`, `ScanPolicy`, and `Audit`.
- S3 `Fixturing` + `Forming` + `Joining` + `Spec` + `Kinematics` fleet — planning owners: `Workholding`, `ExclusionZone`, and `SetupSchedule`; `FlatPattern` and `TubeProgram`; `Weld`, `JointPrep`, `Sequence`, and `Procedure`; `Tolerance`, `Capability`, and `Manufacturability`; `MachineInstance`, `ProcessEnvelope`, and `Fleet`.
- S4 `Toolpath` + `Process/Derivation` — the CAM plane composing tools, kinematics, and keep-outs (`Cam`, `MotionRun`, `Guard`, `BevelPass`) beside the `Derivation`/`FabricationProjector` terminal aggregator over the downstream plans.
- S5 `Posting` + `Verify` + `Documentation` + `Process` telemetry — emission and truth: the `CutProgram` AST and `Dialect` emit, the `Removal`/`Probe`/`Simulate` verifiers, the `Hlr`/`Traveler`/`QualityReport` shop documents, and the `FabricationFact` fan projecting settled receipts onto the `rasm.fabrication.*` instruments.

Same-stratum policy exchange among `Fixturing`, `Joining`, `Spec`, and the kinematics fleet carries no dependency-order edge; only their downstream consumers enter the stratum graph.

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart TB
    accTitle: Rasm.Fabrication interior strata
    accDescr: Six stacked strata from the posting, verify, and documentation truth tier through the CAM plane and derivation aggregator, the planning owners, the capability owners, and the substrate lanes onto the process atoms floor, every consumption edge downward and solid naming one sourced type, and one forbidden upward edge labeled as such.
    subgraph L5["S5 EMISSION + TRUTH"]
        Posting[Posting]
        Verify[Verify]
        Documentation[Documentation]
        Telemetry[Process telemetry]
    end
    subgraph L4["S4 CAM + DERIVATION"]
        Toolpath[Toolpath]
        Derivation[Derivation]
    end
    subgraph L3["S3 PLANNING"]
        Fixturing[Fixturing]
        Forming[Forming]
        Joining[Joining]
        Spec[Spec]
        Fleet[Kinematics fleet]
    end
    subgraph L2["S2 CAPABILITY"]
        Tooling[Tooling]
        Nesting[Nesting]
        Additive[Additive]
    end
    subgraph L1["S1 SUBSTRATE"]
        Geometry2D[Geometry2D]
        Ingress[Ingress]
        Motion[Kinematics motion]
    end
    subgraph L0["S0 PROCESS ATOMS"]
        Atoms[Process atoms]
    end
    Verify e2@-->|"[IMPORT]: DatumReceipt"| Fixturing
    Documentation e3@-->|"[IMPORT]: CapabilityReport"| Spec
    Toolpath e4@-->|"[IMPORT]: ToolAssembly"| Tooling
    Toolpath e5@-->|"[IMPORT]: MachineTool"| Motion
    Toolpath e6@-->|"[IMPORT]: ExclusionZone"| Fixturing
    Derivation e7@-->|"[IMPORT]: SetupSchedule"| Fixturing
    Derivation e8@-->|"[IMPORT]: Fleet, AvailabilityPlan.Finish"| Fleet
    Fixturing e9@-->|"[IMPORT]: CuttingData"| Tooling
    Spec e10@-->|"[IMPORT]: SupportPolicy"| Additive
    Joining e11@-->|"[IMPORT]: Move"| Atoms
    Tooling e12@-->|"[IMPORT]: PolygonAlgebra"| Geometry2D
    Nesting e13@-->|"[IMPORT]: PolygonAlgebra"| Geometry2D
    Additive e14@-->|"[IMPORT]: PolygonAlgebra"| Geometry2D
    Geometry2D e15@-->|"[IMPORT]: Loop"| Atoms
    Ingress e16@-->|"[IMPORT]: AdmittedComponent"| Atoms
    Motion e17@-->|"[IMPORT]: MachineAxis"| Atoms
    Fleet e18@-->|"[IMPORT]: SlotMap"| Tooling
    Documentation e22@-->|"[IMPORT]: ProcedureReceipt"| Joining
    Posting e23@-->|"[IMPORT]: WcsSlot"| Fixturing
    Posting e24@-->|"[IMPORT]: MotionDynamics"| Motion
    Telemetry e25@-->|"[IMPORT]: WearReceipt"| Tooling
    Telemetry e26@-->|"[IMPORT]: MachineMatch"| Fleet
    Telemetry e27@-->|"[IMPORT]: CapabilityReport"| Spec
    Telemetry e28@-->|"[IMPORT]: RunEvidence"| Atoms
    Atoms f1@-->|"forbidden: atoms upward"| L5
```

## [03]-[SEAMS]

`Toolpath/guard` owns every PicoGK voxel lease, and `Kinematics/cell` owns every Rhino3dm robot adapter; downstream receipts carry evidence and no native handle.

[POSTING]:
- `Posting/program` sends `CutProgram` and `EmitPolicy` to `Posting/dialect`; `PostImage` owns rendered records, bytes, physical count, and emitted `ContentKey`.
- `Toolpath` preserves controller instructions and specialized evidence through `MotionDirective`; `Posting/program` retains each directive in `GNode`, while `Posting/dialect` owns executable lowering or annotation spelling.
- `Posting/program` projects analytic `ProgramEvent.Motion` rows into the kernel `ToolpathPath`; line and arc spans share one `PackOp.Toolpath` carrier, and arc centre and sense remain digest-bearing channels.
- `Posting/program`, `Process/physics`, and `Tooling/cuttingdata` feed `Posting/optimization`; `OptimizationIngress` and `OptimizationEgress` close on `Fin<OptimizationResult>`.
- `Posting/dialect` lowers `GNode.CoordinateFrame` through `WcsSlot` into offset write and selection words.
- `Posting/optimization` prices every span through `MotionDynamics` rapid, feed, acceleration, and junction law.

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: Fabrication AEC-domain peer seams
    accDescr: Fabrication exchanges its registered projector and admitted graph shape with Element, publishes the tolerance wire to the Python artifacts plane, and sends its telemetry fact envelopes to the AppHost receipt rail; peer implementation shapes never cross any seam.
    subgraph fabrication[RASM.FABRICATION]
        Process[Process rail]
        Telemetry[Process telemetry]
        Ingress[Ingress admission]
        Joining[Joining engineering]
        Spec[Spec tolerances]
    end
    Element{{Rasm.Element}}
    Artifacts{{python:artifacts}}
    AppHost{{Rasm.AppHost}}
    Process e1@-->|"[PROJECTION]: FabricationProjector"| Element
    Element e2@-->|"[SHAPE]: ElementGraph"| Ingress
    Spec e8@-->|"[WIRE]: IToleranceEncoder bytes"| Artifacts
    Telemetry e9@-->|"[RECEIPT]: FabricationFact"| AppHost
    AppHost e10@-->|"[PORT]: TelemetryContributorPort"| Telemetry
    Telemetry e11@-->|"[HOOK]: FabricationHooks points"| AppHost
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
    accTitle: Fabrication kernel and platform seams
    accDescr: Fabrication sub-domain owners consuming kernel geometry from Rasm, publishing the toolpath pack back into it, and delivering the hidden-line receipt to the app shell, one labeled edge per contract family.
    subgraph fabrication[RASM.FABRICATION]
        Process[Process rail]
        Ingress[Ingress admission]
        Geometry2D[Geometry2D substrate]
        Toolpath[Toolpath CAM]
        Additive[Additive slicing]
        Forming[Forming unfold]
        Kinematics[Kinematics motion]
        Nesting[Nesting layout]
        Spec[Spec capability]
        Verify[Verify truth]
        Documentation[Documentation shop]
        Posting[Posting emission]
    end
    Rasm{{Rasm}}
    App([Rasm.App])
    Rasm e1@-->|"[SHAPE]: Predicate"| Process
    Rasm e2@-->|"[WIRE]: MeshSpace"| Ingress
    Rasm e3@-->|"[WIRE]: ParametricOp"| Geometry2D
    Rasm e4@-->|"[WIRE]: CurveSkeleton"| Toolpath
    Rasm e5@-->|"[WIRE]: SliceStack"| Additive
    Rasm e6@-->|"[WIRE]: Development"| Forming
    Rasm e7@-->|"[WIRE]: VectorIntent"| Kinematics
    Rasm e9@-->|"[PROJECTION]: ChartAtlas"| Nesting
    Rasm e10@-->|"[WIRE]: Stat"| Spec
    Rasm e11@-->|"[WIRE]: FitReceipt"| Verify
    Rasm e12@-->|"[WIRE]: DrawingProjection"| Documentation
    Posting e13@-->|"[WIRE]: ToolpathPath"| Rasm
    Documentation e14@-->|"[RECEIPT]: ProjectionReceipt"| App
```

## [04]-[FAULT_REGISTRY]

`FabricationFault` is one `[Union]` on the `FaultBand.Fabrication` band `Rasm.Element` owns. Each sub-domain folder owns its fault arms and lowers them onto the band; a folder producing no fault leaves its lane receipt-only, and projection routes the kernel geometry fault rather than minting its own. `Process/faults` owns the arm-to-code allocation and the band's free frontier; the arms preserving wire-code decode from before the folder partition retype in place, never reallocate.

Every `FabricationFault` case declares its owning `FabConcern` and stratum, so receipts partition without a second table; degenerate fixture geometry routes through `GeometryFault.DegenerateInput`.

## [05]-[CROSS_PACKAGE]

Seam edges carry which package exchanges which shape; the load-bearing cross-package invariants are:
- Every machine-consumable egress mints its content key through the kernel `ContentHash.Of` seed-zero entry, with no second mint.
- `EgressKind`, the local discriminant, federates to the Persistence `ArtifactKind` rows at the content-key boundary, never a type reference.
- `Fabrication` realizes the one `FabricationProjector` registration; every quantity lowered back to the seam rides that projector.
- An absent peer capability binds as an injected delegate column, so the contract remains whole without an implementation-shape dependency.
- Machine telemetry enters through the AppHost decode lane, never a direct transport reference; `Kinematics/observation` admits the decoded entities once, and every measured consumer — wear signals, fleet performance refresh, engagement measured-load ceilings — folds the one `MachineObservation` slice.
- Durable shop state rides the Persistence slot registry's contributed span as the `store.fabrication.<domain>.<verb>` family — remnant transitions, fleet performance horizons, magazine exchanges, capability history — each owning page naming its slot spellings as value federation, mounted as call-site data at the composition root.
- Solver memo truth content-keys through the same kernel mint the egress spine seeds: the runtime-carried `HybridCache` tier replays NFP pair polygons under `PairTable.Key` identities in process, and the durable tier federates at the Persistence cache seam beside the benchmark index.
- Fabrication speed claims resolve to `BenchmarkReceipt` rows: the `FabricationBench` roster keys the branch bench tier's gated cases as `{Suite}/{Case}` Persistence `BenchmarkRow` claims, and `ProbeRoute.Measured` authorizes its parallel substrate only against a mintable claim key.
- Program delivery closes chain-of-custody by value: the cell drive receipt re-mints a content key from the exact controller-bound records, `Posting/dialect` `ProgramDelivery` proves transfer integrity by digest equality, and the delivery fact rides the tap onto the receipt rail.
- Fabrication facts leave through the one `FabricationTap` port onto the AppHost receipt rail as `FabricationFact` envelopes; the `TelemetryContributorPort` carries the `rasm.fabrication.*` instrument roster inward at composition, the `FabricationInstruments.Arms` kind-arm table merges onto the AppHost receipt fan beside its own arms, and classification federates by value to the suite `DataClassification` taxonomy — never a type reference in either direction.
- Fabrication hook points register on the AppHost hook registry at composition through the runtime-carried `FabricationHooks` roster; modality and payload close at declaration, and subscribers attach only at app roots.
- Engine spans ride the `ActivitySource` named by `TelemetrySource.Fabrication`, admitted at the AppHost root source roster; trace-based exemplars join the fabrication histograms to their solve traces.
- `FabricationSlos` rows feed the AppHost alert rail and the deploy-plane dashboard compile from one row set; burn thresholds stay the core multi-window burn table, never re-decided here.
