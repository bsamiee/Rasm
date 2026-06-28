# [RASM_BIM_API_XBIM_COBIEEXPRESS]

The three-assembly COBie EXPRESS digital-handover stack: `Xbim.CobieExpress` is the COBie schema
ENTITY MODEL (the asset-information graph — `CobieFacility`/`CobieFloor`/`CobieSpace`/`CobieZone`/
`CobieType`/`CobieComponent`/`CobieSystem`/`CobieSpare`/`CobieResource`/`CobieJob`/`CobieDocument`/
`CobieAttribute`/`CobieContact`/`CobieIssue`/`CobieImpact`/`CobieConnection`/`CobieCoordinate` over
the shared `CobieAsset`/`CobieReferencedObject`/`CobiePickValue` bases, plus the
`EntityFactoryCobieExpress`); `Xbim.IO.CobieExpress` is the `CobieModel` STORE (a full `IModel` —
STEP21/Esent/zip open-save, the `ExportToTable`/`ImportFromTable` COBie-spreadsheet bridge over
`Xbim.IO.Table`, transactions, `InsertCopy`, inverse/entity caching); and `Xbim.CobieExpress.Exchanger`
is the turnkey IFC→COBie converter (`IfcToCoBieExpressExchanger`/`CobieExpressConverter` driving the
`MappingIfc*ToCobie*` mapping family under `EntityIdentifierMode`/`SystemExtractionMode`/`OutputFilters`).
COBie is the FM ASSET-REGISTER export — the post-construction operations handover (spaces, equipment
types and instances, systems, spares, maintenance jobs, documents, contacts, and their attributes) —
so this is a leg of the `Exchange/export#EXPORT_RAIL` complementing the geometry (`SharpGLTF`) and
model-graph (`GeometryGym DatabaseIfc`) export arms. The LOAD-BEARING BOUNDARY: the exchanger reads
an xBIM `Xbim.Common.IModel` (`Xbim.Ifc4.Interfaces.IIfcBuilding`), a PARALLEL IFC stack to Rasm's
GeometryGym semantic authority (`api-geometrygym-ifc`), so the canonical path AUTHORS the `CobieModel`
DIRECTLY from the `Model/elements#ELEMENT_MODEL` `BimModel`/`ElementSet` + properties through the store
API (never standing up a second in-memory IFC graph), and the exchanger is admissible ONLY as a TERMINAL
one-way file→file handover off the persisted `.ifc` (a transient xBIM read, never a held authority). It
is pure-managed under CDDL-1.0 (`requireLicenseAcceptance=false`), beside the `Xbim.Properties`
(`api-xbim-properties`) and `Xbim.InformationSpecifications` (`api-xbim-informationspecifications`)
siblings under the same xBIM lineage and license posture.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Xbim.CobieExpress` (entity model) · `Xbim.IO.CobieExpress` (store) · `Xbim.CobieExpress.Exchanger` (converter)
- package: `Xbim.CobieExpress` + `Xbim.IO.CobieExpress` + `Xbim.CobieExpress.Exchanger` — three direct
  csproj pins under the "COBie Exchange" group
- version: `6.0.172` (all three); the deeper xBIM core binds the centrally-pinned
  `Xbim.Common`/`Xbim.Ifc`/`Xbim.Ifc4` `6.0.587` + `Xbim.IO.Table` `6.0.172` (the packages' own
  nuspecs floor the core at `6.0.445`; the central pin lifts the consumed assemblies to `6.0.587`)
- license: CDDL-1.0 (`license type="expression"`, xBimTeam) with `requireLicenseAcceptance=false` — the
  weak-copyleft file-level reciprocity is satisfied by referencing the unmodified NuGet binaries, never
  vendoring or modifying source (the same posture as `Xbim.Properties`/`Xbim.InformationSpecifications`)
- assembly: `Xbim.CobieExpress` / `Xbim.IO.CobieExpress` / `Xbim.CobieExpress.Exchanger` → the
  `net10.0` consumer binds `lib/netstandard2.0` for each (each multi-targets `netstandard2.0`+`net472`;
  `net472` is .NET-Framework-only and incompatible with net10, so `netstandard2.0` is the bound asset);
  pure-managed AnyCPU IL, ALC-safe, no native asset
- namespace: `Xbim.CobieExpress` (+ `.Interfaces` `ICobie*` mirror) for the entity model;
  `Xbim.IO.CobieExpress` (+ `.Resolvers`) for the `CobieModel` store; `Xbim.CobieExpress.Exchanger`
  (+ `.Conversion`/`.Classifications`/`.FilterHelper`/`.IfcHelpers`) for the IFC→COBie converter
- transitive: `Xbim.Common` 6.0.587 (the `IModel`/`IPersistEntity`/`ITransaction`/`ExpressMetaData`
  EXPRESS runtime the entity model rides); `Xbim.IO.Table` 6.0.172 (the COBie-spreadsheet `ModelMapping`
  the store's `ExportToTable`/`ImportFromTable` uses); `Xbim.Ifc`/`Xbim.Ifc4` 6.0.587 (the xBIM IFC
  reader the EXCHANGER consumes as its source — the parallel-authority surface, see boundary);
  `Microsoft.Extensions.Logging.Abstractions`, `Newtonsoft.Json`, `System.Configuration.ConfigurationManager`
  (exchanger config/log)
- scope: the COBie EXPRESS FM-handover asset-information schema, its STEP21/Esent/spreadsheet store, and
  the turnkey IFC→COBie conversion; NOT an IFC semantic authority (GeometryGym owns that), NOT a
  geometry store (COBie carries only `CobieCoordinate` placement points), NOT a property-template source
  (`Xbim.Properties` owns Pset templates)
- rail: `Exchange/export#EXPORT_RAIL` (the FM digital-handover export leg)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: COBie entity model + shared bases (`Xbim.CobieExpress`)
- rail: export
- note: every entity is an `IPersistEntity` in a `CobieModel`; the inheritance spine is
  `PersistEntity` → `CobieReferencedObject` (external-id + provenance) → `CobieAsset` (named asset with
  categories/attributes/documents/impacts/representations) → `CobieTypeOrComponent` → `CobieType`/
  `CobieComponent`. Author entities through `model.Instances.New<TEntity>()` inside a transaction.

