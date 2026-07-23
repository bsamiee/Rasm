# [RASM_BIM_API_XBIM_INFORMATIONSPECIFICATIONS]

`Xbim.InformationSpecifications` owns the buildingSMART IDS specification document model: the in-memory `Xids` graph that parses, authors, and round-trips IDS XML and native JSON, holding `SpecificationsGroup`s of `Specification`s that each pair an applicability and a requirement `FacetGroup` under an IDS `ICardinality`. Each facet is `: FacetBase, IFacet`, deciding a candidate value through the `ValueConstraint` engine. It carries no IFC entity graph, only the specification that validates one, and feeds the `validation#IDS_FACETS` rail; an IDS parse fault lifts to `BimFault`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Xbim.InformationSpecifications`
- package: `Xbim.InformationSpecifications` (CDDL-1.0)
- assembly: `Xbim.InformationSpecifications`; the `net10.0` consumer binds `lib/net8.0` (pure-managed AnyCPU IL, ALC-safe, no native asset)
- namespace: `Xbim.InformationSpecifications`, `.Helpers`, `.Helpers.Measures`, `.IO`, `.Cardinality`, `.Facets.buildingSMART`
- transitive: `ids-lib` (`api-ids-lib`, the IDS-file audit + `IfcSchema` metadata), `System.Text.Json` (native JSON IO), `System.IO.Compression.ZipFile` (the IDS `.zip` container)
- rail: `validation#IDS_FACETS`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: IDS document model

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]     | [CAPABILITY]                                                                       |
| :-----: | :----------------------- | :---------------- | :--------------------------------------------------------------------------------- |
|  [01]   | `Xids`                   | document root     | the IDS document — groups, load/save, buildingSMART round-trip                     |
|  [02]   | `SpecificationsGroup`    | spec group        | a named group of `Specification`s under an `Xids`                                  |
|  [03]   | `Specification`          | specification     | applicability `FacetGroup` + requirement `FacetGroup` + `Cardinality`              |
|  [04]   | `FacetGroup`             | facet set         | an `ObservableCollection<IFacet>` with a `FacetUse` role flag                      |
|  [05]   | `IFacet` / `FacetBase`   | facet contract    | `Short()` / `IsValid()` + `ApplicabilityDescription`/`RequirementDescription`      |
|  [06]   | `ISpecificationMetadata` | metadata contract | the `Provider`/`Consumers`/`Stages`/`Name` provenance an `Xids`/group/spec carries |
|  [07]   | `Project`                | project ref       | the optional IDS project/repository reference metadata                             |
|  [08]   | `FacetGroupRepository`   | facet store       | the de-duplicating facet repository an `Xids` owns                                 |

[PUBLIC_TYPE_SCOPE]: the IDS facets
- note: each facet is `: FacetBase, IFacet`, its match fields `ValueConstraint`s (or nullable strings); `IfcTypeFacet` through `PartOfFacet` are the six core IDS facets, `DocumentFacet`/`IfcRelationFacet` xbim extensions beyond the IDS mirror.

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]        | [CAPABILITY]                                                                       |
| :-----: | :----------------------- | :------------------- | :--------------------------------------------------------------------------------- |
|  [01]   | `IfcTypeFacet`           | entity facet         | `IfcType` + `PredefinedType` (`ValueConstraint?`) + `IncludeSubtypes`              |
|  [02]   | `AttributeFacet`         | attribute facet      | `AttributeName` + `AttributeValue` (`ValueConstraint?`)                            |
|  [03]   | `IfcPropertyFacet`       | property facet       | `PropertySetName`/`PropertyName`/`PropertyValue` (`ValueConstraint?`) + `DataType` |
|  [04]   | `IfcClassificationFacet` | classification facet | `ClassificationSystem`/`Identification` (`ValueConstraint?`) + `IncludeSubClasses` |
|  [05]   | `MaterialFacet`          | material facet       | `Value` (`ValueConstraint?`) — the material value matcher                          |
|  [06]   | `PartOfFacet`            | part-of facet        | `EntityType` (`IfcTypeFacet?`) + `EntityRelation` (`PartOfRelation` string)        |
|  [07]   | `DocumentFacet`          | document facet       | a document-reference value (extended)                                              |
|  [08]   | `IfcRelationFacet`       | relation facet       | `Source` (`FacetGroup?`) + `RelationType` relation kind (extended)                 |

