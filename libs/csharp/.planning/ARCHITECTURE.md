# [CSHARP_BRANCH_ARCHITECTURE]

`libs/csharp` orders the C# packages across the strata under one acyclic, upward-only reference graph: the `Rasm` kernel at the base, the AEC domain and app platform above it, the host boundary at the leaf. Each package's interior is its own architecture's charter; the branch roster, the cross-runtime seams, and the stratum-permission law are the branch grain.

## [01]-[PACKAGE_ROSTER]

```text codemap
libs/csharp/
├── Rasm/              # [KERNEL]        RhinoCommon-aware geometry and numeric kernel
├── Rasm.Element/      # [AEC_DOMAIN]    Lowest AEC element seam onto the one ElementGraph
├── Rasm.Materials/    # [AEC_DOMAIN]    Host-neutral profiles, appearance, and construction
├── Rasm.Bim/          # [AEC_DOMAIN]    Host-neutral BIM object model and IFC/glTF/STEP exchange
├── Rasm.Fabrication/  # [AEC_DOMAIN]    Host-neutral fabrication and detailing
├── Rasm.AppHost/      # [APP_PLATFORM]  Runtime spine and app-platform composition root
├── Rasm.Compute/      # [APP_PLATFORM]  Measured tensor, model, and solver execution
├── Rasm.Persistence/  # [APP_PLATFORM]  Durable element, query, and version stores
├── Rasm.AppUi/        # [APP_PLATFORM]  Avalonia product UI shell
├── Rasm.Rhino/        # [HOST_BOUNDARY] RhinoCommon host APIs; references only Rasm
└── Rasm.Grasshopper/  # [HOST_BOUNDARY] GH2 host APIs; references only Rasm
```

Planning-scoped packages carry a `.planning/` scaffold of index docs and design pages; `Rasm.Element` is the lowest AEC seam the AEC peers and app-platform stores depend up on. `Rasm.Rhino` and `Rasm.Grasshopper` add a folder `.api/` tier over their host assemblies (RhinoCommon + Eto; Grasshopper2 + Eto) and reference only the `Rasm` kernel.

## [02]-[STRATA]

- L1 `Rasm` — references no sibling and carries every stratum above it.
- L2 AEC domain — `Rasm.Element` references only `Rasm` and mints the one `ElementGraph` seam; the peers (`Rasm.Materials`, `Rasm.Bim`, `Rasm.Fabrication`) reference `{Rasm, Rasm.Element}`, never a peer — alignment travels seam contracts and the content-keyed wire.
- L3 app platform — `Rasm.AppHost` references only `Rasm`, a PORT peer decoding Persistence shapes without a downward reference; `Rasm.Persistence` references `{Rasm, Rasm.Element}` and persists the `ElementGraph` as system of record; `Rasm.Compute` reads it one-way; `Rasm.AppUi` references downward only and aligns with peers by contract, never by reference.
- L4 host boundary — `Rasm.Rhino` and `Rasm.Grasshopper` reference only `Rasm` and enter at the host app root; no host-neutral package references them.

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
    accTitle: C# branch package import strata
    accDescr: Four stacked strata from the host boundary through the app platform and the AEC domain onto the kernel — every reference edge downward and solid, labeled edges naming one sourced type, the host boundary skipping straight to the kernel, and one forbidden host-neutral upward edge styled red.
    subgraph L4["L4 HOST BOUNDARY"]
        Rhino[Rasm.Rhino]
        Grasshopper[Rasm.Grasshopper]
    end
    subgraph L3["L3 APP PLATFORM"]
        Persistence[Rasm.Persistence]
        Compute[Rasm.Compute]
        AppHost[Rasm.AppHost]
        AppUi[Rasm.AppUi]
    end
    subgraph L2["L2 AEC DOMAIN"]
        Bim[Rasm.Bim]
        Element[Rasm.Element]
        Materials[Rasm.Materials]
        Fabrication[Rasm.Fabrication]
    end
    subgraph L1["L1 KERNEL"]
        Rasm[Rasm]
    end
    Rhino ~~~ AppHost
    Rhino ~~~ AppUi
    Grasshopper ~~~ Compute
    Grasshopper ~~~ Persistence
    Rhino e1@-->|"[IMPORT]: PerceptualColor"| Rasm
    Grasshopper e2@-->|"[IMPORT]: MonotonicTimeline"| Rasm
    AppHost e3@-->|"[IMPORT]: ContentHash"| Rasm
    Persistence e4@-->|"[IMPORT]: ElementGraph"| Element
    Persistence e5@-->|"[IMPORT]: ContentHash"| Rasm
    Compute e6@-->|"[IMPORT]: ElementGraph"| Element
    Compute e7@-->|"[IMPORT]: ContentHash"| Rasm
    AppUi e8@-->|"[IMPORT]: ContentHash"| Rasm
    Materials e9@-->|"[IMPORT]: IElementProjection"| Element
    Materials e10@-->|"[IMPORT]: Op"| Rasm
    Bim e11@-->|"[IMPORT]: GraphDelta"| Element
    Bim e12@-->|"[IMPORT]: GeometryMeasures"| Rasm
    Fabrication e13@-->|"[IMPORT]: IElementProjection"| Element
    Fabrication e14@-->|"[IMPORT]: MeshSpace"| Rasm
    Element e15@-->|"[IMPORT]: ContentHash"| Rasm
    Rasm f1@-->|"forbidden: host-neutral upward"| L4
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef recessed fill:#21222C,stroke:#6272A4,color:#F8F8F2
    classDef edgeControl stroke:#FF79C6,color:#F8F8F2
    classDef edgeError stroke:#FF5555,stroke-width:3px,color:#F8F8F2
    class Rhino,Grasshopper,AppHost,Compute,Persistence,AppUi,Element,Materials,Bim,Fabrication primary
    class Rasm recessed
    class e1,e2,e3,e4,e5,e6,e7,e8,e9,e10,e11,e12,e13,e14,e15 edgeControl
    class f1 edgeError
