# [BIM_VALIDATION]

The three-tier model-QA owner — ONE model-health verdict surface over the frozen seam graph. The STRUCTURAL tier is the seam `Rasm.Element/Projection/audit#AUDIT_FOLD` `ModelAudit` composed whole: neutral graph integrity and per-discipline coverage ratios graded where the graph is owned, so a dangling reference or a `Compose` cycle reports as the graph fact it is and this page never re-derives one. The BASELINE tier is the spec-free `Semantics/properties#TEMPLATE_AUDIT` `TemplateFinding` stream — produced there against the buildingSMART templates themselves, composed HERE beneath any authored specification, so every model carries a health floor before any IDS exists. The AUTHORED tier is buildingSMART IDS whole: one `IdsSpecification` parsed from the `Xbim.InformationSpecifications` `Xids` document, its six facets folded into one closed `IdsFacet` `[Union]` that LOWERS each applicability/requirement facet onto the `Model/query#ELEMENT_SET` `ElementPredicate` algebra AT FULL ALGEBRA DEPTH — a patterned `Pset_.*` set or property name lowers WHOLE onto the three-`ValueMatch` `ByProperty`, a patterned attribute name onto `ByAttribute`, a patterned predefined token expands against the `ids-lib` per-class `PredefinedTypeValues` roster into typed `ByPredefinedType` arms, and ALL FIVE `PartOf` relations lower (`Contained` transitively via `BySpatialContainer`+`SpatialReach.Ancestry`, `Aggregated`/`Nested` via `ByComposed`, `Grouped` via `ByZone`, `Voided` via both `ByVoided` sides) with the container-entity facet lowering to `NodeMatch.Matching` directly — never a materialize-then-join, never a second selection surface, never a `ValueConstraint.IsSatisfiedBy` value engine beside the query. Every schema-dependent resolution — subtype expansion, the patterned-entity class roster, predefined tokens, standard-Pset datatypes, the minimal-supertype collapse — reads the `IfcSchemaVersions` axis the SPECIFICATION itself declares through `Specification.IfcVersion`, never a pinned version. The lifecycle is the FULL round-trip: `Parse` admits foreign IDS bytes once — every facet it cannot lower leaving as a typed `DroppedFacet` that makes the specification's `IdsOutcome` `Indeterminate` rather than a silently discarded requirement reporting a clean pass, and the cardinality legality table gating the pairing at THIS hostile-document half as well as at publish — `Audit` folds the frozen seam graph totally over the IFC-visible universe — the three cardinalities as partition ROWS, Optional the conditional present-must-satisfy split over the `Presence()`-widened facet, never a pass-everything no-op — `Publish` raises the same closed family back through `Xids.PrepareSpecification`/`ExportBuildingSmartIDS` (the appointing-party authoring half — concrete entity expansions collapsed to minimal supertypes via `TrySimplifyTopClasses`, per-requirement cardinality written back through `FacetGroup.RequirementOptions` and gated by the package's `GetAllowedCardinality` legality table), `AuditFile` runs the buildingSMART-official `ids-lib` `Audit` over the document itself, and `IdsAudit.Reconcile` joins the ifctester oracle. Range bounds are UNIT-SAFE end to end: the facet `DataType` — or, when absent, the standard-Pset declaration `PropertySetInfo.Get` resolves — coerces through `ValueConstraint.TryGetNetType`/`ParseValue` onto the `ids-lib`-resolved SI `Dimension`, so a datatype-less `Pset_WallCommon.ThermalTransmittance` range is dimension-checked, never silently dimensionless. Only the boundary admissions carry the `Fin` rail, each foreign-byte fault lifting BARE as `Model/faults#FAULT_BAND` `BimFault.ModelRejected` (band 2600 IS the `Expected` `Code`, no `.ToError()` hop). The page composes the seam `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph`, the `Model/query#ELEMENT_SET` algebra, the `Model/elements#IFC_CLASS` roster, the `Semantics/classification#CLASSIFICATION_AXIS` projector, the `Semantics/properties#PROPERTY_TEMPLATES` template authority, and the `ids-lib` `IdsLib.IfcSchema` offline schema authority as settled vocabulary; the in-process fold is the immediate self-audit and the IfcOpenShell ifctester companion the deterministic cross-tool oracle the `IdsVerdict` rows carry. `ModelHealth.Audit` joins the three tiers into the ONE verdict surface — `ModelFinding` the closed verdict family whose case IS the tier discriminant, each finding carrying the `RuleSeverity` band its tier declares — so `Rasm.AppUi` and the review pipeline read one entry, never a forked set of reports, and `Conforms` means no BLOCKING finding rather than no finding at all. The page is HOST-LOCAL.

## [01]-[INDEX]

- [02]-[IDS_FACETS]: `RuleSeverity` the package-wide enforcement band, `IdsOutcome` the three-valued specification verdict, `DropReason`/`DroppedFacet` the typed unliftable-facet accounting, `IdsFacet` the closed `[Union]` of seam-lowered facets (each `ToPredicate()` a graph-free `ElementPredicate`, the value a `ValueMatch`), `PartOfRelation` the relation policy rows, `IdsRequirement`/`IdsSpecification` the spec records, `IdsSpecification.Parse`/`Publish` the bidirectional `Xids` boundary, `IdsSpecification.Audit` the total model-audit fold, `IdsSpecification.AuditFile` over `ids-lib` `Audit`, the `IdsSchema` schema-parameterized offline authority, the `IdsAudit` receipt, and the `IdsAudit.Reconcile` -> `IdsParity` ifctester-oracle cross-tool diff over the Bim-owned `IdsVerdict` row on the ordinal-qualified (GlobalId, requirement, facet-key) axis.
- [03]-[MODEL_HEALTH]: `ModelHealth` the three-tier model-QA receipt and its `Audit` entry — the neutral seam `ModelAudit` structural grade and the baseline `TemplateFinding` stream composed beneath the authored per-spec `IdsAudit` fold under one threaded `TemplateScope` policy the receipt records — `ModelFinding` the closed three-case verdict family (the case the tier discriminant) carrying its `Severity` band, `Findings` the one flattened verdict stream, `Coordinate` the per-finding report group key, `Conforms` the one model-health verdict.

## [02]-[IDS_FACETS]

