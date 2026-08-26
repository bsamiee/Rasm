# [RASM_BIM_API_VIVIDORANGE_CASES]

`VividOrange.Cases` (concrete) over `VividOrange.ICases` (contract) owns the Eurocode EN 1990 load-case-and-combination algebra: it folds typed `VividOrange.Loads` `ILoad` actions into design cases, then synthesizes the ULS/SLS combination set with the partial-safety (`γ`) and combination (`ψ`) factors a `NationalAnnex` selects. `Model/eurocode#EUROCODE_ALGEBRA` composes it to PRODUCE a load-combination group's neutral rows — the generated `Definition` beside the `GetFactoredLoads()` design actions. It feeds the `load-case` path.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: case contract family (`VividOrange.ICases`)

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]         | [CAPABILITY]                                                                           |
| :-----: | :--------------------- | :-------------------- | :------------------------------------------------------------------------------------- |
|  [01]   | `ICase`                | named case base       | `: ITaxonomySerializable` + `string Name`                                              |
|  [02]   | `ILoadCase`            | applied load case     | `: ICase` + `Nickname`, `IList<ILoad> Loads`, `IsHorizontal`, `ActionClass`            |
|  [03]   | `IPermanentCase`       | permanent (`G`) case  | `: ILoadCase` marker (`γG`-governed)                                                   |
|  [04]   | `IVariableCase`        | variable (`Q`) case   | `: ILoadCase` + `ψ0`/`ψ1`/`ψ2` factors (`Ratio`)                                       |
|  [05]   | `IDesignSituation`     | partial-factor policy | `DesignSituationClass Class` + `γ` reads (`double?`) + `ReductionFactor`               |
|  [06]   | `ActionClass`          | action-nature enum    | `Permanent` / `Variable` / `Accidental`                                                |
|  [07]   | `DesignSituationClass` | `[Flags]` enum        | `Persistent`=1, `Transient`=2, `PersistentAndTransient`=3, `Accidental`=4, `Seismic`=8 |
|  [08]   | `ImposedLoadCategory`  | EN imposed category   | `CategoryA`…`CategoryH`                                                                |

[PUBLIC_TYPE_SCOPE]: combination contract family (`VividOrange.ICases`)
- note: `ILoadCombination: ICase` roots the family; ULS leaves derive `: IUltimateLimitState`, SLS leaves `: IServiceabilityLimitState`

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY]       | [CAPABILITY]                                                            |
| :-----: | :------------------------------------- | :------------------ | :---------------------------------------------------------------------- |
|  [01]   | `ILoadCombination`                     | combination base    | `: ICase` + `GetFactoredLoads()`, `Definition`, `LeadingVariableCases`  |
|  [02]   | `IUltimateLimitState`                  | ULS base            | + `IDesignSituation DesignSituation`                                    |
|  [03]   | `IServiceabilityLimitState`            | SLS base            | + `AccompanyingVariableCases`                                           |
|  [04]   | `IMemberDesignCombination`             | STR/GEO Set B       | + `Main`/`OtherAccompanyingVariableCases`                               |
|  [05]   | `IGeotechnicalMemberDesignCombination` | STR/GEO Set C       | + `AccompanyingVariableCases`                                           |
|  [06]   | `IAccidentalCombination`               | accidental ULS      | + `UseFrequentCombinationFactorForMainAccompanying`, accompanying cases |
|  [07]   | `ISeismicCombination`                  | seismic ULS         | EN seismic                                                              |
|  [08]   | `IEquilibriumCombination`              | EQU Set A           | static equilibrium                                                      |
|  [09]   | `ICharacteristicCombination`           | SLS characteristic  | EN characteristic                                                       |
|  [10]   | `IFrequentCombination`                 | SLS frequent        | EN frequent                                                             |
|  [11]   | `IQuasiPermanentCombination`           | SLS quasi-permanent | EN quasi-permanent                                                      |

[PUBLIC_TYPE_SCOPE]: EN 1990 Annex A1 code-table family (`VividOrange.ICases`)

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]            | [CAPABILITY]                                               |
| :-----: | :----------------------- | :----------------------- | :--------------------------------------------------------- |
|  [01]   | `EN.ITableA1_1`          | `ψ`-factor table         | `TableA1_1Properties GetProperties(NationalAnnex)`         |
|  [02]   | `EN.ITableA1_1Imposed`   | imposed `ψ`-factor table | `GetProperties(ImposedLoadCategory, NationalAnnex)`        |
|  [03]   | `EN.ITableA1_1Snow`      | snow `ψ`-factor table    | altitude/region-keyed `ψ` lookup                           |
|  [04]   | `EN.TableA1_1Properties` | `ψ`-factor struct        | `Ratio Phi_0`/`Phi_1`/`Phi_2` (`ψ0`/`ψ1`/`ψ2`)             |
|  [05]   | `EN.ITableA1_2`          | `γ`-factor table         | `TableA1_2Properties GetProperties(NationalAnnex)`         |
|  [06]   | `EN.TableA1_2Properties` | `γ`-factor struct        | `Ratio Gamma_Gsup`/`Gamma_Ginf`/`Gamma_Q1`/`Gamma_Qi`/`Xi` |

