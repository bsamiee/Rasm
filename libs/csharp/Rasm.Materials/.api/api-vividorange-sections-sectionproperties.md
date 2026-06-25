# [RASM_MATERIALS_API_VIVIDORANGE_SECTIONS_SECTIONPROPERTIES]

`VividOrange.Sections.SectionProperties` supplies the arbitrary-closed-polygon section-property solver —
one shoelace / Green's-theorem polygon integral (with void subtraction + parallel-axis transfer) over any
`IProfile` — that returns `Area`, elastic `Centroid`, `MomentOfInertia` Yy/Zz, `ElasticSectionModulus` Yy/Zz,
`RadiusOfGyration` Yy/Zz, and `Perimeter` as `UnitsNet` quantities. `SectionProperties` is constructed from
an `IProfile` (or `ISection`). The sibling `ConcreteSectionProperties` reinforced-concrete surface is
ADMISSION-GATED ([01]-[ADMISSION_GATE]) — REAL in the assembly but UNREACHABLE from the admitted set, because its
`IConcreteSection` input (and the `Rebars` kernel's `IRebar`/`ILongitudinalReinforcement`/`SectionFace` arguments)
come from `VividOrange.Sections.Reinforcement`, which is NOT a nuspec dependency of this package and is NOT
restored; the admitted, composable surface is the plain `SectionProperties(IProfile|ISection)` polygon integral.
It is the COMPUTATION half of the Materials Profiles section-property seam — the DATA half is
`VividOrange.Profiles.Catalogue` (`api-vividorange-profiles-catalogue.md`), whose `CatalogueFactory` produces
the `IProfile` this solver consumes. One solver replaces the per-family rectangular section-property literals
across the Profiles families (timber/CMU/masonry/composite); it composes the in-folder `UnitsNet` quantity
owner (`api-unitsnet.md`).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `VividOrange.Sections.SectionProperties`
- package: `VividOrange.Sections.SectionProperties`
- version: `0.1.0`
- license: MIT (`licenses.nuget.org/MIT` — MagmaWorks / VividOrange taxonomy)
- assembly: `VividOrange.Sections.SectionProperties`
- namespace: `VividOrange.Sections.SectionProperties`, `.Utility`
- asset: runtime library, pure-managed AnyCPU, NO native RID asset. Multi-TFM `net9.0` / `net8.0` / `net7.0` / `net6.0` / `net48`; the consumer `net10.0` binds the highest asset `lib/net9.0`. The `ISectionProperties` contract lives in the transitive `VividOrange.Sections.ISectionProperties` floor (namespace `VividOrange.Sections.SectionProperties`); the `IProfile`/`IPerimeter`/`ISection` inputs and the `ILocalPoint2d`/`ILocalDomain2d` geometry returns come from the `VividOrange.IProfiles` / `VividOrange.Profiles.Perimeter` / `VividOrange.Geometry` floor; the property quantities (`Area`, `Volume`, `AreaMomentOfInertia`, `Length`, `Ratio`) are `UnitsNet`.
- rail: profiles (section computation)
- ABI floor: a `0.1.0` PRE-1.0 contract — the `ISectionProperties` member set may break across a minor bump. The full transitive floor (`VividOrange.Sections.ISectionProperties`, `VividOrange.Profiles.Perimeter`, `VividOrange.Geometry`/`IGeometry`/`ICartesianBase` at `1.8.0`, `VividOrange.Profiles`/`ISections`/`IMaterials`/`IStandards` at `0.1.0`) is centrally pinned for deterministic restore; `UnitsNet` `5.75.0` is the shared quantity floor. `AreaMomentOfInertia` is a `UnitsNet` quantity present on the consumed `net9.0`+ surface.

[ADMISSION_GATE]: the reinforced-concrete surface — `ConcreteSectionProperties`, the `IConcreteSectionProperties` floor contract, and the `.Utility` `Rebars` kernel — is REAL in the assembly but UNREACHABLE from the admitted Materials set. Constructing it requires an `IConcreteSection` (carrying `Rebars`/`Link`) plus `IRebar`/`ILongitudinalReinforcement`/`ILink`/`SectionFace` from `VividOrange.Sections.Reinforcement`, which is NOT a nuspec dependency of this package (the deps are only `VividOrange.Sections.ISectionProperties` / `VividOrange.Geometry` / `VividOrange.Profiles.Perimeter`) and is NOT restored anywhere (`uv run python -m tools.assay api resolve --key vividorange.sections.reinforcement` -> `no source; unknown`; every reinforcement member decompiles with `missing references`). The `Rebars` kernel ships in this assembly's `.Utility`, but every public method parameter (`IConcreteSection` / `IRebar` / `IEnumerable<ILongitudinalReinforcement>` / `SectionFace`) is a reinforcement-floor type, so it is uncallable until that floor is admitted. COMPOSING the RC surface is a two-step cross-folder action — admit `VividOrange.Sections.Reinforcement` (a survey-gaps admission) AND place a reinforced-concrete section owner — NOT realizable from the current admitted set; until then a design page composes ONLY the plain `SectionProperties(IProfile|ISection)` solver (the `ParametricSection` bridge the Profiles pages already use), never `ConcreteSectionProperties`.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: section-property carriers
- rail: profiles

| [INDEX] | [SYMBOL]                    | [PACKAGE_ROLE]      | [CAPABILITY]                                                                |
| :-----: | :-------------------------- | :------------------ | :-------------------------------------------------------------------------- |
|  [01]   | `SectionProperties`         | property carrier    | `ISectionProperties` over any `IProfile`/`ISection` — the polygon-integral solver |
|  [02]   | `ConcreteSectionProperties` | property carrier (gated) | ADMISSION-GATED ([01]-[ADMISSION_GATE], unreachable) — `SectionProperties` + reinforcement properties over an `IConcreteSection` (its `Rebars`/`Link` from the UNADMITTED `VividOrange.Sections.Reinforcement`: `ILongitudinalReinforcement`/`IRebar`/`ILink`) |
|  [03]   | `ISectionProperties`        | contract (floor)    | the property-surface contract (`: ITaxonomySerializable`) — lives in the `VividOrange.Sections.ISectionProperties` floor, NOT this assembly |
|  [04]   | `IConcreteSectionProperties`| contract (floor, gated) | ADMISSION-GATED ([01]-[ADMISSION_GATE], unreachable) — `ISectionProperties` + the reinforcement members (incl. `EffectiveDepth(SectionFace)` / `ReinforcementArea(SectionFace)`) — in the floor; its members reference the UNADMITTED `VividOrange.Sections.Reinforcement` `SectionFace` |

[PUBLIC_TYPE_SCOPE]: static polygon-integral kernels (`.Utility`)
- rail: profiles

The lazy property getters delegate to these `static` kernels; each is the Green's-theorem integral over the
profile's perimeter and is callable directly when a single property is needed without a carrier. The integral is
NOT a bare shoelace over straight edges: the perimeter is decomposed into typed parts in the `.Utility.Parts`
sub-namespace (`IPart` with the `TrapezoidalPart` and `EllipseQuarterPart` implementations), so a fillet, a
rounded-rectangle HSS corner, or a circular/elliptical edge integrates exactly rather than being polygonized. The
`ProfileParts` / `PerimeterProfiles` types are the `internal`-mechanism part builders driving that decomposition —
they expose no consumer-callable surface and are not entrypoints. The `Rebars` kernel listed below is the one
`.Utility` member that is NOT directly callable from the admitted set — its arguments are
`VividOrange.Sections.Reinforcement` types, so it is admission-gated ([01]-[ADMISSION_GATE]).

| [INDEX] | [SYMBOL]            | [PACKAGE_ROLE] | [CAPABILITY]                                                                  |
| :-----: | :------------------ | :------------- | :---------------------------------------------------------------------------- |
|  [01]   | `Areas`             | static kernel  | `CalculateArea(IProfile)` -> `Area` (shoelace integral, void-subtracted)      |
|  [02]   | `Centroids`         | static kernel  | `CalculateCentroid(IProfile)` -> `ILocalPoint2d` (first-moment / area)         |
|  [03]   | `Inertiae`          | static kernel  | `CalculateInertiaYy/Zz(IProfile)` -> `AreaMomentOfInertia` (second moment + parallel-axis) |
|  [04]   | `SectionModuli`     | static kernel  | `CalculateSectionModulusYy/Zz(IProfile)` -> `Volume` (`I / c` elastic modulus) |
|  [05]   | `RadiusOfGyrations` | static kernel  | `CalculateRadiusOfGyrationYy/Zz(IProfile)` -> `Length` (`sqrt(I / A)`)         |
|  [06]   | `PerimeterLengths`  | static kernel  | `CalculatePerimeter(IProfile)` -> `Length` (polygon edge sum)                 |
|  [07]   | `Extends`           | static kernel  | `GetDomain(IProfile)` -> `ILocalDomain2d` (bounding extents)                  |
|  [08]   | `Rebars`            | static kernel (gated) | ADMISSION-GATED ([01]-[ADMISSION_GATE], uncallable) — the concrete-section reinforcement surface `CalculateArea`/`CalculateInertiaYy/Zz`/`CalculateRadiusOfGyrationYy/Zz`/`CalculateEffectiveDepth`; ships in `.Utility` but every argument type (`IConcreteSection`/`IRebar`/`ILongitudinalReinforcement`/`SectionFace`) is from the UNADMITTED `VividOrange.Sections.Reinforcement` floor |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: solver construction + properties
- rail: profiles

| [INDEX] | [SURFACE]                                       | [CALL_SHAPE]   | [CAPABILITY]                                                                |
| :-----: | :---------------------------------------------- | :------------- | :-------------------------------------------------------------------------- |
|  [01]   | `new SectionProperties(IProfile profile)`       | constructor    | the solver over a catalogued/parametric profile (lazy property evaluation)  |
|  [02]   | `new SectionProperties(ISection section)`       | constructor    | the solver over a full section (profile + material)                         |
|  [03]   | `SectionProperties.Area`                        | property       | `UnitsNet.Area` — the void-subtracted cross-section area                    |
|  [04]   | `SectionProperties.Centroid`                    | property       | `ILocalPoint2d` — the elastic centroid (lazy, cached)                       |
|  [05]   | `SectionProperties.MomentOfInertiaYy` / `Zz`    | property       | `AreaMomentOfInertia` — second moment about the elastic-centroid axes       |
|  [06]   | `SectionProperties.ElasticSectionModulusYy` / `Zz` | property    | `UnitsNet.Volume` — `I / c` elastic section modulus                         |
|  [07]   | `SectionProperties.RadiusOfGyrationYy` / `Zz`   | property       | `UnitsNet.Length` — `sqrt(I / A)`                                           |
|  [08]   | `SectionProperties.Perimeter`                   | property       | `UnitsNet.Length` — the section perimeter                                   |
|  [09]   | `SectionProperties.Extends`                     | property       | `ILocalDomain2d` — the bounding extents (lazy, cached)                       |

[ENTRYPOINT_SCOPE]: reinforced-concrete surface (`ConcreteSectionProperties`) — ADMISSION-GATED ([01]-[ADMISSION_GATE])
- rail: profiles
- gate: UNREACHABLE from the admitted set — every entry below needs an `IConcreteSection`/`SectionFace` from the UNADMITTED `VividOrange.Sections.Reinforcement` floor; compose only after that floor is admitted, never from the current Materials set

| [INDEX] | [SURFACE]                                       | [CALL_SHAPE]   | [CAPABILITY]                                                                |
| :-----: | :---------------------------------------------- | :------------- | :-------------------------------------------------------------------------- |
|  [01]   | `new ConcreteSectionProperties(IConcreteSection)`| constructor   | the reinforced-section solver (transformed properties + rebar)              |
|  [02]   | `.TotalReinforcementArea` / `.ConcreteArea`     | property       | `UnitsNet.Area` — gross steel area and net concrete area                    |
|  [03]   | `.GeometricReinforcementRatio`                  | property       | `UnitsNet.Ratio` — `As / Ac` steel ratio                                    |
|  [04]   | `.ReinforcementSecondMomentOfAreaYy` / `Zz`     | property       | `AreaMomentOfInertia` — the reinforcement contribution to inertia           |
|  [05]   | `.ReinforcementRadiusOfGyrationYy` / `Zz`       | property       | `UnitsNet.Length` — reinforcement radius of gyration                        |
|  [06]   | `.EffectiveDepth(SectionFace face)`             | method         | `UnitsNet.Length` — `d` to the reinforcement layer of a section face        |
|  [07]   | `.ReinforcementArea(SectionFace face)`          | method         | `UnitsNet.Area` — the steel area at a section face                          |
|  [08]   | `.CrossSectionalShearReinforcementArea`         | property       | `UnitsNet.Area` — the link/stirrup shear-reinforcement area                 |

[ENTRYPOINT_SCOPE]: direct kernel calls (single-property, no carrier)
- rail: profiles

| [INDEX] | [SURFACE]                                       | [CALL_SHAPE]   | [CAPABILITY]                                                                |
| :-----: | :---------------------------------------------- | :------------- | :-------------------------------------------------------------------------- |
|  [01]   | `Areas.CalculateArea(IProfile)`                 | static call    | `Area` without constructing a carrier (the shoelace integral)               |
|  [02]   | `Inertiae.CalculateInertiaYy(IProfile)` / `Zz`  | static call    | `AreaMomentOfInertia` directly                                              |
|  [03]   | `SectionModuli.CalculateSectionModulusYy(IProfile)` / `Zz` | static call | `Volume` directly                                                          |
|  [04]   | `Centroids.CalculateCentroid(IProfile)`         | static call    | `ILocalPoint2d` directly                                                    |
|  [05]   | `RadiusOfGyrations.CalculateRadiusOfGyrationYy(IProfile)` / `Zz` | static call | `Length` directly                                                   |
|  [06]   | `PerimeterLengths.CalculatePerimeter(IProfile)` | static call    | `Length` directly                                                           |

## [04]-[IMPLEMENTATION_LAW]

[SOLVER_ALGEBRA]:
- carrier root: `SectionProperties(IProfile|ISection)` -> lazy `UnitsNet`-typed property getters
- concrete extension (ADMISSION-GATED, [01]-[ADMISSION_GATE] — REAL but UNREACHABLE until `VividOrange.Sections.Reinforcement` is admitted): `ConcreteSectionProperties(IConcreteSection)` -> reinforcement properties + `SectionFace` queries
- kernel root: the `.Utility` `static` Green's-theorem integrals (`Areas`/`Inertiae`/`SectionModuli`/`Centroids`/`RadiusOfGyrations`/`PerimeterLengths`)
- input root: `IProfile` (the perimeter — outer boundary + void edges, straight `TrapezoidalPart`s and curved `EllipseQuarterPart`s) from the catalogue or a parametric profile
- output root: `UnitsNet` quantities (`Area`, `Volume`, `AreaMomentOfInertia`, `Length`, `Ratio`) + `ILocalPoint2d`/`ILocalDomain2d` geometry

[POLYGON_INTEGRAL_CONTRACT]:
- The solver is ONE Green's-theorem integral over the `IProfile` perimeter — outer boundary minus void edges, with
  parallel-axis transfer to the elastic centroid — so it runs over ANY closed section (catalogued steel, parametric
  timber/CMU/masonry/composite) without per-family literals. The kernels take `IProfile`, not a shape enum.
- The perimeter is decomposed into typed `.Utility.Parts` (`TrapezoidalPart` for straight edges, `EllipseQuarterPart`
  for fillets / rounded-HSS corners / circular-pipe arcs), and each part contributes its closed-form area moment to
  the integral — so a filleted W-shape, a rounded rectangular HSS, and a circular Pipe all integrate EXACTLY, not as
  polygonized approximations. This is why the catalogue's `IIParallelFlange.FilletRadius` and the HSS corner radii
  carry into the computed `MomentOfInertia` without loss.
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
- geometry seam: the `Centroid`/`Extends` returns are `VividOrange.Geometry` `ILocalPoint2d`/`ILocalDomain2d`
  (the transitive `1.8.0` floor); the perimeter polygon the integral iterates is `VividOrange.Profiles.Perimeter`
  `IPerimeter` — a parametric (non-catalogued) section feeds the solver by constructing an `IProfile` over an
  `IPerimeter` (outer polyline + void edges), so user-defined sections compute identically to catalogued ones.

[RAIL_LAW]:
- Package: `VividOrange.Sections.SectionProperties` `0.1.0` (MIT, pure-managed AnyCPU, `net10.0` binds `net9.0`, PRE-1.0 contract)
- Owns: the arbitrary-closed-polygon section-property solver (shoelace/Green's-theorem integral with void
  subtraction + parallel-axis transfer) over any `IProfile`, the `SectionProperties`/`ConcreteSectionProperties`
  carriers, the `.Utility` `static` kernels, and (ADMISSION-GATED, [01]-[ADMISSION_GATE] — unreachable until `VividOrange.Sections.Reinforcement` is admitted) the reinforced-concrete reinforcement surface — all returning `UnitsNet` quantities
- Accept: a section property computed for the Materials Profiles family axis from an `IProfile` (catalogued or
  parametric), read as a `UnitsNet` quantity, the carrier admitted at the boundary
- Reject: a per-family rectangular section-property literal where the polygon integral computes it; a raw-`double`
  read of a `UnitsNet` property; a section-property computation that bypasses the `IProfile` perimeter contract
