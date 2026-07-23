# [RASM_MATERIALS_API_VIVIDORANGE_PROFILES_CATALOGUE]

`VividOrange.Profiles.Catalogue` mints the published AISC and EN 10365:2017 section database as typed sealed-singleton profile classes, each carrying its geometry as `UnitsNet.Length` dimensions behind the `VividOrange.IProfiles` family contracts. `CatalogueFactory` maps a section-identity enum to an `ICatalogue`/`IProfile`; the catalogue owns the DATA half of the Materials Profiles section-property seam, and `VividOrange.Sections.SectionProperties` consumes each `IProfile` as the computation half. Published data seeds the Profiles steel and EN family axis over hand-keyed section literals.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `VividOrange.Profiles.Catalogue`
- package: `VividOrange.Profiles.Catalogue` (MIT)
- assembly: `VividOrange.Profiles.Catalogue`
- namespace: `VividOrange.Profiles`
- asset: runtime library, pure-managed AnyCPU, no native RID; the `net10.0` consumer binds `lib/net8.0`
- floor: geometry contracts and `UnitsNet.Length` dimensions ride the transitive `VividOrange.IProfiles`; the `ITaxonomySerializable` JSON taxonomy contract rides `VividOrange.ISerialization`; `UnitsNet` is the shared quantity floor
- rail: profiles (section data)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: catalogue factory + entry contracts

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                                  |
| :-----: | :------------------- | :------------ | :-------------------------------------------- |
|  [01]   | `CatalogueFactory`   | class         | mint a singleton `ICatalogue` by identity     |
|  [02]   | `IAmericanCatalogue` | interface     | American entry; carries `AmericanShape Shape` |
|  [03]   | `IEuropeanCatalogue` | interface     | European entry; carries `EuropeanShape Shape` |

[PUBLIC_TYPE_SCOPE]: section-identity + shape-family enums — `American` keys `CreateAmerican`, `European` keys `CreateEuropean`

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [CAPABILITY]                     |
| :-----: | :-------------- | :------------ | :------------------------------- |
|  [01]   | `American`      | enum          | published AISC section names     |
|  [02]   | `European`      | enum          | published EN 10365 section names |
|  [03]   | `AmericanShape` | enum          | AISC section-family discriminant |
|  [04]   | `EuropeanShape` | enum          | EN section-family discriminant   |

[AMERICAN_SHAPE]: `W` `M` `S` `HP` `C` `MC` `L` `WT` `MT` `ST` `DoubleL` `HSS` `Pipe`.
[EU_SHAPE]: `IPEAA` `IPEA` `IPE` `IPEO` `IPEV` `HEAA` `HEA` `HEB` `HEC` `HEM` `HE` `HL` `HLZ` `HD` `HP` `UBP` `UB` `UC` `IPN` `J` `UPE` `PFC` `UPN` `U` `CH`.
[IDENTITY_SPELLING]: `W44x408` `HSS10x10x_188` `Pipe…` exemplify the `American`/`European` section-name form.

[PUBLIC_TYPE_SCOPE]: inherited geometry contracts (transitive `VividOrange.IProfiles` floor, consumed not redefined)

`AmericanShape`/`EuropeanShape` selects the valid family cast on each concrete profile; a non-matching cast raises `InvalidCastException`, while the section-property solver instead consumes the polymorphic `IProfile` perimeter and integrates every family uniformly. Every symbol below is a `VividOrange.IProfiles` interface.

[BASE]: `IProfile` `ICatalogue` `ITaxonomySerializable`.
[I_FAMILY]: `II` `IIParallelFlange` `IITaperFlange`.
[CHANNEL]: `IChannel` `IChannelParallelFlange` `IChannelTaperFlange`.
[TEE_ANGLE]: `ITee` `IAngle` `IDoubleAngle`.
[RECTANGULAR]: `IRectangle` `IRectangularHollow` `IRoundedRectangularHollow`.
[CIRCULAR]: `ICircle` `ICircularHollow`.
[WALL]: `IHollowStructuralSection`.

[DIMENSIONS]: every dimension member is a `UnitsNet.Length`.
- I-family: `Height` `Width` `FlangeThickness` `WebThickness`; `IIParallelFlange.FilletRadius`, `IITaperFlange` the taper variant
- channel / tee / angle: `Height` `Width` `WebThickness` `FlangeThickness`; `IDoubleAngle.BackToBackDistance`
- rectangular / circular: `Height`/`Width` or `Diameter`; the hollow variants add `IHollowStructuralSection.Thickness`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: catalogue construction + entry properties

