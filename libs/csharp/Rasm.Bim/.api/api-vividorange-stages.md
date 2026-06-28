# [RASM_BIM_API_VIVIDORANGE_STAGES]

`VividOrange.Stages` (+ its interface floor `VividOrange.IStages`) is the authoritative AEC
DESIGN/CONSTRUCTION LIFECYCLE-STAGE taxonomy — NOT a structural-analysis stage set (the
admission-run "structural context" label is wrong; this package never touches loads, forces, or
analysis, and the structural load/case taxonomy is the sibling `VividOrange.Loads`/`VividOrange.Cases`
admissions feeding `Model/structural#ANALYSIS_MODEL`). It is the project-PHASE dimension: the
`IStage` contract (`Name`/`Description`/`Id`/`Governance`) plus the national-body `IGovernance`
contract (`Name`/`FullBodyName`/`Country`, the `ICountry` resolving against the sibling
`VividOrange.Countries`), and a roster of concrete stage classes spanning the international
Whitby-Wood baseline, the UK RIBA Plan of Work (2020 stages 0-7 plus the 2007 stages A-L), the
German HOAI Leistungsphasen (LP1-LP9), the Italian CSLP scale (PFTE/DD/EXE/DL/Collaudo), and the
Danish AB89 scale. The INTEGRATION VALUE is the cross-national NORMALIZATION carried by the
category interfaces: every concrete national stage implements BOTH `IStage` AND one (or more) of
the ten international category interfaces (`IIdea`/`IBrief`/`ICompetition`/`IPredesign`/
`IConceptualDesign`/`ISchematicDesign`/`IDetailedDesign`/`IConstruction`/`IHandover`/`IInUse`/
`IEndOfLife`), so a UK `RIBAStage5`, a German `LP8`, and a Danish `Udfoerelse` are all `IConstruction`
— a single international category axis unifies otherwise heterogeneous national phase scales. The
Bim consumer reads this as the canonical project-lifecycle vocabulary the planning owner stamps onto
the model phase (`IfcProject.Phase`) and the schedule, lowering the `IStage`/category pair onto a
`[SmartEnum]`/`[ValueObject]` `ProjectStage` discriminant rather than a hand-rolled RIBA enum, and
the same vocabulary supplies the COBie handover phase (`api-xbim-cobieexpress` `CobiePhase`/
`CobieStageType`). It is pure-managed AnyCPU IL under MIT, beside the `VividOrange.Countries`
national-context sibling under the same publisher and license.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `VividOrange.Stages` (+ `VividOrange.IStages`)
- package: `VividOrange.Stages` (the concrete stage classes) — the csproj direct pin; it pulls
  `VividOrange.IStages` (the interface floor), `VividOrange.Countries`, and `VividOrange.ISerialization`
  as transitive pins (all centrally pinned `0.1.0`)
- version: `0.1.0` (informational `0.1.0-preview.221`; `AssemblyVersion` is `0.0.0.0`, so bind by
  package identity, never by a strong-name `AssemblyVersion`)
- license: MIT (`license type="expression"`, publisher "Vivid Orange", `vivid-orange/Taxonomy`); no
  `requireLicenseAcceptance` — reference the unmodified NuGet binary
- assembly: `VividOrange.Stages` + `VividOrange.IStages` → the `net10.0` consumer binds `lib/net8.0`
  (each ships `net8.0`/`net7.0`/`net6.0`/`netstandard2.0`/`net48`; `net8.0` is the bound asset and
  binds forward under `net10.0`); pure-managed AnyCPU IL, ALC-safe inside the in-Rhino plugin assembly,
  no native asset
- namespace: `VividOrange.Stages` (the `IStage`/`IGovernance`/category interfaces in `IStages`, plus
  the International concrete stages + `International` governance in `Stages`), `VividOrange.Stages.UK`
  (RIBA 2020 + `RIBA` governance), `VividOrange.Stages.UK.RIBA2007` (RIBA 2007 stages A-L),
  `VividOrange.Stages.Germany` (HOAI), `VividOrange.Stages.Italy` (CSLP), `VividOrange.Stages.Denmark`
  (AB89)
- transitive: `VividOrange.IStages` (the interface contracts), `VividOrange.Countries` (the `ICountry`
  the `IGovernance.Country` returns — `api-vividorange-countries`), `VividOrange.ISerialization` (the
  `ITaxonomySerializable` marker every stage carries) — all MIT, all pure-managed, all centrally pinned
- scope: the design/construction project-lifecycle STAGE taxonomy and its national-body governance +
  cross-national category normalization; NOT a structural-load/case taxonomy (that is the sibling
  `VividOrange.Loads`/`VividOrange.Cases`), NOT an analysis-stage or construction-sequence model
  (that is `Planning/schedule#SCHEDULE`)
