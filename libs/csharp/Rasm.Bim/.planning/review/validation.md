# [BIM_IDS]

The buildingSMART IDS v1.0 model-validation owner: one `IdsSpecification` parsed from the `Xbim.InformationSpecifications` `Xids` document, its six facets folded into one closed `IdsFacet` `[Union]` that LOWERS each applicability/requirement facet onto the `Model/query#ELEMENT_SET` `ElementPredicate` algebra — the structural selection AND the typed `ValueMatch` value restriction in ONE predicate — one TOTAL `IdsSpecification.Audit` model-audit fold over the seam `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph`, and one `IdsFileAudit` over the buildingSMART-official `ids-lib` `Audit` for the IDS document's own validity. The validation predicate IS the query predicate: an IDS Entity/Attribute/Property/Classification/Material/PartOf facet lowers to the SAME `ElementPredicate` arm `ElementSet.Query` reads, the value decided by the seam `ValueMatch` (`Exact`/`Pattern`/`Range`/`OneOf`/`Length`/`Present`) the facet's `ValueConstraint` components lower onto at parse — never a second selection surface, and never a `ValueConstraint.IsSatisfiedBy` value engine beside the query. The model audit is TOTAL because `ElementSet.Query`/`Where`/`Except` are total over a frozen graph; only the boundary admissions carry the `Fin` rail — `IdsSpecification.Parse` over `Xids.LoadBuildingSmartIDS` and `IdsSpecification.AuditFile` over `ids-lib` `Audit.Run`, each foreign-byte fault lifting BARE as `Model/faults#FAULT_BAND` `BimFault.ModelRejected` (band 2600 IS the `Expected` `Code`, no `.ToError()` hop). The page composes the seam `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph`/`Element`, the `Model/query#ELEMENT_SET` algebra, the `Model/elements#IFC_CLASS` `IfcClass` roster, the `Semantics/classification#CLASSIFICATION_AXIS` `ClassificationSystem` projector, the `Semantics/properties#PROPERTY_TEMPLATES` template authority, and the `ids-lib` `IdsLib.IfcSchema` offline schema authority (subtype expansion + measure-dimension resolution) as settled vocabulary; the in-process fold is the immediate self-audit and the IfcOpenShell ifctester companion the deterministic cross-tool oracle the `IdsVerdict` rows carry. The page is HOST-LOCAL.

## [01]-[INDEX]

- [01]-[IDS_FACETS]: `IdsFacet` the closed `[Union]` of seam-lowered facets (each `ToPredicate(graph)` an `ElementPredicate`, the value a `ValueMatch`), `IdsRequirement`/`IdsSpecification` the spec records, `IdsSpecification.Parse` over `Xids.LoadBuildingSmartIDS` (the boundary lowering of every `ValueConstraint`), `IdsSpecification.Audit` the total model-audit fold over the seam `ElementGraph`, `IdsSpecification.AuditFile` over `ids-lib` `Audit` (with captured `IdsDiagnostic` rows), the `IdsSchema` offline schema authority, the `IdsAudit` deterministic receipt, and the `IdsAudit.Reconcile` -> `IdsParity` ifctester-oracle cross-tool diff over the Bim-owned `IdsVerdict` row.

## [02]-[IDS_FACETS]

