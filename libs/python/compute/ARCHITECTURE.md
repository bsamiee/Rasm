# [PY_COMPUTE_ARCHITECTURE]

`compute` maps host-free offline scientific evidence outward through one rail: independent numeric-science sub-domains meet only through the one solve receipt, the one study spine, and the one graduation rail, and the numeric substrate every sub-domain admits through carries any backend array while the package imports no host runtime. Geometry, columnar data, and tensor sessions cross the `HandoffAxis` as receipt data, decoded and never re-owned.

## [01]-[DOMAIN_MAP]

```text codemap
compute/                    # Offline scientific evidence, graduating outward through one rail
├── solvers/                # Unified solve routes plus sensitivity, weak-form assembly, and field readout
│   ├── receipt.py          # SolverReceipt — the method-discriminated receipt every route folds
│   ├── linear.py           # LinearIntent — dense, sparse, and eigen solves
│   ├── nonlinear.py        # NonlinearIntent — root, minimise, fixed-point, and least-squares solves
│   ├── quadrature.py       # QuadratureIntent — quadrature, interpolation, and the weak-form FEM fold
│   ├── differential.py     # DifferentialIntent — adjoint-differentiable ODE, SDE, and CDE integration
│   ├── sensitivity.py      # Differentiation — reverse-mode and implicit-adjoint sensitivity over the solvers
│   ├── mesh.py             # MeshField — mesh topology, per-node and per-cell fields, weak-form assembly
│   └── field.py            # FieldQuery — interpolate, project, and resample readout over a discrete field
├── optimization/           # Offline optimization discriminated by problem structure
│   ├── design.py           # DesignProblem — differentiable design over the implicit-adjoint gradient
│   ├── program.py          # ProgramIntent — constrained, integer, global, and assignment programs
│   └── convex.py           # ConvexProgram — disciplined-convex programs with a dual-certificate proof
├── experiments/            # Study spine, run history, inference, and model assets
│   ├── study.py            # Study — DOE sampling, sensitivity, surrogate fitting, benchmark discriminant
│   ├── history.py          # RunHistory — content-keyed run persistence, partial resume, comparison
│   ├── inference.py        # Inference — gradient-MCMC posteriors with convergence diagnostics
│   └── model.py            # ModelAsset — classical-estimator validation, smoke inference, ONNX export
├── numerics/               # Numeric substrate every sub-domain admits through
│   ├── array.py            # ArrayPayload — namespace-dispatched array admission
│   ├── jit.py              # JitBackend — the LLVM and XLA compile routes over one capture table
│   ├── interval.py         # IntervalNumerics — the certified-interval floor ladder
│   ├── quantity.py         # UncertainQuantity — correlated uncertainty through unit algebra
│   └── statistics.py       # Statistics — in-memory hypothesis tests and MLE distribution fit
├── analysis/               # Classical-math evidence producers
│   ├── signal.py           # SignalOp — IIR/FIR filtering, spectral estimation, resample, wavelet fold
│   ├── transform.py        # SpectralReadout — in-memory DFT, trigonometric, and analytic-signal transforms
│   ├── symbolic.py         # SymbolicDerivation — symbolic lowering to a numpy or C handoff artifact
│   └── spatial.py          # SpatialQuery — neighbour, hull, Delaunay, Voronoi, and alpha-shape folds
└── graduation/             # Multi-domain graduation hub and C# stub codegen
    ├── handoff.py          # GraduationReceipt/HandoffAxis — outward egress, geometry decode, evidence weave
    └── codegen.py          # StubCodegen — the C# EvidenceBundle stub emitter under the drift gate
```

