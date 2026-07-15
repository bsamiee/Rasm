# [RASM_ELEMENT_ARCHITECTURE]

Domain map of `Rasm.Element` — the lowest AEC-DOMAIN seam between the `Rasm` kernel and the AEC peers `{Rasm.Materials, Rasm.Bim, Rasm.Fabrication}`. Each sub-domain folder maps to exactly one FOLDER-TRUE namespace — a file at `<SubFolder>/<File>.cs` declares `namespace Rasm.Element.<SubFolder>;` per `dotnet_style_namespace_match_folder`, cross-subfolder consumption riding explicit `using Rasm.Element.<SubFolder>;` rows — every sub-domain composes the one `ElementGraph` and lowers onto the one `ElementFault` band, and the peers depend up on the `IElementProjection`/`IGraphConstraint` contracts, aligning by the content-keyed graph rather than by referencing each other.

## [01]-[DOMAIN_MAP]

```text codemap
Rasm.Element/             # refs ../Rasm ONLY; no GeometryGym; no host geometry (geometry by content hash)
├── Graph/                # Authoritative property graph and its mutation algebra
│   ├── Element.cs        # Property-graph spine
│   ├── Delta.cs          # Mutation algebra
│   ├── Wire.cs           # Content-key-preserving crossing
│   └── element.proto     # Language-neutral rasm.element.v1 oneof contract
├── Relations/            # Neutral objectified-edge algebra
│   └── Relation.cs       # Neutral edge algebra
├── Classification/       # Neutral cross-cutting axes
│   └── Classification.cs # Cross-cutting axes
├── Properties/           # Typed property/quantity value vocabulary
│   ├── Property.cs       # Typed value vocabulary
│   └── Quantity.cs       # Physical-quantity carrier
├── Composition/          # Material composition and intrinsic acoustic folds
│   ├── Material.cs       # Material composition
│   └── Acoustic.cs       # Intrinsic acoustic folds
├── Assessment/           # Generic analysis receipt
│   └── Assessment.cs     # Generic analysis receipt
├── Geospatial/           # Georeferenced coverage and CRS
│   ├── Coverage.cs       # Georeferenced coverage
│   └── Reference.cs      # Map-conversion-and-CRS record
└── Projection/           # Cross-stratum contracts, the content codec, and the fault band
    ├── Projection.cs     # Cross-stratum contracts
    ├── Address.cs        # Content codec
    └── Fault.cs          # Fault band
```

