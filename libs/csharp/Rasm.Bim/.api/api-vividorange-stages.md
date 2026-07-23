# [RASM_BIM_API_VIVIDORANGE_STAGES]

`VividOrange.Stages`, over the `VividOrange.IStages` contract floor, owns the AEC design/construction project-lifecycle stage taxonomy: the `IStage` project-phase contract, the national-body `IGovernance`, and the concrete national stage classes the international and regional planning bodies govern.

Every concrete stage implements one international category interface, so heterogeneous national phases normalize onto a single cross-national axis a consumer dispatches by `is IConstruction`; the Bim layer folds the roster into the canonical `ProjectStage` discriminant the `Planning/schedule#SCHEDULE` owner carries, stamping `IfcProject.Phase` and the COBie handover stage, each governing body's national context read off `IGovernance.Country` as the sibling `VividOrange.Countries` `ICountry`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `VividOrange.Stages` (+ `VividOrange.IStages`)
- package: `VividOrange.Stages` (MIT)
- assembly: `VividOrange.Stages` + `VividOrange.IStages` — pure-managed AnyCPU IL, no native asset, ALC-safe inside the in-Rhino plugin; the `net10.0` consumer binds `lib/net8.0`
- namespace: `VividOrange.Stages` (contracts + International stages), `.UK` (RIBA 2020), `.UK.RIBA2007` (RIBA A-L), `.Germany` (HOAI), `.Italy` (CSLP), `.Denmark` (AB89)
- depends: `VividOrange.IStages` (interface contracts), `VividOrange.Countries` (the `ICountry` off `IGovernance.Country`), `VividOrange.ISerialization` (the `ITaxonomySerializable` marker) — all MIT, pure-managed
- rail: `Planning/schedule#SCHEDULE` — the project-phase axis feeding the `IfcProject.Phase` model-phase label and the COBie `CobiePhase`/`CobieStageType` handover stage

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: stage and governance contracts (`VividOrange.IStages`)

Every concrete stage implements `IStage` and one or more category interfaces; the category interface is the international normalization axis, so a consumer dispatches `stage is IConstruction`, never a string compare on `Name`/`Id`.

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]           | [CAPABILITY]                                                    |
| :-----: | :------------------ | :---------------------- | :-------------------------------------------------------------- |
|  [01]   | `IStage`            | stage contract          | `Name`/`Description`/`Id`/`Governance` — the project-phase head |
|  [02]   | `IGovernance`       | governing-body contract | `Name`/`FullBodyName`/`Country` (`ICountry`); defines a scale   |
|  [03]   | `IPredesign`        | pre-design super-cat    | `IIdea`/`IBrief`/`ICompetition` refine it                       |
|  [04]   | `IIdea`             | pre-design category     | strategic-definition phase                                      |
|  [05]   | `IBrief`            | pre-design category     | brief phase                                                     |
|  [06]   | `ICompetition`      | pre-design category     | competition phase                                               |
|  [07]   | `IConceptualDesign` | design category         | concept design phase                                            |
|  [08]   | `ISchematicDesign`  | design category         | developed-spatial design phase                                  |
|  [09]   | `IDetailedDesign`   | design category         | technical design phase                                          |
|  [10]   | `IConstruction`     | delivery category       | construction phase                                              |
|  [11]   | `IHandover`         | delivery category       | handover phase                                                  |
|  [12]   | `IInUse`            | operate category        | in-use phase                                                    |
|  [13]   | `IEndOfLife`        | operate category        | end-of-life phase                                               |

[PUBLIC_TYPE_SCOPE]: governing bodies (`VividOrange.Stages` + national namespaces)

`International` is the cross-national baseline the category interfaces realize; the national bodies own the regional scales.

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :------------------------------------------ | :------------ | :-------------------------------------------------------------- |
|  [01]   | `International` (`Stages`)                  | governance    | Whitby-Wood international stage definition; `Country` = UK      |
|  [02]   | `UK.RIBA`                                   | governance    | Royal Institute of British Architects (RIBA Plan of Work)       |
|  [03]   | `Germany.HOAI`                              | governance    | Honorarordnung für Architekten und Ingenieure (Leistungsphasen) |
|  [04]   | `Italy.ConsiglioSuperioreDeiLavoriPubblici` | governance    | CSLP — the Italian public-works council scale                   |
|  [05]   | `Denmark.AB89`                              | governance    | the Danish FRI/Danske Arkitektvirksomheder scale                |

