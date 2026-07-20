# [FABRICATION_ARCHITECTURE]

`Rasm.Fabrication` maps host-neutral production fabrication over `{Rasm, Rasm.Element}`. Each sub-domain owns one namespace and one polymorphic owner over `FabricationPolicy`/`FabricationResult`. Every flagship terminates in a content-keyed machine artifact; `EgressKind` collapses egress onto entry vocabulary, and its fold seeds `ContentHash.Of`. `FabricationProjector : IElementProjection` is the sole Element dependency; AEC alignment crosses seam contracts and the content-keyed wire.

## [01]-[DOMAIN_MAP]

```text codemap
Rasm.Fabrication/
├── Process/                 # Entry vocabulary, axes, physics, rail, and plan orchestrator
│   ├── Owner.cs             # Fabrication entry owner and atoms vocabulary
│   ├── Family.cs            # ProcessKind and Machine axis families
│   ├── Physics.cs           # Material identity carrying per-modality physics and the removal budget
│   ├── Faults.cs            # FabricationFault band-2700 registry entrypoint
│   └── Derivation.cs        # RunDerive plan orchestrator
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
├── Kinematics/              # Motion topology and the fleet registry
│   ├── Cell.cs              # Robot targets, placement optimization over one loaded cell, compilation, library, and controller boundaries
│   ├── Machine.cs           # Parameterized machine-chain inverse by bounded least squares, TCP/RTCP, continuity, and motion dynamics
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
│   ├── Remnant.cs           # Offcut lifecycle partial
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

Sub-domain dependencies are acyclic. Split packages declare ledger nodes without splitting pages: `Process` places atoms at S0 and terminal derivation at S4; `Kinematics` places motion at S1 and its consuming fleet at S3, and motion never reads fleet policy. Shared discriminants mint on atoms, while residual and verdict state flow forward as policy-case input. Per-flagship pipelines live on owning implementation pages.

## [02]-[STRATA]

Six strata order the sub-domains; split-package ledger nodes preserve one direction: `Process` places atoms at the floor and `Derivation` beside the CAM plane, while `Kinematics` places motion at S1 and its consuming fleet at S3. `Verify` parses the `CutProgram` AST `Posting` emits as a same-stratum fact; every cross-stratum consumption edge points down.

- S0 `Process` atoms — the one vocabulary floor: `FabricationPolicy`, `FabricationResult`, `EgressKind`, `ContentKey`, `Move`, `MotionDirective`, `SpecializedToolpathEnvelope`, `Loop`, `MaterialSpec`, `ProcessRange`, `EquipmentEnvelope`, and `FabricationFault`; every plane reads it, and it reads no sibling.
- S1 `Geometry2D` + `Ingress` + `Kinematics` motion — substrate lanes over the atoms alone: `PolygonAlgebra`, `ArcAlgebra`, and `CurveAlgebra`; the `Ingress.Admit` fold and `AdmittedGeometry`; `MachineTool`, `MachineKinematics`, and `RobotProgram`.
- S2 `Tooling` + `Nesting` + `Additive` — capability owners over the 2D algebra: `ToolAssembly`, `ToolSelection`, `CuttingData`, `PowerLawFit`, and `ToolWear`; `Nest`, `StockNest`, and `NoFitPolygon`; `Slice`, `SupportPolicy`, `ScanPolicy`, and `Audit`.
- S3 `Fixturing` + `Forming` + `Joining` + `Spec` + `Kinematics` fleet — planning owners: `Workholding`, `ExclusionZone`, and `SetupSchedule`; `FlatPattern` and `TubeProgram`; `Weld`, `JointPrep`, `Sequence`, and `Procedure`; `Tolerance`, `Capability`, and `Manufacturability`; `MachineInstance`, `ProcessEnvelope`, and `Fleet`.
- S4 `Toolpath` + `Process/Derivation` — the CAM plane composing tools, kinematics, and keep-outs (`Cam`, `MotionRun`, `Guard`, `BevelPass`) beside the `Derivation`/`FabricationProjector` terminal aggregator over the downstream plans.
- S5 `Posting` + `Verify` + `Documentation` — emission and truth: the `CutProgram` AST and `Dialect` emit, the `Removal`/`Probe`/`Simulate` verifiers, and the `Hlr`/`Traveler`/`QualityReport` shop documents.

Same-stratum policy exchange among `Fixturing`, `Joining`, `Spec`, and the kinematics fleet carries no dependency-order edge; only their downstream consumers enter the stratum graph.

```mermaid
---
config:
  theme: base
  look: classic
  layout: elk
  flowchart:
    curve: linear
    padding: 25
  themeVariables:
    darkMode: true
    fontFamily: "SF Mono, Menlo, Cascadia Mono, Segoe UI Mono, Consolas, monospace"
    useGradient: false
    dropShadow: "none"
    background: "#282A36"
    primaryColor: "#44475A"
    primaryTextColor: "#F8F8F2"
    primaryBorderColor: "#BD93F9"
    lineColor: "#FF79C6"
    textColor: "#F8F8F2"
    clusterBkg: "#21222C"
    clusterBorder: "#D6BCFA"
    edgeLabelBackground: "#21222C"
    labelBackgroundColor: "#21222C"
    titleColor: "#D6BCFA"
  themeCSS: ".nodeLabel{font-size:13px;font-weight:500}.edgeLabel{font-size:12px;font-weight:500}.cluster-label .nodeLabel{font-size:13.5px;font-weight:700;letter-spacing:.08em}.edge-thickness-normal{stroke-width:2px}.edge-thickness-thick{stroke-width:3px}.edge-pattern-dashed,.edge-pattern-dotted{stroke-width:1.5px;stroke-dasharray:4 6}.node rect,.node circle,.node polygon,.node path,.node .outer-path{stroke-width:1.5px;filter:none!important}.cluster rect{stroke-width:1px!important;stroke-dasharray:5 4!important;filter:none!important}.marker path{transform:scale(.8);transform-origin:5px 5px}.marker circle{transform:scale(.48);transform-origin:5px 5px}.edgeLabel rect{transform-box:fill-box;transform-origin:center;transform:scale(1.1,1.2)}"
