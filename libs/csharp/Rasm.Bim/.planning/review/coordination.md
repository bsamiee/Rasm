# [BIM_COORDINATION]

The model-checking and coordination DOMAIN owner: the if-X-then-Y rule engine, clash-resolution proposal fold, A/B model-impact report, and BCF sign-off state machine the IDS/BCF/Diff owners give the vocabulary for but never compose into a workflow — and the host-neutral BCF issue-board DOMAIN owner the `Rasm.Persistence/Sync/annotation` and `Rasm.AppUi/Editing/issues` relocations settle here. One `CoordinationRule` `[Union]` of applicability-predicate-then-requirement-predicate rows reusing the `Model/query#ELEMENT_SET` `ElementPredicate` algebra verbatim (the `All`/`Any` composite arms confirmed present), a `ClashProposal` fold over the `Model/systems#INTERFERENCE` `Interference` evidence proposing a ranked `Resolution`, an `ImpactReport` fold over two `Review/diff#MODEL_DIFF` change-sets into the downstream-affected element/schedule/cost set, and a `SignOff` `[SmartEnum]` state machine over the `Review/issues#BCF_ARCHIVE` `BcfStatus` lifecycle, all lowering onto the one `Model/faults#FAULT_BAND` `BimFault` band. The coordination owner is the BCF DOMAIN: `Rasm.Bim/coordination` owns the issue-board semantics (topic threading, status/priority/type discrimination, viewpoint-anchored selection) over the `Review/issues#BCF_ARCHIVE` `BcfTopic`/`BcfComment`/`BcfViewpoint` record family and the `.bcfzip`/`BcfApi` codec, while `Rasm.Persistence/Sync/annotation` keeps the durable op-log/CDE-sync store and `Rasm.AppUi/Editing/issues` keeps only the board projection — the three joined by the `Model/elements#ELEMENT_MODEL` `GlobalId` content-key, never a second BCF schema across the boundary. A coordination rejection lowers onto `Model/faults#FAULT_BAND` `BimFault` via `.ToError()`, never a new fault family.

## [01]-[INDEX]

- [01]-[COORDINATION]: `CoordinationRule` `[Union]` (`Require`/`Prohibit`/`Recommend` each carrying applicability and requirement `ElementPredicate`), the `ClashProposal` fold over the `Model/systems#INTERFERENCE` evidence, the `ImpactReport` fold over two `Review/diff#MODEL_DIFF` change-sets, and the rule-check fold over `BimModel`.
- [02]-[SIGN_OFF]: the `SignOff` `[SmartEnum]` state machine over the `Review/issues#BCF_ARCHIVE` `BcfStatus` transitions, the issue-board fold over the `BcfTopic` family the `Rasm.AppUi/Editing/issues` relocation grounds here, and the `ClashProposal`-to-`BcfTopic` anchor.

## [02]-[COORDINATION]

