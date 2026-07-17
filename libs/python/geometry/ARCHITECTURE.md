# [PY_GEOMETRY_ARCHITECTURE]

`geometry` maps the host-free geometry and IFC/BIM band of the Python branch as the load-bearing cross-boundary owner: each sub-domain folder maps to one namespace, and the `graduation` spine mints the content-keyed evidence receipt every producer graduates through. It is a peer producer, never a Rasm consumer — alignment travels through the `ComputeService`/`ArtifactSync` contract and the content-keyed GLB tessellation rail, never a shared reference.

## [01]-[DOMAIN_MAP]

```text codemap
geometry/
├── graduation.py         # Tier-0 evidence spine: the subject union and handoff carrier every producer composes
├── scan/                 # Point-cloud and 3D-scan ingestion, registration, deviation, and reconstruction
│   ├── ingestion.py      # Point-cloud ingestion and E57 station-provenance decode over the filter graph
│   ├── registration.py   # Global then multi-scale point-cloud registration
│   ├── deviation.py      # Signed nearest-surface deviation against the content-keyed reference
│   └── reconstruction.py # Registered-cloud-to-watertight-mesh reconstruction, closure-graded
├── ifc/                  # IFC property, quantity, and relationship analysis, validation, and 5D/4D lifecycle
│   ├── analysis.py       # Pset, IDS, clash, space-program, and BCF analysis and BIM-compliance evidence
│   ├── costing.py        # 5D quantity take-off, cost rollup, 4D scheduling, and revision diff
│   ├── selector.py       # Validated selector grammar gating element selection
│   ├── authoring.py      # IFC spatial, element, and geometry authoring transactions over the GUID rail
│   └── structural.py     # Section-integral properties over IfcProfileDef and the warping/plastic/shear FE
├── mesh/                 # GLB tessellation daemon, serve wire, CAD-STEP hop, mesh algebra, and B-rep
│   ├── daemon.py         # Tessellation daemon: source bytes and policy to per-element mesh rails
│   ├── serve.py          # Tessellation servicer registered in the runtime ServerHost
│   ├── cad.py            # STEP and IGES B-rep to GLB over the OCCT XCAF bridge
│   ├── repair.py         # Watertight repair, winding and normal fix, and the public manifold boolean
│   ├── brep.py           # B-rep evaluation: booleans, sew and NURBS conditioning, cross-section offset
│   ├── spatial.py        # Proximity, ray, contains, bounds/nearest, and signed-clearance queries
│   └── quality.py        # Aspect, skewness, manifold, and genus receipts and the public closure fold
├── graph/                # Non-manifold topology, AEC computational geometry, and network analytics
│   ├── analytic.py       # Analytic-value reducer union, ranked board fold, and census projections
│   ├── nonmanifold.py    # CellComplex construction, decomposition, adjacency, and the cached dual graph
│   ├── algebra.py        # Network adjacency, form-finding, numerical primitives, and mesh algebra
│   └── features.py       # Centrality, community, cycle, and connectivity analytics over the network graph
└── energy/               # Out-of-process building-physics band: climate, model, district, and simulation
    ├── climate.py        # EPW admission, series algebra, solar sunpath, and thermal-comfort maps
    ├── model.py          # HBJSON and BIM-to-BEM admission under one gate with standards-resolved assignment
    ├── district.py       # District admission, auto-zoning, and the to-honeybee model explosion
    └── simulate.py       # Offloaded energy translation, recipe binding, and result decode
```

## [02]-[SEAMS]

Seam map splits by counterpart role — the C# cross-runtime peers on one fence, the Python siblings on the other. An in-package relation between two geometry sub-domains is never a seam; it lives in the codemap, and the `graph` sub-domain projects only onto the home `graduation` spine, so it carries no cross-boundary edge.

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
    accTitle: Geometry cross-runtime C# peer seams
    accDescr: Geometry sub-domain owners exchanging tessellation wires, content keys, IFC crossings, and validation evidence with the C# runtimes Rasm.Compute, Rasm.Element, and Rasm.Bim, edge rails colored by kind and nodes classed by seam direction.
    subgraph geometry[GEOMETRY]
        Mesh[Mesh tessellation]
        Ifc[IFC analysis]
        Scan[Scan ingest]
        Energy[Energy band]
    end
    Bim{{Rasm.Bim}}
    Compute{{Rasm.Compute}}
    Element{{Rasm.Element}}
    Mesh e1@<-->|"[WIRE]: ComputeService"| Compute
    Mesh e2@<-->|"[CONTENT_KEY]: ContentIdentity"| Compute
    Mesh e3@<-->|"[WIRE]: GlbContentHash"| Element
    Ifc e4@<-->|"[WIRE]: IfcWire"| Bim
    Ifc e5@-->|"[BOUNDARY]: IdsVerdict"| Bim
    Bim e6@-->|"[SHAPE]: GlbReference"| Scan
    Energy e7@<-->|"[WIRE]: Hbjson"| Bim
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef external fill:#8BE9FDBF,stroke:#8BE9FD,color:#282A36
    classDef edgeData stroke:#FFB86C,color:#F8F8F2
    classDef edgeControl stroke:#FF79C6,color:#F8F8F2
    class Mesh,Ifc,Scan,Energy primary
    class Bim,Compute,Element external
    class e1,e2,e3,e4,e7 edgeData
    class e5,e6 edgeControl
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
    accTitle: Geometry Python sibling seams
    accDescr: Geometry sub-domain owners exchanging graduation receipts, tessellation registry rows, mesh payloads, point records, a recipe port, and result frames with the Python compute, runtime, data, and artifacts siblings, edge rails colored by kind and nodes classed by seam direction.
    subgraph geometry[GEOMETRY]
        Graduation[Graduation spine]
        Mesh[Mesh tessellation]
        Scan[Scan ingest]
        Energy[Energy band]
    end
    Compute([python:compute])
    Runtime{{python:runtime}}
    Data{{python:data}}
    Artifacts{{python:artifacts}}
    Graduation e1@-->|"[GRADUATION]: GeometryHandoff"| Compute
    Mesh e2@<-->|"[WIRE]: TessellationRequest"| Runtime
    Mesh e3@-->|"[CONTENT_KEY]: ContentIdentity"| Runtime
    Data e4@-->|"[SHAPE]: MeshPayload"| Mesh
    Mesh e5@-->|"[BOUNDARY]: Trimesh"| Data
    Mesh e6@-->|"[BOUNDARY]: SceneGrid"| Artifacts
    Data e7@-->|"[SHAPE]: PointRecordTable"| Scan
    Energy e8@-->|"[PORT]: RecipeInterface"| Runtime
    Energy e9@-->|"[SHAPE]: ResultFrame"| Data
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef external fill:#8BE9FDBF,stroke:#8BE9FD,color:#282A36
    classDef recessed fill:#21222C,stroke:#6272A4,color:#F8F8F2
    classDef edgeData stroke:#FFB86C,color:#F8F8F2
    classDef edgeControl stroke:#FF79C6,color:#F8F8F2
    class Graduation,Mesh,Scan,Energy primary
    class Runtime,Data,Artifacts external
    class Compute recessed
    class e1,e2,e3 edgeData
    class e4,e5,e6,e7,e8,e9 edgeControl
