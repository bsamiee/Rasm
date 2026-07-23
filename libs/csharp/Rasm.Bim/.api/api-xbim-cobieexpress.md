# [RASM_BIM_API_XBIM_COBIEEXPRESS]

`Xbim.CobieExpress` owns the COBie EXPRESS FM asset-register: an operations-handover schema, the `CobieModel` STEP21/Esent/spreadsheet store, and the turnkey IFC→COBie converter. COBie feeds `Exchange/export#EXPORT_RAIL` as the FM digital-handover export leg, carrying the asset information the geometry and IFC-graph arms omit. Its exchanger reads a PARALLEL xBIM `IModel`, never Rasm's GeometryGym authority, so the canonical path authors the `CobieModel` from the `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph`, the exchanger admitted only as a terminal `.ifc` file→file handover.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Xbim.CobieExpress` (entity model) · `Xbim.IO.CobieExpress` (store) · `Xbim.CobieExpress.Exchanger` (converter)
- packages: `Xbim.CobieExpress` + `Xbim.IO.CobieExpress` + `Xbim.CobieExpress.Exchanger` (CDDL-1.0)
- assembly: `Xbim.CobieExpress` / `Xbim.IO.CobieExpress` / `Xbim.CobieExpress.Exchanger` — net10 binds `lib/netstandard2.0` (each also ships `net472`), pure-managed AnyCPU IL, ALC-safe, no native asset
- namespace: `Xbim.CobieExpress` (+ `.Interfaces` `ICobie*`) entity model; `Xbim.IO.CobieExpress` (+ `.Resolvers`) `CobieModel` store; `Xbim.CobieExpress.Exchanger` (+ `.Conversion`/`.Classifications`/`.FilterHelper`/`.IfcHelpers`) converter
- transitive: `Xbim.Common` (the `IModel`/`IPersistEntity`/`ITransaction`/`ExpressMetaData` EXPRESS runtime); `Xbim.IO.Table` (the COBie-spreadsheet `ModelMapping` behind `ExportToTable`/`ImportFromTable`); `Xbim.Ifc`/`Xbim.Ifc4` (the parallel xBIM IFC stack the exchanger reads); `Microsoft.Extensions.Logging.Abstractions`, `Newtonsoft.Json`, `System.Configuration.ConfigurationManager` (exchanger config/log)
- scope: the COBie EXPRESS FM-handover asset schema, its STEP21/Esent/spreadsheet store, and the turnkey IFC→COBie conversion
- rail: `Exchange/export#EXPORT_RAIL` — the FM digital-handover export leg

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: COBie entity model + shared bases (`Xbim.CobieExpress`)
- note: every entity is an `IPersistEntity`; author through `model.Instances.New<TEntity>()` inside a `BeginTransaction` scope.

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY]          | [CAPABILITY]                                                        |
| :-----: | :---------------------------------------- | :--------------------- | :------------------------------------------------------------------ |
|  [01]   | `CobieReferencedObject` (abstract)        | provenance base        | externally-keyed provenance head (members [01])                     |
|  [02]   | `CobieAsset` (abstract)                   | named-asset base       | `: CobieReferencedObject`; named asset (members [02])               |
|  [03]   | `CobieFacility`/`CobieFloor`/`CobieSpace` | spatial entities       | IFC building/storey/space (`SpatialDivision`; `CobieSite` at site)  |
|  [04]   | `CobieType`/`CobieComponent`              | type/instance          | `: CobieTypeOrComponent` — TYPE catalogue + installed COMPONENTs    |
|  [05]   | `CobieSystem`/`CobieZone`                 | grouping entities      | `: CobieAsset` — system groups components, zone groups spaces       |
|  [06]   | `CobieSpare`/`CobieResource`/`CobieJob`   | FM-operations          | spare parts, maintenance resources, jobs that consume them          |
|  [07]   | `CobieContact`                            | contact entity         | a person/organisation responsible party (keyed by `Email`)          |
|  [08]   | `CobieAttribute`                          | attribute entity       | a `CobieAsset.Attributes` member (members [08])                     |
|  [09]   | `CobieDocument` + satellites              | satellite entities     | the 6 satellite reference kinds (roster [09])                       |
|  [10]   | `CobieProject` + header entities          | header entities        | project/site/phase header + created-by/on record (roster [10])      |
|  [11]   | `CobiePickValue` (abstract) + subtypes    | pick-list dictionaries | 17 closed enumeration/classification vocabularies (roster [11])     |
|  [12]   | `CobieAreaUnit` + unit pick-values        | unit pick-values       | 6 unit dictionaries — cost currency + `UnitsNet` (roster [12])      |
|  [13]   | `AttributeValue` (select) + value structs | value select           | EXPRESS select over 5 primitive value structs (roster [13])         |
|  [14]   | `EntityFactoryCobieExpress`               | entity factory         | `: IEntityFactory` — the schema factory `CobieModel` builds through |

