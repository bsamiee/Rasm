# [MATERIALS_ARCHITECTURE]

Domain map of `Rasm.Materials` — host-neutral AEC-DOMAIN materials projector onto the shared `Rasm.Element` seam. `Component`, `Appearance`, `Properties`, and `Projection` sub-domains each collapse to one owner per axis, and the one `ComponentProjector : IElementProjection` lowers every owner into the shared `ElementGraph`. Its single `Project` fold discriminates a pure-substance `MaterialSpec` arm from a Type-minting `ComponentSpec` arm, minting the deterministic-rooted Type `Object` from the `Component`'s canonical content and authoring the content-keyed `Material`/`Appearance` subgraph the seam `Assemble` fold merges with every sibling projector. AEC peers depend up on `{Rasm, Rasm.Element}` and align by seam contract, never by sibling reference.

## [01]-[DOMAIN_MAP]

```text codemap
Rasm.Materials/            # AEC-DOMAIN materials projector; refs {Rasm, Rasm.Element}; VividOrange in-folder; no host geometry
├── Component/             # One polymorphic Component over the closed component-family axis, class-discriminated
│   ├── Component.cs       # Component owner and the one section solver over the profile algebra
│   ├── Masonry.cs         # Masonry family
│   ├── Steel.cs           # Steel family over the catalogued AISC and EN sections
│   ├── Cmu.cs             # Concrete-masonry-unit family
│   ├── Timber.cs          # Timber family over sawn, glulam, and CLT lamellae
│   ├── Glazing.cs         # Glazing family over insulated-glass pane, spacer, and cavity records
│   ├── Reinforcement.cs   # Reinforcement family over the rebar arrangement and prestressing-strand line
│   ├── Fastener.cs        # Fastener family over the threaded bolt, nut, and washer assembly
│   ├── Connector.cs       # Connector family
│   ├── Joint.cs           # Joint family over the weld, adhesive, and stud connection record
│   ├── Panel.cs           # Panel family over sheet-goods built elements
│   └── Capacity.cs        # One section-capacity resolution and check rail
├── Appearance/            # Measured appearance engine — node graph, BSDF lobe family, and the material wire
│   ├── Bsdf.cs            # Closed BSDF lobe family and the microfacet kernel
│   ├── Graph.cs           # MaterialGraph node-DAG program and the material-library table
│   ├── Surface.cs         # OpenPBR color-science lowering and the layered slab stack
│   ├── Texture.cs         # Texture-sampling fold over the closed texture-source union
│   ├── Photometric.cs     # Light-unit admission fold — the in-folder UnitsNet boundary
│   ├── Weathering.cs      # Aging fold over the closed weathering-effect union
│   ├── Acquisition.cs     # Capture-import fold over the closed capture-source union
│   ├── Finish.cs          # Kubelka-Munk pigment-reflectance finish engine
│   └── Interchange.cs     # MaterialWire and MaterialX .mtlx interchange projection
├── Properties/            # Typed engineering-property source lowered onto the seam property sets
│   ├── Properties.cs      # Intrinsic mechanical, thermal, acoustic, and fire measurements
│   └── Sustainability.cs  # Lifecycle impact, unit-cost basis, and classification rows
└── Projection/            # One IElementProjection onto the Rasm.Element seam
    └── Component.cs       # ComponentProjector minting Type Objects and material subgraphs
```