- rail: `Planning/schedule#SCHEDULE` (the project-phase/lifecycle axis), feeding the `IfcProject.Phase`
  model-phase label and the `Exchange` COBie `CobiePhase`/`CobieStageType` handover stage

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: stage and governance contracts (`VividOrange.IStages`)
- rail: schedule
- note: every concrete stage implements `IStage` (the four-member project-phase contract) AND one or
  more category interfaces; the category interface IS the international normalization axis a national
  phase maps onto, so dispatch is `stage is IConstruction`, never a string compare on `Name`/`Id`.

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]        | [RAIL]                                                       |
| :-----: | :----------------------------- | :------------------- | :---------------------------------------------------------- |
|  [01]   | `IStage : ITaxonomySerializable` | stage contract     | `Name`/`Description`/`Id`/`Governance` — the project-phase head every concrete stage carries |
|  [02]   | `IGovernance`                  | governing-body contract | `Name`/`FullBodyName`/`Country` (`ICountry`) — the national/international body that defines a stage scale |
|  [03]   | `IPredesign : IStage`          | category (pre-design) | the pre-design super-category `IIdea`/`IBrief`/`ICompetition` refine |
|  [04]   | `IIdea` / `IBrief` / `ICompetition` | category (pre-design) | `: IPredesign, IStage` — the strategic-definition / brief / competition phases |
|  [05]   | `IConceptualDesign` / `ISchematicDesign` / `IDetailedDesign` | category (design) | `: IStage` — the concept / developed-spatial / technical design phases |
|  [06]   | `IConstruction` / `IHandover` / `IInUse` / `IEndOfLife` | category (delivery/operate) | `: IStage` — the construction / handover / in-use / end-of-life phases |

[PUBLIC_TYPE_SCOPE]: governing bodies (`VividOrange.Stages` + national namespaces)
- rail: schedule
- note: an `IGovernance` carries the body's `Country`; the International body is the cross-national
  baseline the category interfaces realize, the national bodies the regional scales.

| [INDEX] | [SYMBOL]                                              | [TYPE_FAMILY]   | [RAIL]                                            |
| :-----: | :--------------------------------------------------- | :-------------- | :----------------------------------------------- |
|  [01]   | `International` (`Stages`)                            | governance      | Whitby-Wood international stage definition; `Country` = UK |
|  [02]   | `UK.RIBA`                                             | governance      | Royal Institute of British Architects (RIBA Plan of Work) |
|  [03]   | `Germany.HOAI`                                        | governance      | Honorarordnung für Architekten und Ingenieure (Leistungsphasen) |
|  [04]   | `Italy.ConsiglioSuperioreDeiLavoriPubblici`          | governance      | CSLP — the Italian public-works council scale     |
|  [05]   | `Denmark.AB89`                                        | governance      | the Danish FRI/Danske Arkitektvirksomheder scale  |

[PUBLIC_TYPE_SCOPE]: concrete national stage rosters
- rail: schedule
- note: each row is a set of `IStage` classes; the category interface column is the international
  axis the national scale maps onto (the normalization the consumer dispatches by).

| [INDEX] | [SYMBOL]                                              | [TYPE_FAMILY]   | [RAIL]                                            |
| :-----: | :--------------------------------------------------- | :-------------- | :----------------------------------------------- |
|  [01]   | `Idea`/`Brief`/`Competition`/`ConceptDesign`/`SchematicDesign`/`DetailedDesign`/`Construction`/`Handover`/`InUse`/`EndOfLife` (`Stages`) | International stages (Id 1-10) | the Whitby-Wood baseline, each `IStage` + its category interface |
|  [02]   | `UK.RIBAStage0`…`RIBAStage7` (+ `RIBAStageF`)        | RIBA 2020 stages | Strategic-Definition(0) … In-Use(7) Plan of Work, each mapped to a category interface |
|  [03]   | `UK.RIBA2007.RIBAStageA`…`RIBAStageL`                | RIBA 2007 stages | Appraisal(A) … Post-Practical-Completion(L) legacy scale |
|  [04]   | `Germany.LP1`…`LP9`                                  | HOAI phases     | Grundlagenermittlung(1) … Objektbetreuung(9) Leistungsphasen |
|  [05]   | `Italy.PFTE`/`DD`/`EXE`/`DL`/`Collaudo`             | CSLP stages (1-5) | feasibility / definitive / executive / works-direction / testing |
|  [06]   | `Denmark.Ideoplaeg`/`Byggeprogram`/`Dispositionsforslag`/`Projektforslag`/`Myndighedsprojekt`/`Hovedprojekt`/`Projektopfoelgning`/`Udfoerelse`/`Aflevering` | AB89 stages (0-8) | idea / program / outline / proposal / permission / main-project / supervision / construction / handover |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: stage construction and member read
- rail: schedule
- note: a stage is a parameterless `new` of the concrete class (the `Name`/`Description`/`Id`/
  `Governance` are constant per class); the consumer instantiates the roster once and folds it into a
  lookup keyed by `(Governance.Name, Id)` or by category interface.

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY]  | [RAIL]                                            |
| :-----: | :---------------------------------------------------------------- | :-------------- | :----------------------------------------------- |
|  [01]   | `new VividOrange.Stages.UK.RIBAStage4()` (and every roster class) | construct       | the concrete national stage (constant members)    |
|  [02]   | `stage.Name` / `stage.Description` / `stage.Id`                    | read            | the display name, human description, and scale-local id (`"4"`, `"F"`, `"LP5"`-style) |
|  [03]   | `stage.Governance` → `IGovernance`                                 | read            | the governing body (`Name`/`FullBodyName`/`Country`) |
|  [04]   | `stage.Governance.Country` → `ICountry`                            | read            | the `VividOrange.Countries` national context the body belongs to |

