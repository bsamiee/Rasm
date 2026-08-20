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
- with `VividOrange.Cases` (`.api/api-vividorange-cases`) + `VividOrange.IStandards` (`.api/api-vividorange-istandards`): `Country` is the broad axis and `NationalAnnex` the Eurocode parameter axis every `ENLoadCaseFactory`/`ENCombinationFactory` and `GetProperties` call dispatches on; no compiled `Country`→`NationalAnnex` member exists, so `Model/eurocode#EUROCODE_ALGEBRA` `AnnexRegime.Of(ICountry)` holds the correspondence keyed on `CountryCode`, that owner naming the name-equality misses
- with `VividOrange.Stages` (`.api/api-vividorange-stages`): `IGovernance.Country` returns this `ICountry` over a compiled pin, so a governing body's nation (RIBA/HOAI/CSLP/AB89) is a typed read — `Planning/schedule#SCHEDULE` `ProjectStage.Nation` states the body-to-nation law and `StageLabels.Nation` derives the project's context off the composed roster, excluding the `International` body whose `Country` is Whitby Wood's own `UnitedKingdom` domicile
- with `VividOrange.ISerialization`: `ICountry: ITaxonomySerializable` rides the shared taxonomy-serialization marker
- with `StructuralAnalysisFormat` (`.api/api-structural-analysis-format`): the `AnnexRegime` row an `ICountry` admits onto carries the SAF `ExcelNationalCode` member as its own KEY, so `Exchange/saf#SAF_EXCHANGE` `Workbook` writes `ExcelModelInformation.NationalCode` off that roster with no second table; `ExcelStructuralLoadCombination.NationalStandard` is the separate combination-clause axis (`ExcelLoadCaseCombinationStandard`), not the design code

[LOCAL_ADMISSION]:
- this branch reaches `ICountry` through TWO landed paths — `IGovernance.Country` off a `VividOrange.Stages` governing body, resolved once at `Planning/schedule#SCHEDULE` `StageLabels.Nation`, and `Model/eurocode#EUROCODE_ALGEBRA` `AnnexRegime.Of(ICountry)` admitting that nation onto its design regime — and `Utility.GetCountry` is the enum→singleton resolver any other reach takes, never a `new` per call and never a free-text country string
- `CountryCode` is the ONLY key a Bim fence matches an `ICountry` on — unique across all 249 nations where the display `Name` diverges from the annex spellings — and it carries the nation onto `NationalAnnex` and from there onto the SAF `ExcelNationalCode`
- `AnnexRegime.Recommended` (`NationalAnnex.RecommendedValues`, SAF `EC_Standard_EN`) receives every nation with no national annex: the EN regime, never a fault and never an absent factor set

[RAIL_LAW]:
- Package: `VividOrange.Countries`
- Owns: the ISO 3166-1 national-context taxonomy — enum, per-nation singleton family, and resolver
- Accept: national context held as `Country`/`ICountry`; resolution through `Utility.GetCountry`; the ISO `CountryCode` as the stable boundary key
- Reject: a free-text country string, a parallel nation enum beside this one, a claim that a compiled `Country`→`NationalAnnex` map ships here, a match on the display `Name` where the ISO code is the stable key, `new`-ing a singleton where `Utility.GetCountry` is canonical, and reading `International.Country` as a project nation where it is Whitby Wood's own domicile
