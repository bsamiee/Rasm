# [RASM_BIM_API_IDS_LIB]

`ids-lib` is the buildingSMART-official IDS-file audit engine — it validates a `.ids`
Information Delivery Specification document against the IDS XSD schema(s) AND the
implementation agreements, emitting a `Status` flags result through an `ILogger`. It is also
the transitive floor of `Xbim.InformationSpecifications`. Beyond the audit, it embeds a
COMPLETE IFC schema-reflection library (`IdsLib.IfcSchema`): the IFC2x3/IFC4/IFC4x3 class
hierarchy graph, every class's attributes, the standard property-set/quantity definitions, and
the measure/unit metadata — a self-contained, offline IFC schema authority the BIM property,
quantity, and classification owners read directly, independent of the audit path.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ids-lib`
- package: `ids-lib`
- license: MIT
- assembly: `ids-lib`
- namespace: `IdsLib` (the `Audit` engine + options)
- namespace: `IdsLib.IfcSchema` (the embedded IFC schema graph)
- namespace: `IdsLib.SchemaProviders` (the IDS-XSD provider seam)
- namespace: `IdsLib.Messages` (the diagnostic message catalog)
- asset: net10.0; the net10.0 consumer binds the `lib/net10.0` asset; ships `ids-lib.xml`
- logging: diagnostics flow through `Microsoft.Extensions.Logging.ILogger?`
- rail: ids-validation

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: audit engine and result
- package: `ids-lib`
- namespace: `IdsLib`
- rail: ids-validation

| [INDEX] | [SYMBOL]                              | [CAPABILITY]                                                                                     |
| :-----: | :------------------------------------ | :----------------------------------------------------------------------------------------------- |
|  [01]   | `Audit`                               | `static`; the audit entrypoint (`Run`/`RunAsync` overloads in [03])                              |
|  [02]   | `Audit.Status`                        | `[Flags]` result enum (values in the fence below)                                                |
|  [03]   | `Audit.ISchemaProvider`               | IDS-XSD source seam; `Status GetSchemas(Stream source, ILogger?, out IEnumerable<XmlSchema>)`    |
|  [04]   | `AuditHelper`                         | the per-run audit state holder (`Options`, `SchemaStatus`, `ValidationReporter` XSD event sink)  |
|  [05]   | `AuditHelper.BufferedValidationIssue` | a captured issue: `Level`/`Message`/`Line`/`Position`/`Schema` (`Original` enum)                 |
|  [06]   | `LibraryInformation`                  | `static`; `AssemblyVersion`, `Commit`/`Sha`/`CommitDate`, `Isdirty` — the engine build           |
|  [07]   | `NullLogger.Instance`                 | `NullLogger: ILogger` (transitive) — the no-op sink when diagnostics are discarded, not buffered |

```csharp signature
[Flags] enum Audit.Status =                     // Ok=0 is the pass; any error flag (or IdsStructureWarning under strict) rejects
    Ok=0 | NotImplementedError | InvalidOptionsError | NotFoundError | IdsStructureError
    | IdsContentError | XsdSchemaError | UnhandledError | IdsStructureWarning;
