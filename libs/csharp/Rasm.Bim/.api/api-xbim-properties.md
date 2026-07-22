# [RASM_BIM_API_XBIM_PROPERTIES]

`Xbim.Properties` is the authoritative buildingSMART `Pset_*`/`Qto_*` property-set and quantity-set TEMPLATE-DEFINITION dataset — the offline, schema-versioned catalogue of every standard IFC property set (its applicable classes, its properties, each property's data type and value-type kind) and quantity set (its base quantities and measurement method) that the `Semantics/properties#PROPERTY_TEMPLATES` owner reads to know what a `Pset_WallCommon` or `Qto_SlabBaseQuantities` is SUPPOSED to contain. It is a static template SOURCE, NOT an IFC model reader: the `Definitions<T>` catalogue (`T: QuantityPropertySetDef`) loads the bundled buildingSMART definitions per `IfcVersion`, indexes them by name, and exposes each `PropertySetDef`/`QtoSetDef` with its `ApplicableClasses`, `PropertyDefinitions`/`QuantityDefinitions`, and each definition's `PropertyType.PropertyValueType` value-type kind (the scalar IFC `DataTypeEnum` token) / `QtoTypeEnum`. It is pure-managed (AnyCPU IL) and carries NO IFC entity graph — it sits beside the GeometryGym `IfcSemanticModel` (`api-geometrygym-ifc`) as the template oracle, and beside `Xbim.InformationSpecifications` (`api-xbim-informationspecifications`) under the same CDDL-1.0 / `requireLicenseAcceptance` posture. It supplies the `PropertyKey` static anchors and the bSDD-union template the `Semantics/classification#BSDD_RESOLUTION` live dictionary enriches.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Xbim.Properties`
- package: `Xbim.Properties` (single assembly, version, direct pin)
- license: CDDL-1.0 (`xBimTeam/XbimWindowsUI`-lineage Pset dataset); `requireLicenseAcceptance=true` — the weak-copyleft file-level reciprocity is satisfied by referencing the unmodified NuGet binary, never vendoring or modifying its source (the same posture as `Xbim.InformationSpecifications`)
- assembly: `Xbim.Properties` → the `net10.0` consumer binds `lib/netstandard2.1/Xbim.Properties.dll` (the package also ships `lib/netstandard2.0`; only `netstandard2.1` is the bound asset; pure-managed AnyCPU IL, ALC-safe, no native asset)
- namespace: `Xbim.Properties` (the definition model), `No.Catenda.Peregrine.Model.Objects` + `.Pset` (a bundled IFD/bSDD-style object model, unused by the Bim rail)
- transitive: `Microsoft.CSharp` + `System.Data.DataSetExtensions` — both are net10 IN-BOX facades (the `dynamic` runtime binder and the `DataSet` LINQ extensions), so they pull NO `Xbim.Essentials` and stand up NO parallel IFC model; the central pins win resolution
- scope: the buildingSMART Pset/Qto TEMPLATE dataset + the load/query/save catalogue API; NOT an IFC model reader (no entity graph), NOT a property-VALUE store (it defines what a property IS, not an element's value), NOT a validation engine (`Xbim.InformationSpecifications` owns IDS)
- rail: `properties#PROPERTY_TEMPLATES` (the standard-Pset template source the typed property model resolves against)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the definition catalogue (`Xbim.Properties`)
- rail: properties#PROPERTY_TEMPLATES
- note: `Definitions<T>` is the generic catalogue (`T: QuantityPropertySetDef`); instantiate `Definitions<PropertySetDef>(Version)` for property sets and `Definitions<QtoSetDef>(Version)` for quantity sets, then `LoadAllDefault` to populate from the bundled buildingSMART data. Both `PropertySetDef` and `QtoSetDef` derive the shared `QuantityPropertySetDef` base, so the catalogue indexes either by name.

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY]    | [RAIL]                                                             |
| :-----: | :----------------------------------- | :--------------- | :----------------------------------------------------------------- |
|  [01]   | `Definitions<T>`                     | catalogue root   | load/index/query/save bundled templates per `Version`              |
|  [02]   | `QuantityPropertySetDef`             | set-def base     | the set-def base (members [02])                                    |
|  [03]   | `PropertySetDef`                     | property-set def | `: QuantityPropertySetDef` — property-set (members [03])           |
|  [04]   | `QtoSetDef`                          | quantity-set def | `: QuantityPropertySetDef` — quantity-set (members [04])           |
|  [05]   | `QuantityPropertySetDef.Definitions` | member sequence  | `IEnumerable<QuantityPropertyDef>` — the base indexer's members    |
|  [06]   | `ApplicableClass`                    | applicability    | `ClassName` + `PredefinedType` — the IFC entity the set applies to |
|  [07]   | `IfcVersion`                         | schema carrier   | `version` + `schema` — the schema label on each set definition     |
|  [08]   | `NameAlias`                          | localized alias  | per-language display-name/definition alias                         |