- Owner: `RuleSeverity` the package-wide `[SmartEnum<string>]` enforcement band whose row carries its own `Blocking` policy — declared HERE at the `Rasm.Bim` root and composed by `Review/coordination#COORDINATION`'s rule library from its child namespace, so one severity vocabulary spans model-check verdicts and IDS findings; `IdsOutcome` the three-valued specification verdict (`Conformant`/`NonConformant`/`Indeterminate`, the last the partially-graded state a dropped requirement facet forces); `DropReason` the closed vocabulary of lowering gaps and `DroppedFacet` its per-facet row carrying the `FacetGroup.FacetUse` role and the foreign `Short()` label; `IdsSpecification` the IDS specification record carrying the applicability and requirement facet sets, the cardinality, its OWN declared `IfcSchemaVersions` schema axis, its enforcement band, and the facets no lowering lifts; `IdsFacet` the closed `[Union]` (Entity, Attribute, Property, Classification, Material, PartOf) each carrying SEAM-LOWERED data — resolved `IfcClass`/`PredefinedType` sets, `ValueMatch` name restrictions (a patterned set/property/attribute name is a first-class `ValueMatch`, never a dropped facet), `ValueMatch` value restrictions, resolved `Classification` branches — admitted ONCE at parse so the interior never sees an `Xbim` `ValueConstraint`; `PartOfRelation` the `[SmartEnum<string>]` relation POLICY ROWS, each case carrying its query-arm lowering delegate AND its foreign `PartOfFacet.PartOfRelation` member as row data (the boundary map and the reverse raise both DERIVE from the one row set); each `IdsFacet` arm `ToPredicate()` lowering GRAPH-FREE to a `Model/query#ELEMENT_SET` `ElementPredicate`, `Presence()` deriving the value-widened conditional form the Optional cardinality partitions against, and `FacetKey(schema)` projecting the injective join token as a seam `ContentAddress` over the ONE kernel hasher; `IdsAudit` the deterministic per-specification receipt carrying the spec-level `IdsCardinality`, the enforcement band, the dropped-facet rows, and the `Outcome` those three decide; `IdsFileAudit` the IDS-document validity receipt; `IdsVerdict` the per-(GlobalId, requirement) cross-runtime oracle row the `Rasm.Compute/Runtime/codecs#TWO_HOP_TESSELLATION` `IdsAuditRequest` companion-rpc leg projects and COMPOSES from here (Bim owns the row, Compute references up-stratum); `IdsParity` the typed reconcile receipt joining self-audit against oracle on the ordinal-qualified (GlobalId, requirement, `FacetKey`) axis.
- Entry: `IdsSpecification.Audit(ElementGraph graph)` is TOTAL — it folds the applicability facets into one `ElementPredicate` scoped to the IFC-VISIBLE universe (`ExternalId`-bearing objects: the identity the verdict rows, the applicable-count rule, and the ifctester oracle share, so an unexported authored element is never counted into `ApplicableCount` then silently dropped from the `Failed` set that decides `Conforms`), queries through `ElementSet.Query(graph, predicate)`, refines by each requirement's `ToPredicate()` through `ElementSet.Where`, and partitions pass/fail through the `IdsCardinality.Partition` row over `(matched, applicable, presence)` — the presence thunk (`IdsFacet.Presence()`, the value-widened facet) forced only by the Optional row, whose conditional semantics fail exactly the present-but-violating elements; `IdsSpecification.Parse(ReadOnlyMemory<byte> idsBytes, Op key)` admits an IDS XML document through `Xids.LoadBuildingSmartIDS`, reading each spec's own `Specification.IfcVersion` onto the `IfcSchemaVersions` axis through `IfcSchemaVersionHelper` and lowering every facet and every `ValueConstraint` onto the closed union (the one ingress boundary), each unliftable facet leaving as a typed `DroppedFacet` and the SAME `GetAllowedCardinality` legality table gating the pairing HERE — the half that receives a hostile document — so an illegal facet/cardinality pairing can never grade under a rule its own schema forbids; `IdsSpecification.Publish(Seq<IdsSpecification> specifications, Op key)` is the ingress INVERSE — a pure `Fin` legality gate raises every requirement facet and pairs it with its cardinality row BEFORE any document is built (an illegal pairing short-circuits the whole publish onto `ids-publish:cardinality-illegal`, never a throw out of a half-assembled `Xids`), the per-requirement cardinality writing back through `FacetGroup.RequirementOptions` (the `IdsCardinality.AuthoredFacet` row column — a Prohibited requirement never republishes as the Expected default), specs assembling through `Xids.PrepareSpecification` under the spec's own raised schema set and serializing through `ExportBuildingSmartIDS`, so authored exchange requirements publish as standard `.ids` bytes for the CDE transmittal and the ifctester oracle; `IdsSpecification.AuditFile(ReadOnlyMemory<byte> idsBytes, Op key)` validates the document's own conformance through `ids-lib` `Audit.Run` onto an `IdsFileAudit` with `BufferingLogger`-captured `IdsDiagnostic` rows.
- Auto: `Parse` is the value-lowering boundary and every gap it cannot lower leaves as a NAMED `DropReason` on an `Either` Left rail, never a `Choose`-discarded `None` — `Matches` folds a facet's `ValueConstraint.AcceptedValues` onto `Seq<ValueMatch>` (an all-exact set collapses to one `OneOf`, a `PatternConstraint` to `Pattern`, a `RangeConstraint` to a dimension-checked `Range` whose inclusivity rides the `RangeBound` arm, a pure length-bearing `StructureConstraint` to `Length`, a pure digits-bearing one to `Digits`, an absent constraint to `Present`; only a StructureConstraint MIXING the two axes drops, because one component lowers to one `ValueMatch` and splitting ORs two partial matches into a false PASS — beside it a bounds-crossed or exclusive-coincident range drops as `UnsatisfiableRange`, since it lowers cleanly yet admits no value and a Prohibited requirement then passes every applicable element on a rule that graded nothing); `NameMatch` lowers a NAME-position constraint to ONE `ValueMatch` so patterned names survive; `Predefineds` expands a patterned predefined token against the resolved classes' `IdsSchema.PredefinedTokens` roster through the ONE seam matcher (`ValueMatch.Pattern` — its cached anchored NonBacktracking compile, never a second regex engine); `DataTypeOf` resolves the range-bound datatype from the facet or the `PropertySetInfo.Get` standard-Pset declaration under the spec's schema; `Numeric` coerces bound literals through `ValueConstraint.TryGetNetType`/`ParseValue` in the IFC datatype's value space; `ResolveClasses` expands an Entity facet's `IfcType` to its `IdsSchema.ConcreteClasses` subtypes when `IncludeSubtypes` and expands a PATTERNED entity name against `IdsSchema.ClassRoster`, an entity facet resolving to no rostered class dropping as `UnknownClass` rather than lowering to a match-nothing predicate a Prohibited requirement reads as a model-wide pass; `ClassificationBranches` resolves the system through the `Semantics/classification#CLASSIFICATION_AXIS` roster; `Audit` then folds each facet's graph-free `ToPredicate()` — the validation fold reuses the query algebra for BOTH selection and value with one total `Switch` — and stamps each verdict's `FacetKey` ONCE under the spec's schema, so `Reconcile` and the `ModelFinding` projection read a computed key.
- Receipt: `IdsAudit` carries the specification name, the `Spec` document ordinal (the cross-runtime join identity — spec names are not unique in IDS), the `Model` provenance digest (the seam `Projection/address#CONTENT_ADDRESS` `ContentAddress.OfGraph` snapshot address of the graph the fold ran over, so a stored verdict set names the model it graded and a re-audit after an edit re-keys), the spec-level `IdsCardinality`, the enforcement band, the applicable element count, the passed/failed `GlobalId` sets per facet with each facet's computed key, and the `DroppedFacet` rows; `IdsAudit.Outcome` is the three-valued verdict — `Indeterminate` whenever a facet dropped (the grading covered a subset of the specification), else the spec-level applicable-count rule (`SpecSatisfied`) AND every requirement verdict passing — and `Conforms` reads its row column; `IdsAudit.Reconcile(Seq<IdsVerdict> oracle)` folds the companion ifctester projection against the self-audit into an `IdsParity` receipt on the ordinal-qualified (GlobalId, requirement, `FacetKey`) axis — `IdsParity.Conformant` is the cross-tool parity verdict, never a message diff; `IdsFileAudit.Conforms`/`Errors` reads the `Status` and the captured diagnostics.
- Packages: Xbim.InformationSpecifications, ids-lib, Microsoft.Extensions.Logging.Abstractions, Rasm.Element (the seam `ElementGraph`, the `ElementPredicate` algebra, and the `Projection/address#CANONICAL_WRITER`/`#CONTENT_ADDRESS` codec the facet key and the snapshot digest ride), Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm
- Growth: a new IDS facet is one `IdsFacet` union arm lowering to its `ElementPredicate` arm plus its `Matches` lowering, its `Presence()` widening, its `Write` key contribution, and its `Raise` inverse; a new PartOf relation is one `PartOfRelation` row (delegate + foreign member — zero switch edits); a new value-match modality is one `ValueMatch` arm the seam already folds plus one `WriteMatch` arm ordinal; a new lowering gap is one `DropReason` row; a new enforcement band is one `RuleSeverity` row carrying its `Blocking` policy; a new cardinality is one `IdsCardinality` row carrying its `(matched, applicable, presence)` partition, spec rule, AND both authored inverses (`CardinalityEnum` + `RequirementCardinalityOptions.Cardinality`); a new IFC schema version is already the `IfcSchemaVersions` flags axis each spec declares; the cross-tool audit is one companion-rpc shape over the existing `Rasm.Compute/Runtime/codecs#TWO_HOP_TESSELLATION` pattern; never a second validation predicate surface, never a second value engine, never a hand-rolled IDS parser or writer, and never a transport minted here.
- Boundary: The composing rail emits `VerdictIssued` with specification, ordinal, model address, outcome, and severity.
- Boundary: `VerdictIssued` also carries finding count and failing GlobalIds.
- Boundary: `VerdictIssued` subject is `name#ordinal`, matching the IDS parity and coordination keys.
- Boundary: `Exchange/events` owns message-envelope seal and transport; validation owns no second emitter.
- Boundary: `ElementPredicate` is the only validation selection algebra.
- Boundary: `ByProperty` and `ByAttribute` preserve `ValueMatch` name restrictions.
- Boundary: `PartOf` lowers recursively to `NodeMatch.Matching`.
- Boundary: `Aggregated`, `Nested`, and `Voided` lower through `ByComposed` and `ByVoided`.
- Boundary: `ValueMatch` is the sole validation value engine.
- Boundary: Model audit reads the seam `ElementGraph` assembled by `SemanticProjector`.
- Boundary: `Xids` owns IDS parsing and publishing.
- Boundary: buildingSMART `ids-lib Audit.Run` owns IDS-file audit independently from model audit.
- Boundary: `IdsSchema` resolves entity, predefined, property, and measure facts from the real `IdsLib.IfcSchema` graph.
- Boundary: Non-standard Xbim `DocumentFacet` and `IfcRelationFacet` cases drop explicitly at parse.
- Boundary: `IdsVerdict` is the Bim-owned companion-oracle row, and Compute only composes its RPC.
- Events: an issued `IdsAudit` fires the `Model/observability#HOOK_RAIL` `rasm.bim.review.verdict` point with `BimFact.Verdict` — the specification name beside its document ORDINAL (IDS spec names are not unique, so the ordinal keeps two same-named specifications' verdicts apart), the `Model` provenance address the fold ran over, the tier, the `IdsOutcome` key, the `RuleSeverity` key, the finding tally, and the failing `GlobalId` set — at the `Audit` fold's own edge; the point is REPLAY modality so a late panel drains the recent window, and the CloudEvents announcement is `Exchange/events#EVENT_PROJECTION`'s observe subscription over it, subject the model address. Minting a verdict message envelope at this rail is the deleted form.
- Boundary: `IdsAudit` and `IdsFileAudit` remain typed validation evidence.
- Boundary: `IdsAudit` is C# host-local, and neither it nor `IdsVerdict` mints a TypeScript family.
- Admission: Every unliftable facet becomes a `DroppedFacet` and makes its specification `Indeterminate`.
- Admission: Unknown classes, unsatisfiable ranges, and empty prohibited grading remain `Indeterminate`.
- Admission: Every schema-dependent read uses its specification's `IfcSchemaVersions` axis.
- Identity: Facet joins use seam `ContentAddress` over `CanonicalWriter`.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
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
// The package-wide QA enforcement axis, declared ONCE here at the Rasm.Bim root and composed by the
// Review/coordination#COORDINATION rule library from its child namespace: the row carries its own Blocking policy, so
// a consumer verdict reads `severity.Blocking` instead of comparing against an Error literal, and a new enforcement
// band is one row. Advisory findings report without failing a model, which is what lets ModelHealth.Conforms mean
// "no blocking finding" rather than "no finding at all".
[SmartEnum<string>]
public sealed partial class RuleSeverity {
    public static readonly RuleSeverity Info    = new("info",    blocking: false);
    public static readonly RuleSeverity Warning = new("warning", blocking: false);
    public static readonly RuleSeverity Error   = new("error",   blocking: true);

    public bool Blocking { get; }
}

// The three-valued specification outcome: a graded specification either conforms or does not, and a specification
// carrying a requirement facet the lowering could not lift was graded PARTIALLY — Indeterminate is that third state,
// so an unliftable requirement can never read as a pass. Conforms is the row column every consumer already reads.
[SmartEnum<string>]
public sealed partial class IdsOutcome {
    public static readonly IdsOutcome Conformant    = new("conformant",     conforms: true);
    public static readonly IdsOutcome NonConformant = new("non-conformant", conforms: false);
    public static readonly IdsOutcome Indeterminate = new("indeterminate",  conforms: false);

    public bool Conforms { get; }
}

// The named reasons a foreign facet cannot lower onto the seam algebra — the typed accounting that replaces the
// silent Choose-discard. Each row names an EXACT lowering gap, so a parse receipt states which facet the audit could
// not grade and why, and a new gap is one row plus one lowering arm.
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
}