```

[PUBLIC_TYPE_SCOPE]: audit options
- package: `ids-lib`
- namespace: `IdsLib`
- rail: ids-validation

| [INDEX] | [SYMBOL]                                  | [CAPABILITY]                                                                         |
| :-----: | :---------------------------------------- | :----------------------------------------------------------------------------------- |
|  [01]   | `AuditProcessOptions`                     | `SchemaProvider`, `OmitIds{Schema,Content}Audit`, `XmlWarningAction`                 |
|  [02]   | `SingleAuditOptions`                      | `AuditProcessOptions` + `IdsVersion` (default `Ids1_0`); the single-file argument    |
|  [03]   | `IBatchAuditOptions`                      | batch contract: `InputSource`/`InputExtension`/`SchemaFiles`                         |
|  [04]   | `IBatchAuditOptions`                      | + `AuditSchemaDefinition`/`OmitIdsContentAudit`(+`Pattern`)                          |
|  [05]   | `AuditProcessOptions.XmlWarningBehaviour` | how XSD warnings escalate: `ReportAsInformation` (default), promote to warning/error |

[PUBLIC_TYPE_SCOPE]: IDS-XSD schema providers
- package: `ids-lib`
- namespace: `IdsLib.SchemaProviders`
- rail: ids-validation

| [INDEX] | [SYMBOL]                       | [CAPABILITY]                                                                                            |
| :-----: | :----------------------------- | :------------------------------------------------------------------------------------------------------ |
|  [01]   | `SchemaProvider`               | `abstract` base for the IDS-XSD source providers                                                        |
|  [02]   | `SeekableStreamSchemaProvider` | the default: detects the `IdsVersion` from the source stream and supplies the matching embedded IDS XSD |
|  [03]   | `FixedVersionSchemaProvider`   | ctor `(IdsVersion)`; forces a specific IDS schema version regardless of the document declaration        |
|  [04]   | `FileBasedSchemaProvider`      | ctor `(IEnumerable<string> schemaFiles, ILogger?)`; audits against externally-supplied XSD files        |

[PUBLIC_TYPE_SCOPE]: IfcSchema — schema graph root
- package: `ids-lib`
- namespace: `IdsLib.IfcSchema`
- rail: ifc-schema

`SchemaInfo` is the embedded IFC schema authority — `SchemaIfc2x3`/`SchemaIfc4`/`SchemaIfc4x3` are ready-built static graphs, each an `IEnumerable<ClassInfo>` indexable by class name.

| [INDEX] | [SYMBOL]            | [CAPABILITY]                                                                                                   |
| :-----: | :------------------ | :------------------------------------------------------------------------------------------------------------- |
|  [01]   | `SchemaInfo`        | `IEnumerable<ClassInfo>`; static `SchemaIfc{2x3,4,4x3}`, `this[className]`, `Version`, `PropertySets` ([03])   |
|  [02]   | `IfcSchemaVersions` | `[Flags]` enum: `IfcNoVersion=0`, `Ifc2x3=1`, `Ifc4=2`, `Ifc4x3=4`, `IfcAllVersions=7`                         |
|  [03]   | `ClassInfo`         | one IFC entity (fields in the fence below); `Is(string)` subtype test                                          |
|  [04]   | `ClassType`         | enum: `Abstract`, `Concrete`, `Enumeration`                                                                    |
|  [05]   | `FunctionalType`    | the element/type/group functional role of a class                                                              |
|  [06]   | `AttributeInfo`     | `record`; `Name`, `ExpressType` (`ExpressDefinition`), `IsOptional`, `IsCollection`, `BaseType`, `XmlBaseType` |
|  [07]   | `ExpressDefinition` | the parsed EXPRESS type of an attribute (optional/collection/base-type facts)                                  |

[PUBLIC_TYPE_SCOPE]: IfcSchema — properties, measures, and units
- package: `ids-lib`
- namespace: `IdsLib.IfcSchema`
- rail: ifc-schema

| [INDEX] | [SYMBOL]                                  | [CAPABILITY]                                                                                |
| :-----: | :---------------------------------------- | :------------------------------------------------------------------------------------------ |
|  [01]   | `PropertySetInfo`                         | `Name`/`Properties`/`ApplicableClasses`/`PropertyNames`/`GetProperty(name)` (lists in [03]) |
|  [02]   | `IPropertyTypeInfo` / `NamedPropertyType` | a property definition inside a Pset (name + value type)                                     |
|  [03]   | `IfcMeasureInformation`                   | `record: IUnitInformation` (fields in the fence below)                                      |
|  [04]   | `IUnitInformation`                        | the unit-metadata contract                                                                  |
|  [05]   | `IfcConversionUnitInformation`            | the standard conversion-unit rows                                                           |
|  [06]   | `IfcDataTypeInformation`                  | a defined IFC data type (`TryParseIfcDataType`/`TryParseIfcMeasure` target)                 |
|  [07]   | `DimensionType`                           | the 7-vector SI dimensional exponents of a measure                                          |
|  [08]   | `SchemaInfo.ClassRelationInfo`            | a class-to-class relation row (`ClassName` + `ClassAttributeMode`)                          |

```csharp signature
record ClassInfo {                             // one IFC entity in the schema graph
    Name; ParentName; Parent; Type;           // Type is ClassType
    FunctionalType; SubClasses;
    PredefinedTypeValues; EnumerationValues; DirectAttributesInfo;
    NameSpace; MatchingConcreteClasses; RelationTypeClasses;
}
record IfcMeasureInformation : IUnitInformation {
    Id; IfcMeasure; Unit; UnitSymbol; Description; SiUnitNameEnums;
    IsBasicUnit; IsPureNumber; IsDirectSIUnit; Exponents;   // Exponents = dimensional 7-vector
}
```

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Audit — run
- package: `ids-lib`
- namespace: `IdsLib`
- rail: ids-validation

`Run` blocks on the async core; prefer `RunAsync`. The `ILogger` is the issue channel — attach an `ILogger` (or the buffered reporter) to collect the per-line diagnostics; the `Status` flags are the pass/fail discriminant.

| [INDEX] | [SURFACE]                        | [CALL_SHAPE]                                               | [CAPABILITY]                        |
| :-----: | :------------------------------- | :--------------------------------------------------------- | :---------------------------------- |
|  [01]   | `Audit.RunAsync`                 | `(Stream, SingleAuditOptions, ILogger?)` → `Task<Status>`  | audit one `.ids` stream (preferred) |
|  [02]   | `Audit.Run`                      | `(Stream, SingleAuditOptions, ILogger? = null)` → `Status` | synchronous single-file audit       |
|  [03]   | `Audit.Run`                      | `(IBatchAuditOptions, ILogger? = null)` → `Status`         | directory/glob batch audit          |
|  [04]   | `new SingleAuditOptions`         | `{ IdsVersion, SchemaProvider, OmitIdsContentAudit, … }`   | configure version/provider/scope    |
|  [05]   | `new FixedVersionSchemaProvider` | `(IdsVersion)`                                             | pin a fixed IDS-XSD version         |
|  [06]   | `new FileBasedSchemaProvider`    | `(IEnumerable<string>, ILogger?)`                          | audit against external XSD files    |

[ENTRYPOINT_SCOPE]: IfcSchema — class graph queries
- package: `ids-lib`
- namespace: `IdsLib.IfcSchema`
- rail: ifc-schema
- note: the `SchemaInfo` query methods are static; `SchemaIfc{4,4x3,2x3}` are static properties.

| [INDEX] | [SURFACE]                           | [CALL_SHAPE]                                          | [CAPABILITY]                           |
| :-----: | :---------------------------------- | :---------------------------------------------------- | :------------------------------------- |
|  [01]   | `SchemaInfo.SchemaIfc4`             | property → `SchemaInfo`                               | IFC4 schema graph                      |
|  [02]   | `SchemaInfo.SchemaIfc4x3`           | property → `SchemaInfo`                               | IFC4x3 schema graph                    |
|  [03]   | `SchemaInfo.SchemaIfc2x3`           | property → `SchemaInfo`                               | IFC2x3 schema graph                    |
|  [04]   | `SchemaInfo[className]`             | indexer → `ClassInfo?`                                | resolve a class by name                |
|  [05]   | `ClassInfo.Is`                      | `(string className)` → `bool`                         | subtype test up the parent chain       |
|  [06]   | `SchemaInfo.GetConcreteClassesFrom` | `(string, IfcSchemaVersions)` → `IEnumerable<string>` | abstract supertype → concrete leaves   |
|  [07]   | `SchemaInfo.TrySimplifyTopClasses`  | `(IEnumerable<string>, IfcSchemaVersions, out …)`     | concrete set → minimal supertypes      |
|  [08]   | `SchemaInfo.TrySearchTopClass`      | `(IEnumerable<string>, IfcSchemaVersions, out …)`     | search the minimal supertype           |
|  [09]   | `SchemaInfo.GetClassesByType`       | `(IIfcTypeConstraint)` → `IEnumerable<ClassInfo>`     | filter classes by a type constraint    |
|  [10]   | `SchemaInfo.GetAttributesByType`    | `(IIfcTypeConstraint)` → `AttributeInfo`              | filter attributes by a type constraint |
|  [11]   | `SchemaInfo.GetAttributeClasses`    | `(string attributeName, …)`                           | attribute → classes                    |
|  [12]   | `SchemaInfo.GetAttributeRelations`  | `(string attributeName, …)`                           | attribute → relations                  |
|  [13]   | `SchemaInfo.GetAttributeNames`      | `(string attributeName, …)`                           | attribute → names                      |

[ENTRYPOINT_SCOPE]: IfcSchema — property and measure resolution
- package: `ids-lib`
- namespace: `IdsLib.IfcSchema`
- rail: ifc-schema

| [INDEX] | [SURFACE]                             | [CALL_SHAPE]                                                 | [CAPABILITY]                     |
| :-----: | :------------------------------------ | :----------------------------------------------------------- | :------------------------------- |
|  [01]   | `PropertySetInfo.Get`                 | `(IfcSchemaVersions, string, string)` → `IPropertyTypeInfo?` | resolve a standard Pset property |
|  [02]   | `PropertySetInfo.GetSchema`           | `(IfcSchemaVersions)` → `IList<PropertySetInfo>?`            | the full Pset/Qto catalog        |
|  [03]   | `SchemaInfo.TryParseIfcMeasure`       | `(string value, out IfcDataTypeInformation?, bool strict)`   | resolve a measure name           |
|  [04]   | `SchemaInfo.TryParseIfcDataType`      | `(string value, out IfcDataTypeInformation?, bool strict)`   | resolve a datatype name          |
|  [05]   | `SchemaInfo.GetMeasureInformation`    | `(IfcSchemaVersions = IfcAllVersions)`                       | the measure→SI-unit metadata     |
|  [06]   | `SchemaInfo.TryGetMeasureInformation` | `(string, out …)`                                            | try-resolve a measure by name    |
|  [07]   | `SchemaInfo.AllConcreteClasses`       | `IEnumerable<string>`                                        | every concrete IFC class         |
|  [08]   | `SchemaInfo.AllAttributes`            | `IEnumerable<AttributeInfo>`                                 | every attribute                  |
|  [09]   | `SchemaInfo.AllDataTypes`             | `IEnumerable<…>`                                             | every defined data type          |
|  [10]   | `SchemaInfo.StandardConversionUnits`  | `IEnumerable<IfcConversionUnitInformation>`                  | the standard conversion units    |

## [04]-[IMPLEMENTATION_LAW]

[AUDIT_RAIL]:
- `var status = await Audit.RunAsync(idsStream, new SingleAuditOptions { IdsVersion = IdsVersion.Ids1_0 }, logger);` — `status == Audit.Status.Ok` is the pass; any error flag (or `IdsStructureWarning` under a strict policy) is the reject. Map the flags onto the `Review/validation#IDS_FACETS` `IdsAudit` receipt and lower onto `Fin<T>` via `BimFault`.
- the `ILogger` is the per-issue channel; route it to a buffering logger to capture `BufferedValidationIssue` rows (`Level`/`Message`/`Line`/`Position`/`Schema`) for the `IdsAudit` line-level diagnostics.
- scope knobs live on the options: `OmitIdsSchemaAudit`/`OmitIdsContentAudit` skip the XSD vs implementation-agreement legs; `XmlWarningAction` escalates XSD warnings; `SchemaProvider` pins the IDS-XSD source (`FixedVersionSchemaProvider` for an air-gapped fixed version).
- record `LibraryInformation.AssemblyVersion` on the audit receipt so a stored `IdsAudit` is reproducible against the exact engine build.

