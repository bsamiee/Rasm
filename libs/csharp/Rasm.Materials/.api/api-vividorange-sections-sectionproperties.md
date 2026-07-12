# [RASM_MATERIALS_API_VIVIDORANGE_SECTIONS_SECTIONPROPERTIES]

`VividOrange.Sections.SectionProperties` supplies the arbitrary-closed-polygon section-property solver: one shoelace / Green's-theorem polygon integral with void subtraction and parallel-axis transfer over any `IProfile` returns `Area`, elastic `Centroid`, `MomentOfInertia` Yy/Zz, `ElasticSectionModulus` Yy/Zz, `RadiusOfGyration` Yy/Zz, and `Perimeter` as `UnitsNet` quantities. `SectionProperties` is constructed from an `IProfile` or `ISection`. The sibling `ConcreteSectionProperties: SectionProperties, IConcreteSectionProperties` reinforced-concrete surface is first-class composable ([01]-[RC_COMPOSITION_PATH]): its `IConcreteSection` input and the `.Utility` `Rebars` kernel's `IRebar`/`ILongitudinalReinforcement`/`SectionFace` arguments come from the admitted `VividOrange.Sections` assembly over the admitted `VividOrange.ISections` floor, so a Materials RC owner builds a `ConcreteSection` and reads the transformed-section reinforcement properties without further admission. `VividOrange.Profiles.Catalogue` (`api-vividorange-profiles-catalogue.md`) supplies the `IProfile`; this computation owner replaces rectangular section-property literals across the timber, CMU, masonry, and composite families and composes the in-folder `UnitsNet` quantity owner (`api-unitsnet.md`).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `VividOrange.Sections.SectionProperties`

- package: `VividOrange.Sections.SectionProperties`
- license: MIT (`licenses.nuget.org/MIT` — MagmaWorks / VividOrange taxonomy)
- assembly: `VividOrange.Sections.SectionProperties`
- namespace: `VividOrange.Sections.SectionProperties`, `.Utility`
- asset: runtime library, pure-managed AnyCPU, no native RID asset
- frameworks: `net9.0` / `net8.0` / `net7.0` / `net6.0` / `net48`; the `net10.0` consumer binds `lib/net9.0`
- contract floor: `ISectionProperties` lives in transitive `VividOrange.Sections.ISectionProperties` under `VividOrange.Sections.SectionProperties`
- input floor: `IProfile`/`IPerimeter`/`ISection` come from `VividOrange.IProfiles` / `VividOrange.Profiles.Perimeter`
- geometry floor: `ILocalPoint2d`/`ILocalDomain2d` come from `VividOrange.Geometry`
- quantity floor: `Area`, `Volume`, `AreaMomentOfInertia`, `Length`, and `Ratio` are `UnitsNet`
- rail: profiles (section computation)
- ABI floor: a pre-1.0 contract whose `ISectionProperties` member set may break across a minor bump. The full transitive floor (`VividOrange.Sections.ISectionProperties`, `VividOrange.Profiles.Perimeter`, `VividOrange.Geometry`/`IGeometry`/`ICartesianBase` at, `VividOrange.Profiles`/`ISections`/`IMaterials`/`IStandards` at) is centrally pinned for deterministic restore; `UnitsNet` is the shared quantity floor. `AreaMomentOfInertia` is a `UnitsNet` quantity present on the consumed `net9.0`+ surface. `ISectionProperties` and `ConcreteSectionProperties` implement `VividOrange.Serialization.ITaxonomySerializable` from `VividOrange.ISerialization`, a distinct CLR identity from the `VividOrange.Taxonomy.Serialization.ITaxonomySerializable` in `VividOrange.Taxonomy.ISerialization` used by `VividOrange.Uncertainties` (`api-vividorange-uncertainties.md`). `TaxonomyJsonSerializer` does not serialize these types, so one VividOrange serializer cannot cover both lanes.