- Owner: `CoordinationRule` the closed `[Union]` of rule rows — `Require` (an applicability `ElementPredicate` then a requirement `ElementPredicate` every applicable element must satisfy), `Prohibit` (an applicability then a requirement no applicable element may satisfy), `Recommend` (a non-blocking advisory requirement) — each reusing the `Model/query#ELEMENT_SET` `ElementPredicate` algebra verbatim, never a second selection surface; `RuleVerdict` the per-rule receipt folding the applicable element set into the passing and the violating GlobalId sets with the rule kind; `ClashProposal` the fold over the `Model/systems#INTERFERENCE` `Interference` evidence proposing a ranked `Resolution` (the suggested re-route/re-size/clearance-grant) anchored to a `BcfTopic`; `Resolution` the `[Union]` of proposed fixes (`Reroute`/`Resize`/`GrantClearance`/`Reject`); `ImpactReport` the fold over two `Review/diff#MODEL_DIFF` change-sets into the downstream-affected element/schedule/cost set; `CoordinationCheck` the static fold over `BimModel`.
- Cases: `CoordinationRule` arms `Require` (`Applicability`/`Requirement` `ElementPredicate`, `Severity`) · `Prohibit` (`Applicability`/`Requirement` `ElementPredicate`, `Severity`) · `Recommend` (`Applicability`/`Requirement` `ElementPredicate`) (3) — a coordination rule carries a then-action proposal richer than an IDS pass/fail facet yet folds to the same `ElementPredicate` algebra, never a second predicate surface; `Resolution` arms `Reroute` (a suggested centerline offset) · `Resize` (a suggested dimension change) · `GrantClearance` (an accepted clearance exception) · `Reject` (no fix, the clash stands) (4).
- Entry: `CoordinationCheck.Check(BimModel model, Seq<CoordinationRule> rules)` folds each rule over the model — running the rule's applicability `ElementPredicate` through `ElementSet.Query` for the applicable set, partitioning it by the requirement `ElementPredicate` (a `Require` rule fails an applicable element NOT matching the requirement, a `Prohibit` rule fails one matching it, a `Recommend` rule reports without blocking), into one `Seq<RuleVerdict>` — `Fin<T>` aborts on a rule referencing an unknown classification or property template (`Model/faults#FAULT_BAND` `BimFault.UnmappedClass`) lowered with `.ToError()`; `ClashProposal.Propose(Seq<Interference> interferences, BcfStatus initial)` folds the `Model/systems#INTERFERENCE` ranked clash evidence into proposals — each `Interference` mapping its `ClashKind`/deficit onto a ranked `Resolution` (a hard clash proposes `Reroute`/`Resize`, a minor clearance graze proposes `GrantClearance`) and anchoring the proposal to a new `BcfTopic` over the clashing `GlobalId` pair so the proposed fix lands on a coordination issue; `ImpactReport.Between(ModelDiff before, ModelDiff after, ScheduleNetwork schedule, CostSchedule cost)` folds two change-sets into the downstream-affected set — the elements the two diffs both touch, the `Planning/schedule#SCHEDULE` tasks assigning them, and the `Planning/cost#ESTIMATE` lines pricing them — so a design change's schedule-and-cost ripple reads one report.
- Auto: `Check` reads each `CoordinationRule` and folds it to a `RuleVerdict` through the `ElementPredicate` algebra — the applicability predicate selects the applicable set through `ElementSet.Query`, the requirement predicate partitions it (`Require`: pass = matches requirement, fail = applicable minus matches; `Prohibit`: pass = applicable minus matches, fail = matches; `Recommend`: report the non-matching set without a fail), the `RuleVerdict` carrying the rule kind, the passing GlobalIds, and the violating GlobalIds — the validation predicate IS the query predicate, the coordination rule a then-action proposal over it; `ClashProposal.Propose` reads the `Model/systems#INTERFERENCE` `Interference` ranked evidence (consuming the systems-page clash evidence rather than re-deriving proximity — the `Model/systems#INTERFERENCE` `Interference` row is the `ClashProposal` substrate), maps each clash's `ClashKind`/`Deficit`/discipline pair onto the ranked `Resolution` (a cross-discipline hard clash ranks a `Reroute` above an intra-discipline `GrantClearance`), and mints a `BcfTopic` per proposal anchoring the clash `GlobalId` pair onto a `BcfViewpoint` `SelectedGlobalIds` so the proposed fix and the BCF issue carry one element identity; `ImpactReport.Between` intersects the two `ModelDiff` `ElementChange` sets by `GlobalId`, joins the affected elements to the `ScheduleNetwork` tasks (the tasks whose `TaskAssignment` names them) and the `CostSchedule` lines (the lines pricing them), and folds the downstream-affected element/schedule/cost partition so an A/B change-impact analysis reads one fold.
- Receipt: the `Seq<RuleVerdict>` is the parameterized rule-checking evidence (a coordination-rule library the IDS/BCF/Diff owners give the vocabulary for), the `ClashProposal` ranked `Resolution` the clash report with proposed fixes anchored to BCF, and the `ImpactReport` the change-impact analysis over two diffs; the `SignOff` lifecycle governs the issue, so a governed coordination workflow — rule check → clash proposal → impact report → BCF sign-off — reads one composed pipeline over the settled IDS/BCF/Diff owners.
- Packages: GeometryGymIFC_Core, Thinktecture.Runtime.Extensions, System.IO.Hashing, NodaTime, LanguageExt.Core, Rasm
- Growth: a new rule modality is one `CoordinationRule` union arm reusing the `ElementPredicate` algebra; a new resolution kind is one `Resolution` union arm; a new impact dimension (a downstream-affected zone, a downstream-affected system) is one column on `ImpactReport` over the same diff intersection; never a per-rule-kind type, never a second selection surface, and never a re-derived proximity in this owner.
- Boundary: a coordination rule carries a then-action proposal richer than an IDS pass/fail facet yet reuses the `Model/query#ELEMENT_SET` `ElementPredicate` algebra (the `All`/`Any` composite arms confirmed present), never a second selection surface — a parallel `RuleSelector`/`CoordinationQuery` expression type is the deleted form; the `ClashProposal` fold's clash evidence is the `Model/systems#INTERFERENCE` `Interference` row (the systems page owns the geometric proximity, this owner consumes the ranked clash evidence and proposes the fix) — re-running the proximity test in this owner is the named cross-page drift defect, the `MEP_INTERFERENCE_CHECK` `Interference` the `ClashProposal` substrate; the `ImpactReport` folds two `Review/diff#MODEL_DIFF` change-sets and the `Planning/schedule#SCHEDULE`/`Planning/cost#ESTIMATE` joins consumed as settled vocabulary, never a re-derived diff or schedule; the rule check, clash proposal, and impact report all lower onto the one `Model/faults#FAULT_BAND` `BimFault` band; the `CoordinationRule`/`Resolution` unions are closed `[Union]` families and a per-kind class is the deleted form; a coordination rejection lowers onto `Model/faults#FAULT_BAND` `BimFault` through `.ToError()`.

