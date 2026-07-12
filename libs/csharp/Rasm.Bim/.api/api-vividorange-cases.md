# [RASM_BIM_API_VIVIDORANGE_CASES]

`VividOrange.Cases` (concrete) over `VividOrange.ICases` (contract) is the Eurocode EN 1990
load-case-and-combination engine: the taxonomy that turns a bag of typed `VividOrange.Loads`
`ILoad` actions into design load cases, groups them by `ActionClass`, and folds them into the
full ULS/SLS combination set with the correct partial-safety (`γ`) and combination (`ψ`) factors
for a chosen `NationalAnnex`. It is the semantic content behind the `Model/structural#ANALYSIS_MODEL`
`StructuralLoadKind.LoadCase`/`LoadCombination` discriminants — the `AnalysisModel` graph owns
load-group topology by GlobalId, while this package owns the code-checked combination algebra the
Compute solver evaluates. The hierarchy is interface-deep: `ICase` (a named entry) → `ILoadCase`
(`Loads`, `ActionClass`, `Nickname`) → `IPermanentCase`/`IVariableCase` (the `ψ0`/`ψ1`/`ψ2`
factors), and `ILoadCombination` → `IUltimateLimitState`/`IServiceabilityLimitState` → the concrete
combination contracts (`IMemberDesignCombination`, `IGeotechnicalMemberDesignCombination`,
`IAccidentalCombination`, `ISeismicCombination`, `ICharacteristicCombination`, `IFrequentCombination`,
`IQuasiPermanentCombination`, `IEquilibriumCombination`). The two static factories are the real
entrypoints: `ENLoadCaseFactory` mints `VariableCase` rows pre-loaded with EN 1990 Annex A1.1
`ψ` factors for imposed/snow/thermal/wind actions, and `ENCombinationFactory` emits the entire
combination set per EN / 6.10a-b. The factors come from data-table singletons
(`ITableA1_1`/`ITableA1_2` → `TableA1_1Properties`/`TableA1_2Properties`) keyed by `NationalAnnex`,
so a national deviation is a table row, never a code branch. The package STACKS on
`VividOrange.Loads` (`.api/api-vividorange-loads`) for the `ILoad` payload it factors, on `UnitsNet`
`Ratio` for every `ψ`/`γ` factor, and on `VividOrange.IStandards`' `NationalAnnex` (`.api/api-vividorange-istandards`); its combination outputs map onto
the SAF `ExcelStructuralLoadCombination` wire whose `ExcelLoadCaseCombinationStandard` enum
(`EnUlsSetB`/`EnUlsSetC`/`EnAccidental*`/`EnSeismic`/`EnSls*`) is the exact image of these factories.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `VividOrange.Cases`
- package: `VividOrange.Cases` (concrete cases, combinations, factories, EN tables), `VividOrange.ICases` (contracts, enums, property structs)
- license: MIT
- assembly: `VividOrange.Cases`, `VividOrange.ICases`
- namespace: `VividOrange.Loads` (`DesignSituation`, `ICase`, `IDesignSituation`, `ActionClass`, `DesignSituationClass`), `VividOrange.Loads.Cases` (load cases + `ImposedLoadCategory`), `VividOrange.Loads.Cases.EN` (Annex A1.1 imposed/snow/thermal/wind tables), `VividOrange.Loads.Combinations` (ULS/SLS combinations + `Utility`), `VividOrange.Loads.Combinations.EN` (Annex A1.2 partial-factor table)
- asset: multi-target `net48`/`net6.0`/`net7.0`/`net8.0`/`netstandard2.0`; the `net10.0` consumer binds `lib/net8.0`
- asset: pure-managed AnyCPU IL-only assemblies; no native binaries; ALC-safe inside the in-Rhino plugin assembly
- dependency: `VividOrange.ICases` → `VividOrange.ILoads` (the `ILoad` payload) + `VividOrange.IStandards` (`NationalAnnex`) + `VividOrange.ISerialization` (`ITaxonomySerializable`) + `UnitsNet` (`Ratio`); `VividOrange.Cases` → `VividOrange.ICases` + `VividOrange.IStandards`
- rail: load-case

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: case contract family (`VividOrange.ICases`)
- rail: load-case

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]            | [RAIL]                                                                                             |
| :-----: | :--------------------- | :----------------------- | :------------------------------------------------------------------------------------------------- |
|  [01]   | `ICase`                | named case base          | `: ITaxonomySerializable` + `string Name`                                                          |
|  [02]   | `ILoadCase`            | applied load case        | `: ICase` + `Nickname`, `IList<ILoad> Loads`, `IsHorizontal`, `ActionClass`                        |
|  [03]   | `IPermanentCase`       | permanent (`G`) case     | `: ILoadCase` (marker — `γG`-governed)                                                             |
|  [04]   | `IVariableCase`        | variable (`Q`) case      | `: ILoadCase` + `Ratio CombinationFactor`/`FrequentFactor`/`QuasiPermanentFactor` (`ψ0`/`ψ1`/`ψ2`) |
|  [05]   | `IDesignSituation`     | partial-factor policy    | `DesignSituationClass Class` + `γ`-factor reads (`double`/`double?`) + `ReductionFactor`           |
|  [06]   | `ActionClass`          | action-nature enum       | `Permanent` / `Variable` / `Accidental`                                                            |
|  [07]   | `DesignSituationClass` | `[Flags]` situation enum | `Persistent`=1 / `Transient`=2 / `PersistentAndTransient`=3 / `Accidental`=4 / `Seismic`=8         |
|  [08]   | `ImposedLoadCategory`  | EN imposed-load category | `CategoryA`…`CategoryH`                                                                            |

