# [BIM_VALIDATION]

The three-tier model-QA owner exposes one model-health verdict over the frozen seam graph. The STRUCTURAL tier composes `Rasm.Element/Projection/audit#AUDIT_FOLD` `ModelAudit` whole, so neutral graph integrity and discipline coverage stay where the graph owns them. The BASELINE tier composes the spec-free `Semantics/properties#TEMPLATE_AUDIT` `TemplateFinding` stream beneath every authored specification. The AUTHORED tier carries buildingSMART IDS whole: `Parse` admits one `Xbim.InformationSpecifications` document and records every unliftable facet as a `DroppedFacet`; `Resolve` settles authored classification reach through `Semantics/classification#BSDD_RESOLUTION`; `Audit` grades the frozen IFC-visible universe through the one `Model/query#ELEMENT_SET` algebra; `Publish` raises the same family through `Xids`; and `AuditFile` runs the buildingSMART `ids-lib` document audit. Range bounds remain unit-safe through the IDS datatype or standard-Pset declaration into the `ids-lib` SI dimension. `ModelHealth.Audit` joins the structural, baseline, and authored facts into the closed `ModelFinding` family, and `Conforms` means no blocking finding. The page is host-local.

## [01]-[INDEX]

- [02]-[IDS_FACETS]: `RuleSeverity` the package-wide enforcement band, `IdsOutcome` the three-valued specification verdict, `DropReason`/`DroppedFacet` the typed unliftable-facet accounting, `ClassificationReach` the authored-subclass axis and its settled branch payload, `IdsFacet` the closed `[Union]` of seam-lowered facets (each `ToPredicate()` a graph-free `BimTerm`, the value a seam `ValueMatch`), `PartOfRelation` the relation policy rows, `IdsRequirement`/`IdsSpecification` the spec records, `IdsSpecification.Parse`/`Resolve`/`Publish` the three declared boundary steps, `IdsResolved` the graded-specification evidence and its total `Audit`, `IdsSpecification.AuditFile` over `ids-lib` `Audit`, the `IdsSchema` schema-parameterized offline authority, and the `IdsAudit` receipt.
- [03]-[MODEL_HEALTH]: `ModelHealth` the three-tier model-QA receipt and its `Audit` entry — the neutral seam `ModelAudit` structural grade and the baseline `TemplateFinding` stream composed beneath the authored per-spec `IdsAudit` fold under one threaded `TemplateScope` policy the receipt records — `ModelFinding` the closed three-case verdict family carrying its `Severity` band, `Findings` the one flattened verdict stream, `Coordinate` the per-finding report group key, `Conforms` the one model-health verdict.

## [02]-[IDS_FACETS]

