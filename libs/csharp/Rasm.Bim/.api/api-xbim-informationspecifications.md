# [RASM_BIM_API_XBIM_INFORMATIONSPECIFICATIONS]

`Xbim.InformationSpecifications` is the buildingSMART IDS v1.0 specification document model — the in-memory `Xids` graph that parses, authors, and round-trips an IDS XML (or its native JSON) and that the `Review/validation#IDS_FACETS` owner reads to drive model validation. It owns the IDS object model: an `Xids` document holding `SpecificationsGroup`s of `Specification`s, each carrying an applicability `FacetGroup` and a requirement `FacetGroup` with an IDS `Cardinality`; the six closed facet types (`IfcTypeFacet`, `AttributeFacet`, `IfcPropertyFacet`, `IfcClassificationFacet`, `MaterialFacet`, `PartOfFacet`) all `: FacetBase, IFacet`; and the `ValueConstraint` value-matching engine (exact / pattern / range / structure components, IFC-datatype-aware) that decides whether a candidate value satisfies a facet. It is pure-managed (AnyCPU IL) and pairs with its transitive dependency `ids-lib` (`api-ids-lib`) — the buildingSMART-official IDS-file audit engine and the `IdsLib.IfcSchema` schema metadata `IfcSchemaVersionHelper` converts to and from. The IDS parse fault lifts to `BimFault` at the boundary.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Xbim.InformationSpecifications`
- package: `Xbim.InformationSpecifications` (single assembly, version 1.0.24, direct pin)
- license: CDDL-1.0 (`CBenghi/Xbim.Xids`); `requireLicenseAcceptance=true` — the weak-copyleft file-level reciprocity is satisfied by referencing the unmodified NuGet binary, never vendoring or modifying its source
- assembly: `Xbim.InformationSpecifications` → the `net10.0` consumer binds `lib/net8.0/Xbim.InformationSpecifications.dll` (the package also ships `lib/netstandard2.0`; only `net8.0` is the bound asset; pure-managed AnyCPU IL, ALC-safe, no native asset)
- namespace: `Xbim.InformationSpecifications`, `.Helpers`, `.Helpers.Measures`, `.IO`, `.Cardinality`, `.Facets.buildingSMART`
- transitive: `ids-lib` (`api-ids-lib`, the IDS-file audit + `IfcSchema` metadata; package floor 1.0.112, the central 1.0.121 pin wins resolution), `System.Text.Json` 8.0.5 (native JSON IO), `System.IO.Compression.ZipFile` (the IDS `.zip` container) — the central pins win resolution
- scope: the IDS v1.0 document model + the value-constraint matching engine; NOT an IFC model reader — it carries no entity graph, only the specification that validates one
- rail: `validation#IDS_FACETS` (the IDS spec model the audit fold reads)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: IDS document model
- rail: validation#IDS_FACETS

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]      | [RAIL]                                                       |
| :-----: | :------------------------ | :----------------- | :---------------------------------------------------------- |
|  [01]   | `Xids`                    | document root      | the IDS document — groups, load/save, buildingSMART round-trip |
|  [02]   | `SpecificationsGroup`     | spec group         | a named group of `Specification`s under an `Xids`           |
|  [03]   | `Specification`           | specification      | applicability `FacetGroup` + requirement `FacetGroup` + `Cardinality` |
|  [04]   | `FacetGroup`              | facet set          | an `ObservableCollection<IFacet>` with a `FacetUse` role flag |
|  [05]   | `IFacet` / `FacetBase`    | facet contract     | `Short()` / `IsValid()` + `ApplicabilityDescription`/`RequirementDescription` |
|  [06]   | `ISpecificationMetadata`  | metadata contract  | the `Provider`/`Consumers`/`Stages`/`Name` provenance an `Xids`/group/spec carries |
|  [07]   | `Project` / `IRepositoryRef` | project ref     | the optional IDS project/repository reference metadata      |
|  [08]   | `FacetGroupRepository`    | facet store        | the de-duplicating facet repository an `Xids` owns          |