[PUBLIC_TYPE_SCOPE]: combination contract family (`VividOrange.ICases`)
- rail: load-case

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY]       | [RAIL]                                                                                                                |
| :-----: | :------------------------------------- | :------------------ | :-------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `ILoadCombination`                     | combination base    | `: ICase` + `Definition`, `PermanentCases`, `PermanentCaseIsFavourable`, `LeadingVariableCases`, `GetFactoredLoads()` |
|  [02]   | `IUltimateLimitState`                  | ULS base            | `: ILoadCombination` + `IDesignSituation DesignSituation`                                                             |
|  [03]   | `IServiceabilityLimitState`            | SLS base            | `: ILoadCombination` + `AccompanyingVariableCases`                                                                    |
|  [04]   | `IMemberDesignCombination`             | STR/GEO Set B       | `: IUltimateLimitState` + `Main`/`OtherAccompanyingVariableCases`                                                     |
|  [05]   | `IGeotechnicalMemberDesignCombination` | STR/GEO Set C       | `: IUltimateLimitState` + `AccompanyingVariableCases`                                                                 |
|  [06]   | `IAccidentalCombination`               | accidental ULS      | `: IUltimateLimitState` + `UseFrequentCombinationFactorForMainAccompanying` + accompanying cases                      |
|  [07]   | `ISeismicCombination`                  | seismic ULS         | `: IUltimateLimitState` (EN)                                                                                          |
|  [08]   | `IEquilibriumCombination`              | EQU Set A           | `: IUltimateLimitState` (static equilibrium)                                                                          |
|  [09]   | `ICharacteristicCombination`           | SLS characteristic  | `: IServiceabilityLimitState` (EN)                                                                                    |
|  [10]   | `IFrequentCombination`                 | SLS frequent        | `: IServiceabilityLimitState` (EN)                                                                                    |
|  [11]   | `IQuasiPermanentCombination`           | SLS quasi-permanent | `: IServiceabilityLimitState` (EN)                                                                                    |

[PUBLIC_TYPE_SCOPE]: EN 1990 Annex A1 code-table family (`VividOrange.ICases`)
- rail: load-case

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]            | [RAIL]                                                     |
| :-----: | :----------------------- | :----------------------- | :--------------------------------------------------------- |
|  [01]   | `EN.ITableA1_1`          | `ψ`-factor table         | `TableA1_1Properties GetProperties(NationalAnnex)`         |
|  [02]   | `EN.ITableA1_1Imposed`   | imposed `ψ`-factor table | `GetProperties(ImposedLoadCategory, NationalAnnex)`        |
|  [03]   | `EN.ITableA1_1Snow`      | snow `ψ`-factor table    | altitude/region-keyed `ψ` lookup                           |
|  [04]   | `EN.TableA1_1Properties` | `ψ`-factor struct        | `Ratio Phi_0`/`Phi_1`/`Phi_2` (`ψ0`/`ψ1`/`ψ2`)             |
|  [05]   | `EN.ITableA1_2`          | `γ`-factor table         | `TableA1_2Properties GetProperties(NationalAnnex)`         |
|  [06]   | `EN.TableA1_2Properties` | `γ`-factor struct        | `Ratio Gamma_Gsup`/`Gamma_Ginf`/`Gamma_Q1`/`Gamma_Qi`/`Xi` |