- Owner: `IdsSpecification` the IDS specification record carrying the applicability and requirement facet sets and the cardinality; `IdsFacet` the closed `[Union]` (Entity, Attribute, Property, Classification, Material, PartOf) each carrying SEAM-LOWERED data (resolved `IfcClass`/`PredefinedType` sets, the typed `ValueMatch` value restriction, resolved `Classification` branches) admitted ONCE at parse so the interior never sees an `Xbim` `ValueConstraint`, each arm `ToPredicate(graph)` lowering to a `Model/query#ELEMENT_SET` `ElementPredicate` and projecting a `FacetKey` join token that identifies the facet UNIQUELY within its specification (a value-discriminated arm folds its Pset/name, attribute, system, or relation into the token so two same-kind requirements never collide in the reconcile join); `IdsAudit` the deterministic per-specification receipt folding the matched/passed/failed element `GlobalId`s AND carrying the spec-level `IdsCardinality` so `Conforms` enforces the applicable-count rule; `IdsFileAudit` the IDS-document validity receipt (the `ids-lib` `Status` plus the captured `IdsDiagnostic` rows and the engine build); `IdsVerdict` the per-(GlobalId, facet) cross-runtime oracle row the `csharp:Rasm.Compute/Runtime/codecs#TWO_HOP_TESSELLATION` `IdsAuditRequest` companion-rpc leg projects from the IfcOpenShell ifctester and COMPOSES from here (Bim owns the row, Compute references up-stratum — Bim never depends on Compute); `IdsParity` the typed reconcile receipt joining the C# self-audit against the ifctester oracle on the (GlobalId, `FacetKey`) axis, carrying the per-row divergence set — never a message diff.
- Entry: `IdsSpecification.Audit(ElementGraph graph)` is TOTAL — it folds the applicability facets into one `ElementQuery` and runs `ElementSet.Query(graph, query)` for the applicable set, then for each requirement facet refines the applicable set by `ElementSet.Where(facet.ToPredicate(graph))` and partitions pass/fail through the `IdsCardinality.Partition` row (total dispatch, no runtime-silent switch arm); `IdsSpecification.Parse(ReadOnlyMemory<byte> idsBytes, Op key)` admits an IDS XML document through `Xids.LoadBuildingSmartIDS`, lowering every `Specification`'s applicability/requirement `FacetGroup` and every facet's `ValueConstraint` onto the closed `IdsFacet` union (the one boundary), `Fin<T>` lifting `BimFault.ModelRejected` BARE on a malformed payload; `IdsSpecification.AuditFile(ReadOnlyMemory<byte> idsBytes, Op key)` validates the IDS document's own conformance through the `ids-lib` `Audit.Run` engine onto an `IdsFileAudit` carrying the `Status` plus the per-issue `IdsDiagnostic` rows a `BufferingLogger` captures off the engine's `ILogger` channel.
- Auto: `Parse` is the value-lowering boundary — `Matches` folds a facet's `ValueConstraint.AcceptedValues` components onto a `Seq<ValueMatch>` (an `ExactConstraint` set collapses to one `OneOf`, a `PatternConstraint` to a `Pattern`, a `RangeConstraint` to a dimension-checked `Range` whose bound `MeasureValue` carries the `ids-lib`-resolved `Dimension`, a `StructureConstraint` to a `Length`, an absent constraint to `Present`), `ResolveClasses` expands an Entity facet's `IfcType` to its `IdsSchema.ConcreteClasses` subtypes when `IncludeSubtypes`, and `ClassificationBranches` resolves the system name through the `Semantics/classification#CLASSIFICATION_AXIS` `ClassificationSystem` roster and mints a seam `Classification` branch per identification; `Audit` then folds each facet's `ToPredicate(graph)` — Entity to an `Any` of `ByClass`/`ByPredefinedType`, Attribute to `ByAttribute`, Property to `ByProperty`, Classification to `ByClassification` (the `Within` prefix realizing sub-branch inclusion), Material to `ByMaterial`, PartOf to an `Any` of `BySpatialContainer`/`ByZone` over the graph-resolved container set — so the validation fold reuses the query algebra for BOTH selection and value with one total `Switch`.
- Receipt: `IdsAudit` carries the specification name, the spec-level `IdsCardinality`, the applicable element count, and the passed/failed `GlobalId` sets per facet so a requirement-driven exchange acceptance reads one typed receipt; `IdsAudit.Conforms` is BOTH the spec-level applicable-count rule (`SpecSatisfied` — a Required spec needs an applicable element, a Prohibited spec none) AND every requirement verdict passing, never a stored-but-unread cardinality; `IdsAudit.Reconcile(Seq<IdsVerdict> oracle)` folds the companion ifctester projection against the self-audit into an `IdsParity` receipt, joining each row on the (GlobalId, `IdsFacet.FacetKey`) axis where `FacetKey` is unique within the spec so two same-kind facets never collapse a verdict, so an element where the self-audit and the standards-conformant oracle disagree lands one typed `IdsParity.Row` divergence — `IdsParity.Conformant` is the cross-tool parity verdict, never a message diff; `IdsFileAudit.Conforms`/`Errors` reads the `Status` plus the captured diagnostics.
- Packages: Xbim.InformationSpecifications, ids-lib, Microsoft.Extensions.Logging.Abstractions, Rasm.Element, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm
- Growth: a new IDS facet is one `IdsFacet` union arm lowering to its `ElementPredicate` arm plus its `Matches` value lowering and projecting its `FacetKey` token; a new value-match modality is one `ValueMatch` arm the seam already folds; a new cardinality is one `IdsCardinality` row; the cross-tool audit is one companion-rpc shape over the existing `csharp:Rasm.Compute/Runtime/codecs#TWO_HOP_TESSELLATION` pattern and one `IdsAudit.Reconcile` fold over the returned `IdsVerdict` rows; a new parity dimension is one column on `IdsParity.Row`; never a second validation predicate surface, never a second value engine, never a hand-rolled IDS parser, and never a transport minted here.
- Boundary: the validation predicate IS the `Model/query#ELEMENT_SET` `ElementPredicate` — an `IdsValidator`/`IdsRule` evaluation family or a second selection surface is the deleted form, the facet lowering to the query algebra verbatim — and the VALUE match is the seam `ValueMatch` the facet's `Xbim` `ValueConstraint` LOWERS onto at parse, so a `ValueConstraint.IsSatisfiedBy(candidate, …)` value engine, a `String.Equals` value compare, or a stringly `Candidate(BimElement)` walk beside the query is the deleted form (the retired second value surface); the model `Audit` reads the seam `ElementGraph` the `Projection/semantic#SEMANTIC_PROJECTOR` projector assembled and the retired `BimModel.Elements`/`BimElement` element record is GONE — a `new ElementSet(model.Elements)` over a second stored record is the deleted form, the audit reading the graph the `Bake` fold derives the consumer `Element` from; the IDS document parse is `Xids.LoadBuildingSmartIDS` (the package owns the buildingSMART IDS v1.0 schema binding) and a hand-rolled `XmlReaderSettings.Schemas`/`XDocument` IDS parser is the retired form; the IDS-FILE audit is the buildingSMART-official `ids-lib` `Audit.Run` returning a `Status` flag set, orthogonal to the MODEL audit and never re-implementing it; the facet's IFC entity/predefined/measure references resolve against the real `ids-lib` `IdsLib.IfcSchema` `SchemaInfo` graph (`GetConcreteClassesFrom`/`TryGetMeasureInformation`) through the `IdsSchema` helper so a facet is validated against the actual IFC4.3 schema, not a hard-coded class list; the cross-tool audit routes to the IfcOpenShell ifctester companion over Compute's existing companion rpc, the C# owner parsing the spec through `Xids` and reconciling the returned `IdsVerdict` oracle rows through `IdsAudit.Reconcile` into the typed `IdsParity` receipt — the `IdsVerdict` row is the one seam contract this owner OWNS and Compute composes up-stratum (Bim never references Compute), so a positional verdict-list compare or a Compute-side `IdsAudit` re-projection is the deleted form; the `IdsAudit`/`IdsFileAudit` receipts are the typed validation evidence on the `Fin<T>` rail, never a generic `IReceipt`.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Globalization;
using System.IO;
using IdsLib;
using IdsLib.IfcSchema;
using LanguageExt;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Rasm.Element;
using Thinktecture;
using Xbim.InformationSpecifications;
using static LanguageExt.Prelude;
using Op = Rasm.Domain.Op;
using SeamClassification = Rasm.Element.Classification;   // the seam (system, code) value-object — aliased so the
                                                         // nested IdsFacet.Classification arm never shadows it.

namespace Rasm.Bim;

