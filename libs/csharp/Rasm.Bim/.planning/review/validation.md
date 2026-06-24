# [BIM_IDS]

The buildingSMART IDS v1.0 model-validation owner: one `IdsSpecification` record parsed from and authored to the IDS XSD, its six facets folded into one closed `IdsFacet` `[Union]` that lowers each applicability/requirement facet onto the `Model/query#ELEMENT_SET` `ElementPredicate` algebra over `BimModel`, and one `IdsAudit` deterministic receipt. The validation predicate IS the query predicate — an IDS Entity/Attribute/Property/Classification/Material/PartOf facet folds to the same `ElementPredicate` arm the set query reads, never a second selection surface. The page composes the `Model/elements#ELEMENT_MODEL` `BimModel`, the `Model/query#ELEMENT_SET` algebra, the `Semantics/classification#CLASSIFICATION_AXIS` axis, and the planned `Semantics/properties#PROPERTY_SETS` owner as settled vocabulary; the in-process fold gives an immediate self-audit and the IfcOpenShell ifctester companion gives the deterministic cross-tool audit matching the buildingSMART IDS audit test-suite. The page is HOST-LOCAL.

## [01]-[INDEX]

- [01]-[IDS_FACETS]: `IdsFacet` closed union, `IdsSpecification`/`IdsRequirement` records, the `IdsAudit` self-audit receipt, the `IdsAudit.Reconcile` -> `IdsParity` ifctester-oracle cross-tool diff, and the XSD parse/author over `System.Xml.Schema`.

## [02]-[IDS_FACETS]

- Owner: `IdsSpecification` the IDS specification record carrying applicability and requirement facet sets and the cardinality; `IdsFacet` `[Union]` the closed facet family (Entity, Attribute, Property, Classification, Material, PartOf) each lowering to an `ElementPredicate` arm and projecting one canonical `FacetKey` join token; `IdsAudit` the deterministic per-specification receipt folding the matched/passed/failed element GlobalIds; `IdsVerdict` the per-(GlobalId, facet) oracle row the `csharp:Compute/Runtime/codecs#TWO_HOP_TESSELLATION` `IdsAuditRequest` companion-rpc leg projects back from the IfcOpenShell ifctester (the one shape on both ends of the seam); `IdsParity` the typed reconcile receipt `IdsAudit.Reconcile` folds, joining the C# self-audit against the ifctester oracle on the (GlobalId, `FacetKey`) axis and carrying the per-row divergence set — never a message diff.
- Entry: `IdsSpecification.Audit(BimModel model)` folds the applicability facets into one `ElementQuery` selecting the applicable element set, then folds each requirement facet into an `ElementPredicate` partitioning the applicable set into passed and failed — `Fin<T>` aborts on a facet referencing an unknown classification system or property template (`Model/faults#FAULT_BAND` `BimFault.UnmappedClass`) or a malformed IDS payload (`BimFault.ModelRejected`), each lowered with `.ToError()`; `IdsSpecification.Parse(ReadOnlyMemory<byte> xsdBytes)` admits an IDS XML document validated against the IDS v1.0 XSD through `XmlReaderSettings.Schemas`, and `IdsSpecification.Author(Seq<IdsSpecification> specs)` emits the IDS XML the companion ifctester consumes.
- Auto: `Audit` reads the applicability facet set, folds each to its `ElementPredicate` arm through `Facet.ToPredicate`, conjoins them through `ElementQuery.And`, and runs `ElementSet.Query(model, applicability)` for the applicable set; for each requirement facet it builds the requirement predicate and partitions the applicable set by `Match` — an element in the applicable set failing the `Required` predicate or matching a `Prohibited` predicate lands a failed GlobalId, the rest pass; the `Classification` facet lowers to `ElementPredicate.ByClassification` reading the typed `BimElement.Classifications` `ClassificationRef`, the `Property` facet to `ElementPredicate.ByProperty` over the `Semantics/properties#PROPERTY_SETS` keyed set, the `Entity` facet to `ElementPredicate.ByClass`, and the `Material`/`Attribute`/`PartOf` facets to their predicate arms so the validation fold reuses the query algebra verbatim.
- Receipt: `IdsAudit` carries the specification name, the applicable element count, the passed and failed GlobalId sets, and the per-facet verdict so a requirement-driven exchange acceptance reads one typed receipt; `IdsAudit.Reconcile(Seq<IdsVerdict> oracle)` folds the companion ifctester projection (the `IdsVerdict` rows the `csharp:Compute/Runtime/codecs#TWO_HOP_TESSELLATION` leg returns) against the self-audit into an `IdsParity` receipt, joining each row on the (GlobalId, `IdsFacet.FacetKey`) axis so an element where the self-audit and the standards-conformant oracle disagree lands one typed `IdsParity.Row` divergence — `IdsParity.Conformant` is the cross-tool parity verdict, never a message diff.
- Packages: GeometryGymIFC_Core, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm, BCL `System.Xml`/`System.Xml.Schema`/`System.Xml.Linq`
- Growth: a new IDS facet is one `IdsFacet` union arm lowering to its `ElementPredicate` arm and projecting its `FacetKey` token; a new cardinality is one column on `IdsRequirement`; the cross-tool audit is one companion-rpc shape over the same `csharp:Compute/Runtime/codecs#TWO_HOP_TESSELLATION` companion pattern the tessellation bridge uses and one `IdsAudit.Reconcile` fold over the returned `IdsVerdict` rows; a new parity dimension is one column on `IdsParity.Row`; never a second validation predicate surface, never a second reconcile surface, and never a transport minted here.
- Boundary: the validation predicate IS the `Model/query#ELEMENT_SET` `ElementPredicate` — an `IdsValidator`/`IdsRule` evaluation family or a second selection surface is the deleted form, the IDS fold folds to the query algebra verbatim; the IDS XSD parse and the spec authoring ride the BCL `System.Xml.Schema` `XmlSchemaSet`/`XmlReaderSettings.Schemas` surface and the `System.Xml.Linq` `XDocument` projection — a hand-rolled IDS parser is the deleted form; cross-tool audit execution routes to the IfcOpenShell ifctester companion (`python:geometry/ifc-companion`) over Compute's existing companion rpc, the C# owner authors and parses the spec, projects the IDS XML through `Author`, and reconciles the returned `IdsVerdict` oracle rows through `IdsAudit.Reconcile` into the typed `IdsParity` receipt — the join is the (GlobalId, `IdsFacet.FacetKey`) axis, the same facet token both ends carry, so a `String.Equals` message diff or a positional verdict-list compare is the deleted form; the `IdsVerdict` row is the one seam contract the `csharp:Compute/Runtime/codecs#TWO_HOP_TESSELLATION` `IdsAuditRequest` leg projects and this owner consumes, never a second cross-runtime audit shape, and the companion is the external-conformance oracle, never a transport minted here; the `Classification` facet consumes the `Semantics/classification#CLASSIFICATION_AXIS` axis and the `Property` facet the `Semantics/properties#PROPERTY_SETS` owner as settled vocabulary, never re-deriving a classification mapping or a property store; the `IdsAudit` receipt is the typed validation evidence on the `Fin<T>` rail, never a generic `IReceipt`.