[PUBLIC_TYPE_SCOPE]: concrete national stage rosters

Each scale is a set of `IStage` classes mapped to their category interfaces; the indexed list carries the full class roster per scale.

| [INDEX] | [SCALE]                     | [GOVERNANCE]  | [ID_RANGE]   |
| :-----: | :-------------------------- | :------------ | :----------- |
|  [01]   | International (Whitby-Wood) | `Stages`      | Id 1-10      |
|  [02]   | UK RIBA 2020 (+ RIBA F)     | `UK`          | 0-7, F       |
|  [03]   | UK RIBA 2007                | `UK.RIBA2007` | A-L (no F/I) |
|  [04]   | Germany HOAI                | `Germany`     | LP1-LP9      |
|  [05]   | Italy CSLP                  | `Italy`       | 1-5          |
|  [06]   | Denmark AB89                | `Denmark`     | 0-8          |

- [01]-[INTERNATIONAL]: `Idea` `Brief` `Competition` `ConceptDesign` `SchematicDesign` `DetailedDesign` `Construction` `Handover` `InUse` `EndOfLife` — the Whitby-Wood baseline, each implementing `IStage` and its category interface.
- [02]-[RIBA2020]: `RIBAStage0`…`RIBAStage7` and `RIBAStageF` — Strategic-Definition through In-Use; `RIBAStageF` is the retained RIBA-2007 Stage F (`Id "F"`, Product Information) kept in the `UK` namespace.
- [03]-[RIBA2007]: `RIBAStageA` through `L`, skipping F and I — Appraisal through Post-Practical-Completion, ten classes A-E/G-H/J-L, F living in `UK`.
- [04]-[HOAI]: `LP1`…`LP9` — Grundlagenermittlung through Objektbetreuung Leistungsphasen.
- [05]-[CSLP]: `PFTE` `DD` `EXE` `DL` `Collaudo` — feasibility/definitive/executive/works-direction/testing.
- [06]-[AB89]: `Ideoplaeg` `Byggeprogram` `Dispositionsforslag` `Projektforslag` `Myndighedsprojekt` `Hovedprojekt` `Projektopfoelgning` `Udfoerelse` `Aflevering` — idea through handover.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: stage construction and member read

A stage is a parameterless `new` of a concrete class, its members constant per class; the consumer instantiates the roster once and folds it into a lookup keyed by `(Governance.Name, Id)` or category interface.

| [INDEX] | [SURFACE]                                       | [SHAPE]  | [CAPABILITY]                                                   |
| :-----: | :---------------------------------------------- | :------- | :------------------------------------------------------------- |
|  [01]   | `new UK.RIBAStage4()` (and every roster class)  | ctor     | the concrete national stage (constant members)                 |
|  [02]   | `stage.Name` / `stage.Description` / `stage.Id` | property | display name, human description, scale-local `Id` (unprefixed) |
|  [03]   | `stage.Governance` → `IGovernance`              | property | the governing body (`Name`/`FullBodyName`/`Country`)           |
|  [04]   | `stage.Governance.Country` → `ICountry`         | property | the `VividOrange.Countries` context the body belongs to        |

- `Id` is a bare scale-local token, unprefixed — RIBA `"4"`/`"F"`, HOAI `"5"` (not `"LP5"`), Danish `"7"`, Italian `"1"`.

[ENTRYPOINT_SCOPE]: cross-national category normalization

Category interfaces are the normalization API — a pattern-match lifts a heterogeneous national stage onto its international category, the operation a multi-standard project performs.