// --- [TYPES] ------------------------------------------------------------------------------
// The IDS cardinality vocabulary owns BOTH its policies as delegate rows so the audit folds the row's behavior
// rather than an imperative switch with a runtime-silent arm: `Partition` is the requirement-level pass/fail split
// over the (matched, applicable) sets — Required passes the matched and fails the rest, Prohibited inverts, Optional
// passes all — and `SpecSatisfied` is the spec-level applicable-count rule, the Xbim ICardinality.IsSatisfiedBy
// truth (Required count>0, Prohibited count==0, Optional any) the IdsAudit conformance reads.
[SmartEnum<string>]
public sealed partial class IdsCardinality {
    public static readonly IdsCardinality Required   = new("required",   static (matched, applicable) => (matched, applicable.Except(matched)),         static count => count > 0);
    public static readonly IdsCardinality Prohibited = new("prohibited", static (matched, applicable) => (applicable.Except(matched), matched),         static count => count == 0);
    public static readonly IdsCardinality Optional   = new("optional",   static (matched, applicable) => (applicable, applicable.Except(applicable)),   static _ => true);

    public Func<ElementSet, ElementSet, (ElementSet Pass, ElementSet Fail)> Partition { get; }
    public Func<int, bool> SpecSatisfied { get; }
}

// The neutral PartOf relation, mapped from the Xbim PartOfFacet.PartOfRelation at parse so the union never carries
// the foreign enum. ONLY Contained (the Compose{Contain} spatial parent → BySpatialContainer) and Grouped (the
// Assign{Group} membership → ByZone) are expressible by the current query incidence arms; Aggregated/Nested/Voided
// have NO arm yet (and an Undefined parse-fail has no seam mirror), so a PartOf facet over them is DROPPED at parse
// (MapRelation → None / the PartOfFacetOf filter) rather than mis-lowered onto the Contain-only BySpatialContainer
// (which a decomposition/void/unparseable edge never satisfies — a false failure the page's drop-the-inexpressible-
// facet doctrine forbids); the three expressible-pending relations fold to precise predicates once the query grows a
// ByComposed(kind)/ByVoid incidence arm, with no page-local change beyond widening the filter (see [03]-[RESEARCH]).
public enum PartOfRelation : byte { Contained = 0, Aggregated = 1, Nested = 2, Grouped = 3, Voided = 4 }

// The closed IDS facet family — the mirror of the six Xbim FacetBase facets, but carrying SEAM-LOWERED data only
// (resolved IfcClass/PredefinedType sets, the typed ValueMatch restriction, resolved Classification branches): the
// Xbim ValueConstraint is admitted ONCE at IdsSpecification.Parse and never crosses into this interior. Each arm
// ToPredicate(graph) lowers to ONE Model/query#ELEMENT_SET ElementPredicate (structural selection + ValueMatch
// value), so the validation predicate IS the query predicate and there is no second Satisfies value engine.
[Union]
public partial record IdsFacet {
    partial record Entity(Seq<IfcClass> Classes, Seq<PredefinedType> Predefined);
    partial record Attribute(ObjectAttribute Attribute, Seq<ValueMatch> Value);
    partial record Property(string SetName, PropertyName Name, Seq<ValueMatch> Value);
    partial record Classification(Seq<SeamClassification> Branches);
    partial record Material(Seq<ValueMatch> Value);
    partial record PartOf(Option<IdsFacet> Container, PartOfRelation Relation);

    // The ONE lowering to the query algebra: a value-bearing arm folds its Seq<ValueMatch> into an Any of the
    // matching predicate (or the single arm), the Entity arm crosses its class set with its predefined tokens,
    // and PartOf resolves its container object set off the graph and folds the per-relation incidence arm. The
    // graph threads as Switch state so every arm stays static (PartOf is the only graph-reading arm).
    public ElementPredicate ToPredicate(ElementGraph graph) => Switch(
        state:          graph,
        entity:         static (_, f) => AnyOf(f.Classes.Bind(cls => f.Predefined.IsEmpty
                            ? Seq1<ElementPredicate>(new ElementPredicate.ByClass(cls))
                            : f.Predefined.Map(pt => (ElementPredicate)new ElementPredicate.ByPredefinedType(cls, pt)))),
        attribute:      static (_, f) => AnyOf(f.Value.Map(vm => (ElementPredicate)new ElementPredicate.ByAttribute(f.Attribute, vm))),
        property:       static (_, f) => AnyOf(f.Value.Map(vm => (ElementPredicate)new ElementPredicate.ByProperty(f.SetName, f.Name, vm))),
        classification: static (_, f) => AnyOf(f.Branches.Map(b => (ElementPredicate)new ElementPredicate.ByClassification(b))),
        material:       static (_, f) => AnyOf(f.Value.Map(vm => (ElementPredicate)new ElementPredicate.ByMaterial(vm))),
        partOf:         static (g, f) => PartOfLowering(g, f));

    // The cross-runtime join token an IdsAudit verdict and the companion IdsVerdict share on the (GlobalId, facet)
    // axis. It MUST identify a facet UNIQUELY WITHIN a specification — a spec carrying two property requirements
    // (distinct Psets/names) must NOT collapse to one "property" key, or IdsAudit.Reconcile's per-key join silently
    // drops a verdict — so the value-discriminated arms fold their identifying data into the token (the structural
    // Entity/Material axes stay singular: a spec carries one entity/material requirement). The companion
    // csharp:Rasm.Compute/Runtime/codecs#TWO_HOP_TESSELLATION IdsAuditRequest leg projects IdsVerdict.Facet in THIS
    // exact format so the two runtimes join on identical keys.
    public string FacetKey => Switch(
        entity:         static _ => "entity",
        attribute:      static a => $"attribute:{a.Attribute.Key}",
        property:       static p => $"property:{p.SetName}:{p.Name.Value}",
        classification: static c => $"classification:{c.Branches.Head.Map(static b => b.System).IfNone(string.Empty)}",
        material:       static _ => "material",
        partOf:         static p => $"partOf:{p.Relation}");

    // An empty arm set is a facet the query algebra cannot yet express (a system-only classification, an unresolvable
    // attribute) — it matches nothing rather than smuggling a raw Func<Node.Object,bool> walk past the query surface;
    // Parse drops such facets so the audit never raises a false failure (see [03]-[RESEARCH]).
    static ElementPredicate AnyOf(Seq<ElementPredicate> arms) =>
        arms.IsEmpty ? new ElementPredicate.Any(Seq<ElementPredicate>())
        : arms.Tail.IsEmpty ? arms.Head
        : new ElementPredicate.Any(arms);