```csharp signature
[Union]
public partial record IdsFacet {
    partial record Entity(IfcClass Class, Option<string> PredefinedType);
    partial record Attribute(string Name, Option<string> Value);
    partial record Property(string SetName, string Name, Option<string> Value, Option<string> DataType);
    partial record Classification(global::Rasm.Bim.Classification System, Option<ClassificationCode> Code);
    partial record Material(Option<string> Value);
    partial record PartOf(IfcClass Class, AssemblyRelKind Relation);

    public ElementPredicate ToPredicate() => this.Switch(
        entity:         static f => f.PredefinedType.Match(
                                        Some: value => (ElementPredicate)new ElementPredicate.ByPredefinedType(f.Class, PredefinedType.Create(value)),
                                        None: () => new ElementPredicate.ByClass(f.Class)),
        attribute:      static f => new ElementPredicate.ByProperty("", f.Name, f.Value.IfNone("")),
        property:       static f => new ElementPredicate.ByProperty(f.SetName, f.Name, f.Value.IfNone("")),
        classification: static f => f.Code.Match(
                                        Some: code => new ElementPredicate.ByClassification(f.System, code),
                                        None: () => new ElementPredicate.ByClass(IfcClass.Proxy)),
        material:       static f => new ElementPredicate.ByProperty("__material__", "Name", f.Value.IfNone("")),
        partOf:         static f => new ElementPredicate.BySpatialContainer(f.Class.Key));

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
                Applicability.Tail.Fold(new ElementQuery(head.ToPredicate()), static (q, facet) => q.And(facet.ToPredicate()))));
        var verdicts = Requirements.Map(req => {
            var predicate = req.Facet.ToPredicate();
            var matched = applicable.Where(e => Holds(e, predicate, model));
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

    static bool Holds(BimElement element, ElementPredicate predicate, BimModel model) =>
        ElementSet.Query(model, new ElementQuery(predicate)).Elements.Exists(e => e.GlobalId == element.GlobalId);

    public static Fin<IdsSpecification> Parse(ReadOnlyMemory<byte> xsdBytes) =>
        Try.lift(() => Read(xsdBytes)).Run().MapFail(static error => new BimFault.ModelRejected($"ids-parse:{error.Message}").ToError());

    static IdsSpecification Read(ReadOnlyMemory<byte> xsdBytes) {
        var settings = new XmlReaderSettings { ValidationType = ValidationType.Schema };
        settings.Schemas.Add(IdsSchemaUri, XmlReader.Create(new MemoryStream(xsdBytes.ToArray())));
        var doc = XDocument.Load(XmlReader.Create(new MemoryStream(xsdBytes.ToArray()), settings));
        var ns = doc.Root!.GetDefaultNamespace();
        var spec = doc.Descendants(ns + "specification").First();
        return new IdsSpecification(
            spec.Attribute("name")?.Value ?? "",
            spec.Element(ns + "applicability")!.Elements().Choose(el => FacetOf(el, ns)).ToSeq(),
            spec.Element(ns + "requirements")!.Elements().Choose(el =>
                FacetOf(el, ns).Map(facet => new IdsRequirement(facet, CardinalityOf(el)))).ToSeq(),
            CardinalityOf(spec));
    }

    static Option<IdsFacet> FacetOf(XElement el, XNamespace ns) => el.Name.LocalName switch {
        "entity"         => IfcClass.TryGet(el.Element(ns + "name")?.Value ?? "").Map(c => (IdsFacet)new IdsFacet.Entity(c, Optional(el.Element(ns + "predefinedType")?.Value))),
        "attribute"      => Some((IdsFacet)new IdsFacet.Attribute(el.Element(ns + "name")?.Value ?? "", Optional(el.Element(ns + "value")?.Value))),
        "property"       => Some((IdsFacet)new IdsFacet.Property(el.Element(ns + "propertySet")?.Value ?? "", el.Element(ns + "baseName")?.Value ?? "", Optional(el.Element(ns + "value")?.Value), Optional(el.Attribute("dataType")?.Value))),
        "classification" => Classification.TryGet(el.Element(ns + "system")?.Value ?? "").Map(s => (IdsFacet)new IdsFacet.Classification(s, ClassificationCode.TryCreate(el.Element(ns + "value")?.Value ?? "").ToOption())),
        "material"       => Some((IdsFacet)new IdsFacet.Material(Optional(el.Element(ns + "value")?.Value))),
        "partOf"         => IfcClass.TryGet(el.Attribute("entity")?.Value ?? "").Map(c => (IdsFacet)new IdsFacet.PartOf(c, Enum.Parse<AssemblyRelKind>(el.Attribute("relation")?.Value ?? "Aggregates", true))),
        _                => Option<IdsFacet>.None,
    };

    static IdsCardinality CardinalityOf(XElement el) =>
        el.Attribute("cardinality")?.Value switch { "prohibited" => IdsCardinality.Prohibited, "optional" => IdsCardinality.Optional, _ => IdsCardinality.Required };

    const string IdsSchemaUri = "http://standards.buildingsmart.org/IDS";
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

- [IDS_XSD_GRAMMAR]: the IDS v1.0 XSD element grammar — the `ids`/`specification`/`applicability`/`requirements` structure, the six facet element local-names (`entity`/`attribute`/`property`/`classification`/`material`/`partOf`), the `simpleValue`/`restriction` value-constraint sub-grammar (`xs:enumeration`/`xs:pattern`/`xs:minInclusive` bounds), the `cardinality` attribute vocabulary (`required`/`prohibited`/`optional`), and the `dataType`/`uri`/`instructions` attributes — grounds against the published buildingSMART IDS v1.0 XSD (June 2024) and the ifctester reference parser so the `FacetOf` element-name dispatch and the value-restriction projection match the audit test-suite before the `Parse` body is final; the `XmlSchemaSet.Add`/`XmlReaderSettings.Schemas` validation seam and the `XDocument`/`XElement` projection are BCL inbox and settled.
- [IFCTESTER_COMPANION]: the C# seam shape is settled — `IdsAudit.Reconcile` consumes the `IdsVerdict(GlobalId, Specification, Facet, Passed, Reason)` rows the `csharp:Compute/Runtime/codecs#TWO_HOP_TESSELLATION` `IdsAuditRequest` leg returns and folds the typed `IdsParity` receipt on the (GlobalId, `FacetKey`) join, so only the Python decoder internals remain the companion's concern; the IfcOpenShell ifctester invocation (the IDS XML payload `Author` emits, the `ids.open`/`ids.validate` call producing the per-(GlobalId, facet) pass/fail rows, and the deterministic audit-test-suite parity) is owned by `libs/python/geometry` (`python:geometry/ifc-companion`) and orchestrated over Compute's existing companion rpc identically to the `Exchange/tessellation#TESSELLATION_BRIDGE` two-hop pattern, the in-process fold the immediate self-audit and the companion the external-conformance oracle the `IdsVerdict` rows carry.