[PUBLIC_TYPE_SCOPE]: value-constraint engine
- note: a facet field is a `ValueConstraint` holding `AcceptedValues: List<IValueConstraintComponent>`; `IsSatisfiedBy` decides a candidate satisfied if ANY component matches, never a stringly-typed compare.

| [INDEX] | [SYMBOL]                                   | [TYPE_FAMILY]      | [CAPABILITY]                                                        |
| :-----: | :----------------------------------------- | :----------------- | :------------------------------------------------------------------ |
|  [01]   | `ValueConstraint`                          | constraint root    | the value matcher — `AcceptedValues` + `BaseType` + `IsSatisfiedBy` |
|  [02]   | `IValueConstraintComponent`                | component contract | `IsSatisfiedBy(object, ValueConstraint, bool ignoreCase, ILogger)`  |
|  [03]   | `ExactConstraint`                          | exact match        | a literal value (`xs:enumeration`)                                  |
|  [04]   | `PatternConstraint`                        | regex match        | an XSD regular-expression pattern (`xs:pattern`)                    |
|  [05]   | `RangeConstraint`                          | range match        | inclusive/exclusive min/max (`xs:minInclusive`/`maxExclusive`/…)    |
|  [06]   | `StructureConstraint`                      | structure match    | length/format facets (`xs:length`/`minLength`/`maxLength`)          |
|  [07]   | `NetTypeName` / `ValueConstraint.BaseType` | type axis          | the IFC↔XSD↔.NET base-type the constraint coerces against           |

[PUBLIC_TYPE_SCOPE]: cardinality, schema, and measures
- note: `ICardinality` is the IDS occurrence rule the requirement fold checks; `IfcSchemaVersion` is the closed schema axis `IfcSchemaVersionHelper` bridges to `ids-lib`.

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]        | [CAPABILITY]                                                                |
| :-----: | :------------------------------ | :------------------- | :-------------------------------------------------------------------------- |
|  [01]   | `ICardinality`                  | cardinality contract | `ExpectsRequirements`/`AllowsRequirements` + `IsSatisfiedBy(int count)`     |
|  [02]   | `RequirementCardinalityOptions` | cardinality enum     | `.Cardinality`: `Expected`/`Optional`/`Prohibited`                          |
|  [03]   | `Cardinality.MinMaxCardinality` | cardinality impl     | bounded-count cardinality                                                   |
|  [04]   | `Cardinality.SimpleCardinality` | cardinality impl     | simple cardinality                                                          |
|  [05]   | `IfcSchemaVersion`              | schema enum          | `Undefined`/`IFC2X3`/`IFC4`/`IFC4X3`                                        |
|  [06]   | `IfcSchemaVersionHelper`        | schema bridge        | `ToIds`/`FromIds` ↔ `IdsLib.IfcSchema.IfcSchemaVersions`                    |
|  [07]   | `Helpers.Measures.MeasureUnit`  | measure helper       | the SI unit conversion — 7-fundamental dimensional exponents + ratio/offset |
|  [08]   | `XidsSettings`                  | serialization policy | export/load formatting via `Xids.Settings.PrettyPrint`                      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: document load and save
- note: every load/save/probe overload takes a trailing optional `ILogger? logger = null`.