```csharp contract
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using NodaTime;
using Rasm.Bim.Coordination;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [TYPES] ------------------------------------------------------------------------------
public enum RuleSeverity : byte { Info = 0, Warning = 1, Error = 2 }

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public partial record CoordinationRule {
    partial record Require(ElementPredicate Applicability, ElementPredicate Requirement, RuleSeverity Severity);
    partial record Prohibit(ElementPredicate Applicability, ElementPredicate Requirement, RuleSeverity Severity);
    partial record Recommend(ElementPredicate Applicability, ElementPredicate Requirement);

    public ElementPredicate Applicability => Switch(
        require:   static r => r.Applicability,
        prohibit:  static r => r.Applicability,
        recommend: static r => r.Applicability);

    public ElementPredicate Requirement => Switch(
        require:   static r => r.Requirement,
        prohibit:  static r => r.Requirement,
        recommend: static r => r.Requirement);
}

[Union]
public partial record Resolution {
    partial record Reroute(double Offset);
    partial record Resize(double Dimension);
    partial record GrantClearance(double Accepted);
    partial record Reject;
}

public sealed record RuleVerdict(string RuleKind, RuleSeverity Severity, Seq<string> Passed, Seq<string> Violated) {
    public bool Conforms => Violated.IsEmpty;
}

public sealed record ClashProposalRow(string FirstGlobalId, string SecondGlobalId, ClashKind Kind, Resolution Proposed, BcfTopic Topic);

public sealed record ImpactReport(Seq<string> AffectedElements, Seq<string> AffectedTasks, Seq<string> AffectedCostLines);

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class CoordinationCheck {
    public static Fin<Seq<RuleVerdict>> Check(BimModel model, Seq<CoordinationRule> rules) =>
        Try.lift(() => rules.Map(rule => Verdict(model, rule))).Run()
            .MapFail(static error => new BimFault.UnmappedClass($"coordination-rule:{error.Message}").ToError());

    static RuleVerdict Verdict(BimModel model, CoordinationRule rule) {
        var applicable = ElementSet.Query(model, new ElementQuery(rule.Applicability));
        var matching = applicable.Where(e => Holds(model, e, rule.Requirement));
        var (pass, fail, severity) = rule.Switch(
            require:   r => (matching, applicable.Except(matching), r.Severity),
            prohibit:  r => (applicable.Except(matching), matching, r.Severity),
            recommend: _ => (applicable.Except(matching), new ElementSet(Seq<BimElement>()), RuleSeverity.Info));
        return new RuleVerdict(rule.GetType().Name, severity,
            pass.Elements.Map(static e => e.GlobalId), fail.Elements.Map(static e => e.GlobalId));
    }

    static bool Holds(BimModel model, BimElement element, ElementPredicate requirement) =>
        ElementSet.Query(model, new ElementQuery(requirement)).Elements.Exists(e => e.GlobalId == element.GlobalId);
}

public static class ClashProposal {
    public static Seq<ClashProposalRow> Propose(Seq<Interference> interferences, string author, Instant at) =>
        interferences.Map(clash => new ClashProposalRow(
            clash.FirstGlobalId, clash.SecondGlobalId, clash.Kind,
            ProposalOf(clash),
            TopicOf(clash, author, at)));

    static Resolution ProposalOf(Interference clash) =>
        clash.Kind == ClashKind.Hard
            ? (clash.CrossDiscipline ? new Resolution.Reroute(clash.Deficit) : new Resolution.Resize(clash.Deficit))
            : new Resolution.GrantClearance(clash.Deficit);

    static BcfTopic TopicOf(Interference clash, string author, Instant at) =>
        new($"clash-{clash.FirstGlobalId}-{clash.SecondGlobalId}",
            $"{clash.Kind.Key} clash: {clash.FirstGlobalId} / {clash.SecondGlobalId}",
            BcfStatus.Open, "Clash", clash.CrossDiscipline ? "High" : "Normal", author, at,
            Seq<BcfComment>(),
            Seq(new BcfViewpoint($"vp-{clash.FirstGlobalId}", default, default, default, 60d,
                Seq(clash.FirstGlobalId, clash.SecondGlobalId), Seq<string>(), Option<ReadOnlyMemory<byte>>.None)));
}

public static class ChangeImpact {
    public static ImpactReport Between(ModelDiff before, ModelDiff after, ScheduleNetwork schedule, CostSchedule cost) {
        var touched = toHashSet(after.Changes.Map(GlobalIdOf));
        var affected = before.Changes.Map(GlobalIdOf).Filter(touched.Contains).Distinct().ToSeq();
        var affectedSet = toHashSet(affected);
        var tasks = schedule.Assignments
            .Filter(a => a.ElementGlobalIds.Exists(affectedSet.Contains))
            .Map(static a => a.TaskGlobalId).Distinct().ToSeq();
        var lines = cost.Items
            .Filter(i => i.PricedGlobalIds.Exists(affectedSet.Contains))
            .Map(static i => i.GlobalId).Distinct().ToSeq();
        return new ImpactReport(affected, tasks, lines);
    }

    static string GlobalIdOf(ElementChange change) => change.Switch(
        added:    static c => c.GlobalId, removed: static c => c.GlobalId,
        modified: static c => c.GlobalId, moved:   static c => c.GlobalId);
}
```

