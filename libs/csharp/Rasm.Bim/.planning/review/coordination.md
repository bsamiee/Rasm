# [BIM_COORDINATION]

The model-checking and coordination DOMAIN owner over the seam `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph`: the if-X-then-Y rule engine, the clash-resolution proposal fold, the A/B model-impact report, the BCF sign-off state machine, and the host-neutral BCF issue-board the `csharp:Rasm.Persistence/Sync/annotation` and `csharp:Rasm.AppUi/Editing/issues` relocations settle here. Each workflow COMPOSES a settled vocabulary the IDS/BCF/Diff owners supply but never assemble — the `Model/query#ELEMENT_SET` `ElementPredicate` algebra (the one selection surface), the `Model/systems#INTERFERENCE` `Interference` ranked clash evidence, the `Review/diff#MODEL_DIFF` change-sets, the `Review/issues#BCF_ARCHIVE` `BcfTopic` family, and the `Planning/schedule#SCHEDULE`/`Planning/cost#ESTIMATE` joins — and re-derives none of them: no second predicate surface, no re-run proximity test, no re-computed diff.

Identity follows the seam law [H6]: the graph is keyed by the neutral `Rasm.Element/Graph/element#NODE_MODEL` `NodeId`, and a receipt keys by the axis its consumer joins on. A kernel receipt keys on `NodeId` — the COMPLETE identity present on every node — so a `RuleVerdict` run over the WORKING graph reports an authored element that carries no IFC `GlobalId` yet, where an `ExternalId`-keyed receipt would silently drop it (the `Model/query#ELEMENT_SET` `GlobalIds` projection `Choose`s over `Option<string>` and discards `None`). An IFC-semantic receipt keys on the IFC `ExternalId` — the BCF viewpoint anchor and the impact report's schedule/cost/system join — because those targets (`Review/issues#BCF_ARCHIVE` viewpoints, `Planning/schedule#SCHEDULE` `TaskAssignment.ElementGlobalIds`, `Planning/cost#ESTIMATE` `CostItem.PricedGlobalIds`) are themselves IFC-GlobalId-keyed; the `NodeId → ExternalId` projection happens at the boundary through the Bim-stored `Node.Object.ExternalId`, never a second identity scheme.

The coordination owner is the BCF DOMAIN: `Rasm.Bim/coordination` owns the issue-board semantics over the `Review/issues#BCF_ARCHIVE` `BcfTopic`/`BcfComment`/`BcfViewpoint` family and the `.bcfzip`/`BcfApi` codec, while `csharp:Rasm.Persistence/Sync/annotation` keeps the durable op-log/CDE-sync store and `csharp:Rasm.AppUi/Editing/issues` keeps only the board projection — the three joined by the `Node.Object` `ExternalId` content-key, never a second BCF schema across the boundary. Every coordination rejection lifts the `Model/faults#FAULT_BAND` `BimFault` band BARE (the `Expected`-derived case IS the `Error`, no `.ToError()` hop), never a new fault family. The page is HOST-LOCAL.

## [01]-[INDEX]

- [01]-[COORDINATION]: the `CoordinationRule` `[Union]` (the five model-check modalities — `Require`/`Prohibit`/`Recommend`/`Cardinality`/`Unique`), the `Resolution` `[Union]`, the `RuleVerdict` `NodeId`-partition receipt, the `ClashProposalRow`, the `ImpactReport`, and the `Coordination` fold owner (`Check`/`Propose`/`Between`) over the seam `ElementGraph`.
- [02]-[SIGN_OFF]: the `SignOff` `[SmartEnum<string>]` lifecycle over the `Review/issues#BCF_ARCHIVE` `BcfStatus`, and the `IssueBoard` host-neutral board projection the `csharp:Rasm.AppUi/Editing/issues` relocation grounds here.

## [02]-[COORDINATION]