[SCHEMA_AUTHORITY]:
- `IdsLib.IfcSchema` is usable WITHOUT running an audit — it is a standalone offline IFC schema graph. `SchemaInfo.SchemaIfc4`/`SchemaIfc4x3`/`SchemaIfc2x3` give the class hierarchy; `ClassInfo.Is(name)` and `GetConcreteClassesFrom(topClass, version)` are the inheritance algebra; `PropertySetInfo.GetSchema(version)` is the standard Pset/Qto catalog.
- `SchemaInfo.GetMeasureInformation`/`TryGetMeasureInformation` (and `IfcMeasureInformation.Exponents`/`SiUnitNameEnums`) carry the measure→SI-base dimensional metadata the quantity-derivation unit coercion needs.

[INTEGRATION_STACK]:
- spec-model leg: this engine AUDITS a `.ids` file; the sibling `Xbim.InformationSpecifications` (`api-xbim-informationspecifications`, the same IDS_VALIDATION manifest cluster, floored by this very package) is the in-memory IDS SPEC OBJECT MODEL — an `Xids` document of `SpecificationsGroup`s of `Specification`s, each carrying an applicability and a requirement `FacetGroup` over the six `FacetBase` facets (`IfcTypeFacet`/`AttributeFacet`/`IfcPropertyFacet`/`IfcClassificationFacet`/`MaterialFacet`/`PartOfFacet`) whose match fields are `ValueConstraint`s — so `Review/validation#IDS_FACETS` AUTHORS/reads specs through `Xids.LoadBuildingSmartIDS`/`ExportBuildingSmartIDS` and AUDITS the serialized `.ids` through `Audit.RunAsync`, the two sharing one schema vocabulary through `IfcSchemaVersionHelper` (`Xids` `IfcSchemaVersion` ↔ this engine's `IfcSchemaVersions`); one rail, two roles, never a duplicated IDS reader.
- facet-to-predicate leg: the `Review/validation#IDS_FACETS` `IdsFacet` `[Union]` owner folds the IDS six facets onto the Bim `Model/query#ELEMENT_SET` `ElementPredicate` algebra; `SchemaInfo`/`ClassInfo` resolve the facet's IFC entity/predefined-type/attribute references against the real schema graph (`GetConcreteClassesFrom`, `ClassInfo.PredefinedTypeValues`, `GetAttributeNames`) so a facet is validated against the actual IFC version, not a hard-coded class list.
- property-template leg: `Semantics/properties.md`'s `PropertyKey` template + `QuantitySet.Derive` per-`IfcClass` base-quantity fold reads `PropertySetInfo.Get`/`GetSchema` for the standard Pset/Qto definitions and `SchemaInfo.GetMeasureInformation` for the SI-base dimensional metadata that drives the `UnitsNet` (`api-unitsnet`) SI coercion — ids-lib supplies the SCHEMA TRUTH, UnitsNet supplies the value conversion.
- classification leg: `Semantics/classification.md`'s bSDD axis cross-checks resolved class codes against `SchemaInfo`'s concrete-class roster; `TrySimplifyTopClasses` collapses a classified element set to its minimal IFC supertypes for the `IfcRelAssociatesClassification` round-trip.
- semantic-graph leg: the IFC entities ids-lib reflects are the same ones `GeometryGymIFC_Core` (`api-geometrygym-ifc`) materializes at ingest; ids-lib is the SCHEMA META-MODEL (what an `IfcWall` may have), GeometryGym is the INSTANCE graph (this `IfcWall`'s data) — validation joins them by class name + attribute.
- issues-board leg: a failing `Audit.Status` plus its captured `BufferedValidationIssue` rows (`Level`/`Message`/`Line`/`Position`/`Schema`) feed the `Review/coordination#COORDINATION` `ClashProposal`-style topic author, minting a `Review/issues#BCF_ARCHIVE` `BcfTopic`/`BcfComment` per non-conformance so an IDS audit failure becomes a coordination issue. BCF carries no structured line/position column, so the `Line`/`Position` provenance lands in the topic `Title`/`Description` or a `BcfComment` free-text body; the `.bcfzip` serialization that topic family round-trips through is the `api-smino-bcf-toolkit` codec capability. This audit-issue handoff is a SECOND IDS⇄BCF seam beside the shared `Model/query#ELEMENT_SET` `ElementPredicate` algebra — the requirement predicate is the read-time join, the audit-failure topic the write-time issue.

[LOCAL_ADMISSION]:
- IDS file audit enters through `Audit.RunAsync(stream, SingleAuditOptions, logger)`; the IDS-XSD source is selected once via the `SchemaProvider` option.
- IFC schema queries enter through the static `SchemaInfo.SchemaIfc4*` graphs and `PropertySetInfo.GetSchema`; these are pure offline lookups with no audit dependency.

[RAIL_LAW]:
- Package: `ids-lib`
- Owns: buildingSMART-official `.ids` file audit (XSD + implementation-agreement validation → `Status` flags) AND the embedded offline IFC2x3/IFC4/IFC4x3 schema-reflection graph (class hierarchy, attributes, standard Psets/Qtos, measures/units)
- Accept: IDS conformance auditing, IFC schema/class/attribute/property/measure resolution, facet-against-schema validation
- Reject: authoring/serializing the IDS spec object model (Xbim.InformationSpecifications), the IFC instance graph (GeometryGym), value unit conversion (UnitsNet), and BCF issue authoring (Smino.Bcf.Toolkit)