[PUBLIC_TYPE_SCOPE]: concrete case + design-situation family (`VividOrange.Cases`)

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]              | [CAPABILITY]                                                         |
| :-----: | :---------------- | :------------------------- | :------------------------------------------------------------------- |
|  [01]   | `PermanentCase`   | permanent case impl        | `ActionClass.Permanent`; defaults `Nickname="G"`, `Name="Dead Load"` |
|  [02]   | `VariableCase`    | variable case impl         | `ActionClass.Variable`; `ψ0`/`ψ1`/`ψ2` reads; `IsFavourable`         |
|  [03]   | `DesignSituation` | partial-factor policy impl | `γ`-factor settable rows (defaults) incl. `PrestressPartialFactor`   |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: EN load-case construction (`ENLoadCaseFactory`, static)
- note: each `Create*` mints a `VariableCase` pre-loaded with the action's EN 1990 Annex A1.1 `ψ` factors, and every action factory carries BOTH a no-loads overload and an `IList<ILoad>` loaded overload.
- note: the `EN.ITableA1_2` γ-table CONTRACT is public while its only implementations (`ENTableA1_2A/B/C`) are `internal` — no public member yields one, so a consumer composes the public `IDesignSituation` partial-factor surface (`GammaQ1`/`GammaQi`/`Xi`) and never a table singleton.

| [INDEX] | [SURFACE]                                            | [CAPABILITY]                             |
| :-----: | :--------------------------------------------------- | :--------------------------------------- |
|  [01]   | `CreateImposed(ImposedLoadCategory, NationalAnnex)`  | category-keyed `ψ` factors, empty case   |
|  [02]   | `CreateSnow(NationalAnnex, bool altitudeAbove1000m)` | altitude-keyed snow `ψ` factors          |
|  [03]   | `CreateThermal(NationalAnnex)`                       | thermal `ψ` factors                      |
|  [04]   | `CreateWind(NationalAnnex)`                          | wind `ψ` factors                         |
|  [05]   | `Create{Imposed,Snow,Thermal,Wind}(IList<ILoad>, …)` | the LOADS-first overload of each factory |

[ENTRYPOINT_SCOPE]: EN combination synthesis (`ENCombinationFactory`, static)
- note: each `Create*` folds an `IList<ILoadCase> cases` (`prefix="LC"`, `firstCaseId=1`; ULS forms add `NationalAnnex`) into the EN /6.10a-b set

| [INDEX] | [SURFACE]                                                                                   | [CAPABILITY]        |
| :-----: | :------------------------------------------------------------------------------------------ | :------------------ |
|  [01]   | `CreateCharacteristic -> IList<ICharacteristicCombination>`                                 | SLS characteristic  |
|  [02]   | `CreateFrequent -> IList<IFrequentCombination>`                                             | SLS frequent        |
|  [03]   | `CreateQuasiPermanent -> IList<IQuasiPermanentCombination>`                                 | SLS quasi-permanent |
|  [04]   | `CreateEquSetA(…[, NationalAnnex]) -> IList<IEquilibriumCombination>`                       | EQU Set A           |
|  [05]   | `CreateStrGeoSetB(…, NationalAnnex, bool use6_10aAnd6_10b, …)`                              | STR/GEO Set B       |
|  [06]   | `CreateStrGeoSetC(…[, NationalAnnex]) -> IList<IGeotechnicalMemberDesignCombination>`       | STR/GEO Set C       |
|  [07]   | `CreateAccidental(IVariableCase, double partialFactor, …) -> IList<IAccidentalCombination>` | accidental ULS      |
|  [08]   | `CreateSeismic(IList<IVariableCase>, Ratio partialFactor, …) -> IList<ISeismicCombination>` | seismic ULS         |

[ENTRYPOINT_SCOPE]: load-factoring algebra (`Combinations.Utility`, static; EN-table reads)
- note: `<T>` is `where T: ILoadCase`; the accompanying-variable overloads narrow it `where T: IVariableCase`

