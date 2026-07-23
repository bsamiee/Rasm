# [RASM_BIM_API_XBIM_PROPERTIES]

`Xbim.Properties` owns the offline, schema-versioned buildingSMART template dataset — every `Pset_*` and `Qto_*` set with its applicable classes, member definitions, and each definition's value-type kind and scalar `DataTypeEnum`. A `Definitions<T>` catalogue loads the bundled definitions per `IfcVersion`, resolving a `PropertySetDef`/`QtoSetDef` by name; it holds no entity graph and no values, defining what a `Pset_WallCommon` IS, never a wall's `FireRating`. It feeds the `properties` owner as the network-free floor beneath live bSDD, GeometryGym owning the entity graph it lacks.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Xbim.Properties`
- package: `Xbim.Properties`
- license: CDDL-1.0 (`xBimTeam` Pset dataset), `requireLicenseAcceptance=true` — the file-level weak-copyleft reciprocity is satisfied by referencing the unmodified NuGet binary, never vendoring or editing its source.
- assembly: `Xbim.Properties` — the `net10.0` consumer binds `lib/netstandard2.1/Xbim.Properties.dll`; pure-managed AnyCPU IL, ALC-safe, no native asset.
- namespace: `Xbim.Properties` (the definition model); the bundled `No.Catenda.Peregrine.Model.Objects`/`.Pset` IFD object model rides along unused.
- transitive: `Microsoft.CSharp` and `System.Data.DataSetExtensions` are net10 in-box facades (the `dynamic` binder and `DataSet` LINQ extensions), pulling no `Xbim.Essentials` and standing up no parallel IFC model.
- rail: `properties#PROPERTY_TEMPLATES` — the standard-Pset template source the typed property model resolves against.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the definition catalogue and its property/quantity definitions

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                                                                                |
| :-----: | :----------------------- | :------------ | :------------------------------------------------------------------------------------------ |
|  [01]   | `Definitions<T>`         | class         | catalogue root (`T: QuantityPropertySetDef`); load/index/query/save per `Version`           |
|  [02]   | `QuantityPropertySetDef` | class         | set-def base — `Name` `Definition` `IfcVersion` `ApplicableClasses` `Definitions`           |
|  [03]   | `PropertySetDef`         | class         | `: QuantityPropertySetDef` — `PropertyDefinitions` `Applicability` `templatetype` `IfdGuid` |
|  [04]   | `QtoSetDef`              | class         | `: QuantityPropertySetDef` — `QuantityDefinitions` `MethodOfMeasurement`                    |
|  [05]   | `ApplicableClass`        | class         | `ClassName` + `PredefinedType` — the IFC entity a set applies to                            |
|  [06]   | `IfcVersion`             | class         | `version` + `schema` — the per-set schema stamp                                             |
|  [07]   | `NameAlias`              | class         | per-language display-name/definition alias                                                  |
|  [08]   | `QuantityPropertyDef`    | class         | def base — `Name` `Definition` `NameAliases` `DefinitionAliases`                            |
|  [09]   | `PropertyDef`            | class         | `: QuantityPropertyDef` — `PropertyType` `IfdGuid`                                          |
|  [10]   | `QtoDef`                 | class         | `: QuantityPropertyDef` — `QuantityType: QtoTypeEnum`                                       |
|  [11]   | `PropertyType`           | class         | `PropertyValueType: IPropertyValueType` — the value-type kind wrapper                       |
|  [12]   | `IPropertyValueType`     | interface     | the value-type kind contract                                                                |
|  [13]   | `DataType`               | class         | IFC value data type — `DataType.Type` is `DataTypeEnum?`                                    |