[RC_COMPOSITION_PATH]: the reinforced-concrete surface — `ConcreteSectionProperties: SectionProperties, IConcreteSectionProperties, ISectionProperties, ITaxonomySerializable`, the `IConcreteSectionProperties` floor contract, and the `.Utility` `Rebars` kernel — is FIRST-CLASS COMPOSABLE from the admitted Materials set. The `IConcreteSection` it consumes (carrying `IList<ILongitudinalReinforcement> Rebars`, `ILink Link`, `Length Cover`) lives in the admitted `VividOrange.ISections` floor (namespace `VividOrange.Sections`), and the `IRebar` (`Length Diameter`; `IMaterial Material`) / `ILongitudinalReinforcement` (`IRebar Rebar`; `int CountPerBundle`) / `ILink` / `SectionFace` arguments resolve to the admitted `VividOrange.Sections.Reinforcement` floor (same `VividOrange.ISections` assembly). The `ConcreteSection` concrete plus the full `VividOrange.Sections.Reinforcement` namespace (`Rebar`/`Link`/`LongitudinalReinforcement`/`FaceReinforcementLayer`/`PerimeterReinforcementLayer`/`ReinforcementLayoutByCount`/`BySpacing`/`MinimumReinforcementSpacing`) ship in the admitted `VividOrange.Sections` assembly (`api-vividorange-sections.md`), centrally pinned + restored. The `Rebars` kernel ships in this assembly's `.Utility` (`CalculateArea(IEnumerable<ILongitudinalReinforcement>)` / `CalculateArea(IRebar)` / `CalculateArea(IConcreteSection, SectionFace)` / `CalculateEffectiveDepth(IConcreteSection, SectionFace)` / `CalculateInertiaYy/Zz(IConcreteSection)` / `CalculateRadiusOfGyrationYy/Zz(IConcreteSection)`) and every parameter type is now reachable, so it is directly callable. A design page composes the RC surface in one folder-local action — build a `ConcreteSection` from an admitted `IProfile` + EN material + a face/perimeter rebar-layer arrangement (`api-vividorange-sections.md`), then `new ConcreteSectionProperties(thatSection)` reads the transformed-section reinforcement properties — alongside the plain `SectionProperties(IProfile\|ISection)` solver. The same `IConcreteSection` feeds the `VividOrange.InteractionDiagram` N-M-M capacity engine (`api-vividorange-interactiondiagram.md`), so the elastic transformed-section properties and the ultimate capacity hull share one RC section input.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: section-property carriers

- rail: profiles

`SectionProperties` implements `ISectionProperties` over `IProfile` or `ISection`. `ConcreteSectionProperties` extends `SectionProperties` and implements `IConcreteSectionProperties`; its reinforcement members operate over `IConcreteSection`, and its `SectionFace` argument resolves from the admitted `VividOrange.ISections` floor ([01]-[RC_COMPOSITION_PATH]). Both floor contracts live outside this assembly, `ISectionProperties` implements `ITaxonomySerializable`, and `IConcreteSectionProperties` extends `ISectionProperties` with `EffectiveDepth(SectionFace)` and `ReinforcementArea(SectionFace)`.

| [INDEX] | [SYMBOL]                     | [KIND]         | [CAPABILITY]             |
| :-----: | :--------------------------- | :------------- | :----------------------- |
|  [01]   | `SectionProperties`          | carrier        | polygon-integral results |
|  [02]   | `ConcreteSectionProperties`  | RC carrier     | reinforcement properties |
|  [03]   | `ISectionProperties`         | floor contract | property surface         |
|  [04]   | `IConcreteSectionProperties` | floor contract | reinforcement queries    |

[PUBLIC_TYPE_SCOPE]: static polygon-integral kernels (`.Utility`)

- rail: profiles

