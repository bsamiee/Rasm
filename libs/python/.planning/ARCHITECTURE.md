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

## [02]-[STRATA]

- S0 `runtime` — mints every shared rail exactly once (`faults`/`resilience`, `identity`, `receipts`, `lanes`/`workers`, `roots`, `wire`/`serve`) and imports no sibling; worker, lane, retry, content-key, and receipt logic root here and nowhere else.
- S1 `data` — composes runtime alone and publishes the surfaces its upper strata import: the tabular contract (`FrameAdmission`/`FrameInterop`) and the columnar `arrow_bytes` projection; the mesh and point-record shapes (`MeshPayload`, `PointRecordTable`) cross to geometry as seam payloads, never imports.
- S2 `compute` + `geometry` — peers composing runtime plus data (compute imports `FrameAdmission`/`FrameInterop`, geometry imports `arrow_bytes`); no import crosses between them — geometry evidence enters compute as `GeometryHandoff` wire, and compute's graduation hub (`HandoffAxis`) is the one egress all branch evidence crosses.
- S3 `artifacts` — composes runtime plus compute's graduation handoff (`GraduationReceipt`); geometry scene facts cross as glb bytes admitted through `SceneGrid.of_glb` into `MeshScene`, and no package imports another's interior — cross-package coupling is a published boundary import or a content-keyed wire.

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
    accTitle: Python branch import strata
    accDescr: Four import strata — artifacts over the compute-geometry peer pair over data over the runtime foundation — every package importing runtime directly, each labeled import naming its one sourced surface, geometry evidence crossing into compute as dashed wire data rather than import, one forbidden upward edge styled red, and runtime's interior rails descending from wire/serve through lanes/workers, joined by roots, onto resilience, receipts, identity, and faults.
    subgraph S3["S3 ARTIFACTS"]
        Artifacts[artifacts]
    end
    subgraph S2["S2 COMPUTE + GEOMETRY"]
        Compute[compute]
        Geometry[geometry]
    end
    subgraph S1["S1 DATA"]
        Data[data]
    end
    subgraph S0["S0 RUNTIME"]
        Wire["wire / serve"]
        Lanes["lanes / workers"]
        Roots[roots]
        Resilience[resilience]
        Receipts[receipts]
        Identity[identity]
        Faults[faults]
    end
    Artifacts e1@-->|"[IMPORT]: GraduationReceipt"| Compute
    Compute e2@-->|"[IMPORT]: FrameAdmission"| Data
    Geometry e3@-->|"[IMPORT]: arrow_bytes"| Data
    Geometry e4@-.->|"[WIRE]: GeometryHandoff"| Compute
    Data e5@--> S0
    Geometry e6@--> S0
    Compute e7@--> S0
    Artifacts e8@--> S0
    Artifacts ~~~ Compute
    S0 e9@-->|"forbidden: upward import"| S3
    Wire r1@--> Lanes
    Lanes r2@--> Resilience
    Roots r3@--> Resilience
    Resilience r4@--> Receipts
    Receipts r5@--> Identity
    Identity r6@--> Faults
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef recessed fill:#21222C,stroke:#6272A4,color:#F8F8F2
    classDef edgeControl stroke:#FF79C6,color:#F8F8F2
    classDef edgeData stroke:#FFB86C,color:#F8F8F2
    classDef edgeError stroke:#FF5555,stroke-width:3px,color:#F8F8F2
    class Artifacts,Compute,Geometry,Data primary
    class Wire,Lanes,Roots,Resilience,Receipts,Identity,Faults recessed
    class e1,e2,e3,e5,e6,e7,e8,r1,r2,r3,r4,r5,r6 edgeControl
    class e4 edgeData
    class e9 edgeError
