# [BIM_SCHEDULE]

The host-neutral 4D construction-sequencing and CPM DOMAIN owner: one `ScheduleNetwork` record carrying the activity network — the `ConstructionTask` row whose `IfcTaskTime` schedule and actual start/finish fold onto NodaTime `Interval`s over the work-calendar zone, whose authored `IfcTaskTime.Completion` percent feeds the cost earned-value join, and whose `TaskKind` (`IfcTask.PredefinedType` — the construction/demolition/installation 4D modality) discriminates the playback read, the `SequenceRel` edge record carrying each `IfcRelSequence` dependency lag as a NodaTime `Period`, discriminated by the `SequenceKind` `[SmartEnum]` (`FinishToStart`/`StartToStart`/`FinishToFinish`/`StartToFinish` as a 2x2 of `FromFinish`/`ToFinish` behaviour columns, never four identical-payload union arms), the `TaskAssignment` projecting `IfcRelAssignsToProcess` to join a task to the assigned `Node.Object` set, and the `ScheduleProjection.Project` fold that folds the GeometryGym `IfcWorkPlan`→`IfcWorkSchedule`→`IfcTask` container into the typed network — the plan-to-schedule and summary-to-detail `IfcRelNests` WBS tree flattened so a nested work-breakdown orders alongside its top-level activities — plus the `WorkCalendar` working-time fold over the public `IfcWorkCalendar.ExceptionTimes` spans (the `WorkingTimes` recurrence is schema-internal in GeometryGym 25.7.30, so the work-week/shift ride the calendar record), the `CriticalPath` forward/backward-pass fold over the `SequenceRel` DAG topologically ordered through the shared `QuikGraph` `[GRAPH_ALGORITHM]` owner (`SourceFirstBidirectionalTopologicalSort` over a transient `BidirectionalGraph<string, STaggedEdge<string, SequenceRel>>` — each dependency riding its edge as the `Tag` so the pass reads predecessors and successors off `InEdges`/`OutEdges`, the same managed graph-algorithm owner the `Model/systems#SYSTEM_TRACE` reachability and `Review/versioning#VERSION_GRAPH` common-ancestor walks share, never three bespoke graph walks), and the `ConstructionState.At(Instant)` snapshot that selects the seam element set whose task `Interval.Contains` the queried instant. The schedule is a VIEW of the seam `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph`, never a re-modeled element graph and never the retired `BimModel`: a task carries its assigned-element IFC GlobalIds, the snapshot resolves them against the one `ElementGraph` through the `Model/query#ELEMENT_SET` `ByAttribute(ObjectAttribute.GlobalId, …)` predicate matching each `Node.Object.ExternalId`, and the `csharp:Rasm.AppUi/Charts` schedule report and the `csharp:Rasm.Persistence/Query/federation#FEDERATION` activity-network persistence consume the typed network by reference. The schedule is HOST-NEUTRAL — the calendar values ride NodaTime `Interval`/`Period`/`ZonedDateTime`/`LocalTime`/`IsoDayOfWeek` over the model's work-calendar zone and never a BCL `DateTime` on a public signature — and the task-to-element join is the `Model/query#ELEMENT_SET` `ElementPredicate` algebra selecting the assigned `Node.Object` set, never a second selection surface. The activity network is the `Planning/cost#ESTIMATE` 5D pairing's time axis: a `ConstructionResource` joins `Planning/schedule#SCHEDULE` `ConstructionTask` by its `GlobalId` (the 5D resource-to-activity pairing) and the task `Completion` is the schedule-performance signal the `Planning/cost#EARNED_VALUE` `EarnedValue` fold reads off the `ConstructionTask`, so this page authors the `[2]-[SCHEDULE]` `ConstructionTask` anchor the cost resource-join cluster cites by reference rather than re-deriving the activity network. The `ScheduleNetwork.Schedule` forward/backward pass is the SINGLE CPM owner the whole workspace shares: its `SequenceRel` edge set originates EITHER from this owner's `IfcWorkPlan` projection OR from a `csharp:Rasm.Persistence/Version/ledger # [WIRE]` MPXJ-parsed `ProjectFile` (the durable Primavera P6 XER / MS-Project store, parse-only — it maps each `Relation`'s `RelationType`/`Lag` onto a `SequenceRel` edge and correlates its float overlay to this owner's `ConstructionTask.GlobalId`, never the external P6 `TaskId`), so Persistence supplies edges plus the durable store while the forward/backward float pass is THIS owner's one fold — a Persistence `CpmPass` re-deriving the order, or a Bim `CriticalPath` reading a raw P6 relation, is the cross-package drift defect the relocation forbids. A schedule rejection lowers onto `Model/faults#FAULT_BAND` `BimFault` (band 2600, `Expected`-derived), the typed case lifting BARE onto the `Fin<T>` rail with no `.ToError()` hop: a task assigning, or a dependency naming, a GlobalId the network never declares is `BimFault.DanglingReference` (the seam-element `task-assigns-absent-element`, the dependency `schedule-dependency-absent-process`), a cyclic dependency the forward pass cannot topologically order or a task finishing before it starts is `BimFault.ModelRejected`, never a sixth arm.

## [01]-[INDEX]

- [01]-[SCHEDULE]: `ScheduleNetwork` record, the `ConstructionTask` record (`IfcTaskTime` as a NodaTime `Interval` + the authored `Completion`), the `SequenceRel` edge record carrying a `Period` lag discriminated by the `SequenceKind` `[SmartEnum]` (the FS/SS/FF/SF 2x2 modality with `FromFinish`/`ToFinish` behaviour columns), the `TaskAssignment` record, the `TaskStatus`/`TaskKind`/`WorkScheduleKind` `[SmartEnum]` vocabularies, the `ScheduleProjection.Project` fold from the GeometryGym `IfcWorkPlan`→`IfcWorkSchedule`→`IfcTask` surface (the `IfcRelNests` WBS tree flattened), and the `ConstructionState.At(Instant)` seam-element snapshot.
- [02]-[CRITICAL_PATH]: the `WorkCalendar` working-time fold over the public `IfcWorkCalendar.ExceptionTimes` spans (`DateInterval` exception spans, `AnnualDate` recurring-holiday rows, the `WorkingBetween` working-content measure feeding the pass — the `WorkingTimes` recurrence is schema-internal, per `[04]-[WORK_CALENDAR_RECURRENCE]`), the `CriticalPath` value record per task, and the `ScheduleNetwork.Schedule` forward/backward-pass CPM fold over the `SequenceRel`-tagged DAG by the `QuikGraph` `SourceFirstBidirectionalTopologicalSort` Kahn orders (source-first forward, sink-first backward) — the SINGLE CPM owner both the IFC and the MPXJ lanes feed.

## [02]-[SCHEDULE]

