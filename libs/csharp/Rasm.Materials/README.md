# [MATERIALS]

`Rasm.Materials` is the host-neutral AEC-domain materials PROJECTOR onto the shared `Rasm.Element` seam, owning architectural materials across six sub-domains. One `Projection/material#MATERIAL_PROJECTOR` `MaterialProjector` implements the seam's single `IElementProjection` contract with one `Fin<GraphDelta> Project(ProjectionContext ctx)` op, capturing every catalogue below internally and lowering it into one additive `GraphDelta` of content-keyed seam `Material`/`Appearance`/`Assessment` nodes the seam's `Assemble` fold merges with every sibling projector's delta — usable in ISOLATION (a content-keyed material subgraph with no dangling element edge) yet ALIGNED-not-coupled (peers never reference each other; alignment travels through the `Rasm.Element` contracts, and the element→material `Associate` edge is authored only when the context vouches for the element NodeId [H12]). `Profiles/` owns one polymorphic `Profile` over a closed `ProfileFamily` axis — masonry, steel, CMU, timber, and glazing realized — with the steel family grounded in the `VividOrange.Profiles.Catalogue` published AISC v16.0 + EN 10365 section database and EVERY family's section properties computed by the one shared `ParametricSection`/`SectionReader` `VividOrange.Sections.SectionProperties` polygon integral, never a hand-keyed section literal, and `Profiles/capacity` owns the one `SectionCapacity` `[Union]` surface — the reinforced-concrete elastic transformed-section (`ConcreteSectionProperties`), the ultimate biaxial N-M-M capacity hull (`VividOrange.InteractionDiagram` fibre-integration over the `IForceMomentMesh`), and the rolled-steel LRFD receipt — folded against one `Demand`/`Utilisation` rail. The reinforced-concrete SECTION the capacity solvers consume is assembled by the `Connection/reinforcement` `RcSection` owner: the `VividOrange.Sections` `ConcreteSection` + `Rebar`/`Link` + `FaceReinforcementLayer`/`PerimeterReinforcementLayer`/`ReinforcementLayoutByCount`/`ReinforcementLayoutBySpacing`/`MinimumReinforcementSpacing` rebar-layout engines over the EN-10080 `BarDiameter` catalogue, the EN/Eurocode material grades composed from `VividOrange.Materials` (`EnConcreteMaterial`/`EnRebarMaterial`) bound to their `VividOrange.Standards` `En1992` citation instead of hand-keyed grade scalars. `Connection/` owns one polymorphic `ConnectionItem` over the `ConnectionFamily` axis — reinforcement, fastener, hanger, joint — with anchor folded as a `FastenerKind` arm and `joint` the continuous weld/adhesive/stud sibling; the catalogue serializes over the BIM wire as the IFC `IfcReinforcingBar`/`IfcMechanicalFastener` element. `Appearance/` owns one measured appearance engine: a node `MaterialGraph`, a closed seven-lobe `BsdfLobe` family lowered from the OpenPBR Surface 1.1 `SlabStack`, a `MaterialLibrary` row table grounded by the measured conductor complex-IOR table with the Pointer real-surface gamut and CVD-preview seam, procedural texture and photometric admission, the Kubelka-Munk pigment/coat-stack finish engine, the weathering aging operator, measured-material acquisition import, and the OpenPBR/MaterialX wire vocabulary host-free peers decode. `Construction/` owns the host-neutral material-composition author and layout model — the `MATERIAL_COMPOSITION` author building the seam `MaterialComposition` (single/layer-set/profile-set(`ProfileRef`)/constituent-set) plus the `LayerSetUsage`/`ProfileSetUsage` occurrence payloads the seam `Associate` edge carries [C7], the `RunPath`/`Placement` host-neutral run geometry the one `ConstructionLayout.Resolve` placement fold produces, plus the `Nesting/` cutting-stock owner — one `StockNest.Pack` fold over the `NestStrategy` `[Union]` collapsing the five `RectangleBinPack.CSharp` packers (max-rects/skyline/guillotine/shelf/mass-cut) into one sheet-yield engine with a typed `NestYield` material-utilization receipt, disjoint from `Rasm.Fabrication`'s true-shape nesting kernel at the strata wall. `Properties/` owns the typed engineering-property CATALOGUE — mechanical, thermal, acoustic over per-octave-band spectra, fire over the IFC `IfcMaterialProperties` set, and the sustainability discipline of embodied-carbon/cost/classification — lowered by the projector into the seam `Discipline`-keyed `MaterialPropertySet` cases the `Material` node carries AND the `Discipline`-keyed `Assessment` INPUT payloads the projector attaches; the intrinsic single-material acoustic folds (`Nrc`/`Saa`/`StcWeighted` over the shared `RatingContour.Stc.Fit` contour kernel) are SEAM-owned (`Rasm.Element` `Composition/acoustic`, referenced never re-authored) and the multi-ply `AssemblyAggregator` (series-resistance U-value, layered STC, rule-of-mixtures, GWP/cost) is `Rasm.Compute`'s — Materials owns the single-material property SOURCE, never the assembly aggregation. A material is a LIBRARY ROW, an appearance variation a NODE CASE, a lobe a `[Union]` CASE, a cross-section a `ProfileFamily` ROW resolved one-hop to a section through a seam `ProfileRef` [M7], a connection a `ConnectionFamily` ROW, a layout the ONE placement fold, and the whole material subgraph the ONE `MaterialProjector.Project` over the `Rasm.Element` seam — never a per-material or per-family type. The package projects onto the `Rasm.Element` seam for the canonical element graph (the `Material`/`MaterialComposition`/`MaterialPropertySet`/`Discipline`/`Relationship`/`ContentAddress` vocabulary), composes the `Rasm` kernel for vector/dimension value-objects and the seed-zero `XxHash128` content seed the seam `ContentAddress` composes, consumes `Wacton.Unicolour` as its scene-linear/spectral color owner, and admits `UnitsNet` IN-FOLDER for the photometric/appearance unit coercion (the engineering-property SI coercion now rides the seam `MeasureValue`; the strata-acyclic AEC-domain never reaches DOWN to the app-platform `Rasm.Compute` units owner), never re-minting a vector, a color space, a unit owner, or a seam type. The sub-domain map and domain law live in `ARCHITECTURE.md`, the forward concept pool in `IDEAS.md`, and the work log in `TASKLOG.md`.