| [INDEX] | [SYMBOL]                                      | [TYPE_FAMILY]      | [RAIL]                                            |
| :-----: | :-------------------------------------------- | :----------------- | :----------------------------------------------- |
|  [01]   | `CobieReferencedObject` (abstract)            | provenance base    | `Created` (`CobieCreatedInfo`), `ExternalObject` (`CobieExternalObject`), `ExternalSystem` (`CobieExternalSystem`), `ExternalId`/`AltExternalId` — every COBie row's externally-keyed provenance head |
|  [02]   | `CobieAsset` (abstract)                        | named-asset base   | `: CobieReferencedObject` — `Name`, `Description`, `Categories`, `Attributes`, `Documents`, `Impacts`, `Representations`, `CausingIssues`, `AffectedBy` |
|  [03]   | `CobieFacility` / `CobieFloor` / `CobieSpace` | spatial entities | the IFC building/storey/space spatial breakdown (`SpatialDivision` select, with `CobieSite` at the site level) |
|  [04]   | `CobieType` / `CobieComponent` (`: CobieTypeOrComponent`) | type/instance | the equipment TYPE catalogue and its installed instance COMPONENTs |
|  [05]   | `CobieSystem` / `CobieZone`                    | grouping entities  | `: CobieAsset` logical groupings — a functional system grouping components, a zone grouping spaces (NOT `SpatialDivision`) |
|  [06]   | `CobieSpare` / `CobieResource` / `CobieJob`    | FM-operations      | spare parts, maintenance resources, and the maintenance/operation jobs that consume them |
|  [07]   | `CobieContact`                                 | contact entity     | a person/organisation responsible party (keyed by `Email`) |
|  [08]   | `CobieAttribute`                               | attribute entity   | one Pset property as a COBie attribute (`Name` + `Value` (`AttributeValue` select) + `Unit` + `Stage` (`CobieStageType`) + `AllowedValues`), the `CobieAsset.Attributes` member |
|  [09]   | `CobieDocument` / `CobieConnection` / `CobieCoordinate` / `CobieIssue` / `CobieImpact` / `CobieClassification` | satellite entities | linked documents, component connections, geometry placement, issues, sustainability impacts, classification references |
|  [10]   | `CobieProject` / `CobieSite` / `CobiePhase` / `CobieCreatedInfo` | header entities | the COBie project/site/phase header and the created-by/created-on provenance record |
|  [11]   | `CobiePickValue` (abstract) + `CobieCategory`/`CobieRole`/`CobieAssetType`/`CobieStageType`/`CobieConnectionType`/`CobieDocumentType`/`CobieJobType`/`CobieJobStatusType`/`CobieIssueType`/`CobieSpareType`/`CobieResourceType`/`CobieImpactType`/`CobieImpactStage`/`CobieApprovalType`/`CobieIssueChance`/`CobieIssueImpact`/`CobieIssueRisk` | pick-list dictionaries | the closed enumeration/classification vocabularies COBie rows reference (the `Issue*` risk/chance/impact triad keys `CobieIssue`) |
|  [12]   | `CobieAreaUnit`/`CobieLinearUnit`/`CobieVolumeUnit`/`CobieCurrencyUnit`/`CobieDurationUnit`/`CobieImpactUnit` | unit pick-values | the COBie unit dictionaries (the `Planning/cost` currency + `UnitsNet` measure touchpoints) |
|  [13]   | `AttributeValue` (select) + `StringValue`/`IntegerValue`/`FloatValue`/`BooleanValue`/`DateTimeValue` | value select | the typed COBie attribute value (EXPRESS select over the five primitive value structs) |
|  [14]   | `EntityFactoryCobieExpress` (`IEntityFactory`) | entity factory     | the schema entity factory the `CobieModel` constructs entities through |