- [02]-[SETDEF]: `Name`, `Definition`, `IfcVersion`, `ApplicableClasses`, `DefinitionAliases`, `Definitions`, `this[name]`.
- [03]-[PSET]: `PropertyDefinitions: List<PropertyDef>`, `Applicability`, `ApplicableTypeValue`, `templatetype`, `IfdGuid`.
- [04]-[QTO]: `QuantityDefinitions: List<QtoDef>`, `MethodOfMeasurement`, `ApplicableTypeValue`.

[PUBLIC_TYPE_SCOPE]: property and quantity definitions (`Xbim.Properties`)
- rail: properties#PROPERTY_TEMPLATES
- note: a `PropertyDef` is one named property in a set carrying its `PropertyType` (which value-type kind: single/enumerated/bounded/list/table/reference/complex) and its `ValueDef` (the data type and allowed values); a `QtoDef` is one base quantity carrying its `QtoTypeEnum` (length/area/volume/weight/count/time).

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY]       | [RAIL]                                                        |
| :-----: | :---------------------------------------------- | :------------------ | :------------------------------------------------------------ |
|  [01]   | `QuantityPropertyDef`                           | def base            | `Name`/`Definition`/`NameAliases`/`DefinitionAliases`         |
|  [02]   | `PropertyDef`                                   | property def        | `: QuantityPropertyDef` — `PropertyType`/`ValueDef`/`IfdGuid` |
|  [03]   | `QtoDef`                                        | quantity def        | `: QuantityPropertyDef` — `QuantityType: QtoTypeEnum`         |
|  [04]   | `PropertyType`                                  | value-type kind     | `PropertyValueType: IPropertyValueType` wrapper               |
|  [05]   | `IPropertyValueType`                            | value-type contract | the value-type kind interface                                 |
|  [06]   | `TypeSimpleProperty` / `TypeComplexProperty`    | value-type contract | the simple/complex split                                      |
|  [07]   | `TypeProperty*` value-type cases (6)            | value-type case     | the six IFC property value-type kinds (roster [07])           |
|  [08]   | `DataType` / `DataTypeEnum`                     | data-type axis      | IFC value data type: `IfcLabel`/`IfcReal`/`IfcBoolean`/…      |
|  [09]   | `ValueDef` + value catalogue                    | value catalogue     | allowed-value/range/enum-list/constant defs (roster [09])     |
|  [10]   | `UnitType` + unit axis                          | unit axis           | IFC unit + data/unit pairing (roster [10])                    |
|  [11]   | `QtoTypeEnum` / `templatetype` / `DataTypeEnum` | enum                | quantity/template/data-type axes (values [11])                |