[PUBLIC_TYPE_SCOPE]: concrete case + design-situation family (`VividOrange.Cases`)
- rail: load-case

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]              | [RAIL]                                                               |
| :-----: | :---------------- | :------------------------- | :------------------------------------------------------------------- |
|  [01]   | `PermanentCase`   | permanent case impl        | `ActionClass.Permanent`; defaults `Nickname="G"`, `Name="Dead Load"` |
|  [02]   | `VariableCase`    | variable case impl         | `ActionClass.Variable`; `ψ0`/`ψ1`/`ψ2` reads; `IsFavourable`         |
|  [03]   | `DesignSituation` | partial-factor policy impl | `γ`-factor settable rows (defaults) incl. `PrestressPartialFactor`   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: EN load-case construction (`ENLoadCaseFactory`, static)
- rail: load-case

| [INDEX] | [SURFACE]                                                         | [ENTRY_FAMILY]  | [RAIL]                                                                                                                  |
| :-----: | :---------------------------------------------------------------- | :-------------- | :---------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `CreateImposed(IList<ILoad>, ImposedLoadCategory, NationalAnnex)` | imposed case    | `VariableCase` carrying its `ILoad` actions, pre-loaded with category `ψ` factors                                       |
|  [02]   | _(no loadless `CreateImposed`)_                                   | imposed case    | Imposed has ONLY the loaded overload — unlike Snow/Thermal/Wind there is no `(ImposedLoadCategory, NationalAnnex)` form |
|  [03]   | `CreateSnow(NationalAnnex, bool altitudeAbove1000m)`              | snow case       | `VariableCase` with altitude-keyed snow `ψ` factors                                                                     |
|  [04]   | `CreateThermal(NationalAnnex)`                                    | thermal case    | `VariableCase` with thermal `ψ` factors                                                                                 |
|  [05]   | `CreateWind(NationalAnnex)`                                       | wind case       | `VariableCase` with wind `ψ` factors                                                                                    |
|  [06]   | `Create{Snow,Thermal,Wind}(IList<ILoad>, NationalAnnex, …)`       | loaded variants | each action factory has an `IList<ILoad>` overload                                                                      |

[ENTRYPOINT_SCOPE]: EN combination synthesis (`ENCombinationFactory`, static)
- rail: load-case

| [INDEX] | [SURFACE]                                                                                                      | [ENTRY_FAMILY]      | [RAIL]                                                |
| :-----: | :------------------------------------------------------------------------------------------------------------- | :------------------ | :---------------------------------------------------- |
|  [01]   | `CreateCharacteristic(IList<ILoadCase>, prefix="LC", firstCaseId=1)`                                           | SLS characteristic  | `IList<ICharacteristicCombination>` (EN)              |
|  [02]   | `CreateFrequent(IList<ILoadCase>, …)`                                                                          | SLS frequent        | `IList<IFrequentCombination>` (EN)                    |
|  [03]   | `CreateQuasiPermanent(IList<ILoadCase>, …)`                                                                    | SLS quasi-permanent | `IList<IQuasiPermanentCombination>` (EN)              |
|  [04]   | `CreateEquSetA(IList<ILoadCase>[, NationalAnnex, …])`                                                          | EQU Set A           | `IList<IEquilibriumCombination>` (static equilibrium) |
|  [05]   | `CreateStrGeoSetB(IList<ILoadCase>[, NationalAnnex, bool use6_10aAnd6_10b, …])`                                | STR/GEO Set B       | `IList<IMemberDesignCombination>` (EN or 6.10a/b)     |
|  [06]   | `CreateStrGeoSetC(IList<ILoadCase>[, NationalAnnex, …])`                                                       | STR/GEO Set C       | `IList<IGeotechnicalMemberDesignCombination>`         |
|  [07]   | `CreateAccidental(IVariableCase, double partialFactor, IList<ILoadCase>, NationalAnnex, bool useFrequent…, …)` | accidental ULS      | `IList<IAccidentalCombination>` (EN)                  |
|  [08]   | `CreateSeismic(IList<IVariableCase>, Ratio partialFactor, IList<ILoadCase>, NationalAnnex, …)`                 | seismic ULS         | `IList<ISeismicCombination>` (EN)                     |