[PUBLIC_TYPE_SCOPE]: the `CobieModel` store (`Xbim.IO.CobieExpress`)
- rail: export
- note: `CobieModel : IModel, IDisposable` is the full xBIM model store over the COBie schema — it
  holds the entity graph, the transaction log, and the serialization surface; author through
  `Instances.New<T>()` in a `BeginTransaction` scope and serialize through the save family.

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]      | [RAIL]                                            |
| :-----: | :----------------------------- | :----------------- | :----------------------------------------------- |
|  [01]   | `CobieModel`                   | model store        | `: IModel, IDisposable` — the COBie entity store (`Instances`/`Metadata`/`ModelFactors`/`SchemaVersion`, transactions, STEP21/Esent/spreadsheet IO) |
|  [02]   | `COBieModelProviderFactory`    | provider factory   | `: IModelProviderFactory` — the model-provider wiring (`CreateProvider`/`Use`) |
|  [03]   | `AttributeTypeResolver` (`.Resolvers`) | type resolver | `: ITypeResolver` — resolves COBie attribute value types on load |
|  [04]   | `ExcelTypeEnum` (`Xbim.IO.Table`) | spreadsheet kind | the XLS/XLSX selector the `ExportToTable(Stream, ExcelTypeEnum, …)` overload takes |
|  [05]   | `ModelMapping` (`Xbim.IO.Table`) | table mapping    | the COBie-spreadsheet column/sheet mapping `GetMapping()` supplies and `ExportToTable`/`ImportFromTable` drive |