- [07]-[CASES]: `TypePropertySingleValue`/`TypePropertyBoundedValue`/`TypePropertyEnumeratedValue`/`TypePropertyListValue`/`TypePropertyTableValue`/`TypePropertyReferenceValue` mirror `IfcPropertySingleValue`/`…BoundedValue`/`…EnumeratedValue`/`…ListValue`/`…TableValue`/`…ReferenceValue`.
- [09]-[VALUES]: `ValueDef`/`ValueRangeDef`/`Value`/`EnumList`/`ConstantDef`.
- [10]-[UNITS]: `UnitType`/`SimplePropertyUnitType`/`SimplePropertyDataType`.
- [11]-[QTO]: `Q_LENGTH`/`Q_AREA`/`Q_VOLUME`/`Q_WEIGHT`/`Q_COUNT`/`Q_TIME`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: catalogue load
- rail: properties#PROPERTY_TEMPLATES
- note: construct the typed catalogue for a `Version` (`IFC2x3`/`IFC4`/`IFC4x3`), then `LoadAllDefault` to populate from the bundled buildingSMART definitions; `LoadFromDirectory`/`Load(Stream)` ingest custom or updated definition files. The `IfcVersion` carrier (`version`+`schema`) is the per-set schema STAMP read back off a loaded definition, distinct from the `Version` constructor enum.

| [INDEX] | [SURFACE]                                                           | [ENTRY_FAMILY] | [RAIL]                                          |
| :-----: | :------------------------------------------------------------------ | :------------- | :---------------------------------------------- |
|  [01]   | `new Definitions<PropertySetDef>(Version)` / `<QtoSetDef>(Version)` | construct      | typed Pset/Qto catalogue per schema version     |
|  [02]   | `definitions.LoadAllDefault()`                                      | load           | populate from bundled buildingSMART definitions |
|  [03]   | `definitions.LoadIFC4COBie()` / `definitions.LoadIFC4AndCOBie()`    | load           | the IFC4 + COBie definition supersets           |
|  [04]   | `definitions.LoadFromDirectory(string directory, SearchOption)`     | load           | ingest a directory of definition files          |
|  [05]   | `definitions.Load(string)` / `(Stream)` / `(TextReader)`            | load           | ingest one definition file/stream               |

[ENTRYPOINT_SCOPE]: catalogue query
- rail: properties#PROPERTY_TEMPLATES
- note: index a set by name, enumerate the sets, or query across all properties; the member indexer on a set resolves one property/quantity by name.
- note: `GetAllProperties`/`GetPropertiesWhere` are generic `where TP: QuantityPropertyDef`.

| [INDEX] | [SURFACE]                                                 | [ENTRY_FAMILY] | [RAIL]                                                |
| :-----: | :-------------------------------------------------------- | :------------- | :---------------------------------------------------- |
|  [01]   | `definitions[string name]` → `T`                          | index          | resolve a set by name (null if absent)                |
|  [02]   | `definitions.DefinitionSets` → `IEnumerable<T>`           | enumerate      | every loaded set definition                           |
|  [03]   | `setDef[string name]` → `QuantityPropertyDef`             | index          | resolve one property/quantity within a set by name    |
|  [04]   | `setDef.Definitions` → `IEnumerable<QuantityPropertyDef>` | enumerate      | the set's `PropertyDefinitions`/`QuantityDefinitions` |
|  [05]   | `definitions.GetAllProperties<TP>(bool nested = true)`    | query          | every property/quantity across all sets               |
|  [06]   | `definitions.GetPropertiesWhere<TP>(Func<TP, bool>)`      | query          | filtered cross-set property/quantity query            |
|  [07]   | `setDef.ApplicableClasses` / `AddApplicableClass(...)`    | applicability  | the IFC classes a set applies to (read + author)      |

[ENTRYPOINT_SCOPE]: definition read and save
- rail: properties#PROPERTY_TEMPLATES
- note: read a property's data type and value-type kind to drive the typed property model; save authored/updated definitions back to disk.