- [01]-[PROVENANCE]: `Created` (`CobieCreatedInfo`), `ExternalObject` (`CobieExternalObject`), `ExternalSystem` (`CobieExternalSystem`), `ExternalId`/`AltExternalId`.
- [02]-[ASSET]: `Name`, `Description`, `Categories`, `Attributes`, `Documents`, `Impacts`, `Representations`, `CausingIssues`, `AffectedBy`.
- [08]-[ATTRIBUTE]: `Name` + `Value` (`AttributeValue` select) + `Unit` + `Stage` (`CobieStageType`) + `AllowedValues`.
- [09]-[SATELLITE]: `CobieDocument`/`CobieConnection`/`CobieCoordinate`/`CobieIssue`/`CobieImpact`/`CobieClassification`.
- [10]-[HEADER]: `CobieProject`/`CobieSite`/`CobiePhase`/`CobieCreatedInfo`.
- [11]-[PICK]: `CobieCategory`/`CobieRole`/`CobieAssetType`/`CobieStageType`/`CobieConnectionType`/`CobieDocumentType`/`CobieJobType`/`CobieJobStatusType`/`CobieIssueType`/`CobieSpareType`/`CobieResourceType`/`CobieImpactType`/`CobieImpactStage`/`CobieApprovalType`/`CobieIssueChance`/`CobieIssueImpact`/`CobieIssueRisk` — the `Issue*` risk/chance/impact triad keys `CobieIssue`.
- [12]-[UNITS]: `CobieAreaUnit`/`CobieLinearUnit`/`CobieVolumeUnit`/`CobieCurrencyUnit`/`CobieDurationUnit`/`CobieImpactUnit`.
- [13]-[VALUE]: `StringValue`/`IntegerValue`/`FloatValue`/`BooleanValue`/`DateTimeValue`.

[PUBLIC_TYPE_SCOPE]: the `CobieModel` store (`Xbim.IO.CobieExpress`)
- note: `CobieModel : IModel, IDisposable` holds the entity graph, transaction log, and serialization surface; author in a `BeginTransaction` scope, serialize through the save family.

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY]    | [CAPABILITY]                                                     |
| :-----: | :------------------------------------- | :--------------- | :--------------------------------------------------------------- |
|  [01]   | `CobieModel`                           | model store      | `: IModel, IDisposable` — the COBie entity store (members [01])  |
|  [02]   | `AttributeTypeResolver` (`.Resolvers`) | type resolver    | `: ITypeResolver` — resolves COBie attribute value types on load |
|  [03]   | `ExcelTypeEnum` (`Xbim.IO.Table`)      | spreadsheet kind | XLS/XLSX selector for the `ExportToTable(Stream, …)` overload    |
|  [04]   | `ModelMapping` (`Xbim.IO.Table`)       | table mapping    | `GetMapping()` mapping for `ExportToTable`/`ImportFromTable`     |

