# [RASM_BIM_API_VIVIDORANGE_ISTANDARDS]

`VividOrange.IStandards` mints the Eurocode standards-identity taxonomy: the `IStandard`/`StandardBody` contract naming which body publishes a code, the closed `NationalAnnex` key parameterizing every Eurocode `ψ`/`γ` table lookup, the per-Eurocode `En199xPart` clause-part enums, and the `MissingNationalAnnexException` fault for an unmapped annex. Concrete `VividOrange.Standards` realizes it as the mutable `En1990`…`En1999` classes fixing `Body => StandardBody.EN`, and Bim reads the taxonomy on the standards rail, lowering the unmapped-annex fault onto `BimFault` at ingest.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `VividOrange.IStandards`
- package: `VividOrange.IStandards` (contract), `VividOrange.Standards` (concrete realization + internal title kernel); MIT, Vivid Orange
- assembly: `VividOrange.IStandards`, `VividOrange.Standards`
- namespace: `VividOrange.Standards` (`IStandard`, `StandardBody`), `VividOrange.Standards.Eurocode` (`NationalAnnex`, `En1990Part`…`En1999Part`, `MissingNationalAnnexException`, concrete `En1990`…`En1999`)
- asset: multi-target `net48`/`net6.0`/`net7.0`/`net8.0`/`netstandard2.0`; the `net10.0` consumer binds `lib/net8.0`
- asset: pure-managed AnyCPU IL-only assemblies; no native binaries; ALC-safe inside the in-Rhino plugin assembly
- dependency: `VividOrange.IStandards` → `VividOrange.ISerialization` (`ITaxonomySerializable`); `VividOrange.Standards` → `VividOrange.IStandards`
- consumer: Bim binds `VividOrange.IStandards` transitively via `VividOrange.Cases → VividOrange.ICases → VividOrange.IStandards` (the `NationalAnnex` key + `MissingNationalAnnexException`); the concrete `VividOrange.Standards` (`En1990`…`En1999`) is referenced by `Rasm.Materials`
- rail: standards

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: standard-identity contract family (`VividOrange.IStandards`)
- note: `NationalAnnex`/`MissingNationalAnnexException` are `.Eurocode`-namespaced; each `StandardBody` member carries a `[Description]`.

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [CAPABILITY]                                                       |
| :-----: | :------------------------------ | :------------ | :----------------------------------------------------------------- |
|  [01]   | `IStandard`                     | interface     | `: ITaxonomySerializable`; `StandardBody Body`, `string Title`     |
|  [02]   | `StandardBody`                  | enum          | `AASHTO`/`ACI`/`AISC`/`ANSI`/`AS`/`BS`/`CSA`/`EN`/`HK`/`IS`/`SANS` |
|  [03]   | `NationalAnnex`                 | enum          | `RecommendedValues` (=0) then 36 nations `Austria`…`UnitedKingdom` |
|  [04]   | `MissingNationalAnnexException` | exception     | `: Exception` → `"National Annex of {na} not implemented"`         |

[PUBLIC_TYPE_SCOPE]: Eurocode clause-part enums (`VividOrange.IStandards`)

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY]    | [CAPABILITY]                                                                         |
| :-----: | :----------- | :--------------- | :----------------------------------------------------------------------------------- |
|  [01]   | `En1990Part` | EC0 basis        | `None` only — EN 1990 carries no clause-part split                                   |
|  [02]   | `En1991Part` | EC1 actions      | `Part1_1`…`Part1_7`/`Part2`/`Part3`/`Part4` (dead/fire/snow/wind/thermal/…/silos)    |
|  [03]   | `En1992Part` | EC2 concrete     | `Part1_1`/`Part1_2`/`Part2`/`Part3` (general/fire/bridges/liquid-retaining)          |
|  [04]   | `En1993Part` | EC3 steel        | `Part1_1`…`Part1_12`/`Part2`/`Part3_1`/`Part3_2`/`Part4_1`…`Part4_3`/`Part5`/`Part6` |
|  [05]   | `En1994Part` | EC4 composite    | `Part1_1`/`Part1_2`/`Part2` (general/fire/bridges)                                   |
|  [06]   | `En1995Part` | EC5 timber       | `Part1_1`/`Part1_2`/`Part2` (general/fire/bridges)                                   |
|  [07]   | `En1996Part` | EC6 masonry      | `Part1_1`/`Part1_2`/`Part2`/`Part3`                                                  |
|  [08]   | `En1997Part` | EC7 geotechnical | `Part1`/`Part2` (general/ground-investigations)                                      |
|  [09]   | `En1998Part` | EC8 seismic      | `Part1`…`Part6` (general/bridges/retrofit/silos/geotech/towers)                      |
|  [10]   | `En1999Part` | EC9 aluminium    | `Part1_1`…`Part1_5` (general/fire/fatigue/cold-formed/shell)                         |

