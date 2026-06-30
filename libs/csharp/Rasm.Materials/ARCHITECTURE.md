# [MATERIALS_ARCHITECTURE]

The domain map of `Rasm.Materials` — the host-neutral AEC-DOMAIN materials PROJECTOR onto the shared `Rasm.Element` seam. One `ProfileFamily`/`MaterialLibrary`/`ConnectionFamily`/`MaterialPropertySet` row per concept across the `Profiles`, `Connection`, `Appearance`, `Construction`, `Properties`, and `Projection` sub-domains, never a per-material or per-family type; the `Projection` sub-domain lowers every owner into the shared seam `ElementGraph` through the one `MaterialProjector:IElementProjection`, authoring a content-keyed material subgraph (and, when the context vouches for an element NodeId, the element→material `Associate` edge [H12]) the seam `Assemble` fold merges with every sibling projector. AEC peers depend up on `{Rasm, Rasm.Element}` and never reference each other; alignment is by the seam contracts, not sibling coupling.

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
├── Construction/         # Host-neutral composition author and layout model
│   ├── Assembly.cs       # Seam MaterialComposition author (layer/profile(ProfileRef)/constituent + C7 LayerSetUsage/ProfileSetUsage) and the host-neutral RunPath/Placement/ConstructionFault run geometry (Element/MaterialAssignment RETIRED — the seam owns the element and the composition)
│   ├── Layout.cs         # One ConstructionLayout.Resolve placement fold over any ProfileFamily, producing a Seq<Placement> stream
│   └── Nesting.cs        # One StockNest.Pack cutting-stock fold over the RectangleBinPack packer collapse
├── Properties/           # Typed engineering-property source, lowered into the seam
│   ├── Properties.cs     # MaterialPropertyCatalogue (mechanical/thermal/acoustic/fire) lowered into the seam Discipline-keyed MaterialPropertySet + the MaterialAssessmentInput discipline marshalling (acoustic folds SEAM-owned; AssemblyAggregator relocated to Rasm.Compute)
│   └── Sustainability.cs # Environmental/Cost/Classification discipline source lowered into the seam MaterialPropertySet (the lifecycle AggregateEnvironmental/AggregateCost folds relocated to Rasm.Compute)
└── Projection/           # The IElementProjection onto the Rasm.Element seam
    └── Material.cs        # MaterialProjector:IElementProjection — Project-only material subgraph (Material/Appearance/Assessment nodes) + H12 element→material Associate edges, content-keyed via the seam ContentAddress
