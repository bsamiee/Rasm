# [BIM_COORDINATION]

`Rasm.Bim/coordination` owns model-checking and coordination over the seam `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph`: the if-X-then-Y rule engine, the clash-resolution proposal fold, the A/B model-impact report, the IDS-audit board handoff, the BCF sign-off state machine, and the host-neutral BCF issue-board the `Rasm.Persistence/Version/ledger` and `Rasm.AppUi/Collab/issues` relocations settle here. Every workflow composes a settled vocabulary the IDS/BCF/Diff owners supply but never assemble — the `Model/query#ELEMENT_SET` `ElementPredicate` algebra, the `Model/systems#INTERFERENCE` `Interference` ranked clash evidence, the `Model/systems#CONNECTIVITY`/`#SYSTEM_TRACE` `DistributionNetwork.View` flow views and `SystemTrace` fold, the `Review/diff#MODEL_DIFF` change-sets, the `Review/issues#BCF_ARCHIVE` `BcfTopic` family, and the `Planning/schedule#SCHEDULE`/`Planning/cost#ESTIMATE` joins — re-deriving none: no second predicate surface, no re-run proximity test, no second reachability walk, no re-computed diff.

Identity follows the seam law [H6]: a kernel receipt keys on the neutral `Rasm.Element/Graph/element#NODE_MODEL` `NodeId` — the COMPLETE identity present on every node, so a `RuleVerdict` over the WORKING graph reports an authored element carrying no IFC `GlobalId` yet — while an IFC-semantic receipt keys on the IFC `ExternalId` because its join targets (`Review/issues#BCF_ARCHIVE` viewpoints, `Planning/schedule#SCHEDULE` `TaskAssignment.ElementGlobalIds`, `Planning/cost#ESTIMATE` `CostItem.PricedGlobalIds`) are themselves GlobalId-keyed; the `NodeId → ExternalId` projection happens at the boundary through the Bim-stored `Node.Object.ExternalId`. This owner also holds the BCF issue-board DOMAIN over the `Review/issues#BCF_ARCHIVE` `BcfTopic`/`BcfComment`/`BcfViewpoint` family and the `BcfApi` server dialect, while `Rasm.Persistence/Version/ledger` keeps the durable op-log/CDE-sync store and `Rasm.AppUi/Collab/issues` keeps only the board projection — the three joined by the `ExternalId` content-key, never a second BCF schema across the boundary. FILE-WIRE legs cross as `.bcfzip` BYTES through the branch's ONE container custodian: `Propose`/`Raise`-minted topics LEAVE through the `BcfArchive` codec this package owns and a foreign tool's resolved topics RETURN as the status moves the `SignOff` lifecycle consumes, while `Rasm.Persistence/Ingest/issue` holds the durable `IssueTopic` rows the composition root transcribes under the `BcfTopic`⇄`IssueTopic` correspondence law — the two non-referencing S2 ends meeting at the root, never over a second container codec. Behaviour a composition tunes — the discipline yield hierarchy, the ripple severity bands, the clash-test library — arrives as `CoordinationPolicy` VALUES the `Semantics/classification#CLASSIFICATION_AXIS` `BsddPins` precedent shapes, never as durable roster edits. Every coordination rejection lifts the `Model/faults#FAULT_BAND` `BimFault` band BARE (the `Expected`-derived case IS the `Error`, no `.ToError()` hop). Coordination is HOST-LOCAL.

## [01]-[INDEX]

