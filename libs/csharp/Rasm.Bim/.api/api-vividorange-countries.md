# [RASM_BIM_API_VIVIDORANGE_COUNTRIES]

`VividOrange.Countries` owns the ISO 3166-1 national-context taxonomy: a closed `Country` enum, one `sealed` `ICountry` singleton per nation, and `Utility.GetCountry` mapping each member to its instance. One national-context owner serves both the structural and lifecycle VividOrange families, and its ISO alpha-2 `CountryCode` keys georeference, addressing, and design-code selection; every nation round-trips through the shared `ITaxonomySerializable` marker.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `VividOrange.Countries`
- package: `VividOrange.Countries` (contract, enum, and singletons in one assembly; `ICountry` ships here) (MIT)
- assembly: `VividOrange.Countries`
- namespace: `VividOrange.Countries`
- asset: multi-target `net48`/`net6.0`/`net7.0`/`net8.0`/`netstandard2.0`; the `net10.0` consumer binds `lib/net8.0`
- asset: pure-managed AnyCPU IL-only assembly; no native binaries; ALC-safe inside the in-Rhino plugin assembly
- dependency: `VividOrange.ISerialization` (the `ITaxonomySerializable` marker) only; Countries is a leaf taxonomy
- rail: national-context

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: national-context family

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]           | [CAPABILITY]                                                        |
| :-----: | :------------------------ | :---------------------- | :------------------------------------------------------------------ |
|  [01]   | `ICountry`                | nation contract         | `: ITaxonomySerializable`; `string Name`, `string CountryCode`      |
|  [02]   | `Country`                 | ISO 3166-1 nation enum  | 249 members `Afghanistan`…`Zimbabwe`                                |
|  [03]   | `SingletonCountryBase<T>` | CRTP singleton base     | `where T: SingletonCountryBase<T>`; `static T Default` lazy         |
|  [04]   | `Germany`/`France`/`…`    | per-nation singleton    | `sealed: SingletonCountryBase<T>, ICountry`; `CountryCode` (`"DE"`) |
|  [05]   | `Utility`                 | enum→singleton resolver | `static ICountry GetCountry(Country)`                               |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: nation resolution and read

| [INDEX] | [SURFACE]                     | [SHAPE]  | [CAPABILITY]                                           |
| :-----: | :---------------------------- | :------- | :----------------------------------------------------- |
|  [01]   | `Utility.GetCountry(Country)` | static   | resolve a `Country` member to its `ICountry` singleton |
|  [02]   | `Germany.Default`             | static   | the lazily constructed per-nation singleton            |
|  [03]   | `new Germany()`               | ctor     | a per-nation singleton instance                        |
|  [04]   | `ICountry.Name`               | property | the nation display name                                |
|  [05]   | `ICountry.CountryCode`        | property | the ISO 3166-1 alpha-2 code (`"DE"`, `"FR"`)           |

- `Utility.GetCountry`: switches over all 249 declared members; an out-of-domain `(Country)` cast throws `NotImplementedException`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- each `Country` enum member maps 1:1 to a `sealed` `ICountry` singleton; `Utility.GetCountry` is the sole enum→instance resolver and `CountryCode` (ISO 3166-1 alpha-2) the stable boundary key
- five nations are non-ASCII C# identifiers — `Curaçao`, `CôteDivoire`, `Réunion`, `SaintBarthélemy`, `ÅlandIslands` — spelled verbatim at every call site

[STACKING]:
- with `VividOrange.Cases` (`.api/api-vividorange-cases`) + `VividOrange.IStandards` (`.api/api-vividorange-istandards`): `Country` is the broad national axis and `NationalAnnex` the Eurocode parameter axis the `ENLoadCaseFactory`/`ENCombinationFactory` and `ITableA1_1`/`ITableA1_2.GetProperties` dispatch on; the two bridge BY NAME at the design layer — no compiled `Country`→`NationalAnnex` member exists, and the abbreviation table is internal to `VividOrange.Standards`
- with `VividOrange.Stages` (`.api/api-vividorange-stages`): `IGovernance.Country` returns this `ICountry`, so a governing body's nation (RIBA/HOAI/CSLP) resolves through the same `Utility.GetCountry` value the structural standards key selects — one national-context owner across the structural and lifecycle families
- with `VividOrange.ISerialization`: `ICountry: ITaxonomySerializable` rides the shared taxonomy-serialization marker
- with `StructuralAnalysisFormat` (`.api/api-structural-analysis-format`): a model's `ICountry` and selected `NationalAnnex` map onto the SAF `ExcelNationalCode` design-code axis (`EC_DIN_EN`/`EC_NF_EN`/`EC_UNI_EN`…) at the XLSX boundary, carried on the wire by `ExcelStructuralLoadCombination.NationalStandard`

[LOCAL_ADMISSION]:
- this branch reaches `ICountry` through ONE landed path — `IGovernance.Country` off a `VividOrange.Stages` governing body — and `Utility.GetCountry` is the enum→singleton resolver any other reach takes, never a `new` per call and never a free-text country string
- design-code selection never reads off `ICountry`; the Eurocode regime is a `NationalAnnex` the composition elects, `NationalAnnex.RecommendedValues` covering a nation with no national annex

[RAIL_LAW]:
- Package: `VividOrange.Countries`
- Owns: the ISO 3166-1 national-context taxonomy — enum, per-nation singleton family, and resolver
- Accept: national context held as `Country`/`ICountry`; resolution through `Utility.GetCountry`; the ISO `CountryCode` as the stable boundary key
- Reject: a free-text country string, a parallel nation enum beside this one, a fabricated compiled `Country`→`NationalAnnex` map, `new`-ing a singleton where `Utility.GetCountry` is canonical