// One unliftable facet, carried from parse to receipt: the group role it sat in (an applicability drop WIDENS the
// graded set, a requirement drop leaves a requirement ungraded — both make the verdict partial), the facet's own
// Xbim Short() label as the operator-readable evidence, and the reason row.
public readonly record struct DroppedFacet(FacetGroup.FacetUse Role, string Facet, DropReason Reason);

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

    // The cross-runtime join token an IdsAudit verdict and the companion IdsVerdict share on the ordinal-qualified
    // (GlobalId, Requirement, FacetKey) axis — a seam ContentAddress over the ONE kernel seed-zero hasher every
    // federation, cache, and diff edge keys on, so the join axis cannot fork a second content space. INJECTIVITY LAW:
    // the fold writes EVERY identity-bearing slot — an arm ordinal, names, value restrictions, relation, container
    // (case-owned recursion) — through the length-prefixed CanonicalWriter, so no delimiter forges a collision the
    // prior interpolated "entity:{…}:{…}" scheme admitted (a class literally named `a|b` collided with the two-class
    // set); two DISTINCT facets in one specification never address alike, and duplicate identical facets merge
    // harmlessly because their verdicts coincide. Entity classes fold through IdsSchema.Simplify under the SPEC's own
    // schema so the token matches the AUTHORED document form both runtimes read — the schema is an argument because
    // the minimal-supertype collapse is schema-dependent. The Rasm.Compute/Runtime/codecs#TWO_HOP_TESSELLATION
    // IdsAuditRequest leg projects IdsVerdict.Facet from THIS fold — owned HERE, mirrored there.
    public ContentAddress FacetKey(IfcSchemaVersions schema) {
        CanonicalWriter writer = new(0.0);
        Write(writer, schema);
        return ContentAddress.Of(writer.ToBytes().Span);
    }

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
                                .Ordinal(f.Branches.Count);
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

    // A ValueMatch writes deterministically: an arm ordinal then its own slots — exacts their literals, a pattern its
    // expression, a range its bounds (each an inclusivity ordinal plus the CanonicalWriter.Measure fold, so a bound's
    // dimension and quantity type ride the key exactly as the Raise inverse emits them), a length or digits bound its
    // presence-flagged integers, the match-any nothing. Every collection is count-prefixed and every string
    // length-prefixed, so a bound that is absent and a bound that is zero address differently.
    static CanonicalWriter WriteMatch(CanonicalWriter writer, ValueMatch match) => match switch {
        ValueMatch.OneOf o   => o.Allowed.Fold(writer.Ordinal(0).Ordinal(o.Allowed.Count), static (w, a) => w.String(a)),
        ValueMatch.Pattern p => writer.Ordinal(1).String(p.Expression),
        ValueMatch.Range r   => WriteBound(WriteBound(writer.Ordinal(2), r.Lower), r.Upper),
        ValueMatch.Length l  => WriteCount(WriteCount(writer.Ordinal(3), l.Min), l.Max),
        ValueMatch.Digits d  => WriteCount(WriteCount(writer.Ordinal(4), d.Total), d.Fraction),
        _                    => writer.Ordinal(5),
    };

    // Inclusivity is the RangeBound ARM (Inclusive/Exclusive each carrying the MeasureValue), never a sibling
    // boolean flag on Range — the arm ordinal carries it into the key.
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
    static ElementPredicate AnyOf(Seq<ElementPredicate> arms) =>
        arms.IsEmpty ? new ElementPredicate.Any(Seq<ElementPredicate>())
        : arms.Tail.IsEmpty ? arms.Head.IfNone(new ElementPredicate.Any(Seq<ElementPredicate>()))
        : new ElementPredicate.Any(arms);
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record IdsRequirement(IdsFacet Facet, IdsCardinality Cardinality);