| [INDEX] | [SURFACE]                                                        | [SHAPE]  | [CAPABILITY]                                  |
| :-----: | :--------------------------------------------------------------- | :------- | :-------------------------------------------- |
|  [01]   | `Xids.LoadBuildingSmartIDS(Stream)` / `(string)` / `(XElement)`  | static   | parses a buildingSMART IDS XML document       |
|  [02]   | `Xids.ExportBuildingSmartIDS(string)` / `(Stream)`               | instance | writes IDS XML/`.zip` → `ExportedFormat`      |
|  [03]   | `Xids.Load(FileInfo)` / `Xids.LoadFromJson(string sourceFile)`   | static   | native JSON / format-detecting load           |
|  [04]   | `Xids.LoadFromJsonAsync(Stream)` -> `Task<Xids?>`                | static   | awaitable native-JSON stream load             |
|  [05]   | `Xids.SaveAsJson(string destinationFile)` / `(Stream)`           | instance | writes the native JSON form                   |
|  [06]   | `Xids.CanLoad(FileInfo)` / `IsZipped(Stream)` / `HasData(Xids?)` | static   | format admissibility and content probes       |
|  [07]   | `Xids.AllSpecifications()`                                       | fold     | flatten groups → `IEnumerable<Specification>` |
|  [08]   | `Xids.Purge()`                                                   | instance | drop-empty cleanup                            |
|  [09]   | `Xids.AssemblyVersion`                                           | property | the DLL version stamped into json persistence |

[ENTRYPOINT_SCOPE]: specification authoring
- note: `PrepareSpecification` creates a `Specification` for one or more `IfcSchemaVersion`s, wiring it into the document graph; every overload takes trailing `FacetGroup? applicability = null, FacetGroup? requirement = null`.

| [INDEX] | [SURFACE]                                                            | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :------------------------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `Xids.PrepareSpecification(IfcSchemaVersion ifcVersion, …)`          | factory  | a spec for one schema version           |
|  [02]   | `Xids.PrepareSpecification(IEnumerable<IfcSchemaVersion>, …)`        | factory  | a spec spanning schema versions         |
|  [03]   | `Xids.PrepareSpecification(SpecificationsGroup destinationGroup, …)` | factory  | a spec into a named group               |
|  [04]   | `Specification.Applicability` / `.Requirement`: `FacetGroup`         | property | applicability + requirement facet sets  |
|  [05]   | `Specification.Cardinality`: `ICardinality` / `.IfcVersion`          | property | the occurrence rule + target schema set |
|  [06]   | `new FacetGroup(FacetGroupRepository repository)`                    | ctor     | bound to the document repository        |
|  [07]   | `FacetGroup.Facets`: `ObservableCollection<IFacet>`                  | property | the group's facet collection            |
|  [08]   | `FacetGroup.IsUsed(SpecificationsGroup, FacetUse mode)`              | instance | facet role lookup                       |
|  [09]   | `FacetGroup.GetRequirementCardinalityOption(IFacet, out …)`          | instance | per-facet cardinality lookup            |

[ENTRYPOINT_SCOPE]: value-constraint matching
- note: `ValueConstraint.IsSatisfiedBy` is the match the requirement fold runs per candidate; every match overload takes a trailing optional `ILogger? logger = null`.

| [INDEX] | [SURFACE]                                                                | [SHAPE]  | [CAPABILITY]                                 |
| :-----: | :----------------------------------------------------------------------- | :------- | :------------------------------------------- |
|  [01]   | `ValueConstraint.IsSatisfiedBy(object? candidateValue, bool ignoreCase)` | instance | true if any accepted component matches       |
|  [02]   | `ValueConstraint.IsSatisfiedIgnoringCaseBy(object? candidateValue)`      | instance | case-insensitive match                       |
|  [03]   | `new ValueConstraint(string)` / `(IEnumerable<string>)`                  | ctor     | an exact-value / value-set constraint        |
|  [04]   | `new ValueConstraint(NetTypeName, string)` / `(int)`                     | ctor     | a typed exact constraint                     |
|  [05]   | `ValueConstraint.CreatePattern(string pattern)`                          | factory  | a pattern constraint                         |
|  [06]   | `ValueConstraint.AddAccepted(IValueConstraintComponent)`                 | instance | add an accepted component                    |
|  [07]   | `new ExactConstraint(string)`                                            | ctor     | literal-value component                      |
|  [08]   | `new PatternConstraint(string pattern)`                                  | ctor     | XSD-regex component                          |
|  [09]   | `new RangeConstraint(min, minInclusive, max, maxInclusive)`              | ctor     | inclusive/exclusive range component          |
|  [10]   | `IValueConstraintComponent.IsSatisfiedBy(object, ValueConstraint, bool)` | instance | the per-component match the constraint folds |
|  [11]   | `ValueConstraint.TryGetNetType(string?, out NetTypeName)`                | static   | resolve the IFC↔.NET base type               |
|  [12]   | `ValueConstraint.ParseValue(string?, NetTypeName)`                       | static   | parse a value to the base type               |