## [01]-[ROUTER]

- [01]-[PROFILE](.planning/Profiles/profile.md)
- [02]-[MASONRY](.planning/Profiles/masonry.md)
- [03]-[STEEL](.planning/Profiles/steel.md)
- [04]-[CMU](.planning/Profiles/cmu.md)
- [05]-[TIMBER](.planning/Profiles/timber.md)
- [06]-[GLAZING](.planning/Profiles/glazing.md)
- [07]-[CAPACITY](.planning/Profiles/capacity.md)
- [08]-[CONNECTION](.planning/Connection/connection.md)
- [09]-[REINFORCEMENT](.planning/Connection/reinforcement.md)
- [10]-[FASTENER](.planning/Connection/fastener.md)
- [11]-[HANGER](.planning/Connection/hanger.md)
- [12]-[JOINT](.planning/Connection/joint.md)
- [13]-[BSDF](.planning/Appearance/bsdf.md)
- [14]-[GRAPH](.planning/Appearance/graph.md)
- [15]-[TEXTURE](.planning/Appearance/texture.md)
- [16]-[PHOTOMETRIC](.planning/Appearance/photometric.md)
- [17]-[WEATHERING](.planning/Appearance/weathering.md)
- [18]-[ACQUISITION](.planning/Appearance/acquisition.md)
- [19]-[FINISH](.planning/Appearance/finish.md)
- [20]-[INTERCHANGE](.planning/Appearance/interchange.md)
- [21]-[SURFACE](.planning/Appearance/surface.md)
- [22]-[ASSEMBLY](.planning/Construction/assembly.md)
- [23]-[LAYOUT](.planning/Construction/layout.md)
- [24]-[NESTING](.planning/Construction/nesting.md)
- [25]-[PROPERTIES](.planning/Properties/properties.md)
- [26]-[SUSTAINABILITY](.planning/Properties/sustainability.md)
- [27]-[PROJECTION](.planning/Projection/material.md)

## [02]-[DOMAIN_PACKAGES]

Domain packages admitted by this folder; versions are centralized in the one C# manifest and corroborated by this folder's `.api/`.

[COLOR_SPECTRAL]:
- `Wacton.Unicolour`
- `Wacton.Unicolour.Datasets`