---
flowchart TB
    accTitle: Rasm.Fabrication interior strata
    accDescr: Six stacked strata from the posting, verify, and documentation truth tier through the CAM plane and derivation aggregator, the planning owners, the capability owners, and the substrate lanes onto the process atoms floor, every consumption edge downward and solid naming one sourced type, and one forbidden upward edge styled red.
    subgraph L5["S5 EMISSION + TRUTH"]
        Posting[Posting]
        Verify[Verify]
        Documentation[Documentation]
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
    Atoms f1@-->|"forbidden: atoms upward"| L5
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef recessed fill:#21222C,stroke:#6272A4,color:#F8F8F2
    classDef edgeControl stroke:#FF79C6,color:#F8F8F2
    classDef edgeError stroke:#FF5555,stroke-width:3px,color:#F8F8F2
    class Posting,Verify,Documentation,Toolpath,Derivation,Fixturing,Forming,Joining,Spec,Fleet,Tooling,Nesting,Additive primary
    class Geometry2D,Ingress,Motion,Atoms recessed
    class e2,e3,e4,e5,e6,e7,e8,e9,e10,e11,e12,e13,e14,e15,e16,e17,e18,e22,e23,e24 edgeControl
    class f1 edgeError
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
  theme: base
  look: classic
  layout: elk
  flowchart:
    curve: linear
    padding: 25
  themeVariables:
    darkMode: true
    fontFamily: "SF Mono, Menlo, Cascadia Mono, Segoe UI Mono, Consolas, monospace"
    useGradient: false
    dropShadow: "none"
    background: "#282A36"
    primaryColor: "#44475A"
    primaryTextColor: "#F8F8F2"
    primaryBorderColor: "#BD93F9"
    lineColor: "#FF79C6"
    textColor: "#F8F8F2"
    clusterBkg: "#21222C"
    clusterBorder: "#D6BCFA"
    edgeLabelBackground: "#21222C"
    labelBackgroundColor: "#21222C"
    titleColor: "#D6BCFA"
  themeCSS: ".nodeLabel{font-size:13px;font-weight:500}.edgeLabel{font-size:12px;font-weight:500}.cluster-label .nodeLabel{font-size:13.5px;font-weight:700;letter-spacing:.08em}.edge-thickness-normal{stroke-width:2px}.edge-thickness-thick{stroke-width:3px}.edge-pattern-dashed,.edge-pattern-dotted{stroke-width:1.5px;stroke-dasharray:4 6}.node rect,.node circle,.node polygon,.node path,.node .outer-path{stroke-width:1.5px;filter:none!important}.cluster rect{stroke-width:1px!important;stroke-dasharray:5 4!important;filter:none!important}.marker path{transform:scale(.8);transform-origin:5px 5px}.marker circle{transform:scale(.48);transform-origin:5px 5px}.edgeLabel rect{transform-box:fill-box;transform-origin:center;transform:scale(1.1,1.2)}"
