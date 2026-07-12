# [RASM_MATERIALS_API_VIVIDORANGE_PROFILES_CATALOGUE]

`VividOrange.Profiles.Catalogue` supplies the published structural-section database as a typed catalogue of generated profile classes: 2299 American sections from the AISC Shapes Database and 558 European sections from EN 10365:2017. Each profile is a sealed CRTP singleton (`SingletonAmericanBase<T>` or `SingletonEuropeanBase<T>`) carrying cross-section geometry as `UnitsNet.Length` dimensions through the `VividOrange.IProfiles` family contracts: `II` and `IIParallelFlange` for I-sections, `IChannel`, `ITee`, `IAngle` and `IDoubleAngle`, and `IRectangularHollow` or `ICircularHollow` with `IHollowStructuralSection` for HSS and Pipe.

`CatalogueFactory.CreateAmerican` and `CatalogueFactory.CreateEuropean` map section-identity enum values to `ICatalogue` and `IProfile` instances. The catalogue owns the data half of the Materials Profiles section-property seam; `VividOrange.Sections.SectionProperties` (`api-vividorange-sections-sectionproperties.md`) consumes each produced `IProfile` as its computation half. Registered published data grounds the Profiles steel and EN section-geometry seed, replacing hand-keyed section literals while composing the in-folder `UnitsNet` quantity owner (`api-unitsnet.md`).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `VividOrange.Profiles.Catalogue`
- package: `VividOrange.Profiles.Catalogue`
- license: MIT (`licenses.nuget.org/MIT` — MagmaWorks / VividOrange taxonomy)
- assembly: `VividOrange.Profiles.Catalogue`
- namespace: `VividOrange.Profiles`
- asset: runtime library, pure-managed AnyCPU, no native RID asset
- target frameworks: `net8.0`, `net7.0`, `net6.0`, and `net48`; the `net10.0` consumer binds `lib/net8.0`
- geometry floor: `VividOrange.IProfiles` and `VividOrange.Profiles` own the geometry contracts and `UnitsNet.Length` dimension properties
- catalogue contribution: section-identity enums, generated concrete-profile singletons, and `CatalogueFactory`
- serialization floor: `VividOrange.ISerialization` owns the `ITaxonomySerializable` JSON taxonomy contract
- rail: profiles (section data)
- ABI floor: a PRE-1.0 contract — the enum member set and interface shapes may break across a minor bump. The full transitive floor (`VividOrange.IProfiles`, `VividOrange.ISerialization`) is centrally pinned at for deterministic restore; the geometry transitives `VividOrange.Geometry`/`VividOrange.IGeometry` ride `. `UnitsNet` is the shared quantity floor.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: catalogue factory + contracts
- rail: profiles

`CatalogueFactory.CreateAmerican(American)` and `CatalogueFactory.CreateEuropean(European)` mint singleton `ICatalogue` instances through the factory switch. Both profile contracts compose `ICatalogue`, `IProfile`, and `ITaxonomySerializable`; their `Shape` property carries the matching family enum.

| [INDEX] | [SYMBOL]             | [PACKAGE_ROLE]   | [CAPABILITY]             |
| :-----: | :------------------- | :--------------- | :----------------------- |
|  [01]   | `CatalogueFactory`   | `static` factory | singleton construction   |
|  [02]   | `IAmericanCatalogue` | profile contract | American catalogue entry |
|  [03]   | `IEuropeanCatalogue` | profile contract | European catalogue entry |

[SHAPE_PROPERTIES]:
- `IAmericanCatalogue`: `AmericanShape Shape`
- `IEuropeanCatalogue`: `EuropeanShape Shape`

[PUBLIC_TYPE_SCOPE]: section-identity + shape-family enums
- rail: profiles

`American` keys `CreateAmerican`, and `European` keys `CreateEuropean`; the family enums carry the published family vocabularies.

| [INDEX] | [SYMBOL]        | [PACKAGE_ROLE] | [CATALOGUE_SCOPE]       |
| :-----: | :-------------- | :------------- | :---------------------- |
|  [01]   | `American`      | identity enum  | 2299 AISC section names |
|  [02]   | `European`      | identity enum  | 558 EN section names    |
|  [03]   | `AmericanShape` | family enum    | 13 AISC families        |
|  [04]   | `EuropeanShape` | family enum    | 25 EN families          |

[AMERICAN_IDENTITIES]: `W44x408`, `HSS…`, and `Pipe…` exemplify the published section-name vocabulary.
[AMERICAN_SHAPE]: `W` `M` `S` `HP` `C` `MC` `L` `WT` `MT` `ST` `DoubleL` `HSS` `Pipe`.
[EUROPEAN_SHAPE]: `IPEAA` `IPEA` `IPE` `IPEO` `IPEV` `HEAA` `HEA` `HEB` `HEC` `HEM` `HE` `HL` `HLZ` `HD` `HP` `UBP` `UB` `UC` `IPN` `J` `UPE` `PFC` `UPN` `U` `CH`.

[PUBLIC_TYPE_SCOPE]: inherited geometry contracts (transitive floor, consumed not redefined)
- rail: profiles

The geometry these catalogue entries carry is the transitive `VividOrange.IProfiles` floor. Each concrete profile implements its family contracts, so `AmericanShape` or `EuropeanShape` selects the valid cast: `II` and `IIParallelFlange` for W, HEA, and IPE; `IChannel` for C, MC, and UPE; `ITee` for WT, MT, and ST; `IAngle` or `IDoubleAngle` for L and DoubleL; and `IRectangularHollow` or `ICircularHollow` with `IHollowStructuralSection` for HSS and Pipe. Casting a non-I family to `II` raises `InvalidCastException`; the section-property solver (`api-vividorange-sections-sectionproperties.md`) instead consumes the polymorphic `IProfile` perimeter and integrates every family uniformly.

| [INDEX] | [SYMBOL]                    | [PACKAGE_ROLE]       |
| :-----: | :-------------------------- | :------------------- |
|  [01]   | `IProfile`                  | base contract        |
|  [02]   | `ICatalogue`                | catalogue contract   |
|  [03]   | `II`                        | I-family geometry    |
|  [04]   | `IIParallelFlange`          | I-family geometry    |
|  [05]   | `IITaperFlange`             | I-family geometry    |
|  [06]   | `IChannel`                  | channel geometry     |
|  [07]   | `IChannelParallelFlange`    | channel geometry     |
|  [08]   | `IChannelTaperFlange`       | channel geometry     |
|  [09]   | `ITee`                      | tee geometry         |
|  [10]   | `IAngle`                    | angle geometry       |
|  [11]   | `IDoubleAngle`              | angle geometry       |
|  [12]   | `IRectangle`                | rectangular geometry |
|  [13]   | `IRectangularHollow`        | rectangular geometry |
|  [14]   | `IRoundedRectangularHollow` | rectangular geometry |
|  [15]   | `ICircle`                   | circular geometry    |
|  [16]   | `ICircularHollow`           | circular geometry    |
|  [17]   | `IHollowStructuralSection`  | wall-thickness facet |
|  [18]   | `ITaxonomySerializable`     | serialization        |

[BASE_CONTRACTS]:
- `IProfile`: `string Description` plus `ITaxonomySerializable` for every profile
- `ICatalogue`: `Catalogue Catalogue` for provenance, `Enum Type` for identity, and `string Label`

[GEOMETRY_MEMBERS]: all dimension members are `UnitsNet.Length`.
- I-family: `Height`, `Width`, `FlangeThickness`, and `WebThickness`; `IIParallelFlange` adds `FilletRadius` for W, HEA, and IPE, while `IITaperFlange` carries the S and HP taper-flange variant
- Channel: `Height`, `Width`, `WebThickness`, and `FlangeThickness` for C, MC, UPE, PFC, and UPN
- Tee: `Height`, `Width`, `FlangeThickness`, and `WebThickness` for WT, MT, and ST through `ICutTeeParallelFlange` or `ICutTeeTaperFlange`
- Angle: `Height`, `Width`, `WebThickness`, and `FlangeThickness`; `IDoubleAngle: IAngle, IBackToBack` adds `BackToBackDistance` for L and DoubleL
- Rectangular: `Height` and `Width`; the hollow variants add `IHollowStructuralSection.Thickness` for rectangular HSS
- Circular: `Diameter`; `ICircularHollow` adds `IHollowStructuralSection.Thickness` for Pipe and round HSS
- Wall: `IHollowStructuralSection.Thickness` is the common wall facet implemented by every HSS and Pipe entry
- Serialization: `ITaxonomySerializable` is the MagmaWorks JSON taxonomy contract from `VividOrange.ISerialization`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: catalogue construction
- rail: profiles

Factory calls return geometry-bearing `ICatalogue` instances; the property surfaces expose their family, identity, label, and provenance.

| [INDEX] | [SURFACE]                                   | [CALL_SHAPE] | [CAPABILITY]        |
| :-----: | :------------------------------------------ | :----------- | :------------------ |
|  [01]   | `CatalogueFactory.CreateAmerican(American)` | factory call | named AISC section  |
|  [02]   | `CatalogueFactory.CreateEuropean(European)` | factory call | named EN section    |
|  [03]   | `IAmericanCatalogue.Shape`                  | property     | `AmericanShape`     |
|  [04]   | `IEuropeanCatalogue.Shape`                  | property     | `EuropeanShape`     |
|  [05]   | `ICatalogue.Type`                           | property     | section identity    |
|  [06]   | `ICatalogue.Label`                          | property     | display label       |
|  [07]   | `ICatalogue.Catalogue`                      | property     | standard provenance |

[DIMENSION_SURFACES]: every accessor returns `UnitsNet.Length` through its family contract.
- I-family: `((II)catalogue).Height`, `.Width`, `.FlangeThickness`, `.WebThickness`, and `.FilletRadius` expose W, HEA, and IPE dimensions through `II` or `IIParallelFlange`
- C, WT, L, and DoubleL: `((IChannel)c).WebThickness`, `((ITee)c).FlangeThickness`, `((IAngle)c).Width`, and `((IDoubleAngle)c).BackToBackDistance`
- HSS and Pipe: `((IRectangularHollow)c).Height`, `((ICircularHollow)c).Diameter`, and `((IHollowStructuralSection)c).Thickness` expose envelope and wall thickness

## [04]-[IMPLEMENTATION_LAW]

[CATALOGUE_ALGEBRA]:
- identity root: the `American` (2299) / `European` (558) section-name enums
- family root: `AmericanShape` (13 AISC families) / `EuropeanShape` (EN families)
- factory root: `CatalogueFactory.CreateAmerican` / `CreateEuropean`
- geometry root: the `UnitsNet.Length` dimension properties on the per-family shape interfaces (`II`/`IChannel`/`ITee`/`IAngle` `Height`/`Width`/…, `IRectangle`/`ICircle`, `IHollowStructuralSection.Thickness`, `IBackToBack.BackToBackDistance`)
- provenance root: `ICatalogue.Catalogue` + `ITaxonomySerializable`

[GEOMETRY_CONTRACT]:
- Every concrete profile is a sealed singleton over a CRTP base: `W44x408: SingletonAmericanBase<W44x408>` and `IPE100: SingletonEuropeanBase<IPE100>` both derive `SingletonCatalogueBase<T>`.
- Each singleton implements its family geometry contracts: `W44x408: IIParallelFlange, II`; `HSS…: IRoundedRectangularHollow, IHollowStructuralSection`; `Pipe…: ICircularHollow, IHollowStructuralSection`; `C…: IChannel`; `L…: IAngle`; and `WT…: ITee`.
- Identity equality is reference equality; the base folds `Type => Shape` and `Catalogue`, and geometry remains immutable published data rather than a mutable record.
- Dimensions are `UnitsNet.Length`, NOT raw `double`, and are carried in their NATIVE published unit — AISC
  data is `LengthUnit.Inch` (`new Length(, Inch)`), EN 10365 data is `LengthUnit.Millimeter`. Neither is the SI
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
- wire contract seam: `ITaxonomySerializable` is the MagmaWorks JSON taxonomy contract.
- wire codec seam: a catalogued section reaching `interchange#MATERIAL_WIRE` serializes through the canonical Thinktecture-generated `System.Text.Json` codec owned by the in-folder `Thinktecture.Runtime.Extensions.Json` wire surface.
- wire payload seam: the codec carries the `American` or `European` identity token plus dimensions normalized to one boundary wire unit for TS and Python peers; the package taxonomy JSON and raw mixed Inch/Millimeter quantities remain outside the wire.

[RAIL_LAW]:
- Package: `VividOrange.Profiles.Catalogue` (MIT, pure-managed AnyCPU, `net10.0` binds `net8.0`, PRE-1.0 contract)
- Owns: the published AISC (2299) + EN 10365 (558) section database as typed sealed-singleton profile
  classes carrying `UnitsNet.Length` geometry, the `American`/`European` identity enums, the `AmericanShape`/
  `EuropeanShape` family enums, and `CatalogueFactory`
- Accept: a steel/EN section seeded into the Materials Profiles family axis by `CatalogueFactory`, its geometry
  read as `UnitsNet.Length`, its `IProfile` fed to the section-property solver
- Reject: a hand-keyed section dimension literal where the catalogue carries the published value; a raw-`double`
  read of a `UnitsNet.Length` dimension; a blind `((II)entry)` cast on a non-I family (use the
  `AmericanShape`/`EuropeanShape`-selected family interface, or the polymorphic `IProfile` solver path); a parallel
  section-family enum duplicating `AmericanShape`/`EuropeanShape`