[NUMERICS]:
- `MathNet.Numerics`

[SECTION_CATALOGUE]:
- `VividOrange.Profiles.Catalogue` — published AISC Shapes Database v16.0 (2299 American) + EN 10365-2017 (558 European) structural sections as typed profile classes, each carrying real section geometry as `UnitsNet.Length` over the full IFC profile-family set; grounds the `Profiles/` steel and EN section-geometry seed in the registered published data instead of hand-keyed rows, composing the in-folder `UnitsNet` quantity owner.
- `VividOrange.Sections.SectionProperties` — arbitrary closed-polygon section-property SOLVER, the computation complement to the catalogue data: feeds any `IPerimeter` (outer polyline + void edges) and returns `Area`, elastic `Centroid`, `MomentOfInertia` Yy/Zz, `ElasticSectionModulus` Yy/Zz, `RadiusOfGyration` Yy/Zz, and `Perimeter` via a shoelace/Green's-theorem polygon integral with void subtraction and parallel-axis transfer; one solver composes over every `ProfileFamily` (timber, CMU, masonry, composite, cold-formed) instead of per-family rectangular literals, composing the same in-folder `UnitsNet` quantity owner. Same `VividOrange`/MagmaWorks MIT taxonomy as the catalogue.
- `VividOrange.Sections` — the concrete `ConcreteSection`/`Section` + reinforcement (`Rebar`/`Link`/`LongitudinalReinforcement` plus the `ReinforcementLayoutByCount`/`ReinforcementLayoutBySpacing`/`FaceReinforcementLayer`/`PerimeterReinforcementLayer`/`MinimumReinforcementSpacing` layout engines and the EN-10080 `BarDiameter` D6..D50 catalogue) implementations of the already-pinned `VividOrange.ISections` interface floor. Closes the reinforced-concrete phantom the admitted `VividOrange.Sections.SectionProperties` `ConcreteSectionProperties` transformed-section solver requires, making the RC section-property path constructible end-to-end, and owns the reinforced-concrete section + rebar arrangement the `Connection/reinforcement` seam composes instead of a hand-rolled `BarSize`/`RebarSection` table. All transitive deps (`IMaterials`/`IProfiles`/`Profiles.Perimeter`/`ISections`/`Geometry`) already pinned; pure-managed AnyCPU, MIT.

[STRUCTURAL_MATERIAL_DATA]:
- `VividOrange.Materials` — the concrete EN/Eurocode structural-material grade DATA owner of the in-folder `VividOrange.IMaterials` interface floor: `EnConcreteMaterial`/`EnSteelMaterial`/`EnRebarMaterial` with the `EnConcreteFactory`/`EnSteelFactory`/`EnRebarFactory` grade->property tables (EN 1992-1-1 / EN 1993-1-1 Table 3.1, partial factors gamma_c/gamma_M0/gamma_M1/gamma_M2, `EnSteelSpecification` EN 10025-5 corrosion rules) plus the `BiLinearMaterial`/`LinearElasticMaterial`/`LinearElasticOrthotropicMaterial`/`ParabolaRectangleMaterial` constitutive models via `AnalysisMaterialFactory`. Replaces hand-keyed material-grade scalars with registered grade->fck/fy/E + full stress-strain + partial-factor data, supplies the `IMaterial` of `MaterialType.Reinforcement` the `VividOrange.Sections` `Rebar`/`ConcreteSection` consume, and feeds the transformed-section modular-ratio computation. Same `VividOrange`/MagmaWorks MIT taxonomy; composes the in-folder `UnitsNet`.
- `VividOrange.Standards` — the Eurocode standard/identity DATA owner of the in-folder `VividOrange.IStandards` interface floor: `En1990` (basis), `En1991` (actions), `En1992` (concrete), `En1993` (steel, incl. Part 1-8 joints and 1-3 cold-formed), `En1994` (composite), `En1995` (timber), `En1996` (masonry), `En1997` (geotechnical), `En1998` (seismic), `En1999` (aluminium) as typed `IStandard` data, plus per-code utilities and `NationalAnnexUtility` (37 EU national-annex bodies). The `IStandard` reference the material grades cite and the design pages name instead of inline code literals; also the hard transitive floor of `VividOrange.Materials`. MIT, pure-managed.

