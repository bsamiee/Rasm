# [BIM_COORDINATION]

`Rasm.Bim/coordination` owns model-checking and coordination over the shared `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph`: the if-X-then-Y rule engine, the clash-resolution proposal fold, the A/B model-impact report, the IDS-audit board handoff, the BCF sign-off state machine, and the host-neutral BCF issue-board the `Rasm.Persistence/Version/ledger` and `Rasm.AppUi/Collab/issues` relocations settle here. Every workflow composes a settled vocabulary the IDS/BCF/Diff owners supply but never assemble — the `Model/query#ELEMENT_SET` `BimTerm` algebra, the `Model/systems#INTERFERENCE` `Interference` ranked clash evidence, the `Model/systems#CONNECTIVITY`/`#SYSTEM_TRACE` `DistributionNetwork.View` flow views and `SystemTrace` fold, the `Review/diff#MODEL_DIFF` change-sets, the `Review/issues#BCF_ARCHIVE` `BcfTopic` family, and the `Planning/schedule#SCHEDULE`/`Planning/cost#ESTIMATE` joins — re-deriving none: no second predicate surface, no re-run proximity test, no second reachability walk, no re-computed diff.

Identity follows the contract law [H6]: a kernel verdict keys on the neutral `Rasm.Element/Graph/element#NODE_MODEL` `NodeId` — the COMPLETE identity present on every node, so a `RuleVerdict` over the WORKING graph reports an authored element carrying no IFC `GlobalId` yet — while an IFC-semantic verdict keys on the IFC `ExternalId` because its join targets (`Review/issues#BCF_ARCHIVE` viewpoints, `Planning/schedule#SCHEDULE` `TaskAssignment.ElementGlobalIds`, `Planning/cost#ESTIMATE` `CostItem.PricedGlobalIds`) are themselves GlobalId-keyed; the `NodeId → ExternalId` projection happens at the boundary through the Bim-stored `Node.Object.ExternalId`. This owner also holds the BCF issue-board DOMAIN over the `Review/issues#BCF_ARCHIVE` `BcfTopic`/`BcfComment`/`BcfViewpoint` family and the `BcfApi` server dialect, while `Rasm.Persistence/Version/ledger` keeps the durable op-log/CDE-sync store and `Rasm.AppUi/Collab/issues` keeps only the board projection — the three joined by the `ExternalId` content-key, never a second BCF schema across the boundary. FILE-WIRE legs cross as `.bcfzip` BYTES through the branch's ONE container custodian: `Run`/`Raise`-minted topics LEAVE through the `BcfArchive` codec this package owns and a foreign tool's resolved topics RETURN as the status moves the `SignOff` lifecycle consumes, while `Rasm.Persistence/Ingest/issue` holds the durable `IssueTopic` rows the composition root transcribes under the `BcfTopic`⇄`IssueTopic` correspondence law. Behaviour a composition tunes — the discipline yield hierarchy, the ripple severity bands, the clash-test library — arrives as `CoordinationPolicy` VALUES the `Semantics/classification#CLASSIFICATION_AXIS` `BsddPins` precedent shapes, never as durable roster edits. Every coordination rejection lifts the `Model/faults#FAULT_BAND` `BimFault` band BARE (the `Fault`-derived case IS the `Error`, no `.ToError()` hop). Coordination is HOST-LOCAL.

## [01]-[INDEX]