The lazy property getters delegate to these `.Utility` static kernels, and a caller can invoke one kernel without constructing a carrier. The integral decomposes the perimeter into `.Utility.Parts` implementations of `IPart`: `TrapezoidalPart` integrates straight edges, while `EllipseQuarterPart` integrates fillets, rounded-rectangle HSS corners, and circular or elliptical edges without polygonization. The internal `ProfileParts` and `PerimeterProfiles` builders expose no consumer-callable surface. `Rebars` accepts admitted `VividOrange.Sections.Reinforcement` and `IConcreteSection` types, so it is directly callable ([01]-[RC_COMPOSITION_PATH]).

| [INDEX] | [SYMBOL]            | [CAPABILITY]              |
| :-----: | :------------------ | :------------------------ |
|  [01]   | `Areas`             | void-subtracted area      |
|  [02]   | `Centroids`         | elastic centroid          |
|  [03]   | `Inertiae`          | centroidal second moments |
|  [04]   | `SectionModuli`     | elastic section moduli    |
|  [05]   | `RadiusOfGyrations` | radii of gyration         |
|  [06]   | `PerimeterLengths`  | perimeter length          |
|  [07]   | `Extends`           | bounding domain           |
|  [08]   | `Rebars`            | RC reinforcement results  |

[KERNEL_CALLS]: each call takes the stated profile or section and returns the stated quantity.

- `Areas`: `CalculateArea(IProfile)` -> `Area`; shoelace integral with void subtraction
- `Centroids`: `CalculateCentroid(IProfile)` -> `ILocalPoint2d`; first moment divided by area
- `Inertiae`: `CalculateInertiaYy(IProfile)` / `CalculateInertiaZz(IProfile)` -> `AreaMomentOfInertia`; second moment with parallel-axis transfer
- `SectionModuli`: `CalculateSectionModulusYy(IProfile)` / `CalculateSectionModulusZz(IProfile)` -> `Volume`; elastic `I / c`
- `RadiusOfGyrations`: `CalculateRadiusOfGyrationYy(IProfile)` / `CalculateRadiusOfGyrationZz(IProfile)` -> `Length`; `sqrt(I / A)`
- `PerimeterLengths`: `CalculatePerimeter(IProfile)` -> `Length`; polygon edge sum
- `Extends`: `GetDomain(IProfile)` -> `ILocalDomain2d`; bounding extents
- `Rebars`: `CalculateArea(IEnumerable<ILongitudinalReinforcement>)` / `CalculateArea(IRebar)` / `CalculateArea(IConcreteSection, SectionFace)` / `CalculateInertiaYy(IConcreteSection)` / `CalculateInertiaZz(IConcreteSection)` / `CalculateRadiusOfGyrationYy(IConcreteSection)` / `CalculateRadiusOfGyrationZz(IConcreteSection)` / `CalculateEffectiveDepth(IConcreteSection, SectionFace)`; every argument type resolves from the admitted `VividOrange.Sections`/`ISections` floor

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: solver construction + properties

- rail: profiles

The carrier evaluates properties lazily; `Centroid` and `Extends` cache their results.

| [INDEX] | [SURFACE]                                   | [KIND]      | [RETURN]              | [CAPABILITY]                |
| :-----: | :------------------------------------------ | :---------- | :-------------------- | :-------------------------- |
|  [01]   | `new SectionProperties(IProfile profile)`   | constructor | `SectionProperties`   | profile-backed solver       |
|  [02]   | `new SectionProperties(ISection section)`   | constructor | `SectionProperties`   | profile-and-material solver |
|  [03]   | `SectionProperties.Area`                    | property    | `UnitsNet.Area`       | void-subtracted area        |
|  [04]   | `SectionProperties.Centroid`                | property    | `ILocalPoint2d`       | cached elastic centroid     |
|  [05]   | `SectionProperties.MomentOfInertiaYy`       | property    | `AreaMomentOfInertia` | Yy second moment            |
|  [06]   | `SectionProperties.MomentOfInertiaZz`       | property    | `AreaMomentOfInertia` | Zz second moment            |
|  [07]   | `SectionProperties.ElasticSectionModulusYy` | property    | `UnitsNet.Volume`     | Yy elastic `I / c`          |
|  [08]   | `SectionProperties.ElasticSectionModulusZz` | property    | `UnitsNet.Volume`     | Zz elastic `I / c`          |
|  [09]   | `SectionProperties.RadiusOfGyrationYy`      | property    | `UnitsNet.Length`     | Yy `sqrt(I / A)`            |
|  [10]   | `SectionProperties.RadiusOfGyrationZz`      | property    | `UnitsNet.Length`     | Zz `sqrt(I / A)`            |
|  [11]   | `SectionProperties.Perimeter`               | property    | `UnitsNet.Length`     | section perimeter           |
|  [12]   | `SectionProperties.Extends`                 | property    | `ILocalDomain2d`      | cached bounding extents     |