    // PartOf is a graph-reachability facet: resolve the container Object set off the entity sub-facet (one
    // ElementSet.Query), then fold the per-relation incidence arm over the container ids — Grouped reads the
    // Assign{Group} membership (ByZone), Contained the Compose containment (BySpatialContainer). These are the ONLY
    // two relations that reach here; Aggregated/Nested/Voided were dropped at parse (FacetOf) as inexpressible.
    static ElementPredicate PartOfLowering(ElementGraph graph, PartOf f) {
        Seq<NodeId> containers = f.Container.Match(
            Some: c => ElementSet.Query(graph, ElementQuery.Of(c.ToPredicate(graph))).Objects.Map(static o => o.Id),
            None: () => graph.ObjectNodes.Map(static o => o.Id));
        Func<NodeId, ElementPredicate> arm = f.Relation == PartOfRelation.Grouped
            ? static id => new ElementPredicate.ByZone(id)
            : static id => new ElementPredicate.BySpatialContainer(id);
        return AnyOf(containers.Map(arm));
    }
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record IdsRequirement(IdsFacet Facet, IdsCardinality Cardinality);

public sealed record IdsSpecification(
    string Name,
    Seq<IdsFacet> Applicability,
    Seq<IdsRequirement> Requirements,
    IdsCardinality Cardinality) {

    // TOTAL model audit over the frozen seam graph: ElementSet.Query/Where/Except carry no rail and every facet's
    // seam-typed payload is admitted at Parse (no audit-time mint), so the audit is a pure fold (only the foreign-byte
    // admissions Parse/AuditFile carry Fin). The applicable set is the conjunction of the applicability facets (or
    // every object when applicability is empty); each requirement partitions the applicable set through its
    // IdsCardinality.Partition row (no runtime-silent switch arm), the verdict GlobalIds projected off the selected
    // ExternalId set, and the spec-level Cardinality rides onto the receipt so Conforms honors the applicable-count rule.
    public IdsAudit Audit(ElementGraph graph) {
        // LanguageExt v5 `Seq.Head` is `Option<IdsFacet>`, so the seed predicate reads through `Match` (the empty arm is
        // the every-object set) rather than `.ToPredicate` on the Option; the non-empty arm folds the tail with `And`.
        ElementSet applicable = Applicability.Head.Match(
            None: () => new ElementSet(graph, toHashSet(graph.ObjectNodes.Map(static o => o.Id))),
            Some: head => ElementSet.Query(graph, Applicability.Tail.Fold(
                ElementQuery.Of(head.ToPredicate(graph)),
                (q, facet) => q.And(facet.ToPredicate(graph)))));
        Seq<IdsAudit.FacetVerdict> verdicts = Requirements.Map(req => {
            ElementSet matched = applicable.Where(req.Facet.ToPredicate(graph));
            (ElementSet pass, ElementSet fail) = req.Cardinality.Partition(matched, applicable);
            return new IdsAudit.FacetVerdict(req.Facet, req.Cardinality, pass.GlobalIds, fail.GlobalIds);
        });
        return new IdsAudit(Name, Cardinality, applicable.Count, verdicts);
    }

    // The IDS document parse composes Xbim.InformationSpecifications `Xids` (the buildingSMART IDS v1.0 schema
    // binding) and is the ONE boundary that lowers every facet's `ValueConstraint` onto seam-typed data — retiring
    // the hand-rolled XmlReaderSettings.Schemas/XDocument parser. The fault lifts BARE (band 2600 IS the Code).
    public static Fin<Seq<IdsSpecification>> Parse(ReadOnlyMemory<byte> idsBytes, Op key) =>
        Try.lift(() => {
            using MemoryStream stream = new(idsBytes.ToArray());
            Xids xids = Xids.LoadBuildingSmartIDS(stream, NullLogger.Instance)
                ?? throw new InvalidDataException("ids-load-empty");
            return xids.AllSpecifications().ToSeq().Map(Project);
        }).Run().MapFail(error => new BimFault.ModelRejected(key, $"ids-parse:{error.Message}"));

    static IdsSpecification Project(Specification spec) =>
        new(spec.Name ?? "",
            Facets(spec.Applicability),
            Requirements(spec.Requirement),
            Cardinality(spec.Cardinality));

    static Seq<IdsFacet> Facets(FacetGroup? group) =>
        Optional(group).Map(static g => g.Facets.ToSeq().Choose(FacetOf)).IfNone(Seq<IdsFacet>());

    // Each requirement facet carries its OWN cardinality (Expected/Prohibited/Optional), read off the requirement
    // FacetGroup per facet through GetRequirementCardinalityOption — NOT the spec-level ICardinality applied to all
    // (the deleted conflation): a spec can require one facet, prohibit another, and leave a third optional in one pass.
    static Seq<IdsRequirement> Requirements(FacetGroup? group) =>
        Optional(group).Map(static g => g.Facets.ToSeq().Choose(facet =>
            FacetOf(facet).Map(lowered => new IdsRequirement(lowered, RequirementCardinality(g, facet)))))
            .IfNone(Seq<IdsRequirement>());

    static IdsCardinality RequirementCardinality(FacetGroup group, IFacet facet) =>
        group.GetRequirementCardinalityOption(facet, out RequirementCardinalityOptions.Cardinality? card) && card is { } c
            ? c switch {
                RequirementCardinalityOptions.Cardinality.Expected   => IdsCardinality.Required,     // the IDS default: the facet must match
                RequirementCardinalityOptions.Cardinality.Prohibited => IdsCardinality.Prohibited,
                RequirementCardinalityOptions.Cardinality.Optional   => IdsCardinality.Optional,
                _                                                    => IdsCardinality.Required,     // forward-compat: an unknown future cardinality is treated as required
            }
            : IdsCardinality.Required;

    // The boundary map: each Xbim FacetBase facet -> the closed IdsFacet arm, every ValueConstraint lowered HERE
    // (Matches/ResolveClasses/ClassificationBranches) so the interior is seam-typed. A facet the query algebra
    // cannot express (a pattern-named property set, an unresolvable object attribute, a system-only classification)
    // returns None and is dropped, so the audit never evaluates an inexpressible facet as a false failure.
    static Option<IdsFacet> FacetOf(IFacet facet) => facet switch {
        IfcTypeFacet f           => Some((IdsFacet)new IdsFacet.Entity(ResolveClasses(f.IfcType, f.IncludeSubtypes), Predefineds(f.PredefinedType))),
        AttributeFacet f         => SingleValue(f.AttributeName).Bind(name => ObjectAttribute.TryGet(name))
                                        .Map(attr => (IdsFacet)new IdsFacet.Attribute(attr, Matches(f.AttributeValue, None))),
        IfcPropertyFacet f       => from set in SingleValue(f.PropertySetName)
                                    from name in SingleValue(f.PropertyName)
                                    select (IdsFacet)new IdsFacet.Property(set, PropertyName.Create(name), Matches(f.PropertyValue, Optional(f.DataType))),
        IfcClassificationFacet f => ClassificationFacet(f),
        MaterialFacet f          => Some((IdsFacet)new IdsFacet.Material(Matches(f.Value, None))),
        PartOfFacet f            => PartOfFacetOf(f),
        _                        => Option<IdsFacet>.None,
    };

    static Option<IdsFacet> ClassificationFacet(IfcClassificationFacet f) =>
        ClassificationBranches(f) is { IsEmpty: false } branches
            ? Some((IdsFacet)new IdsFacet.Classification(branches))
            : Option<IdsFacet>.None;

    // A PartOf facet is admitted ONLY for a relation the query incidence vocabulary can express (Contained →
    // BySpatialContainer, Grouped → ByZone); an Aggregated/Nested/Voided relation — and an Undefined parse-fail
    // (MapRelation → None) — is DROPPED here rather than mis-lowered onto the Contain-only BySpatialContainer arm a
    // decomposition/void/unparseable edge never satisfies (which would fail every applicable element as a false
    // negative), so the audit never raises a false failure — the page's drop-the-inexpressible-facet doctrine,
    // lifted to precise predicates when the query grows ByComposed/ByVoid.
    static Option<IdsFacet> PartOfFacetOf(PartOfFacet f) =>
        MapRelation(f.GetRelation())
            .Filter(static r => r is PartOfRelation.Contained or PartOfRelation.Grouped)
            .Map(relation => (IdsFacet)new IdsFacet.PartOf(
                Optional(f.EntityType).Map(static t => (IdsFacet)new IdsFacet.Entity(ResolveClasses(t.IfcType, t.IncludeSubtypes), Predefineds(t.PredefinedType))),
                relation));

    // Resolve an Xbim IfcType ValueConstraint to the seam IfcClass set, expanding each accepted entity name to its
    // ids-lib concrete subtypes when IncludeSubtypes (the buildingSMART default), dropping a name the IfcClass
    // roster does not carry (the projector stamps only rostered keys, so an unrostered facet selects nothing anyway).
    static Seq<IfcClass> ResolveClasses(ValueConstraint? type, bool includeSubtypes) =>
        ExactValues(type)
            .Bind(name => includeSubtypes ? IdsSchema.ConcreteClasses(name).Add(name) : Seq1(name))
            .Distinct()
            .Choose(IfcClass.TryGet);

    static Seq<PredefinedType> Predefineds(ValueConstraint? predefined) =>
        ExactValues(predefined).Filter(static t => !string.IsNullOrWhiteSpace(t)).Map(static t => PredefinedType.Create(t));

    // Resolve the IDS classification system name through the Semantics/classification#CLASSIFICATION_AXIS roster to
    // the same canonical token the projector stamps onto Object.Classification, then mint a seam Classification
    // branch per identification code. The query ByClassification uses Within (prefix), which realizes the
    // IncludeSubClasses=true sub-branch inclusion (and the equal-code exact case); the strict IncludeSubClasses=false
    // exact-only match over a parent code awaits a query-algebra exact arm. A facet with no system or no code yields no branch.
    static Seq<SeamClassification> ClassificationBranches(IfcClassificationFacet f) =>
        SingleValue(f.ClassificationSystem).Filter(static s => !string.IsNullOrWhiteSpace(s)).Map(ResolveSystem).Match(
            Some: system => ExactValues(f.Identification).Filter(static c => !string.IsNullOrWhiteSpace(c)).Map(code => SeamClassification.Create(system, code, None)),
            None: static () => Seq<SeamClassification>());

    static string ResolveSystem(string name) =>
        ClassificationSystem.Items.ToSeq()
            .Find(s => string.Equals(s.Title, name, StringComparison.OrdinalIgnoreCase) || string.Equals(s.Key, name, StringComparison.OrdinalIgnoreCase))
            .Map(static s => s.Key)
            .IfNone(name.Trim().ToLowerInvariant());

    // The foreign PartOfRelation -> the seam-neutral PartOfRelation, EXPLICIT over every real Xbim member so the
    // parse-fail sentinel (PartOfFacet.GetRelation() returns Undefined when EntityRelation is absent/unparseable) is
    // NOT conflated with the legitimate IfcRelContainedInSpatialStructure: Undefined (and any future foreign value)
    // drops to None so PartOfFacetOf never mis-lowers an unparseable relation onto the Contain-only BySpatialContainer.
    static Option<PartOfRelation> MapRelation(PartOfFacet.PartOfRelation relation) => relation switch {
        PartOfFacet.PartOfRelation.IfcRelAggregates                  => Some(PartOfRelation.Aggregated),
        PartOfFacet.PartOfRelation.IfcRelNests                       => Some(PartOfRelation.Nested),
        PartOfFacet.PartOfRelation.IfcRelAssignsToGroup              => Some(PartOfRelation.Grouped),
        PartOfFacet.PartOfRelation.IfcRelVoidsFillsElement           => Some(PartOfRelation.Voided),
        PartOfFacet.PartOfRelation.IfcRelContainedInSpatialStructure => Some(PartOfRelation.Contained),
        _                                                            => None,
    };

    // The SPEC-level cardinality (the whole specification's applicability rule), distinct from the per-facet
    // requirement cardinality above; carried on IdsSpecification.Cardinality and ENFORCED by IdsAudit.Conforms
    // through SpecSatisfied. The Xbim ICardinality truth table is (ExpectsRequirements, AllowsRequirements):
    // Optional=(true,true), Required=(false,true), Prohibited=(false,false) — so Prohibited is the no-requirements
    // case, Optional the ExpectsRequirements case, and Required the remainder (a naive Expects:false→Optional read
    // swaps Required and Optional).
    static IdsCardinality Cardinality(ICardinality? cardinality) =>
        cardinality is { AllowsRequirements: false } ? IdsCardinality.Prohibited
        : cardinality is { ExpectsRequirements: true } ? IdsCardinality.Optional
        : IdsCardinality.Required;

    // --- [VALUE_LOWERING] -----------------------------------------------------------------
    // The IDS value engine, lowered ONCE: a ValueConstraint's accepted components fold onto the seam ValueMatch
    // family the query decides — an absent/empty constraint is Present (existence), an all-exact set collapses to
    // one OneOf, otherwise each component lowers to its own ValueMatch the predicate ORs. A ValueConstraint never
    // crosses into the interior and IsSatisfiedBy is never called — the query's typed ValueMatch is the only matcher.
    static Seq<ValueMatch> Matches(ValueConstraint? constraint, Option<string> dataType) {
        Seq<IValueConstraintComponent> components =
            Optional(constraint).Bind(static c => Optional(c.AcceptedValues)).Map(static a => a.ToSeq()).IfNone(Seq<IValueConstraintComponent>());
        if (components.IsEmpty) {
            return Seq1(ValueMatch.Any);
        }
        Seq<string> exacts = components.Choose(static c => c is ExactConstraint e ? Some(e.Value) : Option<string>.None);
        return exacts.Count == components.Count
            ? Seq1<ValueMatch>(new ValueMatch.OneOf(exacts))
            : components.Map(c => Lower(c, dataType));
    }

    static ValueMatch Lower(IValueConstraintComponent component, Option<string> dataType) => component switch {
        ExactConstraint e     => new ValueMatch.OneOf(Seq1(e.Value)),
        PatternConstraint p   => new ValueMatch.Pattern(p.Pattern),
        RangeConstraint r     => new ValueMatch.Range(Bound(r.MinValue, dataType), Bound(r.MaxValue, dataType), r.MinInclusive, r.MaxInclusive),
        StructureConstraint s => new ValueMatch.Length(
                                     Optional(s.Length).Match(Some: static l => Some(l), None: () => Optional(s.MinLength)),
                                     Optional(s.Length).Match(Some: static l => Some(l), None: () => Optional(s.MaxLength))),
        _                     => ValueMatch.Any,
    };

    // A numeric range bound carries its Dimension so the query's dimension-checked InRange can compare it against an
    // SI-coerced candidate measure: the IDS DataType (IfcLengthMeasure, …) resolves through ids-lib to the SI 7-vector
    // dimension; a bound without a measure datatype is Dimensionless (it compares against a Count candidate).
    static Option<MeasureValue> Bound(string? raw, Option<string> dataType) =>
        from text in Optional(raw)
        from value in ParseDouble(text)
        select dataType.Bind(IdsSchema.DimensionOf).Match(
            Some: d => MeasureValue.OfSi(d, value),
            None: () => MeasureValue.OfSi(Dimension.Dimensionless, value));

    static Option<double> ParseDouble(string text) =>
        double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out double value) ? Some(value) : None;