[PUBLIC_TYPE_SCOPE]: the six IDS facets
- rail: validation#IDS_FACETS
- note: each is `: FacetBase, IFacet`; its match fields are `ValueConstraint`s (or nullable strings), so the `validation#IDS_FACETS` `IdsFacet` union lowers each to its `ElementPredicate` arm.

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]      | [RAIL]                                                       |
| :-----: | :------------------------- | :----------------- | :---------------------------------------------------------- |
|  [01]   | `IfcTypeFacet`             | entity facet       | `IfcType` + `PredefinedType` (`ValueConstraint`) + `IncludeSubtypes` |
|  [02]   | `AttributeFacet`           | attribute facet    | `AttributeName` + `AttributeValue` (`ValueConstraint`)      |
|  [03]   | `IfcPropertyFacet`         | property facet     | `PropertySetName` + `PropertyName` + `PropertyValue` (`ValueConstraint`) + `DataType` |
|  [04]   | `IfcClassificationFacet`   | classification facet | `ClassificationSystem` + `Identification` (both `ValueConstraint?`) + `IncludeSubClasses` |
|  [05]   | `MaterialFacet`            | material facet     | `Value` (`ValueConstraint?`) — the material value matcher   |
|  [06]   | `PartOfFacet`              | part-of facet      | `EntityType` (`IfcTypeFacet?`, the containing-entity facet whose `IfcType` the part-of lowers) + `EntityRelation` (the IFC relation kind) |
|  [07]   | `DocumentFacet`            | document facet     | a document reference value (an extended facet)              |
|  [08]   | `IfcRelationFacet`         | relation facet     | an IFC relation kind (`IfcRelationFacet.RelationType`)      |

[PUBLIC_TYPE_SCOPE]: value-constraint engine
- rail: validation#IDS_FACETS
- note: a facet field is a `ValueConstraint` holding `AcceptedValues : List<IValueConstraintComponent>`; the constraint is satisfied if ANY component matches, so a facet decides a candidate value through `IsSatisfiedBy`, never a stringly-typed compare.

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]      | [RAIL]                                                    |
| :-----: | :----------------------------- | :----------------- | :------------------------------------------------------- |
|  [01]   | `ValueConstraint`              | constraint root    | the value matcher — `AcceptedValues` + `BaseType` + `IsSatisfiedBy` |
|  [02]   | `IValueConstraintComponent`    | component contract | `IsSatisfiedBy(object, ValueConstraint, bool ignoreCase, ILogger)` |
|  [03]   | `ExactConstraint`              | exact match        | a literal value (`xs:enumeration`)                       |
|  [04]   | `PatternConstraint`            | regex match        | an XSD regular-expression pattern (`xs:pattern`)         |
|  [05]   | `RangeConstraint`              | range match        | inclusive/exclusive min/max (`xs:minInclusive`/`maxExclusive`/…) |
|  [06]   | `StructureConstraint`          | structure match    | length/format facets (`xs:length`/`minLength`/`maxLength`) |
|  [07]   | `NetTypeName` / `ValueConstraint.BaseType` | type axis | the IFC↔XSD↔.NET base-type the constraint coerces against |