[ENTRYPOINT_SCOPE]: facet construction and cardinality

| [INDEX] | [SURFACE]                                                       | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :-------------------------------------------------------------- | :------- | :--------------------------------------- |
|  [01]   | `new IfcTypeFacet { … }`                                        | ctor     | the entity/predefined-type facet         |
|  [02]   | `new IfcPropertyFacet { … }`                                    | ctor     | the property facet                       |
|  [03]   | `IFacet.Short()` / `IFacet.IsValid()`                           | instance | short label + validity                   |
|  [04]   | `FacetBase.ApplicabilityDescription` / `RequirementDescription` | property | the human-readable descriptions          |
|  [05]   | `ICardinality.IsSatisfiedBy(int count)`                         | instance | the occurrence-rule check                |
|  [06]   | `IfcSchemaVersionHelper.ToIds` / `.FromIds`                     | static   | the `api-ids-lib` schema-metadata bridge |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Xids` is the document root (`ISpecificationMetadata`): it owns `SpecificationsGroup`s of `Specification`s and a `FacetGroupRepository`, and round-trips IDS XML (`LoadBuildingSmartIDS`/`ExportBuildingSmartIDS` → `ExportedFormat.XML`/`ZIP`) and native JSON (`Load`/`SaveAsJson`).
- a `Specification` pairs an applicability `FacetGroup`, a requirement `FacetGroup`, an `ICardinality`, and a target `IfcVersion` set; `PrepareSpecification` is the authoring factory that wires it into the document graph.
- a `FacetGroup` is an `ObservableCollection<IFacet>` with a `FacetUse` role flag (`Applicability`/`Requirement`/`RelationSource`); each facet is `: FacetBase, IFacet` exposing `Short()`/`IsValid()` and the applicability/requirement descriptions.
- a `ValueConstraint` holds `AcceptedValues: List<IValueConstraintComponent>` and a `BaseType: NetTypeName`; `IsSatisfiedBy` returns true if ANY component (`ExactConstraint`/`PatternConstraint`/`RangeConstraint`/`StructureConstraint`) matches, coercing the candidate to the IFC/XSD base type (`TryGetNetType`/`ParseValue`) before comparing.
- cardinality is `ICardinality.IsSatisfiedBy(int count)` over `RequirementCardinalityOptions.Cardinality { Expected, Optional, Prohibited }` (`MinMaxCardinality`/`SimpleCardinality` impls).
- `IfcSchemaVersion { Undefined, IFC2X3, IFC4, IFC4X3 }` is the closed schema axis; `IfcSchemaVersionHelper.ToIds`/`.FromIds` is the explicit bridge to `IdsLib.IfcSchema.IfcSchemaVersions`, so the two packages agree on the schema vocabulary.

[STACKING]:
- `Review/validation#IDS_FACETS`(`api-xbim-informationspecifications`): composes `Xids.LoadBuildingSmartIDS` as its ONE ingress and `PrepareSpecification`+`ExportBuildingSmartIDS` as egress, lowering each `FacetBase` facet's `ValueConstraint` onto its closed `IdsFacet` `[Union]` (interior never sees an `Xbim` type) and raising it back for publish; each arm lowers to a `Model/query#ELEMENT_SET` `ElementPredicate`, so the `ValueConstraint` match over a candidate IFC value IS the predicate the set query reads.
- `ids-lib`(`.api/api-ids-lib`): `IdsLib.Audit` audits the IDS FILE (spec validity itself) while `Xids` authors/parses the spec MODEL; `IfcSchemaVersionHelper` converts the schema axis between the two — model + file-validator, never a duplicated IDS reader, the two audits orthogonal.
- `Semantics/classification#CLASSIFICATION_AXIS` + `Semantics/properties#PROPERTY_TEMPLATES`: `IfcType`/`IfcClassification`/`IfcProperty` facet fields resolve against the classification axis and keyed property store; `ValueConstraint` `DataType`/`NetTypeName` coercion reads the IFC measure metadata (`Helpers.Measures`) reconciled with the `UnitsNet` SI coercion the property store owns, never a re-derived measure table.
- `csharp:Compute/Runtime/codecs#TWO_HOP_TESSELLATION`: `Xids` authors the IDS XML (`ExportBuildingSmartIDS`) that crosses to the IfcOpenShell ifctester companion over that rpc; returned `IdsVerdict` rows reconcile against the in-process self-audit on the (GlobalId, `FacetKey`) axis — the companion is the conformance oracle.
- within-lib: every match/parse takes an optional `Microsoft.Extensions.Logging.ILogger`; the validation fold threads its rail logger so a parse diagnostic rides the structured log, and the parse fault lifts to `BimFault.ModelRejected` at the boundary, never a thrown exception.

