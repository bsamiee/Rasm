# [RASM_BIM_API_VIVIDORANGE_ISTANDARDS]

`VividOrange.IStandards` is verified-transitive through direct package `VividOrange.Cases`: the Bim compile surface receives the Eurocode standards-identity taxonomy and the `NationalAnnex` dispatch key the rest of the VividOrange structural family reads.
It carries four concerns in one assembly: the `IStandard`/`StandardBody` standard-identity contract
(which standards body publishes a code — `EN`, `ACI`, `AISC`, `BS`, …), the closed `NationalAnnex`
enum (37 members: `RecommendedValues` — the EN standard fallback — plus 36 national annexes) that
parameterizes every Eurocode `ψ`/`γ` table lookup, the nine `En199xPart` part enums (one per Eurocode
EN 1990…EN 1999, naming the clause-part split), and the `MissingNationalAnnexException` fault type for
an unmapped annex. The CONCRETE `VividOrange.Standards` package (Materials-owned, not on the Bim
compile surface) realizes the contract as the `En1990`…`En1999` standard-identity classes, each pinning
`Body => StandardBody.EN`, carrying a settable `NationalAnnex` (and, except `En1990`, an `En199xPart`),
and deriving a composed `IStandard.Title` from an internal title kernel — `NationalAnnexUtility`
(annex → national standards-body abbreviation: `DIN`/`NF`/`BS`/`UNI`/…) and `En199xUtility`
(part → full clause description). That kernel's default arm, `_ => throw new MissingNationalAnnexException(na)`,
is the single throw site, reached transitively through `IStandard.Title`. This is the dispatch axis the
`Cases` Eurocode engine (`.api/api-vividorange-cases`) consumes — `NationalAnnex` keys every
`ENLoadCaseFactory`/`ENCombinationFactory` and `ITableA1_1`/`ITableA1_2.GetProperties` — and the axis
the `Countries` taxonomy (`.api/api-vividorange-countries`) bridges to a nation BY NAME (there is no
compiled `Country`→`NationalAnnex` map anywhere in the VividOrange surface). Every standard is
`IStandard: ITaxonomySerializable`, so standard identity round-trips through the same VividOrange
taxonomy-serialization marker as loads, cases, countries, and stages.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `VividOrange.IStandards`
- package: `VividOrange.IStandards` (contract — `IStandard`/`StandardBody`, `NationalAnnex`, the `En199xPart` parts, `MissingNationalAnnexException`); `VividOrange.Standards` (concrete — the `En1990`…`En1999` realization + the internal title kernel)
- license: MIT
- assembly: `VividOrange.IStandards`, `VividOrange.Standards`
- namespace: `VividOrange.Standards` (`IStandard`, `StandardBody`), `VividOrange.Standards.Eurocode` (`NationalAnnex`, `En1990Part`…`En1999Part`, `MissingNationalAnnexException`, the concrete `En1990`…`En1999`)
- asset: multi-target `net48`/`net6.0`/`net7.0`/`net8.0`/`netstandard2.0`; the `net10.0` consumer binds `lib/net8.0`
- asset: pure-managed AnyCPU IL-only assemblies; no native binaries; ALC-safe inside the in-Rhino plugin assembly
- dependency: `VividOrange.IStandards` → `VividOrange.ISerialization` (the `ITaxonomySerializable` marker) only; `VividOrange.Standards` → `VividOrange.IStandards`
- consumer: Bim consumes `VividOrange.IStandards` TRANSITIVELY via `VividOrange.Cases → VividOrange.ICases → VividOrange.IStandards` (the `NationalAnnex` key + `MissingNationalAnnexException` type); the concrete `VividOrange.Standards` (`En1990`…`En1999`) is referenced by `Rasm.Materials`
- rail: standards

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: standard-identity contract family (`VividOrange.IStandards`)
- rail: standards

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [RAIL] |
|:-----: |:----------------------------- |:------------------------ |:------------------------------------------------------------- |
| [01] | `IStandard` | standard-identity contract | `: ITaxonomySerializable` + `StandardBody Body`, `string Title` |
| [02] | `StandardBody` | publishing-body enum | 11 bodies `AASHTO`/`ACI`/`AISC`/`ANSI`/`AS`/`BS`/`CSA`/`EN`/`HK`/`IS`/`SANS` (each `[Description]`) |
| [03] | `Eurocode.NationalAnnex` | national-annex dispatch key | 37 members: `RecommendedValues` (EN fallback, =0) + 36 nations (`Austria`…`UnitedKingdom`) |
| [04] | `Eurocode.MissingNationalAnnexException` | unmapped-annex fault | `: Exception`; ctor `(NationalAnnex na)` → message `"National Annex of {na} not implemented"` |

