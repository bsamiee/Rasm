# [BIM_IDS]

The buildingSMART IDS v1.0 model-validation owner: one `IdsSpecification` record parsed from the `Xbim.InformationSpecifications` `Xids` document model, its six facets folded into one closed `IdsFacet` `[Union]` that lowers each applicability/requirement facet onto the `Model/query#ELEMENT_SET` `ElementPredicate` algebra for STRUCTURAL selection and decides each VALUE through the `ValueConstraint` typed-match engine, one `IdsAudit` deterministic model-audit receipt, and one `IdsFileAudit` over the buildingSMART-official `ids-lib` `Audit` for the IDS document's own validity. The validation predicate IS the query predicate — an IDS Entity/Attribute/Property/Classification/Material/PartOf facet folds to the same `ElementPredicate` arm the set query reads, never a second selection surface — while the value match is the `ValueConstraint.IsSatisfiedBy` exact/pattern/range/structure engine, never a `String.Equals`. The page composes the `Model/elements#ELEMENT_MODEL` `BimModel`, the `Model/query#ELEMENT_SET` algebra, the `Semantics/classification#CLASSIFICATION_AXIS` axis, the `Semantics/properties#PROPERTY_SETS` owner, and the `ids-lib` `IdsLib.IfcSchema` offline schema authority as settled vocabulary; the in-process fold gives an immediate self-audit and the IfcOpenShell ifctester companion gives the deterministic cross-tool audit matching the buildingSMART IDS audit test-suite. The page is HOST-LOCAL.

## [01]-[INDEX]

- [01]-[IDS_FACETS]: `IdsFacet` closed union (structural `ToPredicate` + typed `Satisfies` value match), `IdsSpecification`/`IdsRequirement` records, the `IdsSpecification.Parse` over `Xids.LoadBuildingSmartIDS`, the `IdsSpecification.AuditFile` over `ids-lib` `Audit`, the `IdsSchema` over `SchemaInfo`, the `IdsAudit` self-audit receipt, and the `IdsAudit.Reconcile` -> `IdsParity` ifctester-oracle cross-tool diff.

## [02]-[IDS_FACETS]