[ENTRYPOINT_SCOPE]: cross-national category normalization (the integration entry)
- rail: schedule
- note: the category interfaces ARE the normalization API — pattern-match a heterogeneous stage onto
  its international category, the canonical operation a multi-standard project performs.

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY]  | [RAIL]                                            |
| :-----: | :---------------------------------------------------------------- | :-------------- | :----------------------------------------------- |
|  [01]   | `stage is IConstruction` / `is IDetailedDesign` / `is IConceptualDesign` / … | category test | normalize any national stage onto the international category axis |
|  [02]   | `stage is IPredesign`                                              | super-category test | the pre-design grouping (`IIdea`/`IBrief`/`ICompetition`) |
|  [03]   | `roster.OfType<IConstruction>()`                                   | category select | every construction-phase stage across all national scales |
|  [04]   | `roster.Where(s => s.Governance is UK.RIBA)`                       | body select     | the stage scale of one governing body              |

## [04]-[IMPLEMENTATION_LAW]

[STAGE_TOPOLOGY]:
- the contract floor lives in `VividOrange.IStages` (namespace `VividOrange.Stages`): `IStage`
  (`Name`/`Description`/`Id`/`Governance`) is the project-phase head, `IGovernance`
  (`Name`/`FullBodyName`/`Country`) is the defining body, and the ten category interfaces
  (`IIdea`/`IBrief`/`ICompetition`/`IPredesign`/`IConceptualDesign`/`ISchematicDesign`/
  `IDetailedDesign`/`IConstruction`/`IHandover`/`IInUse`/`IEndOfLife`) are the international axis
- the concrete classes live in `VividOrange.Stages` and the national namespaces; each is a constant
  value object (its members are compile-time literals) implementing `IStage` + its category interface
  + `ITaxonomySerializable`, so the roster is a fixed catalogue instantiated once, never data-driven
- `Id` is SCALE-LOCAL and stringly-typed (`"4"`, `"F"`, German `"5"`, Danish `"0"`) — it is NOT a
  cross-scale key; the cross-scale key is the category INTERFACE, so a consumer normalizes by `is
  IConstruction`, never by parsing `Id`
- the package carries NO ordering/transition logic and NO date binding — it is a STATIC taxonomy of
  phase identities; the phase ORDER and the phase-to-schedule binding are the consumer's

[INTEGRATION_STACK]:
- with `Planning/schedule#SCHEDULE` (the project-phase rail): the `IStage` + category pair lowers onto
  a canonical `[SmartEnum<string>]`/`[ValueObject]` `ProjectStage` discriminant carried beside the
  `WorkScheduleKind` axis — the smart-enum key is `(Governance.Name, Id)` and the category interface
  is the cross-national fold (`ProjectStage.Category` = the international `IConstruction`/… arm), so a
  multi-standard project's RIBA/HOAI/CSLP/AB89 phases reconcile on one international axis rather than
  per-standard parallel enums; a hand-rolled RIBA-only phase enum beside this taxonomy is the retired
  form
- with `Model/elements#ELEMENT_MODEL`/`IfcProject.Phase`: the model phase label the GeometryGym
  `IfcProject.Phase` carries is stamped from the resolved `IStage.Name` (or the international category)
  at the `Exchange` boundary — the taxonomy supplies the canonical phase string the IFC project header
  carries, never a free-typed phase