[PUBLIC_TYPE_SCOPE]: the IFC→COBie exchanger (`Xbim.CobieExpress.Exchanger`)
- rail: export
- note: the exchanger consumes an xBIM `IModel` source (`IIfcBuilding`), NOT the GeometryGym authority
  — the parallel-IFC-stack boundary; use it only as a terminal file→file handover off the persisted
  `.ifc`. The mapping family (`MappingIfc*ToCobie*`) is internal; the public surface is the converter +
  its parameter/mode types.

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]      | [RAIL]                                            |
| :-----: | :----------------------------- | :----------------- | :----------------------------------------------- |
|  [01]   | `IfcToCoBieExpressExchanger`   | exchanger          | `: XbimExchanger<IModel, IModel>` — iterates `IIfcBuilding` → `CobieFacility` via the `MappingIfc*` family; `Convert()` returns the target `IModel` |
|  [02]   | `CobieExpressConverter`        | turnkey converter  | `: ICobieConverter` — `Run(CobieConversionParams)` → `Task<IModel>`; builds a `CobieModel`, runs the exchanger in a transaction, commits |
|  [03]   | `ICobieConverter`              | converter contract | `Task<IModel> Run(CobieConversionParams)` |
|  [04]   | `CobieConversionParams`        | conversion params  | `Source` (xBIM `IModel`), `NewCobieModel` (`Func<IModel>`), `ExtId`, `SysMode`, `Filter`, `ConfigFile`, `ReportProgress` |
|  [05]   | `EntityIdentifierMode`         | id-mode enum       | `IfcEntityLabels` / `GloballyUniqueIds` / `None` — how COBie external-ids reference the source IFC |
|  [06]   | `SystemExtractionMode` (`[Flags]`) | system-mode enum | `System` / `PropertyMaps` / `Types` — what counts as a COBie system during extraction |
|  [07]   | `ExternalReferenceMode`        | ext-ref enum       | which external system/entity-type names COBie writes |
|  [08]   | `ExportFormatEnum`             | format enum        | `XLS`/`XLSX`/`JSON`/`XML`/`IFC`/`STEP21` — the COBie export targets |
|  [09]   | `OutputFilters` / `ObjectFilter` / `PropertyFilter` / `RoleFilter` / `ImportSet` | extraction filters | the COBie role/object/property inclusion filters (`OutputFilters` rides `CobieConversionParams.Filter`) |
|  [10]   | `XbimExchanger<TSource, TTarget>` / `XbimMappings<…>` / `IXbimMappings<…>` / `ProgressReporter` | exchanger framework | the generic mapping-engine base the IFC→COBie family extends |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: canonical path — author the `CobieModel` directly (GeometryGym authority respected)
- rail: export
- note: the canonical Rasm path builds the COBie register FROM the `BimModel`/`ElementSet` + properties
  through the store API, with NO xBIM IFC reader — `BimElement` → `CobieComponent`, its type →
  `CobieType`, spatial parent → `CobieFloor`/`CobieSpace`, Pset → `CobieAttribute`.

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY]  | [RAIL]                                            |
| :-----: | :---------------------------------------------------------------- | :-------------- | :----------------------------------------------- |
|  [01]   | `new CobieModel()` / `new CobieModel(IModel)` / `new CobieModel(string esentDbFile)` | construct | an in-memory (or Esent-backed) COBie store |
|  [02]   | `using var txn = model.BeginTransaction(name)` … `txn.Commit()`    | transaction     | author entities inside a transaction scope        |
|  [03]   | `model.Instances.New<CobieFacility>()` (and `New<CobieFloor>`/`New<CobieSpace>`/`New<CobieType>`/`New<CobieComponent>`/`New<CobieSystem>`/`New<CobieAttribute>`/…) | author | create a COBie entity in the store from the `BimModel` projection |
|  [04]   | `model.SetDefaultEntityInfo(date, email, givenName, familyName)` → `CobieCreatedInfo` | provenance | the default created-by/created-on stamp every authored row inherits |
|  [05]   | `asset.Categories` / `asset.Attributes` / `asset.Documents` (`IOptionalItemSet<T>`) | author | add the Pset attributes, classification categories, and linked documents to an asset |
|  [06]   | `model.InsertCopy<T>(toCopy, mappings, propTransform, includeInverses, keepLabels)` | author | deep-copy an entity (and inverses) between stores |

[ENTRYPOINT_SCOPE]: serialization and the COBie-spreadsheet bridge
- rail: export
- note: the COBie register serializes to the EXPRESS STEP21 form, the COBie spreadsheet (XLS/XLSX), or
  Esent; the spreadsheet `ExportToTable` is the canonical FM-handover deliverable.

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY]  | [RAIL]                                            |
| :-----: | :---------------------------------------------------------------- | :-------------- | :----------------------------------------------- |
|  [01]   | `model.SaveAsStep21(file)` / `SaveAsStep21Zip(file)` / `SaveAsEsent(dbName)` | save | the COBie EXPRESS STEP / zipped-STEP / Esent forms |
|  [02]   | `model.ExportToTable(file, out report, mapping = null, template = null)` | save (spreadsheet) | the COBie spreadsheet deliverable (the FM-handover XLS/XLSX) |
|  [03]   | `model.ExportToTable(Stream, ExcelTypeEnum, out report, mapping, template)` | save (spreadsheet) | the stream + explicit XLS/XLSX-kind overload |
|  [04]   | `CobieModel.OpenStep21(path/stream, esentDB)` / `OpenStep21Zip(…)` / `OpenEsent(db)` | open | re-open a persisted COBie register |
|  [05]   | `CobieModel.ImportFromTable(file, out report, mapping = null)` → `CobieModel` | open (spreadsheet) | round-trip a COBie spreadsheet back into the model |
|  [06]   | `CobieModel.GetMapping()` → `ModelMapping`                         | mapping         | the default COBie-spreadsheet column/sheet mapping |

