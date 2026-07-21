# [CSHARP_BRANCH_ARCHITECTURE]

`libs/csharp` orders the C# packages across the strata under one acyclic, upward-only reference graph: the `Rasm` kernel at the base, the AEC domain and app platform above it, the host boundary at the leaf. Each package's interior is its own architecture's charter; the branch roster, the cross-runtime seams, the cross-package flow spines, and the stratum-permission law are the branch grain.

## [01]-[DOMAIN_MAP]

```text codemap
libs/csharp/
├── Rasm/              # [KERNEL]         RhinoCommon-aware geometry and numeric kernel
├── Rasm.Element/      # [AEC_DOMAIN]     Lowest AEC element seam onto the one ElementGraph
├── Rasm.Materials/    # [AEC_DOMAIN]     Host-neutral profiles, appearance, and construction
├── Rasm.Bim/          # [AEC_DOMAIN]     Host-neutral BIM object model and IFC/glTF/STEP exchange
├── Rasm.Fabrication/  # [AEC_DOMAIN]     Host-neutral fabrication and detailing
├── Rasm.AppHost/      # [APP_PLATFORM]   Runtime spine and app-platform composition root
├── Rasm.Compute/      # [APP_PLATFORM]   Measured tensor, model, and solver execution
├── Rasm.Persistence/  # [APP_PLATFORM]   Durable element, query, and version stores
├── Rasm.AppUi/        # [APP_PLATFORM]   Avalonia product UI shell
├── Rasm.Rhino/        # [HOST_BOUNDARY]  RhinoCommon host APIs; references only Rasm
└── Rasm.Grasshopper/  # [HOST_BOUNDARY]  GH2 host APIs; references only Rasm
```

Planning-scoped packages carry a `.planning/` scaffold of index docs and design pages; `Rasm.Rhino` and `Rasm.Grasshopper` add a folder `.api/` tier over their host assemblies (RhinoCommon + Eto; Grasshopper2 + Eto).

## [02]-[STRATA]

- L1 `Rasm` — references no sibling and carries every stratum above it.
- Shared-machinery homing: a mechanism serving multiple packages homes at the LOWEST stratum every consumer references; a shared owner homed above a consumer's reach manufactures per-folder twins and is the named strata defect.
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

## [04]-[INTERNAL]

`Rasm.Element`, `Rasm.Materials`, and `Rasm.Bim` meet at one seam — the `ElementGraph`: Element owns what a thing IS, Materials what a thing is MADE OF, Bim what a thing MEANS in IFC. Materials seeds and projects components onto the graph, Bim lowers foreign IFC onto it and re-authors IFC off it, and every cross-package fact travels as graph content — neither projector reaches into the other.

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: AEC triad projection spine
    accDescr: Materials and Bim projecting onto the one ElementGraph seam, Bim exchanging IFC files, and the app-platform packages consuming the graph by receipt and content key.
    Materials[Rasm.Materials]
    Bim[Rasm.Bim]
    Graph[(ElementGraph)]
    Ifc([IFC file])
    Compute[Rasm.Compute]
    Persistence[Rasm.Persistence]
    Materials e1@-->|"ComponentProjector"| Graph
    Bim e2@-->|"SemanticProjector.Project"| Graph
    Graph e3@-->|"SemanticProjector.Emit"| Bim
    Bim e4@<-->|"DatabaseIfc"| Ifc
    Graph e5@-->|"baked receipts"| Compute
    Graph e6@-->|"content keys"| Persistence
```

Two projection surfaces, both declared in `Rasm.Element`, are the only cross-package contracts: `IElementProjection` (Materials' `ComponentProjector`, Bim's `SemanticProjector`) and `IGraphConstraint` (Bim's `IfcLegality`, rejecting an illegal delta at composition time). Owners mint their own identity at their own seam — Materials the deterministic Type node, Bim the per-ingest rooted id — and nothing re-mints a peer's.

Materials carries IFC names only as neutral `IfcBinding` row data; Bim never re-derives section geometry or material data; Element never carries a fact only one projector understands. A consumer that needs the thing reads the graph; a consumer that needs the IFC meaning reads Bim's projection; nothing reads across. A canonical seam surface changes only through an explicit brief entry naming the owner and the migration.

Signal crosses the strata on one fabric: the OTel-free signal capsule is kernel L1 vocabulary every stratum composes as instances, per-folder fact unions are the only legitimate per-folder signal types, and the app platform alone laces OTel, correlation, tenancy, and host evidence over the composed surface — telemetry leaves the branch opaque on the `[TRANSPORT]` seam.

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: C# branch signal fabric spine
    accDescr: The kernel signal capsule composing as its own kernel rail and as per-folder fact-union instances, both emitting signal facts into the AppHost governance lacing, and the laced series leaving on the OtelExport transport seam.
    Capsule[Rasm · signal capsule]
    KernelRail[Rasm · SignalRail]
    Folders[folder fact unions · composed instances]
    Governance[Rasm.AppHost · SignalGovernance]
    Egress([OtelExport transport])
    Capsule e1@-->|"compose: SignalFact instance"| KernelRail
    Capsule e2@-->|"compose: fact-union instance"| Folders
    KernelRail e3@-->|"emit: signal facts"| Governance
    Folders e4@-->|"emit: signal facts"| Governance
    Governance e5@-->|"lace: correlation + tenancy + host evidence"| Egress
```

Exact per-stage wiring lives on the owning implementation pages.

## [05]-[ROUTING]

Every extension lands on a canonical owner — a row where possible, a compiler-forced arm on the one dispatch site otherwise. Each owner's page carries the full growth law; this table routes and never restates it.

| [INDEX] | [CHANGE]                    | [OWNER_SURFACE]                          | [SHAPE_OF_THE_EDIT]                         |
| :-----: | :-------------------------- | :--------------------------------------- | :------------------------------------------ |
|  [01]   | new component family        | `ComponentFamily` + one seed page        | one policy row + seed row table             |
|  [02]   | new section shape           | `SectionProfile` + `SectionSolver.Solve` | one union arm + one dispatch arm            |
|  [03]   | new IFC entity or category  | emitter + `ClassIntroductions`           | regenerate + one overlay row                |
|  [04]   | new property or detail      | `DetailSchema`                           | one schema row                              |
|  [05]   | new relation semantics      | sub-kind rows or `Generic` attributes    | one row or attribute convention             |
|  [06]   | new quantity or dimension   | `QuantityRow`, `Dimension`               | one mint row or member                      |
|  [07]   | new fault or band           | owning `*Fault` union + `FaultBand`      | one union case or one registry row          |
|  [08]   | new seam participant        | `IElementProjection` + `FaultBand`       | one projector + one band row                |
|  [09]   | new folder signal surface   | the folder's composed capsule instance   | one fact case, point row, or instrument row |
|  [10]   | new capsule mechanism       | kernel signal capsule (`Rasm`)           | one member on the one mechanism             |
|  [11]   | new OTel wiring or exporter | `Rasm.AppHost` `SignalGovernance`        | one governance row; lacing stays L3         |

## [06]-[ADMISSION_POLICY]

Root `Directory.Packages.props` owns NuGet package admission and central version pins; per-package `.csproj` manifests stay label-grouped by owner and carry no versions. Host assemblies (RhinoCommon, Grasshopper2, Eto) enter only through the host-boundary packages' folder `.api/` tiers; no host-neutral package names a host assembly.