[PUBLIC_TYPE_SCOPE]: cardinality, schema, and measures
- rail: validation#IDS_FACETS
- note: `ICardinality` is the IDS occurrence rule (required/optional/prohibited) the requirement fold checks; `IfcSchemaVersion` is the closed schema axis `IfcSchemaVersionHelper` bridges to `ids-lib`.

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY]      | [RAIL]                                                  |
| :-----: | :---------------------------------------- | :----------------- | :----------------------------------------------------- |
|  [01]   | `ICardinality`                            | cardinality contract | `ExpectsRequirements`/`AllowsRequirements` + `IsSatisfiedBy(int count)` |
|  [02]   | `RequirementCardinalityOptions` / `RequirementCardinalityOptions.Cardinality` | cardinality enum | `Required`/`Optional`/`Prohibited` |
|  [03]   | `Cardinality.MinMaxCardinality` / `Cardinality.SimpleCardinality` | cardinality impl | bounded-count vs simple cardinality                 |
|  [04]   | `IfcSchemaVersion`                        | schema enum        | `Undefined`/`IFC2X3`/`IFC4`/`IFC4X3`                   |
|  [05]   | `IfcSchemaVersionHelper`                  | schema bridge      | converts `IfcSchemaVersion` ↔ `IdsLib.IfcSchema.IfcSchemaVersions` |
|  [06]   | `Helpers.IfcMeasureInfo` / `Helpers.Measures.MeasureUnit` / `Helpers.Measures.UnitConversion` | measure helper | the IFC measure/unit metadata the property `DataType` resolves |
|  [07]   | `XidsSettings` / `Xids.PrettyOutput`      | serialization policy | the export formatting and load settings              |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: document load and save
- rail: validation#IDS_FACETS
- note: `Xids` is the document root; `LoadBuildingSmartIDS`/`ExportBuildingSmartIDS` is the IDS XML round-trip, `Load`/`SaveAsJson` the native JSON, and `CanLoad`/`IsZipped` the format probes.

| [INDEX] | [SURFACE]                                                                       | [ENTRY_FAMILY] | [RAIL]                                            |
| :-----: | :------------------------------------------------------------------------------ | :------------- | :----------------------------------------------- |
|  [01]   | `Xids.LoadBuildingSmartIDS(Stream stream, ILogger? logger = null)` / `(string fileName, …)` / `(XElement main, …)` | static load | parses a buildingSMART IDS XML document |
|  [02]   | `Xids.ImportBuildingSmartIDS(Stream stream, ILogger? logger = null)` (alias of load) | static load | the import-spelled IDS XML parse |
|  [03]   | `Xids.ExportBuildingSmartIDS(string destinationFileName, ILogger? logger = null)` / `(Stream, …)` → `ExportedFormat` | author | writes the IDS XML (or `.zip`), returns `XML`/`ZIP` |
|  [04]   | `Xids.Load(FileInfo sourceFile, ILogger? logger = null)` / `Xids.LoadFromJson(string sourceFile, …)` | static load | native JSON / format-detecting load |
|  [05]   | `Xids.SaveAsJson(string destinationFile, ILogger? logger = null)` / `(Stream, …)`  | save           | writes the native JSON form                       |
|  [06]   | `Xids.CanLoad(FileInfo sourceFile, ILogger? logger = null)` / `Xids.IsZipped(Stream, …)` / `Xids.HasData(Xids?)` | probe | format admissibility and content probes |
|  [07]   | `Xids.AllSpecifications()` / `Xids.FromStream(Stream s)` / `Xids.Purge()` / `Xids.AssemblyVersion` | document op | flatten every `SpecificationsGroup`'s `Specification`s (`IEnumerable<Specification>` the `Parse` fold maps), stream load, drop-empty cleanup, package version |

[ENTRYPOINT_SCOPE]: specification authoring
- rail: validation#IDS_FACETS
- note: `PrepareSpecification` is the canonical authoring factory — it creates a `Specification` for one or more `IfcSchemaVersion`s with optional applicability/requirement groups already attached to the document graph.