[ENTRYPOINT_SCOPE]: reinforced-concrete surface (`ConcreteSectionProperties`) — FIRST-CLASS COMPOSABLE ([01]-[RC_COMPOSITION_PATH])

- rail: profiles
- composition law: every entry below takes an `IConcreteSection`/`SectionFace` from the admitted `VividOrange.Sections`/`ISections` floor (`api-vividorange-sections.md`) — build a `ConcreteSection` (profile + EN material + rebar layers) and construct `new ConcreteSectionProperties(section)`; the same section also feeds the `VividOrange.InteractionDiagram` capacity engine

| [INDEX] | [SURFACE]                                         | [KIND]      | [RETURN]                    | [CAPABILITY]                     |
| :-----: | :------------------------------------------------ | :---------- | :-------------------------- | :------------------------------- |
|  [01]   | `new ConcreteSectionProperties(IConcreteSection)` | constructor | `ConcreteSectionProperties` | transformed RC solver            |
|  [02]   | `.TotalReinforcementArea`                         | property    | `UnitsNet.Area`             | gross steel area                 |
|  [03]   | `.ConcreteArea`                                   | property    | `UnitsNet.Area`             | net concrete area                |
|  [04]   | `.GeometricReinforcementRatio`                    | property    | `UnitsNet.Ratio`            | `As / Ac` steel ratio            |
|  [05]   | `.ReinforcementSecondMomentOfAreaYy`              | property    | `AreaMomentOfInertia`       | Yy reinforcement inertia         |
|  [06]   | `.ReinforcementSecondMomentOfAreaZz`              | property    | `AreaMomentOfInertia`       | Zz reinforcement inertia         |
|  [07]   | `.ReinforcementRadiusOfGyrationYy`                | property    | `UnitsNet.Length`           | Yy reinforcement gyration radius |
|  [08]   | `.ReinforcementRadiusOfGyrationZz`                | property    | `UnitsNet.Length`           | Zz reinforcement gyration radius |
|  [09]   | `.EffectiveDepth(SectionFace face)`               | method      | `UnitsNet.Length`           | `d` to face reinforcement        |
|  [10]   | `.ReinforcementArea(SectionFace face)`            | method      | `UnitsNet.Area`             | face steel area                  |
|  [11]   | `.CrossSectionalShearReinforcementArea`           | property    | `UnitsNet.Area`             | link/stirrup shear area          |

[ENTRYPOINT_SCOPE]: direct kernel calls (single-property, no carrier)

- rail: profiles