| [INDEX] | [SURFACE]                                   | [SHAPE]  | [CAPABILITY]               |
| :-----: | :------------------------------------------ | :------- | :------------------------- |
|  [01]   | `CatalogueFactory.CreateAmerican(American)` | static   | named AISC `ICatalogue`    |
|  [02]   | `CatalogueFactory.CreateEuropean(European)` | static   | named EN `ICatalogue`      |
|  [03]   | `IAmericanCatalogue.Shape`                  | property | `AmericanShape` family     |
|  [04]   | `IEuropeanCatalogue.Shape`                  | property | `EuropeanShape` family     |
|  [05]   | `ICatalogue.Type`                           | property | section identity (`Enum`)  |
|  [06]   | `ICatalogue.Label`                          | property | display label              |
|  [07]   | `ICatalogue.Catalogue`                      | property | standard provenance        |
|  [08]   | `IProfile.Description`                      | property | thin-space label rendering |

- Dimension access is a family-selected cast returning `UnitsNet.Length`: `((II)entry).FlangeThickness`, `((ICircularHollow)entry).Diameter`, `((IHollowStructuralSection)entry).Thickness`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Each entry is a sealed CRTP singleton over `SingletonCatalogueBase<T>` (`SingletonAmericanBase<T>` / `SingletonEuropeanBase<T>`): identity is reference equality, geometry is immutable published data, and the base folds `Type => Shape` with a constant `Catalogue` provenance.
- A dimension travels in its native published unit — AISC `LengthUnit.Inch`, EN 10365 `LengthUnit.Millimeter` — the value carrying its unit so `UnitsNet` owns every conversion and no millimetre literal is re-keyed.
- `AmericanShape` / `EuropeanShape` is the typed discriminant that ALSO selects the geometry cast, so a Materials Profiles `[SmartEnum]` family axis maps directly onto it — the AISC/EN family taxonomy IS the section discriminant.

[STACKING]:
- `VividOrange.Sections.SectionProperties`(`.api/api-vividorange-sections-sectionproperties.md`): `new SectionProperties(CatalogueFactory.CreateAmerican(American.W44x408))` — the produced `IProfile` is the solver's input, so one polygon integral computes `Area`/`MomentOfInertiaYy`/`ElasticSectionModulusZz` over every catalogued section with no per-family literal.
- `UnitsNet`(`libs/csharp/.api/api-unitsnet.md`): a catalogued AISC (`Inch`), catalogued EN (`Millimeter`), and measured/parametric section are the SAME `UnitsNet.Length` type, so the Materials family axis folds them through one typed surface and the family gate `q.Dimensions.Equals(Length.Info.BaseDimensions)` applies uniformly.
- within-lib: `CatalogueFactory` seeds the Materials Profiles `[SmartEnum]` steel + EN family axis; a catalogued section reaching the material wire carries its `American`/`European` identity token and boundary-unit-normalized dimensions through the Thinktecture-generated `System.Text.Json` codec, and the package's own `ITaxonomySerializable` taxonomy JSON stays off the repo wire.

[LOCAL_ADMISSION]:
- `CatalogueFactory` admits only through the Materials Profiles boundary that seeds the steel/EN section families; `American`/`European` and the `ICatalogue` instance map onto the canonical Profile owner at the edge.
- Geometry is read as `UnitsNet.Length` in its native unit and never converted inside an interior signature; the `UnitsNet` boundary (`libs/csharp/.api/api-unitsnet.md`) owns any rescale.
- A Profiles family that needs an AISC/EN section reads it from the catalogue, never from an inline dimension table.

[RAIL_LAW]:
- Package: `VividOrange.Profiles.Catalogue` (MIT)
- Owns: the published AISC + EN 10365 section database as typed sealed-singleton profiles carrying `UnitsNet.Length` geometry, the `American`/`European` identity enums, the `AmericanShape`/`EuropeanShape` family enums, and `CatalogueFactory`
- Accept: a steel/EN section seeded into the Materials Profiles family axis by `CatalogueFactory`, geometry read as `UnitsNet.Length`, its `IProfile` fed to the section-property solver
- Reject: a hand-keyed section-dimension literal where the catalogue carries the published value; a raw-`double` read of a `UnitsNet.Length` dimension; a blind `((II)entry)` cast on a non-I family; a parallel section-family enum duplicating `AmericanShape`/`EuropeanShape`