## [02]-[SEAMS]

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
    accTitle: Compute package seam registry
    accDescr: Compute sub-domain owners exchanging graduation evidence, quantities, content keys, frame shapes, and the runtime Kernel port with Rasm.Compute and the Python geometry, artifacts, runtime, and data peers, edge rails colored by kind and nodes classed by seam direction.
    subgraph compute[COMPUTE]
        Graduation[Graduation rail]
        Solvers[Solve receipt]
        Numerics[Numeric substrate]
        Experiments[Study spine]
        Analysis[Analysis producers]
        Optimization[Optimization programs]
    end
    Compute{{Rasm.Compute}}
    Runtime{{python:runtime}}
    Geometry([python:geometry])
    Artifacts([python:artifacts])
    Data([python:data])
    Graduation e1@-->|"[GRADUATION]: HandoffAxis"| Compute
    Compute e2@-->|"[WIRE]: EvidenceBundle"| Graduation
    Solvers e3@-->|"[PROJECTION]: SolverReceipt"| Compute
    Numerics e4@<-->|"[WIRE]: QuantityFamily"| Compute
    Geometry e5@-->|"[GRADUATION]: GeometryHandoff"| Graduation
    Artifacts e6@-->|"[GRADUATION]: HandoffAxis"| Graduation
    Artifacts e7@-->|"[SHAPE]: SignalOp"| Analysis
    Runtime e8@-->|"[CONTENT_KEY]: ParityReceipt"| Numerics
    Experiments e9@-->|"[BOUNDARY]: ResourceRef"| Runtime
    Data e10@-->|"[SHAPE]: FrameAdmission"| Experiments
    Runtime e11@-->|"[PORT]: Kernel"| Solvers
    Runtime e12@-->|"[PORT]: measured"| Graduation
    Runtime e13@-->|"[PORT]: Kernel"| Experiments
    Runtime e14@-->|"[PORT]: Kernel"| Analysis
    Runtime e15@-->|"[PORT]: Kernel"| Optimization
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef external fill:#8BE9FDBF,stroke:#8BE9FD,color:#282A36
    classDef recessed fill:#21222C,stroke:#6272A4,color:#F8F8F2
    classDef edgeData stroke:#FFB86C,color:#F8F8F2
    classDef edgeExternal stroke:#8BE9FD,color:#F8F8F2
    classDef edgeControl stroke:#FF79C6,color:#F8F8F2
    class Graduation,Solvers,Numerics,Experiments,Analysis,Optimization primary
    class Compute,Runtime external
    class Geometry,Artifacts,Data recessed
    class e1,e2,e4,e5,e6,e8 edgeData
    class e3 edgeExternal
    class e7,e9,e10,e11,e12,e13,e14,e15 edgeControl
```

`ContentIdentity` keys ride beneath the `ParityReceipt` parity seam, the graduation reverse leg carries `EvidenceBundle`, C#-owned as `GraduationEvidence`, and `UncertainQuantity` is the interior owner beneath the C#-spelled `QuantityFamily` wire; each collapsed edge stands for every contract between the two owners at that kind, with the per-contract wiring on the owning implementation pages.

## [03]-[INTERNAL]

Independent sub-domains produce nothing outward on their own: every solve route folds into the one solve receipt, every experiment into the one study spine, and those two rails plus the direct analysis and numeric producers converge on the single graduation rail that crosses outward.

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
    edgeLabelBackground: "#21222C"
    labelBackgroundColor: "#21222C"
  themeCSS: ".nodeLabel{font-size:13px;font-weight:500}.edgeLabel{font-size:12px;font-weight:500}.cluster-label .nodeLabel{font-size:13.5px;font-weight:700;letter-spacing:.08em}.edge-thickness-normal{stroke-width:2px}.edge-thickness-thick{stroke-width:3px}.edge-pattern-dashed,.edge-pattern-dotted{stroke-width:1.5px;stroke-dasharray:4 6}.node rect,.node circle,.node polygon,.node path,.node .outer-path{stroke-width:1.5px;filter:none!important}.cluster rect{stroke-width:1px!important;stroke-dasharray:5 4!important;filter:none!important}.marker path{transform:scale(.8);transform-origin:5px 5px}.marker circle{transform:scale(.48);transform-origin:5px 5px}.edgeLabel rect{transform-box:fill-box;transform-origin:center;transform:scale(1.1,1.2)}"
---
flowchart LR
    accTitle: Compute internal convergence
    accDescr: The independent numeric-science sub-domains converging through the one solve receipt and the one study spine onto the single graduation rail that crosses outward, edge rails colored by the shape each stage folds.
    SolveRoutes[[Solve routes]] e1@-->|"SolverReceipt"| Receipt[Solve receipt]
    Optimization[[Optimization programs]] e2@-->|"SolveStatus"| Receipt
    History[[Run history]] e3@-->|"StudyReceipt"| Study[Study spine]
    Inference[[Inference and models]] e4@-->|"Measured"| Study
    Receipt e5@-->|"GraduationReceipt"| Handoff[Graduation rail]
    Study e6@-->|"EvidenceScope"| Handoff
    Analysis[[Analysis producers]] e7@-->|"EvidenceScope"| Handoff
    Numerics[[Numeric substrate]] e8@-->|"EvidenceScope"| Handoff
    Handoff e9@-->|"HandoffAxis"| Out([Outward egress])
    classDef boundary fill:#282A36,stroke:#BD93F9,color:#F8F8F2
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef edgeData stroke:#FFB86C,color:#F8F8F2
    classDef edgeSuccess stroke:#50FA7B,color:#F8F8F2
    classDef edgeControl stroke:#FF79C6,color:#F8F8F2
    class Out boundary
    class SolveRoutes,Optimization,History,Inference,Receipt,Study,Analysis,Numerics,Handoff primary
    class e1,e6,e7,e8 edgeSuccess
    class e2,e3,e4 edgeControl
    class e5,e9 edgeData
```

