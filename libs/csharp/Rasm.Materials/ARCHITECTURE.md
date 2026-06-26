# [MATERIALS_ARCHITECTURE]

The domain map of `Rasm.Materials` — the host-neutral AEC-DOMAIN materials owner. One `ProfileFamily`/`MaterialLibrary`/`ConnectionFamily`/`MaterialProperty` row per concept across the `Profiles`, `Connection`, `Appearance`, `Construction`, and `Properties` sub-domains, never a per-material or per-family type.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
Rasm.Materials/
├── Profiles/             # One polymorphic Profile over a closed ProfileFamily axis; one ParametricSection section-property solver; one SectionCapacity surface owner
│   ├── Profile.cs        # Profile polymorphic owner + the shared ParametricSection VividOrange section-property bridge (gross elastic + the IProfile perimeter the RC section consumes)
│   ├── Masonry.cs        # Masonry ProfileFamily with regional catalogue rows
│   ├── Steel.cs          # Steel ProfileFamily over the VividOrange AISC v16.0 + EN 10365 published catalogue + section solver
│   ├── Cmu.cs            # CMU ProfileFamily over ASTM C90 cell/face-shell record + exact hollow net section
│   ├── Timber.cs         # Timber ProfileFamily over sawn/glulam/CLT lamella records
│   ├── Glazing.cs        # Glazing ProfileFamily over pane/spacer/cavity records
│   └── Capacity.cs       # SectionCapacity
├── Connection/           # One polymorphic ConnectionItem over ConnectionFamily axis; the host-neutral reinforced-concrete-section assembler
│   ├── Connection.cs     # ConnectionItem polymorphic owner over ConnectionFamily axis
│   ├── Reinforcement.cs  # Reinforcement ConnectionFamily over ASTM A615/A706 bar record + the RcSection VividOrange ConcreteSection assembler
│   ├── Fastener.cs       # Fastener ConnectionFamily over ISO 898/SAE J429 thread record
│   ├── Hanger.cs         # Hanger ConnectionFamily over carried-member/capacity record
│   └── Joint.cs          # Joint ConnectionFamily over AWS D1.1 weld/adhesive/stud continuous-connection record
├── Appearance/           # Measured appearance engine
│   ├── Bsdf.cs           # Closed seven-lobe BsdfLobe family and scene-linear spectral edge
│   ├── Graph.cs          # Node-DAG MaterialGraph program and MaterialLibrary row table
│   ├── Texture.cs        # TextureUv sampling fold over closed TextureSource union
│   ├── Photometric.cs    # Light-unit admission fold over PhotometricQuantity band
│   ├── Weathering.cs     # Weathering aging fold over closed WeatheringEffect union
│   ├── Acquisition.cs    # Acquisition import fold over closed CaptureSource union
│   ├── Finish.cs         # FinishMix Kubelka-Munk pigment-reflectance engine
│   ├── Interchange.cs    # MaterialWire and MaterialX .mtlx interchange projection
│   └── Surface.cs        # SpectralUpsample/ToneMap/ConductorIor/SlabStack OpenPBR color-science lowering
├── Construction/         # Host-neutral construction model
│   ├── Assembly.cs       # IFC 4.3 material-assignment owner and Element placed-unit model
│   ├── Layout.cs         # One ConstructionLayout.Resolve placement fold over any ProfileFamily
│   └── Nesting.cs        # One StockNest.Pack cutting-stock fold over the RectangleBinPack packer collapse
└── Properties/           # Typed engineering-property model
    ├── Properties.cs     # MaterialProperty closed family with quantity-coercion fold
    └── Sustainability.cs # Environmental/Cost/Classification MaterialProperty discipline with lifecycle aggregation folds