```

## [03]-[SEAMS]

Python couples to C# only at the wire — content-keyed shapes cross serialized, never as imported code. Each edge freezes one {KIND, name, direction} representative at the owner's verbatim spelling; companion legs fold to prose — runtime↔Rasm.AppHost also carries `TraceContext` and `HlcStamp`, runtime↔Rasm.Compute an `XxHash128` leg, runtime↔Rasm.Element's `ContentAddress` spells from the Element owner with the runtime `ContentKey` minting beneath it, and the graduation seam's reverse payload is `EvidenceBundle`, C#-owned as `GraduationEvidence`. This registry records the package-level aggregate; file-level detail lives on the owning folder's design page and the cross-`libs/` ledger.

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
    accTitle: Python branch C# seam registry
    accDescr: Python packages exchanging kinded contract shapes with their C# counterparts — data-bearing kinds on the orange rail, shape contracts on the pink rail — persistence as the one data store, bidirectional peers as hexagons, one-way sources and sinks as stadiums.
    subgraph python[LIBS/PYTHON]
        Geometry[geometry]
        Runtime[runtime]
        Compute[compute]
        Data[data]
        Artifacts[artifacts]
    end
    Bim{{Rasm.Bim}}
    Element{{Rasm.Element}}
    Rasm{{Rasm}}
    AppHost{{Rasm.AppHost}}
    RasmCompute{{Rasm.Compute}}
    Persistence[(Rasm.Persistence)]
    Materials([Rasm.Materials])
    Fabrication([Rasm.Fabrication])
    Geometry e12@<-->|"[WIRE]: IfcWire"| Bim
    Geometry e13@<-->|"[WIRE]: GlbContentHash"| Element
    Geometry e11@<-->|"[WIRE]: ComputeService"| RasmCompute
    Runtime e5@<-->|"[CONTENT_KEY]: ContentAddress"| Element
    Runtime e1@<-->|"[CONTENT_KEY]: XxHash128"| Rasm
    Runtime e2@<-->|"[WIRE]: DiscoveryResult"| AppHost
    Runtime e4@<-->|"[WIRE]: ProtoVocabulary"| RasmCompute
    Runtime e3@<-->|"[WIRE]: OpLogEntry"| Persistence
    Compute e10@<-->|"[GRADUATION]: HandoffAxis"| RasmCompute
    Data e7@-->|"[SHAPE]: DoeDataset"| RasmCompute
    Data e6@<-->|"[WIRE]: SubstraitPlan"| Persistence
    Data e8@-->|"[WIRE]: Environmental"| Materials
    Bim e9@-->|"[WIRE]: GeoFeatureWkb"| Data
    Artifacts e14@-->|"[CONTENT_KEY]: SignedArtifact"| Persistence
    Fabrication e15@-->|"[SHAPE]: Tolerance"| Artifacts
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef external fill:#8BE9FDBF,stroke:#8BE9FD,color:#282A36
    classDef data fill:#FFB86CBF,stroke:#FFB86C,color:#282A36
    classDef recessed fill:#21222C,stroke:#6272A4,color:#F8F8F2
    classDef edgeData stroke:#FFB86C,color:#F8F8F2
    classDef edgeControl stroke:#FF79C6,color:#F8F8F2
    class Runtime,Data,Compute,Geometry,Artifacts primary
    class Rasm,AppHost,RasmCompute,Element,Bim external
    class Persistence data
    class Materials,Fabrication recessed
    class e1,e2,e3,e4,e5,e6,e8,e9,e10,e11,e12,e13,e14 edgeData
    class e7,e15 edgeControl
```

## [04]-[ADMISSION_POLICY]

One root manifest owns interpreter admission, dependency groups, version bounds, and `python_version` markers. This branch targets a normal-GIL CPython core; worker-lane exceptions stay in the root manifest until resolver evidence permits removal. Installation rationale stays in the manifest; package-local docs name capability, entrypoints, boundaries, and exclusions.

`protobuf` and `grpcio` are core runtime dependencies. `grpcio-tools` is codegen-only. Native rendering and OCCT/STEP concerns stay on their owning geometry/artifacts tasks and root-manifest admissions. `specklepy` is not a branch dependency.
