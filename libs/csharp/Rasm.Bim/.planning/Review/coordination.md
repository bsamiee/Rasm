# [BIM_COORDINATION]

The model-checking and coordination DOMAIN owner over the seam `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph`: the if-X-then-Y rule engine, the clash-resolution proposal fold, the A/B model-impact report, the IDS-audit board handoff, the BCF sign-off state machine, and the host-neutral BCF issue-board the `csharp:Rasm.Persistence/Version/ledger` and `csharp:Rasm.AppUi/Collab/issues` relocations settle here. Each workflow COMPOSES a settled vocabulary the IDS/BCF/Diff owners supply but never assemble — the `Model/query#ELEMENT_SET` `ElementPredicate` algebra (the one selection surface), the `Model/systems#INTERFERENCE` `Interference` ranked clash evidence, the `Model/systems#CONNECTIVITY`/`#SYSTEM_TRACE` `DistributionNetwork.View` flow views and `SystemTrace` reachability fold, the `Review/diff#MODEL_DIFF` change-sets, the `Review/issues#BCF_ARCHIVE` `BcfTopic` family, and the `Planning/schedule#SCHEDULE`/`Planning/cost#ESTIMATE` joins — and re-derives none of them: no second predicate surface, no re-run proximity test, no second reachability walk, no re-computed diff.

Identity follows the seam law [H6]: the graph is keyed by the neutral `Rasm.Element/Graph/element#NODE_MODEL` `NodeId`, and a receipt keys by the axis its consumer joins on. A kernel receipt keys on `NodeId` — the COMPLETE identity present on every node — so a `RuleVerdict` run over the WORKING graph reports an authored element that carries no IFC `GlobalId` yet, where an `ExternalId`-keyed receipt would silently drop it (the `Model/query#ELEMENT_SET` `GlobalIds` projection `Choose`s over `Option<string>` and discards `None`). An IFC-semantic receipt keys on the IFC `ExternalId` — the BCF viewpoint anchor and the impact report's schedule/cost/system join — because those targets (`Review/issues#BCF_ARCHIVE` viewpoints, `Planning/schedule#SCHEDULE` `TaskAssignment.ElementGlobalIds`, `Planning/cost#ESTIMATE` `CostItem.PricedGlobalIds`) are themselves IFC-GlobalId-keyed; the `NodeId → ExternalId` projection happens at the boundary through the Bim-stored `Node.Object.ExternalId`, never a second identity scheme.

The coordination owner is the BCF DOMAIN: `Rasm.Bim/coordination` owns the issue-board semantics over the `Review/issues#BCF_ARCHIVE` `BcfTopic`/`BcfComment`/`BcfViewpoint` family and the `.bcfzip`/`BcfApi` codec, while `csharp:Rasm.Persistence/Version/ledger` keeps the durable op-log/CDE-sync store and `csharp:Rasm.AppUi/Collab/issues` keeps only the board projection — the three joined by the `Node.Object` `ExternalId` content-key, never a second BCF schema across the boundary. Every coordination rejection lifts the `Model/faults#FAULT_BAND` `BimFault` band BARE (the `Expected`-derived case IS the `Error`, no `.ToError()` hop), never a new fault family. The page is HOST-LOCAL.

## [01]-[INDEX]

- [01]-[COORDINATION]: the `CoordinationRule` `[Union]` (the five model-check modalities — `Require`/`Prohibit`/`Cardinality`/`Unique`/`Reachable`, severity the abstract axis), the `Resolution` `[Union]`, the `RuleVerdict` `NodeId`-partition receipt, the `ClashProposalRow`, the `ImpactReport` transitive-closure impact receipt, and the `Coordination` fold owner (`Check`/`Propose`/`Between`/`Raise`) over the seam `ElementGraph`.
- [02]-[SIGN_OFF]: the `SignOff` `[SmartEnum<string>]` lifecycle over the `Review/issues#BCF_ARCHIVE` `BcfStatus`, and the `IssueBoard` host-neutral board projection the `csharp:Rasm.AppUi/Collab/issues` relocation grounds here.

## [02]-[COORDINATION]