- Owner: `CoordinationRule` the closed `[Union]` of the five model-check modalities, each carrying an applicability `Model/query#ELEMENT_SET` `ElementPredicate` (the X — the SAME selection surface the `Review/validation#IDS_FACETS` IDS facet fold reads) plus its modality-specific requirement (the Y); `Resolution` the closed `[Union]` of proposed clash fixes; `RuleVerdict` the per-rule receipt folding the applicable set into the passing and the violating `NodeId` partition with the rule and its `RuleSeverity`; `ClashProposalRow` the proposed fix over one `Model/systems#INTERFERENCE` `Interference` — the clashing `NodeId` pair, the yielding endpoint, the ranked `Resolution`, and the anchored `BcfTopic`; `ImpactReport` the A/B fold over two `Review/diff#MODEL_DIFF` change-sets into the downstream-affected element/task/cost-line/system sets; `Coordination` the static fold owner (`Check`/`Propose`/`Between`) collapsing the prior `CoordinationCheck`/`ClashProposal`/`ChangeImpact` triplet into one deep coordination domain owner.
- Cases: `CoordinationRule` arms `Require` (`ElementPredicate Applicability`/`Requirement`, `RuleSeverity`) · `Prohibit` (same shape) · `Recommend` (`Applicability`/`Requirement`, advisory `Info`) · `Cardinality` (`Applicability`, `int Min`, `Option<int> Max`, `RuleSeverity` — the applicable-set count must lie in `[Min, Max]`) · `Unique` (`Applicability`, `ObjectAttribute Attribute`, `RuleSeverity` — every applicable element's attribute value distinct) (5) — a per-element predicate, a set-count bound, and an attribute uniqueness are the three irreducible model-check shapes, each one arm reusing the one selection algebra, never a per-rule-kind class and never a second predicate surface; `Resolution` arms `Reroute` (a suggested centerline offset for a linear MEP run) · `Resize` (a suggested dimension reduction for a discrete element) · `GrantClearance` (an accepted clearance exception) · `Reject` (no fix — the clash stands for a coordinator's manual review) (4).
- Entry: `Coordination.Check(ElementGraph graph, Seq<CoordinationRule> rules, Op key)` validates the rule library then folds each rule to a `RuleVerdict` over the seam graph — `Fin<T>` aborting a malformed rule (a `Cardinality` bound `Min < 0` or `Max < Min`) onto `Model/faults#FAULT_BAND` `BimFault.UnmappedClass` (the `coordination-rule` band) lifted BARE, the well-formed fold itself total; `Coordination.Propose(ElementGraph graph, Seq<Interference> interferences, string author, Instant at)` folds the ranked `Model/systems#INTERFERENCE` clash evidence into `ClashProposalRow`s — total, each clash `NodeId` resolved to its IFC `ExternalId` for the BCF anchor; `Coordination.Between(ElementGraph graph, ModelDiff before, ModelDiff after, ScheduleNetwork schedule, CostSchedule cost)` folds the two change-sets into the downstream-affected sets — total, the schedule/cost/system joins reading the IFC-GlobalId axis.
- Auto: `Check` first runs each rule through `Validate` (`TraverseM` short-circuiting the whole library on the first malformed rule) then maps the well-formed library to verdicts; `Verdict` selects the applicable set ONCE through `ElementSet.Query(graph, ElementQuery.Of(rule.Applicability))` and dispatches the generated total `Switch` — `Require`/`Prohibit`/`Recommend` collapse to ONE `Predicated` partition derived by a `prohibits` policy bit (the matching subset re-folded over only the current members via `ElementSet.Where`, never the retired O(n²) `Holds` re-query — the passing set for `Require`/`Recommend`, the violating set for `Prohibit`), `Cardinality` partitions on the applicable-set count (in range the whole set passes, out of range the whole set is the violating evidence so the board highlights every element of a storey that breaches its exit count), and `Unique` groups the applicable set by the `ObjectAttribute.Read` value (a duplicate-valued OR an unreadable-attribute member violates, the rest pass); `Propose` reads each `Interference` (consuming the systems-page ranked clash evidence rather than re-deriving proximity), folds it onto a `Resolution` through the `DisciplinePriority` hierarchy and the `ClashKind`/cross-discipline span (a clearance graze is waived `GrantClearance`, a cross-discipline hard clash re-routes the lower-priority service `Reroute`, a same-discipline hard clash re-sizes the yielding element `Resize`, `Reject` reserved for the coordinator's manual override), records the yielding endpoint, and mints a `BcfTopic` per clash anchoring the resolved `ExternalId` pair onto a `BcfViewpoint.SelectedGlobalIds`; `Between` intersects the two `ModelDiff` change-sets by IFC GlobalId, joins the affected set to the `ScheduleNetwork.Assignments` tasks naming them and the `CostSchedule.Items` lines pricing them, and resolves the affected GlobalIds to `NodeId`s (through the graph's `ExternalId` index) to surface the `Model/systems#CONNECTIVITY` `DistributionSystem`s whose member set they intersect.
- Receipt: the `Seq<RuleVerdict>` is the parameterized model-checking evidence (a coordination-rule library the IDS/BCF/Diff owners give the vocabulary for) on the `NodeId` axis the AppUi board highlights and Persistence stores; the `Seq<ClashProposalRow>` the ranked clash report with proposed fixes anchored to BCF on the IFC `ExternalId` axis a viewer round-trips; the `ImpactReport` the A/B change-impact analysis on the IFC-GlobalId axis a 4D/5D federation reads; the `SignOff` lifecycle governs the issue, so a governed workflow — rule check → clash proposal → impact report → BCF sign-off — reads one composed pipeline over the settled owners.
- Packages: Rasm.Element, Thinktecture.Runtime.Extensions, NodaTime, LanguageExt.Core, Rasm
- Growth: a new model-check modality is one `CoordinationRule` union arm reusing the `ElementPredicate` algebra (the rule library never forks a second selector); a new clash fix is one `Resolution` arm; a new proposal heuristic is one `DisciplinePriority` row, never a branch in the fold; a new impact dimension (a downstream-affected zone, a downstream-affected document) is one column on `ImpactReport` over the same diff intersection; never a per-rule-kind type, never a second selection surface, and never a re-derived proximity or diff in this owner.
- Boundary: the rule applicability/requirement is the `Model/query#ELEMENT_SET` `ElementPredicate` (the `All`/`Any`/`Not` boolean closure composing a multi-condition rule), so the validation predicate IS the query predicate IS the coordination predicate — one selection surface across `Model/query#ELEMENT_SET`, `Review/validation#IDS_FACETS`, and this owner, a parallel `RuleSelector`/`CoordinationQuery` expression type the deleted form, and the retired `new ElementSet(model.Elements)` over a second stored `BimElement` collection GONE (the rule folds the seam graph the `Projection/semantic#SEMANTIC_PROJECTOR` assembles); the `ClashProposal` fold's clash evidence is the `Model/systems#INTERFERENCE` `Interference` row carrying the `NodeId` pair (the systems page owns the geometric proximity, this owner consumes the ranked evidence and proposes the fix) — re-running the proximity test here is the named cross-page drift defect, and the retired `clash.FirstGlobalId`/`clash.SecondGlobalId` string pair is GONE, replaced by `clash.First`/`clash.Second` `NodeId` with the IFC `ExternalId` resolved only at the BCF anchor; a `RuleVerdict` keys on the COMPLETE `NodeId` identity (an `ExternalId`-keyed verdict silently dropping an un-emitted authored element is the deleted form), while the BCF anchor and the impact join key on the IFC `ExternalId` their targets demand; the `CoordinationRule`/`Resolution` unions are closed `[Union]` families and a per-kind class is the deleted form; the three coordination operations live on the ONE `Coordination` owner (the prior `CoordinationCheck`/`ClashProposal`/`ChangeImpact` single-method classes collapsed), and a coordination rejection lifts `Model/faults#FAULT_BAND` `BimFault` BARE — the `Expected`-derived case IS the `Error`, the retired `.ToError()` lowering hop and the `new BimFault.X("string")` single-arg construction GONE (the band ctor is `(Op key, string detail)`).