[ENTRYPOINT_SCOPE]: load-factoring algebra (`Combinations.Utility`, static; EN-table reads)
- rail: load-case

| [INDEX] | [SURFACE]                                                                                   | [ENTRY_FAMILY]            | [RAIL]                                                         |
| :-----: | :------------------------------------------------------------------------------------------ | :------------------------ | :------------------------------------------------------------- |
|  [01]   | `FactorLoads<T>(Ratio partialFactor, IList<T>) where T: ILoadCase`                          | uniform factor            | every case's `ILoad` set scaled by one `γ`                     |
|  [02]   | `FactorLoads<T>(IDesignSituation, IList<T>, IList<bool> isFavourable) where T: ILoadCase`   | situation factor          | per-case favourable/unfavourable `γ` from the design situation |
|  [03]   | `GetLoads<T>(IList<T>) where T: ILoadCase`                                                  | load gather               | flatten the case set's `ILoad` rows                            |
|  [04]   | `FactorAccompanyingVariableLoads<T>(Ratio, IList<T>, Func<T,Ratio>) where T: IVariableCase` | `ψ`-weighted accompanying | accompanying-action `ψ·γ` factoring by selector                |
|  [05]   | `SelectAccompanyingVariableLoads<T>(IList<T>, Func<T,Ratio>) where T: IVariableCase`        | `ψ` selection             | accompanying-action `ψ` selection by selector                  |
|  [06]   | `ILoadCombination.GetFactoredLoads()`                                                       | factored output           | the fully factored `IList<ILoad>` the solver evaluates         |
|  [07]   | `EN.ITableA1_1/.ITableA1_2.GetProperties(NationalAnnex)`                                    | factor lookup             | the `ψ`/`γ` `Ratio` factor set for a national annex            |

## [04]-[IMPLEMENTATION_LAW]