    // The exact accepted literals of a ValueConstraint (the xs:enumeration / single-value components) — the class
    // names, predefined tokens, classification codes, and pset/property names a structural arm needs as concrete keys.
    static Seq<string> ExactValues(ValueConstraint? constraint) =>
        Optional(constraint).Bind(static c => Optional(c.AcceptedValues)).Map(static a => a.ToSeq()).IfNone(Seq<IValueConstraintComponent>())
            .Choose(static c => c is ExactConstraint e ? Some(e.Value) : Option<string>.None);

    static Option<string> SingleValue(ValueConstraint? constraint) => ExactValues(constraint).Head;

    // The IDS-FILE audit (the spec document's own validity) is the buildingSMART-official ids-lib engine, orthogonal
    // to the MODEL audit: Audit.Run validates the .ids against the IDS v1.0 XSD + implementation agreements, the
    // Status flagging pass/fail and a BufferingLogger capturing the per-issue diagnostics off the engine's channel.
    public static Fin<IdsFileAudit> AuditFile(ReadOnlyMemory<byte> idsBytes, Op key) =>
        Try.lift(() => {
            using MemoryStream stream = new(idsBytes.ToArray());
            BufferingLogger sink = new();
            global::IdsLib.Audit.Status status = global::IdsLib.Audit.Run(stream, new SingleAuditOptions { IdsVersion = IdsVersion.Ids1_0 }, sink);
            return new IdsFileAudit(status, LibraryInformation.AssemblyVersion, sink.Drain());
        }).Run().MapFail(error => new BimFault.ModelRejected(key, $"ids-file-audit:{error.Message}"));
}

