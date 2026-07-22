# [RASM_MATERIALS_API_VIVIDORANGE_SECTIONS_SECTIONPROPERTIES]

`VividOrange.Sections.SectionProperties` owns the closed-polygon section-property solver: one Green's-theorem integral with void subtraction and parallel-axis transfer computes every elastic section property over any `IProfile` as a `UnitsNet` quantity. `ConcreteSectionProperties` extends it over an `IConcreteSection` for transformed-section reinforcement, and the `.Utility` static kernels answer any single property without a carrier. It serves the Materials Profiles family axis, replacing every per-family rectangular section-property literal.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `VividOrange.Sections.SectionProperties`
- package: `VividOrange.Sections.SectionProperties` (MIT, MagmaWorks / VividOrange taxonomy)
- assembly: `VividOrange.Sections.SectionProperties`
- namespace: `VividOrange.Sections.SectionProperties`, `.Utility`
- asset: runtime library, pure-managed AnyCPU, no native RID asset; the `net10.0` consumer binds the `net9.0` asset
- rail: profiles (section computation)
- depends: the input, geometry, and quantity floors live outside this assembly
  - `ISectionProperties`: `VividOrange.Sections.ISectionProperties`
  - `IProfile`/`IPerimeter`/`ISection`: `VividOrange.IProfiles`, `VividOrange.Profiles.Perimeter`
  - `ILocalPoint2d`/`ILocalDomain2d`: `VividOrange.Geometry`
  - `IConcreteSection`/`IRebar`/`ILongitudinalReinforcement`/`SectionFace`: `VividOrange.ISections`
  - `Area`/`Volume`/`AreaMomentOfInertia`/`Length`/`Ratio`: `UnitsNet`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: section-property carriers and their floor contracts

`ConcreteSectionProperties` extends `SectionProperties` and adds `EffectiveDepth(SectionFace)` and `ReinforcementArea(SectionFace)` over an `IConcreteSection`; both carriers implement the `ITaxonomySerializable` marker and round-trip through `api-vividorange-serialization.md`.

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]             |
| :-----: | :--------------------------- | :------------ | :----------------------- |
|  [01]   | `SectionProperties`          | class         | polygon-integral results |
|  [02]   | `ConcreteSectionProperties`  | class         | reinforcement properties |
|  [03]   | `ISectionProperties`         | interface     | property surface         |
|  [04]   | `IConcreteSectionProperties` | interface     | reinforcement queries    |

[PUBLIC_TYPE_SCOPE]: `.Utility` static kernels (single-property, carrier-free)

Each kernel integrates one property directly from an `IProfile` or `IConcreteSection`; the carrier's lazy getters delegate to them, and `.Utility.Parts` (`TrapezoidalPart`, `EllipseQuarterPart`, `IPart`) is the closed-form part algebra the integral folds over.

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]              |
| :-----: | :------------------ | :------------ | :------------------------ |
|  [01]   | `Areas`             | static class  | void-subtracted area      |
|  [02]   | `Centroids`         | static class  | elastic centroid          |
|  [03]   | `Inertiae`          | static class  | centroidal second moments |
|  [04]   | `SectionModuli`     | static class  | elastic section moduli    |
|  [05]   | `RadiusOfGyrations` | static class  | radii of gyration         |
|  [06]   | `PerimeterLengths`  | static class  | perimeter length          |
|  [07]   | `Extends`           | static class  | bounding domain           |
|  [08]   | `Rebars`            | static class  | RC reinforcement results  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: solver construction and elastic properties

| [INDEX] | [SURFACE]                                   | [SHAPE]  | [CAPABILITY]                |
| :-----: | :------------------------------------------ | :------- | :-------------------------- |
|  [01]   | `new SectionProperties(IProfile)`           | ctor     | profile-backed solver       |
|  [02]   | `new SectionProperties(ISection)`           | ctor     | profile-and-material solver |
|  [03]   | `.Area -> Area`                             | property | void-subtracted area        |
|  [04]   | `.Centroid -> ILocalPoint2d`                | property | cached elastic centroid     |
|  [05]   | `.MomentOfInertiaYy -> AreaMomentOfInertia` | property | Yy second moment            |
|  [06]   | `.MomentOfInertiaZz -> AreaMomentOfInertia` | property | Zz second moment            |
|  [07]   | `.ElasticSectionModulusYy -> Volume`        | property | Yy elastic `I / c`          |
|  [08]   | `.ElasticSectionModulusZz -> Volume`        | property | Zz elastic `I / c`          |
|  [09]   | `.RadiusOfGyrationYy -> Length`             | property | Yy `sqrt(I / A)`            |
|  [10]   | `.RadiusOfGyrationZz -> Length`             | property | Zz `sqrt(I / A)`            |
|  [11]   | `.Perimeter -> Length`                      | property | section perimeter           |
|  [12]   | `.Extends -> ILocalDomain2d`                | property | cached bounding extents     |

