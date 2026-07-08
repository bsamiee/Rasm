# [RASM_MATERIALS_API_VIVIDORANGE_SECTIONS_SECTIONPROPERTIES]

`VividOrange.Sections.SectionProperties` supplies the arbitrary-closed-polygon section-property solver —
one shoelace / Green's-theorem polygon integral (with void subtraction + parallel-axis transfer) over any
`IProfile` — that returns `Area`, elastic `Centroid`, `MomentOfInertia` Yy/Zz, `ElasticSectionModulus` Yy/Zz,
`RadiusOfGyration` Yy/Zz, and `Perimeter` as `UnitsNet` quantities. `SectionProperties` is constructed from
an `IProfile` (or `ISection`). The sibling `ConcreteSectionProperties: SectionProperties, IConcreteSectionProperties`
reinforced-concrete surface is FIRST-CLASS COMPOSABLE ([01]-[RC_COMPOSITION_PATH]): its `IConcreteSection` input
(and the `.Utility` `Rebars` kernel's `IRebar`/`ILongitudinalReinforcement`/`SectionFace` arguments) come from the
admitted `VividOrange.Sections` assembly (the `ConcreteSection` concrete + the full
`VividOrange.Sections.Reinforcement` namespace, `api-vividorange-sections.md`) over the admitted
`VividOrange.ISections` floor (`IConcreteSection`/`IRebar`/`ILongitudinalReinforcement`/`SectionFace`), so a
Materials RC owner builds a `ConcreteSection` and reads the transformed-section reinforcement properties with no
further admission. It is the COMPUTATION half of the Materials Profiles section-property seam — the DATA half is
`VividOrange.Profiles.Catalogue` (`api-vividorange-profiles-catalogue.md`), whose `CatalogueFactory` produces
the `IProfile` this solver consumes. One solver replaces the per-family rectangular section-property literals
across the Profiles families (timber/CMU/masonry/composite); it composes the in-folder `UnitsNet` quantity
owner (`api-unitsnet.md`).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `VividOrange.Sections.SectionProperties`
- package: `VividOrange.Sections.SectionProperties`
- license: MIT (`licenses.nuget.org/MIT` — MagmaWorks / VividOrange taxonomy)
- assembly: `VividOrange.Sections.SectionProperties`
- namespace: `VividOrange.Sections.SectionProperties`, `.Utility`
- asset: runtime library, pure-managed AnyCPU, NO native RID asset. Multi-TFM `net9.0` / `net8.0` / `net7.0` / `net6.0` / `net48`; the consumer `net10.0` binds the highest asset `lib/net9.0`. The `ISectionProperties` contract lives in the transitive `VividOrange.Sections.ISectionProperties` floor (namespace `VividOrange.Sections.SectionProperties`); the `IProfile`/`IPerimeter`/`ISection` inputs and the `ILocalPoint2d`/`ILocalDomain2d` geometry returns come from the `VividOrange.IProfiles` / `VividOrange.Profiles.Perimeter` / `VividOrange.Geometry` floor; the property quantities (`Area`, `Volume`, `AreaMomentOfInertia`, `Length`, `Ratio`) are `UnitsNet`.
- rail: profiles (section computation)
- ABI floor: a PRE-1.0 contract — the `ISectionProperties` member set may break across a minor bump. The full transitive floor (`VividOrange.Sections.ISectionProperties`, `VividOrange.Profiles.Perimeter`, `VividOrange.Geometry`/`IGeometry`/`ICartesianBase` at, `VividOrange.Profiles`/`ISections`/`IMaterials`/`IStandards` at) is centrally pinned for deterministic restore; `UnitsNet` is the shared quantity floor. `AreaMomentOfInertia` is a `UnitsNet` quantity present on the consumed `net9.0`+ surface. The `ITaxonomySerializable`
on `ISectionProperties`/`ConcreteSectionProperties` is the `VividOrange.Serialization.ITaxonomySerializable`
(assembly `VividOrange.ISerialization`) — a DISTINCT CLR type identity from the
`VividOrange.Taxonomy.Serialization.ITaxonomySerializable` (assembly `VividOrange.Taxonomy.ISerialization`) the
`VividOrange.Uncertainties` packages ride (`api-vividorange-uncertainties.md`); the `TaxonomyJsonSerializer`
does NOT serialize the types, so a Materials design page never assumes one shared VividOrange serializer
covers both lanes.

[RC_COMPOSITION_PATH]: the reinforced-concrete surface — `ConcreteSectionProperties: SectionProperties, IConcreteSectionProperties, ISectionProperties, ITaxonomySerializable`, the `IConcreteSectionProperties` floor contract, and the `.Utility` `Rebars` kernel — is FIRST-CLASS COMPOSABLE from the admitted Materials set. The `IConcreteSection` it consumes (carrying `IList<ILongitudinalReinforcement> Rebars`, `ILink Link`, `Length Cover`) lives in the admitted `VividOrange.ISections` floor (namespace `VividOrange.Sections`), and the `IRebar` (`Length Diameter`; `IMaterial Material`) / `ILongitudinalReinforcement` (`IRebar Rebar`; `int CountPerBundle`) / `ILink` / `SectionFace` arguments resolve to the admitted `VividOrange.Sections.Reinforcement` floor (same `VividOrange.ISections` assembly). The `ConcreteSection` concrete plus the full `VividOrange.Sections.Reinforcement` namespace (`Rebar`/`Link`/`LongitudinalReinforcement`/`FaceReinforcementLayer`/`PerimeterReinforcementLayer`/`ReinforcementLayoutByCount`/`BySpacing`/`MinimumReinforcementSpacing`) ship in the admitted `VividOrange.Sections` assembly (`api-vividorange-sections.md`), centrally pinned + restored. The `Rebars` kernel ships in this assembly's `.Utility` (`CalculateArea(IEnumerable<ILongitudinalReinforcement>)` / `CalculateArea(IRebar)` / `CalculateArea(IConcreteSection, SectionFace)` / `CalculateEffectiveDepth(IConcreteSection, SectionFace)` / `CalculateInertiaYy/Zz(IConcreteSection)` / `CalculateRadiusOfGyrationYy/Zz(IConcreteSection)`) and every parameter type is now reachable, so it is directly callable. A design page composes the RC surface in one folder-local action — build a `ConcreteSection` from an admitted `IProfile` + EN material + a face/perimeter rebar-layer arrangement (`api-vividorange-sections.md`), then `new ConcreteSectionProperties(thatSection)` reads the transformed-section reinforcement properties — alongside the plain `SectionProperties(IProfile\|ISection)` solver. The same `IConcreteSection` feeds the `VividOrange.InteractionDiagram` N-M-M capacity engine (`api-vividorange-interactiondiagram.md`), so the elastic transformed-section properties and the ultimate capacity hull share one RC section input.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: section-property carriers
- rail: profiles

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY] |
|:-----: |:-------------------------- |:------------------ |:-------------------------------------------------------------------------- |
| [01] | `SectionProperties` | property carrier | `ISectionProperties` over any `IProfile`/`ISection` — the polygon-integral solver |
| [02] | `ConcreteSectionProperties` | property carrier (RC) | `: SectionProperties, IConcreteSectionProperties` — `SectionProperties` + reinforcement properties over an `IConcreteSection` (its `Rebars`/`Link` from the admitted `VividOrange.Sections`: `ILongitudinalReinforcement`/`IRebar`/`ILink`, `api-vividorange-sections.md`); first-class composable ([01]-[RC_COMPOSITION_PATH]) |
| [03] | `ISectionProperties` | contract (floor) | the property-surface contract (`: ITaxonomySerializable`) — lives in the `VividOrange.Sections.ISectionProperties` floor, NOT this assembly |
| [04] | `IConcreteSectionProperties` | contract (floor, RC) | `: ISectionProperties` + the reinforcement members (incl. `EffectiveDepth(SectionFace)` / `ReinforcementArea(SectionFace)`) — in the floor; its `SectionFace` member type resolves from the admitted `VividOrange.ISections` floor ([01]-[RC_COMPOSITION_PATH]) |