```

## [03]-[SEAMS]

Every cross-runtime seam is data-bearing: the peer decodes the content-keyed wire without re-minting. Each edge freezes the single load-bearing contract at its partner grain, spelled verbatim from the owning package page; companion wires fold to the package pages, which enumerate the per-shape bytes. Two fences partition by peer runtime. Graduation crosses one seam: python's `HandoffAxis` names the forward receipt axis, and C# owns the reverse evidence envelope as `GraduationEvidence`, python-spelled `EvidenceBundle`.

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
    accTitle: C# branch python seam registry
    accDescr: C# packages exchanging kinded contract shapes with the python packages — data-bearing kinds on the orange rail, shape contracts on the pink rail — bidirectional peers as hexagons, one-way sources and sinks as stadiums.
    subgraph csharp[LIBS/CSHARP]
        Rasm[Rasm]
        Element[Rasm.Element]
        Bim[Rasm.Bim]
        Materials[Rasm.Materials]
        Fabrication[Rasm.Fabrication]
        AppHost[Rasm.AppHost]
        Compute[Rasm.Compute]
        Persistence[Rasm.Persistence]
    end
    PyRuntime{{python:runtime}}
    PyGeometry{{python:geometry}}
    PyCompute{{python:compute}}
    PyData{{python:data}}
    PyArtifacts([python:artifacts])
    Rasm e1@<-->|"[CONTENT_KEY]: XxHash128"| PyRuntime
    Element e2@<-->|"[WIRE]: GlbContentHash"| PyGeometry
    Element e3@<-->|"[CONTENT_KEY]: ContentAddress"| PyRuntime
    Bim e4@<-->|"[WIRE]: IfcWire"| PyGeometry
    Bim e5@-->|"[WIRE]: GeoWire"| PyData
    PyData e6@-->|"[WIRE]: Environmental"| Materials
    Fabrication e7@-->|"[SHAPE]: Tolerance"| PyArtifacts
    AppHost e8@<-->|"[WIRE]: DiscoveryResult"| PyRuntime
    Compute e9@<-->|"[WIRE]: ComputeService"| PyGeometry
    Compute e10@<-->|"[WIRE]: ProtoVocabulary"| PyRuntime
    Compute e11@<-->|"[GRADUATION]: GraduationEvidence"| PyCompute
    PyData e12@-->|"[SHAPE]: DoeDataset"| Compute
    Persistence e13@<-->|"[WIRE]: OpLogEntry"| PyRuntime
    PyArtifacts e14@-->|"[CONTENT_KEY]: SignedArtifact"| Persistence
    Persistence e15@<-->|"[WIRE]: SubstraitPlan"| PyData
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef external fill:#8BE9FDBF,stroke:#8BE9FD,color:#282A36
    classDef recessed fill:#21222C,stroke:#6272A4,color:#F8F8F2
    classDef edgeData stroke:#FFB86C,color:#F8F8F2
    classDef edgeControl stroke:#FF79C6,color:#F8F8F2
    class Rasm,Element,Bim,Materials,Fabrication,AppHost,Compute,Persistence primary
    class PyRuntime,PyGeometry,PyCompute,PyData external
    class PyArtifacts recessed
    class e1,e2,e3,e4,e5,e6,e8,e9,e10,e11,e13,e14,e15 edgeData
    class e7,e12 edgeControl
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
    accTitle: C# branch typescript seam registry
    accDescr: C# packages producing kinded wires the typescript core, runtime, and ui domains decode — data-bearing kinds on the orange rail, telemetry transport on the cyan rail — bidirectional peers as hexagons, one-way sinks as stadiums.
    subgraph csharp[LIBS/CSHARP]
        Rasm[Rasm]
        Element[Rasm.Element]
        Compute[Rasm.Compute]
        Persistence[Rasm.Persistence]
        Bim[Rasm.Bim]
        Materials[Rasm.Materials]
        AppUi[Rasm.AppUi]
        AppHost[Rasm.AppHost]
    end
    TsCore{{typescript:core}}
    TsUi([typescript:ui])
    TsRuntime([typescript:runtime])
    Rasm e1@<-->|"[CONTENT_KEY]: XxHash128"| TsCore
    Element e2@<-->|"[WIRE]: rasm.element.v1"| TsCore
    Compute e3@<-->|"[WIRE]: QuantityFamily"| TsCore
    Persistence e4@-->|"[WIRE]: CrdtOpWire"| TsCore
    Bim e5@-->|"[WIRE]: IfcWire"| TsCore
    Materials e6@-->|"[WIRE]: MaterialWire"| TsCore
    AppUi e7@-->|"[WIRE]: CommandPayloadWire"| TsCore
    AppHost e8@-->|"[WIRE]: ReceiptEnvelopeWire"| TsCore
    Bim e12@-->|"[WIRE]: GlobalIdSet"| TsUi
    Materials e13@-->|"[WIRE]: OpenPbrGroupsWire"| TsUi
    AppUi e11@-->|"[WIRE]: ControlIntentWire"| TsUi
    AppHost e10@-->|"[WIRE]: BindingStatusWire"| TsUi
    AppHost e9@-->|"[TRANSPORT]: OtelExport"| TsRuntime
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef external fill:#8BE9FDBF,stroke:#8BE9FD,color:#282A36
    classDef recessed fill:#21222C,stroke:#6272A4,color:#F8F8F2
    classDef edgeData stroke:#FFB86C,color:#F8F8F2
    classDef edgeExternal stroke:#8BE9FD,color:#F8F8F2
    class Rasm,Element,Bim,Materials,AppHost,Compute,Persistence,AppUi primary
    class TsCore external
    class TsRuntime,TsUi recessed
    class e1,e2,e3,e4,e5,e6,e7,e8,e10,e11,e12,e13 edgeData
    class e9 edgeExternal
```

Owning package pages enumerate the per-shape bytes; each diagram edge is the single load-bearing contract at its partner grain.

## [04]-[ADMISSION_POLICY]

Root `Directory.Packages.props` owns NuGet package admission and central version pins; per-package `.csproj` manifests stay label-grouped by owner and carry no versions. Host assemblies (RhinoCommon, Grasshopper2, Eto) enter only through the host-boundary packages' folder `.api/` tiers; no host-neutral package names a host assembly.
