# [RASM_MATERIALS_API_VIVIDORANGE_PROFILES_CATALOGUE]

`VividOrange.Profiles.Catalogue` supplies the published structural-section database as a typed catalogue
of generated profile classes — 2299 American (AISC Shapes Database v16.0) and 558 European (EN 10365:2017)
sections — each a sealed CRTP singleton (`SingletonAmericanBase<T>` / `SingletonEuropeanBase<T>`) carrying real
cross-section geometry as `UnitsNet.Length` dimensions over the per-family shape interfaces of `VividOrange.IProfiles`
(`II`/`IIParallelFlange` for I-sections, `IChannel`, `ITee`, `IAngle`/`IDoubleAngle`, `IRectangularHollow`/
`ICircularHollow`+`IHollowStructuralSection` for HSS/Pipe). The single entry is `CatalogueFactory.CreateAmerican` /
`CreateEuropean`, which maps a section-identity enum value to its `ICatalogue`/`IProfile` instance. It is the
DATA half of the Materials Profiles section-property seam — the COMPUTATION half is `VividOrange.Sections.SectionProperties`
(`api-vividorange-sections-sectionproperties.md`), which consumes the `IProfile` this factory produces. It
grounds the Profiles steel + EN section-geometry seed in registered published data, replacing hand-keyed
section literals; it composes the in-folder `UnitsNet` quantity owner (`api-unitsnet.md`).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `VividOrange.Profiles.Catalogue`
- package: `VividOrange.Profiles.Catalogue`
- version: `0.1.0`
- license: MIT (`licenses.nuget.org/MIT` — MagmaWorks / VividOrange taxonomy)
- assembly: `VividOrange.Profiles.Catalogue`
- namespace: `VividOrange.Profiles`
- asset: runtime library, pure-managed AnyCPU, NO native RID asset. Multi-TFM `net8.0` / `net7.0` / `net6.0` / `net48`; the consumer `net10.0` binds the highest asset `lib/net8.0`. The geometry contracts (`IProfile`, `ICatalogue`, `II`, `IIParallelFlange`, …) and the `UnitsNet.Length`-typed dimension properties live in the transitive `VividOrange.IProfiles` / `VividOrange.Profiles` floor; this package contributes the section-identity enums, the generated concrete-profile singletons, and `CatalogueFactory`. `ITaxonomySerializable` (the JSON taxonomy contract) comes from the `VividOrange.ISerialization` floor.
- rail: profiles (section data)
- ABI floor: a `0.1.0` PRE-1.0 contract — the enum member set and interface shapes may break across a minor bump. The full transitive floor (`VividOrange.IProfiles`, `VividOrange.ISerialization`) is centrally pinned at `0.1.0` for deterministic restore; the geometry transitives `VividOrange.Geometry`/`VividOrange.IGeometry` ride `1.8.0`. `UnitsNet` `5.75.0` is the shared quantity floor.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: catalogue factory + contracts
- rail: profiles

| [INDEX] | [SYMBOL]              | [PACKAGE_ROLE]    | [CAPABILITY]                                                                |
| :-----: | :-------------------- | :---------------- | :-------------------------------------------------------------------------- |
|  [01]   | `CatalogueFactory`    | `static` factory  | `CreateAmerican(American)` / `CreateEuropean(European)` -> `ICatalogue` (a giant `switch` minting the singleton) |
|  [02]   | `IAmericanCatalogue`  | profile contract  | `ICatalogue, IProfile, ITaxonomySerializable` + `AmericanShape Shape` — an American catalogue entry |
|  [03]   | `IEuropeanCatalogue`  | profile contract  | `ICatalogue, IProfile, ITaxonomySerializable` + `EuropeanShape Shape` — a European catalogue entry |

[PUBLIC_TYPE_SCOPE]: section-identity + shape-family enums
- rail: profiles