| [INDEX] | [SURFACE]                                                 | [RETURN]              | [CAPABILITY]          |
| :-----: | :-------------------------------------------------------- | :-------------------- | :-------------------- |
|  [01]   | `Areas.CalculateArea(IProfile)`                           | `Area`                | void-subtracted area  |
|  [02]   | `Inertiae.CalculateInertiaYy(IProfile)`                   | `AreaMomentOfInertia` | Yy second moment      |
|  [03]   | `Inertiae.CalculateInertiaZz(IProfile)`                   | `AreaMomentOfInertia` | Zz second moment      |
|  [04]   | `SectionModuli.CalculateSectionModulusYy(IProfile)`       | `Volume`              | Yy elastic modulus    |
|  [05]   | `SectionModuli.CalculateSectionModulusZz(IProfile)`       | `Volume`              | Zz elastic modulus    |
|  [06]   | `Centroids.CalculateCentroid(IProfile)`                   | `ILocalPoint2d`       | elastic centroid      |
|  [07]   | `RadiusOfGyrations.CalculateRadiusOfGyrationYy(IProfile)` | `Length`              | Yy radius of gyration |
|  [08]   | `RadiusOfGyrations.CalculateRadiusOfGyrationZz(IProfile)` | `Length`              | Zz radius of gyration |
|  [09]   | `PerimeterLengths.CalculatePerimeter(IProfile)`           | `Length`              | perimeter length      |

## [04]-[IMPLEMENTATION_LAW]

[SOLVER_ALGEBRA]:

- carrier root: `SectionProperties(IProfile\|ISection)` -> lazy `UnitsNet`-typed property getters
- concrete extension (FIRST-CLASS COMPOSABLE, [01]-[RC_COMPOSITION_PATH] — the `IConcreteSection`/reinforcement floor is admitted via `VividOrange.Sections`): `ConcreteSectionProperties(IConcreteSection)` -> reinforcement properties + `SectionFace` queries
- kernel root: the `.Utility` `static` Green's-theorem integrals (`Areas`/`Inertiae`/`SectionModuli`/`Centroids`/`RadiusOfGyrations`/`PerimeterLengths`)
- input root: `IProfile` (the perimeter — outer boundary + void edges, straight `TrapezoidalPart`s and curved `EllipseQuarterPart`s) from the catalogue or a parametric profile
- output root: `UnitsNet` quantities (`Area`, `Volume`, `AreaMomentOfInertia`, `Length`, `Ratio`) + `ILocalPoint2d`/`ILocalDomain2d` geometry

[POLYGON_INTEGRAL_CONTRACT]:

- The solver is ONE Green's-theorem integral over the `IProfile` perimeter — outer boundary minus void edges, with
  parallel-axis transfer to the elastic centroid — so it runs over ANY closed section (catalogued steel, parametric
  timber/CMU/masonry/composite) without per-family literals. The kernels take `IProfile`, not a shape enum.
- `.Utility.Parts` decomposes the perimeter into `TrapezoidalPart` straight edges and `EllipseQuarterPart` fillet, rounded-HSS, and circular-pipe arcs. Each part contributes a closed-form area moment, so filleted W-shapes, rounded HSS, and circular pipes integrate without polygonization; catalogue `IIParallelFlange.FilletRadius` and HSS corner radii carry into `MomentOfInertia` without loss.
- Properties are `UnitsNet` quantities, NOT raw `double` — `Area`, `Volume` (section modulus), `AreaMomentOfInertia`,
  `Length` (radius of gyration, perimeter), `Ratio` (reinforcement ratio); a consumer reads them typed and the
  `UnitsNet` family-gate (`api-unitsnet.md`) applies.
- Property evaluation is LAZY and cached on the carrier (`Centroid`, `Extends` memoize); a single-property need
  calls the `.Utility` kernel directly (`Areas.CalculateArea(profile)`) without constructing a carrier.

[LOCAL_ADMISSION]:

- The solver is admitted ONLY through the Materials Profiles boundary that computes section properties for the
  family axis; the `IProfile` input and the `UnitsNet` outputs map onto the canonical Profile/section owner at the edge.
- Properties are read as `UnitsNet` quantities and never reduced to `double` inside an interior signature; an
  aggregation over a measured-series (e.g. layered composite) folds through `UnitMath.Sum<T>` (`api-unitsnet.md`), never a raw accumulation.
- The solver REPLACES the per-family rectangular section-property literals — a Profiles family that needs `Area`/
  `MomentOfInertia` computes it from the perimeter, never from an inline closed-form constant.

