# [MATERIALS]

Three layers separate substance from buildable from sited. A **Material** is pure substance — homogeneous, no geometry: a steel grade, a concrete mix, a mineral-wool fill. A **Component** is the placement-free TYPE that marries a parametric cross-section or profile to a material and owns its complete shape. An **Element** is the sited occurrence the `Rasm.Element` seam bakes from a Component. `Rasm.Materials` owns the first two — the substance CATALOGUE (`Properties/`) and the buildable Component TYPE (`Component/`) — and projects them onto the seam as `Material` nodes and deterministic-rooted Type `Object`s; `Rasm.Bim` ingests the same Types from IFC, `Rasm.Compute` reads the property sets for analysis, and the downstream generation lib reads a Component's captured geometry to author real solids.

A Component carries the COMPLETE generative parametric data a generator builds geometry from, not the handful of scalars an analysis reads: a CMU is its per-cell geometry, web split, grout cells, and special-unit shape; a fastener its thread form, head, nut, and washer; a glazing unit its pane dimensions, spacer, interlayer, and gas fill. One owner serves analysis AND generation. A thin entry — the obvious three fields where the concept carries fifteen, a placeholder dimension, a scalar smeared in place of real geometry — starves the generator and is a DEFECT, never a simplification: a Component models its concept to completion or it does not model it. The same bar governs the substance catalogue — a `Properties` row is the full mechanical, thermal, acoustic, fire, and environmental signature of a grade, not a name and a density.

The buildable roster is open and grows by ROW inside the closed owners (the enumeration below), never as a parallel type. The `ComponentFamily` axis is closed at TEN — the panel/sheet-goods family the assembly use-cases (a stud wall, a sheathed frame, a floor deck, a membrane roof) demanded is now the tenth row: gypsum board (drywall/sheetrock), gypsum and wood-structural sheathing (plywood / OSB), cement board, steel deck, membrane sheet goods, and rigid-board insulation are buildable `Component/panel#PANEL_FAMILY` Components, each carrying its generative fields — board length/width/thickness, edge profile, face/core layer stack, fastening field/edge pattern, orientation, membrane seam, and the deck rib geometry where structural — with the board's substance resolving to a `Properties` material row. A panel is a Component, not a Property: gypsum lives in the `Properties` catalogue as a SUBSTANCE while the buildable board an assembly lays up is the `Component` in the `ComponentFamily.Panel` row (`ComponentClass.Panel`, the many-pieces-per-type `IfcBuiltElement` covering/plate/slab), so a gypsum-over-stud partition, a plywood-sheathed shear wall, a steel-deck floor, and an EPDM/PVC/TPO roof membrane place through the SAME product-form family; roof/wall/floor are layout/use roles, never Component families.

- `Component.SubstanceId` is the canonical material identity — engineering, lifecycle, classification, cost; `AppearanceId` independent visual/finish identity.
- `Panel` is the sheet/deck/membrane product-form family. Roof, wall, and floor are layout/use roles outside `ComponentFamily`.
- `Properties/` has two owners: intrinsic engineering measurements and lifecycle/cost-basis/classification; assembly performance is computed above Materials.
- Standards data lives in-fence C# under `SEED_ROW_LAW`: each table is sourced `REFLECTED` (a foreign machine-readable schema) over `DELEGATED` (an admitted vendor factory — steel via `CatalogueFactory`, EN grades via `EnSteelFactory`/`EnConcreteFactory`/`EnRebarFactory`) over `AUTHORED` (no producer — the masonry/CMU/timber/glazing/fastener/panel/EPD rosters, verbatim); every value carries `VENDOR`/`DEFINED`/`PUBLISHED` column provenance; policy vocabularies stay `[SmartEnum]` while pure standards-data enums are frozen `readonly record struct` row tables; and every seed row flows through the ONE `ComponentFamily.Rows -> Component.Of -> SectionSolver.Solve` generator as a fail-loud `Fin<Seq<ComponentRow>>` built by `Traverse`.