| [INDEX] | [SYMBOL]         | [PACKAGE_ROLE] | [CAPABILITY]                                                                  |
| :-----: | :--------------- | :------------- | :---------------------------------------------------------------------------- |
|  [01]   | `American`       | identity enum  | 2299 AISC v16.0 section names (`W44x408`, `HSS…`, `Pipe…`) — the `CreateAmerican` key |
|  [02]   | `European`       | identity enum  | 558 EN 10365 section names — the `CreateEuropean` key                         |
|  [03]   | `AmericanShape`  | family enum    | the 13 AISC families: `W` `M` `S` `HP` `C` `MC` `L` `WT` `MT` `ST` `DoubleL` `HSS` `Pipe` |
|  [04]   | `EuropeanShape`  | family enum    | the 25 EN families: `IPEAA` `IPEA` `IPE` `IPEO` `IPEV` `HEAA` `HEA` `HEB` `HEC` `HEM` `HE` `HL` `HLZ` `HD` `HP` `UBP` `UB` `UC` `IPN` `J` `UPE` `PFC` `UPN` `U` `CH` |

[PUBLIC_TYPE_SCOPE]: inherited geometry contracts (transitive floor, consumed not redefined)
- rail: profiles

The geometry these catalogue entries carry is the transitive `VividOrange.IProfiles` floor. Each concrete
profile implements the geometry interface(s) of ITS family, so a consumer reads dimensions by casting to the
family contract the `AmericanShape`/`EuropeanShape` discriminant selects — `II`/`IIParallelFlange` for W/HEA/IPE,
`IChannel` for C/MC/UPE, `ITee` for WT/MT/ST, `IAngle`/`IDoubleAngle` for L/DoubleL, `IRectangularHollow`/
`ICircularHollow`+`IHollowStructuralSection` for HSS/Pipe — never `((II)entry)` for a non-I family (an
`InvalidCastException`). The section-property solver (`api-vividorange-sections-sectionproperties.md`) avoids the
cast entirely: it consumes the polymorphic `IProfile` perimeter and integrates over any family uniformly.

| [INDEX] | [SYMBOL]            | [PACKAGE_ROLE]   | [CAPABILITY]                                                              |
| :-----: | :------------------ | :--------------- | :----------------------------------------------------------------------- |
|  [01]   | `IProfile`          | base contract    | `string Description` + `ITaxonomySerializable` — every profile           |
|  [02]   | `ICatalogue`        | catalogue contract| `Catalogue Catalogue` (provenance), `Enum Type` (identity), `string Label` |
|  [03]   | `II` / `IIParallelFlange` / `IITaperFlange` | I-family geometry | `Length Height`/`Width`/`FlangeThickness`/`WebThickness`; `IIParallelFlange` adds `Length FilletRadius` (W/HEA/IPE); `IITaperFlange` is the S/HP taper-flange variant — all `UnitsNet.Length` |
|  [04]   | `IChannel` / `IChannelParallelFlange` / `IChannelTaperFlange` | channel geometry | `Height`/`Width`/`WebThickness`/`FlangeThickness` (the C/MC/UPE/PFC/UPN family) |
|  [05]   | `ITee`              | tee geometry      | `Height`/`Width`/`FlangeThickness`/`WebThickness` (the WT/MT/ST cut-tee family, via `ICutTeeParallelFlange`/`ICutTeeTaperFlange`) |
|  [06]   | `IAngle` / `IDoubleAngle` | angle geometry | `Height`/`Width`/`WebThickness`/`FlangeThickness`; `IDoubleAngle : IAngle, IBackToBack` adds `Length BackToBackDistance` (the L / DoubleL family) |
|  [07]   | `IRectangle` / `IRectangularHollow` / `IRoundedRectangularHollow` | rectangular geometry | `Length Height`/`Width`; the hollow variants add `IHollowStructuralSection.Thickness` (the rectangular HSS family) |
|  [08]   | `ICircle` / `ICircularHollow` | circular geometry | `Length Diameter`; `ICircularHollow` adds `IHollowStructuralSection.Thickness` (the Pipe / round-HSS family) |
|  [09]   | `IHollowStructuralSection` | wall-thickness facet | `Length Thickness` — the common wall facet every HSS/Pipe entry also implements |
|  [10]   | `ITaxonomySerializable` | serialization | the MagmaWorks JSON-taxonomy contract (`VividOrange.ISerialization` floor)|

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: catalogue construction
- rail: profiles

