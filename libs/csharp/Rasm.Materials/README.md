# [MATERIALS]

`Rasm.Materials` is the host-neutral AEC-domain owner of architectural materials across five sub-domains. `Profiles/` owns one polymorphic `Profile` over a closed `ProfileFamily` axis — masonry, steel, CMU, timber, and glazing realized — with the steel family grounded in the `VividOrange.Profiles.Catalogue` published AISC v16.0 + EN 10365 section database and EVERY family's section properties computed by the one shared `ParametricSection`/`SectionReader` `VividOrange.Sections.SectionProperties` polygon integral, never a hand-keyed section literal. `Connection/` owns one polymorphic `ConnectionItem` over the `ConnectionFamily` axis — reinforcement, fastener, hanger, joint — with anchor folded as a `FastenerKind` arm and `joint` the continuous weld/adhesive/stud sibling; the catalogue serializes over the BIM wire as the IFC `IfcReinforcingBar`/`IfcMechanicalFastener` element. `Appearance/` owns one measured appearance engine: a node `MaterialGraph`, a closed seven-lobe `BsdfLobe` family lowered from the OpenPBR Surface 1.1 `SlabStack`, a `MaterialLibrary` row table grounded by the measured conductor complex-IOR table with the Pointer real-surface gamut and CVD-preview seam, procedural texture and photometric admission, the Kubelka-Munk pigment/coat-stack finish engine, the weathering aging operator, measured-material acquisition import, and the OpenPBR/MaterialX wire vocabulary host-free peers decode. `Construction/` owns the host-neutral element-to-assembly-to-layout data model, with materials assigned by the IFC 4.3 layer-set/profile-set/constituent-set trichotomy and resolved to portable scalar placements, plus the `Nesting/` cutting-stock owner — one `StockNest.Pack` fold over the `NestStrategy` `[Union]` collapsing the five `RectangleBinPack.CSharp` packers (max-rects/skyline/guillotine/shelf/mass-cut) into one sheet-yield engine with a typed `NestYield` material-utilization receipt, disjoint from `Rasm.Fabrication`'s true-shape nesting kernel at the strata wall. `Properties/` owns the typed `MaterialProperty` engineering-property family — mechanical, thermal, acoustic over per-octave-band spectra, fire over the IFC `IfcMaterialProperties` set, and the sustainability discipline of embodied-carbon/cost/classification — plus the `AssemblyProperty` series-resistance/rule-of-mixtures/layered-STC and lifecycle GWP/cost aggregation folds. A material is a LIBRARY ROW, an appearance variation a NODE CASE, a lobe a `[Union]` CASE, a cross-section a `ProfileFamily` ROW, a connection a `ConnectionFamily` ROW, a layout the ONE placement fold, and an assembly property the ONE aggregation fold — never a per-material or per-family type. The package composes the `Rasm` kernel for vector/dimension value-objects, consumes `Wacton.Unicolour` as its scene-linear/spectral color owner, and admits `UnitsNet` IN-FOLDER for the photometric and engineering-property unit coercion (the strata-acyclic AEC-domain owns its own unit boundary; it never reaches DOWN to the app-platform `Rasm.Compute` units owner), never re-minting a vector, a color space, or a unit owner. The sub-domain map and domain law live in `ARCHITECTURE.md`, the forward concept pool in `IDEAS.md`, and the work log in `TASKLOG.md`.

## [01]-[ROUTER]