```csharp contract
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using System.Linq;
using LanguageExt;
using NodaTime;
using Rasm.Element;
using Thinktecture;
using Op = Rasm.Domain.Op;
using static LanguageExt.Prelude;

// The coordination DOMAIN namespace the ARCHITECTURE seams name — the Review/issues#BCF_ARCHIVE BcfTopic family
// and the SignOff lifecycle the csharp:Rasm.AppUi/Editing/issues board consumes as Rasm.Bim.Coordination.*; the
// child namespace resolves the sibling Rasm.Bim owners (ElementSet/ElementPredicate/Interference/BimFault/IfcDomain
// /ModelDiff/ScheduleNetwork/CostSchedule/DistributionNetwork) implicitly, the seam owners through Rasm.Element.
namespace Rasm.Bim.Coordination;

// --- [TYPES] ------------------------------------------------------------------------------
public enum RuleSeverity : byte { Info = 0, Warning = 1, Error = 2 }

// --- [MODELS] -----------------------------------------------------------------------------
// The if-X-then-Y model-check vocabulary: every arm carries an applicability ElementPredicate (the X — the SAME
// Model/query#ELEMENT_SET selection surface the IDS facet fold reads, never a second RuleSelector) plus its own
// modality requirement (the Y). Five modalities — a per-element requirement (Require/Prohibit/Recommend), a
// set-count bound (Cardinality), an attribute uniqueness (Unique) — the closed family a new modality lands in as
// one arm. Applicability is the abstract property every arm overrides positionally, so a rule selects without a Switch.
[Union]
public abstract partial record CoordinationRule {
    private CoordinationRule() { }

    public abstract ElementPredicate Applicability { get; }

    public sealed record Require(ElementPredicate Applicability, ElementPredicate Requirement, RuleSeverity Severity) : CoordinationRule;
    public sealed record Prohibit(ElementPredicate Applicability, ElementPredicate Requirement, RuleSeverity Severity) : CoordinationRule;
    public sealed record Recommend(ElementPredicate Applicability, ElementPredicate Requirement) : CoordinationRule;
    public sealed record Cardinality(ElementPredicate Applicability, int Min, Option<int> Max, RuleSeverity Severity) : CoordinationRule;
    public sealed record Unique(ElementPredicate Applicability, ObjectAttribute Attribute, RuleSeverity Severity) : CoordinationRule;
}

// The closed clash-fix family proposed onto a Model/systems#INTERFERENCE clash. Reroute/Resize carry the deficit
// scalar the proposal derives; Reject is the coordinator's manual override (never auto-minted by the fold).
[Union]
public abstract partial record Resolution {
    private Resolution() { }

    public sealed record Reroute(double Offset) : Resolution;
    public sealed record Resize(double Dimension) : Resolution;
    public sealed record GrantClearance(double Accepted) : Resolution;
    public sealed record Reject : Resolution;
}

// The per-rule receipt on the COMPLETE NodeId identity (present on every node pre-Emit), never the IFC ExternalId
// — a coordination check on the working graph must report an authored element that carries no GlobalId yet. The
// rule rides the receipt so the board reads which rule and which modality failed without a stringly RuleKind tag;
// a consumer raising a BCF topic from a violation projects NodeId -> ExternalId through the graph it checked.
public sealed record RuleVerdict(CoordinationRule Rule, RuleSeverity Severity, Seq<NodeId> Passed, Seq<NodeId> Violated) {
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
public sealed record ImpactReport(
    Seq<string> AffectedElements,
    Seq<string> AffectedTasks,
    Seq<string> AffectedCostLines,
    Seq<string> AffectedSystems);

// --- [POLICIES] ---------------------------------------------------------------------------
// The MEP coordination priority: the lower-priority (higher-rank) discipline's element is the one a clash
// proposal elects to move — structural never yields to a service, a flexible conduit yields before a gravity main.
// One frozen rank row per IfcDomain; a re-tuned trade hierarchy is one row edit, never a branch in the proposal fold.
static class DisciplinePriority {
    static readonly FrozenDictionary<IfcDomain, int> Rank = new Dictionary<IfcDomain, int> {
        [IfcDomain.Structural] = 0,
        [IfcDomain.Plumbing]   = 1,
        [IfcDomain.HvacFire]   = 2,
        [IfcDomain.Electrical] = 3,
    }.ToFrozenDictionary();

    static int Of(IfcDomain domain) => Rank.GetValueOrDefault(domain, int.MaxValue);

    // True when the second endpoint is the lower-or-equal-priority discipline (it yields); an equal-rank
    // same-discipline clash tie-breaks onto the second endpoint deterministically.
    public static bool SecondYields(IfcDomain first, IfcDomain second) => Of(second) >= Of(first);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
// The coordination DOMAIN owner: the rule-check fold, the clash-to-proposal fold, and the A/B impact fold — the
// three workflows the IDS/BCF/Diff owners give the vocabulary for but never compose. Each reads the seam graph
// the Projection/semantic#SEMANTIC_PROJECTOR assembles; none re-derives a selection surface, a proximity test, or a diff.
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

    // The A/B impact fold: the elements both change-sets touch (the IFC-GlobalId intersection of two diffs this
    // owner consumes, never re-derives), joined to the assigning tasks, the pricing lines, and the MEP systems
    // whose member set the change ripples into.
    public static ImpactReport Between(ElementGraph graph, ModelDiff before, ModelDiff after, ScheduleNetwork schedule, CostSchedule cost) {
        var touched = toHashSet(after.Changes.Map(GlobalIdOf));
        var affected = before.Changes.Map(GlobalIdOf).Filter(touched.Contains).Distinct().ToSeq();
        var affectedSet = toHashSet(affected);
        var tasks = schedule.Assignments
            .Filter(a => a.ElementGlobalIds.Exists(affectedSet.Contains))
            .Map(static a => a.TaskGlobalId).Distinct().ToSeq();
        var lines = cost.Items
            .Filter(i => i.PricedGlobalIds.Exists(affectedSet.Contains))
            .Map(static i => i.GlobalId).Distinct().ToSeq();
        return new ImpactReport(affected, tasks, lines, AffectedSystemsOf(graph, affected));
    }

    // The rule-shape gate as the generated TOTAL Switch (no runtime-silent `_`): the closed-vocabulary predicates
    // (ByClass/ByClassification carry already-validated IfcClass/Classification values) cannot miss, so the per-element
    // and uniqueness arms admit unconditionally — but a SIXTH modality cannot land without declaring its own shape gate
    // (the Switch breaks every site at compile time), where a `_` catch-all would silently admit it unvalidated. The
    // lone malformed shape is an impossible Cardinality bound; the band is pinned by Model/faults#FAULT_BAND.
    static Fin<CoordinationRule> Validate(CoordinationRule rule, Op key) => rule.Switch(
        state:       key,
        require:     static (_, r) => Fin.Succ<CoordinationRule>(r),
        prohibit:    static (_, r) => Fin.Succ<CoordinationRule>(r),
        recommend:   static (_, r) => Fin.Succ<CoordinationRule>(r),
        cardinality: static (k, r) => r.Min < 0 || r.Max.Match(Some: hi => hi < r.Min, None: static () => false)
                         ? new BimFault.UnmappedClass(k, $"coordination-rule:cardinality-bound:{r.Min}..{r.Max.Match(Some: h => h.ToString(), None: static () => "*")}")
                         : Fin.Succ<CoordinationRule>(r),
        unique:      static (_, r) => Fin.Succ<CoordinationRule>(r));

    static RuleVerdict Verdict(ElementGraph graph, CoordinationRule rule) {
        var applicable = ElementSet.Query(graph, ElementQuery.Of(rule.Applicability));
        return rule.Switch(
            state:       applicable,
            require:     static (app, r) => Predicated(r, app, r.Requirement, r.Severity, prohibits: false),
            prohibit:    static (app, r) => Predicated(r, app, r.Requirement, r.Severity, prohibits: true),
            recommend:   static (app, r) => Predicated(r, app, r.Requirement, RuleSeverity.Info, prohibits: false),
            cardinality: static (app, r) => Counted(r, app),
            unique:      static (app, r) => Distinct(r, app));
    }

    // Require/Prohibit/Recommend collapse to ONE partition derived by the `prohibits` policy bit: the matching
    // subset (re-folded over only the current members via ElementSet.Where, never an O(n^2) re-query) is the
    // passing set for Require/Recommend and the violating set for Prohibit.
    static RuleVerdict Predicated(CoordinationRule rule, ElementSet applicable, ElementPredicate requirement, RuleSeverity severity, bool prohibits) {
        var matching = applicable.Where(requirement);
        var (pass, violated) = prohibits ? (applicable.Except(matching), matching) : (matching, applicable.Except(matching));
        return new RuleVerdict(rule, severity, Ids(pass), Ids(violated));
    }

    // The set-count bound: the whole applicable set is the evidence — in range it passes, out of range it
    // violates (a storey with zero exits surfaces every applicable element so the board highlights the breach).
    static RuleVerdict Counted(CoordinationRule.Cardinality rule, ElementSet applicable) =>
        rule.Min <= applicable.Count && rule.Max.Match(Some: hi => applicable.Count <= hi, None: static () => true)
            ? new RuleVerdict(rule, rule.Severity, Ids(applicable), Seq<NodeId>())
            : new RuleVerdict(rule, rule.Severity, Seq<NodeId>(), Ids(applicable));

    // The uniqueness check: group the applicable set by the ObjectAttribute value (the Model/query#ELEMENT_SET
    // Name/Tag/GlobalId vocabulary). A duplicate-valued member violates; an unreadable-attribute member (None)
    // cannot prove distinctness so it violates too, the rest pass — one group fold, never a nested compare.
    static RuleVerdict Distinct(CoordinationRule.Unique rule, ElementSet applicable) {
        var keyed = applicable.Objects.Map(o => (o.Id, Key: rule.Attribute.Read(o).Map(static v => v.Render())));
        var duplicated = toHashSet(keyed.Choose(static r => r.Key)
            .GroupBy(static k => k).Where(static g => g.Count() > 1).Select(static g => g.Key));
        var violated = keyed.Filter(r => r.Key.Match(Some: duplicated.Contains, None: static () => true)).Map(static r => r.Id);
        var pass = keyed.Filter(r => r.Key.Match(Some: k => !duplicated.Contains(k), None: static () => false)).Map(static r => r.Id);
        return new RuleVerdict(rule, rule.Severity, pass, violated);
    }

    static Seq<NodeId> Ids(ElementSet set) => set.Objects.Map(static o => o.Id);

    // The proposal: the lower-priority discipline's element yields (DisciplinePriority), the fix derived from the
    // clash kind and the discipline span — a clearance graze is waived, a cross-discipline hard clash re-routes
    // the yielding service, a same-discipline hard clash re-sizes it; Reject is never auto-minted (manual override).
    static (NodeId Yields, Resolution Fix) ResolveOf(Interference clash) {
        var yields = DisciplinePriority.SecondYields(clash.FirstDomain, clash.SecondDomain) ? clash.Second : clash.First;
        Resolution fix = clash.Kind == ClashKind.Clearance
            ? new Resolution.GrantClearance(clash.Deficit)
            : clash.CrossDiscipline
                ? new Resolution.Reroute(clash.Deficit)
                : new Resolution.Resize(clash.Deficit);
        return (yields, fix);
    }

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
            Seq(new BcfViewpoint($"vp-{clash.Identity:X32}", default, default, default, 60d,
                Seq(first, second), Seq<string>(), Option<ReadOnlyMemory<byte>>.None)));
    }

    // The MEP systems a change ripples into: an affected element's NodeId (resolved from its IFC ExternalId
    // through the graph's Object index) falling in a DistributionSystem member set surfaces that system's
    // ExternalId — an air-handling system whose duct moved reads as impacted. Reuses the Model/systems#CONNECTIVITY
    // view, never a second membership walk.
    static Seq<string> AffectedSystemsOf(ElementGraph graph, Seq<string> affected) {
        var byExternal = graph.ObjectNodes.Choose(static o => o.ExternalId.Map(e => (Key: e, o.Id)))
            .ToHashMap(static p => p.Key, static p => p.Id);
        var affectedNodes = toHashSet(affected.Choose(byExternal.Find));
        return DistributionNetwork.View(graph)
            .Filter(s => s.Members.Exists(affectedNodes.Contains))
            .Choose(static s => s.ExternalId).Distinct().ToSeq();
    }

    // The IFC GlobalId the BCF viewpoint anchors on — the Bim-stored Object ExternalId [H6], falling back to the
    // neutral NodeId for an authored element not yet emitted so the topic anchor is total.
    static string ExternalOf(ElementGraph graph, NodeId id) =>
        graph.Find<Node.Object>(id).Bind(static o => o.ExternalId).IfNone(id.Value);

    static string GlobalIdOf(ElementChange change) => change.Switch(
        added:    static c => c.GlobalId, removed: static c => c.GlobalId,
        modified: static c => c.GlobalId, moved:   static c => c.GlobalId);
}
```