---
flowchart LR
    accTitle: Fabrication AEC-domain peer seams
    accDescr: Fabrication exchanges its registered projector and admitted graph wire with Element, then publishes the tolerance wire to the Python artifacts plane; peer implementation shapes never cross either seam.
    subgraph fabrication[RASM.FABRICATION]
        Process[Process rail]
        Ingress[Ingress admission]
        Joining[Joining engineering]
        Spec[Spec tolerances]
    end
    Element{{Rasm.Element}}
    Artifacts{{python:artifacts}}
    Process e1@-->|"[PROJECTION]: FabricationProjector"| Element
    Element e2@-->|"[WIRE]: ElementGraph"| Ingress
    Spec e8@-->|"[WIRE]: IToleranceEncoder bytes"| Artifacts
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef external fill:#8BE9FDBF,stroke:#8BE9FD,color:#282A36
    classDef annotation fill:#21222C,stroke:#6272A4,color:#F8F8F2
    classDef edgeData stroke:#FFB86C,color:#F8F8F2
    classDef edgeExternal stroke:#8BE9FD,color:#F8F8F2
    classDef edgeControl stroke:#FF79C6,color:#F8F8F2
    class Process,Ingress,Joining,Spec primary
    class Element,Artifacts external
    class e1 edgeExternal
    class e2,e8 edgeData
```

```mermaid
---
config:
  theme: base
  look: classic
  layout: elk
  flowchart:
    curve: linear
    padding: 25
  themeVariables:
    darkMode: true
    fontFamily: "SF Mono, Menlo, Cascadia Mono, Segoe UI Mono, Consolas, monospace"
    useGradient: false
    dropShadow: "none"
    background: "#282A36"
    primaryColor: "#44475A"
    primaryTextColor: "#F8F8F2"
    primaryBorderColor: "#BD93F9"
    lineColor: "#FF79C6"
    textColor: "#F8F8F2"
    clusterBkg: "#21222C"
    clusterBorder: "#D6BCFA"
    edgeLabelBackground: "#21222C"
    labelBackgroundColor: "#21222C"
    titleColor: "#D6BCFA"
  themeCSS: ".nodeLabel{font-size:13px;font-weight:500}.edgeLabel{font-size:12px;font-weight:500}.cluster-label .nodeLabel{font-size:13.5px;font-weight:700;letter-spacing:.08em}.edge-thickness-normal{stroke-width:2px}.edge-thickness-thick{stroke-width:3px}.edge-pattern-dashed,.edge-pattern-dotted{stroke-width:1.5px;stroke-dasharray:4 6}.node rect,.node circle,.node polygon,.node path,.node .outer-path{stroke-width:1.5px;filter:none!important}.cluster rect{stroke-width:1px!important;stroke-dasharray:5 4!important;filter:none!important}.marker path{transform:scale(.8);transform-origin:5px 5px}.marker circle{transform:scale(.48);transform-origin:5px 5px}.edgeLabel rect{transform-box:fill-box;transform-origin:center;transform:scale(1.1,1.2)}"
---
flowchart LR
    accTitle: Fabrication kernel and platform seams
    accDescr: Fabrication sub-domain owners consuming kernel geometry from Rasm, publishing the toolpath pack back into it, and delivering the hidden-line receipt to the app shell, edge rails colored by kind and nodes classed by seam direction.
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
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef external fill:#8BE9FDBF,stroke:#8BE9FD,color:#282A36
    classDef annotation fill:#21222C,stroke:#6272A4,color:#F8F8F2
    classDef edgeData stroke:#FFB86C,color:#F8F8F2
    classDef edgeSuccess stroke:#50FA7B,color:#F8F8F2
    classDef edgeExternal stroke:#8BE9FD,color:#F8F8F2
    classDef edgeControl stroke:#FF79C6,color:#F8F8F2
    class Process,Ingress,Geometry2D,Toolpath,Additive,Forming,Kinematics,Nesting,Spec,Verify,Documentation,Posting primary
    class Rasm external
    class App annotation
    class e2,e3,e4,e5,e6,e7,e10,e11,e12,e13 edgeData
    class e1 edgeControl
    class e9 edgeExternal
    class e14 edgeSuccess
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
- Machine telemetry enters through the AppHost decode lane, never a direct transport reference; every telemetry read consumes the one decoded slice.