[PUBLIC_TYPE_SCOPE]: concrete Eurocode-standard realization (`VividOrange.Standards`)
- note: each is `: IStandard` with a settable `NationalAnnex` and (except `En1990`) an `En199xPart Part`; the cell carries the composed `Title` discipline string.

| [INDEX] | [SYMBOL] | [TYPE_FAMILY]    | [CAPABILITY]                                                    |
| :-----: | :------- | :--------------- | :-------------------------------------------------------------- |
|  [01]   | `En1990` | EC0 basis        | `"… EN 1990: Eurocode - Basis of Structural Design"`            |
|  [02]   | `En1991` | EC1 actions      | `"Eurocode 1: Actions on Structures"`                           |
|  [03]   | `En1992` | EC2 concrete     | `"Eurocode 2: Design of Concrete Structures"`                   |
|  [04]   | `En1993` | EC3 steel        | `"Eurocode 3: Design of Steel Structures"`                      |
|  [05]   | `En1994` | EC4 composite    | `"Eurocode 4: Design of Composite Steel & Concrete Structures"` |
|  [06]   | `En1995` | EC5 timber       | `"Eurocode 5: Design of Timber Structures"`                     |
|  [07]   | `En1996` | EC6 masonry      | `"Eurocode 6: Design of Masonry Structures"`                    |
|  [08]   | `En1997` | EC7 geotechnical | `"Eurocode 7: Geotechnical Design"`                             |
|  [09]   | `En1998` | EC8 seismic      | `"Eurocode 8: Design of Structures for Earthquake Resistance"`  |
|  [10]   | `En1999` | EC9 aluminium    | `"Eurocode 9: Design of Aluminium Structures"`                  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: standard-identity construction and read

| [INDEX] | [SURFACE]                                                | [SHAPE]  | [CAPABILITY]                                      |
| :-----: | :------------------------------------------------------- | :------- | :------------------------------------------------ |
|  [01]   | `new En1990(NationalAnnex)` / `new En1990()`             | ctor     | EC0 basis-of-design identity for a national annex |
|  [02]   | `new En199x(En199xPart, NationalAnnex)` / `new En199x()` | ctor     | a Eurocode identity for a clause part + annex     |
|  [03]   | `En199x.NationalAnnex`                                   | property | settable national-annex value                     |
|  [04]   | `En199x.Part`                                            | property | settable `En199xPart`; absent on `En1990`         |
|  [05]   | `IStandard.Body`                                         | property | publishing `StandardBody`, always `EN` here       |
|  [06]   | `IStandard.Title`                                        | property | composed identity string (format below)           |
|  [07]   | `new MissingNationalAnnexException(NationalAnnex)`       | ctor     | unmapped-annex fault, constructible for re-raise  |

- [06]-[TITLE]: `Title` = `"{abbr} EN 199x[-part]: Eurocode N: {discipline} - {clause}".TrimStart(' ')`; `abbr` empties for `RecommendedValues`, and `Part` renders `Part`/`_`→`-`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- contract: `IStandard: ITaxonomySerializable` — `StandardBody Body { get; }`, `string Title { get; }`; `StandardBody` is the publishing-body axis (`EN` at index 7) above the Eurocode-specific `En199x` family
- national-annex key: `NationalAnnex` is the closed 37-member enum, `RecommendedValues` at 0 then 36 nations; it is the single dispatch axis for the whole Eurocode regime — `Cases` tables and factories and the `En199x.Title` abbreviation all key on it
- part enums: each Eurocode owns its `En199xPart` clause-part enum (`En1990` carries only `None`, `En1993` the deepest at 20 parts); the concrete `En199x` carries the matching `Part` except `En1990`
- concrete realization: `En1990`…`En1999` are mutable `IStandard` impls, each `Body => StandardBody.EN` with settable `NationalAnnex`/`Part`; they are the standard-identity authoring surface
- title composition: `IStandard.Title` composes lazily via `En199x.GetTitle()`, never stored — it reads `NationalAnnexUtility.GetAbbreviation(NationalAnnex)`, the EN code number, the discipline, and (for `En1991`+) `En199xUtility.GetPartDescription(Part)`, then `TrimStart(' ')`
- annex abbreviation: `NationalAnnexUtility.GetAbbreviation` (internal) maps all 37 annexes to the national standards-body abbreviation (`RecommendedValues` → `""`, `Germany` → `DIN`, `France` → `NF`, `Italy` → `UNI`, `UnitedKingdom` → `BS`, non-Latin `Greece` → `ΕΛΟΤ`); its `_ => throw new MissingNationalAnnexException(na)` default is the single throw site, reached through `IStandard.Title`, firing only for an out-of-domain cast since the map covers every declared member
- part description: `En199xUtility.GetPartDescription` (internal) maps each part to its full clause description, throwing `ArgumentException("Unknown part")` for an unmapped part; both utility families are `internal static`, so the observable surface is `IStandard.Title` and the `MissingNationalAnnexException` it can raise