`Rasm.Materials` is the host-neutral AEC-domain materials PROJECTOR onto the shared `Rasm.Element` seam, owning architectural materials across four sub-domains. One `Projection/component#COMPONENT_PROJECTOR` `ComponentProjector` — the merge of the prior `MaterialProjector` and `ConnectionProjector` — implements the seam's single `IElementProjection` contract with one `Fin<GraphDelta> Project(ProjectionContext ctx)` op whose fold discriminates a pure-substance `MaterialSpec` arm from a Type-minting `ComponentSpec` arm: the substance arm lowers every catalogue below into content-keyed seam `Material`/`Appearance` nodes, and the Type arm MINTS the deterministic-rooted Type `Object` from the `Component`'s canonical content (the owner-mints-its-identity law — `Rasm.Materials` owns Component TYPES), stamps its `Classification`/`PredefinedType` off the stored `IfcBinding` row (`Component.IfcEntity`/`PredefinedToken` field reads), bakes its `ComputedSection` onto the structural composition, authors the neutral realization-detail bag, and binds vouched occurrences via `Assign.TypeDefinition` — all in ONE additive `GraphDelta` the seam's `Assemble` fold merges with every sibling projector's delta, usable in ISOLATION (a content-keyed subgraph with no dangling element edge) yet ALIGNED-not-coupled (peers never reference each other; alignment travels through the `Rasm.Element` contracts, and the element→material `Associate` edge is authored only when the context vouches for the element NodeId [H12]). `Component/` owns one polymorphic `Component` over a closed `ComponentFamily` `[SmartEnum<string>]` axis closed at TEN — masonry · cmu · steel · timber · glazing · reinforcement · fastener · connector · joint · panel — each carrying its `ComponentClass` primary/panel/minor discriminant (the primary space-bounding members steel/timber projecting the `IfcBuiltElement` supertype with geometry-on-type and one-occurrence-one-piece, the panel sheet-goods family projecting the `IfcBuiltElement` covering/plate/slab supertype with one-type-many-pieces, the minor standardized parts projecting `IfcElementComponent` with one-type-many-pieces), with the steel family grounded in the `VividOrange.Profiles.Catalogue` published AISC v16.0 + EN 10365 section database and EVERY `Sectioned` row's section properties computed by the ONE `SectionSolver.Solve` exhaustive dispatch over the closed `SectionProfile` `[Union]` algebra — the `VividOrange.Sections.SectionProperties` polygon integral over typed `VividOrange.IProfiles` contracts plus the `Forms` closed-form supplements (the steel-deck rib a `Sectioned` `Component/panel#PANEL_FAMILY` row the same solver fills), never a hand-keyed section literal or per-family perimeter builder; `Component/capacity` owns the one `SectionCapacity` `[Union]` surface — the reinforced-concrete elastic transformed-section (`ConcreteSectionProperties`), the ultimate biaxial N-M-M capacity hull (`VividOrange.InteractionDiagram` fibre-integration over the `IForceMomentMesh`), the rolled-steel LRFD receipt, and the timber/masonry cases — folded against one `Demand`/`Utilisation`/`GoverningAction` rail, the reinforced-concrete SECTION assembled by the family-agnostic `Component/reinforcement` `RcSection` owner over the `VividOrange.Sections` `ConcreteSection` + `Rebar`/`Link` + `FaceReinforcementLayer`/`PerimeterReinforcementLayer`/`ReinforcementLayoutByCount`/`ReinforcementLayoutBySpacing`/`MinimumReinforcementSpacing` rebar-layout engines over the EN-10080 `BarDiameter` catalogue, the EN/Eurocode material grades composed from `VividOrange.Materials` (`EnConcreteMaterial`/`EnRebarMaterial`) bound to their `VividOrange.Standards` `En1992` citation instead of hand-keyed grade scalars. A `Component` is NEVER a per-material class — a brick, a steel section, a #5 rebar, a joist connector, an 8 mm fillet weld, a gypsum board differ only in the `SectionProfile` `[Union]` arm (a `Component` FIELD, never a peer) and the `ComponentFamily` policy row — and `anchor` folds as a `FastenerKind` arm inside the fastener vocabulary, never a family of its own; `joint` is the deliberate continuous weld/adhesive/stud widening, `connector` the fabricated-framing-hardware family (the `hanger`→`connector` rename), `panel` the sheet-goods family (gypsum/sheathing/plywood/OSB/cement-board/steel-deck/rigid-board as buildable boards, the board substance a `Properties` row, the `Component/panel#PANEL_FAMILY` board geometry riding `SectionProfile.Layered`/`Corrugated` and `PanelSeed.Rows` folding the board generative data the `RASM-GENERATION-SPEC.md` `[04]`/`[05]` sheathing run lays over a frame). `Appearance/` owns one measured appearance engine: a node `MaterialGraph`, a closed seven-lobe `BsdfLobe` family lowered from the OpenPBR Surface 1.1 `SlabStack`, a `MaterialLibrary` row table grounded by the measured conductor complex-IOR table with the Pointer real-surface gamut and CVD-preview seam, procedural texture and photometric admission, the Kubelka-Munk pigment/coat-stack finish engine, the weathering aging operator, measured-material acquisition import, and the OpenPBR/MaterialX wire vocabulary host-free peers decode. The former `Construction/` sub-domain is fully dispersed: the cutting-stock nesting engine MOVED to `Rasm.Fabrication` `Nesting/stock` (the `RectangleBinPack.CSharp` pin departing with it); the seam `MaterialComposition` author (single/layer-set/profile-set(`ProfileRef`)/constituent-set plus the `LayerSetUsage`/`ProfileSetUsage` occurrence payloads the seam `Associate` edge carries [C7]) re-homed into `Projection/component#COMPOSITION_AUTHOR`, and the host-neutral run/layout geometry (`RunPath`/`Placement` streams, the placement fold over any `ComponentFamily`) landed in `RASM-GENERATION-SPEC.md` `[04]`/`[05]`. `Properties/` owns the typed engineering-property CATALOGUE — mechanical, thermal, acoustic over per-octave-band spectra, fire over the IFC `IfcMaterialProperties` set, and the sustainability discipline of embodied-carbon/cost/classification — lowered by the projector into the seam `Discipline`-keyed `MaterialPropertySet` cases the `Material` node carries; the intrinsic single-material acoustic folds (`Nrc`/`Saa`/`StcWeighted` over the shared `RatingContour.Fit` contour kernel) are SEAM-owned (`Rasm.Element` `Composition/acoustic`, referenced never re-authored) and the multi-ply `AssemblyAggregator` (series-resistance U-value, layered STC, rule-of-mixtures, GWP/cost) is `Rasm.Compute`'s — Materials owns the single-material property SOURCE, never the assembly aggregation. A material is a LIBRARY ROW, an appearance variation a NODE CASE, a lobe a `[Union]` CASE, a cross-section a `ComponentFamily` ROW resolved one-hop to a section through a seam `ProfileRef` [M7], a standardized part a `ComponentFamily` ROW, and the whole material-and-Type subgraph the ONE `ComponentProjector.Project` over the `Rasm.Element` seam — never a per-material or per-family type. The package projects onto the `Rasm.Element` seam for the canonical element graph (the `Material`/`MaterialComposition`/`MaterialPropertySet`/`Discipline`/`Relationship`/`ContentAddress`/`ObjectKind`/`DetailSchema` vocabulary), composes the `Rasm` kernel for the vector/dimension value-objects (`PositiveMagnitude`/`Dimension`/`UnitInterval` in `Rasm.Numerics`) and the seed-zero `XxHash128` content seed the seam `ContentAddress` composes, consumes `Wacton.Unicolour` as its scene-linear/spectral color owner, and admits `UnitsNet` IN-FOLDER for the photometric/appearance unit coercion (the engineering-property SI coercion now rides the seam `MeasureValue`; the strata-acyclic AEC-domain never reaches DOWN to the app-platform `Rasm.Compute` units owner), never re-minting a vector, a color space, a unit owner, or a seam type. The sub-domain map and domain law live in `ARCHITECTURE.md`, the forward concept pool in `IDEAS.md`, and the work log in `TASKLOG.md`.