VividOrange grounds the structural section, capacity, and rebar data in-folder, never a hand-keyed literal, and the per-page consumption law lives on the owning implementation pages. Return type names the rail: a `SurfaceShade`/`Unicolour` carrier where the result is total, `Fin<T>` where a banded fault routes; the projector returns the seam `Fin<GraphDelta>` the `Assemble` fold merges with every sibling delta. C# is the sole producer of the material wire — `Appearance/Interchange` mints the OpenPBR-vector `MaterialWire` and the MaterialX `.mtlx` document once, and the TypeScript and Python peers decode both, so a peer re-mint of the OpenPBR algebra or the MaterialX schema is the named cross-language drift.

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
    accTitle: Materials AEC-domain projection seams
    accDescr: Materials sub-domain owners exchanging projections, section handles, and property sets with the AEC peers Element, Bim, and Fabrication, edge rails colored by kind and nodes classed by seam direction.
    subgraph materials[RASM.MATERIALS]
        Projection[Projection contracts]
        Component[Component families]
        Properties[Property source]
        Appearance[Appearance engine]
    end
    Element{{Rasm.Element}}
    Bim([Rasm.Bim])
    Fabrication([Rasm.Fabrication])
    Element e1@-->|"[SHAPE]: IElementProjection"| Projection
    Projection e2@-->|"[PROJECTION]: GraphDelta"| Element
    Projection e3@-->|"[SHAPE]: DetailSchema"| Bim
    Component e4@<-->|"[SHAPE]: ProfileRef"| Element
    Component e5@-->|"[WIRE]: IIfcTypeReconciler"| Bim
    Properties e6@<-->|"[SHAPE]: MaterialPropertySet"| Element
    Properties e7@-->|"[WIRE]: MaterialPropertySet"| Fabrication
    Appearance e8@-->|"[CONTENT_KEY]: AppearanceSummary"| Element
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef external fill:#8BE9FDBF,stroke:#8BE9FD,color:#282A36
    classDef annotation fill:#21222C,stroke:#6272A4,color:#F8F8F2
    classDef edgeData stroke:#FFB86C,color:#F8F8F2
    classDef edgeExternal stroke:#8BE9FD,color:#F8F8F2
    classDef edgeControl stroke:#FF79C6,color:#F8F8F2
    class Projection,Component,Properties,Appearance primary
    class Element external
    class Bim,Fabrication annotation
    class e2 edgeExternal
    class e1,e3,e4,e6 edgeControl
    class e5,e7,e8 edgeData
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
    accTitle: Materials platform, compute, and cross-runtime seams
    accDescr: Materials sub-domain owners exchanging capacity, property, appearance, and capture wires with compute, the render host, the Python data peer, and the TypeScript core and viewer peers, edge rails colored by kind and nodes classed by seam direction.
    subgraph materials[RASM.MATERIALS]
        Component[Component families]
        Properties[Property source]
        Appearance[Appearance engine]
    end
    Compute{{Rasm.Compute}}
    AppUi([Rasm.AppUi])
    DataPeer([python:data])
    Core([typescript:core])
    Ui([typescript:ui])
    Host([Host boundary])
    Component e1@-->|"[WIRE]: SectionCapacity"| Compute
    Properties e2@-->|"[WIRE]: MaterialPropertySet"| Compute
    Compute e3@-->|"[SHAPE]: AssemblyAggregator"| Properties
    DataPeer e4@-->|"[WIRE]: Environmental"| Properties
    Appearance e5@-->|"[BOUNDARY]: LayeredBsdf"| AppUi
    Appearance e6@-->|"[WIRE]: MaterialWire"| Core
    Appearance e7@-->|"[WIRE]: PbrGroups"| Ui
    Host e8@-->|"[WIRE]: CaptureSource"| Appearance
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef external fill:#8BE9FDBF,stroke:#8BE9FD,color:#282A36
    classDef annotation fill:#21222C,stroke:#6272A4,color:#F8F8F2
    classDef edgeData stroke:#FFB86C,color:#F8F8F2
    classDef edgeControl stroke:#FF79C6,color:#F8F8F2
    class Component,Properties,Appearance primary
    class Compute external
    class AppUi,DataPeer,Core,Ui,Host annotation
    class e3,e5 edgeControl
    class e1,e2,e4,e6,e7,e8 edgeData
```

## [03]-[DOMAIN_LAW]

Canonical-collapse law the sub-domains share — one owner per axis, one entrypoint family per rail, growth by data. Per-page boundary cards carry the concrete seams.

- One component owner: a cross-section is a `ComponentFamily` row over one `Component`, solved by the one dispatch over the closed profile algebra.
- One appearance owner: a material is a `MaterialLibrary` row over one `MaterialGraph`, a lobe a case, a layering modifier a `Slab`.
- One `ComponentProjector.Project` carries the whole material-and-Type subgraph.
- Growth is a row or a closed-union case; a per-family type, a second projector, or a generic material abstraction is the named drift.
- `ComponentFamily` is a closed axis, each family carrying its `ComponentClass` discriminant over the Primary, Panel, and Minor rows.
- An anchor folds as a `FastenerKind` arm; a metal deck, gypsum board, or rigid-board insulation is a `PanelKind` row, never a new family.
- `BsdfLobe` is a closed family; a new lobe admits only when no parameterization reproduces the measured physics, then serves every material.
- Material-composition vocabulary is the seam `MaterialComposition`, referenced and never re-owned; a new case is seam growth.
- Owner mints its own identity: the `ComponentProjector` mints the deterministic-rooted Type `Object` from the exclusion-seeded canonical bytes.
- A Type stamps `Classification`/`PredefinedType` off the stored `IfcBinding` row, so a later geometry attach never re-keys it.
- A model author mints Occurrence `Object`s and `Rasm.Bim` ingests `IfcElementType` into the same Type; the `Bake` inheritance is the seam's.
- Model is host-neutral: no owner holds a host curve or transform; run and layout geometry lands in `Rasm.Generation` at the app root.
- Composition over re-mint at every seam: Materials projects onto `Rasm.Element` and re-mints no seam type, color axis, unit owner, or dimension.
- Color is the admitted perceptual owner consumed directly; units admit UnitsNet at the photometric boundary and ride the seam `MeasureValue`.
- Only the documented author-kernel set — RGB-to-SPD, scene-referred tone-map, BSDF microfacet, noise — is hand-authored.
- An out-of-gamut, non-finite, or degenerate result rails to its banded fault off the `FaultBand` registry, never a propagated NaN or sentinel.
- Standards data is in-fence C# under `SEED_ROW_LAW`: a table is `REFLECTED`, `DELEGATED`, or `AUTHORED`.
- Every seed column carries `VENDOR`, `DEFINED`, or `PUBLISHED` provenance.
- Policy vocabularies stay `[SmartEnum]`, standards enums become frozen row tables, and every seed row flows the one catalogue-to-solver rail.
