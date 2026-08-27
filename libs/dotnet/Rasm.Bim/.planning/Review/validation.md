# [BIM_VALIDATION]

The three-tier model-QA owner exposes one model-health verdict over the frozen element graph. The STRUCTURAL tier composes `Rasm.Element/Projection/audit#AUDIT_FOLD` `ModelAudit` whole, so neutral graph integrity and discipline coverage stay where the graph owns them. The BASELINE tier composes the spec-free `Semantics/properties#TEMPLATE_AUDIT` `TemplateFinding` stream beneath every authored specification. The AUTHORED tier carries buildingSMART IDS whole: `Parse` admits one `Xbim.InformationSpecifications` document and records every unliftable facet as a `DroppedFacet`; `Resolve` settles authored classification reach through `Semantics/classification#BSDD_RESOLUTION`; `Audit` grades the frozen IFC-visible universe through the one `Model/query#ELEMENT_SET` algebra; `Publish` raises the same family through `Xids`; and `AuditFile` runs the buildingSMART `ids-lib` document audit. Range bounds remain unit-safe through the IDS datatype or standard-Pset declaration into the `ids-lib` SI dimension. `ModelHealth.Audit` joins the structural, baseline, and authored facts into the closed `ModelFinding` family, and `Conforms` means no blocking finding. The page is host-local.

## [01]-[INDEX]

- [02]-[IDS_FACETS]: `RuleSeverity` the package-wide enforcement band, `IdsOutcome` the three-valued specification verdict, `DropReason`/`DroppedFacet` the typed unliftable-facet accounting, `ClassificationReach` the authored-subclass axis and its settled branch payload, `IdsFacet` the closed `[Union]` of contract-lowered facets (each `ToPredicate()` a graph-free `BimTerm`, the value a shared `ValueMatch`), `PartOfRelation` the relation policy rows, `IdsRequirement`/`IdsSpecification` the spec records, `IdsSpecification.Parse`/`Resolve`/`Publish` the three declared boundary steps, `IdsResolved` the graded-specification evidence and its total `Audit`, `IdsSpecification.AuditFile` over `ids-lib` `Audit`, the `IdsSchema` schema-parameterized offline authority, and the `IdsAudit` per-specification verdict.
- [03]-[MODEL_HEALTH]: `ModelHealth` the three-tier model-QA verdict and its `Audit` entry — the neutral shared `ModelAudit` structural grade and the baseline `TemplateFinding` stream composed beneath the authored per-spec `IdsAudit` fold under one threaded `TemplateScope` policy it records — `ModelFinding` the closed three-case verdict family carrying its `Severity` band, `Findings` the one flattened verdict stream, `Coordinate` the per-finding report group key, `Conforms` the one model-health verdict.

## [02]-[IDS_FACETS]