```

Implementation collapses to one owner per axis and one entrypoint family per rail: a new cross-section is a `ProfileFamily` row (a steel section one `American`/`European` identity in the `VividOrange.Profiles.Catalogue` published database the `SectionReader` admits and the `VividOrange.Sections.SectionProperties` solver computes from, never a hand-keyed literal), a new material a `MaterialLibrary` row, a new lobe a `BsdfLobe` `[Union]` case, a new connection a `ConnectionFamily` row, a new rebar arrangement a `RebarLayout` `[Union]` case the one `RcSection.Of` fold dispatches over the `VividOrange.Sections` `ConcreteSection`, a new section-capacity kind a `SectionCapacity` `[Union]` case the one `SectionCapacity.Resolve`/`Check` rail discriminates (the RC N-M-M `VividOrange.InteractionDiagram` hull, the RC `ConcreteSectionProperties` elastic transformed-section, the steel LRFD receipt), a new engineering property a `MaterialProperty` case, a new cutting-stock algorithm a `NestStrategy` `[Union]` case the one `StockNest.Pack` fold dispatches — never a new surface. The rail is named in the return type: a `SurfaceShade`/`Unicolour` carrier where the result is total, `Fin<T>` where a banded fault routes — `ProfileFault` 2300, `ConstructionFault` 2350, `ConnectionFault` 2360, `MaterialFault` 2450, all disjoint from the kernel `GeometryFault` band 2400. C# is the sole producer of the material wire: `Appearance/Interchange` `MaterialWire` and `MtlxDocument` mint the OpenPBR-vector and MaterialX `.mtlx` interchange once, and the TypeScript and Python peers decode both — a peer re-mint of the OpenPBR algebra, the `ConductorIor` table, or the MaterialX schema is the named cross-language drift defect.

## [02]-[SEAMS]

```text seams
Appearance/interchange     →  typescript:interchange/codec     # [WIRE]: MaterialWire OpenPBR vector wire
Appearance/bsdf            →  csharp:Rasm.Bim/Semantics        # [CONTENT_KEY]: BimAppearance
Appearance/bsdf            ←  host-free-peer / host-edge wire  # [WIRE]
Appearance/bsdf            →  csharp:Rasm.AppUi/Render/pathtrace  # [BOUNDARY]: LayeredBsdf.Sample/Evaluate/Pdf + SlabStack.ToLayered at PATH_TRACE seam
Appearance/bsdf            →  csharp:Rasm.AppUi/Render/shading    # [BOUNDARY]: LayeredBsdf lobe-weight uniforms at SURFACE_SHADE seam
Appearance/graph           →  csharp:Rasm.AppUi/Render/pathtrace  # [BOUNDARY]: MaterialGraph.Evaluate SurfaceShade sink to integrator + GPU shading pass
Appearance/graph           →  csharp:Rasm.Persistence          # [TRANSPORT]: MaterialLibrary content-keyed durable catalogue rows
Connection                 →  csharp:Rasm.Bim/Model            # [WIRE]: ConnectionItem IFC wire IfcReinforcingBar/IfcMechanicalFastener
Connection/joint           →  csharp:Rasm.Bim/Model            # [WIRE]: weld/stud IfcMechanicalFastener + IfcRelConnectsWithRealizingElements
Properties                 →  csharp:Rasm.Fabrication/Process  # [WIRE]: Thermal Conductivity / SpecificHeat / Density scalars
Properties/sustainability  →  csharp:Rasm.Bim/Semantics        # [PROJECTION]: Pset_EnvironmentalImpactValues + Uniclass/OmniClass
Properties/sustainability  →  cs:AEC_SIMULATION_BRIDGE         # [WIRE]: embodied-carbon GWP + cost rollup by MaterialId
Construction/assembly      →  csharp:Rasm.Bim/Semantics        # [PROJECTION]: MaterialAssignment IFC trichotomy LayerSet/ProfileSet/ConstituentSet
Construction/layout        →  csharp:Rasm.Rhino                # [BOUNDARY]: Placement / RebarBend / dome ring-course host materialization
Construction/nesting       →  csharp:Rasm.Fabrication/Process  # [WIRE]
Construction/nesting       →  csharp:Rasm.Materials/Properties # [PROJECTION]
Profiles/steel             ←  VividOrange.Profiles.Catalogue   # [DATA]
Connection/reinforcement   ←  VividOrange.Sections             # [DATA]
Connection/reinforcement   ←  VividOrange.Materials            # [DATA]
Connection/reinforcement   →  csharp:Rasm.Materials/Profiles   # [BOUNDARY]
Profiles/capacity          ←  VividOrange.InteractionDiagram   # [DATA]
Profiles/capacity          ←  VividOrange.Sections.SectionProperties  # [DATA]
Profiles/capacity          →  cs:AEC_SIMULATION_BRIDGE         # [WIRE]
```

## [03]-[DOMAIN_LAW]

The canonical-collapse law the three domains share — one owner per axis, one entrypoint family per rail, growth by data. The per-page boundary cards carry the concrete seams; this map states only the collapse rule and the closed counts the codemap enforces.

- One owner per concept: a cross-section is a `ProfileFamily` row over one `Profile`, its section properties the one `ParametricSection`/`SectionReader` over the `VividOrange.Sections.SectionProperties` polygon integral (catalogued steel, parametric cmu/timber alike — never a per-family rectangular literal), a material a `MaterialLibrary` row over one `MaterialParameters`/`MaterialGraph`, an appearance variation an `AppearanceNode` case, a lobe a `BsdfLobe` `[Union]` case, a layering modifier a `Slab` case over one `SlabStack`, a measured metal a `ConductorMetal` row over one `ConductorIor` table, a material wire the one `MaterialWire`/`MtlxDocument`, a construction element an `Element`, an assignment a `MaterialAssignment` case, a layout the one `ConstructionLayout.Resolve` fold, a cutting-stock nest the one `StockNest.Pack` fold over the `NestStrategy` `[Union]` collapsing the five `RectangleBinPack` packers. A `BrickProfile`/`SteelSection`/`GoldMaterial` class, a `MetalFactory`, a hand-keyed section-property literal table, a per-family `Profile`/graph/layout variant, a per-packer nesting surface, a peer-side OpenPBR re-mint, or a generic `IMaterial`/`IProfile`/`IAppearance` abstraction is the named defect; growth is a row or a closed-union case, never a new surface.
- Two closed counts the codemap fixes: the `BsdfLobe` family is closed at seven (a new lobe admits only when no parameterization of the set reproduces the measured physics, and then serves ALL materials), and the `MaterialAssignment` trichotomy is closed at three (`IfcMaterialList` deprecated, never admitted; a fourth case or a per-element-type assignment is the defect).
- The construction model is host-neutral: a `Placement` is a scalar tuple and a `Layout` a `Seq<Element>` of placements, never a `Rhino.Geometry` curve/transform — the host materializes the stream at the future APP root and the assignment serializes to IFC at the `Rasm.Bim` boundary, never an interior `IfcOpenShell` evaluation.
- Composition over re-mint at every seam: `Rasm.Materials` references no host-boundary package and re-mints no color axis, unit owner, vector, or dimension — color is Wacton.Unicolour consumed directly as the scene-linear/spectral owner, the photometric and engineering-property unit coercion admits UnitsNet IN-FOLDER through the `Appearance/photometric` `MaterialUnits` boundary coerced once at admission (the strata-acyclic AEC-domain owns its own unit boundary; it never reaches DOWN to the app-platform `Rasm.Compute` units owner, the seam the `Rasm.Compute` `Symbolic/units` owner and its `ARCHITECTURE [04]` enshrine), and dimensions/frames the `Rasm` kernel value-objects; only the documented author-kernel set (RGB→SPD, RRT/ODT, scene-referred tone-map, BSDF/Fresnel/GGX/noise) is hand-authored, and an out-of-gamut, non-finite, or degenerate result rails to a banded fault (`ProfileFault` 2300, `ConstructionFault` 2350, `ConnectionFault` 2360, `MaterialFault` 2450 — all disjoint from the kernel `GeometryFault` band 2400), never a propagated NaN or sentinel.