| [INDEX] | [SURFACE]                                                                                                  | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :-------------------------------------------------------------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `Xids.PrepareSpecification(IfcSchemaVersion ifcVersion, FacetGroup? applicability = null, FacetGroup? requirement = null)` | author | a spec for one schema version       |
|  [02]   | `Xids.PrepareSpecification(IEnumerable<IfcSchemaVersion> ifcVersion, FacetGroup? applicability, FacetGroup? requirement)`   | author | a spec spanning schema versions     |
|  [03]   | `Xids.PrepareSpecification(SpecificationsGroup destinationGroup, IfcSchemaVersion ifcVersion, …)`         | author         | a spec into a named group                    |
|  [04]   | `Specification.Applicability` / `Specification.Requirement` { get; set; } : `FacetGroup`                   | spec edit      | the applicability and requirement facet sets |
|  [05]   | `Specification.Cardinality` { get; set; } : `ICardinality` / `Specification.IfcVersion`                    | spec edit      | the occurrence rule and target schema set    |
|  [06]   | `new FacetGroup(FacetGroupRepository repository)` / `FacetGroup.Facets` : `ObservableCollection<IFacet>`   | facet set      | a facet group bound to the document repository |
|  [07]   | `FacetGroup.IsUsed(SpecificationsGroup, FacetUse mode)` / `GetRequirementCardinalityOption(IFacet, out …)` | facet query    | facet role and per-facet cardinality lookup  |

[ENTRYPOINT_SCOPE]: value-constraint matching
- rail: validation#IDS_FACETS
- note: `ValueConstraint.IsSatisfiedBy` is the match the requirement fold runs per candidate value; the constraint is built from typed components and an IFC/XSD base type.

| [INDEX] | [SURFACE]                                                                                       | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :---------------------------------------------------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `ValueConstraint.IsSatisfiedBy(object? candidateValue, bool ignoreCase, ILogger? logger = null)` | match          | true if any accepted component matches       |
|  [02]   | `ValueConstraint.IsSatisfiedIgnoringCaseBy(object? candidateValue, ILogger? logger = null)`      | match          | case-insensitive match                       |
|  [03]   | `new ValueConstraint(string value)` / `new ValueConstraint(IEnumerable<string> stringValues)`    | ctor           | an exact-value / value-set constraint        |
|  [04]   | `new ValueConstraint(NetTypeName valueType, string value)` / `new ValueConstraint(int value)`    | ctor           | a typed exact constraint                     |
|  [05]   | `ValueConstraint.CreatePattern(string pattern)` / `ValueConstraint.AddAccepted(IValueConstraintComponent)` | build | a pattern constraint / add a component  |
|  [06]   | `new ExactConstraint(string)` / `new PatternConstraint(string pattern)` / `new RangeConstraint(string? min, bool minInclusive, string? max, bool maxInclusive)` | ctor | the typed constraint components |
|  [07]   | `IValueConstraintComponent.IsSatisfiedBy(object, ValueConstraint context, bool ignoreCase, ILogger?)` | match | the per-component match the constraint folds over |
|  [08]   | `ValueConstraint.TryGetNetType(string? ifcDataTypeName, out NetTypeName)` / `ValueConstraint.ParseValue(string?, NetTypeName)` | type coerce | IFC-datatype-aware value parsing |

[ENTRYPOINT_SCOPE]: facet construction and cardinality
- rail: validation#IDS_FACETS
- note: a facet's match fields are `ValueConstraint`s; the requirement cardinality is checked through `ICardinality.IsSatisfiedBy(int count)`.

| [INDEX] | [SURFACE]                                                                       | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :------------------------------------------------------------------------------ | :------------- | :------------------------------------------- |
|  [01]   | `IfcTypeFacet { IfcType, PredefinedType : ValueConstraint?, IncludeSubtypes : bool }` | facet build | the entity/predefined-type facet         |
|  [02]   | `IfcPropertyFacet { PropertySetName, PropertyName, PropertyValue : ValueConstraint?, DataType : string? }` | facet build | the property facet            |
|  [03]   | `IFacet.Short()` / `IFacet.IsValid()` / `FacetBase.ApplicabilityDescription` / `RequirementDescription` | facet read | the human-readable + validity surface |
|  [04]   | `ICardinality.IsSatisfiedBy(int count)` / `ExpectsRequirements` / `AllowsRequirements` | cardinality | the occurrence-rule check                |
|  [05]   | `IfcSchemaVersionHelper` (`IfcSchemaVersion` ↔ `IdsLib.IfcSchema.IfcSchemaVersions`) | schema bridge | the `api-ids-lib` schema-metadata seam   |