- [01]-[STORE]: `CobieModel` exposes `Instances`/`Metadata`/`ModelFactors`/`SchemaVersion` + transactions.

[PUBLIC_TYPE_SCOPE]: the IFC→COBie exchanger (`Xbim.CobieExpress.Exchanger`)
- note: the `MappingIfc*ToCobie*` family is internal; the public surface is the converter and its parameter/mode types.

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY]       | [CAPABILITY]                                                               |
| :-----: | :--------------------------------- | :------------------ | :------------------------------------------------------------------------- |
|  [01]   | `IfcToCoBieExpressExchanger`       | exchanger           | `: XbimExchanger<IModel, IModel>`; `Convert()` → `CobieFacility`           |
|  [02]   | `CobieExpressConverter`            | turnkey converter   | `: ICobieConverter`; `Run(...)` → `Task<IModel>`, runs exchanger + commits |
|  [03]   | `ICobieConverter`                  | converter contract  | `Task<IModel> Run(CobieConversionParams)`                                  |
|  [04]   | `CobieConversionParams`            | conversion params   | the conversion params (fields [04])                                        |
|  [05]   | `EntityIdentifierMode`             | id-mode enum        | `IfcEntityLabels`/`GloballyUniqueIds`/`None` — external-id mode            |
|  [06]   | `SystemExtractionMode` (`[Flags]`) | system-mode enum    | `System`/`PropertyMaps`/`Types` — what counts as a COBie system            |
|  [07]   | `ExternalReferenceMode`            | ext-ref enum        | which external system/entity-type names COBie writes                       |
|  [08]   | `ExportFormatEnum`                 | format enum         | `XLS`/`XLSX`/`JSON`/`XML`/`IFC`/`STEP21` — the COBie export targets        |
|  [09]   | `OutputFilters` + filter family    | extraction filters  | role/object/property inclusion filters (rides `.Filter`; roster [09])      |
|  [10]   | `XbimExchanger<TSource, TTarget>`  | exchanger framework | the generic mapping-engine base (roster [10])                              |

- [04]-[PARAMS]: `Source` (xBIM `IModel`), `NewCobieModel` (`Func<IModel>`), `ExtId`, `SysMode`, `Filter`, `ConfigFile`, `ReportProgress`.
- [09]-[FILTERS]: `ObjectFilter`/`PropertyFilter`/`RoleFilter`/`ImportSet`.
- [10]-[FRAMEWORK]: `XbimMappings<…>`/`IXbimMappings<…>`/`ProgressReporter`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: author the `CobieModel` directly (canonical path)
- note: the canonical Rasm path builds the register from the `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph` `Element`s + properties with NO xBIM IFC reader — `Element` → `CobieComponent`, its type → `CobieType`, spatial parent → `CobieFloor`/`CobieSpace`, Pset → `CobieAttribute`.

| [INDEX] | [SURFACE]                                                                             | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :------------------------------------------------------------------------------------ | :------- | :--------------------------------- |
|  [01]   | `new CobieModel()` / `(IModel)` / `(string)`                                          | ctor     | in-memory or Esent-backed store    |
|  [02]   | `model.BeginTransaction(string) -> ITransaction`                                      | instance | author scope; `txn.Commit()` seals |
|  [03]   | `model.Instances.New<T>()`                                                            | instance | author an entity (types [03])      |
|  [04]   | `model.SetDefaultEntityInfo(DateTime, string, string, string) -> CobieCreatedInfo`    | instance | default provenance stamp           |
|  [05]   | `asset.Categories` / `asset.Attributes` / `asset.Documents`                           | property | attribute/category/document sets   |
|  [06]   | `model.InsertCopy<T>(T, XbimInstanceHandleMap, PropertyTranformDelegate, bool, bool)` | instance | deep-copy an entity + inverses     |