## [03]-[SIGN_OFF]

- Owner: `SignOff` the `[SmartEnum<string>]` state machine over the `Review/issues#BCF_ARCHIVE` `BcfStatus` lifecycle — each case carrying its legal forward `SignOff` set as per-case data (the transition table folded onto the generated case family, never a parallel `FrozenDictionary<BcfStatus,…>` the instances mirror) so the SmartEnum IS the dispatch surface and a governed coordination workflow advances an issue through `Open → InProgress → Resolved → Closed` (and the `Reopened` re-entry) under a compile-checked, addressable lifecycle vocabulary the AppUi board references by `SignOff.Resolved`; `BcfStatus` stays the wire serialization value the `Review/issues#TS_PROJECTION` projects, `SignOff` the host-neutral transition owner over it; `IssueBoard` the host-neutral issue-board fold over the `BcfTopic` family the `Rasm.AppUi/Editing/issues` relocation grounds here — topic threading (the `BcfComment` set), status/priority/type discrimination, and viewpoint-anchored selection (the `BcfViewpoint` `SelectedGlobalIds`) — the issue-board DOMAIN the desktop and any future head project over one contract.
- Entry: `SignOff.Advance` is the one polymorphic transition entrypoint discriminating on input shape — the instance `state.Advance(SignOff to)` reads the case's own legal forward set, the wire overload `SignOff.Advance(BcfStatus from, BcfStatus to)` resolving each value through `SignOff.Of` first so a caller holding a wire `BcfStatus` transitions through the same owner — `Fin<T>` aborts on an illegal transition (`Model/faults#FAULT_BAND` `BimFault.ModelRejected`) lowered with `.ToError()`, so a `Closed → InProgress` skip is rejected while a `Closed → Reopened` re-entry is admitted; `IssueBoard.Of(Seq<BcfTopic> topics)` folds the BCF topic set into the board projection (the open/in-progress/resolved/closed partition, the priority ordering, the viewpoint-anchored element selection) the AppUi head materializes, and `IssueBoard.Anchor(ClashProposalRow proposal)` binds a `ClashProposal` to its `BcfTopic` so the proposed fix lands on the issue board.
- Auto: `SignOff.Advance` reads the state's `Forward` legal set off the case data and admits `to` only when present (`Open`→`{InProgress, Closed}`, `InProgress`→`{Resolved, Open}`, `Resolved`→`{Closed, Reopened}`, `Closed`→`{Reopened}`, `Reopened`→`{InProgress, Closed}`), each forward set a `static () => Seq(…)` lazy projection on the case (the self-referential forward references resolved through the delegate-backed enum behaviour Thinktecture owns) so a new lifecycle state is one `SignOff` case carrying its own forward set, never a second table to keep in sync; `IssueBoard.Of` partitions the `BcfTopic` set by `BcfStatus`, orders within each partition by the `Priority` string, and projects each topic's `BcfViewpoint` `SelectedGlobalIds` onto the element-selection the AppUi board highlights — the AppUi `Editing/issues` consuming the Bim `BcfTopic` contract at the package edge as a board projection and never re-minting a BCF schema; the board reads the one `Review/issues#BCF_ARCHIVE` `BcfTopic`/`BcfComment`/`BcfViewpoint` family, the `.bcfzip`/`BcfApi` codec staying the `Review/issues#BCF_ARCHIVE` concern.
- Receipt: the `SignOff` lifecycle is the governed sign-off workflow the `ClashProposal` anchors its proposed fix to (the proposal's `BcfTopic` advancing through the lifecycle as the clash is resolved), the `IssueBoard` projection the single BCF issue-board domain the desktop and any future head project over one contract; the AppUi keeps only the board projection, the durable op-log/CDE-sync store stays the `Rasm.Persistence/Sync/annotation` concern joined by `GlobalId`, and Bim owns the issue-board domain over the `BcfTopic` contract.
- Packages: Thinktecture.Runtime.Extensions, NodaTime, LanguageExt.Core, Rasm
- Growth: a new lifecycle state is one `SignOff` case carrying its own forward set (no second transition table to widen); a new board partition is one fold over the same `BcfTopic` set; the `ClashProposal`-to-`BcfTopic` anchor rides the existing `BcfViewpoint`; never a per-state class, never a second BCF schema, and never a board-side issue store.
- Boundary: `Rasm.Bim/coordination` owns the issue-board DOMAIN over the `Review/issues#BCF_ARCHIVE` `BcfTopic` contract and `Rasm.AppUi/Editing/issues` owns only the board projection — the AppUi head re-mints no BCF schema and reads the domain at the package edge, the `[ISSUES_RELOCATE_TO_BIM]` relocation leaving `SignOff` on a settled Bim owner; `Rasm.Persistence/Sync/annotation` owns the durable op-log/CDE-sync store joined by the `Model/elements#ELEMENT_MODEL` `GlobalId` content-key, the `[ANNOTATION_RELOCATE_TO_BIM]` relocation leaving the BCF record family plus the `.bcfzip`/`BcfApi` codec wholly in `Rasm.Bim` (the `Review/issues#BCF_ARCHIVE` owner) — neither side re-mints the BCF schema across the boundary, joining only on the `GlobalId` content-key the durable annotation row carries; the `SignOff` legal transitions are per-case data on the SmartEnum (the case's own `Forward` set) and a parallel `FrozenDictionary<BcfStatus,…>` the instances merely mirror, a per-transition method, or an unchecked status setter is the deleted form — the `[SmartEnum]` is the dispatch surface so `Advance` is its operation and an illegal advance faults; the `IssueBoard` reads the one `BcfTopic` family and a parallel board-side issue record is the named cross-package drift defect; a sign-off rejection lowers onto `Model/faults#FAULT_BAND` `BimFault` through `.ToError()`.

```csharp contract
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Linq;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Bim.Coordination;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [MODELS] -----------------------------------------------------------------------------
// The lifecycle state machine over the wire `BcfStatus`: each state carries its legal forward set as
// per-case data (the transition table folded onto the generated case family, never a parallel
// FrozenDictionary<BcfStatus,...> the instances mirror), so the SmartEnum IS the dispatch surface and
// `Advance` is its instance operation. `BcfStatus` stays the wire value; `SignOff` owns the transitions.
[SmartEnum<string>]
[KeyMemberEqualityComparer<InterchangeKeyPolicy, string>]
public sealed partial class SignOff {
    public static readonly SignOff Open       = new("open",        BcfStatus.Open,       static () => Seq(InProgress, Closed));
    public static readonly SignOff InProgress = new("in-progress", BcfStatus.InProgress, static () => Seq(Resolved, Open));
    public static readonly SignOff Resolved   = new("resolved",    BcfStatus.Resolved,   static () => Seq(Closed, Reopened));
    public static readonly SignOff Closed     = new("closed",      BcfStatus.Closed,     static () => Seq(Reopened));
    public static readonly SignOff Reopened   = new("reopened",    BcfStatus.Reopened,   static () => Seq(InProgress, Closed));

    public BcfStatus Status { get; }
    readonly Func<Seq<SignOff>> next;

    public Seq<SignOff> Forward => next();

    public static SignOff Of(BcfStatus status) => Items.First(s => s.Status == status);

    // One polymorphic transition entrypoint discriminating on input shape: the SmartEnum state advances
    // itself, the wire `BcfStatus` overload resolves through `Of` first — a Closed→InProgress skip faults
    // while a Closed→Reopened re-entry is admitted, the legal set read off the case data, never a table lookup.
    public Fin<SignOff> Advance(SignOff to) =>
        Forward.Contains(to)
            ? FinSucc(to)
            : FinFail<SignOff>(new BimFault.ModelRejected($"signoff-illegal-transition:{Status}->{to.Status}").ToError());

    public static Fin<BcfStatus> Advance(BcfStatus from, BcfStatus to) =>
        Of(from).Advance(Of(to)).Map(static s => s.Status);
}

public sealed record IssueBoard(Map<BcfStatus, Seq<BcfTopic>> Lanes) {
    public static IssueBoard Of(Seq<BcfTopic> topics) =>
        new(topics
            .GroupBy(static t => t.Status)
            .ToMap(static g => g.Key, static g => g.OrderBy(static t => t.Priority).ToSeq()));

    public Seq<string> Anchor(ClashProposalRow proposal) => proposal.Topic.Viewpoints.Bind(static v => v.SelectedGlobalIds);
}
```

## [04]-[RESEARCH]

- [RULE_PREDICATE_REUSE]: the `CoordinationRule` `[Union]` (`Require`/`Prohibit`/`Recommend`) reuses the `Model/query#ELEMENT_SET` `ElementPredicate` algebra verbatim — the applicability and requirement predicates are `ElementPredicate` values folded through `ElementSet.Query`, the `All`/`Any` composite arms (confirmed present at `Model/query#ELEMENT_SET` L28-29) composing a rule's multi-condition applicability without a second expression type; a coordination rule carries a then-action proposal richer than an IDS pass/fail facet yet folds to the same predicate algebra (never a parallel `RuleSelector`), so the validation predicate IS the query predicate IS the coordination predicate, one selection surface across `Model/query#ELEMENT_SET`, `Review/validation#IDS_FACETS`, and this owner; the `RuleVerdict` partition mirrors the `IdsSpecification.Audit` pass/fail fold so a coordination rule and an IDS requirement share the partition shape.
- [CLASH_TO_BCF]: the `ClashProposal` fold consumes the `Model/systems#INTERFERENCE` `Interference` ranked evidence (the `MEP_INTERFERENCE_CHECK` `Interference` row is the `ClashProposal` substrate, so coordination consumes the systems-page clash evidence rather than re-deriving proximity — re-running the proximity test here is the named cross-page drift defect), maps each clash's `ClashKind`/`Deficit`/discipline pair onto a ranked `Resolution`, and anchors the proposal to a `Review/issues#BCF_ARCHIVE` `BcfTopic` over the clash `GlobalId` pair (the `BcfViewpoint.SelectedGlobalIds` carrying the two clashing elements) so the proposed fix and the BCF issue carry one element identity; the `BcfTopic`/`BcfComment`/`BcfViewpoint`/`BcfStatus` family is the confirmed Bim-owned BCF surface at `Review/issues#BCF_ARCHIVE` (the `Rasm.Bim.Coordination` namespace), so the `SignOff` state machine reads a settled `BcfStatus` lifecycle and the `ClashProposal` anchors its fix to that `BcfTopic`.
- [BCF_DOUBLE_OWNERSHIP_RESOLUTION]: the BCF double-ownership the `[COORDINATION]` idea recorded — `Rasm.Persistence/Sync/annotation` also declaring a `BcfTopic`/`BcfComment`/`BcfViewpoint` backbone reading and writing BCF 2.1/3.0 archives plus the BCF-API REST surface — resolves with Bim owning the BCF record family and the `.bcfzip`/`BcfApi` codec (the `Review/issues#BCF_ARCHIVE` owner) while Persistence keeps the durable op-log/CDE-sync store and AppUi keeps only the board projection, the three joined by the `Model/elements#ELEMENT_MODEL` `GlobalId` content-key — a `BcfViewpoint.SelectedGlobalIds`/`VisibleGlobalIds` IFC `GlobalId` resolves to the `FederatedEntity` the durable annotation `Anchor` carries, so a BCF topic and a durable annotation bind one element without this owner gaining a durable-op-log row or Persistence gaining a second BCF schema; the relocation is a cross-package seam the reconcile pass aligns (the `coordination ⇄ csharp:Rasm.Persistence/Sync/annotation` and `coordination → csharp:Rasm.AppUi/Editing/issues` ARCHITECTURE seams), the Bim half realized here.