## [04]-[IMPLEMENTATION_LAW]

[SPEC_TOPOLOGY]:
- `Xids` is the document root (`ISpecificationMetadata`): it owns `SpecificationsGroup`s of `Specification`s and a `FacetGroupRepository`; it loads/saves IDS XML (`LoadBuildingSmartIDS`/`ExportBuildingSmartIDS`, returning `ExportedFormat.XML`/`ZIP`) and native JSON (`Load`/`SaveAsJson`)
- a `Specification` carries an applicability `FacetGroup`, a requirement `FacetGroup`, an `ICardinality`, and the target `IfcVersion` set; `PrepareSpecification(IfcSchemaVersion, applicability, requirement)` is the authoring factory that wires it into the document graph
- a `FacetGroup` is an `ObservableCollection<IFacet>` with a `FacetUse` role flag (`Applicability`/`Requirement`/`RelationSource`); the six facets are all `: FacetBase, IFacet` (`IfcTypeFacet`, `AttributeFacet`, `IfcPropertyFacet`, `IfcClassificationFacet`, `MaterialFacet`, `PartOfFacet`), each exposing `Short()`/`IsValid()` and the applicability/requirement descriptions
- a facet's match fields are `ValueConstraint`s; `ValueConstraint` holds `AcceptedValues : List<IValueConstraintComponent>` and a `BaseType : NetTypeName`, and `IsSatisfiedBy(object, bool ignoreCase, ILogger)` returns true if ANY component matches — the components are `ExactConstraint` (literal), `PatternConstraint` (XSD regex), `RangeConstraint` (inclusive/exclusive min/max), and `StructureConstraint` (length/format), and the engine coerces the candidate to the IFC/XSD base type (`TryGetNetType`/`ParseValue`) before comparing
- cardinality is `ICardinality.IsSatisfiedBy(int count)` over `RequirementCardinalityOptions.Cardinality { Required, Optional, Prohibited }` (with `MinMaxCardinality`/`SimpleCardinality` impls)
- `IfcSchemaVersion { Undefined, IFC2X3, IFC4, IFC4X3 }` is the closed schema axis; `IfcSchemaVersionHelper` is the explicit bridge to `IdsLib.IfcSchema.IfcSchemaVersions`, so the two packages agree on the schema vocabulary