[ENTRYPOINT_SCOPE]: serialization and the COBie-spreadsheet bridge
- note: the register serializes to EXPRESS STEP21, the COBie spreadsheet (XLS/XLSX), or Esent; `ExportToTable` is the canonical FM-handover deliverable.

| [INDEX] | [SURFACE]                                                                        | [SHAPE]  | [CAPABILITY]                 |
| :-----: | :------------------------------------------------------------------------------- | :------- | :--------------------------- |
|  [01]   | `model.SaveAsStep21` / `SaveAsStep21Zip` / `SaveAsEsent`                         | instance | STEP/zip/Esent forms         |
|  [02]   | `model.ExportToTable(string, out string, ModelMapping?, Stream?)`                | instance | FM XLS/XLSX deliverable      |
|  [03]   | `model.ExportToTable(Stream, ExcelTypeEnum, out string, ModelMapping?, Stream?)` | instance | stream overload              |
|  [04]   | `CobieModel.OpenStep21` / `OpenStep21Zip` / `OpenEsent`                          | static   | re-open a register           |
|  [05]   | `CobieModel.ImportFromTable(string, out string, ModelMapping?) -> CobieModel`    | static   | round-trip a spreadsheet     |
|  [06]   | `CobieModel.GetMapping() -> ModelMapping`                                        | static   | default column/sheet mapping |

[ENTRYPOINT_SCOPE]: turnkey IFC→COBie conversion (terminal file→file handover only)
- note: the exchanger reads an xBIM `IModel` (never GeometryGym) — admissible only as a terminal one-way transform off the persisted `.ifc`, opened/converted/disposed within the call.

| [INDEX] | [SURFACE]                                                                      | [SHAPE]  | [CAPABILITY]                           |
| :-----: | :----------------------------------------------------------------------------- | :------- | :------------------------------------- |
|  [01]   | `new CobieExpressConverter(logger).Run(CobieConversionParams) -> Task<IModel>` | instance | turnkey async conversion (params [01]) |
|  [02]   | `new IfcToCoBieExpressExchanger(...).Convert() -> IModel`                      | instance | lower-level exchanger (ctor [02])      |
|  [03]   | `CobieConversionParams { ExtId, SysMode, Filter, ConfigFile, ReportProgress }` | ctor     | id-mode/system/filter knobs            |

- [01]-[RUN]: `Source = xbimIfcModel`, `NewCobieModel = () => new CobieModel`, `ExtId = GloballyUniqueIds`, `SysMode = System | Types`, `Filter = OutputFilters.Default`.
- [02]-[EXCHANGER]: ctor `(source, target, reportProgress, filter, configFile, extId, sysMode, classify)`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Xbim.CobieExpress` is an EXPRESS entity model over `Xbim.Common` — every entity is an `IPersistEntity` activated lazily in a `CobieModel`, authored inside a `BeginTransaction` scope, and inverse navigation (`CobieAsset.CausingIssues`/`AffectedBy`) is index-backed
- `CobieReferencedObject` heads the inheritance spine that carries shared facets once: it owns the external-id + `CobieCreatedInfo` provenance, `CobieAsset` owns the name/description + the `IOptionalItemSet<T>` category/attribute/document/impact/representation collections, and `CobieTypeOrComponent` splits into `CobieType` (equipment catalogue) and `CobieComponent` (installed instance)
- `CobiePickValue` heads the closed dictionary tier (categories, roles, asset/job/issue/spare types, units, currencies) — the enumeration vocabularies COBie rows reference, authored once per model and shared
- `CobieAttribute` bridges the Pset: a (name, `AttributeValue` select over `StringValue`/`IntegerValue`/`FloatValue`/`BooleanValue`/`DateTimeValue`, optional unit) carried on `CobieAsset.Attributes` — the COBie projection of an IFC property
- `IfcToCoBieExpressExchanger` reads the xBIM `Xbim.Ifc`/`Xbim.Ifc4` toolkit (`IIfcBuilding`/`IIfcSpace`/`IIfcTypeObject`), a separate IFC implementation from Rasm's GeometryGym semantic authority (`api-geometrygym-ifc`); the two never share an in-memory graph

[STACKING]:
- `Exchange/export#EXPORT_RAIL`: COBie is one export leg beside the geometry (`SharpGLTF`) and IFC-graph (`GeometryGym DatabaseIfc`) arms — an `InterchangeCodec.Cobie` arm and `InterchangeFormat` rows (XLSX via `ExportToTable`, STEP21 via `SaveAsStep21`, JSON), the `ExportArtifact` bytes content-keyed by the shared `Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING` `InterchangeIdentity`
- `Semantics/properties#PROPERTY_TEMPLATES` (`api-xbim-properties`): the `CobieAttribute` rows project the Pset properties onto (name + `AttributeValue` + unit), the same Pset vocabulary the template defines, never a re-typed property model
- `Planning/cost#ESTIMATE` (`api-nodamoney`) + `UnitsNet` (`libs/csharp/.api/api-unitsnet.md`): the `CobieCurrencyUnit`/`CobieAreaUnit`/`CobieLinearUnit`/`CobieVolumeUnit` pick-values stamp from the `NodaMoney` `Currency` and `UnitsNet` SI units the cost/quantity owners hold
- `VividOrange.Stages` (`api-vividorange-stages`): the `CobiePhase`/`CobieStageType`/`CobieImpactStage` draw their stage vocabulary from the project-lifecycle taxonomy
- `NodaTime` (`libs/csharp/.api/api-nodatime.md`): the `CobieCreatedInfo.CreatedOn` provenance instant stamps from the model `ClockPolicy`, never a BCL `DateTime.Now` at the call site