| [INDEX] | [SURFACE]                                                   | [ENTRY_FAMILY] | [RAIL]                                              |
| :-----: | :---------------------------------------------------------- | :------------- | :-------------------------------------------------- |
|  [01]   | `propertyDef.PropertyType.PropertyValueType`                | read           | value-type kind + `DataTypeEnum` (rules [01])       |
|  [02]   | `propertyDef.ValueDef` → `ValueDef` (`[Obsolete]`)          | read           | `[Obsolete]` catalogue; no data type (values [02])  |
|  [03]   | `qtoDef.QuantityType` → `QtoTypeEnum`                       | read           | the base quantity kind (`QtoTypeEnum`, values [11]) |
|  [04]   | `setDef.IfcVersion` / `.Definition` / `.DefinitionAliases`  | read           | the set's schema, definition, and localized aliases |
|  [05]   | `definitions.Save(string, string)` / `Save(string, T pSet)` | save           | persist an authored/updated definition              |
|  [06]   | `definitions.SaveToDirectory(string directory)`             | save           | persist every definition to a directory             |

- [01]-[VALUE_TYPE]: the scalar `DataTypeEnum` token lives on the `TypePropertySingleValue`/`…BoundedValue`/`…ReferenceValue` `DataType.Type` + `TypeSimpleProperty.DataType.Type`; the composite enumerated/list/table/complex kinds carry no scalar token.
- [02]-[VALUEDEF]: read `ValueDef` for the allowed `Value`/`ValueRangeDef`/`EnumList` only, never as the data-type source — the scalar IFC type lives on the value-type kind above.

## [04]-[IMPLEMENTATION_LAW]

[DEFINITION_TOPOLOGY]:
- `Definitions<T>(Version version)` is the catalogue (`T: QuantityPropertySetDef`); `LoadAllDefault` populates it from the bundled buildingSMART data for that `Version`, then `definitions[name]` resolves a `PropertySetDef`/`QtoSetDef` by its `Pset_*`/`Qto_*` name and `DefinitionSets` enumerates them
- a `PropertySetDef: QuantityPropertySetDef` carries `Name` + `IfcVersion` + `ApplicableClasses` (each an `ApplicableClass { ClassName, PredefinedType }`) + `PropertyDefinitions: List<PropertyDef>`; a `QtoSetDef` carries `QuantityDefinitions: List<QtoDef>` + a `MethodOfMeasurement`
- a `PropertyDef: QuantityPropertyDef` carries its `PropertyType` (the value-type kind — `TypePropertySingleValue`/`TypePropertyEnumeratedValue`/`TypePropertyBoundedValue`/`TypePropertyListValue`/`TypePropertyTableValue`/`TypePropertyReferenceValue`/`TypeComplexProperty`), the value-type kind owning BOTH the discriminant AND the scalar IFC `DataTypeEnum` token (off the single/bounded/reference `DataType.Type` + `TypeSimpleProperty.DataType.Type`); its `ValueDef` is the `[Obsolete]`  min/max/default catalogue carrying allowed `Value`/`ValueRangeDef`/`EnumList` but NO data type; a `QtoDef` carries its `QtoTypeEnum` quantity kind
- the data type axis `DataTypeEnum` (`IfcLabel`/`IfcReal`/`IfcBoolean`/`IfcLengthMeasure`/`IfcAreaMeasure`/…) is what makes a property TYPED — a `Pset_WallCommon.FireRating` is declared `IfcLabel`, a `Pset_WallCommon.ThermalTransmittance` is `IfcThermalTransmittanceMeasure`; this is the template the typed property value must agree with
- this package carries NO IFC entity graph and NO property VALUES — it defines what `Pset_WallCommon` IS (its properties, their types), never what a specific wall's `FireRating` value is; the model under test is the GeometryGym `IfcSemanticModel` (`api-geometrygym-ifc`)

