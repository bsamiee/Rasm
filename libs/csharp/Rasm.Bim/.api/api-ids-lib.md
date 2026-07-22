# [RASM_BIM_API_IDS_LIB]

`ids-lib` mints the buildingSMART-official `.ids` audit: `Audit.RunAsync` validates an IDS Information Delivery Specification against the IDS XSD schemas and the implementation agreements, returning a `Status` flags result over a caller-supplied `ILogger` diagnostic sink.

Beyond the audit, `IdsLib.IfcSchema` embeds the offline IFC2x3/IFC4/IFC4x3 schema graph — class hierarchy, attributes, standard Psets/Qtos, and measure/unit metadata — a self-contained schema authority the Bim property, quantity, and classification owners read directly without the audit path.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ids-lib`
- package: `ids-lib` (MIT)
- assembly: `ids-lib`
- namespace: `IdsLib`, `IdsLib.IfcSchema`, `IdsLib.SchemaProviders`
- asset: net10.0; the net10.0 consumer binds `lib/net10.0` and ships `ids-lib.xml`
- diagnostics: `Microsoft.Extensions.Logging.ILogger?` — the caller-owned issue sink
- rail: ids-validation — `.ids` conformance audit and the offline IFC schema-reflection authority

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: IdsLib — audit engine, result status, schema seam

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [CAPABILITY]                                                                           |
| :-----: | :---------------------- | :------------ | :------------------------------------------------------------------------------------- |
|  [01]   | `Audit`                 | class         | static audit entrypoint; `Run`/`RunAsync` overloads in [03]                            |
|  [02]   | `Audit.Status`          | enum          | `[Flags]` pass/fail result; flag set below                                             |
|  [03]   | `Audit.ISchemaProvider` | interface     | IDS-XSD source seam: `GetSchemas(Stream, ILogger?, out IEnumerable<XmlSchema>)`        |
|  [04]   | `LibraryInformation`    | class         | static engine-build facts: `AssemblyVersion`, `Commit`, `Sha`, `CommitDate`, `Isdirty` |

[`Audit.Status`]: `Ok`(pass) `NotImplementedError` `InvalidOptionsError` `NotFoundError` `IdsStructureError` `IdsContentError` `XsdSchemaError` `UnhandledError` `IdsStructureWarning`

[PUBLIC_TYPE_SCOPE]: IdsLib — audit options

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                                                                                     |
| :-----: | :-------------------- | :------------ | :----------------------------------------------------------------------------------------------- |
|  [01]   | `AuditProcessOptions` | class         | `SchemaProvider`, `OmitIdsSchemaAudit`, `OmitIdsContentAudit`, `XmlWarningAction`                |
|  [02]   | `SingleAuditOptions`  | class         | `AuditProcessOptions` + `IdsVersion` (default `Ids1_0`); the single-file argument                |
|  [03]   | `IBatchAuditOptions`  | interface     | the batch contract; members below                                                                |
|  [04]   | `XmlWarningBehaviour` | enum          | nested `AuditProcessOptions` escalation: `ReportAsInformation`/`ReportAsWarning`/`ReportAsError` |

[`IBatchAuditOptions`]: `InputSource` `InputExtension` `SchemaFiles` `AuditSchemaDefinition` `OmitIdsContentAudit` `OmitIdsContentAuditPattern`

[PUBLIC_TYPE_SCOPE]: IdsLib.SchemaProviders — IDS-XSD source seam

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]                                                                              |
| :-----: | :--------------------------- | :------------ | :---------------------------------------------------------------------------------------- |
|  [01]   | `SchemaProvider`             | class         | `abstract` base; subclass or implement `Audit.ISchemaProvider` to supply IDS XSD schemas  |
|  [02]   | `FixedVersionSchemaProvider` | class         | ctor `(IdsVersion)`; forces one IDS schema version regardless of the document declaration |

- default `SchemaProvider`: detects the `IdsVersion` from the source stream and loads the matching embedded IDS XSD.

[PUBLIC_TYPE_SCOPE]: IdsLib.IfcSchema — class graph

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                                                                             |
| :-----: | :------------------------ | :------------ | :--------------------------------------------------------------------------------------- |
|  [01]   | `SchemaInfo`              | class         | graph root: static `SchemaIfc{2x3,4,4x3}`, `this[name]`, `PropertySets`; queries in [03] |
|  [02]   | `IfcSchemaVersions`       | enum          | `[Flags]`: `IfcNoVersion`=0, `Ifc2x3`=1, `Ifc4`=2, `Ifc4x3`=4, `IfcAllVersions`=7        |
|  [03]   | `ClassInfo`               | class         | one IFC entity; fields below; `Is(name)` subtype test                                    |
|  [04]   | `ClassType`               | enum          | `Abstract`, `Concrete`, `Enumeration`                                                    |
|  [05]   | `FunctionalType`          | enum          | class functional role: `Element`, `ElementWithTypes`, `TypeOfElement`                    |
|  [06]   | `AttributeInfo`           | record        | one EXPRESS attribute; fields below                                                      |
|  [07]   | `ExpressDefinition`       | record        | parsed EXPRESS attribute type: optional/collection/base-type facts                       |
|  [08]   | `IfcClassInformation`     | class         | flat class record the `AllConcreteClasses` roster yields                                 |
|  [09]   | `IfcAttributeInformation` | class         | flat attribute record the `AllAttributes` roster yields                                  |

[`ClassInfo`]: `Name` `ParentName` `Parent` `Type` `FunctionalType` `SubClasses` `PredefinedTypeValues` `EnumerationValues` `DirectAttributesInfo` `NameSpace` `MatchingConcreteClasses` `RelationTypeClasses`
[`AttributeInfo`]: `Name` `ExpressType`(`ExpressDefinition`) `IsOptional` `IsCollection` `BaseType` `XmlBaseType`

[PUBLIC_TYPE_SCOPE]: IdsLib.IfcSchema — properties, measures, units

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [CAPABILITY]                                                                      |
| :-----: | :----------------------------- | :------------ | :-------------------------------------------------------------------------------- |
|  [01]   | `PropertySetInfo`              | class         | `Name`, `Properties`, `ApplicableClasses`, `PropertyNames`, `GetProperty(name)`   |
|  [02]   | `IPropertyTypeInfo`            | interface     | a property definition inside a Pset (name + value type)                           |
|  [03]   | `NamedPropertyType`            | class         | `IPropertyTypeInfo` impl: a named property value type                             |
|  [04]   | `IfcMeasureInformation`        | record        | `IUnitInformation`; measure → SI metadata; fields below                           |
|  [05]   | `IUnitInformation`             | interface     | the unit-metadata contract                                                        |
|  [06]   | `IfcConversionUnitInformation` | record        | `IUnitInformation`; the standard conversion-unit rows                             |
|  [07]   | `IfcDataTypeInformation`       | class         | a defined IFC data type; `TryParseIfcDataType` target                             |
|  [08]   | `DimensionalExponents`         | class         | the 7-vector SI exponents; `GetUnit`/`GetExponent(DimensionType)`, `IsPureNumber` |
|  [09]   | `DimensionType`                | enum          | the 7 SI base dimensions (below)                                                  |
|  [10]   | `SchemaInfo.ClassRelationInfo` | struct        | class-to-class relation: `ClassName` + `Connection` (`ClassAttributeMode`)        |

[`IfcMeasureInformation`]: `Id` `IfcMeasure` `Unit` `UnitSymbol` `DefaultDisplay` `Description` `SiUnitNameEnums` `UnitTypeEnum` `Exponents`(`DimensionalExponents`) `IsBasicUnit` `IsPureNumber` `IsDirectSIUnit`
[`DimensionType`]: `Length` `Mass` `Time` `ElectricCurrent` `Temperature` `AmountOfSubstance` `LuminousIntensity`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Audit — run

| [INDEX] | [SURFACE]                                                              | [SHAPE] | [CAPABILITY]                                       |
| :-----: | :--------------------------------------------------------------------- | :------ | :------------------------------------------------- |
|  [01]   | `Audit.RunAsync(Stream, SingleAuditOptions, ILogger?) -> Task<Status>` | static  | audit one `.ids` stream (preferred)                |
|  [02]   | `Audit.Run(Stream, SingleAuditOptions, ILogger?) -> Status`            | static  | synchronous single-file audit                      |
|  [03]   | `Audit.Run(IBatchAuditOptions, ILogger?) -> Status`                    | static  | directory/glob batch audit                         |
|  [04]   | `new SingleAuditOptions()`                                             | ctor    | configure `IdsVersion`/`SchemaProvider`/omit-scope |
|  [05]   | `new FixedVersionSchemaProvider(IdsVersion)`                           | ctor    | pin a fixed IDS-XSD version                        |

[ENTRYPOINT_SCOPE]: IdsLib.IfcSchema — `SchemaInfo` graph queries (bare rows are `SchemaInfo` members)

| [INDEX] | [SURFACE]                                                                  | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :------------------------------------------------------------------------- | :------- | :--------------------------------------- |
|  [01]   | `SchemaInfo.SchemaIfc{2x3,4,4x3} -> SchemaInfo`                            | property | ready-built schema graph per IFC version |
|  [02]   | `SchemaInfo[string] -> ClassInfo?`                                         | property | resolve a class by name                  |
|  [03]   | `ClassInfo.Is(string) -> bool`                                             | instance | subtype test up the parent chain         |
|  [04]   | `GetConcreteClassesFrom(string, IfcSchemaVersions) -> IEnumerable<string>` | static   | abstract supertype → concrete leaves     |
|  [05]   | `TrySimplifyTopClasses(IEnumerable<string>, IfcSchemaVersions, out)`       | static   | concrete set → minimal supertype strings |
|  [06]   | `GetClassesByType(IIfcTypeConstraint) -> IEnumerable<ClassInfo>`           | instance | filter classes by a type constraint      |
|  [07]   | `GetAttributesByType(IIfcTypeConstraint) -> IEnumerable<AttributeInfo>`    | instance | filter attributes by a type constraint   |
|  [08]   | `GetAttributeClasses(string, bool) -> string[]`                            | instance | attribute → classes                      |
|  [09]   | `GetAttributeRelations(string) -> IEnumerable<ClassRelationInfo>`          | instance | attribute → relations                    |
|  [10]   | `GetAttributeNames() -> IEnumerable<string>`                               | instance | every attribute name                     |

[ENTRYPOINT_SCOPE]: IdsLib.IfcSchema — property and measure resolution (bare rows are `SchemaInfo` members)

| [INDEX] | [SURFACE]                                                                        | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :------------------------------------------------------------------------------- | :------- | :--------------------------------- |
|  [01]   | `PropertySetInfo.Get(IfcSchemaVersions, string, string) -> IPropertyTypeInfo?`   | static   | resolve a standard Pset property   |
|  [02]   | `PropertySetInfo.GetSchema(IfcSchemaVersions) -> IList<PropertySetInfo>?`        | static   | the full Pset/Qto catalog          |
|  [03]   | `TryParseIfcDataType(string, out IfcDataTypeInformation?, bool)`                 | static   | resolve a datatype or measure name |
|  [04]   | `GetMeasureInformation(IfcSchemaVersions) -> IEnumerable<IfcMeasureInformation>` | static   | measure → SI-unit metadata         |
|  [05]   | `TryGetMeasureInformation(string, out IfcMeasureInformation?)`                   | static   | try-resolve a measure by name      |
|  [06]   | `AllMeasureInformation -> IEnumerable<IfcMeasureInformation>`                    | property | every measure's SI metadata        |
|  [07]   | `AllConcreteClasses -> IEnumerable<IfcClassInformation>`                         | property | every concrete IFC class           |
|  [08]   | `AllAttributes -> IEnumerable<IfcAttributeInformation>`                          | property | every attribute                    |
|  [09]   | `AllDataTypes -> IEnumerable<IfcDataTypeInformation>`                            | property | every defined data type            |
|  [10]   | `StandardConversionUnits -> IEnumerable<IfcConversionUnitInformation>`           | property | the standard conversion units      |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Audit.RunAsync` returns `Status` flags — `Ok`(0) passes, any error flag (or `IdsStructureWarning` when `XmlWarningAction` escalates it) rejects; `OmitIdsSchemaAudit`/`OmitIdsContentAudit` drop the XSD vs implementation-agreement legs and `SchemaProvider` pins the IDS-XSD source.
- diagnostics flow through the caller-supplied `ILogger`; a list-backed `ILogger` captures the per-line `LogLevel`/message diagnostics for the `Review/validation#IDS_FACETS` `IdsAudit` receipt, which records `LibraryInformation.AssemblyVersion` so a stored audit reproduces against the exact engine build, and lowers onto `Fin<T>` via `BimFault`.
- `IdsLib.IfcSchema` resolves without an audit: `SchemaIfc{2x3,4,4x3}` give the class hierarchy, `ClassInfo.Is(name)`/`GetConcreteClassesFrom(top, version)` are the inheritance algebra, `PropertySetInfo.GetSchema(version)` the standard Pset/Qto catalog, and `GetMeasureInformation`/`IfcMeasureInformation.Exponents` the measure → SI-base dimensional metadata.