[PUBLIC_TYPE_SCOPE]: Eurocode clause-part enums (`VividOrange.IStandards`)
- rail: standards

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [RAIL] |
|:-----: |:-------------- |:------------------------ |:------------------------------------------------------------- |
| [01] | `En1990Part` | EC0 part enum (basis) | `None` only — EN 1990 carries no clause-part split |
| [02] | `En1991Part` | EC1 actions parts | 10: `Part1_1`…`Part1_7`/`Part2`/`Part3`/`Part4` (Dead/Fire/Snow/Wind/Thermal/Construction/Accidental/Bridge/Crane/Silos) |
| [03] | `En1992Part` | EC2 concrete parts | 4: `Part1_1`/`Part1_2`/`Part2`/`Part3` (General/Fire/Bridges/Liquid-Retaining) |
| [04] | `En1993Part` | EC3 steel parts | 20: `Part1_1`…`Part1_12`/`Part2`/`Part3_1`/`Part3_2`/`Part4_1`/`Part4_2`/`Part4_3`/`Part5`/`Part6` |
| [05] | `En1994Part` | EC4 composite parts | 3: `Part1_1`/`Part1_2`/`Part2` (General/Fire/Bridges) |
| [06] | `En1995Part` | EC5 timber parts | 3: `Part1_1`/`Part1_2`/`Part2` (General/Fire/Bridges) |
| [07] | `En1996Part` | EC6 masonry parts | 4: `Part1_1`/`Part1_2`/`Part2`/`Part3` |
| [08] | `En1997Part` | EC7 geotechnical parts | 2: `Part1`/`Part2` (General/Ground-Investigations) |
| [09] | `En1998Part` | EC8 seismic parts | 6: `Part1`…`Part6` (General/Bridges/Retrofit/Silos/Geotech/Towers) |
| [10] | `En1999Part` | EC9 aluminium parts | 5: `Part1_1`…`Part1_5` (General/Fire/Fatigue/Cold-Formed/Shell) |

[PUBLIC_TYPE_SCOPE]: concrete Eurocode-standard realization (`VividOrange.Standards`)
- rail: standards

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [RAIL] |
|:-----: |:--------- |:--------------------------- |:------------------------------------------------------------- |
| [01] | `En1990` | EC0 basis of design | `: IStandard` + `NationalAnnex` (no `Part`); `Title` = "… EN 1990: Eurocode - Basis of Structural Design" |
| [02] | `En1991` | EC1 actions on structures | `: IStandard` + `En1991Part Part`, `NationalAnnex`; "Eurocode 1: Actions on Structures" |
| [03] | `En1992` | EC2 concrete | `: IStandard` + `En1992Part Part`, `NationalAnnex`; "Eurocode 2: Design of Concrete Structures" |
| [04] | `En1993` | EC3 steel | `En1993Part Part`; "Eurocode 3: Design of Steel Structures" |
| [05] | `En1994` | EC4 composite steel+concrete | `En1994Part Part`; "Eurocode 4: Design of Composite Steel & Concrete Structures" |
| [06] | `En1995` | EC5 timber | `En1995Part Part`; "Eurocode 5: Design of Timber Structures" |
| [07] | `En1996` | EC6 masonry | `En1996Part Part`; "Eurocode 6: Design of Masonry Structures" |
| [08] | `En1997` | EC7 geotechnical | `En1997Part Part`; "Eurocode 7: Geotechnical Design" |
| [09] | `En1998` | EC8 seismic | `En1998Part Part`; "Eurocode 8: Design of Structures for Earthquake Resistance" |
| [10] | `En1999` | EC9 aluminium | `En1999Part Part`; "Eurocode 9: Design of Aluminium Structures" |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: standard-identity construction and read
- rail: standards

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [RAIL] |
|:-----: |:-------------------------------------------------- |:------------------ |:----------------------------------------------------- |
| [01] | `new En1990(NationalAnnex)` / `new En1990()` | EC0 identity | basis-of-design standard for a national annex (no part) |
| [02] | `new En199x(En199xPart part, NationalAnnex na)` / `new En199x()` | parted identity | a Eurocode standard pinned to a clause part + national annex |
| [03] | `En199x.NationalAnnex { get; set; }` | annex set/read | the settable national-annex dispatch value on the concrete standard |
| [04] | `En199x.Part { get; set; }` | part set/read | the settable `En199xPart` clause discriminant (absent on `En1990`) |
| [05] | `IStandard.Body` | body read | the publishing `StandardBody` (always `EN` for the `En199x` family) |
| [06] | `IStandard.Title` | composed identity | `"{abbr} EN 199x[-part]: Eurocode N: {discipline} - {clause description}".TrimStart(' ')` — `abbr` empty for `RecommendedValues`, `Part` rendered with `Part`/`_` → `-` |
| [07] | `new MissingNationalAnnexException(NationalAnnex)` | fault construction | the unmapped-annex fault type (thrown internally; constructible at the boundary for re-raise) |