- Owner: `RuleSeverity` the package-wide `[SmartEnum<string>]` enforcement band whose row carries its own `Blocking` policy — declared HERE at the `Rasm.Bim` root and composed by `Review/coordination#COORDINATION`'s rule library from its child namespace, so one severity vocabulary spans model-check verdicts and IDS findings; `IdsOutcome` the three-valued specification verdict (`Conformant`/`NonConformant`/`Indeterminate`, the last the partially-graded state a dropped requirement facet forces); `DropReason` the closed vocabulary of lowering gaps and `DroppedFacet` its per-facet row carrying the `FacetGroup.FacetUse` role and the foreign `Short()` label; `ClassificationReach` the closed authored-subclass axis (`Exact` the declared codes alone, `Pending` an unsettled `IncludeSubClasses`, `Subsumed` the resolver's closed branch payload); `IdsSpecification` the specification record carrying the applicability and requirement facet sets, the cardinality, its OWN declared `IfcSchemaVersions` axis, its enforcement band, and the facets no lowering lifts; `IdsResolved` the graded-specification evidence `Resolve` alone mints and `Audit` alone accepts; `IdsFacet` the closed `[Union]` (Entity, Attribute, Property, Classification, Material, PartOf) each carrying SEAM-LOWERED data — resolved `IfcClass`/`PredefinedType` sets, `ValueMatch` name and value restrictions, resolved `Classification` branches — admitted ONCE at parse so the interior never sees an `Xbim` `ValueConstraint`; `PartOfRelation` the `[SmartEnum<string>]` relation POLICY ROWS, each carrying its query-arm lowering delegate AND its foreign `PartOfFacet.PartOfRelation` member as row data; each `IdsFacet` arm `ToPredicate()` lowering GRAPH-FREE to a `Model/query#ELEMENT_SET` `BimTerm`, `Presence()` deriving the value-widened conditional form the Optional cardinality partitions against, and `FacetKey(schema)` projecting the stable requirement token as a seam `ContentAddress` over the ONE kernel hasher; `IdsAudit` the deterministic per-specification receipt; `IdsFileAudit` the IDS-document validity receipt.
- Entry: `IdsSpecification.Parse(ReadOnlyMemory<byte> idsBytes, Op key)` admits an IDS XML document through `Xids.LoadBuildingSmartIDS`, reading each spec's own `Specification.IfcVersion` onto the `IfcSchemaVersions` axis through `IfcSchemaVersionHelper` and lowering every facet and every `ValueConstraint` onto the closed union, each unliftable facet leaving as a typed `DroppedFacet` and the SAME `GetAllowedCardinality` legality table gating the pairing HERE. `IdsSpecification.Resolve(Seq<IdsSpecification>, BsddPort, BsddPins, CancellationToken, Op)` is the railed dictionary step. `IdsResolved.Audit(ElementGraph graph)` folds applicability into one `BimTerm` scoped to the IFC-visible `ExternalId`-bearing universe, queries through `ElementQuery.Query`, refines requirements through `ElementQuery.Where`, and partitions through `IdsCardinality.Partition`. `IdsSpecification.Publish` is the ingress inverse and `AuditFile` validates the document's own conformance through `ids-lib` `Audit.Run`.
- Auto: `Parse` is the value-lowering boundary and every gap it cannot lower leaves as a NAMED `DropReason` on an `Either` Left rail, never a `Choose`-discarded `None` — `Matches` folds a facet's `ValueConstraint.AcceptedValues` onto `Seq<ValueMatch>` (an all-exact set collapses to one `OneOf`, a `PatternConstraint` to `Pattern`, a `RangeConstraint` to a dimension-checked `Range` whose inclusivity rides the `RangeBound` arm, a pure length-bearing `StructureConstraint` to `Length`, a pure digits-bearing one to `Digits`, an absent constraint to `Present`; only a StructureConstraint MIXING the two axes drops, because one component lowers to one `ValueMatch` and splitting ORs two partial matches into a false PASS — beside it a bounds-crossed or exclusive-coincident range drops as `UnsatisfiableRange`); `NameMatch` lowers a NAME-position constraint to ONE `ValueMatch` so patterned names survive; `Predefineds` expands a patterned predefined token against the resolved classes' `IdsSchema.PredefinedTokens` roster through the ONE seam matcher; `DataTypeOf` resolves the range-bound datatype from the facet or the `PropertySetInfo.Get` standard-Pset declaration; `Numeric` coerces bound literals through `ValueConstraint.TryGetNetType`/`ParseValue` in the IFC datatype's value space; `ResolveClasses` expands an Entity facet's `IfcType` to its `IdsSchema.ConcreteClasses` subtypes when `IncludeSubtypes` and expands a PATTERNED entity name against `IdsSchema.ClassRoster`, an entity facet resolving to no rostered class dropping as `UnknownClass` rather than lowering to a match-nothing predicate a Prohibited requirement reads as a model-wide pass; `ClassificationBranches` resolves the system through the `Semantics/classification#CLASSIFICATION_AXIS` roster and admits each code through the seam `Classification.Of` door, the facet's own code set being the branch the seam arm decides SET MEMBERSHIP over — the SUB-branch expansion is the `Resolve` step's, never a code-prefix derivation; `Audit` then folds each facet's graph-free `ToPredicate()` — the validation fold reuses the query algebra for BOTH selection and value with one total `Switch` — and stamps each verdict's `FacetKey` ONCE under the spec's schema.
- Receipt: `IdsAudit` carries the specification name, the `Spec` document ordinal, the `Model` provenance digest (the seam `Projection/address#CONTENT_ADDRESS` `ContentAddress.OfGraph` snapshot address of the graph the fold ran over, so a stored verdict set names the model it graded and a re-audit after an edit re-keys), the spec-level `IdsCardinality`, the enforcement band, the applicable element count, the passed/failed `GlobalId` sets per facet with each facet's computed key, and the `DroppedFacet` rows; `IdsAudit.Outcome` is the three-valued verdict — `Indeterminate` whenever a facet dropped, else the spec-level applicable-count rule (`SpecSatisfied`) AND every requirement verdict passing — and `Conforms` reads its row column; `IdsFileAudit.Conforms`/`Errors` reads the `Status` and the captured diagnostics.
- Packages: Xbim.InformationSpecifications, ids-lib, Microsoft.Extensions.Logging.Abstractions, Rasm.Element (the seam `ElementGraph`, the `Query/predicate#ELEMENT_PREDICATE` algebra, and the kernel `CanonicalWriter` (`Rasm/Domain/identity#CONTENT_KEY`) and `Projection/address#CONTENT_ADDRESS` codec the facet key and the snapshot digest ride), Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm
- Growth: a new IDS facet is one `IdsFacet` union arm lowering to its `BimLeaf` or seam `ElementLeaf` arm plus its `Matches` lowering, its `Presence()` widening, its `Write` key contribution, and its `Raise` inverse; a new PartOf relation is one `PartOfRelation` row (delegate + foreign member — zero switch edits); a new value-match modality is one `ValueMatch` arm the seam already folds plus one `WriteMatch` arm ordinal; a new lowering gap is one `DropReason` row; a new enforcement band is one `RuleSeverity` row carrying its `Blocking` policy; a new cardinality is one `IdsCardinality` row carrying its `(matched, applicable, presence)` partition, spec rule, AND both authored inverses; a new dictionary reach modality is one `ClassificationReach` case with its `Descendants` and `Subsumes` arms; a new IFC schema version is already the `IfcSchemaVersions` flags axis each spec declares; never a second validation predicate surface, never a second value engine, never a hand-rolled IDS parser or writer, and never a transport minted here.
- Boundary: THE THREE STEPS ARE DECLARED AND THE ORDER IS TYPED — `Parse` decodes XML synchronously and reaches nothing, `Resolve` is the ONE dictionary hop, and `Audit` accepts `IdsResolved` alone, which `Resolve` alone mints; a caller-run expansion before the fold was REFUSED because an audit that grades correctly only when every caller remembers a prior step is ceremony pushed onto callers, and an `IncludeSubClasses` facet graded on its declared codes alone reports a clean pass over a branch it never covered. The `Model/query#ELEMENT_SET` `BimTerm` algebra is the ONLY validation selection surface and `ValueMatch` the ONLY value engine — `ByProperty`/`ByAttribute` carry the name restrictions natively, `PartOf` lowers recursively to `NodeMatch<ElementLeaf>.Where` with its container PROVED nested-lowerable at parse, and `Aggregated`/`Nested`/`Voided` lower through `ByComposed`/`ByVoided`. `Xids` owns IDS parsing and publishing, buildingSMART `ids-lib Audit.Run` owns IDS-FILE audit independently of model audit, and `IdsSchema` resolves entity, predefined, property, and measure facts from the real `IdsLib.IfcSchema` graph under the specification's OWN declared version — a pinned literal grades an IFC2X3 exchange against classes its schema never had. Non-standard Xbim `DocumentFacet` and `IfcRelationFacet` cases drop explicitly at parse; every unliftable facet becomes a `DroppedFacet` and makes its specification `Indeterminate`, as do unknown classes, unsatisfiable ranges, unresolvable branches, and empty prohibited grading. `??` survives at ONE site — the `Project` foreign-document transcription, where the Xbim record's nullable authoring columns admit; anywhere else on this page it is the deleted form. `IdsAudit` and `IdsFileAudit` are typed validation evidence, C# host-local, and neither mints a TypeScript family. Model audit reads the seam `ElementGraph` assembled by `SemanticProjector`.
- Events: an issued `IdsAudit` fires the `Model/observability#HOOK_RAIL` `rasm.bim.review.verdict` point with `BimFact.Verdict` — the specification name beside its document ORDINAL (IDS spec names are not unique, so the ordinal keeps two same-named specifications' verdicts apart), the `Model` provenance address the fold ran over, the tier, the `IdsOutcome` key, the `RuleSeverity` key, the finding tally, and the failing `GlobalId` set — at the `Audit` fold's own edge; the point is REPLAY modality so a late panel drains the recent window, and the CloudEvents announcement is `Exchange/events#EVENT_PROJECTION`'s observe subscription over it, subject `name#ordinal` matching the coordination key. Minting a verdict message envelope at this rail is the deleted form.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
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
using LanguageExt.UnsafeValueAccess;
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
using Op = Rasm.Domain.Op;
using SeamClassification = Rasm.Element.Classification.Classification;   // aliased so the nested IdsFacet.Classification arm never shadows it
using BimTerm = Rasm.Element.Query.Predicate<Rasm.Bim.Model.BimLeaf>;         // closed-generic aliases: the bare name
using ElementTerm = Rasm.Element.Query.Predicate<Rasm.Element.Query.ElementLeaf>;   // collides with global-using System.Predicate<T>

namespace Rasm.Bim;

// --- [TYPES] ------------------------------------------------------------------------------
// The package-wide QA enforcement axis, declared ONCE here at the Rasm.Bim root and composed by the
// Review/coordination#COORDINATION rule library from its child namespace. Blocking is roster POLICY, not a knob: a
// consumer reads `severity.Blocking` instead of comparing against an Error literal, and a band that advises above
// Warning is representable — which is what lets ModelHealth.Conforms mean "no blocking finding".
[SmartEnum<string>]
public sealed partial class RuleSeverity {
    public static readonly RuleSeverity Info    = new("info",    blocking: false);
    public static readonly RuleSeverity Warning = new("warning", blocking: false);
    public static readonly RuleSeverity Error   = new("error",   blocking: true);

    public bool Blocking { get; }
}

// A graded specification either conforms or does not; one carrying a requirement facet the lowering could not lift
// was graded PARTIALLY — Indeterminate is that third state, so an unliftable requirement can never read as a pass.
[SmartEnum<string>]
public sealed partial class IdsOutcome {
    public static readonly IdsOutcome Conformant    = new("conformant",     conforms: true);
    public static readonly IdsOutcome NonConformant = new("non-conformant", conforms: false);
    public static readonly IdsOutcome Indeterminate = new("indeterminate",  conforms: false);

    public bool Conforms { get; }
}

// The named reasons a foreign facet cannot lower onto the seam algebra — the typed accounting that replaces the
// silent Choose-discard. Each row names an EXACT gap, so a receipt states which facet the audit could not grade.
[SmartEnum<string>]
public sealed partial class DropReason {
    public static readonly DropReason UnknownClass        = new("unknown-class");         // no accepted entity name resolves to a rostered IfcClass
    public static readonly DropReason UnknownAttribute    = new("unknown-attribute");     // the name restriction selects no seam ObjectAttribute column
    public static readonly DropReason MixedNameMatch      = new("mixed-name-match");      // a mixed exact+pattern NAME constraint has no single-ValueMatch spelling
    public static readonly DropReason MixedStructure      = new("mixed-structure");       // a StructureConstraint mixing digits and length facets has no single-arm spelling
    public static readonly DropReason MalformedPattern    = new("malformed-pattern");     // an uncompilable or NonBacktracking-unsupported XSD regex
    public static readonly DropReason IllegalCardinality  = new("illegal-cardinality");   // the document pairs a facet with a cardinality its own schema law forbids
    public static readonly DropReason ExtendedFacet       = new("extended-facet");        // an Xbim DocumentFacet/IfcRelationFacet outside buildingSMART IDS v1.0
    public static readonly DropReason EmptyClassification = new("empty-classification");  // a classification facet carrying neither system nor identification
    public static readonly DropReason UnsatisfiableRange  = new("unsatisfiable-range");   // a range whose bounds cross, or whose equal bounds carry an exclusive end
    public static readonly DropReason NestedContainer     = new("nested-container");      // a PartOf container facet with no spelling in the seam nested-leaf vocabulary
    public static readonly DropReason UnresolvedBranch    = new("unresolved-branch");     // an IncludeSubClasses code the dictionary could not answer, so the branch never closed
}

// One unliftable facet, carried from parse to receipt: the group role it sat in (an applicability drop WIDENS the
// graded set, a requirement drop leaves a requirement ungraded — both make the verdict partial), the facet's own
// Xbim Short() label as the operator-readable evidence, and the reason row.
public readonly record struct DroppedFacet(FacetGroup.FacetUse Role, string Facet, DropReason Reason);

// The authored IncludeSubClasses axis as a closed reach. Parse cannot reach a dictionary, so a subclass-inclusive
// facet lands Pending and the Resolve step settles it into Subsumed, carrying the descendants the dictionary
// states — the ByClassification posture, where the seam leaf decides SET MEMBERSHIP over a resolved branch and
// derives no hierarchy, so the resolver hands the branch IN. Subsumes is the AUTHORED bit both the join key and
// the Publish inverse write, so a republished facet reads as authored rather than as a flattened leaf enumeration.
[Union]
public abstract partial record ClassificationReach {
    private ClassificationReach() { }

    public sealed record Exact : ClassificationReach;
    public sealed record Pending : ClassificationReach;
    public sealed record Subsumed(Seq<SeamClassification> Descendants) : ClassificationReach;

    public static readonly ClassificationReach Declared = new Exact();
    public static readonly ClassificationReach Awaiting = new Pending();

    public Seq<SeamClassification> Codes => Switch(
        exact:    static _ => Seq<SeamClassification>(),
        pending:  static _ => Seq<SeamClassification>(),
        subsumed: static s => s.Descendants);

    public bool Subsumes => this is not Exact;
}

// The IDS cardinality vocabulary owns ALL FOUR policies as row data: `Partition` the requirement-level pass/fail
// split over (matched, applicable, presence-thunk), `SpecSatisfied` the spec-level applicable-count rule (the Xbim
// ICardinality.IsSatisfiedBy truth), and the two authored inverses the Publish egress writes back. Optional is the
// CONDITIONAL requirement (present-must-satisfy): only elements carrying the facet's feature with a violating value
// FAIL — a pass-everything Optional row would false-PASS the buildingSMART semantics — and only this row forces the
// presence thunk, so Required/Prohibited never pay the second fold.
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

// The neutral PartOf-relation POLICY ROWS: each case carries its query-arm lowering as delegate data and its
// foreign Xbim member as a data column, so the ingress map (MapRelation) and the egress raise (SetRelation) both
// DERIVE from this one row set. ALL FIVE relations lower — Contained TRANSITIVELY (SpatialReach.Ancestry: an IDS
// partOf storey holds for a space-contained element), the other four through their seam twins; Grouped takes the
// BimLeaf.InZone modality pair, and Voided accepts either Void-axis side.
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

// The closed IDS facet family — the mirror of the six buildingSMART facets carrying SEAM-LOWERED data only: the
// Xbim ValueConstraint is admitted ONCE at Parse and never crosses into this interior. Name positions are
// ValueMatch (a patterned Pset_.* set/property/attribute name lowers WHOLE), value positions Seq<ValueMatch>.
// Each arm ToPredicate() lowers GRAPH-FREE to ONE BimTerm, so the validation predicate IS the query predicate.
[Union]
public partial record IdsFacet {
    partial record Entity(Seq<IfcClass> Classes, Seq<PredefinedType> Predefined);
    partial record Attribute(ValueMatch Name, Seq<ValueMatch> Value);
    partial record Property(ValueMatch Set, ValueMatch Name, Seq<ValueMatch> Value);
    partial record Classification(Seq<SeamClassification> Branches, Option<string> System, ClassificationReach Reach) {
        // The graded branch set: the authored codes plus whatever the Resolve step settled beneath them. Pending is
        // unreachable behind IdsResolved — the resolver either closes the branch or drops the facet.
        public Seq<SeamClassification> Selected => Branches + Reach.Codes;
    }
    partial record Material(Seq<ValueMatch> Value);
    partial record PartOf(Option<IdsFacet> Container, PartOfRelation Relation);

    // Classification.Of drains structurally at both lowering sites: its only gates are a blank system and a blank
    // code, and every caller filters those first, so no admitted facet loses a branch to the Choose.
    internal static readonly Op Lowering = Op.Of(name: "ids-facet-lowering");

    // The ONE lowering to the query algebra: a value-bearing arm folds its Seq<ValueMatch> into an Any of the
    // matching term, the Entity arm crosses its class set with its predefined tokens, and PartOf lowers its
    // container facet to NodeMatch.Where DIRECTLY — case-owned recursion inside the one algebra, retiring the
    // materialize-then-join. The branch set folds to ONE ByClassification arm rather than an Any over singletons:
    // the seam leaf decides SET MEMBERSHIP over the resolved branch.
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
                            // Unreachable behind the parse gate: PartOfFacetOf drops a container no Nested lowering
                            // lifts, so the fail-closed match-nothing here proves totality and never grades.
                            Some: static c => Nested(c).IfNone(new ElementTerm.Any(Seq<ElementTerm>())),
                            None: static () => ElementTerm.Open))));

    // Nested lowers a container onto the SEAM leaf vocabulary the incidence arms type their target over. Entity
    // classes fold to ONE resolved "ifc"-system branch, and three shapes with no seam twin — a predefined token, a
    // system-only membership, a nested PartOf — answer None, so the parse drops them typed rather than widening a
    // container facet into a more permissive one.
    internal static Option<ElementTerm> Nested(IdsFacet facet) => facet.Switch(
        entity:         static f => f.Predefined.IsEmpty
                            ? Some((ElementTerm)new ElementTerm.Leaf(new ElementLeaf.ByClassification(
                                f.Classes.Map(c => SeamClassification.Of(ElementQuery.IfcSystem, c.Key, Lowering).ThrowIfFail()))))
                            : Option<ElementTerm>.None,
        attribute:      static f => Some(AnyElement(f.Value.Map(vm => (ElementLeaf)new ElementLeaf.ByAttribute(f.Name, vm)))),
        property:       static f => Some(AnyElement(f.Value.Map(vm => (ElementLeaf)new ElementLeaf.ByProperty(f.Set, f.Name, vm)))),
        classification: static f => f.Selected.IsEmpty
                            ? Option<ElementTerm>.None
                            : Some((ElementTerm)new ElementTerm.Leaf(new ElementLeaf.ByClassification(f.Selected))),
        material:       static f => Some(AnyElement(f.Value.Map(vm => (ElementLeaf)new ElementLeaf.ByMaterial(vm)))),
        partOf:         static _ => Option<ElementTerm>.None);

    // The value-widened PRESENCE form the Optional cardinality partitions against — the facet with its value
    // restriction relaxed to Any, so "carries the feature" separates from "carries it satisfying". Entity and
    // PartOf return the facet whole (their Optional is schema-illegal, so presence never partitions them);
    // Classification widens to system-only membership, falling back whole only when no system resolved.
    public IdsFacet Presence() => Switch(
        entity:         static f => (IdsFacet)f,
        attribute:      static f => f with { Value = Seq(ValueMatch.Any) },
        property:       static f => f with { Value = Seq(ValueMatch.Any) },
        classification: static f => f.System.IsSome
                            ? f with { Branches = Seq<SeamClassification>(), Reach = ClassificationReach.Declared }
                            : (IdsFacet)f,
        material:       static _ => new IdsFacet.Material(Seq(ValueMatch.Any)),
        partOf:         static f => (IdsFacet)f);

    // The stable requirement token writes EVERY identity-bearing slot through
    // the length-prefixed CanonicalWriter, so no delimiter forges a collision the prior interpolated scheme
    // admitted (a class literally named `a|b` collided with the two-class set). The token is DOCUMENT-derived — the
    // classification reach writes its AUTHORED bit, never its resolved descendants.
    // The fold writes a range bound's Measure, so it QUANTIZES and takes the ONE tolerance-bound digest entry
    // rather than the kernel's ZeroTolerance hasher. The grid is exact by declaration: an IDS bound is the
    // document's own literal and two bounds that differ below any model tolerance are different requirements.
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

    // A ValueMatch writes deterministically: an arm ordinal then its own slots. Every collection is count-prefixed
    // and every string length-prefixed, so a bound that is absent and a bound that is zero address differently.
    static CanonicalWriter WriteMatch(CanonicalWriter writer, ValueMatch match) => match switch {
        ValueMatch.OneOf o   => o.Allowed.Fold(writer.Ordinal(0).Ordinal(o.Allowed.Count), static (w, a) => w.String(a)),
        ValueMatch.Pattern p => writer.Ordinal(1).String(p.Expression),
        ValueMatch.Range r   => WriteBound(WriteBound(writer.Ordinal(2), r.Lower), r.Upper),
        ValueMatch.Length l  => WriteCount(WriteCount(writer.Ordinal(3), l.Min), l.Max),
        ValueMatch.Digits d  => WriteCount(WriteCount(writer.Ordinal(4), d.Total), d.Fraction),
        _                    => writer.Ordinal(5),
    };

    // Inclusivity is the RangeBound ARM, never a sibling boolean flag on Range — the arm ordinal carries it into
    // the key, and CanonicalWriter.Measure folds the bound's dimension and quantity type exactly as Raise emits it.
    static CanonicalWriter WriteBound(CanonicalWriter writer, Option<RangeBound> bound) =>
        bound.Match(
            Some: b => b.Switch(
                state:     writer.Bool(true),
                inclusive: static (w, i) => w.Ordinal(0).Measure(i.Value),
                exclusive: static (w, e) => w.Ordinal(1).Measure(e.Value)),
            None: () => writer.Bool(false));

    static CanonicalWriter WriteCount(CanonicalWriter writer, Option<int> bound) =>
        bound.Match(Some: v => writer.Bool(true).Ordinal(v), None: () => writer.Bool(false));

    // An empty arm set is a facet whose every value restriction lowered to nothing — it matches nothing rather than
    // smuggling a raw Func<Node.Object,bool> walk past the query surface. An entity facet resolving to NO rostered
    // class never reaches here: it is a typed DropReason.UnknownClass at parse, because a match-nothing predicate
    // flips a Prohibited requirement into a model-wide PASS.
    static BimTerm AnyOf(Seq<BimTerm> arms) =>
        arms.Count == 1 ? arms[0] : new BimTerm.Any(arms);

    static ElementTerm AnyElement(Seq<ElementLeaf> leaves) =>
        leaves.Count == 1 ? new ElementTerm.Leaf(leaves[0])
        : new ElementTerm.Any(leaves.Map(static l => (ElementTerm)new ElementTerm.Leaf(l)));
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record IdsRequirement(IdsFacet Facet, IdsCardinality Cardinality);