- Owner: `ScheduleNetwork` the single host-neutral 4D activity-network record carrying the task set, the dependency-edge set, the assignment set, the work-calendar zone the calendar values resolve against, and the `(GeometryKey, ScheduleKey)` content-key identity the AppUi report and Persistence federation read it by; `ConstructionTask` the task row promoting one `IfcTask`/`IfcTaskTime` into a first-class owner carrying its stable `GlobalId`, name, `TaskStatus`, its `TaskKind` (the `IfcTask.PredefinedType` 4D modality — a `DEMOLITION` task's elements leave the model at its finish where a `CONSTRUCTION` task's arrive, the AppUi playback discriminant), milestone flag, the scheduled `Interval` and the optional actual `Interval` (both over the model's work-calendar zone), the optional authored `Completion` percent, and the `WorkScheduleKind` the controlling `IfcWorkSchedule` predefines; `SequenceRel` the ONE dependency-edge record carrying its predecessor/successor task GlobalIds and the dependency `Period` lag the `IfcRelSequence.TimeLag` `IfcLagTime` declares, discriminated by `SequenceKind` the `[SmartEnum<string>]` over the four IFC modalities — `FinishToStart` (the predecessor's finish gates the successor's start), `StartToStart` (the two starts gated together), `FinishToFinish` (the two finishes gated together), `StartToFinish` (the predecessor's start gates the successor's finish, the just-in-time edge) — collapsed to a closed 2x2 of two behaviour columns (`FromFinish` anchoring the predecessor finish vs start, `ToFinish` anchoring the successor finish vs start) the CPM shift reads, never four identical-payload union arms; `TaskAssignment` the `IfcRelAssignsToProcess` row joining one task to the assigned `Node.Object` GlobalId set; `TaskStatus` the `[SmartEnum<string>]` over the IFC task status; `TaskKind` the `[SmartEnum<string>]` over `IfcTaskTypeEnum`; `WorkScheduleKind` the `[SmartEnum<string>]` over `IfcWorkScheduleTypeEnum`; `ConstructionState` the snapshot owner folding the network at one `Instant` into a `Model/query#ELEMENT_SET` `ElementSet`; `ScheduleProjection` the static fold over the GeometryGym `IfcWorkPlan` container.
- Cases: `SequenceKind` rows `FinishToStart` · `StartToStart` · `FinishToFinish` · `StartToFinish` (4), each a `(FromFinish, ToFinish)` cell of the 2x2 modality space, and `SequenceRel` the one `(PredecessorGlobalId, SuccessorGlobalId, Period Lag, SequenceKind Kind)` edge record — the `IfcSequenceEnum.START_FINISH` member resolves to the first-class `StartToFinish` row rather than collapsing onto `FinishToStart`, and the `USERDEFINED`/`NOTDEFINED` fallbacks fold onto `FinishToStart` through the `SequenceKind.Of` resolver so an unknown modality defaults to the dominant edge without minting a parallel edge type; the `ConstructionTask` row carries its `GlobalId`, `Name`, `TaskStatus`, `TaskKind`, `bool IsMilestone`, the `Interval Scheduled` schedule window, the `Option<Interval> Actual` actual window (present once the task carries an `ActualStart`/`ActualFinish`), the `Option<double> PercentComplete` (the `IfcTaskTime.Completion` ratio, `None` when unset so the earned-value fold falls back to the actual-interval fraction), and the `WorkScheduleKind` — a milestone is a zero-duration `Interval` whose start equals its finish, while a spanning activity carries a positive-duration `Interval`; the `TaskAssignment` row carries its `TaskGlobalId` and the `Seq<string>` of assigned-element GlobalIds the `IfcRelAssignsToProcess.RelatedObjects` `IfcProduct` set names.
- Entry: `ScheduleProjection.Project(IfcWorkPlan plan, ElementGraph graph, DateTimeZone zone, Op key)` folds one GeometryGym work-plan container into one `ScheduleNetwork` — materializing the plan's nested `IfcWorkSchedule` set once through the `IfcRelNests` decomposition, folding each schedule's controlled `IfcTask` set AND each task's nested sub-task tree onto flat `ConstructionTask` rows (reading the `TaskTime` `IfcTaskTime` schedule/actual start-finish onto the `Interval`s over `zone`, the `Completion` onto the percent, and the `Status`/`PredefinedType`/`IsMilestone` onto the typed members), folding each task's `IsPredecessorTo` `IfcRelSequence` set onto `SequenceRel` edges (resolving `SequenceType` onto the `SequenceKind` modality through `SequenceRel.Of` and reading the `TimeLag` `IfcLagTime` onto the `Period`), folding each task's `OperatesOn` `IfcRelAssignsToProcess` set onto `TaskAssignment` rows binding the assigned-`IfcProduct` GlobalIds, and deriving the `(GeometryKey, ScheduleKey)` identity — and `ScheduleProjection.ProjectAll(Seq<IfcWorkPlan> plans, ElementGraph graph, DateTimeZone zone, Op key)` lifts every work plan in a model onto the `Seq<ScheduleNetwork>` the report reads; `Fin<T>` aborts on a task assigning a product GlobalId the seam graph never declares (`Model/faults#FAULT_BAND` `BimFault.DanglingReference`) or a task whose schedule finish precedes its start (`BimFault.ModelRejected`), the typed case lifting bare. `ConstructionState.At(ScheduleNetwork network, ElementGraph graph, Instant instant)` reads the element set whose task `Interval.Contains(instant)` — folding the network's tasks to the active set (a task is active when its effective `Interval` — the actual window when present, else the scheduled window — contains the instant), unioning each active task's `TaskAssignment` element GlobalIds, and resolving them against the seam graph through the `Model/query#ELEMENT_SET` `ByAttribute(ObjectAttribute.GlobalId, ValueMatch.OneOf(…))` predicate into one `ElementSet`, so the 4D playback at instant `t` is one fold over the network into one query, never an enumerated per-task arm and never a second store.
- Auto: `Project` reads the `IfcWorkPlan` runtime graph and folds it into the typed network — `SchedulesOf` materializes the plan's nested `IfcWorkSchedule` set once through `plan.IsNestedBy` `IfcRelNests.RelatedObjects` (the GeometryGym work-control decomposition path, distinct from `Controls` which reaches tasks), `ControlledTasks` materializes each schedule's controlled `IfcTask` set through `schedule.Controls` `IfcRelAssignsToControl.RelatedObjects` and flattens each task's `IsNestedBy` `IfcRelNests` sub-task tree so a summary→detail WBS folds onto the flat row set the CPM orders, and `TasksOf` threads each row's `WorkScheduleKind` from the owning schedule's `PredefinedType` and dedups on `GlobalId` so a task reached through both control and nesting orders once; the `TaskOf` projection reads `IfcTask.TaskTime` onto the scheduled `Interval` through `IntervalOf(taskTime.ScheduleStart, taskTime.ScheduleFinish, zone, key)`, the optional actual `Interval` through `ActualOf(taskTime.ActualStart, taskTime.ActualFinish, zone)`, and the `IfcTaskTime.Completion` onto the optional percent (each BCL `DateTime` lifting to a NodaTime `LocalDateTime` via `LocalDateTime.FromDateTime`, mapping into `zone` leniently through `InZoneLeniently` to absorb the daylight-transition gap/overlap an IFC calendar value carries no offset for, and projecting to the `Instant` the `Interval` bounds); `SequencesOf` folds each task's `IsPredecessorTo` `IfcRelSequence` set onto `SequenceRel` edges discriminating `SequenceType` through `SequenceRel.Of` and reading the `TimeLag` `IfcLagTime.LagValue` `IfcDuration` onto the `Period` through `PeriodOf` (building the NodaTime `Period` from the decomposed `Years`/`Months`/`Days`/`Hours`/`Minutes`/`Seconds` integer fields, never a fragile `ToString()` round-trip); `AssignmentsOf` folds each task's `OperatesOn` `IfcRelAssignsToProcess` set onto `TaskAssignment` rows reading the `RelatedObjects` `IfcProduct` GlobalIds; the `ScheduleNetwork.BindAssignments` fold resolves each assignment's element GlobalIds against the seam `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph.ObjectNodes` `ExternalId` index so a task assigning a product the graph never declares aborts the projection, and the `ScheduleNetwork.Identity` fold derives the `(GeometryKey, ScheduleKey)` `UInt128` pair the AppUi report and Persistence federation read the network by — `GeometryKey` over the ordinally-sorted assigned-element GlobalIds through `XxHash128.HashToUInt128` so the report re-reads only a network whose assigned geometry changed, and `ScheduleKey` over the ordinally-sorted task effective-`Interval`/status/authored-percent/`TaskKind`/`WorkScheduleKind`/milestone rows and the kind-discriminated dependency-edge lags so a re-sequenced (a re-kinded SS→FF edge included), re-statused, re-progressed, or re-kinded (a CONSTRUCTION task re-authored DEMOLITION — the 4D playback discriminant) plan re-renders — the canonical sort making both keys invariant to the unstable `IfcSet` iteration order a re-parse yields. `ConstructionState.At` folds the network at the instant: the `active` fold filters the task set to those whose effective `Interval` contains the instant through `Interval.Contains`, the `assigned` fold unions the active tasks' `TaskAssignment` element GlobalIds into one distinct GlobalId set, and the `Model/query#ELEMENT_SET` `ElementSet.Query` over the `ByAttribute(ObjectAttribute.GlobalId, ValueMatch.OneOf(assigned))` predicate resolves the active-element set against the seam graph — so a Gantt scrub at instant `t` materializes the in-progress element set as a `Model/query#ELEMENT_SET` value the AppUi viewport renders.
- Receipt: the `Seq<ScheduleNetwork>` is the 4D activity-network evidence the `csharp:Rasm.AppUi/Charts` schedule report reads by the `(GeometryKey, ScheduleKey)` reference and the `csharp:Rasm.Persistence/Query/federation#FEDERATION` activity-network persistence stores by GlobalId, the `ConstructionState.At(Instant)` snapshot is the `Model/query#ELEMENT_SET` `ElementSet` the AppUi 4D viewport renders at a Gantt instant, and the `ConstructionTask` row is the `Planning/cost#ESTIMATE` 5D cost pairing's time axis the `ConstructionResource` joins by `GlobalId` and the `Planning/cost#EARNED_VALUE` fold reads the `Completion` percent off; a scheduled milestone task, a start-to-finish dependency lag, and an assigned-element set each carry their typed calendar value on one network record, and the `TaskKind` row is the construct-vs-demolish modality the 4D playback discriminates per task.
- Packages: GeometryGymIFC_Core, NodaTime, Thinktecture.Runtime.Extensions, System.IO.Hashing, LanguageExt.Core, Rasm.Element, Rasm
- Growth: a new dependency modality is one `SequenceKind` row (its `FromFinish`/`ToFinish` cell) the column-driven CPM shift reads with zero new arm, reached from the next `IfcSequenceEnum` member; a new task status is one `TaskStatus` row; a new task kind is one `TaskKind` row reading the next `IfcTaskTypeEnum` member; a new work-schedule kind is one `WorkScheduleKind` row reading the next `IfcWorkScheduleTypeEnum` member; a new task time field (the `IfcTaskTime` `RemainingTime`, `StatusTime`) is one column on `ConstructionTask` read by the same fold; a new work plan rides the existing `ProjectAll` fold on one row; never a per-task-status record, never a second schedule store, never a `Get<Status>` task family, and never a re-modeled element graph on the schedule.
- Boundary: the dependency edge is ONE `SequenceRel` record discriminated by the `SequenceKind` `[SmartEnum]` over the four modalities — a `FinishToStartRel`/`StartToStartRel`/`FinishToFinishRel`/`StartToFinishRel` record family, four identical-payload union arms with triplicated `PredecessorGlobalId`/`SuccessorGlobalId`/`Lag` accessor switches, or four sibling factory methods is the deleted form mirroring the no-per-element-class law at `Model/elements#IFC_CLASS`, the modality variation carrying NO payload variation so it is a discriminant value not a case shape; the `ConstructionTask` carries its calendar value as a NodaTime `Interval` over the model's work-calendar `DateTimeZone` and a BCL `DateTime`/`DateTimeOffset` field on the task or a public projection signature is the named host-neutrality defect — the IFC `DateTime` crosses the projection boundary once at `IntervalOf` and never reaches a domain signature, the schedule consuming the full NodaTime `Interval`/`Period`/`ZonedDateTime`/`LocalDateTime`/`Interval.Contains` surface for the calendar arithmetic; the GeometryGym `IfcWorkPlan`/`IfcWorkSchedule`/`IfcTask`/`IfcTaskTime`/`IfcRelSequence`/`IfcLagTime`/`IfcRelAssignsToProcess`/`IfcRelNests` surface is consumed as settled vocabulary through the `IfcProcess`/`IfcTask` discrimination and a hand-rolled task reader is the deleted form; the plan→schedule link is the `IfcRelNests` decomposition (`IsNestedBy`) and reading schedules off `Controls` (the task-control path) is the named projection defect, while the WBS sub-task tree is the `IfcTask.IsNestedBy` recursion and a flat top-level-only task read is the deleted form; the task-to-element join is the seam `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph` resolved through the `Model/query#ELEMENT_SET` `ByAttribute(ObjectAttribute.GlobalId, …)` predicate matching each `Node.Object.ExternalId`, and the retired `BimModel`/`BimElement` element record or a `new ElementSet(model.Elements)` over a second store is the deleted form — the schedule produces the assigned GlobalIds, the query owns the resolution, and a parallel schedule-element selection arm is the no-second-selection-surface reject; the `(GeometryKey, ScheduleKey)` content-key identity is derived through the `Review/diff#MODEL_DIFF` `XxHash128.HashToUInt128` idiom over ordinally-sorted id/row sets (invariant to the unstable `IfcSet` iteration order) and the AppUi report and Persistence federation read the network by that reference — minting a second identity scheme for the report join is the named cross-folder drift defect; the `Planning/cost#ESTIMATE` 5D resource-join cluster cites this `[2]-[SCHEDULE]` `ConstructionTask` anchor by reference and re-deriving the activity network on the cost page is the deleted form; a schedule rejection lifts the typed `BimFault` case BARE onto the `Fin<T>` rail and a `.ToError()` lowering hop or a one-arg `new BimFault.X(detail)` ctor bypassing the kernel `Op` context is the named seam defect.

```csharp contract
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.IO.Hashing;
using System.Text;
using GeometryGym.Ifc;
using LanguageExt;
using Rasm.Bim.Model;
using NodaTime;
using NodaTime.Text;
using Rasm.Element.Graph;
using Thinktecture;
using Op = Rasm.Domain.Op;                            // the kernel operation key each typed BimFault case carries
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class TaskStatus {
    public static readonly TaskStatus NotYetStarted = new("NOTYETSTARTED");
    public static readonly TaskStatus Started       = new("STARTED");
    public static readonly TaskStatus Completed     = new("COMPLETED");
    public static readonly TaskStatus Delayed       = new("DELAYED");
    public static readonly TaskStatus NotDefined    = new("NOTDEFINED");

    // IfcTask.Status is a free IfcLabel string, so the resolver trims/normalizes and lands an unrostered status on
    // NotDefined rather than aborting — the closed vocabulary widens by one row, never a parallel status record.
    // The ONE Option-lift over the generated bool TryGet(string?, out T?) — the settled corpus idiom
    // (elements/spatial/zones pattern); the Option-returning form is NOT a generated member.
    public static Option<TaskStatus> TryGet(string key) =>
        TryGet(key, out TaskStatus? row) && row is { } hit ? Some(hit) : None;

    public static TaskStatus Of(string? status) =>
        TryGet((status ?? "").Trim().ToUpperInvariant()).IfNone(NotDefined);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class WorkScheduleKind {
    public static readonly WorkScheduleKind Actual      = new("ACTUAL");
    public static readonly WorkScheduleKind Baseline    = new("BASELINE");
    public static readonly WorkScheduleKind Planned     = new("PLANNED");
    public static readonly WorkScheduleKind UserDefined = new("USERDEFINED");
    public static readonly WorkScheduleKind NotDefined  = new("NOTDEFINED");

    public static Option<WorkScheduleKind> TryGet(string key) =>
        TryGet(key, out WorkScheduleKind? row) && row is { } hit ? Some(hit) : None;

    public static WorkScheduleKind Of(IfcWorkScheduleTypeEnum kind) =>
        TryGet(kind.ToString()).IfNone(NotDefined);
}

// The 4D task-kind vocabulary (IfcTask.PredefinedType, IfcTaskTypeEnum): construction vs demolition vs temporary
// works is the playback modality — a DEMOLITION/REMOVAL task's elements leave the model at its finish where a
// CONSTRUCTION/INSTALLATION task's arrive — so the kind rides the task row and the AppUi 4D read discriminates on
// it, never a task-name heuristic; an unrostered future member lands NotDefined through the generated TryGet.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class TaskKind {
    public static readonly TaskKind Attendance   = new("ATTENDANCE");
    public static readonly TaskKind Construction = new("CONSTRUCTION");
    public static readonly TaskKind Demolition   = new("DEMOLITION");
    public static readonly TaskKind Dismantle    = new("DISMANTLE");
    public static readonly TaskKind Disposal     = new("DISPOSAL");
    public static readonly TaskKind Installation = new("INSTALLATION");
    public static readonly TaskKind Logistic     = new("LOGISTIC");
    public static readonly TaskKind Maintenance  = new("MAINTENANCE");
    public static readonly TaskKind Move         = new("MOVE");
    public static readonly TaskKind Operation    = new("OPERATION");
    public static readonly TaskKind Removal      = new("REMOVAL");
    public static readonly TaskKind Renovation   = new("RENOVATION");
    public static readonly TaskKind UserDefined  = new("USERDEFINED");
    public static readonly TaskKind NotDefined   = new("NOTDEFINED");

    public static Option<TaskKind> TryGet(string key) =>
        TryGet(key, out TaskKind? row) && row is { } hit ? Some(hit) : None;

    public static TaskKind Of(IfcTaskTypeEnum kind) =>
        TryGet(kind.ToString()).IfNone(NotDefined);
}

// The CPM dependency-modality vocabulary the IfcSequenceEnum carries — a closed 2x2 over {anchor the predecessor
// EARLY FINISH vs EARLY START} x {anchor the successor FINISH vs START}, so the four modalities are ONE wire-keyed
// [SmartEnum] discriminant with two behaviour columns the CPM shift reads, never four identical-payload union arms
// with triplicated accessors. START_FINISH stays first-class (the just-in-time edge anchoring the successor finish
// to the predecessor start); the USERDEFINED/NOTDEFINED fallbacks fold onto FinishToStart, the dominant edge.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class SequenceKind {
    public static readonly SequenceKind FinishToStart  = new("FINISH_START",  fromFinish: true,  toFinish: false);
    public static readonly SequenceKind StartToStart   = new("START_START",   fromFinish: false, toFinish: false);
    public static readonly SequenceKind FinishToFinish = new("FINISH_FINISH", fromFinish: true,  toFinish: true);
    public static readonly SequenceKind StartToFinish  = new("START_FINISH",  fromFinish: false, toFinish: true);

    // FromFinish anchors the predecessor EARLY FINISH (else EARLY START); ToFinish anchors the successor FINISH, off
    // which the CPM recedes the successor working content for its early start — together the full 2x2 modality space.
    public bool FromFinish { get; }
    public bool ToFinish { get; }

    public static SequenceKind Of(IfcSequenceEnum kind) => kind switch {
        IfcSequenceEnum.START_START   => StartToStart,
        IfcSequenceEnum.FINISH_FINISH => FinishToFinish,
        IfcSequenceEnum.START_FINISH  => StartToFinish,
        _                             => FinishToStart,
    };
}

// --- [MODELS] -----------------------------------------------------------------------------
// ONE dependency-edge record discriminated by the SequenceKind modality — the predecessor/successor task GlobalIds
// and the IfcLagTime lag as a NodaTime Period applied as elapsed calendar time anchored in the model zone (a
// months/years lag resolves by calendar arithmetic, never the throwing Period.ToDuration fixed-unit path). A
// FinishToStartRel/StartToStartRel/… record family or four sibling factories is the deleted form.
public sealed record SequenceRel(string PredecessorGlobalId, string SuccessorGlobalId, Period Lag, SequenceKind Kind) {
    public static SequenceRel Of(IfcSequenceEnum kind, string predecessor, string successor, Period lag) =>
        new(predecessor, successor, lag, SequenceKind.Of(kind));
}

public sealed record ConstructionTask(
    string GlobalId,
    string Name,
    TaskStatus Status,
    TaskKind Kind,
    WorkScheduleKind ScheduleKind,
    bool IsMilestone,
    Interval Scheduled,
    Option<Interval> Actual,
    Option<double> PercentComplete) {
    public Interval Effective => Actual.IfNone(Scheduled);

    public bool ActiveAt(Instant instant) => Effective.Contains(instant);
}

public sealed record TaskAssignment(string TaskGlobalId, Seq<string> ElementGlobalIds);

public sealed record ScheduleNetwork(
    string GlobalId,
    string Name,
    DateTimeZone Zone,
    Seq<ConstructionTask> Tasks,
    Seq<SequenceRel> Dependencies,
    Seq<TaskAssignment> Assignments) {
    public (UInt128 GeometryKey, UInt128 ScheduleKey) Identity => (
        XxHash128.HashToUInt128(Encoding.UTF8.GetBytes(
            string.Join("\n", Assignments.Bind(static a => a.ElementGlobalIds).OrderBy(static id => id, StringComparer.Ordinal)))),
        XxHash128.HashToUInt128(Encoding.UTF8.GetBytes(
            string.Join("\n", Tasks
                    .Map(static t => $"{t.GlobalId}={t.Effective.Start.ToUnixTimeTicks()}-{t.Effective.End.ToUnixTimeTicks()}:{t.Status.Key}:{t.PercentComplete.IfNone(0d):R}:{t.Kind.Key}:{t.ScheduleKind.Key}:{(t.IsMilestone ? 1 : 0)}")
                    .OrderBy(static row => row, StringComparer.Ordinal))
                + "\u001f"
                + string.Join("\n", Dependencies
                    .Map(static d => $"{d.PredecessorGlobalId}>{d.SuccessorGlobalId}:{d.Kind.Key}:{PeriodPattern.Roundtrip.Format(d.Lag)}")
                    .OrderBy(static row => row, StringComparer.Ordinal)))));

    // The assigned-element GlobalIds resolve against the seam graph's Object-node ExternalId index (the IFC GlobalId
    // is a Bim-stored Object attribute, never the neutral NodeId); an assignment naming an undeclared product aborts.
    public Fin<ScheduleNetwork> BindAssignments(ElementGraph graph, Op key) {
        var index = toHashSet(graph.ObjectNodes.Choose(static o => o.ExternalId));
        return Assignments
            .Bind(static a => a.ElementGlobalIds)
            .Find(id => !index.Contains(id))
            .Match(
                Some: id => FinFail<ScheduleNetwork>(new BimFault.DanglingReference(key, $"task-assigns-absent-element:{id}")),
                None: () => FinSucc(this));
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class ScheduleProjection {
    public static Fin<ScheduleNetwork> Project(IfcWorkPlan plan, ElementGraph graph, DateTimeZone zone, Op key) {
        // IfcTask.TaskTime is schema-OPTIONAL: a dateless row (a P6 WBS container imported as a summary IfcTask) is
        // structure, not an activity — it projects NO ConstructionTask (its dated leaves already flatten onto the row
        // set), while a dependency or assignment naming a dateless row faults typed at the CPM/binding gates.
        var tasks = TasksOf(plan).Filter(static entry => entry.Task.TaskTime is { } time && time.ScheduleStart != default);
        return tasks
            .TraverseM(entry => TaskOf(entry.Task, entry.Kind, zone, key))
            .As()
            .Map(rows => new ScheduleNetwork(
                plan.GlobalId,
                plan.Name ?? "",
                zone,
                rows,
                SequencesOf(tasks.Map(static entry => entry.Task)),
                AssignmentsOf(tasks.Map(static entry => entry.Task))))
            .Bind(network => network.BindAssignments(graph, key));
    }

    public static Fin<Seq<ScheduleNetwork>> ProjectAll(Seq<IfcWorkPlan> plans, ElementGraph graph, DateTimeZone zone, Op key) =>
        plans.TraverseM(plan => Project(plan, graph, zone, key)).As();

    // A work plan decomposes its schedules through IfcRelNests (IsNestedBy), NOT Controls — Controls reaches the
    // task-control assignment; the plan→schedule edge is the work-control nesting GeometryGym's own traversal reads.
    static Seq<IfcWorkSchedule> SchedulesOf(IfcWorkPlan plan) =>
        plan.IsNestedBy
            .AsIterable()
            .SelectMany(static rel => rel.RelatedObjects.AsIterable())
            .OfType<IfcWorkSchedule>()
            .ToSeq();

    // Each schedule's controlled tasks plus every nested sub-task (the IfcRelNests WBS tree) flattened, paired with
    // the owning schedule's kind, deduped by GlobalId so a task reached through both control and nesting orders once.
    static Seq<(IfcTask Task, WorkScheduleKind Kind)> TasksOf(IfcWorkPlan plan) =>
        SchedulesOf(plan)
            .AsIterable()
            .SelectMany(static schedule => ControlledTasks(schedule)
                .Map(task => (Task: task, Kind: WorkScheduleKind.Of(schedule.PredefinedType))))
            .DistinctBy(static entry => entry.Task.GlobalId)
            .ToSeq();

    static Seq<IfcTask> ControlledTasks(IfcWorkSchedule schedule) =>
        schedule.Controls
            .AsIterable()
            .SelectMany(static rel => rel.RelatedObjects.AsIterable())
            .OfType<IfcTask>()
            .SelectMany(NestedTasks)
            .ToSeq();

    // The summary→detail work-breakdown: a task plus the transitive closure of its IfcRelNests sub-tasks, so a
    // P6/MS-Project WBS imported as nested IfcTasks orders as flat activities alongside its top-level rows.
    static Seq<IfcTask> NestedTasks(IfcTask task) =>
        Seq(task) + task.IsNestedBy
            .AsIterable()
            .SelectMany(static rel => rel.RelatedObjects.AsIterable())
            .OfType<IfcTask>()
            .ToSeq()
            .Bind(NestedTasks);

    static Fin<ConstructionTask> TaskOf(IfcTask task, WorkScheduleKind kind, DateTimeZone zone, Op key) =>
        IntervalOf(task.TaskTime?.ScheduleStart, task.TaskTime?.ScheduleFinish, zone, key)
            .Map(scheduled => new ConstructionTask(
                task.GlobalId,
                task.Name ?? "",
                TaskStatus.Of(task.Status),
                TaskKind.Of(task.PredefinedType),
                kind,
                task.IsMilestone,
                scheduled,
                ActualOf(task.TaskTime?.ActualStart, task.TaskTime?.ActualFinish, zone),
                CompletionOf(task.TaskTime)));

    static Seq<SequenceRel> SequencesOf(Seq<IfcTask> tasks) =>
        tasks
            .AsIterable()
            .SelectMany(static task => task.IsPredecessorTo.AsIterable())
            .OfType<IfcRelSequence>()
            .Map(rel => SequenceRel.Of(
                rel.SequenceType,
                rel.RelatingProcess?.GlobalId ?? "",
                rel.RelatedProcess?.GlobalId ?? "",
                PeriodOf(rel.TimeLag)))
            .Where(static edge => edge.PredecessorGlobalId.Length > 0 && edge.SuccessorGlobalId.Length > 0)
            .ToSeq();

    static Seq<TaskAssignment> AssignmentsOf(Seq<IfcTask> tasks) =>
        tasks
            .Map(static task => new TaskAssignment(
                task.GlobalId,
                task.OperatesOn
                    .AsIterable()
                    .SelectMany(static rel => rel.RelatedObjects.AsIterable())
                    .OfType<IfcProduct>()
                    .Select(static product => product.GlobalId)
                    .Where(static id => id.Length > 0)
                    .ToSeq()))
            .Filter(static assignment => assignment.ElementGlobalIds.IsEmpty == false);

    static Fin<Interval> IntervalOf(DateTime? start, DateTime? finish, DateTimeZone zone, Op key) =>
        (InstantOf(start, zone), InstantOf(finish, zone)) switch {
            ({ } from, { } to) when to >= from => FinSucc(new Interval(from, to)),
            ({ } from, { } to)                 => FinFail<Interval>(new BimFault.ModelRejected(key, $"task-finish-before-start:{to}<{from}")),
            ({ } from, null)                   => FinSucc(new Interval(from, from)),
            _                                  => FinFail<Interval>(new BimFault.ModelRejected(key, "task-missing-schedule-start")),
        };

    static Option<Interval> ActualOf(DateTime? start, DateTime? finish, DateTimeZone zone) =>
        (InstantOf(start, zone), InstantOf(finish, zone)) switch {
            ({ } from, { } to) when to >= from => Some(new Interval(from, to)),
            ({ } from, _)                      => Some(new Interval(from, from)),
            _                                  => None,
        };

    // GeometryGym stamps an absent IfcDate as DateTime.MinValue (== default), not null, so a set-but-empty
    // ScheduleStart must read as absent rather than minting a year-0001 instant — the same sentinel WorkCalendar.SpanOf reads.
    static Instant? InstantOf(DateTime? value, DateTimeZone zone) =>
        value is { } moment && moment != default
            ? LocalDateTime.FromDateTime(moment).InZoneLeniently(zone).ToInstant()
            : null;

    // The IFC-authored percent-complete (IfcTaskTime.Completion); None when unset so the Planning/cost#EARNED_VALUE
    // fold reads the actual-interval fraction rather than a spurious zero. A ratio outside (0, 1] reads as unset —
    // the GG double default 0 and a bogus >1 stamp alike — the same guard the cost resource Completion holds.
    static Option<double> CompletionOf(IfcTaskTime? taskTime) =>
        taskTime is { Completion: var completion } && completion is > 0d and <= 1d ? Some(completion) : None;

    // IfcLagTime.LagValue is an IfcTimeOrRatioSelect whose duration leg IfcDuration carries decomposed integer
    // fields, so the Period builds from those directly — a structured measure, never an ISO-string round-trip.
    static Period PeriodOf(IfcLagTime? lag) =>
        lag?.LagValue is IfcDuration duration
            ? new PeriodBuilder {
                Years = duration.Years, Months = duration.Months, Days = duration.Days,
                Hours = duration.Hours, Minutes = duration.Minutes,
                Seconds = (long)Math.Round(duration.Seconds),
            }.Build()
            : Period.Zero;
}

public static class ConstructionState {
    // The 4D snapshot at instant t: fold the active task set (effective interval contains t), union the assigned
    // element GlobalIds, and select them off the seam graph through the Model/query#ELEMENT_SET predicate algebra
    // matching each Node.Object.ExternalId — one ElementSet the AppUi viewport renders, never a second selection surface.
    public static ElementSet At(ScheduleNetwork network, ElementGraph graph, Instant instant) {
        var active = toHashSet(network.Tasks.Filter(task => task.ActiveAt(instant)).Map(static task => task.GlobalId));
        var assigned = toHashSet(network.Assignments
            .Filter(assignment => active.Contains(assignment.TaskGlobalId))
            .Bind(static assignment => assignment.ElementGlobalIds)).ToSeq();
        return ElementSet.Query(graph,
            new ElementPredicate.ByAttribute(ObjectAttribute.GlobalId, new ValueMatch.OneOf(assigned)));
    }
}
```

## [03]-[CRITICAL_PATH]

- Owner: `WorkCalendar` the host-neutral working-time function folded from one `IfcWorkCalendar` — a work-week `IsoDayOfWeek` set, a daily shift `LocalTime` span, the inclusive `DateInterval` exception spans the holiday/weather `IfcWorkTime` windows project onto (one typed span per window, never a hand-expanded concrete-date set), and the `AnnualDate` recurring-holiday rows an external calendar feed lands (the P6/MS-Project recurring exception a flat date set structurally cannot carry) — that maps a working-content `Duration` onto a calendar finish past the non-working days and reads the working content of an arbitrary span; `CriticalPath` the per-task value record carrying `EarlyStart`/`EarlyFinish`/`LateStart`/`LateFinish` as `Instant`, `TotalFloat`/`FreeFloat` as `Duration`, and the `bool IsCritical` zero-total-float flag; `ScheduleNetwork.Schedule` the forward/backward-pass CPM fold over the `SequenceRel` adjacency by topological order producing the `Map<string, CriticalPath>` per-task float window — one immutable fold over the edge set the network already owns, the SINGLE CPM owner both the IFC-projected and the MPXJ-parsed `SequenceRel` edge sets feed, never a mutable PERT accumulator and never a second pass in Persistence.
- Entry: `WorkCalendar.Of(IfcWorkCalendar calendar, DateTimeZone zone)` folds one GeometryGym work-calendar container into the typed working-time function — reading each `ExceptionTimes` `IfcWorkTime` window's public `StartDate`/`FinishDate` onto ONE inclusive NodaTime `DateInterval` span, over the default construction work-week and shift because the GeometryGym 25.7.30 `IfcWorkTime.RecurrencePattern` (the weekday/shift recurrence) is schema-internal with no public accessor, while the `AnnualHolidays` `AnnualDate` column lands a recurring annual holiday from a calendar feed; `WorkCalendar.Default` is the standard 5-day Monday-through-Friday 08:00–16:00 calendar a plan without an `IfcWorkCalendar` resolves against, so a duration-driven task always has a working-time function. `ScheduleNetwork.Schedule(WorkCalendar calendar, Op key)` folds the network into the `Map<string, CriticalPath>` — the `SequenceRel` dependencies fold into a transient `BidirectionalGraph<string, STaggedEdge<string, SequenceRel>>` (the task `GlobalId` the vertex, each dependency riding its value edge as the `Tag` so `InEdges`/`OutEdges` ARE the predecessor/successor reads, `allowParallelEdges: true` keeping a real SS+FF pair between one task pair as two constraints) and `graph.SourceFirstBidirectionalTopologicalSort()` IS the `QuikGraph` Kahn order the forward pass folds in (a residual cycle the `IsDirectedAcyclicGraph()` pre-gate rejects, lowered to `BimFault.ModelRejected`, rather than a hand-rolled in-degree drain), the forward pass folds each task in that order deriving `EarlyStart` as the maximum over `InEdges` of the lagged, modality-anchored predecessor float — the `SequenceKind.FromFinish` column anchors the predecessor `EarlyFinish` (`FinishToStart`/`FinishToFinish`) or `EarlyStart` (`StartToStart`/`StartToFinish`), the edge `Period` lag applies as a calendar offset in the model zone (`anchor.InZone(zone).LocalDateTime + lag`, never the throwing `Period.ToDuration`), and a `SequenceKind.ToFinish` finish-anchored modality (`FinishToFinish`/`StartToFinish`) RECEDES the successor working content off the lagged finish through `calendar.Recede` so the early start skips non-working days rather than a raw `Duration` subtraction — and `EarlyFinish` through `calendar.Advance(EarlyStart, workContent)`, the backward pass folds in the sink-first `SourceFirstBidirectionalTopologicalSort(TopologicalSortDirection.Backward)` order — the API's own reverse Kahn order, never an `order.Rev()` re-derivation — deriving `LateFinish` as the minimum over `OutEdges` of the `BackShift` bound (the exact backward DUAL of `Shift` over the same `SequenceKind` 2x2: the `ToFinish` column anchors the successor `LateFinish` vs `LateStart`, the lag RECEDES as the same zone-anchored calendar `Period`, and a start-anchored modality ADVANCES the predecessor working content so the bound lands on its finish) and `LateStart` through `calendar.Recede`, and `TotalFloat = LateStart − EarlyStart`, `FreeFloat` the minimum out-edge SLACK (the successor's achieved `EarlyStart` minus this edge's `Shift` demand — exact over all four modalities and lags, collapsing to `min successor EarlyStart − EarlyFinish` only on an unlagged FS graph), `IsCritical = TotalFloat ≤ Duration.Zero`; the fold is total over the acyclic graph and carries the `Fin<T>` rail only for the cycle rejection.
- Auto: `WorkCalendar.Advance(Instant from, Duration work)` walks forward from `from` in the model zone, consuming each working day's REMAINING shift content (the cursor clamped up to `ShiftStart`, then run to `ShiftEnd` — a mid-shift start contributes only its tail, a non-working/exception day zero) until the accumulated working `Duration` reaches `work` and the finish lands INSIDE the shift, never past `ShiftEnd`, so a three-working-day task starting on a Friday resolves its finish on the following Tuesday across the intervening weekend; `Recede` is the symmetric backward walk the backward pass reads, the cursor clamped down to `ShiftEnd`; `ShiftWindow(LocalDate)` is the day's `[ShiftStart, ShiftEnd]` working window (skipped on a non-working/exception day) the two walks clamp into, and `WorkingBetween(Instant, Instant)` sums each working day's OVERLAP with that window so the CPM feeds `Advance` the TRUE working content of each task — `calendar.WorkingBetween(task.Scheduled.Start, task.Scheduled.End)` — rather than the raw calendar span the interval bounds, which would double-count the weekends and partial off-shift hours the calendar already skips; `WorkingBetween` folds the inclusive `DateInterval` day sequence directly (the interval IS the `IEnumerable<LocalDate>` walk) and `SpanOf` projects each exception window onto one `DateInterval` value, so no hand-rolled day-range arithmetic exists beside the NodaTime interval owner. `ScheduleNetwork.Schedule` threads the `(forward, backward)` accumulator as two folds over the one topological order: the forward fold seeds a no-predecessor task at the project start (the minimum scheduled `Interval.Start`), reads the predecessor `CriticalPath` already computed (topological order guarantees it is present), and the backward fold seeds a no-successor task at the project finish (the maximum forward `EarlyFinish`); a task whose `SequenceKind` modality is `StartToStart`/`FinishToFinish`/`StartToFinish` reads its `FromFinish`/`ToFinish` columns in BOTH passes (`Shift` forward, `BackShift` backward — the two duals over the one 2x2) rather than assuming finish-to-start, so the float algebra is column-driven over all four modalities and a new modality needs no new shift arm. The topological orders are the `QuikGraph` `SourceFirstBidirectionalTopologicalSort` source-first and sink-first Kahn sorts over the transient `BidirectionalGraph` folded once from the `SequenceRel`-tagged edges — the graph is the algorithm input only, never a domain field, its `InEdges`/`OutEdges` the one predecessor/successor surface the float fold reads (the `GroupBy` side maps are the deleted form) — so the schedule, the MEP trace, and the commit-DAG merge-base share ONE graph-algorithm owner rather than three hand-rolled walks, and an MPXJ-parsed `ProjectFile` whose `Relation` edges map onto the same `SequenceRel` set feeds this one pass rather than a second `CpmPass` in Persistence.
- Receipt: the `Map<string, CriticalPath>` is the CPM evidence the `csharp:Rasm.AppUi/Charts` critical-chain report and the 4D playback read — the critical-path set is `path.Filter(static (_, cp) => cp.IsCritical)`, the per-task float window the resource-leveling read, and the `EarlyStart`/`LateFinish` the schedule bounds; the `WorkCalendar` working-time finish is the calendar-accurate `Interval` the `ConstructionState.At` snapshot reads, never a continuous-span approximation, while the `Planning/cost#EARNED_VALUE` `EarnedValue` schedule-performance fold reads the `ConstructionTask.Completion` progress off this network.
- Packages: GeometryGymIFC_Core, NodaTime, QuikGraph, LanguageExt.Core, Rasm
- Growth: a new dependency modality is one `SequenceKind` row (its `FromFinish`/`ToFinish` cell) the column-driven CPM shift reads with no new arm, the same row the `[2]-[SCHEDULE]` `[SmartEnum]` widens on; a new recurring holiday is one `AnnualDate` row on the `AnnualHolidays` column and a new exception window one `DateInterval` span, both the same `IsWorking` read (a per-day shift override stays one future column on `WorkCalendar`); a new float metric (a working-day float, a critical-chain buffer) is one column on `CriticalPath` derived from the same forward/backward pass; a new graph query over the network (a longest-path duration, a resource-constrained reorder) rides the same `QuikGraph` `AlgorithmExtensions` facade over the transient `BidirectionalGraph`; never a per-modality CPM pass, never a second calendar engine, never a hand-rolled topological sort beside `QuikGraph`, and never a mutable PERT accumulator.
- Boundary: the CPM pass is ONE immutable fold over the `SequenceRel` adjacency by topological order — a mutable `Dictionary<string, double>` early-start accumulator mutated in a `for` loop is the deleted form, the forward/backward pass threading the `Map<string, CriticalPath>` accumulator through the topological fold; the topological orders are the `QuikGraph` `SourceFirstBidirectionalTopologicalSort` source-first/sink-first Kahn sorts over the transient `BidirectionalGraph<string, STaggedEdge<string, SequenceRel>>` folded from the tagged dependencies, and a hand-rolled in-degree drain over a `Map<>` adjacency, a `GroupBy` predecessor/successor side map beside the graph's own `InEdges`/`OutEdges`, or an `order.Rev()` re-derivation of the sink-first order the API yields directly, is the deleted form (`QuikGraph` owns the ORDER and the edge-carried `SequenceRel`, the `WorkCalendar` fold owns the float/calendar arithmetic), as is a backward gate reading every out-edge as finish-to-start — dropping the modality columns and the lag misprices the float of every lagged or SS/FF/SF dependency, so the backward pass runs the `BackShift` dual of the same 2x2 — a cyclic dependency edge surfaces through `IsDirectedAcyclicGraph()` lowered BARE to `BimFault.ModelRejected` rather than looping, the `Fin<T>` rail carried only for the cycle rejection; the transient graph is the algorithm input only and never a domain field on `ScheduleNetwork`, the `(GeometryKey, ScheduleKey)` content key keying the fold so the order re-runs only on a changed network; the CPM consumes the TRUE working content `calendar.WorkingBetween(scheduled)` as the `Advance`/`Recede` duration and feeding the raw calendar span (which double-counts non-working days the calendar skips) is the named correctness defect; the `WorkCalendar` working-time arithmetic composes the NodaTime `LocalDate`/`LocalTime`/`DateInterval`/`AnnualDate`/`IsoDayOfWeek` surface — the exception window is one inclusive `DateInterval` value and the recurring holiday one `AnnualDate` row — and a hand-rolled day-counter, a concrete expanded date set, or a BCL `DateTime.AddDays` loop is the deleted form — NodaTime owns the date arithmetic; the GeometryGym `IfcWorkCalendar.WorkingTimes`/`ExceptionTimes` `SET<IfcWorkTime>` and the public `IfcWorkTime.StartDate`/`FinishDate` `DateTime` spans are consumed as settled vocabulary, while the schema-internal `IfcWorkTime.RecurrencePattern`/`IfcRecurrencePattern` carries NO public accessor in 25.7.30 — reading `RecurrencePattern.WeekdayComponent` off a different assembly is the named phantom-member defect, the calendar resolving the public exception spans over the default work-week instead; the `CriticalPath` float window is the `Planning/cost#EARNED_VALUE` `EarnedValue` schedule-performance read and re-deriving the activity network on the cost page is the deleted form; the CPM is THIS owner's single fold and a Persistence `CpmPass` re-deriving the order over MPXJ-parsed edges is the named cross-package drift defect; a CPM rejection lifts the typed `BimFault` case BARE, a `.ToError()` hop being the named seam defect.

```csharp contract
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using GeometryGym.Ifc;
using LanguageExt;
using NodaTime;
using QuikGraph;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.TopologicalSort;
using Op = Rasm.Domain.Op;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [MODELS] -----------------------------------------------------------------------------
// The work-week set is a FrozenSet read table; the exception windows are inclusive NodaTime DateInterval values
// (ONE value per IfcWorkTime span, never a hand-expanded concrete-date set) and the recurring holidays AnnualDate
// rows (the landing column a P6/MS-Project recurring calendar exception fills); the calendar is a pure working-time
// function, not an accumulator.
public sealed record WorkCalendar(
    FrozenSet<IsoDayOfWeek> WorkWeek,
    LocalTime ShiftStart,
    LocalTime ShiftEnd,
    Seq<DateInterval> ExceptionSpans,
    Seq<AnnualDate> AnnualHolidays,
    DateTimeZone Zone) {
    public static readonly WorkCalendar Default = new(
        new[] { IsoDayOfWeek.Monday, IsoDayOfWeek.Tuesday, IsoDayOfWeek.Wednesday, IsoDayOfWeek.Thursday, IsoDayOfWeek.Friday }.ToFrozenSet(),
        new LocalTime(8, 0), new LocalTime(16, 0), Seq<DateInterval>(), Seq<AnnualDate>(), DateTimeZone.Utc);

    // AnnualDate.InYear lands a Feb-29 holiday on Feb-28 in a non-leap year, so a recurring row always excludes a day.
    bool IsWorking(LocalDate day) =>
        WorkWeek.Contains(day.DayOfWeek)
        && !ExceptionSpans.Exists(span => span.Contains(day))
        && !AnnualHolidays.Exists(holiday => holiday.InYear(day.Year) == day);

    // The day's [ShiftStart, ShiftEnd] working window as zone-resolved instants — the span Advance/Recede clamp the
    // cursor INTO so a partial first/last day contributes only its remaining shift content, and WorkingBetween sums
    // the OVERLAP of [from, to] with. A non-working/exception day is handled by the IsWorking gate at each caller.
    (Instant Lo, Instant Hi) ShiftWindow(LocalDate day) =>
        ((day + ShiftStart).InZoneLeniently(Zone).ToInstant(), (day + ShiftEnd).InZoneLeniently(Zone).ToInstant());

    // Ten consecutive workless YEARS marks a degenerate calendar (an AnnualHolidays set covering every work-week
    // day recurs FOREVER — exception spans alone are finite, the annual rows are not): the walks clamp at the
    // cursor, the same unmoved posture as the empty work-week guard, so the walk is BOUNDED on adversarial input.
    const int BarrenLimit = 3660;

    // Walk forward consuming each working day's remaining shift content — from the cursor clamped UP to ShiftStart to
    // ShiftEnd — until `work` is met, so a mid-shift start contributes only its tail and the finish lands INSIDE the
    // shift (never past ShiftEnd), skipping non-working days. A zero-work milestone finishes where it starts; an
    // empty work-week or a BarrenLimit-degenerate calendar returns the cursor unmoved.
    public Instant Advance(Instant from, Duration work) {
        if (work <= Duration.Zero || WorkWeek.Count == 0) { return from; }
        var cursor = from;
        var remaining = work;
        var barren = 0;
        while (barren <= BarrenLimit) {
            var day = cursor.InZone(Zone).Date;
            if (IsWorking(day)) {
                barren = 0;
                var (lo, hi) = ShiftWindow(day);
                var start = cursor > lo ? cursor : lo;
                if (start < hi) {
                    var available = hi - start;
                    if (remaining <= available) { return start + remaining; }
                    remaining -= available;
                }
            } else { barren++; }
            cursor = (day.PlusDays(1) + ShiftStart).InZoneLeniently(Zone).ToInstant();
        }
        return from;
    }

    // The symmetric backward walk the backward CPM pass reads: consume each working day's content from the cursor
    // clamped DOWN to ShiftEnd back to ShiftStart, so a late-finish receding past non-working days lands inside the shift.
    public Instant Recede(Instant to, Duration work) {
        if (work <= Duration.Zero || WorkWeek.Count == 0) { return to; }
        var cursor = to;
        var remaining = work;
        var barren = 0;
        while (barren <= BarrenLimit) {
            var day = cursor.InZone(Zone).Date;
            if (IsWorking(day)) {
                barren = 0;
                var (lo, hi) = ShiftWindow(day);
                var end = cursor < hi ? cursor : hi;
                if (end > lo) {
                    var available = end - lo;
                    if (remaining <= available) { return end - remaining; }
                    remaining -= available;
                }
            } else { barren++; }
            cursor = (day.PlusDays(-1) + ShiftEnd).InZoneLeniently(Zone).ToInstant();
        }
        return to;
    }

    // The TRUE working content of a span: the inclusive DateInterval [from.Date, to.Date] IS the day walk (the
    // interval enumerates its own LocalDate sequence), each working day contributing the OVERLAP of [from, to] with
    // its shift window — so the CPM feeds Advance the exact working duration of a task, never the raw calendar span.
    public Duration WorkingBetween(Instant from, Instant to) =>
        to <= from
            ? Duration.Zero
            : new DateInterval(from.InZone(Zone).Date, to.InZone(Zone).Date).AsIterable()
                .Fold(Duration.Zero, (total, day) => {
                    if (!IsWorking(day)) { return total; }
                    var (lo, hi) = ShiftWindow(day);
                    var start = from > lo ? from : lo;
                    var end = to < hi ? to : hi;
                    return end > start ? total + (end - start) : total;
                });

    // GeometryGym 25.7.30 exposes IfcWorkTime only by its public StartDate/FinishDate spans — the weekday/shift
    // IfcRecurrencePattern is schema-internal (no public accessor), so the IFC fold reads the public ExceptionTimes
    // holiday/non-working windows onto DateInterval spans over the default work-week; the AnnualHolidays recurring
    // rows land from a calendar feed (the MPXJ P6 recurring-exception lane), never off this internal-member surface.
    public static WorkCalendar Of(IfcWorkCalendar calendar, DateTimeZone zone) =>
        Default with { ExceptionSpans = calendar.ExceptionTimes.AsIterable().Choose(SpanOf).ToSeq(), Zone = zone };

    // ONE inclusive DateInterval per exception window; GeometryGym stamps an absent IfcDate as DateTime.MinValue
    // (== default), so a dateless exception yields None and an inverted span never reaches the throwing ctor.
    static Option<DateInterval> SpanOf(IfcWorkTime exception) {
        if (exception.StartDate == default) { return None; }
        var start = LocalDate.FromDateTime(exception.StartDate);
        var finish = exception.FinishDate == default ? start : LocalDate.FromDateTime(exception.FinishDate);
        return finish < start ? None : Some(new DateInterval(start, finish));
    }
}

public readonly record struct CriticalPath(
    Instant EarlyStart, Instant EarlyFinish,
    Instant LateStart, Instant LateFinish,
    Duration TotalFloat, Duration FreeFloat, bool IsCritical) {
    public static CriticalPath Of(Instant es, Instant ef, Instant ls, Instant lf, Duration free) =>
        new(es, ef, ls, lf, ls - es, free, (ls - es) <= Duration.Zero);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class ScheduleCpm {
    public static Fin<Map<string, CriticalPath>> Schedule(this ScheduleNetwork network, WorkCalendar calendar, Op key) =>
        network.Tasks.IsEmpty
            ? FinSucc(Map<string, CriticalPath>())
            : Graph(network, key).Map(graph => {
                var duration = network.Tasks.Map(t => (t.GlobalId, calendar.WorkingBetween(t.Scheduled.Start, t.Scheduled.End))).ToMap();
                var projectStart = network.Tasks.Map(static t => t.Scheduled.Start).Min();

                var forward = toSeq(graph.SourceFirstBidirectionalTopologicalSort()).Fold(Map<string, (Instant Es, Instant Ef)>(), (acc, id) => {
                    var gates = graph.InEdges(id).AsIterable().Map(edge => Shift(edge.Tag, acc, duration, calendar)).ToSeq();
                    var es = gates.IsEmpty ? projectStart : gates.Max();
                    return acc.Add(id, (es, calendar.Advance(es, duration[id])));
                });
                var projectFinish = forward.Values.Map(static p => p.Ef).Max();

                var backward = toSeq(graph.SourceFirstBidirectionalTopologicalSort(TopologicalSortDirection.Backward))
                    .Fold(Map<string, (Instant Ls, Instant Lf)>(), (acc, id) => {
                        var gates = graph.OutEdges(id).AsIterable().Map(edge => BackShift(edge.Tag, acc, duration, calendar)).ToSeq();
                        var lf = gates.IsEmpty ? projectFinish : gates.Min();
                        return acc.Add(id, (calendar.Recede(lf, duration[id]), lf));
                    });

                // FreeFloat is the minimum out-edge SLACK — the successor's achieved EarlyStart minus this edge's
                // Shift demand — exact over all four modalities and lags (an FS-only `min successor Es − Ef` read
                // under-floats a lagged or SS/FF/SF graph); a sink task carries zero free float.
                return network.Tasks.Fold(Map<string, CriticalPath>(), (acc, task) => {
                    var (es, ef) = forward[task.GlobalId];
                    var (ls, lf) = backward[task.GlobalId];
                    var slacks = graph.OutEdges(task.GlobalId).AsIterable().Map(edge => forward[edge.Target].Es - Shift(edge.Tag, forward, duration, calendar)).ToSeq();
                    var free = slacks.IsEmpty ? Duration.Zero : slacks.Min();
                    return acc.Add(task.GlobalId, CriticalPath.Of(es, ef, ls, lf, free));
                });
            });

    // The successor's candidate EarlyStart from one predecessor edge (the SequenceRel riding the edge Tag), column-
    // driven over the SequenceKind 2x2: the anchor reads the predecessor early FINISH or START (FromFinish), the lag
    // applies as a calendar Period in the model zone (never the throwing Period.ToDuration), and a finish-anchored
    // modality (ToFinish) RECEDES the successor working content off the lagged finish through the same calendar so
    // the early start skips non-working days — the FF/SF backoff is calendar-consistent, never a raw Duration
    // subtraction. The topological order guarantees the predecessor float is already computed (Graph gate).
    static Instant Shift(SequenceRel edge, Map<string, (Instant Es, Instant Ef)> forward, Map<string, Duration> duration, WorkCalendar calendar) {
        var (es, ef) = forward[edge.PredecessorGlobalId];
        var lagged = ((edge.Kind.FromFinish ? ef : es).InZone(calendar.Zone).LocalDateTime + edge.Lag).InZoneLeniently(calendar.Zone).ToInstant();
        return edge.Kind.ToFinish ? calendar.Recede(lagged, duration[edge.SuccessorGlobalId]) : lagged;
    }

    // The predecessor's candidate LateFinish from one successor edge — the exact BACKWARD dual of Shift over the
    // same SequenceKind 2x2: the anchor reads the successor Late FINISH or START (ToFinish), the lag RECEDES as the
    // same zone-anchored calendar Period, and a start-anchored modality (FromFinish: false — the constraint binds the
    // predecessor START) ADVANCES the predecessor working content so the bound lands on its finish. A backward gate
    // reading every out-edge as finish-to-start (dropping the columns and the lag) is the deleted form — it
    // misprices the float of every lagged or SS/FF/SF dependency.
    static Instant BackShift(SequenceRel edge, Map<string, (Instant Ls, Instant Lf)> backward, Map<string, Duration> duration, WorkCalendar calendar) {
        var (ls, lf) = backward[edge.SuccessorGlobalId];
        var bound = ((edge.Kind.ToFinish ? lf : ls).InZone(calendar.Zone).LocalDateTime - edge.Lag).InZoneLeniently(calendar.Zone).ToInstant();
        return edge.Kind.FromFinish ? bound : calendar.Advance(bound, duration[edge.PredecessorGlobalId]);
    }

    // ONE transient BidirectionalGraph<string, STaggedEdge<string, SequenceRel>> per pass: the SequenceRel rides each
    // edge as its Tag, so the predecessor/successor reads are graph.InEdges/OutEdges — the GroupBy side maps are the
    // deleted form — the forward order is the source-first Kahn sort and the backward order the SAME sort sink-first
    // (TopologicalSortDirection.Backward), never an order.Rev() re-derivation; allowParallelEdges:true keeps a real
    // P6 SS+FF pair between one task pair as two constraints. A dependency naming a process the network never
    // declares lifts BARE onto BimFault.DanglingReference FIRST (a phantom vertex never enters the float fold);
    // AddVertexRange orders a no-dependency isolate; the IsDirectedAcyclicGraph pre-gate lifts a residual cycle BARE
    // onto BimFault.ModelRejected before either sort throws. The graph is the algorithm input only — never a field.
    static Fin<BidirectionalGraph<string, STaggedEdge<string, SequenceRel>>> Graph(ScheduleNetwork network, Op key) {
        var taskIds = toHashSet(network.Tasks.Map(static t => t.GlobalId));
        return network.Dependencies
            .Find(d => !taskIds.Contains(d.PredecessorGlobalId) || !taskIds.Contains(d.SuccessorGlobalId))
            .Match(
                Some: edge => FinFail<BidirectionalGraph<string, STaggedEdge<string, SequenceRel>>>(
                    new BimFault.DanglingReference(key, $"schedule-dependency-absent-process:{edge.PredecessorGlobalId}>{edge.SuccessorGlobalId}")),
                None: () => {
                    var graph = new BidirectionalGraph<string, STaggedEdge<string, SequenceRel>>(allowParallelEdges: true);
                    graph.AddVertexRange(network.Tasks.Map(static t => t.GlobalId));
                    network.Dependencies.Iter(d => graph.AddEdge(new STaggedEdge<string, SequenceRel>(d.PredecessorGlobalId, d.SuccessorGlobalId, d)));
                    return graph.IsDirectedAcyclicGraph()
                        ? FinSucc(graph)
                        : FinFail<BidirectionalGraph<string, STaggedEdge<string, SequenceRel>>>(new BimFault.ModelRejected(key, "schedule-cyclic-dependency"));
                });
    }
}
```

## [04]-[RESEARCH]

- [WORK_PLAN_DISPATCH]: the `IfcWorkPlan`→`IfcWorkSchedule`→`IfcTask` container traversal is verified against the live GeometryGym 25.7.30 decompile — a work plan decomposes its schedules through `IfcWorkControl.IsNestedBy` `IfcRelNests.RelatedObjects` (`OfType<IfcWorkSchedule>`), NOT `Controls` (the `IfcRelAssignsToControl` task-control path GeometryGym's own `ComputeAndSetScheduledStartFinishTimes` reads tasks through), so `SchedulesOf` reads the nesting decomposition; each schedule's tasks are `IfcWorkControl.Controls` `IfcRelAssignsToControl.RelatedObjects` (`OfType<IfcTask>`), and each task's summary→detail WBS is the `IfcTask.IsNestedBy` `IfcRelNests` sub-task tree the `NestedTasks` closure flattens — the GeometryGym `IfcTask` itself folds `base.IsNestedBy.SelectMany(x => x.RelatedObjects).OfType<IfcTask>()` for its own descendant set, so the recursion mirrors the library; the `IfcProcess.IsPredecessorTo`/`OperatesOn` (`SET<IfcRelSequence>`/`SET<IfcRelAssignsToProcess>`, both public), the `IfcRelSequence.RelatingProcess`/`RelatedProcess`/`TimeLag`/`SequenceType`, the `IfcTask.TaskTime`/`Status`/`IsMilestone`/`PredefinedType` (`IfcTaskTypeEnum` — the `TaskKind` roster over the IFC4 base members, an unrostered future member landing `NotDefined` through the generated `TryGet`, the roster re-sourcing on the GG pin bump like every token set), and the `IfcRelAssignsToProcess.RelatedObjects` member spellings confirm against the catalogued surface (`.api/api-geometrygym-ifc` scheduling rows 01-08, traversal entrypoints 04-06) before the projection fold is final — the `IfcProcess.OperatesOn` assignment path is distinct from the `IfcWorkSchedule.Controls` control path the kind threads through, so a task's `WorkScheduleKind` reads from the owning schedule the traversal already holds rather than a re-resolved task inverse.
- [TASK_TIME_INTERVAL]: the `IfcTaskTime` schedule/actual/completion the `TaskOf` fold reads is verified against the live decompile — `IfcTaskTime : IfcSchedulingTime` exposes the public `ScheduleStart`/`ScheduleFinish`/`ActualStart`/`ActualFinish` (`DateTime`), `ScheduleDuration`/`ActualDuration`/`RemainingTime`/`TotalFloat`/`FreeFloat` (`IfcDuration`), `DurationType` (`IfcTaskDurationEnum` `ELAPSEDTIME`/`WORKTIME`), `IsCritical` (`bool`), and `Completion` (`double`); `IfcTask.TaskTime` itself is schema-OPTIONAL, so a dateless row (a P6 WBS container imported as a summary `IfcTask`, or an absent/`DateTime.MinValue` `ScheduleStart`) is filtered BEFORE `TaskOf` — it projects no `ConstructionTask` (the WBS shape rides the `IfcRelNests` recursion, the dated leaves carry the activities) and a dependency or assignment naming it faults typed downstream, so a schema-legal dateless container never aborts the whole import; the schedule/actual `DateTime`s lift through `LocalDateTime.FromDateTime` and map into the model's `DateTimeZone` through `InZoneLeniently` (`.api/api-nodatime` `LocalDateTime.InZoneLeniently` row 13) so an IFC calendar value carrying no offset resolves the daylight-transition gap/overlap, and the `Interval.Contains` membership predicate (`.api/api-nodatime` `Interval.Contains` row 19) backs the `ConstructionTask.ActiveAt` fold so the 4D snapshot reads the receipt-window membership the NodaTime `Interval` owns; the `IfcTaskTime.Completion` ratio rides the `Option<double> PercentComplete` the `Planning/cost#EARNED_VALUE` BCWP fold consumes — the source-authored progress the cost owner previously hand-derived from the actual-interval fraction — while the `ScheduleDuration`/`DurationType` working content is reconciled by the calendar-derived `WorkCalendar.WorkingBetween` so the CPM never trusts a raw `ELAPSEDTIME` span as working content.
- [LAG_PERIOD]: the `IfcRelSequence.TimeLag` `IfcLagTime` dependency lag the `PeriodOf` fold reads onto the NodaTime `Period` is verified against the live decompile — `IfcLagTime.LagValue` is an `IfcTimeOrRatioSelect` interface the duration leg `IfcDuration` (`: IfcSimpleValue, IfcTimeOrRatioSelect`) implements, so the fold narrows `LagValue is IfcDuration` and reads the public decomposed `Years`/`Months`/`Days`/`Hours`/`Minutes` (`int`) and `Seconds` (`double`) fields directly through a NodaTime `PeriodBuilder` (`.api/api-nodatime` `PeriodBuilder` row 14, `PeriodBuilder.Build` row 18) rather than round-tripping the `IfcDuration.ToString()` ISO string — `IfcDuration` is a structured measure, so the decomposed-field build is the exact projection; the lag `Period` then applies in the CPM as an ELAPSEDTIME calendar offset anchored in the model zone (`anchor.InZone(zone).LocalDateTime + lag`), because the `NodaTime` `Period.ToDuration` rejects non-fixed (`Months`/`Years`) units with an `InvalidOperationException` — the zone-anchored application resolves a months/years lag by calendar arithmetic and never throws into the float fold; the verified-public `IfcLagTime.DurationType` (`IfcTaskDurationEnum` `ELAPSEDTIME`/`WORKTIME`/`NOTDEFINED`) is carried by the same `IfcLagTime` but the pass applies the lag as elapsed calendar time, the WORKTIME working-day lag projection deferred (the ratio leg `LagValue is IfcRatioMeasure` likewise folds to `Period.Zero`, the duration leg the exact case); the `IfcSequenceEnum` members `START_START`/`START_FINISH`/`FINISH_START`/`FINISH_FINISH`/`USERDEFINED`/`NOTDEFINED` (verified — six members; `.api/api-geometrygym-ifc` enum row 10) fold onto the FOUR `SequenceKind` `[SmartEnum]` rows through the `SequenceKind.Of` resolver, the real `START_FINISH` just-in-time modality resolving to the first-class `StartToFinish` row (CPM-distinct: it anchors the successor finish to the predecessor start) and only the `USERDEFINED`/`NOTDEFINED` fallbacks lowering onto `FinishToStart`, so no real modality is dropped and no guessed modality mints a parallel edge type — the same four `RelationType` modalities (`FinishStart`/`StartStart`/`FinishFinish`/`StartFinish`) a `csharp:Rasm.Persistence/Version/ledger # [WIRE]` MPXJ `Relation` maps onto this `SequenceKind` vocabulary (`api-quikgraph` schedule consumer).
- [SCHEDULE_CONTENT_KEY]: the `ScheduleNetwork.Identity` `(GeometryKey, ScheduleKey)` `UInt128` pair the `csharp:Rasm.AppUi/Charts` report and `csharp:Rasm.Persistence/Query/federation#FEDERATION` activity-network persistence read the network by grounds against the `Review/diff#MODEL_DIFF` `XxHash128.HashToUInt128` content-key idiom (`libs/csharp/.api/api-hashing` `XxHash128.HashToUInt128(ReadOnlySpan<byte>, long seed = 0)` row 03, seed-zero), so the report re-reads the network only on a changed `GeometryKey` (the assigned-element GlobalIds) or `ScheduleKey` (the task effective-`Interval` bounds, status, authored percent, `TaskKind`, `WorkScheduleKind`, and milestone flag, plus the kind-discriminated dependency-edge lags — a parallel SS+FF pair between one task pair hashes as two distinct rows, and a re-kinded edge, a re-kinded task, or a re-progressed plan re-keys) rather than re-projecting the container; the `ConstructionState.At(Instant)` snapshot resolves the active-element set through the `Model/query#ELEMENT_SET` `ElementSet.Query` over the `ByAttribute(ObjectAttribute.GlobalId, ValueMatch.OneOf(…))` predicate against the seam `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph.ObjectNodes` (each `Node.Object.ExternalId` the Bim-stored IFC GlobalId), so the 4D playback at a Gantt instant is one `ElementSet` value the AppUi viewport renders — re-minting a schedule-element selection surface or a `ByGlobalId` predicate arm is the no-second-selection-surface reject, the retired `BimModel.Elements` collection GONE, and the `Planning/cost#ESTIMATE` 5D resource-join cluster reads this `ConstructionTask` time axis by the `[2]-[SCHEDULE]` anchor rather than re-deriving the activity network. The durable external-scheduler store `csharp:Rasm.Persistence/Version/ledger # [WIRE]` federates its Primavera P6/MS-Project float overlay to this host-neutral domain by the `ConstructionTask.GlobalId` — the Persistence `ScheduleTask.GlobalId` correlation joins each external activity to the IFC-projected task so a stored P6 total-float erosion overlays the IFC `CriticalPath` window by `GlobalId`, never by the external P6 `TaskId`; MPXJ is parse-only (it supplies `SequenceRel`-mapped edges, never the forward/backward pass), so a Persistence `CpmPass` re-deriving this owner's order is the cross-package drift the `[CPM_TOPOLOGICAL_PASS]` boundary forbids.
- [WORK_CALENDAR_RECURRENCE]: the `IfcWorkCalendar`/`IfcWorkTime` working-time surface the `WorkCalendar.Of` fold reads is verified against the live decompile — `IfcWorkCalendar : IfcControl` exposes the PUBLIC `WorkingTimes`/`ExceptionTimes` (`SET<IfcWorkTime>`) and `PredefinedType` (`IfcWorkCalendarTypeEnum`); `IfcWorkTime : IfcSchedulingTime` exposes the PUBLIC `StartDate`/`FinishDate` (`DateTime`, `DateTime.MinValue` sentinel for absent), but its `RecurrencePattern` (`IfcRecurrencePattern`) getter is `internal` and `IfcRecurrencePattern`'s `mRecurrenceType`/`mWeekdayComponent`/`mDayComponent`/`mMonthComponent`/`mInterval`/`mOccurrences`/`mTimePeriods` are ALL `internal` fields with NO public accessor in 25.7.30 — so a cross-assembly read of `RecurrencePattern.WeekdayComponent` does not compile and is the named phantom-member defect; the `WorkCalendar` therefore composes the public `ExceptionTimes` holiday/non-working `StartDate`/`FinishDate` windows onto inclusive NodaTime `DateInterval` spans (the `SpanOf` projection — `DateInterval` the calendar-interval owner whose `Contains(LocalDate)` backs `IsWorking` and whose own `IEnumerable<LocalDate>` walk backs `WorkingBetween`, `.api/api-nodatime` duration-type row 16) over the default 5-day shift work-week; the `AnnualHolidays` `AnnualDate` column (`.api/api-nodatime` local-type row 08, `InYear(int)` resolving the concrete date per year, Feb-29 landing Feb-28 in a non-leap year) carries the recurring annual holiday a concrete-date set structurally cannot — the `csharp:Rasm.Persistence/Version/ledger # [WIRE]` MPXJ lane's P6 recurring calendar exception is its feed — and a finer weekday/shift recurrence rides the same spans when a future GeometryGym release publishes a public recurrence accessor, the calendar function honest to the surface the library actually exposes.
- [CPM_TOPOLOGICAL_PASS]: the `ScheduleNetwork.Schedule` forward/backward-pass CPM fold orders the `SequenceRel` predecessor→successor DAG through the `QuikGraph` `[GRAPH_ALGORITHM]` owner — the dependencies fold into a transient `BidirectionalGraph<string, STaggedEdge<string, SequenceRel>>` (the task `GlobalId` the vertex directly, the `SequenceRel` riding each value edge as its `Tag` per the `api-quikgraph` edge-shape row 05 so `InEdges(id)`/`OutEdges(id)` ARE the predecessor/successor reads and no `GroupBy` side map exists beside the graph, `AddVertexRange` adding every task so an unsequenced isolate still orders, `allowParallelEdges: true` keeping a real P6 SS+FF pair between one task pair as two edge-carried constraints), `graph.SourceFirstBidirectionalTopologicalSort()` is the Kahn source-first order the forward pass folds in and `SourceFirstBidirectionalTopologicalSort(TopologicalSortDirection.Backward)` the sink-first order the backward pass folds in directly (`api-quikgraph` topological-order row 03 — the API yields the reverse order, never an `order.Rev()` re-derivation), the `graph.IsDirectedAcyclicGraph()` predicate (row 04) the cheap acyclicity pre-gate that lifts a residual cycle BARE onto `BimFault.ModelRejected` before either sort would throw `NonAcyclicGraphException` — verified against the live `QuikGraph` surface (`SourceFirstBidirectionalTopologicalSort<TVertex, TEdge>(this IBidirectionalGraph<TVertex, TEdge>[, TopologicalSortDirection])`, the `QuikGraph.Algorithms.TopologicalSort.TopologicalSortDirection` `Forward`/`Backward` members, `STaggedEdge<TVertex, TTag>(source, target, tag) : IEdge<TVertex>, ITagged<TTag>`, `IsDirectedAcyclicGraph<TVertex, TEdge>(this IVertexListGraph<TVertex, TEdge>)`, `AddEdge`/`AddVertexRange` on `BidirectionalGraph`); the forward pass folds each task in that order deriving `EarlyStart` as the maximum over `InEdges` of the lagged modality-anchored predecessor float (the `SequenceKind` `FromFinish`/`ToFinish` columns discriminating all four of `FinishToStart`/`StartToStart`/`FinishToFinish`/`StartToFinish` so the float algebra never assumes finish-to-start, the lag applying as a zone-anchored calendar `Period` and the finish-anchored `FinishToFinish`/`StartToFinish` modalities receding the successor working content through `WorkCalendar.Recede` rather than a raw `Duration` subtraction) and `EarlyFinish` through `WorkCalendar.Advance(EarlyStart, WorkingBetween(scheduled))`, the backward pass derives `LateFinish` as the minimum `BackShift` bound over `OutEdges` (the same modality columns and calendar lag, dualized — never an FS-assumed successor-`LateStart` read) and `LateStart` through `WorkCalendar.Recede`, and `TotalFloat = LateStart − EarlyStart`, `FreeFloat` the minimum out-edge slack (successor `EarlyStart` − the edge's `Shift` demand) off `OutEdges`, `IsCritical` on zero total float — `QuikGraph` owns the ORDER and the edge-carried dependency payload, the domain owns the calendar/float arithmetic and the typed `CriticalPath` receipt; this is the SINGLE CPM owner (`api-quikgraph` schedule consumer names `Planning/schedule#CRITICAL_PATH` the pass owner) both the IFC-projected and the `csharp:Rasm.Persistence/Version/ledger # [WIRE]` MPXJ-parsed `SequenceRel` edge sets feed, the topological order making a single pass over each task sufficient and the `WorkCalendar` working-time `Advance`/`Recede` resolving the working-content finish past the non-working days so a three-working-day task spanning a weekend reads its calendar-true finish.