## [03]-[SIGN_OFF]

- Owner: `SignOff` the `[SmartEnum<string>]` state machine over the `Review/issues#BCF_ARCHIVE` `BcfStatus` lifecycle — each case carrying its legal forward `SignOff` set as per-case delegate data (the transition table folded onto the generated case family, never a parallel `FrozenDictionary<BcfStatus,…>` the instances mirror) so the SmartEnum IS the dispatch surface and a governed workflow advances an issue through `Open → InProgress → Resolved → Closed` (with the `Reopened` re-entry) under a compile-addressable lifecycle the AppUi board references by `SignOff.Resolved`; `BcfStatus` stays the wire serialization value the `Review/issues#TS_PROJECTION` projects, `SignOff` the host-neutral transition owner over it; `IssueBoard` the host-neutral board fold over the `BcfTopic` family the `csharp:Rasm.AppUi/Editing/issues` relocation grounds here — the status lanes, the SEMANTIC-priority ordering, and the viewpoint-anchored element selection the desktop and any future head project over one contract.
- Entry: `SignOff.Advance` is the one polymorphic transition entrypoint discriminating on input shape — the instance `state.Advance(SignOff to, Op key)` reads the case's own legal forward set, the wire overload `SignOff.Advance(BcfStatus from, BcfStatus to, Op key)` resolving each value through `SignOff.Of` first so a caller holding a wire `BcfStatus` transitions through the same owner — `Fin<T>` aborting an illegal transition (`Model/faults#FAULT_BAND` `BimFault.ModelRejected`) lifted BARE, so a `Closed → InProgress` skip is rejected while a `Closed → Reopened` re-entry is admitted; `IssueBoard.Of(Seq<BcfTopic> topics)` folds the topic set into the board projection (the status partition, the priority ordering, the viewpoint-anchored selection) the AppUi head materializes, and `IssueBoard.Anchor(ClashProposalRow proposal)` binds a proposal to its `BcfTopic` so the proposed fix lands on the board.
- Auto: `Advance` reads the state's `Forward` legal set off the case data and admits `to` only when present (`Open`→`{InProgress, Closed}`, `InProgress`→`{Resolved, Open}`, `Resolved`→`{Closed, Reopened}`, `Closed`→`{Reopened}`, `Reopened`→`{InProgress, Closed}`), each forward set a `static () => Seq(…)` lazy projection on the case (the self-referential forward references resolved through the delegate-backed enum behaviour Thinktecture owns) so a new lifecycle state is one `SignOff` case carrying its own forward set, never a second table to keep in sync; `SignOff.Of` resolves the wire `BcfStatus` to its case total (the union is complete over the enum — one case per status); `IssueBoard.Of` partitions the topic set by `BcfStatus`, orders within each partition by the `PriorityRank` SEMANTIC rank (never the lexical string — `"High" < "Low" < "Normal"` alphabetically inverts the real urgency), and projects each topic's `BcfViewpoint.SelectedGlobalIds` onto the element selection the AppUi board highlights — the AppUi `Editing/issues` consuming the `BcfTopic` contract at the package edge as a board projection and never re-minting a BCF schema.
- Receipt: the `SignOff` lifecycle is the governed sign-off workflow the `ClashProposalRow` anchors its proposed fix to (the proposal's `BcfTopic` advancing through the lifecycle as the clash is resolved) and the `Review/versioning#VERSION_GRAPH` `MergeConflict` resolution advances through; the `IssueBoard` projection the single BCF issue-board domain the desktop and any future head project over one contract; the AppUi keeps only the board projection, the durable op-log/CDE-sync store stays the `csharp:Rasm.Persistence/Sync/annotation` concern joined by `ExternalId`, and Bim owns the issue-board domain over the `BcfTopic` contract.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm
- Growth: a new lifecycle state is one `SignOff` case carrying its own forward set (no second transition table to widen); a new board partition is one fold over the same `BcfTopic` set; a new priority band is one `PriorityRank` row; the `ClashProposalRow`-to-`BcfTopic` anchor folds the proposal's topic onto the board through the one `Of` partition (never a second sort); never a per-state class, never a second BCF schema, and never a board-side issue store.
- Boundary: `Rasm.Bim/coordination` owns the issue-board DOMAIN over the `Review/issues#BCF_ARCHIVE` `BcfTopic` contract and `csharp:Rasm.AppUi/Editing/issues` owns only the board projection — the AppUi head re-mints no BCF schema and reads the domain at the package edge, the `[ISSUES_RELOCATE_TO_BIM]` relocation leaving `SignOff` on a settled Bim owner; `csharp:Rasm.Persistence/Sync/annotation` owns the durable op-log/CDE-sync store joined by the `Node.Object` `ExternalId` content-key, the `[ANNOTATION_RELOCATE_TO_BIM]` relocation leaving the BCF record family plus the `.bcfzip`/`BcfApi` codec wholly in `Rasm.Bim` — neither side re-mints the BCF schema across the boundary, joining only on the `ExternalId` the durable annotation row carries; the `SignOff` legal transitions are per-case data on the SmartEnum (the case's own `Forward` set) and a parallel `FrozenDictionary<BcfStatus,…>` the instances merely mirror, a per-transition method, or an unchecked status setter is the deleted form — the `[SmartEnum]` is the dispatch surface so `Advance` is its operation and an illegal advance lifts `BimFault.ModelRejected` BARE (no `.ToError()`); the `IssueBoard` reads the one `BcfTopic` family and orders by the `PriorityRank` semantic band, a lexical priority sort or a parallel board-side issue record being the deleted forms.

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
// its instance operation. The generated ctor assigns the Status and Transitions get-only properties from the
// new(...) args in declaration order; BcfStatus stays the wire value, SignOff owns the transitions.
[SmartEnum<string>]
public sealed partial class SignOff {
    public static readonly SignOff Open       = new("open",        BcfStatus.Open,       static () => Seq(InProgress, Closed));
    public static readonly SignOff InProgress = new("in-progress", BcfStatus.InProgress, static () => Seq(Resolved, Open));
    public static readonly SignOff Resolved   = new("resolved",    BcfStatus.Resolved,   static () => Seq(Closed, Reopened));
    public static readonly SignOff Closed     = new("closed",      BcfStatus.Closed,     static () => Seq(Reopened));
    public static readonly SignOff Reopened   = new("reopened",    BcfStatus.Reopened,   static () => Seq(InProgress, Closed));

    public BcfStatus Status { get; }
    Func<Seq<SignOff>> Transitions { get; }

    public Seq<SignOff> Forward => Transitions();

    // Terminal == no legal forward transition (Forward.IsEmpty), the honest derivation: the BCF lifecycle is fully
    // re-enterable (Closed itself can Reopen) so this is invariantly false today and a `== Closed` form would lie.
    public bool IsTerminal => Forward.IsEmpty;

    // Every BcfStatus maps to exactly one SignOff case (the union is complete over the wire enum), so the resolve
    // is total and never misses — an out-of-roster status degrades to Open through the IfNone fallback (the same
    // default the Review/issues#BCF_ARCHIVE codec lands an unparseable status on), never a throwing First in domain logic.
    public static SignOff Of(BcfStatus status) => Items.ToSeq().Find(s => s.Status == status).IfNone(Open);

    // ONE polymorphic transition entrypoint discriminating on input shape: the SmartEnum state advances itself,
    // the wire BcfStatus overload resolves through Of first — a Closed->InProgress skip faults while a
    // Closed->Reopened re-entry is admitted, the legal set read off the case data, never a table lookup. The
    // illegal transition lifts Model/faults#FAULT_BAND BimFault.ModelRejected BARE (Expected-derived, no .ToError()).
    public Fin<SignOff> Advance(SignOff to, Op key) =>
        Forward.Contains(to)
            ? Fin.Succ(to)
            : Fin.Fail<SignOff>(new BimFault.ModelRejected(key, $"signoff-illegal-transition:{Status}->{to.Status}"));

    public static Fin<BcfStatus> Advance(BcfStatus from, BcfStatus to, Op key) =>
        Of(from).Advance(Of(to), key).Map(static s => s.Status);
}

public sealed record IssueBoard(Map<BcfStatus, Seq<BcfTopic>> Lanes) {
    public static IssueBoard Of(Seq<BcfTopic> topics) =>
        new(topics
            .GroupBy(static t => t.Status)
            .ToMap(static g => g.Key, static g => g.OrderBy(static t => PriorityRank.Of(t.Priority)).ToSeq()));

    // Land a clash proposal on the board: the proposal's minted BcfTopic folds into its status lane through Of, so the
    // partition + PriorityRank ordering stay the ONE owner (never a second sort) and the proposed fix becomes a
    // first-class board issue. The element selection a viewer highlights is the topic's own BcfViewpoint.SelectedGlobalIds
    // (Review/issues#BCF_ARCHIVE BcfWire.Anchor projects it) — never re-extracted here, this owner INTEGRATES the proposal.
    public IssueBoard Anchor(ClashProposalRow proposal) =>
        Of(Lanes.Values.ToSeq().Bind(static lane => lane).Add(proposal.Topic));
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

- [RULE_PREDICATE_REUSE]: the `CoordinationRule` `[Union]` reuses the migrated `Model/query#ELEMENT_SET` `ElementPredicate` algebra verbatim — every arm's applicability is an `ElementPredicate` folded through `ElementSet.Query(ElementGraph, ElementQuery)` over the seam graph (returning the `NodeSet`-backed `ElementSet`, never the retired `new ElementSet(model.Elements)` over a second stored `BimElement` collection), the `All`/`Any`/`Not` closure composing a multi-condition applicability without a second expression type; the `Require`/`Prohibit` requirement is itself an `ElementPredicate` re-folded by `ElementSet.Where` (O(current-members), never the retired O(n²) `Holds` re-query), so the validation predicate IS the query predicate IS the coordination predicate, one selection surface `Model/query#ELEMENT_SET` confirms as the shared owner ("the same `ElementPredicate` algebra backs the `Review/validation#IDS_FACETS` IDS facet fold and the `Review/coordination#COORDINATION` rules"); the five modalities extend the prior three — `Cardinality` is a set-count bound (an applicable-set count in `[Min, Max]`, the model-check shape a per-element predicate cannot express, e.g. a storey's minimum exit count) and `Unique` an attribute-distinctness check over the `Model/query#ELEMENT_SET` `ObjectAttribute` `Name`/`Tag`/`GlobalId` vocabulary (the duplicate-detection a coordination suite owns), each one closed-union arm. The `RuleVerdict` partition mirrors the `Review/validation#IDS_FACETS` `IdsAudit` pass/fail fold SHAPE, but keys on the COMPLETE `NodeId` identity rather than the IFC `ExternalId` so a check on the working graph reports an authored element with no `GlobalId` yet — the `Model/query#ELEMENT_SET` `GlobalIds` projection (which `Choose`s over the `Option<string>` `ExternalId` and drops `None`) being the deleted form for a kernel model-check receipt.
- [CLASH_TO_BCF]: the `ClashProposal` fold consumes the migrated `Model/systems#INTERFERENCE` `Interference` ranked evidence (`First`/`Second` `NodeId`, `Kind` `ClashKind`, `Deficit`, `FirstDomain`/`SecondDomain` `IfcDomain`, the derived `CrossDiscipline`/`Rank`) — coordination consumes the systems-page clash evidence rather than re-deriving proximity, re-running the proximity test here being the named cross-page drift defect, the retired `clash.FirstGlobalId`/`clash.SecondGlobalId` string pair GONE; the proposal elects the lower-priority discipline's element to yield through the `DisciplinePriority` frozen hierarchy (structural never yields to a service), derives the fix from the kind and discipline span, and anchors the proposal to a `Review/issues#BCF_ARCHIVE` `BcfTopic` over the clash `ExternalId` pair (the `Node.Object.ExternalId` resolved through the graph, falling back to the neutral `NodeId` for an un-emitted element — the BCF viewpoint anchor total) so the proposed fix and the BCF issue carry one element identity; the `BcfTopic`/`BcfComment`/`BcfViewpoint`/`BcfStatus` family is the Bim-owned `Rasm.Bim.Coordination` surface at `Review/issues#BCF_ARCHIVE`, so the `SignOff` state machine reads a settled `BcfStatus` lifecycle and the `ImpactReport` `AffectedSystems` dimension composes the `Model/systems#CONNECTIVITY` `DistributionNetwork.View` over the same graph.
- [BCF_DOUBLE_OWNERSHIP_RESOLUTION]: the BCF double-ownership the `[COORDINATION]` idea recorded — `csharp:Rasm.Persistence/Sync/annotation` also declaring a BCF backbone reading and writing 2.1/3.0 archives plus the BCF-API REST surface — resolves with Bim owning the BCF record family and the `.bcfzip`/`BcfApi` codec (the `Review/issues#BCF_ARCHIVE` owner) while Persistence keeps the durable op-log/CDE-sync store and AppUi keeps only the board projection, the three joined by the `Node.Object` `ExternalId` content-key — a `BcfViewpoint.SelectedGlobalIds`/`VisibleGlobalIds` IFC GlobalId resolves to the durable annotation `Anchor` the federation entity carries, so a BCF topic and a durable annotation bind one element without this owner gaining a durable-op-log row or Persistence gaining a second BCF schema; the relocation is a cross-package seam the reconcile pass aligns (the `coordination ⇄ csharp:Rasm.Persistence/Sync/annotation` and `coordination → csharp:Rasm.AppUi/Editing/issues` ARCHITECTURE seams), the Bim half realized here.
