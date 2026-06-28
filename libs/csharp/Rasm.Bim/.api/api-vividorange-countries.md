# [RASM_BIM_API_VIVIDORANGE_COUNTRIES]

`VividOrange.Countries` is the national-context taxonomy: the full ISO 3166-1 nation set as a
closed `Country` enum plus a per-nation singleton class family (`Germany`, `France`, …, 249 in
total) behind the `ICountry` contract, each carrying its display `Name` and its ISO 3166-1
alpha-2 `CountryCode` (`"DE"`, `"FR"`). It is the SHARED national-context axis the whole VividOrange
family reads, NOT a structural-only taxonomy — a model declares its nation once as a `Country`/`ICountry`,
the `VividOrange.Cases` Eurocode engine (`.api/api-vividorange-cases`) selects the matching `NationalAnnex`
parameter set for its `ψ`/`γ` factor tables, and the `VividOrange.Stages` lifecycle taxonomy
(`.api/api-vividorange-stages`) reads the very same `ICountry` off its `IGovernance.Country` to record the
nationality of each governing body (RIBA→UK, HOAI→Germany, CSLP→Italy). One national-context owner backs
both the structural load/case standards key and the lifecycle stage governance. The enum→singleton
resolution is the one entrypoint: `Utility.GetCountry(Country)` maps an enum member to its
`ICountry` instance, and each singleton is a CRTP `SingletonCountryBase<T>` exposing a lazily
constructed `T Default`. Every country is `ICountry : ITaxonomySerializable`, so the national
context round-trips through the same VividOrange taxonomy-serialization marker as loads and cases —
one serialization seam for the whole structural taxonomy. The package STACKS with the Eurocode
`NationalAnnex` (the `VividOrange.Standards.Eurocode` enum, a 37-member CEN subset) the `Cases`
factories dispatch on, and maps onto the SAF `ExcelNationalCode` design-code axis
(`.api/api-structuralanalysisformat`) at the XLSX boundary; the `Country`↔`NationalAnnex` mapping
is BY NAME at the design layer, not a compiled member (the standards-body abbreviation table that
backs it is internal to `VividOrange.Standards`).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `VividOrange.Countries`
- package: `VividOrange.Countries` (contract + enum + singletons together; no separate interface package — `ICountry` ships in this assembly)
- version: `0.1.0`
- license: MIT
- assembly: `VividOrange.Countries`
- namespace: `VividOrange.Countries`
- asset: multi-target `net48`/`net6.0`/`net7.0`/`net8.0`/`netstandard2.0`; the `net10.0` consumer binds `lib/net8.0`
- asset: pure-managed AnyCPU IL-only assembly; no native binaries; ALC-safe inside the in-Rhino plugin assembly
- dependency: `VividOrange.ISerialization` (the `ITaxonomySerializable` marker) only — no `UnitsNet`, no `IStandards`; Countries is a leaf taxonomy
- rail: national-context

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: national-context family
- rail: national-context

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]              | [RAIL]                                                       |
| :-----: | :-------------------------- | :------------------------ | :--------------------------------------------------------- |
|  [01]   | `ICountry`                  | nation contract           | `: ITaxonomySerializable` + `string Name`, `string CountryCode` |
|  [02]   | `Country`                   | ISO 3166-1 nation enum    | 249 members (`Afghanistan`…`Zimbabwe`)                     |
|  [03]   | `SingletonCountryBase<T>`   | CRTP singleton base       | `abstract … where T : SingletonCountryBase<T>` + `static T Default` (lazy) |
|  [04]   | `Germany`/`France`/`…`      | per-nation singleton      | `sealed : SingletonCountryBase<T>, ICountry` — `Name`, `CountryCode` (`"DE"`…) |
|  [05]   | `Utility`                   | enum→singleton resolver   | `static ICountry GetCountry(Country)`                       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: nation resolution and read
- rail: national-context

| [INDEX] | [SURFACE]                          | [ENTRY_FAMILY]      | [RAIL]                                                  |
| :-----: | :--------------------------------- | :------------------ | :----------------------------------------------------- |
|  [01]   | `Utility.GetCountry(Country)`      | enum→instance       | resolve a `Country` enum member to its `ICountry` singleton |
|  [02]   | `Germany.Default` (`SingletonCountryBase<T>.Default`) | typed singleton | the lazily constructed per-nation instance             |
|  [03]   | `new Germany()` (and family)       | direct construct    | a per-nation singleton class instance                  |
|  [04]   | `ICountry.Name`                    | display read        | the nation display name                                |
|  [05]   | `ICountry.CountryCode`             | ISO code read       | the ISO 3166-1 alpha-2 code (`"DE"`, `"FR"`, `"US"`)    |