## [04]-[IMPLEMENTATION_LAW]

[STANDARD_TOPOLOGY]:
- contract: `IStandard: ITaxonomySerializable` — `StandardBody Body { get; }`, `string Title { get; }`; `StandardBody` is the 11-member publishing-body enum (`EN` = index 7), the broad axis above the Eurocode-specific `En199x` family
- national-annex key: `NationalAnnex` is a closed 37-member enum, `RecommendedValues` at 0 then 36 nations; it is the single dispatch axis for the whole Eurocode regime — `Cases` tables, factories, and the `En199x.Title` abbreviation all key on it
- part enums: each Eurocode owns its `En199xPart` clause-part enum (EN 1990 has only `None`; EN 1993 is the deepest at 20 parts); the concrete `En199x` carries the matching `Part` except `En1990`
- concrete realization: `En1990`…`En1999` are `IStandard` impls, each `Body => StandardBody.EN`, with settable `NationalAnnex`/`Part` and a derived `Title`; they are the standard-identity authoring surface

[TITLE_KERNEL]:
- `IStandard.Title` is composed lazily, never stored: `En199x.GetTitle()` reads `NationalAnnexUtility.GetAbbreviation(NationalAnnex)`, the EN code number, the discipline string, and (for `En1991`+) `En199xUtility.GetPartDescription(Part)`, then `TrimStart(' ')`
- `NationalAnnexUtility.GetAbbreviation(NationalAnnex)` (internal) maps the 37 annexes to the national standards-body abbreviation — `RecommendedValues` → `""`, `Austria` → `ONORM`, `Belgium` → `NBN`, `France` → `NF`, `Germany` → `DIN`, `Italy` → `UNI`, `Netherlands` → `NEN`, `UnitedKingdom` → `BS`, plus non-Latin scripts (`Belarus` → `Ткп`, `Bulgaria` → `БДС`, `Greece` → `ΕΛΟΤ`) — and its default arm `_ => throw new MissingNationalAnnexException(na)` is the SINGLE throw site, reached transitively through `IStandard.Title`; the table covers all 37 declared members, so the guard fires only for an out-of-domain `(NationalAnnex)` cast, never a declared annex
- `En199xUtility.GetPartDescription(En199xPart)` (internal) maps each part to its full clause description (e.g. `En1991Part.Part1_3` → "Part 1-3: General actions - Snow loads"); an unmapped part throws a bare `ArgumentException("Unknown part")`
- both utility families are `internal static` — not consumer-callable; the OBSERVABLE surface is `IStandard.Title` (the composed string) and the `MissingNationalAnnexException` it can raise