- [02]-[COORDINATION]: the `CoordinationRule` `[Union]` (the five model-check modalities — `Require`/`Prohibit`/`Cardinality`/`Unique`/`Reachable`, severity the abstract axis), the `Resolution` `[Union]`, the `RuleVerdict` `NodeId`-partition receipt, the `ClashTest` predicate-pair test and its `ClashReport`/`ClashProposalRow` lifecycle-columned run receipt, the composition-supplied `CoordinationPolicy` (`DisciplineRule` yield rows + `ImpactSeverity` bands), the `ImpactReport`/`ImpactRow` hop-measured impact receipt, the `FederatedModel` union-ingress receipt over the seam `Federate`, and the `Coordination` fold owner (`Federate`/`Check`/`Run`/`Propose`/`Cleared`/`Between`/`Raise`) over the seam `ElementGraph`.
- [03]-[SIGN_OFF]: the `SignOff` `[SmartEnum<string>]` lifecycle over the `Review/issues#BCF_ARCHIVE` `BcfStatus`, and the `IssueBoard` host-neutral board projection (lanes plus the archive's declared priority roster) the `Rasm.AppUi/Collab/issues` relocation grounds here.

## [02]-[COORDINATION]

- Owner: `CoordinationRule` the closed `[Union]` of the five model-check modalities, each carrying an applicability `Model/query#ELEMENT_SET` `ElementPredicate` (the X — the SAME selection surface the `Review/validation#IDS_FACETS` IDS facet fold reads) and its modality-specific requirement (the Y), with `Applicability` and `Severity` the abstract members every arm overrides positionally; `Resolution` the closed `[Union]` of proposed clash fixes; `RuleVerdict` the per-rule receipt folding the applicable set into the passing and the violating `NodeId` partition; `ClashTest` the named coordination test — the discipline predicate PAIR, the seam `MeasureValue` tolerance, and the `ClashKind` the pair is tested under — whose `Run` answers a `ClashReport` of `ClashProposalRow`s grouped under the test identity and DIFFED against the prior run, so a coordinator reads a clash matrix's cells rather than one undifferentiated interference stream; `ClashProposalRow` the proposed fix over one `Model/systems#INTERFERENCE` `Interference` — the clashing `NodeId` pair, the yielding endpoint, the ranked `Resolution` carrying its deficit as a seam `MeasureValue`, the `ClashState` lifecycle column, and the `Option<BcfTopic>` anchor an IFC-visible pair earns; `CoordinationPolicy` the ONE composition-supplied behaviour value (the `DisciplineRule` yield/fix rows and the `ImpactSeverity` hop bands, `Default` serving an unconfigured root) the `Semantics/classification#CLASSIFICATION_AXIS` `BsddPins` precedent shapes; `ImpactReport` the A/B fold over two `Review/diff#MODEL_DIFF` change-sets into the contested seed and the transitively downstream-affected element/task/cost-line/system closures, each rippled row an `ImpactRow` carrying its hop distance and severity band; `FederatedModel` the union-ingress receipt carrying the one federated graph beside the seam `FederationReceipt` provenance rows and the geodetic `FrameAlignment` matrix the union was gated on; `Coordination` the static fold owner (`Federate`/`Check`/`Run`/`Propose`/`Cleared`/`Between`/`Raise`) collapsing the prior `CoordinationCheck`/`ClashProposal`/`ChangeImpact` triplet into one deep coordination domain owner and carrying the write-time IDS-audit-to-board handoff.
- Cases: `CoordinationRule` arms `Require` (`ElementPredicate Applicability`/`Requirement`, `RuleSeverity`) · `Prohibit` (same shape, the IDS-mirrored declarative polarity) · `Cardinality` (`Applicability`, `int Min`, `Option<int> Max`, `RuleSeverity` — the applicable-set count must lie in `[Min, Max]`) · `Unique` (`Applicability`, `ValueSource Source`, `RuleSeverity` — every applicable element's source value distinct, the source a direct `ObjectAttribute` OR an effective Pset/Qto property read through `ElementSet.ValuesOf`) · `Reachable` (`Applicability`, `ElementPredicate Target`, `TraceMode Mode`, `RuleSeverity` — every applicable element must reach a `Target`-matching element through its owning `DistributionSystem`'s `Mode`-oriented flow graph) (5) — a per-element predicate, a set-count bound, an attribute uniqueness, and a graph-reachability incidence are the four irreducible model-check shapes, each one arm reusing the one selection algebra (the `Reachable` arm also reusing the `Model/systems#SYSTEM_TRACE` `TraceMode` orientation rows), never a per-rule-kind class and never a second predicate surface; an advisory check is `RuleSeverity.Info` on any arm — the retired `Recommend` arm was `Require`@`Info` spelled as a modality, the severity parameter already generating that space; `Resolution` arms `Reroute` (a suggested centerline offset for a linear MEP run) · `Resize` (a suggested dimension reduction for a discrete element) · `GrantClearance` (an accepted clearance exception) · `Sleeve` (a framed penetration bore through the prevailing element) · `Reject` (no fix — the clash stands for a coordinator's manual review) (5), the four dimensioned arms carrying a seam `MeasureValue` and a bare `double` offset being the deleted form; every arm has a producer on the `CoordinationPolicy` discipline rows — `Sleeve` where a rigid Structural or Architectural element prevails, `Reject` where neither side moves on a fold's authority; `ClashState` rows `New` (absent from the prior run) · `Active` (present in both) · `Resolved` (present in the prior run alone, carried forward with its prior fix so the row survives its own disappearance) (3) — the run-over-run lifecycle a re-test answers, keyed by the `ClashProposalRow` pair-and-test identity so a re-run neither re-opens a settled cell nor silently drops a fixed one.
- Entry: `Coordination.Federate(Seq<(string Model, ElementGraph Graph)> models, Header coordination, (double X, double Y, double Z) anchor, CancellationToken token, Op key)` is the FEDERATED ingress every multi-discipline cycle enters through — it runs the `Semantics/georeference#GEODETIC_TRANSFORM` `GeoTransform.Preflight` pairwise matrix at the anchor FIRST, refuses on an `Unresolvable` row (`Model/faults#FAULT_BAND` `BimFault.CapabilityMiss` lifted BARE, the `coordination-frame-unresolvable:` detail naming both models and the leg's cause), then unions through the seam `Rasm.Element/Graph/element#FEDERATION` `ElementGraph.Federate`, returning the `FederatedModel` receipt whose union graph the folds below read and whose `FederationReceipt` provenance rows and alignment matrix ride the coordination evidence; `Coordination.Check(ElementGraph graph, Seq<CoordinationRule> rules, Op key)` validates the rule library then folds each rule to a `RuleVerdict` over the seam graph — `Fin<T>` aborting a malformed rule (a `Cardinality` bound `Min < 0` or `Max < Min`) onto `Model/faults#FAULT_BAND` `BimFault.ModelRejected` (the `coordination-rule:` detail family — the semantic-reject arm, never `UnmappedClass` whose meaning is an element-class miss) lifted BARE, the well-formed fold itself total, and the `DistributionNetwork.View` flow decomposition built ONCE per check and threaded into every verdict rather than rebuilt inside each reachability arm; `Coordination.Propose(ElementGraph graph, Seq<Interference> interferences, CoordinationPolicy policy, string author, Instant at)` folds the ranked `Model/systems#INTERFERENCE` clash evidence into `ClashProposalRow`s — `Fin<T>` because the clash deficit admits through the ONE seam `MeasureValue.OfSi` gate, each clash `NodeId` resolved to its IFC `ExternalId` for the BCF anchor and a pair carrying no IFC identity yielding a proposal with no topic rather than a forged one; `Coordination.Run(ElementGraph graph, Seq<Interference> interferences, ClashTest test, Option<ClashReport> prior, CoordinationPolicy policy, string author, Instant at)` is the named-test entry — it scopes the interference evidence to the pair of discipline predicates under the test's tolerance and `ClashKind`, folds it through the SAME `Propose` body, then stamps each row's `ClashState` by joining the run against `prior` on the pair-and-test identity, so the report answers what is new, what still stands, and what a fix closed; `Coordination.Cleared(ClashIndex index, ClashProposalRow row)` is the verification a `GrantClearance` implies — it reads the yielding member's seated `BoundVolume` off the `ClashIndex` handle registry and answers the `Model/systems#INTERFERENCE` `InterferenceCheck.Neighborhood(index, volume)` clearance ring minus the granted pair, total, so a waiver states every further element inside the envelope it accepted; `Coordination.Between(ElementGraph graph, ModelDiff before, ModelDiff after, ScheduleNetwork schedule, CostSchedule cost, CoordinationPolicy policy)` folds the two change-sets into the contested seed and its reachability closures — total, the flow leg composing the settled `Model/systems#SYSTEM_TRACE` fold, the schedule/cost legs one `QuikGraph` transitive closure, every join reading the IFC-GlobalId axis; `Coordination.Raise(Seq<IdsAudit> audits, string author, Instant at)` folds every NON-conforming `Review/validation#IDS_FACETS` `IdsAudit` onto one `BcfTopic` keyed on the audit's document ORDINAL (`ids-{a.Spec}`, titled `name#ordinal` — the same ordinal-qualified identity `IdsAudit.Reconcile` and `BimEvent.VerdictIssued` join on, because IDS v1.0 spec names are not unique and a name-keyed topic silently merges two specifications' failures into one board row), one `BcfComment` per failing facet, the viewpoint anchoring every failed `GlobalId` — the write-time IDS↔BCF seam the read-time shared `ElementPredicate` algebra mirrors, total because the audit is already IFC-GlobalId-keyed.
- Auto: `Check` first runs each rule through `Validate` (`TraverseM` short-circuiting the whole library on the first malformed rule) then maps the well-formed library to verdicts; `Verdict` selects the applicable set ONCE through `ElementSet.Query(graph, rule.Applicability)` and dispatches the generated total `Switch` — `Require`/`Prohibit` collapse to ONE `Predicated` partition derived by a `prohibits` policy bit (the matching subset re-folded over only the current members via `ElementSet.Where`, never the retired O(n²) `Holds` re-query — the passing set for `Require`, the violating set for `Prohibit`), `Cardinality` partitions on the applicable-set count (in range the whole set passes, out of range the whole set is the violating evidence so the board highlights every element of a storey that breaches its exit count), `Unique` groups the applicable set by the `ObjectAttribute.Read` value (a duplicate-valued OR an unreadable-attribute member violates, the rest pass), and `Reachable` folds each applicable element through the settled `Model/systems#SYSTEM_TRACE` closure — the check's ONE `DistributionNetwork.View` decomposition threaded as `Switch` state, the `Mode`-oriented `SystemTrace.From`, intersected against the ONE-query `Target` set — so an element in no system or whose trace reaches no target violates (the orphaned terminal IS the defect the rule surfaces); `Propose` reads each `Interference` (consuming the systems-page ranked clash evidence rather than re-deriving proximity), admits its deficit as a seam Length measure, elects the yielding endpoint through the threaded `CoordinationPolicy` discipline ranks, and reads the fix off that discipline's own row delegate under the `ClashKind` waiver the policy holds, then mints a `BcfTopic` per clash anchoring the resolved `ExternalId` pair onto a `BcfViewpoint.SelectedGlobalIds`; `Run` narrows the interference stream to the pairs whose two endpoints satisfy the test's `Left`/`Right` predicates in either orientation and whose deficit clears the test tolerance, folds them through the SAME `Propose` body, then LEFT-joins the prior report on the `(Test, First, Second)` identity so an unmatched incoming row stamps `New`, a matched one `Active`, and an unmatched PRIOR row carries forward stamped `Resolved` — the report is the run-over-run clash matrix cell, never a re-computed proximity test; `Between` intersects the two `ModelDiff` change-sets by IFC GlobalId into the contested seed, resolves the seed to `NodeId`s through the graph's `ExternalId` index, propagates each seed member downstream through its owning system's flow graph (`SystemTrace.From(system, seed, TraceMode.Downstream)` — the settled reachability fold, never a second walk), joins the CLOSED affected set to the `ScheduleNetwork.Assignments` tasks naming it and the `CostSchedule.Items` lines pricing it, closes BOTH transitively through the one `Downstream` kernel — the `SequenceRel` predecessor→successor DAG (a slipped task delays every transitive successor) and the `CostItem.ParentGlobalId` roll-up tree (a repriced line stales every ancestor) two edge-row inputs to ONE multi-source `QuikGraph` breadth-first walk — the graph folded once per leg with a synthetic source edged to every seed, so a single `Compute` from that source discovers the whole frontier at once and the attached `VertexDistanceRecorderObserver` measures every row's hop distance from its NEAREST seed by construction, the recorded distance shifted down one to seat the seeds at zero — and surfaces the `Model/systems#CONNECTIVITY` `DistributionSystem`s whose member set the closure intersects.
- Receipt: the `Seq<RuleVerdict>` is the parameterized model-checking evidence (a coordination-rule library the IDS/BCF/Diff owners give the vocabulary for) on the `NodeId` axis the AppUi board highlights and Persistence stores; the `ClashReport` the per-test run receipt — the ranked rows with proposed fixes, each carrying its `ClashState` against the prior run and its BCF anchor on the IFC `ExternalId` axis a viewer round-trips — so a coordination cycle reports progress (what closed, what remains) rather than a fresh undifferentiated stream each pass; the `ImpactReport` the transitive A/B change-impact closure on the IFC-GlobalId axis a 4D/5D federation reads (the contested seed the board anchors contention on, the closures the ripple evidence); the `SignOff` lifecycle governs the issue, so a governed workflow — rule check → clash test run → impact report → BCF sign-off — reads one composed pipeline over the settled owners.
- Packages: Rasm.Element, QuikGraph, SwiftCollections.Lean (the `BoundVolume` the clearance read passes through; the broad phase itself is `Model/systems#INTERFERENCE`'s), Thinktecture.Runtime.Extensions, NodaTime, LanguageExt.Core, Rasm
- Growth: a new model-check modality is one `CoordinationRule` union arm reusing the `ElementPredicate` algebra (the rule library never forks a second selector — the `Reachable` arm reuses the `TraceMode` rows as its orientation policy, never a second trace enum); a new clash fix is one `Resolution` arm plus the `CoordinationPolicy` discipline row that elects it; a new proposal heuristic is one row's delegate column and a re-tuned trade hierarchy, ripple threshold, or clash-test library one value a composition passes, never a durable-page edit and never a branch in the fold; a new severity band is one `RuleSeverity` row `Review/validation#IDS_FACETS` owns; a new clash-lifecycle state is one `ClashState` row the join stamps; a new post-fix verification is one read over the broad phase `Model/systems#INTERFERENCE` already retains (`Cleared` the standing exemplar), never a second index here; a new impact dimension (a downstream-affected zone, a downstream-affected document) is one column on `ImpactReport` with at most one edge-row input to the one `Downstream` distance kernel; never a per-rule-kind type, never a second selection surface, and never a re-derived proximity, reachability walk, or diff in this owner.
- Boundary: the rule applicability/requirement is the `Model/query#ELEMENT_SET` `ElementPredicate` (the `All`/`Any`/`Not` boolean closure composing a multi-condition rule), so the validation predicate IS the query predicate IS the coordination predicate — one selection surface across `Model/query#ELEMENT_SET`, `Review/validation#IDS_FACETS`, and this owner, a parallel `RuleSelector`/`CoordinationQuery` expression type the deleted form, and the retired `new ElementSet(model.Elements)` over a second stored `BimElement` collection GONE (the rule folds the seam graph the `Projection/semantic#SEMANTIC_PROJECTOR` assembles); the `ClashProposal` fold's clash evidence is the `Model/systems#INTERFERENCE` `Interference` row carrying the `NodeId` pair (the systems page owns the geometric proximity, this owner consumes the ranked evidence and proposes the fix) — re-running the proximity test here is the named cross-page drift defect, and the clearance verification obeys the SAME law: `Cleared` composes the systems-owned `InterferenceCheck.Neighborhood` ring over the retained `ClashIndex`, so a coordination-local radius query, a second spatial structure, or a hand-rolled distance sweep beside the seated broad phase is the deleted form, and the retired `clash.FirstGlobalId`/`clash.SecondGlobalId` string pair is GONE, replaced by `clash.First`/`clash.Second` `NodeId` with the IFC `ExternalId` resolved only at the BCF anchor; the `ImpactReport` ripple and the `Reachable` verdict are REAL reachability — the flow leg composes the settled `Model/systems#SYSTEM_TRACE` `SystemTrace.From` over the ONE graph-scoped `DistributionNetwork.View` decomposition (a per-rule rebuild of that fold is the deleted form) and the schedule/cost legs the one `Downstream` `QuikGraph` breadth-first distance kernel whose recorder attaches through the observer's own `IDisposable` scope — so the retired flat two-diff intersection + one-hop membership join (`AffectedSystemsOf` testing set intersection where the prose claimed "ripples into"), a hand-rolled visited-set walk, and a distance-blind closure that reads a fourth-order successor as a seed are the deleted forms, the `[GRAPH_ALGORITHM]` owner collapsing every such walk; the distance kernel is ONE multi-source walk over ONE graph — a synthetic source edged to every seed makes the whole seed set the frontier at depth one, so the per-seed `Compute` loop with its `Math.Min` distance merge is the deleted form (it re-walked the shared closure once per seed) and so is any post-hoc seeding of the distance map, because `VertexDistanceRecorderObserver` records off TREE EDGES alone and an unsequenced task or parentless cost line reached by no edge would otherwise carry no measurement while its successors landed, dropping the row the severity bands rank HIGHEST; a `Resolution` carries a seam `MeasureValue` and a bare `double` deficit crossing to a BCF topic or a fabrication consumer is the deleted form; a BCF viewpoint anchors ONLY on a real IFC `ExternalId` and the `IfNone(NodeId)` fallback that leaked a neutral key into a `SelectedGlobalIds` slot is the deleted form; a minted viewpoint carries `Option<BcfCamera>` and the zero-filled `new BcfCamera.Perspective(default, default, default)` is the deleted form — a selection-only viewpoint is legal BCF, and a degenerate camera at the origin with a zero direction publishes a view a receiving tool renders as a black frame while reading as authored intent; a `RuleVerdict` keys on the COMPLETE `NodeId` identity (an `ExternalId`-keyed verdict silently dropping an un-emitted authored element is the deleted form), while the BCF anchor and the impact join key on the IFC `ExternalId` their targets demand; a coordination run is a NAMED `ClashTest` over a discipline predicate pair and its own tolerance, and a bare `Propose` over an undifferentiated interference stream is the thin form this owner outgrew — a coordinator works a clash matrix whose cells carry lifecycle, so the report groups under the test identity and every row states its `ClashState` against the prior run; the yield hierarchy, the fix delegates, and the ripple bands are `CoordinationPolicy` VALUES a composition supplies (`Default` serving an unconfigured root, the `Semantics/classification#CLASSIFICATION_AXIS` `BsddPins` precedent) and a durable `static class DisciplinePriority`/`ImpactSeverity` table is the deleted form — a project's trade hierarchy is project data, and freezing it into the page forces a page edit for a decision the composition owns; an IDS-raised topic keys on the audit's document ORDINAL and a specification-NAME key is the deleted form (IDS v1.0 names are not unique, so two specifications collapse onto one board row and the second's failures vanish); the multi-model union is the SEAM's `Rasm.Element/Graph/element#FEDERATION` `ElementGraph.Federate` — it owns the coordination-header refusal axes and the id-collision-versus-dedup discrimination, so a coordination-local graph merge, a per-source header roster, or a re-decided id rename beside that entry is the deleted form, and the geodetic `GeoTransform.Preflight` matrix gates the union rather than reconciling frames (frame alignment is the reprojection leg's, never the union's); the `CoordinationRule`/`Resolution`/`ClashState` unions are closed families and a per-kind class is the deleted form; the coordination operations live on the ONE `Coordination` owner (the prior `CoordinationCheck`/`ClashProposal`/`ChangeImpact` single-method classes collapsed; the IDS-audit board handoff `Raise` a fourth fold on the same owner, never a sibling class), and a coordination rejection lifts `Model/faults#FAULT_BAND` `BimFault` BARE — the `Expected`-derived case IS the `Error`, the retired `.ToError()` lowering hop and the `new BimFault.X("string")` single-arg construction GONE (the band ctor is `(Op key, string detail)`).

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LanguageExt;
using Rasm.Bim.Model;
using NodaTime;
using QuikGraph;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.Observers;
using QuikGraph.Algorithms.Search;
using Rasm.Element.Graph;
using Rasm.Element.Properties;
using SwiftCollections.Query;   // BoundVolume — the ONE bound type Model/systems#INTERFERENCE indexes and this owner only passes through
using Thinktecture;
using Op = Rasm.Domain.Op;
using static LanguageExt.Prelude;

// Coordination DOMAIN namespace the ARCHITECTURE seams name — the Review/issues#BCF_ARCHIVE BcfTopic family
// and the SignOff lifecycle the Rasm.AppUi/Collab/issues board consumes as Rasm.Bim.Coordination.*; the
// child namespace resolves the sibling Rasm.Bim owners (ElementSet/ElementPredicate/Interference/ClashIndex/
// ClashCandidate/InterferenceCheck/IfcDomain/RuleSeverity/ModelDiff/ScheduleNetwork/CostSchedule/
// DistributionNetwork/SystemTrace/TraceMode/IdsAudit) implicitly, the seam owners through Rasm.Element.
namespace Rasm.Bim.Coordination;

// --- [MODELS] -----------------------------------------------------------------------------
// If-X-then-Y model-check vocabulary: every arm carries an applicability ElementPredicate (the X — the SAME
// Model/query#ELEMENT_SET selection surface the IDS facet fold reads, never a second RuleSelector) with its own
// modality requirement (the Y). Five modalities — a per-element polarity pair (Require/Prohibit), a set-count
// bound (Cardinality), a value-source uniqueness (Unique — a direct attribute or an effective Pset/Qto value),
// a flow-graph reachability (Reachable, orientation the Model/systems#SYSTEM_TRACE TraceMode rows) — the closed
// family a new modality lands in as one arm. Applicability and Severity are abstract members every arm overrides
// positionally, so a rule selects and reports without a Switch; an ADVISORY rule is any arm at RuleSeverity.Info —
// severity is the axis, never a sibling arm (the retired Recommend case was Require@Info spelled as a modality),
// and the vocabulary is the one Review/validation#IDS_FACETS declares, so a rule verdict and a model-health
// finding band identically.
[Union]
public abstract partial record CoordinationRule {
    private CoordinationRule() { }

    public abstract ElementPredicate Applicability { get; }
    public abstract RuleSeverity Severity { get; }

    public sealed record Require(ElementPredicate Applicability, ElementPredicate Requirement, RuleSeverity Severity) : CoordinationRule;
    public sealed record Prohibit(ElementPredicate Applicability, ElementPredicate Requirement, RuleSeverity Severity) : CoordinationRule;
    public sealed record Cardinality(ElementPredicate Applicability, int Min, Option<int> Max, RuleSeverity Severity) : CoordinationRule;
    public sealed record Unique(ElementPredicate Applicability, ValueSource Source, RuleSeverity Severity) : CoordinationRule;   // distinctness over a direct attribute OR an effective Pset/Qto value (space numbers, door marks)
    public sealed record Reachable(ElementPredicate Applicability, ElementPredicate Target, TraceMode Mode, RuleSeverity Severity) : CoordinationRule;
}

// Closed clash-fix family proposed onto a Model/systems#INTERFERENCE clash. Every dimensioned arm carries a seam
// MeasureValue, never a bare double: the clash Deficit is a kernel-SI scalar and a proposal that crosses to a BCF
// topic, a fabrication ticket, or a UI dimension entry must carry its quantity type and dimension with it — an
// unlabelled double is the estate's named defect, and the consumer that re-derives millimetres from it is why.
// Reject is the coordinator's manual override, the one arm carrying no measure.
[Union]
public abstract partial record Resolution {
    private Resolution() { }

    public sealed record Reroute(MeasureValue Offset) : Resolution;
    public sealed record Resize(MeasureValue Dimension) : Resolution;
    public sealed record GrantClearance(MeasureValue Accepted) : Resolution;
    public sealed record Sleeve(MeasureValue Bore) : Resolution;
    public sealed record Reject : Resolution;
}

// Per-rule receipt on the COMPLETE NodeId identity (present on every node pre-Emit), never the IFC ExternalId
// — a coordination check on the working graph must report an authored element that carries no GlobalId yet. The
// rule rides the receipt so the board reads which rule and which modality failed without a stringly RuleKind tag,
// and Severity DERIVES from the rule (a receipt column restating the rule's own severity is the deleted
// duplication); a consumer raising a BCF topic from a violation projects NodeId -> ExternalId through the graph it checked.
public sealed record RuleVerdict(CoordinationRule Rule, Seq<NodeId> Passed, Seq<NodeId> Violated) {
    public RuleSeverity Severity => Rule.Severity;
    public bool Conforms => Violated.IsEmpty;
    public bool Blocking => !Conforms && Severity.Blocking;
}

// The run-over-run lifecycle a re-test answers, stamped by joining a run against its predecessor on the
// (Test, First, Second) identity. Resolved rows are CARRIED FORWARD from the prior report rather than dropped: a
// coordination cycle is measured by what closed, and a row that vanishes silently reports nothing.
[SmartEnum<string>]
public sealed partial class ClashState {
    public static readonly ClashState New = new("new");
    public static readonly ClashState Active = new("active");
    public static readonly ClashState Resolved = new("resolved");
}

// One NAMED coordination test — the clash-matrix cell a coordinator actually works: the two discipline
// ElementPredicates (the ONE selection algebra, never a second selector), the seam MeasureValue tolerance below
// which an interference is not reported, and the ClashKind the pair is tested under (a hard clash between
// structure and ducts, a clearance graze around a valve). The predicates are ORIENTATION-FREE — a pair matches
// when either assignment satisfies (Left, Right) — because an interference carries no authored direction.
public sealed record ClashTest(string Name, ElementPredicate Left, ElementPredicate Right, MeasureValue Tolerance, ClashKind Kind);

// Clash proposal: the owning test identity, the clashing NodeId pair (kernel identity), the Yields endpoint the
// policy's discipline hierarchy elects to change, the ranked Resolution, the lifecycle State against the prior
// run, and the BcfTopic the fix lands on. The topic is OPTIONAL because BCF is an IFC-native container whose
// SelectedGlobalIds carry IFC GlobalIds: a clash between elements the exchange has never seen has no BCF anchor,
// and leaking a neutral NodeId into that slot publishes a viewpoint referencing an element no receiving tool can
// resolve. The proposal itself always lands — it keys on the COMPLETE NodeId identity — so a coordinator sees the
// fix and the board simply carries no topic until the pair is emitted.
public sealed record ClashProposalRow(
    string Test, NodeId First, NodeId Second, NodeId Yields, ClashKind Kind,
    Resolution Proposed, ClashState State, Option<BcfTopic> Topic);

// One test's run receipt: the test that produced it, its rows with their lifecycle stamps, and the instant the
// run sealed. Outstanding is the derived working set — a projection, never a second stored column.
public sealed record ClashReport(ClashTest Test, Seq<ClashProposalRow> Rows, Instant At) {
    public Seq<ClashProposalRow> Outstanding => Rows.Filter(static row => row.State != ClashState.Resolved);
    public bool Clear => Outstanding.IsEmpty;
}

// One impacted row on the IFC GlobalId axis: WHICH element/task/line, HOW FAR from the contested seed, and the
// severity band that distance earns. Hops is real graph distance on the schedule and cost legs (the DAG a
// breadth-first walk measures); on the element leg the flow trace publishes REACH and not depth, so a seed is zero
// and everything its systems carry downstream is one propagation hop.
public readonly record struct ImpactRow(string GlobalId, int Hops, RuleSeverity Severity);

// A/B change-impact report — every dimension on the IFC ExternalId axis because the join targets (the IFC
// model diff, the schedule assignments, the cost lines) are IFC-GlobalId-keyed receipts, never the neutral NodeId.
// Contested is the direct two-diff seed the BCF board anchors contention on; Elements the seed plus the
// flow-downstream closure; the task/line columns transitively closed, the systems the closure's memberships. Every
// rippled column carries typed rows rather than a bare id list, so a 4D/5D consumer sorts and thresholds on
// distance and severity instead of treating a seed and a fourth-order successor as one undifferentiated set.
public sealed record ImpactReport(
    Seq<string> Contested,
    Seq<ImpactRow> Elements,
    Seq<ImpactRow> Tasks,
    Seq<ImpactRow> CostLines,
    Seq<string> Systems);

// The federated ingress receipt: the ONE union graph every downstream fold reads, the seam FederationReceipt
// provenance rows (per-source snapshot address, header columns, node/edge counts, the merged tally the dedup
// produced), and the geodetic alignment matrix the union was gated on — so a coordination cycle over N discipline
// models states WHICH models it unioned, what the dedup merged, and on what frame evidence, instead of presenting an
// unattributed graph a coordinator cannot trace a clash back through.
public sealed record FederatedModel(ElementGraph Graph, FederationReceipt Federation, Seq<FrameAlignment> Alignment);

// --- [POLICIES] ---------------------------------------------------------------------------
// One coordination row per discipline: its yield RANK in rigidity order and the FIX a proposal elects when this
// discipline's element is the one that gives way. The delegate column is what makes the Growth promise real — a
// re-tuned trade hierarchy or a new proposal heuristic is one row edit, and Sleeve and Reject gain the producers a
// ternary ladder over ClashKind could never give them.
public readonly record struct DisciplineRule(int Rank, Func<MeasureValue, Resolution> Propose);

// The ONE composition-supplied coordination behaviour value — the trade hierarchy a project negotiates and the
// ripple thresholds a programme tunes are PROJECT data, so they live at an overridable value whose Default serves
// an unconfigured root (the Semantics/classification#CLASSIFICATION_AXIS BsddPins precedent), never frozen into a
// durable roster a re-tuning would have to edit. Ripple bands read the ONE RuleSeverity vocabulary
// Review/validation#IDS_FACETS owns, so a rule verdict and an impact row band identically.
public sealed record CoordinationPolicy(
    FrozenDictionary<IfcDomain, DisciplineRule> Disciplines,
    FrozenDictionary<int, RuleSeverity> RippleBands) {

    // An unrostered discipline never yields silently and never auto-fixes: it ranks last and proposes Reject.
    static readonly DisciplineRule Unranked = new(int.MaxValue, static _ => new Resolution.Reject());

    // The full seven-member default roster, so no discipline falls to the unranked default under Default: a
    // service crossing a rigid Structural or Architectural element is bored and framed, a Geotechnical or
    // Infrastructure conflict is coordinator-elected because neither side moves on a fold's authority, a gravity
    // Plumbing run re-sizes before it re-routes (its fall is fixed), and only the freely reroutable HvacFire and
    // Electrical services re-route. The contested seed blocks, its immediate successors warn, everything further
    // downstream is advisory context.
    public static readonly CoordinationPolicy Default = new(
        new Dictionary<IfcDomain, DisciplineRule> {
            [IfcDomain.Structural]     = new(0, static deficit => new Resolution.Sleeve(deficit)),
            [IfcDomain.Geotechnical]   = new(0, static _ => new Resolution.Reject()),
            [IfcDomain.Infrastructure] = new(0, static _ => new Resolution.Reject()),
            [IfcDomain.Architecture]   = new(1, static deficit => new Resolution.Sleeve(deficit)),
            [IfcDomain.Plumbing]       = new(2, static deficit => new Resolution.Resize(deficit)),
            [IfcDomain.HvacFire]       = new(3, static deficit => new Resolution.Reroute(deficit)),
            [IfcDomain.Electrical]     = new(4, static deficit => new Resolution.Reroute(deficit)),
        }.ToFrozenDictionary(),
        new Dictionary<int, RuleSeverity> { [0] = RuleSeverity.Error, [1] = RuleSeverity.Warning }.ToFrozenDictionary());

    DisciplineRule Rule(IfcDomain domain) => Disciplines.GetValueOrDefault(domain, Unranked);

    // True when the second endpoint is the lower-or-equal-priority discipline (it yields); an equal-rank
    // same-discipline clash tie-breaks onto the second endpoint deterministically.
    public bool SecondYields(IfcDomain first, IfcDomain second) => Rule(second).Rank >= Rule(first).Rank;

    // The clash KIND axis decides waiver, the yielding DISCIPLINE row decides the fix — the one branch lives here in
    // the policy owner, so the proposal fold stays a straight map and a new heuristic never grows an arm in it. A
    // clearance graze is an accepted exception whatever discipline yields; a hard clash reads its row's delegate.
    public Resolution Propose(ClashKind kind, IfcDomain yielding, MeasureValue deficit) =>
        kind == ClashKind.Clearance ? new Resolution.GrantClearance(deficit) : Rule(yielding).Propose(deficit);

    public RuleSeverity Band(int hops) => RippleBands.GetValueOrDefault(hops, RuleSeverity.Info);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
// Coordination DOMAIN owner: the rule-check fold, the clash-to-proposal fold, the A/B impact fold, and the
// IDS-audit board handoff — the workflows the IDS/BCF/Diff owners give the vocabulary for but never compose.
// Each reads the seam graph the Projection/semantic#SEMANTIC_PROJECTOR assembles; none re-derives a selection
// surface, a proximity test, a reachability walk, or a diff.
public static class Coordination {
    // The FEDERATED ingress: N discipline models become the ONE graph Check/Run/Between fold over. Frame alignment is
    // the geodetic preflight's — the pairwise matrix runs FIRST and an Unresolvable row REFUSES the union, because a
    // clash matrix over models the datum leg could not reconcile ranks proximity across two frames and still reads as
    // clean. The union itself is the seam ElementGraph.Federate: it refuses a bitwise-divergent tolerance and a
    // structurally-divergent GeoReference and discriminates an occurrence-id collision (always a fault) from a
    // content-id repeat (dedup on equal payloads), so this owner re-decides none of it and mints no merge of its own —
    // a coordination-local model union beside that entry is the deleted form. The FederationReceipt rows ride the
    // coordination evidence beside the alignment matrix, so a re-run over drifted sources reads as a distinct row set.
    public static Fin<FederatedModel> Federate(
        Seq<(string Model, ElementGraph Graph)> models, Header coordination,
        (double X, double Y, double Z) anchor, CancellationToken token, Op key) =>
        GeoTransform.Preflight(models.Map(static m => (m.Model, m.Graph.Header.Reference)), anchor, token, key) switch {
            var alignment =>
                alignment.Find(static row => row.Verdict is FrameVerdict.Unresolvable)
                is { IsSome: true, Case: FrameAlignment { Verdict: FrameVerdict.Unresolvable blocked } row }
                    ? Fin.Fail<FederatedModel>(new BimFault.CapabilityMiss(
                        key, $"coordination-frame-unresolvable:{row.SourceModel}:{row.TargetModel}:{blocked.Cause}"))
                    : ElementGraph.Federate(models, coordination, key)
                        .Map(union => new FederatedModel(union.Graph, union.Receipt, alignment)),
        };

    // Rule-check fold: validate the library (a malformed bound short-circuits the whole check onto the
    // Model/faults#FAULT_BAND coordination-rule band), then fold each well-formed rule to a verdict — pure ROP,
    // never a Try.lift exception funnel, because ElementSet.Query is total. The DistributionNetwork.View flow
    // decomposition is built ONCE per check and threaded through every verdict: it is a graph-scoped fold, so
    // rebuilding it inside each Reachable arm re-decomposed the whole network per rule.
    public static Fin<Seq<RuleVerdict>> Check(ElementGraph graph, Seq<CoordinationRule> rules, Op key) =>
        rules.TraverseM(rule => Validate(rule, key)).As()
            .Map(valid => Systems(graph) switch {
                var systems => valid.Map(rule => Verdict(graph, systems, rule)),
            });

    // The one graph-scoped flow decomposition: each DistributionSystem paired with its membership set, folded once
    // and threaded into every consumer that tests reach or membership.
    static Seq<(DistributionSystem View, LanguageExt.HashSet<NodeId> Members)> Systems(ElementGraph graph) =>
        DistributionNetwork.View(graph, None).Map(static system => (View: system, Members: toHashSet(system.Members)));

    // Clash-to-proposal fold: each ranked Interference maps onto a Resolution + the BcfTopic its resolved IFC
    // ExternalId pair anchors. Fin because the deficit admits as a seam MeasureValue — the ONE construction gate for
    // a dimensioned scalar, which refuses a non-finite magnitude rather than carrying it into a published fix. Rows
    // land New; Run re-stamps them against its prior report, so the unscoped entry states the honest default.
    public static Fin<Seq<ClashProposalRow>> Propose(
        ElementGraph graph, Seq<Interference> interferences, CoordinationPolicy policy, string author, Instant at) =>
        Propose(graph, interferences, policy, test: string.Empty, author, at);

    static Fin<Seq<ClashProposalRow>> Propose(
        ElementGraph graph, Seq<Interference> interferences, CoordinationPolicy policy, string test, string author, Instant at) =>
        interferences.Traverse(clash =>
            ResolveOf(clash, policy).Map(fix => new ClashProposalRow(
                test, clash.First, clash.Second, fix.Yields, clash.Kind, fix.Fix, ClashState.New,
                TopicOf(graph, clash, author, at)))).As();

    // The NAMED-test run: scope the ranked interference evidence to the test's discipline predicate pair (the ONE
    // selection algebra, each side queried ONCE) under the test's ClashKind and tolerance, propose through the SAME
    // body, then LEFT-JOIN the prior report on the (Test, First, Second) identity — an unmatched incoming row is
    // New, a matched one Active, and an unmatched PRIOR row is carried forward Resolved so the report measures what
    // a fix closed rather than losing it. The orientation is free: an Interference carries no authored direction,
    // so a pair matches when EITHER assignment satisfies (Left, Right).
    public static Fin<ClashReport> Run(
        ElementGraph graph, Seq<Interference> interferences, ClashTest test, Option<ClashReport> prior,
        CoordinationPolicy policy, string author, Instant at) {
        LanguageExt.HashSet<NodeId> left = ElementSet.Query(graph, test.Left).Ids;
        LanguageExt.HashSet<NodeId> right = ElementSet.Query(graph, test.Right).Ids;
        Seq<Interference> scoped = interferences
            .Filter(clash => clash.Kind == test.Kind && clash.Deficit >= test.Tolerance.Si)
            .Filter(clash => (left.Contains(clash.First) && right.Contains(clash.Second))
                          || (left.Contains(clash.Second) && right.Contains(clash.First)));
        return Propose(graph, scoped, policy, test.Name, author, at)
            .Map(rows => new ClashReport(test, Lifecycle(rows, prior.Map(static p => p.Rows).IfNone(Seq<ClashProposalRow>())), at));
    }

    // The one lifecycle join: the incoming run keyed by pair identity decides New vs Active, and every prior row
    // the run no longer reports is carried forward AS IT STOOD under Resolved — the fix a coordinator landed stays
    // readable on the row that proves it. The key is the (Test, First, Second) triple, so two tests over the same
    // pair keep distinct cells and a re-run of one never touches the other.
    static Seq<ClashProposalRow> Lifecycle(Seq<ClashProposalRow> run, Seq<ClashProposalRow> prior) {
        LanguageExt.HashSet<(string, NodeId, NodeId)> seen = toHashSet(prior.Map(Cell));
        LanguageExt.HashSet<(string, NodeId, NodeId)> current = toHashSet(run.Map(Cell));
        return run.Map(row => row with { State = seen.Contains(Cell(row)) ? ClashState.Active : ClashState.New })
             + prior.Filter(row => !current.Contains(Cell(row))).Map(static row => row with { State = ClashState.Resolved });
    }

    static (string, NodeId, NodeId) Cell(ClashProposalRow row) => (row.Test, row.First, row.Second);

    // A/B impact fold: the contested seed — the IFC-GlobalId intersection of two diffs this owner consumes,
    // never re-derives — propagated as a REAL reachability closure: downstream through each owning system's flow
    // graph (the settled SystemTrace fold), then transitively through the schedule DAG and the cost roll-up tree
    // via the one Successors kernel, every membership join reading the CLOSED set.
    public static ImpactReport Between(ElementGraph graph, ModelDiff before, ModelDiff after, ScheduleNetwork schedule, CostSchedule cost, CoordinationPolicy policy) {
        var touched = toHashSet(after.Changes.Map(static c => c.GlobalId));
        var contested = before.Changes.Map(static c => c.GlobalId).Filter(touched.Contains).Distinct().ToSeq();
        var byExternal = graph.ObjectNodes.Choose(static o => o.ExternalId.Map(e => (e, o.Id))).ToHashMap();
        var seeds = toHashSet(contested.Choose(byExternal.Find));
        Seq<(DistributionSystem View, LanguageExt.HashSet<NodeId> Members)> systems = Systems(graph);
        var ripple = toHashSet(systems.Bind(entry => entry.Members.Intersect(seeds).ToSeq()
            .Bind(seed => SystemTrace.From(entry.View, seed, TraceMode.Downstream).ReachedElements)));
        var affectedNodes = seeds.Union(ripple);
        // The flow leg publishes REACH, not depth, so a contested element is hop zero and everything its systems
        // carry downstream is one propagation hop; the schedule and cost legs measure true DAG distance below.
        var rippled = ripple.ToSeq().Choose(id => graph.Find<Node.Object>(id).Bind(static o => o.ExternalId))
            .Filter(id => !contested.Contains(id)).Distinct().ToSeq();
        var elements = contested.Map(id => new ImpactRow(id, 0, policy.Band(0)))
            + rippled.Map(id => new ImpactRow(id, 1, policy.Band(1)));
        var affectedSet = toHashSet(elements.Map(static row => row.GlobalId));
        var assigned = schedule.Assignments
            .Filter(a => a.ElementGlobalIds.Exists(affectedSet.Contains))
            .Map(static a => a.TaskGlobalId).Distinct().ToSeq();
        var priced = cost.Items
            .Filter(i => i.PricedGlobalIds.Exists(affectedSet.Contains))
            .Map(static i => i.GlobalId).Distinct().ToSeq();
        return new ImpactReport(
            contested,
            elements,
            Downstream(schedule.Dependencies.Map(static d => (d.PredecessorGlobalId, d.SuccessorGlobalId)), assigned, policy),
            Downstream(cost.Items.Choose(static i => i.ParentGlobalId.Map(parent => (i.GlobalId, parent))), priced, policy),
            systems.Filter(entry => entry.Members.Exists(affectedNodes.Contains))
                .Choose(static entry => entry.View.ExternalId).Distinct().ToSeq());
    }

    // Rule-shape gate as the generated TOTAL Switch (no runtime-silent `_`): the closed-vocabulary predicates
    // (ByClass/ByClassification carry already-validated IfcClass/Classification values) and the SmartEnum TraceMode
    // cannot miss, so the per-element, uniqueness, and reachability arms admit unconditionally — but a SIXTH
    // modality cannot land without declaring its own shape gate (the Switch breaks every site at compile time),
    // where a `_` catch-all would silently admit it unvalidated. The lone malformed shape is an impossible
    // Cardinality bound; the band is pinned by Model/faults#FAULT_BAND.
    static Fin<CoordinationRule> Validate(CoordinationRule rule, Op key) => rule.Switch(
        state:       key,
        require:     static (_, r) => Fin.Succ<CoordinationRule>(r),
        prohibit:    static (_, r) => Fin.Succ<CoordinationRule>(r),
        cardinality: static (k, r) => r.Min < 0 || r.Max.Match(Some: hi => hi < r.Min, None: static () => false)
                         ? new BimFault.ModelRejected(k, $"coordination-rule:cardinality-bound:{r.Min}..{r.Max.Match(Some: h => h.ToString(), None: static () => "*")}")
                         : Fin.Succ<CoordinationRule>(r),
        unique:      static (_, r) => Fin.Succ<CoordinationRule>(r),
        reachable:   static (_, r) => Fin.Succ<CoordinationRule>(r));

    // The applicable set and the graph-scoped flow decomposition thread as ONE Switch state, so every arm stays a
    // closure-free static lambda and the reachability arm reads the systems the check already built.
    static RuleVerdict Verdict(ElementGraph graph, Seq<(DistributionSystem View, LanguageExt.HashSet<NodeId> Members)> systems, CoordinationRule rule) =>
        rule.Switch(
            state:       (Applicable: ElementSet.Query(graph, rule.Applicability), Systems: systems),
            require:     static (s, r) => Predicated(r, s.Applicable, r.Requirement, prohibits: false),
            prohibit:    static (s, r) => Predicated(r, s.Applicable, r.Requirement, prohibits: true),
            cardinality: static (s, r) => Counted(r, s.Applicable),
            unique:      static (s, r) => Distinct(r, s.Applicable),
            reachable:   static (s, r) => Traced(r, s.Applicable, s.Systems));

    // Require/Prohibit collapse to ONE partition derived by the `prohibits` policy bit: the matching subset
    // (re-folded over only the current members via ElementSet.Where, never an O(n^2) re-query) is the passing
    // set for Require and the violating set for Prohibit; the verdict severity rides the rule itself.
    static RuleVerdict Predicated(CoordinationRule rule, ElementSet applicable, ElementPredicate requirement, bool prohibits) {
        var matching = applicable.Where(requirement);
        var (pass, violated) = prohibits ? (applicable.Except(matching), matching) : (matching, applicable.Except(matching));
        return new RuleVerdict(rule, Ids(pass), Ids(violated));
    }

    // Set-count bound: the whole applicable set is the evidence — in range it passes, out of range it
    // violates (a storey with zero exits surfaces every applicable element so the board highlights the breach).
    static RuleVerdict Counted(CoordinationRule.Cardinality rule, ElementSet applicable) =>
        rule.Min <= applicable.Count && rule.Max.Match(Some: hi => applicable.Count <= hi, None: static () => true)
            ? new RuleVerdict(rule, Ids(applicable), Seq<NodeId>())
            : new RuleVerdict(rule, Seq<NodeId>(), Ids(applicable));

    // Uniqueness check: group the applicable set by the ValueSource value — the direct ObjectAttribute row
    // OR the effective Pset/Qto property (space numbers, door marks — the dominant coordination distinctness checks),
    // read through the ONE Model/query#ELEMENT_SET ElementSet.ValuesOf exposure so the seam bag merge is never
    // re-derived here. A duplicate-valued member violates; an unreadable-source member (empty read) cannot prove
    // distinctness so it violates too, the rest pass — one group fold, never a nested compare.
    static RuleVerdict Distinct(CoordinationRule.Unique rule, ElementSet applicable) {
        var keyed = applicable.Objects.Map(o => (o.Id, Key: ElementSet.ValuesOf(applicable.Graph, o, rule.Source).Head.Map(static v => v.Render())));
        var duplicated = toHashSet(keyed.Choose(static r => r.Key)
            .GroupBy(static k => k).Where(static g => g.Count() > 1).Select(static g => g.Key));
        var violated = keyed.Filter(r => r.Key.Match(Some: duplicated.Contains, None: static () => true)).Map(static r => r.Id);
        var pass = keyed.Filter(r => r.Key.Match(Some: k => !duplicated.Contains(k), None: static () => false)).Map(static r => r.Id);
        return new RuleVerdict(rule, pass, violated);
    }

    // Graph-incidence verdict: every applicable element must reach a Target-matching element through its
    // owning DistributionSystem's Mode-oriented flow graph — the connectivity model-check (every terminal traces
    // upstream to an air-handler, every fixture to a water source) the per-element/count/uniqueness modalities
    // cannot express. Composes the settled Model/systems#SYSTEM_TRACE fold over DistributionNetwork.View and the
    // ONE selection algebra for Target; an element in no system, or whose trace reaches no target, violates — the
    // orphaned terminal IS the defect the rule surfaces (a target element passes by self-reach).
    static RuleVerdict Traced(CoordinationRule.Reachable rule, ElementSet applicable, Seq<(DistributionSystem View, LanguageExt.HashSet<NodeId> Members)> systems) {
        LanguageExt.HashSet<NodeId> targets = ElementSet.Query(applicable.Graph, rule.Target).Ids;
        (Seq<NodeId> pass, Seq<NodeId> violated) = Ids(applicable).Partition(id => systems
            .Filter(entry => entry.Members.Contains(id))
            .Exists(entry => SystemTrace.From(entry.View, id, rule.Mode).ReachedElements.Exists(targets.Contains)));
        return new RuleVerdict(rule, pass, violated);
    }

    static Seq<NodeId> Ids(ElementSet set) => set.Objects.Map(static o => o.Id);

    // Proposal: the lower-priority discipline's element yields and ITS row's delegate names the fix — the whole
    // heuristic is policy data, so a re-tuned trade response is a row edit and never an arm here. The deficit
    // admits ONCE as a seam Length measure through the one dimensioned-scalar gate, so every published fix carries
    // its quantity type and dimension and a non-finite proximity result rails instead of reaching a BCF topic.
    static Fin<(NodeId Yields, Resolution Fix)> ResolveOf(Interference clash, CoordinationPolicy policy) =>
        MeasureValue.OfSi(QuantityType.Length, Dimension.LengthDim, clash.Deficit)
            .Map(deficit => policy.SecondYields(clash.FirstDomain, clash.SecondDomain)
                ? (clash.Second, policy.Propose(clash.Kind, clash.SecondDomain, deficit))
                : (clash.First, policy.Propose(clash.Kind, clash.FirstDomain, deficit)));

    // The verification a GrantClearance IMPLIES: an accepted exception waives the clearance envelope around the
    // YIELDING member, so the grant is only readable beside whatever that envelope still reaches. The read is the
    // systems-owned clearance-ring modality — InterferenceCheck.Neighborhood over the RETAINED ClashIndex spatial
    // hash the interference fold already seated — never a second proximity test here, exactly as the proposal fold
    // consumes ranked Interference evidence rather than re-deriving it. The granted pair drops out, so an empty
    // answer IS a clean waiver and a non-empty one names every further element the coordinator is accepting a graze
    // against. Only the GrantClearance arm reads: the other four fixes move or bore geometry rather than waiving an
    // envelope, and their verification is the next Run.
    public static Seq<NodeId> Cleared(ClashIndex index, ClashProposalRow row) =>
        row.Proposed is Resolution.GrantClearance
            ? Volume(index, row.Yields)
                .Map(volume => InterferenceCheck.Neighborhood(index, volume)
                    .Map(static member => member.Id)
                    .Filter(id => id != row.First && id != row.Second))
                .IfNone(Seq<NodeId>())
            : Seq<NodeId>();

    // The yielding member's seated volume off the ONE SwiftBucket handle registry both structures index on — the
    // registry owns the key space, so no NodeId map sits beside it and an unseated member answers None rather than
    // a zero box the ring would then query as a point.
    static Option<BoundVolume> Volume(ClashIndex index, NodeId member) =>
        toSeq(index.Registry).Find(entry => entry.Member.Id == member).Map(static entry => entry.Bounds);

    // Write-time IDS<->BCF seam (the read-time seam is the shared ElementPredicate algebra the facet fold lowers
    // onto): one BcfTopic per NON-conforming IdsAudit, keyed on the audit's document ORDINAL so a re-audit re-lands
    // the SAME topic — IDS v1.0 specification NAMES are not unique, so a name key merges two specifications' topics
    // and the second's failures vanish from the board; the ordinal is the identity IdsAudit.Reconcile and
    // BimEvent.VerdictIssued already join on, the name carried in the title as name#ordinal for a reader. One
    // BcfComment per failing facet naming the FacetKey and its failed count; the viewpoint anchors every failed
    // GlobalId (already the IFC axis, no NodeId projection needed) and carries NO camera — a selection-only
    // viewpoint is legal BCF and a zero-filled Perspective publishes a degenerate view as authored intent.
    public static Seq<BcfTopic> Raise(Seq<IdsAudit> audits, string author, Instant at) =>
        audits.Filter(static a => !a.Conforms).Map(a => new BcfTopic(
            $"ids-{a.Spec}", $"IDS non-conformance: {a.Specification}#{a.Spec}",
            BcfStatus.Open, "IDS", "Normal", author, at,
            a.Verdicts.Filter(static v => !v.Failed.IsEmpty).Map(v => new BcfComment(
                $"ids-{a.Spec}-{v.Key.Value:X32}", author,
                $"{v.Key.Value:X32}: {v.Failed.Count} failed", Option<string>.None, at)),
            Seq(new BcfViewpoint($"vp-ids-{a.Spec}", Option<BcfCamera>.None,
                a.Verdicts.Bind(static v => v.Failed).Distinct().ToSeq(), Seq<string>(), Option<ReadOnlyMemory<byte>>.None))));

    // Clash topic: a BcfTopic over the resolved IFC ExternalId pair, the cross-discipline clash carrying High
    // priority, the viewpoint SelectedGlobalIds the two clashing elements so the proposed fix and the BCF issue
    // carry one element identity; keyed on the clash content identity so a re-proposal of the same clash is stable.
    // BOTH endpoints must resolve: BCF SelectedGlobalIds are IFC GlobalIds, so a half-anchored topic points a
    // receiving tool at one element and a token it cannot resolve, and a topic naming neither has nothing to show.
    // The viewpoint carries NO camera — the selection IS the anchor a coordination viewer frames itself.
    static Option<BcfTopic> TopicOf(ElementGraph graph, Interference clash, string author, Instant at) =>
        from first in ExternalOf(graph, clash.First)
        from second in ExternalOf(graph, clash.Second)
        select new BcfTopic(
            $"clash-{clash.Identity:X32}", $"{clash.Kind.Key} clash: {first} / {second}",
            BcfStatus.Open, "Clash", clash.CrossDiscipline ? "High" : "Normal", author, at,
            Seq<BcfComment>(),
            Seq(new BcfViewpoint($"vp-{clash.Identity:X32}", Option<BcfCamera>.None,
                Seq(first, second), Seq<string>(), Option<ReadOnlyMemory<byte>>.None)));

    // ONE parameterized MULTI-SOURCE distance kernel: every seed plus every transitive successor over the edge
    // rows, each carrying its HOP distance from the NEAREST seed — the schedule SequenceRel DAG (a slipped task
    // delays every transitive successor) and the CostItem.ParentGlobalId roll-up tree (a repriced line stales
    // every ancestor) are two edge-row inputs to one QuikGraph breadth-first walk. The graph folds ONCE and a
    // synthetic Source vertex edges to every seed, so a single Compute discovers the whole frontier at level one
    // and the nearest-seed distance falls out of level-order discovery itself — the per-seed Compute loop with its
    // Math.Min merge re-walked the shared closure once per seed for an answer one walk already holds. The recorded
    // depth shifts down one so the seeds seat at zero, which also MEASURES a seed participating in no edge (an
    // unsequenced task, a top-level cost line with no parent): the observer attaches to ITreeBuilderAlgorithm and
    // records off TREE EDGES alone, so such a seed is measured only because the synthetic edge reaches it, and it
    // is the row the severity bands rank HIGHEST. The recorder rides its own Attach IDisposable scope under a unit
    // edge weight, so the relaxer counts hops; the synthetic vertex is dropped from the projection.
    static Seq<ImpactRow> Downstream(Seq<(string From, string To)> edges, Seq<string> seeds, CoordinationPolicy policy) {
        // The synthetic vertex leads with the NUL escape: every real key on these legs is an IFC GlobalId or a
        // schedule/cost identifier, none of which can carry a control character, so the sentinel cannot collide.
        const string Source = "\0multi-source";
        var dag = new AdjacencyGraph<string, SEdge<string>>();
        foreach (var (from, to) in edges) { dag.AddVerticesAndEdge(new SEdge<string>(from, to)); }
        foreach (string seed in seeds) { dag.AddVerticesAndEdge(new SEdge<string>(Source, seed)); }
        BreadthFirstSearchAlgorithm<string, SEdge<string>> bfs = new(dag);
        VertexDistanceRecorderObserver<string, SEdge<string>> distances = new(static _ => 1.0);
        using (distances.Attach(bfs)) { bfs.Compute(Source); }
        // The ordered run re-enters the carrier through Prelude.toSeq, because OrderBy answers an
        // IOrderedEnumerable the K-rail ToSeq binds nothing of.
        return toSeq(distances.Distances
            .Where(entry => !string.Equals(entry.Key, Source, StringComparison.Ordinal))
            .Select(entry => new ImpactRow(entry.Key, (int)entry.Value - 1, policy.Band((int)entry.Value - 1)))
            .OrderBy(static row => row.Hops));
    }

    // IFC GlobalId the BCF viewpoint anchors on — the Bim-stored Object ExternalId [H6]. An element with no
    // ExternalId is OUTSIDE the IFC exchange, so the absence stays typed: leaking the neutral NodeId into a
    // GlobalId slot publishes a viewpoint whose selection no receiving tool can resolve, and the consumer that
    // reads it back cannot tell the forged token from a real GlobalId.
    static Option<string> ExternalOf(ElementGraph graph, NodeId id) =>
        graph.Find<Node.Object>(id).Bind(static o => o.ExternalId);
}
```

## [03]-[SIGN_OFF]

- Owner: `SignOff` the `[SmartEnum<string>]` state machine over the `Review/issues#BCF_ARCHIVE` `BcfStatus` lifecycle — each case carrying its legal forward `SignOff` set as per-case delegate data (the transition table folded onto the generated case family, never a parallel `FrozenDictionary<BcfStatus,…>` the instances mirror) so the SmartEnum IS the dispatch surface and a governed workflow advances an issue through `Open → InProgress → Resolved → Closed` (with the `Reopened` re-entry) under a compile-addressable lifecycle the AppUi board references by `SignOff.Resolved`; `BcfStatus` stays the wire serialization value the `Review/issues#TS_PROJECTION` projects, `SignOff` the host-neutral transition owner over it; `IssueBoard` the host-neutral board fold over the `BcfTopic` family the `Rasm.AppUi/Collab/issues` relocation grounds here — the status lanes, the vocabulary-ranked priority ordering, and the viewpoint-anchored element selection the desktop and any future head project over one contract.
- Entry: `SignOff.Advance` is the one polymorphic transition entrypoint discriminating on input shape — the instance `state.Advance(SignOff to, Op key)` reads the case's own legal forward set, the wire overload `SignOff.Advance(BcfStatus from, BcfStatus to, Op key)` resolving each value through `SignOff.Of` first so a caller holding a wire `BcfStatus` transitions through the same owner — `Fin<T>` aborting an illegal transition (`Model/faults#FAULT_BAND` `BimFault.ModelRejected`) lifted BARE, so a `Closed → InProgress` skip is rejected while a `Closed → Reopened` re-entry is admitted; `IssueBoard.Of(Seq<BcfTopic> topics, Seq<string> priorities)` folds the topic set into the board projection (the status partition, the vocabulary-ranked priority ordering, the viewpoint-anchored selection) the AppUi head materializes, `priorities` the ingested `Review/issues#BCF_ARCHIVE` `BcfVocabulary.Priorities` roster, and `IssueBoard.Anchor(params ReadOnlySpan<ClashProposalRow>)` binds one proposal or a whole `Propose` run to its `BcfTopic`s in ONE re-partition — the span absorbing the single and the batch arity — so the proposed fixes land on the board.
- Auto: `Advance` reads the state's `Forward()` legal set off the case data and admits `to` only when present (`Open`→`{InProgress, Closed}`, `InProgress`→`{Resolved, Open}`, `Resolved`→`{Closed, Reopened}`, `Closed`→`{Reopened}`, `Reopened`→`{InProgress, Closed}`), each forward set a `static () => Seq(…)` delegate the `[UseDelegateFromConstructor]` generated `Forward()` binds (the self-referential forward references resolved lazily, no Func-property-plus-wrapper pair beside the case data) so a new lifecycle state is one `SignOff` case carrying its own forward set, never a second table to keep in sync; `SignOff.Of` resolves the wire `BcfStatus` to its case total (the union is complete over the enum — one case per status); `IssueBoard.Of` partitions the topic set by `BcfStatus`, orders within each partition by the `PriorityRank` INDEX into the archive's own declared priority roster (buildingSMART declares it most-urgent-first, so the index IS the rank and a project's renamed bands order correctly), falling back to the frozen canonical roster only where the archive declares no vocabulary — never the lexical string, which reads `"High" < "Low" < "Normal"` and inverts the real urgency, and projects each topic's `BcfViewpoint.SelectedGlobalIds` onto the element selection the AppUi board highlights — the AppUi `Collab/issues` consuming the `BcfTopic` contract at the package edge as a board projection and never re-minting a BCF schema.
- Receipt: the `SignOff` lifecycle is the governed sign-off workflow the `ClashProposalRow` anchors its proposed fix to (the proposal's `BcfTopic` advancing through the lifecycle as the clash is resolved) and the `Review/versioning#VERSION_GRAPH` `MergeConflict` resolution advances through; the `IssueBoard` projection the single BCF issue-board domain the desktop and any future head project over one contract; the AppUi keeps only the board projection, the durable op-log/CDE-sync store stays the `Rasm.Persistence/Version/ledger` concern joined by `ExternalId`, and Bim owns the issue-board domain over the `BcfTopic` contract.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm
- Growth: a new lifecycle state is one `SignOff` case carrying its own forward set (no second transition table to widen); a new board partition is one fold over the same `BcfTopic` set; a new priority band is one row on the archive's own declared vocabulary and needs no page edit at all; the `ClashProposalRow`-to-`BcfTopic` anchor folds the proposals' topics onto the board through the one `Of` partition (never a second sort, never a per-proposal re-partition); never a per-state class, never a second BCF schema, and never a board-side issue store.
- Boundary: `Rasm.Bim/coordination` owns the issue-board DOMAIN over the `Review/issues#BCF_ARCHIVE` `BcfTopic` contract and `Rasm.AppUi/Collab/issues` owns only the board projection — the AppUi head re-mints no BCF schema and reads the domain at the package edge, the `[ISSUES_RELOCATE_TO_BIM]` relocation leaving `SignOff` on a settled Bim owner; `Rasm.Persistence/Version/ledger` owns the durable op-log/CDE-sync store joined by the `Node.Object` `ExternalId` content-key, the `[ANNOTATION_RELOCATE_TO_BIM]` relocation leaving the BCF record family, the `BcfApi` server dialect, AND the `.bcfzip` container wire in `Rasm.Bim` — the `Review/issues#BCF_ARCHIVE` `BcfArchive` is the branch's one custodian, `Rasm.Persistence/Ingest/issue` holding the durable rows (`IssueOp.Egress` releasing them for the root's custodian write, `IssueRows.Reconcile` partitioning the returned cycle) — neither side re-mints the BCF schema across the boundary, joining only on the `ExternalId` the durable annotation row carries; the `SignOff` legal transitions are per-case data on the SmartEnum (the case's own `Forward` set) and a parallel `FrozenDictionary<BcfStatus,…>` the instances merely mirror, a per-transition method, or an unchecked status setter is the deleted form — the `[SmartEnum]` is the dispatch surface so `Advance` is its operation and an illegal advance lifts `BimFault.ModelRejected` BARE (no `.ToError()`); the `IssueBoard` reads the one `BcfTopic` family and ranks by the archive's own `BcfVocabulary.Priorities` order, so a frozen canonical rank table as the PRIMARY authority is the deleted form (it reads a project's own `"P1"`/`"P2"` bands as unknown and flattens the board) and survives only as the no-vocabulary fallback; a lexical priority sort or a parallel board-side issue record are the deleted forms.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using Rasm.Bim.Model;
using Thinktecture;
using Op = Rasm.Domain.Op;
using static LanguageExt.Prelude;

namespace Rasm.Bim.Coordination;

// --- [MODELS] -----------------------------------------------------------------------------
// Lifecycle state machine over the wire BcfStatus: each state carries its legal forward set as per-case
// delegate data (the transition table folded onto the generated case family, never a parallel
// FrozenDictionary<BcfStatus,...> the instances mirror), so the SmartEnum IS the dispatch surface and Advance is
// its instance operation. [UseDelegateFromConstructor] binds the generated Forward() to the new(...) delegate arg
// in declaration order (key, Status, forward); the static () => Seq(...) form resolves the self-referential
// forward cases lazily. BcfStatus stays the wire value, SignOff owns the transitions.
[SmartEnum<string>]
public sealed partial class SignOff {
    public static readonly SignOff Open       = new("open",        BcfStatus.Open,       static () => Seq(InProgress, Closed));
    public static readonly SignOff InProgress = new("in-progress", BcfStatus.InProgress, static () => Seq(Resolved, Open));
    public static readonly SignOff Resolved   = new("resolved",    BcfStatus.Resolved,   static () => Seq(Closed, Reopened));
    public static readonly SignOff Closed     = new("closed",      BcfStatus.Closed,     static () => Seq(Reopened));
    public static readonly SignOff Reopened   = new("reopened",    BcfStatus.Reopened,   static () => Seq(InProgress, Closed));

    public BcfStatus Status { get; }

    [UseDelegateFromConstructor]
    public partial Seq<SignOff> Forward();

    // Terminal == no legal forward transition (Forward().IsEmpty), the honest derivation: the BCF lifecycle is fully
    // re-enterable (Closed itself can Reopen) so this is invariantly false today and a `== Closed` form would lie.
    public bool IsTerminal => Forward().IsEmpty;

    // Every BcfStatus maps to exactly one SignOff case (the union is complete over the wire enum), so the resolve
    // is total and never misses — an out-of-roster status degrades to Open through the IfNone fallback (the same
    // default the Review/issues#BCF_ARCHIVE codec lands an unparseable status on), never a throwing First in domain logic.
    public static SignOff Of(BcfStatus status) => toSeq(Items).Find(s => s.Status == status).IfNone(Open);

    // ONE polymorphic transition entrypoint discriminating on input shape: the SmartEnum state advances itself, the
    // wire BcfStatus overload resolves through Of first — a Closed->InProgress skip faults while a
    // Closed->Reopened re-entry is admitted, the legal set read off the case data, never a table lookup. The
    // illegal transition lifts Model/faults#FAULT_BAND BimFault.ModelRejected BARE (Expected-derived, no .ToError()).
    public Fin<SignOff> Advance(SignOff to, Op key) =>
        Forward().Contains(to)
            ? Fin.Succ(to)
            : Fin.Fail<SignOff>(new BimFault.ModelRejected(key, $"signoff-illegal-transition:{Status}->{to.Status}"));

    public static Fin<BcfStatus> Advance(BcfStatus from, BcfStatus to, Op key) =>
        Of(from).Advance(Of(to), key).Map(static s => s.Status);
}

// The board carries the archive's OWN priority vocabulary beside its lanes: BCF priority is a project extension
// roster whose DECLARED ORDER is the project's urgency order, so the ingested BcfVocabulary.Priorities is the
// ranking authority and the frozen canonical roster is the fallback for an archive that declares none.
public sealed record IssueBoard(Map<BcfStatus, Seq<BcfTopic>> Lanes, Seq<string> Priorities) {
    public static IssueBoard Of(Seq<BcfTopic> topics, Seq<string> priorities) =>
        new(topics
            .GroupBy(static t => t.Status)
            .Select(g => (g.Key, toSeq(g.OrderBy(t => PriorityRank.Of(priorities, t.Priority)))))
            .ToMap(), priorities);

    // Land clash proposals on the board: every proposal's minted BcfTopic folds into its status lane through ONE Of
    // re-partition — the span absorbing the single and the whole-Propose-run arity, never a per-proposal re-sort —
    // so the partition + PriorityRank ordering stay the ONE owner. Anchoring is IDEMPOTENT by topic Guid: a
    // re-proposal of the same clash (the clash-content-keyed topic id is stable) REPLACES its stale board copy,
    // never duplicates it. The element selection a viewer highlights is each topic's own
    // BcfViewpoint.SelectedGlobalIds — never re-extracted here, this owner INTEGRATES the proposals. A proposal
    // whose clash pair carries no IFC identity yet mints no topic and simply contributes no lane row.
    public IssueBoard Anchor(params ReadOnlySpan<ClashProposalRow> proposals) {
        var incoming = toSeq(Iterable<ClashProposalRow>.FromSpan(proposals).Choose(static p => p.Topic));
        var replaced = toHashSet(incoming.Map(static t => t.Guid));
        return Of(Lanes.Values.ToSeq().Bind(static lane => lane).Filter(t => !replaced.Contains(t.Guid)) + incoming, Priorities);
    }
}

// --- [POLICIES] ---------------------------------------------------------------------------
// Board ordering rank: a BCF Priority is an extension-defined free string, but a board orders by SEMANTIC urgency,
// never alphabetically ("High" < "Low" < "Normal" lexically inverts the real order). The archive's OWN
// BcfVocabulary.Priorities roster is the authority — buildingSMART declares it MOST-URGENT-FIRST, so the row's
// INDEX is its rank and a project that renames its bands ("P1"/"P2"/"P3") orders correctly with no table edit,
// where a frozen canonical table reads every one of them as unknown and flattens the board. The literal roster is
// the no-vocabulary fallback alone; an out-of-roster priority sorts after the declared bands, never throwing.
static class PriorityRank {
    static readonly FrozenDictionary<string, int> Fallback = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase) {
        ["Critical"] = 0, ["High"] = 1, ["Major"] = 1, ["Normal"] = 2, ["Medium"] = 2, ["Low"] = 3, ["Minor"] = 3,
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    // The indexed Choose on SeqExtensions takes the INDEX FIRST (the instance Map's (value, index) order inverts
    // here), so the ordinal binds the first slot; Head.IfNone lands an out-of-roster priority after every declared band.
    public static int Of(Seq<string> vocabulary, string priority) =>
        vocabulary.IsEmpty
            ? Fallback.GetValueOrDefault(priority, int.MaxValue)
            : vocabulary.Choose((index, row) => string.Equals(row, priority, StringComparison.OrdinalIgnoreCase) ? Some(index) : None)
                .Head.IfNone(int.MaxValue);
}
```

## [04]-[RESEARCH]

(none)