- Owner: `CoordinationRule` the closed `[Union]` of the five model-check modalities, each carrying an applicability `Model/query#ELEMENT_SET` `ElementPredicate` (the X — the SAME selection surface the `Review/validation#IDS_FACETS` IDS facet fold reads) plus its modality-specific requirement (the Y), with `Applicability` and `Severity` the abstract members every arm overrides positionally — an advisory rule is any arm at `RuleSeverity.Info`, the severity axis owning what a sibling `Recommend` modality would restate; `Resolution` the closed `[Union]` of proposed clash fixes; `RuleVerdict` the per-rule receipt folding the applicable set into the passing and the violating `NodeId` partition, its `Severity` derived from the rule it carries; `ClashProposalRow` the proposed fix over one `Model/systems#INTERFERENCE` `Interference` — the clashing `NodeId` pair, the yielding endpoint, the ranked `Resolution`, and the anchored `BcfTopic`; `ImpactReport` the A/B fold over two `Review/diff#MODEL_DIFF` change-sets into the contested seed plus the transitively downstream-affected element/task/cost-line/system closures; `Coordination` the static fold owner (`Check`/`Propose`/`Between`/`Raise`) collapsing the prior `CoordinationCheck`/`ClashProposal`/`ChangeImpact` triplet into one deep coordination domain owner and carrying the write-time IDS-audit-to-board handoff.
- Cases: `CoordinationRule` arms `Require` (`ElementPredicate Applicability`/`Requirement`, `RuleSeverity`) · `Prohibit` (same shape, the IDS-mirrored declarative polarity) · `Cardinality` (`Applicability`, `int Min`, `Option<int> Max`, `RuleSeverity` — the applicable-set count must lie in `[Min, Max]`) · `Unique` (`Applicability`, `ValueSource Source`, `RuleSeverity` — every applicable element's source value distinct, the source a direct `ObjectAttribute` OR an effective Pset/Qto property read through `ElementSet.ValuesOf`) · `Reachable` (`Applicability`, `ElementPredicate Target`, `TraceMode Mode`, `RuleSeverity` — every applicable element must reach a `Target`-matching element through its owning `DistributionSystem`'s `Mode`-oriented flow graph) (5) — a per-element predicate, a set-count bound, an attribute uniqueness, and a graph-reachability incidence are the four irreducible model-check shapes, each one arm reusing the one selection algebra (the `Reachable` arm additionally reusing the `Model/systems#SYSTEM_TRACE` `TraceMode` orientation rows), never a per-rule-kind class and never a second predicate surface; an advisory check is `RuleSeverity.Info` on any arm — the retired `Recommend` arm was `Require`@`Info` spelled as a modality, the severity parameter already generating that space; `Resolution` arms `Reroute` (a suggested centerline offset for a linear MEP run) · `Resize` (a suggested dimension reduction for a discrete element) · `GrantClearance` (an accepted clearance exception) · `Sleeve` (a coordinator-elected framed penetration bore through the prevailing element) · `Reject` (no fix — the clash stands for a coordinator's manual review) (5).
- Entry: `Coordination.Check(ElementGraph graph, Seq<CoordinationRule> rules, Op key)` validates the rule library then folds each rule to a `RuleVerdict` over the seam graph — `Fin<T>` aborting a malformed rule (a `Cardinality` bound `Min < 0` or `Max < Min`) onto `Model/faults#FAULT_BAND` `BimFault.ModelRejected` (the `coordination-rule:` detail family — the semantic-reject arm, never `UnmappedClass` whose meaning is an element-class miss) lifted BARE, the well-formed fold itself total; `Coordination.Propose(ElementGraph graph, Seq<Interference> interferences, string author, Instant at)` folds the ranked `Model/systems#INTERFERENCE` clash evidence into `ClashProposalRow`s — total, each clash `NodeId` resolved to its IFC `ExternalId` for the BCF anchor; `Coordination.Between(ElementGraph graph, ModelDiff before, ModelDiff after, ScheduleNetwork schedule, CostSchedule cost)` folds the two change-sets into the contested seed and its reachability closures — total, the flow leg composing the settled `Model/systems#SYSTEM_TRACE` fold, the schedule/cost legs one `QuikGraph` transitive closure, every join reading the IFC-GlobalId axis; `Coordination.Raise(Seq<IdsAudit> audits, string author, Instant at)` folds every NON-conforming `Review/validation#IDS_FACETS` `IdsAudit` onto one `BcfTopic` (keyed on the specification name so a re-audit re-lands the same topic, one `BcfComment` per failing facet, the viewpoint anchoring every failed `GlobalId`) — the write-time IDS↔BCF seam the read-time shared `ElementPredicate` algebra mirrors, total because the audit is already IFC-GlobalId-keyed.
- Auto: `Check` first runs each rule through `Validate` (`TraverseM` short-circuiting the whole library on the first malformed rule) then maps the well-formed library to verdicts; `Verdict` selects the applicable set ONCE through `ElementSet.Query(graph, rule.Applicability)` and dispatches the generated total `Switch` — `Require`/`Prohibit` collapse to ONE `Predicated` partition derived by a `prohibits` policy bit (the matching subset re-folded over only the current members via `ElementSet.Where`, never the retired O(n²) `Holds` re-query — the passing set for `Require`, the violating set for `Prohibit`), `Cardinality` partitions on the applicable-set count (in range the whole set passes, out of range the whole set is the violating evidence so the board highlights every element of a storey that breaches its exit count), `Unique` groups the applicable set by the `ObjectAttribute.Read` value (a duplicate-valued OR an unreadable-attribute member violates, the rest pass), and `Reachable` folds each applicable element through the settled `Model/systems#SYSTEM_TRACE` closure — its owning `DistributionNetwork.View` systems, the `Mode`-oriented `SystemTrace.From`, intersected against the ONE-query `Target` set — so an element in no system or whose trace reaches no target violates (the orphaned terminal IS the defect the rule surfaces); `Propose` reads each `Interference` (consuming the systems-page ranked clash evidence rather than re-deriving proximity), folds it onto a `Resolution` through the `DisciplinePriority` hierarchy and the `ClashKind`/cross-discipline span (a clearance graze is waived `GrantClearance`, a cross-discipline hard clash re-routes the lower-priority service `Reroute`, a same-discipline hard clash re-sizes the yielding element `Resize`, `Sleeve`/`Reject` reserved for the coordinator's manual election), records the yielding endpoint, and mints a `BcfTopic` per clash anchoring the resolved `ExternalId` pair onto a `BcfViewpoint.SelectedGlobalIds`; `Between` intersects the two `ModelDiff` change-sets by IFC GlobalId into the contested seed, resolves the seed to `NodeId`s through the graph's `ExternalId` index, propagates each seed member downstream through its owning system's flow graph (`SystemTrace.From(system, seed, TraceMode.Downstream)` — the settled reachability fold, never a second walk), joins the CLOSED affected set to the `ScheduleNetwork.Assignments` tasks naming it and the `CostSchedule.Items` lines pricing it, closes BOTH transitively through the one `Successors` kernel — the `SequenceRel` predecessor→successor DAG (a slipped task delays every transitive successor) and the `CostItem.ParentGlobalId` roll-up tree (a repriced line stales every ancestor) two edge-row inputs to one `QuikGraph` `ComputeTransitiveClosure` — and surfaces the `Model/systems#CONNECTIVITY` `DistributionSystem`s whose member set the closure intersects.
- Receipt: the `Seq<RuleVerdict>` is the parameterized model-checking evidence (a coordination-rule library the IDS/BCF/Diff owners give the vocabulary for) on the `NodeId` axis the AppUi board highlights and Persistence stores; the `Seq<ClashProposalRow>` the ranked clash report with proposed fixes anchored to BCF on the IFC `ExternalId` axis a viewer round-trips; the `ImpactReport` the transitive A/B change-impact closure on the IFC-GlobalId axis a 4D/5D federation reads (the contested seed the board anchors contention on, the closures the ripple evidence); the `SignOff` lifecycle governs the issue, so a governed workflow — rule check → clash proposal → impact report → BCF sign-off — reads one composed pipeline over the settled owners.
- Packages: Rasm.Element, QuikGraph, Thinktecture.Runtime.Extensions, NodaTime, LanguageExt.Core, Rasm
- Growth: a new model-check modality is one `CoordinationRule` union arm reusing the `ElementPredicate` algebra (the rule library never forks a second selector — the `Reachable` arm reuses the `TraceMode` rows as its orientation policy, never a second trace enum); a new clash fix is one `Resolution` arm; a new proposal heuristic is one `DisciplinePriority` row, never a branch in the fold; a new impact dimension (a downstream-affected zone, a downstream-affected document) is one column on `ImpactReport` plus at most one edge-row input to the one `Successors` closure kernel; never a per-rule-kind type, never a second selection surface, and never a re-derived proximity, reachability walk, or diff in this owner.
- Boundary: the rule applicability/requirement is the `Model/query#ELEMENT_SET` `ElementPredicate` (the `All`/`Any`/`Not` boolean closure composing a multi-condition rule), so the validation predicate IS the query predicate IS the coordination predicate — one selection surface across `Model/query#ELEMENT_SET`, `Review/validation#IDS_FACETS`, and this owner, a parallel `RuleSelector`/`CoordinationQuery` expression type the deleted form, and the retired `new ElementSet(model.Elements)` over a second stored `BimElement` collection GONE (the rule folds the seam graph the `Projection/semantic#SEMANTIC_PROJECTOR` assembles); the `ClashProposal` fold's clash evidence is the `Model/systems#INTERFERENCE` `Interference` row carrying the `NodeId` pair (the systems page owns the geometric proximity, this owner consumes the ranked evidence and proposes the fix) — re-running the proximity test here is the named cross-page drift defect, and the retired `clash.FirstGlobalId`/`clash.SecondGlobalId` string pair is GONE, replaced by `clash.First`/`clash.Second` `NodeId` with the IFC `ExternalId` resolved only at the BCF anchor; the `ImpactReport` ripple and the `Reachable` verdict are REAL reachability — the flow leg composes the settled `Model/systems#SYSTEM_TRACE` `SystemTrace.From` over `DistributionNetwork.View` and the schedule/cost legs the one `Successors` `QuikGraph` `ComputeTransitiveClosure` kernel — so the retired flat two-diff intersection + one-hop membership join (`AffectedSystemsOf` testing set intersection where the prose claimed "ripples into") and a hand-rolled visited-set walk are the deleted forms, the `[GRAPH_ALGORITHM]` owner collapsing every such walk; a `RuleVerdict` keys on the COMPLETE `NodeId` identity (an `ExternalId`-keyed verdict silently dropping an un-emitted authored element is the deleted form), while the BCF anchor and the impact join key on the IFC `ExternalId` their targets demand; the `CoordinationRule`/`Resolution` unions are closed `[Union]` families and a per-kind class is the deleted form; the coordination operations live on the ONE `Coordination` owner (the prior `CoordinationCheck`/`ClashProposal`/`ChangeImpact` single-method classes collapsed; the IDS-audit board handoff `Raise` a fourth fold on the same owner, never a sibling class), and a coordination rejection lifts `Model/faults#FAULT_BAND` `BimFault` BARE — the `Expected`-derived case IS the `Error`, the retired `.ToError()` lowering hop and the `new BimFault.X("string")` single-arg construction GONE (the band ctor is `(Op key, string detail)`).

```csharp contract
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using System.Linq;
using LanguageExt;
using Rasm.Bim.Model;
using NodaTime;
using QuikGraph;
using QuikGraph.Algorithms;
using Rasm.Element.Graph;
using Rasm.Element.Properties;
using Thinktecture;
using Op = Rasm.Domain.Op;
using static LanguageExt.Prelude;

// The coordination DOMAIN namespace the ARCHITECTURE seams name — the Review/issues#BCF_ARCHIVE BcfTopic family
// and the SignOff lifecycle the csharp:Rasm.AppUi/Collab/issues board consumes as Rasm.Bim.Coordination.*; the
// child namespace resolves the sibling Rasm.Bim owners (ElementSet/ElementPredicate/Interference/BimFault/IfcDomain
// /ModelDiff/ScheduleNetwork/CostSchedule/DistributionNetwork/SystemTrace/TraceMode/IdsAudit) implicitly, the seam
// owners through Rasm.Element.
namespace Rasm.Bim.Coordination;

// --- [TYPES] ------------------------------------------------------------------------------
public enum RuleSeverity : byte { Info = 0, Warning = 1, Error = 2 }

// --- [MODELS] -----------------------------------------------------------------------------
// The if-X-then-Y model-check vocabulary: every arm carries an applicability ElementPredicate (the X — the SAME
// Model/query#ELEMENT_SET selection surface the IDS facet fold reads, never a second RuleSelector) plus its own
// modality requirement (the Y). Five modalities — a per-element polarity pair (Require/Prohibit), a set-count
// bound (Cardinality), a value-source uniqueness (Unique — a direct attribute or an effective Pset/Qto value), a flow-graph reachability (Reachable, its orientation
// the Model/systems#SYSTEM_TRACE TraceMode rows) — the closed family a new modality lands in as one arm.
// Applicability and Severity are abstract members every arm overrides positionally, so a rule selects and reports
// without a Switch; an ADVISORY rule is any arm at RuleSeverity.Info — severity is the axis, never a sibling arm
// (the retired Recommend case was Require@Info spelled as a modality).
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

// The closed clash-fix family proposed onto a Model/systems#INTERFERENCE clash. Reroute/Resize carry the deficit
// scalar the proposal derives; Sleeve is the coordinator-elected framed penetration through the prevailing element
// (sizing needs the yielding run's dimension the clash row does not carry) and Reject the coordinator's manual
// override — neither auto-minted by the fold.
[Union]
public abstract partial record Resolution {
    private Resolution() { }

    public sealed record Reroute(double Offset) : Resolution;
    public sealed record Resize(double Dimension) : Resolution;
    public sealed record GrantClearance(double Accepted) : Resolution;
    public sealed record Sleeve(double Bore) : Resolution;
    public sealed record Reject : Resolution;
}

// The per-rule receipt on the COMPLETE NodeId identity (present on every node pre-Emit), never the IFC ExternalId
// — a coordination check on the working graph must report an authored element that carries no GlobalId yet. The
// rule rides the receipt so the board reads which rule and which modality failed without a stringly RuleKind tag,
// and Severity DERIVES from the rule (a receipt column restating the rule's own severity is the deleted
// duplication); a consumer raising a BCF topic from a violation projects NodeId -> ExternalId through the graph it checked.
public sealed record RuleVerdict(CoordinationRule Rule, Seq<NodeId> Passed, Seq<NodeId> Violated) {
    public RuleSeverity Severity => Rule.Severity;
    public bool Conforms => Violated.IsEmpty;
    public bool Blocking => !Conforms && Severity == RuleSeverity.Error;
}

// The clash proposal: the clashing NodeId pair (kernel identity), the Yields endpoint the DisciplinePriority
// hierarchy elects to change, the ranked Resolution, and the BcfTopic the fix lands on. The topic's BcfViewpoint
// anchors on the IFC ExternalId the pair resolves to (BCF is an IFC-native container — SelectedGlobalIds carry
// IFC GlobalIds), the ExternalId.IfNone(NodeId) fallback total for an authored element not yet emitted.
public sealed record ClashProposalRow(NodeId First, NodeId Second, NodeId Yields, ClashKind Kind, Resolution Proposed, BcfTopic Topic);

// The A/B change-impact report — every dimension on the IFC ExternalId axis because the join targets (the IFC
// model diff, the schedule assignments, the cost lines) are IFC-GlobalId-keyed receipts, never the neutral NodeId.
// Contested is the direct two-diff seed the BCF board anchors contention on; AffectedElements the seed plus the
// flow-downstream closure; the task/line columns transitively closed, the systems the closure's memberships.
public sealed record ImpactReport(
    Seq<string> Contested,
    Seq<string> AffectedElements,
    Seq<string> AffectedTasks,
    Seq<string> AffectedCostLines,
    Seq<string> AffectedSystems);

// --- [POLICIES] ---------------------------------------------------------------------------
// The MEP coordination priority in rigidity order: the lower-priority (higher-rank) discipline's element is the
// one a clash proposal elects to move — Structural/Geotechnical/Infrastructure never yield, Architecture conveying
// plant (a lift core, an escalator) yields only to them, gravity Plumbing before HvacFire before the most
// reroutable Electrical. One frozen rank row per IfcDomain — the full seven-member roster, so no discipline falls
// to the silent unranked default; a re-tuned trade hierarchy is one row edit, never a branch in the proposal fold.
static class DisciplinePriority {
    static readonly FrozenDictionary<IfcDomain, int> Rank = new Dictionary<IfcDomain, int> {
        [IfcDomain.Structural]     = 0,
        [IfcDomain.Geotechnical]   = 0,
        [IfcDomain.Infrastructure] = 0,
        [IfcDomain.Architecture]   = 1,
        [IfcDomain.Plumbing]       = 2,
        [IfcDomain.HvacFire]       = 3,
        [IfcDomain.Electrical]     = 4,
    }.ToFrozenDictionary();

    static int Of(IfcDomain domain) => Rank.GetValueOrDefault(domain, int.MaxValue);

    // True when the second endpoint is the lower-or-equal-priority discipline (it yields); an equal-rank
    // same-discipline clash tie-breaks onto the second endpoint deterministically.
    public static bool SecondYields(IfcDomain first, IfcDomain second) => Of(second) >= Of(first);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
// The coordination DOMAIN owner: the rule-check fold, the clash-to-proposal fold, the A/B impact fold, and the
// IDS-audit board handoff — the workflows the IDS/BCF/Diff owners give the vocabulary for but never compose.
// Each reads the seam graph the Projection/semantic#SEMANTIC_PROJECTOR assembles; none re-derives a selection
// surface, a proximity test, a reachability walk, or a diff.
public static class Coordination {
    // The rule-check fold: validate the library (a malformed bound short-circuits the whole check onto the
    // Model/faults#FAULT_BAND coordination-rule band), then fold each well-formed rule to a verdict — pure ROP,
    // never a Try.lift exception funnel, because ElementSet.Query is total.
    public static Fin<Seq<RuleVerdict>> Check(ElementGraph graph, Seq<CoordinationRule> rules, Op key) =>
        rules.TraverseM(rule => Validate(rule, key)).As().Map(valid => valid.Map(rule => Verdict(graph, rule)));

    // The clash-to-proposal fold: each ranked Interference maps onto a Resolution + a BcfTopic anchored on the
    // resolved IFC ExternalId pair — total, the clash endpoints are graph NodeIds the systems fold produced.
    public static Seq<ClashProposalRow> Propose(ElementGraph graph, Seq<Interference> interferences, string author, Instant at) =>
        interferences.Map(clash => {
            var (yields, fix) = ResolveOf(clash);
            return new ClashProposalRow(clash.First, clash.Second, yields, clash.Kind, fix, TopicOf(graph, clash, author, at));
        });

    // The A/B impact fold: the contested seed — the IFC-GlobalId intersection of two diffs this owner consumes,
    // never re-derives — propagated as a REAL reachability closure: downstream through each owning system's flow
    // graph (the settled SystemTrace fold), then transitively through the schedule DAG and the cost roll-up tree
    // via the one Successors kernel, every membership join reading the CLOSED set.
    public static ImpactReport Between(ElementGraph graph, ModelDiff before, ModelDiff after, ScheduleNetwork schedule, CostSchedule cost) {
        var touched = toHashSet(after.Changes.Map(static c => c.GlobalId));
        var contested = before.Changes.Map(static c => c.GlobalId).Filter(touched.Contains).Distinct().ToSeq();
        var byExternal = graph.ObjectNodes.Choose(static o => o.ExternalId.Map(e => (e, o.Id))).ToHashMap();
        var seeds = toHashSet(contested.Choose(byExternal.Find));
        Seq<(DistributionSystem View, LanguageExt.HashSet<NodeId> Members)> systems = DistributionNetwork.View(graph, None).Map(system => (View: system, Members: toHashSet(system.Members)));
        var ripple = toHashSet(systems.Bind(entry => entry.Members.Intersect(seeds).ToSeq()
            .Bind(seed => SystemTrace.From(entry.View, seed, TraceMode.Downstream).ReachedElements)));
        var affectedNodes = seeds.Union(ripple);
        var affected = (contested + ripple.ToSeq().Choose(id => graph.Find<Node.Object>(id).Bind(static o => o.ExternalId)))
            .Distinct().ToSeq();
        var affectedSet = toHashSet(affected);
        var assigned = schedule.Assignments
            .Filter(a => a.ElementGlobalIds.Exists(affectedSet.Contains))
            .Map(static a => a.TaskGlobalId).Distinct().ToSeq();
        var priced = cost.Items
            .Filter(i => i.PricedGlobalIds.Exists(affectedSet.Contains))
            .Map(static i => i.GlobalId).Distinct().ToSeq();
        return new ImpactReport(
            contested,
            affected,
            Successors(schedule.Dependencies.Map(static d => (d.PredecessorGlobalId, d.SuccessorGlobalId)), assigned),
            Successors(cost.Items.Choose(static i => i.ParentGlobalId.Map(parent => (i.GlobalId, parent))), priced),
            systems.Filter(entry => entry.Members.Exists(affectedNodes.Contains))
                .Choose(static entry => entry.View.ExternalId).Distinct().ToSeq());
    }

    // The rule-shape gate as the generated TOTAL Switch (no runtime-silent `_`): the closed-vocabulary predicates
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

    static RuleVerdict Verdict(ElementGraph graph, CoordinationRule rule) {
        ElementSet applicable = ElementSet.Query(graph, rule.Applicability);
        return rule.Switch(
            state:       applicable,
            require:     static (app, r) => Predicated(r, app, r.Requirement, prohibits: false),
            prohibit:    static (app, r) => Predicated(r, app, r.Requirement, prohibits: true),
            cardinality: static (app, r) => Counted(r, app),
            unique:      static (app, r) => Distinct(r, app),
            reachable:   static (app, r) => Traced(r, app));
    }

    // Require/Prohibit collapse to ONE partition derived by the `prohibits` policy bit: the matching subset
    // (re-folded over only the current members via ElementSet.Where, never an O(n^2) re-query) is the passing
    // set for Require and the violating set for Prohibit; the verdict severity rides the rule itself.
    static RuleVerdict Predicated(CoordinationRule rule, ElementSet applicable, ElementPredicate requirement, bool prohibits) {
        var matching = applicable.Where(requirement);
        var (pass, violated) = prohibits ? (applicable.Except(matching), matching) : (matching, applicable.Except(matching));
        return new RuleVerdict(rule, Ids(pass), Ids(violated));
    }

    // The set-count bound: the whole applicable set is the evidence — in range it passes, out of range it
    // violates (a storey with zero exits surfaces every applicable element so the board highlights the breach).
    static RuleVerdict Counted(CoordinationRule.Cardinality rule, ElementSet applicable) =>
        rule.Min <= applicable.Count && rule.Max.Match(Some: hi => applicable.Count <= hi, None: static () => true)
            ? new RuleVerdict(rule, Ids(applicable), Seq<NodeId>())
            : new RuleVerdict(rule, Seq<NodeId>(), Ids(applicable));

    // The uniqueness check: group the applicable set by the ValueSource value — the direct ObjectAttribute row OR
    // the effective Pset/Qto property (space numbers, door marks — the dominant coordination distinctness checks),
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

    // The graph-incidence verdict: every applicable element must reach a Target-matching element through its
    // owning DistributionSystem's Mode-oriented flow graph — the connectivity model-check (every terminal traces
    // upstream to an air-handler, every fixture to a water source) the per-element/count/uniqueness modalities
    // cannot express. Composes the settled Model/systems#SYSTEM_TRACE fold over DistributionNetwork.View and the
    // ONE selection algebra for Target; an element in no system, or whose trace reaches no target, violates —
    // the orphaned terminal IS the defect the rule surfaces (a target element passes by self-reach).
    static RuleVerdict Traced(CoordinationRule.Reachable rule, ElementSet applicable) {
        LanguageExt.HashSet<NodeId> targets = ElementSet.Query(applicable.Graph, rule.Target).Ids;
        Seq<(DistributionSystem View, LanguageExt.HashSet<NodeId> Members)> systems = DistributionNetwork.View(applicable.Graph, None)
            .Map(system => (View: system, Members: toHashSet(system.Members)));
        (Seq<NodeId> pass, Seq<NodeId> violated) = Ids(applicable).Partition(id => systems
            .Filter(entry => entry.Members.Contains(id))
            .Exists(entry => SystemTrace.From(entry.View, id, rule.Mode).ReachedElements.Exists(targets.Contains)));
        return new RuleVerdict(rule, pass, violated);
    }

    static Seq<NodeId> Ids(ElementSet set) => set.Objects.Map(static o => o.Id);

    // The proposal: the lower-priority discipline's element yields (DisciplinePriority), the fix derived from the
    // clash kind and the discipline span — a clearance graze is waived, a cross-discipline hard clash re-routes
    // the yielding service, a same-discipline hard clash re-sizes it; Sleeve/Reject are never auto-minted
    // (coordinator-elected on the board).
    static (NodeId Yields, Resolution Fix) ResolveOf(Interference clash) {
        var yields = DisciplinePriority.SecondYields(clash.FirstDomain, clash.SecondDomain) ? clash.Second : clash.First;
        Resolution fix = clash.Kind == ClashKind.Clearance
            ? new Resolution.GrantClearance(clash.Deficit)
            : clash.CrossDiscipline
                ? new Resolution.Reroute(clash.Deficit)
                : new Resolution.Resize(clash.Deficit);
        return (yields, fix);
    }

    // The write-time IDS<->BCF seam (the read-time seam is the shared ElementPredicate algebra the facet fold
    // lowers onto): one BcfTopic per NON-conforming IdsAudit — keyed on the specification name so a re-audit
    // re-lands the SAME topic, one BcfComment per failing facet naming the FacetKey and its failed count, the
    // viewpoint anchoring every failed GlobalId (already the IFC axis, no NodeId projection needed).
    public static Seq<BcfTopic> Raise(Seq<IdsAudit> audits, string author, Instant at) =>
        audits.Filter(static a => !a.Conforms).Map(a => new BcfTopic(
            $"ids-{a.Specification}", $"IDS non-conformance: {a.Specification}",
            BcfStatus.Open, "IDS", "Normal", author, at,
            a.Verdicts.Filter(static v => !v.Failed.IsEmpty).Map(v => new BcfComment(
                $"ids-{a.Specification}-{v.Facet.FacetKey}", author,
                $"{v.Facet.FacetKey}: {v.Failed.Count} failed", Option<string>.None, at)),
            Seq(new BcfViewpoint($"vp-ids-{a.Specification}", new BcfCamera.Perspective(default, default, default),
                a.Verdicts.Bind(static v => v.Failed).Distinct().ToSeq(), Seq<string>(), Option<ReadOnlyMemory<byte>>.None))));

    // The clash topic: a BcfTopic over the resolved IFC ExternalId pair, the cross-discipline clash carrying High
    // priority, the viewpoint SelectedGlobalIds the two clashing elements so the proposed fix and the BCF issue
    // carry one element identity; keyed on the clash content identity so a re-proposal of the same clash is stable.
    static BcfTopic TopicOf(ElementGraph graph, Interference clash, string author, Instant at) {
        var first = ExternalOf(graph, clash.First);
        var second = ExternalOf(graph, clash.Second);
        return new BcfTopic(
            $"clash-{clash.Identity:X32}", $"{clash.Kind.Key} clash: {first} / {second}",
            BcfStatus.Open, "Clash", clash.CrossDiscipline ? "High" : "Normal", author, at,
            Seq<BcfComment>(),
            Seq(new BcfViewpoint($"vp-{clash.Identity:X32}", new BcfCamera.Perspective(default, default, default),
                Seq(first, second), Seq<string>(), Option<ReadOnlyMemory<byte>>.None)));
    }

    // ONE parameterized closure kernel: the seeds plus every transitive successor over the edge rows — the
    // schedule SequenceRel DAG (a slipped task delays every transitive successor) and the CostItem.ParentGlobalId
    // roll-up tree (a repriced line stales every ancestor) are two edge-row inputs to one QuikGraph
    // ComputeTransitiveClosure, never two bespoke visited-set walks; AddVertexRange admits isolate seeds so an
    // unsequenced task or a root line still reports.
    static Seq<string> Successors(Seq<(string From, string To)> edges, Seq<string> seeds) {
        var dag = new AdjacencyGraph<string, SEdge<string>>();
        dag.AddVertexRange(seeds);
        foreach (var (from, to) in edges) { dag.AddVerticesAndEdge(new SEdge<string>(from, to)); }
        var closure = dag.ComputeTransitiveClosure(static (from, to) => new SEdge<string>(from, to));
        return (seeds + seeds.Bind(seed => closure.ContainsVertex(seed)
                ? toSeq(closure.OutEdges(seed)).Map(static e => e.Target)
                : Seq<string>()))
            .Distinct().ToSeq();
    }

    // The IFC GlobalId the BCF viewpoint anchors on — the Bim-stored Object ExternalId [H6], falling back to the
    // neutral NodeId for an authored element not yet emitted so the topic anchor is total.
    static string ExternalOf(ElementGraph graph, NodeId id) =>
        graph.Find<Node.Object>(id).Bind(static o => o.ExternalId).IfNone(id.Value);
}
```

## [03]-[SIGN_OFF]

- Owner: `SignOff` the `[SmartEnum<string>]` state machine over the `Review/issues#BCF_ARCHIVE` `BcfStatus` lifecycle — each case carrying its legal forward `SignOff` set as per-case delegate data (the transition table folded onto the generated case family, never a parallel `FrozenDictionary<BcfStatus,…>` the instances mirror) so the SmartEnum IS the dispatch surface and a governed workflow advances an issue through `Open → InProgress → Resolved → Closed` (with the `Reopened` re-entry) under a compile-addressable lifecycle the AppUi board references by `SignOff.Resolved`; `BcfStatus` stays the wire serialization value the `Review/issues#TS_PROJECTION` projects, `SignOff` the host-neutral transition owner over it; `IssueBoard` the host-neutral board fold over the `BcfTopic` family the `csharp:Rasm.AppUi/Collab/issues` relocation grounds here — the status lanes, the SEMANTIC-priority ordering, and the viewpoint-anchored element selection the desktop and any future head project over one contract.
- Entry: `SignOff.Advance` is the one polymorphic transition entrypoint discriminating on input shape — the instance `state.Advance(SignOff to, Op key)` reads the case's own legal forward set, the wire overload `SignOff.Advance(BcfStatus from, BcfStatus to, Op key)` resolving each value through `SignOff.Of` first so a caller holding a wire `BcfStatus` transitions through the same owner — `Fin<T>` aborting an illegal transition (`Model/faults#FAULT_BAND` `BimFault.ModelRejected`) lifted BARE, so a `Closed → InProgress` skip is rejected while a `Closed → Reopened` re-entry is admitted; `IssueBoard.Of(Seq<BcfTopic> topics)` folds the topic set into the board projection (the status partition, the priority ordering, the viewpoint-anchored selection) the AppUi head materializes, and `IssueBoard.Anchor(params ReadOnlySpan<ClashProposalRow>)` binds one proposal or a whole `Propose` run to its `BcfTopic`s in ONE re-partition — the span absorbing the single and the batch arity — so the proposed fixes land on the board.
- Auto: `Advance` reads the state's `Forward()` legal set off the case data and admits `to` only when present (`Open`→`{InProgress, Closed}`, `InProgress`→`{Resolved, Open}`, `Resolved`→`{Closed, Reopened}`, `Closed`→`{Reopened}`, `Reopened`→`{InProgress, Closed}`), each forward set a `static () => Seq(…)` delegate the `[UseDelegateFromConstructor]` generated `Forward()` binds (the self-referential forward references resolved lazily, no Func-property-plus-wrapper pair beside the case data) so a new lifecycle state is one `SignOff` case carrying its own forward set, never a second table to keep in sync; `SignOff.Of` resolves the wire `BcfStatus` to its case total (the union is complete over the enum — one case per status); `IssueBoard.Of` partitions the topic set by `BcfStatus`, orders within each partition by the `PriorityRank` SEMANTIC rank (never the lexical string — `"High" < "Low" < "Normal"` alphabetically inverts the real urgency), and projects each topic's `BcfViewpoint.SelectedGlobalIds` onto the element selection the AppUi board highlights — the AppUi `Editing/issues` consuming the `BcfTopic` contract at the package edge as a board projection and never re-minting a BCF schema.
- Receipt: the `SignOff` lifecycle is the governed sign-off workflow the `ClashProposalRow` anchors its proposed fix to (the proposal's `BcfTopic` advancing through the lifecycle as the clash is resolved) and the `Review/versioning#VERSION_GRAPH` `MergeConflict` resolution advances through; the `IssueBoard` projection the single BCF issue-board domain the desktop and any future head project over one contract; the AppUi keeps only the board projection, the durable op-log/CDE-sync store stays the `csharp:Rasm.Persistence/Version/ledger` concern joined by `ExternalId`, and Bim owns the issue-board domain over the `BcfTopic` contract.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm
- Growth: a new lifecycle state is one `SignOff` case carrying its own forward set (no second transition table to widen); a new board partition is one fold over the same `BcfTopic` set; a new priority band is one `PriorityRank` row; the `ClashProposalRow`-to-`BcfTopic` anchor folds the proposals' topics onto the board through the one `Of` partition (never a second sort, never a per-proposal re-partition); never a per-state class, never a second BCF schema, and never a board-side issue store.
- Boundary: `Rasm.Bim/coordination` owns the issue-board DOMAIN over the `Review/issues#BCF_ARCHIVE` `BcfTopic` contract and `csharp:Rasm.AppUi/Collab/issues` owns only the board projection — the AppUi head re-mints no BCF schema and reads the domain at the package edge, the `[ISSUES_RELOCATE_TO_BIM]` relocation leaving `SignOff` on a settled Bim owner; `csharp:Rasm.Persistence/Version/ledger` owns the durable op-log/CDE-sync store joined by the `Node.Object` `ExternalId` content-key, the `[ANNOTATION_RELOCATE_TO_BIM]` relocation leaving the BCF record family plus the `.bcfzip`/`BcfApi` codec wholly in `Rasm.Bim` — neither side re-mints the BCF schema across the boundary, joining only on the `ExternalId` the durable annotation row carries; the `SignOff` legal transitions are per-case data on the SmartEnum (the case's own `Forward` set) and a parallel `FrozenDictionary<BcfStatus,…>` the instances merely mirror, a per-transition method, or an unchecked status setter is the deleted form — the `[SmartEnum]` is the dispatch surface so `Advance` is its operation and an illegal advance lifts `BimFault.ModelRejected` BARE (no `.ToError()`); the `IssueBoard` reads the one `BcfTopic` family and orders by the `PriorityRank` semantic band, a lexical priority sort or a parallel board-side issue record being the deleted forms.

```csharp contract
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using System.Linq;
using LanguageExt;
using Thinktecture;
using Op = Rasm.Domain.Op;
using static LanguageExt.Prelude;

namespace Rasm.Bim.Coordination;

// --- [MODELS] -----------------------------------------------------------------------------
// The lifecycle state machine over the wire BcfStatus: each state carries its legal forward set as per-case
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
    public static SignOff Of(BcfStatus status) => Items.ToSeq().Find(s => s.Status == status).IfNone(Open);

    // ONE polymorphic transition entrypoint discriminating on input shape: the SmartEnum state advances itself,
    // the wire BcfStatus overload resolves through Of first — a Closed->InProgress skip faults while a
    // Closed->Reopened re-entry is admitted, the legal set read off the case data, never a table lookup. The
    // illegal transition lifts Model/faults#FAULT_BAND BimFault.ModelRejected BARE (Expected-derived, no .ToError()).
    public Fin<SignOff> Advance(SignOff to, Op key) =>
        Forward().Contains(to)
            ? Fin.Succ(to)
            : Fin.Fail<SignOff>(new BimFault.ModelRejected(key, $"signoff-illegal-transition:{Status}->{to.Status}"));

    public static Fin<BcfStatus> Advance(BcfStatus from, BcfStatus to, Op key) =>
        Of(from).Advance(Of(to), key).Map(static s => s.Status);
}

public sealed record IssueBoard(Map<BcfStatus, Seq<BcfTopic>> Lanes) {
    public static IssueBoard Of(Seq<BcfTopic> topics) =>
        new(topics
            .GroupBy(static t => t.Status)
            .Select(static g => (g.Key, g.OrderBy(static t => PriorityRank.Of(t.Priority)).ToSeq()))
            .ToMap());

    // Land clash proposals on the board: every proposal's minted BcfTopic folds into its status lane through ONE Of
    // re-partition — the span absorbing the single and the whole-Propose-run arity, never a per-proposal re-sort —
    // so the partition + PriorityRank ordering stay the ONE owner. Anchoring is IDEMPOTENT by topic Guid: a
    // re-proposal of the same clash (the clash-content-keyed topic id is stable) REPLACES its stale board copy,
    // never duplicates it. The element selection a viewer highlights is each topic's own
    // BcfViewpoint.SelectedGlobalIds — never re-extracted here, this owner INTEGRATES the proposals.
    public IssueBoard Anchor(params ReadOnlySpan<ClashProposalRow> proposals) {
        var incoming = toSeq(Iterable<ClashProposalRow>.FromSpan(proposals).Map(static p => p.Topic));
        var replaced = toHashSet(incoming.Map(static t => t.Guid));
        return Of(Lanes.Values.ToSeq().Bind(static lane => lane).Filter(t => !replaced.Contains(t.Guid)) + incoming);
    }
}

// --- [POLICIES] ---------------------------------------------------------------------------
// The board ordering rank: a BCF Priority is an extensible free string, but a board orders by SEMANTIC urgency,
// never alphabetically ("High" < "Low" < "Normal" lexically inverts the real order). One frozen rank row per
// canonical priority band (case-insensitive); an unknown free-text priority sorts after the known band, never throwing.
static class PriorityRank {
    static readonly FrozenDictionary<string, int> Rank = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase) {
        ["Critical"] = 0, ["High"] = 1, ["Major"] = 1, ["Normal"] = 2, ["Medium"] = 2, ["Low"] = 3, ["Minor"] = 3,
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    public static int Of(string priority) => Rank.GetValueOrDefault(priority, int.MaxValue);
}
```

## [04]-[RESEARCH]

- [RULE_PREDICATE_REUSE]: the `CoordinationRule` `[Union]` reuses the migrated `Model/query#ELEMENT_SET` `ElementPredicate` algebra verbatim — every arm's applicability is an `ElementPredicate` folded directly through `ElementSet.Query(ElementGraph, ElementPredicate)` over the seam graph (returning the `NodeSet`-backed `ElementSet`, never the retired `new ElementSet(model.Elements)` over a second stored `BimElement` collection), the `All`/`Any`/`Not` closure composing a multi-condition applicability without a second expression type; the `Require`/`Prohibit` requirement is itself an `ElementPredicate` re-folded by `ElementSet.Where` (O(current-members), never the retired O(n²) `Holds` re-query), so the validation predicate IS the query predicate IS the coordination predicate, one selection surface `Model/query#ELEMENT_SET` confirms as the shared owner ("the same `ElementPredicate` algebra backs the `Review/validation#IDS_FACETS` IDS facet fold and the `Review/coordination#COORDINATION` rules"); the prior `Recommend` arm is RETIRED as an enumerated instance of the severity axis (`Require`@`RuleSeverity.Info` — a model-check rule carries severity as a parameter of the rule, never a sibling advisory modality, so `Severity` is the abstract union member the `RuleVerdict` derives from), and the five modalities extend the prior per-element pair — `Cardinality` is a set-count bound (an applicable-set count in `[Min, Max]`, the model-check shape a per-element predicate cannot express, e.g. a storey's minimum exit count), `Unique` an attribute-distinctness check over the `Model/query#ELEMENT_SET` `ObjectAttribute` `Name`/`Tag`/`GlobalId` vocabulary (the duplicate-detection a coordination suite owns), and `Reachable` a flow-graph incidence check (every applicable element must reach a `Target`-matching element through its system's `TraceMode`-oriented closure — the connectivity model-check no per-element predicate can express, composing the settled `Model/systems#SYSTEM_TRACE` fold and the SAME two-predicate selection algebra), each one closed-union arm. The `RuleVerdict` partition mirrors the `Review/validation#IDS_FACETS` `IdsAudit` pass/fail fold SHAPE, but keys on the COMPLETE `NodeId` identity rather than the IFC `ExternalId` so a check on the working graph reports an authored element with no `GlobalId` yet — the `Model/query#ELEMENT_SET` `GlobalIds` projection (which `Choose`s over the `Option<string>` `ExternalId` and drops `None`) being the deleted form for a kernel model-check receipt.
- [CLASH_TO_BCF]: the `ClashProposal` fold consumes the migrated `Model/systems#INTERFERENCE` `Interference` ranked evidence (`First`/`Second` `NodeId`, `Kind` `ClashKind`, `Deficit`, `FirstDomain`/`SecondDomain` `IfcDomain`, the derived `CrossDiscipline`/`Rank`) — coordination consumes the systems-page clash evidence rather than re-deriving proximity, re-running the proximity test here being the named cross-page drift defect, the retired `clash.FirstGlobalId`/`clash.SecondGlobalId` string pair GONE; the proposal elects the lower-priority discipline's element to yield through the `DisciplinePriority` frozen hierarchy (structural never yields to a service), derives the fix from the kind and discipline span, and anchors the proposal to a `Review/issues#BCF_ARCHIVE` `BcfTopic` over the clash `ExternalId` pair (the `Node.Object.ExternalId` resolved through the graph, falling back to the neutral `NodeId` for an un-emitted element — the BCF viewpoint anchor total) so the proposed fix and the BCF issue carry one element identity; the `BcfTopic`/`BcfComment`/`BcfViewpoint`/`BcfStatus` family is the Bim-owned `Rasm.Bim.Coordination` surface at `Review/issues#BCF_ARCHIVE`, so the `SignOff` state machine reads a settled `BcfStatus` lifecycle and the `ImpactReport` `AffectedSystems` dimension composes the `Model/systems#CONNECTIVITY` `DistributionNetwork.View` over the same graph; the same topic-author fold lands an IDS non-conformance on the board — `Coordination.Raise` mints one `BcfTopic` per failed `Review/validation#IDS_FACETS` `IdsAudit` (a comment per failing facet, the viewpoint the failed `GlobalId` set), so IDS is the requirement-audit wire and BCF the issue-exchange wire meeting at TWO seams: the shared `ElementPredicate` algebra at read time and this audit-to-topic handoff at write time; the `DisciplinePriority` rank table covers the full seven-member `IfcDomain` roster (`Structural`/`Geotechnical`/`Infrastructure` rank 0 never yield, the `Architecture` conveying plant yields only to them, gravity `Plumbing` before `HvacFire` before the most reroutable `Electrical` — the rigidity order), so no discipline falls to the silent unranked default.
- [IMPACT_REACHABILITY]: the `ImpactReport` downstream propagation is a TRANSITIVE reachability closure over the settled owners, never a flat membership join — the flow leg composes `Model/systems#SYSTEM_TRACE` `SystemTrace.From(system, seed, TraceMode.Downstream)` (the `FlowDirection`-oriented closure over the `DistributionNetwork.View` port-and-element graph, the `libs/csharp/.api/api-quikgraph` `TreeBreadthFirstSearch` law) from every contested member, and the schedule/cost legs fold through ONE `Successors` kernel — an `AdjacencyGraph<string, SEdge<string>>` (`AddVertexRange` admitting isolate seeds, `AddVerticesAndEdge` the zero-allocation value edges) closed by `ComputeTransitiveClosure` (the api-quikgraph reachability-closure precompute) — with the `Planning/schedule#SCHEDULE` `SequenceRel` predecessor→successor DAG and the `Planning/cost#ESTIMATE` `CostItem.ParentGlobalId` child→parent roll-up tree two edge-row inputs to the one kernel; the `Reachable` rule arm reads the SAME `SystemTrace` composition (the `TraceMode` orientation vocabulary reused as a policy value, never a second trace enum), so the coordination owner carries ZERO bespoke graph walks — the `[GRAPH_ALGORITHM]` substrate owns every closure and the domain owns only the seed selection, the edge rows, and the typed receipt.
- [BCF_DOUBLE_OWNERSHIP_RESOLUTION]: the BCF double-ownership the `[COORDINATION]` idea recorded — `csharp:Rasm.Persistence/Version/ledger` also declaring a BCF backbone reading and writing 2.1/3.0 archives plus the BCF-API REST surface — resolves with Bim owning the BCF record family and the `.bcfzip`/`BcfApi` codec (the `Review/issues#BCF_ARCHIVE` owner) while Persistence keeps the durable op-log/CDE-sync store and AppUi keeps only the board projection, the three joined by the `Node.Object` `ExternalId` content-key — a `BcfViewpoint.SelectedGlobalIds`/`VisibilityExceptions` IFC GlobalId resolves to the durable annotation `Anchor` the federation entity carries, so a BCF topic and a durable annotation bind one element without this owner gaining a durable-op-log row or Persistence gaining a second BCF schema; the relocation is a cross-package seam the reconcile pass aligns (the `coordination ⇄ csharp:Rasm.Persistence/Version/ledger` and `coordination → csharp:Rasm.AppUi/Collab/issues` ARCHITECTURE seams), the Bim half realized here.