// One captured ids-lib audit diagnostic (the level + rendered message off the engine's ILogger channel) — the
// line-level evidence the IDS-FILE audit carries so a non-conformance reads its reason, never a bare boolean.
public readonly record struct IdsDiagnostic(LogLevel Level, string Message);

// The IDS-FILE audit receipt: the ids-lib Status, the captured diagnostics, and the engine build the audit ran
// under so a stored receipt is reproducible. Status.Ok (the zero flag) is the pass; any error flag is the reject.
public sealed record IdsFileAudit(global::IdsLib.Audit.Status Status, string EngineVersion, Seq<IdsDiagnostic> Diagnostics) {
    public bool Conforms => Status == global::IdsLib.Audit.Status.Ok;
    public Seq<IdsDiagnostic> Errors => Diagnostics.Filter(static d => d.Level >= LogLevel.Error);
}

public sealed record IdsAudit(string Specification, IdsCardinality SpecCardinality, int ApplicableCount, Seq<IdsAudit.FacetVerdict> Verdicts) {
    public sealed record FacetVerdict(IdsFacet Facet, IdsCardinality Cardinality, Seq<string> Passed, Seq<string> Failed);

    // Conformance is BOTH the spec-level applicable-count rule (a Required spec needs >=1 applicable element, a
    // Prohibited spec none — the Xbim ICardinality.IsSatisfiedBy truth) AND every requirement verdict passing; the
    // spec-level rule is enforced here rather than left as a stored-but-unread field.
    public bool Conforms => SpecCardinality.SpecSatisfied(ApplicableCount) && Verdicts.ForAll(static v => v.Failed.IsEmpty);