[SECTION_CAPACITY_ENGINE]:
- `VividOrange.InteractionDiagram` — the RC/steel column biaxial Force-Moment-Moment (N-M-M / P-M onion) capacity-SURFACE engine: a TriangleNet-meshed concrete+rebar section, parallel strain-plane sweep, fibre-stress integration over the compression zone, and the emitted `IForceMomentMesh` 3D capacity hull. The additive reinforced-concrete interaction-diagram capability the elastic-only `VividOrange.Sections.SectionProperties` (`Area`/`I`/`S`/`r`) does not compute, composing the admitted `VividOrange.Sections` + `VividOrange.Materials`. Closure packages `VividOrange.ForceMomentInteraction` (the material-agnostic `AnalyticalFace` fibre-integration solver), `VividOrange.IForceMomentInteraction` (the `IForceMomentMesh`/`IForceMomentVertex`/`IForceMomentTriFace` interface surface), and `VividOrange.Serialization` (the `ITaxonomySerializable` impl, distinct from the interface-only `VividOrange.ISerialization`) ride the manifest as transitive floors, alongside the forced non-framework floors `MIConvexHull` (the N-M-M onion hull builder), `UnitsNet.Serialization.JsonNet` (the UnitsNet<->Json.NET converter), and `Triangle` (the Triangle.NET Delaunay section mesher). All MIT/MIT-0, pure-managed AnyCPU.

[PROPERTY_UNCERTAINTY]:
- `VividOrange.Uncertainties` — scalar uncertainty arithmetic for material-property values: absolute, relative, interval, and normal-distribution uncertainty models over central values, with propagation operations carried by the Materials property rows instead of ad hoc tolerance fields.
- `VividOrange.Uncertainties.Quantities` — UnitsNet quantity uncertainty arithmetic for measured and declared material properties: pressure, density, modulus, thermal, acoustic, fire, carbon, cost, and profile-derived quantities keep value, unit, and uncertainty together through the Properties and Profiles surfaces. `VividOrange.IUncertainties` rides the manifest as the interface floor.

[LAYOUT_PACKING]:
- `RectangleBinPack.CSharp` — `Construction/` 2D rectangle bin-packing / cutting-stock owner for sheet/stock-material arrangement and material-utilization: the full canonical port of Jukka Jylanki's RectangleBinPack exposing `MaxRects`/`Skyline`/`Guillotine`/`Shelf` algorithm families plus `SingleBinPack` mass-cut, each over explicit heuristic enums (the Guillotine split-heuristic is the panel-saw straight-cut constraint, `SingleBin` the mass-cut-identical-parts sheet-yield case). The in-folder packing pin the strata law requires — Materials, an AEC-DOMAIN peer, cannot reference `Fabrication`'s `RectpackSharp`/`Clipper2` nesting kernel; AEC peers depend only upward to the `Rasm` kernel. Pure-managed AnyCPU, single netstandard2.0 TFM, zero dependencies.

[PROJECTS]:
- `Rasm` — the kernel: vector/dimension value-objects, the `IObjectFactory` admission floor, and the generic seed-zero `XxHash128` content-hash entry the seam `ContentAddress` composes.
- `Rasm.Element` — the shared lowest-AEC element seam this folder projects onto: the `ElementGraph`/`Node`/`NodeId`/`Relationship` graph algebra, the `MaterialComposition`/`MaterialPropertySet`/`Discipline` material vocabulary (with the intrinsic acoustic folds), the `ProfileRef` one-hop section handle, the `Assessment` node, and the one `IElementProjection` contract `MaterialProjector` implements. AEC peers depend up on `{Rasm, Rasm.Element}` and never reference each other; alignment is by these contracts, not sibling coupling.

## [03]-[SUBSTRATE_PACKAGES]

Substrate packages from the C# registry consumed by this folder; full registry and substrate contracts live in [`libs/csharp/.planning/README.md`](../.planning/README.md), with shared API evidence in `libs/csharp/.api/`.

[FUNCTIONAL_CORE]:
- `LanguageExt.Core`
- `Thinktecture.Runtime.Extensions`
- `Thinktecture.Runtime.Extensions.Json`
- `Thinktecture.Runtime.Extensions.MessagePack`
- `JetBrains.Annotations`

[NUMERIC_SUBSTRATE]:
- `UnitsNet`