[ValueTypeKind]: `TypeSimpleProperty` `TypeComplexProperty` `TypePropertySingleValue` `TypePropertyBoundedValue` `TypePropertyEnumeratedValue` `TypePropertyListValue` `TypePropertyTableValue` `TypePropertyReferenceValue`
[ValueCatalogue]: `ValueDef` `ValueRangeDef` `Value` `EnumList` `ConstantDef`
[UnitAxis]: `UnitType` `SimplePropertyUnitType` `SimplePropertyDataType`
[Enums]: `DataTypeEnum` `QtoTypeEnum` `templatetype`
[QtoTypeEnum]: `Q_LENGTH` `Q_AREA` `Q_VOLUME` `Q_WEIGHT` `Q_COUNT` `Q_TIME`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: catalogue load, query, definition read, and save

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]                                          |
| :-----: | :----------------------------------------------------------------- | :------- | :---------------------------------------------------- |
|  [01]   | `new Definitions<PropertySetDef>(Version)` / `<QtoSetDef>`         | ctor     | typed Pset/Qto catalogue per schema version           |
|  [02]   | `definitions.LoadAllDefault()`                                     | instance | populate from bundled buildingSMART definitions       |
|  [03]   | `definitions.LoadIFC4COBie()` / `LoadIFC4AndCOBie()`               | instance | the IFC4 and IFC4-plus-COBie definition supersets     |
|  [04]   | `definitions.LoadFromDirectory(string, SearchOption)`              | instance | ingest a directory of definition files                |
|  [05]   | `definitions.Load(string)` / `(Stream)` / `(TextReader)`           | instance | ingest one definition file, stream, or reader         |
|  [06]   | `definitions[string]` -> `T`                                       | property | resolve a set by name, null if absent                 |
|  [07]   | `definitions.DefinitionSets` -> `IEnumerable<T>`                   | property | every loaded set definition                           |
|  [08]   | `definitions.GetAllProperties<TP>(bool)`                           | instance | every property/quantity across all sets               |
|  [09]   | `definitions.GetPropertiesWhere<TP>(Func<TP,bool>, bool)`          | instance | filtered cross-set property/quantity query            |
|  [10]   | `definitions.Save(string, string)` / `(string, T)` / `(Stream, T)` | instance | persist an authored or updated definition             |
|  [11]   | `definitions.SaveToDirectory(string)`                              | instance | persist every definition to a directory               |
|  [12]   | `setDef[string]` -> `QuantityPropertyDef`                          | property | resolve one property/quantity within a set by name    |
|  [13]   | `setDef.Definitions` -> `IEnumerable<QuantityPropertyDef>`         | property | the set's `PropertyDefinitions`/`QuantityDefinitions` |
|  [14]   | `setDef.ApplicableClasses` -> `IEnumerable<ApplicableClass>`       | property | the IFC classes a set applies to                      |
|  [15]   | `setDef.AddApplicableClass(ApplicableClass)`                       | instance | author an applicable class onto a set                 |
|  [16]   | `propertyDef.PropertyType.PropertyValueType`                       | property | value-type kind and scalar `DataTypeEnum`             |
|  [17]   | `qtoDef.QuantityType` -> `QtoTypeEnum`                             | property | the base quantity kind                                |

- `PropertyDef.PropertyType.PropertyValueType`: a property's scalar `DataTypeEnum` lives on the single/bounded/reference `DataType.Type` and `TypeSimpleProperty.DataType.Type`; composite enumerated/list/table/complex kinds carry no scalar type, exposing the allowed-value catalogue instead — `TypePropertyEnumeratedValue.EnumList`/`ConstantList`, `TypePropertyBoundedValue.ValueRangeDef`, `TypePropertyListValue.ListValue`, `TypePropertyTableValue.DefiningValue`/`DefinedValue`.
- `setDef.IfcVersion`: (`version`+`schema`) the per-set schema stamp read off a loaded definition, distinct from the `Version` constructor enum (`IFC2x3`/`IFC4`/`IFC4x3`).

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every set, property, and quantity resolves by name off a `Definitions<T>` loaded once per `Version` — `definitions[name]` binds a set, `DefinitionSets` enumerates, `setDef[name]` binds one member.
- A property's declared type folds through the ONE `PropertyType.PropertyValueType`, a quantity through `QtoTypeEnum`; the `DataTypeEnum` axis (`IfcLabel`/`IfcReal`/`IfcLengthMeasure`/…) is what makes a property typed — the template the typed value must agree with.

