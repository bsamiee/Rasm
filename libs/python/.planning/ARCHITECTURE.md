# [PYTHON_BRANCH_ARCHITECTURE]

`libs/python` is the host-free science, compute, data, geometry, and IFC companion. `runtime` mints the shared value shapes; `compute`, `data`, `geometry`, and `artifacts` compose them at their boundary.

## [01]-[DOMAIN_MAP]

```text codemap
libs/python/
├── runtime/    # Host-free execution foundation four siblings compose
├── compute/    # Offline scientific evidence that graduates through one rail
├── data/       # Portable data interchange: tabular, spatial, gridded, graph
├── geometry/   # Host-free geometry + IFC/BIM companion and cross-boundary owner
└── artifacts/  # Self-contained artifact-production utility under one ArtifactReceipt
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
    accTitle: Python branch seam registry
    accDescr: Python branch package owners exchanging content keys, wires, shapes, and graduation evidence with the C# kernel, app host, persistence, compute, and Bim packages, edge rails colored by kind and nodes classed by seam direction.
    subgraph python[LIBS/PYTHON]
        Runtime[runtime]
        Compute[compute]
        Data[data]
        Geometry[geometry]
        Artifacts[artifacts]
    end
    Rasm{{Rasm}}
    AppHost{{Rasm.AppHost}}
    RasmCompute{{Rasm.Compute}}
    Bim{{Rasm.Bim}}
    Persistence[(Rasm.Persistence)]
    Runtime e1@<-->|"[CONTENT_KEY]: XxHash128"| Rasm
    Runtime e2@<-->|"[WIRE]: CommandReceipt"| AppHost
    Runtime e3@<-->|"[WIRE]: OpLogEntry"| Persistence
    Data e4@<-->|"[WIRE]: SubstraitPlan"| Persistence
    Data e5@-->|"[SHAPE]: DoeDataset"| RasmCompute
    Compute e6@-->|"[GRADUATION]: HandoffAxis"| RasmCompute
    Geometry e7@<-->|"[WIRE]: IfcWire"| Bim
    Geometry e8@<-->|"[WIRE]: ArtifactSync"| RasmCompute
    Artifacts e9@-->|"[CONTENT_KEY]: SignedArtifact"| Persistence
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef external fill:#8BE9FDBF,stroke:#8BE9FD,color:#282A36
    classDef data fill:#FFB86CBF,stroke:#FFB86C,color:#282A36
    classDef edgeData stroke:#FFB86C,color:#F8F8F2
    classDef edgeControl stroke:#FF79C6,color:#F8F8F2
    class Runtime,Compute,Data,Geometry,Artifacts primary
    class Rasm,AppHost,RasmCompute,Bim external
    class Persistence data
    class e1,e2,e3,e4,e6,e7,e8,e9 edgeData
    class e5 edgeControl
```

## [03]-[DEPENDENCY_DIRECTION]

`runtime` is the foundation and references no sibling; `compute`, `data`, `geometry`, and `artifacts` compose its owners at their boundary as settled vocabulary and mint no second copy. No package imports another package's interior.

Two named boundary compositions cross between consumers. `compute` composes the `data` DOE-frame admission arm (`FrameAdmission`/`FrameInterop`) as a study input — labelled arrays are `xarray` carriers `compute`'s array owner admits from any producer, owing no data seam. `compute` accepts `geometry` evidence through the graduation `HandoffAxis` case, crossing only on that rail. Each composes a published shape at the boundary; every other cross-folder fact rides a folder task.

Python couples to C# only at the wire. Seam registries record the package-level aggregate; file-level detail lives on the owning folder's design page and the cross-`libs/` ledger.

## [04]-[ADMISSION_POLICY]

One root manifest owns interpreter admission, dependency groups, version bounds, and `python_version` markers. This branch targets a normal-GIL CPython core; worker-lane exceptions stay in the root manifest until resolver evidence permits removal. Installation rationale stays in the manifest; package-local docs name capability, entrypoints, boundaries, and exclusions.

`protobuf` and `grpcio` are core runtime dependencies. `grpcio-tools` is codegen-only. Native rendering and OCCT/STEP concerns stay on their owning geometry/artifacts tasks and root-manifest admissions. `specklepy` is not a branch dependency.
