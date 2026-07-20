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
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart TB
    accTitle: C# branch package import strata
    accDescr: Four stacked strata from the host boundary through the app platform and the AEC domain onto the kernel — every reference edge downward and solid, labeled edges naming one sourced type, the host boundary skipping straight to the kernel, and one forbidden host-neutral upward edge.
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
    Rhino -->|"[IMPORT]: PerceptualColor"| Rasm
    Grasshopper -->|"[IMPORT]: MonotonicTimeline"| Rasm
    AppHost -->|"[IMPORT]: ContentHash"| Rasm
    Persistence -->|"[IMPORT]: ElementGraph"| Element
    Persistence -->|"[IMPORT]: ContentHash"| Rasm
    Compute -->|"[IMPORT]: ElementGraph"| Element
    Compute -->|"[IMPORT]: ContentHash"| Rasm
    AppUi -->|"[IMPORT]: ContentHash"| Rasm
    Materials -->|"[IMPORT]: IElementProjection"| Element
    Materials -->|"[IMPORT]: Op"| Rasm
    Bim -->|"[IMPORT]: GraphDelta"| Element
    Bim -->|"[IMPORT]: GeometryMeasures"| Rasm
    Fabrication -->|"[IMPORT]: IElementProjection"| Element
    Fabrication -->|"[IMPORT]: MeshSpace"| Rasm
    Element -->|"[IMPORT]: ContentHash"| Rasm
    Rasm -->|"forbidden: host-neutral upward"| L4
```

## [03]-[SEAMS]

Every cross-runtime seam is data-bearing: the peer decodes the content-keyed wire without re-minting. Each edge freezes the single load-bearing contract at its partner grain, spelled verbatim from the owning package page; companion wires fold to the package pages, which enumerate the per-shape bytes. Two fences partition by peer runtime. Graduation crosses one seam: python's `HandoffAxis` names the forward receipt axis, and C# owns the reverse evidence envelope as `GraduationEvidence`, python-spelled `EvidenceBundle`.

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: C# branch python seam registry
    accDescr: C# packages exchanging kinded contract shapes with the python packages — data-bearing kinds and shape contracts as labeled edges — bidirectional peers as hexagons, one-way sources and sinks as stadiums.
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
    Rasm <-->|"[CONTENT_KEY]: XxHash128"| PyRuntime
    Element <-->|"[WIRE]: GlbContentHash"| PyGeometry
    Element <-->|"[CONTENT_KEY]: ContentAddress"| PyRuntime
    Bim <-->|"[WIRE]: IfcWire"| PyGeometry
    Bim -->|"[WIRE]: GeoWire"| PyData
    PyData -->|"[WIRE]: Environmental"| Materials
    Fabrication -->|"[SHAPE]: Tolerance"| PyArtifacts
    AppHost <-->|"[WIRE]: DiscoveryResult"| PyRuntime
    Compute <-->|"[WIRE]: ComputeService"| PyGeometry
    Compute <-->|"[WIRE]: ProtoVocabulary"| PyRuntime
    Compute <-->|"[GRADUATION]: GraduationEvidence"| PyCompute
    PyData -->|"[SHAPE]: DoeDataset"| Compute
    Persistence <-->|"[WIRE]: OpLogEntry"| PyRuntime
    PyArtifacts -->|"[CONTENT_KEY]: SignedArtifact"| Persistence
    Persistence <-->|"[WIRE]: SubstraitPlan"| PyData
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
    accTitle: C# branch typescript seam registry
    accDescr: C# packages producing kinded wires the typescript core, runtime, and ui domains decode — data-bearing wires and the telemetry transport edge, labeled per kind — bidirectional peers as hexagons, one-way sinks as stadiums.
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
    Rasm <-->|"[CONTENT_KEY]: XxHash128"| TsCore
    Element <-->|"[WIRE]: rasm.element.v1"| TsCore
    Compute <-->|"[WIRE]: QuantityFamily"| TsCore
    Persistence -->|"[WIRE]: CrdtOpWire"| TsCore
    Bim -->|"[WIRE]: IfcWire"| TsCore
    Materials -->|"[WIRE]: MaterialWire"| TsCore
    AppUi -->|"[WIRE]: CommandPayloadWire"| TsCore
    AppHost -->|"[WIRE]: ReceiptEnvelopeWire"| TsCore
    Bim -->|"[WIRE]: GlobalIdSet"| TsUi
    Materials -->|"[WIRE]: OpenPbrGroupsWire"| TsUi
    AppUi -->|"[WIRE]: ControlIntentWire"| TsUi
    AppHost -->|"[WIRE]: BindingStatusWire"| TsUi
    AppHost -->|"[TRANSPORT]: OtelExport"| TsRuntime
```

Owning package pages enumerate the per-shape bytes; each diagram edge is the single load-bearing contract at its partner grain.

## [04]-[ADMISSION_POLICY]

Root `Directory.Packages.props` owns NuGet package admission and central version pins; per-package `.csproj` manifests stay label-grouped by owner and carry no versions. Host assemblies (RhinoCommon, Grasshopper2, Eto) enter only through the host-boundary packages' folder `.api/` tiers; no host-neutral package names a host assembly.