[LOCAL_ADMISSION]:
- `NationalAnnex` is the canonical Eurocode dispatch key the Bim design layer carries; `RecommendedValues` is the EN standard fallback when no national annex applies (the default the `Countries` bridge resolves to for a non-CEN nation)
- a national deviation is selected by the `NationalAnnex` enum value, never a per-country code branch — the annex is a discriminant the `Cases` tables key on, not a switch in Bim
- `MissingNationalAnnexException` (declared in `VividOrange.IStandards`, thrown from the concrete `NationalAnnexUtility`) is caught at the Bim ingest boundary and lowered onto `Model/faults#FAULT_BAND` `BimFault.CapabilityMiss`/`Fin<T>` through `.ToError()`, never propagated into the fold
- the concrete `En1990`…`En1999` are MUTABLE settable-property carriers (the Materials-owned concrete package); treat them as the standard-identity authoring surface and project the resolved `(StandardBody, NationalAnnex, Title)` onto the immutable Bim records — never re-export the concrete class as the canonical graph shape

[STACKING]:
- with `VividOrange.Cases` (`.api/api-vividorange-cases`): `NationalAnnex` is the dispatch key for every `ENLoadCaseFactory`/`ENCombinationFactory` entry and every `ITableA1_1`/`ITableA1_2.GetProperties(NationalAnnex)` — a national deviation is a table row keyed by this enum; the `Cases` engine also throws `NotImplementedException` from its own EN `ψ`/`γ` table singletons for an annex its tables do not cover, so the Bim ingest catches BOTH that and `MissingNationalAnnexException` and lowers them onto `BimFault.CapabilityMiss`
- with `VividOrange.Countries` (`.api/api-vividorange-countries`): `Country` (249 ISO nations) is the broad national axis and `NationalAnnex` (37 annexes) is the narrow Eurocode-parameter axis; they meet at the project's nation, bridged BY NAME at the design layer (`Germany` ↔ `NationalAnnex.Germany`), defaulting to `RecommendedValues` for a nation with no annex — there is NO compiled `Country`→`NationalAnnex` member, and the `NationalAnnex`→standards-body abbreviation table is internal to `VividOrange.Standards`
- with `VividOrange.ISerialization`: `IStandard: ITaxonomySerializable` shares the one taxonomy-serialization marker with loads, cases, countries, and stages, so standard identity round-trips through the single VividOrange serializer that covers the whole structural taxonomy
- with `StructuralAnalysisFormat` (`.api/api-structuralanalysisformat`): a model's `StandardBody.EN` + selected `NationalAnnex` map onto the SAF `ExcelNationalCode` design-code axis (`EC_DIN_EN`/`EC_NF_EN`/`EC_UNI_EN`/…) at the XLSX boundary, and `ExcelStructuralLoadCombination.NationalStandard` carries it on the wire — the `NationalAnnex` abbreviation (`DIN`/`NF`/`UNI`) is the same national-body prefix the SAF `ExcelNationalCode` member encodes
- with `Thinktecture.Runtime.Extensions` (`.api/api-thinktecture-json`): when the Bim layer needs a canonical standards discriminant it owns a `[SmartEnum]`/`[ValueObject]` keyed by the standard identity; `NationalAnnex`/`StandardBody` are the boundary vocabulary mapped onto it at ingest, never re-exported as the canonical shape
- with `LanguageExt.Core`: the unmapped-annex path is a typed boundary fault — `MissingNationalAnnexException` (and the `Cases` `NotImplementedException`) is captured at ingest into `Fin<T>` via `.ToError()`, so the Eurocode dispatch never throws into the Bim fold

[RAIL_LAW]:
- Package: `VividOrange.IStandards` (contract) realized by `VividOrange.Standards` (concrete)
- Owns: the Eurocode standards-identity taxonomy — the `IStandard`/`StandardBody` contract, the `NationalAnnex` dispatch key, the `En199xPart` clause-part enums, the concrete `En1990`…`En1999` realization, and the `MissingNationalAnnexException` fault type
- Accept: national-annex selection as a `NationalAnnex` enum value; `RecommendedValues` as the EN fallback; standard identity read through `IStandard.Body`/`Title`; serialization through `ITaxonomySerializable`; the unmapped-annex fault caught and lowered onto `BimFault`
- Reject: a per-country code branch in place of the `NationalAnnex` discriminant, a fabricated compiled `Country`→`NationalAnnex` map (the bridge is by-name at the design layer), re-exporting the concrete `En199x` class as the canonical Bim graph shape, propagating `MissingNationalAnnexException` into the fold instead of lowering it onto `Fin<T>`