## [04]-[IMPLEMENTATION_LAW]

[COUNTRY_TOPOLOGY]:
- namespace: `VividOrange.Countries`
- contract: `ICountry : ITaxonomySerializable` — `Name`, `CountryCode` (ISO 3166-1 alpha-2)
- enum: `Country` — the full ISO 3166-1 nation set (249 members), the discriminant `Utility.GetCountry` switches on
- singletons: 249 `sealed` classes (`Germany : SingletonCountryBase<Germany>, ICountry`), each a CRTP singleton whose `Default` is `Lazy<T>` over a non-public ctor
- resolver: `Utility.GetCountry(Country)` is the one enum→`ICountry` map

[LOCAL_ADMISSION]:
- a model's national context is held as one `Country`/`ICountry`, resolved through `Utility.GetCountry`, never a free-text country string; the ISO `CountryCode` is the stable boundary key for georeference, addressing, and design-code selection
- the Eurocode design-code regime is NOT read off `ICountry` directly — the `Cases` factories dispatch on `NationalAnnex` (the CEN subset), and the design layer maps the project nation onto a `NationalAnnex` by name, defaulting to `NationalAnnex.RecommendedValues` for a non-CEN nation
- the singleton classes are immutable read-only nation records; treat `Utility.GetCountry` as the canonical access path rather than `new`-ing a country per call

[STACKING]:
- with `VividOrange.Cases` + `VividOrange.IStandards` (`.api/api-vividorange-cases`): `Country` (249 ISO nations) is the broad national axis; `NationalAnnex` (37 CEN members incl. `RecommendedValues`) is the Eurocode parameter axis the `ENLoadCaseFactory`/`ENCombinationFactory` and the `ITableA1_1`/`ITableA1_2` `GetProperties` dispatch on — the two meet at the project's nation, bridged by name at the design layer (the `Country`→standards-body map is internal to `VividOrange.Standards`, not a public member)
- with `VividOrange.Stages` (`.api/api-vividorange-stages`): the lifecycle stage taxonomy's `IGovernance.Country` returns this `ICountry` (a compiled `VividOrange.IStages`→`VividOrange.Countries` pin), so the project-phase family reads the SAME national-context owner — the country a governing body (RIBA/HOAI/CSLP/AB89) belongs to is the same `Utility.GetCountry` value the structural standards key selects, never a parallel nation enum on the stage side; Countries is the one national-context owner across BOTH the structural (loads/cases) and lifecycle (stages) VividOrange families
- with `VividOrange.ISerialization`: `ICountry : ITaxonomySerializable` shares the one taxonomy-serialization marker with loads, cases, AND stages — every VividOrange taxonomy carries the same marker, so the national context serializes through the one rail that covers the whole family
- with `StructuralAnalysisFormat` (`.api/api-structuralanalysisformat`): the SAF `ExcelNationalCode` enum (`EC_DIN_EN`, `EC_NF_EN`, `EC_UNI_EN`, … plus `IBC`/`NBR`/`SIA_26x`) is the SAF design-code axis; a model's `ICountry` + selected `NationalAnnex` map onto the matching `ExcelNationalCode` at the XLSX boundary, and the `ExcelStructuralLoadCombination.NationalStandard` carries it on the wire
- with `Thinktecture.Runtime.Extensions` (`.api/api-thinktecture-json`): when the Bim layer needs a canonical national discriminant it owns a `[SmartEnum]`/`[ValueObject]` keyed by the ISO `CountryCode`; the VividOrange `Country` enum + `ICountry` are the boundary vocabulary mapped onto it, never re-exported as the canonical shape
- with `NetTopologySuite`/`ProjNET` (the `Semantics/georeference` owner): the ISO `CountryCode` keys the national CRS/datum and georeferenced site-context selection, joining the national-context taxonomy to the geospatial seam

[RAIL_LAW]:
- Package: `VividOrange.Countries`
- Owns: the ISO 3166-1 national-context taxonomy (enum + singleton family + resolver)
- Accept: national context held as `Country`/`ICountry`; resolution through `Utility.GetCountry`; the ISO `CountryCode` as the stable boundary key
- Reject: free-text country strings, a parallel nation enum on the Bim graph, a fabricated compiled `Country`→`NationalAnnex` mapping (the bridge is by-name at the design layer), `new`-ing a country singleton where `Utility.GetCountry` is the canonical access path