Convergence above answers what folds where; the strata below answer which sub-domain may import which.

- S0 `graduation` — mints the outward rail exactly once (`HandoffAxis`, `GraduationReceipt`, `EvidenceScope`) and imports no sibling; `codegen` composes `handoff` inside the stratum, and every producer returns through the hub.
- S1 `numerics` + `solvers` — one band: `solvers` folds `SolverReceipt`/`SolveStatus` onto the rail, `numerics` admits `ArrayPayload` and the `JitBackend`/`LoweredSpec` compile routes; the interleave is module-acyclic — `quadrature` composes `jit`, `interval` composes the receipt `graduate` fold — so no stratum cycle exists at module grain.
- S2 `analysis` + `experiments` + `optimization` — the producer tier no sibling imports: analysis composes `ArrayPayload`, experiments the `JitBackend` capture and the study spine, optimization the `SolveStatus` verdicts; all three graduate through `EvidenceScope`.

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
    accTitle: Compute interior import strata
    accDescr: Three import strata — the analysis, experiments, and optimization producers over the numerics-solvers band over the graduation foundation — each labeled downward edge naming its one sourced type, the band interleave held to prose, and one forbidden upward edge styled red.
    subgraph C2["S2 PRODUCERS"]
        Analysis[analysis]
        Experiments[experiments]
        Optimization[optimization]
    end
    subgraph C1["S1 NUMERICS + SOLVERS"]
        Numerics[numerics]
        Solvers[solvers]
    end
    subgraph C0["S0 GRADUATION"]
        Codegen[codegen]
        Handoff[handoff]
    end
    Analysis s1@-->|"[IMPORT]: ArrayPayload"| Numerics
    Analysis s2@-->|"[IMPORT]: EvidenceScope"| Handoff
    Experiments s3@-->|"[IMPORT]: JitBackend"| Numerics
    Experiments s4@-->|"[IMPORT]: EvidenceScope"| Handoff
    Optimization s5@-->|"[IMPORT]: SolveStatus"| Solvers
    Optimization s6@-->|"[IMPORT]: EvidenceScope"| Handoff
    Numerics s7@-->|"[IMPORT]: GraduationReceipt"| Handoff
    Solvers s8@-->|"[IMPORT]: GraduationReceipt"| Handoff
    Codegen s9@-->|"[IMPORT]: EvidenceScope"| Handoff
    Handoff f1@-->|"forbidden: upward import"| C2
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef recessed fill:#21222C,stroke:#6272A4,color:#F8F8F2
    classDef edgeControl stroke:#FF79C6,color:#F8F8F2
    classDef edgeError stroke:#FF5555,stroke-width:3px,color:#F8F8F2
    class Analysis,Experiments,Optimization,Numerics,Solvers primary
    class Codegen,Handoff recessed
    class s1,s2,s3,s4,s5,s6,s7,s8,s9 edgeControl
    class f1 edgeError
```

## [04]-[ORGANIZATION]

Sub-domains are independent numeric-science concerns meeting only through the one solve receipt, the one study spine, and the one graduation rail. Each discriminates its variation by problem structure — solve route, program class, experiment stage — and composes the numeric substrate and solver owners rather than re-owning them, so a new numeric route or program class lands as a case on the owning discriminant, never a parallel owner. Per-owner wiring — which substrate each route folds, which adjoint the sensitivity owner reads, how the study spine stages its sampling — lives on the owning implementation pages.

## [05]-[BOUNDARIES]

- `compute` is not a production runtime, benchmark authority, substrate selector, tensor-session owner, or product-receipt owner.
- It owns offline evidence that graduates through the one rail.
- Columnar and labelled-array interchange stays in the `data` branch; `compute` composes its shapes, never re-owning the data interior.
- Labelled arms extract `.data` and coords; the study DOE-frame arm admits through the published data contract.
- Columnar and gridded statistical aggregation is the `data` branch gridded/field owner; `numerics/statistics` operates on in-memory samples only.
- Geometry tessellation, registration, and topology stay in the `geometry` branch, graduating as `GeometryHandoff` data under its minted union.
- `compute` decodes the crossing at its `HandoffAxis` geometry case; it never re-implements geometry and never imports it.
- Classical statistics, validated numerics, surrogate and classification model assets, and gradient-MCMC inference are in-scope.
- Generative and deep-learning model authoring is out of scope across every sub-domain.