| [INDEX] | [SURFACE]                                       | [CALL_SHAPE]   | [CAPABILITY]                                                                |
| :-----: | :---------------------------------------------- | :------------- | :------------------------------------------------------------------------- |
|  [01]   | `CatalogueFactory.CreateAmerican(American)`     | factory call   | `ICatalogue` for the named AISC section, carrying its `UnitsNet.Length` geometry |
|  [02]   | `CatalogueFactory.CreateEuropean(European)`     | factory call   | `ICatalogue` for the named EN section, carrying its `UnitsNet.Length` geometry |
|  [03]   | `IAmericanCatalogue.Shape` / `IEuropeanCatalogue.Shape` | property | the `AmericanShape` / `EuropeanShape` family of the constructed entry        |
|  [04]   | `ICatalogue.Type` / `Label` / `Catalogue`       | property       | the section-identity `Enum`, the display label, and the standard provenance |
|  [05]   | `((II)catalogue).Height` / `.Width` / `.FlangeThickness` / `.WebThickness` / `.FilletRadius` | property | the W/HEA/IPE cross-section dimensions (cast to `II`/`IIParallelFlange`), `UnitsNet.Length` |
|  [06]   | `((IChannel)c).WebThickness` · `((ITee)c).FlangeThickness` · `((IAngle)c).Width` · `((IDoubleAngle)c).BackToBackDistance` | property | the C/WT/L/DoubleL geometry through their family contracts (`UnitsNet.Length`) |
|  [07]   | `((IRectangularHollow)c).Height` · `((ICircularHollow)c).Diameter` · `((IHollowStructuralSection)c).Thickness` | property | the HSS/Pipe envelope + wall thickness (`UnitsNet.Length`) |

## [04]-[IMPLEMENTATION_LAW]

[CATALOGUE_ALGEBRA]:
- identity root: the `American` (2299) / `European` (558) section-name enums
- family root: `AmericanShape` (13 AISC families) / `EuropeanShape` (EN families)
- factory root: `CatalogueFactory.CreateAmerican` / `CreateEuropean`
- geometry root: the `UnitsNet.Length` dimension properties on the per-family shape interfaces (`II`/`IChannel`/`ITee`/`IAngle` `Height`/`Width`/…, `IRectangle`/`ICircle`, `IHollowStructuralSection.Thickness`, `IBackToBack.BackToBackDistance`)
- provenance root: `ICatalogue.Catalogue` + `ITaxonomySerializable`

[GEOMETRY_CONTRACT]:
- Every concrete profile is a SEALED SINGLETON over a CRTP base — `W44x408 : SingletonAmericanBase<W44x408>`,
  `IPE100 : SingletonEuropeanBase<IPE100>` (both deriving `SingletonCatalogueBase<T>`) — implementing the
  geometry interface(s) of its family (`W44x408 : IIParallelFlange, II`; `HSS… : IRoundedRectangularHollow,
  IHollowStructuralSection`; `Pipe… : ICircularHollow, IHollowStructuralSection`; `C… : IChannel`; `L… : IAngle`;
  `WT… : ITee`). Identity equality is reference equality, `Type => Shape` and `Catalogue` are folded on the base,
  and the geometry is immutable published data — never a mutable record.
- Dimensions are `UnitsNet.Length`, NOT raw `double`, and are carried in their NATIVE published unit — AISC v16.0
  data is `LengthUnit.Inch` (`new Length(44.8, Inch)`), EN 10365 data is `LengthUnit.Millimeter`. Neither is the SI
  base unit; the value travels WITH its unit, so a consumer reads `.Millimeters`/`.Inches`/`.Meters` and `UnitsNet`
  owns the conversion — the `UnitsNet` family-gate (`api-unitsnet.md`) applies, and no millimetre literal is re-keyed.