[ENTRYPOINT_SCOPE]: reinforced-concrete surface (`ConcreteSectionProperties`)

Build a `ConcreteSection` from an admitted `IProfile`, EN material, and rebar layers (`api-vividorange-sections.md`), then `new ConcreteSectionProperties(section)` reads the transformed-section properties.

| [INDEX] | [SURFACE]                                                   | [SHAPE]  | [CAPABILITY]              |
| :-----: | :---------------------------------------------------------- | :------- | :------------------------ |
|  [01]   | `new ConcreteSectionProperties(IConcreteSection)`           | ctor     | transformed RC solver     |
|  [02]   | `.TotalReinforcementArea -> Area`                           | property | gross steel area          |
|  [03]   | `.ConcreteArea -> Area`                                     | property | net concrete area         |
|  [04]   | `.GeometricReinforcementRatio -> Ratio`                     | property | `As / Ac` steel ratio     |
|  [05]   | `.ReinforcementSecondMomentOfAreaYy -> AreaMomentOfInertia` | property | Yy reinforcement inertia  |
|  [06]   | `.ReinforcementSecondMomentOfAreaZz -> AreaMomentOfInertia` | property | Zz reinforcement inertia  |
|  [07]   | `.ReinforcementRadiusOfGyrationYy -> Length`                | property | Yy reinforcement gyration |
|  [08]   | `.ReinforcementRadiusOfGyrationZz -> Length`                | property | Zz reinforcement gyration |
|  [09]   | `.EffectiveDepth(SectionFace) -> Length`                    | instance | `d` to face reinforcement |
|  [10]   | `.ReinforcementArea(SectionFace) -> Area`                   | instance | face steel area           |
|  [11]   | `.CrossSectionalShearReinforcementArea -> Area`             | property | link/stirrup shear area   |

[ENTRYPOINT_SCOPE]: direct `.Utility` kernel calls

| [INDEX] | [SURFACE]                                                                 | [SHAPE] | [CAPABILITY]              |
| :-----: | :------------------------------------------------------------------------ | :------ | :------------------------ |
|  [01]   | `Areas.CalculateArea(IProfile) -> Area`                                   | static  | void-subtracted area      |
|  [02]   | `Centroids.CalculateCentroid(IProfile) -> ILocalPoint2d`                  | static  | elastic centroid          |
|  [03]   | `Inertiae.CalculateInertiaYy(IProfile) -> AreaMomentOfInertia`            | static  | Yy second moment          |
|  [04]   | `Inertiae.CalculateInertiaZz(IProfile) -> AreaMomentOfInertia`            | static  | Zz second moment          |
|  [05]   | `SectionModuli.CalculateSectionModulusYy(IProfile) -> Volume`             | static  | Yy elastic modulus        |
|  [06]   | `SectionModuli.CalculateSectionModulusZz(IProfile) -> Volume`             | static  | Zz elastic modulus        |
|  [07]   | `RadiusOfGyrations.CalculateRadiusOfGyrationYy(IProfile) -> Length`       | static  | Yy radius of gyration     |
|  [08]   | `RadiusOfGyrations.CalculateRadiusOfGyrationZz(IProfile) -> Length`       | static  | Zz radius of gyration     |
|  [09]   | `PerimeterLengths.CalculatePerimeter(IProfile) -> Length`                 | static  | perimeter length          |
|  [10]   | `Extends.GetDomain(IProfile) -> ILocalDomain2d`                           | static  | bounding extents          |
|  [11]   | `Rebars.CalculateArea(IEnumerable<ILongitudinalReinforcement>) -> Area`   | static  | bar-group steel area      |
|  [12]   | `Rebars.CalculateArea(IRebar) -> Area`                                    | static  | single-bar area           |
|  [13]   | `Rebars.CalculateArea(IConcreteSection, SectionFace) -> Area`             | static  | face steel area           |
|  [14]   | `Rebars.CalculateInertiaYy(IConcreteSection) -> AreaMomentOfInertia`      | static  | Yy reinforcement inertia  |
|  [15]   | `Rebars.CalculateInertiaZz(IConcreteSection) -> AreaMomentOfInertia`      | static  | Zz reinforcement inertia  |
|  [16]   | `Rebars.CalculateRadiusOfGyrationYy(IConcreteSection) -> Length`          | static  | Yy reinforcement gyration |
|  [17]   | `Rebars.CalculateRadiusOfGyrationZz(IConcreteSection) -> Length`          | static  | Zz reinforcement gyration |
|  [18]   | `Rebars.CalculateEffectiveDepth(IConcreteSection, SectionFace) -> Length` | static  | `d` to face reinforcement |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `SectionProperties` folds one Green's-theorem integral over the `IProfile` perimeter — outer boundary minus void edges, parallel-axis transferred to the elastic centroid — so it runs over any closed section (catalogued steel, parametric timber/CMU/masonry/composite) with no per-family literal; every kernel takes `IProfile`, never a shape enum.
- `.Utility.Parts` decomposes the perimeter into `TrapezoidalPart` straight edges and `EllipseQuarterPart` fillet, rounded-HSS, and circular-pipe arcs, each contributing a closed-form area moment, so filleted W-shapes, rounded HSS, and circular pipes integrate without polygonization and catalogue fillet and HSS corner radii carry into `MomentOfInertia`.
- Every property is a `UnitsNet` quantity, never raw `double`; evaluation is lazy, the carrier memoizes `Centroid` and `Extends`, and a single-property need calls a `.Utility` kernel without constructing a carrier.
- `ConcreteSectionProperties` extends the integral over an `IConcreteSection`, reading transformed-section reinforcement properties and `SectionFace` queries through the `.Utility` `Rebars` kernel.