[ENTRYPOINT_SCOPE]: turnkey IFC→COBie conversion (terminal file→file handover only)
- rail: export
- note: the exchanger reads an xBIM `IModel` (NOT GeometryGym) — admissible only as a terminal one-way
  transform off the persisted `.ifc`, never a held second authority; the source xBIM model is opened,
  converted, and disposed within the export call.

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY]  | [RAIL]                                            |
| :-----: | :---------------------------------------------------------------- | :-------------- | :----------------------------------------------- |
|  [01]   | `new CobieExpressConverter(logger).Run(new CobieConversionParams { Source = xbimIfcModel, NewCobieModel = () => new CobieModel(), ExtId = GloballyUniqueIds, SysMode = System \| Types, Filter = OutputFilters.Default })` → `Task<IModel>` | convert | the turnkey async IFC→COBie conversion |
|  [02]   | `new IfcToCoBieExpressExchanger(source, target, reportProgress, filter, configFile, extId, sysMode, classify).Convert()` → `IModel` | convert | the lower-level exchanger (`source`/`target` both xBIM `IModel`) |
|  [03]   | `CobieConversionParams { ExtId, SysMode, Filter, ConfigFile, ReportProgress }` | configure | the conversion knobs (id mode, system extraction, role/object/property filter) |

## [04]-[IMPLEMENTATION_LAW]

[ENTITY_TOPOLOGY]:
- the COBie schema is an EXPRESS entity model over `Xbim.Common` — every entity is an
  `IPersistEntity` activated lazily in a `CobieModel`, authored only inside a `BeginTransaction` scope,
  and the inverse navigation (`CobieAsset.CausingIssues`/`AffectedBy`) is index-backed
- the inheritance spine carries the shared facets once: `CobieReferencedObject` owns the external-id +
  `CobieCreatedInfo` provenance, `CobieAsset` owns the name/description + the `IOptionalItemSet<T>`
  category/attribute/document/impact/representation collections, `CobieTypeOrComponent` splits into the
  `CobieType` (equipment catalogue) and `CobieComponent` (installed instance) — the canonical COBie
  type↔instance distinction
- the `CobiePickValue` hierarchy is the closed dictionary tier (categories, roles, asset/job/issue/spare
  types, units, currencies) — these are the enumeration vocabularies COBie rows reference, authored once
  per model and shared
- the `CobieAttribute` is the Pset bridge: one attribute is a (name, `AttributeValue` select over
  `StringValue`/`IntegerValue`/`FloatValue`/`BooleanValue`/`DateTimeValue`, optional unit) carried on
  `CobieAsset.Attributes` — the COBie projection of an IFC property

[BOUNDARY] — the xBIM parallel-IFC-stack (load-bearing):
- the EXCHANGER (`IfcToCoBieExpressExchanger`/`CobieExpressConverter`) reads an xBIM `Xbim.Common.IModel`
  with `Xbim.Ifc4.Interfaces.IIfcBuilding`/`IIfcSpace`/`IIfcTypeObject` entities — this is the xBIM IFC
  toolkit (`Xbim.Ifc`/`Xbim.Ifc4` 6.0.587), a SEPARATE IFC implementation from Rasm's GeometryGym
  semantic authority (`api-geometrygym-ifc`); the two never share an in-memory graph
- canonical Rasm path: AUTHOR the `CobieModel` directly from the `Model/elements#ELEMENT_MODEL`
  `BimModel`/`ElementSet` + the `Semantics/properties#PROPERTY_SETS` properties through the store API —
  no xBIM IFC reader is stood up, GeometryGym stays the sole authority, and the COBie register is a
  projection of the canonical model
- terminal-handover path (guarded): the turnkey `CobieExpressConverter.Run` reading the persisted `.ifc`
  through `Xbim.Ifc.IfcStore.Open` is admissible ONLY as a one-way file→file transform at the export
  edge — the xBIM model is opened, converted, and disposed inside the call, never retained as a second
  authority; holding an xBIM `IModel` alongside the GeometryGym `IfcSemanticModel` as a live source is
  the rejected form

[INTEGRATION_STACK]:
- with `Exchange/export#EXPORT_RAIL`: COBie is a new export leg beside the geometry (`SharpGLTF`) and
  IFC-graph (`GeometryGym DatabaseIfc`) arms — a `InterchangeCodec.Cobie` arm and `InterchangeFormat`
  rows (COBie XLSX via `ExportToTable`, COBie STEP21 via `SaveAsStep21`, COBie JSON), the `ExportArtifact`
  bytes content-keyed by the same `Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING` `InterchangeIdentity`
  the GLB/IFC export seals; COBie carries the FM ASSET-INFORMATION the geometry/IFC arms do not