[LICENSE_LAW]:
- CDDL-1.0 with `requireLicenseAcceptance=true` — reference the unmodified NuGet binary; do NOT vendor or edit its source (the file-level weak-copyleft reciprocity attaches to modified source files only). The bundled `No.Catenda.Peregrine.*` IFD object model rides along under the same license but is UNUSED by the Bim rail (the bSDD seam is `Semantics/classification`'s live dictionary, not this bundled static IFD model)

[INTEGRATION_STACK]:
- properties owner: `Semantics/properties#PROPERTY_TEMPLATES` reads this catalogue as the OFFLINE authoritative template source — its `PropertyKey` `[SmartEnum<string>]` static anchors (`Pset_WallCommon`/`Pset_SlabCommon`/`Qto_*`) are the well-known set names, and the `Xbim.Properties` `PropertySetDef.PropertyDefinitions`/`QtoSetDef.QuantityDefinitions` supply each value's declared data type and value-type kind so a seam `PropertyValue.Measure` carries its IFC data type rather than a stringly-typed text; the `properties` page's `PropertyCatalog.LowerValue` resolves the scalar `DataTypeEnum` token off `propertyDef.PropertyType.PropertyValueType` (the `TypePropertySingleValue`/`…BoundedValue`/`…ReferenceValue` `DataType.Type` + `TypeSimpleProperty.DataType.Type`, since `ValueDef` is `[Obsolete]` and carries none) into the EXPECTED type the seam `Rasm.Element/Properties/property#PROPERTY_VALUE` arm is validated against, and `qtoDef.QuantityType` (`QtoTypeEnum`) into the seam `Rasm.Element/Properties/quantity#MEASURE_VALUE` `Dimension` `[ComplexValueObject]` (the retired `QuantityKind` enum replaced [H2])
- bSDD union: the `properties` page's `PropertyKey.Resolve(cls, schema, Option<BsddClass>)` UNIONS the live `Semantics/classification#BSDD_RESOLUTION` `BsddClass.Properties` dictionary rows OVER this offline catalogue (dictionary-wins on a `{Set}.{Code}` collision) — `Xbim.Properties` is the OFFLINE/canonical buildingSMART template (deterministic, schema-versioned, network-free) and bSDD is the LIVE dictionary that enriches/overrides it; the two stack as static-template + live-dictionary, the offline catalogue the always-available floor when bSDD is unreachable, never a second property store
- quantity seam: a `QtoDef.QuantityType` (`QtoTypeEnum.Q_LENGTH`/`Q_AREA`/`Q_VOLUME`/`Q_WEIGHT`/`Q_COUNT`/`Q_TIME`) maps through the `properties` `PropertyCatalog.DimensionOf` to the seam `Rasm.Element/Properties/quantity#MEASURE_VALUE` `Dimension` `[ComplexValueObject]` (the retired six-case `QuantityKind` enum replaced [H2]) and thence to the `UnitsNet` typed quantity (`libs/csharp/.api/api-unitsnet.md`) — `Qto_SlabBaseQuantities.NetVolume` is a `QtoTypeEnum.Q_VOLUME` → `Dimension.VolumeDim`, derived as a `UnitsNet` `Volume` from the kernel geometry and SI-coerced; the template declares the KIND, `UnitsNet` carries the dimensioned value, the kernel geometry supplies the magnitude (`Q_COUNT`/`Q_TIME` are not geometry-derivable, so `DimensionOf` yields None and the derivation skips)
- applicability seam: `PropertySetDef.ApplicableClasses` (`ApplicableClass { ClassName, PredefinedType }`) is the IFC entity/predefined-type the set applies to — the `properties` `PropertyKey` row's applicable `IfcClass`/`IfcDomain` is corroborated against this, so a `Pset_WallCommon` resolves only for a `Model/elements#IFC_CLASS` `IfcClass` whose name is in the applicable set (the seam `Rasm.Element` graph `Node.Object` the class stamps, never the retired `BimElement`)
- IDS seam: the `Review/validation#IDS_FACETS` Property facet (`api-xbim-informationspecifications` `IfcPropertyFacet`) names a `PropertySetName`/`PropertyName`/`DataType` — that `DataType` is the same IFC data type this catalogue declares for the property, so the IDS spec, the typed property model, and this template agree on the data type of `Pset_WallCommon.FireRating`; the three share the data-type vocabulary, none re-derives it
- round-trip oracle: the bundled definitions are the buildingSMART standard, so the GeometryGym `IfcRelDefinesByProperties`/`IfcElementQuantity` round-trip (`api-geometrygym-ifc`) emits properties whose set names and data types match this catalogue — `Xbim.Properties` is the template oracle the round-trip is validated against, never a parallel emitter

[LOCAL_ADMISSION]:
- the standard-Pset template source is `Definitions<PropertySetDef>(Version).LoadAllDefault` (and the `QtoSetDef` twin) loaded ONCE per schema and held as the frozen template catalogue; the `properties` owner reads `definitions.DefinitionSets`/`setDef[propertyName]` to resolve a property's declared value-type kind and scalar data-type token, never hand-coding a `Pset_*` property table
- a property's data type AND value-type kind both derive from the ONE `propertyDef.PropertyType.PropertyValueType` (the `properties` `PropertyCatalog.LowerValue` collapse): the scalar `DataTypeEnum` token off the `TypePropertySingleValue`/`TypePropertyBoundedValue`/`TypePropertyReferenceValue` `DataType.Type` + `TypeSimpleProperty.DataType.Type`, the value-type kind off the same discriminated wrapper — the typed seam `PropertyValue` arm is chosen from these, never a stringly-typed guess and never `propertyDef.ValueDef.DataType` (`ValueDef` is `[Obsolete]` and carries NO data type, only the auxiliary min/max/default/enum-list catalogue)
- the bSDD live dictionary (`Semantics/classification`) is unioned OVER this offline catalogue with dictionary-wins precedence; this package is the always-available canonical floor, not the sole source — a bSDD-declared property with no entry here still resolves from the dictionary
- this package is consulted as a TEMPLATE source only — an IFC entity reader, a property-value store, or an IDS validator sought from `Xbim.Properties` is the rejected form (GeometryGym owns the entity graph, the `properties` owner owns the value model, `Xbim.InformationSpecifications` owns IDS)

[RAIL_LAW]:
- Package: `Xbim.Properties` (, CDDL-1.0, `requireLicenseAcceptance`, pure-managed `lib/netstandard2.1` AnyCPU IL; `Microsoft.CSharp` + `System.Data.DataSetExtensions` net10 in-box facades, NO `Xbim.Essentials`)
- Owns: the authoritative buildingSMART `Pset_*`/`Qto_*` TEMPLATE dataset — the `Definitions<T>` catalogue, `PropertySetDef`/`QtoSetDef` with their applicable classes and member definitions, each `PropertyDef`'s `DataType`/value-type kind, each `QtoDef`'s `QtoTypeEnum`, and the load/index/query/save catalogue API, schema-versioned by `IfcVersion`/`Version`
- Accept: `Definitions<PropertySetDef>(Version).LoadAllDefault` (+ the `QtoSetDef` twin) as the frozen offline template source the `properties#PROPERTY_TEMPLATES` `PropertyKey` anchors read, the declared `DataTypeEnum` (off `PropertyType.PropertyValueType`, never the `[Obsolete]` `ValueDef`) / `QtoTypeEnum` lowered into the seam-owned typed `Rasm.Element/Properties/property#PROPERTY_VALUE` `PropertyValue` / `Rasm.Element/Properties/quantity#MEASURE_VALUE` `Dimension` model (the retired `QuantityKind` enum replaced [H2]), the `ApplicableClasses` corroborating the `PropertyKey` domain, and the catalogue unioned UNDER the bSDD live dictionary with dictionary-wins precedence
- Reject: a hand-coded `Pset_*` property table beside the `Definitions<T>` catalogue; treating this as an IFC entity reader (it has no graph), a property-value store (it defines templates, not values), or an IDS validator (`Xbim.InformationSpecifications` owns that); making this the SOLE property template when the bSDD live dictionary must win on a collision; vendoring or modifying the CDDL-1.0 source rather than referencing the binary; consuming the bundled `No.Catenda.Peregrine.*` IFD model (unused — the bSDD seam is the live dictionary)