    // The cross-tool outer join on the (GlobalId, FacetKey) axis: the self-audit verdict rows against the conformant
    // ifctester oracle rows for THIS specification, each key carrying the optional self/oracle pass so a divergence
    // (a row the two tools disagree on, or one tool reaches and the other does not) lands one typed IdsParity.Row.
    public IdsParity Reconcile(Seq<IdsVerdict> oracle) {
        Seq<((string GlobalId, string Facet), bool Passed)> mine = Verdicts.Bind(v =>
            v.Passed.Map(g => ((g, v.Facet.FacetKey), true)) + v.Failed.Map(g => ((g, v.Facet.FacetKey), false)));
        Seq<((string GlobalId, string Facet), bool Passed)> theirs = oracle
            .Filter(o => o.Specification == Specification)
            .Map(o => ((o.GlobalId, o.Facet), o.Passed));
        HashMap<(string, string), bool> mineMap = mine.ToHashMap(static r => r.Item1, static r => r.Passed);
        HashMap<(string, string), bool> theirMap = theirs.ToHashMap(static r => r.Item1, static r => r.Passed);
        Seq<(string, string)> keys = (mine.Map(static r => r.Item1) + theirs.Map(static r => r.Item1)).Distinct();
        return new IdsParity(Specification,
            keys.Map(key => new IdsParity.Row(key.Item1, key.Item2, mineMap.Find(key), theirMap.Find(key))));
    }
}

// The per-(GlobalId, facet) cross-runtime audit row — OWNED HERE (Bim is the IDS authority and cannot reference the
// up-stratum Rasm.Compute) and COMPOSED by the csharp:Rasm.Compute/Runtime/codecs#TWO_HOP_TESSELLATION IdsAuditRequest
// leg, which projects the IfcOpenShell ifctester oracle into this exact shape; the one cross-runtime audit row.
public readonly record struct IdsVerdict(string GlobalId, string Specification, string Facet, bool Passed, string Reason);

public sealed record IdsParity(string Specification, Seq<IdsParity.Row> Rows) {
    public readonly record struct Row(string GlobalId, string Facet, Option<bool> SelfAudit, Option<bool> Oracle) {
        public bool Agrees => SelfAudit.Match(s => Oracle.Match(o => s == o, static () => false), static () => false);
    }

    public Seq<Row> Divergences => Rows.Filter(static r => !r.Agrees);

    public bool Conformant => Divergences.IsEmpty;
}