- Owner: `IdsSpecification` the IDS specification record carrying applicability and requirement facet sets and the cardinality; `IdsFacet` `[Union]` the closed facet family (Entity, Attribute, Property, Classification, Material, PartOf) each lowering to an `ElementPredicate` arm and projecting one canonical `FacetKey` join token; `IdsAudit` the deterministic per-specification receipt folding the matched/passed/failed element GlobalIds; `IdsVerdict` the per-(GlobalId, facet) oracle row the `csharp:Compute/Runtime/codecs#TWO_HOP_TESSELLATION` `IdsAuditRequest` companion-rpc leg projects back from the IfcOpenShell ifctester (the one shape on both ends of the seam); `IdsParity` the typed reconcile receipt `IdsAudit.Reconcile` folds, joining the C# self-audit against the ifctester oracle on the (GlobalId, `FacetKey`) axis and carrying the per-row divergence set — never a message diff.
- Entry: `IdsSpecification.Audit(BimModel model)` folds the applicability facets into one `ElementQuery` selecting the structurally-applicable element set then filtering by `Facet.Satisfies`, and folds each requirement facet into the `ValueConstraint`-decided pass/fail partition — `Fin<T>` aborts on a facet referencing an unknown classification system or property template (`Model/faults#FAULT_BAND` `BimFault.UnmappedClass`) or a malformed IDS payload (`BimFault.ModelRejected`), each lowered with `.ToError()`; `IdsSpecification.Parse(ReadOnlyMemory<byte> idsBytes)` admits an IDS XML document through `Xids.LoadBuildingSmartIDS` and projects every `Specification`'s applicability/requirement `FacetGroup` onto the `IdsFacet` union; `IdsSpecification.AuditFile(ReadOnlyMemory<byte> idsBytes)` validates the IDS document's own conformance through the `ids-lib` `Audit.Run` engine onto an `IdsFileAudit` Status receipt.
- Auto: `Audit` reads the applicability facet set, lowers each to its `ElementPredicate` arm through `Facet.ToPredicate` (the structural class/set/name selection), conjoins them through `ElementQuery.And`, runs `ElementSet.Query(model, applicability)`, then filters the result by `Facet.Satisfies` (the `ValueConstraint.IsSatisfiedBy` typed value match) for the applicable set; for each requirement facet it partitions the applicable set by `Facet.Satisfies` — an element failing a `Required` value constraint or matching a `Prohibited` one lands a failed GlobalId, the rest pass; the `Classification` facet's structural arm lowers to `ElementPredicate.ByClassification` reading the typed `BimElement.Classifications` `ClassificationRef` and its `Code` value match runs `ValueConstraint.IsSatisfiedBy` over the element's classification code, the `Property` facet to `ElementPredicate.ByProperty` over the `Semantics/properties#PROPERTY_SETS` keyed set with the `ValueConstraint` deciding the property value (pattern/range/exact), the `Entity` facet to `ElementPredicate.ByClass` with the predefined-type `ValueConstraint` refining, and the `Material`/`Attribute`/`PartOf` facets to their arms so the validation fold reuses the query algebra for selection and the `ValueConstraint` engine for value.
- Receipt: `IdsAudit` carries the specification name, the applicable element count, the passed and failed GlobalId sets, and the per-facet verdict so a requirement-driven exchange acceptance reads one typed receipt; `IdsAudit.Reconcile(Seq<IdsVerdict> oracle)` folds the companion ifctester projection (the `IdsVerdict` rows the `csharp:Compute/Runtime/codecs#TWO_HOP_TESSELLATION` leg returns) against the self-audit into an `IdsParity` receipt, joining each row on the (GlobalId, `IdsFacet.FacetKey`) axis so an element where the self-audit and the standards-conformant oracle disagree lands one typed `IdsParity.Row` divergence — `IdsParity.Conformant` is the cross-tool parity verdict, never a message diff.
- Packages: Xbim.InformationSpecifications, ids-lib, GeometryGymIFC_Core, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm
- Growth: a new IDS facet is one `IdsFacet` union arm lowering to its `ElementPredicate` arm for selection plus its `Candidate`/`Constraint` value match and projecting its `FacetKey` token; a new cardinality is one column on `IdsRequirement`; a new value-match modality is one `ValueConstraint` component the engine already folds (exact/pattern/range/structure); the cross-tool audit is one companion-rpc shape over the same `csharp:Compute/Runtime/codecs#TWO_HOP_TESSELLATION` companion pattern and one `IdsAudit.Reconcile` fold over the returned `IdsVerdict` rows; a new parity dimension is one column on `IdsParity.Row`; never a second validation predicate surface, never a second reconcile surface, never a hand-rolled IDS parser, and never a transport minted here.
- Boundary: the validation predicate IS the `Model/query#ELEMENT_SET` `ElementPredicate` for STRUCTURAL selection — an `IdsValidator`/`IdsRule` evaluation family or a second selection surface is the deleted form, the IDS fold folds to the query algebra verbatim — and the VALUE match is the `Xbim.InformationSpecifications` `ValueConstraint.IsSatisfiedBy` engine (the `ExactConstraint`/`PatternConstraint`/`RangeConstraint`/`StructureConstraint` components, IFC-datatype-aware), so a `String.Equals` value compare beside `ValueConstraint` is the deleted form; the IDS document parse is `Xids.LoadBuildingSmartIDS` (the package owns the buildingSMART IDS v1.0 schema binding) and a hand-rolled `XmlReaderSettings.Schemas`/`XDocument` IDS parser is the retired form; the IDS-FILE audit (the spec's own validity) is the buildingSMART-official `ids-lib` `Audit.Run`/`RunAsync` returning a `Status` flag set, orthogonal to the MODEL audit the `Audit` fold runs — neither re-implements the other; the facet's IFC entity/predefined/attribute references resolve against the real `ids-lib` `IdsLib.IfcSchema` `SchemaInfo` graph (`GetConcreteClassesFrom`/`ClassInfo.Is`/`PredefinedTypeValues`) through the `IdsSchema` helper so a facet is validated against the actual IFC4.3 schema, not a hard-coded class list, the `IfcSchemaVersionHelper` bridging the `Xids` `IfcSchemaVersion` to the `ids-lib` `IfcSchemaVersions`; cross-tool audit execution routes to the IfcOpenShell ifctester companion (`python:geometry/ifc-companion`) over Compute's existing companion rpc, the C# owner parses the spec through `Xids`, and reconciles the returned `IdsVerdict` oracle rows through `IdsAudit.Reconcile` into the typed `IdsParity` receipt — the join is the (GlobalId, `IdsFacet.FacetKey`) axis, so a positional verdict-list compare is the deleted form; the `IdsVerdict` row is the one seam contract the `csharp:Compute/Runtime/codecs#TWO_HOP_TESSELLATION` `IdsAuditRequest` leg projects and this owner consumes, never a second cross-runtime audit shape, the companion the external-conformance oracle never a transport minted here; the `Classification` facet consumes the `Semantics/classification#CLASSIFICATION_AXIS` axis and the `Property` facet the `Semantics/properties#PROPERTY_SETS` owner as settled vocabulary; the `IdsAudit`/`IdsFileAudit` receipts are the typed validation evidence on the `Fin<T>` rail, never a generic `IReceipt`.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using GeometryGym.Ifc;
using IdsLib;
using IdsLib.IfcSchema;
using LanguageExt;
using Microsoft.Extensions.Logging.Abstractions;
using Thinktecture;
using Xbim.InformationSpecifications;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

[Union]
public partial record IdsFacet {
    partial record Entity(IfcClass Class, Option<ValueConstraint> PredefinedType);
    partial record Attribute(string Name, Option<ValueConstraint> Value);
    partial record Property(string SetName, string Name, Option<ValueConstraint> Value, Option<string> DataType);
    partial record Classification(global::Rasm.Bim.Classification System, Option<ValueConstraint> Code);
    partial record Material(Option<ValueConstraint> Value);
    partial record PartOf(IfcClass Class, AssemblyRelKind Relation);

    // The STRUCTURAL selection lowers to the Model/query#ELEMENT_SET ElementPredicate verbatim — the
    // validation predicate IS the query predicate — and the VALUE match is the Xbim.InformationSpecifications
    // ValueConstraint engine (exact/pattern/range/structure, IFC-datatype-aware) via Satisfies, never a
    // String.Equals: ToPredicate selects the applicable set by class/set/name presence, Satisfies decides value.
    public ElementPredicate ToPredicate() => this.Switch(
        entity:         static f => (ElementPredicate)new ElementPredicate.ByClass(f.Class),
        attribute:      static f => new ElementPredicate.ByProperty("", f.Name, ""),
        property:       static f => new ElementPredicate.ByProperty(f.SetName, f.Name, ""),
        classification: static f => new ElementPredicate.ByClassification(f.System, Option<ClassificationCode>.None),
        material:       static f => new ElementPredicate.ByProperty("__material__", "Name", ""),
        partOf:         static f => new ElementPredicate.BySpatialContainer(f.Class.Key));

    public bool Satisfies(BimElement element) =>
        Constraint.Match(
            Some: constraint => Candidate(element).Exists(value => constraint.IsSatisfiedBy(value, ignoreCase: true, NullLogger.Instance)),
            None: () => Candidate(element).Any || this is PartOf);

    Option<ValueConstraint> Constraint => this.Switch(
        entity:         static f => f.PredefinedType, attribute: static f => f.Value, property: static f => f.Value,
        classification: static f => f.Code,           material:  static f => f.Value, partOf:   static _ => Option<ValueConstraint>.None);

    // The element's actual value for this facet — the candidate the ValueConstraint matches against.
    Seq<string> Candidate(BimElement element) => this.Switch(
        entity:         f => Seq1(element.Predefined.ToString()),
        attribute:      f => f.Name == "Name" ? Seq1(element.Name) : f.Name == "Tag" ? Seq1(element.Tag) : Seq<string>(),
        property:       f => element.Properties.Filter(p => p.SetName == f.SetName && p.Name == f.Name).Map(static p => p.Value),
        classification: f => element.Classifications.Filter(c => c.System == f.System.Key).Map(static c => c.Code),
        material:       f => element.Materials.Map(static m => m.MaterialName),
        partOf:         f => element.SpatialContainerId.ToSeq());

    public string FacetKey => this.Switch(
        entity:         static _ => "entity",
        attribute:      static _ => "attribute",
        property:       static _ => "property",
        classification: static _ => "classification",
        material:       static _ => "material",
        partOf:         static _ => "partOf");
}

public enum AssemblyRelKind : byte { Aggregates = 0, Nests = 1, ContainedIn = 2, Voids = 3, Connects = 4 }

public enum IdsCardinality : byte { Required = 0, Prohibited = 1, Optional = 2 }

public sealed record IdsRequirement(IdsFacet Facet, IdsCardinality Cardinality);

public sealed record IdsSpecification(
    string Name,
    Seq<IdsFacet> Applicability,
    Seq<IdsRequirement> Requirements,
    IdsCardinality Cardinality) {
    public Fin<IdsAudit> Audit(BimModel model) {
        var applicable = Applicability.Match(
            Empty: () => new ElementSet(model.Elements),
            Head:  head => ElementSet.Query(model,
                Applicability.Tail.Fold(new ElementQuery(head.ToPredicate()), static (q, facet) => q.And(facet.ToPredicate())))
                .Where(e => Applicability.ForAll(f => f.Satisfies(e))));
        var verdicts = Requirements.Map(req => {
            var matched = applicable.Where(e => req.Facet.Satisfies(e));
            var (pass, fail) = req.Cardinality switch {
                IdsCardinality.Required   => (matched, applicable.Except(matched)),
                IdsCardinality.Prohibited => (applicable.Except(matched), matched),
                _                         => (applicable, new ElementSet(Seq<BimElement>())),
            };
            return new IdsAudit.FacetVerdict(req.Facet, req.Cardinality,
                pass.Elements.Map(static e => e.GlobalId), fail.Elements.Map(static e => e.GlobalId));
        });
        return Fin.Succ(new IdsAudit(Name, applicable.Elements.Count, verdicts));
    }

    // The IDS document parse composes Xbim.InformationSpecifications `Xids` — the buildingSMART IDS v1.0
    // schema binding — projecting each `Specification`'s applicability/requirement `FacetGroup` onto the
    // closed `IdsFacet` union, retiring the hand-rolled XmlReaderSettings.Schemas/XDocument parser.
    public static Fin<Seq<IdsSpecification>> Parse(ReadOnlyMemory<byte> idsBytes) =>
        Try.lift(() => {
            using var stream = new MemoryStream(idsBytes.ToArray());
            var xids = Xids.LoadBuildingSmartIDS(stream, NullLogger.Instance)
                ?? throw new InvalidDataException("ids-load-empty");
            return xids.AllSpecifications().ToSeq().Map(Project);
        }).Run().MapFail(static error => new BimFault.ModelRejected($"ids-parse:{error.Message}").ToError());

    static IdsSpecification Project(Specification spec) =>
        new(spec.Name ?? "",
            Facets(spec.Applicability).Map(static f => f),
            Facets(spec.Requirement).Map(facet => new IdsRequirement(facet, Cardinality(spec.Cardinality))),
            Cardinality(spec.Cardinality));

    static Seq<IdsFacet> Facets(FacetGroup? group) =>
        Optional(group).Map(static g => g.Facets.AsIterable().Choose(FacetOf).ToSeq()).IfNone(Seq<IdsFacet>());

    static Option<IdsFacet> FacetOf(IFacet facet) => facet switch {
        IfcTypeFacet f         => IfcClass.TryGet(f.IfcType?.ToString() ?? "").Map(c => (IdsFacet)new IdsFacet.Entity(c, Optional(f.PredefinedType))),
        AttributeFacet f       => Some((IdsFacet)new IdsFacet.Attribute(f.AttributeName?.ToString() ?? "", Optional(f.AttributeValue))),
        IfcPropertyFacet f     => Some((IdsFacet)new IdsFacet.Property(f.PropertySetName?.ToString() ?? "", f.PropertyName?.ToString() ?? "", Optional(f.PropertyValue), Optional(f.DataType))),
        IfcClassificationFacet f => Classification.TryGet(f.ClassificationSystem?.ToString() ?? "").Map(s => (IdsFacet)new IdsFacet.Classification(s, Optional(f.Identification))),
        MaterialFacet f        => Some((IdsFacet)new IdsFacet.Material(Optional(f.Value))),
        PartOfFacet f          => IfcClass.TryGet(f.EntityType?.IfcType?.ToString() ?? "").Map(c => (IdsFacet)new IdsFacet.PartOf(c, AssemblyRelKind.Aggregates)),
        _                      => Option<IdsFacet>.None,
    };

    static IdsCardinality Cardinality(ICardinality? cardinality) =>
        cardinality is { ExpectsRequirements: false, AllowsRequirements: true } ? IdsCardinality.Optional
        : cardinality is { AllowsRequirements: false } ? IdsCardinality.Prohibited
        : IdsCardinality.Required;

    // The IDS-FILE audit (the spec document's own validity) is the buildingSMART-official ids-lib engine,
    // orthogonal to the MODEL audit (elements vs spec) the Audit fold runs: Audit.RunAsync validates the
    // .ids against the IDS v1.0 XSD + implementation agreements, the Status flags the pass/fail discriminant.
    public static Fin<IdsFileAudit> AuditFile(ReadOnlyMemory<byte> idsBytes) =>
        Try.lift(() => {
            using var stream = new MemoryStream(idsBytes.ToArray());
            var status = global::IdsLib.Audit.Run(stream,
                new SingleAuditOptions { IdsVersion = IdsVersion.Ids1_0 }, NullLogger.Instance);
            return new IdsFileAudit(status, LibraryInformation.AssemblyVersion);
        }).Run().MapFail(static error => new BimFault.ModelRejected($"ids-file-audit:{error.Message}").ToError());
}

// The IDS-FILE audit receipt: the ids-lib Status flags plus the engine build the audit ran under so a
// stored receipt is reproducible. Status.Ok is the pass; any error flag is the reject.
public sealed record IdsFileAudit(global::IdsLib.Audit.Status Status, string EngineVersion) {
    public bool Conforms => Status == global::IdsLib.Audit.Status.Ok;
}

// The offline IFC schema authority (ids-lib IdsLib.IfcSchema): resolves a facet's entity/predefined/attribute
// references against the real IFC4.3 schema graph so a facet is validated against the actual schema, not a
// hard-coded class list — GetConcreteClassesFrom expands an abstract supertype, PredefinedTypeValues the
// admitted predefined tokens, GetMeasureInformation the SI-base dimensional metadata the property store reads.
public static class IdsSchema {
    static readonly SchemaInfo Ifc4x3 = SchemaInfo.SchemaIfc4x3;

    public static Seq<string> ConcreteClasses(string topClass) =>
        SchemaInfo.GetConcreteClassesFrom(topClass, IfcSchemaVersions.Ifc4x3).ToSeq();

    public static Option<ClassInfo> ClassOf(string name) => Optional(Ifc4x3[name]);

    public static bool IsSubtype(string candidate, string supertype) =>
        Optional(Ifc4x3[candidate]).Exists(c => c.Is(supertype));
}

public sealed record IdsAudit(string Specification, int ApplicableCount, Seq<IdsAudit.FacetVerdict> Verdicts) {
    public sealed record FacetVerdict(IdsFacet Facet, IdsCardinality Cardinality, Seq<string> Passed, Seq<string> Failed);

    public bool Conforms => Verdicts.ForAll(static v => v.Failed.IsEmpty);

    public IdsParity Reconcile(Seq<IdsVerdict> oracle) {
        var mineRows = Verdicts.Bind(v =>
            v.Passed.Map(g => ((g, v.Facet.FacetKey), Passed: true)) + v.Failed.Map(g => ((g, v.Facet.FacetKey), Passed: false)));
        var theirRows = oracle.Filter(o => o.Specification == Specification).Map(o => ((o.GlobalId, o.Facet), Passed: o.Passed));
        var mine = mineRows.ToHashMap(static row => row.Item1, static row => row.Passed);
        var theirs = theirRows.ToHashMap(static row => row.Item1, static row => row.Passed);
        var keys = (mineRows.Map(static row => row.Item1) + theirRows.Map(static row => row.Item1)).Distinct();
        return new IdsParity(Specification,
            keys.Map(key => new IdsParity.Row(key.Item1, key.Item2, mine.Find(key), theirs.Find(key))));
    }
}

public readonly record struct IdsVerdict(string GlobalId, string Specification, string Facet, bool Passed, string Reason);

public sealed record IdsParity(string Specification, Seq<IdsParity.Row> Rows) {
    public readonly record struct Row(string GlobalId, string Facet, Option<bool> SelfAudit, Option<bool> Oracle) {
        public bool Agrees => SelfAudit.Match(s => Oracle.Match(o => s == o, static () => false), static () => false);
    }

    public Seq<Row> Divergences => Rows.Filter(static r => !r.Agrees);

    public bool Conformant => Divergences.IsEmpty;
}
```

## [03]-[RESEARCH]

- [IDS_SPEC_MODEL]: the IDS v1.0 spec model is the `Xbim.InformationSpecifications` `Xids` document (`.api/api-xbim-informationspecifications`) — `Xids.LoadBuildingSmartIDS(Stream)` parses the buildingSMART IDS XML, `AllSpecifications()` enumerates each `Specification` (an applicability `FacetGroup` + a requirement `FacetGroup` + an `ICardinality`), and the six `: FacetBase, IFacet` facets (`IfcTypeFacet`/`AttributeFacet`/`IfcPropertyFacet`/`IfcClassificationFacet`/`MaterialFacet`/`PartOfFacet`) carry their match fields as `ValueConstraint`s — so the `FacetOf` dispatch maps each `IFacet` concrete type onto the closed `IdsFacet` union arm and reads the `ValueConstraint` (the `ExactConstraint`/`PatternConstraint`/`RangeConstraint`/`StructureConstraint` components the `IsSatisfiedBy` engine folds, IFC-datatype-aware via `TryGetNetType`/`ParseValue`) rather than a stringly value; the hand-rolled `XmlReaderSettings.Schemas`/`XDocument` parser is retired because `Xids` owns the schema binding, and the `IfcSchemaVersion`↔`IfcSchemaVersions` bridge (`IfcSchemaVersionHelper`) keeps the spec-model and the `ids-lib` schema authority on one vocabulary.
- [IDS_FILE_SCHEMA_AUTHORITY]: the IDS-FILE audit and the IFC schema truth are the buildingSMART-official `ids-lib` (`.api/api-ids-lib`) — `Audit.Run(Stream, SingleAuditOptions { IdsVersion = Ids1_0 }, ILogger)` validates the `.ids` document against the IDS v1.0 XSD plus the implementation agreements onto a `Status` flag set (`Ok` the pass, any error flag the reject), `LibraryInformation.AssemblyVersion` stamping the engine build onto the `IdsFileAudit` receipt for reproducibility; the embedded `IdsLib.IfcSchema` `SchemaInfo` (`SchemaIfc4x3`/`GetConcreteClassesFrom`/`ClassInfo.Is`/`PredefinedTypeValues`/`GetMeasureInformation`) is the offline IFC4.3 schema graph the `IdsSchema` helper resolves a facet's entity/predefined/attribute references against, so a facet is validated against the actual schema rather than a hard-coded list and the property-template/measure metadata reconciles with the `Semantics/properties#PROPERTY_SETS` `UnitsNet` SI coercion — `ids-lib` supplies the schema truth, `UnitsNet` the value conversion.
- [IFCTESTER_COMPANION]: the C# seam shape is settled — `IdsAudit.Reconcile` consumes the `IdsVerdict(GlobalId, Specification, Facet, Passed, Reason)` rows the `csharp:Compute/Runtime/codecs#TWO_HOP_TESSELLATION` `IdsAuditRequest` leg returns and folds the typed `IdsParity` receipt on the (GlobalId, `FacetKey`) join, so only the Python decoder internals remain the companion's concern; the IfcOpenShell ifctester invocation (the IDS XML payload `Author` emits, the `ids.open`/`ids.validate` call producing the per-(GlobalId, facet) pass/fail rows, and the deterministic audit-test-suite parity) is owned by `libs/python/geometry` (`python:geometry/ifc-companion`) and orchestrated over Compute's existing companion rpc identically to the `Exchange/tessellation#TESSELLATION_BRIDGE` two-hop pattern, the in-process fold the immediate self-audit and the companion the external-conformance oracle the `IdsVerdict` rows carry.
