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

Planning-scoped packages carry a `.planning/` scaffold of four index docs and design pages; `Rasm.Element` is the lowest AEC seam the AEC peers and app-platform stores depend up on. `Rasm.Rhino` and `Rasm.Grasshopper` add a folder `.api/` tier over their host assemblies (RhinoCommon + Eto; Grasshopper2 + Eto) and reference only the `Rasm` kernel.

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
    accTitle: C# branch cross-runtime seam registry
    accDescr: C# branch packages exchanging content-keyed wires, tessellation, and graduation evidence with the TypeScript core and the Python geometry, runtime, and compute peers, edge rails colored by kind and nodes classed by seam direction.
    subgraph csharp[LIBS/CSHARP]
        Rasm[Rasm]
        Element[Rasm.Element]
        Bim[Rasm.Bim]
        AppHost[Rasm.AppHost]
        Compute[Rasm.Compute]
        Persistence[Rasm.Persistence]
    end
    Core{{typescript:core}}
    Runtime{{python:runtime}}
    Geometry{{python:geometry}}
    PyCompute([python:compute])
    AppHost e1@-->|"[WIRE]: ReceiptEnvelope"| Core
    Compute e2@-->|"[WIRE]: FileDescriptorSet"| Core
    Persistence e3@-->|"[WIRE]: OpLogWire"| Core
    Rasm e4@<-->|"[CONTENT_KEY]: XxHash128"| Runtime
    Element e5@<-->|"[WIRE]: ElementGraph"| Geometry
    Element e6@<-->|"[WIRE]: ElementGraph"| Core
    Bim e7@<-->|"[TESSELLATION]: TessellationRequest"| Geometry
    AppHost e8@<-->|"[WIRE]: DiscoveryResult"| Runtime
    PyCompute e9@-->|"[GRADUATION]: HandoffAxis"| Compute
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef external fill:#8BE9FDBF,stroke:#8BE9FD,color:#282A36
    classDef annotation fill:#21222C,stroke:#6272A4,color:#F8F8F2
    classDef edgeData stroke:#FFB86C,color:#F8F8F2
    class Rasm,Element,Bim,AppHost,Compute,Persistence primary
    class Core,Runtime,Geometry external
    class PyCompute annotation
    class e1,e2,e3,e4,e5,e6,e7,e8,e9 edgeData
```

Every cross-runtime seam is data-bearing: the peer decodes the content-keyed wire without re-minting. Owning package pages enumerate the per-shape bytes; each diagram edge is the single load-bearing contract at its partner grain.

## [03]-[DEPENDENCY_DIRECTION]

Dependency is strictly upward through the strata, acyclic, with the kernel at the base, `Rasm.AppHost` the app-platform composition root, and the host shells at the leaf. Downward references are legal including skips; no host-neutral package references a host-boundary package.

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
    accTitle: C# branch package stratum law
    accDescr: Four stacked strata from the host boundary through the app platform and the AEC domain onto the kernel, downward-only reference edges, the host-boundary skip straight to the kernel, and the forbidden host-neutral upward edge styled red.
    subgraph L4[HOST BOUNDARY]
        Host[Rasm.Rhino]
    end
    subgraph L3[APP PLATFORM]
        Platform[Rasm.AppHost]
    end
    subgraph L2[AEC DOMAIN]
        Domain[Rasm.Element]
    end
    subgraph L1[KERNEL]
        Kernel[Rasm]
    end
    Platform --> Domain
    Domain --> Kernel
    Host -.->|"legal skip: host to kernel"| Kernel
    Kernel -->|"forbidden: host-neutral upward"| L4
    linkStyle 2 stroke:#6272A4,color:#F8F8F2,stroke-width:1.5px,stroke-dasharray:4 6
    linkStyle 3 stroke:#FF5555,stroke-width:3px,color:#F8F8F2
    classDef stratumHost fill:#8BE9FDBF,stroke:#8BE9FD,color:#282A36
    classDef stratumPlatform fill:#FF79C680,stroke:#FF79C6,color:#F8F8F2
    classDef stratumDomain fill:#FFB86CBF,stroke:#FFB86C,color:#282A36
    classDef boundary fill:#282A36,stroke:#BD93F9,color:#F8F8F2
    class Host stratumHost
    class Platform stratumPlatform
    class Domain stratumDomain
    class Kernel boundary
```

- KERNEL: `Rasm` references no sibling and carries every stratum above it.
- AEC seam: `Rasm.Element` references only `Rasm`, names no IFC or provider package; the AEC peers and app-platform stores reference it.
- AEC peers: each references `{Rasm, Rasm.Element}`, never a peer, never upward; alignment travels seam contracts and the content-keyed wire.
- APP-PLATFORM root: `Rasm.AppHost` references only `Rasm`; a PORT peer decoding Persistence shapes without a downward `Rasm.Persistence` reference.
- APP-PLATFORM stores: `Rasm.Persistence` references `{Rasm, Rasm.Element}` and persists the `ElementGraph`; `Rasm.Compute` reads it one-way.
- APP-PLATFORM leaves: `Rasm.Compute` and `Rasm.AppUi` reference downward only; `Rasm.AppUi` stays pure-UI, no AEC-domain or host-boundary reference.
- HOST-BOUNDARY: `Rasm.Rhino` and `Rasm.Grasshopper` reference only `Rasm` and enter at the host app root; no host-neutral package references them.