[PUBLIC_TYPE_SCOPE]: static polygon-integral kernels (`.Utility`)
- rail: profiles

The lazy property getters delegate to these `static` kernels; each is the Green's-theorem integral over the
profile's perimeter and is callable directly when a single property is needed without a carrier. The integral is
NOT a bare shoelace over straight edges: the perimeter is decomposed into typed parts in the `.Utility.Parts`
sub-namespace (`IPart` with the `TrapezoidalPart` and `EllipseQuarterPart` implementations), so a fillet, a
rounded-rectangle HSS corner, or a circular/elliptical edge integrates exactly rather than being polygonized. The
`ProfileParts` / `PerimeterProfiles` types are the `internal`-mechanism part builders driving that decomposition —
they expose no consumer-callable surface and are not entrypoints. The `Rebars` kernel listed below is the
`.Utility` member whose arguments are `VividOrange.Sections.Reinforcement` / `IConcreteSection` types; those floor
types are admitted (`api-vividorange-sections.md`), so it is directly callable ([01]-[RC_COMPOSITION_PATH]).

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY] |
|:-----: |:------------------ |:------------- |:---------------------------------------------------------------------------- |
| [01] | `Areas` | static kernel | `CalculateArea(IProfile)` -> `Area` (shoelace integral, void-subtracted) |
| [02] | `Centroids` | static kernel | `CalculateCentroid(IProfile)` -> `ILocalPoint2d` (first-moment / area) |
| [03] | `Inertiae` | static kernel | `CalculateInertiaYy/Zz(IProfile)` -> `AreaMomentOfInertia` (second moment + parallel-axis) |
| [04] | `SectionModuli` | static kernel | `CalculateSectionModulusYy/Zz(IProfile)` -> `Volume` (`I / c` elastic modulus) |
| [05] | `RadiusOfGyrations` | static kernel | `CalculateRadiusOfGyrationYy/Zz(IProfile)` -> `Length` (`sqrt(I / A)`) |
| [06] | `PerimeterLengths` | static kernel | `CalculatePerimeter(IProfile)` -> `Length` (polygon edge sum) |
| [07] | `Extends` | static kernel | `GetDomain(IProfile)` -> `ILocalDomain2d` (bounding extents) |
| [08] | `Rebars` | static kernel (RC) | the concrete-section reinforcement surface — `CalculateArea(IEnumerable<ILongitudinalReinforcement>)` / `CalculateArea(IRebar)` / `CalculateArea(IConcreteSection, SectionFace)` / `CalculateInertiaYy/Zz(IConcreteSection)` / `CalculateRadiusOfGyrationYy/Zz(IConcreteSection)` / `CalculateEffectiveDepth(IConcreteSection, SectionFace)`; ships in `.Utility` and every argument type (`IConcreteSection`/`IRebar`/`ILongitudinalReinforcement`/`SectionFace`) resolves from the admitted `VividOrange.Sections`/`ISections` floor — directly callable ([01]-[RC_COMPOSITION_PATH]) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: solver construction + properties
- rail: profiles

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:---------------------------------------------- |:------------- |:-------------------------------------------------------------------------- |
| [01] | `new SectionProperties(IProfile profile)` | constructor | the solver over a catalogued/parametric profile (lazy property evaluation) |
| [02] | `new SectionProperties(ISection section)` | constructor | the solver over a full section (profile + material) |
| [03] | `SectionProperties.Area` | property | `UnitsNet.Area` — the void-subtracted cross-section area |
| [04] | `SectionProperties.Centroid` | property | `ILocalPoint2d` — the elastic centroid (lazy, cached) |
| [05] | `SectionProperties.MomentOfInertiaYy` / `Zz` | property | `AreaMomentOfInertia` — second moment about the elastic-centroid axes |
| [06] | `SectionProperties.ElasticSectionModulusYy` / `Zz` | property | `UnitsNet.Volume` — `I / c` elastic section modulus |
| [07] | `SectionProperties.RadiusOfGyrationYy` / `Zz` | property | `UnitsNet.Length` — `sqrt(I / A)` |
| [08] | `SectionProperties.Perimeter` | property | `UnitsNet.Length` — the section perimeter |
| [09] | `SectionProperties.Extends` | property | `ILocalDomain2d` — the bounding extents (lazy, cached) |