[STACKING]:
- `VividOrange.Profiles.Catalogue`(`api-vividorange-profiles-catalogue.md`): the data package produces the `IProfile` this computation package consumes, meeting at the `IProfile` contract, so one solver computes every catalogued AISC/EN section with no per-section literal.
- `VividOrange.Sections`(`api-vividorange-sections.md`): mints the `IConcreteSection` (profile, EN material, face/perimeter rebar layers) and its `IRebar`/`ILongitudinalReinforcement`/`ILink`/`SectionFace` reinforcement types that `ConcreteSectionProperties` and the `Rebars` kernel consume.
- `VividOrange.InteractionDiagram`(`api-vividorange-interactiondiagram.md`): consumes the same `IConcreteSection`, so one RC section input drives both the elastic transformed-section properties here and the ultimate biaxial N-M-M capacity hull there.
- `VividOrange.Serialization`(`api-vividorange-serialization.md`): `ISectionProperties` and `ConcreteSectionProperties` implement the `ITaxonomySerializable` marker, so a carrier round-trips through `ToJson`/`FromJson`.
- `UnitsNet`(`libs/csharp/.api/api-unitsnet.md`): every property is a `UnitsNet` quantity, so a computed section property and a measured material property are the same quantity type, and section-modulus `Volume` and inertia `AreaMomentOfInertia` carry their dimension into any downstream structural check.
- Profiles family axis: the solver owns section properties for every Profiles `[SmartEnum]` family (steel, CMU, timber, masonry, composite), collapsing the per-family rectangular literals into one `IProfile` polygon integral.

[LOCAL_ADMISSION]:
- Admission runs through the Materials Profiles boundary that computes section properties for the family axis; the `IProfile` input and `UnitsNet` outputs map onto the canonical Profile/section owner at the edge.
- Properties are read as `UnitsNet` quantities, never reduced to `double` in an interior signature; a measured-series aggregation (e.g. layered composite) folds through `UnitMath.Sum<T>` (`libs/csharp/.api/api-unitsnet.md`), never a raw accumulation.
- A Profiles family that needs `Area` or `MomentOfInertia` computes it from the perimeter, never from an inline closed-form constant.

[RAIL_LAW]:
- Package: `VividOrange.Sections.SectionProperties` (MIT, pure-managed AnyCPU, `net10.0` binds `net9.0`)
- Owns: the arbitrary-closed-polygon section-property solver (Green's-theorem integral, void subtraction, parallel-axis transfer) over any `IProfile`, the `SectionProperties` and `ConcreteSectionProperties` carriers, the `.Utility` static kernels, and the reinforced-concrete transformed-section reinforcement surface — all returning `UnitsNet` quantities
- Accept: a section property computed from an `IProfile` (catalogued or parametric), read as a `UnitsNet` quantity, the carrier admitted at the boundary; a reinforced-section property from `ConcreteSectionProperties` over a `ConcreteSection` built from `VividOrange.Sections`
- Reject: a per-family rectangular section-property literal where the polygon integral computes it; a raw-`double` read of a `UnitsNet` property; a computation that bypasses the `IProfile` perimeter contract