[LOCAL_ADMISSION]:
- `CobieModel` is the FM-handover asset register, authored from the `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph` `Element`s + properties through `Instances.New<T>()` in a transaction; a hand-rolled COBie spreadsheet writer or a parallel asset-register model beside it is the rejected form
- `IfcToCoBieExpressExchanger` admits its xBIM source ONLY as a terminal `.ifc`-file→COBie transform opened/converted/disposed inside the export call; a retained xBIM `IModel` as a live second authority alongside GeometryGym is the boundary violation
- COBie attributes project the Pset vocabulary (`api-xbim-properties`), units the `NodaMoney`/`UnitsNet` owners, phases `VividOrange.Stages`, provenance the `NodaTime` `ClockPolicy` — never a fresh vocabulary minted in the COBie owner
- `ExportToTable` mints the canonical COBie-spreadsheet FM deliverable, `SaveAsStep21` the STEP21 interchange form, Esent a working-store backend

[RAIL_LAW]:
- Package: `Xbim.CobieExpress` + `Xbim.IO.CobieExpress` + `Xbim.CobieExpress.Exchanger` (CDDL-1.0)
- Owns: the COBie EXPRESS FM digital-handover asset-information schema, the `CobieModel` store (STEP21/Esent/spreadsheet IO, transactions, `ExportToTable`/`ImportFromTable`), and the turnkey IFC→COBie converter
- Accept: the `CobieModel` authored directly from the `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph` `Element`s + `Semantics/properties#PROPERTY_TEMPLATES` Pset attributes + `VividOrange.Stages` phases + `NodaMoney`/`UnitsNet` units + `NodaTime` provenance, serialized as the COBie spreadsheet/STEP21 export leg of `Exchange/export#EXPORT_RAIL` content-keyed by the shared `InterchangeIdentity`
- Reject: standing up the xBIM `Xbim.Ifc`/`Xbim.Ifc4` reader as a second in-memory IFC authority beside GeometryGym; a hand-rolled COBie spreadsheet writer or a parallel asset-register model beside `CobieModel`; re-typing the Pset/unit/phase vocabulary instead of projecting the owners; binding the `net472` asset (net10 binds `netstandard2.0`); vendoring or modifying the CDDL-1.0 source rather than referencing the binary