## [01]-[ROUTER]

- [01]-[COMPONENT](.planning/Component/component.md)
- [02]-[MASONRY](.planning/Component/masonry.md)
- [03]-[STEEL](.planning/Component/steel.md)
- [04]-[CMU](.planning/Component/cmu.md)
- [05]-[TIMBER](.planning/Component/timber.md)
- [06]-[GLAZING](.planning/Component/glazing.md)
- [07]-[REINFORCEMENT](.planning/Component/reinforcement.md)
- [08]-[FASTENER](.planning/Component/fastener.md)
- [09]-[CONNECTOR](.planning/Component/connector.md)
- [10]-[JOINT](.planning/Component/joint.md)
- [11]-[PANEL](.planning/Component/panel.md)
- [12]-[CAPACITY](.planning/Component/capacity.md)
- [13]-[BSDF](.planning/Appearance/bsdf.md)
- [14]-[GRAPH](.planning/Appearance/graph.md)
- [15]-[SURFACE](.planning/Appearance/surface.md)
- [16]-[TEXTURE](.planning/Appearance/texture.md)
- [17]-[PHOTOMETRIC](.planning/Appearance/photometric.md)
- [18]-[WEATHERING](.planning/Appearance/weathering.md)
- [19]-[ACQUISITION](.planning/Appearance/acquisition.md)
- [20]-[FINISH](.planning/Appearance/finish.md)
- [21]-[INTERCHANGE](.planning/Appearance/interchange.md)
- [22]-[PROPERTIES](.planning/Properties/properties.md)
- [23]-[SUSTAINABILITY](.planning/Properties/sustainability.md)
- [24]-[PROJECTION](.planning/Projection/component.md)