| [INDEX] | [SURFACE]                                           | [SHAPE]  | [CAPABILITY]                                              |
| :-----: | :-------------------------------------------------- | :------- | :-------------------------------------------------------- |
|  [01]   | `stage is IConstruction` / `is IDetailedDesign` / … | operator | normalize a national stage onto the international axis    |
|  [02]   | `stage is IPredesign`                               | operator | the pre-design grouping (`IIdea`/`IBrief`/`ICompetition`) |
|  [03]   | `roster.OfType<IConstruction>()`                    | fold     | every construction-phase stage across all national scales |
|  [04]   | `roster.Where(s => s.Governance is UK.RIBA)`        | fold     | the stage scale of one governing body                     |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `VividOrange.IStages` holds the contract floor: `IStage` (`Name`/`Description`/`Id`/`Governance`) is the project-phase head, `IGovernance` (`Name`/`FullBodyName`/`Country`) the defining body, and the category interfaces the international axis; `IStage`/`IGovernance` are `: ITaxonomySerializable`, each category interface is `: IStage`, and `IIdea`/`IBrief`/`ICompetition` refine `IPredesign` — itself both a leaf category Denmark's `Ideoplaeg` implements directly and their super-category
- each concrete class is a constant value object with compile-time-literal members implementing `IStage`, its category interface, and `ITaxonomySerializable`, so the roster is a fixed catalogue instantiated once, never data-driven
- `Id` is scale-local and stringly-typed, not a cross-scale key; the cross-scale key is the category interface, so a consumer normalizes by `is IConstruction`, never by parsing `Id`
- `VividOrange.Stages` carries no ordering, transition, or date logic — a static taxonomy of phase identities whose phase order and phase-to-schedule binding are the consumer's

[STACKING]:
- `Planning/schedule#SCHEDULE` (within-lib): the `IStage` + category pair lowers onto a canonical `[SmartEnum<string>]`/`[ValueObject]` `ProjectStage` discriminant keyed by `(Governance.Name, Id)`, its `Category` arm the international interface, so a multi-standard project's RIBA/HOAI/CSLP/AB89 phases reconcile on one axis
- `Model/elements#ELEMENT_MODEL` (within-lib): the GeometryGym `IfcProject.Phase` label is stamped from the resolved `IStage.Name` or its international category at the `Exchange` boundary
- `VividOrange.Loads`/`VividOrange.Cases`(`.api/api-vividorange-cases`): a design-layer seam sharing no compiled member — a construction `IStage` selects the `ILoadCase` set live during that phase, joining the `Planning/schedule#SCHEDULE` `ConstructionTask` to the `Model/structural#ANALYSIS_MODEL` load rows active at that task, the sole compiled meeting point the shared `VividOrange.Countries` context
- `VividOrange.Countries`(`.api/api-vividorange-countries`): `IGovernance.Country` returns the sibling `ICountry` over a compiled `VividOrange.IStages`→`VividOrange.Countries` pin; that catalogue owns the one national-context axis backing both the structural and lifecycle families
- `api-xbim-cobieexpress`(`.api/api-xbim-cobieexpress`): the COBie `CobiePhase`/`CobieStageType`/`CobieImpactStage` pick-values draw their stage vocabulary from this taxonomy, so the design phase and the FM-handover phase share one vocabulary across the model, schedule, and COBie register
- `VividOrange.ISerialization`: `IStage`/`IGovernance` carry the shared `ITaxonomySerializable` marker, so the stage roster round-trips through the one VividOrange taxonomy serializer covering the whole family

[LOCAL_ADMISSION]:
- `VividOrange.Stages` is the project-lifecycle phase vocabulary: the Bim layer folds the `IStage` roster into the canonical `ProjectStage` discriminant once at startup, keyed by `(Governance.Name, Id)`, with the international category interface as the cross-national normalization arm

[RAIL_LAW]:
- Package: `VividOrange.Stages` (MIT)
- Owns: the AEC design/construction project-lifecycle stage taxonomy — the `IStage`/`IGovernance` contracts, the international category interfaces, and the concrete national stage rosters with their governing bodies, each carrying its `VividOrange.Countries` national context
- Accept: the canonical project-phase vocabulary the `Planning/schedule#SCHEDULE` `ProjectStage` discriminant folds in once, cross-nationally normalized through the category interfaces, supplying the `IfcProject.Phase` model-phase label and the COBie `CobiePhase`/`CobieStageType` handover stage, resolving national context against the shared `VividOrange.Countries` owner
- Reject: a structural-load/analysis reading — the load/case taxonomy is the sibling `VividOrange.Loads`/`VividOrange.Cases` → `Model/structural#ANALYSIS_MODEL`, and this package touches no loads, forces, or analysis; a hand-rolled RIBA/HOAI phase enum beside the taxonomy; cross-national reconciliation by parsing the scale-local `Id` or comparing `Name` instead of the category interface; a parallel country enum where `VividOrange.Countries` owns the national context; binding by a strong-name `AssemblyVersion` rather than package identity