// Ordinal is the ZERO-BASED document position Parse stamps — the spec identity the cross-runtime join keys on,
// because IDS v1.0 does NOT require spec names unique within a document. Schema is the spec's OWN declared IFC
// version set on the ids-lib flags axis. Dropped carries the facets no lowering could lift, so a partially-graded
// specification says so instead of passing. Severity is the appointing party's enforcement band. Identifier reads
// `Specification.Guid`, the xbim model's own identity column minted lazily on first read, so a stable appointing
// handle round-trips rather than every republished document arriving at a CDE as a brand-new requirement.
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

    // The IDS document parse composes Xbim.InformationSpecifications `Xids` (the buildingSMART IDS v1.0 schema
    // binding) and is the ONE ingress boundary that lowers every facet's `ValueConstraint` onto seam-typed data —
    // retiring the hand-rolled XmlReaderSettings.Schemas/XDocument parser. An Xids the loader answers empty is a
    // typed refusal on the same rail the decode faults ride, never a thrown sentinel the funnel re-classifies.
    public static Fin<Seq<IdsSpecification>> Parse(ReadOnlyMemory<byte> idsBytes, Op key) =>
        key.Catch(() => {
            using MemoryStream stream = new(idsBytes.ToArray());
            // Document order IS the spec identity the ordinal-qualified join keys on — stamped once at parse.
            return Optional(Xids.LoadBuildingSmartIDS(stream, NullLogger.Instance))
                .Map(static xids => toSeq(xids.AllSpecifications().Select(static (spec, i) => Project(spec) with { Ordinal = i })));
        })
        .Bind(loaded => loaded.Match(
            Some: Fin.Succ,
            None: () => Fin.Fail<Seq<IdsSpecification>>(new BimFault.Refused(key, BimScope.Review, BimReason.Rejected, string.Join(':', new object?[] { "ids-lane", "parse", "load-empty" })))));

    // The DICTIONARY step, declared because Parse cannot take it: an IDS classification facet carrying
    // IncludeSubClasses names a BRANCH, and only bSDD states what a MasterFormat or Uniformat code contains. Every
    // Pending reach settles here into the closed descendant payload the ByClassification arm decides membership
    // over. A missing system drops its facet under UnresolvedBranch; transport, decode, and cancellation remain on
    // the exact rail rather than becoming an Indeterminate result.
    // Caller-expands was REFUSED: an audit correct only when every caller remembers a prior step is ceremony.
    public static Fin<Seq<IdsResolved>> Resolve(Seq<IdsSpecification> specifications, BsddPort port, BsddPins pins, CancellationToken token, Op key) =>
        specifications.TraverseM(spec => Settle(spec, port, pins, token, key)).As();

    static Fin<IdsResolved> Settle(IdsSpecification spec, BsddPort port, BsddPins pins, CancellationToken token, Op key) =>
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

    // Only a Pending classification reaches the dictionary; an Exact reach and every other arm pass through, and a
    // PartOf container recurses so a nested classification container settles with its host.
    static Fin<Either<DroppedFacet, IdsFacet>> Reached(IdsFacet facet, FacetGroup.FacetUse role, BsddPort port, BsddPins pins, CancellationToken token, Op key) =>
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

    // Every declared code must answer, because a partially expanded branch grades a set neither the document nor
    // the dictionary describes: the traverse is all-or-drop.
    static Fin<Either<DropReason, IdsFacet>> Descend(IdsFacet.Classification facet, BsddPort port, BsddPins pins, CancellationToken token, Op key) =>
        facet.Branches.TraverseM(branch => Children(branch, port, pins, token, key)).As()
            .Map(expanded => expanded.ForAll(static rows => rows.IsSome)
                ? Right<DropReason, IdsFacet>(facet with {
                    Reach = new ClassificationReach.Subsumed(expanded.Somes().Bind(static rows => rows).Distinct()),
                })
                : Left<DropReason, IdsFacet>(DropReason.UnresolvedBranch));

    // BsddClass.Children IS the authoritative containment — the parent a MasterFormat or Uniformat code string does
    // not encode and the seam carries no derivation for — so each child admits through the same seam door the
    // authored codes took and a code the seam refuses drops the whole branch rather than lowering blank.
    static Fin<Option<Seq<SeamClassification>>> Children(SeamClassification branch, BsddPort port, BsddPins pins, CancellationToken token, Op key) =>
        toSeq(ClassificationSystem.Items).Find(row => string.Equals(row.Key, branch.System, StringComparison.OrdinalIgnoreCase))
            .Match(
                Some: system => BsddResolution.Resolve(system, branch.Code, port, pins, token, key)
                    .Map(resolved => resolved.Children
                        .Traverse(child => SeamClassification.Of(branch.System, child.Code, key).ToOption()).As()),
                None: static () => Fin.Succ(Option<Seq<SeamClassification>>.None));

    // The authoring egress — the ingress INVERSE riding the SAME closed family, each spec publishing under its OWN
    // declared schema set. The cardinality LEGALITY table is a PURE ROP gate that runs BEFORE any document is built,
    // so the whole publish short-circuits on the first illegal pairing rather than throwing out of a half-assembled
    // Xids. Each requirement writes BOTH halves back — its facet plus its per-facet cardinality — because a
    // Prohibited requirement republishing at the Expected default would INVERT its meaning; FacetGroup.IsValid()
    // demands a RequirementOptions count MATCHING the facet count, which the one-projection-per-requirement pairing
    // satisfies structurally.
    public static Fin<byte[]> Publish(Seq<IdsSpecification> specifications, Op key) =>
        from raised in specifications.Traverse(spec => Raisable(spec, key)).As()
        from bytes in key.Catch(() => Serialize(raised))
        select bytes;

    // The pure legality gate: every requirement raises to its Xbim facet ONCE and pairs with its cardinality row,
    // and the package's own GetAllowedCardinality decides admissibility — the raised pair is the value the writer
    // consumes, so the check and the emission read one projection instead of two.
    static Fin<(IdsSpecification Spec, Seq<IFacet> Applicability, Seq<(IFacet Facet, RequirementCardinalityOptions Options)> Requirements)> Raisable(
        IdsSpecification spec, Op key) =>
        spec.Requirements
            .Traverse(req => Legal(new RequirementCardinalityOptions(Raise(req.Facet, spec.Schema), req.Cardinality.AuthoredFacet), req.Cardinality, key)).As()
            .Map(rows => (spec, spec.Applicability.Map(facet => Raise(facet, spec.Schema)), rows.Map(static row => (row.RelatedFacet, row))));

    static Fin<RequirementCardinalityOptions> Legal(RequirementCardinalityOptions row, IdsCardinality cardinality, Op key) =>
        row.GetAllowedCardinality().Contains(cardinality.AuthoredFacet)
            ? Fin.Succ(row)
            : new BimFault.Refused(key, BimScope.Review, BimReason.Rejected, string.Join(':', new object?[] { "cardinality-illegal", row.RelatedFacet.GetType().Name, cardinality.Key }));

    // The Xbim document builder is a mutable ObservableCollection graph, so this is the page's ONE statement-shaped
    // kernel — the platform-forced boundary named: the domain side arrives fully projected and every decision is
    // already settled, leaving the body a pure transcription.
    static byte[] Serialize(Seq<(IdsSpecification Spec, Seq<IFacet> Applicability, Seq<(IFacet Facet, RequirementCardinalityOptions Options)> Requirements)> raised) {
        Xids xids = new();
        foreach (var (spec, applicability, requirements) in raised) {
            Specification prepared = xids.PrepareSpecification(spec.Schema.FromIds());
            prepared.Name = spec.Name;
            prepared.Cardinality = new SimpleCardinality(spec.Cardinality.Authored);
            // An empty domain value writes null so the exporter omits the element, and a carried Identifier
            // re-stamps the SAME Guid rather than letting the lazy getter mint a fresh one.
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

    // The ONE foreign-document transcription and the page's only coalesce site: the Xbim record's authoring columns
    // are nullable strings the domain carries non-nullable. A document declaring no IFC version grades against
    // IFC4X3, the IDS v1.0 reference schema — the ONE default, stated here rather than pinned at six call sites. A
    // parsed specification is a contracted exchange requirement, so it lands at the blocking severity.
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

    // Either publishes a RIGHT-side Seq projection (RightToSeq) and NO left-side counterpart, so the drop side
    // folds through the two-arm Match — the arms are named because the signature orders Left first.
    static Seq<DroppedFacet> Dropped<R>(Either<DroppedFacet, R> row) =>
        row.Match(Left: static d => Seq(d), Right: static _ => Seq<DroppedFacet>());

    static Seq<Either<DroppedFacet, IdsFacet>> Lowered(FacetGroup? group, FacetGroup.FacetUse role, IfcSchemaVersions schema) =>
        Optional(group).Map(g => toSeq(g.Facets).Map(facet => FacetOf(facet, schema).MapLeft(reason => new DroppedFacet(role, facet.Short(), reason))))
            .IfNone(Seq<Either<DroppedFacet, IdsFacet>>());

    // Each requirement facet carries its OWN cardinality, read off the requirement FacetGroup per facet through
    // GetRequirementCardinalityOption — NOT the spec-level ICardinality applied to all (the deleted conflation): a
    // spec can require one facet, prohibit another, and leave a third optional in one pass. The SAME
    // GetAllowedCardinality legality table the Publish egress gates on runs HERE too, because this is the half that
    // receives a foreign document.
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
                RequirementCardinalityOptions.Cardinality.Expected   => IdsCardinality.Required,     // the IDS default: the facet must match
                RequirementCardinalityOptions.Cardinality.Prohibited => IdsCardinality.Prohibited,
                RequirementCardinalityOptions.Cardinality.Optional   => IdsCardinality.Optional,
                _                                                    => IdsCardinality.Required,     // forward-compat: an unknown future cardinality is treated as required
            }
            : IdsCardinality.Required;

    // The boundary map: each Xbim facet -> the closed IdsFacet arm, every ValueConstraint lowered HERE so the
    // interior is seam-typed. Every shape that cannot lower leaves as a NAMED DropReason on the Left rail rather
    // than a silent None the Choose discarded.
    static Either<DropReason, IdsFacet> FacetOf(IFacet facet, IfcSchemaVersions schema) => facet switch {
        IfcTypeFacet f                    => EntityOf(f, schema).Map(static e => (IdsFacet)e),
        // An attribute name restriction selecting NO seam ObjectAttribute row would fail every element as a false
        // negative (the seam node carries no such column), so it drops under its own reason rather than evaluating.
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

    // An entity facet resolving to NO rostered class is a typed UNKNOWN-CLASS outcome, never an empty class set: an
    // empty set lowers to a match-nothing predicate, and a match-nothing REQUIREMENT under the Prohibited row
    // passes every applicable element — a model-wide false PASS off one misspelled entity name.
    static Either<DropReason, IdsFacet.Entity> EntityOf(IfcTypeFacet f, IfcSchemaVersions schema) =>
        ResolveClasses(f.IfcType, f.IncludeSubtypes, schema) switch {
            { IsEmpty: true } => Left(DropReason.UnknownClass),
            var classes       => Right(new IdsFacet.Entity(classes, Predefineds(f.PredefinedType, classes, schema))),
        };

    // A code-bearing facet lowers its branch set and its authored REACH — IncludeSubClasses lands Pending because
    // this half reaches no dictionary; a SYSTEM-ONLY facet lowers the resolved system alone onto the
    // ByClassificationSystem membership arm; a facet with neither system nor code has nothing to select on.
    static Either<DropReason, IdsFacet> ClassificationFacet(IfcClassificationFacet f) {
        Option<string> system = SingleValue(f.ClassificationSystem).Filter(static s => !string.IsNullOrWhiteSpace(s)).Map(ResolveSystem);
        Seq<SeamClassification> branches = ClassificationBranches(f);
        return branches.IsEmpty && system.IsNone
            ? Left(DropReason.EmptyClassification)
            : Right((IdsFacet)new IdsFacet.Classification(branches, system,
                f.IncludeSubClasses is true && !branches.IsEmpty ? ClassificationReach.Awaiting : ClassificationReach.Declared));
    }

    // Every relation the Xbim parse resolves lowers through its policy row; the Undefined parse-fail sentinel — and
    // any future foreign member with no row — drops typed. A container entity facet that cannot resolve carries ITS
    // reason up, never a silent container-less PartOf that widens to match-any. The container is PROVED
    // nested-lowerable here, so a container restricted by a predefined token drops rather than widening to the
    // class alone; proving at admission is what keeps ToPredicate total.
    static Either<DropReason, IdsFacet> PartOfFacetOf(PartOfFacet f, IfcSchemaVersions schema) =>
        from relation in MapRelation(f.GetRelation()).ToEither(DropReason.ExtendedFacet)
        from container in Optional(f.EntityType).Match(
            Some: t => EntityOf(t, schema).Bind(e => IdsFacet.Nested(e).Match(
                Some: _ => Right<DropReason, Option<IdsFacet>>(Some((IdsFacet)e)),
                None: static () => Left<DropReason, Option<IdsFacet>>(DropReason.NestedContainer))),
            None: static () => Right<DropReason, Option<IdsFacet>>(None))
        select (IdsFacet)new IdsFacet.PartOf(container, relation);

    // The foreign-relation map DERIVES from the PartOfRelation rows' own Foreign column — one primary
    // correspondence, no parallel switch to keep in sync.
    static Option<PartOfRelation> MapRelation(PartOfFacet.PartOfRelation relation) =>
        toSeq(PartOfRelation.Items).Find(row => row.Foreign == relation);

    // Resolve an Xbim IfcType ValueConstraint to the seam IfcClass set under the SPEC's schema: an exact name
    // expands to its ids-lib concrete subtypes when IncludeSubtypes (the buildingSMART default), and a PATTERN name
    // expands against the schema's own class roster through the ONE seam matcher — the entity-side twin of the
    // classification reach, settled HERE because the IFC schema graph is offline where a dictionary is not.
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

    // Exact predefined tokens pass verbatim; a PATTERN token expands against the resolved classes' ids-lib
    // PredefinedTypeValues roster through the ONE seam matcher, so a patterned predefined facet lowers to typed
    // ByPredefinedType tokens instead of silently widening to a class-only match — a false PASS on a requirement
    // facet, the worst failure mode.
    static Seq<PredefinedType> Predefineds(ValueConstraint? predefined, Seq<IfcClass> classes, IfcSchemaVersions schema) {
        Seq<IValueConstraintComponent> components = Components(predefined);
        Seq<string> roster = classes.Bind(c => IdsSchema.PredefinedTokens(c.Key, schema)).Distinct();
        return components.Bind(component => component switch {
                ExactConstraint e   => Seq(e.Value),
                // A malformed predefined pattern mints no matcher and expands to no tokens — narrowing, never a
                // class-only widening — the same Lift admission the Unliftable gate runs.
                PatternConstraint p => ValueMatch.Pattern.Lift(p.Pattern).Map(matcher => Expand(matcher, roster)).IfNone(Seq<string>()),
                _                   => Seq<string>(),
            })
            .Distinct().Filter(static t => !string.IsNullOrWhiteSpace(t)).Map(static t => PredefinedType.Create(t));
    }

    // The one anchored NonBacktracking matcher built once, applied across the schema token roster.
    static Seq<string> Expand(ValueMatch matcher, Seq<string> roster) =>
        roster.Filter(token => matcher.Matches(new PropertyValue.Text(token)));

    // The IDS system name resolves through the Semantics/classification#CLASSIFICATION_AXIS roster onto the
    // canonical token the projector stamps, then a seam Classification admits per identification code through the
    // ONE railed door. Every branch here is the facet's OWN code set; the sub-branch closure is the Resolve step's.
    static Seq<SeamClassification> ClassificationBranches(IfcClassificationFacet f) =>
        SingleValue(f.ClassificationSystem).Filter(static s => !string.IsNullOrWhiteSpace(s)).Map(ResolveSystem).Match(
            Some: system => ExactValues(f.Identification).Filter(static c => !string.IsNullOrWhiteSpace(c))
                                .Map(code => SeamClassification.Of(system, code, IdsFacet.Lowering).ThrowIfFail()),
            None: static () => Seq<SeamClassification>());

    static string ResolveSystem(string name) =>
        toSeq(ClassificationSystem.Items)
            .Find(s => string.Equals(s.Title, name, StringComparison.OrdinalIgnoreCase) || string.Equals(s.Key, name, StringComparison.OrdinalIgnoreCase))
            .Map(static s => s.Key)
            .IfNone(name.Trim().ToLowerInvariant());

    // The SPEC-level cardinality, distinct from the per-facet requirement cardinality and ENFORCED by
    // IdsAudit.Conforms through SpecSatisfied. The Xbim ICardinality truth table is (ExpectsRequirements,
    // AllowsRequirements): Optional=(true,true), Required=(false,true), Prohibited=(false,false) — a naive
    // Expects:false -> Optional read swaps Required and Optional.
    static IdsCardinality Cardinality(ICardinality? cardinality) =>
        cardinality is { AllowsRequirements: false } ? IdsCardinality.Prohibited
        : cardinality is { ExpectsRequirements: true } ? IdsCardinality.Optional
        : IdsCardinality.Required;

    // --- [VALUE_LOWERING] -----------------------------------------------------------------
    // The IDS value engine, lowered ONCE: a ValueConstraint's accepted components fold onto the seam ValueMatch
    // family the query decides — an absent constraint is Present (existence), an all-exact set collapses to one
    // OneOf, otherwise each component lowers to its own ValueMatch the predicate ORs. A ValueConstraint never
    // crosses into the interior and IsSatisfiedBy is never called. An Unliftable component drops the WHOLE facet
    // under its own named reason: the ANY-component-matches fold cannot soundly skip one.
    static Either<DropReason, Seq<ValueMatch>> Matches(ValueConstraint? constraint, Option<string> dataType) {
        Seq<IValueConstraintComponent> components = Components(constraint);
        Seq<string> exacts = components.Choose(static c => c is ExactConstraint e ? Some(e.Value) : Option<string>.None);
        return components.Choose(c => Unliftable(c, dataType)).Head.Match(
            Some: Left<DropReason, Seq<ValueMatch>>,
            None: () => components.IsEmpty ? Right(Seq(ValueMatch.Any))
                : exacts.Count == components.Count ? Right(Seq<ValueMatch>(new ValueMatch.OneOf(exacts)))
                : Right(components.Map(c => Lower(c, dataType))));
    }

    // A NAME-position constraint lowers to ONE ValueMatch (the query ByProperty/ByAttribute name slots): a mixed
    // exact+pattern name constraint has no single-arm spelling and drops the facet.
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
        PatternConstraint p   => ValueMatch.Pattern.Lift(p.Pattern).IfNone(ValueMatch.Any),   // guaranteed Some behind the Unliftable gate
        RangeConstraint r     => new ValueMatch.Range(
            Bound(r.MinValue, dataType).Map(value => r.MinInclusive ? (RangeBound)new RangeBound.Inclusive(value) : new RangeBound.Exclusive(value)),
            Bound(r.MaxValue, dataType).Map(value => r.MaxInclusive ? (RangeBound)new RangeBound.Inclusive(value) : new RangeBound.Exclusive(value))),
        StructureConstraint s => s.TotalDigits is not null || s.FractionDigits is not null
                                     ? new ValueMatch.Digits(Optional(s.TotalDigits), Optional(s.FractionDigits))
                                     : new ValueMatch.Length(
                                           Optional(s.Length) | Optional(s.MinLength),   // xs:length is the exact bound — it wins both slots
                                           Optional(s.Length) | Optional(s.MaxLength)),
        _                     => ValueMatch.Any,
    };

    // Three faces of ONE law — a component that cannot become a single sound ValueMatch drops its whole facet. A
    // MIXED digits+length StructureConstraint would OR two partial matches into a false PASS. A MALFORMED pattern
    // is admission-gated (ValueMatch.Pattern has a private ctor), so an uncompilable XSD regex cannot exist as a
    // value the fold would mis-read as an ordinary non-match. An UNSATISFIABLE range lowers cleanly then admits no
    // value, and a match-nothing predicate under Prohibited hands EVERY applicable element to Passed — clean
    // conformance on a rule that graded nothing.
    static Option<DropReason> Unliftable(IValueConstraintComponent component, Option<string> dataType) =>
        component switch {
            StructureConstraint s when (s.TotalDigits is not null || s.FractionDigits is not null)
                                       && (s.Length is not null || s.MinLength is not null || s.MaxLength is not null) => Some(DropReason.MixedStructure),
            PatternConstraint p when ValueMatch.Pattern.Lift(p.Pattern).IsNone                                         => Some(DropReason.MalformedPattern),
            RangeConstraint r when Crossed(r, dataType)                                                                => Some(DropReason.UnsatisfiableRange),
            _                                                                                                         => None,
        };

    // Empty by construction: the bounds cross, or they coincide on an exclusive end. A one-sided or unparseable
    // bound is NOT crossed — an open range admits values and an unusable literal is the Bound None the range arm
    // already reads as unbounded, so only a provably empty interval drops.
    static bool Crossed(RangeConstraint r, Option<string> dataType) =>
        (from lo in Bound(r.MinValue, dataType)
         from hi in Bound(r.MaxValue, dataType)
         select lo.Si > hi.Si || (lo.Si == hi.Si && !(r.MinInclusive && r.MaxInclusive))).IfNone(false);

    // A numeric range bound carries its Dimension so the query's dimension-checked InRange compares like for like;
    // a bound with no resolvable datatype is Dimensionless. The OfSi Fin lowers to Option: a non-finite literal
    // bound is unusable evidence — None, never a swallowed default.
    static Option<MeasureValue> Bound(string? raw, Option<string> dataType) =>
        from text in Optional(raw)
        from value in Numeric(text, dataType)
        from bound in dataType.Bind(IdsSchema.DimensionOf).Match(
            Some: d => MeasureValue.OfSi(d, value),
            None: () => MeasureValue.OfSi(Dimension.Dimensionless, value)).ToOption()
        select bound;

    // IFC-datatype-aware literal coercion: TryGetNetType resolves the IFC datatype to its NetTypeName and
    // ParseValue coerces the literal in that type's value space (an IfcInteger bound parses integral, an
    // IfcLengthMeasure floating), the lazy BiBind None arm falling through to the invariant parse — never a bare
    // double.TryParse over a typed IFC literal.
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

    // The declared datatype for a range bound: the facet's own DataType, else the buildingSMART standard-Pset
    // declaration when the set+name are exact singles — so a datatype-less numeric range over
    // Pset_WallCommon.ThermalTransmittance is dimension-checked (W/(m2.K)), never silently Dimensionless.
    static Option<string> DataTypeOf(IfcPropertyFacet f, IfcSchemaVersions schema) =>
        Optional(f.DataType).Filter(static d => !string.IsNullOrWhiteSpace(d))
            .BiBind(Some: Some, None: () => from set in SingleValue(f.PropertySetName)
                                            from name in SingleValue(f.PropertyName)
                                            from declared in IdsSchema.StandardDatatype(set, name, schema)
                                            select declared);

    static Seq<IValueConstraintComponent> Components(ValueConstraint? constraint) =>
        Optional(constraint).Bind(static c => Optional(c.AcceptedValues)).Map(static a => toSeq(a)).IfNone(Seq<IValueConstraintComponent>());

    // The exact accepted literals of a ValueConstraint (the xs:enumeration / single-value components) — the class
    // names, classification codes, and standard-Pset join keys a structural arm needs as concrete keys.
    static Seq<string> ExactValues(ValueConstraint? constraint) =>
        Components(constraint).Choose(static c => c is ExactConstraint e ? Some(e.Value) : Option<string>.None);

    static Option<string> SingleValue(ValueConstraint? constraint) => ExactValues(constraint).Head;

    // --- [FACET_RAISE] ----------------------------------------------------------------------
    // The facet inverse the Publish egress folds: seam-typed data raises to the Xbim facet shape, the value matches
    // back to constraint components. An expanded concrete entity set collapses to its minimal supertypes under the
    // SPEC's own schema (TrySimplifyTopClasses), and the classification facet republishes the AUTHORED branch set
    // with its own IncludeSubClasses bit — raising the resolved descendants instead would turn one subclass-inclusive
    // requirement into a frozen leaf enumeration the next dictionary revision silently outdates. The schema threads
    // as Switch state, so every arm stays a closure-free static lambda.
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
                            ClassificationSystem = f.Branches.Head.Map(static b => new ValueConstraint(b.System)).ValueUnsafe(),
                            Identification = f.Branches.IsEmpty ? null : new ValueConstraint(f.Branches.Map(static b => b.Code)),
                            IncludeSubClasses = f.Reach.Subsumes,
                        },
        material:       static (_, f) => (IFacet)new MaterialFacet { Value = RaiseMatches(f.Value) },
        partOf:         static (s, f) => {
                            PartOfFacet raised = new() {
                                EntityType = f.Container.Bind(c => c is IdsFacet.Entity e ? Some((IfcTypeFacet)Raise(e, s)) : Option<IfcTypeFacet>.None).ValueUnsafe(),
                            };
                            raised.SetRelation(f.Relation.Foreign);
                            return (IFacet)raised;
                        });

    static ValueConstraint? RaiseName(ValueMatch name) => name switch {
        ValueMatch.OneOf o   => new ValueConstraint(o.Allowed),
        ValueMatch.Pattern p => ValueConstraint.CreatePattern(p.Expression),
        _                    => null,
    };

    // The ValueMatch inverse: each arm raises to its constraint component (a Range bound renders its SI magnitude
    // invariant round-trip, "R"); a Present-only value raises to the null constraint (existence). Inclusivity is
    // the RangeBound ARM, so the Switch reads the magnitude and the inclusive bit off ONE case and an Exclusive
    // bound can never publish as inclusive.
    static ValueConstraint? RaiseMatches(Seq<ValueMatch> value) {
        Seq<IValueConstraintComponent> components = value.Bind(static m => m switch {
            ValueMatch.OneOf o   => o.Allowed.Map(static a => (IValueConstraintComponent)new ExactConstraint(a)),
            ValueMatch.Pattern p => Seq<IValueConstraintComponent>(new PatternConstraint(p.Expression)),
            ValueMatch.Range r   => Seq<IValueConstraintComponent>(new RangeConstraint(
                                        RaiseBound(r.Lower).Magnitude, RaiseBound(r.Lower).Inclusive,
                                        RaiseBound(r.Upper).Magnitude, RaiseBound(r.Upper).Inclusive)),
            ValueMatch.Length l  => Seq<IValueConstraintComponent>(new StructureConstraint {
                                        MinLength = l.Min.Match<int?>(static v => v, static () => null),
                                        MaxLength = l.Max.Match<int?>(static v => v, static () => null),
                                    }),
            ValueMatch.Digits d  => Seq<IValueConstraintComponent>(new StructureConstraint {
                                        TotalDigits = d.Total.Match<int?>(static v => v, static () => null),
                                        FractionDigits = d.Fraction.Match<int?>(static v => v, static () => null),
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

    // The magnitude/inclusive pair is the foreign RangeConstraint constructor's OWN shape — a schema surface this
    // page transcribes, never a house carrier. An absent bound raises to the null magnitude it reads as unbounded.
    static (string? Magnitude, bool Inclusive) RaiseBound(Option<RangeBound> bound) =>
        bound.Match(
            Some: static b => b.Switch(
                inclusive: static i => ((string?)i.Value.Si.ToString("R", CultureInfo.InvariantCulture), true),
                exclusive: static e => ((string?)e.Value.Si.ToString("R", CultureInfo.InvariantCulture), false)),
            None: static () => ((string?)null, false));

    // The IDS-FILE audit (the spec document's own validity) is the buildingSMART-official ids-lib engine,
    // orthogonal to the MODEL audit: Audit.Run validates the .ids against the IDS v1.0 XSD + implementation
    // agreements, a BufferingLogger capturing the per-issue diagnostics off the engine's channel.
    public static Fin<IdsFileAudit> AuditFile(ReadOnlyMemory<byte> idsBytes, Op key) =>
        key.Catch(() => {
            using MemoryStream stream = new(idsBytes.ToArray());
            BufferingLogger sink = new();
            global::IdsLib.Audit.Status status = global::IdsLib.Audit.Run(stream, new SingleAuditOptions { IdsVersion = IdsVersion.Ids1_0 }, sink);
            return new IdsFileAudit(status, LibraryInformation.AssemblyVersion, sink.Drain());
        });
}

// The graded specification: Resolve is its ONLY mint and Audit its ONLY consumer, so a parsed specification whose
// IncludeSubClasses reach never settled cannot be graded. The gate is the TYPE, not a guard at each call: an audit
// over an unexpanded branch reports a clean pass on a requirement it covered only in part, which no runtime check
// scattered across callers reliably catches.
public sealed record IdsResolved {
    private IdsResolved(IdsSpecification specification) => Specification = specification;

    internal static IdsResolved Of(IdsSpecification specification) => new(specification);

    public IdsSpecification Specification { get; }

    // TOTAL model audit over the frozen seam graph: ElementQuery carries no rail and every facet's seam-typed
    // payload is admitted at Parse or settled at Resolve, so the audit is a pure fold. The applicable set is the
    // conjunction of the applicability facets (or every object when applicability is empty) SCOPED to the
    // IFC-visible universe; each requirement partitions it through its IdsCardinality.Partition row, and the join
    // key folds ONCE per requirement here because the schema the collapse needs is in hand exactly at this point.
    public IdsAudit Audit(ElementGraph graph) {
        IdsSpecification spec = Specification;
        // LanguageExt v5 Seq.Head is Option<IdsFacet>, so the seed predicate reads through Match (the empty arm is
        // the every-object set); the non-empty arm folds the tail with And.
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

    // The audit universe is the IFC-VISIBLE object set: verdict rows and the applicable-count rule
    // oracle all key on the IFC GlobalId, so an authored element carrying no ExternalId yet is OUTSIDE the IDS
    // exchange by definition — scoped ONCE here, never counted into ApplicableCount and then silently dropped from
    // the verdict whose Failed set decides Conforms.
    static ElementQuery IfcVisible(ElementQuery selected) =>
        selected.Where(BimLeaf.Of(new ElementLeaf.ByAttribute(
            new ValueMatch.Exact(new PropertyValue.Text(ObjectAttribute.GlobalId.Key)),
            ValueMatch.Any)));
}

// One captured ids-lib audit diagnostic: the level, the buildingSMART audit error code lifted TYPED off the
// structured log state (every ids-lib error/warning template leads with {errorCode}), and the rendered message.
public readonly record struct IdsDiagnostic(LogLevel Level, Option<int> Code, string Message);

// The IDS-FILE audit receipt: the ids-lib Status, the captured diagnostics, and the engine build the audit ran
// under so a stored receipt is reproducible. Status.Ok (the zero flag) is the pass; any error flag is the reject.
public sealed record IdsFileAudit(global::IdsLib.Audit.Status Status, string EngineVersion, Seq<IdsDiagnostic> Diagnostics) {
    public bool Conforms => Status == global::IdsLib.Audit.Status.Ok;
    public Seq<IdsDiagnostic> Errors => Diagnostics.Filter(static d => d.Level >= LogLevel.Error);
}

// Model is the graph the verdicts were produced over — the seam ContentAddress.OfGraph snapshot digest. Without it
// a stored verdict set names no model, so a re-audit after an edit cannot be told from a stale receipt and
// two runs over DIFFERENT graphs cannot be mistaken for one. The digest excludes the StepHeader/Instant provenance, so a
// re-export under a new timestamp keeps one identity while a semantic edit re-keys.
public sealed record IdsAudit(
    string Specification, int Spec, ContentAddress Model, IdsCardinality SpecCardinality, RuleSeverity Severity,
    int ApplicableCount, Seq<IdsAudit.FacetVerdict> Verdicts, Seq<DroppedFacet> Dropped) {
    public sealed record FacetVerdict(IdsFacet Facet, ContentAddress Key, IdsCardinality Cardinality, Seq<string> Passed, Seq<string> Failed);

    // A dropped facet makes the grading PARTIAL, so the specification is Indeterminate whatever the graded verdicts
    // say. Otherwise conformance is BOTH the spec-level applicable-count rule (the Xbim ICardinality.IsSatisfiedBy
    // truth) AND every requirement verdict passing.
    public IdsOutcome Outcome =>
        !Dropped.IsEmpty ? IdsOutcome.Indeterminate
        : SpecCardinality.SpecSatisfied(ApplicableCount) && Verdicts.ForAll(static v => v.Failed.IsEmpty) ? IdsOutcome.Conformant
        : IdsOutcome.NonConformant;

    public bool Conforms => Outcome.Conforms;
}

// --- [SERVICES] ---------------------------------------------------------------------------
// The boundary capture kernel: a buffering ILogger draining the ids-lib Audit.Run per-issue channel into typed
// IdsDiagnostic rows (the one mutable accumulation, contained at the logging boundary the engine writes to). Only
// Warning+ issues are captured (Information is progress noise); the no-op scope satisfies the ILogger contract.
file sealed class BufferingLogger : ILogger {
    readonly List<IdsDiagnostic> sink = [];

    public Seq<IdsDiagnostic> Drain() => toSeq(sink);
    public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Warning;

    // The ids-lib templates are structured ("Error {errorCode}: … on {location}."), so TState is the standard
    // KeyValuePair list and the errorCode value lifts typed; a non-template message carries None.
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

// --- [OPERATIONS] -------------------------------------------------------------------------
// The offline IFC schema authority (ids-lib IdsLib.IfcSchema): every facet reference resolves against the real
// schema graph the SPECIFICATION declares, never a hard-coded list and never a pinned version. The version is an
// ARGUMENT on every schema-dependent read, because grading an IFC2X3 exchange against IFC4X3 classes reports
// failures for entities the model's own schema never defined. IfcSchemaVersions is a FLAGS axis, so a
// multi-version specification resolves through GetSchemas and unions.
public static class IdsSchema {
    // Both rosters MEMOIZE on the schema-version key (the Semantics/properties#PROPERTY_TEMPLATES precedent): a
    // patterned entity or predefined facet expands against the WHOLE class graph, and Parse walks that expansion
    // once per patterned facet per specification. The cache is process-static and keyed on the flags value, so a
    // multi-version specification and a single-version one occupy distinct entries.
    static readonly ConcurrentDictionary<IfcSchemaVersions, Seq<string>> Classes = new();
    static readonly ConcurrentDictionary<(string Class, IfcSchemaVersions Schema), Seq<string>> Predefined = new();

    public static Seq<string> ConcreteClasses(string topClass, IfcSchemaVersions schema) =>
        toSeq(SchemaInfo.GetConcreteClassesFrom(topClass, schema));

    // Every class name the declared schema graphs carry — the roster a PATTERN-restricted entity name expands
    // against. SchemaInfo IS IEnumerable<ClassInfo>, so the graph enumerates its own classes with no accessor hop.
    public static Seq<string> ClassRoster(IfcSchemaVersions schema) =>
        Classes.GetOrAdd(schema, static key =>
            toSeq(SchemaInfo.GetSchemas(key)).Bind(static graph => toSeq(graph).Map(static c => c.Name)).Distinct());

    // The per-class predefined-token roster (ClassInfo.PredefinedTypeValues) a patterned predefined facet expands
    // against — the schema's own token space, so the expansion can never admit an out-of-schema token.
    public static Seq<string> PredefinedTokens(string className, IfcSchemaVersions schema) =>
        Predefined.GetOrAdd((className, schema), static key =>
            toSeq(SchemaInfo.GetSchemas(key.Schema))
                .Choose(graph => Optional(graph[key.Class]))
                .Bind(static c => Optional(c.PredefinedTypeValues).Map(static tokens => toSeq(tokens)).IfNone(Seq<string>()))
                .Distinct());

    // The buildingSMART standard-Pset property declaration: the declared IFC datatype of a SingleValuePropertyType
    // (an enumeration or reference property carries none) — the DataTypeOf fallback that closes the
    // datatype-less-range unit-safety gap. PropertySetInfo.Get keys on ONE version, so the flags axis resolves
    // through each declared graph's own Version and the first declaration wins.
    public static Option<string> StandardDatatype(string setName, string propertyName, IfcSchemaVersions schema) =>
        toSeq(SchemaInfo.GetSchemas(schema))
            .Map(static graph => graph.Version)
            .Choose(version => Optional(PropertySetInfo.Get(version, setName, propertyName)))
            .Choose(static p => p is SingleValuePropertyType single ? Some(single.DataType) : Option<string>.None)
            .Head;

    // The minimal-supertype collapse (TrySimplifyTopClasses): a concrete expansion folds back to its top classes
    // for a compact published Entity facet; an unsimplifiable set passes through verbatim.
    public static Seq<string> Simplify(Seq<string> concreteClasses, IfcSchemaVersions schema) =>
        SchemaInfo.TrySimplifyTopClasses(concreteClasses, schema, out IEnumerable<string> tops)
            ? toSeq(tops)
            : concreteClasses;

    // The IFC measure datatype -> seam Dimension: ids-lib carries each measure's SI dimensional exponents, lowered
    // onto the seam Dimension value-object so a range bound built from the declared datatype shares the candidate's
    // dimension (a length bound never satisfies a pressure candidate).
    public static Option<Dimension> DimensionOf(string ifcDataType) =>
        SchemaInfo.TryGetMeasureInformation(ifcDataType, out IfcMeasureInformation? info) && info is { Exponents: { } e }
            ? Some(Dimension.Create(e.Length, e.Mass, e.Time, e.ElectricCurrent, e.Temperature, e.AmountOfSubstance, e.LuminousIntensity))
            : Option<Dimension>.None;
}
```

## [03]-[MODEL_HEALTH]

- Owner: `ModelHealth` the three-tier model-QA receipt — the ONE model-health entry `Rasm.AppUi` and the review pipeline read; `ModelFinding` the closed `[Union]` verdict family EVERY tier converges on — `Structural` carrying the seam `Rasm.Element/Projection/audit#AUDIT_FOLD` `AuditFinding` row WHOLE (neutral graph integrity and coverage, graded at the graph's own owner), `Baseline` carrying the `Semantics/properties#TEMPLATE_AUDIT` `TemplateFinding` row WHOLE (produced there against the buildingSMART templates, composed here — the spec-free ground truth), `Authored` the per-element failed IDS requirement with its full typed evidence (`IdsFacet`, `IdsCardinality`, the ordinal join identity, the computed facet key) — the case IS the tier discriminant, so tier dispatch is the generated `Switch` and a parallel tier vocabulary beside the family is the deleted form; `Severity` the derived `RuleSeverity` band; `Coordinate` the per-finding report group key.
- Entry: `ModelHealth.Audit(ElementGraph graph, TemplateScope scope, Seq<IdsResolved> specifications, Func<IfcClass, Option<BsddClass>> dictionary, Op key)` runs all three tiers over the one frozen seam graph — the STRUCTURAL tier composes the seam `ModelAudit.Of(graph, key)` under its default structural thresholds, so every model carries the neutral integrity-and-coverage grade beneath any IFC claim; the baseline tier ALWAYS runs (an empty specification set still yields the zero-configuration verdict, so every model carries a health floor before any IDS is authored), each authored specification folding through its own total `IdsResolved.Audit` — the parameter type is the gate, so a caller cannot hand this entry a specification whose dictionary reach never settled; `scope` is the `Semantics/properties#PROPERTY_TEMPLATES` `TemplateScope` policy value threaded straight into `TemplateAudit.Run`, so a `Handover` audit grades COBie completeness on the SAME baseline fold a `Standard` audit runs; the `Fin` rails are the seam audit's uniform entry rail and the baseline's seam `Bake`.
- Auto: `Findings` flattens every tier onto the one `ModelFinding` stream — every seam `AuditFinding` a `Structural` case, every baseline row a `Baseline` case, every `FacetVerdict.Failed` element an `Authored` case carrying its requirement ordinal, its computed facet key, and its specification's enforcement band — a pure fold over the stored tier receipts; `Conforms` is the one verdict: NO blocking finding AND every `IdsAudit.Conforms`, so a spec-free template-floor gap advises where the prior empty-baseline rule failed a model that satisfied every authored specification.
- Receipt: `ModelHealth` stores the tier receipts typed — the seam `ModelAudit` whole, the `TemplateFinding` rows with their failing-axis evidence, and the `IdsAudit` rows with their cardinality rule and model snapshot digest — beside the `Scope` policy naming the baseline definition set. `Findings` is the derived projection, never a third stored copy.
- Packages: Rasm.Element, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm
- Growth: a new baseline verdict axis is the properties owner's `TemplateVerdict` row and rides the `Baseline` case untouched; a new enforcement band is one `RuleSeverity` row `[02]` owns; a new definition-set scope is one `TemplateScope` row the threaded policy value carries with zero edits here; a new facet, cardinality, relation, or dictionary reach rides the authored tier's own growth rows; a new neutral integrity class or coverage axis is the seam audit's own growth row and rides the `Structural` case untouched; a fourth verdict source is one `ModelFinding` case and one `Findings` arm with every consumer `Switch` broken loudly at compile time; never a second model-health entry, and never a per-tier report type.
- Boundary: the scope split with the seam audit is absolute — `Rasm.Element/Projection/audit#AUDIT_FOLD` `ModelAudit` owns neutral structural integrity and coverage ratios, while this page owns IFC semantics. The baseline stream is produced by `Semantics/properties#TEMPLATE_AUDIT`; this owner composes those rows. The convergence is the finding family, never the matcher vocabularies. Document-validity `AuditFile` stays independent of model audit.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using LanguageExt;
using Rasm.Bim.Model;
using Rasm.Bim.Semantics;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Thinktecture;
using Op = Rasm.Domain.Op;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [TYPES] ------------------------------------------------------------------------------
// The ONE model-health verdict family all three QA tiers converge on — the case IS the tier discriminant, so a
// consumer dispatches tier through the generated Switch. Baseline carries the Semantics-produced TemplateFinding
// row WHOLE (minted by TemplateAudit, never re-shaped here); Authored carries the per-element failed requirement
// with its full typed evidence. The identities differ BY DOMAIN: a baseline row keys the seam NodeId (the template
// audit covers the IFC-classified occurrence nodes, authored elements not yet exported included), an authored row
// the IFC GlobalId (the IDS exchange is defined over the IFC-visible universe).
[Union]
public partial record ModelFinding {
    partial record Structural(AuditFinding Finding);
    partial record Baseline(TemplateFinding Finding);
    partial record Authored(string GlobalId, string Specification, int Spec, int Requirement, ContentAddress Key, IdsFacet Facet, IdsCardinality Cardinality, RuleSeverity Severity);

    // The STRUCTURAL tier grades on the seam's own ConstraintSeverity, so its band is READ off the audit row's
    // Blocks column rather than re-decided here. The BASELINE tier is the spec-free floor — no party authored it as
    // an exchange requirement, so a missing standard property advises; the AUTHORED tier carries its
    // specification's own declared band, so a contracted requirement blocks while an advisory one reports.
    public RuleSeverity Severity => Switch(
        structural: static s => s.Finding.Severity.Blocks ? RuleSeverity.Error : RuleSeverity.Warning,
        baseline: static _ => RuleSeverity.Warning,
        authored: static a => a.Severity);

    // The per-finding report group key: the audit category for a structural row, the template coordinate for a
    // baseline row, the ordinal-qualified facet key for an authored row. The structural key is the seam's own
    // frozen category token, so a report and a waiver pin the same string the audit detail already keys on.
    public string Coordinate => Switch(
        structural: static s => s.Finding.Category.Key,
        baseline: static b => $"{b.Finding.Set}.{b.Finding.Code}",
        authored: static a => $"{a.Spec}:{a.Requirement}:{a.Key.Value:X32}");
}

// --- [MODELS] -----------------------------------------------------------------------------
// The three-tier model-QA receipt — the ONE entry Rasm.AppUi and the review pipeline read. Scope is the POLICY
// column the baseline graded under, stored because an empty baseline under the standard set and under a COBie
// handover answer different questions. The tier receipts stay typed evidence: the seam ModelAudit is stored WHOLE
// (its structural equality is the audit owner's own), the baseline TemplateFinding rows carry the failing axis a
// fix pass acts on; Findings is the derived one-family
// projection, never a fourth stored copy.
public sealed record ModelHealth(TemplateScope Scope, ModelAudit Structural, Seq<TemplateFinding> Baseline, Seq<IdsAudit> Authored) {

    // The three-tier entry over ONE frozen graph, under an absolute scope split: the seam audit owns what any
    // consumer can name (a dangling bag reference, a Compose cycle, an address drift), this page owns the IFC
    // claims only the schema authority can make. The baseline tier ALWAYS runs, and each authored spec arrives
    // ALREADY RESOLVED — the IdsResolved parameter is the gate, so an unexpanded dictionary branch cannot reach the
    // fold. scope threads straight into the one TemplateAudit.Run, so a handover audit grades COBie completeness on
    // the SAME fold that grades the standard sets.
    public static Fin<ModelHealth> Audit(ElementGraph graph, TemplateScope scope, Seq<IdsResolved> specifications, Func<IfcClass, Option<BsddClass>> dictionary, Op key) =>
        from structural in ModelAudit.Of(graph, key)
        from baseline in TemplateAudit.Run(graph, scope, dictionary, key)
        select new ModelHealth(scope, structural, baseline, specifications.Map(spec => spec.Audit(graph)));

    // The one flattened verdict stream: every seam audit row a Structural case, every baseline row a Baseline case,
    // every authored FacetVerdict's Failed element an Authored case carrying the requirement's ordinal identity,
    // its computed facet key, and its specification's enforcement band — a pure fold over the stored receipts.
    public Seq<ModelFinding> Findings =>
        Structural.Findings.Map(static f => (ModelFinding)new ModelFinding.Structural(f))
        + Baseline.Map(static f => (ModelFinding)new ModelFinding.Baseline(f))
        + Authored.Bind(static audit => audit.Verdicts
            .Map(static (v, i) => (Verdict: v, Requirement: i))
            .Bind(r => r.Verdict.Failed.Map(g => (ModelFinding)new ModelFinding.Authored(
                g, audit.Specification, audit.Spec, r.Requirement, r.Verdict.Key, r.Verdict.Facet, r.Verdict.Cardinality, audit.Severity))));

    // The one model-health verdict: no BLOCKING finding, and no authored specification left partially graded. A
    // template-floor gap advises rather than failing — the prior empty-baseline rule failed a model that satisfied
    // every authored specification because one standard property was absent — while an Indeterminate audit is never
    // a pass, because nothing graded it.
    public bool Conforms =>
        !Findings.Exists(static f => f.Severity.Blocking) && Authored.ForAll(static a => a.Conforms);
}
```

## [04]-[RESEARCH]

(none)