- Shape is a typed discriminant (`AmericanShape.W`, `EuropeanShape.IPE`) and ALSO selects the geometry interface to
  cast to, so a Profiles `[SmartEnum]` family axis maps directly onto it — the AISC/EN family taxonomy IS the
  section discriminant (13 American / 25 European families), not a parallel enum.

[LOCAL_ADMISSION]:
- The factory is admitted ONLY through the Materials Profiles boundary that seeds the steel/EN section families;
  the `American`/`European` enum and the `ICatalogue` instance map onto the canonical Profile owner at the edge.
- Geometry is read as `UnitsNet.Length` and never converted inside an interior signature — the `MaterialUnits`
  boundary (`api-unitsnet.md`) owns any rescale; the catalogue's published values pass through in their native
  unit (AISC=`Inch`, EN=`Millimeter`), the unit travelling with the quantity rather than being normalized at read.
- The published data REPLACES per-family hand-keyed section literals; a Profiles family that needs an AISC/EN
  section reads it from the catalogue, never from an inline dimension table.

[STACK]:
- section-property seam: `CatalogueFactory.CreateAmerican(American.W44x408)` produces an `IProfile`, which
  `new SectionProperties(IProfile)` (`api-vividorange-sections-sectionproperties.md`) consumes to compute
  `Area`/`MomentOfInertiaYy`/`ElasticSectionModulusZz` — the DATA package and the COMPUTATION package meet at
  the `IProfile` contract, so one solver runs over every catalogued section with no per-family literal.
- units seam: every dimension is `UnitsNet.Length` carried in its native published unit (`api-unitsnet.md`), so a
  catalogued AISC section (`Inch`), a catalogued EN section (`Millimeter`), and a measured/parametric section are
  the SAME quantity type regardless of unit — the Profiles family axis folds them through one `UnitsNet`-typed
  surface, `UnitsNet` owns the cross-unit conversion, and the family-membership gate
  (`q.Dimensions.Equals(Length.Info.BaseDimensions)`) applies uniformly.
- family-axis seam: `AmericanShape` / `EuropeanShape` are the AISC/EN section-family discriminants the Materials
  Profiles `[SmartEnum]` family axis (steel/CMU/timber/masonry) maps onto for its steel + EN seed — the published
  family taxonomy IS the discriminant, not a parallel local enum.
- wire seam: `ITaxonomySerializable` is the MagmaWorks JSON-taxonomy contract; a catalogued section that reaches
  the `interchange#MATERIAL_WIRE` projection serializes through the canonical Thinktecture-generated `System.Text.Json`
  codec (the in-folder `Thinktecture.Runtime.Extensions.Json` wire owner) as its identity token (the `American`/
  `European` enum value) plus dimensions normalized to one wire unit at the boundary, the TS/Python peers decoding
  the same shape — never the package's own taxonomy JSON, and never the raw mixed Inch/Millimeter unit, over the wire.

[RAIL_LAW]:
- Package: `VividOrange.Profiles.Catalogue` `0.1.0` (MIT, pure-managed AnyCPU, `net10.0` binds `net8.0`, PRE-1.0 contract)
- Owns: the published AISC v16.0 (2299) + EN 10365 (558) section database as typed sealed-singleton profile
  classes carrying `UnitsNet.Length` geometry, the `American`/`European` identity enums, the `AmericanShape`/
  `EuropeanShape` family enums, and `CatalogueFactory`
- Accept: a steel/EN section seeded into the Materials Profiles family axis by `CatalogueFactory`, its geometry
  read as `UnitsNet.Length`, its `IProfile` fed to the section-property solver
- Reject: a hand-keyed section dimension literal where the catalogue carries the published value; a raw-`double`
  read of a `UnitsNet.Length` dimension; a blind `((II)entry)` cast on a non-I family (use the
  `AmericanShape`/`EuropeanShape`-selected family interface, or the polymorphic `IProfile` solver path); a parallel
  section-family enum duplicating `AmericanShape`/`EuropeanShape`
