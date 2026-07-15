# [BIM_VALIDATION]

The two-tier model-QA owner — ONE model-health verdict surface over the frozen seam graph. The BASELINE tier is the spec-free `Semantics/properties#TEMPLATE_AUDIT` `TemplateFinding` stream — produced there against the buildingSMART templates themselves, composed HERE beneath any authored specification, so every model carries a health floor before any IDS exists. The AUTHORED tier is buildingSMART IDS v1.0 whole: one `IdsSpecification` parsed from the `Xbim.InformationSpecifications` `Xids` document, its six facets folded into one closed `IdsFacet` `[Union]` that LOWERS each applicability/requirement facet onto the `Model/query#ELEMENT_SET` `ElementPredicate` algebra AT FULL ALGEBRA DEPTH — a patterned `Pset_.*` set or property name lowers WHOLE onto the three-`ValueMatch` `ByProperty`, a patterned attribute name onto `ByAttribute`, a patterned predefined token expands against the `ids-lib` per-class `PredefinedTypeValues` roster into typed `ByPredefinedType` arms, and ALL FIVE `PartOf` relations lower (`Contained` transitively via `BySpatialContainer`+`SpatialReach.Ancestry`, `Aggregated`/`Nested` via `ByComposed`, `Grouped` via `ByZone`, `Voided` via both `ByVoided` sides) with the container-entity facet lowering to `NodeMatch.Matching` directly — never a materialize-then-join, never a second selection surface, never a `ValueConstraint.IsSatisfiedBy` value engine beside the query. The lifecycle is the FULL round-trip: `Parse` admits foreign IDS bytes once, `Audit` folds the frozen seam graph totally over the IFC-visible universe — the three cardinalities as partition ROWS, Optional the conditional present-must-satisfy split over the `Presence()`-widened facet, never a pass-everything no-op — `Publish` raises the same closed family back through `Xids.PrepareSpecification`/`ExportBuildingSmartIDS` (the appointing-party authoring half — concrete entity expansions collapsed to minimal supertypes via `TrySimplifyTopClasses`, per-requirement cardinality written back through `FacetGroup.RequirementOptions` and gated by the package's `GetAllowedCardinality` legality table), `AuditFile` runs the buildingSMART-official `ids-lib` `Audit` over the document itself, and `IdsAudit.Reconcile` joins the ifctester oracle. Range bounds are UNIT-SAFE end to end: the facet `DataType` — or, when absent, the standard-Pset declaration `PropertySetInfo.Get` resolves — coerces through `ValueConstraint.TryGetNetType`/`ParseValue` onto the `ids-lib`-resolved SI `Dimension`, so a datatype-less `Pset_WallCommon.ThermalTransmittance` range is dimension-checked, never silently dimensionless. Only the boundary admissions carry the `Fin` rail, each foreign-byte fault lifting BARE as `Model/faults#FAULT_BAND` `BimFault.ModelRejected` (band 2600 IS the `Expected` `Code`, no `.ToError()` hop). The page composes the seam `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph`, the `Model/query#ELEMENT_SET` algebra, the `Model/elements#IFC_CLASS` roster, the `Semantics/classification#CLASSIFICATION_AXIS` projector, the `Semantics/properties#PROPERTY_TEMPLATES` template authority, and the `ids-lib` `IdsLib.IfcSchema` offline schema authority as settled vocabulary; the in-process fold is the immediate self-audit and the IfcOpenShell ifctester companion the deterministic cross-tool oracle the `IdsVerdict` rows carry. The page is HOST-LOCAL.

## [01]-[INDEX]

- [01]-[IDS_FACETS]: `IdsFacet` the closed `[Union]` of seam-lowered facets (each `ToPredicate()` a graph-free `ElementPredicate`, the value a `ValueMatch`), `PartOfRelation` the relation policy rows, `IdsRequirement`/`IdsSpecification` the spec records, `IdsSpecification.Parse`/`Publish` the bidirectional `Xids` boundary, `IdsSpecification.Audit` the total model-audit fold, `IdsSpecification.AuditFile` over `ids-lib` `Audit`, the `IdsSchema` offline schema authority, the `IdsAudit` receipt, and the `IdsAudit.Reconcile` -> `IdsParity` ifctester-oracle cross-tool diff over the Bim-owned `IdsVerdict` row.

## [02]-[IDS_FACETS]

- Owner: `IdsSpecification` the IDS specification record carrying the applicability and requirement facet sets and the cardinality; `IdsFacet` the closed `[Union]` (Entity, Attribute, Property, Classification, Material, PartOf) each carrying SEAM-LOWERED data — resolved `IfcClass`/`PredefinedType` sets, `ValueMatch` name restrictions (a patterned set/property/attribute name is a first-class `ValueMatch`, never a dropped facet), `ValueMatch` value restrictions, resolved `Classification` branches — admitted ONCE at parse so the interior never sees an `Xbim` `ValueConstraint`; `PartOfRelation` the `[SmartEnum<string>]` relation POLICY ROWS, each case carrying its query-arm lowering delegate AND its foreign `PartOfFacet.PartOfRelation` member as row data (the boundary map and the reverse raise both DERIVE from the one row set); each `IdsFacet` arm `ToPredicate()` lowering GRAPH-FREE to a `Model/query#ELEMENT_SET` `ElementPredicate`, `Presence()` deriving the value-widened conditional form the Optional cardinality partitions against, and `FacetKey` projecting the injective join token unique within its specification; `IdsAudit` the deterministic per-specification receipt carrying the spec-level `IdsCardinality`; `IdsFileAudit` the IDS-document validity receipt; `IdsVerdict` the per-(GlobalId, facet) cross-runtime oracle row the `csharp:Rasm.Compute/Runtime/codecs#TWO_HOP_TESSELLATION` `IdsAuditRequest` companion-rpc leg projects and COMPOSES from here (Bim owns the row, Compute references up-stratum); `IdsParity` the typed reconcile receipt joining self-audit against oracle on the (GlobalId, `FacetKey`) axis.
- Entry: `IdsSpecification.Audit(ElementGraph graph)` is TOTAL — it folds the applicability facets into one `ElementPredicate` scoped to the IFC-VISIBLE universe (`ExternalId`-bearing objects: the identity the verdict rows, the applicable-count rule, and the ifctester oracle share, so an unexported authored element is never counted into `ApplicableCount` then silently dropped from the `Failed` set that decides `Conforms`), queries through `ElementSet.Query(graph, predicate)`, refines by each requirement's `ToPredicate()` through `ElementSet.Where`, and partitions pass/fail through the `IdsCardinality.Partition` row over `(matched, applicable, presence)` — the presence thunk (`IdsFacet.Presence()`, the value-widened facet) forced only by the Optional row, whose conditional semantics fail exactly the present-but-violating elements; `IdsSpecification.Parse(ReadOnlyMemory<byte> idsBytes, Op key)` admits an IDS XML document through `Xids.LoadBuildingSmartIDS`, lowering every facet and every `ValueConstraint` onto the closed union (the one ingress boundary); `IdsSpecification.Publish(Seq<IdsSpecification> specifications, Op key)` is the ingress INVERSE — each facet raises through `Raise` onto its Xbim facet WITH its per-requirement cardinality written back through `FacetGroup.RequirementOptions` (the `IdsCardinality.AuthoredFacet` row column — a Prohibited requirement never republishes as the Expected default) and gated by the package's own `RequirementCardinalityOptions.GetAllowedCardinality` legality law (an Entity requirement is Expected-only, `PartOf` never Optional — an illegal pairing faults `ids-publish:cardinality-illegal` instead of emitting a non-conformant document), specs assemble through `Xids.PrepareSpecification` and serialize through `ExportBuildingSmartIDS`, so authored exchange requirements publish as standard `.ids` bytes for the CDE transmittal and the ifctester oracle; `IdsSpecification.AuditFile(ReadOnlyMemory<byte> idsBytes, Op key)` validates the document's own conformance through `ids-lib` `Audit.Run` onto an `IdsFileAudit` with `BufferingLogger`-captured `IdsDiagnostic` rows.
- Auto: `Parse` is the value-lowering boundary — `Matches` folds a facet's `ValueConstraint.AcceptedValues` onto `Seq<ValueMatch>` (an all-exact set collapses to one `OneOf`, a `PatternConstraint` to `Pattern`, a `RangeConstraint` to a dimension-checked `Range`, a length-bearing `StructureConstraint` to `Length`, an absent constraint to `Present`; a digits-bearing `StructureConstraint` has no seam spelling and drops the facet — an empty `Length` lowering would false-PASS); `NameMatch` lowers a NAME-position constraint to ONE `ValueMatch` so patterned names survive; `Predefineds` expands a patterned predefined token against the resolved classes' `IdsSchema.PredefinedTokens` roster through the ONE seam matcher (`ValueMatch.Pattern` — its cached anchored NonBacktracking compile, never a second regex engine); `DataTypeOf` resolves the range-bound datatype from the facet or the `PropertySetInfo.Get` standard-Pset declaration; `Numeric` coerces bound literals through `ValueConstraint.TryGetNetType`/`ParseValue` in the IFC datatype's value space; `ResolveClasses` expands an Entity facet's `IfcType` to its `IdsSchema.ConcreteClasses` subtypes when `IncludeSubtypes`; `ClassificationBranches` resolves the system through the `Semantics/classification#CLASSIFICATION_AXIS` roster; `Audit` then folds each facet's graph-free `ToPredicate()` — the validation fold reuses the query algebra for BOTH selection and value with one total `Switch`.
- Receipt: `IdsAudit` carries the specification name, the `Spec` document ordinal (the cross-runtime join identity — spec names are not unique in IDS v1.0), the spec-level `IdsCardinality`, the applicable element count, and the passed/failed `GlobalId` sets per facet; `IdsAudit.Conforms` is BOTH the spec-level applicable-count rule (`SpecSatisfied`) AND every requirement verdict passing; `IdsAudit.Reconcile(Seq<IdsVerdict> oracle)` folds the companion ifctester projection against the self-audit into an `IdsParity` receipt on the (GlobalId, `FacetKey`) axis — `IdsParity.Conformant` is the cross-tool parity verdict, never a message diff; `IdsFileAudit.Conforms`/`Errors` reads the `Status` plus the captured diagnostics.
- Packages: Xbim.InformationSpecifications, ids-lib, Microsoft.Extensions.Logging.Abstractions, Rasm.Element, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm
- Growth: a new IDS facet is one `IdsFacet` union arm lowering to its `ElementPredicate` arm plus its `Matches` lowering, its `Presence()` widening, its `FacetKey` token, and its `Raise` inverse; a new PartOf relation is one `PartOfRelation` row (delegate + foreign member — zero switch edits); a new value-match modality is one `ValueMatch` arm the seam already folds; a new cardinality is one `IdsCardinality` row carrying its `(matched, applicable, presence)` partition, spec rule, AND both authored inverses (`CardinalityEnum` + `RequirementCardinalityOptions.Cardinality`); the cross-tool audit is one companion-rpc shape over the existing `csharp:Rasm.Compute/Runtime/codecs#TWO_HOP_TESSELLATION` pattern; never a second validation predicate surface, never a second value engine, never a hand-rolled IDS parser or writer, and never a transport minted here.
- Boundary: the validation predicate IS the `Model/query#ELEMENT_SET` `ElementPredicate` — an `IdsValidator`/`IdsRule` evaluation family or a second selection surface is the deleted form; the facet lowering rides the algebra's OWN depth — `ByProperty`/`ByAttribute` carry `ValueMatch` name restrictions so a patterned name lowers whole (the prior exact-single `SingleValue` drop is the deleted form), the `PartOf` container lowers to `NodeMatch.Matching` as case-owned recursion (the prior `ElementSet.Query`-then-fold-per-id materialize-then-join is the query page's named deleted form), and `Aggregated`/`Nested`/`Voided` lower through `ByComposed`/`ByVoided` (the prior parse-drop retired by the query algebra's growth); the VALUE match is the seam `ValueMatch` — a `ValueConstraint.IsSatisfiedBy(candidate, …)` engine, a `String.Equals` compare, or a stringly `Candidate(BimElement)` walk beside the query is the deleted form; the model `Audit` reads the seam `ElementGraph` the `Projection/semantic#SEMANTIC_PROJECTOR` assembled and the retired `BimModel.Elements`/`BimElement` record is GONE; the IDS document parse AND publish are `Xids` (`LoadBuildingSmartIDS`/`PrepareSpecification`+`ExportBuildingSmartIDS`) — a hand-rolled `XmlReaderSettings.Schemas`/`XDocument` parser or writer is the retired form; the IDS-FILE audit is the buildingSMART-official `ids-lib` `Audit.Run`, orthogonal to the MODEL audit; the facet's IFC entity/predefined/property/measure references resolve against the real `ids-lib` `IdsLib.IfcSchema` graph (`GetConcreteClassesFrom`/`ClassInfo.PredefinedTypeValues`/`PropertySetInfo.Get`/`TryGetMeasureInformation`/`TrySimplifyTopClasses`) through the `IdsSchema` helper — never a hard-coded class list, never a hand pattern engine beside the seam matcher; the `DocumentFacet`/`IfcRelationFacet` Xbim EXTENDED facets sit outside buildingSMART IDS v1.0 and drop EXPLICITLY at parse; the cross-tool audit routes to the IfcOpenShell ifctester companion over Compute's existing companion rpc — the `IdsVerdict` row is the one seam contract this owner OWNS and Compute composes up-stratum, so a positional verdict-list compare or a Compute-side `IdsAudit` re-projection is the deleted form; the `IdsAudit`/`IdsFileAudit` receipts are the typed validation evidence, never a generic `IReceipt`.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using IdsLib;
using IdsLib.IfcSchema;
using LanguageExt;
using Rasm.Bim.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Rasm.Element.Classification;
using Rasm.Element.Graph;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using Thinktecture;
using Xbim.InformationSpecifications;
using Xbim.InformationSpecifications.Cardinality;
using static LanguageExt.Prelude;
using Op = Rasm.Domain.Op;
using SeamClassification = Rasm.Element.Classification.Classification;   // the seam (system, code) value-object — aliased so the
                                                         // nested IdsFacet.Classification arm never shadows it.

namespace Rasm.Bim;

// --- [TYPES] ------------------------------------------------------------------------------
// The IDS cardinality vocabulary owns ALL FOUR policies as row data: `Partition` the requirement-level pass/fail
// split over (matched, applicable, presence-thunk), `SpecSatisfied` the spec-level applicable-count rule (the Xbim
// ICardinality.IsSatisfiedBy truth), and the two authored inverses the Publish egress writes back — `Authored` the
// spec-level CardinalityEnum, `AuthoredFacet` the per-requirement RequirementCardinalityOptions row value. Optional
// is the CONDITIONAL requirement (present-must-satisfy): only the elements carrying the facet's feature with a
// violating value FAIL — a pass-everything Optional row would false-PASS the buildingSMART semantics — and only
// this row forces the presence thunk, so Required/Prohibited never pay the second fold.
[SmartEnum<string>]
public sealed partial class IdsCardinality {
    public static readonly IdsCardinality Required   = new("required",   static (matched, applicable, _) => (matched, applicable.Except(matched)), static count => count > 0,  CardinalityEnum.Required,   RequirementCardinalityOptions.Cardinality.Expected);
    public static readonly IdsCardinality Prohibited = new("prohibited", static (matched, applicable, _) => (applicable.Except(matched), matched), static count => count == 0, CardinalityEnum.Prohibited, RequirementCardinalityOptions.Cardinality.Prohibited);
    public static readonly IdsCardinality Optional   = new("optional",   static (matched, applicable, present) => present().Except(matched) switch {
                                                           var violating => (applicable.Except(violating), violating),
                                                       }, static _ => true, CardinalityEnum.Optional, RequirementCardinalityOptions.Cardinality.Optional);

    public Func<ElementSet, ElementSet, Func<ElementSet>, (ElementSet Pass, ElementSet Fail)> Partition { get; }
    public Func<int, bool> SpecSatisfied { get; }
    public CardinalityEnum Authored { get; }
    public RequirementCardinalityOptions.Cardinality AuthoredFacet { get; }
}

// The neutral PartOf-relation POLICY ROWS: each case carries its query-arm lowering as delegate data and its
// foreign Xbim member as a data column, so the ingress map (MapRelation) and the egress raise (SetRelation) both
// DERIVE from this one row set. ALL FIVE relations lower — the query algebra's ByComposed/ByVoided/NodeMatch
// growth retired the Aggregated/Nested/Voided parse-drop: Contained lowers TRANSITIVELY (SpatialReach.Ancestry —
// an IDS partOf storey holds for a space-contained element), Voided accepts either Void-axis side (the IDS
// IfcRelVoidsFillsElement pairs the feature-voids-host and filler-fills-opening reads).
[SmartEnum<string>]
public sealed partial class PartOfRelation {
    public static readonly PartOfRelation Contained  = new("Contained",  PartOfFacet.PartOfRelation.IfcRelContainedInSpatialStructure, static t => new ElementPredicate.BySpatialContainer(t, SpatialReach.Ancestry));
    public static readonly PartOfRelation Aggregated = new("Aggregated", PartOfFacet.PartOfRelation.IfcRelAggregates,                  static t => new ElementPredicate.ByComposed(ComposeKind.Aggregate, t));
    public static readonly PartOfRelation Nested     = new("Nested",     PartOfFacet.PartOfRelation.IfcRelNests,                       static t => new ElementPredicate.ByComposed(ComposeKind.Nest, t));
    public static readonly PartOfRelation Grouped    = new("Grouped",    PartOfFacet.PartOfRelation.IfcRelAssignsToGroup,              static t => new ElementPredicate.ByZone(t));
    public static readonly PartOfRelation Voided     = new("Voided",     PartOfFacet.PartOfRelation.IfcRelVoidsFillsElement,           static t => new ElementPredicate.Any(Seq<ElementPredicate>(
                                                           new ElementPredicate.ByVoided(VoidKind.Void, t), new ElementPredicate.ByVoided(VoidKind.Fill, t))));

    public PartOfFacet.PartOfRelation Foreign { get; }

    [UseDelegateFromConstructor]
    public partial ElementPredicate Lower(NodeMatch container);
}

// The closed IDS facet family — the mirror of the six buildingSMART facets carrying SEAM-LOWERED data only: the
// Xbim ValueConstraint is admitted ONCE at IdsSpecification.Parse and never crosses into this interior. Name
// positions are ValueMatch (a patterned Pset_.* set/property/attribute name lowers WHOLE — the query ByProperty/
// ByAttribute carry name restrictions natively), value positions Seq<ValueMatch>. Each arm ToPredicate() lowers
// GRAPH-FREE to ONE Model/query#ELEMENT_SET ElementPredicate, so the validation predicate IS the query predicate.
[Union]
public partial record IdsFacet {
    partial record Entity(Seq<IfcClass> Classes, Seq<PredefinedType> Predefined);
    partial record Attribute(ValueMatch Name, Seq<ValueMatch> Value);
    partial record Property(ValueMatch Set, ValueMatch Name, Seq<ValueMatch> Value);
    partial record Classification(Seq<SeamClassification> Branches, Option<string> System);   // empty Branches + Some System = the system-only (no-identification) facet
    partial record Material(Seq<ValueMatch> Value);
    partial record PartOf(Option<IdsFacet> Container, PartOfRelation Relation);

    // The ONE lowering to the query algebra: a value-bearing arm folds its Seq<ValueMatch> into an Any of the
    // matching predicate, the Entity arm crosses its class set with its predefined tokens, and PartOf lowers its
    // container facet to NodeMatch.Matching DIRECTLY — case-owned recursion inside the one algebra, retiring the
    // materialize-then-join that ran a container query and folded per-id arms; an absent container is the
    // match-any nested predicate (an empty All), and the relation's own policy row selects the incidence arm.
    public ElementPredicate ToPredicate() => Switch(
        entity:         static f => AnyOf(f.Classes.Bind(cls => f.Predefined.IsEmpty
                            ? Seq<ElementPredicate>(new ElementPredicate.ByClass(cls))
                            : f.Predefined.Map(pt => (ElementPredicate)new ElementPredicate.ByPredefinedType(cls, pt)))),
        attribute:      static f => AnyOf(f.Value.Map(vm => (ElementPredicate)new ElementPredicate.ByAttribute(f.Name, vm))),
        property:       static f => AnyOf(f.Value.Map(vm => (ElementPredicate)new ElementPredicate.ByProperty(f.Set, f.Name, vm))),
        classification: static f => f.Branches.IsEmpty
                            ? f.System.Match(
                                Some: static s => (ElementPredicate)new ElementPredicate.ByClassificationSystem(s),
                                None: static () => new ElementPredicate.Any(Seq<ElementPredicate>()))
                            : AnyOf(f.Branches.Map(b => (ElementPredicate)new ElementPredicate.ByClassification(b))),
        material:       static f => AnyOf(f.Value.Map(vm => (ElementPredicate)new ElementPredicate.ByMaterial(vm))),
        partOf:         static f => f.Relation.Lower(f.Container.Match(
                            Some: static c => (NodeMatch)c.ToPredicate(),
                            None: static () => (NodeMatch)new ElementPredicate.All(Seq<ElementPredicate>()))));

    // The value-widened PRESENCE form the Optional cardinality partitions against — the facet with its value
    // restriction relaxed to Any, so "carries the feature" separates from "carries it satisfying". Attribute/
    // Property/Material widen the value slot; Entity and PartOf return the facet whole (their Optional is
    // schema-illegal — the Xbim GetAllowedCardinality law the Publish gate enforces — so presence never
    // partitions them); Classification widens to system-only membership (ByClassificationSystem — "classified in
    // the system at all"), falling back whole only when no system resolved.
    public IdsFacet Presence() => Switch(
        entity:         static f => (IdsFacet)f,
        attribute:      static f => f with { Value = Seq(ValueMatch.Any) },
        property:       static f => f with { Value = Seq(ValueMatch.Any) },
        classification: static f => f.System.IsSome
                            ? f with { Branches = Seq<SeamClassification>() }
                            : (IdsFacet)f,
        material:       static _ => new IdsFacet.Material(Seq(ValueMatch.Any)),
        partOf:         static f => (IdsFacet)f);

    // The cross-runtime join token an IdsAudit verdict and the companion IdsVerdict share on the (GlobalId, facet)
    // axis. INJECTIVITY LAW: the token folds EVERY identity-bearing slot — kind, names, value restrictions, relation,
    // container (case-owned recursion) — so two DISTINCT facets in one specification never render one token (duplicate
    // identical facets merge harmlessly: their verdicts coincide); entity classes render through IdsSchema.Simplify so
    // the token matches the AUTHORED document form both runtimes read. The csharp:Rasm.Compute/Runtime/codecs
    // #TWO_HOP_TESSELLATION IdsAuditRequest leg projects IdsVerdict.Facet in THIS exact format — owned HERE, mirrored there.
    public string FacetKey => Switch(
        entity:         static f => $"entity:{string.Join('|', IdsSchema.Simplify(f.Classes.Map(static c => c.Key)))}:{string.Join('|', f.Predefined.Map(static p => p.Token))}",
        attribute:      static a => $"attribute:{KeyOf(a.Name)}:{string.Join(',', a.Value.Map(KeyOf))}",
        property:       static p => $"property:{KeyOf(p.Set)}:{KeyOf(p.Name)}:{string.Join(',', p.Value.Map(KeyOf))}",
        classification: static c => $"classification:{c.System.IfNone(() => c.Branches.Head.Map(static b => b.System).IfNone(string.Empty))}:{string.Join('|', c.Branches.Map(static b => b.Code))}",
        material:       static m => $"material:{string.Join(',', m.Value.Map(KeyOf))}",
        partOf:         static p => $"partOf:{p.Relation.Key}:{p.Container.Map(static c => c.FacetKey).IfNone("*")}");

    // A ValueMatch renders deterministically: exacts their literals, a pattern its expression, a range its SI
    // bounds ("R" invariant — the same rendering the Raise inverse emits), a length its character bounds, the
    // match-any "*" — the stable per-slot identity the reconcile join reads.
    static string KeyOf(ValueMatch match) => match switch {
        ValueMatch.OneOf o   => string.Join('|', o.Allowed),
        ValueMatch.Pattern p => p.Expression,
        ValueMatch.Range r   => $"[{Rendered(r.Lower)}..{Rendered(r.Upper)}]",
        ValueMatch.Length l  => $"len:{l.Min.Map(static v => v.ToString(CultureInfo.InvariantCulture)).IfNone("")}..{l.Max.Map(static v => v.ToString(CultureInfo.InvariantCulture)).IfNone("")}",
        ValueMatch.Digits d  => $"digits:{d.Total.Map(static v => v.ToString(CultureInfo.InvariantCulture)).IfNone("")}.{d.Fraction.Map(static v => v.ToString(CultureInfo.InvariantCulture)).IfNone("")}",
        _                    => "*",
    };

    static string Rendered(Option<MeasureValue> bound) =>
        bound.Map(static b => b.Si.ToString("R", CultureInfo.InvariantCulture)).IfNone("");

    // An empty arm set is a facet whose every lowering resolved to nothing (an unrostered entity name) — it
    // matches nothing rather than smuggling a raw Func<Node.Object,bool> walk past the query surface.
    static ElementPredicate AnyOf(Seq<ElementPredicate> arms) =>
        arms.IsEmpty ? new ElementPredicate.Any(Seq<ElementPredicate>())
        : arms.Tail.IsEmpty ? arms.Head.IfNone(new ElementPredicate.Any(Seq<ElementPredicate>()))
        : new ElementPredicate.Any(arms);
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record IdsRequirement(IdsFacet Facet, IdsCardinality Cardinality);

// Ordinal is the ZERO-BASED document position Parse stamps — the spec identity the cross-runtime join keys on,
// because IDS v1.0 does NOT require spec names unique within a document (two same-named specs would cross-join).
public sealed record IdsSpecification(
    string Name,
    Seq<IdsFacet> Applicability,
    Seq<IdsRequirement> Requirements,
    IdsCardinality Cardinality,
    int Ordinal = 0) {

    // TOTAL model audit over the frozen seam graph: ElementSet.Query/Where/Except carry no rail and every facet's
    // seam-typed payload is admitted at Parse (no audit-time mint), so the audit is a pure fold. The applicable set
    // is the conjunction of the applicability facets (or every object when applicability is empty), SCOPED to the
    // IFC-visible universe; each requirement partitions the applicable set through its IdsCardinality.Partition row
    // (the presence thunk forced only by Optional), the verdict GlobalIds projected off the selected ExternalId set,
    // and the spec-level Cardinality rides onto the receipt for the applicable-count rule.
    public IdsAudit Audit(ElementGraph graph) {
        // LanguageExt v5 `Seq.Head` is `Option<IdsFacet>`, so the seed predicate reads through `Match` (the empty
        // arm is the every-object set); the non-empty arm folds the tail with `And`.
        ElementSet applicable = IfcVisible(Applicability.Head.Match(
            None: () => ElementSet.Query(graph, new ElementPredicate.All(Seq<ElementPredicate>())),
            Some: head => ElementSet.Query(graph, Applicability.Tail.Fold(
                head.ToPredicate(),
                static (predicate, facet) => predicate.And(facet.ToPredicate())))));
        Seq<IdsAudit.FacetVerdict> verdicts = Requirements.Map(req => {
            ElementSet matched = applicable.Where(req.Facet.ToPredicate());
            (ElementSet pass, ElementSet fail) = req.Cardinality.Partition(matched, applicable, () => applicable.Where(req.Facet.Presence().ToPredicate()));
            return new IdsAudit.FacetVerdict(req.Facet, req.Cardinality, pass.GlobalIds, fail.GlobalIds);
        });
        return new IdsAudit(Name, Ordinal, Cardinality, applicable.Count, verdicts);
    }

    // The audit universe is the IFC-VISIBLE object set: verdict rows, the applicable-count rule, and the ifctester
    // oracle all key on the IFC GlobalId, so an authored element carrying no ExternalId yet is OUTSIDE the IDS
    // exchange by definition — scoped ONCE here, never counted into ApplicableCount and then silently dropped from
    // the verdict whose Failed set decides Conforms (the counted-but-invisible false PASS is the deleted defect).
    static ElementSet IfcVisible(ElementSet selected) =>
        selected.Where(new ElementPredicate.ByAttribute(
            new ValueMatch.Exact(new PropertyValue.Text(ObjectAttribute.GlobalId.Key)),
            ValueMatch.Any));

    // The IDS document parse composes Xbim.InformationSpecifications `Xids` (the buildingSMART IDS v1.0 schema
    // binding) and is the ONE ingress boundary that lowers every facet's `ValueConstraint` onto seam-typed data —
    // retiring the hand-rolled XmlReaderSettings.Schemas/XDocument parser. The fault lifts BARE (band 2600 IS the Code).
    public static Fin<Seq<IdsSpecification>> Parse(ReadOnlyMemory<byte> idsBytes, Op key) =>
        Try.lift(() => {
            using MemoryStream stream = new(idsBytes.ToArray());
            Xids xids = Xids.LoadBuildingSmartIDS(stream, NullLogger.Instance)
                ?? throw new InvalidDataException("ids-load-empty");
            // Document order IS the spec identity the ordinal-qualified join keys on — stamped once at parse.
            return xids.AllSpecifications().Select(static (spec, i) => Project(spec) with { Ordinal = i }).ToSeq();
        }).Run().MapFail(error => new BimFault.ModelRejected(key, $"ids-parse:{error.Message}"));

    // The authoring egress — the ingress INVERSE: each facet raises through Raise onto its Xbim facet, specs
    // assemble through Xids.PrepareSpecification (the groups created and wired by the factory) and serialize
    // through ExportBuildingSmartIDS, so the appointing-party half of the IDS lifecycle (author exchange
    // requirements from Rasm vocabulary, publish .ids to the CDE, feed the ifctester oracle) rides the SAME
    // closed family. Each requirement writes BOTH halves back — its facet plus its per-facet cardinality through
    // the FacetGroup.RequirementOptions row and the IdsCardinality.AuthoredFacet column (a Prohibited requirement
    // republishing at the Expected default would INVERT its meaning); the spec-level rule writes back through
    // Authored. Per-facet cardinality LEGALITY gates through the package's own GetAllowedCardinality (an Entity
    // requirement is Expected-only, PartOf never Optional — the IDS v1.0 schema law), so an illegal pairing faults
    // typed at Publish instead of emitting a non-conformant .ids the AuditFile leg would only catch downstream.
    public static Fin<byte[]> Publish(Seq<IdsSpecification> specifications, Op key) =>
        Try.lift(() => {
            Xids xids = new();
            specifications.Iter(spec => {
                Specification prepared = xids.PrepareSpecification(IfcSchemaVersion.IFC4X3);
                prepared.Name = spec.Name;
                prepared.Cardinality = new SimpleCardinality(spec.Cardinality.Authored);
                spec.Applicability.Iter(facet => prepared.Applicability.Facets.Add(Raise(facet)));
                FacetGroup requirement = prepared.Requirement!;
                ObservableCollection<RequirementCardinalityOptions> options = requirement.RequirementOptions ??= [];
                spec.Requirements.Iter(req => {
                    IFacet raised = Raise(req.Facet);
                    RequirementCardinalityOptions row = new(raised, req.Cardinality.AuthoredFacet);
                    _ = row.GetAllowedCardinality().Contains(req.Cardinality.AuthoredFacet)
                        ? unit : throw new InvalidDataException($"cardinality-illegal:{raised.GetType().Name}:{req.Cardinality.Key}");
                    requirement.Facets.Add(raised);
                    options.Add(row);
                });
            });
            using MemoryStream sink = new();
            xids.ExportBuildingSmartIDS(sink, NullLogger.Instance);
            return sink.ToArray();
        }).Run().MapFail(error => new BimFault.ModelRejected(key, $"ids-publish:{error.Message}"));

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

    // The boundary map: each Xbim facet -> the closed IdsFacet arm, every ValueConstraint lowered HERE so the
    // interior is seam-typed. Only three shapes still drop, each named: an attribute whose name restriction
    // selects NO seam ObjectAttribute row (the seam node carries no such column — evaluating it would fail every
    // element as a false negative), a mixed exact+pattern NAME constraint (no single-ValueMatch spelling; a
    // digits+length-MIXED StructureConstraint drops on the same one-arm law — see Unliftable, the pure digits
    // facet now lowering onto ValueMatch.Digits), and the Xbim EXTENDED DocumentFacet/IfcRelationFacet (outside
    // buildingSMART IDS v1.0 — deliberately unmodeled).
    static Option<IdsFacet> FacetOf(IFacet facet) => facet switch {
        IfcTypeFacet f                    => Some((IdsFacet)EntityOf(f)),
        AttributeFacet f                  => from name in NameMatch(f.AttributeName)
                                                 .Filter(static name => toSeq(ObjectAttribute.Items).Exists(row => name.Matches(new PropertyValue.Text(row.Key))))
                                             from value in Matches(f.AttributeValue, None)
                                             select (IdsFacet)new IdsFacet.Attribute(name, value),
        IfcPropertyFacet f                => from set in NameMatch(f.PropertySetName)
                                             from name in NameMatch(f.PropertyName)
                                             from value in Matches(f.PropertyValue, DataTypeOf(f))
                                             select (IdsFacet)new IdsFacet.Property(set, name, value),
        IfcClassificationFacet f          => ClassificationFacet(f),
        MaterialFacet f                   => Matches(f.Value, None).Map(static value => (IdsFacet)new IdsFacet.Material(value)),
        PartOfFacet f                     => PartOfFacetOf(f),
        DocumentFacet or IfcRelationFacet => Option<IdsFacet>.None,
        _                                 => Option<IdsFacet>.None,
    };

    static IdsFacet.Entity EntityOf(IfcTypeFacet f) {
        Seq<IfcClass> classes = ResolveClasses(f.IfcType, f.IncludeSubtypes);
        return new IdsFacet.Entity(classes, Predefineds(f.PredefinedType, classes));
    }

    // A code-bearing facet lowers its branch set; a SYSTEM-ONLY facet (no identification) lowers the resolved
    // system alone onto the ByClassificationSystem membership arm — the parse-time drop this closes; only a facet
    // with neither system nor code drops (nothing to select on).
    static Option<IdsFacet> ClassificationFacet(IfcClassificationFacet f) {
        Option<string> system = SingleValue(f.ClassificationSystem).Filter(static s => !string.IsNullOrWhiteSpace(s)).Map(ResolveSystem);
        Seq<SeamClassification> branches = ClassificationBranches(f);
        return branches.IsEmpty && system.IsNone
            ? Option<IdsFacet>.None
            : Some((IdsFacet)new IdsFacet.Classification(branches, system));
    }

    // Every relation the Xbim parse resolves lowers through its policy row; only the Undefined parse-fail
    // sentinel — and any future foreign member with no row — drops, so an unparseable EntityRelation never
    // mis-lowers onto a wrong-semantics arm.
    static Option<IdsFacet> PartOfFacetOf(PartOfFacet f) =>
        MapRelation(f.GetRelation()).Map(relation => (IdsFacet)new IdsFacet.PartOf(
            Optional(f.EntityType).Map(static t => (IdsFacet)EntityOf(t)),
            relation));

    // The foreign-relation map DERIVES from the PartOfRelation rows' own Foreign column — one primary
    // correspondence, no parallel switch to keep in sync.
    static Option<PartOfRelation> MapRelation(PartOfFacet.PartOfRelation relation) =>
        PartOfRelation.Items.ToSeq().Find(row => row.Foreign == relation);

    // Resolve an Xbim IfcType ValueConstraint to the seam IfcClass set, expanding each accepted entity name to its
    // ids-lib concrete subtypes when IncludeSubtypes (the buildingSMART default), dropping a name the IfcClass
    // roster does not carry (the projector stamps only rostered keys, so an unrostered facet selects nothing anyway).
    static Seq<IfcClass> ResolveClasses(ValueConstraint? type, bool includeSubtypes) =>
        ExactValues(type)
            .Bind(name => includeSubtypes ? IdsSchema.ConcreteClasses(name).Add(name) : Seq(name))
            .Distinct()
            .Choose(IfcClass.TryGet);

    // Exact predefined tokens pass verbatim; a PATTERN token expands against the resolved classes' ids-lib
    // PredefinedTypeValues roster through the ONE seam matcher (ValueMatch.Pattern — its cached anchored
    // NonBacktracking compile), so a patterned predefined facet lowers to typed ByPredefinedType tokens instead
    // of silently widening to a class-only match — a false PASS on a requirement facet, the worst failure mode.
    static Seq<PredefinedType> Predefineds(ValueConstraint? predefined, Seq<IfcClass> classes) {
        Seq<IValueConstraintComponent> components = Components(predefined);
        Seq<string> roster = classes.Bind(static c => IdsSchema.PredefinedTokens(c.Key)).Distinct();
        return components.Bind(component => component switch {
                ExactConstraint e   => Seq(e.Value),
                // The admitted-pattern probe: a malformed predefined pattern mints no matcher and expands to no
                // tokens (narrowing, never a class-only widening), the same Lift admission the Unliftable gate runs.
                PatternConstraint p => ValueMatch.Pattern.Lift(p.Pattern).Map(matcher => Expand(matcher, roster)).IfNone(Seq<string>()),
                _                   => Seq<string>(),
            })
            .Distinct().Filter(static t => !string.IsNullOrWhiteSpace(t)).Map(static t => PredefinedType.Create(t));
    }

    // The one anchored NonBacktracking matcher built once, applied across the schema token roster.
    static Seq<string> Expand(ValueMatch matcher, Seq<string> roster) =>
        roster.Filter(token => matcher.Matches(new PropertyValue.Text(token)));

    // Resolve the IDS classification system name through the Semantics/classification#CLASSIFICATION_AXIS roster to
    // the same canonical token the projector stamps onto Object.Classification, then mint a seam Classification
    // branch per identification code. The query ByClassification uses Within (prefix), which realizes the
    // IncludeSubClasses=true sub-branch inclusion; a facet with no system or no code yields no branch (see [03]-[RESEARCH]).
    static Seq<SeamClassification> ClassificationBranches(IfcClassificationFacet f) =>
        SingleValue(f.ClassificationSystem).Filter(static s => !string.IsNullOrWhiteSpace(s)).Map(ResolveSystem).Match(
            Some: system => ExactValues(f.Identification).Filter(static c => !string.IsNullOrWhiteSpace(c)).Map(code => SeamClassification.Create(system, code, "", None, None, None)),
            None: static () => Seq<SeamClassification>());

    static string ResolveSystem(string name) =>
        ClassificationSystem.Items.ToSeq()
            .Find(s => string.Equals(s.Title, name, StringComparison.OrdinalIgnoreCase) || string.Equals(s.Key, name, StringComparison.OrdinalIgnoreCase))
            .Map(static s => s.Key)
            .IfNone(name.Trim().ToLowerInvariant());

    // The SPEC-level cardinality (the whole specification's applicability rule), distinct from the per-facet
    // requirement cardinality; carried on IdsSpecification.Cardinality and ENFORCED by IdsAudit.Conforms through
    // SpecSatisfied. The Xbim ICardinality truth table is (ExpectsRequirements, AllowsRequirements):
    // Optional=(true,true), Required=(false,true), Prohibited=(false,false) — a naive Expects:false→Optional read
    // swaps Required and Optional.
    static IdsCardinality Cardinality(ICardinality? cardinality) =>
        cardinality is { AllowsRequirements: false } ? IdsCardinality.Prohibited
        : cardinality is { ExpectsRequirements: true } ? IdsCardinality.Optional
        : IdsCardinality.Required;

    // --- [VALUE_LOWERING] -----------------------------------------------------------------
    // The IDS value engine, lowered ONCE: a ValueConstraint's accepted components fold onto the seam ValueMatch
    // family the query decides — an absent/empty constraint is Present (existence), an all-exact set collapses to
    // one OneOf, otherwise each component lowers to its own ValueMatch the predicate ORs. A ValueConstraint never
    // crosses into the interior and IsSatisfiedBy is never called — the query's typed ValueMatch is the only matcher.
    // An Unliftable component drops the WHOLE facet (None): the ANY-component-matches fold cannot soundly skip one.
    static Option<Seq<ValueMatch>> Matches(ValueConstraint? constraint, Option<string> dataType) {
        Seq<IValueConstraintComponent> components = Components(constraint);
        if (components.Exists(Unliftable)) {
            return Option<Seq<ValueMatch>>.None;
        }
        if (components.IsEmpty) {
            return Some(Seq(ValueMatch.Any));
        }
        Seq<string> exacts = components.Choose(static c => c is ExactConstraint e ? Some(e.Value) : Option<string>.None);
        return Some(exacts.Count == components.Count
            ? Seq<ValueMatch>(new ValueMatch.OneOf(exacts))
            : components.Map(c => Lower(c, dataType)));
    }

    // A NAME-position constraint lowers to ONE ValueMatch (the query ByProperty/ByAttribute name slots): absent ->
    // Any, all-exact -> OneOf, a single component -> its arm; a mixed exact+pattern name constraint has no
    // single-arm spelling and drops the facet — the narrow honest residue replacing the prior any-pattern drop.
    static Option<ValueMatch> NameMatch(ValueConstraint? constraint) {
        Seq<IValueConstraintComponent> components = Components(constraint);
        Seq<string> exacts = components.Choose(static c => c is ExactConstraint e ? Some(e.Value) : Option<string>.None);
        return components.Exists(Unliftable) ? Option<ValueMatch>.None
            : components.IsEmpty ? Some(ValueMatch.Any)
            : exacts.Count == components.Count ? Some<ValueMatch>(new ValueMatch.OneOf(exacts))
            : components.Tail.IsEmpty ? components.Head.Map(c => Lower(c, None))
            : Option<ValueMatch>.None;
    }

    static ValueMatch Lower(IValueConstraintComponent component, Option<string> dataType) => component switch {
        ExactConstraint e     => new ValueMatch.OneOf(Seq(e.Value)),
        PatternConstraint p   => ValueMatch.Pattern.Lift(p.Pattern).IfNone(ValueMatch.Any),   // guaranteed Some behind the Unliftable gate — a malformed pattern never reaches Lower
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

    // A StructureConstraint MIXING digits and length facets has no single-arm spelling (one component lowers to
    // ONE ValueMatch; splitting would OR two partial matches — a false PASS, the worst failure mode) — the narrow
    // honest drop. A pure digits facet lowers onto ValueMatch.Digits, a pure length facet onto Length. A
    // MALFORMED pattern is unliftable on the same law: ValueMatch.Pattern is admission-gated (private ctor), so
    // an uncompilable or NonBacktracking-unsupported XSD regex drops the whole facet here — the named accounting —
    // rather than existing as a value the fold could only mis-read as an ordinary non-match.
    static bool Unliftable(IValueConstraintComponent component) =>
        component switch {
            StructureConstraint s => (s.TotalDigits is not null || s.FractionDigits is not null)
                                     && (s.Length is not null || s.MinLength is not null || s.MaxLength is not null),
            PatternConstraint p   => ValueMatch.Pattern.Lift(p.Pattern).IsNone,
            _                     => false,
        };

    // A numeric range bound carries its Dimension so the query's dimension-checked InRange compares like for like:
    // the declared datatype (facet-carried or standard-Pset-resolved) resolves through ids-lib to the SI 7-vector;
    // a bound with no resolvable datatype is Dimensionless (it compares against a Count candidate).
    static Option<MeasureValue> Bound(string? raw, Option<string> dataType) =>
        from text in Optional(raw)
        from value in Numeric(text, dataType)
        // The OfSi Fin lowers to Option: a non-finite literal bound is unusable evidence — None, never a swallowed default.
        from bound in dataType.Bind(IdsSchema.DimensionOf).Match(
            Some: d => MeasureValue.OfSi(d, value),
            None: () => MeasureValue.OfSi(Dimension.Dimensionless, value)).ToOption()
        select bound;

    // IFC-datatype-aware literal coercion: TryGetNetType resolves the IFC datatype to its NetTypeName and
    // ParseValue coerces the literal in that type's value space (an IfcInteger bound parses integral, an
    // IfcLengthMeasure floating); a numeric result lowers to the SI double, a datatype-less or non-numeric
    // literal falls through the lazy BiBind None arm to the invariant parse — never a bare double.TryParse
    // over a typed IFC literal.
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

    // The declared datatype for a range bound: the facet's own DataType, else (the lazy BiBind None arm) the
    // buildingSMART standard-Pset declaration (PropertySetInfo.Get) when the set+name are exact singles — so a
    // datatype-less numeric range over Pset_WallCommon.ThermalTransmittance is dimension-checked (W/(m2.K)),
    // never silently Dimensionless.
    static Option<string> DataTypeOf(IfcPropertyFacet f) =>
        Optional(f.DataType).Filter(static d => !string.IsNullOrWhiteSpace(d))
            .BiBind(Some: Some, None: () => from set in SingleValue(f.PropertySetName)
                                            from name in SingleValue(f.PropertyName)
                                            from declared in IdsSchema.StandardDatatype(set, name)
                                            select declared);

    static Seq<IValueConstraintComponent> Components(ValueConstraint? constraint) =>
        Optional(constraint).Bind(static c => Optional(c.AcceptedValues)).Map(static a => a.ToSeq()).IfNone(Seq<IValueConstraintComponent>());

    // The exact accepted literals of a ValueConstraint (the xs:enumeration / single-value components) — the class
    // names, classification codes, and standard-Pset join keys a structural arm needs as concrete keys.
    static Seq<string> ExactValues(ValueConstraint? constraint) =>
        Components(constraint).Choose(static c => c is ExactConstraint e ? Some(e.Value) : Option<string>.None);

    static Option<string> SingleValue(ValueConstraint? constraint) => ExactValues(constraint).Head;

    // --- [FACET_RAISE] ----------------------------------------------------------------------
    // The facet inverse the Publish egress folds: seam-typed data raises to the Xbim facet shape, the value
    // matches back to constraint components. An expanded concrete entity set collapses to its minimal supertypes
    // (TrySimplifyTopClasses) + IncludeSubtypes, so a published facet reads as authored, never a 40-leaf enumeration;
    // the PartOf relation writes back through its row's Foreign member and SetRelation.
    static IFacet Raise(IdsFacet facet) => facet.Switch(
        entity:         static f => (IFacet)new IfcTypeFacet {
                            IfcType = new ValueConstraint(IdsSchema.Simplify(f.Classes.Map(static c => c.Key))),
                            PredefinedType = f.Predefined.IsEmpty ? null : new ValueConstraint(f.Predefined.Map(static p => p.Token)),
                            IncludeSubtypes = true,
                        },
        attribute:      static f => (IFacet)new AttributeFacet { AttributeName = RaiseName(f.Name), AttributeValue = RaiseMatches(f.Value) },
        property:       static f => (IFacet)new IfcPropertyFacet { PropertySetName = RaiseName(f.Set), PropertyName = RaiseName(f.Name), PropertyValue = RaiseMatches(f.Value) },
        classification: static f => (IFacet)new IfcClassificationFacet {
                            ClassificationSystem = f.Branches.Head.Map(static b => new ValueConstraint(b.System)).IfNoneUnsafe((ValueConstraint?)null),
                            Identification = f.Branches.IsEmpty ? null : new ValueConstraint(f.Branches.Map(static b => b.Code)),
                            IncludeSubClasses = true,
                        },
        material:       static f => (IFacet)new MaterialFacet { Value = RaiseMatches(f.Value) },
        partOf:         static f => {
                            PartOfFacet raised = new() {
                                EntityType = f.Container.Bind(static c => c is IdsFacet.Entity e ? Some((IfcTypeFacet)Raise(e)) : Option<IfcTypeFacet>.None).IfNoneUnsafe((IfcTypeFacet?)null),
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
    // invariant round-trip, "R"); a Present-only value raises to the null constraint (existence).
    static ValueConstraint? RaiseMatches(Seq<ValueMatch> value) {
        Seq<IValueConstraintComponent> components = value.Bind(static m => m switch {
            ValueMatch.OneOf o   => o.Allowed.Map(static a => (IValueConstraintComponent)new ExactConstraint(a)),
            ValueMatch.Pattern p => Seq<IValueConstraintComponent>(new PatternConstraint(p.Expression)),
            ValueMatch.Range r   => Seq<IValueConstraintComponent>(new RangeConstraint(
                                        r.Lower.MatchUnsafe(static b => b.Si.ToString("R", CultureInfo.InvariantCulture), static () => null), r.LowerInclusive,
                                        r.Upper.MatchUnsafe(static b => b.Si.ToString("R", CultureInfo.InvariantCulture), static () => null), r.UpperInclusive)),
            ValueMatch.Length l  => Seq<IValueConstraintComponent>(new StructureConstraint {
                                        MinLength = l.Min.MatchUnsafe(static v => (int?)v, static () => null),
                                        MaxLength = l.Max.MatchUnsafe(static v => (int?)v, static () => null),
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

// One captured ids-lib audit diagnostic: the level, the buildingSMART audit error code lifted TYPED off the
// structured log state (every ids-lib error/warning template leads with {errorCode}), and the rendered message —
// the line-level evidence the IDS-FILE audit carries so a non-conformance reads its coded reason, never a bare boolean.
public readonly record struct IdsDiagnostic(LogLevel Level, Option<int> Code, string Message);

// The IDS-FILE audit receipt: the ids-lib Status, the captured diagnostics, and the engine build the audit ran
// under so a stored receipt is reproducible. Status.Ok (the zero flag) is the pass; any error flag is the reject.
public sealed record IdsFileAudit(global::IdsLib.Audit.Status Status, string EngineVersion, Seq<IdsDiagnostic> Diagnostics) {
    public bool Conforms => Status == global::IdsLib.Audit.Status.Ok;
    public Seq<IdsDiagnostic> Errors => Diagnostics.Filter(static d => d.Level >= LogLevel.Error);
}

public sealed record IdsAudit(string Specification, int Spec, IdsCardinality SpecCardinality, int ApplicableCount, Seq<IdsAudit.FacetVerdict> Verdicts) {
    public sealed record FacetVerdict(IdsFacet Facet, IdsCardinality Cardinality, Seq<string> Passed, Seq<string> Failed);

    // Conformance is BOTH the spec-level applicable-count rule (a Required spec needs >=1 applicable element, a
    // Prohibited spec none — the Xbim ICardinality.IsSatisfiedBy truth) AND every requirement verdict passing; the
    // spec-level rule is enforced here rather than left as a stored-but-unread field.
    public bool Conforms => SpecCardinality.SpecSatisfied(ApplicableCount) && Verdicts.ForAll(static v => v.Failed.IsEmpty);

    // The cross-tool outer join on the ORDINAL-QUALIFIED (GlobalId, Requirement, FacetKey) axis, oracle rows
    // filtered by the Spec DOCUMENT ordinal — never the spec NAME (IDS v1.0 does not require names unique, so two
    // same-named specs would cross-join) — and keyed by the requirement's position within the spec (a byte-identical
    // facet duplicated under two cardinalities is two requirements sharing one FacetKey; the ordinal splits them).
    // Both sides derive the ordinals from the same document order, so the join is injective by construction.
    public IdsParity Reconcile(Seq<IdsVerdict> oracle) {
        Seq<((string GlobalId, int Requirement, string Facet), bool Passed)> mine = Verdicts
            .Select(static (v, i) => (Verdict: v, Requirement: i)).ToSeq()
            .Bind(r => r.Verdict.Passed.Map(g => ((g, r.Requirement, r.Verdict.Facet.FacetKey), true))
                     + r.Verdict.Failed.Map(g => ((g, r.Requirement, r.Verdict.Facet.FacetKey), false)));
        Seq<((string GlobalId, int Requirement, string Facet), bool Passed)> theirs = oracle
            .Filter(o => o.Spec == Spec)
            .Map(o => ((o.GlobalId, o.Requirement, o.Facet), o.Passed));
        // LanguageExt ships ONLY the tuple-form ToHashMap over IEnumerable<(K,V)> — the two-selector overload is a phantom.
        HashMap<(string, int, string), bool> mineMap = mine.Map(static r => (r.Item1, r.Passed)).ToHashMap();
        HashMap<(string, int, string), bool> theirMap = theirs.Map(static r => (r.Item1, r.Passed)).ToHashMap();
        Seq<(string, int, string)> keys = (mine.Map(static r => r.Item1) + theirs.Map(static r => r.Item1)).Distinct();
        return new IdsParity(Specification,
            keys.Map(key => new IdsParity.Row(key.Item1, key.Item2, key.Item3, mineMap.Find(key), theirMap.Find(key))));
    }
}

// The per-(GlobalId, requirement) cross-runtime audit row — OWNED HERE (Bim is the IDS authority and cannot reference
// the up-stratum Rasm.Compute) and COMPOSED by the csharp:Rasm.Compute/Runtime/codecs#TWO_HOP_TESSELLATION
// IdsAuditRequest leg, which projects the IfcOpenShell ifctester oracle into this exact shape. Spec is the
// specification's ZERO-BASED document ordinal and Requirement the facet's ordinal within it — both derived from the
// one document order the two tools share — so the join survives duplicate spec names and cardinality-duplicated
// facets; Facet stays the injective FacetKey evidence column.
public readonly record struct IdsVerdict(string GlobalId, string Specification, int Spec, int Requirement, string Facet, bool Passed, string Reason);

public sealed record IdsParity(string Specification, Seq<IdsParity.Row> Rows) {
    public readonly record struct Row(string GlobalId, int Requirement, string Facet, Option<bool> SelfAudit, Option<bool> Oracle) {
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
    readonly List<IdsDiagnostic> sink = [];

    public Seq<IdsDiagnostic> Drain() => sink.ToSeq();
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
// IFC4.3 schema graph, never a hard-coded list — subtype expansion, per-class predefined-token rosters, the
// standard-Pset property declarations, measure dimensions, and the minimal-supertype collapse the Publish egress
// compacts an expanded entity set with.
public static class IdsSchema {
    public static Seq<string> ConcreteClasses(string topClass) =>
        SchemaInfo.GetConcreteClassesFrom(topClass, IfcSchemaVersions.Ifc4x3).ToSeq();

    // The per-class predefined-token roster (ClassInfo.PredefinedTypeValues) a patterned predefined facet
    // expands against — the schema's own token space, so the expansion can never admit an out-of-schema token.
    public static Seq<string> PredefinedTokens(string className) =>
        Optional(SchemaInfo.SchemaIfc4x3[className])
            .Bind(static c => Optional(c.PredefinedTypeValues))
            .Map(static tokens => tokens.ToSeq())
            .IfNone(Seq<string>());

    // The buildingSMART standard-Pset property declaration: the declared IFC datatype of a SingleValuePropertyType
    // (an enumeration or reference property carries none) — the DataTypeOf fallback that closes the
    // datatype-less-range unit-safety gap.
    public static Option<string> StandardDatatype(string setName, string propertyName) =>
        Optional(PropertySetInfo.Get(IfcSchemaVersions.Ifc4x3, setName, propertyName))
            .Bind(static p => p is SingleValuePropertyType single ? Some(single.DataType) : Option<string>.None);

    // The minimal-supertype collapse (TrySimplifyTopClasses): a concrete expansion folds back to its top classes
    // for a compact published Entity facet; an unsimplifiable set passes through verbatim.
    public static Seq<string> Simplify(Seq<string> concreteClasses) =>
        SchemaInfo.TrySimplifyTopClasses(concreteClasses, IfcSchemaVersions.Ifc4x3, out IEnumerable<string> tops)
            ? tops.ToSeq()
            : concreteClasses;

    // The IFC measure datatype -> seam Dimension: ids-lib carries each measure's SI dimensional exponents, lowered
    // onto the seam Dimension value-object so a range bound built from the declared datatype shares the candidate's
    // dimension (a length bound never satisfies a pressure candidate) — the same SI metadata the property store reads.
    public static Option<Dimension> DimensionOf(string ifcDataType) =>
        SchemaInfo.TryGetMeasureInformation(ifcDataType, out IfcMeasureInformation? info) && info is { Exponents: { } e }
            ? Some(Dimension.Create(e.Length, e.Mass, e.Time, e.ElectricCurrent, e.Temperature, e.AmountOfSubstance, e.LuminousIntensity))
            : Option<Dimension>.None;
}
```

## [03]-[RESEARCH]

- [IDS_SPEC_MODEL]: the IDS v1.0 spec model is the `Xbim.InformationSpecifications` `Xids` document (`.api/api-xbim-informationspecifications`) — `Xids.LoadBuildingSmartIDS(Stream, ILogger?)` parses the buildingSMART IDS XML, `AllSpecifications()` enumerates each `Specification` (an applicability `FacetGroup` + a requirement `FacetGroup` + an `ICardinality`), and the six `: FacetBase, IFacet` facets carry their match fields as `ValueConstraint`s: `IfcTypeFacet` (`IfcType`/`PredefinedType` + `IncludeSubtypes`), `AttributeFacet` (`AttributeName`/`AttributeValue`), `IfcPropertyFacet` (`PropertySetName`/`PropertyName`/`PropertyValue` + `DataType : string?`), `IfcClassificationFacet` (`ClassificationSystem`/`Identification` + `IncludeSubClasses`), `MaterialFacet` (`Value`), and `PartOfFacet` (`EntityType : IfcTypeFacet?`, `GetRelation()`/`SetRelation(PartOfRelation)` over `IfcRelAggregates`/`IfcRelAssignsToGroup`/`IfcRelContainedInSpatialStructure`/`IfcRelNests`/`IfcRelVoidsFillsElement`); the AUTHORING half is the same document — `Xids.PrepareSpecification(IfcSchemaVersion.IFC4X3)` creates and wires the spec (both facet groups minted by the factory), `Specification.Name`/`Cardinality` settable (`SimpleCardinality(CardinalityEnum)` over `Required`/`Optional`/`Prohibited`), `ExportBuildingSmartIDS(Stream, ILogger?)` serializes — so `Parse` and `Publish` are one bidirectional `Xids` boundary and a hand-rolled IDS XML reader OR writer is the deleted form; each requirement facet's cardinality is read PER FACET through `FacetGroup.GetRequirementCardinalityOption` (`Expected`/`Prohibited`/`Optional`, the IDS default `Expected`) rather than the spec-level `ICardinality` applied to every requirement (the deleted conflation), and WRITTEN back per facet at `Publish` through `FacetGroup.RequirementOptions` rows (`RequirementCardinalityOptions(IFacet, Cardinality)`) so the requirement cardinality round-trips — a Prohibited requirement republishing at the Expected default inverts its meaning, the deleted lossy egress; `RequirementCardinalityOptions.GetAllowedCardinality()` is the package's own per-facet-kind legality table (an `IfcTypeFacet` requirement admits ONLY `Expected`, a `PartOfFacet` never `Optional`, every other facet all three — the IDS v1.0 schema law) and `Publish` gates every written row through it, so an illegal kind-cardinality pairing faults typed at authoring rather than surfacing as an `AuditFile` non-conformance; `FacetGroup.IsValid()` additionally requires a non-empty `RequirementOptions` collection to match the facet count exactly, which the one-row-per-raised-facet `Publish` loop satisfies by construction; the Xbim EXTENDED `DocumentFacet`/`IfcRelationFacet` sit outside buildingSMART IDS v1.0 and drop EXPLICITLY at `FacetOf` — deliberately unmodeled, never a silent `_` fallthrough.
- [VALUE_LOWERING]: the IDS value engine lowers onto the seam `Model/query#ELEMENT_SET` `ValueMatch` family — `ValueConstraint.AcceptedValues` is satisfied if ANY component matches, so an all-`ExactConstraint` set collapses to one `ValueMatch.OneOf`, a `PatternConstraint.Pattern` to `ValueMatch.Pattern` through the query's admission-gated `Pattern.Lift` (the whole-value-anchored NonBacktracking regex compiled ONCE per pattern — the same cached matcher the `Predefineds` roster expansion reuses, never a second regex engine; a malformed pattern is `Unliftable` and drops the whole facet, the named accounting, because the query's private-ctor admission makes it unrepresentable as a matcher), a `RangeConstraint` to `ValueMatch.Range`, a pure length-bearing `StructureConstraint` to `ValueMatch.Length`, and a pure digits-bearing one to `ValueMatch.Digits` (only the digits+length-MIXED single component drops the whole facet per `Unliftable`: one component lowers to ONE arm, and an OR of two partial matches would false-PASS the requirement); NAME positions lower through `NameMatch` onto the query's `ByProperty(Set, Name, Restriction)`/`ByAttribute(Attribute, Restriction)` `ValueMatch` slots so a patterned `Pset_.*` facet lowers WHOLE (the prior exact-single `SingleValue` drop retired by the query algebra's three-restriction growth); a patterned PREDEFINED token expands against the per-class `ClassInfo.PredefinedTypeValues` roster into typed `ByPredefinedType` arms (a pattern that silently widened to class-only was a false PASS on requirements); the range bound is UNIT-SAFE end to end — the declared datatype (facet `DataType`, else the `PropertySetInfo.Get(version, set, prop)` standard-Pset `SingleValuePropertyType.DataType` declaration) coerces the literal through `ValueConstraint.TryGetNetType(dataType, out NetTypeName)`/`ValueConstraint.ParseValue(text, netType)` in the IFC datatype's value space and resolves through `SchemaInfo.TryGetMeasureInformation` → `IfcMeasureInformation.Exponents` (`DimensionalExponents` `Length`/`Mass`/`Time`/`ElectricCurrent`/`Temperature`/`AmountOfSubstance`/`LuminousIntensity`) → seam `Dimension.Create` so the query's dimension-checked `InRange` compares like for like; `IncludeSubtypes` expands via `SchemaInfo.GetConcreteClassesFrom(topClass, IfcSchemaVersions.Ifc4x3)` and `IncludeSubClasses` rides the seam `Classification.Within` prefix containment; `ValueConstraint.IsSatisfiedBy` is never called — the typed `ValueMatch` is the only matcher.
- [IDS_FILE_SCHEMA_AUTHORITY]: the IDS-FILE audit and the IFC schema truth are the buildingSMART-official `ids-lib` (`.api/api-ids-lib`) — `Audit.Run(Stream, SingleAuditOptions { IdsVersion = IdsVersion.Ids1_0 }, ILogger)` validates the `.ids` document against the IDS v1.0 XSD plus the implementation agreements onto a `[Flags]` `Audit.Status` (`Ok = 0` the pass), a `BufferingLogger` capturing the per-issue rows into the `IdsFileAudit` receipt — the buildingSMART `{errorCode}` structured value every ids-lib template leads with lifted TYPED onto `IdsDiagnostic.Code` — and `LibraryInformation.AssemblyVersion` stamping the engine build for reproducibility; the embedded `IdsLib.IfcSchema` `SchemaInfo` graph is the offline IFC4.3 schema authority the `IdsSchema` helper resolves EVERY facet reference against — `GetConcreteClassesFrom` the subtype expansion, `SchemaIfc4x3[className].PredefinedTypeValues` the per-class token roster, `PropertySetInfo.Get` the standard-Pset property declarations, `TryGetMeasureInformation` the measure dimensions, `TrySimplifyTopClasses` the minimal-supertype collapse the `Publish` egress compacts an expanded entity set with — so the property-template/measure metadata reconciles with the `Semantics/properties#PROPERTY_TEMPLATES` `UnitsNet` SI coercion: `ids-lib` supplies the schema truth, the seam `MeasureValue` the value conversion.
- [IFCTESTER_COMPANION]: the cross-runtime seam shape is settled and Bim-owned — `IdsVerdict(GlobalId, Specification, Spec, Requirement, Facet, Passed, Reason)` is declared HERE because `Rasm.Bim` is the IDS authority and the strata forbid an AEC-DOMAIN owner referencing the APP-PLATFORM `Rasm.Compute`; the `csharp:Rasm.Compute/Runtime/codecs#TWO_HOP_TESSELLATION` `IdsAuditRequest` leg (APP-PLATFORM, depends up on Bim) COMPOSES this row, projecting the IfcOpenShell ifctester oracle (the `python:geometry/ifc-companion` `ids` oracle reached over Compute's existing companion rpc) into it, and `IdsAudit.Reconcile` folds the returned rows against the in-process self-audit on the ORDINAL-QUALIFIED (GlobalId, `Requirement`, `FacetKey`) join — oracle rows filtered by the `Spec` DOCUMENT ordinal, never the spec NAME (IDS v1.0 does not require names unique within a document, so a name filter cross-joins two same-named specs), and keyed by the requirement's position within the spec (a byte-identical facet duplicated under two cardinalities is two requirements sharing one `FacetKey`; the ordinal splits them) — into the typed `IdsParity` receipt; `FacetKey` stays the injective evidence column (the token folds every identity-bearing slot: kind, `Simplify`-collapsed entity classes + predefined tokens, name AND value `KeyOf` renderings, classification system + codes, relation + recursive container key), the ordinals both sides derive from the ONE shared document order; the Compute leg's projection derives the identical tokens and ordinals from the same document — never a positional verdict-list compare and never a Compute-side `IdsAudit` re-projection; the `Publish` bytes feed the same oracle when the spec originates in Rasm rather than a foreign CDE.
- [PREDICATE_GAPS]: the classification gap is CLOSED — the query grew `ByClassificationSystem(string)`, so a system-only (no-identification) `IfcClassificationFacet` lowers onto the system-membership arm (`IdsFacet.Classification` carrying `Option<string> System` beside its branches) and `Presence()` widens the Classification kind to that same arm; the digits gap is CLOSED — `ValueMatch.Digits(Option<int> Total, Option<int> Fraction)` decides xs:totalDigits/fractionDigits over the canonical numeric rendering, only the digits+length-MIXED single component still dropping (one component, one arm — see `Unliftable`); the prior `PartOf` gap is CLOSED — the query's `ByComposed(ComposeKind, NodeMatch)`/`ByVoided(VoidKind, NodeMatch)`/`NodeMatch.Matching` growth lifted `Aggregated`/`Nested`/`Voided` into precise policy-row lowerings and retired the materialize-then-join, exactly as the query page's `[ALGEBRA_REUSE]` legislates. The remaining drop is a cross-file addition to `Model/query#ELEMENT_SET`; the mixed exact+pattern NAME constraint (no single-`ValueMatch` spelling) and the digits-bearing `StructureConstraint` (`xs:totalDigits`/`fractionDigits`, no `ValueMatch` digits arm — dropped rather than false-PASSED as an empty `Length`) are the two page-local honest residues, both surfaced by the `IdsParity` oracle join when the ifctester side evaluates them.