[CASE_TOPOLOGY]:
- case hierarchy: `ICase` (`Name`) → `ILoadCase` (`Nickname`, `IList<ILoad> Loads`, `IsHorizontal`, `ActionClass`) → `IPermanentCase` (marker) / `IVariableCase` (`CombinationFactor`/`FrequentFactor`/`QuasiPermanentFactor` = `ψ0`/`ψ1`/`ψ2` as `Ratio`)
- combination hierarchy: `ILoadCombination` (`Definition`, `PermanentCases`, `PermanentCaseIsFavourable`, `LeadingVariableCases`, `GetFactoredLoads()`) → `IUltimateLimitState` (+ `DesignSituation`) / `IServiceabilityLimitState` (+ accompanying cases) → the EN-clause leaf contracts
- design situation: `IDesignSituation` carries the EN partial factors — `UnfavourablePermanentActionsPartialFactor`/`FavourablePermanentActionsPartialFactor` (`γG,sup`/`γG,inf`), `LeadingActionPartialFactor`/`MainAccompanyingVariableActionsPartialFactor` (`γQ`, nullable for the leading-action sweep), `OtherAccompanyingVariableActionsPartialFactor`, `ReductionFactor` (`ξ`); the concrete `DesignSituation` adds `PrestressPartialFactor`
- code tables: `TableA1_1Properties` = `ψ0`/`ψ1`/`ψ2` (`Ratio`), `TableA1_2Properties` = `γG,sup`/`γG,inf`/`γQ,1`/`γQ,i`/`ξ` (`Ratio`); both constructed from `double` via the `RatioUnit.DecimalFraction` ctor; the EN table singletons (`ENTableA1_1Imposed`/`Snow`/`Thermal`/`Wind`, `ENTableA1_2A`/`B`/`C`) dispatch on `NationalAnnex`
- factories: `ENLoadCaseFactory` (action → `VariableCase` with table `ψ`), `ENCombinationFactory` (case set → the EN /6.10a-b/// combination set)

[LOCAL_ADMISSION]:
- the `Model/structural#ANALYSIS_MODEL` `StructuralLoadKind` `[SmartEnum<string>]` (`LoadCase`/`LoadCombination`) is the canonical in-graph discriminant; the VividOrange `ILoadCase`/`ILoadCombination` are the Eurocode semantic engine the design layer composes to PRODUCE those rows, never re-exported as the canonical graph shape
- every `ψ`/`γ` factor is a `UnitsNet` `Ratio`, never a bare `double` factor; the `IDesignSituation` `double` partial-factor reads are the boundary the design layer wraps in `Ratio.FromDecimalFractions` before factoring
- a national deviation is selected by passing the right `NationalAnnex` to `GetProperties`/the factory — it is a data-table row, never a per-country code branch; `RecommendedValues` is the EN standard fallback when no national annex applies
- the concrete `PermanentCase`/`VariableCase`/`DesignSituation` are MUTABLE settable-property carriers; treat them as the Eurocode authoring surface and project the produced combination set onto the immutable Bim records

[STACKING]:
- with `VividOrange.Loads` (`.api/api-vividorange-loads`): `ILoadCase.Loads` is `IList<ILoad>` and `ILoadCombination.GetFactoredLoads()` returns `IList<ILoad>`; the case/combination engine is a fold OVER the load value taxonomy — `Combinations.Utility.FactorLoads` applies `ILoad.Factor(Ratio)` across a case set, so the factored output is still typed `UnitsNet`-backed loads
- with `UnitsNet` (`.api/api-unitsnet`): every combination/partial factor is a `Ratio`, and `ILoad.Factor(γ)` multiplies by `γ.DecimalFractions`; the `ψ·γ` accompanying-action weighting is `Ratio` arithmetic, never raw-`double` factoring
- with `VividOrange.IStandards` (`.api/api-vividorange-istandards`) + `VividOrange.Countries` (`.api/api-vividorange-countries`): `NationalAnnex` (`VividOrange.Standards.Eurocode.NationalAnnex`, a 37-member enum incl. `RecommendedValues`) is the dispatch key for every factory and table; the project's `ICountry` selects which `NationalAnnex` parameterizes the combination set — Countries names the nation, the Standards `NationalAnnex` names its Eurocode parameter set, and the design layer bridges them by name (there is no compiled `Country`→`NationalAnnex` map)
- with `VividOrange.ISerialization`: `ICase`/`ILoadCombination: ITaxonomySerializable` share the one taxonomy-serialization marker with loads and countries, so the whole structural taxonomy round-trips through a single serializer
- with `StructuralAnalysisFormat` (`.api/api-structuralanalysisformat`): the combination outputs map onto `ExcelStructuralLoadCase` (`ExcelActionType`=`ActionClass`, `ExcelLoadCaseType`) and `ExcelStructuralLoadCombination` (`ExcelLoadCaseCombinationCategory`=ULS/SLS, `ExcelLoadCaseCombinationStandard.EnUlsSetB`/`EnUlsSetC`/`EnAccidental*`/`EnSeismic`/`EnSlsCharacteristic`/`Frequent`/`QuasiPermanent` = the exact image of `CreateStrGeoSetB`/`SetC`/`CreateAccidental`/`CreateSeismic`/`CreateCharacteristic`/`CreateFrequent`/`CreateQuasiPermanent`); the `LoadFactors`/`LoadMultipliers`/`LoadCases` arrays carry the factored result to the SAF wire
- with `LanguageExt.Core`: an uncovered `NationalAnnex` faults two ways at the package boundary — the EN `ψ`/`γ` table singletons throw `NotImplementedException` (`"NA … not implemented for EN1990 Table A1.1/A1.2"`), and the standards title kernel (`NationalAnnexUtility`, via `IStandard.Title`) throws `VividOrange.Standards.Eurocode.MissingNationalAnnexException` (`.api/api-vividorange-istandards`) for an annex outside the mapped set (an unrecognised case type throws a bare `Exception`); the Bim ingest catches all three and lowers onto `Model/faults#FAULT_BAND` `BimFault.CapabilityMiss`/`Fin<T>` through `.ToError()`, never propagating the exception into the fold

[RAIL_LAW]:
- Package: `VividOrange.Cases` over `VividOrange.ICases`
- Owns: the Eurocode EN 1990 load-case/combination taxonomy, the `ψ`/`γ` factor tables, and the ULS/SLS combination synthesis
- Accept: load cases built from typed `ILoad` actions; combinations synthesized through `ENCombinationFactory`; national deviations selected by `NationalAnnex` table row; factors carried as `Ratio`
- Reject: per-country combination code branches, bare-`double` partial factors, a hand-rolled EN combination sweep, re-exporting `ILoadCombination` as the canonical Bim graph shape (the owner is `Model/structural` `StructuralLoadKind`), a SAF combination round-trip that loses the `ExcelLoadCaseCombinationStandard` ↔ factory mapping
