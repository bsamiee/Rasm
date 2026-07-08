# [MATERIALS_ARCHITECTURE]

The domain map of `Rasm.Materials` — the host-neutral AEC-DOMAIN materials PROJECTOR onto the shared `Rasm.Element` seam. One `Component`/`MaterialLibrary`/`MaterialPropertySet` row per concept across the `Component`, `Appearance`, `Properties`, and `Projection` sub-domains, never a per-material or per-family type; the `Projection` sub-domain lowers every owner into the shared seam `ElementGraph` through the one `ComponentProjector:IElementProjection` (the merge of the prior `MaterialProjector` and `ConnectionProjector`), whose single `Project` fold discriminates a pure-substance `MaterialSpec` arm from a Type-minting `ComponentSpec` arm — minting the deterministic-rooted Type `Object` from the `Component`'s canonical content (the owner-mints-its-identity law) AND authoring the content-keyed `Material`/`Appearance` subgraph, the whole delta the seam `Assemble` fold merges with every sibling projector. AEC peers depend up on `{Rasm, Rasm.Element}` and never reference each other; alignment is by the seam contracts, not sibling coupling.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
Rasm.Materials/
├── Component/ # One polymorphic Component over the closed ten-family axis, class-discriminated
│   ├── Component.cs # The Component owner + its class/family axes, the SectionProfile algebra, the one SectionSolver, the ComputedSection receipt
│   ├── Masonry.cs # Masonry family Minor
│   ├── Steel.cs # Steel family Primary over the VividOrange AISC + EN catalogue
│   ├── Cmu.cs # Cmu family Minor
│   ├── Timber.cs # Timber family Primary over sawn/glulam/CLT lamellae
│   ├── Glazing.cs        # Glazing family (Minor) over EN 1279 insulated-glass pane/spacer/cavity records
│   ├── Reinforcement.cs # Reinforcement family Minor
│   ├── Fastener.cs # Fastener family Minor over the ISO 898 thread record + bolt/nut/washer assembly
│   ├── Connector.cs # Connector family Minor
│   ├── Joint.cs          # Joint family (Minor) over the AWS D1.1 weld/adhesive/stud continuous-connection record and the steel shear-stud interface
│   ├── Panel.cs # Panel family Panel, IfcBuiltElement
│   └── Capacity.cs # The one SectionCapacity [Union]
├── Appearance/           # The measured appearance engine — node graph, closed seven-lobe BSDF, OpenPBR color-science, and the C#-sole material wire
│   ├── Bsdf.cs           # The closed seven-lobe BsdfLobe family and the frame-local microfacet kernel
│   ├── Graph.cs          # The node-DAG MaterialGraph program and the MaterialLibrary row table
│   ├── Surface.cs        # OpenPBR color-science lowering — spectral upsample, tone-map, conductor metal, and the layered slab stack
│   ├── Texture.cs        # The TextureUv sampling fold over the closed TextureSource union
│   ├── Photometric.cs    # The light-unit admission fold over the PhotometricQuantity band — the in-folder UnitsNet boundary
│   ├── Weathering.cs     # The aging fold over the closed WeatheringEffect union
│   ├── Acquisition.cs    # The import fold over the closed CaptureSource union — the MathNet.Numerics thin-QR GGX fit
│   ├── Finish.cs         # The FinishMix Kubelka-Munk pigment-reflectance engine
│   └── Interchange.cs    # The MaterialWire and MaterialX .mtlx interchange projection — C# the sole producer
├── Properties/           # The typed engineering-property source, lowered into the seam MaterialPropertySet cases
│   ├── Properties.cs # Intrinsic mechanical/thermal/acoustic/fire measurements, VividOrange uncertainty lowered to the seam MeasureBand
│   └── Sustainability.cs # Lifecycle impact, unit-cost basis, and classification rows lowered into the seam Environmental/Cost sets
└── Projection/           # The one IElementProjection onto the Rasm.Element seam
    └── Component.cs # ComponentProjector:IElementProjection the MaterialProjector+ConnectionProjector merge