- [01]-[PROFILE](.planning/Profiles/profile.md)
- [02]-[MASONRY](.planning/Profiles/masonry.md)
- [03]-[STEEL](.planning/Profiles/steel.md)
- [04]-[CMU](.planning/Profiles/cmu.md)
- [05]-[TIMBER](.planning/Profiles/timber.md)
- [06]-[GLAZING](.planning/Profiles/glazing.md)
- [07]-[CONNECTION](.planning/Connection/connection.md): `ConnectionItem` `[Union]` catalogue over the `ConnectionFamily` axis.
- [08]-[REINFORCEMENT](.planning/Connection/reinforcement.md)
- [09]-[FASTENER](.planning/Connection/fastener.md)
- [10]-[HANGER](.planning/Connection/hanger.md)
- [11]-[JOINT](.planning/Connection/joint.md)
- [12]-[BSDF](.planning/Appearance/bsdf.md)
- [13]-[GRAPH](.planning/Appearance/graph.md)
- [14]-[TEXTURE](.planning/Appearance/texture.md)
- [15]-[PHOTOMETRIC](.planning/Appearance/photometric.md)
- [16]-[WEATHERING](.planning/Appearance/weathering.md)
- [17]-[ACQUISITION](.planning/Appearance/acquisition.md)
- [18]-[FINISH](.planning/Appearance/finish.md)
- [19]-[INTERCHANGE](.planning/Appearance/interchange.md)
- [20]-[SURFACE](.planning/Appearance/surface.md)
- [21]-[ASSEMBLY](.planning/Construction/assembly.md)
- [22]-[LAYOUT](.planning/Construction/layout.md)
- [23]-[NESTING](.planning/Construction/nesting.md): `StockNest.Pack` cutting-stock / sheet-yield fold over the `NestStrategy` `[Union]` collapsing the five `RectangleBinPack.CSharp` packers, with the `NestYield` material-utilization receipt.
- [24]-[PROPERTIES](.planning/Properties/properties.md)
- [25]-[SUSTAINABILITY](.planning/Properties/sustainability.md)

## [02]-[DOMAIN_PACKAGES]

Domain packages admitted by this folder; versions are centralized in the one C# manifest and corroborated by this folder's `.api/`.

[COLOR_SPECTRAL]:
- `Wacton.Unicolour`
- `Wacton.Unicolour.Datasets`

[UNITS]:
- `UnitsNet`

[NUMERICS]:
- `MathNet.Numerics`

[SECTION_CATALOGUE]:
- `VividOrange.Profiles.Catalogue` — published AISC Shapes Database v16.0 (2299 American) + EN 10365-2017 (558 European) structural sections as typed profile classes, each carrying real section geometry as `UnitsNet.Length` over the full IFC profile-family set; grounds the `Profiles/` steel and EN section-geometry seed in the registered published data instead of hand-keyed rows, composing the in-folder `UnitsNet` quantity owner.
- `VividOrange.Sections.SectionProperties` — arbitrary closed-polygon section-property SOLVER, the computation complement to the catalogue data: feeds any `IPerimeter` (outer polyline + void edges) and returns `Area`, elastic `Centroid`, `MomentOfInertia` Yy/Zz, `ElasticSectionModulus` Yy/Zz, `RadiusOfGyration` Yy/Zz, and `Perimeter` via a shoelace/Green's-theorem polygon integral with void subtraction and parallel-axis transfer; one solver composes over every `ProfileFamily` (timber, CMU, masonry, composite, cold-formed) instead of per-family rectangular literals, composing the same in-folder `UnitsNet` quantity owner. Same `VividOrange`/MagmaWorks MIT taxonomy as the catalogue.

[LAYOUT_PACKING]:
- `RectangleBinPack.CSharp` — `Construction/` 2D rectangle bin-packing / cutting-stock owner for sheet/stock-material arrangement and material-utilization: the full canonical port of Jukka Jylanki's RectangleBinPack exposing `MaxRects`/`Skyline`/`Guillotine`/`Shelf` algorithm families plus `SingleBinPack` mass-cut, each over explicit heuristic enums (the Guillotine split-heuristic is the panel-saw straight-cut constraint, `SingleBin` the mass-cut-identical-parts sheet-yield case). The in-folder packing pin the strata law requires — Materials, an AEC-DOMAIN peer, cannot reference `Fabrication`'s `RectpackSharp`/`Clipper2` nesting kernel; AEC peers depend only upward to the `Rasm` kernel. Pure-managed AnyCPU, single netstandard2.0 TFM, zero dependencies.

[PROJECTS]:
- `Rasm`

## [03]-[SUBSTRATE_PACKAGES]

Substrate packages from the C# registry consumed by this folder; full registry and substrate contracts live in [`libs/csharp/.planning/README.md`](../csharp/.planning/README.md) and this folder's `.api/`.

[FUNCTIONAL_CORE]:
- `LanguageExt.Core`
- `Thinktecture.Runtime.Extensions`
- `Thinktecture.Runtime.Extensions.Json`
- `Thinktecture.Runtime.Extensions.MessagePack`
- `JetBrains.Annotations`