- with `Exchange` COBie (`api-xbim-cobieexpress`): the COBie handover `CobiePhase` and the
  `CobieStageType`/`CobieImpactStage` pick-values draw their stage vocabulary from this taxonomy, so
  the FM-handover phase and the design phase share ONE stage vocabulary across the IFC model, the
  schedule, and the COBie register
- with `VividOrange.Countries` (`api-vividorange-countries`): the `IGovernance.Country` is the sibling
  `ICountry`, so the national-context axis (the country a governing body belongs to) is the same
  `VividOrange.Countries` value the regional `VividOrange.Loads`/`VividOrange.Cases` standards key on —
  one national-context owner across the whole VividOrange family, never a parallel country enum
- with `VividOrange.Loads`/`VividOrange.Cases` (sibling structural-taxonomy admissions →
  `Model/structural#ANALYSIS_MODEL`): a sibling, NOT a compiled dependency — this package is the
  project-phase axis, those are the structural load/case axis, and the two share no compiled member.
  The DESIGN-LAYER seam is staged construction: a construction `IStage` (the `IConstruction` category)
  selects the `VividOrange.Cases` `ILoadCase` set live during that phase, joining the
  `Planning/schedule#SCHEDULE` `ConstructionTask` to the `Model/structural#ANALYSIS_MODEL`
  `StructuralLoadKind.LoadCase` rows active at that task — a composition the design layer wires, never a
  shared package member; the only COMPILED meeting point is the shared `VividOrange.Countries` national
  context off `IGovernance.Country`
- with `VividOrange.ISerialization` (the shared taxonomy-serialization floor): `IStage`/`IGovernance`
  carry the same `ITaxonomySerializable` marker as the structural siblings `ILoad`/`ICase`/`ICountry`,
  so the lifecycle stage roster round-trips through the ONE VividOrange serializer that covers the whole
  family — the same serialization seam as the loads, cases, and national context, never a parallel stage
  serializer

[LOCAL_ADMISSION]:
- the project-lifecycle phase vocabulary is this taxonomy; the canonical `ProjectStage` discriminant
  the `Planning/schedule#SCHEDULE` owner carries is folded from the `IStage` roster ONCE at startup,
  keyed by `(Governance.Name, Id)`, with the international category interface as the cross-national
  normalization arm — a hand-coded RIBA/HOAI phase enum is the rejected form
- cross-national reconciliation is the category INTERFACE (`is IConstruction`/…), never an `Id` parse
  or a `Name` string compare — the scale-local `Id` is not a cross-scale key
- the national context the governance belongs to is the `VividOrange.Countries` `ICountry` off
  `IGovernance.Country`, never a re-modeled country enum
- this is a STATIC identity taxonomy: phase ordering, phase transitions, and phase-to-date binding are
  the `Planning/schedule#SCHEDULE` consumer's concern, never sought from this package; treating it as a
  structural-analysis stage model (it has no loads/forces/analysis) is the named mislabel

[RAIL_LAW]:
- Package: `VividOrange.Stages` (+ floors `VividOrange.IStages`/`VividOrange.Countries`/
  `VividOrange.ISerialization`, all `0.1.0`, MIT, pure-managed `lib/net8.0` AnyCPU IL binding forward
  under net10, no native asset, no `requireLicenseAcceptance`)
- Owns: the AEC design/construction project-LIFECYCLE stage taxonomy — the `IStage`/`IGovernance`
  contracts, the ten international category interfaces, and the concrete national stage rosters
  (International Whitby-Wood, UK RIBA 2020 + 2007, German HOAI, Italian CSLP, Danish AB89) with their
  governing bodies, each carrying its `VividOrange.Countries` national context
- Accept: the canonical project-phase vocabulary the `Planning/schedule#SCHEDULE` `ProjectStage`
  discriminant folds in once, cross-nationally normalized through the category interfaces, supplying the
  `IfcProject.Phase` model-phase label and the COBie `CobiePhase`/`CobieStageType` handover stage, and
  resolving its national context against the shared `VividOrange.Countries` owner
- Reject: treating this as a structural-analysis/load stage model (it is project-lifecycle only — the
  load/case taxonomy is the sibling `VividOrange.Loads`/`VividOrange.Cases` → `Model/structural#ANALYSIS_MODEL`);
  a hand-rolled RIBA/HOAI phase enum beside the taxonomy; cross-national reconciliation by parsing the
  scale-local `Id` or comparing `Name` instead of dispatching the category interface; a parallel country
  enum where `VividOrange.Countries` owns the national context; binding by a strong-name `AssemblyVersion`
  (it is `0.0.0.0` — bind by package identity)