[LOCAL_ADMISSION]:
- `Xids` is the one IDS document owner; the `validation#IDS_FACETS` page composes its load/author/facet/constraint surface rather than re-minting an IDS parser over BCL XML.
- `Parse` lowers the facets and `ValueConstraint` components ONCE onto the seam `Model/query#ELEMENT_SET` `ValueMatch` family (`TryGetNetType`/`ParseValue` coerce range literals at that boundary); the typed `ValueMatch` is the only in-process matcher.
- `IfcSchemaVersion` is the schema axis, bridged to `ids-lib` through `IfcSchemaVersionHelper`; a second schema-version enum beside the bridge is the rejected form.
- `api-ids-lib` `IdsLib.Audit` owns the IDS-FILE audit; the `validation#IDS_FACETS` fold over the query algebra owns the MODEL audit (elements vs the spec) — orthogonal, neither re-implements the other.
- this package carries no IFC entity graph — the model under test is the seam `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph` the `Projection/semantic#SEMANTIC_PROJECTOR` assembles.

[RAIL_LAW]:
- Package: `Xbim.InformationSpecifications` (CDDL-1.0)
- Owns: the buildingSMART IDS specification document model (`Xids`/`SpecificationsGroup`/`Specification`/`FacetGroup`), the facet types, the `ValueConstraint` value-matching engine, IDS cardinality, the schema axis, and the IDS XML/JSON round-trip
- Accept: an IDS document loaded/authored through `Xids`, its facets read as the closed `IdsFacet` union the `validation#IDS_FACETS` page lowers to the `ElementPredicate` algebra, its `ValueConstraint`s lowered at parse onto the seam `ValueMatch` family (range literals coerced through `TryGetNetType`/`ParseValue`), the schema bridged to `ids-lib` through `IfcSchemaVersionHelper`, and the authored XML reconciled against the ifctester oracle
- Reject: a hand-rolled IDS XSD parser beside `Xids.LoadBuildingSmartIDS`; a stringly-typed value compare beside `ValueConstraint`; a second schema-version enum beside `IfcSchemaVersionHelper`; an IFC entity-graph reader sought from this package; re-implementing the IDS-FILE audit `api-ids-lib` owns; vendoring or modifying the CDDL-1.0 source rather than referencing the binary