// --- [SERVICES] ---------------------------------------------------------------------------
// The boundary capture kernel: a buffering ILogger draining the ids-lib Audit.Run per-issue channel into typed
// IdsDiagnostic rows (the one mutable accumulation, contained at the logging boundary the ids-lib engine writes to).
// Only Warning+ issues are captured (Information is progress noise); the no-op scope satisfies the ILogger contract.
file sealed class BufferingLogger : ILogger {
    readonly System.Collections.Generic.List<IdsDiagnostic> sink = [];

    public Seq<IdsDiagnostic> Drain() => sink.ToSeq();
    public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Warning;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) {
        if (logLevel >= LogLevel.Warning) {
            sink.Add(new IdsDiagnostic(logLevel, formatter(state, exception)));
        }
    }

    sealed class NullScope : IDisposable {
        public static readonly NullScope Instance = new();
        public void Dispose() { }
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
// The offline IFC schema authority (ids-lib IdsLib.IfcSchema): resolves a facet's entity-subtype expansion and a
// range bound's measure dimension against the real IFC4.3 schema graph so a facet is validated against the actual
// schema, not a hard-coded class list — GetConcreteClassesFrom expands an entity to its concrete leaves for
// IncludeSubtypes, TryGetMeasureInformation maps an IFC measure datatype onto the SI 7-vector Dimension.
public static class IdsSchema {
    public static Seq<string> ConcreteClasses(string topClass) =>
        SchemaInfo.GetConcreteClassesFrom(topClass, IfcSchemaVersions.Ifc4x3).ToSeq();

    // The IFC measure datatype -> seam Dimension: ids-lib carries each measure's SI dimensional exponents, lowered
    // onto the seam Dimension value-object so a range bound built from the facet DataType shares the candidate's
    // dimension (a length bound never satisfies a pressure candidate) — the same SI metadata the property store reads.
    public static Option<Dimension> DimensionOf(string ifcDataType) =>
        SchemaInfo.TryGetMeasureInformation(ifcDataType, out IfcMeasureInformation? info) && info is { Exponents: { } e }
            ? Some(Dimension.Create(e.Length, e.Mass, e.Time, e.ElectricCurrent, e.Temperature, e.AmountOfSubstance, e.LuminousIntensity))
            : Option<Dimension>.None;
}
```

## [03]-[RESEARCH]

- [IDS_SPEC_MODEL]: the IDS v1.0 spec model is the `Xbim.InformationSpecifications` `Xids` document (`.api/api-xbim-informationspecifications`) — `Xids.LoadBuildingSmartIDS(Stream, ILogger?)` parses the buildingSMART IDS XML, `AllSpecifications()` enumerates each `Specification` (an applicability `FacetGroup` + a requirement `FacetGroup` + an `ICardinality`), and the six `: FacetBase, IFacet` facets carry their match fields as `ValueConstraint`s: `IfcTypeFacet` (`IfcType`/`PredefinedType : ValueConstraint?` + `IncludeSubtypes : bool`, default true), `AttributeFacet` (`AttributeName`/`AttributeValue : ValueConstraint?`), `IfcPropertyFacet` (`PropertySetName`/`PropertyName`/`PropertyValue : ValueConstraint?` + `DataType : string?`), `IfcClassificationFacet` (`ClassificationSystem`/`Identification : ValueConstraint?` + `IncludeSubClasses : bool`), `MaterialFacet` (`Value : ValueConstraint?`), and `PartOfFacet` (`EntityType : IfcTypeFacet?`, `EntityRelation : string` resolved through `GetRelation() : PartOfRelation` over `IfcRelAggregates`/`IfcRelAssignsToGroup`/`IfcRelContainedInSpatialStructure`/`IfcRelNests`/`IfcRelVoidsFillsElement`); so `FacetOf` admits each `IFacet` once at parse, reads its `ValueConstraint`s, and lowers them onto the closed `IdsFacet` union arm — the names extracted as the single exact value, the values lowered to `ValueMatch`, the entity classes resolved to the `IfcClass` roster — never a stringly `f.IfcType?.ToString()`, never a `ValueConstraint` crossing into the interior; each requirement facet's cardinality is read PER FACET through `FacetGroup.GetRequirementCardinalityOption(facet, out RequirementCardinalityOptions.Cardinality?)` (`Expected`/`Prohibited`/`Optional`, the IDS default `Expected`) rather than the spec-level `ICardinality` applied to every requirement (the deleted conflation), so a single specification can require, prohibit, and optionally-check distinct facets in one audit.
- [VALUE_LOWERING]: the IDS value engine lowers onto the seam `Model/query#ELEMENT_SET` `ValueMatch` family (`.api/api-xbim-informationspecifications`) — `ValueConstraint.AcceptedValues : List<IValueConstraintComponent>?` is satisfied if ANY component matches, so an all-`ExactConstraint` set collapses to one `ValueMatch.OneOf` (the `ExactConstraint.Value` literals), a `PatternConstraint.Pattern` to `ValueMatch.Pattern` (the whole-value-anchored NonBacktracking regex the query compiles), a `RangeConstraint` (`MinValue`/`MinInclusive`/`MaxValue`/`MaxInclusive`) to `ValueMatch.Range`, and a `StructureConstraint` (`Length`/`MinLength`/`MaxLength`) to `ValueMatch.Length`; the range bound carries its `Dimension` resolved through `ids-lib` `SchemaInfo.TryGetMeasureInformation(dataType, out IfcMeasureInformation)` → `IfcMeasureInformation.Exponents : DimensionalExponents` (`Length`/`Mass`/`Time`/`ElectricCurrent`/`Temperature`/`AmountOfSubstance`/`LuminousIntensity`) → seam `Dimension.Create` so the query's dimension-checked `InRange` compares like-for-like; `IncludeSubtypes` expands the entity via `SchemaInfo.GetConcreteClassesFrom(topClass, IfcSchemaVersions.Ifc4x3)` and `IncludeSubClasses` rides the seam `Classification.Within` prefix containment — so the lowering composes the real IFC4.3 schema, not a hard-coded list, and `ValueConstraint.IsSatisfiedBy` is never called (the typed `ValueMatch` is the only matcher, the retired second value engine the deleted form).
- [IDS_FILE_SCHEMA_AUTHORITY]: the IDS-FILE audit and the IFC schema truth are the buildingSMART-official `ids-lib` (`.api/api-ids-lib`) — `Audit.Run(Stream, SingleAuditOptions { IdsVersion = IdsVersion.Ids1_0 }, ILogger)` validates the `.ids` document against the IDS v1.0 XSD plus the implementation agreements onto a `[Flags]` `Audit.Status` (`Ok = 0` the pass, any error flag the reject), a `BufferingLogger` capturing the per-issue rows off the `ILogger` channel (the `AuditHelper.BufferedValidationIssue` level/message diagnostics) into the `IdsFileAudit` receipt and `LibraryInformation.AssemblyVersion` stamping the engine build for reproducibility; the embedded `IdsLib.IfcSchema` `SchemaInfo` (`GetConcreteClassesFrom`/`TryGetMeasureInformation`) is the offline IFC4.3 schema graph the `IdsSchema` helper resolves a facet's references against, so the property-template/measure metadata reconciles with the `Semantics/properties#PROPERTY_TEMPLATES` `UnitsNet` SI coercion — `ids-lib` supplies the schema truth, the seam `MeasureValue` the value conversion.
- [IFCTESTER_COMPANION]: the cross-runtime seam shape is settled and Bim-owned — `IdsVerdict(GlobalId, Specification, Facet, Passed, Reason)` is declared HERE because `Rasm.Bim` is the IDS authority and the strata forbid an AEC-DOMAIN owner referencing the APP-PLATFORM `Rasm.Compute`; the `csharp:Rasm.Compute/Runtime/codecs#TWO_HOP_TESSELLATION` `IdsAuditRequest` leg (APP-PLATFORM, depends up on Bim) COMPOSES this row, projecting the IfcOpenShell ifctester oracle (the `python:geometry/ifc-companion` `ids` oracle reached over Compute's existing companion rpc, identical to the `Exchange/tessellation#TESSELLATION_BRIDGE` two-hop pattern) into it, and `IdsAudit.Reconcile` folds the returned rows against the in-process self-audit on the (GlobalId, `FacetKey`) join into the typed `IdsParity` receipt — the in-process fold the immediate self-audit, the companion the external-conformance oracle, never a Compute-side `IdsAudit` re-projection and never a transport minted here.
- [PREDICATE_GAPS]: two facet shapes exceed the current `Model/query#ELEMENT_SET` incidence vocabulary and are DROPPED at parse rather than handled with a raw `Func<Node.Object,bool>` walk (the query boundary's deleted form) OR mis-lowered onto a wrong-semantics arm — a `PartOf` facet over `Aggregated`/`Nested`/`Voided` is dropped (NOT folded onto `BySpatialContainer`: that arm matches only the `Compose{Contain}` sub-kind, which a decomposition/aggregation/void edge never carries, so the mis-lowering would fail every applicable element as a false negative the drop-the-inexpressible-facet doctrine forbids) until the query algebra grows a `ByComposed(NodeId, ComposeKind)` / `ByVoid(NodeId)` arm reading the `Rasm.Element/Relations/relation#EDGE_ALGEBRA` `Compose`/`Void` sub-kinds, and a system-only (no-identification) `IfcClassificationFacet` yields no `ByClassification` branch (which needs a code) and is likewise dropped at parse until the query grows a `ByClassificationSystem(string)` arm; both are cross-file additions to `Model/query#ELEMENT_SET`, after which the `IdsFacet` lowering folds them into precise predicates with no page-local change beyond widening the `PartOfFacetOf` guard plus the new arm reference (`MapRelation` already resolves all five `PartOfRelation` values, so the lift admits the currently-dropped relations and routes them to the new arms).