[STACKING]:
- `xbim-informationspecifications`(`.api/api-xbim-informationspecifications.md`): the in-memory IDS spec object model (`Xids` → `SpecificationsGroup` → `Specification` over the six `FacetBase` facets); `Review/validation#IDS_FACETS` authors and reads specs through `Xids.LoadBuildingSmartIDS`/`ExportBuildingSmartIDS` and audits the serialized `.ids` through `Audit.RunAsync`, sharing one schema vocabulary via `IfcSchemaVersionHelper` (`Xids` `IfcSchemaVersion` ↔ this engine's `IfcSchemaVersions`) — one rail, two roles, never a duplicated IDS reader.
- `unitsnet`(`libs/csharp/.api/api-unitsnet.md`): `SchemaInfo.GetMeasureInformation` supplies the SI-base dimensional truth (`IfcMeasureInformation.Exponents`, `SiUnitNameEnums`) that drives the `Semantics/properties.md` `QuantitySet.Derive` unit coercion — ids-lib owns the schema truth, UnitsNet the value conversion.
- `smino-bcf-toolkit`(`.api/api-smino-bcf-toolkit.md`): a failing `Audit.Status` and its per-line diagnostics mint a `Review/issues#BCF_ARCHIVE` `BcfTopic`/`BcfComment` per non-conformance, line/position provenance riding the topic free-text since BCF carries no structured position column.
- `geometrygym-ifc`(`.api/api-geometrygym-ifc.md`): ids-lib is the schema meta-model (what an `IfcWall` may carry), GeometryGym the instance graph (this `IfcWall`'s data); validation joins them by class name + attribute.
- within-lib facet fold: the `Review/validation#IDS_FACETS` `IdsFacet` `[Union]` folds the six IDS facets onto the Bim `Model/query#ELEMENT_SET` `ElementPredicate` algebra, resolving each facet's IFC references against the live graph (`GetConcreteClassesFrom`, `ClassInfo.PredefinedTypeValues`, `GetAttributeNames`) so a facet validates against the real IFC version, not a hard-coded class list.
- within-lib classification: `Semantics/classification.md` cross-checks resolved class codes against `AllConcreteClasses` and collapses a classified element set to its minimal IFC supertypes via `TrySimplifyTopClasses` for the `IfcRelAssociatesClassification` round-trip.

[LOCAL_ADMISSION]:
- IDS file audit enters through `Audit.RunAsync(stream, SingleAuditOptions, logger)`; the IDS-XSD source is selected once via the `SchemaProvider` option.
- IFC schema queries enter through the static `SchemaInfo.SchemaIfc*` graphs and `PropertySetInfo.GetSchema` — pure offline lookups with no audit dependency.

[RAIL_LAW]:
- Package: `ids-lib`
- Owns: buildingSMART-official `.ids` audit (XSD + implementation-agreement validation → `Status` flags) and the embedded offline IFC2x3/IFC4/IFC4x3 schema-reflection graph
- Accept: IDS conformance auditing, IFC schema/class/attribute/property/measure resolution, facet-against-schema validation
- Reject: authoring the IDS spec object model (`Xbim.InformationSpecifications`), the IFC instance graph (`GeometryGym`), value unit conversion (`UnitsNet`), BCF issue authoring (`Smino.Bcf.Toolkit`)