[STACKING]:
- `Semantics/properties#PROPERTY_TEMPLATES`: reads this catalogue as the offline authoritative template source — its `PropertyKey` `[SmartEnum<string>]` anchors name the well-known `Pset_*`/`Qto_*` sets, and `PropertyCatalog.LowerValue` lowers the scalar `DataTypeEnum` off `PropertyType.PropertyValueType` into the seam-owned `Rasm.Element/Properties/property#PROPERTY_VALUE` `PropertyValue` type.
- `api-bsdd`(`.api/api-bsdd.md`): `PropertyKey.Resolve(cls, schema, Option<BsddClass>)` unions the live `Semantics/classification#BSDD_RESOLUTION` dictionary rows OVER this offline catalogue, dictionary-wins on a `{Set}.{Code}` collision; this catalogue is the always-available floor when bSDD is unreachable.
- `api-unitsnet`(`libs/csharp/.api/api-unitsnet.md`): `QtoDef.QuantityType` maps through `PropertyCatalog.DimensionOf` to the seam `Rasm.Element/Properties/quantity#MEASURE_VALUE` `Dimension` `[ComplexValueObject]` and thence the `UnitsNet` typed quantity — `Qto_SlabBaseQuantities.NetVolume` is `Q_VOLUME` -> `Dimension.VolumeDim` -> a `UnitsNet` `Volume`; `Q_COUNT`/`Q_TIME` are not geometry-derivable, so `DimensionOf` yields None.
- `api-geometrygym-ifc`(`.api/api-geometrygym-ifc.md`): the bundled definitions are the buildingSMART standard, so the GeometryGym `IfcRelDefinesByProperties`/`IfcElementQuantity` round-trip is validated against this template oracle; GeometryGym owns the entity graph this package lacks.
- `api-xbim-informationspecifications`(`.api/api-xbim-informationspecifications.md`): the `Review/validation#IDS_FACETS` `IfcPropertyFacet` `DataType` is the same IFC data type this catalogue declares, so the IDS spec, the typed property model, and this template share one data-type vocabulary.
- applicability: `PropertySetDef.ApplicableClasses` (`ApplicableClass { ClassName, PredefinedType }`) corroborates the `PropertyKey` domain, so a `Pset_WallCommon` resolves only for a seam `Rasm.Element` graph `Node.Object` whose stamped `Model/elements#IFC_CLASS` name is in the applicable set.

[LOCAL_ADMISSION]:
- `Definitions<PropertySetDef>(Version).LoadAllDefault` and its `QtoSetDef` twin admit the standard-Pset template, loaded once per schema and held frozen; the `properties` owner reads `DefinitionSets`/`setDef[name]` and resolves each declared type off `PropertyType.PropertyValueType`, never a hand-coded `Pset_*` table.
- bSDD (`Semantics/classification`) unions OVER this catalogue with dictionary-wins precedence; a bSDD-declared property absent here still resolves from the dictionary, so this package is the network-free floor, never the sole source.

[RAIL_LAW]:
- Package: `Xbim.Properties` (CDDL-1.0, `requireLicenseAcceptance`; pure-managed `lib/netstandard2.1` AnyCPU IL; `Microsoft.CSharp` and `System.Data.DataSetExtensions` in-box facades, no `Xbim.Essentials`)
- Owns: the authoritative buildingSMART `Pset_*`/`Qto_*` template dataset — the `Definitions<T>` catalogue, `PropertySetDef`/`QtoSetDef` with applicable classes and member definitions, each `PropertyDef`'s value-type kind and scalar `DataTypeEnum`, each `QtoDef`'s `QtoTypeEnum`, and the load/index/query/save API, schema-versioned by `IfcVersion`/`Version`
- Accept: `Definitions<PropertySetDef>(Version).LoadAllDefault` (plus the `QtoSetDef` twin) as the frozen offline source the `properties#PROPERTY_TEMPLATES` `PropertyKey` anchors read; the declared `DataTypeEnum` (off `PropertyType.PropertyValueType`) and `QtoTypeEnum` lowered into the seam-owned `PropertyValue`/`Dimension` model; the `ApplicableClasses` corroborating the `PropertyKey` domain; the catalogue unioned UNDER the bSDD live dictionary
- Reject: a hand-coded `Pset_*` table beside `Definitions<T>`; treating this as an IFC entity reader, a property-value store, or an IDS validator (`Xbim.InformationSpecifications` owns IDS); making it the sole template when bSDD must win a collision; vendoring or modifying the CDDL-1.0 source; consuming the bundled `No.Catenda.Peregrine.*` IFD model