- with `Semantics/properties#PROPERTY_SETS` (`api-xbim-properties`): the `CobieAttribute` rows are the
  COBie projection of the Pset properties — the same Pset vocabulary the `Xbim.Properties` template
  defines and the GeometryGym model carries, mapped onto `CobieAttribute` (name + `AttributeValue` +
  unit), never a re-typed property model
- with `Planning/cost#ESTIMATE` (`api-nodamoney`) + `UnitsNet` (`api-unitsnet`): the
  `CobieCurrencyUnit`/`CobieAreaUnit`/`CobieLinearUnit`/`CobieVolumeUnit` pick-values are stamped from
  the `NodaMoney` `Currency` and the `UnitsNet` SI units the cost/quantity owners hold, so the COBie
  register's units agree with the model's typed money/measures
- with `VividOrange.Stages` (`api-vividorange-stages`): the COBie `CobiePhase`/`CobieStageType`/
  `CobieImpactStage` draw their stage vocabulary from the project-lifecycle taxonomy, so the design
  phase and the handover phase share one stage vocabulary
- with `NodaTime` (`api-nodatime`): the `CobieCreatedInfo.CreatedOn` provenance instant is stamped from
  the model `ClockPolicy`, never a BCL `DateTime.Now` at the call site

[LOCAL_ADMISSION]:
- the FM-handover asset register is the `CobieModel`, AUTHORED from the `BimModel`/`ElementSet` +
  properties through `Instances.New<T>()` in a transaction — a hand-rolled COBie spreadsheet writer or
  a parallel asset-register model beside `CobieModel` is the rejected form
- the IFC→COBie exchanger's xBIM source is admitted ONLY as a terminal `.ifc`-file→COBie transform; a
  retained xBIM `IModel` as a live second authority alongside GeometryGym is the named boundary violation
- COBie attributes are the Pset projection (`api-xbim-properties` vocabulary), units the
  `NodaMoney`/`UnitsNet` projection, phases the `VividOrange.Stages` projection, provenance the
  `NodaTime` `ClockPolicy` projection — never a fresh vocabulary minted in the COBie owner
- the COBie spreadsheet (`ExportToTable`) is the canonical FM deliverable; STEP21 (`SaveAsStep21`) is
  the interchange form; Esent is a working-store backend, not a deliverable

[RAIL_LAW]:
- Package: `Xbim.CobieExpress` + `Xbim.IO.CobieExpress` + `Xbim.CobieExpress.Exchanger` (6.0.172,
  CDDL-1.0, `requireLicenseAcceptance=false`, pure-managed `lib/netstandard2.0` AnyCPU IL binding
  forward under net10; transitive `Xbim.Common`/`Xbim.IO.Table` 6.0.172 + the xBIM `Xbim.Ifc`/`Xbim.Ifc4`
  6.0.587 IFC stack the exchanger reads)
- Owns: the COBie EXPRESS FM digital-handover asset-information schema (facility/floor/space/zone,
  type/component, system, spare/resource/job, document/contact/attribute/issue/impact/connection/
  coordinate + the pick-value dictionaries), the `CobieModel` store (STEP21/Esent/spreadsheet IO,
  transactions, `ExportToTable`/`ImportFromTable`), and the turnkey IFC→COBie converter
- Accept: the canonical `CobieModel` authored DIRECTLY from the `Model/elements#ELEMENT_MODEL` `BimModel`
  + `Semantics/properties#PROPERTY_SETS` Pset attributes + `VividOrange.Stages` phases +
  `NodaMoney`/`UnitsNet` units + `NodaTime` provenance, serialized as the COBie spreadsheet/STEP21 export
  leg of `Exchange/export#EXPORT_RAIL` content-keyed by the shared `InterchangeIdentity`
- Reject: standing up the xBIM `Xbim.Ifc`/`Xbim.Ifc4` reader as a second in-memory IFC authority beside
  GeometryGym (the exchanger is a terminal `.ifc`-file→COBie transform only); a hand-rolled COBie
  spreadsheet writer or a parallel asset-register model beside `CobieModel`; re-typing the Pset/unit/
  phase vocabulary instead of projecting the `Xbim.Properties`/`UnitsNet`/`NodaMoney`/`VividOrange.Stages`
  owners; binding the `net472` asset (net10 binds `netstandard2.0`); vendoring or modifying the CDDL-1.0
  source rather than referencing the binary