[STACK]:

- catalogue seam: `new SectionProperties(CatalogueFactory.CreateAmerican(American.W44x408))`
  (`api-vividorange-profiles-catalogue.md`) — the DATA package produces the `IProfile`, this COMPUTATION package
  consumes it; the two meet at the `IProfile` contract, so one solver computes `Area`/`MomentOfInertiaYy`/
  `ElasticSectionModulusZz` for every catalogued AISC/EN section with no per-section literal.
- units seam: every property is a `UnitsNet` quantity (`api-unitsnet.md`), so a computed section property and a
  measured material property are the SAME quantity type — the Materials family axis folds catalogued, parametric,
  and computed sections through one `UnitsNet`-typed surface; section-modulus `Volume` and inertia
  `AreaMomentOfInertia` carry their dimension into any downstream structural check.
- family-axis seam: the solver is the section-property owner for EVERY Profiles `[SmartEnum]` family
  (steel/CMU/timber/masonry/composite), not steel alone — the timber/CMU/masonry families that previously needed
  rectangular closed-form literals now compute through the same `IProfile` polygon integral, collapsing the
  per-family property literals into one dispatch.
- RC seam: the `ConcreteSectionProperties(IConcreteSection)` carrier + the `.Utility` `Rebars` kernel compute the
  transformed-section reinforcement properties (`TotalReinforcementArea`/`ConcreteArea`/`GeometricReinforcementRatio`/
  `ReinforcementSecondMomentOfAreaYy/Zz`/`EffectiveDepth(SectionFace)`/`CrossSectionalShearReinforcementArea`) over the
  `ConcreteSection` minted by the admitted `VividOrange.Sections` (`api-vividorange-sections.md`) — the same
  `IConcreteSection` the `VividOrange.InteractionDiagram` N-M-M capacity engine consumes
  (`api-vividorange-interactiondiagram.md`); one RC section input drives both the elastic transformed-section
  properties here and the ultimate biaxial capacity hull there ([01]-[RC_COMPOSITION_PATH]).
- geometry seam: the `Centroid`/`Extends` returns are `VividOrange.Geometry` `ILocalPoint2d`/`ILocalDomain2d`
  (the transitive floor); the perimeter polygon the integral iterates is `VividOrange.Profiles.Perimeter`
  `IPerimeter` — a parametric (non-catalogued) section feeds the solver by constructing an `IProfile` over an
  `IPerimeter` (outer polyline + void edges), so user-defined sections compute identically to catalogued ones.

[RAIL_LAW]:

- Package: `VividOrange.Sections.SectionProperties` (MIT, pure-managed AnyCPU, `net10.0` binds `net9.0`, PRE-1.0 contract)
- Owns: the arbitrary-closed-polygon section-property solver (shoelace/Green's-theorem integral with void
  subtraction + parallel-axis transfer) over any `IProfile`, the `SectionProperties`/`ConcreteSectionProperties`
  carriers, the `.Utility` `static` kernels, and (FIRST-CLASS COMPOSABLE, [01]-[RC_COMPOSITION_PATH] — the
  `IConcreteSection`/reinforcement floor is admitted via `VividOrange.Sections`) the reinforced-concrete transformed-section reinforcement surface — all returning `UnitsNet` quantities
- Accept: a section property computed for the Materials Profiles family axis from an `IProfile` (catalogued or
  parametric), read as a `UnitsNet` quantity, the carrier admitted at the boundary; a reinforced-section property
  read from `ConcreteSectionProperties` over a `ConcreteSection` built from the admitted `VividOrange.Sections` floor
- Reject: a per-family rectangular section-property literal where the polygon integral computes it; a raw-`double`
  read of a `UnitsNet` property; a section-property computation that bypasses the `IProfile` perimeter contract;
  treating the RC transformed-section surface as admission-gated (the `IConcreteSection`/reinforcement floor IS
  admitted, [01]-[RC_COMPOSITION_PATH])