```

Implementation collapses to one owner per axis and one entrypoint family per rail: a new cross-section is a `ComponentFamily` row over one `Component` (a steel section one `American`/`European` identity in the `VividOrange.Profiles.Catalogue` published database `CatalogueFactory.CreateAmerican`/`CreateEuropean` admits and the one `SectionSolver.Solve` computes from through `VividOrange.Sections.SectionProperties`, never a hand-keyed literal), a new material a `MaterialLibrary` row, a new lobe a `BsdfLobe` `[Union]` case, a new standardized part a `ComponentFamily` row in its family vocabulary, a new rebar arrangement a `RebarLayout` `[Union]` case the one `RcSection.Of` fold dispatches over the `VividOrange.Sections` `ConcreteSection`, a new section-capacity kind a `SectionCapacity` `[Union]` case the one `SectionCapacity.Resolve`/`Check` rail discriminates (the RC N-M-M `VividOrange.InteractionDiagram` hull, the RC `ConcreteSectionProperties` elastic transformed-section, the steel LRFD receipt, the timber/masonry cases), a new engineering property a seam `MaterialPropertySet` case the `MaterialPropertyCatalogue` lowers into, a new projected node a seam `Node` case the one `ComponentProjector.Project` fold authors — never a new surface. The rail is named in the return type: a `SurfaceShade`/`Unicolour` carrier where the result is total, `Fin<T>` where a banded fault routes — `ComponentFault` (profile and connection failures share the one owner band), `MaterialFault`, and `ProjectionFault`, each reading its band off the `Rasm.Element` `FaultBand` registry (`Component`/`Material`/`Projection`; band 2350 is registry-reserved for `Rasm.Generation`), disjointness type-enforced against the kernel `GeometryFault` and seam `ElementFault` rows; the projector returns the seam `Fin<GraphDelta>` and the seam `Assemble` fold merges every projector's delta. C# is the sole producer of the material wire: `Appearance/Interchange` `MaterialWire` and `MtlxDocument` mint the OpenPBR-vector and MaterialX `.mtlx` interchange once, and the TypeScript and Python peers decode both — a peer re-mint of the OpenPBR algebra, the `ConductorMetal` rows, or the MaterialX schema is the named cross-language drift defect.

## [02]-[SEAMS]

```text seams
Projection/component       ←  csharp:Rasm.Element/Projection # [SHAPE]: IElementProjection contract set + Rasm.Bim IGraphConstraint validate gate
Projection/component       →  csharp:Rasm.Element/Graph # [PROJECTION]: mints deterministic-rooted Type Object + content-keyed Material subgraph
Projection/component       →  csharp:Rasm.Element/Relations # [PROJECTION]: Associate edge + Assign.TypeDefinition bind, gated on ctx.Owns
Projection/component       ⇄  csharp:Rasm.Element/Properties # [SHAPE]: DetailSchema realization/product bags + canonical PropertyName vocabulary
Projection/component       →  csharp:Rasm.Bim/Semantics # [SHAPE]: BIM Round-trips IDENTICAL DetailSchema realization bag at IFC ingress/Emit
Component/component        ⇄  csharp:Rasm.Element/Graph # [SHAPE]: the owner-mints-its-identity law + named Bake type→occurrence inheritance
Component/component        ←  csharp:Rasm.Element/Composition # [SHAPE]: the seam ProfileRef/ProfileSet section handle + receipt, composed unchanged
Component/component        ←  csharp:Rasm.Element/Composition # [PROJECTION]: ProfileRef to ResolvedComponent lifts ComputedSection
Component/capacity         →  csharp:Rasm.Compute                    # [WIRE]: section capacity feeds the structural Assessment route
Properties/properties      ⇄  csharp:Rasm.Element/Composition # [SHAPE]: MaterialPropertySet cases plus MeasureBand uncertainty
Properties/properties      →  csharp:Rasm.Compute # [WIRE]: Discipline-keyed MaterialPropertySet Compute reads off Material node directly
Properties/sustainability  →  csharp:Rasm.Compute # [WIRE]: lifecycle AggregateEnvironmental/AggregateCost folds + carbon/cost rollup
Properties/sustainability  ←  python:data/impact # [WIRE]: EN 15804 set as Discipline.Environmental / MaterialPropertySet.Environmental
Properties/properties      ←  csharp:Rasm.Compute/Analysis           # [SHAPE]
Component/component        →  csharp:Rasm.Bim/Model # [WIRE]: IIfcTypeReconciler Type Object identity
Properties/properties      →  csharp:Rasm.Fabrication/Process/physics # [WIRE]: raw-double Conductivity, SpecificHeat, and Density scalars
Projection/component       ←  csharp:Rasm.Element/Composition # [SHAPE]: MaterialComposition/MaterialUsage cases
Appearance/interchange     →  csharp:Rasm.Element/Graph # [CONTENT_KEY]: lowers row to content-keyed AppearanceSummary at full precision
Appearance/interchange     →  typescript:core/interchange/codec # [WIRE]: decode-only mirror of C# projection
Appearance/interchange     →  typescript:ui/viewer                   # [WIRE]: PbrGroups appearance decode at the scene appearance leaves
Appearance/bsdf            →  csharp:Rasm.AppUi/Render/pathtrace # [BOUNDARY]: LayeredBsdf.Sample/Evaluate/Pdf + SlabStack.ToLayered at PATH_TRACE
Appearance/bsdf            →  csharp:Rasm.AppUi/Render/shading       # [BOUNDARY]: LayeredBsdf lobe-weight uniforms at the SURFACE_SHADE seam
Appearance/graph           →  csharp:Rasm.AppUi/Render/pathtrace # [BOUNDARY]: MaterialGraph.Evaluate SurfaceShade sink to integrator + GPU shading
Appearance/acquisition     ←  host-free-peer / host-edge wire # [WIRE]: app-root .bsdf/.exr decode feeds import fold
Component/steel            ←  VividOrange.Profiles.Catalogue         # [BOUNDARY]
Component/capacity         ←  VividOrange.InteractionDiagram         # [BOUNDARY]
Component/capacity         ←  VividOrange.Sections.SectionProperties # [BOUNDARY]
Component/reinforcement    ←  VividOrange.Sections                   # [BOUNDARY]
Component/reinforcement    ←  VividOrange.Materials                  # [BOUNDARY]
```

## [03]-[DOMAIN_LAW]

The canonical-collapse law the sub-domains share — one owner per axis, one entrypoint family per rail, growth by data. The per-page boundary cards carry the concrete seams; this map states only the collapse rule and the closed counts the codemap enforces.

- One owner per concept: a cross-section is a `ComponentFamily` row over one `Component` (the prior parallel `Profile`/`ConnectionItem` collapsed into the one owner), its section properties the one `SectionSolver.Solve` exhaustive dispatch over the closed `SectionProfile` algebra + the `VividOrange.Sections.SectionProperties` polygon integral (catalogued steel, parametric cmu/timber alike — never a per-family rectangular literal or perimeter builder), a material a `MaterialLibrary` row over one `MaterialParameters`/`MaterialGraph`, an appearance variation an `AppearanceNode` case, a lobe a `BsdfLobe` `[Union]` case, a layering modifier a `Slab` case over one `SlabStack`, a measured metal a `ConductorMetal` row carrying its measured `ComplexIor`, a material wire the one `MaterialWire`/`MtlxDocument` the projector lowers to the seam `Appearance` node, a material composition a seam `MaterialComposition` case the `Projection/component#COMPOSITION_AUTHOR` builds, the whole material-and-Type subgraph the one `ComponentProjector.Project` over the seam. A `BrickProfile`/`SteelSection`/`Rebar`/`Hanger`/`Weld`/`GoldMaterial` class, a `MetalFactory`, a hand-keyed section-property literal table, a per-family `Component`/graph variant, a second `MaterialProjector`/`ConnectionProjector` surface, a peer-side OpenPBR re-mint, or a generic `IMaterial`/`IProfile`/`IAppearance` abstraction is the named defect; growth is a row or a closed-union case, never a new surface.
- Three closed counts the codemap fixes: the `ComponentFamily` axis is closed at TEN (masonry · cmu · steel · timber · glazing · reinforcement · fastener · connector · joint · panel, each carrying its `ComponentClass` discriminant grown to THREE rows {Primary one-piece `IfcBuiltElement`, Panel many-pieces `IfcBuiltElement` sheet goods, Minor many-pieces `IfcElementComponent`} — `anchor` folds as a `FastenerKind` arm and a metal deck / gypsum board / sheathing / rigid-board insulation is a `PanelKind` row in the one `panel` family, never an eleventh family), the `BsdfLobe` family is closed at seven (a new lobe admits only when no parameterization of the set reproduces the measured physics, and then serves ALL materials), and the material-composition vocabulary is closed by the SEAM `MaterialComposition` (single/layer-set/profile-set(`ProfileRef`)/constituent-set; `IfcMaterialList` deprecated, never admitted) — referenced here, not re-owned, so a fourth composition case is a seam growth and a parallel Materials assignment type is the named defect.
- The owner mints its own identity: `Rasm.Materials` owns Component TYPES, so the one `Projection/component#COMPONENT_PROJECTOR` MINTS the deterministic-rooted Type `Object` (the seam `ObjectKind.Type` regime, its `NodeId` derived from the `Component`'s representation-excluded canonical seed so a later geometry attach never re-keys the type and identical Components dedup to one Type) AND stamps its `Classification`/`PredefinedType` off the `Component/component#COMPONENT_OWNER` stored `IfcBinding` row (`Component.IfcEntity`/`PredefinedToken` field reads — the deleted switch), never a minter stamping a foreign projector's egress; a model author mints Occurrence `Object`s and `Rasm.Bim` ingests `IfcElementType`→the SAME Type `Object`. The named `Bake` type→occurrence inheritance (single fields occurrence-overrides-type, `Seq` fields union+dedup-by-key, the `TypeId`/`BakedMaterial`/`TypeBinding` accessors) is the seam's, the projector binding occurrences via `Assign.TypeDefinition`.
- The model is host-neutral: no owner holds a `Rhino.Geometry` curve/transform. The run/layout geometry (a scalar `Placement` tuple, a `Layout` as a `Seq<Placement>` stream) landed in `Rasm.Generation` — the host materializes the stream at the APP root; `Rasm.Materials` keeps only the material composition lowering into the seam `Material` node the `ComponentProjector` authors (`Rasm.Bim` reads the IFC projection from the seam graph), never an interior `IfcOpenShell` evaluation.
- Composition over re-mint at every seam: `Rasm.Materials` projects onto the `Rasm.Element` seam, references no host-boundary package, and re-mints no seam type, color axis, unit owner, vector, or dimension — color is Wacton.Unicolour consumed directly as the scene-linear/spectral owner, the photometric and appearance unit coercion admits UnitsNet IN-FOLDER through the `Appearance/photometric` `MaterialUnits` boundary coerced once at admission (the strata-acyclic AEC-domain owns its own unit boundary; the engineering-property SI coercion now rides the seam `MeasureValue`, and the folder never reaches DOWN to the app-platform `Rasm.Compute` units owner), and dimensions the `Rasm` kernel value-objects (`PositiveMagnitude`/`Dimension`/`UnitInterval`, living in `Rasm.Numerics`); only the documented author-kernel set (RGB→SPD, RRT/ODT, scene-referred tone-map, BSDF/Fresnel/GGX/noise) is hand-authored, and an out-of-gamut, non-finite, or degenerate result rails to a banded fault (`ComponentFault`/`MaterialFault`/`ProjectionFault`, each reading its integer off the type-enforced `Rasm.Element` `FaultBand` registry — disjoint from the kernel `GeometryFault` and seam `ElementFault` rows by construction, a duplicate integer failing at type initialization; band 2350 is registry-reserved for `Rasm.Generation`), never a propagated NaN or sentinel.
- Standards data is in-fence C# under `SEED_ROW_LAW`: tables are `REFLECTED`, `DELEGATED`, or `AUTHORED`; columns carry `VENDOR`/`DEFINED`/`PUBLISHED` provenance; policy vocabularies stay `[SmartEnum]`; standards-data enums become `readonly record struct` row tables; every seed row flows through `ComponentFamily.Rows -> Component.Of -> SectionSolver.Solve` as `Fin<Seq<ComponentRow>>`.