| [INDEX] | [SURFACE]                                                              | [CAPABILITY]                           |
| :-----: | :--------------------------------------------------------------------- | :------------------------------------- |
|  [01]   | `FactorLoads<T>(Ratio partialFactor, IList<T>)`                        | each case's `ILoad` scaled by one `γ`  |
|  [02]   | `FactorLoads<T>(IDesignSituation, IList<T>, IList<bool> isFavourable)` | per-case favourable/unfavourable `γ`   |
|  [03]   | `GetLoads<T>(IList<T>)`                                                | flatten the case set's `ILoad` rows    |
|  [04]   | `FactorAccompanyingVariableLoads<T>(Ratio, IList<T>, Func<T,Ratio>)`   | `ψ·γ` accompanying factoring           |
|  [05]   | `SelectAccompanyingVariableLoads<T>(IList<T>, Func<T,Ratio>)`          | `ψ` selection by selector              |
|  [06]   | `ILoadCombination.GetFactoredLoads()`                                  | factored `IList<ILoad>` for the solver |
|  [07]   | `EN.ITableA1_1/.ITableA1_2.GetProperties(NationalAnnex)`               | `ψ`/`γ` `Ratio` factor set per annex   |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- case hierarchy: `ICase` (`Name`) → `ILoadCase` (`Nickname`, `IList<ILoad> Loads`, `IsHorizontal`, `ActionClass`) → `IPermanentCase` (marker) / `IVariableCase` (`CombinationFactor`/`FrequentFactor`/`QuasiPermanentFactor` = `ψ0`/`ψ1`/`ψ2`, `Ratio`)
- combination hierarchy: `ILoadCombination` (`Definition`, `PermanentCases`, `PermanentCaseIsFavourable`, `LeadingVariableCases`, `GetFactoredLoads()`) → `IUltimateLimitState` (+ `DesignSituation`) / `IServiceabilityLimitState` (+ accompanying cases) → the EN-clause leaf contracts
- design situation: `IDesignSituation` carries `UnfavourablePermanentActionsPartialFactor`/`FavourablePermanentActionsPartialFactor` (`γG,sup`/`γG,inf`), `LeadingActionPartialFactor`/`MainAccompanyingVariableActionsPartialFactor` (`γQ`, nullable for the leading-action sweep), `OtherAccompanyingVariableActionsPartialFactor`, `ReductionFactor` (`ξ`); concrete `DesignSituation` adds `PrestressPartialFactor`
- code tables: `TableA1_1Properties`/`TableA1_2Properties` construct from `double` via the `RatioUnit.DecimalFraction` ctor; the EN singletons (`ENTableA1_1Imposed`/`Snow`/`Thermal`/`Wind`, `ENTableA1_2A`/`B`/`C`) dispatch on `NationalAnnex`

[STACKING]:
- with `VividOrange.Loads` (`.api/api-vividorange-loads`): `ILoadCase.Loads` and `ILoadCombination.GetFactoredLoads()` are both `IList<ILoad>`; the case/combination engine folds OVER the load taxonomy — `Combinations.Utility.FactorLoads` applies `ILoad.Factor(Ratio)` across a case set, so factored output stays `UnitsNet`-backed loads
- with `UnitsNet` (`libs/dotnet/.api/api-unitsnet.md`): every combination/partial factor is a `Ratio` and `ILoad.Factor(γ)` multiplies by `γ.DecimalFractions`; the `ψ·γ` accompanying-action weighting is `Ratio` arithmetic
- with `VividOrange.IStandards` (`.api/api-vividorange-istandards`) + `VividOrange.Countries` (`.api/api-vividorange-countries`): `NationalAnnex` (incl. `RecommendedValues`) is the dispatch key for every factory and table; the project's `ICountry` selects which `NationalAnnex` parameterizes the set through `Model/eurocode#EUROCODE_ALGEBRA` `AnnexRegime.Of(ICountry)` over the ISO alpha-2 `CountryCode` (no compiled `Country`→`NationalAnnex` map exists, and the display `Name` is unsound as the key)
- with `VividOrange.ISerialization`: `ICase`/`ILoadCombination: ITaxonomySerializable` share the one taxonomy-serialization marker, so the whole structural taxonomy round-trips through a single serializer
- with `StructuralAnalysisFormat` (`.api/api-structural-analysis-format`): outputs map onto `ExcelStructuralLoadCase` (`ExcelActionType`=`ActionClass`) and `ExcelStructuralLoadCombination` whose `ExcelLoadCaseCombinationStandard` (`EnUlsSetB`/`EnUlsSetC`/`EnAccidental*`/`EnSeismic`/`EnSls*`) images `CreateStrGeoSetB`/`SetC`/`CreateAccidental`/`CreateSeismic`/`CreateCharacteristic`/`Frequent`/`QuasiPermanent`; `LoadFactors`/`LoadMultipliers`/`LoadCases` carry the factored result to the SAF wire
- with `LanguageExt.Core`: an uncovered `NationalAnnex` faults at the boundary — the EN `ψ`/`γ` singletons throw `NotImplementedException` and `IStandard.Title` can throw `MissingNationalAnnexException`; `Op.Catch` preserves either exact exceptional error, while explicit unsupported-policy admissions mint `BimFault.Refused` under the model/capability axes

[LOCAL_ADMISSION]:
- the canonical in-graph carrier is the neutral `PropertyName`→`PropertyValue` row set `Model/eurocode#EUROCODE_ALGEBRA` stamps; VividOrange `ILoadCase`/`ILoadCombination` are the Eurocode engine that PRODUCES those rows and never a type re-exported across the boundary
- every `ψ`/`γ` factor is a `Ratio`, never a bare `double`; the `IDesignSituation` `double` partial-factor reads are wrapped in `Ratio.FromDecimalFractions` at the boundary before factoring
- a national deviation is a `NationalAnnex` table row passed to `GetProperties`/the factory, never a per-country code branch; `RecommendedValues` is the EN fallback when no annex applies
- concrete `PermanentCase`/`VariableCase`/`DesignSituation` are MUTABLE settable-property carriers — the Eurocode authoring surface; project the produced combination set onto the immutable Bim records