// Ordinal is the ZERO-BASED document position Parse stamps — the spec identity the cross-runtime join keys on,
// because IDS v1.0 does NOT require spec names unique within a document (two same-named specs would cross-join).
// Schema is the spec's OWN declared IFC version set folded to the ids-lib flags axis through IfcSchemaVersionHelper
// (the one bridge between the two vocabularies), so subtype expansion, predefined-token rosters, standard-Pset
// datatypes, and the minimal-supertype collapse all resolve against the schema the DOCUMENT declares — a pinned
// IFC4X3 literal grades an IFC2X3 exchange against classes its schema never had. Dropped carries the facets no
// lowering could lift, so a partially-graded specification says so instead of passing. Severity is the appointing
// party's enforcement band for this specification: a parsed IDS lands Error (a contracted exchange requirement),
// while a Rasm-authored advisory specification reports without blocking.
// Description, Instructions, and Identifier are the specification's own AUTHORING columns. Description and
// Instructions are `Specification.Description`/`Instructions` verbatim (both nullable strings, empty when the
// document declares none); Identifier reads `Specification.Guid` — the xbim model carries no `@identifier` slot and
// Guid IS its identity column, minted lazily on first read, so an appointing party's stable per-requirement handle
// round-trips through the one member that actually persists it. Dropping the three re-authored every published
// document with an empty description and a FRESH guid, which reads to a receiving CDE as a brand-new requirement
// each transmittal.
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

    // TOTAL model audit over the frozen seam graph: ElementSet.Query/Where/Except carry no rail and every facet's
    // seam-typed payload is admitted at Parse (no audit-time mint), so the audit is a pure fold. The applicable set
    // is the conjunction of the applicability facets (or every object when applicability is empty), SCOPED to the
    // IFC-visible universe; each requirement partitions the applicable set through its IdsCardinality.Partition row
    // (the presence thunk forced only by Optional), the verdict GlobalIds projected off the selected ExternalId set,
    // and the spec-level Cardinality rides onto the receipt for the applicable-count rule. The join key is folded
    // ONCE per requirement here — the schema the collapse needs is in hand exactly at this point — and stored on the
    // verdict, so Reconcile and the ModelFinding projection read a computed key rather than re-deriving one.
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
            return new IdsAudit.FacetVerdict(req.Facet, req.Facet.FacetKey(Schema), req.Cardinality, pass.GlobalIds, fail.GlobalIds);
        });
        return new IdsAudit(Name, Ordinal, ContentAddress.OfGraph(graph), Cardinality, Severity, applicable.Count, verdicts, Dropped);
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
            return toSeq(xids.AllSpecifications().Select(static (spec, i) => Project(spec) with { Ordinal = i }));
        }).Run().MapFail(error => new BimFault.ModelRejected(key, $"ids-parse:{error.Message}"));

    // The authoring egress — the ingress INVERSE: each facet raises through Raise onto its Xbim facet, specs
    // assemble through Xids.PrepareSpecification (the groups created and wired by the factory) and serialize
    // through ExportBuildingSmartIDS, so the appointing-party half of the IDS lifecycle (author exchange
    // requirements from Rasm vocabulary, publish .ids to the CDE, feed the ifctester oracle) rides the SAME
    // closed family. Each spec publishes under its OWN declared schema set (the ids-lib flags axis raised back
    // through IfcSchemaVersionHelper.FromIds), never a pinned literal. The cardinality LEGALITY table is a PURE ROP
    // gate that runs BEFORE any document is built — the whole publish short-circuits on the first illegal pairing
    // (an Entity requirement is Expected-only, PartOf never Optional — the IDS v1.0 schema law) rather than throwing
    // out of a half-assembled Xids. Each requirement writes BOTH halves back: its facet plus its per-facet
    // cardinality through the FacetGroup.RequirementOptions row and the IdsCardinality.AuthoredFacet column (a
    // Prohibited requirement republishing at the Expected default would INVERT its meaning); the spec-level rule
    // writes back through Authored. FacetGroup.IsValid() demands a RequirementOptions collection whose count MATCHES
    // the facet count, which the one-projection-per-requirement pairing satisfies structurally.
    public static Fin<byte[]> Publish(Seq<IdsSpecification> specifications, Op key) =>
        from raised in specifications.Traverse(spec => Raisable(spec, key)).As()
        from bytes in Try.lift(() => Serialize(raised)).Run()
            .MapFail(error => new BimFault.ModelRejected(key, $"ids-publish:{error.Message}"))
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
            : new BimFault.ModelRejected(key, $"cardinality-illegal:{row.RelatedFacet.GetType().Name}:{cardinality.Key}");

    // The Xbim document builder is a mutable ObservableCollection graph, so this is the page's ONE statement-shaped
    // kernel — the platform-forced boundary named: the domain side arrives fully projected and every decision (which
    // schema, which facets, which cardinality rows) is already settled, leaving the body a pure transcription.
    static byte[] Serialize(Seq<(IdsSpecification Spec, Seq<IFacet> Applicability, Seq<(IFacet Facet, RequirementCardinalityOptions Options)> Requirements)> raised) {
        Xids xids = new();
        foreach (var (spec, applicability, requirements) in raised) {
            Specification prepared = xids.PrepareSpecification(spec.Schema.FromIds());
            prepared.Name = spec.Name;
            prepared.Cardinality = new SimpleCardinality(spec.Cardinality.Authored);
            // The authoring columns round-trip: an empty domain value writes null so the exporter omits the
            // element, and a carried Identifier re-stamps the SAME Guid rather than letting the lazy getter mint a
            // fresh one — a republished specification stays the specification a CDE already accepted.
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

    // A document declaring no IFC version grades against IFC4X3, the IDS v1.0 reference schema — the ONE default,
    // stated once here rather than pinned at six call sites. A parsed specification is a contracted exchange
    // requirement, so it lands at the blocking severity; the authoring half re-stamps an advisory one.
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

    // Each requirement facet carries its OWN cardinality (Expected/Prohibited/Optional), read off the requirement
    // FacetGroup per facet through GetRequirementCardinalityOption — NOT the spec-level ICardinality applied to all
    // (the deleted conflation): a spec can require one facet, prohibit another, and leave a third optional in one
    // pass. The SAME GetAllowedCardinality legality table the Publish egress gates on runs HERE too, because this is
    // the half that receives a foreign document: a hostile .ids pairing a PartOf facet with Optional grades under a
    // conditional rule its own schema forbids, so the pairing drops typed and the specification reads Indeterminate.
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
    // than a silent None the Choose discarded — an ungraded requirement facet is evidence, and the specification
    // carrying it reads Indeterminate instead of passing on the facets that did lower.
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
    // empty set lowers to a match-nothing predicate, and a match-nothing REQUIREMENT under the Prohibited row passes
    // every applicable element — a model-wide false PASS off one misspelled or out-of-roster entity name. The drop
    // makes the specification Indeterminate instead.
    static Either<DropReason, IdsFacet.Entity> EntityOf(IfcTypeFacet f, IfcSchemaVersions schema) =>
        ResolveClasses(f.IfcType, f.IncludeSubtypes, schema) switch {
            { IsEmpty: true } => Left(DropReason.UnknownClass),
            var classes       => Right(new IdsFacet.Entity(classes, Predefineds(f.PredefinedType, classes, schema))),
        };

    // A code-bearing facet lowers its branch set; a SYSTEM-ONLY facet (no identification) lowers the resolved
    // system alone onto the ByClassificationSystem membership arm — the parse-time drop this closes; a facet with
    // neither system nor code has nothing to select on and drops typed.
    static Either<DropReason, IdsFacet> ClassificationFacet(IfcClassificationFacet f) {
        Option<string> system = SingleValue(f.ClassificationSystem).Filter(static s => !string.IsNullOrWhiteSpace(s)).Map(ResolveSystem);
        Seq<SeamClassification> branches = ClassificationBranches(f);
        return branches.IsEmpty && system.IsNone
            ? Left(DropReason.EmptyClassification)
            : Right((IdsFacet)new IdsFacet.Classification(branches, system));
    }

    // Every relation the Xbim parse resolves lowers through its policy row; the Undefined parse-fail sentinel — and
    // any future foreign member with no row — drops typed, so an unparseable EntityRelation never mis-lowers onto a
    // wrong-semantics arm. A container entity facet that cannot resolve carries ITS reason up, never a silent
    // container-less PartOf that widens to match-any.
    static Either<DropReason, IdsFacet> PartOfFacetOf(PartOfFacet f, IfcSchemaVersions schema) =>
        from relation in MapRelation(f.GetRelation()).ToEither(DropReason.ExtendedFacet)
        from container in Optional(f.EntityType).Match(
            Some: t => EntityOf(t, schema).Map(static e => Some((IdsFacet)e)),
            None: static () => Right<DropReason, Option<IdsFacet>>(None))
        select (IdsFacet)new IdsFacet.PartOf(container, relation);

    // The foreign-relation map DERIVES from the PartOfRelation rows' own Foreign column — one primary
    // correspondence, no parallel switch to keep in sync.
    static Option<PartOfRelation> MapRelation(PartOfFacet.PartOfRelation relation) =>
        toSeq(PartOfRelation.Items).Find(row => row.Foreign == relation);

    // Resolve an Xbim IfcType ValueConstraint to the seam IfcClass set under the SPEC's schema: an exact name
    // expands to its ids-lib concrete subtypes when IncludeSubtypes (the buildingSMART default), and a PATTERN name
    // expands against the schema's own class roster through the ONE seam matcher — the same narrowing law the
    // predefined-token expansion runs, so a patterned entity facet lowers to typed classes rather than resolving
    // empty. A name the IfcClass roster does not carry drops here (the projector stamps only rostered keys); a facet
    // whose every name drops surfaces as DropReason.UnknownClass at the caller.
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
    // PredefinedTypeValues roster through the ONE seam matcher (ValueMatch.Pattern — its cached anchored
    // NonBacktracking compile), so a patterned predefined facet lowers to typed ByPredefinedType tokens instead
    // of silently widening to a class-only match — a false PASS on a requirement facet, the worst failure mode.
    static Seq<PredefinedType> Predefineds(ValueConstraint? predefined, Seq<IfcClass> classes, IfcSchemaVersions schema) {
        Seq<IValueConstraintComponent> components = Components(predefined);
        Seq<string> roster = classes.Bind(c => IdsSchema.PredefinedTokens(c.Key, schema)).Distinct();
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
    // IncludeSubClasses=true sub-branch inclusion; a facet with no system or no code yields no branch.
    static Seq<SeamClassification> ClassificationBranches(IfcClassificationFacet f) =>
        SingleValue(f.ClassificationSystem).Filter(static s => !string.IsNullOrWhiteSpace(s)).Map(ResolveSystem).Match(
            Some: system => ExactValues(f.Identification).Filter(static c => !string.IsNullOrWhiteSpace(c)).Map(code => SeamClassification.Create(system, code, "", None, None, None)),
            None: static () => Seq<SeamClassification>());

    static string ResolveSystem(string name) =>
        toSeq(ClassificationSystem.Items)
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
    // An Unliftable component drops the WHOLE facet under its own named reason: the ANY-component-matches fold
    // cannot soundly skip one, and the reason is the evidence the receipt carries.
    static Either<DropReason, Seq<ValueMatch>> Matches(ValueConstraint? constraint, Option<string> dataType) {
        Seq<IValueConstraintComponent> components = Components(constraint);
        Seq<string> exacts = components.Choose(static c => c is ExactConstraint e ? Some(e.Value) : Option<string>.None);
        return components.Choose(c => Unliftable(c, dataType)).Head.Match(
            Some: Left<DropReason, Seq<ValueMatch>>,
            None: () => components.IsEmpty ? Right(Seq(ValueMatch.Any))
                : exacts.Count == components.Count ? Right(Seq<ValueMatch>(new ValueMatch.OneOf(exacts)))
                : Right(components.Map(c => Lower(c, dataType))));
    }

    // A NAME-position constraint lowers to ONE ValueMatch (the query ByProperty/ByAttribute name slots): absent ->
    // Any, all-exact -> OneOf, a single component -> its arm; a mixed exact+pattern name constraint has no
    // single-arm spelling and drops the facet — the narrow honest residue replacing the prior any-pattern drop.
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
    // rather than existing as a value the fold could only mis-read as an ordinary non-match. An UNSATISFIABLE range
    // drops on the third face of that same law: it lowers cleanly, then admits no value, and a match-nothing
    // predicate reaching the fold is the model-wide-pass defect this page already deletes at UnknownClass — under
    // Prohibited the partition hands EVERY applicable element to Passed and the count rule holds, so a bounds-crossed
    // requirement reports clean conformance on a rule that graded nothing. Coercion runs in the same datatype value
    // space the bounds themselves lower through, so the comparison is SI magnitude against SI magnitude.
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
    // The facet inverse the Publish egress folds: seam-typed data raises to the Xbim facet shape, the value
    // matches back to constraint components. An expanded concrete entity set collapses to its minimal supertypes
    // under the SPEC's own schema (TrySimplifyTopClasses) + IncludeSubtypes, so a published facet reads as authored,
    // never a 40-leaf enumeration; the PartOf relation writes back through its row's Foreign member and SetRelation.
    // The schema threads as Switch state, so every arm stays a closure-free static lambda.
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
                            IncludeSubClasses = true,
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
    // invariant round-trip, "R"); a Present-only value raises to the null constraint (existence). Inclusivity is the
    // RangeBound ARM, never a flag on Range — the Switch reads the magnitude and the min/maxInclusive bit off ONE
    // case, so an Exclusive bound can never publish as inclusive. A Digits match raises its own totalDigits/
    // fractionDigits StructureConstraint slots, the inverse of the lowering that admitted them.
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

    // An absent bound raises to the null magnitude the RangeConstraint reads as unbounded; a present one carries its
    // SI magnitude and the inclusivity its own arm declares.
    static (string? Magnitude, bool Inclusive) RaiseBound(Option<RangeBound> bound) =>
        bound.Match(
            Some: static b => b.Switch(
                inclusive: static i => ((string?)i.Value.Si.ToString("R", CultureInfo.InvariantCulture), true),
                exclusive: static e => ((string?)e.Value.Si.ToString("R", CultureInfo.InvariantCulture), false)),
            None: static () => ((string?)null, false));

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

// Model is the graph the verdicts were produced over — the seam ContentAddress.OfGraph snapshot digest, the one
// order-independent identity Rasm.Persistence and Rasm.Compute already key on. Without it a stored verdict set names
// no model, so a re-audit after an edit cannot be told from a stale receipt and the ifctester Reconcile can join two
// runs over DIFFERENT graphs. The digest excludes the StepHeader/Instant provenance, so a re-export under a new
// timestamp keeps one identity and an actual semantic edit re-keys.
// Dropped carries the facets the lowering could not lift, so a specification graded over a SUBSET of its own
// requirements says so: the fold that silently Chose them away reported a clean PASS for a requirement it never
// evaluated, which is the worst failure mode this page names.
public sealed record IdsAudit(
    string Specification, int Spec, ContentAddress Model, IdsCardinality SpecCardinality, RuleSeverity Severity,
    int ApplicableCount, Seq<IdsAudit.FacetVerdict> Verdicts, Seq<DroppedFacet> Dropped) {
    public sealed record FacetVerdict(IdsFacet Facet, ContentAddress Key, IdsCardinality Cardinality, Seq<string> Passed, Seq<string> Failed);

    // The three-valued outcome: a dropped facet makes the grading PARTIAL, so the specification is Indeterminate
    // whatever the graded verdicts say. Otherwise conformance is BOTH the spec-level applicable-count rule (a
    // Required spec needs >=1 applicable element, a Prohibited spec none — the Xbim ICardinality.IsSatisfiedBy
    // truth) AND every requirement verdict passing.
    public IdsOutcome Outcome =>
        !Dropped.IsEmpty ? IdsOutcome.Indeterminate
        : SpecCardinality.SpecSatisfied(ApplicableCount) && Verdicts.ForAll(static v => v.Failed.IsEmpty) ? IdsOutcome.Conformant
        : IdsOutcome.NonConformant;

    public bool Conforms => Outcome.Conforms;

    // The cross-tool outer join on the ORDINAL-QUALIFIED (GlobalId, Requirement, FacetKey) axis, oracle rows
    // filtered by the Spec DOCUMENT ordinal — never the spec NAME (IDS v1.0 does not require names unique, so two
    // same-named specs would cross-join) — and keyed by the requirement's position within the spec (a byte-identical
    // facet duplicated under two cardinalities is two requirements sharing one FacetKey; the ordinal splits them).
    // Both sides derive the ordinals from the same document order, so the join is injective by construction, and the
    // facet key is the one the Audit fold already computed — never re-derived here without the spec's schema.
    public IdsParity Reconcile(Seq<IdsVerdict> oracle) {
        Seq<((string GlobalId, int Requirement, ContentAddress Facet), bool Passed)> mine = Verdicts
            .Map(static (v, i) => (Verdict: v, Requirement: i))
            .Bind(r => r.Verdict.Passed.Map(g => ((g, r.Requirement, r.Verdict.Key), true))
                     + r.Verdict.Failed.Map(g => ((g, r.Requirement, r.Verdict.Key), false)));
        Seq<((string GlobalId, int Requirement, ContentAddress Facet), bool Passed)> theirs = oracle
            .Filter(o => o.Spec == Spec)
            .Map(o => ((o.GlobalId, o.Requirement, o.Facet), o.Passed));
        HashMap<(string, int, ContentAddress), bool> mineMap = mine.Map(static r => (r.Item1, r.Passed)).ToHashMap();
        HashMap<(string, int, ContentAddress), bool> theirMap = theirs.Map(static r => (r.Item1, r.Passed)).ToHashMap();
        Seq<(string, int, ContentAddress)> keys = (mine.Map(static r => r.Item1) + theirs.Map(static r => r.Item1)).Distinct();
        return new IdsParity(Specification,
            keys.Map(key => new IdsParity.Row(key.Item1, key.Item2, key.Item3, mineMap.Find(key), theirMap.Find(key))));
    }
}

// The per-(GlobalId, requirement) cross-runtime audit row — OWNED HERE (Bim is the IDS authority and cannot reference
// the up-stratum Rasm.Compute) and COMPOSED by the Rasm.Compute/Runtime/codecs#TWO_HOP_TESSELLATION
// IdsAuditRequest leg, which projects the IfcOpenShell ifctester oracle into this exact shape. Spec is the
// specification's ZERO-BASED document ordinal and Requirement the facet's ordinal within it — both derived from the
// one document order the two tools share — so the join survives duplicate spec names and cardinality-duplicated
// facets; Facet is the seam ContentAddress the FacetKey fold mints, so the wire carries the content key itself
// rather than a delimiter-joined rendering the far side must reproduce character for character.
public readonly record struct IdsVerdict(string GlobalId, string Specification, int Spec, int Requirement, ContentAddress Facet, bool Passed, string Reason);

public sealed record IdsParity(string Specification, Seq<IdsParity.Row> Rows) {
    public readonly record struct Row(string GlobalId, int Requirement, ContentAddress Facet, Option<bool> SelfAudit, Option<bool> Oracle) {
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
// schema graph the SPECIFICATION declares, never a hard-coded list and never a pinned version — subtype expansion,
// the class roster a patterned entity name expands against, per-class predefined-token rosters, the standard-Pset
// property declarations, measure dimensions, and the minimal-supertype collapse the Publish egress compacts an
// expanded entity set with. The version is an ARGUMENT on every schema-dependent read, because grading an IFC2X3
// exchange against IFC4X3 classes reports failures for entities the model's own schema never defined.
// IfcSchemaVersions is a FLAGS axis, so a multi-version specification resolves through GetSchemas and unions.
public static class IdsSchema {
    // Both rosters MEMOIZE on the schema-version key (the Semantics/properties#PROPERTY_TEMPLATES precedent): a
    // patterned entity or predefined facet expands against the WHOLE class graph, and Parse walks that expansion
    // once per patterned facet per specification — a document declaring forty specifications over one schema
    // otherwise re-enumerates every SchemaInfo class forty times for an answer the version alone determines. The
    // cache is process-static and keyed on the flags value, so a multi-version specification and a single-version
    // one occupy distinct entries and neither can read the other's roster.
    static readonly ConcurrentDictionary<IfcSchemaVersions, Seq<string>> Classes = new();
    static readonly ConcurrentDictionary<(string Class, IfcSchemaVersions Schema), Seq<string>> Predefined = new();

    public static Seq<string> ConcreteClasses(string topClass, IfcSchemaVersions schema) =>
        toSeq(SchemaInfo.GetConcreteClassesFrom(topClass, schema));

    // Every class name the declared schema graphs carry — the roster a PATTERN-restricted entity name expands
    // against, the entity-side counterpart of the predefined-token roster. SchemaInfo IS IEnumerable<ClassInfo>,
    // so the graph enumerates its own classes with no accessor hop.
    public static Seq<string> ClassRoster(IfcSchemaVersions schema) =>
        Classes.GetOrAdd(schema, static key =>
            toSeq(SchemaInfo.GetSchemas(key)).Bind(static graph => toSeq(graph).Map(static c => c.Name)).Distinct());

    // The per-class predefined-token roster (ClassInfo.PredefinedTypeValues) a patterned predefined facet
    // expands against — the schema's own token space, so the expansion can never admit an out-of-schema token.
    public static Seq<string> PredefinedTokens(string className, IfcSchemaVersions schema) =>
        Predefined.GetOrAdd((className, schema), static key =>
            toSeq(SchemaInfo.GetSchemas(key.Schema))
                .Choose(graph => Optional(graph[key.Class]))
                .Bind(static c => Optional(c.PredefinedTypeValues).Map(static tokens => toSeq(tokens)).IfNone(Seq<string>()))
                .Distinct());

    // The PartOf relation's own schema authority: GetAttributeRelations answers which classes reach an attribute
    // and THROUGH WHICH connection (ViaElement directly, ViaRelationType through the type edge), and
    // GetAttributeClasses the flat class roster with an onlyTopClasses collapse. A PartOf container facet resolves
    // its admissible container classes here rather than against a transcribed list, so an IFC2X3 exchange never
    // grades against an IFC4-only container.
    public static Seq<SchemaInfo.ClassRelationInfo> AttributeRelations(string attributeName, IfcSchemaVersions schema) =>
        toSeq(SchemaInfo.GetSchemas(schema)).Bind(graph => toSeq(graph.GetAttributeRelations(attributeName))).Distinct();

    public static Seq<string> AttributeClasses(string attributeName, IfcSchemaVersions schema, bool topOnly = false) =>
        toSeq(SchemaInfo.GetSchemas(schema)).Bind(graph => toSeq(graph.GetAttributeClasses(attributeName, topOnly))).Distinct();

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
    // dimension (a length bound never satisfies a pressure candidate) — the same SI metadata the property store reads.
    public static Option<Dimension> DimensionOf(string ifcDataType) =>
        SchemaInfo.TryGetMeasureInformation(ifcDataType, out IfcMeasureInformation? info) && info is { Exponents: { } e }
            ? Some(Dimension.Create(e.Length, e.Mass, e.Time, e.ElectricCurrent, e.Temperature, e.AmountOfSubstance, e.LuminousIntensity))
            : Option<Dimension>.None;
}
```

## [03]-[MODEL_HEALTH]

- Owner: `ModelHealth` the three-tier model-QA receipt — the ONE model-health entry `Rasm.AppUi` and the review pipeline read; `ModelFinding` the closed `[Union]` verdict family EVERY tier converges on — `Structural` carrying the seam `Rasm.Element/Projection/audit#AUDIT_FOLD` `AuditFinding` row WHOLE (neutral graph integrity and coverage, graded at the graph's own owner), `Baseline` carrying the `Semantics/properties#TEMPLATE_AUDIT` `TemplateFinding` row WHOLE (produced there against the buildingSMART templates, composed here — the spec-free ground truth), `Authored` the per-element failed IDS requirement with its full typed evidence (`IdsFacet`, `IdsCardinality`, the ordinal join identity, the computed facet key) — the case IS the tier discriminant, so tier dispatch is the generated `Switch` and a parallel tier vocabulary beside the family is the deleted form; `Severity` the derived `RuleSeverity` band (a structural row reads the seam `ConstraintSeverity.Blocks` column, the spec-free baseline advises, an authored specification carries its own declared band); `Coordinate` the per-finding report group key.
- Entry: `ModelHealth.Audit(ElementGraph graph, TemplateScope scope, Seq<IdsSpecification> specifications, Func<IfcClass, Option<BsddClass>> dictionary, Op key)` runs all three tiers over the one frozen seam graph — the STRUCTURAL tier composes the seam `Rasm.Element/Projection/audit#AUDIT_FOLD` `ModelAudit.Of(graph, key)` under its default structural thresholds, so every model carries the neutral integrity-and-coverage grade beneath any IFC claim; the baseline tier ALWAYS runs (an empty specification set still yields the zero-configuration verdict, so every model carries a health floor before any IDS is authored), each authored specification folds through its own total `IdsSpecification.Audit`; `scope` is the `Semantics/properties#PROPERTY_TEMPLATES` `TemplateScope` policy value threaded straight into `TemplateAudit.Run`, so a `Handover` audit grades COBie completeness on the SAME baseline fold a `Standard` audit runs; the `Fin` rails are the seam audit's uniform entry rail and the baseline's seam `Bake`.
- Auto: `Findings` flattens every tier onto the one `ModelFinding` stream — every seam `AuditFinding` a `Structural` case, every baseline row a `Baseline` case, every `FacetVerdict.Failed` element an `Authored` case carrying its requirement ordinal, its computed facet key, and its specification's enforcement band — a pure fold over the stored tier receipts; `Conforms` is the one verdict: NO blocking finding AND every `IdsAudit.Conforms` (each already folding its spec-level applicable-count rule, its requirement verdicts, and its dropped-facet `Indeterminate` state), so a spec-free template-floor gap advises where the prior empty-baseline rule failed a model that satisfied every authored specification.
- Receipt: `ModelHealth` stores the tier receipts TYPED — the seam `ModelAudit` keeps its graded snapshot `ContentAddress`, its `CoverageCensus`, and the `AuditThresholds` it graded under (so a stored health receipt re-reads against the floors it was actually gated on and a delivery gate reads `Structural.Clears` without re-folding the graph), the `TemplateFinding` rows keep the failing-axis evidence a fix pass acts on, the `IdsAudit` rows keep the spec-level cardinality rule and the `Model` snapshot digest and feed `Reconcile` unchanged — beside the `Scope` policy column naming the definition set the baseline graded under, because an empty baseline means one thing under `Standard` and another under `Handover` and a scope-less receipt reads standard-set silence as handover completeness; `Findings` is the derived projection, never a third stored copy; the two cases keep their native element identities (the seam `NodeId` for the baseline, whose audit covers the IFC-classified occurrence nodes including authored elements not yet exported; the IFC `GlobalId` for the authored tier, whose exchange is defined over the IFC-visible universe): the identity is the tier's own audit universe, never coerced onto one axis.
- Packages: Rasm.Element, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm
- Growth: a new baseline verdict axis is the properties owner's `TemplateVerdict` row and rides the `Baseline` case untouched; a new enforcement band is one `RuleSeverity` row `[02]` owns; a new definition-set scope is one `TemplateScope` row the threaded policy value carries with zero edits here; a new facet, cardinality, or relation rides the authored tier's own growth rows; a new neutral integrity class or coverage axis is the seam audit's own growth row and rides the `Structural` case untouched; a fourth verdict source is one `ModelFinding` case and one `Findings` arm with every consumer `Switch` broken loudly at compile time; never a second model-health entry, and never a per-tier report type.
- Boundary: the SCOPE SPLIT with the seam audit is absolute — `Rasm.Element/Projection/audit#AUDIT_FOLD` `ModelAudit` owns neutral structural integrity and coverage ratios (a dangling bag reference, a `Compose` cycle, an address drift, a per-`Discipline` assessed share) because those are graph facts any consumer can name, and this page owns IFC semantics (a missing `Pset_WallCommon` row, an IDS requirement) because those are claims only the schema authority can make; the two COMPOSE on one model and a Bim-local re-derivation of an integrity sweep or a coverage ratio is the deleted form, as is a seam-audit finding re-worded here (the frozen `AuditCategory` and its detail token ride WHOLE, so a report and a waiver pin the string the audit already keys on); the baseline stream is PRODUCED by `Semantics/properties#TEMPLATE_AUDIT` — `TemplateAudit.Run` stays authored there and this owner COMPOSES the rows, so re-deriving the template resolution, the `Bake` merge, or any per-axis check here is the deleted form; the convergence is the FINDING family, never the matcher vocabularies — the authored tier keeps the query `ValueMatch`, the baseline keeps the axis-named `TemplateVerdict`, and collapsing those two erases the axis evidence; a `Rasm.AppUi` pairing of two sibling reports is the deleted form this owner forecloses; the document-validity `AuditFile` and the cross-tool `Reconcile` stay orthogonal legs — file conformance and oracle parity are not model health.

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
// The ONE model-health verdict family both QA tiers converge on — the case IS the tier discriminant, so a
// consumer dispatches tier through the generated Switch and a parallel tier vocabulary beside the family is the
// deleted form. Baseline carries the Semantics-produced TemplateFinding row WHOLE (axis-named verdict, template
// coordinate, actual value — minted by TemplateAudit, never re-shaped here); Authored carries the per-element
// failed requirement with its full typed evidence and the computed facet key. The identities differ BY DOMAIN: a
// baseline row keys the seam NodeId (the template audit covers the IFC-classified occurrence nodes, authored
// elements not yet exported included), an authored row the IFC GlobalId (the IDS exchange is defined over the
// IFC-visible universe) — the identity is the tier's own audit universe.
[Union]
public partial record ModelFinding {
    partial record Structural(AuditFinding Finding);
    partial record Baseline(TemplateFinding Finding);
    partial record Authored(string GlobalId, string Specification, int Spec, int Requirement, ContentAddress Key, IdsFacet Facet, IdsCardinality Cardinality, RuleSeverity Severity);

    // The enforcement band a model-health verdict reads. The STRUCTURAL tier grades on the seam's own
    // ConstraintSeverity, so its band is READ off the audit row's Blocks column rather than re-decided here — a
    // dangling reference blocks wherever it is found and a coverage shortfall advises. The BASELINE tier is the
    // spec-free floor — no party authored it as an exchange requirement, so a missing standard property advises and
    // never fails a model that satisfies every authored specification; the AUTHORED tier carries its specification's
    // own declared band, so a contracted requirement blocks while a Rasm-authored advisory specification reports.
    public RuleSeverity Severity => Switch(
        structural: static s => s.Finding.Severity.Blocks ? RuleSeverity.Error : RuleSeverity.Warning,
        baseline: static _ => RuleSeverity.Warning,
        authored: static a => a.Severity);

    // The per-finding report group key: the audit category for a structural row, the template coordinate for a
    // baseline row, the ordinal-qualified facet key for an authored row — the axis a report groups one element's
    // findings on. The structural key is the seam's own frozen category token, so a report and a waiver pin the
    // same string the audit detail already keys on.
    public string Coordinate => Switch(
        structural: static s => s.Finding.Category.Key,
        baseline: static b => $"{b.Finding.Set}.{b.Finding.Code}",
        authored: static a => $"{a.Spec}:{a.Requirement}:{a.Key.Value:X32}");
}

// --- [MODELS] -----------------------------------------------------------------------------
// The three-tier model-QA receipt — the ONE entry Rasm.AppUi and the review pipeline read. Scope is the POLICY
// column the baseline graded under (Semantics/properties#PROPERTY_TEMPLATES TemplateScope), stored because an
// empty baseline under the standard set and under a COBie handover answer different questions — a receipt naming
// no definition set lets a reader read standard-set silence as handover completeness. The tier receipts stay
// typed evidence: the seam ModelAudit carries its graded snapshot address, its coverage census, and the policy it
// graded under (so a stored health receipt re-reads against the floors it was actually gated on), the baseline
// TemplateFinding rows carry the failing axis a fix pass acts on, the composed IdsAudit rows carry the spec-level
// cardinality rule and feed Reconcile unchanged; Findings is the derived one-family projection, never a fourth
// stored copy.
public sealed record ModelHealth(TemplateScope Scope, ModelAudit Structural, Seq<TemplateFinding> Baseline, Seq<IdsAudit> Authored) {

    // The three-tier entry over ONE frozen graph. The STRUCTURAL tier is the seam's own
    // Rasm.Element/Projection/audit#AUDIT_FOLD ModelAudit — neutral graph integrity and per-discipline coverage
    // ratios, graded where the graph is owned — composed here beneath the IFC tiers under an absolute scope split:
    // the seam audit owns what any consumer can name (a dangling bag reference, a Compose cycle, an address drift),
    // this page owns the IFC claims only the schema authority can make. The baseline tier ALWAYS runs — an empty
    // specification set still yields the zero-configuration template verdict — and each authored spec folds through
    // its own total Audit; the Fin rails are the audit's uniform seam rail and the baseline's seam Bake. scope
    // arrives pre-constructed and threads straight into the one TemplateAudit.Run fold, so a handover audit grades
    // COBie completeness on the SAME fold that grades the standard sets — never a second baseline entry per scope.
    public static Fin<ModelHealth> Audit(ElementGraph graph, TemplateScope scope, Seq<IdsSpecification> specifications, Func<IfcClass, Option<BsddClass>> dictionary, Op key) =>
        from structural in ModelAudit.Of(graph, key)
        from baseline in TemplateAudit.Run(graph, scope, dictionary, key)
        select new ModelHealth(scope, structural, baseline, specifications.Map(spec => spec.Audit(graph)));

    // The one flattened verdict stream: every seam audit row a Structural case, every baseline row a Baseline case,
    // every authored FacetVerdict's Failed element an Authored case carrying the requirement's ordinal identity, its
    // computed facet key, and its specification's enforcement band — a pure fold over the stored receipts.
    public Seq<ModelFinding> Findings =>
        Structural.Findings.Map(static f => (ModelFinding)new ModelFinding.Structural(f))
        + Baseline.Map(static f => (ModelFinding)new ModelFinding.Baseline(f))
        + Authored.Bind(static audit => audit.Verdicts
            .Map(static (v, i) => (Verdict: v, Requirement: i))
            .Bind(r => r.Verdict.Failed.Map(g => (ModelFinding)new ModelFinding.Authored(
                g, audit.Specification, audit.Spec, r.Requirement, r.Verdict.Key, r.Verdict.Facet, r.Verdict.Cardinality, audit.Severity))));

    // The one model-health verdict: no BLOCKING finding, and no authored specification left partially graded. A
    // template-floor gap advises rather than failing — the prior empty-baseline rule failed a model that satisfied
    // every authored specification because one standard property was absent — while an Indeterminate audit (a
    // requirement facet the lowering could not lift) is never a pass, because nothing graded it.
    public bool Conforms =>
        !Findings.Exists(static f => f.Severity.Blocking) && Authored.ForAll(static a => a.Conforms);
}
```

## [04]-[RESEARCH]

(none)