## [02]-[DOMAIN_PACKAGES]

Domain packages admitted by this folder; versions are centralized in the one C# manifest and corroborated by this folder's `.api/`.

[COLOR_SPECTRAL]:
- `Wacton.Unicolour`
- `Wacton.Unicolour.Datasets`

[NUMERICS]:
- `MathNet.Numerics`

[SECTION_CATALOGUE]:
- `VividOrange.Profiles.Catalogue` — AISC v16.0 + EN 10365 typed profiles with `UnitsNet.Length` geometry; grounds the section seed in published data.
- `VividOrange.Sections.SectionProperties` — polygon section solver: `IPerimeter` → the 20-field `ComputedSection`; one solver over every `ComponentFamily`.
- `VividOrange.Sections` — `ConcreteSection`/`Section` + rebar layout engines over the pinned `ISections` floor; composed by the `RcSection` assembler.

[STRUCTURAL_MATERIAL_DATA]:
- `VividOrange.Materials` — EN/Eurocode grade data over the `IMaterials` floor: grade→property factories, partial factors, and the constitutive-model family.
- `VividOrange.Standards` — Eurocode data over the `IStandards` floor: `En1990`..`En1999` typed rows + `NationalAnnexUtility`; cited, never inline literals.

[SECTION_CAPACITY_ENGINE]:
- `VividOrange.InteractionDiagram` — biaxial N-M-M capacity surface: strain sweep + fibre integration to the `IForceMomentMesh` hull; closure floors pinned.

[PROPERTY_UNCERTAINTY]:
- `VividOrange.Uncertainties` — scalar uncertainty arithmetic (absolute/relative/interval/normal); propagation rides the property rows, never tolerance fields.
- `VividOrange.Uncertainties.Quantities` — UnitsNet quantity uncertainty; value/unit/uncertainty travel the property surfaces; `IUncertainties` the floor.

[WIRE_SERIALIZERS]:
- `MessagePack` — binary appearance-interchange wire: source-generated resolver, `Lz4BlockArray`, `UntrustedData` hardening; analyzer proves `[Key]` coverage.

[PROJECTS]:
- `Rasm` — the kernel: `Rasm.Numerics` value-objects, the `IObjectFactory` admission floor, and the seed-zero `XxHash128` hash entry `ContentAddress` composes.
- `Rasm.Element` — the projected element seam: graph algebra, material vocabulary, section handle, `DetailSchema` bag, `IElementProjection`; peers never couple.

## [03]-[SUBSTRATE_PACKAGES]

Substrate packages from the C# registry consumed by this folder; full registry and substrate contracts live in [`libs/csharp/.planning/README.md`](../.planning/README.md), with shared API evidence in `libs/csharp/.api/`.

[FUNCTIONAL_CORE]:
- `LanguageExt.Core`
- `Thinktecture.Runtime.Extensions`
- `Thinktecture.Runtime.Extensions.Json`
- `Thinktecture.Runtime.Extensions.MessagePack`
- `Riok.Mapperly` — boundary transcription: `WireMap` `OpenPbrSurface`→`OpenPbrGroupsWire` + `Provenance`→`WireProvenance` under the RMG completeness gate.
- `JetBrains.Annotations`

[NUMERIC_SUBSTRATE]:
- `UnitsNet`

[MEMORY_PLANES]:
- `CommunityToolkit.HighPerformance` — dense planes: mips via `AsMemory2D` into a `ReadOnlyMemory2D<ShadeVec4>` pyramid; samplers read spans, never offsets.

[GRAPH_ALGORITHM]:
- `QuikGraph` — `Appearance/graph` folds the DAG into `AdjacencyGraph<PortId, SEdge<PortId>>` + `SourceFirstTopologicalSort`, replacing the Kahn walk.