[INTEGRATION_STACK]:
- validation owner: `Review/validation#IDS_FACETS` reads this model — its `IdsFacet` `[Union]` (Entity/Attribute/Property/Classification/Material/PartOf) is the closed mirror of the six `FacetBase` facets, and each arm lowers to a `Model/query#ELEMENT_SET` `ElementPredicate` arm, so the value-matching the `ValueConstraint` engine performs over a candidate IFC value is the same predicate the set query reads — the validation predicate IS the query predicate
- IDS round-trip: `Xids.LoadBuildingSmartIDS`/`ExportBuildingSmartIDS` is the IDS XML ingest/author the `validation#IDS_FACETS` `IdsSpecification.Parse`/`Author` must compose — the package owns the buildingSMART IDS v1.0 schema binding, so a hand-rolled `System.Xml.Schema`/`System.Xml.Linq` IDS parser beside `Xids` is the rejected form. The current `Review/validation#IDS_FACETS` page still mints `IdsSpecification.Parse` over `XmlReaderSettings.Schemas` and re-declares its own `IdsSpecification`/`IdsFacet` `[Union]` rather than composing `Xids` + the six `FacetBase` facets; reconciling that page onto `Xids` as the spec-model owner is a `plan-cs` integration, not a `.api` edit — this catalog records `Xids` as the owner the page must promote to.
- ids-lib seam: the transitive `api-ids-lib` `IdsLib.Audit` is the buildingSMART-official IDS-FILE audit (it validates the IDS document itself against the IDS rules); `IfcSchemaVersionHelper` converts the schema axis between the two packages, so `Xids` authors/parses the spec and `ids-lib` audits the spec file — the two stack as model + file-validator, never a duplicated IDS reader
- schema/classification/property: the `IfcTypeFacet`/`IfcClassificationFacet`/`IfcPropertyFacet` match fields resolve against the `Semantics/classification#CLASSIFICATION_AXIS` axis and the `Semantics/properties#PROPERTY_TEMPLATES` keyed store the `validation#IDS_FACETS` fold reads as settled vocabulary — the `ValueConstraint` `DataType`/`NetTypeName` coercion uses the IFC measure metadata (`Helpers.Measures`), reconciled with the `UnitsNet` SI coercion the property store owns, never a re-derived measure table
- cross-tool oracle: the authored IDS XML (`ExportBuildingSmartIDS`) crosses to the IfcOpenShell ifctester companion over the `csharp:Compute/Runtime/codecs#TWO_HOP_TESSELLATION` rpc; the returned `IdsVerdict` rows reconcile against the in-process self-audit on the (GlobalId, `FacetKey`) axis — `Xids` is the authoring side of that seam, the companion is the conformance oracle
- logging: every match/parse takes an optional `Microsoft.Extensions.Logging.ILogger`; the validation fold threads its rail logger so an IDS parse diagnostic rides the structured log, not a thrown exception (the parse fault lifts to `BimFault.ModelRejected` at the boundary)

[LOCAL_ADMISSION]:
- `Xids` is the one IDS document owner; the `validation#IDS_FACETS` page composes its load/author/facet/constraint surface rather than re-minting an IDS parser over BCL XML
- the six facets and the `ValueConstraint` components are a closed set the `IdsFacet` union mirrors; a candidate IFC value is matched through `ValueConstraint.IsSatisfiedBy` (typed, IFC-datatype-aware), never a `String.Equals` compare
- the schema axis is `IfcSchemaVersion` bridged to `ids-lib` through `IfcSchemaVersionHelper`; a second schema-version enum beside the bridge is the rejected form
- the IDS-FILE audit (the spec validity itself) is `api-ids-lib`'s `IdsLib.Audit`, and the MODEL audit (elements vs the spec) is the `validation#IDS_FACETS` fold over the query algebra — the two are orthogonal and neither re-implements the other
- this package carries no IFC entity graph — the model under test is the `Model/elements#ELEMENT_MODEL` `BimModel` (from GeometryGym ingest), and an IFC reader is never sought here

[RAIL_LAW]:
- Package: `Xbim.InformationSpecifications` (1.0.24, CDDL-1.0, `requireLicenseAcceptance`, pure-managed `lib/net8.0` AnyCPU IL; `ids-lib` 1.0.121 + `System.Text.Json` transitive)
- Owns: the buildingSMART IDS v1.0 specification document model (`Xids`/`SpecificationsGroup`/`Specification`/`FacetGroup`), the six facet types, the `ValueConstraint` value-matching engine, IDS cardinality, the schema axis, and the IDS XML/JSON round-trip
- Accept: an IDS document loaded/authored through `Xids`, its facets read as the closed `IdsFacet` union the `validation#IDS_FACETS` page lowers to the `ElementPredicate` algebra, candidate values matched through `ValueConstraint.IsSatisfiedBy`, the schema bridged to `ids-lib` through `IfcSchemaVersionHelper`, and the authored XML reconciled against the ifctester oracle
- Reject: a hand-rolled IDS XSD parser beside `Xids.LoadBuildingSmartIDS`; a stringly-typed value compare beside `ValueConstraint`; a second schema-version enum beside `IfcSchemaVersionHelper`; an IFC entity-graph reader sought from this package (it has none); re-implementing the IDS-FILE audit `api-ids-lib` owns; vendoring or modifying the CDDL-1.0 source rather than referencing the binary