- Owner: `RuleSeverity` the package-wide `[SmartEnum<string>]` enforcement band whose row carries its own `Blocking` policy — declared HERE at the `Rasm.Bim` root and composed by `Review/coordination#COORDINATION`'s rule library from its child namespace, so one severity vocabulary spans model-check verdicts and IDS findings; `IdsOutcome` the three-valued specification verdict (`Conformant`/`NonConformant`/`Indeterminate`, the last the partially-graded state a dropped requirement facet forces); `DropReason` the closed vocabulary of lowering gaps and `DroppedFacet` its per-facet row carrying the `FacetGroup.FacetUse` role and the foreign `Short()` label; `ClassificationReach` the closed authored-subclass axis (`Exact` the declared codes alone, `Pending` an unsettled `IncludeSubClasses`, `Subsumed` the resolver's closed branch payload); `IdsSpecification` the specification record carrying the applicability and requirement facet sets, the cardinality, its OWN declared `IfcSchemaVersions` axis, its enforcement band, and the facets no lowering lifts; `IdsResolved` the graded-specification evidence `Resolve` alone mints and `Audit` alone accepts; `IdsFacet` the closed `[Union]` (Entity, Attribute, Property, Classification, Material, PartOf) each carrying CONTRACT-LOWERED data — resolved `IfcClass`/`PredefinedType` sets, `ValueMatch` name and value restrictions, resolved `Classification` branches — admitted ONCE at parse so the interior never sees an `Xbim` `ValueConstraint`; `PartOfRelation` the `[SmartEnum<string>]` relation POLICY ROWS, each carrying its query-arm lowering delegate AND its foreign `PartOfFacet.PartOfRelation` member as row data; each `IdsFacet` arm `ToPredicate()` lowering GRAPH-FREE to a `Model/query#ELEMENT_SET` `BimTerm`, `Presence()` deriving the value-widened conditional form the Optional cardinality partitions against, and `FacetKey(schema)` projecting the stable requirement token as a shared `ContentAddress` over the ONE kernel hasher; `IdsAudit` the deterministic per-specification verdict; `IdsFileAudit` the IDS-document validity verdict.
- Entry: `IdsSpecification.Parse(ReadOnlyMemory<byte> idsBytes)` admits an IDS XML document through `Xids.LoadBuildingSmartIDS`, reading each spec's own `Specification.IfcVersion` onto the `IfcSchemaVersions` axis through `IfcSchemaVersionHelper` and lowering every facet and every `ValueConstraint` onto the closed union, each unliftable facet leaving as a typed `DroppedFacet` and the SAME `GetAllowedCardinality` legality table gating the pairing HERE. `IdsSpecification.Resolve(Seq<IdsSpecification>, BsddPort, BsddPins, CancellationToken)` is the result-returning dictionary step. `IdsResolved.Audit(ElementGraph graph)` folds applicability into one `BimTerm` scoped to the IFC-visible `ExternalId`-bearing universe, queries through `ElementQuery.Query`, refines requirements through `ElementQuery.Where`, and partitions through `IdsCardinality.Partition`. `IdsSpecification.Publish` is the ingress inverse and `AuditFile` validates the document's own conformance through `ids-lib` `Audit.Run`.
- Auto: `Parse` is the value-lowering boundary and every gap it cannot lower leaves as a NAMED `DropReason` on an `Either` Left channel, never a `Choose`-discarded `None` — `Matches` folds a facet's `ValueConstraint.AcceptedValues` onto `Seq<ValueMatch>` (an all-exact set collapses to one `OneOf`, a `PatternConstraint` to `Pattern`, a `RangeConstraint` to a dimension-checked `Range` whose inclusivity rides the `RangeBound` arm, a pure length-bearing `StructureConstraint` to `Length`, a pure digits-bearing one to `Digits`, an absent constraint to `Present`; only a StructureConstraint MIXING the two axes drops, because one component lowers to one `ValueMatch` and splitting ORs two partial matches into a false PASS — beside it a bounds-crossed or exclusive-coincident range drops as `UnsatisfiableRange`); `NameMatch` lowers a NAME-position constraint to ONE `ValueMatch` so patterned names survive; `Predefineds` expands a patterned predefined token against the resolved classes' `IdsSchema.PredefinedTokens` roster through the ONE shared matcher; `DataTypeOf` resolves the range-bound datatype from the facet or the `PropertySetInfo.Get` standard-Pset declaration; `Numeric` coerces bound literals through `ValueConstraint.TryGetNetType`/`ParseValue` in the IFC datatype's value space; `ResolveClasses` expands an Entity facet's `IfcType` to its `IdsSchema.ConcreteClasses` subtypes when `IncludeSubtypes` and expands a PATTERNED entity name against `IdsSchema.ClassRoster`, an entity facet resolving to no rostered class dropping as `UnknownClass` rather than lowering to a match-nothing predicate a Prohibited requirement reads as a model-wide pass; `ClassificationBranches` resolves the system through the `Semantics/classification#CLASSIFICATION_AXIS` roster and admits each code through the shared `Classification.Of` door, the facet's own code set being the branch the shared arm decides SET MEMBERSHIP over — the SUB-branch expansion is the `Resolve` step's, never a code-prefix derivation; `Audit` then folds each facet's graph-free `ToPredicate()` — the validation fold reuses the query algebra for BOTH selection and value with one total `Switch` — and stamps each verdict's `FacetKey` ONCE under the spec's schema.
- Output: `IdsAudit` carries the specification name, the `Spec` document ordinal, the `Model` provenance digest (the shared `Projection/address#CONTENT_ADDRESS` `ContentAddress.OfGraph` snapshot address of the graph the fold ran over, so a stored verdict set names the model it graded and a re-audit after an edit re-keys), the spec-level `IdsCardinality`, the enforcement band, the applicable element count, the passed/failed `GlobalId` sets per facet with each facet's computed key, and the `DroppedFacet` rows; `IdsAudit.Outcome` is the three-valued verdict — `Indeterminate` whenever a facet dropped, else the spec-level applicable-count rule (`SpecSatisfied`) AND every requirement verdict passing — and `Conforms` reads its row column; `IdsFileAudit.Conforms`/`Errors` reads the `Status` and the captured diagnostics.
- Packages: Xbim.InformationSpecifications, ids-lib, Microsoft.Extensions.Logging.Abstractions, Rasm.Element (the shared `ElementGraph`, the `Query/predicate#ELEMENT_PREDICATE` algebra, and the kernel `CanonicalWriter` (`Rasm/Domain/identity#CONTENT_KEY`) and `Projection/address#CONTENT_ADDRESS` codec the facet key and the snapshot digest ride), Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm
- Growth: a new IDS facet is one `IdsFacet` union arm lowering to its `BimLeaf` or shared `ElementLeaf` arm plus its `Matches` lowering, its `Presence()` widening, its `Write` key contribution, and its `Raise` inverse; a new PartOf relation is one `PartOfRelation` row (delegate + foreign member — zero switch edits); a new value-match modality is one `ValueMatch` arm the contract already folds plus one `WriteMatch` arm ordinal; a new lowering gap is one `DropReason` row; a new enforcement band is one `RuleSeverity` row carrying its `Blocking` policy; a new cardinality is one `IdsCardinality` row carrying its `(matched, applicable, presence)` partition, spec rule, AND both authored inverses; a new dictionary reach modality is one `ClassificationReach` case with its `Descendants` and `Subsumes` arms; a new IFC schema version is already the `IfcSchemaVersions` flags axis each spec declares; never a second validation predicate surface, never a second value engine, never a hand-rolled IDS parser or writer, and never a transport minted here.
- Boundary: THE THREE STEPS ARE DECLARED AND THE ORDER IS TYPED — `Parse` decodes XML synchronously and reaches nothing, `Resolve` is the ONE dictionary hop, and `Audit` accepts `IdsResolved` alone, which `Resolve` alone mints; a caller-run expansion before the fold was REFUSED because an audit that grades correctly only when every caller remembers a prior step is ceremony pushed onto callers, and an `IncludeSubClasses` facet graded on its declared codes alone reports a clean pass over a branch it never covered. The `Model/query#ELEMENT_SET` `BimTerm` algebra is the ONLY validation selection surface and `ValueMatch` the ONLY value engine — `ByProperty`/`ByAttribute` carry the name restrictions natively, `PartOf` lowers recursively to `NodeMatch<ElementLeaf>.Where` with its container PROVED nested-lowerable at parse, and `Aggregated`/`Nested`/`Voided` lower through `ByComposed`/`ByVoided`. `Xids` owns IDS parsing and publishing, buildingSMART `ids-lib Audit.Run` owns IDS-FILE audit independently of model audit, and `IdsSchema` resolves entity, predefined, property, and measure facts from the real `IdsLib.IfcSchema` graph under the specification's OWN declared version — a pinned literal grades an IFC2X3 exchange against classes its schema never had. Non-standard Xbim `DocumentFacet` and `IfcRelationFacet` cases drop explicitly at parse; every unliftable facet becomes a `DroppedFacet` and makes its specification `Indeterminate`, as do unknown classes, unsatisfiable ranges, unresolvable branches, and empty prohibited grading. `??` survives at ONE site — the `Project` foreign-document transcription, where the Xbim record's nullable authoring columns admit; anywhere else on this page it is the deleted form. `IdsAudit` and `IdsFileAudit` are typed validation evidence, C# host-local, and neither mints a TypeScript family. Model audit reads the shared `ElementGraph` assembled by `SemanticProjector`.
- Events: an issued `IdsAudit` fires the `Model/observability#HOOKS` `rasm.bim.review.verdict` point with `BimFact.Verdict` — the specification name beside its document ORDINAL (IDS spec names are not unique, so the ordinal keeps two same-named specifications' verdicts apart), the `Model` provenance address the fold ran over, the tier, the `IdsOutcome` key, the `RuleSeverity` key, the finding tally, and the failing `GlobalId` set — at the `Audit` fold's own edge; the point is REPLAY modality so a late panel drains the recent window, and the CloudEvents announcement is `Exchange/events#EVENT_PROJECTION`'s observe subscription over it, subject `name#ordinal` matching the coordination key. Minting a verdict message envelope at this path is the deleted form.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using IdsLib;
using IdsLib.IfcSchema;
using LanguageExt;
using Rasm.Bim.Model;
using Rasm.Bim.Semantics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Rasm.Element.Classification;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Rasm.Element.Query;
using Rasm.Element.Relations;
using Thinktecture;
using Xbim.InformationSpecifications;
using Xbim.InformationSpecifications.Cardinality;
using static LanguageExt.Prelude;
using ElementClassification = Rasm.Element.Classification.Classification;
using BimTerm = Rasm.Element.Query.Predicate<Rasm.Bim.Model.BimLeaf>;
using ElementTerm = Rasm.Element.Query.Predicate<Rasm.Element.Query.ElementLeaf>;

namespace Rasm.Bim;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class RuleSeverity {
    public static readonly RuleSeverity Info    = new("info",    blocking: false);
    public static readonly RuleSeverity Warning = new("warning", blocking: false);
    public static readonly RuleSeverity Error   = new("error",   blocking: true);

    public bool Blocking { get; }
}

[SmartEnum<string>]
public sealed partial class IdsOutcome {
    public static readonly IdsOutcome Conformant    = new("conformant",     conforms: true);
    public static readonly IdsOutcome NonConformant = new("non-conformant", conforms: false);
    public static readonly IdsOutcome Indeterminate = new("indeterminate",  conforms: false);

    public bool Conforms { get; }
}

[SmartEnum<string>]
public sealed partial class DropReason {
    public static readonly DropReason UnknownClass        = new("unknown-class");
    public static readonly DropReason UnknownAttribute    = new("unknown-attribute");
    public static readonly DropReason MixedNameMatch      = new("mixed-name-match");
    public static readonly DropReason MixedStructure      = new("mixed-structure");
    public static readonly DropReason MalformedPattern    = new("malformed-pattern");
    public static readonly DropReason IllegalCardinality  = new("illegal-cardinality");
    public static readonly DropReason ExtendedFacet       = new("extended-facet");
    public static readonly DropReason EmptyClassification = new("empty-classification");
    public static readonly DropReason UnsatisfiableRange  = new("unsatisfiable-range");
    public static readonly DropReason NestedContainer     = new("nested-container");
    public static readonly DropReason UnresolvedBranch    = new("unresolved-branch");
}

public readonly record struct DroppedFacet(FacetGroup.FacetUse Role, string Facet, DropReason Reason);

[Union]
public abstract partial record ClassificationReach {
    private ClassificationReach() { }

    public sealed record Exact : ClassificationReach;
    public sealed record Pending : ClassificationReach;
    public sealed record Subsumed(Seq<ElementClassification> Descendants) : ClassificationReach;

    public static readonly ClassificationReach Declared = new Exact();
    public static readonly ClassificationReach Awaiting = new Pending();

    public Seq<ElementClassification> Codes => Switch(
        exact:    static _ => Seq<ElementClassification>(),
        pending:  static _ => Seq<ElementClassification>(),
        subsumed: static s => s.Descendants);

    public bool Subsumes => this is not Exact;
}

[SmartEnum<string>]
public sealed partial class IdsCardinality {
    public static readonly IdsCardinality Required   = new("required",   static (matched, applicable, _) => (matched, applicable.Except(matched)), static count => count > 0,  CardinalityEnum.Required,   RequirementCardinalityOptions.Cardinality.Expected);
    public static readonly IdsCardinality Prohibited = new("prohibited", static (matched, applicable, _) => (applicable.Except(matched), matched), static count => count == 0, CardinalityEnum.Prohibited, RequirementCardinalityOptions.Cardinality.Prohibited);
    public static readonly IdsCardinality Optional   = new("optional",   static (matched, applicable, present) => present().Except(matched) switch {
                                                           var violating => (applicable.Except(violating), violating),
                                                       }, static _ => true, CardinalityEnum.Optional, RequirementCardinalityOptions.Cardinality.Optional);

    public Func<ElementQuery, ElementQuery, Func<ElementQuery>, (ElementQuery Pass, ElementQuery Fail)> Partition { get; }
    public Func<int, bool> SpecSatisfied { get; }
    public CardinalityEnum Authored { get; }
    public RequirementCardinalityOptions.Cardinality AuthoredFacet { get; }
}

[SmartEnum<string>]
public sealed partial class PartOfRelation {
    public static readonly PartOfRelation Contained  = new("Contained",  PartOfFacet.PartOfRelation.IfcRelContainedInSpatialStructure, static t => new BimTerm.Leaf(new BimLeaf.BySpatialContainer(t, SpatialReach.Ancestry)));
    public static readonly PartOfRelation Aggregated = new("Aggregated", PartOfFacet.PartOfRelation.IfcRelAggregates,                  static t => BimLeaf.Of(new ElementLeaf.ByComposed(ComposeKind.Aggregate, t)));
    public static readonly PartOfRelation Nested     = new("Nested",     PartOfFacet.PartOfRelation.IfcRelNests,                       static t => BimLeaf.Of(new ElementLeaf.ByComposed(ComposeKind.Nest, t)));
    public static readonly PartOfRelation Grouped    = new("Grouped",    PartOfFacet.PartOfRelation.IfcRelAssignsToGroup,              BimLeaf.InZone);
    public static readonly PartOfRelation Voided     = new("Voided",     PartOfFacet.PartOfRelation.IfcRelVoidsFillsElement,           static t => new BimTerm.Any(Seq<BimTerm>(
                                                           BimLeaf.Of(new ElementLeaf.ByVoided(VoidKind.Void, t)), BimLeaf.Of(new ElementLeaf.ByVoided(VoidKind.Fill, t)))));

    public PartOfFacet.PartOfRelation Foreign { get; }

    [UseDelegateFromConstructor]
    public partial BimTerm Lower(NodeMatch<ElementLeaf> container);
}

[Union]
public partial record IdsFacet {
    partial record Entity(Seq<IfcClass> Classes, Seq<PredefinedType> Predefined);
    partial record Attribute(ValueMatch Name, Seq<ValueMatch> Value);
    partial record Property(ValueMatch Set, ValueMatch Name, Seq<ValueMatch> Value);
    partial record Classification(Seq<ElementClassification> Branches, Option<string> System, ClassificationReach Reach) {
        public Seq<ElementClassification> Selected => Branches + Reach.Codes;
    }
    partial record Material(Seq<ValueMatch> Value);
    partial record PartOf(Option<IdsFacet> Container, PartOfRelation Relation);


    public BimTerm ToPredicate() => Switch(
        entity:         static f => AnyOf(f.Classes.Bind(cls => f.Predefined.IsEmpty
                            ? Seq<BimTerm>(new BimTerm.Leaf(new BimLeaf.ByClass(cls)))
                            : f.Predefined.Map(pt => (BimTerm)new BimTerm.Leaf(new BimLeaf.ByPredefinedType(cls, pt))))),
        attribute:      static f => AnyOf(f.Value.Map(vm => BimLeaf.Of(new ElementLeaf.ByAttribute(f.Name, vm)))),
        property:       static f => AnyOf(f.Value.Map(vm => BimLeaf.Of(new ElementLeaf.ByProperty(f.Set, f.Name, vm)))),
        classification: static f => f.Selected.IsEmpty
                            ? f.System.Match(
                                Some: static s => (BimTerm)new BimTerm.Leaf(new BimLeaf.ByClassificationSystem(s)),
                                None: static () => new BimTerm.Any(Seq<BimTerm>()))
                            : BimLeaf.Classified(f.Selected),
        material:       static f => AnyOf(f.Value.Map(vm => BimLeaf.Of(new ElementLeaf.ByMaterial(vm)))),
        partOf:         static f => f.Relation.Lower(new NodeMatch<ElementLeaf>.Where(f.Container.Match(
                            Some: static c => Nested(c).IfNone(new ElementTerm.Any(Seq<ElementTerm>())),
                            None: static () => ElementTerm.Open))));

    internal static Option<ElementTerm> Nested(IdsFacet facet) => facet.Switch(
        entity:         static f => f.Predefined.IsEmpty
                            ? Some((ElementTerm)new ElementTerm.Leaf(new ElementLeaf.ByClassification(
                                f.Classes.Map(c => ElementClassification.Of(ElementQuery.IfcSystem, c.Key, Lowering).ThrowIfFail()))))
                            : Option<ElementTerm>.None,
        attribute:      static f => Some(AnyElement(f.Value.Map(vm => (ElementLeaf)new ElementLeaf.ByAttribute(f.Name, vm)))),
        property:       static f => Some(AnyElement(f.Value.Map(vm => (ElementLeaf)new ElementLeaf.ByProperty(f.Set, f.Name, vm)))),
        classification: static f => f.Selected.IsEmpty
                            ? Option<ElementTerm>.None
                            : Some((ElementTerm)new ElementTerm.Leaf(new ElementLeaf.ByClassification(f.Selected))),
        material:       static f => Some(AnyElement(f.Value.Map(vm => (ElementLeaf)new ElementLeaf.ByMaterial(vm)))),
        partOf:         static _ => Option<ElementTerm>.None);

    public IdsFacet Presence() => Switch(
        entity:         static f => (IdsFacet)f,
        attribute:      static f => f with { Value = Seq(ValueMatch.Any) },
        property:       static f => f with { Value = Seq(ValueMatch.Any) },
        classification: static f => f.System.IsSome
                            ? f with { Branches = Seq<ElementClassification>(), Reach = ClassificationReach.Declared }
                            : (IdsFacet)f,
        material:       static _ => new IdsFacet.Material(Seq(ValueMatch.Any)),
        partOf:         static f => (IdsFacet)f);

    public ContentAddress FacetKey(IfcSchemaVersions schema) =>
        ContentAddress.Of((Facet: this, Schema: schema), tolerance: 0.0, static (state, w) => state.Facet.Write(w, state.Schema));

    void Write(CanonicalWriter writer, IfcSchemaVersions schema) => Switch(
        state:          (Writer: writer, Schema: schema),
        entity:         static (w, f) => {
                            w.Writer.Ordinal(0).Ordinal(f.Classes.Count);
                            IdsSchema.Simplify(f.Classes.Map(static c => c.Key), w.Schema).Iter(name => w.Writer.String(name));
                            w.Writer.Ordinal(f.Predefined.Count);
                            f.Predefined.Iter(p => w.Writer.String(p.Token));
                        },
        attribute:      static (w, f) => WriteValues(w.Writer.Ordinal(1), f.Name, f.Value),
        property:       static (w, f) => WriteValues(WriteMatch(w.Writer.Ordinal(2), f.Set), f.Name, f.Value),
        classification: static (w, f) => {
                            w.Writer.Ordinal(3).String(f.System.IfNone(() => f.Branches.Head.Map(static b => b.System).IfNone(string.Empty)))
                                .Bool(f.Reach.Subsumes).Ordinal(f.Branches.Count);
                            f.Branches.Iter(b => w.Writer.String(b.Code));
                        },
        material:       static (w, f) => WriteValues(w.Writer.Ordinal(4), ValueMatch.Any, f.Value),
        partOf:         static (w, f) => {
                            w.Writer.Ordinal(5).String(f.Relation.Key).Bool(f.Container.IsSome);
                            f.Container.IfSome(c => c.Write(w.Writer, w.Schema));
                        });

    static CanonicalWriter WriteValues(CanonicalWriter writer, ValueMatch name, Seq<ValueMatch> value) {
        WriteMatch(writer, name).Ordinal(value.Count);
        value.Iter(match => WriteMatch(writer, match));
        return writer;
    }

    static CanonicalWriter WriteMatch(CanonicalWriter writer, ValueMatch match) => match switch {
        ValueMatch.OneOf o   => o.Allowed.Fold(writer.Ordinal(0).Ordinal(o.Allowed.Count), static (w, a) => w.String(a)),
        ValueMatch.Pattern p => writer.Ordinal(1).String(p.Expression),
        ValueMatch.Range r   => WriteBound(WriteBound(writer.Ordinal(2), r.Lower), r.Upper),
        ValueMatch.Length l  => WriteCount(WriteCount(writer.Ordinal(3), l.Min), l.Max),
        ValueMatch.Digits d  => WriteCount(WriteCount(writer.Ordinal(4), d.Total), d.Fraction),
        _                    => writer.Ordinal(5),
    };

    static CanonicalWriter WriteBound(CanonicalWriter writer, Option<RangeBound> bound) =>
        bound.Match(
            Some: b => b.Switch(
                state:     writer.Bool(true),
                inclusive: static (w, i) => w.Ordinal(0).Measure(i.Value),
                exclusive: static (w, e) => w.Ordinal(1).Measure(e.Value)),
            None: () => writer.Bool(false));

    static CanonicalWriter WriteCount(CanonicalWriter writer, Option<int> bound) =>
        bound.Match(Some: v => writer.Bool(true).Ordinal(v), None: () => writer.Bool(false));

    static BimTerm AnyOf(Seq<BimTerm> arms) =>
        arms.Count == 1 ? arms[0] : new BimTerm.Any(arms);

    static ElementTerm AnyElement(Seq<ElementLeaf> leaves) =>
        leaves.Count == 1 ? new ElementTerm.Leaf(leaves[0])
        : new ElementTerm.Any(leaves.Map(static l => (ElementTerm)new ElementTerm.Leaf(l)));
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record IdsRequirement(IdsFacet Facet, IdsCardinality Cardinality);

public sealed record IdsSpecification(
    string Name,
    Seq<IdsFacet> Applicability,
    Seq<IdsRequirement> Requirements,
    IdsCardinality Cardinality,
    IfcSchemaVersions Schema,
    RuleSeverity Severity,
    Seq<DroppedFacet> Dropped,
    int Ordinal = 0,
    string Description = "",
    string Instructions = "",
    string Identifier = "") {

    public static Fin<Seq<IdsSpecification>> Parse(ReadOnlyMemory<byte> idsBytes) =>
        Try.lift(() => {
            using MemoryStream stream = new(idsBytes.ToArray());
            return Optional(Xids.LoadBuildingSmartIDS(stream, NullLogger.Instance))
                .Map(static xids => toSeq(xids.AllSpecifications().Select(static (spec, i) => Project(spec) with { Ordinal = i })));
        }).Run().Bind(static inner => inner)
        .Bind(loaded => loaded.ToFin(
            new BimFault.Refused(BimScope.Review, BimReason.Rejected, string.Join(':', new object?[] { "ids-lane", "parse", "load-empty" }))));

    public static Fin<Seq<IdsResolved>> Resolve(Seq<IdsSpecification> specifications, BsddPort port, BsddPins pins, CancellationToken token) =>
        specifications.TraverseM(spec => Settle(spec, port, pins, token, key)).As();

    static Fin<IdsResolved> Settle(IdsSpecification spec, BsddPort port, BsddPins pins, CancellationToken token) =>
        from applicability in spec.Applicability
            .TraverseM(facet => Reached(facet, FacetGroup.FacetUse.Applicability, port, pins, token, key)).As()
        from requirements in spec.Requirements
            .TraverseM(req => Reached(req.Facet, FacetGroup.FacetUse.Requirement, port, pins, token, key)
                .Map(reached => reached.Map(facet => req with { Facet = facet }))).As()
        select IdsResolved.Of(spec with {
            Applicability = applicability.Bind(static e => e.RightToSeq()),
            Requirements = requirements.Bind(static e => e.RightToSeq()),
            Dropped = spec.Dropped + applicability.Bind(Dropped) + requirements.Bind(Dropped),
        });

    static Fin<Either<DroppedFacet, IdsFacet>> Reached(IdsFacet facet, FacetGroup.FacetUse role, BsddPort port, BsddPins pins, CancellationToken token) =>
        facet switch {
            IdsFacet.Classification { Reach: ClassificationReach.Pending } c =>
                Descend(c, port, pins, token, key)
                    .Map(reached => reached.MapLeft(reason => new DroppedFacet(role, c.Reach.Key, reason))),
            IdsFacet.PartOf p => p.Container.Match(
                Some: inner => Reached(inner, role, port, pins, token, key)
                    .Map(reached => reached.Map(settled => (IdsFacet)(p with { Container = Some(settled) }))),
                None: () => Fin.Succ(Right<DroppedFacet, IdsFacet>(p))),
            _ => Fin.Succ(Right<DroppedFacet, IdsFacet>(facet)),
        };

    static Fin<Either<DropReason, IdsFacet>> Descend(IdsFacet.Classification facet, BsddPort port, BsddPins pins, CancellationToken token) =>
        facet.Branches.TraverseM(branch => Children(branch, port, pins, token, key)).As()
            .Map(expanded => expanded.ForAll(static rows => rows.IsSome)
                ? Right<DropReason, IdsFacet>(facet with {
                    Reach = new ClassificationReach.Subsumed(expanded.Somes().Bind(static rows => rows).Distinct()),
                })
                : Left<DropReason, IdsFacet>(DropReason.UnresolvedBranch));

    static Fin<Option<Seq<ElementClassification>>> Children(ElementClassification branch, BsddPort port, BsddPins pins, CancellationToken token) =>
        toSeq(ClassificationSystem.Items).Find(row => string.Equals(row.Key, branch.System, StringComparison.OrdinalIgnoreCase))
            .Match(
                Some: system => BsddResolution.Resolve(system, branch.Code, port, pins, token, key)
                    .Map(resolved => resolved.Children
                        .Traverse(child => ElementClassification.Of(branch.System, child.Code, key).ToOption()).As()),
                None: static () => Fin.Succ(Option<Seq<ElementClassification>>.None));

    public static Fin<byte[]> Publish(Seq<IdsSpecification> specifications) =>
        from raised in specifications.Traverse(spec => Raisable(spec, key)).As()
        from bytes in Try.lift(() => Serialize(raised)).Run().Bind(static inner => inner)
        select bytes;

    static Fin<(IdsSpecification Spec, Seq<IFacet> Applicability, Seq<(IFacet Facet, RequirementCardinalityOptions Options)> Requirements)> Raisable(
        IdsSpecification spec) =>
        spec.Requirements
            .Traverse(req => Legal(new RequirementCardinalityOptions(Raise(req.Facet, spec.Schema), req.Cardinality.AuthoredFacet), req.Cardinality, key)).As()
            .Map(rows => (spec, spec.Applicability.Map(facet => Raise(facet, spec.Schema)), rows.Map(static row => (row.RelatedFacet, row))));

    static Fin<RequirementCardinalityOptions> Legal(RequirementCardinalityOptions row, IdsCardinality cardinality) =>
        row.GetAllowedCardinality().Contains(cardinality.AuthoredFacet)
            ? Fin.Succ(row)
            : new BimFault.Refused(BimScope.Review, BimReason.Rejected, string.Join(':', new object?[] { "cardinality-illegal", row.RelatedFacet.GetType().Name, cardinality.Key }));

    static byte[] Serialize(Seq<(IdsSpecification Spec, Seq<IFacet> Applicability, Seq<(IFacet Facet, RequirementCardinalityOptions Options)> Requirements)> raised) {
        Xids xids = new();
        foreach (var (spec, applicability, requirements) in raised) {
            Specification prepared = xids.PrepareSpecification(spec.Schema.FromIds());
            prepared.Name = spec.Name;
            prepared.Cardinality = new SimpleCardinality(spec.Cardinality.Authored);
            prepared.Description = spec.Description is { Length: > 0 } text ? text : null;
            prepared.Instructions = spec.Instructions is { Length: > 0 } notes ? notes : null;
            if (spec.Identifier is { Length: > 0 } identity) { prepared.Guid = identity; }
            foreach (IFacet facet in applicability) { prepared.Applicability.Facets.Add(facet); }
            FacetGroup requirement = prepared.Requirement!;
            requirement.Facets = new ObservableCollection<IFacet>(requirements.Map(static r => r.Facet));
            requirement.RequirementOptions = new ObservableCollection<RequirementCardinalityOptions>(requirements.Map(static r => r.Options));
        }
        using MemoryStream sink = new();
        xids.ExportBuildingSmartIDS(sink, NullLogger.Instance);
        return sink.ToArray();
    }

    static IdsSpecification Project(Specification spec) {
        IfcSchemaVersions schema = Optional(spec.IfcVersion).Map(static v => v.ToIds()).Filter(static s => s != IfcSchemaVersions.IfcNoVersion)
            .IfNone(IfcSchemaVersions.Ifc4x3);
        Seq<Either<DroppedFacet, IdsFacet>> applicability = Lowered(spec.Applicability, FacetGroup.FacetUse.Applicability, schema);
        Seq<Either<DroppedFacet, IdsRequirement>> requirements = Required(spec.Requirement, schema);
        return new(spec.Name ?? "",
            applicability.Bind(static e => e.RightToSeq()),
            requirements.Bind(static e => e.RightToSeq()),
            Cardinality(spec.Cardinality),
            schema,
            RuleSeverity.Error,
            applicability.Bind(Dropped) + requirements.Bind(Dropped),
            Ordinal: 0,
            Description: spec.Description ?? "",
            Instructions: spec.Instructions ?? "",
            Identifier: spec.Guid);
    }

    static Seq<DroppedFacet> Dropped<R>(Either<DroppedFacet, R> row) =>
        row.Match(Left: static d => Seq(d), Right: static _ => Seq<DroppedFacet>());

    static Seq<Either<DroppedFacet, IdsFacet>> Lowered(FacetGroup? group, FacetGroup.FacetUse role, IfcSchemaVersions schema) =>
        Optional(group).Map(g => toSeq(g.Facets).Map(facet => FacetOf(facet, schema).MapLeft(reason => new DroppedFacet(role, facet.Short(), reason))))
            .IfNone(Seq<Either<DroppedFacet, IdsFacet>>());

    static Seq<Either<DroppedFacet, IdsRequirement>> Required(FacetGroup? group, IfcSchemaVersions schema) =>
        Optional(group).Map(g => toSeq(g.Facets).Map(facet =>
            (from cardinality in Admissible(g, facet)
             from lowered in FacetOf(facet, schema)
             select new IdsRequirement(lowered, cardinality))
            .MapLeft(reason => new DroppedFacet(FacetGroup.FacetUse.Requirement, facet.Short(), reason))))
            .IfNone(Seq<Either<DroppedFacet, IdsRequirement>>());

    static Either<DropReason, IdsCardinality> Admissible(FacetGroup group, IFacet facet) =>
        RequirementCardinality(group, facet) switch {
            var cardinality when new RequirementCardinalityOptions(facet, cardinality.AuthoredFacet)
                .GetAllowedCardinality().Contains(cardinality.AuthoredFacet) => Right(cardinality),
            _                                                                => Left(DropReason.IllegalCardinality),
        };

    static IdsCardinality RequirementCardinality(FacetGroup group, IFacet facet) =>
        group.GetRequirementCardinalityOption(facet, out RequirementCardinalityOptions.Cardinality? card) && card is { } c
            ? c switch {
                RequirementCardinalityOptions.Cardinality.Expected   => IdsCardinality.Required,
                RequirementCardinalityOptions.Cardinality.Prohibited => IdsCardinality.Prohibited,
                RequirementCardinalityOptions.Cardinality.Optional   => IdsCardinality.Optional,
                _                                                    => IdsCardinality.Required,
            }
            : IdsCardinality.Required;

    static Either<DropReason, IdsFacet> FacetOf(IFacet facet, IfcSchemaVersions schema) => facet switch {
        IfcTypeFacet f                    => EntityOf(f, schema).Map(static e => (IdsFacet)e),
        AttributeFacet f                  => from name in NameMatch(f.AttributeName).Bind(name =>
                                                 toSeq(ObjectAttribute.Items).Exists(row => name.Matches(new PropertyValue.Text(row.Key)))
                                                     ? Right<DropReason, ValueMatch>(name) : Left(DropReason.UnknownAttribute))
                                             from value in Matches(f.AttributeValue, None)
                                             select (IdsFacet)new IdsFacet.Attribute(name, value),
        IfcPropertyFacet f                => from set in NameMatch(f.PropertySetName)
                                             from name in NameMatch(f.PropertyName)
                                             from value in Matches(f.PropertyValue, DataTypeOf(f, schema))
                                             select (IdsFacet)new IdsFacet.Property(set, name, value),
        IfcClassificationFacet f          => ClassificationFacet(f),
        MaterialFacet f                   => Matches(f.Value, None).Map(static value => (IdsFacet)new IdsFacet.Material(value)),
        PartOfFacet f                     => PartOfFacetOf(f, schema),
        _                                 => Left(DropReason.ExtendedFacet),
    };

    static Either<DropReason, IdsFacet.Entity> EntityOf(IfcTypeFacet f, IfcSchemaVersions schema) =>
        ResolveClasses(f.IfcType, f.IncludeSubtypes, schema) switch {
            { IsEmpty: true } => Left(DropReason.UnknownClass),
            var classes       => Right(new IdsFacet.Entity(classes, Predefineds(f.PredefinedType, classes, schema))),
        };

    static Either<DropReason, IdsFacet> ClassificationFacet(IfcClassificationFacet f) {
        Option<string> system = SingleValue(f.ClassificationSystem).Filter(static s => !string.IsNullOrWhiteSpace(s)).Map(ResolveSystem);
        Seq<ElementClassification> branches = ClassificationBranches(f);
        return branches.IsEmpty && system.IsNone
            ? Left(DropReason.EmptyClassification)
            : Right((IdsFacet)new IdsFacet.Classification(branches, system,
                f.IncludeSubClasses is true && !branches.IsEmpty ? ClassificationReach.Awaiting : ClassificationReach.Declared));
    }

    static Either<DropReason, IdsFacet> PartOfFacetOf(PartOfFacet f, IfcSchemaVersions schema) =>
        from relation in MapRelation(f.GetRelation()).ToEither(DropReason.ExtendedFacet)
        from container in Optional(f.EntityType).Match(
            Some: t => EntityOf(t, schema).Bind(e => IdsFacet.Nested(e).Match(
                Some: _ => Right<DropReason, Option<IdsFacet>>(Some((IdsFacet)e)),
                None: static () => Left<DropReason, Option<IdsFacet>>(DropReason.NestedContainer))),
            None: static () => Right<DropReason, Option<IdsFacet>>(None))
        select (IdsFacet)new IdsFacet.PartOf(container, relation);

    static Option<PartOfRelation> MapRelation(PartOfFacet.PartOfRelation relation) =>
        toSeq(PartOfRelation.Items).Find(row => row.Foreign == relation);

    static Seq<IfcClass> ResolveClasses(ValueConstraint? type, bool includeSubtypes, IfcSchemaVersions schema) =>
        Components(type)
            .Bind(component => component switch {
                ExactConstraint e   => Seq(e.Value),
                PatternConstraint p => ValueMatch.Pattern.Lift(p.Pattern).Map(matcher => Expand(matcher, IdsSchema.ClassRoster(schema))).IfNone(Seq<string>()),
                _                   => Seq<string>(),
            })
            .Bind(name => includeSubtypes ? IdsSchema.ConcreteClasses(name, schema).Add(name) : Seq(name))
            .Distinct()
            .Choose(IfcClass.TryGet);

    static Seq<PredefinedType> Predefineds(ValueConstraint? predefined, Seq<IfcClass> classes, IfcSchemaVersions schema) {
        Seq<IValueConstraintComponent> components = Components(predefined);
        Seq<string> roster = classes.Bind(c => IdsSchema.PredefinedTokens(c.Key, schema)).Distinct();
        return components.Bind(component => component switch {
                ExactConstraint e   => Seq(e.Value),
                PatternConstraint p => ValueMatch.Pattern.Lift(p.Pattern).Map(matcher => Expand(matcher, roster)).IfNone(Seq<string>()),
                _                   => Seq<string>(),
            })
            .Distinct().Filter(static t => !string.IsNullOrWhiteSpace(t)).Map(static t => PredefinedType.Create(t));
    }

    static Seq<string> Expand(ValueMatch matcher, Seq<string> roster) =>
        roster.Filter(token => matcher.Matches(new PropertyValue.Text(token)));

    static Seq<ElementClassification> ClassificationBranches(IfcClassificationFacet f) =>
        SingleValue(f.ClassificationSystem).Filter(static s => !string.IsNullOrWhiteSpace(s)).Map(ResolveSystem).Match(
            Some: system => ExactValues(f.Identification).Filter(static c => !string.IsNullOrWhiteSpace(c))
                                .Map(code => ElementClassification.Of(system, code, IdsFacet.Lowering).ThrowIfFail()),
            None: static () => Seq<ElementClassification>());

    static string ResolveSystem(string name) =>
        toSeq(ClassificationSystem.Items)
            .Find(s => string.Equals(s.Title, name, StringComparison.OrdinalIgnoreCase) || string.Equals(s.Key, name, StringComparison.OrdinalIgnoreCase))
            .Map(static s => s.Key)
            .IfNone(name.Trim().ToLowerInvariant());

    static IdsCardinality Cardinality(ICardinality? cardinality) =>
        cardinality is { AllowsRequirements: false } ? IdsCardinality.Prohibited
        : cardinality is { ExpectsRequirements: true } ? IdsCardinality.Optional
        : IdsCardinality.Required;

    // --- [VALUE_LOWERING] --------------------------------------------------------------
    static Either<DropReason, Seq<ValueMatch>> Matches(ValueConstraint? constraint, Option<string> dataType) {
        Seq<IValueConstraintComponent> components = Components(constraint);
        Seq<string> exacts = components.Choose(static c => c is ExactConstraint e ? Some(e.Value) : Option<string>.None);
        return components.Choose(c => Unliftable(c, dataType)).Head.Match(
            Some: Left<DropReason, Seq<ValueMatch>>,
            None: () => components.IsEmpty ? Right(Seq(ValueMatch.Any))
                : exacts.Count == components.Count ? Right(Seq<ValueMatch>(new ValueMatch.OneOf(exacts)))
                : Right(components.Map(c => Lower(c, dataType))));
    }

    static Either<DropReason, ValueMatch> NameMatch(ValueConstraint? constraint) {
        Seq<IValueConstraintComponent> components = Components(constraint);
        Seq<string> exacts = components.Choose(static c => c is ExactConstraint e ? Some(e.Value) : Option<string>.None);
        return components.Choose(static c => Unliftable(c, None)).Head.Match(
            Some: Left<DropReason, ValueMatch>,
            None: () => components.IsEmpty ? Right(ValueMatch.Any)
                : exacts.Count == components.Count ? Right<ValueMatch>(new ValueMatch.OneOf(exacts))
                : components.Tail.IsEmpty ? components.Head.Map(c => Lower(c, None)).ToEither(DropReason.MixedNameMatch)
                : Left(DropReason.MixedNameMatch));
    }

    static ValueMatch Lower(IValueConstraintComponent component, Option<string> dataType) => component switch {
        ExactConstraint e     => new ValueMatch.OneOf(Seq(e.Value)),
        PatternConstraint p   => ValueMatch.Pattern.Lift(p.Pattern).IfNone(ValueMatch.Any),
        RangeConstraint r     => new ValueMatch.Range(
            Bound(r.MinValue, dataType).Map(value => r.MinInclusive ? (RangeBound)new RangeBound.Inclusive(value) : new RangeBound.Exclusive(value)),
            Bound(r.MaxValue, dataType).Map(value => r.MaxInclusive ? (RangeBound)new RangeBound.Inclusive(value) : new RangeBound.Exclusive(value))),
        StructureConstraint s => s.TotalDigits is not null || s.FractionDigits is not null
                                     ? new ValueMatch.Digits(Optional(s.TotalDigits), Optional(s.FractionDigits))
                                     : new ValueMatch.Length(
                                           Optional(s.Length) | Optional(s.MinLength),
                                           Optional(s.Length) | Optional(s.MaxLength)),
        _                     => ValueMatch.Any,
    };

    static Option<DropReason> Unliftable(IValueConstraintComponent component, Option<string> dataType) =>
        component switch {
            StructureConstraint s when (s.TotalDigits is not null || s.FractionDigits is not null)
                                       && (s.Length is not null || s.MinLength is not null || s.MaxLength is not null) => Some(DropReason.MixedStructure),
            PatternConstraint p when ValueMatch.Pattern.Lift(p.Pattern).IsNone                                         => Some(DropReason.MalformedPattern),
            RangeConstraint r when Crossed(r, dataType)                                                                => Some(DropReason.UnsatisfiableRange),
            _                                                                                                         => None,
        };

    static bool Crossed(RangeConstraint r, Option<string> dataType) =>
        (from lo in Bound(r.MinValue, dataType)
         from hi in Bound(r.MaxValue, dataType)
         select lo.Si > hi.Si || (lo.Si == hi.Si && !(r.MinInclusive && r.MaxInclusive))).IfNone(false);

    static Option<MeasureValue> Bound(string? raw, Option<string> dataType) =>
        from text in Optional(raw)
        from value in Numeric(text, dataType)
        from bound in dataType.Bind(IdsSchema.DimensionOf).Match(
            Some: d => MeasureValue.OfSi(d, value),
            None: () => MeasureValue.OfSi(Dimension.Dimensionless, value)).ToOption()
        select bound;

    static Option<double> Numeric(string text, Option<string> dataType) =>
        dataType.Bind(dt => ValueConstraint.TryGetNetType(dt, out NetTypeName net)
                ? Optional(ValueConstraint.ParseValue(text, net))
                : Option<object>.None)
            .Bind(static parsed => parsed switch {
                double d  => Some(d),
                float f   => Some((double)f),
                int i     => Some((double)i),
                long l    => Some((double)l),
                decimal m => Some((double)m),
                _         => Option<double>.None,
            })
            .BiBind(Some: Some, None: () => ParseDouble(text));

    static Option<double> ParseDouble(string text) =>
        double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out double value) ? Some(value) : None;

    static Option<string> DataTypeOf(IfcPropertyFacet f, IfcSchemaVersions schema) =>
        Optional(f.DataType).Filter(static d => !string.IsNullOrWhiteSpace(d))
            .BiBind(Some: Some, None: () => from set in SingleValue(f.PropertySetName)
                                            from name in SingleValue(f.PropertyName)
                                            from declared in IdsSchema.StandardDatatype(set, name, schema)
                                            select declared);

    static Seq<IValueConstraintComponent> Components(ValueConstraint? constraint) =>
        Optional(constraint).Bind(static c => Optional(c.AcceptedValues)).Map(static a => toSeq(a)).IfNone(Seq<IValueConstraintComponent>());

    static Seq<string> ExactValues(ValueConstraint? constraint) =>
        Components(constraint).Choose(static c => c is ExactConstraint e ? Some(e.Value) : Option<string>.None);

    static Option<string> SingleValue(ValueConstraint? constraint) => ExactValues(constraint).Head;

    // --- [FACET_RAISE] -----------------------------------------------------------------
    static IFacet Raise(IdsFacet facet, IfcSchemaVersions schema) => facet.Switch(
        state:          schema,
        entity:         static (s, f) => (IFacet)new IfcTypeFacet {
                            IfcType = new ValueConstraint(IdsSchema.Simplify(f.Classes.Map(static c => c.Key), s)),
                            PredefinedType = f.Predefined.IsEmpty ? null : new ValueConstraint(f.Predefined.Map(static p => p.Token)),
                            IncludeSubtypes = true,
                        },
        attribute:      static (_, f) => (IFacet)new AttributeFacet { AttributeName = RaiseName(f.Name), AttributeValue = RaiseMatches(f.Value) },
        property:       static (_, f) => (IFacet)new IfcPropertyFacet { PropertySetName = RaiseName(f.Set), PropertyName = RaiseName(f.Name), PropertyValue = RaiseMatches(f.Value) },
        classification: static (_, f) => (IFacet)new IfcClassificationFacet {
                            ClassificationSystem = HostEdge.Slot(f.Branches.Head.Map(static b => new ValueConstraint(b.System))),
                            Identification = f.Branches.IsEmpty ? null : new ValueConstraint(f.Branches.Map(static b => b.Code)),
                            IncludeSubClasses = f.Reach.Subsumes,
                        },
        material:       static (_, f) => (IFacet)new MaterialFacet { Value = RaiseMatches(f.Value) },
        partOf:         static (s, f) => {
                            PartOfFacet raised = new() {
                                EntityType = HostEdge.Slot(f.Container.Bind(c => c is IdsFacet.Entity e ? Some((IfcTypeFacet)Raise(e, s)) : Option<IfcTypeFacet>.None)),
                            };
                            raised.SetRelation(f.Relation.Foreign);
                            return (IFacet)raised;
                        });

    static ValueConstraint? RaiseName(ValueMatch name) => name switch {
        ValueMatch.OneOf o   => new ValueConstraint(o.Allowed),
        ValueMatch.Pattern p => ValueConstraint.CreatePattern(p.Expression),
        _                    => null,
    };

    static ValueConstraint? RaiseMatches(Seq<ValueMatch> value) {
        Seq<IValueConstraintComponent> components = value.Bind(static m => m switch {
            ValueMatch.OneOf o   => o.Allowed.Map(static a => (IValueConstraintComponent)new ExactConstraint(a)),
            ValueMatch.Pattern p => Seq<IValueConstraintComponent>(new PatternConstraint(p.Expression)),
            ValueMatch.Range r   => Seq<IValueConstraintComponent>(new RangeConstraint(
                                        RaiseBound(r.Lower).Magnitude, RaiseBound(r.Lower).Inclusive,
                                        RaiseBound(r.Upper).Magnitude, RaiseBound(r.Upper).Inclusive)),
            ValueMatch.Length l  => Seq<IValueConstraintComponent>(new StructureConstraint {
                                        MinLength = HostEdge.Nullable(l.Min),
                                        MaxLength = HostEdge.Nullable(l.Max),
                                    }),
            ValueMatch.Digits d  => Seq<IValueConstraintComponent>(new StructureConstraint {
                                        TotalDigits = HostEdge.Nullable(d.Total),
                                        FractionDigits = HostEdge.Nullable(d.Fraction),
                                    }),
            _                    => Seq<IValueConstraintComponent>(),
        });
        if (components.IsEmpty) {
            return null;
        }
        ValueConstraint constraint = new();
        components.Iter(component => constraint.AddAccepted(component));
        return constraint;
    }

    static (string? Magnitude, bool Inclusive) RaiseBound(Option<RangeBound> bound) =>
        bound.Match(
            Some: static b => b.Switch(
                inclusive: static i => ((string?)i.Value.Si.ToString("R", CultureInfo.InvariantCulture), true),
                exclusive: static e => ((string?)e.Value.Si.ToString("R", CultureInfo.InvariantCulture), false)),
            None: static () => ((string?)null, false));

    public static Fin<IdsFileAudit> AuditFile(ReadOnlyMemory<byte> idsBytes) =>
        Try.lift(() => {
            using MemoryStream stream = new(idsBytes.ToArray());
            BufferingLogger sink = new();
            global::IdsLib.Audit.Status status = global::IdsLib.Audit.Run(stream, new SingleAuditOptions { IdsVersion = IdsVersion.Ids1_0 }, sink);
            return new IdsFileAudit(status, LibraryInformation.AssemblyVersion, sink.Drain());
        }).Run().Bind(static inner => inner);
}

public sealed record IdsResolved {
    private IdsResolved(IdsSpecification specification) => Specification = specification;

    internal static IdsResolved Of(IdsSpecification specification) => new(specification);

    public IdsSpecification Specification { get; }

    public IdsAudit Audit(ElementGraph graph) {
        IdsSpecification spec = Specification;
        ElementQuery applicable = IfcVisible(spec.Applicability.Head.Match(
            None: () => ElementQuery.Query(graph, BimTerm.Open),
            Some: head => ElementQuery.Query(graph, spec.Applicability.Tail.Fold(
                head.ToPredicate(),
                static (term, facet) => term.And(facet.ToPredicate())))));
        Seq<IdsAudit.FacetVerdict> verdicts = spec.Requirements.Map(req => {
            ElementQuery matched = applicable.Where(req.Facet.ToPredicate());
            (ElementQuery pass, ElementQuery fail) = req.Cardinality.Partition(matched, applicable, () => applicable.Where(req.Facet.Presence().ToPredicate()));
            return new IdsAudit.FacetVerdict(req.Facet, req.Facet.FacetKey(spec.Schema), req.Cardinality, pass.GlobalIds, fail.GlobalIds);
        });
        return new IdsAudit(spec.Name, spec.Ordinal, ContentAddress.OfGraph(graph), spec.Cardinality, spec.Severity, applicable.Count, verdicts, spec.Dropped);
    }

    static ElementQuery IfcVisible(ElementQuery selected) =>
        selected.Where(BimLeaf.Of(new ElementLeaf.ByAttribute(
            new ValueMatch.Exact(new PropertyValue.Text(ObjectAttribute.GlobalId.Key)),
            ValueMatch.Any)));
}

public readonly record struct IdsDiagnostic(LogLevel Level, Option<int> Code, string Message);

public sealed record IdsFileAudit(global::IdsLib.Audit.Status Status, string EngineVersion, Seq<IdsDiagnostic> Diagnostics) {
    public bool Conforms => Status == global::IdsLib.Audit.Status.Ok;
    public Seq<IdsDiagnostic> Errors => Diagnostics.Filter(static d => d.Level >= LogLevel.Error);
}

public sealed record IdsAudit(
    string Specification, int Spec, ContentAddress Model, IdsCardinality SpecCardinality, RuleSeverity Severity,
    int ApplicableCount, Seq<IdsAudit.FacetVerdict> Verdicts, Seq<DroppedFacet> Dropped) {
    public sealed record FacetVerdict(IdsFacet Facet, ContentAddress Key, IdsCardinality Cardinality, Seq<string> Passed, Seq<string> Failed);

    public IdsOutcome Outcome =>
        !Dropped.IsEmpty ? IdsOutcome.Indeterminate
        : SpecCardinality.SpecSatisfied(ApplicableCount) && Verdicts.ForAll(static v => v.Failed.IsEmpty) ? IdsOutcome.Conformant
        : IdsOutcome.NonConformant;

    public bool Conforms => Outcome.Conforms;
}

// --- [SERVICES] ------------------------------------------------------------------------
file sealed class BufferingLogger : ILogger {
    readonly List<IdsDiagnostic> sink = [];

    public Seq<IdsDiagnostic> Drain() => toSeq(sink);
    public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Warning;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) {
        if (logLevel >= LogLevel.Warning) {
            Option<int> code = state is IReadOnlyList<KeyValuePair<string, object?>> values
                ? toSeq(values).Find(static kv => kv.Key == "errorCode").Bind(static kv => kv.Value is int c ? Some(c) : Option<int>.None)
                : Option<int>.None;
            sink.Add(new IdsDiagnostic(logLevel, code, formatter(state, exception)));
        }
    }

    sealed class NullScope : IDisposable {
        public static readonly NullScope Instance = new();
        public void Dispose() { }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class IdsSchema {
    static readonly ConcurrentDictionary<IfcSchemaVersions, Seq<string>> Classes = new();
    static readonly ConcurrentDictionary<(string Class, IfcSchemaVersions Schema), Seq<string>> Predefined = new();

    public static Seq<string> ConcreteClasses(string topClass, IfcSchemaVersions schema) =>
        toSeq(SchemaInfo.GetConcreteClassesFrom(topClass, schema));

    public static Seq<string> ClassRoster(IfcSchemaVersions schema) =>
        Classes.GetOrAdd(schema, static key =>
            toSeq(SchemaInfo.GetSchemas()).Bind(static graph => toSeq(graph).Map(static c => c.Name)).Distinct());

    public static Seq<string> PredefinedTokens(string className, IfcSchemaVersions schema) =>
        Predefined.GetOrAdd((className, schema), static key =>
            toSeq(SchemaInfo.GetSchemas(key.Schema))
                .Choose(graph => Optional(graph[key.Class]))
                .Bind(static c => Optional(c.PredefinedTypeValues).Map(static tokens => toSeq(tokens)).IfNone(Seq<string>()))
                .Distinct());

    public static Option<string> StandardDatatype(string setName, string propertyName, IfcSchemaVersions schema) =>
        toSeq(SchemaInfo.GetSchemas(schema))
            .Map(static graph => graph.Version)
            .Choose(version => Optional(PropertySetInfo.Get(version, setName, propertyName)))
            .Choose(static p => p is SingleValuePropertyType single ? Some(single.DataType) : Option<string>.None)
            .Head;

    public static Seq<string> Simplify(Seq<string> concreteClasses, IfcSchemaVersions schema) =>
        SchemaInfo.TrySimplifyTopClasses(concreteClasses, schema, out IEnumerable<string> tops)
            ? toSeq(tops)
            : concreteClasses;

    public static Option<Dimension> DimensionOf(string ifcDataType) =>
        SchemaInfo.TryGetMeasureInformation(ifcDataType, out IfcMeasureInformation? info) && info is { Exponents: { } e }
            ? Some(Dimension.Create(e.Length, e.Mass, e.Time, e.ElectricCurrent, e.Temperature, e.AmountOfSubstance, e.LuminousIntensity))
            : Option<Dimension>.None;
}
```

## [03]-[MODEL_HEALTH]

- Owner: `ModelHealth` the three-tier model-QA verdict — the ONE model-health entry `Rasm.AppUi` and the review pipeline read; `ModelFinding` the closed `[Union]` verdict family EVERY tier converges on — `Structural` carrying the shared `Rasm.Element/Projection/audit#AUDIT_FOLD` `AuditFinding` row WHOLE (neutral graph integrity and coverage, graded at the graph's own owner), `Baseline` carrying the `Semantics/properties#TEMPLATE_AUDIT` `TemplateFinding` row WHOLE (produced there against the buildingSMART templates, composed here — the spec-free ground truth), `Authored` the per-element failed IDS requirement with its full typed evidence (`IdsFacet`, `IdsCardinality`, the ordinal join identity, the computed facet key) — the case IS the tier discriminant, so tier dispatch is the generated `Switch` and a parallel tier vocabulary beside the family is the deleted form; `Severity` the derived `RuleSeverity` band; `Coordinate` the per-finding report group key.
- Entry: `ModelHealth.Audit(ElementGraph graph, TemplateScope scope, Seq<IdsResolved> specifications, Func<IfcClass, Option<BsddClass>> dictionary)` runs all three tiers over the one frozen element graph — the STRUCTURAL tier composes the shared `ModelAudit.Of(graph)` under its default structural thresholds, so every model carries the neutral integrity-and-coverage grade beneath any IFC claim; the baseline tier ALWAYS runs (an empty specification set still yields the zero-configuration verdict, so every model carries a health floor before any IDS is authored), each authored specification folding through its own total `IdsResolved.Audit` — the parameter type is the gate, so a caller cannot hand this entry a specification whose dictionary reach never settled; `scope` is the `Semantics/properties#PROPERTY_TEMPLATES` `TemplateScope` policy value threaded straight into `TemplateAudit.Run`, so a `Handover` audit grades COBie completeness on the SAME baseline fold a `Standard` audit runs; the `Fin` results are the shared audit's uniform entry point and the baseline's shared `Bake`.
- Auto: `Findings` flattens every tier onto the one `ModelFinding` stream — every shared `AuditFinding` a `Structural` case, every baseline row a `Baseline` case, every `FacetVerdict.Failed` element an `Authored` case carrying its requirement ordinal, its computed facet key, and its specification's enforcement band — a pure fold over the stored tiers; `Conforms` is the one verdict: NO blocking finding AND every `IdsAudit.Conforms`, so a spec-free template-floor gap advises where the prior empty-baseline rule failed a model that satisfied every authored specification.
- Output: `ModelHealth` stores the tiers typed — the shared `ModelAudit` whole, the `TemplateFinding` rows with their failing-axis evidence, and the `IdsAudit` rows with their cardinality rule and model snapshot digest — beside the `Scope` policy naming the baseline definition set. `Findings` is the derived projection, never a third stored copy.
- Packages: Rasm.Element, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm
- Growth: a new baseline verdict axis is the properties owner's `TemplateVerdict` row and rides the `Baseline` case untouched; a new enforcement band is one `RuleSeverity` row `[02]` owns; a new definition-set scope is one `TemplateScope` row the threaded policy value carries with zero edits here; a new facet, cardinality, relation, or dictionary reach rides the authored tier's own growth rows; a new neutral integrity class or coverage axis is the shared audit's own growth row and rides the `Structural` case untouched; a fourth verdict source is one `ModelFinding` case and one `Findings` arm with every consumer `Switch` broken loudly at compile time; never a second model-health entry, and never a per-tier report type.
- Boundary: the scope split with the shared audit is absolute — `Rasm.Element/Projection/audit#AUDIT_FOLD` `ModelAudit` owns neutral structural integrity and coverage ratios, while this page owns IFC semantics. The baseline stream is produced by `Semantics/properties#TEMPLATE_AUDIT`; this owner composes those rows. The convergence is the finding family, never the matcher vocabularies. Document-validity `AuditFile` stays independent of model audit.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using LanguageExt;
using Rasm.Bim.Model;
using Rasm.Bim.Semantics;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [TYPES] ---------------------------------------------------------------------------
[Union]
public partial record ModelFinding {
    partial record Structural(AuditFinding Finding);
    partial record Baseline(TemplateFinding Finding);
    partial record Authored(string GlobalId, string Specification, int Spec, int Requirement, ContentAddress Key, IdsFacet Facet, IdsCardinality Cardinality, RuleSeverity Severity);

    public RuleSeverity Severity => Switch(
        structural: static s => s.Finding.Severity.Blocks ? RuleSeverity.Error : RuleSeverity.Warning,
        baseline: static _ => RuleSeverity.Warning,
        authored: static a => a.Severity);

    public string Coordinate => Switch(
        structural: static s => s.Finding.Category.Key,
        baseline: static b => $"{b.Finding.Set}.{b.Finding.Code}",
        authored: static a => $"{a.Spec}:{a.Requirement}:{a.Key.ToValue():X32}");
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ModelHealth(TemplateScope Scope, ModelAudit Structural, Seq<TemplateFinding> Baseline, Seq<IdsAudit> Authored) {

    public static Fin<ModelHealth> Audit(ElementGraph graph, TemplateScope scope, Seq<IdsResolved> specifications, Func<IfcClass, Option<BsddClass>> dictionary) =>
        from structural in ModelAudit.Of(graph, key)
        from baseline in TemplateAudit.Run(graph, scope, dictionary, key)
        select new ModelHealth(scope, structural, baseline, specifications.Map(spec => spec.Audit(graph)));

    public Seq<ModelFinding> Findings =>
        Structural.Findings.Map(static f => (ModelFinding)new ModelFinding.Structural(f))
        + Baseline.Map(static f => (ModelFinding)new ModelFinding.Baseline(f))
        + Authored.Bind(static audit => audit.Verdicts
            .Map(static (v, i) => (Verdict: v, Requirement: i))
            .Bind(r => r.Verdict.Failed.Map(g => (ModelFinding)new ModelFinding.Authored(
                g, audit.Specification, audit.Spec, r.Requirement, r.Verdict.Key, r.Verdict.Facet, r.Verdict.Cardinality, audit.Severity))));

    public bool Conforms =>
        !Findings.Exists(static f => f.Severity.Blocking) && Authored.ForAll(static a => a.Conforms);
}
```

## [04]-[RESEARCH]

(none)