[STACKING]:
- with `VividOrange.Cases` (`.api/api-vividorange-cases`): `NationalAnnex` keys every `ENLoadCaseFactory`/`ENCombinationFactory` entry and every `ITableA1_1`/`ITableA1_2.GetProperties(NationalAnnex)`, so a national deviation is a table row; the `Cases` EN `ψ`/`γ` singletons throw `NotImplementedException` for an uncovered annex, so Bim ingest catches both that and `MissingNationalAnnexException` and lowers onto `BimFault.CapabilityMiss`
- with `VividOrange.Countries` (`.api/api-vividorange-countries`): `Country` (249 ISO nations) is the broad national axis and `NationalAnnex` the narrow Eurocode-parameter axis; they meet at the project nation, bridged by name at the design layer (`Germany` ↔ `NationalAnnex.Germany`), defaulting to `RecommendedValues` — no compiled `Country`→`NationalAnnex` member exists, and the abbreviation table is internal to `VividOrange.Standards`
- with `VividOrange.ISerialization`: `IStandard: ITaxonomySerializable` rides the shared taxonomy-serialization marker, so standard identity round-trips through the one VividOrange serializer
- with `StructuralAnalysisFormat` (`.api/api-structuralanalysisformat`): a model's `StandardBody.EN` + `NationalAnnex` map onto the SAF `ExcelNationalCode` design-code axis (`EC_DIN_EN`/`EC_NF_EN`/`EC_UNI_EN`/…) at the XLSX boundary, and `ExcelStructuralLoadCombination.NationalStandard` carries it on the wire — the `NationalAnnex` abbreviation (`DIN`/`NF`/`UNI`) is the SAF national-body prefix
- with `Thinktecture.Runtime.Extensions` (`libs/csharp/.api/api-thinktecture-json.md`): the Bim layer owns a `[SmartEnum]`/`[ValueObject]` keyed by standard identity, and `NationalAnnex`/`StandardBody` are the boundary vocabulary mapped onto it at ingest, never re-exported as the canonical shape
- with `LanguageExt.Core`: `MissingNationalAnnexException` (and the `Cases` `NotImplementedException`) is captured at ingest into `Fin<T>` via `.ToError()`, so the Eurocode dispatch never throws into the Bim fold

[LOCAL_ADMISSION]:
- `NationalAnnex` is the canonical Eurocode dispatch key the Bim design layer carries; `RecommendedValues` is the EN fallback when no national annex applies, the `Countries` bridge default for a non-CEN nation
- a national deviation is a `NationalAnnex` value the `Cases` tables key on, selected as a discriminant rather than a Bim branch
- `MissingNationalAnnexException` is caught at the Bim ingest boundary and lowered onto `Model/faults` `BimFault.CapabilityMiss`/`Fin<T>` via `.ToError()`
- concrete `En1990`…`En1999` are the Materials-owned authoring surface; project the resolved `(StandardBody, NationalAnnex, Title)` onto the immutable Bim records rather than re-exporting the concrete class as the canonical graph shape

[RAIL_LAW]:
- Package: `VividOrange.IStandards` (contract) realized by `VividOrange.Standards` (concrete)
- Owns: the Eurocode standards-identity taxonomy — `IStandard`/`StandardBody`, the `NationalAnnex` dispatch key, the `En199xPart` clause-part enums, the concrete `En1990`…`En1999`, and `MissingNationalAnnexException`
- Accept: national-annex selection as a `NationalAnnex` value; `RecommendedValues` as the EN fallback; identity read through `IStandard.Body`/`Title`; serialization through `ITaxonomySerializable`; the unmapped-annex fault lowered onto `BimFault`
- Reject: a per-country code branch replacing the `NationalAnnex` discriminant, a fabricated compiled `Country`→`NationalAnnex` map, re-exporting the concrete `En199x` as the canonical Bim graph shape, propagating `MissingNationalAnnexException` into the fold