[ENTRYPOINT_SCOPE]: reinforced-concrete surface (`ConcreteSectionProperties`) — FIRST-CLASS COMPOSABLE ([01]-[RC_COMPOSITION_PATH])
- rail: profiles
- composition law: every entry below takes an `IConcreteSection`/`SectionFace` from the admitted `VividOrange.Sections`/`ISections` floor (`api-vividorange-sections.md`) — build a `ConcreteSection` (profile + EN material + rebar layers) and construct `new ConcreteSectionProperties(section)`; the same section also feeds the `VividOrange.InteractionDiagram` capacity engine

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:---------------------------------------------- |:------------- |:-------------------------------------------------------------------------- |
| [01] | `new ConcreteSectionProperties(IConcreteSection)` | constructor | the reinforced-section solver (transformed properties + rebar) |
| [02] | `.TotalReinforcementArea` / `.ConcreteArea` | property | `UnitsNet.Area` — gross steel area and net concrete area |
| [03] | `.GeometricReinforcementRatio` | property | `UnitsNet.Ratio` — `As / Ac` steel ratio |
| [04] | `.ReinforcementSecondMomentOfAreaYy` / `Zz` | property | `AreaMomentOfInertia` — the reinforcement contribution to inertia |
| [05] | `.ReinforcementRadiusOfGyrationYy` / `Zz` | property | `UnitsNet.Length` — reinforcement radius of gyration |
| [06] | `.EffectiveDepth(SectionFace face)` | method | `UnitsNet.Length` — `d` to the reinforcement layer of a section face |
| [07] | `.ReinforcementArea(SectionFace face)` | method | `UnitsNet.Area` — the steel area at a section face |
| [08] | `.CrossSectionalShearReinforcementArea` | property | `UnitsNet.Area` — the link/stirrup shear-reinforcement area |

[ENTRYPOINT_SCOPE]: direct kernel calls (single-property, no carrier)
- rail: profiles

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:---------------------------------------------- |:------------- |:-------------------------------------------------------------------------- |
| [01] | `Areas.CalculateArea(IProfile)` | static call | `Area` without constructing a carrier (the shoelace integral) |
| [02] | `Inertiae.CalculateInertiaYy(IProfile)` / `Zz` | static call | `AreaMomentOfInertia` directly |
| [03] | `SectionModuli.CalculateSectionModulusYy(IProfile)` / `Zz` | static call | `Volume` directly |
| [04] | `Centroids.CalculateCentroid(IProfile)` | static call | `ILocalPoint2d` directly |
| [05] | `RadiusOfGyrations.CalculateRadiusOfGyrationYy(IProfile)` / `Zz` | static call | `Length` directly |
| [06] | `PerimeterLengths.CalculatePerimeter(IProfile)` | static call | `Length` directly |

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