```

Implementation collapses to one owner per axis and one entrypoint family per rail: a new cross-section is a `ProfileFamily` row (a steel section one `American`/`European` identity in the `VividOrange.Profiles.Catalogue` published database the `SectionReader` admits and the `VividOrange.Sections.SectionProperties` solver computes from, never a hand-keyed literal), a new material a `MaterialLibrary` row, a new lobe a `BsdfLobe` `[Union]` case, a new connection a `ConnectionFamily` row, a new rebar arrangement a `RebarLayout` `[Union]` case the one `RcSection.Of` fold dispatches over the `VividOrange.Sections` `ConcreteSection`, a new section-capacity kind a `SectionCapacity` `[Union]` case the one `SectionCapacity.Resolve`/`Check` rail discriminates (the RC N-M-M `VividOrange.InteractionDiagram` hull, the RC `ConcreteSectionProperties` elastic transformed-section, the steel LRFD receipt), a new engineering property a seam `MaterialPropertySet` case the `MaterialPropertyCatalogue` lowers into, a new cutting-stock algorithm a `NestStrategy` `[Union]` case the one `StockNest.Pack` fold dispatches, a new projected node a seam `Node` case the one `MaterialProjector.Project` fold authors — never a new surface. The rail is named in the return type: a `SurfaceShade`/`Unicolour` carrier where the result is total, `Fin<T>` where a banded fault routes — `ProfileFault` 2300, `ConstructionFault` 2350, `ConnectionFault` 2360, `MaterialFault` 2450, `ProjectionFault` 2470, all disjoint from the kernel `GeometryFault` band 2400; the projector returns the seam `Fin<GraphDelta>` and the seam `Assemble` fold merges every projector's delta. C# is the sole producer of the material wire: `Appearance/Interchange` `MaterialWire` and `MtlxDocument` mint the OpenPBR-vector and MaterialX `.mtlx` interchange once, and the TypeScript and Python peers decode both — a peer re-mint of the OpenPBR algebra, the `ConductorIor` table, or the MaterialX schema is the named cross-language drift defect.

## [02]-[SEAMS]

```text seams
Projection/material        ←  csharp:Rasm.Element/Projection    # [CONTRACT]: IElementProjection / ProjectionContext / GraphDelta / NodeId / ContentAddress / Assemble
Projection/material        →  csharp:Rasm.Element/Graph         # [PROJECTION]: MaterialProjector.Project lowers the material subgraph into one GraphDelta the seam Assemble fold merges
Projection/material        →  csharp:Rasm.Element/Relations     # [H12]: element→material Associate edge (Subject=element, Resource=material) + C7 MaterialUsage, authored only when ctx.ElementIds vouches the element NodeId
Properties/properties      ←  csharp:Rasm.Element/Composition   # [CONTRACT]: MaterialPropertySet cases + the intrinsic acoustic folds (Nrc/Saa/StcWeighted over the shared RatingContour.Stc.Fit kernel), referenced not re-authored
Properties/properties      →  csharp:Rasm.Element/Assessment    # [PROJECTION]: Discipline-keyed Assessment INPUT payloads (MaterialAssessmentInput) on the seam Assessment node
Properties/properties      →  csharp:Rasm.Compute              # [WIRE]: the multi-ply AssemblyAggregator + discipline solvers + Assessment.Result writeback (RELOCATED)
Properties/sustainability  →  csharp:Rasm.Compute              # [WIRE]: the lifecycle AggregateEnvironmental / AggregateCost folds + embodied-carbon/cost rollup (RELOCATED)
Construction/assembly      ←  csharp:Rasm.Element/Composition   # [CONTRACT]: MaterialComposition (Single/LayerSet/ProfileSet(ProfileRef)/ConstituentSet) + MaterialUsage (None/LayerSet/ProfileSet)
Construction/layout        →  csharp:Rasm.Rhino                # [BOUNDARY]: Seq<Placement> / RebarBend / dome ring-course host materialization
Construction/nesting       →  csharp:Rasm.Fabrication/Process  # [WIRE]
Construction/nesting       →  csharp:Rasm.Materials/Properties # [PROJECTION]
Profiles/profile           ←  csharp:Rasm.Element/Composition   # [CONTRACT]: ProfileRef — the seam ProfileSet case's section handle
Profiles/profile           →  csharp:Rasm.Element/Composition   # [M7]: ProfileResolution resolves ProfileRef → ComputedSection one-hop so structural consumers never re-resolve per call
Profiles/capacity          →  csharp:Rasm.Compute              # [WIRE]: section capacity feeds the structural Assessment route
Appearance/interchange     →  csharp:Rasm.Element/Composition   # [CONTENT_KEY]: the OpenPBR/MaterialX MaterialWire lowers to the content-keyed seam Appearance node
Appearance/interchange     →  typescript:interchange/codec     # [WIRE]: MaterialWire OpenPBR vector wire
Appearance/bsdf            ←  host-free-peer / host-edge wire  # [WIRE]
Appearance/bsdf            →  csharp:Rasm.AppUi/Render/pathtrace  # [BOUNDARY]: LayeredBsdf.Sample/Evaluate/Pdf + SlabStack.ToLayered at PATH_TRACE seam
Appearance/bsdf            →  csharp:Rasm.AppUi/Render/shading    # [BOUNDARY]: LayeredBsdf lobe-weight uniforms at SURFACE_SHADE seam
Appearance/graph           →  csharp:Rasm.AppUi/Render/pathtrace  # [BOUNDARY]: MaterialGraph.Evaluate SurfaceShade sink to integrator + GPU shading pass
Connection                 →  csharp:Rasm.Bim/Model            # [WIRE]: ConnectionItem IFC wire IfcReinforcingBar/IfcMechanicalFastener (connection-item egress; a connection's material binding rides the MaterialProjector)
Connection/joint           →  csharp:Rasm.Bim/Model            # [WIRE]: weld/stud IfcMechanicalFastener + IfcRelConnectsWithRealizingElements
Properties                 →  csharp:Rasm.Fabrication/Process  # [WIRE]: Thermal Conductivity / SpecificHeat / Density scalars
Profiles/steel             ←  VividOrange.Profiles.Catalogue   # [DATA]
Profiles/capacity          ←  VividOrange.InteractionDiagram   # [DATA]
Profiles/capacity          ←  VividOrange.Sections.SectionProperties  # [DATA]
Connection/reinforcement   ←  VividOrange.Sections             # [DATA]
Connection/reinforcement   ←  VividOrange.Materials            # [DATA]
Connection/reinforcement   →  csharp:Rasm.Materials/Profiles   # [BOUNDARY]
```

## [03]-[DOMAIN_LAW]

The canonical-collapse law the three domains share — one owner per axis, one entrypoint family per rail, growth by data. The per-page boundary cards carry the concrete seams; this map states only the collapse rule and the closed counts the codemap enforces.

- One owner per concept: a cross-section is a `ProfileFamily` row over one `Profile`, its section properties the one `ParametricSection`/`SectionReader` over the `VividOrange.Sections.SectionProperties` polygon integral (catalogued steel, parametric cmu/timber alike — never a per-family rectangular literal), a material a `MaterialLibrary` row over one `MaterialParameters`/`MaterialGraph`, an appearance variation an `AppearanceNode` case, a lobe a `BsdfLobe` `[Union]` case, a layering modifier a `Slab` case over one `SlabStack`, a measured metal a `ConductorMetal` row over one `ConductorIor` table, a material wire the one `MaterialWire`/`MtlxDocument` the projector lowers to the seam `Appearance` node, a material composition a seam `MaterialComposition` case the `Construction/assembly#MATERIAL_COMPOSITION` author builds, a layout the one `ConstructionLayout.Resolve` fold over a `Seq<Placement>`, the whole material subgraph the one `MaterialProjector.Project` over the seam, a cutting-stock nest the one `StockNest.Pack` fold over the `NestStrategy` `[Union]` collapsing the five `RectangleBinPack` packers. A `BrickProfile`/`SteelSection`/`GoldMaterial` class, a `MetalFactory`, a hand-keyed section-property literal table, a per-family `Profile`/graph/layout variant, a per-packer nesting surface, a peer-side OpenPBR re-mint, or a generic `IMaterial`/`IProfile`/`IAppearance` abstraction is the named defect; growth is a row or a closed-union case, never a new surface.
- Two closed counts the codemap fixes: the `BsdfLobe` family is closed at seven (a new lobe admits only when no parameterization of the set reproduces the measured physics, and then serves ALL materials), and the material-composition vocabulary is closed by the SEAM `MaterialComposition` (single/layer-set/profile-set(`ProfileRef`)/constituent-set; `IfcMaterialList` deprecated, never admitted) — referenced here, not re-owned, so a fourth composition case is a seam growth and a parallel Materials assignment type is the named defect.
- The construction model is host-neutral: a `Placement` is a scalar tuple and a `Layout` a `Seq<Placement>` stream, never a `Rhino.Geometry` curve/transform — the host materializes the stream at the APP root and the material composition lowers into the seam `Material` node the `MaterialProjector` authors (`Rasm.Bim` reads the IFC projection from the seam graph), never an interior `IfcOpenShell` evaluation.
- Composition over re-mint at every seam: `Rasm.Materials` projects onto the `Rasm.Element` seam, references no host-boundary package, and re-mints no seam type, color axis, unit owner, vector, or dimension — color is Wacton.Unicolour consumed directly as the scene-linear/spectral owner, the photometric and engineering-property unit coercion admits UnitsNet IN-FOLDER through the `Appearance/photometric` `MaterialUnits` boundary coerced once at admission (the strata-acyclic AEC-domain owns its own unit boundary; it never reaches DOWN to the app-platform `Rasm.Compute` units owner, the seam the `Rasm.Compute` `Symbolic/units` owner and its `ARCHITECTURE [04]` enshrine), and dimensions/frames the `Rasm` kernel value-objects; only the documented author-kernel set (RGB→SPD, RRT/ODT, scene-referred tone-map, BSDF/Fresnel/GGX/noise) is hand-authored, and an out-of-gamut, non-finite, or degenerate result rails to a banded fault (`ProfileFault` 2300, `ConstructionFault` 2350, `ConnectionFault` 2360, `MaterialFault` 2450 — all disjoint from the kernel `GeometryFault` band 2400), never a propagated NaN or sentinel.