- [02]-[COORDINATION]: the `CoordinationRule` `[Union]` (the five model-check modalities — `Require`/`Prohibit`/`Cardinality`/`Unique`/`Reachable`, severity the abstract axis), the `Resolution` `[Union]`, the `RuleVerdict` `NodeId` partition, the `ClashTest` predicate-pair test and its `ClashReport`/`ClashProposalRow` lifecycle-columned run report, the composition-supplied `CoordinationPolicy` (`DisciplineRule` yield rows + `ImpactSeverity` bands), the `ImpactReport`/`ImpactRow` hop-measured impact closure, the `FederatedModel` union ingress over the shared `Federate`, and the `Coordination` fold owner (`Federate`/`Check`/`Run`/`Cleared`/`Between`/`Raise`) over the shared `ElementGraph`.
- [03]-[SIGN_OFF]: the `SignOff` `[SmartEnum<string>]` lifecycle over the `Review/issues#BCF_ARCHIVE` `BcfStatus`, and the `IssueBoard` host-neutral board projection (lanes plus the archive's declared priority roster) the `Rasm.AppUi/Collab/issues` relocation grounds here.

## [02]-[COORDINATION]

- Owner: `CoordinationRule` the closed `[Union]` of the five model-check modalities, each carrying an applicability `Model/query#ELEMENT_SET` `BimTerm` (the X — the SAME selection surface the `Review/validation#IDS_FACETS` IDS facet fold reads) and its modality-specific requirement (the Y), with `Applicability` and `Severity` the abstract members every arm overrides positionally; `Resolution` the closed `[Union]` of proposed clash fixes; `RuleVerdict` the per-rule verdict folding the applicable set into the passing and the violating `NodeId` partition; `ClashTest` the named coordination test — the discipline predicate PAIR, the shared `MeasureValue` tolerance, and the `ClashKind` the pair is tested under — whose `Run` answers a `ClashReport` of `ClashProposalRow`s grouped under the test identity and DIFFED against the prior run, so a coordinator reads a clash matrix's cells rather than one undifferentiated interference stream; `ClashProposalRow` the proposed fix over one `Model/systems#INTERFERENCE` `Interference` — the clashing `NodeId` pair, the yielding endpoint, the ranked `Resolution` carrying its deficit as a shared `MeasureValue`, the `ClashState` lifecycle column, and the `Option<BcfTopic>` anchor an IFC-visible pair earns; `CoordinationPolicy` the ONE composition-supplied behaviour value (the `DisciplineRule` yield/fix rows and the `ImpactSeverity` hop bands, `Default` serving an unconfigured root) the `Semantics/classification#CLASSIFICATION_AXIS` `BsddPins` precedent shapes; `ImpactReport` the A/B fold over two `Review/diff#MODEL_DIFF` change-sets into the contested seed and the transitively downstream-affected element/task/cost-line/system closures, each rippled row an `ImpactRow` carrying its hop distance and severity band; `FederatedModel` the union ingress carrying the one federated graph beside the shared `FederationCensus` provenance rows and the geodetic `FrameAlignment` matrix the union was gated on; `Coordination` the static fold owner (`Federate`/`Check`/`Run`/`Cleared`/`Between`/`Raise`) collapsing the prior `CoordinationCheck`/`ClashProposal`/`ChangeImpact` triplet into one deep coordination domain owner and carrying the write-time IDS-audit-to-board handoff.
- Cases: `CoordinationRule` arms `Require` (`BimTerm Applicability`/`Requirement`, `RuleSeverity`) · `Prohibit` (same shape, the IDS-mirrored declarative polarity) · `Cardinality` (`Applicability`, `int Min`, `Option<int> Max`, `RuleSeverity` — the applicable-set count must lie in `[Min, Max]`) · `Unique` (`Applicability`, `ValueSource Source`, `RuleSeverity` — every applicable element's source value distinct, the source a direct `ObjectAttribute` OR an effective Pset/Qto property read through `ElementQuery.ValuesOf`) · `Reachable` (`Applicability`, `BimTerm Target`, `TraceMode Mode`, `RuleSeverity` — every applicable element must reach a `Target`-matching element through its owning `DistributionSystem`'s `Mode`-oriented flow graph) (5) — a per-element predicate, a set-count bound, an attribute uniqueness, and a graph-reachability incidence are the four irreducible model-check shapes, each one arm reusing the one selection algebra, never a per-rule-kind class and never a second predicate surface; an advisory check is `RuleSeverity.Info` on any arm — the retired `Recommend` arm was `Require`@`Info` spelled as a modality; `Resolution` arms `Reroute` (a suggested centerline offset for a linear MEP run) · `Resize` (a suggested dimension reduction for a discrete element) · `GrantClearance` (an accepted clearance exception) · `Sleeve` (a framed penetration bore through the prevailing element) · `Reject` (no fix — the clash stands for a coordinator's manual review) (5), the four dimensioned arms carrying a shared `MeasureValue` and a bare `double` offset being the deleted form; every arm has a producer on the `CoordinationPolicy` discipline rows — `Sleeve` where a rigid Structural or Architectural element prevails, `Reject` where neither side moves on a fold's authority; `ClashState` rows `New` (absent from the prior run) · `Active` (present in both) · `Resolved` (present in the prior run alone, carried forward with its prior fix so the row survives its own disappearance) (3), keyed by the `ClashProposalRow` pair-and-test identity so a re-run neither re-opens a settled cell nor silently drops a fixed one.
- Entry: `Coordination.Federate(Seq<(string Model, ElementGraph Graph)> models, Header coordination, (double X, double Y, double Z) anchor, CancellationToken token)` is the FEDERATED ingress every multi-discipline cycle enters through — it runs the `Semantics/georeference#GEODETIC_TRANSFORM` `GeoTransform.Preflight` pairwise matrix at the anchor FIRST, passes an `Unresolvable` row's exact `Error` cause through, passes `Errors.Cancelled` through on abandonment, then unions through the shared `Rasm.Element/Graph/element#FEDERATION` `ElementGraph.Federate`, returning the `FederatedModel` whose union graph the folds below read; `Coordination.Check(ElementGraph graph, Seq<CoordinationRule> rules)` validates the rule library then folds each rule to a `RuleVerdict` over the element graph — `Fin<T>` aborting a malformed rule (a `Cardinality` bound `Min < 0` or `Max < Min`) onto `BimFault.Refused` with `BimReason.Rejected`, the well-formed fold itself total, and the `DistributionNetwork.View` flow decomposition built ONCE per check and threaded into every verdict; `Coordination.Run(ElementGraph graph, Seq<Interference> interferences, ClashTest test, Option<ClashReport> prior, CoordinationPolicy policy, string author, Instant at)` is the ONE proposal entry — a coordination run is always a NAMED test, so it scopes the interference evidence to the pair of discipline predicates under the test's tolerance and `ClashKind`, folds each clash onto its ranked `Resolution` and its BCF anchor, then stamps each row's `ClashState` by joining the run against `prior` on the pair-and-test identity, `Fin<T>` because the clash deficit admits through the ONE shared `MeasureValue.OfSi` gate under that operation and a pair carrying no IFC identity yields a proposal with no topic rather than a forged one; `Coordination.Cleared(ClashIndex index, ClashProposalRow row)` is the verification a `GrantClearance` implies — it reads the yielding member's seated `BoundVolume` off the `ClashIndex` handle registry and answers the `InterferenceCheck.Neighborhood(index, volume)` clearance ring minus the granted pair, total; `Coordination.Between(ElementGraph graph, ModelDiff before, ModelDiff after, ScheduleNetwork schedule, CostSchedule cost, CoordinationPolicy policy)` folds the two change-sets into the contested seed and its reachability closures — total, the flow leg composing the settled `Model/systems#SYSTEM_TRACE` fold, the schedule/cost legs one `QuikGraph` breadth-first distance walk; `Coordination.Raise(Seq<IdsAudit> audits, string author, Instant at)` folds every NON-conforming `Review/validation#IDS_FACETS` `IdsAudit` onto one `BcfTopic` keyed on the audit's document ORDINAL (`ids-{a.Spec}`, titled `name#ordinal` — the same ordinal-qualified identity the `Exchange/events#EVENT_PROJECTION` verdict announcement uses), one `BcfComment` per failing facet, the viewpoint anchoring every failed `GlobalId`.
- Auto: `Check` first runs each rule through `Validate` (`TraverseM` short-circuiting the whole library on the first malformed rule) then maps the well-formed library to verdicts; `Verdict` selects the applicable set ONCE through `ElementQuery.Query(graph, rule.Applicability)` and dispatches the generated total `Switch` — `Require`/`Prohibit` collapse to ONE `Predicated` partition derived by a `prohibits` policy bit (the matching subset re-folded over only the current members via `ElementQuery.Where`, never the retired O(n²) `Holds` re-query), `Cardinality` partitions on the applicable-set count (out of range the whole set is the violating evidence so the board highlights every element of a storey that breaches its exit count), `Unique` groups the applicable set by the `ValueSource` value (a duplicate-valued OR an unreadable-source member violates), and `Reachable` folds each applicable element through the settled `SystemTrace` closure — the check's ONE `DistributionNetwork.View` decomposition threaded as `Switch` state, intersected against the ONE-query `Target` set — so an element in no system or whose trace reaches no target violates; `Run` narrows the interference stream to the pairs whose two endpoints satisfy the test's `Left`/`Right` predicates in either orientation and whose deficit clears the test tolerance, reads each `Interference` (consuming the systems-page ranked clash evidence rather than re-deriving proximity), admits its deficit as a shared Length measure, elects the yielding endpoint through the threaded discipline ranks, reads the fix off that discipline's own row delegate under the `ClashKind` waiver, mints a `BcfTopic` per clash anchoring the resolved `ExternalId` pair, then LEFT-joins the prior report on the `(Test, First, Second)` identity so an unmatched incoming row stamps `New`, a matched one `Active`, and an unmatched PRIOR row carries forward stamped `Resolved`; `Between` intersects the two `ModelDiff` change-sets by IFC GlobalId into the contested seed, resolves the seed to `NodeId`s through the graph's `ExternalId` index, propagates each seed member downstream through its owning system's flow graph (`SystemTrace.From(system, seed, TraceMode.Downstream)`) and reduces those traces to the NEAREST-seed element-hop depth per IFC identity so every rippled element bands at its measured distance, joins the CLOSED affected set to the `ScheduleNetwork.Assignments` tasks naming it and the `CostSchedule.Items` lines pricing it, closes BOTH transitively through the one `Downstream` kernel — the `SequenceRel` predecessor→successor DAG and the `CostItem.ParentGlobalId` roll-up tree two edge-row inputs to ONE multi-source `QuikGraph` breadth-first walk, the graph folded once per leg with a synthetic source edged to every seed so a single `Compute` discovers the whole frontier and the attached `VertexDistanceRecorderObserver` measures every row's hop distance from its NEAREST seed by construction — and surfaces the `DistributionSystem`s whose member set the closure intersects.
- Output: the `Seq<RuleVerdict>` is the parameterized model-checking evidence on the `NodeId` axis the AppUi board highlights and Persistence stores; the `ClashReport` the per-test run report — the ranked rows with proposed fixes, each carrying its `ClashState` against the prior run and its BCF anchor on the IFC `ExternalId` axis a viewer round-trips — so a coordination cycle reports progress rather than a fresh undifferentiated stream each pass; the `ImpactReport` the transitive A/B change-impact closure on the IFC-GlobalId axis a 4D/5D federation reads; the `SignOff` lifecycle governs the issue, so a governed workflow — rule check → clash test run → impact report → BCF sign-off — reads one composed pipeline over the settled owners.
- Growth: a new model-check modality is one `CoordinationRule` union arm reusing the `BimTerm` algebra (the `Reachable` arm reuses the `TraceMode` rows as its orientation policy, never a second trace enum); a new clash fix is one `Resolution` arm plus the `CoordinationPolicy` discipline row that elects it; a new proposal heuristic is one row's delegate column and a re-tuned trade hierarchy, ripple threshold, or clash-test library one value a composition passes; a new severity band is one `RuleSeverity` row `Review/validation#IDS_FACETS` owns; a new clash-lifecycle state is one `ClashState` row the join stamps; a new post-fix verification is one read over the broad phase `Model/systems#INTERFERENCE` already retains (`Cleared` the standing exemplar), never a second index here; a new impact dimension is one column on `ImpactReport` with at most one edge-row input to the one `Downstream` distance kernel; never a per-rule-kind type, never a second selection surface, and never a re-derived proximity, reachability walk, or diff in this owner.
- Boundary: the rule applicability/requirement is the `Model/query#ELEMENT_SET` `BimTerm`, so the validation predicate IS the query predicate IS the coordination predicate — one selection surface across `Model/query#ELEMENT_SET`, `Review/validation#IDS_FACETS`, and this owner, a parallel `RuleSelector`/`CoordinationQuery` expression type the deleted form. The clash evidence is the `Model/systems#INTERFERENCE` `Interference` row carrying the `NodeId` pair — the systems page owns the geometric proximity, this owner consumes the ranked evidence and proposes the fix, so re-running the proximity test here is the named cross-page drift defect, and the clearance verification obeys the SAME law: `Cleared` composes the systems-owned `InterferenceCheck.Neighborhood` ring over the retained `ClashIndex`, so a coordination-local radius query or a hand-rolled distance sweep beside the seated broad phase is the deleted form. A COORDINATION RUN IS A NAMED TEST — `Run` is the ONE proposal entry and the retired unscoped `Propose` (whose rows carried an empty-string test sentinel and whose report had no lifecycle to join on) is GONE; the whole-stream fold it spelled is `ClashTest.Everything`, which carries a name and a run-over-run diff besides. The `ImpactReport` ripple and the `Reachable` verdict are REAL reachability — the flow leg composes the settled `SystemTrace.From` over the ONE graph-scoped `DistributionNetwork.View` decomposition (a per-rule rebuild is the deleted form) and the schedule/cost legs the one `Downstream` breadth-first distance kernel whose recorder attaches through the observer's own `IDisposable` scope, so a hand-rolled visited-set walk and a distance-blind closure that reads a fourth-order successor as a seed are the deleted forms; EVERY leg publishes DISTANCE — the flow leg reads the trace's own `TraceHop` element-hop column, so flattening a whole downstream closure to one propagation hop is the deleted form and re-tracing per level to recover the depth the walk already measured is the other; the two legs merge multiple seeds under DIFFERENT laws because their walks differ — the schedule/cost distance kernel is ONE multi-source walk over ONE graph, a synthetic source edged to every seed making the whole seed set the frontier at depth one, so a per-seed `Compute` loop with its `Math.Min` merge is the deleted form there, while the flow leg's nearest-seed `Math.Min` fold is the LAWFUL reduction because each trace is a single-source walk over its own system view and the trace owner stays single-source for `Runs`' one-seed route contract; a post-hoc seeding of the distance map is deleted on both legs, because `VertexDistanceRecorderObserver` records off TREE EDGES alone and an unsequenced task or parentless cost line reached by no edge drops the row the severity bands rank HIGHEST. A `Resolution` carries a shared `MeasureValue` and a bare `double` deficit crossing to a BCF topic or a fabrication consumer is the deleted form; a BCF viewpoint anchors ONLY on a real IFC `ExternalId` (the `IfNone(NodeId)` fallback that leaked a neutral key into a `SelectedGlobalIds` slot is the deleted form) and carries `Option<BcfCamera>` absence, because a selection-only viewpoint is legal BCF while a degenerate origin camera publishes a black frame as authored intent. A `RuleVerdict` keys on the COMPLETE `NodeId` identity while the BCF anchor and the impact join key on the IFC `ExternalId` their targets demand. The yield hierarchy, the fix delegates, and the ripple bands are `CoordinationPolicy` VALUES a composition supplies (`Default` serving an unconfigured root) and a durable `static class DisciplinePriority`/`ImpactSeverity` table is the deleted form — a project's trade hierarchy is project data. An IDS-raised topic keys on the audit's document ORDINAL and a specification-NAME key is the deleted form. The multi-model union is the CONTRACT's `ElementGraph.Federate` — it owns the coordination-header refusal axes and the id-collision-versus-dedup discrimination, so a coordination-local graph merge or a re-decided id rename beside that entry is the deleted form, and the geodetic `GeoTransform.Preflight` matrix gates the union rather than reconciling frames. The `CoordinationRule`/`Resolution`/`ClashState` unions are closed families; the coordination operations live on the ONE `Coordination` owner (the prior `CoordinationCheck`/`ClashProposal`/`ChangeImpact` single-method classes collapsed), and a rejection raises its `Model/faults#FAULT_BAND` `BimFault.Refused` value carrying its closed scope and reason and lifts `BimFault` BARE.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using LanguageExt;
// Contracts are retired from this logic.
using Rasm.Bim.Model;
using NodaTime;
using QuikGraph;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.Observers;
using QuikGraph.Algorithms.Search;
using Rasm.Element.Graph;
using Rasm.Element.Properties;
using Rasm.Element.Query;
using SwiftCollections.Query;
using Thinktecture;
using static LanguageExt.Prelude;
using BimTerm = Rasm.Element.Query.Predicate<Rasm.Bim.Model.BimLeaf>;

namespace Rasm.Bim.Coordination;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record CoordinationRule {
    private CoordinationRule() { }

    public abstract BimTerm Applicability { get; }
    public abstract RuleSeverity Severity { get; }

    public sealed record Require(BimTerm Applicability, BimTerm Requirement, RuleSeverity Severity) : CoordinationRule;
    public sealed record Prohibit(BimTerm Applicability, BimTerm Requirement, RuleSeverity Severity) : CoordinationRule;
    public sealed record Cardinality(BimTerm Applicability, int Min, Option<int> Max, RuleSeverity Severity) : CoordinationRule;
    public sealed record Unique(BimTerm Applicability, ValueSource Source, RuleSeverity Severity) : CoordinationRule;
    public sealed record Reachable(BimTerm Applicability, BimTerm Target, TraceMode Mode, RuleSeverity Severity) : CoordinationRule;
}

[Union]
public abstract partial record Resolution {
    private Resolution() { }

    public sealed record Reroute(MeasureValue Offset) : Resolution;
    public sealed record Resize(MeasureValue Dimension) : Resolution;
    public sealed record GrantClearance(MeasureValue Accepted) : Resolution;
    public sealed record Sleeve(MeasureValue Bore) : Resolution;
    public sealed record Reject : Resolution;
}

public sealed record RuleVerdict(CoordinationRule Rule, Seq<NodeId> Passed, Seq<NodeId> Violated) {
    public RuleSeverity Severity => Rule.Severity;
    public bool Conforms => Violated.IsEmpty;
    public bool Blocking => !Conforms && Severity.Blocking;
}

[SmartEnum<string>]
public sealed partial class ClashState {
    public static readonly ClashState New = new("new");
    public static readonly ClashState Active = new("active");
    public static readonly ClashState Resolved = new("resolved");
}

public sealed record ClashTest(string Name, BimTerm Left, BimTerm Right, MeasureValue Tolerance, ClashKind Kind) {
    public static ClashTest Everything(string name, MeasureValue tolerance, ClashKind kind) =>
        new(name, BimTerm.Open, BimTerm.Open, tolerance, kind);
}

public sealed record ClashProposalRow(
    string Test, NodeId First, NodeId Second, NodeId Yields, ClashKind Kind,
    Resolution Proposed, ClashState State, Option<BcfTopic> Topic);

public sealed record ClashReport(ClashTest Test, Seq<ClashProposalRow> Rows, Instant At) {
    public Seq<ClashProposalRow> Outstanding => Rows.Filter(static row => row.State != ClashState.Resolved);
    public bool Clear => Outstanding.IsEmpty;
}

public readonly record struct ImpactRow(string GlobalId, int Hops, RuleSeverity Severity);

public sealed record ImpactReport(
    Seq<string> Contested,
    Seq<ImpactRow> Elements,
    Seq<ImpactRow> Tasks,
    Seq<ImpactRow> CostLines,
    Seq<string> Systems);

public sealed record FederatedModel(ElementGraph Graph, FederationCensus Federation, Seq<FrameAlignment> Alignment);

// --- [POLICIES] ------------------------------------------------------------------------
public readonly record struct DisciplineRule(int Rank, Func<MeasureValue, Resolution> Propose);

public sealed record CoordinationPolicy(
    FrozenDictionary<IfcDomain, DisciplineRule> Disciplines,
    FrozenDictionary<int, RuleSeverity> RippleBands) {

    static readonly DisciplineRule Unranked = new(int.MaxValue, static _ => new Resolution.Reject());

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

    public bool SecondYields(IfcDomain first, IfcDomain second) => Rule(second).Rank >= Rule(first).Rank;

    public Resolution Propose(ClashKind kind, IfcDomain yielding, MeasureValue deficit) =>
        kind == ClashKind.Clearance ? new Resolution.GrantClearance(deficit) : Rule(yielding).Propose(deficit);

    public RuleSeverity Band(int hops) => RippleBands.GetValueOrDefault(hops, RuleSeverity.Info);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Coordination {
    public static Fin<FederatedModel> Federate(
        Seq<(string Model, ElementGraph Graph)> models, Header coordination,
        (double X, double Y, double Z) anchor, CancellationToken token) =>
        GeoTransform.Preflight(models.Map(static m => (m.Model, m.Graph.Header.Reference)), anchor, token)
            .Bind(alignment =>
                alignment.Find(static row => row.Verdict is FrameVerdict.Unresolvable)
                is { IsSome: true, Case: FrameAlignment { Verdict: FrameVerdict.Unresolvable blocked } }
                    ? Fin.Fail<FederatedModel>(blocked.Cause)
                    : ElementGraph.Federate(models, coordination)
                        .Map(union => new FederatedModel(union.Graph, union.Census, alignment)));

    public static Fin<Seq<RuleVerdict>> Check(ElementGraph graph, Seq<CoordinationRule> rules) =>
        rules.TraverseM(rule => Validate(rule)).As()
            .Map(valid => Systems(graph) switch {
                var systems => valid.Map(rule => Verdict(graph, systems, rule)),
            });

    static Seq<(DistributionSystem View, LanguageExt.HashSet<NodeId> Members)> Systems(ElementGraph graph) =>
        DistributionNetwork.View(graph, None).Map(static system => (View: system, Members: toHashSet(system.Members)));

    public static Fin<ClashReport> Run(
        ElementGraph graph, Seq<Interference> interferences, ClashTest test, Option<ClashReport> prior,
        CoordinationPolicy policy, string author, Instant at) {
        LanguageExt.HashSet<NodeId> left = toHashSet(ElementQuery.Query(graph, test.Left).Ids);
        LanguageExt.HashSet<NodeId> right = toHashSet(ElementQuery.Query(graph, test.Right).Ids);
        Seq<Interference> scoped = interferences
            .Filter(clash => clash.Kind == test.Kind && clash.Deficit >= test.Tolerance.Si)
            .Filter(clash => (left.Contains(clash.First) && right.Contains(clash.Second))
                          || (left.Contains(clash.Second) && right.Contains(clash.First)));
        return Rows(graph, scoped, policy, test.Name, author, at)
            .Map(rows => new ClashReport(test, Lifecycle(rows, prior.Map(static p => p.Rows).IfNone(Seq<ClashProposalRow>())), at));
    }

    static Fin<Seq<ClashProposalRow>> Rows(
        ElementGraph graph, Seq<Interference> interferences, CoordinationPolicy policy, string test, string author, Instant at) =>
        interferences.Traverse(clash =>
            ResolveOf(clash, policy).Map(fix => new ClashProposalRow(
                test, clash.First, clash.Second, fix.Yields, clash.Kind, fix.Fix, ClashState.New,
                TopicOf(graph, clash, author, at)))).As();

    static Seq<ClashProposalRow> Lifecycle(Seq<ClashProposalRow> run, Seq<ClashProposalRow> prior) {
        LanguageExt.HashSet<(string, NodeId, NodeId)> seen = toHashSet(prior.Map(Cell));
        LanguageExt.HashSet<(string, NodeId, NodeId)> current = toHashSet(run.Map(Cell));
        return run.Map(row => row with { State = seen.Contains(Cell(row)) ? ClashState.Active : ClashState.New })
             + prior.Filter(row => !current.Contains(Cell(row))).Map(static row => row with { State = ClashState.Resolved });
    }

    static (string, NodeId, NodeId) Cell(ClashProposalRow row) => (row.Test, row.First, row.Second);

    public static ImpactReport Between(ElementGraph graph, ModelDiff before, ModelDiff after, ScheduleNetwork schedule, CostSchedule cost, CoordinationPolicy policy) {
        var touched = toHashSet(after.Changes.Map(static c => c.GlobalId));
        var contested = before.Changes.Map(static c => c.GlobalId).Filter(touched.Contains).Distinct();
        var byExternal = graph.ObjectNodes.Choose(static o => o.ExternalId.Map(e => (e, o.Id))).ToHashMap();
        var seeds = toHashSet(contested.Choose(byExternal.Find));
        Seq<(DistributionSystem View, LanguageExt.HashSet<NodeId> Members)> systems = Systems(graph);
        Map<NodeId, int> ripple = systems.Fold(Map<NodeId, int>(), (map, entry) => entry.Members.Intersect(seeds).ToSeq()
            .Fold(map, (near, seed) => SystemTrace.From(entry.View, seed, TraceMode.Downstream).ElementHops
                .Fold(near, static (acc, hop) => acc.AddOrUpdate(hop.Node, existing => Math.Min(existing, hop.Hops), hop.Hops))));
        var affectedNodes = seeds.Union(toHashSet(ripple.Keys));
        Map<string, int> rippled = toSeq(ripple).Fold(Map<string, int>(), (map, row) => ExternalOf(graph, row.Key)
            .Filter(id => !contested.Contains(id))
            .Match(Some: id => map.AddOrUpdate(id, existing => Math.Min(existing, row.Value), row.Value), None: () => map));
        var elements = contested.Map(id => new ImpactRow(id, 0, policy.Band(0)))
            + toSeq(toSeq(rippled)
                .Map(row => new ImpactRow(row.Key, row.Value, policy.Band(row.Value)))
                .OrderBy(static row => row.Hops));
        var affectedSet = toHashSet(elements.Map(static row => row.GlobalId));
        var assigned = schedule.Assignments
            .Filter(a => a.ElementGlobalIds.Exists(affectedSet.Contains))
            .Map(static a => a.TaskGlobalId).Distinct();
        var priced = cost.Items
            .Filter(i => i.PricedGlobalIds.Exists(affectedSet.Contains))
            .Map(static i => i.GlobalId).Distinct();
        return new ImpactReport(
            contested,
            elements,
            Downstream(schedule.Dependencies.Map(static d => (d.PredecessorGlobalId, d.SuccessorGlobalId)), assigned, policy),
            Downstream(cost.Items.Choose(static i => i.ParentGlobalId.Map(parent => (i.GlobalId, parent))), priced, policy),
            systems.Filter(entry => entry.Members.Exists(affectedNodes.Contains))
                .Choose(static entry => entry.View.ExternalId).Distinct());
    }

    static Fin<CoordinationRule> Validate(CoordinationRule rule) => rule.Switch(
        require:     static (_, r) => Fin.Succ<CoordinationRule>(r),
        prohibit:    static (_, r) => Fin.Succ<CoordinationRule>(r),
        cardinality: static (k, r) => r.Min < 0 || r.Max.Match(Some: hi => hi < r.Min, None: static () => false)
                         ? new BimFault.Refused(k, BimScope.Review, BimReason.Rejected, string.Join(':', new object?[] { "coordination-rule", "cardinality-bound", r.Min.ToString(CultureInfo.InvariantCulture), r.Max.Match(Some: h => h.ToString(CultureInfo.InvariantCulture), None: static () => "*") }))
                         : Fin.Succ<CoordinationRule>(r),
        unique:      static (_, r) => Fin.Succ<CoordinationRule>(r),
        reachable:   static (_, r) => Fin.Succ<CoordinationRule>(r));

    static RuleVerdict Verdict(ElementGraph graph, Seq<(DistributionSystem View, LanguageExt.HashSet<NodeId> Members)> systems, CoordinationRule rule) =>
        rule.Switch(
            state:       (Applicable: ElementQuery.Query(graph, rule.Applicability), Systems: systems),
            require:     static (s, r) => Predicated(r, s.Applicable, r.Requirement, prohibits: false),
            prohibit:    static (s, r) => Predicated(r, s.Applicable, r.Requirement, prohibits: true),
            cardinality: static (s, r) => Counted(r, s.Applicable),
            unique:      static (s, r) => Distinct(r, s.Applicable),
            reachable:   static (s, r) => Traced(r, s.Applicable, s.Systems));

    static RuleVerdict Predicated(CoordinationRule rule, ElementQuery applicable, BimTerm requirement, bool prohibits) {
        var matching = applicable.Where(requirement);
        var (pass, violated) = prohibits ? (applicable.Except(matching), matching) : (matching, applicable.Except(matching));
        return new RuleVerdict(rule, Ids(pass), Ids(violated));
    }

    static RuleVerdict Counted(CoordinationRule.Cardinality rule, ElementQuery applicable) =>
        rule.Min <= applicable.Count && rule.Max.Match(Some: hi => applicable.Count <= hi, None: static () => true)
            ? new RuleVerdict(rule, Ids(applicable), Seq<NodeId>())
            : new RuleVerdict(rule, Seq<NodeId>(), Ids(applicable));

    static RuleVerdict Distinct(CoordinationRule.Unique rule, ElementQuery applicable) {
        var keyed = applicable.Objects.Map(o => (o.Id, Key: ElementQuery.ValuesOf(applicable.Graph, o, rule.Source).Head.Map(static v => v.Render())));
        var duplicated = toHashSet(keyed.Choose(static r => r.Key)
            .GroupBy(static k => k).Where(static g => g.Count() > 1).Select(static g => g.Key));
        var violated = keyed.Filter(r => r.Key.Match(Some: duplicated.Contains, None: static () => true)).Map(static r => r.Id);
        var pass = keyed.Filter(r => r.Key.Match(Some: k => !duplicated.Contains(k), None: static () => false)).Map(static r => r.Id);
        return new RuleVerdict(rule, pass, violated);
    }

    static RuleVerdict Traced(CoordinationRule.Reachable rule, ElementQuery applicable, Seq<(DistributionSystem View, LanguageExt.HashSet<NodeId> Members)> systems) {
        LanguageExt.HashSet<NodeId> targets = toHashSet(ElementQuery.Query(applicable.Graph, rule.Target).Ids);
        (Seq<NodeId> pass, Seq<NodeId> violated) = Ids(applicable).Partition(id => systems
            .Filter(entry => entry.Members.Contains(id))
            .Exists(entry => SystemTrace.From(entry.View, id, rule.Mode).ReachedElements.Exists(targets.Contains)));
        return new RuleVerdict(rule, pass, violated);
    }

    static Seq<NodeId> Ids(ElementQuery query) => query.Ids;

    static Fin<(NodeId Yields, Resolution Fix)> ResolveOf(Interference clash, CoordinationPolicy policy) =>
        MeasureValue.OfSi(QuantityType.Length, Dimension.LengthDim, clash.Deficit)
            .Map(deficit => policy.SecondYields(clash.FirstDomain, clash.SecondDomain)
                ? (clash.Second, policy.Propose(clash.Kind, clash.SecondDomain, deficit))
                : (clash.First, policy.Propose(clash.Kind, clash.FirstDomain, deficit)));

    public static Seq<NodeId> Cleared(ClashIndex index, ClashProposalRow row) =>
        row.Proposed is Resolution.GrantClearance
            ? Volume(index, row.Yields)
                .Map(volume => InterferenceCheck.Neighborhood(index, volume)
                    .Map(static member => member.Id)
                    .Filter(id => id != row.First && id != row.Second))
                .IfNone(Seq<NodeId>())
            : Seq<NodeId>();

    static Option<BoundVolume> Volume(ClashIndex index, NodeId member) =>
        toSeq(index.Registry).Find(entry => entry.Member.Id == member).Map(static entry => entry.Bounds);

    public static Seq<BcfTopic> Raise(Seq<IdsAudit> audits, string author, Instant at) =>
        audits.Filter(static a => !a.Conforms).Map(a => new BcfTopic(
            $"ids-{a.Spec}", $"IDS non-conformance: {a.Specification}#{a.Spec}",
            BcfStatus.Open, "IDS", "Normal", author, at,
            a.Verdicts.Filter(static v => !v.Failed.IsEmpty).Map(v => new BcfComment(
                $"ids-{a.Spec}-{v.Key.ToValue():X32}", author,
                $"{v.Key.ToValue():X32}: {v.Failed.Count} failed", Option<string>.None, at)),
            Seq(new BcfViewpoint($"vp-ids-{a.Spec}", Option<BcfCamera>.None,
                a.Verdicts.Bind(static v => v.Failed).Distinct(), BcfVisibility.Everything, Option<ReadOnlyMemory<byte>>.None))));

    static Option<BcfTopic> TopicOf(ElementGraph graph, Interference clash, string author, Instant at) =>
        from first in ExternalOf(graph, clash.First)
        from second in ExternalOf(graph, clash.Second)
        select new BcfTopic(
            $"clash-{clash.Identity:X32}", $"{clash.Kind.Key} clash: {first} / {second}",
            BcfStatus.Open, "Clash", clash.CrossDiscipline ? "High" : "Normal", author, at,
            Seq<BcfComment>(),
            Seq(new BcfViewpoint($"vp-{clash.Identity:X32}", Option<BcfCamera>.None,
                Seq(first, second), BcfVisibility.Everything, Option<ReadOnlyMemory<byte>>.None)));

    static Seq<ImpactRow> Downstream(Seq<(string From, string To)> edges, Seq<string> seeds, CoordinationPolicy policy) {
        const string Source = "\0multi-source";
        var dag = new AdjacencyGraph<string, SEdge<string>>();
        foreach (var (from, to) in edges) { dag.AddVerticesAndEdge(new SEdge<string>(from, to)); }
        foreach (string seed in seeds) { dag.AddVerticesAndEdge(new SEdge<string>(Source, seed)); }
        BreadthFirstSearchAlgorithm<string, SEdge<string>> bfs = new(dag);
        VertexDistanceRecorderObserver<string, SEdge<string>> distances = new(static _ => 1.0);
        using (distances.Attach(bfs)) { bfs.Compute(Source); }
        return toSeq(distances.Distances
            .Where(entry => !string.Equals(entry.Key, Source, StringComparison.Ordinal))
            .Select(entry => new ImpactRow(entry.Key, (int)entry.Value - 1, policy.Band((int)entry.Value - 1)))
            .OrderBy(static row => row.Hops));
    }

    static Option<string> ExternalOf(ElementGraph graph, NodeId id) =>
        graph.Find<Node.Object>(id).Bind(static o => o.ExternalId);
}
```

## [03]-[SIGN_OFF]

- Owner: `SignOff` the `[SmartEnum<string>]` state machine over the `Review/issues#BCF_ARCHIVE` `BcfStatus` lifecycle — each case carrying its legal forward `SignOff` set as per-case delegate data (the transition table folded onto the generated case family, never a parallel `FrozenDictionary<BcfStatus,…>` the instances mirror) so the SmartEnum IS the dispatch surface and a governed workflow advances an issue through `Open → InProgress → Resolved → Closed` (with the `Reopened` re-entry) under a compile-addressable lifecycle the AppUi board references by `SignOff.Resolved`; `BcfStatus` stays the wire serialization value the `Review/issues#TS_PROJECTION` projects, `SignOff` the host-neutral transition owner over it; `IssueBoard` the host-neutral board fold over the `BcfTopic` family the `Rasm.AppUi/Collab/issues` relocation grounds here — the status lanes, the vocabulary-ranked priority ordering, and the viewpoint-anchored element selection the desktop and any future head project over one contract.
- Entry: `SignOff.Advance` is the one polymorphic transition entrypoint discriminating on input shape — the instance `state.Advance(SignOff to)` reads the case's own legal forward set, the wire overload `SignOff.Advance(BcfStatus from, BcfStatus to)` resolving each value through `SignOff.Of` first so a caller holding a wire `BcfStatus` transitions through the same owner — `Fin<T>` aborting an illegal transition (`BimFault.Refused` with `BimReason.Rejected`) lifted BARE, so a `Closed → InProgress` skip is rejected while a `Closed → Reopened` re-entry is admitted; `IssueBoard.Of(Seq<BcfTopic> topics, Seq<string> priorities)` folds the topic set into the board projection (the status partition, the vocabulary-ranked priority ordering, the viewpoint-anchored selection) the AppUi head materializes, `priorities` the ingested `BcfVocabulary.Priorities` roster, and `IssueBoard.Anchor(params ReadOnlySpan<ClashProposalRow>)` binds one proposal or a whole `Run` to its `BcfTopic`s in ONE re-partition — the span absorbing the single and the batch arity.
- Auto: `Advance` reads the state's `Forward()` legal set off the case data and admits `to` only when present (`Open`→`{InProgress, Closed}`, `InProgress`→`{Resolved, Open}`, `Resolved`→`{Closed, Reopened}`, `Closed`→`{Reopened}`, `Reopened`→`{InProgress, Closed}`), each forward set a `static () => Seq(…)` delegate the `[UseDelegateFromConstructor]` generated `Forward()` binds, so a new lifecycle state is one `SignOff` case carrying its own forward set, never a second table to keep in sync; `SignOff.Of` resolves the wire `BcfStatus` to its case total; `IssueBoard.Of` partitions the topic set by `BcfStatus`, orders within each partition by the `PriorityRank` INDEX into the archive's own declared priority roster (buildingSMART declares it most-urgent-first, so the index IS the rank and a project's renamed bands order correctly), falling back to the frozen canonical roster only where the archive declares no vocabulary — never the lexical string, which reads `"High" < "Low" < "Normal"` and inverts the real urgency — and projects each topic's `BcfViewpoint.SelectedGlobalIds` onto the element selection the AppUi board highlights.
- Output: the `SignOff` lifecycle is the governed sign-off workflow the `ClashProposalRow` anchors its proposed fix to (the proposal's `BcfTopic` advancing through the lifecycle as the clash is resolved) and the `Review/versioning#VERSION_GRAPH` `MergeConflict` resolution advances through; the `IssueBoard` projection the single BCF issue-board domain the desktop and any future head project over one contract; the AppUi keeps only the board projection, the durable op-log/CDE-sync store stays the `Rasm.Persistence/Version/ledger` concern joined by `ExternalId`, and Bim owns the issue-board domain over the `BcfTopic` contract.
- Growth: a new lifecycle state is one `SignOff` case carrying its own forward set (no second transition table to widen); a new board partition is one fold over the same `BcfTopic` set; a new priority band is one row on the archive's own declared vocabulary and needs no page edit at all; the `ClashProposalRow`-to-`BcfTopic` anchor folds the proposals' topics onto the board through the one `Of` partition; never a per-state class, never a second BCF schema, and never a board-side issue store.
- Boundary: `Rasm.Bim/coordination` owns the issue-board DOMAIN over the `Review/issues#BCF_ARCHIVE` `BcfTopic` contract and `Rasm.AppUi/Collab/issues` owns only the board projection — the AppUi head re-mints no BCF schema and reads the domain at the package edge, the `[ISSUES_RELOCATE_TO_BIM]` relocation leaving `SignOff` on a settled Bim owner; `Rasm.Persistence/Version/ledger` owns the durable op-log/CDE-sync store joined by the `Node.Object` `ExternalId` content-key, the `[ANNOTATION_RELOCATE_TO_BIM]` relocation leaving the BCF record family, the `BcfApi` server dialect, AND the `.bcfzip` container wire in `Rasm.Bim` — `BcfArchive` is the branch's one custodian and `Rasm.Persistence/Ingest/issue` holds the durable rows, neither side re-minting the BCF schema across the boundary; the `SignOff` legal transitions are per-case data on the SmartEnum and a parallel `FrozenDictionary<BcfStatus,…>` the instances merely mirror, a per-transition method, or an unchecked status setter is the deleted form — the `[SmartEnum]` is the dispatch surface so `Advance` is its operation and an illegal advance lifts `BimFault.Refused` with `BimReason.Rejected` BARE; the `IssueBoard` ranks by the archive's own `BcfVocabulary.Priorities` order, so a frozen canonical rank table as the PRIMARY authority is the deleted form (it reads a project's own `"P1"`/`"P2"` bands as unknown and flattens the board) and survives only as the no-vocabulary fallback; a lexical priority sort or a parallel board-side issue record are the deleted forms.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
// Contracts are retired from this logic.
using Rasm.Bim.Model;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Bim.Coordination;

// --- [MODELS] --------------------------------------------------------------------------
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

    public bool IsTerminal => Forward().IsEmpty;

    public static SignOff Of(BcfStatus status) => toSeq(Items).Find(s => s.Status == status).IfNone(Open);

    public Fin<SignOff> Advance(SignOff to) =>
        Forward().Contains(to)
            ? Fin.Succ(to)
            : Fin.Fail<SignOff>(new BimFault.Refused(BimScope.Review, BimReason.Rejected, string.Join(':', new object?[] { "signoff-illegal-transition", Status.ToString(), to.Status.ToString() })));

    public static Fin<BcfStatus> Advance(BcfStatus from, BcfStatus to) =>
        Of(from).Advance(Of(to)).Map(static s => s.Status);
}

public sealed record IssueBoard(Map<BcfStatus, Seq<BcfTopic>> Lanes, Seq<string> Priorities) {
    public static IssueBoard Of(Seq<BcfTopic> topics, Seq<string> priorities) =>
        new(topics
            .GroupBy(static t => t.Status)
            .Select(g => (g.Key, toSeq(g.OrderBy(t => PriorityRank.Of(priorities, t.Priority)))))
            .ToMap(), priorities);

    public IssueBoard Anchor(params ReadOnlySpan<ClashProposalRow> proposals) {
        var incoming = toSeq(Iterable<ClashProposalRow>.FromSpan(proposals).Choose(static p => p.Topic));
        var replaced = toHashSet(incoming.Map(static t => t.Guid));
        return Of(Lanes.Values.ToSeq().Bind(static lane => lane).Filter(t => !replaced.Contains(t.Guid)) + incoming, Priorities);
    }
}

// --- [POLICIES] ------------------------------------------------------------------------
static class PriorityRank {
    static readonly FrozenDictionary<string, int> Fallback = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase) {
        ["Critical"] = 0, ["High"] = 1, ["Major"] = 1, ["Normal"] = 2, ["Medium"] = 2, ["Low"] = 3, ["Minor"] = 3,
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    public static int Of(Seq<string> vocabulary, string priority) =>
        vocabulary.IsEmpty
            ? Fallback.GetValueOrDefault(priority, int.MaxValue)
            : vocabulary.Choose((index, row) => string.Equals(row, priority, StringComparison.OrdinalIgnoreCase) ? Some(index) : None)
                .Head.IfNone(int.MaxValue);
}
```

## [04]-[RESEARCH]

(none)