`Graph` is the spine every other sub-domain feeds: each owns a `Node` case payload or a cross-cutting value the one `ElementGraph` composes, and the `Graph/Element` `Bake` applies both the type→occurrence inheritance and the `Properties/Property` `InheritanceMode` bag merge. Seam identity re-mints nothing the kernel owns — the content-identity seed, the op-key, and the fault base are the kernel `XxHash128` seed-zero entry, `Op`, and `Expected`. Per-page declarations, the shared `Projection/Address` codec fan-in, and the inheritance merge rules live on the owning implementation pages.

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
    accTitle: Element AEC-domain projection seams
    accDescr: Element sub-domain owners exchanging projections and neutral shapes with the AEC peers Bim, Materials, and Fabrication, edge rails colored by kind and nodes classed by seam direction.
    subgraph element[RASM.ELEMENT]
        Graph[Graph spine]
        Projection[Projection contracts]
        Composition[Composition folds]
        Properties[Property vocabulary]
        Geospatial[Geospatial coverage]
    end
    Bim{{Rasm.Bim}}
    Materials{{Rasm.Materials}}
    Fabrication([Rasm.Fabrication])
    Bim e1@-->|"[PROJECTION]: GraphDelta"| Graph
    Materials e2@-->|"[PROJECTION]: GraphDelta"| Graph
    Bim e3@-->|"[PORT]: IGraphConstraint"| Projection
    Fabrication e4@-->|"[PROJECTION]: FabricationProjector"| Projection
    Projection e5@-->|"[SHAPE]: IElementProjection"| Materials
    Composition e6@<-->|"[PROJECTION]: MaterialComposition"| Bim
    Composition e7@<-->|"[SHAPE]: ProfileRef"| Materials
    Properties e8@<-->|"[SHAPE]: DetailSchema"| Bim
    Properties e9@<-->|"[SHAPE]: DetailSchema"| Materials
    Bim e10@-->|"[PROJECTION]: GeoReference"| Geospatial
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef external fill:#8BE9FDBF,stroke:#8BE9FD,color:#282A36
    classDef annotation fill:#21222C,stroke:#6272A4,color:#F8F8F2
    classDef edgeExternal stroke:#8BE9FD,color:#F8F8F2
    classDef edgeControl stroke:#FF79C6,color:#F8F8F2
    class Graph,Projection,Composition,Properties,Geospatial primary
    class Bim,Materials external
    class Fabrication annotation
    class e1,e2,e4,e6,e10 edgeExternal
    class e3,e5,e7,e8,e9 edgeControl
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
    accTitle: Element platform and cross-runtime wire seams
    accDescr: Element sub-domain owners exchanging content keys and wires with the kernel, app host, compute, persistence, and the Python and TypeScript peers, edge rails colored by kind and nodes classed by seam direction.
    subgraph element[RASM.ELEMENT]
        Graph[Graph spine]
        Projection[Projection contracts]
        Composition[Composition folds]
        Properties[Property vocabulary]
        Assessment[Assessment receipt]
    end
    Rasm{{Rasm}}
    AppHost([Rasm.AppHost])
    Compute{{Rasm.Compute}}
    Persistence[(Rasm.Persistence)]
    Geometry{{python:geometry}}
    Runtime{{python:runtime}}
    Core{{typescript:core}}
    Projection e1@<-->|"[CONTENT_KEY]: XxHash128"| Rasm
    AppHost e2@-->|"[PORT]: ProjectionContext"| Projection
    Projection e3@-->|"[CONTENT_KEY]: ContentAddress"| Persistence
    Graph e4@<-->|"[SHAPE]: ElementGraph"| Persistence
    Graph e5@<-->|"[CONTENT_KEY]: RepresentationHash"| Compute
    Composition e6@-->|"[SHAPE]: AssemblyAggregator"| Compute
    Properties e7@<-->|"[SHAPE]: DimensionMonomial"| Compute
    Assessment e8@-->|"[SHAPE]: Assessment"| Compute
    Graph e9@<-->|"[WIRE]: GlbContentHash"| Geometry
    Projection e10@<-->|"[CONTENT_KEY]: ContentAddress"| Runtime
    Graph e11@<-->|"[WIRE]: rasm.element.v1"| Core
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef external fill:#8BE9FDBF,stroke:#8BE9FD,color:#282A36
    classDef data fill:#FFB86CBF,stroke:#FFB86C,color:#282A36
    classDef annotation fill:#21222C,stroke:#6272A4,color:#F8F8F2
    classDef edgeData stroke:#FFB86C,color:#F8F8F2
    classDef edgeControl stroke:#FF79C6,color:#F8F8F2
    class Graph,Projection,Composition,Properties,Assessment primary
    class Rasm,Compute,Geometry,Runtime,Core external
    class Persistence data
    class AppHost annotation
    class e1,e3,e5,e9,e10,e11 edgeData
    class e2,e4,e6,e7,e8 edgeControl
```

`[PROJECTION]` rows are inversion of control: every provider — GeometryGym, VividOrange, and peers — stays in the AEC peer that implements `IElementProjection` and lowers its foreign source onto a `GraphDelta`, so no provider edge points down into the seam and no second IFC or section-property stack forms. Each provider owns its concept and mints its own `Object` identity under the owner-mints-its-identity law, so a minter never stamps a foreign projector's egress and the one type representation is authored and ingested unified; per-provider Type and Occurrence minting lives on the owning implementation pages. Acyclic strata holds: every AEC peer references `{Rasm, Rasm.Element}` as a shared lower stratum and peers never reference each other, and the live element assembly — registering the `Seq<IElementProjection>`, binding the tessellation adapter, running `Assemble` against a live source — is an APP/HOST-BOUNDARY composition-root concern, the seam owning the `Assemble` capability and the apps the wiring.

[CONTENT_KEY_IDIOM]:
- Every page joining the persistence, projection, assessment-cache, or diff lane derives its typed `UInt128` through the `Projection/address` `XxHash128.HashToUInt128` seed-zero entry over the one `CanonicalWriter` projection, one content space shared with the kernel `GeometryHash` and the Python/TypeScript peers; a second hasher or a non-zero seed is the named cross-folder drift defect.
- `Graph/wire` carries every content key verbatim, so the codec re-derives no identity on either side of the wire and the cross-runtime parity corpus anchors byte-for-byte agreement.
- Non-rooted `NodeId` is the self-hash of the node's own canonical bytes; a rooted `Object` id carries one regime with two `ObjectKind`-keyed seedings — an Occurrence's Guid-v7 placement identity and a Type's deterministic derivation over canonical bytes with the volatile `Representations` excluded — never a second identity owner. Exact `NodeId.Content` mint, the `Verify` dual, and per-lane key derivations live on the owning implementation pages.