```

Each collapsed edge stands for every contract between that sub-domain and that partner at the load-bearing kind: the streaming GLB transport, the IFC projection, and the payload shapes fold into the one labeled rail, and the per-contract wiring lives on the owning implementation pages. `GlbContentHash` spells from its Rasm.Element owner; geometry interior pages spell only the `ContentKey` mint beneath it. Scene facts cross one-way as glb bytes the artifacts `SceneGrid.of_glb` admits, and geometry receives nothing back on that boundary.

## [03]-[INTERNAL]

- S0 `graduation` — mints the evidence spine exactly once (`GeometrySubject`, `GeometryHandoff`, the `ContentKey` fold) and imports no sibling; every producer returns through it.
- S1 `mesh` + `ifc` + `graph` + `energy` — producer tier composing the spine alone; no import crosses among the four, and each folds its receipts onto `GeometryHandoff`.
- S2 `scan` — the one cross-producer consumer: composes graduation plus the mesh quality receipts (`QualityMetrics`) for deviation and reconstruction closure grading.

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
    accTitle: Geometry interior import strata
    accDescr: Three import strata — scan over the mesh, ifc, graph, and energy producer tier over the graduation foundation — each labeled downward edge naming its one sourced type and one forbidden upward edge styled red.
    subgraph G2["S2 SCAN"]
        Scan[scan]
    end
    subgraph G1["S1 PRODUCERS"]
        Mesh[mesh]
        Ifc[ifc]
        Graph[graph]
        Energy[energy]
    end
    subgraph G0["S0 GRADUATION"]
        Graduation[graduation]
    end
    Scan s1@-->|"[IMPORT]: QualityMetrics"| Mesh
    Scan s2@-->|"[IMPORT]: GeometryHandoff"| Graduation
    Mesh s3@-->|"[IMPORT]: GeometrySubject"| Graduation
    Ifc s4@-->|"[IMPORT]: GeometryHandoff"| Graduation
    Graph s5@-->|"[IMPORT]: GeometryHandoff"| Graduation
    Energy s6@-->|"[IMPORT]: GeometryHandoff"| Graduation
    Scan ~~~ Energy
    Graduation f1@-->|"forbidden: upward import"| G2
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef recessed fill:#21222C,stroke:#6272A4,color:#F8F8F2
    classDef edgeControl stroke:#FF79C6,color:#F8F8F2
    classDef edgeError stroke:#FF5555,stroke-width:3px,color:#F8F8F2
    class Scan,Mesh,Ifc,Graph,Energy primary
    class Graduation recessed
    class s1,s2,s3,s4,s5,s6 edgeControl
    class f1 edgeError
```

## [04]-[COMPANION_LANES]

Every sub-domain rides the companion engine the branch manifest selects, with the compiled geometry and IFC cores and the copyleft packages isolated at the process boundary.

Runtime lane carries the pure-Python spine owners; the worker lanes carry the compiled enrichment rows and the IFC core behind function-local gates. Probe selection over `find_spec` is governed, never implicit — each probe selects a capability tier, never an offload route, because a process-pool worker shares the one venv and module presence is identical on every floor. Compiled bands cross worker seams as `KernelTrait.HOSTILE` kernels on the warm process pool — no compiled package imports under an isolated subinterpreter — and a live native handle never meets the pickle seam: shapes cross as sealed STEP octets, clouds as the scan `Cloud` array carrier, models as document bytes. AGPL companion band carries no root-manifest row and provisions through the companion-lane owner; the exact lane assignments live on the owning implementation pages.

AGPL Ladybug Tools band — `ladybug-*`, `honeybee-*` with its standards backends, and `dragonfly-*` — rides the `energy/` owners with function-local boundary imports and process-boundary evidence exchange: HBJSON, dfjson, EPW document bytes, and result frames cross the wire, never a distributed link. Simulation engines — Radiance, OpenStudio, and EnergyPlus behind the runtime recipe rail; URBANopt, Modelica, RNM, and REopt behind the district translation rows — are external process-boundary services.
